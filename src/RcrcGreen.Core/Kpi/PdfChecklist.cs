using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One plot's PDF, beside its workbook, in its own folder, named after the same UID2.
    ///
    /// **THE EXCEL IS WRITTEN FIRST AND THE PDF SECOND**, because two of the PDF's fields read
    /// cells out of the workbook this run has just written. That ordering is a rule and the
    /// caller keeps it.
    ///
    /// **The form is checked before a byte is written, on every run.** A form whose field names
    /// or notes have moved is one this tool does not know, and it writes nothing into one rather
    /// than putting a lawn area into a box that has become something else.
    /// </summary>
    public static class PdfChecklist
    {
        public const string Extension = ".pdf";

        public const string NoFormFolder =
            "no forms folder is set, so no PDF was written. Browse to the folder holding the "
            + "client's Projects Basic Data forms and press Create again";

        public const string NoFormFile =
            "the forms folder holds no file this tool recognises as ";

        /// <summary>
        /// The PDF path beside the workbook: same folder, same name, a different extension. It
        /// is built off the workbook's own path so the two can never be filed apart.
        /// </summary>
        public static string Beside(PlotWorkbookPath where)
        {
            if (where == null || !where.Ok) return string.Empty;

            return Path.Combine(
                where.FolderPath, Path.GetFileNameWithoutExtension(where.FilePath) + Extension);
        }

        /// <summary>
        /// Fill one plot's form. The form file is read, checked, filled and read back, and
        /// everything the report prints comes off that read back.
        /// </summary>
        public static PdfOutcome Write(string formPath, string outputPath, PdfPlan plan)
        {
            if (plan == null) throw new ArgumentNullException("plan");

            if (!plan.Wanted)
            {
                return PdfOutcome.WroteNothing(plan.PlotId, plan.Form, plan.Why, null);
            }

            byte[] file;
            try
            {
                file = PdfFormFile.Read(formPath);
            }
            catch (IOException failed)
            {
                return PdfOutcome.WroteNothing(plan.PlotId, plan.Form,
                    "the form " + formPath + " could not be read. " + failed.Message, null);
            }
            catch (UnauthorizedAccessException denied)
            {
                return PdfOutcome.WroteNothing(plan.PlotId, plan.Form,
                    "the form " + formPath + " was refused. " + denied.Message, null);
            }

            string refusal;
            IReadOnlyList<PdfFieldRead> fields = PdfFormFile.Fields(file, out refusal);
            PdfFormCheck check = PdfFormCheck.Of(plan.Form, fields, refusal);

            if (!check.Matched)
            {
                return PdfOutcome.WroteNothing(plan.PlotId, plan.Form, check.Why, check);
            }

            var values = plan.Writing
                .Select(one => new KeyValuePair<string, string>(one.FieldName, one.Text))
                .ToList();

            byte[] filled = PdfFormFile.Filled(file, values, out refusal);
            if (filled == null)
            {
                return PdfOutcome.WroteNothing(plan.PlotId, plan.Form,
                    "nothing was written into the form. " + refusal, check);
            }

            try
            {
                File.WriteAllBytes(outputPath, filled);
            }
            catch (IOException failed)
            {
                return PdfOutcome.WroteNothing(plan.PlotId, plan.Form,
                    "the PDF could not be written. Close it if it is open, then press Create again. "
                    + failed.Message, check);
            }
            catch (UnauthorizedAccessException denied)
            {
                return PdfOutcome.WroteNothing(plan.PlotId, plan.Form,
                    "the PDF was refused. " + denied.Message, check);
            }

            // **READ THE FIELDS BACK OFF THE OUTPUT**, the same rule every written cell of the
            // workbook already follows, so the report says what landed rather than what was sent.
            IReadOnlyList<PdfFieldRead> back = PdfFormFile.Fields(File.ReadAllBytes(outputPath), out refusal);

            var landed = new List<PdfLandedField>();
            foreach (PdfFieldFill one in plan.Writing)
            {
                PdfFieldRead found = back.FirstOrDefault(
                    held => string.Equals(held.Name, one.FieldName, StringComparison.Ordinal));

                landed.Add(new PdfLandedField(
                    one.Value, one.FieldName, one.Text, found == null ? string.Empty : found.Value,
                    one.Unit, one.Working));
            }

            return PdfOutcome.Wrote(
                plan.PlotId, plan.Form, outputPath, check, landed, plan.Blank, plan.WhatWasChecked);
        }

        /// <summary>
        /// Which file in the browsed folder is which form, decided by the fields it holds and
        /// never by its name. **A folder holding a renamed form still works and a folder holding
        /// a file named like a form but shaped like something else does not.**
        /// </summary>
        public static string FileFor(IEnumerable<string> paths, PdfForm form, out string why)
        {
            why = string.Empty;
            if (form == null) throw new ArgumentNullException("form");

            foreach (string path in (paths ?? Enumerable.Empty<string>()).OrderBy(one => one, StringComparer.Ordinal))
            {
                byte[] file;
                try
                {
                    file = PdfFormFile.Read(path);
                }
                catch (IOException)
                {
                    continue;
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }

                string refusal;
                IReadOnlyList<PdfFieldRead> fields = PdfFormFile.Fields(file, out refusal);
                if (PdfFormCheck.Of(form, fields, refusal).Matched) return path;
            }

            why = NoFormFile + form.Name;
            return string.Empty;
        }
    }
}
