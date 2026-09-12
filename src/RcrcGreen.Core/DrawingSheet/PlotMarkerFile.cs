using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One line of the marker file: which model, which plot, which marker. The model is in
    /// every row because one file remembers every model the user works in, the way the title
    /// block file remembers every view type.
    /// </summary>
    public sealed class PlotMarkerRow
    {
        public PlotMarkerRow(string model, string plotId, string marker)
        {
            Model = (model ?? string.Empty).Trim();
            PlotId = (plotId ?? string.Empty).Trim();
            Marker = (marker ?? string.Empty).Trim();
        }

        public string Model { get; }

        public string PlotId { get; }

        public string Marker { get; }
    }

    /// <summary>
    /// What the marker file held, and every line of it that could not be read. An unreadable
    /// line is kept and said, never dropped, because a file one line short reads exactly like
    /// a file that never had the line.
    /// </summary>
    public sealed class PlotMarkerFileContents
    {
        internal PlotMarkerFileContents(
            IReadOnlyList<PlotMarkerRow> rows, IReadOnlyList<string> notRead)
        {
            Rows = rows;
            NotRead = notRead;
        }

        public IReadOnlyList<PlotMarkerRow> Rows { get; }

        public IReadOnlyList<string> NotRead { get; }

        /// <summary>
        /// The rows for one model, as the markers the panel holds. The model is matched
        /// exactly, because two titles differing by case could be two different files on
        /// disk.
        /// </summary>
        public IReadOnlyList<PlotMarker> For(string model)
        {
            string wanted = (model ?? string.Empty).Trim();

            return Rows
                .Where(one => string.Equals(one.Model, wanted, StringComparison.Ordinal))
                .Select(one => new PlotMarker(one.PlotId, one.Marker, MarkerSource.Remembered))
                .ToList();
        }
    }

    /// <summary>
    /// Reading and writing the plot marker file.
    ///
    /// Tab separated, three fields: the model, the plot and the marker. The model is the
    /// document's title, which is the file name without its extension, because that is the
    /// one name a detached model that has never been saved still carries. Nothing here
    /// touches a file: where it sits belongs to the machine and is the store's one job.
    /// </summary>
    public static class PlotMarkerFile
    {
        public const char Separator = '\t';

        public const char NoteMark = '#';

        public static PlotMarkerFileContents Read(string text)
        {
            var rows = new List<PlotMarkerRow>();
            var notRead = new List<string>();

            if (string.IsNullOrEmpty(text))
            {
                return new PlotMarkerFileContents(rows, notRead);
            }

            string[] lines = text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            for (int at = 0; at < lines.Length; at++)
            {
                string line = lines[at];
                if (line.Trim().Length == 0) continue;
                if (line.TrimStart().Length > 0 && line.TrimStart()[0] == NoteMark) continue;

                string[] fields = line.Split(Separator);
                if (fields.Length != 3
                    || fields[0].Trim().Length == 0
                    || fields[1].Trim().Length == 0
                    || fields[2].Trim().Length == 0)
                {
                    notRead.Add("line "
                        + (at + 1).ToString(CultureInfo.InvariantCulture) + ", " + line);
                    continue;
                }

                rows.Add(new PlotMarkerRow(fields[0], fields[1], fields[2]));
            }

            return new PlotMarkerFileContents(rows, notRead);
        }

        /// <summary>
        /// The text of the file, every model's rows together, with a note at the top for
        /// whoever opens it looking for why a marker filled itself in.
        /// </summary>
        public static string Write(IEnumerable<PlotMarkerRow> rows)
        {
            var text = new StringBuilder();

            text.Append("# RCRC Green, the sheet number marker of each plot, per model.").Append(Line);
            text.Append("# One row per plot, tab separated: model, plot, marker. The model is ")
                .Append("the document title.").Append(Line);
            text.Append("# Written by the Drawing Sheet panel whenever a marker is set in step 1.")
                .Append(Line);

            foreach (PlotMarkerRow one in (rows ?? Enumerable.Empty<PlotMarkerRow>())
                .Where(one => one != null
                    && one.Model.Length > 0 && one.PlotId.Length > 0 && one.Marker.Length > 0)
                .OrderBy(one => one.Model, StringComparer.Ordinal)
                .ThenBy(one => one.PlotId, NaturalOrder.Comparer))
            {
                text.Append(one.Model).Append(Separator)
                    .Append(one.PlotId).Append(Separator)
                    .Append(one.Marker).Append(Line);
            }

            return text.ToString();
        }

        private const string Line = "\r\n";
    }
}
