using System;
using System.IO;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Where one plot's checklist goes, or why nowhere.
    ///
    /// <code>
    /// MUGHARAZAT/                        the root the user browsed to
    ///    FRIDAY MOSQUE/                  the component folder
    ///       ANH-008-MO-100006/           the plot's own folder, its UID2
    ///          ANH-008-MO-100006.xlsx    the workbook, named after its folder
    /// </code>
    ///
    /// **The plot gets a folder of its own holding one file.** That reads as one file too many
    /// levels deep until you know why: a PDF of the same checklist is asked for beside it later,
    /// and a folder per plot is what leaves room for that without moving anything. Nothing here
    /// builds the PDF and nothing names one.
    ///
    /// **A UID2 that would not sit in a path REFUSES rather than being cleaned.** Every other
    /// name this tool writes goes through <see cref="ScanFileName.Cleaned"/>, and that is right
    /// for a name a person typed. This one is an identifier the team searches folders by, so a
    /// cleaned ANH/007 filed under ANH_007 is a plot nobody finds and no error anybody sees.
    /// </summary>
    public sealed class PlotWorkbookPath
    {
        private PlotWorkbookPath(bool ok, string folder, string uid2, string folderPath, string filePath, string why)
        {
            Ok = ok;
            Folder = folder ?? string.Empty;
            Uid2 = uid2 ?? string.Empty;
            FolderPath = folderPath ?? string.Empty;
            FilePath = filePath ?? string.Empty;
            Why = why ?? string.Empty;
        }

        public bool Ok { get; }

        /// <summary>
        /// The component folder under the root, empty when the component placed nothing.
        /// </summary>
        public string Folder { get; }

        public string Uid2 { get; }

        /// <summary>
        /// The plot's own folder. **It is created where it does not exist and never deleted**,
        /// and the writer puts a file into whatever is already there.
        /// </summary>
        public string FolderPath { get; }

        public string FilePath { get; }

        /// <summary>
        /// Why this plot has no path, empty when it has one. It is what the report prints beside
        /// the plot under the ones that wrote nothing.
        /// </summary>
        public string Why { get; }

        public const string NoRoot =
            "no output root is set, so there is nowhere to build the folder tree";

        public const string NoUid2 =
            "no " + KpiNames.PlotUid2 + " was read off this plot's first sheet, and the folder "
            + "and the file are both named after it";

        public static PlotWorkbookPath Refused(string why)
        {
            return new PlotWorkbookPath(false, string.Empty, string.Empty, string.Empty, string.Empty, why);
        }

        public static PlotWorkbookPath For(string root, string component, string uid2)
        {
            if (string.IsNullOrWhiteSpace(root)) return Refused(NoRoot);

            string folder = ComponentFolders.For(component);
            if (folder.Length == 0) return Refused(ComponentFolders.NoFolderFor(component));

            string held = (uid2 ?? string.Empty).Trim();
            if (held.Length == 0) return Refused(NoUid2);

            string bad = Unusable(held);
            if (bad.Length > 0) return Refused(bad);

            string plotFolder = Path.Combine(root.Trim(), folder, held);

            return new PlotWorkbookPath(
                true, folder, held, plotFolder,
                Path.Combine(plotFolder, held + OutputName.Extension),
                string.Empty);
        }

        /// <summary>
        /// What Windows will not take in a file or folder name, written out rather than asked
        /// for.
        ///
        /// **`Path.GetInvalidFileNameChars` answers for the machine the code is running on, and
        /// that is two different answers.** On Windows it names these nine and the control
        /// characters. On the Linux runner the gate uses it names two, the null and the forward
        /// slash, so a UID2 reading ANH*007 is refused by the tool and accepted by the test that
        /// is meant to be checking the tool. Revit runs on Windows, so the rule is Windows's and
        /// it is data here, the same as every other rule this tool applies.
        /// </summary>
        public static readonly char[] RefusedInAName =
            { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };

        /// <summary>
        /// Why this identifier cannot be a folder name, or empty when it can. A separator is
        /// named on its own, because a UID2 carrying one would put the plot outside the folder
        /// the tree says it is in. Both separators are checked whichever machine this runs on,
        /// for the same reason the list above is written out.
        /// </summary>
        private static string Unusable(string uid2)
        {
            if (uid2.IndexOf('/') >= 0 || uid2.IndexOf('\\') >= 0)
            {
                return KpiNames.PlotUid2 + " reads " + uid2
                    + ", which holds a path separator, so it cannot name a folder";
            }

            char[] found = uid2
                .Where(one => RefusedInAName.Contains(one) || one < ' ')
                .Distinct()
                .ToArray();
            if (found.Length == 0) return string.Empty;

            return KpiNames.PlotUid2 + " reads " + uid2 + ", which holds "
                + string.Join(" ", found.Select(one => "'" + one + "'").ToArray())
                + ", so it cannot name a folder";
        }
    }
}
