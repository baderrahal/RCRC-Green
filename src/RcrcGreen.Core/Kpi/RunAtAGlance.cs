using System;
using System.Collections.Generic;
using System.Globalization;
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
                    + ", which is the type " + RegionChoice.TheNote + " names, "
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
        public DivisionGlance(
            int found, int byACellThisRunWrote, IEnumerable<string> where,
            IEnumerable<string> notEvaluated = null)
        {
            Found = found;
            ByACellThisRunWrote = byACellThisRunWrote;
            Where = (where ?? Enumerable.Empty<string>()).ToList();
            NotEvaluated = (notEvaluated ?? Enumerable.Empty<string>()).ToList();
        }

        public static readonly DivisionGlance NoneFound =
            new DivisionGlance(0, 0, new List<string>());

        /// <summary>
        /// **AT MOST THIS MANY CELLS ARE NAMED UNDER THE LINE.** The 21:38 press held 284
        /// divisions it could not work out, S70 and T70 on both tree lists of every one of its
        /// 71 written plots, and the glance printed all 284. A glance is a glance: the count is
        /// the fact and the full list is in each plot's own block.
        /// </summary>
        public const int Named = 5;

        /// <summary>
        /// Every division the check LOOKED AT and could not work out, the plot and the cell.
        /// **A division nobody evaluated is not a division that is fine**, and a line saying
        /// none was found over a press that could evaluate none of them is the fault the 16:37
        /// glance shipped.
        /// </summary>
        public IReadOnlyList<string> NotEvaluated { get; }

        /// <summary>
        /// The ones the glance prints, at most <see cref="Named"/> of them.
        /// </summary>
        public IReadOnlyList<string> NotEvaluatedNamed
        {
            get { return NotEvaluated.Take(Named).ToList(); }
        }

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
                        + "press." + Unevaluated;
                }

                return "THE DIVISIONS: " + Found
                    + (Found == 1 ? " #DIV/0! was found" : " #DIV/0! were found")
                    + " and " + ByACellThisRunWrote + " of them divide by a cell THIS RUN WROTE."
                    + (ByACellThisRunWrote == 0
                        ? " The rest divide by a cell the template already held, which is its "
                            + "own arithmetic over a "
                            + "real number and not this tool's doing."
                        : " A division by a cell this run wrote is this tool's doing.")
                    + Unevaluated;
            }
        }

        /// <summary>
        /// The one sentence about what could not be worked out, left off a press where every
        /// division was evaluated, so a run with nothing to say about them does not carry a
        /// count of 0.
        /// </summary>
        private string Unevaluated
        {
            get
            {
                if (NotEvaluated.Count == 0) return string.Empty;

                return " " + NotEvaluated.Count
                    + (NotEvaluated.Count == 1 ? " division was" : " divisions were")
                    + " looked at and could not be worked out, so nothing here says whether "
                    + (NotEvaluated.Count == 1 ? "it is" : "they are") + " a #DIV/0!. "
                    + (NotEvaluated.Count > Named
                        ? "The first " + Named + " are named under this line"
                        : (NotEvaluated.Count == 1 ? "It is named" : "They are named")
                            + " under this line")
                    + " and every one of them is in its own plot's block below.";
            }
        }
    }

    /// <summary>
    /// How many PDFs the press wrote, how many plots got a workbook and no PDF, and how many
    /// forms did not match what the tool knows.
    ///
    /// **The third count is the one that would otherwise go unnoticed.** A client reissuing a
    /// form with its field names moved writes nothing into it and says so per plot, and on 78
    /// street plots that is 78 lines nobody reads. One number at the top says it once.
    /// </summary>
    public sealed class PdfGlance
    {
        public PdfGlance(
            int written, IEnumerable<string> withNoPdf, IEnumerable<string> formsThatDidNotMatch,
            int noughtBoxes = 0, IEnumerable<string> noughtPlots = null)
        {
            Written = written;
            WithNoPdf = (withNoPdf ?? Enumerable.Empty<string>()).ToList();
            FormsThatDidNotMatch = (formsThatDidNotMatch ?? Enumerable.Empty<string>()).ToList();
            NoughtBoxes = noughtBoxes;
            NoughtPlots = (noughtPlots ?? Enumerable.Empty<string>()).ToList();
        }

        public static readonly PdfGlance NonePlanned = new PdfGlance(0, null, null);

        /// <summary>
        /// How many boxes this press wrote 0 into because the plot's shrubs and lawn schedule
        /// was read and printed no such group. **Bader's decision of 15 September turned those
        /// blanks into noughts**, and a number the tool decided rather than read is one the run
        /// has to count out loud.
        /// </summary>
        public int NoughtBoxes { get; }

        /// <summary>The plots those boxes are on, each named once however many of its boxes.</summary>
        public IReadOnlyList<string> NoughtPlots { get; }

        public int Written { get; }

        /// <summary>
        /// One line per plot whose workbook was written and whose PDF was not, the plot and the
        /// reason. **A count with nobody named sends a person back through the file.**
        /// </summary>
        public IReadOnlyList<string> WithNoPdf { get; }

        /// <summary>
        /// One line per form the tool read and did not recognise, said once per form rather than
        /// once per plot.
        /// </summary>
        public IReadOnlyList<string> FormsThatDidNotMatch { get; }

        public string InWords
        {
            get
            {
                if (Written == 0 && WithNoPdf.Count == 0)
                {
                    return "THE PDFS: no PDF was planned in this press, so none was written and "
                        + "nothing about a form was checked.";
                }

                return "THE PDFS: " + Written
                    + (Written == 1 ? " PDF was written" : " PDFs were written") + ", "
                    + WithNoPdf.Count + " of the plots that got a workbook got no PDF, and "
                    + FormsThatDidNotMatch.Count
                    + (FormsThatDidNotMatch.Count == 1 ? " form did not match" : " forms did not match")
                    + " what this tool knows." + Noughts;
            }
        }

        /// <summary>
        /// The one sentence about the noughts, left off a press that wrote none so a run with
        /// nothing to say about them does not carry a nought count of 0.
        /// </summary>
        private string Noughts
        {
            get
            {
                if (NoughtBoxes == 0) return string.Empty;

                return " " + NoughtBoxes + (NoughtBoxes == 1 ? " box was" : " boxes were")
                    + " written 0 on " + NoughtPlots.Count
                    + (NoughtPlots.Count == 1 ? " plot" : " plots")
                    + " whose shrubs and lawn schedule was read and printed no such group.";
            }
        }
    }

    /// <summary>
    /// One reason a computed number was not written, with every plot it happened to.
    ///
    /// **A COUNT WITH NOBODY NAMED SENDS A PERSON BACK THROUGH THE FILE**, and one line per plot
    /// over 150 plots is the thing this section exists to save. So the reason is said once with
    /// its count and the first few plots named.
    /// </summary>
    public sealed class BlankedFor
    {
        public BlankedFor(string why, IEnumerable<string> plots)
        {
            Why = why ?? string.Empty;
            Plots = (plots ?? Enumerable.Empty<string>()).ToList();
        }

        /// <summary>How many plots are named outright before the count stands for the rest.</summary>
        public const int Named = 4;

        public string Why { get; }

        public IReadOnlyList<string> Plots { get; }

        public string InWords
        {
            get
            {
                return Plots.Count.ToString(CultureInfo.InvariantCulture)
                    + (Plots.Count == 1 ? " plot" : " plots") + ", " + Which + ": " + Why;
            }
        }

        private string Which
        {
            get
            {
                if (Plots.Count <= Named) return string.Join(", ", Plots.ToArray());

                return string.Join(", ", Plots.Take(Named).ToArray())
                    + " and " + (Plots.Count - Named).ToString(CultureInfo.InvariantCulture) + " more";
            }
        }
    }

    /// <summary>
    /// One of the two numbers the tool COMPUTES, how many plots got it and why the rest did not.
    /// </summary>
    public sealed class ComputedFieldCount
    {
        public ComputedFieldCount(string name, int written, IEnumerable<BlankedFor> blanked)
        {
            Name = name ?? string.Empty;
            Written = written;
            Blanked = (blanked ?? Enumerable.Empty<BlankedFor>()).ToList();
        }

        public string Name { get; }

        public int Written { get; }

        /// <summary>One entry per reason, never one per plot.</summary>
        public IReadOnlyList<BlankedFor> Blanked { get; }

        public int NotWritten
        {
            get { return Blanked.Sum(one => one.Plots.Count); }
        }

        public int Plots
        {
            get { return Written + NotWritten; }
        }

        public string InWords
        {
            get
            {
                if (Plots == 0)
                {
                    return Name + ": no form in this press asks for it, so nothing was computed "
                        + "and nothing about it was refused.";
                }

                return Name + ": " + Written + " of " + Plots
                    + (Plots == 1 ? " form" : " forms") + " got it and " + NotWritten + " did not."
                    + (Blanked.Count == 0
                        ? string.Empty
                        : " " + Blanked.Count + (Blanked.Count == 1 ? " reason" : " reasons")
                            + " under this line.");
            }
        }
    }

    /// <summary>
    /// The two numbers no schedule printed, counted over the whole press.
    ///
    /// **THE 19:52 RUN IS WHY.** Both parks templates wrote neither number on any of their plots
    /// while the other five wrote both, and the reason was in the report, once per plot, among
    /// 82,048 lines. Every route that blanks them is already recorded on the field itself, so
    /// nothing new is decided here: the reasons are counted and said once, and the detail stays
    /// in each plot's own block.
    ///
    /// **THEY ARE COUNTED TOGETHER BECAUSE THEY FAIL TOGETHER.** Both rest on the canopy, so one
    /// guard blanks both and two more blank one each, and a glance showing one of them would
    /// have read as a single field's problem.
    /// </summary>
    public sealed class ComputedGlance
    {
        public ComputedGlance(ComputedFieldCount greened, ComputedFieldCount percentage)
        {
            Greened = greened ?? new ComputedFieldCount(ComputedPlaces.GreenCoverName, 0, null);
            Percentage = percentage ?? new ComputedFieldCount(ComputedPlaces.PercentageName, 0, null);
        }

        public static readonly ComputedGlance NonePlanned = new ComputedGlance(null, null);

        public ComputedFieldCount Greened { get; }

        public ComputedFieldCount Percentage { get; }

        public string InWords
        {
            get
            {
                return "THE TWO COMPUTED NUMBERS: the workbook computes both and the patcher "
                    + "drops its cached results, so the tool works them out and shows its "
                    + "working. Each is counted over the forms that ask for it.";
            }
        }
    }

    /// <summary>
    /// The questions one press answers, counted once over every template so each can be read
    /// in one look instead of out of hundreds of lines.
    ///
    /// **Every count here comes off the OUTCOME, never off the plan**, which is the shape this
    /// repo settled on after a report named four views as created and as not created in one
    /// file. The area cell counts as written when it landed in the output file and holds
    /// something, read back off the file rather than taken from what the fill set out to write.
    /// </summary>
    /// <summary>
    /// **HOW BIG EVERY WRITTEN VALUE CAME OUT, counted once for the whole press.**
    ///
    /// Measured on ANH-007-MO-100011: 2797.6 cut off at the edge of its box, 0.0008 cut off, and
    /// tree counts drawn taller than the boxes holding them. The size of every value is now a
    /// decision this tool makes and a decision it has to account for.
    ///
    /// **ONE COUNT PER OUTCOME AND A LINE PER BOX ONLY WHERE SOMEBODY HAS TO ACT.** Held at 6
    /// means the text runs over anyway and the box has to be looked at. Widths UNKNOWN and no /DA
    /// mean the tool could not decide at all and wrote the client's own size. A value that fits,
    /// or one shrunk to fit, needs no line over 150 plots.
    /// </summary>
    public sealed class TextFitGlance
    {
        public TextFitGlance(IEnumerable<PdfFieldFit> fits)
        {
            Fits = (fits ?? Enumerable.Empty<PdfFieldFit>()).ToList();
        }

        public static readonly TextFitGlance NonePlanned = new TextFitGlance(null);

        public IReadOnlyList<PdfFieldFit> Fits { get; }

        public int Count(PdfFitOutcome outcome)
        {
            return Fits.Count(one => one.Outcome == outcome);
        }

        /// <summary>
        /// Every box a person has to look at: the text that does not fit at the floor, and the
        /// field whose font this tool could not measure. **Named one by one and never counted
        /// alone**, because a count says a size is wrong somewhere and a line says which box.
        /// </summary>
        public IReadOnlyList<PdfFieldFit> Named
        {
            get
            {
                return Fits
                    .Where(one => one.Outcome == PdfFitOutcome.HeldAtSix
                        || one.Outcome == PdfFitOutcome.WidthsUnknown
                        || one.Outcome == PdfFitOutcome.NoDefaultAppearance)
                    .ToList();
            }
        }

        /// <summary>
        /// Every field whose size did not land, read back off the written file. **A size the tool
        /// meant to set and did not is a box still cut off**, so it is counted apart from the
        /// ones it never tried to set.
        /// </summary>
        public IReadOnlyList<PdfFieldFit> DidNotLand
        {
            get { return Fits.Where(one => !one.SizeLanded).ToList(); }
        }

        public string InWords
        {
            get
            {
                if (Fits.Count == 0)
                {
                    return "THE TEXT SIZES: no PDF field was written in this press, so no size "
                        + "was fitted to a box.";
                }

                return "THE TEXT SIZES: " + Fits.Count
                    + (Fits.Count == 1 ? " value was written, " : " values were written, ")
                    + Count(PdfFitOutcome.KeptTheClientsSize) + " at the size their own /DA sets, "
                    + Count(PdfFitOutcome.Shrunk) + " shrunk to fit, "
                    + Count(PdfFitOutcome.CappedAtTen) + " capped at "
                    + PdfTextFit.AutoCeiling.ToString("0.###", CultureInfo.InvariantCulture)
                    + " where the field's /DA gives nought, "
                    + Count(PdfFitOutcome.HeldAtSix) + " held at "
                    + PdfTextFit.Smallest.ToString("0.###", CultureInfo.InvariantCulture)
                    + " and running over, " + Count(PdfFitOutcome.WidthsUnknown)
                    + " with the font's widths UNKNOWN, and "
                    + Count(PdfFitOutcome.NoDefaultAppearance) + " with no /DA to change."
                    + (DidNotLand.Count == 0
                        ? string.Empty
                        : " " + DidNotLand.Count + " of the sizes this run set did NOT land in "
                            + "the written file, which is a bug in the tool.");
            }
        }
    }

    public sealed class RunGlance
    {
        public RunGlance(
            AreaCellGlance streetsArea, RegionGlance regions, DivisionGlance divisions,
            PdfGlance pdfs = null, ComputedGlance computed = null, TextFitGlance textSizes = null,
            string sharing = null, string ready = null, string noPlanting = null,
            string unreadCanopy = null, IEnumerable<string> treeLists = null)
        {
            StreetsArea = streetsArea ?? AreaCellGlance.NoStreetPlots;
            Regions = regions ?? RegionGlance.NoAreaRead;
            Divisions = divisions ?? DivisionGlance.NoneFound;
            Pdfs = pdfs ?? PdfGlance.NonePlanned;
            Computed = computed ?? ComputedGlance.NonePlanned;
            TextSizes = textSizes ?? TextFitGlance.NonePlanned;
            Sharing = sharing ?? string.Empty;
            Ready = ready ?? string.Empty;
            NoPlanting = noPlanting ?? string.Empty;
            UnreadCanopy = unreadCanopy ?? string.Empty;
            TreeLists = (treeLists ?? Enumerable.Empty<string>()).ToList();
        }

        /// <summary>
        /// One line per ticked template: clean, or how many cells of its tree lists are named.
        /// None of it showed until a plot hit a bad row.
        /// </summary>
        public IReadOnlyList<string> TreeLists { get; }

        /// <summary>
        /// Every ticked template whose tree list canopy total could not be read, named. One line
        /// every press, because a check that switched itself off used to read exactly like one
        /// that passed.
        /// </summary>
        public string UnreadCanopy { get; }

        /// <summary>
        /// How many plots were written nowhere because another ticked plot shares their
        /// PRX_Plot_UID2 and their folder, and the values. One line, every press.
        /// </summary>
        public string Sharing { get; }

        /// <summary>
        /// Ready N of M, over the team's own plot list. Empty where no list was set, because
        /// ready is a question about their list rather than about the model.
        /// </summary>
        public string Ready { get; }

        /// <summary>
        /// Every plot whose two schedules both printed a heading and no rows, named.
        /// </summary>
        public string NoPlanting { get; }

        /// <summary>How big every written value came out, and how many did not fit.</summary>
        public TextFitGlance TextSizes { get; }

        public PdfGlance Pdfs { get; }

        /// <summary>
        /// The two numbers no schedule printed. **Every EXISTING PARKS and FUTURE PARKS form of
        /// the 19:52 run wrote neither, and the file said why once per plot and never once.**
        /// </summary>
        public ComputedGlance Computed { get; }

        public AreaCellGlance StreetsArea { get; }

        public RegionGlance Regions { get; }

        public DivisionGlance Divisions { get; }
    }

    public static class RunAtAGlance
    {
        public static RunGlance Of(KpiCreateRunSet set)
        {
            if (set == null) throw new ArgumentNullException("set");

            return new RunGlance(
                StreetsArea(set), Regions(set), Divisions(set), Pdfs(set), Computed(set),
                TextSizes(set),
                SharedUid2.InWords(set.Sharing),
                PlotReady.InWords(
                    set.PlotList != null && set.PlotList.Read
                        ? PlotReady.Of(set, set.PlotList.Plots)
                        : null),
                NoPlanting.InWords(NoPlanting.In(set)),
                UnreadableCanopyColumns.InWords(UnreadableCanopyColumns.In(set)),
                set.TreeLists
                    .GroupBy(one => one.TemplateName, StringComparer.Ordinal)
                    .Select(group => TreeListCheck.InWords(group.Key, group))
                    .ToList());
        }

        /// <summary>
        /// Every size this press fitted, over every PDF it wrote.
        /// </summary>
        private static TextFitGlance TextSizes(KpiCreateRunSet set)
        {
            return new TextFitGlance(set.PlotOutcomes
                .Where(one => one.Pdf != null && one.Pdf.Written)
                .SelectMany(one => one.Pdf.Fitted));
        }

        /// <summary>
        /// The two computed numbers, counted off what each PDF really wrote and each blanked
        /// field's own recorded reason.
        ///
        /// **A FIELD THE FORM DOES NOT ASK FOR IS NOT COUNTED EITHER WAY.** Only the Parks form
        /// names the percentage, so counting the other two forms' plots as not having written it
        /// would read as 111 failures. A form is in a field's count when the run recorded the
        /// field on it, written or blank, and nothing here holds a list of which form asks for
        /// what.
        /// </summary>
        private static ComputedGlance Computed(KpiCreateRunSet set)
        {
            return new ComputedGlance(
                OneComputed(set, PdfValue.TotalAreasToBeGreened, ComputedPlaces.GreenCoverName),
                OneComputed(set, PdfValue.PercentageCanopy, ComputedPlaces.PercentageName));
        }

        private static ComputedFieldCount OneComputed(KpiCreateRunSet set, PdfValue value, string name)
        {
            int written = 0;
            var order = new List<string>();
            var plots = new Dictionary<string, List<string>>(StringComparer.Ordinal);

            foreach (PlotOutcome one in set.PlotOutcomes)
            {
                if (one.Pdf == null || !one.Pdf.Written) continue;

                if (one.Pdf.Landed.Any(field => field.Value == value))
                {
                    written = written + 1;
                    continue;
                }

                PdfFieldFill blank = one.Pdf.Blank.FirstOrDefault(field => field.Value == value);
                if (blank == null) continue;

                List<string> held;
                if (!plots.TryGetValue(blank.Why, out held))
                {
                    held = new List<string>();
                    plots[blank.Why] = held;
                    order.Add(blank.Why);
                }

                held.Add(one.PlotId);
            }

            return new ComputedFieldCount(
                name, written, order.Select(why => new BlankedFor(why, plots[why])));
        }

        /// <summary>
        /// Counted off the PLOT OUTCOMES, which carry what each plot's PDF really did, and the
        /// forms counted once each rather than once per plot that used one.
        /// </summary>
        private static PdfGlance Pdfs(KpiCreateRunSet set)
        {
            int written = 0;
            var without = new List<string>();
            var forms = new List<string>();
            var named = new HashSet<string>(StringComparer.Ordinal);
            int noughtBoxes = 0;
            var noughtPlots = new List<string>();
            var noughtNamed = new HashSet<string>(StringComparer.Ordinal);

            foreach (PlotOutcome one in set.PlotOutcomes)
            {
                if (one.Pdf == null) continue;

                if (one.Pdf.Written) written = written + 1;
                else if (one.Written) without.Add(one.PlotId + ": " + one.Pdf.Refusal);

                // **COUNTED OFF THE FLAG EACH FIELD CARRIES**, never off the words its working
                // prints, so the count cannot part from the decision when the words are rewritten.
                int noughts = one.Pdf.Landed.Count(field => field.NoughtForAnAbsentGroup);
                if (noughts > 0)
                {
                    noughtBoxes = noughtBoxes + noughts;
                    if (noughtNamed.Add(one.PlotId)) noughtPlots.Add(one.PlotId);
                }

                if (!one.Pdf.FormDidNotMatch) continue;

                string form = one.Pdf.FormName.Length == 0 ? "(no form)" : one.Pdf.FormName;
                if (named.Add(form)) forms.Add(form + ": " + one.Pdf.Check.Why);
            }

            return new PdfGlance(written, without, forms, noughtBoxes, noughtPlots);
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
            var notEvaluated = new List<string>();

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
                            : "divides by a cell the template already held"));
                }

                // **THE CELL, NOT THE SENTENCE.** The finding carries its own sheet and cell,
                // so the glance names a place rather than repeating a paragraph 284 times, and
                // the formula and the reason stay in the plot's own block.
                foreach (DivisionNotEvaluated one in run.Outcome.Formulas.DivisionsNotEvaluated)
                {
                    notEvaluated.Add(plot + " | " + one.Where);
                }
            }

            return new DivisionGlance(found, ours, where, notEvaluated);
        }
    }
}
