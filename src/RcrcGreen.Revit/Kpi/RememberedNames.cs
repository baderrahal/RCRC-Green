using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RcrcGreen.Revit.Kpi
{
    /// <summary>
    /// The two name boxes, kept between runs beside the installed assembly.
    ///
    /// Neither comes from Revit and neither is worth typing twice a day. They are kept the
    /// same way <see cref="TemplateFolder"/> keeps its folder, so nothing in this project has
    /// to know where the repo is, and the file sits in a folder that never leaves the machine.
    /// A read or a write that fails costs nothing but an empty box.
    /// </summary>
    internal static class RememberedNames
    {
        public const string FileName = "kpi-prepared-by.txt";

        public static string PreparedBy()
        {
            return Line(0);
        }

        public static string Position()
        {
            return Line(1);
        }

        public static void Remember(string preparedBy, string position)
        {
            try
            {
                string path = Beside();
                if (path.Length == 0) return;

                File.WriteAllLines(path, new[] { preparedBy ?? string.Empty, position ?? string.Empty });
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (IOException)
            {
            }
        }

        private static string Line(int which)
        {
            try
            {
                string path = Beside();
                if (path.Length == 0 || !File.Exists(path)) return string.Empty;

                string[] lines = File.ReadAllLines(path);
                return which < lines.Length ? lines[which] : string.Empty;
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

        private static string Beside()
        {
            string assembly = Assembly.GetExecutingAssembly().Location;
            if (string.IsNullOrEmpty(assembly)) return string.Empty;

            string folder = Path.GetDirectoryName(assembly);
            return string.IsNullOrEmpty(folder) ? string.Empty : Path.Combine(folder, FileName);
        }
    }
}
