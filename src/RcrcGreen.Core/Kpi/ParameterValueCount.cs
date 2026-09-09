using System;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One distinct value of one parameter over a set of elements, with how many hold it. The
    /// values of a parameter called Botanical Name are the tree list, and the values of one
    /// called Status are what separates existing from proposed, if anything does.
    /// </summary>
    public sealed class ParameterValueCount
    {
        public ParameterValueCount(string parameterName, string value, int count, bool onType)
        {
            if (parameterName == null) throw new ArgumentNullException("parameterName");
            if (count < 0) throw new ArgumentOutOfRangeException("count");

            ParameterName = parameterName;
            Value = value ?? string.Empty;
            Count = count;
            OnType = onType;
        }

        public string ParameterName { get; }

        public string Value { get; }

        /// <summary>
        /// Elements, not parameters. A value on a type counts once for every element of that
        /// type, so the counts under one name add up to elements and never past them.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// True when the value sat on the element's type rather than on the element. A shared
        /// parameter bound to the family and again to the project puts one name in both
        /// places, and summing the two under one name counted 114 values over 57 trees.
        /// </summary>
        public bool OnType { get; }
    }
}
