# Audit 4, the KPI tool, 2026-09-15. A software firm's review

Main at `f2e2f42`. **1939 tests, 1119 of them KPI, 28 hook cases**, which is the snapshot every
line number and count below describes. This audit builds nothing and fixes nothing. The files
it writes are this one, `steps/ai-max-state-kpi.md`, which `require-file-on-commit.sh` refuses
a commit without, and one entry in `steps/log-kpi.md`, which is the standing rule and is what
audit 3 did. **No code, no test, no rules file and no hook was changed**, and `git status` was
clean before and after every measurement.

**Read first: `steps/audit-kpi.md`, `steps/audit-kpi-2.md` and `steps/audit-kpi-3.md`. 63
numbered findings, 19 carrying a FIXED mark, 44 open**, derived by walking the three files
rather than off a note: 29 numbered with 8 FIXED, 20 numbered with 9 FIXED, 14 numbered with 2
FIXED. **Nothing they report is reported again here.** Where a thing below has one of their
numbers, that number is cited in one line and the new half is what is written.

The three earlier audits read the code for faults. This one reads the project the way a firm
reads a codebase before taking it over: could a new engineer own it, what does it depend on,
what happens when it breaks, and is the debt growing or shrinking. **Seventeen findings, 64 to
80. Nine were dropped for having no cost to the user** and are named at the end. The cap of 40
was never approached.

Two deliberate changes were made to measure what the gate tells a new engineer. **Both were
made in a copy of the tip under the session scratchpad and never in this repository**, so
nothing here was edited even for a moment.

---

## THE FIRM'S VERDICT

**I would take it on, at a price, and I would put the first week into two things that have
nothing to do with features.**

What is good here is rare and it is not cosmetic. The product has **zero third party runtime
dependencies**: the shipped add-in is two assemblies, `install.ps1:54`, and every package in
the solution is reference only or test only. The Core half carries **no reference to the Revit
API at all**, proved three ways, so 1,939 tests run on a machine with no Revit on it and the
gate is real rather than decorative. The data tables that decide what goes into a client's
workbook are bound to each other by tests: I added an eighth template to a copy of the tip and
the suite told me, in two rounds, every other table I had to fill in. And the reasoning behind
the hard calls is written down where the call was made. That is a codebase somebody has been
thinking about, not one that was generated and shipped.

What I would charge extra for is the reading cost. `KpiCreateReport.cs` is 1,711 lines,
`KpiPanel.cs` is 1,627, and `src/RcrcGreen.Core/Kpi` is 96 files in one flat folder. More to
the point, the documentation that exists to orient a new reader is a whole subsystem behind the
code: **`CLAUDE.md` contains the string PDF zero times**, and the tool fills a client AcroForm
for every plot out of 2,149 lines across seven files. A new engineer is told the tool writes a
workbook and a report. It also writes a client form, and they would find that out by reading
`PdfChecklist.cs`.

**What I would fix first is not on the feature list. It is that the client's project name,
their consultant and a real contract reference are constants in a public repository.** The
`.gitignore` says in its own words that no client PDF may enter because the forms carry exactly
those facts. Three of them are in a tracked `.cs` file. The engineering reason for holding them
is sound and I would keep the rule, in a file beside the installed assembly rather than in the
repository. That is half a day and it is the half day I would do before anything else.

**Second is the error surface: 88 catch blocks, and the ones that matter answer a failure with
a value that reads like an answer.** A throw while reading a schedule's plot filter comes back
as an empty string and an empty string means the schedule belongs to no plot, so a plot's trees
go missing from a client workbook and nothing anywhere says a read threw. The per plot write
loop has no guard, so a throw on plot 100 of 156 ends the press with 99 workbooks and 99 PDFs
already in the client's folder tree and **no report written at all**. Three of the five readers
that open an .xlsx do not catch `XmlException` and the two that do are the two that write, so
one malformed sheet part in a template takes the whole press down with no file named.

**What would make me walk away is none of the above.** The debt numbers are honest: since the
first audit the KPI code has grown 126 per cent and its tests 161 per cent, so **test density
went up, not down**, which is the opposite of what the research on AI assisted codebases
predicts and it is measured rather than claimed. What would make me walk away is the other
number: **in 42 rounds since the first audit, six closed an audit finding**. Forty four
findings stand, twenty one of them opened five days ago, and the rounds keep arriving. A team
that files faster than it fixes is choosing that, and it is a choice a new owner inherits with
no say in it. I would take the work on the condition that the next three rounds close findings
and add nothing.

---

## Ranked findings

`AREA | file and line | what is wrong | why it matters | cost to fix`

### BLOCKS

**One. It blocks the handover rather than the run, and it is here because a partner reads the
top of the list first.**

64. SECRETS | `src/RcrcGreen.Core/Kpi/PdfForms.cs:224`, `:226` and `:235`, against
    `.gitignore:46-49` | **The client's project name, their consultant and a real contract
    reference are constants in a public repository.** `ProjectName` is
    `Neighborhood Landscape Design - Zone #2`, `ConsultantName` is `SAPL`, `ContractReference`
    is `GP.NH.Z2.052-DES042`. Counted over every tracked file: 5, 4 and 11 occurrences |
    **The ignore file says in its own words why no client PDF may enter: "the forms carry the
    client's branding, the project name, the consultant and a real contract reference, and this
    repository is public". Three of those four are in the repository.** The control and the code
    disagree, and the control is the one the round message stated as a constraint. The
    engineering reason for holding them is good and is written at `:206-222`: a field is
    identified by the value the CLIENT'S OWN template carries, compared whole, and
    `PdfFormCheck.cs:154` refuses a form that does not carry all three, so a form whose header
    has changed writes nothing rather than writing into the wrong box. Nothing about that
    reasoning requires the strings to be in a public repository | The three values move into a
    file beside the installed assembly, read the way `install/ViewFilters.json` and the two
    folder pointers already are, with a shipped file the gate reads. Half a day and one test.
    **The decision is Bader's, because it trades a measured rule for a file the team has to
    keep**

### WRONG

65. LOGIC | `src/RcrcGreen.Revit/Kpi/KpiPlotReader.cs:434-444`, read at `:98` and `:276` |
    **A throw while reading a schedule's plot filter is answered with an empty string, and an
    empty string means the schedule belongs to no plot.** `PlotFilteredOn` catches
    `ApplicationException` and `InvalidOperationException` and returns `string.Empty` from both,
    recording nothing anywhere | Three things follow and none of them is visible. At `:276` the
    schedule is skipped, so the plot reads as holding no softscape and no shrubs and lawn
    schedule, **and the guard that refuses a plot holding two of a kind cannot fire either,
    because the schedule was never counted.** The reconciliation then says the schedule listed
    no species, which is a sentence about the model, and the workbook is written with that
    plot's trees missing. At `:98` the plot drops out of the schedule half of the plot list,
    which is one of the two lines the pane prints as a disagreement between the sheets and the
    schedules, so the disagreement itself becomes wrong. **This is finding 6's shape one step
    earlier**: 6 is about the rows of a schedule that was found, this is about a schedule that
    was never found, and the newer guards do not reach it | A refusal carried on the reading,
    the way `GuardedRead` at `KpiRequestHandler.cs:870` already carries one, plus the schedule's
    name

    FIXED, in the ninety second pass. `PlotFilteredOn` hands back a `SchedulePlotRead` rather
    than a string, so a throw carries on the reading as a refusal naming the schedule, the
    exception's type and its message, the way `GuardedRead` beside it already does. The plot's
    files are not written off a half read, READY reads NO with the schedule's name, and the
    schedule half of the plot list NAMES what it could not read rather than dropping it.
    **THE RULE AND ITS WORDS LIVE IN `Core/Kpi/SchedulePlotRead.cs`, where a test can reach
    them**, because the throw itself is Revit's. `SchedulePlotReadTests` is five cases, among
    them that a refusal belongs to no plot and is not an empty value, and that a refusal with no
    reason is refused outright. **WHAT STAYS UNTESTED IS THAT THE TWO CATCHES IN
    `KpiPlotReader.PlotFilteredOn` REALLY PRODUCE THIS REFUSAL**, because nothing in this
    repository can make Revit's own `ScheduleDefinition` throw. One press on a model holding a
    schedule whose definition Revit refuses is what would settle it.

66. WIRING | `src/RcrcGreen.Revit/Kpi/KpiRequestHandler.cs:528-533` with `:425` | **The per plot
    WRITE loop has no guard, so a throw on plot 100 of 156 ends the press with no report at
    all.** `foreach (PlotReading one in readings) OnePlot(...)` runs under nothing, and
    `ReportFile.Write` sits at `:425`, after every template's loop has finished | Finding 35 put
    `GuardedRead` around each plot's READ and it is fixed. **The write loop is a different loop,
    added when a workbook became one plot, and it has no equivalent.** What can throw inside
    `OnePlot`: `CellWrite.Number` throws `ArgumentOutOfRangeException` on a NaN, which is
    finding 17's still open path through `KpiMerge.Area`, and `CellRef.Parse` throws
    `ArgumentException` on a reference it cannot read. Either unwinds out of `OneTemplate` and
    out of `Create` to `Run`'s catches at `:183-199`, which say "Revit refused that as a bad
    argument" with no plot named. By then **99 workbooks and 99 PDFs are already in the client's
    folder tree and the only record of which plots those were is never written.** The 156 plot
    run is the run the team makes | A try around the `OnePlot` call recording a
    `PlotOutcome.WroteNothing`, which is the shape `GuardedRead` already uses

    FIXED, in the eighty eighth pass. `GuardedWrite` in `KpiRequestHandler` wraps the `OnePlot`
    call and catches every exception type on purpose, the same as `GuardedRead` beside it. The
    plot goes into `plotOutcomes` as a `PlotOutcome.WroteNothing` carrying the exception's type
    and message, the reason joins the template row's own list, the run carries on to the rest and
    the report is written either way. `PlotReadThrewTests` covers the read half and nothing here
    can be run in Revit, so the write half is not observed there.

67. WIRING | `src/RcrcGreen.Core/Kpi/SpeciesList.cs:354-365`,
    `src/RcrcGreen.Core/Kpi/LabelledCells.cs:367-378` and
    `src/RcrcGreen.Core/Kpi/StreetReference.cs:307-318`, against
    `src/RcrcGreen.Core/Kpi/WorkbookPatcher.cs:159-174` and
    `src/RcrcGreen.Core/Kpi/WorkbookPeek.cs:78-93` | **Five classes open an .xlsx. Two catch
    `System.Xml.XmlException` and three do not, and the two that do are the two that WRITE.**
    The three that only read catch `IOException`, `UnauthorizedAccessException` and
    `InvalidDataException` and stop there | Traced end to end. A template whose `xl/workbook.xml`
    is fine and whose `Tree List - Existing` sheet part is malformed passes `WorkbookPeek`, is
    recognised, is listed in the pane and is offered. At the press `OneTemplate` calls
    `SpeciesList.In` at `KpiRequestHandler.cs:509` and the parse throws `XmlException`.
    **`Run`'s catch list at `:183-199` is `ApplicationException`, `InvalidOperationException`,
    `ArgumentException`, `UnauthorizedAccessException` and `IOException`, and `XmlException` is
    none of them**, so it reaches `Execute`'s catch of everything at `:133` and the pane reads
    "That request failed and was stopped here rather than being let out", naming no template, no
    file and no plot, with no report written. A truncated download or a file another tool has
    rewritten is how a sheet part goes bad, and the peek that would have caught it does not read
    that part | One catch in each of the three, returning the refusal each class already has
    words for. `SpeciesList.Refused`, `LabelledCells.Refused` and `StreetReference.Refused` all
    exist

    FIXED, in the ninety second pass. All three catch `System.Xml.XmlException` and return the
    refusal each already had words for, naming the file and the sheet, and `TotalCanopyColumns`,
    added in the ninety first pass with the same three catches as the readers beside it, takes
    the fourth. The rest of the press carries on, which is the rule a refusal on one template
    already follows. `MalformedSheetPartTests` builds a valid zip whose one sheet part is cut
    off mid element and drives all four over it. **Its first case catches the throw itself and
    fails with a message naming `broken.xlsx`**, because without the catch in `SpeciesList` an
    xUnit failure is a bare `XmlException` naming no file at all, which is the very fault: the
    press used to stop naming no template, no file and no plot. **NOTHING HERE CAN BE RUN IN
    REVIT**, so what the pane really shows on such a file is not observed.

### COSTLY

68. DOCS | `CLAUDE.md:29-31` and `:51-53`, with `.claude/rules/kpi-rules.md:42` | **The
    repository's front door does not know the tool writes PDFs.** Measured: `CLAUDE.md` holds
    the string PDF zero times. Its KPI paragraph says Create "copies a template, patches it and
    writes a report" and lists the pane as the model name, KPI Scan, a status line, the template
    picker, the output folder, the plot picker and Create. The tool also fills a client AcroForm
    per plot, `PdfChecklist`, `PdfFill`, `PdfForms`, `PdfFormFile`, `PdfFormCheck`,
    `PdfEmptying` and `PdfOutcome`, **2,149 lines**, and the pane also carries a forms folder
    picker at `KpiPanel.cs:649` and a street reference file picker at `:718`. `:51-53` says Core
    has "three homes ... `DrawingSheet/` and `Kpi/` so far" and it has had four since View
    Filters landed. `kpi-rules.md:42` still says the plot's folder "leaves room for the PDF asked
    for beside it later, and nothing here builds one", **1,080 lines above the section that
    builds one** | The two files a new engineer is told to read before doing any work describe
    two thirds of the tool. `CLAUDE.md` already carries the rule that a line about what the tool
    does is checked against what it does, and records it biting three times | Two paragraphs in
    `CLAUDE.md` and one sentence in `kpi-rules.md`. **`CLAUDE.md` is common ground and not KPI's
    to edit, so it is a request rather than a change**

69. STRUCTURE | `src/RcrcGreen.Core/DrawingSheet/ReportPlaces.cs` | **Three tasks call one
    task's file across the fence.** `ReportPlaces.Written` is called from
    `src/RcrcGreen.Revit/Kpi/KpiRequestHandler.cs:230`, `:429` and `:430`, from
    `src/RcrcGreen.Revit/ViewFilters/ViewFiltersRequestHandler.cs:191` and `:192`, and from
    Drawing Sheet's own handler at three places, and `ReportPlaces.PathFileName` from the common
    `src/RcrcGreen.Revit/ReportFile.cs:52`. It sits in `Core/DrawingSheet`, which
    `.claude/hooks/tasks.txt` maps to the Drawing Sheet task | `territory.md` gives this exact
    case as its worked example, for `PaneLabel`, and its own rule reads "A task needing another
    task's code asks for it to be moved to Shared, in a round of its own, rather than calling
    across the fence or copying it." Three tasks read it and none of them owns where a report
    goes, which is the file's own test for what belongs in Shared. Two consequences today:
    `territory-check.sh` refuses any commit that touches this file beside a KPI file, so a KPI
    session that needs it changed cannot change it, and a Drawing Sheet session changing it
    silently changes what KPI and View Filters print on their status lines | A Shared round,
    which by `territory.md`'s own rule stops every other session first. **Bader's call and
    nobody else's**

70. DEPENDENCIES | `src/RcrcGreen.Revit/RcrcGreen.Revit.csproj:38` and `:42`, with the absence
    of any lock file | **The Revit API reference is a floating version from a third party
    republisher and nothing pins it.** `Nice3point.Revit.Api.RevitAPI` and
    `Nice3point.Revit.Api.RevitAPIUI` are both `Version="2024.*"`. Checked by find over the
    whole tree: there is no `packages.lock.json`, no `nuget.config`, no `Directory.Build.props`
    and no `global.json` | Two machines building one commit can compile against different Revit
    API assemblies, so a build that passed the gate is not necessarily the build a developer
    has, and a compile break that arrives without a commit has no explanation anybody can find.
    The packages are one publisher's republication of Autodesk's assemblies. If they are
    unlisted or the feed is unreachable, `src/RcrcGreen.Revit` cannot be compiled at all and
    there is no vendored copy in the repository. **The shipped product is unaffected**, because
    both carry `ExcludeAssets=runtime` and `install.ps1:54` copies two assemblies and nothing
    else | An exact version in place of the wildcard and
    `<RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>`, then commit the lock
    file. Half an hour

71. DEPENDENCIES | `tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj:4` and
    `.github/workflows/tests.yml`, `dotnet-version: '8.0.x'` | **The gate runs on a runtime
    whose support ends in 56 days.** Checked against Microsoft's own .NET support policy page
    this session, last updated 8 September 2026: **.NET 8 is in Maintenance phase, security
    fixes only, and ends 10 November 2026.** .NET 9 ends the same day and .NET 10 runs to 14
    November 2028 | Neither of the other two targets has a problem and both are recorded with
    their reason. `netstandard2.0` is a surface rather than a runtime, and `net48` is serviced
    for the life of Windows and is what Revit 2024 loads, so **this is one project and one
    workflow line, not three.** Nothing in Core or in the tests uses anything the shared surface
    does not carry | `net10.0` in the test project and `10.0.x` in the workflow, then a green
    run. An hour, and it will be someone's afternoon in December if it waits

    FIXED, in the ninety second pass. `net10.0` in the test project and `10.0.x` in the
    workflow, and nothing else moved. Read off Microsoft's own page again this session,
    `https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core`, last updated
    8 September 2026: **.NET 10 is LTS and Active**, released 11 November 2025, latest patch
    10.0.12 of 8 September 2026, supported to **14 November 2028**. .NET 8 is LTS in
    Maintenance and .NET 9 is STS in Maintenance and both end **10 November 2026**. So .NET 10
    is the current LTS and is what the test project and the gate take. The whole suite was
    built and run on the 10.0.401 SDK here before the gate ever saw it. `netstandard2.0` and
    `net48` are untouched for the reasons this finding already records. **THERE IS NO BREAK
    WATCH FOR A VERSION MOVE**: nothing about what any rule does changed, so there is no rule
    to break and watch go red, and the green run on the new version is the whole of the
    evidence.

72. QA | `src/RcrcGreen.Core/Kpi/PdfForms.cs` with `src/RcrcGreen.Core/Kpi/PdfFill.cs:385-387` |
    **Adding a field to a PDF form reddens two tests and neither of them is the one that
    matters.** Measured, in a copy of the tip: a `PdfValue.Sidewalks` added to the enum and a
    `PdfFormField` for it added to the Roads form gives **2 red of 1,939**,
    `PdfUnitsTests.TheTableCoversEveryFieldOnAllThreeForms` and
    `PdfEmptyingTests.EveryFieldOfTheFormIsInOneOfTheThreeStatesAndNoOther`, both of them
    counts. **Nothing goes red for `PdfFill.One` having no case for the new value** | The new
    field falls to the `default` at `:385`, which blanks it with "nothing in this tool knows
    what Sidewalks is, which is a bug". So the engineer fixes two numbers, the suite goes green,
    and every PDF the tool writes carries an empty box whose own reason calls itself a bug. The
    contrast is the measurement that makes this a finding: **the same experiment with an eighth
    TEMPLATE gives 8 red, and after filling in what those name, 10 more, each one naming the
    next table by name.** The template tables are held together by the gate and the PDF fields
    are not. **The pattern that would catch it is already in this repository, twice, on the
    Drawing Sheet side**: `RunSummaryTests.cs:164` and `PanelStepsTests.cs:274` both assert a
    switch covers `Enum.GetValues`. No KPI test does | One test over `Enum.GetValues(typeof(
    PdfValue))` asserting `PdfFill` answers each with something other than the default

73. ERRORS | `src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:224-231`, and the same shape at
    `:333-343`, `:359-369` and `src/RcrcGreen.Revit/Kpi/ParameterReading.cs:198-208` | **A read
    that could not happen is answered with a value that reads as an answer, at four sites.**
    `CountListed` returns `0` when the collector throws, so "N elements listed" reads as an empty
    schedule. `Spec`, `Unit` and `ParameterReading`'s own spec read return `string.Empty` from
    three identical triples of catches, so a field whose spec could not be read prints as a
    field with no unit override | This is the sentence `CLAUDE.md` already carries, "a read that
    has not happened is an ABSENCE and not a zero", at four places the scan report prints. The
    16:06 run turned on exactly this distinction: every schedule printed a header and no body,
    and what made it readable was `SchedulesWithABody` counting what was FOUND rather than what
    was missing. A count of nought that came from a throw defeats that. **The scan side already
    has the mechanism**, `KpiReader.cs:82` and `KpiScheduleReader.cs:112-124` both add to
    `skipped` and the report prints every skip at the top of its file. These four do not use it |
    The `skipped` list is already threaded through the scan reader. Four call sites

74. QA | `src/RcrcGreen.Core/Kpi` | **18 of 1,269 public members are named nowhere else in
    `src/`, and 5 of them are named nowhere at all, tests included.** Named nowhere:
    `KpiTemplates.MainSheetOpens` (`:22`), `KpiTemplates.MainSheetCloses` (`:24`),
    `PdfForms.ParkType` (`:202`), `PdfForms.RoadType` (`:204`),
    `PlotsPerTemplate.NoTemplateTicked` (`:121`). `Reconciliation.ChooseTheRegion` is finding 23
    and is the sixth. The other twelve are cited and not repeated: 24, 25 and 55 name three, two
    are `ComponentTemplates.ValuesFor` and `PlotPrefixes.PrefixesFor` which exist to keep the
    tables in step and are held deliberately | **`ParkType = "Park"` and `RoadType = "Road"` sit
    at the top of the table that decides what goes into a client's form and read as the two
    values the project type field takes. It takes `reading.Component` verbatim, capitals and
    all, `PdfFill.cs:299-303`.** Two constants that read like a rule and are not one is the shape
    that put PRXComponent on a button. The trend is the other half of the finding: the first
    audit's sweep of this kind found 3, the second 7, this one 18, against a member count that
    has roughly doubled | A decision per member, recorded either way, which is the rule
    `OutputName.Suggested` bought. Not a deletion on reachability

75. DUPLICATION | `src/RcrcGreen.Core/Kpi/WorkbookFormulas.cs:888-897` against
    `src/RcrcGreen.Core/Kpi/CellRef.cs:51-54`, with a third at `WorkbookFormulas.cs:712` |
    **Column letters turn into a column number in two places, and the key that joins the formula
    passes is built a third way by hand.** Both compute `n * 26 + (letter - 'A' + 1)`. They are
    not the same rule: `CellRef.TryParse` refuses anything that is not one to three capitals
    followed by digits and `WorkbookFormulas.ColumnNumber` takes any character, so
    `ColumnNumber("a")` is 33. Separately, `risk` is keyed on `FormulaAtRisk.Where`, which is
    `SheetName + " " + Cell` at `:248-251`, and the level 2 pass looks into it with
    `area.SheetName + " " + Letters(area.FirstColumn) + area.FirstRow` written out at `:712` |
    The level 2 pass is the one that finds `#VALUE!`. If the two spellings ever part, for a
    reference carrying a dollar sign or a sheet name quoted differently, **the lookup finds
    nothing, no `#VALUE!` is reported, and the section that tells the team whether a client
    workbook will compute says everything is fine.** It fails silent by construction, which is
    the one failure mode this repository has paid for eight times | `CellRef.Parse` in place of
    the local maths, and `FormulaAtRisk.Where` built once and asked for rather than spelt again

76. DUPLICATION | `src/RcrcGreen.Core/Kpi/FormulaRepeats.cs:130-158` against `:186-211` | **One
    grouping algorithm written twice over two types.** `Reading(IEnumerable<FormulaCell>)` and
    `Of(IEnumerable<FormulaAtRisk>)` build the shape key, the dictionary, the order list and the
    projection in the same lines. Only the element type and which field supplies the reason
    differ. **Both run in production and both print into the report**, and neither is a test
    only copy | The rule that a shared formula and its master are one shape, measured and
    written at `:161-173`, lives in one of the two. A change to how a shape is keyed lands in
    one of them, which is the split and half kept fault this repository has already had once on
    the Drawing Sheet side | One generic method taking the key builder and the projection, or
    the honest alternative of a comment on each naming the other

77. INTERFACE | `src/RcrcGreen.Revit/Kpi/KpiPanel.cs:600-619`, `:663-682`, `:733-752` and
    `:1474-1486` | **The captioned browse line is written out four times**, the same
    `DockPanel`, the same `WideLabelWidth` caption, the same trimmed path text block and the
    same "No folder set" fallback | The cost is not the lines, it is that **the four do not
    behave alike and nothing says they should.** The output folder line warns when the chosen
    folder is the templates folder, `:625`. The street reference line warns when a remembered
    file has gone, `:756`. The forms folder line does neither, and a forms folder that has been
    moved or unmounted reads as one that was never set, which is the same sentence finding 72's
    other half produces | One method taking the caption, the current value and an optional line
    under it

### TIDY

78. NO VIBE CODING | `tests/RcrcGreen.Core.Tests/Kpi/PdfFormTests.cs:54` with `:57` | The test
    is named `NinePrefixesReachThreeFormsAndEveryFormIsReached` and asserts
    `Assert.Equal(10, PdfForms.All.Sum(one => one.Prefixes.Count))`. The ten are EP, FP, HF, FM,
    DM, PL, SC, NS, ST and MM | A test name is the first thing a reader trusts, which is finding
    60's whole argument at a different test | One rename

79. NO VIBE CODING | `Branded_Factsheet_Template.docx` at the repository root, 2,118,538 bytes,
    added at `57e606a` with the message "Add files via upload" | **What it is, measured by
    unzipping it rather than by its name:** a Word style template, two pages, whose body text is
    "The quick brown fox jumps over the lazy dog" repeated under placeholder headings.
    `dc:creator`, `cp:lastModifiedBy` and `Company` are all empty, so no author is disclosed.
    **What it does carry:** 1,934,251 bytes of one JPEG, three branding SVG and PNG files, and a
    SharePoint content type identity in `customXml/item1.xml`, `contentTypeID
    0x0101002DEBB448C0A2EF4699DF3E3F96BD2B78` and `fieldsID a4989912a230544c42b9cffa918837b3`.
    Nothing in the repository references it: the only two mentions anywhere are
    `steps/log-kpi.md:2586` and `steps/ai-max-state-kpi.md:1118`, both recording that it was
    uploaded | **Should it be there: not until a round uses it.** It is one blob holding 2.7 per
    cent of the whole 78.2 MB history, it leaks none of the client facts the ignore file guards,
    and `*.docx` is not in `.gitignore`, so the next upload of one goes in the same way. It came
    through the GitHub web interface, where no hook runs, which is the same route
    `territory.md` records for the squash message | A decision. Removing it from the tip is one
    commit. Removing it from the history is a rewrite of a public repository and is a separate
    question

80. NO VIBE CODING | `src/RcrcGreen.Revit/Kpi/TemplateFolder.cs:38-41` | The docstring says a
    folder that refuses to be read "comes back empty and the folder line still shows the path,
    so the fault is visible where the person is looking" | The folder line shows the PATH. It
    does not show that the read failed, and an empty listing is what a folder with no workbooks
    in it produces too. A docstring that claims a fault is visible is read as a guarantee, which
    is finding 27's shape | One line, or the change finding 72 asks for on the forms folder
    beside it

---

## Area notes

**Each area was worked on its own and nothing found in one was let into another.** Where two
areas would have reported one fault, it is reported once and the other area cites it.

### Area 1, the maintainer's test

**The three largest files, one sentence each, written from the code with the docstrings
covered.**

```
src/RcrcGreen.Core/Kpi/KpiCreateReport.cs      1711 lines, 49 methods
  Turns a finished run, or a set of runs, into the text of one report file, by appending to
  one StringBuilder through about thirty section writers that each print a heading, a count
  and a table, with the contents block built last and put at the front.

src/RcrcGreen.Revit/Kpi/KpiPanel.cs            1627 lines, 53 methods
  Builds the whole dockable pane in C# on every redraw, from the model header down to Create,
  holding the ticks, the hand picks and the last run and nothing else, and sending every
  request that touches a document through one external event handler.

src/RcrcGreen.Core/Kpi/KpiReport.cs            1122 lines, 34 methods
  The same job for the scan rather than for a run: nine fixed questions about one model
  answered in order, each as a heading, a count and a table, with every read that did not
  happen printed at the top.
```

All three are long and all three are readable, because each is a flat list of one-section
methods over a shared `Line` and `Heading` pair. **Size here is a reading cost and not a
comprehension cost**, which is why finding 29 stands and nothing new is filed against it.

**Every function whose logic I could not reconstruct from the code alone.** Four, out of the 22
longest:

```
WorkbookFormulas.AtRisk            :677   three passes and then a fixed point loop with no
                                          bound stated. It terminates, because every pass that
                                          sets changed adds a key and the keys are bounded by
                                          the formula count, but nothing in the method says so.
                                          Finding 75 is the part of it that can fail silently
WorkbookFormulas.Levels            :672   the level 1, 2 and 3 docstring is the only record of
                                          what a level means and it is above the method rather
                                          than on the type that carries the number
PdfFormFile, the incremental       whole  a hand written PDF writer over five regular
update path                               expressions. The reasoning is recorded in full at
                                          :60-89 and the code follows it, but nobody could
                                          rebuild it from the code without that block
KpiPanel.RedrawTemplates           :424   151 lines that draw the template block and gate
                                          everything below it. This is finding 3's old site and
                                          finding 29's evidence, and it is cited rather than
                                          refiled
```

Everything else I read reconstructs. `SoftscapeRows.Read` is 143 lines and every branch says
why it is there. `Reconciliation.Of` is 158 and is a list of independent checks.

**Could a new engineer add an eighth template without asking anyone. The number is 5 production
files and 8 test cases, and the gate names every one of them, in two rounds.** Measured in a
copy of the tip:

```
step 1   add the template to KpiTemplates and to All
         8 red of 1939, naming: the count, the name list, the main sheet list, the angle
         bracket rule, that no component value reaches it, that no prefix reaches it, and
         two candidate counts
step 2   add its component value and its plot prefix
         10 red of 1939, now naming: the component folder table, the eleven value table, the
         prefix and component tables agreeing, and that no PDF form takes the new prefix
```

So the tables ARE bound and the engineer is led through them. **What the gate cannot tell them
is the one thing that decides whether the workbook is right:** `KpiTemplates.Standard` hands
back D3, E4, H7, F10 and H10, the layout four of the seven share, and a new template built
through it looks complete and is a guess. The cell map is a measurement off a client file and
the measurement lives in prose in `kpi-rules.md`. **The tests pin the seven maps by hand,
`KpiTemplatesTests.cs:58` onward, against the code rather than against a file**, so they say
the map has not changed and never that it is right.

**Could they add a field to a PDF form. Finding 72, and the answer is no.**

**What is written down that is now false.** Every rules file was read against the code.
`CLAUDE.md:29-31`, `:51-53` and `kpi-rules.md:42` are finding 68. `TemplateFolder.cs:38-41` is
finding 80. Findings 27, 28, 40, 41, 45, 58 and 59 already hold the rest and are not repeated.
**Checked and TRUE**, each named so nobody
checks twice:

```
the 28 hook cases                    run here and counted, 28 passed
reports/README.md                    the only tracked file under reports/
*.xlsx and *.pdf ignored             and NO blob of either extension anywhere in the history
the three target frameworks          netstandard2.0, net48, net8.0, all as CLAUDE.md says
the four hooks                       all four wired in .claude/settings.json
the template never written to        guarded on both sides, findings 1's fix
tasks.txt against territory.md       six tasks in each, and hook-tests.sh refuses otherwise
```

### Area 2, duplication, measured

**A six line window sweep over all 110 KPI source files found 18 repeated blocks. Eleven are
`using` headers and are not duplication.** The seven that are:

```
what                                                      where                      which runs
the shape grouping in FormulaRepeats                      :130-158 and :186-211      both
column letters to a column number                         CellRef :51 WorkbookFormulas :893  both
the risk key, built as a property twice and inline once    :183, :251, :709           both
the captioned browse line                                 KpiPanel :600 :663 :733 :1474  all four
catch three types and return the empty string             KpiScheduleReader :333 :359,
                                                          ParameterReading :198       all three
the two identical Where properties                        WorkbookFormulas :183 :251  both
the .xlsx reader catch list, three types against four      five files, Area 3         all five
```

Findings 75, 76, 77 and 73 carry them. **The answer to the question the brief puts, which copy
runs in production and which is only reached by tests, is that NONE of the seven has a test only
copy.** Finding 48 was the one of that shape and it was fixed by moving the rule into Core.
Every copy above runs.

**Two public members are reached by tests alone ON PURPOSE and they are not this fault.**
`ComponentTemplates.ValuesFor` at `:89` and `PlotPrefixes.PrefixesFor` at `:111` exist so that
`ComponentTemplatesTests.EveryTemplateIsReachedByAtLeastOneValue` and
`PlotPrefixesTests.EveryTemplateIsReachedByAtLeastOnePrefix` can hold the tables level. Those
are two of the eight tests that went red in Area 1's experiment. **They are the reason adding a
template is safe and they should stay.**

### Area 3, what happens when it breaks

**88 catch blocks across the 110 KPI files**, every one read.

```
57   name the failure in a value the caller returns or in a list the report prints
 5   name it and stop the request, which is what Run and Execute are for
20   answer with a value that reads as an answer and record nothing
 5   are empty or bare, of which 3 are deliberate and 2 are finding 6's
 1   is Stop's own catch of everything, which is the last thing before Revit would go down
     and is right
```

**The twenty that swallow**, grouped by what the silence costs. Findings 65 and 73 carry the
ones with a cost. `PdfChecklist.cs:237` and `:241` are a bare `continue` over a form file that
could not be read while `FileFor` holds a `why` out parameter it never tells, so a locked Parks
form and a missing one give the same sentence, `NoFormFile + form.Name` at `:252`. The six in
`RememberedFolder` and the four in `RememberedNames` answer a pointer file that cannot be read
with none set, which for a remembered folder is honest enough that it is dropped, and the one in
`TemplateFolder.cs:50` is finding 80. `SamePath`'s four return the empty string and the caller
treats an empty string as cannot tell, which is correct and is dropped.

**Every path where a Revit call can throw, and whether the guard names the plot.** The read side
is guarded per plot and per schedule and each names both, `GuardedRead` at
`KpiRequestHandler.cs:860-873` and `KpiPlotReader.cs:329` and `:347`. That is finding 35, fixed.
**The write side is not guarded at all and that is finding 66.** The schedule filter read is
guarded and swallows, which is finding 65.

**The four cases the brief names, traced.**

```
disk full                     IOException. Caught at WorkbookPatcher :163, KpiRequestHandler
                              :908 and PdfChecklist :160, each refusing that plot by name and
                              the run carrying on. CORRECT
the file locked by Excel      IOException on the delete or the copy. Same three catches, and
                              CreateWords.CouldNotBeWritten says to close it and press again.
                              CORRECT, and the PDF half says the same at :160
the network path going        the root is read once at the press, KpiRequestHandler :315.
away mid run                  Each plot's MadeTheFolder and Patched then fail on their own
                              IOException and each plot is named. CORRECT for the workbook.
                              The forms FOLDER read does record its own
                              failure, :797 and :802, and the per FILE skip inside FileFor
                              does not. Finding 72
the template deleted          the peek listed it, the press opens it. SpeciesList, LabelledCells
between listing and press     and StreetReference all catch IOException and refuse by name.
                              CORRECT, except for the XmlException hole, finding 67
a 156 plot run failing        FINDING 66. The read survives it. The write does not, and the
at plot 100                   report is written after every loop rather than as it goes
```

### Area 4, dependencies and what they cost

**Every package in the solution, with what it is and what it costs.**

```
package                                     version   where        licence  ships?
Nice3point.Revit.Api.RevitAPI               2024.*    Revit        MIT wrapper over Autodesk's
                                                                   own assemblies    NO
Nice3point.Revit.Api.RevitAPIUI             2024.*    Revit        as above           NO
Microsoft.NETFramework.ReferenceAssemblies  1.0.3     Revit        MIT                NO
Microsoft.NET.Test.Sdk                      17.12.0   tests        MIT                NO
xunit                                       2.9.2     tests        Apache 2.0         NO
xunit.runner.visualstudio                   2.8.2     tests        Apache 2.0         NO
```

**Nothing ships. The deliverable is `RcrcGreen.Revit.dll` and `RcrcGreen.Core.dll` and nothing
else**, `install.ps1:54`, and `src/RcrcGreen.Core/RcrcGreen.Core.csproj` holds zero package
references and zero assembly references. Every workbook is read and written with
`System.IO.Compression` and `System.Xml` off the platform, and every PDF with hand written byte
work over `PdfFormFile`. **There is no licence question in the shipped product because there is
nothing in it to license.**

**Whether the licences permit use by a company of twenty or more people.** MIT and Apache 2.0
both do, without condition or per seat fee, and only the four build and test packages are
involved at all. **The EPPlus question the brief raises is answered and the reasoning IS
recorded**, in `steps/log-kpi.md:6750-6753`: EPPlus moved from LGPL to Polyform Noncommercial at
version 5 in 2020, which blocks free use by a company of more than twenty people, and ClosedXML
and NPOI were looked at beside it. The PDF side is recorded at `log-kpi.md:985-986`, iText 7 as
AGPL or paid and IronPDF, Aspose, Syncfusion, DevExpress, Apryse and DynamicPDF all per seat, and
the PDFsharp measurement is in the code at `PdfFormFile.cs:71-78`. **The cost is where it is
written:** all of it is in a log 6,750 lines deep and none of it is in a rules file or in
`CLAUDE.md`, so a new engineer reaching for a workbook library finds no rule saying not to. Not
filed as its own finding because finding 68 is the same gap in the same two files.

**Anything pinned to an unmaintained version:** nothing. Every package above is the current or
near current release of a maintained project.

**Anything unrebuildable from a source that could disappear:** the two Nice3point packages,
finding 70.

**The three .NET targets, against support.** `netstandard2.0` is a surface rather than a runtime
and has no support date of its own. `net48` is serviced for the life of the Windows versions
that carry it and is what Revit 2024 loads, so it is not a choice this project gets to make.
**`net8.0` ends 10 November 2026 and is in Maintenance now**, finding 71, verified against
Microsoft's page this session.

### Area 5, secrets and what leaves the building

**What was scanned and how.** Every blob in the whole history and not the tip: `git rev-list
--objects --all` piped through `git cat-file --batch-check`, deduplicated by sha. **1,942
distinct blobs, 78,220,014 bytes. 1,941 of them are text and every one was decoded and scanned**,
the one skipped being `Branded_Factsheet_Template.docx`, which was unzipped and read separately.
Thirteen patterns: AWS key ids and secrets, GitHub tokens, Slack tokens, Google API keys, private
key blocks, Stripe keys, Azure connection strings and SAS, JWTs, password and api key
assignments, database connection strings, bearer headers and email addresses.

```
0   credentials of every kind, on all twelve credential patterns
3   email addresses, all placeholders: nobody@example.com, nobody@example.invalid,
    scratch@example.invalid
```

**No credential has ever been committed to this repository.**

**Every client specific fact in a committed file, counted at the tip.** Twelve patterns over all
490 tracked files:

```
occurrences  distinct  what
       3501        93  plot identifiers, DM-11, NS-32, FM-05
        478         -  strings beginning PRX_, the greedy match so the distinct count is not
                       given
        371        49  sheet numbers, 010QE, 200Q, 010001A
        310         8  species names, ALBIZIA LEBBECK, Phoenix dactylifera
        181        21  model file names, RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached
        156        25  plot UID2 values, ANH-008-MO-100006
         11         1  the contract reference, GP.NH.Z2.052-DES042
         10         1  the client folder path, MUGHARAZAT
         10         2  the street reference file name, Scope_Validation_21072026
          7         1  the neighbourhood code, ANH-007
          5         1  the project name, Neighborhood Landscape Design - Zone #2
          4         1  the consultant, SAPL
```

**5,044 occurrences across 243 of 490 tracked files.** Ten examples, one per kind: `DM-11`,
`PRX_Ref Plot ID`, `010QE`, `ALBIZIA LEBBECK`, `RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached`,
`ANH-008-MO-100006`, `GP.NH.Z2.052-DES042`, `MUGHARAZAT`, `ANH-007`, `SAPL`.

**Most of this is the price of the project's own rule** that a measurement is written down where
it was made, and it is spread across the rules files, the logs and the test fixtures, which is
where it does the work. The three at the bottom of the table are different in kind, because the
`.gitignore` names them by name as the reason a file is banned. That is finding 64 and it is the
only one of the twelve filed.

**Anything in the history that is not in the tip.** Nothing of consequence. The extension census
over every blob ever: 1267 `.cs`, 577 `.md`, 30 `.html`, 25 `.sh`, 9 `.ps1`, 6 `.csproj`, 5
`.yml`, 5 `.txt`, 5 `.py`, 4 `.json`, 4 `.gitignore`, 2 `.template`, 1 `.sln`, 1 `.docx`, 1
`.addin`. **No `.xlsx` and no `.pdf` has ever been committed**, which is the rule holding over
203 commits. The 30 `.html` are the design mockups under `design/`, all present at the tip.

**`Branded_Factsheet_Template.docx`, 2 MB, uploaded straight to main at `57e606a`.** Finding 79
says what it is, measured by unzipping it, and whether it should be there.

### Area 6, is the debt growing or shrinking

**Read off `steps/log-kpi.md`, the three audit files and git. Numbers, not an impression.**

The first audit landed as pull request 47 and the next KPI round is the fortieth pass, whose
log entry reads "Two guards off the audit". **The current merged round is the eighty third.**
`steps/log-kpi.md` holds 43 entries at or after the fortieth pass, one of them an interstitial
note, so **42 numbered rounds since the first audit.**

```
rounds since the first audit                                              42
of those, rounds that CLOSED an audit finding                              6   40, 45, 46, 51,
                                                                               68, 70
of those, rounds that were themselves an audit                             2   42, 69
of those, rounds that added a feature or changed behaviour                34
```

**Findings opened against closed, over time.**

```
                     opened  closed  open   source
10 Sep  audit 1         +29       0    29   audit-kpi.md, 29 numbered
10 Sep  round 40                 +2    27
10 Sep  audit 2         +20           47   audit-kpi-2.md, 20 numbered
10 Sep  rounds 45, 46           +6    41
11 Sep  round 51               +5     36
13 Sep  round 68               +4     32   matches audit 3's own header exactly
14 Sep  audit 3         +14           46
14 Sep  round 70               +2     44
15 Sep  today                         44
```

**63 opened, 19 closed, 44 open.** The gap widened by 18 between audit 1 and audit 2, narrowed
by 1 between audit 2 and audit 3, and has narrowed by 2 since. **In absolute terms the open
count has gone from 29 to 44, up 52 per cent.**

**Six of the 44 are open against code that no longer exists in that form.** Audit 3 marked
findings 3, 20, 21, 22, 26 and 43 PASSED BY. They are neither fixed nor reproducible and they
sit in the open count for ever, so the honest open number is 38 with 6 unresolvable.

**Test count against KPI file count, over the same period.**

```
                    Core/Kpi  Revit/Kpi  test files  KPI src lines  tests  KPI tests
audit 1  92dd36c         57         13          24         12,575    904        429
audit 2  82d95f4         59         13          26         12,987    942        459
audit 3  f3fe456         78         14          57         21,887   1673        853
today    f2e2f42         96         14          76         28,401   1939       1119
```

```
since audit 1     KPI source lines      +126 per cent
                  KPI test count        +161 per cent
                  KPI test files        +217 per cent
                  Core/Kpi files         +68 per cent
```

**KPI tests per 100 KPI source lines: 3.41, 3.53, 3.90, 3.94.** Test density has risen 16 per
cent while the code more than doubled.

**Open findings per 1,000 KPI source lines: 2.31, 3.62, 1.46, 1.55.** Flat, after the spike the
second audit put there.

**The answer, plainly.** *This project is not accumulating debt faster than it clears it by the
measure that matters most.* Test density is up, finding density is flat, and the one study
number the brief cites, technical debt rising 30 to 41 per cent after AI tool adoption, is not
what this repository shows: the code doubled and the tests outgrew it. **What it IS doing is
accumulating findings faster than it closes them in absolute terms, 63 against 19, and spending
6 rounds in 42 on closing.** Those two statements are both true and they are about different
things. The first is about the code. The second is about the backlog, and the backlog is what a
new owner inherits.

### Area 7, the boundaries

**Core must never reference the Revit API. Checked four ways and it holds.**

```
1  grep for Autodesk over src/RcrcGreen.Core             2 hits, BOTH inside comments,
                                                         DrawingSheet/ReportPlaces.cs:21 and
                                                         Kpi/SamePath.cs:41
2  the project file                                      zero PackageReference, zero Reference
3  the target                                            netstandard2.0, which the Revit
                                                         packages do not support
4  transitively, off the BUILT assembly                  RcrcGreen.Core.dll names exactly one
                                                         external assembly, netstandard, and
                                                         the only strings holding Revit are
                                                         two of its own property names,
                                                         RevitName and RevitPrints
```

**Transitively there is nothing to check, by construction:** Core has no project reference and
no package reference at all, so it has no transitive closure. That is the strongest form the
rule can take and it is the form this project has.

**Every crossing between the Kpi folder and Core/Shared, both directions.**

```
Kpi reads Shared      6 of Shared's 18 public types, over 46 file mentions
                      PlotId 20 files, NaturalOrder 15, ParsedViewName 4, ViewNameParser 4,
                      ScanFileName 2, PaneLabel 1
Shared reads Kpi      NOTHING. grep for Kpi and Pdf over Core/Shared returns nothing
```

**Every crossing between Drawing Sheet and KPI, both directions.**

```
Revit/Kpi names a Drawing Sheet Revit type            NONE, over all 32 types at that root
the Revit root names a Revit/Kpi type                 ONE file, RcrcGreenApplication.cs, naming
                                                      ShowKpiCommand and KpiPanel, which is the
                                                      one line territory.md allows
Revit/Kpi names a common root type                    PanelTheme, PanelMetrics, ReportFile,
                                                      ReportPlaces, RcrcGreenApplication
Core/Kpi names a Core/DrawingSheet type               ReportPlaces, which is FINDING 69
```

**Has any boundary moved since it was set. Two have, and one that should have has not.**
`PaneLabel` moved from KPI to Shared with its tests and `ReportFileNames` moved from Shared to
Drawing Sheet, both recorded in `territory.md` as worked examples, and both are where that file
says they are. **`ReportPlaces` is the one that did not move**, and three tasks call it. Finding
69.

### Area 8, what has never run

**The last run in Revit recorded anywhere is the 19:52 run on 14 September**, which fed the
eighty first pass. Rounds 82, 83 and this one came off verifiers and off reading.

**What has been built since that run and never executed in Revit**, measured with
`git diff --stat` from the eightieth pass's record `484251e` to `f2e2f42`:

```
Core/Kpi      15 files, 1,272 insertions,  44 deletions
Revit/Kpi      3 files,     66 insertions, 19 deletions
```

**1,338 lines added across 18 KPI files with no Revit run behind any of them**, and the list of
what that is matters more than the number:

```
the client's header left alone and the contract reference written per plot   round 81
   this is the fix for the 19:52 run sending 150 PDFs out with NO project
   name, NO consultant and NO contract reference. IT HAS NEVER BEEN RUN
the report reordered, the contents block, ReportSections, FormulaRepeats     round 81
PlotOrigins and the plot list at the top of the report                       round 82
HandTicks, the symmetric tick and untick, the row line on the pane           round 83
```

**Every code path no test reaches AND no run has exercised. That intersection is 80 lines.**
`src/RcrcGreen.Revit/Kpi` is 5,296 lines across 14 files that no test loads and nothing can,
without Revit. Most of it HAS been run, repeatedly, through 14 September. What has neither been
tested nor run is what changed in that folder since: **`KpiPanel.cs`, 74 lines, and
`KpiRequestHandler.cs`, 6 lines.** The 74 are `HandTicks` reaching the tick boxes, Select all
and Clear, and the row line under a workbook row, which is precisely the behaviour round 83 was
asked to make correct. **A tick box is the one thing a test cannot press.**

**Core/Kpi public types no test names at all: 13 of 193.** Audit 3 measured 8 of 143, so the
share went from 5.6 to 6.7 per cent.

```
ComponentFolder, ComponentTemplate, FilledCell, MeasureAnswer, SpeciesAlias, SpeciesMeasure
   rows reached through the type that holds them, as audit 3 recorded
WorkbookFormulas                                   the entry point itself, FINDING 56
PlotOrigin                                         reached through PlotOriginList
AreaCellGlance, RegionGlance, RegionTypeCount,
DivisionGlance, ComputedFieldCount                 the five row types of RunAtAGlance,
                                                   reached through RunGlance
```

`PlotPrefix` came off audit 3's list. **Five of the six new ones are the row types of the
glance, which is the FIRST section of every report.** They are reached through their parent, so
this is reach rather than absence and is not filed, but the direction is worth the line.

**Every rule built on a single observation, updating audit 3's table.** Its fourteen rows stand
where they stood. What has been CONFIRMED since:

```
rule                              was                        now
E5, G5, H5 by letter              NO, finding 50             CONFIRMED and superseded. Row 5
                                                             measured on all seven on 14 Sep,
                                                             the letters were right BY LUCK,
                                                             and the three go by label now
the two computed cells' row       not on the table           MEASURED on all seven, C9 on
                                                             three and C8 on four, which is why
                                                             ComputedPlaces holds no letter
the region the note names         one column read on 2 plots CONFIRMED by count, the 14:29 run,
                                                             98 of 156 and 0 against the note
the group row shape               NO, finding 51             STILL NO. The printed schedule
                                                             section would settle it and no non
                                                             mosque schedule has been read back
a group row against the next row  NO, finding 52             STILL NO, same measurement
the 00 link is named 00           NO, finding 44             STILL NO
CellNumber reads no separator     NO                         STILL NO
```

**Two NEW rules stand on one observation and neither is on audit 3's table.**

```
the three PDF forms' field names and their coordinates, PdfForms.cs:264-361, measured off one
   copy of each of the three files on 14 September. A form the client reissues with a field
   renamed writes nothing into it, which PdfFormCheck refuses on, so it fails safe. RECORDED
   RATHER THAN FILED
the client's three header values, PdfForms.cs:224-235, measured off all three files once. The
   same refusal covers it. This is finding 64's subject for a different reason
```

---

## What was dropped

**Nine findings, dropped for having no cost to the user**, named so nobody looks for them twice.

- `RegionChoiceTests.cs:46` works raw square feet out with `10.7639` while `CreateFixture.cs:116`
  uses `10.763910416709722`. Two test fixtures now disagree in the seventh digit about one
  conversion. Audit 2 dropped the first and no test reads the raw against a hand written number,
  so the second is dropped too
- `WorkbookFormulas.AtRisk`'s fixed point loop is O(formulas squared) over the reads. On a
  4,160 cell workbook it is not measurable
- `KpiRequestHandler.Stop`'s empty catch of everything at `:926` is the last thing before Revit
  would go down and its docstring says so. Correct
- `SamePath`'s four catches return the empty string and every caller reads that as cannot tell
- `RememberedFolder`'s six catches and `RememberedNames`' four answer an unreadable pointer file
  with none set, which is what the person sees and can act on
- `ParameterReading.cs:42` is an empty catch inside a loop that tries the next parameter, which
  is the loop's own meaning
- `PdfFill` blanks `IrrigationWaterDemand` and `GroundCover` unconditionally, `:340` and `:377`,
  and the reason travels with each field into the report. It is a stated gap, not a fault
- `design/` holds 29 mockup HTML files, one per pull request, never pruned. Nothing reads them
  and they cost 30 blobs
- `KpiCreateReport.cs` has two methods named `ThePlots`, `:332` and `:757`, one per overload of
  the report. Legal, and a reader grepping for it finds both

**No em dash, emoji or banned word was found anywhere in `src/RcrcGreen.Core/Kpi`,
`src/RcrcGreen.Revit/Kpi` or the KPI tests**, checked again by grep over the list in
`.claude/skills/ai-max/references/writing-rules.md`.

## What would settle the UNKNOWNs

```
finding 64   Bader's decision on whether the three header values leave the repository
finding 65   whether PlotFilteredOn has ever thrown on a real model. A run would say, if the
             refusal existed to print
finding 67   whether any client template has ever carried a malformed sheet part. UNKNOWN and
             it does not need to have: the hole is the same size either way
finding 71   nothing. The date is verified and the work is an hour
finding 79   whether any round is going to use the docx
audit 3's    unchanged: one non mosque softscape schedule printed as it prints settles 51 and
open list    52, one workbook's E5 in Excel settles 49, a second 00 link settles 44
```

**Report only. Nothing in this repository was changed except this file, the state file the
commit hook requires, and one log entry.**
