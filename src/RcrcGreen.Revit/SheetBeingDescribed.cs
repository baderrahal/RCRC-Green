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
        /// What the user typed into one row's number box, so the panel can count it as an
        /// occupied slot when it builds the other rows' numbers.
        /// </summary>
        public bool TypedNumber(string plotId, string signature, out string number)
        {
            return Typed(_numbersTyped, plotId, signature, out number);
        }

        /// <summary>
        /// One row per sheet this definition will make, per ticked plot, each carrying its
        /// name and number, built or typed, or the reason nothing could be built.
        ///
        /// The built numbers arrive from the panel, worked out across every described sheet
        /// at once, because a plot's sheet letters run in one sequence per view code however
        /// many definitions add sheets to it. Working them out here per definition is how a
        /// plot would get two sheets both lettered A.
        /// </summary>
        public IReadOnlyList<SheetRowShown> RowsFor(
            IReadOnlyList<ViewType> stillTicked,
            IEnumerable<string> plots,
            IReadOnlyDictionary<string, SheetNumberProposal> numbersBuilt)
        {
            if (numbersBuilt == null) throw new ArgumentNullException("numbersBuilt");

            SheetDefinition definition = Built(stillTicked);
            IReadOnlyList<PlannedSheet> planned = definition.Planned;

            var rows = new List<SheetRowShown>();

            foreach (string plotId in (plots ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrEmpty(one)))
            {
                for (int sheetAt = 0; sheetAt < planned.Count; sheetAt++)
                {
                    rows.Add(OneRow(
                        definition,
                        plotId,
                        planned[sheetAt],
                        numbersBuilt,
                        NumberKey(plotId, sheetAt)));
                }
            }

            return rows;
        }

        /// <summary>
        /// How the panel and this type address one row's built number. The plot and the
        /// position, because the signature repeats when two plots get the same set of views.
        /// </summary>
        public static string NumberKey(string plotId, int sheetAt)
        {
            return plotId + "|" + sheetAt.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        private SheetRowShown OneRow(
            SheetDefinition definition,
            string plotId,
            PlannedSheet sheet,
            IReadOnlyDictionary<string, SheetNumberProposal> numbersBuilt,
            string key)
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
            else
            {
                SheetNumberProposal proposal;
                if (numbersBuilt.TryGetValue(key, out proposal))
                {
                    number = proposal.Number;
                    numberGenerated = proposal.Offered;
                    whyNoNumber = proposal.WhyNot;
                }
            }

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
                    numberGenerated,
                    whyNoNumber),
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
        /// Why the number box starts empty on a sheet whose number could not be built, said
        /// next to the box rather than left as a surprise at Run.
        /// </summary>
        public string WhyNoNumber { get; }
    }
}
