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
        private ReadOfTheModel(bool happened, bool wholeModel, DateTime at, int elements, double seconds)
        {
            Happened = happened;
            WholeModel = wholeModel;
            When = at;
            Elements = elements;
            Seconds = seconds;
        }

        /// <summary>
        /// Nothing has read this model. The header says so and names no number.
        /// </summary>
        public static readonly ReadOfTheModel NotYet =
            new ReadOfTheModel(false, false, DateTime.MinValue, 0, 0.0);

        /// <summary>
        /// The plots read: it counts the elements and reads the sheets and the schedules the
        /// plot list comes off. **IT IS NOT A READ OF THE MODEL** and the header must not say it
        /// is.
        /// </summary>
        public static ReadOfTheModel ThePlots(DateTime when, int elements, double seconds)
        {
            return new ReadOfTheModel(true, false, when, elements, seconds);
        }

        /// <summary>
        /// The scan inside Create, which reads the whole document into nine sections.
        /// </summary>
        public static ReadOfTheModel TheWholeModel(DateTime when, int elements, double seconds)
        {
            return new ReadOfTheModel(true, true, when, elements, seconds);
        }

        public bool Happened { get; }

        /// <summary>
        /// **Which read this number came off.** The 18:15 session showed why it has to travel
        /// with the number: the header read `108733 elements in 0.8 seconds` after the plots
        /// press and `46.2 seconds` after Create's scan, on the same model, and the line printed
        /// the two the same way. 108,733 elements in 0.8 seconds is not a read of a model, and a
        /// line that claims it is makes the honest number beside it unreadable too.
        /// </summary>
        public bool WholeModel { get; }

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
        /// Said after the plots press, which counted the elements and read the plots and nothing
        /// else. **A COUNT BESIDE A MODEL NAME IS A CLAIM THAT THE MODEL WAS READ**, so the line
        /// says what really happened instead.
        /// </summary>
        public const string ThePlotsAndNotTheModel =
            " The plots were read, and the elements counted. The model itself was not read.";

        /// <summary>
        /// The one line that names a count. The time and the seconds go with it because they say
        /// which state of the model the number describes, and **which read produced it goes with
        /// them**, because two different reads set this and 0.8 seconds and 46.2 seconds on one
        /// model are not two readings of one thing.
        /// </summary>
        public static string ReadIn(ReadOfTheModel read)
        {
            if (read == null) throw new ArgumentNullException("read");

            string line = string.Format(
                CultureInfo.InvariantCulture,
                "{0} at {1}, {2} elements in {3} seconds.",
                read.WholeModel ? "Read" : "Counted",
                read.When.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
                read.Elements,
                read.Seconds.ToString("0.0", CultureInfo.InvariantCulture));

            return read.WholeModel ? line : line + ThePlotsAndNotTheModel;
        }
    }
}
