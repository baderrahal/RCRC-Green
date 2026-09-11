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


        // The two settings files, read when the panel is built and again after every save.
        // This is a copy, and it is the one copy the panel is allowed: the panel is what writes
        // the user's file, so nothing else moves it while the pane is open.
        private TitleBlockSettings _titleBlocks = TitleBlockSettings.Nothing;
        private IReadOnlyList<string> _settingsNotRead = new List<string>();

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

        /// <summary>
        /// How far down each rebuilt list was scrolled, by name. The lists themselves cannot
        /// hold it, because every one of them is thrown away on each change.
        /// </summary>
        private readonly Dictionary<string, double> _scrolledTo =
            new Dictionary<string, double>(StringComparer.Ordinal);

        /// <summary>
        /// One for every row of every sheet table on screen, called with a fresh set of rows
        /// whenever anything about the sheets changes.
        ///
        /// A number typed into one box can change what every other box should say, because two
        /// rows given one number is a clash neither knows about on its own, and a proposal
        /// steps aside when a typed number takes the one it offered. Refreshing only the box
        /// being typed in would leave the others stale, and rebuilding the tree would take the
        /// cursor out of the box.
        /// </summary>
        private readonly List<Action<IReadOnlyList<IReadOnlyList<SheetRowShown>>>> _numberWarnings =
            new List<Action<IReadOnlyList<IReadOnlyList<SheetRowShown>>>>();

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

            ReadTheTitleBlockSettings();

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
            ForgetTheNumberWarnings();

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
                GridNow().MarkedCount,
                _model.TitleBlockTypes.Count,
                _sheets.Count,
                SheetsToMake(),
                SheetsWanted().Count(one => !one.Definition.CanBeUsed),
                SheetsWanted().Sum(one => one.RowsShortOfANameOrANumber),
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

            // Every sheet row, not only the one being typed in. Two rows given one number is a
            // clash neither box knows about on its own, and a proposal has to move when a
            // typed number takes the one it offered.
            IReadOnlyList<IReadOnlyList<SheetRowShown>> rows = DescribedRows();
            foreach (Action<IReadOnlyList<IReadOnlyList<SheetRowShown>>> said in _numberWarnings)
            {
                said(rows);
            }
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

            block.Children.Add(Faint(PanelSteps.PlotsLine(
                _model.Empty, _picked.TickedCount, _picked.InRangeCount, _model.PlotIds.Count)));

            var ticks = new StackPanel();
            foreach (string plotId in _picked.InRange)
            {
                string which = plotId;

                // The suffix marks the plots no view carries, which are the ones with
                // everything missing and the ones the old view-only list silently dropped.
                // The words come from the record so the panel formats no rule of its own.
                PlotRecord record = _model.RecordOf(which);
                string suffix = record == null ? string.Empty : record.NoViewsInWords();

                var tick = new CheckBox
                {
                    Content = Label(suffix.Length == 0 ? which : which + "   " + suffix),
                    IsChecked = _picked.IsTicked(which),
                    Margin = PanelMetrics.Row,
                    ToolTip = record == null
                        ? which
                        : which + ", found through " + record.SourcesInWords() + "."
                };

                tick.Checked += (sender, e) => PlotTicked(which, true);
                tick.Unchecked += (sender, e) => PlotTicked(which, false);
                ticks.Children.Add(tick);
            }

            block.Children.Add(Scrolling(ticks, PanelMetrics.ListHeight, "plots"));
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
                    Content = Label(which + KindWord(which) + (_columns.IsNew(which) ? "   new" : string.Empty)),
                    IsChecked = _columns.IsShown(which),
                    Margin = PanelMetrics.Row
                };

                tick.Checked += (sender, e) => ColumnShown(which, true);
                tick.Unchecked += (sender, e) => ColumnShown(which, false);
                list.Children.Add(tick);
            }
            _filling = false;

            block.Children.Add(Scrolling(list, PanelMetrics.ListHeight, "view types"));

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

        /// <summary>
        /// The grid as it stands, and the one place marks are read from for anything the user
        /// sees or the run is handed. <c>_marked</c> is a memory: it keeps a mark on a view type
        /// unticked in step 2 so the mark comes back with the column. What the header counts,
        /// what the status line says and what reaches the plan is what this grid shows, because
        /// a run once made a view in a column the user had unticked and could not see, while the
        /// header counted marks over a grid showing none.
        /// </summary>
        private SheetGrid GridNow()
        {
            return SheetGrid.Build(
                _picked.InRange, _columns.Shown, _model.Present, _model.PlotsWithAScopeBox, _marked);
        }

        private UIElement InsideMark()
        {
            var block = new StackPanel();

            block.Children.Add(Legend());

            SheetGrid grid = GridNow();

            if (grid.Rows.Count == 0)
            {
                block.Children.Add(Faint(NothingToDraw()));
                return block;
            }

            block.Children.Add(Sweeps(grid));
            block.Children.Add(TheGrid(grid));
            block.Children.Add(TheColumnKey(grid));
            block.Children.Add(NextButton(PanelStep.Mark));
            return block;
        }

        /// <summary>
        /// Marking a lot of cells at once. 17 plots by 8 view types is 136 clicks, which is
        /// what the first person to use the grid actually did.
        ///
        /// The other two ways in are the grid itself: the plot name marks its row and the
        /// column header marks its column. Every one of them goes through
        /// <see cref="BulkMarking"/>, so a sweep can never mean something a single click does
        /// not.
        /// </summary>
        private UIElement Sweeps(SheetGrid grid)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = PanelMetrics.Row };

            row.Children.Add(Secondary(
                "Mark every missing",
                () => MarkThese(BulkMarking.EveryMissing(grid, _picked.Ticked)),
                "Marks every empty square on every ticked plot. A plot name marks its row and a "
                + "column header marks its column."));

            row.Children.Add(Secondary("Clear all marks", ClearTheMarks,
                "Takes every mark off. Nothing in the model changes either way."));

            return row;
        }

        /// <summary>
        /// What each shortened column header stands for.
        ///
        /// The header is the code alone wherever the code is unique, because
        /// (200) General Arrangement Layout is thirty characters over a column one square wide
        /// and eight of them ran off the right edge. Only the columns that lost something are
        /// listed, so a grid of plain codes carries no key at all.
        /// </summary>
        private UIElement TheColumnKey(SheetGrid grid)
        {
            var key = new WrapPanel { Margin = PanelMetrics.Row };

            foreach (GridColumnLabel label in GridColumnLabels.For(grid.Columns))
            {
                if (!label.Shortened) continue;

                key.Children.Add(new TextBlock
                {
                    Text = label.Short + " is " + label.Full + "      ",
                    Foreground = _theme.Faint
                });
            }

            return key;
        }

        private void MarkThese(IReadOnlyList<PlotViewKey> cells)
        {
            foreach (PlotViewKey one in cells) _marked.Add(one);

            Redraw();

            // Every cell handed back was missing, so the count is what was really added rather
            // than what was asked for.
            Say(BulkMarking.InWords(cells.Count, GridNow().MarkedCount));
        }

        private void ClearTheMarks()
        {
            int had = _marked.Count;
            _marked.Clear();

            Redraw();
            Say(BulkMarking.ClearedInWords(had));
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

            IReadOnlyList<GridColumnLabel> headers = GridColumnLabels.For(grid.Columns);
            for (int column = 0; column < headers.Count; column++)
            {
                Put(scrolling, ColumnHeader(headers[column], grid), 0, column);
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

                Put(frozen, Flat(label, line.PlotId + ", click to mark every missing view on it",
                    () => MarkThese(BulkMarking.WholeRow(grid, _picked.Ticked, line.PlotId))),
                    row + 1, 1);

                for (int column = 0; column < line.Cells.Count; column++)
                {
                    Put(scrolling, CellButton(line.PlotId, line.Cells[column], ticked), row + 1, column);
                }
            }

            // Both viewers come back where they were left. Every mark redraws the grid, and a
            // fresh viewer starts at the top left, so marking ten cells down a long grid was
            // ten re-scrolls. The user reported it twice.
            var both = new DockPanel { LastChildFill = true };
            DockPanel.SetDock(frozen, Dock.Left);
            both.Children.Add(frozen);
            both.Children.Add(Remembering(new ScrollViewer
            {
                Content = scrolling,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
            }, "grid columns"));

            return Remembering(new ScrollViewer
            {
                Content = both,
                MaxHeight = PanelMetrics.GridHeight,
                Margin = PanelMetrics.Row,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            }, "grid rows");
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
        private UIElement ColumnHeader(GridColumnLabel label, SheetGrid grid)
        {
            ViewType type = label.Type;
            var stack = new StackPanel { Margin = PanelMetrics.CellPad, MaxWidth = PanelMetrics.ColumnWidth };

            stack.Children.Add(new TextBlock
            {
                Text = label.Short,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap
            });

            stack.Children.Add(new TextBlock
            {
                Text = KindOf(type) + (_columns.IsNew(type) ? "   new" : string.Empty),
                Foreground = _theme.Faint
            });

            return Flat(stack, label.Full + ", click to mark it on every ticked plot",
                () => MarkThese(BulkMarking.WholeColumn(grid, _picked.Ticked, type)));
        }

        /// <summary>
        /// Something that takes a click without becoming a button to look at.
        ///
        /// A plot name and a column header are labels, and turning either into an ordinary
        /// button would put chrome down the frozen column and across the top of a grid that is
        /// already dense. The padding is nothing so the frozen column stays in step with the
        /// scrolling cells beside it, which is the only thing making the two halves line up.
        /// </summary>
        private Button Flat(UIElement what, string why, Action clicked)
        {
            var button = new Button
            {
                Content = what,
                Background = _theme.Clear,
                BorderThickness = PanelMetrics.Nothing,
                Padding = PanelMetrics.Nothing,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Center,
                Cursor = System.Windows.Input.Cursors.Hand,
                ToolTip = why
            };

            button.Click += (sender, e) => clicked();
            return button;
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
        /// The sheets the user describes, and the set each definition makes per ticked plot.
        ///
        /// A sheet is described once and its views divide into as many sheets as they need,
        /// made for every ticked plot. It used to be copied off a sheet that already existed,
        /// which meant one had to be laid out by hand first and meant picking an empty one
        /// gave an empty sheet.
        /// </summary>
        private UIElement InsideSheets()
        {
            var block = new StackPanel();

            if (_sheets.Count == 0)
            {
                block.Children.Add(Faint("No sheet added yet. A sheet is described once here, "
                    + "its views divide into as many sheets as they need, and each ticked plot "
                    + "gets the whole set."));
            }

            // A settings line that could not be read is said once, at the top, rather than
            // against every sheet. Dropping it would leave a pairing that silently never
            // arrives, which reads exactly like a pairing nobody ever wrote.
            foreach (string line in _settingsNotRead)
            {
                block.Children.Add(Faint("A title block setting could not be read: " + line));
            }

            IReadOnlyList<IReadOnlyList<SheetRowShown>> rows = DescribedRows();

            // Read once here and shared by every box below. Each name box and each number box
            // used to copy its list item by item, 1,385 names and about as many numbers per box
            // on the measured model, for every row of every sheet on every redraw.
            var offers = new SheetOffers(_model.SheetNamesInUse, _model.FreeSheetNumbers);

            for (int at = 0; at < _sheets.Count; at++)
            {
                block.Children.Add(OneSheet(_sheets[at], at, rows, offers));
            }

            block.Children.Add(Secondary("Add a sheet", AddASheet,
                "Describes another sheet. One run can give a plot its list of drawings and its "
                + "general arrangement layout together."));

            block.Children.Add(NextButton(PanelStep.Sheets));
            return block;
        }

        /// <summary>
        /// The two dropdown lists every row's boxes offer, read off the snapshot once per redraw.
        /// </summary>
        private sealed class SheetOffers
        {
            public SheetOffers(IReadOnlyList<string> namesInUse, IReadOnlyList<string> freeNumbers)
            {
                NamesInUse = namesInUse;
                FreeNumbers = freeNumbers;
            }

            public IReadOnlyList<string> NamesInUse { get; }

            public IReadOnlyList<string> FreeNumbers { get; }
        }

        private UIElement OneSheet(
            SheetBeingDescribed sheet,
            int at,
            IReadOnlyList<IReadOnlyList<SheetRowShown>> rows,
            SheetOffers offers)
        {
            var block = new StackPanel { Margin = PanelMetrics.StepInside };

            var heading = new DockPanel { LastChildFill = true };
            Button remove = Secondary("Remove", () => RemoveASheet(sheet),
                "Takes this sheet out. It asks first when a name or a number has been typed on "
                + "any of its rows.");
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
                RememberTheTitleBlock(sheet);
                Redraw();
            };
            // Title block, in those words, everywhere. It was captioned Type here, called a
            // sheet type in the shut step and the refusal, and sat over a list of view type
            // tick boxes, three words for one control next to a fourth thing called a type.
            block.Children.Add(Labelled("Title block", type));

            // Where this one came from: the user's own settings, the shipped defaults, or
            // neither. A value that filled itself in is a different thing to the person
            // deciding whether to trust it than one they picked.
            block.Children.Add(Faint(
                _titleBlocks.WhereItCameFrom(sheet.Built(_columns.Shown).Views)));

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
                + "step 2. They go onto sheets in the order they are ticked here."));

            var views = new StackPanel();
            IReadOnlyList<ViewType> offered = _columns.Shown;
            if (offered.Count == 0)
            {
                views.Children.Add(Faint("No view type is ticked in step 2, so this definition "
                    + "makes no sheets."));
            }

            _filling = true;
            foreach (ViewType viewType in offered)
            {
                ViewType which = viewType;
                var tick = new CheckBox
                {
                    Content = Label(which + KindWord(which)),
                    IsChecked = sheet.Carries(which),
                    Margin = PanelMetrics.Row
                };
                tick.Checked += (sender, e) => SheetViewTicked(sheet, which, true);
                tick.Unchecked += (sender, e) => SheetViewTicked(sheet, which, false);
                views.Children.Add(tick);
            }
            _filling = false;

            block.Children.Add(Scrolling(views, PanelMetrics.ListHeight, "sheet " + at + " views"));

            SheetDefinition described = sheet.Built(_columns.Shown);
            block.Children.Add(Faint(described.InWords()));

            // Why some rows start with empty boxes, said once per shape of sheet rather than
            // repeated down every plot's row.
            foreach (PlannedSheet planned in described.Planned)
            {
                if (planned.NamedFromItsView) continue;

                block.Children.Add(Faint(
                    planned.ViewsInWords() + ": " + planned.WhyNothingIsProposed()));
            }

            var batchLine = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Margin = PanelMetrics.Row
            };
            block.Children.Add(batchLine);

            int whichSheet = at;
            _numberWarnings.Add(fresh => SayBatchLine(batchLine, whichSheet, fresh));
            SayBatchLine(batchLine, whichSheet, rows);

            block.Children.Add(Scrolling(
                SheetTable(sheet, at, rows, offers), PanelMetrics.ListHeight, "sheet " + at + " table"));

            return new Border
            {
                Background = _theme.StepHeader,
                BorderBrush = _theme.Line,
                BorderThickness = PanelMetrics.Hairline,
                Margin = PanelMetrics.Row,
                Child = block
            };
        }

        private void ForgetTheNumberWarnings()
        {
            _numberWarnings.Clear();
        }

        private void SheetViewTicked(SheetBeingDescribed sheet, ViewType which, bool carried)
        {
            if (_filling) return;

            sheet.Carry(which, carried);

            // Eleven title blocks were picked by hand on the first real run and the same eleven
            // would have been picked again for every plot. A sheet with none yet takes the one
            // the settings name for the view just ticked. One already picked is left alone,
            // because it was a choice and this is only an offer.
            if (carried && sheet.TitleBlock == null)
            {
                TitleBlockPairing pairing = _titleBlocks.For(which);
                TitleBlockType held = pairing == null ? null : TitleBlockNamed(pairing);

                if (held != null) sheet.TitleBlock = held;
                else if (pairing != null) Say(TitleBlockSettings.WhyUnset(pairing));
            }

            Redraw();
        }

        /// <summary>
        /// The title block type this model holds under the name the settings give, or null when
        /// it holds none. The settings are shared across projects, so that is ordinary.
        /// </summary>
        private TitleBlockType TitleBlockNamed(TitleBlockPairing pairing)
        {
            foreach (TitleBlockType one in _model.TitleBlockTypes)
            {
                if (string.Equals(one.FamilyName, pairing.FamilyName, StringComparison.Ordinal)
                    && string.Equals(one.TypeName, pairing.TypeName, StringComparison.Ordinal))
                {
                    return one;
                }
            }

            return null;
        }

        /// <summary>
        /// Remembers the title block against every view type ticked on that sheet, because those
        /// are the views the user has just said it is for. A sheet with nothing ticked pairs it
        /// with nothing, since there is no view type to remember it against.
        /// </summary>
        private void RememberTheTitleBlock(SheetBeingDescribed sheet)
        {
            if (sheet.TitleBlock == null) return;

            TitleBlockSettings settings = _titleBlocks;
            foreach (ViewType which in sheet.Built(_columns.Shown).Views)
            {
                settings = settings.With(
                    which, sheet.TitleBlock.FamilyName, sheet.TitleBlock.TypeName);
            }

            if (ReferenceEquals(settings, _titleBlocks)) return;

            _titleBlocks = settings;

            string refused = TitleBlockSettingsStore.Save(settings);
            if (refused.Length > 0) Say(refused);
        }

        /// <summary>
        /// The user's own file first, then the defaults shipped beside the add-in. Every line
        /// neither could read is kept, because a settings file one line short reads exactly like
        /// one that never had the line.
        /// </summary>
        private void ReadTheTitleBlockSettings()
        {
            StoredSettings stored = TitleBlockSettingsStore.Read();

            _titleBlocks = stored.Settings;
            _settingsNotRead = stored.NotRead;
        }

        /// <summary>
        /// The line over a definition's table: how many sheets this one press makes across the
        /// ticked plots, and how many rows are still short of a name or a number.
        /// </summary>
        private void SayBatchLine(
            TextBlock line, int whichSheet, IReadOnlyList<IReadOnlyList<SheetRowShown>> rows)
        {
            if (whichSheet < 0 || whichSheet >= _sheets.Count || whichSheet >= rows.Count) return;

            line.Text = new SheetBatch(
                _sheets[whichSheet].Built(_columns.Shown),
                rows[whichSheet].Select(one => one.Row)).InWords();
        }

        /// <summary>
        /// One row per sheet that will be made, per ticked plot: the views it holds, read
        /// only, and its name and number, prefilled where they could be proposed and editable
        /// everywhere. Editing either marks it typed, and the report says which was which.
        /// </summary>
        private UIElement SheetTable(
            SheetBeingDescribed sheet,
            int at,
            IReadOnlyList<IReadOnlyList<SheetRowShown>> rows,
            SheetOffers offers)
        {
            var table = new Grid();
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            table.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(1.0, GridUnitType.Star)
            });
            table.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(PanelMetrics.ColumnWidth)
            });
            table.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(PanelMetrics.ColumnWidth)
            });

            table.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Put(table, HeaderCell("Plot"), 0, 0);
            Put(table, HeaderCell("Views"), 0, 1);
            Put(table, HeaderCell("Name"), 0, 2);
            Put(table, HeaderCell("Number"), 0, 3);

            IReadOnlyList<SheetRowShown> mine =
                at < rows.Count ? rows[at] : new List<SheetRowShown>();

            for (int row = 0; row < mine.Count; row++)
            {
                SheetRowShown shown = mine[row];
                table.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                Put(table, new TextBlock
                {
                    Text = shown.PlotId,
                    Margin = PanelMetrics.CellPad,
                    VerticalAlignment = VerticalAlignment.Center
                }, row + 1, 0);

                Put(table, new TextBlock
                {
                    Text = shown.Planned.ViewsInWords(),
                    Margin = PanelMetrics.CellPad,
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Center
                }, row + 1, 1);

                Put(table, NameBox(sheet, at, row, shown, offers), row + 1, 2);
                Put(table, NumberBox(sheet, at, row, shown, rows, offers), row + 1, 3);
            }

            return table;
        }

        private UIElement NameBox(
            SheetBeingDescribed sheet, int at, int row, SheetRowShown shown, SheetOffers offers)
        {
            var box = new ComboBox
            {
                IsEditable = true,
                Margin = PanelMetrics.Row,
                Text = shown.Row.SheetName
            };

            // The names already in use, an offer and never a restriction. The box starts on
            // the name built from the sheet's one view, which the user can type over. The
            // list is the one every other box on the panel shows, not a copy of it.
            box.ItemsSource = offers.NamesInUse;

            string plotId = shown.PlotId;
            string signature = shown.Planned.Signature;
            string starting = shown.Row.SheetName;
            box.Loaded += (sender, e) =>
            {
                _filling = true;
                box.Text = starting;
                _filling = false;
            };

            // A keystroke changes what the headers and the other boxes say and nothing else,
            // because rebuilding the tree under the cursor takes the cursor out of the box.
            box.AddHandler(
                System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent,
                new TextChangedEventHandler((sender, e) =>
                {
                    if (_filling) return;
                    sheet.TypeName(plotId, signature, box.Text ?? string.Empty);
                    RefreshHeaders();
                }));

            box.SelectionChanged += (sender, e) =>
            {
                if (_filling) return;
                sheet.TypeName(plotId, signature,
                    (box.SelectedItem as string) ?? box.Text ?? string.Empty);
                RefreshHeaders();
            };

            int whichSheet = at;
            int whichRow = row;
            _numberWarnings.Add(fresh => RefillBox(box, whichSheet, whichRow, fresh, true));

            return box;
        }

        private UIElement NumberBox(
            SheetBeingDescribed sheet,
            int at,
            int row,
            SheetRowShown shown,
            IReadOnlyList<IReadOnlyList<SheetRowShown>> rows,
            SheetOffers offers)
        {
            var box = new ComboBox
            {
                IsEditable = true,
                Margin = PanelMetrics.Row,
                Text = shown.Row.SheetNumber
            };

            // Numbers no sheet in this model carries. It used to offer the ones in use, so
            // every entry in it was certain to be refused, and three sheets were lost to that
            // in one run. Free typing stays, because the list is an offer and never a
            // restriction.
            box.ItemsSource = offers.FreeNumbers;

            var wrong = new TextBlock
            {
                Foreground = _theme.Warning,
                TextWrapping = TextWrapping.Wrap,
                Visibility = Visibility.Collapsed
            };

            string plotId = shown.PlotId;
            string signature = shown.Planned.Signature;
            string starting = shown.Row.SheetNumber;
            box.Loaded += (sender, e) =>
            {
                _filling = true;
                box.Text = starting;
                _filling = false;
            };

            box.AddHandler(
                System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent,
                new TextChangedEventHandler((sender, e) =>
                {
                    if (_filling) return;
                    sheet.TypeNumber(plotId, signature, box.Text ?? string.Empty);
                    RefreshHeaders();
                }));

            box.SelectionChanged += (sender, e) =>
            {
                if (_filling) return;
                sheet.TypeNumber(plotId, signature,
                    (box.SelectedItem as string) ?? box.Text ?? string.Empty);
                RefreshHeaders();
            };

            int whichSheet = at;
            int whichRow = row;
            _numberWarnings.Add(fresh =>
            {
                RefillBox(box, whichSheet, whichRow, fresh, false);
                SayRowFault(wrong, whichSheet, whichRow, fresh);
            });
            SayRowFault(wrong, at, row, rows);

            var stacked = new StackPanel();
            stacked.Children.Add(box);
            stacked.Children.Add(wrong);
            return stacked;
        }

        /// <summary>
        /// Puts a fresh value into a box the user is not typing in. A proposal moves when a
        /// typed number takes the one it offered, and a box left showing the old one would
        /// have the run make a sheet the screen never showed.
        /// </summary>
        private void RefillBox(
            ComboBox box,
            int whichSheet,
            int whichRow,
            IReadOnlyList<IReadOnlyList<SheetRowShown>> rows,
            bool name)
        {
            if (box.IsKeyboardFocusWithin) return;
            if (whichSheet < 0 || whichSheet >= rows.Count) return;
            if (whichRow < 0 || whichRow >= rows[whichSheet].Count) return;

            SheetRowShown shown = rows[whichSheet][whichRow];
            string fresh = name ? shown.Row.SheetName : shown.Row.SheetNumber;
            if (string.Equals(box.Text, fresh, StringComparison.Ordinal)) return;

            _filling = true;
            box.Text = fresh;
            _filling = false;
        }

        /// <summary>
        /// What is wrong with one row's number, said under the box as it is typed rather than
        /// found out from a refusal after Run. The same method the run summary counts with, so
        /// the two can never disagree. An empty box that would have been proposed a number
        /// says why it was not.
        /// </summary>
        private void SayRowFault(
            TextBlock wrong,
            int whichSheet,
            int whichRow,
            IReadOnlyList<IReadOnlyList<SheetRowShown>> rows)
        {
            if (whichSheet < 0 || whichSheet >= rows.Count
                || whichRow < 0 || whichRow >= rows[whichSheet].Count)
            {
                wrong.Visibility = Visibility.Collapsed;
                return;
            }

            SheetRowShown shown = rows[whichSheet][whichRow];

            List<string> asked = rows
                .SelectMany(one => one)
                .Select(one => one.Row.SheetNumber)
                .ToList();

            SheetNumberFault fault = SheetNumbers.FaultIn(
                shown.Row.SheetNumber, _model.SheetNumbersInUse, asked);

            if (fault != SheetNumberFault.None)
            {
                wrong.Text = SheetNumbers.FaultInWords(fault);
                wrong.Visibility = Visibility.Visible;
                return;
            }

            if (!shown.Row.HasNumber && shown.WhyNoNumber.Length > 0)
            {
                wrong.Text = shown.WhyNoNumber;
                wrong.Visibility = Visibility.Visible;
                return;
            }

            wrong.Visibility = Visibility.Collapsed;
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

        /// <summary>
        /// Takes a described sheet out, asking first when any of its rows holds typed text. A
        /// row of typed numbers over seventeen plots is the most expensive thing on the panel
        /// to type and the one thing with no way back, and Run and Assign both confirm.
        /// </summary>
        private void RemoveASheet(SheetBeingDescribed sheet)
        {
            int at = _sheets.IndexOf(sheet);
            if (at < 0) return;

            IReadOnlyList<IReadOnlyList<SheetRowShown>> rows = DescribedRows();
            var batch = new SheetBatch(sheet.Built(_columns.Shown), rows[at].Select(one => one.Row));

            string asks = batch.WhyRemovalAsks();
            if (asks.Length > 0)
            {
                MessageBoxResult answer = MessageBox.Show(
                    asks,
                    "Remove sheet " + (at + 1),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No);

                if (answer != MessageBoxResult.Yes) return;
            }

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
            string clashes = SheetNumbers.InWords(SheetNumbersThatWillBeRefused());

            return PlanNow().InWords()
                + " Only marked cells on ticked plots are made, and nothing is copied from "
                + "another plot. A sheet is made only once its row in step 4 has a name and a "
                + "number."
                + (clashes.Length == 0 ? string.Empty : " " + clashes);
        }

        /// <summary>
        /// How many sheets this run would ask Revit for under a number it will not take. Three
        /// sheets were refused that way in one run and nothing said so until afterwards.
        /// </summary>
        private int SheetNumbersThatWillBeRefused()
        {
            return SheetNumbers
                .Problems(SheetsWanted(), _model.SheetNumbersInUse)
                .Count;
        }

        /// <summary>
        /// The six counts, with every case but B openable in one click. It sits inside step 5
        /// because it acts on the same ticked plots the run does.
        ///
        /// A count on its own is not enough. On the real model F was 1, one view carrying a
        /// scope box that is not its plot's, and finding out which view that was meant opening
        /// a report file and reading down it.
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

            block.Children.Add(Faint(PanelSteps.ScopeBoxLine(counts.Considered, _picked.TickedCount)));

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
                Content = Label((open ? "-  " : "+  ") + label + ": " + inIt.Count),
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
                    Content = Label(one.PlotId + "   " + one.ViewName
                        + (one.CurrentScopeBoxName.Length > 0 ? "   holds " + one.CurrentScopeBoxName : string.Empty)),
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

        /// <summary>
        /// A name on a control that reads its content as a caption. WPF takes the first
        /// underscore in a CheckBox or Button caption as an access key marker, swallows it and
        /// underlines the next letter, so PRX_Plot_ID read as PRXPlot_ID on the KPI pane, and
        /// this panel's job is exact names. The escape is in Shared, where both panels read
        /// it. It was KPI's and was called across the fence for one round.
        /// </summary>
        private static string Label(string text)
        {
            return PaneLabel.Escaped(text);
        }

        private Button Secondary(string text, Action clicked, string why)
        {
            var button = new Button
            {
                Content = Label(text),
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

        /// <summary>
        /// A scrolling list that comes back where it was left.
        ///
        /// Ticking a view type near the bottom of 84 threw the list back to the top, so the
        /// user scrolled down again for every single tick. The step is thrown away and built
        /// again on every change, and a brand new ScrollViewer starts at nothing. Every list on
        /// the panel goes through here now. The grid and the step 4 lists used to be built
        /// bare, so every mark and every tick in step 4 threw them back to the top, the same
        /// fault a round after it was fixed for the two lists it was reported on.
        /// </summary>
        private UIElement Scrolling(UIElement what, double tall, string remembered)
        {
            return Remembering(new ScrollViewer
            {
                Content = what,
                MaxHeight = tall,
                Margin = PanelMetrics.Row,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            }, remembered);
        }

        /// <summary>
        /// Keeps a viewer's position across the rebuild, by name, both ways: the grid scrolls
        /// down through its rows and across through its columns.
        ///
        /// The offsets are restored on the first layout pass rather than on Loaded, because a
        /// ScrollViewer that has not measured its content yet clamps any offset to zero and
        /// the restore reads as though it worked.
        /// </summary>
        private ScrollViewer Remembering(ScrollViewer view, string remembered)
        {
            string down = remembered + " down";
            string across = remembered + " across";

            double wasDown;
            double wasAcross;
            bool hadDown = _scrolledTo.TryGetValue(down, out wasDown) && wasDown > 0.0;
            bool hadAcross = _scrolledTo.TryGetValue(across, out wasAcross) && wasAcross > 0.0;

            if (hadDown || hadAcross)
            {
                EventHandler once = null;
                once = (sender, e) =>
                {
                    view.LayoutUpdated -= once;
                    if (hadDown) view.ScrollToVerticalOffset(wasDown);
                    if (hadAcross) view.ScrollToHorizontalOffset(wasAcross);
                };
                view.LayoutUpdated += once;
            }

            view.ScrollChanged += (sender, e) =>
            {
                _scrolledTo[down] = view.VerticalOffset;
                _scrolledTo[across] = view.HorizontalOffset;
            };

            return view;
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
                GridNow().Marked,
                _picked.Ticked,
                _model.PlotsWithAScopeBox,
                _model.ScheduleTypes,
                _model.SectionTypes,
                _model.CapturableScheduleTypes,
                SheetsWanted(),
                _model.Present.Select(one => one.Where),
                _model.UncapturableSchedules);
        }

        /// <summary>
        /// Every described sheet's rows, one list per sheet, with one set of taken numbers
        /// threaded through the lot so no two proposals on this panel ever offer one number.
        /// </summary>
        private IReadOnlyList<IReadOnlyList<SheetRowShown>> DescribedRows()
        {
            var taken = new HashSet<string>(StringComparer.Ordinal);
            foreach (string number in _model.SheetNumbersInUse)
            {
                string held = (number ?? string.Empty).Trim();
                if (held.Length > 0) taken.Add(held);
            }

            IReadOnlyList<ViewType> ticked = _columns.Shown;
            IReadOnlyList<string> plots = _picked.Ticked;

            return _sheets
                .Select(one => one.RowsFor(ticked, plots, _model, taken))
                .ToList();
        }

        private IReadOnlyList<SheetBatch> SheetsWanted()
        {
            IReadOnlyList<IReadOnlyList<SheetRowShown>> rows = DescribedRows();
            IReadOnlyList<ViewType> ticked = _columns.Shown;

            var batches = new List<SheetBatch>();
            for (int at = 0; at < _sheets.Count; at++)
            {
                batches.Add(new SheetBatch(
                    _sheets[at].Built(ticked),
                    rows[at].Select(one => one.Row)));
            }

            return batches;
        }

        /// <summary>
        /// How many sheets a run would make right now: every row of every usable definition
        /// that has its name and its number.
        /// </summary>
        private int SheetsToMake()
        {
            return SheetsWanted().Sum(one => one.WillBeMade);
        }

        private void AskToRun()
        {
            IReadOnlyList<string> ticked = _picked.Ticked;
            if (ticked.Count == 0)
            {
                Say(PanelSteps.NoPlotsTicked("run"));
                return;
            }

            if (PlanNow().MakesNothing)
            {
                Say(PanelSteps.NothingToRunYet);
                return;
            }

            Say("Working out the run.");
            _handler.AskToRun(ticked, GridNow().Marked, SheetsWanted());
            _asking.Raise();
        }

        private void AskToAssign()
        {
            IReadOnlyList<string> ticked = _picked.Ticked;
            if (ticked.Count == 0)
            {
                Say(PanelSteps.NoPlotsTicked("assign"));
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
        /// had changed, which is most of why an early version looked broken. The plots the user
        /// unticked are put back the same way, because putting the range back rebuilds the
        /// selection with every plot ticked and a refresh used to undo five unticks in silence.
        /// The marks are cleared, since the model they were made against has been read again,
        /// and what comes back is the clause the refresh message carries about that.
        /// </summary>
        private string Took(DrawingSheetSnapshot snapshot)
        {
            return Dispatcher.Invoke(() =>
            {
                string prefixWas = _prefix.SelectedItem as string;
                string fromWas = _from.SelectedItem as string;
                string toWas = _to.SelectedItem as string;
                List<string> untickedWere = _picked.InRange
                    .Where(plotId => !_picked.IsTicked(plotId))
                    .ToList();

                _model = snapshot ?? DrawingSheetSnapshot.Nothing;
                _readOnce = true;
                int marksCleared = _marked.Count;
                _marked.Clear();
                _leftPlotsAlready = false;
                _columns = _columns.OverTheseTypes(_model.ViewTypes);

                FillPrefixes();
                PutTheRangeBack(prefixWas, fromWas, toWas);

                // Ticking is a no-op for a plot no longer in the range, so a plot that has gone
                // from the model is simply not put back.
                foreach (string plotId in untickedWere) _picked = _picked.Ticking(plotId, false);

                _stepOpen = StepsNow().FirstUnfinished;
                Redraw();

                return marksCleared == 0 ? string.Empty : BulkMarking.ClearedInWords(marksCleared);
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
            return PanelSteps.NothingToDraw(_readOnce, _model.Empty, _prefix.SelectedItem != null);
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
            Say(PanelSteps.MarkedLine(GridNow().MarkedCount));
        }

        private void OpenTheView(long viewId)
        {
            _handler.AskToSelect(viewId);
            _asking.Raise();
        }
    }
}
