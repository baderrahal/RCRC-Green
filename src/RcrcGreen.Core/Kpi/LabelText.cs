using System;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// **THE ONE RULE FOR COMPARING A LABEL AGAINST WHAT A FILE HOLDS.**
    ///
    /// Every whole-label lookup in this tool asks this and nothing else: the labelled cells on a
    /// template's main sheet, the two cells the workbook computes, and the header columns of the
    /// team's street reference file.
    ///
    /// **EDGE WHITESPACE OFF BOTH SIDES, WITHOUT CASE, AND THE INSIDE UNTOUCHED.** A label with a
    /// space at either end is the same label. **A DOUBLE SPACE BETWEEN WORDS IS A DIFFERENT
    /// LABEL AND MUST NOT MATCH**, because a title block in this project really is named
    /// `LOD /  HARDSCAPE SCHEDULES` with two spaces, and a comparison that collapsed runs would
    /// quietly answer for a name the file does not hold.
    ///
    /// **IT EXISTS BECAUSE THE RULE WAS BURIED AND ONE SIDED.** The lookups trimmed the cell's
    /// text where they read it and compared it against the label as written, so the rule lived
    /// in a bare `Trim()` that nothing named and that covered one side of the comparison only.
    /// Measured on 14 September, by taking that trim out: the green cover lookup came back with
    /// `no cell on &lt;Mosques&gt; reads Total Green cover (m²)` and five cases reddened. It
    /// worked, and nothing said so, and a label constant carrying a stray space would still have
    /// failed with no sign of why.
    ///
    /// **AND THE LABELS THEMSELVES ARE WHAT THE FILE HOLDS.** All seven templates carry
    /// ` Total Green cover (m²)` with a LEADING SPACE, measured by Bader on all seven. The
    /// client's PDF note writes it without. A label is what the file holds, not what a note
    /// calls it.
    /// </summary>
    public static class LabelText
    {
        /// <summary>
        /// Whether two labels are the same label. Null and empty are the same as each other and
        /// match nothing else, because a cell with no text names no label.
        /// </summary>
        public static bool Same(string one, string other)
        {
            return string.Equals(Trimmed(one), Trimmed(other), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// A label with its edge whitespace off and every character between its ends kept, which
        /// is the half of the rule that has to be said out loud.
        /// </summary>
        public static string Trimmed(string text)
        {
            return (text ?? string.Empty).Trim();
        }
    }
}
