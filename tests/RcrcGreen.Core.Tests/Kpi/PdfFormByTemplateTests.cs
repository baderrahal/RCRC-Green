using System;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// Bader's rule of 29 October, K35.01: the form follows the template the plot was placed
    /// in, and the prefix answers only for a plot no template placed. NG05's walkway was placed
    /// in STREETS by component-rules.csv and wrote no PDF, because its prefix is none of the ten.
    /// </summary>
    public class PdfFormByTemplateTests
    {
        private static readonly DateTime Today = new DateTime(2026, 10, 29);

        private const string Parks = "Projects Basic Data - Parks";

        private const string OpenSpaces = "Projects Basic Data - Open spaces associated to buildings";

        private const string Roads = "Projects Basic Data - Roads";

        public static TheoryData<KpiTemplate, string> EveryTemplate()
        {
            return new TheoryData<KpiTemplate, string>
            {
                { KpiTemplates.Streets, Roads },
                { KpiTemplates.Mosques, OpenSpaces },
                { KpiTemplates.MosquesMoreThan60, OpenSpaces },
                { KpiTemplates.MosquesLessThan60, OpenSpaces },
                { KpiTemplates.Schools, OpenSpaces },
                { KpiTemplates.SchoolsMoreThan60, OpenSpaces },
                { KpiTemplates.SchoolsLessThan60, OpenSpaces },
                { KpiTemplates.Healthcare, OpenSpaces },
                { KpiTemplates.Parking, OpenSpaces },
                { KpiTemplates.ExistingParks, Parks },
                { KpiTemplates.FutureParks, Parks }
            };
        }

        [Theory]
        [MemberData(nameof(EveryTemplate))]
        public void EveryTemplateTakesItsForm(KpiTemplate template, string form)
        {
            Assert.Equal(form, PdfForms.ForTemplate(template).Name);
        }

        /// <summary>
        /// Eleven templates, so a template added to the list with no form fails here rather
        /// than writing no PDF on a real press.
        /// </summary>
        [Fact]
        public void EveryTemplateTheToolKnowsHasAForm()
        {
            Assert.Equal(11, KpiTemplates.All.Count);

            foreach (KpiTemplate template in KpiTemplates.All)
            {
                Assert.True(PdfForms.ForTemplate(template) != null, template.Name + " takes no form");
            }
        }

        /// <summary>
        /// **Every plot prefix measured so far reaches the form it reached before**, now through
        /// the template its prefix names. The expected forms are written out by hand, the ten of
        /// K35.01's table.
        /// </summary>
        [Theory]
        [InlineData("EP-14", Parks)]
        [InlineData("FP-17", Parks)]
        [InlineData("HF-01", OpenSpaces)]
        [InlineData("FM-05", OpenSpaces)]
        [InlineData("DM-11", OpenSpaces)]
        [InlineData("PL-24", OpenSpaces)]
        [InlineData("SC-06", OpenSpaces)]
        [InlineData("NS-03", Roads)]
        [InlineData("ST-05", Roads)]
        [InlineData("MM-01", Roads)]
        public void EveryMeasuredPrefixReachesTheSameFormThroughItsTemplate(string plotId, string form)
        {
            Assert.Equal(form, PdfForms.ForPlot(plotId, null).Name);
            Assert.Equal(form, PdfForms.ForPlot(plotId, PlotPrefixes.For(plotId)).Name);
        }

        /// <summary>
        /// The walkway: a prefix no table holds, placed in STREETS, takes the Roads form. With no
        /// template it is what it was, no form and named.
        /// </summary>
        [Fact]
        public void APlotWhosePrefixNoTableHoldsTakesItsTemplatesForm()
        {
            Assert.Same(PdfForms.Roads, PdfForms.ForPlot("WW-01", KpiTemplates.Streets));

            Assert.Null(PdfForms.ForPlot("WW-01", null));
            Assert.Equal(
                "no form is named for the plot prefix WW, so no PDF was written",
                PdfForms.NoFormFor("WW-01", null));
        }

        /// <summary>
        /// **The template wins over the prefix.** A DM plot whose component placed it in STREETS
        /// takes the Roads form, because the form follows the template.
        /// </summary>
        [Fact]
        public void TheTemplateWinsOverThePrefix()
        {
            Assert.Same(PdfForms.Roads, PdfForms.ForPlot("DM-11", KpiTemplates.Streets));
            Assert.Same(PdfForms.Parks, PdfForms.ForPlot("ST-05", KpiTemplates.ExistingParks));
        }

        /// <summary>
        /// A template offered on its layout is its kind's version and takes its kind's form,
        /// with no line of its own in the table.
        /// </summary>
        [Fact]
        public void ANewTemplateTakesItsKindsForm()
        {
            KpiTemplate offered = new KpiTemplate(
                "STREETS PEDESTRIAN",
                KpiTemplates.Streets.MainSheetName,
                KpiTemplates.Streets.Cells,
                new TreeSheet(KpiTemplates.ExistingTreesSheet),
                new TreeSheet(KpiTemplates.ProposedTreesSheet),
                KpiTemplates.Streets.GroupsCountedAsProposed,
                KpiTemplates.Streets);

            Assert.Same(PdfForms.Roads, PdfForms.ForTemplate(offered));
        }

        /// <summary>
        /// The whole plan for a walkway plot: Roads, wanted, and the road width written off the
        /// street reference answer.
        /// </summary>
        [Fact]
        public void AWalkwayPlotPlacedInStreetsGetsARoadsPdf()
        {
            PdfPlan plan = PdfFill.Of(
                CreateFixture.Plot("WW-01", component: "WALKWAY", uid2: "ANH-000-ST-100002"),
                KpiTemplates.Streets,
                CountedGroups.Of(KpiTemplates.Streets),
                StreetReferenceAnswer.Of(10.0, 30.0, "10", "30"),
                Today,
                true,
                PdfWorkbookNumbers.None);

            Assert.True(plan.Wanted, plan.Why);
            Assert.Same(PdfForms.Roads, plan.Form);
            Assert.Equal("10", plan.Fields.Single(one => one.Value == PdfValue.Row).Text);
            Assert.Equal("ANH-000-ST-100002", plan.Fields.Single(one => one.Value == PdfValue.Uid).Text);
        }

        /// <summary>
        /// **Project Type prints the component exactly as the model holds it**, WALKWAY as
        /// WALKWAY, on the one form that has the box. **The Roads form has no Project Type box**,
        /// measured off the client's Roads file on 29 October, 34 fields and none of them a type,
        /// so a walkway's Roads PDF carries no WALKWAY anywhere. That is Q21.
        /// </summary>
        [Fact]
        public void ProjectTypePrintsTheComponentAsHeldAndTheRoadsFormHasNoSuchBox()
        {
            PdfPlan open = PdfFill.Of(
                CreateFixture.Plot("DM-11", component: "WALKWAY"),
                null,
                CountedGroups.Of(KpiTemplates.Mosques),
                null,
                Today,
                true,
                PdfWorkbookNumbers.None);

            Assert.Equal("WALKWAY", open.Fields.Single(one => one.Value == PdfValue.ProjectType).Text);

            PdfPlan roads = PdfFill.Of(
                CreateFixture.Plot("WW-01", component: "WALKWAY"),
                KpiTemplates.Streets,
                CountedGroups.Of(KpiTemplates.Streets),
                null,
                Today,
                true,
                PdfWorkbookNumbers.None);

            Assert.False(PdfForms.Roads.Takes(PdfValue.ProjectType));
            Assert.DoesNotContain(roads.Fields, one => one.Value == PdfValue.ProjectType);
        }

        /// <summary>
        /// The pane's warning asks the same two rules the press does, so a walkway placed in
        /// STREETS is not named there as getting no PDF.
        /// </summary>
        [Fact]
        public void TheListWarningAgreesThatAWalkwayGetsAPdf()
        {
            Assert.Equal(string.Empty, TickingTheList.WouldWriteNothing("WW-01", one => "WALKWAY"));
            Assert.Equal(string.Empty, TickingTheList.WouldWriteNothing("ST-05", one => "STREET 30m ROW"));
        }
    }
}
