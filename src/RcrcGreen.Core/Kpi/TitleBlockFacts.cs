using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Everything section 3 of the report prints: the sheets, the title blocks on them, and
    /// the parameters on each of the three places a sheet value can live.
    /// </summary>
    public sealed class TitleBlockFacts
    {
        public TitleBlockFacts(
            int sheetCount,
            int placeholderCount,
            int titleBlockInstances,
            IEnumerable<TitleBlockCount> titleBlocks,
            ParameterHome onInstances,
            ParameterHome onTypes,
            ParameterHome onSheets)
        {
            if (sheetCount < 0) throw new ArgumentOutOfRangeException("sheetCount");
            if (placeholderCount < 0 || placeholderCount > sheetCount) throw new ArgumentOutOfRangeException("placeholderCount");
            if (titleBlockInstances < 0) throw new ArgumentOutOfRangeException("titleBlockInstances");

            SheetCount = sheetCount;
            PlaceholderCount = placeholderCount;
            TitleBlockInstances = titleBlockInstances;
            TitleBlocks = (titleBlocks ?? Enumerable.Empty<TitleBlockCount>()).Where(one => one != null).ToList();
            OnInstances = onInstances ?? ParameterHome.Empty(OnInstancesWhere);
            OnTypes = onTypes ?? ParameterHome.Empty(OnTypesWhere);
            OnSheets = onSheets ?? ParameterHome.Empty(OnSheetsWhere);
        }

        public const string OnInstancesWhere = "title block instance";

        public const string OnTypesWhere = "title block type";

        public const string OnSheetsWhere = "sheet";

        public int SheetCount { get; }

        /// <summary>
        /// A placeholder sheet has no title block, so the title block count and the sheet count
        /// can differ by exactly this and nothing is wrong.
        /// </summary>
        public int PlaceholderCount { get; }

        public int TitleBlockInstances { get; }

        public IReadOnlyList<TitleBlockCount> TitleBlocks { get; }

        public ParameterHome OnInstances { get; }

        public ParameterHome OnTypes { get; }

        public ParameterHome OnSheets { get; }

        public IEnumerable<ParameterHome> Homes
        {
            get
            {
                yield return OnInstances;
                yield return OnTypes;
                yield return OnSheets;
            }
        }

        /// <summary>
        /// False when the read threw and this is the empty fallback. A section that was never
        /// filled in must not print NOT FOUND, because that reads as a measured absence.
        /// </summary>
        public bool WasRead { get; private set; } = true;

        public string WhyNotRead { get; private set; } = string.Empty;

        public static TitleBlockFacts Nothing()
        {
            return new TitleBlockFacts(0, 0, 0, null, null, null, null);
        }

        public static TitleBlockFacts NotRead(string why)
        {
            return new TitleBlockFacts(0, 0, 0, null, null, null, null) { WasRead = false, WhyNotRead = why ?? string.Empty };
        }
    }
}
