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
    ///
    /// **A fourth, measured on the 1428 workbook.** All three held and Excel opened the file
    /// showing every written number and every formula cell blank, and Ctrl Alt F9 filled them.
    /// calcPr carried no calcMode. Excel's calculation mode is a session setting and the first
    /// workbook opened in a session sets it, so anyone with a manual workbook open, or manual
    /// in their own options, opened this file into a manual session. calcMode is set to auto
    /// outright now and it is the fifth thing checked. The check is over what the file says
    /// and not over what Excel does with it, which nothing here can run.
    /// </summary>
    public sealed class CacheCheck
    {
        public CacheCheck(
            bool recalculatesOnOpen,
            string calcId,
            int formulaCellsCarryingACachedValue,
            bool calcChainRemoved,
            int cachedValuesDropped,
            string calcMode = null,
            IEnumerable<string> otherCalculationSettings = null)
        {
            RecalculatesOnOpen = recalculatesOnOpen;
            CalcId = calcId ?? string.Empty;
            FormulaCellsCarryingACachedValue = formulaCellsCarryingACachedValue;
            CalcChainRemoved = calcChainRemoved;
            CachedValuesDropped = cachedValuesDropped;
            CalcMode = calcMode ?? string.Empty;
            OtherCalculationSettings = (otherCalculationSettings ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrWhiteSpace(one)).ToList();
        }

        /// <summary>
        /// What a refused patch carries, so nothing reads a zero here as a check that passed.
        /// </summary>
        public static readonly CacheCheck NotChecked =
            new CacheCheck(false, string.Empty, 0, false, 0);

        /// <summary>
        /// The value calcPr carries for calcMode, read back off the output, or empty when it
        /// carries none. Only auto passes.
        /// </summary>
        public string CalcMode { get; }

        public bool CalcModeAuto
        {
            get { return string.Equals(CalcMode, "auto", StringComparison.Ordinal); }
        }

        /// <summary>
        /// Every other place in the package that was found holding a calculation setting: a
        /// sheetCalcPr element in a sheet part, a VBA project, or another attribute on calcPr.
        /// Empty when none was found, and the report says what was looked for either way.
        /// </summary>
        public IReadOnlyList<string> OtherCalculationSettings { get; }

        public const string LookedFor =
            "a sheetCalcPr element in every sheet part, an xl/vbaProject.bin part, and every "
            + "attribute on calcPr other than the three set here";

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
        /// All five together, because any one of them on its own leaves Excel free to trust
        /// the cache or to open the file into a manual session. This is what the report prints
        /// and what a test asserts.
        /// </summary>
        public bool WillRecalculate
        {
            get
            {
                return RecalculatesOnOpen
                    && CalcIdCleared
                    && FormulaCellsCarryingACachedValue == 0
                    && CalcChainRemoved
                    && CalcModeAuto;
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
            CacheCheck cache,
            FormulaCheck formulas)
        {
            Written = written;
            Refusal = refusal ?? string.Empty;
            PartsInSource = partsInSource;
            PartsInOutput = partsInOutput;
            ChangedParts = changedParts ?? new List<string>();
            Landed = landed ?? new List<LandedCell>();
            Cache = cache ?? CacheCheck.NotChecked;
            Formulas = formulas ?? FormulaCheck.NotChecked;
        }

        /// <summary>
        /// What the output's formulas will compute from the cells this run wrote, read off the
        /// output. A run whose written cells leave a formula reading an error is refused and
        /// the output is deleted again, and this carries why. Checked whether or not the run
        /// was refused, so the report has the section either way.
        /// </summary>
        public FormulaCheck Formulas { get; }

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
            return new PatchOutcome(false, why, 0, 0, null, null, null, null);
        }

        /// <summary>
        /// The output was written, read back, found to leave a formula reading an error off a
        /// row this run wrote, and deleted again. The check travels so the report can print
        /// every formula at risk.
        /// </summary>
        public static PatchOutcome RefusedAfterWriting(string why, FormulaCheck formulas)
        {
            if (why == null) throw new ArgumentNullException("why");
            if (formulas == null) throw new ArgumentNullException("formulas");

            return new PatchOutcome(false, why, 0, 0, null, null, null, formulas);
        }

        public static PatchOutcome Done(
            int partsInSource,
            int partsInOutput,
            IEnumerable<string> changedParts,
            IEnumerable<LandedCell> landed,
            CacheCheck cache,
            FormulaCheck formulas = null)
        {
            return new PatchOutcome(
                true,
                null,
                partsInSource,
                partsInOutput,
                (changedParts ?? Enumerable.Empty<string>()).ToList(),
                (landed ?? Enumerable.Empty<LandedCell>()).Where(cell => cell != null).ToList(),
                cache,
                formulas);
        }
    }
}
