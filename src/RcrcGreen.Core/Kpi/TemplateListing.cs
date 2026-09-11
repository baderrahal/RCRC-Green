using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The templates folder's workbooks as recognised, held once per folder, and how many
    /// times a workbook was opened to build it. The pane redraws its whole template block on
    /// every tick, every picker press and every grouping press, and every redraw used to
    /// open and peek every .xlsx in the folder on the interface thread: seven zips for each of
    /// 155 ticks. This is the one copy the pane holds under THE PANE HOLDS NO COPY OF
    /// ANYTHING IT CAN ASK FOR: keyed on the folder, cleared when the folder changes and
    /// after any press of Create that reached the patcher with an output path in it, wrote or
    /// not, and the folder itself is listed on every draw so a file added or gone is seen on
    /// the next redraw and opened once.
    ///
    /// It records and does not decide: <see cref="Opened"/> and <see cref="Drawn"/> go into
    /// the report so the seven opens per tick cannot come back unnoticed.
    /// </summary>
    public sealed class TemplateListing
    {
        private readonly Dictionary<string, RecognisedWorkbook> _byPath;

        private TemplateListing(string folder, IReadOnlyList<string> paths, Dictionary<string, RecognisedWorkbook> byPath, int opened, int drawn)
        {
            Folder = folder ?? string.Empty;
            _byPath = byPath ?? new Dictionary<string, RecognisedWorkbook>(StringComparer.OrdinalIgnoreCase);
            Paths = paths ?? new List<string>();
            Opened = opened;
            Drawn = drawn;
        }

        public static readonly TemplateListing Nothing =
            new TemplateListing(string.Empty, null, null, 0, 0);

        public string Folder { get; }

        /// <summary>
        /// The full paths as the folder listed them on the last draw, in that order.
        /// </summary>
        public IReadOnlyList<string> Paths { get; }

        /// <summary>
        /// One recognised workbook per path, in the folder's order.
        /// </summary>
        public IReadOnlyList<RecognisedWorkbook> Workbooks
        {
            get { return Paths.Select(one => _byPath[one]).ToList(); }
        }

        /// <summary>
        /// Zip opens made to build every listing of this folder so far.
        /// </summary>
        public int Opened { get; }

        /// <summary>
        /// Redraws that asked for this folder's listing so far.
        /// </summary>
        public int Drawn { get; }

        public bool IsNothing
        {
            get { return Folder.Length == 0 && Drawn == 0; }
        }

        /// <summary>
        /// The same folder, compared the way Windows compares a path: without case and with
        /// a trailing separator off. An empty folder is nobody's.
        /// </summary>
        public bool IsFor(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder) || Folder.Length == 0) return false;

            return string.Equals(Trimmed(Folder), Trimmed(folder), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The listing after one draw of <paramref name="folder"/> holding <paramref name="paths"/>.
        /// A folder this is not for opens every path. The same folder opens only the paths it
        /// has not seen, drops the ones gone, and keeps the rest as they were recognised.
        /// </summary>
        public TemplateListing For(string folder, IReadOnlyList<string> paths, Func<string, RecognisedWorkbook> recognise)
        {
            if (recognise == null) throw new ArgumentNullException("recognise");

            List<string> listed = (paths ?? new List<string>()).Where(one => !string.IsNullOrWhiteSpace(one)).ToList();
            bool same = IsFor(folder);
            var held = new Dictionary<string, RecognisedWorkbook>(StringComparer.OrdinalIgnoreCase);
            int opened = same ? Opened : 0;

            foreach (string path in listed)
            {
                RecognisedWorkbook already;
                if (same && _byPath.TryGetValue(path, out already))
                {
                    held[path] = already;
                    continue;
                }

                held[path] = recognise(path);
                opened++;
            }

            return new TemplateListing(folder, listed, held, opened, (same ? Drawn : 0) + 1);
        }

        public string InWords
        {
            get { return TemplateWords.Opened(Paths.Count, Opened, Drawn); }
        }

        private static string Trimmed(string folder)
        {
            return folder.Trim().TrimEnd('\\', '/');
        }
    }
}
