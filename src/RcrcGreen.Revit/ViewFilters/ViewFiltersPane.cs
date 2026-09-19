using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Autodesk.Revit.UI;
using RcrcGreen.Core;
using RcrcGreen.Core.ViewFilters;
using RcrcGreen.Revit.Kpi;
using ComboBox = System.Windows.Controls.ComboBox;
using TextBox = System.Windows.Controls.TextBox;

namespace RcrcGreen.Revit.ViewFilters
{
    /// <summary>
    /// The View Filters pane: a numbered rail down the left and one step's controls beside
    /// it, KEYWORDS, FILTER ROWS, SCAN, APPLY and RESULTS. Modeless, and it names no
    /// Document, no Transaction and no ElementId. Everything it wants goes through
    /// <see cref="ViewFiltersRequestHandler"/> and its own external event, never another
    /// pane's, so a failure in one pane cannot cost another.
    ///
    /// No brush and no number is written here: colours come from PanelTheme and every size
    /// from PanelMetrics, which is what kept the first panel from shipping black on black a
    /// second time. Every caption on a button or a tick box goes through PaneLabel.
    ///
    /// No reachability decision lives here. Which step can be used, which is finished and
    /// what a shut one says are all <see cref="ViewFilterSteps"/>, worked out by comparing
    /// through ApplyGate rather than by a flag, and this file only draws the answers.
    /// Whether a model is open, and whether it is still the scanned one, is decided on the
    /// Revit thread when a button is pressed, because a pane holds no copy of anything it
    /// can ask for.
    /// </summary>
    internal sealed class ViewFiltersPane : UserControl, IDockablePaneProvider
    {
        private readonly ExternalEvent _asking;

        private readonly ViewFiltersRequestHandler _handler;

        private readonly TextBox _keywords = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Height = PanelMetrics.KeywordsHeight
        };

        private readonly StackPanel _rowsPanel = new StackPanel();

        private readonly StackPanel _scanArea = new StackPanel();

        private readonly StackPanel _resultsArea = new StackPanel();

        private readonly StackPanel _logList = new StackPanel();

        private readonly TextBlock _said = new TextBlock { TextWrapping = TextWrapping.Wrap };

        private readonly List<FilterRowEditor> _rows = new List<FilterRowEditor>();

        private Border _strip;

        private Border _status;

        private Button _scan;

        private Button _apply;

        private TextBlock _applyWhy;

        private Button _openReport;

        private TextBlock _settingsLine;

        private TextBlock _header;

        // The rail's five cells and what each shows: the number until the step is done and
        // the tick after. Built once, restyled by RefreshSteps, never rebuilt.
        private Border[] _railCells;

        private TextBlock[] _railNumbers;

        private System.Windows.Shapes.Path[] _railTicks;

        private FrameworkElement[] _bodies;

        private ViewFilterStep _open = ViewFilterStep.Keywords;

        private PanelTheme _theme;

        // The inputs the last press sent, and the inputs the last answered scan used. The
        // first is captured at the press so an edit made while Revit works cannot pass as
        // scanned. The second is what ApplyGate compares the boxes against, through
        // ViewFilterSteps.
        private Inputs _askedInputs;

        private Inputs _scannedInputs;

        // What the last answered scan read and what the last answered run did. Records of
        // answers this pane was handed, not copies of anything it could ask for.
        private ViewFilterScanResult _scannedResult;

        private ViewFilterResult _run;

        private KpiProgressWindow _running;

        // The marks the theme repaint walk looks for. The rows and the lists are built once
        // and kept, so a Revit theme switch has to find their quiet brushes where they sit
        // rather than trusting a rebuild that never comes. The rail cells wear no mark,
        // because their brushes follow their state and RefreshSteps restyles them whole.
        private const string FaintMark = "theme faint";

        private const string HairlineMark = "theme hairline";

        private const string ShadeMark = "theme shade";

        public ViewFiltersPane()
        {
            _handler = new ViewFiltersRequestHandler
            {
                Scanned = Scanned,
                Applied = Applied,
                Told = Say,
                Progressed = Moved,
                Logged = Logged
            };
            _asking = ExternalEvent.Create(_handler);

            _keywords.Text = ViewFilterWords.DefaultKeywords;
            _keywords.TextChanged += (sender, e) => RefreshSteps();

            Layout();
            StartingRows();
            _open = Steps().FirstUnfinished;
            PaintFromTheTheme();

            IsVisibleChanged += (sender, e) => PaintFromTheTheme();
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this;
            data.InitialState = new DockablePaneState
            {
                DockPosition = DockPosition.Right
            };
        }

        private void Layout()
        {
            FontSize = PanelMetrics.Body;

            var heading = new TextBlock
            {
                Text = "View Filters",
                FontWeight = FontWeights.Bold
            };
            var hint = new TextBlock
            {
                Text = "Scan reads and changes nothing. Apply writes once, one undo.",
                TextWrapping = TextWrapping.Wrap
            };
            var stripInside = new StackPanel { Margin = PanelMetrics.StripInside };
            stripInside.Children.Add(heading);
            stripInside.Children.Add(hint);
            _strip = new Border { Child = stripInside };

            _said.Margin = PanelMetrics.StripInside;
            _status = new Border
            {
                Child = _said,
                BorderThickness = PanelMetrics.HairlineAbove
            };

            _header = new TextBlock
            {
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                Margin = PanelMetrics.Heading
            };

            _bodies = new FrameworkElement[]
            {
                KeywordsBody(), RowsBody(), ScanBody(), ApplyBody(), ResultsBody()
            };

            var bodyColumn = new StackPanel();
            bodyColumn.Children.Add(_header);
            foreach (FrameworkElement body in _bodies)
            {
                bodyColumn.Children.Add(body);
            }

            // Only the body scrolls. The rail sits outside the scroller, so the way to
            // another step stays on screen however long the open one grows.
            var scrolled = new ScrollViewer
            {
                Content = bodyColumn,
                Padding = PanelMetrics.Edge,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };

            var middle = new Grid();
            middle.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            middle.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(1.0, GridUnitType.Star)
            });

            StackPanel rail = Rail();
            Grid.SetColumn(rail, 0);
            Grid.SetColumn(scrolled, 1);
            middle.Children.Add(rail);
            middle.Children.Add(scrolled);

            var whole = new DockPanel { LastChildFill = true };
            DockPanel.SetDock(_strip, Dock.Top);
            DockPanel.SetDock(_status, Dock.Bottom);
            whole.Children.Add(_strip);
            whole.Children.Add(_status);
            whole.Children.Add(middle);

            Content = whole;
        }

        /// <summary>
        /// The five cells, one per step, each holding its number and its tick with one of
        /// the two showing. The tick is a drawn shape rather than a character, so no font
        /// gets a say in what done looks like. Every brush is set by RefreshSteps, because
        /// a cell's colours follow its state.
        /// </summary>
        private StackPanel Rail()
        {
            var rail = new StackPanel
            {
                Width = PanelMetrics.RailWidth,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = PanelMetrics.Row
            };

            _railCells = new Border[5];
            _railNumbers = new TextBlock[5];
            _railTicks = new System.Windows.Shapes.Path[5];

            for (int at = 0; at < 5; at++)
            {
                ViewFilterStep step = (ViewFilterStep)(at + 1);

                _railNumbers[at] = new TextBlock
                {
                    Text = (at + 1).ToString(CultureInfo.InvariantCulture),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                _railTicks[at] = new System.Windows.Shapes.Path
                {
                    Data = Geometry.Parse("M 0,5.2 L 2,3.2 L 4.4,5.6 L 10,0 L 12,2 L 4.4,9.6 Z"),
                    Stretch = Stretch.Uniform,
                    Width = PanelMetrics.RailTick,
                    Height = PanelMetrics.RailTick,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Visibility = Visibility.Collapsed
                };

                var inside = new Grid();
                inside.Children.Add(_railNumbers[at]);
                inside.Children.Add(_railTicks[at]);

                var cell = new Border
                {
                    Child = inside,
                    Width = PanelMetrics.RailCell,
                    Height = PanelMetrics.RailCell,
                    CornerRadius = new CornerRadius(PanelMetrics.RailCell / 2.0),
                    Margin = PanelMetrics.Row,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                cell.MouseLeftButtonUp += (sender, e) => RailClicked(step);

                _railCells[at] = cell;
                rail.Children.Add(cell);
            }

            return rail;
        }

        private FrameworkElement KeywordsBody()
        {
            var body = new StackPanel();
            body.Children.Add(Faint(
                "A view is read when its name holds any of these, one per line. A comma or "
                + "a semicolon also separates them."));
            body.Children.Add(WithRowMargin(_keywords));
            return body;
        }

        private FrameworkElement RowsBody()
        {
            var body = new StackPanel();
            _settingsLine = Faint(string.Empty);
            body.Children.Add(_settingsLine);
            body.Children.Add(_rowsPanel);

            var addRow = new Button
            {
                Content = PaneLabel.Escaped("Add row"),
                Padding = PanelMetrics.CellPad,
                Margin = PanelMetrics.Row,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            addRow.Click += (sender, e) => AddRow(EmptyRow());
            body.Children.Add(addRow);
            return body;
        }

        private FrameworkElement ScanBody()
        {
            var body = new StackPanel();
            body.Children.Add(Faint(
                "Rows are plots, columns are the filter prefixes, and each square says "
                + "whether that filter exists, will be created, or cannot be."));
            _scan = new Button
            {
                Content = PaneLabel.Escaped("Scan"),
                Padding = PanelMetrics.CellPad,
                Margin = PanelMetrics.Row,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            _scan.Click += (sender, e) => AskedToScan();
            body.Children.Add(_scan);
            body.Children.Add(_scanArea);
            return body;
        }

        private FrameworkElement ApplyBody()
        {
            var body = new StackPanel();
            _apply = new Button
            {
                Content = PaneLabel.Escaped("Apply"),
                Padding = PanelMetrics.CellPad,
                Margin = PanelMetrics.Row,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            _apply.Click += (sender, e) => AskedToApply();
            _applyWhy = Faint(string.Empty);
            body.Children.Add(_apply);
            body.Children.Add(_applyWhy);
            return body;
        }

        private FrameworkElement ResultsBody()
        {
            var body = new StackPanel();
            body.Children.Add(_resultsArea);

            _openReport = new Button
            {
                Content = PaneLabel.Escaped("Open the report"),
                Padding = PanelMetrics.CellPad,
                Margin = PanelMetrics.Row,
                HorizontalAlignment = HorizontalAlignment.Left,
                Visibility = Visibility.Collapsed
            };
            _openReport.Click += (sender, e) => OpenReport();
            body.Children.Add(_openReport);

            body.Children.Add(Scrolling(_logList, PanelMetrics.ListHeight));
            return body;
        }

        /// <summary>
        /// The rows start from ViewFilters.json beside the installed add-in, and the line
        /// above them says so, or says what stopped the read. A file that cannot be read
        /// leaves one empty row, never a guessed set.
        /// </summary>
        private void StartingRows()
        {
            if (!ViewFiltersStore.Exists())
            {
                _settingsLine.Text = ViewFilterWords.MissingSettings(ViewFiltersFile.FileName);
                AddRow(EmptyRow());
                return;
            }

            ViewFiltersFileRead read = ViewFiltersFile.Read(ViewFiltersStore.ReadJson());
            if (!read.WasRead || read.Rows.Count == 0)
            {
                _settingsLine.Text = ViewFilterWords.BrokenSettings(
                    ViewFiltersFile.FileName,
                    read.WasRead ? "It holds no rows." : read.Problem);
                AddRow(EmptyRow());
                return;
            }

            _settingsLine.Text = ViewFilterWords.DefaultsLine(ViewFiltersFile.FileName);
            foreach (FilterConfig row in read.Rows)
            {
                AddRow(row);
            }
        }

        private static FilterConfig EmptyRow()
        {
            return new FilterConfig
            {
                Prefix = string.Empty,
                Enabled = true,
                Visible = true,
                LineColor = string.Empty,
                LineWeight = LineWeights.Default,
                ForegroundPatternType = PatternTypes.None,
                ForegroundPatternColor = string.Empty,
                BackgroundPatternType = PatternTypes.None,
                BackgroundPatternColor = string.Empty
            };
        }

        private void AddRow(FilterConfig starting)
        {
            var editor = new FilterRowEditor(starting, RefreshSteps, RemoveRow);
            _rows.Add(editor);
            _rowsPanel.Children.Add(editor.Root);
            RefreshRemoveButtons();
            RefreshSteps();
        }

        /// <summary>At least one row stays, so Remove greys out on the last one.</summary>
        private void RemoveRow(FilterRowEditor editor)
        {
            if (_rows.Count <= 1) return;

            _rows.Remove(editor);
            _rowsPanel.Children.Remove(editor.Root);
            RefreshRemoveButtons();
            RefreshSteps();
        }

        private void RefreshRemoveButtons()
        {
            foreach (FilterRowEditor editor in _rows)
            {
                editor.RemoveAllowed(_rows.Count > 1);
            }
        }

        private Inputs CurrentInputs()
        {
            var filters = new List<FilterConfig>();
            foreach (FilterRowEditor editor in _rows)
            {
                filters.Add(editor.Collected());
            }

            return new Inputs
            {
                ViewKeywords = _keywords.Text,
                Filters = filters.ToArray()
            };
        }

        /// <summary>
        /// The five steps off what the pane holds right now, which is the only question
        /// this file ever asks about reachability.
        /// </summary>
        private ViewFilterSteps Steps()
        {
            return ViewFilterSteps.Of(CurrentInputs(), _scannedInputs, _scannedResult, _run);
        }

        /// <summary>
        /// Redraws the rail and the open step from <see cref="ViewFilterSteps"/>: the tick
        /// or the number, the fill on the open cell, the tooltip every cell carries, which
        /// body shows, and whether Apply can be pressed with the one line saying why not.
        /// Worked out by comparing, never by a flag, so an edit undone by hand brings a
        /// step back without anything to reset.
        /// </summary>
        private void RefreshSteps()
        {
            if (_railCells == null || _apply == null || _theme == null) return;

            ViewFilterSteps steps = Steps();

            foreach (ViewFilterStepState step in steps.All)
            {
                int at = step.Number - 1;
                bool open = step.Step == _open;

                _railCells[at].Background = open ? _theme.Primary : _theme.Clear;
                _railCells[at].ToolTip = step.Tip;

                Brush inside = open
                    ? _theme.OnPrimary
                    : (step.Usable ? _theme.Foreground : _theme.Faint);
                _railNumbers[at].Foreground = inside;
                _railTicks[at].Fill = inside;

                _railNumbers[at].Visibility = step.Done ? Visibility.Collapsed : Visibility.Visible;
                _railTicks[at].Visibility = step.Done ? Visibility.Visible : Visibility.Collapsed;

                _bodies[at].Visibility = open ? Visibility.Visible : Visibility.Collapsed;
            }

            _header.Text = steps.For(_open).Header;

            ViewFilterStepState apply = steps.For(ViewFilterStep.Apply);
            _apply.IsEnabled = apply.Usable;
            _applyWhy.Text = apply.WhyNot;
        }

        /// <summary>
        /// A click on a cell opens its step when Core says the step can be used, and does
        /// nothing at all when it cannot, which is what a muted number promises.
        /// </summary>
        private void RailClicked(ViewFilterStep step)
        {
            if (!Steps().For(step).Usable) return;

            _open = step;
            RefreshSteps();
        }

        private void Ask(ViewFiltersRequest wanted)
        {
            _handler.Ask(wanted);
            _asking.Raise();
        }

        private void AskedToScan()
        {
            // The handler gets a copy, because the ported run trims each row's prefix on
            // the row object itself and the original is the pane's record of the press.
            _askedInputs = CurrentInputs();
            _handler.Asked = InputsCopy.Deep(_askedInputs);
            Say(ViewFilterWords.Scanning);
            Ask(ViewFiltersRequest.Scan);
        }

        private void AskedToApply()
        {
            _askedInputs = CurrentInputs();
            _handler.Asked = InputsCopy.Deep(_askedInputs);

            _resultsArea.Children.Clear();
            _logList.Children.Clear();

            // The last run's record goes with its lines, or a press that ends in a status
            // sentence instead of an answer leaves RESULTS wearing the old run's tick and
            // counts over the body this just emptied, with its button opening the old
            // run's report.
            _run = null;
            _openReport.Visibility = Visibility.Collapsed;
            RefreshSteps();

            // Opened here and not inside Execute: this is a click handler on the pane's own
            // thread, so the window is up before the external event is raised. Told closes
            // it, whatever ends the run.
            Shut();
            _running = KpiProgressWindow.Opened(ViewFilterWords.Applying);

            Ask(ViewFiltersRequest.Apply);
        }

        // The handler calls everything below from the Revit thread, so the hop to the
        // pane's own thread happens here rather than being forgotten at a call site.

        private void Scanned(ViewFilterScanResult result)
        {
            Dispatcher.Invoke(() =>
            {
                _scannedInputs = _askedInputs;
                _scannedResult = result;
                DrawScan(result);

                // A scan coming back lands the pane on the first step with work left in
                // it, which is Core's answer rather than this file's. Only while the
                // boxes still say what the scan was asked with: an answer landing after
                // an edit must not fold the step the edit is being typed in, so a moved
                // scan, which Core reads as not done, moves nobody.
                ViewFilterSteps steps = Steps();
                if (steps.For(ViewFilterStep.Scan).Done)
                {
                    _open = steps.FirstUnfinished;
                }

                RefreshSteps();
            });
        }

        private void Applied(ViewFilterResult result, string reportSentence)
        {
            Dispatcher.Invoke(() =>
            {
                _run = result;

                _resultsArea.Children.Clear();
                foreach (string line in result.Lines)
                {
                    _resultsArea.Children.Add(new TextBlock
                    {
                        Text = line,
                        Margin = PanelMetrics.Row
                    });
                }

                // One line per thing not applied, the run's own sentence for it, in a
                // bounded scroller because a run can fail a lot of items at once.
                if (result.Failures.Length > 0)
                {
                    var failed = new StackPanel();
                    foreach (ViewFilterFailure failure in result.Failures)
                    {
                        failed.Children.Add(new TextBlock
                        {
                            Text = failure.Why,
                            TextWrapping = TextWrapping.Wrap,
                            Margin = PanelMetrics.Row
                        });
                    }

                    _resultsArea.Children.Add(Scrolling(failed, PanelMetrics.ListHeight));
                }

                if (!string.IsNullOrEmpty(reportSentence))
                {
                    _resultsArea.Children.Add(Faint(reportSentence));
                }

                _openReport.Visibility = result.HasReport
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                // A run that has answered opens its results, the one move the round asked
                // the pane to make on its own.
                _open = ViewFilterStep.Results;
                RefreshSteps();
            });
        }

        /// <summary>
        /// The report is opened with whatever the machine opens text files with. A missing
        /// file or a machine with no association must not take the pane down, so a failure
        /// goes to the status line and nothing else happens.
        /// </summary>
        private void OpenReport()
        {
            ViewFilterResult run = _run;
            if (run == null || !run.HasReport) return;

            try
            {
                System.Diagnostics.Process.Start(run.ReportPlace);
            }
            catch (Exception failed)
            {
                _said.Text = "The report could not be opened. " + failed.Message;
            }
        }

        private void Logged(string line)
        {
            Dispatcher.Invoke(() =>
            {
                _logList.Children.Add(Faint(line));
            });
        }

        private void Say(string line)
        {
            Dispatcher.Invoke(() =>
            {
                _said.Text = line ?? string.Empty;
                Shut();
            });
        }

        private void Moved(string line)
        {
            Dispatcher.Invoke(() =>
            {
                _said.Text = line ?? string.Empty;
                if (_running != null) _running.Moved(line);
            });

            // One empty job at render priority lets the paint through when the pane shares
            // Revit's own thread, and keeps every queued click queued. Render and never
            // background, for the reasons the KPI pane's rules record.
            Dispatcher.Invoke(new Action(() => { }), System.Windows.Threading.DispatcherPriority.Render);
        }

        private void Shut()
        {
            KpiProgressWindow running = _running;
            _running = null;
            if (running != null) running.Done();
        }

        private void DrawScan(ViewFilterScanResult result)
        {
            _scanArea.Children.Clear();

            _scanArea.Children.Add(Faint(ViewFilterWords.ScannedLine(result)));

            if (result.Plots.Count > 0 && result.Prefixes.Count > 0)
            {
                // The one thing on the pane that scrolls sideways, because a grid of plots
                // by prefixes is as wide as it is and squeezing it would make it a lie.
                _scanArea.Children.Add(new ScrollViewer
                {
                    Content = ScanGrid(result),
                    MaxHeight = PanelMetrics.GridHeight,
                    Margin = PanelMetrics.Row,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
                });
            }

            _scanArea.Children.Add(Head(ViewFilterWords.SkippedHeading(result.SkippedViews.Count)));
            if (result.SkippedViews.Count > 0)
            {
                var skipped = new StackPanel();
                foreach (string name in result.SkippedViews)
                {
                    skipped.Children.Add(Faint(name));
                }

                _scanArea.Children.Add(Scrolling(skipped, PanelMetrics.ListHeight));
            }

            _scanArea.Children.Add(Head(ViewFilterWords.BlockedHeading(result.BlockedViews.Count)));
            if (result.BlockedViews.Count > 0)
            {
                var blocked = new StackPanel();
                foreach (BlockedView view in result.BlockedViews)
                {
                    blocked.Children.Add(Faint(view.ViewName + "   template " + view.TemplateName));
                }

                _scanArea.Children.Add(Scrolling(blocked, PanelMetrics.ListHeight));
            }
        }

        private Grid ScanGrid(ViewFilterScanResult result)
        {
            var grid = new Grid();

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            for (int c = 0; c < result.Prefixes.Count; c++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(PanelMetrics.ColumnWidth)
                });
            }

            grid.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(PanelMetrics.RowHeight)
            });
            for (int r = 0; r < result.Plots.Count; r++)
            {
                grid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(PanelMetrics.RowHeight)
                });
            }

            for (int c = 0; c < result.Prefixes.Count; c++)
            {
                grid.Children.Add(Cell(
                    result.Prefixes[c], 0, c + 1, true, result.Prefixes[c]));
            }

            for (int r = 0; r < result.Plots.Count; r++)
            {
                string plot = result.Plots[r];
                grid.Children.Add(Cell(plot, r + 1, 0, true, null));

                for (int c = 0; c < result.Prefixes.Count; c++)
                {
                    ScanCell answer = result.CellAt(plot, c);
                    grid.Children.Add(Cell(
                        ViewFilterWords.CellWords(answer),
                        r + 1,
                        c + 1,
                        false,
                        ViewFilterNames.TargetFilterName(result.Prefixes[c], plot)));
                }
            }

            return grid;
        }

        private UIElement Cell(string text, int row, int column, bool strong, string tip)
        {
            var words = new TextBlock
            {
                Text = text,
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center
            };
            if (strong) words.FontWeight = FontWeights.Bold;

            bool shaded = row > 0 && row % 2 == 0;
            var held = new Border
            {
                Child = words,
                Padding = PanelMetrics.CellPad,
                Background = shaded ? _theme.RowShade : _theme.Clear
            };
            if (shaded) held.Tag = ShadeMark;
            if (!string.IsNullOrEmpty(tip)) held.ToolTip = tip;

            Grid.SetRow(held, row);
            Grid.SetColumn(held, column);
            return held;
        }

        private void PaintFromTheTheme()
        {
            _theme = PanelTheme.Current();
            Background = _theme.Background;
            Foreground = _theme.Foreground;
            _strip.Background = _theme.Strip;
            _status.Background = _theme.Strip;
            _status.BorderBrush = _theme.Line;
            Repaint(Content);

            // The rail's brushes follow each cell's state, so a theme switch redraws them
            // through the same method every other change does.
            RefreshSteps();
        }

        /// <summary>
        /// The rows and the lists are built once and kept, so a Revit theme switch has to
        /// find them where they sit. Every faint line, hairline and row shade carries a
        /// mark, and this walk repaints whatever carries one, which is what stops a kept
        /// control wearing the old theme's quiet brush over the new theme's background.
        /// </summary>
        private void Repaint(object element)
        {
            TextBlock words = element as TextBlock;
            if (words != null && Equals(words.Tag, FaintMark)) words.Foreground = _theme.Faint;

            Border edge = element as Border;
            if (edge != null)
            {
                if (Equals(edge.Tag, HairlineMark)) edge.BorderBrush = _theme.Line;
                if (Equals(edge.Tag, ShadeMark)) edge.Background = _theme.RowShade;
            }

            HexColorBox colours = element as HexColorBox;
            if (colours != null) colours.PaintFromTheTheme();

            DependencyObject holder = element as DependencyObject;
            if (holder == null) return;

            foreach (object child in LogicalTreeHelper.GetChildren(holder))
            {
                Repaint(child);
            }
        }

        private static TextBlock Head(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                Margin = PanelMetrics.Heading
            };
        }

        private TextBlock Faint(string text)
        {
            return new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                Foreground = PanelTheme.Current().Faint,
                Margin = PanelMetrics.Row,
                Tag = FaintMark
            };
        }

        private static UIElement WithRowMargin(FrameworkElement what)
        {
            what.Margin = PanelMetrics.Row;
            return what;
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

        /// <summary>
        /// One filter row's controls, holding exactly the FilterConfig properties. Built
        /// once and kept, because a ComboBox carries its own list and a TextBox carries
        /// what somebody is halfway through typing, so nothing here is rebuilt on a change.
        /// Its option lines wrap rather than run sideways, because the rail takes its own
        /// width off a pane that is already narrow.
        /// </summary>
        private sealed class FilterRowEditor
        {
            private readonly TextBox _prefix = new TextBox { MinWidth = PanelMetrics.ColumnWidth };

            private readonly CheckBox _enabled = New("Enabled", "Whether the filter is enabled in the view.");

            private readonly CheckBox _visible = New("Visible", "Whether the elements the filter matches are visible.");

            private readonly CheckBox _halftone = New("Halftone", "Draw the matched elements halftoned.");

            private readonly CheckBox _lineTick = New("Line colour", "Override the line colour with the colour beside it.");

            private readonly HexColorBox _lineColour = new HexColorBox();

            private readonly ComboBox _weight = Weights();

            private readonly CheckBox _frontTick = New("Foreground pattern", "Override the surface and cut foreground pattern.");

            private readonly ComboBox _frontType = Patterns();

            private readonly HexColorBox _frontColour = new HexColorBox();

            private readonly CheckBox _backTick = New("Background pattern", "Override the surface and cut background pattern.");

            private readonly ComboBox _backType = Patterns();

            private readonly HexColorBox _backColour = new HexColorBox();

            private readonly Button _remove;

            public FilterRowEditor(FilterConfig starting, Action changed, Action<FilterRowEditor> removed)
            {
                _prefix.Text = starting.Prefix ?? string.Empty;
                _enabled.IsChecked = starting.Enabled;
                _visible.IsChecked = starting.Visible;
                _halftone.IsChecked = starting.Halftone;
                _lineTick.IsChecked = starting.OverrideLineColor;
                _lineColour.Hex = starting.LineColor ?? string.Empty;
                _weight.SelectedIndex =
                    starting.LineWeight >= LineWeights.Default && starting.LineWeight <= LineWeights.Heaviest
                        ? starting.LineWeight
                        : LineWeights.Default;
                _frontTick.IsChecked = starting.OverrideForegroundPattern;
                _frontType.SelectedIndex = PatternIndex(starting.ForegroundPatternType);
                _frontColour.Hex = starting.ForegroundPatternColor ?? string.Empty;
                _backTick.IsChecked = starting.OverrideBackgroundPattern;
                _backType.SelectedIndex = PatternIndex(starting.BackgroundPatternType);
                _backColour.Hex = starting.BackgroundPatternColor ?? string.Empty;

                // The override tick greys its own colour box, so a colour that will not be
                // applied does not read as one that will, and a greyed box takes no click.
                _lineColour.IsEnabled = starting.OverrideLineColor;
                _frontColour.IsEnabled = starting.OverrideForegroundPattern;
                _backColour.IsEnabled = starting.OverrideBackgroundPattern;

                _remove = new Button
                {
                    Content = PaneLabel.Escaped("Remove row"),
                    Padding = PanelMetrics.CellPad,
                    Margin = PanelMetrics.Gap
                };
                _remove.Click += (sender, e) => removed(this);

                Root = Built();
                Wire(changed);
            }

            public FrameworkElement Root { get; }

            public void RemoveAllowed(bool allowed)
            {
                _remove.IsEnabled = allowed;
            }

            public FilterConfig Collected()
            {
                return new FilterConfig
                {
                    Prefix = _prefix.Text,
                    Enabled = _enabled.IsChecked == true,
                    Visible = _visible.IsChecked == true,
                    Halftone = _halftone.IsChecked == true,
                    OverrideLineColor = _lineTick.IsChecked == true,
                    LineColor = _lineColour.Hex,
                    LineWeight = _weight.SelectedIndex < 0 ? 0 : _weight.SelectedIndex,
                    OverrideForegroundPattern = _frontTick.IsChecked == true,
                    ForegroundPatternType = TypeWord(_frontType),
                    ForegroundPatternColor = _frontColour.Hex,
                    OverrideBackgroundPattern = _backTick.IsChecked == true,
                    BackgroundPatternType = TypeWord(_backType),
                    BackgroundPatternColor = _backColour.Hex
                };
            }

            private FrameworkElement Built()
            {
                var inside = new StackPanel { Margin = PanelMetrics.StepInside };

                var nameLine = new DockPanel { LastChildFill = true, Margin = PanelMetrics.Row };
                var nameWord = new TextBlock
                {
                    Text = "Prefix",
                    Width = PanelMetrics.LabelWidth,
                    VerticalAlignment = VerticalAlignment.Center
                };
                DockPanel.SetDock(nameWord, Dock.Left);
                DockPanel.SetDock(_remove, Dock.Right);
                nameLine.Children.Add(nameWord);
                nameLine.Children.Add(_remove);
                nameLine.Children.Add(_prefix);
                inside.Children.Add(nameLine);

                var ticks = new WrapPanel { Margin = PanelMetrics.Row };
                ticks.Children.Add(_enabled);
                ticks.Children.Add(_visible);
                ticks.Children.Add(_halftone);
                inside.Children.Add(ticks);

                var line = new WrapPanel { Margin = PanelMetrics.Row };
                line.Children.Add(_lineTick);
                line.Children.Add(_lineColour);
                line.Children.Add(new TextBlock
                {
                    Text = "Weight",
                    Margin = PanelMetrics.Gap,
                    VerticalAlignment = VerticalAlignment.Center
                });
                line.Children.Add(_weight);
                line.Children.Add(new TextBlock
                {
                    Text = "0 keeps the view's own",
                    Margin = PanelMetrics.Gap,
                    VerticalAlignment = VerticalAlignment.Center
                });
                inside.Children.Add(line);

                var front = new WrapPanel { Margin = PanelMetrics.Row };
                front.Children.Add(_frontTick);
                front.Children.Add(_frontType);
                front.Children.Add(_frontColour);
                inside.Children.Add(front);

                var back = new WrapPanel { Margin = PanelMetrics.Row };
                back.Children.Add(_backTick);
                back.Children.Add(_backType);
                back.Children.Add(_backColour);
                inside.Children.Add(back);

                return new Border
                {
                    Child = inside,
                    BorderThickness = PanelMetrics.Hairline,
                    BorderBrush = PanelTheme.Current().Line,
                    Margin = PanelMetrics.StepGap,
                    Tag = HairlineMark
                };
            }

            private void Wire(Action changed)
            {
                _prefix.TextChanged += (sender, e) => changed();
                _lineColour.HexChanged += (sender, e) => changed();
                _frontColour.HexChanged += (sender, e) => changed();
                _backColour.HexChanged += (sender, e) => changed();
                WireTick(_enabled, changed);
                WireTick(_visible, changed);
                WireTick(_halftone, changed);
                WireTick(_lineTick, changed);
                WireTick(_frontTick, changed);
                WireTick(_backTick, changed);
                WireGate(_lineTick, _lineColour);
                WireGate(_frontTick, _frontColour);
                WireGate(_backTick, _backColour);
                _weight.SelectionChanged += (sender, e) => changed();
                _frontType.SelectionChanged += (sender, e) => changed();
                _backType.SelectionChanged += (sender, e) => changed();
            }

            private static void WireTick(CheckBox box, Action changed)
            {
                box.Checked += (sender, e) => changed();
                box.Unchecked += (sender, e) => changed();
            }

            private static void WireGate(CheckBox tick, HexColorBox colour)
            {
                tick.Checked += (sender, e) => colour.IsEnabled = true;
                tick.Unchecked += (sender, e) => colour.IsEnabled = false;
            }

            private static CheckBox New(string caption, string tip)
            {
                return new CheckBox
                {
                    Content = PaneLabel.Escaped(caption),
                    Margin = PanelMetrics.Gap,
                    VerticalAlignment = VerticalAlignment.Center,
                    ToolTip = tip
                };
            }

            private static ComboBox Weights()
            {
                var box = new ComboBox
                {
                    Width = PanelMetrics.HexWidth,
                    Margin = PanelMetrics.Gap,
                    VerticalAlignment = VerticalAlignment.Center
                };
                for (int weight = LineWeights.Default; weight <= LineWeights.Heaviest; weight++)
                {
                    box.Items.Add(weight);
                }

                box.SelectedIndex = 0;
                return box;
            }

            private static ComboBox Patterns()
            {
                var box = new ComboBox
                {
                    Width = PanelMetrics.HexWidth,
                    Margin = PanelMetrics.Gap,
                    VerticalAlignment = VerticalAlignment.Center
                };
                box.Items.Add(PatternTypes.None);
                box.Items.Add(PatternTypes.Solid);
                box.SelectedIndex = 0;
                return box;
            }

            private static int PatternIndex(string patternType)
            {
                return PatternTypes.IsSolid(patternType) ? 1 : 0;
            }

            private static string TypeWord(ComboBox box)
            {
                return box.SelectedIndex == 1 ? PatternTypes.Solid : PatternTypes.None;
            }
        }
    }
}
