using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One title block type the model holds, offered under Title block when a sheet is described.
    ///
    /// The user's model has AR-PRX-Title_Block_A1 with types including KEYPLAN, LOD, SCHEDULES,
    /// GA-DETAILED DESIGN and SECTION WITH KEYPLAN. That list is read from the model every
    /// refresh and none of it is written down anywhere here, because the next project will have
    /// its own.
    /// </summary>
    public sealed class TitleBlockType : IComparable<TitleBlockType>
    {
        public TitleBlockType(string familyName, string typeName)
        {
            if (familyName == null) throw new ArgumentNullException("familyName");
            if (typeName == null) throw new ArgumentNullException("typeName");

            FamilyName = familyName;
            TypeName = typeName;
        }

        public string FamilyName { get; }

        public string TypeName { get; }

        public int CompareTo(TitleBlockType other)
        {
            if (ReferenceEquals(other, null)) return 1;

            int byFamily = string.CompareOrdinal(FamilyName, other.FamilyName);
            return byFamily != 0 ? byFamily : string.CompareOrdinal(TypeName, other.TypeName);
        }

        public override string ToString()
        {
            return FamilyName.Length == 0 ? TypeName : FamilyName + "   " + TypeName;
        }
    }
}
