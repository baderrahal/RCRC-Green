using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RcrcGreen.Core.Kpi;

namespace RcrcGreen.Revit.Kpi
{
    /// <summary>
    /// Finds the three components.csv files on this machine, puts their table in force and adds
    /// the one line a person takes in the pane. Core reads, layers and words them, and this only
    /// says where each one is: beside the installed assembly, in %APPDATA%\RCRC Green, and in the
    /// templates folder the person set in Setup.
    ///
    /// It reads afresh every time it is asked, because the shared file is the team's and can
    /// change under an open pane, and a copy held here would be the second record of it.
    /// </summary>
    internal static class ComponentFiles
    {
        public const string UserFolderName = "RCRC Green";

        public static string SharedPath(string templatesFolder)
        {
            return string.IsNullOrWhiteSpace(templatesFolder)
                ? string.Empty
                : Path.Combine(templatesFolder, ComponentTable.FileName);
        }

        public static string PersonalPath()
        {
            string profile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrEmpty(profile)) return string.Empty;

            return Path.Combine(Path.Combine(profile, UserFolderName), ComponentTable.FileName);
        }

        /// <summary>
        /// Reads the three, lowest standing first, over the known templates and any made for a
        /// new file in the templates folder, puts the table in force and hands it back.
        /// </summary>
        public static ComponentTable PutInForce(IEnumerable<KpiTemplate> made = null)
        {
            List<KpiTemplate> templates = KpiTemplates.All
                .Concat((made ?? Enumerable.Empty<KpiTemplate>()).Where(one => one != null && !KpiTemplates.All.Contains(one)))
                .Distinct()
                .ToList();

            ComponentTable table = ComponentTable.Layered(
                templates,
                ComponentFile.From(ComponentSource.Shipped, ComponentTable.ShippedPath),
                ComponentFile.From(ComponentSource.Personal, PersonalPath()),
                ComponentFile.From(ComponentSource.Shared, SharedPath(TemplateFolder.Read())));

            ComponentTable.Use(table);
            return table;
        }

        /// <summary>
        /// Adds one row, to the shared file where the templates folder is set and takes the
        /// write, else to the person's own. Hands back the path written, or empty with why.
        /// Nothing already in either file is rewritten.
        /// </summary>
        public static string Add(string component, string template, string folder, out string why)
        {
            string line = ComponentTable.Line(component, template, folder);
            var tried = new List<string>();

            foreach (string path in new[] { SharedPath(TemplateFolder.Read()), PersonalPath() })
            {
                if (path.Length == 0) continue;

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    string existing = File.Exists(path) ? File.ReadAllText(path) : null;
                    File.AppendAllText(path, ComponentTable.TextToAppend(existing, line), new UTF8Encoding(false));
                    why = string.Empty;
                    return path;
                }
                catch (IOException failed)
                {
                    tried.Add(path + ": " + failed.Message);
                }
                catch (UnauthorizedAccessException failed)
                {
                    tried.Add(path + ": " + failed.Message);
                }
                catch (ArgumentException failed)
                {
                    // A hand edited templates pointer can hold a character no path takes.
                    tried.Add(path + ": " + failed.Message);
                }
                catch (NotSupportedException failed)
                {
                    tried.Add(path + ": " + failed.Message);
                }
            }

            why = tried.Count == 0 ? "No folder to write a components.csv in." : string.Join(" ", tried.ToArray());
            return string.Empty;
        }
    }
}
