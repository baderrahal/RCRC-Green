using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using RcrcGreen.Core;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// Writes a report to the reports folder inside the repo, and nowhere else.
    ///
    /// **It went to the Desktop as well until the round after the first real run.** Two copies
    /// of every report meant two places to look, two to tidy up and two that could disagree once
    /// one of them was moved, and the Desktop copy is the one nothing reading the code can ever
    /// reach. One place, named in full on the panel, is what the team asked for.
    ///
    /// Revit runs the add-in out of the Autodesk Addins folder and has no idea where the repo
    /// is, so install.ps1 leaves a file next to the assembly holding the path. **No file means
    /// nothing is written at all**, and the caller says so plainly. Falling back to the Desktop
    /// would put the file exactly where nobody agreed to look for it.
    /// </summary>
    internal static class ReportFile
    {
        /// <summary>
        /// Where it landed, which is one path or none. The write is allowed to throw, because a
        /// report that cannot be written is worth a message rather than a silent gap: it is the
        /// only copy now.
        /// </summary>
        public static IReadOnlyList<string> Write(string fileName, string contents)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");

            string inTheRepo = RepoFolder();
            if (inTheRepo.Length == 0) return new List<string>();

            Directory.CreateDirectory(inTheRepo);

            string at = Path.Combine(inTheRepo, fileName);
            File.WriteAllText(at, contents, new UTF8Encoding(false));

            return new List<string> { at };
        }

        private static string RepoFolder()
        {
            try
            {
                string beside = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(beside)) return string.Empty;

                string pointer = Path.Combine(beside, ReportPlaces.PathFileName);
                if (!File.Exists(pointer)) return string.Empty;

                return File.ReadAllText(pointer).Trim();
            }
            catch (UnauthorizedAccessException)
            {
                return string.Empty;
            }
            catch (IOException)
            {
                return string.Empty;
            }
        }
    }
}
