using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class ViewPlotReaderTests
    {
        [Fact]
        public void TheParameterBeatsTheNameWhenBothAreThere()
        {
            ViewPlotReading reading = ViewPlotReader.Read("PF-12", "DM-41-(010) Location Key Plan");

            Assert.Equal("PF-12", reading.PlotId);
            Assert.Equal(PlotSourceOnView.Parameter, reading.Source);
            Assert.True(reading.Found);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\r\n")]
        public void TheNameIsUsedWhenTheParameterIsEmptyOrAbsent(string parameter)
        {
            ViewPlotReading reading = ViewPlotReader.Read(parameter, "DM-41-(010) Location Key Plan");

            Assert.Equal("DM-41", reading.PlotId);
            Assert.Equal(PlotSourceOnView.ViewName, reading.Source);
        }

        [Fact]
        public void TheNameThatTheScopeBoxRunSkippedIsStillNotAPlotOnItsOwn()
        {
            ViewPlotReading reading = ViewPlotReader.Read(null, "SOFTSCAPE SCHEDULES");

            Assert.False(reading.Found);
            Assert.Equal(PlotSourceOnView.None, reading.Source);
        }

        [Fact]
        public void ThatSameViewIsRecoveredOnceTheParameterIsRead()
        {
            ViewPlotReading reading = ViewPlotReader.Read("DM-41", "SOFTSCAPE SCHEDULES");

            Assert.Equal("DM-41", reading.PlotId);
            Assert.Equal(PlotSourceOnView.Parameter, reading.Source);
        }

        [Fact]
        public void SurroundingWhitespaceOnTheParameterDoesNotStopIt()
        {
            ViewPlotReading reading = ViewPlotReader.Read("  DM-41\r\n", "SOFTSCAPE SCHEDULES");

            Assert.Equal("DM-41", reading.PlotId);
            Assert.Equal(PlotSourceOnView.Parameter, reading.Source);
        }

        [Theory]
        [InlineData("dm-41")]
        [InlineData("NG05")]
        [InlineData("Plot 41")]
        public void AParameterThatIsPresentAndWrongDoesNotFallBackToTheName(string parameter)
        {
            ViewPlotReading reading = ViewPlotReader.Read(parameter, "DM-41-(010) Location Key Plan");

            Assert.False(reading.Found);
            Assert.Equal(PlotSourceOnView.ParameterNotAPlot, reading.Source);
            Assert.Equal(parameter, reading.RawParameterValue);
        }

        [Fact]
        public void NeitherSourceGivingAnythingIsAnOrdinaryAnswer()
        {
            ViewPlotReading reading = ViewPlotReader.Read(null, null);

            Assert.False(reading.Found);
            Assert.Equal(PlotSourceOnView.None, reading.Source);
            Assert.Empty(reading.PlotId);
            Assert.Empty(reading.RawParameterValue);
        }

        [Fact]
        public void AModelWithNoPlotParameterAnywhereStillReadsEveryNameItCan()
        {
            string[] names =
            {
                "DM-41-(010) Location Key Plan",
                "SOFTSCAPE SCHEDULES",
                "PF-12-(200) General Arrangement Layout"
            };

            ViewPlotReading[] readings = System.Array.ConvertAll(
                names, name => ViewPlotReader.Read(null, name));

            Assert.Equal("DM-41", readings[0].PlotId);
            Assert.False(readings[1].Found);
            Assert.Equal("PF-12", readings[2].PlotId);
        }
    }
}
