namespace RcrcGreen.Revit
{
    /// <summary>
    /// The values the interface starts from when it asks Core to place a cross section.
    /// </summary>
    public static class SectionDefaults
    {
        /// <summary>
        /// What the team asked for. It is a starting value and it will change once sections
        /// have been placed in a real model.
        /// </summary>
        public const double SectionDepthMetres = 10.0;

        /// <summary>
        /// One foot is 0.3048 metres by definition, so this is exact rather than rounded.
        /// </summary>
        private const double MetresPerFoot = 0.3048;

        /// <summary>
        /// Revit holds every length in feet, so a scope box read from the model gives feet and
        /// the numbers handed to Core are feet. Passing the 10 above straight through would
        /// place a section ten feet deep rather than ten metres, and nothing downstream would
        /// say so. The unit is in the name of this constant for the same reason.
        /// </summary>
        public const double SectionDepthFeet = SectionDepthMetres / MetresPerFoot;
    }
}
