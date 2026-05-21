# Templatepaket-Kandidat: MEDISTAR + TOPCON TRK2P

Status: testseitig vorbereitet, praktische MEDISTAR-Validierung offen.

## Zweck

Dieses Paket beschreibt die direkte Anbindung `MEDISTAR + TOPCON TRK2P` fuer originale TOPCON-TRK-2P-XML-Dateien.

Der TRK-2P kombiniert:

- REF / objektive Refraktion ueber `6228`
- KM / Keratometer ueber `6221`
- Pachymetrie / CCT ueber `6220`, wenn CCT vorhanden ist
- TM / Tonometrie ueber `6205`
- SBJ / subjektive Refraktion ueber `6227`, falls echte SBJ-Werte vorhanden sind

## Enthaltene BuiltIns

- AIS: `ais-medistar-default`
- Geraet: `device-topcon-trk2p-default`
- Export: `export-medistar-topcon-trk2p-default`
- Schnittstelle: `interface-medistar-topcon-trk2p-default`

## XML-Erkennung

Erwartet wird ein TOPCON JOIA-/Ophthalmology-XML:

- Root `Ophthalmology`
- `Common/Company = TOPCON`
- `Common/ModelName = TRK-2P`
- `Measure type="REF"`
- `Measure type="KM"`
- `Measure type="TM"`
- optional `Measure type="CCT"`
- optional `Measure type="SBJ"`

Die Namespaces `nsCommon`, `nsREF`, `nsKM`, `nsTM` und optional `nsSBJ` werden namespace-tolerant gelesen.

## Ausgabe

REF:

- rechte und linke vorbereitete `MedistarLine`
- Ausgabe ueber `6228`
- PD und VD werden einmal in der rechten REF-Zeile ausgegeben, wenn vorhanden

KM:

- eine `6221`-Zeile fuer R/L R1/R2
- eine `6221`-Zeile fuer R/L AV/CYL
- Median-Werte werden verwendet

Pachymetrie:

- `6220` mit Header `Pachymetrie` und eigener `RA`/`LA`-Zeile, wenn CCT vorhanden ist
- separate `Measure type="CCT"` wird bevorzugt
- falls keine separate CCT-Messung vorhanden ist, wird `CorrectedIOP/CCT` als Fallback genutzt
- CCT-Listen duerfen `ERROR`-Eintraege enthalten; fuer die Ausgabe werden nur gueltige `CCT_mm`-Werte verwendet
- wenn kein Average vorhanden ist, wird aus den gueltigen CCT-Werten ein arithmetischer Mittelwert gebildet und in Mikrometer gerundet, z. B. `0.511` und `0.509` -> `[510] µm` beziehungsweise `RA: 0.510`

Tonometrie:

- `6205` mit Header `Tonometrie`
- IOP-Listen/Average werden als eigene `6205`-Zeile ausgegeben
- bei CorrectedIOP werden gemessener/korrigierter IOP, Param1, Param2 und CCT in mehrere lesbare `6205`-Zeilen aufgeteilt
- TM/CCT-only-Dateien ohne REF/KM/SBJ sind zulaessig und erzeugen nur `6205`/`6220`
- keine EV-Zusaetze

SBJ:

- nur bei echten SBJ-Werten
- konservative `6227`-Zeilen analog KR800S
- keine leeren R-/L-Zeilen

Nicht enthalten:

- keine Messwertausgabe ueber `6302` bis `6305`
- keine 6330-Zeilentypautomatik
- keine Anhang-ZIP-/Sammellogik

## Testfixtures

Im Repository liegen echte TRK-2P-Fixtures:

- `XdtDeviceBridge.Tests/TestData/Devices/Topcon/TRK2P/M-Serial0001_20190411_113829_TOPCON_TRK-2P_5270367.xml`
- `XdtDeviceBridge.Tests/TestData/Devices/Topcon/TRK2P/M-Serial0135_20130809_174556_TOPCON_TRK-2P_.xml`
- `XdtDeviceBridge.Tests/TestData/Devices/Topcon/TRK2P/M-Serial1165_20241126_225512_TOPCON_TRK-2P_5284298.xml`

Serial0001 validiert REF, KM und TM ohne CCT/SBJ.

Serial0135 validiert REF, KM, TM, CorrectedIOP und CCT-Fallback.
Die Tonometrie-/Pachymetrieausgabe ist dabei an das bereits erfolgreiche NT530P-Mehrzeilenformat angelehnt.

Serial1165 validiert eine Teilmessung mit nur TM/CCT: keine REF-/KM-/SBJ-Zeilen, keine leeren Platzhalter, CCT-ERROR-Eintraege werden ignoriert.

## Tests

Abgesichert durch:

- `TopconTrk2PProfileTests`
- `MedistarTopconTrk2PTemplatePackageTests`
- BuiltIn-Definitionstests fuer Geraet, Export und Schnittstelle
- gezielte Reparaturtests fuer alte persistierte BuiltIn-TRK2P-Profile
- generische AlreadyProcessed-/Duplikat-Nachlauf-Tests: neue Fingerprints werden verarbeitet, echte Duplikate werden nicht erneut exportiert und bekannte Importdateien werden archiviert, in den Fehlerordner verschoben oder entfernt

## Offen

- praktische MEDISTAR-Validierung mit beiden XML-Dateien
- erneuter Live-Test nach korrigiertem AlreadyProcessed-/Duplikat-Nachlauf
- Bewertung der neuen mehrzeiligen `6205`-/`6220`-Anzeige mit CorrectedIOP/CCT in MEDISTAR
- erneuter Live-Test mit TM/CCT-only-Datei Serial1165
- SBJ-Ausgabe erst nach echten TRK-2P-SBJ-Dateien praktisch bewerten
- offizielles ZIP-Artefakt erst nach Release-Regel
