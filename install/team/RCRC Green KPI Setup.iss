; The one file Setup.exe for the RCRC Green KPI team kit. build-team-kit.ps1 compiles it with
; Inno Setup against the kit folder it has just checked, so the Setup.exe and the zip hold the
; same files and the same version:
;
;   ISCC.exe /DKit=<kit folder> /DVersion=<version off the stamp> /O<output folder> this file
;
; It installs for the person running it and needs no admin rights, into the layout
; team-install.ps1 makes:
;
;   %APPDATA%\Autodesk\Revit\Addins\2024\RcrcGreen.addin
;   %APPDATA%\Autodesk\Revit\Addins\2024\RcrcGreen\         the assemblies, components.csv, features.txt, VERSION.txt
;   Documents\RCRC Green\reports\                           where the pane writes its reports
;
; What somebody chose in the pane lives in pointer files beside the assemblies. Setup makes the
; three an install needs only where they are missing, in code rather than under [Files], so the
; uninstall, which removes what [Files] put there and nothing else, leaves every one of them.

#ifndef Kit
  #error Pass the kit folder: /DKit=<folder>
#endif
#ifndef Version
  #error Pass the version read off the build stamp: /DVersion=<version>
#endif

[Setup]
; One id for every version, so a newer Setup.exe upgrades in place and Windows lists one app.
AppId={{B2B66EEE-05CF-49A3-9F38-6020B8046379}
AppName=RCRC Green KPI
AppVersion={#Version}
AppVerName=RCRC Green KPI {#Version}
UninstallDisplayName=RCRC Green KPI
OutputBaseFilename=RCRC Green KPI Setup {#Version}
PrivilegesRequired=lowest
DefaultDirName={userappdata}\Autodesk\Revit\Addins\2024
; The folder is Revit's and never a choice, and an earlier install's folder is never reused,
; because Revit looks in this one place only.
DisableDirPage=yes
UsePreviousAppDir=no
DisableProgramGroupPage=yes
DisableWelcomePage=no
; Outside Addins\2024, so neither Revit nor uninstall.ps1 taking the RcrcGreen folder away can
; leave an entry in Apps whose uninstaller is gone.
UninstallFilesDir={userappdata}\RCRC Green\Setup
; Setup refuses while Revit runs, so it never offers to close Revit with work unsaved.
CloseApplications=no
RestartApplications=no
ShowLanguageDialog=no
WizardStyle=modern
Compression=lzma2
SolidCompression=yes

[Messages]
WelcomeLabel2=This installs [name/ver] for Revit 2024, for you alone. It needs no admin rights.%n%nClose Revit before you go on.
FinishedLabelNoIcons=Start Revit 2024. When it asks about RCRC Green, choose Always Load.
FinishedLabel=Start Revit 2024. When it asks about RCRC Green, choose Always Load.

[Files]
Source: "{#Kit}\RcrcGreen.addin"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Kit}\RcrcGreen.Revit.dll"; DestDir: "{app}\RcrcGreen"; Flags: ignoreversion
Source: "{#Kit}\RcrcGreen.Core.dll"; DestDir: "{app}\RcrcGreen"; Flags: ignoreversion
Source: "{#Kit}\RcrcGreen.Revit.pdb"; DestDir: "{app}\RcrcGreen"; Flags: ignoreversion
Source: "{#Kit}\RcrcGreen.Core.pdb"; DestDir: "{app}\RcrcGreen"; Flags: ignoreversion
Source: "{#Kit}\components.csv"; DestDir: "{app}\RcrcGreen"; Flags: ignoreversion
Source: "{#Kit}\features.txt"; DestDir: "{app}\RcrcGreen"; Flags: ignoreversion
Source: "{#Kit}\VERSION.txt"; DestDir: "{app}\RcrcGreen"; Flags: ignoreversion

[Code]
const
  RevitRunning = 'Revit is running. Save your work, close every Revit window, and run Setup again. Nothing was changed.';
  RevitRunningUninstall = 'Revit is running. Save your work, close every Revit window, and uninstall again. Nothing was changed.';

function RevitIsRunning: Boolean;
var
  Locator, Service, Found: Variant;
  Code: Integer;
begin
  try
    Locator := CreateOleObject('WbemScripting.SWbemLocator');
    Service := Locator.ConnectServer('.', 'root\CIMV2');
    Found := Service.ExecQuery('SELECT ProcessId FROM Win32_Process WHERE Name = ''Revit.exe''');
    Result := Found.Count > 0;
  except
    // A machine with WMI switched off still has tasklist, and find answers 0 when it is listed.
    Result := Exec(ExpandConstant('{cmd}'), '/C tasklist /FI "IMAGENAME eq Revit.exe" /NH | find /I "Revit.exe"',
      '', SW_HIDE, ewWaitUntilTerminated, Code) and (Code = 0);
  end;
end;

// The full install Bader makes with install.ps1 writes no features.txt, which shows all three
// tools. A kit install always writes one.
function FullInstallIsHere: Boolean;
begin
  Result := FileExists(ExpandConstant('{app}\RcrcGreen\RcrcGreen.Revit.dll'))
    and not FileExists(ExpandConstant('{app}\RcrcGreen\features.txt'));
end;

// The Documents folder the person sees, which OneDrive may have moved off the profile, the way
// team-install.ps1 finds it. Windows can answer with nothing for a profile it does not
// recognise, and then the plain one is used.
function ReportsFolder: String;
begin
  try
    Result := ExpandConstant('{userdocs}');
  except
    Result := '';
  end;
  if Result = '' then
    Result := ExpandConstant('{%USERPROFILE}\Documents');
  Result := AddBackslash(Result) + 'RCRC Green\reports';
end;

function InitializeSetup: Boolean;
begin
  Result := not RevitIsRunning;
  if not Result then
    SuppressibleMsgBox(RevitRunning, mbError, MB_OK, IDOK);
end;

// Asked again at the press of Install, because Revit can be started while the wizard is open.
function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  if RevitIsRunning then
    Result := RevitRunning
  else
    Result := '';
end;

function InitializeUninstall: Boolean;
begin
  Result := not RevitIsRunning;
  if not Result then
    SuppressibleMsgBox(RevitRunningUninstall, mbError, MB_OK, IDOK);
end;

// What is installed now, so an older Setup run over a newer one says so before Install. The
// zip's own install prints both versions in the same way.
function InstalledNow: String;
var
  Lines: TArrayOfString;
begin
  Result := '';
  if LoadStringsFromFile(ExpandConstant('{app}\RcrcGreen\VERSION.txt'), Lines) and (GetArrayLength(Lines) > 0) then
    Result := Trim(Lines[0])
  else if FileExists(ExpandConstant('{app}\RcrcGreen\RcrcGreen.Revit.dll')) then
    Result := 'an RCRC Green build with no VERSION.txt';
end;

// The memo does not wrap, so every line is kept short enough to read without scrolling sideways.
function UpdateReadyMemo(Space, NewLine, MemoUserInfoInfo, MemoDirInfo, MemoTypeInfo,
  MemoComponentsInfo, MemoGroupInfo, MemoTasksInfo: String): String;
var
  Now: String;
begin
  Now := InstalledNow;
  if Now = '' then
    Now := 'nothing';
  Result := 'RCRC Green KPI {#Version} for Revit 2024,' + NewLine
    + 'for ' + GetUserNameString + ' alone.' + NewLine
    + NewLine + 'Installed now:' + NewLine + Space + Now + NewLine
    + NewLine + 'Into:' + NewLine + Space + ExpandConstant('{app}') + NewLine
    + NewLine + 'Your chosen folders, your reports and your components.csv are kept.';
  if FullInstallIsHere then
    Result := Result + NewLine + NewLine
      + 'The full RCRC Green install, with all three tools, is here.' + NewLine
      + 'Setup replaces it with KPI only. To put it back, uninstall' + NewLine
      + 'RCRC Green KPI in Apps, then run install\install.ps1.';
end;

procedure WriteWhereMissing(Path, Text: String);
var
  Held: AnsiString;
  Lines: TArrayOfString;
begin
  if FileExists(Path) and LoadStringFromFile(Path, Held) and (Trim(String(Held)) <> '') then
    Exit;
  SetArrayLength(Lines, 1);
  Lines[0] := Text;
  SaveStringsToUTF8FileWithoutBOM(Path, Lines, False);
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  Reports, Beside: String;
begin
  if CurStep = ssInstall then
  begin
    if FullInstallIsHere then
      Log('A full install with no features.txt is here and is replaced with KPI only.');
  end;
  if CurStep = ssPostInstall then
  begin
    Reports := ReportsFolder;
    Beside := ExpandConstant('{app}\RcrcGreen\');
    ForceDirectories(Reports);
    // The pane writes its reports where this file says. A file already naming a folder is left
    // as it is, so a machine that sends its reports elsewhere keeps doing so.
    WriteWhereMissing(Beside + 'reports-folder.txt', Reports);
    // The two the pane expects to find, made empty only where missing, which leaves a folder
    // somebody already chose exactly as it was.
    if not FileExists(Beside + 'templates-folder.txt') then
      SaveStringToFile(Beside + 'templates-folder.txt', '', False);
    if not FileExists(Beside + 'kpi-output-folder.txt') then
      SaveStringToFile(Beside + 'kpi-output-folder.txt', '', False);
  end;
end;
