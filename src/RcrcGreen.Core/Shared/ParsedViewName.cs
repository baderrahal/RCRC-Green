namespace RcrcGreen.Core
{
    /// <summary>
    /// One view or sheet name broken into its three parts, with the text it came from.
    /// </summary>
    public sealed class ParsedViewName
    {
        public ParsedViewName(string plotId, string code, string viewName, string original)
        {
            PlotId = plotId;
            Code = code;
            ViewName = viewName;
            Original = original;
        }

        public string PlotId { get; }

        public string Code { get; }

        public string ViewName { get; }

        public string Original { get; }

        /// <summary>
        /// The code alone does not identify a view. Two names can share code 010 and be
        /// different views, so the type is the code and the view name together.
        /// </summary>
        public ViewType Type
        {
            get { return new ViewType(Code, ViewName); }
        }

        public override string ToString()
        {
            return Original;
        }
    }
}
