using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One measured value of PRX_Component and the template it means.
    /// </summary>
    public sealed class ComponentTemplate
    {
        public ComponentTemplate(string component, KpiTemplate template)
        {
            if (string.IsNullOrWhiteSpace(component)) throw new ArgumentNullException("component");
            if (template == null) throw new ArgumentNullException("template");

            Component = component;
            Template = template;
        }

        /// <summary>
        /// The value as the model prints it, copied off the scan.
        /// </summary>
        public string Component { get; }

        public KpiTemplate Template { get; }
    }

    /// <summary>
    /// The table: which template each value of PRX_Component means.
    ///
    /// Measured on the 1548 scan, 11 distinct values over 1384 sheets, off the component values
    /// block at the end of section 3 of that report. That report is not in this repository,
    /// because nothing under reports/ is ever committed.
    ///
    /// **It is a table and never a string rule.** No string rule turns HEALTH into HEALTHCARE or
    /// NH STRT 20m ROW into STREETS, and the one that used to run here matched a word of the
    /// component against a word of the template name: PARK inside PARKING made a parking plot
    /// look like a park, and every street value matched nothing at all.
    ///
    /// **It is many to one.** Two values mean MOSQUES and four mean STREETS.
    ///
    /// **The plot prefix decides nothing and is read nowhere.** STREET 36m ROW covers MM and ST
    /// plots, and NS carries two different street widths.
    ///
    /// A value this table does not hold means no template. Nothing guesses at one and nothing
    /// falls back to matching on a name.
    /// </summary>
    public static class ComponentTemplates
    {
        public static readonly IReadOnlyList<ComponentTemplate> All = new[]
        {
            new ComponentTemplate("DAILY MOSQUE", KpiTemplates.Mosques),
            new ComponentTemplate("FRIDAY MOSQUE", KpiTemplates.Mosques),
            new ComponentTemplate("SCHOOL", KpiTemplates.Schools),
            new ComponentTemplate("HEALTH", KpiTemplates.Healthcare),
            new ComponentTemplate("PARKING LOT", KpiTemplates.Parking),
            new ComponentTemplate("EXISTING PARK", KpiTemplates.ExistingParks),
            new ComponentTemplate("FUTURE PARK", KpiTemplates.FutureParks),
            new ComponentTemplate("NH STRT LESS 20m ROW", KpiTemplates.Streets),
            new ComponentTemplate("NH STRT 20m ROW", KpiTemplates.Streets),
            new ComponentTemplate("STREET 30m ROW", KpiTemplates.Streets),
            new ComponentTemplate("STREET 36m ROW", KpiTemplates.Streets)
        };

        /// <summary>
        /// The template a value means, or null when the table does not hold it.
        ///
        /// The whole value is compared, without case and with surrounding whitespace off, and
        /// nothing else. That is the same plainness species matching keeps: no part of a value
        /// matches, so PARK never finds PARKING LOT and STREET never finds STREET 30m ROW.
        /// </summary>
        public static KpiTemplate For(string component)
        {
            if (string.IsNullOrWhiteSpace(component)) return null;

            string held = component.Trim();

            ComponentTemplate found = All.FirstOrDefault(
                one => string.Equals(one.Component, held, StringComparison.OrdinalIgnoreCase));

            return found == null ? null : found.Template;
        }

        /// <summary>
        /// Every value that means one template, in the order the table lists them, so the report
        /// and the pane can say which values a template answers to rather than only its name.
        /// </summary>
        public static IReadOnlyList<string> ValuesFor(KpiTemplate template)
        {
            if (template == null) return new List<string>();

            return All
                .Where(one => ReferenceEquals(one.Template, template))
                .Select(one => one.Component)
                .ToList();
        }
    }
}
