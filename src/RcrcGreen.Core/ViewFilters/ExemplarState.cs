namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// What one prefix has to copy from when a filter is missing: the first filter in the
    /// document whose name starts with the prefix, read once per scan. CanCreate mirrors the
    /// second edit's guards exactly, an exemplar exists, its own tail reads as a plot id and
    /// its rules could be copied, so a Will create cell is a create the run will not refuse.
    /// </summary>
    public sealed class ExemplarState
    {
        private ExemplarState(string prefix, bool hasExemplar, string exemplarName, bool rulesClonable)
        {
            Prefix = prefix ?? string.Empty;
            HasExemplar = hasExemplar;
            ExemplarName = exemplarName ?? string.Empty;
            ExemplarPlotCode = ViewFilterNames.ExemplarPlotCode(ExemplarName, Prefix);
            RulesClonable = rulesClonable;
        }

        public string Prefix { get; }

        public bool HasExemplar { get; }

        public string ExemplarName { get; }

        public string ExemplarPlotCode { get; }

        public bool RulesClonable { get; }

        public bool CanCreate
        {
            get
            {
                return HasExemplar
                    && ViewFilterPlotCode.StartsWithAPlotId(ExemplarPlotCode)
                    && RulesClonable;
            }
        }

        public static ExemplarState None(string prefix)
        {
            return new ExemplarState(prefix, false, string.Empty, false);
        }

        public static ExemplarState Of(string prefix, string exemplarName, bool rulesClonable)
        {
            return new ExemplarState(prefix, true, exemplarName, rulesClonable);
        }
    }
}
