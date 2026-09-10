namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// How long one press of Create took, and how much of that was reading the model.
    ///
    /// **The 0928 run over 20 plots took about five minutes and no file recorded a duration.**
    /// The checklist report carried one timestamp, Written, and nothing else, so the only thing
    /// anybody could say about the slowest thing this tool does was that it felt slow. The scan
    /// report has carried elements and seconds at the top since its first round and this is the
    /// same line for the other half of the tool.
    ///
    /// It records rather than decides. Nothing here reads a duration and changes what the run
    /// does, and the two numbers are what a person holds against the next run.
    /// </summary>
    public sealed class RunTiming
    {
        private RunTiming(bool wasTimed, double totalSeconds, double readSeconds)
        {
            WasTimed = wasTimed;
            TotalSeconds = totalSeconds;
            ReadSeconds = readSeconds;
        }

        /// <summary>
        /// What a run that nothing timed carries, so a report never prints nought seconds for a
        /// run that took five minutes. A zero reads as an answer.
        /// </summary>
        public static readonly RunTiming NotTimed = new RunTiming(false, 0.0, 0.0);

        public static RunTiming Of(double totalSeconds, double readSeconds)
        {
            return new RunTiming(true, totalSeconds, readSeconds);
        }

        public bool WasTimed { get; }

        /// <summary>
        /// The whole press, from the refusal check to the report being handed back.
        /// </summary>
        public double TotalSeconds { get; }

        /// <summary>
        /// Reading the model, apart from the merging, the accounting and the writing. It is the
        /// part that grows with the number of plots ticked.
        /// </summary>
        public double ReadSeconds { get; }

        /// <summary>
        /// Everything after the read: the merge, the accounting, the species matching, the copy
        /// and the patch. Printed beside the read so the two can be compared rather than one
        /// being assumed to be the whole of it.
        /// </summary>
        public double RestSeconds
        {
            get { return TotalSeconds - ReadSeconds; }
        }
    }
}
