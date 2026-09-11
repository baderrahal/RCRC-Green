using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Where a report gets written, and what to say about it afterwards.
    ///
    /// One place: the reports folder inside the repo. It went to the Desktop as well until the
    /// round after the first real run, which meant two files per run, two places to look and
    /// two to tidy up, and the Desktop one was the copy nothing reading the code could reach.
    ///
    /// That folder is in .gitignore and stays there. This repository is public and a report
    /// carries client view names, sheet numbers, plot identifiers and schedule setups.
    /// </summary>
    public static class ReportPlaces
    {
        /// <summary>
        /// The file that install.ps1 leaves next to the assembly, holding the absolute path of
        /// that folder. Revit runs the add-in from the Autodesk Addins folder and has no idea
        /// where the repo is, so the installer is the only thing that can say.
        /// </summary>
        public const string PathFileName = "reports-folder.txt";

        /// <summary>
        /// What the panel says once a report is written. It names where the file really is, and
        /// when there is no file it says why and what to do, because a report nobody can find is
        /// the same as no report.
        ///
        /// Nothing is written at all when the pointer is missing. That used to be the case where
        /// one path came back and the line said the Desktop copy was the only one, and the whole
        /// point of dropping the Desktop copy is that there is no second place for a report to
        /// be hiding in.
        /// </summary>
        public static string Written(IEnumerable<string> paths)
        {
            List<string> real = (paths ?? Enumerable.Empty<string>())
                .Where(path => !string.IsNullOrEmpty(path))
                .ToList();

            if (real.Count == 0)
            {
                return "NO REPORT WAS WRITTEN, because " + PathFileName + " is not next to the "
                    + "add-in and it is the only thing that says where the reports folder is. "
                    + "Run install.ps1 again to put it there, then run this again.";
            }

            return "Report at " + string.Join(" and ", real.ToArray()) + ".";
        }
    }
}
