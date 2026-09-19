using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// What the pane shows after a press, in place of step 4's controls.
    ///
    /// **IT IS BUILT OFF `PlotListRows`, THE SAME ROWS THE REPORT'S OWN THE PLOT LIST IS BUILT
    /// FROM.** That is the whole point of it. A second walk over the outcomes beside the button,
    /// counting ready its own way, is two records of one fact, and this pane and that report
    /// would disagree the first time either rule moved. The press reads no differently and the
    /// report prints no differently for this existing.
    ///
    /// **THE REASONS ARE THE REPORT'S OWN WORDS**, word for word, because a pane saying a plot
    /// failed for one reason and a report saying another sends somebody looking for a second
    /// fault that is not there.
    /// </summary>
    public sealed class KpiResults
    {
        private KpiResults(bool listWasSet, IReadOnlyList<PlotListRow> rows)
        {
            ListWasSet = listWasSet;
            Rows = rows ?? new List<PlotListRow>();
        }

        /// <summary>
        /// Whether the press had a plot list file to read. **READY IS A QUESTION ABOUT THE
        /// TEAM'S LIST**, so a press with no list set counts nothing rather than counting nought
        /// of nought, which is the rule the report's own `ready:` count already follows.
        /// </summary>
        public bool ListWasSet { get; }

        /// <summary>Every row of the team's list, in the file's order.</summary>
        public IReadOnlyList<PlotListRow> Rows { get; }

        public int Ready
        {
            get { return Rows.Count(one => one.Ready); }
        }

        public int NotReady
        {
            get { return Rows.Count(one => !one.Ready); }
        }

        /// <summary>
        /// The plots that are not ready, in the list's own order, each with its plot, its UID2
        /// and its reason. **Every one of them**, because a results panel that showed the first
        /// few would be a panel somebody has to open the report behind anyway.
        /// </summary>
        public IReadOnlyList<PlotListRow> NotReadyRows
        {
            get { return Rows.Where(one => !one.Ready).ToList(); }
        }

        public const string NoPlotList =
            "No plot list file was set for this press, so there is no list to count ready "
            + "against. The report says what each ticked plot did.";

        public const string EveryPlotIsReady = "Every plot on the list is ready.";

        public const string ReadyHeading = "ready";

        public const string NotReadyHeading = "not ready";

        /// <summary>
        /// The two counts as one line, for a pane too narrow to be sure two boxes sit side by
        /// side. The pane draws them as two boxes and this is what each box says.
        /// </summary>
        public string CountsInWords
        {
            get
            {
                if (!ListWasSet) return NoPlotList;

                return ReadyHeading + " " + Ready.ToString(CultureInfo.InvariantCulture)
                    + ", " + NotReadyHeading + " "
                    + NotReady.ToString(CultureInfo.InvariantCulture)
                    + ", of " + Rows.Count.ToString(CultureInfo.InvariantCulture)
                    + (Rows.Count == 1 ? " plot on the list" : " plots on the list");
            }
        }

        /// <summary>
        /// The line above the list of plots that are not ready, or the one saying there are
        /// none. **A press where every plot is ready says so**, because a block that disappears
        /// when there is nothing to report reads the same as one nobody wrote.
        /// </summary>
        public string NotReadyInWords
        {
            get
            {
                if (!ListWasSet) return string.Empty;
                if (NotReady == 0) return EveryPlotIsReady;

                return NotReady.ToString(CultureInfo.InvariantCulture)
                    + (NotReady == 1 ? " plot is" : " plots are")
                    + " not ready to send:";
            }
        }

        public static KpiResults Of(KpiCreateRunSet set)
        {
            if (set == null) throw new ArgumentNullException("set");

            PlotListRead list = set.PlotList;
            bool listWasSet = list != null && list.Set && list.Read;

            return new KpiResults(listWasSet, PlotListRows.Of(set));
        }
    }
}
