using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One species row of a shrubs and lawn group: its printed name, its area, the phase row it
    /// sat under and the row it printed on.
    ///
    /// **THE NAME IS KEPT BECAUSE THE NAME IS WHAT SAYS WHICH BOX THE AREA GOES IN.** The group
    /// is called SHRUBS &amp; GROUND COVER and it holds two kinds, and the only thing in the
    /// schedule that tells them apart is the prefix the species name carries.
    /// </summary>
    public sealed class ShrubSpecies
    {
        public ShrubSpecies(string botanicalName, double squareMetres, string phase, int rowNumber)
        {
            if (rowNumber < 0) throw new ArgumentOutOfRangeException("rowNumber");

            BotanicalName = (botanicalName ?? string.Empty).Trim();
            SquareMetres = squareMetres;
            Phase = (phase ?? string.Empty).Trim();
            RowNumber = rowNumber;
        }

        public string BotanicalName { get; }

        public double SquareMetres { get; }

        /// <summary>The phase row this species sat under, empty where it sat under none.</summary>
        public string Phase { get; }

        /// <summary>The printed row, counting the heading row as row 1.</summary>
        public int RowNumber { get; }
    }

    /// <summary>
    /// The kind a species name declares, read off the text before its first colon.
    ///
    /// **MEASURED ON THE 05:49 REPORT, over 156 plots**: 143 names begin `SHRUBS:`, 107 begin
    /// `GROUND COVER:` and 55 begin `GRASS:`. So the prefix is data the schedule already prints
    /// and nothing here infers a kind from anything else.
    ///
    /// **IT IS NEVER GUESSED, AND THERE IS ONE NAME THAT IS NOT A GUESS.** A name carrying no
    /// colon, or a prefix that is neither of the two, goes into NEITHER figure and is named with
    /// its plot, its row and what its prefix read. Putting it in one is how an area lands in a
    /// box nobody measured.
    ///
    /// **THE EXCEPTION IS A BOTANICAL NAME THAT IS A SINGLE DASH ONCE TRIMMED**, which counts as
    /// <see cref="Shrubs"/> and then goes by its phase like any `SHRUBS:` species. Bader's
    /// decision of 15 September, off his own record: **44 schedule rows across the model carry a
    /// botanical name of a dash**, and four plots came out blank because of them, EP-01, EP-09,
    /// EP-14 and HF-01, which are the four the eighty seventh pass made refuse. His screenshot of
    /// ANH-007-HF-100002 shows Existing Shrubs, Proposed Shrubs, TOTAL Shrubs and Ground Cover
    /// all empty.
    ///
    /// **ONLY THE DASH.** A name with no colon that is not a dash, and a prefix that is neither
    /// `SHRUBS` nor `GROUND COVER`, refuses exactly as it did. A dash row under a phase the
    /// template leaves out stays out as it did, and a dash row under NO phase row still refuses,
    /// because nothing says whether it is existing or proposed and that is a different gap from
    /// not knowing what kind it is.
    /// </summary>
    public static class SpeciesPrefix
    {
        public const string Shrubs = "SHRUBS";

        public const string GroundCover = "GROUND COVER";

        /// <summary>
        /// The text before the first colon, edge whitespace off, or empty where the name carries
        /// no colon at all. The inside is untouched, the same rule
        /// <see cref="LabelText.Trimmed"/> holds for a label.
        /// </summary>
        public static string Of(string botanicalName)
        {
            string name = botanicalName ?? string.Empty;
            int colon = name.IndexOf(':');

            return colon < 0 ? string.Empty : name.Substring(0, colon).Trim();
        }

        public static bool IsShrubs(string botanicalName)
        {
            return LabelText.Same(Of(botanicalName), Shrubs);
        }

        public static bool IsGroundCover(string botanicalName)
        {
            return LabelText.Same(Of(botanicalName), GroundCover);
        }

        /// <summary>
        /// The mark a schedule prints where a species has no botanical name, measured on 44 rows
        /// across the model. It is the WHOLE cell once the edges are off, never a dash inside a
        /// name, so `ACACIA / VACHELLIA FARNESIANA` and `SHRUBS: X - Y` are untouched.
        /// </summary>
        public const string Dash = "-";

        public static bool IsDash(string botanicalName)
        {
            return string.Equals(LabelText.Trimmed(botanicalName), Dash, StringComparison.Ordinal);
        }

        /// <summary>
        /// **What reaches the two shrubs figures**, which is a `SHRUBS:` name or a dash. The two
        /// are asked through one method so the dash rule cannot be added in one place and
        /// forgotten in another, which is the shape this repository keeps paying for.
        /// </summary>
        public static bool CountsAsShrubs(string botanicalName)
        {
            return IsShrubs(botanicalName) || IsDash(botanicalName);
        }

        /// <summary>
        /// Why a species reached neither figure, said in the words a person can check against
        /// the schedule in front of them.
        /// </summary>
        public static string WhyUnplaced(string botanicalName)
        {
            string prefix = Of(botanicalName);

            return prefix.Length == 0
                ? "its name carries no prefix before a colon, so nothing says whether it is "
                    + Shrubs + " or " + GroundCover
                : "its prefix reads " + prefix + ", which is neither " + Shrubs + " nor " + GroundCover;
        }
    }

    /// <summary>
    /// A species the split could place in neither figure, carried with everything a person needs
    /// to find it.
    /// </summary>
    public sealed class UnplacedSpecies
    {
        public UnplacedSpecies(string plotId, string botanicalName, double squareMetres, int rowNumber, string why)
        {
            PlotId = (plotId ?? string.Empty).Trim();
            BotanicalName = (botanicalName ?? string.Empty).Trim();
            SquareMetres = squareMetres;
            RowNumber = rowNumber;
            Why = (why ?? string.Empty).Trim();
        }

        public string PlotId { get; }

        public string BotanicalName { get; }

        public double SquareMetres { get; }

        public int RowNumber { get; }

        public string Why { get; }

        public string InWords
        {
            get
            {
                return PlotId + " row " + RowNumber.ToString(CultureInfo.InvariantCulture) + ", "
                    + BotanicalName + ", " + GroundCoverSplit.Area(SquareMetres) + ": " + Why;
            }
        }
    }

    /// <summary>
    /// **THE SHRUBS AND GROUND COVER GROUP HOLDS TWO KINDS AND THE FORM HAS TWO BOXES.**
    ///
    /// The tool wrote the whole group total into Proposed Shrubs and left Ground Cover blank.
    /// **On DM-14 that is the wrong box on a real plot**, read off the schedule on screen: its
    /// group holds `GROUND COVER: CARISSA MACROCAPA` 270 and `GROUND COVER: LAMPRANTHUS AUREUS`
    /// 198, adding to the printed group total of 468, and every one of its species is GROUND
    /// COVER and none is SHRUBS. DM-11 is the opposite end, two `SHRUBS:` species adding to 70.
    ///
    /// **ONLY ONE OF THE TWO READINGS ADDS UP.** Writing the group total into Ground Cover as
    /// well would give DM-11 Ground Cover 70 beside Proposed Shrubs 70, the same area in two
    /// boxes of one form. The prefix split gives each box its own species and the two add to the
    /// group total, which is the check.
    ///
    /// **THE GRASS GROUP IS UNTOUCHED.** Its species carry `GRASS:` and it already goes to lawn
    /// through its own heading, so nothing here reads it.
    ///
    /// **THE SUM IS HELD AGAINST THE ROW THE SCHEDULE PRINTED.** Shrubs plus ground cover plus
    /// everything unplaced must equal the group total, within the room the project's own area
    /// rounding earns, which is the rule the phase rows are already checked by rather than a
    /// second one written here. Outside it the split refuses and names all three numbers.
    /// </summary>
    /// <summary>
    /// One row whose botanical name is a single dash, counted as SHRUBS and placed by its phase.
    ///
    /// **EVERY ONE OF THEM IS ON THE RECORD.** A row with no name that reaches a client's box is
    /// exactly the kind of quiet placement this tool refuses everywhere else, so it is allowed
    /// only because Bader decided it and only with its plot, its row, its phase, its area and the
    /// box it went into printed beside the species no prefix placed.
    /// </summary>
    public sealed class DashRow
    {
        public DashRow(string plotId, double squareMetres, int rowNumber, string phase, string box)
        {
            PlotId = (plotId ?? string.Empty).Trim();
            SquareMetres = squareMetres;
            RowNumber = rowNumber;
            Phase = (phase ?? string.Empty).Trim();
            Box = (box ?? string.Empty).Trim();
        }

        public string PlotId { get; }

        public double SquareMetres { get; }

        /// <summary>The printed row, counting the heading row as row 1.</summary>
        public int RowNumber { get; }

        /// <summary>Empty where the row sits under no phase row, which still refuses.</summary>
        public string Phase { get; }

        /// <summary>Which of the four boxes took it, or why none did.</summary>
        public string Box { get; }

        public string InWords
        {
            get
            {
                return PlotId + " row " + RowNumber.ToString(CultureInfo.InvariantCulture) + ", "
                    + (Phase.Length == 0 ? "under no phase row" : Phase) + ", "
                    + GroundCoverSplit.Area(SquareMetres) + ", " + Box;
            }
        }
    }

    public sealed class GroundCoverSplit
    {
        private GroundCoverSplit(
            double existingShrubs,
            double proposedShrubs,
            double groundCover,
            IEnumerable<UnplacedSpecies> unplaced,
            IEnumerable<UnplacedSpecies> outOfScope,
            bool groupTotalPrinted,
            double groupTotal,
            string refusal,
            bool speciesRead,
            IEnumerable<DashRow> dashRows = null)
        {
            ExistingShrubsSquareMetres = existingShrubs;
            ProposedShrubsSquareMetres = proposedShrubs;
            GroundCoverSquareMetres = groundCover;
            Unplaced = (unplaced ?? Enumerable.Empty<UnplacedSpecies>()).ToList();
            OutOfScope = (outOfScope ?? Enumerable.Empty<UnplacedSpecies>()).ToList();
            GroupTotalPrinted = groupTotalPrinted;
            GroupTotal = groupTotal;
            Refusal = (refusal ?? string.Empty).Trim();
            SpeciesRead = speciesRead;
            DashRows = (dashRows ?? Enumerable.Empty<DashRow>()).ToList();
        }

        /// <summary>
        /// Every row whose botanical name was a single dash, with the box it reached. Empty on
        /// every plot whose species are all named, which is most of them.
        /// </summary>
        public IReadOnlyList<DashRow> DashRows { get; }

        public static readonly GroundCoverSplit NoGroup =
            new GroundCoverSplit(0.0, 0.0, 0.0, null, null, false, 0.0, string.Empty, false);

        public double ExistingShrubsSquareMetres { get; }

        public double ProposedShrubsSquareMetres { get; }

        public double TotalShrubsSquareMetres
        {
            get { return ExistingShrubsSquareMetres + ProposedShrubsSquareMetres; }
        }

        /// <summary>
        /// **ONE FIGURE AND NOT TWO.** All three forms print one Ground Cover box with no phase
        /// beside it, where Shrubs are asked for existing and proposed apart, so the ground cover
        /// species of every counted phase add into this one number.
        /// </summary>
        public double GroundCoverSquareMetres { get; }

        /// <summary>
        /// **Species this tool could not READ into either box**, because their prefix is neither
        /// of the two or they carry none, or because nothing said which phase they are. Every
        /// one of these REFUSES the plot: an area the tool cannot account for is not a number to
        /// leave out quietly.
        /// </summary>
        public IReadOnlyList<UnplacedSpecies> Unplaced { get; }

        /// <summary>
        /// **Species the tool read perfectly well and the TEMPLATE leaves out**, which today is
        /// a `SHRUBS:` species under a phase no tree list sheet is named for, Street Design on a
        /// mosque plot being the measured one.
        ///
        /// **THESE ARE NOT A REFUSAL AND THE DIFFERENCE IS THE POINT.** Bader decided Street
        /// Design is somebody else's scope and does not belong on this plot's checklist, and the
        /// phase rows have left it out and named it since that decision. Refusing the plot over
        /// it would reverse a decision rather than catch a fault, and FM-05 alone carries 459 m2
        /// of it. So it is a TERM of the sum below, named beside the figures, and what the tool
        /// could not read is what refuses.
        /// </summary>
        public IReadOnlyList<UnplacedSpecies> OutOfScope { get; }

        public double OutOfScopeSquareMetres
        {
            get { return OutOfScope.Sum(one => one.SquareMetres); }
        }

        public bool GroupTotalPrinted { get; }

        public double GroupTotal { get; }

        /// <summary>
        /// Whether the group printed any species row at all. **A group with none cannot be
        /// split**, and the refusal says so rather than writing nought into both boxes.
        /// </summary>
        public bool SpeciesRead { get; }

        /// <summary>
        /// Empty when the split adds up to the group total. Otherwise it names all three numbers
        /// and refuses, because a split that does not add up is a question for the team rather
        /// than a number this tool picks.
        /// </summary>
        public string Refusal { get; }

        public bool AddsUp
        {
            get { return Refusal.Length == 0; }
        }

        public double UnplacedSquareMetres
        {
            get { return Unplaced.Sum(one => one.SquareMetres); }
        }

        /// <summary>
        /// **What the four boxes WOULD have read**, which on a refused plot is the size of the
        /// loss beside the group total rather than a number anybody writes. HF-01 would have
        /// read 52 m2 against a group total of 283, so 231 is what the form would have been
        /// short by if the refusal had not fired.
        /// </summary>
        public double WouldHaveWrittenSquareMetres
        {
            get { return ExistingShrubsSquareMetres + ProposedShrubsSquareMetres + GroundCoverSquareMetres; }
        }

        /// <summary>
        /// One group split. <paramref name="counted"/> decides which phase is existing and which
        /// is proposed, through the same <see cref="CountedGroups.SheetFor"/> the tree lists ask,
        /// so Street Design counts as proposed on STREETS and is left out elsewhere without that
        /// rule being written twice.
        /// </summary>
        public static GroundCoverSplit Of(
            string plotId, GroupSubtotal group, CountedGroups counted, ProjectUnit areaUnit)
        {
            if (counted == null) throw new ArgumentNullException("counted");
            if (group == null) return NoGroup;

            double existing = 0.0;
            double proposed = 0.0;
            double cover = 0.0;
            var unplaced = new List<UnplacedSpecies>();
            var outOfScope = new List<UnplacedSpecies>();
            var dashRows = new List<DashRow>();

            foreach (ShrubSpecies species in group.Species)
            {
                if (SpeciesPrefix.IsGroundCover(species.BotanicalName))
                {
                    cover = cover + species.SquareMetres;
                    continue;
                }

                // **A DASH IS A SHRUB, AND IT IS THE ONLY NAME THIS RULE READS AS ONE.** Bader's
                // decision of 15 September, off 44 such rows across the model. Every other
                // unprefixed name still refuses, so the rule is one literal and not a loosening.
                if (!SpeciesPrefix.CountsAsShrubs(species.BotanicalName))
                {
                    unplaced.Add(new UnplacedSpecies(
                        plotId, species.BotanicalName, species.SquareMetres, species.RowNumber,
                        SpeciesPrefix.WhyUnplaced(species.BotanicalName)));
                    continue;
                }

                bool dash = SpeciesPrefix.IsDash(species.BotanicalName);
                string sheet = counted.SheetFor(species.Phase);

                if (LabelText.Same(sheet, KpiTemplates.ExistingTreesSheet))
                {
                    existing = existing + species.SquareMetres;
                    if (dash) dashRows.Add(Dashed(plotId, species, ExistingShrubsBox));
                }
                else if (LabelText.Same(sheet, KpiTemplates.ProposedTreesSheet))
                {
                    proposed = proposed + species.SquareMetres;
                    if (dash) dashRows.Add(Dashed(plotId, species, ProposedShrubsBox));
                }
                else if (species.Phase.Length == 0)
                {
                    if (dash) dashRows.Add(Dashed(plotId, species, NoBoxNoPhase));
                    // Nothing says which phase it is, so it cannot be READ into either box. That
                    // is an area unaccounted for and it refuses.
                    unplaced.Add(new UnplacedSpecies(
                        plotId, species.BotanicalName, species.SquareMetres, species.RowNumber,
                        "it sits under no phase row, so nothing says whether it is existing or proposed"));
                }
                else
                {
                    // **READ AND DELIBERATELY LEFT OUT, which is not the same as unreadable.**
                    // Street Design on a mosque plot: the tool knows exactly what it is and the
                    // template does not take it. It is a term of the sum rather than a refusal,
                    // the same answer the phase rows have given since that decision.
                    outOfScope.Add(new UnplacedSpecies(
                        plotId, species.BotanicalName, species.SquareMetres, species.RowNumber,
                        "its phase " + species.Phase + " is one no tree list sheet is named for, "
                        + "so this template leaves it out"));

                    if (dash) dashRows.Add(Dashed(plotId, species, NoBoxLeftOut));
                }
            }

            // **WHAT WOULD BE WRITTEN, AND NOTHING ELSE.** Anything placed nowhere is a
            // DISAGREEMENT and never a term of this sum. Adding it in is the fault the 08:38 run
            // measured: EP-01's whole 411 m2 was unplaced, the equation still closed, the check
            // said YES and all four boxes were written reading nought.
            double written = existing + proposed + cover;
            string refusal = Disagreeing(group, written, existing, proposed, cover, unplaced, outOfScope, areaUnit);

            return new GroundCoverSplit(
                existing, proposed, cover, unplaced, outOfScope,
                group.GroupTotalPrinted, group.GroupTotalSquareMetres, refusal,
                group.Species.Count > 0, dashRows);
        }

        /// <summary>The four answers a dash row can come to, spelt once.</summary>
        public const string ExistingShrubsBox = "Existing Shrubs";

        public const string ProposedShrubsBox = "Proposed Shrubs";

        public const string NoBoxLeftOut = "no box, this template leaves its phase out";

        public const string NoBoxNoPhase = "no box, it sits under no phase row, so the plot refuses";

        private static DashRow Dashed(string plotId, ShrubSpecies species, string box)
        {
            return new DashRow(plotId, species.SquareMetres, species.RowNumber, species.Phase, box);
        }

        /// <summary>
        /// The split against the row the schedule printed.
        ///
        /// **A SPECIES PLACED NOWHERE REFUSES THE PLOT ON ITS OWN, whatever the arithmetic
        /// says.** This is the fault the 08:38 run measured and the wording that caused it was
        /// the round message's own: with the unplaced area inside the equation the sum ALWAYS
        /// closes, so the check read YES on EP-01, EP-09, EP-14 and HF-01 while every one of
        /// their four boxes was written short. EP-01 lost all 411 m2 and HF-01 went from 231,
        /// 52 and 283 on the 18:15 run to nought, nought and nought.
        ///
        /// **The room is the one the phase rows already earn**, half the project's area rounding
        /// step for each row summed, read off the project units and never a constant, because
        /// every printed area is already rounded and a sum of rounded numbers need not equal a
        /// rounded sum. It applies to the arithmetic and never to an unplaced species: a species
        /// nobody could place is not a rounding difference however small its area.
        /// </summary>
        private static string Disagreeing(
            GroupSubtotal group,
            double written,
            double existing,
            double proposed,
            double cover,
            List<UnplacedSpecies> unplaced,
            List<UnplacedSpecies> outOfScope,
            ProjectUnit areaUnit)
        {
            // **FIRST, AND WITHOUT LOOKING AT ANY NUMBER.** A species this tool could not place
            // is an area it cannot account for, so nothing is written for the plot and every one
            // of them is named. It fires where the schedule printed no group total too, which is
            // the case the arithmetic below cannot see at all.
            if (unplaced.Count > 0)
            {
                return Count(unplaced.Count, "species row") + " could not be placed in either box, so "
                    + "NOTHING was written into any of the four. " + Area(UnplacedIn(unplaced))
                    + " is unaccounted for"
                    + (group.GroupTotalPrinted
                        ? " of the " + Area(group.GroupTotalSquareMetres) + " the schedule printed"
                        : string.Empty)
                    + ", and the four boxes would have read " + Area(written) + ". Each one: "
                    + string.Join("; ", unplaced.Select(one => one.InWords).ToArray()) + ".";
            }

            if (!group.GroupTotalPrinted) return string.Empty;

            int summed = group.Species.Count;
            if (summed == 0)
            {
                return "its group total reads " + Area(group.GroupTotalSquareMetres)
                    + " and the schedule printed no species row under it, so nothing says how much"
                    + " of it is " + SpeciesPrefix.Shrubs + " and how much is " + SpeciesPrefix.GroundCover + ".";
            }

            // **WHAT IS ACCOUNTED FOR: what would be written, plus what the template deliberately
            // leaves out.** Nothing else is a term. The area the tool could not read has already
            // refused above, so it never reaches this sum and can never close it.
            double total = group.GroupTotalSquareMetres;
            double accounted = written + outOfScope.Sum(one => one.SquareMetres);
            double off = Math.Abs(total - accounted);
            double relative = Totalled.Tolerance * Math.Max(1.0, Math.Abs(total));
            if (off <= relative) return string.Empty;


            double step = areaUnit == null ? double.NaN : areaUnit.Accuracy;
            bool stepRead = !double.IsNaN(step) && step > 0.0;
            double allowed = stepRead ? summed * step / 2.0 : 0.0;
            if (stepRead && off <= allowed + relative) return string.Empty;

            string room = stepRead
                ? ", which is more than the " + Area(allowed) + " that " + summed
                    + (summed == 1 ? " row" : " rows") + " rounded to " + KpiReport.Step(step)
                    + (summed == 1 ? " allows" : " allow")
                : ", and the project's area rounding step was not read, so no rounding room was allowed";

            return "the split does not add up to the group total the schedule printed. "
                + SpeciesPrefix.Shrubs + " " + Area(existing + proposed) + " and "
                + SpeciesPrefix.GroundCover + " " + Area(cover)
                + " are what would be written, adding to " + Area(written)
                + (outOfScope.Count == 0
                    ? string.Empty
                    : ", and " + Area(UnplacedIn(outOfScope)) + " this template leaves out, "
                        + Area(accounted) + " in all")
                + ", against its printed group total of " + Area(total)
                + ", off by " + Area(off) + room + ".";
        }

        private static double UnplacedIn(IEnumerable<UnplacedSpecies> unplaced)
        {
            return unplaced.Sum(one => one.SquareMetres);
        }

        private static string Count(int howMany, string thing)
        {
            return howMany + " " + thing + (howMany == 1 ? string.Empty : "s");
        }

        /// <summary>
        /// Six places, the same as the phase rows' own note, because a number held against a
        /// rounding step has to print at the scale of that step.
        /// </summary>
        public static string Area(double value)
        {
            return value.ToString("0.######", CultureInfo.InvariantCulture) + " m²";
        }
    }
}
