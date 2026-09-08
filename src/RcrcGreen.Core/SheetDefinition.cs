using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One view on a sheet: which view type it is, and where the middle of it sits.
    ///
    /// The position is measured from the sheet origin in feet, because that is the unit Revit
    /// holds a sheet in and converting it here would mean converting it back to place it.
    /// </summary>
    public sealed class SheetViewPlacement
    {
        public SheetViewPlacement(ViewType type, double centreX, double centreY, bool isASchedule)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (double.IsNaN(centreX) || double.IsInfinity(centreX)
                || double.IsNaN(centreY) || double.IsInfinity(centreY))
            {
                throw new ArgumentException("That viewport centre is not a real position.");
            }

            Type = type;
            CentreX = centreX;
            CentreY = centreY;
            IsASchedule = isASchedule;
        }

        public ViewType Type { get; }

        public double CentreX { get; }

        public double CentreY { get; }

        /// <summary>
        /// A schedule on a sheet is a different element from a drawing on a sheet and is placed
        /// by a different call, so the two are told apart when the definition is captured
        /// rather than guessed at when it is used.
        /// </summary>
        public bool IsASchedule { get; }

        public override string ToString()
        {
            return Type + " at " + CentreX.ToString("0.###") + " " + CentreY.ToString("0.###");
        }
    }

    /// <summary>
    /// A sheet the user set up by hand, read into plain values so another one can be built like
    /// it.
    ///
    /// The team answered the three questions that stopped sheet creation before. The title
    /// block is whichever one the source sheet carries, a view sits where it sits on the source
    /// sheet, and several views lay out however the source sheet has them. So none of it is
    /// decided here. All of it is copied from a sheet somebody already got right.
    ///
    /// Same shape as <see cref="ScheduleDefinition"/> and for the same reason. Capture fills one
    /// in from a sheet that exists, create builds one in the model, and neither half knows about
    /// the other.
    /// </summary>
    public sealed class SheetDefinition
    {
        public SheetDefinition(
            string titleBlockFamilyName,
            string titleBlockTypeName,
            double sheetWidth,
            double sheetHeight,
            IEnumerable<SheetViewPlacement> views)
        {
            if (titleBlockFamilyName == null) throw new ArgumentNullException("titleBlockFamilyName");
            if (titleBlockTypeName == null) throw new ArgumentNullException("titleBlockTypeName");

            TitleBlockFamilyName = titleBlockFamilyName;
            TitleBlockTypeName = titleBlockTypeName;
            SheetWidth = sheetWidth;
            SheetHeight = sheetHeight;

            // One placement per view type. A source sheet holding the same view type twice
            // gives no way to tell which position the new one should take, so the first is
            // kept and the second is dropped rather than the pair being placed on top of
            // each other.
            Views = (views ?? Enumerable.Empty<SheetViewPlacement>())
                .Where(one => one != null)
                .GroupBy(one => one.Type)
                .Select(byType => byType.First())
                .OrderBy(one => one.Type)
                .ToList();
        }

        public string TitleBlockFamilyName { get; }

        public string TitleBlockTypeName { get; }

        /// <summary>
        /// The sheet size in feet, carried so a report can say what was copied. Nothing sets it
        /// on a new sheet, because the title block is what decides the size.
        /// </summary>
        public double SheetWidth { get; }

        public double SheetHeight { get; }

        public IReadOnlyList<SheetViewPlacement> Views { get; }

        public IReadOnlyList<ViewType> ViewTypes
        {
            get { return Views.Select(one => one.Type).ToList(); }
        }

        /// <summary>
        /// A sheet with no title block cannot be rebuilt, because the title block is the one
        /// thing a new sheet is created with. A sheet with no views on it can be, and it makes
        /// an empty sheet, which is a thing somebody might want.
        /// </summary>
        public bool CanBeUsed
        {
            get { return TitleBlockFamilyName.Length > 0 && TitleBlockTypeName.Length > 0; }
        }

        public SheetViewPlacement PlacementFor(ViewType type)
        {
            if (type == null) return null;
            return Views.FirstOrDefault(one => one.Type.Equals(type));
        }

        /// <summary>
        /// What the panel and the report say about the captured sheet, so the user can see what
        /// is about to be copied before pressing anything.
        /// </summary>
        public string InWords()
        {
            if (!CanBeUsed) return "That sheet carries no title block, so nothing can be copied from it.";

            string block = TitleBlockFamilyName + " " + TitleBlockTypeName;

            if (Views.Count == 0)
            {
                return "Title block " + block + ", no views on it. A new sheet would be empty.";
            }

            return "Title block " + block + ", " + Views.Count
                + (Views.Count == 1 ? " view on it: " : " views on it: ")
                + string.Join(", ", Views.Select(one => one.Type.ToString()).ToArray());
        }
    }
}
