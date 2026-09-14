using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// What one plot's PDF gets, and the two new reads the form asks for that the workbook never
    /// has: the shrubs split by phase, and the ground cover on its own.
    ///
    /// Every expected value is written out by hand.
    /// </summary>
    public class PdfFillTests : IDisposable
    {
        private readonly string _folder = PdfFixture.Folder();

        public void Dispose()
        {
            try { Directory.Delete(_folder, true); }
            catch (IOException) { }
        }

        private static readonly DateTime Today = new DateTime(2026, 9, 14);

        /// <summary>
        /// A mosque plot's shrubs group with an existing phase and a proposed one, off the shape
        /// the 0928 run measured: a subtotal per phase, then the group total.
        /// </summary>
        private static GroupSubtotal Shrubs(params PhaseSubtotal[] phases)
        {
            double taken = phases.Where(one => one.Counted).Sum(one => one.SquareMetres);

            return new GroupSubtotal(
                KpiMerge.ShrubsHeading, taken, 0, phases.Length + 1, double.NaN, null, 0, null,
                phases, true, phases.Sum(one => one.SquareMetres), 0);
        }

        private static PhaseSubtotal Phase(string name, double squareMetres, bool counted = true)
        {
            return new PhaseSubtotal(name, 0, squareMetres, 0, counted, string.Empty);
        }

        private static PlotReading Plot(string plotId, params GroupSubtotal[] subtotals)
        {
            return CreateFixture.Plot(plotId, subtotals: subtotals, uid2: "ANH-008-MO-100006");
        }

        private static PdfPlan Plan(PlotReading reading, KpiTemplate template = null, bool workbookWritten = true)
        {
            return PdfFill.Of(
                reading, CountedGroups.Of(template ?? KpiTemplates.Mosques),
                null, Today, workbookWritten, PdfWorkbookNumbers.None);
        }

        private static string Wrote(PdfPlan plan, PdfValue value)
        {
            return plan.Fields.Single(one => one.Value == value).Text;
        }

        private static string Blank(PdfPlan plan, PdfValue value)
        {
            return plan.Fields.Single(one => one.Value == value).Why;
        }

        /// <summary>
        /// **THE SHRUBS SPLIT BY PHASE.** The PDF wants Existing Shrubs and Proposed Shrubs
        /// apart where the workbook has only ever wanted them together, and the numbers are the
        /// phase rows the schedule already prints.
        /// </summary>
        [Fact]
        public void TheShrubsSplitIntoTheirPhases()
        {
            PdfPlan plan = Plan(Plot("DM-16",
                Shrubs(Phase(CreateFixture.Existing, 30.0), Phase(CreateFixture.Proposed, 54.0))));

            Assert.Equal("30", Wrote(plan, PdfValue.ExistingShrubs));
            Assert.Equal("54", Wrote(plan, PdfValue.ProposedShrubs));
        }

        /// <summary>
        /// **TOTAL SHRUBS IS EXISTING PLUS PROPOSED ON ALL THREE FORMS.** Never the group total
        /// row, which holds every phase the schedule printed. DM-16 prints 30, 54 and a group
        /// total of 84, and 30 plus 54 is 84.
        /// </summary>
        [Fact]
        public void TotalShrubsIsTheTwoAddedAndNeverTheGroupTotalRow()
        {
            PdfPlan plan = Plan(Plot("DM-16",
                Shrubs(Phase(CreateFixture.Existing, 30.0), Phase(CreateFixture.Proposed, 54.0))));

            Assert.Equal("84", Wrote(plan, PdfValue.TotalShrubs));
        }

        /// <summary>
        /// **A phase this template leaves out is in neither number.** FM-05's shrubs print
        /// Proposed 361 and Street Design 459 with a group total of 820, and on MOSQUES the
        /// street is somebody else's scope, so the proposed shrubs are 361 and the total is 361.
        /// Taking the group total row would write 820.
        /// </summary>
        [Fact]
        public void APhaseTheTemplateLeavesOutIsInNeitherNumber()
        {
            PdfPlan plan = Plan(Plot("FM-05",
                Shrubs(Phase(CreateFixture.Proposed, 361.0), Phase("Street Design", 459.0, false))));

            Assert.Equal("0", Wrote(plan, PdfValue.ExistingShrubs));
            Assert.Equal("361", Wrote(plan, PdfValue.ProposedShrubs));
            Assert.Equal("361", Wrote(plan, PdfValue.TotalShrubs));

            // **AND IT IS NAMED AS LEFT OUT rather than as a phase no sheet takes.** The two are
            // different facts: one is this template's decision and the other is a shape nobody
            // has measured. Asserting the numbers alone left a break that mixed them up green.
            PhaseSplit split = ShrubsByPhase.Of(
                Plot("FM-05", Shrubs(Phase(CreateFixture.Proposed, 361.0), Phase("Street Design", 459.0, false))),
                KpiMerge.ShrubsHeading,
                CountedGroups.Of(KpiTemplates.Mosques));

            Assert.Equal("Street Design", Assert.Single(split.PhasesLeftOut));
            Assert.Empty(split.PhasesWithNoSheet);
        }

        /// <summary>
        /// **And the same schedule read for STREETS counts it**, because the split asks the same
        /// CountedGroups the species merge asks and there is no second rule. 361 plus 459 is 820.
        /// </summary>
        [Fact]
        public void OnStreetsTheStreetDesignPhaseCountsAsProposed()
        {
            PdfPlan plan = Plan(
                Plot("ST-05", Shrubs(Phase(CreateFixture.Proposed, 361.0), Phase("Street Design", 459.0))),
                KpiTemplates.Streets);

            Assert.Equal("820", Wrote(plan, PdfValue.ProposedShrubs));
            Assert.Equal("820", Wrote(plan, PdfValue.TotalShrubs));
        }

        /// <summary>
        /// A group that printed no phase row at all splits into nothing, because its one row is
        /// the whole group and nothing on it says which phase that is.
        /// </summary>
        [Fact]
        public void AGroupWithNoPhaseRowSplitsIntoNothing()
        {
            PhaseSplit split = ShrubsByPhase.Of(
                Plot("DM-11", new GroupSubtotal(KpiMerge.ShrubsHeading, 70.0, 58)),
                KpiMerge.ShrubsHeading,
                CountedGroups.Of(KpiTemplates.Mosques));

            Assert.True(split.GroupFound);
            Assert.Equal(0.0, split.ExistingSquareMetres);
            Assert.Equal(0.0, split.ProposedSquareMetres);
            Assert.Equal(
                "the group printed no phase row, so nothing says which phase its area is",
                Assert.Single(split.PhasesWithNoSheet));
        }

        /// <summary>
        /// A plot whose schedule printed no shrubs group at all leaves all three fields blank and
        /// names it. **An absence is not a nought.**
        /// </summary>
        [Fact]
        public void APlotWithNoShrubsGroupLeavesAllThreeBlankAndNamesIt()
        {
            PdfPlan plan = Plan(Plot("DM-16"));

            foreach (PdfValue value in new[] { PdfValue.ExistingShrubs, PdfValue.ProposedShrubs, PdfValue.TotalShrubs })
            {
                Assert.Equal(
                    "this plot's shrubs and lawn schedule printed no SHRUBS & GROUND COVER group",
                    Blank(plan, value));
            }
        }

        /// <summary>
        /// **GROUND COVER IS NOT PRINTED APART FROM SHRUBS, so nothing is written and it is
        /// named.** The schedule prints SHRUBS & GROUND COVER as one group over one set of rows,
        /// measured on every scan this project has taken. Splitting one printed number into two
        /// would be a number nobody measured.
        /// </summary>
        [Fact]
        public void GroundCoverIsLeftBlankAndNamedRatherThanDerived()
        {
            PdfPlan plan = Plan(Plot("DM-16",
                Shrubs(Phase(CreateFixture.Existing, 30.0), Phase(CreateFixture.Proposed, 54.0))));

            Assert.Equal(
                "the schedule prints SHRUBS & GROUND COVER as one group and nothing prints ground "
                + "cover on its own, so it is not derived",
                Blank(plan, PdfValue.GroundCover));
        }

        /// <summary>
        /// **A plot whose workbook was not written computes neither number and says so.** Bader
        /// asked for the PDF either way, so everything off Revit still goes in and the two the
        /// workbook's own numbers are built from are named.
        /// </summary>
        [Fact]
        public void APlotWithNoWorkbookComputesNeitherNumberAndSaysWhy()
        {
            PdfPlan refused = PdfFill.Of(
                Plot("EP-05"), CountedGroups.Of(KpiTemplates.ExistingParks), null, Today, false,
                PdfWorkbookNumbers.None);

            Assert.Equal(PdfFill.TheWorkbookWasNotWritten, Blank(refused, PdfValue.TotalAreasToBeGreened));
            Assert.Equal(PdfFill.TheWorkbookWasNotWritten, Blank(refused, PdfValue.PercentageCanopy));

            // **A workbook that was written but whose formulas were never read computes nothing
            // either**, because the canopy is the workbook's own arithmetic worked out again and
            // a check that cannot see its subject has to refuse.
            PdfPlan unchecked_ = PdfFill.Of(
                Plot("EP-05"), CountedGroups.Of(KpiTemplates.ExistingParks), null, Today, true,
                PdfWorkbookNumbers.None);

            Assert.Equal(WorkbookArithmetic.NoWorkbookRead, Blank(unchecked_, PdfValue.TotalAreasToBeGreened));
        }

        /// <summary>
        /// The report date is today, day then month then year. Bader, 14 September.
        /// </summary>
        [Fact]
        public void TheReportDateIsTodayDayMonthYear()
        {
            Assert.Equal("14/09/2026", Wrote(Plan(Plot("DM-16")), PdfValue.ReportDate));
        }

        /// <summary>
        /// **The project type is PRX_Component EXACTLY as Revit holds it, capitals and all.**
        /// Bader, 14 September. Nothing turns FRIDAY MOSQUE into anything else.
        /// </summary>
        [Fact]
        public void TheProjectTypeIsTheComponentExactlyAsRevitHoldsIt()
        {
            PlotReading reading = CreateFixture.Plot(
                "DM-16", component: "FRIDAY MOSQUE", uid2: "ANH-008-MO-100006");

            Assert.Equal("FRIDAY MOSQUE", Wrote(Plan(reading), PdfValue.ProjectType));
        }

        /// <summary>
        /// The UID is PRX_Plot_UID2, and a plot carrying none leaves the field blank and names
        /// the parameter rather than writing an empty box nobody can trace.
        /// </summary>
        [Fact]
        public void APlotWithNoUid2LeavesTheFieldBlankAndNamesTheParameter()
        {
            PdfPlan plan = Plan(CreateFixture.Plot("DM-16"));

            Assert.Equal("this plot's sheet carries no PRX_Plot_UID2", Blank(plan, PdfValue.Uid));
        }

        /// <summary>
        /// A plot no form is named for plans no PDF and says so, and nothing about it throws.
        /// </summary>
        [Fact]
        public void APlotNoFormIsNamedForPlansNothing()
        {
            PdfPlan plan = Plan(CreateFixture.Plot("ZZ-01"));

            Assert.False(plan.Wanted);
            Assert.Null(plan.Form);
            Assert.Equal("no form is named for the plot prefix ZZ, so no PDF was written", plan.Why);
        }

        /// <summary>
        /// The road width and the total length come off the street reference answer, and a
        /// street plot the file does not name leaves both blank with the file's own reason.
        /// </summary>
        [Fact]
        public void TheRoadWidthAndLengthComeOffTheReferenceFileOrAreNamed()
        {
            PdfPlan found = PdfFill.Of(
                CreateFixture.Plot("ST-05", uid2: "ANH-007-ST-100210"),
                CountedGroups.Of(KpiTemplates.Streets),
                StreetReferenceAnswer.Of(20.0, 330.66, "20", "330.66"),
                Today, true, PdfWorkbookNumbers.None);

            // The width is metres into a box the form prints m beside, so it goes in unchanged.
            // The length is metres into a box the form prints km beside, so it is divided.
            Assert.Equal("20", Wrote(found, PdfValue.Row));
            Assert.Equal("0.33066", Wrote(found, PdfValue.Length));

            PdfPlan missing = PdfFill.Of(
                CreateFixture.Plot("ST-05", uid2: "ANH-007-ST-100210"),
                CountedGroups.Of(KpiTemplates.Streets),
                StreetReferenceAnswer.Nothing("the file names no row for this plot"),
                Today, true, PdfWorkbookNumbers.None);

            Assert.Equal("the file names no row for this plot", Blank(missing, PdfValue.Row));
        }

        /// <summary>
        /// **A form that does not match writes NOTHING into it**, which is the whole of the
        /// broken field names rule. The names on the open spaces form are undefined_4.1 and 0_2,
        /// and writing a lawn area into a box that has become something else is exactly the
        /// silent wrong number this tool exists to prevent.
        /// </summary>
        [Fact]
        public void AFormWhoseNoteHasMovedIsLeftAloneAndNamed()
        {
            PdfForm form = PdfForms.Parks;
            var fields = form.Fields
                .Select(one => new PdfFieldRead(
                    one.FieldName,
                    one.Value == PdfValue.Lawn ? "SOMETHING THE CLIENT CHANGED" : one.Note,
                    1))
                .ToList();

            fields.AddRange(Header());

            PdfFormCheck check = PdfFormCheck.Of(form, fields, string.Empty);

            Assert.False(check.Matched);
            Assert.Empty(check.Missing);
            Assert.Single(check.DifferingNotes);
            Assert.Contains("Lawn holds \"SOMETHING THE CLIENT CHANGED\"", check.Why);
            Assert.Contains("nothing was written into it", check.Why);
        }

        /// <summary>
        /// A field this tool fills that the file does not hold at all is named as missing, which
        /// is the other half of the same refusal.
        /// </summary>
        [Fact]
        public void AFieldThatIsGoneFromTheFormIsNamedAsMissing()
        {
            PdfForm form = PdfForms.OpenSpaces;
            var fields = form.Fields
                .Where(one => one.Value != PdfValue.Uid)
                .Select(one => new PdfFieldRead(one.FieldName, one.Note, 1))
                .ToList();

            fields.AddRange(Header());

            PdfFormCheck check = PdfFormCheck.Of(form, fields, string.Empty);

            Assert.False(check.Matched);
            Assert.Equal("undefined_4.1", Assert.Single(check.Missing));
        }

        /// <summary>
        /// A form whose fields and notes all hold is matched, and the client's own fields that
        /// this tool never writes into are free to move.
        /// </summary>
        [Fact]
        public void AFormWhoseFieldsAndNotesHoldIsMatched()
        {
            PdfForm form = PdfForms.Roads;
            var fields = form.Fields
                .Select(one => new PdfFieldRead(one.FieldName, one.Note, 1, one.X, one.Y)).ToList();
            fields.Add(new PdfFieldRead("Sidewalk Quantity", string.Empty, 1));
            fields.AddRange(Header());

            PdfFormCheck check = PdfFormCheck.Of(form, fields, string.Empty);

            Assert.True(check.Matched, check.Why);
            Assert.Equal(string.Empty, check.Why);
        }

        /// <summary>
        /// A file that could not be read at all is told apart from one read and found to be
        /// another form.
        /// </summary>
        [Fact]
        public void AFileThatCouldNotBeReadIsToldApartFromOneThatDidNotMatch()
        {
            PdfFormCheck check = PdfFormCheck.Of(PdfForms.Parks, null, PdfFormFile.HoldsObjectStreams);

            Assert.False(check.Read);
            Assert.False(check.Matched);
            Assert.Equal(PdfFormFile.HoldsObjectStreams, check.Why);
        }

        /// <summary>
        /// The PDF is beside the workbook, in the same folder, under the same name.
        /// </summary>
        [Fact]
        public void ThePdfSitsBesideTheWorkbookUnderTheSameName()
        {
            PlotWorkbookPath where = PlotWorkbookPath.For(
                Path.Combine(_folder, "root"), KpiTemplates.Mosques, "FRIDAY MOSQUE", "ANH-008-MO-100006");

            Assert.True(where.Ok, where.Why);
            Assert.Equal(
                Path.Combine(where.FolderPath, "ANH-008-MO-100006.pdf"),
                PdfChecklist.Beside(where));
        }

        /// <summary>
        /// The client's own header, which every one of their three forms carries and the check
        /// now requires. The field names are the fixture's own: the tool finds all three by the
        /// value the template holds, because the real names are measured nowhere here.
        /// </summary>
        private static IEnumerable<PdfFieldRead> Header()
        {
            return new[]
            {
                new PdfFieldRead("Project name", PdfForms.ProjectName, 900, 60.0, 700.0, "Tx"),
                new PdfFieldRead("Consultant", PdfForms.ConsultantName, 901, 60.0, 690.0, "Tx"),
                new PdfFieldRead("Contract reference", PdfForms.ContractReference, 902, 60.0, 680.0, "Tx")
            };
        }
    }
}
