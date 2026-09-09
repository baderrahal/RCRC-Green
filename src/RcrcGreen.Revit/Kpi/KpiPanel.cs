using System;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.UI;
using RcrcGreen.Core.Kpi;

namespace RcrcGreen.Revit.Kpi
{
    /// <summary>
    /// The KPI pane. Three things and no more: the model name with when it was last read, one
    /// button reading KPI Scan, and one status line. Kept that small on purpose, because this
    /// round proves the pane, its identifier, its external event and its handler in one
    /// install, with almost nothing to go wrong.
    ///
    /// Nothing in this file reads a Document or touches the Revit API. Everything it wants
    /// doing goes through <see cref="KpiRequestHandler"/> and its own external event. No
    /// brush and no spacing is written here either. Colours come from <see cref="PanelTheme"/>
    /// and every margin from <see cref="PanelMetrics"/>, both shared with the Drawing Sheet
    /// pane and changed by neither.
    /// </summary>
    internal sealed class KpiPanel : UserControl, IDockablePaneProvider
    {
        private readonly ExternalEvent _asking;
        private readonly KpiRequestHandler _handler;

        private readonly TextBlock _modelName = new TextBlock();
        private readonly TextBlock _readAt = new TextBlock();
        private readonly TextBlock _said = new TextBlock { TextWrapping = TextWrapping.Wrap };

        private readonly Border _strip = new Border();
        private readonly Border _status = new Border();

        private PanelTheme _theme = PanelTheme.Current();

        // The title the read line and the status line describe. Null until a scan. The name
        // and the read line used to be set by two callbacks, so model B's name sat over model
        // A's read line after a document switch.
        private string _scannedTitle;

        public KpiPanel()
        {
            _handler = new KpiRequestHandler
            {
                Named = Took,
                Scanned = Scanned,
                Told = Say
            };
            _asking = ExternalEvent.Create(_handler);

            Content = Layout();
            PaintFromTheTheme();

            _modelName.Text = KpiPaneWords.NoModelName;
            _readAt.Text = KpiPaneWords.NotScanned;
            Say(KpiPaneWords.Waiting);

            // Not the constructor. A dockable pane is built during OnStartup, when no document
            // exists, so the first read has to wait for the pane to be put on screen.
            IsVisibleChanged += (sender, e) => Shown();
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this;
            data.InitialState = new DockablePaneState
            {
                DockPosition = DockPosition.Right
            };
        }

        /// <summary>
        /// Asks for the model name every time the pane is shown, because a second document
        /// can have been opened while it sat closed. The theme is re-read at the same moment.
        /// </summary>
        private void Shown()
        {
            if (!IsVisible) return;

            PaintFromTheTheme();
            Ask(KpiRequest.WhichModel);
        }

        private void PaintFromTheTheme()
        {
            _theme = PanelTheme.Current();

            // Set once, here. Foreground is inherited, so every TextBlock below picks it up.
            // The strip and the status bar are repainted by hand because a colour set once,
            // before the theme it follows is read, is how the Drawing Sheet came up black on
            // black the first time.
            Background = _theme.Background;
            Foreground = _theme.Foreground;
            FontSize = PanelMetrics.Body;

            _strip.Background = _theme.Strip;
            _status.Background = _theme.Strip;
            _status.BorderBrush = _theme.Line;
            _readAt.Foreground = _theme.Faint;
        }

        /// <summary>
        /// The strip at the top, the status line at the bottom, and nothing between them yet.
        /// The buttons this ribbon panel will carry later each get their own controls here
        /// when they arrive, and an empty middle now is honest about that.
        /// </summary>
        private UIElement Layout()
        {
            var everything = new DockPanel { LastChildFill = true };

            UIElement strip = Strip();
            DockPanel.SetDock(strip, Dock.Top);
            everything.Children.Add(strip);

            UIElement status = Status();
            DockPanel.SetDock(status, Dock.Bottom);
            everything.Children.Add(status);

            everything.Children.Add(new Border { Margin = PanelMetrics.Edge });

            return everything;
        }

        private UIElement Strip()
        {
            var inside = new DockPanel { Margin = PanelMetrics.StripInside, LastChildFill = true };

            var scan = new Button
            {
                Content = "KPI Scan",
                Margin = PanelMetrics.Gap,
                Padding = PanelMetrics.CellPad,
                ToolTip = KpiPaneWords.ReadOnly
            };
            scan.Click += (sender, e) => AskedForAScan();
            DockPanel.SetDock(scan, Dock.Right);
            inside.Children.Add(scan);

            var named = new StackPanel();
            _modelName.FontWeight = FontWeights.Bold;
            _modelName.TextTrimming = TextTrimming.CharacterEllipsis;
            named.Children.Add(_modelName);
            named.Children.Add(_readAt);
            inside.Children.Add(named);

            _strip.Child = inside;
            return _strip;
        }

        private UIElement Status()
        {
            _status.BorderThickness = PanelMetrics.HairlineAbove;
            _status.Child = new Border { Margin = PanelMetrics.StripInside, Child = _said };
            return _status;
        }

        private void Ask(KpiRequest wanted)
        {
            _handler.Ask(wanted);
            _asking.Raise();
        }

        private void AskedForAScan()
        {
            Say(KpiPaneWords.Scanning);
            Ask(KpiRequest.Scan);
        }

        /// <summary>
        /// The handler calls these from the Revit thread, so the hop to the pane's own thread
        /// happens here rather than being forgotten at each call site.
        /// </summary>
        private void Took(string documentTitle)
        {
            Dispatcher.Invoke(() =>
            {
                _modelName.Text = KpiPaneWords.ModelNamed(documentTitle);

                if (!string.Equals(documentTitle, _scannedTitle, StringComparison.Ordinal))
                {
                    _scannedTitle = null;
                    _readAt.Text = KpiPaneWords.NotScanned;
                    if (!string.IsNullOrEmpty(documentTitle)) _said.Text = KpiPaneWords.NotScanned;
                }
            });
        }

        private void Scanned(KpiScan scan, DateTime readAt)
        {
            Dispatcher.Invoke(() =>
            {
                _scannedTitle = scan.Document.Title;
                _modelName.Text = KpiPaneWords.ModelNamed(scan.Document.Title);
                _readAt.Text = KpiPaneWords.ReadAt(readAt, scan.Document.ElementInstances, scan.Document.ReadSeconds);
            });
        }

        private void Say(string what)
        {
            Dispatcher.Invoke(() => _said.Text = what ?? string.Empty);
        }
    }
}
