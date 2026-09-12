using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One name Revit prints and the name the client's list holds for the same thing.
    /// </summary>
    public sealed class SpeciesAlias
    {
        public SpeciesAlias(string revitName, string workbookName)
        {
            if (string.IsNullOrEmpty(revitName)) throw new ArgumentNullException("revitName");
            if (string.IsNullOrEmpty(workbookName)) throw new ArgumentNullException("workbookName");

            RevitName = revitName;
            WorkbookName = workbookName;
        }

        /// <summary>
        /// The name as the model prints it, compared without case and with edge spaces off,
        /// which is the one comparison this tool makes anywhere.
        /// </summary>
        public string RevitName { get; }

        /// <summary>
        /// The name the workbook's own list holds.
        /// </summary>
        public string WorkbookName { get; }

        /// <summary>
        /// For the report, so a count that arrived through a decision of the team's never reads
        /// the same as one that matched on its own name.
        /// </summary>
        public string InWords
        {
            get { return RevitName + " means " + WorkbookName; }
        }
    }

    /// <summary>
    /// **A TABLE, not a rule.** Bader's decision, and the whole of the reasoning is why no rule
    /// could do this job.
    ///
    /// Measured on the workbook the 1552 run wrote on the NG03 model, which is not in this
    /// repository: exactly one of the 98 names in the MOSQUES Tree List - Existing opens with
    /// UNKNOWN, which is Unknown Tree at row 101, and the Proposed list holds none. So a rule
    /// reaching Unknown Tree from UNKNOWN by a shared opening, a prefix or a longest match
    /// would work on that one name and then reach into these, where it has to pick one:
    ///
    /// <code>
    /// Conocarpus erectus    and  Conocarpus lancifolius
    /// Ficus benjamina       and  Ficus religiosa       and  Ficus pseudosycomorus
    /// Prosopis juliflora    and  Prosopis glandulosa
    /// </code>
    ///
    /// A picked genus is a silent wrong number in a client file, which is worse than a tree
    /// that goes nowhere. **UNKNOWN is not a species.** It is Revit's placeholder for a tree
    /// nobody has identified and Unknown Tree is the client's placeholder for the same thing.
    /// Two placeholders meeting is a fact about this project, and a fact about the project is
    /// data.
    ///
    /// <see cref="SpeciesMatching.ClosestName"/> stays a print and never becomes a match. It is
    /// how the next entry here gets found.
    /// </summary>
    public static class SpeciesAliases
    {
        private static readonly SpeciesAlias[] Held =
        {
            new SpeciesAlias("UNKNOWN", "Unknown Tree")
        };

        /// <summary>
        /// Every alias the tool holds. One today, and a second goes in only when the team says
        /// so, the same rule the Street Design list follows.
        /// </summary>
        public static IReadOnlyList<SpeciesAlias> All
        {
            get { return Held; }
        }

        /// <summary>
        /// The alias for a name Revit printed, or nothing. The comparison is the one this tool
        /// makes everywhere, without case and with edge spaces off, and never a prefix.
        /// </summary>
        public static SpeciesAlias For(string revitName)
        {
            string wanted = (revitName ?? string.Empty).Trim();
            if (wanted.Length == 0) return null;

            return Held.FirstOrDefault(one =>
                string.Equals(one.RevitName, wanted, StringComparison.OrdinalIgnoreCase));
        }
    }
}
