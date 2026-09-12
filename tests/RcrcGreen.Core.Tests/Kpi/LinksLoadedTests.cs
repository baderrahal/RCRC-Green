using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **Measured on the first STREETS run, NG05 at 16:06, 78 plots.** All 78 contributed
    /// nothing and all 156 schedules printed one row, the header, and no body. The scan from
    /// the same session says six link instances and NONE LOADED, including
    /// RCRC_NG05_NU_MAIN_RVT24_00.rvt. The plants live in the linked component models, so the
    /// tool was right and every plot carried its reason. What it never said was the one thing
    /// that explains all 78 at once.
    ///
    /// Every expected value below is written out by hand.
    /// </summary>
    public class LinksLoadedTests
    {
        private static ScannedLinkInstance Link(string name, bool loaded)
        {
            return new ScannedLinkInstance(name, name + " type", loaded);
        }

        private static LinksLoaded Of(params ScannedLinkInstance[] instances)
        {
            return LinksLoaded.Of(new LinkFacts(null, instances, null));
        }

        /// <summary>
        /// The 16:06 run's own shape, six instances and not one loaded.
        /// </summary>
        [Fact]
        public void NotOneLoadedIsSaidWithTheCountAndEveryName()
        {
            LinksLoaded links = Of(
                Link("RCRC_NG05_NU_MAIN_RVT24_00.rvt", false),
                Link("RCRC_NG05_NU_MAIN_RVT24_01.rvt", false));

            Assert.True(links.Worth);
            Assert.Equal(2, links.Instances);
            Assert.Equal(0, links.Loaded);
            Assert.Equal(
                "NO LINK IS LOADED. 2 link instances and not one of them loaded, so every schedule that "
                + "lists linked elements lists nothing and every plot will contribute nothing. Load the "
                + "links and read the model again. Not loaded: RCRC_NG05_NU_MAIN_RVT24_00.rvt, "
                + "RCRC_NG05_NU_MAIN_RVT24_01.rvt.",
                links.Warning);
        }

        /// <summary>
        /// One instance reads in the singular, because a line saying 1 link instances is a
        /// line somebody has to read twice.
        /// </summary>
        [Fact]
        public void OneInstanceReadsInTheSingular()
        {
            Assert.Equal(
                "NO LINK IS LOADED. 1 link instance and not one of them loaded, so every schedule that "
                + "lists linked elements lists nothing and every plot will contribute nothing. Load the "
                + "links and read the model again. Not loaded: 00.rvt.",
                Of(Link("00.rvt", false)).Warning);
        }

        /// <summary>
        /// Every link loaded is the one state worth saying nothing about, so the report and
        /// the pane stay quiet and the run reads as it always did.
        /// </summary>
        [Fact]
        public void EveryLinkLoadedSaysNothingAtAll()
        {
            LinksLoaded links = Of(Link("00.rvt", true), Link("01.rvt", true));

            Assert.False(links.Worth);
            Assert.Equal(string.Empty, links.Warning);
            Assert.Equal(2, links.Instances);
            Assert.Equal(2, links.Loaded);
        }

        /// <summary>
        /// Some loaded and some not is its own line: the run will find what the loaded ones
        /// hold and nothing of the rest, which is not the same as finding nothing.
        /// </summary>
        [Fact]
        public void SomeLoadedAndSomeNotNamesTheOnesThatAreNot()
        {
            LinksLoaded links = Of(Link("00.rvt", true), Link("01.rvt", false), Link("02.rvt", false));

            Assert.True(links.Worth);
            Assert.Equal(1, links.Loaded);
            Assert.Equal(
                "2 link instances of 3 not loaded, so a schedule that lists what they hold lists nothing. "
                + "Not loaded: 01.rvt, 02.rvt.",
                links.Warning);
        }

        /// <summary>
        /// A model holding no link at all is not a model whose links are fine. The plants are
        /// in the linked component models, so it lists nothing either, and it says so in its
        /// own words rather than borrowing the none loaded ones.
        /// </summary>
        [Fact]
        public void NoLinkAtAllHasItsOwnLine()
        {
            LinksLoaded links = Of();

            Assert.True(links.Worth);
            Assert.Equal(0, links.Instances);
            Assert.Equal(
                "THIS MODEL HOLDS NO LINKED MODEL. The plants are in the linked component models, so "
                + "every schedule that lists them lists nothing.",
                links.Warning);
        }

        /// <summary>
        /// A read that did not happen claims nothing either way, the rule the whole scan
        /// follows: a guard that cannot see its subject says so rather than passing.
        /// </summary>
        [Fact]
        public void AReadThatDidNotHappenClaimsNothing()
        {
            Assert.False(LinksLoaded.NotRead.Worth);
            Assert.False(LinksLoaded.NotRead.WasRead);
            Assert.Equal(string.Empty, LinksLoaded.NotRead.Warning);
            Assert.False(LinksLoaded.Of(null).WasRead);

            LinksLoaded threw = LinksLoaded.Of(LinkFacts.NotRead("the link read threw"));
            Assert.True(threw.Worth);
            Assert.False(threw.WasRead);
            Assert.Equal(
                "THE LINKED MODELS WERE NOT READ, so nothing here can say whether the schedules had "
                + "anything to list. the link read threw",
                threw.Warning);
        }

        /// <summary>
        /// **At the TOP of the checklist report, before the reconciliation.** The 16:06 run
        /// ended with 78 identical lines and the one fact explaining all of them was nowhere
        /// in the file, while the scan report said it in section 4.
        /// </summary>
        [Fact]
        public void TheReportOpensWithTheReasonARunFoundNothing()
        {
            string report = KpiCreateReport.Write(
                CreateFixture.Run(links: Of(Link("RCRC_NG05_NU_MAIN_RVT24_00.rvt", false))),
                new System.DateTime(2026, 9, 12, 16, 6, 0));

            Assert.Contains(
                "NO LINK IS LOADED. 1 link instance and not one of them loaded, so every schedule that "
                + "lists linked elements lists nothing and every plot will contribute nothing. Load the "
                + "links and read the model again. Not loaded: RCRC_NG05_NU_MAIN_RVT24_00.rvt.",
                report);

            Assert.True(
                report.IndexOf("NO LINK IS LOADED", System.StringComparison.Ordinal)
                    < report.IndexOf("RECONCILIATION", System.StringComparison.Ordinal),
                "the reason must come before the reconciliation");
        }

        /// <summary>
        /// A run whose links are all loaded says nothing about them, so the report reads as it
        /// always did and the line means something when it does appear.
        /// </summary>
        [Fact]
        public void AReportSaysNothingAboutLinksThatAreAllLoaded()
        {
            string report = KpiCreateReport.Write(
                CreateFixture.Run(links: Of(Link("00.rvt", true))),
                new System.DateTime(2026, 9, 12, 16, 6, 0));

            Assert.DoesNotContain("NO LINK IS LOADED", report);
            Assert.DoesNotContain("not loaded", report);
        }

        /// <summary>
        /// **The counts a zero cannot fake.** The schedules holding a group line reads 0 both
        /// when the rule worked and when nothing was found at all, and on the 16:06 run it
        /// read green over a run where all 156 schedules printed one row and no body.
        /// </summary>
        [Fact]
        public void TheReconciliationCountsWhatWasFoundAndNotOnlyWhatWasLeftOut()
        {
            // Two group rows off the softscape and one shrubs and lawn group is three, and one
            // of the two schedules printed a body.
            string found = KpiCreateReport.Write(
                CreateFixture.Run(new[]
                {
                    CreateFixture.Plot(
                        "DM-12",
                        subtotals: new[] { CreateFixture.Subtotal(KpiMerge.ShrubsHeading, 30.0, 39) },
                        printedGroups: new[]
                        {
                            new PrintedGroup("Existing", 3, null, true, 10, 5, true, string.Empty),
                            new PrintedGroup("Proposed", 6, null, true, 20, 9, true, string.Empty)
                        },
                        printedSchedules: new[]
                        {
                            CreateFixture.Softscape("DM-12", new[] { "ALBIZIA LEBBECK", "10" }),
                            CreateFixture.ShrubsAndLawn("DM-12")
                        })
                }),
                new System.DateTime(2026, 9, 12, 16, 6, 0));

            Assert.Contains("  group rows found across the run   3", found);
            Assert.Contains("  schedules that printed a body   1 of 2", found);

            // **The 16:06 shape.** Two schedules, each printing one row, the header, and no
            // body, and no group row anywhere. The line above them reads 0 on both runs, which
            // is exactly why these two exist.
            string nothing = KpiCreateReport.Write(
                CreateFixture.Run(new[]
                {
                    CreateFixture.Plot(
                        "ST-05",
                        printedSchedules: new[]
                        {
                            CreateFixture.Softscape("ST-05"),
                            CreateFixture.ShrubsAndLawn("ST-05")
                        })
                }),
                new System.DateTime(2026, 9, 12, 16, 6, 0));

            Assert.Contains("  group rows found across the run   0", nothing);
            Assert.Contains("  schedules that printed a body   0 of 2", nothing);
        }
    }
}
