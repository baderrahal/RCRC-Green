using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Sections 5 to 8. Every schedule, the phases in the model, the elements behind the
    /// schedules the workbook draws from, and the areas read off them.
    /// </summary>
    public sealed class ScheduleFacts
    {
        public ScheduleFacts(
            IEnumerable<ScannedSchedule> schedules,
            int templateCount,
            IEnumerable<string> phases,
            IEnumerable<ScheduleElements> elements,
            IEnumerable<MeasuredArea> areas)
        {
            if (templateCount < 0) throw new ArgumentOutOfRangeException("templateCount");

            Schedules = Held(schedules);
            TemplateCount = templateCount;
            Phases = Held(phases);
            Elements = Held(elements);
            Areas = Held(areas);
        }

        public IReadOnlyList<ScannedSchedule> Schedules { get; }

        /// <summary>
        /// Schedule templates are counted and not listed. They hold no rows and no plot.
        /// </summary>
        public int TemplateCount { get; }

        /// <summary>
        /// Every phase in the model, in the order the project holds them.
        /// </summary>
        public IReadOnlyList<string> Phases { get; }

        public IReadOnlyList<ScheduleElements> Elements { get; }

        public IReadOnlyList<MeasuredArea> Areas { get; }

        public IEnumerable<ScannedSchedule> Marked
        {
            get { return Schedules.Where(schedule => schedule.IsMarked); }
        }

        public IEnumerable<ScannedSchedule> Softscape
        {
            get { return Schedules.Where(schedule => schedule.IsSoftscape); }
        }

        public static ScheduleFacts Nothing()
        {
            return new ScheduleFacts(null, 0, null, null, null);
        }

        private static IReadOnlyList<T> Held<T>(IEnumerable<T> items) where T : class
        {
            if (items == null) return new List<T>();
            return items.Where(item => item != null).ToList();
        }
    }
}
