using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// Whether a press of Create has to scan the model first.
    ///
    /// **Pressing Create used to want a scan the user had to know to do first.** The scan is
    /// a step inside Create now, so this is the rule that decides it: read when there is
    /// nothing held or what is held is another model, and otherwise use what is there. Every
    /// expected value below is written out by hand.
    /// </summary>
    public class ScanNeededTests
    {
        [Fact]
        public void NothingHeldMeansTheModelIsRead()
        {
            ScanSource decided = ScanNeeded.Decide(null, "RCRC_NG03_EZ");

            Assert.False(decided.Held);
            Assert.Equal("no scan was held, so the model was read", decided.Why);

            Assert.False(ScanNeeded.Decide(string.Empty, "RCRC_NG03_EZ").Held);
        }

        /// <summary>
        /// The one the round exists for: a scan of another model answers nothing about this
        /// one, and taking it would fill a workbook off a model nobody asked about.
        /// </summary>
        [Fact]
        public void AScanOfAnotherModelMeansTheModelIsReadAndBothAreNamed()
        {
            ScanSource decided = ScanNeeded.Decide("RCRC_NG05_NU_MAIN", "RCRC_NG03_EZ");

            Assert.False(decided.Held);
            Assert.Equal(
                "the scan held is of RCRC_NG05_NU_MAIN and this press is on RCRC_NG03_EZ, so the model was read",
                decided.Why);
        }

        /// <summary>
        /// The reuse the 1552 run measured, a second press finishing in seconds where the
        /// first read for two minutes.
        /// </summary>
        [Fact]
        public void AScanOfThisModelIsUsedAgainAndSaysSo()
        {
            ScanSource decided = ScanNeeded.Decide("RCRC_NG03_EZ", "RCRC_NG03_EZ");

            Assert.True(decided.Held);
            Assert.Equal("the scan held is of this model, so it was not read again", decided.Why);
        }

        /// <summary>
        /// The title is compared as it is written. Two models whose names differ only by case
        /// are two models, and Revit lets both be open at once.
        /// </summary>
        [Fact]
        public void TheTitleIsComparedExactly()
        {
            Assert.False(ScanNeeded.Decide("RCRC_NG03_EZ", "rcrc_ng03_ez").Held);
            Assert.False(ScanNeeded.Decide("RCRC_NG03_EZ ", "RCRC_NG03_EZ").Held);
        }

        /// <summary>
        /// No document is not a model the scan can answer for, so it reads rather than
        /// claiming the scan it holds covers it. Create refuses on no document anyway, and a
        /// decision that said otherwise here would be a second rule waiting to be trusted.
        /// </summary>
        [Fact]
        public void NoDocumentIsNeverAnsweredByAScanThatIsHeld()
        {
            ScanSource decided = ScanNeeded.Decide("RCRC_NG03_EZ", null);

            Assert.False(decided.Held);
            Assert.Equal(
                "the scan held is of RCRC_NG03_EZ and this press is on no model, so the model was read",
                decided.Why);
        }
    }
}
