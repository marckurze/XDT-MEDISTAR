; XDTBox 1.11 customer installer.
; Build with Inno Setup 6 after publishing the WPF app through scripts/build-xdtbox-installer.ps1.

#define MyAppName "XDTBox"
#ifndef MyAppVersion
#define MyAppVersion "1.11"
#endif
#ifndef MyVersionInfoVersion
#define MyVersionInfoVersion "1.11.0.0"
#endif
#define MyAppPublisher "Technik-Apparat M.Kurze"
#define MyAppExeName "XdtDeviceBridge.App.exe"
#ifndef MyPublishDir
#define MyPublishDir "..\artifacts\publish\XDTBox"
#endif
#ifndef MyInstallerOutputDir
#define MyInstallerOutputDir "..\artifacts\installer"
#endif

[Setup]
AppId={{7E45D05D-7E53-4B47-961D-9113C3D42623}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL=https://www.XDTBox.com
AppSupportURL=mailto:info@XDTBox.com
AppUpdatesURL=https://www.XDTBox.com
DefaultDirName=C:\XDTBox
DefaultGroupName=XDTBox
DisableProgramGroupPage=no
DisableDirPage=no
UsePreviousAppDir=yes
OutputDir={#MyInstallerOutputDir}
OutputBaseFilename=XDTBox_Setup_{#MyAppVersion}
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
VersionInfoDescription=XDTBox Setup
VersionInfoProductName=XDTBox
VersionInfoProductVersion={#MyAppVersion}

[Languages]
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "Desktop-Verknuepfung erstellen"; GroupDescription: "Verknuepfungen:"; Flags: unchecked

[Files]
Source: "{#MyPublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "XdtBox.LicenseManager.exe,XdtBox.LicenseManager.dll,XdtBox.LicenseIssuer.exe,XdtBox.LicenseIssuer.dll,*.Tests.*,*.pdb,*.pem,*.key,license-history.json,license-manager-settings.json,.git*,Codex*"

[Icons]
Name: "{group}\XDTBox"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\XDTBox deinstallieren"; Filename: "{uninstallexe}"
Name: "{autodesktop}\XDTBox"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "XDTBox starten"; Flags: nowait postinstall skipifsilent unchecked

[Registry]
Root: HKLM; Subkey: "Software\XDTBox"; ValueType: string; ValueName: "DisplayName"; ValueData: "XDTBox"; Flags: uninsdeletekeyifempty
Root: HKLM; Subkey: "Software\XDTBox"; ValueType: string; ValueName: "DisplayVersion"; ValueData: "{#MyAppVersion}"; Flags: uninsdeletekeyifempty
Root: HKLM; Subkey: "Software\XDTBox"; ValueType: string; ValueName: "Publisher"; ValueData: "{#MyAppPublisher}"; Flags: uninsdeletekeyifempty
Root: HKLM; Subkey: "Software\XDTBox"; ValueType: string; ValueName: "InstallLocation"; ValueData: "{app}"; Flags: uninsdeletekeyifempty
Root: HKLM; Subkey: "Software\XDTBox"; ValueType: string; ValueName: "MarkerFile"; ValueData: "{app}\XDTBox.installation.json"; Flags: uninsdeletekeyifempty

[Code]
var
  InstallModePage: TInputOptionWizardPage;
  DetectedInstallDir: string;

function AddSlashIfNeeded(const Path: string): string;
begin
  Result := AddBackslash(Path);
end;

function IsXdtBoxInstallDir(const Dir: string): Boolean;
begin
  Result :=
    FileExists(AddSlashIfNeeded(Dir) + '{#MyAppExeName}') or
    FileExists(AddSlashIfNeeded(Dir) + 'XDTBox.installation.json');
end;

function TryDetectInstallDir(var Dir: string): Boolean;
var
  RegDir: string;
begin
  Result := False;
  Dir := '';

  if RegQueryStringValue(HKLM, 'Software\XDTBox', 'InstallLocation', RegDir) then
  begin
    if IsXdtBoxInstallDir(RegDir) then
    begin
      Dir := RegDir;
      Result := True;
      exit;
    end;
  end;

  if RegQueryStringValue(HKLM, 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{7E45D05D-7E53-4B47-961D-9113C3D42623}_is1', 'InstallLocation', RegDir) then
  begin
    if IsXdtBoxInstallDir(RegDir) then
    begin
      Dir := RegDir;
      Result := True;
      exit;
    end;
  end;

  if IsXdtBoxInstallDir('C:\XDTBox') then
  begin
    Dir := 'C:\XDTBox';
    Result := True;
  end;
end;

procedure InitializeWizard();
begin
  TryDetectInstallDir(DetectedInstallDir);
  InstallModePage := CreateInputOptionPage(
    wpWelcome,
    'Installationsart',
    'Neuinstallation oder Update',
    'Bitte waehlen Sie, ob XDTBox neu installiert oder eine bestehende Installation aktualisiert werden soll. Kundendaten bleiben bei Updates erhalten.',
    True,
    False);
  InstallModePage.Add('Neuinstallation');
  InstallModePage.Add('Bestehende XDTBox aktualisieren');

  if DetectedInstallDir <> '' then
  begin
    InstallModePage.SelectedValueIndex := 1;
  end
  else
  begin
    InstallModePage.SelectedValueIndex := 0;
  end;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  TargetDir: string;
begin
  Result := True;

  if CurPageID = InstallModePage.ID then
  begin
    if InstallModePage.SelectedValueIndex = 1 then
    begin
      if TryDetectInstallDir(DetectedInstallDir) then
      begin
        WizardForm.DirEdit.Text := DetectedInstallDir;
        MsgBox(
          'Bestehende XDTBox-Installation erkannt:' + #13#10 + DetectedInstallDir + #13#10#13#10 +
          'Bitte erstellen Sie vor dem Update eine Sicherung unter Sicherung/Umzug. Kundendaten, Profile, COM-Port-Einstellungen, Lizenzen, Baukasten-Templates, Geraetebild-Overrides und externe Praxisordner werden durch das Update nicht geloescht.',
          mbInformation,
          MB_OK);
      end
      else
      begin
        MsgBox(
          'Es wurde keine bestehende XDTBox-Installation erkannt. Bitte waehlen Sie im naechsten Schritt den vorhandenen XDTBox-Installationsordner manuell aus. Ein Update wird nicht blind in einen unplausiblen Ordner geschrieben.',
          mbInformation,
          MB_OK);
      end;
    end
    else
    begin
      WizardForm.DirEdit.Text := 'C:\XDTBox';
    end;
  end;

  if CurPageID = wpSelectDir then
  begin
    TargetDir := WizardForm.DirEdit.Text;
    if InstallModePage.SelectedValueIndex = 1 then
    begin
      if not IsXdtBoxInstallDir(TargetDir) then
      begin
        MsgBox(
          'Der ausgewaehlte Ordner sieht nicht wie eine bestehende XDTBox-Installation aus. Bitte waehlen Sie den Ordner mit XdtDeviceBridge.App.exe oder kehren Sie zur Neuinstallation zurueck.',
          mbError,
          MB_OK);
        Result := False;
      end;
    end
    else
    begin
      if IsXdtBoxInstallDir(TargetDir) then
      begin
        if MsgBox(
          'Im Zielordner wurde bereits XDTBox erkannt. Fuer diesen Fall wird ein Update empfohlen. Moechten Sie trotzdem als Neuinstallation in diesen Ordner fortfahren?',
          mbConfirmation,
          MB_YESNO or MB_DEFBUTTON2) <> IDYES then
        begin
          Result := False;
        end;
      end
      else if DirExists(TargetDir) then
      begin
        if MsgBox(
          'Der Zielordner existiert bereits, enthaelt aber keine erkannte XDTBox-Installation. Moechten Sie XDTBox in diesen Ordner installieren?',
          mbConfirmation,
          MB_YESNO or MB_DEFBUTTON2) <> IDYES then
        begin
          Result := False;
        end;
      end;
    end;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  Marker: string;
begin
  if CurStep = ssPostInstall then
  begin
    Marker :=
      '{' + #13#10 +
      '  "product": "XDTBox",' + #13#10 +
      '  "version": "{#MyAppVersion}",' + #13#10 +
      '  "publisher": "{#MyAppPublisher}",' + #13#10 +
      '  "installLocation": "' + ExpandConstant('{app}') + '",' + #13#10 +
      '  "dataRoot": "%LocalAppData%\\XdtDeviceBridge",' + #13#10 +
      '  "uninstallPolicy": "VariantBKeepCustomerDataByDefault"' + #13#10 +
      '}' + #13#10;
    SaveStringToFile(ExpandConstant('{app}\XDTBox.installation.json'), Marker, False);
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  CustomerDataDir: string;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    CustomerDataDir := ExpandConstant('{localappdata}\XdtDeviceBridge');
    if MsgBox(
      'XDTBox wurde entfernt. Kundendaten bleiben standardmaessig erhalten.' + #13#10#13#10 +
      'Moechten Sie zusaetzlich alle lokalen XDTBox-Einstellungen, Profile, Lizenzen, Baukasten-Templates, Geraetebild-Overrides und lokalen Konfigurationsdaten unter ' + CustomerDataDir + ' loeschen? Diese Aktion kann nicht rueckgaengig gemacht werden.' + #13#10#13#10 +
      'Externe AIS-/Geraete-/Archiv-/Fehlerordner und Praxisordner werden nicht geloescht.',
      mbConfirmation,
      MB_YESNO or MB_DEFBUTTON2) = IDYES then
    begin
      if MsgBox(
        'Letzte Bestaetigung: Lokale XDTBox-Kundendaten unter ' + CustomerDataDir + ' wirklich dauerhaft loeschen?',
        mbConfirmation,
        MB_YESNO or MB_DEFBUTTON2) = IDYES then
      begin
        DelTree(CustomerDataDir, True, True, True);
      end;
    end;
  end;
end;
