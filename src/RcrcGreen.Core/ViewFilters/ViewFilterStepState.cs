using System.Globalization;

namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// One step of the View Filters rail: what it says while it is shut, whether it can be
    /// used, whether it is finished, and the one line the rail cell shows under the
    /// pointer. The same shape as the Drawing Sheet's StepState and deliberately not that
    /// type, because the two panes' steps are two facts and a shared type bent to fit both
    /// would be one record of two.
    /// </summary>
    public sealed class ViewFilterStepState
    {
        internal ViewFilterStepState(
            ViewFilterStep step,
            string title,
            string summary,
            bool usable,
            string whyNot,
            bool done)
        {
            Step = step;
            Title = title ?? string.Empty;
            Summary = summary ?? string.Empty;
            Usable = usable;
            WhyNot = whyNot ?? string.Empty;
            Done = done;
        }

        public ViewFilterStep Step { get; }

        public int Number
        {
            get { return (int)Step; }
        }

        public string Title { get; }

        /// <summary>What the step has to say about its own state, empty when nothing yet.</summary>
        public string Summary { get; }

        public bool Usable { get; }

        /// <summary>One line saying why the step cannot be used, empty when it can.</summary>
        public string WhyNot { get; }

        public bool Done { get; }

        /// <summary>
        /// What the body shows over the open step's controls, so the number, the title and
        /// the state read without hovering anything.
        /// </summary>
        public string Header
        {
            get
            {
                string start = Number.ToString(CultureInfo.InvariantCulture) + "  " + Title;
                return Summary.Length == 0 ? start : start + "   " + Summary;
            }
        }

        /// <summary>
        /// The rail cell's tooltip. The rail shows a number and nothing else, so this is
        /// the only place a cell says what it is: the title, then the summary while the
        /// step is usable, or the reason while it is shut. Never empty, because the title
        /// never is.
        /// </summary>
        public string Tip
        {
            get
            {
                string said = Usable ? Summary : WhyNot;
                return said.Length == 0 ? Title : Title + "   " + said;
            }
        }
    }
}
