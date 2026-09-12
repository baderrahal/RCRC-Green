using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// What one model can honour out of one preset. A preset is shared across projects, so a
    /// model without a view type or a title block it names fills in the rest and says what it
    /// could not take.
    /// </summary>
    public class PresetFitTests
    {
        private const string Family = "AR-PRX-Title_Block_A1";

        private static readonly ViewType Drawings = new ViewType("010", "LIST OF DRAWINGS");
        private static readonly ViewType Location = new ViewType("010", "Location Key Plan");
        private static readonly ViewType General =
            new ViewType("200", "General Arrangement Layout");
        private static readonly ViewType Hardscape = new ViewType("600", "HARDSCAPE SCHEDULE");

        private static PresetSheet Sheet(string typeName, params ViewType[] views)
        {
            return new PresetSheet(Family, typeName, 1, views);
        }

        private static TitleBlockType Block(string typeName)
        {
            return new TitleBlockType(Family, typeName);
        }

        private static Preset Three()
        {
            return new Preset(
                "Starting point",
                new[] { Drawings, Location, General },
                new[] { Sheet("LOD", Drawings), Sheet("KEYPLAN", Location, General) });
        }

        [Fact]
        public void AModelHoldingEverythingTakesAllOfIt()
        {
            PresetFit fit = PresetFit.Of(
                Three(),
                new[] { Drawings, Location, General },
                new[] { Block("LOD"), Block("KEYPLAN") });

            Assert.True(fit.EverythingFits);
            Assert.False(fit.NothingFits);
            Assert.Equal(3, fit.Ticked.Count);
            Assert.Equal(2, fit.Sheets.Count);
            Assert.Equal(string.Empty, fit.WhatIsMissing());
            Assert.Equal(
                "Filled from Starting point: 3 view types and 2 sheet definitions.",
                fit.InWords());
        }

        [Fact]
        public void AViewTypeTheModelDoesNotHoldIsNotTickedAndIsNamed()
        {
            PresetFit fit = PresetFit.Of(
                Three(),
                new[] { Drawings, General },
                new[] { Block("LOD"), Block("KEYPLAN") });

            Assert.False(fit.EverythingFits);
            Assert.Equal(
                new[] { "(010) Location Key Plan" },
                fit.TypesNotHeld.Select(one => one.ToString()).ToArray());

            Assert.Equal(
                "Filled from Starting point: 2 view types and 2 sheet definitions. This model "
                    + "holds no view called (010) Location Key Plan. (010) Location Key Plan "
                    + "came off a sheet it was on.",
                fit.InWords());
        }

        [Fact]
        public void AViewTheModelDoesNotHoldComesOffTheSheetItWasOn()
        {
            PresetFit fit = PresetFit.Of(
                Three(),
                new[] { Drawings, General },
                new[] { Block("LOD"), Block("KEYPLAN") });

            Assert.Equal(
                new[] { "(200) General Arrangement Layout" },
                fit.Sheets[1].Views.Select(one => one.ToString()).ToArray());

            Assert.Equal(
                new[] { "(010) Location Key Plan" },
                fit.ViewsDroppedFromSheets.Select(one => one.ToString()).ToArray());
        }

        [Fact]
        public void ATitleBlockTheModelDoesNotHoldLeavesItsSheetOutAndNamesIt()
        {
            PresetFit fit = PresetFit.Of(
                Three(),
                new[] { Drawings, Location, General },
                new[] { Block("LOD") });

            Assert.Single(fit.Sheets);
            Assert.Equal(
                new[] { "AR-PRX-Title_Block_A1 KEYPLAN" },
                fit.SheetsNotHeld.Select(one => one.TitleBlock).ToArray());

            Assert.Equal(
                "Filled from Starting point: 3 view types and 1 sheet definition. This model "
                    + "holds no title block called AR-PRX-Title_Block_A1 KEYPLAN, so 1 sheet "
                    + "definition is left out.",
                fit.InWords());
        }

        [Fact]
        public void AViewOnASheetWhoseTitleBlockIsMissingIsNotCountedTwice()
        {
            PresetFit fit = PresetFit.Of(
                Three(),
                new[] { Drawings, General },
                new[] { Block("LOD") });

            // Location Key Plan is in neither the model nor a kept sheet, so it is named once
            // under the types and nowhere else.
            Assert.Empty(fit.ViewsDroppedFromSheets);
            Assert.Equal(
                "Filled from Starting point: 2 view types and 1 sheet definition. This model "
                    + "holds no view called (010) Location Key Plan and no title block called "
                    + "AR-PRX-Title_Block_A1 KEYPLAN, so 1 sheet definition is left out.",
                fit.InWords());
        }

        [Fact]
        public void AModelHoldingNoneOfItSaysSoAndChangesNothing()
        {
            PresetFit fit = PresetFit.Of(Three(), null, null);

            Assert.True(fit.NothingFits);
            Assert.Empty(fit.Ticked);
            Assert.Empty(fit.Sheets);
            Assert.Equal(
                "Nothing in Starting point is in this model, so steps 2 and 4 are unchanged. "
                    + "This model holds no views called (010) LIST OF DRAWINGS, (010) Location "
                    + "Key Plan, (200) General Arrangement Layout and no title blocks called "
                    + "AR-PRX-Title_Block_A1 LOD, AR-PRX-Title_Block_A1 KEYPLAN, so 2 sheet "
                    + "definitions are left out.",
                fit.InWords());
        }

        [Fact]
        public void ATitleBlockOfTheSameTypeNameInAnotherFamilyIsNotTheSameBlock()
        {
            PresetFit fit = PresetFit.Of(
                new Preset("x", null, new[] { Sheet("LOD", Drawings) }),
                new[] { Drawings },
                new[] { new TitleBlockType("AR-PRX-Title_Block_A0", "LOD") });

            Assert.Empty(fit.Sheets);
            Assert.Single(fit.SheetsNotHeld);
        }

        [Fact]
        public void ATitleBlockTypeIsMatchedOnEverySpaceTheModelGaveIt()
        {
            var preset = new Preset(
                "x", new[] { Hardscape },
                new[] { Sheet("LOD /  HARDSCAPE SCHEDULES", Hardscape) });

            Assert.Empty(PresetFit
                .Of(preset, new[] { Hardscape }, new[] { Block("LOD / HARDSCAPE SCHEDULES") })
                .Sheets);

            Assert.Single(PresetFit
                .Of(preset, new[] { Hardscape }, new[] { Block("LOD /  HARDSCAPE SCHEDULES") })
                .Sheets);
        }

        [Fact]
        public void AKeptSheetKeepsItsViewsPerSheetAndItsOrder()
        {
            var preset = new Preset(
                "x", new[] { Drawings, Location },
                new[] { new PresetSheet(Family, "KEYPLAN", 2, new[] { Location, Drawings }) });

            PresetFit fit = PresetFit.Of(
                preset, new[] { Drawings, Location }, new[] { Block("KEYPLAN") });

            Assert.Equal(2, fit.Sheets[0].ViewsPerSheet);
            Assert.Equal(
                new[] { "(010) Location Key Plan", "(010) LIST OF DRAWINGS" },
                fit.Sheets[0].Views.Select(one => one.ToString()).ToArray());
        }

        [Fact]
        public void ATitleSheetWithNoViewsFitsAModelHoldingItsBlock()
        {
            PresetFit fit = PresetFit.Of(
                new Preset("x", null, new[] { Sheet("COVER PAGE") }),
                null,
                new[] { Block("COVER PAGE") });

            Assert.True(fit.EverythingFits);
            Assert.False(fit.NothingFits);
            Assert.Single(fit.Sheets);
            Assert.Empty(fit.Sheets[0].Views);
        }

        [Fact]
        public void APresetIsAlwaysNeeded()
        {
            Assert.Throws<ArgumentNullException>(() => PresetFit.Of(null, null, null));
        }
    }
}
