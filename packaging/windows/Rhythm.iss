#define MyAppName "Rhythm"
#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif
#ifndef MyAppPublisher
  #define MyAppPublisher "Rhythm Open Source"
#endif
#ifndef MyAppExeName
  #define MyAppExeName "Rhythm.App.exe"
#endif
#ifndef MyAppSourceDir
  #define MyAppSourceDir "..\..\artifacts\publish\win-x64"
#endif
#ifndef MyAppOutputDir
  #define MyAppOutputDir "..\..\artifacts\installer"
#endif

[Setup]
AppId={{DDF689A8-E3C8-4B09-8AF6-2B47F6ABBA74}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
OutputDir={#MyAppOutputDir}
OutputBaseFilename=Rhythm-Setup-{#MyAppVersion}-win-x64
SetupIconFile=..\..\src\Rhythm.App\Assets\Rhythm.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
WizardStyle=modern
Compression=lzma2/ultra64
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked
Name: "startup"; Description: "Launch Rhythm automatically when I sign in"; GroupDescription: "Startup behavior:"

[Files]
Source: "{#MyAppSourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: startup

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

[Code]
const
  RuntimeDisplayName = '.NET Desktop Runtime 8 (x64)';
  RuntimeDownloadUrl = 'https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime';
  RuntimeWingetPackageId = 'Microsoft.DotNet.DesktopRuntime.8';
  ProductOverviewTitle = 'What Rhythm helps with';
  ProductOverviewBody =
    'Rhythm helps you build a healthier computer-use routine.' + #13#10#13#10 +
    '- Reminds you to take breaks on your own schedule' + #13#10 +
    '- Resets the timer after the PC is locked' + #13#10 +
    '- Shows a full-screen semi-transparent break overlay' + #13#10 +
    '- Tracks completed breaks, skipped breaks, and actual rest time' + #13#10#13#10 +
    'Startup is enabled by default below, so Rhythm can quietly help from the moment you sign in.';

var
  ProductOverviewPage: TWizardPage;
  ProductOverviewText: TNewStaticText;

procedure InitializeWizard();
begin
  ProductOverviewPage :=
    CreateCustomPage(
      wpWelcome,
      ProductOverviewTitle,
      'A quick look at what will be installed');

  ProductOverviewText := TNewStaticText.Create(ProductOverviewPage);
  ProductOverviewText.Parent := ProductOverviewPage.Surface;
  ProductOverviewText.Left := ScaleX(0);
  ProductOverviewText.Top := ScaleY(8);
  ProductOverviewText.Width := ProductOverviewPage.SurfaceWidth;
  ProductOverviewText.Height := ScaleY(180);
  ProductOverviewText.AutoSize := False;
  ProductOverviewText.WordWrap := True;
  ProductOverviewText.Caption := ProductOverviewBody;
end;

function IsDesktopRuntimeInstalled(): Boolean;
var
  RuntimeRoot: string;
  FindRec: TFindRec;
begin
  Result := False;
  RuntimeRoot := ExpandConstant('{commonpf64}\dotnet\shared\Microsoft.WindowsDesktop.App\');
  if not DirExists(RuntimeRoot) then
    Exit;

  if FindFirst(RuntimeRoot + '8.0*', FindRec) then
  begin
    try
      repeat
        if (FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY) <> 0 then
        begin
          Result := True;
          Exit;
        end;
      until not FindNext(FindRec);
    finally
      FindClose(FindRec);
    end;
  end;
end;

function EnsureRuntimeInstalledAfterWinget(): Boolean;
var
  Retry: Integer;
begin
  Result := False;
  for Retry := 0 to 29 do
  begin
    if IsDesktopRuntimeInstalled() then
    begin
      Result := True;
      Exit;
    end;

    Sleep(1000);
  end;
end;

function TryInstallRuntimeWithWinget(): Boolean;
var
  ResultCode: Integer;
begin
  Result :=
    Exec(
      ExpandConstant('{cmd}'),
      '/C winget install ' + RuntimeWingetPackageId + ' --architecture x64 --accept-package-agreements --accept-source-agreements',
      '',
      SW_SHOWNORMAL,
      ewWaitUntilTerminated,
      ResultCode) and ((ResultCode = 0) or (ResultCode = 3010));
end;

procedure OpenRuntimeDownloadPage();
var
  ResultCode: Integer;
begin
  ShellExec('open', RuntimeDownloadUrl, '', '', SW_SHOWNORMAL, ewNoWait, ResultCode);
end;

function InitializeSetup(): Boolean;
var
  Choice: Integer;
begin
  Result := True;

  if IsDesktopRuntimeInstalled() then
    Exit;

  Choice := MsgBox(
    ExpandConstant(
      '{#MyAppName} requires ' + RuntimeDisplayName + ' to run.' + #13#10#13#10 +
      'Click Yes to try installing it with WinGet.' + #13#10 +
      'Click No to open the official Microsoft download page.' + #13#10 +
      'Click Cancel to stop setup.'),
    mbConfirmation,
    MB_YESNOCANCEL);

  if Choice = IDCANCEL then
  begin
    Result := False;
    Exit;
  end;

  if Choice = IDYES then
  begin
    if TryInstallRuntimeWithWinget() and EnsureRuntimeInstalledAfterWinget() then
      Exit;

    OpenRuntimeDownloadPage();
    MsgBox(
      'Install ' + RuntimeDisplayName + ' and then run this setup again.',
      mbInformation,
      MB_OK);
    Result := False;
    Exit;
  end;

  OpenRuntimeDownloadPage();
  MsgBox(
    'Install ' + RuntimeDisplayName + ' and then run this setup again.',
    mbInformation,
    MB_OK);
  Result := False;
end;
