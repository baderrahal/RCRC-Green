using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One schedule's L/DAY TOTAL, read off the row the schedule prints that word on.
    ///
    /// **THE TOTAL ROW, NEVER THE SPECIES ROWS ADDED UP.** Bader, 15 September: take the total
    /// of all. A sum this tool worked out is a different number from one the schedule printed,
    /// and the difference would be invisible, so nothing here adds anything. If a schedule
    /// prints no TOTAL row this half writes nothing and is named, and the species rows are not
    /// used instead.
    ///
    /// **THE COLUMN IS FOUND BY ITS HEADING, MATCHED WHOLE.** Three columns of the real
    /// softscape schedule hold the word WATER and only one reads L/DAY, so
    /// <see cref="ScheduleColumns.Reading"/> matches the whole heading and refuses both none
    /// and more than one, naming what the heading row held.
    ///
    /// **THE TOTAL ROW IS FOUND BY ITS FIRST CELL AND THAT IS ITS DEFINITION.** The rule that a
    /// schedule value is never read by cell position is about VALUES. Which row is the total is
    /// a fact about the row's shape, the same way <see cref="ScheduleColumns.IsStructureRow"/>
    /// is, and <see cref="SoftscapeRows"/> already finds its own TOTAL row this way. The mark is
    /// read off that one class so the two cannot part.
    /// </summary>
    public sealed class WaterDemandRead
    {
        private WaterDemandRead(
            string scheduleName, bool read, double litresADay, int totalRow, string why,
            bool nothingScheduled = false)
        {
            ScheduleName = (scheduleName ?? string.Empty).Trim();
            Read = read;
            LitresADay = litresADay;
            TotalRow = totalRow;
            Why = (why ?? string.Empty).Trim();
            NothingScheduled = nothingScheduled;
        }

        /// <summary>The schedule this half came off, empty when the plot holds none of its kind.</summary>
        public string ScheduleName { get; }

        public bool Read { get; }

        /// <summary>Litres a day exactly as the TOTAL row printed it. Meaningless unless Read.</summary>
        public double LitresADay { get; }

        /// <summary>
        /// The row number the total was taken off, counting the heading row as row 1, so a
        /// person can open the schedule and look at the same row this read. **Nought on a
        /// schedule that printed no body rows**, which has no TOTAL row to name.
        /// </summary>
        public int TotalRow { get; }

        /// <summary>
        /// Whether this half was read off a schedule that printed its heading row and nothing
        /// under it. **The 0 it carries came from the schedule holding nothing, not from a TOTAL
        /// row printing 0**, and a report that could not tell the two apart would be reporting a
        /// number it cannot point at a row for.
        /// </summary>
        public bool NothingScheduled { get; }

        public string Why { get; }

        /// <summary>
        /// The default a reading carries when nothing filled one in, which is every reading
        /// built before this round and every reading whose plot threw. It says the read did not
        /// happen rather than reading as a schedule that printed nothing.
        /// </summary>
        public static readonly WaterDemandRead Nothing =
            new WaterDemandRead(string.Empty, false, 0.0, 0, "no schedule of that kind was read on this plot");

        public static WaterDemandRead NoSchedule(string kind)
        {
            return new WaterDemandRead(string.Empty, false, 0.0, 0,
                "this plot holds no " + (kind ?? string.Empty).Trim() + " schedule to read a total off");
        }

        public static WaterDemandRead Refused(string scheduleName, string why)
        {
            if (string.IsNullOrWhiteSpace(why)) throw new ArgumentException("A refusal needs a reason.", "why");

            return new WaterDemandRead(scheduleName, false, 0.0, 0, why);
        }

        public static WaterDemandRead Of(string scheduleName, double litresADay, int totalRow)
        {
            if (totalRow < 1) throw new ArgumentOutOfRangeException("totalRow");

            return new WaterDemandRead(scheduleName, true, litresADay, totalRow, string.Empty);
        }

        /// <summary>
        /// **A SCHEDULE THAT PRINTED ITS HEADING ROW AND NOTHING UNDER IT IS NOUGHT FOR ITS
        /// HALF.** Bader's decision of 15 September, off the 13:32 run: MM-01, MM-06, MM-07 and
        /// NS-23 wrote no Irrigation water demand at all with both halves empty, and MM-08,
        /// NS-28 and NS-38 with the shrubs and lawn half empty. A schedule holding nothing has
        /// nothing to irrigate, so the whole box was blank on a plot the other half of which was
        /// read perfectly well.
        ///
        /// **A SCHEDULE WITH BODY ROWS AND NO TOTAL ROW STILL REFUSES**, which is the rule one
        /// method down and is untouched. Rows with no total is a schedule whose total nobody
        /// printed, and adding the rows up here would be the sum this class exists not to make.
        /// </summary>
        public static WaterDemandRead NothingToTotal(string scheduleName)
        {
            return new WaterDemandRead(scheduleName, true, 0.0, 0, NoBodyRows, true);
        }

        public const string NoBodyRows =
            "it prints its heading row and no rows under it, so this plot schedules nothing of "
            + "that kind and its half of the demand is 0";

        /// <summary>
        /// The whole rule, over the rows exactly as the schedule printed them.
        /// </summary>
        public static WaterDemandRead From(ScannedSchedule schedule)
        {
            if (schedule == null) throw new ArgumentNullException("schedule");

            string name = schedule.Name;

            if (!schedule.RowsWereRead)
            {
                return Refused(name, "its rows were not read, so no TOTAL row could be looked for");
            }

            List<IReadOnlyList<string>> rows = schedule.Rows.ToList();
            if (rows.Count == 0) return Refused(name, "it printed no rows at all");

            IReadOnlyList<string> headings = rows[0];
            int column = ScheduleColumns.Reading(headings, ScheduleColumns.LitresADayHeading);
            if (column == -2)
            {
                return Refused(name, ScheduleColumns.MoreThanOneReading(headings, ScheduleColumns.LitresADayHeading));
            }

            if (column < 0)
            {
                return Refused(name, ScheduleColumns.NoneReading(headings, ScheduleColumns.LitresADayHeading));
            }

            string heading = LabelText.Trimmed(headings[column]);

            // **NOTHING UNDER THE HEADING ROW IS A NOUGHT, and it is asked AFTER the column has
            // been found on purpose.** A heading row that does not name the L/DAY column is a
            // schedule this reader cannot read, and answering 0 for it would be the fall back to
            // a position that this repository forbids everywhere else. So an empty schedule of
            // the right shape counts 0 and an empty schedule of an unknown shape still refuses.
            // The alternative, counting 0 before looking at the heading row at all, is recorded
            // in the log as the choice not taken.
            //
            // One row means the heading row and nothing under it, counted rather than inferred
            // from the loop below falling through, because falling through is also what a
            // schedule full of species with no TOTAL row does and those two are different
            // answers.
            if (rows.Count == 1) return NothingToTotal(name);

            for (int index = 1; index < rows.Count; index++)
            {
                IReadOnlyList<string> row = rows[index];
                if (row == null || row.Count == 0) continue;
                if (!LabelText.Same(ScheduleColumns.At(row, 0), SoftscapeRows.TotalMark)) continue;

                string cell = ScheduleColumns.At(row, column);
                CellNumberRead number = CellNumber.Read(cell);
                if (number.IsRefused)
                {
                    return Refused(name, "row " + (index + 1) + ", the TOTAL row: its " + heading
                        + " cell " + number.Refusal);
                }

                if (!number.IsNumber)
                {
                    return Refused(name, "row " + (index + 1) + ", the TOTAL row: its " + heading
                        + " cell holds '" + cell + "' and not a number");
                }

                return Of(name, number.Value, index + 1);
            }

            return Refused(name, NoTotalRow);
        }

        public const string NoTotalRow =
            "it prints no TOTAL row, so there is no total to take. The species rows are NOT added "
            + "up instead, because a sum this tool worked out is a different number from one the "
            + "schedule printed";

        public string InWords
        {
            get
            {
                if (!Read)
                {
                    return (ScheduleName.Length == 0 ? "no schedule" : ScheduleName) + ": " + Why;
                }

                // **A NOUGHT WITH NO ROW BEHIND IT SAYS SO.** Printing `0 L/day off its TOTAL
                // row, row 0` would name a row the schedule does not have.
                if (NothingScheduled) return ScheduleName + ": " + Why;

                return ScheduleName + ": " + WaterDemand.Litres(LitresADay) + " off its TOTAL row, row "
                    + TotalRow.ToString(CultureInfo.InvariantCulture);
            }
        }
    }

    /// <summary>
    /// One plot's irrigation water demand, both halves and what the form asks for.
    ///
    /// **BOTH SCHEDULES OR NOTHING.** Bader specified the total of both, so a plot missing
    /// either schedule, or whose either total cannot be read, writes nothing into the field and
    /// is named with which schedule and why. Writing half a demand into a box printed
    /// `Irrigation water demand` would be a number nobody could tell was half.
    ///
    /// **THE DIVISION BY A THOUSAND IS FOR THE FORM AND FOR NOTHING ELSE.** The schedules print
    /// litres a day and the form's unit column reads m with a superscript three over day, so the
    /// litres are divided here and the litres are what the report prints beside them. That is
    /// the same shape as the Roads page's Length field, whose metres are divided for the PDF
    /// alone and left alone in the workbook.
    /// </summary>
    public sealed class PlotWaterDemand
    {
        public PlotWaterDemand(WaterDemandRead softscape, WaterDemandRead shrubsAndLawn)
        {
            Softscape = softscape ?? WaterDemandRead.Nothing;
            ShrubsAndLawn = shrubsAndLawn ?? WaterDemandRead.Nothing;
        }

        public static readonly PlotWaterDemand NotRead =
            new PlotWaterDemand(WaterDemandRead.Nothing, WaterDemandRead.Nothing);

        public WaterDemandRead Softscape { get; }

        public WaterDemandRead ShrubsAndLawn { get; }

        public bool BothRead
        {
            get { return Softscape.Read && ShrubsAndLawn.Read; }
        }

        /// <summary>The two totals added, in litres a day. Meaningless unless BothRead.</summary>
        public double LitresADay
        {
            get { return Softscape.LitresADay + ShrubsAndLawn.LitresADay; }
        }

        /// <summary>What the form asks for, the litres divided by a thousand.</summary>
        public double CubicMetresADay
        {
            get { return LitresADay / WaterDemand.LitresInACubicMetre; }
        }

        /// <summary>
        /// Why nothing can be written, naming BOTH halves where both failed, because a report
        /// short of the second reads exactly like a plot that had one thing wrong with it.
        /// </summary>
        public string Why
        {
            get
            {
                if (BothRead) return string.Empty;

                var said = new List<string>();
                if (!Softscape.Read) said.Add(WaterDemand.SoftscapeKind + ", " + Softscape.Why);
                if (!ShrubsAndLawn.Read) said.Add(WaterDemand.ShrubsAndLawnKind + ", " + ShrubsAndLawn.Why);

                return string.Join(". ", said.ToArray());
            }
        }

        public string InWords
        {
            get
            {
                if (!BothRead) return "no irrigation water demand: " + Why;

                return WaterDemand.Litres(Softscape.LitresADay) + " plus "
                    + WaterDemand.Litres(ShrubsAndLawn.LitresADay) + " is "
                    + WaterDemand.Litres(LitresADay) + ", which is "
                    + WaterDemand.CubicMetres(CubicMetresADay) + " for the form";
            }
        }
    }

    /// <summary>
    /// The names, the units and the one division, held once.
    /// </summary>
    public static class WaterDemand
    {
        public const string SoftscapeKind = "softscape";

        public const string ShrubsAndLawnKind = "shrubs and lawn";

        /// <summary>
        /// The one place litres a day become cubic metres a day. A second copy of this number is
        /// the shape this repository keeps paying for.
        /// </summary>
        public const double LitresInACubicMetre = 1000.0;

        public const string LitresUnit = "L/day";

        /// <summary>
        /// **THE UNIT THE FORM ASKS FOR, and it is the one the report says.** The source gives
        /// litres and the box is printed in cubic metres, so a report naming the source's unit
        /// beside the number the tool wrote would be a report at war with the document.
        /// </summary>
        public const string CubicMetresUnit = "m³/day";

        public static PlotWaterDemand Of(WaterDemandRead softscape, WaterDemandRead shrubsAndLawn)
        {
            return new PlotWaterDemand(softscape, shrubsAndLawn);
        }

        public static string Litres(double value)
        {
            return Number(value) + " " + LitresUnit;
        }

        public static string CubicMetres(double value)
        {
            return Number(value) + " " + CubicMetresUnit;
        }

        private static string Number(double value)
        {
            return value.ToString("0.####", CultureInfo.InvariantCulture);
        }
    }
}
