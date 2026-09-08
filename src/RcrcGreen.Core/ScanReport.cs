using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Turns a <see cref="ModelScan"/> into the text that goes in the file.
    ///
    /// Every section heading carries its own count, so a reader can tell a section that found
    /// nothing apart from a section that was never filled in.
    /// </summary>
    public static class ScanReport
    {
        /// <summary>
        /// The file is read in Notepad on Windows, so the line ending is fixed rather than
        /// taken from whatever machine wrote it. That also keeps the tests the same on a
        /// build runner.
        /// </summary>
        public const string LineEnd = "\r\n";

        public static string Write(ModelScan scan, DateTime writtenAt)
        {
            if (scan == null) throw new ArgumentNullException("scan");

            NameParseSummary parsing = NameParseSummary.Of(scan);
            var report = new StringBuilder();

            Line(report, "RCRC Green model scan");
            Line(report, "Document: " + scan.DocumentTitle);
            Line(report, "Written: " + writtenAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
            Line(report, "Read only. Nothing in the model was changed.");
            Line(report, string.Format(
                CultureInfo.InvariantCulture,
                "Elements read looking for PRX_Plot_ID: {0}, in {1} seconds",
                scan.ElementsScanned,
                scan.ScanSeconds.ToString("0.0", CultureInfo.InvariantCulture)));
            Line(report, "A sheet appears once, under SHEETS. It is not repeated in the view sections.");
            Line(report, "Scope box numbers are feet, which is the unit Revit holds them in.");
            Line(report, string.Empty);

            List<ScannedSheet> sheets = scan.Sheets
                .OrderBy(sheet => sheet.SheetNumber, NaturalOrder.Comparer)
                .ToList();
            Heading(report, "SHEETS", sheets.Count,
                "sheet number | sheet name | views on sheet | PRX_Plot_ID");

            // Two counts about the shape of the set rather than about any one sheet. Only one
            // plot in the first model has real sheet numbers, and a group of sheets carries no
            // plot at all, so both are worth a number before anybody reads the list.
            int copies = sheets.Count(sheet => sheet.NumberedAsACopy);
            int noPlot = sheets.Count(sheet => !sheet.HasPlotId);

            Line(report, copies + " of them are numbered as a duplicate, meaning the number holds "
                + ScannedSheet.CopyMark + ".");
            Line(report, noPlot + " of them carry no PRX_Plot_ID, so the Sheet List will not find "
                + "them under any plot.");
            Line(report, "Nothing here was changed. This is a count of what is there.");

            foreach (ScannedSheet sheet in sheets)
            {
                Line(report, Join(
                    sheet.SheetNumber,
                    sheet.SheetName,
                    Count(sheet.ViewsOnSheet, "view"),
                    sheet.HasPlotId ? sheet.PlotId : "(none)"));
            }
            Line(report, string.Empty);

            List<ScannedView> onSheets = scan.ViewsOnSheets
                .OrderBy(view => view.SheetNumber, NaturalOrder.Comparer)
                .ThenBy(view => view.Name, NaturalOrder.Comparer)
                .ToList();
            Heading(report, "VIEWS ON SHEETS", onSheets.Count, "sheet number | view name | view type");
            foreach (ScannedView view in onSheets)
            {
                Line(report, Join(view.SheetNumber, view.Name, view.ViewTypeName));
            }
            Line(report, string.Empty);

            List<ScannedView> loose = scan.ViewsNotOnSheets
                .OrderBy(view => view.Name, NaturalOrder.Comparer)
                .ToList();
            Heading(report, "VIEWS NOT ON SHEETS", loose.Count, "view name | view type");
            foreach (ScannedView view in loose)
            {
                Line(report, Join(view.Name, view.ViewTypeName));
            }
            Line(report, string.Empty);

            List<ScannedView> templates = scan.Templates
                .OrderBy(view => view.Name, NaturalOrder.Comparer)
                .ToList();
            Heading(report, "VIEW TEMPLATES", templates.Count, "template name | view type");
            foreach (ScannedView view in templates)
            {
                Line(report, Join(view.Name, view.ViewTypeName));
            }
            Line(report, string.Empty);

            List<ScannedViewFamilyType> familyTypes = scan.ViewFamilyTypes
                .OrderBy(type => type.ViewFamily, NaturalOrder.Comparer)
                .ThenBy(type => type.Name, NaturalOrder.Comparer)
                .ToList();
            Heading(report, "VIEW FAMILY TYPES", familyTypes.Count, "view family | type name");
            Line(report, "This is what a new view is made with. A type name does not have to match "
                + "the view names that use it, and on this model several do not.");
            foreach (ScannedViewFamilyType type in familyTypes)
            {
                Line(report, Join(type.ViewFamily, type.Name));
            }
            Line(report, string.Empty);

            List<ScannedScopeBox> boxes = scan.ScopeBoxes
                .OrderBy(box => box.Name, NaturalOrder.Comparer)
                .ToList();
            Heading(report, "SCOPE BOXES", boxes.Count, "name | minimum x y z | maximum x y z");
            foreach (ScannedScopeBox box in boxes)
            {
                Line(report, box.HasBounds
                    ? Join(box.Name, Point(box.MinX, box.MinY, box.MinZ), Point(box.MaxX, box.MaxY, box.MaxZ))
                    : Join(box.Name, "no bounding box", "no bounding box"));
            }
            Line(report, string.Empty);

            List<ScannedParameterValue> values = scan.PlotIdValues
                .OrderBy(value => value.Value, NaturalOrder.Comparer)
                .ToList();
            Heading(report, "PRX_Plot_ID VALUES", values.Count, "value | elements carrying it");
            foreach (ScannedParameterValue value in values)
            {
                Line(report, Join(
                    value.Value.Length == 0 ? "(empty)" : value.Value,
                    Count(value.ElementCount, "element")));
            }
            Line(report, string.Empty);

            List<ScannedDisagreement> disagreeing = scan.Disagreements
                .OrderBy(one => one.PlotInTheName, NaturalOrder.Comparer)
                .ThenBy(one => one.ViewName, NaturalOrder.Comparer)
                .ToList();
            Heading(report, "VIEWS THAT DISAGREE WITH THEMSELVES", disagreeing.Count,
                "view name | plot in the name | plot in PRX_Plot_ID");
            Line(report, "Nothing here was changed. The grid follows the name, because a cell "
                + "filled from a disagreeing parameter is a claim no view backs up. Which of the "
                + "two is right is a question about the project.");
            foreach (ScannedDisagreement one in disagreeing)
            {
                Line(report, Join(one.ViewName, one.PlotInTheName, one.PlotInTheParameter));
            }
            Line(report, string.Empty);

            Heading(report, "PARSE SUMMARY", parsing.ParsedTotal + parsing.NotParsedTotal, "names read by the parser");
            Line(report, "View templates are not read here. They are listed above under their own heading.");
            Line(report, string.Empty);

            foreach (NameParseTally tally in parsing.Tallies)
            {
                Line(report, string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}: {1} of {2} parsed, {3} did not",
                    tally.Kind,
                    tally.Parsed,
                    tally.Total,
                    tally.NotParsed.Count));
            }

            foreach (NameParseTally tally in parsing.Tallies)
            {
                if (tally.NotParsed.Count == 0) continue;

                Line(report, string.Empty);
                Line(report, "Did not parse, " + tally.Kind + ", showing "
                    + Math.Min(NameParseSummary.ShownPerKind, tally.NotParsed.Count)
                    + " of " + tally.NotParsed.Count + ":");

                foreach (string refused in tally.NotParsed.Take(NameParseSummary.ShownPerKind))
                {
                    Line(report, "  " + refused);
                }
            }

            return report.ToString();
        }

        private static void Heading(StringBuilder report, string title, int count, string columns)
        {
            Line(report, "== " + title + " (" + count.ToString(CultureInfo.InvariantCulture) + ") ==");
            Line(report, columns);
        }

        private static string Join(params string[] fields)
        {
            return string.Join(" | ", fields);
        }

        private static string Point(double x, double y, double z)
        {
            return string.Format(
                CultureInfo.InvariantCulture, "{0:0.###} {1:0.###} {2:0.###}", x, y, z);
        }

        private static string Count(int howMany, string thing)
        {
            return howMany.ToString(CultureInfo.InvariantCulture)
                + " " + thing + (howMany == 1 ? string.Empty : "s");
        }

        private static void Line(StringBuilder report, string text)
        {
            report.Append(text);
            report.Append(LineEnd);
        }
    }
}
