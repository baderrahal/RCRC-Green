using System;
using System.IO;
using System.Linq;
using System.Reflection;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// The sheet name table. Every expected name below is written out by hand from the two
    /// measured models, where four of DM-11's eight sheet names disprove the old rule that a
    /// sheet is its view name upper cased.
    /// </summary>
    public class SheetNameSettingsTests
    {
        private static readonly ViewType LocationKeyPlan = new ViewType("010", "Location Key Plan");

        private static readonly ViewType OverallKeyPlan = new ViewType("010", "Overall Key Plan");

        private static SheetNameSettings TheShippedShape()
        {
            return SheetNameSettings.Of(
                null,
                new[]
                {
                    new SheetNamePairing(
                        LocationKeyPlan, "PROJECT LOCATION KEY PLAN", SheetNameSource.Unset),
                    new SheetNamePairing(
                        OverallKeyPlan, "OVERALL KEYPLAN", SheetNameSource.Unset)
                });
        }

        /// <summary>
        /// The pairing wins, and a type neither file holds falls back to the derivation that
        /// wrote LOCATION KEY PLAN onto a sheet the team calls PROJECT LOCATION KEY PLAN.
        /// </summary>
        [Fact]
        public void ThePairingNamesTheSheetAndTheDerivationIsOnlyTheFallback()
        {
            SheetNameSettings settings = TheShippedShape();

            Assert.Equal("PROJECT LOCATION KEY PLAN", settings.NameFor(LocationKeyPlan));
            Assert.Equal("OVERALL KEYPLAN", settings.NameFor(OverallKeyPlan));

            var unpaired = new ViewType("200", "General Arrangement Layout");
            Assert.Null(settings.For(unpaired));
            Assert.Equal("GENERAL ARRANGEMENT LAYOUT", settings.NameFor(unpaired));
        }

        [Fact]
        public void TheUsersOwnPairingWinsOverTheShippedOne()
        {
            SheetNameSettings settings = SheetNameSettings.Of(
                new[]
                {
                    new SheetNamePairing(LocationKeyPlan, "SITE KEY PLAN", SheetNameSource.Unset)
                },
                new[]
                {
                    new SheetNamePairing(
                        LocationKeyPlan, "PROJECT LOCATION KEY PLAN", SheetNameSource.Unset)
                });

            Assert.Equal("SITE KEY PLAN", settings.NameFor(LocationKeyPlan));
            Assert.Equal(SheetNameSource.UserFile, settings.For(LocationKeyPlan).Source);

            SheetNamePairing theirs = Assert.Single(settings.TheirOwn);
            Assert.Equal("SITE KEY PLAN", theirs.SheetName);
        }

        [Fact]
        public void TheWordsSayRememberedOrDerivedAndWhereFrom()
        {
            SheetNameSettings settings = TheShippedShape();

            Assert.Equal(
                "Remembered from the shipped sheet names.",
                settings.NamedInWords(LocationKeyPlan));

            Assert.Equal(
                "Remembered from your own sheet names.",
                settings.With(LocationKeyPlan, "SITE KEY PLAN").NamedInWords(LocationKeyPlan));

            Assert.Equal(
                "Derived by upper casing the view name, because neither sheet name file holds "
                + "(200) General Arrangement Layout. Type over it and it is remembered.",
                settings.NamedInWords(new ViewType("200", "General Arrangement Layout")));
        }

        [Fact]
        public void SettingANameMarksItTheirsAndAnEmptyOneClearsIt()
        {
            SheetNameSettings held = SheetNameSettings.Nothing
                .With(LocationKeyPlan, "  PROJECT LOCATION KEY PLAN  ");

            Assert.Equal("PROJECT LOCATION KEY PLAN", held.NameFor(LocationKeyPlan));
            Assert.Equal(SheetNameSource.UserFile, held.For(LocationKeyPlan).Source);

            Assert.Null(held.With(LocationKeyPlan, "   ").For(LocationKeyPlan));
            Assert.Null(held.Without(LocationKeyPlan).For(LocationKeyPlan));
        }

        /// <summary>
        /// The one record rule at work: the division resolves through the table, so the
        /// letter order and the row names agree with SheetOrder. Ticked out of order, the
        /// LIST OF DRAWINGS sheet comes first because the table gives the list's own words.
        /// Derived names would read LOCATION KEY PLAN, which the list does not hold, and the
        /// ticked order would stand.
        /// </summary>
        [Fact]
        public void TheResolvedNameIsWhatTheDivisionOrdersBy()
        {
            SheetNameSettings settings = SheetNameSettings.Of(
                null,
                new[]
                {
                    new SheetNamePairing(
                        LocationKeyPlan, "PROJECT LOCATION KEY PLAN", SheetNameSource.Unset),
                    new SheetNamePairing(
                        new ViewType("010", "LIST OF DRAWINGS"), "LIST OF DRAWINGS",
                        SheetNameSource.Unset)
                });

            var planned = SheetDivision.Of(new[]
            {
                LocationKeyPlan,
                new ViewType("010", "LIST OF DRAWINGS")
            }, 1, settings);

            Assert.Equal(
                new[] { "LIST OF DRAWINGS", "PROJECT LOCATION KEY PLAN" },
                planned.Select(one => one.ProposedName).ToArray());

            Assert.Equal(
                "Remembered from the shipped sheet names.",
                planned[0].NamedInWords());
        }
    }

    /// <summary>
    /// The sheet name file: tab separated, three fields, only the code trimmed, unreadable
    /// lines kept and said.
    /// </summary>
    public class SheetNameFileTests
    {
        [Fact]
        public void WhatIsWrittenReadsBack()
        {
            string text = SheetNameFile.Write(new[]
            {
                new SheetNamePairing(
                    new ViewType("010", "Location Key Plan"), "PROJECT LOCATION KEY PLAN",
                    SheetNameSource.UserFile)
            });

            SheetNameFileContents held = SheetNameFile.Read(text);

            Assert.Empty(held.NotRead);
            SheetNamePairing only = Assert.Single(held.Pairings);
            Assert.Equal("010", only.Type.Code);
            Assert.Equal("Location Key Plan", only.Type.ViewName);
            Assert.Equal("PROJECT LOCATION KEY PLAN", only.SheetName);
        }

        [Fact]
        public void ALineShortOfItsFieldsIsKeptWithItsNumber()
        {
            SheetNameFileContents held = SheetNameFile.Read(
                "# a note\r\n010\tLocation Key Plan\tPROJECT LOCATION KEY PLAN\r\n010\tOverall Plan\r\n");

            Assert.Single(held.Pairings);
            string bad = Assert.Single(held.NotRead);
            Assert.Equal("line 3, 010\tOverall Plan", bad);
        }

        [Fact]
        public void NothingReadsAsNothing()
        {
            Assert.Empty(SheetNameFile.Read(string.Empty).Pairings);
            Assert.Empty(SheetNameFile.Read(null).NotRead);
        }

        /// <summary>
        /// The shipped file itself, read off the repo, so a broken line fails here rather
        /// than on somebody's install. Seven of the nine names are the team's order list's
        /// own words. OVERALL PLAN and FURNITURE SCHEDULES are not on that list, they rest
        /// on the brief that supplied the table, and the two scan reports were not available
        /// where this was written, so checking the nine against them is still open.
        /// </summary>
        [Fact]
        public void TheShippedSheetNamesHoldTheNine()
        {
            SheetNameFileContents read = SheetNameFile.Read(File.ReadAllText(ShippedFile()));

            Assert.Empty(read.NotRead);
            Assert.Equal(9, read.Pairings.Count);

            Assert.Equal(
                "PROJECT LOCATION KEY PLAN",
                read.Pairings.Single(one => one.Type.ViewName == "Location Key Plan").SheetName);
            Assert.Equal(
                "OVERALL KEYPLAN",
                read.Pairings.Single(one => one.Type.ViewName == "Overall Key Plan").SheetName);
            Assert.Equal(
                "HARDSCAPE SCHEDULES",
                read.Pairings.Single(one => one.Type.ViewName == "HARDSCAPE SCHEDULE").SheetName);
            Assert.Equal(
                "SOFTSCAPE SCHEDULES",
                read.Pairings.Single(one => one.Type.ViewName == "SOFTSCAPE SCHEDULE").SheetName);

            int unlisted = SheetOrder.TheTeamsOrder.Count;
            Assert.Equal(
                new[] { "FURNITURE SCHEDULES", "OVERALL PLAN" },
                read.Pairings
                    .Where(one => SheetOrder.Position(one.SheetName) == unlisted)
                    .Select(one => one.SheetName)
                    .OrderBy(one => one, StringComparer.Ordinal)
                    .ToArray());
        }

        private static string ShippedFile()
        {
            string at = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            while (at != null)
            {
                string candidate = Path.Combine(Path.Combine(at, "install"), "sheet-names.txt");
                if (File.Exists(candidate)) return candidate;
                at = Path.GetDirectoryName(at);
            }

            throw new FileNotFoundException("install/sheet-names.txt was not found above the "
                + "test assembly, and the shipped defaults cannot be checked without it.");
        }
    }
}
