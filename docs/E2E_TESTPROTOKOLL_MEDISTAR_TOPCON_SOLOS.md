# E2E-Testprotokoll MEDISTAR + TOPCON Solos

Stand: 2026-05-24

## Status

Fixture-validierter Lensmeter-Kandidat fuer die MEDISTAR-XDT-Rueckgabe aus TOPCON Solos XML. Die praktische MEDISTAR-Abnahme steht noch aus.

## Validierter Ablauf

- MEDISTAR liefert eine AIS-GDT/XDT-Datei mit passender Untersuchungsart.
- TOPCON Solos liefert eine XML-Geraetedatei.
- Die App erkennt TOPCON Solos XML ueber `Ophthalmology`, `Company = TOPCON`, `ModelName = SOLOS` und `Measure type="LM"`.
- Die App liest Lensmeter-Werte fuer vorhandene Augen.
- Die App erzeugt eine MEDISTAR-kompatible XDT-Rueckgabedatei.
- MEDISTAR uebernimmt `8402` aus der AIS-Datei.
- MEDISTAR soll die `6228`-Ergebniszeilen fuer rechts und links uebernehmen.

## Validierte Ergebnisfelder

- `8000 = 6310`
- `3000` Patientennummer
- `3101` Nachname
- `3102` Vorname
- `3103` Geburtsdatum
- `8402` Untersuchungsart aus AIS
- `6228` rechte Lensmeter-Zeile
- `6228` linke Lensmeter-Zeile

## Fixture-Ergebnis

Die folgenden Inhalte stammen aus `SolosExportSample.xml`. Patientendaten sind nicht live dokumentiert; XDT-Laengenpraefixe werden in der App automatisch berechnet.

```text
6228 R.:S=- 1.25 Z=+ 0.00*  0 P=H=-0.50 PD= 65.5
6228 L.:S=- 1.25 Z=+ 0.00*  0 P=H=+1.00
```

Validiert:

- TOPCON Solos XML wird erkannt.
- `Company = TOPCON`, `ModelName = SOLOS`, `Measure type="LM"` werden gelesen.
- Rechts und links werden Sphere, Cylinder und Axis gelesen.
- Gesamt-PD `65.5` wird erkannt und rechts ausgegeben.
- H-Prismen werden als signierte Komponenten ausgegeben.
- V-Prisma `0` wird ausgelassen.
- Leere ADD-/ADD2-Tags erzeugen keine Fragmente.
- Keine leeren optionalen Fragmente.
- Keine `6205`, `6220`, `6221`, `6227` oder `6330` fuer Solos-Messwerte.

## XML-Struktur

- Root: `Ophthalmology`
- Common-Namespace: `nsCommon`
- Lensmeter-Namespace: `nsLM`
- `Common/Company = TOPCON`
- `Common/ModelName = SOLOS`
- `Measure type="LM"`
- Encoding: UTF-8
- Fixture: `SolosExportSample.xml`

## Ausgabehinweise

- Ausgabe erfolgt ueber `6228`.
- `8402` kommt aus AIS/MEDISTAR.
- ADD/ADD2 werden nur ausgegeben, wenn vorhanden.
- PD wird nur ausgegeben, wenn vorhanden.
- Prisma wird konservativ als signierte H/V-Komponente ausgegeben.
- OUT/IN/UP/DOWN wird nicht geraten.
- Null-Prismen werden weggelassen.
- Fehlende Augen werden weggelassen.
- Es entstehen keine leeren optionalen Fragmente.

## Grenzen und offene Punkte

- Praktische MEDISTAR-Importvalidierung ist noch offen.
- Transmission ist in der aktuellen Fixture nicht gefuellt; `UVTransmittance` wird erkannt, aber noch nicht exportiert.
- Blau-/sichtbares-Licht-Transmission benoetigt echte XML-Beispiele.
- Solos-PDF-Berichte benoetigen echte PDF-Fixtures und Ablaufregeln, bevor sie ueber XDT-Anhanglogik angebunden werden.
- Offizielles ZIP-Artefakt erst nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md`.
- Keine Live-Pfade, Kundendaten oder echten Patientendaten dokumentiert.
