using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One step of writing a plot, as the row names it. **The step is RECORDED AS IT IS REACHED
    /// rather than worked out afterwards from what is on disk**, which is the rule this
    /// repository keeps paying for: a setting nobody recorded cannot be argued about.
    /// </summary>
    public sealed class CreateStep
    {
        private CreateStep(string inWords)
        {
            InWords = inWords;
        }

        /// <summary>Nothing of this plot had been touched, so no file of its own can exist.</summary>
        public static readonly CreateStep BeforeAnythingWasWritten =
            new CreateStep("before any file of this plot was touched");

        public static readonly CreateStep TheFolder =
            new CreateStep("while making the plot's folder");

        public static readonly CreateStep TheWorkbookCopy =
            new CreateStep("while copying the template to the plot's workbook");

        public static readonly CreateStep TheWorkbookPatch =
            new CreateStep("while patching the plot's workbook");

        public static readonly CreateStep ThePdf = new CreateStep("while writing the plot's PDF");

        /// <summary>
        /// The whole clause that follows `the write threw`, its own `while` included, because
        /// the first step does not take one and a caller adding the word would have to know
        /// which step it was holding.
        /// </summary>
        public string InWords { get; }
    }

    /// <summary>
    /// One file of the plot's own, as the disk holds it after the crash.
    /// </summary>
    public sealed class CrashFile
    {
        public CrashFile(string what, string path, bool onDisk)
        {
            What = (what ?? string.Empty).Trim();
            Path = (path ?? string.Empty).Trim();
            OnDisk = onDisk;
        }

        /// <summary>The folder, the workbook or the PDF.</summary>
        public string What { get; }

        public string Path { get; }

        public bool OnDisk { get; }

        public string InWords
        {
            get
            {
                return What + " " + (OnDisk ? "is on disk at " : "is not on disk, which would be ")
                    + (Path.Length == 0 ? "a path nothing recorded" : Path);
            }
        }
    }

    /// <summary>
    /// **AFTER A CRASH THE TOOL LEAVES EVERY FILE WHERE IT IS AND THE PLOT'S ROW NAMES THEM.**
    /// Bader's decision of 15 September.
    ///
    /// A run over 154 plots that throws on one of them leaves that plot's folder made, and
    /// perhaps its workbook, and perhaps a PDF beside it. Deleting them would destroy the one
    /// piece of evidence about what went wrong, and saying nothing about them leaves somebody
    /// opening a folder tree to find out whether a half written file is in it. So nothing is
    /// deleted, and every file of that plot is named with its path and with whether the disk
    /// really holds it, read at the moment the row is built.
    ///
    /// **THE FOLDER FLAG READS TRUE WHEN THE FOLDER EXISTS.** It used to be handed false in the
    /// catch whatever was on disk, so a press that made 154 folders and threw on one counted
    /// 153, and the count was the tool's own claim rather than the disk's answer.
    /// </summary>
    public static class PlotCrash
    {
        /// <summary>
        /// The refusal the plot's row carries: what threw, where it got to, and what it left.
        /// </summary>
        public static string Row(CreateStep step, string exceptionName, string message, IEnumerable<CrashFile> files)
        {
            if (step == null) throw new ArgumentNullException("step");

            List<CrashFile> held = (files ?? Enumerable.Empty<CrashFile>())
                .Where(one => one != null)
                .ToList();

            return "the write threw " + step.InWords + ", and nothing more was written for "
                + "this plot. " + Named(exceptionName) + ": " + Said(message) + ". "
                + Left(held);
        }

        /// <summary>
        /// What the crash left behind, named one by one. **Nothing is deleted**, so this is a
        /// list of what to go and look at rather than a note about a tidy up.
        /// </summary>
        public static string Left(IEnumerable<CrashFile> files)
        {
            List<CrashFile> held = (files ?? Enumerable.Empty<CrashFile>())
                .Where(one => one != null)
                .ToList();

            if (held.Count == 0)
            {
                return "Nothing is deleted after a crash, and nothing recorded a path for this "
                    + "plot, so there is no file to name";
            }

            return "Nothing is deleted after a crash. Of this plot's own files, "
                + string.Join(", ", held.Select(one => one.InWords).ToArray());
        }

        private static string Named(string exceptionName)
        {
            string held = (exceptionName ?? string.Empty).Trim();

            return held.Length == 0 ? "an exception naming no type" : held;
        }

        private static string Said(string message)
        {
            string held = (message ?? string.Empty).Trim();

            return held.Length == 0 ? "it carried no message" : held;
        }
    }
}
