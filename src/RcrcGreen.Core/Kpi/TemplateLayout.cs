using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// What a template workbook looks like to the tool: every sheet name in order, and every
    /// label on its first sheet at its address. Two workbooks with the same layout take the same
    /// cell map, because every cell the map writes is found by a letter or by a label on that
    /// sheet, and every sheet the run reads is found by its name.
    ///
    /// **A label is a cell holding typed text**, a shared or an inline string with no formula.
    /// A formula's result is not a label, so MOSQUES MORE THAN 60, whose five changed cells are
    /// formulas reading another row of targets, has the layout of MOSQUES. A label is compared
    /// through <see cref="LabelText"/>, edge whitespace off both sides and the inside untouched,
    /// the one rule every label comparison in this tool goes through.
    ///
    /// It is a stricter test than the map needs, on purpose. A template the client changed in
    /// any typed word of its first sheet is a template a person should look at before it is
    /// filled, and the answer for one is Unknown template layout rather than a guess.
    /// </summary>
    public sealed class TemplateLayout
    {
        private TemplateLayout(IReadOnlyList<string> sheetNames, IReadOnlyDictionary<string, string> labels)
        {
            SheetNames = sheetNames;
            Labels = labels;
        }

        public IReadOnlyList<string> SheetNames { get; }

        /// <summary>The first sheet's labels by cell address, upper case, edge whitespace off.</summary>
        public IReadOnlyDictionary<string, string> Labels { get; }

        /// <summary>Nothing could be read, so this matches nothing.</summary>
        public bool Empty
        {
            get { return SheetNames.Count == 0; }
        }

        public static TemplateLayout Of(IEnumerable<string> sheetNames, IReadOnlyDictionary<string, string> firstSheetLabels)
        {
            var labels = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, string> one in firstSheetLabels ?? new Dictionary<string, string>())
            {
                string text = LabelText.Trimmed(one.Value);
                if (text.Length == 0 || string.IsNullOrWhiteSpace(one.Key)) continue;

                labels[one.Key.Trim().ToUpperInvariant()] = text;
            }

            return new TemplateLayout((sheetNames ?? Enumerable.Empty<string>()).Select(one => one ?? string.Empty).ToList(), labels);
        }

        public static readonly TemplateLayout Nothing = Of(null, null);

        public bool Same(TemplateLayout other)
        {
            return other != null && !Empty && !other.Empty && Difference(other).Length == 0;
        }

        /// <summary>
        /// The first way the two differ, in words, or empty where they are the same. Said in the
        /// report and the tooltip so Unknown template layout can be checked against the file.
        /// </summary>
        public string Difference(TemplateLayout other)
        {
            if (other == null || Empty || other.Empty) return "one of the two could not be read";

            if (!SheetNames.SequenceEqual(other.SheetNames, StringComparer.Ordinal))
            {
                return "its sheets are " + string.Join(", ", SheetNames.ToArray())
                    + " and the other's are " + string.Join(", ", other.SheetNames.ToArray());
            }

            foreach (string cell in Labels.Keys.Union(other.Labels.Keys).OrderBy(one => one, StringComparer.Ordinal))
            {
                string mine;
                string theirs;
                Labels.TryGetValue(cell, out mine);
                other.Labels.TryGetValue(cell, out theirs);

                if (!LabelText.Same(mine, theirs))
                {
                    return SheetNames[0] + " " + cell + " reads " + Said(mine) + " and the other's reads " + Said(theirs);
                }
            }

            return string.Empty;
        }

        private static string Said(string text)
        {
            return string.IsNullOrEmpty(text) ? "nothing" : "'" + text + "'";
        }
    }
}
