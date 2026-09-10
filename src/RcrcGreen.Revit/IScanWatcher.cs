namespace RcrcGreen.Revit
{
    /// <summary>
    /// Lets a long read say how far it has got and be told to stop.
    ///
    /// The model scanner and the scope box scanner both take one. The panel hands them a
    /// watcher that never stops anything, because the read it runs came back in 1.4 seconds
    /// over 96,934 elements on the first real model and has no Cancel button. The progress
    /// window that once implemented this went with the ribbon buttons that showed it.
    /// </summary>
    internal interface IScanWatcher
    {
        /// <summary>
        /// True once the user has asked to stop. The loop checks it and gives up.
        /// </summary>
        bool Cancelled { get; }

        void Report(int done, int total);
    }
}
