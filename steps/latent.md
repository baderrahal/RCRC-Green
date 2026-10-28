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
Input: a disk error between deleting the old kept folder and writing kept-files.txt
Why latent: it needs the disk to fail in that moment
Found: KPI team kit, reviewer and breaker

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
