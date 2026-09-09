using System;
using RcrcGreen.Core;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// Builds a <see cref="ModelScan"/> the way the Revit side would, so a test can name only
    /// the part it cares about.
    /// </summary>
    internal static class ScanFixture
    {
        public static ModelScan Build(
            string documentTitle = "NG05",
            ScannedSheet[] sheets = null,
            ScannedView[] views = null,
            ScannedViewFamilyType[] viewFamilyTypes = null,
            ScannedScopeBox[] scopeBoxes = null,
            ScannedParameterValue[] plotIdValues = null,
            ScannedDisagreement[] disagreements = null,
            int elementsScanned = 0,
            double scanSeconds = 0.0,
            ViewportRecord[] viewports = null)
        {
            return new ModelScan(
                documentTitle,
                sheets ?? new ScannedSheet[0],
                views ?? new ScannedView[0],
                viewFamilyTypes ?? new ScannedViewFamilyType[0],
                scopeBoxes ?? new ScannedScopeBox[0],
                plotIdValues ?? new ScannedParameterValue[0],
                disagreements ?? new ScannedDisagreement[0],
                elementsScanned,
                scanSeconds,
                viewports ?? new ViewportRecord[0]);
        }

        public static ScannedView View(string name, string viewTypeName)
        {
            return new ScannedView(name, viewTypeName, false, string.Empty);
        }

        public static ScannedView ViewOn(string sheetNumber, string name, string viewTypeName)
        {
            return new ScannedView(name, viewTypeName, false, sheetNumber);
        }

        public static ScannedView Template(string name, string viewTypeName)
        {
            return new ScannedView(name, viewTypeName, true, string.Empty);
        }

        public static ScannedScopeBox Box(string name, double minX, double minY, double maxX, double maxY)
        {
            return new ScannedScopeBox(name, true, minX, minY, 0, maxX, maxY, 30);
        }
    }
}
