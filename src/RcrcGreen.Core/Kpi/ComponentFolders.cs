using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One value of PRX_Component and the folder the team files that plot's checklist under.
    /// </summary>
    public sealed class ComponentFolder
    {
        public ComponentFolder(string component, string folder)
        {
            if (string.IsNullOrWhiteSpace(component)) throw new ArgumentNullException("component");
            if (string.IsNullOrWhiteSpace(folder)) throw new ArgumentNullException("folder");

            Component = component;
            Folder = folder;
        }

        public string Component { get; }

        /// <summary>
        /// The folder name exactly as the team spells it. SCHOOL is singular, FUTURE PARKS is
        /// plural and EXISTING PARK is singular, which is not a pattern and is why this is a
        /// table. One wrong letter makes a second folder beside the team's.
        /// </summary>
        public string Folder { get; }
    }

    /// <summary>
    /// The third table read off PRX_Component, and it answers a different question from the
    /// other two. <see cref="ComponentTemplates"/> says which WORKBOOK a plot is filled from
    /// and this says which FOLDER the filled workbook is filed in. They are not the same answer
    /// and neither derives from the other:
    ///
    /// <code>
    /// DAILY MOSQUE      template MOSQUES    folder DAILY MOSQUE
    /// FRIDAY MOSQUE     template MOSQUES    folder FRIDAY MOSQUE
    /// HEALTH            template HEALTHCARE folder HEALTHCARE
    /// PARKING LOT       template PARKING    folder PARKING LOT
    /// </code>
    ///
    /// The two mosque values share one template and get two folders, and PARKING LOT and HEALTH
    /// share neither spelling with the template they fill from. So no string rule turns a
    /// template name into a folder name any more than one turns a component value into a
    /// template name, and this is data measured against the team's own folders.
    ///
    /// **Eleven values. The table as Bader wrote it reaches EIGHT distinct folders**, counted
    /// off the entries below rather than off the round message, which said nine. The team's snip
    /// also holds a GOVERMENT BUILDING folder, spelt that way, and it is deliberately not in
    /// here because no plot in either measured model carries a component for it. Whether that is
    /// the ninth is for the team.
    ///
    /// A value this table does not hold places nothing and is named, the same rule the template
    /// table follows. Nothing falls back to the template name, because a folder built from a
    /// guess sits beside the team's own and nobody notices for a month.
    /// </summary>
    public static class ComponentFolders
    {
        public static readonly IReadOnlyList<ComponentFolder> All = new[]
        {
            new ComponentFolder("DAILY MOSQUE", "DAILY MOSQUE"),
            new ComponentFolder("FRIDAY MOSQUE", "FRIDAY MOSQUE"),
            new ComponentFolder("SCHOOL", "SCHOOL"),
            new ComponentFolder("HEALTH", "HEALTHCARE"),
            new ComponentFolder("PARKING LOT", "PARKING LOT"),
            new ComponentFolder("EXISTING PARK", "EXISTING PARK"),
            new ComponentFolder("FUTURE PARK", "FUTURE PARKS"),
            new ComponentFolder("NH STRT 20m ROW", "STREETS"),
            new ComponentFolder("NH STRT LESS 20m ROW", "STREETS"),
            new ComponentFolder("STREET 30m ROW", "STREETS"),
            new ComponentFolder("STREET 36m ROW", "STREETS")
        };

        /// <summary>
        /// Every distinct folder the table reaches, in the order the table first names each.
        /// </summary>
        public static IReadOnlyList<string> Folders
        {
            get
            {
                var seen = new List<string>();
                foreach (ComponentFolder one in All)
                {
                    if (!seen.Contains(one.Folder, StringComparer.Ordinal)) seen.Add(one.Folder);
                }

                return seen;
            }
        }

        /// <summary>
        /// The folder for one component value, or empty when the table does not hold it. The
        /// comparison is the whole value without case and with edge whitespace off, never a
        /// prefix and never a word of it: NH STRT 20m ROW and NH STRT LESS 20m ROW share an
        /// opening and are two different values.
        /// </summary>
        public static string For(string component)
        {
            if (string.IsNullOrWhiteSpace(component)) return string.Empty;

            string held = component.Trim();
            ComponentFolder found = All.FirstOrDefault(
                one => string.Equals(one.Component, held, StringComparison.OrdinalIgnoreCase));

            return found == null ? string.Empty : found.Folder;
        }

        /// <summary>
        /// Said beside a plot whose component this table does not hold, naming the value so the
        /// next entry can be measured rather than guessed at.
        /// </summary>
        public static string NoFolderFor(string component)
        {
            return "the component folder table does not hold "
                + (string.IsNullOrWhiteSpace(component) ? "an empty component" : component.Trim())
                + ", so nothing says which folder this plot is filed under";
        }
    }
}
