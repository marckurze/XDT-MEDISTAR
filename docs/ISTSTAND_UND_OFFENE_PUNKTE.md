# Iststand und offene Punkte

Stand: 2026-05-11

Basis dieses Abgleichs:

- Repository-Stand nach Ruecksetzung auf Commit `6b9bc20bab896e7fa103748ba20e435c34d3e8cd`
- Dokumente: `README.md`, `CHANGELOG.md`, `VERSION`, `Directory.Build.props`, `docs/PFLICHTENHEFT.md`, `docs/ARCHITEKTUR.md`, `docs/ROADMAP.md`, `docs/PROJEKT_UEBERBLICK.md`, `docs/GERAETE_BEISPIELE.md`, `docs/END_TO_END_TESTPLAN.md`
- Code-/Teststand in `XdtDeviceBridge.App`, `XdtDeviceBridge.Core`, `XdtDeviceBridge.Infrastructure` und `XdtDeviceBridge.Tests`

## 1. Kurzfazit

Die Dokumentation passt grundsaetzlich zum aktuellen Projektstand. README, Roadmap, Projektueberblick und Architektur beschreiben den stabilen Kern mit MEDISTAR und NIDEK ARK1S, die manuell gestartete periodische Automatik, XDT-Anhaenge fuer AIS, den Baukasten-Testexport und den sicheren Templatepaket-Import weitgehend zutreffend.

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
- Datei- und Paketlogik fuer stabile AIS-/Geraete-/XDT-Anhangdateien

Vorbereitet, aber noch nicht als produktiv abgenommen:

- V2-Geraeteprofile fuer LM7/LM7P, NT530P, TOPCON CL300, TOPCON KR800 und TOPCON TRK2P
- weitere End-to-End-Testfaelle der automatischen AIS-/Geraete-/XDT-Anhang-Verarbeitung; ein Pflicht-Anhang-Praxislauf mit MEDISTAR + NIDEK ARK1S ist dokumentiert, weitere Faelle bleiben offen
- Aktivierungsassistent fuer importierte Schnittstellenprofile; erste read-only Backend-Bewertung, UI-Pruefvorschau, vorbereitende Aktivierungsvorschau, technische Guard-Schicht, internes Modell fuer spaetere Warnungsbestaetigung und rein lesender ActivationPlan-Service sind vorhanden, inklusive strukturierter Ordner- und XDT-Anhang-Detailanzeige sowie rein lesender Guard-Entscheidung, Warnungsbestaetigungsvorschau und ActivationPlan-Anzeige im Dialog `Aktivierung vorbereiten`; produktive Aktivierung bleibt offen
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
- Templatepaket Export/Import
- Importvalidierung, Konfliktanalyse, Importplan, Dry-Run, Importvorschau und sichere UserDefined-Uebernahme
- Baukastenbereich `Test & Vorschau`

Der Baukasten `Test & Vorschau` kann:

- AIS-Testdatei laden
- Geraetedatei laden
- XDT-Anhang aus beliebigem Speicherort einlesen
- Messwerte pruefen
- Gesamtexport-Vorschau XDT anzeigen
- Testexport erstellen

Beim Baukasten-Testexport wird der Anhang physisch in den gewaehlt Testordner kopiert, waehrend `6305` in der XDT-Datei den simulierten Zielpfad aus dem Schnittstellenprofil verwendet. Exportprofile und BuiltIn-Profile werden dadurch nicht veraendert.

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

Der Aktivierungsassistent ist als sicherer Vorbereitungsstand vorhanden. Er bewertet und visualisiert importierte beziehungsweise UserDefined-Schnittstellenprofile, fuehrt aber keine produktive Aktivierung aus. Es gibt keinen produktiven Aktivieren-Button, keine automatische Aktivierung, keine Profiländerung und keine Speicherung. Der `InterfaceProfileActivationExecutorStub` ist nur eine defensive Backend-Stufe ohne produktive Wirkung.

Im Tab `Schnittstellenprofile` gibt es den Bereich `Pruefung vor Aktivierung`. Er zeigt:

- Gesamtstatus und grundsaetzliche Aktivierbarkeit
- Zaehler fuer Blocker, Warnungen und Hinweise
- strukturierte Ordnerpruefung
- strukturierte XDT-Anhang-Konfiguration
- eingeklappte Tabelle `Alle Pruefpunkte`
- Button `Pruefung aktualisieren`
- Button `Aktivierung vorbereiten`

Der Dialog `Aktivierung vorbereiten` ist ein reines, scrollbares Preview-Fenster mit OK-/Schliessen-Aktion. Er zeigt die Aktivierungsbewertung, die technische Guard-Entscheidung, die Warnungsbestaetigungsvorschau und den `InterfaceProfileActivationPlan` kompakt gegliedert. Bestaetigungspflichtige Warnungen werden zentral im Abschnitt Warnungsbestaetigung angezeigt; Guard und Aktivierungsplan wiederholen diese Liste nicht vollstaendig. PlannedSteps werden nur kurz beschrieben und nicht ausgefuehrt.

Die fachliche Service-Kette lautet:

```text
InterfaceProfileActivationEvaluationService
=> InterfaceProfileActivationGuardService
=> InterfaceProfileActivationWarningConfirmationService
=> InterfaceProfileActivationPlanService
=> InterfaceProfileActivationPreparationPreviewService / UI-Vorschau
```

Die Bausteine haben folgende Rollen:

- `InterfaceProfileActivationEvaluationService`: bewertet Profilgrunddaten, BuiltIn-/UserDefined-Schutz, Abhaengigkeiten, Ordner, XDT-Anhang-Konfiguration und Lizenzhinweise.
- `InterfaceProfileActivationGuardService`: prueft, ob eine spaetere Aktivierungsanforderung technisch weitergehen duerfte.
- `InterfaceProfileActivationWarningConfirmationService`: bereitet nur im Speicher vor, welche Warnungen spaeter bewusst bestaetigt werden muessten.
- `InterfaceProfileActivationPlanService`: fasst Bewertung, Guard und Warnungsbestaetigung zu einem rein beschreibenden Plan zusammen.
- `InterfaceProfileActivationPreparationPreviewService`: bereitet die lesbare Dialogzusammenfassung auf.

Aktuelle Statuswerte:

- Evaluation: `NotEvaluated`, `Ready`, `ReadyWithWarnings`, `Blocked`, `Unknown`
- Guard: `Allowed`, `AllowedWithWarnings`, `RequiresWarningConfirmation`, `Blocked`, `Unknown`
- WarningConfirmation: `NotAvailable`, `MissingEvaluation`, `Blocked`, `NoWarnings`, `ConfirmationRequired`, `Unknown`
- ActivationPlan: `NotAvailable`, `Blocked`, `RequiresWarningConfirmation`, `Ready`, `ReadyWithAcceptedWarnings`, `Unknown`

Sicherheitsgrenzen:

- keine echte Aktivierung
- kein Aktivieren-Button
- keine automatische Aktivierung
- keine Aenderung an `IsActive`
- keine Aenderung an `IsAttachmentProcessingEnabled`
- keine Profiländerung und keine Speicherung
- keine Änderung an BuiltIn-Profilen
- keine produktive Warnungsbestaetigung und keine dauerhafte Speicherung einer Warnungsbestaetigung
- keine produktive `ActivationExecutor`-Implementierung; nur defensiver Backend-Stub ohne Speicherung
- keine Datei-/Ordneroperationen
- keine produktive Verarbeitung
- keine neue MEDISTAR-/AIS-Exporttemplate-Default-Logik
- keine automatische 6330-Zeilentypautomatik
- keine harte Lizenzsperre

BuiltIn-Profile bleiben geschützt. Eine direkte BuiltIn-Aktivierung oder BuiltIn-Aenderung wird konservativ blockiert; die Aktivierungsvorbereitung ist auf kontrollierte UserDefined-Schnittstellenprofile ausgerichtet. Importierte Schnittstellenprofile bleiben weiterhin inaktiv, bis eine spaetere echte Aktivierung bewusst umgesetzt und fachlich freigegeben wird.

Ordner und XDT-Anhang-Konfiguration werden angezeigt und bewertet, aber nicht ausgefuehrt. Es werden keine Ordner angelegt und keine Dateien erzeugt, kopiert, verschoben oder geloescht. Die Linkfelder `6302`, `6303`, `6304` und `6305` werden nur als Konfiguration geprüft und angezeigt. XDT-Laengenpraefixe bleiben ausschliesslich Aufgabe des `XdtExportBuilder`.

Als naechste technische Leitplanke existiert ein Interface-/Model-Skelett fuer einen spaeteren `ActivationExecutor`: `IInterfaceProfileActivationExecutor`, `InterfaceProfileActivationExecutorRequest`, `InterfaceProfileActivationExecutorResult`, `InterfaceProfileActivationExecutorStatus` und `InterfaceProfileActivationExecutorPrecondition`. Der Request kann inzwischen Zielprofil-ID/-Name, OperationMode, Preview-Kontext, erwartete Statuswerte, optionalen Fingerprint und Warnungsbestaetigungsdaten transportieren; das Result kann fehlendes frisches Laden, fehlende sichere UserDefined-Speicherung und erforderliche finale Re-Evaluation ausdruecken. Fuer den spaeteren Loader-/Store-Rand ist zusaetzlich `IInterfaceProfileActivationProfileStore` mit Load-/Save-Resultmodellen und einem nicht-produktiven `InterfaceProfileActivationProfileStoreStub` vorbereitet. Der Stub modelliert UserDefined/BuiltIn-Schutz, arbeitet nur in-memory, speichert nichts und fuehrt keine Datei-/Ordneroperation aus. Zusaetzlich gibt es mit `InterfaceProfileActivationExecutorStub` eine defensive Backend-Implementierung, die nur Preconditions bewertet und Statuswerte wie `ReadyButNotExecuted`, `Blocked`, `RequiresWarningConfirmation`, `NotImplemented` oder `NotAvailable` zurueckgibt. Sie ist nicht in UI-Flows registriert, setzt `IsActive` nicht, speichert nichts und startet keine Verarbeitung.

Ein produktiver Executor duerfte erst implementiert werden, wenn fachlich entschieden ist, ob `ReadyWithWarnings` nach Bestaetigung aktivierbar ist, wie Warnungsbestaetigungen gespeichert oder auditiert werden, ob `IsAttachmentProcessingEnabled` beim Aktivieren veraendert wird, welches Aktivierungsflag gespeichert wird, welche finalen Pruefungen direkt vor dem Speichern laufen, ob ein Audit-/Logeintrag erzeugt wird, welche Benutzerrolle aktivieren darf und welche UI den Prozess fuehrt.

Die offenen fachlichen Entscheidungen sind in `docs/AKTIVIERUNG_ENTSCHEIDUNGSNOTIZ.md` gebuendelt und dort inzwischen kompakter priorisiert. Die V1-Linie lautet knapp: UserDefined ja, BuiltIn nein, `Ready` ja, `ReadyWithWarnings` nur nach bewusster Bestaetigung, `Blocked`/`Unknown` nein, finale Re-Evaluation Pflicht, keine Sofortverarbeitung, `IsAttachmentProcessingEnabled` bleibt separat, Deaktivierung ist keine Loeschung/Dateioperation und laufende oder wartende Pakete duerfen nicht stillschweigend veraendert werden. `IsActive` ist der naheliegende Kandidat fuer das Aktivierungsflag, aber ein produktiver Executor braucht weiterhin eine echte Adapterimplementierung auf Basis von `ProfileCatalogService`/`AppDataPaths`, frisches Laden und sichere UserDefined-Speicherung.

Statische Pruefung und praktische Windows-Sichtpruefung des aktuellen Aktivierungsassistenten sind in `docs/UI_PRUEFPROTOKOLL_AKTIVIERUNGSASSISTENT.md` dokumentiert. Die WPF-Oberflaeche wurde in der Codex-Umgebung nicht praktisch bedient; XAML, Codebehind und Preview-Services wurden statisch darauf geprueft, dass der Dialog Vorschau bleibt und keine Aktivierung, Warnungsbestaetigung, Speicherung, Datei-/Ordneroperation oder Verarbeitung ausloest. Der scrollbare Dialog `Aktivierung vorbereiten` wurde durch den Benutzer auf Windows praktisch sichtgeprueft und fuer den aktuellen Vorschau-Status abgenommen.

Die zuletzt bestaetigte technische Absicherung dieses Standes:

- `dotnet build XdtDeviceBridge.sln` erfolgreich, `0` Warnungen, `0` Fehler
- `dotnet test XdtDeviceBridge.sln` erfolgreich, `940` Tests bestanden, `0` fehlgeschlagen, `0` uebersprungen

Offen bleiben die echte produktive Aktivierung und Deaktivierung, die finale technische Bestaetigung des Aktivierungsflags, die konkrete Repository-/Store-Methode mit frischem Laden, eine bewusste Warnungsbestaetigungs-UI, eine moegliche auditierbare Speicherung einer Warnungsbestaetigung, eine produktive `ActivationExecutor`-Implementierung beziehungsweise ein Deaktivierungspfad, konkrete Paketstatus-Erkennung, Umgang mit Altbestand/Fingerprint, Umgang mit laufenden/wartenden Paketen und die finale fachliche Freigabe der V1-Linie.

Empfohlener naechster Schritt: Noch nicht produktiv aktivieren oder deaktivieren. Als naechstes sollten der konkrete Feldname fuer das Aktivierungsflag, die produktive Store-Methode, das Audit-/Warnungsbestaetigungsmodell und die Paketregel fuer Aktivierung/Deaktivierung fachlich final freigegeben werden, insbesondere ob Deaktivierung bei laufenden oder wartenden Paketen blockiert. Eine produktive Executor-Implementierung bleibt danach ein separater, fachlich freizugebender Schritt.

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
| BuiltIn/UserDefined-Schutz | testseitig validiert | `ProfileCatalogServiceTests`, TemplateImport-Tests |

Teilweise praktisch abgeschlossen ist die manuelle Praxisabnahme fuer MEDISTAR + NIDEK ARK1S + XDT-Anhang-Link: Pflicht-Anhang vorhanden, Pflicht-Anhang fehlt/Fehlerfall und MEDISTAR-Livesystem-Linkaufruf sind bestanden. Noch offen ist die vollstaendige manuelle Praxisabnahme der weiteren Faelle aus `docs/END_TO_END_TESTPLAN.md`. Fuer die weitere Durchfuehrung steht die ausfuellbare Vorlage `docs/E2E_TESTPROTOKOLL_TEMPLATE.md` bereit.

## 4. Vorbereitet, aber noch nicht produktiv validiert

- NIDEK LM7/LM7P
- NIDEK NT530P
- TOPCON CL300
- TOPCON KR800
- TOPCON TRK2P
- vollstaendiger Profil-Assistent fuer unbekannte Geraete
- Geraete-Datei-Explorer
- weitere AIS-Systeme ausser MEDISTAR
- AIS-/MEDISTAR-Exporttemplate-Default-Konzept mit Feldkennungen je Untersuchungsart: bewusst zurueckgestellt, neues Fachkonzept erforderlich
- automatische MEDISTAR `6330`-Default-Zusatzzeilen: bewusst zurueckgestellt
- produktive Aktivierung importierter Schnittstellenprofile; die aktuelle Pruefung, Guard-Schicht, Warnungsbestaetigungsvorschau und ActivationPlan-Anzeige sind rein lesend vorbereitet
- bewusste Warnungsbestaetigung mit UI, moegliche dauerhafte Speicherung, finale Sicherheitspruefung und produktive `ActivationExecutor`-Implementierung
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
| Templatepaket-Import | Roadmap/Projektueberblick beschreiben Analyse, Plan, Dry-Run, UI-Vorschau, Benutzerwahl und UserDefined-Uebernahme. | Entsprechende Services, UI-Glue und E2E-nahe Tests vorhanden. | passt | ReplaceExisting und Aktivierungsassistent weiter als offen fuehren. |
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
| hoch | Templatepaket-Import Aktivierungsassistent | Sicherer Import als UserDefined vorhanden; Backend-Bewertung, UI-Pruefvorschau, vorbereitende Aktivierungsvorschau und Guard-Schicht fuer importierte Schnittstellenprofile pruefen Abhaengigkeiten, Ordner, XDT-Anhang-Konfiguration, Lizenzhinweise, BuiltIn/UserDefined-Schutz und Warnungsbestaetigung read-only. Der Dialog `Aktivierung vorbereiten` ist als scrollbares Preview-Fenster umgesetzt und zeigt Guard-Entscheidung, spaeter erforderliche Warnungsbestaetigung und ActivationPlan rein lesend an; ein internes Warning-Confirmation-Modell stellt bestaetigungspflichtige Warnungen ohne Speicherung bereit. Die praktische Sichtpruefung des Dialogs wurde fuer den Vorschau-Status abgenommen. Der `InterfaceProfileActivationPlanService` fasst diese Ergebnisse zu einem rein beschreibenden Aktivierungsplan mit PlannedSteps zusammen. Ein defensiver `InterfaceProfileActivationExecutorStub` bewertet Preconditions, fuehrt aber nichts aus. | Bewusste Benutzerfreigabe und eigentliche Aktivierung fehlen weiterhin. | Frisches Laden, sichere UserDefined-Speicherung und UI-Konzept fuer echte Warnungsbestaetigung separat spezifizieren; weiterhin keine automatische Aktivierung. | Importierte Profile bleiben zwar sicher, aber fuer Anwender noch nicht komfortabel produktiv nutzbar. | TemplateImport Executor, Profilkatalog, InterfaceProfileActivationEvaluationService, InterfaceProfileActivationGuardService |
| hoch | Lizenzsignatur | Lizenzanzeige und Karenzzeitmodell vorhanden. | Digitale Signaturpruefung, Schluesselmodell, Manipulationsschutz. | Signaturformat und Validierungsservice spezifizieren. | Lizenzdateien sind vor produktiver Sperre nicht ausreichend gesichert. | Lizenzmodell, Supportprozess |
| mittel | ReplaceExisting fuer UserDefined | Bewusst deaktiviert; Executor blockiert/ueberspringt. | Konfliktloesungsdialog, Sicherung/Backup, explizite Benutzerentscheidung. | Erst nach Aktivierungsassistent als separates Import-Epic planen. | Anwender muessen Konflikte ueber Kopien loesen, Katalog kann wachsen. | Importvorschau, SelectionService |
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

### Phase 3: Template-/Profil-Baukasten

- Read-only Aktivierungsassistent ist praktisch in der UI sichtgeprueft und fuer den Vorschau-Status abgenommen; Pruefung, Guard, Warnungsbestaetigungsvorschau und ActivationPlan bleiben ohne produktive Wirkung.
- V1-Spezifikation aus `docs/AKTIVIERUNG_ENTSCHEIDUNGSNOTIZ.md` fachlich final abnehmen: Aktivierungsflag, UserDefined-Persistenz mit frischem Laden, Aenderungsschutz, Warnungsbestaetigung/Audit, Deaktivierungslinie, Paketregel fuer laufende/wartende Pakete und finale Re-Evaluation.
- UI-Konzept fuer spaetere bewusste Warnungsbestaetigung ohne sofortige Speicherung entwerfen.
- Produktive `ActivationExecutor`-Implementierung und Deaktivierungspfad erst nach Fachentscheidung zu Preconditions, Audit, Rollen, Speicherung, Warnungsbestaetigung, Deaktivierungsregeln, Paketstatus-Erkennung, frischer Profilkatalog-Ladung und konkreter Store-Methode planen; Entscheidungsgrundlage: `docs/AKTIVIERUNG_ENTSCHEIDUNGSNOTIZ.md`.
- `docs/UI_PRUEFPROTOKOLL_AKTIVIERUNGSASSISTENT.md` als Regressionscheck fuer spaetere UI-Aenderungen weiterfuehren.
- Finale Sicherheitspruefung, Audit-/Logeintrag und erneuten Build-/Testlauf direkt vor einer spaeteren echten Aktivierung einplanen.
- Optional spaeter ReplaceExisting fuer UserDefined mit Backup/Bestaetigung.
- AIS-/MEDISTAR-Default-Exporttemplates nicht umsetzen, bis ein neues Fachkonzept vorliegt.

### Phase 4: Geraete-Datei-Explorer / Profil-Assistent

- Read-only Explorer fuer Geraetedateien und SourcePaths.
- Uebernahme erkannter Messwerte in Exportregel-Entwurf.
- Gefuehrter Profil-Assistent fuer neue Geraete.

### Phase 5: Weitere Geraete validieren

- LM7/LM7P
- NT530P
- TOPCON CL300
- TOPCON KR800
- TOPCON TRK2P

### Phase 6: Lizenzierung haerten

- Digitale Signaturpruefung.
- Manipulationsschutz.
- Klare Sperrregeln und Karenzlogik.
- Optional spaeter Online-Lizenzierung.

### Phase 7: Installer / Deployment

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

**Restliche E2E-Testfaelle praktisch ausfuehren und protokollieren.**

Konkreter Umfang:

- keine Produktivlogik aendern
- keine Profile oder BuiltIns aendern
- ein synthetisches Testdatenpaket bzw. konkrete synthetische Dateien fuer die noch offenen Varianten bereitstellen
- die noch nicht protokollierten Faelle aus `docs/END_TO_END_TESTPLAN.md` praktisch Schritt fuer Schritt abarbeiten
- Ergebnisse in `docs/E2E_TESTPROTOKOLL_TEMPLATE.md` dokumentieren
- danach Build und Tests ausfuehren

Das ist der beste naechste Schritt, weil die Kernlogik bereits umfangreich testseitig abgesichert ist, aber die praktische Abnahme der automatischen Paket- und Anhanglogik noch als groesste Restunsicherheit bleibt.

## 10. Keine Aenderungen an Code

Dieser Abgleich ist eine reine Dokumentationsdatei. Es wurden keine Produktivlogik, UI, Tests, Profile, BuiltIn-Profile oder Exportprofile geaendert.
