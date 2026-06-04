# XDTBox Installation-, Update- und Deinstallations-Datenpolitik

Stand: 2026-06-04

Dieses Dokument beschreibt die Datenabgrenzung fuer einen spaeteren XDTBox-Installer. Es ist bewusst noch kein Installer-Konzept und baut keinen Installer. Ziel ist, vorab festzulegen, welche Dateien bei Installation, Update und Deinstallation ersetzt werden duerfen und welche Kundendaten geschuetzt bleiben muessen.

## Grundsatz

XDTBox trennt drei Bereiche:

1. App-Bestandteile duerfen durch Installer und Updates ersetzt werden.
2. Kundendaten duerfen bei Updates nicht geloescht oder ungefragt ueberschrieben werden.
3. Externe Praxisordner duerfen vom Installer und Deinstaller niemals blind geloescht werden.

Die produktive Verarbeitung, Parser, MEDISTAR-Mapping, CV-5000-/RT-6100-Logik und RS232-Kommunikation werden durch diese Datenpolitik nicht fachlich veraendert.

## Aktueller Datenstamm

Der aktuelle lokale Kundendatenstamm liegt noch unter:

```text
%LocalAppData%\XdtDeviceBridge
```

Der Name ist historisch aus dem Projekt entstanden. Fuer eine spaetere produktive Maschineninstallation ist als Zielbild ein gemeinsamer XDTBox-Datenstamm unter `ProgramData` empfohlen:

```text
%ProgramData%\XDTBox
```

Eine solche Migration ist ein eigener spaeterer Schritt. Sie darf nur mit Backup, klarer Diagnose und Rollback-Plan erfolgen.

## App-Bestandteile

Diese Bestandteile sind Teil der installierten Anwendung und duerfen bei Updates ersetzt werden:

- `XdtDeviceBridge.App.exe`
- DLLs, Runtime-Dateien und Abhaengigkeiten
- WPF-Themes und Styles
- lokale Hilfe unter `Assets/Help`
- Branding, Icons und Standard-Assets
- Standard-Geraetebilder als App-Assets
- BuiltIn-Definitionen im Code

BuiltIn-Definitionen sind keine Kundendaten. Sie duerfen durch eine neue App-Version bereitgestellt und ueber BuiltIn-Repair aktualisiert werden. Dabei gilt: Nur BuiltIns werden repariert, UserDefined-Profile und konkrete Praxisparameter nicht.

## Kundendaten

Folgende Daten sind Kundendaten und bleiben bei Updates erhalten:

- Profile unter `profiles`
- AIS-, Geraete-, Export- und Schnittstellenprofile
- UserDefined-Profile
- konkrete Ordnerpfade und COM-Port-Parameter in Schnittstellenprofilen
- lokale Baukasten-Templates und Templatepakete unter `template-packages`
- vorbereiteter Legacy-/lokaler Templateordner `templates`
- Lizenzordner `licenses`
- `license.json`
- `license.xdtboxlic`
- `license-customer-data.json`
- `device-grace-periods.json`
- Lizenzanforderungen unter `license-requests`
- lokale Geraetebilder unter `DeviceImages`
- `device-image-overrides.json`
- UI-Komfortdaten wie `ui/app-settings.json`
- UI-Floating-Fensterstatus `ui/floating-interface-windows.json`
- `installation.json`
- Logs und Diagnoseausgaben
- XDTBox-Backups, z. B. unter `C:\XDTBox\Backup`

Updates duerfen diese Daten nicht blind loeschen. Wenn neue BuiltIns fehlen, duerfen sie ergaenzt werden. Wenn vorhandene BuiltIns repariert werden, duerfen UserDefined-Daten nicht veraendert werden.

## Externe Praxisordner

Schnittstellenprofile koennen externe Ordner enthalten, zum Beispiel:

```text
C:\XDTBox\RT3100RS232\Patient2Box
D:\Praxis\AIS\Export
\\SERVER\Praxis\Geraete\Import
```

Solche Ordner enthalten potentiell Praxisdaten, Patientendateien, Geraetedateien, Exportdateien, Archiv- oder Fehlerdaten. Sie sind nicht Teil des Installationsordners und nicht Teil des internen Kundendatenstamms.

Der Installer und Deinstaller duerfen diese Ordner niemals automatisch bereinigen oder loeschen. Auch wenn ein Pfad in einem XDTBox-Profil konfiguriert ist, gehoert er nicht automatisch XDTBox.

## Herstellerwerkzeuge und private Schluessel

Das interne Hersteller-Lizenztool nutzt eigene Daten unter:

```text
C:\XDTBox\Lizenzaktivierung
```

Darunter liegen unter anderem:

- ausgestellte Lizenzen
- Lizenzhistorie
- Hersteller-Einstellungen
- private Hersteller-Schluessel

Diese Daten gehoeren nicht in den Kundeninstaller. Private Hersteller-Schluessel duerfen niemals mit dem Kunden-Setup verteilt, geloescht oder migriert werden.

## Backup/Restore-Abdeckung

Der Tab `Sicherung/Umzug` erzeugt `.xdtboxbackup`-Dateien. Gesichert werden aktuell:

- Profile inklusive UserDefined-Profile
- lokale Templatepakete und Baukasten-Templates
- lokale Geraetebilder
- `device-image-overrides.json`
- UI-AppSettings
- Floating-Fensterstatus
- Lizenz-Kundendaten
- `device-grace-periods.json`
- optional `license.xdtboxlic`

Nicht gesichert werden:

- Patientendaten
- Geraete-Messdateien
- AIS-Importordner-Inhalte
- Geraete-Importordner-Inhalte
- Exportordner-Inhalte
- Archivordner-Inhalte
- Fehlerordner-Inhalte
- externe Praxisordner
- Hersteller-Private-Keys

Der vorbereitete lokale Ordner `templates` ist als Kundendatenordner klassifiziert. Die aktuelle Sicherung deckt praktisch die genutzten lokalen `template-packages` ab; falls der Legacy-Ordner `templates` spaeter wieder aktiv genutzt wird, muss seine Backup-Abdeckung vor einem Installer erneut geprueft werden.

## Update-Regeln

Bei einem Update gilt:

- App-Dateien ersetzen: erlaubt.
- Hilfe, Themes und Standard-Assets ersetzen: erlaubt.
- BuiltIns ergaenzen oder BuiltIn-Repair ausfuehren: erlaubt.
- UserDefined-Profile ueberschreiben: nicht erlaubt.
- Schnittstellenprofile mit Praxisordnern oder COM-Port-Parametern ueberschreiben: nicht erlaubt.
- Lizenzdateien loeschen: nicht erlaubt.
- lokale Geraetebilder und Overrides loeschen: nicht erlaubt.
- Backups loeschen: nicht erlaubt.
- externe Praxisordner anraeitern oder bereinigen: nicht erlaubt.

Vor groesseren Updates soll die App den Nutzer auf eine Sicherung hinweisen. Der Installer selbst soll vorhandene Kundendaten nicht still migrieren, wenn kein getesteter Migrationspfad vorhanden ist.

## Deinstallation Variante B

Fuer XDTBox gilt als Zielentscheidung Variante B:

1. Standard-Deinstallation entfernt nur die Anwendung.
2. Kundendaten bleiben standardmaessig erhalten.
3. Optional kann der Deinstaller Kundendaten loeschen, aber nur nach ausdruecklicher, separater Bestaetigung.
4. Die Bestaetigung muss klar benennen, dass Profile, Lizenzen, lokale Geraetebilder, Baukasten-Templates, UI-Einstellungen und Backups betroffen sein koennen.
5. Externe Praxisordner werden auch dann nicht automatisch geloescht.

Damit kann XDTBox deinstalliert und spaeter wieder installiert werden, ohne dass Praxiskonfigurationen, Lizenzdaten oder lokale Profile verloren gehen.

## Technische Absicherung

Die zentrale technische Klassifikation liegt in:

```text
XdtDeviceBridge.Infrastructure/XdtBoxInstallationDataPolicy.cs
```

Sie unterscheidet App-Komponenten, BuiltIn-Templates, Kundendaten, temporaere Diagnosedaten, externe Praxisdaten, Herstellerdaten und Hersteller-Private-Keys. Tests sichern ab, dass Kundendaten als zu schuetzende Daten klassifiziert werden und externe Praxisordner beim Deinstallationsentscheid niemals automatisch geloescht werden.

## Offene Punkte vor einem echten Installer

- Installationsziel final festlegen, z. B. `%ProgramFiles%\XDTBox`.
- ProgramData-Migration nur mit eigenem Migrationskonzept umsetzen.
- Installer-Technologie auswaehlen.
- Signatur und Updatekanal klaeren.
- Vor Update eine Backup-Empfehlung oder Backup-Aktion anbieten.
- Deinstaller-Dialog fuer Variante B textlich und technisch finalisieren.
- Rechtekonzept fuer Terminalserver/Praxisarbeitsplaetze pruefen.
- Hersteller-Lizenztools strikt vom Kundeninstaller trennen.
