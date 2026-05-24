# Templatepaket MEDISTAR + Manuelle Dokumentuebergabe

Dieses Dokument beschreibt den V1-Kandidaten fuer `MEDISTAR + Manuelle Dokumentuebergabe`. Der Workflow ist fuer Arbeitsablaeufe gedacht, bei denen MEDISTAR eine AIS-Datei sendet, aber keine Geraetedatei automatisch in einem Importordner landet.

## Status

V1-Kandidat, testseitig abgesichert. Eine praktische MEDISTAR-Abnahme steht noch aus. Ein offizielles ZIP-Artefakt wird erst nach der Release-Regel in `docs/TEMPLATEPAKET_RELEASE_REGEL.md` dauerhaft abgelegt.

## Enthaltene Profile

| Profiltyp | ID | Name | Hinweis |
| --- | --- | --- | --- |
| AIS-Profil | `ais-medistar-default` | `MEDISTAR` | BuiltIn |
| Geraeteprofil | `device-manual-document-selection-default` | `Manuelle Dokumentauswahl` | BuiltIn, keine Messwertdatei |
| Exportprofil | `export-medistar-manual-document-transfer-default` | `MEDISTAR + Manuelle Dokumentuebergabe Export` | BuiltIn, AIS-Basisfelder plus Anhang-Linkfelder |
| Schnittstellenprofil | `interface-medistar-manual-document-transfer-default` | `MEDISTAR + Manuelle Dokumentuebergabe` | BuiltIn, inaktiv |

## Ablauf

1. MEDISTAR/AIS schreibt die Patientendatei.
2. Die App oeffnet direkt den Dialog `Dokumente uebertragen` mit leerer Dateiliste; die Monitoring-Karte bleibt im Tab `Verarbeitung` angedockt.
3. Der Anwender fuegt Dateien per Drag-&-Drop, ueber `Dateien hinzufuegen` oder aus einer Datei-Zwischenablage per `Strg+V`/`Einfügen` hinzu.
4. Pro Datei kann eine Beschreibung eingegeben werden.
5. Erst der Klick auf `Uebertragen` erzeugt die XDT-Rueckgabe.

Ohne AIS-Datei wird beim Start der Ueberwachung kein Dialog geoeffnet und kein Floating-Fenster automatisch abgedockt. Auch bei AIS-Eingang wird fuer diesen Sondermodus kein gruenes Geraeteanbindungsfenster abgedockt; nur der Dokumentdialog kommt in den Vordergrund. Ohne `Uebertragen` wird kein Export erzeugt. Es gibt keinen Wartezeitmodus und keinen automatischen Abschluss.

Nach `Uebertragen` wird der interne Dialog- und Bestaetigungszustand des Profils verworfen. Ein spaeterer AIS-Vorgang startet deshalb wieder mit einem neuen leeren Dialog; gleiche AIS-Dateinamen wie `Patient.gdt` sind zulaessig, sobald die Datei stabil und lesbar im Eingangsordner liegt.

## Dateiverhalten

Unterstuetzt werden dieselben sicheren Dokumentanhang-Formate wie beim automatischen Dokumentgeraet:

- `.pdf`
- `.jpg` / `.jpeg`
- `.png`
- `.tif` / `.tiff`
- `.dcm`
- `.txt`
- `.xml`
- `.mp4`
- `.mp3`
- `.wav`

Ordner und nicht unterstuetzte Dateitypen werden abgelehnt. XML ist in diesem Modus immer Anhang, nicht Messwert-XML. Es gibt keine Bild-, DICOM-, Audio-, Video- oder Dokumentanalyse.

Der Dialog `Dokumente uebertragen` bleibt standardmaessig im Vordergrund, damit Dateien bequem aus Explorer oder anderen Programmen hineingezogen werden koennen. Der kompakte `🔝`-Schalter kann dieses Verhalten fuer das Dialogfenster deaktivieren und folgt optisch den Geraeteanbindungsfenstern.

## MEDISTAR-Ausgabe

Die XDT-Ausgabe enthaelt AIS-Basisfelder wie `8000`, Patientendaten und `8402` aus der AIS-Datei. Fuer jede Datei werden eigene Linkfelder erzeugt:

```text
6302 Dokumentenname
6303 Dateiformat
6304 Beschreibung
6305 vollstaendiger Dateipfad
```

`6304` enthaelt den eingegebenen Datei-Text. Wenn kein Text eingegeben wurde, wird der Originaldateiname verwendet. Mehrzeiliger Text wird fuer `6304` einzeilig zusammengefuehrt. `6305` bleibt der technische Zielpfad im Dokument-Exportordner.

Es werden keine Messwertfelder erzeugt: keine `6228`, keine `6205`, keine `6220` und keine Sammel-`6227` aus Datei-Kommentaren.

## Sicherheit

Manuell ausgewaehlte Quelldateien werden nicht geloescht. Die Uebergabe nutzt fuer diesen Modus konservativ Kopieren in den konfigurierten Dokument-Exportordner; Kollisionsschutz und die bestehenden `6302`-`6305`-Linkfelder bleiben erhalten.

Reset verwirft nur die aktuelle Auswahl und den Vorgang. Fremde Quellordner wie Desktop, Netzlaufwerk oder USB-Laufwerk werden nicht bereinigt.

## Grenzen

- keine Messwertparser
- keine OCR
- keine Bild-, DICOM-, Audio- oder Videoanalyse
- keine Vorschaukachel-Galerie
- keine Ordnerauswahl
- keine automatische Geraetedatei-Erwartung
