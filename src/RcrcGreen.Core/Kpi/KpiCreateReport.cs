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

        public const string SchedulesHeading = "EVERY SCHEDULE THIS RUN READ, AS THE SCHEDULE PRINTS IT";

        public const string FormulasHeading = "WHAT THE WORKBOOK WILL COMPUTE FROM THIS";

        /// <summary>
        /// The most rows one schedule prints in the report, the same cap the scan report uses.
        /// </summary>
        public const int ShownRows = 200;

        public static string Write(KpiCreateRun run, DateTime writtenAt)
        {
            if (run == null) throw new ArgumentNullException("run");

            var report = new StringBuilder();

            Line(report, "RCRC Green KPI checklist");
            Line(report, "Document: " + Shown(run.DocumentTitle));
            Line(report, "Written: " + writtenAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
            Line(report, "Read only. Nothing in the model was changed and the template was not touched.");
            TheClock(report, run);
            Line(report, "Every schedule this run read is printed as the schedule prints it, at the end of this");
            Line(report, "file under " + SchedulesHeading + ", so every number above it can be held against the drawing.");
            Line(report, string.Empty);

            TheReconciliation(report, run);
            ThePlots(report, run);
            TheCellsWritten(report, run);
            TheCellsNotWritten(report, run);
            TheFormulas(report, run);
            TheSpecies(report, run);
            TheTreeLists(report, run);
            TheChoices(report, run);
            TheSchedules(report, run);

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
            Line(report, "  " + Counted("plots with one softscape schedule",
                held.Read.Count - held.WithoutSoftscape.Count - held.WithMoreThanOneSoftscape.Count,
                held.WithoutSoftscape, held.WithMoreThanOneSoftscape));
            Line(report, "  " + Counted("plots with one shrubs and lawn schedule",
                held.Read.Count - held.WithoutShrubsAndLawn.Count - held.WithMoreThanOneShrubsAndLawn.Count,
                held.WithoutShrubsAndLawn, held.WithMoreThanOneShrubsAndLawn));
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

                // Which schedule each number came off, by name. FM-05 holds two whose names
                // hold SOFTSCAPE and a line saying only that species rows were read could not
                // show which, or that both had been.
                Line(report, "    " + Schedules("softscape", reading.SoftscapeSchedules,
                    Count(reading.Species.Count, "species row") + " read"));

                // The species rows against the TOTAL the schedule printed, so a row the reader
                // dropped is visible here rather than only in a workbook short of trees.
                if (reading.SoftscapeRead)
                {
                    Line(report, "      species rows add to " + reading.SpeciesSum + ", "
                        + (reading.SoftscapeTotalRead
                            ? "the TOTAL row prints " + reading.SoftscapeTotal
                                + (reading.SoftscapeTotalRow > 0 ? " at row " + reading.SoftscapeTotalRow : string.Empty)
                            : "no TOTAL row was found to hold that against"));
                    if (reading.SoftscapeRowsPassedOver > 0)
                    {
                        Line(report, "      " + Count(reading.SoftscapeRowsPassedOver, "row")
                            + " with a count and no botanical name passed over, the subtotals");
                    }

                    // A species on two rows under one group is what FM-05 10, FM-05 10 was, and
                    // it is said here beside the plot rather than left to be read off a merged row.
                    foreach (RepeatedSpecies repeated in reading.SpeciesPrintedOnMoreThanOneRow)
                    {
                        Line(report, "      " + repeated.BotanicalName + " under " + repeated.GroupName
                            + " is printed on " + repeated.Rows.Count + " rows, " + repeated.InWords);
                    }
                }

                Line(report, "    " + Schedules("shrubs and lawn", reading.ShrubsAndLawnSchedules,
                    Count(reading.Subtotals.Count, "group") + " read"));

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
            Line(report, "  WILL EXCEL RECALCULATE THIS FILE: " + (cache.WillRecalculate ? "YES" : "NO")
                + ", five things checked off the output file, over what the file says and not over what Excel does with it");
            Line(report, "    recalculate on open   " + (cache.RecalculatesOnOpen ? "set" : "NOT SET"));
            Line(report, "    calcId                " + Shown(cache.CalcId)
                + (cache.CalcIdCleared ? string.Empty : "   NOT CLEARED, so Excel may trust the cache"));
            Line(report, "    cached results left   " + cache.FormulaCellsCarryingACachedValue
                + ", dropped " + cache.CachedValuesDropped);
            Line(report, "    " + WorkbookPatcher.CalcChainPart + "     "
                + (cache.CalcChainRemoved ? "removed" : "STILL THERE"));

            // The 1428 file passed the four above and opened into a manual session showing every
            // formula cell blank, because calcPr carried no calcMode and Excel takes the mode
            // from the first workbook it opens.
            Line(report, "    calcMode              " + (cache.CalcMode.Length == 0 ? "NOT SET" : cache.CalcMode)
                + (cache.CalcModeAuto ? string.Empty : "   NOT AUTO, so a manual Excel session opens it without calculating"));
            Line(report, "    other calculation settings in the package: "
                + (cache.OtherCalculationSettings.Count == 0 ? "none found" : cache.OtherCalculationSettings.Count.ToString(CultureInfo.InvariantCulture)));
            foreach (string setting in cache.OtherCalculationSettings) Line(report, "      " + setting);
            Line(report, "      looked for: " + CacheCheck.LookedFor);

            if (cache.WillRecalculate) return;

            Line(report, "  THE FILE MAY OPEN SHOWING THE TEMPLATE'S OWN CACHED NUMBERS RATHER THAN THESE.");
            Line(report, "  Press Ctrl Alt F9 in Excel to force it, and treat this as a bug in the tool.");
        }

        /// <summary>
        /// What the output's formulas will make of the cells that landed, read off the output's
        /// own text. This is the section that would have caught the canopy fault without anyone
        /// opening Excel: a written row's neighbouring formula returned a space and the cell
        /// beside it multiplied that space by the count.
        /// </summary>
        private static void TheFormulas(StringBuilder report, KpiCreateRun run)
        {
            FormulaCheck check = run.Outcome == null ? FormulaCheck.NotChecked : run.Outcome.Formulas;

            Heading(report, FormulasHeading, check.AtRisk.Count(one => one.IsAnError),
                "formulas that would read an error, checked off the output file over what its formulas read and never by evaluating one");

            if (!check.WasChecked)
            {
                Line(report, "  not checked. " + (run.Outcome == null
                    ? "The accounting refused this run, so no file was written to check."
                    : "The patch was refused before a file was written."));
                Line(report, string.Empty);
                return;
            }

            Line(report, "  formulas in the output   " + check.FormulaCount
                + ", of which " + check.ReadingWrittenRows.Count + " read a cell on a row this run wrote into");
            if (check.RefusesTheWrite)
            {
                Line(report, "  THIS RUN IS REFUSED ON WHAT FOLLOWS. " + check.Refusal);
            }

            Line(report, string.Empty);
            Line(report, "  FORMULAS AT RISK, " + check.AtRisk.Count
                + ": a formula returning text where a number was expected, the #VALUE! that arithmetic on it gives, and every formula that reads one");
            Line(report, "  sheet | cell | formula | why");
            foreach (FormulaAtRisk one in check.AtRisk)
            {
                Line(report, "  " + Join(one.SheetName, one.Cell, one.Text,
                    one.Reason + (one.FromWrittenRow ? string.Empty : " (not from a row this run wrote into)")));
            }

            Line(report, string.Empty);
            Line(report, "  THE CELLS THE WORKBOOK COMPUTES FROM, " + check.ComputesFrom.Count
                + ", every cell the map names on the main sheet and whether the formulas reading it have their inputs");
            foreach (ComputedFrom one in check.ComputesFrom)
            {
                Line(report, "  " + Join(
                    one.Input.ToString(),
                    one.Present ? "present, holds " + Shown(one.Holds) : "NOT PRESENT, nothing was written there",
                    "read by " + Count(one.Readers.Count, "formula")));
                foreach (string reader in one.Readers) Line(report, "      " + reader);
                foreach (string blank in one.BlankInputs) Line(report, "      " + blank);
            }

            Line(report, string.Empty);
            Line(report, "  FORMULAS READING A ROW THIS RUN WROTE INTO, " + check.ReadingWrittenRows.Count);
            Line(report, "  sheet | cell | reads | formula");
            foreach (FormulaCell one in check.ReadingWrittenRows)
            {
                Line(report, "  " + Join(one.SheetName, one.Cell, string.Join(", ", one.ReadsWritten.ToArray()),
                    one.Text + (one.SharedWith.Length == 0 ? string.Empty : " (shared with " + one.SharedWith + ")")));
            }

            Line(report, string.Empty);
            Line(report, "  FUNCTIONS THE READER'S EXCEL MAY NOT HAVE, " + check.FunctionsExcelMayNotHave.Count
                + ". The file stores a function with the _xlfn. prefix when an older Excel does not have it,");
            Line(report, "  and such a cell reads #NAME? there. The tool writes no formula and did not put these here.");
            foreach (FunctionUse one in check.FunctionsExcelMayNotHave)
            {
                Line(report, "    _xlfn." + one.Name + " in " + Count(one.Cells, "cell")
                    + ". Those cells need a version of Excel that has " + one.Name + ".");
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
            List<SpeciesMatch> unreached = matches.Where(one => one.NotReachedByTheTotal).ToList();
            List<SpeciesMatch> missed = matches.Where(one => !one.Matched && !one.NotReachedByTheTotal).ToList();

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

            // Two numbers for one species, the client's and the model's. Named, and the client's
            // row left as it is, because which is right is a question for the team.
            IReadOnlyList<MeasureDifference> differences = run.Plan == null
                ? new List<MeasureDifference>()
                : run.Plan.Differences;
            Heading(report, "MATCHED SPECIES WHOSE HEIGHT OR DIAMETER IN REVIT DIFFERS FROM THE ROW'S", differences.Count,
                "named and CHANGED NOTHING, the client's row keeps its own number");
            Line(report, "  sheet | row | workbook name | what | Revit prints | the row holds");
            foreach (MeasureDifference one in differences)
            {
                Line(report, "  " + Join(one.SheetName, one.Row.ToString(CultureInfo.InvariantCulture),
                    one.WorkbookName, one.What, one.RevitPrints, one.WorkbookHolds, "CHANGED NOTHING"));
            }

            Line(report, string.Empty);

            // The list holds the name and its total does not reach the row. The count is not
            // written there, because a count on the sheet that no total adds reads as complete
            // and is short, and this is where a person sees which rows the client's total is
            // short of.
            Heading(report, "SPECIES THE LIST HOLDS ON A ROW ITS TOTAL DOES NOT REACH", unreached.Count,
                "the count was NOT written, so it is not in the total, and the row is named");
            Line(report, "  sheet | row | workbook name | Revit name | group | merged | from | why");
            foreach (SpeciesMatch match in unreached)
            {
                Line(report, "  " + Join(
                    match.SheetName,
                    match.Row.ToString(CultureInfo.InvariantCulture),
                    Shown(match.WorkbookName),
                    match.Species.BotanicalName,
                    match.Species.GroupName,
                    match.Species.Quantity.ToString(CultureInfo.InvariantCulture),
                    Working(match.Species),
                    match.Why));
            }

            Line(report, string.Empty);

            // Where each one landed, not that it was written nowhere. A row here says the count
            // reaches the sheet's total, and an empty one says it did not.
            Heading(report, "SPECIES REVIT HELD THAT THE WORKBOOK'S LIST DOES NOT", missed.Count,
                "named with the count, never dropped, and written into an empty row where there was one");
            Line(report, "  Revit name | group | merged | from | where it landed | height | diameter | why");
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
                    match.Placed ? Measured(match.Species.Height, match.HeightColumn, match.WhyNoHeightColumn, match.Row) : "-",
                    match.Placed ? Measured(match.Species.Diameter, match.DiameterColumn, match.WhyNoDiameterColumn, match.Row) : "-",
                    match.Why));
            }

            if (missed.Any(one => one.Placed))
            {
                Line(report, "  A written row carries the botanical name, the count, and the height and the canopy");
                Line(report, "  diameter the schedule printed beside it, into the columns the sheet's header row names,");
                Line(report, "  and NOTHING ELSE. Family, genus, native and every code column are the client's data,");
                Line(report, "  so they stay empty and any KPI that needs one still cannot see this species.");
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

        /// <summary>
        /// The two tree lists as they were read off the template, so what the tool matched
        /// against can be checked against the file by eye.
        ///
        /// **Three answers to where a list ends were measured on one file**: the map said row
        /// 83, the total said SUM(B4:B92), and the names ran to row 101. The map's answer is
        /// gone, and the other two are printed here side by side so the next disagreement is
        /// read off the report rather than found in a workbook 85 trees short.
        /// </summary>
        private static void TheTreeLists(StringBuilder report, KpiCreateRun run)
        {
            Heading(report, "THE WORKBOOK'S OWN TREE LISTS", 2,
                "read off the template when Create was pressed, never off a row range in this tool");

            TheTreeList(report, run.Template == null ? KpiTemplates.ExistingTreesSheet
                : run.Template.ExistingTrees.SheetName, run.ExistingList);
            TheTreeList(report, run.Template == null ? KpiTemplates.ProposedTreesSheet
                : run.Template.ProposedTrees.SheetName, run.ProposedList);

            Line(report, string.Empty);
        }

        public const string ListsNotRead = "not read. The accounting refused before the template was opened.";

        private static void TheTreeList(StringBuilder report, string sheetName, SpeciesList list)
        {
            Line(report, "  " + sheetName);

            if (list == null)
            {
                Line(report, "    " + ListsNotRead);
                return;
            }

            if (!list.WasRead)
            {
                Line(report, "    NOT READ: " + list.Refusal);
                return;
            }

            Line(report, "    botanical names in column " + KpiTemplates.BotanicalColumn + ": "
                + (list.Rows.Count == 0
                    ? "none, the row under the header is empty"
                    : Count(list.Rows.Count, "name") + ", rows " + list.Rows[0].Row + " to "
                        + list.Rows[list.Rows.Count - 1].Row));
            Line(report, "    the quantity total: " + list.TotalInWords
                + (list.TotalFound ? ", reaching rows " + list.TotalFirstRow + " to " + list.TotalLastRow : string.Empty));
            Line(report, "    empty rows the total reaches, for a species the list does not hold: "
                + list.EmptyRows.Count
                + (list.EmptyRows.Count == 0 ? string.Empty : ", rows " + Rows(list.EmptyRows)));

            if (list.OutsideTheTotal.Count > 0)
            {
                Line(report, "    NAMES THE TOTAL DOES NOT REACH: " + list.OutsideTheTotal.Count + ", rows "
                    + Rows(list.OutsideTheTotal.Select(one => one.Row).ToList())
                    + ". A count written there would never reach the total, so a species matching");
                Line(report, "    one of these is not written and is named under SPECIES THE LIST HOLDS ON A ROW ITS TOTAL DOES NOT REACH.");
            }

            if (list.BelowTheList.Count > 0)
            {
                Line(report, "    names below the first empty row of the list, at row " + list.FirstGapRow + ": "
                    + list.BelowTheList.Count + ", rows " + Rows(list.BelowTheList.Select(one => one.Row).ToList())
                    + ". These are not the list and were not matched against.");
            }
        }

        /// <summary>
        /// A run of rows as 93 to 101, or the rows one by one when they do not run.
        /// </summary>
        private static string Rows(IReadOnlyList<int> rows)
        {
            if (rows.Count == 0) return string.Empty;
            if (rows.Count == 1) return rows[0].ToString(CultureInfo.InvariantCulture);

            bool contiguous = true;
            for (int at = 1; at < rows.Count; at++)
            {
                if (rows[at] != rows[at - 1] + 1) contiguous = false;
            }

            return contiguous
                ? rows[0] + " to " + rows[rows.Count - 1]
                : string.Join(", ", rows.Select(one => one.ToString(CultureInfo.InvariantCulture)).ToArray());
        }

        /// <summary>
        /// One kind of schedule on one plot: the name and what was read off it, or none, or
        /// every name found when there was more than one and none was read.
        /// </summary>
        private static string Schedules(string kind, IReadOnlyList<string> names, string read)
        {
            if (names.Count == 0) return kind + " schedule: none filters on this plot, so nothing was read";
            if (names.Count == 1) return kind + " schedule: " + names[0] + ", " + read;

            return kind + " schedules: " + names.Count + " FOUND AND NONE READ, "
                + string.Join(" | ", names.ToArray());
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
            return species.Working;
        }

        /// <summary>
        /// 15 into I84, or why nothing went there, with every value found so the reader can see
        /// what the rows printed.
        /// </summary>
        private static string Measured(MeasureAnswer answer, string column, string whyNoColumn, int row)
        {
            if (string.IsNullOrEmpty(column)) return "not written: " + whyNoColumn;
            if (!answer.Write) return "not written: " + answer.Why;

            return Number(answer.Value) + " into " + column + row.ToString(CultureInfo.InvariantCulture)
                + (answer.Why.Length == 0 ? string.Empty : " (" + answer.Why + ")");
        }

        /// <summary>
        /// Every schedule this run read, as the schedule prints it, last in the file because it
        /// is the longest and named at the top because it is what makes the rest checkable.
        /// Nothing above it was what the tool read, and a plot counted on two rows survived two
        /// rounds because nothing showed the rows.
        /// </summary>
        private static void TheSchedules(StringBuilder report, KpiCreateRun run)
        {
            int howMany = run.Readings.Sum(one => one.PrintedSchedules.Count);
            Heading(report, SchedulesHeading, howMany,
                "one block per schedule, every column, row numbers counting the heading row as 1, so it can be held against the drawing");

            foreach (PlotReading reading in run.Readings)
            {
                foreach (ScannedSchedule schedule in reading.PrintedSchedules)
                {
                    Line(report, string.Empty);
                    Line(report, "  " + reading.PlotId + ", " + schedule.Name);
                    Line(report, "    " + WhatWasRead(reading, schedule));

                    int shown = Math.Min(ShownRows, schedule.Rows.Count);
                    Line(report, "    rows printed " + schedule.Rows.Count + ", shown " + shown
                        + (shown < schedule.Rows.Count ? " of " + schedule.Rows.Count + ", the rest are cut off here" : string.Empty)
                        + (schedule.BodyRowCount != schedule.Rows.Count ? ", the model holds " + schedule.BodyRowCount : string.Empty));

                    foreach (string line in Aligned(schedule.Rows.Take(shown).ToList())) Line(report, "    " + line);
                }
            }

            Line(report, string.Empty);
        }

        /// <summary>
        /// Which rows were read as what, per kind: the species rows and the TOTAL row of a
        /// softscape schedule, and for a shrubs and lawn schedule WHICH SUBTOTAL ROW WAS TAKEN
        /// for each group and why the others were not.
        /// </summary>
        private static string WhatWasRead(PlotReading reading, ScannedSchedule schedule)
        {
            if (schedule.IsSoftscape)
            {
                List<int> rows = reading.Species.Where(one => one.RowNumber > 0).Select(one => one.RowNumber).OrderBy(one => one).ToList();
                return "read as species rows: " + rows.Count
                    + (rows.Count == 0 ? string.Empty : ", rows " + Rows(rows))
                    + ", passed over as subtotals: " + reading.SoftscapeRowsPassedOver
                    + ", TOTAL row: " + (reading.SoftscapeTotalRead ? "row " + reading.SoftscapeTotalRow : "none found");
            }

            if (reading.Subtotals.Count == 0) return "read as group values: none";

            return "read as group values: " + string.Join("; ", reading.Subtotals.Select(one =>
                one.Heading + " taken off row " + one.RowNumber + ", the last of its "
                + Count(one.RowsConsidered.Count, "subtotal row") + " (" + Rows(one.RowsConsidered) + ")"
                + (one.RowsConsidered.Count > 1
                    ? ", the rows above it are the phase subtotals that add to it and are not taken"
                    : ", nothing above it to check against")).ToArray());
        }

        /// <summary>
        /// The rows with every column padded to the widest cell in it, so a column can be read
        /// down. A cell longer than the cap is cut and marked.
        /// </summary>
        private static IEnumerable<string> Aligned(IReadOnlyList<IReadOnlyList<string>> rows)
        {
            const int Cap = 40;
            int columns = rows.Count == 0 ? 0 : rows.Max(row => row.Count);
            var widths = new int[columns];
            for (int column = 0; column < columns; column++)
            {
                widths[column] = rows.Max(row => Cell(row, column, Cap).Length);
            }

            int rowWidth = rows.Count.ToString(CultureInfo.InvariantCulture).Length;
            for (int index = 0; index < rows.Count; index++)
            {
                var cells = new List<string>();
                for (int column = 0; column < columns; column++)
                {
                    cells.Add(Cell(rows[index], column, Cap).PadRight(widths[column]));
                }

                yield return (index + 1).ToString(CultureInfo.InvariantCulture).PadLeft(rowWidth) + " | " + string.Join(" | ", cells.ToArray());
            }
        }

        private static string Cell(IReadOnlyList<string> row, int column, int cap)
        {
            string text = column < row.Count && !string.IsNullOrEmpty(row[column]) ? row[column] : "-";
            return text.Length <= cap ? text : text.Substring(0, cap - 1) + "~";
        }

        private static string Counted(
            string what, int howMany, IReadOnlyList<string> without, IReadOnlyList<string> withMoreThanOne = null)
        {
            string said = what + "   " + howMany;
            if (without.Count > 0) said += ", without: " + string.Join(", ", without.ToArray());
            if (withMoreThanOne != null && withMoreThanOne.Count > 0)
            {
                said += ", with more than one: " + string.Join(", ", withMoreThanOne.ToArray());
            }

            return said;
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
