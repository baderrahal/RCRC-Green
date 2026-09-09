using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Section 4. Every link type and instance, and the filled regions inside each loaded one.
    /// </summary>
    public sealed class LinkFacts
    {
        public LinkFacts(
            IEnumerable<ScannedLinkType> types,
            IEnumerable<ScannedLinkInstance> instances,
            IEnumerable<LinkContents> contents)
        {
            Types = Held(types);
            Instances = Held(instances);
            Contents = Held(contents);
        }

        public IReadOnlyList<ScannedLinkType> Types { get; }

        public IReadOnlyList<ScannedLinkInstance> Instances { get; }

        public IReadOnlyList<LinkContents> Contents { get; }

        public IEnumerable<string> NamesHoldingTheMark
        {
            get
            {
                return Types.Where(type => type.HoldsTheMark).Select(type => type.Name)
                    .Concat(Instances.Where(one => one.HoldsTheMark).Select(one => one.Name))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(name => name, NaturalOrder.Comparer);
            }
        }

        public static LinkFacts Nothing()
        {
            return new LinkFacts(null, null, null);
        }

        private static IReadOnlyList<T> Held<T>(IEnumerable<T> items) where T : class
        {
            if (items == null) return new List<T>();
            return items.Where(item => item != null).ToList();
        }
    }
}
