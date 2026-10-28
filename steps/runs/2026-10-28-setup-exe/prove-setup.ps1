<#
.SYNOPSIS
Proves the KPI Setup.exe in throwaway Windows profiles, the way the zip was proved in #174.

.DESCRIPTION
Each scenario gets its own profile folder under -Root, with USERPROFILE, APPDATA and
LOCALAPPDATA pointed into it for the runs. Inno Setup reads its folders through the shell, which
expands them from those variables, measured with a probe before any of this ran. The one thing
a profile folder cannot hold is the uninstall entry, which a per user Setup writes to the real
HKCU, so every scenario ends on an uninstall and the script checks the entry is gone.

  Fresh       install, the files, the Apps entry, then the person's choices and an uninstall
  Upgrade     the older Setup, a chosen templates folder, then the newer Setup over it
  Refused     a stand-in Revit.exe running, for Setup and for the uninstall
  Full        install.ps1's full install, then the Setup over it
  NoDocuments a profile with no Documents folder, which the shell may make itself

Run it in Windows PowerShell 5.1:

  powershell -NoProfile -ExecutionPolicy Bypass -File prove-setup.ps1 -Older <exe> -Newer <exe> -Zip <zip> -Root <folder>
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $Older,
    [Parameter(Mandatory)] [string] $Newer,
    [Parameter(Mandatory)] [string] $Zip,
    [Parameter(Mandatory)] [string] $Root
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
$entry = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\{B2B66EEE-05CF-49A3-9F38-6020B8046379}_is1'
$real = @{ USERPROFILE = $env:USERPROFILE; APPDATA = $env:APPDATA; LOCALAPPDATA = $env:LOCALAPPDATA }

if (Test-Path $entry) { throw "An RCRC Green KPI entry is already in Apps for this user. Nothing was run." }
if (Get-Process -Name Revit -ErrorAction SilentlyContinue) { throw 'Revit is running. Nothing was run.' }

function Profile([string] $name, [switch] $NoDocuments) {
    $folder = Join-Path $Root $name
    if (Test-Path $folder) { Remove-Item $folder -Recurse -Force }
    New-Item -ItemType Directory -Force (Join-Path $folder 'AppData\Roaming'), (Join-Path $folder 'AppData\Local') | Out-Null
    if (-not $NoDocuments) { New-Item -ItemType Directory -Force (Join-Path $folder 'Documents') | Out-Null }
    $env:USERPROFILE = $folder
    $env:APPDATA = Join-Path $folder 'AppData\Roaming'
    $env:LOCALAPPDATA = Join-Path $folder 'AppData\Local'
    Write-Host ''
    Write-Host "=== $name, profile $folder"
    return $folder
}

function Back {
    foreach ($key in $real.Keys) { Set-Item "env:$key" $real[$key] }
}

function Setup([string] $exe, [string] $log) {
    $run = Start-Process -FilePath $exe -ArgumentList '/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', "/LOG=`"$log`"" -Wait -PassThru
    Write-Host "Setup $(Split-Path $exe -Leaf) exit $($run.ExitCode)"
    return $run.ExitCode
}

function Uninstall([string] $folder) {
    $uninstaller = Join-Path $folder 'AppData\Roaming\RCRC Green\Setup\unins000.exe'
    $run = Start-Process -FilePath $uninstaller -ArgumentList '/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART' -Wait -PassThru
    # The uninstaller hands over to a copy of itself in TEMP, so the files go a moment later.
    $until = (Get-Date).AddSeconds(60)
    while ((Test-Path $uninstaller) -and (Get-Date) -lt $until) { Start-Sleep -Milliseconds 500 }
    Write-Host "Uninstall exit $($run.ExitCode), uninstaller still there: $(Test-Path $uninstaller)"
    return $run.ExitCode
}

function Files([string] $folder) {
    Write-Host 'Files in the profile:'
    foreach ($file in Get-ChildItem $folder -Recurse -File -Force | Sort-Object FullName) {
        Write-Host ('  {0,9}  {1}' -f $file.Length, $file.FullName.Substring($folder.Length + 1))
    }
}

function Entry {
    if (Test-Path $entry) {
        $held = Get-ItemProperty $entry
        Write-Host "Apps entry: DisplayName '$($held.DisplayName)', DisplayVersion '$($held.DisplayVersion)'"
    }
    else { Write-Host 'Apps entry: none' }
}

function Hash([string] $path) { (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.Substring(0, 16) }

function Check([string] $what, [bool] $held) {
    Write-Host ("  {0}  {1}" -f $(if ($held) { 'PASS' } else { 'FAIL' }), $what)
    if (-not $held) { $script:failed++ }
}

$script:failed = 0
$unzipped = Join-Path $Root 'zip'
if (Test-Path $unzipped) { Remove-Item $unzipped -Recurse -Force }
Expand-Archive -LiteralPath $Zip -DestinationPath $unzipped

try {
    # Fresh install, then the choices a person makes in the pane, then the uninstall.
    $p = Profile 'fresh'
    $addins = Join-Path $p 'AppData\Roaming\Autodesk\Revit\Addins\2024'
    $beside = Join-Path $addins 'RcrcGreen'
    $code = Setup $Newer (Join-Path $Root 'fresh-setup.log')
    Files $p
    Entry
    Check 'exit code 0' ($code -eq 0)
    foreach ($name in 'RcrcGreen.Revit.dll', 'RcrcGreen.Core.dll', 'RcrcGreen.Revit.pdb', 'RcrcGreen.Core.pdb', 'components.csv', 'features.txt', 'VERSION.txt') {
        Check "$name the same bytes as in the zip" ((Hash (Join-Path $beside $name)) -eq (Hash (Join-Path $unzipped $name)))
    }
    Check 'RcrcGreen.addin the same bytes as in the zip' ((Hash (Join-Path $addins 'RcrcGreen.addin')) -eq (Hash (Join-Path $unzipped 'RcrcGreen.addin')))
    Check "features.txt reads KPI" ((Get-Content (Join-Path $beside 'features.txt') -Raw).Trim() -eq 'KPI')
    $reports = Join-Path $p 'Documents\RCRC Green\reports'
    Check 'the reports folder is made' (Test-Path $reports)
    Check 'reports-folder.txt names it' ((Get-Content (Join-Path $beside 'reports-folder.txt') -Raw).Trim() -eq $reports)
    Check 'the Apps entry is RCRC Green KPI' ((Get-ItemProperty $entry).DisplayName -eq 'RCRC Green KPI')

    $templates = Join-Path $p 'Shared\KPI templates'
    New-Item -ItemType Directory -Force $templates | Out-Null
    Set-Content (Join-Path $templates 'components.csv') "Component,Template,Folder`r`nSHARED ROW,MOSQUES,SHARED ROW" -Encoding UTF8
    Set-Content (Join-Path $beside 'templates-folder.txt') $templates -Encoding UTF8 -NoNewline
    Set-Content (Join-Path $beside 'kpi-output-folder.txt') (Join-Path $p 'Output') -Encoding UTF8 -NoNewline
    Set-Content (Join-Path $beside 'kpi-forms-folder.txt') (Join-Path $p 'Forms') -Encoding UTF8 -NoNewline
    Set-Content (Join-Path $beside 'kpi-prepared-by.txt') 'A Person' -Encoding UTF8 -NoNewline
    New-Item -ItemType Directory -Force (Join-Path $p 'AppData\Roaming\RCRC Green') | Out-Null
    Set-Content (Join-Path $p 'AppData\Roaming\RCRC Green\components.csv') "Component,Template,Folder`r`nOWN ROW,SCHOOLS,OWN ROW" -Encoding UTF8
    Set-Content (Join-Path $reports 'kpi-report.txt') 'a report' -Encoding UTF8
    $kept = @(
        (Join-Path $beside 'templates-folder.txt'), (Join-Path $beside 'kpi-output-folder.txt'),
        (Join-Path $beside 'kpi-forms-folder.txt'), (Join-Path $beside 'kpi-prepared-by.txt'),
        (Join-Path $beside 'reports-folder.txt'), (Join-Path $templates 'components.csv'),
        (Join-Path $p 'AppData\Roaming\RCRC Green\components.csv'), (Join-Path $reports 'kpi-report.txt'))
    $before = @{}
    foreach ($file in $kept) { $before[$file] = Hash $file }

    $code = Uninstall $p
    Files $p
    Entry
    Check 'uninstall exit code 0' ($code -eq 0)
    Check 'RcrcGreen.addin is gone' (-not (Test-Path (Join-Path $addins 'RcrcGreen.addin')))
    foreach ($name in 'RcrcGreen.Revit.dll', 'RcrcGreen.Core.dll', 'RcrcGreen.Revit.pdb', 'RcrcGreen.Core.pdb', 'components.csv', 'features.txt', 'VERSION.txt') {
        Check "$name beside the DLLs is gone" (-not (Test-Path (Join-Path $beside $name)))
    }
    foreach ($file in $kept) {
        Check "kept unchanged: $($file.Substring($p.Length + 1))" ((Test-Path $file) -and ((Hash $file) -eq $before[$file]))
    }
    Check 'the Apps entry is gone' (-not (Test-Path $entry))
    Back

    # Upgrade, the older Setup and then the newer one over it.
    $p = Profile 'upgrade'
    $beside = Join-Path $p 'AppData\Roaming\Autodesk\Revit\Addins\2024\RcrcGreen'
    $code = Setup $Older (Join-Path $Root 'upgrade-older.log')
    Entry
    $olderVersion = (Get-ItemProperty $entry).DisplayVersion
    $olderDll = Hash (Join-Path $beside 'RcrcGreen.Revit.dll')
    Set-Content (Join-Path $beside 'templates-folder.txt') 'C:\Chosen\Templates' -Encoding UTF8 -NoNewline
    $code2 = Setup $Newer (Join-Path $Root 'upgrade-newer.log')
    Files $p
    Entry
    Check 'both exit codes 0' (($code -eq 0) -and ($code2 -eq 0))
    Check 'the DLL is the newer one' ((Hash (Join-Path $beside 'RcrcGreen.Revit.dll')) -eq (Hash (Join-Path $unzipped 'RcrcGreen.Revit.dll')) -and $olderDll -ne (Hash (Join-Path $beside 'RcrcGreen.Revit.dll')))
    Check "the Apps entry moved on from '$olderVersion'" ((Get-ItemProperty $entry).DisplayVersion -ne $olderVersion)
    Check 'VERSION.txt is the newer one' ((Hash (Join-Path $beside 'VERSION.txt')) -eq (Hash (Join-Path $unzipped 'VERSION.txt')))
    Check 'the chosen templates folder is kept' ((Get-Content (Join-Path $beside 'templates-folder.txt') -Raw) -eq 'C:\Chosen\Templates')
    Check 'one uninstaller, not a second' (-not (Test-Path (Join-Path $p 'AppData\Roaming\RCRC Green\Setup\unins001.exe')))
    $code = Uninstall $p
    Check 'uninstall exit code 0 and the Apps entry gone' (($code -eq 0) -and -not (Test-Path $entry))
    Back

    # Refused, with a stand-in Revit.exe running.
    $p = Profile 'refused'
    $standin = Join-Path $Root 'standin\Revit.exe'
    New-Item -ItemType Directory -Force (Split-Path $standin) | Out-Null
    Copy-Item "$env:SystemRoot\System32\PING.EXE" $standin -Force
    $revit = Start-Process -FilePath $standin -ArgumentList '-n', '600', '127.0.0.1' -WindowStyle Hidden -PassThru
    try {
        $code = Setup $Newer (Join-Path $Root 'refused-setup.log')
        Files $p
        Entry
        Check "Setup refused with a non zero exit, $code" ($code -ne 0)
        Check 'nothing written under AppData\Roaming' (-not (Get-ChildItem (Join-Path $p 'AppData\Roaming') -Recurse -File))
        Check 'no Apps entry' (-not (Test-Path $entry))
    }
    finally { Stop-Process -Id $revit.Id -Force; $revit.WaitForExit() }
    $code = Setup $Newer (Join-Path $Root 'refused-then-installed.log')
    $revit = Start-Process -FilePath $standin -ArgumentList '-n', '600', '127.0.0.1' -WindowStyle Hidden -PassThru
    try {
        $uninstaller = Join-Path $p 'AppData\Roaming\RCRC Green\Setup\unins000.exe'
        $run = Start-Process -FilePath $uninstaller -ArgumentList '/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART' -Wait -PassThru
        Start-Sleep -Seconds 3
        Write-Host "Uninstall with Revit running, exit $($run.ExitCode)"
        Check "the uninstall refused with a non zero exit, $($run.ExitCode)" ($run.ExitCode -ne 0)
        Check 'the add-in is still installed' (Test-Path (Join-Path $p 'AppData\Roaming\Autodesk\Revit\Addins\2024\RcrcGreen\RcrcGreen.Revit.dll'))
        Check 'the Apps entry is still there' (Test-Path $entry)
    }
    finally { Stop-Process -Id $revit.Id -Force; $revit.WaitForExit() }
    $code = Uninstall $p
    Check 'uninstall after Revit closed, exit 0 and the Apps entry gone' (($code -eq 0) -and -not (Test-Path $entry))
    Back

    # Bader's full install, then the Setup over it.
    $p = Profile 'full'
    $addins = Join-Path $p 'AppData\Roaming\Autodesk\Revit\Addins\2024'
    $beside = Join-Path $addins 'RcrcGreen'
    & (Join-Path $repoRoot 'install\install.ps1') | Out-Null
    Write-Host 'After install.ps1:'
    Files $p
    $pointer = Get-Content (Join-Path $beside 'reports-folder.txt') -Raw
    $code = Setup $Newer (Join-Path $Root 'full-setup.log')
    Files $p
    Entry
    Check 'exit code 0' ($code -eq 0)
    Check 'features.txt reads KPI' ((Get-Content (Join-Path $beside 'features.txt') -Raw).Trim() -eq 'KPI')
    Check 'the DLL is the kit one' ((Hash (Join-Path $beside 'RcrcGreen.Revit.dll')) -eq (Hash (Join-Path $unzipped 'RcrcGreen.Revit.dll')))
    Check 'the log names the full install' ((Get-Content (Join-Path $Root 'full-setup.log') -Raw) -match 'A full install with no features.txt is here')
    Check 'reports-folder.txt left naming the repo reports' ((Get-Content (Join-Path $beside 'reports-folder.txt') -Raw) -eq $pointer)
    foreach ($script in 'install.ps1', 'uninstall.ps1') {
        $said = ''
        try { & (Join-Path $repoRoot "install\$script") | Out-Null } catch { $said = $_.Exception.Message }
        Write-Host "$script over the Setup: $said"
        Check "$script refuses while the Setup's entry is in Apps" ($said -match 'Uninstall it')
        Check "the kit DLL and features.txt still there after $script" (((Hash (Join-Path $beside 'RcrcGreen.Revit.dll')) -eq (Hash (Join-Path $unzipped 'RcrcGreen.Revit.dll'))) -and (Test-Path (Join-Path $beside 'features.txt')))
    }
    $code = Uninstall $p
    Files $p
    Check 'uninstall exit code 0 and the Apps entry gone' (($code -eq 0) -and -not (Test-Path $entry))
    & (Join-Path $repoRoot 'install\install.ps1') | Out-Null
    Check 'install.ps1 puts the full install back, no features.txt' ((Test-Path (Join-Path $beside 'RcrcGreen.Revit.dll')) -and -not (Test-Path (Join-Path $beside 'features.txt')))
    Back

    # No Documents folder. The shell may make it itself, and the reports folder lands under
    # USERPROFILE\Documents either way.
    $p = Profile 'nodocuments' -NoDocuments
    $code = Setup $Newer (Join-Path $Root 'nodocuments-setup.log')
    $reports = Join-Path $p 'Documents\RCRC Green\reports'
    Files $p
    Check 'exit code 0' ($code -eq 0)
    Check 'the reports folder is made under USERPROFILE\Documents' (Test-Path $reports)
    $code = Uninstall $p
    Check 'uninstall exit code 0 and the Apps entry gone' (($code -eq 0) -and -not (Test-Path $entry))
    Back
}
finally {
    Back
    if (Test-Path $entry) { Write-Host "An Apps entry is left at $entry. Remove it." ; $script:failed++ }
}

Write-Host ''
if ($script:failed -eq 0) { Write-Host 'DONE: every check passed.' } else { Write-Host "FAILED: $($script:failed) checks failed." }
exit $script:failed
