using System;
using System.Collections.Generic;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// The header's three states. Every expected value written out by hand.
    ///
    /// **THE HEADER MUST COST NOTHING.** The name is free and the element count is the read
    /// itself, so only a model something has read names a count.
    /// </summary>
    public class KpiHeaderTests
    {
        /// <summary>
        /// Shown with no document. The name is all there is to say, and the line under it is
        /// about opening one rather than about a read that has not happened to a model that
        /// does not exist.
        /// </summary>
        [Fact]
        public void ShownWithNoDocumentNamesNoModelAndNoCount()
        {
            IReadOnlyList<string> lines = KpiHeader.Lines(OpenModel.Nothing, ReadOfTheModel.NotYet);

            Assert.Equal(
                new[]
                {
                    "No model open",
                    "Open a model. This pane reads its name, which costs nothing, and nothing else."
                },
                lines);
        }

        /// <summary>
        /// Shown with a document and nothing read. The name, and a line that says why there is
        /// no count. **It must not say zero**, because zero is a number and this is an absence.
        /// </summary>
        [Fact]
        public void ShownWithADocumentNothingHasReadNamesTheModelAndNoCount()
        {
            IReadOnlyList<string> lines = KpiHeader.Lines(
                OpenModel.Of("RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached"), ReadOfTheModel.NotYet);

            Assert.Equal(
                new[]
                {
                    "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                    "Not read yet. Counting every element is the read itself, so the count waits for one."
                },
                lines);

            // The line under the name carries no digit at all, so nothing there can read as a
            // count of nought. The name itself holds digits and is the model's own.
            Assert.DoesNotContain(lines[1], char.IsDigit);
        }

        /// <summary>
        /// Shown with a document already read. **The only one of the three that names a count**,
        /// with the time and the seconds beside it because they say which state of the model
        /// the number describes.
        /// </summary>
        [Fact]
        public void ShownWithADocumentAlreadyReadIsTheOnlyOneThatNamesACount()
        {
            IReadOnlyList<string> lines = KpiHeader.Lines(
                OpenModel.Of("RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached"),
                ReadOfTheModel.TheWholeModel(new DateTime(2026, 9, 13, 8, 37, 4), 96959, 1.4));

            Assert.Equal(
                new[]
                {
                    "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                    "Read at 08:37:04, 96959 elements in 1.4 seconds."
                },
                lines);
        }

        /// <summary>
        /// **THE PLOTS READ IS NOT A READ OF THE MODEL AND THE LINE SAYS SO.** The 18:15 session
        /// showed the header carrying `108733 elements in 0.8 seconds` after the plots press and
        /// `46.2 seconds` after Create's scan, on the same model, printed the same way. A count
        /// beside a model name is a claim that the model was read, so the two now read
        /// differently and the plots one says what it really did.
        /// </summary>
        [Fact]
        public void ThePlotsPressCountsTheElementsAndSaysItDidNotReadTheModel()
        {
            IReadOnlyList<string> lines = KpiHeader.Lines(
                OpenModel.Of("RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached"),
                ReadOfTheModel.ThePlots(new DateTime(2026, 9, 14, 18, 6, 58), 108733, 0.8));

            Assert.Equal(
                "Counted at 18:06:58, 108733 elements in 0.8 seconds."
                + " The plots were read, and the elements counted. The model itself was not read.",
                lines[1]);

            Assert.False(
                ReadOfTheModel.ThePlots(new DateTime(2026, 9, 14, 18, 6, 58), 108733, 0.8).WholeModel);
            Assert.True(
                ReadOfTheModel.TheWholeModel(new DateTime(2026, 9, 14, 18, 9, 37), 108733, 46.2).WholeModel);
        }

        /// <summary>
        /// A read that has not happened carries no numbers at all, so nothing downstream can
        /// print one off it by mistake.
        /// </summary>
        [Fact]
        public void AReadThatHasNotHappenedHoldsNothing()
        {
            Assert.False(ReadOfTheModel.NotYet.Happened);
            Assert.Equal(0, ReadOfTheModel.NotYet.Elements);
            Assert.True(ReadOfTheModel.TheWholeModel(new DateTime(2026, 9, 13, 8, 37, 4), 1, 0.1).Happened);
            Assert.True(ReadOfTheModel.ThePlots(new DateTime(2026, 9, 13, 8, 37, 4), 1, 0.1).Happened);
        }

        /// <summary>
        /// Nothing hands the header a null and gets a throw: a pane drawn before any answer has
        /// come back is the ordinary case, and it reads as the no document state.
        /// </summary>
        [Fact]
        public void NothingAnsweredYetReadsAsNoDocument()
        {
            Assert.Equal(
                new[]
                {
                    "No model open",
                    "Open a model. This pane reads its name, which costs nothing, and nothing else."
                },
                KpiHeader.Lines(null, null));
        }
    }
}
