using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The nine questions this round exists to answer, each with one line saying what the
    /// scan found for it. This is the last section of the report and the one a reader goes to
    /// first, so every line names the section that holds the detail.
    ///
    /// Answered is decided by whether the thing was found, never by whether a value looks
    /// right. Nothing here picks a parameter or a field. It says what is there.
    /// </summary>
    public static class KpiQuestions
    {
        public const int HowMany = 9;

        public static readonly string[] Questions =
        {
            "Which sheet holds the title block carrying PRX_COMPONENT and PRX_Plot_UID2",
            "Is PRX_COMPONENT on the title block instance or on the sheet itself",
            "What is PRX_Plot_UID2",
            "What is the real name of the neighbourhood parameter in Project Information",
            "Is there a linked model whose name holds 00, and what does ID FILLED REGION mean in it",
            "What are the exact schedule names in the model",
            "Which field in the softscape schedule is the botanical name and which is the quantity",
            "What separates existing trees from proposed ones",
            "What unit does each area come back in, raw and as printed"
        };

        public static IReadOnlyList<KpiAnswer> Answers(KpiScan scan)
        {
            if (scan == null) throw new ArgumentNullException("scan");

            return new List<KpiAnswer>
            {
                WhichSheet(scan),
                InstanceOrSheet(scan),
                WhatIsPlotUid2(scan),
                Neighbourhood(scan),
                TheLink(scan),
                ScheduleNames(scan),
                SoftscapeFields(scan),
                ExistingAndProposed(scan),
                Units(scan)
            };
        }

        private static KpiAnswer WhichSheet(KpiScan scan)
        {
            if (!scan.TitleBlocks.WasRead) return NotRead(1, KpiReport.TitleBlocksAndSheets);

            ParameterHome instances = scan.TitleBlocks.OnInstances;
            var parts = new List<string>();
            bool both = true;

            foreach (string name in KpiNames.OnSheets)
            {
                ParameterTally tally = instances.TallyFor(name);
                if (tally == null)
                {
                    both = false;
                    parts.Add(name + " NOT FOUND on any title block instance");
                    continue;
                }

                parts.Add(name + " on " + Count(tally.Carrying, "title block instance")
                    + ", " + tally.WithValue + " with a value");
            }

            string said = string.Join(". ", parts.ToArray()) + ". ";

            if (!both)
            {
                return new KpiAnswer(1, Questions[0], said + "Section 3 lists the near misses.", false);
            }

            // The question asks for the sheet holding a title block that carries BOTH names.
            // Two families each carrying one would satisfy the two tallies and answer nothing,
            // so the two lists of sheets are intersected and the first is named.
            HashSet<string> withUid2 = new HashSet<string>(
                instances.ValuesOf(KpiNames.PlotUid2).Select(one => one.SheetNumber), StringComparer.Ordinal);

            List<SheetValue> together = instances.ValuesOf(KpiNames.Component)
                .Where(one => one.SheetNumber != NoSheet && withUid2.Contains(one.SheetNumber))
                .OrderBy(one => HasSomething(one.Value) ? 0 : 1)
                .ThenBy(one => one.SheetNumber, NaturalOrder.Comparer)
                .ToList();

            if (together.Count == 0)
            {
                return new KpiAnswer(1, Questions[0],
                    said + "No title block instance carries both names on one sheet. Section 3 lists each on its own.",
                    false);
            }

            SheetValue first = together[0];
            return new KpiAnswer(1, Questions[0],
                said + "Both on " + Count(together.Count, "sheet") + ", the first " + first.SheetNumber
                + (first.SheetName.Length == 0 ? string.Empty : " " + first.SheetName)
                + ". Section 3 lists up to twenty sheets for each.",
                true);
        }

        /// <summary>
        /// The sheet number the readers put on a title block found on no sheet.
        /// </summary>
        public const string NoSheet = "(no sheet)";

        private static KpiAnswer NotRead(int number, string section)
        {
            return new KpiAnswer(number, Questions[number - 1],
                "NOT READ. Section " + section + " was not read, see READS THAT DID NOT HAPPEN at the top.",
                false);
        }

        private static bool HasSomething(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private static KpiAnswer InstanceOrSheet(KpiScan scan)
        {
            if (!scan.TitleBlocks.WasRead) return NotRead(2, KpiReport.TitleBlocksAndSheets);

            var parts = new List<string>();
            bool anywhere = false;

            foreach (ParameterHome home in scan.TitleBlocks.Homes)
            {
                ParameterTally tally = home.TallyFor(KpiNames.Component);
                if (tally == null)
                {
                    parts.Add("not on the " + home.Where);
                    continue;
                }

                anywhere = true;
                parts.Add("on the " + home.Where + ", " + tally.Carrying + " of "
                    + Count(home.ElementCount, home.Where) + " carry it and " + tally.WithValue
                    + " hold a value");
            }

            string said = KpiNames.Component + " is " + string.Join(", ", parts.ToArray()) + ".";
            return new KpiAnswer(2, Questions[1], said, anywhere);
        }

        private static KpiAnswer WhatIsPlotUid2(KpiScan scan)
        {
            if (!scan.TitleBlocks.WasRead) return NotRead(3, KpiReport.TitleBlocksAndSheets);

            var parts = new List<string>();
            var samples = new List<string>();
            int withValue = 0;
            int plotShaped = 0;

            foreach (ParameterHome home in scan.TitleBlocks.Homes)
            {
                ParameterTally tally = home.TallyFor(KpiNames.PlotUid2);
                if (tally == null) continue;

                parts.Add("on the " + home.Where + " for " + tally.Carrying + " of "
                    + Count(home.ElementCount, home.Where));

                foreach (SheetValue value in home.ValuesOf(KpiNames.PlotUid2))
                {
                    if (!HasSomething(value.Value)) continue;

                    withValue++;
                    if (PlotId.IsPlotId(value.Value)) plotShaped++;
                    if (samples.Count < 5 && !samples.Contains(value.Value)) samples.Add(value.Value);
                }
            }

            if (parts.Count == 0)
            {
                return new KpiAnswer(3, Questions[2],
                    KpiNames.PlotUid2 + " NOT FOUND on a title block instance, a title block type or a "
                    + "sheet. It is neither PRX_Plot_ID nor PRX_Ref Plot ID and the model does not "
                    + "hold it under this name. Section 3 lists the near misses.",
                    false);
            }

            string said = KpiNames.PlotUid2 + " is " + string.Join(", ", parts.ToArray()) + ". "
                + withValue + " values, " + plotShaped + " of them shaped like a plot identifier such as DM-41.";
            if (samples.Count > 0) said += " First values: " + string.Join(", ", samples.ToArray()) + ".";

            return new KpiAnswer(3, Questions[2], said, withValue > 0);
        }

        private static KpiAnswer Neighbourhood(KpiScan scan)
        {
            if (!scan.ProjectInformationRead) return NotRead(4, KpiReport.ProjectInformation);

            List<ReadParameter> near = scan.ProjectInformation
                .Where(one => KpiNames.HoldsAny(one.Name, KpiNames.NeighbourhoodNearMisses))
                .OrderBy(one => one.Name, NaturalOrder.Comparer)
                .ToList();

            if (near.Count == 0)
            {
                return new KpiAnswer(4, Questions[3],
                    "No Project Information parameter name holds " + Words(KpiNames.NeighbourhoodNearMisses)
                    + ". All " + Count(scan.ProjectInformation.Count, "name") + " are in section 2 for the team to pick from.",
                    false);
            }

            string said = string.Join("; ", near
                .Select(one => one.Name + " (" + one.Kind + ") holds " + Shown(one.Printed))
                .ToArray());

            return new KpiAnswer(4, Questions[3],
                Count(near.Count, "name") + " in Project Information hold" + (near.Count == 1 ? "s " : " ")
                + Words(KpiNames.NeighbourhoodNearMisses) + ": " + said + ".",
                true);
        }

        private static KpiAnswer TheLink(KpiScan scan)
        {
            if (!scan.Links.WasRead) return NotRead(5, KpiReport.LinkedModels);

            List<string> marked = scan.Links.NamesHoldingTheMark.ToList();

            if (marked.Count == 0)
            {
                return new KpiAnswer(5, Questions[4],
                    "No link name holds 00 among " + Count(scan.Links.Types.Count, "link type") + " and "
                    + Count(scan.Links.Instances.Count, "instance") + ", all named in section 4.",
                    false);
            }

            List<LinkContents> contents = scan.Links.Contents
                .Where(link => KpiNames.Holds(link.LinkName, KpiNames.LinkMark)
                    || KpiNames.Holds(link.DocumentTitle, KpiNames.LinkMark))
                .ToList();

            string said = Count(marked.Count, "link name") + " hold" + (marked.Count == 1 ? "s" : string.Empty)
                + " 00: " + string.Join(", ", marked.ToArray()) + ". ";

            if (contents.Count == 0)
            {
                return new KpiAnswer(5, Questions[4], said + NoDocumentToRead(scan.Links, true), false);
            }

            var found = new List<string>();
            bool regions = false;
            bool parameter = false;

            foreach (LinkContents link in contents)
            {
                regions |= link.FilledRegionCount > 0;
                parameter |= link.HoldsInterventionArea;

                List<string> idTypes = link.TypeCounts.Select(one => one.Name)
                    .Where(name => KpiNames.Holds(name, KpiNames.RegionMark)).ToList();
                List<string> idViews = link.ViewCounts.Select(one => one.Name)
                    .Where(name => KpiNames.Holds(name, KpiNames.RegionMark)).ToList();

                string line = link.LinkName + " holds " + Count(link.FilledRegionCount, "filled region");
                line += idTypes.Count > 0
                    ? ", region types holding ID: " + string.Join(", ", idTypes.ToArray())
                    : ", no region type name holds ID";
                line += idViews.Count > 0
                    ? ", views holding ID: " + string.Join(", ", idViews.ToArray())
                    : ", no view name holds ID";

                ParameterTally tally = link.InterventionTally;
                line += tally == null
                    ? ", " + KpiNames.InterventionArea + " NOT FOUND"
                    : ", " + KpiNames.InterventionArea + " on " + tally.Carrying + " with " + tally.WithValue + " holding a value";

                found.Add(line);
            }

            return new KpiAnswer(5, Questions[4],
                said + string.Join(". ", found.ToArray()) + ". Section 4 has the detail.",
                regions && parameter);
        }

        /// <summary>
        /// Why no filled region was read, built from the reads rather than asserted. A link
        /// type can read Loaded while no placed instance hands back a document, and telling
        /// somebody to load a link that is loaded sends them the wrong way.
        /// </summary>
        public static string NoDocumentToRead(LinkFacts links, bool markedOnly)
        {
            IEnumerable<ScannedLinkType> types = markedOnly ? links.Types.Where(type => type.HoldsTheMark) : links.Types;
            IEnumerable<ScannedLinkInstance> instances = markedOnly ? links.Instances.Where(one => one.HoldsTheMark) : links.Instances;

            int loadedTypes = types.Count(type => type.IsLoaded);
            int placed = instances.Count();
            int loadedInstances = instances.Count(one => one.IsLoaded);

            if (loadedTypes == 0)
            {
                return "None is loaded, so no filled region was read. Load the link and scan again.";
            }

            return Count(loadedTypes, "link type") + " read" + (loadedTypes == 1 ? "s" : string.Empty)
                + " Loaded and " + loadedInstances + " of " + Count(placed, "placed instance")
                + " handed back a document, so no filled region was read. Place an instance and scan again.";
        }

        private static KpiAnswer ScheduleNames(KpiScan scan)
        {
            if (!scan.Schedules.WasRead) return NotRead(6, KpiReport.Schedules);

            List<ScannedSchedule> schedules = scan.Schedules.Schedules.ToList();
            int distinct = schedules.Select(one => one.NameWithoutThePlot).Distinct(StringComparer.Ordinal).Count();

            if (schedules.Count == 0)
            {
                return new KpiAnswer(6, Questions[5], "No schedule in the model.", false);
            }

            var parts = new List<string>();
            foreach (string word in KpiNames.ScheduleWords)
            {
                List<string> names = schedules
                    .Where(one => KpiNames.Holds(one.Name, word))
                    .Select(one => one.NameWithoutThePlot)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(name => name, NaturalOrder.Comparer)
                    .ToList();

                parts.Add(word + ": " + (names.Count == 0 ? "none" : string.Join(", ", names.ToArray())));
            }

            bool any = scan.Schedules.Marked.Any();
            return new KpiAnswer(6, Questions[5],
                Count(schedules.Count, "schedule") + ", " + distinct + " distinct once the plot is taken off, "
                + "all in section 5. Names holding each workbook word, plot taken off. "
                + string.Join(". ", parts.ToArray()) + ".",
                any);
        }

        private static KpiAnswer SoftscapeFields(KpiScan scan)
        {
            if (!scan.Schedules.WasRead) return NotRead(7, KpiReport.Schedules);

            List<ScannedSchedule> read = scan.Schedules.Softscape
                .Where(one => one.ReadInFull)
                .OrderBy(one => one.Name, NaturalOrder.Comparer)
                .ToList();

            if (read.Count == 0)
            {
                bool named = scan.Schedules.Softscape.Any();
                return new KpiAnswer(7, Questions[6],
                    named
                        ? "A schedule named for SOFTSCAPE exists and none was read in full. Section 5 has the names."
                        : "No schedule name holds SOFTSCAPE, so no field could be read. Section 5 lists every name.",
                    false);
            }

            // Every softscape schedule read in full, one part each, because the reader reads
            // several plots per name and two names holding SOFTSCAPE both arrive here.
            var parts = new List<string>();
            bool anything = false;

            foreach (ScannedSchedule schedule in read)
            {
                ScheduleElements elements = scan.Schedules.Elements
                    .FirstOrDefault(one => string.Equals(one.ScheduleName, schedule.Name, StringComparison.Ordinal));

                List<string> counts = schedule.Fields.Where(field => field.IsCount).Select(field => field.Heading).ToList();
                List<string> nameLike = elements == null
                    ? new List<string>()
                    : elements.ParameterNamesHolding("BOTANIC", "LATIN", "SPECIES").ToList();

                anything |= counts.Count > 0 || nameLike.Count > 0;

                string said = schedule.Name + " has " + Count(schedule.Fields.Count, "field") + ", headed "
                    + string.Join(" | ", schedule.Fields.Select(field => field.Heading).ToArray()) + ". ";
                said += counts.Count > 0
                    ? "Count fields: " + string.Join(", ", counts.ToArray()) + ". "
                    : "No Count field. ";
                said += nameLike.Count > 0
                    ? "Parameters on its elements holding BOTANIC, LATIN or SPECIES: " + string.Join(", ", nameLike.ToArray())
                    : "No parameter on its elements holds BOTANIC, LATIN or SPECIES";
                parts.Add(said);
            }

            return new KpiAnswer(7, Questions[6],
                string.Join(". ", parts.ToArray()) + ". Section 6 has the rows as printed and every value.",
                anything);
        }

        private static KpiAnswer ExistingAndProposed(KpiScan scan)
        {
            if (!scan.Schedules.WasRead) return NotRead(8, KpiReport.Schedules);

            var parts = new List<string>();
            parts.Add(scan.Schedules.Phases.Count == 0
                ? "No phase read"
                : Count(scan.Schedules.Phases.Count, "phase") + ": " + string.Join(", ", scan.Schedules.Phases.ToArray()));

            bool anything = false;

            foreach (ScannedSchedule schedule in scan.Schedules.Softscape.Where(one => one.ReadInFull))
            {
                parts.Add(schedule.Name + " is on phase " + Shown(schedule.PhaseName)
                    + " with phase filter " + Shown(schedule.PhaseFilterName));

                ScheduleElements elements = scan.Schedules.Elements
                    .FirstOrDefault(one => string.Equals(one.ScheduleName, schedule.Name, StringComparison.Ordinal));
                if (elements == null) continue;

                if (elements.CreatedPhases.Count > 0)
                {
                    anything |= elements.CreatedPhases.Count > 1;
                    parts.Add("its elements by phase created: " + string.Join(", ",
                        elements.CreatedPhases.Select(one => one.Name + " " + one.Count).ToArray()));
                }

                List<string> statusNames = elements.ParameterNamesHolding(KpiNames.StatusWords).ToList();
                if (statusNames.Count > 0)
                {
                    // Phase Created and Phase Demolished are on every phased element, so they
                    // are printed and never count as the thing that separates the trees.
                    anything |= statusNames.Any(name => !KpiNames.BuiltInPhaseNames.Contains(name, StringComparer.Ordinal));
                    parts.Add("parameters on them holding " + Words(KpiNames.StatusWords) + ": "
                        + string.Join(", ", statusNames.ToArray()));
                }
                else
                {
                    parts.Add("no parameter on them holds " + Words(KpiNames.StatusWords));
                }
            }

            List<string> headings = scan.Schedules.Schedules
                .SelectMany(schedule => schedule.Fields.Select(field => field.Heading))
                .Where(heading => KpiNames.HoldsAny(heading, "EXISTING", "PROPOSED"))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(heading => heading, NaturalOrder.Comparer)
                .ToList();

            if (headings.Count > 0)
            {
                anything = true;
                parts.Add("column headings holding EXISTING or PROPOSED: " + string.Join(", ", headings.ToArray()));
            }

            return new KpiAnswer(8, Questions[7],
                string.Join(". ", parts.ToArray()) + ". Section 7 has the counts.",
                anything);
        }

        private static KpiAnswer Units(KpiScan scan)
        {
            if (!scan.Schedules.WasRead) return NotRead(9, KpiReport.Schedules);

            ProjectUnit area = scan.Document.Area;
            int measured = scan.Schedules.Areas.Count;

            string said = "Raw areas come back in square feet, which is what Revit holds whatever the project "
                + "shows. Printed areas come back in the project unit, " + area.Label
                + (double.IsNaN(area.Accuracy) ? string.Empty : ", rounded to " + KpiReport.Step(area.Accuracy))
                + ". " + Count(measured, "area") + (measured == 1 ? " was" : " were") + " measured both ways";

            said += measured > 0 ? " in section 8." : ", so section 8 has nothing to show the difference on.";

            return new KpiAnswer(9, Questions[8], said, area.IsKnown && measured > 0);
        }

        private static string Words(string[] words)
        {
            if (words.Length == 0) return string.Empty;
            if (words.Length == 1) return words[0];

            return string.Join(", ", words.Take(words.Length - 1).ToArray()) + " or " + words[words.Length - 1];
        }

        private static string Shown(string value)
        {
            return string.IsNullOrEmpty(value) ? "(empty)" : value;
        }

        private static string Count(int howMany, string thing)
        {
            return howMany.ToString(CultureInfo.InvariantCulture)
                + " " + thing + (howMany == 1 ? string.Empty : "s");
        }
    }
}
