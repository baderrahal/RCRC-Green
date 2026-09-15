using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **HOW BIG A VALUE IS WRITTEN, so it stays inside the box the client drew.**
    ///
    /// Measured on ANH-007-MO-100011, a DAILY MOSQUE plot on the Open spaces form: the Area box
    /// showed 2797.6 cut off at its edge, Total areas to be greened showed 0.0008 cut off, and
    /// the tree and shrub counts were drawn taller than the boxes holding them.
    ///
    /// **EVERY EXPECTED NUMBER HERE IS WORKED OUT BY HAND**, never with the rule the code uses.
    /// </summary>
    public class PdfTextFitTests : IDisposable
    {
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

        private static PdfFontWidths Helvetica
        {
            get { return StandardFonts.For("Helvetica"); }
        }

        private static PdfFieldFit Fit(string text, double width, double height, string appearance)
        {
            return PdfTextFit.Of(
                "Area", text, new PdfTextBox(0.0, 0.0, width, height),
                PdfDefaultAppearance.Of(appearance), Helvetica);
        }

        /// <summary>
        /// **THE WORKED EXAMPLE, WRITTEN OUT BY HAND.** `2797.64` is six digits at 556 and one
        /// full stop at 278, which is 3,614 thousandths, so 3.614 em. A box 30 pt wide and 14 pt
        /// tall leaves 26 pt across and 10 pt down once 2 pt comes off each side. 26 over 3.614
        /// is 7.1948, and rounded DOWN to a tenth that is 7.1.
        /// </summary>
        [Fact]
        public void TwentySevenNinetySevenPointSixFourComesOutAtSevenPointOne()
        {
            PdfFieldFit fit = Fit("2797.64", 30.0, 14.0, "/Helv 0 Tf 0 g");

            // **THE FAILURE NAMES THE FIELD, THE TEXT AND THE BOX.** A bare Expected 7.1 against
            // Actual 0 says a number moved and not which box in a client document is cut off.
            Assert.True(
                Math.Abs(fit.Size - 7.1) < 0.0001,
                fit.FieldName + ", holding '" + fit.Text + "' in a box " + fit.Box.InWords
                + ", came out at " + fit.Size + " pt and " + PdfTextFit.Words(fit.Outcome)
                + ". 30 pt across less 2 pt each side is 26, over 3.614 em is 7.1948, and rounded "
                + "down to a tenth that is 7.1.");

            Assert.Equal(PdfFitOutcome.Shrunk, fit.Outcome);
            Assert.Equal("2797.64", fit.Text);
            Assert.Equal("Helvetica", fit.FontName);
        }

        /// <summary>
        /// **HELVETICA'S DIGITS ARE 556 AND ITS FULL STOP IS 278**, which are the two widths the
        /// worked example rests on. Six digits and a full stop is 6 times 556 plus 278, which is
        /// 3,614 thousandths of the size.
        /// </summary>
        [Fact]
        public void HelveticaMeasuresTheWorkedExampleAtThreePointSixOneFourEms()
        {
            Assert.Equal(3.614, Helvetica.Ems("2797.64"), 6);
            Assert.Equal(0.556, Helvetica.Ems("2"), 6);
            Assert.Equal(0.278, Helvetica.Ems("."), 6);
            Assert.Equal(0.0, Helvetica.Ems(string.Empty));
        }

        /// <summary>
        /// **NEVER ABOVE THE CLIENT'S OWN SIZE.** A box with room to spare does not make the
        /// number bigger than the size their field carries.
        /// </summary>
        [Fact]
        public void TheSizeIsNeverRaisedAboveTheClientsOwn()
        {
            PdfFieldFit fit = Fit("12", 200.0, 40.0, "/Helv 9 Tf 0 g");

            Assert.Equal(PdfFitOutcome.KeptTheClientsSize, fit.Outcome);
            Assert.Equal(9.0, fit.Size);
            Assert.Equal(9.0, fit.ClientSize);
        }

        /// <summary>
        /// **A /DA OF NOUGHT IS THE CLIENT ASKING THE VIEWER TO PICK, and the viewer picks one
        /// that fills the box.** That is how a tree count came out drawn taller than its box, so
        /// the tool's own ceiling of 10 pt stands in its place.
        /// </summary>
        [Fact]
        public void AnAutoSizeIsCappedAtTen()
        {
            PdfFieldFit fit = Fit("12", 200.0, 40.0, "/Helv 0 Tf 0 g");

            Assert.Equal(PdfFitOutcome.CappedAtTen, fit.Outcome);
            Assert.Equal(10.0, fit.Size);
            Assert.Equal(0.0, fit.ClientSize);
        }

        /// <summary>
        /// **THE HEIGHT BOUNDS IT TOO.** A box 200 pt wide and 11 pt tall leaves 7 pt down once
        /// 2 pt comes off the top and the bottom, and `12` is 1.112 em, which would fit 176 pt
        /// across. The smaller of the two wins.
        /// </summary>
        [Fact]
        public void AShortBoxBoundsTheSizeByItsHeight()
        {
            PdfFieldFit fit = Fit("12", 200.0, 11.0, "/Helv 0 Tf 0 g");

            Assert.Equal(PdfFitOutcome.Shrunk, fit.Outcome);
            Assert.Equal(7.0, fit.Size);
        }

        /// <summary>
        /// **NEVER BELOW 6 PT, AND THE BOX IS NAMED.** `2797.64` is 3.614 em, so a box 20 pt wide
        /// leaves 16 pt across and needs 4.4 pt. It is written at 6 and it runs over, and the
        /// line says so with the field, the text and the box.
        /// </summary>
        [Fact]
        public void AValueThatDoesNotFitAtSixIsWrittenAtSixAndNamed()
        {
            PdfFieldFit fit = Fit("2797.64", 20.0, 14.0, "/Helv 0 Tf 0 g");

            Assert.Equal(PdfFitOutcome.HeldAtSix, fit.Outcome);
            Assert.Equal(6.0, fit.Size);
            Assert.Contains("2797.64", fit.Why);
            Assert.Contains("20 pt across and 14 pt tall", fit.Why);
            Assert.Contains("runs over", fit.Why);
        }

        /// <summary>
        /// **ROUNDED DOWN AND NEVER TO THE NEAREST.** A tenth of a point rounded up is a tenth of
        /// a point of text outside the box.
        /// </summary>
        [Fact]
        public void TheSizeIsRoundedDownToATenth()
        {
            Assert.Equal(7.1, PdfTextFit.Floored(7.1948));
            Assert.Equal(7.9, PdfTextFit.Floored(7.99));
            Assert.Equal(6.0, PdfTextFit.Floored(6.0));
            Assert.Equal(0.0, PdfTextFit.Floored(double.NaN));
            Assert.Equal(0.0, PdfTextFit.Floored(double.PositiveInfinity));
        }

        /// <summary>
        /// **A FONT THIS TOOL CANNOT MEASURE WRITES THE CLIENT'S OWN SIZE AND IS NAMED.** Never a
        /// guess at a width, and never a size chosen off a font nobody measured.
        /// </summary>
        [Fact]
        public void AFontWithNoWidthsWritesTheClientsSizeAndNamesTheFont()
        {
            PdfFieldFit fit = PdfTextFit.Of(
                "Area", "2797.64", new PdfTextBox(0.0, 0.0, 20.0, 14.0),
                PdfDefaultAppearance.Of("/Cour 8 Tf 0 g"),
                PdfFontWidths.NotRead("Courier New", StandardFonts.NotOneOfTheFourteen));

            Assert.Equal(PdfFitOutcome.WidthsUnknown, fit.Outcome);
            Assert.Equal(8.0, fit.Size);
            Assert.False(fit.Writes);
            Assert.Contains("Courier New", fit.Why);
            Assert.Contains(StandardFonts.NotOneOfTheFourteen, fit.Why);
        }

        /// <summary>
        /// **ONE CHARACTER WITH NO WIDTH MAKES THE WHOLE STRING UNMEASURABLE.** A sum short of a
        /// term is a narrower string than the real one, which would shrink the text too little
        /// and leave it running over with the report saying it fits.
        /// </summary>
        [Fact]
        public void OneCharacterWithNoWidthMakesTheWholeStringUnmeasurable()
        {
            Assert.True(Helvetica.Ems("2797.64 m²") < 0.0);
            Assert.Equal("²", Helvetica.FirstUnmeasurable("2797.64 m²"));

            PdfFieldFit fit = Fit("2797.64 m²", 30.0, 14.0, "/Helv 8 Tf 0 g");

            Assert.Equal(PdfFitOutcome.WidthsUnknown, fit.Outcome);
            Assert.Equal(8.0, fit.Size);
        }

        /// <summary>
        /// **NO /DA ANYWHERE MEANS THERE IS NO SIZE TO CHANGE**, so the value is written
        /// unchanged and the field is named. It is a different fact from a font nobody could
        /// measure and it is counted apart.
        /// </summary>
        [Fact]
        public void AFieldWithNoDefaultAppearanceIsWrittenUnchangedAndNamed()
        {
            PdfFieldFit fit = Fit("2797.64", 30.0, 14.0, "0 g");

            Assert.Equal(PdfFitOutcome.NoDefaultAppearance, fit.Outcome);
            Assert.False(fit.Writes);
            Assert.Equal(PdfTextFit.NoDefaultAppearance, fit.Why);
        }

        /// <summary>
        /// **A /Rect WITH NO WIDTH OR NO HEIGHT IS NOT A BOX.** Nothing is fitted into it and the
        /// field keeps the client's size, rather than a nought becoming a size.
        /// </summary>
        [Fact]
        public void ABoxWithNoSizeFitsNothingAndKeepsTheClientsSize()
        {
            PdfFieldFit fit = PdfTextFit.Of(
                "Area", "2797.64", PdfTextBox.NotRead, PdfDefaultAppearance.Of("/Helv 8 Tf 0 g"), Helvetica);

            Assert.Equal(PdfFitOutcome.WidthsUnknown, fit.Outcome);
            Assert.Equal(PdfTextFit.BoxNotRead, fit.Why);
        }

        /// <summary>
        /// **A RECTANGLE MAY NAME ITS CORNERS EITHER WAY ROUND**, which a PDF allows, so the
        /// size is the distance and never the subtraction.
        /// </summary>
        [Fact]
        public void ARectangleNamedBackwardsStillHasItsSize()
        {
            var box = new PdfTextBox(120.0, 60.0, 90.0, 46.0);

            Assert.Equal(30.0, box.Width);
            Assert.Equal(14.0, box.Height);
            Assert.True(box.Read);
        }

        /// <summary>
        /// **ONLY THE SIZE MOVES INSIDE THE /DA.** The client's font name and their colour
        /// operators are carried through exactly as the file holds them.
        /// </summary>
        [Fact]
        public void OnlyTheSizeChangesInsideTheDefaultAppearance()
        {
            PdfDefaultAppearance da = PdfDefaultAppearance.Of("0.25 0.25 0.6 rg /HeBo 0 Tf");

            Assert.True(da.Read);
            Assert.True(da.IsAuto);
            Assert.Equal("HeBo", da.FontResource);
            Assert.Equal("0.25 0.25 0.6 rg /HeBo 7.1 Tf", da.WithSize(7.1));
        }

        /// <summary>
        /// **EVERY STANDARD TABLE COVERS EVERY PRINTABLE CHARACTER THIS TOOL CAN WRITE**, so a
        /// field is never left unmeasured over a comma. Walked rather than trusted.
        /// </summary>
        [Fact]
        public void EveryStandardFontMeasuresEveryPrintableCharacter()
        {
            foreach (string name in StandardFonts.All)
            {
                PdfFontWidths widths = StandardFonts.For(name);
                Assert.True(widths.Read, name + " is in the table and could not be read");

                for (int code = PdfFontWidths.FirstCode; code <= PdfFontWidths.LastCode; code++)
                {
                    string one = ((char)code).ToString();

                    Assert.True(
                        widths.Ems(one) > 0.0,
                        name + " has no width for '" + one + "', code " + code);
                }
            }
        }

        /// <summary>
        /// **COURIER IS 600 FOR EVERY CHARACTER, which is what monospaced means.** It is the one
        /// table nobody can get subtly wrong.
        /// </summary>
        [Fact]
        public void EveryCourierFaceIsSixHundredThroughout()
        {
            foreach (string name in new[]
            {
                "Courier", "Courier-Bold", "Courier-Oblique", "Courier-BoldOblique"
            })
            {
                PdfFontWidths widths = StandardFonts.For(name);

                Assert.Equal(0.6, widths.Ems("A"), 6);
                Assert.Equal(3.0, widths.Ems("12345"), 6);
            }
        }

        /// <summary>
        /// **THE OBLIQUE FACES SHARE THEIR UPRIGHT'S WIDTHS AND TIMES-ITALIC DOES NOT.** The
        /// first is one design slanted and the second is a design of its own.
        /// </summary>
        [Fact]
        public void TheObliqueFacesShareTheirUprightsWidthsAndTimesItalicDoesNot()
        {
            Assert.Equal(
                StandardFonts.For("Helvetica").Ems("Hamburg"),
                StandardFonts.For("Helvetica-Oblique").Ems("Hamburg"), 6);

            Assert.Equal(
                StandardFonts.For("Helvetica-Bold").Ems("Hamburg"),
                StandardFonts.For("Helvetica-BoldOblique").Ems("Hamburg"), 6);

            Assert.NotEqual(
                StandardFonts.For("Times-Roman").Ems("Hamburg"),
                StandardFonts.For("Times-Italic").Ems("Hamburg"), 6);
        }

        /// <summary>
        /// **A SUBSET PREFIX IS NOT PART OF THE NAME.** A font subset into a file reads
        /// `ABCDEF+Helvetica`, and six letters and a plus are not a font nobody has heard of.
        /// </summary>
        [Fact]
        public void ASubsetPrefixComesOffBeforeTheLookup()
        {
            Assert.Equal("Helvetica", StandardFonts.Named("ABCDEF+Helvetica"));
            Assert.Equal("Helvetica", StandardFonts.Named("/Helvetica"));
            Assert.Equal("Helvetica-Bold", StandardFonts.Named("  Helvetica-Bold  "));
            Assert.True(StandardFonts.For("QWERTY+Times-Roman").Read);
        }

        /// <summary>
        /// **NO NEAR MISS IS IN THAT TABLE.** Arial is metric compatible with Helvetica and
        /// Courier New with Courier, and neither is written in, because a name standing in for a
        /// measurement is the fault this repository keeps paying for. Such a font must carry its
        /// own `/Widths` in the file, and where it does not the field is named.
        /// </summary>
        [Fact]
        public void ANameThatIsNotOneOfTheStandardFourteenMeasuresNothing()
        {
            foreach (string name in new[] { "Arial", "ArialMT", "CourierNewPSMT", "Symbol", "ZapfDingbats" })
            {
                PdfFontWidths widths = StandardFonts.For(name);

                Assert.False(widths.Read, name + " was answered for out of the standard fourteen");
                Assert.Equal(StandardFonts.NotOneOfTheFourteen, widths.Why);
            }
        }

        /// <summary>
        /// **AND THE FITTED SIZE REALLY LANDS IN THE WRITTEN FILE.** Read back off the output,
        /// the same rule every written cell of the workbook already follows. A box 30 pt wide
        /// and 14 pt tall holding `2797.64` comes out at 7.1 pt, and the VALUE is untouched.
        /// </summary>
        [Fact]
        public void TheWrittenFileCarriesTheFittedSizeAndTheValueUnchanged()
        {
            byte[] file = PdfFixture.Bytes(new[]
            {
                PdfFixture.Sized("Area", "REVIT 00 LINK", 30.0, 14.0, "/Helv 0 Tf 0 g")
            });

            IReadOnlyList<PdfFieldFit> fits;
            string refusal;
            byte[] filled = PdfFormFile.Filled(
                file, new[] { new KeyValuePair<string, string>("Area", "2797.64") }, out fits, out refusal);

            Assert.Equal(string.Empty, refusal);
            Assert.NotNull(filled);

            PdfFieldFit fit = Assert.Single(fits);
            Assert.Equal(PdfFitOutcome.Shrunk, fit.Outcome);
            Assert.Equal(7.1, fit.Size);

            PdfFieldRead back = PdfFormFile.Fields(filled, out refusal)
                .Single(one => string.Equals(one.Name, "Area", StringComparison.Ordinal));

            Assert.Equal("2797.64", back.Value);
            Assert.True(back.DefaultAppearance.Read);
            Assert.Equal(7.1, back.DefaultAppearance.Size);
            Assert.Equal("Helv", back.DefaultAppearance.FontResource);
            Assert.Equal(30.0, back.Box.Width);
            Assert.Equal(14.0, back.Box.Height);
        }

        /// <summary>
        /// **EMPTYING A FIELD PUTS NO TEXT IN IT**, so there is nothing to fit and the client's
        /// own size is left exactly as it was.
        /// </summary>
        [Fact]
        public void AnEmptiedFieldIsNotFittedAndKeepsItsOwnAppearance()
        {
            byte[] file = PdfFixture.Bytes(new[]
            {
                PdfFixture.Sized("Toilets", "nr", 30.0, 14.0, "/Helv 9 Tf 0 g")
            });

            IReadOnlyList<PdfFieldFit> fits;
            string refusal;
            byte[] filled = PdfFormFile.Filled(
                file, new[] { new KeyValuePair<string, string>("Toilets", string.Empty) }, out fits, out refusal);

            Assert.Empty(fits);

            PdfFieldRead back = PdfFormFile.Fields(filled, out refusal)
                .Single(one => string.Equals(one.Name, "Toilets", StringComparison.Ordinal));

            Assert.Equal(string.Empty, back.Value);
            Assert.Equal(9.0, back.DefaultAppearance.Size);
        }

        /// <summary>
        /// **ONE COUNT PER OUTCOME AT THE TOP OF THE REPORT, and a line only for the boxes
        /// somebody has to look at.** Over 150 plots a line per field is 2,000 lines nobody
        /// reads, and a count alone says a size is wrong somewhere without saying which box.
        /// </summary>
        [Fact]
        public void TheGlanceCountsEveryOutcomeAndNamesOnlyTheBoxesToLookAt()
        {
            var glance = new TextFitGlance(new[]
            {
                Fit("12", 200.0, 40.0, "/Helv 9 Tf 0 g").With(9.0),
                Fit("2797.64", 30.0, 14.0, "/Helv 0 Tf 0 g").With(7.1),
                Fit("12", 200.0, 40.0, "/Helv 0 Tf 0 g").With(10.0),
                Fit("2797.64", 20.0, 14.0, "/Helv 0 Tf 0 g").With(6.0),
                Fit("2797.64", 30.0, 14.0, "0 g").With(-1.0)
            });

            Assert.Equal(1, glance.Count(PdfFitOutcome.KeptTheClientsSize));
            Assert.Equal(1, glance.Count(PdfFitOutcome.Shrunk));
            Assert.Equal(1, glance.Count(PdfFitOutcome.CappedAtTen));
            Assert.Equal(1, glance.Count(PdfFitOutcome.HeldAtSix));
            Assert.Equal(0, glance.Count(PdfFitOutcome.WidthsUnknown));
            Assert.Equal(1, glance.Count(PdfFitOutcome.NoDefaultAppearance));

            Assert.Contains(
                "THE TEXT SIZES: 5 values were written, 1 at the client's own size, 1 shrunk to "
                + "fit, 1 capped at 10 where the client's /DA gives nought, 1 held at 6 and "
                + "running over, 0 with the font's widths UNKNOWN, and 1 with no /DA to change.",
                glance.InWords);

            // Two named: the one held at 6, which runs over, and the one with no /DA at all.
            Assert.Equal(2, glance.Named.Count);
            Assert.Empty(glance.DidNotLand);
        }

        /// <summary>
        /// **A SIZE THIS RUN SET THAT DID NOT LAND IS A BUG IN THE TOOL AND IS COUNTED AS ONE.**
        /// It is a different fact from a size the tool never tried to set.
        /// </summary>
        [Fact]
        public void ASizeThatDidNotLandIsCountedApart()
        {
            var glance = new TextFitGlance(new[]
            {
                Fit("2797.64", 30.0, 14.0, "/Helv 0 Tf 0 g").With(0.0),
                Fit("2797.64", 30.0, 14.0, "/Helv 0 Tf 0 g").With(7.1)
            });

            PdfFieldFit missed = Assert.Single(glance.DidNotLand);
            Assert.Equal(7.1, missed.Size);
            Assert.Equal(0.0, missed.Landed);
            Assert.Contains("did NOT land in the written file", glance.InWords);
        }

        /// <summary>
        /// **A FIELD THAT STATES NO /DA INHERITS ONE**, through its parents and then the
        /// AcroForm's own, which is how an AcroForm is defined. The open spaces form's lawn area
        /// sits under a parent, which is the shape this walks.
        /// </summary>
        [Fact]
        public void AFieldInheritsTheAppearanceItDoesNotState()
        {
            byte[] file = PdfFixture.Bytes(new[]
            {
                PdfFixture.Field(PdfFixture.ParentedFieldName, "sum of the above", 40.0, 40.0)
            });

            string refusal;
            PdfFieldRead field = PdfFormFile.Fields(file, out refusal)
                .Single(one => string.Equals(one.Name, PdfFixture.ParentedFieldName, StringComparison.Ordinal));

            Assert.True(field.DefaultAppearance.Read);
            Assert.Equal("Helv", field.DefaultAppearance.FontResource);
        }
    }
}
