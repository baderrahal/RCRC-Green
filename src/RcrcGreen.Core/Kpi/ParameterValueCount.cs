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
        public ParameterValueCount(string parameterName, string value, int count)
        {
            if (parameterName == null) throw new ArgumentNullException("parameterName");
            if (count < 0) throw new ArgumentOutOfRangeException("count");

            ParameterName = parameterName;
            Value = value ?? string.Empty;
            Count = count;
        }

        public string ParameterName { get; }

        public string Value { get; }

        public int Count { get; }
    }
}
