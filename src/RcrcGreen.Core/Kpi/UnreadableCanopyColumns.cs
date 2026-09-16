using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One template's tree list sheet whose total canopy column could not be read.
    /// </summary>
    public sealed class UnreadCanopyColumn
    {
        public UnreadCanopyColumn(string templateName, string sheetName, string why)
        {
            TemplateName = (templateName ?? string.Empty).Trim();
            SheetName = (sheetName ?? string.Empty).Trim();
            Why = (why ?? string.Empty).Trim();
        }

        public string TemplateName { get; }

        public string SheetName { get; }

        public string Why { get; }

        public string InWords
        {
            get { return TemplateName + ", " + SheetName + ": " + Why; }
        }
    }

    /// <summary>
    /// **A GUARD THAT SWITCHES ITSELF OFF READS EXACTLY LIKE A GUARD THAT PASSED.**
    ///
    /// The total canopy check asks <see cref="TotalCanopyColumns.In"/> for the column each tree
    /// list sheet's canopy total adds, read off a chain through the template file. Where that
    /// chain could not be followed the check used to return an empty reason, so the green cover
    /// and the canopy percentage were written with no total canopy check at all, every empty row
    /// was still offered to a new species, and the chain's own reason was printed nowhere.
    ///
    /// **TODAY'S SEVEN TEMPLATES READ FINE AND THE TEAM IS EDITING THEM**, which is exactly when
    /// a check nobody can see switching off costs something. Each unread column now blanks both
    /// computed numbers on any plot holding a count on that sheet, narrows that sheet's empty
    /// rows to none, and is named here once for the press rather than once per plot.
    ///
    /// **A TEMPLATE THAT NAMES NO Total Green cover CELL IS NOT IN HERE.** It holds no canopy
    /// total, so a row of it reaches none by construction and there is nothing to check, which is
    /// Bader's decision of 16 September and the one case that is unchanged.
    /// </summary>
    public static class UnreadableCanopyColumns
    {
        public const string Heading = "THE TEMPLATES WHOSE CANOPY TOTAL COLUMN COULD NOT BE READ";

        private const string Apart = "|";

        /// <summary>
        /// Every ticked template's sheet whose column was refused for a real reason, in the order
        /// the runs were made, with a sheet named once however many plots that template wrote.
        /// </summary>
        public static IReadOnlyList<UnreadCanopyColumn> In(KpiCreateRunSet set)
        {
            if (set == null) throw new ArgumentNullException("set");

            var found = new List<UnreadCanopyColumn>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (KpiCreateRun run in set.Runs.Where(one => one != null))
            {
                string template = run.Template == null ? string.Empty : run.Template.Name;

                foreach (SpeciesList list in new[] { run.ExistingList, run.ProposedList })
                {
                    if (list == null || list.TotalCanopyUnreadable.Length == 0) continue;

                    string sheet = list.TotalCanopy == null
                        ? string.Empty
                        : list.TotalCanopy.SheetName;

                    // A press is one run per plot, so a template of 78 plots would name its own
                    // sheet 78 times. The sheet is what could not be read and it is said once.
                    if (!seen.Add(template + Apart + sheet)) continue;

                    found.Add(new UnreadCanopyColumn(template, sheet, list.TotalCanopyUnreadable));
                }
            }

            return found;
        }

        /// <summary>
        /// The one line the glance carries. **A press where every template read says so**,
        /// because a line that disappears when there is nothing to report reads the same as one
        /// nobody wrote, which is the shape this whole item is about.
        /// </summary>
        public static string InWords(IEnumerable<UnreadCanopyColumn> columns)
        {
            List<UnreadCanopyColumn> held = (columns ?? Enumerable.Empty<UnreadCanopyColumn>()).ToList();

            if (held.Count == 0)
            {
                return Heading + ": every ticked template's canopy total named the column it adds.";
            }

            var templates = held
                .Select(one => one.TemplateName)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            return Heading + ": " + held.Count
                + (held.Count == 1 ? " tree list sheet on " : " tree list sheets on ")
                + templates.Count + (templates.Count == 1 ? " template" : " templates")
                + " could not be read, so no plot holding a count on "
                + (held.Count == 1 ? "it" : "them")
                + " got a Total areas to be greened or a canopy percentage. "
                + string.Join(", ", templates.ToArray()) + ".";
        }
    }
}
