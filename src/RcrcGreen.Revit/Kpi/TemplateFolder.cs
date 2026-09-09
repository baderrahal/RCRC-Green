using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RcrcGreen.Revit.Kpi
{
    /// <summary>
    /// Where the GRP KPI Checklist templates live, remembered in a file beside the installed
    /// assembly. Revit runs the add-in out of the Autodesk Addins folder and has no other way
    /// to know, which is the same reason reports-folder.txt exists. install.ps1 creates the
    /// file empty when it is missing and never overwrites a folder the user has set.
    /// </summary>
    internal static class TemplateFolder
    {
        public const string PointerFileName = "templates-folder.txt";

        /// <summary>
        /// The remembered folder, or empty when none is set, the pointer is missing or the
        /// folder it names is gone. An empty answer shows the set-the-folder line rather than
        /// an empty list that reads as a folder with nothing in it.
        /// </summary>
        public static string Read()
        {
            try
            {
                string pointer = PointerPath();
                if (pointer == null || !File.Exists(pointer)) return string.Empty;

                string folder = File.ReadAllText(pointer).Trim();
                return folder.Length > 0 && Directory.Exists(folder) ? folder : string.Empty;
            }
            catch (IOException)
            {
                return string.Empty;
            }
            catch (UnauthorizedAccessException)
            {
                return string.Empty;
            }
        }

        public static bool Remember(string folder)
        {
            try
            {
                string pointer = PointerPath();
                if (pointer == null) return false;

                File.WriteAllText(pointer, folder ?? string.Empty);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>
        /// Every .xlsx in the folder by name, full paths, in name order. A folder that
        /// refuses to be read comes back empty and the folder line still shows the path, so
        /// the fault is visible where the person is looking.
        /// </summary>
        public static IReadOnlyList<string> WorkbooksIn(string folder)
        {
            if (string.IsNullOrEmpty(folder)) return new List<string>();

            try
            {
                return Directory.GetFiles(folder, "*.xlsx")
                    .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch (IOException)
            {
                return new List<string>();
            }
            catch (UnauthorizedAccessException)
            {
                return new List<string>();
            }
        }

        private static string PointerPath()
        {
            string beside = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return string.IsNullOrEmpty(beside) ? null : Path.Combine(beside, PointerFileName);
        }
    }
}
