using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// A schedule type that exists in the model and cannot be captured into a usable
    /// definition, with the reason in the words the refusal prints.
    ///
    /// The run used to answer every one of these with no plot in this model has that
    /// schedule, which is false for all of them: the type came from a schedule that plainly
    /// exists. The reason travels with the type now, so a schedule that filters on no plot
    /// and one whose filter could not be read at capture each send the user to the right
    /// place. The panel's plan preview and the run's plan read the same list, so the two can
    /// never promise different schedules.
    /// </summary>
    public sealed class UncapturableSchedule
    {
        public UncapturableSchedule(ViewType type, string why)
        {
            if (type == null) throw new ArgumentNullException("type");

            Type = type;
            Why = why ?? string.Empty;
        }

        public ViewType Type { get; }

        public string Why { get; }

        public override string ToString()
        {
            return Type + ", " + Why;
        }
    }
}
