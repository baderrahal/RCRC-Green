using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **A PLOT COULD GET A TEMPLATE AND NO FOLDER, AND SIX DID.**
    ///
    /// Measured on the 09:18 run, NG05, 156 plots over 7 templates, 150 workbooks. EP-05, EP-11,
    /// EP-12, EP-13, EP-15 and FM-08 were ticked, read, and dropped at the last step with
    /// "the component folder table does not hold an empty component". Those plots are on no
    /// sheet, so they carry no component, so <see cref="PlotsPerTemplate"/> placed them by their
    /// PLOT PREFIX, which is what its own rule says it must do and what its docstring names
    /// EP-05, EP-11, EP-12 and EP-13 as the case for. Then the folder table, keyed on the
    /// component, had nothing for them.
    ///
    /// **Two routes to a template and one to a folder**, which is the two records shape where
    /// the two records are the two steps of one decision.
    ///
    /// The answer adds no table. The template and the folder are two columns of one row per
    /// component, so which folders a template reaches is already written down. Under the rules of
    /// 29 Oct five templates reach exactly ONE folder, and that one is where a plot with no
    /// component of its own is filed. MOSQUES and SCHOOLS file under each value, so FM-08 is
    /// still refused, and it is refused BEFORE the press.
    /// </summary>
    public class PlotWithNoComponentTests
    {
        private static string Joined(params string[] parts)
        {
            return string.Join(Path.DirectorySeparatorChar.ToString(), parts);
        }

        /// <summary>
        /// Counted off the rules by hand rather than worked out with the same rule the code uses.
        /// Five templates reach one named folder each.
        /// </summary>
        [Theory]
        [InlineData("EXISTING PARKS", "EXISTING PARK")]
        [InlineData("FUTURE PARKS", "FUTURE PARKS")]
        [InlineData("HEALTHCARE", "HEALTHCARE")]
        [InlineData("PARKING", "PARKING LOT")]
        [InlineData("STREETS", "STREETS")]
        public void FiveTemplatesReachExactlyOneFolderAndThatIsTheOne(string name, string folder)
        {
            KpiTemplate template = KpiTemplates.All.Single(one => one.Name == name);

            Assert.Equal(new[] { folder }, ComponentFolders.FoldersOf(template));
            Assert.Equal(folder, ComponentFolders.OnlyFolderFor(template));
        }

        /// <summary>
        /// **MOSQUES and SCHOOLS cannot answer**, and nothing is derived for them. Their rules file
        /// each value under a folder named as the value, so a plot with no component has no value
        /// to name its folder. SCHOOLS reached SCHOOL alone while SCHOOL was its only row, and the
        /// rule of 29 Oct makes it the same as MOSQUES, Q14.
        /// </summary>
        [Theory]
        [InlineData("MOSQUES")]
        [InlineData("SCHOOLS")]
        public void AFolderPerValueSoNothingIsDerived(string name)
        {
            KpiTemplate template = KpiTemplates.All.Single(one => one.Name == name);

            Assert.Empty(ComponentFolders.FoldersOf(template));
            Assert.Equal(string.Empty, ComponentFolders.OnlyFolderFor(template));
            Assert.Equal(
                name + " files each value under a folder named as the value, and this plot carries no "
                + "component to say which",
                ComponentFolders.NoFolderForTemplate(template));
        }

        /// <summary>
        /// **The five park plots the 09:18 run threw away.** EP-05, EP-11, EP-12, EP-13 and
        /// EP-15 file under EXISTING PARK, where every other EXISTING PARKS plot files.
        /// </summary>
        [Theory]
        [InlineData("ANH-007-EP-100005")]
        [InlineData("ANH-007-EP-100011")]
        public void AParkPlotWithNoComponentIsFiledUnderItsTemplatesOwnFolder(string uid2)
        {
            PlotWorkbookPath path = PlotWorkbookPath.For(
                Joined("R"), KpiTemplates.ExistingParks, string.Empty, uid2);

            Assert.True(path.Ok, path.Why);
            Assert.Equal("EXISTING PARK", path.Folder);
            Assert.Equal(Joined("R", "EXISTING PARK", uid2, uid2 + ".xlsx"), path.FilePath);
        }

        /// <summary>
        /// **FM-08 is still refused, and the reason says the folder is named per value** rather
        /// than saying the table does not hold an empty component, which told nobody anything.
        /// </summary>
        [Fact]
        public void AMosquePlotWithNoComponentIsStillRefusedAndSaysWhy()
        {
            PlotWorkbookPath path = PlotWorkbookPath.For(
                Joined("R"), KpiTemplates.Mosques, string.Empty, "ANH-007-MO-100008");

            Assert.False(path.Ok);
            Assert.Equal(
                "MOSQUES files each value under a folder named as the value, and this plot carries no "
                + "component to say which",
                path.Why);
        }

        /// <summary>
        /// **AN ABSENCE AND AN ANSWER NOBODY KNOWS ARE TWO DIFFERENT THINGS.** A component the
        /// table does not hold still places nothing and still names the value, because falling
        /// back to the template there would file a value the team has never seen under a folder
        /// the team never chose.
        /// </summary>
        [Fact]
        public void AComponentTheTableDoesNotHoldStillPlacesNothingEvenWithATemplate()
        {
            PlotWorkbookPath path = PlotWorkbookPath.For(
                Joined("R"), KpiTemplates.Schools, "GOVERMENT BUILDING", "ANH-007-GB-100001");

            Assert.False(path.Ok);
            Assert.Equal(
                "the component folder table does not hold GOVERMENT BUILDING, so nothing says "
                + "which folder this plot is filed under",
                path.Why);
        }

        /// <summary>
        /// A plot that no template placed has no folder either, and the reason says which of the
        /// two it was rather than reading as a folder question.
        /// </summary>
        [Fact]
        public void APlotNoTemplatePlacedIsSaidToBeThatRatherThanAFolderQuestion()
        {
            PlotWorkbookPath path = PlotWorkbookPath.For(Joined("R"), null, string.Empty, "ANH-1");

            Assert.False(path.Ok);
            Assert.Equal("no template placed this plot, so it has no folder either", path.Why);
        }

        /// <summary>
        /// **A plot WITH a component is unmoved.** DAILY MOSQUE and FRIDAY MOSQUE fill one
        /// workbook and are filed apart, so the plot's own component still decides and the
        /// template never overrides it.
        /// </summary>
        [Theory]
        [InlineData("DAILY MOSQUE", "DAILY MOSQUE")]
        [InlineData("FRIDAY MOSQUE", "FRIDAY MOSQUE")]
        public void APlotWithAComponentIsStillFiledByItsOwnComponent(string component, string folder)
        {
            PlotWorkbookPath path = PlotWorkbookPath.For(
                Joined("R"), KpiTemplates.Mosques, component, "ANH-008-MO-100006");

            Assert.True(path.Ok, path.Why);
            Assert.Equal(folder, path.Folder);
        }

        private static TemplateSplit SplitOf(
            IReadOnlyDictionary<string, string> components, params KpiTemplate[] ticked)
        {
            return PlotsPerTemplate.Split(
                components.Keys.ToList(),
                plot => components[plot],
                ticked);
        }

        /// <summary>
        /// **Said BEFORE the press, so the user sees it coming.** The count of ticked plots with
        /// no component, how many of them can be filed, and by name the one that cannot.
        /// </summary>
        [Fact]
        public void ThePaneCountsThePlotsWithNoComponentAndNamesTheOneThatCanBeFiledNowhere()
        {
            var components = new Dictionary<string, string>
            {
                { "EP-05", string.Empty },
                { "EP-11", string.Empty },
                { "EP-12", string.Empty },
                { "FM-08", string.Empty },
                { "DM-11", "FRIDAY MOSQUE" }
            };

            IReadOnlyList<string> said = CreateWords.PlotsWithNoComponent(
                SplitOf(components, KpiTemplates.ExistingParks, KpiTemplates.Mosques));

            Assert.Equal(
                new[]
                {
                    "4 ticked plots carry no component, because it is on no sheet, and the plot "
                    + "prefix placed each one, EP-05, EP-11, EP-12, FM-08.",
                    "3 of those file under their template's own folder, EP-05, EP-11, EP-12.",
                    "FM-08 WILL BE READ AND WRITTEN NOWHERE: MOSQUES files each value under a folder "
                    + "named as the value, and this plot carries no component to say which. Untick it, "
                    + "or give it a sheet carrying a component."
                },
                said);
        }

        /// <summary>
        /// A run where every ticked plot carries a component says nothing at all, because a note
        /// about nothing is a line the team reads past on every other press.
        /// </summary>
        [Fact]
        public void ARunWhereEveryPlotCarriesAComponentSaysNothing()
        {
            var components = new Dictionary<string, string>
            {
                { "DM-11", "FRIDAY MOSQUE" },
                { "DM-12", "DAILY MOSQUE" }
            };

            Assert.Empty(CreateWords.PlotsWithNoComponent(SplitOf(components, KpiTemplates.Mosques)));
            Assert.Empty(CreateWords.PlotsWithNoComponent(null));
        }

        /// <summary>
        /// One plot with no component reads in the singular, because the count is on screen
        /// before every press and a line reading 1 plots is what a person stops trusting.
        /// </summary>
        [Fact]
        public void OnePlotWithNoComponentReadsInTheSingular()
        {
            var components = new Dictionary<string, string> { { "EP-05", string.Empty } };

            IReadOnlyList<string> said = CreateWords.PlotsWithNoComponent(
                SplitOf(components, KpiTemplates.ExistingParks));

            Assert.Equal(
                "1 ticked plot carries no component, because it is on no sheet, and the plot "
                + "prefix placed it, EP-05.",
                said[0]);
            Assert.Equal("1 of those file under its template's own folder, EP-05.", said[1]);
        }

        /// <summary>
        /// **The pane never names PRX_COMPONENT**, which is the workbook's note and is in no
        /// model. The rule already has a test over the template block and this is the same rule
        /// over the same words on a different line.
        /// </summary>
        [Fact]
        public void NoLineNamesTheParameterTheNoteAsksFor()
        {
            var components = new Dictionary<string, string> { { "FM-08", string.Empty } };

            foreach (string line in CreateWords.PlotsWithNoComponent(
                SplitOf(components, KpiTemplates.Mosques)))
            {
                Assert.DoesNotContain("PRX_COMPONENT", line, StringComparison.Ordinal);
            }
        }
    }
}
