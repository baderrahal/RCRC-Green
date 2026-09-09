using System;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One area read off one element that a marked schedule lists. Raw is the internal
    /// number, which Revit holds in square feet whatever the project shows, printed is what a
    /// Properties panel shows, and the square metres are worked out here from the raw number
    /// so the three can be held against each other.
    /// </summary>
    public sealed class MeasuredArea
    {
        public MeasuredArea(
            string scheduleName,
            string parameterName,
            string elementLabel,
            double rawSquareFeet,
            string printed)
        {
            if (scheduleName == null) throw new ArgumentNullException("scheduleName");
            if (parameterName == null) throw new ArgumentNullException("parameterName");
            if (double.IsNaN(rawSquareFeet) || double.IsInfinity(rawSquareFeet))
            {
                throw new ArgumentOutOfRangeException("rawSquareFeet");
            }

            ScheduleName = scheduleName;
            ParameterName = parameterName;
            ElementLabel = elementLabel ?? string.Empty;
            RawSquareFeet = rawSquareFeet;
            Printed = printed ?? string.Empty;
        }

        public string ScheduleName { get; }

        public string ParameterName { get; }

        public string ElementLabel { get; }

        public double RawSquareFeet { get; }

        public string Printed { get; }

        public double SquareMetres
        {
            get { return AreaUnits.SquareMetresFromSquareFeet(RawSquareFeet); }
        }
    }
}
