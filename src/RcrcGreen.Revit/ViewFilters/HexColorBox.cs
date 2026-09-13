using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RcrcGreen.Core.ViewFilters;

namespace RcrcGreen.Revit.ViewFilters
{
    /// <summary>
    /// One hex box with its colour square, used three times on every filter row so the
    /// line colour and the two patterns look and behave the same. Typing a valid hex
    /// repaints the square live. Typing anything else paints the square's border in the
    /// warning colour and leaves the stored value alone, so a red edge means the text on
    /// screen is not the value the run would use. Clicking the square opens the Windows
    /// colour picker over Revit, already expanded so the red, green and blue fields show,
    /// and a pick comes back as upper case #RRGGBB.
    ///
    /// The round that asked for it named RcrcGreen.Core as its home, as a XAML pair. Core
    /// cannot reference WPF at all, and this project builds its interface in C# because an
    /// SDK style net48 project has no XAML step, the decision recorded in the csproj. So
    /// the control lives here, built in code, and the rules it applies, the parse, the
    /// writer and the keep or take, live in Core as HexColor, with the tests.
    ///
    /// The picker is the WinForms ColorDialog, owned by Revit's main window handle the
    /// same way the progress window is, so it opens in front of Revit. WinForms types stay
    /// fully qualified, because this class is a WPF control and the two toolkits share
    /// half their type names.
    /// </summary>
    internal sealed class HexColorBox : UserControl
    {
        /// <summary>
        /// The colours somebody mixes in the picker, kept for the rest of the Revit
        /// session and shared by every box, because they belong to the person, not a row.
        /// </summary>
        private static int[] _customColors = new int[16];

        private readonly TextBox _hexText;

        private readonly Border _swatch;

        // One flag breaks the feedback loop both ways, the setter syncing the text and the
        // text syncing the setter. It carries one constraint: nothing wired to HexChanged
        // may set Hex back on the SAME box from inside the callback, because it would run
        // under the flag and the text would quietly stop matching the value. Today's one
        // subscriber only re-gates Apply.
        private bool _syncing;

        public static readonly DependencyProperty HexProperty =
            DependencyProperty.Register(
                "Hex", typeof(string), typeof(HexColorBox),
                new FrameworkPropertyMetadata("#FF0000",
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnHexChanged));

        public string Hex
        {
            get { return (string)GetValue(HexProperty); }
            set { SetValue(HexProperty, value); }
        }

        /// <summary>
        /// Raised whenever the stored value really moves, a valid type or a pick, and not
        /// for typing that does not parse. The row wires it to the same gate every other
        /// edit reaches.
        /// </summary>
        public event EventHandler HexChanged;

        public HexColorBox()
        {
            Margin = PanelMetrics.Gap;

            _hexText = new TextBox
            {
                Width = PanelMetrics.HexWidth,
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = "A colour as hex, #FF0000 or FF0000."
            };

            _swatch = new Border
            {
                Width = PanelMetrics.Swatch,
                Height = PanelMetrics.Swatch,
                Margin = PanelMetrics.Gap,
                BorderThickness = PanelMetrics.Outline,
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = "Click to pick a colour"
            };

            StackPanel inside = new StackPanel { Orientation = Orientation.Horizontal };
            inside.Children.Add(_hexText);
            inside.Children.Add(_swatch);
            Content = inside;

            _hexText.Text = Hex ?? string.Empty;
            _hexText.TextChanged += TypedInto;
            _swatch.MouseLeftButtonUp += SwatchClicked;
            IsEnabledChanged += (sender, e) => PaintFromTheTheme();

            PaintFromTheTheme();
        }

        private static void OnHexChanged(DependencyObject changed, DependencyPropertyChangedEventArgs e)
        {
            HexColorBox box = changed as HexColorBox;
            if (box == null) return;

            if (!box._syncing)
            {
                box._syncing = true;
                box._hexText.Text = e.NewValue as string ?? string.Empty;
                box._syncing = false;
                box.PaintFromTheTheme();
            }

            box.HexChanged?.Invoke(box, EventArgs.Empty);
        }

        private void TypedInto(object sender, TextChangedEventArgs e)
        {
            if (_syncing) return;

            _syncing = true;
            Hex = HexColor.Kept(Hex, _hexText.Text);
            _syncing = false;

            PaintFromTheTheme();
        }

        private void SwatchClicked(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled) return;

            using (System.Windows.Forms.ColorDialog picking = new System.Windows.Forms.ColorDialog())
            {
                picking.FullOpen = true;
                picking.AnyColor = true;
                picking.SolidColorOnly = false;
                picking.CustomColors = _customColors;

                if (HexColor.TryParse(_hexText.Text, out byte red, out byte green, out byte blue))
                {
                    picking.Color = System.Drawing.Color.FromArgb(red, green, blue);
                }

                RevitOwner owner = new RevitOwner(
                    System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);

                if (picking.ShowDialog(owner) == System.Windows.Forms.DialogResult.OK)
                {
                    _customColors = picking.CustomColors;
                    Hex = HexColor.Written(picking.Color.R, picking.Color.G, picking.Color.B);
                }
            }
        }

        /// <summary>
        /// The square's own painter, called by the pane's theme walk as well, so a Revit
        /// theme switch reaches a control that is built once and kept. The colour itself
        /// is the user's own datum shown back, the one brush built outside PanelTheme, and
        /// it is frozen because one is made per keystroke. An empty box is nothing chosen
        /// rather than a fault, so it wears the plain hairline and only text that fails to
        /// parse gets the warning edge.
        /// </summary>
        public void PaintFromTheTheme()
        {
            PanelTheme theme = PanelTheme.Current();

            if (HexColor.TryParse(_hexText.Text, out byte red, out byte green, out byte blue))
            {
                SolidColorBrush shown = new SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(red, green, blue));
                shown.Freeze();
                _swatch.Background = shown;
                _swatch.BorderBrush = theme.Line;
            }
            else if (string.IsNullOrWhiteSpace(_hexText.Text))
            {
                _swatch.Background = theme.Clear;
                _swatch.BorderBrush = theme.Line;
            }
            else
            {
                _swatch.Background = theme.Clear;
                _swatch.BorderBrush = theme.Warning;
            }

            _swatch.Opacity = IsEnabled ? 1.0 : PanelMetrics.FadedOpacity;
        }

        private sealed class RevitOwner : System.Windows.Forms.IWin32Window
        {
            private readonly IntPtr _handle;

            public RevitOwner(IntPtr handle)
            {
                _handle = handle;
            }

            public IntPtr Handle
            {
                get { return _handle; }
            }
        }
    }
}
