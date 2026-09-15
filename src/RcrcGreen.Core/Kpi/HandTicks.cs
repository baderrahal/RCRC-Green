using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The plots a person has ticked or unticked BY HAND, held as the one record of that choice.
    ///
    /// **IT USED TO BE A BARE SET OF THE PLOTS TAKEN OFF, AND NOTHING KEPT IT IN STEP WITH THE
    /// TICKS.** Three routes had come apart by the eighty third pass. Unticking a workbook row
    /// removed every plot of that template without asking this list and without recording what it
    /// took, so a plot the person had ticked by hand went with it. Select all ticked every plot
    /// while this list still said one of them was off, so the next row tick dropped it again.
    /// Clear did the same the other way. **A record that disagrees with what is on screen is
    /// worse than no record**, because the pane looks right and the next press acts on the
    /// disagreement.
    ///
    /// **BOTH DIRECTIONS ARE KEPT, WHICH IS WHAT MAKES THE TWO ROW PRESSES SYMMETRIC.** Ticking a
    /// row skips the plots taken off by hand, and unticking a row keeps the plots put on by hand,
    /// so unticking a row undoes exactly what ticking it did and ticking it again restores the
    /// same state. A set of one direction could only ever do half of that.
    ///
    /// It is immutable, the same shape <see cref="PlotTicks"/> already has, so a caller cannot
    /// change it under the pane and every path that moves it hands back a new one.
    /// </summary>
    public sealed class HandTicks
    {
        private readonly HashSet<string> _off;
        private readonly HashSet<string> _on;

        private HandTicks(IEnumerable<string> off, IEnumerable<string> on)
        {
            _off = new HashSet<string>(Cleaned(off), StringComparer.Ordinal);
            _on = new HashSet<string>(Cleaned(on), StringComparer.Ordinal);
        }

        /// <summary>
        /// Nobody has touched a plot by hand. **This is also what Select all and Clear leave
        /// behind**, and what a model change leaves behind, because another model's DM-14 is not
        /// this one's.
        /// </summary>
        public static readonly HandTicks None = new HandTicks(null, null);

        /// <summary>The plots taken off by hand, in the order they were taken off.</summary>
        public IReadOnlyList<string> Off
        {
            get { return _off.ToList(); }
        }

        /// <summary>The plots put on by hand.</summary>
        public IReadOnlyList<string> On
        {
            get { return _on.ToList(); }
        }

        public int Count
        {
            get { return _off.Count + _on.Count; }
        }

        public bool IsOff(string plotId)
        {
            return plotId != null && _off.Contains(plotId);
        }

        public bool IsOn(string plotId)
        {
            return plotId != null && _on.Contains(plotId);
        }

        /// <summary>
        /// The record after a person takes one plot off by hand. **A plot is in one direction or
        /// neither, never both**, so this drops any standing put on for it.
        /// </summary>
        public HandTicks TakenOff(string plotId)
        {
            if (string.IsNullOrWhiteSpace(plotId)) return this;

            return new HandTicks(_off.Concat(new[] { plotId.Trim() }), Without(_on, plotId));
        }

        /// <summary>The record after a person puts one plot on by hand.</summary>
        public HandTicks PutOn(string plotId)
        {
            if (string.IsNullOrWhiteSpace(plotId)) return this;

            return new HandTicks(Without(_off, plotId), _on.Concat(new[] { plotId.Trim() }));
        }

        /// <summary>
        /// The record after a press that replaces every tick. **Select all and Clear both land
        /// here.** Pressing Select all is a person saying they want everything, and pressing
        /// Clear is a person starting over, so neither leaves a per plot choice standing behind
        /// a list that no longer shows it.
        /// </summary>
        public HandTicks Forgotten()
        {
            return None;
        }

        public string InWords
        {
            get
            {
                if (Count == 0) return NothingByHand;

                return Count + (Count == 1 ? " plot" : " plots")
                    + " picked by hand: " + _on.Count + " on, " + _off.Count + " off.";
            }
        }

        public const string NothingByHand = "No plot has been ticked or unticked by hand.";

        private static IEnumerable<string> Without(IEnumerable<string> held, string plotId)
        {
            return held.Where(one => !string.Equals(one, plotId.Trim(), StringComparison.Ordinal));
        }

        private static IEnumerable<string> Cleaned(IEnumerable<string> plots)
        {
            return (plots ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrWhiteSpace(one))
                .Select(one => one.Trim());
        }
    }
}
