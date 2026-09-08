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

        public const string NotAvailableTip = "The Drawing Sheet panel is not available.";

        public const string NotAvailable =
            "The Drawing Sheet panel did not register when Revit started, so it cannot be "
            + "opened. Scan Model and Scope Box on the Reports panel are unaffected. Restart "
            + "Revit to try again.";

        /// <summary>
        /// Set once by <see cref="RcrcGreenApplication"/> at startup.
        ///
        /// Registration can fail while the rest of the add-in loads, and the button is built
        /// either way rather than left off the ribbon, because a button that says why it will
        /// not open beats one that is silently missing.
        /// </summary>
        public static bool PaneRegistered { get; internal set; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (!PaneRegistered)
            {
                message = NotAvailable;
                return Result.Failed;
            }

            try
            {
                DockablePane pane = commandData.Application.GetDockablePane(PaneId);
                pane.Show();
                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException failed)
            {
                // Registration reported success and the pane is still not there, so something
                // took it away afterwards. Saying so beats a raw crash dialog.
                message = NotAvailable + " " + failed.Message;
                return Result.Failed;
            }
        }
    }
}
