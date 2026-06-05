# Templatepaket-Kandidaten MEDISTAR + Shin-Nippon Batch 4

Stand: 2026-06-05

Dieser Sammelvermerk beschreibt die testseitig abgesicherten Templatepaket-Kandidaten fuer die Shin-Nippon-BuiltIns aus Batch 4. Die Kandidaten sind als Startkonfigurationen gedacht, bleiben aber bis zu echten Praxisrohdateien und MEDISTAR-Abnahme fachlich vorlaeufig.

## Kandidaten

| Templatepaket | Geraeteprofil | Exportprofil | Schnittstellenprofil | Status |
| --- | --- | --- | --- | --- |
| MEDISTAR + Shin-Nippon Accuref R-800 | `device-shin-nippon-accuref-r800-default` | `export-medistar-shin-nippon-accuref-r800-default` | `interface-medistar-shin-nippon-accuref-r800-default` | testseitig vorbereitet |
| MEDISTAR + Shin-Nippon Accuref K-900 | `device-shin-nippon-accuref-k900-default` | `export-medistar-shin-nippon-accuref-k900-default` | `interface-medistar-shin-nippon-accuref-k900-default` | testseitig vorbereitet |
| MEDISTAR + Shin-Nippon DL-1000 | `device-shin-nippon-dl1000-default` | `export-medistar-shin-nippon-dl1000-default` | `interface-medistar-shin-nippon-dl1000-default` | testseitig vorbereitet |
| MEDISTAR + Shin-Nippon DL-800 | `device-shin-nippon-dl800-default` | `export-medistar-shin-nippon-dl800-default` | `interface-medistar-shin-nippon-dl800-default` | testseitig vorbereitet |
| MEDISTAR + Shin-Nippon DL-900 | `device-shin-nippon-dl900-default` | `export-medistar-shin-nippon-dl900-default` | `interface-medistar-shin-nippon-dl900-default` | testseitig vorbereitet |
| MEDISTAR + Shin-Nippon NCT-200 | `device-shin-nippon-nct200-default` | `export-medistar-shin-nippon-nct200-default` | `interface-medistar-shin-nippon-nct200-default` | testseitig vorbereitet |
| MEDISTAR + Shin-Nippon SLM-4000 | `device-shin-nippon-slm4000-default` | `export-medistar-shin-nippon-slm4000-default` | `interface-medistar-shin-nippon-slm4000-default` | testseitig vorbereitet |

## Exportregeln

- Accuref R-800: REF rechts/links nach `6228`.
- Accuref K-900: REF nach `6228`, KM nach `6221`.
- DL-1000/DL-800/DL-900/SLM-4000: Lensmeter rechts/links nach `6228`.
- NCT-200: Tonometrie nach `6205`.

Die Profile erzeugen keine kuenstlichen Trenner, keine `6330`-Zeilen und keine Werte ohne erkannte Quelle.

## Sicherheit

Die Schnittstellenprofile sind inaktiv, enthalten keine konkreten Praxisordner, keine lokalen Entwicklungspfade und keine COM-Port-Namen. Templatepaket-Tests pruefen Export, Reimport, SerialSettings und dass keine externen Referenzmarker in das Paket wandern.

## Finaler Pruefstand 2026-06-05

Der Batch-4-Stand wurde mit einem vollstaendigen Solution-Build und Testlauf bestaetigt:

- `dotnet build XdtDeviceBridge.sln`: erfolgreich, 0 Warnungen, 0 Fehler.
- `dotnet test XdtDeviceBridge.sln`: 1847 Tests insgesamt, 1847 erfolgreich, 0 fehlgeschlagen, 0 uebersprungen.

Der Kundeninstaller wurde anschliessend neu erzeugt: `C:\GitHub\XDT-MEDISTAR\artifacts\installer\XDTBox_Setup_1.0.exe`, 63.413.022 Bytes, Zeitstempel 2026-06-05 14:34:40. Die Publish-Validierung lief sauber und `XdtDeviceBridge.App.exe` startete erfolgreich aus `artifacts\publish\XDTBox`.

## Offene Abnahme

Vor einem dauerhaften ZIP-Release nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md` fehlen echte Shin-Nippon-Rohdaten und praktische MEDISTAR-Importtests je Messart.
