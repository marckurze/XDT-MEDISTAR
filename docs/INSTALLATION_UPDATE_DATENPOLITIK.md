# XDTBox Installation-, Update- und Deinstallations-Datenpolitik

Stand: 2026-06-09

Dieses Dokument beschreibt die Datenabgrenzung fuer den XDTBox-Installer ab Version 1.0. Ziel ist, verbindlich festzulegen, welche Dateien bei Installation, Update und Deinstallation ersetzt werden duerfen und welche Kundendaten geschuetzt bleiben muessen.

## Grundsatz

XDTBox trennt drei Bereiche:

1. App-Bestandteile duerfen durch Installer und Updates ersetzt werden.
2. Kundendaten duerfen bei Updates nicht geloescht oder ungefragt ueberschrieben werden.
3. Externe Praxisordner duerfen vom Installer und Deinstaller niemals blind geloescht werden.

Die produktive Verarbeitung, Parser, MEDISTAR-Mapping, CV-5000-/RT-6100-Logik und RS232-Kommunikation werden durch diese Datenpolitik nicht fachlich veraendert.

## Installer 1.11

XDTBox 1.11 nutzt ein Inno-Setup-Skript unter `installer/XDTBox.iss`. Der Installer installiert die Kunden-App standardmaessig nach `C:\XDTBox`, erlaubt einen anderen Installationsordner, erzeugt Startmenue- und optionale Desktop-Verknuepfungen, schreibt Registry-/Uninstall-Informationen und legt im Installationsordner `XDTBox.installation.json` als Installationsmarker ab.

Der Build erfolgt reproduzierbar ueber `scripts/build-xdtbox-installer.ps1`; die Buildanleitung steht in `docs/INSTALLER_BUILD_ANLEITUNG.md`. Der Kundeninstaller enthaelt nur die Publish-Ausgabe von `XdtDeviceBridge.App`. `XdtBox.LicenseManager`, `XdtBox.LicenseIssuer`, private Hersteller-Schluessel, Lizenzhistorien und Hersteller-Einstellungen werden nicht aufgenommen.

Der XDTBox Lizenzmanager besitzt eine eigene Version (`1.11`) und ein eigenes Setup. Kunden-App- und Lizenzmanager-Version duerfen auseinanderlaufen. Setups werden ab diesem Stand nur neu gebaut, wenn Marc dies ausdruecklich beauftragt.

Vor jedem Neubau bereinigt das Buildskript ausschliesslich die Build-Artefakte `artifacts\publish\XDTBox` und `artifacts\installer`. Es loescht keine Kundendatenordner, kein `%LocalAppData%\XdtDeviceBridge`, kein `C:\XDTBox` und keine externen Praxisordner.

Vor dem Publish prueft das Buildskript ausserdem die offiziellen BuiltIn-Geraetebilder unter `XdtDeviceBridge.App\Assets\Devices`. Fehlt eines der kuratierten App-Assets, bricht der Installerbuild vor dem Publish ab. Dadurch bleibt der Kundeninstaller weiterhin frei von lokalen Overrides, enthaelt aber die offiziellen BuiltIn-Bilder fuer einen nackten First-Run.

Nach dem Publish prueft das Buildskript hart, ob lokale Kundendaten oder Entwicklungsdaten in die Kunden-App-Ausgabe geraten sind. Verboten sind unter anderem UserDefined-/persistierte Profilordner, lokale Schnittstellenprofile, `app-settings.json`, Lizenzdateien, Geraetebild-Overrides, Baukasten-Templates, lokale Templatepakete, Backups, Diagnose-Logs, Testfixtures, Hersteller-Lizenztools, private Schluessel und konkrete Entwicklungsdatenmuster wie Marc-Pfade oder RT3100-Testordner. Bei einem Treffer bricht der Build mit einer klaren Meldung ab.

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
- Standard-Geraetebilder als App-Assets unter `XdtDeviceBridge.App\Assets\Devices`, referenziert ueber `pack://application:,,,/Assets/Devices/...`
- Standard-Geraete-Steckbriefe als App-Assets unter `XdtDeviceBridge.App\Assets\DeviceInfo`
- BuiltIn-Definitionen im Code

BuiltIn-Definitionen sind keine Kundendaten. Sie duerfen durch eine neue App-Version bereitgestellt und ueber BuiltIn-Repair aktualisiert werden. Dabei gilt: Nur BuiltIns werden repariert, UserDefined-Profile und konkrete Praxisparameter nicht.

Die Geraetebild-Aufloesung bleibt bewusst kundenfreundlich: ein lokaler Override wird zuerst verwendet, danach das BuiltIn-App-Asset und erst danach der stabile Platzhalter. Dadurch behalten Praxen eigene Bilder nach Updates, waehrend eine Neuinstallation ohne lokale Overrides trotzdem offizielle BuiltIn-Bilder zeigt. Dokumentgeraete haben getrennte offizielle Assets fuer `Generisches Dokumentgeraet` und `Manuelle Dokumentauswahl`.

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
- Rechnungs-E-Mail, Bankdaten, SEPA-Zustimmung und Rechnung-Option in den Lizenz-Kundendaten
- `device-grace-periods.json`
- Lizenzanforderungen unter `license-requests`
- lokale Geraetebilder unter `DeviceImages`
- `device-image-overrides.json`
- `device-info-overrides.json` fuer lokale Geraete-Steckbrief-Anpassungen und Techniker-Notizen
- Techniker-Whiteboard unter `technician-notes`
- Tab-Schutz-Konfiguration unter `ui/tab-protection.json`
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
- `device-info-overrides.json`
- Techniker-Whiteboard unter `technician-notes`
- Tab-Schutz-Konfiguration unter `ui/tab-protection.json`
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
- offizielle BuiltIn-Geraetebilder als App-Assets ersetzen oder ergaenzen: erlaubt.
- BuiltIns ergaenzen oder BuiltIn-Repair ausfuehren: erlaubt.
- UserDefined-Profile ueberschreiben: nicht erlaubt.
- Schnittstellenprofile mit Praxisordnern oder COM-Port-Parametern ueberschreiben: nicht erlaubt.
- Lizenzdateien loeschen: nicht erlaubt.
- lokale Geraetebilder und Overrides loeschen: nicht erlaubt.
- lokale Techniker-Notizen oder Tab-Schutz-Konfiguration loeschen: nicht erlaubt.
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

## Offene Punkte nach Installer 1.0

- ProgramData-Migration nur mit eigenem Migrationskonzept umsetzen.
- Signatur und Updatekanal klaeren.
- Vor Update eine Backup-Empfehlung oder Backup-Aktion anbieten.
- Rechtekonzept fuer Terminalserver/Praxisarbeitsplaetze pruefen.
- Installierte Setup-Datei auf einem separaten Windows-Testsystem praktisch gegen Neuinstallation, Update, Deinstallation und Programme-&-Features-Eintrag abnehmen.
