## 2026-09-14, sixty sixth pass. The commit hooks could be bypassed in silence

**A shared change, so it runs alone.** Nothing else is in this round: no KPI code, no rules
file, no test project. The branch came off a fresh pull of main at `f7fca6d`. **1640 dotnet
tests, 820 of them KPI, unchanged, because nothing under `src` or `tests` moved.**

### What was open

`commit-scope.py` read the commit message out of the command, and for a `-F` path it opened the
file, except that a path of `-` was skipped with a bare continue. The message then came back as
zero bytes, which breaks no writing rule, so `writing-check.sh` passed it. Found by walking into
it on 14 September: the sixty fifth pass made its first commit with `-F -` and a heredoc, carried
a co-author credit line, and was taken without a word.

**One correction to what that round's log said.** It said all three commit hooks scanned an empty
string. That is wrong and is corrected here rather than left standing. **Only
`writing-check.sh` asks for the message.** `require-file-on-commit.sh` asks for `paths` and
`territory-check.sh` for `all-paths`, and neither reads the message at all, so the hole blinded
one hook rather than three. Checked by grep over the three scripts.

### The fix, in two halves

**`commit-scope.py` reports the standard input case rather than skipping it**, and
**`writing-check.sh` refuses on it and says what to do.** Not the wording the unreadable-file
case already used: a file that cannot be opened and a message that was never in a file are
different problems, and only one of them has a remedy a person can act on. The refusal reads
that the message is on standard input, where a hook that runs before the command cannot read
it, and to write the message to a file and name that file with the message flag and its path.

### The first fix was wrong, and its own commit is what said so

The reason was reported by printing a MARKER WORD into the message, which is what the older
unreadable-file case had always done, and the hook searched the message for that word. **The
commit carrying that fix was refused by its own message**, because the message described the
hole and named the marker. Every word of it was accurate and none of it was a failure.

**A signal that travels in the data is not a signal.** A message that has the problem and a
message that merely talks about it are the same string to a substring search, and the older
marker had the same fault for as long as it has existed. Refusing is the safe direction, so
nothing was ever let through by it, but a check that refuses correct work is a check people
route around.

So the reasons come back BESIDE the text rather than inside it. `commit_message` hands back the
text and a list of what could not be read, `main` exits non-zero with the reasons on standard
error, and `writing-check.sh` reads them off standard error and prints them. **Both markers are
deleted**, the older one included, and with them the whole class. `scope` in the hook returns
the answer and the reason together for the same reason.

A case pins it: a message that talks about a message on standard input and about a message file
that could not be read, while being neither, passes. That case fails against the marker version,
which is how it was chosen.

### The second instance, in the same file

`read_commit_call` catches the unbalanced quote case and its own comment says to say nothing was
found and let the caller fail closed. **No caller ever read the flag.** `main` went straight on
and answered off the index, so a command that cannot run as written was answered with a guess at
what it would have committed, to all three hooks. Measured before the change: a command holding
an unbalanced quote came back `index` with exit 0.

The fix is that the script itself exits non-zero, because **all three hooks already refuse on
that** and had done all along. Nothing in any hook changed for it. A `readable` flag carries it,
separate from `found`, because a line holding no commit at all is ordinary and must still pass.

### Tested three ways, and the last two refuse

`.claude/hooks/hook-tests.sh` is new. It drives the real hooks with the JSON payload Claude Code
sends and checks the exit code, 0 for a pass and 2 for a refusal. **15 cases, all green.**

```
a message file it can read passes            exit 0
a credit line is refused                     exit 2
an em dash is refused                        exit 2
a banned word is refused                     exit 2
a message file it cannot read is refused     exit 2
a message on standard input is refused       exit 2
an inline message it can read passes         exit 0
a message ABOUT a failure passes             exit 0
```

The three the round asks for are the first, the fifth and the sixth. Beyond them it checks the
case above, that the scope script refuses a command it cannot read, that all three hooks refuse
that same command, and that the ordinary shapes are still read rather than refused: the message
flag, a message file with pathspecs, and the all form. **Every forbidden string in that file is
built from pieces**, because the file is scanned by the hook it tests and a literal one would
refuse the commit carrying it. The hook itself already did this and the comment saying why is
what pointed the way.

**It runs nothing in the gate**, which runs only the dotnet tests, so it is run by hand. Wiring
it in is a question for Bader rather than something done here.

### Three break watches, all restored byte for byte

1. The dash case put back to a bare continue. **1 red**, a message on standard input is refused.
2. The `readable` flag never set. **4 red**: the scope script directly, and all three hooks.
3. `main` ignoring what `commit_message` could not read. **2 red**, standard input and the
   unreadable file, which is what shows one exit path now carries both.

**The second break found a gap in my own test before it found anything else.** Run against the
first version of the fix it reddened 2 of 3, because `require-file-on-commit.sh` refused for its
own reason: the index was empty, so no state file was in the commit either way. A case that
passes incidentally proves nothing, so a direct case on the scope script went in ahead of the
three hook cases. Same shape as last round, where a break watch went green and exposed a missing
case rather than working code.

### Counts and the merge

**1640 dotnet tests locally at this branch, 0 failed and 0 skipped, 820 of them KPI, none added
and none changed**, because nothing under `src` or `tests` moved. `hook-tests.sh` 15 of 15. Build
zero warnings.

Pull request 115, merged into main as `7f10019`. **The runner executed 1640 tests against the
pull request head, 0 failed and 0 skipped. Locally the same 1640 ran at `7f10019`, 820 of them
KPI, and the hook tests are 15 of 15 there.** The merge went through the API with the title and
the message both passed on the call, the commit came back off main carrying neither a co-author
credit line nor a generated-by footer, and **the merged tree is byte for byte the branch head**,
checked by diffing `1836630` against `7f10019`.

### The other hooks, checked for the same shape

The shape is a guard that cannot see its subject and passes rather than refusing. Every exit
path of the other three was read, and two of them probed.

**`require-file-on-commit.sh`: nothing found.** Its four refusal paths all fail closed. A missing
scope script refuses, a repo with no state file at all refuses, a non-zero from the scope script
refuses, and anything that falls through refuses. The pipe into `tr` would have masked the scope
script's exit code, and does not, because the script sets `pipefail`. Checked by driving it.

**`territory-check.sh`: nothing found in its own logic.** A missing scope script refuses, a
non-zero refuses, and a fault inside its own embedded python refuses, which was probed by making
that python throw and watching it come back exit 2 with the traceback. What it cannot see it is
already documented as not seeing, in `territory.md`: a folder not in the TASKS list reads as
common, a new Drawing Sheet file at the Revit root has to be added to a list by hand, and Drawing
Sheet's flat test files read as common. Those are known limits rather than new findings.

**`writing-check.sh`: one thing left as it is, deliberately.** A file whose content holds a NUL
byte is skipped with a bare continue. That is the same silence the `-` case had, but a binary
file has no text lines to check and refusing every commit carrying an image would be wrong. It is
named here so the next reader knows it was looked at rather than missed.

**`block-paths.sh`: one real hole, NOT fixed this round.** It is wired to Write, Edit AND
NotebookEdit, and it reads `file_path` only. **NotebookEdit's argument is `notebook_path`**, so
the hook reads an empty string, hits `if [ -z "$TARGET" ]; then exit 0` and passes. Measured:
a payload writing to `/etc/evil.ipynb` came back exit 0, where the same path under `file_path`
came back exit 2. This repository holds no `.ipynb` file, checked, so it is open rather than
being exploited. **It is left alone because this round is the hook fix and nothing else.** The
change is one line, reading both keys, and it is Bader's call.

### Still open, for Bader

- **`block-paths.sh` does not see a NotebookEdit path.** Above, with the measurement.
- **Should `hook-tests.sh` run in the gate?** It is a test nobody runs automatically, which is
  its own kind of silence. The gate is `dotnet test` only today.
- **`territory.md` is behind the hook again.** The hook names six tasks and the file numbers
  five: ViewFilters is missing from the list. The file's own header says it fell behind once
  already and to check the two against each other, which is how this was found. Not corrected
  here, because nothing else is in this round.
- **A message the hook still cannot see.** `--amend --no-edit` and `-C <ref>` reuse a message
  from an existing commit, and the scope script returns empty for both, so they pass unchecked.
  That is the same shape again, but refusing every amend would block a normal flow, so it is a
  decision rather than a bug to fix quietly.
- **Non-permeable hardscape** is still blank on every workbook, unchanged from last round.

### For the Drawing Sheet session

**This landed in `CLAUDE.md`**, under the things that have gone wrong, because that is the file
both sessions read every round and neither owns. Two entries: the standard input hole with the
remedy, and the flag that was set and never read. The Hooks section names `hook-tests.sh` and
says it is run by hand. **Write a commit message to a file and name that file with `-F <path>`.**
A heredoc on standard input is refused now, so a round that uses one will be told rather than
passed.

---

## 2026-09-13, sixty fifth pass. Four things off the first per plot run

The tree is right and both files recalculate with zero errors. The street reference file works:
ANH-007-ST-100217 came out ROW 36, length 928.782391, and the workbook computed the area at
33,436.17 itself. So this round is the four things the run showed, and every one of them is
wording or a count rather than anything that reads a model. The branch came off a fresh pull of
main at `70fdb82`, so the baseline is **1625 tests, 805 of them KPI**, measured at that commit
before anything was written. **The audit findings stay open, not renumbered and not reordered,
and this round closes none of them.** **Nothing in this round has been observed in Revit.**

### Character and Context are found by their labels, never by a letter

Bader measured them off the three workbooks written on 13 September:

```
MOSQUES   Character label C7, value D7.   Context label E7, value F7.
SCHOOLS   Character label C7, value D7.   Context label E7, value F7.
STREETS   Category  label C7, value D7.   Character label E7, value F7.
                                          Context   label G7, value H7.
```

**A map holding a letter would have overwritten a formula.** Two of the three templates put
Character at D7 and the third puts a Category formula there, so Character at D7 because two
templates say so destroys the street sheet's own calculation on the third. Nothing in
`FixedCells` holds a letter now. The template is opened when Create is pressed, its main sheet
is read, the label is looked for on it, and the cell to the RIGHT of the label is what gets
written. The street's Category is never touched because nothing looks for the word Category.

A label is matched whole, without case and with edge whitespace off, so Characteristics is not
Character. A template naming neither label writes nothing and says which label was missing. One
naming a label TWICE also writes nothing, and says both cells, because nothing says which of the
two is meant.

`LabelledCell` carries what the value cell ALREADY HOLDS, which is not decoration. The mosque
template came filled, holding Urban Area Zone at D7 and Urban at F7 before this tool touched it,
and the street file has both blank. Without that the report would read as though this run had
put the mosque's values there.

`KpiCreatePlan.Of` takes the labels as a thirteenth REQUIRED argument and throws on null. A
default would have meant a caller that forgot them wrote nothing and said the template was not
opened, which is a lie that looks like a finding.

### The name box is gone, and the reason is the second one

It read GRP-KPI-Checklist-DD-MOSQUES.xlsx and no file has been called that since the folder tree
landed. Every workbook is named from its plot's UID2. The box is out and one line stands where it
was, saying the root, then the component folder, then the UID2, with the workbook named after its
folder, and showing one real path.

**`OutputName.Suggested` is DELETED, and this is the second of the two reasons a thing gets
deleted here, not the first.** The first is reachability, which is the test that was wrong last
round. This is the other one: **the shape it is the record of no longer exists.** There is no
one file per template any more, so a name built from a template and a date is not an unused
method, it is a method describing a thing this tool does not do. Its test went with it.
`OutputName.Final` and `Extension` stay, because a file still has to be named and they are what
names it.

### Count what happened, never what was planned

The run said **MOSQUES: Nothing was written. 20 of 21 plots wrote a workbook.** Twenty workbooks
were on disk. One line held two records of one fact: the row counted what the run set out to do
and the sentence beside it counted what happened.

`TemplateOutcome` counts workbooks now. `Workbooks` is the number, `Written` is that number above
zero, and the new `WroteSomeOfThem` is the case that had no word before, some wrote and some did
not. **Refused is kept for a template where NOTHING was written**, which is the one place the
word still fits. The summary line went the same way: it read 1 workbook written of 2 templates
ticked on a run that wrote 98, because it was counting templates and calling them workbooks. It
counts workbooks, plots and templates each as itself now.

### The create block was a wall

Nine lines of red and orange before the button, and on 78 street plots the plot list alone ran
off the screen.

- **A template row never lists every plot.** `CreateWords.Range` names up to four and gives the
  count and the first to last beyond that, so STREETS reads 78 plots, ST-01 to ST-78. The report
  names them, and the row says so.
- **A refusal and a note no longer look alike.** One stops a workbook and the other does not.
  The three that are notes rather than refusals, the links, the groups left out and a template
  with no ticked plot, go through a new `Noted` that reads in the ordinary text colour and starts
  with the word Note. Nothing moved and no control changed.
- **The link note was six lines of paths.** `LinksLoaded.OnThePane` gives the count and what to
  do about it, and the names stay in the report where there is room for them.
- **The STREETS area line was wrong.** It said the number is typed by hand. It is not: the sheet
  works the area out from the road width and the total length, and both of those come off the
  street reference file. Corrected.

### Four break watches, all restored byte for byte

Each break was made with a script, built, tested, then restored from a backup and checked with a
diff that comes back empty.

1. `FixedCells.Off` taking `"D" + row` as the value cell, which is the letter two of the three
   templates use, rather than the cell right of the label. **5 red**, every one of them in
   `LabelledCellsTests`: the mosque layout, the street layout, the plan writing, a label found
   twice and the whole word match. The street one is the one that matters, because that break is
   exactly the bug the round exists to prevent.
2. `TemplateOutcome.WroteSomeOfThem` returning false, so 20 of 21 reads as refused again.
   **1 red**, `ATemplateWhoseOnePlotFailedStillWroteTheOtherTwenty`.
3. `CreateWords.Range` listing every plot by name whatever the count. **1 red**,
   `ARowOfManyPlotsNamesTheCountAndTheRangeRatherThanEveryPlot`.
4. `WroteAcross` counting templates and calling them workbooks. **1 red**,
   `TheStatusLineCountsWorkbooksAndPlotsRatherThanTemplates`.

Seven existing tests were changed by hand, each because the truth under it moved rather than
because it was failing: two skip reasons now read the template was not opened, the two fixed
values are named without a cell being guessed, a template row drops the words written to and
gains The report names them, and the area reason names the reference file.

### Open, for Bader

**NON-PERMEABLE HARDSCAPE IS BLANK ON EVERY WORKBOOK AND NO NOTE MENTIONS IT.** Nobody has said
where it comes from. It is not read, it is not computed and nothing in this repository names a
schedule, a parameter or a filter that would produce it. It is written down here rather than
guessed at. Three things would settle it: which schedule or parameter holds it, whether it is an
area or a count, and whether a plot with none should print a zero or stay blank. Until one of
those answers arrives the cell stays blank and the report says nothing about it, which is the
honest state rather than the finished one.

### A guard that failed open, found by walking into it

**`writing-check.sh` cannot see a commit message passed on standard input, and says nothing.**
This round's first commit was made with `-F -` and a heredoc. It carried a co-author credit line,
which is exactly what that hook exists to refuse, and the commit was taken without a word.

The mechanism is one line. `commit-scope.py` reads the message out of the COMMAND, and for a
`-F` path it opens the file, except that `if path == "-": continue` skips it in silence. The hook
then scans an empty string, finds nothing and passes. Checked by calling the script by hand with
that command: what comes back as the message is zero bytes. The other two commit hooks read the
same script, so the same command hides a commit's message from all of them.

**This is the shape CLAUDE.md already records at the top of its list**, and it is the second time
for this same file. The first was the file list split on newlines, where a name holding a space
reached the scanner in pieces. The rule that came out of it was that a check which cannot see its
own subject has to refuse. A path of `-` is that case: the message is real and the hook has no
way to read it, so the honest answer is a refusal naming the reason rather than a pass.

The commit was reset and made again through a message file the hook can open, so the guard really
ran on it, and the credit line is gone. **The hook itself is NOT changed here.** It belongs to no
task, every session depends on it, and a one line fix to a shared guard is not something to slip
into a KPI round. It is written down for Bader instead. The fix is small: refuse when a message
file is `-`, the way an unreadable file is already refused through
`RCRC_UNREADABLE_MESSAGE_FILE`.

### Counts

**Locally 1640 tests at this branch, 0 failed and 0 skipped, 820 of them KPI, 15 added, against
the 1625 main carries** at the branch point `70fdb82`. Build zero warnings, measured after the
last file was written.

Pull request 114, merged into main as `9c865b1`. **The runner executed 1640 tests against the
pull request head, 0 failed and 0 skipped. Locally the same 1640 ran at `9c865b1`, 0 failed and
0 skipped, 820 of them KPI.** The merge went through the API with the title and the message both
passed on the call, and the commit came back off main carrying neither a co-author credit line
nor a generated-by footer. **The merged tree IS byte for byte the branch head** this time,
checked by diffing `4b6fc7c` against `9c865b1` over src, tests, .claude and steps, which comes
back empty. Last round it was not, because a file was uploaded to main between the branch point
and the merge, so this is checked rather than assumed.

---

## 2026-09-13, sixty fourth pass. One workbook per plot, in a folder tree

The team came back with how they file these. A checklist is one plot and it lives in a folder
named after it. The branch came off a fresh pull of main at `386948e`, so the baseline is
**1578 tests, 758 of them KPI**, measured at that commit before anything was written. **The audit findings
stay open, not renumbered and not reordered, and this round closes none. Counted off the two
files today: 29 plus 20 is 49 findings, 13 carrying a FIXED mark, so 36 are open.** The round
message said 32. The files say 36 and nothing here was changed to make the two agree.
**Nothing in this round has been observed in Revit.**

Pull request 113, merged into main as `ae9c324`. **The runner executed 1625 tests against the
pull request head, 0 failed and 0 skipped. Locally the same 1625 ran at `ae9c324`, 0 failed and
0 skipped, 805 of them KPI.** **The branch itself carried 1625, 47 added here**, against the
1578 main held at the branch point `386948e`. Build zero warnings at the merge. The merge went
through the API with the title and the message both passed on the call, and the commit came back
off main carrying neither a co-author credit line nor a generated-by footer.

**The merged tree is NOT byte for byte the branch head, and that is not a fault of this round.**
Bader uploaded `Branded_Factsheet_Template.docx` straight to main through the web at 13:04 local,
commit `57e606a`, which is between the branch point and the merge, so `ae9c324` sits on top of it.
Every file this round touched landed intact, checked by diffing the branch head against merged
main over `src`, `tests`, `.claude` and `steps`, which comes back empty. That 2 MB file is the
only difference and it is not mine to touch. **This repository is public**, which is worth saying
once beside a branded template nobody here put there.

**GitHub was in a declared major outage on Pull Requests while this was merging.** Two squash
merge calls came back 500 and 502 and the create call came back 500 once before that. Nothing was
forced and nothing was worked around: the press was retried until it took, and the status page
was still reading major outage when the merge that succeeded went through, so the page lagged the
recovery rather than the other way round.

### What the reference file really measures

Bader sent Scope_Validation_21072026. It is not in this repository and never will be. Read for
its shape and its numbers, 2026-09-13:

```
one sheet, Sheet1, header in row 1, 8,353 data rows
D  ID_UID *        H  ES_QUANTITY     I  QUANTITY UNIT     O  ROAD_WIDTH
A1 holds the number 21484 and there is NO B1 CELL AT ALL
units      m 6,301,  sqm 2,051,  <Null> 1
ANH-007-ST rows   313, which is exactly what the round message predicted
ANH-007-ST-100210 reads 330.65849900000001, m, 20, the row the message names
widths on those 313   15 on 156,  20 on 65,  10 on 57,  30 on 15,  36 on 13,  and 5, 6, 8, 12
duplicate UIDs    33, NONE of them in ANH-007
```

**Three of those are why the code is shaped the way it is.**

The four wanted columns sit at D, H, I and O with a gap at B, so a reader counting along the row
would take C, G and L and one of them would be a status word. They are found by the names in the
header row.

**The file writes an absent value as the text Null in angle brackets** rather than leaving the
cell empty, on all 2,051 sqm rows' road width and on one row's unit. Handed to a parser of my
own it might have come back 0. It goes through `CellNumber`, which reads no number out of text,
and the plot is named instead.

**156 of 313 street plots read a width of 15.** The component values are STREET 30m ROW, STREET
36m ROW, NH STRT 20m ROW and NH STRT LESS 20m ROW. Reading a width off the component name would
have been wrong on more than half of them, which is what Bader's do not estimate is about, and
the file proves it rather than the instruction alone.

### 1. One workbook per plot, in the tree

`PlotWorkbookPath` is the rule and `ComponentFolders` is the table under it. The handler's
`OneTemplate` still reads its whole share in one pass, untouched, because the held readings, the
progress count and the area unit are all decided once per template. `OnePlot` is new and sits
under it: each reading is merged, reconciled, planned, patched and filed on its own.

**Nothing about reading a plot changed.** The merge functions take a list of one now, which is
what makes that literally true rather than a claim: the same `KpiMerge`, the same
`Reconciliation`, the same `SpeciesMatching`, the same patcher.

**The plot gets a folder of its own holding one file.** That reads as one level too deep until
you know why, so it is written down: the PDF asked for later goes beside it.

**A UID2 that would not sit in a path refuses rather than being cleaned**, because this one is
what the team searches folders by and a cleaned name is a plot nobody finds.

### 2. A platform answered differently from the machine the tool runs on

`Path.GetInvalidFileNameChars` was the first guard on that UID2. On Windows it names nine
characters and the control characters. **On the Linux runner this gate uses it names two**, the
null and the forward slash, so `ANH*007?` was refused by the tool on a real machine and accepted
by the test that exists to check the tool. My own test caught it on the first run.

The nine are written out as data now and both separators are checked whichever machine this runs
on. It is the same shape as every other rule here: ask what the thing IS, and where the answer
depends on where you are standing, write the answer down.

### 3. The component folder table, and a count that did not agree

Eleven values, and **the table as Bader wrote it reaches EIGHT distinct folders.** The round
message said nine. The eight are named in a test by hand rather than counted off the list, and
the likely ninth is GOVERMENT BUILDING, spelt that way in the team's snip, deliberately not in
the table because no plot in either measured model carries a component for it. **That is a
question for Bader and nothing guesses at it.**

The two mosque values share one template and get two folders, which is the whole reason this is
a second table beside `ComponentTemplates` rather than a rule on the template name.

### 4. Two cells that are always the same, and a cell reference that does not exist

Character is always Urban Area Zone and Context is always Urban. `FixedCells` holds both.

**Which cell each goes in is measured nowhere.** Neither word appears in the map, in the rules,
or in any run this repository records, and the round message itself says the STREETS sheet is
laid out differently, Category and Character against Character and Context. So the mechanism is
built, no template's map names a cell, and every template reports both as not written with that
reason, one line per template in every run, until somebody measures them.

Guessing a cell reference would put Urban Area Zone into whatever D4 happens to be on seven
client templates and the workbook would look filled. **That is the one part of this round that is
UNKNOWN rather than done**, and it is one line per template in the map when the measurement
arrives.

### 5. A judgement about the root, cheap to overrule

Section 2 calls the root a third remembered thing and section 4 calls the street file a third
Browse button. Those two counts cannot both be right, so one of them had to be read.

**I took the output folder to BE the root.** Three reasons. The street file being the third
button counts templates, output and street, which leaves no room for a fourth. A second folder
deciding nothing is how one stale string became a dead end here already, which this file records.
And the pointer file name is unchanged, `kpi-output-folder.txt`, so nobody's existing setting is
lost and the same refusal already guards it. If Bader wanted a fourth browsed thing it is one
`RememberedFolder` and one pane row.

### 6. The report, and what stands down

One report for the run. The accounting gained the per plot half the round asks for: plots ticked,
folders made, workbooks written, plots that wrote nothing, and **written plus wrote nothing must
equal ticked**. The folders are counted beside those two and deliberately not among them, because
a folder can outlive a refused workbook and one made by an earlier press is not made twice.

Two lines that used to sit in the accounting, plots that went into a workbook and plots ticked and
written nowhere, are gone. They counted plots off the TEMPLATE outcomes, and keeping them beside
a per plot accounting would be two records of one fact.

**Nothing is deleted.** What is no longer reached, and why each was kept, is in the rules file
under its own heading: the merged species list across plots, the identical raw area check, the
sum against the printed total across plots, the plot counted into two workbooks, and the output
name box. Each is the only record of a measurement or a rule.

**The rounding room on a group total does NOT stand down**, against what the round message said.
It is per schedule and per plot, inside `ShrubsAndLawnRows`, and nothing about it moved. Said
here because a round that reported it as stood down would be wrong in the log for ever.

### The break watches

Five, each restored byte for byte and each checked with a diff against its backup.

```
the plot's own folder dropped from the tree     3 red   ThePathIsTheRootTheFolderTheUidAndTheUidAgain
                                                        FourStreetValuesFileIntoOneFolderAndKeepTheirOwnPlotFolders
                                                        TheTwoMosqueComponentsShareATemplateAndNotAFolder
FUTURE PARKS spelt singular, one letter         2 red   EveryComponentValueReachesItsFolder
                                                        ElevenValuesReachEightFolders
the four columns read at A B C D by position    9 red   every StreetReferenceTests case
the two fixed cells skipped and unrecorded      3 red   TheTwoFixedValuesAreNamedAndNoCellIsGuessed
                                                        EveryMappedCellWithAValueBecomesOneWrite
                                                        AValueNoChosenPlotHeldIsSkippedAsNotFound
a plot with no folder counted nowhere           1 red   WrittenPlusWroteNothingIsTheTickedCount
```

**The fifth found a gap in my own test rather than in the code.** Breaking
`PlotsThatWroteNothing` to count only plots that got a folder went GREEN, because every plot in
that test had one. A plot no route places gets neither a folder nor a workbook, which is exactly
the case the accounting exists to catch, and it was not in the test. The case is in it now and
the same break reddens it.

Six existing tests were changed by hand, each because the truth under it moved:
`EveryMappedCellWithAValueBecomesOneWrite` and `AValueNoChosenPlotHeldIsSkippedAsNotFound` for
the four new cells, `TheStreetsBlockNamesNoAreaCellAndSaysWhy` because D8 is on the STREETS sheet
now and is not an area, `TheExistingParksBlockNamesEveryCellAndWhereItsValueComesFrom` because
one plot's area comes off its one chosen region rather than being totalled off several, and
`TheReportOpensWithTheRunAndCarriesEveryTemplateUnderItsOwnName` for the per plot accounting.

### Open, for Bader

1. **Eight folders or nine.** The table gives eight. Is GOVERMENT BUILDING the ninth, and what
   component value reaches it
2. **Where Character and Context sit**, per template. Nothing is written until this is measured
3. **The root and the output folder.** Taken as one thing, for the reasons above. If they are two
   things, say so
4. **The other six asset types in the reference file.** Parking, mosque, park, school, health and
   government rows are all in there and nothing reads them

---

## 2026-09-13, sixty third pass. The read press is agreed, and the seven members are judged

Two answers from Bader and one short piece of work. The branch came off a fresh pull of main at
`6e1b309`, so the baseline is **1456 tests, 763 of them KPI**, measured at that commit before
anything was written. The other 36 audit findings stay open and this round closes none.
**Nothing in this round has been observed in Revit.**

Pull request 104, merged into main as `e2e14a3`. **The runner executed 1451 tests against the
pull request head, 0 failed and 0 skipped. Locally the same 1451 ran at `e2e14a3`, 0 failed and
0 skipped, 758 of them KPI.** **The branch itself carried 1451, 5 fewer than here**, against the
1456 main held at the branch point `6e1b309`, because five tests went with the members they
tested and nothing new was added. Build zero warnings at the merge. The merged tree is byte for
byte the branch head, nothing landed in between. The merge went through the API with the title
and the message both passed on the call, and the commit came back off main carrying neither a
co-author credit line nor a generated-by footer.

### 1. The Read this model button stays, and the judgement note comes off

Bader's answer, in his words: the round message said Create is the only thing that reads and the
rule is that nothing reads without a press. **A button is a press. The fault was a read nobody
asked for rather than a way to ask for one.** So the wording was wrong and the button was right.

He also placed it: **it is better placed than KPI Scan was.** That button sat at the top of the
pane and on the ribbon whether or not anybody needed it, and this one sits in the plots block,
where the thing it produces goes.

The paragraph in `.claude/rules/kpi-rules.md` that named the press as my judgement and said it
was cheap to overrule is replaced by that, recorded as agreed. Nothing in the code moved.

### 2. The seven members, one at a time

**Reachability is the wrong test and last round proved it**, because `OutputName.Suggested` was
deleted on reachability alone and was the only record of the output name's shape. Bader's test
is what each one RECORDS. Three questions each: what shape or measurement is it the written
record of, is that shape written down anywhere else by name, and would any test lose its meaning
without it.

**`CreateWords.GroupsHeading`**, the line over the button row. It records that the grouping went
by the plot prefix and that a button reading MOSQUES has to say what gathered its plots. That is
written down in `PlotPrefixes`'s own class docstring and in the prefix section of
`kpi-rules.md`. **No test anywhere references it**, not one, so no test loses anything and there
is no red to watch for it either. DELETED.

**`CreateWords.GroupLabel`**, a button's own text, MOSQUES, 2 plots. It records that a control
offering to tick a template's plots names the count BEFORE the press rather than after it, and
the plural of plot against that count. The rule is `CreateWords.TemplateRow`, which is live and
prints MOSQUES: 2 plots, DM-12, FM-05 on every ticked row, and the plural comes off
`CreateWords.Count`, which both of them call. `TheRowCountsWhatWillGoInRatherThanWhatItCouldTake`
already asserts that line by hand. DELETED.

**`CreateWords.NoGroupFor`**, the footnote naming the plots no button reached. It records that a
plot no route places has to be visible rather than left out in silence. `TemplateSplit.Unplaced`
records that now, the pane prints one warned line per unplaced plot with its own reason, the
report heads a section PLOTS TICKED THAT WENT INTO NO WORKBOOK, and
`APlotWhoseTemplateIsNotTickedIsNamedAndNotRead` asserts it by hand. **Its own advice is now
false**: it says to tick such a plot by hand, and ticking one today lands it in `Unplaced`
writing nowhere. DELETED.

**`PlotTicks.OnlyFor`**, the press itself. It records REPLACE rather than add, on the reasoning
that one checklist is one template. **That is not a duplicated record, it is a contradicted
one.** `TickingATemplate.Ticked` adds, because several templates are ticked at once now, and its
docstring and `kpi-rules.md` both say why replacing is wrong. A method implementing the rule the
tool decided against is a second and contradictory record waiting for a caller, which is the
shape this repository keeps paying for. DELETED.

**`PlotPrefixes.Grouped`**, one entry per template the plots point at. Two shapes. The ordering,
one entry per template in `KpiTemplates.All`'s own order rather than in the order the plots came,
which `PlotsPerTemplate.Split` does and `TheSharesComeOutInTheTemplateListsOwnOrder` asserts. And
leaving a template no plot points at out of the list, which is **deliberately reversed**: Bader's
decision is that such a template stays ticked, stays listed and says it will write nothing, which
is `PlotsPerTemplate.NoPlotBelongs`. One half recorded elsewhere and one half overruled. DELETED.

**`PlotPrefixes.WithNoKnownPrefix`**, the plots the table has no prefix for. It records that an
unknown prefix is a real answer with its own bucket rather than an absence. `Across` is what
holds that bucket, it is live through `TemplateForComponent.OnePrefixTemplate`, and
`AcrossNamesEveryTemplateTheChosenPlotsPointAt` asserts its name, (no prefix this tool knows), by
hand. This was one line over it. DELETED.

**`PlotPrefixes.PlotsFor`**, the prefix route applied over a list. `PlotPrefixes.For` is the
route itself, it is live through `PlotsPerTemplate.For`, and `EveryConfirmedPrefixResolves`
writes all ten prefix answers out by hand. The order it came back in, the model's own, is
recorded by `TickingATemplate.PlotsOf` and asserted there. DELETED.

### Why all seven came out the same

Bader asked for this rather than for seven identical answers, and the answer is that **they are
one feature's parts and not seven things.** A heading, a button's text, its footnote, its press
and the three lookups that fed it are the grouping row, and a feature is removed as a feature.
What was worth keeping was never among them. It is the prefix TABLE, which is the measurement
the team confirmed, and `All`, `Of`, `For`, `Across` and `PrefixesFor` all stay.

The two rounds come out opposite ways for a reason that is visible in the two subjects.
`OutputName.Suggested` lost its last caller while **the thing it named still existed**, so it
was the only record of a live shape. These seven lost their callers because **the thing they
served stopped existing**, and every shape worth keeping had already been written down somewhere
that is still live and still tested. Two of them were worse than merely unused, and those two
are the ones worth reading twice: `OnlyFor` and `NoGroupFor` both give answers the tool has
since decided against.

### The one kept on that test, and it is not one of the seven

**`PlotPrefixes.PrefixesFor` is reached only from tests and stays**, with a docstring that now
says so in those words. Git says it never had a caller outside the tests in any round, which
makes it a plainer case than the seven rather than a leftover of theirs. It is the only record of the many to one shape: three prefixes mean
STREETS, two mean MOSQUES, HEALTHCARE has one. Read the other way, off `For`, a template is
reached one prefix at a time and the many to one is invisible.
`ThreePrefixesMeanStreetsAndTwoMeanMosques` writes those three lists out by hand and
`EveryTemplateIsReachedByAtLeastOnePrefix` is what the agreement with the component table rests
on. That is the comment Bader asked for, put where it applies rather than on a member that did
not earn it.

### The break watches

Four, each restored byte for byte and each checked with a diff against its backup.

```
PlotsFor returns an empty list      3 red   GroupingGathersEveryPlotForOneTemplate
                                            OneGroupingPressTicksEveryPlotOfThatTemplateAndNothingElse
                                            ItTicksByTheSplitsRuleSoNoTickedPlotCanLandInNoWorkbook
Grouped returns an empty list       2 red   GroupingOffersOneButtonPerTemplateThePlotsReallyPointAt
                                            TheGroupingWordsSayWhatTheButtonDoesAndWhatItLeavesOut
WithNoKnownPrefix returns nothing   1 red   APlotNoButtonGathersIsNamed
GroupLabel drops its count and
NoGroupFor drops its opening        1 red   TheGroupingWordsSayWhatTheButtonDoesAndWhatItLeavesOut
```

The first break reddened `OnlyFor`'s test through `OnlyFor`, which is how a deletion two members
deep was watched rather than assumed. **`GroupsHeading` is the one with nothing to watch**, and
that is its own finding: a constant no test and no caller mentions is a line of text nobody
would learn was wrong.

Five tests went with the members, the four that tested only them plus the grouping press. One
test of my own was rewritten rather than deleted: the contrast in
`ItTicksByTheSplitsRuleSoNoTickedPlotCanLandInNoWorkbook` used `PlotsFor` to show what a prefix
button would have ticked, and it now names the two answers for DM-12 side by side,
`PlotPrefixes.For` saying MOSQUES and `PlotsPerTemplate.For` placing it nowhere, which says the
same thing about one plot instead of about a list.

---

## 2026-09-13, sixty second pass. The header costs nothing, and a template ticks its plots

Three things off the first press over several templates, NG05 at 08:37. Two faults and one
regression. The other 36 audit findings stay open. The branch came off a fresh pull of main at
`d6c9f4a`, so the baseline is **1443 tests, 750 of them KPI**, measured at that commit before
anything was written. **Nothing in this round has been observed in Revit.**

Pull request 103, merged into main as `f41b044`. **The runner executed 1456 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1456 ran at `f41b044`, 0 failed and 0
skipped, 763 of them KPI.** **The branch itself carried 1456, 13 added here**, against the 1443
main held at the branch point `d6c9f4a`. Build zero warnings at the merge. The merged tree is
byte for byte the branch head, nothing landed in between. The merge went through the API with
the title and the message both passed on the call, and the commit came back off main carrying
neither a co-author credit line nor a generated-by footer.

### 1. Every path that can start a read, one by one

Asked of the code rather than reasoned about. Five paths, four of them already right.

```
the pane becoming visible      Ask(WhichModel), the title alone        FREE     was right
every redraw of the block      Ask(WhichModel), the title alone        FREE     was right
a model answering with a title Ask(Plots), every sheet and schedule    HEAVY    THE FAULT
Create                         the scan and the plot reads inside it   HEAVY    a press
DocumentOpened, DocumentClosed the KPI pane subscribes to NEITHER      none     was right
```

The third is the one. `Took` asked for the plots whenever a title it had not read plots for
answered, which is every model anybody opens, because a dockable pane is restored visible at
Revit startup. It asks for nothing now.

**The header is decided in Core.** `KpiHeader.Lines` is three states with one line each and
only the third names a count. A model nothing has read says so and names NO NUMBER, because a
read that has not happened is an absence rather than a zero, and the test asserts that line
carries no digit at all.

**The plot read is behind a press, and that press is a judgement of mine.** The round said
Create is the only thing that reads, and the plot picker cannot be used before the plots exist,
so either the picker waits for a first press of Create or a press exists to fill it. I added
Read this model to the plots block, which is one control where this round removes a whole row of
them. It is written into the rules as a judgement so it is cheap to overrule.

### 2. Ticking a template ticks its plots

`TickingATemplate` is the rule and the template row is the grouping button now, so the separate
row of buttons is gone.

**It ticks by the SPLIT'S rule and never by the prefix.** The buttons used
`PlotPrefixes.PlotsFor`, two letters at the front of an identifier, while the split reads
PRX_Component first. Two rules for one question is this repository's oldest fault, and here it
would tick a plot that then lands in no workbook. A test ticks MOSQUES over a plot whose
component says SCHOOL: the prefix would have taken it and the tick does not.

The three things asked for. **A plot unticked by hand stays unticked** and the hand list clears
when the model changes, because another model's DM-14 is not this one's. **The count on the row
is what will go in**, which needed nothing new: the row counts off the split of the TICKED
plots, so a hand untick moves it from 3 to 2. **A template with no plots still ticks and still
says it will write nothing**, unchanged.

One thing the round had to add that the message did not name: **a row ticked before the read
gets its plots when the read lands.** The template rows come off the templates folder and need
no model, so ticking MOSQUES and then pressing Read is the ordinary order.

**What the buttons did that the rows do not**, which the round asked me to say. Two things, both
deliberate. They REPLACED the ticks rather than adding, which was right when one checklist was
one template and is wrong now. And they could tick a template's plots with that template's
workbook unticked, which is now impossible and is the point. `GroupsHeading`, `GroupLabel`,
`NoGroupFor`, `PlotTicks.OnlyFor`, `PlotPrefixes.Grouped`, `WithNoKnownPrefix` and `PlotsFor`
are now reachable only from tests. **I did not delete them**, and that is the lesson of the
round before: `OutputName.Suggested` was deleted on reachability alone and it was the only
record of the output name's shape. Whether they go is Bader's call.

### 3. The output names have their shape back

`OutputName.Suggested` is restored and asked per row with that row's own file, so each ticked
template carries the shape rather than one of them carrying it. Checked against what the working
runs wrote: `GRP_KPI_Checklist_DD_MOSQUES.xlsx` suggests
**GRP-KPI-Checklist-DD-MOSQUES.xlsx** and the STREETS file suggests
**GRP-KPI-Checklist-DD-STREETS.xlsx**, both written out by hand in the test.

**The deletion was mine and the reasoning was wrong.** A method the last caller stopped calling
can still be the only record of a shape. That goes in the rules beside the restored method
rather than only here.

### The mockup

`design/pr-103/kpi-pane.html`, hand drawn from the code: the table of every read path with its
verdict, the header's three states side by side, the plots block before and after the one press
with the grouping buttons struck through, and a ticked MOSQUES with DM-14 taken off by hand
reading 19 rather than 20. It says in the file that it is a mockup and not a screenshot.

### Three watches, one per item, all red

**Item 1.** `KpiHeader.Lines` made to print the count line for a model nothing has read. **1
red**: `KpiHeaderTests.ShownWithADocumentNothingHasReadNamesTheModelAndNoCount`.

**Item 2.** The hand list check in `TickingATemplate.Ticked` replaced with one that never
fires. **2 red**: `TickingATemplateTests.APlotUntickedByHandStaysUntickedWhenItsTemplateIsTicked`
and `TheRowCountsWhatWillGoInRatherThanWhatItCouldTake`.

**Item 3.** `OutputName.Suggested` made to answer the template name alone again, which is
exactly the regression. **1 red**:
`OutputNameTests.TheSuggestionIsTheTemplateFilesOwnNameAndKeepsItsShape`.

All three restored byte for byte, each checked with a diff against its backup.

### Two existing lines changed by hand

`CreateWords.PlotsBlock`'s second state said the plots were BEING READ and its first promised
that the pane reads a model's plots as soon as one is open. Both were true and both were the
fault. They read as not read yet and waiting for a press now, and
`KpiCreateTests.ThePlotsBlockReadsFourWaysOneLineEach` was changed with them, with its docstring
saying why.

### What worked on that press, on the record

The link note fired and named the link, 1 instance of 6 not loaded with the file named. The
area note fired on STREETS. The templates were opened 7 times over 16 redraws, which is the
folder listing holding its recognitions as it should. The refusal named the missing output
folder and what to press.

---

## 2026-09-12, sixty first pass. Several templates in one press, one workbook each

Round two of the two Bader sent together, off a fresh pull of main at `96b6239`, which carries
round one. The baseline is **1416 tests, 723 of them KPI**, measured at that commit before
anything was written. The other 36 audit findings stay open, not renumbered and not reordered.
**Nothing in this round has been observed in Revit.**

Pull request 102, merged into main as `131b1dd`. **The runner executed 1443 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1443 ran at `131b1dd`, 0 failed and 0
skipped, 750 of them KPI.** **The branch itself carried 1443, 27 added here**, against the 1416
main held at the branch point `96b6239`. Build zero warnings at the merge. The merged tree is
byte for byte the branch head, nothing landed in between. The merge went through the API with
the title and the message both passed on the call, and the commit came back off main carrying
neither a co-author credit line nor a generated-by footer.

### What must not change, and what holds it

Two correct workbooks exist, MOSQUES on NG03 at 15:52 and STREETS on NG05 at 01:30, and this
round must not cost them. **Nothing about the per template logic changed.** `KpiCreateRun` is
still one template's run and `KpiCreateReport.Write` still prints one template's sections, both
untouched. What is new sits above them: a split, a set of runs, and a report that prints each
template's own sections through the writer that was already tested.

### The split

`PlotsPerTemplate` is the decision and it is the rule this repository already carried for
preselecting, asked per plot. PRX_Component decides, the prefix is the cross check, and where
they disagree neither does. Three plots go into no workbook and each is named:

```
component in the table          placed by the component, prefix agreeing or not named
component not in the table      placed NOWHERE, with the value and what the prefix says
no component at all             placed by the PREFIX, which is what EP-05 and its three are
the two routes disagree         placed by NEITHER, both named
```

The middle one is the one worth arguing about. The prefix could have answered in the
component's place, and it does not, because the route that decides gave an answer nobody knows
and a plot going into a client workbook on a cross check alone is a guess. It is named instead.

### The double count

**A plot in two workbooks refuses the whole press.** Every plot resolves to at most one template,
so it cannot happen by construction, and `TemplateSplit` asks the split as it really came out
anyway, naming the plot and both templates. Two tests, one over the guard with shares built by
hand and one over the rule underneath it, because a construction that cannot go wrong is not a
check and the check is what survives the next change.

### Read once, and no second cache

A plot belongs to one template, so it is read once with that template's own counted groups and
its own area rule. `HeldReadings.Decide` is asked once per template with that template's own
share of the plots and the run IT produced last press, so the mechanism that turned a 123 second
read into a 2.5 second second press is the same one and there is nothing beside it. A held run
for one template never answers for another, and a test says so in the words the refusal uses.

### One thing the round had to fix in the progress line

Counting the plots per template would have restarted the count at 1 on the second workbook, and
the progress rule in this repository says the count only grows and cannot go backwards. The
total is every plot every ticked template will read, worked out before the first one is, and a
template answering from held readings advances it by its own share so the total is still
reached.

### The pane

The workbook rows are CheckBoxes rather than a pick, each ticked row carries its own output name
box, and the create block grew a row per ticked template before the press and a row per outcome
after it. Nothing moved. The three typed boxes are one set for the whole run and the pane says
so when more than one template is ticked.

### The mockup

`design/pr-102/kpi-pane.html`, hand drawn from the code: the workbook list as one pick and as
tick boxes, the create block before the press with a ticked template that gets nothing, the same
block after a press where one of three was refused, on the dark theme, and the report's own
opening. It says in the file that it is a mockup and not a screenshot.

### One deletion

`OutputName.Suggested` offered the template file's own name and nothing called it once each row
took its name from `CreateWords.SuggestedName` with its own plots. Deleted with its test rather
than left beside the thing that replaced it.

### Three watches, all red

**The double count check.** `if (holding.Count <= 1) continue;` widened to `<= 99`, so a plot in
two shares passes. **2 red**: `PlotsPerTemplateTests.APlotInTwoWorkbooksRefusesTheWholeRunAndNamesBoth`
and `RunAcrossTemplatesTests.ADoubleCountedPlotTakesOverTheStatusLine`.

**The unplaced naming.** The filter that finds plots belonging to no ticked template replaced
with one that finds none, so such a plot disappears. **1 red**:
`PlotsPerTemplateTests.APlotWhoseTemplateIsNotTickedIsNamedAndNotRead`.

**The refused count.** `WasRefused` made to answer false, so a refused template reads as one
with nothing to write. **2 red**: `RunAcrossTemplatesTests.TheFourCountsAddUpToTheNumberTicked`
and `TheStatusLineCountsTheTemplatesAndNamesTheReport`.

All three restored byte for byte, each checked with a diff against its backup rather than by
reading it.

### The one existing test changed by hand

`KpiCreateTests.OneRefusalListsEverythingMissingRatherThanOnePerThing` pinned the words No
template picked. The rows are ticked now, several at once, so the refusal reads No template
ticked. Changed with the behaviour and the docstring says why.

### What waits on a run

Everything. **Nothing here has been observed in Revit.** The two correct workbooks are the thing
this round must not have cost, and only a press over MOSQUES and SCHOOLS together can say
whether it did.

---

## 2026-09-12, sixtieth pass. The two answers, and UNKNOWN reaches row 101

Two answers from Bader, both measured on the workbook the 1552 run wrote on the NG03 model,
which is not in this repository. Neither could have been taken here. The other 36 audit findings
stay open. The branch came off a fresh pull of main at `96b6239`, so the baseline is **1416
tests, 723 of them KPI**, measured at that commit before anything was written. **Nothing in this
round has been observed in Revit.**

Pull request 101, merged into main as `3a3312f`. **The runner executed 1425 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1425 ran at `3a3312f`, 0 failed and 0
skipped, 732 of them KPI.** **The branch itself carried 1425, 9 added here**, against the 1416
main held at the branch point `96b6239`. Build zero warnings at the merge. The merged tree is
byte for byte the branch head, nothing landed in between. The merge went through the API with
the title and the message both passed on the call, and the commit came back off main carrying
neither a co-author credit line nor a generated-by footer.

### The third blocker does not bite, and that UNKNOWN is closed

The fifty ninth pass said whether `BelowTheList` reaches row 101 was UNKNOWN here, because no
workbook and no report is in this repository. Bader measured it:

```
Tree List - Existing   98 names, rows 4 to 101, no empty row inside the list, first gap 102
Tree List - Proposed   80 names, rows 4 to 83,  no empty row inside the list, first gap 84
```

Row 101 is ABOVE the first empty row, so the rule never reaches it. **No behaviour changed for
this item.** The rule is right and unchanged, and what was an UNKNOWN in this log is now a
measurement with the file and the date it came from beside it.

### The name rule is an alias, and the reasoning is the point

**It is not a rule at all.** Exactly one of the 98 names opens with UNKNOWN, so any rule built
on a shared opening, a prefix or a longest match would work on that one name and then have to
pick between Conocarpus erectus and Conocarpus lancifolius, between Ficus benjamina, Ficus
religiosa and Ficus pseudosycomorus, and between Prosopis juliflora and Prosopis glandulosa.
A picked genus is a silent wrong number in a client file, which is worse than a tree that goes
nowhere. And UNKNOWN is not a species: it is Revit's placeholder and Unknown Tree is the
client's placeholder for the same thing, so the two meeting is a fact about this project and a
fact about the project is data.

`SpeciesAliases` in Core is the table, one entry, UNKNOWN means Unknown Tree. Three guards, all
tested.

**It applies only where it resolves to exactly one row on that sheet.** Two rows is a refusal
naming every one of them. None is nothing at all, and a species then goes down the empty row
route exactly as it did before, where the report already names it, so nothing is skipped and
nothing is silent.

**It never overrides a real match.** The exact name is matched above the table and the table is
not consulted at all, and a name below the first empty row keeps its own refusal, because that
is a row the workbook really carries.

**The report says which count rests on it.** SPECIES MATCHED grew a how column, matched on its
own name or THROUGH THE ALIAS UNKNOWN means Unknown Tree, and SPECIES MATCHED THROUGH AN ALIAS
is a block of its own with the alias, the sheet, the row and the count.

`ClosestName` is untouched and still prints beside every remaining miss. It is how the next
alias gets found and it is still a print and never a match.

**The check cannot be run here.** UNKNOWN Existing is 16 trees, Total Trees should move 374 to
390 and the canopy should stay at 11,168, because row 101 holds a real zero for its diameter.
Nothing in this repository can open that workbook, so those three numbers are for the next run.

### One thing the round simplified

The total's reach used to be checked in the matching loop and would have been checked a second
time on the alias route. `OnTheRow` is the one place a row becomes a match now, asked by both,
so there is one rule rather than two that agree on the day they are written.

### Two watches, both red

**Guard a.** `if (aliased.Count > 1)` changed to `if (aliased.Count > 99)`, so two rows would
take the first. **2 red**: `SpeciesAliasTests.AnAliasThatReachesTwoRowsIsRefusedAndNamesBoth`
and `SpeciesMatchingTests.AnAliasReachingFourRowsIsRefusedAndNamesEveryOneOfThem`. Restored.

**Guard b.** The alias consulted before the exact match rather than after it. **1 red**:
`SpeciesAliasTests.AnExactMatchWinsAndTheAliasIsNotConsulted`. Restored byte for byte and
checked with a diff against the backup rather than by reading it.

### The one existing test changed by hand

`SpeciesMatchingTests.UnknownIsNeverMatchedToUnknownTreeAndTakesNoRowEither` pinned the old
rule, that UNKNOWN could reach no row and was withheld for having no size. Its fixture holds
four Unknown Tree rows, which is the 2026-09-09 measurement, so under the alias it is refused by
guard a instead. It is renamed `AnAliasReachingFourRowsIsRefusedAndNamesEveryOneOfThem` and
asserts the new refusal word for word. **The count still goes nowhere either way**, which is
the part of it that did not change, and its docstring says why it was rewritten.

### Still never exercised

**Street Design counting as Proposed on STREETS now HAS a run.** The STREETS workbook on NG05 at
01:30 read ST-05 as 369 existing, 2 proposed and 68 Street Design taken as proposed against the
schedule's own TOTAL of 439, which is the exact arithmetic the rule was written to. That
sentence has been in this log as never exercised since the rule landed and it comes out now.

---

## 2026-09-12, fifty ninth pass. UNKNOWN is written, and a run with no link loaded says so

Two items, measured on two runs. The other 36 audit findings stay open. The branch came off a
fresh pull of main at `e3ba145`, so the baseline is **1316 tests, measured at that commit in a
worktree of its own rather than remembered**. **Nothing in this round has been observed in
Revit.**

Pull request 97, merged into main as `6275bf3`. **The runner executed 1385 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1385 ran at `6275bf3`, 0 failed and 0
skipped, 723 of them KPI.** **The branch itself carried 1332, 16 added here**, against the 1316
main held at the branch point `e3ba145`, measured there in a worktree of its own. Build zero
warnings at the merge. The merge went through the API with the title and the message both
passed on the call, and the commit came back off main carrying neither a co-author credit line
nor a generated-by footer.

**The merged tree is NOT byte for byte the branch head.** Two Drawing Sheet rounds landed while
this one waited on a GitHub rate limit, `d77b90a` and `e2aa4ad`, pull requests 96 and 98, and
that is the whole of the jump from 1332 to 1385, 53 tests, none of them KPI. **The KPI half did
land byte for byte**: a diff of this task's files between the branch head `621f969` and the merge
is empty, and the KPI count is 723 on both.

### Item 1(b) was already correct, and saying so is the first thing this entry owes

The round message named two reasons UNKNOWN was not written on the 1552 run, and asked for the
second to be fixed: the empty row route withheld it for having no diameter, the right rule in
the wrong place, and it must not apply to a row the workbook already holds. **At today's lines
it already does not.** `SpeciesMatching.Against` asks `MeasureAnswer.Write` of the canopy
diameter on the `WrittenInto` branch alone, `NotSized` is constructed nowhere else, and
`KpiCreatePlan` writes a matched row's count into column B and touches none of the client's
other cells. There was one reason UNKNOWN went unwritten, not two, and it is the name.

That is worth more than the fix would have been, because the premise came from reading a report
rather than the code and it would have been fixed by changing a rule that is already right. It
is pinned now rather than left as an assertion in a log entry: `AMatchedRowWithNoDiameterIsStillWritten`
puts UNKNOWN on a matched row with no height and no diameter, and asserts the count is written
into column B and that the name, the height and the diameter columns of that row are not
written at all.

### The third blocker, which is in no round message

Even a name matching word for word is refused when it sits past the list's first empty row.
`SpeciesList` reads column D as far as the first gap and holds everything after it in
`BelowTheList`, and `SpeciesMatching.Against` refuses a species whose only name is there rather
than writing it in a second time above the gap. That rule is right and is not being changed.

**Whether it bites for the real row 101 is UNKNOWN from this repository.** No workbook is in it,
nothing under `reports/` is committed, and the round message gives row 101's cells but not where
the list's first empty row is. The create report already prints the names below the first empty
row with their rows under THE WORKBOOK'S OWN TREE LISTS, so the 1552 report answers it off the
file. `ANameHeldBelowTheFirstEmptyRowIsRefusedEvenWhenItMatchesExactly` pins the refusal and its
exact words, so whichever way the answer goes, the behaviour is written down rather than
discovered again.

### The name, which is Bader's to rule on

**The match is not widened.** UNKNOWN against Unknown Tree still does not match, and no rule
was invented for it. What the tool does instead is report what it missed:
`SpeciesMatching.ClosestName` finds the workbook name sharing the longest opening with the
Revit name, ties broken by the shorter name and then naturally, empty where nothing is shared
at all. It rides on `SpeciesMatch.NearestInTheList` and prints as the second column of the
unmatched species list. **It is printed and never matched on**, and a matched species carries
none, so the column cannot start deciding anything by accident.

**I cannot enumerate the 1552 run's unmatched species from this repository.** The report is not
committed and never will be, and the workbook is not here either. So the answer to whether it
is one name or a family of them comes off the next run, where every unmatched species prints
with the nearest name in the list beside it. Three shapes are already known and they are not
one problem: UNKNOWN against Unknown Tree is a different word, ACACIA / VACHELLIA FARNESIANA
matches Acacia / Vachellia farnesiana today, and BOUGAINVILLEA GLABRA 'PINK PIXIE' carries an
apostrophe that nothing has yet been measured against.

**The check on this half cannot be run here.** UNKNOWN Existing is 16 trees, Total Trees moves
374 to 390 and the canopy stays 11,168 once the row is reachable. Reachable needs the name rule,
which is Bader's, so that check is for the run after the rule lands.

### Item 2, a run with no links loaded

Measured on the first STREETS run, NG05 at 16:06, 78 plots: all 78 contributed nothing, all 156
schedules printed one row and no body, group rows none on every one, and the scan says six link
instances with NONE LOADED. Every number the tool printed was right. It never said the one thing
that explains all 78.

`LinksLoaded` is the new Core file and the whole rule, five states with one line each, a note and
never a refusal. It reaches three places.

**The report opens with it.** `TheLinks` sits between the clock and the first step in
`KpiCreateReport`, so the warning is above RECONCILIATION. The test asserts the index of NO LINK
IS LOADED is below the index of RECONCILIATION rather than asserting both appear, because two
present strings say nothing about which a person reads first.

**The pane says it before the press.** `KpiPlotFacts` carries a `LinksLoaded` built by
`ReadThePlots` off the host document's own `RevitLinkInstance` collection, and `TheCreateButton`
puts it above Create beside the refusal line. Twenty minutes are spent before the report exists,
so a warning only the file holds is a warning that arrives too late to save the run.

**And the reconciliation grew the two counts it could not fake.** Every count it already carried
read green over that run: 78 ticked, 78 read, 156 schedules found, nothing refused, nothing
missing, because a schedule that printed no body is still a schedule that was read.
`GroupRowsFound` is how many group rows the run found across every plot and `SchedulesWithABody`
how many printed one, said as a count of the schedules printed. The 16:06 run reads 0 and 0 of
156.

### The mockup

The round changes the pane by one line, so it carries `design/pr-97/kpi-pane.html`, hand drawn
from the code: the Create block before and after on the light theme, the partly loaded state on
the dark theme where the warning colour is `#ff8080` rather than firebrick, and the report's own
opening with the warning above RECONCILIATION. It says in the file that it is a mockup and not a
screenshot, that Create stays live because this is a note, and that the six link names in it are
invented for the drawing, since only the count and the NONE LOADED state were measured.

### Two watches, one per item, both red

Rebuilt before each, because a `--no-build` run after a restore reads the old assembly and that
has cost this session twice.

**Item 2.** `LinksLoaded.Of` had `if (loaded == 0)` changed to `if (false)`, so a model with
six instances and none loaded fell through to the some-loaded-and-some-not line. **3 red**:
`TheReportOpensWithTheReasonARunFoundNothing`, `NotOneLoadedIsSaidWithTheCountAndEveryName`,
`OneInstanceReadsInTheSingular`. Restored byte for byte.

**Item 1.** `KpiCreatePlan` took `if (!match.Species.Diameter.Write) continue;` immediately
before the quantity write, which is the no-diameter rule reaching a matched row, the thing the
round message believed was already happening. **4 red**:
`UnknownRowTests.AMatchedRowWithNoDiameterIsStillWritten`,
`KpiCreatePlanTests.AMatchedSpeciesBecomesAQuantityInColumnBOfItsOwnRow`,
`KpiCreatePlanTests.AMatchedSpeciesWritesTheCountAloneAndLeavesTheNameAsTheWorkbookSpellsIt`,
`KpiCreatePlanTests.AnAddedSpeciesWithNoListColumnsWritesItsNameAndItsCountAndNamesTheOtherTwo`.
Restored byte for byte. **The first attempt at this one was a no-op**: the line went inside the
`!match.Added` block after the quantity write, so nothing changed and nothing went red. A break
that changes no behaviour proves nothing, and it read exactly like a break that the tests
survived.

A third, optional watch on the nearest-name guard did not go red either way it was broken,
`opening == 0` to `opening < 0` and `shared = 0` to `shared = -1`, because the tie-break still
rejects a name sharing nothing. That guard is not load bearing in that form and is recorded
here rather than left as a silent pass.

### The one existing test changed by hand

`CanopyColumnsTests.TheReportSaysWhatWentIntoEachMeasureCell` asserts the unmatched species
lines word for word, and those lines grew a column. Both its expectations took `(empty)` in the
new nearest-name position, written out by hand: the fixture's list holds only Albizia lebbeck,
which shares no opening letter with BAUHINIA PURPUREA or with UNKNOWN, so the nearest name for
both is nothing. Nothing else in the suite was touched.

### STILL NEVER EXERCISED

**Street Design counting as Proposed on STREETS has tests and no run.** The words appear nowhere
in the 2,292 line report of the 16:06 STREETS run, which is the only STREETS run there has been,
and they could not: no link was loaded, so no schedule printed a body, so no group row of any
name was found. `CountedGroups.Of` reading `KpiTemplate.GroupsCountedAsProposed`, the
`GroupByDecision` pointing at Tree List - Proposed, `SheetFor` answering by decision second, and
`KpiMerge.Species` keying on the sheet that takes the group are all covered by tests and none of
it has ever met a real street schedule. ST-05's numbers, 369 existing and 2 plus 68 proposed
against a printed TOTAL of 439, are still the only measurement behind the whole rule and they
were read off a screen rather than off a run.

### What this round did not touch

The 36 open audit findings stay open. Nothing about the softscape or shrubs and lawn readers
moved, nothing about the patcher moved, and the matching rule itself is exactly what it was.

---

## 2026-09-12, fifty eighth pass. Create scans if it needs to, and the scan button is gone

Item 1 of two, on its own as Bader asked, so that if the first real run breaks nobody has to
work out which half did it. Item 2, several templates in one run, is a round of its own and is
NOT in this one. The branch came off a fresh pull of main at `b5e2c90`, so the baseline is
**1282 tests, 700 of them KPI**. **Nothing in this round has been observed in Revit**, and the
1552 run's correct workbook is what it must not cost: 36 parts, calcChain removed, calcId 0,
calcMode auto, no cached value left, zero errors on recalculation, Total Green cover 13,516,
Canopy 11,168, Total Trees 374, Planting 1,493, Lawn 855 off 70,343.

Pull request 93, merged into main as `ffccfce`. **The runner executed 1289 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1289 ran at `ffccfce`, 0 failed and 0
skipped, 707 of them KPI.** **The branch itself carried 1289, 7 added here**, against the 1282
main held at the branch point `b5e2c90`, and the merged tree is byte for byte the branch head,
nothing landed in between. Build zero warnings at the merge. The merge went through the API
with the title and the message both passed on the call, and the commit came back off main
carrying neither a co-author credit line nor a generated-by footer.

### The scan is a step inside Create

`ScanNeeded.Decide` in Core is the whole rule and it is the same shape `HeldReadings.Decide`
already uses for the per plot readings, because two shapes for one kind of question would be
two rules to keep in step. The model is read when no scan is held or what is held is of
another model, and a scan already held answers a second press on the same model. The handler
holds the title its scan was read from and Create calls `Scan` itself before it reads a plot.
Nothing about the reading reuse moved: `HeldReadings` decides that separately and the 2.5
second finish after a 123 second read is that working, untouched.

The press says which of the two it did, `Reusing the scan already held` or the real read's own
section lines, for the same reason the readings say it. **The scan's headline moved from
`Told` to `Progressed`**, which matters: `Told` is the run's end line and it is what shuts the
progress window, so the headline said there would have ended the press on screen, and closed
the window, with the workbook still being filled.

### The button, and where the header's numbers come from now

`KPI Scan` is off the pane's strip and `AskedForAScan` is deleted, the `Scan` request is off
the enum and out of the switch, and the ribbon tooltip no longer describes a button that is
not there. Four code comments naming it are corrected and two test files pinned its old
wording, both updated by hand.

**The header's element count now comes back with the plot read** rather than off the scan.
`KpiPlotFacts` carries the count and the seconds, `ReadThePlots` counts instances the same way
the scan counts them, and `Found` fills the line. So the model's name has a number under it as
soon as the pane is shown, which is what the scan line used to do only after a press. That
read is once per model already, guarded by the title the plots were last asked for, so the
count costs one collector pass per model rather than one per redraw.

### The progress window

`KpiProgressWindow`, modeless, owned by the Revit main window through its handle, showing the
same `ProgressWords` lines the status line shows. **Opened by the pane before the external
event is raised and closed when the run's last answer comes back, never from inside
`Execute`**, which is the hazard the fifty fifth pass's breaker caught when the repaint pump
was pumping at a priority that let queued clicks run inside the handler's call frame. `Told`
closes it, being the end line whatever ended the run, a finish, a refusal or a throw.

**There is no Cancel, deliberately, and this is the round's one refusal to build something.**
Bader asked for one if it is cheap. It is not: cancelling mid read leaves a half read set of
plots that the reconciliation would count as plots read, which is exactly the half read state
the rest of the tool would trust. An honest cancel is a cancellation threaded through every
reader plus a discard of everything read, which is its own round. A cancel that lies is worse
than no cancel.

### Break watches

Three, each restored byte for byte and checked with cmp, the suite rebuilt and rerun green at
1289.

- a held scan of another model taken for this one: **3 red**, the other model case, the exact
  title comparison and the no document case
- nothing held taken as a scan of this model: **1 red**, the first press on a fresh pane
- the pane words naming the vanished button again: **1 red**, the fixed lines test

### Existing tests changed

Two, both by hand. `KpiPaneWordsTests` pinned `Not scanned yet. Press KPI Scan.` and the
`ReadOnly` line naming the button, and gained a line holding that none of the three fixed
lines names it. `KpiCreateTests` pinned the `NoPlots` line telling somebody to press it. No
test moved for any other reason.

`PaneLabelTests` at the tests root also holds the string KPI Scan, and it is LEFT ALONE: it
is not KPI's file, and it uses the words as input to the caption escape rather than asserting
any button exists.

### The diff read adversarially, and what it found

A breaker was set on the diff and died on a rate limit before reporting, so the checks were
made here instead, at today's lines. Four came back clean and one is a real limit.

- **Every path that ends a press reaches `Told`, so the window always shuts.** Run's four
  typed catches, the `document == null` return, the `CannotCreate` return and Execute's own
  `Stop(failed)` all invoke it, `Stop` inside a try of its own so even a throw on the way out
  cannot skip it. That was the worst case worth ruling out: a modeless window left over Revit
  with no way to shut it
- **The scan runs after the refusal guard**, so a press refused for no template, no plot or no
  output folder does not spend two minutes reading the model first
- **The header cannot be wiped by the redraw that follows it.** `Found` sets `_scannedTitle`
  off `_model.Title`, and `_model` is always set first, because `Named` is invoked before the
  switch that reaches `ReadThePlots`. The next `Took` then compares equal and resets nothing
- **Nothing still references the removed request.** The two `AskedForAScan` hits left in the
  tree are the Drawing Sheet's own, in its own file, untouched

**The one real limit, known and left: a second press of Create while the first is still
running leaves the second run without a window.** `Shut` closes the first window before the
second opens, so no window is orphaned over Revit, but the first run's end line then shuts the
second run's window and the second run goes on unseen. A busy flag on the press would fix it
and would introduce a worse failure: a flag that fails to clear leaves Create dead with no way
back, and the pane has no way to ask whether a run is still going. The same reasoning as the
cancel. It is written down rather than guarded, and the first run is what says whether anybody
presses twice.

### What is open

Whether the window appears where it should, whether it shuts on every path, and whether the
status line moves mid run are all things only a run on a real pane can show. The scan report's
own location now shows in the progress line while the run goes rather than sitting on the
status line at the end, because the end line belongs to Create. Whether the team wants it to
persist somewhere after the press is a question for Bader.

---

## 2026-09-12, fifty seventh pass. The plots block that said open a model, and the ten warnings

Two things off the 13:48 run on RCRC_NG03_EZ. The branch came off a fresh pull of main at
`aa0f546`, the fifty eighth pass, so the baseline is **1288 tests, 699 of them KPI**, not the
1266 Bader's message names: the Drawing Sheet rounds `#82`, `#83`, `#84` and `#88` landed
since the fifty sixth pass and none of them is KPI. **Nothing in this round has been observed
in Revit.** The first of the two changes the pane's ask flow, which no Core test can reach, so
it is recorded here for the first run on a real pane. Committed in two, the warnings first as
a checkpoint, then this.

Pull request 92, merged into main as `c8bd1b8`. **The runner executed 1282 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1282 ran at `c8bd1b8`, 0 failed and 0
skipped, 700 of them KPI.** **The branch itself carried 1289, 1 added here**, against the 1288
main held at the branch point `aa0f546`. The drop from 1289 to 1282 is the Drawing Sheet
sixtieth pass `a7cd601` that landed in between, which deleted its marker family and net
removed seven tests, none of them KPI, so the KPI count is 700 on the branch and at the merge
alike, checked. The merge went through the API with the title and the message both passed on
the call, and the commit came back off main carrying neither a co-author credit line nor a
generated-by footer. Ten warnings cleared, build zero warnings.

### The plots block said open a model while a model was open

At 13:48 the header named RCRC_NG03_EZ and the footer carried its scan counts, and the plots
block read Open a model with no list, while the same model had listed 18 plots at 12:08 on the
build before. A workflow of three investigators and a judge told the two candidates Bader
named apart from the real cause, and neither candidate was it.

- **Not the repaint pump moving from background to render.** Its premise is false: git shows
  the pump was born at render in the fifty fifth pass and never ran at background, so nothing
  moved. `_facts` has one writer, reached by the priority-less `Dispatcher.Invoke` overload
  that runs inline or marshals at Send, above any pump priority, so no pump can gate it. And
  the build before the pump listed the plots with no pump of any priority at all. Told apart
  by git history and by the single writer.
- **Not the progress line's redraws firing between the ask and the answer.** Those pumps run
  only inside a scan or a create `Execute`, and every `Execute` empties the one request slot at
  its start, so a pending `Plots` cannot be in the slot while a pump runs. Told apart by when
  the pump can run against when the `Plots` request lives.

The real cause is older than both and the round's diff never touched it. `Ask(Plots)` lived at
one site, `Shown`, fired only by a visibility rise. Installing the fifty fifth and fifty sixth
passes forced a Revit restart, a pane restored visible at startup was shown before any document
existed, that one ask was consumed against No open document, and nothing ever asked again: every
later answer runs `Took`, whose redraw asks only `WhichModel`, which reads the plots nowhere. So
a scan filled the header while `_facts` stayed null and the block drew its waiting text, whose
words were Open a model. A KPI Scan pressed before the plot read returned loses the ask the same
way, displacing the pending `Plots` in the one slot. Which of the three happened at 13:48 is a
visibility and timing fact only the user's memory or a run can settle, and it is UNKNOWN, the
restart being the most economical.

**The fix is two halves.** The message, in Core with the test: `CreateWords.PlotsBlock` reads
four ways, one line each, no document says open one, a document not yet answered says it is
reading the plots, a document answered with no plots says the model holds none, and a document
answered with plots hands to `PlotSources`. Open a model is right only where no model is open,
and the not answered state is told from it by whether a document is open, read off the live
document, never a held copy. The recovery, in the pane and untestable in Core: `Ask(Plots)`
moved out of `Shown` into `Took`, the one place that knows which model answered, guarded by the
title the last ask was for and reset on close, so the plots are read once per model and a model
opened under the pane or one whose first ask was lost is read the moment any request answers
with it. `Shown` asks only for the model now. This also killed a latent double read: `Shown`
and `Took` both asking would have read the plots twice on every show.

### The ten xUnit warnings, cleared

All ten were the same mistake in two shapes, asserting on a filtered collection rather than the
condition, so the message named the filtered length rather than what it found.

- **Four xUnit2029**, `Assert.Empty(x.Where(p))` to `Assert.DoesNotContain(x, p)`: KpiCreateTests
  681 (no Folder property on OpenModel), CanopyColumnsTests 283 (no skip on the proposed sheet)
  and 308 (no write on it), WorkbookFormulasTests 137 (no formula at risk is an error)
- **Six xUnit2031**, `Assert.Single(x.Where(p))` to `Assert.Single(x, p)`, the filtering overload
  returning the one element typed: CanopyColumnsTests 310 and 348, DiameterColumnTests 186,
  TreeListRowsTests 258, 480 and 481

The assertion changed and never the subject. None changed what it checks: the `DoesNotContain`
overload takes the same predicate, and the `Single` filtering overload returns the same typed
element the chain did, which every call site still assigns and reads. Committed first as a
checkpoint, `54ab011`, so the tree was not left dirty while the workflow ran.

### Break watches

Two, each restored byte for byte and checked with cmp, the suite rebuilt and rerun green at 1289.

- the block says open a model whether or not one is open, the regression itself: **1 red**, the
  document open and not answered case
- the block says reading even with no document: **1 red**, the no document case, so both halves
  of the distinction are shown load bearing

### Existing tests changed

None. The warnings pass rewrote ten assertions and added none. The plots pass added one Core
test, `ThePlotsBlockReadsFourWaysOneLineEach`, and moved no other.

---

## 2026-09-12, fifty sixth pass. The coarse step is said before any number, and the pane counts the notes

Two lines off Bader's answers to two of the fifty fifth pass's four open questions, and no
behaviour changes beyond them. Nothing else was touched: not the Drawing Sheet, not
`Core/Shared`, not `CLAUDE.md`, not `PanelTheme`, `PanelMetrics` or `ReportFile`. **Nothing
in this round has been observed in Revit.** The branch came off a fresh pull of main at
`32cbdf1`.

Pull request 85, merged into main as `52e4395`. **The runner executed 1266 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1266 ran at `52e4395`, 0 failed and 0
skipped**, 699 of them KPI. **The branch itself carried 1266, 5 added here**, against the
1261 main held at the branch point `32cbdf1`, and the merged tree is byte for byte the
branch head, nothing landed in between. The merge went through the API with the title and
the message both passed on the call, and the commit came back off main carrying neither a
co-author credit line nor a generated-by footer.

### No ceiling on the rounding room, a line instead

Both measured models round areas to 1, so a coarser step is hypothetical and a ceiling chosen
today is a constant pretending to be a rule, the fault the fifty fifth pass's own breaker
caught in that same round. Instead, a project whose step is coarser than the metre opens the
create report with one line before anybody reads a number: THE PROJECT ROUNDS AREAS COARSER
THAN THE METRE: its step is 2, so the group total check allows 1 square metre of room for
every row summed. `KpiCreateRun` carries the `ProjectUnit` whose step gated the checks. The
handler passes it on a fresh read and carries the held run's forward on a reused press,
because the notes were earned against that unit and a step read again since could have
changed. The metre, anything finer and an unread step print nothing, tested each way with the
step of 2, the step of 10 and its 5 in the plural written out by hand.

### The pane says the notes exist

A note only the report file holds is a note nobody reads. `CreateWords.Wrote` counts the
readings' rounding notes and puts one sentence beside the written count, 2 rounding notes are
in the report, singular when one, nothing when none. The detail stays in the report, where
each note sits beside its group total row. FM-21 and FM-22 off the 1208 run are the test's
two notes.

### Break watches

Two, each restored byte for byte and checked with cmp, the suite rebuilt and rerun green at
1266.

- the coarse step line never printed: **2 red**, the step of 2 and the step of 10
- the note count never spoken: **1 red**, the status line counting FM-21 and FM-22

### Existing tests changed

None. `CreateFixture.Run` gained an optional area unit it passes through, every existing call
unchanged, and `KpiCreateRun` gained the unit as an optional trailing argument that defaults
to the unread unit, so no caller moved.

---

