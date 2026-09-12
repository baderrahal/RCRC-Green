using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RcrcGreen.Core;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// The new view setup file, and nothing about what an answer means.
    ///
    /// One user file beside the other settings and nothing shipped, because the three
    /// answers name things a model holds and no install can know them. Reading and writing
    /// the answers is <see cref="NewViewSetups"/> and <see cref="NewViewSetupFile"/>, with
    /// tests. No Revit API call is made here, so the panel reads and writes it directly.
    /// </summary>
    internal static class NewViewSetupStore
    {
        public const string FileName = "new-view-setups.txt";

        public static StoredNewViewSetups Read()
        {
            NewViewSetupFileContents held = NewViewSetupFile.Read(TextIn(UserPath()));

            var notRead = new List<string>();
            foreach (string line in held.NotRead) notRead.Add("new view setups, " + line);

            return new StoredNewViewSetups(NewViewSetups.Remembered(held.Answers), notRead);
        }

        /// <summary>
        /// Writes every saved answer, and hands back why not when it could not. An answer
        /// that looks saved and is not would have somebody pick the same three again next
        /// week and wonder why it never sticks.
        /// </summary>
        public static string Save(NewViewSetups setups)
        {
            if (setups == null) return string.Empty;

            string path = UserPath();
            if (path.Length == 0)
            {
                return "Those answers could not be remembered, because this machine gave no "
                    + "folder to keep settings in.";
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(
                    path, NewViewSetupFile.Write(setups.All), new UTF8Encoding(false));

                return string.Empty;
            }
            catch (UnauthorizedAccessException denied)
            {
                return "Those answers could not be remembered. " + denied.Message;
            }
            catch (IOException failed)
            {
                return "Those answers could not be remembered. " + failed.Message;
            }
        }

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
    /// What came out of the file: the answers, and every line that could not be read.
    /// </summary>
    internal sealed class StoredNewViewSetups
    {
        public StoredNewViewSetups(NewViewSetups setups, IReadOnlyList<string> notRead)
        {
            Setups = setups;
            NotRead = notRead;
        }

        public NewViewSetups Setups { get; }

        public IReadOnlyList<string> NotRead { get; }
    }
}
