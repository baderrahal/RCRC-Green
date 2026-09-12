using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Which markers the model's own sheet numbers already use, and so which ones a plot must
    /// not be offered.
    ///
    /// A real number is the view code, the marker, then at most one sheet letter, so the
    /// marker has to be read back out of the middle. That read is ambiguous on purpose-built
    /// examples: 010QE is marker Q with sheet letter E on DM-11, but nothing in the string
    /// rules out marker QE with no letter, so BOTH readings are counted in use. Barring one
    /// marker too many costs a dropdown entry, and barring one too few costs a refused sheet.
    ///
    /// The one exception is a number whose digits run four or longer with at most one letter
    /// after them, like 010001A. There the three digits before the letters are a marker of
    /// the 001 kind and the letter is the sheet letter, so only 001 is counted. Counting A as
    /// well would bar most of the alphabet on a model numbered the NG03 way, where every plot
    /// carries 010xxxA to D.
    ///
    /// A copy number says nothing about markers: 010QE Copy 001 is DM-11's Q duplicated onto
    /// another plot, which is exactly the read that must not happen.
    /// </summary>
    public sealed class MarkerLedger
    {
        private readonly Dictionary<string, HashSet<string>> _byPlot;

        private readonly HashSet<string> _onNoPlot;

        private MarkerLedger(
            Dictionary<string, HashSet<string>> byPlot, HashSet<string> onNoPlot)
        {
            _byPlot = byPlot;
            _onNoPlot = onNoPlot;
        }

        public static readonly MarkerLedger Nothing = new MarkerLedger(
            new Dictionary<string, HashSet<string>>(StringComparer.Ordinal),
            new HashSet<string>(StringComparer.Ordinal));

        /// <summary>
        /// Read off the model: the numbers each plot's sheets carry, and every number in use,
        /// so a number on a sheet with no PRX_Plot_ID still bars its marker for everyone.
        /// </summary>
        public static MarkerLedger Of(
            IEnumerable<SheetOnAPlot> numbersOnPlots, IEnumerable<string> allNumbersInUse)
        {
            var byPlot = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            var plotted = new HashSet<string>(StringComparer.Ordinal);

            foreach (SheetOnAPlot one in (numbersOnPlots ?? Enumerable.Empty<SheetOnAPlot>())
                .Where(one => one != null && one.PlotId.Length > 0 && one.SheetNumber.Length > 0))
            {
                plotted.Add(one.SheetNumber);

                HashSet<string> held;
                if (!byPlot.TryGetValue(one.PlotId, out held))
                {
                    held = new HashSet<string>(StringComparer.Ordinal);
                    byPlot.Add(one.PlotId, held);
                }

                foreach (string marker in MarkersIn(new[] { one.SheetNumber }))
                {
                    held.Add(marker);
                }
            }

            var onNoPlot = new HashSet<string>(
                MarkersIn((allNumbersInUse ?? Enumerable.Empty<string>())
                    .Where(one => one != null && !plotted.Contains(one.Trim()))),
                StringComparer.Ordinal);

            return new MarkerLedger(byPlot, onNoPlot);
        }

        /// <summary>
        /// The markers this plot must not be offered: every marker another plot's sheets use,
        /// every marker on a sheet with no plot, and every marker another plot holds on the
        /// panel. The plot's own are left in, because Q is exactly what the user will pick
        /// for DM-11.
        /// </summary>
        public IReadOnlyCollection<string> BarredFor(string plotId, PlotMarkers held)
        {
            var barred = new HashSet<string>(_onNoPlot, StringComparer.Ordinal);

            foreach (KeyValuePair<string, HashSet<string>> plot in _byPlot)
            {
                if (string.Equals(plot.Key, plotId, StringComparison.Ordinal)) continue;
                foreach (string marker in plot.Value) barred.Add(marker);
            }

            foreach (string marker in (held ?? PlotMarkers.Nothing).MarkersOfOtherPlots(plotId))
            {
                barred.Add(marker);
            }

            return barred;
        }

        /// <summary>
        /// The warning under a plot whose marker is taken, empty when it is not. A typed
        /// marker is warned about here rather than refused, because the dropdowns are an
        /// offer and never a restriction.
        /// </summary>
        public string Warning(string plotId, PlotMarkers held)
        {
            string marker = (held ?? PlotMarkers.Nothing).MarkerOf(plotId);
            if (marker.Length == 0) return string.Empty;

            string holder = (held ?? PlotMarkers.Nothing).WhoHolds(marker, plotId);
            if (holder.Length > 0)
            {
                return "Marker " + marker + " is set on " + holder
                    + " in step 1 as well, so their sheet numbers would collide.";
            }

            foreach (KeyValuePair<string, HashSet<string>> plot in _byPlot)
            {
                if (string.Equals(plot.Key, plotId, StringComparison.Ordinal)) continue;
                if (plot.Value.Contains(marker))
                {
                    return "Marker " + marker + " is already used by " + plot.Key
                        + "'s sheet numbers in this model.";
                }
            }

            if (_onNoPlot.Contains(marker))
            {
                return "Marker " + marker + " is already used by a sheet number in this model "
                    + "on a sheet carrying no plot.";
            }

            return string.Empty;
        }

        /// <summary>
        /// Every marker the given numbers could be using, copy numbers skipped. Each number
        /// splits into leading digits and a tail: four or more digits with at most one letter
        /// is the 001 kind and gives the last three digits, and a letters tail gives itself
        /// and itself short one letter, because the sheet letter cannot be told from the end
        /// of the marker.
        /// </summary>
        public static IReadOnlyCollection<string> MarkersIn(IEnumerable<string> numbers)
        {
            var found = new HashSet<string>(StringComparer.Ordinal);

            foreach (string number in (numbers ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrWhiteSpace(one))
                .Select(one => one.Trim())
                .Where(one => one.IndexOf(ScannedSheet.CopyMark, StringComparison.Ordinal) < 0))
            {
                int digits = 0;
                while (digits < number.Length && char.IsDigit(number[digits])) digits++;
                if (digits == 0) continue;

                string tail = number.Substring(digits);
                if (tail.Length > 0 && !tail.All(char.IsLetter)) continue;

                if (digits >= 4 && tail.Length <= 1)
                {
                    found.Add(number.Substring(digits - 3, 3));
                    continue;
                }

                if (tail.Length == 0) continue;

                string upper = tail.ToUpperInvariant();
                found.Add(upper);
                if (upper.Length >= 2) found.Add(upper.Substring(0, upper.Length - 1));
            }

            return found;
        }
    }
}
