using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// The sheet numbers one plot's sheets take under one view code, built rather than
    /// proposed from a list.
    ///
    /// The convention, read off two models and confirmed by the user: the code, then the
    /// plot's marker, then A, B, C within the code in sheet order, and no letter at all when
    /// the code holds a single sheet. FP-39 on NG03 reads 010001A to 010001D and 200001, and
    /// DM-11 on NG05 reads 010QE to 010QH and 200Q, differing only in the marker.
    ///
    /// The letters count the sheets the plot already has under the code as well as the ones
    /// this run builds, so a second run continues after the last letter rather than
    /// colliding: DM-11 already holding 600QC and 600QD gets 600QE next. A bare number
    /// occupies the first letter's place, so a plot holding 200Q gets 200QB for its second
    /// 200 sheet, because renaming the model's own 200Q is not this tool's to do.
    /// </summary>
    public sealed class SheetNumberRun
    {
        private readonly string _code;

        private readonly string _marker;

        private readonly int _building;

        private int _nextSlot;

        private int _given;

        private readonly bool _startedEmpty;

        /// <param name="occupiedNumbers">Every number that could take a slot under this code
        /// and marker: the plot's own numbers from the model plus the numbers typed on the
        /// panel this run, so a typed 010001A pushes the first built one to B.</param>
        /// <param name="buildingThisRun">How many numbers this run will ask this sequence
        /// for. One sheet under an empty code goes bare, and several are lettered from the
        /// first, which cannot be told row by row.</param>
        public SheetNumberRun(
            string code, string marker, IEnumerable<string> occupiedNumbers, int buildingThisRun)
        {
            if (string.IsNullOrEmpty(code)) throw new ArgumentException("There is no view code.", "code");
            if (string.IsNullOrEmpty(marker)) throw new ArgumentException("There is no marker.", "marker");

            _code = code;
            _marker = marker;
            _building = buildingThisRun;

            List<int> occupied = SlotsIn(code, marker, occupiedNumbers);
            _startedEmpty = occupied.Count == 0;
            _nextSlot = occupied.Count == 0 ? 0 : occupied.Max() + 1;
        }

        /// <summary>
        /// The next number, in the order the rows come. Bare when this is the code's only
        /// sheet, lettered otherwise, and a reason instead once the letters run out.
        /// </summary>
        public SheetNumberProposal Next()
        {
            _given++;

            if (_startedEmpty && _building == 1 && _given == 1)
            {
                return SheetNumberProposal.For(_code + _marker);
            }

            int slot = _nextSlot;
            _nextSlot++;

            if (slot > 'Z' - 'A')
            {
                return SheetNumberProposal.Nothing(
                    "Every letter to Z after " + _code + _marker
                    + " is used, so no number could be built. Type the number.");
            }

            return SheetNumberProposal.For(_code + _marker + (char)('A' + slot));
        }

        /// <summary>
        /// Why a row gets no built number when its plot has no marker. Said under the box and
        /// carried into the run refusal, so the plot that needs its marker set is named in
        /// both places.
        /// </summary>
        public static string NoMarkerWords(string plotId)
        {
            return "No marker is set for " + plotId + " in step 1, so no number could be "
                + "built. Set it there or type the number.";
        }

        /// <summary>
        /// Why a row gets no built number when its views carry more than one code. Picking
        /// either code for the front of the number would be a guess.
        /// </summary>
        public static string MixedCodesWords()
        {
            return "This sheet's views carry different codes, so no single code can front its "
                + "number. Type the number.";
        }

        /// <summary>
        /// The slots the given numbers take under this code and marker: the bare number holds
        /// the first letter's place and a single trailing letter holds its own. Anything else
        /// under the prefix, such as a copy number, holds nothing.
        /// </summary>
        private static List<int> SlotsIn(
            string code, string marker, IEnumerable<string> occupiedNumbers)
        {
            string front = code + marker;
            var slots = new List<int>();

            foreach (string number in (occupiedNumbers ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrWhiteSpace(one))
                .Select(one => one.Trim())
                .Where(one => one.IndexOf(ScannedSheet.CopyMark, StringComparison.Ordinal) < 0)
                .Where(one => one.StartsWith(front, StringComparison.Ordinal)))
            {
                string tail = number.Substring(front.Length);

                if (tail.Length == 0)
                {
                    slots.Add(0);
                    continue;
                }

                if (tail.Length == 1 && tail[0] >= 'A' && tail[0] <= 'Z')
                {
                    slots.Add(tail[0] - 'A');
                }
            }

            return slots;
        }
    }
}
