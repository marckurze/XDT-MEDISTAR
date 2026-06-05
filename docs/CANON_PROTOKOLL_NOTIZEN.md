# Canon Protokollnotizen

Stand: 2026-06-06

Diese Notizen dokumentieren die statisch abgeleitete Canon-Unterstuetzung aus Referenzpaket Batch 8. Es wurden keine Referenzprogramme ausgefuehrt und keine fremden Skripte oder Live-Pfade uebernommen.

## Umgesetzte Geraete

| Geraet | Verbindung | Parser | MEDISTAR-Ausgabe | Status |
| --- | --- | --- | --- | --- |
| Canon RK-F2 | Datei/LAN XML | `CanonZeissVisionixDeviceParser`, JOIA-nahe REF-Struktur | `6228` REF rechts/links | BuiltIn vorbereitet, synthetische neutrale Fixture |
| Canon TX-20P | Datei/LAN XML | `CanonZeissVisionixDeviceParser`, JOIA-nahe TM-/PM-Struktur | `6205` Tonometrie, `6220` Pachymetrie/CCT | BuiltIn vorbereitet, synthetische neutrale Fixture |

## Bewusst nicht umgesetzt

Canon OCT A-1 Xephilo wurde als OCT-/Bildworkflow bewusst ausgeschlossen. Es gibt kein BuiltIn, keinen Parser und kein Templatepaket fuer dieses Geraet.

## Regeln

- REF wird nur aus klar erkannten rechten/linken Werten erzeugt und nach `6228` ausgegeben.
- Tonometrie wird nach `6205` ausgegeben; Durchschnittswerte werden nicht als Einzelmessungen dupliziert.
- Pachymetrie/CCT wird nach `6220` ausgegeben.
- Es werden keine KM-, OCT- oder Bildwerte geraten.
- Die aktuellen Fixtures sind synthetisch aus Referenzlogik abgeleitet und ersetzen keine Praxisrohdateien.

## Offene Abnahme

- Echte RK-F2- und TX-20P-Praxisdateien sammeln.
- MEDISTAR-Import der erzeugten `6228`, `6205` und `6220`-Zeilen praktisch pruefen.
- OCT-/Bildworkflows getrennt fachlich entscheiden, falls sie spaeter beauftragt werden.
