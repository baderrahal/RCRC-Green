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
    /// one that runs, and three of them are exactly where the fixture's copy answered
    /// differently.
    /// </summary>
    public sealed class RegionChoiceTests
    {
        private const string Cadastral = "CADASTRAL LIMIT";
        private const string OutOfScope = "OUT OF SCOPE (PRESENTATION)";

        private static RegionArea Region(string typeName, double squareMetres)
        {
            return new RegionArea(typeName, squareMetres * 10.7639, squareMetres, string.Empty);
        }

        [Fact]
        public void OneRegionHoldingAnAreaAnswersItself()
        {
            Assert.Equal(
                OutOfScope,
                RegionChoice.For(new[] { Region(OutOfScope, 3728.757) }, null));
        }

        /// <summary>
        /// DM-11, DM-12 and DM-13 hold the area on OUT OF SCOPE with cadastral at 0, so the one
        /// holding nothing is passed over rather than counted.
        /// </summary>
        [Fact]
        public void TheRegionHoldingNothingIsPassedOver()
        {
            Assert.Equal(
                OutOfScope,
                RegionChoice.For(
                    new[] { Region(Cadastral, 0.0), Region(OutOfScope, 3728.757) },
                    null));
        }

        /// <summary>
        /// **The fixture's copy answered CADASTRAL LIMIT here**, because it took the first
        /// region it was handed. Two regions holding an area is a question the type name
        /// cannot settle.
        /// </summary>
        [Fact]
        public void TwoRegionsHoldingAnAreaChooseNothing()
        {
            Assert.Equal(
                string.Empty,
                RegionChoice.For(
                    new[] { Region(Cadastral, 900.0), Region(OutOfScope, 250.0) },
                    null));
        }

        /// <summary>
        /// **The fixture's copy answered CADASTRAL LIMIT here too.** A region holding no area
        /// is not a choice, it is the absence of one.
        /// </summary>
        [Fact]
        public void ASingleRegionHoldingNoAreaChoosesNothing()
        {
            Assert.Equal(
                string.Empty,
                RegionChoice.For(new[] { Region(Cadastral, 0.0) }, null));
        }

        [Fact]
        public void NoRegionsChooseNothing()
        {
            Assert.Equal(string.Empty, RegionChoice.For(new RegionArea[0], null));
            Assert.Equal(string.Empty, RegionChoice.For(null, null));
        }

        /// <summary>
        /// A person's pick is an answer and the rule is only a guess at one, so it wins even
        /// where the rule would have chosen for itself.
        /// </summary>
        [Fact]
        public void APickByHandWinsOverTheRule()
        {
            Assert.Equal(
                Cadastral,
                RegionChoice.For(
                    new[] { Region(Cadastral, 900.0), Region(OutOfScope, 250.0) },
                    Cadastral));

            Assert.Equal(
                Cadastral,
                RegionChoice.For(new[] { Region(OutOfScope, 3728.757) }, Cadastral));
        }

        /// <summary>
        /// An empty string is nobody having picked, not a pick of nothing, because that is what
        /// the handler reads back off a pane where no button has been pressed.
        /// </summary>
        [Fact]
        public void AnEmptyPickIsNotAPick()
        {
            Assert.Equal(
                OutOfScope,
                RegionChoice.For(new[] { Region(OutOfScope, 3728.757) }, string.Empty));
        }

        [Fact]
        public void TheReasonNamesWhichOfTheThreeCasesItWas()
        {
            Assert.Equal(
                "the plot has no filled region in the link",
                RegionChoice.WhyUnchosen(new RegionArea[0]));

            Assert.Equal(
                "no filled region on this plot holds an area",
                RegionChoice.WhyUnchosen(new[] { Region(Cadastral, 0.0) }));

            Assert.Equal(
                "2 filled regions hold an area, " + Cadastral + " and " + OutOfScope
                + ", and the type name cannot say which is the plot's",
                RegionChoice.WhyUnchosen(
                    new[] { Region(Cadastral, 900.0), Region(OutOfScope, 250.0) }));

            Assert.Equal(
                string.Empty,
                RegionChoice.WhyUnchosen(new[] { Region(OutOfScope, 3728.757) }));
        }
    }
}
