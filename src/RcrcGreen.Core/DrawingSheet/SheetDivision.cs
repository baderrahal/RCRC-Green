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
        /// The one code every view on this sheet shares, empty when they disagree or there
        /// are none. It is what fronts the built number: a sheet of two 600 schedules is
        /// still a 600 sheet, the way 600QC and 600QD are, and a sheet mixing codes gets no
        /// number built because picking either code would be a guess.
        /// </summary>
        public string SingleCode
        {
            get
            {
                if (Views.Count == 0) return string.Empty;

                string code = Views[0].Code;
                return Views.All(one => string.Equals(one.Code, code, StringComparison.Ordinal))
                    ? code
                    : string.Empty;
            }
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
        /// Why the name box starts empty, said next to it rather than left as a surprise at
        /// Run. Only the name: the number is built whenever the views share one code, and a
        /// row whose number could not be built says why under its own box.
        /// </summary>
        public string WhyNothingIsProposed()
        {
            if (NamedFromItsView) return string.Empty;

            return Views.Count == 0
                ? "Holds no views, the way a title sheet does, so the name is typed rather "
                    + "than proposed."
                : "Holds " + Views.Count + " views, so the name is typed rather than proposed.";
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

            // A definition with no views plans ONE empty sheet rather than none. The title
            // sheet is a real sheet on both measured models, 0 views on 010001A and on
            // 010QE, and the writer places nothing without complaint. No views meant no
            // rows before, so the one sheet the team starts a set with could not be
            // described at all. A half filled definition still makes nothing, because its
            // row is refused until its typed name and number arrive.
            if (wanted.Count == 0)
            {
                planned.Add(new PlannedSheet(wanted));
                return planned;
            }

            for (int start = 0; start < wanted.Count; start += each)
            {
                planned.Add(new PlannedSheet(wanted.Skip(start).Take(each)));
            }

            // The team's order: the code first, then the order list within a code, then the
            // ticked order for anything not on the list. It is also the letter order, which
            // is why it is applied here, before the numbers are built, rather than at Run.
            return planned
                .Select((sheet, at) => new PlannedInOrder(sheet, at))
                .OrderBy(one => SheetOrder.CodeToOrderBy(one.Sheet.Views), NaturalOrder.Comparer)
                .ThenBy(one => SheetOrder.Position(one.Sheet.ProposedName))
                .ThenBy(one => one.At)
                .Select(one => one.Sheet)
                .ToList();
        }

        /// <summary>
        /// One planned sheet with where it arrived, so the order can fall back to the ticked
        /// order for names the list does not hold.
        /// </summary>
        private sealed class PlannedInOrder
        {
            public PlannedInOrder(PlannedSheet sheet, int at)
            {
                Sheet = sheet;
                At = at;
            }

            public PlannedSheet Sheet { get; }

            public int At { get; }
        }
    }
}
