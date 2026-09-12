using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    public class PlotListTests
    {
        [Fact]
        public void ThePlotListIsTheUnionOfBothSourcesInNaturalOrder()
        {
            PlotsInTheModel plots = PlotsInTheModel.Of(
                new[] { "DM-100", "DM-2", "FM-05" },
                new[] { "DM-2", "DM-100", "FM-05" });

            Assert.Equal(new[] { "DM-2", "DM-100", "FM-05" }, plots.All);
            Assert.True(plots.Agree);
        }

        /// <summary>
        /// PRX_Plot_ID on the sheets and the PRX_Ref Plot ID filter on the schedules are two
        /// records of one fact. Both lists are kept and the disagreement is shown, because
        /// picking a winner here is the shape that has been the fault six times in this repo.
        /// </summary>
        [Fact]
        public void APlotNamedByOnlyOneSourceIsKeptAndNamedAsSuch()
        {
            PlotsInTheModel plots = PlotsInTheModel.Of(
                new[] { "DM-11", "DM-12" },
                new[] { "DM-11", "NS-06" });

            Assert.Equal(new[] { "DM-11", "DM-12", "NS-06" }, plots.All);
            Assert.False(plots.Agree);
            Assert.Equal(new[] { "DM-12" }, plots.OnSheetsOnly);
            Assert.Equal(new[] { "NS-06" }, plots.OnSchedulesOnly);
        }

        [Fact]
        public void APlotTheModelDoesNotHoldCannotBeTicked()
        {
            var ticks = new PlotTicks(PlotsInTheModel.Of(new[] { "DM-11" }, null));

            Assert.Equal(0, ticks.With("PF-99").Count);
            Assert.Equal(1, ticks.With("DM-11").Count);
        }

        [Fact]
        public void SelectAllAndClearAreTheThreeModesBetweenThem()
        {
            var ticks = new PlotTicks(PlotsInTheModel.Of(new[] { "DM-11", "DM-12", "DM-13" }, null));

            Assert.Equal(1, ticks.With("DM-12").Count);
            Assert.Equal(2, ticks.With("DM-12").With("DM-13").Count);
            Assert.Equal(3, ticks.All().Count);
            Assert.Equal(0, ticks.All().None().Count);
        }

        [Fact]
        public void TheTickedCountIsSaidInWordsAtAllTimes()
        {
            var ticks = new PlotTicks(PlotsInTheModel.Of(new[] { "DM-11", "DM-12" }, null));

            Assert.Equal("0 of 2 plots ticked.", ticks.InWords);
            Assert.Equal("1 of 2 plots ticked.", ticks.With("DM-11").InWords);
            Assert.Equal("No plot in this model.", new PlotTicks(PlotsInTheModel.Of(null, null)).InWords);
        }

        [Fact]
        public void TickingIsToggledAndTheOrderIsAlwaysTheModelOrder()
        {
            var ticks = new PlotTicks(PlotsInTheModel.Of(new[] { "DM-2", "DM-100" }, null));

            PlotTicks both = ticks.Toggled("DM-100").Toggled("DM-2");
            Assert.Equal(new[] { "DM-2", "DM-100" }, both.Ticked);
            Assert.Equal(1, both.Toggled("DM-2").Count);
        }
    }

    public class ComponentTemplatesTests
    {
        /// <summary>
        /// The eleven measured on the 1548 scan, each written out by hand rather than worked out
        /// with the table's own rule. Two mean MOSQUES and four mean STREETS.
        /// </summary>
        [Theory]
        [InlineData("DAILY MOSQUE", "MOSQUES")]
        [InlineData("FRIDAY MOSQUE", "MOSQUES")]
        [InlineData("SCHOOL", "SCHOOLS")]
        [InlineData("HEALTH", "HEALTHCARE")]
        [InlineData("PARKING LOT", "PARKING")]
        [InlineData("EXISTING PARK", "EXISTING PARKS")]
        [InlineData("FUTURE PARK", "FUTURE PARKS")]
        [InlineData("NH STRT LESS 20m ROW", "STREETS")]
        [InlineData("NH STRT 20m ROW", "STREETS")]
        [InlineData("STREET 30m ROW", "STREETS")]
        [InlineData("STREET 36m ROW", "STREETS")]
        public void EveryMeasuredComponentValueResolves(string component, string templateName)
        {
            KpiTemplate found = ComponentTemplates.For(component);

            Assert.NotNull(found);
            Assert.Equal(templateName, found.Name);
        }

        [Fact]
        public void TheTableHoldsTheElevenAndNothingElse()
        {
            Assert.Equal(11, ComponentTemplates.All.Count);
            Assert.Equal(11, ComponentTemplates.All.Select(one => one.Component).Distinct().Count());
            Assert.All(ComponentTemplates.All,
                one => Assert.Contains(one.Template, KpiTemplates.All));
        }

        /// <summary>
        /// Many to one, said as counts rather than left to be read off the list.
        /// </summary>
        [Fact]
        public void TwoValuesMeanMosquesAndFourMeanStreets()
        {
            Assert.Equal(
                new[] { "DAILY MOSQUE", "FRIDAY MOSQUE" },
                ComponentTemplates.ValuesFor(KpiTemplates.Mosques));
            Assert.Equal(
                new[] { "NH STRT LESS 20m ROW", "NH STRT 20m ROW", "STREET 30m ROW", "STREET 36m ROW" },
                ComponentTemplates.ValuesFor(KpiTemplates.Streets));
        }

        /// <summary>
        /// A template no value reaches could never be preselected, so the map would have a hole
        /// nothing on screen would show.
        /// </summary>
        [Fact]
        public void EveryTemplateIsReachedByAtLeastOneValue()
        {
            Assert.All(KpiTemplates.All, one => Assert.NotEmpty(ComponentTemplates.ValuesFor(one)));
        }

        /// <summary>
        /// The whole value is compared and no part of one matches. PARK is not a value, and
        /// finding it inside PARKING LOT is the string rule this table replaced.
        /// </summary>
        [Fact]
        public void NoPartOfAValueMatches()
        {
            Assert.Null(ComponentTemplates.For("PARK"));
            Assert.Null(ComponentTemplates.For("PARKING"));
            Assert.Null(ComponentTemplates.For("STREET"));
            Assert.Null(ComponentTemplates.For("MOSQUE"));
            Assert.Null(ComponentTemplates.For("NH STRT 20m ROW AND MORE"));
        }

        [Fact]
        public void TheCaseAndTheSurroundingSpaceComeOffAndNothingElseDoes()
        {
            Assert.Equal("HEALTHCARE", ComponentTemplates.For("  health  ").Name);
            Assert.Equal("STREETS", ComponentTemplates.For("street 36M row").Name);
            Assert.Null(ComponentTemplates.For("STREET36m ROW"));
            Assert.Null(ComponentTemplates.For(null));
            Assert.Null(ComponentTemplates.For("   "));
        }
    }

    public class TemplateForComponentTests
    {
        private static AgreedValue Holding(params string[] components)
        {
            return new AgreedValue(components.Select((one, at) => new PlotText("P-" + at, one)));
        }

        private static AgreedValue OnPlots(string component, params string[] plotIds)
        {
            return new AgreedValue(plotIds.Select(plotId => new PlotText(plotId, component)));
        }

        [Fact]
        public void FridayMosquePreselectsMosques()
        {
            TemplateChoice choice = TemplateForComponent.For(Holding("FRIDAY MOSQUE"), null, null);

            Assert.False(choice.NeedsAPick);
            Assert.Equal("MOSQUES", choice.Preselected.Name);
            Assert.Equal(
                "FRIDAY MOSQUE on the chosen plots preselects MOSQUES. Change it if it is wrong.",
                choice.Why);
        }

        [Fact]
        public void SchoolPreselectsSchools()
        {
            Assert.Equal("SCHOOLS", TemplateForComponent.For(Holding("SCHOOL"), null, null).Preselected.Name);
        }

        /// <summary>
        /// The park tie is broken by the model, not by the tool and not by the user. EXISTING
        /// PARK and FUTURE PARK are separate values on the sheets, so each preselects its own
        /// template. PARKING LOT is a third value and never stands beside them.
        /// </summary>
        [Fact]
        public void TheTwoParkValuesEachPreselectTheirOwnTemplate()
        {
            Assert.Equal("EXISTING PARKS", TemplateForComponent.For(Holding("EXISTING PARK"), null, null).Preselected.Name);
            Assert.Equal("FUTURE PARKS", TemplateForComponent.For(Holding("FUTURE PARK"), null, null).Preselected.Name);
            Assert.Equal("PARKING", TemplateForComponent.For(Holding("PARKING LOT"), null, null).Preselected.Name);
        }

        /// <summary>
        /// The plot is not read. STREET 36m ROW covers MM and ST plots, and NS carries two
        /// different street widths, so a rule on the prefix would answer three of them wrongly.
        /// </summary>
        [Fact]
        public void ThePlotPrefixChangesNothing()
        {
            Assert.Equal("STREETS",
                TemplateForComponent.For(OnPlots("STREET 36m ROW", "MM-03", "ST-11"), null, null).Preselected.Name);
            Assert.Equal("STREETS",
                TemplateForComponent.For(OnPlots("NH STRT 20m ROW", "NS-06"), null, null).Preselected.Name);
            Assert.Equal("STREETS",
                TemplateForComponent.For(OnPlots("NH STRT LESS 20m ROW", "NS-19"), null, null).Preselected.Name);
        }

        [Fact]
        public void PlotsDisagreeingOnTheComponentPutThePickToTheUser()
        {
            TemplateChoice choice = TemplateForComponent.For(Holding("FRIDAY MOSQUE", "SCHOOL"), null, null);

            Assert.True(choice.NeedsAPick);
            Assert.Contains("FRIDAY MOSQUE, SCHOOL", choice.Why);
            Assert.Equal(7, choice.Candidates.Count);
        }

        /// <summary>
        /// A value the table does not hold preselects nothing, names itself, and offers all
        /// seven. Nothing guesses and nothing falls back to matching on a name.
        /// </summary>
        [Fact]
        public void AComponentNotInTheTablePutsThePickToTheUserAndNamesIt()
        {
            TemplateChoice choice = TemplateForComponent.For(Holding("PUMP STATION"), null, null);

            Assert.True(choice.NeedsAPick);
            Assert.Equal(7, choice.Candidates.Count);
            Assert.Equal(
                "PUMP STATION is not one of the component values this tool knows. Pick the "
                + "template. The eleven it knows are in ComponentTemplates, measured off the 1548 scan.",
                choice.Why);
        }

        /// <summary>
        /// PARK on its own was the value that used to offer three templates, because PARKING
        /// begins with PARK. It is not a component value, so it now preselects nothing.
        /// </summary>
        [Fact]
        public void ParkOnItsOwnIsNotAComponentValue()
        {
            TemplateChoice choice = TemplateForComponent.For(Holding("PARK"), null, null);

            Assert.True(choice.NeedsAPick);
            Assert.Contains("PARK is not one of the component values", choice.Why);
        }

        [Fact]
        public void NoComponentAtAllPutsThePickToTheUser()
        {
            TemplateChoice choice = TemplateForComponent.For(Holding(string.Empty), null, null);

            Assert.True(choice.NeedsAPick);
            Assert.Equal(TemplateForComponent.NoComponent, choice.Why);
        }

        /// <summary>
        /// A caller offering a shorter list gets the pick back rather than a preselection it
        /// does not show. Nothing on screen would say why the highlighted one was not there.
        /// </summary>
        [Fact]
        public void ATemplateTheCallerDoesNotOfferIsNotPreselected()
        {
            TemplateChoice choice = TemplateForComponent.For(
                Holding("SCHOOL"), null, new[] { KpiTemplates.Mosques, KpiTemplates.Streets });

            Assert.True(choice.NeedsAPick);
            Assert.Contains("SCHOOL means SCHOOLS, which is not among the templates offered", choice.Why);
        }
    }

    public class KpiCreatePlanTests
    {
        private static SpeciesMatch Matched(string name, string group, int quantity, int row)
        {
            var species = new MergedSpecies(name, group, new[] { new PlotNumber("DM-12", quantity) });
            return new SpeciesMatch(species, KpiTemplates.ProposedTreesSheet, row, name, string.Empty);
        }

        [Fact]
        public void EveryMappedCellWithAValueBecomesOneWrite()
        {
            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.ExistingParks,
                new AgreedValue(new[] { new PlotText("DM-11", "PARK") }),
                new AgreedValue(new[] { new PlotText("DM-11", "ANH-007") }),
                "KING FAHD",
                Totalled.Adding(new[] { new PlotNumber("DM-11", 1000.0) }),
                Totalled.Adding(new[] { new PlotNumber("DM-11", 70.0) }),
                Totalled.Adding(new[] { new PlotNumber("DM-11", 35.0) }),
                null,
                "2026-09-09", "B RAHAL", "BIM COORDINATOR");

            Assert.Equal(9, plan.Writes.Count);
            Assert.Empty(plan.Skipped);
            Assert.Equal(new[] { "E5", "G5", "H5", "D3", "C5", "E4", "D8", "F11", "H11" },
                plan.Writes.Select(one => one.Cell.ToString()));
            Assert.All(plan.Writes, one => Assert.Equal("<Park Name>", one.SheetName));
        }

        /// <summary>
        /// What the team types on the pane reaches the cells. E5, G5 and H5 came out of the
        /// first real workbook holding the template's own &lt;Date&gt;, &lt;Name&gt; and
        /// &lt;Position&gt;, with the report saying nobody typed them, on a run where all three
        /// boxes were filled in: the pane collected them and handed none of the three on.
        ///
        /// **The three are required arguments now**, so a caller that forgets them does not
        /// compile. That is what a default of null was hiding.
        /// </summary>
        [Fact]
        public void WhatIsTypedOnThePaneReachesTheWrite()
        {
            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.Mosques, null, null, string.Empty, null, null, null, null,
                "2026-09-09", "xx", "bb");

            Assert.Equal("2026-09-09", Written(plan, "E5"));
            Assert.Equal("xx", Written(plan, "G5"));
            Assert.Equal("bb", Written(plan, "H5"));
            Assert.DoesNotContain(plan.Skipped, one => one.Cell == "E5");
            Assert.DoesNotContain(plan.Skipped, one => one.Cell == "G5");
            Assert.DoesNotContain(plan.Skipped, one => one.Cell == "H5");
        }

        /// <summary>
        /// A box left empty is still recorded with its reason, so a workbook short of the date
        /// says so rather than reading as one nobody looked at.
        /// </summary>
        [Fact]
        public void ABoxLeftEmptyIsRecordedWithItsReason()
        {
            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.Mosques, null, null, string.Empty, null, null, null, null,
                "2026-09-09", "   ", null);

            Assert.Equal("2026-09-09", Written(plan, "E5"));
            Assert.Equal(KpiCreatePlan.TypedByTheTeam, plan.Skipped.Single(one => one.Cell == "G5").Why);
            Assert.Equal(KpiCreatePlan.TypedByTheTeam, plan.Skipped.Single(one => one.Cell == "H5").Why);
        }

        /// <summary>
        /// Surrounding space comes off and nothing else does, the same plainness everything
        /// else here keeps.
        /// </summary>
        [Fact]
        public void TheSurroundingSpaceComesOffAndNothingElseDoes()
        {
            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.Mosques, null, null, string.Empty, null, null, null, null,
                "  2026-09-09  ", "B RAHAL", "BIM COORDINATOR");

            Assert.Equal("2026-09-09", Written(plan, "E5"));
            Assert.Equal("BIM COORDINATOR", Written(plan, "H5"));
        }

        /// <summary>
        /// An added species is the botanical name into column D, the count into column B, and
        /// the height and the diameter into the columns the sheet's header row names. A match
        /// built with no list behind it has no such columns, so those two are named under the
        /// cells not written rather than written by a letter somebody assumed. Family, genus,
        /// native and every code column are the client's data and the tool does not know them.
        /// </summary>
        [Fact]
        public void AnAddedSpeciesWithNoListColumnsWritesItsNameAndItsCountAndNamesTheOtherTwo()
        {
            var species = new MergedSpecies("PHOENIX DACTYLIFERA", CreateFixture.Existing,
                new[] { new PlotNumber("DM-12", 5) });
            var added = new SpeciesMatch(species, KpiTemplates.ExistingTreesSheet, 84,
                string.Empty, SpeciesMatching.WrittenIn, true);

            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.Mosques, null, null, string.Empty, null, null, null,
                new[] { added }, null, null, null);

            List<CellWrite> onTheSheet = plan.Writes
                .Where(one => one.SheetName == KpiTemplates.ExistingTreesSheet)
                .ToList();

            Assert.Equal(2, onTheSheet.Count);
            Assert.Equal("5", onTheSheet.Single(one => one.Cell.ToString() == "B84").Stored);
            Assert.Equal("PHOENIX DACTYLIFERA", onTheSheet.Single(one => one.Cell.ToString() == "D84").Stored);

            List<NotWritten> named = plan.Skipped.Where(one => one.SheetName == KpiTemplates.ExistingTreesSheet).ToList();
            Assert.Equal(new[] { "PHOENIX DACTYLIFERA height", "PHOENIX DACTYLIFERA diameter" }, named.Select(one => one.What));
            Assert.All(named, one => Assert.Equal(SpeciesMatch.ListNotRead, one.Why));
        }

        /// <summary>
        /// A matched species writes the count alone. The workbook already spells its name and
        /// overwriting it with Revit's spelling would change the client's own list.
        /// </summary>
        [Fact]
        public void AMatchedSpeciesWritesTheCountAloneAndLeavesTheNameAsTheWorkbookSpellsIt()
        {
            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.Mosques, null, null, string.Empty, null, null, null,
                new[] { Matched("Albizia lebbeck", CreateFixture.Proposed, 13, 7) },
                null, null, null);

            List<CellWrite> onTheSheet = plan.Writes
                .Where(one => one.SheetName == KpiTemplates.ProposedTreesSheet)
                .ToList();

            Assert.Single(onTheSheet);
            Assert.Equal("B7", onTheSheet[0].Cell.ToString());
        }

        /// <summary>
        /// A species that reached no row is still named with its count, because a quantity
        /// dropped in silence leaves a tree list that reads as complete and is short.
        /// </summary>
        [Fact]
        public void ASpeciesThatReachedNoRowIsStillNamedWithItsCount()
        {
            var species = new MergedSpecies("UNKNOWN", CreateFixture.Existing,
                new[] { new PlotNumber("DM-12", 2) });
            var nowhere = new SpeciesMatch(species, KpiTemplates.ExistingTreesSheet, 0,
                string.Empty, SpeciesMatching.NoEmptyRowLeft);

            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.Mosques, null, null, string.Empty, null, null, null,
                new[] { nowhere }, null, null, null);

            NotWritten said = plan.Skipped.Single(one => one.SheetName == KpiTemplates.ExistingTreesSheet);

            Assert.Contains("UNKNOWN 2", said.What);
            Assert.Equal(SpeciesMatching.NoEmptyRowLeft, said.Why);
            Assert.DoesNotContain(plan.Writes, one => one.SheetName == KpiTemplates.ExistingTreesSheet);
        }

        private static string Written(KpiCreatePlan plan, string cell)
        {
            return plan.Writes.Single(one => one.Cell.ToString() == cell).Stored;
        }

        /// <summary>
        /// STREETS takes no area at all. The sheet works it out from the road width and the
        /// total length, which the team types.
        /// </summary>
        [Fact]
        public void StreetsSkipsTheAreaAndSaysItIsTypedByHand()
        {
            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.Streets,
                new AgreedValue(new[] { new PlotText("PL-17", "STREET") }),
                new AgreedValue(new[] { new PlotText("PL-17", "ANH-007") }),
                "KING FAHD",
                Totalled.Adding(new[] { new PlotNumber("PL-17", 1000.0) }),
                Totalled.Adding(new[] { new PlotNumber("PL-17", 70.0) }),
                Totalled.Adding(new[] { new PlotNumber("PL-17", 35.0) }),
                null,
                "2026-09-09", "B RAHAL", "BIM COORDINATOR");

            NotWritten skipped = plan.Skipped.Single(one => one.What == "Area");
            Assert.Equal(KpiCreatePlan.TypedByHand, skipped.Why);
            Assert.DoesNotContain(plan.Writes, one => one.Cell.ToString() == "D8");
        }

        [Fact]
        public void PlotsDisagreeingOnASingleValueCellWriteNothingIntoItAndSayWhy()
        {
            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.ExistingParks,
                new AgreedValue(new[]
                {
                    new PlotText("FM-05", "FRIDAY MOSQUE"),
                    new PlotText("SC-03", "SCHOOL")
                }),
                null,
                string.Empty,
                null,
                null,
                null,
                null,
                null, null, null);

            NotWritten component = plan.Skipped.Single(one => one.What == "Component");
            Assert.Equal("D3", component.Cell);
            Assert.Equal(KpiCreatePlan.Disagreed + ": FRIDAY MOSQUE, SCHOOL", component.Why);
            Assert.DoesNotContain(plan.Writes, one => one.Cell.ToString() == "D3");
        }

        [Fact]
        public void AValueNoChosenPlotHeldIsSkippedAsNotFound()
        {
            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.ExistingParks, null, null, string.Empty, null, null, null, null,
                null, null, null);

            Assert.Empty(plan.Writes);
            Assert.Equal(9, plan.Skipped.Count);
            Assert.Equal(6, plan.Skipped.Count(one => one.Why == KpiCreatePlan.NotFound));
            Assert.Equal(3, plan.Skipped.Count(one => one.Why == KpiCreatePlan.TypedByTheTeam));
        }

        [Fact]
        public void AMatchedSpeciesBecomesAQuantityInColumnBOfItsOwnRow()
        {
            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.ExistingParks, null, null, string.Empty, null, null, null,
                new[] { Matched("Albizia lebbeck", "Proposed", 18, 7) }, null, null, null);

            CellWrite write = Assert.Single(plan.Writes);
            Assert.Equal(KpiTemplates.ProposedTreesSheet, write.SheetName);
            Assert.Equal("B7", write.Cell.ToString());
            Assert.Equal("18", write.Stored);
        }

        [Fact]
        public void AnUnmatchedSpeciesIsWrittenNowhereAndNamedWithItsCount()
        {
            var species = new MergedSpecies("UNKNOWN", "Existing", new[] { new PlotNumber("DM-12", 2) });
            var unmatched = new SpeciesMatch(
                species, KpiTemplates.ExistingTreesSheet, 0, string.Empty, SpeciesMatching.NotInTheList);

            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.ExistingParks, null, null, string.Empty, null, null, null,
                new[] { unmatched }, null, null, null);

            Assert.Empty(plan.Writes);
            NotWritten skipped = plan.Skipped.Single(one => one.What.StartsWith("UNKNOWN"));
            Assert.Equal("UNKNOWN 2 under Existing", skipped.What);
            Assert.Equal(SpeciesMatching.NotInTheList, skipped.Why);
        }
    }

    public class CreateWordsTests
    {
        /// <summary>
        /// What decides Create's refusal, each written out by hand. It is decided against the
        /// live document on the Revit thread and against the pointer file at the moment the
        /// button is pressed. **The Revit half has never worked and no test here covers it:
        /// this is a test over the decision, not over Revit.**
        /// </summary>
        private static readonly OpenModel NoDocument = OpenModel.Nothing;

        private static readonly OpenModel NeverSaved = OpenModel.Of("NG05_detached");

        private static readonly OpenModel Saved = OpenModel.Of("NG05");

        private const string Somewhere = @"C:\kpi out";

        [Fact]
        public void NoDocumentSaysNoModelIsOpen()
        {
            Assert.Equal(
                "Cannot create. No model is open.",
                CreateWords.CannotCreate(NoDocument, Somewhere, true, true));
        }

        /// <summary>
        /// **THIS REVERSES THE RULE OF THE ROUND BEFORE.** A model that has never been saved
        /// used to be refused, with a line saying to save it, because the workbook was written
        /// beside the model. That cost the team most of an afternoon: a detached model is what
        /// they work on and it could not be used at all. Whether the model has been saved is
        /// now asked nowhere, and a detached model with an output folder set creates.
        /// </summary>
        [Fact]
        public void AModelThatWasNeverSavedIsNoLongerRefused()
        {
            Assert.Equal(string.Empty, CreateWords.CannotCreate(NeverSaved, Somewhere, true, true));
            Assert.Equal(
                CreateWords.CannotCreate(Saved, Somewhere, true, true),
                CreateWords.CannotCreate(NeverSaved, Somewhere, true, true));
        }

        /// <summary>
        /// What arms Create in its place, and the line says what to do about it.
        /// </summary>
        [Fact]
        public void NoOutputFolderSaysToBrowseForOne()
        {
            string said = CreateWords.CannotCreate(Saved, string.Empty, true, true);

            Assert.Equal(
                "Cannot create. No output folder is set. Press Browse beside Output folder and "
                + "point at where the filled workbooks should be written.",
                said);
            Assert.Equal(said, CreateWords.CannotCreate(Saved, "   ", true, true));
            Assert.Equal(said, CreateWords.CannotCreate(Saved, null, true, true));
        }

        [Fact]
        public void ADocumentAndAFolderRefuseNothing()
        {
            Assert.Equal(string.Empty, CreateWords.CannotCreate(Saved, Somewhere, true, true));
        }

        /// <summary>
        /// The lines are different lines. Two of them reading the same would leave somebody
        /// with no way to tell which state they are in.
        /// </summary>
        [Fact]
        public void EachStateGivesItsOwnLine()
        {
            string none = CreateWords.CannotCreate(NoDocument, Somewhere, true, true);
            string nowhere = CreateWords.CannotCreate(Saved, string.Empty, true, true);
            string ready = CreateWords.CannotCreate(Saved, Somewhere, true, true);

            Assert.Equal(3, new[] { none, nowhere, ready }.Distinct().Count());
            Assert.DoesNotContain(CreateWords.NoOutputFolder, none);
            Assert.DoesNotContain(CreateWords.NoModel, nowhere);
        }

        /// <summary>
        /// Browsing for the folder is what arms Create now, and it is the only thing that
        /// changes between these two.
        /// </summary>
        [Fact]
        public void SettingTheOutputFolderIsWhatArmsCreate()
        {
            Assert.NotEqual(string.Empty, CreateWords.CannotCreate(Saved, string.Empty, true, true));
            Assert.Equal(string.Empty, CreateWords.CannotCreate(Saved, Somewhere, true, true));
        }

        [Fact]
        public void OneRefusalListsEverythingMissingRatherThanOnePerThing()
        {
            Assert.Equal(
                "Cannot create. No model is open. " + TemplateWords.NoOutputFolder
                    + " No template picked. No plot ticked.",
                CreateWords.CannotCreate(NoDocument, string.Empty, false, false));

            Assert.Equal("Cannot create. No plot ticked.",
                CreateWords.CannotCreate(Saved, Somewhere, true, false));
            Assert.Equal(string.Empty, CreateWords.CannotCreate(Saved, Somewhere, true, true));
        }

        /// <summary>
        /// Create says the same words as the output folder line above it, from the one constant,
        /// rather than a second sentence about the same condition.
        /// </summary>
        [Fact]
        public void TheNoFolderRefusalIsTheLineTheOutputBlockAlreadyShows()
        {
            Assert.Equal(TemplateWords.NoOutputFolder, CreateWords.NoOutputFolder);
            Assert.Equal(TemplateWords.NoOutputFolder, TemplateWords.Output(string.Empty));
        }

        /// <summary>
        /// Nothing answered yet reads as no model, which is what the pane holds before Revit
        /// has said anything and what Revit says when the document is closed under it.
        /// </summary>
        [Fact]
        public void NothingAnsweredIsNoModel()
        {
            Assert.False(OpenModel.Nothing.IsOpen);
            Assert.Equal(
                "Cannot create. No model is open.",
                CreateWords.CannotCreate(null, Somewhere, true, true));
        }

        /// <summary>
        /// The title is the whole of the record now. The folder came off it with the never
        /// saved refusal, so nothing here can go stale about where the model sits.
        /// </summary>
        [Fact]
        public void TheModelRecordIsTheTitleAndNothingElse()
        {
            OpenModel model = OpenModel.Of("NG05");

            Assert.True(model.IsOpen);
            Assert.True(model.Is(OpenModel.Of("NG05")));
            Assert.False(model.Is(OpenModel.Of("NG06")));
            Assert.False(model.Is(null));
            Assert.DoesNotContain(typeof(OpenModel).GetProperties(),
                one => one.Name.IndexOf("Folder", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>
        /// A folder with no document is still no model. A document is what makes a model open
        /// and where the file goes says nothing about that.
        /// </summary>
        [Fact]
        public void AFolderWithNoModelIsStillNoModel()
        {
            Assert.Equal(
                "Cannot create. No model is open.",
                CreateWords.CannotCreate(OpenModel.Of(string.Empty), Somewhere, true, true));
        }

        /// <summary>
        /// The four values under Reference belong to a plot, so the block names which. It was
        /// printing DM-11's four with DM-12 ticked.
        /// </summary>
        [Fact]
        public void TheReferenceValuesNameThePlotTheyBelongTo()
        {
            Assert.Equal(
                "What each holds on DM-12, the first ticked plot:",
                CreateWords.ReferenceValuesOn("DM-12"));
            Assert.Equal(
                "What each holds on NS-06, the first ticked plot:",
                CreateWords.ReferenceValuesOn("  NS-06  "));
        }

        [Fact]
        public void WithNoPlotTickedTheReferenceBlockSaysThereIsNothingToShow()
        {
            Assert.Equal(
                "No plot is ticked, so there is no value to show here. Tick a plot to see what "
                + "each of the four holds on it.",
                CreateWords.NoPlotForTheReferenceValues);

            Assert.Equal(
                "What each holds on (no plot), the first ticked plot:",
                CreateWords.ReferenceValuesOn(string.Empty));
        }

        [Fact]
        public void TheTwoPlotSourcesAgreeingIsOneLineAndDisagreeingListsBoth()
        {
            Assert.Equal(
                new[] { "The sheets and the schedules name the same 2 plots." },
                CreateWords.PlotSources(PlotsInTheModel.Of(new[] { "DM-11", "DM-12" }, new[] { "DM-11", "DM-12" })));

            IReadOnlyList<string> split = CreateWords.PlotSources(
                PlotsInTheModel.Of(new[] { "DM-11", "DM-12" }, new[] { "DM-11", "NS-06" }));

            Assert.Equal(3, split.Count);
            Assert.Contains("On a sheet and on no schedule, 1: DM-12", split[1]);
            Assert.Contains("On a schedule and on no sheet, 1: NS-06", split[2]);
        }

        /// <summary>
        /// The plots block reads four ways, each its own line, so open a model never stands
        /// beside a model's own name again. It sent the team to Revit for an hour: a scan
        /// filled the header while this block said open a model, because it said so on any
        /// answer not yet back rather than only on no document. The two are told apart by
        /// whether a document is open, which the pane reads off the live document, and by
        /// whether the plots have come back, a null standing for not answered rather than
        /// answered with none.
        /// </summary>
        [Fact]
        public void ThePlotsBlockReadsFourWaysOneLineEach()
        {
            // No document: the one place open a model is right.
            Assert.Equal(
                new[] { "No model open. This pane reads a model's plots as soon as one is open." },
                CreateWords.PlotsBlock(false, null));

            // A document open, the plots not back yet: waiting, NOT open a model.
            Assert.Equal(
                new[] { "Reading the plots from the model. On a large model this takes a moment." },
                CreateWords.PlotsBlock(true, null));

            // A document answered with no plots: the model holds none.
            Assert.Equal(
                new[] { "No plot in this model. Press KPI Scan first, or open a model that holds one." },
                CreateWords.PlotsBlock(true, PlotsInTheModel.Of(null, null)));

            // A document answered with plots: the count, off PlotSources.
            Assert.Equal(
                new[] { "The sheets and the schedules name the same 2 plots." },
                CreateWords.PlotsBlock(true, PlotsInTheModel.Of(new[] { "DM-11", "DM-12" }, new[] { "DM-11", "DM-12" })));
        }

        [Fact]
        public void TheSuggestedNameFallsBackFromTheComponentToThePlotToTheCount()
        {
            Assert.Equal("MOSQUES FRIDAY MOSQUE",
                CreateWords.SuggestedName(KpiTemplates.Mosques, "FRIDAY MOSQUE", new[] { "FM-05" }));

            Assert.Equal("MOSQUES FM-05",
                CreateWords.SuggestedName(KpiTemplates.Mosques, string.Empty, new[] { "FM-05" }));

            Assert.Equal("MOSQUES 3 plots",
                CreateWords.SuggestedName(KpiTemplates.Mosques, string.Empty, new[] { "FM-05", "FM-06", "FM-07" }));

            Assert.Equal("MOSQUES",
                CreateWords.SuggestedName(KpiTemplates.Mosques, string.Empty, new string[0]));
        }

        [Fact]
        public void TheConfirmLineNamesEveryPlotSharingAnArea()
        {
            var identical = new[]
            {
                new IdenticalArea(new[] { "MM-03", "MM-04" }, 12182.05561411, "1131.7 m2")
            };

            IReadOnlyList<string> said = CreateWords.ConfirmIdentical(identical);

            Assert.Equal(2, said.Count);
            Assert.Contains("1 set of chosen plots report the same area", said[0]);
            Assert.Contains("MM-03, MM-04", said[1]);
            Assert.Empty(CreateWords.ConfirmIdentical(new IdenticalArea[0]));
        }
    }
}
