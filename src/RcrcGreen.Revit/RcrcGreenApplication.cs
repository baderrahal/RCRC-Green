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

        public const string ReportsPanelName = "Reports";

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
            application.RegisterDockablePane(
                ShowDrawingSheetCommand.PaneId,
                ShowDrawingSheetCommand.PaneTitle,
                new DrawingSheetPanel());

            RibbonPanel drawingSheet = PanelNamed(application, DrawingSheetPanelName);
            Add(drawingSheet, ShowDrawingSheetCommand.ButtonName, ShowDrawingSheetCommand.ButtonText,
                typeof(ShowDrawingSheetCommand),
                "Open the Drawing Sheet panel.",
                "Pick a plot range, see which view types each plot in it has, and mark the ones "
                + "that are wanted. Marking records intent and changes nothing. The panel also "
                + "runs the scope box assignment over the plots in range.");

            // The two report commands sit on their own panel. They read the whole model rather
            // than a range, and they are the check the panel is measured against.
            RibbonPanel reports = PanelNamed(application, ReportsPanelName);

            Add(reports, ScanModelCommand.ButtonName, ScanModelCommand.ButtonText,
                typeof(ScanModelCommand),
                "Read the open model and write what is in it to a text file on the Desktop.",
                "Lists every sheet, view, view template and scope box, and every value of "
                + "PRX_Plot_ID with a count of the elements carrying it. Runs each view name, "
                + "sheet name and sheet number through the naming pattern and reports which "
                + "ones it did not fit. Nothing in the model is changed.");

            Add(reports, AssignScopeBoxCommand.ButtonName, AssignScopeBoxCommand.ButtonText,
                typeof(AssignScopeBoxCommand),
                "Give every view that names a plot the scope box named for that plot.",
                "Sorts every view into one of six cases and shows the counts before writing "
                + "anything. Only a view with no scope box, whose plot has a scope box of "
                + "exactly that name, is changed. A view that already carries a scope box is "
                + "left alone whether it is the right one or not, and reported. Every "
                + "assignment goes in one transaction, so it is one undo.");

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
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
