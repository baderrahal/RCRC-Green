using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One species printed on more than one row under one group on one plot, with the rows.
    /// </summary>
    public sealed class RepeatedSpecies
    {
        public RepeatedSpecies(string botanicalName, string groupName, IEnumerable<SpeciesRow> rows)
        {
            if (botanicalName == null) throw new ArgumentNullException("botanicalName");

            BotanicalName = botanicalName;
            GroupName = groupName ?? string.Empty;
            Rows = (rows ?? Enumerable.Empty<SpeciesRow>()).Where(one => one != null).OrderBy(one => one.RowNumber).ToList();
        }

        public string BotanicalName { get; }

        public string GroupName { get; }

        public IReadOnlyList<SpeciesRow> Rows { get; }

        /// <summary>
        /// rows 12 and 13, counting 10 and 10.
        /// </summary>
        public string InWords
        {
            get
            {
                return "rows " + Joined(Rows.Select(one => one.RowNumber.ToString(CultureInfo.InvariantCulture)))
                    + ", counting " + Joined(Rows.Select(one => one.Quantity.ToString(CultureInfo.InvariantCulture)));
            }
        }

        private static string Joined(IEnumerable<string> parts)
        {
            List<string> held = parts.ToList();
            if (held.Count <= 1) return string.Join(string.Empty, held.ToArray());

            return string.Join(", ", held.Take(held.Count - 1).ToArray()) + " and " + held[held.Count - 1];
        }
    }

    /// <summary>
    /// One measure a schedule prints beside a species, the height or the canopy diameter, as
    /// the cell printed it and whether that is a number a workbook can compute with.
    ///
    /// **Held means a number greater than nought.** UNKNOWN prints a dash for its height and 0
    /// for its diameter on the real model, and a canopy worked out from a nought is a nought
    /// written where the workbook's own formula expected a blank. A cell holding a digit past
    /// its number is not read, the way a count is not, and the reason travels with the row into
    /// the report rather than refusing the schedule, because a species counts whether or not
    /// its height reads.
    /// </summary>
    public sealed class PrintedMeasure
    {
        private PrintedMeasure(string printed, bool held, double value, string whyNotHeld)
        {
            Printed = printed ?? string.Empty;
            Held = held;
            Value = value;
            WhyNotHeld = whyNotHeld ?? string.Empty;
        }

        /// <summary>
        /// A row that came from somewhere the measures were never read, a fixture or an older
        /// caller. Not held, and it says so rather than reading as a blank cell.
        /// </summary>
        public static readonly PrintedMeasure NotRead = new PrintedMeasure(string.Empty, false, 0.0, "was not read");

        public static PrintedMeasure NoColumn(string word)
        {
            return new PrintedMeasure(string.Empty, false, 0.0,
                "the heading row names no column holding " + (word ?? string.Empty));
        }

        public static PrintedMeasure Of(string cell)
        {
            string text = (cell ?? string.Empty).Trim();
            CellNumberRead read = CellNumber.Read(text);

            if (read.IsRefused) return new PrintedMeasure(text, false, 0.0, read.Refusal);
            if (read.IsEmpty)
            {
                return new PrintedMeasure(text, false, 0.0,
                    text.Length == 0 ? "prints nothing" : "prints '" + text + "'");
            }

            if (read.Value <= 0.0)
            {
                return new PrintedMeasure(text, false, read.Value, "prints " + text + ", which is no size");
            }

            return new PrintedMeasure(text, true, read.Value, string.Empty);
        }

        public static PrintedMeasure Holding(double value)
        {
            if (value <= 0.0) throw new ArgumentOutOfRangeException("value");

            return new PrintedMeasure(value.ToString("0.##", CultureInfo.InvariantCulture), true, value, string.Empty);
        }

        /// <summary>
        /// Exactly as the schedule printed the cell, so the report can show it.
        /// </summary>
        public string Printed { get; }

        public bool Held { get; }

        public double Value { get; }

        public string WhyNotHeld { get; }
    }

    /// <summary>
    /// One species row as the softscape schedule printed it, with the group row it sat under,
    /// the row it printed on, and the height and the canopy diameter printed beside it.
    ///
    /// The group travels with the row because a species is not unique in the schedule.
    /// ALBIZIA LEBBECK on DM-12 is 1 under Existing and 13 under Proposed, and a row that
    /// arrives without its group collapses those two numbers into one in the wrong sheet.
    ///
    /// **The row number travels too**, so a species the schedule prints on two rows under one
    /// group can be named by its rows. FM-05 prints ALBIZIA LEBBECK twice under Proposed, 10
    /// and 10, and for two rounds that read as one plot appearing twice.
    /// </summary>
    public sealed class SpeciesRow
    {
        public SpeciesRow(
            string botanicalName,
            string groupName,
            int quantity,
            string plotId = null,
            int rowNumber = 0,
            PrintedMeasure height = null,
            PrintedMeasure diameter = null)
        {
            if (botanicalName == null) throw new ArgumentNullException("botanicalName");
            if (quantity < 0) throw new ArgumentOutOfRangeException("quantity");
            if (rowNumber < 0) throw new ArgumentOutOfRangeException("rowNumber");

            BotanicalName = botanicalName;
            GroupName = groupName ?? string.Empty;
            Quantity = quantity;
            PlotId = plotId ?? string.Empty;
            RowNumber = rowNumber;
            Height = height ?? PrintedMeasure.NotRead;
            Diameter = diameter ?? PrintedMeasure.NotRead;
        }

        /// <summary>
        /// The same row with its plot named, for a row that travels out of its reading.
        /// </summary>
        public SpeciesRow OnPlot(string plotId)
        {
            return new SpeciesRow(BotanicalName, GroupName, Quantity, plotId, RowNumber, Height, Diameter);
        }

        /// <summary>
        /// The printed row, counting the heading row as 1, the way every reader here numbers a
        /// row. Nought when the row came from no schedule.
        /// </summary>
        public int RowNumber { get; }

        /// <summary>
        /// HEIGHT (m) as the schedule printed it. The workbook's Mature Height column, measured
        /// identical on six species sitting in both.
        /// </summary>
        public PrintedMeasure Height { get; }

        /// <summary>
        /// DIAMETER (m) as the schedule printed it, which is the workbook's Average Mature
        /// Canopy Diameter and what its canopy formula computes from.
        /// </summary>
        public PrintedMeasure Diameter { get; }

        public string BotanicalName { get; }

        /// <summary>
        /// Empty when the row sat under no group row at all. Such a row is reported and never
        /// assumed into a group, because guessing puts a count in the wrong sheet.
        /// </summary>
        public string GroupName { get; }

        public int Quantity { get; }

        public string PlotId { get; }

        public bool HasGroup
        {
            get { return GroupName.Length > 0; }
        }
    }

    /// <summary>
    /// One filled region in the 00 link that could be a plot's intervention area, with the
    /// area it holds. Which of a plot's two regions carries the area varies by plot, so both
    /// are offered and the type name decides nothing.
    /// </summary>
    public sealed class RegionArea
    {
        public RegionArea(string typeName, double rawSquareFeet, double squareMetres, string printed)
        {
            if (typeName == null) throw new ArgumentNullException("typeName");

            TypeName = typeName;
            RawSquareFeet = rawSquareFeet;
            SquareMetres = squareMetres;
            Printed = printed ?? string.Empty;
        }

        public string TypeName { get; }

        /// <summary>
        /// What Revit holds, square feet whatever the project displays. The identical area
        /// check compares this rather than the rounded metres, because MM-03 and MM-04 read
        /// 12182.05561411 alike to eight decimals and rounding would hide a narrower match.
        /// </summary>
        public double RawSquareFeet { get; }

        public double SquareMetres { get; }

        public string Printed { get; }

        public bool HoldsAnArea
        {
            get { return SquareMetres > 0.0; }
        }
    }

    /// <summary>
    /// One group's value off a schedule that prints in groups, exactly as the row read. The
    /// workbook wants the two group values of SHRUBS AND LAWN and never the schedule's TOTAL.
    ///
    /// **A GROUP PRINTS ONE SUBTOTAL PER PHASE, THEN THE GROUP TOTAL.** Measured on the 0928
    /// run over 20 mosque plots. A group holding Existing and Proposed planting prints three
    /// rows and the third is the group. A group holding one phase prints two equal rows, which
    /// is why DM-11 looked like a doubled subtotal and why the old rule, take the first of two,
    /// was right on that one plot and wrong everywhere else.
    ///
    /// The last row is the value. The check is that it equals the rows above it added together,
    /// in area and in item count, and a last row that does not is a real disagreement carried
    /// here that refuses the write.
    /// </summary>
    public sealed class GroupSubtotal
    {
        public GroupSubtotal(
            string heading,
            double squareMetres,
            int itemCount,
            int repeats = 1,
            double speciesSum = double.NaN,
            string disagreement = null,
            int rowNumber = 0,
            IEnumerable<int> rowsConsidered = null)
        {
            if (heading == null) throw new ArgumentNullException("heading");
            if (repeats < 0) throw new ArgumentOutOfRangeException("repeats");
            if (rowNumber < 0) throw new ArgumentOutOfRangeException("rowNumber");

            Heading = heading;
            SquareMetres = squareMetres;
            ItemCount = itemCount;
            Repeats = repeats;
            SpeciesSum = speciesSum;
            Disagreement = disagreement ?? string.Empty;
            RowNumber = rowNumber;
            RowsConsidered = (rowsConsidered ?? Enumerable.Empty<int>()).ToList();
        }

        /// <summary>
        /// The printed row the value was taken off, counting the heading row as 1, so the
        /// report can say which subtotal row was taken. Nought when the value came from no
        /// schedule.
        /// </summary>
        public int RowNumber { get; }

        /// <summary>
        /// Every subtotal row the group printed, in order. The last is the one taken and the
        /// ones before it are the phase subtotals that add to it.
        /// </summary>
        public IReadOnlyList<int> RowsConsidered { get; }

        public string Heading { get; }

        public double SquareMetres { get; }

        public int ItemCount { get; }

        /// <summary>
        /// How many subtotal rows the group printed, which is one per phase plus the group
        /// total. Three for a group holding Existing and Proposed, two for a group holding one
        /// phase. It used to be read as how many times one subtotal repeated.
        /// </summary>
        public int Repeats { get; }

        /// <summary>
        /// The species rows of the group added up, or NaN when the schedule named no botanical
        /// column to find them in. DM-11's shrubs are 36 and 34 against a subtotal of 70, so
        /// the two can be held against each other. It is printed rather than enforced, because
        /// every one of those numbers is already rounded to the metre on the way out of Revit
        /// and a sum of rounded numbers need not equal a rounded sum.
        /// </summary>
        public double SpeciesSum { get; }

        /// <summary>
        /// Empty unless the group total row disagreed with the phase subtotals above it.
        /// </summary>
        public string Disagreement { get; }

        public bool Agrees
        {
            get { return Disagreement.Length == 0; }
        }
    }

    /// <summary>
    /// Everything one chosen plot contributed, read once, in plain values. The Revit side
    /// builds one of these per plot and Core merges them afterwards. Nothing is ever read
    /// across plots in one go, because every schedule and every filled region in this model is
    /// per plot and filtered on PRX_Ref Plot ID.
    ///
    /// A read that did not happen is a plot with no schedule of that kind rather than an absent
    /// number, so a plot with no softscape schedule is a named plot in the reconciliation rather
    /// than a zero nobody notices.
    ///
    /// **THE SCHEDULES OF EACH KIND ARE NAMED, AND THERE IS ONE OF EACH OR THE KIND IS NOT
    /// READ.** FM-05 holds two schedules whose names hold SOFTSCAPE. The reader appended both
    /// and the merge added them by name and group, so that plot's trees were counted twice and
    /// ALBIZIA LEBBECK proposed read 170 across twenty plots where the truth is nearer 160. The
    /// shrubs and lawn read had the other half of the same fault, taking the first group with
    /// the heading and dropping the second schedule in silence. The tool cannot know which of
    /// two is the real one, so a plot holding two of a kind carries their names and no numbers
    /// off either, and the reconciliation refuses the write naming all of them.
    /// </summary>
    public sealed class PlotReading
    {
        public PlotReading(
            string plotId,
            string component,
            string reference,
            IEnumerable<string> softscapeSchedules,
            IEnumerable<SpeciesRow> species,
            IEnumerable<string> shrubsAndLawnSchedules,
            IEnumerable<GroupSubtotal> subtotals,
            IEnumerable<RegionArea> regions,
            double readSeconds,
            string chosenRegionTypeName = null,
            IEnumerable<string> notes = null,
            IEnumerable<string> readRefusals = null,
            bool softscapeTotalRead = false,
            int softscapeTotal = 0,
            int softscapeRowsPassedOver = 0,
            IEnumerable<ScannedSchedule> printedSchedules = null,
            int softscapeTotalRow = 0)
        {
            if (plotId == null) throw new ArgumentNullException("plotId");
            if (softscapeRowsPassedOver < 0) throw new ArgumentOutOfRangeException("softscapeRowsPassedOver");
            if (softscapeTotalRow < 0) throw new ArgumentOutOfRangeException("softscapeTotalRow");

            PlotId = plotId;
            PrintedSchedules = Held(printedSchedules);
            SoftscapeTotalRow = softscapeTotalRow;
            ReadRefusals = Held(readRefusals).Where(one => !string.IsNullOrWhiteSpace(one)).ToList();
            SoftscapeTotalRead = softscapeTotalRead;
            SoftscapeTotal = softscapeTotal;
            SoftscapeRowsPassedOver = softscapeRowsPassedOver;
            ReadSeconds = readSeconds;
            Component = component ?? string.Empty;
            Reference = reference ?? string.Empty;
            SoftscapeSchedules = Held(softscapeSchedules).Where(one => !string.IsNullOrWhiteSpace(one)).ToList();
            Species = Held(species);
            ShrubsAndLawnSchedules = Held(shrubsAndLawnSchedules).Where(one => !string.IsNullOrWhiteSpace(one)).ToList();
            Subtotals = Held(subtotals);

            // Numbers off two schedules of one kind are the doubled count, and numbers off none
            // came from nowhere. Neither is a reading this can hold.
            if (Species.Count > 0 && SoftscapeSchedules.Count != 1)
            {
                throw new ArgumentException(
                    plotId + " carries " + Species.Count + " species rows and " + SoftscapeSchedules.Count
                    + " softscape schedules. Species rows come off exactly one.", "species");
            }

            if (Subtotals.Count > 0 && ShrubsAndLawnSchedules.Count != 1)
            {
                throw new ArgumentException(
                    plotId + " carries " + Subtotals.Count + " subtotals and " + ShrubsAndLawnSchedules.Count
                    + " shrubs and lawn schedules. Subtotals come off exactly one.", "subtotals");
            }

            Regions = Held(regions);
            ChosenRegionTypeName = chosenRegionTypeName ?? string.Empty;
            Notes = Held(notes);
        }

        public string PlotId { get; }

        public string Component { get; }

        public string Reference { get; }

        /// <summary>
        /// The name of every schedule filtered on this plot whose name holds SOFTSCAPE. One is
        /// the schedule the species came off. None is a plot without one. Two or more is a plot
        /// nothing was read from, because nothing says which is the real one.
        /// </summary>
        public IReadOnlyList<string> SoftscapeSchedules { get; }

        public bool SoftscapeRead
        {
            get { return SoftscapeSchedules.Count == 1; }
        }

        public bool MoreThanOneSoftscape
        {
            get { return SoftscapeSchedules.Count > 1; }
        }

        public IReadOnlyList<SpeciesRow> Species { get; }

        /// <summary>
        /// The same for the schedules whose names hold SHRUB or LAWN.
        /// </summary>
        public IReadOnlyList<string> ShrubsAndLawnSchedules { get; }

        public bool ShrubsAndLawnRead
        {
            get { return ShrubsAndLawnSchedules.Count == 1; }
        }

        public bool MoreThanOneShrubsAndLawn
        {
            get { return ShrubsAndLawnSchedules.Count > 1; }
        }

        public IReadOnlyList<GroupSubtotal> Subtotals { get; }

        /// <summary>
        /// Every filled region carrying this plot, whatever its area. The chosen one is named
        /// separately so the report can trace a wrong number back to the choice that made it.
        /// </summary>
        public IReadOnlyList<RegionArea> Regions { get; }

        public string ChosenRegionTypeName { get; }

        /// <summary>
        /// How long this plot took to read, its schedules and its filled regions together.
        ///
        /// **It is per plot because the cost is per plot.** The 0928 run over 20 plots took
        /// about five minutes, which is fifteen seconds a plot, and the STREETS button ticks 78.
        /// A number beside each plot is what says whether they all cost the same or one of them
        /// carries the run.
        /// </summary>
        public double ReadSeconds { get; }

        public IReadOnlyList<string> Notes { get; }

        /// <summary>
        /// Every reason a schedule read on this plot was refused: a column the heading row did
        /// not name, a cell holding a digit past where its number ends, a species row with no
        /// whole count. Each one refuses the write and prints on the pane and in the report.
        /// **A refused read is never a zero**, which is what a fallback to a cell position was.
        /// </summary>
        public IReadOnlyList<string> ReadRefusals { get; }

        /// <summary>
        /// True when the softscape schedule printed a TOTAL row and its count was read whole.
        /// </summary>
        public bool SoftscapeTotalRead { get; }

        public int SoftscapeTotal { get; }

        /// <summary>
        /// Rows of the softscape schedule carrying a count and no botanical name, the subtotals,
        /// counted rather than dropped in silence.
        /// </summary>
        public int SoftscapeRowsPassedOver { get; }

        /// <summary>
        /// The printed row the TOTAL was read off, counting the heading row as 1. Nought when no
        /// TOTAL row was found.
        /// </summary>
        public int SoftscapeTotalRow { get; }

        /// <summary>
        /// Every schedule this plot's numbers were read off, row for row as the schedule prints
        /// it, so the report can show what was read and not only what was concluded from it.
        /// Nothing in the report was what the tool read until this, and a plot counted twice
        /// survived two rounds because nothing showed the rows.
        /// </summary>
        public IReadOnlyList<ScannedSchedule> PrintedSchedules { get; }

        /// <summary>
        /// Every species the softscape schedule printed on more than one row under one group,
        /// with those rows. FM-05 prints ALBIZIA LEBBECK on two rows under Proposed, 10 and
        /// 10, BAUHINIA PURPUREA 19 and 20, CASSIA GLAUCA 3 and 4. Nothing in the schedule says
        /// whether that is two types of one species or one counted twice, so the accounting
        /// refuses on it and names the rows.
        /// </summary>
        public IReadOnlyList<RepeatedSpecies> SpeciesPrintedOnMoreThanOneRow
        {
            get
            {
                return Species
                    .Where(one => one.HasGroup)
                    .GroupBy(one => one.GroupName.Trim().ToUpperInvariant() + " | " + one.BotanicalName.Trim().ToUpperInvariant())
                    .Where(group => group.Count() > 1)
                    .Select(group => new RepeatedSpecies(group.First().BotanicalName, group.First().GroupName, group))
                    .ToList();
            }
        }

        /// <summary>
        /// The species rows added up, which is adding printed numbers and allowed. The
        /// reconciliation holds it against <see cref="SoftscapeTotal"/>, because the first real
        /// workbook read 31 trees where the schedule printed 39 and nothing could say so.
        /// </summary>
        public int SpeciesSum
        {
            get { return Species.Sum(one => one.Quantity); }
        }

        public RegionArea ChosenRegion
        {
            get
            {
                return Regions.FirstOrDefault(one =>
                    string.Equals(one.TypeName, ChosenRegionTypeName, StringComparison.Ordinal));
            }
        }

        public IReadOnlyList<RegionArea> RegionsHoldingAnArea
        {
            get { return Regions.Where(one => one.HoldsAnArea).ToList(); }
        }

        public GroupSubtotal SubtotalHeaded(string heading)
        {
            if (heading == null) return null;

            return Subtotals.FirstOrDefault(one =>
                string.Equals(one.Heading.Trim(), heading.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static IReadOnlyList<T> Held<T>(IEnumerable<T> items) where T : class
        {
            if (items == null) return new List<T>();
            return items.Where(item => item != null).ToList();
        }
    }
}
