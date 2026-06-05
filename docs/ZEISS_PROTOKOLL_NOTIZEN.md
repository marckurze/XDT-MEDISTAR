# ZEISS Protokollnotizen

Stand: 2026-06-06

Diese Notizen dokumentieren die statisch abgeleitete ZEISS-Unterstuetzung aus Referenzpaket Batch 8. Es wurden keine Referenzprogramme ausgefuehrt und keine fremden Skripte oder Live-Pfade uebernommen.

## Umgesetzte Geraete

| Geraet | Verbindung | Parser | MEDISTAR-Ausgabe | Status |
| --- | --- | --- | --- | --- |
| ZEISS VISULENS 550 | Datei/LAN XML | `CanonZeissVisionixDeviceParser`, JOIA-nahe LM-Struktur | `6228` Lensmeter rechts/links | BuiltIn vorbereitet, synthetische neutrale Fixture |
| ZEISS VISUPLAN 500 | RS232 Text | `CanonZeissVisionixDeviceParser`, neutraler TM-Text | `6205` Tonometrie | BuiltIn vorbereitet, synthetische neutrale Fixture |
| ZEISS VISUREF 100 | RS232 Text | `CanonZeissVisionixDeviceParser`, neutraler REF-/KM-Text | `6228` REF, `6221` KM | BuiltIn vorbereitet, synthetische neutrale Fixture |

## Serielle Defaults

| Geraet | Baud | Datenbits | Paritaet | Stopbits | DTR/RTS |
| --- | --- | --- | --- | --- | --- |
| VISUPLAN 500 | 19200 | 8 | None | 1 | aus |
| VISUREF 100 | 9600 | 8 | None | 1 | aus |

## Bewusst nicht umgesetzt

ZEISS IOLMaster 700 bleibt Kandidat. Biometrie- und IOL-Werte sind sichtbar, aber ohne klare MEDISTAR-Ziel-Feldkennung wird kein halbfertiges BuiltIn ausgeliefert.

## Regeln

- Lensmeterwerte werden nach `6228` ausgegeben.
- REF wird nach `6228`, KM nach `6221` ausgegeben.
- Tonometrie wird nach `6205` ausgegeben.
- Biometrie-/IOL-Werte werden nicht geraten.
- Die aktuellen Fixtures sind synthetisch aus Referenzlogik abgeleitet und ersetzen keine Praxisrohdateien.

## Offene Abnahme

- Echte VISULENS-/VISUPLAN-/VISUREF-Praxisdaten sammeln.
- Serielle Einstellungen an echten Geraeten pruefen.
- MEDISTAR-Import der erzeugten Karteikartenzeilen praktisch pruefen.
- IOLMaster 700 erst nach fachlicher Ziel-Feldentscheidung neu bewerten.
