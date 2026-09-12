using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Where a plot's marker came from. It is said beside the plot in step 1, because a value
    /// somebody set a minute ago and a value remembered from another day are different things
    /// to the person about to run.
    /// </summary>
    public enum MarkerSource
    {
        Unset = 0,
        Remembered = 1,
        SetNow = 2
    }

    /// <summary>
    /// One plot and the marker the user gave it.
    /// </summary>
    public sealed class PlotMarker
    {
        public PlotMarker(string plotId, string marker, MarkerSource source)
        {
            if (plotId == null) throw new ArgumentNullException("plotId");

            PlotId = plotId;
            Marker = (marker ?? string.Empty).Trim();
            Source = source;
        }

        public string PlotId { get; }

        public string Marker { get; }

        public MarkerSource Source { get; }

        public override string ToString()
        {
            return PlotId + " " + Marker;
        }
    }

    /// <summary>
    /// The markers of one model's plots, set by the user and remembered, never derived.
    ///
    /// A sheet number is the view code, then this marker, then a sheet letter when the code
    /// holds several sheets. The marker is the one part the model cannot answer: NG05 uses Q
    /// on DM-11 and NG03 uses 001 on FP-39, and every other NG05 plot carries only copy
    /// numbers, so anything read off the model would be a guess on most plots.
    /// </summary>
    public sealed class PlotMarkers
    {
        private readonly Dictionary<string, PlotMarker> _byPlot;

        private PlotMarkers(Dictionary<string, PlotMarker> byPlot)
        {
            _byPlot = byPlot;
        }

        public static readonly PlotMarkers Nothing =
            new PlotMarkers(new Dictionary<string, PlotMarker>(StringComparer.Ordinal));

        /// <summary>
        /// The markers read out of the store for one model, each marked remembered. What the
        /// user changes on the panel afterwards goes through <see cref="With"/> and is marked
        /// set now, so the provenance line can tell the two apart.
        /// </summary>
        public static PlotMarkers Remembered(IEnumerable<PlotMarker> stored)
        {
            var byPlot = new Dictionary<string, PlotMarker>(StringComparer.Ordinal);

            foreach (PlotMarker one in (stored ?? Enumerable.Empty<PlotMarker>())
                .Where(one => one != null && one.PlotId.Length > 0 && one.Marker.Length > 0))
            {
                byPlot[one.PlotId] = new PlotMarker(one.PlotId, one.Marker, MarkerSource.Remembered);
            }

            return new PlotMarkers(byPlot);
        }

        public PlotMarker For(string plotId)
        {
            if (plotId == null) return null;

            PlotMarker found;
            return _byPlot.TryGetValue(plotId, out found) ? found : null;
        }

        /// <summary>
        /// The marker alone, empty when the plot has none, which is what the number build
        /// takes.
        /// </summary>
        public string MarkerOf(string plotId)
        {
            PlotMarker found = For(plotId);
            return found == null ? string.Empty : found.Marker;
        }

        /// <summary>
        /// A new set with this plot's marker set now. An empty marker clears the plot instead,
        /// because an entry holding nothing is indistinguishable from no entry everywhere it
        /// is read.
        /// </summary>
        public PlotMarkers With(string plotId, string marker)
        {
            if (string.IsNullOrEmpty(plotId)) return this;

            string wanted = (marker ?? string.Empty).Trim();
            if (wanted.Length == 0) return Without(plotId);

            var byPlot = new Dictionary<string, PlotMarker>(_byPlot);
            byPlot[plotId] = new PlotMarker(plotId, wanted, MarkerSource.SetNow);

            return new PlotMarkers(byPlot);
        }

        public PlotMarkers Without(string plotId)
        {
            if (plotId == null || !_byPlot.ContainsKey(plotId)) return this;

            var byPlot = new Dictionary<string, PlotMarker>(_byPlot);
            byPlot.Remove(plotId);

            return new PlotMarkers(byPlot);
        }

        public IReadOnlyList<PlotMarker> All
        {
            get
            {
                return _byPlot.Values
                    .OrderBy(one => one.PlotId, NaturalOrder.Comparer)
                    .ToList();
            }
        }

        /// <summary>
        /// The markers every plot but this one holds, which are the ones this plot must not be
        /// offered. Two plots sharing a marker share sheet numbers, and the second one asked
        /// for is refused by Revit.
        /// </summary>
        public IReadOnlyList<string> MarkersOfOtherPlots(string plotId)
        {
            return _byPlot.Values
                .Where(one => !string.Equals(one.PlotId, plotId, StringComparison.Ordinal))
                .Select(one => one.Marker)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(one => one, NaturalOrder.Comparer)
                .ToList();
        }

        /// <summary>
        /// Which plot on this panel already holds a marker, empty when none does. It names the
        /// plot, because a warning that only says taken sends somebody scrolling.
        /// </summary>
        public string WhoHolds(string marker, string besidesPlotId)
        {
            string wanted = (marker ?? string.Empty).Trim();
            if (wanted.Length == 0) return string.Empty;

            PlotMarker found = _byPlot.Values
                .Where(one => !string.Equals(one.PlotId, besidesPlotId, StringComparison.Ordinal))
                .FirstOrDefault(one => string.Equals(one.Marker, wanted, StringComparison.Ordinal));

            return found == null ? string.Empty : found.PlotId;
        }

        /// <summary>
        /// The line beside the plot in step 1: the marker and where it came from, or that
        /// there is none and what that costs.
        /// </summary>
        public string WordsFor(string plotId)
        {
            PlotMarker found = For(plotId);

            if (found == null)
            {
                return "No marker. A plot with no marker gets no sheet.";
            }

            return found.Source == MarkerSource.SetNow
                ? "Marker " + found.Marker + ", set now."
                : "Marker " + found.Marker + ", remembered.";
        }
    }
}
