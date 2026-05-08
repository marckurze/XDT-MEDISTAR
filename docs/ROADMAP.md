# Roadmap XdtDeviceBridge

Stand: 2026-05-08

Projekt: XdtDeviceBridge / XDT Verwaltung

## 1. Aktueller Stand

### Version

- Aktuelle Version: `0.1.0-prototype`
- `VERSION` und `Directory.Build.props` sind konsistent:
  - `Version`: `0.1.0-prototype`
  - `AssemblyVersion`: `0.1.0.0`
  - `FileVersion`: `0.1.0.0`
  - `InformationalVersion`: `0.1.0-prototype`

### Validierter Kernworkflow MEDISTAR + NIDEK ARK1S

- Praktisch validierter Kernworkflow: MEDISTAR-GDT/XDT-Eingang + NIDEK ARK1S XML-Eingang + MEDISTAR-kompatibler XDT-Export.
- Erzeugt werden insbesondere:
  - `8000=6310`
  - `8402` Untersuchungsart
  - `6228` Ergebniszeilen
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

- BuiltIn-Profile werden nicht überschrieben.
- UserDefined-Profile werden separat gespeichert.
- Profile werden JSON-basiert unter `%LocalAppData%\XdtDeviceBridge\profiles` verwaltet.
- Templatepaket-Export, Templatepaket-Import und Importvalidierung sind vorbereitet.
- Die produktive Übernahme importierter Templatepakete mit Konfliktlösung ist noch offen.

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
- Manuelle Verarbeitung für den validierten Kernworkflow.
- MEDISTAR-kompatibler XDT-Export mit zentral erzeugten Längenpräfixen.
- Baukasten-Testexport für XDT-Datei plus umbenannten XDT-Anhang ist testseitig abgesichert.
- Externe AIS-Linkfelder `6302`, `6303`, optional `6304` und `6305` sind fachlich anhand des MEDISTAR-Beispiels und technisch im Baukasten-/Testpfad belegt.

Wichtig: Der XDT-Anhang-Link ist im Baukasten und in Tests vorbereitet. Eine vollständige produktive Praxisabnahme für automatische Attachment-Verarbeitung sollte anhand von `docs/END_TO_END_TESTPLAN.md` noch separat protokolliert werden.

## 3. Was ist vorbereitet, aber noch nicht produktiv validiert?

- NIDEK LM7/LM7P LAN/XML.
- NIDEK NT530P.
- TOPCON CL300.
- TOPCON KR800.
- TOPCON TRK2P.
- Vollständige produktive Templatepaket-Übernahme mit Konfliktlösung.
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
- Offene produktive Validierungen klar markieren.

### Phase 2: Templatepaket produktiv übernehmen mit Konfliktlösung

- Konfliktmodell für Profil-IDs, Namen, Versionen und BuiltIn/UserDefined-Regeln definieren.
- Importierte Profile zunächst als Vorschau anzeigen.
- Produktive Übernahme nur als UserDefined-Profile erlauben.
- Konflikte interaktiv oder regelbasiert lösen.

### Phase 3: Geräte-Datei-Explorer / Profil-Assistent

- Unbekannte Geräte-/XML-/Textdateien analysieren.
- SourcePaths anzeigen.
- Feldvorschläge und Platzhalter ableiten.
- Profilentwurf als UserDefined speichern.

### Phase 4: Produktive Validierung vorbereiteter Geräteprofile

- LM7/LM7P mit echten LAN/XML-Dateien validieren.
- NT530P, TOPCON CL300, TOPCON KR800 und TOPCON TRK2P mit echten Gerätedateien testen.
- Exporttemplates je Gerät gegen AIS-Anforderungen prüfen.
- Dokumentierte Beispielprofile mit Testergebnissen verknüpfen.

### Phase 5: Lizenzsignatur und Lizenzdurchsetzung

- Signaturformat und Public-Key-Prüfung definieren.
- Importierte Lizenzdateien kryptografisch prüfen.
- Harte Sperren erst aktivieren, wenn Ausnahmepfade und Karenzzeiten fachlich bestätigt sind.

### Phase 6: Installer / Deployment

- Installationsziel, Datenordner und Rechtekonzept definieren.
- Desktop-Shortcut und Startmenüeintrag prüfen.
- Update-/Backup-Konzept für lokale Profile und Lizenzen festlegen.

## 6. Empfohlene nächste kleine Codex-Schritte

1. `docs/ROADMAP.md` und `docs/PROJEKT_UEBERBLICK.md` fachlich abgleichen.
2. `CHANGELOG.md` mit einem Abschnitt für den aktuellen Entwicklungsstand fortführen.
3. Version für den nächsten Meilenstein nur vorbereiten, aber erst nach E2E-Abnahme erhöhen.
4. Templatepaket-Konfliktmodell als kleines Design-Dokument ergänzen.
5. Datenmodell für Templatepaket-Importkonflikte implementieren.
6. Tests für Konflikterkennung ergänzen: gleiche ID, gleicher Name, BuiltIn-Ziel, ältere Version, neuere Version.
7. Importvorschau für Templatepakete im UI vorbereiten.
8. Produktive Übernahme importierter Profile ausschließlich als UserDefined ermöglichen.
9. E2E-Testplan mit realen Testordnern ausführen und Ergebnisprotokoll ergänzen.
10. Profil-Assistent zunächst read-only beginnen: Datei laden, Parserpfade anzeigen, keine Profiländerung.
11. LM7/LM7P-Beispieldateien gegen die dokumentierten SourcePaths testen.
12. Lizenzsignatur-Konzept dokumentieren, bevor produktive Sperren umgesetzt werden.
13. Installer-/Deployment-Checkliste erstellen.

## 7. Risiken / offene Entscheidungen

- JSON vs SQLite: JSON ist aktuell einfach und transparent; SQLite kann später bei Historie, Audit und größerem Profilbestand sinnvoll werden.
- Windows-Dienst später ja/nein: aktuell bewusst nein; für echten Dauerbetrieb eventuell später erneut bewerten.
- FileSystemWatcher später ja/nein: aktuell periodischer Scan; ein Watcher könnte später ergänzen, aber nicht die Stabilitätsprüfung ersetzen.
- Lizenzsperre wann aktivieren: erst nach Signaturprüfung, Karenzzeitentscheidung und klarer Fehlerkommunikation.
- EV-Verknüpfung vs `6302`-`6305`-Link: MEDISTAR-Beispiel ist ausgewertet; die genaue AIS-Wirkung muss je Zielsystem validiert werden.
- Mehrfachanhang-Zuordnung: aktuell keine automatische Auswahl; spätere Heuristiken brauchen sichere Patienten-/Auftragsbezüge.
- Zweite AIS-Unterstützung außerhalb MEDISTAR: Feldlogik, Dateinamen, Importsteuerung und externe Links können je AIS abweichen.
- Produktive Attachment-Automatik: Tests sind vorhanden, praktische Abnahme muss die Reihenfolge AIS, Gerät und Anhang realistisch prüfen.
- Langsam schreibende Geräte: Stabilitätszeiten müssen im Praxisbetrieb je Gerät justiert werden.

## 8. Abnahmekriterien für nächsten Meilenstein

- `dotnet build XdtDeviceBridge.sln` ist grün.
- `dotnet test XdtDeviceBridge.sln` ist grün.
- E2E-Testplan aus `docs/END_TO_END_TESTPLAN.md` ist mit Ergebnisprotokoll bestanden.
- MEDISTAR + NIDEK ARK1S bleibt unverändert funktionsfähig.
- Export ohne aktivierte XDT-Anhang-Funktion enthält keine `6302`, `6303`, `6304`, `6305`.
- XDT-Anhang-Testexport erzeugt XDT-Datei plus korrekt umbenannten Anhang.
- `6305` zeigt im Baukasten-Test auf den simulierten Schnittstellenprofil-Zielpfad.
- Templatepaket kann produktiv als UserDefined übernommen werden.
- Konflikte beim Templatepaket-Import werden erkannt und nicht still überschrieben.
- BuiltIn-Profile bleiben geschützt.
- Keine unbekannten Dateien werden gelöscht, verschoben oder verarbeitet.
