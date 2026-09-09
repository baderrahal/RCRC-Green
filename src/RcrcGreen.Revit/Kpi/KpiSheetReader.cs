using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RcrcGreen.Core.Kpi;

namespace RcrcGreen.Revit.Kpi
{
    /// <summary>
    /// Section 3. The sheets, the title blocks placed on them, and every parameter on each of
    /// the three places a sheet value can live: the title block instance, its type, and the
    /// sheet itself.
    ///
    /// All three are read because Sheet Width turned out to be an instance parameter that a
    /// type cannot be asked for, and a parameter that is not on the element you ask reads as
    /// a value of zero. Asking all three is cheaper than guessing once.
    /// </summary>
    internal static class KpiSheetReader
    {
        public static TitleBlockFacts Read(Document document)
        {
            if (document == null) throw new ArgumentNullException("document");

            List<ViewSheet> sheets = new FilteredElementCollector(document)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .Where(sheet => !sheet.IsTemplate)
                .ToList();

            var sheetById = new Dictionary<ElementId, ViewSheet>();
            foreach (ViewSheet sheet in sheets) sheetById[sheet.Id] = sheet;

            List<FamilyInstance> blocks = new FilteredElementCollector(document)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .Cast<FamilyInstance>()
                .ToList();

            var perType = new Dictionary<string, int>(StringComparer.Ordinal);
            var typeById = new Dictionary<ElementId, FamilySymbol>();
            var sheetsPerType = new Dictionary<ElementId, HashSet<ElementId>>();

            foreach (FamilyInstance block in blocks)
            {
                FamilySymbol symbol = block.Symbol;
                if (symbol == null) continue;

                ParameterReading.Bump(perType, FamilyAndType(symbol));
                typeById[symbol.Id] = symbol;

                // Sheets, not instances. A sheet carrying a main block and a key plan family
                // made in the title block category is ordinary, and counting instances read
                // as more sheets than the model holds.
                HashSet<ElementId> sheetsUsing;
                if (!sheetsPerType.TryGetValue(symbol.Id, out sheetsUsing))
                {
                    sheetsUsing = new HashSet<ElementId>();
                    sheetsPerType[symbol.Id] = sheetsUsing;
                }
                if (sheetById.ContainsKey(block.OwnerViewId)) sheetsUsing.Add(block.OwnerViewId);
            }

            List<TitleBlockCount> counts = perType
                .Select(pair => new TitleBlockCount(BeforeTheSplit(pair.Key), AfterTheSplit(pair.Key), pair.Value))
                .ToList();

            return new TitleBlockFacts(
                sheets.Count,
                sheets.Count(sheet => sheet.IsPlaceholder),
                blocks.Count,
                counts,
                OnInstances(blocks, sheetById),
                OnTypes(typeById.Values, sheetsPerType),
                OnSheets(sheets));
        }

        private static ParameterHome OnInstances(List<FamilyInstance> blocks, Dictionary<ElementId, ViewSheet> sheetById)
        {
            var values = new List<SheetValue>();

            foreach (FamilyInstance block in blocks)
            {
                ViewSheet sheet;
                sheetById.TryGetValue(block.OwnerViewId, out sheet);
                string number = sheet == null ? KpiQuestions.NoSheet : sheet.SheetNumber;
                string name = sheet == null ? string.Empty : sheet.Name;

                foreach (string wanted in KpiNames.OnSheets)
                {
                    int howMany;
                    Parameter found = ParameterReading.Named(block, wanted, out howMany);
                    if (found == null) continue;

                    values.Add(new SheetValue(wanted, number, name, ParameterReading.Printed(found)));
                }
            }

            return new ParameterHome(
                TitleBlockFacts.OnInstancesWhere,
                blocks.Count,
                ParameterReading.Tally(blocks.Cast<Element>()),
                values);
        }

        /// <summary>
        /// A type has no sheet number, so the two columns carry the family and type and how
        /// many sheets use it instead. The report labels them that way for this home.
        /// </summary>
        private static ParameterHome OnTypes(IEnumerable<FamilySymbol> types, Dictionary<ElementId, HashSet<ElementId>> sheetsPerType)
        {
            List<FamilySymbol> all = types.ToList();
            var values = new List<SheetValue>();

            foreach (FamilySymbol type in all)
            {
                HashSet<ElementId> sheetsUsing;
                int used = sheetsPerType.TryGetValue(type.Id, out sheetsUsing) ? sheetsUsing.Count : 0;

                foreach (string wanted in KpiNames.OnSheets)
                {
                    int howMany;
                    Parameter found = ParameterReading.Named(type, wanted, out howMany);
                    if (found == null) continue;

                    values.Add(new SheetValue(
                        wanted,
                        FamilyAndType(type),
                        "used on " + used + (used == 1 ? " sheet" : " sheets"),
                        ParameterReading.Printed(found)));
                }
            }

            return new ParameterHome(
                TitleBlockFacts.OnTypesWhere,
                all.Count,
                ParameterReading.Tally(all.Cast<Element>()),
                values);
        }

        private static ParameterHome OnSheets(List<ViewSheet> sheets)
        {
            var values = new List<SheetValue>();

            foreach (ViewSheet sheet in sheets)
            {
                foreach (string wanted in KpiNames.OnSheets)
                {
                    int howMany;
                    Parameter found = ParameterReading.Named(sheet, wanted, out howMany);
                    if (found == null) continue;

                    values.Add(new SheetValue(wanted, sheet.SheetNumber, sheet.Name, ParameterReading.Printed(found)));
                }
            }

            return new ParameterHome(
                TitleBlockFacts.OnSheetsWhere,
                sheets.Count,
                ParameterReading.Tally(sheets.Cast<Element>()),
                values);
        }

        private const string Split = " : ";

        private static string FamilyAndType(FamilySymbol symbol)
        {
            return (symbol.FamilyName ?? string.Empty) + Split + (symbol.Name ?? string.Empty);
        }

        private static string BeforeTheSplit(string key)
        {
            int at = key.IndexOf(Split, StringComparison.Ordinal);
            return at < 0 ? key : key.Substring(0, at);
        }

        private static string AfterTheSplit(string key)
        {
            int at = key.IndexOf(Split, StringComparison.Ordinal);
            return at < 0 ? string.Empty : key.Substring(at + Split.Length);
        }
    }
}
