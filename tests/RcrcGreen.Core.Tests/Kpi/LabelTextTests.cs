using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// The one rule for comparing a label against what a file holds, and the label texts written
    /// out by hand as the seven templates carry them.
    ///
    /// **ALL SEVEN CARRY A LEADING SPACE ON THE GREEN COVER LABEL**, measured by Bader on
    /// 14 September:
    ///
    /// <code>
    /// EXISTING PARKS   C9   ' Total Green cover (m²)'
    /// FUTURE PARKS     C9   ' Total Green cover (m²)'
    /// HEALTHCARE       C8   ' Total Green cover (m²)'
    /// MOSQUES          C8   ' Total Green cover (m²)'
    /// PARKING          C8   ' Total Green cover (m²)'
    /// SCHOOLS          C8   ' Total Green cover (m²)'
    /// STREETS          C9   ' Total Green cover (m²)'
    ///
    /// HEALTHCARE, MOSQUES, PARKING, SCHOOLS   C31  '% of Total area covered by canopy'
    /// STREETS                                 C32  '% of Total area covered by canopy'
    /// </code>
    ///
    /// The client's PDF note writes the first WITHOUT the space, and that is where the constant
    /// came from the first time.
    /// </summary>
    public class LabelTextTests
    {
        /// <summary>
        /// **A LABEL IS WHAT THE FILE HOLDS, NOT WHAT A NOTE CALLS IT.** The constant carries the
        /// leading space every one of the seven templates was measured to hold, and the
        /// percentage label carries none.
        /// </summary>
        [Fact]
        public void TheGreenCoverLabelCarriesTheLeadingSpaceTheTemplatesHold()
        {
            Assert.Equal(" Total Green cover (m²)", ComputedPlaces.GreenCoverLabel);
            Assert.StartsWith(" ", ComputedPlaces.GreenCoverLabel, StringComparison.Ordinal);

            Assert.Equal("% of Total area covered by canopy", ComputedPlaces.PercentageLabel);
            Assert.False(
                ComputedPlaces.PercentageLabel.StartsWith(" ", StringComparison.Ordinal),
                "the percentage label carries no leading space on any template");
        }

        /// <summary>
        /// **Edge whitespace off BOTH sides.** The rule used to trim the cell where it was read
        /// and compare that against the label as written, which is one side of a two sided
        /// question, so a label constant carrying a stray space failed with nothing saying why.
        /// </summary>
        [Theory]
        [InlineData(" Total Green cover (m²)", "Total Green cover (m²)")]
        [InlineData("Total Green cover (m²)", " Total Green cover (m²)")]
        [InlineData(" Total Green cover (m²) ", " Total Green cover (m²)")]
        [InlineData("REF :", "  REF :  ")]
        [InlineData("date:", "Date:")]
        [InlineData("", "   ")]
        public void TwoLabelsAreTheSameWithTheirEdgeWhitespaceOff(string one, string other)
        {
            Assert.True(LabelText.Same(one, other));
            Assert.True(LabelText.Same(other, one));
        }

        /// <summary>
        /// **DO NOT TRIM INSIDE THE LABEL. A double space between words is a different label.**
        /// A title block in this project really is named `LOD /  HARDSCAPE SCHEDULES` with two
        /// spaces, so a comparison that collapsed runs would answer for a name no file holds.
        /// </summary>
        [Theory]
        [InlineData("Total  Green cover (m²)", "Total Green cover (m²)")]
        [InlineData("LOD /  HARDSCAPE SCHEDULES", "LOD / HARDSCAPE SCHEDULES")]
        [InlineData("Prepared By:", "PreparedBy:")]
        [InlineData("Character", "Characteristics")]
        [InlineData("Total Green cover", "Total Green cover (m²)")]
        public void ADoubleSpaceInsideIsADifferentLabel(string one, string other)
        {
            Assert.False(LabelText.Same(one, other));
            Assert.False(LabelText.Same(other, one));
        }

        /// <summary>
        /// The trim keeps every character between the ends, which is the half of the rule worth
        /// saying out loud beside the half everyone expects.
        /// </summary>
        [Fact]
        public void TheTrimTakesTheEndsAndLeavesTheInside()
        {
            Assert.Equal("LOD /  HARDSCAPE SCHEDULES", LabelText.Trimmed("  LOD /  HARDSCAPE SCHEDULES  "));
            Assert.Equal(string.Empty, LabelText.Trimmed(null));
        }

        /// <summary>
        /// **EVERY WHOLE LABEL THIS TOOL LOOKS UP, checked one by one.** Five in the table that
        /// gets written, two in the read only table, and four column names in the team's street
        /// reference file. None of the eleven carries an edge space today and all eleven now go
        /// through the same rule, so one that grows a space on the next issue costs nothing.
        /// </summary>
        [Fact]
        public void EveryLabelThisToolLooksUpIsSpeltAsItWasMeasured()
        {
            Assert.Equal("REF :", LabelledPlaces.ReferenceLabel);
            Assert.Equal("Date:", LabelledPlaces.DateLabel);
            Assert.Equal("Prepared By:", LabelledPlaces.PreparedByLabel);
            Assert.Equal("Character", FixedCells.CharacterLabel);
            Assert.Equal("Context", FixedCells.ContextLabel);

            Assert.Equal("ID_UID *", StreetReferenceFile.UidColumn);
            Assert.Equal("ES_QUANTITY", StreetReferenceFile.QuantityColumn);
            Assert.Equal("QUANTITY UNIT", StreetReferenceFile.UnitColumn);
            Assert.Equal("ROAD_WIDTH", StreetReferenceFile.WidthColumn);

            // The green cover is the one of the eleven that does carry an edge space.
            var labels = new List<string>
            {
                LabelledPlaces.ReferenceLabel, LabelledPlaces.DateLabel, LabelledPlaces.PreparedByLabel,
                FixedCells.CharacterLabel, FixedCells.ContextLabel,
                ComputedPlaces.PercentageLabel,
                StreetReferenceFile.UidColumn, StreetReferenceFile.QuantityColumn,
                StreetReferenceFile.UnitColumn, StreetReferenceFile.WidthColumn
            };

            Assert.All(labels, one => Assert.Equal(one, one.Trim()));
            Assert.NotEqual(ComputedPlaces.GreenCoverLabel, ComputedPlaces.GreenCoverLabel.Trim());
        }
    }
}
