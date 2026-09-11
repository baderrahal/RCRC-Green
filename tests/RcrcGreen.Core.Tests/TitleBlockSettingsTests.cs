using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// Which title block goes with which view type, merged from the user's own file and the
    /// shipped defaults.
    /// </summary>
    public class TitleBlockSettingsTests
    {
        private static readonly ViewType Overall = new ViewType("010", "Overall Plan");

        private static readonly ViewType General =
            new ViewType("200", "General Arrangement Layout");

        private static readonly ViewType Softscape =
            new ViewType("600", "SOFTSCAPE SCHEDULE");

        private const string Family = "AR-PRX-Title_Block_A1";

        private static TitleBlockPairing Pairing(ViewType type, string typeName)
        {
            return new TitleBlockPairing(type, Family, typeName, TitleBlockSource.Unset);
        }

        [Fact]
        public void AShippedPairingIsUsedWhenTheUserHasNone()
        {
            TitleBlockSettings settings = TitleBlockSettings.Of(
                null, new[] { Pairing(Overall, "KEYPLAN") });

            Assert.Equal("AR-PRX-Title_Block_A1 KEYPLAN", settings.For(Overall).TitleBlock);
            Assert.Equal(TitleBlockSource.ShippedDefaults, settings.For(Overall).Source);
        }

        /// <summary>
        /// The user's file is read first and wins. The shipped file is a starting point and a
        /// choice somebody made beats it, whatever an install puts back.
        /// </summary>
        [Fact]
        public void TheUsersOwnPairingWinsOverTheShippedOne()
        {
            TitleBlockSettings settings = TitleBlockSettings.Of(
                new[] { Pairing(Overall, "GA-SCHEMATIC") },
                new[] { Pairing(Overall, "KEYPLAN") });

            Assert.Equal("AR-PRX-Title_Block_A1 GA-SCHEMATIC", settings.For(Overall).TitleBlock);
            Assert.Equal(TitleBlockSource.UserFile, settings.For(Overall).Source);
        }

        /// <summary>
        /// A view type neither file names has no pairing at all. Null is the answer, so the
        /// panel shows an empty box and asks, rather than filling in something nobody chose.
        /// </summary>
        [Fact]
        public void AViewTypeNeitherFileNamesHasNoPairing()
        {
            TitleBlockSettings settings = TitleBlockSettings.Of(
                null, new[] { Pairing(Overall, "KEYPLAN") });

            Assert.Null(settings.For(General));
            Assert.Null(settings.For(null));
            Assert.Null(TitleBlockSettings.Nothing.For(Overall));
        }

        /// <summary>
        /// Changing one marks it theirs, and only theirs is written back. Writing the shipped
        /// ones into the user's file as well would freeze this version's defaults into it and
        /// the next install could never move them.
        /// </summary>
        [Fact]
        public void OnlyWhatTheUserSetIsWrittenBack()
        {
            TitleBlockSettings settings = TitleBlockSettings
                .Of(null, new[] { Pairing(Overall, "KEYPLAN"), Pairing(General, "GA-SCHEMATIC") })
                .With(General, Family, "SECTION / ENLARGEMENTS");

            Assert.Equal(2, settings.All.Count);
            Assert.Single(settings.TheirOwn);
            Assert.Equal(General, settings.TheirOwn[0].Type);
            Assert.Equal("AR-PRX-Title_Block_A1 SECTION / ENLARGEMENTS",
                settings.For(General).TitleBlock);
            Assert.Equal("AR-PRX-Title_Block_A1 KEYPLAN", settings.For(Overall).TitleBlock);
        }

        /// <summary>
        /// Setting the same one the shipped file already names still marks it theirs, because
        /// they chose it and a later change to the defaults should not move it under them.
        /// </summary>
        [Fact]
        public void ChoosingWhatTheDefaultAlreadySaysStillCountsAsTheirs()
        {
            TitleBlockSettings settings = TitleBlockSettings
                .Of(null, new[] { Pairing(Overall, "KEYPLAN") })
                .With(Overall, Family, "KEYPLAN");

            Assert.Equal(TitleBlockSource.UserFile, settings.For(Overall).Source);
            Assert.Single(settings.TheirOwn);
        }

        [Fact]
        public void APairingCanBeTakenOutAgain()
        {
            TitleBlockSettings settings = TitleBlockSettings
                .Of(new[] { Pairing(Overall, "KEYPLAN") }, null)
                .Without(Overall);

            Assert.Null(settings.For(Overall));
            Assert.Empty(settings.TheirOwn);
        }

        /// <summary>
        /// Nothing is changed in place. The settings are held by the panel across a redraw, and
        /// a half changed copy is the shape the grid columns were built to avoid.
        /// </summary>
        [Fact]
        public void SettingOneLeavesTheOldSettingsAlone()
        {
            TitleBlockSettings before = TitleBlockSettings.Of(
                null, new[] { Pairing(Overall, "KEYPLAN") });

            TitleBlockSettings after = before.With(Overall, Family, "LOD");

            Assert.Equal("AR-PRX-Title_Block_A1 KEYPLAN", before.For(Overall).TitleBlock);
            Assert.Equal("AR-PRX-Title_Block_A1 LOD", after.For(Overall).TitleBlock);
        }

        [Fact]
        public void TheLineSaysWhichFileThePairingCameFrom()
        {
            TitleBlockSettings settings = TitleBlockSettings.Of(
                new[] { Pairing(General, "GA-SCHEMATIC") },
                new[] { Pairing(Overall, "KEYPLAN") });

            Assert.Equal(
                "AR-PRX-Title_Block_A1 KEYPLAN, from the shipped defaults.",
                settings.WhereItCameFrom(new[] { Overall }));

            Assert.Equal(
                "AR-PRX-Title_Block_A1 GA-SCHEMATIC, from your own settings.",
                settings.WhereItCameFrom(new[] { General }));
        }

        [Fact]
        public void WithNothingTickedAndNothingKnownTheLineSaysSo()
        {
            TitleBlockSettings settings = TitleBlockSettings.Of(
                null, new[] { Pairing(Overall, "KEYPLAN") });

            Assert.Equal("No view is ticked for this sheet yet.",
                settings.WhereItCameFrom(new ViewType[0]));

            Assert.Equal(
                "The settings hold no title block for (200) General Arrangement Layout. Pick one "
                + "and it is remembered for next time.",
                settings.WhereItCameFrom(new[] { General }));
        }

        /// <summary>
        /// A sheet holds one title block and its views can want different ones. Both are named
        /// and neither wins, which is the rule everywhere else here that two sources answer one
        /// question.
        /// </summary>
        [Fact]
        public void ViewsThatDisagreeAreBothNamedAndNeitherWins()
        {
            TitleBlockSettings settings = TitleBlockSettings.Of(
                null,
                new[] { Pairing(Overall, "KEYPLAN"), Pairing(General, "GA-SCHEMATIC") });

            string said = settings.WhereItCameFrom(new[] { Overall, General });

            Assert.StartsWith("These views disagree about the title block:", said);
            Assert.Contains("(010) Overall Plan wants AR-PRX-Title_Block_A1 KEYPLAN", said);
            Assert.Contains("(200) General Arrangement Layout wants AR-PRX-Title_Block_A1 GA-SCHEMATIC", said);
        }

        /// <summary>
        /// Views that agree, with one of them in neither file, take the agreed one and the odd
        /// one out is named rather than passed over.
        /// </summary>
        [Fact]
        public void AViewWithNoPairingBesideTwoThatAgreeIsNamed()
        {
            TitleBlockSettings settings = TitleBlockSettings.Of(
                null,
                new[] { Pairing(Overall, "KEYPLAN"), Pairing(General, "KEYPLAN") });

            string said = settings.WhereItCameFrom(new[] { Overall, General, Softscape });

            Assert.StartsWith("AR-PRX-Title_Block_A1 KEYPLAN, from the shipped defaults.", said);
            Assert.Contains("(600) SOFTSCAPE SCHEDULE is in neither file", said);
        }

        /// <summary>
        /// The settings are shared across projects, so a model without the named block is
        /// ordinary. It is said rather than treated as a fault.
        /// </summary>
        [Fact]
        public void ATitleBlockThisModelDoesNotHoldIsSaidRatherThanRefused()
        {
            Assert.Equal(
                "This model holds no title block called AR-PRX-Title_Block_A1 KEYPLAN, which is "
                + "what the settings name for (010) Overall Plan. Pick one this model has.",
                TitleBlockSettings.WhyUnset(Pairing(Overall, "KEYPLAN")));

            Assert.Equal(string.Empty, TitleBlockSettings.WhyUnset(null));
        }
    }

    /// <summary>
    /// The file itself. Tab separated, four fields, and a line that cannot be read is kept.
    /// </summary>
    public class TitleBlockSettingsFileTests
    {
        /// <summary>
        /// The double space in the middle of this type name is what the model really holds, so
        /// it has to survive a round trip through the file exactly.
        /// </summary>
        private const string TwoSpaces = "LOD /  HARDSCAPE SCHEDULES";

        [Fact]
        public void AFourFieldLineReadsAsAPairing()
        {
            TitleBlockFileContents read = TitleBlockSettingsFile.Read(
                "010\tOverall Plan\tAR-PRX-Title_Block_A1\tKEYPLAN");

            Assert.Single(read.Pairings);
            Assert.Equal("010", read.Pairings[0].Type.Code);
            Assert.Equal("Overall Plan", read.Pairings[0].Type.ViewName);
            Assert.Equal("AR-PRX-Title_Block_A1", read.Pairings[0].FamilyName);
            Assert.Equal("KEYPLAN", read.Pairings[0].TypeName);
            Assert.Empty(read.NotRead);
        }

        [Fact]
        public void TwoSpacesInATypeNameSurviveARoundTrip()
        {
            string written = TitleBlockSettingsFile.Write(new[]
            {
                new TitleBlockPairing(
                    new ViewType("600", "HARDSCAPE SCHEDULE"),
                    "AR-PRX-Title_Block_A1",
                    TwoSpaces,
                    TitleBlockSource.UserFile)
            });

            TitleBlockFileContents back = TitleBlockSettingsFile.Read(written);

            Assert.Single(back.Pairings);
            Assert.Equal(TwoSpaces, back.Pairings[0].TypeName);
            Assert.Contains("  ", back.Pairings[0].TypeName);
        }

        /// <summary>
        /// A view name with a leading or trailing space is a name the model holds that way, so
        /// nothing but the code is trimmed. A name tidied up here is a name that matches nothing.
        /// </summary>
        [Fact]
        public void OnlyTheCodeIsTrimmed()
        {
            TitleBlockFileContents read = TitleBlockSettingsFile.Read(
                " 600 \tSHRUBS & LAWN SCHEDULE\tAR-PRX-Title_Block_A1\tLOD / SCHEDULES ");

            Assert.Equal("600", read.Pairings[0].Type.Code);
            Assert.Equal("LOD / SCHEDULES ", read.Pairings[0].TypeName);
        }

        [Fact]
        public void BlankLinesAndNotesArePassedOverAndNotCounted()
        {
            TitleBlockFileContents read = TitleBlockSettingsFile.Read(
                "# a note\r\n\r\n010\tOverall Plan\tFAMILY\tTYPE\r\n   \r\n# another\r\n");

            Assert.Single(read.Pairings);
            Assert.Empty(read.NotRead);
        }

        /// <summary>
        /// A line that is not four fields is kept with its number. A settings file one line
        /// short reads exactly like one that never had the line.
        /// </summary>
        [Fact]
        public void ALineThatCannotBeReadIsKeptWithItsNumber()
        {
            TitleBlockFileContents read = TitleBlockSettingsFile.Read(
                "010\tOverall Plan\tFAMILY\tTYPE\r\n600 SOFTSCAPE SCHEDULE FAMILY TYPE\r\n"
                + "\tno code\tFAMILY\tTYPE");

            Assert.Single(read.Pairings);
            Assert.Equal(2, read.NotRead.Count);
            Assert.Contains("line 2", read.NotRead[0]);
            Assert.Contains("line 3", read.NotRead[1]);

            Assert.Equal(
                "1 pairings read and 2 lines could not be: line 2, 600 SOFTSCAPE SCHEDULE FAMILY "
                + "TYPE; line 3, \tno code\tFAMILY\tTYPE",
                read.InWords());
        }

        [Fact]
        public void NoFileAtAllReadsAsNothingRatherThanThrowing()
        {
            Assert.Empty(TitleBlockSettingsFile.Read(null).Pairings);
            Assert.Empty(TitleBlockSettingsFile.Read(string.Empty).Pairings);
            Assert.Empty(TitleBlockSettingsFile.Read(null).NotRead);
        }

        /// <summary>
        /// The shipped file itself, read off the repo. It is the only record of the eleven the
        /// team picked by hand on 2026-09-11, so a change that breaks it fails here rather than
        /// on somebody's install.
        /// </summary>
        [Fact]
        public void TheShippedDefaultsHoldTheElevenTheTeamPicked()
        {
            TitleBlockFileContents read = TitleBlockSettingsFile.Read(
                File.ReadAllText(ShippedFile()));

            Assert.Empty(read.NotRead);
            Assert.Equal(11, read.Pairings.Count);

            Assert.All(read.Pairings,
                one => Assert.Equal("AR-PRX-Title_Block_A1", one.FamilyName));

            // Ordinal, and the double space is why: a space sorts before a letter, so
            // LOD /  HARDSCAPE SCHEDULES comes before LOD / SCHEDULES. Written out the other
            // way round first and this test said so.
            Assert.Equal(
                new[] { "GA-SCHEMATIC", "KEYPLAN", "LOD", TwoSpaces, "LOD / SCHEDULES",
                    "SECTION / ENLARGEMENTS" },
                read.Pairings.Select(one => one.TypeName)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(one => one, StringComparer.Ordinal)
                    .ToArray());

            Assert.Equal(2, read.Pairings.Count(one => one.TypeName == TwoSpaces));
            Assert.Equal(3, read.Pairings.Count(one => one.TypeName == "LOD / SCHEDULES"));
            Assert.Equal(4, read.Pairings.Count(one => one.Type.Code == "010"));
        }

        /// <summary>
        /// Walks up from the test assembly, because the working folder a test runs in is not
        /// something to depend on.
        /// </summary>
        private static string ShippedFile()
        {
            var at = new DirectoryInfo(AppContext.BaseDirectory);

            while (at != null)
            {
                string here = Path.Combine(at.FullName, "install", "title-blocks.txt");
                if (File.Exists(here)) return here;
                at = at.Parent;
            }

            throw new FileNotFoundException(
                "install/title-blocks.txt was not found above " + AppContext.BaseDirectory
                + ". It is the shipped defaults and install.ps1 copies it beside the add-in.");
        }
    }
}
