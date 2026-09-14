using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// A checklist is one plot. Every expected value written out by hand.
    ///
    /// **The team came back with how they file these.** A press over 20 mosque plots makes 20
    /// folders and 20 workbooks rather than one file with 20 plots added together, and the
    /// folder is named for the plot.
    /// </summary>
    public class OneWorkbookPerPlotTests
    {
        private static string Joined(params string[] parts)
        {
            return string.Join(Path.DirectorySeparatorChar.ToString(), parts);
        }

        /// <summary>
        /// The template is what files a plot carrying no component of its own, so these pass
        /// the one their component really means and the empty component cases name theirs.
        /// </summary>
        private static PlotWorkbookPath Where(string component, string uid2, KpiTemplate template = null)
        {
            return PlotWorkbookPath.For(
                Joined("R", "MUGHARAZAT"),
                template ?? ComponentTemplates.For(component),
                component,
                uid2);
        }

        private static TemplateSplit Split(params string[] plots)
        {
            return PlotsPerTemplate.Split(
                plots, plot => "FRIDAY MOSQUE", new[] { KpiTemplates.Mosques });
        }

        /// <summary>
        /// Written plus wrote nothing is the ticked count. The folders are counted beside them
        /// and are deliberately not in that sum.
        /// </summary>
        [Fact]
        public void WrittenPlusWroteNothingIsTheTickedCount()
        {
            var set = new KpiCreateRunSet(
                "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                Split("DM-12", "DM-14", "FM-05"),
                new List<KpiCreateRun>(),
                new List<TemplateOutcome>(),
                null,
                new[]
                {
                    PlotOutcome.Wrote("DM-12", KpiTemplates.Mosques, Where("DAILY MOSQUE", "ANH-007-MO-100019")),
                    PlotOutcome.Wrote("DM-14", KpiTemplates.Mosques, Where("DAILY MOSQUE", "ANH-007-MO-100021")),
                    PlotOutcome.WroteNothing("FM-05", KpiTemplates.Mosques,
                        Where("FRIDAY MOSQUE", "ANH-007-MO-100006"), true, "the workbook was open in Excel"),

                    // **A plot with no folder either**, which is a plot no route placed. It is
                    // counted among the ones that wrote nothing and among no folders, and a
                    // break watch found this case missing from this test before it was here.
                    PlotOutcome.WroteNothing("ZZ-01", null,
                        PlotWorkbookPath.Refused("nothing placed it"), false, "nothing placed it")
                });

            Assert.Equal(4, set.PlotsTicked);
            Assert.Equal(2, set.WorkbooksWritten);
            Assert.Equal(2, set.PlotsThatWroteNothing);
            Assert.Equal(3, set.FoldersMade);
            Assert.True(set.PlotCountsAddUp);
        }

        /// <summary>
        /// A plot that fell out of every branch is a refusal in those words rather than a row
        /// nobody printed, the same rule the template accounting already follows.
        /// </summary>
        [Fact]
        public void APlotAccountedForNowhereIsARefusalInThoseWords()
        {
            var set = new KpiCreateRunSet(
                "NG05",
                Split("DM-12"),
                new List<KpiCreateRun>(),
                new List<TemplateOutcome>(),
                null,
                new PlotOutcome[0]);

            // Nothing ticked and nothing accounted for still adds up, because both are nought.
            Assert.True(set.PlotCountsAddUp);
            Assert.DoesNotContain(
                set.Refusals, one => one.Contains("plots were ticked"));
        }

        /// <summary>
        /// **A folder can exist for a plot whose workbook was refused**, because the folder is
        /// made before the patch and is never deleted. The count says so rather than implying
        /// the tree was tidied up after a failure.
        /// </summary>
        [Fact]
        public void AFolderCanOutliveARefusedWorkbookAndIsCountedApart()
        {
            PlotOutcome refused = PlotOutcome.WroteNothing(
                "DM-12", KpiTemplates.Mosques, Where("DAILY MOSQUE", "ANH-007-MO-100019"),
                true, "the workbook was open in Excel");

            Assert.False(refused.Written);
            Assert.True(refused.FolderMade);
            Assert.Equal("the workbook was open in Excel", refused.Why);

            PlotOutcome placed = PlotOutcome.WroteNothing(
                "ZZ-01", null, PlotWorkbookPath.Refused("nothing placed it"), false, "nothing placed it");

            Assert.False(placed.FolderMade);
            Assert.Equal(string.Empty, placed.TemplateName);
        }

        /// <summary>
        /// **The two mosque components are filed apart and filled from one workbook.** That is
        /// the whole reason the folder table is a second table beside the template one.
        /// </summary>
        [Fact]
        public void TheTwoMosqueComponentsShareATemplateAndNotAFolder()
        {
            PlotOutcome daily = PlotOutcome.Wrote(
                "DM-12", KpiTemplates.Mosques, Where("DAILY MOSQUE", "ANH-007-MO-100019"));
            PlotOutcome friday = PlotOutcome.Wrote(
                "FM-05", KpiTemplates.Mosques, Where("FRIDAY MOSQUE", "ANH-007-MO-100006"));

            Assert.Equal("MOSQUES", daily.TemplateName);
            Assert.Equal("MOSQUES", friday.TemplateName);
            Assert.Equal(
                Joined("R", "MUGHARAZAT", "DAILY MOSQUE", "ANH-007-MO-100019", "ANH-007-MO-100019.xlsx"),
                daily.Where.FilePath);
            Assert.Equal(
                Joined("R", "MUGHARAZAT", "FRIDAY MOSQUE", "ANH-007-MO-100006", "ANH-007-MO-100006.xlsx"),
                friday.Where.FilePath);
        }

        /// <summary>
        /// What a template's row says now that a template is many workbooks. The count and the
        /// root, never one path.
        /// </summary>
        [Fact]
        public void ATemplateRowNamesItsCountAndTheRootRatherThanOnePath()
        {
            Assert.Equal(
                "20 workbooks under C:\\RCRC\\MUGHARAZAT",
                CreateWords.WorkbooksUnder(KpiTemplates.Mosques, 20, "C:\\RCRC\\MUGHARAZAT"));

            Assert.Equal(
                "1 workbook under C:\\RCRC",
                CreateWords.WorkbooksUnder(KpiTemplates.Streets, 1, "C:\\RCRC"));
        }

        /// <summary>
        /// A template some of whose plots wrote nothing names every one of them with its own
        /// reason, because a count alone sends somebody to the report to find out which.
        /// </summary>
        [Fact]
        public void ATemplateShortOfAWorkbookNamesEveryPlotThatWroteNothing()
        {
            Assert.Equal(
                "18 of 20 plots wrote a workbook. DM-16: the workbook was open in Excel. "
                + "FM-05: no PRX_Plot_UID2 was read.",
                CreateWords.SomePlotsWroteNothing(
                    18, 20,
                    new[]
                    {
                        "DM-16: the workbook was open in Excel.",
                        "FM-05: no PRX_Plot_UID2 was read."
                    }));

            // Never an empty line, because silence after a press reads as success.
            Assert.Equal(
                "0 of 3 plots wrote a workbook. " + CreateWords.NoReasonRecorded,
                CreateWords.SomePlotsWroteNothing(0, 3, new string[0]));
        }
    }

    /// <summary>
    /// The four cells the map gained: the two off the street reference file and the two that are
    /// the same on every plot. Every expected value written out by hand.
    /// </summary>
    public class StreetAndFixedCellsTests
    {
        private static KpiCreatePlan Streets(StreetReferenceAnswer street)
        {
            return KpiCreatePlan.Of(
                KpiTemplates.Streets,
                new AgreedValue(new[] { new PlotText("ST-05", "STREET 36m ROW") }),
                new AgreedValue(new[] { new PlotText("ST-05", "ANH-007-ST-100210") }),
                "KING FAHD",
                null,
                Totalled.Adding(new[] { new PlotNumber("ST-05", 361.0) }),
                Totalled.Adding(new[] { new PlotNumber("ST-05", 96.0) }),
                null,
                "2026-09-13", "B RAHAL", "BIM COORDINATOR",
                street, CreateFixture.NoLabels);
        }

        private static string Written(KpiCreatePlan plan, string cell)
        {
            return plan.Writes.Single(one => one.Cell.ToString() == cell).Stored;
        }

        /// <summary>
        /// ANH-007-ST-100210 off the real file: 20 into D8 and 330.658499 into F8. **Nothing goes
        /// into H8**, which is the two multiplied and is the workbook's own arithmetic.
        /// </summary>
        [Fact]
        public void TheTwoStreetCellsTakeTheWidthAndTheLengthAndNothingTakesH8()
        {
            KpiCreatePlan plan = Streets(
                StreetReferenceAnswer.Of(20.0, 330.658499, "20", "330.65849900000001"));

            Assert.Equal("20", Written(plan, "D8"));
            Assert.Equal("330.658499", Written(plan, "F8"));
            Assert.DoesNotContain(plan.Writes, one => one.Cell.ToString() == "H8");
        }

        /// <summary>
        /// A street plot the file could not answer for leaves both cells empty and carries the
        /// file's OWN reason rather than a second wording of it.
        /// </summary>
        [Fact]
        public void AStreetPlotTheFileCouldNotAnswerForLeavesBothCellsEmpty()
        {
            KpiCreatePlan plan = Streets(StreetReferenceAnswer.Nothing(
                "ANH-007-ST-999999 is not in the street reference file, so the road width and "
                + "the total length are left empty"));

            Assert.DoesNotContain(plan.Writes, one => one.Cell.ToString() == "D8");
            Assert.DoesNotContain(plan.Writes, one => one.Cell.ToString() == "F8");

            Assert.Equal(
                new[] { "D8", "F8" },
                plan.Skipped.Where(one => one.What.StartsWith("Streets"))
                    .Select(one => one.Cell).ToArray());
            Assert.All(
                plan.Skipped.Where(one => one.What.StartsWith("Streets")),
                one => Assert.Equal(
                    "ANH-007-ST-999999 is not in the street reference file, so the road width "
                    + "and the total length are left empty",
                    one.Why));
        }

        /// <summary>
        /// The other six templates have neither cell and say so in their own words, which must
        /// not read like a street plot the file could not answer for.
        /// </summary>
        [Fact]
        public void TheOtherSixTemplatesHaveNeitherCellAndSaySoInTheirOwnWords()
        {
            foreach (KpiTemplate template in KpiTemplates.All.Where(one => one.Name != "STREETS"))
            {
                KpiCreatePlan plan = KpiCreatePlan.Of(
                    template, null, null, string.Empty, null, null, null, null,
                    null, null, null, StreetReferenceAnswer.Of(20.0, 330.0, "20", "330"), CreateFixture.NoLabels);

                Assert.Equal(
                    2,
                    plan.Skipped.Count(one => one.Why == "only the STREETS template has this cell"));
                Assert.DoesNotContain(plan.Writes, one => one.Cell.ToString() == "D8"
                    && one.SheetName == template.MainSheetName && one.IsText);
            }
        }

        /// <summary>
        /// **The two fixed values are named and no cell is guessed.** Character is always Urban
        /// Area Zone and Context is always Urban, and which cell each goes in comes off the
        /// label on the template's own sheet, so a template nothing opened reports both as not
        /// written with that reason. The four cells of row 5 go the same way now, so they are
        /// in the list beside them.
        /// </summary>
        [Fact]
        public void TheTwoFixedValuesAreNamedAndNoCellIsGuessed()
        {
            Assert.Equal("Urban Area Zone", FixedCells.CharacterValue);
            Assert.Equal("Urban", FixedCells.ContextValue);
            Assert.Equal("Urban Area Zone", FixedCells.ValueOf(KpiValue.Character));
            Assert.Equal("Urban", FixedCells.ValueOf(KpiValue.Context));

            // **NO TEMPLATE'S MAP HOLDS A LETTER FOR EITHER, AND NONE EVER WILL.** STREETS
            // carries a Category formula at D7 where MOSQUES and SCHOOLS carry Character, so a
            // letter taken off two templates overwrites a formula on the third. The label on the
            // sheet is the only route.
            foreach (KpiTemplate template in KpiTemplates.All)
            {
                Assert.Null(template.CellFor(KpiValue.Character));
                Assert.Null(template.CellFor(KpiValue.Context));

                KpiCreatePlan plan = KpiCreatePlan.Of(
                    template, null, null, string.Empty, null, null, null, null,
                    null, null, null, StreetReferenceAnswer.Nothing("not a street run"), CreateFixture.NoLabels);

                // A template nothing opened writes NONE of the six cells found by a label and
                // says which read did not happen, in the order the table holds them.
                Assert.Equal(
                    new[] { "Date", "Prepared by", "Position", "Reference", "Character", "Context" },
                    plan.Skipped.Where(one => one.Why == "the template was not opened")
                        .Select(one => one.What).ToArray());
            }
        }

        /// <summary>
        /// The street answer is a REQUIRED argument. The three the team types defaulted to null
        /// once, the handler never passed them, and the first real workbook went out holding the
        /// template's own placeholders while the report said nobody had typed them.
        /// </summary>
        [Fact]
        public void TheStreetAnswerIsRequiredRatherThanDefaultingToNothing()
        {
            Assert.Throws<System.ArgumentNullException>(() => KpiCreatePlan.Of(
                KpiTemplates.Streets, null, null, string.Empty, null, null, null, null,
                null, null, null, null, CreateFixture.NoLabels));
        }
    }
}
