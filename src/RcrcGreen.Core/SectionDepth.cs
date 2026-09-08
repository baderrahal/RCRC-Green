namespace RcrcGreen.Core
{
    /// <summary>
    /// How far a cross section looks, as the team stated it.
    ///
    /// The number lives here rather than in the Revit project because the run report is Core
    /// and has to be able to say it. The conversion to feet stays at the Revit boundary in
    /// SectionDefaults, because feet is Revit's unit and nothing in Core should assume it.
    /// </summary>
    public static class SectionDepth
    {
        /// <summary>
        /// A starting value. The team will change it once sections have been placed in a real
        /// model and somebody has looked at one.
        /// </summary>
        public const double Metres = 10.0;
    }
}
