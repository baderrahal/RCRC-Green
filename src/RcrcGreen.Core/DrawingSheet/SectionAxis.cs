namespace RcrcGreen.Core
{
    /// <summary>
    /// Which way the section line runs across the plot.
    /// </summary>
    public enum SectionAxis
    {
        /// <summary>
        /// The line spans the shorter of the two horizontal extents, so the cut goes the short
        /// way across the plot.
        /// </summary>
        ShortSide,

        /// <summary>
        /// The line spans the longer of the two horizontal extents.
        /// </summary>
        LongSide
    }
}
