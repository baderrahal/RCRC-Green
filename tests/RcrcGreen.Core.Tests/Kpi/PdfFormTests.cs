using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// The three forms, which prefix reaches each, and the file work that fills one.
    ///
    /// **Every field name and every note in <see cref="PdfForms"/> was read off the client's own
    /// three files on 14 September and checked back against them, not copied from a list.** The
    /// cases here are about the rules around that table, because the table itself is a
    /// measurement and the files are not in this repository.
    /// </summary>
    public class PdfFormTests : IDisposable
    {
        private readonly string _folder = PdfFixture.Folder();

        public void Dispose()
        {
            try { Directory.Delete(_folder, true); }
            catch (IOException) { }
        }

        /// <summary>
        /// **KEYED ON THE PLOT PREFIX, which is the first thing the prefix decides on its own.**
        /// Everywhere else it cross checks a component that has the last word. Which form a plot
        /// gets is the team's filing and PRX_Component says nothing about it.
        /// </summary>
        [Theory]
        [InlineData("EP", "Projects Basic Data - Parks")]
        [InlineData("FP", "Projects Basic Data - Parks")]
        [InlineData("HF", "Projects Basic Data - Open spaces associated to buildings")]
        [InlineData("FM", "Projects Basic Data - Open spaces associated to buildings")]
        [InlineData("DM", "Projects Basic Data - Open spaces associated to buildings")]
        [InlineData("PL", "Projects Basic Data - Open spaces associated to buildings")]
        [InlineData("SC", "Projects Basic Data - Open spaces associated to buildings")]
        [InlineData("NS", "Projects Basic Data - Roads")]
        [InlineData("ST", "Projects Basic Data - Roads")]
        [InlineData("MM", "Projects Basic Data - Roads")]
        public void EveryPrefixReachesItsForm(string prefix, string form)
        {
            Assert.Equal(form, PdfForms.For(prefix).Name);
        }

        /// <summary>
        /// Nine prefixes over three forms, written out by hand rather than counted off the
        /// table with the table's own rule.
        /// </summary>
        [Fact]
        public void NinePrefixesReachThreeFormsAndEveryFormIsReached()
        {
            Assert.Equal(3, PdfForms.All.Count);
            Assert.Equal(10, PdfForms.All.Sum(one => one.Prefixes.Count));

            Assert.Equal(new[] { "EP", "FP" }, PdfForms.Parks.Prefixes.ToArray());
            Assert.Equal(new[] { "HF", "FM", "DM", "PL", "SC" }, PdfForms.OpenSpaces.Prefixes.ToArray());
            Assert.Equal(new[] { "NS", "ST", "MM" }, PdfForms.Roads.Prefixes.ToArray());

            foreach (PdfForm form in PdfForms.All) Assert.NotEmpty(form.Prefixes);
        }

        /// <summary>
        /// **The nine prefixes are the seven the plot prefix table already holds**, so the two
        /// records cannot say different things about which prefixes exist. MM, NS and ST are
        /// three of them and all three are streets.
        /// </summary>
        [Fact]
        public void EveryPrefixTheToolKnowsReachesAForm()
        {
            foreach (PlotPrefix held in PlotPrefixes.All)
            {
                Assert.True(PdfForms.For(held.Prefix) != null, held.Prefix + " reaches no form");
            }
        }

        /// <summary>
        /// **A prefix the table does not hold writes no PDF and is named**, the same rule a
        /// component the folder table does not hold follows.
        /// </summary>
        [Fact]
        public void APrefixTheTableDoesNotHoldWritesNothingAndIsNamed()
        {
            Assert.Null(PdfForms.For("ZZ"));
            Assert.Null(PdfForms.ForPlot("ZZ-01"));

            Assert.Equal(
                "no form is named for the plot prefix ZZ, so no PDF was written",
                PdfForms.NoFormFor("ZZ-01"));
        }

        /// <summary>
        /// Every form names a UID field and a report date field, because those two go on all
        /// three, and only the open spaces form names a project type.
        /// </summary>
        [Fact]
        public void WhichValuesEachFormTakes()
        {
            foreach (PdfForm form in PdfForms.All)
            {
                Assert.True(form.Takes(PdfValue.Uid), form.Name + " names no UID field");
                Assert.True(form.Takes(PdfValue.ReportDate), form.Name + " names no report date field");
                Assert.True(form.Takes(PdfValue.TotalShrubs), form.Name + " names no total shrubs field");
            }

            // Parks and Roads carry their project type as typed text and name no field for it.
            Assert.True(PdfForms.OpenSpaces.Takes(PdfValue.ProjectType));
            Assert.False(PdfForms.Parks.Takes(PdfValue.ProjectType));
            Assert.False(PdfForms.Roads.Takes(PdfValue.ProjectType));

            // The road width and the length are the road form's alone.
            Assert.True(PdfForms.Roads.Takes(PdfValue.Row));
            Assert.True(PdfForms.Roads.Takes(PdfValue.Length));
            Assert.False(PdfForms.Parks.Takes(PdfValue.Row));

            // An intervention area is on parks and open spaces and not on roads.
            Assert.True(PdfForms.Parks.Takes(PdfValue.Area));
            Assert.True(PdfForms.OpenSpaces.Takes(PdfValue.Area));
            Assert.False(PdfForms.Roads.Takes(PdfValue.Area));

            // The canopy percentage is the parks form's alone.
            Assert.True(PdfForms.Parks.Takes(PdfValue.PercentageCanopy));
            Assert.False(PdfForms.OpenSpaces.Takes(PdfValue.PercentageCanopy));
            Assert.False(PdfForms.Roads.Takes(PdfValue.PercentageCanopy));
        }

        /// <summary>
        /// **THE OPEN SPACES FORM'S FIELD NAMES ARE BROKEN AND THE TABLE CARRIES THEM AS THEY
        /// REALLY ARE.** Five of the nine prefixes use this form. Every one of these was read off
        /// the client's file.
        /// </summary>
        [Theory]
        [InlineData(PdfValue.ProjectType, "undefined_4.0")]
        [InlineData(PdfValue.Uid, "undefined_4.1")]
        [InlineData(PdfValue.ReportDate, "undefined_4.2")]
        [InlineData(PdfValue.GroundCover, "0")]
        [InlineData(PdfValue.Lawn, "0_2")]
        [InlineData(PdfValue.ProposedTrees, "Proposed Trees.0")]
        [InlineData(PdfValue.TotalTrees, "Proposed Trees.1")]
        [InlineData(PdfValue.ExistingShrubs, "Existing Shrubs.1")]
        [InlineData(PdfValue.ProposedShrubs, "Proposed Shrubs.1.0")]
        [InlineData(PdfValue.TotalShrubs, "Proposed Shrubs.1.1")]
        public void TheOpenSpacesFieldNamesAreTheBrokenOnesTheFileHolds(PdfValue value, string fieldName)
        {
            Assert.Equal(fieldName, PdfForms.OpenSpaces.FieldFor(value).FieldName);
        }

        /// <summary>
        /// Every field in every form names a value, a field and a note, and no two fields of one
        /// form take the same value or the same name.
        /// </summary>
        [Fact]
        public void NoFormNamesOneValueTwiceOrOneFieldTwice()
        {
            foreach (PdfForm form in PdfForms.All)
            {
                Assert.Equal(form.Fields.Count, form.Fields.Select(one => one.Value).Distinct().Count());
                Assert.Equal(form.Fields.Count, form.Fields.Select(one => one.FieldName).Distinct().Count());

                foreach (PdfFormField one in form.Fields)
                {
                    Assert.False(string.IsNullOrWhiteSpace(one.FieldName), form.Name + " names an empty field");
                    Assert.False(string.IsNullOrWhiteSpace(one.Note),
                        form.Name + " carries no note for " + one.FieldName);
                }
            }
        }

        /// <summary>
        /// **Fields are read by their FULL name, built through the parent chain.** On the real
        /// open spaces form the lawn area sits in a field whose own title is 0_2, so a title on
        /// its own names nothing.
        /// </summary>
        [Fact]
        public void FieldsAreReadByTheirFullNameThroughTheParentChain()
        {
            byte[] file = PdfFixture.Bytes(new[]
            {
                PdfFixture.Field("UID", "REVIT SHEETS /TITLE BLOCK/PRX_Plot_UID2"),
                PdfFixture.Field("Lawn", "LAWN (GRASS) TOTAL AREA"),
                PdfFixture.Field(PdfFixture.ParentedFieldName, "GROUND COVER TOTAL AREA")
            });

            string why;
            IReadOnlyList<PdfFieldRead> fields = PdfFormFile.Fields(file, out why);

            Assert.Equal(string.Empty, why);
            Assert.Equal(new[] { "Lawn", "Numbers", "Numbers.0", "UID" },
                fields.Select(one => one.Name).ToArray());

            Assert.Equal("GROUND COVER TOTAL AREA",
                fields.Single(one => one.Name == "Numbers.0").Value);
        }

        /// <summary>
        /// **THE SOURCE BYTES ARE THE FIRST BYTES OF THE ANSWER, UNCHANGED.** The client's form
        /// carries their branding, their layout and their Reset button, and a rebuilt page is
        /// not their document. A PDF supports an incremental update by its own design.
        /// </summary>
        [Fact]
        public void TheFilledFileKeepsEveryByteOfTheFormAndAppendsAfterIt()
        {
            byte[] file = PdfFixture.Bytes(new[]
            {
                PdfFixture.Field("UID", "a note"),
                PdfFixture.Field("Lawn", "another note")
            });

            string why;
            byte[] filled = PdfFormFile.Filled(file, new[]
            {
                new KeyValuePair<string, string>("UID", "ANH-008-MO-100006")
            }, out why);

            Assert.Equal(string.Empty, why);
            Assert.True(filled.Length > file.Length, "nothing was appended");
            Assert.Equal(file, filled.Take(file.Length).ToArray());

            IReadOnlyList<PdfFieldRead> back = PdfFormFile.Fields(filled, out why);
            Assert.Equal(2, back.Count);
            Assert.Equal("ANH-008-MO-100006", back.Single(one => one.Name == "UID").Value);

            // The field nobody wrote into still holds what the client put there.
            Assert.Equal("another note", back.Single(one => one.Name == "Lawn").Value);
        }

        /// <summary>
        /// **The stale appearance is dropped and NeedAppearances is set**, so the viewer draws
        /// the new value off the field's own default appearance. Keeping the old one beside a new
        /// value shows the client's note on screen over the number underneath it, and a stale
        /// word that looks like an answer is the worst thing this tool can put in a file.
        /// </summary>
        [Fact]
        public void TheStaleAppearanceIsDroppedAndNeedAppearancesIsSet()
        {
            byte[] file = PdfFixture.Bytes(new[]
            {
                PdfFixture.Field("UID", "a note"),
                PdfFixture.Field("Lawn", "another note")
            });

            Assert.Contains("/AP<</N 5 0 R>>", Text(file));

            string why;
            byte[] filled = PdfFormFile.Filled(file, new[]
            {
                new KeyValuePair<string, string>("Lawn", "60")
            }, out why);

            string appended = Text(filled).Substring(file.Length);

            Assert.DoesNotContain("/AP<</N 5 0 R>>", appended);
            Assert.Contains("/NeedAppearances true", appended);
        }

        /// <summary>
        /// A square metre sign survives, because a value that is not plain text goes in as UTF-16
        /// with a byte order mark, which is how a PDF carries one.
        /// </summary>
        [Fact]
        public void AValueThatIsNotPlainTextSurvivesTheRoundTrip()
        {
            byte[] file = PdfFixture.Bytes(new[] { PdfFixture.Field("Lawn", "a note") });

            string why;
            byte[] filled = PdfFormFile.Filled(file, new[]
            {
                new KeyValuePair<string, string>("Lawn", "60 m²")
            }, out why);

            IReadOnlyList<PdfFieldRead> back = PdfFormFile.Fields(filled, out why);
            Assert.Equal("60 m²", back.Single(one => one.Name == "Lawn").Value);
        }

        /// <summary>
        /// **A form the tool cannot read is refused by name rather than half written.** All
        /// three of the client's forms were measured to hold no compressed object stream, and one
        /// that arrives with them is a file this reader does not understand.
        /// </summary>
        [Fact]
        public void AFormHoldingObjectStreamsIsRefusedByName()
        {
            byte[] file = PdfFixture.Bytes(new[] { PdfFixture.Field("UID", "a note") });
            byte[] withStreams = Bytes(Text(file).Replace("/Type/Catalog", "/Type/Catalog/X/ObjStm"));

            string why;
            IReadOnlyList<PdfFieldRead> fields = PdfFormFile.Fields(withStreams, out why);

            Assert.Empty(fields);
            Assert.Equal(PdfFormFile.HoldsObjectStreams, why);
            Assert.Null(PdfFormFile.Filled(withStreams, new List<KeyValuePair<string, string>>(), out why));
        }

        /// <summary>
        /// A file with no AcroForm at all is refused rather than read as a form with no fields.
        /// </summary>
        [Fact]
        public void AFileWithNoAcroFormIsRefused()
        {
            byte[] file = PdfFixture.Bytes(new[] { PdfFixture.Field("UID", "a note") });
            byte[] without = Bytes(Text(file).Replace("/AcroForm 3 0 R", "/X 3 0 R"));

            string why;
            Assert.Empty(PdfFormFile.Fields(without, out why));
            Assert.Equal(PdfFormFile.NoAcroForm, why);
        }

        /// <summary>
        /// Filling a field the file does not hold refuses rather than writing into nothing.
        /// </summary>
        [Fact]
        public void AFieldTheFileDoesNotHoldRefusesTheFill()
        {
            byte[] file = PdfFixture.Bytes(new[] { PdfFixture.Field("UID", "a note") });

            string why;
            byte[] filled = PdfFormFile.Filled(file, new[]
            {
                new KeyValuePair<string, string>("Lawn", "60")
            }, out why);

            Assert.Null(filled);
            Assert.Equal("the field Lawn is not in this file", why);
        }

        private static string Text(byte[] file)
        {
            var built = new System.Text.StringBuilder(file.Length);
            foreach (byte one in file) built.Append((char)one);
            return built.ToString();
        }

        private static byte[] Bytes(string text)
        {
            var found = new byte[text.Length];
            for (int at = 0; at < text.Length; at++) found[at] = (byte)text[at];
            return found;
        }
    }
}
