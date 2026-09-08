using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class ScheduleDefinitionTests
    {
        /// <summary>
        /// The real HARDSCAPE SCHEDULE on plot DM-11, read off the user's screenshot. Two
        /// filters, and only one of them names the plot.
        /// </summary>
        private static ScheduleDefinition Hardscape()
        {
            return new ScheduleDefinition(
                new ViewType("600", "HARDSCAPE SCHEDULE"),
                "Floors",
                new[]
                {
                    "PRX_Hardscape Image", "PRX_Hardscape Code", "Description",
                    "PRX_Hardscape Material", "PRX_Hardscape Finish", "PRX_Hardscape Pattern",
                    "PRX_Hardscape Color", "PRX_Hardscape Joint", "PRX_Hardscape Size", "Area",
                    "PRX_Hardscape Specification", "PRX_Hardscape Detail Ref.", "Area Conversion",
                    "Cost", "TOTAL SAR", "PRX_Ref Plot ID", "PRX_Included In Budget",
                    "PRX_Element Grouping", "Phase Created"
                },
                new[]
                {
                    new ScheduleFilterRule("PRX_Ref Plot ID", "DM-11"),
                    new ScheduleFilterRule("PRX_Element Grouping", "HARDSCAPE")
                },
                true,
                false);
        }

        private static ScheduleDefinition SheetList()
        {
            return new ScheduleDefinition(
                new ViewType("010", "LIST OF DRAWINGS"),
                "Sheets",
                new[] { "Sheet Numbering", "Sheet Name", "PRX_Sheet_Scale" },
                new[] { new ScheduleFilterRule("PRX_Plot_ID", "DM-11") },
                true,
                true);
        }

        /// <summary>
        /// There are two plot parameters in this model. A quantity schedule filters on
        /// PRX_Ref Plot ID with spaces, the Sheet List filters on PRX_Plot_ID. Which one is
        /// read off the captured schedule rather than decided in code.
        /// </summary>
        [Fact]
        public void TheQuantityScheduleFiltersOnTheRefPlotParameter()
        {
            Assert.Equal("PRX_Ref Plot ID", Hardscape().PlotParameterName);
        }

        [Fact]
        public void TheSheetListFiltersOnThePlotIdParameter()
        {
            Assert.Equal("PRX_Plot_ID", SheetList().PlotParameterName);
            Assert.True(SheetList().IsASheetList);
            Assert.False(Hardscape().IsASheetList);
        }

        [Fact]
        public void OnlyTheRuleNamingAPlotChangesWhenItIsAimedAtAnotherPlot()
        {
            ScheduleDefinition made = Hardscape().ForPlot("DM-28");

            Assert.Equal(
                new[] { "PRX_Ref Plot ID equals DM-28", "PRX_Element Grouping equals HARDSCAPE" },
                made.Filters.Select(rule => rule.ToString()).ToArray());
        }

        /// <summary>
        /// HARDSCAPE and SHRUBS AND LAWN are both category Floors and are told apart only by
        /// the rule that does not name the plot. Losing it would build the wrong schedule.
        /// </summary>
        [Fact]
        public void TheRuleThatTellsTwoFloorSchedulesApartSurvives()
        {
            ScheduleDefinition made = Hardscape().ForPlot("DM-28");

            Assert.Contains(made.Filters, rule =>
                rule.ParameterName == "PRX_Element Grouping" && rule.Value == "HARDSCAPE");
        }

        [Fact]
        public void FieldOrderIsCarriedAcrossExactly()
        {
            ScheduleDefinition made = Hardscape().ForPlot("DM-28");

            Assert.Equal("PRX_Hardscape Image", made.FieldsInOrder[0]);
            Assert.Equal("Phase Created", made.FieldsInOrder[18]);
            Assert.Equal(19, made.FieldsInOrder.Count);
        }

        /// <summary>
        /// PRX_Furniture Lenght is spelled that way in the model. Copied, not corrected, or the
        /// field will not be found when the schedule is built.
        /// </summary>
        [Fact]
        public void AMisspelledFieldNameIsCarriedAcrossAsItIs()
        {
            var furniture = new ScheduleDefinition(
                new ViewType("600", "FURNITURE SCHEDULE"),
                "Furniture",
                new[] { "PRX_Ref Plot ID", "PRX_Furniture Lenght" },
                new[] { new ScheduleFilterRule("PRX_Ref Plot ID", "DM-11") },
                true,
                false);

            Assert.Equal("PRX_Furniture Lenght", furniture.ForPlot("DM-28").FieldsInOrder[1]);
        }

        [Fact]
        public void TheCategoryAndTheLinkSettingAreCarriedAcross()
        {
            ScheduleDefinition made = Hardscape().ForPlot("DM-28");

            Assert.Equal("Floors", made.CategoryName);
            Assert.True(made.IncludesLinkedFiles);
        }

        [Fact]
        public void TheNewScheduleTakesTheNamingUsedEverywhereElse()
        {
            Assert.Equal("DM-28-(600) HARDSCAPE SCHEDULE", Hardscape().NameFor("DM-28"));
        }

        /// <summary>
        /// A schedule that filters on no plot at all cannot be aimed at one. Saying so is
        /// better than quietly building a copy that shows the whole model.
        /// </summary>
        [Fact]
        public void AScheduleWithNoPlotFilterCannotBeMadeForAnotherPlot()
        {
            var everything = new ScheduleDefinition(
                new ViewType("600", "ALL FURNITURE"),
                "Furniture",
                new[] { "Description" },
                new[] { new ScheduleFilterRule("PRX_Element Grouping", "HARDSCAPE") },
                true,
                false);

            Assert.False(everything.CanBeMadeForAnotherPlot);
            Assert.Equal(string.Empty, everything.PlotParameterName);
        }

        [Fact]
        public void ARuleWhoseValueIsNotAPlotIsNotThePlotRule()
        {
            Assert.False(new ScheduleFilterRule("PRX_Element Grouping", "HARDSCAPE").NamesThePlot);
            Assert.True(new ScheduleFilterRule("PRX_Ref Plot ID", "DM-11").NamesThePlot);
            Assert.False(new ScheduleFilterRule("PRX_Ref Plot ID", "dm-11").NamesThePlot);
        }

        [Fact]
        public void ADefinitionWithNoFieldsOrFiltersIsAnEmptyOneRatherThanAThrow()
        {
            var bare = new ScheduleDefinition(
                new ViewType("600", "EMPTY"), "Furniture", null, null, false, false);

            Assert.Empty(bare.FieldsInOrder);
            Assert.Empty(bare.Filters);
            Assert.False(bare.CanBeMadeForAnotherPlot);
        }
    }
}
