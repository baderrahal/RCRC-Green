using System;
using System.Collections.Generic;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Where each named list was scrolled to, kept across the redraws that throw the list
    /// away. 155 tick boxes threw themselves back to the top on every tick, because the plot
    /// list is a new viewer on every redraw and every tick redraws. The Drawing Sheet fixed
    /// the same fault on its lists with a remembered offset per list, and this is the rule
    /// half of that, apart from the WPF wiring: a note on every scroll, and a restore wanted
    /// only when something above the top was noted, because restoring nought is a scroll to
    /// the top that reads as though it worked.
    /// </summary>
    public sealed class ScrollMemory
    {
        private readonly Dictionary<string, double> _down = new Dictionary<string, double>(StringComparer.Ordinal);
        private readonly Dictionary<string, double> _across = new Dictionary<string, double>(StringComparer.Ordinal);

        public void Note(string list, double down, double across)
        {
            if (list == null) throw new ArgumentNullException("list");

            _down[list] = down;
            _across[list] = across;
        }

        /// <summary>
        /// True when the list was last noted somewhere other than its top left, with both
        /// offsets to restore. A list never noted, or noted at nought, wants nothing.
        /// </summary>
        public bool Wants(string list, out double down, out double across)
        {
            down = 0.0;
            across = 0.0;
            if (list == null) return false;

            double wasDown;
            double wasAcross;
            bool hadDown = _down.TryGetValue(list, out wasDown) && wasDown > 0.0;
            bool hadAcross = _across.TryGetValue(list, out wasAcross) && wasAcross > 0.0;
            if (!hadDown && !hadAcross) return false;

            down = hadDown ? wasDown : 0.0;
            across = hadAcross ? wasAcross : 0.0;
            return true;
        }
    }
}
