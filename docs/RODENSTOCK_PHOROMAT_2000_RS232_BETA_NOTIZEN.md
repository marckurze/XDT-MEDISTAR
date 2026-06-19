# Rodenstock Phoromat 2000 RS232 Beta-Notizen

Stand: 2026-06-18

## Status

Rodenstock Phoromat 2000 ist als inaktives Beta-BuiltIn vorbereitet. Die Umsetzung ist synthetisch getestet, aber nicht produktiv live validiert.

Es wurde kein Setup gebaut und keine App-Version erhoeht.

## Grundregeln

- XDTBox berechnet keine medizinischen Werte.
- XDTBox formatiert nur Werte, die im empfangenen Geraeterahmen oder in der AIS-Historie vorhanden sind.
- VA wird aktuell nur als Baukasten-Platzhalter angeboten und nicht automatisch in die produktive MEDISTAR-Ausgabe geschrieben.
- Das BuiltIn-Schnittstellenprofil bleibt standardmaessig inaktiv.

## RS232-Default

- Baudrate: `9600`
- Datenbits: `8`
- Paritaet: `None`
- Stoppbits: `1`
- Handshake: `None`
- DTR: `false`
- RTS: `false`
- ReadTimeout: `30000 ms`
- WriteTimeout: `1000 ms`

## Steuerzeichen und Rahmen

- `SOH = 0x01`
- `STX = 0x02`
- `EOT = 0x04`
- `ETB = 0x17`
- `LF = 0x0A`
- `CR = 0x0D`

Empfang:

- Start: `<SOH>*PC_RCV_S<EOT>`
- Ende: `<SOH>*PC_RCV_E<EOT><CR><LF>`
- LF-only am Ende wird tolerant akzeptiert.

Senden:

- Start: `<SOH>*PC_SND_S<EOT><LF>`
- Ende: `<SOH>*PC_SND_E<EOT><LF>`

## Rueckgabe-Mapping nach MEDISTAR

| Geraetewert | MEDISTAR-Ziel | Hinweis |
| --- | --- | --- |
| `FN/SP` | `6228` | Sphaere in der Phoropterzeile |
| `FN/CY` | `6228` | Zylinder in der Phoropterzeile |
| `FN/AX` | `6228` | Achse in der Phoropterzeile |
| `FN/AD` | `6228` | Addition `A=` |
| `FN/PH`, `FN/PV` | `6228` | Prisma, wenn geliefert |
| `PD` | `6228` | PD in der Phoropterzeile, wenn geliefert |
| `WD` | `6227` | Working Distance |
| `VA` | kein automatischer Produktivexport | nur Baukasten-Platzhalter |

Pflichtwerte fuer eine `6228`-Phoropterzeile sind `SP`, `CY` und `AX` pro Auge. Fehlen sie, erzeugt der Parser eine Warnung statt eine unvollstaendige Zeile zu schreiben.

## Ausgabe an Geraet

Der Writer erzeugt den seriellen Sendeframe aus vorhandenen AIS-Historienwerten. Unterstuetzt sind:

- `V0` Lensmeter nach `LM`
- `V1` Autoref nach `AR`
- `V2` Phoropter nach `FN`
- `V6` Working Distance nach `WD`
- optional `PD`
- `TIME` immer als Abschlussinformation

Nicht produktiv umgesetzt sind weitere nicht abgesicherte Historienwerte, VA-Sendung und Werte ohne klare Datenquelle.

## Offene Live-Abnahme

1. Echte RS232-Rueckgabeframes aufnehmen.
2. PC->Geraet-Sendeframe am echten Geraet pruefen.
3. MEDISTAR-Import der erzeugten `6228`-/`6227`-Ausgabe praktisch abnehmen.
4. Danach entscheiden, ob VA produktiv zusaetzlich ausgegeben werden soll.
