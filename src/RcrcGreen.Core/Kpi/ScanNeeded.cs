using System;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Where a press of Create got its scan of the model: read on this press, or the one
    /// already held. Said on the pane and in the report either way, with why.
    ///
    /// The same shape as <see cref="ReadingsSource"/>, because the two answer the same kind
    /// of question about two different reads and a second shape would be a second rule.
    /// </summary>
    public sealed class ScanSource
    {
        private ScanSource(bool held, string why)
        {
            Held = held;
            Why = why ?? string.Empty;
        }

        public static ScanSource Read(string why)
        {
            return new ScanSource(false, why);
        }

        public static ScanSource Reused(string why)
        {
            return new ScanSource(true, why);
        }

        /// <summary>
        /// True when the scan already held answers this press, so nothing was read.
        /// </summary>
        public bool Held { get; }

        public string Why { get; }
    }

    /// <summary>
    /// Whether Create has to scan the model before it fills anything.
    ///
    /// **The scan is a step inside Create rather than a button somebody presses.** Pressing
    /// Create used to want a scan the user had to know to do first, which is the tool's
    /// business and not theirs. Create reads the model when there is nothing to read from,
    /// or when what it holds is not this model, and otherwise uses what it has.
    ///
    /// Only the title decides it. A model edited between the scan and the press is the one
    /// thing this cannot see, which is why the press says which of the two happened rather
    /// than leaving the numbers' source to be reasoned about.
    /// </summary>
    public static class ScanNeeded
    {
        public static ScanSource Decide(string heldScanTitle, string documentTitle)
        {
            string held = heldScanTitle ?? string.Empty;
            string open = documentTitle ?? string.Empty;

            if (held.Length == 0) return ScanSource.Read("no scan was held, so the model was read");

            if (!string.Equals(held, open, StringComparison.Ordinal))
            {
                return ScanSource.Read("the scan held is of " + Said(held) + " and this press is on "
                    + Said(open) + ", so the model was read");
            }

            return ScanSource.Reused("the scan held is of this model, so it was not read again");
        }

        private static string Said(string title)
        {
            return title.Length == 0 ? "no model" : title;
        }
    }
}
