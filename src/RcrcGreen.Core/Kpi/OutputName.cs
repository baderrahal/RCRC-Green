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
        /// This round the suggestion is the template's file name. Next round it gains the
        /// component name read from the model.
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
