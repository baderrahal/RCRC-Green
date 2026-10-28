namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// What the pane draws again when a press's end line arrives.
    /// </summary>
    public static class PressEnd
    {
        /// <summary>
        /// Whether the steps are drawn again, rather than the status line alone. A read that
        /// was refused drew Read model grey when it started, so painting the status line alone
        /// left the button grey until somebody clicked a step circle, which is question 6.
        /// </summary>
        public static bool RedrawsTheSteps(bool createWasRunning, bool readWasRunning)
        {
            return createWasRunning || readWasRunning;
        }
    }
}
