using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// The sheet numbers one plot's sheets take under one view code, built rather than
    /// proposed from a list.
    ///
    /// The number is the view code, then the plot's own identifier with its dash dropped,
    /// then A, B, C within the code in sheet order, and no letter at all when the code
    /// holds a single sheet: DM-42 reads 010DM42A and 010DM42B for two 010 sheets and a
    /// bare 200DM42 for its one 200 sheet. The identifier is unique per plot already, so
    /// nothing is set anywhere, nothing is reserved and nothing runs out. This replaced
    /// the per-plot marker picked in step 1, which was wrong twice over: the user had
    /// already said the numbering must be automatic, and the dropdowns made step 1 long.
    /// The marker family is deleted rather than left beside this, because two records of
    /// one fact is the shape this repo keeps paying for.
    ///
    /// The letters count the sheets the plot already has under the code as well as the
    /// ones this run builds, so a second run continues after the last letter rather than
    /// colliding, and a bare number occupies the first letter's place, because renaming a
    /// sheet the model already holds is not this tool's to do. A number under the old
    /// marker scheme, 010QE on NG05 or 010001A on NG03, does not start with the new front
    /// and holds no slot, so old sheets are never renumbered and the two schemes sit side
    /// by side on a real model, which is the user's decision rather than a fault.
    /// </summary>
    public sealed class SheetNumberRun
    {
        private readonly string _front;

        private readonly int _building;

        private int _nextSlot;

        private int _given;

        private readonly bool _startedEmpty;

        /// <param name="occupiedNumbers">Every number that could take a slot under this
        /// code and plot: the plot's own numbers from the model plus the numbers typed on
        /// the panel this run, so a typed 010DM42A pushes the first built one to B.</param>
        /// <param name="buildingThisRun">How many numbers this run will ask this sequence
        /// for. One sheet under an empty code goes bare, and several are lettered from the
        /// first, which cannot be told row by row.</param>
        public SheetNumberRun(
            string code, string plotId, IEnumerable<string> occupiedNumbers, int buildingThisRun)
        {
            if (string.IsNullOrEmpty(code)) throw new ArgumentException("There is no view code.", "code");

            string stem = StemOf(plotId);
            if (stem.Length == 0)
            {
                throw new ArgumentException(
                    "There is no plot identifier to put in the number.", "plotId");
            }

            _front = code + stem;
            _building = buildingThisRun;

            List<int> occupied = SlotsIn(_front, occupiedNumbers);
            _startedEmpty = occupied.Count == 0;
            _nextSlot = occupied.Count == 0 ? 0 : occupied.Max() + 1;
        }

        /// <summary>
        /// The middle of the number: the plot identifier with its dash dropped, DM-42 to
        /// DM42. Empty for anything that is not a plot identifier, so nothing can build a
        /// number on a name the model never gave.
        /// </summary>
        public static string StemOf(string plotId)
        {
            string read;
            if (!PlotId.TryRead(plotId, out read)) return string.Empty;

            return read.Replace("-", string.Empty);
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
                return SheetNumberProposal.For(_front);
            }

            int slot = _nextSlot;
            _nextSlot++;

            if (slot > 'Z' - 'A')
            {
                return SheetNumberProposal.Nothing(
                    "Every letter to Z after " + _front
                    + " is used, so no number could be built. Type the number.");
            }

            return SheetNumberProposal.For(_front + (char)('A' + slot));
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
        /// Why a row gets no built number when its sheet holds no views AND its title block
        /// answers no code.
        ///
        /// A sheet with no views used to get this whatever it was described on, which is
        /// what put a red line under every COVER PAGE sheet. Such a sheet has no view type,
        /// so its code comes off the title block through TitleBlockSettings.CodeFor, and
        /// only a block neither settings file names, or one paired with two codes, is left
        /// with nothing to build from.
        /// </summary>
        public static string NoViewsWords()
        {
            return "This sheet holds no views, so its code comes from its title block, and "
                + "the settings pair that block with no single view code. Pick the title "
                + "block or type the number.";
        }

        /// <summary>
        /// The slots the given numbers take under this front: the bare number holds the
        /// first letter's place and a single trailing letter holds its own. Anything else
        /// holds nothing, a copy number, an old marker scheme number that never starts
        /// with the front, or a longer plot's number whose tail here is its extra digits.
        /// </summary>
        private static List<int> SlotsIn(string front, IEnumerable<string> occupiedNumbers)
        {
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
