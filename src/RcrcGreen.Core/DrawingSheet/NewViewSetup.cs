using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Where a new view type's three answers came from, said beside them in step 5.
    /// </summary>
    public enum NewViewSource
    {
        Unset = 0,
        Remembered = 1,
        AnsweredNow = 2
    }

    /// <summary>
    /// The three answers a view type with no example in the model needs before one can be
    /// created: the view family type, the view template and the level. A section takes no
    /// level, which is a fact about how Revit creates sections rather than a default.
    /// </summary>
    public sealed class NewViewAnswers
    {
        public NewViewAnswers(
            ViewType type,
            string familyTypeName,
            string templateName,
            string levelName,
            NewViewSource source)
        {
            if (type == null) throw new ArgumentNullException("type");

            Type = type;
            FamilyTypeName = (familyTypeName ?? string.Empty).Trim();
            TemplateName = (templateName ?? string.Empty).Trim();
            LevelName = (levelName ?? string.Empty).Trim();
            Source = source;
        }

        public ViewType Type { get; }

        public string FamilyTypeName { get; }

        public string TemplateName { get; }

        public string LevelName { get; }

        public NewViewSource Source { get; }
    }

    /// <summary>
    /// Which view family kinds a fresh view can be made under at all, and which of them are
    /// sections. ViewPlan.Create takes the plan kinds and a level, ViewSection.CreateSection
    /// takes the section kind and a box, and nothing here can create the others, so they are
    /// not offered. This is an API capability read off what each call accepts, not a
    /// preference.
    /// </summary>
    public static class NewViewFamilies
    {
        private static readonly string[] PlanKinds =
        {
            "FloorPlan", "CeilingPlan", "AreaPlan", "StructuralPlan"
        };

        public const string SectionKind = "Section";

        public static bool CanBeCreated(string viewFamily)
        {
            return IsASection(viewFamily)
                || PlanKinds.Contains(viewFamily ?? string.Empty, StringComparer.Ordinal);
        }

        public static bool IsASection(string viewFamily)
        {
            return string.Equals(viewFamily, SectionKind, StringComparison.Ordinal);
        }

        /// <summary>
        /// The family types the dropdown offers: the model's own, narrowed to the kinds a
        /// view can be created under.
        /// </summary>
        public static IReadOnlyList<ScannedViewFamilyType> Offerable(
            IEnumerable<ScannedViewFamilyType> inTheModel)
        {
            return (inTheModel ?? Enumerable.Empty<ScannedViewFamilyType>())
                .Where(one => one != null && CanBeCreated(one.ViewFamily))
                .ToList();
        }

        /// <summary>
        /// The kind of a named family type, empty when the model does not hold the name.
        /// </summary>
        public static string KindOf(
            string familyTypeName, IEnumerable<ScannedViewFamilyType> inTheModel)
        {
            ScannedViewFamilyType found = (inTheModel ?? Enumerable.Empty<ScannedViewFamilyType>())
                .FirstOrDefault(one => one != null
                    && string.Equals(one.Name, (familyTypeName ?? string.Empty).Trim(),
                        StringComparison.Ordinal));

            return found == null ? string.Empty : found.ViewFamily;
        }
    }

    /// <summary>
    /// The saved answers for every view type that has no example in the model.
    ///
    /// Step 2 lets the user add a view type the model does not hold, and the run refused
    /// every one, because creation copies the family type, the level and the template from
    /// an existing view of that type and there was none. The three are asked for once, in
    /// the panel, each from a dropdown of what the model holds, and remembered here so a
    /// type answered once is answered for good. Nothing is invented and nothing is
    /// defaulted: a marked type with no sibling and no complete answers is refused, naming
    /// what is missing.
    /// </summary>
    public sealed class NewViewSetups
    {
        private readonly Dictionary<ViewType, NewViewAnswers> _byType;

        private NewViewSetups(Dictionary<ViewType, NewViewAnswers> byType)
        {
            _byType = byType;
        }

        public static readonly NewViewSetups Nothing =
            new NewViewSetups(new Dictionary<ViewType, NewViewAnswers>());

        public static NewViewSetups Remembered(IEnumerable<NewViewAnswers> stored)
        {
            var byType = new Dictionary<ViewType, NewViewAnswers>();

            foreach (NewViewAnswers one in (stored ?? Enumerable.Empty<NewViewAnswers>())
                .Where(one => one != null))
            {
                byType[one.Type] = new NewViewAnswers(
                    one.Type, one.FamilyTypeName, one.TemplateName, one.LevelName,
                    NewViewSource.Remembered);
            }

            return new NewViewSetups(byType);
        }

        public NewViewAnswers For(ViewType type)
        {
            if (type == null) return null;

            NewViewAnswers found;
            return _byType.TryGetValue(type, out found) ? found : null;
        }

        public NewViewSetups With(
            ViewType type, string familyTypeName, string templateName, string levelName)
        {
            if (type == null) throw new ArgumentNullException("type");

            var byType = new Dictionary<ViewType, NewViewAnswers>(_byType);
            byType[type] = new NewViewAnswers(
                type, familyTypeName, templateName, levelName, NewViewSource.AnsweredNow);

            return new NewViewSetups(byType);
        }

        public NewViewSetups Without(ViewType type)
        {
            if (type == null || !_byType.ContainsKey(type)) return this;

            var byType = new Dictionary<ViewType, NewViewAnswers>(_byType);
            byType.Remove(type);

            return new NewViewSetups(byType);
        }

        public IReadOnlyList<NewViewAnswers> All
        {
            get { return _byType.Values.OrderBy(one => one.Type).ToList(); }
        }

        /// <summary>
        /// What the run is still short of for one type, empty when it has all it needs. The
        /// level is only needed for a plan kind, because a section is cut from a box and
        /// sits on no level.
        /// </summary>
        public string Missing(ViewType type, bool familyIsASection)
        {
            NewViewAnswers found = For(type);

            var missing = new List<string>();
            if (found == null || found.FamilyTypeName.Length == 0)
            {
                missing.Add("the view family type");
            }

            if (found == null || found.TemplateName.Length == 0)
            {
                missing.Add("the view template");
            }

            if (!familyIsASection && (found == null || found.LevelName.Length == 0))
            {
                missing.Add("the level");
            }

            if (missing.Count == 0) return string.Empty;
            if (missing.Count == 1) return missing[0];

            return string.Join(", ", missing.Take(missing.Count - 1).ToArray())
                + " and " + missing[missing.Count - 1];
        }

        /// <summary>
        /// Where one type's answers came from, for the line beside the dropdowns.
        /// </summary>
        public string WordsFor(ViewType type)
        {
            NewViewAnswers found = For(type);
            if (found == null) return "Not answered yet.";

            return found.Source == NewViewSource.AnsweredNow
                ? "Answered now."
                : "Remembered from last time.";
        }

        /// <summary>
        /// Which of the given types create as sections, read off the saved family type's
        /// kind. The panel's preview and the run both ask this one method, so the plan on
        /// screen and the plan that writes can never route a type two ways.
        /// </summary>
        public IReadOnlyList<ViewType> SectionTypesAmong(
            IEnumerable<ViewType> types, IEnumerable<ScannedViewFamilyType> familiesInTheModel)
        {
            var families = (familiesInTheModel ?? Enumerable.Empty<ScannedViewFamilyType>())
                .Where(one => one != null)
                .ToList();

            return (types ?? Enumerable.Empty<ViewType>())
                .Where(one => one != null)
                .Where(one =>
                {
                    NewViewAnswers found = For(one);
                    return found != null && NewViewFamilies.IsASection(
                        NewViewFamilies.KindOf(found.FamilyTypeName, families));
                })
                .ToList();
        }
    }
}
