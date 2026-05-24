# Templatepaket-Kandidat: MEDISTAR + TOPCON Solos

Stand: 2026-05-24

## Zweck

Fixture-validierter Kandidat fuer die MEDISTAR-Anbindung des TOPCON Solos Lensmeters.

TOPCON Solos nutzt TOPCON-Lensmeter-XML mit `Ophthalmology`-Root, `nsCommon:Common` und `nsLM:Measure type="LM"`. Die Ausgabe orientiert sich fachlich an TOPCON CL-300 und NIDEK LM7, bleibt aber als eigenes BuiltIn-Profil gefuehrt.

## Enthaltene Profile

- AIS: `ais-medistar-default`
- Geraet: `device-topcon-solos-default`
- Export: `export-medistar-topcon-solos-default`
- Schnittstelle: `interface-medistar-topcon-solos-default`

## Geraetedateien

- Dateityp: `.xml`
- Root: `Ophthalmology`
- Common-Daten: `nsCommon:Common`
- Lensmeter-Daten: `nsLM:Measure type="LM"`
- Erkennung: `Company = TOPCON`, `ModelName = SOLOS`, `Measure type="LM"`
- Testfixture: `SolosExportSample.xml`

## MEDISTAR-Ausgabe

- Feldkennung: `6228`
- `8402` kommt aus der AIS-/MEDISTAR-Datei.
- Rechts/links werden als berechnete Lensmeter-Zeilen ausgegeben.
- Keine `6205`, keine `6220`, keine `6221`, keine `6227` und keine `6330` fuer Solos-Messwerte.
- Keine `6302` bis `6305`, solange keine separate Anhanglogik fuer PDF-Berichte konfiguriert wird.

Fixture-Auszug:

```text
R.:S=- 1.25 Z=+ 0.00*  0 P=H=-0.50 PD= 65.5
L.:S=- 1.25 Z=+ 0.00*  0 P=H=+1.00
```

## PD, Addition und Prisma

- Gesamt-PD aus `PD/B/Distance` wird, wenn vorhanden, an der rechten `6228`-Zeile ausgegeben.
- ADD/ADD2 werden nur ausgegeben, wenn echte Werte vorhanden sind.
- Leere `Add1`-/`Add2`-Tags erzeugen keine Fragmente.
- TOPCON-H/V-Prismen werden konservativ als signierte Komponenten ausgegeben, z. B. `P=H=-0.50`.
- Null-Prismen wie `0`, `+0.00` oder `-0.00` werden ausgelassen.
- Es werden keine OUT/IN/UP/DOWN-Basisrichtungen geraten.

## Transmission und PDF-Berichte

- `UVTransmittance` wird parserseitig als optionaler Messwert erkannt, aber noch nicht nach MEDISTAR exportiert.
- Blau- und sichtbares-Licht-Transmission werden erst umgesetzt, wenn echte XML-Tags/Faecher in einer Solos-Fixture eindeutig belegt sind.
- Automatische PDF-Anhangslogik fuer Solos wird in diesem Schritt nicht eingefuehrt. Solos-PDF-Berichte koennen spaeter ueber die bestehende XDT-Anhanglogik angebunden werden, sobald echte PDF-Dateien und Ablaufregeln vorliegen.

## Teststatus

- Parser-, Namespace- und MEDISTAR-Ausgabetests: `TopconSolosProfileTests`
- Exportprofil-/BuiltIn-Tests: `DeviceProfileDefinitionTests`, `ExportProfileDefinitionTests`, `InterfaceProfileDefinitionTests`
- Selektiver Templatepaket-Test: `MedistarTopconSolosTemplatePackageTests`

## Offen

- Praktische MEDISTAR-Abnahme mit echter Solos-Ausgabe.
- Echte Solos-Datei mit gefuellten Transmission-Werten sammeln.
- Echte PDF-Berichte und Dateinamen-/Ablaufregel fuer optionale Anhanguebergabe sammeln.
- Offizielles ZIP-Artefakt erst nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md`.
