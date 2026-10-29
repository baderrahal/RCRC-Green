namespace RcrcGreen.Core.Kpi
{
    /// <summary>What Show on a region option can do in the view that is open.</summary>
    public enum RegionShowAnswer
    {
        Shown,
        NotFound,
        NotAPlan,
        LinkHidden,
        NotDrawn
    }

    /// <summary>
    /// **Show selects one filled region in the 00 link and zooms the open view to it.** Bader's
    /// decision of 29 October, so a person choosing between two areas can see which region is
    /// which. It reads and never writes: a selection and a zoom are no change to the model.
    ///
    /// The Revit side measures four facts and this decides. A region no loaded 00 link holds
    /// any more is said as that. A view that is not a plan, a sheet or a schedule among them,
    /// one hiding the link, or one not drawing the region, cannot show it, and nothing is
    /// selected or zoomed, because a selection the open view does not draw is a change nobody
    /// can see. The three share Bader's one line and each has its own reason behind it.
    /// </summary>
    public static class RegionShowing
    {
        public static RegionShowAnswer Of(bool regionFound, bool viewIsAPlan, bool linkShownInView, bool regionDrawnInView)
        {
            if (!regionFound) return RegionShowAnswer.NotFound;
            if (!viewIsAPlan) return RegionShowAnswer.NotAPlan;
            if (!linkShownInView) return RegionShowAnswer.LinkHidden;
            if (!regionDrawnInView) return RegionShowAnswer.NotDrawn;
            return RegionShowAnswer.Shown;
        }

        /// <summary>The status line for an answer, through <see cref="KpiPaneWords"/>.</summary>
        public static string Words(RegionShowAnswer answer, string typeName)
        {
            switch (answer)
            {
                case RegionShowAnswer.Shown: return KpiPaneWords.RegionShown(typeName);
                case RegionShowAnswer.NotFound: return KpiPaneWords.RegionNotFound;
                default: return KpiPaneWords.OpenAPlanShowingTheLink;
            }
        }

        /// <summary>The status line's tooltip: why the open view cannot show it, else empty.</summary>
        public static string Detail(RegionShowAnswer answer)
        {
            switch (answer)
            {
                case RegionShowAnswer.NotAPlan: return KpiPaneWords.NotAPlanTip;
                case RegionShowAnswer.LinkHidden: return KpiPaneWords.LinkHiddenTip;
                case RegionShowAnswer.NotDrawn: return KpiPaneWords.RegionNotDrawnTip;
                case RegionShowAnswer.NotFound: return KpiPaneWords.RegionNotFoundTip;
                default:
                    return string.Empty;
            }
        }
    }
}
