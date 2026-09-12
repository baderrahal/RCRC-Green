using System;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// The two dropdown lists. Both counts and every spot check are worked out by hand: 26
    /// single letters, 676 pairs, 17,576 triples, and 999 three digit numbers.
    /// </summary>
    public class MarkerChoicesTests
    {
        [Fact]
        public void TheLettersRunAToZThenAaToZzThenAaaToZzz()
        {
            Assert.Equal(18278, MarkerChoices.Letters.Count);
            Assert.Equal("A", MarkerChoices.Letters[0]);
            Assert.Equal("Z", MarkerChoices.Letters[25]);
            Assert.Equal("AA", MarkerChoices.Letters[26]);
            Assert.Equal("ZZ", MarkerChoices.Letters[701]);
            Assert.Equal("AAA", MarkerChoices.Letters[702]);
            Assert.Equal("ZZZ", MarkerChoices.Letters[18277]);
        }

        [Fact]
        public void TheNumbersRunThreeWideFromOneToNineNineNine()
        {
            Assert.Equal(999, MarkerChoices.Numbers.Count);
            Assert.Equal("001", MarkerChoices.Numbers[0]);
            Assert.Equal("010", MarkerChoices.Numbers[9]);
            Assert.Equal("999", MarkerChoices.Numbers[998]);
        }
    }

    /// <summary>
    /// One model's markers and where each came from. Set by the user, remembered per model,
    /// never derived.
    /// </summary>
    public class PlotMarkersTests
    {
        [Fact]
        public void AStoredMarkerReadsAsRemembered()
        {
            PlotMarkers held = PlotMarkers.Remembered(new[]
            {
                new PlotMarker("DM-11", "Q", MarkerSource.Unset)
            });

            Assert.Equal("Q", held.MarkerOf("DM-11"));
            Assert.Equal(MarkerSource.Remembered, held.For("DM-11").Source);
            Assert.Equal("Marker Q, remembered.", held.WordsFor("DM-11"));
        }

        [Fact]
        public void AMarkerSetOnThePanelReadsAsSetNow()
        {
            PlotMarkers held = PlotMarkers.Nothing.With("FP-39", " 001 ");

            Assert.Equal("001", held.MarkerOf("FP-39"));
            Assert.Equal(MarkerSource.SetNow, held.For("FP-39").Source);
            Assert.Equal("Marker 001, set now.", held.WordsFor("FP-39"));
        }

        /// <summary>
        /// The unset line says the cost, because the run really does make no sheet for the
        /// plot and finding that out at Run is too late.
        /// </summary>
        [Fact]
        public void APlotWithNoMarkerSaysWhatThatCosts()
        {
            Assert.Null(PlotMarkers.Nothing.For("DM-16"));
            Assert.Equal(string.Empty, PlotMarkers.Nothing.MarkerOf("DM-16"));
            Assert.Equal(
                "No marker. A plot with no marker gets no sheet.",
                PlotMarkers.Nothing.WordsFor("DM-16"));
        }

        [Fact]
        public void SettingAnEmptyMarkerClearsThePlot()
        {
            PlotMarkers held = PlotMarkers.Nothing.With("DM-11", "Q").With("DM-11", "  ");

            Assert.Null(held.For("DM-11"));
            Assert.Empty(held.All);
        }

        [Fact]
        public void TheOtherPlotsMarkersAreWhatThisPlotIsNotOffered()
        {
            PlotMarkers held = PlotMarkers.Nothing
                .With("DM-11", "Q")
                .With("DM-12", "R");

            Assert.Equal(new[] { "R" }, held.MarkersOfOtherPlots("DM-11").ToArray());
            Assert.Equal(new[] { "Q", "R" }, held.MarkersOfOtherPlots("DM-16").ToArray());
        }

        [Fact]
        public void WhoHoldsNamesTheOtherPlotAndSkipsTheAskersOwn()
        {
            PlotMarkers held = PlotMarkers.Nothing.With("DM-11", "Q");

            Assert.Equal("DM-11", held.WhoHolds("Q", "DM-16"));
            Assert.Equal(string.Empty, held.WhoHolds("Q", "DM-11"));
            Assert.Equal(string.Empty, held.WhoHolds("R", "DM-16"));
        }
    }

    /// <summary>
    /// The marker file: tab separated, three fields, model then plot then marker, notes and
    /// blanks skipped, and every line that could not be read kept and said.
    /// </summary>
    public class PlotMarkerFileTests
    {
        [Fact]
        public void WhatIsWrittenReadsBackAndKeepsEveryModelTogether()
        {
            string text = PlotMarkerFile.Write(new[]
            {
                new PlotMarkerRow("RCRC_NG05_NU_MAIN", "DM-2", "R"),
                new PlotMarkerRow("RCRC_NG03_EZ_MAIN", "FP-39", "001"),
                new PlotMarkerRow("RCRC_NG05_NU_MAIN", "DM-10", "S")
            });

            PlotMarkerFileContents held = PlotMarkerFile.Read(text);

            Assert.Empty(held.NotRead);
            Assert.Equal(3, held.Rows.Count);

            // Models together, plots in natural order inside one, so DM-2 sits before DM-10.
            Assert.Equal(
                new[] { "FP-39", "DM-2", "DM-10" },
                held.Rows.Select(one => one.PlotId).ToArray());

            var markers = held.For("RCRC_NG05_NU_MAIN");
            Assert.Equal(2, markers.Count);
            Assert.All(markers, one => Assert.Equal(MarkerSource.Remembered, one.Source));
        }

        [Fact]
        public void OneModelsRowsDoNotLeakIntoAnothers()
        {
            string text = PlotMarkerFile.Write(new[]
            {
                new PlotMarkerRow("NG05", "DM-11", "Q"),
                new PlotMarkerRow("NG03", "FP-39", "001")
            });

            var markers = PlotMarkerFile.Read(text).For("NG03");

            PlotMarker only = Assert.Single(markers);
            Assert.Equal("FP-39", only.PlotId);
            Assert.Equal("001", only.Marker);
            Assert.Empty(PlotMarkerFile.Read(text).For("NG04"));
        }

        [Fact]
        public void ALineShortOfItsFieldsIsKeptWithItsNumber()
        {
            PlotMarkerFileContents held = PlotMarkerFile.Read(
                "# a note\r\nNG05\tDM-11\tQ\r\nNG05\tDM-12\r\n\r\n");

            Assert.Single(held.Rows);
            string bad = Assert.Single(held.NotRead);
            Assert.Equal("line 3, NG05\tDM-12", bad);
        }

        [Fact]
        public void FieldsAreTrimmedAndEmptyRowsAreNotWritten()
        {
            var row = new PlotMarkerRow(" NG05 ", " DM-11 ", " Q ");
            Assert.Equal("NG05", row.Model);
            Assert.Equal("DM-11", row.PlotId);
            Assert.Equal("Q", row.Marker);

            string text = PlotMarkerFile.Write(new[]
            {
                row,
                new PlotMarkerRow("NG05", "DM-12", string.Empty)
            });

            PlotMarker only = Assert.Single(PlotMarkerFile.Read(text).For("NG05"));
            Assert.Equal("DM-11", only.PlotId);
        }

        [Fact]
        public void NothingReadsAsNothing()
        {
            Assert.Empty(PlotMarkerFile.Read(string.Empty).Rows);
            Assert.Empty(PlotMarkerFile.Read(null).Rows);
        }
    }

    /// <summary>
    /// Which markers the model's own numbers already use. Every expected set is worked out
    /// by hand from the two measured numbering styles, and the ambiguity is kept on purpose:
    /// 010QE cannot say whether its marker is Q or QE, so both are counted in use.
    /// </summary>
    public class MarkerLedgerTests
    {
        private static string[] Sorted(System.Collections.Generic.IEnumerable<string> markers)
        {
            return markers.OrderBy(one => one, StringComparer.Ordinal).ToArray();
        }

        [Fact]
        public void ALettersTailCountsItselfAndItselfShortOneLetter()
        {
            Assert.Equal(
                new[] { "Q", "QC", "QE", "QF" },
                Sorted(MarkerLedger.MarkersIn(new[] { "010QE", "010QF", "200Q", "600QC" })));
        }

        /// <summary>
        /// The NG03 style. The three digits before the sheet letter are the marker and the
        /// letter is not, so 001 is in use and A is not. Counting A as well would bar most of
        /// the alphabet on a model where every plot carries 010xxxA to D.
        /// </summary>
        [Fact]
        public void ADigitMarkerCountsTheDigitsAndNeverTheSheetLetter()
        {
            Assert.Equal(
                new[] { "001" },
                Sorted(MarkerLedger.MarkersIn(new[] { "010001A", "200001" })));
        }

        [Fact]
        public void CopyNumbersAndUnshapedNumbersSayNothing()
        {
            Assert.Empty(MarkerLedger.MarkersIn(new[] { "010QE Copy 001", "400Q Copy 009" }));
            Assert.Empty(MarkerLedger.MarkersIn(new[] { "L-211", "COVER", "010Q-1" }));
            Assert.Empty(MarkerLedger.MarkersIn(null));
        }

        [Fact]
        public void ALowerCaseTailReadsAsItsCapitals()
        {
            Assert.Equal(new[] { "Q" }, Sorted(MarkerLedger.MarkersIn(new[] { "010q" })));
        }

        /// <summary>
        /// A plot is barred the other plots' markers and the ones on sheets with no plot,
        /// and never its own: Q is exactly what the user will pick for DM-11.
        /// </summary>
        [Fact]
        public void APlotIsBarredEveryMarkerButItsOwn()
        {
            MarkerLedger ledger = MarkerLedger.Of(
                new[]
                {
                    new SheetOnAPlot("DM-11", "200Q"),
                    new SheetOnAPlot("DM-12", "300R")
                },
                new[] { "200Q", "300R", "400S" });

            var barred = ledger.BarredFor("DM-11", PlotMarkers.Nothing);
            Assert.Contains("R", barred);
            Assert.Contains("S", barred);
            Assert.DoesNotContain("Q", barred);

            PlotMarkers held = PlotMarkers.Nothing.With("DM-13", "T");
            Assert.Contains("T", ledger.BarredFor("DM-11", held));
            Assert.DoesNotContain("T", ledger.BarredFor("DM-13", held));
        }

        [Fact]
        public void TheWarningNamesWhoHasTheMarker()
        {
            MarkerLedger ledger = MarkerLedger.Of(
                new[] { new SheetOnAPlot("DM-12", "200Q") },
                new[] { "200Q", "400S" });

            Assert.Equal(
                "Marker Q is already used by DM-12's sheet numbers in this model.",
                ledger.Warning("DM-11", PlotMarkers.Nothing.With("DM-11", "Q")));

            Assert.Equal(
                "Marker S is already used by a sheet number in this model on a sheet "
                + "carrying no plot.",
                ledger.Warning("DM-11", PlotMarkers.Nothing.With("DM-11", "S")));

            PlotMarkers doubled = PlotMarkers.Nothing.With("DM-11", "T").With("DM-16", "T");
            Assert.Equal(
                "Marker T is set on DM-11 in step 1 as well, so their sheet numbers would "
                + "collide.",
                ledger.Warning("DM-16", doubled));

            Assert.Equal(string.Empty,
                ledger.Warning("DM-11", PlotMarkers.Nothing.With("DM-11", "V")));
            Assert.Equal(string.Empty, ledger.Warning("DM-11", PlotMarkers.Nothing));
        }
    }
}
