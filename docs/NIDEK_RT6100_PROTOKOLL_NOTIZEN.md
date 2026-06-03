# NIDEK RT-6100 Protokollnotizen

Stand: 2026-06-03

Diese Notizen fassen die aktuelle Auswertung der Herstellerunterlage `RT-6100(RT015)_IntME_RT015-P9A2-A9.pdf`, der frueheren OCR-Beispieldatei `NIDEK  RT6100.XML` und der am 2026-06-03 uebernommenen echten NIDEK-XML-Dateien zusammen. Sie dienen als technische Grundlage fuer den XDTBox-Kandidaten `MEDISTAR + NIDEK RT-6100`.

## Kommunikationsweg

Der RT-6100 wird fuer XDTBox als bidirektionaler LAN-/CIFS-/Shared-Folder-Phoropter modelliert. Die Anbindung bleibt `NetworkLan`, also der bisherige Datei-/UNC-Workflow. RS232 ist fuer dieses Geraet in XDTBox nicht die konfigurierte Quelle.

Die Herstellerunterlage beschreibt fuer die direkte XML-Eingabe ueber MEM-200 den Zielordner `DIRECT_RT_xx\TXT`. Je nach RT-/MEM-200-Konstellation kommen insbesondere diese Unterordner vor:

- `DIRECT_RT_0A\TXT`
- `DIRECT_RT_1A\TXT`
- `DIRECT_RT_1B\TXT`
- `DIRECT_RT_2A\TXT`
- `DIRECT_RT_2B\TXT`
- `DIRECT_RT_3A\TXT`
- `DIRECT_RT_3B\TXT`

XDTBox legt diesen Pfad nicht hart fest. Der Ausgabeordner an das Geraet wird im Schnittstellenprofil im Bereich `Ausgabe an Geraet` konfiguriert.

## XML-Struktur

Die RT-6100-Daten verwenden `Ophthalmology` als Root. Fuer die Erkennung des Rueckgabeparsers muessen zusammen passen:

- `Common/Company = NIDEK`
- `Common/ModelName = RT-6100`
- `Common/Version` beginnt tolerant mit `NIDEK_RT`, auch wenn Leerzeichen statt Unterstrich vorkommen
- `Measure Type="RT"`

Die relevanten Phoropterwerte liegen unter:

```xml
<Measure Type="RT">
  <Phoropter>
    <Corrected CorrectionType="LM_Base|REF_Base|Full|Best" Vision="Distant" Situation="Standard">
      <R>...</R>
      <L>...</L>
      <B>...</B>
    </Corrected>
  </Phoropter>
</Measure>
```

R/L koennen Sphere, Cylinder, Axis, ADD, ADD2, PD, PrismX, PrismY und Visusfelder enthalten. B kann insbesondere PD und Visus-/Vergenzinformationen enthalten. Fuer die MEDISTAR-Refraktionsausgabe werden aktuell nur die sicher verstandenen Refraktionswerte verwendet; Prisma und Visus werden nicht blind in `6227`/`6228` ausgegeben.

## Eingabe an RT-6100

Die Herstellerunterlage beschreibt fuer Computer-Input sicher LM- und AR-Daten. Deshalb erzeugt XDTBox konservativ:

- MEDISTAR `V0`/Lensmeter -> `CorrectionType="LM_Base"`
- MEDISTAR `V1`/Autorefraktion -> `CorrectionType="REF_Base"`
- echte NIDEK LM-7P-XML (`Measure type="LM"`) -> `LM_Base`
- echte NIDEK ARK-1s-XML (`ARMedian`) -> `REF_Base`

Andere historische MEDISTAR-Praefixe werden nicht als RT-6100-Eingabe geschrieben, solange die praktische Geraeteabnahme dafuer fehlt.

Die echte LM-Datei `LM__20251128120038_05D67D.xml` bestaetigt die NIDEK-Schreibweise `Sphare`, `Cylinder`, `Axis` und ein kleingeschriebenes `Measure type="LM"`. Die Datei enthaelt Prismenwerte; diese werden fuer RT-6100-Input aktuell nicht in medizinische Importwerte uebersetzt, sondern als Warnung gefuehrt.

Die echte ARK-Datei `ARK_              _20150528151629.xml` bestaetigt ARMedian als sichere Quelle fuer `REF_Base`. SR-, KM-, CS-, PS-, AC-, RI- und Bilddaten werden nicht als RT-6100-Input exportiert. `VD` und `WorkingDistance` werden aus der ARK-Datei uebernommen, wenn sie vorhanden sind.

Der Dateiname wird stabil aus dem Schnittstellenprofil erzeugt. Default:

```text
RTImport_{PatientNumber}_{yyyyMMdd}_{HHmmss}.xml
```

Die Datei wird als UTF-16-XML geschrieben. Die erzeugte Struktur enthaelt `Common` mit `MachineNo`, `ROMVersion`, vollstaendigem Patientenknoten, ISO-Datum, `Measure Type="RT"`, `Phoropter`, `DiopterStep`, `AxisStep`, `CylinderMode` und `Corrected`-Bloecken mit Einheiten an Sphere/Cylinder/Axis/ADD/PD/VD/WorkingDistance. Fehlende medizinische Werte werden weggelassen oder als leere, laut Herstellerstruktur notwendige Tags belassen; XDTBox erfindet keine Werte.

## Rueckgabe an MEDISTAR

Die RT-6100-Rueckgabe wird analog zum CV5000-Phoropter fachlich getrennt:

- `CorrectionType="Best"` -> finaler Verordnungswert ueber `6228`
- `CorrectionType="Full"` -> Maximalwert/Vollkorrektion ueber `6227`
- nur `Vision="Distant"` und `Situation="Standard"` werden als MEDISTAR-Hauptausgabe verwendet

Nicht erzeugt werden:

- `6330`
- kuenstliche Trennzeilen
- kuenstliche `6228` aus `Full`
- kuenstliche `6227` aus `Best`

`8402` kommt weiterhin aus AIS/MEDISTAR.

Die echte RT-Rueckgabedatei `RT__20260602_095132__.xml` ist als positive Fixture uebernommen. Sie enthaelt `LM_Base`, `REF_Base`, `Full` und `Best`; `Best` und `Full` werden mit den realen R-/L-Werten testseitig auf `6228` beziehungsweise `6227` geprueft.

## Befund zur bereitgestellten OCR-XML

Die Datei `NIDEK  RT6100.XML` ist nicht als echte wohlgeformte Hersteller-/Geraeteausgabe zu behandeln:

- Die XML-Deklaration nennt `UTF-16`, die Datei besitzt aber keinen UTF-16-BOM und wirkt wie UTF-8/ASCII.
- Die Struktur ist unvollstaendig beziehungsweise fehlerhaft; es fehlen schliessende Elemente fuer verschachtelte Bereiche.

XDTBox nutzt diese Datei deshalb nur als Diagnosefall fuer eine klare Fehlermeldung bei defektem XML. Positive Parser- und Exporttests nutzen jetzt echte wohlgeformte NIDEK-XML-Fixtures plus synthetische Grenzfallfixtures ohne echte Patientendaten.

## Offene Praxisabnahme

Noch praktisch zu pruefen:

- MEM-200-Zielordner fuer die konkrete Praxisinstallation.
- Akzeptanz des Dateinamensschemas durch RT-6100/MEM-200.
- Einlesen der erzeugten `LM_Base`-/`REF_Base`-XML am Geraet.
- Echte wohlgeformte RT-6100-Rueckgabedatei nach Untersuchung praktisch am Geraet/MEDISTAR pruefen; eine echte Datei ist testseitig vorhanden.
- MEDISTAR-Import der daraus erzeugten `6228`-/`6227`-XDT-Datei.
