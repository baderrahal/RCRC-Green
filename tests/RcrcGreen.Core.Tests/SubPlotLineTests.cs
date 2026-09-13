using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// What each line of step 1's sub plot list says.
    ///
    /// The three the brief asked for, from the run of 2026-09-13: a sub plot the model does not
    /// hold, one it holds with no scope box, and one with views.
    /// </summary>
    public class SubPlotLineTests
    {
        private static readonly ViewType Overall = new ViewType("010", "Overall Plan");
        private static readonly ViewType General =
            new ViewType("200", "General Arrangement Layout");

        private static PlotViewPresence On(string plotId, ViewType type)
        {
            return new PlotViewPresence(plotId, type, 0L);
        }

        private static IReadOnlyList<SubPlotLine> Three()
        {
            return SubPlotLines.Over(
                new[] { "DM-01", "DM-02", "DM-11" },
                new[] { "DM-02", "DM-11" },
                new[] { "DM-11" },
                new[] { On("DM-11", Overall), On("DM-11", General) });
        }

        [Fact]
        public void ASubPlotTheModelDoesNotHoldSaysSo()
        {
            Assert.Equal("DM-01   not in the model", Three()[0].ToString());
            Assert.False(Three()[0].InTheModel);
            Assert.Equal(0, Three()[0].Views);
        }

        [Fact]
        public void ASubPlotWithNoScopeBoxSaysThatBeforeItsViewCount()
        {
            SubPlotLine line = Three()[1];

            Assert.True(line.InTheModel);
            Assert.False(line.HasScopeBox);
            Assert.Equal("no scope box, no views", line.InWords());
        }

        [Fact]
        public void ASubPlotWithAScopeBoxSaysWhatItHolds()
        {
            SubPlotLine line = Three()[2];

            Assert.True(line.InTheModel);
            Assert.True(line.HasScopeBox);
            Assert.Equal(2, line.Views);
            Assert.Equal("2 views", line.InWords());
        }

        [Fact]
        public void OneViewIsSaidInTheSingular()
        {
            IReadOnlyList<SubPlotLine> lines = SubPlotLines.Over(
                new[] { "DM-11" },
                new[] { "DM-11" },
                new[] { "DM-11" },
                new[] { On("DM-11", Overall) });

            Assert.Equal("1 view", lines[0].InWords());
        }

        [Fact]
        public void ASubPlotInTheModelWithNeitherBoxNorViewsStillSaysBoth()
        {
            IReadOnlyList<SubPlotLine> lines = SubPlotLines.Over(
                new[] { "DM-02" }, new[] { "DM-02" }, null, null);

            Assert.Equal("no scope box, no views", lines[0].InWords());
        }

        [Fact]
        public void AViewOnAnotherSubPlotIsNotCountedOnThisOne()
        {
            IReadOnlyList<SubPlotLine> lines = SubPlotLines.Over(
                new[] { "DM-11", "DM-12" },
                new[] { "DM-11", "DM-12" },
                new[] { "DM-11", "DM-12" },
                new[] { On("DM-11", Overall), On("DM-11", General), On("DM-12", Overall) });

            Assert.Equal(2, lines[0].Views);
            Assert.Equal(1, lines[1].Views);
        }

        [Fact]
        public void TheCountLineHoldsTheCoverAgainstTheModel()
        {
            Assert.Equal(
                "3 sub plots, 2 of them in the model. The rest can still be ticked, and "
                    + "sheets can be made for them.",
                SubPlotLines.InWords(Three()));
        }

        [Fact]
        public void ACoverTheModelHoldsEntirelySaysSoWithoutTheCaveat()
        {
            IReadOnlyList<SubPlotLine> all = SubPlotLines.Over(
                new[] { "DM-11", "DM-12" },
                new[] { "DM-11", "DM-12" },
                null,
                null);

            Assert.Equal(
                "2 sub plots, and the model holds them all.", SubPlotLines.InWords(all));

            Assert.Equal(
                "1 sub plot, and the model holds it.",
                SubPlotLines.InWords(SubPlotLines.Over(
                    new[] { "DM-11" }, new[] { "DM-11" }, null, null)));
        }

        [Fact]
        public void ARangeCoveringNothingSaysToCheckTheEnds()
        {
            Assert.Equal(
                "This range covers no sub plot. Check the From and To.",
                SubPlotLines.InWords(new List<SubPlotLine>()));

            Assert.Equal(
                "This range covers no sub plot. Check the From and To.",
                SubPlotLines.InWords(null));
        }

        [Fact]
        public void EveryLineTheRangeCoversGetsOneWhateverTheModelKnows()
        {
            Assert.Equal(
                new[] { "DM-01", "DM-02", "DM-11" },
                Three().Select(one => one.PlotId).ToArray());

            Assert.Empty(SubPlotLines.Over(null, null, null, null));
        }
    }
}
