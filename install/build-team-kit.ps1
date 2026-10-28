<#
.SYNOPSIS
Builds the RCRC Green KPI team kit from a clean checkout of main and writes the zip outside
this repo.

.DESCRIPTION
The production team runs Revit 2024 with no repo, no Visual Studio and no admin rights, and
needs the KPI tool alone. This makes the one zip they install from:

  RcrcGreen.addin, RcrcGreen.Revit.dll, RcrcGreen.Core.dll and their symbol files
  components.csv                        the component values, their templates and folders
  features.txt                          KPI, so the ribbon builds the KPI panel and nothing else
  VERSION.txt                           the version, read off the build stamp, never typed
  team-install.ps1                      and the two launchers that start it
  Install RCRC Green KPI.cmd
  Put back the previous version.cmd
  INSTALL-TEAM.txt                      the install page

It builds in a throwaway git worktree of origin/main, so what the team gets is what main holds
and never a working copy with changes in it. The zip goes to
%USERPROFILE%\Documents\RCRC-Green-Team-Kit and nothing it makes is ever committed. Client
templates and PDF forms are not in it: each person browses to the shared folders once.

Run it in Windows PowerShell 5.1, because the reference check it runs reads assemblies the
way the team's machines will:

  powershell -NoProfile -ExecutionPolicy Bypass -File .\install\build-team-kit.ps1

.PARAMETER Ref
What to build. origin/main unless you are testing a branch.
#>
[CmdletBinding()]
param(
    [string] $Ref = 'origin/main'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ($PSVersionTable.PSEdition -ne 'Desktop') {
    throw 'Run this in Windows PowerShell 5.1: powershell -NoProfile -ExecutionPolicy Bypass -File .\install\build-team-kit.ps1'
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$outputFolder = Join-Path $env:USERPROFILE 'Documents\RCRC-Green-Team-Kit'
$work = Join-Path ([System.IO.Path]::GetTempPath()) ('rcrc-green-kit-' + [guid]::NewGuid().ToString('N').Substring(0, 8))

# Not named Git: PowerShell names ignore case, so a function called Git calls itself.
function InRepo([string[]] $arguments) {
    & git.exe -C $repoRoot @arguments
    if ($LASTEXITCODE -ne 0) { throw "git $($arguments -join ' ') failed." }
}

InRepo @('fetch', '--quiet', 'origin')
InRepo @('worktree', 'add', '--quiet', '--detach', $work, $Ref)

try {
    $project = Join-Path $work 'src\RcrcGreen.Revit\RcrcGreen.Revit.csproj'
    & dotnet build $project -c Release --nologo -v quiet
    if ($LASTEXITCODE -ne 0) { throw "The Release build of $Ref failed. Nothing was written." }

    $built = Join-Path $work 'src\RcrcGreen.Revit\bin\Release'
    $stamp = [System.Diagnostics.FileVersionInfo]::GetVersionInfo((Join-Path $built 'RcrcGreen.Revit.dll')).ProductVersion

    # A clean worktree cannot carry changes, so a stamp saying otherwise, or none at all, means
    # the stamp step is broken, and a kit whose version cannot be trusted is not written.
    if (-not $stamp -or $stamp -match '^no-commit' -or $stamp -match 'changed$') {
        throw "The build's stamp reads '$stamp', which is not a commit and a time. Nothing was written."
    }

    # The stamp holds a clock time, and a colon cannot sit in a file name.
    $version = $stamp -replace ':', ''
    $commit = (& git -C $work rev-parse HEAD).Trim()

    $kit = Join-Path $work 'kit'
    New-Item -ItemType Directory -Path $kit -Force | Out-Null
    foreach ($name in @('RcrcGreen.addin', 'RcrcGreen.Revit.dll', 'RcrcGreen.Core.dll', 'RcrcGreen.Revit.pdb', 'RcrcGreen.Core.pdb', 'components.csv')) {
        Copy-Item -LiteralPath (Join-Path $built $name) -Destination $kit
    }
    foreach ($item in Get-ChildItem -LiteralPath (Join-Path $work 'install\team') -File) {
        Copy-Item -LiteralPath $item.FullName -Destination $kit
    }

    $utf8 = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText((Join-Path $kit 'features.txt'), "KPI`r`n", $utf8)
    [System.IO.File]::WriteAllText((Join-Path $kit 'VERSION.txt'),
        "RCRC Green KPI $version`r`nBuild: $stamp`r`nCommit: $commit`r`nBuilt from: $Ref`r`n", $utf8)

    # The same check the install runs, run here first, so a kit missing a reference is never
    # written at all rather than refused on twenty machines.
    & "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File (Join-Path $kit 'team-install.ps1') -CheckOnly
    if ($LASTEXITCODE -ne 0) { throw 'The bundle check refused. Nothing was written.' }

    New-Item -ItemType Directory -Path $outputFolder -Force | Out-Null
    $zip = Join-Path $outputFolder "RCRC Green KPI $version.zip"
    if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip -Force }
    Compress-Archive -Path (Join-Path $kit '*') -DestinationPath $zip

    Write-Host ''
    Write-Host "The kit holds:"
    foreach ($item in Get-ChildItem -LiteralPath $kit -File | Sort-Object Name) {
        Write-Host ('  {0,9:N0}  {1}' -f $item.Length, $item.Name)
    }
    $size = (Get-Item -LiteralPath $zip).Length
    Write-Host ''
    Write-Host "DONE: $zip, $('{0:N0}' -f $size) bytes, version $version from $commit. Put it on the project Teams or SharePoint folder."
}
finally {
    & git -C $repoRoot worktree remove --force $work
}
