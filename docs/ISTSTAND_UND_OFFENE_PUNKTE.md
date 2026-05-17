# Iststand und offene Punkte

Stand: 2026-05-12

Basis dieses Abgleichs:

- Repository-Stand nach Ruecksetzung auf Commit `6b9bc20bab896e7fa103748ba20e435c34d3e8cd`
- Dokumente: `README.md`, `CHANGELOG.md`, `VERSION`, `Directory.Build.props`, `docs/PFLICHTENHEFT.md`, `docs/ARCHITEKTUR.md`, `docs/ROADMAP.md`, `docs/PROJEKT_UEBERBLICK.md`, `docs/GERAETE_BEISPIELE.md`, `docs/END_TO_END_TESTPLAN.md`
- Code-/Teststand in `XdtDeviceBridge.App`, `XdtDeviceBridge.Core`, `XdtDeviceBridge.Infrastructure` und `XdtDeviceBridge.Tests`

## 1. Kurzfazit

Die Dokumentation passt grundsaetzlich zum aktuellen Projektstand. README, Roadmap, Projektueberblick und Architektur beschreiben den stabilen Kern mit MEDISTAR und NIDEK ARK1S, die manuell gestartete periodische Automatik, XDT-Anhaenge fuer AIS, den Baukasten-Testexport und den sicheren Templatepaket-Import weitgehend zutreffend.

Neue Arbeitsleitlinie: Der Aktivierungsassistent wird vorerst nicht weiter ausgebaut. Der Produktwert liegt nun zuerst in fertigen Geraeteprofilen und Templatepaketen, damit Anwender den Baukasten moeglichst selten brauchen.

Kritische Abweichungen, bei denen die Dokumentation eine produktiv fertige Funktion behauptet, die im Code eindeutig fehlt, wurden nicht gefunden. Es gibt aber kleinere Unschaerfen:

- `docs/ARCHITEKTUR.md` enthaelt im XDT-Anhang-Abschnitt noch einzelne Formulierungen aus einer frueheren Vorbereitungsphase, waehrend derselbe Abschnitt spaeter bereits die produktive Linkausgabe ueber `6302` bis `6305` beschreibt.
- `CHANGELOG.md` enthaelt im historischen Abschnitt `0.1.0-prototype` noch offene Punkte, die im spaeteren `Unreleased`-Abschnitt inzwischen teilweise umgesetzt sind. Das ist historisch korrekt, kann aber beim Querlesen irritieren.
- `docs/PFLICHTENHEFT.md` ist weiterhin eher Zielbild als Iststand. Aussagen zu SQLite und vollstaendiger Ordnerueberwachung muessen deshalb als Zielanforderungen gelesen werden.
- Die nach dem Sicherungspunkt wieder entfernten MEDISTAR-/AIS-Exporttemplate-Default-Arbeiten sind im aktuellen Code nicht vorhanden. AIS-/MEDISTAR-Default-Feldkennungen und automatische 6330-Vorschlagsregeln sind bewusst zurueckgestellt; bis zu einem neuen Fachkonzept sollen dazu keine Codex-Umsetzungsauftraege erteilt werden.

Besonders stabil wirken aktuell:

- MEDISTAR + NIDEK ARK1S Kernworkflow
- MEDISTAR + NIDEK ARK1S + XDT-Anhang-Link im Pflicht-Anhang-Praxislauf
- zentrale XDT-Erzeugung ueber `XdtExportBuilder`
- BuiltIn/UserDefined-Profiltrennung
- sichere Templatepaket-Importpipeline bis UserDefined-Uebernahme
- stabilisierte Templatepaket-Importvorschau in der App: Vorschau-Erstellung ist UI-nah testbar, der Import-Button wird waehrend der Vorschau gesperrt, rekursive Auswahlereignisse werden abgefangen, leere Abhaengigkeitsauflösung wird erklaert und sichtbare Hinweise/Warnungen sind deutsch
- stabilisierte Grundlage fuer abdockbare Geraeteanbindungsfenster im Tab `Verarbeitung`: Abdocken/Andocken ueber transparenten Symbolbutton `🗗`, Pin/TopMost `🔝` pro Floating-Fenster, Positionsmerken `📌`, dezente `-`/`+`-Intervallbuttons, sicheres X-Schliessen, Start-Wiederherstellung nach sicherer MainWindow-Anzeige, Radar pro Schnittstellenprofil und erste automatische Oeffnung bei Monitoring-Aktivitaet
- Datei- und Paketlogik fuer stabile AIS-/Geraete-/XDT-Anhangdateien
- neue kompakte Geraete-/Template-Matrix als Prioritaetenbasis
- offizielle Paketvorlage fuer MEDISTAR + NIDEK ARK1S inklusive reproduzierbarem Export-/Import-Testweg
- NIDEK AR360 / AR-360A als praktisch validierter Auto-Refraktometer-Workflow fuer XDT-Rueckgabe mit BuiltIn-Profilen, ARMedian-Ausgabe und selektivem Templatepaket-Pfad

Vorbereitet, aber noch nicht als produktiv abgenommen:

- V2-Geraeteprofile fuer LM7/LM7P, NT530P, TOPCON CL300, TOPCON KR800 und TOPCON TRK2P
- fertige, auslieferbare ZIP-Templatepakete fuer diese vorbereiteten Geraeteprofile; fuer ARK1S und AR360 sind Referenzdokumentation und temporaer erzeugte Export-/Import-Tests vorhanden, offizielle ZIP-Artefakte folgen erst nach Release-Regel
- Systray-Betrieb, automatisches Zurueckdocken und einstellbare Rueckdock-Zeit fuer abdockbare Geraeteanbindungen
- weitere End-to-End-Testfaelle der automatischen AIS-/Geraete-/XDT-Anhang-Verarbeitung; ein Pflicht-Anhang-Praxislauf mit MEDISTAR + NIDEK ARK1S ist dokumentiert, weitere Faelle bleiben offen
- Aktivierungsassistent fuer importierte Schnittstellenprofile; read-only Backend-Bewertung, UI-Pruefvorschau, vorbereitende Aktivierungsvorschau und technische Guard-Schicht sind vorhanden. Der Dialog `Aktivierung vorbereiten` ist auf die schlanke V1 reduziert: Bewertung, technische Freigabe, Blocker, Warnungen, Hinweise und Sicherheitshinweis; produktive Aktivierung bleibt offen.
- UI-Korrektur: Die Bereiche `Ordnerbereinigung`, `Archivierung` und `Pruefung vor Aktivierung` im Tab `Schnittstellenprofile` sind wieder sauber getrennt und ueberlappen nicht mehr.
- Praktische Windows-Sichtpruefung: Der Bereich `Pruefung vor Aktivierung` und der scrollbare Dialog `Aktivierung vorbereiten` sind fuer den aktuellen Vorschau-Status abgenommen; Lesbarkeit, Abschnittsgliederung, reduzierte Redundanz und Sicherheitshinweis wurden positiv bewertet.
- Lizenzsignatur, harte Lizenzdurchsetzung und Installer/Deployment
- AIS-/MEDISTAR-Exporttemplate-Default-Konzept bewusst zurueckgestellt

## 2. Aktueller Iststand der App

### Version

- `VERSION`: `0.1.0-prototype`
- `Directory.Build.props`: `Version` und `InformationalVersion` ebenfalls `0.1.0-prototype`
- `AssemblyVersion` und `FileVersion`: `0.1.0.0`

### Validierter Kernworkflow

- Lokale WPF-Desktop-App.
- Dateibasierte Bridge zwischen AIS und augenaerztlichen Messgeraeten.
- Praktischer Kernfokus: MEDISTAR + NIDEK ARK1S.
- MEDISTAR-kompatibler XDT-Export mit:
  - `8000 = 6310`
  - Patientendaten wie `3000`, `3101`, `3102`, `3103`
  - Untersuchungsart `8402`
  - Ergebniszeilen `6228` fuer rechts/links im validierten ARK1S-Workflow
- Keine Cloud, kein Windows-Dienst, kein Autostart, keine Verarbeitung beim App-Start.

### Tabs

Die App hat aktuell vier Haupt-Tabs:

- `Verarbeitung`
- `Profile & Templates`
- `Schnittstellenprofile`
- `Lizenz`

### Verarbeitung

Der Tab `Verarbeitung` ist auf Betrieb und Automatik ausgerichtet:

- aktive Schnittstellenprofile
- manuell startbare und stoppbare Ueberwachung
- Checkbox fuer automatische Verarbeitung gefundener Dateipaare
- Anzeige gefundener Dateipaare bzw. Verarbeitungspakete
- Status-/Logmeldungen
- alter manueller Diagnosebereich als eingeklappter Rueckfallbereich

Die automatische Verarbeitung startet nicht beim App-Start. Sie laeuft nur nach bewusstem Start der Ueberwachung und nur mit aktivierter automatischer Verarbeitung.

### Profile & Templates

Der Tab `Profile & Templates` enthaelt:

- Profiluebersicht
- BuiltIn- und UserDefined-Profile
- Exportprofil-Entwurf
- sichere Wartung fuer UserDefined-Exportprofile und Exportregeln
- Templatepaket Export/Import
- Importvalidierung, Konfliktanalyse, Importplan, Dry-Run, Importvorschau und sichere UserDefined-Uebernahme
- stabilisierte und verstaendlichere Importvorschau fuer Templatepakete; Fehler werden als Status angezeigt, waehrend der Vorschau kann kein zweiter Import parallel gestartet werden, Konflikte starten sicher mit `Ueberspringen`, Zielnamen fuer bewusst gewaehlte Kopien sind editierbar, und leere Abhaengigkeitstabellen zeigen einen kurzen deutschen Hinweis
- Baukastenbereich `Test & Vorschau`

Der Baukasten `Test & Vorschau` kann:

- AIS-Testdatei laden
- Geraetedatei laden
- XDT-Anhang aus beliebigem Speicherort einlesen
- Messwerte pruefen
- Gesamtexport-Vorschau XDT anzeigen
- Testexport erstellen

Beim Baukasten-Testexport wird der Anhang physisch in den gewaehlt Testordner kopiert, waehrend `6305` in der XDT-Datei den simulierten Zielpfad aus dem Schnittstellenprofil verwendet. Exportprofile und BuiltIn-Profile werden dadurch nicht veraendert.

Der Baukasten ist nicht der gewuenschte Standardweg fuer Anwender. Standard soll ein fertiges Geraeteprofil plus fertiges Templatepaket sein; der Baukasten bleibt fuer Sonderfaelle, Tests, Vorschau und kundenspezifische Anpassungen.

UserDefined-Exportprofile koennen im Tab `Profile & Templates` geloescht werden, solange kein Schnittstellenprofil sie verwendet. Exportregeln koennen aus UserDefined-Exportprofilen entfernt werden. Die Buttons sind nach Regel-Aktionen und Profil-Aktionen gruppiert; deaktivierte Loeschaktionen zeigen per Tooltip den Schutzgrund. BuiltIn-Exportprofile bleiben geschuetzt; es werden keine Schnittstellenprofile automatisch angepasst, keine Exportdateien geloescht und keine Verarbeitung gestartet.

### Schnittstellenprofile

Schnittstellenprofile verbinden AIS-Profil, Geraeteprofil, Exportprofil, Ordner und XDT-Anhang-Einstellungen.

Aktuelle Felder und Konzepte:

- AIS-Importordner
- Geraete-Importordner
- Exportordner ans AIS
- Archivordner
- Fehlerordner
- Ordnerabfrage-Intervall, Standard `5` Sekunden, Minimum `1`
- Wartezeit auf Geraetedatei, Standard `10` Minuten
- Archivierungsmodus `Copy`/`Move`
- Archiv-/Fehleroptionen
- XDT-Anhang Importordner
- XDT-Anhang Exportordner
- XDT-Anhang Dateiname
- XDT-Anhang Uebertragung `Copy`/`Move`, Standard `Move`
- XDT-Anhaenge fuer AIS automatisch verarbeiten, Standard aus
- XDT-Anhang ist `Optional` oder `Required`
- Wartezeit auf XDT-Anhang, Standard `30` Sekunden
- Dateistabilitaet abwarten, Standard `2` Sekunden
- XDT-Felder `6302`, `6303`, optional `6304`, `6305`

### Aktivierungsassistent fuer importierte Schnittstellenprofile - Uebergabestand

Der Aktivierungsassistent ist als schlanke V1-Vorschau vorhanden. Er bewertet und visualisiert importierte beziehungsweise UserDefined-Schnittstellenprofile, fuehrt aber keine produktive Aktivierung aus. Es gibt keinen produktiven Aktivieren-Button, keine automatische Aktivierung, keine Profiländerung und keine Speicherung.

Im Tab `Schnittstellenprofile` gibt es den Bereich `Pruefung vor Aktivierung` mit Gesamtstatus, Aktivierbarkeit, Zaehlern, strukturierter Ordnerpruefung, strukturierter XDT-Anhang-Konfiguration, Tabelle `Alle Pruefpunkte` sowie den Buttons `Pruefung aktualisieren` und `Aktivierung vorbereiten`.

Der Dialog `Aktivierung vorbereiten` ist ein reines, scrollbares Preview-Fenster mit OK-/Schliessen-Aktion. Er zeigt nur noch V1-relevante Informationen:

- Profilname
- Status
- `Aktivierbar nach V1`
- technische Guard-Entscheidung
- Blocker
- Warnungen
- Hinweise
- Sicherheitshinweis

Die fachliche Service-Kette lautet jetzt:

```text
InterfaceProfileActivationEvaluationService
=> InterfaceProfileActivationGuardService
=> InterfaceProfileActivationPreparationPreviewService / UI-Vorschau
```

V1-Regel:

- `Ready` ohne Blocker und ohne Warnungen kann spaeter grundsaetzlich aktivierbar sein.
- `ReadyWithWarnings` wird angezeigt, aber in V1 nicht produktiv aktiviert.
- `Blocked`, `Unknown` und nicht eindeutig bewertete Zustaende werden nie aktiviert.
- BuiltIn wird nie direkt veraendert.
- Aktivierung bleibt auf UserDefined-Schnittstellenprofile begrenzt.

Der `InterfaceProfileActivationExecutorStub` bleibt eine defensive Backend-Stufe ohne produktive Wirkung. Im `ValidateOnly`-Modus kann er optional frisch ueber den ActivationProfileStore laden und Evaluation + Guard simulieren. Save bleibt DryRun, `IsActive` wird nicht gesetzt, es wird nichts gespeichert und keine Verarbeitung gestartet. `Activate` ist weiterhin nicht produktiv implementiert.

Bewusst entfernt beziehungsweise nicht mehr V1-Bestandteil sind produktive Warnungsbestaetigung, `ActivationPlan`-/PlannedSteps-Architektur, Deaktivierungsmodus, Fingerprint-/Audit-/Rollen-Kontext und `ReadyWithWarnings` als Produktivpfad.

Sicherheitsgrenzen:

- keine echte Aktivierung
- kein Aktivieren-Button
- keine Warnungsbestaetigung
- keine Aenderung an `IsActive`
- keine Aenderung an `IsAttachmentProcessingEnabled`
- keine Profiländerung und keine Speicherung
- keine Änderung an BuiltIn-Profilen
- keine Datei-/Ordneroperationen
- keine produktive Verarbeitung
- keine neue MEDISTAR-/AIS-Exporttemplate-Default-Logik
- keine automatische 6330-Zeilentypautomatik
- keine harte Lizenzsperre

Die offene V1-Entscheidung bleibt bewusst geparkt: Eine spaetere Aktivierung muesste `IsActive` als Schreibpunkt, eine sichere UserDefined-Store-Methode, frisches Laden sowie finale Evaluation + Guard direkt vor Speicherung klaeren. Fuer die naechsten Schritte hat diese Arbeit keine Prioritaet gegenueber fertigen Geraeteprofilen und Templatepaketen.

Die zuletzt bestaetigte technische Absicherung dieses Standes:

- `dotnet build XdtDeviceBridge.sln` erfolgreich, `0` Warnungen, `0` Fehler
- `dotnet test XdtDeviceBridge.sln` erfolgreich, `913` Tests bestanden, `0` fehlgeschlagen, `0` uebersprungen

### Lizenz

Der Tab `Lizenz` enthaelt:

- InstallationInfo
- Lizenzanfrage exportieren
- Lizenzdatei importieren
- Lizenzstatus anzeigen
- Bewertung aktiver lizenzpflichtiger Schnittstellenprofile
- Karenzzeitmodell

Eine harte produktive Lizenzsperre und digitale Signaturpruefung sind nicht aktiv umgesetzt.

### XDT-Anhaenge fuer AIS

Der Code enthaelt Services und Tests fuer:

- Scannen unterstuetzter Anhaenge in der obersten Ebene
- unterstuetzte Typen wie PDF, JPG/JPEG, PNG, TIF/TIFF, DCM und TXT
- Stabilitaetspruefung vor Verarbeitung
- eindeutige Kandidatenauswahl nur bei genau einem stabilen unterstuetzten Anhang
- Copy/Move-Transfer mit Kollisionsschutz
- Aufbau externer AIS-Linkfelder:
  - `6302` Dokumentenname
  - `6303` Dateiformat
  - `6304` Beschreibung optional
  - `6305` vollstaendiger Dateipfad
- zentrale Laengenpraefix-Erzeugung ueber `XdtExportBuilder`

Mehrere unterstuetzte Anhaenge werden nicht automatisch zugeordnet. Instabile Anhaenge werden nicht verschoben, verlinkt oder exportiert.

### Paket-Wartelogik

Die Paketlogik ist zweistufig:

1. Eine stabile AIS-Datei wartet auf eine stabile Geraetedatei.
2. Erst nach vollstaendigem AIS-/Geraete-Paar beginnt die XDT-Anhang-Wartezeit.

Weitere Regeln:

- `DeviceFileWaitTimeoutMinutes`, Standard `10`
- neuere AIS-Datei ersetzt aeltere wartende AIS-Datei
- abgelaufene AIS-Auftraege erzeugen keinen Export
- optionaler XDT-Anhang: nach Timeout Export ohne Anhang
- verpflichtender XDT-Anhang: nach Timeout Blockade/Fehler
- mehrere Anhaenge: keine automatische Zuordnung; optional Warn-/Skip-Verhalten, Pflicht blockiert

### Dateistabilitaet

- `FileStabilityService` prueft Groesse und Aenderungszeitpunkt.
- AIS-, Geraete- und Anhangdateien werden erst verarbeitet, wenn sie stabil sind.
- Instabile Dateien werden spaeter erneut betrachtet und nicht verschoben, geloescht oder verlinkt.

### Scan-Intervall

- Es gibt keinen `FileSystemWatcher`.
- Die Ueberwachung nutzt periodischen Scan.
- `PeriodicAutoImportScanService` nutzt das pro Schnittstellenprofil konfigurierte Intervall, Standard `5` Sekunden.

### Archivierung und Fehlerablage

- Bekannte erfolgreich verarbeitete AIS-/Geraetedateien koennen nach Profiloption archiviert werden.
- Archivmodus `Copy` oder `Move`.
- Fehlerablage kopiert bekannte betroffene Dateien in den Fehlerordner und schreibt `error.txt`.
- Unbekannte Dateien werden nicht geloescht oder verschoben.
- Exportordner werden nicht pauschal bereinigt.

### Sicherheitsentscheidungen

- BuiltIn-Profile werden nicht ueberschrieben.
- UserDefined-Profile werden separat gespeichert.
- Keine Verarbeitung beim App-Start.
- Kein Windows-Dienst.
- Kein Autostart.
- Kein FileSystemWatcher.
- Keine unbekannten Dateien anfassen.
- Keine pauschale Ordnerleerung.
- Exportordner nicht bereinigen.
- Instabile Dateien nicht verarbeiten.
- Mehrere XDT-Anhaenge nicht automatisch zuordnen.
- Keine medizinische Bewertung.
- Keine harte Lizenzsperre ohne gesonderte Spezifikation.

## 3. Praktisch validiert

Belastbar validiert bzw. testseitig abgesichert sind aktuell:

| Bereich | Validierungsgrad | Grundlage |
| --- | --- | --- |
| MEDISTAR + NIDEK ARK1S Kernworkflow | praktisch und testseitig validiert | README, Projektueberblick, Core-/Pipeline-Tests |
| MEDISTAR + NIDEK ARK1S + XDT-Anhang-Link | praktisch validiert | `docs/E2E_TESTPROTOKOLL_MEDISTAR_ARK1S_XDT_ANHANG.md` |
| MEDISTAR-kompatibler XDT-Export | testseitig validiert | `XdtExportBuilderTests`, `CorePipelineEndToEndTests`, ARK1S-bezogene Tests |
| XDT-Laengenpraefixe | testseitig validiert | `XdtExportBuilderTests`, Linkfeld-Adapter-Tests |
| XDT-Anhang-Linkfelder `6302` bis `6305` | praktisch fuer MEDISTAR + ARK1S Pflicht-Anhang und testseitig validiert | Praxisprotokoll, Attachment-, Coordinator-, ManualProcessor- und BuilderTestExport-Tests |
| Baukasten-Testexport mit simuliertem 6305-Zielpfad | testseitig validiert | `BuilderTestExportServiceTests` |
| Automatische Paketlogik AIS -> Geraet -> XDT-Anhang | testseitig validiert | `AutoImportPackageStateServiceTests`, `AutoImportPairProcessingCoordinatorTests`, `AttachmentPackageDecisionServiceTests` |
| Templatepaket-Importpipeline bis UserDefined-Uebernahme | E2E-nah testseitig validiert | `TemplatePackageImportEndToEndTests` und zugehoerige Service-Tests |
| ARK1S-Referenzpaket Export/Import | reproduzierbar testseitig validiert, inklusive UI-nahem Importvorschau-Pfad und sicherem Konfliktstandard `Ueberspringen` | `MedistarNidekArk1sTemplatePackageTests` |
| AR360-Referenzpaket Export/Import | reproduzierbar testseitig validiert, inklusive selektivem Paketinhalt, Importvorschau, DryRun und sicherer UserDefined-Kopie | `MedistarNidekAr360TemplatePackageTests` |
| Abdockbare Geraeteanbindungsfenster | UI-Grundlage testseitig fuer State, Persistenz, verzoegerte Wiederherstellung und Auto-Abdocken bei Monitoring-Aktivitaet abgesichert | `InterfaceProfileFloatingWindowStateServiceTests`, `InterfaceProfileFloatingWindowStateRepositoryTests`, `InterfaceProfileFloatingWindowRestoreGateTests`, `InterfaceProfileAutoDetachServiceTests` |
| BuiltIn/UserDefined-Schutz | testseitig validiert | `ProfileCatalogServiceTests`, TemplateImport-Tests |

Teilweise praktisch abgeschlossen ist die manuelle Praxisabnahme fuer MEDISTAR + NIDEK ARK1S + XDT-Anhang-Link: Pflicht-Anhang vorhanden, Pflicht-Anhang fehlt/Fehlerfall und MEDISTAR-Livesystem-Linkaufruf sind bestanden. Noch offen ist die vollstaendige manuelle Praxisabnahme der weiteren Faelle aus `docs/END_TO_END_TESTPLAN.md`. Fuer die weitere Durchfuehrung steht die ausfuellbare Vorlage `docs/E2E_TESTPROTOKOLL_TEMPLATE.md` bereit.

## 4. Vorbereitet, aber noch nicht produktiv validiert

- NIDEK AR360 / AR-360A: Auto-Refraktor-XDT-Rueckgabe praktisch validiert; Referenzpaket-Export/Import testseitig abgesichert; XDT-Anhangfall und offizielles ZIP-Artefakt offen
- NIDEK LM7/LM7P
- NIDEK NT530P
- TOPCON CL300
- TOPCON KR800
- TOPCON TRK2P
- fertige Templatepaket-Dateien fuer vorbereitete Geraeteprofile
- vollstaendiger Profil-Assistent fuer unbekannte Geraete
- Geraete-Datei-Explorer
- weitere AIS-Systeme ausser MEDISTAR
- AIS-/MEDISTAR-Exporttemplate-Default-Konzept mit Feldkennungen je Untersuchungsart: bewusst zurueckgestellt, neues Fachkonzept erforderlich
- automatische MEDISTAR `6330`-Default-Zusatzzeilen: bewusst zurueckgestellt
- produktive Aktivierung importierter Schnittstellenprofile; die aktuelle Pruefung, Guard-Schicht und V1-Preview sind rein lesend vorbereitet
- produktive `ActivationExecutor`-Implementierung mit frischem Laden, finaler Evaluation + Guard und sicherer UserDefined-Speicherung
- ReplaceExisting fuer bestehende UserDefined-Profile
- digitale Lizenzsignatur
- produktive Lizenzsperre
- Installer und Deployment
- optionaler spaeterer Windows-Dienst, Autostart oder FileSystemWatcher

## 5. Dokumentationsabgleich

| Bereich | Aussage in Dokumentation | Tatsaechlicher Code-/Projektstand | Bewertung | Empfehlung |
| --- | --- | --- | --- | --- |
| Version | `0.1.0-prototype` in README, VERSION und Buildprops. | Versionen stimmen ueberein. | passt | Keine Aenderung noetig. |
| Automatikmodell | Keine Verarbeitung beim App-Start, manuell gestartete Ueberwachung, periodischer Scan. | Code nutzt `PeriodicAutoImportScanService`, keine automatische Startlogik. | passt | Keine Aenderung noetig. |
| FileSystemWatcher / Dienst / Autostart | Dokumentation sagt: nicht enthalten. | Keine entsprechende Implementierung erkennbar. | passt | Als Sicherheitsentscheidung beibehalten. |
| Paket-Wartelogik | Roadmap, Projektueberblick und E2E-Testplan beschreiben zweistufige Logik. | `AutoImportPackageStateService` und Coordinator bilden die Logik ab. | passt | Praktischen E2E-Testplan ausfuehren. |
| XDT-Anhaenge | Dokumentation beschreibt sichere automatische Vorbereitung und Linkausgabe `6302` bis `6305`. | Services, Coordinator, ManualProcessor und Testexport enthalten diese Logik. | passt | In Architektur fruehere "nur vorbereitet"-Formulierungen glaetten. |
| Baukasten Test & Vorschau | Projektueberblick beschreibt Dateiauswahl, simulierten 6305-Pfad und Testexport. | `BuilderTestExportService` und UI enthalten diese Funktion. | passt | Keine Aenderung noetig. |
| Templatepaket-Export/-Import und Exportprofil-Wartung | Roadmap/Projektueberblick beschreiben selektiven Export, Analyse, Plan, Dry-Run, UI-Vorschau, Benutzerwahl, UserDefined-Uebernahme und sichere UserDefined-Wartung. | Export waehlt ein Schnittstellenprofil als Paketbasis und nimmt nur benoetigte Abhaengigkeiten auf; Import-Services, UI-Glue, stabilisierte Preview-Erstellung, sicherer Default `Ueberspringen`, editierbare Kopie-Zielnamen, deutscher Leerzustand fuer Abhaengigkeiten und E2E-nahe Tests sind vorhanden. UserDefined-Exportprofile koennen nur unreferenziert geloescht werden; Exportregeln koennen nur aus UserDefined-Exportprofilen entfernt werden. | passt | Praktischen selektiven App-Export und erneuten App-Import pruefen; Loeschen/Entfernen mit UserDefined-Kopien testen; ReplaceExisting offen fuehren; Aktivierungsassistent vorerst parken. |
| MEDISTAR/NIDEK ARK1S | Als validierter Kernworkflow beschrieben. | BuiltIn-Profile, Parser, Mapping und Tests vorhanden. | passt | Als stabilen Kern beibehalten. |
| V2-Geraeteprofile | Als vorbereitet, nicht produktiv validiert beschrieben. | BuiltIn-Profile und Beispiel-Doku vorhanden; keine Praxisvalidierung behauptet. | passt | Pro Geraet Validierungsplan abarbeiten. |
| Lizenzsystem | Anzeige, Lizenzdatei, Karenzzeitmodell; keine harte Sperre. | Code enthaelt Lizenzmodelle, Anzeige und Karenzzeitservices; keine harte Durchsetzung. | passt | Signatur und Sperrregeln separat spezifizieren. |
| Installer/Deployment | In Roadmap als offen. | Kein Installer-Projekt erkennbar. | passt | Deployment-Konzept spaeter erstellen. |
| SQLite/JSON-Speicherung | Pflichtenheft nennt SQLite als Ziel, Projektueberblick beschreibt JSON/AppData. | Code nutzt JSON-Dateien unter LocalAppData. | unklar | Architekturentscheidung treffen: JSON bewusst beibehalten oder SQLite neu planen. |
| Profil-Assistent | Roadmap fuehrt Assistent als offen. | Kein vollstaendiger Assistent; Profilkatalog und Entwurfsfunktionen existieren. | passt | Geraete-Datei-Explorer als naechsten vorbereitenden Schritt planen. |
| PDF-/EV-Dokumentenerzeugung | Als Zukunft/offen beschrieben. | Keine PDF-Erzeugung und keine MEDISTAR-interne EV-Zeile. 6302-6305-Link ist der aktuelle Weg. | passt | EV vs. strukturierter Link fachlich entscheiden. |
| MEDISTAR-Feldkennungen / Exporttemplates | Roadmap und Pflichtenheft markieren AIS-/MEDISTAR-Default-Exporttemplates als zurueckgestellt. | Keine `AisExportTemplate`-/Default-MEDISTAR-Template-Implementierung vorhanden. | passt | Keine Umsetzung beauftragen, bis ein neues Fachkonzept vorliegt. |
| Tests / Testabdeckung | Doku nennt automatisierte Abdeckung und E2E-nahe Templateimport-Tests. | Testprojekt enthaelt umfangreiche Service- und E2E-nahe Tests. Letzter bekannter Stand: 811 Tests gruen vor dieser Doku-Aenderung. | passt | Build/Test nach dieser Doku-Aenderung erneut ausfuehren. |
| Pflichtenheft als Iststand | Pflichtenheft enthaelt Zielbild-Aussagen zu SQLite und spaeteren Funktionen. | Code ist Prototyp mit JSON und begrenztem Funktionsumfang. | unklar | Pflichtenheft deutlicher als Ziel-/Anforderungsdokument kennzeichnen. |
| CHANGELOG Historie | Historischer Abschnitt nennt einige damals offene Punkte. | Unreleased dokumentiert spaeter umgesetzte Schritte. | passt mit Unschaerfe | Bei naechster Version Changelog klar in releasefaehige Abschnitte schneiden. |

## 6. Offene Punkte

| Prioritaet | Thema | Aktueller Status | Was fehlt konkret? | Empfohlener naechster Schritt | Risiko, wenn offen bleibt | Abhaengigkeiten |
| --- | --- | --- | --- | --- | --- | --- |
| hoch | E2E-Testplan praktisch weiter ausfuehren | Testplan, Durchfuehrungsschritte und Protokollvorlage sind vorhanden; MEDISTAR + ARK1S + Pflicht-XDT-Anhang wurde am 2026-05-11 praktisch bestanden dokumentiert. | Weitere manuelle Praxisprotokolle fuer die restlichen Testfaelle fehlen, insbesondere optionale Anhaenge, Mehrfachanhaenge, instabile Dateien und nicht unterstuetzte Dateien. | Restliche Faelle aus `docs/END_TO_END_TESTPLAN.md` mit `docs/E2E_TESTPROTOKOLL_TEMPLATE.md` abarbeiten. | Nicht getestete Randfaelle koennen im Praxisbetrieb auffallen. | Testdaten, lokale Ordner, ARK1S-Beispieldateien |
| hoch | Produktive Stabilisierung MEDISTAR + ARK1S + XDT-Anhang | Pflicht-Anhang vorhanden/fehlt und MEDISTAR-Linkaufruf sind praktisch validiert. | Breitere Praxisabnahme mit optionalem Anhang, Mehrfachanhang, Archiv-/Fehlerablage und Wartezeiten. | Naechsten Praxislauf fuer optionale Anhaenge und Mehrfachanhang-Sicherheit protokollieren. | Unerwartete Timing- oder Bedienfaelle koennen in noch nicht getesteten Varianten auffallen. | E2E-Testplan, Testanhaenge |
| hoch | Fertige Geraeteprofile und Templatepakete | BuiltIn-Geraeteprofile und MEDISTAR-Exportprofile sind fuer sieben Geraete vorhanden; praktisch validiert sind MEDISTAR + NIDEK ARK1S und die AR360-Auto-Refraktor-XDT-Rueckgabe. Matrix, ARK1S-/AR360-Referenzdokumentation, selektiver Export, reproduzierbare Export-/Import-Tests und stabilisierter App-Preview-Pfad sind vorhanden. | Dauerhafte ARK1S-/AR360-ZIP-Release-Artefakte und AR360-XDT-Anhangfall fehlen; fuer LM7/LM7P, NT530P und TOPCON fehlen Repository-Testdaten/Praxisabnahmen. | Release-Regel fuer ARK1S/AR360 anwenden, dann LM7/LM7P mit repraesentativen Dateien validieren. | Anwender muessen sonst weiterhin den Baukasten nutzen. | BuiltIn-Profile, TemplatePackageExporter, Testdaten |
| mittel | Templatepaket-Import Aktivierungsassistent | Sicherer Import als UserDefined vorhanden; Backend-Bewertung, UI-Pruefvorschau, vorbereitende Aktivierungsvorschau und Guard-Schicht sind read-only vorhanden. | Bewusste Benutzerfreigabe und eigentliche Aktivierung fehlen weiterhin. | Vorerst parken; nur als Sicherheits-/Regressionsstand beibehalten. | Importierte Profile bleiben sicher, aber noch nicht per finalem V1-Klick aktivierbar. | Profilkatalog, Evaluation, Guard |
| mittel | Abdockbare Geraeteanbindungsfenster | Manuelles Abdocken/Andocken per transparentem `🗗`, sicheres X-Schliessen, Pin/TopMost `🔝` pro Floating-Fenster, Positionsmerken `📌`, dezente `-`/`+`-Intervallbuttons, verzoegerte Neustart-Wiederherstellung, Radar und erste automatische Oeffnung bei Monitoring-Aktivitaet sind vorhanden. | Systray, Auto-Zurueckdocken und Rueckdock-Zeit fehlen bewusst. | Praktisch mit AR360/ARK1S testen und danach Rueckdock-/Systray-Verhalten separat planen. | Ohne Auto-Zurueckdocken bleiben automatisch geoeffnete Fenster sichtbar, bis der Benutzer sie andockt. | Verarbeitungstab, Monitoring-Karten |
| hoch | Lizenzsignatur | Lizenzanzeige und Karenzzeitmodell vorhanden. | Digitale Signaturpruefung, Schluesselmodell, Manipulationsschutz. | Signaturformat und Validierungsservice spezifizieren. | Lizenzdateien sind vor produktiver Sperre nicht ausreichend gesichert. | Lizenzmodell, Supportprozess |
| mittel | ReplaceExisting fuer UserDefined | Bewusst deaktiviert; Executor blockiert/ueberspringt. | Konfliktloesungsdialog, Sicherung/Backup, explizite Benutzerentscheidung. | Erst nach konkreter Profil-/Templatepaket-Arbeit als separates Import-Epic planen. | Anwender muessen Konflikte ueber Kopien loesen, Katalog kann wachsen. | Importvorschau, SelectionService |
| mittel | Manuelle Zielnamen-/ID-Bearbeitung im Templateimport | Automatische Kopienamen vorhanden. | UI zum Bearbeiten vorgeschlagener Namen/IDs. | Nur ergaenzen, wenn Anwenderfeedback Bedarf zeigt. | Importnamen koennen weniger sprechend sein. | Importplan, SelectionService |
| mittel | Geraete-Datei-Explorer | Noch kein vollstaendiger Explorer. | Datei anzeigen, SourcePaths untersuchen, Messwerte markieren, Kandidaten fuer Exportregeln uebernehmen. | Kleinen read-only Explorer fuer XML/Geraetedateien bauen. | Neue Geraeteprofile bleiben Codex-/Entwickleraufgabe. | XmlDeviceParser, PlaceholderDisplayHelper |
| mittel | Profil-Assistent fuer unbekannte Geraete | Noch offen. | Gefuehrtes Erstellen von Geraete-, Export- und Schnittstellenprofilen. | Nach Geraete-Datei-Explorer planen. | Skalierung auf neue Geraete bleibt langsam. | Geraete-Datei-Explorer, ProfileCatalog |
| mittel | NIDEK LM7/LM7P produktiv validieren | Profile und Beispiele vorbereitet. | Echte/repraesentative Dateien, Vergleich mit Praxisanforderung, Exportabnahme. | LM7-Testpaket mit synthetischen/echten Musterdateien erstellen und validieren. | Vorbereitetes Profil koennte in Details falsch mappen. | Testdaten, MEDISTAR-Anforderungen |
| mittel | NIDEK NT530P produktiv validieren | Profile vorbereitet. | Echte Dateien fuer Tonometry/Pachymetry und ggf. Attachmentfaelle. | Geraetespezifischen E2E-Testplan ergaenzen. | Falsche oder unvollstaendige Messwertuebernahme. | Testdaten |
| mittel | TOPCON CL300 produktiv validieren | Profile vorbereitet. | Namespace-/Dateistruktur mit echten Beispielen pruefen. | CL300-Beispieldateien sammeln und Parserpfade bestaetigen. | Vorbereitete SourcePaths koennen unvollstaendig sein. | Testdaten |
| mittel | TOPCON KR800 produktiv validieren | Profile vorbereitet. | REF/KM/SBJ-Strukturen mit echten Daten pruefen. | KR800-Testdaten auswerten und Exportregeln validieren. | Mehruntersuchungsdaten koennen falsch gruppiert werden. | Testdaten |
| mittel | TOPCON TRK2P produktiv validieren | Profile vorbereitet. | TM/CCT/IOP-Strukturen und Einheiten pruefen. | TRK2P-Testdaten auswerten und Exportregeln validieren. | IOP/CCT-Ausgabe koennte fachlich unpassend sein. | Testdaten |
| niedrig | AIS-/MEDISTAR-Exporttemplate-Default-Konzept | Bewusst zurueckgestellt; nach Reset nicht implementiert. | Neues Fachkonzept. Bis dahin keine Modelle, Services, UI, 6330-Automatik oder Codex-Umsetzungsauftraege dazu. | Nicht implementieren; fachliche Neubewertung ausserhalb des aktuellen Entwicklungsstrangs abwarten. | Anwender pflegen Exportregeln weiter manuell im Baukasten. | Neues Fachkonzept, MEDISTAR-Fachabstimmung |
| mittel | Weitere AIS-Profile ausser MEDISTAR | Nicht produktiv umgesetzt. | AIS-spezifische Parser, Feldkennungen, Exportregeln und Tests. | Erst nach MEDISTAR-Kern stabilisieren und Bedarf priorisieren. | App bleibt MEDISTAR-zentriert. | AIS-Dokumentation |
| mittel | SQLite vs. JSON | Code nutzt JSON; Pflichtenheft nennt SQLite als Ziel. | Bewusste Architekturentscheidung und ggf. Migrationsplan. | ADR erstellen: JSON fuer Prototyp beibehalten oder SQLite planen. | Doku und Architekturziele bleiben uneindeutig. | Profilkatalog, Deployment |
| mittel | Installer / Deployment | Offen. | Installer, Updateprozess, AppData-Pfade, Support-/Rollback-Anleitung. | Nach Praxis-E2E einen ersten MSIX/Setup-Plan erstellen. | Installation bleibt Entwickler-/Handarbeit. | Versionierung, Signatur, Support |
| mittel | Technische Installations-/Supportdoku | Teilweise vorhanden. | Klare Anleitung fuer Ordner, Berechtigungen, Backup, Logs, Fehleranalyse. | `docs/INSTALLATION_SUPPORT.md` planen. | Praxiseinrichtung wird fehleranfaellig. | Deployment-Konzept |
| mittel | Testdaten/Sample-Dateien | Tests nutzen interne Beispiele, manuelle Testdaten nicht voll paketiert. | Synthetische AIS-, Geraete- und Anhangdateien fuer E2E. | Testdatenverzeichnis mit synthetischen, patientenfreien Dateien definieren. | Manuelle Reproduktion bleibt schwierig. | Datenschutz, E2E-Testplan |
| mittel | Mehrfachanhang-Zuordnung | Absichtlich nicht automatisch. | Fachliche Regel, wenn mehrere Anhaenge vorkommen. | Zunaechst bei "keine automatische Zuordnung" bleiben; spaeter UI-/Manuell-Auswahl spezifizieren. | Pflicht-Anhang-Faelle blockieren bei mehreren Dateien. | Patientenbezug, Praxisablauf |
| mittel | MEDISTAR EV vs. 6302-6305 | Architektur bevorzugt strukturierte 6302-6305. | Entscheidung, ob EV-Anzeigezeilen jemals durch die App erzeugt werden sollen. | Bei MEDISTAR-Rueckmeldung fachlich pruefen, ansonsten 6302-6305 beibehalten. | Unklare Erwartung an Karteikartenanzeige. | MEDISTAR-Testimport |
| niedrig | Windows-Dienst spaeter ja/nein | Nicht implementiert. | Betriebsentscheidung. | Erst nach stabiler Desktop-Automatik entscheiden. | App muss manuell gestartet bleiben. | Deployment, Lizenz |
| niedrig | Autostart spaeter ja/nein | Nicht implementiert. | Betriebsentscheidung. | Mit Dienst-/Deployment-Konzept klaeren. | Nach Neustart kein automatischer Betrieb. | Deployment |
| niedrig | FileSystemWatcher spaeter ja/nein | Nicht implementiert, periodischer Scan aktiv. | Entscheidung, ob Watcher trotz Stabilitaetslogik sinnvoll ist. | Periodischen Scan in Praxis messen, erst danach entscheiden. | Scan-Latenz bleibt intervallabhaengig. | Stabilitaet, Paketlogik |
| niedrig | Archivloeschung im laufenden Betrieb | Cleanup-Service vorbereitet. | UI/Automatik fuer sichere Vorschau und Ausfuehrung. | Nur mit ausdruecklicher Preview und Schutzregeln planen. | Archiv kann wachsen. | Archivstrategie |
| niedrig | PDF-Erzeugung durch App | Nicht implementiert. | Renderer, Layout, medizinische Freigabe, Linkintegration. | Nach externem Linkworkflow bewerten. | Keine App-eigenen Befund-PDFs. | 6302-6305, MEDISTAR-Anforderungen |
| niedrig | Online-Lizenzierung | Nicht implementiert. | Server/API/Datenschutzkonzept. | Nur bei Produktbedarf planen. | Keine zentrale Lizenzverwaltung. | Signatur, Kundensupport |
| niedrig | UI-Feinschliff | Viele Bereiche funktional, aber gewachsen. | Kleine Bedien-/Layoutkorrekturen nach Praxisfeedback. | Feedback sammeln und gezielt kleine UI-Schritte machen. | Anwender finden fortgeschrittene Funktionen schwerer. | Praxisfeedback |

## 7. Priorisierte Roadmap

### Phase 1: Konsolidierung / Dokumentation / Tests

- Diese Abgleichdatei als aktuelle Orientierung nutzen.
- `docs/END_TO_END_TESTPLAN.md` mit `docs/E2E_TESTPROTOKOLL_TEMPLATE.md` praktisch ausfuehren und protokollieren.
- Synthetische Testdaten fuer AIS, ARK1S und XDT-Anhang vorbereiten.
- Kleinere Doku-Unschaerfen in Architektur/Pflichtenheft spaeter glaetten.

### Phase 2: Stabilisierung produktiver MEDISTAR + ARK1S + XDT-Anhang Workflow

- Automatische Paketlogik in realistischen Ordnern pruefen.
- Optional-/Pflicht-Anhaenge mit echten Timing-Faellen testen.
- Archiv-/Fehlerablage im Praxisablauf pruefen.
- MEDISTAR-Testimport der `6302` bis `6305`-Links dokumentieren.

### Phase 3: Fertige Geraeteprofile und Templatepakete

- `docs/GERAETE_PROFILE_TEMPLATE_MATRIX.md` als Arbeitsliste nutzen.
- MEDISTAR + NIDEK ARK1S stabil halten, den reproduzierbaren Pakettest beibehalten und die App-Importabnahme fuer ein spaeteres ZIP-Release-Artefakt nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md` vorbereiten.
- NIDEK AR360 als zweiten Referenzworkflow stabil halten; den reproduzierbaren Pakettest beibehalten, offizielles ZIP-Artefakt und ggf. XDT-Anhangtest separat planen.
- NIDEK LM7/LM7P als naechstes Geraet anhand repraesentativer Dateien validieren.
- NT530P und TOPCON-Profile erst nach Datenlage priorisieren.
- Baukasten schlank halten; keine neue Assistentenarchitektur, solange fertige Profile und Pakete fehlen.

### Phase 4: Geparkter Aktivierungsassistent

- Read-only Aktivierungsassistent ist praktisch in der UI sichtgeprueft und fuer den Vorschau-Status abgenommen; Pruefung, Guard und V1-Preview bleiben ohne produktive Wirkung.
- Produktive Aktivierung vorerst nicht weiter ausbauen.
- `docs/UI_PRUEFPROTOKOLL_AKTIVIERUNGSASSISTENT.md` als Regressionscheck fuer spaetere UI-Aenderungen weiterfuehren.
- AIS-/MEDISTAR-Default-Exporttemplates nicht umsetzen, bis ein neues Fachkonzept vorliegt.

### Phase 5: Geraete-Datei-Explorer / Profil-Assistent

- Read-only Explorer fuer Geraetedateien und SourcePaths.
- Uebernahme erkannter Messwerte in Exportregel-Entwurf.
- Gefuehrter Profil-Assistent fuer neue Geraete.

### Phase 6: Weitere Geraete validieren

- LM7/LM7P
- NT530P
- TOPCON CL300
- TOPCON KR800
- TOPCON TRK2P

### Phase 7: Lizenzierung haerten

- Digitale Signaturpruefung.
- Manipulationsschutz.
- Klare Sperrregeln und Karenzlogik.
- Optional spaeter Online-Lizenzierung.

### Phase 8: Installer / Deployment

- Installer/Updateprozess.
- Support- und Installationsdokumentation.
- Backup-/Restore-Konzept fuer LocalAppData-Konfiguration.

## 8. Risiken und Entscheidungen

| Entscheidung | Aktueller Stand | Offene Frage |
| --- | --- | --- |
| JSON vs. SQLite | JSON unter LocalAppData ist implementiert. | Bleibt JSON fuer 0.x bewusst gesetzt oder wird SQLite spaeter eingefuehrt? |
| Dienst vs. manuell gestartete App | Manuell gestartete WPF-App. | Soll es spaeter einen Windows-Dienst geben? |
| FileSystemWatcher vs. periodischer Scan | Periodischer Scan, kein Watcher. | Reicht der Scan produktiv aus? |
| Lizenzsperre | Anzeige und Karenzzeitmodell, keine harte Sperre. | Wann und nach welchen Regeln wird eine Sperre aktiviert? |
| MEDISTAR EV vs. 6302-6305 | Strukturierter externer Link ueber `6302` bis `6305`. | Soll MEDISTAR-interne EV-Anzeige jemals aktiv erzeugt werden? |
| Mehrere Anhaenge | Keine automatische Zuordnung. | Bleibt es bei Block/Warnung oder kommt eine manuelle Zuordnung? |
| Mehrere AIS-Dateien | Neuere AIS-Datei ersetzt aeltere wartende Datei. | Reicht diese Regel fuer Mehrplatz-/Mehrpatientenbetrieb? |
| Mehrere Geraete/Arbeitsplaetze | Schnittstellenprofile koennen getrennte Ordner haben. | Braucht es Arbeitsplatz-/Geraetebezeichner und Monitoring-Ansichten? |
| UserDefined-Import / ReplaceExisting | ImportAsNew/ImportAsCopy/KeepExisting/Skip vorhanden, ReplaceExisting aus. | Wann ist Ersetzen sicher genug? |
| Geraeteprofile automatisch erkennen | Nicht implementiert. | Automatisch erkennen oder weiter manuell/assistentengestuetzt erstellen? |
| MEDISTAR-Exporttemplates | Bewusst zurueckgestellt. | Neues Fachkonzept erforderlich; bis dahin keine Umsetzung und keine automatische 6330-Ergaenzung. |
| Archivloeschung | Cleanup-Service vorbereitet, keine automatische Ausfuehrung. | Soll es eine sichere, explizite UI-Aktion geben? |

## 9. Empfohlener naechster Codex-Schritt

Empfohlen wird als naechster kleiner, sicherer und testbarer Schritt:

**ARK1S- und AR360-Referenzpakete nach Release-Regel praktisch in der App importieren und danach als ZIP-Artefakte festlegen.**

Konkreter Umfang:

- keine Produktivlogik aendern
- keine BuiltIn-Profile ueberschreiben
- vorhandene ARK1S-/AR360-Paketdokumente und `MedistarNidekArk1sTemplatePackageTests` / `MedistarNidekAr360TemplatePackageTests` als Spezifikation nutzen
- Paket mit dem vorhandenen `TemplatePackageExporter` erzeugen
- Paket mit `TemplatePackageImporter` testweise einlesen
- Import in der App mit Vorschau/DryRun praktisch abnehmen
- danach LM7/LM7P-Dateien sammeln und gegen `docs/GERAETE_PROFILE_TEMPLATE_MATRIX.md` validieren
- danach Build und Tests ausfuehren

Das ist der beste naechste Schritt, weil es den Baukasten vom Normalweg zum Rueckfallwerkzeug macht und den validierten Kernworkflow in ein wiederverwendbares Anwenderpaket ueberfuehrt.

## 10. Keine Produktivlogik-Aenderungen

Dieser Abgleich beschreibt den aktuellen Stand. Produktivverarbeitung, Aktivierungslogik und BuiltIn-Profile bleiben unveraendert. Ergaenzt wurden reproduzierbare Templatepaket-Wege sowie schlanke UI-Funktionen fuer sichere UserDefined-Wartung im Profilbereich.
