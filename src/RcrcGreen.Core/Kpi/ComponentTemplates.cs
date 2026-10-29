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
    /// Which template each value of PRX_Component means, read off <see cref="ComponentTable"/>,
    /// the components.csv files, and no longer held here.
    ///
    /// The first eleven rows were measured on the 1548 scan, 11 distinct values over 1384
    /// sheets, off the component values block at the end of section 3 of that report. That
    /// report is not in this repository, because nothing under reports/ is ever committed. The
    /// four 60 per cent values were added by the team on 28 Oct.
    ///
    /// **It is data and never a match against the template's name.** No rule on a template name
    /// turns HEALTH into HEALTHCARE or NH STRT 20m ROW into STREETS, and the one that used to run
    /// here matched a word of the component against a word of the template name: PARK inside
    /// PARKING made a parking plot look like a park, and every street value matched nothing at
    /// all. Since 29 Oct the words Bader wrote down in component-rules.csv place a value no row
    /// holds, under <see cref="ComponentRules"/>, and those are whole words he chose.
    ///
    /// **It is many to one.** Two values mean MOSQUES and four mean STREETS.
    ///
    /// **The plot prefix decides nothing here.** STREET 36m ROW covers MM and ST plots, and NS
    /// carries two different street widths.
    ///
    /// A value no file holds means no template. Nothing guesses at one and nothing falls back
    /// to matching on a name.
    /// </summary>
    public static class ComponentTemplates
    {
        /// <summary>
        /// Every row in force whose template is known, in the table's order.
        /// </summary>
        public static IReadOnlyList<ComponentTemplate> All
        {
            get
            {
                ComponentTable table = ComponentTable.InForce;
                return table.Rows
                    .Select(row => new { row, template = KpiTemplates.Named(row.Template, table.Templates) })
                    .Where(one => one.template != null)
                    .Select(one => new ComponentTemplate(one.row.Component, one.template))
                    .ToList();
            }
        }

        /// <summary>
        /// The template a value means, or null when no row and no rule places it.
        ///
        /// Through <see cref="ComponentTable.PlaceOf"/>: an exact row compares the whole value
        /// through <see cref="ComponentMatch"/>, and a rule compares whole words, so STREET 30m
        /// ROW reaches STREETS by its word STREET and PARK never finds PARKING LOT.
        /// </summary>
        public static KpiTemplate For(string component)
        {
            return ComponentTable.InForce.TemplateFor(component);
        }

        /// <summary>
        /// Every value an exact row sends to one template, in the order the table lists them. The
        /// rows are exceptions only since the rules, so this names no value a rule places.
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
