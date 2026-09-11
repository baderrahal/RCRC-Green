using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// Finding 16. The pane opened and peeked every .xlsx in the templates folder on every
    /// redraw, and every tick redraws: seven zips for each of 155 ticks, 1,085 opens. The
    /// folder's workbooks are held once per folder now, keyed on the folder, cleared when it
    /// changes, and the opens are counted into the report so it cannot come back unnoticed.
    /// Every expected value written out by hand, and the opens counted by a fake so no disk is
    /// touched.
    /// </summary>
    public class TemplateListingTests
    {
        private const string Folder = @"C:\clients\GRP\templates";

        private static string[] Seven()
        {
            return new[] { "a", "b", "c", "d", "e", "f", "g" }.Select(one => Folder + "\\" + one + ".xlsx").ToArray();
        }

        private sealed class Counting
        {
            public int Opens;

            // The name after the last backslash by hand, because the gate runs on Linux where
            // the path helper does not split on one, and the pane runs on Windows where it does.
            public RecognisedWorkbook Recognise(string path)
            {
                Opens++;
                return RecognisedWorkbook.Recognise(path.Substring(path.LastIndexOf('\\') + 1), new[] { "<Mosques>" }, null, null);
            }
        }

        [Fact]
        public void AFreshFolderOpensEveryWorkbookOnce()
        {
            var counting = new Counting();

            TemplateListing listing = TemplateListing.Nothing.For(Folder, Seven(), counting.Recognise);

            Assert.Equal(7, listing.Opened);
            Assert.Equal(1, listing.Drawn);
            Assert.Equal(7, listing.Workbooks.Count);
            Assert.Equal(7, counting.Opens);
            Assert.False(listing.IsNothing);
            Assert.Equal("a.xlsx", listing.Workbooks[0].FileName);
        }

        [Fact]
        public void ASecondDrawOfTheSameFolderOpensNothing()
        {
            var counting = new Counting();

            TemplateListing first = TemplateListing.Nothing.For(Folder, Seven(), counting.Recognise);
            TemplateListing second = first.For(Folder, Seven(), counting.Recognise);

            Assert.Equal(7, second.Opened);
            Assert.Equal(2, second.Drawn);
            Assert.Equal(7, counting.Opens);
            for (int at = 0; at < 7; at++) Assert.Same(first.Workbooks[at], second.Workbooks[at]);
        }

        /// <summary>
        /// The number the audit measured against: 155 ticks over seven files was 1,085 opens.
        /// </summary>
        [Fact]
        public void OneHundredAndFiftyFiveDrawsOpenSevenFiles()
        {
            var counting = new Counting();
            TemplateListing listing = TemplateListing.Nothing;

            for (int draw = 0; draw < 155; draw++) listing = listing.For(Folder, Seven(), counting.Recognise);

            Assert.Equal(7, listing.Opened);
            Assert.Equal(155, listing.Drawn);
            Assert.Equal(7, counting.Opens);
        }

        [Fact]
        public void ANewFileInTheFolderIsOpenedOnceAndTheRestAreNot()
        {
            var counting = new Counting();
            TemplateListing listing = TemplateListing.Nothing.For(Folder, Seven(), counting.Recognise);

            listing = listing.For(Folder, Seven().Concat(new[] { Folder + "\\h.xlsx" }).ToArray(), counting.Recognise);

            Assert.Equal(8, listing.Opened);
            Assert.Equal(2, listing.Drawn);
            Assert.Equal(8, listing.Workbooks.Count);
            Assert.Equal(8, counting.Opens);
        }

        [Fact]
        public void AFileGoneFromTheFolderIsDropped()
        {
            var counting = new Counting();
            TemplateListing listing = TemplateListing.Nothing.For(Folder, Seven(), counting.Recognise);

            listing = listing.For(Folder, Seven().Take(6).ToArray(), counting.Recognise);

            Assert.Equal(6, listing.Workbooks.Count);
            Assert.Equal(7, listing.Opened);
            Assert.DoesNotContain(listing.Workbooks, one => one.FileName == "g.xlsx");
        }

        [Fact]
        public void ADifferentFolderStartsAgain()
        {
            var counting = new Counting();
            TemplateListing listing = TemplateListing.Nothing.For(Folder, Seven(), counting.Recognise);

            listing = listing.For(@"C:\elsewhere", new[] { @"C:\elsewhere\x.xlsx", @"C:\elsewhere\y.xlsx", @"C:\elsewhere\z.xlsx" }, counting.Recognise);

            Assert.Equal(3, listing.Opened);
            Assert.Equal(1, listing.Drawn);
            Assert.Equal(@"C:\elsewhere", listing.Folder);
            Assert.Equal(10, counting.Opens);
        }

        /// <summary>
        /// The folder is compared the way Windows compares a path, without case and with a
        /// trailing separator off.
        /// </summary>
        [Fact]
        public void TheSameFolderInAnotherCaseOrWithATrailingSeparatorIsTheSameFolder()
        {
            var counting = new Counting();
            TemplateListing listing = TemplateListing.Nothing.For(Folder, Seven(), counting.Recognise);

            listing = listing.For(Folder.ToUpperInvariant() + "\\", Seven(), counting.Recognise);

            Assert.Equal(7, listing.Opened);
            Assert.Equal(2, listing.Drawn);
            Assert.True(listing.IsFor(Folder));
        }

        [Fact]
        public void NothingIsForNoFolder()
        {
            Assert.True(TemplateListing.Nothing.IsNothing);
            Assert.Equal(0, TemplateListing.Nothing.Opened);
            Assert.Equal(0, TemplateListing.Nothing.Drawn);
            Assert.False(TemplateListing.Nothing.IsFor(string.Empty));
            Assert.False(TemplateListing.Nothing.IsFor(Folder));
        }

        [Fact]
        public void TheWordsSayTheCount()
        {
            Assert.Equal("7 workbooks in the folder, opened 7 times over 158 redraws since the folder was listed.", TemplateWords.Opened(7, 7, 158));
            Assert.Equal("1 workbook in the folder, opened 1 time over 1 redraw since the folder was listed.", TemplateWords.Opened(1, 1, 1));
        }

        /// <summary>
        /// The count reaches the report, and a run nothing recorded prints NOT LISTED rather
        /// than nought. Removing the plumbing anywhere along the way turns this red.
        /// </summary>
        [Fact]
        public void TheReportSaysHowOftenTheTemplatesWereOpened()
        {
            var counting = new Counting();
            TemplateListing listing = TemplateListing.Nothing;
            for (int draw = 0; draw < 158; draw++) listing = listing.For(Folder, Seven(), counting.Recognise);

            string counted = KpiCreateReport.Write(CreateFixture.Run(templatesListed: listing), new DateTime(2026, 9, 11, 9, 0, 0));
            Assert.Contains("  templates folder: 7 workbooks in the folder, opened 7 times over 158 redraws since the folder was listed.\r\n", counted);

            string unrecorded = KpiCreateReport.Write(CreateFixture.Run(), new DateTime(2026, 9, 11, 9, 0, 0));
            Assert.Contains("  templates folder: NOT LISTED. Nothing recorded how many times a workbook was opened.\r\n", unrecorded);
            Assert.DoesNotContain("opened 0 times", unrecorded);
        }
    }

    /// <summary>
    /// Finding 9. The plot list is a new viewer on every redraw and every tick redraws, so 155
    /// tick boxes threw themselves back to the top on every tick. The rule half of the
    /// remembered scroll, the WPF wiring apart: a note on every scroll, and a restore wanted
    /// only when something above the top was noted.
    /// </summary>
    public class ScrollMemoryTests
    {
        [Fact]
        public void ANotedOffsetComesBackByName()
        {
            var memory = new ScrollMemory();
            memory.Note("plots", 120.0, 0.0);

            double down;
            double across;
            Assert.True(memory.Wants("plots", out down, out across));
            Assert.Equal(120.0, down);
            Assert.Equal(0.0, across);
        }

        [Fact]
        public void NothingNotedWantsNothing()
        {
            double down;
            double across;
            Assert.False(new ScrollMemory().Wants("plots", out down, out across));
        }

        /// <summary>
        /// Restoring nought is a scroll to the top that reads as though it worked.
        /// </summary>
        [Fact]
        public void NoughtIsNotWorthRestoring()
        {
            var memory = new ScrollMemory();
            memory.Note("plots", 0.0, 0.0);

            double down;
            double across;
            Assert.False(memory.Wants("plots", out down, out across));
        }

        [Fact]
        public void TheLatestNoteWinsAndScrollingBackToTheTopForgetsIt()
        {
            var memory = new ScrollMemory();
            memory.Note("plots", 120.0, 0.0);
            memory.Note("plots", 40.0, 0.0);

            double down;
            double across;
            Assert.True(memory.Wants("plots", out down, out across));
            Assert.Equal(40.0, down);

            memory.Note("plots", 0.0, 0.0);
            Assert.False(memory.Wants("plots", out down, out across));
        }

        [Fact]
        public void ListsAreKeptApartAndNamesCompareExactly()
        {
            var memory = new ScrollMemory();
            memory.Note("plots", 120.0, 0.0);

            double down;
            double across;
            Assert.False(memory.Wants("view types", out down, out across));
            Assert.False(memory.Wants("Plots", out down, out across));
            Assert.True(memory.Wants("plots", out down, out across));
        }
    }
}
