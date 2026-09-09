# Reports

Every report the add-in writes lands here as well as on the Desktop, with the same file name,
so a run is findable by date and model in either place.

## Nothing in here is ever committed

`.gitignore` keeps this folder empty as far as git is concerned, and this README is the only
file in it that is tracked. That is deliberate and it is not tidiness.

**This repository is public.** A report carries client view names, sheet numbers, plot
identifiers, scope box names and the full field list of every schedule it touched. That is
project information belonging to the client, and none of it goes anywhere near a public
remote. Read the files here locally and leave them here.

If you ever need to share what a run found, quote the numbers rather than attaching the file.

## How the add-in knows where this is

Revit runs the add-in out of `%APPDATA%\Autodesk\Revit\Addins\2024\` and has no idea where
this repo sits. `install/install.ps1` writes the absolute path of this folder into
`reports-folder.txt` next to the installed assembly, and the add-in reads it from there.

No pointer file means the reports go to the Desktop only, and the panel says so rather than
leaving somebody looking for a file that was never written. Running `install.ps1` again puts
it back.

## What lands here

- `RCRC-Green-Scan_<model>_<date>_<time>.txt` from Scan Model
- `RCRC-Green-ScopeBox_<model>_<date>_<time>.txt` from Assign Scope Boxes
- `RCRC-Green-Run_<model>_<date>_<time>.txt` from Run
- `RCRC-Green-KPI_<model>_<date>_<time>.txt` from KPI Scan

A run report is worth reading top to bottom even when it went well, because the section headed
CREATED WRONG AND STILL IN THE MODEL is the only place a schedule that has to be deleted by
hand is named.
