using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB;
using RcrcGreen.Core.Kpi;

namespace RcrcGreen.Revit.Kpi
{
    /// <summary>
    /// Reads a document into a <see cref="KpiScan"/>. Nothing here opens a transaction,
    /// because nothing here writes, and nothing here touches a workbook.
    ///
    /// Each section is read under its own guard. A model this tool has never seen is exactly
    /// where a read is most likely to be refused, and losing eight sections because the ninth
    /// threw would turn a measuring round into a crash report. What did not happen is named at
    /// the top of the file, so a section that was refused never reads as a section that found
    /// nothing.
    /// </summary>
    internal static class KpiReader
    {
        public static KpiScan Read(Document document, Action<string> progressed)
        {
            if (document == null) throw new ArgumentNullException("document");

            var skipped = new List<string>();
            Stopwatch clock = Stopwatch.StartNew();

            // Each line below is raised as the work it names begins, never off a timer. The
            // words come off the report's own section headings, so the numbers on screen are
            // the report's numbers.
            progressed?.Invoke(ProgressWords.Section(KpiReport.ProjectInformation));

            // Each fallback says it was never filled, so a section whose read threw prints
            // NOT READ and never NOT FOUND. An empty list read as a model holding nothing.
            IReadOnlyList<ReadParameter> projectInformation = Guarded(
                skipped, KpiReport.ProjectInformation,
                () => ProjectInformation(document),
                why => null);

            progressed?.Invoke(ProgressWords.Section(KpiReport.TitleBlocksAndSheets));
            TitleBlockFacts titleBlocks = Guarded(
                skipped, KpiReport.TitleBlocksAndSheets,
                () => KpiSheetReader.Read(document),
                TitleBlockFacts.NotRead);

            progressed?.Invoke(ProgressWords.Section(KpiReport.LinkedModels));
            LinkFacts links = Guarded(
                skipped, KpiReport.LinkedModels,
                () => KpiLinkReader.Read(document, skipped),
                LinkFacts.NotRead);

            progressed?.Invoke(ProgressWords.SectionSpan(KpiReport.Schedules, KpiReport.AreasAndUnits));
            ScheduleFacts schedules = Guarded(
                skipped, KpiReport.Schedules + " to " + KpiReport.AreasAndUnits,
                () => KpiScheduleReader.Read(document, skipped, progressed),
                ScheduleFacts.NotRead);

            clock.Stop();

            DocumentFacts facts = Guarded(
                skipped, KpiReport.Document,
                () => Facts(document, clock.Elapsed.TotalSeconds, skipped),
                why => new DocumentFacts(document.Title, string.Empty, 0, 0, clock.Elapsed.TotalSeconds, null, null));

            return new KpiScan(
                facts, projectInformation, titleBlocks, links, schedules, skipped, projectInformation != null);
        }

        /// <summary>
        /// Runs one section's read and hands back the fallback, built from the failure, when
        /// it throws. Every exception type is caught here on purpose, which this repo
        /// otherwise avoids: the section name, the exception type and its message all go in
        /// the file, and a partial report that says what is missing is the point of the guard.
        /// </summary>
        private static T Guarded<T>(List<string> skipped, string section, Func<T> read, Func<string, T> fallback)
        {
            try
            {
                return read();
            }
            catch (Exception failed)
            {
                string why = failed.GetType().Name + ": " + failed.Message;
                skipped.Add("Section " + section + " was not read. " + why);
                return fallback(why);
            }
        }

        private static DocumentFacts Facts(Document document, double seconds, List<string> skipped)
        {
            int instances = new FilteredElementCollector(document).WhereElementIsNotElementType().GetElementCount();
            int types = new FilteredElementCollector(document).WhereElementIsElementType().GetElementCount();

            Units units = document.GetUnits();

            return new DocumentFacts(
                document.Title,
                document.PathName ?? string.Empty,
                instances,
                types,
                seconds,
                Unit(units, SpecTypeId.Area, "area", skipped),
                Unit(units, SpecTypeId.Length, "length", skipped));
        }

        /// <summary>
        /// The project's area unit for the create path, the same read the scan makes, so the
        /// rounding room the group total check allows comes off the one setting the report
        /// prints as Area unit, rounded to. Unknown when it cannot be read, and the check then
        /// allows nothing and says the step was not read where that decided anything.
        /// </summary>
        public static ProjectUnit AreaUnit(Document document)
        {
            if (document == null) throw new ArgumentNullException("document");

            return Unit(document.GetUnits(), SpecTypeId.Area, "area", new List<string>());
        }

        /// <summary>
        /// The project's display unit for one kind of measurement, with Revit's id for it and
        /// the rounding, which together are what turn a raw number into a printed one.
        /// </summary>
        private static ProjectUnit Unit(Units units, ForgeTypeId spec, string what, List<string> skipped)
        {
            try
            {
                FormatOptions options = units.GetFormatOptions(spec);
                ForgeTypeId unitId = options.GetUnitTypeId();

                return new ProjectUnit(
                    LabelUtils.GetLabelForUnit(unitId),
                    unitId.TypeId,
                    options.Accuracy);
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException failed)
            {
                skipped.Add("The project " + what + " unit was not read. " + failed.Message);
                return ProjectUnit.Unknown;
            }
            catch (InvalidOperationException failed)
            {
                skipped.Add("The project " + what + " unit was not read. " + failed.Message);
                return ProjectUnit.Unknown;
            }
        }

        /// <summary>
        /// Every parameter on the Project Information element, all of them and no cap, because
        /// the neighbourhood name is one of them and nothing here knows which.
        /// </summary>
        private static List<ReadParameter> ProjectInformation(Document document)
        {
            ProjectInfo information = document.ProjectInformation;
            if (information == null) throw new InvalidOperationException("The document has no Project Information element.");

            return ParameterReading.ReadAll(information);
        }
    }
}
