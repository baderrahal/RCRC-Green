# 03 your steps

This pass installs the KPI team kit the way the team will, checks the pane in Revit 2024, then
puts your own full install back. Take one screenshot at every step marked Screenshot.

## Part 1, the kit

1. In GitHub Desktop, pick RCRC-Green, branch main, press Fetch origin, then Pull origin.
   Look for: the newest commit names Q6, Q7 and the KPI team kit.
2. In VS Code, File, Open Folder, `C:\Users\bader\Documents\GitHub\RCRC-Green`.
   Look for: `install\build-team-kit.ps1` in the Explorer.
3. Terminal, New Terminal.
   Look for: a prompt ending in `RCRC-Green>`.
4. Skip this step if `C:\Users\bader\Documents\RCRC-Green-Team-Kit` already holds the zip the
   round's final message named. Otherwise build it:

```
powershell -NoProfile -ExecutionPolicy Bypass -File .\install\build-team-kit.ps1
```

   Look for: a last line starting `DONE:` and naming `RCRC Green KPI <version>.zip`.
5. In File Explorer, open `C:\Users\bader\Documents\RCRC-Green-Team-Kit`, right click the zip,
   Extract All, Extract. Look for: a folder of 11 files with `Install RCRC Green KPI.cmd`.
6. Close every Revit window.
   Look for: no Revit in the task bar.
7. In the extracted folder, double click `Install RCRC Green KPI.cmd`.
   Look for: a last line starting `DONE:` that names the version and says your full install is
   kept in `RCRC Green\previous-install`. Screenshot.
8. Start Revit 2024 and open the RCRC Green tab.
   Look for: the KPI panel alone, with no Drawing Sheet and no View Filters. Screenshot.

## Part 2, the pane

9. Open NG05 and press KPI Checklist. Look for: `Build` in the header followed by the same
   hash and time as the zip's `VERSION.txt`. Screenshot.
10. Step 1, Setup. Look for: your templates, output and PDF forms folders already set, ticked
    in green. Screenshot.
11. Press Next: Read. Look for: the grey line `Reads sheets and schedules. Nothing changes.`
    above Read model. Screenshot.
12. Press Read model. Look for: the Plots, Elements and Time boxes. If the status line ever goes
    red on a read instead, press Read model again at once. Look for: it is not grey. Screenshot.
13. Step 3, Plots. Tick Mosques. Look for: about 20 plots ticked. Screenshot.
14. Press Next: Create, then Create, and wait. Look for: the result with Created and Failed and
    a row per asset type. Screenshot.
15. Press Open report. Look for: a first line `Build:` with the same hash and time. Screenshot.

## Part 3, your own install back

16. Close Revit, then double click `Put back the previous version.cmd` in the extracted folder.
    Look for: a last line `DONE:` saying your RCRC Green build is back.
17. In the VS Code terminal, build:

```
dotnet build src\RcrcGreen.Revit\RcrcGreen.Revit.csproj -c Release
```

   Look for: `0 Warning(s)` and `0 Error(s)`.
18. Install your full build:

```
.\install\install.ps1
```

   Look for: `Restart Revit 2024 and look for the RCRC Green tab.`
19. Start Revit 2024 and open the RCRC Green tab.
    Look for: three panels, Drawing Sheet, KPI and View Filters. Screenshot.
20. Send the screenshots, numbered by step, to the chat.
    Look for: nothing more to do until the next round.

## Not covered by this pass

- A team member's PC, with no repo and a Documents folder on OneDrive
- A refused read on demand: nothing in the pane can make Revit refuse one
