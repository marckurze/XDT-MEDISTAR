# E2E-Testprotokoll: MEDISTAR + NIDEK AR360 / AR-360A

Stand: 2026-05-17

## Status

Praktisch validiert fuer Auto-Refraktor-XDT-Rueckgabe aus einer NIDEK-AR360-XML-Beispieldatei in MEDISTAR.

## Validierter Ablauf

1. MEDISTAR liefert eine AIS-GDT/XDT-Datei mit Untersuchungsart `AR360`.
2. NIDEK AR360 / AR-360A liefert eine XML-Geraetedatei.
3. XdtDeviceBridge liest die Auto-Refraktor-Werte aus `ARMedian`.
4. XdtDeviceBridge liest `VD` und `PD/PDList/FarPD`.
5. XdtDeviceBridge erzeugt eine MEDISTAR-kompatible XDT-Rueckgabedatei.
6. MEDISTAR uebernimmt `8402 = AR360`.
7. MEDISTAR uebernimmt die `6228`-Ergebniszeilen fuer rechts und links.

Eine zuerst sichtbare Untersuchungsart `ARK1S` war kein App-Fehler. Sie stammte aus einer alten aus MEDISTAR exportierten Untersuchungsart. Mit korrekter MEDISTAR-Untersuchungsart wurde `AR360` korrekt uebernommen.

## Validierte Ergebnisfelder

- `8000 = 6310`
- `3000` Patientennummer
- `3101` Nachname
- `3102` Vorname
- `3103` Geburtsdatum
- `8402 = AR360`
- `6228` rechte Auto-Refraktor-Zeile
- `6228` linke Auto-Refraktor-Zeile

## Validiertes Ergebnis

Die folgende Darstellung ist anonymisiert. Es sind keine echten Patienten-, Kunden- oder Live-Pfaddaten enthalten.
Die Patientenzeilen zeigen Platzhalter; bei konkreten Testwerten erzeugt `XdtExportBuilder` das passende XDT-Laengenpraefix.

```text
01380006310
0153000<Patientennummer>
0173101<Testnachname>
0133102<Testvorname>
0173103<Geburtsdatum>
0148402AR360
0536228R.:S=+ 2.00 Z=- 1.25*172 PD= 60 VD= 12.00 mm
0336228L.:S=+ 1.00 Z=- 0.75*170
```

## Achsenhinweis

- `R/AR/ARMedian/Axis` liefert rechts `172`.
- Dieser ARMedian-Wert wurde uebernommen und praktisch bestaetigt.
- Es erfolgt keine kuenstliche Anpassung auf `177`.
- Der fruehere TXT-Wert `177` war ein Formatbeispiel bzw. stammt aus `ARList`, nicht aus `ARMedian`.

## Grenzen

- XDT-Anhang-Linkfelder `6302`, `6303`, optional `6304` und `6305` wurden in diesem AR360-Test nicht neu validiert.
- Der Templatepaket-Kandidat `MEDISTAR + NIDEK AR360` ist als zweites Referenzpaket vorbereitet und reproduzierbar export-/importgeprueft; eine offizielle ZIP-Release-Ablage bleibt bis zur Freigabe nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md` offen.
- Die Validierung deckt die Auto-Refraktor-Rueckgabe rechts/links, `8402`, `FarPD` und `VD` ab.
