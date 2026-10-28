using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security;
using System.Text;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// The layout rule and the new template review, on workbooks built here by hand. No client
    /// workbook enters this public repository, so each one holds the five sheet names and a few
    /// labels a test needs and nothing of the client's.
    /// </summary>
    public class NewTemplatesTests
    {
        private static readonly string[] MosqueSheets = { "<Mosques>", "Tree List - Existing", "Tree List - Proposed", "Criteria", "Green Strategy KPI's" };

        private static readonly string[] SchoolSheets = { "<Schools>", "Tree List - Existing", "Tree List - Proposed", "Criteria", "Green Strategy KPI's" };

        /// <summary>
        /// The labels of a built first sheet: shared strings, an inline string, a formula with a
        /// text result and a number. Only the typed text is a label.
        /// </summary>
        private static Cell[] Labels(string area = "Total Green cover (m²)")
        {
            return new[]
            {
                Cell.Shared("B3", "Component"),
                Cell.Shared("B7", " " + area),
                Cell.Inline("G5", "Date:"),
                Cell.Formula("H31", "'Green Strategy KPI''s'!E34", "0.13"),
                Cell.Number("D20", "12")
            };
        }

        [Fact]
        public void APeekReadsOnlyTypedTextAsLabels()
        {
            string folder = Folder();
            string path = Built(folder, "a.xlsx", MosqueSheets, Labels());

            PeekedWorkbook peeked = PeekedWorkbook.Of(path);

            Assert.True(peeked.WasRead, peeked.Refusal);
            Assert.Equal(new[] { "B3", "B7", "G5" }, peeked.FirstSheetLabels.Keys.OrderBy(one => one).ToArray());
            Assert.Equal(" Total Green cover (m²)", peeked.FirstSheetLabels["B7"]);
            Assert.Equal("Date:", peeked.FirstSheetLabels["G5"]);
        }

        /// <summary>
        /// The 60 per cent shape: the same labels, a KPI target formula reading another row and
        /// another cached value. That is the same layout, because a formula is not a label.
        /// </summary>
        [Fact]
        public void AnotherTargetRowIsTheSameLayoutAndAnotherLabelIsNot()
        {
            string folder = Folder();
            TemplateLayout mosques = PeekedWorkbook.Of(Built(folder, "m.xlsx", MosqueSheets, Labels())).Layout;

            Cell[] otherRow = Labels().Select(one => one.At == "H31" ? Cell.Formula("H31", "'Green Strategy KPI''s'!E33", "0.08") : one).ToArray();
            TemplateLayout moreThan = PeekedWorkbook.Of(Built(folder, "m60.xlsx", MosqueSheets, otherRow)).Layout;

            // Edge whitespace is the client's own, so it comes off both sides and nothing else.
            Cell[] trimmed = Labels("Total Green cover (m²)  ").ToArray();
            TemplateLayout edges = PeekedWorkbook.Of(Built(folder, "e.xlsx", MosqueSheets, trimmed)).Layout;

            TemplateLayout reworded = PeekedWorkbook.Of(Built(folder, "r.xlsx", MosqueSheets, Labels("Total  Green cover (m²)"))).Layout;
            TemplateLayout schools = PeekedWorkbook.Of(Built(folder, "s.xlsx", SchoolSheets, Labels())).Layout;

            Assert.True(mosques.Same(moreThan));
            Assert.True(mosques.Same(edges));
            Assert.False(mosques.Same(reworded));
            Assert.Equal("<Mosques> B7 reads 'Total Green cover (m²)' and the other's reads 'Total  Green cover (m²)'", mosques.Difference(reworded));
            Assert.False(mosques.Same(schools));
            Assert.StartsWith("its sheets are <Mosques>,", mosques.Difference(schools));
            Assert.False(TemplateLayout.Nothing.Same(TemplateLayout.Nothing));
        }

        [Fact]
        public void AFileNamedForNoTemplateWithAKnownLayoutIsANewTemplateNamedOffItsFile()
        {
            string folder = Folder();
            IReadOnlyList<RecognisedWorkbook> listed = Listed(folder,
                ("GRP_-_KPI_Checklist_-_DD_MOSQUES.xlsx", MosqueSheets, Labels()),
                ("GRP_-_KPI_Checklist_-_DD_MOSQUES MORE THAN 60.xlsx", MosqueSheets, Labels()),
                ("GRP_-_KPI_Checklist_-_DD_SCHOOLS.xlsx", SchoolSheets, Labels()),
                ("GRP_-_KPI_Checklist_-_DD_MOSQUES MORE THAN 80.xlsx", MosqueSheets, Labels()));

            var made = new Dictionary<string, KpiTemplate>();
            IReadOnlyList<RecognisedWorkbook> reviewed = NewTemplates.Review(listed, null, made);

            Assert.Equal("MOSQUES", reviewed[0].Template.Name);
            Assert.Equal("MOSQUES MORE THAN 60", reviewed[1].Template.Name);
            Assert.Equal("SCHOOLS", reviewed[2].Template.Name);

            RecognisedWorkbook added = reviewed[3];
            Assert.True(added.IsNewTemplate);
            Assert.False(added.IsMatched);
            Assert.Equal("MOSQUES MORE THAN 80", added.Proposed.Name);
            Assert.Same(KpiTemplates.Mosques, added.SameAs);
            Assert.Same(KpiTemplates.Mosques, added.Proposed.Kind);
            Assert.Equal("<Mosques>", added.Proposed.MainSheetName);
            Assert.Equal(KpiTemplates.Mosques.Cells.Select(one => one.Value + one.Cell), added.Proposed.Cells.Select(one => one.Value + one.Cell));
            Assert.StartsWith("New template, same layout as MOSQUES. It would be filled as MOSQUES MORE THAN 80", added.InWords);
            Assert.Equal("Mosques >80%", AssetTypeNames.Display(added.Proposed));

            // A redraw hands back the very template the first review made, so a tick holds.
            Assert.Same(added.Proposed, NewTemplates.Review(listed, null, made)[3].Proposed);
        }

        [Fact]
        public void UseMatchesItAndItSplitsLikeAnyOtherTemplate()
        {
            string folder = Folder();
            IReadOnlyList<RecognisedWorkbook> listed = Listed(folder,
                ("GRP_-_KPI_Checklist_-_DD_MOSQUES.xlsx", MosqueSheets, Labels()),
                ("GRP_-_KPI_Checklist_-_DD_MOSQUES MORE THAN 80.xlsx", MosqueSheets, Labels()));

            var made = new Dictionary<string, KpiTemplate>();
            RecognisedWorkbook used = NewTemplates.Review(listed, new[] { "mosques  more than 80" }, made)[1];

            Assert.True(used.IsMatched);
            Assert.Equal("MOSQUES MORE THAN 80", used.Template.Name);

            ComponentTable table = ComponentTable.Layered(
                KpiTemplates.All.Concat(new[] { used.Template }).ToList(),
                ComponentFile.Parse(ComponentSource.Shared, "s.csv", new[]
                {
                    "Component,Template,Folder", "DAILY MOSQUE MORE THAN 80%,MOSQUES MORE THAN 80,DAILY MOSQUE MORE THAN 80%"
                }));

            Assert.Same(used.Template, table.TemplateFor("DAILY MOSQUE MORE THAN 80 %"));
            Assert.Equal("DAILY MOSQUE MORE THAN 80%", table.FolderFor("DAILY MOSQUE MORE THAN 80%"));

            TemplateSplit split = PlotsPerTemplate.Split(new[] { "DM-41" }, plot => "DAILY MOSQUE", new[] { used.Template, KpiTemplates.Mosques });
            Assert.Equal(new[] { "MOSQUES", "MOSQUES MORE THAN 80" }, split.Shares.Select(one => one.Template.Name).ToArray());
        }

        [Fact]
        public void ALayoutMatchingNothingIsUnknownAndNotUsed()
        {
            string folder = Folder();
            IReadOnlyList<RecognisedWorkbook> listed = Listed(folder,
                ("GRP_-_KPI_Checklist_-_DD_MOSQUES.xlsx", MosqueSheets, Labels()),
                ("GRP_-_KPI_Checklist_-_DD_MOSQUES REVISED.xlsx", MosqueSheets, Labels("Green cover")),
                ("Numbers.xlsx", new[] { "Sheet1" }, new Cell[0]));

            IReadOnlyList<RecognisedWorkbook> reviewed = NewTemplates.Review(listed, null, new Dictionary<string, KpiTemplate>());

            Assert.True(reviewed[1].UnknownLayout);
            Assert.False(reviewed[1].IsMatched);
            Assert.False(reviewed[1].NeedsAPick);
            Assert.Equal(
                "Unknown template layout. Its name names no template this tool knows and its layout matches none in "
                + "this folder. Against GRP_-_KPI_Checklist_-_DD_MOSQUES.xlsx, <Mosques> B7 reads 'Green cover' and the "
                + "other's reads 'Total Green cover (m²)'.",
                reviewed[1].InWords);
            Assert.True(reviewed[2].UnknownLayout);
        }

        /// <summary>
        /// The names the client gives all eleven end with the template's own name, so none of
        /// them is ever asked about. A file named for nothing is, even on a known first sheet.
        /// </summary>
        [Theory]
        [InlineData("GRP_-_KPI_Checklist_-_DD_MOSQUES.xlsx", true)]
        [InlineData("GRP_-_KPI_Checklist_-_DD_MOSQUES LESS THAN 60.xlsx", true)]
        [InlineData("GRP_-_KPI_Checklist_-_DD_MOSQUES LESSTHAN 60.xlsx", true)]
        [InlineData("mosques.xlsx", true)]
        [InlineData("GRP_-_KPI_Checklist_-_DD_MOSQUES MORE THAN 80.xlsx", false)]
        [InlineData("book.xlsx", false)]
        public void AFileIsKnownByNameWhenItsNameEndsWithItsTemplate(string fileName, bool known)
        {
            RecognisedWorkbook found = RecognisedWorkbook.Recognise(fileName, MosqueSheets.ToList(), null);

            Assert.Equal(known, NewTemplates.KnownByName(found));
        }

        [Fact]
        public void TheOpeningEveryKnownFileSharesComesOffTheNewName()
        {
            Assert.Equal("HEALTH MORE THAN 60", NewTemplates.NameFor("GRP_-_KPI_Checklist_-_DD_HEALTH MORE THAN 60.xlsx", "GRP KPI CHECKLIST DD"));
            Assert.Equal("OTHER NAME", NewTemplates.NameFor("other name.xlsx", "GRP KPI CHECKLIST DD"));
            Assert.Equal("GRP KPI CHECKLIST DD X", NewTemplates.NameFor("GRP KPI Checklist DD X.xlsx", string.Empty));
        }

        private static IReadOnlyList<RecognisedWorkbook> Listed(string folder, params (string name, string[] sheets, Cell[] cells)[] files)
        {
            return files.Select(file =>
            {
                PeekedWorkbook peeked = PeekedWorkbook.Of(Built(folder, file.name, file.sheets, file.cells));
                return RecognisedWorkbook.Recognise(file.name, peeked.SheetNames, peeked.Refusal, peeked.FirstSheetCells).Laid(peeked.Layout);
            }).ToList();
        }

        internal static string Folder()
        {
            string folder = Path.Combine(Path.GetTempPath(), "rcrc-new-templates-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            return folder;
        }

        /// <summary>One cell of a built first sheet.</summary>
        internal sealed class Cell
        {
            private Cell(string at, string kind, string text, string formula)
            {
                At = at;
                Kind = kind;
                Text = text;
                FormulaText = formula;
            }

            public string At { get; }

            public string Kind { get; }

            public string Text { get; }

            public string FormulaText { get; }

            public static Cell Shared(string at, string text) { return new Cell(at, "s", text, null); }

            public static Cell Inline(string at, string text) { return new Cell(at, "inlineStr", text, null); }

            public static Cell Formula(string at, string formula, string cached) { return new Cell(at, "n", cached, formula); }

            public static Cell Number(string at, string value) { return new Cell(at, "n", value, null); }
        }

        /// <summary>
        /// A workbook zip with the sheets named, the first holding the cells given and the others
        /// empty, wired through the relationships the way Excel wires them.
        /// </summary>
        internal static string Built(string folder, string fileName, IReadOnlyList<string> sheets, IReadOnlyList<Cell> cells)
        {
            string path = Path.Combine(folder, fileName);
            const string Main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            const string Rel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

            List<string> shared = cells.Where(one => one.Kind == "s").Select(one => one.Text).Distinct().ToList();

            using (var zip = ZipFile.Open(path, ZipArchiveMode.Create))
            {
                Entry(zip, "_rels/.rels",
                    "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
                    + "<Relationship Id=\"rId1\" Type=\"" + Rel + "/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>");

                var book = new StringBuilder("<workbook xmlns=\"" + Main + "\" xmlns:r=\"" + Rel + "\"><sheets>");
                var rels = new StringBuilder("<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");
                for (int at = 0; at < sheets.Count; at++)
                {
                    book.Append("<sheet name=\"" + SecurityElement.Escape(sheets[at]) + "\" sheetId=\"" + (at + 1) + "\" r:id=\"rId" + (at + 1) + "\"/>");
                    rels.Append("<Relationship Id=\"rId" + (at + 1) + "\" Type=\"" + Rel + "/worksheet\" Target=\"worksheets/sheet" + (at + 1) + ".xml\"/>");
                }
                rels.Append("<Relationship Id=\"rIdS\" Type=\"" + Rel + "/sharedStrings\" Target=\"sharedStrings.xml\"/></Relationships>");
                Entry(zip, "xl/workbook.xml", book + "</sheets></workbook>");
                Entry(zip, "xl/_rels/workbook.xml.rels", rels.ToString());

                Entry(zip, "xl/sharedStrings.xml", "<sst xmlns=\"" + Main + "\">"
                    + string.Concat(shared.Select(one => "<si><t xml:space=\"preserve\">" + SecurityElement.Escape(one) + "</t></si>")) + "</sst>");

                for (int at = 0; at < sheets.Count; at++)
                {
                    var data = new StringBuilder("<worksheet xmlns=\"" + Main + "\"><sheetData>");
                    if (at == 0)
                    {
                        foreach (Cell cell in cells)
                        {
                            if (cell.Kind == "s") data.Append("<row><c r=\"" + cell.At + "\" t=\"s\"><v>" + shared.IndexOf(cell.Text) + "</v></c></row>");
                            else if (cell.Kind == "inlineStr") data.Append("<row><c r=\"" + cell.At + "\" t=\"inlineStr\"><is><t>" + SecurityElement.Escape(cell.Text) + "</t></is></c></row>");
                            else if (cell.FormulaText != null) data.Append("<row><c r=\"" + cell.At + "\"><f>" + SecurityElement.Escape(cell.FormulaText) + "</f><v>" + cell.Text + "</v></c></row>");
                            else data.Append("<row><c r=\"" + cell.At + "\"><v>" + cell.Text + "</v></c></row>");
                        }
                    }
                    Entry(zip, "xl/worksheets/sheet" + (at + 1) + ".xml", data + "</sheetData></worksheet>");
                }
            }

            return path;
        }

        private static void Entry(ZipArchive zip, string name, string xml)
        {
            using (var writer = new StreamWriter(zip.CreateEntry(name).Open(), new UTF8Encoding(false)))
            {
                writer.Write(xml);
            }
        }
    }

    /// <summary>
    /// An unknown value, its plots and its suggestion, and the line it writes.
    /// </summary>
    public class UnknownComponentsTests
    {
        [Fact]
        public void EachUnknownValueIsOneBoxWithItsPlotsAndASuggestion()
        {
            var components = new Dictionary<string, string>
            {
                { "DM-50", "DAILY MOSQUE MORE THAN 80%" },
                { "DM-51", "daily mosque  more than 80 %" },
                { "SC-09", "SCHOOL MORE THAN 80%" },
                { "PS-01", "PUMP STATION" },
                { "DM-12", "DAILY MOSQUE" },
                { "EP-05", "" }
            };

            IReadOnlyList<UnknownComponent> unknown = UnknownComponents.In(
                components.Keys, plot => components[plot], ComponentTable.Shipped, KpiTemplates.All);

            Assert.Equal(new[] { "DAILY MOSQUE MORE THAN 80%", "SCHOOL MORE THAN 80%", "PUMP STATION" }, unknown.Select(one => one.Value).ToArray());
            Assert.Equal(new[] { "DM-50", "DM-51" }, unknown[0].Plots.ToArray());
            Assert.Equal("MOSQUES MORE THAN 60", unknown[0].Suggested.Name);
            Assert.Equal("DAILY MOSQUE MORE THAN 80%", unknown[0].Folder);
            Assert.Equal("SCHOOLS MORE THAN 60", unknown[1].Suggested.Name);
            Assert.Null(unknown[2].Suggested);
        }

        /// <summary>
        /// Most shared words, a plural S and a per cent sign off each word, the first listed on a
        /// tie. PARK is a word of no template, PARKS is, and PARKING is its own.
        /// </summary>
        [Theory]
        [InlineData("SCHOOL LESS THAN 70%", "SCHOOLS LESS THAN 60")]
        [InlineData("FRIDAY MOSQUE MORE THAN 60%", "MOSQUES MORE THAN 60")]
        [InlineData("NEW PARK", "EXISTING PARKS")]
        [InlineData("PARKING AREA", "PARKING")]
        [InlineData("HEALTHCARE CLINIC", "HEALTHCARE")]
        [InlineData("STREET 40m ROW", "STREETS")]
        [InlineData("GOVERMENT BUILDING", null)]
        public void TheSuggestionSharesTheMostWords(string value, string template)
        {
            KpiTemplate suggested = UnknownComponents.Suggest(value, KpiTemplates.All);

            Assert.Equal(template, suggested == null ? null : suggested.Name);
        }

        [Fact]
        public void TheBoxesSayWhatTheyAreAbout()
        {
            Assert.Equal("New template, same layout as MOSQUES", KpiPaneWords.NewTemplateSameAs(KpiTemplates.Mosques));
            Assert.Equal("Unknown value PUMP STATION", KpiPaneWords.UnknownValue("PUMP STATION"));
            Assert.Equal("Suggested: Schools >60%", KpiPaneWords.Suggested(KpiTemplates.SchoolsMoreThan60));
            Assert.Equal("3 plots carry it", KpiPaneWords.PlotsCarryIt(3));
            Assert.Equal("The shared components.csv was not read",
                KpiPaneWords.ComponentFileSays(ComponentFile.Unreadable(ComponentSource.Shared, "s.csv", "x")));
            Assert.Equal("Your own components.csv has lines not taken",
                KpiPaneWords.ComponentFileSays(ComponentFile.Parse(ComponentSource.Personal, "p.csv", new[] { "Component,Template,Folder", "A,B" })));
        }

        [Fact]
        public void TheFolderIsTheValueWithItsSpacesTidied()
        {
            Assert.Equal("NEW PARK 2", UnknownComponents.FolderFor("  NEW   PARK 2 "));
        }

        [Fact]
        public void TheLineAppendedStartsAFileWithItsHeaderAndNeverJoinsTheLastLine()
        {
            Assert.Equal("Component,Template,Folder\r\nX,PARKING,X\r\n", ComponentTable.TextToAppend(null, "X,PARKING,X"));
            Assert.Equal("Component,Template,Folder\r\nX,PARKING,X\r\n", ComponentTable.TextToAppend("  ", "X,PARKING,X"));
            Assert.Equal("\r\nX,PARKING,X\r\n", ComponentTable.TextToAppend("Component,Template,Folder\r\nA,B,C", "X,PARKING,X"));
            Assert.Equal("X,PARKING,X\r\n", ComponentTable.TextToAppend("Component,Template,Folder\r\nA,B,C\r\n", "X,PARKING,X"));

            ComponentFile after = ComponentFile.Parse(ComponentSource.Shared, "s.csv",
                ("Component,Template,Folder\r\nA,PARKING,C" + ComponentTable.TextToAppend("Component,Template,Folder\r\nA,PARKING,C", "X,PARKING,X"))
                    .Split(new[] { "\r\n" }, StringSplitOptions.None));
            Assert.Equal(new[] { "A", "X" }, after.Rows.Select(one => one.Component).ToArray());
        }
    }
}
