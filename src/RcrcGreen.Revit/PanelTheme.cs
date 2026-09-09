using System.Windows.Media;
using Autodesk.Revit.UI;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// The colours the Drawing Sheet panel paints itself in, taken from whichever Revit theme
    /// is on.
    ///
    /// The first install proved why this has to exist. A dockable pane on the dark theme sits
    /// on a black background, WPF defaults a TextBlock to black text, and every heading, every
    /// label and the whole grid rendered black on black. The panel now paints both its own
    /// background and its own foreground, so the two are chosen together rather than one of
    /// them being whatever Revit happened to put behind.
    ///
    /// This lives outside the panel file so <see cref="DrawingSheetPanel"/> names no Revit type
    /// at all. Reading the current theme is a UI setting lookup rather than anything that
    /// touches a document, so it needs no external event and is safe to call while the panel is
    /// being built, which is before any document exists.
    /// </summary>
    internal sealed class PanelTheme
    {
        private PanelTheme(
            Brush background,
            Brush foreground,
            Brush warning,
            Brush strip,
            Brush stepHeader,
            Brush rowShade,
            Brush faint,
            Brush line,
            Brush primary,
            Brush onPrimary)
        {
            Background = background;
            Foreground = foreground;
            Warning = warning;
            Strip = strip;
            StepHeader = stepHeader;
            RowShade = rowShade;
            Faint = faint;
            Line = line;
            Primary = primary;
            OnPrimary = onPrimary;
        }

        public Brush Background { get; }

        public Brush Foreground { get; }

        /// <summary>
        /// The band at the top holding the model name and Refresh. It sits apart from the steps
        /// because it belongs to the document rather than to any one of them.
        /// </summary>
        public Brush Strip { get; }

        /// <summary>
        /// A step header. Every step wears it whether it is open or shut, so the five read as
        /// one list rather than as one open thing and four labels.
        /// </summary>
        public Brush StepHeader { get; }

        /// <summary>
        /// Every other row of the grid. Barely there on purpose. A stripe strong enough to
        /// notice is a stripe that competes with the marks.
        /// </summary>
        public Brush RowShade { get; }

        /// <summary>
        /// A summary on a shut step, a legend, the reason a step cannot be used yet. Quieter
        /// than the body text, and still readable, which rules out plain opacity on dark.
        /// </summary>
        public Brush Faint { get; }

        /// <summary>
        /// The hairline under a step header and under the grid's own header row.
        /// </summary>
        public Brush Line { get; }

        /// <summary>
        /// Run, and nothing else. One button on this panel writes to the model and it should
        /// not look like the four next to it.
        /// </summary>
        public Brush Primary { get; }

        public Brush OnPrimary { get; }

        /// <summary>
        /// The mark on a plot with no scope box named for it. Firebrick reads well on white and
        /// almost disappears on Revit's dark grey, so the dark theme gets a lighter red instead
        /// of the same value twice.
        /// </summary>
        public Brush Warning { get; }

        /// <summary>
        /// Nothing at all, for a control that has to take a click without looking like a button.
        /// A plot name and a column header both mark a whole row or column now, and both are
        /// still labels to read rather than buttons to press.
        ///
        /// It is the same in both themes, which is the point: a transparent brush shows
        /// whatever is behind it, so it cannot be the wrong colour for the theme. It lives here
        /// because a brush written into the panel file is how the panel came up black on black.
        /// </summary>
        public Brush Clear
        {
            get { return Brushes.Transparent; }
        }

        public static PanelTheme Current()
        {
            return UIThemeManager.CurrentTheme == UITheme.Dark ? Dark() : Light();
        }

        private static PanelTheme Light()
        {
            return new PanelTheme(
                Frozen(0xFF, 0xFF, 0xFF),
                Frozen(0x1A, 0x1A, 0x1A),
                Frozen(0xB2, 0x22, 0x22),
                Frozen(0xEC, 0xEC, 0xEC),
                Frozen(0xE4, 0xE4, 0xE4),
                Frozen(0xF6, 0xF6, 0xF6),
                Frozen(0x5E, 0x5E, 0x5E),
                Frozen(0xC8, 0xC8, 0xC8),
                Frozen(0x1F, 0x5C, 0x2E),
                Frozen(0xFF, 0xFF, 0xFF));
        }

        private static PanelTheme Dark()
        {
            return new PanelTheme(
                Frozen(0x2E, 0x2E, 0x2E),
                Frozen(0xE6, 0xE6, 0xE6),
                Frozen(0xFF, 0x80, 0x80),
                Frozen(0x26, 0x26, 0x26),
                Frozen(0x3A, 0x3A, 0x3A),
                Frozen(0x34, 0x34, 0x34),
                Frozen(0xAA, 0xAA, 0xAA),
                Frozen(0x4A, 0x4A, 0x4A),
                Frozen(0x6E, 0xB0, 0x83),
                Frozen(0x1A, 0x1A, 0x1A));
        }

        /// <summary>
        /// Frozen because these are built once and read from the UI thread on every redraw, and
        /// an unfrozen brush carries change tracking that nothing here uses.
        /// </summary>
        private static Brush Frozen(byte red, byte green, byte blue)
        {
            var brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
            brush.Freeze();
            return brush;
        }
    }
}
