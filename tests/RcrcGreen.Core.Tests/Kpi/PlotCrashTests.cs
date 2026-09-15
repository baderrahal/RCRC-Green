using System;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **AFTER A CRASH THE TOOL LEAVES EVERY FILE WHERE IT IS AND THE PLOT'S ROW NAMES THEM.**
    /// Bader's decision of 15 September.
    ///
    /// **THE WIRING IN `KpiRequestHandler` HAS NOT BEEN RUN, AND CANNOT BE RUN FROM HERE**,
    /// because it needs Revit. These are the row and its words, which are Core's, and they are
    /// what a person reads when a press throws on one of 154 plots.
    ///
    /// **EVERY EXPECTED VALUE HERE IS WRITTEN OUT BY HAND.**
    /// </summary>
    public class PlotCrashTests
    {
        private const string Folder = @"C:\RCRC\out\MOSQUES\FRIDAY MOSQUE\ANH-008-MO-100006";

        private const string Workbook =
            @"C:\RCRC\out\MOSQUES\FRIDAY MOSQUE\ANH-008-MO-100006\ANH-008-MO-100006.xlsx";

        private const string Pdf =
            @"C:\RCRC\out\MOSQUES\FRIDAY MOSQUE\ANH-008-MO-100006\ANH-008-MO-100006.pdf";

        /// <summary>
        /// **THE STEP THAT THREW AND EVERY FILE, WITH ITS PATH.** The folder was made, the
        /// workbook was copied, and the patch threw, so the row says so and names the half
        /// written file that is sitting there.
        /// </summary>
        [Fact]
        public void TheRowNamesTheStepThatThrewAndEveryFileOnDisk()
        {
            string row = PlotCrash.Row(
                CreateStep.TheWorkbookPatch,
                "InvalidDataException",
                "End of Central Directory record could not be found.",
                new[]
                {
                    new CrashFile("the folder", Folder, true),
                    new CrashFile("the workbook", Workbook, true),
                    new CrashFile("the PDF", Pdf, false)
                });

            Assert.Equal(
                "the write threw while patching the plot's workbook, and nothing more was "
                + "written for this plot. InvalidDataException: End of Central Directory record "
                + "could not be found.. Nothing is deleted after a crash. Of this plot's own "
                + "files, the folder is on disk at " + Folder
                + ", the workbook is on disk at " + Workbook
                + ", the PDF is not on disk, which would be " + Pdf,
                row);
        }

        /// <summary>
        /// **A THROW BEFORE THE COPY IS A DIFFERENT STEP AND A DIFFERENT LIST.** The folder is
        /// there and the workbook is not, and saying which is the whole point of the row.
        /// </summary>
        [Fact]
        public void AThrowDuringTheCopySaysSoAndNamesTheWorkbookAsNotOnDisk()
        {
            string row = PlotCrash.Row(
                CreateStep.TheWorkbookCopy,
                "IOException",
                "The process cannot access the file because it is being used by another process.",
                new[]
                {
                    new CrashFile("the folder", Folder, true),
                    new CrashFile("the workbook", Workbook, false)
                });

            Assert.StartsWith(
                "the write threw while copying the template to the plot's workbook, and nothing "
                + "more was written for this plot. IOException: ",
                row);
            Assert.Contains("the folder is on disk at " + Folder, row);
            Assert.Contains("the workbook is not on disk, which would be " + Workbook, row);
        }

        /// <summary>
        /// **A THROW BEFORE ANYTHING OF THE PLOT WAS TOUCHED SAYS THAT**, and names no file,
        /// rather than printing a list of three paths that were never used.
        /// </summary>
        [Fact]
        public void AThrowBeforeAnyFileWasTouchedNamesNoFile()
        {
            string row = PlotCrash.Row(
                CreateStep.BeforeAnythingWasWritten, "NullReferenceException", string.Empty, null);

            Assert.Equal(
                "the write threw before any file of this plot was touched, and nothing more "
                + "was written for this plot. NullReferenceException: it carried no "
                + "message. Nothing is deleted after a crash, and nothing recorded a path for "
                + "this plot, so there is no file to name",
                row);
        }

        /// <summary>
        /// **THE PDF STEP IS ITS OWN STEP.** A press that wrote the workbook and threw writing
        /// the PDF leaves a good workbook behind, and the row has to say that rather than
        /// reading as a plot that got nothing.
        /// </summary>
        [Fact]
        public void AThrowWritingThePdfLeavesTheWorkbookAndTheRowSaysSo()
        {
            string row = PlotCrash.Row(
                CreateStep.ThePdf, "UnauthorizedAccessException", "Access to the path is denied.",
                new[]
                {
                    new CrashFile("the folder", Folder, true),
                    new CrashFile("the workbook", Workbook, true),
                    new CrashFile("the PDF", Pdf, false)
                });

            Assert.Contains("the write threw while writing the plot's PDF", row);
            Assert.Contains("the workbook is on disk at " + Workbook, row);
        }

        /// <summary>
        /// A step is required, because a row that does not say where the write got to is the
        /// row this round exists to replace.
        /// </summary>
        [Fact]
        public void ARowWithNoStepIsRefused()
        {
            Assert.Throws<ArgumentNullException>(
                () => PlotCrash.Row(null, "IOException", "it threw", null));
        }
    }
}
