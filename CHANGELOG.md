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
- UI-Ueberlagerung im Tab `Schnittstellenprofile` unterhalb der Ordnerbereinigung behoben.
- Tests fuer die Aktivierungsbewertung importierter Schnittstellenprofile ergaenzt, inklusive fehlender Abhaengigkeiten, fehlender Pflichtordner, BuiltIn-Schutz, optional deaktivierter XDT-Anhang-Automatik und lizenzpflichtiger Profile.

### Behoben

- Templatepaket-Importvorschau in der WPF-App stabilisiert: Paketlesen und Vorschau-Erstellung laufen nun ueber einen UI-nah testbaren Preview-Service, der Import-Button ist waehrend der Vorschau gesperrt, und unveraenderte ComboBox-Auswahlereignisse bauen die Vorschau nicht erneut rekursiv auf.
- Templatepaket-Importvorschau benutzerfreundlicher gemacht: Konflikte und BuiltIn-Schutz starten nun mit `Ueberspringen` als sicherem Standard, `Als Kopie importieren` muss bewusst gewaehlt werden, Zielnamen fuer Kopien sind editierbar und leere/doppelte Zielnamen blockieren die Uebernahme verstaendlich.
- Abhaengigkeitsauflösung und Importzusammenfassung im Templatepaket-Import klarer formuliert, inklusive Hinweis auf Zielbereiche fuer importierte Profile und weiterhin fehlende automatische Aktivierung.
- Templatepaket-Importvorschau weiter verstaendlicht: leere Abhaengigkeitsauflösung zeigt nun einen deutschen Leerzustand, sichtbare Hinweise/Warnungen sind deutsch lokalisiert und doppelte Hinweise werden reduziert.

### Dokumentation

- Geraete-/Template-Matrix ergaenzt: BuiltIn-Geraeteprofile, MEDISTAR-Exportprofile, vorhandene Testdaten, Templatepaket-Luecken und V1-Prioritaeten fuer konkrete Geraete-/Templatearbeit sind kompakt dokumentiert.
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
