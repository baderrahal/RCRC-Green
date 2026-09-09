using System;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One column of a schedule: what it is headed on the sheet, which parameter feeds it, and
    /// what kind of field it is.
    ///
    /// The heading and the parameter name are both kept because the workbook note names a
    /// heading such as ENTER EACH PROPOSED TREE QUANTITY, and the tool will have to read the
    /// parameter behind it.
    /// </summary>
    public sealed class ScheduleFieldRead
    {
        public ScheduleFieldRead(
            string heading,
            string parameterName,
            string fieldType,
            bool isHidden,
            string spec,
            string unitLabel)
        {
            Heading = heading ?? string.Empty;
            ParameterName = parameterName ?? string.Empty;
            FieldType = fieldType ?? string.Empty;
            IsHidden = isHidden;
            Spec = spec ?? string.Empty;
            UnitLabel = unitLabel ?? string.Empty;
        }

        public string Heading { get; }

        public string ParameterName { get; }

        /// <summary>
        /// Revit's own word: Instance, ElementType, Count, Formula and so on. A Count field is
        /// the quantity on a schedule that lists one row per tree, and that is worth seeing.
        /// </summary>
        public string FieldType { get; }

        public bool IsHidden { get; }

        /// <summary>
        /// What the field measures, such as Area or Length, or empty for text. Read off the
        /// field so an area column can be told from a text column without guessing from its
        /// heading.
        /// </summary>
        public string Spec { get; }

        /// <summary>
        /// The unit the column prints in when the field overrides the project setting, empty
        /// when it follows the project.
        /// </summary>
        public string UnitLabel { get; }

        public bool IsCount
        {
            get { return string.Equals(FieldType, "Count", StringComparison.OrdinalIgnoreCase); }
        }
    }
}
