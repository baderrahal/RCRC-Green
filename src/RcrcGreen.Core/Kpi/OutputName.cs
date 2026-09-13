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

        // **`Suggested` is deleted, and NOT on reachability this time.** It offered a row the
        // template file's own name to write under, and was restored once after being deleted on
        // reachability alone, because it was then the only record of the shape the working runs
        // wrote. A workbook is one plot now and every one is named from its plot's own
        // PRX_Plot_UID2, so there is no name to suggest: the SHAPE is gone rather than its last
        // caller. `Final` stays, because the cleaning a name needs is still a rule and the
        // extension beside it is what `PlotWorkbookPath` builds every file name with.

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
