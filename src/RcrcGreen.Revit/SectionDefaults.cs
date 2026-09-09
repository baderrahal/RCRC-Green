using RcrcGreen.Core;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// How deep a cross section is cut, at the Revit boundary where feet is the unit.
    ///
    /// The metres live in Core because the run report has to print them. Holding a second copy
    /// of the number here is exactly the shape that has been the bug five times in this repo,
    /// so this reads Core's and converts. Nothing here decides the depth.
    /// </summary>
    public static class SectionDefaults
    {
        /// <summary>
        /// Revit holds every length in feet, so a scope box read from the model gives feet and
        /// the numbers handed to Core are feet. Passing the metres straight through would place
        /// a section that many feet deep, and nothing downstream would say so. The unit is in
        /// the name of this constant for the same reason.
        /// </summary>
        public const double SectionDepthFeet = SectionDepth.Metres / Lengths.MetresPerFoot;
    }
}
