using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One cell as it really is in the written file, read back after the write. What landed,
    /// never what was sent, which is the same rule as the run report in the Drawing Sheet.
    /// </summary>
    public sealed class LandedCell
    {
        public LandedCell(string sheetName, string cell, string value)
        {
            SheetName = sheetName ?? string.Empty;
            Cell = cell ?? string.Empty;
            Value = value ?? string.Empty;
        }

        public string SheetName { get; }

        public string Cell { get; }

        public string Value { get; }
    }

    /// <summary>
    /// What one patch did. Refused before anything was written, or written with the part
    /// counts, the parts that changed and every written cell read back off the output.
    ///
    /// The part counts are here because the failure this guards against is silent: a loaded
    /// and resaved workbook lost 21 of 37 parts, still opened, and looked nearly right.
    /// </summary>
    public sealed class PatchOutcome
    {
        private PatchOutcome(
            bool written,
            string refusal,
            int partsInSource,
            int partsInOutput,
            IReadOnlyList<string> changedParts,
            IReadOnlyList<LandedCell> landed)
        {
            Written = written;
            Refusal = refusal ?? string.Empty;
            PartsInSource = partsInSource;
            PartsInOutput = partsInOutput;
            ChangedParts = changedParts ?? new List<string>();
            Landed = landed ?? new List<LandedCell>();
        }

        public bool Written { get; }

        public string Refusal { get; }

        public int PartsInSource { get; }

        public int PartsInOutput { get; }

        public IReadOnlyList<string> ChangedParts { get; }

        public IReadOnlyList<LandedCell> Landed { get; }

        /// <summary>
        /// True only when every part of the source is in the output and none was added. A
        /// count that differs is the resave failure wearing this tool's name.
        /// </summary>
        public bool KeptEveryPart
        {
            get { return Written && PartsInSource == PartsInOutput; }
        }

        public static PatchOutcome Refused(string why)
        {
            if (why == null) throw new ArgumentNullException("why");
            return new PatchOutcome(false, why, 0, 0, null, null);
        }

        public static PatchOutcome Done(
            int partsInSource, int partsInOutput, IEnumerable<string> changedParts, IEnumerable<LandedCell> landed)
        {
            return new PatchOutcome(
                true,
                null,
                partsInSource,
                partsInOutput,
                (changedParts ?? Enumerable.Empty<string>()).ToList(),
                (landed ?? Enumerable.Empty<LandedCell>()).Where(cell => cell != null).ToList());
        }
    }
}
