using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using RcrcGreen.Core;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// The two preset files, and nothing about what they mean.
    ///
    /// Where the files sit is the one part of this that belongs to the machine rather than to
    /// the rule, so it is the only part on this side. Reading, merging and writing the presets
    /// is <see cref="Presets"/> and <see cref="PresetFile"/>, with tests.
    ///
    /// No Revit API call is made here, so the panel reads and writes it directly rather than
    /// through the external event. A file beside the assembly is not a document.
    /// </summary>
    internal static class PresetStore
    {
        public const string FileName = "presets.txt";

        /// <summary>
        /// The folder under the user's own profile, shared with the title block settings. It is
        /// not beside the assembly, because an install replaces what is there and somebody's
        /// own presets must outlive one.
        /// </summary>
        public const string UserFolderName = "RCRC Green";

        /// <summary>
        /// Both files, merged, with every line neither could read.
        /// </summary>
        public static StoredPresets Read()
        {
            PresetFileContents shipped = PresetFile.Read(TextIn(ShippedPath()));
            PresetFileContents user = PresetFile.Read(TextIn(UserPath()));

            var notRead = new List<string>();
            foreach (string line in shipped.NotRead) notRead.Add("shipped presets, " + line);
            foreach (string line in user.NotRead) notRead.Add("your own presets, " + line);

            return new StoredPresets(
                Presets.Of(user.Presets, shipped.Presets), shipped.Presets, notRead);
        }

        /// <summary>
        /// Writes the user's own presets, and hands back why not when it could not. A preset
        /// that looks saved and is not would have somebody describe the same six sheets again
        /// next week and wonder why it never sticks.
        /// </summary>
        public static string Save(Presets presets)
        {
            if (presets == null) return string.Empty;

            string path = UserPath();
            if (path.Length == 0)
            {
                return "That preset could not be saved, because this machine gave no folder to "
                    + "keep settings in.";
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(
                    path, PresetFile.Write(presets.TheirOwn), new UTF8Encoding(false));

                return string.Empty;
            }
            catch (UnauthorizedAccessException denied)
            {
                return "That preset could not be saved. " + denied.Message;
            }
            catch (IOException failed)
            {
                return "That preset could not be saved. " + failed.Message;
            }
        }

        public static string UserPath()
        {
            string profile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrEmpty(profile)) return string.Empty;

            return Path.Combine(Path.Combine(profile, UserFolderName), FileName);
        }

        /// <summary>
        /// Beside the installed assembly, where install.ps1 puts it, the same way the title
        /// block settings are found.
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
        /// panel simply has no preset to offer until one does.
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
    /// What came out of the two files: the presets, the shipped ones on their own, and every
    /// line that could not be read.
    ///
    /// The shipped ones are kept apart because a delete takes the user's copy away and the
    /// shipped one of that name comes back, and working that out needs both lists.
    /// </summary>
    internal sealed class StoredPresets
    {
        public StoredPresets(
            Presets presets, IReadOnlyList<Preset> shipped, IReadOnlyList<string> notRead)
        {
            Presets = presets;
            Shipped = shipped;
            NotRead = notRead;
        }

        public Presets Presets { get; }

        public IReadOnlyList<Preset> Shipped { get; }

        public IReadOnlyList<string> NotRead { get; }
    }
}
