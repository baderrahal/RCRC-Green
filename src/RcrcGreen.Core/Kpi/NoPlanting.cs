using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// **FOUR PLOTS OF THE 16:37 PRESS READ 0 IN EVERY TREE, SHRUB, LAWN, WATER AND GREEN COVER
    /// BOX**, MM-01, MM-06, MM-07 and NS-23, because both of their schedules printed their
    /// heading row and no body. Writing 0 there is Bader's decision of 15 September and it
    /// stands: a schedule that was read and printed nothing really does hold nothing.
    ///
    /// What was missing is the one line that separates those four from a plot whose numbers
    /// happen to be small. A workbook of noughts and a workbook of noughts because the model
    /// holds no planting look identical in a folder, and only one of them is a question for the
    /// team.
    ///
    /// **IT DOES NOT MOVE READY.** The files were written, every box has its number, and nothing
    /// about them is wrong. It is a note, and the plots are named rather than counted, because
    /// four plots to go and look at is a list and a count of four is a number.
    /// </summary>
    public static class NoPlanting
    {
        public const string Heading = "THE PLOTS WITH NO PLANTING AT ALL";

        /// <summary>
        /// Whether this plot's softscape schedule and its shrubs and lawn schedule were BOTH read
        /// and BOTH printed no body row.
        ///
        /// **A PLOT MISSING A SCHEDULE IS NOT THIS.** That plot is already named by its own
        /// refusal and by the PDF's reason, and calling it a plot with no planting would say
        /// something about the model that nothing measured.
        /// </summary>
        public static bool On(PlotReading reading)
        {
            if (reading == null) return false;
            if (!reading.SoftscapeRead || !reading.ShrubsAndLawnRead) return false;

            return PrintedNothing(reading, reading.SoftscapeSchedules)
                && PrintedNothing(reading, reading.ShrubsAndLawnSchedules);
        }

        /// <summary>
        /// **THE BODY ROW COUNT IS THE SUBJECT AND A SCHEDULE NOBODY PRINTED IS NOT ONE.** A read
        /// that did not happen and a read that came back with nothing are two different facts,
        /// so a schedule named on the reading and absent from what was printed answers false.
        /// </summary>
        private static bool PrintedNothing(PlotReading reading, IReadOnlyList<string> names)
        {
            var printed = names
                .Select(name => reading.PrintedSchedules.FirstOrDefault(
                    one => one != null && string.Equals(one.Name, name, StringComparison.Ordinal)))
                .Where(one => one != null)
                .ToList();

            return printed.Count == names.Count && printed.All(one => one.BodyRowCount == 0);
        }

        /// <summary>Every plot of this press whose two schedules both printed nothing.</summary>
        public static IReadOnlyList<string> In(KpiCreateRunSet set)
        {
            if (set == null) throw new ArgumentNullException("set");

            return set.Runs
                .Where(one => one != null)
                .SelectMany(one => one.Readings)
                .Where(On)
                .Select(one => one.PlotId)
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>
        /// The one line the glance carries. **A press where every plot has planting says so**,
        /// because a line that disappears when there is nothing to report reads the same as one
        /// nobody wrote.
        /// </summary>
        public static string InWords(IEnumerable<string> plots)
        {
            List<string> held = (plots ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrWhiteSpace(one))
                .ToList();

            if (held.Count == 0)
            {
                return Heading + ": every plot this press read printed at least one schedule row.";
            }

            return Heading + ": " + held.Count + (held.Count == 1 ? " plot's" : " plots'")
                + " softscape and shrubs and lawn schedules both printed a heading and no rows, "
                + "so every planting box on " + (held.Count == 1 ? "it" : "them")
                + " was written 0. " + string.Join(", ", held.ToArray()) + ".";
        }
    }
}
