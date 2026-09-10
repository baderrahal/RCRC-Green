namespace RcrcGreen.Core
{
    /// <summary>
    /// Text on its way onto a button or a tick box, made safe to read.
    ///
    /// WPF treats an underscore in a caption as an access key marker: it swallows the first
    /// one and underlines the letter after it. The KPI pane showed PRXComponent, PRXPlot_ID,
    /// PRXPlot_UID, PRXPlot_UID2 and PRXPlot_NH, none of which is a name any model holds, and
    /// both tools turn on exact parameter names, so a pane printing a name that is not in the
    /// model is worse here than almost anywhere.
    ///
    /// Doubling the underscore is the escape WPF itself defines. Nothing else is changed, and
    /// the string a comparison uses is never this one: only what goes on screen.
    ///
    /// It lives in Shared because both panels read it and neither owns its meaning. It was
    /// KPI's, and the Drawing Sheet called it across the fence for one round, which left task
    /// 1 depending on task 2 at compile time between two sessions that cannot see each other.
    /// </summary>
    public static class PaneLabel
    {
        public const char AccessKeyMarker = '_';

        public static string Escaped(string text)
        {
            if (string.IsNullOrEmpty(text)) return text ?? string.Empty;

            return text.Replace(
                AccessKeyMarker.ToString(),
                new string(AccessKeyMarker, 2));
        }
    }
}
