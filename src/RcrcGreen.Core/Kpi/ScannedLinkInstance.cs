using System;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One RevitLinkInstance, which is a placed copy of a link type. One type can be placed
    /// more than once, so a loaded document is read once per instance and the report says
    /// when two instances share one.
    /// </summary>
    public sealed class ScannedLinkInstance
    {
        public ScannedLinkInstance(string name, string typeName, bool isLoaded)
        {
            if (name == null) throw new ArgumentNullException("name");

            Name = name;
            TypeName = typeName ?? string.Empty;
            IsLoaded = isLoaded;
        }

        public string Name { get; }

        public string TypeName { get; }

        /// <summary>
        /// True when the instance handed back a document to read. An instance of a loaded
        /// type can still hand back nothing, which is why this is read off the instance.
        /// </summary>
        public bool IsLoaded { get; }

        public bool HoldsTheMark
        {
            get { return KpiNames.Holds(Name, KpiNames.LinkMark) || KpiNames.Holds(TypeName, KpiNames.LinkMark); }
        }
    }
}
