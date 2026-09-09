using System;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One RevitLinkType: the file the link points at, whether it is loaded, and whether it
    /// came in nested inside another link.
    /// </summary>
    public sealed class ScannedLinkType
    {
        public ScannedLinkType(string name, string status, bool isLoaded, bool isNested)
        {
            if (name == null) throw new ArgumentNullException("name");

            Name = name;
            Status = status ?? string.Empty;
            IsLoaded = isLoaded;
            IsNested = isNested;
        }

        public string Name { get; }

        /// <summary>
        /// Revit's own word for the state, such as Loaded, Unloaded or NotFound, kept as text
        /// so a state this code did not expect still prints.
        /// </summary>
        public string Status { get; }

        public bool IsLoaded { get; }

        public bool IsNested { get; }

        public bool HoldsTheMark
        {
            get { return KpiNames.Holds(Name, KpiNames.LinkMark); }
        }
    }
}
