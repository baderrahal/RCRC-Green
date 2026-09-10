using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The report one press of Create writes.
    ///
    /// The reconciliation comes first, because a number further down is worth nothing if the
    /// plots that went in are not the plots that came out. Every heading carries its own count,
    /// so a section that found nothing reads differently from one that was never filled in, and
    /// every count is of what happened rather than what was intended.
    /// </summary>
    public static class KpiCreateReport
    {
        public const string Refused = "NOTHING WAS WRITTEN";

        /// <summary>
        /// Printed where a species reached no row at all. It is the one case where a count does
        /// not reach the sheet's total, so it is said in those words rather than left blank.
        /// </summary>
        public const string NowhereAtAll = "NOWHERE, so its count is not in the total";

        public static string Write(KpiCreateRun run, DateTime writtenAt)
        {
            if (run == null) throw new ArgumentNullException("run");

            var report = new StringBuilder();

            Line(report, "RCRC Green KPI checklist");
            Line(report, "Document: " + Shown(run.DocumentTitle));
            Line(report, "Written: " + writtenAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
            Line(report, "Read only. Nothing in the model was changed and the template was not touched.");
            TheClock(report, run);
            Line(report, string.Empty);

            TheReconciliation(report, run);
            ThePlots(report, run);
            TheCellsWritten(report, run);
            TheCellsNotWritten(report, run);
            TheSpecies(report, run);
            TheChoices(report, run);

            return report.ToString();
        }

        /// <summary>
        /// How long it took, at the top, because the scan report has carried elements and
        /// seconds since its first round and the other half of the tool carried nothing.
        ///
        /// **A run over 20 plots took about five minutes and no file recorded it.** The read is
        /// printed apart from the rest because the read is the part that grows with the plots
        /// ticked, and a total on its own cannot say which half to look at.
        /// </summary>
        private static void TheClock(StringBuilder report, KpiCreateRun run)
        {
            if (!run.Timing.WasTimed)
            {
                Line(report, "Run: NOT TIMED. Nothing recorded a duration for this run.");
                return;
            }

            Line(report, "Run: " + Seconds(run.Timing.TotalSeconds) + ", of which reading the model took "
                + Seconds(run.Timing.ReadSeconds) + " and everything after it "
                + Seconds(run.Timing.RestSeconds) + ".");
            Line(report, "Every plot's own read is beside it under EVERY PLOT THAT WENT IN.");
        }

        private static void TheReconciliation(StringBuilder report, KpiCreateRun run)
        {
            Reconciliation held = run.Reconciliation;

            Heading(report, "RECONCILIATION", held.Refusals.Count,
                held.AddsUp
                    ? "everything ticked is accounted for below"
                    : "reasons the workbook was not written");

            foreach (string refusal in held.Refusals) Line(report, "  " + refusal);
            if (held.Refusals.Count > 0) Line(report, string.Empty);

            Line(report, "  plots ticked            " + held.Ticked.Count);
            Line(report, "  plots read              " + held.Read.Count);
            Line(report, "  " + Counted("plots with a softscape schedule",
                held.Read.Count - held.WithoutSoftscape.Count, held.WithoutSoftscape));
            Line(report, "  " + Counted("plots with a shrubs and lawn schedule",
                held.Read.Count - held.WithoutShrubsAndLawn.Count, held.WithoutShrubsAndLawn));
            Line(report, held.AreaWanted
                ? "  " + Counted("plots with an area", held.Read.Count - held.WithoutArea.Count, held.WithoutArea)
                : "  plots with an area   " + AreaNotRead);

            Line(report, "  plots that contributed nothing at all   " + held.ContributedNothing.Count);
            foreach (PlotAndReason nothing in held.ContributedNothing)
            {
                Line(report, "    " + nothing.PlotId + ": " + nothing.Reason);
            }

            if (held.IdenticalAreas.Count > 0)
            {
                Line(report, string.Empty);
                Line(report, "  PLOTS REPORTING AN IDENTICAL AREA, " + held.IdenticalAreas.Count);
                Line(report, "  Either they are the same size or one region is counted twice.");
                foreach (IdenticalArea shared in held.IdenticalAreas)
                {
                    Line(report, "    " + string.Join(", ", shared.Plots.ToArray())
                        + " all read " + shared.Printed
                        + ", raw " + shared.RawSquareFeet.ToString("R", CultureInfo.InvariantCulture));
                }
            }

            Line(report, string.Empty);
            if (held.AreaWanted)
            {
                TheWorking(report, "AREA, SQUARE METRES", run.Area);
            }
            else
            {
                Line(report, "  AREA, SQUARE METRES, " + AreaNotRead);
                Line(report, string.Empty);
            }

            TheWorking(report, "SHRUBS, SQUARE METRES", run.Shrubs);
            TheWorking(report, "LAWN, SQUARE METRES", run.Lawn);
        }

        /// <summary>
        /// Said wherever the area would have printed on a template that takes none, so the
        /// section reads as a read that did not happen and never as a plot with no area. The
        /// words are the pane's own line for the same condition, with why nothing was refused.
        /// </summary>
        public static readonly string AreaNotRead = "not read. " + CreateWords.AreaTypedByHand
            + " No filled region was read for any plot and nothing about the area was refused on.";

        /// <summary>
        /// Each plot's own number and the total underneath it, so the arithmetic can be checked
        /// by eye without opening Revit.
        /// </summary>
        private static void TheWorking(StringBuilder report, string what, Totalled total)
        {
            Line(report, "  " + what + ", " + Count(total.PerPlot.Count, "plot"));
            foreach (PlotNumber one in total.PerPlot)
            {
                Line(report, "    " + one.PlotId + " | " + Number(one.Value));
            }

            Line(report, "    TOTAL | " + Number(total.Total)
                + (total.Adds ? string.Empty : "   THE PARTS ADD TO " + Number(total.Sum)
                    + ", WHICH IS A BUG IN THIS TOOL"));
            Line(report, string.Empty);
        }

        private static void ThePlots(StringBuilder report, KpiCreateRun run)
        {
            Heading(report, "EVERY PLOT THAT WENT IN", run.Readings.Count, "and what each contributed");

            foreach (PlotReading reading in run.Readings)
            {
                Line(report, "  " + reading.PlotId);
                Line(report, "    " + run.ComponentParameter + ": " + Shown(reading.Component));
                Line(report, "    " + run.ReferenceParameter + ": " + Shown(reading.Reference));

                Line(report, "    softscape schedule: " + (reading.SoftscapeRead
                    ? Count(reading.Species.Count, "species row") + " read"
                    : "not read"));

                // The species rows against the TOTAL the schedule printed, so a row the reader
                // dropped is visible here rather than only in a workbook short of trees.
                if (reading.SoftscapeRead)
                {
                    Line(report, "      species rows add to " + reading.SpeciesSum + ", "
                        + (reading.SoftscapeTotalRead
                            ? "the TOTAL row prints " + reading.SoftscapeTotal
                            : "no TOTAL row was found to hold that against"));
                    if (reading.SoftscapeRowsPassedOver > 0)
                    {
                        Line(report, "      " + Count(reading.SoftscapeRowsPassedOver, "row")
                            + " with a count and no botanical name passed over, the subtotals");
                    }
                }

                Line(report, "    shrubs and lawn schedule: " + (reading.ShrubsAndLawnRead
                    ? Count(reading.Subtotals.Count, "group") + " read"
                    : "not read"));

                foreach (string refused in reading.ReadRefusals)
                {
                    Line(report, "    REFUSED: " + refused);
                }

                foreach (GroupSubtotal subtotal in reading.Subtotals)
                {
                    Line(report, "      " + subtotal.Heading + " | " + Number(subtotal.SquareMetres)
                        + " | " + subtotal.ItemCount + " items");
                }

                RegionArea chosen = reading.ChosenRegion;
                Line(report, "    area: " + (!run.Reconciliation.AreaWanted
                    ? "not read, the template takes none"
                    : chosen == null
                        ? "none chosen"
                        : chosen.TypeName + " | " + Number(chosen.SquareMetres) + " | raw "
                            + chosen.RawSquareFeet.ToString("R", CultureInfo.InvariantCulture)));

                Line(report, "    read in " + Seconds(reading.ReadSeconds));

                foreach (string note in reading.Notes) Line(report, "    " + note);
                Line(report, string.Empty);
            }
        }

        private static void TheCellsWritten(StringBuilder report, KpiCreateRun run)
        {
            IReadOnlyList<LandedCell> landed = run.Outcome == null
                ? new List<LandedCell>()
                : run.Outcome.Landed;

            Heading(report, "CELLS WRITTEN", landed.Count, run.Wrote
                ? "every one read back off the output file, never as it was sent"
                : Refused);

            if (!run.Wrote)
            {
                Line(report, "  " + (run.Outcome == null
                    ? "The accounting refused this run, so no file was copied."
                    : run.Outcome.Refusal));
                Line(report, "  " + Count(run.Plan == null ? 0 : run.Plan.Writes.Count, "cell")
                    + " would have been written.");
                Line(report, string.Empty);
                return;
            }

            Line(report, "  sheet | cell | value as it landed");
            foreach (LandedCell cell in landed)
            {
                Line(report, "  " + cell.SheetName + " | " + cell.Cell + " | " + Shown(cell.Value));
            }

            Line(report, string.Empty);
            Line(report, "  parts in the template " + run.Outcome.PartsInSource
                + ", parts in the output " + run.Outcome.PartsInOutput
                + ", " + Count(run.Outcome.ChangedParts.Count, "part") + " changed");

            // One part fewer is expected and the reason is said, so the count does not read as
            // a loss. Anything else short is the resave failure wearing this tool's name.
            if (run.Outcome.PartsDeliberatelyRemoved > 0)
            {
                Line(report, "  " + WorkbookPatcher.CalcChainPart + " was removed on purpose, which is "
                    + "the one part fewer. It is Excel's record of");
                Line(report, "  what order to work the formulas out in, written against the cached results "
                    + "this run dropped.");
                Line(report, "  Excel rebuilds it on the first recalculation.");
            }

            if (!run.Outcome.KeptEveryPart)
            {
                Line(report, "  THE OUTPUT DOES NOT HOLD EVERY PART THE TEMPLATE HELD. That is a bug.");
            }

            TheCache(report, run.Outcome.Cache);
            Line(report, string.Empty);
        }

        /// <summary>
        /// Whether the output will really recalculate, read back off it.
        ///
        /// **Excel showed 0 for seven computed cells while the inputs beside them were right.**
        /// The values were never wrong: the template's own cached results were still there and
        /// Excel trusted them. fullCalcOnLoad was already on, so the flag alone is not the fix,
        /// and this prints all three things that are.
        /// </summary>
        private static void TheCache(StringBuilder report, CacheCheck cache)
        {
            Line(report, string.Empty);
            Line(report, "  WILL EXCEL RECALCULATE THIS FILE: " + (cache.WillRecalculate ? "YES" : "NO"));
            Line(report, "    recalculate on open   " + (cache.RecalculatesOnOpen ? "set" : "NOT SET"));
            Line(report, "    calcId                " + Shown(cache.CalcId)
                + (cache.CalcIdCleared ? string.Empty : "   NOT CLEARED, so Excel may trust the cache"));
            Line(report, "    cached results left   " + cache.FormulaCellsCarryingACachedValue
                + ", dropped " + cache.CachedValuesDropped);
            Line(report, "    " + WorkbookPatcher.CalcChainPart + "     "
                + (cache.CalcChainRemoved ? "removed" : "STILL THERE"));

            if (cache.WillRecalculate) return;

            Line(report, "  THE FILE MAY OPEN SHOWING THE TEMPLATE'S OWN CACHED NUMBERS RATHER THAN THESE.");
            Line(report, "  Press Ctrl Alt F9 in Excel to force it, and treat this as a bug in the tool.");
        }

        private static void TheCellsNotWritten(StringBuilder report, KpiCreateRun run)
        {
            IReadOnlyList<NotWritten> skipped = run.Plan == null
                ? new List<NotWritten>()
                : run.Plan.Skipped;

            Heading(report, "CELLS NOT WRITTEN", skipped.Count, "each with the reason");

            foreach (NotWritten one in skipped)
            {
                Line(report, "  " + Join(
                    Shown(one.SheetName),
                    one.Cell.Length == 0 ? "-" : one.Cell,
                    one.What,
                    one.Why));
            }

            Line(report, string.Empty);
        }

        private static void TheSpecies(StringBuilder report, KpiCreateRun run)
        {
            IReadOnlyList<SpeciesMatch> matches = run.Plan == null
                ? new List<SpeciesMatch>()
                : run.Plan.Matches;

            List<SpeciesMatch> matched = matches.Where(one => one.Matched).ToList();
            List<SpeciesMatch> missed = matches.Where(one => !one.Matched).ToList();

            Heading(report, "SPECIES MATCHED", matched.Count,
                "the workbook row against the merged count, with the plots it came from");
            Line(report, "  sheet | row | workbook name | Revit name | group | merged | from");
            foreach (SpeciesMatch match in matched)
            {
                Line(report, "  " + Join(
                    match.SheetName,
                    KpiTemplates.QuantityColumn + match.Row.ToString(CultureInfo.InvariantCulture),
                    match.WorkbookName,
                    match.Species.BotanicalName,
                    match.Species.GroupName,
                    match.Species.Quantity.ToString(CultureInfo.InvariantCulture),
                    Working(match.Species)));
            }

            Line(report, string.Empty);

            // Where each one landed, not that it was written nowhere. A row here says the count
            // reaches the sheet's total, and an empty one says it did not.
            Heading(report, "SPECIES REVIT HELD THAT THE WORKBOOK'S LIST DOES NOT", missed.Count,
                "named with the count, never dropped, and written into an empty row where there was one");
            Line(report, "  Revit name | group | merged | from | where it landed | why");
            foreach (SpeciesMatch match in missed)
            {
                Line(report, "  " + Join(
                    match.Species.BotanicalName,
                    Shown(match.Species.GroupName),
                    match.Species.Quantity.ToString(CultureInfo.InvariantCulture),
                    Working(match.Species),
                    match.Placed
                        ? match.SheetName + " " + KpiTemplates.BotanicalColumn
                            + match.Row.ToString(CultureInfo.InvariantCulture) + " and "
                            + KpiTemplates.QuantityColumn + match.Row.ToString(CultureInfo.InvariantCulture)
                        : NowhereAtAll,
                    match.Why));
            }

            if (missed.Any(one => one.Placed))
            {
                Line(report, "  A written row carries the botanical name and the count and NOTHING ELSE.");
                Line(report, "  Family, genus, native and every code column are the client's data, so they");
                Line(report, "  stay empty and any KPI that needs one still cannot see this species.");
            }

            Line(report, string.Empty);

            Heading(report, "SPECIES ROWS UNDER NO GROUP", run.Ungrouped.Count,
                "reported and never assumed into a group, because the group decides the sheet");
            foreach (SpeciesRow row in run.Ungrouped)
            {
                Line(report, "  " + Join(row.PlotId, row.BotanicalName,
                    row.Quantity.ToString(CultureInfo.InvariantCulture)));
            }

            Line(report, string.Empty);
        }

        private static void TheChoices(StringBuilder report, KpiCreateRun run)
        {
            Heading(report, "WHERE EVERY VALUE CAME FROM", run.Readings.Count,
                "a number with no source is a number nobody can check");

            Line(report, "  template: " + (run.Template == null ? "(none)" : run.Template.Name)
                + ", read from " + Shown(run.TemplatePath));
            Line(report, "  written to: " + Shown(run.OutputPath));
            Line(report, "  component parameter, chosen by the user: " + Shown(run.ComponentParameter));
            Line(report, "  reference parameter, chosen by the user: " + Shown(run.ReferenceParameter));
            Line(report, "  location: " + Shown(run.Location)
                + ", off Project Information, one read with no plot involved");

            if (run.Component != null && !run.Component.Agrees && run.Component.Distinct.Count > 1)
            {
                Line(report, "  the chosen plots disagreed on the component: "
                    + string.Join(", ", run.Component.Distinct.ToArray()));
            }

            if (run.Reference != null && !run.Reference.Agrees && run.Reference.Distinct.Count > 1)
            {
                Line(report, "  the chosen plots disagreed on the reference: "
                    + string.Join(", ", run.Reference.Distinct.ToArray()));
            }

            Line(report, string.Empty);
            if (run.Reconciliation.AreaWanted)
            {
                TheRegions(report, run);
            }
            else
            {
                Line(report, "  WHICH REGION EACH PLOT'S AREA CAME OFF: " + AreaNotRead);
            }

            Line(report, string.Empty);
            Line(report, "  The shrubs, the lawn and every tree quantity came off a row the schedule");
            Line(report, "  printed. Nothing anywhere is worked out from the elements a schedule lists,");
            Line(report, "  because on this model those elements are RVT Link instances and the plants");
            Line(report, "  live inside them.");

            // The paragraph about the area is about a read that did not happen on a template
            // that takes none, so it is left off rather than printed about nothing.
            if (!run.Reconciliation.AreaWanted) return;

            Line(report, string.Empty);
            Line(report, "  THE AREA IS NOT A SCHEDULE ROW. It is " + KpiNames.InterventionArea
                + " read off the");
            Line(report, "  chosen filled region in the 00 link, raw in square feet, converted here to");
            Line(report, "  square metres. Both are in the row above with the value the model prints");
            Line(report, "  beside them, which is rounded to the metre, so the two can be held against");
            Line(report, "  each other: what was written is that same measurement at full precision and");
            Line(report, "  not a different number.");
        }

        private static void TheRegions(StringBuilder report, KpiCreateRun run)
        {
            Line(report, "  WHICH REGION EACH PLOT'S AREA CAME OFF, AND WHAT IT READ");
            Line(report, "  plot | chosen type | raw square feet | written square metres | "
                + "as the model prints it | offered");
            foreach (PlotReading reading in run.Readings)
            {
                RegionArea chosen = reading.ChosenRegion;

                Line(report, "  " + Join(
                    reading.PlotId,
                    reading.ChosenRegionTypeName.Length == 0 ? "(none)" : reading.ChosenRegionTypeName,
                    chosen == null ? "(none)" : Exactly(chosen.RawSquareFeet),
                    chosen == null ? "(none)" : Exactly(chosen.SquareMetres),
                    chosen == null ? "(none)" : Shown(chosen.Printed),
                    reading.Regions.Count == 0
                        ? "(none)"
                        : string.Join(", ", reading.Regions
                            .Select(one => one.TypeName + " " + Number(one.SquareMetres)).ToArray())));
            }
        }

        /// <summary>
        /// A number with nothing taken off it, so a raw square foot reading can be compared
        /// against what the model prints beside it. <see cref="Number"/> rounds for reading and
        /// would hide the difference this row exists to show.
        /// </summary>
        /// <summary>
        /// A duration for reading, to a tenth. Nothing is decided off it and nothing compares
        /// two of these, so a tenth is as fine as a person needs.
        /// </summary>
        private static string Seconds(double value)
        {
            return value.ToString("0.0", CultureInfo.InvariantCulture) + " seconds";
        }

        private static string Exactly(double value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        private static string Working(MergedSpecies species)
        {
            return string.Join(", ", species.PerPlot
                .Select(one => one.PlotId + " " + ((int)one.Value).ToString(CultureInfo.InvariantCulture))
                .ToArray());
        }

        private static string Counted(string what, int howMany, IReadOnlyList<string> without)
        {
            string said = what + "   " + howMany;
            if (without.Count == 0) return said;

            return said + ", without: " + string.Join(", ", without.ToArray());
        }

        private static void Heading(StringBuilder report, string name, int howMany, string said)
        {
            Line(report, "== " + name + " (" + howMany + ") ==");
            Line(report, said);
        }

        private static string Join(params string[] cells)
        {
            return string.Join(" | ", cells);
        }

        private static string Number(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static string Count(int howMany, string thing)
        {
            return howMany.ToString(CultureInfo.InvariantCulture)
                + " " + thing + (howMany == 1 ? string.Empty : "s");
        }

        private static string Shown(string value)
        {
            return string.IsNullOrEmpty(value) ? "(empty)" : value;
        }

        private static void Line(StringBuilder report, string text)
        {
            report.Append(text).Append("\r\n");
        }
    }
}
