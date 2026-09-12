using System;
using System.Collections.Generic;
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
    /// A sheet number built from the convention, or the reason none could be. Never random: a
    /// sheet number goes on an issued drawing and into a register, and a random one cannot be
    /// corrected later.
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
    /// The sheet numbers that will be refused and the words for them.
    ///
    /// Building a number is <see cref="SheetNumberRun"/>: the view code, the plot identifier
    /// with its dash dropped, then a sheet letter when the code holds several sheets. The
    /// rules that read a plot letter off the model's own numbers and stepped free numbers off
    /// the ones in use are gone, because on the measured model only DM-11 has real numbers and
    /// every other plot carries copies, so both rules answered nothing on 159 of 160 plots.
    /// </summary>
    public static class SheetNumbers
    {
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
    }
}
