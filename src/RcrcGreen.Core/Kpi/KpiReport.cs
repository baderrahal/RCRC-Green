using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Turns a <see cref="KpiScan"/> into the text file. Nine numbered sections, one per
    /// question, and every heading carries its own count so a section that found nothing
    /// reads differently from one that was never filled in.
    ///
    /// Nothing here decides an answer. A NOT FOUND is printed with the near misses beside it,
    /// because a near miss that stays silent is how PRX_Plot_ID and PRX_Ref Plot ID came to be
    /// mistaken for one parameter.
    /// </summary>
    public static class KpiReport
    {
        public const string LineEnd = "\r\n";

        public const string Document = "1 DOCUMENT";

        public const string ProjectInformation = "2 PROJECT INFORMATION";

        public const string TitleBlocksAndSheets = "3 TITLE BLOCKS AND SHEETS";

        public const string LinkedModels = "4 LINKED MODELS";

        public const string Schedules = "5 SCHEDULES";

        public const string SoftscapeFields = "6 SOFTSCAPE SCHEDULE FIELDS";

        public const string ExistingAndProposed = "7 EXISTING AND PROPOSED";

        public const string AreasAndUnits = "8 AREAS AND UNITS";

        public const string TheNineQuestions = "9 THE NINE QUESTIONS";

        public const string NotFound = "NOT FOUND";

        /// <summary>
        /// Printed under a heading whose read threw, in place of its body, so an empty section
        /// never reads as a model that holds nothing.
        /// </summary>
        public const string NotReadLine = "NOT READ. This section's read did not happen. READS THAT DID NOT HAPPEN at the top says why.";

        /// <summary>
        /// The brief's cap on examples per name. A list of twenty says what a value looks
        /// like, and the count beside it says how many there were.
        /// </summary>
        public const int ShownExamples = 20;

        /// <summary>
        /// The softscape lists in the client workbook run 80 to 89 species, so a cap of 30
        /// lost about 55 of them from the one section this round exists to fill. When fewer
        /// rows print than exist, the last row read is still the schedule's last row.
        /// </summary>
        public const int ShownRows = 200;

        /// <summary>
        /// How many plots of one schedule name are read in full. The Revit reader reads this
        /// rather than holding its own copy, because the report states the rule and a second
        /// copy of a number is the fault this repo has hit seven times. One plot cannot show
        /// whether the group headings repeat across plots or whether an Existing group appears.
        /// </summary>
        public const int PlotsReadInFull = 3;

        /// <summary>
        /// How many plots of filled regions print with their regions listed under them. Named
        /// rather than typed twice, because the heading counts and the loop takes, and two
        /// literals holding one rule is the fault this repo keeps meeting.
        /// </summary>
        public const int ShownPlots = 3;

        public const int ShownValues = 30;

        public const int ShownFamilyTypes = 40;

        public static string Write(KpiScan scan, DateTime writtenAt)
        {
            if (scan == null) throw new ArgumentNullException("scan");

            var report = new StringBuilder();

            Line(report, "RCRC Green KPI scan");
            Line(report, "Document: " + scan.Document.Title);
            Line(report, "Written: " + writtenAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
            Line(report, "Read only. Nothing in the model was changed and no workbook was touched.");
            Line(report, "Nine sections, one per question the KPI tool has to have answered before it can "
                + "fill a cell. Section 9 is one line per question and is the place to start.");
            Line(report, "Every heading carries a count, so a section that found nothing reads differently "
                + "from one that was never filled in.");
            Line(report, string.Empty);

            WhatDidNotHappen(report, scan);
            TheDocument(report, scan);
            TheProjectInformation(report, scan);
            TheTitleBlocksAndSheets(report, scan);
            TheLinks(report, scan);
            TheSchedules(report, scan);
            TheSoftscapeFields(report, scan);
            TheExistingAndProposed(report, scan);
            TheAreas(report, scan);
            TheQuestions(report, scan);

            return report.ToString();
        }

        /// <summary>
        /// First, before any section, because a section that was refused would otherwise read
        /// as a section that found nothing.
        /// </summary>
        private static void WhatDidNotHappen(StringBuilder report, KpiScan scan)
        {
            Heading(report, "READS THAT DID NOT HAPPEN", scan.Skipped.Count, "what was skipped, and why");
            if (scan.Skipped.Count == 0)
            {
                Line(report, "Every read ran. A zero anywhere below is a real zero.");
            }

            foreach (string skipped in scan.Skipped)
            {
                Line(report, "  " + skipped);
            }
            Line(report, string.Empty);
        }

        private static void TheDocument(StringBuilder report, KpiScan scan)
        {
            DocumentFacts facts = scan.Document;

            Heading(report, Document, facts.ElementInstances, "elements that are not types");
            Line(report, "Title: " + facts.Title);
            Line(report, "Path: " + (facts.Path.Length == 0 ? "(not saved, so there is no path)" : facts.Path));
            Line(report, "Elements: " + Count(facts.ElementInstances, "instance") + " and "
                + Count(facts.ElementTypes, "type"));
            Line(report, "Read in " + facts.ReadSeconds.ToString("0.0", CultureInfo.InvariantCulture) + " seconds");
            Line(report, "Area unit: " + Unit(facts.Area));
            Line(report, "Length unit: " + Unit(facts.Length));
            Line(report, "Revit holds every length in feet and every area in square feet whatever the "
                + "project shows. Every raw number in this file is in those, and every printed one is "
                + "in the project unit above with its rounding.");
            Line(report, string.Empty);
        }

        private static string Unit(ProjectUnit unit)
        {
            if (!unit.IsKnown) return "UNKNOWN, the unit setting could not be read";

            string said = unit.Label;
            if (unit.Id.Length > 0) said += ", id " + unit.Id;
            if (!double.IsNaN(unit.Accuracy)) said += ", rounded to " + Step(unit.Accuracy);
            return said;
        }

        /// <summary>
        /// A rounding step printed with every place it has. Revit rounds finer than four
        /// places, and 0.00001 printed through the four place format read as rounded to 0.
        /// </summary>
        public static string Step(double accuracy)
        {
            return accuracy.ToString("0.############", CultureInfo.InvariantCulture);
        }

        private static void TheProjectInformation(StringBuilder report, KpiScan scan)
        {
            List<ReadParameter> all = scan.ProjectInformation
                .OrderBy(one => one.Name, NaturalOrder.Comparer)
                .ToList();

            Heading(report, ProjectInformation, all.Count, "name | shared, built-in, or project or family | storage | GUID | value");

            if (!scan.ProjectInformationRead)
            {
                Line(report, NotReadLine);
                Line(report, string.Empty);
                return;
            }

            Line(report, "Every parameter on the Project Information element, no cap. The neighbourhood "
                + "name is one of these.");

            foreach (ReadParameter one in all)
            {
                Line(report, Join(one.Name, one.Kind, one.StorageType,
                    one.Guid.Length == 0 ? "-" : one.Guid,
                    Value(one)));
            }

            Line(report, string.Empty);
            List<string> near = all
                .Where(one => KpiNames.HoldsAny(one.Name, KpiNames.NeighbourhoodNearMisses))
                .Select(one => one.Name)
                .ToList();

            string words = Words(KpiNames.NeighbourhoodNearMisses);
            Line(report, near.Count == 0
                ? "No name holds " + words + ". The neighbourhood name is not in Project Information "
                    + "under any of those words, so the team picks from the list above."
                : "Names holding " + words + ": " + string.Join(", ", near.ToArray()) + ".");
            Line(report, string.Empty);
        }

        private static void TheTitleBlocksAndSheets(StringBuilder report, KpiScan scan)
        {
            TitleBlockFacts facts = scan.TitleBlocks;

            Heading(report, TitleBlocksAndSheets, facts.SheetCount, "sheets");

            if (!facts.WasRead)
            {
                Line(report, NotReadLine + " " + facts.WhyNotRead);
                Line(report, string.Empty);
                return;
            }

            Line(report, Count(facts.SheetCount, "sheet") + ", of which " + facts.PlaceholderCount
                + (facts.PlaceholderCount == 1 ? " is a placeholder" : " are placeholders")
                + " with no title block.");
            Line(report, "Title block instances: " + facts.TitleBlockInstances);
            Line(report, string.Empty);

            List<TitleBlockCount> blocks = facts.TitleBlocks
                .OrderBy(one => one.FamilyName, NaturalOrder.Comparer)
                .ThenBy(one => one.TypeName, NaturalOrder.Comparer)
                .ToList();
            Line(report, "TITLE BLOCK FAMILIES AND TYPES, " + blocks.Count);
            Line(report, "family | type | instances");
            foreach (TitleBlockCount block in blocks)
            {
                Line(report, Join(block.FamilyName, block.TypeName, Count(block.Instances, "instance")));
            }
            Line(report, string.Empty);

            foreach (ParameterHome home in facts.Homes)
            {
                TheHome(report, home);
            }

            PlotNamesSideBySide(report, facts.OnSheets);
        }

        /// <summary>
        /// The same block three times, for the title block instance, the title block type and
        /// the sheet. Every name with its tally, then the two wanted names with examples or a
        /// NOT FOUND and the near misses.
        /// </summary>
        private static void TheHome(StringBuilder report, ParameterHome home)
        {
            List<ParameterTally> tallies = home.Tallies
                .OrderBy(one => one.Name, NaturalOrder.Comparer)
                .ToList();

            Line(report, "ON EVERY " + home.Where.ToUpperInvariant() + ", " + Count(tallies.Count, "parameter name")
                + " over " + Count(home.ElementCount, home.Where));
            Line(report, "name | carrying it | with a value");
            foreach (ParameterTally tally in tallies)
            {
                Line(report, Join(tally.Name, tally.Carrying.ToString(CultureInfo.InvariantCulture),
                    tally.WithValue.ToString(CultureInfo.InvariantCulture)));
            }
            Line(report, string.Empty);

            // The near misses belong to the home rather than to whichever wanted name went
            // missing, so they are worked out once. The list names every one of them, because
            // that line is a statement about the home. What is dropped from the values below
            // it is a wanted name the home really holds, which prints under its own heading a
            // few lines away: PRX_Plot_UID2 holds the word Plot, so PRX_COMPONENT's NOT FOUND
            // was printing all of PRX_Plot_UID2's values a second time.
            IReadOnlyList<string> near = home.NamesHolding(KpiNames.SheetNearMisses);
            List<string> toShow = near
                .Where(name => !KpiNames.OnSheets.Any(
                    wanted => string.Equals(wanted, name, StringComparison.Ordinal) && home.Holds(wanted)))
                .ToList();
            bool valuesShown = false;

            foreach (string wanted in KpiNames.OnSheets)
            {
                if (!home.Holds(wanted))
                {
                    Line(report, wanted + " on the " + home.Where + ": " + NotFound);
                    NearMisses(report, near,
                        "No name on the " + home.Where + " holds " + Words(KpiNames.SheetNearMisses) + ".",
                        "Names on the " + home.Where + " holding " + Words(KpiNames.SheetNearMisses) + ": ");

                    if (valuesShown)
                    {
                        // Said once. Repeating twenty rows per near miss under a second
                        // NOT FOUND on the same home says nothing the first block did not.
                        Line(report, "  Their values are shown above.");
                        Line(report, string.Empty);
                        continue;
                    }

                    // A near miss named and never shown is the answer withheld. PRX_COMPONENT
                    // does not exist on the first real model, the sheet carries PRX_Component,
                    // and the report named it while printing not one of its 1384 values.
                    foreach (string missed in toShow)
                    {
                        Values(report, home, missed, "  ");
                    }

                    // Only what was really printed can be pointed at. A home with no near
                    // miss to show has nothing above for a second NOT FOUND to refer to.
                    valuesShown = toShow.Count > 0;
                    Line(report, string.Empty);
                    continue;
                }

                Values(report, home, wanted, string.Empty);
                Line(report, string.Empty);
            }
        }

        /// <summary>
        /// Up to twenty values of one name, the ones with a value first so twenty examples show
        /// what a value looks like wherever any sheet has one rather than twenty blanks off the
        /// first sheets. The exact name and a near miss print through here alike, because a
        /// name worth naming is a name worth showing.
        /// </summary>
        private static void Values(StringBuilder report, ParameterHome home, string name, string indent)
        {
            List<SheetValue> values = home.ValuesOf(name)
                .OrderBy(one => string.IsNullOrWhiteSpace(one.Value) ? 1 : 0)
                .ThenBy(one => one.SheetNumber, NaturalOrder.Comparer)
                .ToList();

            if (values.Count == 0)
            {
                Line(report, indent + name + " on the " + home.Where + ": no value was read for it.");
                return;
            }

            Line(report, indent + name + " on the " + home.Where + ", showing "
                + Math.Min(ShownExamples, values.Count) + " of " + values.Count
                + ", the ones with a value first:");
            Line(report, indent + (string.Equals(home.Where, TitleBlockFacts.OnTypesWhere, StringComparison.Ordinal)
                ? "family : type | used on | value"
                : "sheet number | sheet name | value"));
            foreach (SheetValue value in values.Take(ShownExamples))
            {
                Line(report, indent + Join(value.SheetNumber, value.SheetName, Shown(value.Value)));
            }
        }

        /// <summary>
        /// The four plot parameters on one row per sheet.
        ///
        /// All four exist with values on the first real model and the report showed values for
        /// one of them, so the four could not be told apart from the file. The workbook asks
        /// for one Ref and this is the section that lets somebody pick which.
        /// </summary>
        private static void PlotNamesSideBySide(StringBuilder report, ParameterHome home)
        {
            var bySheet = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            var order = new List<string>();

            foreach (SheetValue value in home.Values)
            {
                if (!KpiNames.PlotNamesOnSheets.Contains(value.ParameterName, StringComparer.Ordinal)) continue;

                Dictionary<string, string> row;
                if (!bySheet.TryGetValue(value.SheetNumber, out row))
                {
                    row = new Dictionary<string, string>(StringComparer.Ordinal);
                    bySheet[value.SheetNumber] = row;
                    order.Add(value.SheetNumber);
                }

                row[value.ParameterName] = value.Value;
            }

            Line(report, "THE FOUR PLOT PARAMETERS ON THE " + home.Where.ToUpperInvariant()
                + ", SIDE BY SIDE, " + bySheet.Count + " sheets, showing "
                + Math.Min(ShownExamples, bySheet.Count));

            if (bySheet.Count == 0)
            {
                Line(report, "  None of " + string.Join(", ", KpiNames.PlotNamesOnSheets)
                    + " was read on the " + home.Where + ".");
                Line(report, string.Empty);
                return;
            }

            Line(report, "  sheet number | " + string.Join(" | ", KpiNames.PlotNamesOnSheets));

            // The sheets carrying the most of the four first, so a reader sees a full row
            // before an empty one and can tell the four apart on it.
            foreach (string sheetNumber in order
                .OrderByDescending(number => bySheet[number].Values.Count(one => !string.IsNullOrWhiteSpace(one)))
                .ThenBy(number => number, NaturalOrder.Comparer)
                .Take(ShownExamples))
            {
                Dictionary<string, string> row = bySheet[sheetNumber];
                var cells = new List<string> { sheetNumber };
                foreach (string name in KpiNames.PlotNamesOnSheets)
                {
                    string held;
                    cells.Add(row.TryGetValue(name, out held) ? Shown(held) : "(not on it)");
                }
                Line(report, "  " + Join(cells.ToArray()));
            }

            Line(report, string.Empty);
        }

        private static void NearMisses(StringBuilder report, IReadOnlyList<string> names, string none, string some)
        {
            if (names.Count == 0)
            {
                Line(report, "  " + none);
                return;
            }

            Line(report, "  " + some + string.Join(", ", names.ToArray()));
        }

        private static void TheLinks(StringBuilder report, KpiScan scan)
        {
            LinkFacts facts = scan.Links;

            Heading(report, LinkedModels, facts.Types.Count, "link types, " + Count(facts.Instances.Count, "placed instance"));

            if (!facts.WasRead)
            {
                Line(report, NotReadLine + " " + facts.WhyNotRead);
                Line(report, string.Empty);
                return;
            }

            Line(report, "link type | status | nested | name holds 00");
            foreach (ScannedLinkType type in facts.Types.OrderBy(one => one.Name, NaturalOrder.Comparer))
            {
                Line(report, Join(type.Name, type.Status, type.IsNested ? "nested" : "not nested",
                    type.HoldsTheMark ? "holds 00" : "-"));
            }
            Line(report, string.Empty);

            Line(report, "link instance | type | loaded | name holds 00");
            foreach (ScannedLinkInstance instance in facts.Instances.OrderBy(one => one.Name, NaturalOrder.Comparer))
            {
                Line(report, Join(instance.Name, instance.TypeName, instance.IsLoaded ? "loaded" : "not loaded",
                    instance.HoldsTheMark ? "holds 00" : "-"));
            }
            Line(report, string.Empty);

            List<string> marked = facts.NamesHoldingTheMark.ToList();
            Line(report, marked.Count == 0
                ? "No link name holds 00, so REVIT 00 LINK is not matched by name. Every link is listed above."
                : "Links whose name holds 00: " + string.Join(", ", marked.ToArray()) + ".");
            Line(report, string.Empty);

            if (facts.Contents.Count == 0)
            {
                Line(report, facts.Types.Count == 0 && facts.Instances.Count == 0
                    ? "No link in the model, so there is no filled region to read."
                    : KpiQuestions.NoDocumentToRead(facts, false));
                Line(report, string.Empty);
                return;
            }

            foreach (LinkContents link in facts.Contents)
            {
                TheLinkContents(report, link);
            }
        }

        private static void TheLinkContents(StringBuilder report, LinkContents link)
        {
            Line(report, "LOADED LINK " + link.LinkName + ", document " + link.DocumentTitle + ", "
                + Count(link.FilledRegionCount, "filled region"));

            Line(report, "  filled region type | regions");
            foreach (NameCount type in link.TypeCounts.OrderBy(one => one.Name, NaturalOrder.Comparer))
            {
                Line(report, "  " + Join(type.Name, Count(type.Count, "region")));
            }

            Line(report, "  view holding them | regions");
            foreach (NameCount view in link.ViewCounts.OrderBy(one => one.Name, NaturalOrder.Comparer))
            {
                Line(report, "  " + Join(view.Name, Count(view.Count, "region")));
            }
            Line(report, string.Empty);

            Line(report, "  First " + link.FirstRegions.Count + " of " + link.FilledRegionCount
                + " filled regions, every parameter:");
            int number = 0;
            foreach (FilledRegionRead region in link.FirstRegions)
            {
                number++;
                Line(report, "  #" + number + " " + region.TypeName + " in " + region.ViewName);
                Line(report, "    name | storage | printed | raw");
                foreach (ReadParameter one in region.Parameters.OrderBy(p => p.Name, NaturalOrder.Comparer))
                {
                    Line(report, "    " + Join(one.Name, one.StorageType, Value(one), one.Raw.Length == 0 ? "-" : one.Raw));
                }
            }
            Line(report, string.Empty);

            ParameterTally tally = link.InterventionTally;
            if (tally == null)
            {
                Line(report, "  " + KpiNames.InterventionArea + ": " + NotFound + " on any filled region in this link");
                NearMisses(report, link.InterventionNearMisses,
                    "No filled region parameter name holds " + Words(KpiNames.InterventionNearMisses) + ".",
                    "Filled region parameter names holding " + Words(KpiNames.InterventionNearMisses) + ": ");
                Line(report, string.Empty);
                return;
            }

            Line(report, "  " + KpiNames.InterventionArea + ": on " + tally.Carrying + " of "
                + Count(link.FilledRegionCount, "filled region") + ", " + tally.WithValue + " with a value, showing "
                + Math.Min(ShownExamples, link.InterventionAreas.Count) + " of " + link.InterventionAreas.Count + ":");
            Line(report, "  region type | plot | measures | raw | printed");
            Line(report, "  The raw number is square feet only where measures reads Area. A number typed by "
                + "hand measures nothing and prints with no unit.");
            foreach (MeasuredValue value in link.InterventionAreas.Take(ShownExamples))
            {
                Line(report, "  " + Join(value.Label, Shown(value.PlotId),
                    value.Spec.Length == 0 ? "-" : value.Spec, value.Raw, value.Printed));
            }
            Line(report, string.Empty);

            // Carried blank and not carried at all read the same way in a row, as an empty
            // plot, and only the first is a value somebody forgot to type.
            Line(report, "  " + KpiNames.RefPlotId + ": on "
                + (link.PlotWithValue + link.PlotCarriedBlank) + " of "
                + Count(link.FilledRegionCount, "filled region") + ", "
                + link.PlotWithValue + " with a value, "
                + link.PlotCarriedBlank + " carrying it blank, "
                + link.PlotNotCarried + " not carrying it at all.");
            Line(report, string.Empty);

            // One plot's regions read together is what settles which type is its intervention
            // area. A type whose regions all carry a plot is a candidate and one whose regions
            // carry none is not, so the table is driven by every type in the link. Driving it
            // by the types that carry one meant the answer none could not print at all.
            Line(report, "  REGIONS OF EACH TYPE CARRYING " + KpiNames.RefPlotId + ", "
                + Count(link.TypeCounts.Count, "type"));
            Line(report, "  filled region type | regions with a plot | regions of that type");
            foreach (NameCount all in link.TypeCounts.OrderBy(one => one.Name, NaturalOrder.Comparer))
            {
                NameCount carrying = link.TypesCarryingAPlot.FirstOrDefault(
                    one => string.Equals(one.Name, all.Name, StringComparison.Ordinal));
                Line(report, "  " + Join(all.Name,
                    (carrying == null ? 0 : carrying.Count).ToString(CultureInfo.InvariantCulture),
                    all.Count.ToString(CultureInfo.InvariantCulture)));
            }

            // Both lists come off one loop over one set of regions, so a name in one and not
            // the other is the tool contradicting itself rather than anything about the model.
            foreach (NameCount carrying in link.TypesCarryingAPlot
                .Where(one => !link.TypeCounts.Any(all => string.Equals(all.Name, one.Name, StringComparison.Ordinal)))
                .OrderBy(one => one.Name, NaturalOrder.Comparer))
            {
                Line(report, "  " + carrying.Name + " carries a plot on " + Count(carrying.Count, "region")
                    + " and is not a type in the link at all. That is a bug in this tool.");
            }
            Line(report, string.Empty);

            List<IGrouping<string, MeasuredValue>> perPlot = link.RegionsCarryingAPlot
                .Where(one => one.PlotId.Length > 0)
                .GroupBy(one => one.PlotId, StringComparer.Ordinal)
                .OrderBy(group => group.Key, NaturalOrder.Comparer)
                .ToList();

            Line(report, "  ONE PLOT'S REGIONS TOGETHER, first " + Math.Min(ShownPlots, perPlot.Count)
                + " of " + perPlot.Count + " plots");
            foreach (IGrouping<string, MeasuredValue> plot in perPlot.Take(ShownPlots))
            {
                Line(report, "  " + plot.Key + ", " + Count(plot.Count(), "region") + ":");
                foreach (MeasuredValue value in plot)
                {
                    Line(report, "    " + Join(value.Label, value.Raw, value.Printed));
                }
            }
            Line(report, string.Empty);
        }

        private static void TheSchedules(StringBuilder report, KpiScan scan)
        {
            ScheduleFacts facts = scan.Schedules;
            List<ScannedSchedule> all = facts.Schedules
                .OrderBy(one => one.Name, NaturalOrder.Comparer)
                .ToList();

            Heading(report, Schedules, all.Count, "schedules, " + Count(facts.TemplateCount, "schedule template") + " not listed");

            if (!facts.WasRead)
            {
                Line(report, NotReadLine + " " + facts.WhyNotRead);
                Line(report, string.Empty);
                return;
            }

            Line(report, "name | category | fields | filters | on a sheet | workbook words in the name");
            foreach (ScannedSchedule schedule in all)
            {
                Line(report, Join(schedule.Name, schedule.CategoryName, Count(schedule.Fields.Count, "field"),
                    schedule.Filters.Count == 0 ? "no filter" : schedule.FilteredOn,
                    schedule.OnASheet ? "on a sheet" : "not on a sheet",
                    schedule.IsMarked ? schedule.MarkedFor : "-"));
            }
            Line(report, string.Empty);

            List<IGrouping<string, ScannedSchedule>> distinct = all
                .GroupBy(one => one.NameWithoutThePlot, StringComparer.Ordinal)
                .OrderBy(group => group.Key, NaturalOrder.Comparer)
                .ToList();

            Line(report, "NAMES ONCE THE PLOT IS TAKEN OFF, " + distinct.Count);
            Line(report, "A name that does not follow PlotID-(code) name is shown whole.");
            Line(report, "name without the plot | schedules | workbook words");
            foreach (IGrouping<string, ScannedSchedule> group in distinct)
            {
                string words = KpiNames.WordsIn(group.Key, KpiNames.ScheduleWords);
                Line(report, Join(group.Key, Count(group.Count(), "schedule"), words.Length == 0 ? "-" : words));
            }
            Line(report, string.Empty);

            List<ScannedSchedule> readInFull = all.Where(one => one.ReadInFull).ToList();
            Line(report, "READ IN FULL, " + readInFull.Count + ", up to " + PlotsReadInFull
                + " plots per name the workbook draws from, the first in name order that list an element");
            foreach (ScannedSchedule schedule in readInFull)
            {
                Line(report, string.Empty);
                Line(report, schedule.Name + ", category " + Shown(schedule.CategoryName) + ", phase "
                    + Shown(schedule.PhaseName) + ", phase filter " + Shown(schedule.PhaseFilterName));
                Line(report, "  fields in order, " + schedule.Fields.Count + ": heading | parameter | field type | measures | unit | hidden");
                foreach (ScheduleFieldRead field in schedule.Fields)
                {
                    Line(report, "  " + Join(field.Heading, field.ParameterName, field.FieldType,
                        field.Spec.Length == 0 ? "-" : field.Spec,
                        field.UnitLabel.Length == 0 ? "project unit" : field.UnitLabel,
                        field.IsHidden ? "hidden" : "shown"));
                }
                Line(report, "  filters, " + schedule.Filters.Count + ": field | rule | value");
                foreach (ScheduleFilterRead filter in schedule.Filters)
                {
                    Line(report, "  " + Join(filter.FieldName, filter.Rule, Shown(filter.Value)));
                }
            }
            Line(report, string.Empty);
        }

        private static void TheSoftscapeFields(StringBuilder report, KpiScan scan)
        {
            List<ScannedSchedule> softscape = scan.Schedules.Softscape
                .OrderBy(one => one.Name, NaturalOrder.Comparer)
                .ToList();
            List<ScannedSchedule> read = softscape.Where(one => one.ReadInFull).ToList();

            Heading(report, SoftscapeFields, softscape.Count, "schedules named for SOFTSCAPE, " + read.Count + " read in full");

            if (!scan.Schedules.WasRead)
            {
                Line(report, NotReadLine + " " + scan.Schedules.WhyNotRead);
                Line(report, string.Empty);
                return;
            }

            if (softscape.Count == 0)
            {
                Line(report, "No schedule name holds SOFTSCAPE. Section 5 lists every name, so the real one can be picked.");
                Line(report, string.Empty);
                return;
            }

            foreach (ScannedSchedule schedule in read)
            {
                Line(report, schedule.Name);
                Line(report, "  fields in order: heading | parameter | field type | measures");
                foreach (ScheduleFieldRead field in schedule.Fields)
                {
                    Line(report, "  " + Join(field.Heading, field.ParameterName, field.FieldType,
                        field.Spec.Length == 0 ? "-" : field.Spec));
                }

                List<string> counts = schedule.Fields.Where(field => field.IsCount).Select(field => field.Heading).ToList();
                Line(report, counts.Count == 0
                    ? "  No Count field."
                    : "  Count fields: " + string.Join(", ", counts.ToArray()));

                TheRows(report, schedule);

                ScheduleElements elements = scan.Schedules.Elements
                    .FirstOrDefault(one => string.Equals(one.ScheduleName, schedule.Name, StringComparison.Ordinal));
                if (elements != null) TheElements(report, elements, KpiNames.PlantingWords);
                Line(report, string.Empty);
            }
        }

        private static void TheRows(StringBuilder report, ScannedSchedule schedule)
        {
            if (!schedule.RowsWereRead)
            {
                Line(report, "  rows as printed: not read. READS THAT DID NOT HAPPEN at the top says why. The "
                    + "elements it lists were read and follow.");
                return;
            }

            Line(report, "  rows as printed, showing " + Math.Min(ShownRows, schedule.Rows.Count) + " of "
                + schedule.BodyRowCount + ". The first row is usually the headings and the last usually the "
                + "total. When fewer are shown than there are, the last one shown is the schedule's last row.");
            Line(report, "  The rows are as the schedule last regenerated, which can be older than the elements "
                + "listed count below it when the model changed since and the schedule was not opened.");
            foreach (IReadOnlyList<string> row in schedule.Rows.Take(ShownRows))
            {
                Line(report, "  " + Join(row.Select(cell => cell == null || cell.Length == 0 ? "-" : cell).ToArray()));
            }
        }

        private static void TheElements(StringBuilder report, ScheduleElements elements, string[] words)
        {
            Line(report, "  elements listed: " + elements.ElementCount + ", categories: "
                + Counted(elements.Categories));
            Line(report, "  family : type, showing " + Math.Min(ShownFamilyTypes, elements.FamilyTypes.Count)
                + " of " + elements.FamilyTypes.Count);
            foreach (NameCount one in elements.FamilyTypes.OrderByDescending(one => one.Count).ThenBy(one => one.Name, NaturalOrder.Comparer).Take(ShownFamilyTypes))
            {
                Line(report, "  " + Join(one.Name, Count(one.Count, "element")));
            }

            Line(report, "  parameter names on the elements, " + elements.InstanceParameters.Count + " on instances and "
                + elements.TypeParameters.Count + " on types");
            Line(report, "  name | on | carrying it | with a value");
            foreach (ParameterTally tally in elements.InstanceParameters.OrderBy(one => one.Name, NaturalOrder.Comparer))
            {
                Line(report, "  " + Join(tally.Name, "instance", tally.Carrying.ToString(CultureInfo.InvariantCulture),
                    tally.WithValue.ToString(CultureInfo.InvariantCulture)));
            }
            foreach (ParameterTally tally in elements.TypeParameters.OrderBy(one => one.Name, NaturalOrder.Comparer))
            {
                Line(report, "  " + Join(tally.Name, "type", tally.Carrying.ToString(CultureInfo.InvariantCulture),
                    tally.WithValue.ToString(CultureInfo.InvariantCulture)));
            }

            TheWordValues(report, elements, words);
        }

        private static void TheWordValues(StringBuilder report, ScheduleElements elements, string[] words)
        {
            List<string> names = elements.ParameterNamesHolding(words).ToList();
            Line(report, names.Count == 0
                ? "  No parameter name holds " + Words(words) + "."
                : "  Values of every parameter whose name holds " + Words(words) + ":");

            foreach (string name in names)
            {
                // Instances and types apart. One name bound to both put the same tree under
                // two values and counted 114 over 57 listed.
                foreach (bool onType in new[] { false, true })
                {
                    List<ParameterValueCount> values = elements.ValuesOf(name, onType)
                        .OrderByDescending(one => one.Count)
                        .ThenBy(one => one.Value, NaturalOrder.Comparer)
                        .ToList();
                    if (values.Count == 0) continue;

                    Line(report, "  " + name + " on " + (onType ? "types" : "instances") + ", "
                        + Count(values.Count, "distinct value")
                        + (values.Count > ShownValues ? ", showing " + ShownValues : string.Empty)
                        + ", counts add to " + values.Sum(one => one.Count) + " over "
                        + Count(elements.ElementCount, "element") + " listed:");
                    foreach (ParameterValueCount value in values.Take(ShownValues))
                    {
                        Line(report, "    " + Join(Shown(value.Value), Count(value.Count, "element")));
                    }
                }
            }
        }

        private static void TheExistingAndProposed(StringBuilder report, KpiScan scan)
        {
            ScheduleFacts facts = scan.Schedules;

            Heading(report, ExistingAndProposed, facts.Phases.Count, "phases in the model, in order");

            if (!facts.WasRead)
            {
                Line(report, NotReadLine + " " + facts.WhyNotRead);
                Line(report, string.Empty);
                return;
            }

            int number = 0;
            foreach (string phase in facts.Phases)
            {
                number++;
                Line(report, "  " + number + ". " + phase);
            }
            Line(report, string.Empty);

            foreach (ScannedSchedule schedule in facts.Softscape.Where(one => one.ReadInFull).OrderBy(one => one.Name, NaturalOrder.Comparer))
            {
                Line(report, schedule.Name + ": phase " + Shown(schedule.PhaseName) + ", phase filter "
                    + Shown(schedule.PhaseFilterName) + ", filters "
                    + (schedule.Filters.Count == 0 ? "none" : schedule.FilteredOn));

                ScheduleElements elements = facts.Elements
                    .FirstOrDefault(one => string.Equals(one.ScheduleName, schedule.Name, StringComparison.Ordinal));
                if (elements == null)
                {
                    Line(report, "  its elements were not read");
                    Line(report, string.Empty);
                    continue;
                }

                Line(report, "  elements by phase created: " + Counted(elements.CreatedPhases));
                Line(report, "  elements by phase demolished: " + Counted(elements.DemolishedPhases));
                Line(report, "  elements by workset: " + Counted(elements.Worksets));
                Line(report, "  elements by design option: " + Counted(elements.DesignOptions));
                TheWordValues(report, elements, KpiNames.StatusWords);
                Line(report, string.Empty);
            }

            // Matched on the heading alone, then labelled with the schedule, so nothing in a
            // schedule name can put a heading on this list.
            List<string> headings = facts.Schedules
                .SelectMany(schedule => schedule.Fields
                    .Where(field => KpiNames.HoldsAny(field.Heading, "EXISTING", "PROPOSED"))
                    .Select(field => schedule.NameWithoutThePlot + ": " + field.Heading))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(line => line, NaturalOrder.Comparer)
                .ToList();

            Line(report, "COLUMN HEADINGS HOLDING EXISTING OR PROPOSED, " + headings.Count + ", plot taken off");
            foreach (string heading in headings)
            {
                Line(report, "  " + heading);
            }
            Line(report, string.Empty);
        }

        private static void TheAreas(StringBuilder report, KpiScan scan)
        {
            ScheduleFacts facts = scan.Schedules;

            Heading(report, AreasAndUnits, facts.Areas.Count, "areas measured off elements the marked schedules list");

            if (!facts.WasRead)
            {
                Line(report, NotReadLine + " " + facts.WhyNotRead);
                Line(report, string.Empty);
                return;
            }

            Line(report, "Project area unit: " + Unit(scan.Document.Area));
            Line(report, "schedule | parameter | element | raw, square feet | square metres worked out from the raw | as printed");
            foreach (MeasuredArea area in facts.Areas)
            {
                Line(report, Join(area.ScheduleName, area.ParameterName, area.ElementLabel,
                    Number(area.RawSquareFeet), Number(area.SquareMetres), Shown(area.Printed)));
            }
            Line(report, string.Empty);

            List<ScannedSchedule> read = facts.Marked
                .Where(one => one.ReadInFull && !one.IsSoftscape)
                .OrderBy(one => one.Name, NaturalOrder.Comparer)
                .ToList();

            Line(report, "ROWS AS PRINTED, " + read.Count + " schedules, the totals as a sheet shows them");
            foreach (ScannedSchedule schedule in read)
            {
                Line(report, string.Empty);
                Line(report, schedule.Name);
                TheRows(report, schedule);
            }
            Line(report, string.Empty);
        }

        private static void TheQuestions(StringBuilder report, KpiScan scan)
        {
            IReadOnlyList<KpiAnswer> answers = KpiQuestions.Answers(scan);
            int answered = answers.Count(answer => answer.Answered);

            Heading(report, TheNineQuestions, answered, "of " + KpiQuestions.HowMany
                + " have something in this file. The rest say " + NotFound + " and where the near misses are.");
            foreach (KpiAnswer answer in answers)
            {
                Line(report, string.Empty);
                Line(report, answer.Number + ". " + answer.Question + "?");
                Line(report, "   " + (answer.Answered ? "FOUND. " : NotFound + ". ") + answer.Answer);
            }
        }

        private static string Counted(IEnumerable<NameCount> counts)
        {
            List<NameCount> all = counts.OrderByDescending(one => one.Count).ThenBy(one => one.Name, NaturalOrder.Comparer).ToList();
            if (all.Count == 0) return "none read";

            return string.Join(", ", all.Select(one => Shown(one.Name) + " " + one.Count).ToArray());
        }

        private static string Value(ReadParameter one)
        {
            if (!one.HasValue) return "(no value)";
            return Shown(one.Printed);
        }

        private static string Shown(string value)
        {
            if (string.IsNullOrEmpty(value)) return "(empty)";
            return string.IsNullOrWhiteSpace(value) ? "(whitespace only)" : value;
        }

        private static string Words(string[] words)
        {
            if (words.Length == 0) return string.Empty;
            if (words.Length == 1) return words[0];

            return string.Join(", ", words.Take(words.Length - 1).ToArray()) + " or " + words[words.Length - 1];
        }

        private static string Number(double value)
        {
            return value.ToString("0.####", CultureInfo.InvariantCulture);
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
