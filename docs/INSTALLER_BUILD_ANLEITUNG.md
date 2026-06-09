# XDTBox 1.10 Installer bauen

Stand: 2026-06-04

XDTBox 1.10 nutzt fuer das Kunden-Setup Inno Setup 6. Die Entscheidung ist bewusst pragmatisch: XDTBox ist eine klassische lokale Windows-Desktopanwendung, braucht einen frei waehlbaren Installationspfad mit Default `C:\XDTBox`, Startmenue-/Desktop-Verknuepfungen, einen Eintrag in Windows "Programme & Features", Update-Erkennung und einen Deinstaller, der Kundendaten standardmaessig behaelt.

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

1. Bereinigung nur des Staging-Ordners `artifacts\staging\xdtbox-installer`
2. Pruefung, dass die offiziellen BuiltIn-Geraetebilder als App-Assets unter `XdtDeviceBridge.App\Assets\Devices` vorhanden sind
3. `dotnet publish` fuer `XdtDeviceBridge.App` in den Staging-Publish
4. harte Sicherheitspruefung der Staging-Publish-Ausgabe
5. Inno-Setup-Build von `installer\XDTBox.iss` gegen den Staging-Publish
6. Ersetzen der finalen Ordner `artifacts\publish\XDTBox` und `artifacts\installer` erst nach erfolgreichem Publish, erfolgreicher Publish-Validierung und erfolgreichem Inno-Build

Ergebnis:

```text
artifacts\publish\XDTBox
artifacts\installer\XDTBox_Setup_1.10.exe
```

Wenn Inno Setup auf dem Build-PC fehlt, bleibt die Publish-Ausgabe bestehen und das Skript meldet klar, dass `ISCC.exe` installiert oder per `ISCC_EXE` angegeben werden muss.

## Aktueller Buildstatus 2026-06-04

Der Installer wurde mit Inno Setup 6.7.3 erzeugt:

```text
C:\Program Files (x86)\Inno Setup 6\ISCC.exe
artifacts\installer\XDTBox_Setup_1.10.exe
```

Die erzeugte Setup-Datei ist noch nicht digital codesigniert. Das ist fuer die erste technische Pruefung dokumentiert; vor Kundenverteilung ist eine Codesignatur beziehungsweise ein definierter Verteilprozess noch offen.

Eine stille Testinstallation aus der Codex-Sitzung konnte nicht vollautomatisch ausgefuehrt werden, weil der aktuelle Prozess nicht mit Administratorrechten lief. Das ist erwartbar, da das Setup fuer `C:\XDTBox` und HKLM/Windows "Programme & Features" bewusst `PrivilegesRequired=admin` nutzt. Die praktische Installation, Update-Erkennung und Deinstallation sollen daher mit Admin-Rechten auf einem Windows-Testsystem ausgefuehrt werden.

## Nachtrag 2026-06-05: EM-3000/EM-4000-Finalstand

Marc hat den finalen EM-3000/EM-4000-Code-/Teststand lokal ausserhalb der Sandbox bestaetigt:

- `dotnet build XdtDeviceBridge.sln`: erfolgreich fuer `XdtDeviceBridge.Core`, `XdtDeviceBridge.Infrastructure`, `XdtBox.LicenseIssuer`, `XdtBox.LicenseManager`, `XdtDeviceBridge.Tests` und `XdtDeviceBridge.App`; Dauer 18,9 Sekunden.
- `dotnet test XdtDeviceBridge.sln`: 1816 Tests insgesamt, 1816 erfolgreich, 0 fehlgeschlagen, 0 uebersprungen; Testdauer 12,4 Sekunden, Gesamt erfolgreich in 16,4 Sekunden.

Der Installer-Build wurde in der Codex-Sandbox erneut gestartet, konnte dort aber nicht final bestaetigt werden: `dotnet publish` brach mit verweigertem Zugriff auf `C:\Users\MarcK\AppData\Local\Microsoft SDKs` ab. Im frueheren Buildskript wurden `artifacts\publish\XDTBox` und `artifacts\installer` noch vor dem Publish bereinigt; dadurch waren nach diesem fehlgeschlagenen Sandbox-Lauf keine finalen Installer-Artefakte mehr vorhanden.

Dieses Risiko ist im Buildskript jetzt abgesichert: `scripts\build-xdtbox-installer.ps1` arbeitet zuerst in `artifacts\staging\xdtbox-installer`. Finale Publish-/Installer-Artefakte werden nur noch ersetzt, wenn `dotnet publish`, die Kundenpublish-Validierung und der Inno-Build erfolgreich waren. Ein Publish-Fehler, ein blockierter SDK-Zugriff oder ein fehlendes Inno-Setup loescht dadurch nicht mehr das letzte funktionierende Setup.

Nach dem Staging-Umbau wurde der Installer-Build mit Vollzugriff erfolgreich ausgefuehrt:

- Setup-Datei: `C:\GitHub\XDT-MEDISTAR\artifacts\installer\XDTBox_Setup_1.10.exe`
- Dateigroesse: 63.430.332 Bytes
- Zeitstempel: 2026-06-05 23:39:52
- Publish-EXE: `C:\GitHub\XDT-MEDISTAR\artifacts\publish\XDTBox\XdtDeviceBridge.App.exe`
- Publish-EXE-Groesse: 421.376 Bytes
- Publish-EXE-Zeitstempel: 2026-06-05 18:26:43
- Publish-Validierung: sauber durchlaufen; der Inno-Build wurde erst nach der Kundenpublish-Pruefung gestartet

Nach der Schnittstellenprofil-/AIS-Ausgabe-Info-Korrektur wurde der vollstaendige Stand erneut geprueft:

- `dotnet build XdtDeviceBridge.sln`: erfolgreich, 0 Warnungen, 0 Fehler.
- `dotnet test XdtDeviceBridge.sln`: 1933 Tests insgesamt, 1933 erfolgreich, 0 fehlgeschlagen, 0 uebersprungen.
- Publish-App-Start: `C:\GitHub\XDT-MEDISTAR\artifacts\publish\XDTBox\XdtDeviceBridge.App.exe` startete erfolgreich und wurde danach wieder beendet.
- App-Start aus Publish: `XdtDeviceBridge.App.exe` startete erfolgreich aus `artifacts\publish\XDTBox` und wurde fuer die Pruefung wieder beendet
- Zusaetzliche Sperrmarker-Pruefung nach dem Installer-Build: Repository-Textscan und Publish-/Setup-Klartextscan ohne Treffer fuer die vier gesperrten externen Marker. Bei einer rein binaeren Suche nach dem sehr kurzen 3-Byte-Marker entstehen erwartbare Zufallsfolgen in kompilierten DLL-/Setup-Binaerdaten; es gibt keinen lesbaren Klartexttreffer.

Ein absichtlich fehlschlagender Kontrolllauf mit ungueltigem RuntimeIdentifier bestaetigte den Schutz: `dotnet publish` brach ab, das Script meldete den Fehler und ersetzte weder `artifacts\publish\XDTBox` noch `artifacts\installer`.

Fuer kuenftige Release-Artefakte muss auf dem lokalen Build-PC erneut ausgefuehrt und protokolliert werden:

```powershell
.\scripts\build-xdtbox-installer.ps1
```

Zu dokumentieren sind danach:

- Pfad und Groesse von `artifacts\installer\XDTBox_Setup_1.10.exe`
- erfolgreiche Publish-Validierung ohne lokale Entwicklungs-/Kundendaten
- erfolgreicher App-Start aus `artifacts\publish\XDTBox\XdtDeviceBridge.App.exe`

## Nachtrag 2026-06-06: UI-Ausbau-Finalstand

Nach dem UI-Ausbau fuer Geraetesteckbrief-Buttons, Aktivierungsstatusbutton, Techniker-Whiteboard, Tab-Schutz und erweiterte Lizenz-Kundendaten wurde der vollstaendige Stand erneut geprueft:

- `dotnet build XdtDeviceBridge.sln`: erfolgreich, 0 Warnungen, 0 Fehler.
- `dotnet test XdtDeviceBridge.sln`: 1984 Tests insgesamt, 1984 erfolgreich, 0 fehlgeschlagen, 0 uebersprungen.
- Installer-Build: `scripts\build-xdtbox-installer.ps1` erfolgreich.
- Setup-Datei: `C:\GitHub\XDT-MEDISTAR\artifacts\installer\XDTBox_Setup_1.10.exe`
- Dateigroesse: 79.314.259 Bytes
- Zeitstempel: 2026-06-06 17:55:40
- Publish-EXE: `C:\GitHub\XDT-MEDISTAR\artifacts\publish\XDTBox\XdtDeviceBridge.App.exe`
- Publish-EXE-Groesse: 421.376 Bytes
- Publish-EXE-Zeitstempel: 2026-06-06 17:53:41
- Publish-Validierung: sauber durchlaufen; der Inno-Build wurde erst nach der Kundenpublish-Pruefung gestartet.
- App-Start aus Publish: `XdtDeviceBridge.App.exe` startete erfolgreich aus `artifacts\publish\XDTBox` und wurde fuer die Pruefung wieder beendet.

## Kundeninstaller-Inhalt

Der Kundeninstaller enthaelt ausschliesslich App-Dateien und BuiltIn-Werksvorlagen. BuiltIns liegen im App-Code beziehungsweise in App-Assets und sind erlaubt. Persistierte Profile, UserDefined-Profile und konkrete Schnittstellenprofile sind Kundendaten und duerfen nicht in den Installer.

Enthalten sind nur die veroeffentlichten Dateien der Kunden-App:

- `XdtDeviceBridge.App.exe`
- benoetigte DLLs und Runtime-Dateien
- Hilfe, Themes, Icons und App-Assets
- BuiltIn-Definitionen als App-Bestandteil
- offizielle BuiltIn-Geraetebilder als WPF-Ressourcen aus `XdtDeviceBridge.App\Assets\Devices`, darunter NIDEK ARK1S, AR360, LM7/LM7P, NT530P, RT-2100/RT-3100/RT-5100 RS232, RT-6100, TOPCON CV-5000/CV-5000S, CL-300, Solos, KR-800S, KR-1, TRK2P, CT-1P, CT-800A, das generische Dokumentgeraet und die manuelle Dokumentauswahl

Nicht enthalten:

- UserDefined-Profile
- lokale Schnittstellenprofile
- konkrete Ordnerpfade oder COM-Port-Einstellungen aus Entwicklung/Praxis
- lokale Baukasten-Templates und Templatepakete
- `device-image-overrides.json`
- lokale `DeviceImages` aus `%LocalAppData%\XdtDeviceBridge`
- lokale `device-info-overrides.json`
- lokale `technician-notes`
- lokale `ui\tab-protection.json`
- `app-settings.json`
- `license.xdtboxlic`, `license.json`, `device-grace-periods.json`
- Backups und Diagnose-Logs
- Testfixtures oder Entwicklungsdaten
- `XdtBox.LicenseManager`
- `XdtBox.LicenseIssuer`
- private Hersteller-Schluessel
- Hersteller-Lizenzhistorie
- Entwicklungs-, Test-, Codex- oder Repository-Artefakte

Die Publish-Validierung bricht den Build ab, wenn solche Dateien, Ordner oder bekannte Entwicklungsdatenmuster im Kundenpublish auftauchen. Die Fehlermeldung beginnt mit:

```text
Kundenpublish enthaelt lokale Kundendaten/Entwicklungsdaten:
```

Die offiziellen BuiltIn-Geraetebilder sind ausdruecklich keine lokalen Overrides. Sie werden als App-Ressourcen eingebettet und ueber `pack://application:,,,/Assets/Devices/...` referenziert. Bei einem nackten First-Run ohne `%LocalAppData%\XdtDeviceBridge` muss daher kein lokaler Bild-Override vorhanden sein, damit BuiltIn-Geraete Bilder anzeigen.

Wichtig fuer Tests auf dem Entwicklungsrechner: Wenn XDTBox nach einer Testinstallation lokale Marc-/Entwicklungsprofile zeigt, bedeutet das nicht automatisch, dass der Installer diese Daten eingepackt hat. Die App liest beim Start weiterhin den bestehenden lokalen Datenstamm des Windows-Benutzers unter `%LocalAppData%\XdtDeviceBridge`. Fuer einen echten "nackten" First-Run-Test muss dieser lokale Datenstamm vorher gesichert und fuer den Test umbenannt werden oder der Test unter einem frischen Windows-Benutzer beziehungsweise auf einer sauberen VM laufen.

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
- lokale Geraetesteckbrief-Overrides und Techniker-Whiteboard-Notizen
- Tab-Schutz-Konfiguration
- AppSettings und UI-State
- erweiterte Lizenz-Kundendaten fuer Rechnung, SEPA und Bankdaten
- externe AIS-/Geraete-/Archiv-/Fehlerordner

Eine Migration nach `%ProgramData%\XDTBox` ist nicht Teil dieses Installers und braucht spaeter ein eigenes Migrationskonzept.

## Deinstallation Variante B

Der Deinstaller entfernt standardmaessig nur die App-Dateien. Kundendaten bleiben erhalten.

Nach der Deinstallation fragt der Deinstaller separat, ob lokale XDTBox-Kundendaten unter `%LocalAppData%\XdtDeviceBridge` ebenfalls geloescht werden sollen. Default ist "Nein". Vor dem Loeschen ist eine zweite Bestaetigung erforderlich.

Externe Praxisordner werden niemals geloescht.

## Manuelle Pruefschritte

1. `.\scripts\build-xdtbox-installer.ps1` ausfuehren.
2. `artifacts\installer\XDTBox_Setup_1.10.exe` starten.
3. Neuinstallation in einem Testordner durchfuehren.
4. App starten und im Info-Dialog Version `1.10` pruefen.
5. Fuer einen nackten First-Run `%LocalAppData%\XdtDeviceBridge` vorher sichern/umbenennen oder einen frischen Windows-Benutzer verwenden.
6. App starten und in `Profilverwaltung` beziehungsweise `XDT-Baukasten` pruefen, dass BuiltIn-Geraete ihre offiziellen Bilder aus App-Assets zeigen und keine lokalen UserDefined-/Override-Daten geladen werden.
7. Update erneut ueber denselben Ordner ausfuehren.
8. Pruefen, dass Kundendaten unter `%LocalAppData%\XdtDeviceBridge` erhalten bleiben.
9. Deinstallation aus Windows "Programme & Features" ausfuehren.
10. Standardfall pruefen: Kundendaten bleiben erhalten.
11. Optionalen Komplettentfernungsdialog separat in einer Testumgebung pruefen.
