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

        public const string TypedByHand =
            "typed by hand, because the sheet works the area out from the road width and length";

        public const string NotFound = "not found on any chosen plot";

        public const string Disagreed = "the chosen plots disagreed";

        public const string TypedByTheTeam = "nobody typed it on the pane, and it comes from no model";

        private KpiCreatePlan(
            KpiTemplate template,
            IReadOnlyList<CellWrite> writes,
            IReadOnlyList<NotWritten> skipped,
            IReadOnlyList<SpeciesMatch> matches)
        {
            Template = template;
            Writes = writes;
            Skipped = skipped;
            Matches = matches;
        }

        public KpiTemplate Template { get; }

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
            string position)
        {
            if (template == null) throw new ArgumentNullException("template");

            var writes = new List<CellWrite>();
            var skipped = new List<NotWritten>();

            // E5, G5 and H5 sit in the same place in every template and none of them comes
            // from Revit. The team types them on the pane and the tool copies them through,
            // so a filled checklist carries who filled it and when.
            //
            // **These three are required arguments and used to default to null.** The handler
            // called this without them, so the first real workbook came out holding the
            // template's own <Date>, <Name> and <Position> while the report said nobody had
            // typed them, on a run where all three boxes were filled in. A default that reads
            // as a deliberate empty is how a whole link in a chain goes missing in silence.
            Typed(template, KpiTemplates.TypedByTheTeam[0], "Date", date, writes, skipped);
            Typed(template, KpiTemplates.TypedByTheTeam[1], "Prepared by", preparedBy, writes, skipped);
            Typed(template, KpiTemplates.TypedByTheTeam[2], "Position", position, writes, skipped);

            Agreed(template, KpiValue.Component, component, writes, skipped);
            Agreed(template, KpiValue.Reference, reference, writes, skipped);
            Text(template, KpiValue.Location, location, writes, skipped);
            Number(template, KpiValue.Area, area, writes, skipped);
            Number(template, KpiValue.Shrubs, shrubs, writes, skipped);
            Number(template, KpiValue.Lawn, lawn, writes, skipped);

            List<SpeciesMatch> held = (matches ?? Enumerable.Empty<SpeciesMatch>())
                .Where(one => one != null)
                .ToList();

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

                if (!match.Added) continue;

                // The name as Revit spells it, because there is nothing else to spell it from,
                // and NOTHING in any other column. Family, genus, native and every code column
                // are the client's data and the tool does not know them.
                writes.Add(CellWrite.Text(
                    match.SheetName,
                    KpiTemplates.BotanicalColumn + match.Row.ToString(CultureInfo.InvariantCulture),
                    match.Species.BotanicalName));
            }

            return new KpiCreatePlan(template, writes, skipped, held);
        }

        private static void Typed(
            KpiTemplate template,
            string cell,
            string what,
            string held,
            List<CellWrite> writes,
            List<NotWritten> skipped)
        {
            if (string.IsNullOrWhiteSpace(held))
            {
                skipped.Add(new NotWritten(template.MainSheetName, cell, what, TypedByTheTeam));
                return;
            }

            writes.Add(CellWrite.Text(template.MainSheetName, cell, held.Trim()));
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
                    value == KpiValue.Area ? TypedByHand : NotInThisTemplate));
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
