using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    public class KpiQuestionsTests
    {
        private const string SiteLink = "RCRC_NG05_NU_00_SITE_RVT24.rvt";

        private const string ArchLink = "RCRC_NG05_NU_01_ARCH_RVT24.rvt";

        private static KpiAnswer Answer(KpiScan scan, int number)
        {
            return KpiQuestions.Answers(scan)[number - 1];
        }

        private static KpiScan WithInstances(int howMany, params ParameterTally[] tallies)
        {
            return KpiFixture.Build(titleBlocks: KpiFixture.TitleBlocks(
                sheetCount: howMany,
                titleBlockInstances: howMany,
                onInstances: KpiFixture.OnInstances(howMany, tallies)));
        }

        private static KpiScan WithSheets(int howMany, ParameterTally[] tallies, SheetValue[] values = null)
        {
            return KpiFixture.Build(titleBlocks: KpiFixture.TitleBlocks(
                sheetCount: howMany,
                onSheets: KpiFixture.OnSheets(howMany, tallies, values)));
        }

        [Fact]
        public void ThereAreAlwaysNineAnswersNumberedOneToNine()
        {
            foreach (KpiScan scan in new[] { KpiFixture.Build(), KpiFixture.RealShaped() })
            {
                IReadOnlyList<KpiAnswer> answers = KpiQuestions.Answers(scan);

                Assert.Equal(9, answers.Count);
                Assert.Equal(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, answers.Select(answer => answer.Number));
                Assert.Equal(KpiQuestions.Questions, answers.Select(answer => answer.Question));
            }
        }

        [Fact]
        public void TheQuestionsAreTheNineTheBriefAsks()
        {
            IReadOnlyList<KpiAnswer> answers = KpiQuestions.Answers(KpiFixture.Build());

            Assert.Equal("Which sheet holds the title block carrying PRX_COMPONENT and PRX_Plot_UID2", answers[0].Question);
            Assert.Equal("What is PRX_Plot_UID2", answers[2].Question);
            Assert.Equal("What unit does each area come back in, raw and as printed", answers[8].Question);
        }

        [Fact]
        public void ANullScanIsRefused()
        {
            Assert.Throws<ArgumentNullException>(() => KpiQuestions.Answers(null));
        }

        [Fact]
        public void QuestionOneIsAnsweredWhenBothNamesAreOnOneTitleBlockInstance()
        {
            KpiScan scan = KpiFixture.Build(titleBlocks: KpiFixture.TitleBlocks(
                sheetCount: 1383,
                titleBlockInstances: 1383,
                onInstances: KpiFixture.OnInstances(1383,
                    new[]
                    {
                        KpiFixture.Tally("PRX_COMPONENT", 1383, 1201),
                        KpiFixture.Tally("PRX_Plot_UID2", 1383, 160)
                    },
                    new[]
                    {
                        KpiFixture.Value("PRX_COMPONENT", "010QE Copy 001", "Copy of key plan", ""),
                        KpiFixture.Value("PRX_COMPONENT", "DM-11-600QD", "SOFTSCAPE SCHEDULES", "SOFTSCAPE"),
                        KpiFixture.Value("PRX_Plot_UID2", "DM-11-600QD", "SOFTSCAPE SCHEDULES", "DM-11"),
                        KpiFixture.Value("PRX_Plot_UID2", "010QE Copy 001", "Copy of key plan", "")
                    })));

            KpiAnswer answer = Answer(scan, 1);

            Assert.True(answer.Answered);
            Assert.Equal(
                "PRX_COMPONENT on 1383 title block instances, 1201 with a value. PRX_Plot_UID2 on 1383 title block "
                + "instances, 160 with a value. Both on 2 sheets, the first DM-11-600QD SOFTSCAPE SCHEDULES. "
                + "Section 3 lists up to twenty sheets for each.",
                answer.Answer);
        }

        /// <summary>
        /// Two families each carrying one of the names satisfy both tallies and answer nothing,
        /// because the question asks for the sheet whose title block carries both.
        /// </summary>
        [Fact]
        public void QuestionOneIsNotAnsweredWhenTheTwoNamesSitOnDifferentSheets()
        {
            KpiScan scan = KpiFixture.Build(titleBlocks: KpiFixture.TitleBlocks(
                sheetCount: 2,
                titleBlockInstances: 2,
                onInstances: KpiFixture.OnInstances(2,
                    new[]
                    {
                        KpiFixture.Tally("PRX_COMPONENT", 1, 1),
                        KpiFixture.Tally("PRX_Plot_UID2", 1, 1)
                    },
                    new[]
                    {
                        KpiFixture.Value("PRX_COMPONENT", "S1", "one", "HARDSCAPE"),
                        KpiFixture.Value("PRX_Plot_UID2", "S2", "two", "DM-11")
                    })));

            KpiAnswer answer = Answer(scan, 1);

            Assert.False(answer.Answered);
            Assert.EndsWith("No title block instance carries both names on one sheet. Section 3 lists each on its own.", answer.Answer);
        }

        [Fact]
        public void QuestionThreeDoesNotCountAWhitespaceValue()
        {
            KpiScan scan = KpiFixture.Build(titleBlocks: KpiFixture.TitleBlocks(
                sheetCount: 1,
                titleBlockInstances: 1,
                onInstances: KpiFixture.OnInstances(1,
                    new[] { KpiFixture.Tally("PRX_Plot_UID2", 1, 1) },
                    new[] { KpiFixture.Value("PRX_Plot_UID2", "S1", "one", " ") })));

            KpiAnswer answer = Answer(scan, 3);

            Assert.False(answer.Answered);
            Assert.Contains("0 values, 0 of them shaped like a plot identifier", answer.Answer);
        }

        /// <summary>
        /// A section whose read threw is not a measured absence. Every question that draws on
        /// it says NOT READ and points at the top of the file.
        /// </summary>
        [Fact]
        public void EveryQuestionOnASectionThatWasNotReadSaysNotReadAndNeverNotFound()
        {
            KpiScan scan = new KpiScan(
                new DocumentFacts("NG05", "", 0, 0, 0.0, null, null),
                null,
                TitleBlockFacts.NotRead("refused"),
                LinkFacts.NotRead("refused"),
                ScheduleFacts.NotRead("refused"),
                null,
                false);

            IReadOnlyList<KpiAnswer> answers = KpiQuestions.Answers(scan);

            Assert.All(answers, answer => Assert.False(answer.Answered));
            Assert.All(answers, answer => Assert.StartsWith("NOT READ. Section ", answer.Answer));
            Assert.All(answers, answer => Assert.DoesNotContain("NOT FOUND", answer.Answer));
            Assert.Equal("NOT READ. Section 2 PROJECT INFORMATION was not read, see READS THAT DID NOT HAPPEN at the top.", answers[3].Answer);
            Assert.Equal("NOT READ. Section 4 LINKED MODELS was not read, see READS THAT DID NOT HAPPEN at the top.", answers[4].Answer);
        }

        [Fact]
        public void QuestionOneCountsOneInstanceWithoutAnS()
        {
            KpiAnswer answer = Answer(WithInstances(1,
                KpiFixture.Tally("PRX_COMPONENT", 1, 1),
                KpiFixture.Tally("PRX_Plot_UID2", 1, 0)), 1);

            Assert.Equal(
                "PRX_COMPONENT on 1 title block instance, 1 with a value. PRX_Plot_UID2 on 1 title block instance, "
                + "0 with a value. No title block instance carries both names on one sheet. Section 3 lists each on its own.",
                answer.Answer);
        }

        [Fact]
        public void QuestionOneIsNotAnsweredWhenOnlyOneNameIsOnTheInstance()
        {
            KpiAnswer answer = Answer(KpiFixture.RealShaped(), 1);

            Assert.False(answer.Answered);
            Assert.Equal(
                "PRX_COMPONENT on 1383 title block instances, 1201 with a value. PRX_Plot_UID2 NOT FOUND on any "
                + "title block instance. Section 3 lists the near misses.",
                answer.Answer);
        }

        [Fact]
        public void QuestionOneIsNotAnsweredByNamesThatSitOnTheSheetInstead()
        {
            KpiAnswer answer = Answer(WithSheets(5, new[]
            {
                KpiFixture.Tally("PRX_COMPONENT", 5, 5),
                KpiFixture.Tally("PRX_Plot_UID2", 5, 5)
            }), 1);

            Assert.False(answer.Answered);
            Assert.Equal(
                "PRX_COMPONENT NOT FOUND on any title block instance. PRX_Plot_UID2 NOT FOUND on any title block "
                + "instance. Section 3 lists the near misses.",
                answer.Answer);
        }

        [Fact]
        public void QuestionTwoSaysWhereComponentIsAndWhereItIsNot()
        {
            KpiAnswer answer = Answer(KpiFixture.RealShaped(), 2);

            Assert.True(answer.Answered);
            Assert.Equal(
                "PRX_COMPONENT is on the title block instance, 1383 of 1383 title block instances carry it and 1201 "
                + "hold a value, not on the title block type, not on the sheet.",
                answer.Answer);
        }

        [Fact]
        public void QuestionTwoFindsComponentOnTheSheetItself()
        {
            KpiAnswer answer = Answer(WithSheets(1385, new[] { KpiFixture.Tally("PRX_COMPONENT", 1385, 900) }), 2);

            Assert.True(answer.Answered);
            Assert.Equal(
                "PRX_COMPONENT is not on the title block instance, not on the title block type, on the sheet, "
                + "1385 of 1385 sheets carry it and 900 hold a value.",
                answer.Answer);
        }

        [Fact]
        public void QuestionTwoIsNotAnsweredWhenComponentIsNowhere()
        {
            KpiAnswer answer = Answer(KpiFixture.Build(), 2);

            Assert.False(answer.Answered);
            Assert.Equal(
                "PRX_COMPONENT is not on the title block instance, not on the title block type, not on the sheet.",
                answer.Answer);
        }

        [Fact]
        public void QuestionThreeCountsTheValuesShapedLikeAPlotIdentifier()
        {
            KpiAnswer answer = Answer(WithSheets(3,
                new[] { KpiFixture.Tally("PRX_Plot_UID2", 3, 2) },
                new[]
                {
                    KpiFixture.Value("PRX_Plot_UID2", "001", "A", "DM-41"),
                    KpiFixture.Value("PRX_Plot_UID2", "002", "B", "010QE Copy 001"),
                    KpiFixture.Value("PRX_Plot_UID2", "003", "C", "")
                }), 3);

            Assert.True(answer.Answered);
            Assert.Equal(
                "PRX_Plot_UID2 is on the sheet for 3 of 3 sheets. 2 values, 1 of them shaped like a plot identifier "
                + "such as DM-41. First values: DM-41, 010QE Copy 001.",
                answer.Answer);
        }

        [Fact]
        public void QuestionThreeAddsUpValuesFromEveryHomeAndNamesEachSampleOnce()
        {
            KpiScan scan = KpiFixture.Build(titleBlocks: KpiFixture.TitleBlocks(
                sheetCount: 2,
                titleBlockInstances: 2,
                onInstances: KpiFixture.OnInstances(2,
                    new[] { KpiFixture.Tally("PRX_Plot_UID2", 2, 1) },
                    new[] { KpiFixture.Value("PRX_Plot_UID2", "001", "A", "DM-41") }),
                onSheets: KpiFixture.OnSheets(2,
                    new[] { KpiFixture.Tally("PRX_Plot_UID2", 2, 2) },
                    new[]
                    {
                        KpiFixture.Value("PRX_Plot_UID2", "001", "A", "DM-41"),
                        KpiFixture.Value("PRX_Plot_UID2", "002", "B", "PF-12")
                    })));

            KpiAnswer answer = Answer(scan, 3);

            Assert.True(answer.Answered);
            Assert.Equal(
                "PRX_Plot_UID2 is on the title block instance for 2 of 2 title block instances, on the sheet for 2 of "
                + "2 sheets. 3 values, 3 of them shaped like a plot identifier such as DM-41. First values: DM-41, PF-12.",
                answer.Answer);
        }

        [Fact]
        public void QuestionThreeIsNotAnsweredWhenTheNameIsCarriedButNeverFilledIn()
        {
            KpiAnswer answer = Answer(WithInstances(4, KpiFixture.Tally("PRX_Plot_UID2", 4, 0)), 3);

            Assert.False(answer.Answered);
            Assert.Equal(
                "PRX_Plot_UID2 is on the title block instance for 4 of 4 title block instances. 0 values, 0 of them "
                + "shaped like a plot identifier such as DM-41.",
                answer.Answer);
        }

        [Fact]
        public void QuestionThreeIsNotAnsweredWhenTheNameIsAbsentEverywhere()
        {
            KpiAnswer answer = Answer(KpiFixture.RealShaped(), 3);

            Assert.False(answer.Answered);
            Assert.Equal(
                "PRX_Plot_UID2 NOT FOUND on a title block instance, a title block type or a sheet. It is neither "
                + "PRX_Plot_ID nor PRX_Ref Plot ID and the model does not hold it under this name. Section 3 lists "
                + "the near misses.",
                answer.Answer);
        }

        [Fact]
        public void QuestionFourNamesEveryProjectParameterHoldingANeighbourhoodWordWithItsValue()
        {
            KpiScan scan = KpiFixture.Build(projectInformation: new[]
            {
                KpiFixture.Parameter("PRX_Zone", "Z3"),
                KpiFixture.Parameter("Project Name", "NG05"),
                KpiFixture.Parameter("PRX_Neighbourhood", "NU05")
            });

            KpiAnswer answer = Answer(scan, 4);

            Assert.True(answer.Answered);
            Assert.Equal(
                "2 names in Project Information hold NEIGH, DISTRICT, COMMUNITY, LOCATION or ZONE: "
                + "PRX_Neighbourhood (shared) holds NU05; PRX_Zone (shared) holds Z3.",
                answer.Answer);
        }

        [Fact]
        public void QuestionFourSaysHoldsForOneName()
        {
            KpiAnswer answer = Answer(KpiFixture.RealShaped(), 4);

            Assert.True(answer.Answered);
            Assert.Equal(
                "1 name in Project Information holds NEIGH, DISTRICT, COMMUNITY, LOCATION or ZONE: "
                + "PRX_Neighbourhood (shared) holds NU05.",
                answer.Answer);
        }

        [Fact]
        public void QuestionFourShowsAnEmptyValueAsEmpty()
        {
            KpiScan scan = KpiFixture.Build(projectInformation: new[] { KpiFixture.Parameter("PRX_District", "") });

            Assert.Equal(
                "1 name in Project Information holds NEIGH, DISTRICT, COMMUNITY, LOCATION or ZONE: "
                + "PRX_District (shared) holds (empty).",
                Answer(scan, 4).Answer);
        }

        [Fact]
        public void QuestionFourIsNotAnsweredWhenNoNameHoldsAWord()
        {
            KpiScan scan = KpiFixture.Build(projectInformation: new[]
            {
                KpiFixture.Parameter("Project Name", "NG05"),
                KpiFixture.Parameter("Project Number", "05")
            });

            KpiAnswer answer = Answer(scan, 4);

            Assert.False(answer.Answered);
            Assert.Equal(
                "No Project Information parameter name holds NEIGH, DISTRICT, COMMUNITY, LOCATION or ZONE. "
                + "All 2 names are in section 2 for the team to pick from.",
                answer.Answer);
        }

        [Fact]
        public void QuestionFiveIsNotAnsweredWhenNoLinkNameHoldsTheMark()
        {
            KpiScan scan = KpiFixture.Build(links: KpiFixture.Links(
                new[] { KpiFixture.LinkType(ArchLink) },
                new[] { KpiFixture.LinkInstance(ArchLink + " : 1", ArchLink) }));

            KpiAnswer answer = Answer(scan, 5);

            Assert.False(answer.Answered);
            Assert.Equal("No link name holds 00 among 1 link type and 1 instance, all named in section 4.", answer.Answer);
        }

        [Fact]
        public void QuestionFiveCountsNoLinksAtAllInThePlural()
        {
            Assert.Equal(
                "No link name holds 00 among 0 link types and 0 instances, all named in section 4.",
                Answer(KpiFixture.Build(), 5).Answer);
        }

        [Fact]
        public void QuestionFiveIsNotAnsweredWhenTheMarkedLinkIsNotLoaded()
        {
            KpiScan scan = KpiFixture.Build(links: KpiFixture.Links(
                new[] { KpiFixture.LinkType(SiteLink, "Unloaded", isLoaded: false) }));

            KpiAnswer answer = Answer(scan, 5);

            Assert.False(answer.Answered);
            Assert.Equal(
                "1 link name holds 00: RCRC_NG05_NU_00_SITE_RVT24.rvt. None is loaded, so no filled region "
                + "was read. Load the link and scan again.",
                answer.Answer);
        }

        [Fact]
        public void QuestionFiveSaysWhenTheMarkedTypeIsLoadedAndNoInstanceHandedBackADocument()
        {
            KpiScan scan = KpiFixture.Build(links: KpiFixture.Links(
                new[] { KpiFixture.LinkType(SiteLink, "Loaded", isLoaded: true) },
                new[] { KpiFixture.LinkInstance(SiteLink + " : 1", SiteLink, isLoaded: false) }));

            KpiAnswer answer = Answer(scan, 5);

            Assert.False(answer.Answered);
            Assert.Equal(
                "2 link names hold 00: RCRC_NG05_NU_00_SITE_RVT24.rvt, RCRC_NG05_NU_00_SITE_RVT24.rvt : 1. 1 link "
                + "type reads Loaded and 0 of 1 placed instance handed back a document, so no filled region was "
                + "read. Place an instance and scan again.",
                answer.Answer);
        }

        /// <summary>
        /// Solid Fill is the filled region type every Revit template ships with and it holds
        /// the letters ID. So does Grid. Neither is the word, and naming Solid Fill first on
        /// the question 5 line would send the reader to the wrong region type.
        /// </summary>
        [Fact]
        public void QuestionFiveDoesNotTakeSolidFillOrGridForTheWordId()
        {
            KpiScan scan = KpiFixture.Build(links: KpiFixture.Links(
                new[] { KpiFixture.LinkType(SiteLink) },
                new[] { KpiFixture.LinkInstance(SiteLink + " : 1", SiteLink) },
                new[]
                {
                    KpiFixture.Contents(SiteLink + " : 1", "RCRC_NG05_NU_00_SITE_RVT24", 3,
                        typeCounts: new[] { KpiFixture.Counted("Solid Fill", 3) },
                        viewCounts: new[] { KpiFixture.Counted("Grid Plan", 2), KpiFixture.Counted("Hidden Line Export", 1) },
                        regionParameters: new[] { KpiFixture.Tally("PRX_Intervention Area", 3, 3) })
                }));

            KpiAnswer answer = Answer(scan, 5);

            Assert.Contains(", no region type name holds ID, no view name holds ID, ", answer.Answer);
        }

        [Fact]
        public void QuestionFiveStillFindsIdWhenItIsAWordOfItsOwn()
        {
            KpiScan scan = KpiFixture.Build(links: KpiFixture.Links(
                new[] { KpiFixture.LinkType(SiteLink) },
                new[] { KpiFixture.LinkInstance(SiteLink + " : 1", SiteLink) },
                new[]
                {
                    KpiFixture.Contents(SiteLink + " : 1", "RCRC_NG05_NU_00_SITE_RVT24", 3,
                        typeCounts: new[] { KpiFixture.Counted("ID Intervention Area", 2), KpiFixture.Counted("Solid Fill", 1) },
                        viewCounts: new[] { KpiFixture.Counted("00 ID PLAN", 3) },
                        regionParameters: new[] { KpiFixture.Tally("PRX_Intervention Area", 3, 3) })
                }));

            KpiAnswer answer = Answer(scan, 5);

            Assert.Contains(", region types holding ID: ID Intervention Area, views holding ID: 00 ID PLAN, ", answer.Answer);
        }

        [Fact]
        public void QuestionFiveIsAnsweredWhenTheLoadedLinkHoldsFilledRegionsCarryingInterventionArea()
        {
            KpiAnswer answer = Answer(KpiFixture.RealShaped(), 5);

            Assert.True(answer.Answered);
            Assert.Equal(
                "2 link names hold 00: RCRC_NG05_NU_00_SITE_RVT24.rvt, RCRC_NG05_NU_00_SITE_RVT24.rvt : 1. "
                + "RCRC_NG05_NU_00_SITE_RVT24.rvt : 1 holds 412 filled regions, region types holding ID: "
                + "ID Intervention Area, no view name holds ID, PRX_Intervention Area on 412 with 398 holding a value. "
                + "Section 4 has the detail.",
                answer.Answer);
        }

        [Fact]
        public void QuestionFiveIsNotAnsweredWhenTheLoadedLinkHasNoInterventionArea()
        {
            KpiScan scan = KpiFixture.Build(links: KpiFixture.Links(
                new[] { KpiFixture.LinkType(SiteLink) },
                null,
                new[]
                {
                    KpiFixture.Contents(SiteLink + " : 1", filledRegionCount: 3,
                        regionParameters: new[] { KpiFixture.Tally("Area", 3, 3) })
                }));

            KpiAnswer answer = Answer(scan, 5);

            Assert.False(answer.Answered);
            Assert.Equal(
                "1 link name holds 00: RCRC_NG05_NU_00_SITE_RVT24.rvt. RCRC_NG05_NU_00_SITE_RVT24.rvt : 1 holds "
                + "3 filled regions, no region type name holds ID, no view name holds ID, PRX_Intervention Area "
                + "NOT FOUND. Section 4 has the detail.",
                answer.Answer);
        }

        [Fact]
        public void QuestionSixListsTheNamesPerWorkbookWordWithThePlotTakenOff()
        {
            KpiAnswer answer = Answer(KpiFixture.RealShaped(), 6);

            Assert.True(answer.Answered);
            Assert.Equal(
                "7 schedules, 4 distinct once the plot is taken off, all in section 5. Names holding each workbook "
                + "word, plot taken off. SOFTSCAPE: (610) SOFTSCAPE SCHEDULE. SHRUB: (620) SHRUBS AND LAWN SCHEDULE. "
                + "LAWN: (620) SHRUBS AND LAWN SCHEDULE. HARDSCAPE: (600) HARDSCAPE SCHEDULE. TREE: none.",
                answer.Answer);
        }

        [Fact]
        public void QuestionSixIsNotAnsweredWhenThereIsNoSchedule()
        {
            KpiAnswer answer = Answer(KpiFixture.Build(), 6);

            Assert.False(answer.Answered);
            Assert.Equal("No schedule in the model.", answer.Answer);
        }

        [Fact]
        public void QuestionSixIsNotAnsweredWhenNoScheduleHoldsAWorkbookWord()
        {
            KpiScan scan = KpiFixture.Build(schedules: KpiFixture.Schedules(
                new[] { KpiFixture.Schedule("Sheet List", "Sheets") }));

            KpiAnswer answer = Answer(scan, 6);

            Assert.False(answer.Answered);
            Assert.Equal(
                "1 schedule, 1 distinct once the plot is taken off, all in section 5. Names holding each workbook "
                + "word, plot taken off. SOFTSCAPE: none. SHRUB: none. LAWN: none. HARDSCAPE: none. TREE: none.",
                answer.Answer);
        }

        [Fact]
        public void QuestionSevenNamesTheCountFieldAndTheBotanicalParameters()
        {
            KpiAnswer answer = Answer(KpiFixture.RealShaped(), 7);

            Assert.True(answer.Answered);
            Assert.Equal(
                "DM-11-(610) SOFTSCAPE SCHEDULE has 5 fields, headed BOTANICAL NAME | COMMON NAME | SIZE | "
                + "ENTER EACH PROPOSED TREE QUANTITY | PRX_Ref Plot ID. Count fields: ENTER EACH PROPOSED TREE "
                + "QUANTITY. Parameters on its elements holding BOTANIC, LATIN or SPECIES: PRX_Botanical Name. "
                + "Section 6 has the rows as printed and every value.",
                answer.Answer);
        }

        [Fact]
        public void QuestionSevenSaysWhenThereIsNoCountFieldAndNoBotanicalParameter()
        {
            KpiScan scan = KpiFixture.Build(schedules: KpiFixture.Schedules(new[]
            {
                KpiFixture.Schedule("DM-11-(610) SOFTSCAPE SCHEDULE", rowsWereRead: true,
                    fields: new[] { KpiFixture.Field("BOTANICAL NAME"), KpiFixture.Field("QTY", "PRX_Quantity") })
            }));

            KpiAnswer answer = Answer(scan, 7);

            // Rows were read and nothing in them says which field is the name or the quantity,
            // so the question is not answered and the line says what is there without deciding.
            Assert.False(answer.Answered);
            Assert.Equal(
                "DM-11-(610) SOFTSCAPE SCHEDULE has 2 fields, headed BOTANICAL NAME | QTY. No Count field. No "
                + "parameter on its elements holds BOTANIC, LATIN or SPECIES. Section 6 has the rows as printed and "
                + "every value.",
                answer.Answer);
        }

        /// <summary>
        /// The reader reads one copy per name, so two names holding SOFTSCAPE both arrive. A
        /// line describing only the first hid a second schedule with a SPECIES field.
        /// </summary>
        [Fact]
        public void QuestionSevenNamesEverySoftscapeScheduleReadInFull()
        {
            KpiScan scan = KpiFixture.Build(schedules: KpiFixture.Schedules(
                new[]
                {
                    KpiFixture.Schedule("DM-11-(610) SOFTSCAPE SCHEDULE", rowsWereRead: true,
                        fields: new[] { KpiFixture.Field("BOTANICAL NAME") }),
                    KpiFixture.Schedule("DM-11-(611) SOFTSCAPE TREES", rowsWereRead: true,
                        fields: new[] { KpiFixture.Field("SPECIES", "PRX_Species") })
                },
                elements: new[]
                {
                    KpiFixture.Elements("DM-11-(611) SOFTSCAPE TREES",
                        instanceParameters: new[] { KpiFixture.Tally("PRX_Species", 4, 4) })
                }));

            KpiAnswer answer = Answer(scan, 7);

            Assert.True(answer.Answered);
            Assert.Equal(
                "DM-11-(610) SOFTSCAPE SCHEDULE has 1 field, headed BOTANICAL NAME. No Count field. No parameter on "
                + "its elements holds BOTANIC, LATIN or SPECIES. DM-11-(611) SOFTSCAPE TREES has 1 field, headed "
                + "SPECIES. No Count field. Parameters on its elements holding BOTANIC, LATIN or SPECIES: "
                + "PRX_Species. Section 6 has the rows as printed and every value.",
                answer.Answer);
        }

        [Fact]
        public void QuestionSevenIsNotAnsweredWhenTheSoftscapeRowsWereNotRead()
        {
            KpiScan scan = KpiFixture.Build(schedules: KpiFixture.Schedules(
                new[] { KpiFixture.Schedule("DM-11-(610) SOFTSCAPE SCHEDULE") }));

            KpiAnswer answer = Answer(scan, 7);

            Assert.False(answer.Answered);
            Assert.Equal(
                "A schedule named for SOFTSCAPE exists and none was read in full. Section 5 has the names.",
                answer.Answer);
        }

        [Fact]
        public void QuestionSevenIsNotAnsweredWhenNoScheduleIsNamedForSoftscape()
        {
            KpiScan scan = KpiFixture.Build(schedules: KpiFixture.Schedules(
                new[] { KpiFixture.Schedule("Sheet List", "Sheets") }));

            KpiAnswer answer = Answer(scan, 7);

            Assert.False(answer.Answered);
            Assert.Equal(
                "No schedule name holds SOFTSCAPE, so no field could be read. Section 5 lists every name.",
                answer.Answer);
        }

        [Fact]
        public void QuestionEightGathersPhasesStatusParametersAndHeadings()
        {
            KpiAnswer answer = Answer(KpiFixture.RealShaped(), 8);

            Assert.True(answer.Answered);
            Assert.Equal(
                "2 phases: Existing, Proposed. DM-11-(610) SOFTSCAPE SCHEDULE is on phase New Construction with "
                + "phase filter Show All. its elements by phase created: Existing 12, Proposed 45. parameters on them "
                + "holding EXIST, PROPOS, STATUS, RETAIN, REMOV, NEW, PHASE or CONDITION: Phase Created. column "
                + "headings holding EXISTING or PROPOSED: ENTER EACH PROPOSED TREE QUANTITY. Section 7 has the counts.",
                answer.Answer);
        }

        [Fact]
        public void QuestionEightIsAnsweredWhenTheElementsSplitAcrossMoreThanOnePhaseCreated()
        {
            KpiScan scan = KpiFixture.Build(schedules: KpiFixture.Schedules(
                new[]
                {
                    KpiFixture.Schedule(KpiFixture.Softscape, rowsWereRead: true,
                        fields: new[] { KpiFixture.Field("BOTANICAL NAME") })
                },
                elements: new[]
                {
                    KpiFixture.Elements(KpiFixture.Softscape,
                        createdPhases: new[] { KpiFixture.Counted("Existing", 1), KpiFixture.Counted("Proposed", 2) })
                }));

            KpiAnswer answer = Answer(scan, 8);

            Assert.True(answer.Answered);
            Assert.Equal(
                "No phase read. DM-11-(610) SOFTSCAPE SCHEDULE is on phase (empty) with phase filter (empty). its "
                + "elements by phase created: Existing 1, Proposed 2. no parameter on them holds EXIST, PROPOS, STATUS, "
                + "RETAIN, REMOV, NEW, PHASE or CONDITION. Section 7 has the counts.",
                answer.Answer);
        }

        [Fact]
        public void QuestionEightIsAnsweredByAParameterHoldingAStatusWord()
        {
            KpiScan scan = KpiFixture.Build(schedules: KpiFixture.Schedules(
                new[]
                {
                    KpiFixture.Schedule(KpiFixture.Softscape, rowsWereRead: true,
                        fields: new[] { KpiFixture.Field("BOTANICAL NAME") })
                },
                elements: new[]
                {
                    KpiFixture.Elements(KpiFixture.Softscape,
                        instanceParameters: new[] { KpiFixture.Tally("PRX_Status", 3, 3) },
                        createdPhases: new[] { KpiFixture.Counted("Proposed", 3) })
                }));

            KpiAnswer answer = Answer(scan, 8);

            Assert.True(answer.Answered);
            Assert.Equal(
                "No phase read. DM-11-(610) SOFTSCAPE SCHEDULE is on phase (empty) with phase filter (empty). its "
                + "elements by phase created: Proposed 3. parameters on them holding EXIST, PROPOS, STATUS, RETAIN, "
                + "REMOV, NEW, PHASE or CONDITION: PRX_Status. Section 7 has the counts.",
                answer.Answer);
        }

        [Fact]
        public void QuestionEightIsAnsweredByAColumnHeadingHoldingExisting()
        {
            KpiScan scan = KpiFixture.Build(schedules: KpiFixture.Schedules(new[]
            {
                KpiFixture.Schedule("DM-11-(630) TREE SURVEY", fields: new[] { KpiFixture.Field("EXISTING TREES") })
            }));

            KpiAnswer answer = Answer(scan, 8);

            Assert.True(answer.Answered);
            Assert.Equal(
                "No phase read. column headings holding EXISTING or PROPOSED: EXISTING TREES. Section 7 has the counts.",
                answer.Answer);
        }

        [Fact]
        public void QuestionEightIsNotAnsweredByOnePhaseAndNoStatusWord()
        {
            KpiScan scan = KpiFixture.Build(schedules: KpiFixture.Schedules(
                new[]
                {
                    KpiFixture.Schedule(KpiFixture.Softscape, rowsWereRead: true,
                        fields: new[] { KpiFixture.Field("BOTANICAL NAME") })
                },
                phases: new[] { "Proposed" },
                elements: new[]
                {
                    KpiFixture.Elements(KpiFixture.Softscape,
                        instanceParameters: new[]
                        {
                            KpiFixture.Tally("Phase Created", 3, 3),
                            KpiFixture.Tally("Phase Demolished", 3, 0)
                        },
                        createdPhases: new[] { KpiFixture.Counted("Proposed", 3) })
                }));

            KpiAnswer answer = Answer(scan, 8);

            // Phase Created and Phase Demolished are on every phased element, so they are
            // printed and they separate nothing.
            Assert.False(answer.Answered);
            Assert.Equal(
                "1 phase: Proposed. DM-11-(610) SOFTSCAPE SCHEDULE is on phase (empty) with phase filter (empty). its "
                + "elements by phase created: Proposed 3. parameters on them holding EXIST, PROPOS, STATUS, RETAIN, "
                + "REMOV, NEW, PHASE or CONDITION: Phase Created, Phase Demolished. Section 7 has the counts.",
                answer.Answer);
        }

        [Fact]
        public void QuestionEightIsNotAnsweredByAnEmptyScan()
        {
            KpiAnswer answer = Answer(KpiFixture.Build(), 8);

            Assert.False(answer.Answered);
            Assert.Equal("No phase read. Section 7 has the counts.", answer.Answer);
        }

        [Fact]
        public void QuestionNineIsAnsweredWhenTheUnitIsKnownAndAnAreaWasMeasured()
        {
            KpiAnswer answer = Answer(KpiFixture.RealShaped(), 9);

            Assert.True(answer.Answered);
            Assert.Equal(
                "Raw areas come back in square feet, which is what Revit holds whatever the project shows. Printed "
                + "areas come back in the project unit, Square meters, rounded to 0.01. 2 areas were measured both "
                + "ways in section 8.",
                answer.Answer);
        }

        [Fact]
        public void QuestionNineIsNotAnsweredWhenNothingWasMeasured()
        {
            KpiAnswer answer = Answer(KpiFixture.Build(area: KpiFixture.SquareMetres), 9);

            Assert.False(answer.Answered);
            Assert.Equal(
                "Raw areas come back in square feet, which is what Revit holds whatever the project shows. Printed "
                + "areas come back in the project unit, Square meters, rounded to 0.01. 0 areas were measured both "
                + "ways, so section 8 has nothing to show the difference on.",
                answer.Answer);
        }

        [Fact]
        public void QuestionNineIsNotAnsweredWhenTheUnitIsUnknown()
        {
            KpiScan scan = KpiFixture.Build(schedules: KpiFixture.Schedules(areas: new[]
            {
                new MeasuredArea("DM-11-(600) HARDSCAPE SCHEDULE", "Area", "Floors 1", 1000.0, "92.90 m²")
            }));

            KpiAnswer answer = Answer(scan, 9);

            Assert.False(answer.Answered);
            Assert.Equal(
                "Raw areas come back in square feet, which is what Revit holds whatever the project shows. Printed "
                + "areas come back in the project unit, UNKNOWN. 1 area was measured both ways in section 8.",
                answer.Answer);
        }
    }
}
