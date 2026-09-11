using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One cell the tool writes, and the test of whether what it holds is something the tool
    /// would have written there. A mark is the only thing that can name a workbook as filled.
    /// </summary>
    public sealed class FilledMark
    {
        private readonly Func<string, bool> _reads;

        internal FilledMark(string cell, string what, Func<string, bool> reads)
        {
            Cell = cell;
            What = what;
            _reads = reads;
        }

        public string Cell { get; }

        /// <summary>
        /// What the tool writes here, as the reason says it.
        /// </summary>
        public string What { get; }

        /// <summary>
        /// Whether the text is something the tool would have written into this cell. Empty is
        /// never that, and neither is a cell that is not in the file at all.
        /// </summary>
        public bool Reads(string text)
        {
            return !string.IsNullOrWhiteSpace(text) && _reads(text.Trim());
        }
    }

    /// <summary>
    /// The one cell that decided a workbook is filled, and what it held, so a workbook the
    /// tool withheld can be traced in one line rather than by opening it.
    /// </summary>
    public sealed class FilledCell
    {
        internal FilledCell(FilledMark mark, string holds)
        {
            Mark = mark;
            Holds = (holds ?? string.Empty).Trim();
        }

        public FilledMark Mark { get; }

        public string Holds { get; }

        public string InWords
        {
            get { return Mark.Cell + " holds " + Holds + ", which is " + Mark.What + " the tool writes."; }
        }
    }

    /// <summary>
    /// What tells a checklist the tool filled from a template it has not touched.
    ///
    /// **The first rule was one cell against one string and it was wrong.** It read a workbook
    /// as filled when E5 held anything other than the template's own placeholder, measured as
    /// the angle bracketed <![CDATA[<Date>]]> on one production template. Two sets say that
    /// cannot decide anything. The KPI CHECKLIST R1 set holds a placeholder in each cell the
    /// team fills, E5, G5, H5 and C5. An earlier production set holds NOTHING at E5, G5 or C5
    /// and real values at D3 and H5, its D3 reading Future Park and its E4 KING ABDULLAH South.
    /// So a placeholder is one set's habit rather than a rule, a clean template can hold real
    /// text in a mapped cell, and an empty cell is not a placeholder either. Under the old rule
    /// a clean template from the second set was withheld from the picker as filled.
    ///
    /// So a workbook is filled when a cell the tool writes holds something the tool would have
    /// written, never when a cell merely differs from one expected string, and **nothing is
    /// ever withheld on a cell the tool has never written.** Two cells can decide it: the date
    /// the team types on the pane, and the plot reference read off the plot's first sheet. The
    /// other cells the tool writes cannot, because a clean template already holds real text in
    /// some of them and a number cell says nothing about who put the number there.
    ///
    /// A filled workbook where the tool wrote neither of the two reads as a template. That is
    /// the stated limit and it is the safe way round: offering a filled file costs a rerun,
    /// withholding a real template leaves the team unable to fill anything at all.
    /// </summary>
    public static class FilledMarks
    {
        /// <summary>
        /// E5, the date the team types on the pane and the tool copies through. A template
        /// holds a placeholder or nothing there, and neither reads as a date.
        /// </summary>
        public static readonly FilledMark Date =
            new FilledMark(KpiTemplates.TypedByTheTeam[0], "a date", ReadsAsADate);

        /// <summary>
        /// The marks that can name a workbook of this template as filled. The reference mark
        /// takes its cell off the template rather than holding a second copy of it.
        /// </summary>
        public static IReadOnlyList<FilledMark> For(KpiTemplate template)
        {
            if (template == null) throw new ArgumentNullException("template");

            var marks = new List<FilledMark> { Date };

            MappedCell reference = template.CellFor(KpiValue.Reference);
            if (reference != null)
            {
                marks.Add(new FilledMark(reference.Cell, "a plot reference", ReadsAsAReference));
            }

            return marks;
        }

        /// <summary>
        /// Every cell any mark of any template reads, which is what the peek asks the file for
        /// before a template is known.
        /// </summary>
        public static IReadOnlyList<string> CellsRead
        {
            get
            {
                return KpiTemplates.All
                    .SelectMany(template => For(template))
                    .Select(mark => mark.Cell)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
        }

        /// <summary>
        /// The first mark whose cell holds something the tool would have written, or nothing
        /// when this workbook reads as a template.
        /// </summary>
        public static FilledCell Decide(KpiTemplate template, IReadOnlyDictionary<string, string> cells)
        {
            if (template == null) throw new ArgumentNullException("template");
            if (cells == null) return null;

            foreach (FilledMark mark in For(template))
            {
                string holds;
                if (!cells.TryGetValue(mark.Cell, out holds)) continue;
                if (mark.Reads(holds)) return new FilledCell(mark, holds);
            }

            return null;
        }

        /// <summary>
        /// A date has at least two numbers in it. Parsing alone is not enough: the invariant
        /// culture reads a bare 10 as a day of this month, and a template holding a lone number
        /// where a date goes would have been withheld for it.
        /// </summary>
        private static bool ReadsAsADate(string text)
        {
            DateTime when;
            return Numbers(text) >= 2
                && DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out when);
        }

        /// <summary>
        /// A plot reference is one run of letters, digits and punctuation with no space in it,
        /// holding at least one letter and at least one digit. Both parameters the team picks
        /// for it read that way, PRX_Plot_ID as DM-12 and PRX_Plot_UID2 as ANH-007-MO-100019.
        /// An angle bracket is refused outright because every placeholder measured in either
        /// template set carries them and the tool writes none.
        /// </summary>
        private static bool ReadsAsAReference(string text)
        {
            return text.IndexOf('<') < 0
                && text.IndexOf('>') < 0
                && !text.Any(char.IsWhiteSpace)
                && text.Any(char.IsLetter)
                && text.Any(char.IsDigit);
        }

        private static int Numbers(string text)
        {
            int runs = 0;
            bool inside = false;

            foreach (char one in text)
            {
                if (char.IsDigit(one))
                {
                    if (!inside) runs++;
                    inside = true;
                    continue;
                }

                inside = false;
            }

            return runs;
        }
    }
}
