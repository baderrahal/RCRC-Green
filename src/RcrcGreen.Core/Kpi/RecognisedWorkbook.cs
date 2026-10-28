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
        private RecognisedWorkbook(string fileName, KpiTemplate template, IReadOnlyList<KpiTemplate> candidates, string reason, FilledCell decidedBy = null, KpiTemplate filledAs = null,
            TemplateLayout layout = null, KpiTemplate proposed = null, bool unknownLayout = false)
        {
            Layout = layout ?? TemplateLayout.Nothing;
            Proposed = proposed;
            UnknownLayout = unknownLayout;
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

        /// <summary>
        /// The file's sheet names and first sheet labels, read in the peek, for
        /// <see cref="NewTemplates"/> to compare. Nothing where the peek did not carry it.
        /// </summary>
        public TemplateLayout Layout { get; }

        /// <summary>
        /// A template made for this file, whose name the tool does not know and whose layout is
        /// a known template's, waiting on Use or Skip. Null for anything else.
        /// </summary>
        public KpiTemplate Proposed { get; }

        public bool IsNewTemplate
        {
            get { return Proposed != null && Template == null; }
        }

        /// <summary>
        /// A file whose name the tool does not know and whose layout matches no known template
        /// in the folder. It is not used.
        /// </summary>
        public bool UnknownLayout { get; }

        /// <summary>The same answer carrying the layout the peek read.</summary>
        public RecognisedWorkbook Laid(TemplateLayout layout)
        {
            return new RecognisedWorkbook(FileName, Template, Candidates, Reason, DecidedBy, FilledAs, layout, Proposed, UnknownLayout);
        }

        /// <summary>
        /// A file whose name the tool does not know, with the layout of <paramref name="sameAs"/>,
        /// and the template it would be filled as once someone presses Use.
        /// </summary>
        public static RecognisedWorkbook NewTemplate(string fileName, KpiTemplate sameAs, KpiTemplate proposed, TemplateLayout layout)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");
            if (sameAs == null) throw new ArgumentNullException("sameAs");
            if (proposed == null) throw new ArgumentNullException("proposed");

            return new RecognisedWorkbook(fileName, null, null,
                KpiPaneWords.NewTemplateSameAs(sameAs) + ". It would be filled as " + proposed.Name
                    + " with the cell map of " + sameAs.Name + ". Press Use in Setup to use it.",
                null, null, layout, proposed);
        }

        /// <summary>
        /// A file the tool used to offer, by its first sheet, whose name it does not know and
        /// whose layout matches nothing known in the folder, or a file no template recognises.
        /// </summary>
        public static RecognisedWorkbook NotUsed(RecognisedWorkbook workbook, string why)
        {
            if (workbook == null) throw new ArgumentNullException("workbook");

            return new RecognisedWorkbook(workbook.FileName, null, null,
                KpiPaneWords.UnknownTemplateLayout + ". " + why, null, null, workbook.Layout, null, true);
        }

        /// <summary>
        /// The same file matched to a template made for it, once Use was pressed or a
        /// components.csv row named that template.
        /// </summary>
        public static RecognisedWorkbook Used(RecognisedWorkbook workbook, KpiTemplate made)
        {
            if (workbook == null) throw new ArgumentNullException("workbook");
            if (made == null) throw new ArgumentNullException("made");

            return new RecognisedWorkbook(workbook.FileName, made, null, null, null, null, workbook.Layout);
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
                if (NeedsAPick)
                {
                    return string.Join(" or ", Candidates.Select(one => one.Name).ToArray())
                        + ". Pick one, nothing is guessed.";
                }
                if (IsFilled) return "filled, not offered. " + Reason;
                if (IsNewTemplate || UnknownLayout) return Reason;
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
            return Between(fileName, new[] { first, second });
        }

        public static RecognisedWorkbook Between(string fileName, IReadOnlyList<KpiTemplate> candidates)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");

            return new RecognisedWorkbook(fileName, null, candidates, null);
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

            if (byName.Any(one => one.IsVariant))
            {
                return ByVersion(fileName, byName, cells);
            }

            if (byName.Count == 1)
            {
                FilledCell decided = FilledMarks.Decide(cells);
                return decided == null ? Matched(fileName, byName[0]) : Filled(fileName, byName[0], decided);
            }

            bool existing = KpiNames.Holds(fileName, "EXISTING");
            bool future = KpiNames.Holds(fileName, "FUTURE");

            // The marks read the same cells whatever the template, so one answer covers both.
            FilledCell park = FilledMarks.Decide(cells);
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

        /// <summary>
        /// A first sheet that a kind and its versions share, `&lt;Mosques&gt;` for MOSQUES and its
        /// two 60 per cent versions. **The file name says which**, through the words that tell a
        /// version from its kind, MORE THAN 60 or LESS THAN 60, as whole words and with the
        /// spacing forgiven. No version's words in the name is the kind itself, which is every
        /// file this tool recognised before the versions existed. Two versions' words in one
        /// name is a pick for the user, never a guess.
        /// </summary>
        private static RecognisedWorkbook ByVersion(string fileName, IReadOnlyList<KpiTemplate> byName, IReadOnlyDictionary<string, string> cells)
        {
            string words = " " + NameWords(fileName) + " ";
            List<KpiTemplate> named = byName
                .Where(one => one.IsVariant && words.Contains(" " + VersionWords(one) + " "))
                .ToList();

            KpiTemplate kind = byName.FirstOrDefault(one => !one.IsVariant) ?? byName[0].Kind;
            FilledCell decided = FilledMarks.Decide(cells);

            if (named.Count > 1)
            {
                return decided == null
                    ? Between(fileName, named)
                    : NotATemplate(fileName, "It is a filled " + string.Join(" or ", named.Select(one => one.Name).ToArray())
                        + " checklist, which its file name cannot tell apart, not a template. " + decided.InWords);
            }

            KpiTemplate which = named.Count == 1 ? named[0] : kind;
            return decided == null ? Matched(fileName, which) : Filled(fileName, which, decided);
        }

        /// <summary>
        /// What a version's name adds to its kind's, MORE THAN 60 on MOSQUES MORE THAN 60.
        /// </summary>
        internal static string VersionWords(KpiTemplate version)
        {
            string whole = ComponentMatch.Key(version.Name);
            string kind = ComponentMatch.Key(version.Kind.Name);

            return whole.StartsWith(kind + " ", StringComparison.Ordinal) ? whole.Substring(kind.Length + 1) : whole;
        }

        /// <summary>
        /// A file name as words: the extension off, every run of anything but a letter or a
        /// digit one space, and the rest through <see cref="ComponentMatch"/>. A per cent sign is
        /// a gap too, so a file named MORE THAN 60% is the MORE THAN 60 version.
        /// </summary>
        internal static string NameWords(string fileName)
        {
            string bare = System.IO.Path.GetFileNameWithoutExtension(fileName ?? string.Empty);
            var spaced = new System.Text.StringBuilder();
            foreach (char one in bare) spaced.Append(char.IsLetterOrDigit(one) ? one : ' ');

            return ComponentMatch.Key(spaced.ToString());
        }
    }
}
