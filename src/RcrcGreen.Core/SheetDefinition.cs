using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One sheet the user described, repeated across every ticked plot.
    ///
    /// It used to be read off a sheet that already existed. The user picked one with no views
    /// on it and got an empty sheet, and copying an existing layout was not how they wanted to
    /// work anyway. So it is filled from four choices instead: which title block type, what the
    /// sheet is called, which of the ticked view types go on it, and how many per sheet.
    ///
    /// The sheet number is not in here. It is typed per plot, because it is the one thing that
    /// differs between the sheets this definition makes, and the tool invents neither it nor
    /// the name.
    ///
    /// A run can carry several of these, so one press can give a plot its LIST OF DRAWINGS and
    /// its GENERAL ARRANGEMENT LAYOUT together.
    /// </summary>
    public sealed class SheetDefinition
    {
        public SheetDefinition(
            string titleBlockFamilyName,
            string titleBlockTypeName,
            string sheetName,
            IEnumerable<ViewType> views,
            int viewsPerSheet)
        {
            if (titleBlockFamilyName == null) throw new ArgumentNullException("titleBlockFamilyName");
            if (titleBlockTypeName == null) throw new ArgumentNullException("titleBlockTypeName");

            TitleBlockFamilyName = titleBlockFamilyName;
            TitleBlockTypeName = titleBlockTypeName;
            SheetName = (sheetName ?? string.Empty).Trim();

            Views = (views ?? Enumerable.Empty<ViewType>())
                .Where(one => one != null)
                .Distinct()
                .OrderBy(one => one)
                .ToList();

            ViewsPerSheet = SheetLayout.IsACount(viewsPerSheet) ? viewsPerSheet : 1;
        }

        public string TitleBlockFamilyName { get; }

        public string TitleBlockTypeName { get; }

        /// <summary>
        /// Typed by the user, or picked off the list of names the model already uses. Never
        /// invented, so a definition without one makes no sheet.
        /// </summary>
        public string SheetName { get; }

        /// <summary>
        /// The view types that go on this sheet, for whichever plot it is made for. Ticked from
        /// the types already ticked in step 2, so a sheet can only carry a view the run either
        /// makes or finds.
        /// </summary>
        public IReadOnlyList<ViewType> Views { get; }

        public int ViewsPerSheet { get; }

        public string TitleBlock
        {
            get { return (TitleBlockFamilyName + " " + TitleBlockTypeName).Trim(); }
        }

        /// <summary>
        /// A sheet is created with a title block and given a name. Without either there is
        /// nothing to make, and neither is guessed.
        /// </summary>
        public bool CanBeUsed
        {
            get { return TitleBlockTypeName.Length > 0 && SheetName.Length > 0; }
        }

        /// <summary>
        /// Empty when it can be used. Otherwise it names which choice is missing, because
        /// "incomplete" sends somebody looking across four controls.
        /// </summary>
        public string WhatIsMissing
        {
            get
            {
                if (TitleBlockTypeName.Length == 0 && SheetName.Length == 0) return "a sheet type and a sheet name";
                if (TitleBlockTypeName.Length == 0) return "a sheet type";
                if (SheetName.Length == 0) return "a sheet name";
                return string.Empty;
            }
        }

        /// <summary>
        /// More views ticked than fit on one sheet. The run makes one sheet per definition, so
        /// the ones past the count are left off and the report says which.
        /// </summary>
        public IReadOnlyList<ViewType> Placed
        {
            get { return Views.Take(ViewsPerSheet).ToList(); }
        }

        public IReadOnlyList<ViewType> LeftOff
        {
            get { return Views.Skip(ViewsPerSheet).ToList(); }
        }

        /// <summary>
        /// What the panel and the confirmation say about this sheet before anything is made. An
        /// empty sheet is a real thing to ask for, so it says so plainly rather than refusing.
        /// </summary>
        public string InWords()
        {
            if (!CanBeUsed) return "This sheet is missing " + WhatIsMissing + ", so none is made.";

            string what = SheetName + " on " + TitleBlock;

            if (Views.Count == 0)
            {
                return what + ", with no views ticked, so every sheet it makes will be empty.";
            }

            string carrying = what + ", " + ViewsPerSheet
                + (ViewsPerSheet == 1 ? " view per sheet: " : " views per sheet: ")
                + string.Join(", ", Placed.Select(one => one.ToString()).ToArray());

            if (LeftOff.Count == 0) return carrying + ".";

            return carrying + ". " + LeftOff.Count + " more ticked than fit, left off: "
                + string.Join(", ", LeftOff.Select(one => one.ToString()).ToArray()) + ".";
        }

        public override string ToString()
        {
            return SheetName.Length == 0 ? "(unnamed sheet)" : SheetName;
        }
    }
}
