using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **WHAT STEP 4 SAYS BEFORE THE PRESS, COUNTED PER TEMPLATE.** Bader's layout of
    /// 17 September asks for what will be written above the rows that already say it per
    /// template, because reading a total off six rows by hand is what a count is for.
    ///
    /// **NOT ONE WORD OF `CreateWords.TemplateRow` MOVES.** This adds a line above the rows and
    /// hands the rows back through the method that has printed them since the round that cut
    /// them down to a line.
    ///
    /// **EVERY EXPECTED VALUE HERE IS WRITTEN OUT BY HAND.**
    /// </summary>
    public class KpiWillWriteTests
    {
        private static IReadOnlyList<string> Mosques(int howMany)
        {
            var plots = new List<string>();
            for (int at = 1; at <= howMany; at++)
            {
                plots.Add("DM-" + at.ToString("00", CultureInfo.InvariantCulture));
            }

            return plots;
        }

        private static IReadOnlyList<string> Streets(int howMany)
        {
            var plots = new List<string>();
            for (int at = 1; at <= howMany; at++)
            {
                plots.Add("ST-" + at.ToString("00", CultureInfo.InvariantCulture));
            }

            return plots;
        }

        /// <summary>
        /// The component a plot's sheet holds, keyed on its prefix the way the real model is.
        /// </summary>
        private static string ComponentOf(string plotId)
        {
            if (plotId.StartsWith("DM-", System.StringComparison.Ordinal)) return "DAILY MOSQUE";
            if (plotId.StartsWith("ST-", System.StringComparison.Ordinal)) return "STREET 36m ROW";
            return "NOTHING THE TABLE HOLDS";
        }

        /// <summary>
        /// **20 MOSQUE PLOTS AND 78 STREET PLOTS IS 98 WORKBOOKS FROM 2 TEMPLATES**, added by
        /// hand rather than worked out with the rule the code uses.
        /// </summary>
        [Fact]
        public void TwoTemplatesOverNinetyEightPlotsSayNinetyEightWorkbooks()
        {
            TemplateSplit split = PlotsPerTemplate.Split(
                Mosques(20).Concat(Streets(78)),
                ComponentOf,
                new[] { KpiTemplates.Mosques, KpiTemplates.Streets });

            Assert.Equal(
                "This press would write 98 workbooks from 2 workbook templates.",
                KpiWillWrite.InWords(split));
        }

        /// <summary>
        /// **ONE PLOT ON ONE TEMPLATE READS IN THE SINGULAR.** A line saying 1 workbooks from
        /// 1 workbook templates is a line nobody wrote by hand.
        /// </summary>
        [Fact]
        public void OnePlotOnOneTemplateReadsInTheSingular()
        {
            TemplateSplit split = PlotsPerTemplate.Split(
                new[] { "DM-11" }, ComponentOf, new[] { KpiTemplates.Mosques });

            Assert.Equal(
                "This press would write 1 workbook from 1 workbook template.",
                KpiWillWrite.InWords(split));
        }

        /// <summary>
        /// **A TICKED TEMPLATE NO PLOT BELONGS TO IS NOT COUNTED AS WRITING.** It stays ticked
        /// and stays listed, which is Bader's decision, and its row says why it writes nothing.
        /// Counting it would say three templates will write where two will.
        /// </summary>
        [Fact]
        public void ATemplateNoPlotBelongsToIsNotCountedAsWriting()
        {
            TemplateSplit split = PlotsPerTemplate.Split(
                Mosques(20).Concat(Streets(78)),
                ComponentOf,
                new[] { KpiTemplates.Mosques, KpiTemplates.Streets, KpiTemplates.Schools });

            Assert.Equal(3, split.Shares.Count);
            Assert.Equal(
                "This press would write 98 workbooks from 2 workbook templates.",
                KpiWillWrite.InWords(split));
        }

        /// <summary>
        /// **THE PLOTS THAT GO NOWHERE ARE IN THE SAME SENTENCE AS THE ONES THAT GO SOMEWHERE.**
        /// On the 16:06 press twelve ticked plots went into no workbook and nothing said so
        /// until a report twenty minutes later.
        /// </summary>
        [Fact]
        public void PlotsThatGoIntoNoWorkbookAreCountedInTheSameSentence()
        {
            TemplateSplit split = PlotsPerTemplate.Split(
                Mosques(20).Concat(new[] { "ZZ-01", "ZZ-02" }),
                ComponentOf,
                new[] { KpiTemplates.Mosques });

            Assert.Equal(2, split.Unplaced.Count);
            Assert.Equal(
                "This press would write 20 workbooks from 1 workbook template. "
                + "2 ticked plots named below go into no workbook at all.",
                KpiWillWrite.InWords(split));
        }

        /// <summary>
        /// **ONE PLOT GOING NOWHERE READS IN THE SINGULAR TOO.**
        /// </summary>
        [Fact]
        public void OnePlotGoingNowhereReadsInTheSingular()
        {
            TemplateSplit split = PlotsPerTemplate.Split(
                Mosques(20).Concat(new[] { "ZZ-01" }),
                ComponentOf,
                new[] { KpiTemplates.Mosques });

            Assert.Equal(
                "This press would write 20 workbooks from 1 workbook template. "
                + "1 ticked plot named below goes into no workbook at all.",
                KpiWillWrite.InWords(split));
        }

        /// <summary>
        /// **NO WORKBOOK TICKED SAYS SO**, rather than saying nought workbooks from nought
        /// templates, which reads as a press that would fail rather than one nobody set up.
        /// </summary>
        [Fact]
        public void NoWorkbookTickedSaysSo()
        {
            TemplateSplit split = PlotsPerTemplate.Split(
                Mosques(20), ComponentOf, new KpiTemplate[0]);

            Assert.Equal(
                "No workbook is ticked, so this press would write nothing.",
                KpiWillWrite.InWords(split));
        }

        /// <summary>
        /// **A TICKED TEMPLATE THAT NO TICKED PLOT REACHES SAYS THE OTHER THING.** A workbook is
        /// ticked and the press would still write nothing, which is a different state from
        /// having ticked nothing at all.
        /// </summary>
        [Fact]
        public void ATickedTemplateNoTickedPlotReachesSaysThePressWouldWriteNothing()
        {
            TemplateSplit split = PlotsPerTemplate.Split(
                Mosques(20), ComponentOf, new[] { KpiTemplates.Schools });

            Assert.Equal(
                "No ticked plot belongs to any ticked workbook, so this press would write "
                + "nothing.",
                KpiWillWrite.InWords(split));
        }

        /// <summary>
        /// **THE ROWS ARE `CreateWords.TemplateRow`'S OWN WORDS, UNCHANGED.** This hands them
        /// back in the split's order and writes none of them.
        /// </summary>
        [Fact]
        public void TheRowsAreTheWordsThePaneAlreadyPrints()
        {
            TemplateSplit split = PlotsPerTemplate.Split(
                Mosques(20).Concat(Streets(78)),
                ComponentOf,
                new[] { KpiTemplates.Mosques, KpiTemplates.Streets });

            IReadOnlyList<string> rows = KpiWillWrite.Rows(split);

            Assert.Equal(2, rows.Count);
            Assert.Equal("MOSQUES: 20 plots, DM-01 to DM-20. The report names them.", rows[0]);
            Assert.Equal("STREETS: 78 plots, ST-01 to ST-78. The report names them.", rows[1]);

            Assert.Equal(
                split.Shares.Select(CreateWords.TemplateRow).ToArray(),
                rows.ToArray());
        }
    }
}
