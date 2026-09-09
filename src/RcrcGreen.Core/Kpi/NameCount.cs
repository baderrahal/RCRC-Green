using System;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// A name and how many of something carry it. A filled region type, a phase, a workset, a
    /// family and type together.
    /// </summary>
    public sealed class NameCount
    {
        public NameCount(string name, int count)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (count < 0) throw new ArgumentOutOfRangeException("count");

            Name = name;
            Count = count;
        }

        public string Name { get; }

        public int Count { get; }
    }
}
