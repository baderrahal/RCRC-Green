using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.UI;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// Builds the ribbon and puts the commands on it.
    /// </summary>
    public class RcrcGreenApplication : IExternalApplication
    {
        public const string TabName = "RCRC Green";

        public const string SheetsPanelName = "Sheets";

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

            RibbonPanel sheets = SheetsPanel(application);

            Add(sheets, ScanModelCommand.ButtonName, ScanModelCommand.ButtonText,
                typeof(ScanModelCommand),
                "Read the open model and write what is in it to a text file on the Desktop.",
                "Lists every sheet, view, view template and scope box, and every value of "
                + "PRX_Plot_ID with a count of the elements carrying it. Runs each view name, "
                + "sheet name and sheet number through the naming pattern and reports which "
                + "ones it did not fit. Nothing in the model is changed.");

            Add(sheets, AssignScopeBoxCommand.ButtonName, AssignScopeBoxCommand.ButtonText,
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
        /// Creating the panel blind gives a second Sheets panel when the add-in is registered
        /// twice, and the user then has two panels holding the same two buttons with no way to
        /// tell which is which.
        /// </summary>
        private static RibbonPanel SheetsPanel(UIControlledApplication application)
        {
            List<RibbonPanel> already = application.GetRibbonPanels(TabName);
            RibbonPanel found = already == null
                ? null
                : already.FirstOrDefault(panel => panel.Name == SheetsPanelName);

            return found ?? application.CreateRibbonPanel(TabName, SheetsPanelName);
        }

        // Text only on purpose. An icon goes on once the buttons have earned a place people
        // look for, and a missing image is better than a wrong one.
        private static void Add(
            RibbonPanel panel, string name, string text, System.Type command, string tip, string longer)
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
