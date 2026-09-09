using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The six values the finished tool reads out of Revit and writes into a workbook.
    /// </summary>
    public enum KpiValue
    {
        Component,
        Reference,
        Location,
        Area,
        Shrubs,
        Lawn
    }

    /// <summary>
    /// One cell the map names on the main sheet: which value goes in it and where it sits.
    /// </summary>
    public sealed class MappedCell
    {
        public MappedCell(KpiValue value, string cellRef)
        {
            if (cellRef == null) throw new ArgumentNullException("cellRef");
            CellRef.Parse(cellRef);

            Value = value;
            Cell = cellRef;
        }

        public KpiValue Value { get; }

        public string Cell { get; }
    }

    /// <summary>
    /// Where the quantities go on one tree list sheet. The row range comes from the map entry
    /// and never from a constant, because writing 89 rows into an 80 row list puts nine
    /// quantities into rows no total sums.
    /// </summary>
    public sealed class TreeRows
    {
        public TreeRows(string sheetName, int firstRow, int lastRow)
        {
            if (sheetName == null) throw new ArgumentNullException("sheetName");
            if (firstRow < 1) throw new ArgumentOutOfRangeException("firstRow");
            if (lastRow < firstRow) throw new ArgumentOutOfRangeException("lastRow");

            SheetName = sheetName;
            FirstRow = firstRow;
            LastRow = lastRow;
        }

        public string SheetName { get; }

        public int FirstRow { get; }

        public int LastRow { get; }

        public int RowCount
        {
            get { return LastRow - FirstRow + 1; }
        }

        /// <summary>
        /// B4 to B92 as the pane prints it.
        /// </summary>
        public string InWords
        {
            get
            {
                return KpiTemplates.QuantityColumn + FirstRow + " to " + KpiTemplates.QuantityColumn + LastRow;
            }
        }
    }

    /// <summary>
    /// One template of the GRP KPI Checklist workbook: how it is recognised and every cell
    /// the tool would fill. Built from the annotated set, measured cell by cell, because the
    /// production templates carry no note saying where any value comes from and the team
    /// decided the tool carries the map.
    /// </summary>
    public sealed class KpiTemplate
    {
        public KpiTemplate(
            string name,
            string mainSheetName,
            IEnumerable<MappedCell> cells,
            TreeRows existingTrees,
            TreeRows proposedTrees)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (mainSheetName == null) throw new ArgumentNullException("mainSheetName");
            if (existingTrees == null) throw new ArgumentNullException("existingTrees");
            if (proposedTrees == null) throw new ArgumentNullException("proposedTrees");

            Name = name;
            MainSheetName = mainSheetName;
            Cells = (cells ?? Enumerable.Empty<MappedCell>()).Where(cell => cell != null).ToList();
            ExistingTrees = existingTrees;
            ProposedTrees = proposedTrees;
        }

        public string Name { get; }

        /// <summary>
        /// The first sheet's name, which is how a workbook is recognised. It changes per
        /// template while the other four sheet names do not.
        /// </summary>
        public string MainSheetName { get; }

        /// <summary>
        /// The cells on the main sheet that get a value from Revit. STREETS has no area cell,
        /// because the user types the road width and the total length by hand and the sheet
        /// works the area out.
        /// </summary>
        public IReadOnlyList<MappedCell> Cells { get; }

        public TreeRows ExistingTrees { get; }

        public TreeRows ProposedTrees { get; }

        public MappedCell CellFor(KpiValue value)
        {
            return Cells.FirstOrDefault(cell => cell.Value == value);
        }

        public bool AreaIsTypedByHand
        {
            get { return CellFor(KpiValue.Area) == null; }
        }
    }
}
