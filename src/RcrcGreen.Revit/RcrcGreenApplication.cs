using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.UI;
using RcrcGreen.Revit.Kpi;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// Builds the ribbon and registers the two dockable panes.
    ///
    /// One tab holding two panels side by side. Drawing Sheet keeps its single button. KPI is
    /// a panel built to carry several buttons later and carries one this round.
    /// </summary>
    public class RcrcGreenApplication : IExternalApplication
    {
        public const string TabName = "RCRC Green";

        public const string DrawingSheetPanelName = "Drawing Sheet";

        public const string KpiPanelName = "KPI";

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                application.CreateRibbonTab(TabName);
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException)
            {
                // Revit throws when the tab is already there, which happens when a second copy
                // of the add-in is registered.
            }

            // The panes have to be registered before any document opens, which is why they
            // are here rather than in the commands that show them. Each goes through the same
            // guard on its own, so a KPI pane that will not register costs the KPI button its
            // pane and nothing else on the ribbon.
            ShowDrawingSheetCommand.PaneRegistered = Registered(
                application,
                ShowDrawingSheetCommand.PaneId,
                ShowDrawingSheetCommand.PaneTitle,
                () => new DrawingSheetPanel());

            ShowKpiCommand.PaneRegistered = Registered(
                application,
                ShowKpiCommand.PaneId,
                ShowKpiCommand.PaneTitle,
                () => new KpiPanel());

            RibbonPanel drawingSheet = PanelNamed(application, DrawingSheetPanelName);
            Add(drawingSheet, ShowDrawingSheetCommand.ButtonName, ShowDrawingSheetCommand.ButtonText,
                typeof(ShowDrawingSheetCommand),
                ShowDrawingSheetCommand.PaneRegistered
                    ? "Open the Drawing Sheet panel."
                    : ShowDrawingSheetCommand.NotAvailableTip,
                ShowDrawingSheetCommand.PaneRegistered
                    ? "Pick a plot range, untick the plots you are not working on, and see "
                      + "which view types each of the rest is missing. Marking a missing view "
                      + "records intent and changes nothing. Scan Model and the scope box "
                      + "assignment are both in there too."
                    : ShowDrawingSheetCommand.NotAvailable);

            // Scan Model and Scope Box were buttons of their own and are not any more. Both are
            // still reachable from inside the Drawing Sheet panel, which is where someone
            // deciding what to do already is. The KPI panel is the second panel on the tab,
            // and its one button this round shows the KPI pane.
            RibbonPanel kpi = PanelNamed(application, KpiPanelName);
            Add(kpi, ShowKpiCommand.ButtonName, ShowKpiCommand.ButtonText,
                typeof(ShowKpiCommand),
                ShowKpiCommand.PaneRegistered
                    ? "Open the KPI pane."
                    : ShowKpiCommand.NotAvailableTip,
                ShowKpiCommand.PaneRegistered
                    ? "Create fills the GRP KPI Checklist from the open model, reading the model "
                      + "itself when it needs to, and writes a text file saying where every value "
                      + "came from. It creates nothing in the model and never writes to a template."
                    : ShowKpiCommand.NotAvailable);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        /// <summary>
        /// Registers one dockable pane and says whether it took.
        ///
        /// Guarded because an exception here leaves OnStartup throwing, and Revit answers that
        /// by building no ribbon at all. A pane that will not register is worth losing that
        /// pane over. It is not worth losing the other pane and every button as well. The
        /// pane is built inside the guard, because building it is WPF work and registering is
        /// Revit work and the throw can come from either side.
        /// </summary>
        private static bool Registered(
            UIControlledApplication application,
            DockablePaneId paneId,
            string title,
            Func<IDockablePaneProvider> build)
        {
            try
            {
                application.RegisterDockablePane(paneId, title, build());
                return true;
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        /// <summary>
        /// Creating a panel blind gives a second one of the same name when the add-in is
        /// registered twice, and the user then has two panels holding the same buttons with no
        /// way to tell which is which.
        /// </summary>
        private static RibbonPanel PanelNamed(UIControlledApplication application, string name)
        {
            List<RibbonPanel> already = application.GetRibbonPanels(TabName);
            RibbonPanel found = already == null
                ? null
                : already.FirstOrDefault(panel => panel.Name == name);

            return found ?? application.CreateRibbonPanel(TabName, name);
        }

        // Text only on purpose. An icon goes on once the buttons have earned a place people
        // look for, and a missing image is better than a wrong one.
        private static void Add(
            RibbonPanel panel, string name, string text, Type command, string tip, string longer)
        {
            var button = new PushButtonData(
                name,
                text,
                Assembly.GetExecutingAssembly().Location,
                command.FullName)
            {
                ToolTip = tip,
                LongDescription = longer
            };

            var placed = (PushButton)panel.AddItem(button);
            placed.ItemText = text;
        }
    }
}
