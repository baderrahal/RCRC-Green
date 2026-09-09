using System;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The value one named parameter holds on one sheet, whether it was read off the title
    /// block on that sheet or off the sheet itself. Which of the two is said by the
    /// <see cref="ParameterHome"/> it sits in.
    /// </summary>
    public sealed class SheetValue
    {
        public SheetValue(string parameterName, string sheetNumber, string sheetName, string value)
        {
            if (parameterName == null) throw new ArgumentNullException("parameterName");

            ParameterName = parameterName;
            SheetNumber = sheetNumber ?? string.Empty;
            SheetName = sheetName ?? string.Empty;
            Value = value ?? string.Empty;
        }

        public string ParameterName { get; }

        public string SheetNumber { get; }

        public string SheetName { get; }

        public string Value { get; }
    }
}
