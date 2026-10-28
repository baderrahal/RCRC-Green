using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// Question 6. A read Revit refused left Read model grey until somebody clicked a step
    /// circle, because only the status line was painted again.
    /// </summary>
    public class PressEndTests
    {
        [Fact]
        public void ARefusedReadDrawsTheStepsAgainSoReadModelIsUsableAtOnce()
        {
            Assert.True(PressEnd.RedrawsTheSteps(createWasRunning: false, readWasRunning: true));
        }

        [Fact]
        public void ARefusedCreateStillDrawsTheStepsAgain()
        {
            Assert.True(PressEnd.RedrawsTheSteps(createWasRunning: true, readWasRunning: false));
        }

        [Fact]
        public void AnEndLineWithNothingRunningPaintsTheStatusLineAlone()
        {
            Assert.False(PressEnd.RedrawsTheSteps(createWasRunning: false, readWasRunning: false));
        }
    }
}
