using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RcrcGreen.Revit.Kpi
{
    /// <summary>
    /// Every colour, size and drawn shape the KPI pane uses, Bader's approved design of
    /// October. **None of them is written in KpiPanel.cs**, the same rule PanelTheme and
    /// PanelMetrics hold for the Drawing Sheet: a colour typed at the control that uses it is
    /// how forty numbers came to need finding when the rhythm changed.
    ///
    /// **ONE SET OF COLOURS FOR BOTH REVIT THEMES, AND THE PANE PAINTS EVERY ONE.** The design
    /// gives light colours only. The pane sets its own background and every foreground rather
    /// than inheriting Revit's, because a pane that inherited the dark theme came up black on
    /// black once. Whether the dark theme gets its own set is a question for Bader.
    ///
    /// **THE TICK AND THE WARNING TRIANGLE ARE DRAWN AS GEOMETRY**, never typed. The writing
    /// hook refuses the code point range both characters sit in.
    /// </summary>
    internal static class KpiTheme
    {
        public static readonly Brush Green = Frozen(0x1F, 0x6B, 0x3A);
        public static readonly Brush GreenHover = Frozen(0x18, 0x57, 0x2F);
        public static readonly Brush GreenTint = Frozen(0xE6, 0xF0, 0xE9);
        public static readonly Brush Text = Frozen(0x1B, 0x1F, 0x1C);
        public static readonly Brush Muted = Frozen(0x5B, 0x63, 0x5D);
        public static readonly Brush InputBorder = Frozen(0xC9, 0xCF, 0xCA);
        public static readonly Brush Divider = Frozen(0xE3, 0xE7, 0xE4);
        public static readonly Brush InputFill = Frozen(0xFA, 0xFB, 0xFA);
        public static readonly Brush Background = Frozen(0xFF, 0xFF, 0xFF);
        public static readonly Brush OnGreen = Frozen(0xFF, 0xFF, 0xFF);
        public static readonly Brush Clear = Brushes.Transparent;

        public static readonly Brush WarningFill = Frozen(0xFF, 0xF4, 0xDB);
        public static readonly Brush WarningBorder = Frozen(0xEB, 0xCB, 0x80);
        public static readonly Brush WarningText = Frozen(0x4A, 0x32, 0x00);

        public static readonly Brush ErrorFill = Frozen(0xFB, 0xE9, 0xE9);
        public static readonly Brush ErrorBorder = Frozen(0xE2, 0xB4, 0xB4);
        public static readonly Brush ErrorText = Frozen(0x7A, 0x1F, 0x1F);
        public static readonly Brush FailedCount = Frozen(0xA1, 0x2B, 0x2B);

        // The design offered the error border or the failed count's red for a path box that is
        // not set. The error border is too faint against a white pane to read as a fault on a
        // 1 pixel line, so the stronger red is the one used.
        public static readonly Brush MissingPathBorder = FailedCount;

        public static readonly FontFamily Body = new FontFamily("Segoe UI");
        public static readonly FontFamily Mono = new FontFamily("Consolas");

        public const double BodySize = 13.0;
        public const double LabelSize = 12.0;
        public const double SectionSize = 11.0;
        public const double StatusSize = 11.0;
        public const double TitleSize = 16.0;
        public const double BigNumberSize = 28.0;
        public const double CountSize = 20.0;
        public const double StepNameSize = 11.0;

        // Section titles have no letter spacing property in WPF, so each glyph is its own text
        // run with this much room after it.
        public const double Tracking = 0.8;

        public const double Circle = 24.0;
        public const double InputHeight = 32.0;
        public const double MainButtonHeight = 40.0;
        public const double Corner = 4.0;
        public const double SidePad = 16.0;
        public const double GroupGap = 14.0;
        public const double RowGap = 6.0;
        public const double SmallGap = 4.0;
        public const double PlotListHeight = 260.0;
        public const double BoxPad = 8.0;
        public const double IconSize = 16.0;
        public const double TickSize = 12.0;
        public const double DisabledOpacity = 0.55;
        public const double SmallButtonHeight = 24.0;
        public const double LabelWidth = 84.0;
        public const double IdWidth = 64.0;
        public const double RangeWidth = 128.0;
        public const double CountWidth = 52.0;
        public const double TooltipWidth = 420.0;

        public const int StepColumns = 4;
        public const int AssetColumns = 2;
        public const int ReadBoxes = 3;
        public const int ResultBoxes = 2;
        public const int ResultButtons = 2;
        public const int DetailsColumns = 2;

        public static readonly Thickness Pane = new Thickness(SidePad, 0, SidePad, 0);
        public static readonly Thickness Header = new Thickness(SidePad, 12, SidePad, 8);
        public static readonly Thickness StepRow = new Thickness(SidePad - 4, 4, SidePad - 4, 10);
        public static readonly Thickness Footer = new Thickness(SidePad, 10, SidePad, 10);
        public static readonly Thickness Group = new Thickness(0, GroupGap, 0, 0);
        public static readonly Thickness SectionTitle = new Thickness(0, GroupGap, 0, RowGap);
        public static readonly Thickness Row = new Thickness(0, 0, 0, RowGap);
        public static readonly Thickness Hint = new Thickness(0, 2, 0, RowGap);
        public static readonly Thickness LeftGap = new Thickness(SmallGap, 0, 0, 0);
        public static readonly Thickness RightGap = new Thickness(0, 0, SmallGap, 0);
        public static readonly Thickness Box = new Thickness(BoxPad);
        public static readonly Thickness BoxRow = new Thickness(0, 0, 0, RowGap);
        public static readonly Thickness CountBox = new Thickness(0, 0, SmallGap, 0);
        public static readonly Thickness ButtonPad = new Thickness(10, 0, 10, 0);
        public static readonly Thickness InputPad = new Thickness(8, 0, 8, 0);
        public static readonly Thickness Hairline = new Thickness(1);
        public static readonly Thickness LineAbove = new Thickness(0, 1, 0, 0);
        public static readonly Thickness LineBelow = new Thickness(0, 0, 0, 1);
        public static readonly Thickness LetterGap = new Thickness(0, 0, Tracking, 0);
        public const double RingThickness = 1.0;
        public static readonly Thickness StatusGap = new Thickness(0, 6, 0, 0);
        public static readonly Thickness IconGap = new Thickness(0, 0, 8, 0);
        public static readonly Thickness BottomSpace = new Thickness(0, 0, 0, GroupGap);
        public static readonly Thickness Nothing = new Thickness(0);

        public static readonly CornerRadius Rounded = new CornerRadius(Corner);
        public static readonly CornerRadius Round = new CornerRadius(Circle / 2.0);

        /// <summary>A tick, in a 12 by 12 box.</summary>
        public static readonly Geometry TickShape = Freeze(Geometry.Parse("M 1.5,6.5 L 4.8,9.6 L 10.5,2.5"));

        /// <summary>A warning triangle, with its bar and its dot, in a 16 by 16 box.</summary>
        public static readonly Geometry TriangleShape = Freeze(Geometry.Parse("M 8,1.5 L 15,14.5 L 1,14.5 Z"));
        public static readonly Geometry TriangleBar = Freeze(Geometry.Parse("M 8,6 L 8,10"));
        public static readonly Geometry TriangleDot = Freeze(Geometry.Parse("M 8,12 L 8,12.6"));

        public static UIElement Tick(Brush colour)
        {
            return new Path
            {
                Data = TickShape,
                Stroke = colour,
                StrokeThickness = 1.8,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                Width = TickSize,
                Height = TickSize,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
        }

        public static UIElement Triangle(Brush colour)
        {
            var drawn = new Grid { Width = IconSize, Height = IconSize, VerticalAlignment = VerticalAlignment.Top };
            foreach (Geometry shape in new[] { TriangleShape, TriangleBar, TriangleDot })
            {
                drawn.Children.Add(new Path
                {
                    Data = shape,
                    Stroke = colour,
                    StrokeThickness = 1.4,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round,
                    StrokeLineJoin = PenLineJoin.Round
                });
            }

            return drawn;
        }

        /// <summary>The full width green button the footer holds.</summary>
        public static Button Main(object content)
        {
            var button = Styled(content, Green, OnGreen, Green, MainButtonHeight);
            button.FontWeight = FontWeights.SemiBold;
            button.HorizontalAlignment = HorizontalAlignment.Stretch;
            button.Template = MainTemplate;
            return button;
        }

        /// <summary>The full width plain button, New run.</summary>
        public static Button Plain(object content)
        {
            var button = Styled(content, Background, Text, InputBorder, MainButtonHeight);
            button.HorizontalAlignment = HorizontalAlignment.Stretch;
            button.Template = PlainTemplate;
            return button;
        }

        /// <summary>A 32 pixel button, Browse, All, None and their kind.</summary>
        public static Button Small(object content)
        {
            var button = Styled(content, Background, Text, InputBorder, InputHeight);
            button.Template = PlainTemplate;
            return button;
        }

        /// <summary>A button that reads as a link, Show in report and Tick list again.</summary>
        public static Button Link(object content)
        {
            var button = Styled(content, Clear, Green, Clear, double.NaN);
            button.Padding = Nothing;
            button.Cursor = System.Windows.Input.Cursors.Hand;
            button.HorizontalAlignment = HorizontalAlignment.Left;
            button.Template = LinkTemplate;
            return button;
        }

        /// <summary>A text box with the design's corners, fill and border.</summary>
        public static TextBox Input()
        {
            return new TextBox
            {
                Height = InputHeight,
                Background = InputFill,
                Foreground = Text,
                BorderBrush = InputBorder,
                BorderThickness = Hairline,
                Padding = InputPad,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontSize = BodySize,
                Template = InputTemplate
            };
        }

        /// <summary>
        /// A dropdown 32 pixels high. **The corners are square**: the system's own combo box
        /// draws its button half with brushes of its own, and replacing that template is a
        /// popup, a toggle and a selection box rebuilt for one control.
        /// </summary>
        public static ComboBox Dropdown()
        {
            var items = new FrameworkElementFactory(typeof(TextBlock));
            items.SetBinding(TextBlock.TextProperty, new Binding());

            return ShowsWhyWhenOff(new ComboBox
            {
                Height = InputHeight,
                Background = InputFill,
                Foreground = Text,
                BorderBrush = InputBorder,
                VerticalContentAlignment = VerticalAlignment.Center,
                Padding = InputPad,
                FontSize = BodySize,

                // A TextBlock template so a name like PRX_Plot_UID2 is shown as written. A
                // bare string would go through a content presenter that may read the first
                // underscore as an access key, which once showed five names no model holds.
                ItemTemplate = new DataTemplate { VisualTree = items }
            });
        }

        private static Button Styled(object content, Brush fill, Brush text, Brush edge, double height)
        {
            return ShowsWhyWhenOff(new Button
            {
                Content = content,
                Background = fill,
                Foreground = text,
                BorderBrush = edge,
                BorderThickness = Hairline,
                Height = height,
                Padding = ButtonPad,
                FontSize = BodySize,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            });
        }

        /// <summary>
        /// **A GREYED CONTROL STILL SAYS WHY.** WPF shows no tooltip on a disabled control
        /// unless told to, so every button, tick box and dropdown this pane can grey carries
        /// its reason through this one call rather than a line at each control.
        /// </summary>
        public static T ShowsWhyWhenOff<T>(T control) where T : FrameworkElement
        {
            ToolTipService.SetShowOnDisabled(control, true);
            return control;
        }

        private static readonly ControlTemplate MainTemplate = ButtonTemplate(GreenHover, InputBorder, OnGreen, true);
        private static readonly ControlTemplate PlainTemplate = ButtonTemplate(GreenTint, InputFill, Muted, false);
        private static readonly ControlTemplate LinkTemplate = LinkButtonTemplate();
        private static readonly ControlTemplate InputTemplate = TextBoxTemplate();

        /// <summary>
        /// A rounded box round the content. Hover tints it, and a disabled button takes the
        /// muted look rather than the system grey, which would sit wrong on this white pane.
        /// </summary>
        private static ControlTemplate ButtonTemplate(Brush hover, Brush disabledFill, Brush disabledText, bool solid)
        {
            var box = new FrameworkElementFactory(typeof(Border)) { Name = "Box" };
            box.SetValue(Border.CornerRadiusProperty, Rounded);
            box.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Control.BackgroundProperty));
            box.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Control.BorderBrushProperty));
            box.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Control.BorderThicknessProperty));

            var inside = new FrameworkElementFactory(typeof(ContentPresenter));
            inside.SetValue(ContentPresenter.MarginProperty, new TemplateBindingExtension(Control.PaddingProperty));
            inside.SetValue(FrameworkElement.HorizontalAlignmentProperty, new TemplateBindingExtension(Control.HorizontalContentAlignmentProperty));
            inside.SetValue(FrameworkElement.VerticalAlignmentProperty, new TemplateBindingExtension(Control.VerticalContentAlignmentProperty));
            inside.SetValue(ContentPresenter.RecognizesAccessKeyProperty, true);
            box.AppendChild(inside);

            var template = new ControlTemplate(typeof(Button)) { VisualTree = box };

            var over = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            over.Setters.Add(new Setter(Border.BackgroundProperty, hover, "Box"));
            template.Triggers.Add(over);

            var off = new Trigger { Property = UIElement.IsEnabledProperty, Value = false };
            off.Setters.Add(new Setter(Border.BackgroundProperty, disabledFill, "Box"));
            off.Setters.Add(new Setter(Control.ForegroundProperty, disabledText));
            if (solid) off.Setters.Add(new Setter(Border.BorderBrushProperty, disabledFill, "Box"));
            template.Triggers.Add(off);

            template.Seal();
            return template;
        }

        private static ControlTemplate LinkButtonTemplate()
        {
            var inside = new FrameworkElementFactory(typeof(ContentPresenter));
            inside.SetValue(ContentPresenter.RecognizesAccessKeyProperty, true);

            var template = new ControlTemplate(typeof(Button)) { VisualTree = inside };

            var off = new Trigger { Property = UIElement.IsEnabledProperty, Value = false };
            off.Setters.Add(new Setter(Control.ForegroundProperty, Muted));
            template.Triggers.Add(off);

            template.Seal();
            return template;
        }

        private static ControlTemplate TextBoxTemplate()
        {
            var box = new FrameworkElementFactory(typeof(Border)) { Name = "Box" };
            box.SetValue(Border.CornerRadiusProperty, Rounded);
            box.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Control.BackgroundProperty));
            box.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Control.BorderBrushProperty));
            box.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Control.BorderThicknessProperty));

            // The name is the one TextBoxBase looks for to put its text in.
            var host = new FrameworkElementFactory(typeof(ScrollViewer)) { Name = "PART_ContentHost" };
            host.SetValue(FrameworkElement.MarginProperty, new TemplateBindingExtension(Control.PaddingProperty));
            host.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            host.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
            host.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
            box.AppendChild(host);

            var template = new ControlTemplate(typeof(TextBox)) { VisualTree = box };

            var focused = new Trigger { Property = UIElement.IsKeyboardFocusWithinProperty, Value = true };
            focused.Setters.Add(new Setter(Border.BorderBrushProperty, Green, "Box"));
            template.Triggers.Add(focused);

            template.Seal();
            return template;
        }

        private static Brush Frozen(byte red, byte green, byte blue)
        {
            var brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
            brush.Freeze();
            return brush;
        }

        private static Geometry Freeze(Geometry shape)
        {
            shape.Freeze();
            return shape;
        }
    }
}
