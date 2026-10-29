using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// What kind of refusal a patch met, set where the refusal is built.
    /// </summary>
    public enum PatchRefusal
    {
        None = 0,
        Other,
        FileInUse,
        WriteFailed,
        FolderRefused,
        FormulaError,
        WouldOverwriteTheTemplate
    }

    /// <summary>
    /// Why one ticked plot has no workbook, as a value. **Set where the plot's outcome is
    /// built**, in the handler and in <see cref="NotWrittenKinds.Of"/>, so the result screen
    /// never reads a reason's words to decide what it was.
    /// </summary>
    public enum NotWrittenKind
    {
        Unknown = 0,
        NoAssetType,
        AssetTypeNotTicked,
        SharedFolder,
        PathRefused,
        NoOutputRoot,
        NoFolderForComponent,
        TwoFoldersFit,
        NoUid2,
        UnusableUid2,
        RegionPick,
        AreaConfirm,
        ScheduleCheck,
        FileInUse,
        WriteFailed,
        FolderRefused,
        FormulaError,
        WouldOverwriteTheTemplate,
        Crashed,
        NoFolderLayout
    }

    public static class PatchRefusals
    {
        // Windows's own codes for a file another program holds open, ERROR_SHARING_VIOLATION
        // and ERROR_LOCK_VIOLATION as HRESULTs. Excel holding last press's workbook is what
        // meets them, which is why that case gets a sentence of its own.
        public const int SharingViolation = unchecked((int)0x80070020);
        public const int LockViolation = unchecked((int)0x80070021);

        public static PatchRefusal ForIo(int hresult)
        {
            return hresult == SharingViolation || hresult == LockViolation
                ? PatchRefusal.FileInUse
                : PatchRefusal.WriteFailed;
        }
    }

    public static class NotWrittenKinds
    {
        /// <summary>
        /// A plot whose regions left the question open: more than one holds an area and nobody
        /// has picked. The result's region buttons and this kind both ask it, one rule.
        /// </summary>
        public static bool WaitsOnARegion(PlotReading reading)
        {
            return reading != null && reading.ChosenRegion == null && reading.RegionsHoldingAnArea.Count > 1;
        }

        /// <summary>
        /// The kind of a plot whose run was built and wrote nothing, read off the run's own
        /// records in the order the press met them: its path, its reconciliation, its patch.
        /// </summary>
        public static NotWrittenKind Of(KpiCreateRun run)
        {
            if (run == null) return NotWrittenKind.Unknown;
            if (run.Where != null && !run.Where.Ok) return ForPath(run.Where);

            if (!run.Reconciliation.AddsUp)
            {
                if (run.Readings.Any(WaitsOnARegion)) return NotWrittenKind.RegionPick;

                return run.Reconciliation.IdenticalAreas.Count > 0
                    ? NotWrittenKind.AreaConfirm
                    : NotWrittenKind.ScheduleCheck;
            }

            if (run.Outcome == null) return NotWrittenKind.Unknown;

            switch (run.Outcome.RefusedAs)
            {
                case PatchRefusal.FileInUse: return NotWrittenKind.FileInUse;
                case PatchRefusal.WriteFailed: return NotWrittenKind.WriteFailed;
                case PatchRefusal.FolderRefused: return NotWrittenKind.FolderRefused;
                case PatchRefusal.FormulaError: return NotWrittenKind.FormulaError;
                case PatchRefusal.WouldOverwriteTheTemplate: return NotWrittenKind.WouldOverwriteTheTemplate;
                default: return NotWrittenKind.Unknown;
            }
        }

        /// <summary>
        /// A refused path's own kind, off the value <see cref="PlotWorkbookPath.For"/> set. FM-08
        /// is TwoFoldersFit, no component and MOSQUES reaching two folders, which is not a
        /// missing UID2 whatever the two sentences have in common.
        /// </summary>
        public static NotWrittenKind ForPath(PlotWorkbookPath where)
        {
            if (where == null || where.Ok) return NotWrittenKind.Unknown;

            switch (where.RefusedAs)
            {
                case PathRefusal.NoRoot: return NotWrittenKind.NoOutputRoot;
                case PathRefusal.NoFolderForComponent: return NotWrittenKind.NoFolderForComponent;
                case PathRefusal.TemplateReachesTwoFolders: return NotWrittenKind.TwoFoldersFit;
                case PathRefusal.NoUid2: return NotWrittenKind.NoUid2;
                case PathRefusal.UnusableUid2: return NotWrittenKind.UnusableUid2;
                case PathRefusal.NoFolderLayout: return NotWrittenKind.NoFolderLayout;
                default: return NotWrittenKind.PathRefused;
            }
        }

        /// <summary>
        /// A plot the split placed in no ticked workbook: its asset type is not ticked, or no
        /// route places it at all. Read off the value the split set.
        /// </summary>
        public static NotWrittenKind ForUnplaced(PlotTemplate plot)
        {
            return plot != null && plot.AssetTypeNotTicked
                ? NotWrittenKind.AssetTypeNotTicked
                : NotWrittenKind.NoAssetType;
        }
    }

    /// <summary>
    /// The result screen's reason beside a failed plot, at most eight words a sentence. **The
    /// full reason is the outcome's own Why**, which is the line's tooltip and is printed in the
    /// report word for word, so nothing is lost by the screen being short.
    /// </summary>
    public static class FailedReasons
    {
        public const string Fallback = "Not written. Details in the report.";

        /// <summary>
        /// Whether pressing Create again with nothing else changed can write this plot. **A plot
        /// refused by what the model or the ticks hold fails the same way every press**, so Retry
        /// failed leaves it out: its asset type not ticked, a folder another plot shares, no
        /// folder for its component, two folders fitting, no UID2 or one that cannot name a
        /// folder, no folder layout picked in Setup, and a plot no route places at all. Everything else, a workbook held open in
        /// Excel, a region to pick, a write that stopped, can go through on the next press.
        /// </summary>
        public static bool Retryable(NotWrittenKind kind)
        {
            switch (kind)
            {
                case NotWrittenKind.NoAssetType:
                case NotWrittenKind.AssetTypeNotTicked:
                case NotWrittenKind.SharedFolder:
                case NotWrittenKind.NoFolderForComponent:
                case NotWrittenKind.TwoFoldersFit:
                case NotWrittenKind.NoUid2:
                case NotWrittenKind.UnusableUid2:
                case NotWrittenKind.NoFolderLayout:
                    return false;
                default:
                    return true;
            }
        }

        public static string Short(NotWrittenKind kind)
        {
            switch (kind)
            {
                case NotWrittenKind.NoAssetType: return "No asset type places this plot.";
                case NotWrittenKind.SharedFolder: return "Shares its folder with another plot.";
                case NotWrittenKind.AssetTypeNotTicked: return "Its asset type is not ticked.";
                case NotWrittenKind.PathRefused: return "Its folder could not be worked out.";
                case NotWrittenKind.NoOutputRoot: return "No output folder to file it under.";
                case NotWrittenKind.NoFolderForComponent: return "No folder is known for its component.";
                case NotWrittenKind.TwoFoldersFit: return "No component, and two folders fit.";
                case NotWrittenKind.NoUid2: return "No PRX_Plot_UID2 to name its folder.";
                case NotWrittenKind.UnusableUid2: return "Its PRX_Plot_UID2 cannot name a folder.";
                case NotWrittenKind.RegionPick: return "Two regions hold an area. Pick one.";
                case NotWrittenKind.AreaConfirm: return "Same area as another plot. Confirm it.";
                case NotWrittenKind.ScheduleCheck: return "Schedule check refused it. Details in the report.";
                case NotWrittenKind.FileInUse: return "Workbook open in Excel. Close it and try again.";
                case NotWrittenKind.WriteFailed: return "Workbook could not be written.";
                case NotWrittenKind.FolderRefused: return "The folder refused the write.";
                case NotWrittenKind.FormulaError: return "Workbook formulas would show an error.";
                case NotWrittenKind.WouldOverwriteTheTemplate: return "It would write over a template.";
                case NotWrittenKind.Crashed: return "The write stopped on an error.";
                case NotWrittenKind.NoFolderLayout: return CreateBlocked.NoLayout + ".";
                default: return Fallback;
            }
        }
    }
}
