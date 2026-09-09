using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One schedule in the model. Every schedule gets its name, category, fields and filters.
    /// The ones the workbook draws from also get their rows as printed, read once per view
    /// type from the first plot that has it, and the report says which.
    /// </summary>
    public sealed class ScannedSchedule
    {
        public ScannedSchedule(
            string name,
            string categoryName,
            bool onASheet,
            IEnumerable<ScheduleFieldRead> fields,
            IEnumerable<ScheduleFilterRead> filters,
            string phaseName,
            string phaseFilterName,
            bool rowsWereRead,
            int bodyRowCount,
            IEnumerable<IReadOnlyList<string>> rows)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (bodyRowCount < 0) throw new ArgumentOutOfRangeException("bodyRowCount");

            Name = name;
            CategoryName = categoryName ?? string.Empty;
            OnASheet = onASheet;
            Fields = Held(fields);
            Filters = Held(filters);
            PhaseName = phaseName ?? string.Empty;
            PhaseFilterName = phaseFilterName ?? string.Empty;
            RowsWereRead = rowsWereRead;
            BodyRowCount = bodyRowCount;
            Rows = Held(rows);
        }

        public string Name { get; }

        public string CategoryName { get; }

        public bool OnASheet { get; }

        public IReadOnlyList<ScheduleFieldRead> Fields { get; }

        public IReadOnlyList<ScheduleFilterRead> Filters { get; }

        public string PhaseName { get; }

        public string PhaseFilterName { get; }

        /// <summary>
        /// False for a schedule whose rows were not read, which is most of them. Rows are read
        /// for one schedule per view type, so a thousand plot copies are not all regenerated.
        /// </summary>
        public bool RowsWereRead { get; }

        /// <summary>
        /// How many rows the body holds in the model, headings and totals included. The report
        /// prints a capped number and says this.
        /// </summary>
        public int BodyRowCount { get; }

        /// <summary>
        /// Each row as printed, cell by cell, exactly the text a sheet shows. The first row is
        /// usually the column headings and the last usually the total.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<string>> Rows { get; }

        /// <summary>
        /// The words from the workbook list the name holds, empty for a schedule the workbook
        /// does not draw from.
        /// </summary>
        public string MarkedFor
        {
            get { return KpiNames.WordsIn(Name, KpiNames.ScheduleWords); }
        }

        public bool IsMarked
        {
            get { return MarkedFor.Length > 0; }
        }

        public bool IsSoftscape
        {
            get { return KpiNames.HoldsAny(Name, KpiNames.SoftscapeWords); }
        }

        /// <summary>
        /// The name with the plot taken off, or the whole name when it does not follow the
        /// naming pattern, so DM-11 and DM-41 copies of one schedule count as one.
        /// </summary>
        public string NameWithoutThePlot
        {
            get
            {
                ParsedViewName parsed;
                return ViewNameParser.TryParse(Name, out parsed) ? parsed.Type.ToString() : Name;
            }
        }

        public string FilteredOn
        {
            get
            {
                return string.Join("; ", Filters.Select(filter => filter.ToString()).ToArray());
            }
        }

        private static IReadOnlyList<T> Held<T>(IEnumerable<T> items) where T : class
        {
            if (items == null) return new List<T>();
            return items.Where(item => item != null).ToList();
        }
    }
}
