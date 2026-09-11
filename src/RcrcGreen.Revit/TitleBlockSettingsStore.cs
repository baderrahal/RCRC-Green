using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using RcrcGreen.Core;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// The two title block settings files, and nothing about what they mean.
    ///
    /// Where the files sit is the one part of this that belongs to the machine rather than to
    /// the rule, so it is the only part on this side. Reading, merging and writing the pairings
    /// is <see cref="TitleBlockSettings"/> and <see cref="TitleBlockSettingsFile"/>, with tests.
    ///
    /// No Revit API call is made here, so the panel reads and writes it directly rather than
    /// through the external event. A file beside the assembly is not a document.
    /// </summary>
    internal static class TitleBlockSettingsStore
    {
        public const string FileName = "title-blocks.txt";

        /// <summary>
        /// The folder under the user's own profile. It is not beside the assembly, because an
        /// install replaces what is there and somebody's own choices must outlive one.
        /// </summary>
        public const string UserFolderName = "RCRC Green";

        /// <summary>
        /// Both files, merged, with every line neither could read.
        /// </summary>
        public static StoredSettings Read()
        {
            TitleBlockFileContents shipped = TitleBlockSettingsFile.Read(TextIn(ShippedPath()));
            TitleBlockFileContents user = TitleBlockSettingsFile.Read(TextIn(UserPath()));

            var notRead = new List<string>();
            foreach (string line in shipped.NotRead) notRead.Add("shipped defaults, " + line);
            foreach (string line in user.NotRead) notRead.Add("your own settings, " + line);

            return new StoredSettings(
                TitleBlockSettings.Of(user.Pairings, shipped.Pairings), notRead);
        }

        /// <summary>
        /// Writes the user's own pairings, and hands back why not when it could not. A setting
        /// that looks saved and is not would have somebody pick the same title block again next
        /// week and wonder why it never sticks.
        /// </summary>
        public static string Save(TitleBlockSettings settings)
        {
            if (settings == null) return string.Empty;

            string path = UserPath();
            if (path.Length == 0)
            {
                return "That title block could not be remembered, because this machine gave no "
                    + "folder to keep settings in.";
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(
                    path,
                    TitleBlockSettingsFile.Write(settings.TheirOwn),
                    new UTF8Encoding(false));

                return string.Empty;
            }
            catch (UnauthorizedAccessException denied)
            {
                return "That title block could not be remembered. " + denied.Message;
            }
            catch (IOException failed)
            {
                return "That title block could not be remembered. " + failed.Message;
            }
        }

        public static string UserPath()
        {
            string profile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrEmpty(profile)) return string.Empty;

            return Path.Combine(Path.Combine(profile, UserFolderName), FileName);
        }

        /// <summary>
        /// Beside the installed assembly, where install.ps1 puts it, the same way the reports
        /// folder pointer is found.
        /// </summary>
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

        /// <summary>
        /// A file that is not there is not a fault. Neither file exists on a first run, and the
        /// panel simply has no pairing to offer until one does.
        /// </summary>
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
    internal sealed class StoredSettings
    {
        public StoredSettings(TitleBlockSettings settings, IReadOnlyList<string> notRead)
        {
            Settings = settings;
            NotRead = notRead;
        }

        public TitleBlockSettings Settings { get; }

        public IReadOnlyList<string> NotRead { get; }
    }
}
