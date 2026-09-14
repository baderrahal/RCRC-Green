using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// The rule that picks which filled region carries a plot's area.
    ///
    /// It used to be written out twice, once in the Revit handler and once in the test
    /// fixture, **and the two were not the same rule**. The fixture took the first region
    /// whatever it held and however many there were. These cases are the handler's rule, the
    /// one that runs.
    ///
    /// **THE 14:29 RUN CHANGED WHAT TWO REGIONS HOLDING AN AREA MEANS.** 51 of 78 street plots
    /// wrote nothing, every one of them because two held an area and the tool asked which, and
    /// every one of the 51 offered RCRC_OUT OF SCOPE (PRESENTATION) as one of its two. Bader's
    /// decision of 14 September: where the client's note names one of them, take that one.
    /// The cases that pinned the old answer are rewritten here rather than deleted, because
    /// what they were protecting is still true wherever the note's type is not among them.
    /// </summary>
    public sealed class RegionChoiceTests
    {
        /// <summary>
        /// **The model's own type names, with the RCRC_ prefix they really carry**, measured on
        /// the 00 link: RCRC_CADASTRAL LIMIT on 124 regions and RCRC_OUT OF SCOPE (PRESENTATION)
        /// on 155.
        ///
        /// **These used to read without the prefix and that cost the file its whole job.** A
        /// break that made two regions holding an area settle on the type the client's note
        /// names left every case here GREEN, because no name in the file was the name the rule
        /// looked for. A fixture whose names are not the model's cannot catch a rule about
        /// names.
        /// </summary>
        private const string Cadastral = "RCRC_CADASTRAL LIMIT";
        private const string OutOfScope = "RCRC_OUT OF SCOPE (PRESENTATION)";

        /// <summary>
        /// A third type no model has shown, for the case where the note's type is not among the
        /// ones holding an area. **It is not a name from a model and is not a rule**, it is a
        /// stand in for whatever a future model calls its other regions.
        /// </summary>
        private const string SomethingElse = "RCRC_SOMETHING NOBODY HAS MEASURED";

        private static RegionArea Region(string typeName, double squareMetres)
        {
            return new RegionArea(typeName, squareMetres * 10.7639, squareMetres, string.Empty);
        }

        [Fact]
        public void OneRegionHoldingAnAreaAnswersItself()
        {
            RegionPick pick = RegionChoice.Pick(new[] { Region(OutOfScope, 3728.757) }, null);

            Assert.Equal(OutOfScope, pick.TypeName);
            Assert.Equal(RegionRoute.TheOnlyOneHoldingAnArea, pick.Route);
            Assert.Equal("the only region holding an area", pick.InWords);
        }

        /// <summary>
        /// DM-11, DM-12 and DM-13 hold the area on OUT OF SCOPE with cadastral at 0, so the one
        /// holding nothing is passed over rather than counted. **That route is the only one
        /// holding an area and NOT the note**, even though the type happens to be the note's,
        /// because one region answers itself whatever it is called.
        /// </summary>
        [Fact]
        public void TheRegionHoldingNothingIsPassedOver()
        {
            RegionPick pick = RegionChoice.Pick(
                new[] { Region(Cadastral, 0.0), Region(OutOfScope, 3728.757) }, null);

            Assert.Equal(OutOfScope, pick.TypeName);
            Assert.Equal(RegionRoute.TheOnlyOneHoldingAnArea, pick.Route);
        }

        /// <summary>
        /// **BADER'S DECISION, 14 September: the note's type wins, in EITHER order.** The three
        /// plots are off the 14:29 run, written out by hand with their own square metres, and
        /// NS-04 is the one that lists out of scope first.
        /// </summary>
        [Theory]
        [InlineData("NS-02", 3982.0, 4295.0)]
        [InlineData("NS-03", 6750.0, 7648.0)]
        public void TwoRegionsHoldingAnAreaTakeTheOneTheNoteNames(string plot, double cadastral, double outOfScope)
        {
            RegionPick pick = RegionChoice.Pick(
                new[] { Region(Cadastral, cadastral), Region(OutOfScope, outOfScope) }, null);

            Assert.Equal(OutOfScope, pick.TypeName);
            Assert.Equal(RegionRoute.TheTypeTheNoteNames, pick.Route);
            Assert.Equal(
                "more than one held an area and this is the type the client's note names",
                pick.InWords);
            Assert.True(pick.WasChosen, plot + " still chose nothing");
        }

        /// <summary>
        /// NS-04 lists out of scope first, 12289 against cadastral's 10754, so the answer cannot
        /// be the order the regions arrive in.
        /// </summary>
        [Fact]
        public void TheNotesTypeWinsWhereverItSitsInTheList()
        {
            RegionPick pick = RegionChoice.Pick(
                new[] { Region(OutOfScope, 12289.0), Region(Cadastral, 10754.0) }, null);

            Assert.Equal(OutOfScope, pick.TypeName);
            Assert.Equal(RegionRoute.TheTypeTheNoteNames, pick.Route);
        }

        /// <summary>
        /// **AND THE QUESTION STAYS WHERE THE NOTE'S TYPE IS NOT AMONG THEM.** That is still
        /// something the data cannot settle, and it is what the old rule was protecting.
        /// </summary>
        [Fact]
        public void TwoRegionsHoldingAnAreaWithoutTheNotesTypeStillChooseNothing()
        {
            RegionPick pick = RegionChoice.Pick(
                new[] { Region(Cadastral, 900.0), Region(SomethingElse, 250.0) }, null);

            Assert.Equal(string.Empty, pick.TypeName);
            Assert.Equal(RegionRoute.Nothing, pick.Route);
            Assert.False(pick.WasChosen);
        }

        /// <summary>
        /// **Two regions BOTH named as the note's type settle nothing either.** The note names
        /// one type and cannot say which of two of them is the plot's, so this is a question for
        /// a person exactly as the pair above is. No model has shown it.
        /// </summary>
        [Fact]
        public void TheNotesTypeHeldTwiceSettlesNothing()
        {
            RegionPick pick = RegionChoice.Pick(
                new[] { Region(OutOfScope, 900.0), Region(OutOfScope, 250.0) }, null);

            Assert.Equal(string.Empty, pick.TypeName);
            Assert.Equal(RegionRoute.Nothing, pick.Route);
        }

        /// <summary>
        /// Three holding an area with the note's type among them is the same decision, because
        /// the rule is about the note being there rather than about there being two.
        /// </summary>
        [Fact]
        public void ThreeHoldingAnAreaWithTheNotesTypeAmongThemTakeTheNote()
        {
            RegionPick pick = RegionChoice.Pick(
                new[] { Region(Cadastral, 900.0), Region(SomethingElse, 250.0), Region(OutOfScope, 4295.0) },
                null);

            Assert.Equal(OutOfScope, pick.TypeName);
            Assert.Equal(RegionRoute.TheTypeTheNoteNames, pick.Route);
        }

        /// <summary>
        /// **The fixture's copy answered CADASTRAL LIMIT here.** A region holding no area is not
        /// a choice, it is the absence of one.
        /// </summary>
        [Fact]
        public void ASingleRegionHoldingNoAreaChoosesNothing()
        {
            RegionPick pick = RegionChoice.Pick(new[] { Region(Cadastral, 0.0) }, null);

            Assert.Equal(string.Empty, pick.TypeName);
            Assert.Equal(RegionRoute.Nothing, pick.Route);
        }

        [Fact]
        public void NoRegionsChooseNothing()
        {
            Assert.Equal(RegionRoute.Nothing, RegionChoice.Pick(new RegionArea[0], null).Route);
            Assert.Equal(RegionRoute.Nothing, RegionChoice.Pick(null, null).Route);
            Assert.Equal("nothing chosen", RegionPick.Nothing.InWords);
        }

        /// <summary>
        /// A person's pick is an answer and everything below it is the tool working one out, so
        /// it wins even where the note would have chosen.
        /// </summary>
        [Fact]
        public void APickByHandWinsOverTheRule()
        {
            RegionPick over = RegionChoice.Pick(
                new[] { Region(Cadastral, 900.0), Region(OutOfScope, 250.0) }, Cadastral);

            Assert.Equal(Cadastral, over.TypeName);
            Assert.Equal(RegionRoute.ChosenByHand, over.Route);
            Assert.Equal("chosen by hand on the pane", over.InWords);

            Assert.Equal(
                Cadastral,
                RegionChoice.Pick(new[] { Region(OutOfScope, 3728.757) }, Cadastral).TypeName);
        }

        /// <summary>
        /// An empty string is nobody having picked, not a pick of nothing, because that is what
        /// the handler reads back off a pane where no button has been pressed.
        /// </summary>
        [Fact]
        public void AnEmptyPickIsNotAPick()
        {
            RegionPick pick = RegionChoice.Pick(new[] { Region(OutOfScope, 3728.757) }, string.Empty);

            Assert.Equal(OutOfScope, pick.TypeName);
            Assert.Equal(RegionRoute.TheOnlyOneHoldingAnArea, pick.Route);
        }

        [Fact]
        public void TheReasonNamesWhichOfTheCasesItWas()
        {
            Assert.Equal(
                "the plot has no filled region in the link",
                RegionChoice.WhyUnchosen(new RegionArea[0]));

            Assert.Equal(
                "no filled region on this plot holds an area",
                RegionChoice.WhyUnchosen(new[] { Region(Cadastral, 0.0) }));

            // Neither is the note's type, so this is the question that stays.
            Assert.Equal(
                "2 filled regions hold an area, RCRC_CADASTRAL LIMIT and "
                + "RCRC_SOMETHING NOBODY HAS MEASURED, none of them "
                + "RCRC_OUT OF SCOPE (PRESENTATION), and the type name cannot say which is the plot's",
                RegionChoice.WhyUnchosen(
                    new[] { Region(Cadastral, 900.0), Region(SomethingElse, 250.0) }));

            // Both are, so the note cannot separate them and the reason says that instead.
            Assert.Equal(
                "2 filled regions hold an area, RCRC_OUT OF SCOPE (PRESENTATION) and "
                + "RCRC_OUT OF SCOPE (PRESENTATION), and 2 of them are "
                + "RCRC_OUT OF SCOPE (PRESENTATION), so the client's note cannot say which",
                RegionChoice.WhyUnchosen(
                    new[] { Region(OutOfScope, 900.0), Region(OutOfScope, 250.0) }));

            Assert.Equal(OutOfScope, RegionChoice.TheNoteNames);

            Assert.Equal(
                string.Empty,
                RegionChoice.WhyUnchosen(new[] { Region(OutOfScope, 3728.757) }));
        }

        /// <summary>
        /// **A pair the note settles has no reason left to print**, because nothing about it is
        /// unchosen any more. That is the half of the change a person reads off the report.
        /// </summary>
        [Fact]
        public void APairTheNoteSettlesHasNoUnchosenReason()
        {
            Assert.Equal(
                string.Empty,
                RegionChoice.WhyUnchosen(
                    new[] { Region(Cadastral, 3982.0), Region(OutOfScope, 4295.0) }));
        }
    }
}
