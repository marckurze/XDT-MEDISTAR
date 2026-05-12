# Roadmap XdtDeviceBridge

Stand: 2026-05-12

Projekt: XdtDeviceBridge / XDT Verwaltung

## 1. Aktueller Stand

### Version

- Aktuelle Version: `0.1.0-prototype`
- `VERSION` und `Directory.Build.props` sind konsistent:
  - `Version`: `0.1.0-prototype`
  - `AssemblyVersion`: `0.1.0.0`
  - `FileVersion`: `0.1.0.0`
  - `InformationalVersion`: `0.1.0-prototype`

### Projektleitlinie ab 2026-05-12

- Schlank vor vollstaendig: keine Architektur auf Vorrat.
- Fertige Geraeteprofile und Templatepakete haben Vorrang vor Baukasten-Nutzung.
- Der Baukasten bleibt Werkzeug fuer Sonderfaelle, Tests, Vorschau und kundenspezifische Anpassungen.
- Der Aktivierungsassistent ist fuer den Moment ausreichend vorbereitet und wird nicht weiter ausgebaut.
- Naechste Entwicklungsarbeit soll konkrete Geraete-/Template-Luecken schliessen.

### Validierter Kernworkflow MEDISTAR + NIDEK ARK1S

- Praktisch validierter Kernworkflow: MEDISTAR-GDT/XDT-Eingang + NIDEK ARK1S XML-Eingang + MEDISTAR-kompatibler XDT-Export.
- Am 2026-05-11 wurde zusätzlich MEDISTAR + NIDEK ARK1S + XDT-Anhang-Link praktisch validiert.
- Erzeugt werden insbesondere:
  - `8000=6310`
  - `8402` Untersuchungsart
  - `6228` Ergebniszeilen
  - bei erfolgreichem XDT-Anhang-Link `6302`, `6303`, optional `6304` und `6305`
- Bestätigt wurden ein Pflicht-XDT-Anhang-Fall mit vorhandenem Anhang, ein Pflicht-XDT-Anhang-Fehlerfall ohne Anhang und der erfolgreiche Linkaufruf aus einer MEDISTAR-Karteikarte.
- Dieser Workflow ist der stabile Prototyp-Kern und darf bei weiteren Ausbauphasen nicht beschädigt werden.

### Manuelle Verarbeitung

- Der manuelle Diagnose-/Testpfad bleibt vorhanden.
- AIS-GDT/XDT-Datei und Gerätedatei können manuell geladen werden.
- Patientendaten, Messwerte und Exportvorschau können angezeigt werden.
- Eine manuelle Exportdatei kann geschrieben werden.

### Automatische periodische Verarbeitung

- Die Überwachung wird manuell gestartet.
- Es gibt keinen Windows-Dienst, keinen Autostart und keinen `FileSystemWatcher`.
- Die Ordnerabfrage erfolgt periodisch.
- Das Scan-Intervall ist pro Schnittstellenprofil konfigurierbar.
- Standard: `5` Sekunden.
- Automatische Verarbeitung erfolgt nur bei:
  - gestarteter Überwachung
  - aktivierter globaler automatischer Verarbeitung
  - aktivem Schnittstellenprofil
  - stabilen Dateien

### XDT-Anhänge für AIS

- Schnittstellenprofile enthalten einen vorbereiteten Bereich `XDT-Anhänge für AIS`.
- Vorbereitet sind:
  - XDT-Anhang Importordner
  - XDT-Anhang Exportordner
  - Dateinamen-Template
  - Transfermodus `Copy`/`Move`, Standard `Move`
  - automatische Einschaltfunktion pro Schnittstellenprofil
  - Erwartung `optional` oder `Pflicht`, Standard `optional`
  - Wartezeit auf XDT-Anhang, Standard `30` Sekunden
  - Dateistabilität, Standard `2` Sekunden
  - XDT-Linkfelder `6302`, `6303`, optional `6304`, `6305`
- Die Linkfelder werden als semantische Feldcode/Wert-Paare vorbereitet. XDT-Längenpräfixe werden weiterhin zentral durch den XDT-Exportmechanismus erzeugt.
- Mehrere unterstützte Anhänge werden nicht automatisch zugeordnet.

### Baukasten `Test & Vorschau`

- Im Tab `Profile & Templates` gibt es den Bereich `Test & Vorschau`.
- Der Baukasten ist nicht der Standardweg fuer Anwender. Der Standardweg soll ein fertiges Geraeteprofil plus fertiges Templatepaket sein.
- Der Bereich ist als manueller Baukasten-Test aufgebaut:
  - AIS-Datei laden
  - Gerätedatei laden
  - optional XDT-Anhang einlesen
  - Messwerte prüfen
  - Gesamtexport-Vorschau XDT prüfen
  - Testexport erstellen
- Das verwendete Schnittstellenprofil wird mit AIS-, Geräte-, Exportprofil- und XDT-Anhang-Konfiguration angezeigt.
- XDT-Anhänge können für den Baukasten-Test aus einem beliebigen Speicherort ausgewählt werden.
- Die Vorschau simuliert den produktiven Zielpfad aus dem Schnittstellenprofil: `6305` zeigt auf `XDT-Anhang Exportordner` plus erzeugten Dateinamen, nicht auf den Quellpfad.
- Exportprofile und BuiltIn-Profile werden durch den Baukasten-Test nicht verändert.

### Testexport

- Der Baukasten-Testexport schreibt eine Test-XDT-Datei in einen vom Benutzer gewählten Testordner.
- Wenn ein XDT-Anhang eingelesen wurde, wird zusätzlich der korrekt umbenannte Anhang in den Testordner kopiert.
- Der Wert in `6305` bleibt auf den simulierten Schnittstellenprofil-Zielpfad ausgerichtet.
- Der produktive XDT-Anhang Exportordner des Schnittstellenprofils wird im Baukasten-Test nicht beschrieben.

### Dateistabilität

- AIS-, Geräte- und XDT-Anhangdateien sollen erst verarbeitet werden, wenn sie stabil und lesbar sind.
- XDT-Anhang-Kandidaten werden mit Stabilitätsstatus betrachtet.
- Instabile Anhänge werden nicht automatisch ausgewählt, kopiert, verschoben oder verlinkt.

### Paket-Wartelogik

- Die Paketlogik ist zweistufig vorbereitet:
  - Phase 1: AIS-Datei wartet auf passende stabile Gerätedatei.
  - Phase 2: Erst nach vollständigem AIS-/Geräte-Paar beginnt die Wartezeit auf einen optionalen oder verpflichtenden XDT-Anhang.
- Wartezeit auf Gerätedatei: pro Schnittstellenprofil, Standard `10` Minuten.
- Eine neuere AIS-Datei kann einen wartenden AIS-Auftrag ersetzen.
- Optionaler XDT-Anhang:
  - genau ein eindeutiger stabiler Anhang: Export mit Linkfeldern
  - kein Anhang nach Timeout: Export ohne Linkfelder
  - mehrere Anhänge: keine automatische Zuordnung, Export ohne Anhang mit Warnstatus
- Pflicht-XDT-Anhang:
  - genau ein eindeutiger stabiler Anhang: Export mit Linkfeldern
  - kein Anhang nach Timeout: Blockade/Fehlerstatus
  - mehrere Anhänge: Blockade/Fehlerstatus

### Schnittstellenprofile und Templatepakete

- Die kompakte Geraete-/Template-Bestandsaufnahme steht in `docs/GERAETE_PROFILE_TEMPLATE_MATRIX.md`.
- BuiltIn-Profile werden nicht überschrieben.
- UserDefined-Profile werden separat gespeichert.
- Profile werden JSON-basiert unter `%LocalAppData%\XdtDeviceBridge\profiles` verwaltet.
- Templatepaket-Export und Templatepaket-Import sind vorhanden.
- Importierte Templatepakete werden validiert.
- Konflikte werden analysiert: gleiche ID, gleicher Name, BuiltIn-Schutz, UserDefined-Konflikte, fehlende Abhängigkeiten und prüfpflichtige Ordner-/XDT-Anhang-Einstellungen.
- Aus der Analyse wird ein Importplan erzeugt.
- Der Dry-Run und die UI-Importvorschau zeigen geplante Aktionen, Ziel-IDs/Zielnamen, Abhängigkeiten und Warnungen.
- Sichere Benutzeraktionen sind möglich:
  - `Neu importieren`
  - `Als Kopie importieren`
  - `Bestehendes behalten`
  - `Überspringen`
- Die explizite Übernahme als UserDefined-Profile ist vorhanden.
- `ImportAsNew` und `ImportAsCopy` werden geschrieben; `KeepExisting`, `Skip` und blockierte Profile werden nicht geschrieben.
- BuiltIn-Profile werden nicht überschrieben.
- Abhängigkeiten von importierten Schnittstellenprofilen werden auf lokale oder neu importierte Zielprofile remapped.
- Importierte Schnittstellenprofile bleiben inaktiv.
- `IsAttachmentProcessingEnabled` wird bei importierten Schnittstellenprofilen deaktiviert.
- XDT-Anhang-Einstellungen bleiben erhalten und müssen vor Aktivierung geprüft werden.
- `ReplaceExisting` bleibt deaktiviert.
- Der sichere Importfluss ist E2E-nah automatisiert getestet.

### Aktivierungsassistent fuer importierte Schnittstellenprofile

- Der Aktivierungsassistent ist als reine Vorschau vorbereitet, aber noch nicht produktiv aktivierend.
- Im Tab `Schnittstellenprofile` gibt es den Bereich `Pruefung vor Aktivierung` mit Status, Aktivierbarkeit, Blocker-/Warnungs-/Hinweiszaehlern, strukturierter Ordnerpruefung, strukturierter XDT-Anhang-Konfiguration und eingeklappter Tabelle `Alle Pruefpunkte`.
- Der Button `Aktivierung vorbereiten` oeffnet ein reines scrollbares Preview-Fenster mit OK-/Schliessen-Aktion. Es zeigt nur noch V1-relevante Informationen: Bewertung, technische Guard-Entscheidung, `Aktivierbar nach V1`, Blocker, Warnungen, Hinweise und Sicherheitshinweis.
- Die Service-Kette lautet: `InterfaceProfileActivationEvaluationService` -> `InterfaceProfileActivationGuardService` -> `InterfaceProfileActivationPreparationPreviewService`.
- `ReadyWithWarnings` wird angezeigt, aber in V1 nicht produktiv aktiviert.
- Ein Interface-/Model-Skelett fuer einen spaeteren `ActivationExecutor` ist vorhanden: Request, Result, Preconditions, Statuswerte und `IInterfaceProfileActivationExecutor`.
- `InterfaceProfileActivationExecutorRequest` traegt nur noch den V1-noetigen Kontext: Zielprofil-ID/-Name, `OperationMode`, Quelle/Zeitpunkt sowie Preview-Evaluation/-Guard als Kontext.
- Eine executor-nahe Store-/Loader-Abstraktion ist vorbereitet: `IInterfaceProfileActivationProfileStore` mit Load-/Save-Resultmodellen, nicht-produktivem In-Memory-Stub und `InterfaceProfileActivationProfileCatalogStore` gegen `ProfileCatalogService`/`AppDataPaths`. Der Catalog-Adapter kann frisch laden, Save bleibt ValidateOnly/DryRun und speichert nichts.
- `InterfaceProfileActivationExecutorStub` ist als defensive Backend-Stufe vorhanden. Er bewertet Preconditions und liefert Statuswerte; im `ValidateOnly`-Modus kann er optional den ActivationProfileStore fuer frisches Laden und eine nicht-produktive finale Pruefkette aus Evaluation + Guard nutzen. Save bleibt DryRun, `IsActive` wird nicht gesetzt, es wird nichts gespeichert, keine Verarbeitung gestartet und keine UI angebunden.
- Es gibt keine produktive Executor-Implementierung, keinen Aktivieren-Button, keine Warnungsbestaetigung, keine Aenderung an `IsActive` oder `IsAttachmentProcessingEnabled` und keine Datei-/Ordneroperationen.
- Die fachliche Entscheidungsgrundlage liegt in `docs/AKTIVIERUNG_ENTSCHEIDUNGSNOTIZ.md`: V1 bleibt schlank und erlaubt spaeter nur `Ready` ohne Warnungen fuer frisch geladene UserDefined-Profile, niemals BuiltIn.
- Statische Pruefung und praktische Windows-Sichtpruefung des Dialogs sind in `docs/UI_PRUEFPROTOKOLL_AKTIVIERUNGSASSISTENT.md` dokumentiert; der aktuelle Vorschau-Dialog ist visuell abgenommen.
- BuiltIn-Profile bleiben direkt geschuetzt; die spaetere Aktivierung ist auf kontrollierte UserDefined-Schnittstellenprofile ausgerichtet.
- Die zuletzt behobene Layout-Ueberlagerung unterhalb `Ordnerbereinigung` ist Teil des aktuellen UI-Stands und darf bei weiteren Arbeiten nicht zurueckfallen.

### Lizenzstatus

- Lizenzstatus wird angezeigt und bewertet.
- Lizenzanfrage kann exportiert werden.
- Lizenzdatei kann importiert werden.
- Lizenzierte Geräte/Anbindungen werden bewertet.
- Karenzzeiten für neue Anbindungen sind vorbereitet.
- Eine harte produktive Lizenzsperre ist noch nicht aktiv.
- Digitale Signaturprüfung und Online-Lizenzierung sind noch offen.

## 2. Was ist praktisch validiert?

- MEDISTAR + NIDEK ARK1S als Kernworkflow.
- MEDISTAR + NIDEK ARK1S + XDT-Anhang-Link über `6302`, `6303`, optional `6304` und `6305` ist im Pflicht-Anhang-Praxislauf validiert.
- Manuelle Verarbeitung für den validierten Kernworkflow.
- MEDISTAR-kompatibler XDT-Export mit zentral erzeugten Längenpräfixen.
- Baukasten-Testexport für XDT-Datei plus umbenannten XDT-Anhang ist testseitig abgesichert.
- Externe AIS-Linkfelder `6302`, `6303`, optional `6304` und `6305` sind fachlich anhand des MEDISTAR-Beispiels und technisch im Baukasten-/Testpfad belegt.
- Sicherer Templatepaket-Importfluss ist E2E-nah testseitig abgesichert: Export/Import, Validierung, Konfliktanalyse, Importplan, Benutzerwahl, Dry-Run, UserDefined-Übernahme und Dependency-Remapping.

Praxisprotokoll: `docs/E2E_TESTPROTOKOLL_MEDISTAR_ARK1S_XDT_ANHANG.md`. Die vollständige Abarbeitung aller weiteren Testfälle aus `docs/END_TO_END_TESTPLAN.md` bleibt als separater Schritt offen.

## 3. Was ist vorbereitet, aber noch nicht produktiv validiert?

- NIDEK LM7/LM7P LAN/XML.
- NIDEK NT530P.
- TOPCON CL300.
- TOPCON KR800.
- TOPCON TRK2P.
- Fertige, auslieferbare Geraete-Templatepakete fuer diese vorbereiteten Profile.
- `ReplaceExisting` für UserDefined-Profile.
- Freie Konfliktlösungs-/Bearbeitungsdialoge.
- Manuelle Zielnamen-/ID-Bearbeitung in der UI.
- Produktive Aktivierung importierter Schnittstellenprofile; Pruefung, Guard und V1-Preview sind nur read-only vorbereitet.
- Produktiver `ActivationExecutor` mit frischem Laden, finaler Evaluation + Guard und sicherer UserDefined-Speicherung.
- Vollständiger Profil-Assistent für unbekannte Geräte.
- Digitale Lizenzsignatur.
- Online-Lizenzierung.
- Harte produktive Lizenzdurchsetzung.
- Installer / Deployment.
- Vollständige AIS-Unterstützung außerhalb MEDISTAR.
- Mehrfachanhang-Zuordnung.
- Produktive Dokument-/Dateianhang-Templates über den Baukasten hinaus.

## 4. Wichtige Sicherheitsentscheidungen

- Keine Verarbeitung beim App-Start.
- Kein Windows-Dienst.
- Kein Autostart.
- Kein `FileSystemWatcher`.
- Periodischer Scan statt ereignisgetriebener Ordnerüberwachung.
- Keine unbekannten Dateien anfassen.
- Keine pauschale Ordnerleerung.
- Exportordner nicht bereinigen.
- BuiltIn-Profile nicht überschreiben.
- UserDefined-Profile separat speichern.
- Instabile Dateien nicht verarbeiten.
- Mehrere XDT-Anhänge nicht automatisch zuordnen.
- Keine automatische Verarbeitung mehrerer Anhänge.
- Keine produktive Änderung von Exportprofilen durch Baukasten-Test oder Testexport.
- XDT-Längenpräfixe nicht manuell konfigurieren, sondern zentral erzeugen.

## 5. Empfohlene nächste Entwicklungsphasen

### Phase 1: Dokumentation und Version konsolidieren

- Roadmap, Projektüberblick, Changelog und End-to-End-Testplan aufeinander abstimmen.
- Aktuellen Stand `0.1.0-prototype` sauber vom nächsten Meilenstein trennen.
- Praxisvalidierung MEDISTAR + NIDEK ARK1S + XDT-Anhang-Link als bestanden führen.
- Restliche E2E-Fälle klar als offen markieren.

### Phase 2: Fertige Geraeteprofile und Templatepakete

- MEDISTAR + NIDEK ARK1S als stabilen Referenzworkflow schuetzen.
- Aus dem validierten ARK1S-Workflow ein eindeutig dokumentiertes erstes Templatepaket ableiten.
- NIDEK LM7/LM7P mit repraesentativen XML-Dateien validieren und als naechstes fertiges Profil-/Templatepaket vorbereiten.
- NIDEK NT530P oder TOPCON-Profile nur dann priorisieren, wenn belastbare Beispiel- und Testdaten vorliegen.

### Phase 3: Baukasten schlank halten

- Der Baukasten bleibt fuer Tests, Vorschau und Sonderfaelle.
- Keine zusaetzlichen Assistenten- oder Wizard-Ebenen einbauen, solange fertige Profile/Templatepakete fehlen.
- Wenn ein Geraete-Datei-Explorer entsteht, zunaechst read-only: Datei laden, SourcePaths anzeigen, keine Profiländerung.

### Phase 4: Geräte-Datei-Explorer / Profil-Assistent

- Unbekannte Geräte-/XML-/Textdateien analysieren.
- SourcePaths anzeigen.
- Feldvorschläge und Platzhalter ableiten.
- Profilentwurf als UserDefined speichern.

### Phase 5: Produktive Validierung vorbereiteter Geräteprofile

- LM7/LM7P mit echten LAN/XML-Dateien validieren.
- NT530P, TOPCON CL300, TOPCON KR800 und TOPCON TRK2P mit echten Gerätedateien testen.
- Manuelle Exportprofile je Gerät gegen AIS-Anforderungen prüfen.
- AIS-/MEDISTAR-Default-Exporttemplates bleiben bewusst zurückgestellt, bis ein neues Fachkonzept vorliegt.
- Dokumentierte Beispielprofile mit Testergebnissen verknüpfen.

### Phase 6: Aktivierungsassistent geparkt

- Read-only Aktivierungsassistent als visuell abgenommenen Sicherheitsstand beibehalten.
- Keine weitere Aktivierungsarchitektur ausbauen, bis konkrete Profil-/Templatepakete den Produktwert erhoeht haben.
- Spaetere Produktivaktivierung separat klein spezifizieren: `Ready`, UserDefined, nicht BuiltIn, keine Warnungen, finale Evaluation + Guard.

### Phase 7: Lizenzsignatur und Lizenzdurchsetzung

- Signaturformat und Public-Key-Prüfung definieren.
- Importierte Lizenzdateien kryptografisch prüfen.
- Harte Sperren erst aktivieren, wenn Ausnahmepfade und Karenzzeiten fachlich bestätigt sind.

### Phase 8: Installer / Deployment

- Installationsziel, Datenordner und Rechtekonzept definieren.
- Desktop-Shortcut und Startmenüeintrag prüfen.
- Update-/Backup-Konzept für lokale Profile und Lizenzen festlegen.

## 6. Empfohlene nächste kleine Codex-Schritte

1. Erstes offizielles Templatepaket fuer MEDISTAR + NIDEK ARK1S aus dem validierten Workflow ableiten.
2. `docs/GERAETE_PROFILE_TEMPLATE_MATRIX.md` als Arbeitsliste fuer Geraete-/Templatepakete fortfuehren.
3. LM7/LM7P-Beispieldateien gegen die dokumentierten SourcePaths testen.
4. Fuer LM7/LM7P ein fertiges Profil-/Templatepaket vorbereiten, wenn die Datenlage reicht.
5. Danach NIDEK NT530P oder TOPCON CL300/KR800/TRK2P anhand vorhandener Beispiel- und Testdaten priorisieren.
6. Restliche E2E-Testfälle mit realen Testordnern ausführen und mit `docs/E2E_TESTPROTOKOLL_TEMPLATE.md` protokollieren.
7. Read-only Aktivierungsassistent nur als geparkten Regressionsstand weiterfuehren.
8. Lizenzsignatur- und Installer-Themen erst nach weiterem Profil-/Template-Nutzen priorisieren.

## 7. Risiken / offene Entscheidungen

- JSON vs SQLite: JSON ist aktuell einfach und transparent; SQLite kann später bei Historie, Audit und größerem Profilbestand sinnvoll werden.
- Windows-Dienst später ja/nein: aktuell bewusst nein; für echten Dauerbetrieb eventuell später erneut bewerten.
- FileSystemWatcher später ja/nein: aktuell periodischer Scan; ein Watcher könnte später ergänzen, aber nicht die Stabilitätsprüfung ersetzen.
- Lizenzsperre wann aktivieren: erst nach Signaturprüfung, Karenzzeitentscheidung und klarer Fehlerkommunikation.
- EV-Verknüpfung vs `6302`-`6305`-Link: Der strukturierte Link über `6302` bis `6305` wurde für MEDISTAR + NIDEK ARK1S praktisch validiert; andere AIS-Zielsysteme müssen separat geprüft werden.
- Mehrfachanhang-Zuordnung: aktuell keine automatische Auswahl; spätere Heuristiken brauchen sichere Patienten-/Auftragsbezüge.
- Zweite AIS-Unterstützung außerhalb MEDISTAR: Feldlogik, Dateinamen, Importsteuerung und externe Links können je AIS abweichen.
- Produktive Attachment-Automatik: Pflicht-Anhang vorhanden/fehlt wurde für MEDISTAR + NIDEK ARK1S praktisch geprüft; weitere E2E-Fälle bleiben offen.
- Langsam schreibende Geräte: Stabilitätszeiten müssen im Praxisbetrieb je Gerät justiert werden.

## 8. Abnahmekriterien für nächsten Meilenstein

- `dotnet build XdtDeviceBridge.sln` ist grün.
- `dotnet test XdtDeviceBridge.sln` ist grün.
- E2E-Testplan aus `docs/END_TO_END_TESTPLAN.md` ist mit Ergebnisprotokoll bestanden; der Pflicht-Anhang-Praxislauf MEDISTAR + NIDEK ARK1S ist bereits separat bestanden.
- MEDISTAR + NIDEK ARK1S bleibt unverändert funktionsfähig.
- Export ohne aktivierte XDT-Anhang-Funktion enthält keine `6302`, `6303`, `6304`, `6305`.
- XDT-Anhang-Testexport erzeugt XDT-Datei plus korrekt umbenannten Anhang.
- `6305` zeigt im Baukasten-Test auf den simulierten Schnittstellenprofil-Zielpfad.
- Templatepaket kann produktiv als UserDefined übernommen werden.
- Konflikte beim Templatepaket-Import werden erkannt und nicht still überschrieben.
- BuiltIn-Profile bleiben geschützt.
- `ReplaceExisting` bleibt deaktiviert, bis es gesondert spezifiziert und getestet ist.
- Importierte Schnittstellenprofile bleiben inaktiv, bis sie bewusst geprüft und aktiviert werden.
- Keine unbekannten Dateien werden gelöscht, verschoben oder verarbeitet.
