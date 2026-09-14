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
    /// **EVERY TEMPLATE CARRIES A SECOND BLOCK OF THE SAME SHAPE**, measured on all seven:
    ///
    /// <code>
    /// row  5   D5  Date:    E5  the date   F5  Prepared By:   G5  a name   H5  a position
    /// row 28   D28 Date:    E28 the date   F28 Reviewed By:   G28 a name   H28 a position
    /// </code>
    ///
    /// at row 28 on HEALTHCARE, MOSQUES, PARKING and SCHOOLS, and at row 29 on EXISTING PARKS,
    /// FUTURE PARKS and STREETS. One word apart. So `Date:` is on every template TWICE and the
    /// preparer's own label is the only thing that separates the two blocks, which is why the
    /// date is looked for on the preparer's row rather than over the sheet.
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
        /// The second block every template carries, the same shape one word apart. Row 28 on
        /// HEALTHCARE, MOSQUES, PARKING and SCHOOLS, row 29 on the two parks and STREETS.
        /// </summary>
        private static Dictionary<string, string> AndTheReviewerBlock(
            Dictionary<string, string> sheet, int row)
        {
            sheet["D" + row] = "Date:";
            sheet["E" + row] = "<Date>";
            sheet["F" + row] = "Reviewed By:";
            sheet["G" + row] = "<Name>";
            sheet["H" + row] = "<Position>";
            return sheet;
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
        [InlineData("EXISTING PARKS", "<Park Name>", true, 29)]
        [InlineData("FUTURE PARKS", "<Park Name>", true, 29)]
        [InlineData("HEALTHCARE", "<Healthcare>", false, 28)]
        [InlineData("MOSQUES", "<Mosques>", false, 28)]
        [InlineData("PARKING", "<Parking Plots>", false, 28)]
        [InlineData("SCHOOLS", "<Schools>", false, 28)]
        [InlineData("STREETS", "<Streets>", false, 29)]
        public void TheLabelsLandOnC5E5G5AndH5OnAllSeven(
            string name, string sheetName, bool parks, int reviewerRow)
        {
            KpiTemplate template = KpiTemplates.All.Single(one => one.Name == name);
            string path = LabelFixture.Create(
                _folder, sheetName,
                AndTheReviewerBlock(parks ? BothParks() : FiveOfThem(), reviewerRow),
                name + ".xlsx");

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

            // **Nothing lands in the reviewer's block.** Its Date: is the second one on the
            // sheet and the whole reason the date is looked for on the preparer's row.
            Assert.DoesNotContain(found.All, one => one.LabelCell == "D" + reviewerRow);
            Assert.DoesNotContain(found.All, one => one.ValueCell == "E" + reviewerRow);
        }

        /// <summary>
        /// And the plan writes the four values into those four cells, on all seven, so the
        /// lookup landing right is checked against what really goes into the workbook rather
        /// than against the read alone.
        /// </summary>
        [Theory]
        [InlineData("EXISTING PARKS", "<Park Name>", true, 29)]
        [InlineData("FUTURE PARKS", "<Park Name>", true, 29)]
        [InlineData("HEALTHCARE", "<Healthcare>", false, 28)]
        [InlineData("MOSQUES", "<Mosques>", false, 28)]
        [InlineData("PARKING", "<Parking Plots>", false, 28)]
        [InlineData("SCHOOLS", "<Schools>", false, 28)]
        [InlineData("STREETS", "<Streets>", false, 29)]
        public void ThePlanWritesTheFourValuesIntoThoseFourCellsOnAllSeven(
            string name, string sheetName, bool parks, int reviewerRow)
        {
            KpiTemplate template = KpiTemplates.All.Single(one => one.Name == name);
            string path = LabelFixture.Create(
                _folder, sheetName,
                AndTheReviewerBlock(parks ? BothParks() : FiveOfThem(), reviewerRow),
                name + "-plan.xlsx");

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

            // **The reviewer's block is never written into.** A date landing there would be a
            // wrong number in a client file that nobody reading the preparer's row would see.
            Assert.DoesNotContain(plan.Writes, one => one.Cell.ToString() == "E" + reviewerRow);
            Assert.DoesNotContain(plan.Writes, one => one.Cell.ToString() == "G" + reviewerRow);
            Assert.DoesNotContain(plan.Writes, one => one.Cell.ToString() == "H" + reviewerRow);
        }

        private static string Stored(KpiCreatePlan plan, string cell)
        {
            CellWrite found = plan.Writes.SingleOrDefault(one => one.Cell.ToString() == cell);
            Assert.True(found != null, "no write landed on " + cell);
            return found.Stored;
        }

        /// <summary>
        /// **THE FAULT THIS ROUND FOUND, WRITTEN AS A TEST.** `Date:` is on every template twice
        /// and the seventieth pass looked for it over the whole sheet, so the found twice guard
        /// fired on all seven and NO DATE WAS WRITTEN ANYWHERE. It is looked for on the row the
        /// preparer's own label sits on now, and this is the case that was red.
        /// </summary>
        [Fact]
        public void TheDateIsFoundOnThePreparersRowAndNeverTheReviewers()
        {
            string path = LabelFixture.Create(
                _folder, "<Mosques>", AndTheReviewerBlock(FiveOfThem(), 28), "two-blocks.xlsx");

            LabelledCells found = LabelledPlaces.In(path, KpiTemplates.Mosques);

            LabelledCell date = found.For("Date");
            Assert.True(date.Found, date.Why);
            Assert.Equal("D5", date.LabelCell);
            Assert.Equal("E5", date.ValueCell);

            // The other three are unmoved, so the anchor did not cost anything else.
            Assert.Equal("C5", found.For("Reference").ValueCell);
            Assert.Equal("G5", found.For("Prepared by").ValueCell);
            Assert.Equal("H5", found.For("Position").ValueCell);
        }

        /// <summary>
        /// **The row is chosen by the preparer's label and never by being first.** A sheet whose
        /// reviewer block sits above the preparer's answers with the preparer's row, which is
        /// what tells this rule apart from one that takes the topmost Date:.
        /// </summary>
        [Fact]
        public void WhenTheReviewerBlockComesFirstTheDateStillFollowsThePreparer()
        {
            string path = LabelFixture.Create(
                _folder, "<Mosques>",
                new Dictionary<string, string>
                {
                    { "D5", "Date:" }, { "E5", "<Date>" },
                    { "F5", "Reviewed By:" }, { "G5", "<Name>" }, { "H5", "<Position>" },
                    { "B28", "REF :" }, { "C28", "<UID>" },
                    { "D28", "Date:" }, { "E28", "<Date>" },
                    { "F28", "Prepared By:" }, { "G28", "<Name>" }, { "H28", "<Position>" }
                },
                "reviewer-first.xlsx");

            LabelledCells found = LabelledPlaces.In(path, KpiTemplates.Mosques);

            Assert.Equal("D28", found.For("Date").LabelCell);
            Assert.Equal("E28", found.For("Date").ValueCell);
            Assert.Equal("G28", found.For("Prepared by").ValueCell);
            Assert.Equal("H28", found.For("Position").ValueCell);
        }

        /// <summary>
        /// The found twice guard still fires inside the row it is allowed to look on, and its
        /// reason says which row that was and why, so a person can check the answer against the
        /// sheet rather than taking the row on trust.
        /// </summary>
        [Fact]
        public void TwoDateLabelsOnThePreparersOwnRowRefuseAndNameBoth()
        {
            Dictionary<string, string> twice = AndTheReviewerBlock(FiveOfThem(), 28);
            twice["A5"] = "Date:";

            string path = LabelFixture.Create(_folder, "<Mosques>", twice, "date-twice-row-5.xlsx");
            LabelledCells found = LabelledPlaces.In(path, KpiTemplates.Mosques);

            Assert.False(found.For("Date").Found);
            Assert.Equal(
                "Date: is on <Mosques> row 5, the row Prepared By: sits on, at A5 and D5, "
                + "and nothing says which is meant",
                found.For("Date").Why);
        }

        /// <summary>
        /// A preparer's row carrying no Date: at all says so, naming the row it looked on, and
        /// never reaches down to the reviewer's copy.
        /// </summary>
        [Fact]
        public void APreparersRowWithNoDateLabelSaysWhichRowItLookedOn()
        {
            Dictionary<string, string> missing = AndTheReviewerBlock(FiveOfThem(), 28);
            missing.Remove("D5");

            string path = LabelFixture.Create(_folder, "<Mosques>", missing, "no-date-row-5.xlsx");
            LabelledCells found = LabelledPlaces.In(path, KpiTemplates.Mosques);

            Assert.False(found.For("Date").Found);
            Assert.Equal(
                "no cell on <Mosques> row 5, the row Prepared By: sits on, reads Date:, "
                + "so nothing is written and no cell is guessed at",
                found.For("Date").Why);
        }

        /// <summary>
        /// **Every anchor names a place that exists and is not itself anchored.** One level, on
        /// purpose: a chain of rows would be a rule nobody can check against a sheet by eye.
        /// </summary>
        [Fact]
        public void EveryAnchorNamesAPlaceThatIsNotItselfAnchored()
        {
            Assert.Equal(
                new[] { "Date" },
                LabelledPlaces.All.Where(one => one.IsAnchored).Select(one => one.Name).ToArray());

            foreach (LabelledPlace place in LabelledPlaces.All.Where(one => one.IsAnchored))
            {
                LabelledPlace anchor = LabelledPlaces.Of(place.OnTheRowOf);
                Assert.True(anchor != null, place.Name + " is anchored to a place that is not in the table");
                Assert.False(anchor.IsAnchored, place.Name + " is anchored to a place that is itself anchored");
            }
        }

        /// <summary>
        /// **MEASURED ON ALL SEVEN: NO LABEL NAMES THE POSITION CELL.** The only cells whose
        /// text names a position hold the placeholder being replaced, not a label, so the
        /// distance of two from Prepared By: stays and nothing anywhere looks for the word.
        /// This is what closes the UNKNOWN the seventieth pass left open.
        /// </summary>
        [Fact]
        public void NothingLooksForTheWordPositionAndThePlaceholderIsNotALabel()
        {
            Assert.DoesNotContain(
                LabelledPlaces.All,
                one => one.Label.IndexOf("Position", StringComparison.OrdinalIgnoreCase) >= 0);

            string path = LabelFixture.Create(
                _folder, "<Mosques>", AndTheReviewerBlock(FiveOfThem(), 28), "placeholder.xlsx");

            LabelledCells found = LabelledPlaces.In(path, KpiTemplates.Mosques);

            // H5 and H28 both hold <Position>. The one this run writes is chosen by the distance
            // from the preparer's label, and the placeholder in it decides nothing.
            Assert.Equal("H5", found.For("Position").ValueCell);
            Assert.Equal("Prepared By:", found.For("Position").Label);
            Assert.Equal("<Position>", found.For("Position").Holds);
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
        ///
        /// **The date goes with them**, and that is the anchor working rather than a side
        /// effect: a sheet where nothing can say which block is the preparer's cannot say which
        /// row the date sits on either, and the reason says exactly that rather than repeating
        /// the anchor's own words.
        /// </summary>
        [Fact]
        public void APreparedByLabelOnTheSheetTwiceRefusesThePersonThePositionAndTheDate()
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

            Assert.False(found.For("Date").Found);
            Assert.Equal(
                "there is no row to look for Date: on, because Prepared By: was not found: "
                + "Prepared By: is on <Mosques> at F5 and F9, and nothing says which is meant",
                found.For("Date").Why);

            // The reference names its own label, so one bad label does not cost it.
            Assert.Equal("C5", found.For("Reference").ValueCell);
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
