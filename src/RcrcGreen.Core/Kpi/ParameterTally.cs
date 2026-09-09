using System;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One parameter name over a set of elements: how many carry it and how many of those hold
    /// a value that is not empty.
    ///
    /// The two counts are kept apart because a parameter every sheet carries and none fills
    /// in is the ordinary state of a shared parameter, and it answers nothing.
    /// </summary>
    public sealed class ParameterTally
    {
        public ParameterTally(string name, int carrying, int withValue)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (carrying < 0) throw new ArgumentOutOfRangeException("carrying");
            if (withValue < 0 || withValue > carrying) throw new ArgumentOutOfRangeException("withValue");

            Name = name;
            Carrying = carrying;
            WithValue = withValue;
        }

        public string Name { get; }

        public int Carrying { get; }

        public int WithValue { get; }
    }
}
