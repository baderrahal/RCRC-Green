using System;
using System.Collections.Generic;
using System.IO;
using RcrcGreen.Core.ViewFilters;
using Xunit;

namespace RcrcGreen.Core.Tests.ViewFilters
{
    /// <summary>
    /// ViewFilters.json in and out. The shipped file is read here through the same reader
    /// the pane uses, so a broken default fails this gate rather than somebody's install,
    /// the same rule the shipped title blocks follow.
    /// </summary>
    public class ViewFiltersFileTests
    {
        [Fact]
        public void TheShippedDefaultsReadAsTheFourRowsTheRoundAskedFor()
        {
            ViewFiltersFileRead read = ViewFiltersFile.Read(File.ReadAllText(ShippedFile()));

            Assert.True(read.WasRead, read.Problem);
            Assert.Equal(4, read.Rows.Count);

            Assert.Equal("(200-260) Presentation", read.Rows[0].Prefix);
            Assert.True(read.Rows[0].Enabled);
            Assert.True(read.Rows[0].Visible);
            Assert.Equal("#FF0000", read.Rows[0].LineColor);
            Assert.Equal("#00FF00", read.Rows[0].ForegroundPatternColor);
            Assert.Equal("#00FF00", read.Rows[0].BackgroundPatternColor);

            Assert.Equal("(200-260) Sections Scope", read.Rows[1].Prefix);
            Assert.False(read.Rows[1].Visible);
            Assert.Equal("#0000FF", read.Rows[1].LineColor);
            Assert.Equal("#FFFF00", read.Rows[1].ForegroundPatternColor);

            Assert.Equal("(200-260) Intervention Limit", read.Rows[2].Prefix);
            Assert.Equal("#FF9900", read.Rows[2].LineColor);

            Assert.Equal("(215) Borders Edging", read.Rows[3].Prefix);
            Assert.Equal("#888888", read.Rows[3].LineColor);

            foreach (FilterConfig row in read.Rows)
            {
                Assert.True(row.Enabled);
                Assert.False(row.OverrideLineColor);
                Assert.False(row.OverrideForegroundPattern);
                Assert.False(row.OverrideBackgroundPattern);
                Assert.Equal(0, row.LineWeight);
                Assert.Equal("none", row.ForegroundPatternType);
                Assert.Equal("none", row.BackgroundPatternType);
                Assert.False(row.Halftone);
            }
        }

        [Fact]
        public void TheRowsSurviveARoundTrip()
        {
            List<FilterConfig> rows = new List<FilterConfig>
            {
                new FilterConfig
                {
                    Prefix = "(215) Borders Edging",
                    Enabled = true,
                    Visible = false,
                    OverrideLineColor = true,
                    LineColor = "#888888",
                    LineWeight = 3,
                    OverrideForegroundPattern = true,
                    ForegroundPatternType = "solid",
                    ForegroundPatternColor = "#888888",
                    OverrideBackgroundPattern = false,
                    BackgroundPatternType = "none",
                    BackgroundPatternColor = "",
                    Halftone = true
                }
            };

            ViewFiltersFileRead back = ViewFiltersFile.Read(ViewFiltersFile.Written(rows));

            Assert.True(back.WasRead, back.Problem);
            Assert.Single(back.Rows);
            Assert.Equal("(215) Borders Edging", back.Rows[0].Prefix);
            Assert.False(back.Rows[0].Visible);
            Assert.True(back.Rows[0].OverrideLineColor);
            Assert.Equal(3, back.Rows[0].LineWeight);
            Assert.Equal("solid", back.Rows[0].ForegroundPatternType);
            Assert.True(back.Rows[0].Halftone);
        }

        /// <summary>
        /// A hand edited file need not keep the properties in any order, so the reader is
        /// held to a shuffled one. If this goes red the reader cannot be trusted with hand
        /// edits and has to change, not the test.
        /// </summary>
        [Fact]
        public void PropertiesInAnotherOrderStillRead()
        {
            string shuffled = "[{\"Halftone\":true,\"Prefix\":\"(215) Borders Edging\","
                + "\"LineWeight\":2,\"Enabled\":true,\"Visible\":false,"
                + "\"OverrideLineColor\":true,\"LineColor\":\"#888888\","
                + "\"BackgroundPatternColor\":\"#888888\",\"BackgroundPatternType\":\"solid\","
                + "\"OverrideBackgroundPattern\":true,\"ForegroundPatternColor\":\"#888888\","
                + "\"ForegroundPatternType\":\"none\",\"OverrideForegroundPattern\":false}]";

            ViewFiltersFileRead read = ViewFiltersFile.Read(shuffled);

            Assert.True(read.WasRead, read.Problem);
            Assert.Single(read.Rows);
            Assert.Equal("(215) Borders Edging", read.Rows[0].Prefix);
            Assert.True(read.Rows[0].Halftone);
            Assert.Equal(2, read.Rows[0].LineWeight);
            Assert.False(read.Rows[0].Visible);
            Assert.True(read.Rows[0].OverrideBackgroundPattern);
            Assert.Equal("solid", read.Rows[0].BackgroundPatternType);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void AnEmptyFileIsARefusalWithTheReason(string json)
        {
            ViewFiltersFileRead read = ViewFiltersFile.Read(json);

            Assert.False(read.WasRead);
            Assert.Empty(read.Rows);
            Assert.Equal("The file is empty.", read.Problem);
        }

        [Fact]
        public void TextThatIsNotJsonIsARefusalWithTheReason()
        {
            ViewFiltersFileRead read = ViewFiltersFile.Read("not json at all");

            Assert.False(read.WasRead);
            Assert.Empty(read.Rows);
            Assert.StartsWith("The file does not read as filter rows.", read.Problem, StringComparison.Ordinal);
        }

        /// <summary>
        /// The same walk the title block settings test uses, up from the test assembly to
        /// the repo root, so the test reads the file the install really copies.
        /// </summary>
        private static string ShippedFile()
        {
            DirectoryInfo at = new DirectoryInfo(AppContext.BaseDirectory);

            while (at != null)
            {
                string here = Path.Combine(at.FullName, "install", "ViewFilters.json");
                if (File.Exists(here)) return here;
                at = at.Parent;
            }

            throw new FileNotFoundException(
                "install/ViewFilters.json was not found above " + AppContext.BaseDirectory
                + ". It is the shipped default rows and install.ps1 copies it beside the add-in.");
        }
    }
}
