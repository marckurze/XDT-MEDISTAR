# Templatepaket MEDISTAR + Dokumentanhang

Dieses Dokument beschreibt den Templatepaket-Kandidaten fuer `MEDISTAR + Dokumentanhang`. Der Workflow ist fuer Geraete oder Arbeitsablaeufe gedacht, die keine Messwerte liefern, sondern Dateien als externe MEDISTAR-Anhaenge uebergeben.

## Status

V1-Kandidat, testseitig abgesichert. Eine praktische MEDISTAR-Abnahme mit echten Praxisdateien steht noch aus. Ein offizielles ZIP-Artefakt wird erst nach der Release-Regel in `docs/TEMPLATEPAKET_RELEASE_REGEL.md` dauerhaft abgelegt.

## Enthaltene Profile

| Profiltyp | ID | Name | Hinweis |
| --- | --- | --- | --- |
| AIS-Profil | `ais-medistar-default` | `MEDISTAR` | BuiltIn |
| Geraeteprofil | `device-document-attachment-default` | `Generisches Dokumentgeraet` | BuiltIn, AttachmentOnly |
| Exportprofil | `export-medistar-document-attachment-default` | `MEDISTAR + Dokumentanhang Export` | BuiltIn, optional `6227`, Linkfelder |
| Schnittstellenprofil | `interface-medistar-document-attachment-default` | `MEDISTAR + Dokumentanhang` | BuiltIn, inaktiv |

## Dateiverhalten

Das Profil erwartet keine Messwertdateien. Unterstuetzte Dateien werden als Anhaenge behandelt:

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

XML-Dateien sind in diesem Profilmodus reine Anhaenge und werden nicht als NIDEK-/TOPCON-Messwert-XML geparst. MP4, MP3 und WAV werden weder abgespielt noch ausgewertet.

## MEDISTAR-Ausgabe

Die XDT-Ausgabe enthaelt die AIS-Basisfelder wie bisher, insbesondere `8000`, Patientendaten und `8402` aus der AIS-Datei.

Optional kann ein Anwendertext als `6227` ausgegeben werden:

```text
6227 <Dokumentationstext>
```

Ohne Text wird keine leere `6227`-Zeile erzeugt.

Fuer jede erfolgreich uebertragene Datei werden eigene Linkfelder erzeugt:

```text
6302 Dokumentenname
6303 Dateiformat
6304 Beschreibung, optional
6305 vollstaendiger Dateipfad
```

Es werden keine Messwertfelder fuer Dokumentgeraete erzeugt: keine `6228`, keine `6205`, keine `6220`.

## Grenzen

- keine Drag-&-Drop-Dateisammlung
- keine Vorschaukacheln
- keine pro-Datei-Kommentare
- keine Bild-, DICOM-, Video- oder Audioanalyse
- keine OCR
- keine App-interne Datei-Umbenennung durch Anwender

Der erste Live-Test sollte AIS-Datei plus mehrere stabile Anhaenge pruefen und bestaetigen, dass MEDISTAR jede Datei einzeln ueber `6302` bis `6305` erhaelt.
