using System;
using System.IO;
using System.Reflection;

namespace RcrcGreen.Revit.Kpi
{
    /// <summary>
    /// A folder the user browsed for, kept in a file beside the installed assembly. Revit runs
    /// the add-in out of the Autodesk Addins folder and has no other way to know, which is the
    /// same reason reports-folder.txt exists.
    ///
    /// Two folders are remembered this way, the templates and the output, and this is the one
    /// piece of code that reads or writes either. Two copies of a read that has to fail quietly
    /// is two places for one of them to start failing loudly.
    /// </summary>
    internal sealed class RememberedFolder
    {
        public RememberedFolder(string pointerFileName)
        {
            if (string.IsNullOrWhiteSpace(pointerFileName))
            {
                throw new ArgumentNullException("pointerFileName");
            }

            PointerFileName = pointerFileName;
        }

        public string PointerFileName { get; }

        /// <summary>
        /// The remembered folder, or empty when none is set, the pointer is missing or the
        /// folder it names is gone. An empty answer shows the set-the-folder line rather than
        /// a path that is not there any more.
        /// </summary>
        public string Read()
        {
            try
            {
                string pointer = PointerPath();
                if (pointer == null || !File.Exists(pointer)) return string.Empty;

                string folder = File.ReadAllText(pointer).Trim();
                return folder.Length > 0 && Directory.Exists(folder) ? folder : string.Empty;
            }
            catch (IOException)
            {
                return string.Empty;
            }
            catch (UnauthorizedAccessException)
            {
                return string.Empty;
            }
        }

        public bool Remember(string folder)
        {
            try
            {
                string pointer = PointerPath();
                if (pointer == null) return false;

                File.WriteAllText(pointer, folder ?? string.Empty);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        private string PointerPath()
        {
            string beside = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return string.IsNullOrEmpty(beside) ? null : Path.Combine(beside, PointerFileName);
        }
    }

    /// <summary>
    /// Where the filled workbooks are written.
    ///
    /// **It used to be the model's own folder and that was the fault.** Writing beside the Revit
    /// model meant a detached model could not be used at all, and a detached model is what the
    /// team works on. Create asks for this folder now and never asks whether the model has been
    /// saved. Nothing defaults it to the model's folder: a folder nobody chose is a file written
    /// somewhere nobody looked.
    /// </summary>
    internal static class OutputFolder
    {
        public const string PointerFileName = "kpi-output-folder.txt";

        private static readonly RememberedFolder Pointer = new RememberedFolder(PointerFileName);

        public static string Read()
        {
            return Pointer.Read();
        }

        public static bool Remember(string folder)
        {
            return Pointer.Remember(folder);
        }
    }
}
