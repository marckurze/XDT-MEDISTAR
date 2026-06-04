# XDTBox 1.0 Installer bauen

Stand: 2026-06-04

XDTBox 1.0 nutzt fuer das Kunden-Setup Inno Setup 6. Die Entscheidung ist bewusst pragmatisch: XDTBox ist eine klassische lokale Windows-Desktopanwendung, braucht einen frei waehlbaren Installationspfad mit Default `C:\XDTBox`, Startmenue-/Desktop-Verknuepfungen, einen Eintrag in Windows "Programme & Features", Update-Erkennung und einen Deinstaller, der Kundendaten standardmaessig behaelt.

## Voraussetzungen

- Windows 10 oder neuer
- .NET 8 SDK fuer den Build-PC
- Inno Setup 6 mit `ISCC.exe`
- Optional: Umgebungsvariable `ISCC_EXE`, falls `ISCC.exe` nicht im PATH liegt

Der Kundeninstaller wird self-contained fuer `win-x64` gebaut. Kunden muessen dadurch keine separate .NET Desktop Runtime installieren.

## Buildbefehl

```powershell
.\scripts\build-xdtbox-installer.ps1
```

Das Skript fuehrt aus:

1. `dotnet publish` fuer `XdtDeviceBridge.App`
2. Sicherheitspruefung der Publish-Ausgabe
3. Inno-Setup-Build von `installer\XDTBox.iss`

Ergebnis:

```text
artifacts\publish\XDTBox
artifacts\installer\XDTBox_Setup_1.0.exe
```

Wenn Inno Setup auf dem Build-PC fehlt, bleibt die Publish-Ausgabe bestehen und das Skript meldet klar, dass `ISCC.exe` installiert oder per `ISCC_EXE` angegeben werden muss.

## Kundeninstaller-Inhalt

Enthalten sind nur die veroeffentlichten Dateien der Kunden-App:

- `XdtDeviceBridge.App.exe`
- benoetigte DLLs und Runtime-Dateien
- Hilfe, Themes, Icons und App-Assets
- BuiltIn-Definitionen als App-Bestandteil

Nicht enthalten:

- `XdtBox.LicenseManager`
- `XdtBox.LicenseIssuer`
- private Hersteller-Schluessel
- Hersteller-Lizenzhistorie
- Entwicklungs-, Test-, Codex- oder Repository-Artefakte

## Installation

Das Setup fragt nach:

- Neuinstallation
- Bestehende XDTBox aktualisieren

Bei Neuinstallation wird `C:\XDTBox` vorgeschlagen. Der Anwender kann den Installationsordner aendern. Wenn im Zielordner bereits XDTBox erkannt wird, empfiehlt das Setup ein Update.

Bei Update sucht das Setup eine bestehende Installation ueber:

- `HKLM\Software\XDTBox`
- Windows-Uninstall-Registry
- `C:\XDTBox`
- `XdtDeviceBridge.App.exe`
- `XDTBox.installation.json`

Wenn keine bestehende Installation erkannt wird, muss der Anwender den Installationsordner manuell waehlen. Ein unplausibler Ordner wird nicht blind aktualisiert.

## Kundendaten

Der Datenstamm bleibt in Version 1.0:

```text
%LocalAppData%\XdtDeviceBridge
```

Updates ersetzen nur App-Dateien. Geschuetzt bleiben unter anderem:

- UserDefined-Profile
- Schnittstellenprofile mit Ordnerpfaden, COM-Port, DTR/RTS, NIDEK-RT Sendemodus und Sendeinhalt
- Lizenzdaten inklusive `license.xdtboxlic`
- Baukasten-Templates und Templatepakete
- lokale Geraetebilder und `device-image-overrides.json`
- AppSettings und UI-State
- externe AIS-/Geraete-/Archiv-/Fehlerordner

Eine Migration nach `%ProgramData%\XDTBox` ist nicht Teil dieses Installers und braucht spaeter ein eigenes Migrationskonzept.

## Deinstallation Variante B

Der Deinstaller entfernt standardmaessig nur die App-Dateien. Kundendaten bleiben erhalten.

Nach der Deinstallation fragt der Deinstaller separat, ob lokale XDTBox-Kundendaten unter `%LocalAppData%\XdtDeviceBridge` ebenfalls geloescht werden sollen. Default ist "Nein". Vor dem Loeschen ist eine zweite Bestaetigung erforderlich.

Externe Praxisordner werden niemals geloescht.

## Manuelle Pruefschritte

1. `.\scripts\build-xdtbox-installer.ps1` ausfuehren.
2. `artifacts\installer\XDTBox_Setup_1.0.exe` starten.
3. Neuinstallation in einem Testordner durchfuehren.
4. App starten und im Info-Dialog Version `1.0` pruefen.
5. Update erneut ueber denselben Ordner ausfuehren.
6. Pruefen, dass Kundendaten unter `%LocalAppData%\XdtDeviceBridge` erhalten bleiben.
7. Deinstallation aus Windows "Programme & Features" ausfuehren.
8. Standardfall pruefen: Kundendaten bleiben erhalten.
9. Optionalen Komplettentfernungsdialog separat in einer Testumgebung pruefen.
