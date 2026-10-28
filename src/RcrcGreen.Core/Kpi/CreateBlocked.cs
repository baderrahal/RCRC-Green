namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The one reason Create cannot run, for the status line under a greyed button.
    ///
    /// **ONE REASON AND A FIXED ORDER.** The old line listed every missing thing in one
    /// sentence, which was right for a refusal after a press and too long for a line under a
    /// button. The order is the order a person fixes them in, so the reason shown is always the
    /// next thing to do. The full refusal is still `CreateWords.CannotCreate`, which the
    /// handler asks again at the press against the live document.
    ///
    /// **AN OUTPUT FOLDER THAT IS THE TEMPLATES FOLDER IS NOT HERE.** The press is allowed and
    /// the per file guard refuses only a name matching a template's, which the log recorded as
    /// a decision, so the pane says it in a warning box that stops nothing.
    /// </summary>
    public static class CreateBlocked
    {
        public const string NoModelOpen = "No model open";
        public const string NoTemplatesFolder = "Templates folder not set";
        public const string NoOutputFolder = "Output folder not set";
        public const string ModelNotRead = "Model not read";
        public const string NoAssetType = "No asset type selected";
        public const string NoPlot = "No plot selected";
        public const string Refused = "Create refused. See the tooltip.";

        /// <summary>
        /// The first reason in the fixed order, or empty where Create can run.
        /// <paramref name="otherRefusal"/> is `CreateWords.CannotCreate`'s answer where the
        /// caller has one: the handler passes it at the press, so a refusal that reached it
        /// still has a short line. `KpiSteps.Of` passes nothing, because every refusal
        /// `CannotCreate` makes today is one of the six above, which a test holds.
        /// </summary>
        public static string Reason(
            bool modelOpen,
            bool templatesFolder,
            bool outputFolder,
            bool modelRead,
            bool assetTypeSelected,
            bool plotSelected,
            string otherRefusal)
        {
            if (!modelOpen) return NoModelOpen;
            if (!templatesFolder) return NoTemplatesFolder;
            if (!outputFolder) return NoOutputFolder;
            if (!modelRead) return ModelNotRead;
            if (!assetTypeSelected) return NoAssetType;
            if (!plotSelected) return NoPlot;
            if (!string.IsNullOrWhiteSpace(otherRefusal)) return Refused;

            return string.Empty;
        }
    }
}
