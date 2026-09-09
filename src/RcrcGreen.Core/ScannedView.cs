using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One view as the scan found it. View templates come through here too, flagged, because
    /// the model gives them back from the same collector.
    /// </summary>
    public sealed class ScannedView
    {
        public ScannedView(
            string name, string viewTypeName, bool isTemplate, string sheetNumber,
            string familyTypeName = null)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (viewTypeName == null) throw new ArgumentNullException("viewTypeName");

            Name = name;
            ViewTypeName = viewTypeName;
            IsTemplate = isTemplate;
            SheetNumber = sheetNumber ?? string.Empty;
            FamilyTypeName = familyTypeName ?? string.Empty;
        }

        public string Name { get; }

        /// <summary>
        /// FloorPlan, Section, Schedule and so on. Revit's own kind for the view, which is not
        /// the same thing as the name of the view family type it was made with.
        /// </summary>
        public string ViewTypeName { get; }

        public bool IsTemplate { get; }

        /// <summary>
        /// The sheet this view is placed on, or empty when it is on none. A template is never
        /// on a sheet.
        /// </summary>
        public string SheetNumber { get; }

        /// <summary>
        /// The view family type this view was made with, which is what a new view of the same
        /// kind copies. Empty when the scan could not read one.
        /// </summary>
        public string FamilyTypeName { get; }

        public bool OnSheet
        {
            get { return SheetNumber.Length > 0; }
        }
    }
}
