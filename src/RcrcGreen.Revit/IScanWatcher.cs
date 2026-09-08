namespace RcrcGreen.Revit
{
    /// <summary>
    /// Lets a long read say how far it has got and be told to stop.
    ///
    /// Both commands walk every element or every view in the document. On a large model that
    /// is a window that has stopped responding with no number on it and no way out, which is
    /// indistinguishable from a crash.
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
