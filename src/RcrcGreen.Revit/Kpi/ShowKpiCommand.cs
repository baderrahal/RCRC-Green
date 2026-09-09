using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RcrcGreen.Revit.Kpi
{
    /// <summary>
    /// Opens the KPI pane and brings it to the front. The button is called KPI Checklist
    /// after the workbook it will one day fill, and the scan inside the pane is KPI Scan,
    /// never Scan Model, because that name is already a button inside the Drawing Sheet pane
    /// and two buttons with one name doing different things is a trap.
    /// </summary>
    [Transaction(TransactionMode.ReadOnly)]
    [Regeneration(RegenerationOption.Manual)]
    public class ShowKpiCommand : IExternalCommand
    {
        public const string ButtonName = "ShowKpiChecklist";

        public const string ButtonText = "KPI\nChecklist";

        /// <summary>
        /// Its own identifier, fixed for the life of the add-in and different from the Drawing
        /// Sheet pane's. Revit keeps the pane by this, so changing it would lose wherever the
        /// user had docked the old one.
        /// </summary>
        public static readonly DockablePaneId PaneId =
            new DockablePaneId(new Guid("c1e92213-9fa7-46d0-bcc5-f5744ec0bd82"));

        public const string PaneTitle = "RCRC Green KPI";

        public const string NotAvailableTip = "The KPI pane is not available.";

        public const string NotAvailable =
            "The KPI pane did not register when Revit started, so it cannot be opened. The "
            + "Drawing Sheet panel is unaffected. Restart Revit to try again.";

        /// <summary>
        /// Set once by <see cref="RcrcGreenApplication"/> at startup, the same way as the
        /// Drawing Sheet pane. A pane that will not register leaves the button on the ribbon
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
