using System.Windows;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// Every spacing and every font size the panel uses, named once.
    ///
    /// The panel was built by typing a margin at each control, so nothing lined up with
    /// anything and changing the rhythm meant finding forty numbers. These are the only numbers
    /// in the interface. The same reason <see cref="PanelTheme"/> holds every colour: a value
    /// written next to the thing it paints is a value nobody can change on purpose.
    ///
    /// This is WPF rather than Revit, so it stays out of Core, which cannot reference either.
    /// </summary>
    internal static class PanelMetrics
    {
        public const double Body = 12.0;

        /// <summary>
        /// How wide the KPI progress window is. Wide enough for the longest line it shows,
        /// Sections 5 to 8 of 9, schedules, 400 of 951, 42%, without it wrapping to two.
        /// </summary>
        public const double ProgressWindowWidth = 420.0;

        public const double StepNumber = 15.0;

        public const double StepTitle = 12.0;

        public const double Cell = 14.0;

        /// <summary>
        /// Every grid row is this tall, in both the frozen column and the scrolling part, which
        /// is what makes the two line up. Auto height on either side would drift the moment one
        /// cell wrapped.
        /// </summary>
        public const double RowHeight = 24.0;

        public const double HeaderRowHeight = 40.0;

        public const double GridHeight = 340.0;

        public const double ListHeight = 200.0;

        public const double ColumnWidth = 150.0;

        public static readonly Thickness Edge = new Thickness(8.0);

        public static readonly Thickness Row = new Thickness(0.0, 3.0, 0.0, 3.0);

        public static readonly Thickness StepGap = new Thickness(0.0, 0.0, 0.0, 4.0);

        public static readonly Thickness StepInside = new Thickness(10.0, 6.0, 6.0, 8.0);

        public static readonly Thickness HeaderInside = new Thickness(8.0, 5.0, 8.0, 5.0);

        public static readonly Thickness StripInside = new Thickness(8.0, 6.0, 8.0, 6.0);

        public static readonly Thickness CellPad = new Thickness(6.0, 2.0, 6.0, 2.0);

        /// <summary>
        /// No padding and no border, for a control that has to sit exactly where the label it
        /// replaced sat. A button's own chrome would push the frozen plot column out of step
        /// with the scrolling cells beside it, and the two lining up is the only thing making
        /// the grid readable.
        /// </summary>
        public static readonly Thickness Nothing = new Thickness(0.0);

        public static readonly Thickness Gap = new Thickness(0.0, 0.0, 6.0, 0.0);

        /// <summary>
        /// Above a block heading in the KPI pane, which scrolls as one long column rather than
        /// as numbered steps, so the headings need the air the Drawing Sheet gets from its
        /// step borders. Added for the KPI pane and used by nothing on the Drawing Sheet.
        /// </summary>
        public static readonly Thickness Heading = new Thickness(0.0, 10.0, 0.0, 4.0);

        public static readonly Thickness Hairline = new Thickness(0.0, 0.0, 0.0, 1.0);

        /// <summary>
        /// The same hairline on the top edge, for a status line docked at the bottom of a pane.
        /// </summary>
        public static readonly Thickness HairlineAbove = new Thickness(0.0, 1.0, 0.0, 0.0);

        public const double LabelWidth = 54.0;

        /// <summary>
        /// A wider caption column for the KPI pane's typed boxes. Prepared by is the longest of
        /// the three captions and came out as Prepared b, running into its box, at the shared 54.
        ///
        /// Added rather than widening LabelWidth, which the Drawing Sheet uses in two places and
        /// this round does not touch. **This number has not been seen in Revit.**
        /// </summary>
        public const double WideLabelWidth = 88.0;
    }
}
