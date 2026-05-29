# Templatepaket MEDISTAR + NIDEK RT-3100 RS232

Stand: 2026-05-29

## Zweck

Dieses Kandidatenpaket bereitet den seriellen NIDEK RT-3100 als bidirektionalen Phoropter fuer MEDISTAR vor. Es nutzt die gemeinsame NIDEK-RT-Serial-Parserfamilie und bleibt bis zur Praxisaufnahme ein Test-/Vorschaustand.

## Enthaltene BuiltIns

- Geraeteprofil: `device-nidek-rt3100-serial-default`
- Exportprofil: `export-medistar-nidek-rt3100-serial-default`
- Schnittstellenprofil: `interface-medistar-nidek-rt3100-serial-default`

## Kommunikation

- Verbindung: `SerialRs232`
- Type 1: 2400 Baud, 7 Datenbits, Even Parity, 2 Stopbits
- Type 2: 9600 Baud, 8 Datenbits, Odd Parity, 1 Stopbit
- Default konservativ: Type 1
- Hinweis: PC port parameter am Geraet muss auf `PC` stehen.
- Bidirektional: ja

## MEDISTAR-Mapping

- `Final` -> `6228` Phoropter finaler Verordnungswert
- `Subjective` -> `6227` Phoropter Maximalwert (Vollkorrektion)
- keine `6330`
- keine kuenstliche Trennzeile
- `8402` kommt aus AIS

## Ausgabe an Geraet

Der XDT-Baukasten kann vorhandene LM-/AR-Historienwerte als PC->RT-Sendeframe vorbereiten. RT-3100-Frames koennen einen ID-Block enthalten. Die Vorschau ist nicht produktiv sendend.

## Teststand

- synthetische Standard- und Expanded-Header-Tests vorhanden
- Writer-Tests fuer ID-Block und LM-/AR-Bloecke vorhanden
- selektiver Templatepaket-Export/-Import testseitig abgesichert
- echte Praxis-Mitschnitte und MEDISTAR-Abnahme offen
