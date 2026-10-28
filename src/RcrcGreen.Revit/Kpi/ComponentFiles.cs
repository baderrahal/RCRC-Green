using System;
using System.IO;
using RcrcGreen.Core.Kpi;

namespace RcrcGreen.Revit.Kpi
{
    /// <summary>
    /// Finds the three components.csv files on this machine and puts their table in force. Core
    /// reads and layers them, this only says where each one is: beside the installed assembly,
    /// in %APPDATA%\RCRC Green, and in the templates folder the person set in Setup.
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
        /// Reads the three, lowest standing first, puts the table in force and hands it back.
        /// </summary>
        public static ComponentTable PutInForce()
        {
            ComponentTable table = ComponentTable.Layered(
                ComponentFile.From(ComponentSource.Shipped, ComponentTable.ShippedPath),
                ComponentFile.From(ComponentSource.Personal, PersonalPath()),
                ComponentFile.From(ComponentSource.Shared, SharedPath(TemplateFolder.Read())));

            ComponentTable.Use(table);
            return table;
        }
    }
}
