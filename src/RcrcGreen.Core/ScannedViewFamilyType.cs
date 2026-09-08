using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One view family type as the scan found it.
    ///
    /// This section exists because it was missing. The tool matched a view family type by name,
    /// on the belief that the type for (010) Location Key Plan is called (010) Location Key
    /// Plan. It is called (010) Key Location Plan. The words are swapped, three of the four
    /// refusals in the first real run were that, and nothing in the scan would have shown it.
    /// A report that lists the templates and not the types cannot answer the question that
    /// needed a Properties panel to answer.
    /// </summary>
    public sealed class ScannedViewFamilyType
    {
        public ScannedViewFamilyType(string name, string viewFamily)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (viewFamily == null) throw new ArgumentNullException("viewFamily");

            Name = name;
            ViewFamily = viewFamily;
        }

        public string Name { get; }

        /// <summary>
        /// FloorPlan, Section, Schedule and so on. It is what says whether a type can be made
        /// by ViewPlan.Create at all.
        /// </summary>
        public string ViewFamily { get; }
    }
}
