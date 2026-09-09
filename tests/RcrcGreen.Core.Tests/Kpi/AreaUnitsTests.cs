using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    public class AreaUnitsTests
    {
        [Fact]
        public void OneSquareFootIsExactlyThatManySquareMetres()
        {
            Assert.Equal(0.09290304, AreaUnits.SquareMetresFromSquareFeet(1.0));
        }

        [Fact]
        public void ZeroStaysZero()
        {
            Assert.Equal(0.0, AreaUnits.SquareMetresFromSquareFeet(0.0));
        }

        [Fact]
        public void TenPointSevenSixSquareFeetIsOneSquareMetre()
        {
            Assert.Equal(1.0, AreaUnits.SquareMetresFromSquareFeet(10.7639), 4);
        }

        [Fact]
        public void AThousandSquareFeetIsWhatSectionEightPrints()
        {
            Assert.Equal(92.90304, AreaUnits.SquareMetresFromSquareFeet(1000.0), 5);
        }
    }
}
