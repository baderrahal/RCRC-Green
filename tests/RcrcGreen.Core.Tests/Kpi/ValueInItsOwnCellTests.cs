using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **One assertion per value against the cell it belongs in.**
    ///
    /// The plan test beside this one asserts the cell references in order and the sheet name on
    /// each, and never which value went into which cell. Proved by the audit: swapping the
    /// shrubs and lawn totals on their way to their cells in <see cref="KpiCreatePlan"/> left
    /// the suite green. Those two are adjacent cells holding similar magnitudes, so nobody
    /// catches it by eye on the workbook either.
    ///
    /// Every value here is a different number or a different word, so a pair that trades places
    /// reddens the case that names the pair rather than sliding past a count.
    /// </summary>
    public sealed class ValueInItsOwnCellTests
    {
        private const string ParkSheet = "<Park Name>";
        private const string StreetSheet = "<Streets>";

        // Deliberately unalike. Two totals of the same size are what let a swap through.
        private const double AreaMetres = 3728.757;
        private const double ShrubsMetres = 84.0;
        private const double LawnMetres = 512.0;
        private const double RoadWidthMetres = 36.0;
        private const double TotalLengthMetres = 928.782391;

        private static string Stored(KpiCreatePlan plan, string cell)
        {
            CellWrite found = plan.Writes.SingleOrDefault(one => one.Cell.ToString() == cell);
            Assert.True(found != null, "no write landed on " + cell);
            return found.Stored;
        }

        /// <summary>
        /// Row 5 as BOTH park templates hold it, measured on 14 September. **H5 already holds a
        /// position**, " Architect Engineer", which the run writes over, so the fixture carries
        /// it rather than a blank.
        ///
        /// **Row 7 on a park template has never been measured**, so this names no label for
        /// Character or Context and the plan writes neither, which is what a park run really
        /// does until somebody opens one and looks.
        /// </summary>
        private static LabelledCells ParkLabels()
        {
            return LabelledCells.Holding(
                ParkSheet,
                new[]
                {
                    LabelledCell.At("Reference", "REF :", "B5", "C5", string.Empty),
                    LabelledCell.At("Date", "Date:", "D5", "E5", string.Empty),
                    LabelledCell.At("Prepared by", "Prepared By:", "F5", "G5", string.Empty),
                    LabelledCell.At("Position", "Prepared By:", "F5", "H5", " Architect Engineer")
                });
        }

        private static KpiCreatePlan ParkPlan()
        {
            return KpiCreatePlan.Of(
                KpiTemplates.ExistingParks,
                new AgreedValue(new[] { new PlotText("DM-11", "EXISTING PARK") }),
                new AgreedValue(new[] { new PlotText("DM-11", "ANH-007-MO-100019") }),
                "KING FAHD",
                Totalled.Adding(new[] { new PlotNumber("DM-11", AreaMetres) }),
                Totalled.Adding(new[] { new PlotNumber("DM-11", ShrubsMetres) }),
                Totalled.Adding(new[] { new PlotNumber("DM-11", LawnMetres) }),
                null,
                "2026-09-14", "B RAHAL", "BIM COORDINATOR",
                CreateFixture.NoStreetFile, ParkLabels());
        }

        /// <summary>
        /// The three the team types on the pane. They went out once holding the template's own
        /// placeholders while the report said nobody typed them, so each is bound to its cell
        /// by name here rather than counted. **The cells are the ones the labels chose** rather
        /// than three letters this tool holds.
        /// </summary>
        [Fact]
        public void TheDateGoesInE5AndNowhereElse()
        {
            Assert.Equal("2026-09-14", Stored(ParkPlan(), "E5"));
        }

        [Fact]
        public void ThePreparedByGoesInG5AndNowhereElse()
        {
            Assert.Equal("B RAHAL", Stored(ParkPlan(), "G5"));
        }

        [Fact]
        public void ThePositionGoesInH5AndNowhereElse()
        {
            Assert.Equal("BIM COORDINATOR", Stored(ParkPlan(), "H5"));
        }

        [Fact]
        public void TheComponentGoesInD3AndNowhereElse()
        {
            Assert.Equal("EXISTING PARK", Stored(ParkPlan(), "D3"));
        }

        [Fact]
        public void TheReferenceGoesInC5AndNowhereElse()
        {
            Assert.Equal("ANH-007-MO-100019", Stored(ParkPlan(), "C5"));
        }

        [Fact]
        public void TheLocationGoesInE4AndNowhereElse()
        {
            Assert.Equal("KING FAHD", Stored(ParkPlan(), "E4"));
        }

        [Fact]
        public void TheAreaGoesInD8AndNowhereElse()
        {
            Assert.Equal("3728.757", Stored(ParkPlan(), "D8"));
        }

        /// <summary>
        /// **The pair the audit swapped.** F11 is shrubs and H11 is lawn, and each names its
        /// own number, so a swap reddens both of these rather than neither.
        /// </summary>
        [Fact]
        public void TheShrubsTotalGoesInF11AndNowhereElse()
        {
            Assert.Equal("84", Stored(ParkPlan(), "F11"));
        }

        [Fact]
        public void TheLawnTotalGoesInH11AndNowhereElse()
        {
            Assert.Equal("512", Stored(ParkPlan(), "H11"));
        }

        /// <summary>
        /// Every value at once, so a pair that trades places cannot be hidden by a case that
        /// reads only one of them.
        /// </summary>
        [Fact]
        public void EveryValueOnAParkPlanSitsInItsOwnCell()
        {
            KpiCreatePlan plan = ParkPlan();

            Assert.Equal(
                new[]
                {
                    "E5=2026-09-14",
                    "G5=B RAHAL",
                    "H5=BIM COORDINATOR",
                    "D3=EXISTING PARK",
                    "C5=ANH-007-MO-100019",
                    "E4=KING FAHD",
                    "D8=3728.757",
                    "F11=84",
                    "H11=512"
                },
                plan.Writes.Select(one => one.Cell + "=" + one.Stored).ToArray());
        }

        private static KpiCreatePlan StreetPlan()
        {
            // Row 5 as the five non park templates hold it, and row 7 as STREETS holds it,
            // one pair right of where the other two carry it because of its Category formula.
            var labels = LabelledCells.Holding(
                StreetSheet,
                new[]
                {
                    LabelledCell.At("Reference", "REF :", "B5", "C5", "<UID>"),
                    LabelledCell.At("Date", "Date:", "D5", "E5", "<Date>"),
                    LabelledCell.At("Prepared by", "Prepared By:", "F5", "G5", "<Name>"),
                    LabelledCell.At("Position", "Prepared By:", "F5", "H5", "<Position>"),
                    LabelledCell.At(FixedCells.CharacterLabel, FixedCells.CharacterLabel, "E7", "F7", string.Empty),
                    LabelledCell.At(FixedCells.ContextLabel, FixedCells.ContextLabel, "G7", "H7", string.Empty)
                });

            return KpiCreatePlan.Of(
                KpiTemplates.Streets,
                new AgreedValue(new[] { new PlotText("ST-05", "STREET 36m ROW") }),
                new AgreedValue(new[] { new PlotText("ST-05", "ANH-007-ST-100217") }),
                "KING FAHD",
                Totalled.Nothing,
                Totalled.Adding(new[] { new PlotNumber("ST-05", ShrubsMetres) }),
                Totalled.Adding(new[] { new PlotNumber("ST-05", LawnMetres) }),
                null,
                "2026-09-14", "B RAHAL", "BIM COORDINATOR",
                StreetReferenceAnswer.Of(RoadWidthMetres, TotalLengthMetres, "36", "928.782391"),
                labels);
        }

        /// <summary>
        /// **The two street cells, which are a pair of numbers on one row.** D8 is the road
        /// width and F8 the total length, and the workbook multiplies them itself, so a swap
        /// would still produce an area and the wrong one.
        /// </summary>
        [Fact]
        public void TheRoadWidthGoesInD8AndTheTotalLengthInF8()
        {
            KpiCreatePlan plan = StreetPlan();

            Assert.Equal("36", Stored(plan, "D8"));
            Assert.Equal("928.782391", Stored(plan, "F8"));
        }

        /// <summary>
        /// **The two fixed cells, found by their labels.** Character sits right of its label and
        /// Context right of its own, and on STREETS they are one pair further along than on the
        /// other templates, so the values are bound to the cells the labels chose.
        /// </summary>
        [Fact]
        public void CharacterGoesRightOfItsLabelAndContextRightOfItsOwn()
        {
            KpiCreatePlan plan = StreetPlan();

            Assert.Equal("Urban Area Zone", Stored(plan, "F7"));
            Assert.Equal("Urban", Stored(plan, "H7"));
        }

        /// <summary>
        /// D7 is the STREETS Category, which holds a formula. Nothing this tool writes may land
        /// on it, and the only guard against that is that the values are placed off the labels.
        /// </summary>
        [Fact]
        public void NothingIsWrittenIntoTheStreetsCategoryCell()
        {
            Assert.DoesNotContain(StreetPlan().Writes, one => one.Cell.ToString() == "D7");
        }

        [Fact]
        public void EveryValueOnAStreetPlanSitsInItsOwnCell()
        {
            KpiCreatePlan plan = StreetPlan();

            Assert.Equal(
                new[]
                {
                    "E5=2026-09-14",
                    "G5=B RAHAL",
                    "H5=BIM COORDINATOR",
                    "D3=STREET 36m ROW",
                    "C5=ANH-007-ST-100217",
                    "E4=KING FAHD",
                    "F11=84",
                    "H11=512",
                    "D8=36",
                    "F8=928.782391",
                    "F7=Urban Area Zone",
                    "H7=Urban"
                },
                plan.Writes.Select(one => one.Cell + "=" + one.Stored).ToArray());
        }
    }
}
