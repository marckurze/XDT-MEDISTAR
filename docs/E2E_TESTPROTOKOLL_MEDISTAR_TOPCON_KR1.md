# E2E-Testprotokoll MEDISTAR + TOPCON KR-1

Stand: 2026-05-24

## Status

Fixture-validierter TOPCON-Keratorefraktometer-Kandidat fuer die MEDISTAR-XDT-Rueckgabe aus einer echten TOPCON KR-1 XML-Datei. Die aktuelle Fixture enthaelt REF-Daten; die praktische MEDISTAR-Abnahme steht noch aus.

## Validierter Ablauf

- MEDISTAR liefert eine AIS-GDT/XDT-Datei mit passender Untersuchungsart.
- TOPCON KR-1 liefert eine XML-Geraetedatei.
- Die App erkennt TOPCON KR-1 XML ueber `Ophthalmology`, `Company = TOPCON`, `ModelName = KR-1` und `Measure type="REF"`.
- Die App liest die vorhandenen REF-Medianwerte.
- Die App erzeugt eine MEDISTAR-kompatible XDT-Rueckgabedatei.
- MEDISTAR uebernimmt `8402` aus der AIS-Datei.
- MEDISTAR soll die `6228`-Autorefraktionszeilen uebernehmen.

## Validierte Ergebnisfelder

- `8000 = 6310`
- `3000` Patientennummer
- `3101` Nachname
- `3102` Vorname
- `3103` Geburtsdatum
- `8402` Untersuchungsart aus AIS
- `6228` REF / Autorefraktion

## Fixture-Ergebnis

Die folgenden Inhalte stammen aus der echten KR-1 XML-Fixture. Patientendaten sind nicht live dokumentiert; XDT-Laengenpraefixe werden in der App automatisch berechnet.

`M-Serial0001_20190411_120732_TOPCON_KR-1_4430227.xml`

```text
6228 R.:S=+ 0.75 Z=- 0.50*165 PD= 68 VD= 13.75
6228 L.:S=+ 0.50 Z=+ 0.00*  0
```

Validiert:

- TOPCON KR-1 XML mit Shift-JIS-Encoding wird gelesen.
- `Company = TOPCON`, `ModelName = KR-1`, `Measure type="REF"` werden erkannt.
- `VD = 13.75` wird gelesen.
- `PD/Distance = 68.00` wird als Binokular-PD verwendet.
- Rechte REF-Medianwerte: Sphäre `+0.75`, Zylinder `-0.50`, Achse `165`.
- Linke REF-Medianwerte: Sphäre `+0.50`, Zylinder `+0.00`, Achse `0`.
- Einzelne REF-List-Werte werden nicht statt Medianwerten fuer die MEDISTAR-Ausgabe verwendet.
- `SE`, `ConfidenceIndex`, `CataractMode` und `IOLMode` werden nicht nach MEDISTAR ausgegeben.
- Keine `6221`, `6227`, `6205`, `6220` oder `6330` fuer die aktuelle KR-1-Fixture.

## XML-Struktur

- Root: `Ophthalmology`
- Common-Namespace: `nsCommon`
- REF-Namespace: `nsREF`
- `Common/Company = TOPCON`
- `Common/ModelName = KR-1`
- `Measure type="REF"`
- REF-Daten: `REF/R/Median` und `REF/L/Median`
- PD: `PD/Distance` und `PD/Near`
- Fixture: Serial0001

## Ausgabehinweise

- Ausgabe erfolgt ueber `6228`.
- `8402` kommt aus AIS/MEDISTAR.
- Sphäre und Zylinder werden mit Vorzeichen und zwei Nachkommastellen ausgegeben.
- Achse wird als Ganzzahl im bestehenden REF-Format ausgegeben.
- PD und VD werden bevorzugt einmal in der rechten Zeile ausgegeben.
- Fehlende Augen oder optionale Werte werden weggelassen.
- Es entstehen keine leeren optionalen Fragmente.
- Wenn keine exportierbaren REF-Werte vorhanden sind, wird keine AIS-only-XDT erzeugt.

## Grenzen und offene Punkte

- Praktische MEDISTAR-Importvalidierung ist noch offen.
- KR-1 kann fachlich Keratometer-/KRT- und periphere KRT-Daten liefern; die aktuelle Fixture enthaelt aber keinen KM-/KRT-Block.
- `6221`-Keratometer-Ausgabe wird fuer KR-1 erst ergaenzt, wenn eine echte KR-1-Datei mit KM/KRT/peripheren KRT-Werten vorliegt.
- Near-PD wird aktuell nicht ungeprueft fuer die Ausgabe verwendet.
- Offizielles ZIP-Artefakt erst nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md`.
- Keine Live-Pfade, Kundendaten oder echten Patientendaten dokumentiert.
