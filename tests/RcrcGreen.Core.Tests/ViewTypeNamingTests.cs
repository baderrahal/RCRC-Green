using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class ViewTypeNamingTests
    {
        private static readonly ViewType General = new ViewType("200", "General Arrangement Layout");
        private static readonly ViewType Location = new ViewType("010", "Location Key Plan");

        /// <summary>
        /// Read off DM-18-(200) General Arrangement Layout. The view family type is named
        /// exactly the view type, not a generic floor plan type.
        /// </summary>
        [Fact]
        public void TheFamilyTypeIsNamedExactlyTheViewType()
        {
            Assert.Equal("(200) General Arrangement Layout", ViewTypeNaming.FamilyTypeNameFor(General));
            Assert.Equal("(010) Location Key Plan", ViewTypeNaming.FamilyTypeNameFor(Location));
        }

        /// <summary>
        /// The real template on that view. The view type, then how it is drawn.
        /// </summary>
        [Fact]
        public void OneTemplateStartingWithTheViewTypeIsTheMatch()
        {
            TemplateMatch match = ViewTypeNaming.TemplateFor(
                General,
                new[]
                {
                    "(010) Location Key Plan SC - Scale 1000",
                    "(200) General Arrangement Layout SC - Scale 250",
                    "Coordination Working"
                });

            Assert.True(match.Found);
            Assert.Equal("(200) General Arrangement Layout SC - Scale 250", match.Name);
            Assert.False(match.Ambiguous);
        }

        [Fact]
        public void NoTemplateStartingWithTheViewTypeIsNoMatch()
        {
            TemplateMatch match = ViewTypeNaming.TemplateFor(
                General,
                new[] { "(010) Location Key Plan SC - Scale 1000", "Coordination Working" });

            Assert.False(match.Found);
            Assert.Empty(match.Candidates);
            Assert.False(match.Ambiguous);
        }

        /// <summary>
        /// Scale 250 and Scale 500 are both real templates for one view type. Picking either
        /// would be inventing a rule, so both come back and the caller reports them.
        /// </summary>
        [Fact]
        public void TwoTemplatesStartingWithTheViewTypeAreBothCandidatesAndNeitherIsChosen()
        {
            TemplateMatch match = ViewTypeNaming.TemplateFor(
                General,
                new[]
                {
                    "(200) General Arrangement Layout SC - Scale 500",
                    "(200) General Arrangement Layout SC - Scale 250"
                });

            Assert.False(match.Found);
            Assert.Equal(string.Empty, match.Name);
            Assert.True(match.Ambiguous);
            Assert.Equal(
                new[]
                {
                    "(200) General Arrangement Layout SC - Scale 250",
                    "(200) General Arrangement Layout SC - Scale 500"
                },
                match.Candidates);
        }

        /// <summary>
        /// A template named exactly the view type and nothing more still matches, because a
        /// prefix includes the whole string.
        /// </summary>
        [Fact]
        public void ATemplateNamedExactlyTheViewTypeMatches()
        {
            TemplateMatch match = ViewTypeNaming.TemplateFor(
                General, new[] { "(200) General Arrangement Layout" });

            Assert.True(match.Found);
            Assert.Equal("(200) General Arrangement Layout", match.Name);
        }

        /// <summary>
        /// Case sensitive, the same rule the plot identifier and the scope box name follow.
        /// </summary>
        [Fact]
        public void ATemplateDifferingOnlyInCaseDoesNotMatch()
        {
            TemplateMatch match = ViewTypeNaming.TemplateFor(
                General, new[] { "(200) GENERAL ARRANGEMENT LAYOUT SC - Scale 250" });

            Assert.False(match.Found);
            Assert.Empty(match.Candidates);
        }

        /// <summary>
        /// A name holding the view type somewhere other than the start is not a match. One view
        /// type's template must not be picked up by another.
        /// </summary>
        [Fact]
        public void ATemplateHoldingTheViewTypeSomewhereElseDoesNotMatch()
        {
            TemplateMatch match = ViewTypeNaming.TemplateFor(
                General, new[] { "Working (200) General Arrangement Layout" });

            Assert.False(match.Found);
            Assert.Empty(match.Candidates);
        }

        [Fact]
        public void TheSameTemplateNameTwiceIsOneCandidate()
        {
            TemplateMatch match = ViewTypeNaming.TemplateFor(
                General,
                new[]
                {
                    "(200) General Arrangement Layout SC - Scale 250",
                    "(200) General Arrangement Layout SC - Scale 250"
                });

            Assert.True(match.Found);
            Assert.Single(match.Candidates);
        }

        [Fact]
        public void NoTemplatesAtAllIsAnEmptyAnswerRatherThanAThrow()
        {
            TemplateMatch match = ViewTypeNaming.TemplateFor(General, null);

            Assert.False(match.Found);
            Assert.Empty(match.Candidates);
        }
    }
}
