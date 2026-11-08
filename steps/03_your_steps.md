# 03 your steps

One pass in Revit 2024 for the STREETS H8 option, with the rebuilt Setup.exe. Take one screenshot
at every step marked Screenshot. The Setup.exe is the one file named `RCRC Green KPI Setup` and a
version in `C:\Users\bader\Documents\RCRC-Green-Team-Kit`, built from main after this round.

## Install

1. Close every Revit window.
   Look for: no Revit in the task bar.
2. Double click the Setup.exe. If a blue box says Windows protected your PC, choose More info,
   then Run anyway. Press Next, then Install, then Finish.
   Look for: `This installs RCRC Green KPI` and the version on the Ready page. Screenshot.
3. Start Revit 2024, open NG05, press KPI Checklist on the RCRC Green tab and open step 1 Setup.
   Look for: a row `STREETS H8` with `Intervention area` ticked and `ROW x Length` not. Screenshot.

## ROW x Length

4. Tick `ROW x Length`, close the pane and open it again.
   Look for: `ROW x Length` still ticked.
5. Open step 2 and press Read model.
   Look for: Templates matched.
6. In step 3 tick one NH STREET plot and one WALKWAY plot. Write down their plot IDs. Create.
   Look for: 2 created and 0 failed, and no question about two regions. Screenshot.
7. Press Open report.
   Look for: the line under the folder layout line reads `STREETS H8: ROW x Length, the formula =F8*D8.` Screenshot.
8. Search the report for the street plot's area line.
   Look for: `area: from ROW x Length`, the width, the length and their product.
9. Search the report for the walkway's area line.
   Look for: `ROW x Length could not be worked out` naming the walkway's missing length. Screenshot.
10. Open the street plot's workbook, first sheet, and click H8.
    Look for: the formula bar reads `=F8*D8`, and H8 shows D8 times F8, the same number as the
    report's product. Screenshot.
11. Open the walkway's workbook, first sheet, and click H8.
    Look for: the formula bar reads `=F8*D8`, D8 holds the width, F8 is empty and H8 shows 0.
    That is Bader's rule with no usable length, Q22. Screenshot.
12. Open the street plot's PDF.
    Look for: the Roads form with Row and Length filled. It has no Area box, Q23. Row times
    Length in metres is H8. Screenshot.
13. Open the walkway's PDF.
    Look for: the Roads form with Row filled and Length blank. It has no Area box either.

## Intervention area

14. Open step 1 and tick `Intervention area`.
    Look for: `Intervention area` ticked.
15. In step 3 tick only the street plot and Create.
    Look for: 1 created. If its regions differ, the pane asks which, as it did before this round.
16. Press Open report.
    Look for: the line under the folder layout reads `STREETS H8: Intervention area, the Revit area off the 00 link.` and the plot's area line
    names its filled region type and area.
17. Open that workbook, first sheet, and click H8.
    Look for: a plain number, the Revit area, and no formula in the formula bar. Screenshot.
18. Leave `Intervention area` ticked unless the team should fill by ROW x Length.
    Look for: the pick you want stays ticked after you close and open the pane.
