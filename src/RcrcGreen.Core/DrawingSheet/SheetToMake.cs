using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One sheet the run will make: one plot, one slice of the ticked views, one name and one
    /// number.
    ///
    /// A definition used to carry a single typed name and make one sheet per plot, which put
    /// GENERAL ARRANGEMENT LAYOUT onto a sheet holding a location key plan. The name and the
    /// number belong to the individual sheet now. Both are proposals the user can change, and
    /// this row remembers whether each was generated or typed so the report can say.
    /// </summary>
    public sealed class SheetToMake
    {
        public SheetToMake(
            string plotId,
            string sheetNumber,
            string sheetName,
            IEnumerable<ViewType> views,
            int viewsPerSheet,
            string titleBlockFamilyName,
            string titleBlockTypeName,
            bool nameWasGenerated,
            bool numberWasGenerated)
        {
            if (plotId == null) throw new ArgumentNullException("plotId");

            PlotId = plotId;
            SheetNumber = (sheetNumber ?? string.Empty).Trim();
            SheetName = (sheetName ?? string.Empty).Trim();

            // In the order they were ticked, which is the order they go onto the sheet.
            Views = (views ?? Enumerable.Empty<ViewType>())
                .Where(one => one != null)
                .ToList();

            ViewsPerSheet = SheetLayout.IsACount(viewsPerSheet) ? viewsPerSheet : 1;
            TitleBlockFamilyName = titleBlockFamilyName ?? string.Empty;
            TitleBlockTypeName = titleBlockTypeName ?? string.Empty;
            NameWasGenerated = nameWasGenerated && SheetName.Length > 0;
            NumberWasGenerated = numberWasGenerated && SheetNumber.Length > 0;
        }

        public string PlotId { get; }

        public string SheetNumber { get; }

        public string SheetName { get; }

        public IReadOnlyList<ViewType> Views { get; }

        /// <summary>
        /// The grid this sheet lays out on. The last sheet of a definition can hold fewer views
        /// than this, and they still sit in the first cells of the same grid.
        /// </summary>
        public int ViewsPerSheet { get; }

        public string TitleBlockFamilyName { get; }

        public string TitleBlockTypeName { get; }

        public string TitleBlock
        {
            get { return (TitleBlockFamilyName + " " + TitleBlockTypeName).Trim(); }
        }

        /// <summary>
        /// True when the name is the one built from the sheet's single view and the user left
        /// it alone. The report says which were generated and which were typed.
        /// </summary>
        public bool NameWasGenerated { get; }

        public bool NumberWasGenerated { get; }

        public bool HasName
        {
            get { return SheetName.Length > 0; }
        }

        public bool HasNumber
        {
            get { return SheetNumber.Length > 0; }
        }

        public bool CanBeMade
        {
            get { return HasName && HasNumber && TitleBlockTypeName.Length > 0; }
        }

        /// <summary>
        /// Which of the two per-sheet blanks is still empty. The title block is the
        /// definition's to miss, not this row's.
        /// </summary>
        public string WhatIsMissing
        {
            get
            {
                if (!HasName && !HasNumber) return "a name and a number";
                if (!HasName) return "a name";
                if (!HasNumber) return "a number";
                return string.Empty;
            }
        }

        public string ViewsInWords()
        {
            return Views.Count == 0
                ? "no views"
                : string.Join(", ", Views.Select(one => one.ToString()).ToArray());
        }

        /// <summary>
        /// Where the name and the number came from, for the report. A generated value is a
        /// proposal the user chose not to change, and saying so is what makes that checkable.
        /// </summary>
        public string ProvenanceInWords()
        {
            return "The name was " + (NameWasGenerated ? "built from its view" : "typed")
                + " and the number was "
                + (NumberWasGenerated ? "proposed from the plot's own numbering" : "typed") + ".";
        }

        public override string ToString()
        {
            return (SheetNumber + " " + SheetName).Trim();
        }
    }

    /// <summary>
    /// Everything one described sheet makes across the ticked plots: the shared definition and
    /// one row per sheet.
    /// </summary>
    public sealed class SheetBatch
    {
        public SheetBatch(SheetDefinition definition, IEnumerable<SheetToMake> rows)
        {
            if (definition == null) throw new ArgumentNullException("definition");

            Definition = definition;
            Rows = (rows ?? Enumerable.Empty<SheetToMake>())
                .Where(one => one != null)
                .ToList();
        }

        public SheetDefinition Definition { get; }

        public IReadOnlyList<SheetToMake> Rows { get; }

        public int WillBeMade
        {
            get { return Definition.CanBeUsed ? Rows.Count(one => one.CanBeMade) : 0; }
        }

        public int StillMissingSomething
        {
            get { return Rows.Count(one => !one.CanBeMade); }
        }

        /// <summary>
        /// Rows where the user typed a name or a number over the proposal. Those are the only
        /// thing Remove loses that a redraw cannot bring back, so they are what it asks about.
        /// </summary>
        public int RowsWithTypedText
        {
            get
            {
                return Rows.Count(one =>
                    (one.HasName && !one.NameWasGenerated)
                    || (one.HasNumber && !one.NumberWasGenerated));
            }
        }

        /// <summary>
        /// The question Remove puts up, or nothing when there is nothing typed to lose. A
        /// definition whose rows all carry proposals is taken out without asking, because the
        /// proposals come back the moment it is added again.
        /// </summary>
        public string WhyRemovalAsks()
        {
            int typed = RowsWithTypedText;
            if (typed == 0) return string.Empty;

            return "Removing this sheet loses the names or numbers typed on "
                + (typed == 1 ? "1 row" : typed + " rows") + ". Remove it anyway?";
        }

        /// <summary>
        /// The line over the definition's table, so how much this one press makes is on screen
        /// before the confirmation.
        /// </summary>
        public string InWords()
        {
            if (!Definition.CanBeUsed)
            {
                return "Missing " + Definition.WhatIsMissing + ", so it makes nothing.";
            }

            if (Rows.Count == 0)
            {
                return "Makes no sheets. Tick a view for it, and tick a plot in step 1.";
            }

            string making = WillBeMade == 1 ? "Makes 1 sheet" : "Makes " + WillBeMade + " sheets";

            return StillMissingSomething == 0
                ? making + " across the ticked plots."
                : making + " across the ticked plots. " + StillMissingSomething
                    + (StillMissingSomething == 1
                        ? " row still needs a name or a number."
                        : " rows still need a name or a number.");
        }
    }
}
