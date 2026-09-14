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
    /// **Both marks are live on every run.** The reference used to be skipped when the ticked
    /// plots disagreed on it, which a checklist covering several plots usually did, so the typed
    /// date was often the only mark left. Since a workbook became one plot the plots cannot
    /// disagree, so the reference cell always carries that plot's own reference and both marks
    /// decide. That limit has gone and is recorded as gone, because a limit that no longer bites
    /// is as misleading as one that does.
    ///
    /// **Two limits, both stated rather than guarded against.** A filled workbook the tool wrote
    /// neither cell into reads as a template. And a client set that hinted the shape of a
    /// reference rather than bracketing it, DM-00 at C5, would be withheld, because nothing
    /// separates a hint from the thing it stands for. Neither measured set does that.
    ///
    /// The first is the safe way round: offering a filled file costs a rerun, and withholding a
    /// real template leaves the team unable to fill anything at all. The second is visible in
    /// one line, because the report prints what the cell held.
    ///
    /// **THIS IS THE ONE PLACE THAT STILL READS ROW 5 BY LETTER, AND IT IS ON PURPOSE.** The
    /// cells the run WRITES are found by their labels, in <see cref="LabelledPlaces"/>, because
    /// a letter is what the row 7 divergence proved cannot be trusted. This check runs over a
    /// file whose template is not known yet, on every workbook in the folder, so it reads the
    /// letters row 5 was measured to hold on all seven templates on 14 September. The two are
    /// two records of one fact and a test holds them against each other over the measured row
    /// 5: the cells the labels choose must be the cells these marks read.
    /// </summary>
    public static class FilledMarks
    {
        /// <summary>
        /// E5 on all seven templates, measured on 14 September, which is where the cell right of
        /// the Date: label at D5 lands. The letter is here rather than borrowed from the write
        /// side, because the write side has no letter any more.
        /// </summary>
        public const string DateCell = "E5";

        /// <summary>
        /// C5 on all seven templates, measured on 14 September, which is where the cell right of
        /// the REF : label at B5 lands.
        /// </summary>
        public const string ReferenceCell = "C5";

        /// <summary>
        /// The date the team types on the pane and the tool copies through. A template holds a
        /// placeholder or nothing there, and neither reads as a date.
        /// </summary>
        public static readonly FilledMark Date =
            new FilledMark(DateCell, "a date", ReadsAsADate);

        /// <summary>
        /// The plot reference, read off the plot's first sheet and copied through.
        /// </summary>
        public static readonly FilledMark Reference =
            new FilledMark(ReferenceCell, "a plot reference", ReadsAsAReference);

        /// <summary>
        /// The marks that can name a workbook as filled, in the order they decide. **Neither
        /// takes a template**, because row 5 reads the same on all seven, which is what the
        /// 14 September measurement settled.
        /// </summary>
        public static readonly IReadOnlyList<FilledMark> All = new[] { Date, Reference };

        /// <summary>
        /// Every cell the marks read, which is what the peek asks the file for before a template
        /// is known.
        /// </summary>
        public static IReadOnlyList<string> CellsRead
        {
            get
            {
                return All
                    .Select(mark => mark.Cell)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
        }

        /// <summary>
        /// The first mark whose cell holds something the tool would have written, or nothing
        /// when this workbook reads as a template.
        /// </summary>
        public static FilledCell Decide(IReadOnlyDictionary<string, string> cells)
        {
            if (cells == null) return null;

            foreach (FilledMark mark in All)
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
