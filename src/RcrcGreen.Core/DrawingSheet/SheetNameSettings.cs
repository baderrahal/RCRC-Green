using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Where a sheet name pairing came from, shown beside the name so a value somebody chose
    /// and a value that shipped in a file read as the different things they are.
    /// </summary>
    public enum SheetNameSource
    {
        Unset = 0,
        ShippedDefaults = 1,
        UserFile = 2
    }

    /// <summary>
    /// One view type and the sheet name the team gives its sheet.
    /// </summary>
    public sealed class SheetNamePairing
    {
        public SheetNamePairing(ViewType type, string sheetName, SheetNameSource source)
        {
            if (type == null) throw new ArgumentNullException("type");

            Type = type;
            SheetName = sheetName ?? string.Empty;
            Source = source;
        }

        public ViewType Type { get; }

        public string SheetName { get; }

        public SheetNameSource Source { get; }

        public override string ToString()
        {
            return Type + " named " + SheetName;
        }
    }

    /// <summary>
    /// Which sheet name goes with which view type.
    ///
    /// The rule that a sheet is named after its view type, code removed and the rest upper
    /// cased, came from three examples that happened to match. Four of DM-11's eight sheets
    /// disprove it: OVERALL KEYPLAN is not OVERALL KEY PLAN, PROJECT LOCATION KEY PLAN is not
    /// LOCATION KEY PLAN, and HARDSCAPE SCHEDULES and SOFTSCAPE SCHEDULES are plural where
    /// the view names are not. One wrong derivation also made those sheets sort as unlisted,
    /// because SheetOrder holds the team's real words. So the name comes from this table,
    /// the user's own file first and the shipped defaults second, and the derivation is only
    /// the fallback for a view type neither file holds.
    /// </summary>
    public sealed class SheetNameSettings
    {
        private readonly Dictionary<ViewType, SheetNamePairing> _byType;

        private SheetNameSettings(Dictionary<ViewType, SheetNamePairing> byType)
        {
            _byType = byType;
        }

        public static readonly SheetNameSettings Nothing =
            new SheetNameSettings(new Dictionary<ViewType, SheetNamePairing>());

        /// <summary>
        /// The two files merged, the user's pairing winning over the shipped one for the same
        /// view type, each marked with which file it came from.
        /// </summary>
        public static SheetNameSettings Of(
            IEnumerable<SheetNamePairing> user, IEnumerable<SheetNamePairing> shipped)
        {
            var byType = new Dictionary<ViewType, SheetNamePairing>();

            foreach (SheetNamePairing one in (shipped ?? Enumerable.Empty<SheetNamePairing>())
                .Where(one => one != null && one.SheetName.Trim().Length > 0))
            {
                byType[one.Type] = Marked(one, SheetNameSource.ShippedDefaults);
            }

            foreach (SheetNamePairing one in (user ?? Enumerable.Empty<SheetNamePairing>())
                .Where(one => one != null && one.SheetName.Trim().Length > 0))
            {
                byType[one.Type] = Marked(one, SheetNameSource.UserFile);
            }

            return new SheetNameSettings(byType);
        }

        public SheetNamePairing For(ViewType type)
        {
            if (type == null) return null;

            SheetNamePairing found;
            return _byType.TryGetValue(type, out found) ? found : null;
        }

        /// <summary>
        /// The name a sheet holding one view of this type proposes: the table's word when a
        /// pairing exists, the old derivation when none does. One method, because the rows,
        /// the letter order and the run all have to agree on what a sheet is called.
        /// </summary>
        public string NameFor(ViewType type)
        {
            if (type == null) return string.Empty;

            SheetNamePairing found = For(type);
            return found == null ? SheetNaming.FromView(type) : found.SheetName.Trim();
        }

        /// <summary>
        /// Where the proposed name came from, said under the box. A derived name says so and
        /// says typing over it is remembered, because derived is the fallback that wrote
        /// LOCATION KEY PLAN onto a sheet the team calls PROJECT LOCATION KEY PLAN.
        /// </summary>
        public string NamedInWords(ViewType type)
        {
            if (type == null) return string.Empty;

            SheetNamePairing found = For(type);

            if (found == null)
            {
                return "Derived by upper casing the view name, because neither sheet name "
                    + "file holds " + type + ". Type over it and it is remembered.";
            }

            return found.Source == SheetNameSource.UserFile
                ? "Remembered from your own sheet names."
                : "Remembered from the shipped sheet names.";
        }

        public IReadOnlyList<SheetNamePairing> All
        {
            get { return _byType.Values.OrderBy(one => one.Type).ToList(); }
        }

        /// <summary>
        /// Only the user's own pairings, which is what their file holds after a change.
        /// Writing the shipped ones in as well would freeze this version's defaults into the
        /// user's file, and the next install could never move them.
        /// </summary>
        public IReadOnlyList<SheetNamePairing> TheirOwn
        {
            get
            {
                return _byType.Values
                    .Where(one => one.Source == SheetNameSource.UserFile)
                    .OrderBy(one => one.Type)
                    .ToList();
            }
        }

        /// <summary>
        /// The settings with one pairing set by the user. Setting the very name a shipped
        /// default already gives still marks it theirs, because they chose it and a later
        /// change to the shipped file should not move it under them.
        /// </summary>
        public SheetNameSettings With(ViewType type, string sheetName)
        {
            if (type == null) throw new ArgumentNullException("type");

            string wanted = (sheetName ?? string.Empty).Trim();
            if (wanted.Length == 0) return Without(type);

            var byType = new Dictionary<ViewType, SheetNamePairing>(_byType);
            byType[type] = new SheetNamePairing(type, wanted, SheetNameSource.UserFile);

            return new SheetNameSettings(byType);
        }

        public SheetNameSettings Without(ViewType type)
        {
            if (type == null || !_byType.ContainsKey(type)) return this;

            var byType = new Dictionary<ViewType, SheetNamePairing>(_byType);
            byType.Remove(type);

            return new SheetNameSettings(byType);
        }

        private static SheetNamePairing Marked(SheetNamePairing one, SheetNameSource source)
        {
            return new SheetNamePairing(one.Type, one.SheetName, source);
        }
    }
}
