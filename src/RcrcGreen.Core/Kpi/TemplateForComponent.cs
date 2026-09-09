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
    /// PRX_Component on the sheet is the asset type rather than a park name, measured as
    /// FRIDAY MOSQUE and SCHOOL on 1384 of 1385 sheets, and the plot prefixes agree. A word of
    /// the component held by a template's name is what matches, so MOSQUE finds MOSQUES and
    /// SCHOOL finds SCHOOLS.
    ///
    /// EXISTING PARKS and FUTURE PARKS both answer to a park, so that pair is always the user's
    /// choice and nothing here ever breaks the tie between them.
    /// </summary>
    public static class TemplateForComponent
    {
        /// <summary>
        /// A word shorter than this matches too much. THE and OF hold nothing worth matching.
        /// </summary>
        public const int ShortestWord = 3;

        public const string NoComponent =
            "No component was read off the chosen plots, so nothing preselects a template.";

        public const string PlotsDisagree = "The chosen plots hold different components";

        public const string NothingMatches = "No template answers to";

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
            List<KpiTemplate> matching = all.Where(one => Answers(one, held)).ToList();

            if (matching.Count == 0)
            {
                return TemplateChoice.Between(all, NothingMatches + " " + held + ". Pick the template.");
            }

            if (matching.Count > 1)
            {
                return TemplateChoice.Between(matching, held + " answers to "
                    + string.Join(" and ", matching.Select(one => one.Name).ToArray())
                    + ". Pick the template.");
            }

            return TemplateChoice.Preselecting(matching[0],
                held + " on the chosen plots preselects " + matching[0].Name + ". Change it if it is wrong.");
        }

        private static bool Answers(KpiTemplate template, string component)
        {
            return WordsIn(component).Any(word => KpiNames.Holds(template.Name, word));
        }

        private static IEnumerable<string> WordsIn(string component)
        {
            var word = new List<char>();

            foreach (char letter in component + " ")
            {
                if (char.IsLetter(letter))
                {
                    word.Add(letter);
                    continue;
                }

                if (word.Count >= ShortestWord) yield return new string(word.ToArray());
                word.Clear();
            }
        }
    }
}
