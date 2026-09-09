using System;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One value of one parameter, raw and as printed, with a label saying which element it
    /// came off and what the parameter measures. Raw is the number Revit holds, printed is
    /// what a Properties panel shows, and the raw number is square feet only where the
    /// parameter measures an area. A number typed by hand measures nothing and is printed
    /// with no unit, and the file has to say which it is.
    /// </summary>
    public sealed class MeasuredValue
    {
        public MeasuredValue(string label, string spec, string raw, string printed, string plotId = null)
        {
            Label = label ?? string.Empty;
            Spec = spec ?? string.Empty;
            Raw = raw ?? string.Empty;
            Printed = printed ?? string.Empty;
            PlotId = plotId ?? string.Empty;
        }

        public string Label { get; }

        /// <summary>
        /// Revit's word for what the parameter measures, such as Area, or empty for a plain
        /// number or text.
        /// </summary>
        public string Spec { get; }

        public string Raw { get; }

        public string Printed { get; }

        /// <summary>
        /// The plot the element carries, empty when it carries none. One plot's filled regions
        /// read together is what settles which region type is the intervention area, and a row
        /// without it cannot be grouped by plot at all.
        /// </summary>
        public string PlotId { get; }
    }
}
