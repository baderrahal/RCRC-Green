using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Which file a preset came out of. It is said beside the name in the picker, because a
    /// preset somebody saved and a preset that came out of the box are different things to the
    /// person deciding whether to trust it, the same reason a title block pairing says so.
    /// </summary>
    public enum PresetSource
    {
        Unset = 0,
        ShippedDefaults = 1,
        UserFile = 2
    }

    /// <summary>
    /// One sheet definition inside a preset: the title block, how many views go on a sheet, and
    /// which view types, in the order they go on.
    ///
    /// It is not a <see cref="SheetDefinition"/>. A definition works out the sheets it needs and
    /// the names on them, which needs the sheet name settings and a plot, and a preset holds
    /// neither. This is only what the user picked.
    /// </summary>
    public sealed class PresetSheet
    {
        public PresetSheet(
            string titleBlockFamilyName,
            string titleBlockTypeName,
            int viewsPerSheet,
            IEnumerable<ViewType> views)
        {
            TitleBlockFamilyName = titleBlockFamilyName ?? string.Empty;
            TitleBlockTypeName = titleBlockTypeName ?? string.Empty;
            ViewsPerSheet = SheetLayout.IsACount(viewsPerSheet) ? viewsPerSheet : 1;

            // In the order they were ticked, because that is the order they go onto sheets,
            // and deduplicated by hand so that order cannot silently change.
            var seen = new HashSet<ViewType>();
            var wanted = new List<ViewType>();
            foreach (ViewType one in (views ?? Enumerable.Empty<ViewType>())
                .Where(one => one != null))
            {
                if (seen.Add(one)) wanted.Add(one);
            }

            Views = wanted;
        }

        public string TitleBlockFamilyName { get; }

        public string TitleBlockTypeName { get; }

        public int ViewsPerSheet { get; }

        public IReadOnlyList<ViewType> Views { get; }

        public string TitleBlock
        {
            get { return (TitleBlockFamilyName + " " + TitleBlockTypeName).Trim(); }
        }

        /// <summary>
        /// How many sheets this makes per plot, which is what the picker says before anything
        /// is filled in. A definition with no views is a title sheet and makes one.
        /// </summary>
        public int SheetsPerPlot
        {
            get
            {
                if (Views.Count == 0) return 1;

                return (Views.Count + ViewsPerSheet - 1) / ViewsPerSheet;
            }
        }

        public override string ToString()
        {
            return TitleBlock.Length == 0 ? "(no title block)" : TitleBlock;
        }
    }

    /// <summary>
    /// A saved answer to steps 2 and 4: which view types are ticked and which sheets are
    /// described over them.
    ///
    /// **It holds no plot, no sub plot and no sheet number.** Those are what changes between one
    /// run and the next, and a preset that carried them would fill step 1 with last week's
    /// plots and put a number on a sheet somebody else's plot already has. The eleven title
    /// blocks and the six sheets of the DM-11 run are the part that repeats.
    /// </summary>
    public sealed class Preset
    {
        public Preset(
            string name,
            IEnumerable<ViewType> ticked,
            IEnumerable<PresetSheet> sheets,
            PresetSource source = PresetSource.Unset)
        {
            Name = (name ?? string.Empty).Trim();
            Source = source;

            var seen = new HashSet<ViewType>();
            var wanted = new List<ViewType>();
            foreach (ViewType one in (ticked ?? Enumerable.Empty<ViewType>())
                .Where(one => one != null))
            {
                if (seen.Add(one)) wanted.Add(one);
            }

            Ticked = wanted.OrderBy(one => one).ToList();
            Sheets = (sheets ?? Enumerable.Empty<PresetSheet>())
                .Where(one => one != null)
                .ToList();
        }

        /// <summary>
        /// What the user calls it. It is the key: two presets of one name are one preset, and
        /// the user's own file wins over the shipped one, the way a title block pairing does.
        /// </summary>
        public string Name { get; }

        public PresetSource Source { get; }

        /// <summary>
        /// The view types ticked in step 2, in view type order, which is the order the columns
        /// read in.
        /// </summary>
        public IReadOnlyList<ViewType> Ticked { get; }

        /// <summary>
        /// The sheet definitions in step 4, in the order they were described, because that is
        /// the order they are drawn in and the order their sheets come out in.
        /// </summary>
        public IReadOnlyList<PresetSheet> Sheets { get; }

        public bool IsNamed
        {
            get { return Name.Length > 0; }
        }

        /// <summary>
        /// What the picker says under a preset before it is used.
        /// </summary>
        public string InWords()
        {
            int sheets = Sheets.Sum(one => one.SheetsPerPlot);

            return (Ticked.Count == 1 ? "1 view type" : Ticked.Count + " view types")
                + ", " + (Sheets.Count == 1 ? "1 sheet definition" : Sheets.Count
                    + " sheet definitions")
                + ", " + (sheets == 1 ? "1 sheet" : sheets + " sheets") + " per ticked plot.";
        }

        /// <summary>
        /// Whether another preset asks for exactly what this one does: the same view types
        /// ticked, and the same sheet definitions in the same order, each on the same title
        /// block with the same views in the same order.
        ///
        /// This is how the panel knows whether steps 2 and 4 still say what the preset it was
        /// filled from says. The alternative was a flag set by every handler that can change
        /// one of them, and a flag somebody forgets to set in the next handler is a second
        /// record of a fact the steps already hold.
        ///
        /// The name and the file it came from are not part of it. Saving the same answer under
        /// a second name does not make it a different answer.
        /// </summary>
        public bool SameAnswer(Preset other)
        {
            if (other == null) return false;
            if (!Ticked.SequenceEqual(other.Ticked)) return false;
            if (Sheets.Count != other.Sheets.Count) return false;

            for (int at = 0; at < Sheets.Count; at++)
            {
                PresetSheet mine = Sheets[at];
                PresetSheet theirs = other.Sheets[at];

                if (mine.ViewsPerSheet != theirs.ViewsPerSheet) return false;
                if (string.CompareOrdinal(
                        mine.TitleBlockFamilyName, theirs.TitleBlockFamilyName) != 0)
                {
                    return false;
                }

                if (string.CompareOrdinal(mine.TitleBlockTypeName, theirs.TitleBlockTypeName) != 0)
                {
                    return false;
                }

                if (!mine.Views.SequenceEqual(theirs.Views)) return false;
            }

            return true;
        }

        /// <summary>
        /// Where it came from, said beside the name.
        /// </summary>
        public string WhereItCameFrom()
        {
            switch (Source)
            {
                case PresetSource.UserFile: return "your own presets";
                case PresetSource.ShippedDefaults: return "the shipped defaults";
                default: return string.Empty;
            }
        }

        public override string ToString()
        {
            return Name.Length == 0 ? "(unnamed)" : Name;
        }
    }
}
