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
        /// Every folder the components of one template file under, read off THIS table and
        /// <see cref="ComponentTemplates"/> together rather than held as an eighth table.
        ///
        /// Both are keyed on the same eleven component values, so which folders a template
        /// reaches is already written down twice over and there is nothing here to drift from.
        /// Counted off the two: six templates reach exactly ONE folder each and MOSQUES reaches
        /// TWO, because DAILY MOSQUE and FRIDAY MOSQUE fill one workbook and are filed apart.
        /// </summary>
        public static IReadOnlyList<string> FoldersOf(KpiTemplate template)
        {
            if (template == null) return new List<string>();

            var seen = new List<string>();
            foreach (ComponentFolder one in All)
            {
                if (!ReferenceEquals(ComponentTemplates.For(one.Component), template)) continue;
                if (!seen.Contains(one.Folder, StringComparer.Ordinal)) seen.Add(one.Folder);
            }

            return seen;
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
        /// under DAILY MOSQUE and FRIDAY MOSQUE and a plot with no component cannot say which,
        /// so FM-08 is still refused, with a reason that names both folders instead of naming an
        /// empty component. It is refused BEFORE the press now rather than after its read.
        ///
        /// **This is for an ABSENT component only.** A component the table does not hold still
        /// places nothing, because that is an answer nobody has measured rather than an absence,
        /// and falling back to the template there would file a value the team has never seen
        /// under a folder the team never chose.
        /// </summary>
        public static string OnlyFolderFor(KpiTemplate template)
        {
            IReadOnlyList<string> folders = FoldersOf(template);
            return folders.Count == 1 ? folders[0] : string.Empty;
        }

        /// <summary>
        /// Why a plot with no component of its own cannot be filed under its template's folder,
        /// naming the folders so the answer is checkable against the table.
        /// </summary>
        public static string NoFolderForTemplate(KpiTemplate template)
        {
            if (template == null) return "no template placed this plot, so it has no folder either";

            IReadOnlyList<string> folders = FoldersOf(template);
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
