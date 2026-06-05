# Huvitz RS232/Text-Protokollnotizen

Stand: 2026-06-05

Diese Notiz dokumentiert die in XDTBox uebernommene Huvitz-Textfamilie aus neutral ausgewerteten Referenzdaten. Es wurden keine Programme aus den Referenzdaten ausgefuehrt. Die aktuellen Testdaten sind synthetische Fixtures aus der erkannten Parserlogik und keine echten Praxisrohdateien.

## Umgesetzte BuiltIns

| Modell | Geraeteart | Anschluss | Parser | MEDISTAR-Ausgabe |
| --- | --- | --- | --- | --- |
| Huvitz HRK-8000A | Autorefraktor/Keratometer | RS232 Text, 9600 8N1 | `HuvitzTextDeviceParser` | REF nach `6228`, KM nach `6221` |
| Huvitz HRK-9000A | Autorefraktor/Keratometer | RS232 Text, 9600 8N1 | `HuvitzTextDeviceParser` | REF nach `6228`, KM nach `6221` |
| Huvitz HNT-1P | Tonometer/Pachymeter | RS232 Text, 115200 8N1 | `HuvitzTextDeviceParser` | Tonometrie nach `6205`, Pachymetrie nach `6220` |
| Huvitz HTR-1A | Kombigeraet REF/KM/IOP/CCT | RS232 Text, 9600 8N1 | `HuvitzTextDeviceParser` | REF `6228`, KM `6221`, IOP `6205`, CCT `6220` |

## Erkannte Token

- `S-R-R` und `S-R-L`: Autorefraktion rechts/links mit Sphere, Cylinder, Axis und optional PD.
- `S-K-R` und `S-K-L`: Keratometrie rechts/links mit R1, R2 und Axis.
- `T-R01`, `T-R02`, `T-R03`, `T-R-A` sowie linke Entsprechungen: Tonometrie-Einzelwerte und Mittelwert.
- `P-R01`, `P-R02`, `P-R03`, `P-R-A` sowie linke Entsprechungen: Pachymetrie/CCT-Einzelwerte und Mittelwert.

## MEDISTAR-Regeln

- Autorefraktorwerte erzeugen nur vorhandene rechte/linke `6228`-Zeilen im Format `R.:S=... Z=...*...`.
- Keratometrie erzeugt eine `6221`-Zeile mit `R: R1=... R2=... *... // L: ...`, wenn entsprechende Werte vorhanden sind.
- Tonometrie erzeugt `6205` im Format `R = ... [...] // L = ... [...] mmHg`.
- Pachymetrie erzeugt `6220` im Format `RA: 0.xxx // LA: 0.xxx`.
- Es werden keine kuenstlichen Trennzeilen, kein automatisches `6330` und keine erfundenen Werte erzeugt.

## Bewusste Grenzen

- HLM-1, HLM-7000P und HLM-9000 bleiben Lensmeter-Kandidaten. Die Referenzlogik ist interessant, aber fuer ADD/PD/Prisma und sichere `6228`-Ausgabe fehlen noch belastbare Rohdaten.
- HDR-7000 und HDR-9000 bleiben Phoropter-/Refraktor-Kandidaten. Ohne getrennt abgesicherte Rueckgabe- und PC->Geraet-Frames wird kein BuiltIn aktiviert.
- Echte Huvitz-Praxisdateien sollten spaeter als Fixtures ergaenzt werden. Erst danach kann die praktische MEDISTAR-Abnahme erfolgen.
