using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core;

using ViewType = RcrcGreen.Core.ViewType;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// One sheet as the user is still filling it in.
    ///
    /// The panel holds these rather than reading the controls back, because step 4 is thrown
    /// away and built again on every change and a control that has gone is not a place to keep
    /// the only copy of something somebody typed.
    ///
    /// It is a Revit project type only because it is mutable and the panel owns it. What it
    /// hands out is the Core <see cref="SheetDefinition"/>, which is immutable and is what the
    /// run and every count are worked out from.
    /// </summary>
    internal sealed class SheetBeingDescribed
    {
        private readonly HashSet<ViewType> _views = new HashSet<ViewType>();

        private readonly Dictionary<string, string> _numberByPlot =
            new Dictionary<string, string>(StringComparer.Ordinal);

        public TitleBlockType TitleBlock { get; set; }

        public string SheetName { get; set; } = string.Empty;

        public int ViewsPerSheet { get; set; } = 1;

        public bool Carries(ViewType type)
        {
            return type != null && _views.Contains(type);
        }

        public void Carry(ViewType type, bool carried)
        {
            if (type == null) return;

            if (carried) _views.Add(type);
            else _views.Remove(type);
        }

        public string NumberFor(string plotId)
        {
            string found;
            return plotId != null && _numberByPlot.TryGetValue(plotId, out found)
                ? found ?? string.Empty
                : string.Empty;
        }

        public void SetNumber(string plotId, string number)
        {
            if (plotId == null) return;

            _numberByPlot[plotId] = number ?? string.Empty;
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
            IEnumerable<ViewType> carrying = (stillTicked ?? new List<ViewType>())
                .Where(_views.Contains);

            return new SheetDefinition(
                TitleBlock == null ? string.Empty : TitleBlock.FamilyName,
                TitleBlock == null ? string.Empty : TitleBlock.TypeName,
                SheetName,
                carrying,
                ViewsPerSheet);
        }

        public SheetOrder Ordered(IReadOnlyList<ViewType> stillTicked, IEnumerable<string> plots)
        {
            return new SheetOrder(
                Built(stillTicked),
                (plots ?? Enumerable.Empty<string>())
                    .Select(plotId => new SheetRequest(plotId, NumberFor(plotId))));
        }
    }
}
