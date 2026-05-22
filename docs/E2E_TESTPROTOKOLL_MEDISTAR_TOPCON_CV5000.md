# E2E-Testprotokoll: MEDISTAR + TOPCON CV-5000 / CV-5000S

Status: testseitig validierter bidirektionaler Phoropter-Kandidat. Die praktische MEDISTAR-/CV-5000-Livevalidierung steht noch aus.

## Validierter Ablauf per Fixture

### AIS/MEDISTAR -> XdtDeviceBridge -> Phoropter

- MEDISTAR liefert eine AIS-GDT/XDT-Datei mit Patientendaten und historischen Karteikarten-Messzeilen.
- Die App liest Patientendaten aus `3000`, `3101`, `3102`, `3103` und `8402`.
- Die App erkennt historische refraktive MEDISTAR-Zeilen mit `V0`, `V1`, `V2`, `V3` und `V4`.
- `V7`, `P` und `Y` werden als nicht fuer den CV-5000-Import freigegebene Gruppen erkannt.
- Aus ausgewaehlten refraktiven Datensaetzen wird eine TOPCON-CV-5000-kompatible XML-Importdatei erzeugt.

### Phoropter -> XdtDeviceBridge -> AIS/MEDISTAR

- TOPCON CV-5000 liefert eine XML-Geraetedatei.
- Die App erkennt CV-5000/CV-5000S ueber TOPCON-Ophthalmology-XML und `Measure type="SBJ"`.
- Die App liest Type-Bloecke, R/L-Refraktionswerte, VD und PD.
- Die App erzeugt MEDISTAR-kompatible XDT-Rueckgabezeilen ueber `6228`.
- `8402` Untersuchungsart bleibt aus AIS/MEDISTAR.

## Validierte Ergebnisfelder

- `8000 = 6310`
- `3000` Patientennummer
- `3101` Nachname
- `3102` Vorname
- `3103` Geburtsdatum
- `8402` Untersuchungsart aus AIS
- `6228` Phoropter-Rueckgabezeilen

## AIS-Historienfixture

Die Datei `Patient_mit_Phoropter_Daten.XDT` wird als Testfixture verwendet. Die Patientendaten sind Testdaten und werden hier nicht als Live-Daten dokumentiert.

Erkannt werden:

- Patientennummer `4701-1`
- Nachname `Testfrau`
- Vorname `Anna`
- Geburtsdatum `12061955`
- Untersuchungsart `Phoro`
- historische Gruppen `V0`, `V1`, `V2`, `V3`, `V7`, `P`, `Y`

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
6228 R.:S=+ 1.25 Z=- 2.00*  7 PD= 59 VD= 13.75
6228 L.:S=+ 1.25 Z=- 2.00*  7
6228 --
6228 R.:S=+ 1.25 Z=- 2.00*  7 PD= 59 VD= 13.75
6228 L.:S=+ 1.25 Z=- 2.00*  7
```

Fachliche Bestaetigung:

- Phoropter-Rueckgabe wird ueber `6228` ausgegeben.
- `8402` kommt aus AIS/MEDISTAR.
- Beide Type-Bloecke werden getrennt.
- Keine `6227`-Ausgabe fuer CV-5000-Phoropterwerte.
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

## Grenzen / offen

- Praktische MEDISTAR-/CV-5000-Livevalidierung steht noch aus.
- Das Auswahlfenster fuer den Arzt muss im echten Verarbeitungslauf praktisch validiert werden.
- `500.000 cm` als Import-ExamDistance ist konservativ und muss am CV-5000/CV-5000S bestaetigt werden.
- Keratometer/Tonometrie/Pachymetrie aus historischen MEDISTAR-Zeilen werden erkannt, aber noch nicht in die CV-5000-Import-XML geschrieben.
- Kein offizielles ZIP-Artefakt vor Freigabe nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md`.
- Keine Live-Pfade, Kundendaten oder Patientendaten dokumentiert.
