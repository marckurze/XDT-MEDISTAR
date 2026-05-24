# Templatepaket-Kandidat: MEDISTAR + TOPCON CT-800A

Stand: 2026-05-24

## Zweck

Fixture-validierter Kandidat fuer die MEDISTAR-Anbindung des TOPCON CT-800A Non-Contact-Tonometers.

TOPCON CT-800A nutzt TOPCON-TM-XML mit `Ophthalmology`-Root, `nsCommon:Common` und `nsTM:Measure type="TM"`. Die Ausgabe orientiert sich am mehrzeiligen Tonometrieformat von NT530P, TRK2P und CT-1P, bleibt aber als eigenes BuiltIn-Profil gefuehrt.

## Enthaltene Profile

- AIS: `ais-medistar-default`
- Geraet: `device-topcon-ct800a-default`
- Export: `export-medistar-topcon-ct800a-default`
- Schnittstelle: `interface-medistar-topcon-ct800a-default`

## Geraetedateien

- Dateityp: `.xml`
- Root: `Ophthalmology`
- Common-Daten: `nsCommon:Common`
- Tonometrie-Daten: `nsTM:Measure type="TM"`
- Erkennung: `Company = TOPCON`, `ModelName = CT-800A`, `Measure type="TM"`
- Testfixtures:
  - `M-Serial0069_20190411_170716_TOPCON_CT-800A_AA3100481.xml`
  - `M-Serial0070_20190411_171259_TOPCON_CT-800A_AA3100481.xml`
  - `M-Serial0071_20190411_175130_TOPCON_CT-800A_AA3100481.xml`

## MEDISTAR-Ausgabe

- Feldkennung: `6205`
- `8402` kommt aus der AIS-/MEDISTAR-Datei.
- Rechts/links werden als Tonometrie-Liste mit Average und Uhrzeit ausgegeben.
- Vollstaendige CorrectedIOP/CCT-Werte werden je Auge als `6205`-Detailzeilen ausgegeben.
- Keine `6220`, keine `6228`, keine `6221`, keine `6227` und keine `6330` fuer CT-800A-Messwerte.

Fixture-Auszuege:

```text
6205 Tonometrie
6205 PR: 550 [550] um
6205 PL: 550 [550] um
6205 PR: Gemessen = 19.0 mmHg; Korrigiert = 19.0 mmHg;
6205 PL: Gemessen = 17.0 mmHg; Korrigiert = 17.0 mmHg;
6205 R = 18 20 [19.0] // L = 17 16 [17.0] mmHg 17:07
```

```text
6205 Tonometrie
6205 R = 18 18 [18.0] // L = 18 19 [19.0] mmHg 17:12
```

## CorrectedIOP und CCT

- `CorrectedIOP/Formula1` wird nur ausgegeben, wenn fuer das Auge `CCT`, `Measured/IOP_mmHg` und `Corrected/IOP_mmHg` vollstaendig vorhanden sind.
- `Param1 unit="mm"` wird in `um` umgerechnet, wenn die Parameterzeile ausgegeben wird.
- Leere CCT-, Measured- oder Corrected-Werte erzeugen keine `PR`-/`PL`-Detailzeilen.
- ERROR-/Alignment-Bloecke ausserhalb der strukturierten TOPCON-TM-Messwerte werden nicht als harte Fehler und nicht als zusaetzliche Messwerte verwendet.
- `ConfidenceIndex` wird aktuell nicht nach MEDISTAR exportiert.

## Teststatus

- Parser-, Namespace- und MEDISTAR-Ausgabetests: `TopconCt800AProfileTests`
- Exportprofil-/BuiltIn-Tests: `DeviceProfileDefinitionTests`, `ExportProfileDefinitionTests`, `InterfaceProfileDefinitionTests`
- Selektiver Templatepaket-Test: `MedistarTopconCt800ATemplatePackageTests`

## Offen

- Praktische MEDISTAR-Abnahme mit echter CT-800A-Ausgabe.
- Weitere echte CT-800A-Dateien mit einseitigen Messungen sammeln.
- Separates `6220`-Pachymetrie-Zielformat nur nach Praxisvalidierung ergaenzen.
- Offizielles ZIP-Artefakt erst nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md`.
