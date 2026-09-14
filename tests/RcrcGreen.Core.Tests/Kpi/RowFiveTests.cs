using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **Row 5 of the main sheet, measured by Bader on all seven templates on 14 September.**
    ///
    /// It is the measurement finding 50 asked for. Three of the seven had never been looked at,
    /// and the one that differs is FUTURE PARKS, which was one of the three:
    ///
    /// <code>
    /// HEALTHCARE, MOSQUES, PARKING, SCHOOLS, STREETS
    ///   B5 REF :   C5 the UID     D5 Date:   E5 the date
    ///   F5 Prepared By:           G5 a name  H5 a position
    ///
    /// EXISTING PARKS and FUTURE PARKS
    ///   B5 REF :   C5 EMPTY       D5 Date:   E5 EMPTY
    ///   F5 Prepared By:           G5 EMPTY   H5 holds the text " Architect Engineer"
    /// </code>
    ///
    /// **No formula sits at C5, E5, G5 or H5 on any of the seven**, so the letter map the tool
    /// wrote by never overwrote anything and no workbook is damaged. The letters were right on
    /// all seven BY LUCK, and what differs between the two sets is what those cells HOLD.
    ///
    /// No client workbook enters this public repository, so row 5 is rebuilt here from the cells
    /// Bader named, the way the row 7 layouts already are.
    /// </summary>
    public class RowFiveTests : IDisposable
    {
        private readonly string _folder = LabelFixture.Folder();

        public void Dispose()
        {
            try { Directory.Delete(_folder, true); }
            catch (IOException) { }
        }

        private const string ParkSheet = "<Park Name>";

        /// <summary>
        /// Row 5 as the five non park templates hold it, with the R1 set's own placeholders in
        /// the three cells the team fills and in the reference.
        /// </summary>
        private static Dictionary<string, string> FiveOfThem()
        {
            return new Dictionary<string, string>
            {
                { "B5", "REF :" }, { "C5", "<UID>" },
                { "D5", "Date:" }, { "E5", "<Date>" },
                { "F5", "Prepared By:" }, { "G5", "<Name>" }, { "H5", "<Position>" }
            };
        }

        /// <summary>
        /// Row 5 as BOTH park templates hold it. Three of the four value cells are empty and the
        /// fourth already holds a position somebody typed.
        /// </summary>
        private static Dictionary<string, string> BothParks()
        {
            return new Dictionary<string, string>
            {
                { "B5", "REF :" }, { "C5", string.Empty },
                { "D5", "Date:" }, { "E5", string.Empty },
                { "F5", "Prepared By:" }, { "G5", string.Empty }, { "H5", " Architect Engineer" }
            };
        }

        /// <summary>
        /// The same parks row with the three empty cells left out of the file altogether, which
        /// is the other thing an empty cell can be in an .xlsx. Both have to answer alike.
        /// </summary>
        private static Dictionary<string, string> BothParksWithNoCellAtAll()
        {
            return new Dictionary<string, string>
            {
                { "B5", "REF :" }, { "D5", "Date:" },
                { "F5", "Prepared By:" }, { "H5", " Architect Engineer" }
            };
        }

        private RecognisedWorkbook Listed(string sheetName, Dictionary<string, string> row, string fileName)
        {
            string path = LabelFixture.Create(_folder, sheetName, row, fileName);
            PeekedWorkbook peeked = PeekedWorkbook.Of(path);

            Assert.True(peeked.WasRead, peeked.Refusal);

            return RecognisedWorkbook.Recognise(
                Path.GetFileName(path), peeked.SheetNames, peeked.Refusal, peeked.FirstSheetCells);
        }

        /// <summary>
        /// **The question finding 50's measurement raised: the filled check reads E5, and E5 is
        /// empty on both park templates.** A clean EXISTING PARKS workbook has to be OFFERED in
        /// the templates list. Withholding it would leave the team unable to fill a park at all.
        /// </summary>
        [Fact]
        public void ACleanExistingParksTemplateIsOffered()
        {
            RecognisedWorkbook listed = Listed(
                ParkSheet, BothParks(), "GRP KPI Checklist - EXISTING PARKS.xlsx");

            Assert.True(listed.IsMatched);
            Assert.False(listed.IsFilled);
            Assert.Null(listed.DecidedBy);
            Assert.Equal("EXISTING PARKS", listed.Template.Name);
        }

        [Fact]
        public void ACleanFutureParksTemplateIsOffered()
        {
            RecognisedWorkbook listed = Listed(
                ParkSheet, BothParks(), "GRP KPI Checklist - FUTURE PARKS.xlsx");

            Assert.True(listed.IsMatched);
            Assert.False(listed.IsFilled);
            Assert.Equal("FUTURE PARKS", listed.Template.Name);
        }

        /// <summary>
        /// A cell left out of the file reads the same as one holding nothing, so which of the
        /// two a real template carries cannot change the answer.
        /// </summary>
        [Fact]
        public void AParksTemplateWithNoCellAtAllIsOfferedTheSameWay()
        {
            RecognisedWorkbook listed = Listed(
                ParkSheet, BothParksWithNoCellAtAll(), "GRP KPI Checklist - EXISTING PARKS.xlsx");

            Assert.True(listed.IsMatched);
            Assert.False(listed.IsFilled);
        }

        /// <summary>
        /// One of the other five, so the parks answer cannot be read as the check never firing.
        /// The R1 set's placeholders are not what the tool writes, so MOSQUES is offered too.
        /// </summary>
        [Fact]
        public void ACleanMosquesTemplateIsOffered()
        {
            RecognisedWorkbook listed = Listed(
                "<Mosques>", FiveOfThem(), "GRP KPI Checklist - MOSQUES.xlsx");

            Assert.True(listed.IsMatched);
            Assert.False(listed.IsFilled);
            Assert.Equal("MOSQUES", listed.Template.Name);
        }

        /// <summary>
        /// **The position cell is not empty on either park template and the tool writes it.**
        /// It holds a position already, so it cannot be a mark: the check would withhold every
        /// clean park template. It is not one, and this says so rather than leaving it to be
        /// read off the absence of a line.
        /// </summary>
        [Fact]
        public void TheParksPositionCellIsNotAMarkAndDoesNotWithholdTheTemplate()
        {
            RecognisedWorkbook listed = Listed(
                ParkSheet, BothParks(), "GRP KPI Checklist - EXISTING PARKS.xlsx");

            Assert.True(listed.IsMatched);
        }

        /// <summary>
        /// **CHECK YOUR WORK. The lookup has to land on C5, E5, G5 and H5 on all seven.**
        ///
        /// Every template's own main sheet, carrying the row 5 its set was measured to hold, and
        /// the four cells the three labels choose. If this ever fails, the labels are not what
        /// the measurement says and that is a finding rather than a thing to work around.
        /// </summary>
        [Theory]
        [InlineData("EXISTING PARKS", "<Park Name>", true)]
        [InlineData("FUTURE PARKS", "<Park Name>", true)]
        [InlineData("HEALTHCARE", "<Healthcare>", false)]
        [InlineData("MOSQUES", "<Mosques>", false)]
        [InlineData("PARKING", "<Parking Plots>", false)]
        [InlineData("SCHOOLS", "<Schools>", false)]
        [InlineData("STREETS", "<Streets>", false)]
        public void TheLabelsLandOnC5E5G5AndH5OnAllSeven(string name, string sheetName, bool parks)
        {
            KpiTemplate template = KpiTemplates.All.Single(one => one.Name == name);
            string path = LabelFixture.Create(
                _folder, sheetName, parks ? BothParks() : FiveOfThem(), name + ".xlsx");

            LabelledCells found = LabelledPlaces.In(path, template);

            Assert.True(found.Read, found.Why);
            Assert.Equal(sheetName, found.SheetName);

            Assert.Equal("B5", found.For("Reference").LabelCell);
            Assert.Equal("C5", found.For("Reference").ValueCell);
            Assert.Equal("D5", found.For("Date").LabelCell);
            Assert.Equal("E5", found.For("Date").ValueCell);
            Assert.Equal("F5", found.For("Prepared by").LabelCell);
            Assert.Equal("G5", found.For("Prepared by").ValueCell);
            Assert.Equal("F5", found.For("Position").LabelCell);
            Assert.Equal("H5", found.For("Position").ValueCell);

            Assert.All(
                new[] { "Reference", "Date", "Prepared by", "Position" },
                one => Assert.True(found.For(one).Found, one + " was not found on " + sheetName));
        }

        /// <summary>
        /// And the plan writes the four values into those four cells, on all seven, so the
        /// lookup landing right is checked against what really goes into the workbook rather
        /// than against the read alone.
        /// </summary>
        [Theory]
        [InlineData("EXISTING PARKS", "<Park Name>", true)]
        [InlineData("FUTURE PARKS", "<Park Name>", true)]
        [InlineData("HEALTHCARE", "<Healthcare>", false)]
        [InlineData("MOSQUES", "<Mosques>", false)]
        [InlineData("PARKING", "<Parking Plots>", false)]
        [InlineData("SCHOOLS", "<Schools>", false)]
        [InlineData("STREETS", "<Streets>", false)]
        public void ThePlanWritesTheFourValuesIntoThoseFourCellsOnAllSeven(string name, string sheetName, bool parks)
        {
            KpiTemplate template = KpiTemplates.All.Single(one => one.Name == name);
            string path = LabelFixture.Create(
                _folder, sheetName, parks ? BothParks() : FiveOfThem(), name + "-plan.xlsx");

            KpiCreatePlan plan = KpiCreatePlan.Of(
                template,
                null,
                new AgreedValue(new[] { new PlotText("DM-11", "ANH-007-MO-100019") }),
                string.Empty, null, null, null, null,
                "2026-09-14", "B RAHAL", "BIM COORDINATOR",
                CreateFixture.NoStreetFile,
                LabelledPlaces.In(path, template));

            Assert.Equal("ANH-007-MO-100019", Stored(plan, "C5"));
            Assert.Equal("2026-09-14", Stored(plan, "E5"));
            Assert.Equal("B RAHAL", Stored(plan, "G5"));
            Assert.Equal("BIM COORDINATOR", Stored(plan, "H5"));
        }

        private static string Stored(KpiCreatePlan plan, string cell)
        {
            CellWrite found = plan.Writes.SingleOrDefault(one => one.Cell.ToString() == cell);
            Assert.True(found != null, "no write landed on " + cell);
            return found.Stored;
        }

        /// <summary>
        /// **The two records of row 5 are held against each other.** The run finds its cells by
        /// label and the filled check reads letters, because it runs over a file whose template
        /// is not known yet. They must be the same cells, and this is what goes red when one
        /// of the two moves without the other.
        /// </summary>
        [Fact]
        public void TheCellsTheLabelsChooseAreTheCellsTheFilledCheckReads()
        {
            string path = LabelFixture.Create(_folder, "<Mosques>", FiveOfThem(), "agree.xlsx");
            LabelledCells found = LabelledPlaces.In(path, KpiTemplates.Mosques);

            Assert.Equal(FilledMarks.DateCell, found.For("Date").ValueCell);
            Assert.Equal(FilledMarks.ReferenceCell, found.For("Reference").ValueCell);
            Assert.Equal(new[] { "E5", "C5" }, FilledMarks.CellsRead);
        }

        /// <summary>
        /// **NO LABEL NAMES THE POSITION CELL, AND THAT IS THIS ROUND'S OWN FINDING.** Three
        /// labels reach three of the four cells. H5 is one further along from the person's name,
        /// under the same Prepared By, which both park templates confirm by holding a position
        /// there already. It is written as a distance of two and said out loud rather than
        /// dressed up as a label of its own.
        /// </summary>
        [Fact]
        public void ThePositionCellIsADistanceFromThePreparedByLabelAndNotALabelOfItsOwn()
        {
            Assert.Equal(
                new[] { 1, 1, 1, 2, 1, 1 },
                LabelledPlaces.All.Select(one => one.StepsRight).ToArray());

            LabelledPlace position = LabelledPlaces.Of("Position");
            Assert.Equal("Prepared By:", position.Label);
            Assert.Equal(2, position.StepsRight);

            LabelledPlace preparedBy = LabelledPlaces.Of("Prepared by");
            Assert.Equal(position.Label, preparedBy.Label);
            Assert.Equal(1, preparedBy.StepsRight);
        }

        /// <summary>
        /// One label naming two places means both refuse together when the sheet carries it
        /// twice, because the thing that cannot be resolved is the label they share.
        /// </summary>
        [Fact]
        public void APreparedByLabelOnTheSheetTwiceRefusesThePersonAndThePosition()
        {
            Dictionary<string, string> twice = FiveOfThem();
            twice["F9"] = "Prepared By:";

            string path = LabelFixture.Create(_folder, "<Mosques>", twice, "twice-row-five.xlsx");
            LabelledCells found = LabelledPlaces.In(path, KpiTemplates.Mosques);

            Assert.False(found.For("Prepared by").Found);
            Assert.False(found.For("Position").Found);
            Assert.Equal(
                "Prepared By: is on <Mosques> at F5 and F9, and nothing says which is meant",
                found.For("Position").Why);

            // The other two labels are unaffected, so one bad label does not cost the rest.
            Assert.Equal("C5", found.For("Reference").ValueCell);
            Assert.Equal("E5", found.For("Date").ValueCell);
        }

        /// <summary>
        /// **A cell this run writes that was not empty, and what it held.** Both park templates
        /// hold " Architect Engineer" at H5 already. The run writes over it, which is right, and
        /// the read carries what was there so the report can say so.
        /// </summary>
        [Fact]
        public void ThePositionCellOnAParkTemplateIsReadAsHoldingWhatItHolds()
        {
            string path = LabelFixture.Create(
                _folder, ParkSheet, BothParks(), "parks-holds.xlsx");

            LabelledCells found = LabelledPlaces.In(path, KpiTemplates.ExistingParks);

            Assert.Equal("Architect Engineer", found.For("Position").Holds);
            Assert.True(found.For("Position").WasNotEmpty);

            // The other three are empty on a park template, so none of them reads as written over.
            Assert.False(found.For("Reference").WasNotEmpty);
            Assert.False(found.For("Date").WasNotEmpty);
            Assert.False(found.For("Prepared by").WasNotEmpty);
        }

        /// <summary>
        /// A filled park workbook is still withheld, so the four cases above are the rule
        /// answering rather than the check being dead.
        /// </summary>
        [Fact]
        public void AFilledParksWorkbookIsStillWithheld()
        {
            Dictionary<string, string> written = BothParks();
            written["C5"] = "ANH-007-MO-100019";
            written["E5"] = "2026-09-14";

            RecognisedWorkbook listed = Listed(ParkSheet, written, "EXISTING PARKS EP-05.xlsx");

            Assert.True(listed.IsFilled);
            Assert.False(listed.IsMatched);
            Assert.Equal("E5", listed.DecidedBy.Mark.Cell);
            Assert.Equal("2026-09-14", listed.DecidedBy.Holds);
        }
    }
}
