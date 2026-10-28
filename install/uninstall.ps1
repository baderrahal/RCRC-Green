<#
.SYNOPSIS
Takes RCRC Green back out of the per user Revit 2024 add-ins folder.

.DESCRIPTION
Removes exactly what install.ps1 puts there and nothing else. Anything else living in the
add-ins folder belongs to another add-in and is left alone.

.EXAMPLE
.\install\uninstall.ps1
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Removing the files under the team Setup.exe would leave its entry in Apps naming an install
# that is gone. The id is the AppId in install\team\RCRC Green KPI Setup.iss.
if (Test-Path -LiteralPath 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\{B2B66EEE-05CF-49A3-9F38-6020B8046379}_is1') {
    throw 'RCRC Green KPI from the team Setup.exe is installed. Uninstall it in Windows Settings, Apps, Installed apps instead. Nothing was changed.'
}

$addinsFolder = Join-Path $env:APPDATA 'Autodesk\Revit\Addins\2024'
$manifest = Join-Path $addinsFolder 'RcrcGreen.addin'
$assemblyFolder = Join-Path $addinsFolder 'RcrcGreen'

$removed = New-Object System.Collections.Generic.List[string]

if (Test-Path -LiteralPath $manifest) {
    Remove-Item -LiteralPath $manifest -Force
    $removed.Add($manifest)
}

if (Test-Path -LiteralPath $assemblyFolder) {
    foreach ($file in Get-ChildItem -LiteralPath $assemblyFolder -File) {
        $removed.Add($file.FullName)
    }
    Remove-Item -LiteralPath $assemblyFolder -Recurse -Force
    $removed.Add($assemblyFolder)
}

if ($removed.Count -eq 0) {
    Write-Host "Nothing to remove. RCRC Green was not installed under $addinsFolder"
    return
}

Write-Host "Removed RCRC Green from $addinsFolder"
foreach ($item in $removed) {
    Write-Host "  $item"
}
Write-Host "$($removed.Count) items removed. Restart Revit 2024 and the RCRC Green tab is gone."
