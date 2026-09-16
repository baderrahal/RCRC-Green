using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **A GUARD THAT SWITCHES ITSELF OFF READS EXACTLY LIKE A GUARD THAT PASSED.** Where a
    /// template's canopy total column could not be read, the check used to return an empty
    /// reason: the green cover and the canopy percentage were written with no total canopy check
    /// at all, every empty row was still offered to a new species, and the chain's own reason was
    /// printed nowhere.
    ///
    /// The blanking and the narrowing are pinned in <see cref="TotalCanopyColumnTests"/>. These
    /// are the fourth rule of that item: **the glance names every such template**, once for the
    /// press rather than once per plot, because a press of 154 plots on seven templates would
    /// otherwise print one line per plot for a fact about a file.
    ///
    /// **EVERY EXPECTED VALUE HERE IS WRITTEN OUT BY HAND.**
    /// </summary>
    public class UnreadableCanopyColumnsTests : IDisposable
    {
        private readonly string _folder = WorkbookFixture.Folder();

        public void Dispose()
        {
            try
            {
                Directory.Delete(_folder, true);
            }
            catch (IOException)
            {
            }
        }

        private static KpiCreateRunSet Set(params KpiCreateRun[] runs)
        {
            var outcomes = new List<TemplateOutcome>();
            foreach (KpiTemplate template in runs.Select(one => one.Template).Distinct())
            {
                KpiTemplate held = template;
                var plots = runs
                    .Where(one => ReferenceEquals(one.Template, held))
                    .SelectMany(one => one.Readings.Select(reading => reading.PlotId))
                    .ToList();

                outcomes.Add(TemplateOutcome.Wrote(held, plots, plots.Count, @"C:\out"));
            }

            return new KpiCreateRunSet(
                "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                new TemplateSplit(
                    new List<TemplateShare>(), new List<PlotTemplate>(), new List<PlotTemplate>()),
                runs,
                outcomes);
        }

        /// <summary>
        /// A run on a template read off a real file, so the two species lists carry whatever that
        /// file's chain really said rather than a reason written into them here.
        /// </summary>
        private KpiCreateRun RunOn(string path)
        {
            LabelledCells computed = LabelledPlaces.In(path, KpiTemplates.Mosques, ComputedPlaces.All);

            IReadOnlyList<TotalCanopyColumn> columns = TotalCanopyColumns.In(
                path, KpiTemplates.Mosques, computed.For(ComputedPlaces.GreenCoverName));

            return CreateFixture.Run(
                template: KpiTemplates.Mosques,
                existingList: SpeciesList.In(
                    path,
                    KpiTemplates.Mosques.ExistingTrees,
                    TotalCanopyColumns.For(columns, KpiTemplates.ExistingTreesSheet)),
                proposedList: SpeciesList.In(
                    path,
                    KpiTemplates.Mosques.ProposedTrees,
                    TotalCanopyColumns.For(columns, KpiTemplates.ProposedTreesSheet)));
        }

        private string Typed(string fileName)
        {
            return WorkbookFixture.Computing(
                _folder,
                new[] { new WorkbookFixture.TreeRow(4, "Albizia lebbeck", "15", "8") },
                new[] { new WorkbookFixture.TreeRow(4, "Phoenix dactylifera", "15", "8") },
                fileName: fileName,
                canopyCellIsTyped: true,
                withGreenCoverLabel: true);
        }

        /// <summary>
        /// **BOTH TREE LIST SHEETS ARE NAMED AND THE TEMPLATE IS NAMED ONCE.** A press is one run
        /// per plot, so a template of 78 plots would otherwise name its own two sheets 78 times.
        /// </summary>
        [Fact]
        public void EverySheetWhoseColumnCouldNotBeReadIsNamedOncePerPress()
        {
            IReadOnlyList<UnreadCanopyColumn> named = UnreadableCanopyColumns.In(
                Set(RunOn(Typed("typed.xlsx")), RunOn(Typed("typed-two.xlsx"))));

            Assert.Equal(
                new[] { "Tree List - Existing", "Tree List - Proposed" },
                named.Select(one => one.SheetName).ToArray());

            Assert.Equal(new[] { "MOSQUES", "MOSQUES" }, named.Select(one => one.TemplateName).ToArray());

            Assert.Equal(
                "MOSQUES, Tree List - Existing: the canopy cell holds no formula, so nothing says "
                + "which cells its total comes off",
                named[0].InWords);
        }

        /// <summary>
        /// **THE GLANCE LINE, WRITTEN OUT BY HAND.** It says how many sheets, how many templates
        /// and what the cost of it was, and names the templates.
        /// </summary>
        [Fact]
        public void TheGlanceLineNamesTheTemplatesAndWhatItCost()
        {
            KpiCreateRunSet set = Set(RunOn(Typed("glance.xlsx")));

            Assert.Equal(
                "THE TEMPLATES WHOSE CANOPY TOTAL COLUMN COULD NOT BE READ: 2 tree list sheets "
                + "on 1 template could not be read, so no plot holding a count on them got a "
                + "Total areas to be greened or a canopy percentage. MOSQUES.",
                UnreadableCanopyColumns.InWords(UnreadableCanopyColumns.In(set)));

            Assert.Equal(
                "THE TEMPLATES WHOSE CANOPY TOTAL COLUMN COULD NOT BE READ: 2 tree list sheets "
                + "on 1 template could not be read, so no plot holding a count on them got a "
                + "Total areas to be greened or a canopy percentage. MOSQUES.",
                RunAtAGlance.Of(set).UnreadCanopy);
        }

        /// <summary>
        /// **A PRESS WHERE EVERY TEMPLATE READ SAYS SO.** A line that disappears when there is
        /// nothing to report reads the same as one nobody wrote, which is the shape this whole
        /// item is about.
        /// </summary>
        [Fact]
        public void APressWhereEveryTemplateReadSaysSo()
        {
            Assert.Equal(
                "THE TEMPLATES WHOSE CANOPY TOTAL COLUMN COULD NOT BE READ: every ticked "
                + "template's canopy total named the column it adds.",
                UnreadableCanopyColumns.InWords(new List<UnreadCanopyColumn>()));
        }

        /// <summary>
        /// **A TEMPLATE NAMING NO Total Green cover CELL IS NOT IN HERE.** It holds no canopy
        /// total, so a row of it reaches none by construction and there is nothing to check,
        /// which is Bader's decision of 16 September and the one case that is unchanged.
        /// </summary>
        [Fact]
        public void ATemplateNamingNoGreenCoverCellIsNotNamedHere()
        {
            string path = WorkbookFixture.Computing(
                _folder,
                new[] { new WorkbookFixture.TreeRow(4, "Albizia lebbeck", "15", "8") },
                new[] { new WorkbookFixture.TreeRow(4, "Phoenix dactylifera", "15", "8") },
                fileName: "nolabel.xlsx");

            KpiCreateRunSet set = Set(RunOn(path));

            Assert.Empty(UnreadableCanopyColumns.In(set));

            Assert.Equal(
                "THE TEMPLATES WHOSE CANOPY TOTAL COLUMN COULD NOT BE READ: every ticked "
                + "template's canopy total named the column it adds.",
                RunAtAGlance.Of(set).UnreadCanopy);
        }

        /// <summary>
        /// **AND A TEMPLATE WHOSE CHAIN READS FINE IS NOT IN IT EITHER.** Today's seven read
        /// fine, so a press that named one of them would be a false alarm on every run.
        /// </summary>
        [Fact]
        public void ATemplateWhoseChainReadsIsNotNamed()
        {
            string path = WorkbookFixture.Computing(
                _folder,
                new[] { new WorkbookFixture.TreeRow(4, "Albizia lebbeck", "15", "8") },
                new[] { new WorkbookFixture.TreeRow(4, "Phoenix dactylifera", "15", "8") },
                fileName: "sound.xlsx",
                withGreenCoverLabel: true);

            Assert.Empty(UnreadableCanopyColumns.In(Set(RunOn(path))));
        }
    }
}
