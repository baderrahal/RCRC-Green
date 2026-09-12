using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Which group rows count. **Only the groups a tree list sheet is named for**, which on the
    /// map are Tree List - Existing and Tree List - Proposed, and the words Existing and Proposed
    /// are still written nowhere in the code: they come off the sheet names. Every other group
    /// row is out of scope and its rows are named and left out.
    ///
    /// **Measured on the 1536 report.** FM-05's softscape schedule prints three groups, Existing
    /// 6, Proposed 32 and Street Design 38, TOTAL 76, and its shrubs and lawn schedule prints
    /// GRASS as Proposed 96 and Street Design 69 and SHRUBS as Proposed 361 and Street Design
    /// 459. The tool did not know Street Design as a group row, so its 38 trees carried on under
    /// Proposed in silence, and the group totals 165 and 820 were taken whole. Bader has decided
    /// Street Design is somebody else's scope and does not belong on this plot's checklist. The
    /// model will be corrected later, and until it is the tool leaves those rows out and says so.
    /// </summary>
    public sealed class CountedGroups
    {
        private CountedGroups(IEnumerable<string> sheetNames, IEnumerable<GroupByDecision> byDecision, string templateName)
        {
            SheetNames = (sheetNames ?? Enumerable.Empty<string>()).Where(one => !string.IsNullOrWhiteSpace(one)).ToList();
            ByDecision = (byDecision ?? Enumerable.Empty<GroupByDecision>()).Where(one => one != null).ToList();
            TemplateName = templateName ?? string.Empty;
        }

        public IReadOnlyList<string> SheetNames { get; }

        /// <summary>
        /// Groups a sheet takes by decision rather than by its name: Street Design into Tree
        /// List - Proposed on STREETS and nowhere else. Read off the template, so the plot
        /// prefix decides nothing here.
        /// </summary>
        public IReadOnlyList<GroupByDecision> ByDecision { get; }

        public string TemplateName { get; }

        public static CountedGroups Of(KpiTemplate template)
        {
            if (template == null) throw new ArgumentNullException("template");

            return new CountedGroups(
                new[] { template.ExistingTrees.SheetName, template.ProposedTrees.SheetName },
                template.GroupsCountedAsProposed.Select(one => new GroupByDecision(one, template.ProposedTrees.SheetName)),
                template.Name);
        }

        public static CountedGroups Named(params string[] sheetNames)
        {
            return new CountedGroups(sheetNames, null, null);
        }

        public bool Counts(string groupName)
        {
            return SheetFor(groupName) != null;
        }

        /// <summary>
        /// Tree List - Existing is named for it, or Tree List - Proposed takes it on STREETS
        /// by decision, or no tree list sheet is named for it, so it is out of scope. The
        /// report prints this beside every group row, so a Street Design group counted on
        /// STREETS reads differently from one left out on MOSQUES.
        /// </summary>
        public string Why(string groupName)
        {
            string named = SheetNamedFor(groupName);
            if (named != null) return named + " is named for it";

            GroupByDecision decided = DecisionFor(groupName);
            if (decided != null)
            {
                return decided.SheetName + " takes it on " + TemplateName + " by decision, as that plot's own work";
            }

            return LeftOut;
        }

        public const string LeftOut = "no tree list sheet is named for it, so it is out of scope";

        /// <summary>
        /// The tree list sheet a group's rows go to, or null when none takes them. A sheet
        /// named for the group comes first, then a sheet that takes it by decision.
        /// </summary>
        public string SheetFor(string groupName)
        {
            string named = SheetNamedFor(groupName);
            if (named != null) return named;

            GroupByDecision decided = DecisionFor(groupName);
            return decided == null ? null : decided.SheetName;
        }

        private GroupByDecision DecisionFor(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName)) return null;

            return ByDecision.FirstOrDefault(one => string.Equals(one.GroupName, groupName.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// A sheet is named for a group when its name ends in the group's name, word for word
        /// and without case: Tree List - Existing ends in Existing. Nothing looser, because a
        /// group called Tree or List is not what either sheet is for.
        /// </summary>
        private string SheetNamedFor(string groupName)
        {
            List<string> wanted = Words(groupName);
            if (wanted.Count == 0) return null;

            return SheetNames.FirstOrDefault(one => EndsWith(Words(one), wanted));
        }

        private static bool EndsWith(List<string> words, List<string> tail)
        {
            if (tail.Count > words.Count) return false;

            for (int at = 0; at < tail.Count; at++)
            {
                string word = words[words.Count - tail.Count + at];
                if (!string.Equals(word, tail[at], StringComparison.OrdinalIgnoreCase)) return false;
            }

            return true;
        }

        private static List<string> Words(string name)
        {
            var words = new List<string>();
            if (string.IsNullOrEmpty(name)) return words;

            var run = new System.Text.StringBuilder();
            foreach (char letter in name)
            {
                if (char.IsLetterOrDigit(letter))
                {
                    run.Append(letter);
                    continue;
                }

                if (run.Length > 0) words.Add(run.ToString());
                run.Length = 0;
            }

            if (run.Length > 0) words.Add(run.ToString());
            return words;
        }
    }

    /// <summary>
    /// One group a sheet takes by decision. The name is the group as the schedule prints it
    /// and the sheet is the one its rows go to.
    /// </summary>
    public sealed class GroupByDecision
    {
        public GroupByDecision(string groupName, string sheetName)
        {
            if (string.IsNullOrWhiteSpace(groupName)) throw new ArgumentException("A group by decision needs a name.", "groupName");
            if (string.IsNullOrWhiteSpace(sheetName)) throw new ArgumentException("A group by decision needs a sheet.", "sheetName");

            GroupName = groupName.Trim();
            SheetName = sheetName;
        }

        public string GroupName { get; }

        public string SheetName { get; }
    }

    /// <summary>
    /// One group row of a softscape schedule as printed, with the species rows under it, its
    /// own subtotal row, and whether it was taken or left out and why.
    /// </summary>
    public sealed class PrintedGroup
    {
        public PrintedGroup(
            string name,
            int rowNumber,
            IEnumerable<SpeciesRow> species,
            bool subtotalPrinted,
            int subtotal,
            int subtotalRow,
            bool counted,
            string why)
        {
            if (name == null) throw new ArgumentNullException("name");

            Name = name;
            RowNumber = rowNumber;
            Species = (species ?? Enumerable.Empty<SpeciesRow>()).Where(one => one != null).ToList();
            SubtotalPrinted = subtotalPrinted;
            Subtotal = subtotalPrinted ? subtotal : 0;
            SubtotalRow = subtotalPrinted ? subtotalRow : 0;
            Counted = counted;
            Why = why ?? string.Empty;
        }

        public string Name { get; }

        /// <summary>
        /// The printed row of the group row, counting the heading row as 1.
        /// </summary>
        public int RowNumber { get; }

        public IReadOnlyList<SpeciesRow> Species { get; }

        public int SpeciesSum
        {
            get { return Species.Sum(one => one.Quantity); }
        }

        public bool SubtotalPrinted { get; }

        public int Subtotal { get; }

        public int SubtotalRow { get; }

        public bool Counted { get; }

        public string Why { get; }

        /// <summary>
        /// Empty unless the group printed a subtotal that its own species rows do not add to.
        /// </summary>
        public string Disagreement
        {
            get
            {
                if (!SubtotalPrinted || Subtotal == SpeciesSum) return string.Empty;

                return "its species rows add to " + SpeciesSum + " and its subtotal row " + SubtotalRow + " prints " + Subtotal;
            }
        }
    }

    /// <summary>
    /// One phase subtotal row inside a group of the shrubs and lawn schedule, and whether it was
    /// taken or left out and why.
    /// </summary>
    public sealed class PhaseSubtotal
    {
        public PhaseSubtotal(string name, int rowNumber, double squareMetres, int itemCount, bool counted, string why)
        {
            Name = name ?? string.Empty;
            RowNumber = rowNumber;
            SquareMetres = squareMetres;
            ItemCount = itemCount;
            Counted = counted;
            Why = why ?? string.Empty;
        }

        public string Name { get; }

        public int RowNumber { get; }

        public double SquareMetres { get; }

        public int ItemCount { get; }

        public bool Counted { get; }

        public string Why { get; }
    }

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
        /// rows 12 and 13, counting 10 and 10. When the rows sit under two group rows carrying
        /// one name, which DM-25 prints, those rows are named too, because that is a different
        /// thing from one group printing a species twice.
        /// </summary>
        public string InWords
        {
            get
            {
                List<int> groupRows = Rows.Select(one => one.GroupRowNumber).Where(one => one > 0).Distinct().OrderBy(one => one).ToList();
                string under = groupRows.Count > 1
                    ? ", under " + groupRows.Count + " group rows " + (groupRows.Count == 2 ? "both" : "all") + " named " + GroupName + ", rows "
                        + Joined(groupRows.Select(one => one.ToString(CultureInfo.InvariantCulture)))
                    : string.Empty;

                return "rows " + Joined(Rows.Select(one => one.RowNumber.ToString(CultureInfo.InvariantCulture)))
                    + ", counting " + Joined(Rows.Select(one => one.Quantity.ToString(CultureInfo.InvariantCulture))) + under;
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
            PrintedMeasure diameter = null,
            int groupRowNumber = 0)
        {
            if (botanicalName == null) throw new ArgumentNullException("botanicalName");
            if (quantity < 0) throw new ArgumentOutOfRangeException("quantity");
            if (rowNumber < 0) throw new ArgumentOutOfRangeException("rowNumber");
            if (groupRowNumber < 0) throw new ArgumentOutOfRangeException("groupRowNumber");

            BotanicalName = botanicalName;
            GroupName = groupName ?? string.Empty;
            Quantity = quantity;
            PlotId = plotId ?? string.Empty;
            RowNumber = rowNumber;
            Height = height ?? PrintedMeasure.NotRead;
            Diameter = diameter ?? PrintedMeasure.NotRead;
            GroupRowNumber = groupRowNumber;
        }

        /// <summary>
        /// The same row with its plot named, for a row that travels out of its reading.
        /// </summary>
        public SpeciesRow OnPlot(string plotId)
        {
            return new SpeciesRow(BotanicalName, GroupName, Quantity, plotId, RowNumber, Height, Diameter, GroupRowNumber);
        }

        /// <summary>
        /// The printed row of the group row this species sat under, or nought.
        /// </summary>
        public int GroupRowNumber { get; }

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
            IEnumerable<int> rowsConsidered = null,
            IEnumerable<PhaseSubtotal> phases = null,
            bool groupTotalPrinted = false,
            double groupTotalSquareMetres = 0.0,
            int groupTotalItemCount = 0,
            string roundingNote = null)
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
            Phases = (phases ?? Enumerable.Empty<PhaseSubtotal>()).Where(one => one != null).ToList();
            GroupTotalPrinted = groupTotalPrinted;
            GroupTotalSquareMetres = groupTotalPrinted ? groupTotalSquareMetres : 0.0;
            GroupTotalItemCount = groupTotalPrinted ? groupTotalItemCount : 0;
            RoundingNote = roundingNote ?? string.Empty;
        }

        /// <summary>
        /// The rows above the group total add to within the project's own rounding of it, with
        /// every count exact: said in full, or empty. FM-21 prints Existing 2 over 0, Proposed
        /// 51 over 11 and a group total of 52 over 11, and 2 plus 51 is 53, because every
        /// printed area is already rounded and a sum of rounded numbers need not equal a
        /// rounded sum. It is a line in the report rather than a refusal, so a real fault
        /// growing slowly stays visible while correct data goes through.
        /// </summary>
        public string RoundingNote { get; }

        /// <summary>
        /// The printed row of the group total, counting the heading row as 1, or of the one row
        /// the value was taken off where the group printed no phase rows. Nought when the value
        /// came from no schedule.
        /// </summary>
        public int RowNumber { get; }

        /// <summary>
        /// Every subtotal row the group printed, in order.
        /// </summary>
        public IReadOnlyList<int> RowsConsidered { get; }

        /// <summary>
        /// One per phase row the group printed, each taken or left out. **The value is the
        /// phases taken added together and never the group total**, because on FM-05 the group
        /// total holds Street Design, which is out of scope, and 459 of its 820 shrubs are that.
        /// </summary>
        public IReadOnlyList<PhaseSubtotal> Phases { get; }

        public bool GroupTotalPrinted { get; }

        public double GroupTotalSquareMetres { get; }

        public int GroupTotalItemCount { get; }

        public IReadOnlyList<PhaseSubtotal> PhasesLeftOut
        {
            get { return Phases.Where(one => !one.Counted).ToList(); }
        }

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
            int softscapeTotalRow = 0,
            IEnumerable<PrintedGroup> printedGroups = null)
        {
            if (plotId == null) throw new ArgumentNullException("plotId");
            if (softscapeRowsPassedOver < 0) throw new ArgumentOutOfRangeException("softscapeRowsPassedOver");
            if (softscapeTotalRow < 0) throw new ArgumentOutOfRangeException("softscapeTotalRow");

            PlotId = plotId;
            PrintedSchedules = Held(printedSchedules);
            SoftscapeTotalRow = softscapeTotalRow;
            PrintedGroups = Held(printedGroups);
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

        public const string ReadThrew = "the read threw and nothing on this plot was read.";

        /// <summary>
        /// A plot whose read threw. The refusal travels on the reading with what was thrown,
        /// the same as a column the heading row did not name, and never as a plot missing
        /// from the list: a throw on plot 60 of 78 used to end the run with one sentence
        /// naming no plot and no report, and twenty minutes that end with a report naming
        /// the bad plot are worth something. Every other field stays at its empty default, so
        /// the reading reads as not read and the reconciliation refuses the write.
        /// </summary>
        public static PlotReading NotRead(string plotId, string why, double readSeconds)
        {
            if (string.IsNullOrWhiteSpace(why)) throw new ArgumentException("A read that threw needs what was thrown.", "why");

            return new PlotReading(plotId, string.Empty, string.Empty, null, null, null, null, null, readSeconds,
                null, null, new[] { ReadThrew + " " + why.Trim() });
        }

        /// <summary>
        /// The same reading with the region the user chose, so a choice made after a refusal
        /// is applied to what was already read rather than read again. Every argument is this
        /// reading's own, and the two counts the constructor checks are unchanged.
        /// </summary>
        public PlotReading WithChosenRegion(string typeName)
        {
            return new PlotReading(PlotId, Component, Reference, SoftscapeSchedules, Species, ShrubsAndLawnSchedules,
                Subtotals, Regions, ReadSeconds, typeName, Notes, ReadRefusals, SoftscapeTotalRead, SoftscapeTotal,
                SoftscapeRowsPassedOver, PrintedSchedules, SoftscapeTotalRow, PrintedGroups);
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
        /// Every group row the softscape schedule printed, in order, taken or left out. A plot
        /// whose schedule prints only Existing and Proposed lists those two, so a schedule with
        /// the ordinary two reads differently from one nobody has looked at.
        /// </summary>
        public IReadOnlyList<PrintedGroup> PrintedGroups { get; }

        /// <summary>
        /// The species rows under the groups left out, named with their counts and never
        /// counted. FM-05: ALBIZIA LEBBECK 10, BAUHINIA PURPUREA 20, CASSIA GLAUCA 4 and
        /// CONOCARPUS LANCIFOLIUS 4 under Street Design, 38 trees.
        /// </summary>
        public IReadOnlyList<SpeciesRow> LeftOutSpecies
        {
            get { return PrintedGroups.Where(one => !one.Counted).SelectMany(one => one.Species).ToList(); }
        }

        public int LeftOutSum
        {
            get { return LeftOutSpecies.Sum(one => one.Quantity); }
        }

        /// <summary>
        /// True when either schedule on this plot printed a group no tree list sheet is named
        /// for, so the accounting can count the schedules and name the plots.
        /// </summary>
        public bool HoldsAGroupLeftOut
        {
            get
            {
                return PrintedGroups.Any(one => !one.Counted)
                    || Subtotals.Any(one => one.Phases.Any(phase => !phase.Counted));
            }
        }

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
