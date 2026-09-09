using System;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One value of one parameter, raw and as printed, with a label saying which element it
    /// came off. Raw is the number Revit holds, printed is what a Properties panel shows.
    /// </summary>
    public sealed class MeasuredValue
    {
        public MeasuredValue(string label, string raw, string printed)
        {
            Label = label ?? string.Empty;
            Raw = raw ?? string.Empty;
            Printed = printed ?? string.Empty;
        }

        public string Label { get; }

        public string Raw { get; }

        public string Printed { get; }
    }
}
