using RcrcGreen.Core.Kpi;

namespace RcrcGreen.Revit.Kpi
{
    /// <summary>
    /// The folder layout picked in Setup, Flat or Grouped, kept beside the installed assembly
    /// through <see cref="RememberedFolder"/>, the one piece of code that reads and writes those
    /// files, so it is remembered per person the way the folders are. No file, an unreadable
    /// one, or one reading anything else is no layout picked, never a guess, Bader's decision of
    /// 29 October. A write that fails is said on the status line.
    /// </summary>
    internal static class FolderLayoutSetting
    {
        public const string FileName = "kpi-folder-layout.txt";

        private static readonly RememberedFolder Pointer = new RememberedFolder(FileName);

        public static FolderLayout Read()
        {
            return FolderLayouts.Parse(Pointer.ReadRaw());
        }

        public static bool Remember(FolderLayout layout)
        {
            return Pointer.Remember(FolderLayouts.Name(layout));
        }
    }
}
