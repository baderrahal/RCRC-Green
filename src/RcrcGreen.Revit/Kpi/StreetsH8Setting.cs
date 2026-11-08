using RcrcGreen.Core.Kpi;

namespace RcrcGreen.Revit.Kpi
{
    /// <summary>
    /// The STREETS H8 option picked in Setup, kept beside the installed assembly through
    /// <see cref="RememberedFolder"/>, so it is remembered per person the way the folder layout
    /// is. No file, an unreadable one, or one reading anything else is Intervention area,
    /// Bader's default. A write that fails is said on the status line.
    /// </summary>
    internal static class StreetsH8Setting
    {
        public const string FileName = "kpi-streets-h8.txt";

        private static readonly RememberedFolder Pointer = new RememberedFolder(FileName);

        public static StreetsH8Option Read()
        {
            return StreetsH8.Parse(Pointer.ReadRaw());
        }

        public static bool Remember(StreetsH8Option option)
        {
            return Pointer.Remember(StreetsH8.Name(option));
        }
    }
}
