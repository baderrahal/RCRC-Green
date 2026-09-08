using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One view as the scan found it. View templates come through here too, flagged, because
    /// the model gives them back from the same collector.
    /// </summary>
    public sealed class ScannedView
    {
        public ScannedView(string name, string viewTypeName, bool isTemplate, string sheetNumber)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (viewTypeName == null) throw new ArgumentNullException("viewTypeName");

            Name = name;
            ViewTypeName = viewTypeName;
            IsTemplate = isTemplate;
            SheetNumber = sheetNumber ?? string.Empty;
        }

        public string Name { get; }

        public string ViewTypeName { get; }

        public bool IsTemplate { get; }

        /// <summary>
        /// The sheet this view is placed on, or empty when it is on none. A template is never
        /// on a sheet.
        /// </summary>
        public string SheetNumber { get; }

        public bool OnSheet
        {
            get { return SheetNumber.Length > 0; }
        }
    }
}
