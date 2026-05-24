# E2E-Testprotokoll: MEDISTAR + TOPCON CV-5000 / CV-5000S

Status: praktisch validierter bidirektionaler Phoropter-Kandidat. Das Auswahlfenster fuer AIS-Historienwerte ist im Verarbeitungslauf angebunden, die Erzeugung der `CVImport.xml` wurde praktisch gestartet, der Rueckweg ist fuer MEDISTAR-Historien-AIS plus CV-5000-Rueckgabe-Fixture abgesichert, und der MEDISTAR-Import der `6228`-/`6227`-Rueckgabedatei wurde praktisch bestaetigt.

## Validierter Ablauf per Fixture

### AIS/MEDISTAR -> XdtDeviceBridge -> Phoropter

- MEDISTAR liefert eine AIS-GDT/XDT-Datei mit Patientendaten und historischen Karteikarten-Messzeilen.
- Die App liest Patientendaten aus `3000`, `3101`, `3102`, `3103` und `8402`.
- Die App erkennt historische refraktive MEDISTAR-Zeilen mit `V0`, `V1`, `V2`, `V3` und `V4`.
- `V7`, `P` und `Y` werden als nicht fuer den CV-5000-Import freigegebene Gruppen erkannt.
- Bei aktivierter `Ausgabe an Geraet` und gesetztem Ausgabeordner oeffnet der Verarbeitungslauf das Fenster `Werte an Phoropter uebergeben`.
- Im Fenster werden die Gruppen nach Untersuchungsart angezeigt; neueste exportierbare `V0`-, `V1`- und `V2`-Datensaetze sind vorausgewaehlt.
- Aus ausgewaehlten refraktiven Datensaetzen wird eine TOPCON-CV-5000-kompatible XML-Importdatei erzeugt.
- Das Auswahlfenster bleibt standardmaessig per bestehendem `🔝`-Schalter im Vordergrund. Mit `Nichts senden` kann bewusst keine `CVImport.xml` erzeugt werden; die App wartet danach weiter auf die Phoropter-Rueckgabe.
- Die AIS-Datei kann in der Praxis gleich heissen, z. B. `Patient.gdt`; neue Dateiversionen werden ueber Dateiversion/Fingerprint erkannt und duerfen das Auswahlfenster nach einem abgeschlossenen Zyklus erneut oeffnen.
- Nach Phase 1 merkt sich die App bereits vorhandene Phoropter-Dateien als Baseline. Nur neue oder geaenderte Rueckgabedateien werden fuer Phase 2 verwendet; ihr Dateizeitstempel muss nicht zwingend neuer als `Patient.gdt` sein. Bei gleichnamigen ueberschriebenen Rueckgabedateien wird eine erweiterte beobachtete Dateiversion aus Metadaten wie Schreibzeit, Erstellzeit, Zugriffszeit und Groesse verglichen.

### Phoropter -> XdtDeviceBridge -> AIS/MEDISTAR

- TOPCON CV-5000 liefert eine XML-Geraetedatei.
- Die App erkennt CV-5000/CV-5000S ueber TOPCON-Ophthalmology-XML und `Measure type="SBJ"`.
- Der bereits in Phase 1 gelesene AIS-/Patientenkontext bleibt fachlich massgeblich; MEDISTAR-Historienzeilen in der AIS-Datei werden im Rueckweg tolerant gelesen und blockieren den Export nicht.
- Die App liest Type-Bloecke, R/L-Refraktionswerte, VD und PD.
- Die App erzeugt MEDISTAR-kompatible XDT-Rueckgabezeilen ueber `6228`.
- `8402` Untersuchungsart bleibt aus AIS/MEDISTAR.
- Nach erfolgreicher Phase 2 ist der konkrete AIS-/Geraetepaar-Zyklus abgeschlossen; identische Paare werden nicht doppelt exportiert, neue AIS- oder Phoropter-Dateiversionen mit gleichem Dateinamen bleiben aber verarbeitbar.

## Validierte Ergebnisfelder

- `8000 = 6310`
- `3000` Patientennummer
- `3101` Nachname
- `3102` Vorname
- `3103` Geburtsdatum
- `8402` Untersuchungsart aus AIS
- `6228` Prescription-Header und Prescription-Phoropterwerte
- `6227` Full-Correction-Header und Full-Correction-Phoropterwerte

## AIS-Historienfixture

Die Datei `Patient_mit_Phoropter_Daten.XDT` wird als Testfixture verwendet. Die Patientendaten sind Testdaten und werden hier nicht als Live-Daten dokumentiert.

Erkannt werden:

- Patientennummer `4701-1`
- Nachname `Testfrau`
- Vorname `Anna`
- Geburtsdatum `12061955`
- Untersuchungsart `Phoro`
- historische Gruppen `V0`, `V1`, `V2`, `V3`, `V4`, `V7`, `P`, `Y`

Default-Auswahl fuer den CV-5000-Import:

- neuester exportierbarer Lensmeter-Datensatz `V0`
- neuester exportierbarer Autorefraktor-Datensatz `V1`
- neuester exportierbarer Phoropter-Datensatz `V2`

Nicht parsebare oder fuer CV-5000 nicht freigegebene Zeilen erzeugen Warnungen oder nicht exportierbare Gruppen, aber keinen Absturz.

## CV-5000-Import-XML

Die erzeugte XML orientiert sich an `CVImport.xml`:

- Root `Ophthalmology`
- `nsCommon:Common`
- `nsSBJ:Measure type="SBJ"`
- `nsSBJ:RefractionTest`
- ein `Type` je ausgewaehltem Datensatz
- `ExamDistance No="1"`
- R/L `Sph`, `Cyl`, `Axis`
- `VD` und `PD/B`, wenn vorhanden
- UTF-8
- keine kuenstlichen leeren `ExamDistance No="2"`-Bloecke

Die Entfernung ist aktuell konservativ `500.000 cm`; die praktische CV-5000-Importvalidierung muss bestaetigen, ob dieser Wert fuer den Zielworkflow passt oder profilspezifisch angepasst werden soll.

## CV-5000-Rueckgabefixture

Die Datei `M-Serial1234_20130625_170509656_TOPCON_CV-5000_10111.xml` wird als Rueckgabe-Fixture verwendet.

Erkannt werden:

- `Company = TOPCON`
- `ModelName = CV-5000`
- `MachineNo = 10111`
- `Date = 2013-06-25`
- `Time = 17:05:09.656`
- `Measure type="SBJ"`
- TypeName `Prescription`
- TypeName `Full Correction`
- R/L `Sph +1.25`
- R/L `Cyl -2.00`
- R/L `Axis 7`
- `VD 13.75`
- `PD R 29.50`, `PD L 29.50`, `PD B 59.00`

Leere Prism-/VA-/Contrast-Felder werden ignoriert.

## Validierte MEDISTAR-Rueckgabe per Fixture

Die XDT-Laengenpraefixe werden zentral berechnet; die folgenden Zeilen zeigen den fachlichen Inhalt.

```text
6228 Phoropter finaler Verordnungswert
6228 R.:S=+ 1.25 Z=- 2.00*  7 PD= 59 VD= 13.75
6228 L.:S=+ 1.25 Z=- 2.00*  7
6227 Phoropter Maximalwert (Vollkorrektion)
6227 R.:S=+ 1.25 Z=- 2.00*  7 PD= 59 VD= 13.75
6227 L.:S=+ 1.25 Z=- 2.00*  7
```

Fachliche Bestaetigung:

- `Prescription` wird als finaler Verordnungswert vollstaendig ueber `6228` ausgegeben.
- `Full Correction` wird als Maximalwert/Vollkorrektion vollstaendig ueber `6227` ausgegeben.
- Der praktische MEDISTAR-Importtest zeigte, dass `6330` nicht angezeigt wird; CV-5000 verwendet deshalb keine `6330`-Zeilen mehr.
- `8402` kommt aus AIS/MEDISTAR.
- MEDISTAR-Historien-AIS-Dateien mit `V0`/`V1`/`V2`/`V3`/`V4`/`V7`/`P`/`Y`-Karteikartenzeilen werden fuer den CV-5000-Rueckweg tolerant gelesen.
- Beide Type-Bloecke werden ueber Ueberschriften getrennt; keine `6228 --`-Trennzeile mehr.
- Keine leeren R-/L-Zeilen.
- Keine kuenstlichen Nullwerte.
- Keine `6302` bis `6305` fuer Messwerte.

## XML-Struktur

- TOPCON Ophthalmology XML
- `nsCommon`
- `nsSBJ`
- optional `nsREF` / `nsLM`
- `Measure type="SBJ"`
- namespace-tolerantes Lesen nach LocalName

## UI-/Konfigurationshinweis

Im Dialog `Neues Geraet anlegen` wird nur noch die Faehigkeit `Bidirektionales Geraet, z. B. Phoropter` markiert. Ausgabeordner, Dateiname und Format werden dort nicht mehr konfiguriert.

Die konkrete Ausgabe an den Phoropter wird im Schnittstellenprofil gepflegt. Fuer CV-5000/CV-5000S ist dort der Bereich `Ausgabe an Geraet` sichtbar; dort werden Aktiv-Schalter, Ausgabeordner, Dateiname `CVImport.xml` und Format `TOPCON CV-5000 XML` verwaltet. Der Bereich `XDT-Anhaenge fuer AIS` ist nur fuer CV-5000/CV-5000S ausgeblendet und bleibt fuer andere Geraete sichtbar.

Wenn eine passende AIS-Datei im aktiven CV-5000-Schnittstellenprofil erkannt wird, verwendet die App diese Schnittstellenprofil-Konfiguration fuer die Richtung AIS/MEDISTAR -> Geraet. Fehlt der Ausgabeordner, wird keine `CVImport.xml` geschrieben und ein klarer Hinweis ausgegeben.

Die Erzeugung der `CVImport.xml` ist Phase 1 und kein finaler Workflowabschluss. AIS-/Patientenkontext und Eingangsdaten bleiben fuer Phase 2 relevant; erst die erfolgreiche Phoropter-Rueckgabe mit MEDISTAR-XDT-Erzeugung schliesst den Vorgang terminal ab.

`Nichts senden` ist kein Fehler und nicht identisch mit `Abbrechen`: Es schliesst nur das Auswahlfenster, schreibt keine Importdatei und laesst den CV-5000-Workflow fuer die spaetere Rueckgabedatei offen. `Abbrechen` bleibt der konservative Bedienabbruch ohne stillen Erfolg.

Der Duplicate-Schutz ist dateiversionsbezogen: Ein unveraendertes AIS-/Phoropter-Dateipaar wird nicht erneut exportiert und erzeugt keinen Fehlerordner-Nachlauf. Wird `Patient.gdt` fuer einen neuen Praxiszyklus erneut geschrieben, wird die neue Dateiversion erkannt und das Auswahlfenster kann ohne Reset wieder starten. Wird danach die gleichnamige Phoropter-Rueckgabedatei ueberschrieben oder geaendert, wird sie gegen die Phase-1-Baseline als neue Rueckgabe erkannt und der finale Export gestartet.

Wenn ein CV-5000-AIS-/Geraetepaar fachlich vollstaendig ist, wird der Exportpfad gestartet. Alte Phoropter-Dateien aus der Phase-1-Baseline werden dagegen nicht als neue Rueckgabe angezeigt; die Karte bleibt dann bei `Wartet auf Geraet`, bis eine neue oder geaenderte Rueckgabedatei erkannt wird.

## Grenzen / offen

- Weiter im Livebetrieb zu beobachten bleibt der Folgezyklus ohne Reset mit neu geschriebener, gleichnamiger AIS-Datei und gleichnamig ueberschriebener Phoropter-Rueckgabedatei.
- Das Einlesen der erzeugten `CVImport.xml` am echten CV-5000/CV-5000S muss weiter beobachtet werden.
- `500.000 cm` als Import-ExamDistance ist konservativ und muss am CV-5000/CV-5000S bestaetigt werden.
- Keratometer/Tonometrie/Pachymetrie aus historischen MEDISTAR-Zeilen werden erkannt, aber noch nicht in die CV-5000-Import-XML geschrieben.
- Kein offizielles ZIP-Artefakt vor Freigabe nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md`.
- Keine Live-Pfade, Kundendaten oder Patientendaten dokumentiert.
