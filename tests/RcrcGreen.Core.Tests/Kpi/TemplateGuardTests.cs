using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// The path comparison the template guard rests on.
    ///
    /// Every expected answer here is written out by hand rather than worked out with the rule
    /// the code uses. The paths are built with the platform's own separator so one set of cases
    /// means the same thing on the Linux runner and on the Windows machine the add-in runs on.
    /// </summary>
    public class FilePathsTests
    {
        private static string At(params string[] parts)
        {
            return Path.DirectorySeparatorChar + string.Join(
                Path.DirectorySeparatorChar.ToString(), parts);
        }

        private static readonly string Template = At("kpi", "templates", "MOSQUES.xlsx");

        [Fact]
        public void ThePathAgainstItselfIsTheSameFile()
        {
            Assert.Equal(SamePath.Same, FilePaths.Compare(Template, Template));
        }

        /// <summary>
        /// Windows compares a path without case, so MOSQUES.xlsx and mosques.XLSX in a folder
        /// spelt either way are one file and a comparison that says otherwise deletes it.
        /// </summary>
        [Fact]
        public void TheSamePathInADifferentCaseIsTheSameFile()
        {
            Assert.Equal(
                SamePath.Same,
                FilePaths.Compare(Template, At("kpi", "Templates", "mosques.XLSX")));
        }

        [Fact]
        public void TheSamePathWithATrailingSeparatorIsTheSameFile()
        {
            Assert.Equal(
                SamePath.Same,
                FilePaths.Compare(Template, Template + Path.DirectorySeparatorChar));
        }

        /// <summary>
        /// The output folder is browsed for and the template folder is remembered, so the two
        /// can name one folder by two routes without a character in common past the root.
        /// </summary>
        [Fact]
        public void TheSameFileReachedThroughADifferentSpellingOfTheFolderIsTheSameFile()
        {
            Assert.Equal(
                SamePath.Same,
                FilePaths.Compare(Template, At("kpi", "output", "..", "templates", "MOSQUES.xlsx")));

            Assert.Equal(
                SamePath.Same,
                FilePaths.Compare(Template, At("kpi", ".", "templates", "MOSQUES.xlsx")));
        }

        /// <summary>
        /// The ordinary case, which must still go through. Writing a differently named workbook
        /// into the templates folder is allowed and is not what this guard is for.
        /// </summary>
        [Fact]
        public void ADifferentFileIsADifferentFile()
        {
            Assert.Equal(
                SamePath.Different,
                FilePaths.Compare(Template, At("kpi", "output", "MOSQUES.xlsx")));

            Assert.Equal(
                SamePath.Different,
                FilePaths.Compare(Template, At("kpi", "templates", "MOSQUES DM-12.xlsx")));
        }

        /// <summary>
        /// A path that cannot be resolved is not a path that is different. **A check that cannot
        /// see its own subject has to refuse**, so this answer is its own and the guard reading
        /// it refuses on Unreadable exactly as it does on Same.
        /// </summary>
        [Fact]
        public void APathThatCannotBeResolvedIsNeitherSameNorDifferent()
        {
            Assert.Equal(SamePath.Unreadable, FilePaths.Compare(Template, null));
            Assert.Equal(SamePath.Unreadable, FilePaths.Compare(Template, string.Empty));
            Assert.Equal(SamePath.Unreadable, FilePaths.Compare(Template, "   "));
            Assert.Equal(SamePath.Unreadable, FilePaths.Compare(null, Template));
            Assert.Equal(SamePath.Unreadable, FilePaths.Compare(null, null));
        }

        [Fact]
        public void TheCanonicalFormLosesATrailingSeparatorAndNothingElse()
        {
            Assert.Equal(
                FilePaths.Canonical(Template),
                FilePaths.Canonical(Template + Path.DirectorySeparatorChar));

            Assert.Equal(string.Empty, FilePaths.Canonical(null));
            Assert.Equal(string.Empty, FilePaths.Canonical("   "));
        }
    }

    /// <summary>
    /// **NOTHING MAY WRITE TO A TEMPLATE.** Two guards, because either alone is one refactor
    /// from being bypassed. This is the Core one, in the patcher, proven on a workbook the test
    /// builds itself. The Revit one, in front of the delete, is the same call on the same
    /// comparison and no test here reaches it.
    /// </summary>
    public class TemplateGuardTests : IDisposable
    {
        private readonly string _folder = WorkbookFixture.Folder();

        public void Dispose()
        {
            try
            {
                Directory.Delete(_folder, true);
            }
            catch (IOException)
            {
            }
        }

        private static CellWrite[] Writes()
        {
            return new[] { CellWrite.Text(WorkbookFixture.MainSheet, "D3", "GR-NG05-DM-11") };
        }

        [Fact]
        public void WritingOverTheTemplateIsRefusedAndTheTemplateIsUntouched()
        {
            string template = WorkbookFixture.Create(_folder, "MOSQUES.xlsx");
            long before = new FileInfo(template).Length;

            PatchOutcome outcome = WorkbookPatcher.Patch(template, template, Writes());

            Assert.False(outcome.Written);
            Assert.Contains("IS the template this run was about to read", outcome.Refusal);
            Assert.Contains(template, outcome.Refusal);
            Assert.Equal(before, new FileInfo(template).Length);
        }

        /// <summary>
        /// The output folder browsed to the templates folder is what makes this reachable, and
        /// the name box is prefilled with the template's own file name, so the two arrive at one
        /// path by two routes.
        /// </summary>
        [Fact]
        public void TheSamePathReachedTwoWaysIsRefusedToo()
        {
            string template = WorkbookFixture.Create(_folder, "MOSQUES.xlsx");
            string roundAbout = Path.Combine(_folder, "..", Path.GetFileName(_folder), "mosques.XLSX");

            PatchOutcome outcome = WorkbookPatcher.Patch(template, roundAbout, Writes());

            Assert.False(outcome.Written);
            Assert.Contains("The tool never writes to a template", outcome.Refusal);
        }

        [Fact]
        public void APathThatCannotBeCheckedIsRefusedRatherThanRisked()
        {
            string template = WorkbookFixture.Create(_folder, "MOSQUES.xlsx");

            PatchOutcome outcome = WorkbookPatcher.Patch(template, "   ", Writes());

            Assert.False(outcome.Written);
            Assert.Contains("could not be checked against the template", outcome.Refusal);
        }

        /// <summary>
        /// A differently named workbook in the same folder still goes through. The guard refuses
        /// one file, not one folder.
        /// </summary>
        [Fact]
        public void ADifferentNameInTheTemplatesFolderStillGoesThrough()
        {
            string template = WorkbookFixture.Create(_folder, "MOSQUES.xlsx");
            string output = Path.Combine(_folder, "MOSQUES DM-12.xlsx");

            PatchOutcome outcome = WorkbookPatcher.Patch(template, output, Writes());

            Assert.True(outcome.Written);
            Assert.True(File.Exists(template));
            Assert.True(File.Exists(output));
        }
    }

    /// <summary>
    /// **A REFUSED RUN MUST SAY WHY.** The assertion is over every shape a run with no file can
    /// take rather than one test per case, because the fault was a path nobody had thought of:
    /// the accounting passed, the patch was refused, and the status line went to the empty
    /// string. Silence after a press reads as success.
    /// </summary>
    public class StatusLineTests
    {
        private const string Report = @"C:\reports\kpi.txt";

        /// <summary>
        /// Two plots reporting one raw area with nobody having confirmed it, which is a real
        /// refusal the accounting gives before anything is copied.
        /// </summary>
        private static KpiCreateRun Refused(PatchOutcome outcome)
        {
            RegionArea shared = CreateFixture.Region(CreateFixture.OutOfScope, 1131.79, 12182.05561411);

            return CreateFixture.Run(
                new[]
                {
                    CreateFixture.Plot("MM-03", regions: new[] { shared }),
                    CreateFixture.Plot("MM-04", regions: new[] { shared })
                },
                outcome: outcome);
        }

        private static KpiCreateRun Accounted(PatchOutcome outcome)
        {
            return CreateFixture.Run(outcome: outcome);
        }

        /// <summary>
        /// Every run that ended without a file, in one list. None of them may answer nothing.
        /// </summary>
        private static IEnumerable<KpiCreateRun> EveryRunThatWroteNothing()
        {
            yield return Refused(null);
            yield return Refused(PatchOutcome.Refused("The folder refused the workbook."));
            yield return Accounted(null);
            yield return Accounted(PatchOutcome.Refused(CreateWords.CouldNotBeWritten("It is in use.")));
            yield return Accounted(PatchOutcome.Refused("   "));
        }

        [Fact]
        public void NoRunThatWroteNothingEndsWithAnEmptyStatusLine()
        {
            foreach (KpiCreateRun run in EveryRunThatWroteNothing())
            {
                Assert.False(run.Wrote);
                Assert.False(string.IsNullOrWhiteSpace(CreateWords.Wrote(run, Report)));
                Assert.Contains(CreateWords.NothingWritten, CreateWords.Wrote(run, Report));
            }
        }

        /// <summary>
        /// The one that used to go blank, written out by hand. The accounting added up and the
        /// patch was refused, which is the output workbook still open in Excel.
        /// </summary>
        [Fact]
        public void AnAccountingThatPassedAndAPatchThatDidNotSaysWhatToDo()
        {
            KpiCreateRun run = Accounted(
                PatchOutcome.Refused(CreateWords.CouldNotBeWritten("The process cannot access the file.")));

            Assert.Equal(
                "Nothing was written. The workbook could not be written. If it is open in Excel, "
                + "close it and press Create again. The process cannot access the file. "
                + "Report: " + Report,
                CreateWords.Wrote(run, Report));
        }

        [Fact]
        public void AnOutcomeCarryingNoReasonSaysThatIsABugRatherThanSayingNothing()
        {
            Assert.Contains(CreateWords.NoReasonRecorded, CreateWords.Wrote(Accounted(null), Report));
            Assert.Contains(
                CreateWords.NoReasonRecorded,
                CreateWords.Wrote(Accounted(PatchOutcome.Refused("   ")), Report));
        }

        /// <summary>
        /// The accounting speaks first, because it refuses before anything is copied.
        ///
        /// **It counts its reasons rather than repeating them.** The pane prints them in red
        /// above the Create button, and the 0928 run showed all four twice on one screen, once
        /// there and once again word for word down here.
        /// </summary>
        [Fact]
        public void AnAccountingThatRefusedIsCountedRatherThanRepeated()
        {
            KpiCreateRun run = Refused(null);
            string said = CreateWords.Wrote(run, Report);

            Assert.Equal(
                "Nothing was written. 1 reason, shown in full above the Create button. "
                + "Report: " + Report,
                said);
            Assert.DoesNotContain(run.Reconciliation.Refusals.First(), said);
        }

        /// <summary>
        /// The plural, written out by hand, and the reasons still absent from the line.
        /// </summary>
        [Fact]
        public void SeveralReasonsAreCountedInThePluralAndStillNotRepeated()
        {
            Assert.Equal(
                "Nothing was written. 4 reasons, shown in full above the Create button.",
                CreateWords.ReasonsAreAbove(4));

            Assert.Equal(
                "Nothing was written. 1 reason, shown in full above the Create button.",
                CreateWords.ReasonsAreAbove(1));
        }

        /// <summary>
        /// The patch's refusal is said in full, because nothing else on the pane carries it.
        /// Only the accounting's own reasons are already on the screen.
        /// </summary>
        [Fact]
        public void APatchRefusalIsStillSaidInFullBecauseThePaneHoldsItNowhereElse()
        {
            string said = CreateWords.Wrote(
                Accounted(PatchOutcome.Refused("The folder refused the workbook.")), Report);

            Assert.Contains("The folder refused the workbook.", said);
        }
    }
}
