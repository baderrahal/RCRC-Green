@echo off
rem Puts back the version kept before the last install, through Install RCRC Green KPI.cmd beside it, which
rem starts PowerShell the one way it is written and checks for the script it needs.
setlocal
set "here=%~dp0"
if not exist "%here%Install RCRC Green KPI.cmd" (
    echo REFUSED: this file was started from inside the zip, or the zip was not extracted whole. Right click the zip, choose Extract All, and double click this file in the folder that makes. Nothing was changed.
    echo.
    pause
    exit /b 2
)
call "%here%Install RCRC Green KPI.cmd" -PutBack
exit /b %errorlevel%
