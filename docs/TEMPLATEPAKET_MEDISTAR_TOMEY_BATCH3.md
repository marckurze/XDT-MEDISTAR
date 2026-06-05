# Templatepakete MEDISTAR + TOMEY Referenzdatenbatch

Status: Kandidaten, reproduzierbar export-/importgeprueft, noch keine eingecheckten ZIP-Paketdateien

Diese Dokumentation beschreibt die testseitig vorbereiteten TOMEY-Templatepakete:

- MEDISTAR + TOMEY CF-2000
- MEDISTAR + TOMEY TL-2000C
- MEDISTAR + TOMEY TL-6000
- MEDISTAR + TOMEY TL-7000
- MEDISTAR + TOMEY MR-6000
- MEDISTAR + TOMEY TOP-1000

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

## Sicherheitsgrenzen

- Die Schnittstellenprofile sind inaktiv und enthalten keine Praxisordner.
- Die Fixtures sind synthetisch aus Referenzlogik abgeleitet und enthalten keine Patientendaten.
- Es werden keine Referenzrohdateien, PDFs oder fremden Skripte in den Kundeninstaller aufgenommen.
- BuiltIn-Profile bleiben geschuetzt; Anwenderaenderungen erfolgen ueber UserDefined-Kopien.

## Tests

`MedistarTomeyTemplatePackageTests` prueft fuer alle sechs Workflows:

- selektiven Paketinhalt aus dem Schnittstellenprofil,
- ZIP-/Importstruktur,
- keine Live-Pfade,
- keine fremden Systemnamen aus den Referenzdaten,
- sichere UserDefined-Uebernahme.

`ReferencePackageBuiltInDeviceTests` prueft die Parser- und Exportlogik gegen synthetische Fixtures.

## Offene Abnahme

Vor produktiver Nutzung sind echte Praxisrohdateien und ein MEDISTAR-Importtest erforderlich. Erst danach sollte nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md` entschieden werden, ob offizielle ZIP-Artefakte dauerhaft abgelegt werden.
