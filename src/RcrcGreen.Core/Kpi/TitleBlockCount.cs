using System;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One title block family and type, and how many sheets carry an instance of it.
    /// </summary>
    public sealed class TitleBlockCount
    {
        public TitleBlockCount(string familyName, string typeName, int instances)
        {
            if (familyName == null) throw new ArgumentNullException("familyName");
            if (typeName == null) throw new ArgumentNullException("typeName");
            if (instances < 0) throw new ArgumentOutOfRangeException("instances");

            FamilyName = familyName;
            TypeName = typeName;
            Instances = instances;
        }

        public string FamilyName { get; }

        public string TypeName { get; }

        public int Instances { get; }
    }
}
