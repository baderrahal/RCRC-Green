using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// A saved answer to steps 2 and 4, what one file holds and what two files merge to.
    /// </summary>
    public class PresetTests
    {
        private const string Family = "AR-PRX-Title_Block_A1";

        private static readonly ViewType Drawings = new ViewType("010", "LIST OF DRAWINGS");
        private static readonly ViewType Location = new ViewType("010", "Location Key Plan");
        private static readonly ViewType Overall = new ViewType("010", "Overall Key Plan");
        private static readonly ViewType General =
            new ViewType("200", "General Arrangement Layout");
        private static readonly ViewType Section =
            new ViewType("400", "Landscape Cross Section");
        private static readonly ViewType Hardscape = new ViewType("600", "HARDSCAPE SCHEDULE");

        private static PresetSheet Sheet(string typeName, int perSheet, params ViewType[] views)
        {
            return new PresetSheet(Family, typeName, perSheet, views);
        }

        private static Preset SixSheets()
        {
            return new Preset(
                "A starting point, from DM-11",
                new[] { Drawings, Location, Overall, General, Section, Hardscape },
                new[]
                {
                    Sheet("LOD", 1, Drawings),
                    Sheet("KEYPLAN", 1, Location, Overall),
                    Sheet("GA-SCHEMATIC", 1, General),
                    Sheet("SECTION / ENLARGEMENTS", 1, Section),
                    Sheet("LOD /  HARDSCAPE SCHEDULES", 1, Hardscape)
                });
        }

        [Fact]
        public void APresetHoldsItsTickedTypesInViewTypeOrder()
        {
            var preset = new Preset("x", new[] { Hardscape, Drawings, General }, null);

            Assert.Equal(
                new[] { "(010) LIST OF DRAWINGS", "(200) General Arrangement Layout",
                    "(600) HARDSCAPE SCHEDULE" },
                preset.Ticked.Select(one => one.ToString()).ToArray());
        }

        [Fact]
        public void ASheetKeepsItsViewsInTheOrderTheyWereTicked()
        {
            PresetSheet sheet = Sheet("KEYPLAN", 2, Overall, Location);

            Assert.Equal(
                new[] { "(010) Overall Key Plan", "(010) Location Key Plan" },
                sheet.Views.Select(one => one.ToString()).ToArray());
        }

        [Fact]
        public void OneViewTypeTwiceOnASheetIsOneView()
        {
            PresetSheet sheet = Sheet("KEYPLAN", 1, Location, Location);

            Assert.Single(sheet.Views);
        }

        [Fact]
        public void SixViewsAtTwoPerSheetIsThreeSheetsAndFiveIsThree()
        {
            Assert.Equal(3, Sheet(
                "LOD", 2, Drawings, Location, Overall, General, Section, Hardscape)
                .SheetsPerPlot);

            Assert.Equal(3, Sheet(
                "LOD", 2, Drawings, Location, Overall, General, Section).SheetsPerPlot);
        }

        [Fact]
        public void ASheetWithNoViewsIsOneTitleSheet()
        {
            Assert.Equal(1, Sheet("COVER PAGE", 1).SheetsPerPlot);
        }

        [Fact]
        public void AViewsPerSheetCountNobodyOffersFallsBackToOne()
        {
            Assert.Equal(1, Sheet("LOD", 3, Drawings, Location).ViewsPerSheet);
        }

        [Fact]
        public void TheStartingPointReadsAsSixSheetsOverFiveDefinitions()
        {
            Assert.Equal(
                "6 view types, 5 sheet definitions, 6 sheets per ticked plot.",
                SixSheets().InWords());
        }

        [Fact]
        public void APresetWrittenOutAndReadBackIsTheSamePreset()
        {
            string text = PresetFile.Write(new[] { SixSheets() });
            PresetFileContents read = PresetFile.Read(text);

            Assert.Empty(read.NotRead);
            Preset back = Assert.Single(read.Presets);

            Assert.Equal("A starting point, from DM-11", back.Name);
            Assert.Equal(6, back.Ticked.Count);
            Assert.Equal(5, back.Sheets.Count);
            Assert.Equal(
                new[] { "(010) Location Key Plan", "(010) Overall Key Plan" },
                back.Sheets[1].Views.Select(one => one.ToString()).ToArray());
        }

        [Fact]
        public void ATitleBlockTypeKeepsItsDoubleSpaceThroughTheFile()
        {
            PresetFileContents read = PresetFile.Read(PresetFile.Write(new[] { SixSheets() }));

            Assert.Equal(
                "LOD /  HARDSCAPE SCHEDULES", read.Presets[0].Sheets[4].TitleBlockTypeName);
        }

        [Fact]
        public void AViewNameKeepsEveryLeadingAndTrailingSpaceTheModelGaveIt()
        {
            var odd = new ViewType("010", " Overall Plan ");
            string text = PresetFile.Write(
                new[] { new Preset("x", new[] { odd }, null) });

            Assert.Equal(" Overall Plan ", PresetFile.Read(text).Presets[0].Ticked[0].ViewName);
        }

        [Fact]
        public void ANoteAndABlankLineAreNotRecords()
        {
            PresetFileContents read = PresetFile.Read(
                "# a note\r\n\r\npreset\tOne\r\ntype\t010\tOverall Plan\r\n");

            Assert.Empty(read.NotRead);
            Assert.Single(read.Presets);
            Assert.Single(read.Presets[0].Ticked);
        }

        [Fact]
        public void ARecordBeforeTheFirstPresetLineIsNotRead()
        {
            PresetFileContents read = PresetFile.Read(
                "type\t010\tOverall Plan\r\npreset\tOne\r\ntype\t200\tGeneral Arrangement Layout\r\n");

            Assert.Equal(
                new[] { "line 1, type\t010\tOverall Plan" }, read.NotRead.ToArray());
            Assert.Single(read.Presets);
            Assert.Equal(
                "(200) General Arrangement Layout", read.Presets[0].Ticked[0].ToString());
        }

        [Fact]
        public void AnOnLineBeforeAnySheetLineIsNotRead()
        {
            PresetFileContents read = PresetFile.Read(
                "preset\tOne\r\non\t010\tOverall Plan\r\n");

            Assert.Equal(new[] { "line 2, on\t010\tOverall Plan" }, read.NotRead.ToArray());
            Assert.Empty(read.Presets[0].Sheets);
        }

        [Fact]
        public void AWordThisDoesNotKnowIsNotRead()
        {
            PresetFileContents read = PresetFile.Read("preset\tOne\r\nplot\tDM-11\r\n");

            Assert.Equal(new[] { "line 2, plot\tDM-11" }, read.NotRead.ToArray());
        }

        [Fact]
        public void ASheetLineShortOfItsCountIsNotReadRatherThanGuessedAtOne()
        {
            PresetFileContents read = PresetFile.Read(
                "preset\tOne\r\nsheet\t" + Family + "\tLOD\r\n");

            Assert.Equal(
                new[] { "line 2, sheet\t" + Family + "\tLOD" }, read.NotRead.ToArray());
            Assert.Empty(read.Presets[0].Sheets);
        }

        [Fact]
        public void ASheetLineAskingForThreeViewsPerSheetIsNotRead()
        {
            PresetFileContents read = PresetFile.Read(
                "preset\tOne\r\nsheet\t" + Family + "\tLOD\t3\r\n");

            Assert.Single(read.NotRead);
            Assert.Empty(read.Presets[0].Sheets);
        }

        [Fact]
        public void APresetLineWithNoNameIsNotReadAndTheOneBeforeItIsKept()
        {
            PresetFileContents read = PresetFile.Read(
                "preset\tOne\r\ntype\t010\tOverall Plan\r\npreset\t\r\n");

            Assert.Equal(new[] { "line 3, preset\t" }, read.NotRead.ToArray());
            Assert.Single(read.Presets);
            Assert.Equal("One", read.Presets[0].Name);
        }

        [Fact]
        public void EveryLineThatCouldNotBeReadIsCountedInWords()
        {
            PresetFileContents read = PresetFile.Read(
                "preset\tOne\r\nplot\tDM-11\r\nplot\tDM-12\r\n");

            Assert.Equal(
                "1 presets read and 2 lines could not be: line 2, plot\tDM-11; "
                    + "line 3, plot\tDM-12",
                read.InWords());
        }

        [Fact]
        public void OnePresetAsksForWhatAnotherDoesWhateverItIsCalled()
        {
            Assert.True(SixSheets().SameAnswer(SixSheets()));

            var renamed = new Preset(
                "Something else", SixSheets().Ticked, SixSheets().Sheets,
                PresetSource.ShippedDefaults);

            Assert.True(SixSheets().SameAnswer(renamed));
            Assert.False(SixSheets().SameAnswer(null));
        }

        [Fact]
        public void ATickedTypeMoreOrFewerIsADifferentAnswer()
        {
            var one = new Preset("a", new[] { Drawings, Location }, null);

            Assert.False(one.SameAnswer(new Preset("a", new[] { Drawings }, null)));
            Assert.False(one.SameAnswer(
                new Preset("a", new[] { Drawings, Location, Overall }, null)));
        }

        [Fact]
        public void ASheetOnAnotherTitleBlockOrAnotherCountIsADifferentAnswer()
        {
            var one = new Preset("a", null, new[] { Sheet("LOD", 1, Drawings) });

            Assert.False(one.SameAnswer(
                new Preset("a", null, new[] { Sheet("KEYPLAN", 1, Drawings) })));

            Assert.False(one.SameAnswer(
                new Preset("a", null, new[] { Sheet("LOD", 2, Drawings) })));

            Assert.False(one.SameAnswer(
                new Preset("a", null, new[]
                {
                    new PresetSheet("AR-PRX-Title_Block_A0", "LOD", 1, new[] { Drawings })
                })));
        }

        [Fact]
        public void TheSameSheetsInAnotherOrderIsADifferentAnswer()
        {
            var one = new Preset("a", null, new[]
            {
                Sheet("LOD", 1, Drawings), Sheet("KEYPLAN", 1, Location)
            });

            var back = new Preset("a", null, new[]
            {
                Sheet("KEYPLAN", 1, Location), Sheet("LOD", 1, Drawings)
            });

            Assert.False(one.SameAnswer(back));
        }

        [Fact]
        public void TheSameViewsOnOneSheetInAnotherOrderIsADifferentAnswer()
        {
            Assert.False(
                new Preset("a", null, new[] { Sheet("KEYPLAN", 1, Location, Overall) })
                    .SameAnswer(
                        new Preset("a", null, new[] { Sheet("KEYPLAN", 1, Overall, Location) })));
        }

        [Fact]
        public void StepsFilledFromNothingSayNothing()
        {
            Assert.Equal(string.Empty, PresetFilling.InWords(null, SixSheets()));
            Assert.Equal(
                string.Empty, PresetFilling.InWords(new Preset(" ", null, null), SixSheets()));
        }

        [Fact]
        public void StepsStillHoldingWhatThePresetAskedForSayOnlyThat()
        {
            Assert.Equal(
                "Filled from A starting point, from DM-11.",
                PresetFilling.InWords(SixSheets(), SixSheets()));
        }

        [Fact]
        public void StepsChangedSincePickingSaySoRatherThanClaimingThePreset()
        {
            Assert.Equal(
                "Filled from A starting point, from DM-11, changed since. Save as keeps the "
                    + "change under a name.",
                PresetFilling.InWords(
                    SixSheets(), new Preset("whatever", new[] { General }, null)));
        }

        [Fact]
        public void ThePresetsAreNothingUntilAFileHoldsOne()
        {
            Assert.Equal(0, Presets.Nothing.Count);
            Assert.Null(Presets.Nothing.Named("anything"));
        }

        [Fact]
        public void AShippedPresetAndTheUsersOwnBothShowAndSayWhereTheyCameFrom()
        {
            Presets presets = Presets.Of(
                new[] { new Preset("Mine", null, null) },
                new[] { SixSheets() });

            Assert.Equal(
                new[] { "A starting point, from DM-11", "Mine" },
                presets.All.Select(one => one.Name).ToArray());

            Assert.Equal("the shipped defaults", presets.All[0].WhereItCameFrom());
            Assert.Equal("your own presets", presets.All[1].WhereItCameFrom());
        }

        [Fact]
        public void TheUsersOwnWinsOverAShippedPresetOfTheSameNameAndKeepsItsPlace()
        {
            Presets presets = Presets.Of(
                new[] { new Preset("A starting point, from DM-11", new[] { General }, null) },
                new[] { SixSheets(), new Preset("Second", null, null) });

            Assert.Equal(
                new[] { "A starting point, from DM-11", "Second" },
                presets.All.Select(one => one.Name).ToArray());

            Assert.Single(presets.All[0].Ticked);
            Assert.Equal(PresetSource.UserFile, presets.All[0].Source);
        }

        [Fact]
        public void APresetWithNoNameIsNotAPreset()
        {
            Presets presets = Presets.Of(new[] { new Preset("  ", null, null) }, null);

            Assert.Equal(0, presets.Count);
        }

        [Fact]
        public void APresetIsFoundByNameWhateverTheCaseAndTrimmed()
        {
            Presets presets = Presets.Of(null, new[] { SixSheets() });

            Assert.NotNull(presets.Named("  a STARTING point, from DM-11  "));
            Assert.True(presets.Holds("A Starting Point, From DM-11"));
            Assert.Null(presets.Named("something else"));
        }

        [Fact]
        public void SavingOverAShippedPresetKeepsItsPlaceAndSavingANewOneGoesLast()
        {
            Presets presets = Presets.Of(null, new[] { SixSheets(), new Preset("Second", null, null) });

            Presets after = presets
                .With(new Preset("A starting point, from DM-11", new[] { General }, null))
                .With(new Preset("Third", null, null));

            Assert.Equal(
                new[] { "A starting point, from DM-11", "Second", "Third" },
                after.All.Select(one => one.Name).ToArray());

            Assert.Equal(
                new[] { "A starting point, from DM-11", "Third" },
                after.TheirOwn.Select(one => one.Name).ToArray());
        }

        [Fact]
        public void OnlyTheUsersOwnPresetsAreWrittenBackToTheirFile()
        {
            Presets presets = Presets.Of(
                new[] { new Preset("Mine", null, null) }, new[] { SixSheets() });

            Assert.Equal(new[] { "Mine" }, presets.TheirOwn.Select(one => one.Name).ToArray());
        }

        [Fact]
        public void DeletingTheirCopyOfAShippedPresetPutsTheShippedOneBack()
        {
            var shipped = new[] { SixSheets() };
            Presets presets = Presets.Of(
                new[] { new Preset("A starting point, from DM-11", new[] { General }, null) },
                shipped);

            Presets after = presets.Without("A starting point, from DM-11", shipped);

            Assert.Equal(1, after.Count);
            Assert.Equal(PresetSource.ShippedDefaults, after.All[0].Source);
            Assert.Equal(6, after.All[0].Ticked.Count);
        }

        [Fact]
        public void WhatDeletingDoesSaysWhichOfTheThreeThingsWillHappen()
        {
            var shipped = new[] { SixSheets() };
            Presets presets = Presets.Of(
                new[]
                {
                    new Preset("A starting point, from DM-11", new[] { General }, null),
                    new Preset("Mine", null, null)
                },
                shipped);

            Assert.Equal(
                "Deleting A starting point, from DM-11 puts back the one that came with the "
                    + "tool, which has the same name.",
                presets.WhatDeletingDoes("A starting point, from DM-11", shipped));

            Assert.Equal(
                "Deleting Mine cannot be undone.",
                presets.WhatDeletingDoes("Mine", shipped));

            Presets shippedOnly = Presets.Of(null, shipped);
            Assert.Equal(
                "A starting point, from DM-11 came with the tool and cannot be deleted. Save "
                    + "your own under this name to replace it.",
                shippedOnly.WhatDeletingDoes("A starting point, from DM-11", shipped));
        }

        [Fact]
        public void DeletingAPresetNobodySavedChangesNothing()
        {
            Presets presets = Presets.Of(null, new[] { SixSheets() });

            Assert.Equal(1, presets.Without("something else", new[] { SixSheets() }).Count);
            Assert.Equal(string.Empty, presets.WhatDeletingDoes("something else", null));
        }

        /// <summary>
        /// The shipped file itself, read off the repo. It is the six sheets of the DM-11 run
        /// and the only preset a fresh install offers, so a change that breaks it fails here
        /// rather than on somebody's install.
        /// </summary>
        [Fact]
        public void TheShippedFileHoldsTheStartingPointFromTheDmElevenRun()
        {
            PresetFileContents read = PresetFile.Read(File.ReadAllText(ShippedFile("presets.txt")));

            Assert.Empty(read.NotRead);
            Preset only = Assert.Single(read.Presets);

            Assert.Equal("A starting point, from DM-11", only.Name);
            Assert.Equal(
                new[] { "(010) LIST OF DRAWINGS", "(010) Location Key Plan",
                    "(010) Overall Key Plan", "(200) General Arrangement Layout",
                    "(400) Landscape Cross Section", "(600) HARDSCAPE SCHEDULE" },
                only.Ticked.Select(one => one.ToString()).ToArray());

            Assert.Equal(5, only.Sheets.Count);
            Assert.Equal(6, only.Sheets.Sum(one => one.SheetsPerPlot));
            Assert.All(only.Sheets, one => Assert.Equal(1, one.ViewsPerSheet));

            // Every ticked type is on exactly one sheet, so the preset makes a sheet for each
            // and none of them twice.
            Assert.Equal(
                only.Ticked.OrderBy(one => one).ToArray(),
                only.Sheets.SelectMany(one => one.Views).OrderBy(one => one).ToArray());
        }

        /// <summary>
        /// The two shipped files are two records that have to agree. The preset puts a view
        /// type on a title block and title-blocks.txt pairs that same type with one, and a
        /// preset naming a block the pairings do not would make a sheet nobody asked for.
        /// </summary>
        [Fact]
        public void EveryViewOnAShippedSheetIsOnTheTitleBlockTheShippedPairingsGiveIt()
        {
            Preset only = PresetFile.Read(File.ReadAllText(ShippedFile("presets.txt")))
                .Presets[0];

            TitleBlockSettings pairings = TitleBlockSettings.Of(
                null,
                TitleBlockSettingsFile.Read(File.ReadAllText(ShippedFile("title-blocks.txt")))
                    .Pairings);

            foreach (PresetSheet sheet in only.Sheets)
            {
                foreach (ViewType view in sheet.Views)
                {
                    TitleBlockPairing paired = pairings.For(view);

                    Assert.True(paired != null, view + " is on no shipped pairing.");
                    Assert.Equal(paired.FamilyName, sheet.TitleBlockFamilyName);
                    Assert.Equal(paired.TypeName, sheet.TitleBlockTypeName);
                }
            }
        }

        /// <summary>
        /// Walks up from the test assembly, because the working folder a test runs in is not
        /// something to depend on.
        /// </summary>
        private static string ShippedFile(string name)
        {
            var at = new DirectoryInfo(AppContext.BaseDirectory);

            while (at != null)
            {
                string here = Path.Combine(at.FullName, "install", name);
                if (File.Exists(here)) return here;
                at = at.Parent;
            }

            throw new FileNotFoundException(
                "install/" + name + " was not found above " + AppContext.BaseDirectory
                + ". It is shipped beside the add-in and install.ps1 copies it there.");
        }
    }
}
