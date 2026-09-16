using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// **THE TEAM'S PLOT LIST, PRESSED ONTO THE TICKS.**
    ///
    /// **It REPLACES every tick and it FORGETS every hand choice**, which is exactly what Select
    /// all and Clear already do and for the same reason: a person pressing it is saying these are
    /// the plots, and a per plot choice left standing behind it is a record that disagrees with
    /// what is on screen. The next template row press would then act on the disagreement.
    ///
    /// **A LISTED PLOT THE MODEL DOES NOT NAME CANNOT BE TICKED AND IS NAMED.** Nothing here
    /// invents a plot, which is the rule in `CLAUDE.md`, and a plot that quietly fell off a list
    /// of 154 is the whole thing this exists to stop.
    /// </summary>
    public static class TickingTheList
    {
        /// <summary>
        /// The ticks a press of Tick the list leaves: exactly the listed plots the model names,
        /// and nothing else.
        /// </summary>
        public static PlotTicks Ticked(PlotTicks ticks, PlotListRead list)
        {
            if (ticks == null) throw new ArgumentNullException("ticks");
            if (list == null || !list.Read) return ticks.None();

            return new PlotTicks(ticks.Plots, list.Plots.Where(ticks.Plots.Holds));
        }

        /// <summary>Every listed plot the model does not name, in the file's order.</summary>
        public static IReadOnlyList<string> NotInTheModel(PlotsInTheModel plots, PlotListRead list)
        {
            if (list == null || !list.Read) return new List<string>();
            if (plots == null) return list.Plots.ToList();

            return list.Plots.Where(one => !plots.Holds(one)).ToList();
        }

        /// <summary>
        /// Every plot the model names that the list does not, in the model's own order.
        ///
        /// **A LIST SHORTER THAN THE MODEL IS NOT A FAULT AND IS STILL A FACT.** The team's 154
        /// is their scope and the model holds more, so these are named rather than counted, and
        /// whether any of them should have been on the list is for the team.
        /// </summary>
        public static IReadOnlyList<string> NotOnTheList(PlotsInTheModel plots, PlotListRead list)
        {
            if (plots == null) return new List<string>();
            if (list == null || !list.Read) return plots.All.ToList();

            var listed = new HashSet<string>(list.Plots, StringComparer.Ordinal);
            return plots.All.Where(one => !listed.Contains(one)).ToList();
        }

        /// <summary>
        /// Every TICKED plot the list does not name, in the order the ticks came.
        ///
        /// **THE 16:06 PRESS TICKED 166 PLOTS AGAINST A LIST OF 154.** The twelve extra plots
        /// were read, written and filed, three of that press's five shared value collisions came
        /// from them, and neither the pane nor THE PLOT LIST said a word about any of it. This is
        /// the other direction from <see cref="NotOnTheList"/>, which is about the MODEL: a plot
        /// the model holds and the list does not is ordinary, and a plot somebody TICKED that the
        /// list does not hold is work nobody asked for.
        /// </summary>
        public static IReadOnlyList<string> TickedAndNotOnTheList(
            IEnumerable<string> ticked, PlotListRead list)
        {
            if (list == null || !list.Read) return new List<string>();

            var listed = new HashSet<string>(list.Plots, StringComparer.Ordinal);

            return (ticked ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrWhiteSpace(one))
                .Where(one => !listed.Contains(one))
                .ToList();
        }

        public static string TickedAndNotOnTheListInWords(string plotId)
        {
            return plotId + " is ticked and is not on the list, so this press will write a "
                + "workbook and a PDF for a plot the team did not ask for.";
        }

        public const string Heading = "The plot list:";

        /// <summary>
        /// **WHAT THE PANE SAYS BEFORE THE PRESS, and nothing on the list drops out without a
        /// line.** Four things: a listed plot the model does not name, a plot listed twice, a
        /// line that is not a plot, and a listed plot the press would put into no workbook or no
        /// PDF.
        ///
        /// <paramref name="componentOf"/> is the plot's own PRX_Component, which is what
        /// <see cref="PlotsPerTemplate.For"/> asks, so the workbook half is decided by the split's
        /// own rule rather than by a second one written here.
        ///
        /// **A list with nothing wrong with it says nothing at all**, because a line about
        /// nothing is one the team reads past on every other press.
        /// </summary>
        public static IReadOnlyList<string> Lines(
            PlotListRead list, PlotsInTheModel plots, Func<string, string> componentOf,
            IEnumerable<string> ticked = null)
        {
            var said = new List<string>();
            if (list == null || !list.Set) return said;

            if (!list.Read)
            {
                said.Add(Heading + " " + list.Why);
                return said;
            }

            said.Add(Heading + " " + list.InWords);

            foreach (PlotListFault one in list.NotPlots) said.Add("  " + one.InWords);
            foreach (PlotListFault one in list.Repeated) said.Add("  " + one.InWords);

            foreach (string one in NotInTheModel(plots, list))
            {
                said.Add("  " + one + " is on the list and the model does not name it, so it "
                    + "cannot be ticked and nothing will be written for it.");
            }

            foreach (string one in list.Plots)
            {
                if (plots != null && !plots.Holds(one)) continue;

                string why = WouldWriteNothing(one, componentOf);
                if (why.Length > 0) said.Add("  " + why);
            }

            // **A TICKED PLOT THE LIST DOES NOT NAME IS NAMED HERE TOO.** The lines above are
            // every way a listed plot can fall out, and the 16:06 press showed the other
            // direction costs just as much: twelve plots written that nobody asked for.
            foreach (string one in TickedAndNotOnTheList(ticked, list))
            {
                said.Add("  " + TickedAndNotOnTheListInWords(one));
            }

            // Nothing wrong with the list at all is the one line worth keeping, because it says
            // the count the press will really use.
            return said;
        }

        /// <summary>
        /// Why a listed plot the model names would still come out with no workbook or no PDF,
        /// **asked of the two rules that really decide it** rather than of anything restated here.
        /// </summary>
        public static string WouldWriteNothing(string plotId, Func<string, string> componentOf)
        {
            PlotTemplate placed = PlotsPerTemplate.For(
                plotId, componentOf == null ? string.Empty : componentOf(plotId));

            PdfForm form = PdfForms.ForPlot(plotId);

            if (placed.Template == null && form == null)
            {
                return plotId + " would get NO workbook and NO PDF. " + placed.Why + ". "
                    + PdfForms.NoFormFor(plotId);
            }

            if (placed.Template == null)
            {
                return plotId + " would get NO workbook, so no PDF is written beside it either. "
                    + placed.Why;
            }

            if (form == null)
            {
                return plotId + " would get a workbook and NO PDF. " + PdfForms.NoFormFor(plotId);
            }

            return string.Empty;
        }
    }
}
