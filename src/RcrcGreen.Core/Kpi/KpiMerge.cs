using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One plot's own contribution to a total, kept beside the total so the arithmetic can be
    /// checked by eye without opening Revit.
    /// </summary>
    public sealed class PlotNumber
    {
        public PlotNumber(string plotId, double value)
        {
            if (plotId == null) throw new ArgumentNullException("plotId");

            PlotId = plotId;
            Value = value;
        }

        public string PlotId { get; }

        public double Value { get; }
    }

    /// <summary>
    /// A total and the per plot numbers it was made from.
    ///
    /// The total is passed in rather than worked out on demand, and <see cref="Adds"/> works
    /// it out again and compares. That looks like the two records of one fact this repo keeps
    /// being bitten by, and it is the opposite: the schedules print their own subtotal and
    /// TOTAL rows, which are forbidden as a source, so the guard exists to catch a future
    /// caller that reaches for one. A total that does not equal its parts refuses the write.
    /// </summary>
    public sealed class Totalled
    {
        /// <summary>
        /// Summing in a different order moves the last bits of a double, and an area in square
        /// feet runs to five figures before the point, so the comparison is relative.
        /// </summary>
        public const double Tolerance = 1e-9;

        public Totalled(IEnumerable<PlotNumber> perPlot, double total)
        {
            PerPlot = (perPlot ?? Enumerable.Empty<PlotNumber>()).Where(one => one != null).ToList();
            Total = total;
        }

        public IReadOnlyList<PlotNumber> PerPlot { get; }

        public double Total { get; }

        public double Sum
        {
            get { return PerPlot.Sum(one => one.Value); }
        }

        public bool Adds
        {
            get
            {
                double room = Tolerance * Math.Max(1.0, Math.Abs(Total));
                return Math.Abs(Total - Sum) <= room;
            }
        }

        public static Totalled Adding(IEnumerable<PlotNumber> perPlot)
        {
            List<PlotNumber> held = (perPlot ?? Enumerable.Empty<PlotNumber>())
                .Where(one => one != null)
                .ToList();

            return new Totalled(held, held.Sum(one => one.Value));
        }

        public static Totalled Nothing
        {
            get { return new Totalled(null, 0.0); }
        }
    }

    /// <summary>
    /// One species and group across every chosen plot, with each plot's own number kept.
    ///
    /// Merging is on the group AND the botanical name, never the name alone. ALBIZIA LEBBECK
    /// proposed on DM-12 is 13 and on DM-13 is 5, so the merged proposed row is 18, and the
    /// same name existing on DM-12 is 1 and stays out of it.
    /// </summary>
    public sealed class MergedSpecies
    {
        public MergedSpecies(string botanicalName, string groupName, IEnumerable<PlotNumber> perPlot)
        {
            if (botanicalName == null) throw new ArgumentNullException("botanicalName");

            BotanicalName = botanicalName;
            GroupName = groupName ?? string.Empty;
            PerPlot = (perPlot ?? Enumerable.Empty<PlotNumber>()).Where(one => one != null).ToList();
        }

        /// <summary>
        /// As the schedule printed it on the first plot that held it. Nothing is normalised on
        /// the way in, because the report has to name what Revit really said.
        /// </summary>
        public string BotanicalName { get; }

        public string GroupName { get; }

        public IReadOnlyList<PlotNumber> PerPlot { get; }

        public int Quantity
        {
            get { return PerPlot.Sum(one => (int)one.Value); }
        }

        public IReadOnlyList<string> Plots
        {
            get { return PerPlot.Select(one => one.PlotId).ToList(); }
        }
    }

    /// <summary>
    /// Two plots reporting the same raw area to the last bit. Either they are genuinely the
    /// same size or one region is counted twice, and a double count nobody sees is the worst
    /// thing this tool can produce. Measured: MM-03 and MM-04 both read 12182.05561411.
    /// </summary>
    public sealed class IdenticalArea
    {
        public IdenticalArea(IEnumerable<string> plots, double rawSquareFeet, string printed)
        {
            Plots = (plots ?? Enumerable.Empty<string>()).Where(one => one != null).ToList();
            RawSquareFeet = rawSquareFeet;
            Printed = printed ?? string.Empty;
        }

        public IReadOnlyList<string> Plots { get; }

        public double RawSquareFeet { get; }

        public string Printed { get; }
    }

    /// <summary>
    /// One plot and what it holds for a value that is a single cell in the workbook.
    /// </summary>
    public sealed class PlotText
    {
        public PlotText(string plotId, string text)
        {
            if (plotId == null) throw new ArgumentNullException("plotId");

            PlotId = plotId;
            Text = (text ?? string.Empty).Trim();
        }

        public string PlotId { get; }

        public string Text { get; }
    }

    /// <summary>
    /// A value that is one cell in the workbook and cannot be added, held across the chosen
    /// plots. When they all agree the value is written. When they differ the distinct values
    /// are reported and the cell is left empty, because joining them with commas, taking the
    /// first or taking the most common all write something nobody chose.
    /// </summary>
    public sealed class AgreedValue
    {
        public AgreedValue(IEnumerable<PlotText> perPlot)
        {
            PerPlot = (perPlot ?? Enumerable.Empty<PlotText>()).Where(one => one != null).ToList();
            Distinct = PerPlot
                .Select(one => one.Text)
                .Where(text => text.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(text => text, NaturalOrder.Comparer)
                .ToList();
        }

        public IReadOnlyList<PlotText> PerPlot { get; }

        public IReadOnlyList<string> Distinct { get; }

        public bool Agrees
        {
            get { return Distinct.Count == 1; }
        }

        /// <summary>
        /// Empty unless every plot that holds a value holds the same one.
        /// </summary>
        public string Value
        {
            get { return Agrees ? Distinct[0] : string.Empty; }
        }
    }

    /// <summary>
    /// What the chosen plots add up to, with every plot's own number kept beside every total.
    ///
    /// Adding numbers the schedules printed is arithmetic this tool is allowed to do. Working
    /// a number out from the elements a schedule lists is not, and never happens anywhere,
    /// because those elements are RVT Link instances on this model.
    /// </summary>
    public static class KpiMerge
    {
        /// <summary>
        /// The group heading the workbook calls SHRUBS. The schedule spells it out in full.
        /// </summary>
        public const string ShrubsHeading = "SHRUBS & GROUND COVER";

        /// <summary>
        /// The workbook calls this LAWN and the schedule calls it GRASS.
        /// </summary>
        public const string LawnHeading = "GRASS";

        public static Totalled Shrubs(IEnumerable<PlotReading> readings)
        {
            return Subtotalled(readings, ShrubsHeading);
        }

        public static Totalled Lawn(IEnumerable<PlotReading> readings)
        {
            return Subtotalled(readings, LawnHeading);
        }

        public static Totalled Area(IEnumerable<PlotReading> readings)
        {
            var perPlot = new List<PlotNumber>();
            foreach (PlotReading reading in Held(readings))
            {
                RegionArea chosen = reading.ChosenRegion;
                if (chosen == null) continue;

                perPlot.Add(new PlotNumber(reading.PlotId, chosen.SquareMetres));
            }

            return Totalled.Adding(perPlot);
        }

        /// <summary>
        /// Every group of chosen plots whose chosen region reports the same raw area. Reported
        /// and confirmed by a person before anything is written, never added quietly.
        /// </summary>
        public static IReadOnlyList<IdenticalArea> IdenticalAreas(IEnumerable<PlotReading> readings)
        {
            var byRaw = new Dictionary<double, List<PlotReading>>();
            var order = new List<double>();

            foreach (PlotReading reading in Held(readings))
            {
                RegionArea chosen = reading.ChosenRegion;
                if (chosen == null || !chosen.HoldsAnArea) continue;

                List<PlotReading> already;
                if (!byRaw.TryGetValue(chosen.RawSquareFeet, out already))
                {
                    already = new List<PlotReading>();
                    byRaw[chosen.RawSquareFeet] = already;
                    order.Add(chosen.RawSquareFeet);
                }

                already.Add(reading);
            }

            var found = new List<IdenticalArea>();
            foreach (double raw in order)
            {
                List<PlotReading> sharing = byRaw[raw];
                if (sharing.Count < 2) continue;

                found.Add(new IdenticalArea(
                    sharing.Select(one => one.PlotId),
                    raw,
                    sharing[0].ChosenRegion.Printed));
            }

            return found;
        }

        /// <summary>
        /// Every species row across the chosen plots, merged on the group and the botanical
        /// name compared without case and with surrounding whitespace off. Rows that sat under
        /// no group row are left out of here and named by <see cref="Ungrouped"/>.
        /// </summary>
        public static IReadOnlyList<MergedSpecies> Species(IEnumerable<PlotReading> readings)
        {
            var byKey = new Dictionary<string, List<PlotNumber>>(StringComparer.Ordinal);
            var names = new Dictionary<string, SpeciesRow>(StringComparer.Ordinal);
            var order = new List<string>();

            foreach (PlotReading reading in Held(readings))
            {
                foreach (SpeciesRow row in reading.Species)
                {
                    if (!row.HasGroup) continue;

                    string key = Key(row);
                    List<PlotNumber> already;
                    if (!byKey.TryGetValue(key, out already))
                    {
                        already = new List<PlotNumber>();
                        byKey[key] = already;
                        names[key] = row;
                        order.Add(key);
                    }

                    already.Add(new PlotNumber(reading.PlotId, row.Quantity));
                }
            }

            return order
                .Select(key => new MergedSpecies(names[key].BotanicalName, names[key].GroupName, byKey[key]))
                .OrderBy(one => one.GroupName, NaturalOrder.Comparer)
                .ThenBy(one => one.BotanicalName, NaturalOrder.Comparer)
                .ToList();
        }

        /// <summary>
        /// Species rows that sat under no group row. Reported with their plot and never
        /// assumed into a group, because a count in the wrong tree sheet is worse than a gap.
        /// </summary>
        public static IReadOnlyList<SpeciesRow> Ungrouped(IEnumerable<PlotReading> readings)
        {
            var found = new List<SpeciesRow>();
            foreach (PlotReading reading in Held(readings))
            {
                foreach (SpeciesRow row in reading.Species)
                {
                    if (row.HasGroup) continue;

                    found.Add(new SpeciesRow(row.BotanicalName, row.GroupName, row.Quantity, reading.PlotId));
                }
            }

            return found;
        }

        public static AgreedValue Component(IEnumerable<PlotReading> readings)
        {
            return new AgreedValue(Held(readings).Select(one => new PlotText(one.PlotId, one.Component)));
        }

        public static AgreedValue Reference(IEnumerable<PlotReading> readings)
        {
            return new AgreedValue(Held(readings).Select(one => new PlotText(one.PlotId, one.Reference)));
        }

        private static Totalled Subtotalled(IEnumerable<PlotReading> readings, string heading)
        {
            var perPlot = new List<PlotNumber>();
            foreach (PlotReading reading in Held(readings))
            {
                GroupSubtotal subtotal = reading.SubtotalHeaded(heading);
                if (subtotal == null) continue;

                perPlot.Add(new PlotNumber(reading.PlotId, subtotal.SquareMetres));
            }

            return Totalled.Adding(perPlot);
        }

        private static string Key(SpeciesRow row)
        {
            return row.GroupName.Trim().ToUpperInvariant()
                + " | "
                + row.BotanicalName.Trim().ToUpperInvariant();
        }

        private static IEnumerable<PlotReading> Held(IEnumerable<PlotReading> readings)
        {
            return (readings ?? Enumerable.Empty<PlotReading>()).Where(one => one != null);
        }
    }
}
