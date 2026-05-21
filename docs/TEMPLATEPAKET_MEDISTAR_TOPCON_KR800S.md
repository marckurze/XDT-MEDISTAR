# Templatepaket MEDISTAR + TOPCON KR800S

Stand: 2026-05-21

## Zweck

Testseitig abgesicherter Kandidat fuer die MEDISTAR-Anbindung des TOPCON KR-800S mit Autorefraktion, Keratometrie und konservativer subjektiver Refraktionsausgabe.

## Enthaltene Profile

- AIS: `ais-medistar-default`
- Geraet: `device-topcon-kr800-default`
- Export: `export-medistar-topcon-kr800-default`
- Schnittstelle: `interface-medistar-topcon-kr800-default`

## Geraetedateien

- Dateityp: `.xml`
- Encoding: `Shift-JIS`
- Root: `Ophthalmology`
- Common-Daten: `nsCommon:Common`
- Autorefraktion: `nsREF:Measure type="REF"`
- Keratometrie: `nsKM:Measure type="KM"`
- Subjektive Daten: `nsSBJ:Measure type="SBJ"`
- Testfixtures:
  - `M-Serial0036_20131206_213127_TOPCON_KR-800S_.xml`
  - `M-Serial0426_20241126_145500_TOPCON_KR-800S_4871341.xml`

## MEDISTAR-Ausgabe

- `6228`: REF/Autorefraktor-Zeilen rechts und links aus den Median-Werten.
- `6221`: KM/Keratometer-Zeilen fuer R1/R2 sowie AV/CYL.
- `6227`: konservative SBJ-Zeilen fuer vorhandene Full-Correction-Fern-/Nahwerte; Header und Messwert werden jeweils getrennt ausgegeben.
- `8402` kommt weiter aus der AIS-/MEDISTAR-Datei.
- Keine `6205`, keine `6220`, keine `6330`-Automatik.
- Keine Anhangfelder `6302` bis `6305` fuer die Messwerte.
- Persistierte alte BuiltIn-Exportprofile werden beim Katalogstart gezielt repariert, wenn sie noch root-prefixed Einzelplatzhalter oder Keratometer-Regeln ueber `6228` enthalten.

Beispiele testseitig:

```text
R.:S=- 5.50 Z=+ 0.00*  0 PD= 58 VD= 13.75
L.:S=- 5.25 Z=+ 0.00*  0

R: R1=8.48 39.75 *11 R2=7.79 43.50 *101 // L: R1=8.35 40.50 *171 R2=7.87 43.00 *81
R: AV=8.14 41.75 CYL=-3.75 11 // L: AV=8.11 41.75 CYL=-2.50 171

Subjektive Refraktion Full Correction FAR:
R.:S=+ 3.75 Z=- 4.00* 13 VA=0.6 / L.:S=+ 3.75 Z=- 2.50*173 VA=1.0 PD=66 VD=13.75
Subjektive Refraktion Full Correction NEAR:
R.:S=+ 5.50 Z=- 4.00* 13 / L.:S=+ 5.50 Z=- 2.50*173 PD=66 VD=13.75
```

## SBJ-Hinweise

- Ausgegeben werden nur vorhandene, sinnvolle RefractionData-Zeilen.
- Leere R-/L-Zeilen, `Unaided Data`, leere `ContrastVA`- und `GlareVA`-Bloecke werden nicht ausgegeben.
- FAR/NEAR wird aus der ExamDistance konservativ abgeleitet.
- Header wie `Subjektive Refraktion Full Correction FAR:` und die eigentlichen R/L-Werte stehen in getrennten `6227`-Zeilen, damit MEDISTAR die subjektive Refraktion lesbarer darstellt.
- Die praktische Bewertung der `6227`-Darstellung in MEDISTAR steht noch aus und kann nach weiteren echten Beispielen verfeinert werden.

## Teststatus

- Parser- und Namespace-Tests: `TopconKr800SProfileTests`
- Exportprofil-/BuiltIn-Tests: `DeviceProfileDefinitionTests`, `ExportProfileDefinitionTests`, `InterfaceProfileDefinitionTests`
- Katalog-Reparaturtests fuer alte persistierte KR800S-BuiltIns: `ProfileCatalogServiceTests`
- Selektiver Templatepaket-Test: `MedistarTopconKr800STemplatePackageTests`

## Offen

- Praktische MEDISTAR-Validierung nach dem Fix mit echten KR-800S-XML-Dateien.
- Getrennte SBJ-`6227`-Header-/Messwertzeilen im naechsten MEDISTAR-Live-Test pruefen.
- Offizielles ZIP-Artefakt erst nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md`.
