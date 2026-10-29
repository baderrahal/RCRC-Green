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
    /// The folder column of <see cref="ComponentTable"/>, and it answers a different question
    /// from the template column. <see cref="ComponentTemplates"/> says which WORKBOOK a plot is filled from
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
    /// **The rows live in components.csv now, read through <see cref="ComponentTable"/>**, one
    /// table with the template column beside the folder, so the two answers keep one key.
    ///
    /// **Since 29 Oct the rules in component-rules.csv answer every value the rows once held**:
    /// five named folders, and the mosque and school rules file each value under a folder named
    /// as the value. The team's snip also holds a GOVERMENT BUILDING folder, spelt that way, and
    /// no rule word reaches it, because no plot in either measured model carries it.
    ///
    /// A value no row and no rule places places nothing and is named, the same rule the template
    /// column follows. Nothing falls back to the template name, because a folder built from a
    /// guess sits beside the team's own and nobody notices for a month.
    /// </summary>
    public static class ComponentFolders
    {
        /// <summary>
        /// Every row in force, value and folder, read off <see cref="ComponentTable"/>.
        /// </summary>
        public static IReadOnlyList<ComponentFolder> All
        {
            get
            {
                return ComponentTable.InForce.Rows
                    .Select(row => new ComponentFolder(row.Component, row.Folder))
                    .ToList();
            }
        }

        /// <summary>
        /// Every distinct named folder the rows and then the rules reach, in the order each first
        /// names it. A rule filing under the value names no folder of its own and adds none.
        /// </summary>
        public static IReadOnlyList<string> Folders
        {
            get
            {
                var seen = new List<string>();
                IEnumerable<string> named = All.Select(one => one.Folder)
                    .Concat(ComponentTable.InForce.Rules.Where(rule => !rule.FolderIsTheValue).Select(rule => rule.Folder));
                foreach (string one in named)
                {
                    if (!seen.Contains(one, StringComparer.Ordinal)) seen.Add(one);
                }

                return seen;
            }
        }

        /// <summary>
        /// The folder for one component value, or empty when nothing places it: its exact row,
        /// else the first rule whose whole words it holds, through
        /// <see cref="ComponentTable.PlaceOf"/>. An exact row is never a prefix: NH STRT 20m ROW
        /// and NH STRT LESS 20m ROW share an opening and are two different values.
        /// </summary>
        public static string For(string component)
        {
            return ComponentTable.InForce.FolderFor(component);
        }

        /// <summary>
        /// Said beside a plot whose component this table does not hold, naming the value so the
        /// next entry can be measured rather than guessed at. **An EMPTY component no longer
        /// reaches this**, because an absence is a different thing from an answer nobody knows,
        /// and it is answered by <see cref="OnlyFolderFor"/> below.
        /// </summary>
        public static string NoFolderFor(string component)
        {
            return "the component folder table does not hold "
                + (string.IsNullOrWhiteSpace(component) ? "an empty component" : component.Trim())
                + ", so nothing says which folder this plot is filed under";
        }

        /// <summary>
        /// Every named folder the rows and rules of one template file under, read off the same
        /// columns that place a value rather than held as a table of its own. Counted off the
        /// shipped files: five templates reach exactly ONE folder each, and MOSQUES, SCHOOLS and
        /// the four 60 per cent templates reach a folder per value, which this list leaves out.
        /// </summary>
        public static IReadOnlyList<string> FoldersOf(KpiTemplate template)
        {
            bool perValue;
            return ComponentTable.InForce.FoldersOf(template, out perValue);
        }

        /// <summary>
        /// The folder a plot with NO COMPONENT AT ALL is filed under, or empty when the table
        /// cannot say.
        ///
        /// **The 09:18 run read six plots and dropped them at the last step.** EP-05, EP-11,
        /// EP-12, EP-13, EP-15 and FM-08 have no sheet, so no PRX_Component, so
        /// <see cref="PlotsPerTemplate"/> placed them by their PLOT PREFIX, exactly as its own
        /// rule says it should. Then this table, keyed on the component, had nothing for them.
        /// Two routes to a template and one to a folder, which is the two records shape where
        /// the two records are the two steps of one decision.
        ///
        /// **Nothing new is written down to fix it.** Where every component of a plot's template
        /// files under ONE folder, that folder is where every plot of that template goes, so a
        /// plot the prefix placed into EXISTING PARKS files where every other EXISTING PARKS
        /// plot files. That is read off the table rather than chosen, and it places five of the
        /// six.
        ///
        /// **Where a template reaches more than one folder, nothing is derived.** MOSQUES files
        /// each value under a folder named as the value and a plot with no component has no value
        /// to name it, so FM-08 is still refused, with a reason that says so instead of naming an
        /// empty component. It is refused BEFORE the press now rather than after its read.
        ///
        /// **This is for an ABSENT component only.** A component the table does not hold still
        /// places nothing, because that is an answer nobody has measured rather than an absence,
        /// and falling back to the template there would file a value the team has never seen
        /// under a folder the team never chose.
        /// </summary>
        public static string OnlyFolderFor(KpiTemplate template)
        {
            string main;
            return OnlyFolderFor(template, out main);
        }

        /// <summary>
        /// The same, with the main folder Grouped files it under, empty for a one level type.
        /// </summary>
        public static string OnlyFolderFor(KpiTemplate template, out string main)
        {
            return ComponentTable.InForce.OnlyFolderFor(template, out main);
        }

        /// <summary>
        /// The main column of both reports: the main folder, one level for a type filed under its
        /// folder alone in both layouts, NONE where nothing placed the value.
        /// </summary>
        public static string MainInWords(ComponentPlacement placed)
        {
            if (placed == null || placed.Folder.Length == 0) return "NONE";
            return placed.Main.Length == 0 ? "one level" : placed.Main;
        }


        /// <summary>
        /// Why a plot with no component of its own cannot be filed under its template's folder,
        /// naming the folders so the answer is checkable against the table.
        /// </summary>
        public static string NoFolderForTemplate(KpiTemplate template)
        {
            if (template == null) return "no template placed this plot, so it has no folder either";

            bool perValue;
            IReadOnlyList<string> folders = ComponentTable.InForce.FoldersOf(template, out perValue);
            if (perValue)
            {
                return template.Name + " files each value under a folder named as the value"
                    + (folders.Count == 0 ? string.Empty : " or under " + string.Join(" and ", folders.ToArray()))
                    + ", and this plot carries no component to say which";
            }

            if (folders.Count == 0)
            {
                return "no component in the folder table fills " + template.Name
                    + ", so nothing says which folder a plot with no component is filed under";
            }

            return template.Name + " files under " + string.Join(" and ", folders.ToArray())
                + ", and this plot carries no component to say which";
        }
    }
}
