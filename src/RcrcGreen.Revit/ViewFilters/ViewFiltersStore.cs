using System.IO;
using System.Reflection;
using RcrcGreen.Core.ViewFilters;

namespace RcrcGreen.Revit.ViewFilters
{
    /// <summary>
    /// Where ViewFilters.json lives: beside the installed assembly, put there by install.ps1
    /// the way the shipped title blocks and presets are. This holds the path and the read
    /// and nothing else. What the text means is Core's, in <see cref="ViewFiltersFile"/>,
    /// with the tests.
    /// </summary>
    internal static class ViewFiltersStore
    {
        /// <summary>
        /// The file's text, or empty when it is missing or unreadable. Core answers empty
        /// text with the reason, so nothing here needs a second wording for it.
        /// </summary>
        public static string ReadJson()
        {
            try
            {
                string beside = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(beside)) return string.Empty;

                string at = Path.Combine(beside, ViewFiltersFile.FileName);
                if (!File.Exists(at)) return string.Empty;

                return File.ReadAllText(at);
            }
            catch (System.UnauthorizedAccessException)
            {
                return string.Empty;
            }
            catch (IOException)
            {
                return string.Empty;
            }
        }

        /// <summary>Whether the shipped file is there at all, for the pane's missing line.</summary>
        public static bool Exists()
        {
            try
            {
                string beside = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return !string.IsNullOrEmpty(beside)
                    && File.Exists(Path.Combine(beside, ViewFiltersFile.FileName));
            }
            catch (System.UnauthorizedAccessException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}
