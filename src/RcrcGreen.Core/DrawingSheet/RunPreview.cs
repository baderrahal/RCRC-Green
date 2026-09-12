using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One title block type, measured off a sheet in this model that already uses it.
    ///
    /// Sheet Width and Sheet Height are instance parameters, so a title block type on its own
    /// has no size. The run learns the size of a sheet it has just made by measuring the block
    /// it just placed, and the preview cannot wait for that, so it borrows the measurement from
    /// a sheet already using the same type. **Which sheet it was read off travels with it**,
    /// because a size that came from nowhere reads exactly like a size that was measured.
    /// </summary>
    public sealed class MeasuredTitleBlock
    {
        public MeasuredTitleBlock(
            string familyName,
            string typeName,
            double widthFeet,
            double heightFeet,
            string offSheetNumber)
        {
            FamilyName = familyName ?? string.Empty;
            TypeName = typeName ?? string.Empty;
            WidthFeet = widthFeet;
            HeightFeet = heightFeet;
            OffSheetNumber = (offSheetNumber ?? string.Empty).Trim();
        }

        public string FamilyName { get; }

        public string TypeName { get; }

        public double WidthFeet { get; }

        public double HeightFeet { get; }

        /// <summary>
        /// The sheet the measurement came off, empty when whoever built this did not say.
        /// </summary>
        public string OffSheetNumber { get; }

        public string TitleBlock
        {
            get { return (FamilyName + " " + TypeName).Trim(); }
        }

        public bool Measured
        {
            get { return IsReal(WidthFeet) && IsReal(HeightFeet); }
        }

        public string InWords()
        {
            if (!Measured)
            {
                return TitleBlock + " gave no width or height, so nothing can be drawn at its "
                    + "size.";
            }

            string size = Lengths.InMillimetres(WidthFeet)
                    .ToString("0.#", CultureInfo.InvariantCulture)
                + " by "
                + Lengths.InMillimetres(HeightFeet).ToString("0.#", CultureInfo.InvariantCulture)
                + " mm";

            return OffSheetNumber.Length == 0
                ? size + ", off a sheet in this model."
                : size + ", measured off sheet " + OffSheetNumber + ".";
        }

        private static bool IsReal(double length)
        {
            return !double.IsNaN(length) && !double.IsInfinity(length) && length > 0.0;
        }
    }

    /// <summary>
    /// How big each title block type comes out, for the sheets the run has not made yet.
    ///
    /// **A type no sheet in this model uses has no size, and that is an answer rather than a
    /// fault.** It is exactly the type somebody is about to start using, and a preview that
    /// filled in an A1 because the name looks like one would be drawing a guess.
    /// </summary>
    public sealed class TitleBlockSizes
    {
        private readonly List<MeasuredTitleBlock> _measured;

        private TitleBlockSizes(List<MeasuredTitleBlock> measured)
        {
            _measured = measured;
        }

        public static readonly TitleBlockSizes Nothing =
            new TitleBlockSizes(new List<MeasuredTitleBlock>());

        /// <summary>
        /// The first measurement of each type wins, the way the run takes the first title block
        /// found on a sheet. A model whose A1 sheets disagree about their size is a model
        /// question rather than one to answer by averaging.
        /// </summary>
        public static TitleBlockSizes Of(IEnumerable<MeasuredTitleBlock> measured)
        {
            var kept = new List<MeasuredTitleBlock>();

            foreach (MeasuredTitleBlock one in
                (measured ?? Enumerable.Empty<MeasuredTitleBlock>()).Where(one => one != null))
            {
                if (Found(kept, one.FamilyName, one.TypeName) == null) kept.Add(one);
            }

            return new TitleBlockSizes(kept);
        }

        public int Count
        {
            get { return _measured.Count; }
        }

        /// <summary>
        /// The size of that title block type, or null when no sheet in this model uses it.
        /// </summary>
        public MeasuredTitleBlock For(string familyName, string typeName)
        {
            return Found(_measured, familyName, typeName);
        }

        private static MeasuredTitleBlock Found(
            IEnumerable<MeasuredTitleBlock> among, string familyName, string typeName)
        {
            return among.FirstOrDefault(one =>
                string.CompareOrdinal(one.FamilyName, familyName ?? string.Empty) == 0
                && string.CompareOrdinal(one.TypeName, typeName ?? string.Empty) == 0);
        }
    }

    /// <summary>
    /// How big the views already in the model come out on paper, by their full name.
    ///
    /// A view the run is about to create has no size, because Revit has not drawn it. So has a
    /// schedule, whose size is not known until it is placed. Both come back as not measured and
    /// are marked, never filled in.
    /// </summary>
    public sealed class PaperSizes
    {
        private readonly Dictionary<string, ViewOnPaper> _byName;

        private PaperSizes(Dictionary<string, ViewOnPaper> byName)
        {
            _byName = byName;
        }

        public static readonly PaperSizes Nothing =
            new PaperSizes(new Dictionary<string, ViewOnPaper>(StringComparer.Ordinal));

        public static PaperSizes Of(IEnumerable<ViewOnPaper> views)
        {
            var byName = new Dictionary<string, ViewOnPaper>(StringComparer.Ordinal);

            foreach (ViewOnPaper one in (views ?? Enumerable.Empty<ViewOnPaper>())
                .Where(one => one != null && one.ViewName.Length > 0 && one.Measured))
            {
                if (!byName.ContainsKey(one.ViewName)) byName.Add(one.ViewName, one);
            }

            return new PaperSizes(byName);
        }

        public int Count
        {
            get { return _byName.Count; }
        }

        /// <summary>
        /// That view's size, or a not measured one under the same name. Never null, because
        /// every caller here has to lay the view out either way.
        /// </summary>
        public ViewOnPaper For(string viewName)
        {
            string wanted = viewName ?? string.Empty;

            ViewOnPaper found;
            return _byName.TryGetValue(wanted, out found)
                ? found
                : ViewOnPaper.NotMeasured(wanted);
        }
    }

    /// <summary>
    /// One card of the preview: a sheet the run will make, drawn at the size it will come out.
    /// </summary>
    public sealed class PreviewedSheet
    {
        internal PreviewedSheet(
            RunItem item,
            MeasuredTitleBlock titleBlock,
            PlacedSheet placed,
            int ofHowMany,
            int which)
        {
            Item = item;
            TitleBlock = titleBlock;
            Placed = placed;
            OfHowMany = ofHowMany;
            Which = which;
        }

        public RunItem Item { get; }

        public string PlotId
        {
            get { return Item.PlotId; }
        }

        public string SheetNumber
        {
            get { return Item.SheetNumber; }
        }

        public string SheetName
        {
            get { return Item.SheetName; }
        }

        /// <summary>
        /// The size to draw the outline at, null when no sheet in this model uses that title
        /// block. A card with none draws no outline and says why.
        /// </summary>
        public MeasuredTitleBlock TitleBlock { get; }

        /// <summary>
        /// Where the views land, out of the one placement the run itself uses. Null when the
        /// size is not known, because there is no area to lay anything out in.
        /// </summary>
        public PlacedSheet Placed { get; }

        /// <summary>
        /// How many sheets this one row comes out as, and which of them this card is. One row
        /// makes more than one sheet whenever its views do not all fit.
        /// </summary>
        public int OfHowMany { get; }

        public int Which { get; }

        public bool CanBeDrawn
        {
            get { return TitleBlock != null && TitleBlock.Measured && Placed != null; }
        }

        /// <summary>
        /// True for a sheet that exists because the one before it filled up. The brief calls
        /// for a mark on it, and it is the one thing about the run somebody cannot work out by
        /// reading step 4.
        /// </summary>
        public bool CarriesWhatDidNotFit
        {
            get { return Placed != null && Placed.CarriedOver; }
        }

        public bool HasNoViews
        {
            get { return Placed == null || Placed.Views.Count == 0; }
        }

        public bool AnythingNotMeasured
        {
            get { return Placed == null || Placed.AnythingNotMeasured; }
        }

        /// <summary>
        /// What the card says under the outline, after the number and the name.
        /// </summary>
        public string InWords()
        {
            if (TitleBlock == null)
            {
                return "No sheet in this model uses this title block yet, so its size is not "
                    + "known and nothing is drawn to scale.";
            }

            if (!TitleBlock.Measured) return TitleBlock.InWords();

            var said = new List<string> { TitleBlock.InWords() };

            if (HasNoViews)
            {
                said.Add("No views, the way a title sheet is.");
            }
            else
            {
                said.Add(Placed.Views.Count == 1
                    ? "1 view, at " + Placed.ViewsPerSheet + " per sheet."
                    : Placed.Views.Count + " views, at " + Placed.ViewsPerSheet
                        + " per sheet.");
            }

            if (CarriesWhatDidNotFit)
            {
                said.Add("This one carries what did not fit on the sheet before it.");
            }

            if (Placed != null && Placed.HoldsOneTooBig)
            {
                said.Add("A view on it is bigger than the cell it goes in.");
            }

            List<string> unmeasured = Placed == null
                ? new List<string>()
                : Placed.Views
                    .Where(one => !one.View.Measured)
                    .Select(one => one.View.ViewName)
                    .ToList();

            if (unmeasured.Count > 0)
            {
                said.Add((unmeasured.Count == 1 ? "1 view is" : unmeasured.Count + " views are")
                    + " drawn at a nominal size rather than measured: "
                    + string.Join(", ", unmeasured.ToArray()) + ".");
            }

            return string.Join(" ", said.ToArray());
        }
    }

    /// <summary>
    /// The run, drawn before it is made: one card per sheet, in the order the run creates them.
    ///
    /// **It computes no layout of its own.** Every position on every card comes out of
    /// <see cref="SheetPlacement"/>, which is what the writer places from, so a card and the
    /// sheet it draws cannot disagree. Writing the layout twice would have been the eleventh
    /// time this repo kept two records of one fact, and the two would have agreed the day they
    /// were written and drifted the first time either changed.
    ///
    /// What it cannot know it marks rather than guesses. A view the run is about to create has
    /// no size until Revit draws it, and so does every schedule, so both are laid out at their
    /// cell and named as not measured. A title block no sheet in this model uses has no size at
    /// all, and its card says so and draws nothing.
    /// </summary>
    public static class RunPreview
    {
        /// <summary>
        /// How wide a view is drawn when its size is not known, as a share of its cell. It is
        /// a nominal figure and the card says so, rather than a measurement dressed up as one.
        /// </summary>
        public const double NominalShareOfTheCell = 0.9;

        /// <summary>
        /// The cards, in the order the run makes the sheets.
        /// </summary>
        /// <param name="plan">The run as it stands. Only its sheets are drawn.</param>
        /// <param name="titleBlocks">How big each title block type comes out, measured off the
        /// sheets this model already holds.</param>
        /// <param name="views">How big the views already in the model come out on paper. A
        /// view not in it is laid out and marked rather than left off.</param>
        public static IReadOnlyList<PreviewedSheet> Of(
            RunPlan plan, TitleBlockSizes titleBlocks, PaperSizes views)
        {
            if (plan == null) throw new ArgumentNullException("plan");

            TitleBlockSizes sizes = titleBlocks ?? TitleBlockSizes.Nothing;
            PaperSizes measured = views ?? PaperSizes.Nothing;

            var cards = new List<PreviewedSheet>();

            foreach (RunItem item in plan.Items.Where(one => one.Kind == RunItemKind.Sheet))
            {
                MeasuredTitleBlock block = sizes.For(
                    item.Sheet.TitleBlockFamilyName, item.Sheet.TitleBlockTypeName);

                if (block == null || !block.Measured)
                {
                    cards.Add(new PreviewedSheet(item, block, null, 1, 1));
                    continue;
                }

                DrawingArea area = DrawingArea.InsideTheTitleBlock(
                    block.WidthFeet, block.HeightFeet);

                IReadOnlyList<PlacedSheet> placed = item.Sheet.Views.Count == 0
                    ? SheetPlacement.Empty(area, item.Sheet.ViewsPerSheet)
                    : SheetPlacement.Of(
                        area,
                        item.Sheet.ViewsPerSheet,
                        item.Sheet.Views.Select(type => measured.For(ViewNaming.Of(item.PlotId, type))));

                for (int at = 0; at < placed.Count; at++)
                {
                    cards.Add(new PreviewedSheet(
                        item, block, placed[at], placed.Count, at + 1));
                }
            }

            return cards;
        }

        /// <summary>
        /// What the whole preview says above the cards.
        /// </summary>
        public static string InWords(IReadOnlyList<PreviewedSheet> cards)
        {
            if (cards == null || cards.Count == 0)
            {
                return "No sheet is described yet, so there is nothing to draw.";
            }

            int cannot = cards.Count(one => !one.CanBeDrawn);
            int carried = cards.Count(one => one.CarriesWhatDidNotFit);

            string said = cards.Count == 1
                ? "1 sheet, drawn at the size its title block comes out."
                : cards.Count + " sheets, drawn at the size their title blocks come out.";

            if (carried > 0)
            {
                said += " " + (carried == 1
                    ? "1 of them carries what did not fit on the sheet before it."
                    : carried + " of them carry what did not fit on the sheet before them.");
            }

            if (cannot > 0)
            {
                said += " " + (cannot == 1
                    ? "1 cannot be drawn, because no sheet in this model uses its title block "
                        + "yet."
                    : cannot + " cannot be drawn, because no sheet in this model uses their "
                        + "title blocks yet.");
            }

            return said;
        }
    }
}
