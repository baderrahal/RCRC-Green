using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using RcrcGreen.Core;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// The two sheet name files, and nothing about what a name means.
    ///
    /// The same two homes as the title block settings: the user's own file under their
    /// profile, then the defaults shipped beside the add-in, first match winning. Reading,
    /// merging and writing the pairings is <see cref="SheetNameSettings"/> and
    /// <see cref="SheetNameFile"/>, with tests. No Revit API call is made here, so the panel
    /// reads and writes it directly rather than through the external event.
    /// </summary>
    internal static class SheetNameStore
    {
        public const string FileName = "sheet-names.txt";

        public static StoredSheetNames Read()
        {
            SheetNameFileContents shipped = SheetNameFile.Read(TextIn(ShippedPath()));
            SheetNameFileContents user = SheetNameFile.Read(TextIn(UserPath()));

            var notRead = new List<string>();
            foreach (string line in shipped.NotRead) notRead.Add("shipped sheet names, " + line);
            foreach (string line in user.NotRead) notRead.Add("your own sheet names, " + line);

            return new StoredSheetNames(
                SheetNameSettings.Of(user.Pairings, shipped.Pairings), notRead);
        }

        /// <summary>
        /// Writes the user's own pairings, and hands back why not when it could not. A name
        /// that looks saved and is not would have somebody type the same correction again
        /// next week and wonder why it never sticks.
        /// </summary>
        public static string Save(SheetNameSettings settings)
        {
            if (settings == null) return string.Empty;

            string path = UserPath();
            if (path.Length == 0)
            {
                return "That sheet name could not be remembered, because this machine gave no "
                    + "folder to keep settings in.";
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(
                    path,
                    SheetNameFile.Write(settings.TheirOwn),
                    new UTF8Encoding(false));

                return string.Empty;
            }
            catch (UnauthorizedAccessException denied)
            {
                return "That sheet name could not be remembered. " + denied.Message;
            }
            catch (IOException failed)
            {
                return "That sheet name could not be remembered. " + failed.Message;
            }
        }

        public static string UserPath()
        {
            string profile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrEmpty(profile)) return string.Empty;

            return Path.Combine(
                Path.Combine(profile, TitleBlockSettingsStore.UserFolderName), FileName);
        }

        public static string ShippedPath()
        {
            try
            {
                string beside = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return string.IsNullOrEmpty(beside) ? string.Empty : Path.Combine(beside, FileName);
            }
            catch (NotSupportedException)
            {
                return string.Empty;
            }
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
    /// What came out of the two files: the pairings, and every line that could not be read.
    /// </summary>
    internal sealed class StoredSheetNames
    {
        public StoredSheetNames(SheetNameSettings settings, IReadOnlyList<string> notRead)
        {
            Settings = settings;
            NotRead = notRead;
        }

        public SheetNameSettings Settings { get; }

        public IReadOnlyList<string> NotRead { get; }
    }
}
