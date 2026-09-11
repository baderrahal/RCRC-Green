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
        private static ViewportRecord Placed(
            int scale = 250,
            bool schedule = false,
            string scaleAsShown = null,
            string viewportType = null)
        {
            return new ViewportRecord(
                "200QA",
                "GENERAL ARRANGEMENT LAYOUT",
                "DM-11-(200) General Arrangement Layout",
                scale,
                1.0, 0.5,
                2.0, 1.0,
                2.5, 1.5,
                schedule,
                scaleAsShown,
                viewportType);
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

        /// <summary>
        /// DM-11-(200) General Arrangement Layout reads Custom over a Scale Value of 250, under
        /// a template named for 250, and the report said 1:250 with nothing about the Custom.
        /// The two do not disagree about the number, so both are printed and neither is dropped.
        /// A report and a model that differ by a word are the same class of fault as a report
        /// that says created and not created.
        /// </summary>
        [Fact]
        public void AScaleRevitLabelsCustomSaysBothTheRatioAndTheLabel()
        {
            Assert.Equal("1:250, shown as Custom", Placed(scaleAsShown: "Custom").ScaleInWords());

            Assert.Contains(
                "at 1:250, shown as Custom, 609.6 by 304.8 mm",
                Placed(scaleAsShown: "Custom").InWords());
        }

        /// <summary>
        /// Where Revit shows exactly the ratio, saying it twice would be noise.
        /// </summary>
        [Fact]
        public void AScaleShownAsTheRatioIsSaidOnce()
        {
            Assert.Equal("1:250", Placed(scaleAsShown: "1:250").ScaleInWords());
            Assert.Equal("1:250", Placed(scaleAsShown: "").ScaleInWords());
        }

        /// <summary>
        /// Every viewport the first real run made came out PRX_Title With Line and the report
        /// did not say so, because nothing recorded it. It is taken off the sibling now and
        /// named here, so which type a placement got is readable without opening the sheet.
        /// </summary>
        [Fact]
        public void TheViewportTypeIsNamedWhereThereIsOne()
        {
            Assert.EndsWith(
                "on a sheet of 762 by 457.2 mm. Viewport type PRX_Title With Line.",
                Placed(viewportType: "PRX_Title With Line").InWords());

            Assert.EndsWith("on a sheet of 762 by 457.2 mm.", Placed().InWords());
            Assert.Equal(string.Empty, Placed().ViewportTypeName);
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
