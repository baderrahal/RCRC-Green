using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// **THE CLIENT'S DEFAULT VALUES ARE NOTES FOR WHOEVER FILLS THE FORM BY HAND. THEY ARE NOT
    /// CONTENT.**
    ///
    /// Measured on all eight PDFs of the 18:15 run: Irrigation water demand went out holding
    /// `Revit / softscape &amp; shrubs &amp; lawn schedule / total water demand /1000`, and the
    /// Ground Cover box on the open spaces form, the one really named `0`, went out holding
    /// `REVIT SHEET/... GROUND COVER TOTAL AREA`. **150 PDFs went to a client with the
    /// instruction for filling a box printed inside that box.**
    ///
    /// **A FIELD THE TOOL DOES NOT FILL MUST LOOK LIKE A FIELD NOBODY HAS FILLED.** So every text
    /// field of every form this tool writes is either WRITTEN or EMPTIED, including every field
    /// the tool has no source for and never names.
    ///
    /// **Two kinds of field are left as the template has them.**
    ///
    /// The ones that are not text, which is the four stage tick boxes and the Reset button, left
    /// because of what they ARE rather than because anybody listed their names. A field whose
    /// kind cannot be read at all is left alone too, for the same reason: this tool does not
    /// clear what it cannot classify.
    ///
    /// **AND THE CLIENT'S OWN HEADER**, the project name and the consultant, whose defaults are
    /// VALUES rather than notes. The 19:52 run cleared both on all 150, because the rule said
    /// clear every default and could not tell an instruction from a value. They are named in
    /// <see cref="PdfForms.HeaderValuesLeftAlone"/> as data, found by the exact value the
    /// CLIENT'S OWN TEMPLATE carries, and nothing reads their text to classify them. A form that
    /// does not carry all three header values never reaches here, because
    /// <see cref="PdfFormCheck"/> refuses it.
    /// </summary>
    public static class PdfEmptying
    {
        /// <summary>
        /// Why a field the tool never names is cleared. It carries no source, so whatever it
        /// holds is the client's own note to a person filling the form by hand.
        /// </summary>
        public const string NoSourceForIt =
            "this tool has no source for this field, so whatever the template holds in it is the "
            + "client's own note to whoever fills the form by hand";

        /// <summary>
        /// Every field this plan will clear, the ones it names and leaves blank first, in the
        /// form's own order, then the ones it never names, in the file's order.
        ///
        /// **A field the plan WRITES is never in here**, and neither is anything that is not
        /// text.
        /// </summary>
        public static IReadOnlyList<PdfFieldFill> For(PdfPlan plan, IEnumerable<PdfFieldRead> inTheFile)
        {
            if (plan == null || !plan.Wanted) return new List<PdfFieldFill>();

            var written = new HashSet<string>(
                plan.Writing.Select(one => one.FieldName), StringComparer.Ordinal);

            // The fields the plan named and had nothing for. Each carries its own reason
            // already, which is why the ground cover says the schedule prints no such group
            // rather than the general sentence below.
            var clearing = plan.Blank.Where(one => !written.Contains(one.FieldName)).ToList();

            var named = new HashSet<string>(
                plan.Fields.Select(one => one.FieldName), StringComparer.Ordinal);

            foreach (PdfFieldRead field in (inTheFile ?? Enumerable.Empty<PdfFieldRead>()))
            {
                if (field == null || !field.IsText) continue;
                if (named.Contains(field.Name)) continue;
                if (IsTheClientsHeader(field)) continue;

                clearing.Add(PdfFieldFill.Blank(PdfValue.NotOne, field.Name, NoSourceForIt));
            }

            return clearing;
        }

        /// <summary>
        /// Whether this field holds the client's own project name or consultant, compared whole
        /// against the values measured off their three forms. **The contract reference is not in
        /// that list**: it is written from PRX_Plot_NH now, so it is a value the tool fills
        /// rather than one it steps around.
        /// </summary>
        public static bool IsTheClientsHeader(PdfFieldRead field)
        {
            if (field == null) return false;

            return PdfForms.HeaderValuesLeftAlone.Any(
                one => string.Equals((field.Value ?? string.Empty).Trim(), one, StringComparison.Ordinal));
        }

        /// <summary>
        /// The field holding the client's own contract reference in their template, which is the
        /// box this run writes the plot's PRX_Plot_NH into. **Found by that value because the
        /// field's NAME is measured nowhere in this repository**, and no client PDF may enter it
        /// to measure one from. Null where the file holds no such field, which the form check
        /// refuses before anything is written.
        /// </summary>
        public static PdfFieldRead TheContractReference(IEnumerable<PdfFieldRead> inTheFile)
        {
            return (inTheFile ?? Enumerable.Empty<PdfFieldRead>()).FirstOrDefault(
                one => one != null && one.IsText
                    && string.Equals(
                        (one.Value ?? string.Empty).Trim(), PdfForms.ContractReference, StringComparison.Ordinal));
        }
    }
}
