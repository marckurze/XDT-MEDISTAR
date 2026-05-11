# Changelog

## Unreleased

### Hinzugefuegt

- Erste read-only Bewertungslogik fuer einen spaeteren Aktivierungsassistenten importierter Schnittstellenprofile ergaenzt: Abhaengigkeiten, Ordner, XDT-Anhang-Konfiguration und Lizenzhinweise werden bewertet, ohne Profile zu aendern oder Verarbeitung zu starten.
- Erste UI-Pruefansicht fuer die Aktivierungsbewertung im Tab `Schnittstellenprofile` ergaenzt; sie zeigt Gesamtstatus, Aktivierbarkeit, Blocker, Warnungen und Hinweise, fuehrt aber weiterhin keine Aktivierung aus.
- Aktivierungspruefung um strukturierte Detailanzeigen fuer Ordnerpruefung und XDT-Anhang-Konfiguration erweitert; die Anzeige bleibt reine Vorschau ohne Datei-/Ordneroperationen.
- Vorbereitende Aktivierungsvorschau `Aktivierung vorbereiten` ergaenzt; sie fasst Status, Aktivierbarkeit, wichtigste Blocker und Warnungen zusammen und speichert weiterhin keine Aenderungen.
- Technische Guard-/Schutzschicht fuer spaetere Schnittstellenprofil-Aktivierungen vorbereitet; sie prueft aktuelle Bewertung, BuiltIn/UserDefined-Schutz und Warnungsbestaetigung, fuehrt aber keine Aktivierung aus.
- Dialog `Aktivierung vorbereiten` zeigt die Guard-Entscheidung jetzt rein lesend an; `ReadyWithWarnings` weist auf die spaeter erforderliche bewusste Warnungsbestaetigung hin, ohne diese zu speichern oder eine Aktivierung anzubieten.
- Read-only Modell und Service fuer eine spaetere bewusste Warnungsbestaetigung vorbereitet; Warnungen werden als bestaetigungspflichtige Items modelliert, Blocker und Hinweise bleiben nicht bestaetigbar.
- Dialog `Aktivierung vorbereiten` zeigt die vorbereitete Warnungsbestaetigung jetzt rein lesend an: `ReadyWithWarnings` listet bestaetigungspflichtige Warnungen, ohne Checkbox, Bestaetigungsbutton, Speicherung oder Aktivierung.
- Rein lesender `InterfaceProfileActivationPlanService` vorbereitet; er fasst Aktivierungsbewertung, Guard-Entscheidung und Warnungsbestaetigungsvorschau zu einem beschreibenden Plan mit PlannedSteps zusammen, ohne Aktivierung, Speicherung oder Datei-/Ordneroperationen auszufuehren.
- Dialog `Aktivierung vorbereiten` zeigt den `InterfaceProfileActivationPlan` jetzt rein lesend an; PlannedSteps werden beschrieben, aber nicht ausgefuehrt.
- Dialog `Aktivierung vorbereiten` von langer MessageBox auf ein scrollbares, resizebares Preview-Fenster umgestellt; die Inhalte sind kompakter gegliedert und vermeiden doppelte Listen, bleiben aber reine Vorschau ohne Aktivierung, Warnungsbestaetigung oder Speicherung.
- Dialog `Aktivierung vorbereiten` weiter gestrafft: bestaetigungspflichtige Warnungen erscheinen zentral im Abschnitt Warnungsbestaetigung, Guard und Aktivierungsplan wiederholen die Warnungsliste nicht mehr vollstaendig.
- Interface-/Model-Skelett fuer einen spaeteren `ActivationExecutor` ergaenzt; es beschreibt Request, Result, Preconditions und Statuswerte, bleibt aber ohne produktive Implementierung, UI-Anbindung, Speicherung oder Aktivierungswirkung.
- UI-Ueberlagerung im Tab `Schnittstellenprofile` unterhalb der Ordnerbereinigung behoben.
- Tests fuer die Aktivierungsbewertung importierter Schnittstellenprofile ergaenzt, inklusive fehlender Abhaengigkeiten, fehlender Pflichtordner, BuiltIn-Schutz, optional deaktivierter XDT-Anhang-Automatik und lizenzpflichtiger Profile.

### Dokumentation

- `docs/ROADMAP.md` ergänzt als aktualisierte Roadmap für den Stand nach XDT-Anhang-Ausbau, Baukasten-Testexport, Paket-Wartelogik und UI-Refactoring.
- Aktuellen Entwicklungsstand zu XDT-Anhängen für AIS, Test & Vorschau, Testexport, Dateistabilität, konfigurierbarem Scan-Intervall und zweistufiger Paketlogik dokumentiert.
- Templatepaket-Import um aktuellen Stand ergänzt: Konfliktanalyse, Importplan, Dry-Run, UI-Vorschau, sichere Benutzerentscheidungen und explizite UserDefined-Übernahme.
- BuiltIn-Schutz, deaktiviertes `ReplaceExisting`, inaktive importierte Schnittstellenprofile und deaktiviertes `IsAttachmentProcessingEnabled` bei importierten Schnittstellenprofilen dokumentiert.
- E2E-nahe Tests für den sicheren Templatepaket-Importfluss dokumentiert.
- Praxisvalidierung MEDISTAR + NIDEK ARK1S + XDT-Anhang-Link dokumentiert.
- Pflicht-XDT-Anhang-Testfälle erfolgreich geprüft: Anhang vorhanden und Anhang fehlt/Fehlerfall.
- Projektstandsicherung zum aktuellen Aktivierungsassistenten ergänzt: Service-Kette, Statuslogik, Sicherheitsgrenzen, UI-Stand, letzter Build-/Teststand und offene Schritte dokumentiert.
- Entscheidungsnotiz `docs/AKTIVIERUNG_ENTSCHEIDUNGSNOTIZ.md` ergänzt; sie strukturiert die offenen fachlichen Entscheidungen vor einer späteren produktiven Schnittstellenprofil-Aktivierung.
- Entscheidungsnotiz um eine vorlaeufige V1-Entscheidungslinie erweitert: `Ready` aktivierbar, `ReadyWithWarnings` nur nach bewusster Bestaetigung, `Blocked`/`Unknown` nie aktivieren, `IsAttachmentProcessingEnabled` bleibt separat und finale Re-Evaluation vor Speicherung ist Pflicht.
- Entscheidungsnotiz um eine V1-Spezifikation zu Aktivierungsflag, Persistenzstelle, Parallelitaets-/Aenderungsschutz, Warnungsbestaetigung/Audit und finaler Re-Evaluation erweitert; weiterhin ohne produktive Implementierung, Speicherung oder Aktivierungswirkung.
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
