using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Whether the form file on disk is the form this tool knows.
    ///
    /// **THE TOOL MUST NOT TRUST THE FIELD NAMES.** Five of the nine plot prefixes use a form
    /// whose UID is a field called undefined_4.1 and whose lawn area is a field called 0_2.
    /// Writing a lawn area into a box called 0_2 that has become something else is exactly the
    /// silent wrong number this tool exists to prevent, so the names AND the note sitting in
    /// each are checked on every run, and a form reissued with either moved writes NOTHING.
    /// </summary>
    public sealed class PdfFormCheck
    {
        private PdfFormCheck(
            PdfForm form, bool read, string refusal, int fieldsFound,
            IEnumerable<string> missing, IEnumerable<string> differingNotes)
        {
            Form = form;
            Read = read;
            Refusal = refusal ?? string.Empty;
            FieldsFound = fieldsFound;
            Missing = (missing ?? Enumerable.Empty<string>()).ToList();
            DifferingNotes = (differingNotes ?? Enumerable.Empty<string>()).ToList();
        }

        public static PdfFormCheck NotRead(PdfForm form, string refusal)
        {
            return new PdfFormCheck(form, false, refusal, 0, null, null);
        }

        public PdfForm Form { get; }

        /// <summary>
        /// False where the file could not be read at all, which is different from a file read
        /// and found to be another form.
        /// </summary>
        public bool Read { get; }

        public string Refusal { get; }

        public int FieldsFound { get; }

        /// <summary>Fields this tool fills that the file does not hold, by name.</summary>
        public IReadOnlyList<string> Missing { get; }

        /// <summary>
        /// Fields whose note has moved, each said with what the file holds and what the tool
        /// knows, because a note that changed is the client changing what the field means.
        /// </summary>
        public IReadOnlyList<string> DifferingNotes { get; }

        /// <summary>
        /// **Nothing is written into a form that does not match.** Both halves have to hold.
        /// </summary>
        public bool Matched
        {
            get { return Read && Missing.Count == 0 && DifferingNotes.Count == 0; }
        }

        public string Why
        {
            get
            {
                if (!Read) return Refusal;
                if (Matched) return string.Empty;

                var said = new List<string>();
                if (Missing.Count > 0)
                {
                    said.Add("fields this tool fills are not in the file: " + string.Join(", ", Missing.ToArray()));
                }

                if (DifferingNotes.Count > 0)
                {
                    said.Add("fields whose note has moved: " + string.Join(" and ", DifferingNotes.ToArray()));
                }

                return "the form does not match what this tool knows, so nothing was written into it. "
                    + string.Join(". ", said.ToArray());
            }
        }

        /// <summary>
        /// The file's fields held against the form the tool knows. Only the fields this tool
        /// FILLS are checked: the client's own sidewalks and toilets can move freely, because
        /// nothing here writes into them.
        /// </summary>
        public static PdfFormCheck Of(PdfForm form, IReadOnlyList<PdfFieldRead> fields, string refusal)
        {
            if (form == null) throw new ArgumentNullException("form");
            if (!string.IsNullOrWhiteSpace(refusal)) return NotRead(form, refusal);

            IReadOnlyList<PdfFieldRead> held = fields ?? new List<PdfFieldRead>();
            var missing = new List<string>();
            var differing = new List<string>();

            foreach (PdfFormField wanted in form.Fields)
            {
                PdfFieldRead found = held.FirstOrDefault(
                    one => string.Equals(one.Name, wanted.FieldName, StringComparison.Ordinal));

                if (found == null)
                {
                    missing.Add(wanted.FieldName);
                    continue;
                }

                if (!Same(found.Value, wanted.Note))
                {
                    differing.Add(wanted.FieldName + " holds " + Shown(found.Value)
                        + " where this tool knows " + Shown(wanted.Note));
                }
            }

            return new PdfFormCheck(form, true, string.Empty, held.Count, missing, differing);
        }

        /// <summary>
        /// The note is compared with edge whitespace off and without case. **Nothing looser**: a
        /// note that reads differently is the client saying the field means something else, and
        /// a near miss is how a number lands in a box nobody measured.
        /// </summary>
        private static bool Same(string held, string wanted)
        {
            return string.Equals((held ?? string.Empty).Trim(), (wanted ?? string.Empty).Trim(),
                StringComparison.OrdinalIgnoreCase);
        }

        private static string Shown(string text)
        {
            return string.IsNullOrEmpty(text) ? "nothing" : "\"" + text + "\"";
        }
    }
}
