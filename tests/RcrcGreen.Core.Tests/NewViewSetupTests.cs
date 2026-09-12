using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// The three answers for a view type with no example in the model. Nothing is invented
    /// and nothing is defaulted: short of an answer, the run refuses the type naming what is
    /// missing, and a section takes no level because ViewSection.CreateSection takes a box.
    /// </summary>
    public class NewViewSetupTests
    {
        private static readonly ViewType Coordination = new ViewType("300", "Coordination Layout");

        private static readonly ScannedViewFamilyType[] Families =
        {
            new ScannedViewFamilyType("(300) Coordination", "FloorPlan"),
            new ScannedViewFamilyType("Building Section", "Section"),
            new ScannedViewFamilyType("Schedule", "Schedule")
        };

        [Fact]
        public void NothingAnsweredNamesAllThreeForAPlanAndTwoForASection()
        {
            Assert.Equal(
                "the view family type, the view template and the level",
                NewViewSetups.Nothing.Missing(Coordination, false));

            Assert.Equal(
                "the view family type and the view template",
                NewViewSetups.Nothing.Missing(Coordination, true));
        }

        [Fact]
        public void EachMissingAnswerIsNamedAndACompleteSetIsSilent()
        {
            NewViewSetups partly = NewViewSetups.Nothing.With(
                Coordination, "(300) Coordination", string.Empty, string.Empty);

            Assert.Equal(
                "the view template and the level",
                partly.Missing(Coordination, false));

            NewViewSetups complete = NewViewSetups.Nothing.With(
                Coordination, "(300) Coordination", "(300) Coordination Layout", "Level 1");

            Assert.Equal(string.Empty, complete.Missing(Coordination, false));

            NewViewSetups sectionish = NewViewSetups.Nothing.With(
                Coordination, "Building Section", "Section Template", string.Empty);

            Assert.Equal(string.Empty, sectionish.Missing(Coordination, true));
        }

        [Fact]
        public void TheWordsSayAnsweredNowRememberedOrNotYet()
        {
            Assert.Equal("Not answered yet.", NewViewSetups.Nothing.WordsFor(Coordination));

            NewViewSetups now = NewViewSetups.Nothing.With(
                Coordination, "(300) Coordination", "T", "Level 1");
            Assert.Equal("Answered now.", now.WordsFor(Coordination));
            Assert.Equal(NewViewSource.AnsweredNow, now.For(Coordination).Source);

            NewViewSetups stored = NewViewSetups.Remembered(now.All);
            Assert.Equal("Remembered from last time.", stored.WordsFor(Coordination));
            Assert.Equal(NewViewSource.Remembered, stored.For(Coordination).Source);
        }

        /// <summary>
        /// The routing rule the panel's preview and the run's handler both ask: a type whose
        /// saved family type is a section kind creates as a section. Unanswered types and
        /// plan kinds do not.
        /// </summary>
        [Fact]
        public void OnlyATypeWhoseSavedFamilyIsASectionRoutesAsOne()
        {
            var asPlan = new ViewType("300", "As Plan");
            var asSection = new ViewType("310", "As Section");
            var unanswered = new ViewType("320", "Unanswered");

            NewViewSetups setups = NewViewSetups.Nothing
                .With(asPlan, "(300) Coordination", "T", "Level 1")
                .With(asSection, "Building Section", "T", string.Empty);

            var routed = setups.SectionTypesAmong(
                new[] { asPlan, asSection, unanswered }, Families);

            Assert.Equal(new[] { asSection }, routed.ToArray());
        }

        [Fact]
        public void OnlyThePlanAndSectionKindsAreOffered()
        {
            Assert.True(NewViewFamilies.CanBeCreated("FloorPlan"));
            Assert.True(NewViewFamilies.CanBeCreated("CeilingPlan"));
            Assert.True(NewViewFamilies.CanBeCreated("AreaPlan"));
            Assert.True(NewViewFamilies.CanBeCreated("StructuralPlan"));
            Assert.True(NewViewFamilies.CanBeCreated("Section"));
            Assert.False(NewViewFamilies.CanBeCreated("Schedule"));
            Assert.False(NewViewFamilies.CanBeCreated("ThreeDimensional"));
            Assert.False(NewViewFamilies.CanBeCreated(string.Empty));
            Assert.False(NewViewFamilies.CanBeCreated(null));

            Assert.Equal(
                new[] { "(300) Coordination", "Building Section" },
                NewViewFamilies.Offerable(Families).Select(one => one.Name).ToArray());

            Assert.Equal("Section", NewViewFamilies.KindOf("Building Section", Families));
            Assert.Equal(string.Empty, NewViewFamilies.KindOf("Nowhere", Families));
        }
    }

    /// <summary>
    /// The setup file: tab separated, five fields, the level allowed empty because a
    /// section takes none, and every unreadable line kept and said.
    /// </summary>
    public class NewViewSetupFileTests
    {
        [Fact]
        public void WhatIsWrittenReadsBackWithItsEmptyLevel()
        {
            string text = NewViewSetupFile.Write(new[]
            {
                new NewViewAnswers(
                    new ViewType("400", "Landscape Cross Section"),
                    "Building Section", "Section Template", string.Empty,
                    NewViewSource.AnsweredNow),
                new NewViewAnswers(
                    new ViewType("300", "Coordination Layout"),
                    "(300) Coordination", "(300) Template", "Level 1",
                    NewViewSource.AnsweredNow)
            });

            NewViewSetupFileContents held = NewViewSetupFile.Read(text);

            Assert.Empty(held.NotRead);
            Assert.Equal(2, held.Answers.Count);

            NewViewAnswers section = held.Answers
                .Single(one => one.Type.ViewName == "Landscape Cross Section");
            Assert.Equal("Building Section", section.FamilyTypeName);
            Assert.Equal("Section Template", section.TemplateName);
            Assert.Equal(string.Empty, section.LevelName);

            NewViewAnswers plan = held.Answers
                .Single(one => one.Type.ViewName == "Coordination Layout");
            Assert.Equal("Level 1", plan.LevelName);
        }

        [Fact]
        public void ALineShortOfItsFieldsIsKeptWithItsNumber()
        {
            NewViewSetupFileContents held = NewViewSetupFile.Read(
                "# a note\r\n300\tCoordination Layout\tFam\tTempl\tLevel 1\r\n300\tHalf\tFam\r\n");

            Assert.Single(held.Answers);
            string bad = Assert.Single(held.NotRead);
            Assert.Equal("line 3, 300\tHalf\tFam", bad);
        }

        /// <summary>
        /// An answer short of its family type or template is not written, because a line
        /// missing either says nothing a refusal would not say better. The level is the one
        /// field that may be empty on purpose.
        /// </summary>
        [Fact]
        public void AnAnswerShortOfFamilyOrTemplateIsNotWritten()
        {
            string text = NewViewSetupFile.Write(new[]
            {
                new NewViewAnswers(
                    new ViewType("300", "Coordination Layout"),
                    "(300) Coordination", string.Empty, "Level 1",
                    NewViewSource.AnsweredNow)
            });

            Assert.Empty(NewViewSetupFile.Read(text).Answers);
        }

        [Fact]
        public void NothingReadsAsNothing()
        {
            Assert.Empty(NewViewSetupFile.Read(string.Empty).Answers);
            Assert.Empty(NewViewSetupFile.Read(null).NotRead);
        }
    }
}
