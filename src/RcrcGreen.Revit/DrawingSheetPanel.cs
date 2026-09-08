using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.UI;
using RcrcGreen.Core;

// Autodesk.Revit.UI carries a ComboBox of its own for the ribbon. This panel is WPF, so the
// name is pinned to the one that belongs in a UserControl.
using ComboBox = System.Windows.Controls.ComboBox;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// The Drawing Sheet panel. Built in C# rather than XAML, because an SDK style net48
    /// project has no XAML compilation step.
    ///
    /// Nothing in this file reads a Document, opens a Transaction or touches the Revit API.
    /// Everything it wants doing goes through <see cref="DrawingSheetRequestHandler"/> and the
    /// external event. That is the rule the whole panel is built around.
    ///
    /// No brush is written here either. Every colour comes from <see cref="PanelTheme"/>,
    /// because the first install came up black on black on Revit's dark theme.
    /// </summary>
    internal sealed class DrawingSheetPanel : UserControl, IDockablePaneProvider
    {
        /// <summary>
        /// One character per cell rather than a word. Eighteen plots by several columns of
        /// "exists" and "missing" is a wall of text with no shape to it. The shape differs as
        /// well as the fill, so the three states are still apart at a glance. The words stay in
        /// the tooltip.
        /// </summary>
        private const string ExistsMark = "\u25A0";

        private const string MissingMark = "\u25A1";

        private const string MarkedMark = "\u25CF";

        private readonly ExternalEvent _asking;
        private readonly DrawingSheetRequestHandler _handler;

        private readonly ComboBox _prefix = new ComboBox { Margin = new Thickness(0, 2, 0, 2) };
        private readonly ComboBox _from = new ComboBox { Margin = new Thickness(0, 2, 0, 2) };
        private readonly ComboBox _to = new ComboBox { Margin = new Thickness(0, 2, 0, 2) };
        private readonly TextBlock _rangeCount = new TextBlock { Margin = new Thickness(0, 4, 0, 4), TextWrapping = TextWrapping.Wrap };
        private readonly TextBlock _said = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 0) };
        private readonly TextBlock _insteadOfTheGrid = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(4, 8, 4, 8) };
        private readonly TextBlock _columnCount = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 2) };
        private readonly TextBlock _caseCounts = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 4) };
        private readonly Grid _sheet = new Grid();
        private readonly StackPanel _columnList = new StackPanel { Margin = new Thickness(4, 2, 0, 2) };
        private readonly Expander _columnBox = new Expander { Header = "View types", Margin = new Thickness(0, 2, 0, 2) };
        private readonly Button _assign = new Button { Content = "Assign Scope Boxes", Margin = new Thickness(0, 4, 0, 2) };

        private PanelTheme _theme = PanelTheme.Current();
        private DrawingSheetSnapshot _model = DrawingSheetSnapshot.Nothing;
        private GridColumns _columns = GridColumns.ShowingAll(null);
        private PlotSelection _picked = PlotSelection.Nothing;
        private readonly HashSet<PlotViewKey> _marked = new HashSet<PlotViewKey>();
        private bool _filling;
        private bool _readOnce;

        public DrawingSheetPanel()
        {
            _handler = new DrawingSheetRequestHandler
            {
                Read = Took,
                Told = Say
            };
            _asking = ExternalEvent.Create(_handler);

            Content = Layout();
            PaintFromTheTheme();
            WhenRangeIsSet(false);
            Waiting("Open a model. This panel reads it as soon as it is shown.");

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
        /// Reads the model every time the pane is shown.
        ///
        /// It used to return early once it had read anything, so opening a second document left
        /// the first one's plots on screen with nothing saying so. There is no way for the panel
        /// to know from the outside that the document changed, so the only safe answer is to
        /// read again, and the read is fast enough that it costs nothing to.
        ///
        /// The theme is re-read at the same moment, because the user can switch Revit between
        /// light and dark while the pane sits closed.
        /// </summary>
        private void Shown()
        {
            if (!IsVisible) return;

            PaintFromTheTheme();
            AskedForARefresh();
        }

        private void PaintFromTheTheme()
        {
            _theme = PanelTheme.Current();

            // Set once, here. Foreground is an inherited property, so every TextBlock below
            // picks it up and none of them names a colour of its own.
            Background = _theme.Background;
            Foreground = _theme.Foreground;

            // Redraws so the no scope box mark takes the new theme's red rather than the one
            // it was given when the panel was built.
            if (_readOnce) DrawSheet();
        }

        private UIElement Layout()
        {
            var everything = new StackPanel { Margin = new Thickness(8) };

            everything.Children.Add(Heading("PLOT RANGE"));
            everything.Children.Add(Labelled("Prefix", _prefix));
            everything.Children.Add(Labelled("From", _from));
            everything.Children.Add(Labelled("To", _to));
            everything.Children.Add(_rangeCount);

            _prefix.SelectionChanged += (sender, e) => PrefixChosen();
            _from.SelectionChanged += (sender, e) => RangeChosen();
            _to.SelectionChanged += (sender, e) => RangeChosen();

            everything.Children.Add(Heading("COLUMNS"));
            _columnBox.Content = new ScrollViewer
            {
                Content = _columnList,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 200
            };
            everything.Children.Add(_columnBox);
            everything.Children.Add(_columnCount);

            everything.Children.Add(Heading("PLOTS AND VIEW TYPES"));

            // The grid and the line that stands in for it share one slot, so an empty grid is
            // never a blank area with nothing to read.
            var sheetOrReason = new Grid();
            sheetOrReason.Children.Add(_sheet);
            sheetOrReason.Children.Add(_insteadOfTheGrid);

            everything.Children.Add(new ScrollViewer
            {
                Content = sheetOrReason,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 420,
                Margin = new Thickness(0, 2, 0, 6)
            });

            everything.Children.Add(Heading("SCOPE BOXES"));
            everything.Children.Add(_caseCounts);
            _assign.Click += (sender, e) => AskToAssign();
            everything.Children.Add(_assign);

            everything.Children.Add(Heading("MODEL"));

            var refresh = new Button { Content = "Refresh", Margin = new Thickness(0, 2, 0, 2) };
            refresh.Click += (sender, e) => AskedForARefresh();
            everything.Children.Add(refresh);

            var scan = new Button
            {
                Content = "Scan Model",
                Margin = new Thickness(0, 2, 0, 2),
                ToolTip = "Reads the whole document and writes a report to the Desktop. It reads "
                    + "everything rather than the range, so it is the check this panel is "
                    + "measured against."
            };
            scan.Click += (sender, e) => AskedForAScan();
            everything.Children.Add(scan);

            everything.Children.Add(_said);

            return new ScrollViewer
            {
                Content = everything,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
        }

        private static TextBlock Heading(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 10, 0, 4)
            };
        }

        private static UIElement Labelled(string label, UIElement control)
        {
            var line = new DockPanel();
            var caption = new TextBlock { Text = label, Width = 52, VerticalAlignment = VerticalAlignment.Center };
            DockPanel.SetDock(caption, Dock.Left);
            line.Children.Add(caption);
            line.Children.Add(control);
            return line;
        }

        private void Ask(DrawingSheetRequest wanted)
        {
            _handler.Ask(wanted);
            _asking.Raise();
        }

        private void AskedForARefresh()
        {
            Say("Reading the model.");
            Ask(DrawingSheetRequest.Refresh);
        }

        private void AskedForAScan()
        {
            Say("Scanning the whole model. This one reads every element, so it takes longer.");
            Ask(DrawingSheetRequest.ScanModel);
        }

        private void AskToAssign()
        {
            IReadOnlyList<string> ticked = _picked.Ticked;
            if (ticked.Count == 0)
            {
                Say("No plots are ticked, so there is nothing to assign.");
                return;
            }

            _handler.AskToAssign(ticked);
            _asking.Raise();
        }

        /// <summary>
        /// The handler calls this from the Revit thread, so the hop to the panel's own thread
        /// happens here rather than being forgotten at each call site.
        ///
        /// The prefix, the first plot and the last plot are put back afterwards when the model
        /// still holds them. A refresh that emptied all three left the user unable to see what
        /// had changed, which is most of why the last one looked broken.
        /// </summary>
        private void Took(DrawingSheetSnapshot snapshot)
        {
            Dispatcher.Invoke(() =>
            {
                string prefixWas = _prefix.SelectedItem as string;
                string fromWas = _from.SelectedItem as string;
                string toWas = _to.SelectedItem as string;

                _model = snapshot ?? DrawingSheetSnapshot.Nothing;
                _readOnce = true;
                _marked.Clear();
                _columns = _columns.OverTheseTypes(_model.ViewTypes);

                FillPrefixes();
                FillColumnList();
                PutTheRangeBack(prefixWas, fromWas, toWas);
            });
        }

        private void Say(string what)
        {
            Dispatcher.Invoke(() => _said.Text = what ?? string.Empty);
        }

        /// <summary>
        /// What stands where the grid would be. Every path that leaves the grid empty comes
        /// through here, so there is no state in which the panel shows a blank area and says
        /// nothing about why.
        /// </summary>
        private void Waiting(string why)
        {
            _insteadOfTheGrid.Text = why;
            _insteadOfTheGrid.Visibility = Visibility.Visible;
            _sheet.Visibility = Visibility.Collapsed;
        }

        private void ShowingTheGrid()
        {
            _insteadOfTheGrid.Visibility = Visibility.Collapsed;
            _sheet.Visibility = Visibility.Visible;
        }

        private void FillPrefixes()
        {
            _filling = true;

            _prefix.Items.Clear();
            foreach (string prefix in PlotRange.PrefixesIn(_model.PlotIds))
            {
                _prefix.Items.Add(prefix);
            }

            _from.Items.Clear();
            _to.Items.Clear();

            _filling = false;
        }

        /// <summary>
        /// Puts back what the user had picked before the refresh, when the model still holds
        /// it. A plot that has gone is not put back, and then the range is simply unset.
        /// </summary>
        private void PutTheRangeBack(string prefixWas, string fromWas, string toWas)
        {
            if (prefixWas != null && _prefix.Items.Contains(prefixWas))
            {
                // Triggers PrefixChosen, which refills From and To and picks both ends.
                _prefix.SelectedItem = prefixWas;

                _filling = true;
                if (fromWas != null && _from.Items.Contains(fromWas)) _from.SelectedItem = fromWas;
                if (toWas != null && _to.Items.Contains(toWas)) _to.SelectedItem = toWas;
                _filling = false;
            }

            RangeChosen();
        }

        private void FillColumnList()
        {
            _columnList.Children.Clear();

            foreach (ViewType viewType in _columns.All)
            {
                ViewType which = viewType;
                var tick = new CheckBox
                {
                    Content = which.ToString(),
                    IsChecked = _columns.IsShown(which),
                    Margin = new Thickness(0, 1, 0, 1)
                };

                tick.Checked += (sender, e) => ColumnShown(which, true);
                tick.Unchecked += (sender, e) => ColumnShown(which, false);
                _columnList.Children.Add(tick);
            }

            SayTheColumns();
        }

        private void ColumnShown(ViewType which, bool shown)
        {
            if (_filling) return;

            _columns = _columns.Showing(which, shown);
            SayTheColumns();
            DrawSheet();
        }

        private void SayTheColumns()
        {
            int hidden = _columns.HiddenCount;
            _columnBox.Header = _columns.All.Count + " view types";
            _columnCount.Text = hidden == 0
                ? _columns.All.Count + " view types, all shown."
                : _columns.Shown.Count + " of " + _columns.All.Count + " view types shown, "
                    + hidden + " hidden.";
        }

        private void PrefixChosen()
        {
            if (_filling) return;

            string prefix = _prefix.SelectedItem as string;
            IReadOnlyList<string> under = PlotRange.WithPrefix(_model.PlotIds, prefix);

            _filling = true;

            _from.Items.Clear();
            _to.Items.Clear();
            foreach (string plotId in under)
            {
                _from.Items.Add(plotId);
                _to.Items.Add(plotId);
            }

            if (under.Count > 0)
            {
                // Both ends default to the whole prefix, so one click on a prefix already
                // shows something rather than an empty grid waiting on two more clicks.
                _from.SelectedIndex = 0;
                _to.SelectedIndex = under.Count - 1;
            }

            _filling = false;

            RangeChosen();
        }

        /// <summary>
        /// The range changing resets every tick to on, which is the rule. Somebody who has just
        /// moved to a different block of plots is not still excluding two from the last one.
        /// </summary>
        private void RangeChosen()
        {
            if (_filling) return;

            _picked = PlotSelection.AllOf(PlotsInRange());

            SayTheRange();
            WhenRangeIsSet(_picked.InRangeCount > 0);
            DrawSheet();
        }

        private IReadOnlyList<string> PlotsInRange()
        {
            return PlotRange.Between(
                _model.PlotIds,
                _prefix.SelectedItem as string,
                _from.SelectedItem as string,
                _to.SelectedItem as string);
        }

        /// <summary>
        /// The line directly above the grid, so what it says about the range sits next to the
        /// grid it describes.
        /// </summary>
        private void SayTheRange()
        {
            if (_model.Empty)
            {
                _rangeCount.Text = "No plots in this model.";
                return;
            }

            _rangeCount.Text = _picked.TickedCount + " of " + _picked.InRangeCount
                + " plots in range ticked, " + _model.PlotIds.Count + " in the model.";
        }

        /// <summary>
        /// Nothing below the range is usable until a range exists, because every one of those
        /// controls acts on the plots in it.
        /// </summary>
        private void WhenRangeIsSet(bool ready)
        {
            _sheet.IsEnabled = ready;
            _assign.IsEnabled = ready;
            _columnBox.IsEnabled = ready;
        }

        private void PlotTicked(string plotId, bool ticked)
        {
            if (_filling) return;

            _picked = _picked.Ticking(plotId, ticked);
            SayTheRange();
            SayTheCases();
            DrawSheet();
        }

        /// <summary>
        /// The six cases for the ticked plots, worked out from the snapshot with no trip to
        /// Revit, so they follow a tick straight away. What Assign does is decided again from a
        /// fresh read, but by the same rule over the same plots.
        /// </summary>
        private void SayTheCases()
        {
            if (!_readOnce || _picked.TickedCount == 0)
            {
                _caseCounts.Text = "Tick at least one plot to see what Assign would do.";
                return;
            }

            ScopeBoxCounts counts = ScopeBoxCounts.For(
                _model.ViewStates, _model.ScopeBoxNames, _picked.Ticked);

            _caseCounts.Text =
                counts.Considered + " views across " + _picked.TickedCount + " ticked plots."
                + Environment.NewLine
                + "A, name does not parse: " + counts.Of(ScopeBoxCase.NameDoesNotParse)
                + Environment.NewLine
                + "B, cannot hold a scope box: " + counts.Of(ScopeBoxCase.CannotHoldAScopeBox)
                + Environment.NewLine
                + "C, ready to assign: " + counts.ReadyToAssign
                + Environment.NewLine
                + "D, no scope box for that plot: " + counts.Of(ScopeBoxCase.NoMatchingScopeBox)
                + Environment.NewLine
                + "E, already right: " + counts.Of(ScopeBoxCase.AlreadyRight)
                + Environment.NewLine
                + "F, holds a different scope box: " + counts.Of(ScopeBoxCase.HoldsADifferentScopeBox)
                + Environment.NewLine
                + "Only C is written.";
        }

        private void DrawSheet()
        {
            _sheet.Children.Clear();
            _sheet.ColumnDefinitions.Clear();
            _sheet.RowDefinitions.Clear();

            SayTheCases();

            SheetGrid grid = SheetGrid.Build(
                _picked.InRange, _columns.Shown, _model.Present, _model.PlotsWithAScopeBox, _marked);

            if (grid.Rows.Count == 0)
            {
                Waiting(NothingToDraw());
                return;
            }

            ShowingTheGrid();

            // A tick column, then the plot name, then one per view type.
            _sheet.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            _sheet.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            foreach (ViewType ignored in grid.Columns)
            {
                _sheet.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }

            _sheet.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Place(new TextBlock { Text = "Use", FontWeight = FontWeights.Bold, Margin = new Thickness(4, 2, 4, 2) }, 0, 0);
            Place(new TextBlock { Text = "Plot", FontWeight = FontWeights.Bold, Margin = new Thickness(4, 2, 8, 2) }, 0, 1);

            for (int column = 0; column < grid.Columns.Count; column++)
            {
                Place(
                    new TextBlock
                    {
                        Text = grid.Columns[column].ToString(),
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(4, 2, 8, 2),
                        MaxWidth = 160,
                        TextWrapping = TextWrapping.Wrap
                    },
                    0,
                    column + 2);
            }

            for (int row = 0; row < grid.Rows.Count; row++)
            {
                SheetGridRow line = grid.Rows[row];
                bool ticked = _picked.IsTicked(line.PlotId);
                _sheet.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                Place(TickFor(line.PlotId, ticked), row + 1, 0);

                // The mark says the plot has no scope box named for it. A view without one is
                // useless on this project, so it belongs on the row label. Only that case names
                // a colour. Everything else inherits the panel's.
                var label = new TextBlock
                {
                    Text = line.HasScopeBox ? line.PlotId : line.PlotId + "  no scope box",
                    Margin = new Thickness(4, 2, 8, 2),
                    Opacity = ticked ? 1.0 : 0.45
                };
                if (!line.HasScopeBox) label.Foreground = _theme.Warning;

                Place(label, row + 1, 1);

                for (int column = 0; column < line.Cells.Count; column++)
                {
                    Place(CellButton(line.PlotId, line.Cells[column], ticked), row + 1, column + 2);
                }
            }
        }

        private CheckBox TickFor(string plotId, bool ticked)
        {
            string which = plotId;
            var tick = new CheckBox
            {
                IsChecked = ticked,
                Margin = new Thickness(4, 3, 4, 3),
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = "Untick " + which + " to leave it out of the scope box counts and out "
                    + "of anything that writes."
            };

            tick.Checked += (sender, e) => PlotTicked(which, true);
            tick.Unchecked += (sender, e) => PlotTicked(which, false);
            return tick;
        }

        /// <summary>
        /// Why the grid has no rows, in the words the user needs rather than a blank area.
        /// </summary>
        private string NothingToDraw()
        {
            if (!_readOnce) return "Reading the model.";

            if (_model.Empty)
            {
                return "No plots in this model. No view carries a PRX_Plot_ID and no view name "
                    + "gives one. Open the model you meant and press Refresh.";
            }

            if (_prefix.SelectedItem == null)
            {
                return "Pick a prefix above. From and To fill themselves with the plots under "
                    + "it, and the grid follows.";
            }

            return "No plots in that range. Widen From and To.";
        }

        private Button CellButton(string plotId, SheetGridCell cell, bool ticked)
        {
            var button = new Button
            {
                Content = Face(cell.State),
                FontSize = 14,
                Margin = new Thickness(1),
                MinWidth = 30,
                Opacity = ticked ? 1.0 : 0.45,
                ToolTip = plotId + " " + cell.ViewType + ", " + InWords(cell.State)
            };

            button.Click += (sender, e) => CellClicked(plotId, cell);
            return button;
        }

        private static string Face(SheetCellState state)
        {
            switch (state)
            {
                case SheetCellState.Exists: return ExistsMark;
                case SheetCellState.Marked: return MarkedMark;
                default: return MissingMark;
            }
        }

        private static string InWords(SheetCellState state)
        {
            switch (state)
            {
                case SheetCellState.Exists: return "exists, click to open it";
                case SheetCellState.Marked: return "marked, click to unmark";
                default: return "missing, click to mark it";
            }
        }

        /// <summary>
        /// A cell holding a view selects it. An empty one is marked, and a marked one is
        /// unmarked. Marking changes nothing in the model this round, it is only recorded.
        /// </summary>
        private void CellClicked(string plotId, SheetGridCell cell)
        {
            if (cell.State == SheetCellState.Exists)
            {
                _handler.AskToSelect(cell.ViewId);
                _asking.Raise();
                return;
            }

            var key = new PlotViewKey(plotId, cell.ViewType);
            if (!_marked.Remove(key)) _marked.Add(key);

            DrawSheet();
            Say(_marked.Count + " cells marked. Marking records intent and changes nothing in the model.");
        }

        private void Place(UIElement what, int row, int column)
        {
            Grid.SetRow(what, row);
            Grid.SetColumn(what, column);
            _sheet.Children.Add(what);
        }
    }
}
