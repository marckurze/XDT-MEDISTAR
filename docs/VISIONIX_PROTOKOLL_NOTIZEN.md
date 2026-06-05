# Visionix Protokollnotizen

Stand: 2026-06-06

Diese Notizen dokumentieren die statisch abgeleitete Visionix-Unterstuetzung aus Referenzpaket Batch 8. Es wurden keine Referenzprogramme ausgefuehrt und keine fremden Skripte oder Live-Pfade uebernommen.

## Umgesetzte Geraete

| Geraet | Verbindung | Parser | MEDISTAR-Ausgabe | Status |
| --- | --- | --- | --- | --- |
| Visionix Retinomax 5 | RS232 Text | `CanonZeissVisionixDeviceParser`, neutraler REF-/KM-Text | `6228` REF, `6221` KM | BuiltIn vorbereitet, synthetische neutrale Fixture |
| Visionix VX 120 | Datei/LAN XML | `CanonZeissVisionixDeviceParser`, `optic`-REF-Struktur | `6228` REF rechts/links | BuiltIn vorbereitet, synthetische neutrale Fixture |
| Visionix VX 650 | Datei/LAN XML | `CanonZeissVisionixDeviceParser`, `optic`-REF-/TONO-Struktur | `6228` REF, `6205` Tonometrie | BuiltIn vorbereitet, synthetische neutrale Fixture |

## Serielle Defaults

| Geraet | Baud | Datenbits | Paritaet | Stopbits | DTR/RTS |
| --- | --- | --- | --- | --- | --- |
| Retinomax 5 | 115200 | 8 | None | 1 | aus |

## Bewusst nicht umgesetzt

Visionix Optovue iVue 80 und Optovue iVue 100 wurden als OCT-/Bildworkflows bewusst ausgeschlossen. Es gibt keine BuiltIns, Parser oder Templatepakete fuer diese Geraete.

## Regeln

- REF wird nach `6228` ausgegeben.
- KM wird nur bei Retinomax-Textdaten nach `6221` ausgegeben.
- Tonometrie wird bei VX 650 nach `6205` ausgegeben.
- OCT-, Bild- und Spezialanalysewerte werden nicht geraten.
- Die aktuellen Fixtures sind synthetisch aus Referenzlogik abgeleitet und ersetzen keine Praxisrohdateien.

## Offene Abnahme

- Echte Retinomax-5-, VX-120- und VX-650-Praxisdaten sammeln.
- Serielle Retinomax-Einstellungen an echter Hardware pruefen.
- MEDISTAR-Import der erzeugten `6228`, `6221` und `6205`-Zeilen praktisch pruefen.
