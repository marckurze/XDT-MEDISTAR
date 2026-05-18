# Templatepaket MEDISTAR + NIDEK NT530P

Dieses Dokument beschreibt den Templatepaket-Kandidaten fuer `MEDISTAR + NIDEK NT530P` / `NT-530P`. Die technische Verarbeitung ist mit einer echten NT-530P-XML-Fixture abgesichert; eine praktische MEDISTAR-Validierung steht noch aus. Ein offizielles ZIP-Artefakt wird erst nach der Release-Regel in `docs/TEMPLATEPAKET_RELEASE_REGEL.md` dauerhaft abgelegt.

## Enthaltene Profile

| Typ | ID | Name | Hinweis |
| --- | --- | --- | --- |
| AIS-Profil | `ais-medistar-default` | `MEDISTAR` | BuiltIn |
| Geraeteprofil | `device-nidek-nt530p-default` | `NIDEK NT530P` | BuiltIn, Tonometer/Pachymeter XML |
| Exportprofil | `export-medistar-nidek-nt530p-default` | `MEDISTAR + NIDEK NT530P Export` | BuiltIn, `6220` Pachymetrie und `6205` Tonometrie |
| Schnittstellenprofil | `interface-medistar-nidek-nt530p-default` | `MEDISTAR + NIDEK NT530P` | BuiltIn, inaktiv |

## Datenformat

- NT-530P schreibt XML-Dateien mit Root `Data`.
- `Company=NIDEK` und `ModelName=NT-530P` kennzeichnen die Messdatei.
- Die XML enthaelt `NT`-Werte fuer Tonometrie, `CorrectedIOP` und `PACHY`-Werte fuer Pachymetrie.
- Optional koennen JPG-Aufnahmen im Ordner liegen und in `PACHYImage` referenziert werden.
- JPG-/JPEG-Dateien werden nicht als Messwertdatei geparst. Liegen sie als unterstuetzte XDT-Anhaenge im konfigurierten Anhang-Importordner, kann die generische Mehrfachanhang-Logik sie einzeln ueber `6302` bis `6305` uebergeben.

## MEDISTAR-Ausgabe

Das Exportprofil nutzt keine `6228`-Geraetewertzeilen fuer NT530P.

| Feldkennung | Inhalt |
| --- | --- |
| `6205` | Tonometrie mit Header `Tonometrie`, Pachy-Einzelwerten, gemessenem/korrigiertem IOP, CCT, Einzelmessungen, Mittelwert und Uhrzeit |
| `6220` | Pachymetrie mit Header `Pachymetrie`, z. B. `RA: 0.596   // LA: 0.596` |

Der fruehere Zusatz `/ EV:{000000003B} NT-530P Messung` wird nicht mehr erzeugt.

Die Werte stammen aus der XML. Benutzerbeispiele dienen nur als Formatvorlage und werden nicht als Messwerte uebernommen.

## Testabdeckung

`NidekNt530PProfileTests` prueft Erkennung, UTF-16-Fixture, Tonometrie-/Pachymetriewerte, JPG-Klassifizierung, berechnete mehrzeilige MEDISTAR-Werte, `6220`/`6205`-Export, fehlende `6228`-Ausgabe, BuiltIn-Profile und Reparatur alter persistierter BuiltIn-NT530P-Exportprofile. `MedistarNidekNt530PTemplatePackageTests` prueft den selektiven Templatepaket-Export und -Import temporaer im Testordner. Das Paket enthaelt nur MEDISTAR, NIDEK NT530P, das passende Exportprofil und das Schnittstellenprofil; ARK1S-, AR360-, LM7- und TOPCON-Profile sind nicht enthalten.

## Offen

- Praktische MEDISTAR-Validierung mit echtem Importlauf.
- Fachliche Abnahme der korrigierten mehrzeiligen Tonometrie-/Pachymetrie-Darstellung in der Karteikarte.
- XDT-Anhang-Link fuer NT530P-JPG-Aufnahmen praktisch validieren; die generische Mehrfachanhang-Mechanik ist testseitig abgesichert.
- Offizielles ZIP-Artefakt erst nach Release-Regel und App-Abnahme ablegen.
