using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using RcrcGreen.Core;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// Writes a report to the Desktop and, when it can find it, to the reports folder inside
    /// the repo.
    ///
    /// The Desktop is where the team looks. The repo folder is where anything reading the code
    /// can look, which is the difference between a report that helps the next round and one
    /// that sits on somebody's machine. Same file name in both.
    ///
    /// Revit runs the add-in out of the Autodesk Addins folder and has no idea where the repo
    /// is, so install.ps1 leaves a file next to the assembly holding the path. No file means
    /// the Desktop only, and the caller says so rather than pretending both were written.
    /// </summary>
    internal static class ReportFile
    {
        /// <summary>
        /// Every place it landed. The Desktop write is allowed to throw, because a report that
        /// cannot be written at all is worth a message. The repo write is not, because it is
        /// the second copy and losing it must never cost the first.
        /// </summary>
        public static IReadOnlyList<string> Write(string fileName, string contents)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");

            var written = new List<string>();

            string desktop = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), fileName);

            File.WriteAllText(desktop, contents, new UTF8Encoding(false));
            written.Add(desktop);

            string inTheRepo = RepoFolder();
            if (inTheRepo.Length == 0) return written;

            try
            {
                Directory.CreateDirectory(inTheRepo);
                string alsoAt = Path.Combine(inTheRepo, fileName);
                File.WriteAllText(alsoAt, contents, new UTF8Encoding(false));
                written.Add(alsoAt);
            }
            catch (UnauthorizedAccessException)
            {
                // The repo may be on a drive this user cannot write to, or gone entirely. The
                // Desktop copy is already safe and the caller names only what really landed.
            }
            catch (IOException)
            {
            }

            return written;
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
