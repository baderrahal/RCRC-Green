using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
    /// </summary>
    internal sealed class DrawingSheetPanel : UserControl, IDockablePaneProvider
    {
        private readonly ExternalEvent _asking;
        private readonly DrawingSheetRequestHandler _handler;

        private readonly ComboBox _prefix = new ComboBox { Margin = new Thickness(0, 2, 0, 2) };
        private readonly ComboBox _from = new ComboBox { Margin = new Thickness(0, 2, 0, 2) };
        private readonly ComboBox _to = new ComboBox { Margin = new Thickness(0, 2, 0, 2) };
        private readonly ComboBox _typeToAdd = new ComboBox { Margin = new Thickness(0, 2, 0, 2) };
        private readonly TextBlock _rangeCount = new TextBlock { Margin = new Thickness(0, 4, 0, 4) };
        private readonly TextBlock _said = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 0) };
        private readonly Grid _sheet = new Grid();
        private readonly Button _assign = new Button { Content = "Assign Scope Boxes", Margin = new Thickness(0, 4, 0, 2) };
        private readonly Button _addType = new Button { Content = "Add column", Margin = new Thickness(4, 2, 0, 2) };

        private DrawingSheetSnapshot _model = DrawingSheetSnapshot.Nothing;
        private readonly List<ViewType> _columns = new List<ViewType>();
        private readonly HashSet<PlotViewKey> _marked = new HashSet<PlotViewKey>();
        private bool _filling;

        public DrawingSheetPanel()
        {
            _handler = new DrawingSheetRequestHandler
            {
                Read = Took,
                Told = Say
            };
            _asking = ExternalEvent.Create(_handler);

            Content = Layout();
            WhenRangeIsSet(false);
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this;
            data.InitialState = new DockablePaneState
            {
                DockPosition = DockPosition.Right
            };
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

            everything.Children.Add(Heading("GRID"));
            everything.Children.Add(new ScrollViewer
            {
                Content = _sheet,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 420,
                Margin = new Thickness(0, 2, 0, 6)
            });

            everything.Children.Add(Heading("BOTTOM"));

            var adding = new DockPanel { Margin = new Thickness(0, 2, 0, 2) };
            DockPanel.SetDock(_addType, Dock.Right);
            adding.Children.Add(_addType);
            adding.Children.Add(_typeToAdd);
            everything.Children.Add(adding);

            _addType.Click += (sender, e) => AddChosenColumn();
            _assign.Click += (sender, e) => AskToAssign();

            everything.Children.Add(_assign);

            var refresh = new Button { Content = "Refresh", Margin = new Thickness(0, 2, 0, 2) };
            refresh.Click += (sender, e) => Ask(DrawingSheetRequest.Refresh);
            everything.Children.Add(refresh);

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

        private void AskToAssign()
        {
            IReadOnlyList<string> inRange = PlotsInRange();
            if (inRange.Count == 0)
            {
                Say("Set a plot range first.");
                return;
            }

            _handler.AskToAssign(inRange);
            _asking.Raise();
        }

        /// <summary>
        /// The handler calls this from the Revit thread, so the hop to the panel's own thread
        /// happens here rather than being forgotten at each call site.
        /// </summary>
        private void Took(DrawingSheetSnapshot snapshot)
        {
            Dispatcher.Invoke(() =>
            {
                _model = snapshot ?? DrawingSheetSnapshot.Nothing;
                _marked.Clear();
                _columns.Clear();
                FillPrefixes();
                FillTypesToAdd();
                DrawSheet();
            });
        }

        private void Say(string what)
        {
            Dispatcher.Invoke(() => _said.Text = what ?? string.Empty);
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

            WhenRangeIsSet(false);
            _rangeCount.Text = _model.Empty
                ? "No plots in this model."
                : "0 plots in range, " + _model.PlotIds.Count + " in the model.";
        }

        private void FillTypesToAdd()
        {
            _typeToAdd.Items.Clear();

            // Only view types already found in the model. There is no free text here, and no
            // way to name a type that does not exist.
            foreach (ViewType viewType in _model.ViewTypes)
            {
                _typeToAdd.Items.Add(viewType);
            }
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

        private void RangeChosen()
        {
            if (_filling) return;

            IReadOnlyList<string> inRange = PlotsInRange();

            _rangeCount.Text = inRange.Count + " plots in range, " + _model.PlotIds.Count + " in the model.";
            WhenRangeIsSet(inRange.Count > 0);
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
        /// Nothing below the range is usable until a range exists, because every one of those
        /// controls acts on the plots in it.
        /// </summary>
        private void WhenRangeIsSet(bool ready)
        {
            _sheet.IsEnabled = ready;
            _assign.IsEnabled = ready;
            _addType.IsEnabled = ready;
            _typeToAdd.IsEnabled = ready;
        }

        private void AddChosenColumn()
        {
            var chosen = _typeToAdd.SelectedItem as ViewType;
            if (chosen == null)
            {
                Say("Choose a view type to add.");
                return;
            }

            if (_columns.Contains(chosen))
            {
                Say(chosen + " is already a column.");
                return;
            }

            _columns.Add(chosen);
            DrawSheet();
        }

        private void DrawSheet()
        {
            _sheet.Children.Clear();
            _sheet.ColumnDefinitions.Clear();
            _sheet.RowDefinitions.Clear();

            SheetGrid grid = SheetGrid.Build(
                PlotsInRange(), _columns, _model.Present, _model.PlotsWithAScopeBox, _marked);

            if (grid.Rows.Count == 0) return;

            _sheet.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            foreach (ViewType ignored in grid.Columns)
            {
                _sheet.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }

            _sheet.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Place(new TextBlock { Text = "Plot", FontWeight = FontWeights.Bold, Margin = new Thickness(4, 2, 8, 2) }, 0, 0);

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
                    column + 1);
            }

            for (int row = 0; row < grid.Rows.Count; row++)
            {
                SheetGridRow line = grid.Rows[row];
                _sheet.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                Place(
                    new TextBlock
                    {
                        // The mark says the plot has no scope box named for it. A view without
                        // one is useless on this project, so it belongs on the row label.
                        Text = line.HasScopeBox ? line.PlotId : line.PlotId + "  no scope box",
                        Margin = new Thickness(4, 2, 8, 2),
                        Foreground = line.HasScopeBox ? Brushes.Black : Brushes.Firebrick
                    },
                    row + 1,
                    0);

                for (int column = 0; column < line.Cells.Count; column++)
                {
                    Place(CellButton(line.PlotId, line.Cells[column]), row + 1, column + 1);
                }
            }
        }

        private Button CellButton(string plotId, SheetGridCell cell)
        {
            var button = new Button
            {
                Content = Face(cell.State),
                Margin = new Thickness(1),
                MinWidth = 64,
                ToolTip = plotId + " " + cell.ViewType + ", " + cell.State
            };

            button.Click += (sender, e) => CellClicked(plotId, cell);
            return button;
        }

        private static string Face(SheetCellState state)
        {
            switch (state)
            {
                case SheetCellState.Exists: return "exists";
                case SheetCellState.Marked: return "marked";
                default: return "missing";
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
