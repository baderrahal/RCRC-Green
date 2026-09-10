using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// What is wrong with a sheet number before the run touches the model.
    /// </summary>
    public enum SheetNumberFault
    {
        None = 0,

        /// <summary>
        /// A sheet in the model already carries it. Revit keeps sheet numbers unique and will
        /// refuse the new one.
        /// </summary>
        AlreadyInTheModel = 1,

        /// <summary>
        /// Two of the sheets this run would make carry it. The first would be created and the
        /// second refused, which is the same fault arriving a second later.
        /// </summary>
        UsedTwiceInThisRun = 2
    }

    /// <summary>
    /// One sheet whose number will be refused, and why.
    /// </summary>
    public sealed class SheetNumberProblem
    {
        public SheetNumberProblem(
            string plotId, string sheetName, string sheetNumber, SheetNumberFault fault)
        {
            PlotId = plotId ?? string.Empty;
            SheetName = sheetName ?? string.Empty;
            SheetNumber = sheetNumber ?? string.Empty;
            Fault = fault;
        }

        public string PlotId { get; }

        public string SheetName { get; }

        public string SheetNumber { get; }

        public SheetNumberFault Fault { get; }

        public string InWords()
        {
            return PlotId + " " + SheetNumber + " for " + SheetName + ", "
                + SheetNumbers.FaultInWords(Fault);
        }
    }

    /// <summary>
    /// A sheet number proposed from the pattern the model already uses, or the reason none
    /// could be. Never random: a sheet number goes on an issued drawing and into a register,
    /// and a random one cannot be corrected later.
    /// </summary>
    public sealed class SheetNumberProposal
    {
        private SheetNumberProposal(string number, string whyNot)
        {
            Number = number ?? string.Empty;
            WhyNot = whyNot ?? string.Empty;
        }

        public static SheetNumberProposal For(string number)
        {
            return new SheetNumberProposal(number, string.Empty);
        }

        public static SheetNumberProposal Nothing(string whyNot)
        {
            return new SheetNumberProposal(string.Empty, whyNot);
        }

        public string Number { get; }

        public bool Offered
        {
            get { return Number.Length > 0; }
        }

        /// <summary>
        /// Said next to the empty box, so an empty proposal is a reason rather than a surprise
        /// at Run.
        /// </summary>
        public string WhyNot { get; }
    }

    /// <summary>
    /// Sheet numbers that will work, the ones that will not, and the next one in the model's
    /// own pattern.
    ///
    /// Numbers on plot DM-11 read 010QE, 010QF, 010QG, 010QH, 200Q, 400Q, 600QC, 600QD: the
    /// view code, then a letter for the plot, then a letter for the sheet within that code.
    /// The plot letter is read off the numbers the plot already has, never invented, so a plot
    /// with no sheets gets no proposal and says so.
    /// </summary>
    public static class SheetNumbers
    {
        /// <summary>
        /// How many times one seed is stepped on before it is given up as hopeless. A run of a
        /// thousand consecutive taken numbers from one seed has never been seen and would mean
        /// the scheme is full rather than that this needs a bigger number.
        /// </summary>
        private const int Tries = 1000;

        /// <summary>
        /// Numbers no sheet in this model carries, one offered for each number that is in use.
        ///
        /// Each is the number in use with its last run of digits stepped on until it is free, so
        /// every offer is shaped like something the project already does. L-211 gives L-212 and
        /// 010EA gives 011EA. A number holding no digits at all cannot be stepped on and offers
        /// nothing. A copy number such as 010QE Copy 001 is never a seed: it is what Revit
        /// writes when a sheet is duplicated, and stepping it offers 010QE Copy 002, which is a
        /// copy of a copy and not a number the project uses.
        /// </summary>
        public static IReadOnlyList<string> Free(IEnumerable<string> inUse)
        {
            HashSet<string> taken = Trimmed(inUse);

            var offered = new HashSet<string>(StringComparer.Ordinal);

            foreach (string seed in taken.Where(one => !IsACopy(one)))
            {
                string next = seed;
                for (int step = 0; step < Tries; step++)
                {
                    if (!TryStepOn(next, out next)) break;
                    if (taken.Contains(next)) continue;

                    offered.Add(next);
                    break;
                }
            }

            return offered.OrderBy(one => one, NaturalOrder.Comparer).ToList();
        }

        /// <summary>
        /// The letter that stands for the plot in its own sheet numbers, read as the first
        /// letter after the leading digits. Every parseable number the plot has must agree, and
        /// a plot whose numbers disagree gets no letter, because picking a side would put a
        /// wrong number into a drawing register. A copy number says nothing about the plot: on
        /// the first real model only DM-11 has numbers of its own, and every other plot's
        /// 010QE Copy 001 is DM-11's Q duplicated onto it, so a plot carrying only copies has
        /// no letter.
        /// </summary>
        public static string PlotLetter(IEnumerable<string> plotsOwnNumbers)
        {
            string whyNot;
            return PlotLetter(plotsOwnNumbers, out whyNot);
        }

        /// <summary>
        /// The letter, or empty with the reason there is none. Propose reads this rather than
        /// holding a second copy of the rule, so the letter can never be worked out two ways.
        /// </summary>
        public static string PlotLetter(IEnumerable<string> plotsOwnNumbers, out string whyNot)
        {
            List<string> letters = LettersIn(plotsOwnNumbers);

            if (letters.Count == 0)
            {
                whyNot = "This plot has no sheet numbers yet, so there is no plot letter to "
                    + "continue. Type the number.";
                return string.Empty;
            }

            if (letters.Distinct(StringComparer.Ordinal).Count() > 1)
            {
                whyNot = "This plot's own sheet numbers disagree about their plot letter, so none "
                    + "can be continued. Type the number.";
                return string.Empty;
            }

            whyNot = string.Empty;
            return letters[0];
        }

        /// <summary>
        /// The next free number in the pattern the plot already uses: the code, the plot's own
        /// letter, then A, B, C and so on, the first not in use anywhere. Nothing is invented:
        /// no numbers to continue means no proposal, with the reason.
        /// </summary>
        public static SheetNumberProposal Propose(
            string code, IEnumerable<string> numbersInUse, IEnumerable<string> plotsOwnNumbers)
        {
            if (string.IsNullOrEmpty(code))
            {
                return SheetNumberProposal.Nothing(
                    "There is no view code to number from. Type the number.");
            }

            string whyNoLetter;
            string letter = PlotLetter(plotsOwnNumbers, out whyNoLetter);
            if (letter.Length == 0) return SheetNumberProposal.Nothing(whyNoLetter);

            HashSet<string> taken = Trimmed(numbersInUse);

            for (char sheetLetter = 'A'; sheetLetter <= 'Z'; sheetLetter++)
            {
                string offered = code + letter + sheetLetter;
                if (!taken.Contains(offered)) return SheetNumberProposal.For(offered);
            }

            return SheetNumberProposal.Nothing(
                "Every number from " + code + letter + "A to " + code + letter
                + "Z is taken. Type the number.");
        }

        /// <summary>
        /// What is wrong with one number, for the line under its box. The same answer the run
        /// summary counts, so the two can never disagree.
        /// </summary>
        public static SheetNumberFault FaultIn(
            string number, IEnumerable<string> inUse, IEnumerable<string> askedThisRun)
        {
            string wanted = (number ?? string.Empty).Trim();
            if (wanted.Length == 0) return SheetNumberFault.None;

            // Already in the model comes first. Both can be true of a number typed twice
            // against a sheet that also exists, and the model is the one somebody looks at.
            if (Trimmed(inUse).Contains(wanted)) return SheetNumberFault.AlreadyInTheModel;

            int asked = (askedThisRun ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrWhiteSpace(one))
                .Count(one => string.Equals(one.Trim(), wanted, StringComparison.Ordinal));

            return asked > 1 ? SheetNumberFault.UsedTwiceInThisRun : SheetNumberFault.None;
        }

        /// <summary>
        /// Every numbered sheet across the run that will be refused, in the order the sheets
        /// were described.
        /// </summary>
        public static IReadOnlyList<SheetNumberProblem> Problems(
            IEnumerable<SheetBatch> batches, IEnumerable<string> inUse)
        {
            List<SheetToMake> rows = (batches ?? Enumerable.Empty<SheetBatch>())
                .Where(one => one != null && one.Definition.CanBeUsed)
                .SelectMany(one => one.Rows)
                .ToList();

            List<string> asked = rows
                .Where(one => one.HasNumber)
                .Select(one => one.SheetNumber)
                .ToList();

            var problems = new List<SheetNumberProblem>();
            foreach (SheetToMake row in rows.Where(one => one.HasNumber))
            {
                SheetNumberFault fault = FaultIn(row.SheetNumber, inUse, asked);
                if (fault == SheetNumberFault.None) continue;

                problems.Add(new SheetNumberProblem(
                    row.PlotId, row.SheetName, row.SheetNumber, fault));
            }

            return problems;
        }

        /// <summary>
        /// The line the run summary carries. Empty when nothing is wrong, because a run with no
        /// clash should say nothing about clashes.
        /// </summary>
        public static string InWords(int howMany)
        {
            if (howMany <= 0) return string.Empty;

            return howMany == 1
                ? "1 sheet number will be refused, and that sheet will not be made. It is marked "
                    + "in step 4."
                : howMany + " sheet numbers will be refused, and those sheets will not be made. "
                    + "They are marked in step 4.";
        }

        public static string FaultInWords(SheetNumberFault fault)
        {
            switch (fault)
            {
                case SheetNumberFault.AlreadyInTheModel:
                    return "a sheet in this model already has that number";
                case SheetNumberFault.UsedTwiceInThisRun:
                    return "another sheet in this run asks for the same number";
                default:
                    return "nothing";
            }
        }

        private static HashSet<string> Trimmed(IEnumerable<string> numbers)
        {
            return new HashSet<string>(
                (numbers ?? Enumerable.Empty<string>())
                    .Where(one => !string.IsNullOrWhiteSpace(one))
                    .Select(one => one.Trim()),
                StringComparer.Ordinal);
        }

        /// <summary>
        /// The first letter after the leading digits of each parseable number. 010QE gives Q,
        /// 200Q gives Q, and L-211 gives nothing because it does not start with digits. A copy
        /// number is skipped before its letter is read, because the letter in it belongs to
        /// the plot it was duplicated from.
        /// </summary>
        private static List<string> LettersIn(IEnumerable<string> numbers)
        {
            var letters = new List<string>();

            foreach (string number in (numbers ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrWhiteSpace(one))
                .Select(one => one.Trim())
                .Where(one => !IsACopy(one)))
            {
                int at = 0;
                while (at < number.Length && char.IsDigit(number[at])) at++;

                if (at == 0 || at >= number.Length) continue;
                if (!char.IsLetter(number[at])) continue;

                letters.Add(char.ToUpper(number[at], CultureInfo.InvariantCulture)
                    .ToString(CultureInfo.InvariantCulture));
            }

            return letters;
        }

        private static bool IsACopy(string number)
        {
            return number.IndexOf(ScannedSheet.CopyMark, StringComparison.Ordinal) >= 0;
        }

        /// <summary>
        /// The same number with its last run of digits stepped on by one, keeping the width so
        /// 010 becomes 011 rather than 11. False when there are no digits to step on.
        /// </summary>
        private static bool TryStepOn(string number, out string stepped)
        {
            stepped = number;

            int end = -1;
            for (int at = number.Length - 1; at >= 0; at--)
            {
                if (char.IsDigit(number[at])) { end = at; break; }
            }

            if (end < 0) return false;

            int start = end;
            while (start > 0 && char.IsDigit(number[start - 1])) start--;

            string digits = number.Substring(start, end - start + 1);

            long counted;
            if (!long.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out counted))
            {
                return false;
            }

            string next = (counted + 1).ToString(CultureInfo.InvariantCulture)
                .PadLeft(digits.Length, '0');

            stepped = number.Substring(0, start) + next + number.Substring(end + 1);
            return true;
        }
    }
}
