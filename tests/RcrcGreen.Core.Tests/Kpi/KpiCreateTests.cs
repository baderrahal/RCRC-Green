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

    public class TemplateForComponentTests
    {
        private static AgreedValue Holding(params string[] components)
        {
            return new AgreedValue(components.Select((one, at) => new PlotText("P-" + at, one)));
        }

        /// <summary>
        /// FRIDAY MOSQUE on the chosen plots preselects MOSQUES, because MOSQUES holds MOSQUE.
        /// </summary>
        [Fact]
        public void FridayMosquePreselectsMosques()
        {
            TemplateChoice choice = TemplateForComponent.For(Holding("FRIDAY MOSQUE"), null);

            Assert.False(choice.NeedsAPick);
            Assert.Equal("MOSQUES", choice.Preselected.Name);
        }

        [Fact]
        public void SchoolPreselectsSchools()
        {
            Assert.Equal("SCHOOLS", TemplateForComponent.For(Holding("SCHOOL"), null).Preselected.Name);
        }

        /// <summary>
        /// EXISTING PARKS and FUTURE PARKS both answer to a park, so that pair is always the
        /// user's choice and nothing here breaks the tie.
        ///
        /// PARKING is offered beside them, because PARKING begins with PARK and no rule on a
        /// name can tell a park from a parking plot. Offering it is the honest answer: the tool
        /// cannot separate them, so it says so rather than dropping one behind the user's back.
        /// </summary>
        [Fact]
        public void AParkAlwaysPutsThePickToTheUser()
        {
            TemplateChoice choice = TemplateForComponent.For(Holding("PARK"), null);

            Assert.True(choice.NeedsAPick);
            Assert.Equal(new[] { "EXISTING PARKS", "FUTURE PARKS", "PARKING" },
                choice.Candidates.Select(one => one.Name));
        }

        [Fact]
        public void PlotsDisagreeingOnTheComponentPutThePickToTheUser()
        {
            TemplateChoice choice = TemplateForComponent.For(Holding("FRIDAY MOSQUE", "SCHOOL"), null);

            Assert.True(choice.NeedsAPick);
            Assert.Contains("FRIDAY MOSQUE, SCHOOL", choice.Why);
            Assert.Equal(7, choice.Candidates.Count);
        }

        [Fact]
        public void AComponentAnsweringNoTemplatePutsThePickToTheUserAndNamesIt()
        {
            TemplateChoice choice = TemplateForComponent.For(Holding("PUMP STATION"), null);

            Assert.True(choice.NeedsAPick);
            Assert.Contains("PUMP STATION", choice.Why);
        }

        [Fact]
        public void NoComponentAtAllPutsThePickToTheUser()
        {
            TemplateChoice choice = TemplateForComponent.For(Holding(string.Empty), null);

            Assert.True(choice.NeedsAPick);
            Assert.Equal(TemplateForComponent.NoComponent, choice.Why);
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
                null);

            NotWritten component = plan.Skipped.Single(one => one.What == "Component");
            Assert.Equal("D3", component.Cell);
            Assert.Equal(KpiCreatePlan.Disagreed + ": FRIDAY MOSQUE, SCHOOL", component.Why);
            Assert.DoesNotContain(plan.Writes, one => one.Cell.ToString() == "D3");
        }

        [Fact]
        public void AValueNoChosenPlotHeldIsSkippedAsNotFound()
        {
            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.ExistingParks, null, null, string.Empty, null, null, null, null);

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
                new[] { Matched("Albizia lebbeck", "Proposed", 18, 7) });

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
                new[] { unmatched });

            Assert.Empty(plan.Writes);
            NotWritten skipped = plan.Skipped.Single(one => one.What.StartsWith("UNKNOWN"));
            Assert.Equal("UNKNOWN 2 under Existing", skipped.What);
            Assert.Equal(SpeciesMatching.NotInTheList, skipped.Why);
        }
    }

    public class CreateWordsTests
    {
        [Fact]
        public void OneRefusalListsEverythingMissingRatherThanOnePerThing()
        {
            Assert.Equal(
                "Cannot create. No model is open. No template picked. No plot ticked.",
                CreateWords.CannotCreate(false, false, false));

            Assert.Equal("Cannot create. No plot ticked.", CreateWords.CannotCreate(true, true, false));
            Assert.Equal(string.Empty, CreateWords.CannotCreate(true, true, true));
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
