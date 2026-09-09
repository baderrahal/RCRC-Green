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
    /// One plot whose sheet will be refused, and why.
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
    /// Sheet numbers that will work, and the ones that will not.
    ///
    /// Three sheets were refused in a real run, every one of them with "a sheet numbered 010EA
    /// is already in this model". The refusal was right and the offer was wrong: the dropdown
    /// listed the numbers already in use, so every entry in it was certain to be rejected. It
    /// lists free ones now, and a number that will be refused says so before Run rather than
    /// after.
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
        /// every offer is shaped like something the project already does. L-211 gives L-212,
        /// 010QE Copy 001 gives 010QE Copy 002, and 010EA gives 011EA. A number holding no
        /// digits at all cannot be stepped on and offers nothing.
        /// </summary>
        public static IReadOnlyList<string> Free(IEnumerable<string> inUse)
        {
            var taken = new HashSet<string>(
                (inUse ?? Enumerable.Empty<string>())
                    .Where(one => !string.IsNullOrWhiteSpace(one))
                    .Select(one => one.Trim()),
                StringComparer.Ordinal);

            var offered = new HashSet<string>(StringComparer.Ordinal);

            foreach (string seed in taken)
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
        /// What is wrong with each plot's number, one dictionary per sheet the user described,
        /// keyed by plot. The panel puts a line next to the box from this, and the run summary
        /// counts it, so the two can never say different things.
        /// </summary>
        public static IReadOnlyList<IReadOnlyDictionary<string, SheetNumberFault>> Faults(
            IReadOnlyList<SheetOrder> orders,
            IEnumerable<string> ticked,
            IEnumerable<string> inUse)
        {
            List<SheetOrder> wanted = (orders ?? new List<SheetOrder>())
                .Where(one => one != null)
                .ToList();

            var inTheRun = new HashSet<string>(
                (ticked ?? Enumerable.Empty<string>()).Where(one => one != null),
                StringComparer.Ordinal);

            var taken = new HashSet<string>(
                (inUse ?? Enumerable.Empty<string>())
                    .Where(one => !string.IsNullOrWhiteSpace(one))
                    .Select(one => one.Trim()),
                StringComparer.Ordinal);

            // Every number this run would write, counted across all the sheets described,
            // because two definitions asking for one number clash just as hard as two plots do.
            var timesAsked = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (SheetOrder order in wanted.Where(one => one.Definition.CanBeUsed))
            {
                foreach (SheetRequest number in order.Numbers
                    .Where(one => one.Complete && inTheRun.Contains(one.PlotId)))
                {
                    int already;
                    timesAsked[number.SheetNumber] =
                        timesAsked.TryGetValue(number.SheetNumber, out already) ? already + 1 : 1;
                }
            }

            var faults = new List<IReadOnlyDictionary<string, SheetNumberFault>>(wanted.Count);
            foreach (SheetOrder order in wanted)
            {
                var forThisSheet = new Dictionary<string, SheetNumberFault>(StringComparer.Ordinal);

                foreach (SheetRequest number in order.Numbers)
                {
                    if (!number.Complete || !inTheRun.Contains(number.PlotId)) continue;
                    if (!order.Definition.CanBeUsed) continue;

                    SheetNumberFault fault = FaultIn(number.SheetNumber, taken, timesAsked);
                    if (fault != SheetNumberFault.None) forThisSheet.Add(number.PlotId, fault);
                }

                faults.Add(forThisSheet);
            }

            return faults;
        }

        /// <summary>
        /// Every plot whose sheet will be refused, in the order the sheets were described.
        /// </summary>
        public static IReadOnlyList<SheetNumberProblem> Problems(
            IReadOnlyList<SheetOrder> orders,
            IEnumerable<string> ticked,
            IEnumerable<string> inUse)
        {
            List<SheetOrder> wanted = (orders ?? new List<SheetOrder>())
                .Where(one => one != null)
                .ToList();

            IReadOnlyList<IReadOnlyDictionary<string, SheetNumberFault>> faults =
                Faults(wanted, ticked, inUse);

            var problems = new List<SheetNumberProblem>();
            for (int at = 0; at < wanted.Count; at++)
            {
                SheetOrder order = wanted[at];

                foreach (SheetRequest number in order.Numbers)
                {
                    SheetNumberFault fault;
                    if (!faults[at].TryGetValue(number.PlotId, out fault)) continue;

                    problems.Add(new SheetNumberProblem(
                        number.PlotId, order.Definition.SheetName, number.SheetNumber, fault));
                }
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

        private static SheetNumberFault FaultIn(
            string number, HashSet<string> taken, Dictionary<string, int> timesAsked)
        {
            // Already in the model comes first. Both are true of a number typed twice against a
            // sheet that also exists, and the model is the one a person goes and looks at.
            if (taken.Contains(number)) return SheetNumberFault.AlreadyInTheModel;

            int asked;
            return timesAsked.TryGetValue(number, out asked) && asked > 1
                ? SheetNumberFault.UsedTwiceInThisRun
                : SheetNumberFault.None;
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
