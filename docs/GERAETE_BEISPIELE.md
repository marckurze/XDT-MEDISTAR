# Gerätebeispiele und Profilableitungen

## 1. Zweck des Dokuments

Dieses Dokument sammelt Erkenntnisse aus bereitgestellten Beispielordnern verschiedener ophthalmologischer Geräte. Es dient als Grundlage für spätere Geräteprofile, Export-/Mapping-Profile, PDF-Dokumentenerzeugung und MEDISTAR-EV-Verknüpfungen.

Hinweis: Die aktuelle App implementiert diese Profile noch nicht vollständig. Das Dokument dient der fachlichen Vorbereitung.

## 2. Übersicht erkannter Geräte

| Hersteller | Gerät | Gerätetyp | beobachtete Dateiformate | Untersuchungsarten | Zusatzdateien | MEDISTAR-Zielbild | spätere Profilrelevanz |
| --- | --- | --- | --- | --- | --- | --- | --- |
| NIDEK | AR1S | Autorefraktometer | XML | Refraktion, PD | keine im aktuellen Standardfall | zwei `6228`-Ergebniszeilen rechts/links, `8000=6310`, `8402` aus AIS | erstes validiertes Standardprofil, Referenz für Refraktionsformatierung |
| NIDEK | LM7 | Lensmeter / Scheitelbrechwertmesser | XML oder gerätespezifisches Format aus Beispieldaten | Brillenwerte, Sphäre, Zylinder, Achse, Addition, Prisma, Basisrichtung, PD | keine zwingend erkennbar | Lensmeter-Ergebniszeilen mit Sphäre/Zylinder/Achse/Prisma/PD | zweites naheliegendes Geräteprofil wegen ähnlicher refraktiver Ergebniszeilen |
| NIDEK | NT530P | Non-Contact-Tonometer / Pachymeter | XML, JPG | Tonometrie, Pachymetrie, korrigierter IOP, Messbilder/Protokollverweise | JPG-Bilder, ggf. XML-Verweise wie `PACHYImage` | Pachymetrie- und Tonometriezeilen, optional EV-Verweis | wichtig für Attachments, PDF-Protokolle und EV-Verknüpfung |
| TOPCON | CL300 | Lensmeter | Ophthalmology-/JOIA-XML | Lensmeterdaten, Sphäre, Zylinder, Achse, PD | keine zwingend erkennbar | Lensmeter-Ergebniszeilen ähnlich LM7 | erstes TOPCON-/JOIA-Profil mit Namespace- und Attributanforderungen |
| TOPCON | KR800 | Autorefraktometer / Keratometer | Ophthalmology-/JOIA-XML | `REF`, `KM`, `SBJ` | keine zwingend erkennbar | getrennte Ergebniszeilen für Refraktion, Keratometrie und optional subjektive Daten | relevant für Mehruntersuchungsdateien und Measure-Type-Selektion |
| TOPCON | TRK2P | Tonometer / Refraktions-Keratometer je nach Dateninhalt | Ophthalmology-/JOIA-XML | `TM`, `CCT` | keine zwingend erkennbar | Tonometrie- und Pachymetrieausgabe | relevant für Tonometrie/CCT-Kombination und JOIA-Parserlogik |

## 3. NIDEK AR1S

Gerätetyp: Autorefraktometer

Dateiformat: XML

Untersuchungsarten:

- Refraktion
- PD

Werte:

- Sphäre rechts/links
- Zylinder rechts/links
- Achse rechts/links
- SE rechts/links
- PD/FarPD
- PD/NearPD

Validierter aktueller Prototyp:

- `8000 = 6310`
- `8402 = Untersuchungsart aus AIS`, z. B. `ARK1S`
- `6228` Ergebnis rechts
- `6228` Ergebnis links

Beispielausgabe MEDISTAR:

```text
V1 R.:S=- 0.25 Z=- 0.25* 49                              PD=61
V1 L.:S=+ 0.00 Z=- 0.50* 63                              PD=61
```

Status: bereits im Prototyp erfolgreich validiert.

## 4. NIDEK LM7

Gerätetyp: Lensmeter / Scheitelbrechwertmesser

Dateiformat: XML oder gerätespezifisch aus Beispieldaten

Untersuchungsarten:

- gemessene Brillenwerte
- Sphäre
- Zylinder
- Achse
- Prisma
- Basisrichtung
- PD

Beispielausgabe MEDISTAR:

```text
V0   R.:S=+ 6.50 Z=- 1.75*172 P=0.75 OUT 1.00 UP           PD= 59
V0   L.:S=+ 6.00 Z=- 2.25*  2 P=0.50 OUT 1.50 UP
```

Ableitung:

- eigenes Exporttemplate erforderlich
- Dioptrienformatierung erforderlich
- Achsenformatierung erforderlich
- Prisma-/Basisformatierung erforderlich
- PD optional rechts/gesamt

Spätere Profilanforderung:

- Geräteprofil `NIDEK LM7`
- Exportprofil `MEDISTAR + NIDEK LM7`
- Ergebniszeilen über `6228` oder MEDISTAR-konfiguriertes Ziel prüfen

## 4.1 NIDEK LM7 – erkannte SourcePaths

Analysierte Beispieldateien:

- `C:\Users\MarcK\Downloads\Geraeteanbindungen\NIDEK LM7\MEDISTAR Eintrag Lensmeter.txt`
- `C:\Users\MarcK\Downloads\Geraeteanbindungen\NIDEK LM7\RKT\TXT\LM7.XML`

Im Repository selbst wurden keine LM7-Beispieldateien gefunden. Die lokale Datei `LM7.XML` ist ein XML-Fragment ohne XML-Deklaration und ohne vollständigen Root-Knoten. Das Fragment enthält aktuell nur den Knoten `R` für das rechte Auge. Linkes Auge und PD sind im vorliegenden XML-Fragment nicht enthalten und müssen deshalb weiterhin anhand weiterer Beispieldateien validiert werden.

Beobachtete XML-Struktur:

```xml
<R>
  <Sphare unit="D">1.50</Sphare>
  <Cylinder unit="D">-0.50</Cylinder>
  <Axis unit="deg">128</Axis>
  <SE unit="D"></SE>
  <ADD unit="D">1.50</ADD>
  <ADD2 unit="D"></ADD2>
  <NearSphare unit="D">3.00</NearSphare>
  <NearSphare2 unit="D"></NearSphare2>
  <Prism unit="pri">2.00</Prism>
  <PrismBase unit="deg">251</PrismBase>
  <PrismX unit="pri" base="out">0.75</PrismX>
  <PrismY unit="pri" base="down">2.00</PrismY>
  <UVTransmittance unit="%">83</UVTransmittance>
</R>
```

Erkannte bzw. abgeleitete SourcePaths:

| Messwert | Erkannter SourcePath | Beispielwert | Status / Hinweis |
|---|---|---:|---|
| R Sphere | `R/Sphare` | `1.50` | erkannt; Schreibweise im XML lautet `Sphare`, nicht `Sphere` |
| R Cylinder | `R/Cylinder` | `-0.50` | erkannt |
| R Axis | `R/Axis` | `128` | erkannt |
| R PrismHorizontal | `R/PrismX` | `0.75` | erkannt |
| R PrismHorizontalBase | `R/PrismX/@base` | `out` | erkannt; Schreibweise für MEDISTAR-Ausgabe später auf `OUT` normalisieren |
| R PrismVertical | `R/PrismY` | `2.00` | erkannt |
| R PrismVerticalBase | `R/PrismY/@base` | `down` | erkannt; Schreibweise für MEDISTAR-Ausgabe später auf `DOWN` oder fachlich passende MEDISTAR-Notation normalisieren |
| L Sphere | `L/Sphare` | - | noch zu validieren; linkes Auge im vorliegenden XML-Fragment nicht enthalten |
| L Cylinder | `L/Cylinder` | - | noch zu validieren; linkes Auge im vorliegenden XML-Fragment nicht enthalten |
| L Axis | `L/Axis` | - | noch zu validieren; linkes Auge im vorliegenden XML-Fragment nicht enthalten |
| L PrismHorizontal | `L/PrismX` | - | noch zu validieren; linkes Auge im vorliegenden XML-Fragment nicht enthalten |
| L PrismHorizontalBase | `L/PrismX/@base` | - | noch zu validieren; linkes Auge im vorliegenden XML-Fragment nicht enthalten |
| L PrismVertical | `L/PrismY` | - | noch zu validieren; linkes Auge im vorliegenden XML-Fragment nicht enthalten |
| L PrismVerticalBase | `L/PrismY/@base` | - | noch zu validieren; linkes Auge im vorliegenden XML-Fragment nicht enthalten |
| PD | noch nicht erkannt | `59` im MEDISTAR-Beispiel | noch zu validieren; PD steht im MEDISTAR-Zielbild, aber nicht in `LM7.XML` |

Zusätzlich beobachtete Felder:

- `R/SE`
- `R/ADD`
- `R/ADD2`
- `R/NearSphare`
- `R/NearSphare2`
- `R/Prism`
- `R/PrismBase`
- `R/UVTransmittance`

Die Beziehung zwischen `Prism`/`PrismBase` und den getrennten Komponenten `PrismX`/`PrismY` muss fachlich noch validiert werden. Für das spätere LM7-Profil sind `PrismX` und `PrismY` besonders relevant, weil sie der MEDISTAR-Beispielausgabe mit horizontaler und vertikaler Prisma-Komponente nahekommen.

## 5. NIDEK NT530P

Gerätetyp: Non-Contact-Tonometer / Pachymeter

Dateiformate:

- XML
- JPG-Begleitdateien

Untersuchungsarten:

- Tonometrie
- Pachymetrie
- korrigierter Augeninnendruck
- optional Messbilder/Protokolldateien

Beispielausgabe MEDISTAR:

```text
Y  PR: 559 560 558 [559] µm
Y  PL: 559 560 [560] µm
Y  PR: Gemessen = 12.7 mmHg; Korrigiert = 12.3 mmHg; ...
P  R = 12 11 15 [12.7] // L = 14 13 15 [14.0] mmHg 14:51 / EV:{000000003B} NT-530P Messung
```

Zusatzdateien:

- JPG-Dateien können mitgeliefert werden
- XML kann auf Bilddateien verweisen, z. B. `PACHYImage`

Ableitung:

- mehrere Ergebniszeilen erforderlich
- Pachymetrie-Template
- Tonometrie-Template
- korrigierter IOP optional
- Attachment-/EV-Verknüpfung relevant
- optionale PDF-Erzeugung sinnvoll

Spätere Profilanforderung:

- `DeviceProfileDefinition` mit mehreren Untersuchungsarten
- `ExportProfileDefinition` mit mehreren `6228`-Regeln oder MEDISTAR-spezifischen Zielzeilen
- `AttachmentDefinition` / `DocumentExportRule`
- EV-Verweis als optionales Exporttemplate

## 5.1 NIDEK NT530P – erkannte SourcePaths und Attachments

Analysierte lokale Beispieldateien:

- `C:\Users\MarcK\Downloads\Geraeteanbindungen\NIDEK NT530P\Medistar Eintrag NT530P Tonometer.txt`
- `C:\Users\MarcK\Downloads\Geraeteanbindungen\NIDEK NT530P\RKT\TXT\NTP_              _20220404141453.xml`
- `C:\Users\MarcK\Downloads\Geraeteanbindungen\NIDEK NT530P\RKT\TXT\NTP_              _20220406071353.xml`
- `C:\Users\MarcK\Downloads\Geraeteanbindungen\NIDEK NT530P\RKT\JPG\NTP_              _20220406071353RP1.jpg`

Im lokalen Beispielordner wurden mehrere NT530P-XML-Dateien und zahlreiche JPG-Begleitdateien gefunden. Die XML-Dateien verwenden eine XML-Deklaration mit `encoding="UTF-16"` und eine einfache NIDEK-Struktur mit Root-Knoten `Data`.

Beobachtete Basisstruktur:

```xml
<Data>
  <Company>NIDEK</Company>
  <ModelName>NT-530P</ModelName>
  <Date>2022/04/06</Date>
  <Time>07:13:53</Time>
  <R>
    <NT>...</NT>
    <PACHY>...</PACHY>
  </R>
  <L>
    <NT>...</NT>
    <PACHY>...</PACHY>
  </L>
</Data>
```

Erkannte SourcePaths für Messwerte:

| Messwert | Erkannter SourcePath | Beispielwert | Status / Hinweis |
|---|---|---:|---|
| Tonometrie rechts, Einzelwert 1 | `Data/R/NT/NTList[@No='1']/mmHg` | `19` | erkannt |
| Tonometrie rechts, Einzelwert 2 | `Data/R/NT/NTList[@No='2']/mmHg` | `19` | erkannt |
| Tonometrie rechts, Einzelwert 3 | `Data/R/NT/NTList[@No='3']/mmHg` | `20` | erkannt |
| Tonometrie rechts, Mittelwert | `Data/R/NT/NTAverage/mmHg` | `19.3` | erkannt |
| Tonometrie links, Einzelwert 1 | `Data/L/NT/NTList[@No='1']/mmHg` | `28` | erkannt |
| Tonometrie links, Einzelwert 2 | `Data/L/NT/NTList[@No='2']/mmHg` | `29` | erkannt |
| Tonometrie links, Einzelwert 3 | `Data/L/NT/NTList[@No='3']/mmHg` | `28` | erkannt |
| Tonometrie links, Mittelwert | `Data/L/NT/NTAverage/mmHg` | `28.3` | erkannt |
| korrigierter IOP rechts, gemessen | `Data/R/NT/CorrectedIOP/Measured/mmHg` | `19.3` | erkannt |
| korrigierter IOP rechts, korrigiert | `Data/R/NT/CorrectedIOP/Corrected/mmHg` | `19.0` | erkannt |
| korrigierter IOP rechts, CCT | `Data/R/NT/CorrectedIOP/CCT` | `557um` | erkannt; Einheit ist im Wert enthalten |
| korrigierter IOP links, gemessen | `Data/L/NT/CorrectedIOP/Measured/mmHg` | `28.3` | erkannt |
| korrigierter IOP links, korrigiert | `Data/L/NT/CorrectedIOP/Corrected/mmHg` | `27.9` | erkannt |
| korrigierter IOP links, CCT | `Data/L/NT/CorrectedIOP/CCT` | `560um` | erkannt; Einheit ist im Wert enthalten |
| Pachymetrie rechts, Einzelwert 1 | `Data/R/PACHY/PACHYList[@No='1']/Thickness` | `564` | erkannt |
| Pachymetrie rechts, Einzelwert 2 | `Data/R/PACHY/PACHYList[@No='2']/Thickness` | `550` | erkannt |
| Pachymetrie rechts, Mittelwert | `Data/R/PACHY/PACHYAverage/Thickness` | `557` | erkannt |
| Pachymetrie links, Einzelwert 1 | `Data/L/PACHY/PACHYList[@No='1']/Thickness` | `560` | erkannt |
| Pachymetrie links, weitere Einzelwerte | `Data/L/PACHY/PACHYList[@No='2']/Thickness` usw. | - | noch zu validieren; im analysierten Beispiel links nur ein Einzelwert vorhanden |
| Pachymetrie links, Mittelwert | `Data/L/PACHY/PACHYAverage/Thickness` | `560` | erkannt |
| Messdatum | `Data/Date` | `2022/04/06` | erkannt |
| Messzeit | `Data/Time` | `07:13:53` | erkannt |

Zusätzlich beobachtete Felder:

- `Data/Company`
- `Data/ModelName`
- `Data/ROMVersion`
- `Data/Version`
- `Data/Patient/No.`
- `Data/Patient/ID`
- `Data/Comment`
- `Data/R/NT/NTList[@No='...']/kPa`
- `Data/L/NT/NTList[@No='...']/kPa`
- `Data/R/NT/NTAverage/kPa`
- `Data/L/NT/NTAverage/kPa`
- `Data/R/NT/CorrectedIOP/Param1`
- `Data/R/NT/CorrectedIOP/Param2`
- `Data/L/NT/CorrectedIOP/Param1`
- `Data/L/NT/CorrectedIOP/Param2`

Die kPa-Werte und Parameterfelder sind vorhanden, aber für die MEDISTAR-Beispielausgabe noch nicht als primäre Zielfelder validiert.

Erkannte Bild-/JPG-Verweise:

| Attachment | Erkannter SourcePath | Beispielwert | Status / Hinweis |
|---|---|---|---|
| Pachymetrie-Bild rechts | `Data/R/PACHY/PACHYImage` | `NTP_              _20220406071353RP1.jpg` | erkannt; Datei im lokalen JPG-Ordner vorhanden |
| Pachymetrie-Bild links | `Data/L/PACHY/PACHYImage` | `NTP_              _20220406071353LP1.jpg` | erkannt; in diesem Beispiel verweist XML auf die Datei, die lokale Datei fehlt jedoch |
| Pachymetrie-Bild bei Messfehler | `Data/R/PACHY/PACHYImage` | `NTP_              _20220404141453RP1.jpg` | erkannt; Beispiel enthält zusätzlich `Data/R/PACHY/PACHYAverage/Error = ALM` |

Die XML-Dateien verweisen direkt über den Textinhalt von `PACHYImage` auf JPG-Dateinamen. Die zugehörigen Dateien liegen im lokalen Beispielordner unter `RKT\JPG`. Der Dateiname enthält denselben Messzeitstempel wie die XML-Datei sowie eine Augen-/Bildkennung wie `RP1` oder `LP1`.

Bei einer lokalen Stichprobe wurden 65 XML-Dateien und 84 JPG-Dateien gefunden. Aus den XML-Dateien wurden 122 `PACHYImage`-Verweise erkannt; davon waren 84 als JPG-Datei lokal vorhanden und 38 nicht vorhanden. Daraus folgt für die spätere Attachment-Logik:

- XML-Verweise auf JPG-Dateien müssen gegen den konfigurierten Bildordner geprüft werden.
- Fehlende JPG-Dateien müssen protokolliert werden.
- Ob fehlende Bilder die Verarbeitung blockieren, muss pro Profil konfigurierbar sein.
- `PACHYImage` sollte als Attachment-SourcePath in einem späteren `AttachmentDefinition`- oder `DocumentExportRule`-Modell abbildbar sein.
- Die genaue MEDISTAR-EV-Verknüpfung bleibt weiterhin noch zu validieren.

## 6. TOPCON CL300

Gerätetyp: Lensmeter

Dateiformat:

- XML im Ophthalmology-/JOIA-Format
- Namespaces möglich

Beispielhafte SourcePath-Struktur:

```text
Ophthalmology/Common/Company = TOPCON
Ophthalmology/Common/ModelName = CL-300
Ophthalmology/Measure[@type='LM']/LM/R/Sphere
Ophthalmology/Measure[@type='LM']/LM/R/Cylinder
Ophthalmology/Measure[@type='LM']/LM/R/Axis
Ophthalmology/Measure[@type='LM']/PD/B/Distance
```

Untersuchungsarten:

- Lensmeterdaten
- Sphäre
- Zylinder
- Achse
- PD

Ableitung:

- XML-Parser muss Namespaces und Attribute handhaben
- eigenes Geräteprofil `TOPCON CL300`
- ähnliches MEDISTAR-Template wie Lensmeter, aber andere SourcePaths

## 6.1 TOPCON CL300 – erkannte SourcePaths und JOIA/XML-Besonderheiten

Analysierte lokale Beispieldatei:

- `C:\Users\MarcK\Downloads\Geraeteanbindungen\TOPCON CL300\M-Serial1521_20120101_000000_TOPCON_CL-300_00.xml`

Die Datei ist ein JOIA-/Ophthalmology-XML im Format `UTF-8`. Der Root-Knoten `Ophthalmology` hat keinen Default-Namespace, die fachlichen Unterknoten verwenden jedoch Namespace-Präfixe:

- `nsCommon = http://www.joia.or.jp/standardized/namespaces/Common`
- `nsLM = http://www.joia.or.jp/standardized/namespaces/LM`
- `xsi = http://www.w3.org/2001/XMLSchema-instance`

Beobachtete Basisstruktur:

```xml
<Ophthalmology xmlns:nsCommon="http://www.joia.or.jp/standardized/namespaces/Common"
               xmlns:nsLM="http://www.joia.or.jp/standardized/namespaces/LM">
  <nsCommon:Common>
    <nsCommon:Company>TOPCON</nsCommon:Company>
    <nsCommon:ModelName>CL-300</nsCommon:ModelName>
  </nsCommon:Common>
  <nsLM:Measure type="LM">
    <nsLM:LM>
      <nsLM:R>...</nsLM:R>
      <nsLM:L>...</nsLM:L>
    </nsLM:LM>
    <nsLM:PD>...</nsLM:PD>
  </nsLM:Measure>
</Ophthalmology>
```

Erkannte namespace-aware SourcePaths:

| Messwert | Namespace-aware SourcePath | Beispielwert | Status / Hinweis |
|---|---|---:|---|
| Company | `Ophthalmology/nsCommon:Common/nsCommon:Company` | `TOPCON` | erkannt |
| ModelName | `Ophthalmology/nsCommon:Common/nsCommon:ModelName` | `CL-300` | erkannt |
| Measure type LM | `Ophthalmology/nsLM:Measure/@type` | `LM` | erkannt |
| R Sphere | `Ophthalmology/nsLM:Measure[@type='LM']/nsLM:LM/nsLM:R/nsLM:Sphere` | `+3.75` | erkannt; XML-Wert enthält führende Leerzeichen |
| R Cylinder | `Ophthalmology/nsLM:Measure[@type='LM']/nsLM:LM/nsLM:R/nsLM:Cylinder` | `-3.50` | erkannt; XML-Wert enthält führende Leerzeichen |
| R Axis | `Ophthalmology/nsLM:Measure[@type='LM']/nsLM:LM/nsLM:R/nsLM:Axis` | `9` | erkannt; XML-Wert enthält führende Leerzeichen |
| L Sphere | `Ophthalmology/nsLM:Measure[@type='LM']/nsLM:LM/nsLM:L/nsLM:Sphere` | `+3.50` | erkannt; XML-Wert enthält führende Leerzeichen |
| L Cylinder | `Ophthalmology/nsLM:Measure[@type='LM']/nsLM:LM/nsLM:L/nsLM:Cylinder` | `-3.00` | erkannt; XML-Wert enthält führende Leerzeichen |
| L Axis | `Ophthalmology/nsLM:Measure[@type='LM']/nsLM:LM/nsLM:L/nsLM:Axis` | `178` | erkannt |
| PD Distance gesamt | `Ophthalmology/nsLM:Measure[@type='LM']/nsLM:PD/nsLM:B/nsLM:Distance` | `55.0` | erkannt |
| PD Distance rechts | `Ophthalmology/nsLM:Measure[@type='LM']/nsLM:PD/nsLM:R/nsLM:Distance` | `25.0` | erkannt |
| PD Distance links | `Ophthalmology/nsLM:Measure[@type='LM']/nsLM:PD/nsLM:L/nsLM:Distance` | `30.0` | erkannt |

Sonstige relevante Lensmeterwerte:

| Messwert | Namespace-aware SourcePath | Beispielwert | Status / Hinweis |
|---|---|---:|---|
| R Addition 1 | `Ophthalmology/nsLM:Measure[@type='LM']/nsLM:LM/nsLM:R/nsLM:Add1` | `+2.25` | erkannt |
| R Addition 2 | `Ophthalmology/nsLM:Measure[@type='LM']/nsLM:LM/nsLM:R/nsLM:Add2` | leer | erkannt; Nutzung noch zu validieren |
| R Prisma horizontal | `Ophthalmology/nsLM:Measure[@type='LM']/nsLM:LM/nsLM:R/nsLM:H` | `+0.25` | erkannt; Attribut `Prism="P"` |
| R Prisma vertikal | `Ophthalmology/nsLM:Measure[@type='LM']/nsLM:LM/nsLM:R/nsLM:V` | `-0.75` | erkannt; Attribut `Prism="P"` |
| L Addition 1 | `Ophthalmology/nsLM:Measure[@type='LM']/nsLM:LM/nsLM:L/nsLM:Add1` | `+2.25` | erkannt |
| L Addition 2 | `Ophthalmology/nsLM:Measure[@type='LM']/nsLM:LM/nsLM:L/nsLM:Add2` | leer | erkannt; Nutzung noch zu validieren |
| L Prisma horizontal | `Ophthalmology/nsLM:Measure[@type='LM']/nsLM:LM/nsLM:L/nsLM:H` | `-0.00` | erkannt; Attribut `Prism="P"` |
| L Prisma vertikal | `Ophthalmology/nsLM:Measure[@type='LM']/nsLM:LM/nsLM:L/nsLM:V` | `-1.00` | erkannt; Attribut `Prism="P"` |
| DiopterStep | `Ophthalmology/nsLM:Measure[@type='LM']/nsLM:DiopterStep` | `0.25` | erkannt |
| AxisStep | `Ophthalmology/nsLM:Measure[@type='LM']/nsLM:AxisStep` | `1` | erkannt |
| PrismStep | `Ophthalmology/nsLM:Measure[@type='LM']/nsLM:PrismStep` | `0.25` | erkannt |
| CylinderMode | `Ophthalmology/nsLM:Measure[@type='LM']/nsLM:CylinderMode` | `-` | erkannt |
| LensType | `Ophthalmology/nsLM:Measure[@type='LM']/nsLM:LensType` | `glass` | erkannt |

Für spätere anwendernahe Mapping-SourcePaths sollte der Parser Namespaces entweder profilabhängig auf bekannte Präfixe abbilden oder normalisieren. Eine gut lesbare normalisierte Darstellung wäre zum Beispiel:

```text
Ophthalmology/Common/Company
Ophthalmology/Common/ModelName
Ophthalmology/Measure[@type='LM']/LM/R/Sphere
Ophthalmology/Measure[@type='LM']/LM/R/Cylinder
Ophthalmology/Measure[@type='LM']/LM/R/Axis
Ophthalmology/Measure[@type='LM']/LM/L/Sphere
Ophthalmology/Measure[@type='LM']/LM/L/Cylinder
Ophthalmology/Measure[@type='LM']/LM/L/Axis
Ophthalmology/Measure[@type='LM']/PD/B/Distance
```

Wichtig für die spätere Parserlogik:

- Namespaces sind vorhanden und müssen berücksichtigt oder sauber normalisiert werden.
- `Measure` trägt das Attribut `type="LM"` und sollte darüber von anderen TOPCON-Messarten unterscheidbar sein.
- Werte können führende Leerzeichen enthalten und müssen vor Formatierung getrimmt werden.
- Einheiten stehen überwiegend als Attribute, z. B. `unit="D"`, `unit="deg"` und `unit="mm"`.
- Prismatische Werte verwenden `H` und `V` mit Attribut `Prism="P"`; die konkrete MEDISTAR-Basisnotation ist noch zu validieren.
- Für CL300 wurden in der analysierten Datei keine JPG-/PDF-Attachments gefunden.

## 7. TOPCON KR800

Gerätetyp: Autorefraktometer/Keratometer

Dateiformat:

- XML im Ophthalmology-/JOIA-Format

Untersuchungsarten:

- `REF` = Refraktion
- `KM` = Keratometrie
- `SBJ` = subjektive Refraktion / Visus / PD

Ableitung:

- ein Gerät liefert mehrere Untersuchungsarten in einer Datei
- Exportprofil muss auswählen können, welche Untersuchungen übernommen werden
- Refraktion und Keratometrie benötigen getrennte Ergebnis-Templates
- Mapping muss `Measure[@type='...']` unterscheiden können

Spätere Profilanforderung:

- mehrere Messwertgruppen
- mehrere Ergebniszeilen
- optionale Zusammenfassung oder getrennte Ausgabe

## 7.1 TOPCON KR800 – erkannte SourcePaths und Mehruntersuchungsstruktur

Analysierte lokale Beispieldatei:

- `C:\Users\MarcK\Downloads\Geraeteanbindungen\TOPCON KR800\M-Serial0426_20241126_145500_TOPCON_KR-800S_4871341.xml`

Die Datei ist ein JOIA-/Ophthalmology-XML mit `encoding="Shift-JIS"`. Der Root-Knoten `Ophthalmology` hat keinen Default-Namespace, die fachlichen Bereiche verwenden aber mehrere JOIA-Namespace-Präfixe:

- `nsCommon = http://www.joia.or.jp/standardized/namespaces/Common`
- `nsREF = http://www.joia.or.jp/standardized/namespaces/REF`
- `nsKM = http://www.joia.or.jp/standardized/namespaces/KM`
- `nsSBJ = http://www.joia.or.jp/standardized/namespaces/SBJ`
- `xsi = http://www.w3.org/2001/XMLSchema-instance`

Erkannte Untersuchungsarten in einer Datei:

| Untersuchungsart | XML-Struktur | Bedeutung | Status / Hinweis |
|---|---|---|---|
| `REF` | `Ophthalmology/nsREF:Measure[@type='REF']` | Autorefraktion | erkannt |
| `KM` | `Ophthalmology/nsKM:Measure[@type='KM']` | Keratometrie | erkannt |
| `SBJ` | `Ophthalmology/nsSBJ:Measure[@type='SBJ']` | subjektive Refraktion / VA / PD | erkannt |

Allgemeine Gerätedaten:

| Messwert | Namespace-aware SourcePath | Beispielwert | Status / Hinweis |
|---|---|---:|---|
| Company | `Ophthalmology/nsCommon:Common/nsCommon:Company` | `TOPCON` | erkannt |
| ModelName | `Ophthalmology/nsCommon:Common/nsCommon:ModelName` | `KR-800S` | erkannt |
| Messdatum | `Ophthalmology/nsCommon:Common/nsCommon:Date` | `2024-11-26` | erkannt |
| Messzeit | `Ophthalmology/nsCommon:Common/nsCommon:Time` | `14:55:00` | erkannt |

Erkannte REF-SourcePaths:

| Messwert | Namespace-aware SourcePath | Beispielwert | Status / Hinweis |
|---|---|---:|---|
| REF R Sphere | `Ophthalmology/nsREF:Measure[@type='REF']/nsREF:REF/nsREF:R/nsREF:Median/nsREF:Sphere` | `3.75` | erkannt |
| REF R Cylinder | `Ophthalmology/nsREF:Measure[@type='REF']/nsREF:REF/nsREF:R/nsREF:Median/nsREF:Cylinder` | `-4.00` | erkannt |
| REF R Axis | `Ophthalmology/nsREF:Measure[@type='REF']/nsREF:REF/nsREF:R/nsREF:Median/nsREF:Axis` | `13` | erkannt |
| REF R SE | `Ophthalmology/nsREF:Measure[@type='REF']/nsREF:REF/nsREF:R/nsREF:Median/nsREF:SE` | `1.75` | erkannt |
| REF L Sphere | `Ophthalmology/nsREF:Measure[@type='REF']/nsREF:REF/nsREF:L/nsREF:Median/nsREF:Sphere` | `3.75` | erkannt |
| REF L Cylinder | `Ophthalmology/nsREF:Measure[@type='REF']/nsREF:REF/nsREF:L/nsREF:Median/nsREF:Cylinder` | `-2.50` | erkannt |
| REF L Axis | `Ophthalmology/nsREF:Measure[@type='REF']/nsREF:REF/nsREF:L/nsREF:Median/nsREF:Axis` | `173` | erkannt |
| REF L SE | `Ophthalmology/nsREF:Measure[@type='REF']/nsREF:REF/nsREF:L/nsREF:Median/nsREF:SE` | `2.50` | erkannt |
| REF PD Distance | `Ophthalmology/nsREF:Measure[@type='REF']/nsREF:PD/nsREF:Distance` | `66.00` | erkannt |
| REF PD Near | `Ophthalmology/nsREF:Measure[@type='REF']/nsREF:PD/nsREF:Near` | `66.00` | erkannt |

Neben `Median` sind einzelne Messlisten vorhanden, z. B. `nsREF:List[@No='1']` bis `nsREF:List[@No='4']`. Im Beispiel enthält `R/List[@No='1']` einen Fehler `NO CENTER`; daher sollte ein späteres Profil für Standardausgaben bevorzugt die `Median`-Werte verwenden und Einzellisten optional auswerten.

Erkannte KM-SourcePaths:

| Messwert | Namespace-aware SourcePath | Beispielwert | Status / Hinweis |
|---|---|---:|---|
| KM R K1 Radius | `Ophthalmology/nsKM:Measure[@type='KM']/nsKM:KM/nsKM:R/nsKM:Median/nsKM:R1/nsKM:Radius` | `8.48` | erkannt |
| KM R K1 Power | `Ophthalmology/nsKM:Measure[@type='KM']/nsKM:KM/nsKM:R/nsKM:Median/nsKM:R1/nsKM:Power` | `39.75` | erkannt |
| KM R K1 Axis | `Ophthalmology/nsKM:Measure[@type='KM']/nsKM:KM/nsKM:R/nsKM:Median/nsKM:R1/nsKM:Axis` | `11` | erkannt |
| KM R K2 Radius | `Ophthalmology/nsKM:Measure[@type='KM']/nsKM:KM/nsKM:R/nsKM:Median/nsKM:R2/nsKM:Radius` | `7.79` | erkannt |
| KM R K2 Power | `Ophthalmology/nsKM:Measure[@type='KM']/nsKM:KM/nsKM:R/nsKM:Median/nsKM:R2/nsKM:Power` | `43.50` | erkannt |
| KM R K2 Axis | `Ophthalmology/nsKM:Measure[@type='KM']/nsKM:KM/nsKM:R/nsKM:Median/nsKM:R2/nsKM:Axis` | `101` | erkannt |
| KM R Average Radius | `Ophthalmology/nsKM:Measure[@type='KM']/nsKM:KM/nsKM:R/nsKM:Median/nsKM:Average/nsKM:Radius` | `8.14` | erkannt |
| KM R Average Power | `Ophthalmology/nsKM:Measure[@type='KM']/nsKM:KM/nsKM:R/nsKM:Median/nsKM:Average/nsKM:Power` | `41.75` | erkannt |
| KM R Cylinder Power | `Ophthalmology/nsKM:Measure[@type='KM']/nsKM:KM/nsKM:R/nsKM:Median/nsKM:Cylinder/nsKM:Power` | `-3.75` | erkannt |
| KM R Cylinder Axis | `Ophthalmology/nsKM:Measure[@type='KM']/nsKM:KM/nsKM:R/nsKM:Median/nsKM:Cylinder/nsKM:Axis` | `11` | erkannt |
| KM L K1 Radius | `Ophthalmology/nsKM:Measure[@type='KM']/nsKM:KM/nsKM:L/nsKM:Median/nsKM:R1/nsKM:Radius` | `8.35` | erkannt |
| KM L K1 Power | `Ophthalmology/nsKM:Measure[@type='KM']/nsKM:KM/nsKM:L/nsKM:Median/nsKM:R1/nsKM:Power` | `40.50` | erkannt |
| KM L K1 Axis | `Ophthalmology/nsKM:Measure[@type='KM']/nsKM:KM/nsKM:L/nsKM:Median/nsKM:R1/nsKM:Axis` | `171` | erkannt |
| KM L K2 Radius | `Ophthalmology/nsKM:Measure[@type='KM']/nsKM:KM/nsKM:L/nsKM:Median/nsKM:R2/nsKM:Radius` | `7.87` | erkannt |
| KM L K2 Power | `Ophthalmology/nsKM:Measure[@type='KM']/nsKM:KM/nsKM:L/nsKM:Median/nsKM:R2/nsKM:Power` | `43.00` | erkannt |
| KM L K2 Axis | `Ophthalmology/nsKM:Measure[@type='KM']/nsKM:KM/nsKM:L/nsKM:Median/nsKM:R2/nsKM:Axis` | `81` | erkannt |
| KM L Average Radius | `Ophthalmology/nsKM:Measure[@type='KM']/nsKM:KM/nsKM:L/nsKM:Median/nsKM:Average/nsKM:Radius` | `8.11` | erkannt |
| KM L Average Power | `Ophthalmology/nsKM:Measure[@type='KM']/nsKM:KM/nsKM:L/nsKM:Median/nsKM:Average/nsKM:Power` | `41.75` | erkannt |
| KM L Cylinder Power | `Ophthalmology/nsKM:Measure[@type='KM']/nsKM:KM/nsKM:L/nsKM:Median/nsKM:Cylinder/nsKM:Power` | `-2.50` | erkannt |
| KM L Cylinder Axis | `Ophthalmology/nsKM:Measure[@type='KM']/nsKM:KM/nsKM:L/nsKM:Median/nsKM:Cylinder/nsKM:Axis` | `171` | erkannt |

Die Keratometrie nutzt in der JOIA-Struktur `R1` und `R2`. Diese entsprechen fachlich den K-Werten `K1` und `K2`; die Bezeichnung im späteren Profil sollte für Anwender vermutlich `K1`/`K2` verwenden, technisch aber auf `R1`/`R2` im XML abbilden.

Erkannte SBJ-SourcePaths:

| Messwert | Namespace-aware SourcePath | Beispielwert | Status / Hinweis |
|---|---|---:|---|
| SBJ Type 1 Name | `Ophthalmology/nsSBJ:Measure[@type='SBJ']/nsSBJ:RefractionTest/nsSBJ:Type[@No='1']/nsSBJ:TypeName` | `Full Correction` | erkannt |
| SBJ Type 2 Name | `Ophthalmology/nsSBJ:Measure[@type='SBJ']/nsSBJ:RefractionTest/nsSBJ:Type[@No='2']/nsSBJ:TypeName` | `Unaided Data` | erkannt |
| SBJ ExamDistance 1 | `Ophthalmology/nsSBJ:Measure[@type='SBJ']/nsSBJ:RefractionTest/nsSBJ:Type[@No='1']/nsSBJ:ExamDistance[@No='1']/nsSBJ:Distance` | `500.000` | erkannt |
| SBJ R Sphere | `Ophthalmology/nsSBJ:Measure[@type='SBJ']/nsSBJ:RefractionTest/nsSBJ:Type[@No='1']/nsSBJ:ExamDistance[@No='1']/nsSBJ:RefractionData/nsSBJ:R/nsSBJ:Sph` | `3.75` | erkannt |
| SBJ R Cylinder | `Ophthalmology/nsSBJ:Measure[@type='SBJ']/nsSBJ:RefractionTest/nsSBJ:Type[@No='1']/nsSBJ:ExamDistance[@No='1']/nsSBJ:RefractionData/nsSBJ:R/nsSBJ:Cyl` | `-4.00` | erkannt |
| SBJ R Axis | `Ophthalmology/nsSBJ:Measure[@type='SBJ']/nsSBJ:RefractionTest/nsSBJ:Type[@No='1']/nsSBJ:ExamDistance[@No='1']/nsSBJ:RefractionData/nsSBJ:R/nsSBJ:Axis` | `13` | erkannt |
| SBJ L Sphere | `Ophthalmology/nsSBJ:Measure[@type='SBJ']/nsSBJ:RefractionTest/nsSBJ:Type[@No='1']/nsSBJ:ExamDistance[@No='1']/nsSBJ:RefractionData/nsSBJ:L/nsSBJ:Sph` | `3.75` | erkannt |
| SBJ L Cylinder | `Ophthalmology/nsSBJ:Measure[@type='SBJ']/nsSBJ:RefractionTest/nsSBJ:Type[@No='1']/nsSBJ:ExamDistance[@No='1']/nsSBJ:RefractionData/nsSBJ:L/nsSBJ:Cyl` | `-2.50` | erkannt |
| SBJ L Axis | `Ophthalmology/nsSBJ:Measure[@type='SBJ']/nsSBJ:RefractionTest/nsSBJ:Type[@No='1']/nsSBJ:ExamDistance[@No='1']/nsSBJ:RefractionData/nsSBJ:L/nsSBJ:Axis` | `173` | erkannt |
| SBJ VA rechts | `Ophthalmology/nsSBJ:Measure[@type='SBJ']/nsSBJ:RefractionTest/nsSBJ:Type[@No='1']/nsSBJ:ExamDistance[@No='1']/nsSBJ:VA/nsSBJ:R` | `0.6` | erkannt |
| SBJ VA links | `Ophthalmology/nsSBJ:Measure[@type='SBJ']/nsSBJ:RefractionTest/nsSBJ:Type[@No='1']/nsSBJ:ExamDistance[@No='1']/nsSBJ:VA/nsSBJ:L` | `1.0` | erkannt |
| SBJ VA binokular | `Ophthalmology/nsSBJ:Measure[@type='SBJ']/nsSBJ:RefractionTest/nsSBJ:Type[@No='1']/nsSBJ:ExamDistance[@No='1']/nsSBJ:VA/nsSBJ:B` | leer | erkannt; Nutzung noch zu validieren |
| SBJ PD rechts | `Ophthalmology/nsSBJ:Measure[@type='SBJ']/nsSBJ:RefractionTest/nsSBJ:Type[@No='1']/nsSBJ:ExamDistance[@No='1']/nsSBJ:PD/nsSBJ:R` | leer | erkannt; Nutzung noch zu validieren |
| SBJ PD links | `Ophthalmology/nsSBJ:Measure[@type='SBJ']/nsSBJ:RefractionTest/nsSBJ:Type[@No='1']/nsSBJ:ExamDistance[@No='1']/nsSBJ:PD/nsSBJ:L` | leer | erkannt; Nutzung noch zu validieren |
| SBJ PD binokular | `Ophthalmology/nsSBJ:Measure[@type='SBJ']/nsSBJ:RefractionTest/nsSBJ:Type[@No='1']/nsSBJ:ExamDistance[@No='1']/nsSBJ:PD/nsSBJ:B` | `66.00` | erkannt |

Im Beispiel sind vier SBJ-ExamDistance-Kombinationen vorhanden:

- `Type No="1"` / `Full Correction`, `ExamDistance No="1"` = `500.000 cm`, mit R/L subjektiven Werten, VA rechts/links und PD binokular.
- `Type No="1"` / `Full Correction`, `ExamDistance No="2"` = `33.000 cm`, mit Nahwerten für R/L, aber ohne VA.
- `Type No="2"` / `Unaided Data`, `ExamDistance No="1"` und `No="2"`, überwiegend leer, aber mit PD binokular.

Für spätere anwendernahe Mapping-SourcePaths sollte analog zu CL300 eine normalisierte Darstellung verwendet werden, z. B.:

```text
Ophthalmology/Common/Company
Ophthalmology/Common/ModelName
Ophthalmology/Measure[@type='REF']/REF/R/Median/Sphere
Ophthalmology/Measure[@type='REF']/REF/L/Median/Sphere
Ophthalmology/Measure[@type='KM']/KM/R/Median/R1/Power
Ophthalmology/Measure[@type='KM']/KM/L/Median/R2/Power
Ophthalmology/Measure[@type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/VA/R
Ophthalmology/Measure[@type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/PD/B
```

Wichtig für die spätere Parser- und Profil-Logik:

- KR800 liefert mehrere Untersuchungsarten in einer einzigen Datei.
- Exportprofile müssen pro Untersuchungsart entscheiden können, ob `REF`, `KM`, `SBJ` oder nur einzelne Gruppen übernommen werden.
- `Measure[@type='...']` ist zwingend relevant, weil REF/KM/SBJ parallele Strukturen mit unterschiedlichen Namespaces nutzen.
- `Median`-Werte sind für Standardausgaben vermutlich stabiler als Einzelmesslisten.
- SBJ enthält mehrere `Type`- und `ExamDistance`-Varianten; die fachliche Auswahl ist noch zu validieren.
- `Shift-JIS` muss als Eingabe-Encoding unterstützt oder zuverlässig erkannt werden.
- Für KR800 wurden in der analysierten Datei keine JPG-/PDF-Attachments gefunden.

## 8. TOPCON TRK2P

Gerätetyp: Tonometer / Refraktions-Keratometer je nach Dateninhalt

Dateiformat:

- XML im Ophthalmology-/JOIA-Format

Untersuchungsarten:

- `TM` = Tonometrie
- `CCT` = zentrale Hornhautdicke / Pachymetrie

Ableitung:

- Tonometrie-Template
- Pachymetrie-Template
- mögliche Kombination mit korrigiertem IOP
- XML-Attribute und Namespaces beachten

## 8.1 TOPCON TRK2P – erkannte SourcePaths für Tonometrie und CCT

Analysierte lokale Beispieldatei:

- `C:\Users\MarcK\Downloads\Geraeteanbindungen\TOPCON TRK2P\M-Serial1165_20241126_225512_TOPCON_TRK-2P_5284298.xml`

Die Datei ist ein JOIA-/Ophthalmology-XML mit `encoding="UTF-8"`. Der Root-Knoten `Ophthalmology` hat keinen Default-Namespace. Die Common-Daten und die Tonometrie-Hauptmesswerte verwenden Namespace-Präfixe, während zusätzliche Geräte-/CCT-Blöcke innerhalb von `nsTM:Measure` unqualifizierte Elemente verwenden.

Namespaces:

- `nsCommon = http://www.joia.or.jp/standardized/namespaces/Common`
- `nsTM = http://www.joia.or.jp/standardized/namespaces/TM`
- `xsi = http://www.w3.org/2001/XMLSchema-instance`

Erkannte Untersuchungsarten in der analysierten Datei:

| Untersuchungsart | XML-Struktur | Bedeutung | Status / Hinweis |
|---|---|---|---|
| `TM` | `Ophthalmology/nsTM:Measure[@type='TM']` | Tonometrie | erkannt |
| `CCT` | `Ophthalmology/nsTM:Measure/CCT` | zentrale Hornhautdicke / Pachymetrie | erkannt; unqualifizierter Unterknoten innerhalb von `nsTM:Measure` |
| `REF` | `Ophthalmology/nsREF:Measure[@type='REF']` | Refraktion | nicht vorhanden; noch zu validieren für andere TRK2P-Dateien |
| `KM` | `Ophthalmology/nsKM:Measure[@type='KM']` | Keratometrie | nicht vorhanden; noch zu validieren für andere TRK2P-Dateien |

Allgemeine Gerätedaten:

| Messwert | Namespace-aware SourcePath | Beispielwert | Status / Hinweis |
|---|---|---:|---|
| Company | `Ophthalmology/nsCommon:Common/nsCommon:Company` | `TOPCON` | erkannt |
| ModelName | `Ophthalmology/nsCommon:Common/nsCommon:ModelName` | `TRK-2P` | erkannt |
| Messdatum | `Ophthalmology/nsCommon:Common/nsCommon:Date` | `2024-11-26` | erkannt |
| Messzeit | `Ophthalmology/nsCommon:Common/nsCommon:Time` | `22:55:12` | erkannt |

Erkannte TM-SourcePaths:

| Messwert | Namespace-aware SourcePath | Beispielwert | Status / Hinweis |
|---|---|---:|---|
| TM R IOP Einzelwert 1 | `Ophthalmology/nsTM:Measure[@type='TM']/nsTM:TM/nsTM:R/nsTM:List[@No='1']/nsTM:IOP_mmHg` | `15.0` | erkannt |
| TM R IOP Einzelwert 2 | `Ophthalmology/nsTM:Measure[@type='TM']/nsTM:TM/nsTM:R/nsTM:List[@No='2']/nsTM:IOP_mmHg` | `15.0` | erkannt |
| TM R IOP Einzelwert 3 | `Ophthalmology/nsTM:Measure[@type='TM']/nsTM:TM/nsTM:R/nsTM:List[@No='3']/nsTM:IOP_mmHg` | `15.0` | erkannt |
| TM R IOP Mittelwert | `Ophthalmology/nsTM:Measure[@type='TM']/nsTM:TM/nsTM:R/nsTM:Average/nsTM:IOP_mmHg` | `15.0` | erkannt |
| TM L IOP Einzelwert 1 | `Ophthalmology/nsTM:Measure[@type='TM']/nsTM:TM/nsTM:L/nsTM:List[@No='1']/nsTM:IOP_mmHg` | `13.0` | erkannt |
| TM L IOP Einzelwert 2 | `Ophthalmology/nsTM:Measure[@type='TM']/nsTM:TM/nsTM:L/nsTM:List[@No='2']/nsTM:IOP_mmHg` | `12.0` | erkannt |
| TM L IOP Einzelwert 3 | `Ophthalmology/nsTM:Measure[@type='TM']/nsTM:TM/nsTM:L/nsTM:List[@No='3']/nsTM:IOP_mmHg` | `14.0` | erkannt |
| TM L IOP Mittelwert | `Ophthalmology/nsTM:Measure[@type='TM']/nsTM:TM/nsTM:L/nsTM:Average/nsTM:IOP_mmHg` | `13.0` | erkannt |
| TM R IOP hPa | `Ophthalmology/nsTM:Measure[@type='TM']/nsTM:TM/nsTM:R/nsTM:List[@No='1']/nsTM:IOP_Pa` | leer | erkannt; Nutzung noch zu validieren |
| TM L IOP hPa | `Ophthalmology/nsTM:Measure[@type='TM']/nsTM:TM/nsTM:L/nsTM:List[@No='1']/nsTM:IOP_Pa` | leer | erkannt; Nutzung noch zu validieren |

Zusätzliche unqualifizierte TM-Geräteparameter:

| Messwert | SourcePath | Beispielwert | Status / Hinweis |
|---|---|---:|---|
| Messreihenfolge | `Ophthalmology/nsTM:Measure/TM/RL_Order` | `R` | erkannt |
| R Pressure Range | `Ophthalmology/nsTM:Measure/TM/R/List[@No='1']/Pressure_Range` | `30` | erkannt; technischer Parameter |
| L Pressure Range | `Ophthalmology/nsTM:Measure/TM/L/List[@No='1']/Pressure_Range` | `30` | erkannt; technischer Parameter |
| Alignment Mode | `Ophthalmology/nsTM:Measure/TM/R/List[@No='1']/Alignment_Mode` | `Auto` | erkannt; technische Relevanz noch zu validieren |
| IOL Mode | `Ophthalmology/nsTM:Measure/TM/R/List[@No='1']/IOL_Mode` | `Off` | erkannt; technische Relevanz noch zu validieren |

Erkannte CCT-/Pachymetrie-SourcePaths:

| Messwert | SourcePath | Beispielwert | Status / Hinweis |
|---|---|---:|---|
| CCT Messreihenfolge | `Ophthalmology/nsTM:Measure/CCT/RL_Order` | `R` | erkannt |
| R CCT Liste 1 Fehler | `Ophthalmology/nsTM:Measure/CCT/R/List[@No='1']/Error` | `ERROR` | erkannt |
| R CCT Liste 2 Fehler | `Ophthalmology/nsTM:Measure/CCT/R/List[@No='2']/Error` | `ERROR` | erkannt |
| R CCT Liste 3 | `Ophthalmology/nsTM:Measure/CCT/R/List[@No='3']/CCT_mm` | `0.511` | erkannt; Wert ist in mm |
| R CCT Liste 4 | `Ophthalmology/nsTM:Measure/CCT/R/List[@No='4']/CCT_mm` | `0.509` | erkannt; Wert ist in mm |
| R CCT Mittelwert | noch nicht erkannt | - | noch zu validieren; kein expliziter Average-Knoten im analysierten CCT-Block |
| L CCT Liste 1 | `Ophthalmology/nsTM:Measure/CCT/L/List[@No='1']/CCT_mm` | `0.516` | erkannt; Wert ist in mm |
| L CCT Liste 2 | `Ophthalmology/nsTM:Measure/CCT/L/List[@No='2']/CCT_mm` | `0.518` | erkannt; Wert ist in mm |
| L CCT Liste 3 | `Ophthalmology/nsTM:Measure/CCT/L/List[@No='3']/CCT_mm` | `0.518` | erkannt; Wert ist in mm |
| L CCT Mittelwert | noch nicht erkannt | - | noch zu validieren; kein expliziter Average-Knoten im analysierten CCT-Block |
| korrigierter IOP rechts | noch nicht erkannt | - | noch zu validieren; kein `CorrectedIOP`-Knoten in der analysierten Datei |
| korrigierter IOP links | noch nicht erkannt | - | noch zu validieren; kein `CorrectedIOP`-Knoten in der analysierten Datei |

Für spätere anwendernahe Mapping-SourcePaths sollte analog zu CL300/KR800 eine normalisierte Darstellung verwendet werden, z. B.:

```text
Ophthalmology/Common/Company
Ophthalmology/Common/ModelName
Ophthalmology/Measure[@type='TM']/TM/R/List[@No='1']/IOP_mmHg
Ophthalmology/Measure[@type='TM']/TM/R/Average/IOP_mmHg
Ophthalmology/Measure[@type='TM']/TM/L/List[@No='1']/IOP_mmHg
Ophthalmology/Measure[@type='TM']/TM/L/Average/IOP_mmHg
Ophthalmology/Measure[@type='TM']/CCT/R/List[@No='3']/CCT_mm
Ophthalmology/Measure[@type='TM']/CCT/L/List[@No='1']/CCT_mm
```

Wichtig für die spätere Parser- und Profil-Logik:

- `TM` ist der einzige JOIA-Measure-Typ in der analysierten Datei.
- `CCT` ist kein eigener JOIA-`Measure`-Block, sondern ein unqualifizierter Unterknoten innerhalb von `nsTM:Measure`.
- Parser müssen gemischte namespace-qualifizierte und unqualifizierte Elemente innerhalb einer Datei unterstützen.
- CCT-Werte sind in `mm` angegeben, während MEDISTAR-Ausgaben üblicherweise Pachymetrie in `µm` darstellen können; eine spätere Formatfunktion/Umrechnung ist fachlich zu prüfen.
- Rechte CCT-Messlisten können Fehler statt Werte enthalten; Ergebnislogik muss Fehlerlisten überspringen oder sichtbar machen können.
- Ein expliziter korrigierter IOP wurde in der analysierten TRK2P-Datei nicht gefunden.
- Für TRK2P wurden in der analysierten Datei keine JPG-/PDF-Attachments gefunden.

## 9. Allgemeine Erkenntnisse aus den Beispieldaten

1. Jedes Gerät liefert andere Dateistrukturen.
2. XML ist nicht gleich XML; NIDEK und TOPCON unterscheiden sich stark.
3. TOPCON-/JOIA-Dateien können Namespaces und Measure-Type-Attribute nutzen.
4. Ein Gerät kann mehrere Untersuchungsarten in einer Datei liefern.
5. MEDISTAR erwartet oft fertige Ergebniszeilen statt einzelner technischer Wertefelder.
6. Feld `6228` ist für Ergebnistext besonders relevant.
7. Feld `8402` Untersuchungsart kommt aus dem AIS und muss durchgereicht werden.
8. Feld `8000=6310` ist für den MEDISTAR-XDT-Import relevant.
9. Zusatzdateien wie JPG/PDF müssen optional unterstützt werden.
10. EV-Verweise sind ein eigener Exportmechanismus.
11. Optional selbst erzeugte PDF-Protokolle können einen deutlichen Mehrwert bieten.

## 10. Anforderungen an spätere Parser

- XML-Parser muss Attribute unterstützen.
- XML-Parser muss Namespaces unterstützen oder normalisieren.
- Parser muss mehrere Messwertgruppen erkennen.
- Parser muss wiederholte Elemente eindeutig machen.
- Parser muss Begleitdateien erkennen können.
- Parsermodus muss pro Geräteprofil konfigurierbar sein:
  - XML
  - XPath
  - Regex
  - CSV
  - Text
  - GDT/XDT
  - gerätespezifische Sonderparser

## 11. Anforderungen an spätere Exportprofile

- mehrere Ergebniszeilen möglich
- gleicher Zielfeldcode mehrfach möglich, z. B. `6228`
- mehrere Untersuchungsarten pro Datei
- Ausgabe als Ergebnistext, Einzelwerte, Kategorie/Wert-Paar oder Dokumentverweis
- EV-Zeile optional
- PDF-Dokument optional
- Ausgabe-Syntax frei editierbar
- Formatfunktionen je Platzhalter nutzbar:
  - Raw
  - Diopter
  - Axis
  - Pd
  - Iop
  - Pachy
  - Prism
  - Keratometry

## 12. Priorisierung

Empfohlene Reihenfolge für spätere Umsetzung:

1. ARK1S stabil halten.
2. LM7 als zweites Geräteprofil vorbereiten, weil es ähnliche refraktive Ergebniszeilen nutzt.
3. NT530P untersuchen, weil dort Tonometrie, Pachymetrie und Attachments/EV relevant werden.
4. TOPCON CL300 als erstes JOIA-/Namespace-XML-Profil vorbereiten.
5. TOPCON KR800 wegen Mehruntersuchungsdateien.
6. TOPCON TRK2P wegen Tonometrie/CCT.

## 13. Abgrenzung

Dieses Dokument ist eine fachliche Auswertung der Beispieldaten.

Es implementiert keine neuen Geräteprofile.

Es verändert nicht den bestehenden MEDISTAR/NIDEK-ARK1S-Prototyp.

Neue Profile sollen später schrittweise und testgetrieben umgesetzt werden.
