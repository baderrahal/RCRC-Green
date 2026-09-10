using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One of the sheets a definition will make, holding its share of the ticked views.
    /// </summary>
    public sealed class PlannedSheet
    {
        public PlannedSheet(IEnumerable<ViewType> views)
        {
            Views = (views ?? Enumerable.Empty<ViewType>())
                .Where(one => one != null)
                .ToList();
        }

        /// <summary>
        /// In the order they were ticked, which is the order they go onto the sheet.
        /// </summary>
        public IReadOnlyList<ViewType> Views { get; }

        /// <summary>
        /// A sheet holding exactly one view is named after it. One holding more has no view to
        /// name it after, so the user types the name and the sheet is not made until they do.
        /// </summary>
        public bool NamedFromItsView
        {
            get { return Views.Count == 1; }
        }

        public string ProposedName
        {
            get { return NamedFromItsView ? SheetNaming.FromView(Views[0]) : string.Empty; }
        }

        /// <summary>
        /// What the views on this sheet are, for the identity of a typed name or number across
        /// a redraw. Two planned sheets holding the same views the same way are the same sheet
        /// to the person typing into its row.
        /// </summary>
        public string Signature
        {
            get { return string.Join("|", Views.Select(one => one.ToString()).ToArray()); }
        }

        public string ViewsInWords()
        {
            return string.Join(", ", Views.Select(one => one.ToString()).ToArray());
        }

        /// <summary>
        /// Why the name and the number boxes start empty, said next to them rather than left as
        /// a surprise at Run.
        /// </summary>
        public string WhyNothingIsProposed()
        {
            return NamedFromItsView
                ? string.Empty
                : "Holds " + Views.Count + " views, so the name and the number are typed rather "
                    + "than proposed.";
        }

        public override string ToString()
        {
            return ViewsInWords();
        }
    }

    /// <summary>
    /// Divides the ticked views into as many sheets as they need.
    ///
    /// A definition used to make ONE sheet and leave every view past the count off it, named in
    /// the report. That read as an error to the person who had ticked six schedules, and it is
    /// not how the model works: plot DM-11 carries eight sheets, each with its own drawing. So
    /// 6 views at 1 per sheet is 6 sheets, at 2 per sheet is 3, at 4 per sheet is 2 with the
    /// second holding 2, and no view is ever left off.
    /// </summary>
    public static class SheetDivision
    {
        public static IReadOnlyList<PlannedSheet> Of(IEnumerable<ViewType> views, int perSheet)
        {
            List<ViewType> wanted = (views ?? Enumerable.Empty<ViewType>())
                .Where(one => one != null)
                .ToList();

            int each = SheetLayout.IsACount(perSheet) ? perSheet : 1;

            var planned = new List<PlannedSheet>();
            for (int start = 0; start < wanted.Count; start += each)
            {
                planned.Add(new PlannedSheet(wanted.Skip(start).Take(each)));
            }

            return planned;
        }
    }
}
