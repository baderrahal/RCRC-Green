using System;
using RcrcGreen.Core.Kpi;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// Builds a <see cref="KpiScan"/> the way the Revit side would, so a test can name only
    /// the part it cares about. <see cref="RealShaped"/> is the first real model as the scan
    /// saw it, with the numbers CLAUDE.md records.
    /// </summary>
    internal static class KpiFixture
    {
        public const string RealTitle = "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached";

        public const string TitleBlockFamily = "AR-PRX-Title_Block_A1";

        public const string LinkTypeName = "RCRC_NG05_NU_00_SITE_RVT24.rvt";

        public const string LinkInstanceName = "RCRC_NG05_NU_00_SITE_RVT24.rvt : 1";

        public const string Softscape = "DM-11-(610) SOFTSCAPE SCHEDULE";

        public const string ShrubsAndLawn = "DM-11-(620) SHRUBS AND LAWN SCHEDULE";

        public const string Hardscape = "DM-11-(600) HARDSCAPE SCHEDULE";

        public static readonly ProjectUnit SquareMetres =
            new ProjectUnit("Square meters", "autodesk.unit.unit:squareMeters-1.0.1", 0.01);

        public static readonly ProjectUnit Millimetres =
            new ProjectUnit("Millimeters", "autodesk.unit.unit:millimeters-1.0.1", 1.0);

        public static KpiScan Build(
            string title = "NG05",
            string path = "",
            int elementInstances = 0,
            int elementTypes = 0,
            double readSeconds = 0.0,
            ProjectUnit area = null,
            ProjectUnit length = null,
            ReadParameter[] projectInformation = null,
            TitleBlockFacts titleBlocks = null,
            LinkFacts links = null,
            ScheduleFacts schedules = null,
            string[] skipped = null,
            bool projectInformationRead = true)
        {
            var document = new DocumentFacts(title, path, elementInstances, elementTypes, readSeconds, area, length);
            return new KpiScan(document, projectInformation, titleBlocks, links, schedules, skipped, projectInformationRead);
        }

        /// <summary>
        /// A parameter with no printed text has no value at all, the way an unset shared
        /// parameter reads, and one printed as empty text has a value that is empty.
        /// </summary>
        public static ReadParameter Parameter(
            string name,
            string printed = null,
            string kind = ReadParameter.Shared,
            string storageType = "String",
            string guid = "",
            string raw = null)
        {
            return new ReadParameter(name, kind, storageType, guid, printed != null, printed ?? string.Empty, raw ?? printed);
        }

        public static ParameterTally Tally(string name, int carrying, int withValue)
        {
            return new ParameterTally(name, carrying, withValue);
        }

        public static SheetValue Value(string parameterName, string sheetNumber, string sheetName, string value)
        {
            return new SheetValue(parameterName, sheetNumber, sheetName, value);
        }

        public static ParameterHome Home(
            string where,
            int elementCount,
            ParameterTally[] tallies = null,
            SheetValue[] values = null)
        {
            return new ParameterHome(where, elementCount, tallies, values);
        }

        public static ParameterHome OnInstances(int elementCount, ParameterTally[] tallies = null, SheetValue[] values = null)
        {
            return Home(TitleBlockFacts.OnInstancesWhere, elementCount, tallies, values);
        }

        public static ParameterHome OnTypes(int elementCount, ParameterTally[] tallies = null, SheetValue[] values = null)
        {
            return Home(TitleBlockFacts.OnTypesWhere, elementCount, tallies, values);
        }

        public static ParameterHome OnSheets(int elementCount, ParameterTally[] tallies = null, SheetValue[] values = null)
        {
            return Home(TitleBlockFacts.OnSheetsWhere, elementCount, tallies, values);
        }

        public static TitleBlockFacts TitleBlocks(
            int sheetCount = 0,
            int placeholderCount = 0,
            int titleBlockInstances = 0,
            TitleBlockCount[] titleBlocks = null,
            ParameterHome onInstances = null,
            ParameterHome onTypes = null,
            ParameterHome onSheets = null)
        {
            return new TitleBlockFacts(sheetCount, placeholderCount, titleBlockInstances, titleBlocks, onInstances, onTypes, onSheets);
        }

        public static ScannedLinkType LinkType(string name, string status = "Loaded", bool isLoaded = true, bool isNested = false)
        {
            return new ScannedLinkType(name, status, isLoaded, isNested);
        }

        public static ScannedLinkInstance LinkInstance(string name, string typeName, bool isLoaded = true)
        {
            return new ScannedLinkInstance(name, typeName, isLoaded);
        }

        public static LinkFacts Links(
            ScannedLinkType[] types = null,
            ScannedLinkInstance[] instances = null,
            LinkContents[] contents = null)
        {
            return new LinkFacts(types, instances, contents);
        }

        public static LinkContents Contents(
            string linkName,
            string documentTitle = "",
            int filledRegionCount = 0,
            NameCount[] typeCounts = null,
            NameCount[] viewCounts = null,
            FilledRegionRead[] firstRegions = null,
            ParameterTally[] regionParameters = null,
            MeasuredValue[] interventionAreas = null)
        {
            return new LinkContents(linkName, documentTitle, filledRegionCount, typeCounts, viewCounts,
                firstRegions, regionParameters, interventionAreas);
        }

        public static NameCount Counted(string name, int count)
        {
            return new NameCount(name, count);
        }

        public static ScheduleFieldRead Field(
            string heading,
            string parameterName = null,
            string fieldType = "Instance",
            string spec = "",
            string unitLabel = "",
            bool isHidden = false)
        {
            return new ScheduleFieldRead(heading, parameterName ?? heading, fieldType, isHidden, spec, unitLabel);
        }

        public static ScheduleFilterRead Filter(string fieldName, string rule, string value)
        {
            return new ScheduleFilterRead(fieldName, rule, value);
        }

        public static ScannedSchedule Schedule(
            string name,
            string categoryName = "Planting",
            bool onASheet = true,
            ScheduleFieldRead[] fields = null,
            ScheduleFilterRead[] filters = null,
            string phaseName = "",
            string phaseFilterName = "",
            bool rowsWereRead = false,
            int bodyRowCount = 0,
            string[][] rows = null,
            bool rowsRefused = false)
        {
            // rowsWereRead is the ordinary full read. rowsRefused is a full read whose rows
            // Revit refused, so the schedule is read in full and holds no rows.
            return new ScannedSchedule(name, categoryName, onASheet, fields, filters, phaseName, phaseFilterName,
                rowsWereRead || rowsRefused, rowsWereRead, bodyRowCount, rows);
        }

        public static ScheduleElements Elements(
            string scheduleName,
            int elementCount = 0,
            NameCount[] categories = null,
            NameCount[] familyTypes = null,
            ParameterTally[] instanceParameters = null,
            ParameterTally[] typeParameters = null,
            NameCount[] createdPhases = null,
            NameCount[] demolishedPhases = null,
            NameCount[] worksets = null,
            NameCount[] designOptions = null,
            ParameterValueCount[] wordValues = null)
        {
            return new ScheduleElements(scheduleName, elementCount, categories, familyTypes, instanceParameters,
                typeParameters, createdPhases, demolishedPhases, worksets, designOptions, wordValues);
        }

        public static ScheduleFacts Schedules(
            ScannedSchedule[] schedules = null,
            int templateCount = 0,
            string[] phases = null,
            ScheduleElements[] elements = null,
            MeasuredArea[] areas = null)
        {
            return new ScheduleFacts(schedules, templateCount, phases, elements, areas);
        }

        public static ScheduleFieldRead[] SoftscapeFields()
        {
            return new[]
            {
                Field("BOTANICAL NAME", "PRX_Botanical Name"),
                Field("COMMON NAME", "PRX_Common Name"),
                Field("SIZE", "PRX_Tree Size", "ElementType"),
                Field("ENTER EACH PROPOSED TREE QUANTITY", "Count", "Count"),
                Field("PRX_Ref Plot ID", "PRX_Ref Plot ID", isHidden: true)
            };
        }

        public static ScannedSchedule SoftscapeReadInFull(string plot = "DM-11")
        {
            return Schedule(
                plot + "-(610) SOFTSCAPE SCHEDULE",
                fields: SoftscapeFields(),
                filters: new[] { Filter("PRX_Ref Plot ID", "Equals", plot) },
                phaseName: "New Construction",
                phaseFilterName: "Show All",
                rowsWereRead: true,
                bodyRowCount: 4,
                rows: new[]
                {
                    new[] { "BOTANICAL NAME", "COMMON NAME", "SIZE", "ENTER EACH PROPOSED TREE QUANTITY" },
                    new[] { "Acacia tortilis", "Umbrella thorn", "3 m", "30" },
                    new[] { "Ziziphus spina-christi", "Sidr", "3 m", "27" },
                    new[] { "", "", "", "57" }
                });
        }

        public static ScheduleElements SoftscapeElements(string scheduleName = Softscape)
        {
            return Elements(
                scheduleName,
                elementCount: 57,
                categories: new[] { Counted("Planting", 57) },
                familyTypes: new[] { Counted("Tree : Ziziphus spina-christi", 27), Counted("Tree : Acacia tortilis", 30) },
                instanceParameters: new[]
                {
                    Tally("PRX_Botanical Name", 57, 57),
                    Tally("PRX_Ref Plot ID", 57, 57),
                    Tally("Comments", 57, 3),
                    Tally("Phase Created", 57, 57)
                },
                typeParameters: new[] { Tally("PRX_Tree Size", 2, 2) },
                createdPhases: new[] { Counted("Existing", 12), Counted("Proposed", 45) },
                demolishedPhases: new[] { Counted("None", 57) },
                worksets: new[] { Counted("Planting", 57) },
                designOptions: new[] { Counted("Main Model", 57) },
                wordValues: new[]
                {
                    new ParameterValueCount("PRX_Botanical Name", "Acacia tortilis", 30, false),
                    new ParameterValueCount("PRX_Botanical Name", "Ziziphus spina-christi", 27, false),
                    new ParameterValueCount("PRX_Tree Size", "3 m", 57, true),
                    new ParameterValueCount("Phase Created", "Existing", 12, false),
                    new ParameterValueCount("Phase Created", "Proposed", 45, false)
                });
        }

        public static KpiScan RealShaped()
        {
            var onInstances = OnInstances(
                1383,
                new[]
                {
                    Tally("PRX_COMPONENT", 1383, 1201),
                    Tally("PRX_Plot_ID", 1383, 160),
                    Tally("Sheet Width", 1383, 1383),
                    Tally("Sheet Height", 1383, 1383)
                },
                new[]
                {
                    Value("PRX_COMPONENT", "DM-11-600QD", "SOFTSCAPE SCHEDULES", "SOFTSCAPE"),
                    Value("PRX_COMPONENT", "DM-11-610QD", "HARDSCAPE SCHEDULES", "HARDSCAPE"),
                    Value("PRX_COMPONENT", "010QE Copy 001", "Copy of key plan", "")
                });

            var onTypes = OnTypes(2, new[] { Tally("Type Name", 2, 2), Tally("Keynote", 2, 0) });

            var onSheets = OnSheets(
                1385,
                new[]
                {
                    Tally("Sheet Number", 1385, 1385),
                    Tally("Sheet Name", 1385, 1385),
                    Tally("PRX_Plot_ID", 1385, 1200)
                });

            var titleBlocks = TitleBlocks(
                sheetCount: 1385,
                placeholderCount: 2,
                titleBlockInstances: 1383,
                titleBlocks: new[]
                {
                    new TitleBlockCount(TitleBlockFamily, "A1 Metric Key Plan", 83),
                    new TitleBlockCount(TitleBlockFamily, "A1 Metric", 1300)
                },
                onInstances: onInstances,
                onTypes: onTypes,
                onSheets: onSheets);

            var siteLink = Contents(
                LinkInstanceName,
                documentTitle: "RCRC_NG05_NU_00_SITE_RVT24",
                filledRegionCount: 412,
                typeCounts: new[] { Counted("Diagonal Hatch", 300), Counted("ID Intervention Area", 112) },
                viewCounts: new[] { Counted("Site Plan", 412) },
                firstRegions: new[]
                {
                    new FilledRegionRead("ID Intervention Area", "Site Plan", new[]
                    {
                        Parameter("PRX_Intervention Area", "1000.00 m²", storageType: "Double", raw: "10763.91"),
                        Parameter("Area", "1000.00 m²", ReadParameter.BuiltIn, "Double", raw: "10763.91")
                    })
                },
                regionParameters: new[]
                {
                    Tally("PRX_Intervention Area", 412, 398),
                    Tally("Area", 412, 412),
                    Tally("Comments", 412, 0)
                },
                interventionAreas: new[]
                {
                    new MeasuredValue("ID Intervention Area", "Area", "10763.91", "1000.00 m²"),
                    new MeasuredValue("ID Intervention Area", "Area", "5381.96", "500.00 m²")
                });

            var links = Links(
                new[]
                {
                    LinkType("RCRC_NG05_NU_01_ARCH_RVT24.rvt", "Unloaded", isLoaded: false),
                    LinkType(LinkTypeName)
                },
                new[] { LinkInstance(LinkInstanceName, LinkTypeName) },
                new[] { siteLink });

            var schedules = Schedules(
                schedules: new[]
                {
                    Schedule("DM-12-(620) SHRUBS AND LAWN SCHEDULE", "Floors", fields: FloorFields(),
                        filters: new[] { Filter("PRX_Ref Plot ID", "Equals", "DM-12"), Filter("Type Comments", "Contains", "SHRUB") }),
                    Schedule("DM-12-(600) HARDSCAPE SCHEDULE", "Floors", fields: FloorFields(),
                        filters: new[] { Filter("PRX_Ref Plot ID", "Equals", "DM-12"), Filter("Type Comments", "Contains", "HARD") }),
                    Schedule("DM-12-(610) SOFTSCAPE SCHEDULE", fields: SoftscapeFields(),
                        filters: new[] { Filter("PRX_Ref Plot ID", "Equals", "DM-12") }),
                    SoftscapeReadInFull(),
                    Schedule(ShrubsAndLawn, "Floors", fields: FloorFields(),
                        filters: new[] { Filter("PRX_Ref Plot ID", "Equals", "DM-11"), Filter("Type Comments", "Contains", "SHRUB") }),
                    Schedule(Hardscape, "Floors", fields: FloorFields(),
                        filters: new[] { Filter("PRX_Ref Plot ID", "Equals", "DM-11"), Filter("Type Comments", "Contains", "HARD") }),
                    Schedule("Sheet List", "Sheets", onASheet: false,
                        fields: new[] { Field("Sheet Number"), Field("Sheet Name"), Field("PRX_Plot_ID") })
                },
                templateCount: 3,
                phases: new[] { "Existing", "Proposed" },
                elements: new[] { SoftscapeElements() },
                areas: new[]
                {
                    new MeasuredArea(Hardscape, "Area", "Floors 412563", 1000.0, "92.90 m²"),
                    new MeasuredArea(ShrubsAndLawn, "Area", "Floors 412580", 500.0, "46.45 m²")
                });

            return Build(
                title: RealTitle,
                path: "C:\\Models\\" + RealTitle + ".rvt",
                elementInstances: 96934,
                elementTypes: 11208,
                readSeconds: 1.4,
                area: SquareMetres,
                length: Millimetres,
                projectInformation: new[]
                {
                    Parameter("Project Name", "NG05 Neighbourhood Unit", ReadParameter.BuiltIn),
                    Parameter("Project Number"),
                    Parameter("Client Name", "", ReadParameter.BuiltIn),
                    Parameter("PRX_Neighbourhood", "NU05", guid: "8f0b1c2d-3e4f-4a5b-8c6d-7e8f9a0b1c2d")
                },
                titleBlocks: titleBlocks,
                links: links,
                schedules: schedules);
        }

        private static ScheduleFieldRead[] FloorFields()
        {
            return new[]
            {
                Field("Type"),
                Field("Area", spec: "Area"),
                Field("PRX_Ref Plot ID", isHidden: true)
            };
        }
    }
}
