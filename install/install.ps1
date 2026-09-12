<#
.SYNOPSIS
Puts RCRC Green where Revit 2024 will find it, for the person running this and nobody else.

.DESCRIPTION
The manifest names the assembly as RcrcGreen\RcrcGreen.Revit.dll, a path read relative to the
manifest itself. The build puts the manifest and both assemblies in one flat folder, so the
subfolder has to be made here. Doing it by hand is the step people miss, and Revit answers a
missing assembly by not loading the add-in and not saying why.

Layout this produces:

  %APPDATA%\Autodesk\Revit\Addins\2024\RcrcGreen.addin
  %APPDATA%\Autodesk\Revit\Addins\2024\RcrcGreen\RcrcGreen.Revit.dll
  %APPDATA%\Autodesk\Revit\Addins\2024\RcrcGreen\RcrcGreen.Core.dll
  %APPDATA%\Autodesk\Revit\Addins\2024\RcrcGreen\reports-folder.txt
  %APPDATA%\Autodesk\Revit\Addins\2024\RcrcGreen\title-blocks.txt
  %APPDATA%\Autodesk\Revit\Addins\2024\RcrcGreen\sheet-names.txt
  %APPDATA%\Autodesk\Revit\Addins\2024\RcrcGreen\presets.txt
  %APPDATA%\Autodesk\Revit\Addins\2024\RcrcGreen\templates-folder.txt
  %APPDATA%\Autodesk\Revit\Addins\2024\RcrcGreen\kpi-output-folder.txt

.PARAMETER Configuration
Which build to install. Release unless you are debugging.

.PARAMETER BuildOutput
The folder holding the build output. Worked out from the repo layout when it is not given.

.EXAMPLE
.\install\install.ps1

.EXAMPLE
.\install\install.ps1 -Configuration Debug
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [string] $BuildOutput
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot

if (-not $BuildOutput) {
    $BuildOutput = Join-Path $repoRoot "src\RcrcGreen.Revit\bin\$Configuration"
}

$manifestName = 'RcrcGreen.addin'
$assemblyNames = @('RcrcGreen.Revit.dll', 'RcrcGreen.Core.dll')

if (-not (Test-Path -LiteralPath $BuildOutput)) {
    throw "Nothing to install. $BuildOutput does not exist. Build RcrcGreen.sln in $Configuration first."
}

$wanted = @($manifestName) + $assemblyNames
$missing = $wanted | Where-Object { -not (Test-Path -LiteralPath (Join-Path $BuildOutput $_)) }
if ($missing) {
    throw "Nothing to install. $BuildOutput is missing $($missing -join ', '). Build RcrcGreen.sln in $Configuration first."
}

$addinsFolder = Join-Path $env:APPDATA 'Autodesk\Revit\Addins\2024'
$assemblyFolder = Join-Path $addinsFolder 'RcrcGreen'

New-Item -ItemType Directory -Path $addinsFolder -Force | Out-Null
New-Item -ItemType Directory -Path $assemblyFolder -Force | Out-Null

$copied = New-Object System.Collections.Generic.List[string]

Copy-Item -LiteralPath (Join-Path $BuildOutput $manifestName) -Destination $addinsFolder -Force
$copied.Add((Join-Path $addinsFolder $manifestName))

foreach ($assembly in $assemblyNames) {
    Copy-Item -LiteralPath (Join-Path $BuildOutput $assembly) -Destination $assemblyFolder -Force
    $copied.Add((Join-Path $assemblyFolder $assembly))
}

# The symbol files are optional. Without them a stack trace from Revit has no line numbers,
# which is the difference between a report you can act on and one you cannot.
foreach ($symbols in @('RcrcGreen.Revit.pdb', 'RcrcGreen.Core.pdb')) {
    $from = Join-Path $BuildOutput $symbols
    if (Test-Path -LiteralPath $from) {
        Copy-Item -LiteralPath $from -Destination $assemblyFolder -Force
        $copied.Add((Join-Path $assemblyFolder $symbols))
    }
}

# Revit runs the add-in out of the Addins folder and has no idea where this repo is, so the
# path is written down here. It is the only thing that says where a report goes: without it
# nothing is written at all and the panel says so. The folder itself is in .gitignore and stays
# there, because a report carries client view names, sheet numbers and plot identifiers and this
# repository is public.
$reportsFolder = Join-Path $repoRoot 'reports'
New-Item -ItemType Directory -Path $reportsFolder -Force | Out-Null

$pointer = Join-Path $assemblyFolder 'reports-folder.txt'
Set-Content -LiteralPath $pointer -Value $reportsFolder -Encoding UTF8 -NoNewline
$copied.Add($pointer)

# Where the GRP KPI Checklist templates live. The user sets it with Browse in the KPI pane
# and the pane writes it here, so an install must not wipe a folder already chosen. The file
# is made empty only when it is missing, and empty means the pane says to set the folder.
# Which title block goes with which view type, the eleven the team picked by hand on the run
# of 2026-09-11. The panel reads this after the user's own file, so it is a starting point and
# never overwrites a choice: a changed pairing goes to %APPDATA%\RCRC Green\title-blocks.txt.
$titleBlocks = Join-Path $PSScriptRoot 'title-blocks.txt'
if (Test-Path -LiteralPath $titleBlocks) {
    Copy-Item -LiteralPath $titleBlocks -Destination $assemblyFolder -Force
    $copied.Add((Join-Path $assemblyFolder 'title-blocks.txt'))
}

# Which sheet name goes with which view type, the real names read off the two measured
# models. Same two-file rule as the title blocks: the panel reads this after the user's own
# file, so a name typed over in the panel is never overwritten by an install.
$sheetNames = Join-Path $PSScriptRoot 'sheet-names.txt'
if (Test-Path -LiteralPath $sheetNames) {
    Copy-Item -LiteralPath $sheetNames -Destination $assemblyFolder -Force
    $copied.Add((Join-Path $assemblyFolder 'sheet-names.txt'))
}

# The saved answers to steps 2 and 4, the six sheets of the DM-11 run. Same two-file rule
# again: the panel reads this after the user's own file, so a preset somebody saves in the
# panel is never overwritten by an install.
$presets = Join-Path $PSScriptRoot 'presets.txt'
if (Test-Path -LiteralPath $presets) {
    Copy-Item -LiteralPath $presets -Destination $assemblyFolder -Force
    $copied.Add((Join-Path $assemblyFolder 'presets.txt'))
}

$templatesPointer = Join-Path $assemblyFolder 'templates-folder.txt'
if (-not (Test-Path -LiteralPath $templatesPointer)) {
    Set-Content -LiteralPath $templatesPointer -Value '' -Encoding UTF8 -NoNewline
}
$copied.Add($templatesPointer)

# Where the filled workbooks go. Set with Browse in the KPI pane the same way, and left alone
# by an install for the same reason. It used to be the model's own folder, which meant a
# detached model could not be used at all.
$outputPointer = Join-Path $assemblyFolder 'kpi-output-folder.txt'
if (-not (Test-Path -LiteralPath $outputPointer)) {
    Set-Content -LiteralPath $outputPointer -Value '' -Encoding UTF8 -NoNewline
}
$copied.Add($outputPointer)

Write-Host "Installed RCRC Green from $BuildOutput"
foreach ($file in $copied) {
    Write-Host "  $file"
}
Write-Host "$($copied.Count) files copied. Restart Revit 2024 and look for the RCRC Green tab."
Write-Host "Reports go to $reportsFolder"
