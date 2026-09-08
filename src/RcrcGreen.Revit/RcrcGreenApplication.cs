using System.Reflection;
using Autodesk.Revit.UI;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// Builds the ribbon and puts Scan Model on it.
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
                // of the add-in is registered. The panel below still needs creating.
            }

            RibbonPanel sheets = application.CreateRibbonPanel(TabName, SheetsPanelName);

            var scan = new PushButtonData(
                ScanModelCommand.ButtonName,
                ScanModelCommand.ButtonText,
                Assembly.GetExecutingAssembly().Location,
                typeof(ScanModelCommand).FullName);

            scan.ToolTip = "Read the open model and write what is in it to a text file on the Desktop.";
            scan.LongDescription =
                "Lists every sheet, view, view template and scope box, and every value of "
                + "PRX_Plot_ID with a count of the elements carrying it. Runs each view name, "
                + "sheet name and sheet number through the naming pattern and reports which "
                + "ones it did not fit. Nothing in the model is changed.";

            // Text only on purpose. An icon goes on once the button has earned a place people
            // look for, and a missing image is better than a wrong one.
            PushButton placed = (PushButton)sheets.AddItem(scan);
            placed.ItemText = ScanModelCommand.ButtonText;

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
