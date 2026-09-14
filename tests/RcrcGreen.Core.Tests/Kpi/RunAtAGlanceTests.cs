using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **THREE QUESTIONS ONE PRESS ANSWERS, EACH IN ONE LINE AND A SHORT LIST.**
    ///
    /// All three were answerable before this and all three were spread over hundreds of lines.
    /// The streets area sat in one block per plot, the region type in one row per plot, and the
    /// divisions in one formula section per template. On a run of 156 plots that is a person
    /// reading a 2,000 line file to count.
    ///
    /// Every expected number here is written out by hand from the runs the test builds.
    /// </summary>
    public class RunAtAGlanceTests : IDisposable
    {
        private readonly string _folder = WorkbookFixture.Folder();

        public void Dispose()
        {
            try { Directory.Delete(_folder, true); }
            catch (IOException) { }
        }

        private const string OutOfScope = "RCRC_OUT OF SCOPE (PRESENTATION)";
        private const string Cadastral = "RCRC_CADASTRAL LIMIT";

        private static KpiCreateRunSet Set(params KpiCreateRun[] runs)
        {
            var outcomes = new List<TemplateOutcome>();
            foreach (KpiTemplate template in runs.Select(one => one.Template).Distinct())
            {
                KpiTemplate held = template;
                var plots = runs
                    .Where(one => ReferenceEquals(one.Template, held))
                    .SelectMany(one => one.Readings.Select(reading => reading.PlotId))
                    .ToList();

                outcomes.Add(TemplateOutcome.Wrote(held, plots, plots.Count, @"C:\out"));
            }

            return new KpiCreateRunSet(
                "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                new TemplateSplit(
                    new List<TemplateShare>(), new List<PlotTemplate>(), new List<PlotTemplate>()),
                runs,
                outcomes);
        }

        /// <summary>
        /// One street plot whose area cell landed with a number in it. **The cell is taken off
        /// the template's own map**, so the test asks the map for it rather than naming H8.
        /// </summary>
        private static KpiCreateRun StreetPlotWithAnArea(string plotId)
        {
            MappedCell cell = KpiTemplates.Streets.CellFor(KpiValue.Area);

            return CreateFixture.Run(
                template: KpiTemplates.Streets,
                readings: new[] { CreateFixture.Plot(plotId, regions: new[] { CreateFixture.Region(OutOfScope, 12182.05561411) }) },
                outcome: PatchOutcome.Done(37, 36, new[] { "xl/worksheets/sheet1.xml" }, new[]
                {
                    new LandedCell(KpiTemplates.Streets.MainSheetName, cell.Cell, "12182.05561411")
                }, CacheCheck.NotChecked));
        }

        /// <summary>
        /// A street plot the patch refused, so nothing landed anywhere and its area cell holds
        /// nothing.
        /// </summary>
        private static KpiCreateRun StreetPlotThatWroteNothing(string plotId, string why)
        {
            return CreateFixture.Run(
                template: KpiTemplates.Streets,
                readings: new[] { CreateFixture.Plot(plotId, regions: new[] { CreateFixture.Region(Cadastral, 9000.0) }) },
                outcome: PatchOutcome.Refused(why));
        }

        [Fact]
        public void TwoStreetPlotsWithAnAreaAndOneWithoutCountTwoAndOne()
        {
            RunGlance glance = RunAtAGlance.Of(Set(
                StreetPlotWithAnArea("ST-05"),
                StreetPlotWithAnArea("ST-06"),
                StreetPlotThatWroteNothing("ST-07", "the workbook is open in Excel")));

            Assert.Equal(2, glance.StreetsArea.Written);
            Assert.Single(glance.StreetsArea.Without);
            Assert.Equal(3, glance.StreetsArea.Plots);
            Assert.Equal(
                "THE STREETS AREA: 2 of 3 street plots got a value in the area cell and 1 did not."
                + " The ones that did not are named under this line.",
                glance.StreetsArea.InWords);
        }

        /// <summary>
        /// **A count with nobody named sends a person back through 78 per plot blocks.** The
        /// reason comes off the run rather than being written here, so the line says the same
        /// thing as the plot's own block further down the file.
        /// </summary>
        [Fact]
        public void TheStreetPlotWithNoAreaIsNamedWithTheReasonItsOwnRunRecorded()
        {
            RunGlance glance = RunAtAGlance.Of(Set(
                StreetPlotThatWroteNothing("ST-07", "the workbook is open in Excel")));

            Assert.Equal(new[]
            {
                "ST-07: Nothing was written. the workbook is open in Excel"
            }, glance.StreetsArea.Without.ToArray());
        }

        /// <summary>
        /// **No street plot at all is not the same as every street plot failing.** A press over
        /// mosques says so rather than reading as three of three streets gone wrong.
        /// </summary>
        [Fact]
        public void APressWithNoStreetPlotSaysSoRatherThanCountingNought()
        {
            RunGlance glance = RunAtAGlance.Of(Set(
                CreateFixture.Run(readings: new[] { CreateFixture.Plot("DM-12") })));

            Assert.Equal(0, glance.StreetsArea.Plots);
            Assert.Equal(
                "THE STREETS AREA: no street plot was in this press, so nothing was written into "
                + "a streets area cell and nothing about it was refused.",
                glance.StreetsArea.InWords);
        }

        /// <summary>
        /// A mosque plot writes its own area into its own cell and is not counted here, because
        /// the line is about the cell the client emptied and that cell is on STREETS.
        /// </summary>
        [Fact]
        public void AMosquePlotIsNotCountedAmongTheStreetPlots()
        {
            MappedCell cell = KpiTemplates.Mosques.CellFor(KpiValue.Area);

            RunGlance glance = RunAtAGlance.Of(Set(
                CreateFixture.Run(
                    readings: new[] { CreateFixture.Plot("DM-12") },
                    outcome: PatchOutcome.Done(37, 36, null, new[]
                    {
                        new LandedCell(KpiTemplates.Mosques.MainSheetName, cell.Cell, "3728.757")
                    }, CacheCheck.NotChecked)),
                StreetPlotWithAnArea("ST-05")));

            Assert.Equal(1, glance.StreetsArea.Plots);
            Assert.Equal(1, glance.StreetsArea.Written);
        }

        /// <summary>
        /// **A cell that landed holding nothing did not get a value.** The count is read off
        /// what the output really holds, the same rule every other count in this report follows.
        /// </summary>
        [Fact]
        public void AnAreaCellThatLandedEmptyIsCountedAsNotWritten()
        {
            MappedCell cell = KpiTemplates.Streets.CellFor(KpiValue.Area);

            RunGlance glance = RunAtAGlance.Of(Set(
                CreateFixture.Run(
                    template: KpiTemplates.Streets,
                    readings: new[] { CreateFixture.Plot("ST-05") },
                    outcome: PatchOutcome.Done(37, 36, null, new[]
                    {
                        new LandedCell(KpiTemplates.Streets.MainSheetName, cell.Cell, "   ")
                    }, CacheCheck.NotChecked))));

            Assert.Equal(0, glance.StreetsArea.Written);
            Assert.Equal(new[]
            {
                "ST-05: the workbook was written and its area cell was not read back with a value"
            }, glance.StreetsArea.Without.ToArray());
        }

        /// <summary>
        /// **Two counts and a list, not 156 lines.** Which type a plot's area came off is one
        /// row per plot in the section below, and the note is only settled by counting them.
        /// </summary>
        [Fact]
        public void TheRegionTypesAreCountedAcrossEveryTemplate()
        {
            RunGlance glance = RunAtAGlance.Of(Set(
                Plot("DM-11", KpiTemplates.Mosques, OutOfScope),
                Plot("DM-12", KpiTemplates.Mosques, OutOfScope),
                Plot("DM-13", KpiTemplates.Mosques, OutOfScope),
                Plot("NS-19", KpiTemplates.Streets, Cadastral),
                Plot("NS-06", KpiTemplates.Streets, Cadastral)));

            Assert.Equal(5, glance.Regions.Plots);
            Assert.Equal(3, glance.Regions.OffTheTypeTheNoteNames);
            Assert.Equal(2, glance.Regions.OffATypeTheNoteDoesNotName);
            Assert.Equal(0, glance.Regions.NothingChosen);
            Assert.Equal(
                "THE REGION TYPE: of 5 plots wanting an area, 3 took it off "
                + "RCRC_OUT OF SCOPE (PRESENTATION), which is the type the client's note names, "
                + "2 off a type it does not name, and 0 chose no region at all.",
                glance.Regions.InWords);
        }

        /// <summary>
        /// The type the note names is the top row whatever its count, so the two counts the
        /// team asks for are the first two lines rather than somewhere in a sorted list.
        /// </summary>
        [Fact]
        public void TheTypeTheNoteNamesIsTheFirstRowEvenWhenItIsTheSmallerCount()
        {
            RunGlance glance = RunAtAGlance.Of(Set(
                Plot("NS-19", KpiTemplates.Streets, Cadastral),
                Plot("NS-06", KpiTemplates.Streets, Cadastral),
                Plot("NS-32", KpiTemplates.Streets, Cadastral),
                Plot("DM-11", KpiTemplates.Mosques, OutOfScope)));

            Assert.Equal(new[] { OutOfScope, Cadastral },
                glance.Regions.Types.Select(one => one.TypeName).ToArray());
            Assert.Equal(new[] { 1, 3 }, glance.Regions.Types.Select(one => one.Plots).ToArray());
            Assert.True(glance.Regions.Types[0].IsTheTypeTheNoteNames);
            Assert.False(glance.Regions.Types[1].IsTheTypeTheNoteNames);
        }

        /// <summary>
        /// **A plot that chose nothing took its area off nothing.** Two regions holding an area
        /// is a question waiting on a person and none holding one is a plot with nothing to
        /// read, and neither is a disagreement with the client's note, so both are counted apart
        /// from every type.
        /// </summary>
        [Fact]
        public void APlotThatChoseNoRegionIsCountedApartFromEveryType()
        {
            RunGlance glance = RunAtAGlance.Of(Set(
                Plot("DM-11", KpiTemplates.Mosques, OutOfScope),
                CreateFixture.Run(readings: new[]
                {
                    CreateFixture.Plot("MM-03", regions: new[]
                    {
                        CreateFixture.Region(OutOfScope, 12182.05561411),
                        CreateFixture.Region(Cadastral, 9000.0)
                    })
                })));

            Assert.Equal(2, glance.Regions.Plots);
            Assert.Equal(1, glance.Regions.OffTheTypeTheNoteNames);
            Assert.Equal(0, glance.Regions.OffATypeTheNoteDoesNotName);
            Assert.Equal(1, glance.Regions.NothingChosen);
        }

        /// <summary>
        /// No third type name is written into the tool. A model holding one is counted under
        /// its own name rather than falling under nothing, which is what a rule naming two
        /// types outright would have done.
        /// </summary>
        [Fact]
        public void ATypeNeitherNameCoversIsStillCountedUnderItsOwnName()
        {
            RunGlance glance = RunAtAGlance.Of(Set(
                Plot("DM-11", KpiTemplates.Mosques, OutOfScope),
                Plot("EP-05", KpiTemplates.ExistingParks, "RCRC_SOMETHING NOBODY HAS MEASURED")));

            Assert.Equal(new[] { OutOfScope, "RCRC_SOMETHING NOBODY HAS MEASURED" },
                glance.Regions.Types.Select(one => one.TypeName).ToArray());
            Assert.Equal(1, glance.Regions.OffATypeTheNoteDoesNotName);
        }

        private static KpiCreateRun Plot(string plotId, KpiTemplate template, string regionType)
        {
            return CreateFixture.Run(
                template: template,
                readings: new[]
                {
                    CreateFixture.Plot(plotId, regions: new[] { CreateFixture.Region(regionType, 1000.0) })
                });
        }

        /// <summary>
        /// The fixture's own main sheet holds H7 as a real numeric nought and D9 as D8/H7, so a
        /// real patch produces a real #DIV/0! and the count is over what the check found rather
        /// than over a finding the test made up.
        /// </summary>
        private KpiCreateRun Divided(string plotId, bool weWroteTheDivisor)
        {
            string template = WorkbookFixture.Computing(
                _folder,
                new[] { new WorkbookFixture.TreeRow(4, "Albizia lebbeck", "15", "8") },
                new[] { new WorkbookFixture.TreeRow(4, "Cassia glauca", "6", "5") },
                plotId + ".xlsx");

            CellWrite[] writes = weWroteTheDivisor
                ? new[] { CellWrite.Number("<Mosques>", "H7", 0.0) }
                : new[] { CellWrite.Text("<Mosques>", "D3", "FRIDAY MOSQUE") };

            PatchOutcome outcome = WorkbookPatcher.Patch(
                template, Path.Combine(_folder, plotId + "-out.xlsx"), writes);

            Assert.True(outcome.Written, outcome.Refusal);

            return CreateFixture.Run(
                readings: new[] { CreateFixture.Plot(plotId) },
                outcome: outcome);
        }

        /// <summary>
        /// **The second number is the one that matters.** A division by a client cell is their
        /// arithmetic over a real number and a division by a cell the tool wrote is ours.
        /// </summary>
        [Fact]
        public void TheDivisionsAreCountedAcrossTheRunAndTheOnesWeWroteAreCountedApart()
        {
            RunGlance glance = RunAtAGlance.Of(Set(
                Divided("DM-12", false),
                Divided("DM-13", true)));

            Assert.Equal(2, glance.Divisions.Found);
            Assert.Equal(1, glance.Divisions.ByACellThisRunWrote);
            Assert.Equal(
                "THE DIVISIONS: 2 #DIV/0! were found and 1 of them divide by a cell THIS RUN "
                + "WROTE. A division by a cell this run wrote is this tool's doing.",
                glance.Divisions.InWords);
            Assert.Equal(new[]
            {
                "DM-12 | <Mosques> D9 | divides by a client cell",
                "DM-13 | <Mosques> D9 | divides by a cell THIS RUN WROTE"
            }, glance.Divisions.Where.ToArray());
        }

        /// <summary>
        /// **THE KIND AND THE DIVISOR TRAVEL ON THE FINDING, not in the sentence it prints.** A
        /// signal that travels in the data is not a signal: a counter searching the reason would
        /// count a reason that merely talks about a division, which is the shape that once made
        /// a commit refuse its own message.
        /// </summary>
        [Fact]
        public void TheKindAndTheDivisorAreProperties()
        {
            FormulaAtRisk ours = Divided("DM-13", true)
                .Outcome.Formulas.AtRisk.First(one => one.Cell == "D9");

            Assert.True(ours.IsDivideByZero);
            Assert.True(ours.DivisorThisRunWrote);

            FormulaAtRisk theirs = Divided("DM-12", false)
                .Outcome.Formulas.AtRisk.First(one => one.Cell == "D9");

            Assert.True(theirs.IsDivideByZero);
            Assert.False(theirs.DivisorThisRunWrote);

            // Every other kind of risk answers false for both, so a counter asking the property
            // cannot pick up a #VALUE! or a text return on the way past.
            foreach (FormulaAtRisk risk in Divided("DM-14", false).Outcome.Formulas.AtRisk)
            {
                if (risk.IsDivideByZero) continue;
                Assert.False(risk.DivisorThisRunWrote);
            }
        }

        /// <summary>
        /// A press with no division says so rather than printing a nought somebody has to read
        /// as an answer.
        /// </summary>
        [Fact]
        public void NoDivisionSaysSoRatherThanPrintingNought()
        {
            RunGlance glance = RunAtAGlance.Of(Set(
                CreateFixture.Run(readings: new[] { CreateFixture.Plot("DM-12") })));

            Assert.Equal(0, glance.Divisions.Found);
            Assert.Empty(glance.Divisions.Where);
            Assert.Equal(
                "THE DIVISIONS: the formula check found no #DIV/0! anywhere in this press.",
                glance.Divisions.InWords);
        }

        /// <summary>
        /// All three reach the report, at the TOP of the run's own section and above every
        /// per template block, which is the whole point of counting them.
        /// </summary>
        [Fact]
        public void TheReportPrintsTheThreeLinesAboveEveryTemplateBlock()
        {
            KpiCreateRunSet set = Set(
                StreetPlotWithAnArea("ST-05"),
                StreetPlotThatWroteNothing("ST-07", "the workbook is open in Excel"),
                Divided("DM-13", true));

            string report = KpiCreateReport.WriteAll(set, new DateTime(2026, 9, 14, 9, 18, 0));

            RunGlance glance = RunAtAGlance.Of(set);

            Assert.Contains(glance.StreetsArea.InWords, report);
            Assert.Contains("    ST-07: Nothing was written. the workbook is open in Excel", report);
            Assert.Contains(glance.Regions.InWords, report);
            Assert.Contains(glance.Divisions.InWords, report);
            Assert.Contains("DM-13 | <Mosques> D9 | divides by a cell THIS RUN WROTE", report);

            Assert.True(
                report.IndexOf(KpiCreateReport.GlanceHeading, StringComparison.Ordinal)
                    < report.IndexOf("THIS RUN, ACROSS EVERY TEMPLATE", StringComparison.Ordinal),
                "the glance must be above the run's own accounting");

            Assert.True(
                report.IndexOf(KpiCreateReport.GlanceHeading, StringComparison.Ordinal)
                    < report.IndexOf("TEMPLATE: ", StringComparison.Ordinal),
                "the glance must be above every per template block");
        }

        /// <summary>
        /// **THE GLANCE HEADING CARRIES NO COUNT, and every other heading does.** Three is how
        /// many questions there are rather than how many of anything this run found, and a
        /// constant sitting where a count goes is a number that reads as a measurement.
        /// </summary>
        [Fact]
        public void TheGlanceHeadingCarriesNoCountWhereEveryOtherHeadingDoes()
        {
            string report = KpiCreateReport.WriteAll(
                Set(StreetPlotWithAnArea("ST-05")),
                new DateTime(2026, 9, 14, 9, 18, 0));

            Assert.Contains("== " + KpiCreateReport.GlanceHeading + " ==", report);
            Assert.DoesNotContain("== " + KpiCreateReport.GlanceHeading + " (", report);

            // The heading right under it still counts its own rows, so this is one heading's
            // rule rather than the shape going away.
            Assert.Contains("== THIS RUN, ACROSS EVERY TEMPLATE (", report);
        }

        /// <summary>
        /// The per plot detail is still where it was. The glance is a count and never a move.
        /// </summary>
        [Fact]
        public void TheRegionRowPerPlotIsStillPrintedUnderItsTemplate()
        {
            string report = KpiCreateReport.WriteAll(
                Set(Plot("NS-19", KpiTemplates.Streets, Cadastral)),
                new DateTime(2026, 9, 14, 9, 18, 0));

            Assert.Contains("WHICH REGION EACH PLOT'S AREA CAME OFF, AND WHAT IT READ", report);
            Assert.Contains("NS-19 | " + Cadastral + " | NOT the type the note names", report);
        }
    }
}
