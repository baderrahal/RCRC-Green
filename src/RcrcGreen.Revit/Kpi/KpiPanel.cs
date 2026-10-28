using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using Autodesk.Revit.UI;
using RcrcGreen.Core.Kpi;

// Autodesk.Revit.UI carries a TextBox and a ComboBox of its own for the ribbon. This pane is
// WPF, so the names are pinned to the ones that belong in a UserControl.
using TextBox = System.Windows.Controls.TextBox;
using ComboBox = System.Windows.Controls.ComboBox;
using Path = System.IO.Path;

// The caption escape lives in Shared now, where both panels read it. An alias rather than
// a using of the whole Core namespace, so no other name in it can shadow a KPI one.
using PaneLabel = RcrcGreen.Core.PaneLabel;

namespace RcrcGreen.Revit.Kpi
{
    /// <summary>
    /// The KPI pane, Bader's approved design of October: a header, a row of four steps, one
    /// step's body scrolling under it, and a footer holding the step's one button and one short
    /// status line.
    ///
    /// **IT WAS A LOG AND IT IS GOING TO THE WHOLE TEAM.** Every sentence the pane used to show
    /// in full is now a tooltip on the control it explains or a line in the report's BEFORE THE
    /// PRESS, and the word list in `design/kpi-pane-words.md` says where each one went. Nothing
    /// the pane did is gone. Every word it shows is Core's and every colour and size KpiTheme's.
    ///
    /// Nothing in this file reads a Document or touches the Revit API. Everything that needs
    /// one goes through <see cref="KpiRequestHandler"/> and its own external event. The
    /// templates folder and the workbooks in it are plain file reads that touch no document.
    /// Which step can be worked on is <see cref="KpiSteps"/>'s answer, and this draws it.
    /// </summary>
    internal sealed class KpiPanel : UserControl, IDockablePaneProvider
    {
        private readonly ExternalEvent _asking;
        private readonly KpiRequestHandler _handler;

        private readonly TextBlock _modelName = new TextBlock();
        private readonly TextBlock _status = new TextBlock();
        private readonly UniformGrid _stepRow = new UniformGrid { Columns = KpiTheme.StepColumns };
        private readonly StackPanel _body = new StackPanel();
        private readonly StackPanel _footerButtons = new StackPanel();

        // **ONE BOX CANNOT NAME SIX FILES.** Every ticked workbook row carries the template it
        // settled as, held on its tick so it outlives every redraw.
        private sealed class WorkbookTick
        {
            public WorkbookTick(RecognisedWorkbook workbook, KpiTemplate settledAs)
            {
                Workbook = workbook;
                SettledAs = settledAs;
            }

            public RecognisedWorkbook Workbook { get; }

            /// <summary>
            /// The template this file is. Null while a file caught between the two park
            /// templates waits for the user to say which, and nothing is guessed meanwhile.
            /// </summary>
            public KpiTemplate SettledAs { get; set; }
        }

        // The open model as Revit last answered for it, asked for again on every draw. THE PANE
        // HOLDS NO COPY OF ANYTHING IT CAN ASK FOR: a folder string read once left Create
        // refusing on a model that had since been saved.
        private OpenModel _model = OpenModel.Nothing;

        // **WHAT HAS BEEN READ, AND IT IS AN ABSENCE UNTIL SOMETHING READS.** Counting 96,959
        // elements IS the read, and a dockable pane is restored visible at Revit startup, so a
        // count taken on show held NG05 for minutes with nobody having asked.
        private ReadOfTheModel _read = ReadOfTheModel.NotYet;

        // Whether the plots press is out on the handler, so its button does not take a second.
        private bool _reading;

        // Which plots the user took off by hand. Ticking a template is a starting point rather
        // than a lock, so a plot held off here stays off when its template is ticked.
        private HandTicks _byHand = HandTicks.None;

        // The window a press of Create shows while it runs. Opened on this thread before the
        // external event is raised and closed when the run's last answer comes back.
        private KpiProgressWindow _running;

        private readonly List<WorkbookTick> _picks = new List<WorkbookTick>();

        // Why the asset types stand as they do: what preselected one, or what stopped it. It is
        // the Asset types title's tooltip and goes into BEFORE THE PRESS.
        private string _whyThisTemplate = string.Empty;

        private KpiPlotFacts _facts;

        // The one record of which plots are ticked. The list is drawn from it every time it
        // changes rather than letting a tick box remember its own state.
        private PlotTicks _ticks = new PlotTicks(PlotsInTheModel.Of(null, null));

        private string _componentParameter = string.Empty;
        private string _referenceParameter = string.Empty;
        private string _locationParameter = string.Empty;

        // Cleared by anything that could move a number, so a confirmation never carries over
        // onto a different set of plots.
        private bool _confirmedIdentical;

        private readonly Dictionary<string, string> _chosenRegions =
            new Dictionary<string, string>(StringComparer.Ordinal);

        // What the last press really did, across every template, so the result can offer the
        // region choices a refusal asked for and the next press can apply a choice to the
        // readings it holds rather than reading the model again.
        private KpiCreateRunSet _lastSet;

        // The templates folder's workbooks as recognised, held once per folder. The folder is
        // listed on every draw and only each file's recognition is kept.
        private TemplateListing _templatesListed = TemplateListing.Nothing;

        private readonly ScrollMemory _scrolled = new ScrollMemory();

        // **WHICH OF THE FOUR STEPS IS BEING WORKED ON.** Null until somebody presses a cell
        // or a Next, and then it is theirs. Forgotten when the model changes.
        private KpiStep? _stepOpen;

        private ListTickFreshness _listTick = ListTickFreshness.NeverPressed;

        // Whether step 4 is showing what the last press did rather than what a press would do.
        private bool _showingResults;

        // Where the last press wrote its report, for Open report. Cleared with the result.
        private string _reportWhere = string.Empty;

        // **THE LAST REPORT A PRESS WROTE FOR THIS MODEL**, for Show in report. It outlives a
        // tick change, because the warning box it serves is about the model, and goes with the
        // model, because another model's report says nothing about this one.
        private string _lastReport = string.Empty;

        // What the last press wrote under BEFORE THE PRESS, so Show in report is offered only on
        // a box that file really holds, and the one handed to the press now in flight.
        private BeforeThePress _lastBefore = BeforeThePress.Nothing;
        private BeforeThePress _pendingBefore = BeforeThePress.Nothing;

        // A press is out on the handler and no result has come back for it yet. A line told
        // while this holds is a refusal before any plot, and it takes the old result away.
        private bool _pressing;

        // The four steps as this draw worked them out, read by every step body so none of
        // them decides a reason of its own.
        private KpiSteps _steps;

        // A line the handler or a click said, shown in place of the step's own status until the
        // person moves on, with its full text as the line's tooltip.
        private StatusLine _told;

        // What the open step says on the status line when nothing has been told.
        private StatusLine _stepSays = new StatusLine(string.Empty);

        // These outlive every redraw, because each carries what somebody is halfway through
        // typing. Each goes through Reparented on the rebuild.
        private readonly TextBox _date = KpiTheme.Input();
        private readonly TextBox _preparedBy = KpiTheme.Input();
        private readonly TextBox _position = KpiTheme.Input();
        private readonly TextBox _search = KpiTheme.Input();

        // Templates made for files the tool does not know by name, by name, so a redraw hands
        // back the same object and a tick held against one still holds. Use and Skip on those
        // files, and Skip on an unknown component value, last until Revit closes.
        private readonly Dictionary<string, KpiTemplate> _madeTemplates = new Dictionary<string, KpiTemplate>(StringComparer.Ordinal);
        private readonly HashSet<string> _usedTemplates = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _skippedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _skippedValues = new HashSet<string>(StringComparer.Ordinal);
        private string _choosingValue = string.Empty;

        // The plot rows of the open step 3, refilled by the search box without a redraw,
        // because rebuilding the tree under the cursor takes the keyboard out of the box.
        private StackPanel _plotRows;

        public KpiPanel()
        {
            _handler = new KpiRequestHandler
            {
                Named = Took,
                Scanned = Scanned,
                FoundPlots = Found,
                CreatedAcross = Made,
                Told = Say,
                Progressed = Moved,
                Advanced = Advanced
            };
            _asking = ExternalEvent.Create(_handler);

            Content = Layout();
            Paint();

            _date.Text = DateTime.Now.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            _preparedBy.Text = RememberedNames.PreparedBy();
            _position.Text = RememberedNames.Position();
            _search.ToolTip = Tip(KpiPaneWords.SearchTooltip);
            _search.TextChanged += (sender, e) => FillPlotRows();

            TheHeader();

            // Not the constructor. A dockable pane is built during OnStartup, when no document
            // exists, so the first ask has to wait for the pane to be put on screen.
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

        private void Shown()
        {
            if (!IsVisible) return;

            Paint();
            RedrawTemplates();
        }

        /// <summary>
        /// **THE PANE PAINTS ITS OWN BACKGROUND AND EVERY FOREGROUND.** A pane that inherited
        /// Revit's dark theme came up black on black once, so nothing here is left to inherit
        /// from Revit. The design's colours are light and are used under both themes.
        /// </summary>
        private void Paint()
        {
            Background = KpiTheme.Background;
            Foreground = KpiTheme.Text;
            FontFamily = KpiTheme.Body;
            FontSize = KpiTheme.BodySize;
        }

        /// <summary>
        /// The header and the step row at the top, the footer docked at the foot, and the open
        /// step's body scrolling between them.
        /// </summary>
        private UIElement Layout()
        {
            var everything = new DockPanel { LastChildFill = true, Background = KpiTheme.Background };

            UIElement header = Header();
            DockPanel.SetDock(header, Dock.Top);
            everything.Children.Add(header);

            var steps = new Border { Padding = KpiTheme.StepRow, Child = _stepRow };
            DockPanel.SetDock(steps, Dock.Top);
            everything.Children.Add(steps);

            UIElement footer = Footer();
            DockPanel.SetDock(footer, Dock.Bottom);
            everything.Children.Add(footer);

            _body.Margin = KpiTheme.Pane;
            everything.Children.Add(new ScrollViewer
            {
                Content = _body,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                BorderThickness = KpiTheme.Nothing
            });

            return everything;
        }

        private UIElement Header()
        {
            var lines = new StackPanel { Margin = KpiTheme.Header };

            lines.Children.Add(new TextBlock
            {
                Text = KpiPaneWords.Title,
                FontSize = KpiTheme.TitleSize,
                FontWeight = FontWeights.SemiBold,
                Foreground = KpiTheme.Text
            });

            _modelName.TextTrimming = TextTrimming.CharacterEllipsis;
            _modelName.Foreground = KpiTheme.Text;
            lines.Children.Add(_modelName);

            lines.Children.Add(new TextBlock
            {
                Text = KpiPaneWords.Build(KpiBuildStamp.Read()),
                FontSize = KpiTheme.StatusSize,
                Foreground = KpiTheme.Muted
            });

            return new Border
            {
                Child = lines,
                BorderBrush = KpiTheme.Divider,
                BorderThickness = KpiTheme.LineBelow
            };
        }

        private UIElement Footer()
        {
            var inside = new StackPanel { Margin = KpiTheme.Footer };
            inside.Children.Add(_footerButtons);

            _status.FontSize = KpiTheme.StatusSize;
            _status.Foreground = KpiTheme.Muted;
            _status.TextTrimming = TextTrimming.CharacterEllipsis;
            _status.Margin = KpiTheme.StatusGap;
            inside.Children.Add(_status);

            return new Border
            {
                Child = inside,
                Background = KpiTheme.Background,
                BorderBrush = KpiTheme.Divider,
                BorderThickness = KpiTheme.LineAbove
            };
        }

        private void Ask(KpiRequest wanted)
        {
            _handler.Ask(wanted);
            _asking.Raise();
        }

        /// <summary>
        /// The handler calls these from the Revit thread, so the hop to the pane's own thread
        /// happens here rather than being forgotten at each call site.
        /// </summary>
        private void Took(string documentTitle)
        {
            Dispatcher.Invoke(() =>
            {
                OpenModel answered = OpenModel.Of(documentTitle);
                bool moved = !answered.Is(_model);

                _model = answered;

                // **A DIFFERENT MODEL HAS NOT BEEN READ**, whatever was read of the last one, so
                // everything that came off a read goes with it.
                if (moved)
                {
                    _read = ReadOfTheModel.NotYet;
                    _facts = null;
                    _reading = false;
                    _byHand = _byHand.Forgotten();
                    _stepOpen = null;
                    _listTick = ListTickFreshness.NeverPressed;
                    _showingResults = false;
                    _reportWhere = string.Empty;
                    _lastReport = string.Empty;
                    _lastBefore = BeforeThePress.Nothing;
                    _pressing = false;
                    _told = null;
                    _lastSet = null;
                    _confirmedIdentical = false;
                    _chosenRegions.Clear();
                }

                TheHeader();

                // Only when the answer moved. Drawing asks for this again, so redrawing on every
                // answer would spin the external event for as long as the pane is open.
                if (moved) RedrawTemplates();
            });
        }

        private void TheHeader()
        {
            _modelName.Text = KpiPaneWords.ModelNamed(_model.Title);
            _modelName.ToolTip = Tip(_model.Title);
        }

        private void Scanned(KpiScan scan, DateTime readAt)
        {
            Dispatcher.Invoke(() =>
            {
                _read = ReadOfTheModel.TheWholeModel(
                    readAt, scan.Document.ElementInstances, scan.Document.ReadSeconds);
            });
        }

        /// <summary>
        /// The run's end line, whatever ended it, so the window goes with it and can never be
        /// left over a run that is no longer running.
        /// </summary>
        private void Say(StatusLine what)
        {
            Dispatcher.Invoke(() =>
            {
                _told = what;
                bool wasReading = _reading;
                _reading = false;
                Shut();

                // **A PRESS REFUSED BEFORE ANY PLOT LEAVES NO RESULT OF ITS OWN**, and the last
                // press's Created and Failed standing under its refusal read as this press's.
                // Made clears this flag before the end line arrives, so it holds only for a
                // press that ended without one.
                bool wasPressing = _pressing;
                if (wasPressing)
                {
                    _pressing = false;
                    _showingResults = false;
                    _reportWhere = string.Empty;
                }

                if (PressEnd.RedrawsTheSteps(wasPressing, wasReading))
                {
                    RedrawTemplates();
                    return;
                }

                PaintStatus();
            });
        }

        private void Shut()
        {
            KpiProgressWindow running = _running;
            _running = null;
            if (running != null) running.Done();
        }

        /// <summary>
        /// A progress sentence from inside a run. **The window shows the count and the plot
        /// only**, off <see cref="Advanced"/>, so the sentence is not drawn. The empty wait at
        /// Render priority is kept: it is what lets the window repaint mid run without letting
        /// queued clicks through into the handler's Execute.
        /// </summary>
        private void Moved(string what)
        {
            Dispatcher.Invoke(new Action(() => { }), System.Windows.Threading.DispatcherPriority.Render);
        }

        private void Advanced(PlotProgress where)
        {
            Dispatcher.Invoke(() =>
            {
                if (_running != null) _running.Advanced(where);
                _status.Text = where.InWords;
            });
            Dispatcher.Invoke(new Action(() => { }), System.Windows.Threading.DispatcherPriority.Render);
        }

        private void PaintStatus()
        {
            StatusLine shown = _told ?? _stepSays;
            _status.Text = shown.Text;
            _status.ToolTip = Tip(shown.Detail.Length > 0 ? shown.Detail : shown.Text);
        }

        /// <summary>
        /// The pane, drawn again from the state on every change. One path in and out of every
        /// change, the same rule as the Drawing Sheet grid.
        /// </summary>
        private void RedrawTemplates()
        {
            // Every count below places plots through the component table, so it is read first,
            // fresh, and a line added to the shared file shows on the next redraw.
            ComponentFiles.PutInForce(_madeTemplates.Values);

            KpiSteps steps = Steps();
            _steps = steps;
            KpiStep open = _stepOpen ?? steps.FirstUnfinished;

            TheStepRow(steps, open);

            _body.Children.Clear();
            _footerButtons.Children.Clear();
            _plotRows = null;

            KpiStepState state = steps.For(open);

            switch (open)
            {
                case KpiStep.Setup: InsideSetup(state); break;
                case KpiStep.Read: InsideRead(state); break;
                case KpiStep.Plots: InsideStepThree(state); break;
                default: InsideCreate(state); break;
            }

            _body.Children.Add(new Border { Margin = KpiTheme.BottomSpace });
            PaintStatus();

            // Read at the moment the pane is drawn, never held. The answer comes back through
            // Took, which redraws only when it moved, so this does not chase its own tail.
            Ask(KpiRequest.WhichModel);
        }

        /// <summary>
        /// The four steps, worked out in Core off what the pane can ask for at this moment.
        /// **Every value here is read live**, off the pointer files and the last answer.
        /// </summary>
        private KpiSteps Steps()
        {
            return KpiSteps.Of(
                TemplateFolder.Read().Length > 0,
                OutputFolder.Read().Length > 0,
                FormsFolder.Read().Length > 0,
                StreetReferenceFileSetting.Read().Length > 0,
                PlotListFileSetting.Read().Length > 0,
                _model.IsOpen,
                _facts == null ? (int?)null : _facts.Plots.All.Count,
                Settled().Count,
                _ticks.Count,
                _showingResults);
        }

        private IReadOnlyList<string> NotSet()
        {
            return KpiSteps.NotSet(
                TemplateFolder.Read().Length > 0,
                OutputFolder.Read().Length > 0,
                FormsFolder.Read().Length > 0,
                StreetReferenceFileSetting.Read().Length > 0,
                PlotListFileSetting.Read().Length > 0);
        }

        /// <summary>
        /// Four equal cells, a circle over the step's name. The step being worked on is green,
        /// a finished one carries a drawn tick, one not reached is an outline.
        /// </summary>
        private void TheStepRow(KpiSteps steps, KpiStep open)
        {
            _stepRow.Children.Clear();

            foreach (KpiStepState step in steps.All)
            {
                KpiStep which = step.Step;
                bool here = which == open;

                var circle = new Grid { Width = KpiTheme.Circle, Height = KpiTheme.Circle, HorizontalAlignment = HorizontalAlignment.Center };
                var ring = new Ellipse { StrokeThickness = KpiTheme.RingThickness };

                UIElement mark;
                if (here)
                {
                    ring.Fill = KpiTheme.Green;
                    ring.Stroke = KpiTheme.Green;
                    mark = step.Done && _showingResults
                        ? KpiTheme.Tick(KpiTheme.OnGreen)
                        : Number(step.Number, KpiTheme.OnGreen);
                }
                else if (step.Done)
                {
                    ring.Fill = KpiTheme.GreenTint;
                    ring.Stroke = KpiTheme.GreenTint;
                    mark = KpiTheme.Tick(KpiTheme.Green);
                }
                else
                {
                    ring.Fill = KpiTheme.Background;
                    ring.Stroke = KpiTheme.InputBorder;
                    mark = Number(step.Number, KpiTheme.Muted);
                }

                circle.Children.Add(ring);
                circle.Children.Add(mark);

                var inside = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
                inside.Children.Add(circle);
                inside.Children.Add(new TextBlock
                {
                    Text = step.Title,
                    FontSize = KpiTheme.StepNameSize,
                    FontWeight = here ? FontWeights.SemiBold : FontWeights.Normal,
                    Foreground = here ? KpiTheme.Text : KpiTheme.Muted,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Margin = KpiTheme.StatusGap
                });

                Button cell = KpiTheme.Link(inside);
                cell.HorizontalAlignment = HorizontalAlignment.Stretch;
                cell.ToolTip = Tip(step.Tooltip);
                cell.Click += (sender, e) => GoTo(which);
                _stepRow.Children.Add(cell);
            }
        }

        private static UIElement Number(int number, Brush colour)
        {
            return new TextBlock
            {
                Text = number.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Foreground = colour,
                FontSize = KpiTheme.LabelSize,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        /// <summary>
        /// Opens a step because somebody asked to. A line told about the last thing they did
        /// goes with it, because it is about the step they left.
        /// </summary>
        private void GoTo(KpiStep step)
        {
            _stepOpen = step;
            _told = null;
            RedrawTemplates();
        }

        /// <summary>
        /// **A STEP THAT CANNOT BE WORKED ON SHOWS ITS REASON IN PLACE OF ITS BODY.** A cell
        /// that does nothing and says nothing is worse than one that is not there.
        /// </summary>
        private bool ShowsWhyNot(KpiStepState state, string mainButton)
        {
            if (state.Usable) return false;

            _body.Children.Add(new TextBlock
            {
                Text = state.WhyNot,
                Foreground = KpiTheme.Muted,
                TextWrapping = TextWrapping.Wrap,
                Margin = KpiTheme.Group
            });

            Button main = KpiTheme.Main(Caption(mainButton));
            main.IsEnabled = false;
            _footerButtons.Children.Add(main);
            _stepSays = new StatusLine(state.WhyNot);
            return true;
        }

        // ---------------------------------------------------------------- step 1, Setup

        /// <summary>
        /// **STEP 1.** The three folders, the two files and the three boxes the workbook
        /// carries. Every one is reachable at all times.
        /// </summary>
        private void InsideSetup(KpiStepState state)
        {
            string templates = TemplateFolder.Read();
            string output = OutputFolder.Read();
            string forms = FormsFolder.Read();

            _body.Children.Add(Section(KpiPaneWords.Folders, null, null));

            int found = Listing().Count(one => one.IsMatched || one.NeedsAPick);
            _body.Children.Add(PathRow(
                KpiPaneWords.Templates, false, templates, TemplateFolder.ReadRaw(),
                KpiPaneWords.TemplatesFound(found), true,
                templates.Length == 0 ? TemplateWords.NoFolder : templates,
                "Point at the folder holding the GRP KPI Checklist template workbooks. "
                    + "It is remembered beside the installed add-in.",
                BrowseForTheFolder));
            foreach (UIElement box in TemplatePrompts()) _body.Children.Add(box);

            _body.Children.Add(PathRow(
                KpiPaneWords.Output, false, output, OutputFolder.ReadRaw(), string.Empty, true,
                output.Length == 0
                    ? TemplateWords.NoOutputFolder
                    : output + "\n\n" + CreateWords.WhereTheWorkbooksGo(output) + "\n\n" + TemplateWords.Output(output),
                "Point at the folder the filled workbooks should be written to. "
                    + "It is remembered beside the installed add-in.",
                BrowseForTheOutputFolder));

            PaneWarning sameFolder = PaneWarnings.ForOutputFolder(output, templates);
            if (sameFolder != null) _body.Children.Add(WarningBox(sameFolder, null));

            _body.Children.Add(PathRow(
                KpiPaneWords.PdfForms, false, forms, FormsFolder.ReadRaw(), string.Empty, true,
                (forms.Length == 0 ? KpiSteps.FormsFolderNotSet + ". " : forms + "\n\n")
                    + TemplateWords.FormsFolder(forms),
                "Point at the folder holding the three Projects Basic Data forms. Each "
                    + "plot gets one PDF beside its workbook, on the form its plot prefix names. "
                    + "It is remembered beside the installed add-in.",
                BrowseForTheFormsFolder));

            _body.Children.Add(Section(KpiPaneWords.Files, null, null));

            string street = StreetReferenceFileSetting.Read();
            StreetReferenceFile streets = StreetReferenceFile.In(street);
            _body.Children.Add(PathRow(
                KpiPaneWords.StreetReference, false, street, StreetReferenceFileSetting.ReadRaw(),
                streets.Read ? KpiPaneWords.RowsHeld(streets.Rows.Count) : KpiPaneWords.Unreadable,
                streets.Read || street.Length == 0,
                (street.Length == 0 ? string.Empty : street + "\n\n") + KpiPaneWords.StreetReferenceSays(street, streets),
                "Point at the team's Scope_Validation workbook. It fills the road width and the "
                    + "total length on STREETS plots, matched on " + KpiNames.PlotUid2
                    + ". It is remembered beside the installed add-in.",
                BrowseForTheStreetReferenceFile));

            string listFile = PlotListFileSetting.Read();
            PlotListRead list = PlotListFile.In(listFile);
            _body.Children.Add(PathRow(
                KpiPaneWords.PlotList, true, listFile, PlotListFileSetting.ReadRaw(),
                list.Read ? KpiPaneWords.PlotsListed(list.Plots.Count) : KpiPaneWords.Unreadable,
                list.Read || listFile.Length == 0,
                (listFile.Length == 0 ? string.Empty : listFile + "\n\n") + KpiPaneWords.PlotListSays(listFile, list),
                "Point at the team's plot list, a plain text file with one plot per line. Plot "
                    + "list, in step 3, replaces every tick with exactly the plots it names. It is "
                    + "remembered beside the installed add-in.",
                BrowseForThePlotListFile));

            _body.Children.Add(Section(KpiPaneWords.WorkbookDetails, null, CreateWords.OneSetOfFields));
            _body.Children.Add(Labelled(KpiPaneWords.Date, Reparented(_date)));

            var two = new UniformGrid { Columns = KpiTheme.DetailsColumns };
            UIElement preparedBy = Labelled(KpiPaneWords.PreparedBy, Reparented(_preparedBy));
            ((FrameworkElement)preparedBy).Margin = KpiTheme.RightGap;
            two.Children.Add(preparedBy);
            UIElement position = Labelled(KpiPaneWords.Position, Reparented(_position));
            ((FrameworkElement)position).Margin = KpiTheme.LeftGap;
            two.Children.Add(position);
            _body.Children.Add(two);

            Button next = KpiTheme.Main(Caption(KpiPaneWords.NextRead));
            next.IsEnabled = state.Done;
            next.Click += (sender, e) => GoTo(KpiStep.Read);
            _footerButtons.Children.Add(next);

            IReadOnlyList<string> notSet = NotSet();
            _stepSays = new StatusLine(KpiSteps.SetupStatus(notSet), KpiSteps.StillNotSet(notSet));
        }

        /// <summary>
        /// One folder or file: its label, a read only box showing the path cut at the end with
        /// the full path as its tooltip and a drawn tick when it is set, Browse, and one hint.
        /// **A box nobody pointed anywhere, or whose path has gone, has a red border and one
        /// word**, except the plot list, which is optional and whose absence is ordinary.
        /// </summary>
        private UIElement PathRow(
            string label,
            bool optional,
            string path,
            string remembered,
            string hintWhenSet,
            bool readable,
            string boxTooltip,
            string browseTooltip,
            Action browse)
        {
            var row = new StackPanel { Margin = KpiTheme.Row };

            var caption = new StackPanel { Orientation = Orientation.Horizontal, Margin = KpiTheme.Hint };
            caption.Children.Add(Label(label));
            if (optional)
            {
                caption.Children.Add(new TextBlock
                {
                    Text = KpiPaneWords.Optional,
                    FontSize = KpiTheme.LabelSize,
                    Foreground = KpiTheme.Muted,
                    Margin = KpiTheme.LeftGap
                });
            }
            row.Children.Add(caption);

            bool set = path.Length > 0;
            bool gone = !set && remembered.Length > 0;
            bool fault = (!set && !optional) || gone || (set && !readable);

            var line = new DockPanel { LastChildFill = true };

            Button browseButton = KpiTheme.Small(Caption(KpiPaneWords.Browse));
            browseButton.Margin = KpiTheme.LeftGap;
            browseButton.ToolTip = Tip(browseTooltip);
            browseButton.Click += (sender, e) => browse();
            DockPanel.SetDock(browseButton, Dock.Right);
            line.Children.Add(browseButton);

            var inside = new DockPanel { LastChildFill = true, Margin = KpiTheme.InputPad };
            if (set && readable)
            {
                UIElement tick = KpiTheme.Tick(KpiTheme.Green);
                ((FrameworkElement)tick).Margin = KpiTheme.LeftGap;
                DockPanel.SetDock(tick, Dock.Right);
                inside.Children.Add(tick);
            }

            inside.Children.Add(new TextBlock
            {
                Text = set ? path : (gone ? remembered : string.Empty),
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = set ? KpiTheme.Text : KpiTheme.Muted
            });

            line.Children.Add(new Border
            {
                Height = KpiTheme.InputHeight,
                CornerRadius = KpiTheme.Rounded,
                Background = KpiTheme.InputFill,
                BorderBrush = fault ? KpiTheme.MissingPathBorder : KpiTheme.InputBorder,
                BorderThickness = KpiTheme.Hairline,
                Child = inside,
                ToolTip = Tip(gone ? remembered + "\n\n" + boxTooltip : boxTooltip)
            });

            row.Children.Add(line);

            string hint = !set ? (gone ? KpiPaneWords.NotFound : KpiPaneWords.NotSet) : hintWhenSet;
            if (hint.Length > 0)
            {
                row.Children.Add(new TextBlock
                {
                    Text = hint,
                    FontSize = KpiTheme.StatusSize,
                    Foreground = fault ? KpiTheme.FailedCount : KpiTheme.Muted,
                    Margin = KpiTheme.Hint
                });
            }

            return row;
        }

        // ---------------------------------------------------------------- step 2, Read

        /// <summary>
        /// **STEP 2.** The one press that starts a read apart from Create, what it found, the
        /// three parameters the values come from, and every problem the read found.
        /// </summary>
        private void InsideRead(KpiStepState state)
        {
            if (ShowsWhyNot(state, KpiPaneWords.ReadModel)) return;

            if (_facts == null)
            {
                string why = _model.IsOpen ? CreateWords.NothingHasReadThisModel : CreateWords.NoModelToReadPlotsFrom;
                Button read = KpiTheme.Main(Caption(KpiPaneWords.ReadModel));
                read.IsEnabled = _model.IsOpen && !_reading;
                read.ToolTip = Tip(why);
                read.Click += (sender, e) => PressRead();
                _footerButtons.Children.Add(read);
                // While the read runs, "waits for a press" is no longer true, so the line carries no tooltip.
                _body.Children.Add(Muted(KpiPaneWords.ReadsSheetsAndSchedules, _reading ? null : why));

                if (!_model.IsOpen) _stepSays = new StatusLine(KpiPaneWords.NoModelOpen, CreateWords.NoModelToReadPlotsFrom);
                else if (_reading) _stepSays = StatusLine.Reading;
                else _stepSays = new StatusLine(KpiPaneWords.NotReadYet, KpiHeader.NothingReadYet);
                return;
            }

            Button again = KpiTheme.Small(Caption(KpiPaneWords.ReadAgain));
            again.Height = KpiTheme.SmallButtonHeight;
            again.IsEnabled = _model.IsOpen && !_reading;
            again.ToolTip = Tip(CreateWords.NothingHasReadThisModel);
            again.Click += (sender, e) => PressRead();
            _body.Children.Add(Section(KpiPaneWords.ModelRead, again, null));

            var boxes = new UniformGrid { Columns = KpiTheme.ReadBoxes, Margin = KpiTheme.Row };
            boxes.Children.Add(CountBox(
                KpiPaneWords.Number(_facts.Plots.All.Count), KpiPaneWords.PlotsBox, KpiTheme.Text,
                string.Join("\n", CreateWords.PlotSources(_facts.Plots).ToArray())));
            boxes.Children.Add(CountBox(
                KpiPaneWords.Number(_facts.ElementInstances), KpiPaneWords.ElementsBox, KpiTheme.Text,
                KpiHeader.ReadIn(_read)));
            boxes.Children.Add(CountBox(
                KpiPaneWords.Seconds(_facts.ReadSeconds), KpiPaneWords.TimeBox, KpiTheme.Text,
                KpiHeader.ReadIn(_read)));
            _body.Children.Add(boxes);

            PlotListRead list = PlotListFile.In(PlotListFileSetting.Read());
            if (list.Set && list.Read)
            {
                int named = _facts.Plots.All.Count(one => list.Plots.Contains(one, StringComparer.Ordinal));
                _body.Children.Add(ValueLine(
                    KpiPaneWords.InPlotList,
                    KpiPaneWords.OfCount(named, _facts.Plots.All.Count),
                    TickingTheList.Heading + " " + list.InWords));
            }

            IReadOnlyList<RecognisedWorkbook> workbooks = Listing();
            int matched = workbooks.Where(one => one.IsMatched && KpiTemplates.All.Contains(one.Template))
                .Select(one => one.Template).Distinct().Count();
            _body.Children.Add(ValueLine(
                KpiPaneWords.TemplatesMatched,
                KpiPaneWords.OfCount(matched, KpiTemplates.All.Count),
                TemplateWords.Listed(workbooks.Count, workbooks.Count(one => one.IsMatched))));

            _body.Children.Add(Section(KpiPaneWords.ValuesFrom, null, null));
            _body.Children.Add(ValuePicker(
                KpiPaneWords.Component, _facts.ComponentNames, _componentParameter,
                chosen => { _componentParameter = chosen; Changed(); }));
            _body.Children.Add(ValuePicker(
                KpiPaneWords.Reference, KpiNames.PlotNamesOnSheets.ToList(), _referenceParameter,
                chosen => { _referenceParameter = chosen; Changed(); }));
            _body.Children.Add(ValuePicker(
                KpiPaneWords.Location, _facts.LocationNames, _locationParameter,
                chosen => { _locationParameter = chosen; Changed(); }));

            IReadOnlyList<PaneWarning> warnings = PaneWarnings.ForRead(_facts.Plots, _facts.Links, list);
            if (warnings.Count > 0) _body.Children.Add(new Border { Margin = KpiTheme.Group });
            foreach (PaneWarning warning in warnings) _body.Children.Add(WarningBox(warning, null));
            foreach (UIElement box in UnknownValueBoxes()) _body.Children.Add(box);

            Button next = KpiTheme.Main(Caption(KpiPaneWords.NextPlots));
            next.Click += (sender, e) => GoTo(KpiStep.Plots);
            _footerButtons.Children.Add(next);

            _stepSays = _reading
                ? StatusLine.Reading
                : new StatusLine(KpiPaneWords.ReadAtClock(_read.When), KpiHeader.ReadIn(_read));
        }

        /// <summary>
        /// **THE ONE PRESS THAT STARTS A READ APART FROM CREATE.** It used to start itself when
        /// the pane was shown, and every model anybody opened was read with nobody asking.
        /// </summary>
        private void PressRead()
        {
            _reading = true;
            _told = StatusLine.Reading;
            RedrawTemplates();
            Ask(KpiRequest.Plots);
        }

        /// <summary>
        /// One parameter: its label and a dropdown of what the model offers, preselected the way
        /// it always was. What each holds on the first ticked plot is the dropdown's tooltip.
        /// The dropdown is built fresh on every redraw from the read, so it carries no list of
        /// its own across one.
        /// </summary>
        private UIElement ValuePicker(
            string caption, IReadOnlyList<string> names, string chosen, Action<string> pick)
        {
            ComboBox box = KpiTheme.Dropdown();
            box.ItemsSource = names;
            box.IsEnabled = names.Count > 0;
            if (names.Contains(chosen, StringComparer.Ordinal)) box.SelectedItem = chosen;
            box.ToolTip = Tip(names.Count == 0 ? KpiPaneWords.NothingHoldsThatWord : WhatEachHolds());

            // Attached after the selection is set, so setting it does not count as a pick.
            box.SelectionChanged += (sender, e) =>
            {
                string now = box.SelectedItem as string;
                if (!string.IsNullOrEmpty(now) && !string.Equals(now, chosen, StringComparison.Ordinal)) pick(now);
            };

            var row = new DockPanel { LastChildFill = true, Margin = KpiTheme.Row };
            TextBlock label = Label(caption);
            label.Width = KpiTheme.LabelWidth;
            label.VerticalAlignment = VerticalAlignment.Center;
            DockPanel.SetDock(label, Dock.Left);
            row.Children.Add(label);
            row.Children.Add(box);
            return row;
        }

        /// <summary>
        /// What the four plot parameters hold on the FIRST TICKED plot, with that plot named,
        /// or why there is nothing to show. A value belonging to a plot nobody chose is worse
        /// than no value.
        /// </summary>
        private string WhatEachHolds()
        {
            if (_facts == null || _ticks.Count == 0) return CreateWords.NoPlotForTheReferenceValues;

            string plotId = _ticks.Ticked[0];
            var lines = new List<string> { CreateWords.ReferenceValuesOn(plotId) };
            lines.AddRange(_facts.ReferenceValuesOn(plotId).Select(one => one.InWords));
            return string.Join("\n", lines.ToArray());
        }

        // ---------------------------------------------------------------- step 3, Plots

        /// <summary>
        /// **STEP 3.** The asset types, which tick their own plots, then the plots, then the
        /// list tick when it has gone out of date.
        /// </summary>
        private void InsideStepThree(KpiStepState state)
        {
            if (ShowsWhyNot(state, KpiPaneWords.NextCreate)) return;

            TheAssetTypes();
            ThePlots();

            PaneWarning stale = PaneWarnings.ListTick(_listTick, Settled().Select(one => one.Workbook.FileName));
            if (stale != null)
            {
                PlotListRead again = PlotListFile.In(PlotListFileSetting.Read());
                Button press = KpiTheme.Link(Caption(KpiPaneWords.TickListAgain));
                press.IsEnabled = again.Read;
                press.Click += (sender, e) => PressTickTheList();
                _body.Children.Add(WarningBox(stale, press));
            }

            Button next = KpiTheme.Main(Caption(KpiPaneWords.NextCreate));
            next.IsEnabled = state.Done;
            next.ToolTip = Tip(state.Done ? string.Empty : _steps.PlotsLack);
            next.Click += (sender, e) => GoTo(KpiStep.Create);
            _footerButtons.Children.Add(next);

            _stepSays = new StatusLine(KpiPaneWords.PlotsSelected(_ticks.Count), _ticks.InWords);
        }

        /// <summary>
        /// The templates folder's workbooks, one tick each in two columns, with the plots that
        /// belong to each on the right. **Ticking one ticks its plots**, by the split's own rule.
        /// A file caught between the two park templates waits under the grid with a button for
        /// each, because nothing is guessed.
        /// </summary>
        private void TheAssetTypes()
        {
            IReadOnlyList<RecognisedWorkbook> recognised = Listing();
            int tickable = recognised.Count(one => one.IsMatched || one.NeedsAPick);

            _body.Children.Add(Section(
                KpiPaneWords.AssetTypes,
                RightCount(KpiPaneWords.OfCount(_picks.Count, tickable)),
                _whyThisTemplate));

            if (recognised.Count == 0)
            {
                _body.Children.Add(Muted(KpiPaneWords.NoTemplatesInTheFolder, TemplateWords.EmptyFolder));
                return;
            }

            var grid = new UniformGrid { Columns = KpiTheme.AssetColumns, Margin = KpiTheme.Row };
            var waiting = new List<WorkbookTick>();

            foreach (RecognisedWorkbook workbook in recognised)
            {
                RecognisedWorkbook which = workbook;
                bool canTick = which.IsMatched || which.NeedsAPick;
                WorkbookTick tick = TickFor(which);
                if (tick != null && tick.SettledAs == null) waiting.Add(tick);

                KpiTemplate template = tick != null && tick.SettledAs != null ? tick.SettledAs : which.Template;

                var cell = new DockPanel { LastChildFill = true, Margin = KpiTheme.RightGap };

                if (template != null && _facts != null)
                {
                    IReadOnlyList<string> belong = TickingATemplate.PlotsOf(template, _facts.Plots.All, ComponentOn);
                    int going = belong.Count(one => _ticks.IsTicked(one));
                    var count = new TextBlock
                    {
                        Text = tick == null || going == belong.Count
                            ? KpiPaneWords.Number(belong.Count)
                            : KpiPaneWords.OfCount(going, belong.Count),
                        Foreground = KpiTheme.Muted,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    DockPanel.SetDock(count, Dock.Right);
                    cell.Children.Add(count);
                }

                var box = new CheckBox
                {
                    Content = new TextBlock
                    {
                        Text = template != null ? AssetTypeNames.Display(template) : which.FileName,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    },
                    Foreground = KpiTheme.Text,
                    IsEnabled = canTick,
                    IsChecked = tick != null,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Margin = KpiTheme.Row,
                    ToolTip = Tip(canTick ? AssetTypeTooltip(which, template) : which.FileName + "\n\n" + which.Reason)
                };
                KpiTheme.ShowsWhyWhenOff(box);
                box.Click += (sender, e) => ToggledWorkbook(which);
                cell.Children.Add(box);

                grid.Children.Add(cell);
            }

            _body.Children.Add(grid);

            foreach (WorkbookTick tick in waiting) _body.Children.Add(ParkPick(tick));
        }

        /// <summary>
        /// Everything a workbook row used to say in full: the file, what it was recognised as,
        /// how many of its plots are going in, and what it would fill.
        /// </summary>
        private string AssetTypeTooltip(RecognisedWorkbook workbook, KpiTemplate template)
        {
            var lines = new List<string>
            {
                workbook.FileName + ", " + workbook.InWords,
                "Tick this workbook. Several can be ticked at once."
            };

            if (template != null)
            {
                string some = TickingATemplate.RowLine(template, _ticks, ComponentOn);
                if (some.Length > 0) lines.Add(some);

                lines.Add(string.Empty);
                lines.Add("What " + template.Name + " would fill:");
                lines.AddRange(TemplateWords.WouldFill(template, Chosen()));
            }

            return string.Join("\n", lines.ToArray());
        }

        /// <summary>
        /// A ticked file caught between the two park templates: its name and a button for each.
        /// </summary>
        private UIElement ParkPick(WorkbookTick tick)
        {
            var row = new DockPanel { LastChildFill = true, Margin = KpiTheme.Row, ToolTip = Tip(TemplateWords.PickBetween(tick.Workbook)) };

            var buttons = new StackPanel { Orientation = Orientation.Horizontal };
            foreach (KpiTemplate candidate in tick.Workbook.Candidates)
            {
                KpiTemplate chosen = candidate;
                Button choice = KpiTheme.Small(Caption(AssetTypeNames.Display(chosen)));
                choice.Margin = KpiTheme.LeftGap;
                choice.Click += (sender, e) =>
                {
                    tick.SettledAs = chosen;
                    TickItsPlots(tick);
                    _showingResults = false;
                    RedrawTemplates();
                };
                buttons.Children.Add(choice);
            }
            DockPanel.SetDock(buttons, Dock.Right);
            row.Children.Add(buttons);

            row.Children.Add(new TextBlock
            {
                Text = tick.Workbook.FileName,
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center
            });

            return row;
        }

        /// <summary>
        /// The plots: a search box over All, None and Plot list, then one row per plot. **All
        /// and None act on every plot, the search only narrows what is shown.**
        /// </summary>
        private void ThePlots()
        {
            if (_facts == null) return;

            _body.Children.Add(Section(
                KpiPaneWords.PlotsSection,
                RightCount(KpiPaneWords.OfCount(_ticks.Count, _facts.Plots.All.Count)),
                null));

            var bar = new DockPanel { LastChildFill = true, Margin = KpiTheme.Row };
            var buttons = new StackPanel { Orientation = Orientation.Horizontal };

            // **ALL AND NONE BOTH FORGET EVERY HAND CHOICE.** They replace every tick, so a per
            // plot choice left standing behind them is a record that disagrees with the screen.
            Button all = KpiTheme.Small(Caption(KpiPaneWords.All));
            all.Margin = KpiTheme.LeftGap;
            all.Click += (sender, e) => ReplacedEveryTick(_ticks.All());
            buttons.Children.Add(all);

            Button none = KpiTheme.Small(Caption(KpiPaneWords.None));
            none.Margin = KpiTheme.LeftGap;
            none.Click += (sender, e) => ReplacedEveryTick(_ticks.None());
            buttons.Children.Add(none);

            // **PLOT LIST REPLACES EVERY TICK AND FORGETS EVERY HAND CHOICE**, as All and None
            // do. The rule is TickingTheList in Core and this button only presses it.
            string listFile = PlotListFileSetting.Read();
            PlotListRead sent = PlotListFile.In(listFile);
            Button listed = KpiTheme.Small(Caption(KpiPaneWords.PlotListButton));
            listed.Margin = KpiTheme.LeftGap;
            listed.IsEnabled = sent.Read;
            listed.ToolTip = Tip(sent.Read
                ? "Replace every tick with exactly the plots the plot list file names, "
                    + "and forget every plot ticked or unticked by hand."
                : KpiPaneWords.PlotListSays(listFile, sent));
            listed.Click += (sender, e) => PressTickTheList();
            buttons.Children.Add(listed);

            DockPanel.SetDock(buttons, Dock.Right);
            bar.Children.Add(buttons);
            bar.Children.Add(Reparented(_search));
            _body.Children.Add(bar);

            _plotRows = new StackPanel();
            FillPlotRows();

            _body.Children.Add(new Border
            {
                CornerRadius = KpiTheme.Rounded,
                BorderBrush = KpiTheme.InputBorder,
                BorderThickness = KpiTheme.Hairline,
                Margin = KpiTheme.Row,
                Child = Remembering(new ScrollViewer
                {
                    Content = _plotRows,
                    MaxHeight = KpiTheme.PlotListHeight,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
                }, "plots")
            });
        }

        /// <summary>
        /// The plot rows the search box lets through: the tick, the id in Consolas, and on the
        /// right the asset type the split's own rule gives it. **A plot no route places reads
        /// No type, and one whose folder another ticked plot shares reads Shared id**, both
        /// greyed and both still tickable, because both can be ticked today.
        /// </summary>
        private void FillPlotRows()
        {
            if (_plotRows == null || _facts == null) return;

            _plotRows.Children.Clear();
            string wanted = _search.Text.Trim();

            IReadOnlyList<SharedUid2Group> sharing = SharedUid2.Of(TickedFilings());

            foreach (string plotId in _facts.Plots.All)
            {
                if (wanted.Length > 0 && plotId.IndexOf(wanted, StringComparison.OrdinalIgnoreCase) < 0) continue;

                string which = plotId;
                PlotTemplate placed = PlotsPerTemplate.For(which, ComponentOn(which));
                bool shared = SharedUid2.Stops(sharing, which);

                string right;
                string why = string.Empty;
                if (placed.Template == null)
                {
                    right = KpiPaneWords.NoType;
                    why = placed.Why;
                }
                else if (shared)
                {
                    right = KpiPaneWords.SharedId;
                    why = SharedUid2.WhyStopped(sharing, which);
                }
                else
                {
                    right = AssetTypeNames.Display(placed.Template);
                }

                bool greyed = why.Length > 0;
                var row = new DockPanel { LastChildFill = true, Margin = KpiTheme.InputPad, ToolTip = Tip(why) };

                var type = new TextBlock
                {
                    Text = right,
                    Foreground = KpiTheme.Muted,
                    FontSize = KpiTheme.LabelSize,
                    VerticalAlignment = VerticalAlignment.Center
                };
                DockPanel.SetDock(type, Dock.Right);
                row.Children.Add(type);

                var box = new CheckBox
                {
                    Content = new TextBlock
                    {
                        Text = which,
                        FontFamily = KpiTheme.Mono,
                        Foreground = greyed ? KpiTheme.Muted : KpiTheme.Text
                    },
                    IsChecked = _ticks.IsTicked(which),
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Margin = KpiTheme.Hint
                };
                box.Click += (sender, e) =>
                {
                    // **A PLOT TICKED OR UNTICKED BY HAND WINS, IN BOTH DIRECTIONS.**
                    TickedByHand(
                        _ticks.Toggled(which),
                        _ticks.IsTicked(which) ? _byHand.TakenOff(which) : _byHand.PutOn(which));
                };
                row.Children.Add(box);

                _plotRows.Children.Add(row);
            }
        }

        private void ReplacedEveryTick(PlotTicks next)
        {
            _listTick = ListTickFreshness.NeverPressed;
            TickedByHand(next, _byHand.Forgotten());
        }

        /// <summary>
        /// **THE ROWS ARE RECORDED OFF WHAT THE PRESS LEAVES, NOT WHAT IT STARTED FROM**, so the
        /// press does not raise its own out of date warning the moment it finishes.
        /// </summary>
        private void PressTickTheList()
        {
            TickedByHand(
                TickingTheList.Ticked(_ticks, PlotListFile.In(PlotListFileSetting.Read())),
                _byHand.Forgotten());

            _listTick = ListTickFreshness.Pressed(Settled().Select(one => one.Workbook.FileName));

            RedrawTemplates();
        }

        // ---------------------------------------------------------------- step 4, Create

        /// <summary>
        /// **STEP 4.** How many workbooks a press would write, by asset type, every box that
        /// would stop or change a plot, and the button. After a press the result takes its place.
        /// </summary>
        private void InsideCreate(KpiStepState state)
        {
            if (_showingResults && _lastSet != null)
            {
                TheResult();
                return;
            }

            TemplateSplit split = TheSplit();
            string output = OutputFolder.Read();
            string templates = TemplateFolder.Read();
            int workbooks = KpiWillWrite.Workbooks(split, TickedFilings());

            if (state.Usable)
            {
                _body.Children.Add(Section(KpiPaneWords.ReadyToCreate, null, KpiWillWrite.InWords(split)));

                var big = new StackPanel { Orientation = Orientation.Horizontal, Margin = KpiTheme.Row };
                big.Children.Add(new TextBlock
                {
                    Text = KpiPaneWords.Number(workbooks),
                    FontSize = KpiTheme.BigNumberSize,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = KpiTheme.Green,
                    VerticalAlignment = VerticalAlignment.Bottom
                });
                big.Children.Add(new TextBlock
                {
                    Text = KpiPaneWords.WorkbooksBeside(workbooks, FormsFolder.Read().Length > 0),
                    Foreground = KpiTheme.Muted,
                    Margin = KpiTheme.LeftGap,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    ToolTip = Tip(TemplateWords.FormsFolder(FormsFolder.Read()))
                });
                _body.Children.Add(big);

                TheTable(split);

                PaneWarning sameFolder = PaneWarnings.ForOutputFolder(output, templates);
                if (sameFolder != null) _body.Children.Add(WarningBox(sameFolder, null));

                foreach (PaneWarning warning in CreateWarnings(split)) _body.Children.Add(WarningBox(warning, null));

                TheAnswerable();

                _body.Children.Add(Muted(KpiPaneWords.Replaced, TemplateWords.Output(output)));
            }
            else
            {
                _body.Children.Add(new TextBlock
                {
                    Text = state.WhyNot,
                    Foreground = KpiTheme.Muted,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = KpiTheme.Group
                });
            }

            // Greyed with ONE reason, the one KpiSteps worked out for step 4 itself, so the body
            // and this line cannot say two things. The full refusal is the line's tooltip, and
            // the handler asks the live document again at the press, which is what decides.
            string cannot = CreateWords.CannotCreate(_model, output, Settled().Count > 0, _ticks.Count > 0);
            string reason = _steps.CreateBlockedBy;

            Button create = KpiTheme.Main(Caption(KpiPaneWords.CreateWorkbooks(workbooks)));
            create.IsEnabled = reason.Length == 0;
            create.ToolTip = Tip(reason.Length == 0 ? KpiPaneWords.ReadOnly : reason);
            create.Click += (sender, e) => AskedToCreate();
            _footerButtons.Children.Add(create);

            _stepSays = reason.Length == 0
                ? new StatusLine(KpiPaneWords.NothingInTheModelChanges, KpiPaneWords.ReadOnly)
                : new StatusLine(reason, cannot.Length > 0 ? cannot : reason);
        }

        /// <summary>
        /// The table under the number: asset type, plots, and the first to the last plot.
        /// </summary>
        private void TheTable(TemplateSplit split)
        {
            IReadOnlyList<WillWriteRow> rows = KpiWillWrite.Table(split);
            if (rows.Count == 0) return;

            var table = new StackPanel { Margin = KpiTheme.Row };
            table.Children.Add(TableRow(
                KpiPaneWords.AssetTypeColumn, KpiPaneWords.PlotsColumn, KpiPaneWords.RangeColumn, true));

            // The table's rows are the writing shares in the split's own order, so each row's
            // tooltip is its share's old line.
            List<TemplateShare> writing = split.Shares.Where(one => one.WillWrite).ToList();
            for (int at = 0; at < rows.Count; at++)
            {
                UIElement line = TableRow(rows[at].AssetType, KpiPaneWords.Number(rows[at].Plots), rows[at].Range, false);
                ((FrameworkElement)line).ToolTip = Tip(CreateWords.TemplateRow(writing[at]));
                table.Children.Add(line);
            }

            _body.Children.Add(table);
        }

        private static UIElement TableRow(string assetType, string plots, string range, bool heading)
        {
            var row = new DockPanel
            {
                LastChildFill = true,
                Margin = KpiTheme.Hint
            };

            Brush colour = heading ? KpiTheme.Muted : KpiTheme.Text;
            double size = heading ? KpiTheme.LabelSize : KpiTheme.BodySize;

            var rangeText = new TextBlock
            {
                Text = range,
                Width = KpiTheme.RangeWidth,
                Foreground = colour,
                FontSize = size,
                FontFamily = heading ? KpiTheme.Body : KpiTheme.Mono,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            DockPanel.SetDock(rangeText, Dock.Right);
            row.Children.Add(rangeText);

            var plotsText = new TextBlock { Text = plots, Width = KpiTheme.CountWidth, Foreground = colour, FontSize = size };
            DockPanel.SetDock(plotsText, Dock.Right);
            row.Children.Add(plotsText);

            row.Children.Add(new TextBlock
            {
                Text = assetType,
                Foreground = colour,
                FontSize = size,
                TextTrimming = TextTrimming.CharacterEllipsis
            });

            return row;
        }

        private IReadOnlyList<PaneWarning> CreateWarnings(TemplateSplit split)
        {
            return PaneWarnings.ForCreate(
                split,
                PlotListFile.In(PlotListFileSetting.Read()),
                _facts == null ? null : _facts.Plots,
                ComponentOn,
                _ticks.Ticked,
                SharedUid2.Of(TickedFilings()),
                _facts == null ? null : _facts.Links,
                Held());
        }

        /// <summary>
        /// What the last press left for somebody to ANSWER and that is still on screen here: a
        /// plot whose two regions left the question open, and identical areas waiting on a
        /// confirm. **Both re-press**, with the readings the press holds, so nothing is read
        /// again to answer them.
        /// </summary>
        private void TheAnswerable()
        {
            foreach (KpiCreateRun run in Held())
            {
                foreach (PlotReading reading in run.Readings.Where(NeedsARegion))
                {
                    _body.Children.Add(FailedLine(
                        reading.PlotId, FailedReasons.Short(NotWrittenKind.RegionPick),
                        string.Join(" ", run.Reconciliation.Refusals.ToArray())));
                    _body.Children.Add(RegionButtons(reading));
                }
            }

            if (_confirmedIdentical) return;

            PaneWarning same = PaneWarnings.SameArea(
                Held().SelectMany(one => one.Reconciliation.IdenticalAreas).ToList());
            if (same == null) return;

            Button confirm = KpiTheme.Small(Caption(KpiPaneWords.WriteAnyway));
            confirm.Margin = KpiTheme.StatusGap;
            confirm.HorizontalAlignment = HorizontalAlignment.Left;
            confirm.Click += (sender, e) =>
            {
                _confirmedIdentical = true;
                AskedToCreate();
            };
            _body.Children.Add(WarningBox(same, confirm, true));
        }

        private static bool NeedsARegion(PlotReading reading)
        {
            return NotWrittenKinds.WaitsOnARegion(reading);
        }

        private UIElement RegionButtons(PlotReading reading)
        {
            var row = new WrapPanel { Margin = KpiTheme.Row };
            foreach (RegionArea region in reading.RegionsHoldingAnArea)
            {
                string plotId = reading.PlotId;
                string typeName = region.TypeName;
                Button choice = KpiTheme.Small(Caption(typeName + "   " + region.Printed));
                choice.Margin = KpiTheme.RightGap;
                choice.Click += (sender, e) =>
                {
                    _chosenRegions[plotId] = typeName;
                    AskedToCreate();
                };
                row.Children.Add(choice);
            }

            return row;
        }

        // ---------------------------------------------------------------- the result

        /// <summary>
        /// **WHAT THE PRESS DID, IN PLACE OF STEP 4.** A banner, Created and Failed counted off
        /// the run's own outcomes, one short line per plot that wrote nothing with its full
        /// reason as the tooltip, the ready count, and the way to the folder and the report.
        /// </summary>
        private void TheResult()
        {
            PressResult result = PressResult.Of(_lastSet);
            KpiResults ready = KpiResults.Of(_lastSet);

            // The banner's colours follow its kind, which Core counts off the outcomes: green only
            // where every workbook and every expected PDF was written.
            Brush fill = KpiTheme.GreenTint, edge = KpiTheme.GreenTint, words = KpiTheme.Green;
            if (result.Kind == ResultBanner.WorkbooksNotCreated)
            {
                fill = KpiTheme.ErrorFill;
                edge = KpiTheme.ErrorBorder;
                words = KpiTheme.ErrorText;
            }
            else if (result.Kind == ResultBanner.PdfsNotWritten)
            {
                fill = KpiTheme.WarningFill;
                edge = KpiTheme.WarningBorder;
                words = KpiTheme.WarningText;
            }

            _body.Children.Add(new Border
            {
                Background = fill,
                BorderBrush = edge,
                BorderThickness = KpiTheme.Hairline,
                CornerRadius = KpiTheme.Rounded,
                Padding = KpiTheme.Box,
                Margin = KpiTheme.Group,
                ToolTip = Tip(ready.CountsInWords),
                Child = new TextBlock
                {
                    Text = result.Banner,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = words,
                    TextWrapping = TextWrapping.Wrap
                }
            });

            var counts = new UniformGrid { Columns = KpiTheme.ResultBoxes, Margin = KpiTheme.Group };
            counts.Children.Add(CountBox(KpiPaneWords.Number(result.Created), KpiPaneWords.Created, KpiTheme.Text, null));
            counts.Children.Add(CountBox(
                KpiPaneWords.Number(result.Failed), KpiPaneWords.Failed,
                result.Failed > 0 ? KpiTheme.FailedCount : KpiTheme.Text, null));
            _body.Children.Add(counts);

            TheTypeTable(result);

            if (result.Failed > 0)
            {
                _body.Children.Add(Section(KpiPaneWords.Failed, null, null));

                var failed = new StackPanel();
                foreach (FailedPlot plot in result.FailedPlots)
                {
                    failed.Children.Add(FailedLine(plot.PlotId, plot.Short, plot.Why));

                    if (plot.Kind != NotWrittenKind.RegionPick) continue;

                    PlotReading reading = Held().SelectMany(one => one.Readings)
                        .FirstOrDefault(one => string.Equals(one.PlotId, plot.PlotId, StringComparison.Ordinal));
                    if (NeedsARegion(reading)) failed.Children.Add(RegionButtons(reading));
                }

                _body.Children.Add(Remembering(new ScrollViewer
                {
                    Content = failed,
                    MaxHeight = KpiTheme.PlotListHeight,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
                }, "failed"));
            }

            if (!_confirmedIdentical)
            {
                PaneWarning same = PaneWarnings.SameArea(
                    Held().SelectMany(one => one.Reconciliation.IdenticalAreas).ToList());
                if (same != null)
                {
                    Button confirm = KpiTheme.Small(Caption(KpiPaneWords.WriteAnyway));
                    confirm.Margin = KpiTheme.StatusGap;
                    confirm.HorizontalAlignment = HorizontalAlignment.Left;
                    confirm.Click += (sender, e) =>
                    {
                        _confirmedIdentical = true;
                        AskedToCreate();
                    };
                    _body.Children.Add(WarningBox(same, confirm, true));
                }
            }

            // These three are about the press itself, so the report it wrote holds each of them in
            // its own sections and the link is offered whenever that report is there.
            foreach (PaneWarning warning in PaneWarnings.GroupsLeftOut(Held())) _body.Children.Add(WarningBox(warning, null, true));

            PaneWarning pdfs = PaneWarnings.PdfsNotWritten(result);
            if (pdfs != null) _body.Children.Add(WarningBox(pdfs, null, true));

            if (ready.ReadyOf.Length > 0)
            {
                _body.Children.Add(ValueLine(
                    KpiPaneWords.ReadyToSend, ready.ReadyOf,
                    ready.CountsInWords + "\n" + ready.NotReadyInWords));
            }

            PaneWarning notReady = PaneWarnings.NotReady(ready);
            if (notReady != null) _body.Children.Add(WarningBox(notReady, null, true));

            var buttons = new UniformGrid { Columns = KpiTheme.ResultButtons, Margin = KpiTheme.Group };

            string output = OutputFolder.Read();
            Button folder = KpiTheme.Small(Caption(KpiPaneWords.OpenOutputFolder));
            folder.Margin = KpiTheme.RightGap;
            folder.IsEnabled = output.Length > 0 && Directory.Exists(output);
            folder.ToolTip = Tip(output.Length == 0 ? TemplateWords.NoOutputFolder : output);
            folder.Click += (sender, e) => Open(OutputFolder.Read());
            buttons.Children.Add(folder);

            Button report = KpiTheme.Small(Caption(KpiPaneWords.OpenReport));
            report.Margin = KpiTheme.LeftGap;
            report.IsEnabled = _reportWhere.Length > 0 && File.Exists(_reportWhere);
            report.ToolTip = Tip(_reportWhere.Length == 0 ? "This press wrote no report." : _reportWhere);
            report.Click += (sender, e) => Open(_reportWhere);
            buttons.Children.Add(report);

            _body.Children.Add(buttons);

            // Shown only where a retry can write something: a plot that needs a change in the
            // model or the ticks fails the same way on every press, and the status line says how
            // many were left out.
            if (result.Retryable.Count > 0)
            {
                Button retry = KpiTheme.Main(Caption(KpiPaneWords.RetryFailed));
                retry.Margin = KpiTheme.Row;
                retry.IsEnabled = _facts != null;
                retry.Click += (sender, e) => RetryFailed(result);
                _footerButtons.Children.Add(retry);
            }

            // **THE WAY BACK IS A PRESS AND NEVER A TIMER.** The result stands until somebody
            // says they are done with it.
            Button again = KpiTheme.Plain(Caption(KpiPaneWords.NewRun));
            again.Click += (sender, e) =>
            {
                _showingResults = false;
                _reportWhere = string.Empty;
                GoTo(KpiStep.Plots);
            };
            _footerButtons.Children.Add(again);

            _stepSays = new StatusLine(result.StatusLine, result.Status + ". " + ready.CountsInWords);
        }

        /// <summary>
        /// Ticks exactly the failed plots a retry can write, keeps their asset types ticked, and opens
        /// step 4. Every hand choice is forgotten, the same as every press that replaces the
        /// ticks, because a hand record left behind would disagree with the screen.
        /// </summary>
        private void RetryFailed(PressResult result)
        {
            if (_facts == null) return;

            _listTick = ListTickFreshness.NeverPressed;
            _stepOpen = KpiStep.Create;
            TickedByHand(
                new PlotTicks(_facts.Plots, result.Retryable),
                _byHand.Forgotten());
        }

        /// <summary>
        /// One row per ticked asset type, Created and Failed, counted in Core off the outcomes,
        /// because one line for the run would hide which of six failed.
        /// </summary>
        private void TheTypeTable(PressResult result)
        {
            if (result.ByAssetType.Count == 0) return;

            var table = new StackPanel { Margin = KpiTheme.Group };
            table.Children.Add(TypeRow(KpiPaneWords.AssetTypeColumn, KpiPaneWords.Created, KpiPaneWords.Failed, string.Empty, true));

            foreach (AssetTypeResult row in result.ByAssetType)
            {
                table.Children.Add(TypeRow(
                    row.AssetType,
                    KpiPaneWords.Number(row.Created),
                    KpiPaneWords.Number(row.Failed),
                    row.NothingToWrite ? AssetTypeResult.NothingToWriteWords : string.Empty,
                    row.Unplaced));
            }

            _body.Children.Add(table);
        }

        private static UIElement TypeRow(string assetType, string created, string failed, string aside, bool muted)
        {
            // The heading and the row of plots no ticked asset type took are both drawn muted.
            var row = new DockPanel { LastChildFill = true, Margin = KpiTheme.Hint };
            Brush colour = muted ? KpiTheme.Muted : KpiTheme.Text;
            double size = muted ? KpiTheme.LabelSize : KpiTheme.BodySize;

            var failedText = new TextBlock { Text = failed, Width = KpiTheme.CountWidth, Foreground = colour, FontSize = size };
            DockPanel.SetDock(failedText, Dock.Right);
            row.Children.Add(failedText);

            var createdText = new TextBlock { Text = created, Width = KpiTheme.CountWidth, Foreground = colour, FontSize = size };
            DockPanel.SetDock(createdText, Dock.Right);
            row.Children.Add(createdText);

            var name = new TextBlock { Text = assetType, Foreground = colour, FontSize = size };
            DockPanel.SetDock(name, Dock.Left);
            row.Children.Add(name);

            row.Children.Add(new TextBlock
            {
                Text = aside,
                Foreground = KpiTheme.Muted,
                FontSize = KpiTheme.LabelSize,
                Margin = KpiTheme.LeftGap,
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center
            });

            return row;
        }

        private UIElement FailedLine(string plotId, string reason, string why)
        {
            var line = new DockPanel { LastChildFill = true, Margin = KpiTheme.Row, ToolTip = Tip(why) };

            var id = new TextBlock
            {
                Text = plotId,
                FontFamily = KpiTheme.Mono,
                Width = KpiTheme.IdWidth,
                Foreground = KpiTheme.Text
            };
            DockPanel.SetDock(id, Dock.Left);
            line.Children.Add(id);

            line.Children.Add(new TextBlock
            {
                Text = reason,
                Foreground = KpiTheme.ErrorText,
                TextWrapping = TextWrapping.Wrap
            });

            return line;
        }

        // ---------------------------------------------------------------- shared pieces

        /// <summary>
        /// A section title: upper case, semibold, 11 pixels, muted, with a little room after
        /// each letter, and an optional control or count on the right.
        /// </summary>
        private static UIElement Section(string title, UIElement right, string tooltip)
        {
            var row = new DockPanel { LastChildFill = true, Margin = KpiTheme.SectionTitle, ToolTip = Tip(tooltip) };

            if (right != null)
            {
                DockPanel.SetDock(right, Dock.Right);
                row.Children.Add(right);
            }

            // **WPF HAS NO LETTER SPACING ON A TEXT BLOCK**, so each letter is its own run with
            // a margin after it, the one way that spaces glyphs without changing the text.
            var letters = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            foreach (char letter in (title ?? string.Empty).ToUpperInvariant())
            {
                letters.Children.Add(new TextBlock
                {
                    Text = letter.ToString(),
                    FontSize = KpiTheme.SectionSize,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = KpiTheme.Muted,
                    Margin = KpiTheme.LetterGap
                });
            }
            row.Children.Add(letters);

            return row;
        }

        private static UIElement RightCount(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = KpiTheme.LabelSize,
                Foreground = KpiTheme.Muted,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private static TextBlock Label(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = KpiTheme.LabelSize,
                FontWeight = FontWeights.SemiBold,
                Foreground = KpiTheme.Text
            };
        }

        private static UIElement Labelled(string caption, UIElement box)
        {
            var row = new StackPanel { Margin = KpiTheme.Row };
            TextBlock label = Label(caption);
            label.Margin = KpiTheme.Hint;
            row.Children.Add(label);
            row.Children.Add(box);
            return row;
        }

        private static UIElement Muted(string text, string tooltip)
        {
            return new TextBlock
            {
                Text = text,
                Foreground = KpiTheme.Muted,
                TextWrapping = TextWrapping.Wrap,
                Margin = KpiTheme.Group,
                ToolTip = Tip(tooltip)
            };
        }

        /// <summary>A line with its name on the left and its value on the right.</summary>
        private static UIElement ValueLine(string name, string value, string tooltip)
        {
            var line = new DockPanel { LastChildFill = true, Margin = KpiTheme.Row, ToolTip = Tip(tooltip) };
            var right = new TextBlock { Text = value, FontWeight = FontWeights.SemiBold, Foreground = KpiTheme.Text };
            DockPanel.SetDock(right, Dock.Right);
            line.Children.Add(right);
            line.Children.Add(new TextBlock { Text = name, Foreground = KpiTheme.Text });
            return line;
        }

        /// <summary>A number over its caption, in a light box.</summary>
        private static UIElement CountBox(string number, string caption, Brush colour, string tooltip)
        {
            var inside = new StackPanel();
            inside.Children.Add(new TextBlock
            {
                Text = number,
                FontSize = KpiTheme.CountSize,
                FontWeight = FontWeights.SemiBold,
                Foreground = colour,
                TextTrimming = TextTrimming.CharacterEllipsis
            });
            inside.Children.Add(new TextBlock
            {
                Text = caption,
                FontSize = KpiTheme.StatusSize,
                Foreground = KpiTheme.Muted
            });

            return new Border
            {
                Background = KpiTheme.InputFill,
                BorderBrush = KpiTheme.Divider,
                BorderThickness = KpiTheme.Hairline,
                CornerRadius = KpiTheme.Rounded,
                Padding = KpiTheme.Box,
                Margin = KpiTheme.CountBox,
                Child = inside,
                ToolTip = Tip(tooltip)
            };
        }

        /// <summary>
        /// Setup's boxes about the templates folder: each new template with Use and Skip, each
        /// file whose layout is unknown, and each components.csv that was not read whole. The
        /// last is the one a person cannot see otherwise, because a file not read lets the files
        /// below it answer.
        /// </summary>
        private IEnumerable<UIElement> TemplatePrompts()
        {
            var boxes = new List<UIElement>();

            foreach (RecognisedWorkbook workbook in Listing())
            {
                RecognisedWorkbook which = workbook;
                if (_skippedFiles.Contains(which.FileName)) continue;

                if (which.IsNewTemplate)
                {
                    var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = KpiTheme.StatusGap };
                    Button use = KpiTheme.Small(Caption(KpiPaneWords.Use));
                    use.Click += (sender, e) =>
                    {
                        _usedTemplates.Add(which.Proposed.Name);
                        Changed();
                    };
                    Button skip = KpiTheme.Small(Caption(KpiPaneWords.Skip));
                    skip.Margin = KpiTheme.LeftGap;
                    skip.Click += (sender, e) =>
                    {
                        _skippedFiles.Add(which.FileName);
                        RedrawTemplates();
                    };
                    buttons.Children.Add(use);
                    buttons.Children.Add(skip);

                    boxes.Add(WarningBox(new PaneWarning(
                        KpiPaneWords.NewTemplateSameAs(which.SameAs), which.FileName, KpiPaneWords.NewTemplateSays(which), false), buttons));
                }
                else if (which.UnknownLayout)
                {
                    boxes.Add(WarningBox(new PaneWarning(
                        KpiPaneWords.UnknownTemplateLayout, which.FileName, which.FileName + "\n\n" + which.Reason, false), null));
                }
            }

            ComponentTable table = ComponentTable.InForce;
            foreach (ComponentFile file in table.Files.Where(one => one.There && (!one.Read || one.Problems.Count > 0
                || table.Unmatched.Any(row => row.Path == one.Path))))
            {
                boxes.Add(WarningBox(new PaneWarning(
                    KpiPaneWords.ComponentFileSays(file, table.Unmatched.Any(row => row.Path == file.Path)), file.Path,
                    string.Join("\n", table.InWords().ToArray()), !file.Read), null));
            }

            return boxes;
        }

        /// <summary>
        /// Step 2's box per component value no components.csv places, after a read. Its plots
        /// are refused until a line is added. Use suggestion and Choose each add one line, and
        /// Skip hides the box until Revit closes with the plots still refused.
        /// </summary>
        private IEnumerable<UIElement> UnknownValueBoxes()
        {
            var boxes = new List<UIElement>();
            if (_facts == null) return boxes;

            ComponentTable table = ComponentTable.InForce;
            bool shared = TemplateFolder.Read().Length > 0;

            foreach (UnknownComponent unknown in UnknownComponents.In(_facts.Plots.All, ComponentOn, table, table.Templates))
            {
                UnknownComponent which = unknown;
                if (_skippedValues.Contains(which.Key)) continue;

                var lines = new StackPanel();
                lines.Children.Add(new TextBlock { Text = KpiPaneWords.PlotsCarryIt(which.Plots.Count), Foreground = KpiTheme.Muted });
                lines.Children.Add(new TextBlock { Text = KpiPaneWords.Suggested(which.Suggested), Foreground = KpiTheme.Text, TextWrapping = TextWrapping.Wrap });
                lines.Children.Add(new TextBlock { Text = KpiPaneWords.SuggestedFolder(which.Folder), Foreground = KpiTheme.Text, TextWrapping = TextWrapping.Wrap });
                lines.Children.Add(new TextBlock { Text = KpiPaneWords.AddsTo(shared), Foreground = KpiTheme.Muted, TextWrapping = TextWrapping.Wrap });

                var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = KpiTheme.StatusGap };
                Button suggestion = KpiTheme.Small(Caption(KpiPaneWords.UseSuggestion));
                suggestion.IsEnabled = which.Suggested != null;
                suggestion.Click += (sender, e) => AddComponent(which.Value, which.Suggested, which.Folder);
                Button choose = KpiTheme.Small(Caption(KpiPaneWords.Choose));
                choose.Margin = KpiTheme.LeftGap;
                choose.Click += (sender, e) =>
                {
                    _choosingValue = which.Key;
                    RedrawTemplates();
                };
                Button skip = KpiTheme.Small(Caption(KpiPaneWords.Skip));
                skip.Margin = KpiTheme.LeftGap;
                skip.Click += (sender, e) =>
                {
                    _skippedValues.Add(which.Key);
                    RedrawTemplates();
                };
                buttons.Children.Add(suggestion);
                buttons.Children.Add(choose);
                buttons.Children.Add(skip);
                lines.Children.Add(buttons);

                if (_choosingValue == which.Key) lines.Children.Add(Choosing(which, table));

                boxes.Add(WarningBox(new PaneWarning(
                    KpiPaneWords.UnknownValue(which.Value), PlotSpan.Of(which.Plots),
                    KpiPaneWords.UnknownValueSays(which, ComponentFiles.SharedPath(TemplateFolder.Read()), ComponentFiles.PersonalPath()),
                    true), lines));
            }

            return boxes;
        }

        /// <summary>
        /// The two dropdowns Choose opens, every template the table knows and every folder it
        /// already files under with the value itself first, and Add.
        /// </summary>
        private UIElement Choosing(UnknownComponent unknown, ComponentTable table)
        {
            var panel = new StackPanel { Margin = KpiTheme.StatusGap };

            ComboBox templates = KpiTheme.Dropdown();
            foreach (KpiTemplate one in table.Templates) templates.Items.Add(one.Name);
            templates.SelectedItem = unknown.Suggested == null ? null : unknown.Suggested.Name;

            ComboBox folders = KpiTheme.Dropdown();
            folders.Items.Add(unknown.Folder);
            foreach (string one in ComponentFolders.Folders)
            {
                if (!string.Equals(one, unknown.Folder, StringComparison.OrdinalIgnoreCase)) folders.Items.Add(one);
            }
            folders.SelectedIndex = 0;
            folders.Margin = KpiTheme.StatusGap;

            Button add = KpiTheme.Small(Caption(KpiPaneWords.Add));
            add.Margin = KpiTheme.StatusGap;
            add.HorizontalAlignment = HorizontalAlignment.Left;
            add.Click += (sender, e) =>
            {
                KpiTemplate picked = KpiTemplates.Named(templates.SelectedItem as string, table.Templates);
                string folder = folders.SelectedItem as string;
                if (picked == null || string.IsNullOrWhiteSpace(folder))
                {
                    _told = new StatusLine(KpiPaneWords.PickATemplateAndAFolder);
                    PaintStatus();
                    return;
                }

                AddComponent(unknown.Value, picked, folder);
            };

            panel.Children.Add(templates);
            panel.Children.Add(folders);
            panel.Children.Add(add);
            return panel;
        }

        /// <summary>
        /// Writes the one line and says where it went on the status line, then draws again, which
        /// reads the files afresh so the value's plots are placed on this same redraw.
        /// </summary>
        private void AddComponent(string value, KpiTemplate template, string folder)
        {
            if (template == null) return;

            // Asked again at the press of the button, because a colleague may have added the value
            // to the shared file since the box was drawn, and a second line would refuse it.
            if (ComponentFiles.PutInForce(_madeTemplates.Values).Names(value))
            {
                _choosingValue = string.Empty;
                Changed();
                _told = new StatusLine(KpiPaneWords.AlreadyInAFile);
                PaintStatus();
                return;
            }

            string why;
            string written = ComponentFiles.Add(value, template.Name, folder, out why);
            _choosingValue = string.Empty;
            Changed();
            _told = written.Length > 0
                ? new StatusLine(KpiPaneWords.LineWritten(written))
                : new StatusLine(KpiPaneWords.LineNotWritten, why);
            PaintStatus();
        }

        /// <summary>
        /// One warning box: the triangle, the bold line, the plot ids in Consolas, and Show in
        /// report. **A box whose kind stops a write takes the error colours**, read off
        /// <see cref="PaneWarning.Blocks"/> and never off its words. Its whole text is the
        /// tooltip.
        ///
        /// **SHOW IN REPORT POINTS ONLY INTO A FILE THAT HOLDS THE BOX.** A box from before the
        /// press is offered the link when that press's BEFORE THE PRESS held the same box, and a
        /// box about the press itself is held by the report's own sections.
        /// </summary>
        private UIElement WarningBox(PaneWarning warning, UIElement extra, bool aboutThePress = false)
        {
            Brush text = warning.Blocks ? KpiTheme.ErrorText : KpiTheme.WarningText;

            var lines = new StackPanel();
            lines.Children.Add(new TextBlock
            {
                Text = warning.Title,
                FontWeight = FontWeights.SemiBold,
                Foreground = text,
                TextWrapping = TextWrapping.Wrap
            });

            if (warning.Plots.Length > 0)
            {
                lines.Children.Add(new TextBlock
                {
                    Text = warning.Plots,
                    FontFamily = KpiTheme.Mono,
                    Foreground = text,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Margin = KpiTheme.StatusGap
                });
            }

            bool reportThere = _lastReport.Length > 0 && File.Exists(_lastReport)
                && (aboutThePress || _lastBefore.Holds(warning));
            Button show = KpiTheme.Link(Caption(reportThere ? KpiPaneWords.ShowInReport : KpiPaneWords.InTheReportAfterCreate));
            show.IsEnabled = reportThere;
            show.Margin = KpiTheme.StatusGap;
            show.Click += (sender, e) => Open(_lastReport);
            lines.Children.Add(show);

            if (extra != null) lines.Children.Add(extra);

            var inside = new DockPanel { LastChildFill = true };
            UIElement icon = KpiTheme.Triangle(text);
            ((FrameworkElement)icon).Margin = KpiTheme.IconGap;
            DockPanel.SetDock(icon, Dock.Left);
            inside.Children.Add(icon);
            inside.Children.Add(lines);

            return new Border
            {
                Background = warning.Blocks ? KpiTheme.ErrorFill : KpiTheme.WarningFill,
                BorderBrush = warning.Blocks ? KpiTheme.ErrorBorder : KpiTheme.WarningBorder,
                BorderThickness = KpiTheme.Hairline,
                CornerRadius = KpiTheme.Rounded,
                Padding = KpiTheme.Box,
                Margin = KpiTheme.BoxRow,
                Child = inside,
                ToolTip = Tip(warning.Detail.Length > 0 ? warning.Detail : warning.Title)
            };
        }

        /// <summary>
        /// A tooltip that wraps, or none for nothing. The full sentences the pane used to show
        /// run to several lines and a tooltip that does not wrap runs off the screen.
        /// </summary>
        private static object Tip(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            return new ToolTip
            {
                Content = new TextBlock
                {
                    Text = text,
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = KpiTheme.TooltipWidth
                }
            };
        }

        /// <summary>
        /// A caption for a button, through the escape, so a name with an underscore shows as
        /// written rather than losing it to an access key.
        /// </summary>
        private static string Caption(string text)
        {
            return PaneLabel.Escaped(text);
        }

        /// <summary>
        /// The templates folder's workbooks, recognised once per file per folder, or none where
        /// no folder is set or it holds no .xlsx.
        /// </summary>
        private IReadOnlyList<RecognisedWorkbook> Listing()
        {
            string folder = TemplateFolder.Read();
            IReadOnlyList<string> paths = folder.Length == 0 ? new List<string>() : TemplateFolder.WorkbooksIn(folder);

            if (paths.Count == 0)
            {
                _templatesListed = TemplateListing.Nothing;
                return new List<RecognisedWorkbook>();
            }

            // A filled checklist keeps its template's first sheet, so the peek also reads the
            // cells the tool writes and the file is named as filled rather than offered.
            _templatesListed = _templatesListed.For(folder, paths, path =>
            {
                PeekedWorkbook peeked = PeekedWorkbook.Of(path);
                return RecognisedWorkbook.Recognise(
                    Path.GetFileName(path), peeked.SheetNames, peeked.Refusal, peeked.FirstSheetCells)
                    .Laid(peeked.Layout);
            });

            return Reviewed(_templatesListed.Workbooks);
        }

        /// <summary>
        /// The folder looked at as a whole: a file whose name the tool does not know is offered
        /// as a new template where its layout is a known one's, and used once Use is pressed or a
        /// components.csv row names it. A template made here for the first time goes into the
        /// table at once, so a row naming it places plots on this same draw.
        /// </summary>
        private IReadOnlyList<RecognisedWorkbook> Reviewed(IReadOnlyList<RecognisedWorkbook> listed)
        {
            int before = _madeTemplates.Count;
            List<string> used = _usedTemplates.Concat(ComponentTable.InForce.Rows.Select(row => row.Template)).ToList();
            IReadOnlyList<RecognisedWorkbook> reviewed = NewTemplates.Review(listed, used, _madeTemplates);

            if (_madeTemplates.Count != before) ComponentFiles.PutInForce(_madeTemplates.Values);
            return reviewed;
        }

        /// <summary>
        /// Keeps a viewer's position across the rebuild, by name, restored on the first layout
        /// pass because a viewer that has not measured its content clamps any offset to zero.
        /// </summary>
        private ScrollViewer Remembering(ScrollViewer view, string remembered)
        {
            double wasDown;
            double wasAcross;
            if (_scrolled.Wants(remembered, out wasDown, out wasAcross))
            {
                EventHandler once = null;
                once = (sender, e) =>
                {
                    view.LayoutUpdated -= once;
                    if (wasDown > 0.0) view.ScrollToVerticalOffset(wasDown);
                    if (wasAcross > 0.0) view.ScrollToHorizontalOffset(wasAcross);
                };
                view.LayoutUpdated += once;
            }

            view.ScrollChanged += (sender, e) => _scrolled.Note(remembered, view.VerticalOffset, view.HorizontalOffset);

            return view;
        }

        /// <summary>
        /// Opens a folder or a file with whatever Windows opens it with. It touches no
        /// document, so it needs no external event, and a path that will not open says so on
        /// the status line rather than throwing out of a click handler.
        /// </summary>
        private void Open(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                System.Diagnostics.Process.Start(path);
            }
            catch (Exception bad)
            {
                _told = StatusLine.NotOpened(path, bad.Message);
                PaintStatus();
            }
        }

        // ---------------------------------------------------------------- the ticks

        private void Ticked(PlotTicks next)
        {
            _ticks = next;
            Changed();
        }

        /// <summary>
        /// The ticks and the hand record moved together. **Nothing sets one without the other.**
        /// </summary>
        private void TickedByHand(PlotTicks next, HandTicks byHand)
        {
            _byHand = byHand ?? HandTicks.None;
            Ticked(next);
        }

        /// <summary>
        /// Anything that could move a number throws away the last run and the confirmation
        /// with it, so a confirmed pair of areas never carries over onto a different set of
        /// plots and a refusal never sits under a choice that has since changed.
        /// </summary>
        private void Changed()
        {
            _lastSet = null;
            _confirmedIdentical = false;
            _chosenRegions.Clear();
            _showingResults = false;
            _reportWhere = string.Empty;
            _told = null;
            Preselect();
            RedrawTemplates();
        }

        /// <summary>
        /// The component on the ticked plots preselects a template, off the measured table. A
        /// value the table does not hold, and plots that disagree, both leave the pick to the
        /// user with the reason on the Asset types tooltip, and a template already picked by
        /// hand is never moved.
        /// </summary>
        private void Preselect()
        {
            _whyThisTemplate = string.Empty;
            if (_facts == null || _ticks.Count == 0 || _picks.Count > 0) return;

            AgreedValue component = new AgreedValue(_ticks.Ticked
                .Select(plotId => new PlotText(plotId, _facts.ComponentOn(plotId))));

            TemplateChoice choice = TemplateForComponent.For(component, _ticks.Ticked, null);

            _whyThisTemplate = choice.Why;

            if (choice.NeedsAPick) return;

            RecognisedWorkbook offered = _templatesListed.Workbooks.FirstOrDefault(
                one => ReferenceEquals(one.Template, choice.Preselected));
            if (offered == null)
            {
                _whyThisTemplate = choice.Why
                    + " The templates folder holds no file this tool recognises as "
                    + choice.Preselected.Name + ", so nothing was ticked.";
                return;
            }

            _picks.Add(new WorkbookTick(offered, choice.Preselected));
        }

        /// <summary>
        /// **TICKING A TEMPLATE ROW TICKS THE PLOTS THAT WILL GO INTO IT.** A plot held off by
        /// hand stays off.
        /// </summary>
        private void TickItsPlots(WorkbookTick tick)
        {
            if (tick.SettledAs == null || _facts == null) return;

            _ticks = TickingATemplate.Ticked(_ticks, tick.SettledAs, ComponentOn, _byHand);
        }

        private string ComponentOn(string plotId)
        {
            return _facts == null ? string.Empty : _facts.ComponentOn(plotId);
        }

        /// <summary>
        /// This plot's PRX_Plot_UID2, off the same per plot read the dropdown tooltips draw
        /// from. It is the name of the plot's folder and of its file.
        /// </summary>
        private string Uid2On(string plotId)
        {
            if (_facts == null) return string.Empty;

            PlotParameterValue held = _facts.ReferenceValuesOn(plotId)
                .FirstOrDefault(one => string.Equals(one.Name, KpiNames.PlotUid2, StringComparison.Ordinal));

            return held == null ? string.Empty : held.Value;
        }

        /// <summary>
        /// Every ticked plot's folder and file as the press would build them. The plot rows, the
        /// step 4 boxes and the step 4 count all read this one lookup.
        /// </summary>
        private IReadOnlyList<PlotFiling> TickedFilings()
        {
            return PlotFilings.Of(OutputFolder.Read(), _ticks.Ticked, ComponentOn, Uid2On);
        }

        private TemplateSplit TheSplit()
        {
            return PlotsPerTemplate.Split(
                _ticks.Ticked, ComponentOn, Settled().Select(one => one.SettledAs));
        }

        private IReadOnlyList<WorkbookTick> Settled()
        {
            return _picks.Where(one => one.SettledAs != null).ToList();
        }

        private IReadOnlyList<KpiCreateRun> Held()
        {
            return _lastSet == null ? new List<KpiCreateRun>() : _lastSet.Runs;
        }

        private WorkbookTick TickFor(RecognisedWorkbook workbook)
        {
            return _picks.FirstOrDefault(one =>
                string.Equals(one.Workbook.FileName, workbook.FileName, StringComparison.Ordinal));
        }

        /// <summary>
        /// Ticking a workbook row adds it with the template it recognised as, or with none where
        /// the file is caught between the two park templates. **Unticking a template unticks its
        /// plots**, because the row is the grouping button.
        /// </summary>
        private void ToggledWorkbook(RecognisedWorkbook workbook)
        {
            WorkbookTick already = TickFor(workbook);
            if (already != null)
            {
                _picks.Remove(already);

                if (already.SettledAs != null)
                {
                    _ticks = TickingATemplate.Unticked(_ticks, already.SettledAs, ComponentOn, _byHand);
                }
            }
            else
            {
                _picks.Add(new WorkbookTick(workbook, workbook.Template));
                TickItsPlots(_picks[_picks.Count - 1]);
            }

            // The preselection line describes what the preselection did, so a tick by hand is
            // what makes it untrue whichever way it read.
            _whyThisTemplate = string.Empty;

            // **AND SO ARE THE RESULTS OF THE PRESS BEFORE IT**, which this moves the ticks
            // under without going through Changed.
            _showingResults = false;
            _told = null;
            RedrawTemplates();
        }

        private ChosenParameters Chosen()
        {
            return new ChosenParameters(_componentParameter, _referenceParameter, _locationParameter);
        }

        // ---------------------------------------------------------------- the press

        /// <summary>
        /// The model state is not tested here. It is read off the live document inside the
        /// handler, because the pane's copy of it is what went stale.
        /// </summary>
        private void AskedToCreate()
        {
            if (Settled().Count == 0) return;

            RememberedNames.Remember(_preparedBy.Text, _position.Text);

            _pendingBefore = WhatThePaneSaid();
            _pressing = true;

            string folder = TemplateFolder.Read();
            var components = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string plotId in _ticks.Ticked)
            {
                components[plotId] = _facts == null ? string.Empty : _facts.ComponentOn(plotId);
            }

            _handler.Asked = new KpiCreateAsk(
                _ticks.Ticked,
                Settled().Select(one => new TemplatePick(
                    one.SettledAs,
                    Path.Combine(folder, one.Workbook.FileName))),
                _componentParameter,
                _referenceParameter,
                _locationParameter,
                _confirmedIdentical,
                _chosenRegions,
                _date.Text,
                _preparedBy.Text,
                _position.Text,
                Held(),
                _templatesListed,
                components,
                _pendingBefore);

            _told = StatusLine.Creating;
            PaintStatus();

            // **OPENED HERE AND NOT INSIDE EXECUTE.** This is a click handler on the pane's
            // own thread, so the window is up before the external event is raised.
            Shut();
            _running = KpiProgressWindow.Opened(ProgressWords.ReadingTheModel);

            Ask(KpiRequest.Create);
        }

        /// <summary>
        /// Everything the pane would have said in full before this press, for the report's
        /// BEFORE THE PRESS: the folders and files with what each means, the parameters, and
        /// the whole text of every warning box the four steps would draw now.
        /// </summary>
        private BeforeThePress WhatThePaneSaid()
        {
            string street = StreetReferenceFileSetting.Read();
            string listFile = PlotListFileSetting.Read();
            PlotListRead list = PlotListFile.In(listFile);
            string output = OutputFolder.Read();

            var warnings = new List<PaneWarning>();
            if (_facts != null) warnings.AddRange(PaneWarnings.ForRead(_facts.Plots, _facts.Links, list));
            warnings.Add(PaneWarnings.ListTick(_listTick, Settled().Select(one => one.Workbook.FileName)));
            warnings.Add(PaneWarnings.ForOutputFolder(output, TemplateFolder.Read()));
            warnings.AddRange(CreateWarnings(TheSplit()));
            if (!_confirmedIdentical)
            {
                warnings.Add(PaneWarnings.SameArea(Held().SelectMany(one => one.Reconciliation.IdenticalAreas).ToList()));
            }

            var fills = new List<string>();
            foreach (WorkbookTick tick in Settled())
            {
                fills.Add(tick.SettledAs.Name + ", from " + tick.Workbook.FileName + ":");
                fills.AddRange(TemplateWords.WouldFill(tick.SettledAs, Chosen()).Select(one => "  " + one));
            }

            return new BeforeThePress(
                TemplateFolder.Read(),
                output,
                FormsFolder.Read(),
                street,
                KpiPaneWords.StreetReferenceSays(street, StreetReferenceFile.In(street)),
                listFile,
                KpiPaneWords.PlotListSays(listFile, list),
                _componentParameter,
                _referenceParameter,
                _locationParameter,
                _read.Happened ? KpiHeader.ReadIn(_read) : KpiHeader.NothingReadYet,
                _whyThisTemplate,
                fills,
                warnings,
                null);
        }

        private void Found(KpiPlotFacts facts)
        {
            Dispatcher.Invoke(() =>
            {
                _facts = facts;
                _reading = false;
                _told = null;
                _ticks = new PlotTicks(facts.Plots, _ticks.Ticked);

                // **A ROW TICKED BEFORE THE READ GETS ITS PLOTS WHEN THE READ LANDS.**
                foreach (WorkbookTick tick in Settled()) TickItsPlots(tick);

                // **THE PLOTS READ IS NOT A READ OF THE MODEL** and the report's BEFORE THE
                // PRESS says which of the two produced the count.
                _read = ReadOfTheModel.ThePlots(DateTime.Now, facts.ElementInstances, facts.ReadSeconds);

                if (_componentParameter.Length == 0)
                {
                    _componentParameter = Preselected.From(facts.ComponentNames, KpiNames.Component);
                }

                if (_referenceParameter.Length == 0)
                {
                    _referenceParameter = Preselected.From(
                        KpiNames.PlotNamesOnSheets.ToList(), KpiNames.PlotUid2);
                }

                if (_locationParameter.Length == 0)
                {
                    _locationParameter = Preselected.From(facts.LocationNames, KpiNames.NeighbourhoodName);
                }

                // **THE READ WAS A PRESS, SO THE PANE MOVES TO WHAT IT WAS FOR.** Nothing else
                // moves the step by itself except a finished press.
                _stepOpen = KpiStep.Plots;

                RedrawTemplates();
            });
        }

        private void Made(KpiCreateRunSet set, string reportWhere)
        {
            Dispatcher.Invoke(() =>
            {
                _lastSet = set;
                _reportWhere = reportWhere ?? string.Empty;
                if (_reportWhere.Length > 0) _lastReport = _reportWhere;
                _lastBefore = _pendingBefore;
                _pressing = false;

                // **AFTER A PRESS THE PANE SHOWS THE RESULT RATHER THAN SENDING HIM TO THE
                // REPORT FILE.** Step 4's body gives way to it and every step shows a tick.
                _showingResults = true;
                _stepOpen = KpiStep.Create;

                // A press that reached the patcher with an output path in the templates folder
                // lists that folder afresh, because the copy can land there before a refusal.
                foreach (KpiCreateRun run in set.Runs)
                {
                    if (run.Outcome == null || run.OutputPath.Length == 0) continue;
                    if (!_templatesListed.IsFor(Path.GetDirectoryName(run.OutputPath))) continue;

                    _templatesListed = TemplateListing.Nothing;
                    break;
                }

                RedrawTemplates();
            });
        }

        // ---------------------------------------------------------------- browsing

        private void BrowseForTheFolder()
        {
            using (var picking = new System.Windows.Forms.FolderBrowserDialog())
            {
                picking.Description = "The folder holding the GRP KPI Checklist templates";
                string already = TemplateFolder.Read();
                if (already.Length > 0) picking.SelectedPath = already;

                if (picking.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                Remembered(TemplateFolder.Remember(picking.SelectedPath), TemplateFolder.PointerFileName);

                _picks.Clear();
                _whyThisTemplate = string.Empty;
                _templatesListed = TemplateListing.Nothing;

                // Another folder is another set of workbook rows, so a list tick recorded
                // against the old ones would report a drift nobody caused.
                _listTick = ListTickFreshness.NeverPressed;
                RedrawTemplates();
            }
        }

        private void BrowseForTheOutputFolder()
        {
            using (var picking = new System.Windows.Forms.FolderBrowserDialog())
            {
                picking.Description = "The folder the filled GRP KPI Checklist workbooks go to";
                string already = OutputFolder.Read();
                if (already.Length > 0) picking.SelectedPath = already;

                if (picking.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                Remembered(OutputFolder.Remember(picking.SelectedPath), OutputFolder.PointerFileName);
                RedrawTemplates();
            }
        }

        private void BrowseForTheFormsFolder()
        {
            using (var picking = new System.Windows.Forms.FolderBrowserDialog())
            {
                picking.Description = "The folder holding the Projects Basic Data forms";
                string already = FormsFolder.Read();
                if (already.Length > 0) picking.SelectedPath = already;

                if (picking.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                Remembered(FormsFolder.Remember(picking.SelectedPath), FormsFolder.PointerFileName);
                RedrawTemplates();
            }
        }

        private void BrowseForTheStreetReferenceFile()
        {
            using (var picking = new System.Windows.Forms.OpenFileDialog())
            {
                picking.Title = "The team's Scope_Validation workbook";
                picking.Filter = "Excel workbook (*.xlsx)|*.xlsx";
                picking.CheckFileExists = true;

                string already = StreetReferenceFileSetting.Read();
                if (already.Length > 0) picking.FileName = already;

                if (picking.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                Remembered(StreetReferenceFileSetting.Remember(picking.FileName), StreetReferenceFileSetting.PointerFileName);
                RedrawTemplates();
            }
        }

        private void BrowseForThePlotListFile()
        {
            using (var picking = new System.Windows.Forms.OpenFileDialog())
            {
                picking.Title = "The team's plot list, one plot per line";
                picking.Filter = "Text file (*.txt)|*.txt|Every file (*.*)|*.*";
                picking.CheckFileExists = true;

                string already = PlotListFileSetting.Read();
                if (already.Length > 0) picking.FileName = already;

                if (picking.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                Remembered(PlotListFileSetting.Remember(picking.FileName), PlotListFileSetting.PointerFileName);
                RedrawTemplates();
            }
        }

        /// <summary>
        /// A choice the pointer file refused is said on the status line, because the pane would
        /// otherwise read it back as never chosen.
        /// </summary>
        private void Remembered(bool written, string pointerFileName)
        {
            _told = written ? null : StatusLine.NotRemembered(pointerFileName);
        }

        /// <summary>
        /// Takes a control out of whatever held it before it goes somewhere new. The body is
        /// rebuilt on every change and these boxes outlive it, and WPF refuses an element with
        /// two logical parents outright.
        /// </summary>
        private static UIElement Reparented(UIElement what)
        {
            var was = LogicalTreeHelper.GetParent(what) as DependencyObject;

            var panel = was as Panel;
            if (panel != null) panel.Children.Remove(what);

            var holder = was as ContentControl;
            if (holder != null) holder.Content = null;

            var decorator = was as Decorator;
            if (decorator != null) decorator.Child = null;

            return what;
        }
    }
}
