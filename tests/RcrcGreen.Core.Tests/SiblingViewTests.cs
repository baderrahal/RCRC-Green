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

        /// <summary>
        /// How DM-16 and DM-14 are really set: crop on, crop region hidden, annotation crop on.
        /// </summary>
        private static ViewCrop AsTheTeamBuildsThem()
        {
            return new ViewCrop(true, false, true);
        }

        private static SiblingView Plan(string name, ViewType type, string familyType, string template)
        {
            return new SiblingView(
                name, type, SiblingKind.Plan, familyType, template, "Level 1", AsTheTeamBuildsThem());
        }

        private static SiblingView Section(string name, ViewType type, string template)
        {
            return new SiblingView(
                name, type, SiblingKind.Section, "(400) Section", template, string.Empty,
                AsTheTeamBuildsThem());
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

        /// <summary>
        /// The crop settings joined the family type and the template this round, so they are
        /// held to the same rule. All five come off the one view or the object is wrong.
        /// </summary>
        [Fact]
        public void TheCropSettingsComeFromTheSameViewAsEverythingElse()
        {
            var cropped = new SiblingView(
                "DM-16-(200) General Arrangement Layout", General, SiblingKind.Plan,
                "TYPE A", "TEMPLATE A", "Level 1", new ViewCrop(true, false, true));

            var bare = new SiblingView(
                "DM-20-(200) General Arrangement Layout", General, SiblingKind.Plan,
                "TYPE B", "TEMPLATE B", "Level 2", new ViewCrop(false, true, false));

            SiblingView chosen = SiblingChoice.For(General, SiblingKind.Plan, new[] { cropped, bare });

            bool allFromCropped = chosen.FamilyTypeName == "TYPE A"
                && chosen.TemplateName == "TEMPLATE A"
                && chosen.LevelName == "Level 1"
                && chosen.Crop.CropActive
                && !chosen.Crop.CropRegionVisible
                && chosen.Crop.AnnotationCrop;

            bool allFromBare = chosen.FamilyTypeName == "TYPE B"
                && chosen.TemplateName == "TEMPLATE B"
                && chosen.LevelName == "Level 2"
                && !chosen.Crop.CropActive
                && chosen.Crop.CropRegionVisible
                && !chosen.Crop.AnnotationCrop;

            Assert.True(allFromCropped || allFromBare,
                "the settings on " + chosen.ViewName + " came from more than one view.");
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
        /// A plan needs a level, so the kind is preferred. Taking the first match regardless
        /// refused a plan view because the first view of that type happened to be a section,
        /// while a usable plan sat further down the list.
        /// </summary>
        [Fact]
        public void TheKindThatCanAnswerIsPreferredOverTheFirstMatch()
        {
            var section = Section("DM-18-(010) Overall Plan", Overall, "TEMPLATE S");
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
            var section = Section("DM-18-(010) Overall Plan", Overall, "TEMPLATE S");

            Assert.Equal("DM-18-(010) Overall Plan",
                SiblingChoice.For(Overall, SiblingKind.Plan, new[] { section }).ViewName);
        }

        [Fact]
        public void TheWordsNameTheViewAndEverySettingTakenOffIt()
        {
            var plan = Plan("DM-18-(010) Overall Plan", Overall,
                "(200) General Arrangement Layout", "(010) Overall Plan");

            Assert.Equal(
                "Set up from DM-18-(010) Overall Plan: family type (200) General Arrangement "
                + "Layout, view template (010) Overall Plan, level Level 1, crop on, "
                + "crop region hidden.",
                plan.InWords());
        }

        /// <summary>
        /// How a view the tool created reads, so the two lines can be held against each other
        /// in a report.
        /// </summary>
        [Fact]
        public void TheWordsSayWhenAnnotationCropIsOff()
        {
            var made = new SiblingView(
                "DM-11-(010) Overall Plan", Overall, SiblingKind.Plan,
                "TYPE", "TEMPLATE", "Level 1", new ViewCrop(true, true, false));

            Assert.Equal("crop on, crop region shown", made.Crop.CopiedInWords());
            Assert.Equal("crop on, crop region shown, annotation crop off", made.Crop.ToString());

            // The setup line says only what was copied. Annotation crop is said separately,
            // because it is the tool's own and not read off any view.
            Assert.DoesNotContain("annotation crop", made.InWords());
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
                Section("DM-20-(400) Landscape Cross Section", CrossSection, "TEMPLATE S").InWords());
        }

        [Fact]
        public void ASiblingAlwaysHasANameAndAType()
        {
            Assert.Throws<ArgumentNullException>(() => new SiblingView(
                null, CrossSection, SiblingKind.Section, "TYPE", "TEMPLATE", string.Empty,
                AsTheTeamBuildsThem()));

            Assert.Throws<ArgumentNullException>(() => new SiblingView(
                "DM-20-(400) Landscape Cross Section", null, SiblingKind.Section,
                "TYPE", "TEMPLATE", string.Empty, AsTheTeamBuildsThem()));
        }
    }

    /// <summary>
    /// How deep a cross section is cut, and the conversions the reports print.
    /// </summary>
    public class SectionDepthTests
    {
        /// <summary>
        /// The team's decision, after four real sections came back disagreeing: DM-14 and NS-32
        /// at 3.0480 feet, DM-16 at 5.0199, DM-20 at 42.1054. There is no rule in the model to
        /// copy, so the tool sets one number.
        /// </summary>
        [Fact]
        public void TheDepthIsOneMetre()
        {
            Assert.Equal(1.0, SectionDepth.Metres);
        }

        /// <summary>
        /// One foot is 0.3048 metres by definition, so every number here is exact rather than
        /// measured, and each is written out by hand.
        /// </summary>
        [Fact]
        public void FeetReadBackAsMetresForAReport()
        {
            Assert.Equal(12.83, Math.Round(Lengths.InMetres(42.1054), 2));
            Assert.Equal(0.93, Math.Round(Lengths.InMetres(3.0480), 2));
            Assert.Equal(1.53, Math.Round(Lengths.InMetres(5.0199), 2));
            Assert.Equal(10.0, Math.Round(Lengths.InMetres(32.8083989501312), 6));
        }

        /// <summary>
        /// An A1 sheet is 841 by 594 millimetres, which is what a title block reports in feet.
        /// </summary>
        [Fact]
        public void FeetReadBackAsMillimetres()
        {
            Assert.Equal(841.0, Math.Round(Lengths.InMillimetres(2.75919), 0));
            Assert.Equal(594.0, Math.Round(Lengths.InMillimetres(1.94882), 0));
        }

        [Fact]
        public void OneMetreIsThatManyFeet()
        {
            Assert.Equal(3.2808, Math.Round(Lengths.FeetFromMetres(1.0), 4));
        }
    }
}
