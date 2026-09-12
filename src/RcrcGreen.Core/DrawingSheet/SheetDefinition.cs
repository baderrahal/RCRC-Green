using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// The shared half of a set of sheets: which title block type, which of the ticked view
    /// types go on, and how many per sheet.
    ///
    /// It used to carry one typed sheet name and make one sheet per plot, leaving every view
    /// past the count off it. That put a sheet called GENERAL ARRANGEMENT LAYOUT around a
    /// location key plan and left five schedules unmade with only a report line to say so. The
    /// views are divided into as many sheets as they need now, and each sheet's name and number
    /// live on its own row in <see cref="SheetToMake"/>.
    /// </summary>
    public sealed class SheetDefinition
    {
        public SheetDefinition(
            string titleBlockFamilyName,
            string titleBlockTypeName,
            IEnumerable<ViewType> views,
            int viewsPerSheet)
        {
            if (titleBlockFamilyName == null) throw new ArgumentNullException("titleBlockFamilyName");
            if (titleBlockTypeName == null) throw new ArgumentNullException("titleBlockTypeName");

            TitleBlockFamilyName = titleBlockFamilyName;
            TitleBlockTypeName = titleBlockTypeName;

            // Kept in the order they were ticked, because that is the order they go onto
            // sheets, and deduplicated by hand so that order cannot silently change.
            var seen = new HashSet<ViewType>();
            var wanted = new List<ViewType>();
            foreach (ViewType one in (views ?? Enumerable.Empty<ViewType>()).Where(one => one != null))
            {
                if (seen.Add(one)) wanted.Add(one);
            }

            Views = wanted;
            ViewsPerSheet = SheetLayout.IsACount(viewsPerSheet) ? viewsPerSheet : 1;
        }

        public string TitleBlockFamilyName { get; }

        public string TitleBlockTypeName { get; }

        /// <summary>
        /// The view types that go onto this definition's sheets, in the order they were ticked.
        /// Ticked from the types already ticked in step 2, so a sheet can only carry a view the
        /// run either makes or finds.
        /// </summary>
        public IReadOnlyList<ViewType> Views { get; }

        public int ViewsPerSheet { get; }

        public string TitleBlock
        {
            get { return (TitleBlockFamilyName + " " + TitleBlockTypeName).Trim(); }
        }

        /// <summary>
        /// A sheet is created with a title block, and the title block is the one choice every
        /// sheet of the definition shares. The names and the numbers are per sheet.
        /// </summary>
        public bool CanBeUsed
        {
            get { return TitleBlockTypeName.Length > 0; }
        }

        public string WhatIsMissing
        {
            get { return CanBeUsed ? string.Empty : "a title block"; }
        }

        /// <summary>
        /// The sheets the ticked views need, each holding its share in ticked order. Six views
        /// at two per sheet is three sheets, and no view is ever left off.
        /// </summary>
        public IReadOnlyList<PlannedSheet> Planned
        {
            get { return SheetDivision.Of(Views, ViewsPerSheet); }
        }

        /// <summary>
        /// What the panel and the confirmation say about this definition before anything is
        /// made.
        /// </summary>
        public string InWords()
        {
            if (!CanBeUsed) return "This sheet is missing a title block, so none is made.";

            if (Views.Count == 0)
            {
                return "On " + TitleBlock + ", no views, 1 empty sheet per ticked plot, the "
                    + "way a title sheet is. Type its name and its number.";
            }

            int sheets = Planned.Count;

            return "On " + TitleBlock + ", " + ViewsPerSheet
                + (ViewsPerSheet == 1 ? " view per sheet" : " views per sheet")
                + ", " + (sheets == 1 ? "1 sheet" : sheets + " sheets") + " per ticked plot: "
                + string.Join(", ", Views.Select(one => one.ToString()).ToArray()) + ".";
        }

        public override string ToString()
        {
            return TitleBlock.Length == 0 ? "(no title block)" : TitleBlock;
        }
    }
}
