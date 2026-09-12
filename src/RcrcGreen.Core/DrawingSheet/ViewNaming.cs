namespace RcrcGreen.Core
{
    /// <summary>
    /// What a view on a plot is called: the plot identifier, a dash, then the view type.
    ///
    /// `DM-11-(010) Location Key Plan`. The bracketed code and the view name are
    /// <see cref="ViewType"/>'s own format and are not repeated here.
    ///
    /// **One builder.** It was written out by hand in four places, the run plan twice, the
    /// schedule definition once and the writer once, and the writer's copy is what every
    /// created view is named by while the plan's copy is what the report prints. Four records
    /// of one format that happened to agree.
    ///
    /// Reading a name apart is `ViewNameParser` in Shared, and this is the way back. The two
    /// belong side by side and this one is here rather than there because a change to Shared
    /// stops every other session, which is a round of its own and the user's call.
    /// </summary>
    public static class ViewNaming
    {
        public static string Of(string plotId, ViewType type)
        {
            string plot = plotId ?? string.Empty;

            return type == null ? plot : plot + "-" + type;
        }
    }
}
