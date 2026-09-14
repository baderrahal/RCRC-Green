using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **A REASON THAT POINTS AT A SCREEN IS NOT A REASON.**
    ///
    /// The 14:29 run's report said this on 102 rows of 7,083 lines:
    ///
    /// <code>
    /// NS-02 | STREETS | STREETS | ANH-007-ST-100050 |
    ///   Nothing was written. 1 reason, shown in full above the Create button.
    /// </code>
    ///
    /// The plot, the template, the folder and the UID2 were all there, and then it pointed at a
    /// pane nobody has open. **The reason itself appeared NOWHERE in the file.** Two lines down
    /// a plot refused by its own path read its own words, which is what a record looks like.
    ///
    /// A report that cannot be read without the pane beside it is not a record. The count and
    /// the pointer stay on the PANE, where the reasons really are in red above the button.
    /// </summary>
    public class ReasonInTheFileTests
    {
        private const string Pointer = "shown in full above the Create button";

        /// <summary>
        /// A plot whose two regions both hold an area and whose types the client's note does not
        /// name, which is the one region case that still refuses.
        /// </summary>
        private static KpiCreateRun Refused(string plotId)
        {
            return CreateFixture.Run(
                readings: new[]
                {
                    CreateFixture.Plot(plotId, regions: new[]
                    {
                        CreateFixture.Region(CreateFixture.Cadastral, 900.0),
                        CreateFixture.Region(CreateFixture.NotTheNote, 250.0)
                    })
                },
                ticked: new[] { plotId });
        }

        private static KpiCreateRunSet Set(params PlotOutcome[] plots)
        {
            return new KpiCreateRunSet(
                "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                new TemplateSplit(
                    new List<TemplateShare>(), new List<PlotTemplate>(), new List<PlotTemplate>()),
                new List<KpiCreateRun>(),
                new List<TemplateOutcome>(),
                null,
                plots);
        }

        /// <summary>
        /// **The words the reconciliation refused on reach the FILE.** Not a count of them and
        /// not a sentence about where to look.
        /// </summary>
        [Fact]
        public void ThePlotsOwnReasonIsWrittenIntoTheFileRatherThanPointedAt()
        {
            KpiCreateRun run = Refused("NS-02");
            string why = CreateWords.WhyThisOneWroteNothing(run);

            Assert.DoesNotContain(Pointer, why);
            Assert.Contains("NS-02 has 2 filled regions holding an area", why);

            string reason = Assert.Single(run.Reconciliation.Refusals);
            Assert.Contains(reason, why);
        }

        /// <summary>
        /// And it reaches the report through the plot's own row, which is where the 102 lines
        /// were.
        /// </summary>
        [Fact]
        public void TheReportRowCarriesTheReasonAndNoPointer()
        {
            KpiCreateRun run = Refused("NS-02");

            string report = KpiCreateReport.WriteAll(
                Set(PlotOutcome.WroteNothing(
                    "NS-02",
                    KpiTemplates.Streets,
                    PlotWorkbookPath.For(@"C:\out", KpiTemplates.Streets, "STREET 30m ROW", "ANH-007-ST-100050"),
                    true,
                    CreateWords.WhyThisOneWroteNothing(run))),
                new DateTime(2026, 9, 14, 14, 29, 0));

            Assert.Contains("NS-02 has 2 filled regions holding an area", report);
            Assert.DoesNotContain(Pointer, report);
        }

        /// <summary>
        /// **The PANE keeps the count and the pointer**, because the reasons really are in red
        /// directly above the button there and the 0928 run printed the same four lines twice on
        /// one screen. The two answers come out of one method, so they cannot come apart.
        /// </summary>
        [Fact]
        public void ThePaneStillCountsAndPointsWhereTheReasonsAreOnScreen()
        {
            string line = CreateWords.Wrote(Refused("NS-02"), @"C:\reports\kpi.txt");

            Assert.Contains(Pointer, line);
            Assert.Contains("1 reason", line);
            Assert.DoesNotContain("2 filled regions holding an area", line);
        }

        /// <summary>
        /// **Where a refusal genuinely has several reasons the file holds ALL of them**, because
        /// a report short of the second reads exactly like a plot that had one.
        /// </summary>
        [Fact]
        public void EveryReasonReachesTheFileWhereThereIsMoreThanOne()
        {
            KpiCreateRun run = CreateFixture.Run(
                readings: new[]
                {
                    CreateFixture.Plot("NS-02", regions: new[]
                    {
                        CreateFixture.Region(CreateFixture.Cadastral, 900.0),
                        CreateFixture.Region(CreateFixture.NotTheNote, 250.0)
                    })
                },
                ticked: new[] { "NS-02", "NS-03" });

            Assert.Equal(2, run.Reconciliation.Refusals.Count);

            string why = CreateWords.WhyThisOneWroteNothing(run);
            foreach (string reason in run.Reconciliation.Refusals) Assert.Contains(reason, why);

            Assert.Contains("2 plots were ticked and 1 were read", why);
            Assert.Contains("NS-02 has 2 filled regions holding an area", why);
        }

        /// <summary>
        /// **The patch's own refusal was already said in full and still is.** It is the half
        /// nothing on the pane carries, so it never went through the pointer.
        /// </summary>
        [Fact]
        public void APatchRefusalIsStillSaidInFullInBothPlaces()
        {
            KpiCreateRun run = CreateFixture.Run(
                outcome: PatchOutcome.Refused("the workbook is open in Excel"));

            Assert.Contains("the workbook is open in Excel", CreateWords.WhyThisOneWroteNothing(run));
            Assert.Contains("the workbook is open in Excel", CreateWords.Wrote(run, string.Empty));
        }

        /// <summary>
        /// **The path's own refusal was already right**, and it is the line the round message
        /// held up as what a record looks like. EP-05 is on no sheet, so it carries no UID2, and
        /// the folder and the file are both named after it.
        /// </summary>
        [Fact]
        public void APlotRefusedByItsOwnPathAlreadyCarriedItsWordsAndStillDoes()
        {
            PlotWorkbookPath where = PlotWorkbookPath.For(
                @"C:\out", KpiTemplates.ExistingParks, "EXISTING PARK", string.Empty);

            Assert.False(where.Ok);
            Assert.DoesNotContain(Pointer, where.Why);

            string report = KpiCreateReport.WriteAll(
                Set(PlotOutcome.WroteNothing("EP-05", KpiTemplates.ExistingParks, where, false, where.Why)),
                new DateTime(2026, 9, 14, 14, 29, 0));

            Assert.Contains(where.Why, report);
        }

        /// <summary>
        /// A run that wrote nothing still never produces an empty reason, which is the rule this
        /// method already carried and the one silence after a press would break.
        /// </summary>
        [Fact]
        public void NoRunThatWroteNothingProducesAnEmptyReason()
        {
            foreach (KpiCreateRun run in new[]
            {
                Refused("NS-02"),
                CreateFixture.Run(outcome: PatchOutcome.Refused("the workbook is open in Excel")),
                CreateFixture.Run(outcome: null)
            })
            {
                Assert.False(run.Wrote);
                Assert.False(string.IsNullOrWhiteSpace(CreateWords.WhyThisOneWroteNothing(run)));
                Assert.False(string.IsNullOrWhiteSpace(CreateWords.Wrote(run, string.Empty)));
            }
        }
    }
}
