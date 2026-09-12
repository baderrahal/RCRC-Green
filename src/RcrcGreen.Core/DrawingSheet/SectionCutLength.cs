using System.Globalization;

namespace RcrcGreen.Core
{
    /// <summary>
    /// How long a cross section's cut is. One number, the same on every section the tool
    /// makes, and the tool's own setting rather than anything read off a view.
    ///
    /// The cut used to run the whole width of the plot's scope box. On NS-32 that is
    /// 36.4747 metres, and the viewport it produced on a real sheet was still far wider
    /// than the drawing area at the view's own scale. The team halved it.
    ///
    /// The number lives in Core rather than in the Revit project because the run report
    /// has to be able to print it, the same reason <see cref="SectionDepth"/> does. The
    /// conversion into feet stays at the Revit boundary in SectionDefaults.
    /// </summary>
    public static class SectionCutLength
    {
        public const double Metres = 18.2374;

        /// <summary>
        /// What the box was measured at before the halving, kept so the report can say
        /// where the number came from rather than presenting it as a rule from nowhere.
        /// </summary>
        public const double WholeBoxOnNsThirtyTwoMetres = 36.4747;

        public static string InWords()
        {
            return "The cut is "
                + Metres.ToString("0.####", CultureInfo.InvariantCulture)
                + " metres long, centred on the plot's scope box and cut the short way. "
                + "That is the tool's own setting and not a number read off any view or "
                + "off the box. It used to run the whole width of the box, which is "
                + WholeBoxOnNsThirtyTwoMetres.ToString("0.####", CultureInfo.InvariantCulture)
                + " metres on NS-32, and the viewport that produced was wider than the "
                + "drawing area it had to sit in.";
        }
    }
}
