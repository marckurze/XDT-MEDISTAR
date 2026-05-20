# Templatepaket MEDISTAR + Dokumentanhang

Dieses Dokument beschreibt den Templatepaket-Kandidaten fuer `MEDISTAR + Dokumentanhang`. Der Workflow ist fuer Geraete oder Arbeitsablaeufe gedacht, die keine Messwerte liefern, sondern Dateien als externe MEDISTAR-Anhaenge uebergeben.

## Status

V1-Kandidat, testseitig abgesichert. Eine praktische MEDISTAR-Abnahme mit echten Praxisdateien steht noch aus. Ein offizielles ZIP-Artefakt wird erst nach der Release-Regel in `docs/TEMPLATEPAKET_RELEASE_REGEL.md` dauerhaft abgelegt.

## Enthaltene Profile

| Profiltyp | ID | Name | Hinweis |
| --- | --- | --- | --- |
| AIS-Profil | `ais-medistar-default` | `MEDISTAR` | BuiltIn |
| Geraeteprofil | `device-document-attachment-default` | `Generisches Dokumentgeraet` | BuiltIn, AttachmentOnly |
| Exportprofil | `export-medistar-document-attachment-default` | `MEDISTAR + Dokumentanhang Export` | BuiltIn, Linkfelder |
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

Fuer jede erfolgreich uebertragene Datei werden eigene Linkfelder erzeugt:

```text
6302 Dokumentenname
6303 Dateiformat
6304 Beschreibung
6305 vollstaendiger Dateipfad
```

Im manuellen Dokumentfenster gehoert die Beschreibung direkt zur jeweiligen Datei. Dieser pro-Datei-Text wird als `6304` ausgegeben; wenn der Anwender nichts eintraegt, wird der Originaldateiname als sichtbare Beschreibung verwendet. `6305` bleibt immer der technische Zielpfad im Dokument-Exportordner. Mehrzeilige Beschreibungen werden fuer `6304` einzeilig zusammengefuehrt, damit keine nackten Folgezeilen in der XDT-Datei entstehen.

Es werden keine Messwertfelder fuer Dokumentgeraete erzeugt: keine `6228`, keine `6205`, keine `6220`.

## Abschlusslogik

Dokumentgeraete koennen mehrere Dateien nacheinander schreiben. Deshalb wird nicht mehr sofort nach der ersten stabilen Datei exportiert.

- `Abschluss nach Wartezeit`: Nach der ersten stabilen Datei startet eine profilbezogene Wartezeit, Standard 10 Sekunden. Jede weitere stabile Datei startet die Wartezeit neu. Erst nach Ablauf ohne neue Datei wird uebertragen.
- `Manuell bestaetigen`: Sobald AIS-Datei und mindestens eine stabile Dokumentdatei vorhanden sind, oeffnet der Dialog `Dokumente uebertragen`. Der Dialog bleibt offen, neue stabile Dateien werden inkrementell in der Liste ergaenzt, vorhandene Texteingaben bleiben erhalten, und ohne Klick auf `Uebertragen` wird kein Export erzeugt.

## Konfiguration in der App

Im Tab `Schnittstellenprofile` ist fuer AttachmentOnly-Profile der Geraete-/Dokument-Importordner der massgebliche Eingang fuer Dokumentdateien. Der separate XDT-Anhang-Importordner normaler Messgeraete wird fuer diesen Profiltyp ausgeblendet, damit keine zwei konkurrierenden Eingangsordner sichtbar sind. Die internen `6302`-`6305`-Templatefelder werden fuer normale Anwender nicht angezeigt; Defaults wie `Datei`, `{ExtensionUpperWithoutDot}` und `{Attachment.TargetFullPath}` bleiben intern wirksam.

Die Option `Dokumentationstext erfassen` steuert die pro-Datei-Beschreibung im Dialog. Ist sie aus, werden die Anhaenge trotzdem ueber `6302` bis `6305` exportiert; `6304` wird dann mit dem Originaldateinamen gefuellt. Der fruehere globale Sammeltext ueber `6227` wird im manuellen Dokumentuebergabe-Dialog nicht mehr automatisch erzeugt.

Waehrend der Dialog offen ist, wird die Dateiliste nicht bei jedem Polling-Takt neu aufgebaut. Neue stabile Dateien werden nur ergaenzt, vorhandene Eingaben bleiben im Textfeld erhalten, und die automatische Paarverarbeitung wird erst nach Klick auf `Uebertragen` fortgesetzt.

AttachmentOnly erzwingt die Anhangverarbeitung zur Laufzeit ueber den Dokument-Importordner. Alte gespeicherte technische Flags wie deaktivierte XDT-Anhang-Automatik duerfen deshalb nicht mehr dazu fuehren, dass nur AIS-Daten ohne Dokumentlinks exportiert werden. Die Pruefung vor Aktivierung arbeitet mit aktuellen, noch nicht gespeicherten UI-Werten und zeigt die Ordner-Erreichbarkeit als Hinweis/Warnung; ein eingetragener, aktuell nicht erreichbarer Ordner blockiert nicht hart.

## Grenzen

- keine Drag-&-Drop-Dateisammlung
- keine Vorschaukachel-Galerie
- keine pro-Datei-Messwertlogik
- keine Bild-, DICOM-, Video- oder Audioanalyse
- keine OCR
- keine App-interne Datei-Umbenennung durch Anwender

Der erste Live-Test sollte AIS-Datei plus mehrere stabile Anhaenge pruefen und bestaetigen, dass MEDISTAR jede Datei einzeln ueber `6302` bis `6305` erhaelt.
