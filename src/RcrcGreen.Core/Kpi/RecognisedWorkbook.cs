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
        private RecognisedWorkbook(string fileName, KpiTemplate template, IReadOnlyList<KpiTemplate> candidates, string reason)
        {
            FileName = fileName;
            Template = template;
            Candidates = candidates ?? new List<KpiTemplate>();
            Reason = reason ?? string.Empty;
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
        /// The recognition rule. The main sheet name settles five of the seven outright.
        /// Park Name is two templates, so the file name breaks the tie, and a file name
        /// holding both park words or neither leaves the user to pick.
        /// </summary>
        public static RecognisedWorkbook Recognise(string fileName, IReadOnlyList<string> sheetNames, string readRefusal)
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
                return Matched(fileName, byName[0]);
            }

            bool existing = KpiNames.Holds(fileName, "EXISTING");
            bool future = KpiNames.Holds(fileName, "FUTURE");

            if (existing && !future) return Matched(fileName, KpiTemplates.ExistingParks);
            if (future && !existing) return Matched(fileName, KpiTemplates.FutureParks);

            return Between(fileName, KpiTemplates.ExistingParks, KpiTemplates.FutureParks);
        }
    }
}
