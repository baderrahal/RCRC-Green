using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Autodesk.Revit.UI;
using RcrcGreen.Core.Kpi;

// Autodesk.Revit.UI carries a TextBox of its own for the ribbon. This pane is WPF, so the
// name is pinned to the one that belongs in a UserControl.
using TextBox = System.Windows.Controls.TextBox;

// The caption escape lives in Shared now, where both panels read it. An alias rather than
// a using of the whole Core namespace, so no other name in it can shadow a KPI one.
using PaneLabel = RcrcGreen.Core.PaneLabel;

namespace RcrcGreen.Revit.Kpi
{
    /// <summary>
    /// The KPI pane. The strip at the top carries the model name with when it was last read
    /// and nothing to press, because Create reads the model when it needs to and a scan is a
    /// step inside it. Under it sits one status line. Below that the template block: the templates folder, every workbook in it
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

        // **ONE BOX CANNOT NAME SIX FILES.** Every ticked workbook row carries its own name
        // box, held on its tick so it outlives every redraw of the template block with what
        // somebody is halfway through typing still in it. Each goes through Reparented on the
        // rebuild, the same rule the Drawing Sheet panel follows for its combo boxes.
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

        private PanelTheme _theme = PanelTheme.Current();

        // The title the read line and the status line describe. Null until a scan. The name
        // and the read line used to be set by two callbacks, so model B's name sat over model
        // A's read line after a document switch.
        private string _scannedTitle;

        // The open model as Revit last answered for it, title and folder together. ONE record,
        // set by one message and asked for again on every draw.
        //
        // THE PANE HOLDS NO COPY OF ANYTHING IT CAN ASK FOR. This was two loose strings, one set
        // by the plot read and one by the scan, and neither was re-read when the model was
        // saved. Create stayed grey saying no model was open on a model that had just been
        // given a folder, and a second scan did not shift it.
        private OpenModel _model = OpenModel.Nothing;

        // The title the plots were last asked for, so Took asks exactly once per model rather
        // than blind on every show. Null until the first ask and reset when the model closes,
        // so a model reopened is read again. This is the whole recovery from the one lost ask:
        // Ask(Plots) lived only in Shown, fired only by a visibility rise, so a pane shown with
        // no document open, or a scan pressed before the plot read returned, consumed the one
        // ask and nothing asked again, which left the plots block on open a model while a scan
        // filled the header.
        // **WHAT HAS BEEN READ, AND IT IS AN ABSENCE UNTIL SOMETHING READS.** The header used to
        // count the elements as soon as the pane was shown, and counting 96,959 of them IS the
        // read: a dockable pane is restored visible at Revit startup, so that fired on every
        // model anybody opened and held NG05 for minutes with nobody having asked for anything.
        private ReadOfTheModel _read = ReadOfTheModel.NotYet;

        // Which plots the user took off by hand. Ticking a template is a starting point rather
        // than a lock, so a plot held off here stays off when its template is ticked.
        private HandTicks _byHand = HandTicks.None;

        // The window a press of Create shows while it runs. Opened on this thread before the
        // external event is raised and closed when the run's last answer comes back, never
        // from inside Execute. Null whenever no press is running.
        private KpiProgressWindow _running;

        // **THE WORKBOOK ROWS ARE TICKABLE, SEVERAL AT ONCE.** One tick per row the user wants
        // a workbook from, each with the template it settled as and its own output name. For
        // most files the file and the template arrive together. A file caught between the two
        // park templates is ticked with no template until the user says which, and nothing is
        // guessed meanwhile.
        private readonly List<WorkbookTick> _picks = new List<WorkbookTick>();

        // Why the template list stands as it does: what preselected one, or what stopped
        // anything preselecting. Empty only when there is nothing to say yet. A value the table
        // does not hold used to leave the pane silent, which reads as a tool that never looked,
        // and a preselection that came off the plot prefix rather than off PRX_Component has to
        // say which route it took rather than arriving without a word.
        private string _whyThisTemplate = string.Empty;

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

        // What the last press of Create really did, ACROSS EVERY TEMPLATE, so the pane can
        // offer the region choices a refusal asked for without reading anything itself, and so
        // the next press can apply a choice to the readings it holds rather than reading the
        // model again. One set, holding one run per template that ran.
        private KpiCreateRunSet _lastSet;

        // The templates folder's workbooks as recognised, held once per folder. Every redraw
        // used to open and peek every .xlsx in the folder, seven zips for each of 155 ticks.
        // The one copy the pane holds under THE PANE HOLDS NO COPY OF ANYTHING IT CAN ASK
        // FOR: the folder is listed on every draw, only each file's recognition is kept, it is
        // keyed on the folder, and it is cleared when the folder changes and after any press
        // of Create that reached the patcher with an output path in it, wrote or not.
        private TemplateListing _templatesListed = TemplateListing.Nothing;

        // Where the plot list was scrolled to, kept here because the list is thrown away on
        // every redraw and every tick redraws. 155 tick boxes threw themselves back to the
        // top on every tick.
        private readonly ScrollMemory _scrolled = new ScrollMemory();

        // **WHICH OF THE FOUR STEPS IS BEING WORKED ON.** Null until somebody presses a cell
        // in the bar, and then it is theirs: the pane picks the first step worth working on
        // only while nobody has chosen. Forgotten when the model changes, because another
        // model's state is not this one's.
        private KpiStep? _stepOpen;

        // Whether Tick the list still stands, and which workbook rows were ticked when it was
        // pressed. **NOTHING TICKS OR UNTICKS BEHIND HIS BACK**: this only decides whether the
        // pane says the list tick is out of date and offers the button again.
        private ListTickFreshness _listTick = ListTickFreshness.NeverPressed;

        // Whether step 4 is showing what the last press did rather than what a press would do.
        // Cleared by anything that could move a number, the same as the run it describes.
        private bool _showingResults;

        // Where the last press wrote its report, so the results panel can open it. Empty until
        // a press has written one.
        private string _reportWhere = string.Empty;

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
                CreatedAcross = Made,
                Told = Say,
                Progressed = Moved
            };
            _asking = ExternalEvent.Create(_handler);

            Content = Layout();
            PaintFromTheTheme();

            TheHeader();
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

            // Ask only for the model. Took asks for the plots once it knows which model is
            // open, so the plots are asked for in one place off the answered title rather than
            // blind here. Asking Plots here as well lost one of the two to the handler's one
            // slot, and worse, it could not be told apart from Took's ask, so the model was
            // read for its plots twice on every show. A pane restored visible at startup with
            // no document open consumed this blind Plots ask against no document and nothing
            // ever asked again, which is how a scan later filled the header while the plots
            // block sat on open a model. Took now re-asks whenever it sees a model it has not
            // read plots for.
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

            // **NO SCAN BUTTON.** Create reads the model when it needs to, so a scan is a step
            // inside it rather than something the user has to know to do first. The strip
            // carries what the document is and nothing to press.
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

                // **A DIFFERENT MODEL HAS NOT BEEN READ**, whatever was read of the last one,
                // so everything that came off a read goes with it rather than sitting under a
                // name it does not describe.
                if (moved)
                {
                    _read = ReadOfTheModel.NotYet;
                    _facts = null;
                    _scannedTitle = null;

                    // The hand record is plot identifiers, and another model's DM-14 is not this
                    // one's. Holding it across would take a plot off a model nobody had touched.
                    _byHand = _byHand.Forgotten();

                    // And so are the step somebody had open, the list tick they had pressed and
                    // the results of a press made against the model that has gone.
                    _stepOpen = null;
                    _listTick = ListTickFreshness.NeverPressed;
                    _showingResults = false;
                    _reportWhere = string.Empty;

                    // **AND THE RUN ITSELF**, which is one model's plots read against one
                    // model's templates. Held across, its region questions and its refusals
                    // would be asked about a model nobody has open.
                    _lastSet = null;
                    _confirmedIdentical = false;
                    _chosenRegions.Clear();
                }

                TheHeader();

                if (!_model.IsOpen) _said.Text = KpiPaneWords.NoModel;

                // Only when the answer moved. Drawing asks for this again, so redrawing on
                // every answer would spin the external event for as long as the pane is open.
                //
                // **AND IT ASKS FOR NOTHING ELSE.** The plots used to be asked for here, off
                // the answered title, which meant opening a model started a read with no press
                // behind it. Nothing heavy runs without a press now: the plots are read by the
                // press in the plots block, and Create reads what it needs itself.
                if (moved) RedrawTemplates();
            });
        }

        /// <summary>
        /// The two lines at the top, decided in Core and drawn here. **Only a model something
        /// has read names a count**, and a model nothing has read says so rather than saying
        /// nought, because nought is a number and this is an absence.
        /// </summary>
        private void TheHeader()
        {
            IReadOnlyList<string> lines = KpiHeader.Lines(_model, _read);
            _modelName.Text = lines[0];
            _readAt.Text = lines[1];
        }

        private void Scanned(KpiScan scan, DateTime readAt)
        {
            Dispatcher.Invoke(() =>
            {
                // The scan says which model it describes and nothing about the open one. That
                // comes from Took, which every answer from the handler now carries.
                _scannedTitle = scan.Document.Title;
                _read = ReadOfTheModel.TheWholeModel(
                    readAt, scan.Document.ElementInstances, scan.Document.ReadSeconds);
                TheHeader();
            });
        }

        private void Say(string what)
        {
            Dispatcher.Invoke(() =>
            {
                _said.Text = what ?? string.Empty;

                // Told is the run's end line whatever ended it, a finish, a refusal or a
                // throw, so the window goes with it and can never be left over a run that is
                // no longer running. The scan's own headline goes through Progressed for
                // exactly this reason: said here it would shut the window mid press.
                Shut();
            });
        }

        /// <summary>
        /// Closes the progress window if one is open and forgets it. Safe to call when none
        /// is, which is every press that never opened one.
        /// </summary>
        private void Shut()
        {
            KpiProgressWindow running = _running;
            _running = null;
            if (running != null) running.Done();
        }

        /// <summary>
        /// A progress line raised from inside a run on the Revit thread. Setting the text is
        /// not enough on its own: a dockable pane can share Revit's thread, and a line set mid
        /// run then sits unpainted until the run returns, which reads exactly like today's one
        /// unmoving line. Waiting on one empty job pumps everything queued at or above its
        /// priority, so the priority decides what gets in. Render is where layout and paint
        /// sit and is above input, so the paint goes through and every queued click stays
        /// queued until the run returns. Background sits below input, and pumping there would
        /// run click handlers inside the handler's Execute, where the two Browse buttons open
        /// a folder dialog owned by nobody and Revit's own ribbon stays live behind it.
        /// Whether the line visibly moves on a real pane is a fact about Revit's hosting that
        /// only a run can show, and the log records it as open.
        /// </summary>
        private void Moved(string what)
        {
            Dispatcher.Invoke(() =>
            {
                _said.Text = what ?? string.Empty;
                if (_running != null) _running.Moved(what);
            });
            Dispatcher.Invoke(new Action(() => { }), System.Windows.Threading.DispatcherPriority.Render);
        }

        /// <summary>
        /// The pane, drawn again from the state on every change. One path in and out of every
        /// change, the same rule as the Drawing Sheet grid.
        ///
        /// **FOUR STEPS, ONE AT A TIME.** Bader's layout of 17 September. The bar across the
        /// top says which step is which and which are done, and only the step being worked on
        /// shows its controls. Which step may be worked on is decided in
        /// <see cref="KpiSteps"/> and this draws the answer.
        /// </summary>
        private void RedrawTemplates()
        {
            _templates.Children.Clear();

            KpiSteps steps = Steps();
            KpiStep open = _stepOpen ?? steps.FirstUnfinished;

            _templates.Children.Add(TheStepBar(steps, open));

            KpiStepState state = steps.For(open);

            _templates.Children.Add(new TextBlock
            {
                Text = state.Number + "  " + state.Title
                    + (state.Summary.Length == 0 ? string.Empty : "   " + state.Summary),
                FontWeight = FontWeights.Bold,
                Margin = PanelMetrics.Heading,
                TextWrapping = TextWrapping.Wrap
            });

            // **A STEP THAT CANNOT BE WORKED ON SHOWS ITS REASON IN PLACE OF ITS CONTROLS.** A
            // cell that does nothing and says nothing is worse than one that is not there.
            if (!state.Usable)
            {
                _templates.Children.Add(Warned(state.WhyNot));
            }
            else
            {
                switch (open)
                {
                    case KpiStep.Setup: InsideSetup(); break;
                    case KpiStep.Read: InsideRead(); break;
                    case KpiStep.Tick: InsideTick(); break;
                    default: InsideCreate(); break;
                }
            }

            // Read at the moment the pane is drawn, never held. A model saved while the pane
            // sat open used to leave Create refusing on a folder read once and never again, and
            // a second scan did not shift it. The answer comes back through Took, which redraws
            // only when it moved, so this does not chase its own tail.
            Ask(KpiRequest.WhichModel);
        }

        /// <summary>
        /// The four steps, worked out in Core off what the pane can ask for at this moment.
        ///
        /// **EVERY VALUE HERE IS READ LIVE.** The five pointers are read off their own files
        /// and the model off the last answer, because the pane holds no copy of anything it can
        /// ask for, which is the rule one stale folder string already cost this tool.
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

        /// <summary>
        /// The bar across the top: four cells, the one being worked on marked and a done one
        /// carrying its word.
        ///
        /// **FOUR EQUAL CELLS RATHER THAN A ROW THAT WRAPS**, because a dockable pane on the
        /// right of Revit is about 300 pixels wide and this panel has shipped columns running
        /// off the right edge once already. Each cell trims rather than pushing its neighbours.
        /// </summary>
        private UIElement TheStepBar(KpiSteps steps, KpiStep open)
        {
            var bar = new UniformGrid { Columns = 4, Margin = PanelMetrics.Row };

            foreach (KpiStepState step in steps.All)
            {
                KpiStep which = step.Step;
                bool here = which == open;

                var inside = new StackPanel
                {
                    Opacity = step.Usable ? 1.0 : PanelMetrics.FadedOpacity
                };

                inside.Children.Add(new TextBlock
                {
                    Text = step.Cell,
                    FontWeight = here ? FontWeights.Bold : FontWeights.Normal,
                    FontSize = PanelMetrics.StepTitle,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    HorizontalAlignment = HorizontalAlignment.Center
                });

                // **THE MARK IS ITS OWN LINE UNDER THE NAME.** Inside the cell text it was the
                // first thing trimmed at about 70 pixels a cell, so a finished step read
                // shorter than an unfinished one, which is the opposite of what a mark does.
                inside.Children.Add(new TextBlock
                {
                    Text = step.Mark,
                    FontSize = PanelMetrics.StepTitle - 2.0,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    HorizontalAlignment = HorizontalAlignment.Center
                });

                var cell = new Button
                {
                    Content = inside,
                    Padding = PanelMetrics.CellPad,
                    Margin = PanelMetrics.Gap,
                    Background = here ? _theme.Primary : _theme.StepHeader,
                    Foreground = here ? _theme.OnPrimary : _theme.Foreground,
                    BorderBrush = _theme.Line,
                    BorderThickness = PanelMetrics.Outline,
                    ToolTip = step.Usable
                        ? "Work on step " + step.Number + ", " + step.Title + "."
                        : step.WhyNot
                };

                cell.Click += (sender, e) => { _stepOpen = which; RedrawTemplates(); };
                bar.Children.Add(cell);
            }

            return bar;
        }

        /// <summary>
        /// **STEP 1.** The five folders and files this pane is pointed at, each with its own
        /// Browse and its own line, the three the workbook carries that come from no model, and
        /// one line naming what is still not set.
        ///
        /// **EVERY ONE OF THE FIVE IS REACHABLE AT ALL TIMES NOW.** The block used to stop at
        /// the templates folder when none was set and at the workbook list when nothing was
        /// ticked, so the output folder, the forms folder, the street reference and the plot
        /// list could not be browsed for until a template had been picked.
        /// </summary>
        private void InsideSetup()
        {
            string folder = TemplateFolder.Read();

            _templates.Children.Add(BrowsedLine(
                "Templates", folder, "No folder set",
                "Point at the folder holding the GRP KPI Checklist template workbooks. "
                    + "It is remembered beside the installed add-in.",
                BrowseForTheFolder));

            if (folder.Length == 0) _templates.Children.Add(Faint(TemplateWords.NoFolder));

            TheOutputFolder();
            TheFormsFolder();
            TheStreetReferenceFile();
            ThePlotListFile();

            _templates.Children.Add(Head("What the workbook carries"));
            _templates.Children.Add(Boxed("Date", _date));
            _templates.Children.Add(Boxed("Prepared by", _preparedBy));
            _templates.Children.Add(Boxed("Position", _position));

            // **ONCE, AT THE FOOT.** The step header carries a count of what is not set and
            // this carries the naming, because the same sentence twice on one screen is what a
            // run already printed four lines twice over.
            _templates.Children.Add(Faint(KpiSteps.StillNotSet(KpiSteps.NotSet(
                TemplateFolder.Read().Length > 0,
                OutputFolder.Read().Length > 0,
                FormsFolder.Read().Length > 0,
                StreetReferenceFileSetting.Read().Length > 0,
                PlotListFileSetting.Read().Length > 0))));
        }

        /// <summary>
        /// **STEP 2.** The one press that starts a read apart from Create, the counts it
        /// produces, and the named lines that come with it.
        /// </summary>
        private void InsideRead()
        {
            TheReadButton();

            PlotListRead sent = PlotListFile.In(PlotListFileSetting.Read());
            if (sent.Set)
            {
                _templates.Children.Add(sent.Read
                    ? Faint(TickingTheList.Heading + " " + sent.InWords)
                    : Warned(TickingTheList.Heading + " " + sent.Why));
            }

            // **A NOTE AND NEVER A REFUSAL, AND IT IS KNOWN AS SOON AS THE MODEL IS READ.** The
            // first STREETS run spent 78 plots finding nothing because not one of six link
            // instances was loaded. It is said here, where the read that learnt it is, and
            // again above Create, where it has always been.
            if (_facts != null && _facts.Links.Worth)
            {
                _templates.Children.Add(Noted(_facts.Links.OnThePane));
            }
        }

        /// <summary>
        /// **STEP 3.** The workbook rows, then Tick the list under them so the order reads the
        /// way it has to be done, then what is ticked and what that costs.
        /// </summary>
        private void InsideTick()
        {
            TheWorkbookRows();
            ThePlotTicks();
            TheTickLines();
            TheChoices();
        }

        /// <summary>
        /// The templates folder's workbooks, one tickable row each, exactly as they were drawn
        /// before the pane worked in steps. **The folder itself is step 1's**, so this says
        /// which step to go to rather than offering a second Browse for one folder.
        /// </summary>
        private void TheWorkbookRows()
        {
            string folder = TemplateFolder.Read();

            if (folder.Length == 0)
            {
                _templatesListed = TemplateListing.Nothing;
                _templates.Children.Add(Faint(TemplateWords.NoFolder));
                return;
            }

            _templates.Children.Add(new TextBlock
            {
                Text = "GRP KPI Checklist templates",
                FontWeight = FontWeights.Bold,
                Margin = PanelMetrics.Row
            });

            IReadOnlyList<string> paths = TemplateFolder.WorkbooksIn(folder);
            if (paths.Count == 0)
            {
                _templatesListed = TemplateListing.Nothing;
                _templates.Children.Add(Faint(TemplateWords.EmptyFolder));
                return;
            }

            // Opened once per file per folder. A filled checklist keeps its template's first
            // sheet, so the peek also reads the cells the tool writes and the file is named as
            // filled rather than offered.
            _templatesListed = _templatesListed.For(folder, paths, path =>
            {
                PeekedWorkbook peeked = PeekedWorkbook.Of(path);
                return RecognisedWorkbook.Recognise(
                    Path.GetFileName(path), peeked.SheetNames, peeked.Refusal, peeked.FirstSheetCells);
            });
            IReadOnlyList<RecognisedWorkbook> recognised = _templatesListed.Workbooks;

            _templates.Children.Add(Faint(TemplateWords.Listed(
                recognised.Count, recognised.Count(one => one.IsMatched))));
            _templates.Children.Add(Faint(_templatesListed.InWords));

            // Said above the list, because it is the reason the rows below stand as they do.
            if (_whyThisTemplate.Length > 0) _templates.Children.Add(Faint(_whyThisTemplate));

            foreach (RecognisedWorkbook workbook in recognised)
            {
                RecognisedWorkbook which = workbook;
                bool tickable = which.IsMatched || which.NeedsAPick;
                var box = new CheckBox
                {
                    Content = PaneLabel.Escaped(which.FileName + "   " + which.InWords),
                    Margin = PanelMetrics.Row,
                    IsEnabled = tickable,
                    IsChecked = TickFor(which) != null,
                    ToolTip = tickable ? "Tick this workbook. Several can be ticked at once." : which.Reason
                };
                box.Click += (sender, e) => ToggledWorkbook(which);
                _templates.Children.Add(box);

                WorkbookTick tick = TickFor(which);

                // **THE LINE THAT SAYS WHY A PLOT OF THIS TEMPLATE IS NOT GOING IN.** It was
                // built and shown nowhere for every round since it was written, and the whole
                // of the eighty second pass went by asking a 57,143 line report the question
                // this answers on the row a person is looking at when they press.
                if (tick != null && tick.SettledAs != null) _templates.Children.Add(HeldOffOn(tick));

                if (tick == null || tick.SettledAs != null) continue;

                // Ticked and still waiting on which of the two park templates it is. Nothing
                // is guessed, and the row under it is where the answer goes.
                _templates.Children.Add(Faint("   " + TemplateWords.PickBetween(which)));
                var either = new StackPanel { Orientation = Orientation.Horizontal, Margin = PanelMetrics.Row };
                foreach (KpiTemplate candidate in which.Candidates)
                {
                    KpiTemplate chosen = candidate;
                    WorkbookTick waiting = tick;
                    var choice = new Button
                    {
                        Content = PaneLabel.Escaped(chosen.Name),
                        Padding = PanelMetrics.CellPad,
                        Margin = PanelMetrics.Gap
                    };
                    choice.Click += (sender, e) =>
                    {
                        waiting.SettledAs = chosen;
                        TickItsPlots(waiting);
                        _showingResults = false;
                        RedrawTemplates();
                    };
                    either.Children.Add(choice);
                }
                _templates.Children.Add(either);
            }

            if (Settled().Count == 0) return;

            // **ONE ROW PER TICKED TEMPLATE, each with its own name, each editable on its own.**
            // Where exactly one is ticked the box behaves exactly as it did before.
            foreach (WorkbookTick tick in Settled())
            {
                _templates.Children.Add(new TextBlock
                {
                    Text = "What " + tick.SettledAs.Name + " would fill",
                    FontWeight = FontWeights.Bold,
                    Margin = PanelMetrics.Row
                });

                foreach (string line in TemplateWords.WouldFill(tick.SettledAs, Chosen()))
                {
                    _templates.Children.Add(new TextBlock
                    {
                        Text = line,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = PanelMetrics.Row
                    });
                }

            }
        }

        /// <summary>
        /// **STEP 4.** What a press would write, every named line that would stop or change a
        /// plot, and the button. After a press this step shows the results panel instead.
        /// </summary>
        private void InsideCreate()
        {
            if (_showingResults && _lastSet != null)
            {
                TheResults();
                return;
            }

            _templates.Children.Add(Faint(KpiWillWrite.InWords(TheSplit())));

            TheCreateButton();
        }

        /// <summary>
        /// Where the filled workbook goes, browsed for and remembered beside the installed
        /// add-in, the same way the template folder is.
        ///
        /// **It used to be the model's own folder**, so a detached model could not be used at
        /// all and Create sat grey saying the model had never been saved. It is read here at
        /// the moment the pane draws rather than held, and the handler reads it again at the
        /// moment Create is pressed, which is what decides the refusal.
        /// </summary>
        private void TheOutputFolder()
        {
            string folder = OutputFolder.Read();

            _templates.Children.Add(BrowsedLine(
                "Output folder", folder, "No folder set",
                "Point at the folder the filled workbooks should be written to. "
                    + "It is remembered beside the installed add-in.",
                BrowseForTheOutputFolder));

            // Said before Create is pressed rather than after. The per file guard refuses the
            // press that would write over a template, and this is what stops the user reaching
            // it: the name box is prefilled with the template's own file name, so these two
            // folders being one is the whole of the distance to that press.
            if (FilePaths.Compare(folder, TemplateFolder.Read()) == SamePath.Same)
            {
                _templates.Children.Add(Warned(TemplateWords.OutputIsTheTemplateFolder));
            }

            _templates.Children.Add(Faint(TemplateWords.Output(folder)));

            // **In place of the name box, which had gone stale.** It still read
            // GRP-KPI-Checklist-DD-MOSQUES.xlsx when every workbook is named from its plot's
            // own UID2. Said once here rather than once per ticked row, because the shape is
            // the same for every template.
            _templates.Children.Add(Faint(CreateWords.WhereTheWorkbooksGo(folder)));
        }

        /// <summary>
        /// The FOURTH browsed thing, beside the templates folder, the output root and the street
        /// reference file. Bader's decision.
        ///
        /// **It is a note and never a refusal.** A press with none set writes every workbook and
        /// no PDF, so this line greys nothing out.
        /// </summary>
        private void TheFormsFolder()
        {
            string folder = FormsFolder.Read();

            _templates.Children.Add(BrowsedLine(
                "Forms folder", folder, "No folder set",
                "Point at the folder holding the three Projects Basic Data forms. Each "
                    + "plot gets one PDF beside its workbook, on the form its plot prefix names. "
                    + "It is remembered beside the installed add-in.",
                BrowseForTheFormsFolder));

            _templates.Children.Add(folder.Length == 0
                ? Noted(TemplateWords.FormsFolder(folder))
                : Faint(TemplateWords.FormsFolder(folder)));
        }

        private void BrowseForTheFormsFolder()
        {
            using (var picking = new System.Windows.Forms.FolderBrowserDialog())
            {
                picking.Description = "The folder holding the Projects Basic Data forms";

                string already = FormsFolder.Read();
                if (already.Length > 0) picking.SelectedPath = already;

                if (picking.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                if (!FormsFolder.Remember(picking.SelectedPath))
                {
                    Say("The folder could not be remembered. " + FormsFolder.PointerFileName
                        + " beside the installed add-in refused the write.");
                }

                RedrawTemplates();
            }
        }

        /// <summary>
        /// The third browsed thing, beside the templates folder and the output root. It is a
        /// FILE rather than a folder, so it has its own pointer and its own existence check.
        ///
        /// **It is a note and never a refusal.** A run with none set writes every street plot's
        /// road width and total length empty and says so, the same way the link note works, so
        /// this line never greys anything out.
        /// </summary>
        private void TheStreetReferenceFile()
        {
            string file = StreetReferenceFileSetting.Read();
            string remembered = StreetReferenceFileSetting.ReadRaw();

            _templates.Children.Add(BrowsedLine(
                "Street reference", file, "No file set",
                "Point at the team's Scope_Validation workbook. It fills the road width and the "
                    + "total length on STREETS plots, matched on " + KpiNames.PlotUid2
                    + ". It is remembered beside the installed add-in.",
                BrowseForTheStreetReferenceFile));

            // A remembered file that has gone reads as none set unless this says otherwise, and
            // a person would go looking for a Browse they had already pressed.
            if (file.Length == 0 && remembered.Length > 0)
            {
                _templates.Children.Add(Warned(
                    "The remembered street reference file is not there any more: " + remembered));
            }

            _templates.Children.Add(Faint(StreetReferenceFile.In(file).InWords));
        }

        /// <summary>
        /// **THE TEAM SENT 154 PLOTS TO EXPORT AND WANTS EVERY ONE EXPORTED WITH NONE SKIPPED.**
        /// This points at their own list, one plot per line, and Tick the list in the plot block
        /// below presses it onto the ticks.
        ///
        /// **THE FILE NEVER ENTERS THE REPOSITORY**, only the pointer beside the installed
        /// assembly, the same as the street reference file.
        ///
        /// **It is a note and never a refusal.** A press with none set ticks plots the way it
        /// always did, so this greys nothing out.
        /// </summary>
        private void ThePlotListFile()
        {
            string file = PlotListFileSetting.Read();
            string remembered = PlotListFileSetting.ReadRaw();

            _templates.Children.Add(BrowsedLine(
                "Plot list", file, "No file set",
                "Point at the team's plot list, a plain text file with one plot per line. Tick "
                    + "the list, under the plots below, replaces every tick with exactly the "
                    + "plots it names. It is remembered beside the installed add-in.",
                BrowseForThePlotListFile));

            // A remembered file that has gone reads as none set unless this says otherwise, the
            // same line the street reference file already carries.
            if (file.Length == 0 && remembered.Length > 0)
            {
                _templates.Children.Add(Warned(
                    "The remembered plot list file is not there any more: " + remembered));
            }

            PlotListRead read = PlotListFile.In(file);
            if (!read.Set) return;

            _templates.Children.Add(read.Read
                ? Faint(TickingTheList.Heading + " " + read.InWords)
                : Warned(TickingTheList.Heading + " " + read.Why));
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

                if (!PlotListFileSetting.Remember(picking.FileName))
                {
                    Say("The file could not be remembered. " + PlotListFileSetting.PointerFileName
                        + " beside the installed add-in refused the write.");
                }

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

                if (!StreetReferenceFileSetting.Remember(picking.FileName))
                {
                    Say("The file could not be remembered. "
                        + StreetReferenceFileSetting.PointerFileName
                        + " beside the installed add-in refused the write.");
                }

                RedrawTemplates();
            }
        }

        /// <summary>
        /// **STEP 2'S OWN PRESS.** What the model holds, in the four states the block reads,
        /// and the one button that starts a read apart from Create.
        /// </summary>
        private void TheReadButton()
        {
            _templates.Children.Add(Head(CreateWords.Heading));

            // Open a model is only right when no model is open. The block used to say it on
            // any facts == null, so a scan that filled the header left this reading open a
            // model beside the model's own name, and the team went to Revit for an hour. The
            // four states each read differently, told apart by the live document and whether
            // the plots have come back, never by a held copy.
            foreach (string line in CreateWords.PlotsBlock(_model.IsOpen, _facts == null ? null : _facts.Plots))
            {
                _templates.Children.Add(Faint(line));
            }

            // **THE ONE PRESS THAT STARTS A READ APART FROM CREATE.** It used to start itself
            // when the pane was shown, and a dockable pane is restored visible at Revit
            // startup, so every model anybody opened was read with nobody having asked.
            if (_model.IsOpen && _facts == null)
            {
                var read = new Button
                {
                    Content = PaneLabel.Escaped(CreateWords.ReadThisModel),
                    Padding = PanelMetrics.CellPad,
                    Margin = PanelMetrics.Row,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                read.Click += (sender, e) =>
                {
                    Say(CreateWords.ReadingNow);
                    Ask(KpiRequest.Plots);
                };
                _templates.Children.Add(read);
            }
        }

        /// <summary>
        /// The plot picker. Every plot the model holds, ticked one at a time, in a run, or all
        /// at once, with the count showing at all times. There is no free text box: a plot the
        /// model does not hold cannot be chosen, the same rule the Drawing Sheet follows.
        ///
        /// **TICK THE LIST SITS UNDER THE WORKBOOK ROWS**, because ticking a row ticks its
        /// plots and the list tick has to be the last one. The line saying it has gone out of
        /// date is drawn by TheTickLines under this.
        /// </summary>
        private void ThePlotTicks()
        {
            if (_facts == null || _facts.Plots.All.Count == 0) return;

            _templates.Children.Add(Head(CreateWords.Heading));

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = PanelMetrics.Row };
            var all = new Button { Content = PaneLabel.Escaped(CreateWords.SelectAll), Padding = PanelMetrics.CellPad, Margin = PanelMetrics.Gap };
            // **SELECT ALL AND CLEAR BOTH FORGET EVERY HAND CHOICE.** They replace every tick,
            // so a per plot choice left standing behind them is a record that disagrees with
            // what is on screen, and the next template row tick acts on the disagreement. A
            // person pressing Select all is saying they want everything and a person pressing
            // Clear is starting over.
            all.Click += (sender, e) => ReplacedEveryTick(_ticks.All());
            var none = new Button { Content = PaneLabel.Escaped(CreateWords.Clear), Padding = PanelMetrics.CellPad, Margin = PanelMetrics.Gap };
            none.Click += (sender, e) => ReplacedEveryTick(_ticks.None());
            buttons.Children.Add(all);
            buttons.Children.Add(none);

            // **TICK THE LIST REPLACES EVERY TICK AND FORGETS EVERY HAND CHOICE**, exactly as
            // Select all and Clear beside it do, because a person pressing it is saying these are
            // the plots. The rule is TickingTheList in Core, where the tests reach the copy that
            // runs, and this button only presses it.
            PlotListRead sent = PlotListFile.In(PlotListFileSetting.Read());
            if (sent.Set)
            {
                var listed = new Button
                {
                    Content = PaneLabel.Escaped(CreateWords.TickTheList),
                    Padding = PanelMetrics.CellPad,
                    Margin = PanelMetrics.Gap,
                    IsEnabled = sent.Read,
                    ToolTip = sent.Read
                        ? "Replace every tick with exactly the plots the plot list file names, "
                            + "and forget every plot ticked or unticked by hand."
                        : sent.Why
                };
                listed.Click += (sender, e) => PressTickTheList();
                buttons.Children.Add(listed);
            }

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
                box.Click += (sender, e) =>
                {
                    // **A PLOT TICKED OR UNTICKED BY HAND WINS, IN BOTH DIRECTIONS.** Ticking a
                    // template is a starting point rather than a lock, so the choice is
                    // remembered and a later row press leaves it alone whichever way it went.
                    TickedByHand(
                        _ticks.Toggled(which),
                        _ticks.IsTicked(which) ? _byHand.TakenOff(which) : _byHand.PutOn(which));
                };
                list.Children.Add(box);
            }

            _templates.Children.Add(Scrolling(list, PanelMetrics.ListHeight, "plots"));
        }

        /// <summary>
        /// Select all and Clear. **THEY REPLACE EVERY TICK, WHICH IS WHAT TICK THE LIST DOES**,
        /// so they forget the list tick as well as every hand choice. Leaving it standing had
        /// the pane say the list tick still held over ticks it no longer describes, which is a
        /// record that disagrees with the screen, and saying it had gone OUT OF DATE would be
        /// wrong too: a person pressing Select all is saying these are the plots.
        /// </summary>
        private void ReplacedEveryTick(PlotTicks next)
        {
            _listTick = ListTickFreshness.NeverPressed;
            TickedByHand(next, _byHand.Forgotten());
        }

        /// <summary>
        /// Tick the list, from either of the two buttons that offer it.
        ///
        /// **THE ROWS ARE RECORDED OFF WHAT THE PRESS LEAVES, NOT WHAT IT STARTED FROM.**
        /// `TickedByHand` reaches `Changed`, which reaches `Preselect`, which can tick a
        /// workbook row of its own, so recording before the tick made the press raise its own
        /// out of date warning the moment it finished.
        /// </summary>
        private void PressTickTheList()
        {
            TickedByHand(
                TickingTheList.Ticked(_ticks, PlotListFile.In(PlotListFileSetting.Read())),
                _byHand.Forgotten());

            _listTick = ListTickFreshness.Pressed(
                Settled().Select(one => one.Workbook.FileName));

            RedrawTemplates();
        }

        /// <summary>
        /// A list in a viewer that keeps its place across the rebuild, the shape the Drawing
        /// Sheet settled on for the same fault on its own lists. Written here rather than
        /// called across the fence, because that helper is another task's, and a shared one
        /// cannot live in Core, which has no WPF.
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
        /// Keeps a viewer's position across the rebuild, by name. The remembered offsets are
        /// read into locals before any handler is attached, because the fresh viewer's own
        /// first scroll change would note nought over them. They are restored on the first
        /// layout pass rather than on Loaded, because a viewer that has not measured its
        /// content yet clamps any offset to zero and the restore reads as though it worked.
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

            _templates.Children.Add(Picker(
                "Reference", KpiNames.PlotNamesOnSheets.ToList(), _referenceParameter,
                chosen => { _referenceParameter = chosen; Changed(); }));

            TheReferenceValues();

            _templates.Children.Add(Picker(
                "Location", _facts.LocationNames, _locationParameter,
                chosen => { _locationParameter = chosen; Changed(); }));

            _templates.Children.Add(Boxed("Date", _date));
            _templates.Children.Add(Boxed("Prepared by", _preparedBy));
            _templates.Children.Add(Boxed("Position", _position));
        }

        /// <summary>
        /// What the four plot parameters hold ON THE FIRST TICKED PLOT, with that plot named.
        ///
        /// It used to print the first plot in the model's list whatever was ticked, so DM-12
        /// ticked showed DM-11's four values. The block exists so a person picks the reference
        /// by looking at its value, and a value belonging to a plot they did not choose is
        /// worse than no value at all. With nothing ticked it says so and shows none.
        /// </summary>
        private void TheReferenceValues()
        {
            if (_ticks.Count == 0)
            {
                _templates.Children.Add(Faint("   " + CreateWords.NoPlotForTheReferenceValues));
                return;
            }

            string plotId = _ticks.Ticked[0];
            _templates.Children.Add(Faint("   " + CreateWords.ReferenceValuesOn(plotId)));

            foreach (PlotParameterValue value in _facts.ReferenceValuesOn(plotId))
            {
                _templates.Children.Add(Faint("      " + value.InWords));
            }
        }

        /// <summary>
        /// Create, and everything standing between the pane and pressing it. A refusal lists
        /// every missing thing at once rather than one per press.
        /// </summary>
        private void TheCreateButton()
        {
            _templates.Children.Add(Head(CreateWords.Create));

            // The folder is read here rather than held, the same as everywhere else it is read.
            // What really decides the refusal is the handler's own read at the moment the button
            // is pressed: this line is what the pane can say before then.
            string cannot = CreateWords.CannotCreate(
                _model, OutputFolder.Read(), Settled().Count > 0, _ticks.Count > 0);

            if (cannot.Length > 0) _templates.Children.Add(Faint(cannot));

            // **ONE ROW PER TICKED TEMPLATE, BEFORE THE PRESS.** Which plots each workbook will
            // get, and for one no ticked plot belongs to, why it will write nothing. Bader's
            // decision: it stays tickable and stays listed.
            TemplateSplit split = TheSplit();
            IReadOnlyList<string> rows = KpiWillWrite.Rows(split);
            for (int at = 0; at < split.Shares.Count; at++)
            {
                _templates.Children.Add(split.Shares[at].WillWrite
                    ? Faint(rows[at])
                    : Noted(rows[at]));
            }

            foreach (PlotTemplate left in split.Unplaced)
            {
                _templates.Children.Add(Warned("   " + left.PlotId + ": " + left.Why));
            }

            foreach (string refusal in split.Refusals) _templates.Children.Add(Warned(refusal));

            // **A plot with no component is placed by its prefix and may still have no folder.**
            // The 09:18 run read six of them and dropped every one at the last step, after the
            // read and with nothing said before the press. The count is a note and the plot that
            // can be filed nowhere is a refusal, both here rather than twenty minutes later.
            IReadOnlyList<string> noComponent = CreateWords.PlotsWithNoComponent(split);
            for (int at = 0; at < noComponent.Count; at++)
            {
                _templates.Children.Add(noComponent[at].Contains("WRITTEN NOWHERE")
                    ? Warned("   " + noComponent[at])
                    : Noted(noComponent[at]));
            }

            // After the press, each row says what happened to it. Never one line for the run
            // that hides which of six failed.
            if (_lastSet != null)
            {
                foreach (TemplateOutcome outcome in _lastSet.Outcomes)
                {
                    _templates.Children.Add(outcome.Written
                        ? Faint(CreateWords.TemplateOutcomeRow(outcome))
                        : Warned(CreateWords.TemplateOutcomeRow(outcome)));
                }
            }

            // **A NOTE AND NEVER A REFUSAL.** The link state is known as soon as the model is
            // read, and the first STREETS run spent 78 plots finding nothing because not one
            // of six link instances was loaded. Said before the press rather than after it. A
            // model with no link loaded is a legitimate thing to open, so Create stays live.
            if (_facts != null && _facts.Links.Worth)
            {
                _templates.Children.Add(Noted(_facts.Links.OnThePane));
            }

            // No template names no area cell today, STREETS included since the client emptied
            // H8, so this draws nothing. It stays because the map can still express one.
            foreach (WorkbookTick tick in Settled().Where(one => one.SettledAs.TakesNoArea))
            {
                _templates.Children.Add(Faint(tick.SettledAs.Name + ": " + CreateWords.TakesNoArea));
            }

            TheAnswerable();

            // Greyed out on what the PANE owns and on nothing else. Whether a model is open
            // belongs to Revit, and a button greyed out on the pane's last answer about it
            // stayed grey after the model was saved. That is decided on the Revit thread
            // against the live document when this is pressed.
            var create = new Button
            {
                Content = PaneLabel.Escaped(CreateWords.Create),
                Padding = PanelMetrics.CellPad,
                Margin = PanelMetrics.Row,
                HorizontalAlignment = HorizontalAlignment.Left,
                IsEnabled = Settled().Count > 0 && _ticks.Count > 0
            };
            create.Click += (sender, e) => AskedToCreate();
            _templates.Children.Add(create);
        }

        /// <summary>
        /// **WHAT THE TICKS COST, SAID IN STEP 3 RATHER THAN ABOVE CREATE.** Both blocks were
        /// drawn over the button and both are about what is ticked, which is this step's
        /// subject. Not one word of either moves.
        ///
        /// The line above them is new: **the list tick is the last tick**, so a workbook row
        /// ticked or unticked after Tick the list was pressed leaves the ticks saying something
        /// the team's list does not. Nothing is ticked or unticked for him.
        /// </summary>
        private void TheTickLines()
        {
            if (_facts == null) return;

            string stale = _listTick.InWords(Settled().Select(one => one.Workbook.FileName));
            if (stale.Length > 0)
            {
                _templates.Children.Add(Warned(stale));

                PlotListRead again = PlotListFile.In(PlotListFileSetting.Read());
                if (again.Read)
                {
                    var press = new Button
                    {
                        Content = PaneLabel.Escaped(CreateWords.TickTheList),
                        Padding = PanelMetrics.CellPad,
                        Margin = PanelMetrics.Row,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        ToolTip = "Replace every tick with exactly the plots the plot list file "
                            + "names, and forget every plot ticked or unticked by hand."
                    };
                    press.Click += (sender, e) => PressTickTheList();
                    _templates.Children.Add(press);
                }
            }

            // **NOTHING ON THE TEAM'S LIST DROPS OUT WITHOUT A LINE.** They sent 154 plots to
            // export and want every one exported, so a listed plot the model does not name, a
            // plot listed twice, a line that is not a plot and a listed plot the press would put
            // into no workbook or no PDF are each named HERE, before the press, rather than found
            // in a report twenty minutes later. The rule is TickingTheList in Core.
            foreach (string line in TickingTheList.Lines(
                PlotListFile.In(PlotListFileSetting.Read()),
                _facts == null ? null : _facts.Plots,
                ComponentOn,
                _ticks.Ticked))
            {
                _templates.Children.Add(line.StartsWith("  ", StringComparison.Ordinal)
                    ? Warned(line)
                    : Noted(line));
            }

            // **TWO TICKED PLOTS SHARING ONE PRX_Plot_UID2 FILE AT ONE PATH.** On the 16:37 press
            // ten plots did, in two groups, the last one written replaced the others and every
            // row of THE PLOT LIST read YES. It is said HERE, before the press, because twenty
            // minutes that end in a report naming the replaced files is twenty minutes spent.
            //
            // **The pane owns this read already.** `ReferenceValuesPerPlot` carries all four plot
            // parameters for every plot off the same first sheet the component comes from, so no
            // second read is made and no second record of a plot's UID2 exists.
            foreach (SharedUid2Group group in SharedUid2.Of(PlotFilings.Of(
                OutputFolder.Read(), _ticks.Ticked, ComponentOn, Uid2On)))
            {
                // Every plot of the group gets its own line, stopped or filed apart, because a
                // group can hold two plots colliding with each other and a third filed somewhere
                // else. The refusal is red and the filed apart note is not.
                foreach (PlotFiling filed in group.Plots)
                {
                    bool stopped = SharedUid2.Stops(new[] { group }, filed.PlotId);

                    string said = stopped
                        ? SharedUid2.WhyStopped(new[] { group }, filed.PlotId)
                        : SharedUid2.FiledApartFrom(new[] { group }, filed.PlotId);

                    _templates.Children.Add(stopped
                        ? Warned("   " + filed.PlotId + ": " + said)
                        : Noted("   " + filed.PlotId + ": " + said));
                }
            }
        }

        /// <summary>
        /// **WHAT THE PRESS DID, IN THE PANE.** Bader's layout of 17 September, in place of
        /// step 4's controls: two counts side by side, then every plot that is not ready with
        /// its UID2 and its reason, then a way to the folder, to the report and back to step 3.
        ///
        /// **THE COUNTS AND THE REASONS ARE THE REPORT'S OWN.** `KpiResults` reads the rows
        /// `KpiCreateReport` prints THE PLOT LIST from, so the number on this screen and the
        /// `ready:` line in the file cannot say two different things.
        /// </summary>
        private void TheResults()
        {
            KpiResults results = KpiResults.Of(_lastSet);

            // **TWO COUNTS SIDE BY SIDE**, each in its own box so the number is the thing a
            // person sees first rather than a sentence they have to read.
            if (results.ListWasSet)
            {
                var counts = new UniformGrid { Columns = 2, Margin = PanelMetrics.Row };
                counts.Children.Add(CountBox(results.Ready, KpiResults.ReadyHeading, false));
                counts.Children.Add(CountBox(results.NotReady, KpiResults.NotReadyHeading, results.NotReady > 0));
                _templates.Children.Add(counts);
            }

            _templates.Children.Add(Faint(results.CountsInWords));

            // Each row says what happened to its template, written with its path or not written
            // with its reason, because one line for the run would hide which of six failed.
            foreach (TemplateOutcome outcome in _lastSet.Outcomes)
            {
                _templates.Children.Add(outcome.Written
                    ? Faint(CreateWords.TemplateOutcomeRow(outcome))
                    : Warned(CreateWords.TemplateOutcomeRow(outcome)));
            }

            if (results.NotReadyInWords.Length > 0)
            {
                _templates.Children.Add(new TextBlock
                {
                    Text = results.NotReadyInWords,
                    FontWeight = FontWeights.Bold,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = PanelMetrics.Heading
                });
            }

            // **EVERY ONE OF THEM**, in the list's own order. A panel showing the first few is
            // a panel somebody has to open the report behind anyway, which is the thing this
            // exists to save.
            if (results.NotReadyRows.Count > 0)
            {
                var notReady = new StackPanel();
                foreach (PlotListRow row in results.NotReadyRows)
                {
                    notReady.Children.Add(Warned(row.InWords));
                }

                _templates.Children.Add(Scrolling(notReady, PanelMetrics.ListHeight, "not ready"));
            }

            // **THE PRESS THAT RAISES A QUESTION MUST NOT HIDE THE CONTROL THAT ANSWERS IT.**
            // A refusal the reconciliation raised, the buttons naming a plot's two regions and
            // the groups no sheet takes all lived only inside TheCreateButton, which this
            // panel replaces. A press refused over a region nobody picked showed its own
            // refusal with nothing on screen to answer it, and the only route back was two
            // presses nothing signposted.
            TheAnswerable();

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = PanelMetrics.Row };

            string output = OutputFolder.Read();
            var folder = new Button
            {
                Content = PaneLabel.Escaped("Open the output folder"),
                Padding = PanelMetrics.CellPad,
                Margin = PanelMetrics.Gap,
                IsEnabled = output.Length > 0 && Directory.Exists(output),
                ToolTip = output.Length == 0 ? TemplateWords.NoOutputFolder : output
            };
            folder.Click += (sender, e) => Open(OutputFolder.Read());
            buttons.Children.Add(folder);

            var report = new Button
            {
                Content = PaneLabel.Escaped("Open the report"),
                Padding = PanelMetrics.CellPad,
                Margin = PanelMetrics.Gap,
                IsEnabled = _reportWhere.Length > 0 && File.Exists(_reportWhere),
                ToolTip = _reportWhere.Length == 0
                    ? "This press wrote no report."
                    : _reportWhere
            };
            report.Click += (sender, e) => Open(_reportWhere);
            buttons.Children.Add(report);

            _templates.Children.Add(buttons);

            // **THE WAY BACK IS A PRESS AND NEVER A TIMER.** The results stand until somebody
            // says they are done with them, because a panel that clears itself is a panel
            // whose numbers somebody was still reading.
            var again = new Button
            {
                Content = PaneLabel.Escaped("Back to step 3, Tick"),
                Padding = PanelMetrics.CellPad,
                Margin = PanelMetrics.Row,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            again.Click += (sender, e) =>
            {
                _showingResults = false;
                _stepOpen = KpiStep.Tick;
                RedrawTemplates();
            };
            _templates.Children.Add(again);
        }

        /// <summary>
        /// Everything the last press left for somebody to ANSWER: a reconciliation that does
        /// not add up, a plot whose two regions left the question open, an identical pair
        /// waiting on a confirm, and a group no tree list sheet takes.
        ///
        /// **ONE METHOD, DRAWN BY BOTH THE RESULTS PANEL AND STEP 4'S OWN CONTROLS**, so the
        /// two can never offer two different sets of questions about one press.
        /// </summary>
        private void TheAnswerable()
        {
            foreach (KpiCreateRun run in Held().Where(one => !one.Reconciliation.AddsUp))
            {
                foreach (string refusal in run.Reconciliation.Refusals)
                {
                    _templates.Children.Add(Warned(run.Template.Name + ": " + refusal));
                }

                TheRegionChoices(run);

                if (run.Reconciliation.IdenticalAreas.Count > 0 && !_confirmedIdentical)
                {
                    foreach (string line in CreateWords.ConfirmIdentical(run.Reconciliation.IdenticalAreas))
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

            // A group no sheet takes is a note and never a refusal: the workbook was written
            // with those rows left out, and this says where the model needs correcting.
            foreach (KpiCreateRun run in Held().Where(one => one.Template != null))
            {
                string leftOut = CreateWords.GroupsLeftOut(run.Readings, run.Template);
                if (leftOut.Length > 0) _templates.Children.Add(Noted(run.Template.Name + ": " + leftOut));
            }
        }

        /// <summary>
        /// One of the two counts, the number over its word.
        /// </summary>
        private UIElement CountBox(int howMany, string caption, bool warn)
        {
            var inside = new StackPanel { Margin = PanelMetrics.StripInside };

            inside.Children.Add(new TextBlock
            {
                Text = howMany.ToString(System.Globalization.CultureInfo.InvariantCulture),
                FontWeight = FontWeights.Bold,
                FontSize = PanelMetrics.HeaderRowHeight / 2.0,
                Foreground = warn ? _theme.Warning : _theme.Foreground,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            inside.Children.Add(new TextBlock
            {
                Text = caption,
                Foreground = _theme.Faint,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            return new Border
            {
                Background = _theme.StepHeader,
                BorderBrush = _theme.Line,
                BorderThickness = PanelMetrics.Outline,
                Margin = PanelMetrics.Gap,
                Child = inside
            };
        }

        /// <summary>
        /// Opens a folder or a file with whatever Windows opens it with. **It touches no
        /// document**, so it needs no external event, the same reason the Browse dialogs do
        /// not. A path that will not open says so on the status line rather than throwing out
        /// of a click handler and taking the pane with it.
        /// </summary>
        private void Open(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                System.Diagnostics.Process.Start(path);
            }
            catch (System.Exception bad)
            {
                Say("That could not be opened: " + path + ". " + bad.Message);
            }
        }

        /// <summary>
        /// One row of buttons per plot whose regions left the answer open. Which of a plot's
        /// two regions carries the area varies by plot, so the tool asks rather than picking.
        /// </summary>
        private void TheRegionChoices(KpiCreateRun run)
        {
            foreach (PlotReading reading in run.Readings)
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
        /// The ticks and the hand record moved together. **Nothing sets one without the other**,
        /// which is the whole of the fault the eighty third pass came out of: three paths moved
        /// the ticks and left the record standing, and a record that disagrees with the screen is
        /// worse than no record.
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
            _whyThisTemplate = string.Empty;
            if (_facts == null || _ticks.Count == 0 || _picks.Count > 0) return;

            AgreedValue component = new AgreedValue(_ticks.Ticked
                .Select(plotId => new PlotText(plotId, _facts.ComponentOn(plotId))));

            TemplateChoice choice = TemplateForComponent.For(component, _ticks.Ticked, null);

            // Said whichever way it went. A preselection arriving without a word is what the
            // prefix route would otherwise be: the component could not be read on EP-05 and
            // three plots like it, and which route placed them has to be on the screen.
            _whyThisTemplate = choice.Why;

            if (choice.NeedsAPick) return;

            // The preselection TICKS a row rather than settling a template beside the list, so
            // what Create reads and what the screen shows are one record. A folder holding no
            // file of that template ticks nothing and says so through the line above.
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
        /// The name a ticked row offers, filled once and never over what somebody typed. The
        /// suggestion rule is the one a single template run already used, asked per row with
        /// that template's own share of the ticked plots.
        /// </summary>
        /// <summary>
        /// **TICKING A TEMPLATE ROW TICKS THE PLOTS THAT WILL GO INTO IT.** A person who ticks
        /// MOSQUES has already said which plots they mean, and making them find a grouping
        /// button and press that too was the same fact asked for twice. A plot held off by hand
        /// stays off.
        /// </summary>
        private void TickItsPlots(WorkbookTick tick)
        {
            if (tick.SettledAs == null || _facts == null) return;

            _ticks = TickingATemplate.Ticked(_ticks, tick.SettledAs, ComponentOn, _byHand);
        }

        /// <summary>
        /// The row's own line about the plots of this template that are not going in, or nothing
        /// at all where every one of them is.
        ///
        /// **A LINE ABOUT NOTHING IS ONE THE TEAM READS PAST ON EVERY OTHER PRESS**, so
        /// <see cref="TickingATemplate.SomeOfThem"/> answers with an empty string where the
        /// count is whole and this adds no control for it. It is a NOTE rather than a refusal:
        /// the run goes through and the workbook is written, and what the line says is that
        /// fewer plots are going in than belong to the template.
        /// </summary>
        private UIElement HeldOffOn(WorkbookTick tick)
        {
            string line = TickingATemplate.RowLine(tick.SettledAs, _ticks, ComponentOn);

            return line.Length == 0
                ? (UIElement)new StackPanel { Margin = PanelMetrics.Nothing }
                : Noted("   " + line);
        }

        /// <summary>
        /// What one plot's sheets hold for the component, off the read the pane was given, or
        /// nothing where no read has happened. It is the one route the split and the ticking
        /// both ask, so a plot ticked by a template row is a plot that template's workbook
        /// really gets.
        /// </summary>
        private string ComponentOn(string plotId)
        {
            return _facts == null ? string.Empty : _facts.ComponentOn(plotId);
        }

        /// <summary>
        /// This plot's PRX_Plot_UID2, off the same per plot read the reference block already
        /// draws from. **It is the name of the plot's folder and of its file**, so two ticked
        /// plots holding one value write at one path.
        /// </summary>
        private string Uid2On(string plotId)
        {
            if (_facts == null) return string.Empty;

            PlotParameterValue held = _facts.ReferenceValuesOn(plotId)
                .FirstOrDefault(one => string.Equals(one.Name, KpiNames.PlotUid2, StringComparison.Ordinal));

            return held == null ? string.Empty : held.Value;
        }

        /// <summary>
        /// Which ticked plot belongs to which ticked template, worked out in Core off the
        /// component the plot read already carries. The pane applies it and decides none of it.
        /// </summary>
        private TemplateSplit TheSplit()
        {
            return PlotsPerTemplate.Split(
                _ticks.Ticked, ComponentOn, Settled().Select(one => one.SettledAs));
        }

        /// <summary>
        /// The ticked rows whose template is settled. A row still waiting on which of the two
        /// park templates it is has no template and arms nothing.
        /// </summary>
        private IReadOnlyList<WorkbookTick> Settled()
        {
            return _picks.Where(one => one.SettledAs != null).ToList();
        }

        /// <summary>
        /// The runs the last press produced, one per template that ran, or none.
        /// </summary>
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
        /// the file is caught between the two park templates. Unticking forgets its name with it,
        /// because a name typed for a workbook nobody is writing is not a name to keep.
        /// </summary>
        private void ToggledWorkbook(RecognisedWorkbook workbook)
        {
            WorkbookTick already = TickFor(workbook);
            if (already != null)
            {
                _picks.Remove(already);

                // **UNTICKING A TEMPLATE UNTICKS ITS PLOTS.** The row IS the grouping button
                // now, so the two halves of one choice move together.
                if (already.SettledAs != null)
                {
                    _ticks = TickingATemplate.Unticked(
                        _ticks, already.SettledAs, ComponentOn, _byHand);
                }
            }
            else
            {
                _picks.Add(new WorkbookTick(workbook, workbook.Template));
                TickItsPlots(_picks[_picks.Count - 1]);
                }

            // The line describes what the preselection did or did not do, so a tick by hand is
            // what makes it untrue whichever way it read. A line left standing beside the state
            // that contradicts it is the shape this repo has met seven times.
            _whyThisTemplate = string.Empty;

            // **AND SO ARE THE RESULTS OF THE PRESS BEFORE IT.** This moves the plot ticks
            // without going through Changed, so the panel describing the last press would have
            // outlived the ticks it was made from.
            _showingResults = false;
            RedrawTemplates();
        }

        /// <summary>
        /// The three parameters the pickers hold, so a line saying where a value comes from
        /// names the one that will really be read.
        /// </summary>
        private ChosenParameters Chosen()
        {
            return new ChosenParameters(_componentParameter, _referenceParameter, _locationParameter);
        }

        /// <summary>
        /// The model state is not tested here. It is read off the live document inside the
        /// handler, because the pane's copy of it is what went stale.
        /// </summary>
        private void AskedToCreate()
        {
            if (Settled().Count == 0) return;

            RememberedNames.Remember(_preparedBy.Text, _position.Text);

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
                components);

            Say(CreateWords.Creating);

            // **OPENED HERE AND NOT INSIDE EXECUTE.** This is a click handler on the pane's
            // own thread, so the window is up before the external event is raised and the
            // handler never opens a window from inside its own call frame.
            Shut();
            _running = KpiProgressWindow.Opened(CreateWords.Creating);

            Ask(KpiRequest.Create);
        }

        private void Found(KpiPlotFacts facts)
        {
            Dispatcher.Invoke(() =>
            {
                _facts = facts;
                _ticks = new PlotTicks(facts.Plots, _ticks.Ticked);

                // **A ROW TICKED BEFORE THE READ GETS ITS PLOTS WHEN THE READ LANDS.** The
                // template rows come off the templates folder and need no model, so ticking
                // MOSQUES and then pressing Read is the ordinary order, and the plots cannot
                // be ticked until they exist.
                foreach (WorkbookTick tick in Settled()) TickItsPlots(tick);

                // The header's count comes back with the plots, which is a PRESS now. It used
                // to come back with a read that started itself when the pane was shown, so the
                // count was there without anybody asking and the model was held while it was
                // counted.
                _scannedTitle = _model.Title;

                // **THE PLOTS READ IS NOT A READ OF THE MODEL** and the header says which of the
                // two produced the count, because 0.8 seconds off this press and 46.2 off
                // Create's scan printed the same way on one model.
                _read = ReadOfTheModel.ThePlots(DateTime.Now, facts.ElementInstances, facts.ReadSeconds);
                TheHeader();

                // And the status line says the read landed. It used to keep saying it was
                // reading, so the pane showed a finished count beside a line still working.
                Say(CreateWords.PlotsAreRead(facts.Plots.All.Count));

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

                // **THE READ WAS A PRESS, SO THE PANE MOVES TO WHAT IT WAS FOR.** Nothing
                // else moves the step by itself except a finished press, and both are
                // answers to something the person just did.
                _stepOpen = KpiStep.Tick;

                RedrawTemplates();
            });
        }

        private void Made(KpiCreateRunSet set, string reportWhere)
        {
            Dispatcher.Invoke(() =>
            {
                _lastSet = set;
                _reportWhere = reportWhere ?? string.Empty;

                // **AFTER A PRESS THE PANE SHOWS THE RESULT RATHER THAN SENDING HIM TO THE
                // REPORT FILE.** Step 4's controls give way to the results panel, and the way
                // back is a button rather than a timer.
                _showingResults = true;
                _stepOpen = KpiStep.Create;

                // Create is the one thing in this tool that writes a workbook, and it can write
                // into the templates folder. Any press that reached the patcher with an output
                // path in that folder lists it afresh on the redraw, whether or not it wrote,
                // because the copy lands at the output path before the patch and a patch that
                // fails after it leaves the copy behind under a name the listing may hold. An
                // output path anywhere else leaves the listing and its counts standing, because
                // nothing in that folder moved.
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

        /// <summary>
        /// **ONE CAPTIONED BROWSE LINE, used by all four things this pane is pointed at.** The
        /// output folder, the forms folder, the street reference file and the team's plot list
        /// file. It was written out four times, audit 4 finding 77, and the copies had already
        /// drifted: one warned when the chosen folder was the templates folder, one warned when a
        /// remembered file had gone, and one did neither, so a forms folder that had been moved
        /// read exactly like one nobody had ever chosen.
        ///
        /// **What goes UNDER the line stays each caller's own**, because those really are
        /// different facts about different things. What is the same is the shape.
        /// </summary>
        private UIElement BrowsedLine(
            string caption, string value, string whenEmpty, string tooltip, Action browse)
        {
            var line = new DockPanel { Margin = PanelMetrics.Row, LastChildFill = true };
            var button = new Button
            {
                Content = PaneLabel.Escaped("Browse"),
                Padding = PanelMetrics.CellPad,
                Margin = PanelMetrics.Gap,
                ToolTip = tooltip
            };
            button.Click += (sender, e) => browse();
            DockPanel.SetDock(button, Dock.Right);

            var label = new TextBlock
            {
                Text = caption,
                Width = PanelMetrics.WideLabelWidth,
                VerticalAlignment = VerticalAlignment.Center
            };
            DockPanel.SetDock(label, Dock.Left);

            line.Children.Add(button);
            line.Children.Add(label);
            line.Children.Add(new TextBlock
            {
                Text = string.IsNullOrEmpty(value) ? whenEmpty : value,
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center
            });

            return line;
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

        /// <summary>
        /// The output folder, picked the same way the template folder is. Nothing is thrown
        /// away by this: the template pick and the ticks are about the model and the folder is
        /// about where the file lands.
        /// </summary>
        private void BrowseForTheOutputFolder()
        {
            using (var picking = new System.Windows.Forms.FolderBrowserDialog())
            {
                picking.Description = "The folder the filled GRP KPI Checklist workbooks go to";
                string already = OutputFolder.Read();
                if (already.Length > 0) picking.SelectedPath = already;

                if (picking.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                if (!OutputFolder.Remember(picking.SelectedPath))
                {
                    Say("The folder could not be remembered. " + OutputFolder.PointerFileName
                        + " beside the installed add-in refused the write.");
                }

                RedrawTemplates();
            }
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

                _picks.Clear();
                _whyThisTemplate = string.Empty;
                _templatesListed = TemplateListing.Nothing;

                // Another folder is another set of workbook rows, so a list tick recorded
                // against the old ones would report a drift nobody caused.
                _listTick = ListTickFreshness.NeverPressed;
                RedrawTemplates();
            }
        }

        /// <summary>
        /// A NOTE, which is not a refusal. **They were the same colour**, so nine lines of red
        /// and orange sat over the Create button on the first per plot run and nothing said
        /// which of them stopped a workbook. One stops a workbook and the other does not, and
        /// the two have to look different: a refusal keeps the warning colour and a note takes
        /// the body colour with the word Note in front of it.
        /// </summary>
        private TextBlock Noted(string text)
        {
            return new TextBlock
            {
                Text = "Note. " + text,
                TextWrapping = TextWrapping.Wrap,
                Foreground = _theme.Foreground,
                Margin = PanelMetrics.Row
            };
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
