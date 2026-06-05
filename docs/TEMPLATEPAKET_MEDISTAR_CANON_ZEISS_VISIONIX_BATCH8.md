# Templatepaket MEDISTAR + Canon / ZEISS / Visionix Batch 8

Stand: 2026-06-06

Status: Sammelkandidat, testseitig abgesichert, noch keine praktische MEDISTAR-Abnahme

## Zweck

Dieses Dokument beschreibt die vorbereiteten Templatepaket-Kandidaten aus Referenzpaket Batch 8. Die Profile bleiben inaktiv, enthalten keine Praxisordner, keine Patientendaten und keine Referenzrohdateien. Der Export erfolgt selektiv aus dem Schnittstellenprofil und nimmt nur die benoetigten AIS-, Geraete- und Exportprofile auf.

## Enthaltene Kandidaten

| Paket | Geraeteprofil | Exportprofil | Schnittstellenprofil | Ausgabe |
| --- | --- | --- | --- | --- |
| MEDISTAR + Canon RK-F2 | `device-canon-rkf2-default` | `export-medistar-canon-rkf2-default` | `interface-medistar-canon-rkf2-default` | `6228` REF |
| MEDISTAR + Canon TX-20P | `device-canon-tx20p-default` | `export-medistar-canon-tx20p-default` | `interface-medistar-canon-tx20p-default` | `6205` Tono, `6220` Pachy |
| MEDISTAR + ZEISS VISULENS 550 | `device-zeiss-visulens550-default` | `export-medistar-zeiss-visulens550-default` | `interface-medistar-zeiss-visulens550-default` | `6228` Lensmeter |
| MEDISTAR + ZEISS VISUPLAN 500 | `device-zeiss-visuplan500-default` | `export-medistar-zeiss-visuplan500-default` | `interface-medistar-zeiss-visuplan500-default` | `6205` Tono |
| MEDISTAR + ZEISS VISUREF 100 | `device-zeiss-visuref100-default` | `export-medistar-zeiss-visuref100-default` | `interface-medistar-zeiss-visuref100-default` | `6228` REF, `6221` KM |
| MEDISTAR + Visionix Retinomax 5 | `device-visionix-retinomax5-default` | `export-medistar-visionix-retinomax5-default` | `interface-medistar-visionix-retinomax5-default` | `6228` REF, `6221` KM |
| MEDISTAR + Visionix VX 120 | `device-visionix-vx120-default` | `export-medistar-visionix-vx120-default` | `interface-medistar-visionix-vx120-default` | `6228` REF |
| MEDISTAR + Visionix VX 650 | `device-visionix-vx650-default` | `export-medistar-visionix-vx650-default` | `interface-medistar-visionix-vx650-default` | `6228` REF, `6205` Tono |

## Nicht enthalten

- Canon OCT A-1 Xephilo: bewusst ausgeschlossen.
- Visionix Optovue iVue 80 und iVue 100: bewusst ausgeschlossen.
- ZEISS IOLMaster 700: Kandidat ohne klare MEDISTAR-Ziel-Feldkennung.

## Testabdeckung

- BuiltIn-Validierung fuer Geraete-, Export- und Schnittstellenprofile.
- Parser-/Exporttests mit synthetischen neutralen Fixtures.
- Baukasten-Preview fuer nicht-XML-Textdaten.
- `AIS Ausgabe Info` fuer gemischte Messfelder.
- Selektiver Templatepaket-Test ueber den vorhandenen Export-/Importpfad.

## Offene praktische Abnahme

- Echte Praxisrohdateien fuer alle enthaltenen Kandidaten sammeln.
- MEDISTAR-Import der erzeugten Karteikartenzeilen praktisch pruefen.
- Serielle Profile mit echten COM-Parametern validieren.
- Erst nach praktischer Abnahme ein dauerhaftes ZIP-Artefakt nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md` festlegen.
