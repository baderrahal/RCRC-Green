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
        public const string NoPlotNh =
            "this plot's sheet carries no " + KpiNames.PlotNh
            + ", so the contract reference box is left unwritten rather than carrying another "
            + "plot's reference or the template's own";

        public const string NoContractReferenceField =
            "this form holds no field carrying the client's own contract reference, so there is "
            + "nowhere to write it";

        /// <summary>
        /// The plot's PRX_Plot_NH, into the box the client's template carries their contract
        /// reference in. **A plot with none writes nothing there and is named**, the same as any
        /// other unwritten field.
        /// </summary>
        private static PdfFieldFill ContractReference(PdfFieldRead field, string plotNh)
        {
            if (field == null)
            {
                return PdfFieldFill.Blank(
                    PdfValue.ContractReference, "the contract reference", NoContractReferenceField);
            }

            return string.IsNullOrWhiteSpace(plotNh)
                ? PdfFieldFill.Blank(PdfValue.ContractReference, field.Name, NoPlotNh)
                : PdfFieldFill.Writing(PdfValue.ContractReference, field.Name, plotNh.Trim(), "text");
        }

        public static PdfOutcome Write(string formPath, string outputPath, PdfPlan plan)
        {
            return Write(formPath, outputPath, plan, string.Empty);
        }

        /// <summary>
        /// One plot's PDF. <paramref name="plotNh"/> is that plot's PRX_Plot_NH, which goes into
        /// the contract reference box.
        /// </summary>
        public static PdfOutcome Write(string formPath, string outputPath, PdfPlan plan, string plotNh)
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

            // **THE CONTRACT REFERENCE IS WRITTEN FROM PRX_Plot_NH, PER PLOT.** Bader's decision
            // of 14 September. It is found by the value the client's template carries, because
            // its field name is measured nowhere here, and the form check has already refused a
            // file that does not carry that value.
            PdfFieldRead contract = PdfEmptying.TheContractReference(fields);
            PdfFieldFill reference = ContractReference(contract, plotNh);
            if (reference != null && reference.Written)
            {
                values.Add(new KeyValuePair<string, string>(reference.FieldName, reference.Text));
            }

            // **EVERY TEXT FIELD IS WRITTEN OR EMPTIED.** What the client's template holds in a
            // field nobody filled is their own note to a person filling it by hand, and a note
            // printed in a box reads as an answer. The tick boxes and the Reset button are not
            // text, so they are left exactly as they are, and so are the client's own project
            // name and consultant, whose defaults are values rather than notes.
            var emptied = PdfEmptying.For(plan, fields)
                .Where(one => contract == null || !string.Equals(one.FieldName, contract.Name, StringComparison.Ordinal))
                .ToList();

            // **A PLOT WITH NO PRX_Plot_NH GETS THE BOX EMPTIED, NOT LEFT.** The template's own
            // reference is not this plot's, so leaving it standing would hand the client another
            // project's number under this plot's name. It is emptied with its own reason rather
            // than the general one.
            if (contract != null && reference != null && !reference.Written) emptied.Add(reference);

            values.AddRange(emptied.Select(
                one => new KeyValuePair<string, string>(one.FieldName, string.Empty)));

            IReadOnlyList<PdfFieldFit> fits;
            byte[] filled = PdfFormFile.Filled(file, values, out fits, out refusal);
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

            if (reference != null && reference.Written)
            {
                PdfFieldRead landedReference = back.FirstOrDefault(
                    held => string.Equals(held.Name, reference.FieldName, StringComparison.Ordinal));

                landed.Add(new PdfLandedField(
                    reference.Value, reference.FieldName, reference.Text,
                    landedReference == null ? string.Empty : landedReference.Value, "text", string.Empty));
            }

            // **What landed in an emptied field is read back too**, off the same second read, so
            // a box the tool meant to clear and did not is visible rather than assumed.
            var cleared = new List<PdfLandedField>();
            foreach (PdfFieldFill one in emptied)
            {
                PdfFieldRead found = back.FirstOrDefault(
                    held => string.Equals(held.Name, one.FieldName, StringComparison.Ordinal));

                cleared.Add(new PdfLandedField(
                    one.Value, one.FieldName, string.Empty,
                    found == null ? string.Empty : found.Value, string.Empty, string.Empty, one.Why));
            }

            // **AND THE SIZE IS READ BACK OFF THE OUTPUT TOO**, so a /DA this run meant to shrink
            // and did not is visible rather than assumed, the same rule the values already
            // follow. Nothing here compares what was sent against itself.
            foreach (PdfFieldFit one in fits)
            {
                PdfFieldRead found = back.FirstOrDefault(
                    held => string.Equals(held.Name, one.FieldName, StringComparison.Ordinal));

                one.With(found == null || !found.DefaultAppearance.Read ? -1.0 : found.DefaultAppearance.Size);
            }

            // A form holding no contract reference field at all is a different fact from a plot
            // with no PRX_Plot_NH, and it is named among the blanks rather than emptied.
            var blank = plan.Blank.ToList();
            if (contract == null && reference != null && !reference.Written) blank.Add(reference);

            return PdfOutcome.Wrote(
                plan.PlotId, plan.Form, outputPath, check, landed, blank, plan.WhatWasChecked,
                cleared, fits);
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
