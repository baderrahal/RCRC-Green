using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Which plot one schedule is filtered on, or why nothing could read it.
    ///
    /// **AUDIT 4 FINDING 65. AN EMPTY STRING MEANT THE SCHEDULE BELONGS TO NO PLOT.**
    /// `KpiPlotReader.PlotFilteredOn` caught two exception types and returned `string.Empty` from
    /// both, recording nothing anywhere, and an empty value is what a schedule carrying no
    /// `PRX_Ref Plot ID` filter also returns. Three things followed and none of them was visible.
    /// The schedule was skipped, so the plot read as holding no softscape and no shrubs and lawn
    /// schedule. The guard that refuses a plot holding two of a kind could not fire either,
    /// because the schedule was never counted. And the reconciliation then said the schedule
    /// listed no species, which is a sentence about the MODEL, while the workbook went out with
    /// that plot's trees missing.
    ///
    /// **A READ THAT THREW AND A READ THAT CAME BACK EMPTY ARE TWO DIFFERENT FACTS.** This is the
    /// record of which, and it is the same shape `PlotReading.NotRead` already uses for a plot
    /// whose whole read threw.
    ///
    /// **THE CALL THAT CAN THROW IS REVIT'S AND CANNOT BE RUN HERE.** What this file holds is the
    /// rule and the words, which have tests. The catch itself is in `KpiPlotReader`, and nothing
    /// in this repository can make Revit's own `ScheduleDefinition` throw, so that the catch
    /// really reaches this is UNKNOWN until somebody runs it. What would settle it is one press
    /// on a model holding a schedule whose definition Revit refuses.
    /// </summary>
    public sealed class SchedulePlotRead
    {
        private SchedulePlotRead(string scheduleName, string plotId, string why)
        {
            ScheduleName = (scheduleName ?? string.Empty).Trim();
            PlotId = (plotId ?? string.Empty).Trim();
            Why = (why ?? string.Empty).Trim();
        }

        /// <summary>The plot this schedule filters on, which may legitimately be none.</summary>
        public static SchedulePlotRead Of(string scheduleName, string plotId)
        {
            return new SchedulePlotRead(scheduleName, plotId, string.Empty);
        }

        /// <summary>
        /// The read threw. **A reason is required**, because a refusal with no reason is the
        /// empty string this whole class exists to stop being mistaken for an answer.
        /// </summary>
        public static SchedulePlotRead Refused(string scheduleName, string why)
        {
            if (string.IsNullOrWhiteSpace(why))
            {
                throw new ArgumentException("A refused schedule read needs a reason.", "why");
            }

            return new SchedulePlotRead(scheduleName, string.Empty, why);
        }

        public string ScheduleName { get; }

        /// <summary>Empty on a refusal and on a schedule that really filters on no plot.</summary>
        public string PlotId { get; }

        public string Why { get; }

        public bool Read
        {
            get { return Why.Length == 0; }
        }

        /// <summary>
        /// Whether this schedule belongs to the named plot. **A refusal belongs to no plot and
        /// says so**, so nothing downstream reads a throw as a schedule that filters on nothing.
        /// </summary>
        public bool Is(string plotId)
        {
            return Read
                && PlotId.Length > 0
                && string.Equals(PlotId, (plotId ?? string.Empty).Trim(), StringComparison.Ordinal);
        }

        /// <summary>The refusal as it travels on a plot reading, naming the schedule.</summary>
        public string InWords
        {
            get
            {
                return Read
                    ? string.Empty
                    : ScheduleName + ": " + Why;
            }
        }
    }

    /// <summary>
    /// What a press does with the schedules whose plot could not be read.
    ///
    /// **A SCHEDULE THAT COULD NOT BE READ IS NOT A SCHEDULE THAT BELONGS TO NOBODY.** It is
    /// carried as a refusal naming the schedule, so a plot's files are not written off a half
    /// read and READY reads NO with the schedule's name, and the schedule half of the plot list
    /// names it rather than dropping it.
    /// </summary>
    public static class SchedulePlotReads
    {
        public const string Heading = "THE SCHEDULES WHOSE PLOT COULD NOT BE READ";

        /// <summary>
        /// The sentence a throw while reading a schedule's plot filter carries. **It names the
        /// exception's own type and message**, the same as `GuardedRead`'s, because a reason that
        /// says only that something went wrong sends somebody to look at the wrong thing.
        /// </summary>
        public static string ThrewReading(string typeName, string message)
        {
            // **THE EXCEPTION'S OWN MESSAGE USUALLY ENDS IN A FULL STOP AND SOMETIMES DOES NOT.**
            // Adding one either way gives `is not a valid view..`, which reads as a typo in a
            // client facing report, so the stop is added only where the message has none.
            string said = (message ?? string.Empty).Trim();
            string stop = said.EndsWith(".", StringComparison.Ordinal) ? string.Empty : ".";

            return "reading which plot this schedule filters on threw "
                + (string.IsNullOrWhiteSpace(typeName) ? "an exception" : typeName.Trim())
                + ", " + said + stop
                + " Nothing says which plot it belongs to, so it is neither counted for a plot "
                + "nor read as belonging to none";
        }

        /// <summary>Every refusal among a set of reads, in the order they came.</summary>
        public static IReadOnlyList<SchedulePlotRead> Refused(IEnumerable<SchedulePlotRead> reads)
        {
            return (reads ?? Enumerable.Empty<SchedulePlotRead>())
                .Where(one => one != null && !one.Read)
                .ToList();
        }

        /// <summary>
        /// Every plot a set of reads really names, with the refusals left out. **This is what the
        /// schedule half of the plot list is built from**, and a refusal reaching it as an empty
        /// string is the fault finding 65 names.
        /// </summary>
        public static IReadOnlyList<string> PlotsNamed(IEnumerable<SchedulePlotRead> reads)
        {
            return (reads ?? Enumerable.Empty<SchedulePlotRead>())
                .Where(one => one != null && one.Read && one.PlotId.Length > 0)
                .Select(one => one.PlotId)
                .ToList();
        }

        /// <summary>
        /// The one line the plot list's schedule half carries about what it could not read. **A
        /// press that read every one says so**, because a line that disappears when there is
        /// nothing to report reads the same as one nobody wrote.
        /// </summary>
        public static string InWords(IEnumerable<SchedulePlotRead> reads)
        {
            List<SchedulePlotRead> refused = Refused(reads).ToList();

            if (refused.Count == 0)
            {
                return Heading + ": every schedule this read walked named the plot it filters on, "
                    + "or named none.";
            }

            return Heading + ": " + refused.Count
                + (refused.Count == 1 ? " schedule" : " schedules")
                + " threw while being asked which plot they filter on, so the plots they belong "
                + "to are UNKNOWN and no workbook is written off them. "
                + string.Join(", ", refused.Select(one => one.ScheduleName).ToArray()) + ".";
        }
    }
}
