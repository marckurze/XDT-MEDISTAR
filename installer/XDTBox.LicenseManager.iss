; XDTBox Lizenzmanager 1.11 installer.
; Build with scripts/build-xdtbox-licensemanager-installer.ps1.

#define MyAppName "XDTBox Lizenzmanager"
#ifndef MyAppVersion
#define MyAppVersion "1.11"
#endif
#ifndef MyVersionInfoVersion
#define MyVersionInfoVersion "1.11.0.0"
#endif
#define MyAppPublisher "Technik-Apparat M.Kurze"
#define MyAppExeName "XdtBox.LicenseManager.exe"
#ifndef MyPublishDir
#define MyPublishDir "..\artifacts\publish\XDTBox Lizenzmanager"
#endif
#ifndef MyInstallerOutputDir
#define MyInstallerOutputDir "..\artifacts\licensemanager-installer"
#endif

[Setup]
AppId={{E0CE79F8-0E1D-4D57-9B89-4BBD8E793017}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL=https://www.XDTBox.com
AppSupportURL=mailto:info@XDTBox.com
AppUpdatesURL=https://www.XDTBox.com
DefaultDirName=C:\XDTBox\Lizenzaktivierung
DefaultGroupName=XDTBox Lizenzmanager
DisableProgramGroupPage=no
DisableDirPage=no
UsePreviousAppDir=yes
OutputDir={#MyInstallerOutputDir}
OutputBaseFilename=XDTBox_Lizenzmanager_Setup_{#MyAppVersion}
SetupIconFile=..\XdtDeviceBridge.App\Assets\App\XDTBox.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
PrivilegesRequired=admin
CloseApplications=yes
RestartApplications=no
VersionInfoVersion={#MyVersionInfoVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=XDTBox Lizenzmanager Setup
VersionInfoProductName=XDTBox Lizenzmanager
VersionInfoProductVersion={#MyAppVersion}

[Languages]
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "Desktop-Verknüpfung erstellen"; GroupDescription: "Verknüpfungen:"; Flags: unchecked

[Files]
Source: "{#MyPublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "XdtDeviceBridge.App.exe,XdtDeviceBridge.App.dll,XdtBox.LicenseIssuer.exe,*.Tests.*,*.pdb,*.pem,*.key,*.xdtbox-licensemanager-backup,license-manager-customers.json,license-history.json,license-manager-settings.json,.git*,Codex*"

[Icons]
Name: "{group}\XDTBox Lizenzmanager"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\XDTBox Lizenzmanager deinstallieren"; Filename: "{uninstallexe}"
Name: "{autodesktop}\XDTBox Lizenzmanager"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "XDTBox Lizenzmanager starten"; Flags: nowait postinstall skipifsilent unchecked

[Registry]
Root: HKLM; Subkey: "Software\XDTBox\Lizenzmanager"; ValueType: string; ValueName: "DisplayName"; ValueData: "XDTBox Lizenzmanager"; Flags: uninsdeletekeyifempty
Root: HKLM; Subkey: "Software\XDTBox\Lizenzmanager"; ValueType: string; ValueName: "DisplayVersion"; ValueData: "{#MyAppVersion}"; Flags: uninsdeletekeyifempty
Root: HKLM; Subkey: "Software\XDTBox\Lizenzmanager"; ValueType: string; ValueName: "Publisher"; ValueData: "{#MyAppPublisher}"; Flags: uninsdeletekeyifempty
Root: HKLM; Subkey: "Software\XDTBox\Lizenzmanager"; ValueType: string; ValueName: "InstallLocation"; ValueData: "{app}"; Flags: uninsdeletekeyifempty

[Code]
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
  begin
    MsgBox(
      'Der XDTBox Lizenzmanager wurde entfernt. Lokale Hersteller-Verwaltungsdaten, Sicherungen und Schlüsseldateien werden nicht automatisch gelöscht.',
      mbInformation,
      MB_OK);
  end;
end;
