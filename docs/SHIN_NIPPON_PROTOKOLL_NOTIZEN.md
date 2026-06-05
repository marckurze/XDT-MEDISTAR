# Shin-Nippon Protokollnotizen

Stand: 2026-06-05

Diese Notizen dokumentieren die statisch abgeleitete Shin-Nippon-Textfamilie aus dem Referenzdatenbatch. Es wurden keine Programme aus den Referenzdaten ausgefuehrt. Die aktuellen Testdaten sind synthetische Fixtures aus der erkennbaren Parser-/COM-Logik und ersetzen keine echten Praxisrohdateien.

## Umgesetzte Geraete

| Geraet | Geraeteart | Anschluss | Default-Parameter | XDTBox-Parser | MEDISTAR-Ziel |
| --- | --- | --- | --- | --- | --- |
| Accuref R-800 | Autorefraktor | RS232 Text | 115200 8N1, Handshake None, DTR/RTS aus | `ShinNipponDeviceParser` | `6228` |
| Accuref K-900 | Autorefraktor/Keratometer | RS232 Text | 115200 8N1, Handshake None, DTR/RTS aus | `ShinNipponDeviceParser` | `6228`, `6221` |
| DL-1000 | Lensmeter | RS232 Text | 9600 8N1, Handshake None, DTR/RTS aus | `ShinNipponDeviceParser` | `6228` |
| DL-800 | Lensmeter | RS232 Text | 9600 8N1, Handshake None, DTR/RTS aus | `ShinNipponDeviceParser` | `6228` |
| DL-900 | Lensmeter | RS232 Text | 9600 8N1, Handshake None, DTR/RTS aus | `ShinNipponDeviceParser` | `6228` |
| NCT-200 | Non-Contact-Tonometer | RS232 Text | 19200 8N1, Handshake None, DTR/RTS aus | `ShinNipponDeviceParser` | `6205` |
| SLM-4000 | Lensmeter | RS232 Text | 9600 8N1, Handshake None, DTR/RTS aus | `ShinNipponDeviceParser` | `6228` |

Alle BuiltIn-Schnittstellenprofile sind inaktiv und enthalten keine Praxisordner oder COM-Port-Namen. Kundenspezifische Port- und Ordnerwerte werden erst im Schnittstellenprofil gepflegt.

## Parserregeln

- Accuref REF: R/L-SCA, PD und VD werden nur ausgegeben, wenn die Tokens erkannt wurden. Rechts/links gehen als `MedistarLine` nach `6228`.
- Accuref KM: K-Werte werden als zwei `6221`-Zeilen vorbereitet: R1/R2-Zeile und Zylinder-/Achsenzeile.
- DL-/SLM-Lensmeter: R/L-SCA, ADD, PD und erkannte Prismaanteile gehen in die `6228`-Lensmeterzeilen. Fehlende Teilwerte erzeugen keine Platzhalterausgabe.
- NCT-200: erkannte IOP-Einzelwerte und Durchschnittswerte werden zu einer `6205`-Tonometriezeile zusammengefuehrt. Ohne verwertbare Werte wird kein AIS-only-XDT erzeugt.

## Nicht umgesetzt

DR-900 wurde nicht als BuiltIn umgesetzt. Die Datenlage zeigt einen Refraktions-/Phoropterkandidaten mit bidirektionaler Relevanz, aber Rueckgabe, PC-zu-Geraet-Sendeframe und Anschlussverhalten sind fuer eine sichere XDTBox-Anbindung noch nicht belastbar genug.

## Offene Praxisvalidierung

1. Echte Accuref-R-800- und Accuref-K-900-Rohdaten sammeln und gegen die synthetischen Fixtures vergleichen.
2. Echte DL-/SLM-Lensmeter-Rohdaten mit ADD, PD und Prisma sammeln.
3. Echte NCT-200-Tonometrie-Rohdaten sammeln und die `6205`-Darstellung praktisch pruefen.
4. DR-900 erst mit echten Rueckgabe- und Sendeframe-Belegen neu bewerten.
5. Keine externen Referenzrohdaten, Skripte oder Analyseartefakte in Publish oder Installer aufnehmen.
