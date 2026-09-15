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
    /// **IT IS NEVER GUESSED.** A name carrying no colon, or a prefix that is neither of the two,
    /// goes into NEITHER figure and is named with its plot, its row and what its prefix read.
    /// Putting it in one is how an area lands in a box nobody measured, and the group total is
    /// the check that catches it: the two figures plus everything unplaced must equal what the
    /// schedule printed.
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
    public sealed class GroundCoverSplit
    {
        private GroundCoverSplit(
            double existingShrubs,
            double proposedShrubs,
            double groundCover,
            IEnumerable<UnplacedSpecies> unplaced,
            bool groupTotalPrinted,
            double groupTotal,
            string refusal,
            bool speciesRead)
        {
            ExistingShrubsSquareMetres = existingShrubs;
            ProposedShrubsSquareMetres = proposedShrubs;
            GroundCoverSquareMetres = groundCover;
            Unplaced = (unplaced ?? Enumerable.Empty<UnplacedSpecies>()).ToList();
            GroupTotalPrinted = groupTotalPrinted;
            GroupTotal = groupTotal;
            Refusal = (refusal ?? string.Empty).Trim();
            SpeciesRead = speciesRead;
        }

        public static readonly GroundCoverSplit NoGroup =
            new GroundCoverSplit(0.0, 0.0, 0.0, null, false, 0.0, string.Empty, false);

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

        public IReadOnlyList<UnplacedSpecies> Unplaced { get; }

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

            foreach (ShrubSpecies species in group.Species)
            {
                if (SpeciesPrefix.IsGroundCover(species.BotanicalName))
                {
                    cover = cover + species.SquareMetres;
                    continue;
                }

                if (!SpeciesPrefix.IsShrubs(species.BotanicalName))
                {
                    unplaced.Add(new UnplacedSpecies(
                        plotId, species.BotanicalName, species.SquareMetres, species.RowNumber,
                        SpeciesPrefix.WhyUnplaced(species.BotanicalName)));
                    continue;
                }

                string sheet = counted.SheetFor(species.Phase);

                if (LabelText.Same(sheet, KpiTemplates.ExistingTreesSheet))
                {
                    existing = existing + species.SquareMetres;
                }
                else if (LabelText.Same(sheet, KpiTemplates.ProposedTreesSheet))
                {
                    proposed = proposed + species.SquareMetres;
                }
                else
                {
                    // A SHRUBS species under a phase neither tree list is named for, Street
                    // Design on a mosque plot among them. It is in neither figure and named, the
                    // same answer the phase rows already give, and it still counts towards the
                    // group total so the check below stays honest.
                    unplaced.Add(new UnplacedSpecies(
                        plotId, species.BotanicalName, species.SquareMetres, species.RowNumber,
                        species.Phase.Length == 0
                            ? "it sits under no phase row, so nothing says whether it is existing or proposed"
                            : "its phase " + species.Phase + " is one no tree list sheet is named for"));
                }
            }

            double placed = existing + proposed + cover + unplaced.Sum(one => one.SquareMetres);
            string refusal = Disagreeing(group, placed, existing, proposed, cover, unplaced.Count, areaUnit);

            return new GroundCoverSplit(
                existing, proposed, cover, unplaced,
                group.GroupTotalPrinted, group.GroupTotalSquareMetres, refusal, group.Species.Count > 0);
        }

        /// <summary>
        /// The split against the row the schedule printed. **The room is the one the phase rows
        /// already earn**, half the project's area rounding step for each row summed, read off
        /// the project units and never a constant, because every printed area is already rounded
        /// and a sum of rounded numbers need not equal a rounded sum.
        /// </summary>
        private static string Disagreeing(
            GroupSubtotal group,
            double placed,
            double existing,
            double proposed,
            double cover,
            int unplaced,
            ProjectUnit areaUnit)
        {
            if (!group.GroupTotalPrinted) return string.Empty;

            int summed = group.Species.Count;
            if (summed == 0)
            {
                return "its group total reads " + Area(group.GroupTotalSquareMetres)
                    + " and the schedule printed no species row under it, so nothing says how much"
                    + " of it is " + SpeciesPrefix.Shrubs + " and how much is " + SpeciesPrefix.GroundCover + ".";
            }

            double total = group.GroupTotalSquareMetres;
            double off = Math.Abs(total - placed);
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
                + SpeciesPrefix.Shrubs + " " + Area(existing + proposed) + ", "
                + SpeciesPrefix.GroundCover + " " + Area(cover) + " and "
                + unplaced + (unplaced == 1 ? " species" : " species")
                + " placed nowhere add to " + Area(placed)
                + " against its printed group total of " + Area(total)
                + ", off by " + Area(off) + room + ".";
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
