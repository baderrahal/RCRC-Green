using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// Marking more than one cell at a time.
    ///
    /// 17 plots by 8 view types is 136 clicks, which is what the first person to use the grid
    /// actually did. Every expected list here is written out by hand.
    /// </summary>
    public class BulkMarkingTests
    {
        private static readonly ViewType General = new ViewType("200", "General Arrangement Layout");
        private static readonly ViewType KeyPlan = new ViewType("010", "Location Key Plan");
        private static readonly ViewType CrossSection = new ViewType("400", "Landscape Cross Section");

        /// <summary>
        /// Three plots by three view types. DM-11 already has the (200), and DM-12's (010) is
        /// already marked, so seven of the nine cells are missing.
        /// </summary>
        private static SheetGrid Grid()
        {
            return SheetGrid.Build(
                new[] { "DM-11", "DM-12", "DM-13" },
                new[] { KeyPlan, General, CrossSection },
                new[] { new PlotViewPresence("DM-11", General, 4001) },
                new[] { "DM-11", "DM-12", "DM-13" },
                new[] { new PlotViewKey("DM-12", KeyPlan) });
        }

        private static string[] Names(System.Collections.Generic.IEnumerable<PlotViewKey> keys)
        {
            return keys.Select(one => one.PlotId + " " + one.ViewType).ToArray();
        }

        [Fact]
        public void EveryMissingCellIsOneMarkAndTheFullOnesAreLeftAlone()
        {
            var marks = BulkMarking.EveryMissing(Grid(), new[] { "DM-11", "DM-12", "DM-13" });

            Assert.Equal(
                new[]
                {
                    "DM-11 (010) Location Key Plan",
                    "DM-11 (400) Landscape Cross Section",
                    "DM-12 (200) General Arrangement Layout",
                    "DM-12 (400) Landscape Cross Section",
                    "DM-13 (010) Location Key Plan",
                    "DM-13 (200) General Arrangement Layout",
                    "DM-13 (400) Landscape Cross Section"
                },
                Names(marks));
        }

        /// <summary>
        /// A cell holding a view is never marked, because marking says a view is wanted and
        /// that one is there. This is the rule a single click on the grid already follows.
        /// </summary>
        [Fact]
        public void AViewThatExistsIsNeverMarked()
        {
            var marks = BulkMarking.EveryMissing(Grid(), new[] { "DM-11" });

            Assert.DoesNotContain("DM-11 (200) General Arrangement Layout", Names(marks));
        }

        /// <summary>
        /// A cell already marked comes back once from the set the panel holds, not twice, so a
        /// sweep after a sweep adds nothing.
        /// </summary>
        [Fact]
        public void ACellAlreadyMarkedIsNotOfferedAgain()
        {
            var marks = BulkMarking.EveryMissing(Grid(), new[] { "DM-12" });

            Assert.DoesNotContain("DM-12 (010) Location Key Plan", Names(marks));
        }

        /// <summary>
        /// The run only makes things on ticked plots, so a sweep that marked an unticked one
        /// would leave marks the run drops without saying anything.
        /// </summary>
        [Fact]
        public void AnUntickedPlotIsNeverSweptUp()
        {
            var marks = BulkMarking.EveryMissing(Grid(), new[] { "DM-11", "DM-13" });

            Assert.DoesNotContain("DM-12", string.Join(" ", Names(marks)));
            Assert.Equal(5, marks.Count);

            Assert.Empty(BulkMarking.EveryMissing(Grid(), null));
        }

        [Fact]
        public void AWholeRowIsTheOnePlotAndNothingElse()
        {
            var marks = BulkMarking.WholeRow(Grid(), new[] { "DM-11", "DM-12", "DM-13" }, "DM-13");

            Assert.Equal(
                new[]
                {
                    "DM-13 (010) Location Key Plan",
                    "DM-13 (200) General Arrangement Layout",
                    "DM-13 (400) Landscape Cross Section"
                },
                Names(marks));
        }

        [Fact]
        public void AWholeColumnIsTheOneViewTypeDownEveryTickedPlot()
        {
            var marks = BulkMarking.WholeColumn(
                Grid(), new[] { "DM-11", "DM-12", "DM-13" }, CrossSection);

            Assert.Equal(
                new[]
                {
                    "DM-11 (400) Landscape Cross Section",
                    "DM-12 (400) Landscape Cross Section",
                    "DM-13 (400) Landscape Cross Section"
                },
                Names(marks));
        }

        /// <summary>
        /// A column with a view in it skips that row and takes the rest, rather than refusing
        /// the whole sweep.
        /// </summary>
        [Fact]
        public void AColumnPartlyFullTakesTheRest()
        {
            var marks = BulkMarking.WholeColumn(
                Grid(), new[] { "DM-11", "DM-12", "DM-13" }, General);

            Assert.Equal(
                new[]
                {
                    "DM-12 (200) General Arrangement Layout",
                    "DM-13 (200) General Arrangement Layout"
                },
                Names(marks));
        }

        [Fact]
        public void ARowOrColumnThatIsNotThereMarksNothing()
        {
            var ticked = new[] { "DM-11", "DM-12", "DM-13" };

            Assert.Empty(BulkMarking.WholeRow(Grid(), ticked, "DM-99"));
            Assert.Empty(BulkMarking.WholeRow(Grid(), ticked, null));
            Assert.Empty(BulkMarking.WholeColumn(Grid(), ticked, new ViewType("900", "Nothing")));
            Assert.Empty(BulkMarking.WholeColumn(Grid(), ticked, null));
            Assert.Empty(BulkMarking.EveryMissing(null, ticked));
        }

        [Fact]
        public void TheWordsCountWhatWasAddedAndWhatIsMarkedNow()
        {
            Assert.Equal(
                "7 cells marked, 8 in total. Marking records intent and changes nothing until Run.",
                BulkMarking.InWords(7, 8));

            Assert.Equal(
                "1 cell marked, 1 in total. Marking records intent and changes nothing until Run.",
                BulkMarking.InWords(1, 1));
        }

        [Fact]
        public void ASweepThatFindsNothingSaysWhyRatherThanNothing()
        {
            Assert.Equal(
                "Nothing there to mark. Every cell in it is already marked, already holds a view, "
                + "or belongs to a plot that is not ticked.",
                BulkMarking.InWords(0, 12));
        }

        [Fact]
        public void ClearingSaysHowManyWentAndThatTheModelIsUntouched()
        {
            Assert.Equal(
                "34 marks cleared. Nothing in the model has changed.",
                BulkMarking.ClearedInWords(34));

            Assert.Equal(
                "1 mark cleared. Nothing in the model has changed.",
                BulkMarking.ClearedInWords(1));

            Assert.Equal("Nothing was marked.", BulkMarking.ClearedInWords(0));
        }
    }

    /// <summary>
    /// Column headers narrow enough to read across a docked pane.
    /// </summary>
    public class GridColumnLabelsTests
    {
        private static readonly ViewType General = new ViewType("200", "General Arrangement Layout");
        private static readonly ViewType LocationKey = new ViewType("010", "Location Key Plan");
        private static readonly ViewType OverallKey = new ViewType("010", "Overall Key Plan");
        private static readonly ViewType Overall = new ViewType("010", "Overall Plan");

        private static string[] Shorts(params ViewType[] shown)
        {
            return GridColumnLabels.For(shown).Select(one => one.Short).ToArray();
        }

        [Fact]
        public void ACodeUsedOnceIsJustTheCode()
        {
            Assert.Equal(new[] { "(200)" }, Shorts(General));
        }

        /// <summary>
        /// The real pair. Code 010 covers both, so the code alone would put two columns under
        /// one header and there would be no way to tell which square belonged to which.
        /// </summary>
        [Fact]
        public void ASharedCodeTakesTheFirstWordThatTellsThemApart()
        {
            Assert.Equal(
                new[] { "(010) Location", "(010) Overall" },
                Shorts(LocationKey, OverallKey));
        }

        /// <summary>
        /// Two names starting with the same word need the second one as well, and no more.
        /// </summary>
        [Fact]
        public void TwoNamesSharingAFirstWordTakeASecond()
        {
            Assert.Equal(
                new[] { "(010) Overall Key", "(010) Overall Plan" },
                Shorts(OverallKey, Overall));
        }

        [Fact]
        public void ThreeSharingACodeEachTakeOnlyWhatTheyNeed()
        {
            Assert.Equal(
                new[] { "(010) Location", "(010) Overall Key", "(010) Overall Plan" },
                Shorts(LocationKey, OverallKey, Overall));
        }

        /// <summary>
        /// One name is the whole start of the other, so nothing shorter than the whole thing
        /// separates them and the header says the whole thing rather than lying by half.
        /// </summary>
        [Fact]
        public void ANameThatIsTheStartOfAnotherFallsBackToTheWholeThing()
        {
            var shorter = new ViewType("300", "Layout");
            var longer = new ViewType("300", "Layout Detail");

            Assert.Equal(
                new[] { "(300) Layout", "(300) Layout Detail" },
                Shorts(shorter, longer));
        }

        [Fact]
        public void TheFullNameAndWhetherItWasShortenedComeBackToo()
        {
            var labels = GridColumnLabels.For(new[] { General, LocationKey, OverallKey });

            Assert.Equal("(200) General Arrangement Layout", labels[0].Full);
            Assert.True(labels[0].Shortened);

            Assert.Equal("(010) Location Key Plan", labels[1].Full);
            Assert.True(labels[1].Shortened);
        }

        [Fact]
        public void ALabelIsNeverShorterThanItsOwnCode()
        {
            foreach (GridColumnLabel label in GridColumnLabels.For(new[] { General, LocationKey, OverallKey }))
            {
                Assert.StartsWith("(" + label.Type.Code + ")", label.Short);
            }
        }

        [Fact]
        public void NoColumnsIsNoLabels()
        {
            Assert.Empty(GridColumnLabels.For(null));
            Assert.Empty(GridColumnLabels.For(new ViewType[0]));
        }
    }
}
