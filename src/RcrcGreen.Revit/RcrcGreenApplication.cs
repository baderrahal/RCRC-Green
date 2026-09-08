using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.UI;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// Builds the ribbon and registers the dockable panel.
    /// </summary>
    public class RcrcGreenApplication : IExternalApplication
    {
        public const string TabName = "RCRC Green";

        public const string DrawingSheetPanelName = "Drawing Sheet";

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

            // The panel has to be registered before any document opens, which is why it is
            // here rather than in the command that shows it.
            ShowDrawingSheetCommand.PaneRegistered = Registered(application);

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

            // One tab, one panel, one button. Scan Model and Scope Box were buttons of their
            // own and are not any more. Both are still reachable from inside the panel, which
            // is where someone deciding what to do already is.
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        /// <summary>
        /// Registers the dockable pane and says whether it took.
        ///
        /// Guarded because an exception here leaves OnStartup throwing, and Revit answers that
        /// by building no ribbon at all. A panel that will not register is worth losing the
        /// panel over. It is not worth losing Scan Model and Scope Box as well.
        /// </summary>
        private static bool Registered(UIControlledApplication application)
        {
            try
            {
                application.RegisterDockablePane(
                    ShowDrawingSheetCommand.PaneId,
                    ShowDrawingSheetCommand.PaneTitle,
                    new DrawingSheetPanel());
                return true;
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                // Building the panel is WPF work and registering is Revit work, so the throw
                // can come from either side of the line.
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
