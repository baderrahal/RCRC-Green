<#
.SYNOPSIS
Installs RCRC Green KPI for this Windows user from the extracted team zip it sits in, or puts
back the version an install replaced.

.DESCRIPTION
Started by "Install RCRC Green KPI.cmd" in Windows PowerShell 5.1 with the script policy
bypassed for that one run. Needs no admin rights: everything it writes is under this user's
own profile.

  %APPDATA%\Autodesk\Revit\Addins\2024\RcrcGreen.addin
  %APPDATA%\Autodesk\Revit\Addins\2024\RcrcGreen\         the assemblies, features.txt, VERSION.txt
  %APPDATA%\RCRC Green\previous-install\                  the version this install replaced
  %USERPROFILE%\Documents\RCRC Green\reports\             where the pane writes its reports

The folder the kept version sits in is outside Addins\2024 on purpose, so Revit can never find
a second manifest and load two copies.

It never touches the files that remember what somebody chose in the pane: the templates
folder, the output folder, the PDF forms folder, the street reference file, the plot list and
the prepared by names. None of them is in the zip, and none is copied, kept or put back.

Every run ends on one line starting DONE, FAILED or REFUSED that says what to do next.

.PARAMETER PutBack
Puts back the version the last install replaced.

.PARAMETER CheckOnly
Checks the bundle beside this script and stops. build-team-kit.ps1 runs it before it writes a
zip, so the reference check is written once.
#>
[CmdletBinding()]
param(
    [switch] $PutBack,
    [switch] $CheckOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$here = $PSScriptRoot
$addins = Join-Path $env:APPDATA 'Autodesk\Revit\Addins\2024'
$installed = Join-Path $addins 'RcrcGreen'
$manifest = 'RcrcGreen.addin'
$kept = Join-Path $env:APPDATA 'RCRC Green\previous-install'
$keptList = Join-Path $kept 'kept-files.txt'
$reports = Join-Path $env:USERPROFILE 'Documents\RCRC Green\reports'

# What this kit puts in the installed folder, and therefore all it keeps and puts back.
$needed = @('RcrcGreen.Revit.dll', 'RcrcGreen.Core.dll', 'features.txt', 'VERSION.txt')
$optional = @('RcrcGreen.Revit.pdb', 'RcrcGreen.Core.pdb')
$owned = $needed + $optional

# Assemblies Revit 2024 loads itself. The add-in must never carry its own copy of these.
$fromRevit = @('RevitAPI', 'RevitAPIUI', 'AdWindows', 'UIFramework', 'RevitAPIIFC')

function Finish([string] $word, [string] $text, [int] $code) {
    Write-Host ''
    Write-Host "${word}: $text"
    exit $code
}

function Refuse([string] $text) {
    Finish 'REFUSED' "$text Nothing was changed." 2
}

function VersionIn([string] $folder) {
    $file = Join-Path $folder 'VERSION.txt'
    if (Test-Path -LiteralPath $file) {
        $first = Get-Content -LiteralPath $file -TotalCount 1
        if ($first) { return $first.Trim() }
    }
    $dll = Join-Path $folder 'RcrcGreen.Revit.dll'
    if (Test-Path -LiteralPath $dll) {
        $stamp = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dll).ProductVersion
        if ($stamp) { return "RCRC Green build $stamp" }
        return 'RCRC Green, a build with no stamp'
    }
    return $null
}

# A reference satisfied by neither this bundle, the .NET Framework nor Revit fails at load time
# with a message that names no file, so it is refused here instead.
function MissingReferences([string] $folder) {
    $missing = New-Object System.Collections.Generic.List[string]
    foreach ($name in @('RcrcGreen.Revit.dll', 'RcrcGreen.Core.dll')) {
        $assembly = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom((Join-Path $folder $name))
        foreach ($wanted in $assembly.GetReferencedAssemblies()) {
            if (Test-Path -LiteralPath (Join-Path $folder ($wanted.Name + '.dll'))) { continue }
            if ($fromRevit -contains $wanted.Name) { continue }
            try {
                [void][System.Reflection.Assembly]::ReflectionOnlyLoad($wanted.FullName)
            }
            catch {
                $missing.Add("$name needs $($wanted.FullName)")
            }
        }
    }
    return $missing
}

if ($here -match '\.zip(\\|$)') {
    Refuse 'This was started from inside the zip. Right click the zip, choose Extract All, and double click Install RCRC Green KPI.cmd in the folder that makes.'
}

if ($PSVersionTable.PSEdition -ne 'Desktop') {
    Refuse 'This needs Windows PowerShell 5.1, which Install RCRC Green KPI.cmd starts. Double click that file rather than running this script.'
}

if ($CheckOnly) {
    $absent = $needed + @($manifest) | Where-Object { -not (Test-Path -LiteralPath (Join-Path $here $_)) }
    if ($absent) { Finish 'FAILED' "The bundle is missing $($absent -join ', ')." 1 }
    $missing = @(MissingReferences $here)
    if ($missing.Count -gt 0) { Finish 'FAILED' "A reference is in neither the bundle, the framework nor Revit: $($missing -join '. ')." 1 }
    Finish 'DONE' 'The bundle holds every file and every reference is satisfied.' 0
}

if (Get-Process -Name 'Revit' -ErrorAction SilentlyContinue) {
    Refuse 'Revit is running. Save your work, close every Revit window, and double click the same file again.'
}

if ($PutBack) {
    if (-not (Test-Path -LiteralPath $keptList)) {
        Refuse "No previous version is kept in $kept, so there is nothing to put back."
    }

    $was = VersionIn $installed
    $back = VersionIn $kept
    try {
        $had = @(Get-Content -LiteralPath $keptList | Where-Object { $_ })
        foreach ($name in $owned) {
            $target = Join-Path $installed $name
            if ($had -contains $name) {
                Copy-Item -LiteralPath (Join-Path $kept $name) -Destination $target -Force
            }
            elseif (Test-Path -LiteralPath $target) {
                Remove-Item -LiteralPath $target -Force
            }
        }
        if ($had -contains $manifest) {
            Copy-Item -LiteralPath (Join-Path $kept $manifest) -Destination (Join-Path $addins $manifest) -Force
        }
        Remove-Item -LiteralPath $kept -Recurse -Force
    }
    catch {
        Finish 'FAILED' "Putting back stopped part way: $($_.Exception.Message) The add-in folder $installed may hold a mix of two versions. Take a screenshot of this window and send it to Bader." 1
    }

    if (-not $back) { $back = 'the version kept before' }
    if (-not $was) { $was = 'nothing' }
    Finish 'DONE' "$back is back, in place of $was. The pane keeps every folder you chose. Start Revit 2024 and open the RCRC Green tab." 0
}

$absent = $needed + @($manifest) | Where-Object { -not (Test-Path -LiteralPath (Join-Path $here $_)) }
if ($absent) {
    Refuse "This folder is missing $($absent -join ', '). Right click the zip, choose Extract All, and double click Install RCRC Green KPI.cmd in the folder that makes."
}

# A DLL saved from a browser or Teams carries a mark that stops .NET loading it, and Revit then
# says only that the add-in could not be loaded. Taking the mark off first costs nothing.
Get-ChildItem -LiteralPath $here -File | Unblock-File

$missing = @(MissingReferences $here)
if ($missing.Count -gt 0) {
    Refuse "This zip is incomplete: $($missing -join '. '). Send this window to Bader."
}

$version = VersionIn $here
$previous = VersionIn $installed

try {
    New-Item -ItemType Directory -Path $installed -Force | Out-Null
    New-Item -ItemType Directory -Path $reports -Force | Out-Null

    if ($previous) {
        if (Test-Path -LiteralPath $kept) { Remove-Item -LiteralPath $kept -Recurse -Force }
        New-Item -ItemType Directory -Path $kept -Force | Out-Null
        $had = New-Object System.Collections.Generic.List[string]
        foreach ($name in $owned) {
            $from = Join-Path $installed $name
            if (Test-Path -LiteralPath $from) {
                Copy-Item -LiteralPath $from -Destination $kept -Force
                $had.Add($name)
            }
        }
        $from = Join-Path $addins $manifest
        if (Test-Path -LiteralPath $from) {
            Copy-Item -LiteralPath $from -Destination $kept -Force
            $had.Add($manifest)
        }
        [System.IO.File]::WriteAllLines($keptList, $had)
    }

    foreach ($name in $owned) {
        $from = Join-Path $here $name
        $target = Join-Path $installed $name
        if (Test-Path -LiteralPath $from) {
            Copy-Item -LiteralPath $from -Destination $target -Force
        }
        elseif (Test-Path -LiteralPath $target) {
            # A symbol file the older version carried and this one does not would give a stack
            # trace line numbers from the wrong build.
            Remove-Item -LiteralPath $target -Force
        }
    }
    Copy-Item -LiteralPath (Join-Path $here $manifest) -Destination (Join-Path $addins $manifest) -Force

    # The pane writes its reports where this file says. A file already naming a folder is left
    # as it is, so a machine that sends its reports elsewhere keeps doing so.
    $pointer = Join-Path $installed 'reports-folder.txt'
    $reportsGo = ''
    if (Test-Path -LiteralPath $pointer) { $reportsGo = (Get-Content -LiteralPath $pointer -Raw) }
    if (-not $reportsGo -or -not $reportsGo.Trim()) {
        [System.IO.File]::WriteAllText($pointer, $reports)
        $reportsGo = $reports
    }
    $reportsGo = $reportsGo.Trim()

    # The two the pane expects to find, made empty only where missing, which is what a fresh
    # install needs and what leaves a folder somebody already chose exactly as it was.
    foreach ($name in @('templates-folder.txt', 'kpi-output-folder.txt')) {
        $file = Join-Path $installed $name
        if (-not (Test-Path -LiteralPath $file)) { [System.IO.File]::WriteAllText($file, '') }
    }
}
catch {
    $where = if ($previous) { "The version you had before is kept in $kept, so double click Put back the previous version.cmd." } else { 'There was no version before this one.' }
    Finish 'FAILED' "Installing stopped part way: $($_.Exception.Message) The add-in folder $installed may hold a mix of two versions. $where Then send Bader a screenshot of this window." 1
}

$before = if ($previous) { "The version you had before, $previous, is kept in $kept." } else { 'There was no version before this one.' }
Finish 'DONE' "$version is installed for $env:USERNAME. $before Reports go to $reportsGo. Start Revit 2024 and open the RCRC Green tab." 0
