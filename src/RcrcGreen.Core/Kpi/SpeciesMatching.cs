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
    ///
    /// **A species the list holds on a row the sheet's total does not reach is refused**, with
    /// the row and the total both named. The MOSQUES existing list measured on 2026-09-10 names
    /// species on rows 93 to 101 and its total is SUM(B4:B92), so a count written on one of
    /// those lands on the sheet and never reaches the total, and the sheet then reads as
    /// complete and is short. Not writing it is the smaller fault, and the report names it.
    /// </summary>
    public sealed class SpeciesMatch
    {
        public SpeciesMatch(
            MergedSpecies species,
            string sheetName,
            int row,
            string workbookName,
            string why,
            bool added = false,
            bool notReachedByTheTotal = false,
            SpeciesList list = null,
            SpeciesListRow listRow = null)
        {
            if (species == null) throw new ArgumentNullException("species");
            if (row < 0) throw new ArgumentOutOfRangeException("row");

            Species = species;
            SheetName = sheetName ?? string.Empty;
            Row = row;
            WorkbookName = workbookName ?? string.Empty;
            Why = why ?? string.Empty;
            Added = added;
            NotReachedByTheTotal = notReachedByTheTotal;
            HeightColumn = list == null ? string.Empty : list.HeightColumn;
            DiameterColumn = list == null ? string.Empty : list.DiameterColumn;
            WhyNoHeightColumn = list == null ? ListNotRead : list.WhyNoHeightColumn;
            WhyNoDiameterColumn = list == null ? ListNotRead : list.WhyNoDiameterColumn;
            WorkbookHeight = listRow == null ? string.Empty : listRow.Height;
            WorkbookDiameter = listRow == null ? string.Empty : listRow.Diameter;

            // Worked out here, where the list is in hand, and carried as a plain value so the
            // report holds no list of its own. Only ever printed: the match above is exact and
            // this never widens it.
            NearestInTheList = WorkbookName.Length > 0
                ? string.Empty
                : SpeciesMatching.ClosestName(list, species.BotanicalName);
        }

        /// <summary>
        /// The name in the workbook's list this one came nearest to, for a species the list
        /// does not hold, and empty for one it does. **UNKNOWN came nearest to Unknown Tree on
        /// the 1552 run and was written nowhere**, and the report could not say that, so
        /// whether the fault was one name or a family of them could not be read off a run.
        /// </summary>
        public string NearestInTheList { get; }

        public const string ListNotRead = "the sheet's list was not read here";

        /// <summary>
        /// The letter of the sheet's height column, found by its heading, or empty with the
        /// reason beside it. Where a species is written in, its height goes here.
        /// </summary>
        public string HeightColumn { get; }

        public string DiameterColumn { get; }

        public string WhyNoHeightColumn { get; }

        public string WhyNoDiameterColumn { get; }

        /// <summary>
        /// What the matched row already holds for its height and its diameter, as the cells
        /// print, so the report can name a species Revit measures differently. The client's row
        /// is theirs and nothing changes it.
        /// </summary>
        public string WorkbookHeight { get; }

        public string WorkbookDiameter { get; }

        /// <summary>
        /// True when the list holds the name on <see cref="Row"/> and the sheet's total does not
        /// reach that row, or no total was found at all. The row is kept so the report can point
        /// at it, and nothing is written there.
        /// </summary>
        public bool NotReachedByTheTotal { get; }

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
            get { return Row > 0 && SheetName.Length > 0 && !NotReachedByTheTotal; }
        }

        public MergedSpecies Species { get; }

        public string SheetName { get; }

        /// <summary>
        /// The workbook row the quantity goes on, or the row the list names when the total does
        /// not reach it, or nothing when the species did not match.
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

        /// <summary>
        /// **A row written into an empty one carries only what the model prints, and the canopy
        /// diameter is the one measure the sheet computes from**, so a species with no usable
        /// diameter cannot have a row. Measured on the MOSQUES template, Tree List - Proposed
        /// row 21: L21 reads J21, the diameter, M21 reads L21 and the count, O21 reads N21,
        /// which is typed and never written, and nothing reads I21, the height, or K21. A name
        /// and a count with no diameter leave L computing on a blank, an error that ran through
        /// seven formulas on the 1836 run and deleted the workbook. The guard was right and the
        /// rule it caught was wrong: that species has no row to be written into.
        ///
        /// **The height is not load bearing.** The first rule required both measures off the
        /// round message's wording, which made a species with a diameter and no height take no
        /// row for a cell no formula reads. It is written when the model prints one and its
        /// cell is named as not written when it does not, through the same skip every other
        /// measure cell already uses.
        ///
        /// So a species with no usable diameter is reported with its count and its reason, the
        /// way a species the sheet has no room for already is, and the run goes through. **A
        /// species that cannot be sized is not a refusal.** It is one line in the report and a
        /// workbook that computes. UNKNOWN stays withheld: DM-25 row 19 prints 0 for its
        /// diameter, and a nought is no size.
        /// </summary>
        public static string NotSized(MergedSpecies species)
        {
            if (species == null) throw new ArgumentNullException("species");

            IReadOnlyList<string> printed = species.Diameter.Found;

            return "the workbook's list does not hold this name, and a row written into an empty one carries only "
                + "what the model prints, which is no canopy diameter a workbook can compute with, "
                + "so no row was written"
                + (printed.Count == 0 ? string.Empty : ": " + string.Join(", ", printed.ToArray()));
        }

        /// <summary>
        /// The list holds the name and the total does not reach its row. Writing there puts a
        /// count on the sheet that no total adds, which reads as complete and is short.
        /// </summary>
        public static string OutsideTheTotal(int row, SpeciesList list)
        {
            return "the workbook holds this name on row " + row + " and the sheet's total, "
                + list.TotalInWords + ", reaches rows " + list.TotalFirstRow + " to " + list.TotalLastRow
                + " and not that one, so the count was not written where no total would add it";
        }

        /// <summary>
        /// Said for a matched name and for an unmatched one alike when the sheet holds no total
        /// the reader can find, because then nothing says which rows a count reaches.
        /// </summary>
        public static string NoTotalToReach(SpeciesList list)
        {
            return list.TotalInWords + ", so nothing says which rows a count reaches and it was not written";
        }

        /// <summary>
        /// The name sits past the first empty row of the list. It is neither matched nor
        /// written in above itself, and the row is named so a person can look.
        /// </summary>
        public static string BelowTheList(int row, SpeciesList list)
        {
            return "the workbook holds this name on row " + row + ", below the first empty row of its list at row "
                + list.FirstGapRow + ", so it was neither matched nor written in a second time";
        }

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
                TreeSheet sheet = SheetFor(species, template);
                if (sheet == null)
                {
                    found.Add(new SpeciesMatch(species, string.Empty, 0, string.Empty, NoSheetForTheGroup));
                    continue;
                }

                SpeciesList list = string.Equals(sheet.SheetName, template.ExistingTrees.SheetName,
                    StringComparison.Ordinal) ? existing : proposed;

                if (list == null || !list.WasRead)
                {
                    found.Add(new SpeciesMatch(species, sheet.SheetName, 0, string.Empty,
                        list == null ? NotInTheList : list.Refusal));
                    continue;
                }

                List<SpeciesListRow> holding = list.Rows
                    .Where(one => Same(one.BotanicalName, species.BotanicalName))
                    .ToList();

                if (holding.Count == 0)
                {
                    SpeciesListRow lower = list.BelowTheList
                        .FirstOrDefault(one => Same(one.BotanicalName, species.BotanicalName));
                    if (lower != null)
                    {
                        found.Add(new SpeciesMatch(species, sheet.SheetName, lower.Row, lower.BotanicalName,
                            BelowTheList(lower.Row, list), false, true, list, lower));
                        continue;
                    }

                    found.Add(WrittenInto(species, sheet.SheetName, list, free));
                    continue;
                }

                if (holding.Count > 1)
                {
                    found.Add(new SpeciesMatch(species, sheet.SheetName, 0, holding[0].BotanicalName,
                        MoreThanOneRow));
                    continue;
                }

                // The row is the list's, and the total has to reach it before a count goes there.
                SpeciesListRow row = holding[0];
                if (!list.TotalFound)
                {
                    found.Add(new SpeciesMatch(species, sheet.SheetName, row.Row, row.BotanicalName,
                        NoTotalToReach(list), false, true, list, row));
                    continue;
                }

                if (!list.Reaches(row.Row))
                {
                    found.Add(new SpeciesMatch(species, sheet.SheetName, row.Row, row.BotanicalName,
                        OutsideTheTotal(row.Row, list), false, true, list, row));
                    continue;
                }

                found.Add(new SpeciesMatch(
                    species, sheet.SheetName, row.Row, row.BotanicalName, string.Empty, false, false, list, row));
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
            if (!list.TotalFound)
            {
                return new SpeciesMatch(species, sheetName, 0, string.Empty, NoTotalToReach(list), false, false, list);
            }

            // Asked before a row is taken, so a species refused for it never uses one up, and
            // after the sheet's own total, because a sheet with no total writes nothing for
            // anybody and that is the larger fact. The diameter alone decides: it is the one
            // measure the sheet's formulas read, and the height is written when the model
            // prints one and named as not written when it does not.
            if (!species.Diameter.Write)
            {
                return new SpeciesMatch(species, sheetName, 0, string.Empty, NotSized(species), false, false, list);
            }

            Queue<int> rows;
            if (!free.TryGetValue(sheetName, out rows))
            {
                rows = new Queue<int>(list.EmptyRows);
                free[sheetName] = rows;
            }

            if (rows.Count == 0)
            {
                return new SpeciesMatch(species, sheetName, 0, string.Empty, NoEmptyRowLeft, false, false, list);
            }

            return new SpeciesMatch(species, sheetName, rows.Dequeue(), string.Empty, WrittenIn, true, false, list);
        }

        /// <summary>
        /// The sheet a merged species goes to. The rows carry it when a reader decided it, and
        /// a species built with none is placed through <see cref="CountedGroups"/> off its
        /// group name, the one resolver the readers use, so a Street Design species on STREETS
        /// lands on Tree List - Proposed here the same way it was counted there. A sheet that
        /// is neither of the template's two is nothing, and the species is reported.
        /// </summary>
        private static TreeSheet SheetFor(MergedSpecies species, KpiTemplate template)
        {
            string wanted = species.SheetName.Length > 0
                ? species.SheetName
                : CountedGroups.Of(template).SheetFor(species.GroupName);
            if (wanted == null) return null;

            foreach (TreeSheet sheet in new[] { template.ExistingTrees, template.ProposedTrees })
            {
                if (string.Equals(sheet.SheetName, wanted, StringComparison.Ordinal)) return sheet;
            }

            return null;
        }

        /// <summary>
        /// The name in the workbook's list nearest to a name the list does not hold, PRINTED
        /// AND NEVER MATCHED ON. The match itself stays exact, and widening it is the user's
        /// decision rather than this tool's.
        ///
        /// **Measured on the 1552 run.** Revit prints a species called UNKNOWN, 16 trees, and
        /// the MOSQUES Existing list holds Unknown Tree at row 101. Compared without case and
        /// with the edge spaces off they still differ, so it never reached that row and fell
        /// to the empty row route instead, where the no diameter rule withheld it. Which of
        /// those two is the fault is a question about names, and a person cannot answer it
        /// from a report that says only that a name was not held. This prints what it was
        /// nearest to, so a run answers whether it is one name or a family of them.
        ///
        /// Nearest is the longest shared opening, compared without case, and nothing at all
        /// where they share no opening. Longer is nearer, ties go to the shorter name and then
        /// to the natural order, so one run and the next print the same answer.
        /// </summary>
        public static string ClosestName(SpeciesList list, string revitName)
        {
            string wanted = (revitName ?? string.Empty).Trim();
            if (list == null || wanted.Length == 0) return string.Empty;

            string nearest = string.Empty;
            int shared = 0;

            foreach (SpeciesListRow row in list.Rows.Concat(list.BelowTheList))
            {
                string held = (row.BotanicalName ?? string.Empty).Trim();
                if (held.Length == 0) continue;

                int opening = SharedOpening(held, wanted);
                if (opening == 0) continue;

                if (opening > shared
                    || (opening == shared && held.Length < nearest.Length)
                    || (opening == shared && held.Length == nearest.Length
                        && NaturalOrder.Comparer.Compare(held, nearest) < 0))
                {
                    nearest = held;
                    shared = opening;
                }
            }

            return nearest;
        }

        /// <summary>
        /// How many characters two names open with in common, without case. UNKNOWN and
        /// Unknown Tree share seven.
        /// </summary>
        private static int SharedOpening(string one, string other)
        {
            int at = 0;
            while (at < one.Length && at < other.Length
                && char.ToUpperInvariant(one[at]) == char.ToUpperInvariant(other[at]))
            {
                at++;
            }

            return at;
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
