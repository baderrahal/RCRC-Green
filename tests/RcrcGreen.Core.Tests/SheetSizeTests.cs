using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// How big a sheet is, and where that number came from.
    ///
    /// Three sheets were created empty and the report said the title block reported no width or
    /// height. AR-PRX-Title_Block_A1 is a real A1 sheet. Sheet Width and Sheet Height are
    /// read-only instance parameters, so reading them off the title block TYPE returns nothing,
    /// and nothing became zero.
    ///
    /// Every millimetre below is written out by hand from the A1 an architect would name, not
    /// worked out with the same multiplication the code uses.
    /// </summary>
    public class SheetSizeTests
    {
        /// <summary>
        /// 841 by 594 millimetres in feet, which is what Revit hands back.
        /// </summary>
        private const double A1WideFeet = 2.7591864;

        private const double A1TallFeet = 1.9488189;

        private const string Block = "AR-PRX-Title_Block_A1 GA-DETAILED DESIGN";

        [Fact]
        public void ASizeReadOffTheParametersSaysSoAndSaysHowBig()
        {
            SheetSize size = SheetSize.Of(
                SheetSizeSource.TitleBlockParameters, A1WideFeet, A1TallFeet, Block);

            Assert.True(size.CanBeUsed);
            Assert.Equal(A1WideFeet, size.WidthFeet);
            Assert.Equal(A1TallFeet, size.HeightFeet);
            Assert.Equal(
                "841 by 594 mm, from the Sheet Width and Sheet Height of "
                + "AR-PRX-Title_Block_A1 GA-DETAILED DESIGN.",
                size.InWords());
        }

        /// <summary>
        /// A title block family that does not drive those two parameters is still a real sheet,
        /// so it is measured instead and the report says which of the two reads answered.
        /// </summary>
        [Fact]
        public void AMeasuredSizeIsNotPassedOffAsAReadOne()
        {
            SheetSize size = SheetSize.Of(
                SheetSizeSource.TitleBlockOutline, A1WideFeet, A1TallFeet, Block);

            Assert.True(size.CanBeUsed);
            Assert.Equal(
                "841 by 594 mm, measured across AR-PRX-Title_Block_A1 GA-DETAILED DESIGN, "
                + "which does not set Sheet Width and Sheet Height.",
                size.InWords());
        }

        [Fact]
        public void ASizeThatWasNotReadCanNeverBeUsed()
        {
            SheetSize size = SheetSize.NotRead(Block);

            Assert.False(size.CanBeUsed);
            Assert.Equal(SheetSizeSource.NotRead, size.Source);
            Assert.Equal(0.0, size.WidthFeet);
            Assert.Equal(0.0, size.HeightFeet);
        }

        /// <summary>
        /// The guard that was doing the work before, moved to where it cannot be skipped. A zero
        /// or a NaN is not a size, and a NaN in particular survives an ordering check and then
        /// turns every viewport centre into NaN, which comes back looking like a placement.
        /// </summary>
        [Fact]
        public void ANumberThatIsNotALengthIsNotASize()
        {
            Assert.False(SheetSize.Of(SheetSizeSource.TitleBlockParameters, 0.0, A1TallFeet, Block).CanBeUsed);
            Assert.False(SheetSize.Of(SheetSizeSource.TitleBlockParameters, A1WideFeet, 0.0, Block).CanBeUsed);
            Assert.False(SheetSize.Of(SheetSizeSource.TitleBlockParameters, -1.0, A1TallFeet, Block).CanBeUsed);
            Assert.False(SheetSize.Of(SheetSizeSource.TitleBlockParameters, double.NaN, A1TallFeet, Block).CanBeUsed);
            Assert.False(SheetSize.Of(
                SheetSizeSource.TitleBlockParameters, A1WideFeet, double.PositiveInfinity, Block).CanBeUsed);
        }

        [Fact]
        public void ASourceOfNoneIsNeverASizeEvenWithNumbersOnIt()
        {
            Assert.False(SheetSize.Of(SheetSizeSource.NotRead, A1WideFeet, A1TallFeet, Block).CanBeUsed);
        }

        /// <summary>
        /// The refusal names the two things somebody would go and look at, and claims nothing
        /// about what happened to the sheet. Whether the delete worked is the writer's to say.
        /// </summary>
        [Fact]
        public void TheRefusalNamesTheSheetTheBlockAndBothReads()
        {
            string why = SheetSize.NotRead(Block).WhyNotInWords("L-211 GENERAL ARRANGEMENT LAYOUT");

            Assert.StartsWith("L-211 GENERAL ARRANGEMENT LAYOUT was not made.", why);
            Assert.Contains("AR-PRX-Title_Block_A1 GA-DETAILED DESIGN", why);
            Assert.Contains("Sheet Width and Sheet Height are read off the title block placed", why);
            Assert.Contains("measuring across that block gave nothing either", why);
            Assert.DoesNotContain("deleted", why);
        }

        [Fact]
        public void AnUnnamedTitleBlockStillReadsAsASentence()
        {
            Assert.Contains("that title block", SheetSize.NotRead(null).WhyNotInWords("L-211"));
            Assert.Equal("Size not read from that title block.", SheetSize.NotRead(null).InWords());
        }

        /// <summary>
        /// A sheet that is not A1. Written by hand from an A0 at 1189 by 841.
        /// </summary>
        [Fact]
        public void AnotherSheetSizeReadsBackJustAsPlainly()
        {
            SheetSize size = SheetSize.Of(
                SheetSizeSource.TitleBlockParameters, 3.9009186, A1WideFeet, "A0");

            Assert.Contains("1189 by 841 mm", size.InWords());
        }
    }
}
