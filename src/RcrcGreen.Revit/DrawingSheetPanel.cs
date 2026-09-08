using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.UI;
using RcrcGreen.Core;

// Autodesk.Revit.UI carries a ComboBox of its own for the ribbon. This panel is WPF, so the
// name is pinned to the one that belongs in a UserControl.
using ComboBox = System.Windows.Controls.ComboBox;
using TextBox = System.Windows.Controls.TextBox;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// The Drawing Sheet panel. Built in C# rather than XAML, because an SDK style net48
    /// project has no XAML compilation step.
    ///
    /// It is five numbered steps in the order somebody does them, one open at a time, each
    /// carrying its own summary while it is shut. It was a list of controls before, which read
    /// as a wall to anyone who had not built it.
    ///
    /// Nothing in this file reads a Document, opens a Transaction or touches the Revit API.
    /// Everything it wants doing goes through <see cref="DrawingSheetRequestHandler"/> and the
    /// external event. That is the rule the whole panel is built around.
    ///
    /// No brush and no spacing is written here either. Colours come from
    /// <see cref="PanelTheme"/> and every margin and size from <see cref="PanelMetrics"/>. The
    /// first install came up black on black, and the version after it lined nothing up with
    /// anything because each control carried a number somebody typed at it.
    /// </summary>
    internal sealed class DrawingSheetPanel : UserControl, IDockablePaneProvider
    {
        /// <summary>
        /// One character per cell rather than a word. Eighteen plots by several columns of
        /// "exists" and "missing" is a wall of text with no shape to it. The shape differs as
        /// well as the fill, so the three states are still apart at a glance. The legend above
        /// the grid says what each one means, and the words stay in the tooltip too.
        /// </summary>
        private const string ExistsMark = "\u25A0";

        private const string MissingMark = "\u25A1";

        private const string MarkedMark = "\u25CF";

        private readonly ExternalEvent _asking;
        private readonly DrawingSheetRequestHandler _handler;

        private readonly ComboBox _prefix = new ComboBox();
        private readonly ComboBox _from = new ComboBox();
        private readonly ComboBox _to = new ComboBox();
        private readonly TextBox _search = new TextBox();
        private readonly ComboBox _newCode = new ComboBox { Width = PanelMetrics.LabelWidth };
        private readonly TextBox _newName = new TextBox { MinWidth = PanelMetrics.ColumnWidth };


        private readonly StackPanel _steps = new StackPanel();
        private readonly TextBlock _modelName = new TextBlock();
        private readonly TextBlock _readAt = new TextBlock();
        private readonly TextBlock _said = new TextBlock { TextWrapping = TextWrapping.Wrap };

        // The header text of each step, kept so a keystroke in a sheet box can refresh the
        // summaries without rebuilding the tree under the cursor. Both readings come from one
        // PanelSteps and are refreshed together, so they cannot drift.
        private readonly Dictionary<PanelStep, TextBlock> _headerText =
            new Dictionary<PanelStep, TextBlock>();

        private readonly Border _strip = new Border();
        private readonly Border _status = new Border();

        // Built fresh inside step 5 rather than kept as one instance, and remembered here only
        // so a keystroke elsewhere can update its text. See Reparented for why.
        private TextBlock _runLine;

        private PanelTheme _theme = PanelTheme.Current();
        private DrawingSheetSnapshot _model = DrawingSheetSnapshot.Nothing;
        private GridColumns _columns = GridColumns.Over(null);
        private PlotSelection _picked = PlotSelection.Nothing;
        private readonly HashSet<PlotViewKey> _marked = new HashSet<PlotViewKey>();

        // The sheets the user has described, and what they typed for each plot. Held here
        // rather than read back off the controls, because the step is thrown away and rebuilt
        // on every change and a control that has gone is not a place to keep the only copy.
        private readonly List<SheetBeingDescribed> _sheets = new List<SheetBeingDescribed>();

        private PanelStep _stepOpen = PanelStep.Plots;

        // Step 1 opens step 2 by itself the first time a range is picked, because that is the
        // one act that unlocks everything below it. Once per read, so somebody who comes back
        // to change the range later is not thrown out of it again.
        private bool _leftPlotsAlready;

        private bool _filling;
        private bool _readOnce;
        private ScopeBoxCase? _caseOpen;

        public DrawingSheetPanel()
        {
            _handler = new DrawingSheetRequestHandler
            {
                Read = Took,
                Told = Say
            };
            _asking = ExternalEvent.Create(_handler);

            _prefix.SelectionChanged += (sender, e) => PrefixChosen();
            _from.SelectionChanged += (sender, e) => RangeChosen();
            _to.SelectionChanged += (sender, e) => RangeChosen();
            _search.TextChanged += (sender, e) => Redraw();

            Content = Layout();
            PaintFromTheTheme();
            Redraw();
            Say("Open a model. This panel reads it as soon as it is shown.");

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
            FontSize = PanelMetrics.Body;

            // The strip and the status bar are outside the part that gets drawn again, so they
            // are repainted by hand. Missing this is how the panel came up black on black the
            // first time: a colour set once, before the theme it was meant to follow was read.
            _strip.Background = _theme.Strip;
            _status.Background = _theme.Strip;
            _status.BorderBrush = _theme.Line;
            _readAt.Foreground = _theme.Faint;

            Redraw();
        }

        /// <summary>
        /// The strip at the top, the steps in the middle, the status line at the bottom. The
        /// status line is docked rather than placed last inside the scroll, so it stays in the
        /// same place whatever is open above it.
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

            everything.Children.Add(new ScrollViewer
            {
                Content = _steps,
                Padding = PanelMetrics.Edge,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            });

            return everything;
        }

        private UIElement Strip()
        {
            var inside = new DockPanel { Margin = PanelMetrics.StripInside, LastChildFill = true };

            var buttons = new StackPanel { Orientation = Orientation.Horizontal };
            buttons.Children.Add(Secondary("Refresh", AskedForARefresh,
                "Reads this model again. The panel does it by itself every time the pane is shown."));
            buttons.Children.Add(Secondary("Scan Model", AskedForAScan,
                "Reads the whole document and writes a report. It reads everything rather than "
                + "the range, so it is the check this panel is measured against."));
            DockPanel.SetDock(buttons, Dock.Right);
            inside.Children.Add(buttons);

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
            _status.BorderThickness = new Thickness(0.0, 1.0, 0.0, 0.0);
            _status.Child = new Border { Margin = PanelMetrics.StripInside, Child = _said };
            return _status;
        }

        /// <summary>
        /// The whole interface, drawn again from the state. There is one path in and out of
        /// every change, the same reason the column list is drawn from GridColumns rather than
        /// letting a tick box remember itself.
        /// </summary>
        private void Redraw()
        {
            PanelSteps steps = StepsNow();

            _steps.Children.Clear();
            _headerText.Clear();

            _modelName.Text = _model.DocumentTitle.Length == 0
                ? "No model read yet" : _model.DocumentTitle;
            _readAt.Text = _readOnce
                ? _model.ViewsRead + " views read at " + _model.ReadAt.ToString("HH:mm:ss")
                : "Press Refresh";

            foreach (StepState step in steps.All) _steps.Children.Add(Step(step));
        }

        private PanelSteps StepsNow()
        {
            return PanelSteps.Of(
                _readOnce,
                _model.PlotIds.Count,
                _from.SelectedItem as string,
                _to.SelectedItem as string,
                _picked.InRangeCount,
                _picked.TickedCount,
                _columns.ShownCount,
                _columns.All.Count,
                _marked.Count,
                _model.TitleBlockTypes.Count,
                _sheets.Count,
                SheetsToMake(),
                SheetsWanted().Count(one => !one.Definition.CanBeUsed),
                PlanNow());
        }

        /// <summary>
        /// A keystroke in a sheet box changes what the headers say and nothing else. Rebuilding
        /// the tree there would take the cursor out of the box the user is typing in.
        /// </summary>
        private void RefreshHeaders()
        {
            PanelSteps steps = StepsNow();

            foreach (StepState step in steps.All)
            {
                TextBlock header;
                if (_headerText.TryGetValue(step.Step, out header)) header.Text = step.Header;
            }

            if (_runLine != null) _runLine.Text = RunLine();
        }

        private UIElement Step(StepState step)
        {
            var block = new StackPanel { Margin = PanelMetrics.StepGap };
            bool open = _stepOpen == step.Step && step.Usable;

            block.Children.Add(Header(step, open));

            if (!step.Usable)
            {
                block.Children.Add(new Border
                {
                    Margin = PanelMetrics.StepInside,
                    Child = new TextBlock
                    {
                        Text = step.WhyNot,
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = _theme.Faint
                    }
                });

                return block;
            }

            if (open) block.Children.Add(new Border { Margin = PanelMetrics.StepInside, Child = Inside(step) });

            return block;
        }

        private UIElement Header(StepState step, bool open)
        {
            var line = new DockPanel { Margin = PanelMetrics.HeaderInside, LastChildFill = true };

            var sign = new TextBlock
            {
                Text = open ? "-" : "+",
                Width = PanelMetrics.StepNumber,
                Foreground = _theme.Faint,
                VerticalAlignment = VerticalAlignment.Center
            };
            DockPanel.SetDock(sign, Dock.Left);
            line.Children.Add(sign);

            var text = new TextBlock
            {
                Text = step.Header,
                FontWeight = FontWeights.Bold,
                FontSize = PanelMetrics.StepTitle,
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center,
                Opacity = step.Usable ? 1.0 : 0.55
            };
            _headerText[step.Step] = text;
            line.Children.Add(text);

            var header = new Border
            {
                Background = _theme.StepHeader,
                BorderBrush = _theme.Line,
                BorderThickness = PanelMetrics.Hairline,
                Child = line
            };

            if (!step.Usable) return header;

            var button = new Button
            {
                Content = header,
                Padding = new Thickness(0.0),
                BorderThickness = new Thickness(0.0),
                Background = _theme.StepHeader,
                Foreground = _theme.Foreground,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                ToolTip = open ? "Shut this step." : "Open step " + step.Number + "."
            };

            PanelStep which = step.Step;
            button.Click += (sender, e) => StepClicked(which);
            return button;
        }

        private UIElement Inside(StepState step)
        {
            switch (step.Step)
            {
                case PanelStep.Plots: return InsidePlots();
                case PanelStep.ViewTypes: return InsideViewTypes();
                case PanelStep.Mark: return InsideMark();
                case PanelStep.Sheets: return InsideSheets();
                default: return InsideRun();
            }
        }

        private void StepClicked(PanelStep step)
        {
            _stepOpen = step;
            Redraw();
        }

        /// <summary>
        /// Opens the next step that can be used. Nothing to open leaves this one where it is,
        /// because shutting everything would look like the panel had lost its place.
        /// </summary>
        private void MoveOnFrom(PanelStep finished)
        {
            PanelStep? next = StepsNow().OpenAfter(finished);
            if (next.HasValue) _stepOpen = next.Value;
        }

        private UIElement InsidePlots()
        {
            var block = new StackPanel();

            block.Children.Add(Labelled("Prefix", _prefix));
            block.Children.Add(Labelled("From", _from));
            block.Children.Add(Labelled("To", _to));

            block.Children.Add(Faint(_model.Empty
                ? "No plots in this model."
                : _picked.TickedCount + " of " + _picked.InRangeCount + " plots in range ticked, "
                    + _model.PlotIds.Count + " in the model. Untick one to leave it out of the "
                    + "counts and out of anything that writes."));

            var ticks = new StackPanel();
            foreach (string plotId in _picked.InRange)
            {
                string which = plotId;
                var tick = new CheckBox
                {
                    Content = which,
                    IsChecked = _picked.IsTicked(which),
                    Margin = PanelMetrics.Row
                };

                tick.Checked += (sender, e) => PlotTicked(which, true);
                tick.Unchecked += (sender, e) => PlotTicked(which, false);
                ticks.Children.Add(tick);
            }

            block.Children.Add(Scrolling(ticks, PanelMetrics.ListHeight));
            block.Children.Add(NextButton(PanelStep.Plots));
            return block;
        }

        private UIElement InsideViewTypes()
        {
            var block = new StackPanel();

            block.Children.Add(Labelled("Search", _search));

            var allOrNone = new StackPanel { Orientation = Orientation.Horizontal, Margin = PanelMetrics.Row };
            allOrNone.Children.Add(Secondary("All", () => TickWhatTheSearchShows(true),
                "Ticks every type the search is showing."));
            allOrNone.Children.Add(Secondary("None", () => TickWhatTheSearchShows(false),
                "Unticks every type the search is showing."));
            block.Children.Add(allOrNone);

            // The buttons and the Add dropdown are filled from the same list in the same loop.
            // They used to be filled by one method, the buttons were moved inline here when the
            // panel was rebuilt, and the dropdown half was left behind, so it opened empty and
            // no view type could be added at all.
            var codes = new WrapPanel { Margin = PanelMetrics.Row };
            FillTheCodes();
            foreach (string code in _columns.CodesInUse)
            {
                string which = code;
                codes.Children.Add(Secondary(which, () => TickWholeCode(which),
                    "Ticks every " + which + " type."));
            }
            block.Children.Add(codes);

            IReadOnlyList<ViewType> showing = _columns.Matching(_search.Text);

            var list = new StackPanel();
            _filling = true;
            foreach (ViewType viewType in showing)
            {
                ViewType which = viewType;
                var tick = new CheckBox
                {
                    Content = which + KindWord(which) + (_columns.IsNew(which) ? "   new" : string.Empty),
                    IsChecked = _columns.IsShown(which),
                    Margin = PanelMetrics.Row
                };

                tick.Checked += (sender, e) => ColumnShown(which, true);
                tick.Unchecked += (sender, e) => ColumnShown(which, false);
                list.Children.Add(tick);
            }
            _filling = false;

            block.Children.Add(Scrolling(list, PanelMetrics.ListHeight));

            block.Children.Add(Faint(showing.Count == _columns.All.Count
                ? _columns.ShownCount + " of " + _columns.All.Count + " ticked."
                : _columns.ShownCount + " of " + _columns.All.Count + " ticked. The search is "
                    + "showing " + showing.Count + " of them."));

            // A view type the model does not hold is exactly what somebody opens this panel to
            // create. The code list is filled from the model so the naming stays inside the
            // codes already in use, and the name is free text because that is the new part.
            var adding = new DockPanel { Margin = PanelMetrics.Row, LastChildFill = true };
            Button add = Secondary("Add", AddTheTypedType, "Adds a view type this model lacks.");
            DockPanel.SetDock(_newCode, Dock.Left);
            DockPanel.SetDock(add, Dock.Right);
            adding.Children.Add(Reparented(_newCode));
            adding.Children.Add(add);
            adding.Children.Add(Reparented(_newName));
            block.Children.Add(adding);

            block.Children.Add(NextButton(PanelStep.ViewTypes));
            return block;
        }

        /// <summary>
        /// The codes the Add row offers. Refilled rather than rebuilt, because the dropdown is
        /// one of the controls that outlives a redraw and clearing it under an open list would
        /// take the selection with it.
        /// </summary>
        private void FillTheCodes()
        {
            IReadOnlyList<string> codes = _columns.CodesInUse;

            bool same = _newCode.Items.Count == codes.Count;
            for (int at = 0; same && at < codes.Count; at++)
            {
                same = string.Equals(_newCode.Items[at] as string, codes[at], StringComparison.Ordinal);
            }

            if (same) return;

            _filling = true;
            try
            {
                string was = _newCode.SelectedItem as string;

                _newCode.Items.Clear();
                foreach (string code in codes) _newCode.Items.Add(code);

                if (was != null && codes.Contains(was)) _newCode.SelectedItem = was;
                else if (codes.Count > 0) _newCode.SelectedIndex = 0;
            }
            finally
            {
                _filling = false;
            }
        }

        private UIElement InsideMark()
        {
            var block = new StackPanel();

            block.Children.Add(Legend());

            SheetGrid grid = SheetGrid.Build(
                _picked.InRange, _columns.Shown, _model.Present, _model.PlotsWithAScopeBox, _marked);

            if (grid.Rows.Count == 0)
            {
                block.Children.Add(Faint(NothingToDraw()));
                return block;
            }

            block.Children.Add(TheGrid(grid));
            block.Children.Add(NextButton(PanelStep.Mark));
            return block;
        }

        private UIElement Legend()
        {
            var legend = new WrapPanel { Margin = PanelMetrics.Row };

            legend.Children.Add(LegendEntry(ExistsMark, "exists, click to open it"));
            legend.Children.Add(LegendEntry(MissingMark, "missing, click to mark it"));
            legend.Children.Add(LegendEntry(MarkedMark, "marked to be made"));

            return legend;
        }

        private UIElement LegendEntry(string mark, string means)
        {
            var entry = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = PanelMetrics.Gap
            };

            entry.Children.Add(new TextBlock { Text = mark, FontSize = PanelMetrics.Cell });
            entry.Children.Add(new TextBlock
            {
                Text = "  " + means,
                Foreground = _theme.Faint,
                VerticalAlignment = VerticalAlignment.Center
            });

            return entry;
        }

        /// <summary>
        /// The plot column does not scroll sideways and the view type columns do, so the plot a
        /// row belongs to stays on screen however far across somebody has gone. Both halves use
        /// the same fixed row height, because auto height on either side drifts out of step the
        /// moment one cell wraps and then the rows no longer line up.
        /// </summary>
        private UIElement TheGrid(SheetGrid grid)
        {
            var frozen = new Grid();
            frozen.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            frozen.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var scrolling = new Grid();
            foreach (ViewType ignored in grid.Columns)
            {
                scrolling.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }

            frozen.RowDefinitions.Add(new RowDefinition { Height = new GridLength(PanelMetrics.HeaderRowHeight) });
            scrolling.RowDefinitions.Add(new RowDefinition { Height = new GridLength(PanelMetrics.HeaderRowHeight) });

            Put(frozen, HeaderCell("Use"), 0, 0);
            Put(frozen, HeaderCell("Plot"), 0, 1);

            for (int column = 0; column < grid.Columns.Count; column++)
            {
                Put(scrolling, ColumnHeader(grid.Columns[column]), 0, column);
            }

            for (int row = 0; row < grid.Rows.Count; row++)
            {
                SheetGridRow line = grid.Rows[row];
                bool ticked = _picked.IsTicked(line.PlotId);

                frozen.RowDefinitions.Add(new RowDefinition { Height = new GridLength(PanelMetrics.RowHeight) });
                scrolling.RowDefinitions.Add(new RowDefinition { Height = new GridLength(PanelMetrics.RowHeight) });

                // Added before anything in the row, so it sits behind the cells rather than
                // over them. Every other row, faintly, so a long row can be followed across.
                if (row % 2 == 1)
                {
                    Shade(frozen, row + 1, 2);
                    Shade(scrolling, row + 1, grid.Columns.Count);
                }

                Put(frozen, TickFor(line.PlotId, ticked), row + 1, 0);

                // The mark says the plot has no scope box named for it. A view without one is
                // useless on this project, so it belongs on the row label. Only that case names
                // a colour. Everything else inherits the panel's.
                var label = new TextBlock
                {
                    Text = line.HasScopeBox ? line.PlotId : line.PlotId + "  no scope box",
                    Margin = PanelMetrics.CellPad,
                    VerticalAlignment = VerticalAlignment.Center,
                    Opacity = ticked ? 1.0 : 0.45
                };
                if (!line.HasScopeBox) label.Foreground = _theme.Warning;

                Put(frozen, label, row + 1, 1);

                for (int column = 0; column < line.Cells.Count; column++)
                {
                    Put(scrolling, CellButton(line.PlotId, line.Cells[column], ticked), row + 1, column);
                }
            }

            var both = new DockPanel { LastChildFill = true };
            DockPanel.SetDock(frozen, Dock.Left);
            both.Children.Add(frozen);
            both.Children.Add(new ScrollViewer
            {
                Content = scrolling,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
            });

            return new ScrollViewer
            {
                Content = both,
                MaxHeight = PanelMetrics.GridHeight,
                Margin = PanelMetrics.Row,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };
        }

        private void Shade(Grid grid, int row, int columns)
        {
            var stripe = new Border { Background = _theme.RowShade };
            Grid.SetRow(stripe, row);
            Grid.SetColumn(stripe, 0);
            Grid.SetColumnSpan(stripe, Math.Max(columns, 1));
            grid.Children.Add(stripe);
        }

        /// <summary>
        /// A schedule is a different thing to build, it lives under Schedules and Quantities
        /// rather than Views, and it filters on a different parameter. A section is a different
        /// call again. The kind is a word under the name rather than italics alone, because
        /// italics is not something anyone reads off a column header.
        /// </summary>
        private UIElement ColumnHeader(ViewType type)
        {
            var stack = new StackPanel { Margin = PanelMetrics.CellPad, MaxWidth = PanelMetrics.ColumnWidth };

            stack.Children.Add(new TextBlock
            {
                Text = type.ToString(),
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap
            });

            stack.Children.Add(new TextBlock
            {
                Text = KindOf(type) + (_columns.IsNew(type) ? "   new" : string.Empty),
                Foreground = _theme.Faint
            });

            return stack;
        }

        private string KindOf(ViewType type)
        {
            if (_model.IsASchedule(type)) return "schedule";
            if (_model.IsASection(type)) return "section";
            return "plan";
        }

        private string KindWord(ViewType type)
        {
            return "   " + KindOf(type);
        }

        /// <summary>
        /// The sheets the user describes, and the number every plot gets for each.
        ///
        /// A sheet is described once and repeated across the ticked plots. It used to be copied
        /// off a sheet that already existed, which meant one had to be laid out by hand first
        /// and meant picking an empty one gave an empty sheet.
        /// </summary>
        private UIElement InsideSheets()
        {
            var block = new StackPanel();

            if (_sheets.Count == 0)
            {
                block.Children.Add(Faint("No sheet added yet. A sheet is described once here and "
                    + "made for every ticked plot that has a number typed in."));
            }

            for (int at = 0; at < _sheets.Count; at++)
            {
                block.Children.Add(OneSheet(_sheets[at], at));
            }

            block.Children.Add(Secondary("Add a sheet", AddASheet,
                "Describes another sheet. One run can give a plot its list of drawings and its "
                + "general arrangement layout together."));

            block.Children.Add(NextButton(PanelStep.Sheets));
            return block;
        }

        private UIElement OneSheet(SheetBeingDescribed sheet, int at)
        {
            var block = new StackPanel { Margin = PanelMetrics.StepInside };

            var heading = new DockPanel { LastChildFill = true };
            Button remove = Secondary("Remove", () => RemoveASheet(sheet), "Takes this sheet out.");
            DockPanel.SetDock(remove, Dock.Right);
            heading.Children.Add(remove);
            heading.Children.Add(new TextBlock
            {
                Text = "Sheet " + (at + 1),
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center
            });
            block.Children.Add(heading);

            var type = new ComboBox { Margin = PanelMetrics.Row };
            foreach (TitleBlockType one in _model.TitleBlockTypes) type.Items.Add(one);
            if (sheet.TitleBlock != null) type.SelectedItem = ChosenTitleBlock(sheet);
            type.SelectionChanged += (sender, e) =>
            {
                if (_filling) return;
                sheet.TitleBlock = type.SelectedItem as TitleBlockType;
                RefreshHeaders();
            };
            block.Children.Add(Labelled("Type", type));

            // Editable, because the list is the names already in use and a new sheet often wants
            // one that is not. The list is an offer and never a restriction.
            var name = new ComboBox { IsEditable = true, Margin = PanelMetrics.Row, Text = sheet.SheetName };
            foreach (string one in _model.SheetNamesInUse) name.Items.Add(one);
            name.Loaded += (sender, e) => name.Text = sheet.SheetName;
            name.SelectionChanged += (sender, e) =>
            {
                if (_filling) return;
                sheet.SheetName = (name.SelectedItem as string) ?? name.Text ?? string.Empty;
                RefreshHeaders();
            };
            name.AddHandler(
                System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent,
                new TextChangedEventHandler((sender, e) =>
                {
                    if (_filling) return;
                    sheet.SheetName = name.Text ?? string.Empty;
                    RefreshHeaders();
                }));
            block.Children.Add(Labelled("Name", name));

            var perSheet = new StackPanel { Orientation = Orientation.Horizontal, Margin = PanelMetrics.Row };
            perSheet.Children.Add(new TextBlock
            {
                Text = "Views per sheet",
                Width = PanelMetrics.ColumnWidth,
                VerticalAlignment = VerticalAlignment.Center
            });
            foreach (int howMany in SheetLayout.Counts)
            {
                int which = howMany;
                var pick = new RadioButton
                {
                    Content = which.ToString(),
                    GroupName = "perSheet" + at,
                    IsChecked = sheet.ViewsPerSheet == which,
                    Margin = PanelMetrics.Gap,
                    VerticalAlignment = VerticalAlignment.Center
                };
                pick.Checked += (sender, e) =>
                {
                    if (_filling) return;
                    sheet.ViewsPerSheet = which;
                    Redraw();
                };
                perSheet.Children.Add(pick);
            }
            block.Children.Add(perSheet);

            block.Children.Add(Faint("Tick the views that go on it, from the types ticked in "
                + "step 2."));

            var views = new StackPanel();
            IReadOnlyList<ViewType> offered = _columns.Shown;
            if (offered.Count == 0)
            {
                views.Children.Add(Faint("No view type is ticked in step 2, so this sheet will "
                    + "be made empty."));
            }

            _filling = true;
            foreach (ViewType viewType in offered)
            {
                ViewType which = viewType;
                var tick = new CheckBox
                {
                    Content = which + KindWord(which),
                    IsChecked = sheet.Carries(which),
                    Margin = PanelMetrics.Row
                };
                tick.Checked += (sender, e) => SheetViewTicked(sheet, which, true);
                tick.Unchecked += (sender, e) => SheetViewTicked(sheet, which, false);
                views.Children.Add(tick);
            }
            _filling = false;

            block.Children.Add(Scrolling(views, PanelMetrics.ListHeight));
            block.Children.Add(Faint(sheet.Built(_columns.Shown).InWords()));
            block.Children.Add(Scrolling(NumberTable(sheet), PanelMetrics.ListHeight));

            return new Border
            {
                Background = _theme.StepHeader,
                BorderBrush = _theme.Line,
                BorderThickness = PanelMetrics.Hairline,
                Margin = PanelMetrics.Row,
                Child = block
            };
        }

        private void SheetViewTicked(SheetBeingDescribed sheet, ViewType which, bool carried)
        {
            if (_filling) return;

            sheet.Carry(which, carried);
            Redraw();
        }

        /// <summary>
        /// One row per ticked plot, holding the number that plot gets for this sheet. Editable,
        /// because a new sheet usually carries a number no sheet has yet.
        /// </summary>
        private UIElement NumberTable(SheetBeingDescribed sheet)
        {
            var table = new Grid();
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(PanelMetrics.ColumnWidth) });

            table.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Put(table, HeaderCell("Plot"), 0, 0);
            Put(table, HeaderCell("Sheet number"), 0, 1);

            int row = 1;
            foreach (string plotId in _picked.Ticked)
            {
                table.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                Put(table, new TextBlock
                {
                    Text = plotId,
                    Margin = PanelMetrics.CellPad,
                    VerticalAlignment = VerticalAlignment.Center
                }, row, 0);

                Put(table, NumberBox(sheet, plotId), row, 1);
                row++;
            }

            return table;
        }

        private UIElement NumberBox(SheetBeingDescribed sheet, string plotId)
        {
            var box = new ComboBox
            {
                IsEditable = true,
                Margin = PanelMetrics.Row,
                Text = sheet.NumberFor(plotId)
            };

            foreach (string one in _model.SheetNumbersInUse) box.Items.Add(one);

            string forPlot = plotId;
            box.Loaded += (sender, e) => box.Text = sheet.NumberFor(forPlot);

            // A keystroke changes what the headers say and nothing else, because rebuilding the
            // tree under the cursor takes the cursor out of the box.
            box.AddHandler(
                System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent,
                new TextChangedEventHandler((sender, e) =>
                {
                    if (_filling) return;
                    sheet.SetNumber(forPlot, box.Text ?? string.Empty);
                    RefreshHeaders();
                }));

            box.SelectionChanged += (sender, e) =>
            {
                if (_filling) return;
                sheet.SetNumber(forPlot, (box.SelectedItem as string) ?? box.Text ?? string.Empty);
                RefreshHeaders();
            };

            return box;
        }

        private TitleBlockType ChosenTitleBlock(SheetBeingDescribed sheet)
        {
            if (sheet.TitleBlock == null) return null;

            foreach (TitleBlockType one in _model.TitleBlockTypes)
            {
                if (one.CompareTo(sheet.TitleBlock) == 0) return one;
            }

            return null;
        }

        private void AddASheet()
        {
            var sheet = new SheetBeingDescribed();

            // The one title block type in the model is not a choice, so it is picked rather than
            // left for somebody to pick from a list of one.
            if (_model.TitleBlockTypes.Count == 1) sheet.TitleBlock = _model.TitleBlockTypes[0];

            _sheets.Add(sheet);
            Redraw();
        }

        private void RemoveASheet(SheetBeingDescribed sheet)
        {
            _sheets.Remove(sheet);
            Redraw();
        }

        private UIElement InsideRun()
        {
            var block = new StackPanel();

            _runLine = new TextBlock { Text = RunLine(), TextWrapping = TextWrapping.Wrap };
            block.Children.Add(_runLine);

            block.Children.Add(ScopeBoxes());

            var run = new Button
            {
                Content = "Run",
                Margin = PanelMetrics.Row,
                Padding = PanelMetrics.HeaderInside,
                FontWeight = FontWeights.Bold,
                Background = _theme.Primary,
                Foreground = _theme.OnPrimary,
                BorderThickness = new Thickness(0.0),
                ToolTip = "Creates what is marked, in one transaction, after one confirmation."
            };
            run.Click += (sender, e) => AskToRun();
            block.Children.Add(run);

            return block;
        }

        private string RunLine()
        {
            return PlanNow().InWords()
                + " Only marked cells on ticked plots are made, and nothing is copied from "
                + "another plot. A sheet is made only for a plot with a sheet number typed in.";
        }

        /// <summary>
        /// The six counts, with every case but B openable in one click. It sits inside step 5
        /// because it acts on the same ticked plots the run does.
        ///
        /// A count on its own is not enough. On the real model F was 1, one view carrying a
        /// scope box that is not its plot's, and finding out which view that was meant opening
        /// a text file on the Desktop.
        ///
        /// B is left as a number. It is 102 schedules that cannot hold a scope box, which is
        /// nothing anyone acts on.
        /// </summary>
        private UIElement ScopeBoxes()
        {
            var block = new StackPanel { Margin = PanelMetrics.Row };

            block.Children.Add(new TextBlock
            {
                Text = "SCOPE BOXES",
                FontWeight = FontWeights.Bold,
                Margin = PanelMetrics.Row
            });

            if (!_readOnce || _picked.TickedCount == 0)
            {
                block.Children.Add(Faint("Tick at least one plot to see what Assign would do."));
                return block;
            }

            ScopeBoxCounts counts = ScopeBoxCounts.For(
                _model.ViewStates, _model.ScopeBoxNames, _picked.Ticked);

            block.Children.Add(Faint(counts.Considered + " views across " + _picked.TickedCount
                + " ticked plots. Only C is written."));

            AddCase(block, counts, ScopeBoxCase.NameDoesNotParse, "A, name does not parse");

            block.Children.Add(new TextBlock
            {
                Text = "B, cannot hold a scope box: " + counts.Of(ScopeBoxCase.CannotHoldAScopeBox),
                Margin = PanelMetrics.Row
            });

            AddCase(block, counts, ScopeBoxCase.ReadyToAssign, "C, ready to assign");
            AddCase(block, counts, ScopeBoxCase.NoMatchingScopeBox, "D, no scope box for that plot");
            AddCase(block, counts, ScopeBoxCase.AlreadyRight, "E, already right");
            AddCase(block, counts, ScopeBoxCase.HoldsADifferentScopeBox, "F, holds a different scope box");

            block.Children.Add(Secondary("Assign Scope Boxes", AskToAssign,
                "Writes case C only, in one transaction, after one confirmation."));

            return block;
        }

        private void AddCase(
            StackPanel block, ScopeBoxCounts counts, ScopeBoxCase outcome, string label)
        {
            IReadOnlyList<ViewScopeBoxDecision> inIt = counts.In(outcome);
            bool open = _caseOpen.HasValue && _caseOpen.Value == outcome;

            var line = new Button
            {
                Content = (open ? "-  " : "+  ") + label + ": " + inIt.Count,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Padding = PanelMetrics.CellPad,
                Margin = PanelMetrics.Row,
                IsEnabled = inIt.Count > 0,
                ToolTip = inIt.Count == 0
                    ? "Nothing in this case."
                    : "Show the " + inIt.Count + " views in this case. Click one to open it."
            };

            ScopeBoxCase which = outcome;
            line.Click += (sender, e) => CaseOpened(which);
            block.Children.Add(line);

            if (!open) return;

            foreach (ViewScopeBoxDecision decision in inIt)
            {
                ViewScopeBoxDecision one = decision;
                var view = new Button
                {
                    Content = one.PlotId + "   " + one.ViewName
                        + (one.CurrentScopeBoxName.Length > 0 ? "   holds " + one.CurrentScopeBoxName : string.Empty),
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Padding = PanelMetrics.CellPad,
                    Margin = new Thickness(PanelMetrics.StepNumber, 0.0, 0.0, 1.0),
                    ToolTip = "Open " + one.ViewName + " in Revit."
                };

                view.Click += (sender, e) => OpenTheView(one.ViewId);
                block.Children.Add(view);
            }
        }

        /// <summary>
        /// One case open at a time. Six lists at once in a docked pane is a scroll, not a view.
        /// </summary>
        private void CaseOpened(ScopeBoxCase outcome)
        {
            _caseOpen = _caseOpen.HasValue && _caseOpen.Value == outcome
                ? (ScopeBoxCase?)null
                : outcome;

            Redraw();
        }

        private UIElement NextButton(PanelStep from)
        {
            PanelStep? next = StepsNow().OpenAfter(from);
            if (!next.HasValue) return new StackPanel();

            StepState to = StepsNow().For(next.Value);

            return Secondary(
                "Next: " + to.Number + "  " + to.Title,
                () => { _stepOpen = to.Step; Redraw(); },
                "Opens step " + to.Number + ".");
        }

        private Button Secondary(string text, Action clicked, string why)
        {
            var button = new Button
            {
                Content = text,
                Margin = PanelMetrics.Gap,
                Padding = PanelMetrics.CellPad,
                ToolTip = why
            };

            button.Click += (sender, e) => clicked();
            return button;
        }

        private TextBlock Faint(string text)
        {
            return new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                Foreground = _theme.Faint,
                Margin = PanelMetrics.Row
            };
        }

        /// <summary>
        /// Takes a control out of whatever it was in before it goes somewhere new.
        ///
        /// A handful of controls outlive a redraw, because a ComboBox carries its own item list
        /// and a TextBox carries what somebody is halfway through typing. The steps around them
        /// are thrown away and built again on every change, so each of those controls is put
        /// into a new parent every time. WPF refuses that outright: an element already has a
        /// logical parent and adding it to a second one throws, which would take the panel down
        /// on the second click rather than the first.
        /// </summary>
        private static UIElement Reparented(UIElement what)
        {
            var was = LogicalTreeHelper.GetParent(what) as DependencyObject;

            var panel = was as Panel;
            if (panel != null) panel.Children.Remove(what);

            var holder = was as ContentControl;
            if (holder != null) holder.Content = null;

            var around = was as Decorator;
            if (around != null) around.Child = null;

            return what;
        }

        private static UIElement Labelled(string label, UIElement control)
        {
            var line = new DockPanel { Margin = PanelMetrics.Row, LastChildFill = true };
            var caption = new TextBlock
            {
                Text = label,
                Width = PanelMetrics.LabelWidth,
                VerticalAlignment = VerticalAlignment.Center
            };
            DockPanel.SetDock(caption, Dock.Left);
            line.Children.Add(caption);
            line.Children.Add(Reparented(control));
            return line;
        }

        private static UIElement Scrolling(UIElement what, double tall)
        {
            return new ScrollViewer
            {
                Content = what,
                MaxHeight = tall,
                Margin = PanelMetrics.Row,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };
        }

        private static TextBlock HeaderCell(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                Margin = PanelMetrics.CellPad,
                VerticalAlignment = VerticalAlignment.Bottom
            };
        }

        private static void Put(Grid grid, UIElement what, int row, int column)
        {
            Grid.SetRow(what, row);
            Grid.SetColumn(what, column);
            grid.Children.Add(what);
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

        /// <summary>
        /// The same call Revit makes before it writes, over the same plots by the same rule.
        /// Revit reads the model again first, so the numbers can differ if the model changed,
        /// but the rule that produced them cannot.
        /// </summary>
        private RunPlan PlanNow()
        {
            return RunPlan.Of(
                _marked,
                _picked.Ticked,
                _model.PlotsWithAScopeBox,
                _model.ScheduleTypes,
                _model.SectionTypes,
                _model.ScheduleTypes,
                SheetsWanted());
        }

        private IReadOnlyList<SheetOrder> SheetsWanted()
        {
            IReadOnlyList<ViewType> ticked = _columns.Shown;
            IReadOnlyList<string> plots = _picked.Ticked;

            return _sheets.Select(one => one.Ordered(ticked, plots)).ToList();
        }

        /// <summary>
        /// How many sheets a run would make right now: every usable definition against every
        /// ticked plot with a number typed in.
        /// </summary>
        private int SheetsToMake()
        {
            return SheetsWanted()
                .Where(one => one.Definition.CanBeUsed)
                .Sum(one => one.FilledIn);
        }

        private void AskToRun()
        {
            IReadOnlyList<string> ticked = _picked.Ticked;
            if (ticked.Count == 0)
            {
                Say("No plots are ticked, so there is nothing to run.");
                return;
            }

            if (PlanNow().MakesNothing)
            {
                Say("Nothing is marked and no sheet is asked for. Click an empty cell in step 3, "
                    + "or add a sheet in step 4 and type a number for a plot.");
                return;
            }

            Say("Working out the run.");
            _handler.AskToRun(ticked, _marked.ToList(), SheetsWanted());
            _asking.Raise();
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
        /// had changed, which is most of why an early version looked broken.
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
                _leftPlotsAlready = false;
                _columns = _columns.OverTheseTypes(_model.ViewTypes);

                FillPrefixes();
                PutTheRangeBack(prefixWas, fromWas, toWas);

                _stepOpen = StepsNow().FirstUnfinished;
                Redraw();
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

        private void ColumnShown(ViewType which, bool shown)
        {
            if (_filling) return;

            _columns = _columns.Showing(which, shown);
            Redraw();
        }

        /// <summary>
        /// All and None act on what the search is showing, which is the point of having them.
        /// </summary>
        private void TickWhatTheSearchShows(bool ticked)
        {
            _columns = _columns.ShowingThese(_columns.Matching(_search.Text), ticked);
            Redraw();
        }

        private void TickWholeCode(string code)
        {
            _columns = _columns.ShowingCode(code);
            Redraw();
        }

        private void AddTheTypedType()
        {
            string code = _newCode.SelectedItem as string;
            string name = (_newName.Text ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(code))
            {
                Say("Pick a code for the new view type. The list holds the codes this model uses.");
                return;
            }

            if (name.Length == 0)
            {
                Say("Type a name for the new view type.");
                return;
            }

            var wanted = new ViewType(code, name);
            if (_columns.All.Contains(wanted))
            {
                Say(wanted + " is already a column.");
                return;
            }

            _columns = _columns.Adding(wanted);
            _newName.Text = string.Empty;

            Redraw();
            Say(wanted + " added. It shows missing on every plot, which is right, and it is "
                + "marked new until the model holds one.");
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

            // Picking a range is the one act that unlocks everything below, so it opens the
            // next step. Once per read, so coming back to change the range later does not
            // throw the user out of it again.
            if (!_leftPlotsAlready && _picked.InRangeCount > 0 && _stepOpen == PanelStep.Plots)
            {
                _leftPlotsAlready = true;
                MoveOnFrom(PanelStep.Plots);
            }

            Redraw();
        }

        private IReadOnlyList<string> PlotsInRange()
        {
            return PlotRange.Between(
                _model.PlotIds,
                _prefix.SelectedItem as string,
                _from.SelectedItem as string,
                _to.SelectedItem as string);
        }

        private void PlotTicked(string plotId, bool ticked)
        {
            if (_filling) return;

            _picked = _picked.Ticking(plotId, ticked);
            Redraw();
        }

        private CheckBox TickFor(string plotId, bool ticked)
        {
            string which = plotId;
            var tick = new CheckBox
            {
                IsChecked = ticked,
                Margin = PanelMetrics.CellPad,
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
                return "Pick a prefix in step 1. From and To fill themselves with the plots "
                    + "under it, and the grid follows.";
            }

            return "No plots in that range. Widen From and To in step 1.";
        }

        private Button CellButton(string plotId, SheetGridCell cell, bool ticked)
        {
            var button = new Button
            {
                Content = Face(cell.State),
                FontSize = PanelMetrics.Cell,
                Margin = new Thickness(1.0),
                MinWidth = PanelMetrics.RowHeight,
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
        /// unmarked. Marking changes nothing in the model, it is only recorded.
        /// </summary>
        private void CellClicked(string plotId, SheetGridCell cell)
        {
            if (cell.State == SheetCellState.Exists)
            {
                OpenTheView(cell.ViewId);
                return;
            }

            var key = new PlotViewKey(plotId, cell.ViewType);
            if (!_marked.Remove(key)) _marked.Add(key);

            Redraw();
            Say(_marked.Count + " marked. Marking records intent and changes nothing until Run.");
        }

        private void OpenTheView(long viewId)
        {
            _handler.AskToSelect(viewId);
            _asking.Raise();
        }
    }
}
