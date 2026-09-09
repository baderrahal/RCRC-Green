using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.UI;
using RcrcGreen.Core.Kpi;

// Autodesk.Revit.UI carries a TextBox of its own for the ribbon. This pane is WPF, so the
// name is pinned to the one that belongs in a UserControl.
using TextBox = System.Windows.Controls.TextBox;

namespace RcrcGreen.Revit.Kpi
{
    /// <summary>
    /// The KPI pane. The scan block at the top is the first round's and stays as it was: the
    /// model name with when it was last read, one button reading KPI Scan, and one status
    /// line. Below it sits the template block: the templates folder, every workbook in it
    /// with what it was recognised as, one pick at a time, exactly what the pick would fill,
    /// and the name the output would be written under. Nothing here fills anything, and
    /// there is no fill button, because a control that does nothing is a lie about what the
    /// tool can do.
    ///
    /// Nothing in this file reads a Document or touches the Revit API. Everything that needs
    /// one goes through <see cref="KpiRequestHandler"/> and its own external event. The
    /// template folder and the workbooks in it are plain file reads that touch no document,
    /// which is the same reason PanelTheme reads the theme without an event. No brush and no
    /// spacing is written here either. Colours come from <see cref="PanelTheme"/> and every
    /// margin from <see cref="PanelMetrics"/>.
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

        private readonly StackPanel _templates = new StackPanel();

        // Outlives every redraw of the template block, because it carries what somebody is
        // halfway through typing. It goes through Reparented on each rebuild, the same rule
        // the Drawing Sheet panel follows for its combo boxes.
        private readonly TextBox _outputName = new TextBox { MinWidth = PanelMetrics.ColumnWidth };

        private PanelTheme _theme = PanelTheme.Current();

        // The title the read line and the status line describe. Null until a scan. The name
        // and the read line used to be set by two callbacks, so model B's name sat over model
        // A's read line after a document switch.
        private string _scannedTitle;

        // Where the open model sits, from the handler, empty until Revit answers or for a
        // model that has never been saved. The output goes beside the model, so the line
        // under the name box reads it.
        private string _modelFolder = string.Empty;

        // The open document's title, empty when nothing is open. Held apart from the folder
        // because a detached model that has never been saved is open and has no folder, and
        // Create was handed the folder and reported it as no model open.
        private string _modelTitle = string.Empty;

        // The one picked workbook, and the template settled for it. For most files the two
        // arrive together. A file caught between the two park templates has a pick and no
        // template until the user chooses, and nothing is guessed meanwhile.
        private RecognisedWorkbook _picked;
        private KpiTemplate _pickedAs;

        // Why the component on the ticked plots preselected nothing, empty when it preselected
        // something or when there is nothing to say yet. A value the table does not hold used to
        // leave the pane silent, which reads as a tool that never looked.
        private string _whyNoTemplate = string.Empty;

        // What the model holds, read through the external event when the pane is shown. Plain
        // values only, so the pane still names no Revit type.
        private KpiPlotFacts _facts;

        // The one record of which plots are ticked. The list is drawn from it every time it
        // changes rather than letting a tick box remember its own state, which is the rule the
        // Drawing Sheet settled on after a count and its list drifted apart on a real model.
        private PlotTicks _ticks = new PlotTicks(PlotsInTheModel.Of(null, null));

        // Preselected off Preselected.From, never the first name the model happens to offer.
        // Reference came out as PRX_Plot_ID where the note names PRX_Plot_UID2, and Location
        // came out on whichever neighbourhood parameter sorted first.
        private string _componentParameter = string.Empty;
        private string _referenceParameter = string.Empty;
        private string _locationParameter = string.Empty;

        // Cleared by anything that could move a number, so a confirmation never carries over
        // onto a different set of plots.
        private bool _confirmedIdentical;

        private readonly Dictionary<string, string> _chosenRegions =
            new Dictionary<string, string>(StringComparer.Ordinal);

        // What the last press of Create really did, so the pane can offer the region choices
        // a refusal asked for without reading anything itself.
        private KpiCreateRun _lastRun;

        // These three outlive every redraw, because each carries what somebody is halfway
        // through typing. None of them comes from Revit.
        private readonly TextBox _date = new TextBox { MinWidth = PanelMetrics.ColumnWidth };
        private readonly TextBox _preparedBy = new TextBox { MinWidth = PanelMetrics.ColumnWidth };
        private readonly TextBox _position = new TextBox { MinWidth = PanelMetrics.ColumnWidth };

        public KpiPanel()
        {
            _handler = new KpiRequestHandler
            {
                Named = Took,
                Scanned = Scanned,
                FoundPlots = Found,
                Created = Made,
                Told = Say
            };
            _asking = ExternalEvent.Create(_handler);

            Content = Layout();
            PaintFromTheTheme();

            _modelName.Text = KpiPaneWords.NoModelName;
            _readAt.Text = KpiPaneWords.NotScanned;
            _date.Text = DateTime.Now.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            _preparedBy.Text = RememberedNames.PreparedBy();
            _position.Text = RememberedNames.Position();
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
            Ask(KpiRequest.Plots);
            RedrawTemplates();
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
        /// The strip at the top, the status line at the bottom, and the template block
        /// between them, scrolling on its own.
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
                Content = _templates,
                Padding = PanelMetrics.Edge,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            });

            return everything;
        }

        private UIElement Strip()
        {
            var inside = new DockPanel { Margin = PanelMetrics.StripInside, LastChildFill = true };

            var scan = new Button
            {
                Content = PaneLabel.Escaped("KPI Scan"),
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
        private void Took(string documentTitle, string modelFolder)
        {
            Dispatcher.Invoke(() =>
            {
                _modelName.Text = KpiPaneWords.ModelNamed(documentTitle);
                _modelTitle = documentTitle ?? string.Empty;
                _modelFolder = modelFolder ?? string.Empty;

                if (!string.Equals(documentTitle, _scannedTitle, StringComparison.Ordinal))
                {
                    _scannedTitle = null;
                    _readAt.Text = KpiPaneWords.NotScanned;
                    if (!string.IsNullOrEmpty(documentTitle)) _said.Text = KpiPaneWords.NotScanned;
                }

                // The output line names the folder the model sits in, so it is drawn again
                // once Revit has said which that is.
                RedrawTemplates();
            });
        }

        private void Scanned(KpiScan scan, DateTime readAt)
        {
            Dispatcher.Invoke(() =>
            {
                _scannedTitle = scan.Document.Title;
                _modelTitle = scan.Document.Title ?? string.Empty;
                _modelName.Text = KpiPaneWords.ModelNamed(scan.Document.Title);
                _readAt.Text = KpiPaneWords.ReadAt(readAt, scan.Document.ElementInstances, scan.Document.ReadSeconds);
            });
        }

        private void Say(string what)
        {
            Dispatcher.Invoke(() => _said.Text = what ?? string.Empty);
        }

        /// <summary>
        /// The whole template block, drawn again from the state on every change. One path in
        /// and out of every change, the same rule as the Drawing Sheet grid.
        /// </summary>
        private void RedrawTemplates()
        {
            _templates.Children.Clear();

            _templates.Children.Add(new TextBlock
            {
                Text = "GRP KPI Checklist templates",
                FontWeight = FontWeights.Bold,
                Margin = PanelMetrics.Row
            });

            string folder = TemplateFolder.Read();

            var folderLine = new DockPanel { Margin = PanelMetrics.Row, LastChildFill = true };
            var browse = new Button
            {
                Content = PaneLabel.Escaped("Browse"),
                Padding = PanelMetrics.CellPad,
                Margin = PanelMetrics.Gap,
                ToolTip = "Point at the folder holding the client's GRP KPI Checklist templates. "
                    + "It is remembered beside the installed add-in."
            };
            browse.Click += (sender, e) => BrowseForTheFolder();
            DockPanel.SetDock(browse, Dock.Right);
            folderLine.Children.Add(browse);
            folderLine.Children.Add(new TextBlock
            {
                Text = folder.Length == 0 ? "No folder set" : folder,
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center
            });
            _templates.Children.Add(folderLine);

            if (folder.Length == 0)
            {
                _templates.Children.Add(Faint(TemplateWords.NoFolder));
                return;
            }

            IReadOnlyList<string> paths = TemplateFolder.WorkbooksIn(folder);
            if (paths.Count == 0)
            {
                _templates.Children.Add(Faint(TemplateWords.EmptyFolder));
                return;
            }

            List<RecognisedWorkbook> recognised = paths
                .Select(path =>
                {
                    PeekedWorkbook peeked = PeekedWorkbook.Of(path);
                    return RecognisedWorkbook.Recognise(
                        Path.GetFileName(path), peeked.SheetNames, peeked.Refusal);
                })
                .ToList();

            _templates.Children.Add(Faint(TemplateWords.Listed(
                recognised.Count, recognised.Count(one => one.IsMatched))));

            // Said above the list, because it is the reason none of these rows is preselected.
            if (_whyNoTemplate.Length > 0) _templates.Children.Add(Faint(_whyNoTemplate));

            foreach (RecognisedWorkbook workbook in recognised)
            {
                RecognisedWorkbook which = workbook;
                bool pickable = which.IsMatched || which.NeedsAPick;
                var row = new Button
                {
                    Content = PaneLabel.Escaped(which.FileName + "   " + which.InWords),
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Padding = PanelMetrics.CellPad,
                    Margin = PanelMetrics.Row,
                    IsEnabled = pickable,
                    FontWeight = _picked != null && _picked.FileName == which.FileName
                        ? FontWeights.Bold
                        : FontWeights.Normal,
                    ToolTip = pickable ? "Pick this workbook." : which.Reason
                };
                row.Click += (sender, e) => Picked(which);
                _templates.Children.Add(row);
            }

            if (_picked == null) return;

            if (_pickedAs == null)
            {
                _templates.Children.Add(Faint(TemplateWords.PickBetween(_picked)));
                var either = new StackPanel { Orientation = Orientation.Horizontal, Margin = PanelMetrics.Row };
                foreach (KpiTemplate candidate in _picked.Candidates)
                {
                    KpiTemplate chosen = candidate;
                    var choice = new Button
                    {
                        Content = PaneLabel.Escaped(chosen.Name),
                        Padding = PanelMetrics.CellPad,
                        Margin = PanelMetrics.Gap
                    };
                    choice.Click += (sender, e) => { _pickedAs = chosen; RedrawTemplates(); };
                    either.Children.Add(choice);
                }
                _templates.Children.Add(either);
                return;
            }

            _templates.Children.Add(new TextBlock
            {
                Text = "What " + _pickedAs.Name + " would fill",
                FontWeight = FontWeights.Bold,
                Margin = PanelMetrics.Row
            });

            foreach (string line in TemplateWords.WouldFill(_pickedAs, Chosen()))
            {
                _templates.Children.Add(new TextBlock
                {
                    Text = line,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = PanelMetrics.Row
                });
            }

            var named = new DockPanel { Margin = PanelMetrics.Row, LastChildFill = true };
            var caption = new TextBlock
            {
                Text = "Written as",
                Width = PanelMetrics.WideLabelWidth,
                VerticalAlignment = VerticalAlignment.Center
            };
            DockPanel.SetDock(caption, Dock.Left);
            named.Children.Add(caption);
            named.Children.Add(Reparented(_outputName));
            _templates.Children.Add(named);

            _templates.Children.Add(Faint(_modelFolder.Length == 0
                ? TemplateWords.NoModelPath
                : TemplateWords.Output(_modelFolder)));
            _templates.Children.Add(Faint(CreateWords.Overwrite));

            ThePlots();
            TheChoices();
            TheCreateButton();
        }

        /// <summary>
        /// The plot picker. Every plot the model holds, ticked one at a time, in a run, or all
        /// at once, with the count showing at all times. There is no free text box: a plot the
        /// model does not hold cannot be chosen, the same rule the Drawing Sheet follows.
        /// </summary>
        private void ThePlots()
        {
            _templates.Children.Add(Head(CreateWords.Heading));

            if (_facts == null)
            {
                _templates.Children.Add(Faint(KpiPaneWords.Waiting));
                return;
            }

            foreach (string line in CreateWords.PlotSources(_facts.Plots))
            {
                _templates.Children.Add(Faint(line));
            }

            if (_facts.Plots.All.Count == 0) return;

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = PanelMetrics.Row };
            var all = new Button { Content = PaneLabel.Escaped(CreateWords.SelectAll), Padding = PanelMetrics.CellPad, Margin = PanelMetrics.Gap };
            all.Click += (sender, e) => Ticked(_ticks.All());
            var none = new Button { Content = PaneLabel.Escaped(CreateWords.Clear), Padding = PanelMetrics.CellPad, Margin = PanelMetrics.Gap };
            none.Click += (sender, e) => Ticked(_ticks.None());
            buttons.Children.Add(all);
            buttons.Children.Add(none);
            _templates.Children.Add(buttons);

            _templates.Children.Add(new TextBlock
            {
                Text = _ticks.InWords,
                FontWeight = FontWeights.Bold,
                Margin = PanelMetrics.Row
            });

            var list = new StackPanel();
            foreach (string plotId in _facts.Plots.All)
            {
                string which = plotId;
                var box = new CheckBox
                {
                    Content = PaneLabel.Escaped(which),
                    IsChecked = _ticks.IsTicked(which),
                    Margin = PanelMetrics.Row
                };
                box.Click += (sender, e) => Ticked(_ticks.Toggled(which));
                list.Children.Add(box);
            }

            _templates.Children.Add(new ScrollViewer
            {
                Content = list,
                MaxHeight = PanelMetrics.ListHeight,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Margin = PanelMetrics.Row
            });
        }

        /// <summary>
        /// The three choices the model cannot make and the three boxes it knows nothing about.
        /// Both dropdowns are built from the model, and the reference shows each parameter's
        /// value beside its name so the user picks by looking at the value.
        /// </summary>
        private void TheChoices()
        {
            if (_facts == null) return;

            _templates.Children.Add(Head("What the values come from"));

            _templates.Children.Add(Picker(
                "Component", _facts.ComponentNames, _componentParameter,
                chosen => { _componentParameter = chosen; Changed(); }));

            IReadOnlyList<string> references = _ticks.Count == 0
                ? KpiNames.PlotNamesOnSheets.ToList()
                : _facts.ReferenceChoices.Select(one => one.Name).ToList();

            _templates.Children.Add(Picker(
                "Reference", references, _referenceParameter,
                chosen => { _referenceParameter = chosen; Changed(); }));

            foreach (PlotParameterValue value in _facts.ReferenceChoices)
            {
                _templates.Children.Add(Faint("   " + value.InWords));
            }

            _templates.Children.Add(Picker(
                "Location", _facts.LocationNames, _locationParameter,
                chosen => { _locationParameter = chosen; Changed(); }));

            _templates.Children.Add(Boxed("Date", _date));
            _templates.Children.Add(Boxed("Prepared by", _preparedBy));
            _templates.Children.Add(Boxed("Position", _position));
        }

        /// <summary>
        /// Create, and everything standing between the pane and pressing it. A refusal lists
        /// every missing thing at once rather than one per press.
        /// </summary>
        private void TheCreateButton()
        {
            _templates.Children.Add(Head(CreateWords.Create));

            string cannot = CreateWords.CannotCreate(
                _modelTitle.Length > 0, _modelFolder.Length > 0, _pickedAs != null, _ticks.Count > 0);

            if (cannot.Length > 0) _templates.Children.Add(Faint(cannot));

            if (_pickedAs != null && _pickedAs.AreaIsTypedByHand)
            {
                _templates.Children.Add(Faint(CreateWords.AreaTypedByHand));
            }

            if (_lastRun != null && !_lastRun.Reconciliation.AddsUp)
            {
                foreach (string refusal in _lastRun.Reconciliation.Refusals)
                {
                    _templates.Children.Add(Warned(refusal));
                }

                TheRegionChoices();

                if (_lastRun.Reconciliation.IdenticalAreas.Count > 0 && !_confirmedIdentical)
                {
                    foreach (string line in CreateWords.ConfirmIdentical(_lastRun.Reconciliation.IdenticalAreas))
                    {
                        _templates.Children.Add(Warned(line));
                    }

                    var confirm = new Button
                    {
                        Content = PaneLabel.Escaped("These areas are right, write anyway"),
                        Padding = PanelMetrics.CellPad,
                        Margin = PanelMetrics.Row,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    confirm.Click += (sender, e) => { _confirmedIdentical = true; RedrawTemplates(); };
                    _templates.Children.Add(confirm);
                }
            }

            var create = new Button
            {
                Content = PaneLabel.Escaped(CreateWords.Create),
                Padding = PanelMetrics.CellPad,
                Margin = PanelMetrics.Row,
                HorizontalAlignment = HorizontalAlignment.Left,
                IsEnabled = cannot.Length == 0
            };
            create.Click += (sender, e) => AskedToCreate();
            _templates.Children.Add(create);
        }

        /// <summary>
        /// One row of buttons per plot whose regions left the answer open. Which of a plot's
        /// two regions carries the area varies by plot, so the tool asks rather than picking.
        /// </summary>
        private void TheRegionChoices()
        {
            foreach (PlotReading reading in _lastRun.Readings)
            {
                if (reading.ChosenRegion != null || reading.RegionsHoldingAnArea.Count < 2) continue;

                _templates.Children.Add(Faint("   " + reading.PlotId + ", pick its intervention area:"));

                var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = PanelMetrics.Row };
                foreach (RegionArea region in reading.RegionsHoldingAnArea)
                {
                    string plotId = reading.PlotId;
                    string typeName = region.TypeName;
                    var choice = new Button
                    {
                        Content = PaneLabel.Escaped(typeName + "   " + region.Printed),
                        Padding = PanelMetrics.CellPad,
                        Margin = PanelMetrics.Gap
                    };
                    choice.Click += (sender, e) =>
                    {
                        _chosenRegions[plotId] = typeName;
                        AskedToCreate();
                    };
                    row.Children.Add(choice);
                }

                _templates.Children.Add(row);
            }
        }

        private void Ticked(PlotTicks next)
        {
            _ticks = next;
            Changed();
        }

        /// <summary>
        /// Anything that could move a number throws away the last run and the confirmation
        /// with it, so a confirmed pair of areas never carries over onto a different set of
        /// plots and a refusal never sits under a choice that has since changed.
        /// </summary>
        private void Changed()
        {
            _lastRun = null;
            _confirmedIdentical = false;
            _chosenRegions.Clear();
            Preselect();
            RedrawTemplates();
        }

        /// <summary>
        /// The component on the ticked plots preselects a template, off the measured table in
        /// <see cref="ComponentTemplates"/>. A value the table does not hold, and plots that
        /// disagree, both leave the pick to the user WITH THE REASON ON SCREEN, and a template
        /// the user has already picked by hand is never moved.
        /// </summary>
        private void Preselect()
        {
            _whyNoTemplate = string.Empty;
            if (_facts == null || _ticks.Count == 0 || _picked != null) return;

            AgreedValue component = new AgreedValue(_ticks.Ticked
                .Select(plotId => new PlotText(plotId, _facts.ComponentOn(plotId))));

            TemplateChoice choice = TemplateForComponent.For(component, null);
            if (choice.NeedsAPick)
            {
                // Nothing is picked by hand here, because this method returns above when
                // something is, so whatever _pickedAs holds came from an earlier preselection on
                // plots that are no longer the ticked ones. Leaving it would arm Create with a
                // template beside a line saying none was preselected.
                _pickedAs = null;
                _whyNoTemplate = choice.Why;
                return;
            }

            _pickedAs = choice.Preselected;
            if (_outputName.Text.Length == 0)
            {
                _outputName.Text = CreateWords.SuggestedName(_pickedAs, component.Value, _ticks.Ticked);
            }
        }

        /// <summary>
        /// The three parameters the pickers hold, so a line saying where a value comes from
        /// names the one that will really be read.
        /// </summary>
        private ChosenParameters Chosen()
        {
            return new ChosenParameters(_componentParameter, _referenceParameter, _locationParameter);
        }

        private void AskedToCreate()
        {
            if (_pickedAs == null) return;

            RememberedNames.Remember(_preparedBy.Text, _position.Text);

            _handler.Asked = new KpiCreateAsk(
                _ticks.Ticked,
                _pickedAs,
                _picked == null ? string.Empty : Path.Combine(TemplateFolder.Read(), _picked.FileName),
                _outputName.Text,
                _componentParameter,
                _referenceParameter,
                _locationParameter,
                _confirmedIdentical,
                _chosenRegions);

            Say("Creating.");
            Ask(KpiRequest.Create);
        }

        private void Found(KpiPlotFacts facts)
        {
            Dispatcher.Invoke(() =>
            {
                _facts = facts;
                _ticks = new PlotTicks(facts.Plots, _ticks.Ticked);
                _modelFolder = facts.ModelFolder;

                if (_componentParameter.Length == 0)
                {
                    _componentParameter = Preselected.From(facts.ComponentNames, KpiNames.Component);
                }

                if (_referenceParameter.Length == 0)
                {
                    _referenceParameter = Preselected.From(
                        facts.ReferenceChoices.Select(one => one.Name).ToList(), KpiNames.PlotUid2);
                }

                if (_locationParameter.Length == 0)
                {
                    _locationParameter = Preselected.From(facts.LocationNames, KpiNames.NeighbourhoodName);
                }

                RedrawTemplates();
            });
        }

        private void Made(KpiCreateRun run, string reportWhere)
        {
            Dispatcher.Invoke(() =>
            {
                _lastRun = run;
                RedrawTemplates();
            });
        }

        private TextBlock Head(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                Margin = PanelMetrics.Heading
            };
        }

        private TextBlock Warned(string text)
        {
            return new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                Foreground = _theme.Warning,
                Margin = PanelMetrics.Row
            };
        }

        private UIElement Boxed(string caption, TextBox box)
        {
            var row = new DockPanel { Margin = PanelMetrics.Row, LastChildFill = true };
            var label = new TextBlock
            {
                Text = caption,
                Width = PanelMetrics.WideLabelWidth,
                VerticalAlignment = VerticalAlignment.Center
            };
            DockPanel.SetDock(label, Dock.Left);
            row.Children.Add(label);
            row.Children.Add(Reparented(box));
            return row;
        }

        /// <summary>
        /// A row of buttons rather than a ComboBox, because a combo box carries its own item
        /// list across a redraw and this block is thrown away and built again on every change.
        /// </summary>
        private UIElement Picker(
            string caption, IReadOnlyList<string> names, string chosen, Action<string> pick)
        {
            var row = new StackPanel { Margin = PanelMetrics.Row };
            row.Children.Add(new TextBlock { Text = caption, VerticalAlignment = VerticalAlignment.Center });

            if (names.Count == 0)
            {
                row.Children.Add(Faint("   nothing in this model holds that word"));
                return row;
            }

            var buttons = new StackPanel { Orientation = Orientation.Horizontal };
            foreach (string name in names)
            {
                string which = name;
                var choice = new Button
                {
                    Content = PaneLabel.Escaped(which),
                    Padding = PanelMetrics.CellPad,
                    Margin = PanelMetrics.Gap,
                    FontWeight = string.Equals(which, chosen, StringComparison.Ordinal)
                        ? FontWeights.Bold
                        : FontWeights.Normal
                };
                choice.Click += (sender, e) => pick(which);
                buttons.Children.Add(choice);
            }

            row.Children.Add(buttons);
            return row;
        }

        private void Picked(RecognisedWorkbook workbook)
        {
            _picked = workbook;
            _pickedAs = workbook.Template;

            // The line saying nothing was preselected describes a pane with nothing picked, so
            // a hand pick is what makes it untrue. A line left standing beside the state that
            // contradicts it is the shape this repo has met seven times.
            _whyNoTemplate = string.Empty;
            _outputName.Text = OutputName.Suggested(workbook.FileName);
            RedrawTemplates();
        }

        /// <summary>
        /// A folder picker is a Windows dialog rather than a Revit one, so it needs no
        /// external event. The choice is written beside the installed assembly at once,
        /// because the pane can be closed a moment later.
        /// </summary>
        private void BrowseForTheFolder()
        {
            using (var picking = new System.Windows.Forms.FolderBrowserDialog())
            {
                picking.Description = "The folder holding the GRP KPI Checklist templates";
                string already = TemplateFolder.Read();
                if (already.Length > 0) picking.SelectedPath = already;

                if (picking.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                if (!TemplateFolder.Remember(picking.SelectedPath))
                {
                    Say("The folder could not be remembered. " + TemplateFolder.PointerFileName
                        + " beside the installed add-in refused the write.");
                }

                _picked = null;
                _pickedAs = null;
                _whyNoTemplate = string.Empty;
                RedrawTemplates();
            }
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
        /// Takes a control out of whatever held it before it goes somewhere new. The template
        /// block is thrown away and rebuilt on every change and the name box outlives it, and
        /// WPF refuses an element with two logical parents outright. Same rule as the Drawing
        /// Sheet panel's combo boxes.
        /// </summary>
        private static UIElement Reparented(UIElement what)
        {
            var was = LogicalTreeHelper.GetParent(what) as DependencyObject;

            var panel = was as Panel;
            if (panel != null) panel.Children.Remove(what);

            var holder = was as ContentControl;
            if (holder != null) holder.Content = null;

            return what;
        }
    }
}
