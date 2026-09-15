using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **THE CLIENT'S DEFAULT VALUES ARE NOTES FOR WHOEVER FILLS THE FORM BY HAND.**
    ///
    /// Measured on all eight PDFs of the 18:15 run over NG05: Irrigation water demand went out
    /// holding `Revit / softscape and shrubs and lawn schedule / total water demand /1000`, and
    /// the Ground Cover box on the open spaces form, the one really named `0`, went out holding
    /// its own REVIT SHEET note. **150 PDFs reached a client with the instruction for filling a
    /// box printed inside that box.**
    /// </summary>
    public class PdfEmptyingTests : IDisposable
    {
        private static readonly DateTime Today = new DateTime(2026, 9, 14);

        private readonly string _folder = PdfFixture.Folder();

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

        /// <summary>
        /// The Roads form as the tool knows it, plus two fields it names nowhere: one text field
        /// holding the client's own note, and one stage tick box.
        /// </summary>
        private string RoadsForm(string fileName = "roads.pdf")
        {
            var fields = PdfForms.Roads.Fields
                .Select(one => PdfFixture.Field(one.FieldName, one.Note, one.X, one.Y))
                .ToList();

            fields.Add(PdfFixture.Field("Sidewalk", "REVIT / the sidewalk length, by hand"));
            fields.Add(PdfFixture.TickBox("Schematic Design", "Yes"));
            fields.AddRange(PdfFixture.ClientHeader());

            return PdfFixture.Form(_folder, fileName, fields);
        }

        private static PdfPlan RoadPlan()
        {
            return PdfFill.Of(
                CreateFixture.Plot("ST-05", component: "STREET 30m ROW", uid2: "ANH-007-ST-100210"),
                CountedGroups.Of(KpiTemplates.Streets),
                StreetReferenceAnswer.Of(20.0, 330.66, "20", "330.66"),
                Today, true, PdfWorkbookNumbers.None);
        }

        /// <summary>
        /// **EVERY TEXT FIELD IS WRITTEN OR EMPTIED**, and the two the 18:15 run sent out holding
        /// their own instructions are emptied with their own reasons.
        /// </summary>
        [Fact]
        public void TheFieldsTheToolDoesNotFillComeOutEmpty()
        {
            PdfOutcome outcome = PdfChecklist.Write(
                RoadsForm(), Path.Combine(_folder, "out.pdf"), RoadPlan());

            Assert.True(outcome.Written, outcome.Refusal);

            // Every emptied field landed empty, read back off the output rather than assumed.
            Assert.Empty(outcome.EmptiedButNotCleared);

            PdfLandedField water = outcome.Emptied.Single(
                one => one.FieldName == "Irrigation water demand");

            Assert.Equal(string.Empty, water.Landed);

            // **THE BLANK NAMES BOTH HALVES.** This plan's reading holds neither schedule, so
            // both are named: a reason short of the second reads like a plot with one thing
            // wrong with it.
            Assert.Contains("no irrigation water demand could be read", water.Why);
            Assert.Contains("softscape, no schedule of that kind was read on this plot", water.Why);
            Assert.Contains("shrubs and lawn, no schedule of that kind was read on this plot", water.Why);

            PdfLandedField ground = outcome.Emptied.Single(one => one.FieldName == "Ground Cover");

            Assert.Equal(string.Empty, ground.Landed);
            Assert.Equal(PdfFill.GroundCoverIsNotPrintedApart, ground.Why);
        }

        /// <summary>
        /// **INCLUDING EVERY FIELD THE TOOL HAS NO SOURCE FOR AND NAMES NOWHERE.** Sidewalk is
        /// not in the table at all, and what the template holds in it is the client's own note.
        /// </summary>
        [Fact]
        public void AFieldTheToolNamesNowhereIsEmptiedToo()
        {
            PdfOutcome outcome = PdfChecklist.Write(
                RoadsForm("nowhere.pdf"), Path.Combine(_folder, "out2.pdf"), RoadPlan());

            PdfLandedField sidewalk = outcome.Emptied.Single(one => one.FieldName == "Sidewalk");

            Assert.Equal(string.Empty, sidewalk.Landed);
            Assert.Equal(PdfEmptying.NoSourceForIt, sidewalk.Why);
            Assert.Equal(PdfValue.NotOne, sidewalk.Value);
        }

        /// <summary>
        /// **THE ONLY FIELDS LEFT AS THE TEMPLATE HAS THEM ARE THE ONES THAT ARE NOT TEXT**,
        /// which is the four stage tick boxes and the Reset button, and they are left because of
        /// what they ARE rather than because anybody listed their names.
        /// </summary>
        [Fact]
        public void ATickBoxIsLeftExactlyAsTheTemplateHasIt()
        {
            string output = Path.Combine(_folder, "out3.pdf");
            PdfOutcome outcome = PdfChecklist.Write(RoadsForm("tick.pdf"), output, RoadPlan());

            Assert.DoesNotContain(outcome.Emptied, one => one.FieldName == "Schematic Design");

            string refusal;
            IReadOnlyList<PdfFieldRead> back = PdfFormFile.Fields(File.ReadAllBytes(output), out refusal);

            PdfFieldRead tick = back.Single(one => one.Name == "Schematic Design");

            Assert.Equal("Yes", tick.Value);
            Assert.Equal("Btn", tick.FieldType);
            Assert.False(tick.IsText);
        }

        /// <summary>
        /// Every field of the form, one by one, in one of THREE states: **written, emptied, or
        /// left as the template has it.** The third is the four tick boxes and the Reset button,
        /// which are not text, and the client's own project name and consultant, whose defaults
        /// are values rather than notes.
        /// </summary>
        [Fact]
        public void EveryFieldOfTheFormIsInOneOfTheThreeStatesAndNoOther()
        {
            string output = Path.Combine(_folder, "out4.pdf");
            PdfOutcome outcome = PdfChecklist.Write(
                RoadsForm("all.pdf"), output, RoadPlan(), "GP.NH.Z2.052-DES042");

            string refusal;
            IReadOnlyList<PdfFieldRead> back = PdfFormFile.Fields(File.ReadAllBytes(output), out refusal);

            var written = new HashSet<string>(outcome.Landed.Select(one => one.FieldName), StringComparer.Ordinal);
            var emptied = new HashSet<string>(outcome.Emptied.Select(one => one.FieldName), StringComparer.Ordinal);
            var leftAlone = new HashSet<string>(
                new[] { "Project name", "Consultant", "Schematic Design" }, StringComparer.Ordinal);

            foreach (PdfFieldRead field in back)
            {
                if (leftAlone.Contains(field.Name))
                {
                    Assert.False(emptied.Contains(field.Name), field.Name + " must be left alone");
                    Assert.False(written.Contains(field.Name), field.Name + " must be left alone");
                    continue;
                }

                if (!field.IsText)
                {
                    Assert.False(emptied.Contains(field.Name), field.Name + " is not text and must be left alone");
                    continue;
                }

                Assert.True(
                    written.Contains(field.Name) || emptied.Contains(field.Name),
                    field.Name + " is text and was neither written nor emptied");
            }

            // Counted by hand off the Roads table. Fourteen fields the tool names, and this plot
            // writes seven of them: the UID, the report date, the road width, the length and the
            // three tree counts. The contract reference makes eight written. The other seven of
            // the fourteen are emptied, Total areas to be greened and the irrigation demand and
            // the three shrub areas and the ground cover and the lawn, and Sidewalk makes eight
            // emptied, which the tool names nowhere. Eight plus eight plus the two left alone is
            // the eighteen text fields this form holds.
            Assert.Equal(8, outcome.Landed.Count);
            Assert.Equal(8, outcome.Emptied.Count);
            Assert.Equal(18, back.Count(one => one.IsText));
        }

        /// <summary>
        /// **THE CLIENT'S OWN PROJECT NAME AND CONSULTANT COME BACK UNTOUCHED**, and the contract
        /// reference reads this plot's PRX_Plot_NH. The 19:52 run cleared all three on 150 PDFs,
        /// because the rule said clear every default and could not tell an instruction from a
        /// value.
        /// </summary>
        [Fact]
        public void TheClientsHeaderSurvivesAndTheContractReferenceIsThePlotsOwn()
        {
            string output = Path.Combine(_folder, "header.pdf");
            PdfChecklist.Write(RoadsForm("head.pdf"), output, RoadPlan(), "GP.NH.Z2.052-DES042");

            string refusal;
            IReadOnlyList<PdfFieldRead> back = PdfFormFile.Fields(File.ReadAllBytes(output), out refusal);

            Assert.Equal(
                "Neighborhood Landscape Design - Zone #2",
                back.Single(one => one.Name == "Project name").Value);

            Assert.Equal("SAPL", back.Single(one => one.Name == "Consultant").Value);
            Assert.Equal(
                "GP.NH.Z2.052-DES042",
                back.Single(one => one.Name == "Contract reference").Value);
        }

        /// <summary>
        /// **A plot carrying no PRX_Plot_NH writes nothing there and is named**, the same as any
        /// other unwritten field, rather than leaving the template's own reference standing or
        /// carrying another plot's.
        /// </summary>
        [Fact]
        public void APlotWithNoPlotNhLeavesTheContractReferenceUnwrittenAndNamesIt()
        {
            string output = Path.Combine(_folder, "nonh.pdf");
            PdfOutcome outcome = PdfChecklist.Write(
                RoadsForm("nonh-form.pdf"), output, RoadPlan(), string.Empty);

            PdfLandedField left = outcome.Emptied.Single(one => one.Value == PdfValue.ContractReference);

            Assert.Equal(PdfChecklist.NoPlotNh, left.Why);
            Assert.Contains("PRX_Plot_NH", left.Why);

            string refusal;
            IReadOnlyList<PdfFieldRead> back = PdfFormFile.Fields(File.ReadAllBytes(output), out refusal);

            // And the box is EMPTIED rather than left holding the template's own reference,
            // because the template's is not this plot's.
            Assert.Equal(string.Empty, back.Single(one => one.Name == "Contract reference").Value);
        }
    }
}
