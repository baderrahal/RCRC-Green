using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// One placement in words. The numbers go in as feet, which is what Revit hands back, and
    /// come out in millimetres, which is what a person checks a sheet in. One foot is 304.8
    /// millimetres and every expected number below is that arithmetic done by hand.
    /// </summary>
    public class ViewportRecordTests
    {
        private static ViewportRecord Placed(int scale = 250, bool schedule = false)
        {
            return new ViewportRecord(
                "200QA",
                "GENERAL ARRANGEMENT LAYOUT",
                "DM-11-(200) General Arrangement Layout",
                scale,
                1.0, 0.5,
                2.0, 1.0,
                2.5, 1.5,
                schedule);
        }

        [Fact]
        public void TheWholePlacementReadsAsOneLine()
        {
            Assert.Equal(
                "On 200QA GENERAL ARRANGEMENT LAYOUT: DM-11-(200) General Arrangement Layout "
                + "at 1:250, 609.6 by 304.8 mm, centre at 304.8 by 152.4 mm, on a sheet of "
                + "762 by 457.2 mm.",
                Placed().InWords());
        }

        /// <summary>
        /// A schedule has no scale, so its line says what it is instead of printing 1:0.
        /// </summary>
        [Fact]
        public void AScheduleSaysSoInsteadOfAScale()
        {
            Assert.Contains(", a schedule, ", Placed(0, true).InWords());
            Assert.DoesNotContain("1:0", Placed(0, true).InWords());
        }

        /// <summary>
        /// The scale belongs to the view, off its template. A view without one reads as no
        /// scale rather than as a number that means nothing.
        /// </summary>
        [Fact]
        public void TheScaleIsTheViewsOwnOrNothing()
        {
            Assert.Equal("1:250", Placed().ScaleInWords());
            Assert.Equal("no scale", Placed(0).ScaleInWords());
        }

        [Fact]
        public void EveryMeasurementIsSaidInMillimetres()
        {
            ViewportRecord placed = Placed();

            Assert.Equal("609.6 by 304.8 mm", placed.SizeInWords());
            Assert.Equal("304.8 by 152.4 mm", placed.CentreInWords());
            Assert.Equal("762 by 457.2 mm", placed.SheetSizeInWords());
        }

        [Fact]
        public void NothingComesBackNull()
        {
            var bare = new ViewportRecord(
                null, null, null, 0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, false);

            Assert.Equal(string.Empty, bare.SheetNumber);
            Assert.Equal(string.Empty, bare.SheetName);
            Assert.Equal(string.Empty, bare.ViewName);
        }
    }
}
