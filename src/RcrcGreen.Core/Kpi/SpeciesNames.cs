using System;
using System.Text;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// **THE ONE TIDY A BOTANICAL NAME GETS BEFORE IT IS COMPARED.** Bader, 29 October: every
    /// slash comes out and every run of whitespace reads as one space, on the workbook's name
    /// and on the Revit name alike, and then the two are compared without case.
    ///
    /// Why: the client's tree lists hold eight names with a slash, Acacia / Vachellia farnesiana
    /// and Acacia saligna / cyanophylla among them, the same eight on all fourteen tree sheets
    /// of the seven R3 templates, and NG05 found its slash trees counted nowhere. Measured on
    /// those fourteen sheets: the tidy makes no two rows on one sheet the same name, and where
    /// it ever does the match refuses as two rows always have.
    ///
    /// **Nothing else changes.** The apostrophe in 'PINK PIXIE' is kept, UNKNOWN still reaches
    /// Unknown Tree only through its alias, and a slash is never split into two names to try.
    /// A slash with no space either side joins its two words, ACACIA/VACHELLIA reading
    /// ACACIAVACHELLIA, which is the rule as decided and a question in `steps/02_questions.md`.
    ///
    /// The tree merge keys on this too, so two spellings of one tree are one species before
    /// any row is looked for, and two spellings this tidy makes one never aim two counts at one
    /// row. An alias is asked after the merge, so UNKNOWN and Unknown Tree on one plot still
    /// could, L118.
    /// </summary>
    public static class SpeciesNames
    {
        public const char Slash = '/';

        /// <summary>
        /// The name as it is compared: no slash, one space between words, no edge space, upper
        /// case. Null reads as empty.
        /// </summary>
        public static string Key(string name)
        {
            var kept = new StringBuilder();
            bool spaced = false;

            foreach (char one in name ?? string.Empty)
            {
                if (one == Slash) continue;

                if (char.IsWhiteSpace(one))
                {
                    spaced = true;
                    continue;
                }

                if (spaced && kept.Length > 0) kept.Append(' ');
                spaced = false;
                kept.Append(one);
            }

            return kept.ToString().ToUpperInvariant();
        }

        /// <summary>Whether a workbook name and a Revit name are the same tree.</summary>
        public static bool Same(string workbook, string revit)
        {
            return string.Equals(Key(workbook), Key(revit), StringComparison.Ordinal);
        }
    }
}
