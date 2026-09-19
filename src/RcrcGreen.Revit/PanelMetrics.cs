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
        /// The View Filters keyword box, tall enough to show its three default lines at
        /// once. Added for that pane and used by nothing on the other two.
        /// **This number has not been seen in Revit.**
        /// </summary>
        public const double KeywordsHeight = 64.0;

        /// <summary>
        /// The square that shows what a hex box parses to on a View Filters row. Small on
        /// purpose: it answers is this the colour I meant, nothing more.
        /// **This number has not been seen in Revit.**
        /// </summary>
        public const double Swatch = 16.0;

        /// <summary>
        /// A View Filters hex box and its weight picker, sized for #FF0000 and two digits.
        /// **This number has not been seen in Revit.**
        /// </summary>
        public const double HexWidth = 64.0;

        /// <summary>
        /// How faded a disabled colour square draws, so a greyed row still shows which
        /// colour it would use. **This number has not been seen in Revit.**
        /// </summary>
        public const double FadedOpacity = 0.4;

        /// <summary>
        /// The box around a colour square, all four sides, where Hairline is the one edge
        /// a divider wants. **This number has not been seen in Revit.**
        /// </summary>
        public static readonly Thickness Outline = new Thickness(1.0);

        /// <summary>
        /// The tick column on a step row, on the right of its title line. Wide enough for the
        /// word the Drawing Sheet marks a finished step with and nothing more, because the
        /// title beside it is what the row is for.
        /// **This number has not been seen in Revit.**
        /// </summary>
        public const double Tick = 34.0;

        /// <summary>
        /// The width the Drawing Sheet's rows are laid out for. The rows are never measured
        /// narrower than this, so below it the pane scrolls sideways rather than squeezing a
        /// wrapped line down to a word a line.
        /// **This number has not been seen in Revit.**
        /// </summary>
        public const double NarrowPane = 300.0;

        /// <summary>
        /// A wider caption column for the KPI pane's typed boxes. Prepared by is the longest of
        /// the three captions and came out as Prepared b, running into its box, at the shared 54.
        ///
        /// Added rather than widening LabelWidth, which the Drawing Sheet uses in two places and
        /// this round does not touch. **This number has not been seen in Revit.**
        /// </summary>
        public const double WideLabelWidth = 88.0;

        /// <summary>
        /// The View Filters rail, the whole column. The rail has to leave a 300 pixel pane
        /// a readable body, which is where the 34 comes from. Added for that pane and used
        /// by nothing on the other two. **This number has not been seen in Revit.**
        /// </summary>
        public const double RailWidth = 34.0;

        /// <summary>
        /// One rail cell, its width and its height, so the cell is a circle when its
        /// corner radius is half of this. **This number has not been seen in Revit.**
        /// </summary>
        public const double RailCell = 26.0;

        /// <summary>
        /// The tick drawn in a finished rail cell, a shape rather than a character so no
        /// font gets a say in it. **This number has not been seen in Revit.**
        /// </summary>
        public const double RailTick = 12.0;
    }
}
