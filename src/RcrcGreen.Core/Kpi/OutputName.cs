using System;
using System.Globalization;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The name the filled workbook is written under. The user types it, prefilled with a
    /// suggestion, and it goes through the same cleaning ScanFileName already does, because
    /// a Revit document title has already proven that a name can carry characters Windows
    /// will not take in a path.
    /// </summary>
    public static class OutputName
    {
        public const string Extension = ".xlsx";

        /// <summary>
        /// The name a row offers: the template file's own name, cleaned. **The shape matters and
        /// it was lost.** The several templates round deleted this as unreachable once every row
        /// named itself, and the rows named themselves after the template alone, so the boxes
        /// read MOSQUES and a run would have written MOSQUES.xlsx, which nobody recognises in a
        /// folder three months later. The working runs wrote
        /// GRP-KPI-Checklist-DD-MOSQUES.xlsx and GRP-KPI-Checklist-DD-STREETS.xlsx, and this is
        /// what wrote them.
        ///
        /// Asked per ticked row with that row's own file, so several templates in one press each
        /// carry the shape rather than one of them carrying it.
        /// </summary>
        public static string Suggested(string templateFileName)
        {
            return Final(templateFileName);
        }

        /// <summary>
        /// What the file is really called: cleaned, and with .xlsx put back if the user left
        /// it off or the cleaning took the typed name apart.
        /// </summary>
        public static string Final(string typed)
        {
            string bare = typed ?? string.Empty;

            if (bare.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
            {
                bare = bare.Substring(0, bare.Length - Extension.Length);
            }

            return ScanFileName.Cleaned(bare) + Extension;
        }
    }
}
