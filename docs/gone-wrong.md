## Things that have gone wrong before

Add to this whenever something breaks. It is the only part that cannot be rediscovered by
reading the code.

**A guard that fails open reads exactly like a guard that passed.** `writing-check.sh` split
its file list on newlines, so a name holding a space reached the scanner in pieces that each
read as a file that is not there. It passed unchecked. A check that cannot see its own subject
has to refuse. The KPI scan names every read that did not happen at the top of its file.

**The same hook, the same shape, a second time.** A commit whose message arrives on standard
input, through `-F -` and a heredoc, puts it somewhere a hook cannot reach: the hook runs
BEFORE the command, so the heredoc has not been written yet. `commit-scope.py` skipped a
message file named `-` with a bare continue, `writing-check.sh` scanned an empty string, and a
commit carrying a co-author credit line was taken in silence on 14 September. **Write a commit
message to a file in one call and name that file on the next.** Standard input is refused now,
by name and with that remedy, and so is a command line that cannot be read as a shell line at
all, which used to be answered with whatever happened to be sitting in the index.

**Set what is needed twice and the second one drifts.** `commit-scope.py` carried a `found`
flag whose whole purpose was to say the command could not be read, and its own comment said to
let the caller fail closed. No caller ever read it. The fix is that the script itself fails,
because all three hooks already refuse on that, rather than a fourth place deciding again.

**A signal that travels in the data is not a signal.** The first fix for the above reported an
unreadable message by printing a marker word INTO the message, which is what the older
unreadable-file case already did, and the hook searched the message for it. So a message that
merely talks about the problem is indistinguishable from one that has it: the commit carrying
that fix was refused by its own message, for naming the marker it introduced. The reason comes
back beside the text now, as a non-zero exit with the reason on standard error, and every hook
already refuses on a non-zero exit.

**A wall built on one machine is not a wall on another.** Run on Windows on 28 October, every
hook failed: `python3` was the Store alias, `block-paths.sh` judged inside the repo by a forward
slash so every write read as outside it, the two hooks that start `commit-scope.py` from inside
Python reached the alias again, and a message printed down a Windows pipe lost its em dash, so
`an em dash is refused` passed the dash through once the test could read its file. The
comparison is `commonpath` now, the scope script is started with `sys.executable` and writes
UTF-8, and `hook-tests.sh` reads 28 of 28 on Windows. `python3` itself is a shim in
`C:\Users\bader\bin` on Bader's machine, outside this repo.

**A guard wired to a tool it cannot read is not wired to it.** `block-paths.sh` is matched on
Write, Edit AND NotebookEdit and read `file_path` only. NotebookEdit's argument is
`notebook_path`, so every notebook write reached an empty string and took an `exit 0` meant
for a call with no path at all. Measured: a write to `/etc/evil.ipynb` came back exit 0 where
the same path under `file_path` came back exit 2. It reads both keys now, and a call carrying
neither is refused rather than waved through. Check what a tool actually sends before trusting
a matcher to line up with it.

**Two records of one fact, in the file that decides who may touch what.** The task list sat in
`territory-check.sh` and again in `.claude/rules/territory.md`, and they came apart: the hook
named six tasks and the file numbered five, missing View Filters, whose code was already in all
three roots. A task the wall does not know about is a task any session can edit. The list is
`.claude/hooks/tasks.txt` now, the hook reads it, the rules file points at it, and a hook case
refuses when the two disagree. A wall that cannot read its own list refuses rather than
treating every path as common.

**An assumption held for five rounds because nobody ran the thing.** The naming pattern came
from four examples, the read was assumed to need a progress window, and PRX_Plot_ID was assumed
to be on elements only. One run corrected all three. Prefer measured numbers to reasoning.

**Some faults only show when somebody uses the thing.** The panel shipped with every heading
written and none visible, black on black. Later it was correct and unusable: ticking a view type
scrolled the list away, marking 136 cells took 136 clicks, and eight columns ran off the right
edge. Neither round shows in a test or in a mockup.

**Two sources for one fact is two facts.** Eight times now, always the same shape. A view named
for DM-12 filled a DM-11 cell that stayed full after every DM-11 view was deleted. The column
count against its list. The run report, which made nothing and named four views under PLAN VIEWS
and under NOT CREATED. A panel step reading the range off its arguments. One method filling both
the code buttons and the Add row's dropdown, split and half kept. A schedule category read one
way on capture and looked up another way on create. The softscape reader taught to read the
botanical name off the column the heading row names while the group counter that feeds the same
report was left reading the first cell, which is the image. Create handed the model's FOLDER to
the words that say whether a model is open, so a detached model that has never been saved was
refused with No model is open beside a header counting its 96,959 elements.

**A skip with nothing written down is a lie by omission.** `ModelWriter` dropped a schedule
field it could not resolve, and a filter, both with a bare `continue`. A schedule short of a
column looks finished. One short of its plot filter reads as correct on a drawing. Every skip is
recorded now, a lost filter refuses the schedule, and that delete is checked.

**One example is not a rule, and the model may hold no rule at all.** The family type was
matched on the view type because one Properties panel showed a type named exactly that. The next
plot's is `(010) Key Location Plan`. Copying a sibling replaced it, and then three (010) views
copied three different family types. Annotation crop went the same way one round later. **The
worst of them was the shrubs and lawn subtotal**: DM-11 prints two equal rows per group, that
was read as one subtotal printed twice, and the tool took the first of the two for four rounds.
It is one subtotal per PHASE and then the group total, and DM-11 is the one plot where taking
the first cannot be wrong, because every one of its groups holds a single phase. The 0928 run
over 20 plots is what showed it. **One plot is not a sample.**

**A name is not an identity and a word is not a value.** Sheet Width was read off a title block
TYPE, where that instance parameter does not exist, so null became 0.0 and three sheets came out
empty. Slab Edges was looked up by display name in a collection that does not hold it, on a model
that has the category. A Yes/No filter was handed back the word Yes when Revit stores 1. Three
rounds, one shape. Ask the API what a thing IS rather than what it is called.

**Copying takes whatever state the thing is in, including nothing.** The first sheet the tool
made was empty, copied from one the user picked that had no views on it. Nothing failed and
nothing was reported. A sheet is described now, so what goes on it is stated.

**The interface can change a name on its way to the screen.** WPF reads the first underscore in
a button's text as an access key marker, swallows it and underlines the next letter, so the KPI
pane offered PRXComponent, PRXPlot_ID, PRXPlot_UID, PRXPlot_UID2 and PRXPlot_NH. Five names no
model holds, in a tool whose whole job is exact parameter names. The strings were right in the
code and wrong on screen, which no test of the code would ever have caught. What goes on a
button is escaped now and the escape has a test. The Drawing Sheet's tick boxes and its
scope box case buttons go through the same escape now.

**A copy taken once and never refreshed reads exactly like a fact.** The KPI pane held the
model's folder from the one read that happened when it was shown. The model was then saved to a
real folder and Create stayed grey saying No model is open, and a second scan did not shift it,
because nothing re-read the folder. The rule now: **A PANE HOLDS NO COPY OF ANYTHING IT CAN ASK
FOR.** What a button refuses on is read off the live document at the moment it is pressed, and
what the pane shows about the model is asked for again every time it draws. Greying a button out
on a fact the pane does not own is what turned one stale string into a dead end. The Drawing
Sheet panel then did the same with the whole model: its own run wrote sheets its snapshot never
learned about, and three runs in a row asked Revit for numbers the first had created, because
the run's only words were Press Refresh. A run hands back a fresh read now, the run checks its
numbers against the model rather than the panel, and a document opening under the pane triggers
a read of its own.

**A description of the tool is not the tool.** The pane said it reads PRX_COMPONENT and
PRX_Plot_UID2 off the title block. That was the workbook's own note copied onto the screen: the
reader had been corrected rounds before, PRX_COMPONENT is in no model, and neither value is read
off a title block anywhere. A line about what the tool does is checked against what it does.

**A setting nobody recorded cannot be argued about.** A created view came out with a template
that looked right and a family type that looked wrong. The code read both off one view four
lines apart, but nothing said which view. Record where a value came from as you use it.

