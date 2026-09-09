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

        public static string Write(KpiCreateRun run, DateTime writtenAt)
        {
            if (run == null) throw new ArgumentNullException("run");

            var report = new StringBuilder();

            Line(report, "RCRC Green KPI checklist");
            Line(report, "Document: " + Shown(run.DocumentTitle));
            Line(report, "Written: " + writtenAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
            Line(report, "Read only. Nothing in the model was changed and the template was not touched.");
            Line(report, string.Empty);

            TheReconciliation(report, run);
            ThePlots(report, run);
            TheCellsWritten(report, run);
            TheCellsNotWritten(report, run);
            TheSpecies(report, run);
            TheChoices(report, run);

            return report.ToString();
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
            Line(report, "  " + Counted("plots with an area",
                held.Read.Count - held.WithoutArea.Count, held.WithoutArea));

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
            TheWorking(report, "AREA, SQUARE METRES", run.Area);
            TheWorking(report, "SHRUBS, SQUARE METRES", run.Shrubs);
            TheWorking(report, "LAWN, SQUARE METRES", run.Lawn);
        }

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
                Line(report, "    shrubs and lawn schedule: " + (reading.ShrubsAndLawnRead
                    ? Count(reading.Subtotals.Count, "group") + " read"
                    : "not read"));

                foreach (GroupSubtotal subtotal in reading.Subtotals)
                {
                    Line(report, "      " + subtotal.Heading + " | " + Number(subtotal.SquareMetres)
                        + " | " + subtotal.ItemCount + " items");
                }

                RegionArea chosen = reading.ChosenRegion;
                Line(report, "    area: " + (chosen == null
                    ? "none chosen"
                    : chosen.TypeName + " | " + Number(chosen.SquareMetres) + " | raw "
                        + chosen.RawSquareFeet.ToString("R", CultureInfo.InvariantCulture)));

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
            if (!run.Outcome.KeptEveryPart)
            {
                Line(report, "  THE OUTPUT DOES NOT HOLD EVERY PART THE TEMPLATE HELD. That is a bug.");
            }

            Line(report, string.Empty);
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

            Heading(report, "SPECIES REVIT HELD THAT THE WORKBOOK'S LIST DOES NOT", missed.Count,
                "named with the count, never dropped, and written nowhere");
            Line(report, "  Revit name | group | merged | from | why");
            foreach (SpeciesMatch match in missed)
            {
                Line(report, "  " + Join(
                    match.Species.BotanicalName,
                    Shown(match.Species.GroupName),
                    match.Species.Quantity.ToString(CultureInfo.InvariantCulture),
                    Working(match.Species),
                    match.Why));
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
            Line(report, "  WHICH REGION EACH PLOT'S AREA CAME OFF");
            Line(report, "  plot | chosen type | offered");
            foreach (PlotReading reading in run.Readings)
            {
                Line(report, "  " + Join(
                    reading.PlotId,
                    reading.ChosenRegionTypeName.Length == 0 ? "(none)" : reading.ChosenRegionTypeName,
                    reading.Regions.Count == 0
                        ? "(none)"
                        : string.Join(", ", reading.Regions
                            .Select(one => one.TypeName + " " + Number(one.SquareMetres)).ToArray())));
            }

            Line(report, string.Empty);
            Line(report, "  Every number above came off a row the schedule printed. Nothing anywhere is");
            Line(report, "  worked out from the elements a schedule lists, because on this model those");
            Line(report, "  elements are RVT Link instances and the plants live inside them.");
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
