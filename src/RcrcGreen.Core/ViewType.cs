using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// A code paired with a view name. This is the column of the grid.
    /// </summary>
    public sealed class ViewType : IEquatable<ViewType>, IComparable<ViewType>
    {
        public ViewType(string code, string viewName)
        {
            if (code == null) throw new ArgumentNullException("code");
            if (viewName == null) throw new ArgumentNullException("viewName");

            Code = code;
            ViewName = viewName;
        }

        public string Code { get; }

        public string ViewName { get; }

        public bool Equals(ViewType other)
        {
            if (ReferenceEquals(other, null)) return false;
            return string.Equals(Code, other.Code, StringComparison.Ordinal)
                && string.Equals(ViewName, other.ViewName, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ViewType);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Code.GetHashCode() * 397) ^ ViewName.GetHashCode();
            }
        }

        public int CompareTo(ViewType other)
        {
            if (ReferenceEquals(other, null)) return 1;
            int byCode = string.CompareOrdinal(Code, other.Code);
            if (byCode != 0) return byCode;
            return string.CompareOrdinal(ViewName, other.ViewName);
        }

        public override string ToString()
        {
            return "(" + Code + ") " + ViewName;
        }
    }
}
