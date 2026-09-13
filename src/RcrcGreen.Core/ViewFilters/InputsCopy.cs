namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// A deep copy of one press's inputs. The ported run trims each row's prefix on the row
    /// object itself, exactly as the host did, so the handler is handed a copy and the pane
    /// keeps the original as its record of what was pressed. Without the copy, a prefix
    /// typed with a stray space was trimmed inside the scan, the boxes then never equalled
    /// the record again, and Apply sat greyed on Scan again however many times the user
    /// rescanned.
    /// </summary>
    public static class InputsCopy
    {
        public static Inputs Deep(Inputs inputs)
        {
            if (inputs == null) return null;

            FilterConfig[] rows = inputs.Filters == null
                ? null
                : new FilterConfig[inputs.Filters.Length];

            for (int at = 0; rows != null && at < rows.Length; at++)
            {
                FilterConfig row = inputs.Filters[at];
                rows[at] = row == null ? null : new FilterConfig
                {
                    Prefix = row.Prefix,
                    Enabled = row.Enabled,
                    Visible = row.Visible,
                    OverrideLineColor = row.OverrideLineColor,
                    LineColor = row.LineColor,
                    LineWeight = row.LineWeight,
                    OverrideForegroundPattern = row.OverrideForegroundPattern,
                    ForegroundPatternType = row.ForegroundPatternType,
                    ForegroundPatternColor = row.ForegroundPatternColor,
                    OverrideBackgroundPattern = row.OverrideBackgroundPattern,
                    BackgroundPatternType = row.BackgroundPatternType,
                    BackgroundPatternColor = row.BackgroundPatternColor,
                    Halftone = row.Halftone
                };
            }

            return new Inputs
            {
                ViewKeywords = inputs.ViewKeywords,
                Filters = rows
            };
        }
    }
}
