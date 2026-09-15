using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One plot the tool offered, which of the two sources named it, and whether it was ticked.
    /// </summary>
    public sealed class PlotOrigin
    {
        public PlotOrigin(string plotId, bool onSheets, bool onSchedules, bool ticked)
        {
            PlotId = plotId ?? string.Empty;
            OnSheets = onSheets;
            OnSchedules = onSchedules;
            Ticked = ticked;
        }

        public string PlotId { get; }

        /// <summary>A sheet carries this plot in PRX_Plot_ID.</summary>
        public bool OnSheets { get; }

        /// <summary>A schedule filters PRX_Ref Plot ID on this plot.</summary>
        public bool OnSchedules { get; }

        public bool Ticked { get; }

        /// <summary>
        /// **Named by neither source.** The list the pane offers is the union of the two, so
        /// this cannot happen for a plot read off the model. It can only appear against a plot
        /// that was TICKED and that the read at the press does not name, which is either a model
        /// edited between the two reads or something putting a plot into the list that no
        /// parameter and no filter ever held. Either way it is a row rather than a silence.
        /// </summary>
        public bool FromNeither
        {
            get { return !OnSheets && !OnSchedules; }
        }

        public bool FromBoth
        {
            get { return OnSheets && OnSchedules; }
        }

        /// <summary>
        /// Where it came from, spelt the way the pane's own two lines spell their halves, so a
        /// row here and a line there read as the same fact.
        /// </summary>
        public string Source
        {
            get
            {
                if (FromBoth) return PlotOrigins.OnBoth;
                if (OnSheets) return PlotOrigins.OnASheetOnly;

                return OnSchedules ? PlotOrigins.OnAScheduleOnly : PlotOrigins.OnNeither;
            }
        }

        public string InWords
        {
            get { return PlotId + " | " + Source + " | " + (Ticked ? "ticked" : "NOT TICKED"); }
        }
    }

    /// <summary>
    /// Every plot the tool offered, where each came from and whether it was ticked.
    ///
    /// **A PLOT THE TOOL OFFERS THAT THE MODEL DOES NOT HOLD IS A PLOT SOMEBODY WILL TICK**, and
    /// until this section nothing in the report named a plot that was NOT ticked. The 05:49 run
    /// listed 165 plots and ticked 156, and MM-09 to MM-15 sat unticked in that list and appeared
    /// nowhere in 57,143 lines, because every section of the file is over the ticked plots.
    ///
    /// **AND IT SAYS WHAT THE TWO DISAGREEMENT LINES ON THE PANE DO NOT.** Those two count the
    /// plots named by exactly ONE source, so a plot named by BOTH is in neither of them. On that
    /// run they named nine plots and nine plots were unticked, which looks like an arithmetic
    /// that closes and is two different nines: being named by one source and being ticked are
    /// different questions and nothing has ever made them agree.
    /// </summary>
    public static class PlotOriginWords
    {
        public const string NothingRead =
            "the plot list was not read on this press, so this section has nothing to say. "
            + "That is a run built without one rather than a model holding no plot.";
    }

    public static class PlotOrigins
    {
        public const string OnBoth = "on a sheet and on a schedule";

        public const string OnASheetOnly = "on a sheet and on no schedule";

        public const string OnAScheduleOnly = "on a schedule and on no sheet";

        /// <summary>
        /// Spelt as a refusal rather than as a state, because the union of the two sources is
        /// the list, so no plot read off the model can land here.
        /// </summary>
        public const string OnNeither = "NAMED BY NEITHER SOURCE";

        /// <summary>
        /// One row per plot, over the plots the model names AND the plots that were ticked, so a
        /// plot in one and not the other is a row rather than a gap. Ordered the way the picker
        /// orders them, so a person reads the report beside the pane.
        /// </summary>
        public static PlotOriginList Of(PlotsInTheModel plots, IEnumerable<string> ticked)
        {
            PlotsInTheModel held = plots ?? PlotsInTheModel.Of(null, null);

            var wasTicked = new HashSet<string>(
                (ticked ?? Enumerable.Empty<string>())
                    .Where(one => !string.IsNullOrWhiteSpace(one))
                    .Select(one => one.Trim()),
                StringComparer.Ordinal);

            var everyPlot = held.All
                .Concat(wasTicked)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(one => one, NaturalOrder.Comparer)
                .ToList();

            var onSheets = new HashSet<string>(
                WithoutOnly(held.All, held.OnSchedulesOnly), StringComparer.Ordinal);
            var onSchedules = new HashSet<string>(
                WithoutOnly(held.All, held.OnSheetsOnly), StringComparer.Ordinal);

            return new PlotOriginList(everyPlot.Select(one => new PlotOrigin(
                one, onSheets.Contains(one), onSchedules.Contains(one), wasTicked.Contains(one))));
        }

        /// <summary>
        /// The plots one source named, worked back out of the union and the other source's own
        /// list. <see cref="PlotsInTheModel"/> keeps the union and the two disagreements rather
        /// than the two source lists, and rebuilding them here beats asking for them again: a
        /// second reading of the same model would be a second record of one fact.
        /// </summary>
        private static IEnumerable<string> WithoutOnly(
            IEnumerable<string> all, IEnumerable<string> theOtherSourceAlone)
        {
            var other = new HashSet<string>(theOtherSourceAlone, StringComparer.Ordinal);

            return all.Where(one => !other.Contains(one));
        }
    }

    /// <summary>
    /// The rows and the counts off them, so the section's heading and its body come off one list.
    /// </summary>
    public sealed class PlotOriginList
    {
        public PlotOriginList(IEnumerable<PlotOrigin> rows)
        {
            Rows = (rows ?? Enumerable.Empty<PlotOrigin>()).ToList();
        }

        public static readonly PlotOriginList NothingRead = new PlotOriginList(null);

        public IReadOnlyList<PlotOrigin> Rows { get; }

        public int Listed
        {
            get { return Rows.Count; }
        }

        public int Ticked
        {
            get { return Rows.Count(one => one.Ticked); }
        }

        public int NotTicked
        {
            get { return Rows.Count(one => !one.Ticked); }
        }

        public int FromBoth
        {
            get { return Rows.Count(one => one.FromBoth); }
        }

        public int FromASheetOnly
        {
            get { return Rows.Count(one => one.OnSheets && !one.OnSchedules); }
        }

        public int FromAScheduleOnly
        {
            get { return Rows.Count(one => one.OnSchedules && !one.OnSheets); }
        }

        public int FromNeither
        {
            get { return Rows.Count(one => one.FromNeither); }
        }

        /// <summary>
        /// **The arithmetic that really closes.** Four sources and two tick states, each adding
        /// to the number of rows. The pane's two lines add to nine on a model that lists 165 and
        /// this one cannot look as though it closes when it does not.
        /// </summary>
        public bool AddsUp
        {
            get
            {
                return FromBoth + FromASheetOnly + FromAScheduleOnly + FromNeither == Listed
                    && Ticked + NotTicked == Listed;
            }
        }

        public IEnumerable<PlotOrigin> NotTickedRows
        {
            get { return Rows.Where(one => !one.Ticked); }
        }

        public string InWords
        {
            get
            {
                if (Listed == 0)
                {
                    return "THE PLOT LIST: no plot was offered and none was ticked, so this run "
                        + "had nothing to pick from.";
                }

                return "THE PLOT LIST: " + Count(Listed, "plot") + " offered, " + Ticked
                    + " ticked and " + NotTicked + " not."
                    + " " + FromBoth + " named by a sheet AND a schedule, "
                    + FromASheetOnly + " by a sheet alone, "
                    + FromAScheduleOnly + " by a schedule alone"
                    + (FromNeither == 0
                        ? "."
                        : ", and " + FromNeither + " BY NEITHER, WHICH IS A BUG IN THIS TOOL.");
            }
        }

        /// <summary>
        /// The sentence nobody had checked. The pane's two lines name the plots one source named
        /// and the other did not, so the plots named by both are in neither line, and the count
        /// they come to answers nothing about how many plots were ticked.
        /// </summary>
        public string WhatTheTwoLinesCount
        {
            get
            {
                return "The pane's two disagreement lines count the "
                    + (FromASheetOnly + FromAScheduleOnly)
                    + " plots named by exactly ONE source. The " + FromBoth
                    + " named by both are in neither line, and being named by one source is a "
                    + "different question from being ticked, so those two counts answer nothing "
                    + "about each other.";
            }
        }

        public bool Counts
        {
            get { return Listed > 0; }
        }

        private static string Count(int howMany, string thing)
        {
            return howMany.ToString(CultureInfo.InvariantCulture)
                + " " + thing + (howMany == 1 ? string.Empty : "s");
        }
    }
}
