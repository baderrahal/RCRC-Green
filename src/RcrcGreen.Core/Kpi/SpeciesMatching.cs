using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One merged species held against the workbook's own list: where it landed, or why it did
    /// not land anywhere.
    ///
    /// **A species Revit holds that the list does not is now WRITTEN IN**, into the first empty
    /// row below the list on the sheet its group points at, with its name in column D and its
    /// count in column B and nothing in any other column. It used to be named and written
    /// nowhere, which left the workbook reading 31 trees where the model held 39.
    ///
    /// Family, genus, native and every code column are the client's data and stay empty. The
    /// tool does not know them and will not guess them, so the KPIs that need them still cannot
    /// see a species written this way. That is in the log as an open question for the team.
    ///
    /// A species the list holds that Revit does not is simply left empty, which is correct and
    /// needs no line.
    /// </summary>
    public sealed class SpeciesMatch
    {
        public SpeciesMatch(
            MergedSpecies species,
            string sheetName,
            int row,
            string workbookName,
            string why,
            bool added = false)
        {
            if (species == null) throw new ArgumentNullException("species");
            if (row < 0) throw new ArgumentOutOfRangeException("row");

            Species = species;
            SheetName = sheetName ?? string.Empty;
            Row = row;
            WorkbookName = workbookName ?? string.Empty;
            Why = why ?? string.Empty;
            Added = added;
        }

        /// <summary>
        /// True when the workbook's list did not hold this name and it was written into an
        /// empty row. The name goes in as Revit spells it, because there is nothing else to
        /// spell it from.
        /// </summary>
        public bool Added { get; }

        /// <summary>
        /// A row was found for it, whether the list already held the name or the tool wrote it
        /// in. This is what says a quantity reaches the total.
        /// </summary>
        public bool Placed
        {
            get { return Row > 0 && SheetName.Length > 0; }
        }

        public MergedSpecies Species { get; }

        public string SheetName { get; }

        /// <summary>
        /// The workbook row the quantity goes on, or nothing when the species did not match.
        /// </summary>
        public int Row { get; }

        /// <summary>
        /// The name as the workbook spells it, which is not how Revit spells it. Revit prints
        /// ALBIZIA LEBBECK and the workbook holds Albizia lebbeck.
        /// </summary>
        public string WorkbookName { get; }

        public string Why { get; }

        /// <summary>
        /// The workbook's own list held this name. An added species is placed and not matched.
        /// </summary>
        public bool Matched
        {
            get { return Placed && !Added; }
        }
    }

    /// <summary>
    /// Matches what Revit printed against what the workbook holds.
    ///
    /// Plainly: the botanical name compared without case and with surrounding whitespace off,
    /// and nothing else. Names are not stripped, split or normalised past that. Three measured
    /// cases are the reason: the model prints a species called UNKNOWN while the workbook holds
    /// four rows all named Unknown Tree, ACACIA / VACHELLIA FARNESIANA carries a slash, and
    /// BOUGAINVILLEA GLABRA 'PINK PIXIE' carries an apostrophe. Normalising past any of them
    /// would put a quantity on a row nobody chose.
    /// </summary>
    public static class SpeciesMatching
    {
        public const string NotInTheList = "the workbook's list does not hold this name";

        public const string NoSheetForTheGroup =
            "its group names neither tree list sheet, so nothing says which list it belongs to";

        public const string MoreThanOneRow =
            "the workbook holds this name on more than one row, so nothing can say which";

        /// <summary>
        /// Said when the sheet's list is full. Written rather than assumed: what fits goes in
        /// and the rest are named, because a quantity dropped in silence is the fault this whole
        /// section exists to prevent.
        /// </summary>
        public const string NoEmptyRowLeft =
            "the sheet ran out of room: every row its total sums already holds a name";

        public const string WrittenIn = "the list did not hold it, so it was written into an empty row";

        public static IReadOnlyList<SpeciesMatch> Against(
            IEnumerable<MergedSpecies> merged,
            KpiTemplate template,
            SpeciesList existing,
            SpeciesList proposed)
        {
            if (template == null) throw new ArgumentNullException("template");

            var found = new List<SpeciesMatch>();

            // One queue of empty rows per sheet, taken in order, so two species the list does
            // not hold never land on the same row.
            var free = new Dictionary<string, Queue<int>>(StringComparer.Ordinal);

            foreach (MergedSpecies species in (merged ?? Enumerable.Empty<MergedSpecies>())
                .Where(one => one != null))
            {
                TreeRows rows = SheetFor(species.GroupName, template);
                if (rows == null)
                {
                    found.Add(new SpeciesMatch(species, string.Empty, 0, string.Empty, NoSheetForTheGroup));
                    continue;
                }

                SpeciesList list = string.Equals(rows.SheetName, template.ExistingTrees.SheetName,
                    StringComparison.Ordinal) ? existing : proposed;

                if (list == null || !list.WasRead)
                {
                    found.Add(new SpeciesMatch(species, rows.SheetName, 0, string.Empty,
                        list == null ? NotInTheList : list.Refusal));
                    continue;
                }

                List<SpeciesListRow> holding = list.Rows
                    .Where(one => Same(one.BotanicalName, species.BotanicalName))
                    .ToList();

                if (holding.Count == 0)
                {
                    found.Add(WrittenInto(species, rows.SheetName, list, free));
                    continue;
                }

                if (holding.Count > 1)
                {
                    found.Add(new SpeciesMatch(species, rows.SheetName, 0, holding[0].BotanicalName,
                        MoreThanOneRow));
                    continue;
                }

                found.Add(new SpeciesMatch(
                    species, rows.SheetName, holding[0].Row, holding[0].BotanicalName, string.Empty));
            }

            return found;
        }

        /// <summary>
        /// A species the list does not hold, put into the first empty row that sheet has left.
        /// The rows come off the file, in order, one queue per sheet, so nothing lands twice.
        /// </summary>
        private static SpeciesMatch WrittenInto(
            MergedSpecies species,
            string sheetName,
            SpeciesList list,
            Dictionary<string, Queue<int>> free)
        {
            Queue<int> rows;
            if (!free.TryGetValue(sheetName, out rows))
            {
                rows = new Queue<int>(list.EmptyRows);
                free[sheetName] = rows;
            }

            if (rows.Count == 0)
            {
                return new SpeciesMatch(species, sheetName, 0, string.Empty, NoEmptyRowLeft);
            }

            return new SpeciesMatch(species, sheetName, rows.Dequeue(), string.Empty, WrittenIn, true);
        }

        /// <summary>
        /// The tree list sheet a group belongs to, decided by the sheet's own name holding the
        /// group's name. Tree List - Existing holds Existing. The words Existing and Proposed
        /// are written in neither side of this: the group comes off the document's phases and
        /// the sheet name comes off the map, and a group matching neither sheet is reported.
        /// </summary>
        private static TreeRows SheetFor(string groupName, KpiTemplate template)
        {
            if (string.IsNullOrWhiteSpace(groupName)) return null;

            foreach (TreeRows rows in new[] { template.ExistingTrees, template.ProposedTrees })
            {
                if (KpiNames.Holds(rows.SheetName, groupName.Trim())) return rows;
            }

            return null;
        }

        private static bool Same(string workbook, string revit)
        {
            return string.Equals(
                (workbook ?? string.Empty).Trim(),
                (revit ?? string.Empty).Trim(),
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
