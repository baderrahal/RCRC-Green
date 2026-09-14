using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One field as the OUTPUT holds it, read back off the file that was written.
    /// </summary>
    public sealed class PdfLandedField
    {
        public PdfLandedField(
            PdfValue value, string fieldName, string sent, string landed,
            string unit = null, string working = null)
        {
            Value = value;
            Unit = unit ?? string.Empty;
            Working = working ?? string.Empty;
            FieldName = fieldName ?? string.Empty;
            Sent = sent ?? string.Empty;
            Landed = landed ?? string.Empty;
        }

        public PdfValue Value { get; }

        public string FieldName { get; }

        /// <summary>
        /// The unit the value is written IN, as the form's own unit column prints it. **A number
        /// in the wrong unit is plausible and a number with its unit beside it is checkable**,
        /// which is why every written field carries one and the report prints it.
        /// </summary>
        public string Unit { get; }

        /// <summary>
        /// How a computed value was worked out, empty for one that was read off the model or the
        /// reference file. **Two of this form's fields are numbers no schedule printed**, so they
        /// carry their parts and a person can check them against the workbook.
        /// </summary>
        public string Working { get; }

        public bool Computed
        {
            get { return Working.Length > 0; }
        }

        public string Sent { get; }

        /// <summary>What the output really holds, which is what the report prints.</summary>
        public string Landed { get; }

        public bool Agrees
        {
            get { return string.Equals(Sent, Landed, StringComparison.Ordinal); }
        }
    }

    /// <summary>
    /// What one plot's PDF run did. **Every count the report prints comes off this**, never off
    /// the plan, which is the shape this repo settled on after a report named four views as
    /// created and as not created in one file.
    /// </summary>
    public sealed class PdfOutcome
    {
        private PdfOutcome(
            string plotId, PdfForm form, bool written, string path, string refusal,
            PdfFormCheck check, IEnumerable<PdfLandedField> landed, IEnumerable<PdfFieldFill> blank)
        {
            PlotId = plotId ?? string.Empty;
            Form = form;
            Written = written;
            Path = path ?? string.Empty;
            Refusal = refusal ?? string.Empty;
            Check = check;
            Landed = (landed ?? Enumerable.Empty<PdfLandedField>()).ToList();
            Blank = (blank ?? Enumerable.Empty<PdfFieldFill>()).ToList();
        }

        public static PdfOutcome Wrote(
            string plotId, PdfForm form, string path, PdfFormCheck check,
            IEnumerable<PdfLandedField> landed, IEnumerable<PdfFieldFill> blank)
        {
            return new PdfOutcome(plotId, form, true, path, string.Empty, check, landed, blank);
        }

        public static PdfOutcome WroteNothing(string plotId, PdfForm form, string why, PdfFormCheck check)
        {
            return new PdfOutcome(plotId, form, false, string.Empty, why, check, null, null);
        }

        public string PlotId { get; }

        /// <summary>Null where no form is named for the plot's prefix.</summary>
        public PdfForm Form { get; }

        public bool Written { get; }

        public string Path { get; }

        /// <summary>Empty on a PDF that was written. Never empty on one that was not.</summary>
        public string Refusal { get; }

        /// <summary>Null where the file was never opened, so no check could run.</summary>
        public PdfFormCheck Check { get; }

        public IReadOnlyList<PdfLandedField> Landed { get; }

        /// <summary>Fields this run left blank, each with its reason.</summary>
        public IReadOnlyList<PdfFieldFill> Blank { get; }

        public string FormName
        {
            get { return Form == null ? string.Empty : Form.Name; }
        }

        /// <summary>
        /// A form that was read and did not match. It is counted apart from a refusal, because
        /// the client reissuing a form is a different thing from a file that would not open.
        /// </summary>
        public bool FormDidNotMatch
        {
            get { return Check != null && !Check.Matched; }
        }

        public IEnumerable<PdfLandedField> Disagreeing
        {
            get { return Landed.Where(one => !one.Agrees); }
        }
    }
}
