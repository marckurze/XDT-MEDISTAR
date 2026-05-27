# Projektueberblick XdtDeviceBridge / XDT Verwaltung

Stand: 2026-05-24

Diese Datei dient als kompakte Uebergabe fuer neue Chats, spaetere Codex-Sessions und Projektplanung. Sie trennt aktuellen Iststand, validierten Kernworkflow, vorbereitete Bausteine und Zielbild.

## 1. Kurzbeschreibung

XdtDeviceBridge ist eine lokale WPF-Desktop-App fuer die Bridge zwischen Arztinformationssystemen und augenaerztlichen Untersuchungsgeraeten. Der Produktname fuer den spaeteren Einsatz ist XDTBox.

Der Fokus liegt aktuell auf:

- MEDISTAR als AIS/PVS-Ziel
- GDT/XDT-Dateien als Austauschformat
- augenaerztlichen Messgeraeten ueber den bisherigen LAN-/UNC-Dateiworkflow und vorbereitet ueber serielle RS232-/COM-Port-Kommunikation
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
- schlanke Anlage neuer AIS-, Geraete- und Exportprofile als UserDefined
- sichere Anlage neuer Schnittstellenprofile als inaktive UserDefined-Kombination aus AIS-, Geraete- und Exportprofil
- sichere UserDefined-Wartung fuer Exportprofile und Exportregeln
- RS232-/COM-Port-Testfunktion fuer zeitlich begrenzte Rohdaten-Mitschnitte mit Text-/Hexanzeige und optionaler NIDEK-RS232-Auswertung
- Platzhalter
- Templatepaket-Export per ausgewaehltem Schnittstellenprofil
- Templatepaket-Import mit Validierung, Importvorschau, Benutzerwahl und sicherer UserDefined-Uebernahme
- Baukastenbereich `Test & Vorschau`

Im Tab `Schnittstellenprofile` ist der Button `Neues Schnittstellenprofil anlegen` funktionsfaehig: Er oeffnet einen Dialog fuer Profilname, AIS-Profil, Geraeteprofil und Exportprofil. Das neue Profil wird als UserDefined gespeichert, bleibt initial inaktiv und enthaelt keine automatisch erfundenen Ordnerpfade. Ordner, Nachlauf, CV-5000-Ausgabe-an-Geraet und Dokumentanhaenge werden danach im bestehenden Konfigurationsbereich gepflegt.

Im selben Tab kann der Anwender Ordnerpfade vorbereiten, ohne Fachlogik zu aktivieren: `Ordner Default` fuellt Standardpfade unter `C:\XDTBox\<Geraetename>` in die aktuell sichtbaren Pfadfelder, `Ordner anlegen` erstellt die eingetragenen Ordner und meldet Erfolg oder konkrete Pfad-/Berechtigungsfehler. Es werden keine Dateien angelegt, geloescht oder verarbeitet.

Der Bereich `Test & Vorschau` erlaubt:

- AIS-Datei laden
- Geraetedatei laden
- XDT-Anhang einlesen
- Messwerte pruefen
- Gesamtexport-Vorschau XDT pruefen
- Testexport erstellen

Die RS232-Testfunktion kann fuer NIDEK-RS232-Rohdaten Frames mit SOH/STX/ETB/EOT, optionale NCP10-Checksummen, Header, Segmente, Modellinformationen und erste Messwertkandidaten anzeigen. LM-Rohdaten werden nur als `6228`-Kandidaten, NT-Tonometrie nur als `6205`-Kandidaten und PM/Pachymetrie nur als `6220`-Kandidaten markiert. Daraus entsteht noch kein automatischer produktiver XDT-Export.

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

Neue Profile koennen im Tab `Profile & Templates` als V1-Funktion angelegt werden: AIS-Profile ueber Name, System und Codierung, Geraeteprofile ueber Name, Hersteller, Modell, Geraetetyp und vorhandene Parserbasis. Die Dialoge enthalten kurze Hilfetexte, damit MEDISTAR/Generisch, Windows-1252, Geraetetyp und Parserbasis verstaendlich bleiben. Exportprofile werden sichtbar als leerer Entwurf mit eindeutigem Namen vorbereitet und erst nach bewusstem Speichern als UserDefined geschrieben. Name-/ID-Konflikte werden blockiert; BuiltIn-Profile, Schnittstellenprofile, Aktivierung und Verarbeitung bleiben unberuehrt.

UserDefined-Profile koennen umbenannt werden, ohne ihre technische Funktion zu veraendern. Unterstuetzt sind AIS-, Geraete-, Export- und Schnittstellenprofile; geaendert wird nur der sichtbare Name. IDs, Referenzen, Ordnerpfade, Exportregeln, XDT-Anhang-Einstellungen, Aktivierungsstatus und Verarbeitung bleiben unveraendert. BuiltIn-Profile bleiben gegen Umbenennung geschuetzt. Templatepaket-ZIP-Dateien werden nicht als eigene App-Objekte verwaltet; Paketbenennung erfolgt weiterhin ueber Dateiname und Release-Regel.

## 8. Tab Schnittstellenprofile

Ein Schnittstellenprofil verknuepft AIS-Profil, Geraeteprofil, Exportprofil, Ordner, Archivierung, Fehlerablage, Automatik- und XDT-Anhang-Einstellungen.

Geraeteprofile koennen als Quelle `NetworkLan` oder `SerialRs232` tragen. `NetworkLan` ist der bestehende Datei-/UNC-Workflow mit `Geraetedatei an XDTBox`; `SerialRs232` ersetzt diesen Geraeteordner durch eine COM-Port-Konfiguration. AIS-Patientendatei, Ergebnisdatei an AIS, Archiv und Fehler bleiben auch bei RS232 Teil des Schnittstellenprofils.

Aktuell dokumentierte Felder:

- AIS-Importordner
- Geraete-Importordner
- bei RS232 statt Geraete-Importordner: COM-Port, Baudrate, Datenbits, Stoppbits, Paritaet, Flusskontrolle, bidirektionale Option und Timeouts
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
- Die Pruefung zeigt Status, Aktivierbarkeit, Blocker-/Warnungs-/Hinweiszaehler, strukturierte Ordnerpruefung mit Erreichbarkeit, strukturierte XDT-Anhang-Konfiguration und die eingeklappte Tabelle `Alle Pruefpunkte`. Sie nutzt die aktuellen UI-Entwurfswerte auch vor dem Speichern.
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
- XML
- MP4
- MP3
- WAV

Externe AIS-Linkfelder:

- `6302`: Dokumentenname / Anzeige in Karteikarte
- `6303`: Dateiformat, z. B. `PDF`, `JPG`, `DCM`, `TXT`
- `6304`: Beschreibung, optional
- `6305`: vollstaendiger absoluter Dateipfad

Fuer Dokumentgeraete ohne Messwerte gibt es den V1-Kandidaten `MEDISTAR + Dokumentanhang`. Er behandelt Dateien aus dem Geraete-/Dokument-Importordner als reine Anhaenge und erzeugt keine Messwertfelder wie `6228`, `6205` oder `6220`. Im manuellen Dialog gehoert die Beschreibung direkt zur jeweiligen Datei und wird als `6304` ausgegeben; ohne Eingabe wird der Originaldateiname verwendet. `6305` bleibt der technische Zielpfad.

Fuer rein manuelle Arbeitsablaeufe gibt es zusaetzlich `MEDISTAR + Manuelle Dokumentuebergabe`. Erst eine AIS-Datei startet dabei den Dialog `Dokumente uebertragen` ohne erwartete Geraetedatei; das gruene Geraeteanbindungsfenster bleibt fuer diesen Sondermodus angedockt. Der Anwender fuegt Dateien per Drag-&-Drop, Dateiauswahl oder Datei-Zwischenablage hinzu. Nach `Uebertragen` wird der interne State fuer den naechsten AIS-Vorgang freigegeben. Manuell ausgewaehlte Quelldateien werden nicht geloescht.

Der Dialog `Dokumente uebertragen` wird waehrend des Pollings nicht mehr vollstaendig neu aufgebaut. Neue Dateien werden inkrementell ergaenzt, vorhandene Beschreibungen bleiben erhalten, die Liste nutzt den verfuegbaren Platz, und die automatische Paarverarbeitung setzt erst nach `Uebertragen` fort. Das Dialogfenster ist standardmaessig TopMost, nutzt einen kompakten `🔝`-Schalter und ist optisch an die gruenen Geraeteanbindungsfenster angelehnt.

Technische Grundsaetze:

- Der `XdtExportBuilder` erzeugt XDT-Längenpraefixe zentral.
- In UI und Konfiguration werden keine manuellen Längenpraefixe gepflegt.
- Automatische XDT-Anhang-Verarbeitung ist nur unter Sicherheitsbedingungen erlaubt.
- Mehrere stabile unterstuetzte Anhaenge werden stabil sortiert und einzeln verarbeitet.
- Instabile Anhaenge werden nicht verarbeitet, nicht verschoben und nicht verlinkt.
- Nach erfolgreichem Export mit Anhaengen wird der Vorgang terminal abgeschlossen; bekannte AIS-/Geraetedateien werden gemaess Profilregel nachbehandelt und alte Eingangsanzeigen/Timeouts werden nicht als aktiver Kartenstatus beibehalten.
- Exportprofile werden durch XDT-Anhang-Test und Testexport nicht dauerhaft veraendert.
- MEDISTAR + NIDEK ARK1S + XDT-Anhang-Link ist fuer den Pflicht-Anhang-Praxislauf praktisch validiert; weitere Geraete/AIS und Mehrfachanhang-Livelaeufe bleiben separat zu pruefen.

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

- Wenn ein oder mehrere stabile unterstuetzte Anhaenge innerhalb Timeout kommen: Export mit `6302`, `6303`, optional `6304`, `6305` je Datei.
- Wenn kein Anhang kommt: Export ohne Anhang nach Timeout.
- Wenn vorhandene Anhaenge noch instabil sind: weiter warten, solange die Wartezeit laeuft.

Pflicht-XDT-Anhang:

- Wenn ein oder mehrere stabile unterstuetzte Anhaenge kommen: Export mit `6302`, `6303`, optional `6304`, `6305` je Datei.
- Wenn kein Anhang kommt: Blockade/Fehler.
- Wenn vorhandene Anhaenge noch instabil sind: weiter warten; nach Timeout Blockade/Fehler.

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
- Wenn die Profiloption `aus Importordner entfernen` aktiv ist, werden nur diese bekannten verarbeiteten AIS-/Geraetedateien entfernt beziehungsweise ins Archiv verschoben.
- Eine fruehere Verarbeitung, gleicher Pfad, gleicher Dateiname, Fingerprint oder Zeitstempel ist keine Verarbeitungssperre mehr.
- Erfolgreich verarbeitete Dateipaare werden gemaess Archiv-, Fehlerordner- oder Entfernen-Regel nachbehandelt. Bleiben sie im Importordner liegen, koennen sie beim naechsten Scan erneut verarbeitet werden; das ist eine bewusste Folge der Profilkonfiguration.
- Keine unbekannten Dateien werden geloescht.
- Keine Ordner werden pauschal geleert.
- Der Exportordner wird nicht bereinigt.
- Fehlerablage verschiebt die beteiligten bekannten Dateien und erzeugt eine `error.txt`.

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
- koennen fuer AIS, Geraete und Exportprofile schlank aus der App heraus angelegt werden

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
- NIDEK LM7/LM7P: praktisch validierter Lensmeter-Referenzkandidat mit echter XML-Fixture, `Sphare`/`Sphere`-Toleranz, MEDISTAR-Lensmeter-Ausgabe, Reparatur alter persistierter BuiltIn-Exportpfade, MEDISTAR-Praxisprotokoll und selektivem Templatepaket-Test
- NIDEK NT530P: testseitig direkt nutzbarer Tonometrie-/Pachymetrie-Kandidat mit echter XML-Fixture, korrigiertem mehrzeiligem `6205`-/`6220`-Export, Mehrfachanhang-Linkfeldern, korrigiertem Nachlauf/Monitoring-Reset und selektivem Templatepaket-Test; praktische MEDISTAR-Nachpruefung offen
- MEDISTAR + Dokumentanhang: AttachmentOnly-/Dokumentgeraete-V1-Kandidat mit pro-Datei-Beschreibung ueber `6304`, mehreren `6302`-`6305`-Anhaengen, XML/MP4/MP3/WAV als reinen Anhaengen und selektivem Templatepaket-Test; praktische MEDISTAR-Abnahme offen
- MEDISTAR + Manuelle Dokumentuebergabe: AIS-gestarteter V1-Kandidat fuer manuell ausgewaehlte Dateien mit Drag-&-Drop/Dateiauswahl/Zwischenablage, pro-Datei-`6304`, sicherer Copy-Uebergabe und ohne Geraete-Importordner; praktische MEDISTAR-Abnahme offen
- TOPCON CL300: erster praktisch validierter TOPCON-Lensmeter-Referenzkandidat mit echten XML-Fixtures, namespace-toleranter LM-Erkennung, `6228`-Ausgabe inklusive ADD, PD und signierten H/V-Prismenkomponenten
- TOPCON KR800S: praktisch validierter Mehruntersuchungs-Referenzkandidat mit echten Shift-JIS-XML-Fixtures, `6228` REF, `6221` KM, konservativer `6227` SBJ-Ausgabe, Templatepaket-Test, Reparatur alter persistierter BuiltIn-Exportprofile und Praxisprotokoll
- TOPCON KR-1: fixture-validierter Keratorefraktometer-Kandidat mit echter Shift-JIS-Serial0001-Fixture, `6228` REF aus Medianwerten, eigenen BuiltIns, selektivem Templatepaket-Test und Fixture-Protokoll; KM/KRT/periphere KRTs bleiben ohne echte KM/KRT-Fixture offen
- TOPCON TRK2P: praktisch validierter Mehruntersuchungs-Referenzkandidat mit echten XML-Fixtures inklusive TM/CCT-only-Teilmessung, `6228` REF, `6221` KM, `6220` CCT/Pachy, `6205` Tono, optionalem `6227` SBJ-Pfad, Templatepaket-Test, Praxisprotokoll und Repair alter persistierter BuiltIns
- TOPCON CT1P: praktisch validierter Tonometrie-/Pachymetrie-Referenzkandidat mit echter Serial0214-XML-Fixture, `6205` Tonometrie, `6220` Pachymetrie, per Auge optionalem CorrectedIOP/CCT, BuiltIn-Profilen, selektivem Templatepaket-Test und Praxisprotokoll; das Geraet arbeitet fachlich vollautomatisch beidseitig, unvollstaendige XML-Bloecke werden ohne leere Fragmente weggelassen
- TOPCON CT800A: fixture-validierter Non-Contact-Tonometer-Kandidat mit drei echten CT-800A-XML-Fixtures, `6205` Tonometrie, vollstaendiger CorrectedIOP/CCT-Detailausgabe nur bei verwertbaren per-eye-Daten, BuiltIn-Profilen, selektivem Templatepaket-Test und Fixture-Protokoll; praktische MEDISTAR-Abnahme offen
- TOPCON CV5000 / CV-5000S: erster bidirektionaler Phoropter-Kandidat mit AIS-Historienparser fuer MEDISTAR-Karteikartenzeilen, CV-5000-Import-XML-Writer, Rueckgabeparser, fachlich getrennter Rueckgabe (`Prescription` vollstaendig ueber `6228`, `Full Correction` vollstaendig ueber `6227`, keine `6330`-Zeilen), BuiltIn-Profilen, Schnittstellenprofil-Konfiguration fuer Ausgabe an Geraet, selektivem Templatepaket-Test und Fixture-Protokoll; Rueckweg mit MEDISTAR-Historien-AIS ist abgesichert, finaler MEDISTAR-Import der Rueckgabedatei praktisch offen

Wichtig: Diese V2-/BuiltIn-Profile sind vorbereitet und konfigurierbar; TOPCON CL300, TOPCON KR800S, TOPCON TRK2P und TOPCON CT1P sind inzwischen praktisch validiert. Fuer TRK2P ist zusaetzlich die Teilmessungsregel mit einer TM/CCT-only-Datei bestaetigt. TOPCON KR-1 ist testseitig als Keratorefraktometer-Kandidat mit REF-`6228` vorbereitet; KM/KRT bleibt bis zu einer echten Fixture offen. TOPCON CT800A ist testseitig als Non-Contact-Tonometer-Kandidat vorbereitet. TOPCON CV5000 ist testseitig als bidirektionaler Kandidat vorbereitet, aber noch nicht live validiert.

Die kompakte Bestandsaufnahme steht in `docs/GERAETE_PROFILE_TEMPLATE_MATRIX.md`. ARK1S ist Referenzpaket 1 in `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_ARK1S.md`; AR360 ist Referenzpaket 2 in `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_AR360.md`. AR360 nutzt ARMedian, FarPD und VD und ist fuer die Auto-Refraktor-XDT-Rueckgabe praktisch in MEDISTAR validiert. LM7 ist als dritter Referenzkandidat in `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_LM7.md` vorbereitet und fuer die Lensmeter-XDT-Rueckgabe praktisch validiert; das Protokoll steht in `docs/E2E_TESTPROTOKOLL_MEDISTAR_LM7.md`. NT530P ist als Templatepaket-Kandidat in `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_NT530P.md` dokumentiert und nutzt korrigierte `6205`-/`6220`-Zeilen statt `6228`. TOPCON CL300 ist in `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_CL300.md` dokumentiert, nutzt `6228`-Lensmeterzeilen aus echten CL-300-XML-Fixtures inklusive ADD, PD und signierten H/V-Prismenkomponenten und ist in `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_CL300.md` praktisch validiert. TOPCON KR800S ist in `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_KR800S.md` dokumentiert, nutzt echte Shift-JIS-XML-Fixtures mit REF `6228`, KM `6221` und konservativen SBJ-`6227`-Zeilen und ist in `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_KR800S.md` praktisch validiert; alte persistierte KR800S-BuiltIn-Exportprofile mit Einzelplatzhaltern oder KM ueber `6228` werden repariert. TOPCON KR-1 ist in `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_KR1.md` dokumentiert, nutzt eine echte Shift-JIS-Serial0001-Fixture mit REF-Medianwerten ueber `6228` und ist in `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_KR1.md` fixture-validiert; KM/KRT/periphere KRTs bleiben ohne echte KM/KRT-Datei offen. TOPCON TRK2P ist in `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_TRK2P.md` dokumentiert, nutzt echte XML-Fixtures mit REF `6228`, KM `6221`, CCT/Pachy `6220`, Tono `6205` und optionalem SBJ-`6227`-Pfad, ist in `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_TRK2P.md` praktisch validiert und bestaetigt auch TM/CCT-only-Teilmessungen ohne REF/KM/SBJ; alte persistierte TRK2P-BuiltIns mit root-prefixed oder alten TM-/CCT-Pfaden werden repariert. Das generische Dokumentgeraet ist in `docs/TEMPLATEPAKET_MEDISTAR_DOKUMENTANHANG.md` dokumentiert und nutzt pro Datei `6302` bis `6305`, inklusive sichtbarer Beschreibung ueber `6304`. Die manuelle Dokumentuebergabe ist in `docs/TEMPLATEPAKET_MEDISTAR_MANUELLE_DOKUMENTUEBERGABE.md` dokumentiert und nutzt dieselbe Linklogik mit manueller Dateiauswahl. Alte persistierte BuiltIn-LM7-Exportprofile und alte CL300-BuiltIns mit root-prefixed `Ophthalmology`-Pfaden werden gezielt auf die passenden `MedistarLine`-Parserpfade repariert. Der technische Testweg erzeugt die Pakete reproduzierbar temporaer mit `TemplatePackageExporter`, liest sie mit `TemplatePackageImporter` wieder ein und prueft den sicheren UserDefined-Import. Offizielle ZIPs folgen erst nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md`.

TOPCON CT1P ist in `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_CT1P.md` als Tonometrie-/Pachymetrie-Referenzkandidat dokumentiert und in `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_CT1P.md` praktisch validiert. Die Serial0214-Fixture liefert TM-Listen, rechts vollstaendige CorrectedIOP-/CCT-Werte und links nur unvollstaendige CorrectedIOP-Parameter; der Export erzeugt deshalb `6205`-Tonometrie und rechtsseitige `6220`-Pachymetrie ohne leere linke Fragmente.

TOPCON CT800A ist in `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_CT800A.md` als Non-Contact-Tonometer-Kandidat dokumentiert; das Fixture-Protokoll steht in `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_CT800A.md`. Drei echte CT-800A-Dateien decken TM-Listen/Average, vollstaendige CorrectedIOP/CCT-Detailwerte in Serial0069 und ausgelassene unvollstaendige CorrectedIOP-/ERROR-Bloecke in Serial0070/Serial0071 ab. Der Export nutzt `6205` Tonometrie und erzeugt fuer CT-800A bewusst keine separate `6220`-Pachymetrie.

TOPCON CV5000 ist in `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_CV5000.md` als erster bidirektionaler Phoropter-Kandidat dokumentiert. Die Richtung AIS/MEDISTAR -> Phoropter nutzt den Historienparser fuer MEDISTAR-Karteikartenzeilen und den CV-5000-Importwriter; Ausgabeordner, Dateiname und Format werden im Schnittstellenprofil im Bereich `Ausgabe an Geraet` gepflegt. Die Richtung Phoropter -> AIS/MEDISTAR nutzt den CV-5000-Rueckgabeparser: `Prescription` wird als finaler Verordnungswert vollstaendig ueber `6228` ausgegeben, `Full Correction` als Maximalwert/Vollkorrektion vollstaendig ueber `6227`; `6330` wird nach praktischem MEDISTAR-Importtest nicht mehr verwendet. MEDISTAR-Historien-AIS-Dateien werden dabei im CV-5000-Kontext tolerant als Patientenkontext gelesen. Das Fixture-Protokoll steht in `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_CV5000.md`; finaler MEDISTAR-Import der Rueckgabedatei und ExamDistance-Bestaetigung bleiben offen.

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

Der konkrete Produktivplan steht in `docs/LIZENZIERUNG_PRODUKTIVPLAN.md`. Fuer XDTBox V1 wird dort eine offlinefaehige signierte JSON-Lizenz mit asymmetrischer Signatur, datensparsamer Installation-ID, `MaxActiveDeviceConnections`, 7-Tage-Karenz und zentralen Gates vor Aktivierung beziehungsweise Start der Verarbeitung festgelegt. Lizenzpflichtig ist ausschliesslich die Anzahl aktiver Geraeteanbindungen; `NetworkLan`, `SerialRs232`, Dokumentanhang, Profile/Templates und Testbereiche sind keine separaten Lizenzmodule. Parser und XDT-Ausgabe sollen nicht direkt lizenzseitig abbrechen. InstallationId bleibt fuehrend; bei Hardwaretausch wird eine neue Lizenzanforderung erzeugt.

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
- mehrere XDT-Anhaenge nur als einzelne stabile unterstuetzte Dateien mit eigenen Linkfeldgruppen uebergeben
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
- `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_LM7.md`: Templatepaket-Kandidat fuer MEDISTAR + NIDEK LM7 / LM-7P; echte XML-Fixture, Export-/Import-Testweg und praktische Lensmeter-XDT-Rueckgabe sind abgesichert.
- `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_NT530P.md`: Templatepaket-Kandidat fuer MEDISTAR + NIDEK NT530P / NT-530P; echte XML-Fixture, korrigierter `6205`-/`6220`-Export und Export-/Import-Testweg sind abgesichert.
- `docs/TEMPLATEPAKET_MEDISTAR_DOKUMENTANHANG.md`: Templatepaket-Kandidat fuer Dokumentgeraete ohne Messwerte; pro-Datei-Beschreibung ueber `6304` und mehrere `6302`-`6305`-Anhaenge sind testseitig abgesichert.
- `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_CL300.md`: Templatepaket-Kandidat fuer MEDISTAR + TOPCON CL300; echte CL-300-XML-Fixtures, `6228`-Lensmeterzeilen und Export-/Import-Testweg sind abgesichert.
- `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_KR800S.md`: Templatepaket-Kandidat fuer MEDISTAR + TOPCON KR800S; echte Shift-JIS-XML-Fixtures, REF-`6228`, KM-`6221`, SBJ-`6227` und Export-/Import-Testweg sind abgesichert.
- `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_KR1.md`: Templatepaket-Kandidat fuer MEDISTAR + TOPCON KR-1; echte Shift-JIS-Serial0001-Fixture, REF-Medianwerte ueber `6228` und Export-/Import-Testweg sind abgesichert.
- `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_TRK2P.md`: Templatepaket-Kandidat fuer MEDISTAR + TOPCON TRK2P; echte XML-Fixtures, REF-`6228`, KM-`6221`, Pachy-`6220`, Tono-`6205`, optionaler SBJ-`6227`-Pfad, Teilmessungen und Export-/Import-Testweg sind abgesichert.
- `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_CT1P.md`: Templatepaket-Kandidat fuer MEDISTAR + TOPCON CT1P; echte XML-Fixture, Tonometrie-`6205`, Pachymetrie-`6220`, einseitig unvollstaendige CorrectedIOP-Daten, Praxisprotokoll und Export-/Import-Testweg sind abgesichert.
- `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_CT800A.md`: Templatepaket-Kandidat fuer MEDISTAR + TOPCON CT800A; drei echte XML-Fixtures, Tonometrie-`6205`, vollstaendige CorrectedIOP/CCT-Detailzeilen nur bei verwertbaren Daten und Export-/Import-Testweg sind abgesichert.
- `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_CV5000.md`: Templatepaket-Kandidat fuer MEDISTAR + TOPCON CV5000 / CV-5000S; AIS-Historienparser, CV-5000-Importwriter, Rueckgabeparser, `6228`-Prescription-Rueckgabe, `6227`-Full-Correction-Rueckgabe und Export-/Import-Testweg sind abgesichert.
- `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_SOLOS.md`: Templatepaket-Kandidat fuer MEDISTAR + TOPCON Solos; echte Solos-Lensmeter-XML-Fixture, `6228`-Lensmeter-Ausgabe, PD/H-Prisma und Export-/Import-Testweg sind abgesichert.
- `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_CL300.md`: anonymisiertes Praxisprotokoll fuer die TOPCON-CL300-Lensmeter-XDT-Rueckgabe.
- `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_KR800S.md`: anonymisiertes Praxisprotokoll fuer die TOPCON-KR800S-REF-/KM-/SBJ-XDT-Rueckgabe.
- `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_KR1.md`: Fixture-Protokoll fuer TOPCON KR-1 REF-XML und `6228`-Rueckgabe aus Medianwerten.
- `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_TRK2P.md`: anonymisiertes Praxisprotokoll fuer die TOPCON-TRK2P-REF-/KM-/TM-/CCT-XDT-Rueckgabe inklusive TM/CCT-only-Teilmessung.
- `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_CT1P.md`: anonymisiertes Praxisprotokoll fuer die TOPCON-CT1P-Tonometrie-/Pachymetrie-XDT-Rueckgabe.
- `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_CT800A.md`: Fixture-Protokoll fuer die TOPCON-CT800A-Tonometrie-XDT-Rueckgabe.
- `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_CV5000.md`: Fixture-Protokoll fuer den bidirektionalen TOPCON-CV5000-Kandidaten, mit AIS-Historienimport, CV-5000-Import-XML und fachlich getrennter `6228`-/`6227`-Rueckgabe.
- `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_SOLOS.md`: Fixture-Protokoll fuer TOPCON Solos Lensmeter-XML und `6228`-Rueckgabe.
- `docs/TEMPLATEPAKET_RELEASE_REGEL.md`: kleine Freigaberegel fuer dauerhaft abgelegte offizielle Templatepaket-ZIPs.
- `docs/E2E_TESTPROTOKOLL_MEDISTAR_AR360.md`: anonymisiertes Praxisprotokoll fuer die AR360-XDT-Rueckgabe.
- `docs/E2E_TESTPROTOKOLL_MEDISTAR_LM7.md`: anonymisiertes Praxisprotokoll fuer die LM7-Lensmeter-XDT-Rueckgabe.
- `docs/END_TO_END_TESTPLAN.md`: manueller und automatisierter Testplan fuer AIS-/Geraete-/XDT-Anhang-Verarbeitung.
- `docs/PROJEKT_UEBERBLICK.md`: diese kompakte Uebergabedatei.

## 19. Empfohlene naechste Schritte

An `docs/ROADMAP.md` orientierte naechste Schritte:

1. Release-Regel fuer ARK1S und AR360 anwenden, beide Referenzpakete praktisch in der App importieren und danach offizielle ZIP-Paketdateien festlegen.
2. Optional AR360-XDT-Anhangfall separat praktisch pruefen.
3. LM7/LM7P als praktisch validierten Referenzkandidaten beibehalten.
4. Weitere LM7-Prisma-/PD-Faelle sammeln und danach ueber offizielles ZIP nach Release-Regel entscheiden.
5. Danach NT530P praktisch in MEDISTAR mit korrigierter `6205`/`6220`-Anzeige und JPG-Mehrfachanhaengen validieren; TOPCON CT1P als praktisch validierten `6205`/`6220`-Referenzkandidaten beibehalten und eine echte beidseitige CorrectedIOP/CCT-Datei sammeln; TOPCON KR-1 praktisch mit `6228`-REF und TOPCON CT800A praktisch mit `6205`-Tonometrie abnehmen; TOPCON CL300, TOPCON KR800S und TOPCON TRK2P als praktisch validierte Referenzkandidaten beibehalten, H/V-Prisma sowie weitere SBJ-Funktionstestfaelle beobachten.
6. TOPCON CV5000 bidirektional im Livebetrieb pruefen: Auswahl historischer MEDISTAR-Messwerte, CV-5000-Import-XML, Einlesen am Phoropter und Rueckgabe mit `Prescription` unter `6228` sowie `Full Correction` unter `6227`.
7. Baukasten schlank halten und nur fuer Sonderfaelle, Tests und Vorschau erweitern.
8. Aktivierungsassistent vorerst als read-only Regressionsstand ruhen lassen.
9. Restliche E2E-Testfaelle praktisch ausfuehren und protokollieren.

## 20. Kompakter Starttext fuer neuen Chat

Der folgende Block kann in einen neuen Chat kopiert werden:

```text
Wir arbeiten an XdtDeviceBridge / XDT Verwaltung, Version 0.1.0-prototype.

XdtDeviceBridge ist eine lokale WPF-Desktop-App fuer eine dateibasierte AIS-/Geraete-Bridge im augenaerztlichen Umfeld. Fokus ist MEDISTAR mit XDT/GDT und NIDEK ARK1S XML. Es gibt keine Cloud, keinen Windows-Dienst, keinen Autostart, keinen FileSystemWatcher und keine Verarbeitung beim App-Start.

Praktisch validiert ist MEDISTAR + NIDEK ARK1S: AIS-GDT/XDT einlesen, NIDEK ARK1S XML einlesen, Patientendaten und Messwerte mappen und eine MEDISTAR-kompatible XDT-Datei erzeugen. Wichtige Felder sind 8000=6310, Patientendaten 3000/3101/3102/3103, 8402 Untersuchungsart und 6228 Ergebniszeilen rechts/links. Am 2026-05-11 wurde zusaetzlich der XDT-Anhang-Link ueber 6302/6303/optional 6304/6305 praktisch validiert; ein externer Anhang konnte aus einer MEDISTAR-Karteikarte geoeffnet werden.

Die Haupt-Tabs sind Verarbeitung, Profile & Templates, Schnittstellenprofile und Lizenz. Verarbeitung ist der Betriebsbereich mit Schnittstellen-Monitor, aktiven Schnittstellenprofilen, manuell startbarer Ueberwachung, automatischer Verarbeitung per Haken, Paketstatus und Monitoring-Ereignissen.

Im Tab Verarbeitung ist die V1 der abdockbaren Geraeteanbindungsfenster praxisabgenommen; das Protokoll liegt in `docs/PRAXISABNAHME_GERAETEFENSTER_V1.md`. Jede Monitoring-Karte kann manuell als eigenes Fenster geoeffnet und wieder angedockt werden. Relevante Monitoring-Aktivitaet oeffnet automatisch nur das betroffene Floating-Fenster und bringt es nach vorne; neue Dateiversionen mit gleichem Namen werden dabei erneut als Aktivitaet gewertet, waehrend identische Scan-Wiederholungen dedupliziert bleiben. Nach Vorgangsreset wird zusaetzlich der profilbezogene Monitoring-Dedupe-State freigegeben, damit eine neue AIS-Datei nach `↺` und X-Andocken wieder AutoDetach ausloest. Der App-Content-Signalton `04_praxis_terminal_signal.wav` wird nur bei stabil erkanntem Geraetedatei-Eingang mit kurzem Cooldown pro Schnittstellenprofil abgespielt. AIS-, Anhang-, Export- und Statusmeldungen bleiben stumm. Nach terminalem Abschluss dockt ein automatisch geoeffnetes, nicht gepinntes Fenster nach 5 Sekunden Restlaufzeit wieder an; neue Aktivitaet, Pin oder manuelles Andocken beendet den Countdown pro Profil. Schliessen per `X` dockt Floating-Fenster sicher zurueck. Der Reset `↺` verwirft nach Sicherheitsabfrage den aktuellen Vorgang genau eines Schnittstellenprofils, bricht interne Wartezustaende ab und leert nur die AIS-/Geraete-/optionalen XDT-Anhang-Eingangsordner dieses Profils top-level; Export-, Archiv- und Fehlerordner sowie Unterordner bleiben unangetastet. Pin, neues grafisches Standardlayout, `-`/`+`-Scanintervallsteuerung und Positionsmerken arbeiten pro Schnittstellenprofil; Position/Groesse und Abdockstatus werden als UI-State unter AppData gespeichert, wenn Positionsmerken aktiv ist. Beim App-Start werden gespeicherte Floating-Fenster erst nach der sicheren MainWindow-Anzeige wiederhergestellt; ein Restore-Fehler dockt die Karte sicher an. Die Systray-Grundfunktion ist vorhanden: Minimieren oder `X` am Hauptfenster blendet die App in den Infobereich aus, Doppelklick beziehungsweise `Oeffnen` stellt das Hauptfenster wieder her, `Beenden` schliesst bewusst. Die Floating-Fenster sind leicht verbreitert, damit die Symbolleiste `🗗 ↺ 📌 🔝` in einer Zeile bleibt. Autostart, Windows-Dienst, UI-Einstellung fuer Rueckdock-Zeit, sichtbarer Countdown-Hinweis, Ton-Schalter und eigenes Systray-Icon sind Komfortthemen fuer spaeteres Praxisfeedback.

Die TOPCON-CV5000-/CV-5000S-Pilotdarstellung ist als Standardlayout fuer alle Monitoring-Karten und Floating-Geraetefenster uebernommen. Geraete zeigen einen linken Infoblock, ein Geraetebild ueber `DeviceImagePath` oder einen Platzhalter und eine Statuskugel. Die Kugel pulsiert anhand des Scanintervalls nur bei laufender Ueberwachung und kann bei Dateieingang kurz gruen blitzen. `Letzter Scan` und `Automatik` stehen im Detailbereich. BuiltIn-Geraetebilder liegen als App-Assets unter `XdtDeviceBridge.App/Assets/Devices/`; UserDefined-Geraete koennen optional eigene Bildpfade speichern. Bestehende Geraete werden im Tab `Profile & Templates` ueber `Geraet laden` angezeigt; neue Bilder werden in den lokalen AppData-Unterordner `DeviceImages` kopiert. BuiltIn-Geraete werden fachlich nicht ueberschrieben, sondern nutzen bei Bildwechsel einen lokalen Override in `device-image-overrides.json`.

Profile & Templates ist der Profil- und Templatebereich. Standardziel ist fertiges Geraeteprofil plus fertiges Templatepaket; der Baukasten Test & Vorschau bleibt fuer Sonderfaelle, Tests und kundenspezifische Anpassungen. AIS-, Geraete- und Exportprofile koennen jetzt schlank und mit Hilfetexten als UserDefined angelegt werden; bestehende Geraete koennen ueber `Geraet laden` gelesen und in V1 nur hinsichtlich ihres Geraetebildes gepflegt werden. Es gibt dabei keine automatische Aktivierung, keine Schnittstellenprofil-Aenderung und keine Verarbeitung.

XDT-Anhaenge fuer AIS sind fuer den validierten MEDISTAR/ARK1S-Pflicht-Anhang-Praxislauf praktisch bestaetigt. Unterstuetzt sind PDF, JPG, JPEG, PNG, TIF, TIFF, DCM und TXT. Externe Linkfelder sind 6302 Dokumentenname, 6303 Dateiformat, 6304 Beschreibung optional und 6305 vollstaendiger Dateipfad. XDT-Längenpraefixe erzeugt der XdtExportBuilder zentral, nicht die UI.

Im Baukasten kann ein XDT-Anhang aus beliebigem Speicherort gewaehlt werden. Vorschau und Test-XDT simulieren aber den Schnittstellenprofil-Zielpfad: 6305 zeigt auf XDT-Anhang Exportordner plus erzeugten Dateinamen. Der Testexport schreibt XDT-Datei und umbenannten Anhang physisch in einen frei gewaehlten Testordner, ohne Exportprofile oder BuiltIns zu veraendern.

Die automatische Paketlogik ist zweistufig: Phase 1 AIS-Datei wartet auf stabile Geraetedatei, Default 10 Minuten. Eine neue AIS-Datei ersetzt eine aeltere wartende AIS-Datei. Phase 2 startet erst nach vollstaendigem AIS-/Geraete-Paar: optionaler oder verpflichtender XDT-Anhang wartet bis Default 30 Sekunden. Nach erfolgreichem Export mit einem oder mehreren Anhaengen ist der Vorgang terminal abgeschlossen; die Monitoring-Karte geht auf den naechsten Vorgang zurueck und zeigt keinen alten Anhang-Timeout als aktiven Endzustand.

Optionaler XDT-Anhang bedeutet: Wenn ein oder mehrere stabile unterstuetzte Anhaenge rechtzeitig kommen, Export mit je eigener 6302-6305-Linkfeldgruppe; wenn keiner kommt, Export ohne Anhang nach Timeout. Pflicht bedeutet: ohne stabilen unterstuetzten Anhang blockiert die Verarbeitung oder geht in Fehlerstatus. Fuer AttachmentOnly-/Dokumentgeraete gibt es zusaetzlich einen profilbezogenen Abschluss: automatisch nach Ruhezeit seit der letzten stabilen Datei oder manuell per Dialog `Dokumente uebertragen`; dieser Dialog bleibt offen und aktualisiert seine Dateiliste, bis der Anwender `Uebertragen` klickt.

Dateistabilitaet ist wichtig: AIS-, Geraete- und Anhangdateien werden erst verarbeitet, wenn sie stabil und lesbar sind. Default fuer XDT-Anhang-Stabilitaet ist 2 Sekunden. Das Scan-Intervall ist pro Schnittstellenprofil konfigurierbar, Default 5 Sekunden. Die automatische Verarbeitung richtet sich nach dem aktuellen Inhalt der konfigurierten Eingangsordner; interne Historie blockiert keinen neuen MEDISTAR-Auftrag.

Profile sind JSON-basiert unter %LocalAppData%\XdtDeviceBridge\profiles. BuiltIn-Profile duerfen nicht ueberschrieben oder geloescht werden, UserDefined-Profile werden separat gespeichert. Neue AIS-, Geraete- und Exportprofile koennen als UserDefined angelegt werden; Konflikte werden blockiert und es wird nichts automatisch aktiviert. UserDefined-Exportprofile koennen geloescht werden, wenn kein Schnittstellenprofil sie verwendet; Exportregeln koennen nur aus UserDefined-Exportprofilen entfernt werden. Templatepaket-Export erfolgt selektiv auf Basis eines Schnittstellenprofils und nimmt nur benoetigte AIS-/Geraete-/Export-Abhaengigkeiten auf. Templatepaket-Import, Validierung, Konfliktanalyse, Importplan, Dry-Run, UI-Vorschau, sichere Benutzerwahl und explizite UserDefined-Uebernahme sind vorhanden. ReplaceExisting bleibt offen. Importierte Schnittstellenprofile werden nicht automatisch aktiviert; IsAttachmentProcessingEnabled wird deaktiviert.

Der Aktivierungsassistent fuer importierte Schnittstellenprofile ist read-only vorbereitet und ruht vorerst. Im Tab Schnittstellenprofile gibt es Pruefung vor Aktivierung und den Vorschau-Dialog Aktivierung vorbereiten. Die Service-Kette lautet Evaluation -> Guard -> PreparationPreview. Angezeigt werden V1-relevante Vorschauinformationen: Status, Aktivierbarkeit nach V1, technische Freigabe, Blocker, Warnungen, Hinweise, Ordner-Erreichbarkeit und Sicherheitshinweis. Die Pruefung verwendet ungespeicherte UI-Werte, speichert aber nichts. Es gibt keinen Aktivieren-Button, keine produktive Warnungsbestaetigung, keine Speicherung, keine Profiländerung und keine Datei-/Ordneroperation.

Noch nicht praktisch validiert sind das generische Dokumentgeraet und die manuelle Dokumentauswahl. TOPCON TRK2P ist testseitig und praktisch mit drei echten XML-Dateien abgesichert: Namespace-/REF-/KM-/TM-/CCT-Erkennung, MEDISTAR-`6228`-REF-Zeilen, `6221`-KM-Zeilen, `6220`-Pachy-Zeilen aus CCT, mehrzeilige `6205`-Tonometrie, TM/CCT-only-Teilmessung ohne Exportabbruch, optionaler `6227`-SBJ-Pfad, BuiltIn-Schnittstellenprofil, Templatepaket-Kandidat, Praxisprotokoll und Repair alter persistierter BuiltIns. TOPCON CL300 ist testseitig und praktisch mit zwei echten CL-300-XML-Dateien, Namespace-/LM-Erkennung, MEDISTAR-`6228`-Lensmeterzeilen inklusive ADD, PD und signierten H/V-Prismenkomponenten, BuiltIn-Schnittstellenprofil, Templatepaket-Kandidat und Praxisprotokoll abgesichert; offen bleiben H/V-Prisma-Beobachtung und ein offizielles ZIP-Artefakt nach Release-Regel. TOPCON KR800S ist testseitig und praktisch mit zwei echten Shift-JIS-XML-Dateien, Namespace-/REF-/KM-/SBJ-Erkennung, MEDISTAR-`6228`-REF-Zeilen, `6221`-KM-Zeilen, konservativen `6227`-SBJ-Zeilen, BuiltIn-Schnittstellenprofil, Templatepaket-Kandidat und Praxisprotokoll abgesichert; offen bleiben weitere SBJ-/Funktionstestfaelle und ein offizielles ZIP-Artefakt nach Release-Regel. TOPCON CT1P ist testseitig und praktisch mit echter CT-1P-XML-Datei abgesichert: MEDISTAR-`6220`-Pachymetrie, mehrzeilige `6205`-Tonometrie, rechter CorrectedIOP/CCT-Block, ausgelassener unvollstaendiger linker CorrectedIOP/CCT-Block, BuiltIn-Schnittstellenprofil, Templatepaket-Kandidat und Praxisprotokoll sind vorhanden; offen bleibt eine zweite echte beidseitige CorrectedIOP/CCT-Datei. NIDEK NT530P ist testseitig mit echter UTF-16-XML-Datei, korrigierter mehrzeiliger `6205`-Tonometrie, `6220`-Pachymetrie, BuiltIn-Schnittstellenprofil und Templatepaket-Kandidat abgesichert; die praktische MEDISTAR-Abnahme steht noch aus. NIDEK LM7/LM-7P ist testseitig mit echter XML-Datei, Stylesheet-Ignoriertest, MEDISTAR-Lensmeterzeilen, Reparatur alter persistierter BuiltIn-Exportpfade, Templatepaket-Kandidat und praktischer MEDISTAR-Abnahme abgesichert. NIDEK AR360 / AR-360A ist fuer die Auto-Refraktor-XDT-Rueckgabe praktisch validiert; offen bleiben XDT-Anhangfall und offizielles ZIP-Artefakt. MEDISTAR + Dokumentanhang ist als AttachmentOnly-Kandidat fuer Dateien ohne Messwerte vorhanden; XML, MP4, MP3 und WAV sind dort reine Anhaenge, der Geraete-/Dokument-Importordner ist der relevante Dateieingang, mehrere Dateien koennen per Ruhezeit oder manueller Bestaetigung abgeschlossen werden, pro Datei wird `6304` mit Benutzertext oder Originaldateiname gefuellt, die Karte bleibt vor Vorgangsbeginn neutral und die Anhangverarbeitung wird fuer Dokumentgeraete auch bei alten deaktivierten technischen Flags erzwungen. MEDISTAR + Manuelle Dokumentuebergabe erwartet dagegen keinen Geraete-Importordner: Erst AIS-Eingang oeffnet nur den gruen gestalteten TopMost-Dialog, Dateien werden vom Anwender per Drag-&-Drop, Dateidialog oder Zwischenablage hinzugefuegt, nur kopiert und je Datei ueber `6302`-`6305` verlinkt. Die Matrix `docs/GERAETE_PROFILE_TEMPLATE_MATRIX.md` fuehrt Status, Tests, Templatepaket-Luecken und naechste Prioritaeten. Die Vorlagen `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_ARK1S.md`, `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_AR360.md`, `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_LM7.md`, `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_NT530P.md`, `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_CL300.md`, `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_KR800S.md`, `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_TRK2P.md`, `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_CT1P.md`, `docs/TEMPLATEPAKET_MEDISTAR_DOKUMENTANHANG.md` und `docs/TEMPLATEPAKET_MEDISTAR_MANUELLE_DOKUMENTUEBERGABE.md` beschreiben die Referenzpakete beziehungsweise Kandidaten. Der Testweg erzeugt und prueft die Paket-ZIPs temporaer ueber den selektiven Exporter/Importer-Pfad; dauerhaft abgelegt werden sie erst nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md`.

TOPCON CV5000 ist als erster bidirektionaler Phoropter-Kandidat testseitig vorbereitet: `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_CV5000.md` und `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_CV5000.md` dokumentieren AIS-Historienparser, CV-5000-Import-XML-Writer, Schnittstellenprofil-basierte Ausgabe an Geraet, CV-5000-Rueckgabeparser, toleranten Rueckweg mit MEDISTAR-Historien-AIS und MEDISTAR-Rueckgabe mit `6228` fuer `Prescription` sowie `6227` fuer `Full Correction`. Der finale MEDISTAR-Import der Rueckgabedatei bleibt praktisch offen.

TOPCON Solos ist als eigener Lensmeter-Kandidat vorbereitet: `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_SOLOS.md` und `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_SOLOS.md` dokumentieren SOLOS-Erkennung, TOPCON-LM-XML, `6228`-Lensmeter-Ausgabe mit PD und signierten H-Prismen sowie die bewusst offenen Themen Transmission und PDF-Berichte.

TOPCON KR-1 ist als eigener Keratorefraktometer-Kandidat vorbereitet: `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_KR1.md` und `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_KR1.md` dokumentieren Shift-JIS-Erkennung, TOPCON-REF-XML, `6228`-Rueckgabe aus REF-Medianwerten sowie die bewusst offenen KM/KRT-/peripheren-KRT-Themen ohne echte Fixture.

TOPCON CT800A ist als eigener Non-Contact-Tonometer-Kandidat vorbereitet: `docs/TEMPLATEPAKET_MEDISTAR_TOPCON_CT800A.md` und `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_CT800A.md` dokumentieren CT-800A-Erkennung, TOPCON-TM-XML, `6205`-Tonometrie, vollstaendige CorrectedIOP/CCT-Detailzeilen nur bei verwertbaren Daten und ausgelassene ERROR-/Detailbloecke ohne Exportabbruch.

Lizenzsystem: InstallationInfo, Lizenzanfrage, Lizenzimport, Statusanzeige, Bewertung aktiver Geraeteanbindungen und Karenzzeitmodell sind vorbereitet. Die V1-Modelle fuer `LicenseEnvelope`, `LicensePayload`, Signaturstatus und nicht produktiv angeschlossene Policy-Bewertung konkretisieren `MaxActiveDeviceConnections` und 7-Tage-Karenz. Es gibt noch keine harte Lizenzsperre, keine Online-Lizenzierung und keine produktive Signaturpruefung.

Wichtige Sicherheitsregeln: keine unbekannten Dateien anfassen, keine pauschale Ordnerleerung, Exportordner nicht bereinigen, instabile Dateien nicht verarbeiten, mehrere XDT-Anhaenge nur als einzelne unterstuetzte Dateien mit eigenen Linkfeldgruppen uebergeben, keine medizinische Bewertung, BuiltIns nicht ueberschreiben.

Zentrale Dokumente: README.md, CHANGELOG.md, docs/ROADMAP.md, docs/ARCHITEKTUR.md, docs/PFLICHTENHEFT.md, docs/GERAETE_BEISPIELE.md, docs/GERAETE_PROFILE_TEMPLATE_MATRIX.md, docs/TEMPLATEPAKET_MEDISTAR_NIDEK_ARK1S.md, docs/TEMPLATEPAKET_MEDISTAR_NIDEK_AR360.md, docs/TEMPLATEPAKET_MEDISTAR_NIDEK_LM7.md, docs/TEMPLATEPAKET_MEDISTAR_NIDEK_NT530P.md, docs/TEMPLATEPAKET_MEDISTAR_TOPCON_CL300.md, docs/TEMPLATEPAKET_MEDISTAR_TOPCON_SOLOS.md, docs/TEMPLATEPAKET_MEDISTAR_TOPCON_KR800S.md, docs/TEMPLATEPAKET_MEDISTAR_TOPCON_KR1.md, docs/TEMPLATEPAKET_MEDISTAR_TOPCON_TRK2P.md, docs/TEMPLATEPAKET_MEDISTAR_TOPCON_CT1P.md, docs/TEMPLATEPAKET_MEDISTAR_TOPCON_CT800A.md, docs/TEMPLATEPAKET_MEDISTAR_TOPCON_CV5000.md, docs/TEMPLATEPAKET_MEDISTAR_DOKUMENTANHANG.md, docs/TEMPLATEPAKET_MEDISTAR_MANUELLE_DOKUMENTUEBERGABE.md, docs/E2E_TESTPROTOKOLL_MEDISTAR_LM7.md, docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_CL300.md, docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_SOLOS.md, docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_KR800S.md, docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_KR1.md, docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_TRK2P.md, docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_CT1P.md, docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_CT800A.md, docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_CV5000.md, docs/TEMPLATEPAKET_RELEASE_REGEL.md, docs/PRAXISABNAHME_GERAETEFENSTER_V1.md, docs/END_TO_END_TESTPLAN.md und docs/PROJEKT_UEBERBLICK.md.

Naechste sinnvolle Schritte: Geraeteanbindungsfenster V1 als abgenommenen Block beibehalten und nur Komfortthemen nach Praxisfeedback priorisieren; Release-Regel fuer ARK1S- und AR360-ZIP-Artefakte anwenden, optional den AR360-XDT-Anhangfall separat testen, fuer LM7/LM7P weitere Prisma-/PD-Beispielfaelle sammeln, danach NT530P praktisch in MEDISTAR mit korrigierter `6205`/`6220`-Anzeige validieren, TOPCON CT1P als validierten Referenzkandidaten halten und eine echte beidseitige CorrectedIOP/CCT-Datei sammeln, TOPCON KR-1 praktisch mit `6228`-REF und TOPCON CT800A praktisch mit `6205`-Tonometrie abnehmen, TOPCON CL300, TOPCON KR800S und TOPCON TRK2P als validierte Referenzkandidaten halten, TOPCON Solos praktisch in MEDISTAR abnehmen und H/V-Prisma sowie weitere SBJ-/Transmission-/PDF-/KM-KRT-Faelle beobachten, TOPCON CV5000 bidirektional live pruefen, MEDISTAR + Dokumentanhang mit mehreren Dateien live pruefen, die manuelle Dokumentuebergabe mit Drag-&-Drop/Dateidialog/Zwischenablage praktisch pruefen und Aktivierungsassistent vorerst ruhen lassen.
```
