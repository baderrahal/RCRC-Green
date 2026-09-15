using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One group's area split into the phases the tree lists are named for, which the PDF form
    /// wants and the workbook has never asked for.
    ///
    /// **THE WORKBOOK TAKES THE PHASES ADDED TOGETHER AND THE PDF WANTS THEM APART.** Existing
    /// Shrubs and Proposed Shrubs are two fields on all three forms, where F11 on the main sheet
    /// is one number. Nothing about the workbook's number moves.
    ///
    /// **IT IS NOT A SECOND RULE BESIDE THE ONE FOR TREES.** A phase row is sorted by
    /// <see cref="CountedGroups.SheetFor"/>, the same method that decides which tree list a
    /// species goes on, so Street Design counts as Proposed on STREETS and is left out
    /// everywhere else without a word of that rule being written twice. A phase no sheet takes
    /// is in neither number and is named.
    /// </summary>
    public sealed class PhaseSplit
    {
        public PhaseSplit(
            string heading,
            bool groupFound,
            double existingSquareMetres,
            double proposedSquareMetres,
            IEnumerable<string> phasesLeftOut,
            IEnumerable<string> phasesWithNoSheet,
            GroundCoverSplit byPrefix = null)
        {
            Heading = heading ?? string.Empty;
            GroupFound = groupFound;
            ExistingSquareMetres = existingSquareMetres;
            ProposedSquareMetres = proposedSquareMetres;
            PhasesLeftOut = (phasesLeftOut ?? Enumerable.Empty<string>()).ToList();
            PhasesWithNoSheet = (phasesWithNoSheet ?? Enumerable.Empty<string>()).ToList();
            ByPrefix = byPrefix ?? GroundCoverSplit.NoGroup;
        }

        public static readonly PhaseSplit NoGroup =
            new PhaseSplit(string.Empty, false, 0.0, 0.0, null, null);

        /// <summary>
        /// **THE SAME GROUP SPLIT AGAIN, BY THE PREFIX ITS SPECIES NAMES CARRY.** The phase rows
        /// above answer existing against proposed and say nothing about shrubs against ground
        /// cover, which is the other half of the same group and the other box on the same form.
        /// </summary>
        public GroundCoverSplit ByPrefix { get; }

        public string Heading { get; }

        /// <summary>
        /// **False where the plot's schedule printed no group of that heading at all**, which is
        /// an absence and not a nought. The fields are left blank and named rather than written
        /// with a zero a person would read as a measurement.
        /// </summary>
        public bool GroupFound { get; }

        public double ExistingSquareMetres { get; }

        public double ProposedSquareMetres { get; }

        /// <summary>
        /// Total is Existing plus Proposed, which is what every one of the three forms means by
        /// TOTAL Shrubs. **Never the group total row**, because that row holds every phase the
        /// schedule printed, Street Design among them on a mosque plot.
        /// </summary>
        public double TotalSquareMetres
        {
            get { return ExistingSquareMetres + ProposedSquareMetres; }
        }

        /// <summary>
        /// Phase rows the group printed that this template leaves out, by name, so a number
        /// short of a phase is visible rather than quiet.
        /// </summary>
        public IReadOnlyList<string> PhasesLeftOut { get; }

        /// <summary>
        /// Phase rows that ARE counted by the template and whose sheet is neither tree list, a
        /// shape no model has shown. They are in neither number and named, because putting one
        /// into either would be a guess.
        /// </summary>
        public IReadOnlyList<string> PhasesWithNoSheet { get; }
    }

    public static class ShrubsByPhase
    {
        /// <summary>
        /// The shrubs group of one plot, split by phase.
        /// </summary>
        public static PhaseSplit Of(PlotReading reading, string heading, CountedGroups counted, ProjectUnit areaUnit = null)
        {
            if (counted == null) throw new ArgumentNullException("counted");
            if (reading == null) return PhaseSplit.NoGroup;

            GroupSubtotal group = reading.SubtotalHeaded(heading);
            if (group == null) return PhaseSplit.NoGroup;

            // **THE PREFIX SPLIT IS WORKED OUT HERE SO THE FORM ASKS ONE PLACE.** The shrubs
            // figures the PDF writes come off it and not off the phase rows below, because a
            // phase row holds shrubs and ground cover added together and the form wants them
            // apart.
            GroundCoverSplit byPrefix = GroundCoverSplit.Of(reading.PlotId, group, counted, areaUnit);

            double existing = 0.0;
            double proposed = 0.0;
            var leftOut = new List<string>();
            var noSheet = new List<string>();

            foreach (PhaseSubtotal phase in group.Phases)
            {
                if (!phase.Counted)
                {
                    leftOut.Add(phase.Name);
                    continue;
                }

                string sheet = counted.SheetFor(phase.Name);

                if (string.Equals(sheet, KpiTemplates.ExistingTreesSheet, StringComparison.OrdinalIgnoreCase))
                {
                    existing = existing + phase.SquareMetres;
                }
                else if (string.Equals(sheet, KpiTemplates.ProposedTreesSheet, StringComparison.OrdinalIgnoreCase))
                {
                    proposed = proposed + phase.SquareMetres;
                }
                else
                {
                    noSheet.Add(phase.Name);
                }
            }

            // **A GROUP THAT PRINTED NO PHASE ROW AT ALL SPLITS INTO NOTHING.** Its one row is
            // the group's whole value and nothing on it says which phase it is, so putting it
            // into either field would be a guess. The workbook's own number is untouched.
            if (group.Phases.Count == 0)
            {
                return new PhaseSplit(group.Heading, true, 0.0, 0.0, null,
                    new[] { "the group printed no phase row, so nothing says which phase its area is" },
                    byPrefix);
            }

            // **THE PHASE SUMS ARE KEPT AND THEY ARE NOT WHAT THE FORM IS GIVEN.** They are the
            // group's whole area per phase, shrubs and ground cover together, which is what the
            // report holds them against so a split that loses an area is visible.
            return new PhaseSplit(group.Heading, true, existing, proposed, leftOut, noSheet, byPrefix);
        }
    }
}
