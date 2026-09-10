using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One distinct value of a parameter, and how many elements carry it.
    /// </summary>
    public sealed class ScannedParameterValue
    {
        public ScannedParameterValue(string value, int elementCount)
        {
            if (value == null) throw new ArgumentNullException("value");

            Value = value;
            ElementCount = elementCount;
        }

        public string Value { get; }

        public int ElementCount { get; }
    }
}
