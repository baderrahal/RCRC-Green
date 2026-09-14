using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One tree row the canopy is worked out from: what this run wrote into it.
    /// </summary>
    public sealed class CanopyRow
    {
        public CanopyRow(
            string sheetName, int rowNumber, string botanicalName, int count, double diameterMetres,
            bool theClientsRow = false)
        {
            SheetName = sheetName ?? string.Empty;
            RowNumber = rowNumber;
            BotanicalName = botanicalName ?? string.Empty;
            Count = count;
            DiameterMetres = diameterMetres;
            TheClientsRow = theClientsRow;
        }

        /// <summary>
        /// True where the species MATCHED a row the client's own list already held, false where
        /// this run wrote the row into an empty one.
        ///
        /// **RECORD WHERE A VALUE CAME FROM AS YOU USE IT.** The canopy guard holds every row's
        /// formula against the text measured on the MOSQUES template's own row 21, and until the
        /// eightieth pass it only ever saw rows this tool wrote, whose shape this tool put there.
        /// It sees the client's own rows now. A drift on a row the tool wrote is the tool writing
        /// a row the workbook cannot compute, which is the #VALUE! fault that took four rounds to
        /// kill. A drift on a row the client already held is the client's file computing its
        /// canopy another way, which is a question for the team and not a bug. **The two need
        /// different answers and the message could not tell them apart.**
        /// </summary>
        public bool TheClientsRow { get; }

        public string Whose
        {
            get { return TheClientsRow ? "a row the client's list already held" : "a row this run wrote in"; }
        }

        public string SheetName { get; }

        public int RowNumber { get; }

        public string BotanicalName { get; }

        public int Count { get; }

        public double DiameterMetres { get; }

        /// <summary>
        /// **The workbook's own column, worked out the workbook's own way.** Measured on the
        /// MOSQUES template: L reads `IF(ISBLANK(J), " ", ROUND(PI()*(J/2)^2, 0))`, the canopy of
        /// one tree rounded to a whole square metre, and M multiplies that by the count in B.
        /// The rounding is INSIDE, per tree, and moving it outside would give a different number
        /// from the workbook on every row.
        ///
        /// **This and <see cref="WorkbookArithmetic.CanopyColumn"/> are two halves of one
        /// measurement**, the arithmetic here and the Excel text there. They cannot be one record
        /// because one is C# and the other is a formula in a client's file, so two tests hold
        /// them together, each writing its half out by hand off the same measured row 21.
        /// </summary>
        public double SquareMetres
        {
            get { return Math.Round(Math.PI * Math.Pow(DiameterMetres / 2.0, 2.0), 0) * Count; }
        }

        public string Working
        {
            get
            {
                return SheetName + " row " + RowNumber.ToString(CultureInfo.InvariantCulture)
                    + ", " + BotanicalName + ": ROUND(PI()*(" + Number(DiameterMetres) + "/2)^2, 0) * "
                    + Count.ToString(CultureInfo.InvariantCulture) + " = " + Number(SquareMetres);
            }
        }

        private static string Number(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// The canopy area of one plot, added up from the rows this run wrote a count into.
    ///
    /// **THIS IS THE FIRST NUMBER THIS TOOL PRODUCES THAT NO SCHEDULE PRINTED**, and it exists
    /// because the workbook's own Total Green cover is a formula whose cached result the patcher
    /// drops on purpose, so it is not in the file the run just wrote. Opening 150 workbooks by
    /// hand is not a workflow and relaxing the cache rule brings back the stale zeros that took
    /// four rounds to kill, so the tool works it out and **shows its working**.
    ///
    /// Adding printed numbers with the working shown is already allowed. This is that rule one
    /// step further: every part is a number this run wrote or read, the arithmetic is the
    /// workbook's own column, and the report prints every row that went into it.
    /// </summary>
    public sealed class CanopyTotal
    {
        public CanopyTotal(IEnumerable<CanopyRow> rows, IEnumerable<string> skipped)
        {
            Rows = (rows ?? Enumerable.Empty<CanopyRow>()).ToList();
            Skipped = (skipped ?? Enumerable.Empty<string>()).ToList();
        }

        public static readonly CanopyTotal Nothing = new CanopyTotal(null, null);

        public IReadOnlyList<CanopyRow> Rows { get; }

        /// <summary>
        /// Rows this run wrote a count into that carry no usable diameter, each named. **They
        /// are not counted as nought**: a tree with no size contributes no canopy in the
        /// workbook either, because its own L cell returns a space, and the report says which.
        /// </summary>
        public IReadOnlyList<string> Skipped { get; }

        public double SquareMetres
        {
            get { return Rows.Sum(one => one.SquareMetres); }
        }
    }

    public static class CanopyArea
    {
        /// <summary>
        /// The canopy rows of one plot, off the species this run really put a count into.
        ///
        /// **EVERY ROW THIS RUN WROTE A COUNT INTO, MATCHED OR WRITTEN IN**, because the canopy
        /// the workbook computes is over the rows its own counts sit in and a matched row carries
        /// a count just as a written one does.
        ///
        /// **THE 18:15 RUN MEASURED WHAT LEAVING THE MATCHED ROWS OUT COSTS.** Total Green cover
        /// came out as the planting plus the lawn and nothing else on every plot of 150:
        /// ANH-007-MO-100001 wrote 0.000105 km², which is 105 square metres, beside a workbook
        /// holding shrubs 70 and lawn 35, with twelve proposed trees contributing no canopy at
        /// all. The canopy percentage read 0 on both park PDFs, which is the same fault one step
        /// downstream.
        ///
        /// **AND THE DIAMETER OF A MATCHED ROW IS READ OFF THE WORKBOOK, NOT OFF THE RUN.** The
        /// tool writes a diameter only into a row it creates. A matched row is the client's own
        /// row: its measures are already in the file, the formulas beside it already read them,
        /// and the only thing Revit adds is the count in column B. So its canopy comes off the
        /// sheet's own diameter column, which is exactly the rule the species matching already
        /// follows.
        /// </summary>
        public static CanopyTotal From(KpiCreatePlan plan)
        {
            if (plan == null) return CanopyTotal.Nothing;

            var rows = new List<CanopyRow>();
            foreach (SpeciesMatch match in plan.Matches)
            {
                if (match.NotReachedByTheTotal || match.Row <= 0) continue;

                rows.Add(new CanopyRow(
                    match.SheetName, match.Row, match.Species.BotanicalName, match.Species.Quantity,
                    match.Added ? FromTheRun(match) : OffTheWorkbook(match),
                    !match.Added));
            }

            return Of(rows);
        }

        /// <summary>
        /// A row this run created carries the diameter this run wrote into it, off the schedule.
        /// </summary>
        private static double FromTheRun(SpeciesMatch match)
        {
            MeasureAnswer diameter = match.Species.Diameter;

            return diameter.Write ? diameter.Value : 0.0;
        }

        /// <summary>
        /// A row the client already held carries the client's own diameter. **It travels on the
        /// match already**, as `WorkbookDiameter`, read off the sheet's own diameter column where
        /// the match was made, so nothing here looks the row up a second time and the two cannot
        /// come apart. A diameter that does not read as a number contributes nothing and is
        /// named, the same as a row with none.
        /// </summary>
        private static double OffTheWorkbook(SpeciesMatch match)
        {
            double held;
            return double.TryParse(
                (match.WorkbookDiameter ?? string.Empty).Trim(),
                NumberStyles.Float, CultureInfo.InvariantCulture, out held)
                ? held
                : 0.0;
        }

        public static CanopyTotal Of(IEnumerable<CanopyRow> rows)
        {
            var held = (rows ?? Enumerable.Empty<CanopyRow>()).Where(one => one != null).ToList();

            var counted = new List<CanopyRow>();
            var skipped = new List<string>();

            foreach (CanopyRow one in held)
            {
                if (one.Count <= 0) continue;

                if (one.DiameterMetres > 0.0)
                {
                    counted.Add(one);
                    continue;
                }

                skipped.Add(one.SheetName + " row " + one.RowNumber.ToString(CultureInfo.InvariantCulture)
                    + ", " + one.BotanicalName + ": no canopy diameter, so it adds no canopy here "
                    + "and adds none in the workbook either");
            }

            return new CanopyTotal(counted, skipped);
        }
    }
}
