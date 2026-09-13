using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// The one builder of a view's name on a plot.
    ///
    /// It was written out by hand in four places, twice in the run plan, once in the schedule
    /// definition and once in the writer, and the writer's copy is what every created view is
    /// named by. These two came out of the preview's tests, which went with the preview.
    /// </summary>
    public class ViewNamingTests
    {
        private static readonly ViewType Overall = new ViewType("010", "Overall Plan");

        [Fact]
        public void AViewIsNamedThePlotThenTheType()
        {
            Assert.Equal("DM-11-(010) Overall Plan", ViewNaming.Of("DM-11", Overall));
            Assert.Equal("DM-11", ViewNaming.Of("DM-11", null));
            Assert.Equal("-(010) Overall Plan", ViewNaming.Of(null, Overall));
        }

        [Fact]
        public void TheNameTheRunPlanPrintsIsTheOneBuilderAndNotACopyOfIt()
        {
            RunPlan plan = RunFixture.Of(
                new[] { new PlotViewKey("DM-11", Overall) },
                new[] { "DM-11" },
                new[] { "DM-11" },
                null,
                null);

            Assert.Equal(
                ViewNaming.Of("DM-11", Overall),
                plan.Items.Single(one => one.Kind == RunItemKind.PlanView).Name);
        }
    }
}
