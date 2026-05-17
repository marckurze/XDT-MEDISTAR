# Changelog

## Unreleased

### Hinzugefuegt

- Erste read-only Bewertungslogik fuer einen spaeteren Aktivierungsassistenten importierter Schnittstellenprofile ergaenzt: Abhaengigkeiten, Ordner, XDT-Anhang-Konfiguration und Lizenzhinweise werden bewertet, ohne Profile zu aendern oder Verarbeitung zu starten.
- Erste UI-Pruefansicht fuer die Aktivierungsbewertung im Tab `Schnittstellenprofile` ergaenzt; sie zeigt Gesamtstatus, Aktivierbarkeit, Blocker, Warnungen und Hinweise, fuehrt aber weiterhin keine Aktivierung aus.
- Aktivierungspruefung um strukturierte Detailanzeigen fuer Ordnerpruefung und XDT-Anhang-Konfiguration erweitert; die Anzeige bleibt reine Vorschau ohne Datei-/Ordneroperationen.
- Vorbereitende Aktivierungsvorschau `Aktivierung vorbereiten` ergaenzt; sie fasst Status, Aktivierbarkeit, wichtigste Blocker und Warnungen zusammen und speichert weiterhin keine Aenderungen.
- Technische Guard-/Schutzschicht fuer eine schlanke spaetere V1-Aktivierung vorbereitet; sie prueft aktuelle Bewertung, BuiltIn/UserDefined-Schutz und blockiert `ReadyWithWarnings` fuer V1.
- Dialog `Aktivierung vorbereiten` von langer MessageBox auf ein scrollbares, resizebares Preview-Fenster umgestellt und auf V1 reduziert: Bewertung, technische Freigabe, Blocker, Warnungen, Hinweise und Sicherheitshinweis.
- Interface-/Model-Skelett fuer einen spaeteren `ActivationExecutor` ergaenzt; es beschreibt Request, Result, Preconditions und Statuswerte, bleibt aber ohne produktive Implementierung, UI-Anbindung, Speicherung oder Aktivierungswirkung.
- Defensiven `InterfaceProfileActivationExecutorStub` ergaenzt; er bewertet V1-Preconditions und liefert Statuswerte wie `ReadyButNotExecuted`, `Blocked` oder `NotImplemented`, fuehrt aber keine Aktivierung, Speicherung, Datei-/Ordneroperation oder Verarbeitung aus.
- `ActivationExecutor`-Request und -Result fuer eine spaetere Backend-Aktivierung auf V1-Kontext gestrafft: Zielprofil-ID/-Name, OperationMode, Preview-Kontext sowie Result-Flags fuer frisches Laden, sichere UserDefined-Speicherung und finale Bewertung.
- Executor-nahe Store-/Loader-Abstraktion fuer spaeteres frisches Laden und sichere UserDefined-Speicherung vorbereitet; der aktuelle Store-Stub arbeitet nur in-memory, blockiert BuiltIn/Nicht-UserDefined und speichert weiterhin nichts.
- `InterfaceProfileActivationProfileCatalogStore` als ValidateOnly-Adapter gegen `ProfileCatalogService`/`AppDataPaths` ergaenzt; frisches Laden ist moeglich, Save bleibt DryRun ohne `SaveInterfaceProfileDefinition`, ohne JSON-Schreibzugriff und ohne Profilmutation.
- `InterfaceProfileActivationExecutorStub` kann im `ValidateOnly`-Modus optional einen `IInterfaceProfileActivationProfileStore` nutzen; frisches Laden, BuiltIn/UserDefined-Pruefung und Save-DryRun werden ausgewiesen, bleiben aber ohne Aktivierung, ohne Speicherung und ohne UI-Anbindung.
- `InterfaceProfileActivationExecutorStub` bereitet finale Bewertung als nicht-produktive `ValidateOnly`-Simulation vor: Evaluation und Guard koennen optional neu erzeugt werden; fehlen Services, werden MissingCapabilities gemeldet, und Save bleibt DryRun ohne Profilmutation.
- Reproduzierbaren Testweg fuer das MEDISTAR + NIDEK ARK1S Referenz-Templatepaket ergaenzt: `TemplatePackageExporter` erzeugt die ZIP temporaer, `TemplatePackageImporter` liest sie wieder ein, und der sichere Importfluss schuetzt BuiltIns und importiert nur inaktive UserDefined-Kopien.
- Selektiven Templatepaket-Export ergaenzt: Im Tab `Profile & Templates` wird ein Schnittstellenprofil als Paketbasis gewaehlt; exportiert werden nur dieses Schnittstellenprofil und die benoetigten AIS-, Geraete- und Exportprofile.
- Schlanke Bedienfunktionen fuer Exportprofil-Wartung ergaenzt: UserDefined-Exportprofile koennen geloescht werden, wenn kein Schnittstellenprofil sie verwendet; Exportregeln koennen nur aus UserDefined-Exportprofilen entfernt werden. BuiltIns bleiben gesperrt.
- NIDEK AR360 / AR-360A als testgestuetzten Auto-Refraktometer-Kandidaten ergaenzt: BuiltIn-Geraeteprofil, MEDISTAR-Exportprofil, Schnittstellenprofil, ARMedian-XML-Fixtures, UTF-16-Lesetest und selektiver Templatepaket-Test.
- Praktische MEDISTAR-Validierung fuer NIDEK AR360 dokumentiert und testseitig abgesichert: `8402 = AR360`, rechte/linke `6228`-Auto-Refraktor-Zeilen, ARMedian-Achse `172` und `VD= 12.00 mm`.
- AR360-Referenzpakettest analog zu ARK1S ergaenzt: selektiver Export, ZIP-Struktur, Importvorschau, DryRun, sichere UserDefined-Kopie, inaktive Schnittstellenprofile und deaktiviertes `IsAttachmentProcessingEnabled` werden reproduzierbar geprueft.
- Erste Grundlage fuer abdockbare Geraeteanbindungsfenster im Tab `Verarbeitung` ergaenzt: Monitoring-Karten koennen manuell abgedockt und wieder angedockt werden; Pin und Positionsmerken sind pro Schnittstellenprofil als UI-Zustand vorbereitet.
- Erste Stufe fuer automatisches Abdocken bei Monitoring-Aktivitaet ergaenzt: AIS-/Geraete-/Anhang-/Paket- und Verarbeitungsevents oeffnen nur das betroffene Geraetefenster und nutzen einen kurzen Cooldown gegen Event-Spam.
- Akustisches Signal bei Geraetedatei-Eingang ergaenzt: `04_praxis_terminal_signal.wav` wird als App-Content ausgeliefert, pro Schnittstellenprofil per Cooldown begrenzt und bei fehlender/defekter Sounddatei defensiv abgefangen.
- Automatisches Zurueckandocken nach terminalem Monitoring-Abschluss ergaenzt: automatisch geoeffnete, nicht gepinnte Geraetefenster docken nach 5 Sekunden Restlaufzeit wieder an; neue Aktivitaet, Pin oder manuelles Andocken brechen den Countdown pro Schnittstellenprofil ab.
- Systray-Grundfunktion ergaenzt: Das Hauptfenster kann per Minimieren oder `X` in den Infobereich ausgeblendet werden, waehrend Verarbeitung und Floating-Geraetefenster weiterlaufen; Doppelklick beziehungsweise Kontextmenue `Oeffnen` stellt es wieder her, `Beenden` schliesst die App bewusst.
- Sicherer Reset fuer Geraeteanbindungsfenster angepasst: `↺` verwirft den aktuellen Vorgang fuer genau ein Schnittstellenprofil und leert nach Sicherheitsabfrage nur dessen AIS-, Geraete- und optionalen XDT-Anhang-Eingangsordner top-level; Export-, Archiv- und Fehlerordner sowie Unterordner bleiben unangetastet.
- Floating-Geraeteanbindungsfenster leicht verbreitert und die Symbolleiste oben rechts fix horizontal gesetzt, damit `🗗 ↺ 📌 🔝` nicht umbrechen.
- UI-Ueberlagerung im Tab `Schnittstellenprofile` unterhalb der Ordnerbereinigung behoben.
- Tests fuer die Aktivierungsbewertung importierter Schnittstellenprofile ergaenzt, inklusive fehlender Abhaengigkeiten, fehlender Pflichtordner, BuiltIn-Schutz, optional deaktivierter XDT-Anhang-Automatik und lizenzpflichtiger Profile.

### Behoben

- Startfehler bei persistent abgedockten Geraeteanbindungsfenstern behoben: Floating-Fenster werden erst nach der sicheren MainWindow-Anzeige wiederhergestellt, `Owner` wird defensiv gesetzt, und ein Restore-Fehler dockt die Karte statt App-Abbruch sicher an.
- Floating-Geraeteanbindungsfenster stabilisiert: Schliessen per `X` dockt sicher zurueck, ohne rekursives `Close()`, Radar-/Scanbalken ist im Floating-Fenster sichtbar, und die Scanintervall-Buttons `-`/`+` sind dort ebenfalls verfuegbar.
- Textbuttons fuer Floating-Geraeteanbindungsfenster durch kompakte Symbolbuttons ersetzt: `🗗` fuer Abdocken/Andocken, `📌` fuer Position merken und `🔝` fuer Immer im Vordergrund; Funktion und State bleiben unveraendert.
- Symbol- und Scanintervall-Buttons in Geraeteanbindungsfenstern optisch beruhigt: transparente Button-Templates ersetzen die graue Standard-WPF-Fläche; Hover, Pressed und aktive Toggle-Zustaende bleiben sichtbar.
- Pin/TopMost fuer Floating-Geraeteanbindungsfenster getrennt: Floating-Fenster verwenden keinen gemeinsamen WPF-Owner mehr, und `ApplyState` nimmt nur den State der eigenen Schnittstellenprofil-ID an. AR360 und ARK1S koennen unabhaengig gepinnt werden.
- Soundausloesung fuer Geraeteanbindungsfenster korrigiert: Der Signalton haengt nicht mehr an deduplizierten Monitoring-Textmeldungen, wird nur noch bei stabil erkannter Geraetedatei gespielt und funktioniert bei spaeteren Durchlaeufen erneut.
- Templatepaket-Importvorschau in der WPF-App stabilisiert: Paketlesen und Vorschau-Erstellung laufen nun ueber einen UI-nah testbaren Preview-Service, der Import-Button ist waehrend der Vorschau gesperrt, und unveraenderte ComboBox-Auswahlereignisse bauen die Vorschau nicht erneut rekursiv auf.
- Templatepaket-Importvorschau benutzerfreundlicher gemacht: Konflikte und BuiltIn-Schutz starten nun mit `Ueberspringen` als sicherem Standard, `Als Kopie importieren` muss bewusst gewaehlt werden, Zielnamen fuer Kopien sind editierbar und leere/doppelte Zielnamen blockieren die Uebernahme verstaendlich.
- Abhaengigkeitsauflösung und Importzusammenfassung im Templatepaket-Import klarer formuliert, inklusive Hinweis auf Zielbereiche fuer importierte Profile und weiterhin fehlende automatische Aktivierung.
- Templatepaket-Importvorschau weiter verstaendlicht: leere Abhaengigkeitsauflösung zeigt nun einen deutschen Leerzustand, sichtbare Hinweise/Warnungen sind deutsch lokalisiert und doppelte Hinweise werden reduziert.
- Button-Anordnung im Bereich `Exportregeln` verstaendlicher gruppiert: Regel-Aktionen stehen bei der Regelliste, Exportprofil-Speichern/-Loeschen beim Profilentwurf; deaktivierte Loeschbuttons erklaeren BuiltIn- oder Referenzschutz per Tooltip.

### Nicht umgesetzt

- Noch nicht enthalten sind Autostart, Windows-Dienst, UI-Einstellung fuer die Rueckdock-Zeit und ein sichtbarer Countdown-Hinweis fuer abdockbare Geraeteanbindungsfenster.

### Dokumentation

- Geraete-/Template-Matrix ergaenzt: BuiltIn-Geraeteprofile, MEDISTAR-Exportprofile, vorhandene Testdaten, Templatepaket-Luecken und V1-Prioritaeten fuer konkrete Geraete-/Templatearbeit sind kompakt dokumentiert.
- Templatepaket-Kandidat `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_AR360.md` dokumentiert; ARMedian ist fuer AR360 massgeblich, die abweichende Achse aus der bereitgestellten MEDISTAR-TXT wird nicht kuenstlich uebernommen.
- E2E-Protokoll `docs/E2E_TESTPROTOKOLL_MEDISTAR_AR360.md` ergaenzt; die dokumentierte Ausgabe ist anonymisiert und enthaelt keine Kunden-/Patientendaten oder Live-Pfade.
- ARK1S und AR360 als stabile Referenzpakete in Matrix und Paketdokumenten markiert; offizielle ZIP-Artefakte werden erst nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md` dauerhaft abgelegt.
- Kleine Release-Regel fuer offizielle Templatepaket-ZIPs dokumentiert, inklusive selektivem Exporttest, Import-/DryRun-Test, Praxis-App-Importpruefung, BuiltIn-Schutz und Daten-/Pfadpruefung.
- Praxisabnahme `docs/PRAXISABNAHME_GERAETEFENSTER_V1.md` ergänzt: Systray, manuelles und automatisches Abdocken, Auto-Zurueckandocken, Pin/Position, Signalton, Reset und Sicherheitsgrenzen der Geraeteanbindungsfenster sind als V1-Funktionsblock abgenommen.
- Projektleitlinie geschaerft: fertige Geraeteprofile und Templatepakete haben Vorrang vor Baukasten-Nutzung; der Aktivierungsassistent ruht bis auf Weiteres als read-only vorbereiteter Stand.
- Offizielle Paketvorlage `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_ARK1S.md` fuer das erste Referenzpaket MEDISTAR + NIDEK ARK1S ergaenzt; sie nutzt das bestehende ZIP-Templatepaket-Format, enthaelt keine Kunden-/Patientendaten und keine Live-Pfade.
- ARK1S-Paketvorlage dokumentiert jetzt den automatisierten Export-/Import-Testweg; die ZIP wird weiterhin nicht manuell eingecheckt und bleibt bis zur Release-Regel ein temporaeres Testartefakt.
- Templatepaket-Dokumentation ergaenzt: MEDISTAR + NIDEK ARK1S wird ueber den selektiven Export aus dem vorhandenen Schnittstellenprofil erzeugt; Vollkatalog-Exporte sind nicht mehr der Standardweg.
- `docs/ROADMAP.md` ergänzt als aktualisierte Roadmap für den Stand nach XDT-Anhang-Ausbau, Baukasten-Testexport, Paket-Wartelogik und UI-Refactoring.
- Aktuellen Entwicklungsstand zu XDT-Anhängen für AIS, Test & Vorschau, Testexport, Dateistabilität, konfigurierbarem Scan-Intervall und zweistufiger Paketlogik dokumentiert.
- Templatepaket-Import um aktuellen Stand ergänzt: Konfliktanalyse, Importplan, Dry-Run, UI-Vorschau, sichere Benutzerentscheidungen und explizite UserDefined-Übernahme.
- BuiltIn-Schutz, deaktiviertes `ReplaceExisting`, inaktive importierte Schnittstellenprofile und deaktiviertes `IsAttachmentProcessingEnabled` bei importierten Schnittstellenprofilen dokumentiert.
- E2E-nahe Tests für den sicheren Templatepaket-Importfluss dokumentiert.
- Praxisvalidierung MEDISTAR + NIDEK ARK1S + XDT-Anhang-Link dokumentiert.
- Pflicht-XDT-Anhang-Testfälle erfolgreich geprüft: Anhang vorhanden und Anhang fehlt/Fehlerfall.
- Projektstandsicherung zum aktuellen Aktivierungsassistenten ergänzt: Service-Kette, Statuslogik, Sicherheitsgrenzen, UI-Stand, letzter Build-/Teststand und offene Schritte dokumentiert.
- Entscheidungsnotiz `docs/AKTIVIERUNG_ENTSCHEIDUNGSNOTIZ.md` ergänzt; sie strukturiert die offenen fachlichen Entscheidungen vor einer späteren produktiven Schnittstellenprofil-Aktivierung.
- Entscheidungsnotiz radikal auf schlanke V1 gestrafft: spaetere Aktivierung nur fuer frisch geladene UserDefined-Profile mit Status `Ready`, ohne Blocker und ohne Warnungen; `ReadyWithWarnings`, Warnungsbestaetigung, Audit, Deaktivierung, Fingerprint und Paketsonderlogik sind nicht V1.
- Statisches UI-Pruefprotokoll `docs/UI_PRUEFPROTOKOLL_AKTIVIERUNGSASSISTENT.md` ergänzt; die laufende WPF-UI wurde in der Codex-Umgebung nicht praktisch bedient, die Preview-/Dialogpfade wurden statisch auf reine Vorschau und fehlende Aktivierungswirkung geprüft.
- Praktische Windows-Sichtpruefung des Dialogs `Aktivierung vorbereiten` im UI-Pruefprotokoll dokumentiert; scrollbares Preview-Fenster, gestraffte Texte und weiterhin fehlende produktive Wirkung sind fuer den Vorschau-Status abgenommen.
- Klarstellung: Die Änderungen beschreiben den aktuellen Dokumentations- und Vorbereitungsstand; Produktivlogik, BuiltIn-Profile und Exportprofile wurden dadurch nicht geändert.

## 0.1.0-prototype

Erster stabiler Prototyp der XdtDeviceBridge.

### Enthalten

- WPF-Desktop-App
- MEDISTAR-GDT/XDT-Eingang
- NIDEK ARK1S XML-Eingang
- MEDISTAR-kompatibler XDT-Export
- `8000=6310` Steuerfeld
- `8402` Untersuchungsart aus AIS
- `6228` Ergebniszeilen
- Formatfunktionen fuer ophthalmologische Messwerte
- manuelle Verarbeitung
- Schnittstellenprofile
- Profil-/Template-Grundmodell
- Template-Export und Template-Importpruefung
- Exportregel-Baukasten im Entwurfsmodus
- manuell startbare Ueberwachung
- optionale automatische Verarbeitung
- Archivierung per Kopie oder Verschieben
- sichere Duplikatbehandlung
- Fehlerablage mit `error.txt`
- Offline-Lizenz-Prototyp
- lizenzierte Geraete-/Anbindungsanzeige
- Karenzzeitmodell fuer neue Anbindungen
- Testsuite mit aktuellem Stand

### Nicht enthalten / bewusst noch offen

- Windows-Dienst
- Autostart
- echter FileSystemWatcher
- produktive Lizenzsperre
- digitale Signaturpruefung fuer Lizenzdateien
- Online-Lizenzierung
- vollstaendiger Profil-Assistent fuer unbekannte Geraete
- Geräte-Dateianhang-Import und MEDISTAR externer Link über XDT
- Dokument-/Dateianhang-Template
- selbst erzeugte PDF-Protokolle
- produktive Archivloeschung im Hintergrund
- SQLite-Speicherung
- produktive Templatepaket-Uebernahme mit Konfliktloesung
- Installer / Deployment

### Dokumentationsstand nach 0.1.0-prototype

- Geräte-Dateianhänge und externe AIS-Link-Übergabe sind als verbindliche Zielanforderung dokumentiert.
- Für MEDISTAR ist die Link-Übergabe über XDT-Felder `6302`, `6303`, `6304` und `6305` beschrieben.
- Die Beispieldatei `XDT Übergabe externer Link.txt` wurde fachlich ausgewertet; die MEDISTAR-interne `EV:{...}`-Anzeige wird klar von der XDT-Feldübergabe getrennt.
- Keine Implementierung dieser Funktion in `0.1.0-prototype`.
