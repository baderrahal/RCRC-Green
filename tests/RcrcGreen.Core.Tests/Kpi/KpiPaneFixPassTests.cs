using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **THE FIRST FIX PASS ON THE PANE REDESIGN.** A breaker and a reviewer read the round and
    /// found reasons said wrong, a green banner over missing PDFs, a refusal that did not
    /// refuse, rules worked out twice, and tests that were not there. Each test below names the
    /// item it holds.
    ///
    /// **EVERY EXPECTED VALUE HERE IS WRITTEN OUT BY HAND.**
    /// </summary>
    public class KpiPaneFixPassTests
    {
        // ------------------------------------------------------------ item 1, a path refusal's own kind

        /// <summary>
        /// FM-08 carries no component and MOSQUES reaches two folders, so it read no usable
        /// PRX_Plot_UID2, which was a sentence about the wrong thing. Each refusal now carries
        /// its own kind from where it is built.
        /// </summary>
        [Fact]
        public void EveryPathRefusalCarriesItsOwnKind()
        {
            Assert.Equal(PathRefusal.TemplateReachesTwoFolders,
                PlotWorkbookPath.For("C:\\out", KpiTemplates.Mosques, string.Empty, "ANH-007-MO-100030").RefusedAs);
            Assert.Equal(PathRefusal.NoFolderForComponent,
                PlotWorkbookPath.For("C:\\out", KpiTemplates.Mosques, "SOMETHING NOBODY KNOWS", "ANH-007-MO-100030").RefusedAs);
            Assert.Equal(PathRefusal.NoUid2,
                PlotWorkbookPath.For("C:\\out", KpiTemplates.Mosques, "FRIDAY MOSQUE", " ").RefusedAs);
            Assert.Equal(PathRefusal.UnusableUid2,
                PlotWorkbookPath.For("C:\\out", KpiTemplates.Mosques, "FRIDAY MOSQUE", "ANH/007").RefusedAs);
            Assert.Equal(PathRefusal.NoRoot,
                PlotWorkbookPath.For(string.Empty, KpiTemplates.Mosques, "FRIDAY MOSQUE", "ANH-007-MO-100030").RefusedAs);
            Assert.Equal(PathRefusal.Other, PlotWorkbookPath.Refused("from outside").RefusedAs);
            Assert.Equal(PathRefusal.None,
                PlotWorkbookPath.For("C:\\out", KpiTemplates.Mosques, "FRIDAY MOSQUE", "ANH-007-MO-100030").RefusedAs);
        }

        [Fact]
        public void FM08ReadsAsTwoFoldersAndNotAsAMissingUid2()
        {
            PlotWorkbookPath fm08 = PlotWorkbookPath.For("C:\\out", KpiTemplates.Mosques, string.Empty, "ANH-007-MO-100019");

            KpiCreateRun run = CreateFixture.Run(where: fm08);

            Assert.Equal(NotWrittenKind.TwoFoldersFit, NotWrittenKinds.Of(run));
            Assert.Equal("No component, and two folders fit.", FailedReasons.Short(NotWrittenKinds.Of(run)));
        }

        [Fact]
        public void EachPathKindMapsToItsOwnShortReason()
        {
            Assert.Equal(NotWrittenKind.NoFolderForComponent, NotWrittenKinds.ForPath(
                PlotWorkbookPath.For("C:\\out", KpiTemplates.Mosques, "SOMETHING NOBODY KNOWS", "ANH-007-MO-100030")));
            Assert.Equal(NotWrittenKind.NoUid2, NotWrittenKinds.ForPath(
                PlotWorkbookPath.For("C:\\out", KpiTemplates.Mosques, "FRIDAY MOSQUE", string.Empty)));
            Assert.Equal(NotWrittenKind.UnusableUid2, NotWrittenKinds.ForPath(
                PlotWorkbookPath.For("C:\\out", KpiTemplates.Mosques, "FRIDAY MOSQUE", "ANH*007")));
            Assert.Equal(NotWrittenKind.NoOutputRoot, NotWrittenKinds.ForPath(
                PlotWorkbookPath.For(null, KpiTemplates.Mosques, "FRIDAY MOSQUE", "ANH-007-MO-100030")));
            Assert.Equal(NotWrittenKind.PathRefused, NotWrittenKinds.ForPath(PlotWorkbookPath.Refused("from outside")));
            Assert.Equal(NotWrittenKind.Unknown, NotWrittenKinds.ForPath(
                PlotWorkbookPath.For("C:\\out", KpiTemplates.Mosques, "FRIDAY MOSQUE", "ANH-007-MO-100030")));
            Assert.Equal(NotWrittenKind.Unknown, NotWrittenKinds.ForPath(null));
        }

        // ------------------------------------------------------------ item 2, not ticked against no route

        private static TemplateSplit MosquesAndStreetsTicked()
        {
            var components = new Dictionary<string, string>
            {
                { "DM-11", "DAILY MOSQUE" },
                { "SC-03", "SCHOOL" },
                { "EP-05", string.Empty },
                { "ZZ-01", "SOMETHING NOBODY KNOWS" }
            };

            return PlotsPerTemplate.Split(
                new[] { "DM-11", "SC-03", "EP-05", "ZZ-01" },
                plotId => components[plotId],
                new[] { KpiTemplates.Mosques, KpiTemplates.Streets });
        }

        [Fact]
        public void APlotWhoseAssetTypeIsNotTickedIsToldApartFromOneNoRoutePlaces()
        {
            TemplateSplit split = MosquesAndStreetsTicked();

            PlotTemplate school = split.Unplaced.Single(one => one.PlotId == "SC-03");
            Assert.True(school.AssetTypeNotTicked);
            Assert.Same(KpiTemplates.Schools, school.NotTicked);
            Assert.Equal(NotWrittenKind.AssetTypeNotTicked, NotWrittenKinds.ForUnplaced(school));

            PlotTemplate park = split.Unplaced.Single(one => one.PlotId == "EP-05");
            Assert.Same(KpiTemplates.ExistingParks, park.NotTicked);

            PlotTemplate nobody = split.Unplaced.Single(one => one.PlotId == "ZZ-01");
            Assert.False(nobody.AssetTypeNotTicked);
            Assert.Equal(NotWrittenKind.NoAssetType, NotWrittenKinds.ForUnplaced(nobody));

            Assert.Equal("Its asset type is not ticked.", FailedReasons.Short(NotWrittenKind.AssetTypeNotTicked));
        }

        [Fact]
        public void TheTwoUnplacedKindsAreTwoBoxes()
        {
            TemplateSplit split = MosquesAndStreetsTicked();

            IReadOnlyList<PaneWarning> boxes = PaneWarnings.ForCreate(
                split, PlotListRead.NotSet, null, plotId => string.Empty,
                new[] { "DM-11", "SC-03", "EP-05", "ZZ-01" }, null, null, null);

            PaneWarning noRoute = boxes.Single(one => one.Title == "1 plot has no asset type");
            Assert.Equal("ZZ-01", noRoute.Plots);
            Assert.True(noRoute.Blocks);

            PaneWarning notTicked = boxes.Single(one => one.Title == "2 plots belong to an asset type not ticked");
            Assert.Equal("SC-03, EP-05", notTicked.Plots);
            Assert.False(notTicked.Blocks);
        }

        // ------------------------------------------------------------ item 3, the PDFs a press expected

        private static PlotWorkbookPath Where(string uid2)
        {
            return PlotWorkbookPath.For("C:\\out", KpiTemplates.Healthcare, "HEALTH", uid2);
        }

        private static PlotOutcome WithPdf(string plotId, bool pdfWritten)
        {
            PlotWorkbookPath where = Where("ANH-007-HF-1000" + plotId.Substring(3));
            PdfOutcome pdf = pdfWritten
                ? PdfOutcome.Wrote(plotId, PdfForms.ForPlot(plotId), PdfChecklist.Beside(where), null,
                    new List<PdfLandedField>(), new PdfFieldFill[0])
                : PdfOutcome.WroteNothing(plotId, PdfForms.ForPlot(plotId), "the form's fields have moved", null);

            return PlotOutcome.Wrote(plotId, KpiTemplates.Healthcare, where).WithPdf(pdf);
        }

        private static KpiCreateRunSet Press(bool pdfsWanted, IReadOnlyList<TemplateOutcome> templates, params PlotOutcome[] outcomes)
        {
            return new KpiCreateRunSet(
                "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                PlotsPerTemplate.Split(outcomes.Select(one => one.PlotId), plotId => "HEALTH", new[] { KpiTemplates.Healthcare }),
                new List<KpiCreateRun>(),
                templates ?? new List<TemplateOutcome>(),
                null,
                outcomes,
                pdfsWanted: pdfsWanted);
        }

        [Fact]
        public void AMissingExpectedPdfIsNotTheGreenBanner()
        {
            PressResult result = PressResult.Of(Press(true, null, WithPdf("HF-01", true), WithPdf("HF-02", false)));

            Assert.Equal(0, result.Failed);
            Assert.Equal(ResultBanner.PdfsNotWritten, result.Kind);
            Assert.Equal("1 PDF not written", result.Banner);

            MissingPdf missing = result.PdfsNotWritten.Single();
            Assert.Equal("HF-02", missing.PlotId);
            Assert.Equal("the form's fields have moved", missing.Why);

            PaneWarning box = PaneWarnings.PdfsNotWritten(result);
            Assert.Equal("1 PDF not written", box.Title);
            Assert.Equal("HF-02", box.Plots);
            Assert.Equal("HF-02: the form's fields have moved.", box.Detail);
        }

        [Fact]
        public void WithNoFormsFolderNoPdfIsMissing()
        {
            PressResult result = PressResult.Of(Press(false, null, WithPdf("HF-01", false)));

            Assert.Equal(ResultBanner.AllCreated, result.Kind);
            Assert.Equal("All workbooks created", result.Banner);
            Assert.Empty(result.PdfsNotWritten);
            Assert.Null(PaneWarnings.PdfsNotWritten(result));
        }

        [Fact]
        public void AWorkbookNotCreatedOutranksAMissingPdf()
        {
            // **BUILT THE WAY THE HANDLER BUILDS IT.** A plot whose workbook OnePlot refused still
            // gets its PDF run, Bader's decision, so its outcome carries a PDF record. Its missing
            // PDF is a consequence of the workbook, which the failed list already names.
            PlotWorkbookPath where = Where("ANH-007-HF-100003");
            PlotOutcome refused = PlotOutcome.WroteNothing(
                    "HF-03", KpiTemplates.Healthcare, where, true,
                    "The workbook could not be written.", NotWrittenKind.WriteFailed)
                .WithPdf(PdfOutcome.WroteNothing(
                    "HF-03", PdfForms.ForPlot("HF-03"), "the workbook it reads was not written", null));

            PressResult result = PressResult.Of(Press(true, null, WithPdf("HF-01", false), refused));

            Assert.Equal(ResultBanner.WorkbooksNotCreated, result.Kind);
            Assert.Equal("1 workbook not created", result.Banner);
            Assert.Equal(new[] { "HF-01" }, result.PdfsNotWritten.Select(one => one.PlotId).ToArray());
            Assert.Equal("the form's fields have moved", result.PdfsNotWritten[0].Why);
        }

        /// <summary>
        /// **THE 16:37 SHAPE.** NS-01 and NS-42 share ANH-007-ST-100210 and the handler builds
        /// their outcomes with no PDF record at all, because the press stops them before their
        /// PDF step. They are failed plots and not missing PDFs, and the pane counts exactly
        /// what the report's glance counts.
        /// </summary>
        [Fact]
        public void PlotsSharingAFolderAreNotMissingPdfs()
        {
            PlotWorkbookPath shared = PlotWorkbookPath.For("C:\\out", KpiTemplates.Streets, "STREET 30m ROW", "ANH-007-ST-100210");
            PlotOutcome ns01 = PlotOutcome.WroteNothing("NS-01", KpiTemplates.Streets, shared, false, "shared", NotWrittenKind.SharedFolder);
            PlotOutcome ns42 = PlotOutcome.WroteNothing("NS-42", KpiTemplates.Streets, shared, false, "shared", NotWrittenKind.SharedFolder);

            KpiCreateRunSet set = Press(true, null, WithPdf("HF-01", true), WithPdf("HF-02", false), ns01, ns42);
            PressResult result = PressResult.Of(set);

            Assert.Equal(new[] { "HF-02" }, result.PdfsNotWritten.Select(one => one.PlotId).ToArray());
            Assert.DoesNotContain("NS-01", PaneWarnings.PdfsNotWritten(result).Plots);
            Assert.DoesNotContain("NS-42", PaneWarnings.PdfsNotWritten(result).Plots);

            Assert.Equal(
                RunAtAGlance.Of(set).Pdfs.WithNoPdf,
                result.PdfsNotWritten.Select(one => one.PlotId + ": " + one.Why).ToList());
        }

        // ------------------------------------------------------------ item 4, the templates folder as output

        [Fact]
        public void TheTemplatesFolderAsOutputIsANoteAndStopsNothing()
        {
            Assert.False(PaneWarnings.OutputIsTheTemplatesFolder().Blocks);
            Assert.Equal(string.Empty, CreateBlocked.Reason(true, true, true, true, true, true, string.Empty));
        }

        // ------------------------------------------------------------ item 5, every box once

        [Fact]
        public void BeforeThePressHoldsEachBoxOnce()
        {
            var links = new PaneWarning("No linked model is loaded", "00 LINK", "NO LINK IS LOADED.", false);
            var again = new PaneWarning("No linked model is loaded", "00 LINK", "NO LINK IS LOADED.", false);
            var other = new PaneWarning("1 plot has no asset type", "ZZ-01", "ZZ-01: no route.", true);

            var before = new BeforeThePress(
                "C:\\t", "C:\\o", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                "c", "r", "l", string.Empty, string.Empty, null, new[] { links, other, again }, null);

            Assert.Equal(2, before.Warnings.Count);
            Assert.Contains("  warning boxes the four steps would draw for this press, each once: 2", before.Lines());
            Assert.Single(before.Lines(), one => one == "    note: No linked model is loaded | 00 LINK");
        }

        // ------------------------------------------------------------ item 7, one reason Create cannot run

        /// <summary>
        /// Every combination of the six facts: when step 4 is shut its reason is the line under
        /// the button, word for word, and both are <see cref="CreateBlocked.Reason"/>.
        /// </summary>
        [Fact]
        public void StepFoursReasonAndTheButtonsLineCannotDiffer()
        {
            foreach (int bits in Enumerable.Range(0, 64))
            {
                bool templates = (bits & 1) != 0, output = (bits & 2) != 0, open = (bits & 4) != 0;
                bool read = (bits & 8) != 0, assetType = (bits & 16) != 0, plot = (bits & 32) != 0;

                KpiSteps steps = KpiSteps.Of(
                    templates, output, false, false, false, open,
                    read ? 154 : (int?)null, assetType ? 1 : 0, plot ? 154 : 0, false);

                Assert.Equal(
                    CreateBlocked.Reason(open, templates, output, read, assetType, plot, string.Empty),
                    steps.CreateBlockedBy);

                KpiStepState create = steps.For(KpiStep.Create);
                if (!create.Usable) Assert.Equal(steps.CreateBlockedBy, create.WhyNot);

                string cannot = CreateWords.CannotCreate(
                    open ? OpenModel.Of("NG05") : OpenModel.Nothing, output ? "C:\\o" : string.Empty, assetType, plot);
                if (cannot.Length > 0) Assert.NotEqual(string.Empty, steps.CreateBlockedBy);
            }
        }

        // ------------------------------------------------------------ item 8, the boxes and kinds with no test

        [Fact]
        public void IdenticalAreasAreOneErrorBox()
        {
            PaneWarning box = PaneWarnings.SameArea(new[]
            {
                new IdenticalArea(new[] { "MM-03", "MM-04" }, 131127.0, "12182.05561411")
            });

            Assert.Equal("Plots report the same area", box.Title);
            Assert.Equal("MM-03, MM-04", box.Plots);
            Assert.True(box.Blocks);
            Assert.Equal(
                "CONFIRM BEFORE WRITING. 1 set of chosen plots report the same area. MM-03, MM-04 all "
                    + "read 12182.05561411. Either they are the same size or one region is counted twice.",
                box.Detail);

            Assert.Null(PaneWarnings.SameArea(new IdenticalArea[0]));
            Assert.Null(PaneWarnings.SameArea(null));
        }

        private static PrintedGroup LeftOutGroup(string name, int row)
        {
            return new PrintedGroup(name, row, new SpeciesRow[0], true, 0, row + 1, false, CountedGroups.LeftOut);
        }

        private static PrintedGroup TakenGroup(string name, int row)
        {
            return new PrintedGroup(name, row, new SpeciesRow[0], true, 0, row + 1, true, "Tree List - Proposed is named for it");
        }

        private static PlotReading WithStreetDesign(string plot, bool leftOut)
        {
            return CreateFixture.Plot(
                plot,
                regions: new[] { CreateFixture.Region(CreateFixture.OutOfScope, 900.0) },
                printedGroups: leftOut
                    ? new[] { TakenGroup("Proposed", 3), LeftOutGroup("Street Design", 9) }
                    : new[] { TakenGroup("Proposed", 3) });
        }

        [Fact]
        public void TheGroupsLeftOutNameDm16AndFm05()
        {
            PlotReading[] readings =
            {
                WithStreetDesign("FM-05", true), WithStreetDesign("DM-12", false), WithStreetDesign("DM-16", true)
            };

            Assert.Equal(new[] { "DM-16", "FM-05" }, CreateWords.PlotsWithGroupsLeftOut(readings).ToArray());

            KpiCreateRun run = CreateFixture.Run(readings: readings, template: KpiTemplates.Mosques);
            PaneWarning box = PaneWarnings.GroupsLeftOut(new[] { run }).Single();

            Assert.Equal("Rows with no tree list sheet were left out", box.Title);
            Assert.Equal("DM-16, FM-05", box.Plots);
            Assert.False(box.Blocks);
            Assert.Equal(
                "MOSQUES: Street Design found on 2 plots on MOSQUES, which has no sheet for it: DM-16 in "
                    + "its softscape schedule, FM-05 in its softscape schedule. Those rows were left out. "
                    + "Fix them in the model.",
                box.Detail);

            Assert.Empty(PaneWarnings.GroupsLeftOut(new[] { CreateFixture.Run(readings: new[] { WithStreetDesign("DM-12", false) }) }));
        }

        [Fact]
        public void PlotsSharingAnIdInTwoFoldersAreANote()
        {
            var sharing = SharedUid2.Of(new[]
            {
                new PlotFiling("DM-11", "ANH-007-MO-100001",
                    PlotWorkbookPath.For("C:\\out", KpiTemplates.Mosques, "DAILY MOSQUE", "ANH-007-MO-100001")),
                new PlotFiling("FM-05", "ANH-007-MO-100001",
                    PlotWorkbookPath.For("C:\\out", KpiTemplates.Mosques, "FRIDAY MOSQUE", "ANH-007-MO-100001"))
            });

            TemplateSplit split = PlotsPerTemplate.Split(
                new[] { "DM-11", "FM-05" }, plotId => "FRIDAY MOSQUE", new[] { KpiTemplates.Mosques });

            IReadOnlyList<PaneWarning> boxes = PaneWarnings.ForCreate(
                split, PlotListRead.NotSet, null, plotId => "FRIDAY MOSQUE", new[] { "DM-11", "FM-05" }, sharing, null, null);

            PaneWarning apart = boxes.Single(one => one.Title == "2 plots share an id, filed apart");
            Assert.Equal("DM-11, FM-05", apart.Plots);
            Assert.False(apart.Blocks);
            Assert.DoesNotContain(boxes, one => one.Title.EndsWith("None is written.", StringComparison.Ordinal));
        }

        [Fact]
        public void AListedPlotThatGetsNoWorkbookOrPdfIsABox()
        {
            PlotListRead list = PlotListRead.Of("C:\\list.txt", new[] { "DM-11", "ZZ-01" });
            PlotsInTheModel plots = PlotsInTheModel.Of(new[] { "DM-11", "ZZ-01" }, null);
            TemplateSplit split = PlotsPerTemplate.Split(
                new[] { "DM-11" }, plotId => "FRIDAY MOSQUE", new[] { KpiTemplates.Mosques });

            PaneWarning box = PaneWarnings.ForCreate(
                    split, list, plots, plotId => plotId == "DM-11" ? "FRIDAY MOSQUE" : string.Empty,
                    new[] { "DM-11" }, null, null, null)
                .Single(one => one.Title == "1 listed plot gets no workbook or PDF");

            Assert.Equal("ZZ-01", box.Plots);
            Assert.Equal(TickingTheList.WouldWriteNothing("ZZ-01", plotId => string.Empty), box.Detail);
        }

        [Fact]
        public void SchedulesTheReadCouldNotAskAreABox()
        {
            PlotsInTheModel plots = PlotsInTheModel.Of(
                new[] { "DM-11" }, new[] { "DM-11" },
                new[] { "DM-11-(600) SOFTSCAPE SCHEDULE threw.", "DM-11-(600) SHRUBS & LAWN SCHEDULE threw." });

            PaneWarning box = PaneWarnings.ForRead(plots, LinksLoaded.NotRead, PlotListRead.NotSet)
                .Single(one => one.Title == "2 schedules could not be read");

            Assert.Equal(string.Empty, box.Plots);
            Assert.Equal("DM-11-(600) SOFTSCAPE SCHEDULE threw. DM-11-(600) SHRUBS & LAWN SCHEDULE threw.", box.Detail);
        }

        [Fact]
        public void IdenticalAreasReadAsAnAreaConfirm()
        {
            PlotReading mm03 = CreateFixture.Plot("MM-03", regions: new[] { CreateFixture.Region(CreateFixture.OutOfScope, 12182.05561411, 131127.0) });
            PlotReading mm04 = CreateFixture.Plot("MM-04", regions: new[] { CreateFixture.Region(CreateFixture.OutOfScope, 12182.05561411, 131127.0) });

            KpiCreateRun run = CreateFixture.Run(readings: new[] { mm03, mm04 });

            Assert.NotEmpty(run.Reconciliation.IdenticalAreas);
            Assert.Equal(NotWrittenKind.AreaConfirm, NotWrittenKinds.Of(run));
        }

        [Fact]
        public void ARefusedScheduleReadsAsAScheduleCheck()
        {
            KpiCreateRun run = CreateFixture.Run(readings: new[] { CreateFixture.Plot("DM-12", readRefusals: new[] { "the BOTANICAL NAME column was not named by the heading row" }) });

            Assert.False(run.Reconciliation.AddsUp);
            Assert.Equal(NotWrittenKind.ScheduleCheck, NotWrittenKinds.Of(run));
        }

        [Fact]
        public void AFormulaRefusalReadsAsAFormulaError()
        {
            KpiCreateRun run = CreateFixture.Run(
                outcome: PatchOutcome.RefusedAfterWriting("divides by nought", FormulaCheck.NotChecked));

            Assert.Equal(NotWrittenKind.FormulaError, NotWrittenKinds.Of(run));
        }

        // ------------------------------------------------------------ item 12, Show in report

        [Fact]
        public void ShowInReportIsOfferedOnlyOnABoxThatPressWrote()
        {
            var shown = new PaneWarning("1 plot has no asset type", "ZZ-01", "ZZ-01: no route.", true);
            var later = new PaneWarning("2 plots have no asset type", "ZZ-01, ZZ-02", "ZZ-01: no route. ZZ-02: no route.", true);

            var before = new BeforeThePress(
                "C:\\t", "C:\\o", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                "c", "r", "l", string.Empty, string.Empty, null, new[] { shown }, null);

            Assert.True(before.Holds(new PaneWarning("1 plot has no asset type", "ZZ-01", "ZZ-01: no route.", true)));
            Assert.False(before.Holds(later));
            Assert.False(BeforeThePress.Nothing.Holds(shown));
            Assert.False(before.Holds(null));
        }

        // ------------------------------------------------------------ item 13, two counts in the window

        [Fact]
        public void TheWindowCountsReadsAndWritesUnderTwoWords()
        {
            Assert.Equal("Reading 3 of 18", ProgressWords.Reading(3, 18));
            Assert.Equal("Creating 1 of 18", ProgressWords.Creating(1, 18));
            Assert.Equal("Reading the model", ProgressWords.ReadingTheModel);

            PlotProgress reading = PlotProgress.Reading(3, 18, "DM-44");
            Assert.Equal(PressPhase.Reading, reading.Phase);
            Assert.Equal("Reading 3 of 18", reading.InWords);
            Assert.Equal("DM-44", reading.PlotId);

            Assert.Equal("Creating 1 of 18", PlotProgress.Creating(1, 18, "DM-11").InWords);
            Assert.Equal("Reading the model", PlotProgress.TheModel.InWords);
            Assert.Equal(string.Empty, PlotProgress.TheModel.PlotId);
        }

        // ------------------------------------------------------------ item 14, a row per asset type

        [Fact]
        public void TheResultHasARowPerTickedAssetType()
        {
            PlotOutcome refused = PlotOutcome.WroteNothing(
                "HF-03", KpiTemplates.Healthcare, Where("ANH-007-HF-100003"), false, "x", NotWrittenKind.WriteFailed);

            KpiCreateRunSet set = Press(false, new[]
                {
                    TemplateOutcome.WroteSome(KpiTemplates.Healthcare, new[] { "HF-01", "HF-02", "HF-03" }, 2, "C:\\out", "x"),
                    TemplateOutcome.NothingToWrite(KpiTemplates.Schools, PlotsPerTemplate.NoPlotBelongs(KpiTemplates.Schools))
                },
                WithPdf("HF-01", true), WithPdf("HF-02", true), refused);

            IReadOnlyList<AssetTypeResult> rows = PressResult.Of(set).ByAssetType;

            Assert.Equal(2, rows.Count);
            Assert.Equal("Healthcare", rows[0].AssetType);
            Assert.Equal(2, rows[0].Created);
            Assert.Equal(1, rows[0].Failed);
            Assert.False(rows[0].NothingToWrite);

            Assert.Equal("Schools", rows[1].AssetType);
            Assert.Equal(0, rows[1].Created);
            Assert.Equal(0, rows[1].Failed);
            Assert.True(rows[1].NothingToWrite);
            Assert.Equal("Nothing to write", AssetTypeResult.NothingToWriteWords);
        }

        // ------------------------------------------------------------ item 15, the plots not ready

        [Fact]
        public void PlotsNotReadyToSendAreABox()
        {
            List<string> listed = new List<string> { "HF-01", "HF-02" };
            var set = new KpiCreateRunSet(
                "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                PlotsPerTemplate.Split(listed, plotId => "HEALTH", new[] { KpiTemplates.Healthcare }),
                new List<KpiCreateRun>(),
                new List<TemplateOutcome>(),
                null,
                new[]
                {
                    WithPdf("HF-01", true),
                    PlotOutcome.WroteNothing("HF-02", KpiTemplates.Healthcare,
                        PlotWorkbookPath.Refused(PlotWorkbookPath.NoUid2), false, PlotWorkbookPath.NoUid2)
                },
                null,
                PlotsInTheModel.Of(listed, listed),
                PlotListRead.Of("C:\\lists\\154.txt", listed));

            KpiResults results = KpiResults.Of(set);
            PaneWarning box = PaneWarnings.NotReady(results);

            Assert.Equal("1 plot not ready to send", box.Title);
            Assert.Equal("HF-02", box.Plots);
            Assert.Equal(results.NotReadyRows.Single().InWords, box.Detail);
            Assert.False(results.NotReadyInWords.EndsWith(":", StringComparison.Ordinal));
        }

        // ------------------------------------------------------------ item 17, never every plot

        [Fact]
        public void ABoxNamesFourPlotsAndCountsTheRest()
        {
            Assert.Equal("ST-01, ST-02, ST-03, ST-04 and 74 more",
                PaneWarnings.Ids(Enumerable.Range(1, 78).Select(one => "ST-" + one.ToString("00"))));
            Assert.Equal("EP-01, EP-02, EP-03, EP-04 and 2 more",
                PaneWarnings.Ids(new[] { "EP-01", "EP-02", "EP-03", "EP-04", "EP-05", "" }));
            Assert.Equal("EP-01, EP-02, EP-03, EP-04",
                PaneWarnings.Ids(new[] { "EP-01", "EP-02", "EP-03", "EP-04" }));
            Assert.Equal(4, CreateWords.ShownByName);
        }

        // ------------------------------------------------------------ item 11, the button's name

        [Fact]
        public void TheSetupLineNamesThePlotListButton()
        {
            Assert.Contains("so Plot list cannot be pressed", KpiSteps.PlotListNotSet);
            Assert.DoesNotContain("Tick the list", KpiSteps.PlotListNotSet);
        }

        // ------------------------------------------------------------ pass 2, item 2, the rows add up

        /// <summary>
        /// A hand ticked SC-03 with only MOSQUES ticked has no template on its outcome. It sits
        /// in the row of plots no ticked asset type took, so the rows add up to the two boxes.
        /// </summary>
        [Fact]
        public void TheAssetTypeRowsAddUpToTheTwoBoxes()
        {
            PlotWorkbookPath dm11 = PlotWorkbookPath.For("C:\\out", KpiTemplates.Mosques, "DAILY MOSQUE", "ANH-007-MO-100001");
            PlotWorkbookPath dm12 = PlotWorkbookPath.For("C:\\out", KpiTemplates.Mosques, "DAILY MOSQUE", "ANH-007-MO-100002");

            var set = new KpiCreateRunSet(
                "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                PlotsPerTemplate.Split(new[] { "DM-11", "DM-12", "SC-03" },
                    plotId => plotId == "SC-03" ? "SCHOOL" : "DAILY MOSQUE", new[] { KpiTemplates.Mosques }),
                new List<KpiCreateRun>(),
                new[] { TemplateOutcome.WroteSome(KpiTemplates.Mosques, new[] { "DM-11", "DM-12" }, 1, "C:\\out", "x") },
                null,
                new[]
                {
                    PlotOutcome.Wrote("DM-11", KpiTemplates.Mosques, dm11),
                    PlotOutcome.WroteNothing("DM-12", KpiTemplates.Mosques, dm12, true, "x", NotWrittenKind.FileInUse),
                    PlotOutcome.WroteNothing("SC-03", null, PlotWorkbookPath.Refused("not ticked"), false,
                        "not ticked", NotWrittenKind.AssetTypeNotTicked)
                });

            PressResult result = PressResult.Of(set);

            Assert.Equal(result.Created, result.ByAssetType.Sum(one => one.Created));
            Assert.Equal(result.Failed, result.ByAssetType.Sum(one => one.Failed));

            AssetTypeResult nowhere = result.ByAssetType.Single(one => one.Unplaced);
            Assert.Equal("No ticked asset type", nowhere.AssetType);
            Assert.Equal(0, nowhere.Created);
            Assert.Equal(1, nowhere.Failed);

            Assert.Equal(1, result.ByAssetType.Single(one => one.AssetType == "Mosques").Failed);
        }

        // ------------------------------------------------------------ pass 2, item 3, what the press really writes

        private static string MosqueUid2(string plotId)
        {
            return "ANH-007-MO-1" + plotId.Substring(3).PadLeft(5, '0');
        }

        [Fact]
        public void TheCountLeavesOutAPlotFiledNowhere()
        {
            List<string> plots = Enumerable.Range(1, 20).Select(one => "DM-" + one.ToString("00")).ToList();
            plots.Add("FM-08");

            Func<string, string> componentOf = plotId => plotId == "FM-08" ? string.Empty : "DAILY MOSQUE";
            TemplateSplit split = PlotsPerTemplate.Split(plots, componentOf, new[] { KpiTemplates.Mosques });

            Assert.Equal(21, KpiWillWrite.Workbooks(split));
            Assert.Equal(20, KpiWillWrite.Workbooks(split, PlotFilings.Of("C:\\out", plots, componentOf, MosqueUid2)));
        }

        [Fact]
        public void TheCountLeavesOutBothPlotsOfASharedFolder()
        {
            var plots = new[] { "NS-01", "NS-42", "NS-43" };
            Func<string, string> componentOf = plotId => "STREET 30m ROW";
            Func<string, string> uid2Of = plotId => plotId == "NS-43" ? "ANH-007-ST-100211" : "ANH-007-ST-100210";

            TemplateSplit split = PlotsPerTemplate.Split(plots, componentOf, new[] { KpiTemplates.Streets });

            Assert.Equal(3, KpiWillWrite.Workbooks(split));
            Assert.Equal(1, KpiWillWrite.Workbooks(split, PlotFilings.Of("C:\\out", plots, componentOf, uid2Of)));
        }

        // ------------------------------------------------------------ pass 2, item 5, what a retry can write

        [Theory]
        [InlineData(NotWrittenKind.NoAssetType, false)]
        [InlineData(NotWrittenKind.AssetTypeNotTicked, false)]
        [InlineData(NotWrittenKind.SharedFolder, false)]
        [InlineData(NotWrittenKind.NoFolderForComponent, false)]
        [InlineData(NotWrittenKind.TwoFoldersFit, false)]
        [InlineData(NotWrittenKind.NoUid2, false)]
        [InlineData(NotWrittenKind.UnusableUid2, false)]
        [InlineData(NotWrittenKind.Unknown, true)]
        [InlineData(NotWrittenKind.PathRefused, true)]
        [InlineData(NotWrittenKind.NoOutputRoot, true)]
        [InlineData(NotWrittenKind.RegionPick, true)]
        [InlineData(NotWrittenKind.AreaConfirm, true)]
        [InlineData(NotWrittenKind.ScheduleCheck, true)]
        [InlineData(NotWrittenKind.FileInUse, true)]
        [InlineData(NotWrittenKind.WriteFailed, true)]
        [InlineData(NotWrittenKind.FolderRefused, true)]
        [InlineData(NotWrittenKind.FormulaError, true)]
        [InlineData(NotWrittenKind.WouldOverwriteTheTemplate, true)]
        [InlineData(NotWrittenKind.Crashed, true)]
        public void EachKindSaysWhetherARetryCanWriteIt(NotWrittenKind kind, bool retryable)
        {
            Assert.Equal(retryable, FailedReasons.Retryable(kind));
        }

        /// <summary>
        /// The theory above names every kind. A kind added later goes red here until it is
        /// given its answer there.
        /// </summary>
        [Fact]
        public void EveryKindIsAnsweredForRetry()
        {
            Assert.Equal(19, Enum.GetValues(typeof(NotWrittenKind)).Length);
        }

        [Fact]
        public void RetryTicksOnlyWhatARetryCanWriteAndSaysWhatItLeftOut()
        {
            PressResult result = PressResult.Of(Press(false, null,
                PlotOutcome.WroteNothing("HF-01", KpiTemplates.Healthcare, Where("ANH-007-HF-100001"), true, "x", NotWrittenKind.FileInUse),
                PlotOutcome.WroteNothing("HF-02", KpiTemplates.Healthcare, Where("ANH-007-HF-100002"), false, "x", NotWrittenKind.SharedFolder),
                PlotOutcome.WroteNothing("HF-03", KpiTemplates.Healthcare, Where("ANH-007-HF-100003"), false, "x", NotWrittenKind.NoUid2)));

            Assert.Equal(new[] { "HF-01" }, result.Retryable.ToArray());
            Assert.Equal(2, result.NeedAChange);
            Assert.Equal("2 plots need a change first", result.StatusLine);

            PressResult none = PressResult.Of(Press(false, null,
                PlotOutcome.WroteNothing("HF-02", KpiTemplates.Healthcare, Where("ANH-007-HF-100002"), false, "x", NotWrittenKind.SharedFolder)));
            Assert.Empty(none.Retryable);
            Assert.Equal("1 plot needs a change first", none.StatusLine);

            PressResult clean = PressResult.Of(Press(false, null,
                PlotOutcome.WroteNothing("HF-01", KpiTemplates.Healthcare, Where("ANH-007-HF-100001"), true, "x", NotWrittenKind.FileInUse)));
            Assert.Equal("0 created, 1 failed", clean.StatusLine);
        }

        // ------------------------------------------------------------ pass 2, item 6, step 3's own reason

        [Fact]
        public void StepThreeNamesOnlyWhatStepThreeLacks()
        {
            // The output folder is not set and step 3 still names the tick it lacks, never the
            // folder, which is step 1's to say.
            KpiSteps noAssetType = KpiSteps.Of(true, false, false, false, false, true, 154, 0, 154, false);
            Assert.Equal("Output folder not set", noAssetType.CreateBlockedBy);
            Assert.Equal("No asset type selected", noAssetType.PlotsLack);

            Assert.Equal("No plot selected", KpiSteps.Of(true, true, false, false, false, true, 154, 1, 0, false).PlotsLack);
            Assert.Equal(string.Empty, KpiSteps.Of(true, false, false, false, false, true, 154, 1, 154, false).PlotsLack);
        }

        // ------------------------------------------------------------ pass 2, item 7, one rule each

        [Fact]
        public void TheTemplatesFolderComparisonIsOneRule()
        {
            Assert.True(PaneWarnings.OutputIsTemplates("C:\\KPI\\templates", "C:\\KPI\\templates"));
            Assert.False(PaneWarnings.OutputIsTemplates("C:\\KPI\\out", "C:\\KPI\\templates"));
            Assert.False(PaneWarnings.OutputIsTemplates(string.Empty, string.Empty));

            Assert.Equal("Output folder is the templates folder",
                PaneWarnings.ForOutputFolder("C:\\KPI\\templates", "C:\\KPI\\templates").Title);
            Assert.Null(PaneWarnings.ForOutputFolder("C:\\KPI\\out", "C:\\KPI\\templates"));
        }

        [Fact]
        public void TheStreetFileSentenceIsOneRule()
        {
            Assert.Equal(KpiSteps.StreetReferenceNotSet + ".", KpiPaneWords.StreetReferenceSays(string.Empty, null));

            StreetReferenceFile refused = StreetReferenceFile.Refused("C:\\Scope.xlsx", "it is not a workbook");
            Assert.Equal(refused.InWords, KpiPaneWords.StreetReferenceSays("C:\\Scope.xlsx", refused));
        }

        [Fact]
        public void ThePlotListSentenceIsOneRule()
        {
            Assert.Equal(KpiSteps.PlotListNotSet + ".", KpiPaneWords.PlotListSays(string.Empty, null));

            PlotListRead read = PlotListRead.Of("C:\\list.txt", new[] { "DM-11", "DM-12" });
            Assert.Equal("The plot list: " + read.InWords, KpiPaneWords.PlotListSays("C:\\list.txt", read));

            PlotListRead gone = PlotListRead.Refused("C:\\list.txt", "it is gone");
            Assert.Equal("The plot list: " + gone.Why, KpiPaneWords.PlotListSays("C:\\list.txt", gone));
        }

        // ------------------------------------------------------------ pass 2, item 8, the words

        [Fact]
        public void TheReadOnlyLineSaysTheFoldersAreMadeAndNeverDeleted()
        {
            Assert.Contains("a component folder and a UID2 folder it makes where they are missing", KpiPaneWords.ReadOnly);
            Assert.Contains("never deletes", KpiPaneWords.ReadOnly);
        }
    }
}
