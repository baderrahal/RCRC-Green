using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One cell that got no value, with the reason. A cell left empty and not written down
    /// reads as a cell the team forgot rather than one the tool could not fill.
    /// </summary>
    public sealed class NotWritten
    {
        public NotWritten(string sheetName, string cell, string what, string why)
        {
            What = what ?? string.Empty;
            SheetName = sheetName ?? string.Empty;
            Cell = cell ?? string.Empty;
            Why = why ?? string.Empty;
        }

        public string SheetName { get; }

        public string Cell { get; }

        public string What { get; }

        public string Why { get; }
    }

    /// <summary>
    /// A matched species whose height or diameter in Revit is not what the client's row holds.
    /// Named and left alone: PHOENIX DACTYLIFERA prints 15 metres across in the model and the
    /// MOSQUES existing list holds 8 at row 86, two numbers for one species, and which is
    /// right is a question for the team rather than a cell to overwrite.
    /// </summary>
    public sealed class MeasureDifference
    {
        public MeasureDifference(string sheetName, int row, string workbookName, string what, string revitPrints, string workbookHolds)
        {
            SheetName = sheetName ?? string.Empty;
            Row = row;
            WorkbookName = workbookName ?? string.Empty;
            What = what ?? string.Empty;
            RevitPrints = revitPrints ?? string.Empty;
            WorkbookHolds = workbookHolds ?? string.Empty;
        }

        public string SheetName { get; }

        public int Row { get; }

        public string WorkbookName { get; }

        public string What { get; }

        public string RevitPrints { get; }

        public string WorkbookHolds { get; }
    }

    /// <summary>
    /// Every cell this fill would write, and every cell it would not, with the reason.
    ///
    /// The plan is worked out in full before anything is copied, so a refusal writes no file.
    /// It is the intention. What really landed is read back off the output afterwards and is a
    /// different object, the way RunPlan and RunOutcome are on the Drawing Sheet, because a
    /// report that prints the intention as though it were the outcome is how four views came to
    /// be listed as both created and not created.
    /// </summary>
    public sealed class KpiCreatePlan
    {
        public const string NotInThisTemplate = "not in this template";

        /// <summary>
        /// Said of the area on a template whose map names no cell for it, which is none of the
        /// seven since the client emptied STREETS H8. It used to read that the sheet works the
        /// area out from the road width and the length, which was STREETS and is no longer true
        /// of anything.
        /// </summary>
        public const string NoAreaCell =
            "this template names no area cell, so no filled region was read for it";

        /// <summary>
        /// Said of the road width and the total length on a template that has no such cell,
        /// which is the other six. It is not the STREETS refusal and must not read like one.
        /// </summary>
        public const string NotAStreetTemplate =
            "only the STREETS template has this cell";

        /// <summary>
        /// Every cell found by a label, as the plan found it, so the report can say which cell
        /// each value went into and WHAT IT ALREADY HELD. **A template that came filled already
        /// holds the value**, measured on the mosque file at D7 and on both park files at H5,
        /// and a run that writes over somebody's text has to say so rather than reading as
        /// though the cell had been empty.
        /// </summary>
        public IReadOnlyList<LabelledCell> Labelled { get; private set; }

        public const string NotFound = "not found on any chosen plot";

        public const string Disagreed = "the chosen plots disagreed";

        public const string TypedByTheTeam = "nobody typed it on the pane, and it comes from no model";

        private KpiCreatePlan(
            KpiTemplate template,
            IReadOnlyList<CellWrite> writes,
            IReadOnlyList<NotWritten> skipped,
            IReadOnlyList<SpeciesMatch> matches,
            IReadOnlyList<MeasureDifference> differences)
        {
            Template = template;
            Writes = writes;
            Skipped = skipped;
            Matches = matches;
            Differences = differences;
            Labelled = new List<LabelledCell>();
        }

        public KpiTemplate Template { get; }

        /// <summary>
        /// Every matched species whose height or diameter in Revit differs from what its row
        /// already holds. Reported, and nothing written over the client's own numbers.
        /// </summary>
        public IReadOnlyList<MeasureDifference> Differences { get; }

        /// <summary>
        /// Every cell on the main sheet this fill names, written or not, for the formula check
        /// to say whether the workbook's own arithmetic has its inputs. That is the map's own
        /// cells AND the cells the labels chose, because the reference left the map for a label
        /// and dropping it here would quietly shorten the check.
        /// </summary>
        public IReadOnlyList<WorkbookCell> ComputesFrom
        {
            get
            {
                return Template.Cells.Select(cell => cell.Cell)
                    .Concat(Labelled.Where(one => one.Found).Select(one => one.ValueCell))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(cell => new WorkbookCell(Template.MainSheetName, cell))
                    .ToList();
            }
        }

        public IReadOnlyList<CellWrite> Writes { get; }

        public IReadOnlyList<NotWritten> Skipped { get; }

        public IReadOnlyList<SpeciesMatch> Matches { get; }

        public IEnumerable<SpeciesMatch> Unmatched
        {
            get { return Matches.Where(one => !one.Matched); }
        }

        public static KpiCreatePlan Of(
            KpiTemplate template,
            AgreedValue component,
            AgreedValue reference,
            string location,
            Totalled area,
            Totalled shrubs,
            Totalled lawn,
            IEnumerable<SpeciesMatch> matches,
            string date,
            string preparedBy,
            string position,
            StreetReferenceAnswer street,
            LabelledCells labels)
        {
            if (template == null) throw new ArgumentNullException("template");

            // **A REQUIRED ARGUMENT, like the three the team types.** Those three defaulted to
            // null once, the handler never passed them, and the first real workbook went out
            // carrying the template's own placeholders while the report said nobody had typed
            // them. A street run whose two cells went missing the same way would look filled and
            // compute an area of nothing.
            if (street == null) throw new ArgumentNullException("street");
            if (labels == null) throw new ArgumentNullException("labels");

            var writes = new List<CellWrite>();
            var skipped = new List<NotWritten>();

            // None of the three comes from Revit. The team types them on the pane and the tool
            // copies them through, so a filled checklist carries who filled it and when.
            //
            // **These three are required arguments and used to default to null.** The handler
            // called this without them, so the first real workbook came out holding the
            // template's own placeholders while the report said nobody had typed them, on a run
            // where all three boxes were filled in. A default that reads as a deliberate empty
            // is how a whole link in a chain goes missing in silence.
            //
            // **They used to go in by letter, E5, G5 and H5 on all seven.** They go into the
            // cells their labels choose now, for the reason row 7 already proved.
            Typed(template, LabelledPlaces.DateName, "Date", date, labels, writes, skipped);
            Typed(template, LabelledPlaces.PreparedByName, "Prepared by", preparedBy, labels, writes, skipped);
            Typed(template, LabelledPlaces.PositionName, "Position", position, labels, writes, skipped);

            Agreed(template, KpiValue.Component, component, writes, skipped);

            // The plot reference went in by letter too, C5 on all seven, and REF : at B5 names
            // it on all seven, so it is found the same way as the three above.
            AgreedAtTheLabel(template, LabelledPlaces.ReferenceName, KpiValue.Reference, reference, labels, writes, skipped);
            Text(template, KpiValue.Location, location, writes, skipped);
            Number(template, KpiValue.Area, area, writes, skipped);
            Number(template, KpiValue.Shrubs, shrubs, writes, skipped);
            Number(template, KpiValue.Lawn, lawn, writes, skipped);

            // The two cells the STREETS sheet says are typed by hand, off the reference file.
            // H8 is the two multiplied and the workbook computes it, so nothing goes there.
            FromTheStreetFile(template, KpiValue.StreetsRoadWidth, street, street.Width, writes, skipped);
            FromTheStreetFile(template, KpiValue.StreetsTotalLength, street, street.Length, writes, skipped);

            // Always the same two words, into the cell right of each label on this template's
            // own sheet. Never a letter: STREETS carries a Category formula where the other two
            // carry Character, so a letter taken off two templates would overwrite it on a third.
            foreach (KpiValue value in FixedCells.Both) Fixed(template, value, labels, writes, skipped);

            List<SpeciesMatch> held = (matches ?? Enumerable.Empty<SpeciesMatch>())
                .Where(one => one != null)
                .ToList();
            var differences = new List<MeasureDifference>();

            foreach (SpeciesMatch match in held)
            {
                if (!match.Placed)
                {
                    // A row the total does not reach is still a row, and the cell is named so
                    // the line points at where the count was not put.
                    skipped.Add(new NotWritten(
                        match.SheetName,
                        match.Row > 0
                            ? KpiTemplates.QuantityColumn + match.Row.ToString(CultureInfo.InvariantCulture)
                            : string.Empty,
                        match.Species.BotanicalName + " " + match.Species.Quantity
                            + " under " + Shown(match.Species.GroupName),
                        match.Why));
                    continue;
                }

                writes.Add(CellWrite.Number(
                    match.SheetName,
                    KpiTemplates.QuantityColumn + match.Row.ToString(CultureInfo.InvariantCulture),
                    match.Species.Quantity));

                if (!match.Added)
                {
                    // The client's row keeps its own height and diameter. A Revit value that
                    // differs is named and nothing is written over it.
                    Differing(differences, match, "height", match.Species.Height, match.WorkbookHeight);
                    Differing(differences, match, "diameter", match.Species.Diameter, match.WorkbookDiameter);
                    continue;
                }

                // The name as Revit spells it, because there is nothing else to spell it from,
                // then the height and the diameter the schedule printed beside it, into the
                // columns the sheet's header row names, and NOTHING in any other column.
                // Family, genus, native and every code column are the client's data and Revit
                // does not print them. A row written with a name and a count alone broke the
                // canopy maths on the 1428 run: the sheet's own formula returns a space for a
                // blank diameter and the cell beside it multiplied that space by the count.
                writes.Add(CellWrite.Text(
                    match.SheetName,
                    KpiTemplates.BotanicalColumn + match.Row.ToString(CultureInfo.InvariantCulture),
                    match.Species.BotanicalName));

                Measured(match, "height", match.Species.Height, match.HeightColumn, match.WhyNoHeightColumn, writes, skipped);
                Measured(match, "diameter", match.Species.Diameter, match.DiameterColumn, match.WhyNoDiameterColumn, writes, skipped);
            }

            return new KpiCreatePlan(template, writes, skipped, held, differences)
            {
                Labelled = LabelledPlaces.All.Select(one => labels.For(one.Name)).ToList()
            };
        }

        /// <summary>
        /// One of the two values that are the same on every plot, into the cell its label chose,
        /// or not written with the reason. Nothing looks for the word Category, so the street
        /// sheet's own formula at D7 is never reached.
        /// </summary>
        private static void Fixed(
            KpiTemplate template,
            KpiValue value,
            LabelledCells labels,
            List<CellWrite> writes,
            List<NotWritten> skipped)
        {
            LabelledCell found = labels.For(FixedCells.LabelOf(value));
            if (!found.Found)
            {
                skipped.Add(new NotWritten(
                    template.MainSheetName, string.Empty, value.ToString(), found.Why));
                return;
            }

            writes.Add(CellWrite.Text(
                template.MainSheetName, found.ValueCell, FixedCells.ValueOf(value)));
        }

        /// <summary>
        /// The road width or the total length off the reference file. A template with no such
        /// cell says so in its own words, and a plot the file could not answer for carries the
        /// file's own reason rather than a second wording of it.
        /// </summary>
        private static void FromTheStreetFile(
            KpiTemplate template,
            KpiValue value,
            StreetReferenceAnswer street,
            double number,
            List<CellWrite> writes,
            List<NotWritten> skipped)
        {
            MappedCell cell = template.CellFor(value);
            if (cell == null)
            {
                skipped.Add(new NotWritten(
                    template.MainSheetName, string.Empty, value.ToString(), NotAStreetTemplate));
                return;
            }

            if (!street.Found)
            {
                skipped.Add(new NotWritten(template.MainSheetName, cell.Cell, value.ToString(), street.Why));
                return;
            }

            writes.Add(CellWrite.Number(template.MainSheetName, cell.Cell, number));
        }

        /// <summary>
        /// One of the two measures into its column, or not, with the reason: no column on the
        /// sheet, no row that printed a value, or rows that disagree. Nothing is averaged and
        /// nothing is taken first.
        /// </summary>
        private static void Measured(
            SpeciesMatch match,
            string what,
            MeasureAnswer answer,
            string column,
            string whyNoColumn,
            List<CellWrite> writes,
            List<NotWritten> skipped)
        {
            string label = match.Species.BotanicalName + " " + what;
            string row = match.Row.ToString(CultureInfo.InvariantCulture);

            if (string.IsNullOrEmpty(column))
            {
                skipped.Add(new NotWritten(match.SheetName, string.Empty, label, whyNoColumn));
                return;
            }

            if (!answer.Write)
            {
                skipped.Add(new NotWritten(match.SheetName, column + row, label, answer.Why));
                return;
            }

            writes.Add(CellWrite.Number(match.SheetName, column + row, answer.Value));
        }

        private static void Differing(
            List<MeasureDifference> differences, SpeciesMatch match, string what, MeasureAnswer answer, string workbookHolds)
        {
            if (!answer.Write) return;

            CellNumberRead held = CellNumber.Read(workbookHolds);
            bool same = held.IsNumber
                && Math.Abs(held.Value - answer.Value) <= Totalled.Tolerance * Math.Max(1.0, Math.Abs(answer.Value));
            if (same) return;

            differences.Add(new MeasureDifference(
                match.SheetName, match.Row, match.WorkbookName, what,
                answer.Value.ToString("0.##", CultureInfo.InvariantCulture),
                string.IsNullOrWhiteSpace(workbookHolds) ? "(blank)" : workbookHolds.Trim()));
        }

        /// <summary>
        /// One of the three the team types, into the cell its label chose. A template whose
        /// sheet does not name the label writes nothing and carries the read's own reason, and
        /// a box nobody typed into carries its own, so the two cases never read alike.
        /// </summary>
        private static void Typed(
            KpiTemplate template,
            string name,
            string what,
            string held,
            LabelledCells labels,
            List<CellWrite> writes,
            List<NotWritten> skipped)
        {
            LabelledCell found = labels.For(name);
            if (!found.Found)
            {
                skipped.Add(new NotWritten(template.MainSheetName, string.Empty, what, found.Why));
                return;
            }

            if (string.IsNullOrWhiteSpace(held))
            {
                skipped.Add(new NotWritten(template.MainSheetName, found.ValueCell, what, TypedByTheTeam));
                return;
            }

            writes.Add(CellWrite.Text(template.MainSheetName, found.ValueCell, held.Trim()));
        }

        /// <summary>
        /// A value every chosen plot has to agree on, into the cell its label chose rather than
        /// the cell a map names. The refusals are the mapped <see cref="Agreed"/>'s own, so the
        /// two cannot say different things about one disagreement.
        /// </summary>
        private static void AgreedAtTheLabel(
            KpiTemplate template,
            string name,
            KpiValue value,
            AgreedValue agreed,
            LabelledCells labels,
            List<CellWrite> writes,
            List<NotWritten> skipped)
        {
            LabelledCell found = labels.For(name);
            if (!found.Found)
            {
                skipped.Add(new NotWritten(
                    template.MainSheetName, string.Empty, value.ToString(), found.Why));
                return;
            }

            if (agreed == null || agreed.Distinct.Count == 0)
            {
                skipped.Add(new NotWritten(template.MainSheetName, found.ValueCell, value.ToString(), NotFound));
                return;
            }

            if (!agreed.Agrees)
            {
                skipped.Add(new NotWritten(template.MainSheetName, found.ValueCell, value.ToString(),
                    Disagreed + ": " + string.Join(", ", agreed.Distinct.ToArray())));
                return;
            }

            writes.Add(CellWrite.Text(template.MainSheetName, found.ValueCell, agreed.Value));
        }

        private static void Agreed(
            KpiTemplate template,
            KpiValue value,
            AgreedValue agreed,
            List<CellWrite> writes,
            List<NotWritten> skipped)
        {
            MappedCell cell = template.CellFor(value);
            if (cell == null)
            {
                skipped.Add(new NotWritten(template.MainSheetName, string.Empty, value.ToString(),
                    NotInThisTemplate));
                return;
            }

            if (agreed == null || agreed.Distinct.Count == 0)
            {
                skipped.Add(new NotWritten(template.MainSheetName, cell.Cell, value.ToString(), NotFound));
                return;
            }

            if (!agreed.Agrees)
            {
                skipped.Add(new NotWritten(template.MainSheetName, cell.Cell, value.ToString(),
                    Disagreed + ": " + string.Join(", ", agreed.Distinct.ToArray())));
                return;
            }

            writes.Add(CellWrite.Text(template.MainSheetName, cell.Cell, agreed.Value));
        }

        private static void Text(
            KpiTemplate template,
            KpiValue value,
            string held,
            List<CellWrite> writes,
            List<NotWritten> skipped)
        {
            MappedCell cell = template.CellFor(value);
            if (cell == null)
            {
                skipped.Add(new NotWritten(template.MainSheetName, string.Empty, value.ToString(),
                    NotInThisTemplate));
                return;
            }

            if (string.IsNullOrWhiteSpace(held))
            {
                skipped.Add(new NotWritten(template.MainSheetName, cell.Cell, value.ToString(), NotFound));
                return;
            }

            writes.Add(CellWrite.Text(template.MainSheetName, cell.Cell, held.Trim()));
        }

        private static void Number(
            KpiTemplate template,
            KpiValue value,
            Totalled total,
            List<CellWrite> writes,
            List<NotWritten> skipped)
        {
            MappedCell cell = template.CellFor(value);
            if (cell == null)
            {
                skipped.Add(new NotWritten(template.MainSheetName, string.Empty, value.ToString(),
                    value == KpiValue.Area ? NoAreaCell : NotInThisTemplate));
                return;
            }

            if (total == null || total.PerPlot.Count == 0)
            {
                skipped.Add(new NotWritten(template.MainSheetName, cell.Cell, value.ToString(), NotFound));
                return;
            }

            writes.Add(CellWrite.Number(template.MainSheetName, cell.Cell, total.Total));
        }

        private static string Shown(string value)
        {
            return string.IsNullOrEmpty(value) ? "(no group)" : value;
        }
    }
}
