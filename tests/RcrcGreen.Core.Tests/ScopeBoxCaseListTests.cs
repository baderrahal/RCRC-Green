using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class ScopeBoxCaseListTests
    {
        /// <summary>
        /// Laid out so each case has a known occupant, read off by hand below rather than
        /// worked out with the same rule the code uses.
        ///
        /// DM-2 has one C. DM-11 has one E and one F. DM-100 has one D.
        /// </summary>
        private static readonly ViewScopeBoxState[] Views =
        {
            new ViewScopeBoxState(1, "DM-11-(200) General Arrangement Layout", true, "DM-41"),
            new ViewScopeBoxState(2, "DM-11-(010) Location Key Plan", true, "DM-11"),
            new ViewScopeBoxState(3, "DM-100-(200) General Arrangement Layout", true, string.Empty),
            new ViewScopeBoxState(4, "DM-2-(200) General Arrangement Layout", true, string.Empty)
        };

        private static readonly string[] Boxes = { "DM-2", "DM-11", "DM-41" };

        private static readonly string[] Plots = { "DM-2", "DM-11", "DM-100" };

        /// <summary>
        /// The one that matters on the real model. F equals 1, and the useful thing in that
        /// number is which view it is and which box it wrongly holds.
        /// </summary>
        [Fact]
        public void TheOneViewHoldingTheWrongScopeBoxComesBackByName()
        {
            ScopeBoxCounts counts = ScopeBoxCounts.For(Views, Boxes, Plots);

            ViewScopeBoxDecision wrong = Assert.Single(counts.In(ScopeBoxCase.HoldsADifferentScopeBox));

            Assert.Equal("DM-11-(200) General Arrangement Layout", wrong.ViewName);
            Assert.Equal("DM-11", wrong.PlotId);
            Assert.Equal("DM-41", wrong.CurrentScopeBoxName);
            Assert.Equal(1, wrong.ViewId);
        }

        [Fact]
        public void TheViewIdIsCarriedSoTheViewCanBeOpened()
        {
            ScopeBoxCounts counts = ScopeBoxCounts.For(Views, Boxes, Plots);

            Assert.Equal(
                new long[] { 3 },
                counts.In(ScopeBoxCase.NoMatchingScopeBox).Select(one => one.ViewId).ToArray());
        }

        [Fact]
        public void EveryCaseListHasAsManyEntriesAsItsCount()
        {
            ScopeBoxCounts counts = ScopeBoxCounts.For(Views, Boxes, Plots);

            foreach (ScopeBoxCase outcome in new[]
            {
                ScopeBoxCase.NameDoesNotParse,
                ScopeBoxCase.CannotHoldAScopeBox,
                ScopeBoxCase.ReadyToAssign,
                ScopeBoxCase.NoMatchingScopeBox,
                ScopeBoxCase.AlreadyRight,
                ScopeBoxCase.HoldsADifferentScopeBox
            })
            {
                Assert.Equal(counts.Of(outcome), counts.In(outcome).Count);
            }
        }

        /// <summary>
        /// Plots read as numbers, so DM-2 comes before DM-100 in the list a person scrolls.
        /// </summary>
        [Fact]
        public void TheListIsInPlotOrderWithTheNumberReadAsANumber()
        {
            var everything = new[]
            {
                new ViewScopeBoxState(1, "DM-100-(200) General Arrangement Layout", true, string.Empty),
                new ViewScopeBoxState(2, "DM-2-(200) General Arrangement Layout", true, string.Empty),
                new ViewScopeBoxState(3, "DM-11-(200) General Arrangement Layout", true, string.Empty)
            };

            ScopeBoxCounts counts = ScopeBoxCounts.For(
                everything, new[] { "DM-2", "DM-11", "DM-100" }, new[] { "DM-2", "DM-11", "DM-100" });

            Assert.Equal(
                new[] { "DM-2", "DM-11", "DM-100" },
                counts.In(ScopeBoxCase.ReadyToAssign).Select(one => one.PlotId).ToArray());
        }

        [Fact]
        public void ACaseWithNothingInItIsAnEmptyListRatherThanNull()
        {
            ScopeBoxCounts counts = ScopeBoxCounts.For(Views, Boxes, Plots);

            Assert.Empty(counts.In(ScopeBoxCase.NameDoesNotParse));
            Assert.Equal(0, counts.Of(ScopeBoxCase.NameDoesNotParse));
        }

        [Fact]
        public void UntickingAPlotTakesItsViewsOutOfEveryList()
        {
            ScopeBoxCounts without = ScopeBoxCounts.For(Views, Boxes, new[] { "DM-2", "DM-100" });

            Assert.Empty(without.In(ScopeBoxCase.HoldsADifferentScopeBox));
            Assert.Empty(without.In(ScopeBoxCase.AlreadyRight));
            Assert.Single(without.In(ScopeBoxCase.ReadyToAssign));
        }
    }

    public class ReportPlacesTests
    {
        /// <summary>
        /// One place, said in full. A report used to go to the Desktop as well, so the line
        /// named two paths and somebody had two files per run to keep straight.
        /// </summary>
        [Fact]
        public void TheOnePlaceItLandedIsNamed()
        {
            Assert.Equal(
                @"Report at C:\repo\reports\run.txt.",
                ReportPlaces.Written(new[] { @"C:\repo\reports\run.txt" }));
        }

        /// <summary>
        /// Nothing written is not a footnote. The pointer file is the only thing that says where
        /// the reports folder is, and with it missing there is no second place the file might be
        /// in, so the line leads with that and says what to do about it.
        /// </summary>
        [Fact]
        public void NothingWrittenSaysWhyAndWhatToDo()
        {
            foreach (string said in new[]
            {
                ReportPlaces.Written(null),
                ReportPlaces.Written(new string[0]),
                ReportPlaces.Written(new[] { string.Empty, null })
            })
            {
                Assert.StartsWith("NO REPORT WAS WRITTEN", said);
                Assert.Contains("reports-folder.txt", said);
                Assert.Contains("install.ps1", said);
            }
        }

        /// <summary>
        /// The old line for one path said the Desktop copy was the only one and the repo folder
        /// was not found. There is no Desktop copy now, so no line may read that way.
        /// </summary>
        [Fact]
        public void NoLineOffersASecondPlaceAnyMore()
        {
            Assert.DoesNotContain("Desktop", ReportPlaces.Written(null));

            string written = ReportPlaces.Written(new[] { @"C:\repo\reports\run.txt" });

            Assert.DoesNotContain("Desktop", written);

            // The paths were joined with this, so its absence is what says one place is one
            // place. The refusal line above is prose and is allowed the word.
            Assert.DoesNotContain(" and ", written);
        }
    }
}
