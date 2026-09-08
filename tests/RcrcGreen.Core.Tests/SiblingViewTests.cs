using System;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// Where a new view is set up from.
    ///
    /// A run produced a view whose template read (010) Overall Plan and whose family type read
    /// (200) General Arrangement Layout. Nothing in the report said which view either had come
    /// from, so there was no way to check whether they had come from the same one. These tests
    /// are what make that answerable without a Properties panel.
    /// </summary>
    public class SiblingChoiceTests
    {
        private static readonly ViewType Overall = new ViewType("010", "Overall Plan");
        private static readonly ViewType General = new ViewType("200", "General Arrangement Layout");
        private static readonly ViewType CrossSection = new ViewType("400", "Landscape Cross Section");

        private static SiblingView Plan(string name, ViewType type, string familyType, string template)
        {
            return new SiblingView(
                name, type, SiblingKind.Plan, familyType, template, "Level 1", 0.0, false);
        }

        private static SiblingView Section(string name, ViewType type, string template, double farClip)
        {
            return new SiblingView(
                name, type, SiblingKind.Section, "(400) Section", template, string.Empty,
                farClip, farClip > 0.0);
        }

        /// <summary>
        /// The test the round asked for. Two views of the same type disagree about both the
        /// family type and the template, and whichever one is picked, both settings have to be
        /// that view's. It goes red the moment the two are read off different candidates.
        /// </summary>
        [Fact]
        public void TheFamilyTypeAndTheTemplateAlwaysComeFromOneView()
        {
            var first = Plan("DM-18-(010) Overall Plan", Overall,
                "(200) General Arrangement Layout", "(010) Overall Plan");

            var second = Plan("DM-20-(010) Overall Plan", Overall,
                "(010) Overall Plan", "(010) Overall Plan SC - Scale 500");

            SiblingView chosen = SiblingChoice.For(Overall, SiblingKind.Plan, new[] { first, second });

            Assert.NotNull(chosen);

            SiblingView itReallyIs = string.Equals(chosen.ViewName, first.ViewName, StringComparison.Ordinal)
                ? first
                : second;

            Assert.Equal(itReallyIs.FamilyTypeName, chosen.FamilyTypeName);
            Assert.Equal(itReallyIs.TemplateName, chosen.TemplateName);
            Assert.Equal(itReallyIs.LevelName, chosen.LevelName);
        }

        /// <summary>
        /// The same check written the other way round, so it fails on the pairing rather than on
        /// the choice. A family type from one view and a template from another can never both be
        /// on one object, which is the whole reason the settings travel together.
        /// </summary>
        [Fact]
        public void NoChoiceEverPairsOneViewsFamilyTypeWithAnothersTemplate()
        {
            var first = Plan("DM-18-(010) Overall Plan", Overall, "TYPE A", "TEMPLATE A");
            var second = Plan("DM-20-(010) Overall Plan", Overall, "TYPE B", "TEMPLATE B");

            SiblingView chosen = SiblingChoice.For(Overall, SiblingKind.Plan, new[] { first, second });

            bool pairedA = chosen.FamilyTypeName == "TYPE A" && chosen.TemplateName == "TEMPLATE A";
            bool pairedB = chosen.FamilyTypeName == "TYPE B" && chosen.TemplateName == "TEMPLATE B";

            Assert.True(pairedA || pairedB,
                "family type " + chosen.FamilyTypeName + " was paired with template "
                + chosen.TemplateName + ", which belong to two different views.");
        }

        [Fact]
        public void TheFirstOfThatViewTypeIsTheOneChosen()
        {
            var first = Plan("DM-18-(010) Overall Plan", Overall, "TYPE A", "TEMPLATE A");
            var second = Plan("DM-20-(010) Overall Plan", Overall, "TYPE B", "TEMPLATE B");

            Assert.Equal(
                "DM-18-(010) Overall Plan",
                SiblingChoice.For(Overall, SiblingKind.Plan, new[] { first, second }).ViewName);
        }

        [Fact]
        public void AViewOfAnotherTypeIsNeverChosen()
        {
            var wrongType = Plan("DM-18-(200) General Arrangement Layout", General, "TYPE A", "TEMPLATE A");

            Assert.Null(SiblingChoice.For(Overall, SiblingKind.Plan, new[] { wrongType }));
            Assert.Null(SiblingChoice.For(Overall, SiblingKind.Plan, null));
        }

        /// <summary>
        /// A plan needs a level and a section needs a far clip, so the kind is preferred. Taking
        /// the first match regardless refused a plan view because the first view of that type
        /// happened to be a section, while a usable plan sat further down the list.
        /// </summary>
        [Fact]
        public void TheKindThatCanAnswerIsPreferredOverTheFirstMatch()
        {
            var section = Section("DM-18-(010) Overall Plan", Overall, "TEMPLATE S", 42.1054);
            var plan = Plan("DM-20-(010) Overall Plan", Overall, "TYPE P", "TEMPLATE P");

            Assert.Equal("DM-20-(010) Overall Plan",
                SiblingChoice.For(Overall, SiblingKind.Plan, new[] { section, plan }).ViewName);

            Assert.Equal("DM-18-(010) Overall Plan",
                SiblingChoice.For(Overall, SiblingKind.Section, new[] { section, plan }).ViewName);
        }

        /// <summary>
        /// With nothing of the wanted kind the first of that type still comes back, so the
        /// caller refuses with a message naming a real view rather than saying nothing was found.
        /// </summary>
        [Fact]
        public void WithNothingOfTheRightKindTheFirstOfThatTypeStillComesBack()
        {
            var section = Section("DM-18-(010) Overall Plan", Overall, "TEMPLATE S", 42.1054);

            Assert.Equal("DM-18-(010) Overall Plan",
                SiblingChoice.For(Overall, SiblingKind.Plan, new[] { section }).ViewName);
        }

        [Fact]
        public void TheWordsNameTheViewAndBothSettings()
        {
            var plan = Plan("DM-18-(010) Overall Plan", Overall,
                "(200) General Arrangement Layout", "(010) Overall Plan");

            Assert.Equal(
                "Set up from DM-18-(010) Overall Plan: family type (200) General Arrangement "
                + "Layout, view template (010) Overall Plan, level Level 1.",
                plan.InWords());
        }

        [Fact]
        public void ASiblingWithNoTemplateSaysNoneRatherThanNothing()
        {
            var plan = Plan("DM-18-(010) Overall Plan", Overall, "TYPE A", string.Empty);

            Assert.False(plan.HasTemplate);
            Assert.Contains("view template none", plan.InWords());
        }

        [Fact]
        public void ASectionIsNotAskedAboutALevel()
        {
            Assert.DoesNotContain(
                "level",
                Section("DM-20-(400) Landscape Cross Section", CrossSection, "TEMPLATE S", 42.1054).InWords());
        }

        [Fact]
        public void AFarClipThatIsNotANumberIsRefused()
        {
            Assert.Throws<ArgumentException>(() => new SiblingView(
                "DM-20-(400) Landscape Cross Section", CrossSection, SiblingKind.Section,
                "TYPE", "TEMPLATE", string.Empty, double.NaN, true));

            Assert.Throws<ArgumentNullException>(() => new SiblingView(
                null, CrossSection, SiblingKind.Section, "TYPE", "TEMPLATE", string.Empty, 1.0, true));
        }
    }

    /// <summary>
    /// How far a new section looks, and which of the two sources said so.
    /// </summary>
    public class SectionDepthChoiceTests
    {
        private static readonly ViewType CrossSection = new ViewType("400", "Landscape Cross Section");

        /// <summary>
        /// The real numbers. DM-20-(400) Landscape Cross Section reads 42.1054 feet, which is
        /// 12.83 metres. SectionDefaults held 10, named before anybody had opened a section.
        /// </summary>
        private const double TenMetresInFeet = 32.8083989501312;

        private static SiblingView WithFarClip(double feet)
        {
            return new SiblingView(
                "DM-20-(400) Landscape Cross Section", CrossSection, SiblingKind.Section,
                "(400) Section", "(400) Landscape Cross Section - Scale 100", string.Empty,
                feet, feet > 0.0);
        }

        [Fact]
        public void TheModelBeatsTheValueTheTeamNamed()
        {
            SectionDepthChoice depth = SectionDepthChoice.For(WithFarClip(42.1054), TenMetresInFeet);

            Assert.True(depth.FromTheSibling);
            Assert.Equal(42.1054, depth.Feet);
            Assert.Equal(
                "Looks 12.83 metres, taken from DM-20-(400) Landscape Cross Section.",
                depth.InWords());
        }

        [Fact]
        public void WithNoFarClipOnTheSiblingTheNamedValueIsUsedAndSaidSo()
        {
            SectionDepthChoice depth = SectionDepthChoice.For(WithFarClip(0.0), TenMetresInFeet);

            Assert.False(depth.FromTheSibling);
            Assert.Equal(TenMetresInFeet, depth.Feet);
            Assert.Contains("Looks 10 metres, the value the team named", depth.InWords());
            Assert.Contains("has no far clip offset set", depth.InWords());
        }

        [Fact]
        public void WithNoSiblingAtAllTheNamedValueIsUsedAndSaidSo()
        {
            SectionDepthChoice depth = SectionDepthChoice.For(null, TenMetresInFeet);

            Assert.False(depth.FromTheSibling);
            Assert.Equal(TenMetresInFeet, depth.Feet);
            Assert.Contains("no section of that type was found", depth.InWords());
        }

        /// <summary>
        /// One foot is 0.3048 metres by definition, so this is exact rather than measured.
        /// </summary>
        [Fact]
        public void FeetReadBackAsMetresForTheReport()
        {
            Assert.Equal(12.83, Math.Round(SectionDepth.InMetres(42.1054), 2));
            Assert.Equal(10.0, Math.Round(SectionDepth.InMetres(TenMetresInFeet), 6));
        }
    }
}
