#!/usr/bin/env bash
# PreToolUse hook on Bash. The wall between tasks.
#
# Four tasks share this repo and the sessions building them cannot see each other. A rule
# in CLAUDE.md or .claude/rules/territory.md is a request and can be reasoned around. This
# cannot. It refuses a commit that touches more than one task's territory, and a commit
# that puts a Core/Shared change next to any task's work, because a Shared change runs
# alone while every other session is stopped.
#
# The map lives here rather than being read off the folder tree, because a folder that is
# not a task, such as Properties, must never be mistaken for one. A new task adds itself
# to TASKS below on its first round, next to its folders.

set -euo pipefail

INPUT=$(cat)
COMMAND=$(printf '%s' "$INPUT" | python3 -c "import sys,json; print(json.load(sys.stdin).get('tool_input',{}).get('command',''))" 2>/dev/null || echo "")

case "$COMMAND" in
  *"git commit"*) ;;
  *) exit 0 ;;
esac

cd "$(git rev-parse --show-toplevel)"

SCOPE=".claude/hooks/commit-scope.py"
if [ ! -f "$SCOPE" ]; then
  echo "Refused. $SCOPE is missing, so what this commit carries could not be worked out." >&2
  exit 2
fi

export RCRC_COMMIT_COMMAND="$COMMAND"

read -r -d '' WALL <<'PY' || true
import os
import subprocess
import sys

TASKS = ["DrawingSheet", "Kpi", "CoordinationLayout", "BoqSchedules"]

TASK_ROOTS = [
    "src/RcrcGreen.Core/",
    "src/RcrcGreen.Revit/",
    "tests/RcrcGreen.Core.Tests/",
]

SHARED_PREFIX = "src/RcrcGreen.Core/Shared/"

# Drawing Sheet was built before the Revit project had a folder per task, so its Revit
# files sit at the project root. They are named here one by one so the wall still covers
# them. The root files NOT named here are the ones every task shares: the application
# class, the theme, the metrics, the report writer and the manifest.
DRAWING_SHEET_AT_REVIT_ROOT = {
    "src/RcrcGreen.Revit/AssignScopeBoxCommand.cs",
    "src/RcrcGreen.Revit/DrawingSheetPanel.cs",
    "src/RcrcGreen.Revit/DrawingSheetReader.cs",
    "src/RcrcGreen.Revit/DrawingSheetRequestHandler.cs",
    "src/RcrcGreen.Revit/IScanWatcher.cs",
    "src/RcrcGreen.Revit/ModelScanner.cs",
    "src/RcrcGreen.Revit/ModelWriter.cs",
    "src/RcrcGreen.Revit/ScheduleCapture.cs",
    "src/RcrcGreen.Revit/ScopeBoxScanner.cs",
    "src/RcrcGreen.Revit/SectionDefaults.cs",
    "src/RcrcGreen.Revit/SheetBeingDescribed.cs",
    "src/RcrcGreen.Revit/ShowDrawingSheetCommand.cs",
    "src/RcrcGreen.Revit/SiblingReader.cs",
    "src/RcrcGreen.Revit/TitleBlockSettingsStore.cs",
}


def territory_of(path):
    if path.startswith(SHARED_PREFIX):
        return "Shared"
    for task in TASKS:
        for root in TASK_ROOTS:
            if path.startswith(root + task + "/"):
                return task
    if path in DRAWING_SHEET_AT_REVIT_ROOT:
        return "DrawingSheet"
    return None


listed = subprocess.run(
    ["python3", ".claude/hooks/commit-scope.py", "all-paths",
     os.environ.get("RCRC_COMMIT_COMMAND", "")],
    capture_output=True)
if listed.returncode != 0:
    print("Refused. commit-scope.py could not work out what this commit carries.")
    sys.exit(1)

fields = listed.stdout.decode("utf-8", "replace").split("\0")
paths = [field for field in fields[1:] if field]

shared = []
tasks = {}
for path in paths:
    home = territory_of(path)
    if home == "Shared":
        shared.append(path)
    elif home is not None:
        tasks.setdefault(home, []).append(path)

if len(tasks) > 1:
    named = sorted(tasks)
    first = named[0] + " (" + tasks[named[0]][0] + ")"
    second = named[1] + " (" + tasks[named[1]][0] + ")"
    print("Refused. This commit touches two tasks' territory: " + first + " and " + second
          + ". One task per commit. A task needing a change in another task's files asks"
          + " the user for it instead.")
    sys.exit(1)

if shared and tasks:
    task = sorted(tasks)[0]
    print("Refused. " + shared[0] + " is in Core/Shared and this commit also touches "
          + task + " (" + tasks[task][0] + "). A change to Core/Shared runs alone,"
          + " with every other session stopped first, so commit the Shared change on"
          + " its own.")
    sys.exit(1)
PY

if python3 -c "$WALL" >&2; then
  exit 0
fi

exit 2
