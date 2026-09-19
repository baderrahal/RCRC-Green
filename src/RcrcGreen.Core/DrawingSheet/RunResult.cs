using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One thing the run did not make, as the pane prints it: what it would have been called
    /// and why it was not made.
    /// </summary>
    public sealed class RunResultLine
    {
        internal RunResultLine(string name, string because, bool leftInTheModel)
        {
            Name = name;
            Because = because;
            LeftInTheModel = leftInTheModel;
        }

        public string Name { get; }

        public string Because { get; }

        /// <summary>
        /// Created wrong, and the delete that should have taken it out was refused. It is in
        /// the model right now, which is why it is marked rather than printed like the rest.
        /// </summary>
        public bool LeftInTheModel { get; }

        /// <summary>
        /// The same shape a refusal has always printed in, so the pane composes nothing of
        /// its own.
        /// </summary>
        public string InWords()
        {
            return Name + ". " + Because;
        }
    }

    /// <summary>
    /// What the pane shows after a run: two counts, one line per thing that did not happen,
    /// and where the report went.
    ///
    /// **It reads <see cref="RunOutcome"/> and never <see cref="RunPlan"/>.** The plan is what
    /// a run intends and the outcome is what it did. The first real run printed its created
    /// sections off the plan and its refusals off the outcome, so the same four plan views
    /// appeared under PLAN VIEWS and under NOT CREATED while the panel said nothing had been
    /// made. That is two records of one fact, and this is the place it bit.
    ///
    /// So the worked count is <see cref="RunOutcome.CreatedCount"/> and the failed count is
    /// <see cref="RunOutcome.NotCreatedCount"/>, and nothing is counted again beside the
    /// control that draws it.
    /// </summary>
    public sealed class RunResult
    {
        private RunResult(
            int worked,
            int failed,
            IReadOnlyList<RunResultLine> failedLines,
            string alarm,
            string loud,
            string where,
            string reportPath,
            string folderPath)
        {
            Worked = worked;
            Failed = failed;
            FailedLines = failedLines;
            Alarm = alarm;
            Loud = loud;
            Where = where;
            ReportPath = reportPath;
            FolderPath = folderPath;
        }

        /// <summary>
        /// Everything here is read off the outcome that was handed to the run, and the paths
        /// are what the report write returned. A run that was never confirmed hands in an
        /// outcome that wrote nothing, and that is still a result worth drawing: zero and zero
        /// with the report line under it says more than an empty area does.
        /// </summary>
        public static RunResult Of(RunOutcome outcome, IEnumerable<string> reportPaths)
        {
            if (outcome == null) throw new ArgumentNullException("outcome");

            List<string> paths = (reportPaths ?? Enumerable.Empty<string>())
                .Where(path => !string.IsNullOrEmpty(path))
                .ToList();

            var lines = new List<RunResultLine>();
            foreach (RunRefusal one in outcome.NotCreated) lines.Add(Line(one, false));
            foreach (RunRefusal one in outcome.LeftBehind) lines.Add(Line(one, true));

            string first = paths.Count == 0 ? string.Empty : paths[0];

            return new RunResult(
                outcome.CreatedCount,
                outcome.NotCreatedCount,
                lines,
                AlarmOver(outcome.BothWays),
                LoudOver(outcome.LeftBehind.Count),
                ReportPlaces.Written(paths),
                first,
                FolderOf(first));
        }

        /// <summary>How many things the run made. <see cref="RunOutcome.CreatedCount"/>.</summary>
        public int Worked { get; }

        /// <summary>
        /// How many it did not. <see cref="RunOutcome.NotCreatedCount"/>, which counts the
        /// refused and the left behind together, the same two lists
        /// <see cref="FailedLines"/> is built from.
        /// </summary>
        public int Failed { get; }

        /// <summary>
        /// The two counts in one line, in the words the status line has always used. One
        /// formatter, so the line under the pane and the line inside it cannot disagree.
        /// </summary>
        public string CountsInWords
        {
            get { return Worked + " created, " + Failed + " not created."; }
        }

        /// <summary>
        /// One per thing that did not happen, the refused first and then the ones left in the
        /// model. There are exactly <see cref="Failed"/> of them.
        /// </summary>
        public IReadOnlyList<RunResultLine> FailedLines { get; }

        /// <summary>
        /// Empty in a run that behaved. When it is not, something was counted as created and
        /// as not created at once, which is a bug in this code rather than anything wrong with
        /// the model, and it goes at the top where it cannot be missed.
        /// </summary>
        public string Alarm { get; }

        /// <summary>
        /// The one thing that gets worse the longer nobody reads it: schedules created wrong
        /// whose delete Revit refused. Empty when there are none.
        /// </summary>
        public string Loud { get; }

        /// <summary>
        /// Where the report went, or why there is none, in
        /// <see cref="ReportPlaces.Written"/>'s own words rather than a second sentence
        /// saying the same thing.
        /// </summary>
        public string Where { get; }

        /// <summary>
        /// Whether there is a file to open. No pointer file beside the add-in means no report
        /// and no folder, and the pane offers neither button.
        /// </summary>
        public bool HasReport
        {
            get { return ReportPath.Length > 0; }
        }

        public string ReportPath { get; }

        /// <summary>
        /// The folder the report landed in, which is the one the reports button opens. Empty
        /// whenever <see cref="ReportPath"/> is.
        /// </summary>
        public string FolderPath { get; }

        private static RunResultLine Line(RunRefusal refusal, bool leftInTheModel)
        {
            string name = (refusal.Name ?? string.Empty).Trim();
            if (name.Length == 0) name = "Something the run did not name";

            string because = (refusal.Because ?? string.Empty).Trim();
            if (because.Length == 0)
            {
                // The category is always worded, including the one for a caller that did not
                // say, so a line can never come out with nothing after its name. The wording
                // is read from the one place reasons are worded rather than written again.
                because = "Refused " + RunSummary.ReasonInWords(refusal.Kind) + ".";
            }

            return new RunResultLine(name, because, leftInTheModel);
        }

        private static string AlarmOver(IReadOnlyList<string> bothWays)
        {
            if (bothWays == null || bothWays.Count == 0) return string.Empty;

            return "THIS IS A BUG IN THE TOOL, NOT IN THE MODEL. "
                + (bothWays.Count == 1
                    ? "1 thing is counted as created and as not created: "
                    : bothWays.Count + " things are counted as created and as not created: ")
                + string.Join(", ", bothWays.ToArray())
                + ". Nothing here can be trusted until that is fixed.";
        }

        private static string LoudOver(int leftBehind)
        {
            if (leftBehind == 0) return string.Empty;

            return leftBehind
                + (leftBehind == 1 ? " WRONG SCHEDULE IS" : " WRONG SCHEDULES ARE")
                + " IN THE MODEL AND MUST BE DELETED BY HAND.";
        }

        /// <summary>
        /// Cut at the last separator, both kinds, rather than asked of Path.GetDirectoryName.
        ///
        /// **That call answers by the platform it is running on.** The tests run on Linux,
        /// where a backslash is an ordinary character, so a real Windows report path came back
        /// with no folder at all and the pane would have offered one button in Revit and two
        /// on the runner. A rule that answers differently where it is checked from where it
        /// runs is not a checked rule.
        ///
        /// Empty when there is no separator, which leaves the pane offering no folder rather
        /// than one it made up.
        /// </summary>
        private static string FolderOf(string path)
        {
            int cut = path.LastIndexOfAny(new[] { '\\', '/' });

            return cut <= 0 ? string.Empty : path.Substring(0, cut);
        }
    }
}
