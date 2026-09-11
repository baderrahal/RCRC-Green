using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Where a pairing came from. It is shown next to the title block on the panel, because a
    /// value somebody set by hand and a value that came out of the box are different things to
    /// the person deciding whether to trust it.
    /// </summary>
    public enum TitleBlockSource
    {
        Unset = 0,
        ShippedDefaults = 1,
        UserFile = 2
    }

    /// <summary>
    /// One view type and the title block its sheets are made on.
    /// </summary>
    public sealed class TitleBlockPairing
    {
        public TitleBlockPairing(
            ViewType type, string familyName, string typeName, TitleBlockSource source)
        {
            if (type == null) throw new ArgumentNullException("type");

            Type = type;
            FamilyName = familyName ?? string.Empty;
            TypeName = typeName ?? string.Empty;
            Source = source;
        }

        public ViewType Type { get; }

        public string FamilyName { get; }

        public string TypeName { get; }

        public TitleBlockSource Source { get; }

        /// <summary>
        /// The family and the type together, which is how a title block reads everywhere else
        /// on the panel and in the report.
        /// </summary>
        public string TitleBlock
        {
            get { return (FamilyName + " " + TypeName).Trim(); }
        }

        public override string ToString()
        {
            return Type + " on " + TitleBlock;
        }
    }

    /// <summary>
    /// Which title block goes with which view type, so the same eleven choices are not made by
    /// hand on every run.
    ///
    /// Eleven were picked by hand on the run of 2026-09-11 and the same eleven would have been
    /// picked again for every plot after it.
    ///
    /// **Two sources, read in this order, first match wins.** The user's own file, written
    /// whenever they change a pairing, then the defaults shipped beside the add-in so every
    /// installer starts from the same place. Nothing is stored in the model: a pairing is how
    /// this team works rather than a fact about one project, and a model opened by somebody
    /// else should not carry it.
    ///
    /// **A title block the settings name and the model does not hold is not an error.** The
    /// settings are shared across projects and a model is free to have its own blocks. It reads
    /// as unset with the reason, which is what `WhyUnset` says.
    /// </summary>
    public sealed class TitleBlockSettings
    {
        private readonly Dictionary<ViewType, TitleBlockPairing> _byType;

        private TitleBlockSettings(Dictionary<ViewType, TitleBlockPairing> byType)
        {
            _byType = byType;
        }

        public static readonly TitleBlockSettings Nothing =
            new TitleBlockSettings(new Dictionary<ViewType, TitleBlockPairing>());

        /// <summary>
        /// The two files merged. A pairing in the user's file wins over the shipped one for the
        /// same view type, and every pairing carries which file it came from.
        /// </summary>
        public static TitleBlockSettings Of(
            IEnumerable<TitleBlockPairing> user, IEnumerable<TitleBlockPairing> shipped)
        {
            var byType = new Dictionary<ViewType, TitleBlockPairing>();

            foreach (TitleBlockPairing one in (shipped ?? Enumerable.Empty<TitleBlockPairing>())
                .Where(one => one != null))
            {
                byType[one.Type] = Marked(one, TitleBlockSource.ShippedDefaults);
            }

            // Second, so the user's own answer replaces the shipped one for that view type.
            foreach (TitleBlockPairing one in (user ?? Enumerable.Empty<TitleBlockPairing>())
                .Where(one => one != null))
            {
                byType[one.Type] = Marked(one, TitleBlockSource.UserFile);
            }

            return new TitleBlockSettings(byType);
        }

        /// <summary>
        /// The pairing for a view type, or null when neither file names it. Null is a real
        /// answer: the panel shows an empty box and says to pick one, rather than filling in a
        /// title block nobody chose.
        /// </summary>
        public TitleBlockPairing For(ViewType type)
        {
            if (type == null) return null;

            TitleBlockPairing found;
            return _byType.TryGetValue(type, out found) ? found : null;
        }

        /// <summary>
        /// Every pairing, in view type order, which is the order the panel and any report over
        /// them read in.
        /// </summary>
        public IReadOnlyList<TitleBlockPairing> All
        {
            get { return _byType.Values.OrderBy(one => one.Type).ToList(); }
        }

        /// <summary>
        /// What the user's own file should hold after this change. Only their pairings, because
        /// writing the shipped ones into it as well would freeze this version's defaults into
        /// the user's file and the next install could never move them.
        /// </summary>
        public IReadOnlyList<TitleBlockPairing> TheirOwn
        {
            get
            {
                return _byType.Values
                    .Where(one => one.Source == TitleBlockSource.UserFile)
                    .OrderBy(one => one.Type)
                    .ToList();
            }
        }

        /// <summary>
        /// The settings with one pairing set by the user. It comes back as a new object, the way
        /// the grid columns and the plot selection do, so nothing holds a half changed copy.
        ///
        /// Setting the same title block a shipped default already names still marks it theirs,
        /// because they chose it and a later change to the shipped defaults should not move it
        /// under them.
        /// </summary>
        public TitleBlockSettings With(ViewType type, string familyName, string typeName)
        {
            if (type == null) throw new ArgumentNullException("type");

            var byType = new Dictionary<ViewType, TitleBlockPairing>(_byType);
            byType[type] = new TitleBlockPairing(
                type, familyName, typeName, TitleBlockSource.UserFile);

            return new TitleBlockSettings(byType);
        }

        /// <summary>
        /// The settings with a pairing taken out, for a title block the user has cleared.
        /// </summary>
        public TitleBlockSettings Without(ViewType type)
        {
            if (type == null || !_byType.ContainsKey(type)) return this;

            var byType = new Dictionary<ViewType, TitleBlockPairing>(_byType);
            byType.Remove(type);

            return new TitleBlockSettings(byType);
        }

        /// <summary>
        /// Where the title block on a sheet came from, said under the picker. A sheet carries
        /// one title block and its views can disagree about which it should be, so the line
        /// names the disagreement rather than picking a winner, which is the rule this repo
        /// follows everywhere two sources answer one question.
        /// </summary>
        public string WhereItCameFrom(IEnumerable<ViewType> ticked)
        {
            List<ViewType> types = (ticked ?? Enumerable.Empty<ViewType>())
                .Where(one => one != null)
                .ToList();

            if (types.Count == 0) return "No view is ticked for this sheet yet.";

            List<TitleBlockPairing> found = types
                .Select(For)
                .Where(one => one != null)
                .ToList();

            if (found.Count == 0)
            {
                return types.Count == 1
                    ? "The settings hold no title block for " + types[0] + ". Pick one and it is "
                        + "remembered for next time."
                    : "The settings hold no title block for any of these views. Pick one and it "
                        + "is remembered for next time.";
            }

            List<string> distinct = found
                .Select(one => one.TitleBlock)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (distinct.Count > 1)
            {
                return "These views disagree about the title block: "
                    + string.Join(", ", found
                        .Select(one => one.Type + " wants " + one.TitleBlock)
                        .ToArray())
                    + ". Pick the one this sheet should use and it is remembered for all of them.";
            }

            TitleBlockPairing first = found[0];
            string where = first.Source == TitleBlockSource.UserFile
                ? "your own settings"
                : "the shipped defaults";

            string missing = found.Count < types.Count
                ? " " + string.Join(", ", types
                    .Where(one => For(one) == null)
                    .Select(one => one.ToString())
                    .ToArray())
                    + " is in neither file and takes the same one."
                : string.Empty;

            return first.TitleBlock + ", from " + where + "." + missing;
        }

        /// <summary>
        /// Why the picker is empty when the settings did name a title block. The settings are
        /// shared across projects, so a model that does not hold the named block is ordinary
        /// and is said rather than treated as a fault.
        /// </summary>
        public static string WhyUnset(TitleBlockPairing named)
        {
            if (named == null) return string.Empty;

            return "This model holds no title block called " + named.TitleBlock
                + ", which is what the settings name for " + named.Type
                + ". Pick one this model has.";
        }

        private static TitleBlockPairing Marked(TitleBlockPairing one, TitleBlockSource source)
        {
            return new TitleBlockPairing(one.Type, one.FamilyName, one.TypeName, source);
        }
    }
}
