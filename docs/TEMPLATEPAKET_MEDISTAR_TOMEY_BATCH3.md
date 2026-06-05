# Templatepakete MEDISTAR + TOMEY Referenzdatenbatch

Status: Kandidaten, reproduzierbar export-/importgeprueft, noch keine eingecheckten ZIP-Paketdateien

Diese Dokumentation beschreibt die testseitig vorbereiteten TOMEY-Templatepakete:

- MEDISTAR + TOMEY CF-2000
- MEDISTAR + TOMEY TL-2000C
- MEDISTAR + TOMEY TL-6000
- MEDISTAR + TOMEY TL-7000
- MEDISTAR + TOMEY MR-6000
- MEDISTAR + TOMEY TOP-1000
- MEDISTAR + TOMEY EM-3000
- MEDISTAR + TOMEY EM-4000

Die Pakete enthalten jeweils ein BuiltIn-Geraeteprofil, ein MEDISTAR-Exportprofil und ein inaktives Schnittstellenprofil ohne Live-Pfade. Sie werden ueber den selektiven Templatepaket-Export testseitig erzeugt und wieder importiert.

## Ausgabeziele

| Workflow | Ausgabe |
| --- | --- |
| TOMEY CF-2000 | Lensmeter nach `6228` |
| TOMEY TL-2000C | Lensmeter nach `6228` |
| TOMEY TL-6000 | Lensmeter nach `6228` |
| TOMEY TL-7000 | Lensmeter nach `6228` |
| TOMEY MR-6000 | REF nach `6228`, KM nach `6221`, Tonometrie nach `6205`, Pachymetrie/CCT nach `6220` |
| TOMEY TOP-1000 | Tonometrie nach `6205`, Pachymetrie/CCT nach `6220` |
| TOMEY EM-3000 | Endothel-/Zellmesswerte nach `6228`, Kommentare nach `6227`, Bild-/Dateiverweise nach `6302` |
| TOMEY EM-4000 | Zellmesswerte CD/CCT nach `6228`, Kommentare nach `6227`, Bild-/Dateiverweise nach `6302` |

## Sicherheitsgrenzen

- Die Schnittstellenprofile sind inaktiv und enthalten keine Praxisordner.
- Die Fixtures sind synthetisch aus Referenzlogik abgeleitet und enthalten keine Patientendaten.
- Es werden keine Referenzrohdateien, PDFs oder fremden Skripte in den Kundeninstaller aufgenommen.
- BuiltIn-Profile bleiben geschuetzt; Anwenderaenderungen erfolgen ueber UserDefined-Kopien.

## Tests

`MedistarTomeyTemplatePackageTests` prueft fuer alle acht Workflows:

- selektiven Paketinhalt aus dem Schnittstellenprofil,
- ZIP-/Importstruktur,
- keine Live-Pfade,
- keine fremden Systemnamen aus den Referenzdaten,
- sichere UserDefined-Uebernahme.

`ReferencePackageBuiltInDeviceTests` prueft die Parser- und Exportlogik gegen synthetische Fixtures. Fuer EM-3000/EM-4000 sind die Fixtures aus der erkannten CSV-Tokenlogik abgeleitet; echte Praxis-CSV-Dateien und Bildpfade muessen vor produktiver Nutzung noch abgenommen werden.

## Finaler lokaler Pruefstand 2026-06-05

Marc hat den EM-3000/EM-4000-Stand lokal ausserhalb der Sandbox vollstaendig bestaetigt:

- `dotnet build XdtDeviceBridge.sln`: erfolgreich fuer `XdtDeviceBridge.Core`, `XdtDeviceBridge.Infrastructure`, `XdtBox.LicenseIssuer`, `XdtBox.LicenseManager`, `XdtDeviceBridge.Tests` und `XdtDeviceBridge.App`; Builddauer 18,9 Sekunden.
- `dotnet test XdtDeviceBridge.sln`: 1816 Tests insgesamt, 1816 erfolgreich, 0 fehlgeschlagen, 0 uebersprungen; Testdauer 12,4 Sekunden, Gesamt erfolgreich in 16,4 Sekunden.

Damit ist der Code-/Teststand fuer die vorbereiteten TOMEY-EM-BuiltIns lokal final bestaetigt. Nach dem Installer-Staging-Umbau wurde `scripts\build-xdtbox-installer.ps1` erfolgreich ausgefuehrt; `C:\GitHub\XDT-MEDISTAR\artifacts\installer\XDTBox_Setup_1.0.exe` wurde mit 63.404.236 Bytes erzeugt, die Publish-Validierung lief sauber und die App startete aus `artifacts\publish\XDTBox`.

## Offene Abnahme

Vor produktiver Nutzung sind echte Praxisrohdateien und ein MEDISTAR-Importtest erforderlich. Erst danach sollte nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md` entschieden werden, ob offizielle ZIP-Artefakte dauerhaft abgelegt werden.
