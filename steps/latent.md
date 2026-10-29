# Latent

Findings no real input of this repo reaches. Each is kept here with no fix. When real data later
reaches one, it moves back to the plan as REAL with a test.

## L1 Nought created and nought failed reads green

Where: `KpiResults` AllCreated, read by the result banner in `KpiPanel.cs`
Input: a template refused because its split does not add up, which records no plot outcome
Why latent: every plot resolves to at most one template, so the split cannot double count
Found: KPI pane redesign, pull request of 28 Oct 2026, breaker

## L2 A plot whose read threw is labelled a schedule check

Where: `FailedReasons.cs`, the fallback that sends every other refusal to ScheduleCheck
Input: `PlotReading.NotRead`, a plot whose read threw
Why latent: no measured run has had a plot read throw
Found: KPI pane redesign, breaker

## L3 Write anyway and the identical area confirm are unreachable

Where: `KpiPanel.cs`, TheAnswerable and TheResult, and `PaneWarnings.SameArea`
Input: two plots reporting one raw area in one run
Why latent: each run is one plot since a checklist became one plot, so IdenticalAreas is empty.
The button now presses Create again where main only set the flag, and no input reaches it
Found: KPI pane redesign, breaker and reviewer

## L4 Retry failed forgets a region picked by hand

Where: `KpiPanel.cs`, Retry failed through Changed, which clears the chosen regions
Input: a plot whose two regions both hold an area and neither is the note's type
Why latent: the client's note settles every measured pair, 98 of 98 on the 14:29 run
Found: KPI pane redesign, breaker

## L5 Members with no production caller after the redesign

Where: `KpiSteps` NothingHasBeenRead, NoTemplateTicked, NoPlotTicked, `KpiStepState` Cell, Mark,
DoneMark, `KpiWillWrite.Rows`, `KpiResults.NotReadyRows`, `KpiPaneWords` Waiting, NotScanned,
Scanning, ReadAt, and `StatusLine.NoModel`, which the handler builds a second way
Input: none, they are reached by tests only
Why latent: nothing on screen reads them. Whether each is the only record of a shape is for the
round that deletes them, under the two reasons a thing gets deleted in kpi-rules.md
Found: KPI pane redesign, reviewer

## L6 A default kind that reads as a deliberate answer

Where: `KpiCreateRunSet.cs`, `PlotOutcome.WroteNothing(..., NotWrittenKind kind = Unknown)`
Input: a future call that leaves the kind out
Why latent: every production call passes a kind today
Found: KPI pane redesign, reviewer

## L7 A locked template reads as a workbook open in Excel

Where: `WorkbookPatcher.Patch`, the sharing codes 0x80070020 and 0x80070021 on any IOException
Input: a template file held open by another program while the press copies it
Why latent: no run has met a locked template, the output workbook is the file people open
Found: KPI pane redesign, reviewer

## L8 Two older points the review passed by

Where: `KpiRequestHandler.cs` an empty catch reached only when the pane is gone, and
`KpiProgressWindow.cs` a `Process.GetCurrentProcess()` never disposed
Input: the pane closed mid press, and every press for the second
Why latent: both were there before this round and neither changes a number or a file
Found: KPI pane redesign, reviewer

## L9 The Write anyway box is built in two places

Where: `KpiPanel.cs`, TheAnswerable and TheResult
Input: none, one view is drawn at a time and the box is unreachable under L3
Why latent: under L3
Found: KPI pane redesign, reviewer

## L11 A report write that throws after the workbooks landed

Where: `KpiRequestHandler.cs`, the IOException and UnauthorizedAccess catches around the
report write, then `Say` in `KpiPanel.cs`, which clears the result as a press refused early
Input: the reports folder refusing the write after every workbook of a press was written
Why latent: no run has met a report write that fails. The status line still names the failure.
Also here: when the write returns no path, `Made` keeps the previous press's report for Show in
report
Found: KPI pane redesign, second pass, breaker and reviewer

## L12 Coarse short reasons for a reconciliation refusal

Where: `FailedReasons.cs`, the reconciliation branch
Input: a plot refused for a region pick and for another reason at once, where the pick wins and
the second reason shows only after the pick
Why latent: every measured region pair is settled by the client's note, and L2 covers a read
that threw
Found: KPI pane redesign, second pass, breaker

## L13 Two template files settling as one asset type

Where: `KpiPanel.cs`, Settled, and the per asset type result table
Input: two workbooks in the templates folder both recognised as one template
Why latent: the team's folder holds one file per template. What the press does with two is
older than this round
Found: KPI pane redesign, second pass, breaker, marked not verified

## L10 Browse tooltips held in the pane rather than in Core

Where: `KpiPanel.cs`, the Browse and dialog tooltips carried over from main
Input: none, they are tooltips and not pane lines
Why latent: the 10 word rule is for what the pane shows, and these were in the pane before
Found: KPI pane redesign, reviewer and breaker

## L14 The record caps read a file through its own byte order mark

Where: `RecordCapsTests.cs`, Normalised, which reads text and counts it as UTF-8
Input: a UTF-16 file under steps, docs or .claude/rules, as Windows PowerShell 5.1 writes one
Why latent: every record in the repo is UTF-8 and no session writes through PowerShell redirection
Found: records split, 28 Oct 2026, breaker

## L15 A line starting with two hashes inside a code block cuts an entry short

Where: `RecordCapsTests.cs`, Entries
Input: an entry quoting a markdown heading inside a fenced block
Why latent: every such line in the five logs today is a dated entry heading
Found: records split, breaker and reviewer

## L16 A log the caps test cannot read is passed in silence

Where: `RecordCapsTests.cs`, Faults, and text above a log's first entry
Input: a new log whose entries use another heading level, or a long preface
Why latent: all five logs use one shape and their prefaces are four lines
Found: records split, breaker

## L17 The caps globs miss a case and a subfolder

Where: `RecordCapsTests.cs`, the `log-*.md` pattern and the rules folder read
Input: a `Log-x.md` on the runner, or a file in a folder under .claude/rules
Why latent: no log is named that way and .claude/rules has no subfolder
Found: records split, breaker

## L18 Stored line endings, ignored files and pictures under steps

Where: `RecordCapsTests.cs`, which measures the working tree with CRLF counted as one byte
Input: a file committed with CRLF, an ignored PDF under steps, or a screenshot saved there
Why latent: there is no such file, and the steps send screenshots to the chat
Found: records split, breaker

## L19 The split script reads a section number from two characters

Where: `steps/runs/2026-10-28-records-split/split.py`, place
Input: a rules file of 100 sections or more, or a topic list naming a number twice
Why latent: the script has run once, on files of 71, 27 and 26 sections, and every join matched
Found: records split, reviewer

## L20 A fourth walk up to the repo root

Where: `RecordCapsTests.cs`, Root, beside three in other tasks' tests
Input: none, it is a copy
Why latent: a shared helper would cross three territories, which is a Shared round of its own
Found: records split, reviewer

## L21 Two models with one title read as one model

Where: `CreateWords.cs`, OpenModel.Is, which compares titles only
Input: two files of one name opened from two folders in one session
Why latent: no measured session has done it, and it is older than this round
Found: records split, breaker

## L22 The CLAUDE.md heading count takes off one heading without naming it

Where: `steps/runs/2026-10-28-records-split/split.py`, the CLAUDE.md proof
Input: a rerun whose pointer text gains a second heading
Why latent: the script refuses outside 4b244b3, and the line holding the heading is checked once
Found: records split, reviewer second pass

## L23 A throw before the install's try ends on red text

Where: `install/team/team-install.ps1`, Get-Process, Unblock-File and the reference read
Input: a damaged DLL in the zip, or Windows refusing a process list
Why latent: neither has happened, and INSTALL-TEAM.txt asks for a screenshot of red text
Found: KPI team kit, 28 Oct 2026, reviewer and breaker

## L24 A failed copy while keeping the old version promises a put back that refuses

Where: `install/team/team-install.ps1`, the keep step and the FAILED line
Input: a disk error between deleting the old kept folder and writing kept-files.txt, or a copy
that fails on the same zip's second install when nothing was ever kept
Why latent: it needs the disk to fail in that moment
Found: KPI team kit, reviewer and breaker, and the second case in the reviewer's second pass

## L25 Group Policy over the launcher's policy bypass

Where: `install/team/Install RCRC Green KPI.cmd`
Input: a machine or user execution policy set by Group Policy, with the zip's internet mark
Why latent: whether the team's PCs carry one is UNKNOWN, and red text asks for a screenshot
Found: KPI team kit, breaker

## L26 SmartScreen's More info, Run anyway is not on the install page

Where: `install/team/INSTALL-TEAM.txt`, step 3
Input: a zip from Teams or a browser on a PC with SmartScreen for .cmd files
Why latent: which prompt the team's PCs show is UNKNOWN
Found: KPI team kit, reviewer

## L27 C# names written again in PowerShell

Where: `build-team-kit.ps1` writes KPI and matches no-commit and changed, `install.ps1` names
features.txt, `team-install.ps1` words an unstamped build its own way
Input: a rename of `Features.Kpi`, `Features.FileName` or the stamp's words
Why latent: nothing renames them, and PowerShell cannot call the C#
Found: KPI team kit, reviewer

## L28 The stamp is in the pane header and the reports, not a title bar

Where: Phase 10 asks for the window's title bar, the round asked for the pane header
Input: none, it is where the round put it
Why latent: the round's own instruction, recorded here so the audit does not read it as missed
Found: KPI team kit, reviewer

## L29 The Q6 test reaches the rule and not the pane line that asks it

Where: `KpiPanel.cs`, Say, which asks `PressEnd.RedrawsTheSteps`
Input: the pane line put back to asking the pressing flag alone
Why latent: no Core test can reach the pane, and the line asks the rule today
Found: KPI team kit, reviewer

## L30 A stamp built where git cannot answer

Where: `RcrcGreen.Revit.csproj`, RcrcBuildStamp, which takes the error text as changes
Input: a build from a source zip or on a machine with no git, or git printing a warning line
Why latent: every build here runs in a checkout, and the kit refuses a no-commit stamp
Found: KPI team kit, breaker

## L31 VERSION.txt and the DLL can disagree about what is installed

Where: `team-install.ps1`, VersionIn, which reads VERSION.txt before the DLL's stamp
Input: an older install.ps1 or a hand copy over a kit install, which leaves VERSION.txt
Why latent: this round's install.ps1 removes it, and nobody copies by hand
Found: KPI team kit, breaker

## L32 install.ps1 leaves the kit's kept version in place

Where: `install/install.ps1` and `%APPDATA%\RCRC Green\previous-install`
Input: Bader installs the kit, then install.ps1 without a put back, then a put back
Why latent: Bader's steps put back before install.ps1
Found: KPI team kit, breaker

## L33 A second manifest with the same add-in id elsewhere

Where: `team-install.ps1` checks only its own Addins\2024 path
Input: a copy in ProgramData\Autodesk\Revit\Addins\2024 or under another name
Why latent: both installs here write the one path
Found: KPI team kit, breaker

## L34 The KPI pane's not-available words name the Drawing Sheet

Where: `src/RcrcGreen.Revit/Kpi/ShowKpiCommand.cs`, NotAvailable
Input: a kit machine where the KPI pane fails to register
Why latent: no pane has failed to register on any measured machine
Found: KPI team kit, breaker

## L35 The journal write and the event raise are unguarded

Where: `RcrcGreenApplication.cs`, Journal, and `KpiPanel.cs`, Ask, which ignores Raise's answer
Input: Revit throwing on a journal comment, or answering a raise with Denied or TimedOut
Why latent: neither has been seen, and the journal line runs only for an unknown name
Found: KPI team kit, breaker

## L36 A repeat install is known by the stamp, which holds the build minute

Where: `install/team/team-install.ps1`, StampOf and $again
Input: two kits of one commit built minutes apart both installed, or Bader's own build of the
same commit in the same minute as the kit
Why latent: the team installs the one zip on Teams, and the kit is built from a clean worktree
Found: KPI team kit, breaker second pass

## L37 A reports folder named by an earlier kit is kept

Where: `install/team/team-install.ps1`, reports-folder.txt left as it is when it names a folder
Input: a PC that installed a kit from before the OneDrive fix
Why latent: no kit zip has been shared yet
Found: KPI team kit, breaker second pass

## L38 An install after a half failed put back keeps the mix

Where: `install/team/team-install.ps1`, put back then install again
Input: a put back that fails part way, then Install instead of a screenshot
Why latent: it needs a disk failure during the put back
Found: KPI team kit, breaker second pass

## L39 A source file under an ignored build or dist folder is not counted as a change

Where: `RcrcGreen.Revit.csproj`, git status against `.gitignore`
Input: a new .cs file under a folder named build or dist inside src
Why latent: no such folder exists under src
Found: KPI team kit, breaker second pass

## L40 A Documents folder on an offline share

Where: `install/team/team-install.ps1`, the reports folder
Input: Documents redirected to a network share that is offline
Why latent: the install ends on a clear FAILED line
Found: KPI team kit, breaker second pass

## L41 A folder from components.csv is not checked as a folder name

Where: `PlotWorkbookPath.cs` 113 and 139, `Path.Combine(root, folder, held)`
Input: a hand typed Folder such as `D:\KPI`, `..`, `SCHOOL/60` or `STREETS.` in a shared or personal file
Why latent: no shared or personal file exists yet and the twelve shipped folders are plain names
Found: 60 per cent round part (a), breaker and reviewer

## L42 A press reads the table in force more than once

Where: `KpiRequestHandler.cs` before the split, `ComponentFolders.For` in the filing paths, `KpiCreateReport` at the end
Input: the shared components.csv saved by someone during a press
Why latent: a press takes minutes and no edit of a shared file during one has been seen. PutInForce hands back its table and the press does not hold it
Found: 60 per cent round part (a), breaker and reviewer

## L43 A version file name spelt another way reads as its kind

Where: `RecognisedWorkbook.ByVersion`, words MORE THAN 60 looked for whole
Input: a file named MOSQUES MORETHAN60.xlsx or MOSQUES OVER 60.xlsx
Why latent: the client's four files carry the words with spaces. A trailing per cent sign is now taken
Found: 60 per cent round part (a), breaker

## L44 A row naming an unknown template is called an unknown value

Where: `PlotsPerTemplate.For` and `TemplateForComponent.For`, the NotInTheTable sentence
Input: a shared row whose Template column is MOSQUES MORE THAN 60% with the per cent sign
Why latent: no shared file exists yet. Both reports name the row and its template as unknown
Found: 60 per cent round part (a), breaker

## L45 The create report shows one spelling of a value the model spells two ways

Where: `KpiCreateReport.TheComponentTable`, grouped on the key
Input: plots carrying DAILY MOSQUE LESSTHAN 60% and DAILY MOSQUE LESS THAN 60% at once
Why latent: which spelling the model holds is UNKNOWN. The scan report prints both
Found: 60 per cent round part (a), breaker

## L46 Two rules say what a version adds to its kind

Where: `AssetTypeNames.Display` parses MORE or LESS THAN n, `RecognisedWorkbook.VersionWords` cuts the kind's name
Input: a version named other than kind plus MORE or LESS THAN n
Why latent: the four versions are all that shape and agree under both
Found: 60 per cent round part (a), reviewer

## L47 The shipped path and the shared path can throw

Where: `ComponentTable.ShippedPath` on an empty `Assembly.Location`, `ComponentFiles.SharedPath` on a pointer with invalid characters
Input: an add-in loaded from bytes, a hand edited templates-folder.txt
Why latent: Revit loads the add-in from its file and the pane writes the pointer itself
Found: 60 per cent round part (a), breaker

## L48 Every redraw reads the shared file and every lookup works the key out again

Where: `KpiPanel.RedrawTemplates` through `ComponentFiles.PutInForce`, `ComponentTable.RowFor`
Input: a templates folder on a slow share, or a table of thousands of rows
Why latent: the table holds fifteen rows and the templates folder is synced locally today
Found: 60 per cent round part (a), breaker and reviewer

## L49 Two filled version paths have no test

Where: `RecognisedWorkbook.ByVersion`, a filled file with one version's words and with two
Input: a filled MOSQUES MORE THAN 60 checklist dropped back into the templates folder
Why latent: no filled version checklist exists yet. The kind's filled path is tested
Found: 60 per cent round part (a), reviewer

## L50 The header is written down twice in ComponentTable

Where: `ComponentTable.Header` and `HeaderFields`
Input: a change to one of the two and not the other
Why latent: neither has changed and a test reads the shipped header through both
Found: 60 per cent round part (a), reviewer

## L51 A bad shared row refuses whatever its first field reads

Where: `ComponentFile.Parse`, a row that cannot be read refuses the key of its first field
Input: `DAILY MOSQUE;MOSQUES;X` in a comma file, `HEALTH, NEW,HEALTHCARE,HC` unquoted, or a note typed in column D
Why latent: no shared file exists yet. Each case is named in both reports and on Setup
Found: 60 per cent round part (a), breaker pass 2

## L52 A components.csv read while it is being saved

Where: `ComponentFile.From`
Input: a redraw in the moment an editor deletes and renames the shared file, or writes it in place
Why latent: the window is a moment and the next redraw reads it whole
Found: 60 per cent round part (a), breaker pass 2

## L53 A quoted line break and Excel's own code page

Where: `ComponentFile.From`, `StreamReader.ReadLine` and UTF-8
Input: a field holding Alt+Enter, or a value with a non ASCII letter saved as plain CSV
Why latent: every value and folder today is ASCII on one line. A split row is refused and named
Found: 60 per cent round part (a), reviewer pass 2

## L54 Two rules say when two folders are one

Where: `ComponentTable` compares folders case blind, `ComponentFolders.Folders` and `FoldersOf` ordinal
Input: a shared file giving FRIDAY MOSQUE the folder Daily Mosque
Why latent: no file spells one folder two ways
Found: 60 per cent round part (a), reviewer pass 2

## L55 A line added to a semicolon file is written with commas

Where: `ComponentFiles.Add` through `ComponentTable.Line`
Input: a shared components.csv Excel saved with semicolons, then Use suggestion
Why latent: no shared file exists yet. The added line is then refused and named on Setup
Found: 60 per cent round part (b), author

## L56 A new template laid out like STREETS gets no road width and no length

Where: `KpiRequestHandler` asks the street reference file only for the STREETS object, and `RunAtAGlance` and `KpiCreateReport` test for STREETS by object or name
Input: a reissued STREETS file under another name with Use pressed
Why latent: no new street template exists
Found: 60 per cent round part (b), breaker

## L57 A new template's name hangs on the rest of the folder, and an old one stays for the session

Where: `NewTemplates.SharedOpening`, and `_madeTemplates` in `KpiPanel`, never cleared
Input: a `Copy of ..._MOSQUES.xlsx` dropped in the folder, or a new file renamed or removed
Why latent: the folder holds the eleven, all opening the same way
Found: 60 per cent round part (b), breaker

## L58 The template a new file is compared with is the first with its layout

Where: `NewTemplates.Review`, the known file picked by layout alone
Input: a file named FUTURE PARK, singular, when the EXISTING PARKS file is listed first
Why latent: no such file is in the folder
Found: 60 per cent round part (b), breaker

## L59 A tick outlives the recognition it was made under

Where: `KpiPanel.TickFor` by file name, the template frozen at the tick
Input: a colleague removing the row that made a file used, while it is ticked
Why latent: no new template exists, and the press then writes nothing for it
Found: 60 per cent round part (b), breaker

## L60 Adding a line to a file of notes only, or into a missing templates folder

Where: `ComponentTable.TextToAppend`, `ComponentFiles.Add`
Input: a shared file holding only # lines, or a templates folder deleted under an open pane
Why latent: no shared file exists yet and the folder is checked on every draw
Found: 60 per cent round part (b), breaker

## L61 Two people adding at once on the synced folder

Where: `ComponentFiles.Add`, a read then an append, not one step
Input: two machines pressing Use suggestion within one sync, or Excel saving the file meanwhile
Why latent: the team has not added a value yet. A dropped line brings the box back
Found: 60 per cent round part (b), breaker and reviewer

## L62 The box says the shared file and the write falls back to your own

Where: `KpiPaneWords.AddsTo`, read before the write
Input: the shared file held by Excel or read only
Why latent: the status line names the file really written
Found: 60 per cent round part (b), breaker

## L63 Choose offers a template made for a file somebody skipped

Where: `KpiPanel.Choosing` and `TemplateForComponent`, both offer every made template
Input: Skip on a new template, then Choose it for a value, which makes it used by K74
Why latent: no new template exists
Found: 60 per cent round part (b), reviewer

## L64 The review runs several times a redraw, and its tests leave temp folders

Where: `KpiPanel.Listing` and `Reviewed`, `NewTemplatesTests.Folder`
Input: a templates folder of a hundred files, and a long run of the suite on one machine
Why latent: the folder holds eleven files and the temp folders are small
Found: 60 per cent round part (b), breaker and reviewer

## L65 A new threshold is suggested the 60 per cent template

Where: `UnknownComponents.Suggest`, most shared words
Input: a model value DAILY MOSQUE MORE THAN 80% or SCHOOL LESS THAN 70%, then Use suggestion
Why latent: no such value is in a model. The box shows Suggested: Mosques >60% before the press
Found: 60 per cent round part (b), breaker and reviewer pass 2

## L66 A templates folder named another way throughout

Where: `NewTemplates.KnownByName` and `Why`
Input: a folder whose files do not end with their template's name, so none is known by name
Why latent: the client's folder names all eleven that way, measured
Found: 60 per cent round part (b), reviewer

## L67 The zip and the Setup on one PC cross each other

Where: `RCRC Green KPI Setup.iss` and `team-install.ps1`, neither reads what the other left
Input: a zip install then the Setup and a put back, or the Setup then a zip install
Why latent: no zip has been shared yet, and INSTALL-TEAM.txt says one way on a PC, not both
Found: Setup.exe round, breaker

## L68 A full install is known by a missing features.txt alone

Where: `RCRC Green KPI Setup.iss`, FullInstallIsHere
Input: a full install whose features.txt is empty or names all three tools
Why latent: install.ps1 deletes features.txt and the kit always writes KPI
Found: Setup.exe round, breaker

## L69 The Revit check answers not running when it cannot run

Where: `RCRC Green KPI Setup.iss`, RevitIsRunning, when WMI and tasklist both fail, with no Log line
Input: WMI off and tasklist blocked by policy, with Revit open
Why latent: neither is off on any measured machine. Inno then stops on the locked DLL
Found: Setup.exe round, breaker and reviewer

## L70 Run as administrator under another account installs into that account

Where: `RCRC Green KPI Setup.iss`, {userappdata} and {userdocs}
Input: IT choosing Run as administrator with its own account
Why latent: PrivilegesRequired=lowest never asks for admin on a double click
Found: Setup.exe round, breaker

## L71 A write the Setup could not make still ends on Finish

Where: `RCRC Green KPI Setup.iss`, ForceDirectories and the pointer writes, answers not read
Input: Controlled folder access blocking Documents
Why latent: the pane makes the reports folder itself before it writes and says so loudly
Found: Setup.exe round, breaker and reviewer

## L72 The uninstaller sits under %APPDATA%\RCRC Green

Where: `RCRC Green KPI Setup.iss`, UninstallFilesDir
Input: somebody clearing %APPDATA%\RCRC Green to reset their own components.csv
Why latent: nothing tells anybody to clear it, and Uninstall then fails loudly. install.ps1 and
uninstall.ps1 then refuse while the entry stays, with advice that cannot work, reviewer pass 2
Found: Setup.exe round, breaker

## L73 The build can find an older ISCC, or none under a missing Program Files (x86)

Where: `install/build-team-kit.ps1`, the ISCC search
Input: Inno Setup 5 on PATH, or a Windows with no ProgramFiles(x86) variable
Why latent: this machine has Inno Setup 6.7.3 per user and a 64 bit Windows. Both fail loudly
Found: Setup.exe round, breaker and reviewer

## L74 The throwaway profiles follow a Documents folder fixed in the registry

Where: `steps/runs/2026-10-28-setup-exe/prove-setup.ps1`
Input: a machine whose Documents is a fixed OneDrive path in User Shell Folders
Why latent: Bader's Documents is the plain one. The checks then fail loudly
Found: Setup.exe round, breaker

## L75 Any running Revit refuses the Setup, another version or another user's included

Where: `RCRC Green KPI Setup.iss`, RevitIsRunning, as team-install.ps1 does
Input: Revit 2023, 2025 or 2027 open on Bader's machine, or a second user's Revit
Why latent: it refuses loudly and asks only to close Revit, never installs over a locked file
Found: Setup.exe round, breaker and reviewer

## L76 The Ready page names a repo script

Where: `RCRC Green KPI Setup.iss`, UpdateReadyMemo, install\install.ps1
Input: a team PC with a full install
Why latent: only Bader has a full install
Found: Setup.exe round, reviewer

## L77 A reports-folder.txt holding only a byte order mark counts as filled

Where: `RCRC Green KPI Setup.iss`, WriteWhereMissing
Input: a pointer file saved as UTF-8 with a BOM and nothing else
Why latent: nothing writes such a file
Found: Setup.exe round, breaker

## L78 The Setup's AppId is written out beside the .iss

Where: `install/install.ps1`, `install/uninstall.ps1` and `prove-setup.ps1`, the HKCU entry path
Input: an AppId changed in the .iss, which would turn both refusals off
Why latent: the .iss holds one id for every version, and changing it breaks upgrade in place
Found: Setup.exe round, reviewer pass 2

## L79 The four zip-only names repeat team-install.ps1's list

Where: `install/build-team-kit.ps1`, $zipOnly in the drift check
Input: a fifth zip-only file added to install\team
Why latent: none is planned, and the build then refuses, naming the file
Found: Setup.exe round, reviewer pass 2

## L80 The drift check compares file names, not where each goes

Where: `install/build-team-kit.ps1`, the Source names against the kit
Input: a Source line with the right name and the wrong DestDir, features.txt beside the manifest
Why latent: every DestDir in the .iss is right, and the proof compares each installed file
Found: Setup.exe round, breaker pass 2

## L81 Installed now reads nothing over a VERSION.txt with an empty first line

Where: `RCRC Green KPI Setup.iss`, InstalledNow
Input: a hand edited VERSION.txt
Why latent: the kit writes the version on line 1 and install.ps1 deletes the file
Found: Setup.exe round, breaker pass 2

## L82 A Source line written another way stops the build

Where: `install/build-team-kit.ps1`, the Source pattern
Input: a leading space, no space after Source:, a subfolder or a wildcard
Why latent: every line is in the matched form, and the build refuses loudly
Found: Setup.exe round, breaker pass 2

## L83 The full install note shows on the first Setup only

Where: `RCRC Green KPI Setup.iss`, FullInstallIsHere after a Setup wrote features.txt
Input: the Setup run twice over what was a full install
Why latent: only Bader has a full install and his pass runs the Setup once
Found: Setup.exe round, breaker pass 2

## L84 A throwaway profile run of install.ps1 is refused while the Setup is on the PC

Where: `install/install.ps1` and `uninstall.ps1`, the guard reads the real HKCU
Input: a session proving install.ps1 in a throwaway profile between Bader's Part 1 and step 20
Why latent: it refuses loudly and names the Apps uninstall, and step 20 removes the entry
Found: Setup.exe round, breaker pass 2

## L85 A mosque or school value typed with > or ABOVE gets the base template

Where: `install/component-rules.csv` lines 4 and 7, through `ComponentRule.Matches`
Input: DAILY MOSQUE >60%, ABOVE 60%, MORE THAN60%, a trailing dot
Why latent: the only measured spelling is MORE THAN 60%, NG05's is UNKNOWN, Q16
Found: component rules round, breaker pass 1

## L86 One value with and without the per cent sign files in two folders

Where: `ComponentRule.FolderFor`, the folder keeps % and the match drops it
Input: DAILY MOSQUE MORE THAN 60% and DAILY MOSQUE MORE THAN 60 on two plots
Why latent: no measured value leaves the sign off
Found: component rules round, breaker pass 1

## L87 A folder named as the value reaches a path unchecked

Where: `PlotWorkbookPath.For`, only the UID2 goes through Unusable
Input: a MOSQUE or SCHOOL value holding < > | " : / or ending in a dot
Why latent: no measured value holds one, and the pane would throw on the redraw
Found: component rules round, breaker pass 1, reviewer pass 1

## L88 A row the K75 box wrote last round beats the rule for its value

Where: `ComponentTable.PlaceOf`, rows first, and step 2's line counts rules only
Input: a shared components.csv line such as NH STREET 20m ROW,STREETS,NH STREET 20m ROW
Why latent: no components.csv on this machine holds one, team machines UNKNOWN
Found: component rules round, breaker pass 1

## L89 A shared rules file with a header and no rules places nothing, with no box

Where: `ComponentRules.InForce` and the Setup boxes in `KpiPanel`
Input: a shared component-rules.csv cut down to its header, or missing the 60 rules
Why latent: no shared rules file exists yet
Found: component rules round, breaker pass 1

## L90 Choose no longer offers the folders named after a value

Where: `ComponentFolders.Folders`, which feeds the Choose dropdown
Input: an unknown mosque value to be filed beside DAILY MOSQUE
Why latent: no real value reaches a box today, and the value itself is offered first
Found: component rules round, breaker pass 1, reviewer pass 1

## L91 Words that point at a components.csv now holding its header alone

Where: `ComponentTable.InWords` shipped not there, `install.ps1` refusal, `AlreadyInAFile`
Input: a missing shipped file, or a press on a value a rule already names
Why latent: reached only by a missing install file or a value no rule places
Found: component rules round, reviewer pass 1

## L92 INSTALL-TEAM.txt names the shared rules file and not the shipped one

Where: `install/team/INSTALL-TEAM.txt`, the line on new values
Input: a team member looking for the rules file with no shared copy made yet
Why latent: the line is the one the round prompt asked for, and the shipped list answers
Found: component rules round, reviewer pass 1

## L93 Section 3 prints not in the table for a value a components.csv refused

Where: `KpiReport.cs`, the template column through `TemplateShown`
Input: a value a components.csv names twice with two answers
Why latent: the placed by column says it was refused, and no file refuses one today
Found: component rules round, breaker pass 1

## L94 An exception row typed with the other separator is answered by a rule

Where: `ComponentFile.Parse`, a bad row refuses the key of its first field
Input: a comma headed components.csv holding DAILY MOSQUE;MOSQUES MORE THAN 60;DAILY MOSQUE
Why latent: the shipped file is its header alone and no shared file exists yet
Found: component rules round, breaker pass 2

## L95 A rule line starting with # is skipped as a note without a word

Where: `ComponentTable.IsNote`, read by `ComponentRuleFile.Parse`
Input: a shared component-rules.csv line #MOSQUE MORE 60,MOSQUES MORE THAN 60,value
Why latent: no shared rules file exists yet, and a # note is meant to be skipped
Found: component rules round, breaker pass 2

## L96 Fields trims trailing empty fields at the components.csv width

Where: `ComponentTable.Fields`, HeaderFields.Length rather than the header read
Input: a header of other than three names in either file
Why latent: both headers are three names today
Found: component rules round, breaker pass 2

## L97 A hand pick goes in by type name, so one type held twice writes the first area

Where: `KpiPanel.RegionButtons` stores the type, `PlotReading.ChosenRegion` takes the first of it
Input: one plot with two regions of one type holding different areas, the second picked
Why latent: no measured model holds one type twice on a plot
Found: region same area round, reviewer and breaker pass 1

## L98 Show is dropped without a word while another request waits

Where: `KpiRequestHandler.Ask`, Show never takes the slot
Input: Show pressed while a read or a press is still waiting to run
Why latent: the slot empties as soon as Revit is idle, and a second press works
Found: region same area round, reviewer and breaker pass 1

## L99 Which of two 00 link instances Show uses has no test

Where: `KpiRequestHandler.ShowRegion`, the fit of each instance
Input: a model holding the 00 link twice
Why latent: no measured model holds two 00 link instances
Found: region same area round, reviewer pass 1

## L100 A same area pick can name the type the note does not name

Where: `RegionChoice.Pick` takes the first region holding the area
Input: CADASTRAL 900 read first, then OUT OF SCOPE 900 twice
Why latent: the number written is right, and no model holds the note's type twice
Found: region same area round, breaker pass 1

## L101 Three regions, two of one area, offer two lines reading one number

Where: `RegionChoice.OptionLine` over `RegionsHoldingAnArea`
Input: regions of 900, 900 and 250 on one plot
Why latent: no measured plot holds three regions, and the pane still asks
Found: region same area round, breaker pass 1

## L102 A region type read empty chooses with no type to print

Where: `KpiPlotReader.RegionsFor`, `ParameterReading.NameOf` empty
Input: a filled region whose type name reads empty
Why latent: a filled region always has a type, and the same path stood before this round
Found: region same area round, breaker pass 1

## L103 A stale hand pick on a type now at 0 writes 0 into the PDF

Where: `PlotReading.ChosenRegion` and `PdfFill`, a pick by hand beating every rule
Input: a type picked by hand, then the model edited so it holds 0, then a press
Why latent: it needs an edit of the model between two presses with the pane kept open
Found: region same area round, breaker pass 1, standing before this round

## L104 A Show queued behind a press replaces the press's end line

Where: `KpiRequestHandler.ShowRegion` through `Told`
Input: Show pressed while Create runs, then run after it
Why latent: the result screen and the report still carry the press, only the status line moves
Found: region same area round, breaker pass 1

## L105 Show checks the link display and the linked view, not what hides a region inside it

Where: `KpiRequestHandler.RegionDrawnIn`
Input: Custom display hiding detail items, the region hidden or cropped in its own view, or out of the host crop
Why latent: none of these setups is measured on a real model
Found: region same area round, reviewer and breaker pass 2

## L106 The Show tooltips are held by their opening words only

Where: `RegionShowingTests`, Assert.StartsWith over three of the four Detail lines
Input: a broken ending of a tooltip, or of the not found one
Why latent: tooltips only, no number reaches them
Found: region same area round, reviewer pass 2

## L107 A dependent view of the region's own view reads as not drawn

Where: `KpiRequestHandler.RegionDrawnIn`, view ids compared exactly
Input: a host plan showing the 00 link by a dependent of the region's own linked view
Why latent: no team plan is known to link a dependent view, and the answer is loud
Found: region same area round, breaker pass 2

## L108 The report paragraph says the note decides where its type is held twice

Where: `KpiCreateReport`, the paragraph under the region table
Input: OUT OF SCOPE 900 and OUT OF SCOPE 900 on one plot
Why latent: no model holds the note's type twice, and the row beside it is right
Found: region same area round, breaker pass 2

## L109 A rules file's Main or fixed Folder reaches the path unchecked

Where: `FolderLayouts.Under` and `PlotWorkbookPath.For`
Input: a shared rules line with Main `\STREETS`, `C:\KPI`, `..` or the word value
Why latent: no shared rules file exists yet, L89
Found: folder layout round, breaker pass 1

## L110 One 60 per cent value typed with and without THAN files in two folders

Where: `ComponentFolderName.Tidy`, THAN added only before a width in metres
Input: SCHOOL LESS 60% on one plot and SCHOOL LESS THAN 60% on another, or 20 M with a space
Why latent: the measured spellings carry THAN and a per cent sign
Found: folder layout round, breaker pass 1

## L111 A plot with no component is refused in Flat over two mains only Grouped uses

Where: `ComponentTable.OnlyFolderFor`, which takes no layout
Input: a template reaching one folder by a components.csv row and a rule with a fixed Main
Why latent: no shipped template has a fixed folder under a fixed Main
Found: folder layout round, breaker pass 1

## L112 Two people on one output root can leave both layouts in one tree

Where: `FolderLayoutSetting`, the pick per person
Input: one person on Flat and one on Grouped, or one on the old kit, pressing into one root
Why latent: whether the team shares one output root is UNKNOWN
Found: folder layout round, breaker pass 1

## L113 A second Revit window shows the old layout until it redraws

Where: `KpiPanel.TheFolderLayout`, read on each draw
Input: a pick made in one Revit window while a second one shows its pane
Why latent: the press reads the file again and its report names the layout it used
Found: folder layout round, breaker pass 1

## L114 Grouped adds a level, and no path length is caught

Where: `PlotWorkbookPath.For`, nothing in Kpi catching PathTooLongException outside SamePath
Input: a long output root with DAILY MOSQUE\DAILY MOSQUE LESS THAN 60% under it
Why latent: the team's output root length is UNKNOWN
Found: folder layout round, breaker pass 1

## L115 A value folder is not checked for characters Windows refuses

Where: `ComponentFolderName.Tidy` into `PlotWorkbookPath.For`, only the UID2 checked
Input: a PRX_Component value holding a slash, a colon or a question mark, or ending in a dot
Why latent: no measured value holds one
Found: folder layout round, reviewer pass 1

## L116 An unreadable layout file reads as no layout picked

Where: `FolderLayoutSetting.Read` through `RememberedFolder.ReadRaw`
Input: kpi-folder-layout.txt held by another program or denied
Why latent: no real input is known to make it unreadable, and Create stays off
Found: folder layout round, reviewer pass 1

## L117 A refusal names earlier files in the new folder, not the old tree

Where: `KpiRequestHandler` AlreadyInThatFolder over the K78 path
Input: MM-01 and MM-09 to MM-15 refused, an earlier press's file in STREETS\ANH-007-ST-100213
Why latent: moving or naming an earlier run's files is out of this round, and the line is true of the folder it names
Found: folder layout round, breaker pass 1

## L118 UNKNOWN and Unknown Tree on one plot aim two counts at one row

Where: KpiMerge.Key with SpeciesAliases.For, SpeciesMatching.Against
Input: one plot printing both UNKNOWN and Unknown Tree under one group
Why latent: the merge keys on the name and the alias is asked after, so the two stay two species and both write B101. No run has shown both spellings on one plot
Found: slash round, breaker pass 1

## L119 A new template of the STREETS kind gets Roads and no street answer

Where: KpiRequestHandler street answer on ReferenceEquals(pick.Template, KpiTemplates.Streets), against PdfForms.ForTemplate on Kind
Input: a template offered on the STREETS layout under another name
Why latent: no such template is recorded, and the reason it would print, only the STREETS template has this cell, would be untrue
Found: slash round, breaker pass 1 and reviewer pass 1

## L120 A unit other than m and sqm also gives a width

Where: StreetReferenceFile.For, the test is not m rather than is sqm
Input: a street row whose QUANTITY UNIT is empty or Null with a numeric ROAD_WIDTH
Why latent: the one Null unit row of the real file is a SCHOOL row with no width, measured 29 October
Found: slash round, breaker pass 1

## L121 A name of slashes and spaces alone keys as empty

Where: SpeciesNames.Key
Input: a workbook row holding only a slash, against a species with no name
Why latent: no real list or schedule holds one
Found: slash round, breaker pass 1

## L122 Other slash and space characters are not tidied

Where: SpeciesNames.Key
Input: a backslash, a division slash, a full width slash or a zero width space in a name
Why latent: none in the seven templates or the 17 September NG05 run
Found: slash round, breaker pass 1

## L123 Two spellings under one group on one plot now refuse the plot

Where: PlotReading.SpeciesPrintedOnMoreThanOneRow through Reconciliation
Input: X / Y and X Y printed under one group on one plot
Why latent: loud rather than silent, and no run has shown such a pair
Found: slash round, breaker pass 1

## L124 The alias lookup compares names its own way

Where: SpeciesAliases.For and the comment above it
Input: an alias whose Revit name carries a slash or a double space
Why latent: the table holds UNKNOWN alone, which both rules read the same
Found: slash round, reviewer pass 1

## L125 The prefix lists on the three forms are a second record

Where: PdfForm.Prefixes against PlotPrefixes and PdfForms.ByTemplate
Input: a prefix added to one table and not the other
Why latent: the prefix route is reached only for a plot no template placed, and a test holds the two equal
Found: slash round, reviewer pass 1

## L126 SpeciesNames.Slash is public with no reader outside

Where: SpeciesNames
Input: none, it is a visibility question
Why latent: no input reaches it
Found: slash round, reviewer pass 1

## L127 A plot whose read threw is asked with no UID2

Where: PlotReading.NotRead, then the street answer in KpiRequestHandler.OnePlot
Input: a STREETS plot whose read threw, whose title block does carry a UID2
Why latent: no read has thrown on a street plot in a measured run, and the plot's own row names the throw
Found: slash round, breaker pass 2

## L128 A refused split leaves no plot rows behind

Where: KpiRequestHandler where the split does not add up
Input: a split whose shares and unplaced plots do not cover the ticked list
Why latent: PlotsPerTemplate builds the split so that it always adds up, and every per plot section shares it
Found: slash round, breaker pass 2
