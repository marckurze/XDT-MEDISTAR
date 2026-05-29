# NIDEK RT-2100 / RT-3100 / RT-5100 RS232 Protokollnotizen

Stand: 2026-05-29

Diese Notizen fassen die Auswertung der bereitgestellten Herstellerdokumente zusammen:

- `RT2100_Interface_MRT8  RTZ001FE_.pdf`
- `RT-3100(RT-11)_IntE_34090-P982-A2.pdf`
- `RT-5100_IntME_34085-P992-A0.pdf`

Die Auswertung dient der Vorbereitung von Parser, Writer, BuiltIn-Profilen und XDT-Baukasten-Vorschau. Eine produktive Live-Sendung an echte Geraete ist damit noch nicht freigegeben.

## Gemeinsame Familie

- Geraeteklasse: NIDEK-RT-Phoropter
- Verbindung: RS-232C, DIN 8-pin, ASCII, halbduplex
- Steuerzeichen: `SH`, `SX`, `EB`, `ET`, `CR`
- Aktuelle Zuordnung:
  - `SH` = SOH = `0x01`
  - `SX` = STX = `0x02`
  - `EB` = ETB = `0x17`
  - `ET` = EOT = `0x04`
  - `CR` = `0x0D`
- Datenquellen: `LM`, `RM`, `RT`, `KM`, `NT`; bei RT-3100/RT-5100 zusaetzlich `WF`.
- Gross-/Kleinschreibung wird als Tag-/Nacht- beziehungsweise Variantenhinweis behandelt, aber nicht blind medizinisch umgedeutet.

## Kommunikationsparameter

| Geraet | Preset | Baud | Datenbits | Paritaet | Stopbits | Hinweis |
| --- | --- | ---: | ---: | --- | ---: | --- |
| RT-2100 | Type 1 | 2400 | 7 | Even | 2 | konservativer Standard, DTR/DSR relevant |
| RT-3100 | Type 1 | 2400 | 7 | Even | 2 | gilt, wenn kein Type2-Parameter gesetzt ist |
| RT-3100 | Type 2 | 9600 | 8 | Odd | 1 | PC port parameter muss auf `PC` stehen |
| RT-5100 | Type 1 | 2400 | 7 | Even | 2 | konservativer Standard |
| RT-5100 | Type 2 | 9600 | 8 | Odd | 1 | direkter Output ohne erste DTR/DSR-Schritte moeglich |

## RT -> PC

Der Parser `NidekRtSerialPhoropterParser` ist tolerant gegen Standard- und Expanded-Header:

- RT-2100: `SH NIDEK RT-2100 ... DAYYYY/MM/DD CR`
- RT-3100: Standard-/Kompatibilitaetszeilen und Expanded-Header `SH NIDEK_RT-3100 CR`, danach `SX ID... CR` und `SX DAYYYY/MM/DD_SN CR`
- RT-5100: Standard `NIDEK_RT-5100 ...` und Expanded-Header analog RT-3100

Refraktionsdaten werden zuerst fuer SCA/ADD/PD sicher ausgewertet. Sonstige Paketdaten, VA-/Plus-Package-/KM-/NT-Details bleiben je nach Erkennung roh oder diagnostisch, bis echte Praxisdaten vorliegen.

MEDISTAR-Mapping:

- `Final` / finale Prescription -> `6228` mit Header `Phoropter finaler Verordnungswert`
- `Subjective` -> `6227` mit Header `Phoropter Maximalwert (Vollkorrektion)`
- keine `6330`
- keine kuenstliche Trennzeile
- `8402` kommt aus AIS/MEDISTAR

## PC -> RT

Der Writer `NidekRtSerialPhoropterOutputWriter` erzeugt Vorschauframes fuer die Uebergabe von vorhandenen AIS-Historienwerten an das Geraet.

Unterstuetzt in V1:

- AR SCA
- AR PD
- LM SCA
- LM ADD
- LM PD

RT-2100 nutzt keinen ID-Block. RT-3100 und RT-5100 koennen einen ID-Block enthalten. Fehlende Werte werden weggelassen; es werden keine leeren medizinischen Bloecke erfunden. Prisma wird erst nach echter Datenlage aktiviert.

## XDT-Baukasten

Im XDT-Baukasten zeigt die Ansicht `Geraeteausgabe` den sichtbaren Steuerzeichen-Text und einen Hexdump. Die Vorschau schreibt keine produktive Datei und sendet nichts an einen COM-Port.

Warnhinweis im Baukasten:

`NIDEK RT-2100/3100/5100 RS232 ist vorbereitet. Bitte echte Praxis-Mitschnitte pruefen, bevor produktiv gesendet wird.`

## Offene Praxispunkte

- echte RT-2100-/RT-3100-/RT-5100-RS232-Mitschnitte
- Pruefung, welche Header-Variante das konkrete Praxisgeraet sendet
- Pruefung von Type1/Type2 und DTR/DSR-Verhalten
- Live-Abnahme des PC->RT-Sendeframes
- MEDISTAR-Abnahme der `6228`-/`6227`-Rueckgabe
