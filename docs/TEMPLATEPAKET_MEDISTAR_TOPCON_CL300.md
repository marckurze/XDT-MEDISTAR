# Templatepaket MEDISTAR + TOPCON CL300

Stand: 2026-05-21

## Zweck

Kandidat fuer die MEDISTAR-Anbindung des TOPCON CL-300 Lensmeters.

## Enthaltene Profile

- AIS: `ais-medistar-default`
- Geraet: `device-topcon-cl300-default`
- Export: `export-medistar-topcon-cl300-default`
- Schnittstelle: `interface-medistar-topcon-cl300-default`

## Geraetedateien

- Dateityp: `.xml`
- Root: `Ophthalmology`
- Common-Daten: `nsCommon:Common`
- Lensmeter-Daten: `nsLM:Measure type="LM"`
- Testfixtures:
  - `M-Serial0001_20120101_000000_TOPCON_CL-300_00.xml`
  - `M-Serial1521_20120101_000000_TOPCON_CL-300_00.xml`

## MEDISTAR-Ausgabe

- Feldkennung: `6228`
- Rechts/links werden als berechnete Lensmeter-Zeilen ausgegeben.
- `8402` kommt weiter aus der AIS-/MEDISTAR-Datei.
- Keine `6205`, keine `6220`, keine `6330`-Automatik.
- Keine Anhangfelder `6302` bis `6305`, solange keine separate Anhanglogik fuer dieses Profil konfiguriert wird.

Beispiele testseitig:

```text
R.:S=- 0.25 Z=- 1.00* 91
L.:S=+ 0.00 Z=- 1.00* 87

R.:S=+ 3.75 Z=- 3.50*  9 A=+ 2.25 P=H=+0.25 V=-0.75 PD= 55
L.:S=+ 3.50 Z=- 3.00*178 A=+ 2.25 P=V=-1.00
```

## PD und Prisma

- Gesamt-PD aus `PD/B/Distance` wird, wenn vorhanden, an der rechten `6228`-Zeile ausgegeben.
- TOPCON-H/V-Prismen werden signiert als Komponenten uebergeben, z. B. `P=H=+0.25 V=-0.75`.
- Null-Prismen wie `+0.00` oder `-0.00` werden ausgelassen.
- Es werden keine OUT/IN/UP/DOWN-Basisrichtungen geraten.

## Teststatus

- Parser- und Namespace-Tests: `TopconCl300ProfileTests`
- Exportprofil-/BuiltIn-Tests: `DeviceProfileDefinitionTests`, `ExportProfileDefinitionTests`, `InterfaceProfileDefinitionTests`
- Selektiver Templatepaket-Test: `MedistarTopconCl300TemplatePackageTests`

## Offen

- Praktische MEDISTAR-Validierung mit beiden Originaldateien.
- Entscheidung nach Live-Test, ob die H/V-Prismendarstellung fachlich angepasst werden muss.
- Offizielles ZIP-Artefakt erst nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md`.
