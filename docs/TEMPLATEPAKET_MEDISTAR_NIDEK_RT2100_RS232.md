# Templatepaket MEDISTAR + NIDEK RT-2100 RS232

Stand: 2026-05-29

## Zweck

Dieses Kandidatenpaket bereitet den seriellen NIDEK RT-2100 als bidirektionalen Phoropter fuer MEDISTAR vor. Es ist ein Vorbereitungsstand fuer XDT-Baukasten, Parser-Tests und spaetere Praxis-Mitschnitte.

## Enthaltene BuiltIns

- Geraeteprofil: `device-nidek-rt2100-serial-default`
- Exportprofil: `export-medistar-nidek-rt2100-serial-default`
- Schnittstellenprofil: `interface-medistar-nidek-rt2100-serial-default`

## Kommunikation

- Verbindung: `SerialRs232`
- Preset: NIDEK RT-2100 Type 1
- Parameter: 2400 Baud, 7 Datenbits, Even Parity, 2 Stopbits
- Bidirektional: ja

## MEDISTAR-Mapping

- `Final` -> `6228` Phoropter finaler Verordnungswert
- `Subjective` -> `6227` Phoropter Maximalwert (Vollkorrektion)
- keine `6330`
- keine kuenstliche Trennzeile
- `8402` kommt aus AIS

## Ausgabe an Geraet

Der XDT-Baukasten kann PC->RT-Sendedaten fuer vorhandene LM-/AR-Historienwerte als sichtbare Steuerzeichen-Vorschau und Hexdump anzeigen. Es wird nichts produktiv an den COM-Port gesendet.

## Teststand

- synthetische PDF-nahe Parser-/Writer-Tests vorhanden
- selektiver Templatepaket-Export/-Import testseitig abgesichert
- echte Praxis-Mitschnitte und Live-Abnahme offen
