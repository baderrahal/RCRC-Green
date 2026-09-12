using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RcrcGreen.Core;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// The plot marker file, and nothing about what a marker means.
    ///
    /// One file for every model, in the same folder as the user's title block settings, so a
    /// plot set once never has to be set again. A model is identified by its document title,
    /// the file name without its extension, because that is the one name a detached model
    /// that has never been saved still carries. Reading, matching and writing the rows is
    /// <see cref="PlotMarkerFile"/> and <see cref="PlotMarkers"/>, with tests.
    ///
    /// No Revit API call is made here, so the panel reads and writes it directly rather than
    /// through the external event.
    /// </summary>
    internal static class PlotMarkerStore
    {
        public const string FileName = "plot-markers.txt";

        /// <summary>
        /// The markers remembered for one model, with every line the file holds that could
        /// not be read.
        /// </summary>
        public static StoredMarkers Read(string model)
        {
            PlotMarkerFileContents held = PlotMarkerFile.Read(TextIn(UserPath()));

            var notRead = new List<string>();
            foreach (string line in held.NotRead) notRead.Add("plot markers, " + line);

            return new StoredMarkers(PlotMarkers.Remembered(held.For(model)), notRead);
        }

        /// <summary>
        /// Writes one model's markers, keeping every other model's rows as the file holds
        /// them now, and hands back why not when it could not. A marker that looks saved and
        /// is not would have somebody set the same plot again next week.
        /// </summary>
        public static string Save(string model, PlotMarkers markers)
        {
            if (markers == null) return string.Empty;

            string wanted = (model ?? string.Empty).Trim();
            if (wanted.Length == 0)
            {
                return "That marker could not be remembered, because this model has no title "
                    + "to remember it under.";
            }

            string path = UserPath();
            if (path.Length == 0)
            {
                return "That marker could not be remembered, because this machine gave no "
                    + "folder to keep settings in.";
            }

            // The file is read again at every save so another model's rows written since
            // this panel loaded are kept rather than overwritten with a stale copy.
            List<PlotMarkerRow> rows = PlotMarkerFile.Read(TextIn(path)).Rows
                .Where(one => !string.Equals(one.Model, wanted, StringComparison.Ordinal))
                .ToList();

            foreach (PlotMarker one in markers.All)
            {
                rows.Add(new PlotMarkerRow(wanted, one.PlotId, one.Marker));
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, PlotMarkerFile.Write(rows), new UTF8Encoding(false));

                return string.Empty;
            }
            catch (UnauthorizedAccessException denied)
            {
                return "That marker could not be remembered. " + denied.Message;
            }
            catch (IOException failed)
            {
                return "That marker could not be remembered. " + failed.Message;
            }
        }

        /// <summary>
        /// Beside the title block settings, under the user's own profile, because a marker is
        /// how this team numbers rather than a fact about one machine's install.
        /// </summary>
        public static string UserPath()
        {
            string profile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrEmpty(profile)) return string.Empty;

            return Path.Combine(
                Path.Combine(profile, TitleBlockSettingsStore.UserFolderName), FileName);
        }

        private static string TextIn(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            try
            {
                return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
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

    /// <summary>
    /// What came out of the marker file for one model: the markers, and every line that
    /// could not be read.
    /// </summary>
    internal sealed class StoredMarkers
    {
        public StoredMarkers(PlotMarkers markers, IReadOnlyList<string> notRead)
        {
            Markers = markers;
            NotRead = notRead;
        }

        public PlotMarkers Markers { get; }

        public IReadOnlyList<string> NotRead { get; }
    }
}
