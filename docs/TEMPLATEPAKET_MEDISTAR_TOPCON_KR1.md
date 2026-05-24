# Templatepaket-Kandidat: MEDISTAR + TOPCON KR-1

Stand: 2026-05-24

## Zweck

Fixture-validierter Kandidat fuer die MEDISTAR-Anbindung des TOPCON KR-1 Keratorefraktometers.

TOPCON KR-1 nutzt TOPCON-REF-XML mit `Ophthalmology`-Root, `nsCommon:Common` und `nsREF:Measure type="REF"`. Die aktuelle Fixture enthaelt REF-Medianwerte und wird ueber MEDISTAR `6228` ausgegeben.

## Enthaltene Profile

- AIS: `ais-medistar-default`
- Geraet: `device-topcon-kr1-default`
- Export: `export-medistar-topcon-kr1-default`
- Schnittstelle: `interface-medistar-topcon-kr1-default`

## Geraetedateien

- Dateityp: `.xml`
- Encoding: Shift-JIS wird unterstuetzt
- Root: `Ophthalmology`
- Common-Daten: `nsCommon:Common`
- REF-Daten: `nsREF:Measure type="REF"`
- Erkennung: `Company = TOPCON`, `ModelName = KR-1`, `Measure type="REF"`
- Testfixture: `M-Serial0001_20190411_120732_TOPCON_KR-1_4430227.xml`

## MEDISTAR-Ausgabe

- Feldkennung: `6228`
- `8402` kommt aus der AIS-/MEDISTAR-Datei.
- REF wird aus `REF/R/Median` und `REF/L/Median` erzeugt.
- `PD/Distance` wird als Binokular-PD in der rechten Zeile ausgegeben, wenn vorhanden.
- `VD` wird in der rechten Zeile ausgegeben, wenn vorhanden.
- Keine `6221`, keine `6227`, keine `6205`, keine `6220` und keine `6330` fuer die aktuelle KR-1-Fixture.

Fixture-Auszug:

```text
6228 R.:S=+ 0.75 Z=- 0.50*165 PD= 68 VD= 13.75
6228 L.:S=+ 0.50 Z=+ 0.00*  0
```

## Keratometer / KRT

- KR-1 ist fachlich als Keratorefraktometer-Kandidat gefuehrt.
- Die aktuelle Fixture enthaelt nur `Measure type="REF"` und keine echten KM-/KRT-/peripheren KRT-Werte.
- Es wird keine `6221`-Ausgabe erfunden.
- KM/KRT wird erst produktiv ergaenzt, wenn eine echte KR-1-KM/KRT-Fixture vorliegt.

## Teststatus

- Parser-, Shift-JIS-, Median- und MEDISTAR-Ausgabetests: `TopconKr1ProfileTests`
- Selektiver Templatepaket-Test: `MedistarTopconKr1TemplatePackageTests`
- BuiltIn-Katalogtests: `ProfileCatalogServiceTests`

## Offen

- Praktische MEDISTAR-Abnahme mit echter KR-1-Ausgabe.
- Echte KR-1-Datei mit KM/KRT/peripheren KRT-Werten sammeln.
- Danach fachliche `6221`-Keratometer-Zielausgabe pruefen und ergaenzen.
- Offizielles ZIP-Artefakt erst nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md`.
