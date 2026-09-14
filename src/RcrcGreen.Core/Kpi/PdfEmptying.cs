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
    /// **The only fields left as the template has them are the ones that are not text**, which
    /// is the four stage tick boxes and the Reset button, and they are left because of what they
    /// ARE rather than because anybody listed their names. A field whose kind cannot be read at
    /// all is left alone too, for the same reason: this tool does not clear what it cannot
    /// classify.
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

                clearing.Add(PdfFieldFill.Blank(PdfValue.NotOne, field.Name, NoSourceForIt));
            }

            return clearing;
        }
    }
}
