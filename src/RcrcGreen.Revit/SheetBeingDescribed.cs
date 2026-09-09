using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core;

using ViewType = RcrcGreen.Core.ViewType;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// One sheet definition as the user is still filling it in.
    ///
    /// The panel holds these rather than reading the controls back, because step 4 is thrown
    /// away and built again on every change and a control that has gone is not a place to keep
    /// the only copy of something somebody typed.
    ///
    /// It is a Revit project type only because it is mutable and the panel owns it. What it
    /// hands out is the Core <see cref="SheetDefinition"/> and the <see cref="SheetToMake"/>
    /// rows, which are immutable and are what the run and every count are worked out from.
    /// </summary>
    internal sealed class SheetBeingDescribed
    {
        // In the order they were ticked, because that is the order they go onto sheets. This
        // was a HashSet, which has no order to keep, and the division is meaningless without
        // one.
        private readonly List<ViewType> _views = new List<ViewType>();

        // What the user typed over a proposal, keyed by plot and then by the planned sheet's
        // views, so an edit survives the redraw and stays with its own sheet while the
        // division changes shape around it.
        private readonly Dictionary<string, Dictionary<string, string>> _namesTyped =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);

        private readonly Dictionary<string, Dictionary<string, string>> _numbersTyped =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);

        public TitleBlockType TitleBlock { get; set; }

        public int ViewsPerSheet { get; set; } = 1;

        public bool Carries(ViewType type)
        {
            return type != null && _views.Contains(type);
        }

        public void Carry(ViewType type, bool carried)
        {
            if (type == null) return;

            if (carried)
            {
                if (!_views.Contains(type)) _views.Add(type);
            }
            else
            {
                _views.Remove(type);
            }
        }

        public void TypeName(string plotId, string signature, string name)
        {
            Remember(_namesTyped, plotId, signature, name);
        }

        public void TypeNumber(string plotId, string signature, string number)
        {
            Remember(_numbersTyped, plotId, signature, number);
        }

        /// <summary>
        /// The immutable Core definition, narrowed to the view types still ticked in step 2.
        ///
        /// Unticking a type in step 2 has to take it off every sheet, or a sheet would keep
        /// asking for a view the run no longer offers. The tick is remembered rather than
        /// dropped, so re-ticking the type in step 2 puts it back on the sheet.
        /// </summary>
        public SheetDefinition Built(IReadOnlyList<ViewType> stillTicked)
        {
            var offered = new HashSet<ViewType>(stillTicked ?? new List<ViewType>());

            return new SheetDefinition(
                TitleBlock == null ? string.Empty : TitleBlock.FamilyName,
                TitleBlock == null ? string.Empty : TitleBlock.TypeName,
                _views.Where(offered.Contains),
                ViewsPerSheet);
        }

        /// <summary>
        /// One row per sheet this definition will make, per ticked plot, each carrying its
        /// name and number, prefilled or typed, or the reason nothing could be proposed.
        ///
        /// takenNumbers is shared across every described sheet and grows with each row, so two
        /// proposals on one panel can never offer the same number. It starts as the numbers
        /// the model already holds and gains what this panel asks for, typed or proposed.
        /// </summary>
        public IReadOnlyList<SheetRowShown> RowsFor(
            IReadOnlyList<ViewType> stillTicked,
            IEnumerable<string> plots,
            DrawingSheetSnapshot model,
            HashSet<string> takenNumbers)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (takenNumbers == null) throw new ArgumentNullException("takenNumbers");

            SheetDefinition definition = Built(stillTicked);
            IReadOnlyList<PlannedSheet> planned = definition.Planned;

            var rows = new List<SheetRowShown>();

            foreach (string plotId in (plots ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrEmpty(one)))
            {
                foreach (PlannedSheet sheet in planned)
                {
                    rows.Add(OneRow(definition, plotId, sheet, model, takenNumbers));
                }
            }

            return rows;
        }

        private SheetRowShown OneRow(
            SheetDefinition definition,
            string plotId,
            PlannedSheet sheet,
            DrawingSheetSnapshot model,
            HashSet<string> takenNumbers)
        {
            string typedName;
            bool nameTyped = Typed(_namesTyped, plotId, sheet.Signature, out typedName);
            string name = nameTyped ? typedName : sheet.ProposedName;

            string typedNumber;
            bool numberTyped = Typed(_numbersTyped, plotId, sheet.Signature, out typedNumber);

            string number = string.Empty;
            bool numberGenerated = false;
            string whyNoNumber = string.Empty;

            if (numberTyped)
            {
                number = typedNumber;
            }
            else if (sheet.NamedFromItsView)
            {
                SheetNumberProposal proposal = SheetNumbers.Propose(
                    sheet.Views[0].Code, takenNumbers, model.NumbersOnPlot(plotId));

                number = proposal.Number;
                numberGenerated = proposal.Offered;
                whyNoNumber = proposal.WhyNot;
            }

            // Whatever this row asks for is taken from here on, typed or proposed, so no later
            // proposal on this panel can offer it again.
            string asked = (number ?? string.Empty).Trim();
            if (asked.Length > 0) takenNumbers.Add(asked);

            return new SheetRowShown(
                plotId,
                sheet,
                new SheetToMake(
                    plotId,
                    number,
                    name,
                    sheet.Views,
                    definition.ViewsPerSheet,
                    definition.TitleBlockFamilyName,
                    definition.TitleBlockTypeName,
                    !nameTyped && sheet.NamedFromItsView,
                    numberGenerated),
                whyNoNumber);
        }

        private static void Remember(
            Dictionary<string, Dictionary<string, string>> typed,
            string plotId,
            string signature,
            string value)
        {
            if (plotId == null || signature == null) return;

            Dictionary<string, string> forPlot;
            if (!typed.TryGetValue(plotId, out forPlot))
            {
                forPlot = new Dictionary<string, string>(StringComparer.Ordinal);
                typed.Add(plotId, forPlot);
            }

            forPlot[signature] = value ?? string.Empty;
        }

        private static bool Typed(
            Dictionary<string, Dictionary<string, string>> typed,
            string plotId,
            string signature,
            out string value)
        {
            value = string.Empty;

            Dictionary<string, string> forPlot;
            if (plotId == null || signature == null
                || !typed.TryGetValue(plotId, out forPlot))
            {
                return false;
            }

            string held;
            if (!forPlot.TryGetValue(signature, out held)) return false;

            value = held ?? string.Empty;
            return true;
        }
    }

    /// <summary>
    /// One row of the step 4 table: the sheet the run would make and what the panel says next
    /// to it. The Core row inside is what the run and every count read, so the table and the
    /// run can never disagree about a sheet.
    /// </summary>
    internal sealed class SheetRowShown
    {
        public SheetRowShown(
            string plotId, PlannedSheet planned, SheetToMake row, string whyNoNumber)
        {
            PlotId = plotId ?? string.Empty;
            Planned = planned;
            Row = row;
            WhyNoNumber = whyNoNumber ?? string.Empty;
        }

        public string PlotId { get; }

        public PlannedSheet Planned { get; }

        public SheetToMake Row { get; }

        /// <summary>
        /// Why the number box starts empty on a sheet that would have been proposed one, said
        /// next to the box rather than left as a surprise at Run.
        /// </summary>
        public string WhyNoNumber { get; }
    }
}
