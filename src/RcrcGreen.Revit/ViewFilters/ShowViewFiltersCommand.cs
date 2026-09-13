using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RcrcGreen.Revit.ViewFilters
{
    /// <summary>
    /// Opens the View Filters pane and brings it to the front, the one button on the View
    /// Filters ribbon panel. The same shape as the other two show commands, with its own
    /// identifier, so a pane that will not register costs this button its pane and nothing
    /// else on the ribbon.
    /// </summary>
    [Transaction(TransactionMode.ReadOnly)]
    [Regeneration(RegenerationOption.Manual)]
    public class ShowViewFiltersCommand : IExternalCommand
    {
        public const string ButtonName = "ShowViewFilters";

        public const string ButtonText = "View\nFilters";

        /// <summary>
        /// Its own identifier, fixed for the life of the add-in and different from the other
        /// two panes'. Revit keeps the pane by this, so changing it would lose wherever the
        /// user had docked the old one.
        /// </summary>
        public static readonly DockablePaneId PaneId =
            new DockablePaneId(new Guid("e3d618f2-da92-4cc1-b868-15cd58ed79c5"));

        public const string PaneTitle = "RCRC Green View Filters";

        public const string NotAvailableTip = "The View Filters pane is not available.";

        public const string NotAvailable =
            "The View Filters pane did not register when Revit started, so it cannot be "
            + "opened. The other panels are unaffected. Restart Revit to try again.";

        /// <summary>
        /// Set once by <see cref="RcrcGreenApplication"/> at startup, the same way as the
        /// other two panes. A pane that will not register leaves the button on the ribbon
        /// saying why rather than silently missing.
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
