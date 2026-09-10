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
    /// What the output holds about recalculation, read back off the written file.
    ///
    /// **Excel showed 0 for seven computed cells while the inputs beside them were right.** The
    /// values were never wrong: every formula cell still carried the template's own cached
    /// result, all zero, and Excel trusted the cache rather than recalculating. Measured on that
    /// file, fullCalcOnLoad="1" WAS already present, so the flag alone is not enough, and calcPr
    /// read calcId="191029", which tells Excel the cache was written by an engine as new as its
    /// own. Total Planting Area is =F10, F10 held 410, and the cell showed 0.
    ///
    /// Three things together fix it and all three are checked here: calcId set to 0, every
    /// cached result dropped from every formula cell in every sheet, and xl/calcChain.xml
    /// removed. Forcing a recalculation on that same file gave Total Green cover 1518, Canopy
    /// 1048, Total Trees 31, Planting 410 and Lawn 60.
    /// </summary>
    public sealed class CacheCheck
    {
        public CacheCheck(
            bool recalculatesOnOpen,
            string calcId,
            int formulaCellsCarryingACachedValue,
            bool calcChainRemoved,
            int cachedValuesDropped)
        {
            RecalculatesOnOpen = recalculatesOnOpen;
            CalcId = calcId ?? string.Empty;
            FormulaCellsCarryingACachedValue = formulaCellsCarryingACachedValue;
            CalcChainRemoved = calcChainRemoved;
            CachedValuesDropped = cachedValuesDropped;
        }

        /// <summary>
        /// What a refused patch carries, so nothing reads a zero here as a check that passed.
        /// </summary>
        public static readonly CacheCheck NotChecked =
            new CacheCheck(false, string.Empty, 0, false, 0);

        public bool RecalculatesOnOpen { get; }

        /// <summary>
        /// As the output holds it. Zero is what makes Excel distrust the cache. The template
        /// carried 191029 and that is why the flag alone changed nothing.
        /// </summary>
        public string CalcId { get; }

        public int FormulaCellsCarryingACachedValue { get; }

        public bool CalcChainRemoved { get; }

        public int CachedValuesDropped { get; }

        public bool CalcIdCleared
        {
            get { return string.Equals(CalcId, "0", StringComparison.Ordinal); }
        }

        /// <summary>
        /// All three together, because any one of them on its own leaves Excel free to trust
        /// the cache. This is what the report prints and what a test asserts.
        /// </summary>
        public bool WillRecalculate
        {
            get
            {
                return RecalculatesOnOpen
                    && CalcIdCleared
                    && FormulaCellsCarryingACachedValue == 0
                    && CalcChainRemoved;
            }
        }
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
            IReadOnlyList<LandedCell> landed,
            CacheCheck cache)
        {
            Written = written;
            Refusal = refusal ?? string.Empty;
            PartsInSource = partsInSource;
            PartsInOutput = partsInOutput;
            ChangedParts = changedParts ?? new List<string>();
            Landed = landed ?? new List<LandedCell>();
            Cache = cache ?? CacheCheck.NotChecked;
        }

        /// <summary>
        /// What the output really holds about recalculation, read back off the file the same
        /// way every written cell is. A workbook that opens showing stale zeros beside correct
        /// inputs is the worst thing this tool can produce, because it looks finished.
        /// </summary>
        public CacheCheck Cache { get; }

        public bool Written { get; }

        public string Refusal { get; }

        public int PartsInSource { get; }

        public int PartsInOutput { get; }

        public IReadOnlyList<string> ChangedParts { get; }

        public IReadOnlyList<LandedCell> Landed { get; }

        /// <summary>
        /// True only when every part of the source is in the output but for the calculation
        /// chain, which this tool removes on purpose. A count short by anything else is the
        /// resave failure wearing this tool's name.
        /// </summary>
        public bool KeptEveryPart
        {
            get { return Written && PartsInOutput == PartsInSource - PartsDeliberatelyRemoved; }
        }

        /// <summary>
        /// One: xl/calcChain.xml. Excel rebuilds it on the first recalculation, and leaving it
        /// beside formulas whose cached results have been dropped is a file that disagrees with
        /// itself. The report says which part went and why, so the count does not read as a loss.
        /// </summary>
        public int PartsDeliberatelyRemoved
        {
            get { return Written && Cache.CalcChainRemoved ? 1 : 0; }
        }

        public static PatchOutcome Refused(string why)
        {
            if (why == null) throw new ArgumentNullException("why");
            return new PatchOutcome(false, why, 0, 0, null, null, null);
        }

        public static PatchOutcome Done(
            int partsInSource,
            int partsInOutput,
            IEnumerable<string> changedParts,
            IEnumerable<LandedCell> landed,
            CacheCheck cache)
        {
            return new PatchOutcome(
                true,
                null,
                partsInSource,
                partsInOutput,
                (changedParts ?? Enumerable.Empty<string>()).ToList(),
                (landed ?? Enumerable.Empty<LandedCell>()).Where(cell => cell != null).ToList(),
                cache);
        }
    }
}
