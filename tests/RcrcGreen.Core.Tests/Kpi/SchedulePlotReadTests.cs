using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **AUDIT 4 FINDING 65. A THROW WHILE READING A SCHEDULE'S PLOT FILTER WAS ANSWERED WITH AN
    /// EMPTY STRING, AND AN EMPTY STRING MEANS THE SCHEDULE BELONGS TO NO PLOT.** Three things
    /// followed and none was visible: the schedule was skipped so the plot read as holding no
    /// softscape and no shrubs and lawn schedule, the guard that refuses a plot holding two of a
    /// kind could not fire because the schedule was never counted, and the reconciliation said
    /// the schedule listed no species, which is a sentence about the MODEL, while the workbook
    /// went out with that plot's trees missing.
    ///
    /// **THE CALL THAT THROWS IS REVIT'S AND CANNOT BE RUN HERE.** These pin the rule and the
    /// words. What stays untested is that `KpiPlotReader.PlotFilteredOn`'s two catches really
    /// produce this refusal, because nothing in this repository can make Revit's own
    /// `ScheduleDefinition` throw. One press on a model holding a schedule whose definition Revit
    /// refuses is what would settle it.
    ///
    /// **EVERY EXPECTED VALUE HERE IS WRITTEN OUT BY HAND.**
    /// </summary>
    public class SchedulePlotReadTests
    {
        private const string Softscape = "DM-12-(600) SOFTSCAPE SCHEDULE";

        /// <summary>
        /// **A REFUSAL BELONGS TO NO PLOT AND SAYS SO.** It used to read exactly like a schedule
        /// that really filters on nothing.
        /// </summary>
        [Fact]
        public void ARefusedReadBelongsToNoPlotAndIsNotAnEmptyValue()
        {
            SchedulePlotRead refused = SchedulePlotRead.Refused(
                Softscape,
                SchedulePlotReads.ThrewReading(
                    "ApplicationException", "The schedule definition is not available."));

            SchedulePlotRead none = SchedulePlotRead.Of(Softscape, string.Empty);
            SchedulePlotRead read = SchedulePlotRead.Of(Softscape, "DM-12");

            // **THE TWO USED TO BE ONE ANSWER.** A failure reading `expected false, got true`
            // says nothing about the plot whose trees went missing because of it.
            Assert.False(
                refused.Is("DM-12"),
                "a read that THREW answered as though the schedule filters on DM-12. A throw and "
                + "a schedule that really names no plot both came back as an empty string before "
                + "this, so the schedule was skipped, the plot read as holding no softscape "
                + "schedule at all, and the reconciliation said it listed no species.");

            Assert.False(refused.Read);
            Assert.True(none.Read);
            Assert.False(none.Is("DM-12"));
            Assert.True(read.Is("DM-12"));
            Assert.False(read.Is("DM-13"));
        }

        /// <summary>
        /// **THE REFUSAL NAMES THE EXCEPTION'S OWN TYPE AND MESSAGE**, the same as `GuardedRead`'s,
        /// because a reason saying only that something went wrong sends somebody to look at the
        /// wrong thing.
        /// </summary>
        [Fact]
        public void TheRefusalNamesTheScheduleTheTypeAndTheMessage()
        {
            SchedulePlotRead refused = SchedulePlotRead.Refused(
                Softscape,
                SchedulePlotReads.ThrewReading(
                    "InvalidOperationException", "The schedule is not a valid view."));

            // **THE MESSAGE'S OWN FULL STOP IS NOT DOUBLED.** Revit's messages usually end in
            // one, and `is not a valid view..` reads as a typo in a report a client may see.
            Assert.Equal(
                "DM-12-(600) SOFTSCAPE SCHEDULE: reading which plot this schedule filters on "
                + "threw InvalidOperationException, The schedule is not a valid view. Nothing "
                + "says which plot it belongs to, so it is neither counted for a plot nor read "
                + "as belonging to none",
                refused.InWords);

            // And a message with no stop of its own gets one, so the two sentences never run
            // together.
            Assert.Contains(
                "threw ApplicationException, Revit would not do that now. Nothing says which",
                SchedulePlotReads.ThrewReading("ApplicationException", "Revit would not do that now"));
        }

        /// <summary>
        /// **A REFUSAL NEEDS A REASON.** A refusal with none is the empty string this whole rule
        /// exists to stop being mistaken for an answer.
        /// </summary>
        [Fact]
        public void ARefusalWithNoReasonIsRefusedOutright()
        {
            Assert.Throws<System.ArgumentException>(
                () => SchedulePlotRead.Refused(Softscape, string.Empty));
        }

        /// <summary>
        /// **THE SCHEDULE HALF OF THE PLOT LIST NAMES WHAT IT COULD NOT READ RATHER THAN DROPPING
        /// IT.** The plots really named are the plots, and the refusal is a line of its own.
        /// </summary>
        [Fact]
        public void ThePlotsNamedLeaveOutTheRefusalsAndTheLineNamesThem()
        {
            var reads = new List<SchedulePlotRead>
            {
                SchedulePlotRead.Of("DM-11-(600) SOFTSCAPE SCHEDULE", "DM-11"),
                SchedulePlotRead.Refused(
                    Softscape,
                    SchedulePlotReads.ThrewReading("ApplicationException", "Revit refused that.")),
                SchedulePlotRead.Of("DM-13-(600) SOFTSCAPE SCHEDULE", string.Empty)
            };

            Assert.Equal(new[] { "DM-11" }, SchedulePlotReads.PlotsNamed(reads).ToArray());

            Assert.Equal(
                new[] { Softscape },
                SchedulePlotReads.Refused(reads).Select(one => one.ScheduleName).ToArray());

            Assert.Equal(
                "THE SCHEDULES WHOSE PLOT COULD NOT BE READ: 1 schedule threw while being asked "
                + "which plot they filter on, so the plots they belong to are UNKNOWN and no "
                + "workbook is written off them. DM-12-(600) SOFTSCAPE SCHEDULE.",
                SchedulePlotReads.InWords(reads));
        }

        /// <summary>
        /// **AND THE SCHEDULE HALF OF THE PLOT LIST CARRIES THE REFUSAL.** The plot list is the
        /// union of the sheets and the schedules, and a schedule the read could not ask used to
        /// leave it in silence, which is what made the line about a plot on a schedule and on no
        /// sheet wrong about it.
        /// </summary>
        [Fact]
        public void ThePlotListCarriesTheSchedulesItCouldNotReadAndNotAsAPlot()
        {
            var reads = new List<SchedulePlotRead>
            {
                SchedulePlotRead.Of("DM-11-(600) SOFTSCAPE SCHEDULE", "DM-11"),
                SchedulePlotRead.Refused(
                    Softscape,
                    SchedulePlotReads.ThrewReading("ApplicationException", "Revit refused that."))
            };

            PlotsInTheModel plots = PlotsInTheModel.Of(
                new[] { "DM-11", "DM-12" },
                SchedulePlotReads.PlotsNamed(reads),
                SchedulePlotReads.Refused(reads).Select(one => one.InWords).ToList());

            // **THE REFUSAL IS NOT A PLOT.** A line that became one would invent a plot, which is
            // the rule in CLAUDE.md, and the plot it invented would be a sentence about a throw.
            Assert.Equal(new[] { "DM-11", "DM-12" }, plots.All.ToArray());
            Assert.Equal(new[] { "DM-12" }, plots.OnSheetsOnly.ToArray());

            Assert.Equal(
                new[]
                {
                    "DM-12-(600) SOFTSCAPE SCHEDULE: reading which plot this schedule filters on "
                    + "threw ApplicationException, Revit refused that. Nothing says which plot it "
                    + "belongs to, so it is neither counted for a plot nor read as belonging to "
                    + "none"
                },
                plots.SchedulesNotRead.ToArray());
        }

        /// <summary>
        /// **A READ THAT ASKED EVERY SCHEDULE CARRIES NONE**, and so does a list built by hand,
        /// so nothing anywhere has to check for a null.
        /// </summary>
        [Fact]
        public void APlotListBuiltWithNoRefusalsCarriesNone()
        {
            Assert.Empty(PlotsInTheModel.Of(new[] { "DM-11" }, new[] { "DM-11" }).SchedulesNotRead);
            Assert.Empty(PlotsInTheModel.Of(null, null).SchedulesNotRead);
        }

        /// <summary>
        /// **A PRESS THAT READ EVERY ONE SAYS SO**, because a line that disappears when there is
        /// nothing to report reads the same as one nobody wrote.
        /// </summary>
        [Fact]
        public void APressThatReadEveryScheduleSaysSo()
        {
            Assert.Equal(
                "THE SCHEDULES WHOSE PLOT COULD NOT BE READ: every schedule this read walked "
                + "named the plot it filters on, or named none.",
                SchedulePlotReads.InWords(new List<SchedulePlotRead>()));
        }
    }
}
