using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Why a string handed to <see cref="PlotRegistry"/> was not read as a plot.
    /// </summary>
    public enum IgnoredReason
    {
        /// <summary>
        /// It was never shaped like a plot name. Most views in a model land here.
        /// </summary>
        NotAPlotName,

        /// <summary>
        /// It is shaped right and the two letters are not uppercase, so it is a mistyped
        /// plot rather than a plot of its own.
        /// </summary>
        WrongCase
    }

    /// <summary>
    /// One string that did not become a plot, and the reason.
    /// </summary>
    public sealed class IgnoredName
    {
        public IgnoredName(string text, IgnoredReason reason)
        {
            if (text == null) throw new ArgumentNullException("text");

            Text = text;
            Reason = reason;
        }

        public string Text { get; }

        public IgnoredReason Reason { get; }

        public override string ToString()
        {
            if (Reason == IgnoredReason.WrongCase)
            {
                return Text + " (plot identifiers are two uppercase letters)";
            }
            return Text + " (not a plot name)";
        }
    }
}
