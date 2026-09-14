using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// What unit every field on all three forms asks for, and the position that says which row a
    /// field really sits on.
    ///
    /// **A UNIT THAT MATCHES BY LUCK READS THE SAME AS ONE NOBODY CHECKED**, so the table below
    /// is written out by hand, field by field, off the three files as they were measured on 14
    /// September, rather than read back out of the code it is checking.
    ///
    /// **AND THE POSITION IS THE THIRD RECORD.** The field name and the note were both checked
    /// and the position was not, and a field that moves to another row keeps both while meaning
    /// something else. On the open spaces form every shrub row carries two boxes, a quantity and
    /// an area, so a field slipping one column would take a number into the wrong box with its
    /// name and its note intact.
    /// </summary>
    public class PdfUnitsTests
    {
        private static readonly DateTime Today = new DateTime(2026, 9, 14);

        private static GroupSubtotal Shrubs(params PhaseSubtotal[] phases)
        {
            return new GroupSubtotal(KpiMerge.ShrubsHeading, 0.0, 0, phases: phases);
        }

        private static PhaseSubtotal Phase(string name, double squareMetres)
        {
            return new PhaseSubtotal(name, 0, squareMetres, 0, true, string.Empty);
        }

        private static string Wrote(string plotId, KpiTemplate template, PdfValue value)
        {
            PdfPlan plan = PdfFill.Of(
                CreateFixture.Plot(plotId, uid2: "ANH-008-MO-100006", subtotals: new[]
                {
                    Shrubs(Phase(CreateFixture.Existing, 30.0), Phase(CreateFixture.Proposed, 54.0))
                }),
                CountedGroups.Of(template), null, Today, true, PdfWorkbookNumbers.None);

            return plan.Fields.Single(one => one.Value == value).Text;
        }

        /// <summary>
        /// **THE TOTAL SHRUBS NOTE IS WRONG ON THE PARKS FORM AND THE POSITION IS WHAT SHOWS
        /// IT.** Its tooltip reads Existing Shrubs on the row the page prints TOTAL Shrubs Area
        /// (m²), so a tool matching by note would put the existing area into a box labelled TOTAL
        /// in a client document. Bader confirmed with the client on 14 September that all three
        /// forms mean the sum of the two above, and this is what the tool writes on all three.
        /// DM-16 prints 30 existing and 54 proposed and 30 plus 54 is 84.
        /// </summary>
        [Fact]
        public void TotalShrubsIsExistingPlusProposedOnAllThreeForms()
        {
            Assert.Equal("84", Wrote("EP-05", KpiTemplates.ExistingParks, PdfValue.TotalShrubs));
            Assert.Equal("84", Wrote("DM-16", KpiTemplates.Mosques, PdfValue.TotalShrubs));
            Assert.Equal("84", Wrote("ST-05", KpiTemplates.Streets, PdfValue.TotalShrubs));
        }

        /// <summary>
        /// **And the three shrub fields run DOWN the page in the order the page prints them**, on
        /// all three forms: existing, then proposed, then the total. That is the record the note
        /// could not be trusted for, and it is why the position is kept beside the name.
        /// </summary>
        [Fact]
        public void TheThreeShrubFieldsRunDownThePageInThatOrderOnAllThreeForms()
        {
            foreach (PdfForm form in PdfForms.All)
            {
                double existing = form.FieldFor(PdfValue.ExistingShrubs).Y;
                double proposed = form.FieldFor(PdfValue.ProposedShrubs).Y;
                double total = form.FieldFor(PdfValue.TotalShrubs).Y;

                Assert.True(existing > proposed, form.Name + " prints existing above proposed");
                Assert.True(proposed > total, form.Name + " prints proposed above the total");
            }
        }

        /// <summary>
        /// Every field this tool fills names the unit the form asks for. **A field with no unit
        /// is a number nobody can check**, and an empty one would read as a unit that matches.
        /// </summary>
        [Fact]
        public void EveryFieldOnEveryFormNamesTheUnitTheFormAsksFor()
        {
            foreach (PdfForm form in PdfForms.All)
            {
                foreach (PdfFormField field in form.Fields)
                {
                    Assert.False(string.IsNullOrWhiteSpace(field.Unit),
                        form.Name + " " + field.FieldName + " names no unit");
                }
            }
        }

        /// <summary>
        /// The whole table, written out by hand. **Every field on all three forms**, what unit it
        /// asks for, and the ones that need no conversion named beside the two that do.
        /// </summary>
        [Theory]
        // Parks: EP and FP.
        [InlineData("Projects Basic Data - Parks", PdfValue.Uid, "text")]
        [InlineData("Projects Basic Data - Parks", PdfValue.ReportDate, "a date")]
        [InlineData("Projects Basic Data - Parks", PdfValue.Area, "m2")]
        [InlineData("Projects Basic Data - Parks", PdfValue.TotalAreasToBeGreened, "km2")]
        [InlineData("Projects Basic Data - Parks", PdfValue.PercentageCanopy, "%")]
        [InlineData("Projects Basic Data - Parks", PdfValue.IrrigationWaterDemand, "m3 a day")]
        [InlineData("Projects Basic Data - Parks", PdfValue.ExistingTrees, "a count")]
        [InlineData("Projects Basic Data - Parks", PdfValue.ProposedTrees, "a count")]
        [InlineData("Projects Basic Data - Parks", PdfValue.TotalTrees, "a count")]
        [InlineData("Projects Basic Data - Parks", PdfValue.ExistingShrubs, "m2")]
        [InlineData("Projects Basic Data - Parks", PdfValue.ProposedShrubs, "m2")]
        [InlineData("Projects Basic Data - Parks", PdfValue.TotalShrubs, "m2")]
        [InlineData("Projects Basic Data - Parks", PdfValue.GroundCover, "m2")]
        [InlineData("Projects Basic Data - Parks", PdfValue.Lawn, "m2")]
        // Open spaces: HF, FM, DM, PL and SC.
        [InlineData("Projects Basic Data - Open spaces associated to buildings", PdfValue.ProjectType, "text")]
        [InlineData("Projects Basic Data - Open spaces associated to buildings", PdfValue.Uid, "text")]
        [InlineData("Projects Basic Data - Open spaces associated to buildings", PdfValue.ReportDate, "a date")]
        [InlineData("Projects Basic Data - Open spaces associated to buildings", PdfValue.Area, "m2")]
        [InlineData("Projects Basic Data - Open spaces associated to buildings", PdfValue.TotalAreasToBeGreened, "km2")]
        [InlineData("Projects Basic Data - Open spaces associated to buildings", PdfValue.IrrigationWaterDemand, "m3 a day")]
        [InlineData("Projects Basic Data - Open spaces associated to buildings", PdfValue.ExistingTrees, "a count")]
        [InlineData("Projects Basic Data - Open spaces associated to buildings", PdfValue.ProposedTrees, "a count")]
        [InlineData("Projects Basic Data - Open spaces associated to buildings", PdfValue.TotalTrees, "a count")]
        [InlineData("Projects Basic Data - Open spaces associated to buildings", PdfValue.ExistingShrubs, "m2")]
        [InlineData("Projects Basic Data - Open spaces associated to buildings", PdfValue.ProposedShrubs, "m2")]
        [InlineData("Projects Basic Data - Open spaces associated to buildings", PdfValue.TotalShrubs, "m2")]
        [InlineData("Projects Basic Data - Open spaces associated to buildings", PdfValue.GroundCover, "m2")]
        [InlineData("Projects Basic Data - Open spaces associated to buildings", PdfValue.Lawn, "m2")]
        // Roads: NS, ST and MM.
        [InlineData("Projects Basic Data - Roads", PdfValue.Uid, "text")]
        [InlineData("Projects Basic Data - Roads", PdfValue.ReportDate, "a date")]
        [InlineData("Projects Basic Data - Roads", PdfValue.Row, "m")]
        [InlineData("Projects Basic Data - Roads", PdfValue.Length, "km")]
        [InlineData("Projects Basic Data - Roads", PdfValue.TotalAreasToBeGreened, "km2")]
        [InlineData("Projects Basic Data - Roads", PdfValue.IrrigationWaterDemand, "m3 a day")]
        [InlineData("Projects Basic Data - Roads", PdfValue.ExistingTrees, "a count")]
        [InlineData("Projects Basic Data - Roads", PdfValue.ProposedTrees, "a count")]
        [InlineData("Projects Basic Data - Roads", PdfValue.TotalTrees, "a count")]
        [InlineData("Projects Basic Data - Roads", PdfValue.ExistingShrubs, "m2")]
        [InlineData("Projects Basic Data - Roads", PdfValue.ProposedShrubs, "m2")]
        [InlineData("Projects Basic Data - Roads", PdfValue.TotalShrubs, "m2")]
        [InlineData("Projects Basic Data - Roads", PdfValue.GroundCover, "m2")]
        [InlineData("Projects Basic Data - Roads", PdfValue.Lawn, "m2")]
        public void TheUnitTableIsWhatEachFormAsksFor(string formName, PdfValue value, string unit)
        {
            PdfForm form = PdfForms.All.Single(one => one.Name == formName);

            Assert.Equal(unit, form.FieldFor(value).Unit);
        }

        /// <summary>
        /// And the table covers every field the three forms hold, so a field added without a line
        /// above is a red test rather than a unit nobody checked. 14 on parks, 14 on open spaces
        /// and 14 on roads, counted by hand off the three.
        /// </summary>
        [Fact]
        public void TheTableCoversEveryFieldOnAllThreeForms()
        {
            Assert.Equal(14, PdfForms.Parks.Fields.Count);
            Assert.Equal(14, PdfForms.OpenSpaces.Fields.Count);
            Assert.Equal(14, PdfForms.Roads.Fields.Count);
            Assert.Equal(42, PdfForms.All.Sum(one => one.Fields.Count));
        }

        /// <summary>
        /// **A FIELD THAT HAS MOVED ON THE PAGE WRITES NOTHING AND IS NAMED**, with where it sits
        /// beside where this tool measured it. The name and the note both still hold in this
        /// case, which is exactly the shape the position was added for.
        /// </summary>
        [Fact]
        public void AFieldThatHasMovedOnThePageIsNamedAndNothingIsWritten()
        {
            PdfForm form = PdfForms.Parks;

            var fields = form.Fields
                .Select(one => new PdfFieldRead(
                    one.FieldName, one.Note, 1,
                    one.X,
                    one.Value == PdfValue.TotalShrubs ? one.Y - 20.4 : one.Y))
                .ToList();

            PdfFormCheck check = PdfFormCheck.Of(form, fields, string.Empty);

            Assert.False(check.Matched);
            Assert.Contains(
                "fields that have moved on the page: TOTAL Shrubs Area (m²) sits at x 519.2, y 539.9 "
                + "where this tool measured it at x 519.2, y 560.3",
                check.Why);
        }

        /// <summary>
        /// **A field within the tolerance is not a field that has moved.** The rows on these
        /// forms sit about twenty points apart and a viewer rounds a rectangle, so half a point
        /// is room for the rounding and nothing near a row.
        /// </summary>
        [Fact]
        public void AFieldWithinTheToleranceIsNotAFieldThatHasMoved()
        {
            PdfForm form = PdfForms.Parks;

            var fields = form.Fields
                .Select(one => new PdfFieldRead(one.FieldName, one.Note, 1, one.X + 0.4, one.Y - 0.4))
                .ToList();

            PdfFormCheck check = PdfFormCheck.Of(form, fields, string.Empty);

            Assert.True(check.Matched, check.Why);
            Assert.Equal(0.5, PdfFormCheck.Tolerance);
        }
    }
}
