using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// The sheet number one plot gets for one sheet definition.
    ///
    /// It is the only part of a sheet that differs between plots, and it is typed by the user
    /// or picked off the numbers the model already uses. Never invented, so a plot without one
    /// gets no sheet and the run says which plot and which sheet.
    /// </summary>
    public sealed class SheetRequest
    {
        public SheetRequest(string plotId, string sheetNumber)
        {
            if (plotId == null) throw new ArgumentNullException("plotId");

            PlotId = plotId;
            SheetNumber = (sheetNumber ?? string.Empty).Trim();
        }

        public string PlotId { get; }

        public string SheetNumber { get; }

        public bool Complete
        {
            get { return SheetNumber.Length > 0; }
        }

        public override string ToString()
        {
            return PlotId + " " + SheetNumber;
        }
    }

    /// <summary>
    /// One sheet definition and the number every plot gets for it.
    ///
    /// The definition is shared and the numbers are not, which is the whole shape of how the
    /// team works: one sheet described once, repeated across the plots, each carrying its own
    /// number. A run can hold several of these, so one press gives a plot its LIST OF DRAWINGS
    /// and its GENERAL ARRANGEMENT LAYOUT together.
    /// </summary>
    public sealed class SheetOrder
    {
        public SheetOrder(SheetDefinition definition, IEnumerable<SheetRequest> numbers)
        {
            if (definition == null) throw new ArgumentNullException("definition");

            Definition = definition;

            // One number per plot. A second row for a plot would make two sheets that differ
            // only by a number nobody meant to type twice.
            Numbers = (numbers ?? Enumerable.Empty<SheetRequest>())
                .Where(one => one != null)
                .GroupBy(one => one.PlotId, StringComparer.Ordinal)
                .Select(byPlot => byPlot.First())
                .ToList();
        }

        public SheetDefinition Definition { get; }

        public IReadOnlyList<SheetRequest> Numbers { get; }

        public SheetRequest NumberFor(string plotId)
        {
            if (plotId == null) return null;
            return Numbers.FirstOrDefault(
                one => string.Equals(one.PlotId, plotId, StringComparison.Ordinal));
        }

        /// <summary>
        /// How many plots have a number typed in. It is what the step header counts.
        /// </summary>
        public int FilledIn
        {
            get { return Numbers.Count(one => one.Complete); }
        }
    }
}
