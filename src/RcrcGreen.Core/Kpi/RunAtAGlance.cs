using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// How many street plots really got a number in their area cell, and which did not.
    ///
    /// **That cell was empty in the client's template until the seventy third pass.** H8 was
    /// <c>=Width*F8</c> and the client emptied it, so the tool writes it now and this is the
    /// first thing anybody wants to know off a street run. It is counted for STREETS alone
    /// because STREETS is the template whose cell changed.
    /// </summary>
    public sealed class AreaCellGlance
    {
        public AreaCellGlance(int written, IEnumerable<string> without)
        {
            Written = written;
            Without = (without ?? Enumerable.Empty<string>()).ToList();
        }

        /// <summary>
        /// No street plot was in this press at all, which is different from every street plot
        /// failing to write one.
        /// </summary>
        public static readonly AreaCellGlance NoStreetPlots =
            new AreaCellGlance(0, new List<string>());

        public int Written { get; }

        /// <summary>
        /// One line per street plot whose area cell holds nothing, the plot and the reason. **A
        /// count with nobody named sends a person back through 78 per plot blocks**, which is
        /// the whole thing this section exists to save.
        /// </summary>
        public IReadOnlyList<string> Without { get; }

        public int Plots
        {
            get { return Written + Without.Count; }
        }

        public string InWords
        {
            get
            {
                if (Plots == 0)
                {
                    return "THE STREETS AREA: no street plot was in this press, so nothing was "
                        + "written into a streets area cell and nothing about it was refused.";
                }

                return "THE STREETS AREA: " + Written + " of " + Plots
                    + (Plots == 1 ? " street plot" : " street plots")
                    + " got a value in the area cell and " + Without.Count + " did not."
                    + (Without.Count == 0
                        ? string.Empty
                        : " The ones that did not are named under this line.");
            }
        }
    }

    /// <summary>
    /// How many plots took their area off one filled region type. One row per type the run
    /// really found.
    /// </summary>
    public sealed class RegionTypeCount
    {
        public RegionTypeCount(string typeName, int plots)
        {
            TypeName = typeName ?? string.Empty;
            Plots = plots;
        }

        public string TypeName { get; }

        public int Plots { get; }

        public bool IsTheTypeTheNoteNames
        {
            get
            {
                return TypeName.Length > 0
                    && string.Equals(TypeName.Trim(), RegionChoice.TheNoteNames,
                        StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    /// <summary>
    /// Which filled region type the whole run's areas came off, as counts rather than as 156
    /// per plot lines.
    ///
    /// **The client's note names one type and at least two street plots disagree.** Nothing in
    /// the tool chooses on a type name, so this is the measurement that says whether the note
    /// holds, read off a run rather than argued about.
    ///
    /// **The types are counted AS THE RUN FOUND THEM and no second type name is written into
    /// the code.** Only <see cref="RegionChoice.TheNoteNames"/> is held here, because it is the
    /// note being checked. Naming RCRC_CADASTRAL LIMIT beside it would put a second type name
    /// in the tool, and a model holding a third would then be counted under nothing.
    /// </summary>
    public sealed class RegionGlance
    {
        public RegionGlance(IEnumerable<RegionTypeCount> types, int nothingChosen)
        {
            Types = (types ?? Enumerable.Empty<RegionTypeCount>()).ToList();
            NothingChosen = nothingChosen;
        }

        public static readonly RegionGlance NoAreaRead =
            new RegionGlance(new List<RegionTypeCount>(), 0);

        /// <summary>
        /// One per type, the type the note names first and the rest by how many plots took
        /// their area off each, so the two counts the team asks for are the top of the list.
        /// </summary>
        public IReadOnlyList<RegionTypeCount> Types { get; }

        /// <summary>
        /// Plots wanting an area that chose no region at all, which is a plot with none holding
        /// one or two holding one. It is counted apart from every type, because a plot that
        /// chose nothing took its area off nothing and is not a disagreement with the note.
        /// </summary>
        public int NothingChosen { get; }

        public int OffTheTypeTheNoteNames
        {
            get { return Types.Where(one => one.IsTheTypeTheNoteNames).Sum(one => one.Plots); }
        }

        public int OffATypeTheNoteDoesNotName
        {
            get { return Types.Where(one => !one.IsTheTypeTheNoteNames).Sum(one => one.Plots); }
        }

        public int Plots
        {
            get { return Types.Sum(one => one.Plots) + NothingChosen; }
        }

        public string InWords
        {
            get
            {
                if (Plots == 0)
                {
                    return "THE REGION TYPE: no plot in this press wanted an area, so no filled "
                        + "region was read and there is nothing to count.";
                }

                return "THE REGION TYPE: of " + Plots
                    + (Plots == 1 ? " plot" : " plots") + " wanting an area, "
                    + OffTheTypeTheNoteNames + " took it off " + RegionChoice.TheNoteNames
                    + ", which is the type the client's note names, "
                    + OffATypeTheNoteDoesNotName + " off a type it does not name, and "
                    + NothingChosen + " chose no region at all.";
            }
        }
    }

    /// <summary>
    /// Every #DIV/0! the formula check found across the press, and how many of them divide by a
    /// cell THIS RUN wrote.
    ///
    /// **The second number is the one that matters.** A division by a client cell is the
    /// client's own arithmetic over a real number, and a plot with no trees really has no
    /// average. A division by a cell the tool wrote is the tool's doing.
    /// </summary>
    public sealed class DivisionGlance
    {
        public DivisionGlance(int found, int byACellThisRunWrote, IEnumerable<string> where)
        {
            Found = found;
            ByACellThisRunWrote = byACellThisRunWrote;
            Where = (where ?? Enumerable.Empty<string>()).ToList();
        }

        public static readonly DivisionGlance NoneFound =
            new DivisionGlance(0, 0, new List<string>());

        public int Found { get; }

        public int ByACellThisRunWrote { get; }

        /// <summary>
        /// One line per division, the plot, the sheet, the cell and whether this run wrote the
        /// divisor. The formula itself stays in the per template section.
        /// </summary>
        public IReadOnlyList<string> Where { get; }

        public string InWords
        {
            get
            {
                if (Found == 0)
                {
                    return "THE DIVISIONS: the formula check found no #DIV/0! anywhere in this "
                        + "press.";
                }

                return "THE DIVISIONS: " + Found
                    + (Found == 1 ? " #DIV/0! was found" : " #DIV/0! were found")
                    + " and " + ByACellThisRunWrote + " of them divide by a cell THIS RUN WROTE."
                    + (ByACellThisRunWrote == 0
                        ? " The rest divide by a client cell, which is their arithmetic over a "
                            + "real number and not this tool's doing."
                        : " A division by a cell this run wrote is this tool's doing.");
            }
        }
    }

    /// <summary>
    /// The three questions one press answers, counted once over every template so each can be
    /// read in one look instead of out of hundreds of lines.
    ///
    /// **Every count here comes off the OUTCOME, never off the plan**, which is the shape this
    /// repo settled on after a report named four views as created and as not created in one
    /// file. The area cell counts as written when it landed in the output file and holds
    /// something, read back off the file rather than taken from what the fill set out to write.
    /// </summary>
    public sealed class RunGlance
    {
        public RunGlance(AreaCellGlance streetsArea, RegionGlance regions, DivisionGlance divisions)
        {
            StreetsArea = streetsArea ?? AreaCellGlance.NoStreetPlots;
            Regions = regions ?? RegionGlance.NoAreaRead;
            Divisions = divisions ?? DivisionGlance.NoneFound;
        }

        public AreaCellGlance StreetsArea { get; }

        public RegionGlance Regions { get; }

        public DivisionGlance Divisions { get; }
    }

    public static class RunAtAGlance
    {
        public static RunGlance Of(KpiCreateRunSet set)
        {
            if (set == null) throw new ArgumentNullException("set");

            return new RunGlance(StreetsArea(set), Regions(set), Divisions(set));
        }

        /// <summary>
        /// A street plot got a value when the template's own area cell landed in its output and
        /// holds something. **The cell is taken off the map**, so nothing here names H8, and a
        /// plot whose workbook was refused has no landed cell at all and is named with the
        /// reason its run recorded.
        /// </summary>
        private static AreaCellGlance StreetsArea(KpiCreateRunSet set)
        {
            int written = 0;
            var without = new List<string>();

            foreach (KpiCreateRun run in set.Runs)
            {
                if (!ReferenceEquals(run.Template, KpiTemplates.Streets)) continue;

                string plot = OnePlotOf(run);

                if (AreaCellLanded(run)) written = written + 1;
                else without.Add(plot + ": " + WhyNoAreaCell(run));
            }

            return new AreaCellGlance(written, without);
        }

        private static bool AreaCellLanded(KpiCreateRun run)
        {
            MappedCell cell = run.Template == null ? null : run.Template.CellFor(KpiValue.Area);
            if (cell == null || run.Outcome == null) return false;

            string wanted = CellRef.Parse(cell.Cell).ToString();

            return run.Outcome.Landed.Any(one =>
                string.Equals(one.SheetName, run.Template.MainSheetName, StringComparison.Ordinal)
                && string.Equals(one.Cell, wanted, StringComparison.OrdinalIgnoreCase)
                && one.Value.Trim().Length > 0);
        }

        /// <summary>
        /// The reason, taken from the run rather than written here, so the line under the count
        /// says the same thing as the plot's own block further down.
        /// </summary>
        private static string WhyNoAreaCell(KpiCreateRun run)
        {
            if (run.Template != null && run.Template.TakesNoArea)
            {
                return CreateWords.TakesNoArea;
            }

            if (run.Outcome == null || !run.Outcome.Written)
            {
                return CreateWords.WhyThisOneWroteNothing(run);
            }

            NotWritten skipped = run.Plan == null
                ? null
                : run.Plan.Skipped.FirstOrDefault(one =>
                    string.Equals(one.What, KpiValue.Area.ToString(), StringComparison.Ordinal));

            return skipped == null
                ? "the workbook was written and its area cell was not read back with a value"
                : skipped.Why;
        }

        /// <summary>
        /// The plot a run covers. **A run is one plot** since the round that made a checklist
        /// one plot, so a run holding none or several is a shape nobody has seen and says so
        /// rather than printing the first.
        /// </summary>
        private static string OnePlotOf(KpiCreateRun run)
        {
            if (run.Readings.Count == 1) return run.Readings[0].PlotId;

            return run.Readings.Count == 0
                ? "(a run holding no plot, which is UNKNOWN)"
                : "(a run holding " + run.Readings.Count + " plots, which is UNKNOWN)";
        }

        private static RegionGlance Regions(KpiCreateRunSet set)
        {
            var perType = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var order = new List<string>();
            int nothingChosen = 0;

            foreach (KpiCreateRun run in set.Runs)
            {
                if (!run.Reconciliation.AreaWanted) continue;

                foreach (PlotReading reading in run.Readings)
                {
                    string type = (reading.ChosenRegionTypeName ?? string.Empty).Trim();
                    if (type.Length == 0)
                    {
                        nothingChosen = nothingChosen + 1;
                        continue;
                    }

                    if (!perType.ContainsKey(type))
                    {
                        perType[type] = 0;
                        order.Add(type);
                    }

                    perType[type] = perType[type] + 1;
                }
            }

            // The type the note names first, then the rest by how many plots took their area off
            // each, so the two counts the team asks for are the top of the list whatever a model
            // holds. Ties fall back to the name so one run's order is another's.
            List<RegionTypeCount> counts = order
                .Select(one => new RegionTypeCount(one, perType[one]))
                .OrderByDescending(one => one.IsTheTypeTheNoteNames)
                .ThenByDescending(one => one.Plots)
                .ThenBy(one => one.TypeName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return new RegionGlance(counts, nothingChosen);
        }

        /// <summary>
        /// **The kind and the divisor both travel on the finding**, rather than being read back
        /// out of the sentence it prints. A signal that travels in the data is not a signal, and
        /// this repo has paid for that shape once already.
        /// </summary>
        private static DivisionGlance Divisions(KpiCreateRunSet set)
        {
            int found = 0;
            int ours = 0;
            var where = new List<string>();

            foreach (KpiCreateRun run in set.Runs)
            {
                if (run.Outcome == null) continue;

                string plot = OnePlotOf(run);

                foreach (FormulaAtRisk risk in run.Outcome.Formulas.AtRisk)
                {
                    if (!risk.IsDivideByZero) continue;

                    found = found + 1;
                    if (risk.DivisorThisRunWrote) ours = ours + 1;

                    where.Add(plot + " | " + risk.Where + " | "
                        + (risk.DivisorThisRunWrote
                            ? "divides by a cell THIS RUN WROTE"
                            : "divides by a client cell"));
                }
            }

            return new DivisionGlance(found, ours, where);
        }
    }
}
