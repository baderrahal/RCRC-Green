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
        private PanelTheme(Brush background, Brush foreground, Brush warning)
        {
            Background = background;
            Foreground = foreground;
            Warning = warning;
        }

        public Brush Background { get; }

        public Brush Foreground { get; }

        /// <summary>
        /// The mark on a plot with no scope box named for it. Firebrick reads well on white and
        /// almost disappears on Revit's dark grey, so the dark theme gets a lighter red instead
        /// of the same value twice.
        /// </summary>
        public Brush Warning { get; }

        public static PanelTheme Current()
        {
            return UIThemeManager.CurrentTheme == UITheme.Dark ? Dark() : Light();
        }

        private static PanelTheme Light()
        {
            return new PanelTheme(
                Frozen(0xFF, 0xFF, 0xFF),
                Frozen(0x1A, 0x1A, 0x1A),
                Frozen(0xB2, 0x22, 0x22));
        }

        private static PanelTheme Dark()
        {
            return new PanelTheme(
                Frozen(0x2E, 0x2E, 0x2E),
                Frozen(0xE6, 0xE6, 0xE6),
                Frozen(0xFF, 0x80, 0x80));
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
