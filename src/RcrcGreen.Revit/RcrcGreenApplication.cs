using Autodesk.Revit.UI;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// Builds the ribbon. There are no commands on it yet, only the tab and the panel the
    /// Drawing Sheet tool will sit in.
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

            application.CreateRibbonPanel(TabName, SheetsPanelName);
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
