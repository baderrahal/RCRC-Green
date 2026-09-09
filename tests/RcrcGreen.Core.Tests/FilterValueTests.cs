using System;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// A filter value goes back as the kind it came out as.
    ///
    /// Two schedules were refused with "the filter value is not valid for the field and filter
    /// type". Both filter on PRX_Included In Budget equals Yes. That is a Yes/No parameter,
    /// which Revit holds as the integer 1, and every captured value was being flattened to text
    /// and handed back as a string.
    ///
    /// Every expected value here is written out by hand.
    /// </summary>
    public class FilterValueTests
    {
        [Fact]
        public void TextGoesBackAsText()
        {
            FilterValue value = FilterValue.Text("HARDSCAPE");

            Assert.Equal(FilterValueKind.Text, value.Kind);
            Assert.Equal("HARDSCAPE", value.AsText);
        }

        /// <summary>
        /// The one that lost two schedules. Yes is 1 and No is 0, because Revit stores a Yes/No
        /// parameter as an integer and there is no such filter value as the word.
        /// </summary>
        [Fact]
        public void AYesOrNoIsAWholeNumber()
        {
            FilterValue yes = FilterValue.OfWholeNumber(1);

            Assert.Equal(FilterValueKind.WholeNumber, yes.Kind);
            Assert.Equal(1L, yes.WholeNumber);
            Assert.Equal("1", yes.AsText);

            Assert.Equal("0", FilterValue.OfWholeNumber(0).AsText);
        }

        [Fact]
        public void ANumberKeepsItsFraction()
        {
            FilterValue value = FilterValue.OfNumber(12.5);

            Assert.Equal(FilterValueKind.Number, value.Kind);
            Assert.Equal(12.5, value.Number);
            Assert.Equal("12.5", value.AsText);
        }

        [Fact]
        public void AnElementReferenceKeepsItsId()
        {
            FilterValue value = FilterValue.OfElementReference(918273L);

            Assert.Equal(FilterValueKind.ElementReference, value.Kind);
            Assert.Equal(918273L, value.WholeNumber);
            Assert.Equal("918273", value.AsText);
        }

        /// <summary>
        /// All four, out to text and back, because a definition written to a file and loaded
        /// again has to rebuild the same filter rather than four strings.
        /// </summary>
        [Fact]
        public void EveryKindSurvivesTheRoundTrip()
        {
            Back(FilterValueKind.Text, "DM-11");
            Back(FilterValueKind.WholeNumber, "1");
            Back(FilterValueKind.Number, "12.5");
            Back(FilterValueKind.ElementReference, "918273");
        }

        private static void Back(FilterValueKind kind, string text)
        {
            FilterValue read;
            Assert.True(FilterValue.TryParse(kind, text, out read),
                kind + " could not be read back from " + text);

            Assert.Equal(kind, read.Kind);
            Assert.Equal(text, read.AsText);
        }

        /// <summary>
        /// Reading fails rather than falling back to text, because a filter quietly downgraded
        /// to a string is exactly what this type exists to stop.
        /// </summary>
        [Fact]
        public void TextThatIsNotThatKindIsRefusedRatherThanDowngraded()
        {
            FilterValue read;

            Assert.False(FilterValue.TryParse(FilterValueKind.WholeNumber, "Yes", out read));
            Assert.Null(read);

            Assert.False(FilterValue.TryParse(FilterValueKind.Number, "quite a lot", out read));
            Assert.False(FilterValue.TryParse(FilterValueKind.ElementReference, "3.5", out read));
        }

        [Fact]
        public void ANumberThatIsNotANumberIsRefusedAtTheDoor()
        {
            Assert.Throws<ArgumentException>(() => FilterValue.OfNumber(double.NaN));
            Assert.Throws<ArgumentException>(() => FilterValue.OfNumber(double.PositiveInfinity));

            FilterValue read;
            Assert.False(FilterValue.TryParse(FilterValueKind.Number, "NaN", out read));
        }

        /// <summary>
        /// A rule carries the kind with it, so create can never see the value without seeing
        /// how to rebuild it.
        /// </summary>
        [Fact]
        public void ARuleCarriesTheKindAlongWithTheValue()
        {
            var rule = new ScheduleFilterRule(
                "PRX_Included In Budget", FilterValue.OfWholeNumber(1));

            Assert.Equal(FilterValueKind.WholeNumber, rule.Kind);
            Assert.Equal("1", rule.Value);
            Assert.Equal(1L, rule.Held.WholeNumber);
            Assert.Equal("PRX_Included In Budget equals 1", rule.ToString());
        }

        /// <summary>
        /// Only a text value can name a plot, so a whole number is never mistaken for one and
        /// swapped out by ForPlot.
        /// </summary>
        [Fact]
        public void OnlyTextCanNameAPlot()
        {
            var plot = new ScheduleFilterRule("PRX_Ref Plot ID", FilterValue.Text("DM-11"));
            var budget = new ScheduleFilterRule("PRX_Included In Budget", FilterValue.OfWholeNumber(1));

            Assert.True(plot.NamesThePlot);
            Assert.False(budget.NamesThePlot);

            Assert.Equal("DM-28", plot.ForPlot("DM-28").Value);
            Assert.Equal("1", budget.ForPlot("DM-28").Value);
            Assert.Equal(FilterValueKind.WholeNumber, budget.ForPlot("DM-28").Kind);
        }

        /// <summary>
        /// The whole SHRUBS AND LAWN case, end to end. Two filters, one naming the plot as text
        /// and one holding a Yes as an integer, and aiming it at another plot changes the first
        /// and leaves the second alone with its kind intact.
        /// </summary>
        [Fact]
        public void TheScheduleThatWasRefusedRebuildsWithBothKindsIntact()
        {
            var shrubs = new ScheduleDefinition(
                new ViewType("600", "SHRUBS & LAWN SCHEDULE"),
                "Floors",
                new[] { ScheduleFieldEntry.Parameter("Area") },
                new[]
                {
                    new ScheduleFilterRule("PRX_Ref Plot ID", FilterValue.Text("DM-11")),
                    new ScheduleFilterRule("PRX_Included In Budget", FilterValue.OfWholeNumber(1))
                },
                true,
                false);

            ScheduleDefinition made = shrubs.ForPlot("DM-28");

            Assert.Equal("DM-28", made.Filters[0].Value);
            Assert.Equal(FilterValueKind.Text, made.Filters[0].Kind);

            Assert.Equal("1", made.Filters[1].Value);
            Assert.Equal(FilterValueKind.WholeNumber, made.Filters[1].Kind);
        }
    }

    /// <summary>
    /// A field that cannot be added, and why not.
    ///
    /// Three schedules came out short of a field and the report said they were not schedulable
    /// for this category, which sends somebody to look at the category. LIST OF DRAWINGS is
    /// short of Sheet Numbering, HARDSCAPE of Area Conversion and TOTAL SAR, SIGNAGE of CODE.
    /// </summary>
    public class ScheduleFieldEntryTests
    {
        [Fact]
        public void AParameterFieldSaysTheCategoryDoesNotOfferIt()
        {
            ScheduleFieldEntry entry = ScheduleFieldEntry.Parameter("PRX_Hardscape Code");

            Assert.False(entry.IsCalculated);
            Assert.Equal(
                "PRX_Hardscape Code, which this category does not offer as a field here",
                entry.WhyItIsMissing());
        }

        /// <summary>
        /// A formula or a percentage lives inside the schedule that defines it, so Revit never
        /// offers it to a new one and the answer is to write it again rather than to go looking
        /// at the category.
        /// </summary>
        [Fact]
        public void ACalculatedFieldSaysItHasToBeWrittenAgain()
        {
            ScheduleFieldEntry entry = ScheduleFieldEntry.Calculated("TOTAL SAR");

            Assert.True(entry.IsCalculated);
            Assert.Equal(
                "TOTAL SAR, a calculated field defined inside the source schedule, which Revit "
                + "does not offer to a new one and which has to be written again by hand",
                entry.WhyItIsMissing());
        }

        [Fact]
        public void AFieldAlwaysHasAName()
        {
            Assert.Throws<ArgumentNullException>(() => ScheduleFieldEntry.Parameter(null));
        }

        /// <summary>
        /// The kind travels with the field through a plot swap, so a definition aimed at another
        /// plot still knows which of its fields are calculated.
        /// </summary>
        [Fact]
        public void TheKindSurvivesBeingAimedAtAnotherPlot()
        {
            var hardscape = new ScheduleDefinition(
                new ViewType("600", "HARDSCAPE SCHEDULE"),
                "Floors",
                new[]
                {
                    ScheduleFieldEntry.Parameter("Area"),
                    ScheduleFieldEntry.Calculated("Area Conversion"),
                    ScheduleFieldEntry.Calculated("TOTAL SAR")
                },
                new[] { new ScheduleFilterRule("PRX_Ref Plot ID", "DM-11") },
                true,
                false);

            ScheduleDefinition made = hardscape.ForPlot("DM-28");

            Assert.False(made.FieldsInOrder[0].IsCalculated);
            Assert.True(made.FieldsInOrder[1].IsCalculated);
            Assert.True(made.FieldsInOrder[2].IsCalculated);
        }
    }

    /// <summary>
    /// Which category a new schedule is built on.
    ///
    /// KERBS is built on Slab Edges and was refused with "this model has no category named Slab
    /// Edges", on a model that has it. The number is what resolves, not the name.
    /// </summary>
    public class ScheduleCategoryTests
    {
        private static ScheduleDefinition Kerbs(long builtIn)
        {
            return new ScheduleDefinition(
                new ViewType("600", "KERBS SCHEDULE"),
                "Slab Edges",
                new[] { ScheduleFieldEntry.Parameter("Length") },
                new[] { new ScheduleFilterRule("PRX_Ref Plot ID", "DM-11") },
                true,
                false,
                builtIn);
        }

        /// <summary>
        /// A category number. Revit's own value for OST_EdgeSlab is not asserted here, because
        /// it has not been read off a model in this session and a number invented to look right
        /// is worse than one that is plainly a stand in.
        /// </summary>
        private const long ACategoryNumber = -2000180L;

        [Fact]
        public void TheCategoryNumberIsCarriedAndSurvivesAPlotSwap()
        {
            ScheduleDefinition made = Kerbs(ACategoryNumber).ForPlot("DM-28");

            Assert.True(made.HasBuiltInCategory);
            Assert.Equal(ACategoryNumber, made.CategoryBuiltInValue);
            Assert.Equal("Slab Edges", made.CategoryName);
        }

        [Fact]
        public void AScheduleWithNoCategoryNumberSaysTheNameIsNotEnough()
        {
            ScheduleDefinition none = Kerbs(0L);

            Assert.False(none.HasBuiltInCategory);
            Assert.Equal(
                "Slab Edges is not one of Revit's own categories, so there is no number to "
                + "build a new schedule from and the name alone is not enough.",
                none.WhyTheCategoryIsNoGood());
        }

        [Fact]
        public void AScheduleWithANumberTheModelLacksNamesBoth()
        {
            Assert.Equal(
                "This model does not hold category Slab Edges, number -2000180, so that "
                + "schedule cannot be built.",
                Kerbs(ACategoryNumber).WhyTheCategoryIsNoGood());
        }
    }
}
