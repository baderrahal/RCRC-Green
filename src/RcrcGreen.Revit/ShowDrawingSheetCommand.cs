using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// Opens the Drawing Sheet panel and brings it to the front.
    /// </summary>
    [Transaction(TransactionMode.ReadOnly)]
    [Regeneration(RegenerationOption.Manual)]
    public class ShowDrawingSheetCommand : IExternalCommand
    {
        public const string ButtonName = "ShowDrawingSheet";

        public const string ButtonText = "Drawing\nSheet";

        /// <summary>
        /// Fixed for the life of the add-in. Revit keeps the pane by this, so changing it
        /// would lose wherever the user had docked the old one.
        /// </summary>
        public static readonly DockablePaneId PaneId =
            new DockablePaneId(new Guid("7f2b6c94-3a51-4e18-9d0c-5b8e2a1f4c63"));

        public const string PaneTitle = "RCRC Green Drawing Sheet";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                DockablePane pane = commandData.Application.GetDockablePane(PaneId);
                pane.Show();
                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException failed)
            {
                // The pane is registered at startup. If it is not there, the add-in did not
                // load cleanly, and saying so beats a raw crash dialog.
                message = "The Drawing Sheet panel is not registered. Restart Revit. " + failed.Message;
                return Result.Failed;
            }
        }
    }
}
