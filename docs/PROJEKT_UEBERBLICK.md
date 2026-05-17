# Projektueberblick XdtDeviceBridge / XDT Verwaltung

Stand: 2026-05-12

Diese Datei dient als kompakte Uebergabe fuer neue Chats, spaetere Codex-Sessions und Projektplanung. Sie trennt aktuellen Iststand, validierten Kernworkflow, vorbereitete Bausteine und Zielbild.

## 1. Kurzbeschreibung

XdtDeviceBridge ist eine lokale WPF-Desktop-App fuer die dateibasierte Bridge zwischen Arztinformationssystemen und augenaerztlichen Untersuchungsgeraeten.

Der Fokus liegt aktuell auf:

- MEDISTAR als AIS/PVS-Ziel
- GDT/XDT-Dateien als Austauschformat
- augenaerztlichen Messgeraeten
- lokalem, offlinefaehigem Betrieb

Es gibt bewusst:

- keine Cloud-Anbindung
- keinen Windows-Dienst
- keinen Autostart
- keinen `FileSystemWatcher`
- keine Verarbeitung beim App-Start

Aktuelle Entwicklungsleitlinie: Fertige Geraeteprofile und Templatepakete haben Vorrang vor weiterer Aktivierungs- oder Assistentenarchitektur. Der Baukasten bleibt Werkzeug fuer Sonderfaelle, Tests, Vorschau und kundenspezifische Anpassungen, nicht der normale Bedienweg.

## 2. Aktuelle Version

Aktuelle Version laut `VERSION` und `Directory.Build.props`:

```text
0.1.0-prototype
```

Assembly-/Dateiversionen:

- `Version`: `0.1.0-prototype`
- `AssemblyVersion`: `0.1.0.0`
- `FileVersion`: `0.1.0.0`
- `InformationalVersion`: `0.1.0-prototype`

Keine neue Version wurde fuer diesen Dokumentationsstand erfunden.

## 3. Praktisch validierter Kernworkflow

Praktisch validiert ist weiterhin der Workflow:

- MEDISTAR + NIDEK ARK1S
- AIS-GDT/XDT einlesen
- NIDEK ARK1S XML einlesen
- Patientendaten uebernehmen
- Messwerte aus XML mappen
- MEDISTAR-kompatible XDT-Datei erzeugen
- XDT-Anhang-Link ueber `6302`, `6303`, optional `6304` und `6305` im Pflicht-Anhang-Praxislauf

Wichtige XDT-Felder im validierten Kernworkflow:

- `8000 = 6310`
- Patientendaten, insbesondere `3000`, `3101`, `3102`, `3103`
- `8402` Untersuchungsart
- `6228` Ergebniszeilen fuer rechte/linke Messwerte
- bei erfolgreichem XDT-Anhang-Link `6302`, `6303`, optional `6304` und `6305`

Der XDT-Anhang-Link wurde am 2026-05-11 mit MEDISTAR + NIDEK ARK1S praktisch validiert: Pflicht-Anhang vorhanden, Pflicht-Anhang fehlt/Fehlerfall und Linkaufruf aus einer MEDISTAR-Karteikarte wurden erfolgreich geprüft. Es werden keine personenbezogenen Patientendaten oder Live-System-Pfade dokumentiert.

Dieser Kernworkflow ist der stabile Prototyp-Kern und muss bei allen naechsten Schritten unveraendert funktionsfaehig bleiben.

## 4. Aktuelle Haupt-Tabs

Die App besitzt aktuell diese Haupt-Tabs:

- `Verarbeitung`
- `Profile & Templates`
- `Schnittstellenprofile`
- `Lizenz`

## 5. Tab Verarbeitung

Der Tab `Verarbeitung` ist inzwischen staerker als Betriebs- und Automatikbereich gedacht.

Er enthaelt bzw. fokussiert:

- aktive Schnittstellenprofile
- Ueberwachung manuell starten/stoppen
- automatische Verarbeitung per Haken
- gefundene Dateipaare bzw. Verarbeitungspakete
- Status- und Logmeldungen

Die automatische Verarbeitung startet nur, wenn:

- die Ueberwachung manuell gestartet wurde
- die globale automatische Verarbeitung aktiv ist
- ein aktives Schnittstellenprofil vorhanden ist
- die beteiligten Dateien stabil sind

Der alte manuelle Testbereich ist nicht entfernt. Er bleibt als eingeklappter Bereich `Diagnose / manueller Alt-Test` erhalten und dient als Rueckfallfunktion.

## 6. Tab Profile & Templates

Der Tab `Profile & Templates` ist der Profil- und Templatebereich. Der spaetere Standardweg soll ein fertiges Geraeteprofil plus fertiges Templatepaket sein; der Baukasten ist der Rueckfall- und Testbereich.

Er enthaelt:

- Profiluebersicht
- AIS-Profile
- Geraeteprofile
- Exportprofile
- Exportregeln
- sichere UserDefined-Wartung fuer Exportprofile und Exportregeln
- Platzhalter
- Templatepaket-Export per ausgewaehltem Schnittstellenprofil
- Templatepaket-Import mit Validierung, Importvorschau, Benutzerwahl und sicherer UserDefined-Uebernahme
- Baukastenbereich `Test & Vorschau`

Der Bereich `Test & Vorschau` erlaubt:

- AIS-Datei laden
- Geraetedatei laden
- XDT-Anhang einlesen
- Messwerte pruefen
- Gesamtexport-Vorschau XDT pruefen
- Testexport erstellen

## 7. Baukasten `Test & Vorschau`

Der Baukasten dient dem manuellen Testen von Profilen, Exportregeln und XDT-Anhang-Konfigurationen. Er soll schlank bleiben und nicht zum normalen Bedienweg fuer Standardgeraete werden.

Wichtige Rollen:

- Das Schnittstellenprofil dient als Testkontext.
- Das Exportprofil steuert die normalen XDT-Ergebnisfelder.
- Das Schnittstellenprofil verbindet AIS-Profil, Geraeteprofil, Exportprofil, Ordner und XDT-Anhang-Einstellungen.
- XDT-Anhang-Linkfelder `6302` bis `6305` werden im Baukasten transient ergaenzt und veraendern kein Exportprofil.

Aktueller Stand:

- Ein XDT-Anhang kann im Baukasten aus einem beliebigen Speicherort ausgewaehlt werden.
- Die Vorschau simuliert trotzdem den Zielpfad aus dem Schnittstellenprofil.
- `6305` zeigt in Vorschau und Test-XDT auf den simulierten Schnittstellenprofil-Zielpfad.
- Der simulierte Zielpfad besteht aus `XDT-Anhang Exportordner` plus erzeugtem Dateinamen.
- Der Quellpfad der ausgewaehlten Datei wird nicht als `6305` verwendet.
- Der Testexport schreibt physisch eine Test-XDT-Datei und, falls vorhanden, den umbenannten Anhang in einen frei gewaehlten Testordner.
- Der produktive XDT-Anhang Exportordner aus dem Schnittstellenprofil wird im Baukasten-Test nicht beschrieben.
- Exportprofil und BuiltIn-Profile werden nicht veraendert.
- `Messwerte pruefen` ist standardmaessig eingeklappt.
- `Verfuegbare Platzhalter` ist standardmaessig ausgeklappt.

## 8. Tab Schnittstellenprofile

Ein Schnittstellenprofil verknuepft AIS-Profil, Geraeteprofil, Exportprofil, Ordner, Archivierung, Fehlerablage, Automatik- und XDT-Anhang-Einstellungen.

Aktuell dokumentierte Felder:

- AIS-Importordner
- Geraete-Importordner
- Exportordner ans AIS
- Archivordner
- Fehlerordner
- Ordnerabfrage-Intervall, Default `5` Sekunden
- Wartezeit auf Geraetedatei, Default `10` Minuten
- XDT-Anhang Importordner
- XDT-Anhang Exportordner
- XDT-Anhang Dateiname
- XDT-Anhang Uebertragung `Copy`/`Move`, Standard `Move`
- `XDT-Anhaenge fuer AIS automatisch verarbeiten`, Standard aus
- XDT-Anhang ist `optional` oder `Pflicht`, Standard `optional`
- Wartezeit auf XDT-Anhang, Default `30` Sekunden
- Dateistabilitaet abwarten, Default `2` Sekunden
- XDT-Feld `6302` Dokumentenname
- XDT-Feld `6303` Dateiformat
- XDT-Feld `6304` Beschreibung
- XDT-Feld `6305` vollstaendiger Dateipfad bzw. Pfadtemplate
- Archivierungsmodus `Copy`/`Move`
- Archivoptionen
- Fehleroptionen
- Aktiv-Haken
- Lizenzpflicht-Haken

BuiltIn-Schnittstellenprofile werden nicht ueberschrieben. Beim Speichern auf Basis eines BuiltIn-Profils wird eine UserDefined-Kopie erzeugt.

Aktivierungsassistent aktueller Stand:

- Im Tab `Schnittstellenprofile` gibt es den Bereich `Pruefung vor Aktivierung`.
- Die Pruefung zeigt Status, Aktivierbarkeit, Blocker-/Warnungs-/Hinweiszaehler, strukturierte Ordnerpruefung, strukturierte XDT-Anhang-Konfiguration und die eingeklappte Tabelle `Alle Pruefpunkte`.
- Der Button `Aktivierung vorbereiten` oeffnet einen reinen OK-/Vorschaudialog.
- Der Dialog zeigt nur noch V1-relevante Vorschauinformationen: Bewertung, technische Guard-Entscheidung, `Aktivierbar nach V1`, Blocker, Warnungen, Hinweise und Sicherheitshinweis.
- Die Service-Kette ist: `InterfaceProfileActivationEvaluationService` -> `InterfaceProfileActivationGuardService` -> `InterfaceProfileActivationPreparationPreviewService`.
- Ein defensiver `InterfaceProfileActivationExecutorStub` und ein read-only/ValidateOnly-naher ProfileStore-Adapter sind vorhanden, aber nicht an die UI angebunden und nicht produktiv aktivierend.
- Der Stand ruht vorerst: kein Aktivieren-Button, keine automatische Aktivierung, keine produktive Warnungsbestaetigung, keine produktive Executor-Implementierung, keine Aenderung an `IsActive` oder `IsAttachmentProcessingEnabled`, keine Profil-Speicherung und keine Datei-/Ordneroperationen.
- BuiltIn-Profile bleiben geschuetzt; die spaetere Aktivierung ist auf kontrollierte UserDefined-Schnittstellenprofile ausgerichtet.
- Die Layout-Ueberlagerung unterhalb `Ordnerbereinigung` wurde behoben; `Ordnerbereinigung`, `Archivierung` und `Pruefung vor Aktivierung` sind wieder getrennt lesbar.

## 9. XDT-Anhaenge fuer AIS

XDT-Anhaenge fuer AIS sind nicht mehr nur optionales Zukunftsthema. Der Baukasten und die Schnittstellenprofile enthalten vorbereitete Konfiguration und Testpfade fuer externe AIS-Links.

Unterstuetzte Anhangtypen im aktuellen Vorbereitungsstand:

- PDF
- JPG
- JPEG
- PNG
- TIF
- TIFF
- DCM
- TXT

Externe AIS-Linkfelder:

- `6302`: Dokumentenname / Anzeige in Karteikarte
- `6303`: Dateiformat, z. B. `PDF`, `JPG`, `DCM`, `TXT`
- `6304`: Beschreibung, optional
- `6305`: vollstaendiger absoluter Dateipfad

Technische Grundsaetze:

- Der `XdtExportBuilder` erzeugt XDT-Längenpraefixe zentral.
- In UI und Konfiguration werden keine manuellen Längenpraefixe gepflegt.
- Automatische XDT-Anhang-Verarbeitung ist nur unter Sicherheitsbedingungen erlaubt.
- Mehrere unterstuetzte Anhaenge werden nicht automatisch zugeordnet.
- Instabile Anhaenge werden nicht verarbeitet, nicht verschoben und nicht verlinkt.
- Exportprofile werden durch XDT-Anhang-Test und Testexport nicht dauerhaft veraendert.
- MEDISTAR + NIDEK ARK1S + XDT-Anhang-Link ist fuer den Pflicht-Anhang-Praxislauf praktisch validiert; weitere Geraete/AIS und Mehrfachanhangfaelle bleiben separat zu pruefen.

## 10. Paket-Wartelogik

Die automatische Verarbeitung arbeitet fachlich als Verarbeitungspaket:

```text
AIS-Datei -> Geraetedatei -> optionaler oder verpflichtender XDT-Anhang
```

### Phase 1: AIS-Datei wartet auf Geraetedatei

- Eine stabile AIS-Datei allein erzeugt noch keinen Export.
- Die App wartet bis zu `DeviceFileWaitTimeoutMinutes` auf eine passende stabile Geraetedatei.
- Default: `10` Minuten.
- Eine neuere AIS-Datei ersetzt eine aeltere wartende AIS-Datei.
- Eine abgelaufene AIS-Datei erzeugt keinen Export.

### Phase 2: XDT-Anhang-Wartezeit nach vollstaendigem Paar

- Erst wenn AIS-Datei und Geraetedatei als stabiles Paar vorhanden sind, beginnt die XDT-Anhang-Wartezeit.
- Die Wartezeit auf XDT-Anhang ist pro Schnittstellenprofil konfigurierbar.
- Default: `30` Sekunden.

Optionaler XDT-Anhang:

- Wenn genau ein stabiler Anhang innerhalb Timeout kommt: Export mit `6302`, `6303`, optional `6304`, `6305`.
- Wenn kein Anhang kommt: Export ohne Anhang nach Timeout.
- Wenn mehrere unterstuetzte Anhaenge vorhanden sind: keine automatische Zuordnung, Warn-/Skip-Verhalten.

Pflicht-XDT-Anhang:

- Wenn genau ein stabiler Anhang kommt: Export mit `6302`, `6303`, optional `6304`, `6305`.
- Wenn kein Anhang kommt: Blockade/Fehler.
- Wenn mehrere unterstuetzte Anhaenge vorhanden sind: Blockade/Fehler wegen unsicherer Zuordnung.

## 11. Dateistabilitaet

Dateien duerfen erst verarbeitet werden, wenn sie stabil sind:

- AIS-Dateien
- Geraetedateien
- XDT-Anhaenge

Der `FileStabilityService` prueft Groesse und Aenderungszeitpunkt ueber einen konfigurierten Zeitraum.

Fuer XDT-Anhaenge gilt:

- Stabilitaetswartezeit: Default `2` Sekunden.
- Instabile Dateien werden nicht verschoben.
- Instabile Dateien werden nicht verlinkt.
- Instabile Dateien werden nicht exportiert.
- Instabile Dateien werden spaeter erneut bewertet.

## 12. Automatik-Prototyp

Der Automatik-Prototyp ist bewusst konservativ.

Aktuelle Sicherheitsregeln:

- keine Verarbeitung beim App-Start
- kein Windows-Dienst
- kein Autostart
- kein `FileSystemWatcher`
- periodischer Scan
- Scan-Intervall konfigurierbar, Default `5` Sekunden
- automatische Verarbeitung nur bei manuell gestarteter Ueberwachung
- automatische Verarbeitung nur mit gesetztem Haken
- keine unbekannten Dateien anfassen
- keine pauschale Ordnerleerung
- Exportordner wird nicht bereinigt

## 13. Archivierung und Fehlerablage

Archivierung kann pro Schnittstellenprofil konfiguriert werden.

Archivierungsmodi:

- `Copy`
- `Move`

Fuer produktionsnahe Nutzung ist `Move` oft sinnvoll, damit Importordner nach erfolgreicher Verarbeitung sauber bleiben.

Aktuelle Grundsaetze:

- Nur bekannte verarbeitete AIS-/Geraetedateien werden gemaess Profiloption archiviert.
- Keine unbekannten Dateien werden geloescht.
- Keine Ordner werden pauschal geleert.
- Der Exportordner wird nicht bereinigt.
- Fehlerablage erzeugt eine Kopie der beteiligten Dateien und eine `error.txt`.
- Originaldateien bleiben im Fehlerfall erhalten, soweit die aktuelle Sicherheitslogik dies vorsieht.

## 14. Profil- und Template-System

Profilarten:

- AIS-Profile
- Geraeteprofile
- Exportprofile
- Schnittstellenprofile

BuiltIn-Profile:

- werden mit der App geliefert
- sind schreibgeschuetzt
- duerfen nicht ueberschrieben werden
- koennen als Vorlage fuer UserDefined-Kopien dienen

UserDefined-Profile:

- werden separat gespeichert
- liegen unter `%LocalAppData%\XdtDeviceBridge\profiles`
- werden JSON-basiert verwaltet

Templatepakete:

- Templatepaket-Export ist vorhanden.
- Templatepaket-Import ist vorhanden.
- Importvalidierung ist vorhanden.
- Konfliktanalyse ist vorhanden.
- Importplan ist vorhanden.
- Dry-Run / Importvorschau ist vorhanden.
- Die UI-Vorschau zeigt Konflikte, Abhaengigkeiten, BuiltIn-Schutz und XDT-Anhang-Warnungen.
- Sichere Benutzeraktionen sind moeglich:
  - Neu importieren
  - Als Kopie importieren
  - Bestehendes behalten
  - Ueberspringen
- Explizite Uebernahme als UserDefined ist vorhanden.
- BuiltIn-Profile werden nicht ueberschrieben.
- Abhaengigkeiten von Schnittstellenprofilen werden auf lokale oder importierte Zielprofile remapped.
- Importierte Schnittstellenprofile bleiben inaktiv.
- `IsAttachmentProcessingEnabled` wird bei importierten Schnittstellenprofilen deaktiviert.
- XDT-Anhang-Einstellungen bleiben erhalten, muessen aber vor Aktivierung geprueft werden.
- `ReplaceExisting` ist weiterhin nicht aktiv.
- Der sichere Importfluss ist E2E-nah automatisiert getestet.

## 15. Vorbereitete Geraeteprofile

Praktisch validiert:

- MEDISTAR
- NIDEK ARK1S

Vorbereitet, aber noch nicht produktiv validiert:

- NIDEK AR360 / AR-360A: Auto-Refraktor-XDT-Rueckgabe praktisch validiert; XDT-Anhangfall und offizielles ZIP-Artefakt offen
- NIDEK LM7/LM7P
- NIDEK NT530P
- TOPCON CL300
- TOPCON KR800
- TOPCON TRK2P

Wichtig: Diese V2-/BuiltIn-Profile sind vorbereitet und konfigurierbar, aber nicht im gleichen Sinne praktisch validiert wie MEDISTAR + NIDEK ARK1S.

Die kompakte Bestandsaufnahme steht in `docs/GERAETE_PROFILE_TEMPLATE_MATRIX.md`. ARK1S ist Referenzpaket 1 in `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_ARK1S.md`; AR360 ist Referenzpaket 2 in `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_AR360.md`. AR360 nutzt ARMedian, FarPD und VD und ist fuer die Auto-Refraktor-XDT-Rueckgabe praktisch in MEDISTAR validiert. Das Protokoll steht in `docs/E2E_TESTPROTOKOLL_MEDISTAR_AR360.md`. Der technische Testweg erzeugt beide Pakete reproduzierbar temporaer mit `TemplatePackageExporter`, liest sie mit `TemplatePackageImporter` wieder ein und prueft den sicheren UserDefined-Import. Offizielle ZIPs folgen erst nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md`; danach kommt LM7/LM7P anhand repraesentativer Dateien.

## 16. Lizenzsystem

Das Lizenzsystem ist vorbereitet, aber nicht hart durchgesetzt.

Vorhanden:

- `InstallationInfo`
- Lizenzanfrage exportieren
- Lizenzdatei importieren
- Lizenzstatus anzeigen
- aktive lizenzpflichtige Schnittstellenprofile bewerten
- lizenzierte Geraete/Anbindungen darstellen
- Karenzzeitmodell
- Karenzzeiten speichern/laden

Aktueller Stand:

- keine harte produktive Lizenzsperre
- keine Online-Lizenzierung
- keine produktive digitale Signaturpruefung fuer Lizenzdateien
- Lizenzlogik ist Anzeige-, Bewertungs- und Vorbereitungslogik

## 17. Sicherheitsentscheidungen

Diese Entscheidungen gelten fuer weitere Entwicklung:

- BuiltIn-Profile nicht ueberschreiben
- UserDefined-Profile separat speichern
- keine Verarbeitung beim App-Start
- kein Windows-Dienst
- kein Autostart
- kein `FileSystemWatcher`
- keine unbekannten Dateien anfassen
- keine pauschale Ordnerleerung
- Exportordner nicht bereinigen
- instabile Dateien nicht verarbeiten
- mehrere XDT-Anhaenge nicht automatisch zuordnen
- keine medizinische Bewertung
- keine harte Lizenzsperre ohne gesonderte Spezifikation
- keine dauerhafte Aenderung von Exportprofilen durch Baukasten-Test
- XDT-Längenpraefixe zentral erzeugen, nicht manuell konfigurieren

## 18. Zentrale Dokumente

- `README.md`: Einstieg und kompakter aktueller Stand.
- `CHANGELOG.md`: Versions- und Dokumentationshistorie.
- `VERSION`: aktuelle Versionskennung.
- `Directory.Build.props`: Assembly-/Package-Versionen.
- `docs/ROADMAP.md`: Roadmap, Phasen, Risiken und naechste Schritte.
- `docs/ARCHITEKTUR.md`: Architektur und Bausteine.
- `docs/PFLICHTENHEFT.md`: Anforderungen und Zielbild.
- `docs/GERAETE_BEISPIELE.md`: Geraetebeispiele, SourcePaths und Interface-Manual-Auswertungen.
- `docs/GERAETE_PROFILE_TEMPLATE_MATRIX.md`: kompakte Matrix zu Geraeteprofilen, Templates, Validierungsstand und naechsten Prioritaeten.
- `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_ARK1S.md`: Vorlage fuer Referenzpaket 1 im bestehenden Templatepaket-Format; der Export-/Import-Testweg ist automatisiert abgesichert.
- `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_AR360.md`: Vorlage fuer Referenzpaket 2 MEDISTAR + NIDEK AR360 / AR-360A; Auto-Refraktor-XDT-Rueckgabe und Export-/Import-Testweg sind abgesichert.
- `docs/TEMPLATEPAKET_RELEASE_REGEL.md`: kleine Freigaberegel fuer dauerhaft abgelegte offizielle Templatepaket-ZIPs.
- `docs/E2E_TESTPROTOKOLL_MEDISTAR_AR360.md`: anonymisiertes Praxisprotokoll fuer die AR360-XDT-Rueckgabe.
- `docs/END_TO_END_TESTPLAN.md`: manueller und automatisierter Testplan fuer AIS-/Geraete-/XDT-Anhang-Verarbeitung.
- `docs/PROJEKT_UEBERBLICK.md`: diese kompakte Uebergabedatei.

## 19. Empfohlene naechste Schritte

An `docs/ROADMAP.md` orientierte naechste Schritte:

1. Release-Regel fuer ARK1S und AR360 anwenden, beide Referenzpakete praktisch in der App importieren und danach offizielle ZIP-Paketdateien festlegen.
2. Optional AR360-XDT-Anhangfall separat praktisch pruefen.
3. LM7/LM7P-Dateien sammeln und gegen die dokumentierten SourcePaths validieren.
4. Wenn die Datenlage reicht: fertiges LM7/LM7P-Geraeteprofil plus Templatepaket vorbereiten.
5. Danach NT530P oder TOPCON CL300/KR800/TRK2P nach Datenlage priorisieren.
5. Baukasten schlank halten und nur fuer Sonderfaelle, Tests und Vorschau erweitern.
6. Aktivierungsassistent vorerst als read-only Regressionsstand ruhen lassen.
7. Restliche E2E-Testfaelle praktisch ausfuehren und protokollieren.

## 20. Kompakter Starttext fuer neuen Chat

Der folgende Block kann in einen neuen Chat kopiert werden:

```text
Wir arbeiten an XdtDeviceBridge / XDT Verwaltung, Version 0.1.0-prototype.

XdtDeviceBridge ist eine lokale WPF-Desktop-App fuer eine dateibasierte AIS-/Geraete-Bridge im augenaerztlichen Umfeld. Fokus ist MEDISTAR mit XDT/GDT und NIDEK ARK1S XML. Es gibt keine Cloud, keinen Windows-Dienst, keinen Autostart, keinen FileSystemWatcher und keine Verarbeitung beim App-Start.

Praktisch validiert ist MEDISTAR + NIDEK ARK1S: AIS-GDT/XDT einlesen, NIDEK ARK1S XML einlesen, Patientendaten und Messwerte mappen und eine MEDISTAR-kompatible XDT-Datei erzeugen. Wichtige Felder sind 8000=6310, Patientendaten 3000/3101/3102/3103, 8402 Untersuchungsart und 6228 Ergebniszeilen rechts/links. Am 2026-05-11 wurde zusaetzlich der XDT-Anhang-Link ueber 6302/6303/optional 6304/6305 praktisch validiert; ein externer Anhang konnte aus einer MEDISTAR-Karteikarte geoeffnet werden.

Die Haupt-Tabs sind Verarbeitung, Profile & Templates, Schnittstellenprofile und Lizenz. Verarbeitung ist der Betriebsbereich mit Schnittstellen-Monitor, aktiven Schnittstellenprofilen, manuell startbarer Ueberwachung, automatischer Verarbeitung per Haken, Paketstatus und Monitoring-Ereignissen.

Im Tab Verarbeitung ist die V1 der abdockbaren Geraeteanbindungsfenster praxisabgenommen; das Protokoll liegt in `docs/PRAXISABNAHME_GERAETEFENSTER_V1.md`. Jede Monitoring-Karte kann manuell als eigenes Fenster geoeffnet und wieder angedockt werden. Relevante Monitoring-Aktivitaet oeffnet automatisch nur das betroffene Floating-Fenster und bringt es nach vorne; der App-Content-Signalton `04_praxis_terminal_signal.wav` wird nur bei stabil erkanntem Geraetedatei-Eingang mit kurzem Cooldown pro Schnittstellenprofil abgespielt. AIS-, Anhang-, Export- und Statusmeldungen bleiben stumm. Nach terminalem Abschluss dockt ein automatisch geoeffnetes, nicht gepinntes Fenster nach 5 Sekunden Restlaufzeit wieder an; neue Aktivitaet, Pin oder manuelles Andocken beendet den Countdown pro Profil. Schliessen per `X` dockt Floating-Fenster sicher zurueck. Der Reset `↺` verwirft nach Sicherheitsabfrage den aktuellen Vorgang genau eines Schnittstellenprofils, bricht interne Wartezustaende ab und leert nur die AIS-/Geraete-/optionalen XDT-Anhang-Eingangsordner dieses Profils top-level; Export-, Archiv- und Fehlerordner sowie Unterordner bleiben unangetastet. Pin, Radar, `-`/`+`-Scanintervallsteuerung und Positionsmerken arbeiten pro Schnittstellenprofil; Position/Groesse und Abdockstatus werden als UI-State unter AppData gespeichert, wenn Positionsmerken aktiv ist. Beim App-Start werden gespeicherte Floating-Fenster erst nach der sicheren MainWindow-Anzeige wiederhergestellt; ein Restore-Fehler dockt die Karte sicher an. Die Systray-Grundfunktion ist vorhanden: Minimieren oder `X` am Hauptfenster blendet die App in den Infobereich aus, Doppelklick beziehungsweise `Oeffnen` stellt das Hauptfenster wieder her, `Beenden` schliesst bewusst. Die Floating-Fenster sind leicht verbreitert, damit die Symbolleiste `🗗 ↺ 📌 🔝` in einer Zeile bleibt. Autostart, Windows-Dienst, UI-Einstellung fuer Rueckdock-Zeit, sichtbarer Countdown-Hinweis, Ton-Schalter und eigenes Systray-Icon sind Komfortthemen fuer spaeteres Praxisfeedback.

Profile & Templates ist der Profil- und Templatebereich. Standardziel ist fertiges Geraeteprofil plus fertiges Templatepaket; der Baukasten Test & Vorschau bleibt fuer Sonderfaelle, Tests und kundenspezifische Anpassungen.

XDT-Anhaenge fuer AIS sind fuer den validierten MEDISTAR/ARK1S-Pflicht-Anhang-Praxislauf praktisch bestaetigt. Unterstuetzt sind PDF, JPG, JPEG, PNG, TIF, TIFF, DCM und TXT. Externe Linkfelder sind 6302 Dokumentenname, 6303 Dateiformat, 6304 Beschreibung optional und 6305 vollstaendiger Dateipfad. XDT-Längenpraefixe erzeugt der XdtExportBuilder zentral, nicht die UI.

Im Baukasten kann ein XDT-Anhang aus beliebigem Speicherort gewaehlt werden. Vorschau und Test-XDT simulieren aber den Schnittstellenprofil-Zielpfad: 6305 zeigt auf XDT-Anhang Exportordner plus erzeugten Dateinamen. Der Testexport schreibt XDT-Datei und umbenannten Anhang physisch in einen frei gewaehlten Testordner, ohne Exportprofile oder BuiltIns zu veraendern.

Die automatische Paketlogik ist zweistufig: Phase 1 AIS-Datei wartet auf stabile Geraetedatei, Default 10 Minuten. Eine neue AIS-Datei ersetzt eine aeltere wartende AIS-Datei. Phase 2 startet erst nach vollstaendigem AIS-/Geraete-Paar: optionaler oder verpflichtender XDT-Anhang wartet bis Default 30 Sekunden.

Optionaler XDT-Anhang bedeutet: Wenn genau ein stabiler Anhang rechtzeitig kommt, Export mit 6302-6305; wenn keiner kommt, Export ohne Anhang nach Timeout; mehrere Anhaenge werden nicht automatisch zugeordnet. Pflicht bedeutet: ohne eindeutigen Anhang blockiert die Verarbeitung oder geht in Fehlerstatus.

Dateistabilitaet ist wichtig: AIS-, Geraete- und Anhangdateien werden erst verarbeitet, wenn sie stabil und lesbar sind. Default fuer XDT-Anhang-Stabilitaet ist 2 Sekunden. Das Scan-Intervall ist pro Schnittstellenprofil konfigurierbar, Default 5 Sekunden.

Profile sind JSON-basiert unter %LocalAppData%\XdtDeviceBridge\profiles. BuiltIn-Profile duerfen nicht ueberschrieben oder geloescht werden, UserDefined-Profile werden separat gespeichert. UserDefined-Exportprofile koennen geloescht werden, wenn kein Schnittstellenprofil sie verwendet; Exportregeln koennen nur aus UserDefined-Exportprofilen entfernt werden. Templatepaket-Export erfolgt selektiv auf Basis eines Schnittstellenprofils und nimmt nur benoetigte AIS-/Geraete-/Export-Abhaengigkeiten auf. Templatepaket-Import, Validierung, Konfliktanalyse, Importplan, Dry-Run, UI-Vorschau, sichere Benutzerwahl und explizite UserDefined-Uebernahme sind vorhanden. ReplaceExisting bleibt offen. Importierte Schnittstellenprofile werden nicht automatisch aktiviert; IsAttachmentProcessingEnabled wird deaktiviert.

Der Aktivierungsassistent fuer importierte Schnittstellenprofile ist read-only vorbereitet und ruht vorerst. Im Tab Schnittstellenprofile gibt es Pruefung vor Aktivierung und den Vorschau-Dialog Aktivierung vorbereiten. Die Service-Kette lautet Evaluation -> Guard -> PreparationPreview. Angezeigt werden V1-relevante Vorschauinformationen: Status, Aktivierbarkeit nach V1, technische Freigabe, Blocker, Warnungen, Hinweise und Sicherheitshinweis. Es gibt keinen Aktivieren-Button, keine produktive Warnungsbestaetigung, keine Speicherung, keine Profiländerung und keine Datei-/Ordneroperation.

Vorbereitete, aber nicht produktiv validierte Geraeteprofile: NIDEK LM7/LM7P, NIDEK NT530P, TOPCON CL300, TOPCON KR800, TOPCON TRK2P. NIDEK AR360 / AR-360A ist fuer die Auto-Refraktor-XDT-Rueckgabe praktisch validiert; offen bleiben XDT-Anhangfall und offizielles ZIP-Artefakt. Die Matrix `docs/GERAETE_PROFILE_TEMPLATE_MATRIX.md` fuehrt Status, Tests, Templatepaket-Luecken und naechste Prioritaeten. Die Vorlagen `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_ARK1S.md` und `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_AR360.md` beschreiben die beiden Referenzpakete. Der Testweg erzeugt und prueft die Paket-ZIPs temporaer ueber den selektiven Exporter/Importer-Pfad; dauerhaft abgelegt werden sie erst nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md`.

Lizenzsystem: InstallationInfo, Lizenzanfrage, Lizenzimport, Statusanzeige, Bewertung lizenzpflichtiger aktiver Schnittstellenprofile und Karenzzeitmodell sind vorbereitet. Es gibt noch keine harte Lizenzsperre, keine Online-Lizenzierung und keine produktive Signaturpruefung.

Wichtige Sicherheitsregeln: keine unbekannten Dateien anfassen, keine pauschale Ordnerleerung, Exportordner nicht bereinigen, instabile Dateien nicht verarbeiten, mehrere XDT-Anhaenge nicht automatisch zuordnen, keine medizinische Bewertung, BuiltIns nicht ueberschreiben.

Zentrale Dokumente: README.md, CHANGELOG.md, docs/ROADMAP.md, docs/ARCHITEKTUR.md, docs/PFLICHTENHEFT.md, docs/GERAETE_BEISPIELE.md, docs/GERAETE_PROFILE_TEMPLATE_MATRIX.md, docs/TEMPLATEPAKET_MEDISTAR_NIDEK_ARK1S.md, docs/TEMPLATEPAKET_MEDISTAR_NIDEK_AR360.md, docs/TEMPLATEPAKET_RELEASE_REGEL.md, docs/PRAXISABNAHME_GERAETEFENSTER_V1.md, docs/END_TO_END_TESTPLAN.md und docs/PROJEKT_UEBERBLICK.md.

Naechste sinnvolle Schritte: Geraeteanbindungsfenster V1 als abgenommenen Block beibehalten und nur Komfortthemen nach Praxisfeedback priorisieren; Release-Regel fuer ARK1S- und AR360-ZIP-Artefakte anwenden, optional den AR360-XDT-Anhangfall separat testen, LM7/LM7P-Dateien validieren, daraus ein fertiges LM7/LM7P-Paket vorbereiten, danach NT530P oder TOPCON-Profile nach Datenlage priorisieren, Aktivierungsassistent vorerst ruhen lassen.
```
