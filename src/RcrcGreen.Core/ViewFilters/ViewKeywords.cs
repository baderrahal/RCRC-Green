using System;
using System.Linq;

namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// The keyword box into the words the view collection matches on. Split on a comma, a
    /// newline or a semicolon, trimmed, blanks dropped, exactly the ported host's split. A
    /// Windows text box ends its lines with a carriage return as well, and the trim takes
    /// that off each entry.
    /// </summary>
    public static class ViewKeywords
    {
        public static string[] Split(string text)
        {
            return string.IsNullOrWhiteSpace(text)
                ? new string[0]
                : text.Split(new char[] { ',', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray();
        }
    }
}
