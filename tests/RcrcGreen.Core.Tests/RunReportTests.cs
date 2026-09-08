using System;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// The report is the only record a person reads afterwards, so what it says about a thing
    /// left in the model is worth more than the thing itself.
    /// </summary>
    public class RunReportTests
    {
        private static readonly ViewType Hardscape = new ViewType("600", "HARDSCAPE SCHEDULE");
        private static readonly ViewType General = new ViewType("200", "General Arrangement Layout");

        private static readonly DateTime When = new DateTime(2026, 9, 8, 14, 7, 0);

        private static RunPlan OnePlanView()
        {
            return RunPlan.Of(
                new[] { new PlotViewKey("DM-14", General) },
                new[] { "DM-14" },
                new[] { "DM-14" },
                new ViewType[0],
                new ViewType[0]);
        }

        private static string Report(params RunRefusal[] leftBehind)
        {
            return RunReport.Write(
                OnePlanView(),
                "RCRC_NG05",
                When,
                true,
                new RunRefusal[0],
                new RunRefusal[0],
                leftBehind);
        }

        [Fact]
        public void AScheduleStillInTheModelIsNamedUnderItsOwnHeading()
        {
            string written = Report(new RunRefusal(
                "DM-14", Hardscape, "STILL IN THE MODEL, named DM-14-(600) HARDSCAPE SCHEDULE."));

            Assert.Contains("CREATED WRONG AND STILL IN THE MODEL, DELETE BY HAND, 1", written);
            Assert.Contains("  DM-14 (600) HARDSCAPE SCHEDULE. STILL IN THE MODEL, named", written);
        }

        /// <summary>
        /// The banner is above the sections. Somebody who stops reading after the first screen
        /// still finds out, which is the whole reason it is there.
        /// </summary>
        [Fact]
        public void TheBannerComesBeforeTheSectionThatNamesIt()
        {
            string written = Report(new RunRefusal("DM-14", Hardscape, "STILL IN THE MODEL."));

            int banner = written.IndexOf("READ THIS FIRST", StringComparison.Ordinal);
            int section = written.IndexOf(
                "CREATED WRONG AND STILL IN THE MODEL, DELETE BY HAND", StringComparison.Ordinal);

            Assert.True(banner > 0);
            Assert.True(banner < section);
        }

        [Fact]
        public void OneReadsAsOneAndTwoReadAsTwo()
        {
            Assert.Contains(
                "READ THIS FIRST. 1 wrong schedule is in the model and could not",
                Report(new RunRefusal("DM-14", Hardscape, "STILL IN THE MODEL.")));

            Assert.Contains(
                "READ THIS FIRST. 2 wrong schedules are in the model and could not",
                Report(
                    new RunRefusal("DM-14", Hardscape, "STILL IN THE MODEL."),
                    new RunRefusal("DM-15", Hardscape, "STILL IN THE MODEL.")));
        }

        /// <summary>
        /// The usual run. No banner at all, because a warning that shows every time is a
        /// warning nobody reads on the day it means something.
        /// </summary>
        [Fact]
        public void ARunThatLeftNothingBehindHasNoBanner()
        {
            string written = Report();

            Assert.DoesNotContain("READ THIS FIRST", written);
            Assert.Contains("CREATED WRONG AND STILL IN THE MODEL, DELETE BY HAND, 0", written);
            Assert.Contains("  none", written);
        }

        [Fact]
        public void ANullListIsTreatedAsNothingLeftBehind()
        {
            string written = RunReport.Write(
                OnePlanView(), "RCRC_NG05", When, true, null, null, null);

            Assert.DoesNotContain("READ THIS FIRST", written);
        }

        /// <summary>
        /// The closing note used to say a schedule that lost a filter is never created. That
        /// stopped being true the moment the delete could fail, and a report that explains the
        /// wrong rule is worse than one that explains none.
        /// </summary>
        [Fact]
        public void TheClosingNoteSaysWhatHappensWhenTheDeleteIsRefused()
        {
            string written = Report();

            Assert.Contains("deleted again inside the same", written);
            Assert.Contains("When Revit refuses that delete, the schedule is in", written);
            Assert.DoesNotContain("is not created at all", written);
        }

        [Fact]
        public void TheModelAndTheTimeAreAtTheTop()
        {
            string[] lines = Report().Split('\n').Select(one => one.TrimEnd('\r')).ToArray();

            Assert.Equal("RCRC GREEN, DRAWING SHEET RUN", lines[0]);
            Assert.Equal("RCRC_NG05", lines[1]);
            Assert.Equal("2026-09-08 14:07:00", lines[2]);
        }
    }
}
