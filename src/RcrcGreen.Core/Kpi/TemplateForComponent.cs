using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Which template the chosen plots point at, and why. A preselection the user can change,
    /// never a guess the tool acts on by itself.
    /// </summary>
    public sealed class TemplateChoice
    {
        private TemplateChoice(KpiTemplate preselected, IReadOnlyList<KpiTemplate> candidates, string why)
        {
            Preselected = preselected;
            Candidates = candidates;
            Why = why ?? string.Empty;
        }

        /// <summary>
        /// Nothing when the component matched no template, matched more than one, or the
        /// chosen plots disagreed. The user picks in every one of those cases.
        /// </summary>
        public KpiTemplate Preselected { get; }

        public IReadOnlyList<KpiTemplate> Candidates { get; }

        public string Why { get; }

        public bool NeedsAPick
        {
            get { return Preselected == null; }
        }

        public static TemplateChoice Preselecting(KpiTemplate template, string why)
        {
            if (template == null) throw new ArgumentNullException("template");

            return new TemplateChoice(template, new[] { template }, why);
        }

        public static TemplateChoice Between(IEnumerable<KpiTemplate> candidates, string why)
        {
            return new TemplateChoice(
                null,
                (candidates ?? Enumerable.Empty<KpiTemplate>()).Where(one => one != null).ToList(),
                why);
        }
    }

    /// <summary>
    /// Reads the component off the chosen plots and preselects the template that answers to it.
    ///
    /// PRX_Component on the sheet is the asset type rather than a park name. Which template each
    /// of its values means is <see cref="ComponentTemplates"/>, a table measured off the 1548
    /// scan, and this reads that table and nothing else.
    ///
    /// It used to match a word of the component against a word of the template name. **That rule
    /// is gone.** It made PARKING LOT look like a park, because PARKING begins with PARK, and it
    /// answered nothing at all for the four street values. A value the table does not hold now
    /// preselects nothing and says so, rather than falling back to a name.
    ///
    /// **EXISTING PARK and FUTURE PARK are separate values**, so the model breaks the park tie
    /// and each preselects its own template. That pair used to be the user's choice always,
    /// because no rule on a name could separate them. The table can.
    /// </summary>
    public static class TemplateForComponent
    {
        public const string NoComponent =
            "No component was read off the chosen plots, so nothing preselects a template.";

        public const string PlotsDisagree = "The chosen plots hold different components";

        public const string NotInTheTable = "is not one of the component values this tool knows";

        public static TemplateChoice For(AgreedValue component, IReadOnlyList<KpiTemplate> templates)
        {
            IReadOnlyList<KpiTemplate> all = templates ?? KpiTemplates.All;

            if (component == null || component.Distinct.Count == 0)
            {
                return TemplateChoice.Between(all, NoComponent);
            }

            if (!component.Agrees)
            {
                return TemplateChoice.Between(all, PlotsDisagree + ": "
                    + string.Join(", ", component.Distinct.ToArray()) + ". Pick the template.");
            }

            string held = component.Value;
            KpiTemplate meant = ComponentTemplates.For(held);

            if (meant == null)
            {
                return TemplateChoice.Between(all, held + " " + NotInTheTable
                    + ". Pick the template. The eleven it knows are in ComponentTemplates, "
                    + "measured off the 1548 scan.");
            }

            // The template the table names may not be among the ones offered, which happens when
            // a caller hands in a shorter list. Preselecting one that is not on offer would show
            // a pick the user cannot see, so the pick goes back to them with the reason.
            if (!all.Any(one => ReferenceEquals(one, meant)))
            {
                return TemplateChoice.Between(all, held + " means " + meant.Name
                    + ", which is not among the templates offered. Pick the template.");
            }

            return TemplateChoice.Preselecting(meant,
                held + " on the chosen plots preselects " + meant.Name + ". Change it if it is wrong.");
        }
    }
}
