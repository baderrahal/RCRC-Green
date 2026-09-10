using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Where a report gets written, and what to say about it afterwards.
    ///
    /// The Desktop is where the team looks. A folder inside the repo is where anything reading
    /// the code can look, which is what makes a report useful to whoever picks this up next.
    /// The same file name goes in both, so a run is findable by date and model in either place.
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
        /// What the panel says once a report is written. Names every place it really landed and
        /// says plainly when the repo folder was not one of them, because a report nobody can
        /// find is the same as no report.
        /// </summary>
        public static string Written(IEnumerable<string> paths)
        {
            List<string> real = (paths ?? Enumerable.Empty<string>())
                .Where(path => !string.IsNullOrEmpty(path))
                .ToList();

            if (real.Count == 0)
            {
                return "The report could not be written anywhere.";
            }

            if (real.Count == 1)
            {
                return "Report at " + real[0]
                    + ". Not written into the repo, because " + PathFileName
                    + " is not next to the add-in. Run install.ps1 again to put it there.";
            }

            return "Report at " + string.Join(" and ", real.ToArray()) + ".";
        }
    }
}
