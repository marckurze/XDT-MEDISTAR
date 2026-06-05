# TOMEY Referenzdaten-Protokollnotizen

Stand: 2026-06-05

Diese Notiz dokumentiert die in XDTBox uebernommene TOMEY-Familie aus neutral ausgewerteten Referenzdaten. Es wurden keine Programme aus den Referenzdaten ausgefuehrt. Die aktuellen Testdaten sind synthetische Fixtures aus der erkannten Parserlogik und keine echten Praxisrohdateien.

## Implementierte Kandidaten

| Geraet | Typ | Anschluss | Parser | MEDISTAR-Ausgabe |
| --- | --- | --- | --- | --- |
| TOMEY CF-2000 | Lensmeter | RS232 Text, Default `9600 8O1` | `TomeyDeviceParser` | Lensmeter nach `6228` |
| TOMEY TL-2000C | Lensmeter | Datei/CSV | `TomeyDeviceParser` | Lensmeter nach `6228` |
| TOMEY TL-6000 | Lensmeter | Datei/CSV | `TomeyDeviceParser` | Lensmeter nach `6228` |
| TOMEY TL-7000 | Lensmeter | Datei/CSV | `TomeyDeviceParser` | Lensmeter nach `6228` |
| TOMEY MR-6000 | REF/KM/Tonometrie/Pachymetrie | Datei/XML | `TomeyDeviceParser` | REF `6228`, KM `6221`, IOP `6205`, CCT `6220` |
| TOMEY TOP-1000 | Tonometer/Pachymeter | Datei/XML | `TomeyDeviceParser` | IOP `6205`, CCT `6220` |

## Parserregeln

- CF-2000: `LR`/`LL` werden als rechte/linke Lensmeter-SCA-Zeilen gelesen. `AR`/`AL` werden als ADD rechts/links gelesen. `PR`/`PL` werden nur bei verwertbaren Prismenwerten ergaenzt.
- TL-2000C/TL-6000/TL-7000: CSV-Sektionen wie `POWER_R`, `POWER_L`, `ADD_R`, `ADD_L`, `PD` und Prismafelder werden in Lensmeter-SourcePaths ueberfuehrt.
- MR-6000: REF-, KM-, TM- und PM-Bloecke werden getrennt ausgewertet. Teilmessungen duerfen fehlen, ohne dass kuenstliche Zeilen entstehen.
- TOP-1000: IOP/CorrectedIOP/CCT werden nur bei belegten Daten exportiert.
- Fehlende Werte werden nicht geraten. Es werden keine kuenstlichen Trenner und kein automatisches `6330` erzeugt.

## Nicht implementiert

| Geraet | Grund |
| --- | --- |
| TOMEY AP-2500 | keine ausreichend belastbare Messwertrohstruktur fuer einen XDTBox-Parser |
| TOMEY EM-3000 / EM-4000 | Endothel-/Zellmessung ohne freigegebenes MEDISTAR-Zielfeld in den aktuellen Messart-Standards |
| TOMEY TAP-2000 | bidirektionaler Phoropter-/Sendeframe-Workflow ohne echte Rueckgabe- und PC->Geraet-Validierung |

## Offene Praxisvalidierung

- Echte CF-2000-RS232-Rohdaten sammeln und gegen das synthetische Textfixture pruefen.
- Echte TL-2000C/TL-6000/TL-7000-Dateien sammeln und Lensmeterzeilen inklusive ADD, PD und Prisma praktisch abnehmen.
- Echte MR-6000-Dateien mit REF/KM/TM/PM-Teilmessungen sammeln.
- Echte TOP-1000-Dateien mit IOP/CCT sammeln.
- MEDISTAR-Import fuer `6228`, `6221`, `6205` und `6220` praktisch bestaetigen.
