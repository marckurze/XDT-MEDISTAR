# Templatepaket MEDISTAR + NIDEK RT-5100 RS232

Stand: 2026-05-29

## Zweck

Dieses Kandidatenpaket bereitet den seriellen NIDEK RT-5100 als bidirektionalen Phoropter fuer MEDISTAR vor. RT-5100 nutzt dieselbe serielle RT-Familie wie RT-3100, kann aber erweiterte Datenquellen wie `WF` liefern.

## Enthaltene BuiltIns

- Geraeteprofil: `device-nidek-rt5100-serial-default`
- Exportprofil: `export-medistar-nidek-rt5100-serial-default`
- Schnittstellenprofil: `interface-medistar-nidek-rt5100-serial-default`

## Kommunikation

- Verbindung: `SerialRs232`
- Type 1: 2400 Baud, 7 Datenbits, Even Parity, 2 Stopbits
- Type 2: 9600 Baud, 8 Datenbits, Odd Parity, 1 Stopbit
- Default konservativ: Type 1
- Bidirektional: ja

## MEDISTAR-Mapping

- `Final` -> `6228` Phoropter finaler Verordnungswert
- `Subjective` -> `6227` Phoropter Maximalwert (Vollkorrektion)
- keine `6330`
- keine kuenstliche Trennzeile
- `8402` kommt aus AIS

## Ausgabe an Geraet

Der XDT-Baukasten zeigt PC->RT-Sendedaten fuer vorhandene LM-/AR-Historienwerte als Vorschau. Erweiterte Felder und `WF` werden diagnostisch vorbereitet, aber nicht ohne echte Daten nach MEDISTAR exportiert.

## Teststand

- synthetische RT-5100-Header- und Parser-Tests vorhanden
- Writer-Tests fuer ID-Block und LM-/AR-Bloecke vorhanden
- selektiver Templatepaket-Export/-Import testseitig abgesichert
- echte Praxis-Mitschnitte und Live-Abnahme offen
