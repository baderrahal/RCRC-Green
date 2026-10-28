using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **THE PANE WENT TO THE WHOLE TEAM AND READ LIKE A LOG.** Bader's design of October cut
    /// every line it shows to ten words a sentence and moved every explanation to a tooltip or
    /// to the report's BEFORE THE PRESS. These hold the new words, the rules that pick them and
    /// the one section the report gained.
    ///
    /// **EVERY EXPECTED VALUE HERE IS WRITTEN OUT BY HAND.**
    /// </summary>
    public class KpiPaneRedesignTests
    {
        // ------------------------------------------------------------ how long a sentence is

        /// <summary>
        /// The words of each sentence. A sentence ends at a full stop, a question mark or an
        /// exclamation mark followed by a space or the end, so 0.8 s and DM-11 stay whole.
        /// </summary>
        private static IReadOnlyList<int> SentenceLengths(string text)
        {
            var lengths = new List<int>();
            string rest = (text ?? string.Empty).Trim();

            while (rest.Length > 0)
            {
                int end = -1;
                for (int at = 0; at < rest.Length; at++)
                {
                    char c = rest[at];
                    if ((c == '.' || c == '?' || c == '!') && (at == rest.Length - 1 || rest[at + 1] == ' '))
                    {
                        end = at;
                        break;
                    }
                }

                string sentence = end < 0 ? rest : rest.Substring(0, end);
                rest = end < 0 ? string.Empty : rest.Substring(end + 1).Trim();

                int words = sentence.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
                if (words > 0) lengths.Add(words);
            }

            return lengths;
        }

        [Fact]
        public void TheSentenceCounterCountsWhatAPersonReads()
        {
            Assert.Equal(new[] { 4, 5 }, SentenceLengths("Workbook open in Excel. Close it and try again."));
            Assert.Equal(new[] { 2 }, SentenceLengths("0.8 s"));
            Assert.Equal(new[] { 3 }, SentenceLengths("Read at 08:37"));
        }

        private static void Short(string text, int most)
        {
            foreach (int words in SentenceLengths(text))
            {
                Assert.True(words <= most, "'" + text + "' has a sentence of " + words + " words, more than " + most + ".");
            }
        }

        /// <summary>
        /// Every constant and every formatted line the pane shows, with sample values, large
        /// counts included, at most ten words a sentence.
        /// </summary>
        [Fact]
        public void EveryLineThePaneShowsIsTenWordsASentenceAtMost()
        {
            var lines = new List<string>
            {
                KpiPaneWords.Title, KpiPaneWords.Build("1.0.0.0"), KpiPaneWords.Build(null),
                KpiPaneWords.ModelNamed(null),
                KpiPaneWords.Folders, KpiPaneWords.Files, KpiPaneWords.WorkbookDetails,
                KpiPaneWords.Templates, KpiPaneWords.Output, KpiPaneWords.PdfForms,
                KpiPaneWords.StreetReference, KpiPaneWords.PlotList, KpiPaneWords.Optional,
                KpiPaneWords.Browse, KpiPaneWords.Date, KpiPaneWords.PreparedBy, KpiPaneWords.Position,
                KpiPaneWords.NotSet, KpiPaneWords.NotFound, KpiPaneWords.Unreadable,
                KpiPaneWords.TemplatesFound(1), KpiPaneWords.TemplatesFound(1234567),
                KpiPaneWords.RowsHeld(8353), KpiPaneWords.PlotsListed(154),
                KpiPaneWords.NextRead, KpiPaneWords.ModelRead, KpiPaneWords.ReadModel, KpiPaneWords.ReadAgain,
                KpiPaneWords.NotReadYet, KpiPaneWords.ReadsSheetsAndSchedules, KpiPaneWords.NoModelOpen, KpiPaneWords.ReadingTheModel,
                KpiPaneWords.PlotsBox, KpiPaneWords.ElementsBox, KpiPaneWords.TimeBox,
                KpiPaneWords.InPlotList, KpiPaneWords.TemplatesMatched, KpiPaneWords.ValuesFrom,
                KpiPaneWords.Component, KpiPaneWords.Reference, KpiPaneWords.Location,
                KpiPaneWords.NothingHoldsThatWord, KpiPaneWords.NextPlots,
                KpiPaneWords.Seconds(46.2), KpiPaneWords.Number(1234567), KpiPaneWords.OfCount(154, 163),
                KpiPaneWords.ReadAtClock(new DateTime(2026, 10, 28, 8, 37, 12)),
                KpiPaneWords.ShowInReport, KpiPaneWords.InTheReportAfterCreate,
                KpiPaneWords.AssetTypes, KpiPaneWords.PlotsSection, KpiPaneWords.All, KpiPaneWords.None,
                KpiPaneWords.PlotListButton, KpiPaneWords.SearchTooltip, KpiPaneWords.NoType, KpiPaneWords.SharedId,
                KpiPaneWords.TickListAgain, KpiPaneWords.NoTemplatesInTheFolder, KpiPaneWords.NextCreate,
                KpiPaneWords.PlotsSelected(1), KpiPaneWords.PlotsSelected(99999),
                KpiPaneWords.ReadyToCreate, KpiPaneWords.AssetTypeColumn, KpiPaneWords.PlotsColumn,
                KpiPaneWords.RangeColumn, KpiPaneWords.Replaced, KpiPaneWords.NothingInTheModelChanges,
                KpiPaneWords.WorkbooksBeside(1, true), KpiPaneWords.WorkbooksBeside(154, false),
                KpiPaneWords.CreateWorkbooks(1), KpiPaneWords.CreateWorkbooks(123456),
                KpiPaneWords.Failed, KpiPaneWords.Created, KpiPaneWords.AllCreated, KpiPaneWords.ReadyToSend,
                KpiPaneWords.OpenOutputFolder, KpiPaneWords.OpenReport, KpiPaneWords.RetryFailed,
                KpiPaneWords.NewRun, KpiPaneWords.WriteAnyway,
                KpiPaneWords.NotCreated(1), KpiPaneWords.NotCreated(100000),
                KpiPaneWords.ResultStatus(123456, 654321),
                KpiSteps.SetupStatus(new List<string>()), KpiSteps.SetupStatus(new List<string> { "a", "b", "c", "d", "e" }),
                KpiSteps.ReadFirst, KpiPaneWords.PdfsNotWritten(1), KpiPaneWords.PdfsNotWritten(123456),
                ProgressWords.Reading(123456, 654321), ProgressWords.ReadingTheModel, AssetTypeResult.NothingToWriteWords,
                ProgressWords.Creating(123456, 654321), CreateWords.Creating,
                StatusLine.NoModel.Text, StatusLine.RevitRefused("x").Text, StatusLine.RevitWouldNot("x").Text,
                StatusLine.BadArgument("x").Text, StatusLine.ReportRefused("x").Text,
                StatusLine.ReportNotWritten("x").Text, StatusLine.Stopped("IOException", "x").Text,
                StatusLine.NotRemembered("kpi-output-folder.txt").Text, StatusLine.NotOpened("C:\\a", "x").Text,
                StatusLine.Reading.Text, StatusLine.Creating.Text,
                CreateBlocked.NoModelOpen, CreateBlocked.NoTemplatesFolder, CreateBlocked.NoOutputFolder,
                CreateBlocked.ModelNotRead, CreateBlocked.NoAssetType, CreateBlocked.NoPlot,
                CreateBlocked.Refused
            };

            lines.AddRange(KpiTemplates.All.Select(AssetTypeNames.Display));

            foreach (KpiSteps steps in EveryStepState())
            {
                foreach (KpiStepState state in steps.All)
                {
                    lines.Add(state.Title);
                    lines.Add(state.WhyNot);
                    lines.Add(state.Tooltip);
                }
            }

            foreach (PaneWarning warning in EveryWarning()) lines.Add(warning.Title);

            foreach (string line in lines) Short(line, 10);
        }

        /// <summary>
        /// The reasons beside a failed plot are shorter still, eight words a sentence, one
        /// fixed sentence per kind and the fallback.
        /// </summary>
        [Fact]
        public void EveryFailedReasonIsEightWordsASentenceAtMost()
        {
            foreach (NotWrittenKind kind in Enum.GetValues(typeof(NotWrittenKind)))
            {
                Short(FailedReasons.Short(kind), 8);
            }
        }

        private static IEnumerable<KpiSteps> EveryStepState()
        {
            yield return KpiSteps.Of(false, false, false, false, false, false, null, 0, 0, false);
            yield return KpiSteps.Of(true, false, false, false, false, true, null, 0, 0, false);
            yield return KpiSteps.Of(false, true, false, false, false, true, null, 0, 0, false);
            yield return KpiSteps.Of(true, true, true, true, true, true, null, 0, 0, false);
            yield return KpiSteps.Of(true, true, true, true, true, true, 99999, 0, 0, false);
            yield return KpiSteps.Of(true, true, true, true, true, true, 99999, 7, 0, false);
            yield return KpiSteps.Of(true, true, true, true, true, true, 99999, 0, 99999, false);
            yield return KpiSteps.Of(true, true, true, true, true, true, 99999, 7, 99999, true);
        }

        // ------------------------------------------------------------ the step row

        [Fact]
        public void TheThirdStepIsCalledPlots()
        {
            KpiSteps steps = KpiSteps.Of(true, true, true, true, true, true, 154, 1, 154, false);

            Assert.Equal("Plots", steps.For(KpiStep.Plots).Title);
            Assert.Equal(3, (int)KpiStep.Plots);
        }

        [Fact]
        public void EachCellsTooltipSaysItsStateShort()
        {
            KpiSteps nothing = KpiSteps.Of(false, false, false, false, false, false, null, 0, 0, false);
            Assert.Equal("Setup: 5 not set.", nothing.For(KpiStep.Setup).Tooltip);
            Assert.Equal("Read: Set the templates and output folders first.", nothing.For(KpiStep.Read).Tooltip);
            Assert.Equal("Plots: Read the model first.", nothing.For(KpiStep.Plots).Tooltip);

            KpiSteps ticked = KpiSteps.Of(true, true, true, true, true, true, 154, 1, 154, false);
            Assert.Equal("Setup: all set, done.", ticked.For(KpiStep.Setup).Tooltip);
            Assert.Equal("Read: 154 plots read, done.", ticked.For(KpiStep.Read).Tooltip);
            Assert.Equal("Plots: 1 asset type, 154 plots, done.", ticked.For(KpiStep.Plots).Tooltip);
            Assert.Equal("Create: nothing done yet.", ticked.For(KpiStep.Create).Tooltip);
        }

        [Fact]
        public void ReadNamesWhichFolderIsMissing()
        {
            Assert.Equal("Set the output folder first.",
                KpiSteps.Of(true, false, false, false, false, true, null, 0, 0, false).For(KpiStep.Read).WhyNot);
            Assert.Equal("Set the templates folder first.",
                KpiSteps.Of(false, true, false, false, false, true, null, 0, 0, false).For(KpiStep.Read).WhyNot);
        }

        // ------------------------------------------------------------ the status lines

        [Fact]
        public void SetupSaysAllSetOrHowManyAreLeft()
        {
            Assert.Equal("All set", KpiSteps.SetupStatus(KpiSteps.NotSet(true, true, true, true, true)));
            Assert.Equal("3 to set", KpiSteps.SetupStatus(KpiSteps.NotSet(true, true, false, false, false)));
            Assert.Equal("5 to set", KpiSteps.SetupStatus(KpiSteps.NotSet(false, false, false, false, false)));
            Assert.Equal("All set", KpiSteps.SetupStatus(null));
        }

        [Fact]
        public void TheFormattedLinesReadAsDesigned()
        {
            Assert.Equal("Read at 08:37", KpiPaneWords.ReadAtClock(new DateTime(2026, 10, 28, 8, 37, 59)));
            Assert.Equal("0.8 s", KpiPaneWords.Seconds(0.84));
            Assert.Equal("96,959", KpiPaneWords.Number(96959));
            Assert.Equal("154 of 163", KpiPaneWords.OfCount(154, 163));
            Assert.Equal("1 plot selected", KpiPaneWords.PlotsSelected(1));
            Assert.Equal("1,234 plots selected", KpiPaneWords.PlotsSelected(1234));
            Assert.Equal("7 templates found", KpiPaneWords.TemplatesFound(7));
            Assert.Equal("1 template found", KpiPaneWords.TemplatesFound(1));
            Assert.Equal("8,353 rows", KpiPaneWords.RowsHeld(8353));
            Assert.Equal("154 plots", KpiPaneWords.PlotsListed(154));
            Assert.Equal("Build 1.2.3.4", KpiPaneWords.Build("1.2.3.4"));
            Assert.Equal("Create 12 workbooks", KpiPaneWords.CreateWorkbooks(12));
            Assert.Equal("Create 1 workbook", KpiPaneWords.CreateWorkbooks(1));
            Assert.Equal("workbooks, each with its PDF", KpiPaneWords.WorkbooksBeside(12, true));
            Assert.Equal("workbooks, no PDF", KpiPaneWords.WorkbooksBeside(12, false));
            Assert.Equal("workbook, each with its PDF", KpiPaneWords.WorkbooksBeside(1, true));
            Assert.Equal("3 workbooks not created", KpiPaneWords.NotCreated(3));
            Assert.Equal("1 workbook not created", KpiPaneWords.NotCreated(1));
            Assert.Equal("150 created, 6 failed", KpiPaneWords.ResultStatus(150, 6));
            Assert.Equal("Creating 3 of 18", ProgressWords.Creating(3, 18));
            Assert.Equal("Creating 3 of 18", PlotProgress.Creating(3, 18, "DM-44").InWords);
            Assert.Equal("DM-44", PlotProgress.Creating(3, 18, "DM-44").PlotId);
        }

        /// <summary>
        /// The handler's lines are short on the status line and keep their whole text beside
        /// it, so the exception message is in the tooltip rather than lost.
        /// </summary>
        [Fact]
        public void AStatusLineKeepsItsWholeTextAsItsDetail()
        {
            StatusLine refused = StatusLine.RevitRefused("The element is not in the document.");
            Assert.Equal("Revit refused the request.", refused.Text);
            Assert.Equal("The element is not in the document.", refused.Detail);

            StatusLine stopped = StatusLine.Stopped("NullReferenceException", "Object reference not set.");
            Assert.Equal("The request failed and was stopped.", stopped.Text);
            Assert.Equal(
                "That request failed and was stopped here rather than being let out. "
                    + "NullReferenceException. Object reference not set.",
                stopped.Detail);

            StatusLine remembered = StatusLine.NotRemembered("kpi-output-folder.txt");
            Assert.Equal("Could not remember that choice.", remembered.Text);
            Assert.Equal(
                "The choice could not be remembered. kpi-output-folder.txt beside the installed "
                    + "add-in refused the write.",
                remembered.Detail);
        }

        // ------------------------------------------------------------ the one reason Create cannot run

        /// <summary>
        /// The order is the order a person fixes them in: each case below has everything
        /// before it fixed and nothing after it, and the reason is the first one still broken.
        /// </summary>
        [Fact]
        public void CreateNamesTheFirstReasonInTheFixedOrder()
        {
            Assert.Equal("No model open", CreateBlocked.Reason(false, false, false, false, false, false, "x"));
            Assert.Equal("Templates folder not set", CreateBlocked.Reason(true, false, false, false, false, false, "x"));
            Assert.Equal("Output folder not set", CreateBlocked.Reason(true, true, false, false, false, false, "x"));
            Assert.Equal("Model not read", CreateBlocked.Reason(true, true, true, false, false, false, "x"));
            Assert.Equal("No asset type selected", CreateBlocked.Reason(true, true, true, true, false, false, "x"));
            Assert.Equal("No plot selected", CreateBlocked.Reason(true, true, true, true, true, false, "x"));
            Assert.Equal("Create refused. See the tooltip.", CreateBlocked.Reason(true, true, true, true, true, true, "x"));
            Assert.Equal(string.Empty, CreateBlocked.Reason(true, true, true, true, true, true, string.Empty));
        }

        /// <summary>
        /// A refusal CannotCreate gives is never missed by the short reason, whichever order the
        /// two are asked in, because the short one is handed the long one's answer.
        /// </summary>
        [Fact]
        public void EveryRefusalCannotCreateGivesGreysTheButton()
        {
            string cannot = CreateWords.CannotCreate(OpenModel.Of("NG05"), string.Empty, true, true);
            Assert.NotEqual(string.Empty, cannot);
            Assert.Equal("Output folder not set", CreateBlocked.Reason(true, true, false, true, true, true, cannot));
        }

        // ------------------------------------------------------------ the short reasons, one per kind

        [Theory]
        [InlineData(NotWrittenKind.NoAssetType, "No asset type places this plot.")]
        [InlineData(NotWrittenKind.SharedFolder, "Shares its folder with another plot.")]
        [InlineData(NotWrittenKind.AssetTypeNotTicked, "Its asset type is not ticked.")]
        [InlineData(NotWrittenKind.PathRefused, "Its folder could not be worked out.")]
        [InlineData(NotWrittenKind.NoOutputRoot, "No output folder to file it under.")]
        [InlineData(NotWrittenKind.NoFolderForComponent, "No folder is known for its component.")]
        [InlineData(NotWrittenKind.TwoFoldersFit, "No component, and two folders fit.")]
        [InlineData(NotWrittenKind.NoUid2, "No PRX_Plot_UID2 to name its folder.")]
        [InlineData(NotWrittenKind.UnusableUid2, "Its PRX_Plot_UID2 cannot name a folder.")]
        [InlineData(NotWrittenKind.RegionPick, "Two regions hold an area. Pick one.")]
        [InlineData(NotWrittenKind.AreaConfirm, "Same area as another plot. Confirm it.")]
        [InlineData(NotWrittenKind.ScheduleCheck, "Schedule check refused it. Details in the report.")]
        [InlineData(NotWrittenKind.FileInUse, "Workbook open in Excel. Close it and try again.")]
        [InlineData(NotWrittenKind.WriteFailed, "Workbook could not be written.")]
        [InlineData(NotWrittenKind.FolderRefused, "The folder refused the write.")]
        [InlineData(NotWrittenKind.FormulaError, "Workbook formulas would show an error.")]
        [InlineData(NotWrittenKind.WouldOverwriteTheTemplate, "It would write over a template.")]
        [InlineData(NotWrittenKind.Crashed, "The write stopped on an error.")]
        [InlineData(NotWrittenKind.Unknown, "Not written. Details in the report.")]
        public void EachKindHasItsOwnShortReason(NotWrittenKind kind, string expected)
        {
            Assert.Equal(expected, FailedReasons.Short(kind));
        }

        [Fact]
        public void TheFileInUseKindComesOffWindowssOwnCode()
        {
            Assert.Equal(PatchRefusal.FileInUse, PatchRefusals.ForIo(unchecked((int)0x80070020)));
            Assert.Equal(PatchRefusal.FileInUse, PatchRefusals.ForIo(unchecked((int)0x80070021)));
            Assert.Equal(PatchRefusal.WriteFailed, PatchRefusals.ForIo(unchecked((int)0x80070070)));
        }

        [Fact]
        public void APatchCarriesItsRefusalKindFromWhereItWasBuilt()
        {
            Assert.Equal(PatchRefusal.Other, PatchOutcome.Refused("Nothing to write.").RefusedAs);
            Assert.Equal(PatchRefusal.FileInUse, PatchOutcome.Refused("held", PatchRefusal.FileInUse).RefusedAs);
            Assert.Equal(PatchRefusal.Other, PatchOutcome.Refused("held", PatchRefusal.None).RefusedAs);
            Assert.Equal(PatchRefusal.FormulaError,
                PatchOutcome.RefusedAfterWriting("divides by nought", FormulaCheck.NotChecked).RefusedAs);
        }

        /// <summary>
        /// The kind is read off the run's own records and never off a reason's words: a run
        /// whose patch was refused for a file held open reads FileInUse whatever its text says.
        /// </summary>
        [Fact]
        public void ARunsKindComesOffItsRecordsNotItsWords()
        {
            KpiCreateRun held = CreateFixture.Run(outcome: PatchOutcome.Refused(
                "Workbook formulas would show an error.", PatchRefusal.FileInUse));
            Assert.True(held.Reconciliation.AddsUp, "the fixture's DM-12 adds up, so the patch decides");
            Assert.Equal(NotWrittenKind.FileInUse, NotWrittenKinds.Of(held));

            Assert.Equal(NotWrittenKind.FolderRefused, NotWrittenKinds.Of(
                CreateFixture.Run(outcome: PatchOutcome.Refused("x", PatchRefusal.FolderRefused))));
            Assert.Equal(NotWrittenKind.Unknown, NotWrittenKinds.Of(CreateFixture.Run(outcome: PatchOutcome.Refused("x"))));
            Assert.Equal(NotWrittenKind.Unknown, NotWrittenKinds.Of(null));
        }

        [Fact]
        public void TwoRegionsTheNoteDoesNotSettleReadAsARegionPick()
        {
            PlotReading open = CreateFixture.Plot("DM-12", regions: new[]
            {
                CreateFixture.Region("RCRC_SOMETHING NOBODY HAS MEASURED", 3982.0),
                CreateFixture.Region("RCRC_A SECOND UNMEASURED TYPE", 4295.0)
            });

            Assert.True(NotWrittenKinds.WaitsOnARegion(open));
            Assert.Equal(NotWrittenKind.RegionPick, NotWrittenKinds.Of(CreateFixture.Run(readings: new[] { open })));

            Assert.False(NotWrittenKinds.WaitsOnARegion(CreateFixture.Plot("DM-11")));
            Assert.False(NotWrittenKinds.WaitsOnARegion(null));
        }

        [Fact]
        public void AnOutcomeCarriesTheKindItWasBuiltWith()
        {
            PlotOutcome refused = PlotOutcome.WroteNothing(
                "NS-01", KpiTemplates.Streets, PlotWorkbookPath.Refused("shared"), false, "shared",
                NotWrittenKind.SharedFolder);
            Assert.Equal(NotWrittenKind.SharedFolder, refused.Kind);
            Assert.Equal(NotWrittenKind.SharedFolder, refused.WithPdf(null).Kind);

            PlotOutcome unknown = PlotOutcome.WroteNothing(
                "NS-02", KpiTemplates.Streets, PlotWorkbookPath.Refused("x"), false, "x");
            Assert.Equal(NotWrittenKind.Unknown, unknown.Kind);
        }

        // ------------------------------------------------------------ the result

        private static PlotWorkbookPath Where(string uid2)
        {
            return PlotWorkbookPath.For("C:\\out", KpiTemplates.Healthcare, "HEALTH", uid2);
        }

        private static KpiCreateRunSet Press(params PlotOutcome[] outcomes)
        {
            List<string> plots = outcomes.Select(one => one.PlotId).ToList();

            return new KpiCreateRunSet(
                "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                PlotsPerTemplate.Split(plots, plotId => "HEALTH", new[] { KpiTemplates.Healthcare }),
                new List<KpiCreateRun>(),
                new List<TemplateOutcome>(),
                null,
                outcomes);
        }

        [Fact]
        public void TheResultCountsTheRunsOwnOutcomes()
        {
            KpiCreateRunSet set = Press(
                PlotOutcome.Wrote("HF-01", KpiTemplates.Healthcare, Where("ANH-007-HF-100001")),
                PlotOutcome.Wrote("HF-02", KpiTemplates.Healthcare, Where("ANH-007-HF-100002")),
                PlotOutcome.WroteNothing("HF-03", KpiTemplates.Healthcare, Where("ANH-007-HF-100003"), true,
                    "The workbook could not be written. If it is open in Excel, close it and press Create again.",
                    NotWrittenKind.FileInUse));

            PressResult result = PressResult.Of(set);

            Assert.Equal(2, result.Created);
            Assert.Equal(1, result.Failed);
            Assert.Equal(set.WorkbooksWritten, result.Created);
            Assert.Equal(set.PlotsThatWroteNothing, result.Failed);
            Assert.False(result.AllCreated);
            Assert.Equal("1 workbook not created", result.Banner);
            Assert.Equal("2 created, 1 failed", result.Status);

            FailedPlot failed = result.FailedPlots.Single();
            Assert.Equal("HF-03", failed.PlotId);
            Assert.Equal("Workbook open in Excel. Close it and try again.", failed.Short);
            Assert.Equal(
                "The workbook could not be written. If it is open in Excel, close it and press Create again.",
                failed.Why);
        }

        [Fact]
        public void APressWhereEveryPlotWroteSaysSo()
        {
            PressResult result = PressResult.Of(Press(
                PlotOutcome.Wrote("HF-01", KpiTemplates.Healthcare, Where("ANH-007-HF-100001"))));

            Assert.True(result.AllCreated);
            Assert.Equal("All workbooks created", result.Banner);
            Assert.Equal("1 created, 0 failed", result.Status);
            Assert.Empty(result.FailedPlots);
        }

        [Fact]
        public void ReadyToSendIsOnlyCountedAgainstAList()
        {
            Assert.Equal(string.Empty, KpiResults.Of(Press(
                PlotOutcome.Wrote("HF-01", KpiTemplates.Healthcare, Where("ANH-007-HF-100001")))).ReadyOf);
        }

        // ------------------------------------------------------------ names, ranges, the table

        [Fact]
        public void AssetTypesReadInSentenceCase()
        {
            Assert.Equal("Existing parks", AssetTypeNames.Display(KpiTemplates.ExistingParks));
            Assert.Equal("Future parks", AssetTypeNames.Display(KpiTemplates.FutureParks));
            Assert.Equal("Healthcare", AssetTypeNames.Display(KpiTemplates.Healthcare));
            Assert.Equal("Mosques", AssetTypeNames.Display(KpiTemplates.Mosques));
            Assert.Equal("Parking", AssetTypeNames.Display(KpiTemplates.Parking));
            Assert.Equal("Schools", AssetTypeNames.Display(KpiTemplates.Schools));
            Assert.Equal("Streets", AssetTypeNames.Display(KpiTemplates.Streets));
            Assert.Equal(string.Empty, AssetTypeNames.Display(null));
        }

        [Fact]
        public void ARangeIsFirstToLastOrTheOneId()
        {
            Assert.Equal(string.Empty, PlotSpan.Of(new string[0]));
            Assert.Equal(string.Empty, PlotSpan.Of(null));
            Assert.Equal("EP-01", PlotSpan.Of(new[] { "EP-01" }));
            Assert.Equal("EP-01 to EP-14", PlotSpan.Of(new[] { "EP-01", "EP-05", "EP-14" }));
        }

        [Fact]
        public void TheTableHasOneRowPerAssetTypeWithPlots()
        {
            TemplateSplit split = PlotsPerTemplate.Split(
                new[] { "DM-11", "DM-12", "FM-05", "SC-03" },
                plotId => plotId.StartsWith("SC", StringComparison.Ordinal) ? "SCHOOL" : "FRIDAY MOSQUE",
                new[] { KpiTemplates.Mosques, KpiTemplates.Schools, KpiTemplates.Streets });

            Assert.Equal(4, KpiWillWrite.Workbooks(split));

            IReadOnlyList<WillWriteRow> rows = KpiWillWrite.Table(split);
            Assert.Equal(2, rows.Count);
            Assert.Equal("Mosques", rows[0].AssetType);
            Assert.Equal(3, rows[0].Plots);
            Assert.Equal("DM-11 to FM-05", rows[0].Range);
            Assert.Equal("Schools", rows[1].AssetType);
            Assert.Equal(1, rows[1].Plots);
            Assert.Equal("SC-03", rows[1].Range);
        }

        // ------------------------------------------------------------ the warning boxes

        private static IEnumerable<PaneWarning> EveryWarning()
        {
            var links = LinksLoaded.Of(new LinkFacts(null, new[]
            {
                new ScannedLinkInstance("00 LINK", "00", false),
                new ScannedLinkInstance("01 LINK", "01", true)
            }, null));

            PlotsInTheModel plots = PlotsInTheModel.Of(
                new[] { "DM-11", "DM-12" }, new[] { "DM-11", "EP-05", "EP-11" }, new[] { "a schedule threw" });

            PlotListRead list = PlotListRead.Of("C:\\list.txt", new[] { "DM-11", "DM-11", "ZZ-99", "not a plot" });

            foreach (PaneWarning one in PaneWarnings.ForRead(plots, links, list)) yield return one;
            foreach (PaneWarning one in PaneWarnings.ForRead(PlotsInTheModel.Of(null, null), LinksLoaded.NotRead, PlotListRead.Refused("C:\\x", "it is gone"))) yield return one;

            yield return PaneWarnings.ListTick(ListTickFreshness.Pressed(new[] { "MOSQUES.xlsx" }), new[] { "SCHOOLS.xlsx" });
            yield return PaneWarnings.OutputIsTheTemplatesFolder();

            TemplateSplit split = PlotsPerTemplate.Split(
                new[] { "EP-05", "EP-11", "FM-08", "ZZ-01", "DM-11" },
                plotId => string.Empty,
                new[] { KpiTemplates.ExistingParks, KpiTemplates.Mosques, KpiTemplates.Schools });

            foreach (PaneWarning one in PaneWarnings.ForCreate(
                split, list, plots, plotId => string.Empty, new[] { "EP-05", "DM-11" }, null, links, null))
            {
                yield return one;
            }
        }

        [Fact]
        public void LinksSayHowManyAreNotLoaded()
        {
            PaneWarning box = PaneWarnings.Links(LinksLoaded.Of(new LinkFacts(null, new[]
            {
                new ScannedLinkInstance("00 LINK", "00", false),
                new ScannedLinkInstance("01 LINK", "01", true)
            }, null)));

            Assert.Equal("1 of 2 linked models not loaded", box.Title);
            Assert.Equal("00 LINK", box.Plots);
            Assert.StartsWith("1 link instance of 2 not loaded", box.Detail);
            Assert.False(box.Blocks);

            Assert.Equal("No linked model is loaded", PaneWarnings.Links(LinksLoaded.Of(new LinkFacts(null, new[]
            {
                new ScannedLinkInstance("00 LINK", "00", false)
            }, null))).Title);

            Assert.Null(PaneWarnings.Links(LinksLoaded.Of(new LinkFacts(null, new[]
            {
                new ScannedLinkInstance("00 LINK", "00", true)
            }, null))));
        }

        [Fact]
        public void TheReadNamesThePlotsTheTwoSourcesDisagreeOn()
        {
            PlotsInTheModel plots = PlotsInTheModel.Of(
                new[] { "DM-11", "DM-12" }, new[] { "DM-11", "EP-05", "EP-11", "EP-12", "EP-13", "" });

            IReadOnlyList<PaneWarning> boxes = PaneWarnings.ForRead(plots, LinksLoaded.NotRead, PlotListRead.NotSet);

            PaneWarning sheetOnly = boxes.Single(one => one.Title == "1 plot on a sheet but on no schedule");
            Assert.Equal("DM-12", sheetOnly.Plots);

            PaneWarning scheduleOnly = boxes.Single(one => one.Title == "4 plots on a schedule but on no sheet");
            Assert.Equal("EP-05, EP-11, EP-12, EP-13", scheduleOnly.Plots);
            Assert.Contains("On a schedule and on no sheet, 4: EP-05, EP-11, EP-12, EP-13", scheduleOnly.Detail);
        }

        [Fact]
        public void ABlankIdIsCountedRatherThanPrintedAsNothing()
        {
            Assert.Equal("EP-05, EP-11, EP-12, EP-13, one blank id",
                PaneWarnings.Ids(new[] { "EP-05", "EP-11", "", "EP-12", "EP-13" }));
            Assert.Equal("EP-05, 2 blank ids", PaneWarnings.Ids(new[] { "EP-05", " ", null }));
            Assert.Equal(string.Empty, PaneWarnings.Ids(null));
        }

        [Fact]
        public void TheListsOwnFaultsAreEachABox()
        {
            PlotsInTheModel plots = PlotsInTheModel.Of(new[] { "DM-11" }, null);
            PlotListRead list = PlotListRead.Of("C:\\list.txt", new[] { "DM-11", "DM-11", "ZZ-99", "not a plot" });

            IReadOnlyList<PaneWarning> boxes = PaneWarnings.ForTheList(plots, list);

            Assert.Equal("ZZ-99", boxes.Single(one => one.Title == "1 listed plot is not in the model").Plots);
            Assert.Equal("DM-11", boxes.Single(one => one.Title == "1 plot is listed twice").Plots);
            Assert.Equal("line 4", boxes.Single(one => one.Title == "1 line is not plot ids").Plots);
            Assert.All(boxes, one => Assert.False(one.Blocks));

            Assert.Equal("Plot list cannot be read",
                PaneWarnings.ForTheList(plots, PlotListRead.Refused("C:\\x", "it is gone")).Single().Title);
            Assert.Empty(PaneWarnings.ForTheList(plots, PlotListRead.NotSet));
        }

        [Fact]
        public void TheListTickOutOfDateIsABoxAndAStandingOneIsNone()
        {
            PaneWarning stale = PaneWarnings.ListTick(
                ListTickFreshness.Pressed(new[] { "MOSQUES.xlsx" }), new[] { "SCHOOLS.xlsx" });
            Assert.Equal("Plot list tick is out of date", stale.Title);
            Assert.Equal(ListTickFreshness.OutOfDateLine, stale.Detail);

            Assert.Null(PaneWarnings.ListTick(ListTickFreshness.Pressed(new[] { "MOSQUES.xlsx" }), new[] { "MOSQUES.xlsx" }));
            Assert.Null(PaneWarnings.ListTick(ListTickFreshness.NeverPressed, new[] { "MOSQUES.xlsx" }));
        }

        /// <summary>
        /// **THE KIND IS A VALUE, AND THE OLD PANE READ IT OUT OF THE WORDS.** It decided warning
        /// against note by searching for WRITTEN NOWHERE. Here the plot that can be filed nowhere
        /// is the box that blocks and the four that file under their template's own folder are
        /// a note, and nothing reads either box's words to decide it.
        /// </summary>
        [Fact]
        public void ThePlotsWithNoComponentAreANoteAndTheOneFiledNowhereBlocks()
        {
            TemplateSplit split = PlotsPerTemplate.Split(
                new[] { "EP-05", "EP-11", "EP-12", "EP-13", "FM-08" },
                plotId => string.Empty,
                new[] { KpiTemplates.ExistingParks, KpiTemplates.Mosques });

            IReadOnlyList<PaneWarning> boxes = PaneWarnings.ForCreate(
                split, PlotListRead.NotSet, null, plotId => string.Empty,
                split.Answers.Select(one => one.PlotId).ToList(), null, null, null);

            PaneWarning filed = boxes.Single(one => one.Title.StartsWith("4 plots have no component", StringComparison.Ordinal));
            Assert.Equal("4 plots have no component. Saved in their asset type folder.", filed.Title);
            Assert.Equal("EP-05, EP-11, EP-12, EP-13", filed.Plots);
            Assert.False(filed.Blocks);

            PaneWarning nowhere = boxes.Single(one => one.Title == "FM-08 can be filed nowhere");
            Assert.True(nowhere.Blocks);
            Assert.Contains("FM-08 WILL BE READ AND WRITTEN NOWHERE", nowhere.Detail);
        }

        [Fact]
        public void AnUnplacedPlotBlocksAndAnEmptyAssetTypeIsANote()
        {
            TemplateSplit split = PlotsPerTemplate.Split(
                new[] { "ZZ-01", "DM-11" },
                plotId => plotId == "DM-11" ? "FRIDAY MOSQUE" : "SOMETHING NOBODY KNOWS",
                new[] { KpiTemplates.Mosques, KpiTemplates.Schools });

            IReadOnlyList<PaneWarning> boxes = PaneWarnings.ForCreate(
                split, PlotListRead.NotSet, null, plotId => string.Empty, new[] { "ZZ-01", "DM-11" }, null, null, null);

            PaneWarning unplaced = boxes.Single(one => one.Title == "1 plot has no asset type");
            Assert.Equal("ZZ-01", unplaced.Plots);
            Assert.True(unplaced.Blocks);

            PaneWarning empty = boxes.Single(one => one.Title == "Schools has no ticked plot");
            Assert.False(empty.Blocks);
        }

        [Fact]
        public void TickedPlotsTheListDoesNotNameAreABox()
        {
            PlotListRead list = PlotListRead.Of("C:\\list.txt", new[] { "DM-11" });
            TemplateSplit split = PlotsPerTemplate.Split(
                new[] { "DM-11", "DM-12" }, plotId => "FRIDAY MOSQUE", new[] { KpiTemplates.Mosques });

            PaneWarning extra = PaneWarnings.ForCreate(
                    split, list, PlotsInTheModel.Of(new[] { "DM-11", "DM-12" }, null),
                    plotId => "FRIDAY MOSQUE", new[] { "DM-11", "DM-12" }, null, null, null)
                .Single(one => one.Title == "1 ticked plot is not on the list");

            Assert.Equal("DM-12", extra.Plots);
            Assert.Equal(TickingTheList.TickedAndNotOnTheListInWords("DM-12"), extra.Detail);
        }

        [Fact]
        public void PlotsSharingOneFolderBlock()
        {
            var sharing = SharedUid2.Of(new[]
            {
                new PlotFiling("NS-01", "ANH-007-ST-100210",
                    PlotWorkbookPath.For("C:\\out", KpiTemplates.Streets, "STREET 30m ROW", "ANH-007-ST-100210")),
                new PlotFiling("NS-42", "ANH-007-ST-100210",
                    PlotWorkbookPath.For("C:\\out", KpiTemplates.Streets, "STREET 30m ROW", "ANH-007-ST-100210"))
            });

            TemplateSplit split = PlotsPerTemplate.Split(
                new[] { "NS-01", "NS-42" }, plotId => "STREET 30m ROW", new[] { KpiTemplates.Streets });

            PaneWarning shared = PaneWarnings.ForCreate(
                    split, PlotListRead.NotSet, null, plotId => "STREET 30m ROW",
                    new[] { "NS-01", "NS-42" }, sharing, null, null)
                .Single(one => one.Title == "2 plots share a folder. None is written.");

            Assert.Equal("NS-01, NS-42", shared.Plots);
            Assert.True(shared.Blocks);
        }

        // ------------------------------------------------------------ BEFORE THE PRESS

        private static BeforeThePress Before()
        {
            return new BeforeThePress(
                "C:\\templates",
                "C:\\MUGHARAZAT",
                string.Empty,
                "C:\\Scope_Validation.xlsx",
                "Scope_Validation.xlsx: 8,353 rows read.",
                string.Empty,
                string.Empty,
                "PRX_Component",
                "PRX_Plot_UID2",
                "PRX_Plot_NH",
                "Counted at 08:37:12, 96959 elements in 0.8 seconds.",
                "Preselected MOSQUES off PRX_Component.",
                new[] { "MOSQUES, from MOSQUES.xlsx:" },
                new[]
                {
                    new PaneWarning("1 plot has no asset type", "ZZ-01", "ZZ-01: no route places it.", true),
                    new PaneWarning("No linked model is loaded", "00 LINK", "NO LINK IS LOADED. One link instance.", false)
                },
                null);
        }

        [Fact]
        public void BeforeThePressHoldsWhatThePaneUsedToSayInFull()
        {
            IReadOnlyList<string> lines = Before().WithScan("4 sheets read. Report at C:\\r.txt.").Lines();
            string text = string.Join("\n", lines.ToArray());

            Assert.Contains("  output folder: C:\\MUGHARAZAT", lines);
            Assert.Contains("    " + CreateWords.WhereTheWorkbooksGo("C:\\MUGHARAZAT"), lines);
            Assert.Contains("    " + TemplateWords.Output("C:\\MUGHARAZAT"), lines);
            Assert.Contains("  forms folder: (not set)", lines);
            Assert.Contains("    " + TemplateWords.FormsFolder(string.Empty), lines);
            Assert.Contains("    Scope_Validation.xlsx: 8,353 rows read.", lines);
            Assert.Contains("  values from: component PRX_Component, reference PRX_Plot_UID2, location PRX_Plot_NH", lines);
            Assert.Contains("    " + TemplateWords.TypedOnThisPane, lines);
            Assert.Contains("  the plots read: Counted at 08:37:12, 96959 elements in 0.8 seconds.", lines);
            Assert.Contains("  the scan: 4 sheets read. Report at C:\\r.txt.", lines);
            Assert.Contains("  asset types: Preselected MOSQUES off PRX_Component.", lines);
            Assert.Contains("  warning boxes the four steps would draw for this press, each once: 2", lines);
            Assert.Contains("    STOPS A WRITE: 1 plot has no asset type | ZZ-01", lines);
            Assert.Contains("      ZZ-01: no route places it.", lines);
            Assert.Contains("    note: No linked model is loaded | 00 LINK", lines);
            Assert.Contains("MUGHARAZAT\\FRIDAY MOSQUE\\ANH-008-MO-100006", text);
        }

        [Fact]
        public void BeforeThePressSitsAfterTheGlanceAndBeforeTheTreeLists()
        {
            KpiCreateRunSet set = Press(PlotOutcome.Wrote("HF-01", KpiTemplates.Healthcare, Where("ANH-007-HF-100001")));
            DateTime at = new DateTime(2026, 10, 28, 9, 0, 0);

            // A press with a tree list check, so the section that follows is really printed.
            set = new KpiCreateRunSet(
                set.DocumentTitle, set.Split, set.Runs, set.Outcomes, null, set.PlotOutcomes,
                null, null, null, null,
                new[] { new TreeListSheetCheck("HEALTHCARE", "Tree List - Existing", null, null) });

            string report = KpiCreateReport.WriteAll(set, at, Before());

            int glance = report.IndexOf("== " + KpiCreateReport.GlanceHeading, StringComparison.Ordinal);
            int before = report.IndexOf("== BEFORE THE PRESS (2) ==", StringComparison.Ordinal);
            int trees = report.IndexOf("== " + TreeListCheck.Heading, StringComparison.Ordinal);
            Assert.True(glance >= 0, "the glance is printed");
            Assert.True(trees >= 0, "the tree lists are printed");
            Assert.True(before > glance, "BEFORE THE PRESS comes after the glance");
            Assert.True(before < trees, "BEFORE THE PRESS comes before the tree lists");
            Assert.Contains("what the pane said before this press, in full", report);
        }

        [Fact]
        public void AReportWithNothingHandedOverPrintsNoSuchSection()
        {
            KpiCreateRunSet set = Press(PlotOutcome.Wrote("HF-01", KpiTemplates.Healthcare, Where("ANH-007-HF-100001")));
            DateTime at = new DateTime(2026, 10, 28, 9, 0, 0);

            Assert.DoesNotContain("BEFORE THE PRESS", KpiCreateReport.WriteAll(set, at));
            Assert.DoesNotContain("BEFORE THE PRESS", KpiCreateReport.WriteAll(set, at, BeforeThePress.Nothing));
            Assert.Same(BeforeThePress.Nothing, BeforeThePress.Nothing.WithScan("anything"));
        }

        /// <summary>
        /// **NOTHING ELSE IN THE REPORT MOVES.** Taking the new section out of a report that
        /// carries it gives back, line for line, the report built with nothing handed over.
        /// </summary>
        [Fact]
        public void TheNewSectionIsTheOnlyDifference()
        {
            KpiCreateRunSet set = Press(PlotOutcome.Wrote("HF-01", KpiTemplates.Healthcare, Where("ANH-007-HF-100001")));
            DateTime at = new DateTime(2026, 10, 28, 9, 0, 0);

            List<string> without = Body(KpiCreateReport.WriteAll(set, at));
            List<string> with = Body(KpiCreateReport.WriteAll(set, at, Before()));

            int start = with.IndexOf("== BEFORE THE PRESS (2) ==");
            int end = with.FindIndex(start + 1, one => one.Length == 0);
            with.RemoveRange(start, end - start + 1);

            Assert.Equal(without, with);
        }

        // The contents block above the body counts the body's lines, so it differs by exactly
        // the new section's size and is left out of the comparison.
        private static List<string> Body(string report)
        {
            List<string> lines = report.Replace("\r\n", "\n").Split('\n').ToList();
            int body = lines.IndexOf("RCRC Green KPI checklist");
            return lines.Skip(body).ToList();
        }
    }
}
