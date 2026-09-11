using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// Finding 34. Every region choice and the identical areas confirm cost a full run, every
    /// ticked plot's sheets, schedules and regions read from the start, about eight minutes
    /// on 78 plots. A refusal exists so a person can answer a question, and answering it
    /// should not cost the answer again. The run the pane holds is the record: a choice is
    /// applied to its readings, and the model is read again only when the run cannot be
    /// trusted, with the report saying which.
    /// </summary>
    public class HeldReadingsTests
    {
        private const string Title = "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached";

        private const string TemplatePath = @"C:\templates\MOSQUES.xlsx";

        private static PlotReading TwoRegions()
        {
            return CreateFixture.Plot(
                "NS-19",
                species: new[] { CreateFixture.Species("ALBIZIA LEBBECK", CreateFixture.Proposed, 13) },
                regions: new[]
                {
                    CreateFixture.Region(CreateFixture.Cadastral, 900.0),
                    CreateFixture.Region(CreateFixture.OutOfScope, 250.0)
                },
                chosenRegion: string.Empty,
                readSeconds: 15.2);
        }

        private static ReadingsSource DecideSame(KpiCreateRun run, params string[] ticked)
        {
            return HeldReadings.Decide(run, Title, KpiTemplates.Mosques, TemplatePath, "PRX_Component", "PRX_Plot_UID2", ticked);
        }

        /// <summary>
        /// The choice lands on the reading and nothing else on it moves: the regions, the read
        /// seconds, the species and the schedule name are the run before's.
        /// </summary>
        [Fact]
        public void AChoiceIsAppliedToTheHeldReadingAndNothingElseOnItMoves()
        {
            PlotReading reading = TwoRegions();

            IReadOnlyList<PlotReading> applied = HeldReadings.Applied(
                new[] { reading }, plot => plot == "NS-19" ? CreateFixture.Cadastral : string.Empty);

            PlotReading chosen = Assert.Single(applied);
            Assert.Equal(CreateFixture.Cadastral, chosen.ChosenRegionTypeName);
            Assert.Equal(900.0, chosen.ChosenRegion.SquareMetres);
            Assert.Equal(2, chosen.Regions.Count);
            Assert.Equal(15.2, chosen.ReadSeconds);
            Assert.Single(chosen.Species);
            Assert.Equal(new[] { "NS-19-(600) SOFTSCAPE SCHEDULE" }, chosen.SoftscapeSchedules);
        }

        [Fact]
        public void NoChoiceLeavesTheReadingAsItWas()
        {
            PlotReading reading = TwoRegions();

            IReadOnlyList<PlotReading> applied = HeldReadings.Applied(new[] { reading }, plot => string.Empty);

            Assert.Same(reading, applied[0]);
        }

        /// <summary>
        /// The reconciliation that refused the plot before the pick passes on the applied
        /// readings with nothing read, and the area is the region picked.
        /// </summary>
        [Fact]
        public void TheReconciliationPassesOnTheAppliedReadingsWithoutARead()
        {
            PlotReading reading = TwoRegions();

            Reconciliation before = Reconciliation.Of(new[] { "NS-19" }, new[] { reading }, null, false, KpiTemplates.Mosques);
            Assert.False(before.AddsUp);
            Assert.Contains(before.Refusals, one => one.Contains("NS-19 has 2 filled regions holding an area"));

            IReadOnlyList<PlotReading> applied = HeldReadings.Applied(new[] { reading }, plot => CreateFixture.Cadastral);
            Reconciliation after = Reconciliation.Of(new[] { "NS-19" }, applied, null, false, KpiTemplates.Mosques);

            Assert.True(after.AddsUp, string.Join(" ", after.Refusals));
            Assert.Equal(900.0, KpiMerge.Area(applied).Total);
        }

        [Fact]
        public void AHeldRunAnswersTheSameAskOnTheSameModel()
        {
            ReadingsSource source = DecideSame(CreateFixture.Run(), "DM-12");

            Assert.True(source.Reused, source.Why);
            Assert.Equal(
                "the readings of the run before, on the same model, template, parameters and plots, with the choices made since applied to them",
                source.Why);
        }

        [Fact]
        public void NoHeldRunReadsTheModel()
        {
            ReadingsSource source = DecideSame(null, "DM-12");

            Assert.False(source.Reused);
            Assert.Equal("no run is held, so the model was read", source.Why);
        }

        /// <summary>
        /// Each thing that can differ reads the model again and is named: the model, the
        /// template, the template file, either parameter, a plot dropped, a plot added.
        /// </summary>
        [Theory]
        [InlineData("Other", "MOSQUES", TemplatePath, "PRX_Component", "PRX_Plot_UID2", "DM-12", "the run before was on RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached and this press is on Other")]
        [InlineData(Title, "STREETS", TemplatePath, "PRX_Component", "PRX_Plot_UID2", "DM-12", "the run before was for MOSQUES and this press is for STREETS")]
        [InlineData(Title, "MOSQUES", @"C:\templates\MOSQUES copy.xlsx", "PRX_Component", "PRX_Plot_UID2", "DM-12", "the template file changed")]
        [InlineData(Title, "MOSQUES", TemplatePath, "PRX_Plot_ID", "PRX_Plot_UID2", "DM-12", "a parameter picked on the pane changed")]
        [InlineData(Title, "MOSQUES", TemplatePath, "PRX_Component", "PRX_Plot_UID", "DM-12", "a parameter picked on the pane changed")]
        [InlineData(Title, "MOSQUES", TemplatePath, "PRX_Component", "PRX_Plot_UID2", "DM-11", "the plots ticked changed")]
        [InlineData(Title, "MOSQUES", TemplatePath, "PRX_Component", "PRX_Plot_UID2", "DM-12,DM-11", "the plots ticked changed")]
        public void AnythingThatDiffersReadsTheModelAgainAndIsNamed(
            string title, string templateName, string templatePath, string component, string reference, string ticked, string named)
        {
            KpiTemplate template = KpiTemplates.All.Single(one => one.Name == templateName);

            ReadingsSource source = HeldReadings.Decide(
                CreateFixture.Run(), title, template, templatePath, component, reference, ticked.Split(','));

            Assert.False(source.Reused);
            Assert.Contains(named, source.Why);
            Assert.EndsWith("so the model was read again", source.Why, StringComparison.Ordinal);
        }

        [Fact]
        public void TicksInAnotherOrderAreTheSamePlots()
        {
            KpiCreateRun run = CreateFixture.Run(new[]
            {
                CreateFixture.Plot("DM-11"),
                CreateFixture.Plot("DM-12", regions: new[] { CreateFixture.Region(CreateFixture.OutOfScope, 250.0) })
            });

            Assert.True(DecideSame(run, "DM-12", "DM-11").Reused);
            Assert.True(DecideSame(run, " DM-11 ", "DM-12", "DM-12").Reused);
        }

        /// <summary>
        /// A run whose reconciliation was made over two ticked plots and one reading is the
        /// shape a plot that went unread would leave, and it is read again naming the plot.
        /// </summary>
        [Fact]
        public void AHeldRunShortOfAPlotIsReadAgain()
        {
            KpiCreateRun run = CreateFixture.Run(new[] { CreateFixture.Plot("DM-11") }, ticked: new[] { "DM-11", "DM-12" });

            ReadingsSource source = DecideSame(run, "DM-11", "DM-12");

            Assert.False(source.Reused);
            Assert.Equal("the run before holds no reading for DM-12, so the model was read again", source.Why);
        }

        /// <summary>
        /// A plot whose read threw holds nothing to reuse. The team fixes the model and presses
        /// Create again with nothing else changed, and that press has to read, or it hands the
        /// same refusal back for ever. The reason names the plot.
        /// </summary>
        [Fact]
        public void ARunThatCouldNotReadAPlotIsReadAgain()
        {
            KpiCreateRun run = CreateFixture.Run(
                new[]
                {
                    CreateFixture.Plot("DM-11"),
                    PlotReading.NotRead("DM-60", "InvalidOperationException: The schedule is not valid.", 0.4)
                },
                ticked: new[] { "DM-11", "DM-60" });

            ReadingsSource source = DecideSame(run, "DM-11", "DM-60");

            Assert.False(source.Reused);
            Assert.Equal("the run before could not read DM-60, so the model was read again", source.Why);
        }

        /// <summary>
        /// A refusal off one schedule is the same: the plot was read short, and only a read can
        /// see whether the schedule was fixed. Every refused plot is named, in the run's order.
        /// </summary>
        [Fact]
        public void ARunWithASchedulesRefusalOnTwoPlotsIsReadAgainNamingBoth()
        {
            KpiCreateRun run = CreateFixture.Run(
                new[]
                {
                    CreateFixture.Plot("DM-11", readRefusals: new[] { "two softscape schedules filter on DM-11" }),
                    CreateFixture.Plot("DM-12"),
                    CreateFixture.Plot("DM-13", readRefusals: new[] { "SHRUBS AND LAWN - DM-13: the read threw" })
                },
                ticked: new[] { "DM-11", "DM-12", "DM-13" });

            ReadingsSource source = DecideSame(run, "DM-11", "DM-12", "DM-13");

            Assert.False(source.Reused);
            Assert.Equal("the run before could not read DM-11, DM-13, so the model was read again", source.Why);
        }

        /// <summary>
        /// A run that wrote its workbook asked nothing, so the press after it is a new run
        /// and reads the model. The reuse is for answering a refusal.
        /// </summary>
        [Fact]
        public void ARunThatWroteIsNotReused()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                string template = WorkbookFixture.Create(folder);
                PatchOutcome outcome = WorkbookPatcher.Patch(template, Path.Combine(folder, "out.xlsx"),
                    new[] { CellWrite.Text(WorkbookFixture.MainSheet, "E5", "2026-09-11") });
                Assert.True(outcome.Written, outcome.Refusal);

                ReadingsSource source = DecideSame(CreateFixture.Run(outcome: outcome), "DM-12");

                Assert.False(source.Reused);
                Assert.Equal("the run before wrote its workbook, so the model was read again", source.Why);
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        /// <summary>
        /// The report says which it did, beside the seconds. A reused run reads 0.0 seconds of
        /// model read, and the line under it is what makes that true rather than a mistake.
        /// </summary>
        [Fact]
        public void TheReportSaysWhetherTheModelWasReadOnThisPress()
        {
            string reused = KpiCreateReport.Write(
                CreateFixture.Run(timing: RunTiming.Of(3.1, 0.0), readingsSource: ReadingsSource.Held("the readings of the run before, on the same model, template, parameters and plots, with the choices made since applied to them")),
                new DateTime(2026, 9, 11, 9, 0, 0));

            Assert.Contains(
                "Run: 3.1 seconds, of which reading the model took 0.0 seconds and everything after it 3.1 seconds.\r\n"
                + "Readings: reused, nothing was read from the model on this press, the readings of the run before, on the same model, template, parameters and plots, with the choices made since applied to them.\r\n",
                reused);

            string fresh = KpiCreateReport.Write(CreateFixture.Run(), new DateTime(2026, 9, 11, 9, 0, 0));
            Assert.Contains("Readings: read from the model on this press, read on this press.\r\n", fresh);
        }

        /// <summary>
        /// The identical areas confirm is the other question. Confirmed on the held readings
        /// with nothing read, the reconciliation passes and the pair is still named.
        /// </summary>
        [Fact]
        public void AConfirmOfIdenticalAreasPassesOnTheHeldReadings()
        {
            var readings = new[]
            {
                CreateFixture.Plot("MM-03", regions: new[] { CreateFixture.Region(CreateFixture.Cadastral, 1131.7, 12182.05561411) }),
                CreateFixture.Plot("MM-04", regions: new[] { CreateFixture.Region(CreateFixture.Cadastral, 1131.7, 12182.05561411) })
            };

            Reconciliation before = Reconciliation.Of(new[] { "MM-03", "MM-04" }, readings, null, false, KpiTemplates.Mosques);
            Assert.False(before.AddsUp);

            IReadOnlyList<PlotReading> applied = HeldReadings.Applied(readings, plot => string.Empty);
            Reconciliation after = Reconciliation.Of(new[] { "MM-03", "MM-04" }, applied, null, true, KpiTemplates.Mosques);

            Assert.True(after.AddsUp, string.Join(" ", after.Refusals));
            Assert.Single(after.IdenticalAreas);
            Assert.Same(readings[0], applied[0]);
        }
    }
}
