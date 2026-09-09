namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The one place square feet turn into square metres.
    ///
    /// Revit holds every area in square feet internally whatever the project displays. The
    /// Drawing Sheet keeps its length conversions in Lengths for the same reason, and this
    /// sits beside it rather than inside it because that file is shared and not changed here.
    /// </summary>
    public static class AreaUnits
    {
        /// <summary>
        /// One foot is exactly 0.3048 metres, so one square foot is exactly this.
        /// </summary>
        public const double SquareMetresPerSquareFoot = 0.09290304;

        public static double SquareMetresFromSquareFeet(double squareFeet)
        {
            return squareFeet * SquareMetresPerSquareFoot;
        }
    }
}
