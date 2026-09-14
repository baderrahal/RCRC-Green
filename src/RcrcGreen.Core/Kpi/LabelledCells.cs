using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One value the tool writes, the label that finds its cell on the template's own main
    /// sheet, and how many columns right of that label the cell sits.
    /// </summary>
    public sealed class LabelledPlace
    {
        internal LabelledPlace(string name, string label, int stepsRight, string onTheRowOf = null)
        {
            Name = name;
            Label = label;
            StepsRight = stepsRight;
            OnTheRowOf = onTheRowOf ?? string.Empty;
        }

        /// <summary>
        /// What the cell is for, which is how the plan and the report ask for it. Two places
        /// can share one label, so the label cannot be the key.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The text looked for on the sheet, matched whole, without case and with edge
        /// whitespace off.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// **One for every place but the position, which is two.** No label on row 5 names the
        /// position cell: the three labels measured there reach three of the four cells and the
        /// fourth sits one further along from the person's name, under the same Prepared By.
        /// That is recorded as a distance because it is one, rather than dressed up as a label.
        /// </summary>
        public int StepsRight { get; }

        /// <summary>
        /// The place whose label's ROW this one is looked for on, or empty when the label is
        /// looked for over the whole sheet.
        ///
        /// **`Date:` is on every template TWICE**, measured on all seven on 14 September: once
        /// beside `Prepared By:` on row 5 and once beside `Reviewed By:` on row 28, or row 29 on
        /// the two park templates and STREETS. Two blocks of the same shape, one word apart. So
        /// the label alone cannot say which cell is meant, the whole sheet search refused it on
        /// all seven, and no date was written anywhere.
        ///
        /// The preparer's label is the only thing that differs, so the date is found THROUGH the
        /// preparer's block: `Prepared By:` is looked for over the whole sheet, and `Date:` is
        /// then looked for on that label's own row and nowhere else. A sheet whose reviewer block
        /// comes first is answered the same way, because the row is chosen by the label rather
        /// than by its position.
        /// </summary>
        public string OnTheRowOf { get; }

        public bool IsAnchored
        {
            get { return OnTheRowOf.Length > 0; }
        }
    }

    /// <summary>
    /// One label looked for on a template's main sheet, and the cell beside it that takes the
    /// value.
    /// </summary>
    public sealed class LabelledCell
    {
        private LabelledCell(string name, string label, bool found, string labelCell, string valueCell, string holds, string why)
        {
            Name = name ?? string.Empty;
            Label = label ?? string.Empty;
            Found = found;
            LabelCell = labelCell ?? string.Empty;
            ValueCell = valueCell ?? string.Empty;
            Holds = holds ?? string.Empty;
            Why = why ?? string.Empty;
        }

        /// <summary>
        /// What the cell is for. Prepared by and Position share one label, so this is what a
        /// caller asks by and the label alone would answer for two.
        /// </summary>
        public string Name { get; }

        public string Label { get; }

        public bool Found { get; }

        /// <summary>
        /// Where the label itself sits, recorded because a value written into the wrong cell has
        /// to trace back to the label that chose it.
        /// </summary>
        public string LabelCell { get; }

        /// <summary>
        /// The cell the value goes into, which is the label's own cell stepped right.
        /// </summary>
        public string ValueCell { get; }

        /// <summary>
        /// What that cell already holds, so the report can say a template came filled rather
        /// than reading as though this run put the value there. **Both park templates hold a
        /// position at H5 already**, measured on 14 September, and the run writes over it.
        /// </summary>
        public string Holds { get; }

        /// <summary>
        /// Whether this run would write over something somebody else put there.
        /// </summary>
        public bool WasNotEmpty
        {
            get { return Found && Holds.Length > 0; }
        }

        public string Why { get; }

        public static LabelledCell At(string name, string label, string labelCell, string valueCell, string holds)
        {
            return new LabelledCell(name, label, true, labelCell, valueCell, holds, string.Empty);
        }

        public static LabelledCell NotFound(string name, string label, string why)
        {
            return new LabelledCell(name, label, false, string.Empty, string.Empty, string.Empty, why);
        }
    }

    /// <summary>
    /// The labels found on one template's main sheet, read off the file when Create is pressed.
    /// </summary>
    public sealed class LabelledCells
    {
        private LabelledCells(bool read, string sheetName, IEnumerable<LabelledCell> all, string why)
        {
            Read = read;
            SheetName = sheetName ?? string.Empty;
            All = (all ?? Enumerable.Empty<LabelledCell>()).ToList();
            Why = why ?? string.Empty;
        }

        public bool Read { get; }

        public string SheetName { get; }

        public IReadOnlyList<LabelledCell> All { get; }

        public string Why { get; }

        public static readonly LabelledCells NotRead =
            new LabelledCells(false, string.Empty, null, "the template was not opened");

        public static LabelledCells Refused(string why)
        {
            return new LabelledCells(false, string.Empty, null, why);
        }

        public static LabelledCells Holding(string sheetName, IEnumerable<LabelledCell> all)
        {
            return new LabelledCells(true, sheetName, all, string.Empty);
        }

        /// <summary>
        /// What was found for one place, asked by its name. A sheet that was never read answers
        /// not found with the read's own reason, so nothing downstream has to ask twice.
        /// </summary>
        public LabelledCell For(string name)
        {
            LabelledCell found = All.FirstOrDefault(
                one => string.Equals(one.Name, name, StringComparison.OrdinalIgnoreCase));

            if (found != null) return found;

            LabelledPlace place = LabelledPlaces.Of(name);
            string label = place == null ? name : place.Label;

            return LabelledCell.NotFound(
                name, label, Read ? LabelledPlaces.NoLabel(label, SheetName) : Why);
        }
    }

    /// <summary>
    /// **EVERY CELL ON THE MAIN SHEET THIS TOOL WRITES BY NAME IS FOUND BY A LABEL AND NEVER BY
    /// A LETTER.**
    ///
    /// Row 7 proved why. Measured on the three workbooks written on 13 September:
    ///
    /// <code>
    /// MOSQUES   Character label C7, value D7.   Context label E7, value F7.
    /// SCHOOLS   Character label C7, value D7.   Context label E7, value F7.
    /// STREETS   Category  label C7, value D7.   Character label E7, value F7.
    ///           Context   label G7, value H7.
    /// </code>
    ///
    /// STREETS carries a Category at D7 holding a FORMULA, so its Character and Context sit one
    /// pair to the right, and a map that put Character at D7 because two templates out of three
    /// do would have overwritten that formula on the third.
    ///
    /// **Row 5 came here for the same reason, one round later.** The date, the person, their
    /// position and the plot reference were written by letter, E5, G5, H5 and C5, one array for
    /// all seven templates measured on one of them. Row 5 was then measured on all seven, on
    /// 14 September:
    ///
    /// <code>
    /// HEALTHCARE, MOSQUES, PARKING, SCHOOLS, STREETS
    ///   B5 REF :   C5 the UID   D5 Date:   E5 the date
    ///   F5 Prepared By:         G5 a name  H5 a position
    ///
    /// EXISTING PARKS and FUTURE PARKS
    ///   B5 REF :   C5 EMPTY     D5 Date:   E5 EMPTY
    ///   F5 Prepared By:         G5 EMPTY   H5 holds the text " Architect Engineer"
    /// </code>
    ///
    /// **No formula sits at any of the four on any of the seven, so nothing was ever overwritten
    /// and no workbook is damaged.** The letters were right on all seven BY LUCK, the way D7
    /// would have been right on two of three, and the next template set is what a letter cannot
    /// survive. What really differs between the two sets is what those cells HOLD.
    ///
    /// **NO LABEL NAMES THE POSITION CELL.** Three labels on row 5 reach three of the four
    /// cells, C5, E5 and G5. H5 is one further along from the person's name, under the same
    /// Prepared By, which both park templates confirm by already holding a position there. It is
    /// written as a distance of two from that label, said out loud here and in the report,
    /// because a distance dressed up as a label would be the one thing in this lookup nobody
    /// could see. Whether the real sheets name it somewhere else is UNKNOWN and is for the team.
    ///
    /// A template naming a label nowhere writes nothing for it and says which sheet it looked
    /// on. One naming a label TWICE writes nothing for either place under it and names both
    /// cells, because nothing says which is meant.
    /// </summary>
    public static class LabelledPlaces
    {
        public const string ReferenceName = "Reference";

        public const string DateName = "Date";

        public const string PreparedByName = "Prepared by";

        public const string PositionName = "Position";

        /// <summary>
        /// **Spelt exactly as the cells read**, including the space before the colon in REF :
        /// and the capital B in Prepared By. The match is whole, so a template whose label reads
        /// anything else writes nothing and says so rather than reaching for a near miss.
        /// </summary>
        public const string ReferenceLabel = "REF :";

        public const string DateLabel = "Date:";

        public const string PreparedByLabel = "Prepared By:";

        /// <summary>
        /// The table, in the order the report prints it: row 5 across, then row 7.
        /// </summary>
        public static readonly IReadOnlyList<LabelledPlace> All = new[]
        {
            new LabelledPlace(ReferenceName, ReferenceLabel, 1),

            // **On the preparer's row and nowhere else.** Every template carries Date: twice,
            // once beside Prepared By: and once beside Reviewed By: in a second block of the
            // same shape further down. See LabelledPlace.OnTheRowOf.
            new LabelledPlace(DateName, DateLabel, 1, PreparedByName),

            new LabelledPlace(PreparedByName, PreparedByLabel, 1),
            new LabelledPlace(PositionName, PreparedByLabel, 2),
            new LabelledPlace(FixedCells.CharacterLabel, FixedCells.CharacterLabel, 1),
            new LabelledPlace(FixedCells.ContextLabel, FixedCells.ContextLabel, 1)
        };

        public static LabelledPlace Of(string name)
        {
            return All.FirstOrDefault(
                one => string.Equals(one.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public static string NoLabel(string label, string sheetName)
        {
            return NoLabel(label, sheetName, string.Empty);
        }

        public static string NoLabel(string label, string sheetName, string onlyOn)
        {
            return "no cell on " + sheetName + onlyOn + " reads " + label
                + ", so nothing is written and no cell is guessed at";
        }

        public static string TwiceOn(string label, string sheetName, IEnumerable<string> cells)
        {
            return TwiceOn(label, sheetName, string.Empty, cells);
        }

        public static string TwiceOn(string label, string sheetName, string onlyOn, IEnumerable<string> cells)
        {
            return label + " is on " + sheetName + onlyOn + " at "
                + string.Join(" and ", (cells ?? Enumerable.Empty<string>()).ToArray())
                + ", and nothing says which is meant";
        }

        /// <summary>
        /// Which row an anchored place was allowed to look on, said in the reason so a person
        /// can check the answer against the sheet rather than taking the row on trust.
        /// </summary>
        public static string OnlyOn(int row, string anchorLabel)
        {
            return " row " + row.ToString(CultureInfo.InvariantCulture)
                + ", the row " + anchorLabel + " sits on,";
        }

        /// <summary>
        /// An anchored place whose anchor was not found has no row to look on, and it carries
        /// the anchor's own reason rather than a second wording of it.
        /// </summary>
        public static string NoRowToLookOn(string label, LabelledCell anchor)
        {
            return "there is no row to look for " + label + " on, because "
                + anchor.Label + " was not found: " + anchor.Why;
        }

        /// <summary>
        /// The labels on one template's main sheet, read off the file. **A label is matched whole,
        /// without case and with edge whitespace off**, so Characteristics is not Character.
        /// </summary>
        public static LabelledCells In(string path, KpiTemplate template)
        {
            if (template == null) throw new ArgumentNullException("template");
            if (string.IsNullOrWhiteSpace(path)) return LabelledCells.NotRead;

            try
            {
                using (var reading = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var zip = new ZipArchive(reading, ZipArchiveMode.Read))
                {
                    string workbookPart = WorkbookPackage.WorkbookPartPath(zip);
                    if (workbookPart == null) return LabelledCells.Refused("the template is not a workbook this tool can open");

                    Dictionary<string, string> sheets = WorkbookPackage.SheetParts(zip, workbookPart);
                    string part;
                    if (!sheets.TryGetValue(template.MainSheetName, out part))
                    {
                        return LabelledCells.Refused(
                            "the template holds no sheet named " + template.MainSheetName);
                    }

                    XDocument sheet = WorkbookPackage.Read(zip, part);
                    if (sheet == null) return LabelledCells.Refused("the main sheet could not be read");

                    return Off(template.MainSheetName, sheet, WorkbookPackage.SharedStrings(zip));
                }
            }
            catch (IOException failed)
            {
                return LabelledCells.Refused("the template could not be opened: " + failed.Message);
            }
            catch (UnauthorizedAccessException failed)
            {
                return LabelledCells.Refused("the template could not be opened: " + failed.Message);
            }
            catch (InvalidDataException failed)
            {
                return LabelledCells.Refused("the template is not a readable .xlsx: " + failed.Message);
            }
        }

        internal static LabelledCells Off(string sheetName, XDocument sheet, List<string> shared)
        {
            var texts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (XElement cell in WorkbookPackage.Cells(sheet))
            {
                XAttribute reference = cell.Attribute("r");
                if (reference == null) continue;
                if (CellRef.TryParse(reference.Value) == null) continue;

                texts[reference.Value] = (WorkbookPackage.TextOf(cell, shared) ?? string.Empty).Trim();
            }

            var found = new Dictionary<string, LabelledCell>(StringComparer.OrdinalIgnoreCase);

            // A place that names its own label is looked for over the whole sheet. An ANCHORED
            // place waits, because the row it is allowed to look on is not known until the
            // place it is anchored to has been found.
            foreach (LabelledPlace place in All)
            {
                if (place.IsAnchored) continue;

                found[place.Name] = Looking(place, texts, sheetName, null, string.Empty);
            }

            foreach (LabelledPlace place in All)
            {
                if (!place.IsAnchored) continue;

                LabelledCell anchor = found[place.OnTheRowOf];
                if (!anchor.Found)
                {
                    found[place.Name] = LabelledCell.NotFound(
                        place.Name, place.Label, NoRowToLookOn(place.Label, anchor));
                    continue;
                }

                found[place.Name] = Looking(
                    place, texts, sheetName, CellRef.Parse(anchor.LabelCell).Row, anchor.Label);
            }

            return LabelledCells.Holding(sheetName, All.Select(one => found[one.Name]).ToList());
        }

        /// <summary>
        /// One place, over the whole sheet when <paramref name="onlyRow"/> is null and over that
        /// row alone when it is not. Nothing found and more than one found are both refusals,
        /// and each says which of the two it was and where it looked.
        /// </summary>
        private static LabelledCell Looking(
            LabelledPlace place,
            Dictionary<string, string> texts,
            string sheetName,
            int? onlyRow,
            string anchorLabel)
        {
            string onlyOn = onlyRow == null ? string.Empty : OnlyOn(onlyRow.Value, anchorLabel);

            string[] at = texts
                .Where(one => string.Equals(one.Value, place.Label, StringComparison.OrdinalIgnoreCase))
                .Where(one => onlyRow == null || CellRef.Parse(one.Key).Row == onlyRow.Value)
                .Select(one => one.Key)
                .OrderBy(one => CellRef.Parse(one).Row)
                .ThenBy(one => CellRef.Parse(one).ColumnNumber)
                .ToArray();

            if (at.Length == 0)
            {
                return LabelledCell.NotFound(
                    place.Name, place.Label, NoLabel(place.Label, sheetName, onlyOn));
            }

            if (at.Length > 1)
            {
                // Two places under one label refuse together, because the label they share is
                // the thing that cannot be resolved.
                return LabelledCell.NotFound(
                    place.Name, place.Label, TwiceOn(place.Label, sheetName, onlyOn, at));
            }

            CellRef where = CellRef.Parse(at[0]);
            string valueCell = RightOf(where, place.StepsRight);

            string holds;
            return LabelledCell.At(
                place.Name, place.Label, at[0], valueCell,
                texts.TryGetValue(valueCell, out holds) ? holds : string.Empty);
        }

        /// <summary>
        /// The cell so many columns to the right of a label, in the sheet's own column letters.
        /// A is 1, so C stepped once is D and F stepped twice is H.
        /// </summary>
        public static string RightOf(CellRef label, int steps)
        {
            if (label == null) throw new ArgumentNullException("label");
            if (steps < 1) throw new ArgumentOutOfRangeException("steps", "a value cell is at least one column right of its label");

            int number = label.ColumnNumber + steps;
            string column = string.Empty;
            while (number > 0)
            {
                int letter = (number - 1) % 26;
                column = (char)('A' + letter) + column;
                number = (number - 1) / 26;
            }

            return column + label.Row;
        }
    }

    /// <summary>
    /// The two values that are the same on every plot of every template.
    ///
    /// **Bader's answer: Character is always Urban Area Zone and Context is always Urban.** They
    /// come from no model, no schedule and no file, so there is nothing to read and nothing to
    /// agree with. That makes them data, here, beside the reason. Where each one GOES is in
    /// <see cref="LabelledPlaces"/> with every other cell found by a label.
    /// </summary>
    public static class FixedCells
    {
        public const string CharacterLabel = "Character";

        public const string ContextLabel = "Context";

        public const string CharacterValue = "Urban Area Zone";

        public const string ContextValue = "Urban";

        /// <summary>
        /// What the pane and the report say about where these two come from. They are the
        /// team's answer rather than anything the tool found, said the way the section depth and
        /// the annotation crop are said on the other tool.
        /// </summary>
        public const string SourceInWords =
            "the same on every plot, the team's answer rather than anything read, written into "
            + "the cell right of the label on the template's own sheet";

        public static string ValueOf(KpiValue value)
        {
            return value == KpiValue.Character ? CharacterValue : ContextValue;
        }

        public static string LabelOf(KpiValue value)
        {
            return value == KpiValue.Character ? CharacterLabel : ContextLabel;
        }

        /// <summary>
        /// The two, in the order the report prints them.
        /// </summary>
        public static readonly IReadOnlyList<KpiValue> Both =
            new[] { KpiValue.Character, KpiValue.Context };
    }
}
