using System;
using System.Collections.Generic;
using System.Globalization;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Whether this model has been read yet, and what that read found. **A read that has not
    /// happened is an ABSENCE and not a zero**, so it carries no count at all rather than a
    /// count of nought.
    /// </summary>
    public sealed class ReadOfTheModel
    {
        private ReadOfTheModel(bool happened, DateTime at, int elements, double seconds)
        {
            Happened = happened;
            When = at;
            Elements = elements;
            Seconds = seconds;
        }

        /// <summary>
        /// Nothing has read this model. The header says so and names no number.
        /// </summary>
        public static readonly ReadOfTheModel NotYet =
            new ReadOfTheModel(false, DateTime.MinValue, 0, 0.0);

        public static ReadOfTheModel At(DateTime when, int elements, double seconds)
        {
            return new ReadOfTheModel(true, when, elements, seconds);
        }

        public bool Happened { get; }

        public DateTime When { get; }

        public int Elements { get; }

        public double Seconds { get; }
    }

    /// <summary>
    /// The two lines at the top of the KPI pane, decided here so the pane formats neither.
    ///
    /// **THE HEADER MUST COST NOTHING.** The model's name is free to read. The element count is
    /// not: counting 96,959 elements IS the read, and the round that put the count in the header
    /// paid for it by reading the model every time the pane was shown. A dockable pane is
    /// restored VISIBLE at Revit startup, so that read fired on every model anybody opened, with
    /// no press behind it, and on NG05 it held the model for minutes.
    ///
    /// Three states, one line each, and **only the third names a count**:
    ///
    /// <code>
    /// no document open          No model open            the name is all there is to say
    /// open, nothing read        the model's name         Not read yet, and why no count
    /// open and read             the model's name         Read at 08:37, 96,959 elements
    /// </code>
    /// </summary>
    public static class KpiHeader
    {
        public const string NoModel = KpiPaneWords.NoModelName;

        /// <summary>
        /// Said under the name of a model nothing has read. **It must not say zero**, because
        /// zero is a number and this is an absence, and it says why the count is missing rather
        /// than leaving the line looking like a read that found nothing.
        /// </summary>
        public const string NothingReadYet =
            "Not read yet. Counting every element is the read itself, so the count waits for one.";

        /// <summary>
        /// Said beside the name when no document is open at all, so the line under the name is
        /// never the one about a read that has not happened to a model that does not exist.
        /// </summary>
        public const string OpenOne =
            "Open a model. This pane reads its name, which costs nothing, and nothing else.";

        public static IReadOnlyList<string> Lines(OpenModel model, ReadOfTheModel read)
        {
            OpenModel open = model ?? OpenModel.Nothing;
            ReadOfTheModel held = read ?? ReadOfTheModel.NotYet;

            if (!open.IsOpen) return new List<string> { NoModel, OpenOne };

            if (!held.Happened) return new List<string> { open.Title, NothingReadYet };

            return new List<string> { open.Title, ReadIn(held) };
        }

        /// <summary>
        /// The one line that names a count. The time and the seconds go with it because they
        /// say which state of the model the number describes.
        /// </summary>
        public static string ReadIn(ReadOfTheModel read)
        {
            if (read == null) throw new ArgumentNullException("read");

            return string.Format(
                CultureInfo.InvariantCulture,
                "Read at {0}, {1} elements in {2} seconds.",
                read.When.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
                read.Elements,
                read.Seconds.ToString("0.0", CultureInfo.InvariantCulture));
        }
    }
}
