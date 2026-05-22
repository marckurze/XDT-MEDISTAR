# Templatepaket-Kandidat: MEDISTAR + TOPCON CT1P

Stand: 2026-05-22

Dieses Paket beschreibt die direkte Anbindung `MEDISTAR + TOPCON CT1P` fuer originale TOPCON-CT-1P-XML-Dateien.

## Status

- Praktisch validierter TOPCON-Referenzkandidat fuer Tonometrie-/Pachymetrie-XDT-Rueckgabe.
- Praxisprotokoll: `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_CT1P.md`.
- Offizielles ZIP-Artefakt wird erst nach der Release-Regel abgelegt.

## Enthaltene BuiltIns

- AIS-Profil: `ais-medistar-default`
- Geraeteprofil: `device-topcon-ct1p-default`
- Exportprofil: `export-medistar-topcon-ct1p-default`
- Schnittstellenprofil: `interface-medistar-topcon-ct1p-default`

## Geraet

- Hersteller: TOPCON
- Modell: CT-1P
- Typ: kontaktfreies Tonometer / Pachymeter
- Dateityp: XML, Dateiendung `.xml` / `.XML`
- Root: `Ophthalmology`
- Erkennung: `Common/Company = TOPCON`, `Common/ModelName = CT-1P`, `Measure type="TM"`
- Namespaces: `nsCommon`, `nsTM`

Das CT-1P misst fachlich vollautomatisch beidseitig: Nach dem Antippen der Pupillenmitte misst es das rechte Auge und faehrt danach selbststaendig zum linken Auge. Tonometrie und Pachymetrie/CCT werden kombiniert erfasst; korrigierte IOP-Werte koennen pro Auge aus CorrectedIOP ausgegeben werden, wenn sie in der XML vollstaendig vorhanden sind.

## MEDISTAR-Ausgabe

- Tonometrie ueber `6205`
- Pachymetrie ueber `6220`
- `8402` Untersuchungsart kommt aus AIS/MEDISTAR
- keine `6228`, `6221`, `6227` fuer CT-1P-Messwerte
- keine `6302` bis `6305` fuer Messwerte

Die Tonometrie nutzt das mehrzeilige NT530P/TRK2P-Format:

```text
6205 Tonometrie
6205 PR: 767 [767] µm
6205 PR: Gemessen = 16.0 mmHg; Korrigiert = 5.0 mmHg;
6205 PR: Param1 = 545um; Param2 = 0.050; CCT = 767um
6205 R = 13 12 13 26 [16.0] // L = 44 44 44 [44.0] mmHg 15:20
```

Die Pachymetrie wird nur fuer vorhandene CCT-Werte erzeugt:

```text
6220 Pachymetrie
6220 RA: 0.767
```

Wenn ein Auge im `CorrectedIOP`-Block nur Parameter, aber kein verwertbares CCT/Measured/Corrected enthaelt, wird dafuer keine leere PL-/LA-Zeile erzeugt. Wenn spaetere CT-1P-Dateien links vollstaendige CorrectedIOP/CCT-Werte enthalten, kann dieselbe per-eye-Logik diese Werte ausgeben.

## Testfixture

- `XdtDeviceBridge.Tests/TestData/Devices/Topcon/CT1P/M-Serial0214_20130620_152002_TOPCON_CT-1P_2630218.xml`

Die Fixture enthaelt:

- TM rechts: `13.0`, `12.0`, `13.0`, `26.0`, Average `16.0`
- TM links: `44.0`, `44.0`, `44.0`, Average `44.0`
- CorrectedIOP rechts: Param1 `0.545`, Param2 `0.050`, CCT `0.767`, gemessen `16.0`, korrigiert `5.0`
- CorrectedIOP links: nur Param1/Param2, kein verwertbares CCT/Measured/Corrected

## Tests

- Parser- und Namespace-Tests: `TopconCt1PProfileTests`
- MEDISTAR-XDT-Export ueber `6205`/`6220`: `TopconCt1PProfileTests`
- BuiltIn-Profiltests: `DeviceProfileDefinitionTests`, `ExportProfileDefinitionTests`, `InterfaceProfileDefinitionTests`, `ProfileCatalogServiceTests`
- Selektiver Templatepaket-Test: `MedistarTopconCt1PTemplatePackageTests`

## Grenzen / offen

- Weitere CT-1P-Dateien mit vollstaendigen linken CorrectedIOP/CCT-Werten sollen die beidseitige `PR`/`PL`- und `RA`/`LA`-Ausgabe testseitig festnageln.
- Dauerhafte ZIP-Ablage erst nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md`.
