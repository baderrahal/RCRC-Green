namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// Whether Apply may run: a scan exists and what is on the pane still says what the scan
    /// read. It compares rather than remembering, the same rule PresetFilling follows, so an
    /// edit undone by hand makes Apply usable again without a flag anybody has to reset. A
    /// null against a null string counts as the same answer, because an empty hex box and a
    /// box never filled mean one thing on screen.
    /// </summary>
    public static class ApplyGate
    {
        public static bool CanApply(Inputs scanned, Inputs current)
        {
            return scanned != null && current != null && SameInputs(scanned, current);
        }

        public static bool SameInputs(Inputs scanned, Inputs current)
        {
            if (scanned == null || current == null) return false;
            if (!SameText(scanned.ViewKeywords, current.ViewKeywords)) return false;

            FilterConfig[] before = scanned.Filters ?? new FilterConfig[0];
            FilterConfig[] after = current.Filters ?? new FilterConfig[0];
            if (before.Length != after.Length) return false;

            for (int at = 0; at < before.Length; at++)
            {
                if (!SameRow(before[at], after[at])) return false;
            }

            return true;
        }

        private static bool SameRow(FilterConfig before, FilterConfig after)
        {
            if (before == null || after == null) return before == after;

            return SameText(before.Prefix, after.Prefix)
                && before.Enabled == after.Enabled
                && before.Visible == after.Visible
                && before.OverrideLineColor == after.OverrideLineColor
                && SameText(before.LineColor, after.LineColor)
                && before.LineWeight == after.LineWeight
                && before.OverrideForegroundPattern == after.OverrideForegroundPattern
                && SameText(before.ForegroundPatternType, after.ForegroundPatternType)
                && SameText(before.ForegroundPatternColor, after.ForegroundPatternColor)
                && before.OverrideBackgroundPattern == after.OverrideBackgroundPattern
                && SameText(before.BackgroundPatternType, after.BackgroundPatternType)
                && SameText(before.BackgroundPatternColor, after.BackgroundPatternColor)
                && before.Halftone == after.Halftone;
        }

        private static bool SameText(string before, string after)
        {
            return string.Equals(before ?? string.Empty, after ?? string.Empty, System.StringComparison.Ordinal);
        }
    }
}
