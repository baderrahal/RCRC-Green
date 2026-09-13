namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// One filter row as the pane edits it and as ViewFilters.json ships it. Ported from the
    /// working argus host with the same name and the same properties, kept exactly so the run
    /// body reads unchanged. The Override booleans are the ticks: a colour can sit in its box
    /// with the tick off and then changes nothing.
    /// </summary>
    public class FilterConfig
    {
        public string Prefix { get; set; }

        public bool Enabled { get; set; }

        public bool Visible { get; set; }

        public bool OverrideLineColor { get; set; }

        public string LineColor { get; set; }

        public int LineWeight { get; set; }

        public bool OverrideForegroundPattern { get; set; }

        public string ForegroundPatternType { get; set; }

        public string ForegroundPatternColor { get; set; }

        public bool OverrideBackgroundPattern { get; set; }

        public string BackgroundPatternType { get; set; }

        public string BackgroundPatternColor { get; set; }

        public bool Halftone { get; set; }
    }
}
