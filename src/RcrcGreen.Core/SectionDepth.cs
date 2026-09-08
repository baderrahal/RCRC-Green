namespace RcrcGreen.Core
{
    /// <summary>
    /// How far a cross section looks when the model does not say.
    ///
    /// The number lives here rather than in the Revit project because the run report is Core
    /// and has to be able to say it. The conversion to feet for a new section stays at the
    /// Revit boundary in SectionDefaults, because feet is Revit's unit. The conversion the
    /// other way is here, because a report that prints a far clip offset read off a model has
    /// to print it in the unit a person thinks in.
    /// </summary>
    public static class SectionDepth
    {
        /// <summary>
        /// What the team named before anybody had looked at a section in the real model. A real
        /// one reads 12.83 metres, so this is the fallback for a view type the model has no
        /// section of, rather than the rule.
        /// </summary>
        public const double Metres = 10.0;

        /// <summary>
        /// One foot is 0.3048 metres by definition, so this is exact rather than rounded.
        /// </summary>
        public const double MetresPerFoot = 0.3048;

        public static double InMetres(double feet)
        {
            return feet * MetresPerFoot;
        }
    }
}
