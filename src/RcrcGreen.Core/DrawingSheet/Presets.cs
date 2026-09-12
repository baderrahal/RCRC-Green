using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Every saved preset, out of the two files.
    ///
    /// **Two sources, read in this order, first match wins**, the same shape the title block
    /// settings and the sheet names already follow. The user's own file, written whenever they
    /// save one, then the presets shipped beside the add-in so every installer starts from the
    /// same place. Nothing is stored in the model: a preset is how this team works rather than
    /// a fact about one project.
    ///
    /// The name is the key. A preset the user saves under a shipped one's name replaces it for
    /// them and leaves the shipped file alone, so the next install can still move the shipped
    /// one under anybody who has not overridden it.
    /// </summary>
    public sealed class Presets
    {
        private readonly List<Preset> _inOrder;

        private Presets(List<Preset> inOrder)
        {
            _inOrder = inOrder;
        }

        public static readonly Presets Nothing = new Presets(new List<Preset>());

        /// <summary>
        /// The two files merged. A preset in the user's file wins over a shipped one of the same
        /// name, and every preset carries which file it came from.
        /// </summary>
        public static Presets Of(IEnumerable<Preset> user, IEnumerable<Preset> shipped)
        {
            var byName = new Dictionary<string, Preset>(StringComparer.OrdinalIgnoreCase);
            var order = new List<string>();

            foreach (Preset one in OnlyNamed(shipped))
            {
                Keep(byName, order, Marked(one, PresetSource.ShippedDefaults));
            }

            // Second, so the user's own answer replaces the shipped one of that name.
            foreach (Preset one in OnlyNamed(user))
            {
                Keep(byName, order, Marked(one, PresetSource.UserFile));
            }

            return new Presets(order.Select(name => byName[name]).ToList());
        }

        /// <summary>
        /// Every preset, shipped ones first in their file order and then the user's own, which
        /// is the order they were saved in. Alphabetical would move a preset under somebody
        /// between one week and the next.
        /// </summary>
        public IReadOnlyList<Preset> All
        {
            get { return _inOrder; }
        }

        public int Count
        {
            get { return _inOrder.Count; }
        }

        /// <summary>
        /// The preset of that name, or null when none is saved under it. Null is a real answer:
        /// the picker shows its blank entry and nothing is filled in.
        /// </summary>
        public Preset Named(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            return _inOrder.FirstOrDefault(
                one => string.Equals(one.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public bool Holds(string name)
        {
            return Named(name) != null;
        }

        /// <summary>
        /// What the user's own file should hold after this change. Only their presets, because
        /// writing the shipped ones into it as well would freeze this version's defaults into
        /// the user's file and the next install could never move them.
        /// </summary>
        public IReadOnlyList<Preset> TheirOwn
        {
            get { return _inOrder.Where(one => one.Source == PresetSource.UserFile).ToList(); }
        }

        /// <summary>
        /// The presets with one saved by the user. It comes back as a new object, the way the
        /// grid columns and the title block settings do, so nothing holds a half changed copy.
        ///
        /// Saving over a shipped one keeps its place in the list rather than moving it to the
        /// end, because the user is correcting that preset rather than adding another.
        /// </summary>
        public Presets With(Preset one)
        {
            if (one == null || !one.IsNamed) return this;

            Preset theirs = Marked(one, PresetSource.UserFile);
            var kept = new List<Preset>();
            bool replaced = false;

            foreach (Preset held in _inOrder)
            {
                if (string.Equals(held.Name, theirs.Name, StringComparison.OrdinalIgnoreCase))
                {
                    kept.Add(theirs);
                    replaced = true;
                }
                else
                {
                    kept.Add(held);
                }
            }

            if (!replaced) kept.Add(theirs);

            return new Presets(kept);
        }

        /// <summary>
        /// The presets with one taken out.
        ///
        /// Deleting the user's copy of a shipped preset cannot take the shipped one away, since
        /// this file is not written, so what comes back is the shipped one. Saying so is
        /// <see cref="WhatDeletingDoes"/>, because a delete that leaves the entry on screen
        /// reads exactly like a delete that failed.
        /// </summary>
        public Presets Without(string name, IEnumerable<Preset> shipped)
        {
            if (string.IsNullOrEmpty(name)) return this;

            var user = TheirOwn
                .Where(one => !string.Equals(
                    one.Name, name.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Of(user, shipped);
        }

        /// <summary>
        /// What the Manage window says before a delete, so a preset coming back under the same
        /// name is expected rather than a surprise.
        /// </summary>
        public string WhatDeletingDoes(string name, IEnumerable<Preset> shipped)
        {
            Preset held = Named(name);
            if (held == null) return string.Empty;

            bool alsoShipped = OnlyNamed(shipped)
                .Any(one => string.Equals(
                    one.Name, held.Name, StringComparison.OrdinalIgnoreCase));

            if (held.Source == PresetSource.ShippedDefaults)
            {
                return held.Name + " came with the tool and cannot be deleted. Save your own "
                    + "under this name to replace it.";
            }

            return alsoShipped
                ? "Deleting " + held.Name + " puts back the one that came with the tool, which "
                    + "has the same name."
                : "Deleting " + held.Name + " cannot be undone.";
        }

        private static IEnumerable<Preset> OnlyNamed(IEnumerable<Preset> presets)
        {
            return (presets ?? Enumerable.Empty<Preset>())
                .Where(one => one != null && one.IsNamed);
        }

        private static void Keep(
            Dictionary<string, Preset> byName, List<string> order, Preset one)
        {
            if (!byName.ContainsKey(one.Name)) order.Add(one.Name);
            byName[one.Name] = one;
        }

        private static Preset Marked(Preset one, PresetSource source)
        {
            return new Preset(one.Name, one.Ticked, one.Sheets, source);
        }
    }
}
