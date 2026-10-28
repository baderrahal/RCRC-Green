using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// A new template without a code change. The templates folder is read file by file through
    /// <see cref="RecognisedWorkbook.Recognise"/>, and this looks at the whole folder after it.
    ///
    /// **A file the tool knows by name** is one whose name, as words, ends with the name of the
    /// template it was recognised as: GRP_-_KPI_Checklist_-_DD_MOSQUES MORE THAN 60 ends with
    /// MOSQUES MORE THAN 60. The client names all eleven that way. Those stand as they are.
    ///
    /// **Any other file is compared by layout** with the known files in the same folder,
    /// through <see cref="TemplateLayout"/>. One that matches is a new template: it is offered
    /// in Setup as New template, same layout as MOSQUES, with Use and Skip, and it is filled
    /// with that template's cell map and kind. Its name is its file name less the words every
    /// known file opens with, so GRP_-_KPI_Checklist_-_DD_MOSQUES MORE THAN 80 is MOSQUES MORE
    /// THAN 80. One that matches nothing is Unknown template layout and is never used.
    ///
    /// Pressing Use holds for the Revit session. **A components.csv row naming the template is
    /// a use too**, because somebody already chose to send plots to it, and it lasts as long as
    /// the row does. Nothing is used on a layout alone.
    /// </summary>
    public static class NewTemplates
    {
        /// <param name="listed">The folder as recognised, each with the layout its peek read.</param>
        /// <param name="used">Template names pressed Use on, or named by a components.csv row.</param>
        /// <param name="made">
        /// The templates made so far, by name, kept by the caller so a redraw hands back the same
        /// object and a tick held against it still holds. New ones are added to it.
        /// </param>
        public static IReadOnlyList<RecognisedWorkbook> Review(
            IReadOnlyList<RecognisedWorkbook> listed, IEnumerable<string> used, IDictionary<string, KpiTemplate> made)
        {
            if (listed == null) return new List<RecognisedWorkbook>();
            if (made == null) throw new ArgumentNullException("made");

            var usedKeys = new HashSet<string>((used ?? Enumerable.Empty<string>()).Select(ComponentMatch.Key), StringComparer.Ordinal);
            List<RecognisedWorkbook> known = listed.Where(KnownByName).ToList();
            string opening = SharedOpening(known);

            var reviewed = new List<RecognisedWorkbook>();
            foreach (RecognisedWorkbook one in listed)
            {
                if (KnownByName(one) || one.IsFilled || one.NeedsAPick || one.Layout.Empty)
                {
                    reviewed.Add(one);
                    continue;
                }

                RecognisedWorkbook sameAs = known
                    .Where(other => other.Layout.Same(one.Layout))
                    .OrderBy(other => other.Template.IsVariant ? 1 : 0)
                    .FirstOrDefault();

                if (sameAs == null)
                {
                    reviewed.Add(RecognisedWorkbook.NotUsed(one, Why(one, known)));
                    continue;
                }

                KpiTemplate template = Made(NameFor(one.FileName, opening), sameAs.Template, one.Layout, made);
                reviewed.Add(usedKeys.Contains(ComponentMatch.Key(template.Name))
                    ? RecognisedWorkbook.Used(one, template)
                    : RecognisedWorkbook.NewTemplate(one.FileName, sameAs.Template, template, one.Layout));
            }

            return reviewed;
        }

        /// <summary>
        /// Whether a file's name says which template it is: it was recognised as one, and its
        /// name as words ends with that template's name.
        /// </summary>
        public static bool KnownByName(RecognisedWorkbook workbook)
        {
            if (workbook == null || !workbook.IsMatched) return false;

            string words = RecognisedWorkbook.NameWords(workbook.FileName);
            string name = ComponentMatch.Key(workbook.Template.Name);

            return words == name || words.EndsWith(" " + name, StringComparison.Ordinal);
        }

        /// <summary>
        /// A new template's name: its file name as words, less the words every known file in the
        /// folder opens with before its template's name, GRP KPI CHECKLIST DD on the client's.
        /// </summary>
        public static string NameFor(string fileName, string opening)
        {
            string words = RecognisedWorkbook.NameWords(fileName);
            return opening.Length > 0 && words.StartsWith(opening + " ", StringComparison.Ordinal)
                ? words.Substring(opening.Length + 1)
                : words;
        }

        /// <summary>
        /// The words every known file opens with, or empty where they open differently or there
        /// is none.
        /// </summary>
        public static string SharedOpening(IEnumerable<RecognisedWorkbook> known)
        {
            List<string> openings = (known ?? Enumerable.Empty<RecognisedWorkbook>())
                .Select(one =>
                {
                    string words = RecognisedWorkbook.NameWords(one.FileName);
                    string name = ComponentMatch.Key(one.Template.Name);
                    return words.Length > name.Length ? words.Substring(0, words.Length - name.Length).TrimEnd() : string.Empty;
                })
                .Distinct(StringComparer.Ordinal)
                .ToList();

            return openings.Count == 1 ? openings[0] : string.Empty;
        }

        private static KpiTemplate Made(string name, KpiTemplate sameAs, TemplateLayout layout, IDictionary<string, KpiTemplate> made)
        {
            string key = ComponentMatch.Key(name);
            KpiTemplate held;
            if (made.TryGetValue(key, out held) && ReferenceEquals(held.Kind, sameAs.Kind)) return held;

            held = new KpiTemplate(
                name,
                layout.SheetNames[0],
                sameAs.Cells,
                new TreeSheet(sameAs.ExistingTrees.SheetName),
                new TreeSheet(sameAs.ProposedTrees.SheetName),
                sameAs.GroupsCountedAsProposed,
                sameAs.Kind);
            made[key] = held;
            return held;
        }

        private static string Why(RecognisedWorkbook one, IReadOnlyList<RecognisedWorkbook> known)
        {
            if (known.Count == 0) return "No file in this folder is named for a template this tool knows, so there is nothing to compare it with.";

            RecognisedWorkbook nearest = known.FirstOrDefault(other => other.Layout.SheetNames.SequenceEqual(one.Layout.SheetNames, StringComparer.Ordinal))
                ?? known[0];

            return "Its name names no template this tool knows and its layout matches none in this folder. Against "
                + nearest.FileName + ", " + one.Layout.Difference(nearest.Layout) + ".";
        }
    }
}
