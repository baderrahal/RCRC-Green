using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// What matched when a view template was looked for, and what else it could have been.
    /// </summary>
    public sealed class TemplateMatch
    {
        internal TemplateMatch(string name, IReadOnlyList<string> candidates)
        {
            Name = name ?? string.Empty;
            Candidates = candidates;
        }

        /// <summary>
        /// The one template to apply, empty when there was not exactly one.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Every template name that started with the view type, in order. One entry when the
        /// match was clean, none when nothing matched, several when the answer is ambiguous.
        /// </summary>
        public IReadOnlyList<string> Candidates { get; }

        public bool Found
        {
            get { return Name.Length > 0; }
        }

        public bool Ambiguous
        {
            get { return Candidates.Count > 1; }
        }
    }

    /// <summary>
    /// The naming that ties a view type to the Revit objects set up for it.
    ///
    /// Read off DM-18-(200) General Arrangement Layout in the real model. Its view family type
    /// is named "(200) General Arrangement Layout", exactly the view type, and its view
    /// template is "(200) General Arrangement Layout SC - Scale 250", the view type followed by
    /// how it is drawn. Both were guesses before that, and both guesses were wrong.
    /// </summary>
    public static class ViewTypeNaming
    {
        /// <summary>
        /// The view family type a view of this type is made with. Exact, not a prefix. Making a
        /// view with the wrong family type is worse than not making it, so there is no fallback
        /// to a generic floor plan type.
        /// </summary>
        public static string FamilyTypeNameFor(ViewType type)
        {
            if (type == null) throw new ArgumentNullException("type");

            return type.ToString();
        }

        /// <summary>
        /// The view template for this view type, out of the names the model holds.
        ///
        /// The template carries the scale, the detail level, the discipline, the visibility
        /// overrides and the phase filter, so setting it is how the rest follows. A template
        /// name starts with the view type and then says how it is drawn, which is why this
        /// matches on a prefix rather than the whole name.
        ///
        /// Several matches is not an answer. Scale 250 and Scale 500 are both real templates
        /// for one view type and picking either would be inventing a rule, so all of them come
        /// back as candidates and the caller reports them.
        /// </summary>
        public static TemplateMatch TemplateFor(ViewType type, IEnumerable<string> templateNames)
        {
            if (type == null) throw new ArgumentNullException("type");

            string wanted = type.ToString();

            List<string> matching = (templateNames ?? Enumerable.Empty<string>())
                .Where(name => !string.IsNullOrEmpty(name))
                .Where(name => name.StartsWith(wanted, StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToList();

            return new TemplateMatch(matching.Count == 1 ? matching[0] : string.Empty, matching);
        }
    }
}
