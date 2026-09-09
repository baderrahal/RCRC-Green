namespace RcrcGreen.Core
{
    /// <summary>
    /// Feet into the units a person thinks in.
    ///
    /// Revit holds every length in decimal feet, so every number that comes out of a model
    /// arrives here in feet and every report that prints one has to turn it round. One place
    /// does that. The far clip offset of a real section was printed once as its own number of
    /// feet with the word metres after it, which is the same class of fault as a report that
    /// says created and not created.
    /// </summary>
    public static class Lengths
    {
        /// <summary>
        /// One foot is 0.3048 metres by definition, so both of these are exact rather than
        /// rounded.
        /// </summary>
        public const double MetresPerFoot = 0.3048;

        public const double MillimetresPerFoot = 304.8;

        public static double InMetres(double feet)
        {
            return feet * MetresPerFoot;
        }

        public static double InMillimetres(double feet)
        {
            return feet * MillimetresPerFoot;
        }

        public static double FeetFromMetres(double metres)
        {
            return metres / MetresPerFoot;
        }
    }
}
