using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One sheet the user asked for, on one plot.
    ///
    /// The number and the name are typed by the user and are never invented, so a request short
    /// of either is not a sheet. It is refused by name in the report rather than filled in with
    /// something plausible, because a sheet numbered by a tool is a sheet nobody can find.
    /// </summary>
    public sealed class SheetRequest
    {
        public SheetRequest(string plotId, string sheetNumber, string sheetName)
        {
            if (plotId == null) throw new ArgumentNullException("plotId");

            PlotId = plotId;
            SheetNumber = (sheetNumber ?? string.Empty).Trim();
            SheetName = (sheetName ?? string.Empty).Trim();
        }

        public string PlotId { get; }

        public string SheetNumber { get; }

        public string SheetName { get; }

        public bool Complete
        {
            get { return SheetNumber.Length > 0 && SheetName.Length > 0; }
        }

        /// <summary>
        /// Empty when the request is complete. Otherwise it names which box the user left
        /// blank, because "incomplete" sends somebody hunting across two columns.
        /// </summary>
        public string WhatIsMissing
        {
            get
            {
                if (Complete) return string.Empty;
                if (SheetNumber.Length == 0 && SheetName.Length == 0) return "a sheet number and a sheet name";
                return SheetNumber.Length == 0 ? "a sheet number" : "a sheet name";
            }
        }

        /// <summary>
        /// True when the user typed nothing at all, which is a plot they did not ask for a
        /// sheet on rather than one they filled in badly. Those are dropped without a refusal,
        /// the same way an unticked plot is.
        /// </summary>
        public bool Blank
        {
            get { return SheetNumber.Length == 0 && SheetName.Length == 0; }
        }

        public override string ToString()
        {
            return PlotId + " " + SheetNumber + " " + SheetName;
        }
    }
}
