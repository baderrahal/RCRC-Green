using System;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One filter row on a schedule, as read. Unlike the Drawing Sheet's rule this keeps the
    /// operator, because a filter reading Contains is not the same as one reading Equals and
    /// the KPI tool has to reproduce what the schedule counts.
    /// </summary>
    public sealed class ScheduleFilterRead
    {
        public ScheduleFilterRead(string fieldName, string rule, string value)
        {
            FieldName = fieldName ?? string.Empty;
            Rule = rule ?? string.Empty;
            Value = value ?? string.Empty;
        }

        public string FieldName { get; }

        public string Rule { get; }

        public string Value { get; }

        public override string ToString()
        {
            return FieldName + " " + Rule + " " + (Value.Length == 0 ? "(empty)" : Value);
        }
    }
}
