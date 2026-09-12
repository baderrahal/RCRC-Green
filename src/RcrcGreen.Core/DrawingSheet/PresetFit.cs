using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// What one model can honour out of one preset, and what it cannot.
    ///
    /// A preset is shared across projects, the way the title block settings are, so a model
    /// free of a view type or a title block the preset names is ordinary rather than a fault.
    /// **The part that fits is filled in and the part that does not is named.** Refusing the
    /// whole preset over one missing schedule would make it useless on every model but the one
    /// it was saved from, and filling in a title block the model does not hold would put a name
    /// on a sheet that nothing could ever make.
    ///
    /// A view type the model does not hold can still be added by hand in step 2, the way any
    /// type the tool is asked to make from nothing is. What is refused here is filling one in
    /// without saying so.
    /// </summary>
    public sealed class PresetFit
    {
        private PresetFit(
            Preset preset,
            IReadOnlyList<ViewType> ticked,
            IReadOnlyList<ViewType> typesNotHeld,
            IReadOnlyList<PresetSheet> sheets,
            IReadOnlyList<PresetSheet> sheetsNotHeld,
            IReadOnlyList<ViewType> viewsDroppedFromSheets)
        {
            Preset = preset;
            Ticked = ticked;
            TypesNotHeld = typesNotHeld;
            Sheets = sheets;
            SheetsNotHeld = sheetsNotHeld;
            ViewsDroppedFromSheets = viewsDroppedFromSheets;
        }

        /// <summary>
        /// What the model holds out of the preset, against the view types and the title block
        /// types the model actually has.
        ///
        /// A title block is matched on its family and its type together, because that pair is
        /// what a sheet is made with and two families can hold a type of one name.
        /// </summary>
        public static PresetFit Of(
            Preset preset,
            IEnumerable<ViewType> typesInTheModel,
            IEnumerable<TitleBlockType> titleBlocksInTheModel)
        {
            if (preset == null) throw new ArgumentNullException("preset");

            var types = new HashSet<ViewType>(
                (typesInTheModel ?? Enumerable.Empty<ViewType>()).Where(one => one != null));

            List<TitleBlockType> blocks =
                (titleBlocksInTheModel ?? Enumerable.Empty<TitleBlockType>())
                    .Where(one => one != null)
                    .ToList();

            var ticked = new List<ViewType>();
            var typesNotHeld = new List<ViewType>();
            foreach (ViewType one in preset.Ticked)
            {
                if (types.Contains(one)) ticked.Add(one);
                else typesNotHeld.Add(one);
            }

            var sheets = new List<PresetSheet>();
            var sheetsNotHeld = new List<PresetSheet>();
            var dropped = new List<ViewType>();

            foreach (PresetSheet one in preset.Sheets)
            {
                if (!Holds(blocks, one.TitleBlockFamilyName, one.TitleBlockTypeName))
                {
                    sheetsNotHeld.Add(one);
                    continue;
                }

                // A sheet can only carry a view type step 2 offers, the same rule the panel
                // already follows when a type is unticked. A view the model does not hold is
                // named once, under the types, rather than twice.
                List<ViewType> kept = one.Views.Where(types.Contains).ToList();
                foreach (ViewType view in one.Views.Where(view => !types.Contains(view)))
                {
                    if (!dropped.Contains(view)) dropped.Add(view);
                }

                sheets.Add(new PresetSheet(
                    one.TitleBlockFamilyName, one.TitleBlockTypeName, one.ViewsPerSheet, kept));
            }

            return new PresetFit(preset, ticked, typesNotHeld, sheets, sheetsNotHeld, dropped);
        }

        public Preset Preset { get; }

        /// <summary>
        /// The view types to tick in step 2, every one of them held by this model.
        /// </summary>
        public IReadOnlyList<ViewType> Ticked { get; }

        /// <summary>
        /// The view types the preset names and this model does not hold. They are not ticked
        /// and they are named on screen.
        /// </summary>
        public IReadOnlyList<ViewType> TypesNotHeld { get; }

        /// <summary>
        /// The sheet definitions to describe in step 4, each on a title block this model holds
        /// and carrying only views it holds.
        /// </summary>
        public IReadOnlyList<PresetSheet> Sheets { get; }

        /// <summary>
        /// The sheet definitions whose title block this model does not hold. They are not
        /// described and they are named on screen, because a sheet on a title block that is not
        /// there could never be made.
        /// </summary>
        public IReadOnlyList<PresetSheet> SheetsNotHeld { get; }

        /// <summary>
        /// A view type taken off a sheet because the model does not hold it. It is already in
        /// <see cref="TypesNotHeld"/>, and this says which sheets are the poorer for it.
        /// </summary>
        public IReadOnlyList<ViewType> ViewsDroppedFromSheets { get; }

        public bool EverythingFits
        {
            get { return TypesNotHeld.Count == 0 && SheetsNotHeld.Count == 0; }
        }

        /// <summary>
        /// True when the model holds nothing the preset names. Picking it fills in nothing, so
        /// the panel says that rather than leaving somebody looking at two unchanged steps.
        /// </summary>
        public bool NothingFits
        {
            get { return Ticked.Count == 0 && Sheets.Count == 0; }
        }

        /// <summary>
        /// What the panel says after a preset is picked. One line, naming everything this model
        /// could not take, because a preset that half filled itself in silently is the same
        /// shape as the skip this repo has paid for twice.
        /// </summary>
        public string InWords()
        {
            if (NothingFits)
            {
                return "Nothing in " + Preset.Name + " is in this model, so steps 2 and 4 are "
                    + "unchanged. " + WhatIsMissing();
            }

            string filled = "Filled from " + Preset.Name + ": "
                + (Ticked.Count == 1 ? "1 view type" : Ticked.Count + " view types")
                + " and "
                + (Sheets.Count == 1
                    ? "1 sheet definition"
                    : Sheets.Count + " sheet definitions")
                + ".";

            return EverythingFits ? filled : filled + " " + WhatIsMissing();
        }

        /// <summary>
        /// The unavailable part of the preset, named. Empty when all of it fits.
        /// </summary>
        public string WhatIsMissing()
        {
            var said = new List<string>();

            if (TypesNotHeld.Count > 0)
            {
                said.Add("This model holds no "
                    + (TypesNotHeld.Count == 1 ? "view called " : "views called ")
                    + string.Join(", ", TypesNotHeld.Select(one => one.ToString()).ToArray()));
            }

            if (SheetsNotHeld.Count > 0)
            {
                said.Add((said.Count == 0 ? "This model holds no title " : "and no title ")
                    + (SheetsNotHeld.Count == 1 ? "block called " : "blocks called ")
                    + string.Join(", ", SheetsNotHeld
                        .Select(one => one.TitleBlock)
                        .Distinct(StringComparer.Ordinal)
                        .ToArray())
                    + ", so "
                    + (SheetsNotHeld.Count == 1
                        ? "1 sheet definition is"
                        : SheetsNotHeld.Count + " sheet definitions are")
                    + " left out");
            }

            if (said.Count == 0) return string.Empty;

            string missing = string.Join(" ", said.ToArray()) + ".";

            if (ViewsDroppedFromSheets.Count == 0) return missing;

            return missing + " "
                + string.Join(", ", ViewsDroppedFromSheets
                    .Select(one => one.ToString()).ToArray())
                + (ViewsDroppedFromSheets.Count == 1
                    ? " came off a sheet it was on."
                    : " came off sheets they were on.");
        }

        private static bool Holds(
            IEnumerable<TitleBlockType> blocks, string familyName, string typeName)
        {
            return blocks.Any(one =>
                string.CompareOrdinal(one.FamilyName, familyName ?? string.Empty) == 0
                && string.CompareOrdinal(one.TypeName, typeName ?? string.Empty) == 0);
        }
    }
}
