# ZEISS Protokollnotizen

Stand: 2026-06-06

Diese Notizen dokumentieren die statisch abgeleitete ZEISS-Unterstuetzung aus Referenzpaket Batch 8. Es wurden keine Referenzprogramme ausgefuehrt und keine fremden Skripte oder Live-Pfade uebernommen.

## Umgesetzte Geraete

| Geraet | Verbindung | Parser | MEDISTAR-Ausgabe | Status |
| --- | --- | --- | --- | --- |
| ZEISS VISULENS 550 | Datei/LAN XML | `CanonZeissVisionixDeviceParser`, JOIA-nahe LM-Struktur | `6228` Lensmeter rechts/links | BuiltIn vorbereitet, synthetische neutrale Fixture |
| ZEISS VISUPLAN 500 | RS232 Text | `CanonZeissVisionixDeviceParser`, neutraler TM-Text | `6205` Tonometrie | BuiltIn vorbereitet, synthetische neutrale Fixture |
| ZEISS VISUREF 100 | RS232 Text | `CanonZeissVisionixDeviceParser`, neutraler REF-/KM-Text | `6228` REF, `6221` KM | BuiltIn vorbereitet, synthetische neutrale Fixture |
| ZEISS IOLMaster 700 | Datei/LAN XML | `ZeissIolMaster700DeviceParser`, namespace-tolerante IOLMaster-XML-Auswertung | `6227` VKT/AL, `6228` R1/R2-Keratometrie | BuiltIn vorbereitet, synthetische neutrale Fixture |

## Serielle Defaults

| Geraet | Baud | Datenbits | Paritaet | Stopbits | DTR/RTS |
| --- | --- | --- | --- | --- | --- |
| VISUPLAN 500 | 19200 | 8 | None | 1 | aus |
| VISUREF 100 | 9600 | 8 | None | 1 | aus |

## Regeln

- Lensmeterwerte werden nach `6228` ausgegeben.
- REF wird nach `6228`, KM nach `6221` ausgegeben.
- Tonometrie wird nach `6205` ausgegeben.
- IOLMaster-700-Biometrie wird nach `6227` ausgegeben: VKT/AL als eine konservative Zeile.
- IOLMaster-700-Keratometrie wird nach `6228` ausgegeben: R1/R2 mit Achsen als eine konservative Zeile.
- Fuer IOLMaster 700 werden keine `6330`-Zeilen, keine kuenstlichen Trenner und keine automatischen Anhangsfelder erzeugt.
- Die aktuellen Fixtures sind synthetisch aus Referenzlogik abgeleitet und ersetzen keine Praxisrohdateien.

## Offene Abnahme

- Echte VISULENS-/VISUPLAN-/VISUREF-Praxisdaten sammeln.
- Echte IOLMaster-700-Praxis-XMLs sammeln und gegen `6227`/`6228` praktisch in MEDISTAR abnehmen.
- Serielle Einstellungen an echten Geraeten pruefen.
- MEDISTAR-Import der erzeugten Karteikartenzeilen praktisch pruefen.
