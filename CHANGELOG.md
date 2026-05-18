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
- Schlanke V1-Profilanlage im Tab `Profile & Templates` ergaenzt: `Neues AIS anlegen` und `Neues Geraet anlegen` oeffnen einfache Dialoge und speichern nur UserDefined-Profile; `Neues Exportprofil anlegen` bereitet den vorhandenen Exportregel-Entwurf als UserDefined-Kopie vor. BuiltIns bleiben geschuetzt, es erfolgt keine Aktivierung und keine Verarbeitung.
- Dialoge fuer `Neues AIS anlegen` und `Neues Geraet anlegen` verstaendlicher gemacht: kurze Hilfetexte erklaeren Profilname, AIS-System, Codierung, Hersteller, Modell, Geraetetyp und Parserbasis; `Generisch` wird als anderer AIS-Kontext beziehungsweise interner Fallback erklaert.
- Sichere Umbenennung fuer UserDefined-Profile ergaenzt: AIS-, Geraete-, Export- und Schnittstellenprofile koennen ihren sichtbaren Namen aendern, waehrend technische IDs, Referenzen, Ordner, Exportregeln und XDT-Anhang-Einstellungen unveraendert bleiben; BuiltIns bleiben gesperrt.
- NIDEK LM7 / LM-7P als praktisch nutzbaren Lensmeter-Kandidaten ergaenzt: echte `NIDEK LM7.xml`-Fixture, `Sphare`/`Sphere`-Alias, berechnete MEDISTAR-`6228`-Lensmeterzeilen, BuiltIn-Schnittstellenprofil und selektiver LM7-Templatepaket-Test.
- `NIDEK_LM_Stylesheet.xsl` wird als Anzeige-Stylesheet ignoriert und nicht als Geraetedatei klassifiziert.
- Praktische MEDISTAR-Validierung fuer NIDEK LM7 dokumentiert: AIS- und echte LM7-XML-Datei wurden verarbeitet, MEDISTAR uebernimmt `8402` aus AIS sowie die rechten/linken `6228`-Lensmeterzeilen ohne leere Prisma-/PD-/ADD-Fragmente.
- NIDEK NT530P / NT-530P als direkt nutzbaren MEDISTAR-Kandidaten ergaenzt: echte UTF-16-XML-Fixture, Parserwerte fuer Tonometrie, korrigierten IOP und Pachymetrie, berechnete MEDISTAR-Zeilen fuer `6220` und `6205`, BuiltIn-Schnittstellenprofil und selektiver NT530P-Templatepaket-Test. JPG-Dateien werden nicht als Messwertdateien verarbeitet und bleiben fuer spaetere XDT-Anhang-Validierung relevant.
- XDT-Mehrfachanhaenge im Automatiklauf ergaenzt: mehrere stabile unterstuetzte Anhangdateien werden sortiert einzeln per bestehender Copy/Move- und Kollisionslogik uebertragen und erzeugen je Datei eine eigene `6302`/`6303`/optional `6304`/`6305`-Linkfeldgruppe.
- Tests fuer die Aktivierungsbewertung importierter Schnittstellenprofile ergaenzt, inklusive fehlender Abhaengigkeiten, fehlender Pflichtordner, BuiltIn-Schutz, optional deaktivierter XDT-Anhang-Automatik und lizenzpflichtiger Profile.

### Behoben

- Startfehler bei persistent abgedockten Geraeteanbindungsfenstern behoben: Floating-Fenster werden erst nach der sicheren MainWindow-Anzeige wiederhergestellt, `Owner` wird defensiv gesetzt, und ein Restore-Fehler dockt die Karte statt App-Abbruch sicher an.
- Floating-Geraeteanbindungsfenster stabilisiert: Schliessen per `X` dockt sicher zurueck, ohne rekursives `Close()`, Radar-/Scanbalken ist im Floating-Fenster sichtbar, und die Scanintervall-Buttons `-`/`+` sind dort ebenfalls verfuegbar.
- Textbuttons fuer Floating-Geraeteanbindungsfenster durch kompakte Symbolbuttons ersetzt: `🗗` fuer Abdocken/Andocken, `📌` fuer Position merken und `🔝` fuer Immer im Vordergrund; Funktion und State bleiben unveraendert.
- Symbol- und Scanintervall-Buttons in Geraeteanbindungsfenstern optisch beruhigt: transparente Button-Templates ersetzen die graue Standard-WPF-Fläche; Hover, Pressed und aktive Toggle-Zustaende bleiben sichtbar.
- Pin/TopMost fuer Floating-Geraeteanbindungsfenster getrennt: Floating-Fenster verwenden keinen gemeinsamen WPF-Owner mehr, und `ApplyState` nimmt nur den State der eigenen Schnittstellenprofil-ID an. AR360 und ARK1S koennen unabhaengig gepinnt werden.
- Soundausloesung fuer Geraeteanbindungsfenster korrigiert: Der Signalton haengt nicht mehr an deduplizierten Monitoring-Textmeldungen, wird nur noch bei stabil erkannter Geraetedatei gespielt und funktioniert bei spaeteren Durchlaeufen erneut.
- NT530P-MEDISTAR-Ausgabe korrigiert: `6205` beginnt mit `Tonometrie`, `6220` mit `Pachymetrie`, die Tonometrie wird in mehrere Feldzeilen aufgeteilt, und der alte Zusatz `/ EV:{000000003B} NT-530P Messung` entfaellt.
- LM7-Live-Export korrigiert: alte persistierte BuiltIn-Exportprofile mit `Device.R/LM/Median/...`-Pfaden werden beim Katalogstart gezielt auf die Parser-Pfade `Device.Measure[@Type='LM']/LM/R/MedistarLine` und `Device.Measure[@Type='LM']/LM/L/MedistarLine` repariert; leere Prisma-/PD-/ADD-Fragmente erscheinen dadurch nicht mehr im LM7-Export.
- Templatepaket-Importvorschau in der WPF-App stabilisiert: Paketlesen und Vorschau-Erstellung laufen nun ueber einen UI-nah testbaren Preview-Service, der Import-Button ist waehrend der Vorschau gesperrt, und unveraenderte ComboBox-Auswahlereignisse bauen die Vorschau nicht erneut rekursiv auf.
- Templatepaket-Importvorschau benutzerfreundlicher gemacht: Konflikte und BuiltIn-Schutz starten nun mit `Ueberspringen` als sicherem Standard, `Als Kopie importieren` muss bewusst gewaehlt werden, Zielnamen fuer Kopien sind editierbar und leere/doppelte Zielnamen blockieren die Uebernahme verstaendlich.
- Abhaengigkeitsauflösung und Importzusammenfassung im Templatepaket-Import klarer formuliert, inklusive Hinweis auf Zielbereiche fuer importierte Profile und weiterhin fehlende automatische Aktivierung.
- Templatepaket-Importvorschau weiter verstaendlicht: leere Abhaengigkeitsauflösung zeigt nun einen deutschen Leerzustand, sichtbare Hinweise/Warnungen sind deutsch lokalisiert und doppelte Hinweise werden reduziert.
- Button-Anordnung im Bereich `Exportregeln` verstaendlicher gruppiert: Regel-Aktionen stehen bei der Regelliste, Exportprofil-Speichern/-Loeschen beim Profilentwurf; deaktivierte Loeschbuttons erklaeren BuiltIn- oder Referenzschutz per Tooltip.
- `Neues Exportprofil anlegen` im Tab `Profile & Templates` sichtbar repariert: Der Button startet nun einen leeren Exportprofil-Entwurf, setzt einen eindeutigen Profilnamen, zeigt eine Status-/Logmeldung und speichert weiterhin erst nach bewusstem Klick auf `Entwurf als neues Exportprofil speichern`.

### Nicht umgesetzt

- Noch nicht enthalten sind Autostart, Windows-Dienst, UI-Einstellung fuer die Rueckdock-Zeit und ein sichtbarer Countdown-Hinweis fuer abdockbare Geraeteanbindungsfenster.

### Dokumentation

- Geraete-/Template-Matrix ergaenzt: BuiltIn-Geraeteprofile, MEDISTAR-Exportprofile, vorhandene Testdaten, Templatepaket-Luecken und V1-Prioritaeten fuer konkrete Geraete-/Templatearbeit sind kompakt dokumentiert.
- Templatepaket-Kandidat `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_LM7.md` dokumentiert; Lensmeter-XDT-Rueckgabe ist praktisch validiert, Prisma-/PD-Faelle und offizielles ZIP-Artefakt bleiben offen.
- Templatepaket-Kandidat `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_AR360.md` dokumentiert; ARMedian ist fuer AR360 massgeblich, die abweichende Achse aus der bereitgestellten MEDISTAR-TXT wird nicht kuenstlich uebernommen.
- E2E-Protokoll `docs/E2E_TESTPROTOKOLL_MEDISTAR_AR360.md` ergaenzt; die dokumentierte Ausgabe ist anonymisiert und enthaelt keine Kunden-/Patientendaten oder Live-Pfade.
- E2E-Protokoll `docs/E2E_TESTPROTOKOLL_MEDISTAR_LM7.md` ergaenzt; die LM7-Lensmeter-Rueckgabe ist anonymisiert dokumentiert und enthaelt keine Kunden-/Patientendaten oder Live-Pfade.
- ARK1S und AR360 als stabile Referenzpakete in Matrix und Paketdokumenten markiert; offizielle ZIP-Artefakte werden erst nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md` dauerhaft abgelegt.
- Kleine Release-Regel fuer offizielle Templatepaket-ZIPs dokumentiert, inklusive selektivem Exporttest, Import-/DryRun-Test, Praxis-App-Importpruefung, BuiltIn-Schutz und Daten-/Pfadpruefung.
- Profilumbenennung dokumentiert: Templatepaket-ZIP-Dateien werden weiterhin ueber Dateinamen/Release-Regel versioniert; fuer Importvorschau-Zielnamen bleibt die bestehende Vorschau-Bearbeitung massgeblich, es wurde keine separate Templatepaket-Verwaltung eingefuehrt.
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
