using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// What one .xlsx in the templates folder turned out to be. Matched to one template,
    /// caught between the two park templates, or not a template at all with the reason on
    /// screen. Nothing is ever filled on a best guess.
    /// </summary>
    public sealed class RecognisedWorkbook
    {
        private RecognisedWorkbook(string fileName, KpiTemplate template, IReadOnlyList<KpiTemplate> candidates, string reason, FilledCell decidedBy = null, KpiTemplate filledAs = null)
        {
            FileName = fileName;
            Template = template;
            Candidates = candidates ?? new List<KpiTemplate>();
            Reason = reason ?? string.Empty;
            DecidedBy = decidedBy;
            FilledAs = filledAs;
        }

        /// <summary>
        /// The one cell that said this workbook was filled, and what it held. Null for anything
        /// else. It goes in the report so a workbook the tool withheld can be traced in one
        /// line rather than by opening it.
        /// </summary>
        public FilledCell DecidedBy { get; }

        /// <summary>
        /// The template a filled checklist was made from, when this file is one and its
        /// template can be told. Null for a filled park checklist whose file name names
        /// neither park or both, because naming one would be a guess. It is in the same list
        /// as the templates, named as filled with the reason, and never offered.
        /// </summary>
        public KpiTemplate FilledAs { get; }

        public bool IsFilled
        {
            get { return DecidedBy != null; }
        }

        public string FileName { get; }

        /// <summary>
        /// The one template this workbook is, or null when it matched none or two.
        /// </summary>
        public KpiTemplate Template { get; }

        /// <summary>
        /// The two park templates when the sheet name and the file name together could not
        /// tell them apart. The user picks and nothing is guessed.
        /// </summary>
        public IReadOnlyList<KpiTemplate> Candidates { get; }

        public string Reason { get; }

        public bool IsMatched
        {
            get { return Template != null; }
        }

        public bool NeedsAPick
        {
            get { return Template == null && Candidates.Count > 0; }
        }

        /// <summary>
        /// The line beside the file name in the list.
        /// </summary>
        public string InWords
        {
            get
            {
                if (IsMatched) return Template.Name;
                if (NeedsAPick) return "EXISTING PARKS or FUTURE PARKS. Pick one, nothing is guessed.";
                if (IsFilled) return "filled, not offered. " + Reason;
                return "not offered. " + Reason;
            }
        }

        public static RecognisedWorkbook Matched(string fileName, KpiTemplate template)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");
            if (template == null) throw new ArgumentNullException("template");

            return new RecognisedWorkbook(fileName, template, null, null);
        }

        public static RecognisedWorkbook Between(string fileName, KpiTemplate first, KpiTemplate second)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");

            return new RecognisedWorkbook(fileName, null, new[] { first, second }, null);
        }

        public static RecognisedWorkbook NotATemplate(string fileName, string reason)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");
            if (reason == null) throw new ArgumentNullException("reason");

            return new RecognisedWorkbook(fileName, null, null, reason);
        }

        /// <summary>
        /// A checklist the tool filled, told by a cell the tool writes holding something the
        /// tool would have written. Picking one would copy last time's typing and last time's
        /// written species as the template, so it stays in the list, greyed, saying why and
        /// which cell said so. Nothing is deleted or moved.
        /// </summary>
        public static RecognisedWorkbook Filled(string fileName, KpiTemplate template, FilledCell decidedBy)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");
            if (template == null) throw new ArgumentNullException("template");
            if (decidedBy == null) throw new ArgumentNullException("decidedBy");

            return new RecognisedWorkbook(fileName, null, null,
                "It is a filled " + template.Name + " checklist, not a template. " + decidedBy.InWords,
                decidedBy, template);
        }

        /// <summary>
        /// A filled park checklist whose file name names neither park or both. It is filled
        /// and not offered either way, and which park it was for is not guessed: the reason
        /// names both and says the file name cannot tell them apart.
        /// </summary>
        public static RecognisedWorkbook FilledPark(string fileName, FilledCell decidedBy)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");
            if (decidedBy == null) throw new ArgumentNullException("decidedBy");

            return new RecognisedWorkbook(fileName, null, null,
                "It is a filled " + KpiTemplates.ExistingParks.Name + " or " + KpiTemplates.FutureParks.Name
                + " checklist, which its file name cannot tell apart, not a template. " + decidedBy.InWords,
                decidedBy);
        }

        /// <summary>
        /// The recognition rule. The main sheet name settles five of the seven outright.
        /// Park Name is two templates, so the file name breaks the tie, and a file name
        /// holding both park words or neither leaves the user to pick. Before any of those
        /// is offered, <see cref="FilledMarks"/> says whether a cell the tool writes holds
        /// something the tool would have written, because a filled MOSQUES output keeps the
        /// sheet name that recognises it.
        /// </summary>
        public static RecognisedWorkbook Recognise(string fileName, IReadOnlyList<string> sheetNames, string readRefusal, IReadOnlyDictionary<string, string> cells = null)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");

            if (!string.IsNullOrEmpty(readRefusal))
            {
                return NotATemplate(fileName, readRefusal);
            }

            if (sheetNames == null || sheetNames.Count == 0)
            {
                return NotATemplate(fileName, "No sheet could be read from it.");
            }

            string mainSheet = sheetNames[0];
            List<KpiTemplate> byName = KpiTemplates.All
                .Where(template => string.Equals(template.MainSheetName, mainSheet, StringComparison.Ordinal))
                .ToList();

            if (byName.Count == 0)
            {
                return NotATemplate(fileName,
                    "Its first sheet is named " + mainSheet + ", which no template uses.");
            }

            if (byName.Count == 1)
            {
                FilledCell decided = FilledMarks.Decide(byName[0], cells);
                return decided == null ? Matched(fileName, byName[0]) : Filled(fileName, byName[0], decided);
            }

            bool existing = KpiNames.Holds(fileName, "EXISTING");
            bool future = KpiNames.Holds(fileName, "FUTURE");

            // Both parks map every cell to the same place, so either answers for both.
            FilledCell park = FilledMarks.Decide(KpiTemplates.ExistingParks, cells);
            if (park != null)
            {
                // A filled park checklist is not offered either way, and its file name says
                // which park it was for when it can. When it cannot, neither is named.
                if (existing && !future) return Filled(fileName, KpiTemplates.ExistingParks, park);
                if (future && !existing) return Filled(fileName, KpiTemplates.FutureParks, park);
                return FilledPark(fileName, park);
            }

            if (existing && !future) return Matched(fileName, KpiTemplates.ExistingParks);
            if (future && !existing) return Matched(fileName, KpiTemplates.FutureParks);

            return Between(fileName, KpiTemplates.ExistingParks, KpiTemplates.FutureParks);
        }
    }
}
