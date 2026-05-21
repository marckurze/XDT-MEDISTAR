# E2E-Testprotokoll MEDISTAR + TOPCON CL-300

Stand: 2026-05-21

## Status

Praktisch validiert fuer die Lensmeter-XDT-Rueckgabe aus echten TOPCON CL-300 XML-Dateien.

## Validierter Ablauf

- MEDISTAR liefert eine AIS-GDT/XDT-Datei mit passender Untersuchungsart.
- TOPCON CL-300 liefert eine XML-Geraetedatei.
- Die App erkennt TOPCON CL-300 XML ueber `Ophthalmology`, `Company = TOPCON`, `ModelName = CL-300` und `Measure type="LM"`.
- Die App liest Lensmeter-Werte fuer rechts und links.
- Die App erzeugt eine MEDISTAR-kompatible XDT-Rueckgabedatei.
- MEDISTAR uebernimmt `8402` aus der AIS-Datei.
- MEDISTAR uebernimmt die `6228`-Ergebniszeilen fuer rechts und links.

## Validierte Ergebnisfelder

- `8000 = 6310`
- `3000` Patientennummer
- `3101` Nachname
- `3102` Vorname
- `3103` Geburtsdatum
- `8402` Untersuchungsart aus AIS
- `6228` rechte Lensmeter-Zeile
- `6228` linke Lensmeter-Zeile

Die folgenden XDT-Auszuege sind anonymisiert. Patientennummer, Name und Geburtsdatum sind Platzhalter; die CL-300-Ergebniszeilen entsprechen den praktisch validierten Laeufen.

## Validiertes Ergebnis 1: Serial0001

```text
01380006310
0153000<Testpatientennummer>
0173101<Testnachname>
0133102<Testvorname>
0173103<Testgeburtsdatum>
0128402CL300
0336228R.:S=- 0.25 Z=- 1.00* 91
0336228L.:S=+ 0.00 Z=- 1.00* 87
```

Fachlicher MEDISTAR-Karteikartenauszug:

```text
R.:S=- 0.25 Z=- 1.00* 91
L.:S=+ 0.00 Z=- 1.00* 87
```

Validiert:

- AIS-Datei wurde eingelesen.
- TOPCON CL-300 XML wurde eingelesen.
- XDT-Rueckgabedatei wurde erzeugt.
- `8402 = CL300` kommt aus AIS/MEDISTAR.
- Rechte und linke `6228`-Zeile wurden korrekt erzeugt.
- Keine unnoetigen ADD-, PD- oder Prisma-Fragmente.
- Keine leeren optionalen Fragmente.

## Validiertes Ergebnis 2: Serial1521

```text
01380006310
0153000<Testpatientennummer>
0173101<Testnachname>
0133102<Testvorname>
0173103<Testgeburtsdatum>
0148402CL300
0676228R.:S=+ 3.75 Z=- 3.50*  9 A=+ 2.25 P=H=+0.25 V=-0.75 PD= 55
0526228L.:S=+ 3.50 Z=- 3.00*178 A=+ 2.25 P=V=-1.00
```

Fachlicher MEDISTAR-Karteikartenauszug:

```text
R.:S=+ 3.75 Z=- 3.50*  9 A=+ 2.25 P=H=+0.25 V=-0.75 PD= 55
L.:S=+ 3.50 Z=- 3.00*178 A=+ 2.25 P=V=-1.00
```

Validiert:

- AIS-Datei wurde eingelesen.
- TOPCON CL-300 XML wurde eingelesen.
- XDT-Rueckgabedatei wurde erzeugt.
- `8402 = CL300` kommt aus AIS/MEDISTAR.
- Rechte `6228`-Zeile enthaelt Sphere, Cylinder, Axis, Add, Prisma und PD.
- Linke `6228`-Zeile enthaelt Sphere, Cylinder, Axis, Add und Prisma.
- ADD wird ausgegeben, wenn vorhanden.
- PD wird ausgegeben, wenn vorhanden.
- Prisma wird konservativ als signierte H/V-Komponente ausgegeben.
- OUT/IN/UP/DOWN wird nicht geraten.
- Null-Prismen werden weggelassen.
- Keine leeren optionalen Fragmente.

## XML-Struktur

- Root: `Ophthalmology`
- Common-Namespace: `nsCommon`
- Lensmeter-Namespace: `nsLM`
- `Common/Company = TOPCON`
- `Common/ModelName = CL-300`
- `Measure type="LM"`
- Encoding: UTF-8
- Echte Testfixtures:
  - `M-Serial0001_20120101_000000_TOPCON_CL-300_00.xml`
  - `M-Serial1521_20120101_000000_TOPCON_CL-300_00.xml`

## Ausgabehinweise

- Ausgabe erfolgt ueber `6228`.
- `8402` kommt aus AIS/MEDISTAR.
- ADD wird nur ausgegeben, wenn vorhanden.
- PD wird nur ausgegeben, wenn vorhanden.
- Prisma wird konservativ als signierte H/V-Komponente ausgegeben.
- OUT/IN/UP/DOWN wird nicht geraten.
- Null-Prismen werden weggelassen.
- Es entstehen keine leeren optionalen Fragmente.
- Keine `6205`, keine `6220`, keine Dokumentanhang-`6302`-`6305` fuer diesen Lensmeter-Lauf.

## Grenzen und offene Punkte

- Die praktische Bewertung der Prisma-Darstellung `P=H=... V=...` in weiteren MEDISTAR-Anzeigen weiter beobachten.
- Weitere TOPCON-Geraete nur nach echten Dateien und gesonderter Validierung ergaenzen.
- Offizielles ZIP-Artefakt erst nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md`.
- Keine Live-Pfade, Kundendaten oder echten Patientendaten dokumentiert.
