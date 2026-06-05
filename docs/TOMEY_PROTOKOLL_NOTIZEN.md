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
| TOMEY EM-3000 | Endothel-/Zellmessung | Datei/CSV | `TomeyEmDeviceParser` | Messwerte `6228`, Kommentare `6227`, Bild-/Dateiverweise `6302` |
| TOMEY EM-4000 | Endothel-/Zellmessung | Datei/CSV | `TomeyEmDeviceParser` | Messwerte `6228`, Kommentare `6227`, Bild-/Dateiverweise `6302` |

## Parserregeln

- CF-2000: `LR`/`LL` werden als rechte/linke Lensmeter-SCA-Zeilen gelesen. `AR`/`AL` werden als ADD rechts/links gelesen. `PR`/`PL` werden nur bei verwertbaren Prismenwerten ergaenzt.
- TL-2000C/TL-6000/TL-7000: CSV-Sektionen wie `POWER_R`, `POWER_L`, `ADD_R`, `ADD_L`, `PD` und Prismafelder werden in Lensmeter-SourcePaths ueberfuehrt.
- MR-6000: REF-, KM-, TM- und PM-Bloecke werden getrennt ausgewertet. Teilmessungen duerfen fehlen, ohne dass kuenstliche Zeilen entstehen.
- TOP-1000: IOP/CorrectedIOP/CCT werden nur bei belegten Daten exportiert.
- EM-3000: CSV-Token wie `[RL]`, `[NUMBER]`, `[DENSITY]`, `[THK]`, `[FILES_N]` und `[FILE]` werden als rechte/linke Endothelmessung ausgewertet. Vorbereitete `6228`-Zeilen nutzen `R: Anzahl = ...; Dichte = ... mm2; Hornhautdicke = ... um`.
- EM-4000: CSV-Token wie `[RL_1]`, `[DENSITY_1]`, `[THK_1]`, `[RL_2]`, `[DENSITY_2]`, `[THK_2]` und `[FILE]` werden als CD-/CCT-Zellmessung ausgewertet. Vorbereitete `6228`-Zeilen nutzen `R CD = ...`, `R CCT = ...`, `L CD = ...`, `L CCT = ...`.
- EM-Kommentare werden nur bei belegten Kommentar-/Bemerkungstokens nach `6227` uebernommen. Bild-/Dateiverweise aus `[FILE]` werden als externe Referenzen nach `6302` vorbereitet; es werden keine Bilddateien in den Installer aufgenommen.
- Fehlende Werte werden nicht geraten. Es werden keine kuenstlichen Trenner und kein automatisches `6330` erzeugt.

## Nicht implementiert

| Geraet | Grund |
| --- | --- |
| TOMEY AP-2500 | keine ausreichend belastbare Messwertrohstruktur fuer einen XDTBox-Parser |
| TOMEY TAP-2000 | bidirektionaler Phoropter-/Sendeframe-Workflow: COM-Parameter und Framebausteine sind sichtbar, aber echte TAP-Rueckgabeframes und eine Live-PC->Geraet-Bestaetigung fehlen |

## Offene Praxisvalidierung

- Echte CF-2000-RS232-Rohdaten sammeln und gegen das synthetische Textfixture pruefen.
- Echte TL-2000C/TL-6000/TL-7000-Dateien sammeln und Lensmeterzeilen inklusive ADD, PD und Prisma praktisch abnehmen.
- Echte MR-6000-Dateien mit REF/KM/TM/PM-Teilmessungen sammeln.
- Echte TOP-1000-Dateien mit IOP/CCT sammeln.
- Echte EM-3000-/EM-4000-CSV-Dateien mit Bildverweisen sammeln und `6228`/`6227`/`6302` praktisch abnehmen.
- Fuer TAP-2000 echte serielle Rueckgaben und einen bestaetigten PC->Geraet-Sendeframe erfassen, bevor ein produktives BuiltIn entsteht.
- MEDISTAR-Import fuer `6228`, `6227`, `6221`, `6220`, `6205` und `6302` praktisch bestaetigen.
