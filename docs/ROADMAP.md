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
- Mehrere stabile unterstützte Anhänge werden in stabiler Dateinamen-Reihenfolge einzeln verarbeitet; jede Datei erzeugt eine eigene Linkfeldgruppe `6302`, `6303`, optional `6304` und `6305`.

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
  - ein oder mehrere stabile unterstützte Anhänge: Export mit Linkfeldern je Datei
  - kein Anhang nach Timeout: Export ohne Linkfelder
  - vorhandene, aber noch instabile Anhänge: weiter warten, solange die Wartezeit läuft
- Nach erfolgreichem Export mit einem oder mehreren Anhängen ist der Vorgang terminal abgeschlossen: bekannte AIS-/Gerätedateien werden gemäß Profilregel nachbehandelt, alte Timeout-Anzeigen werden nicht als finaler Kartenstatus beibehalten.
- Pflicht-XDT-Anhang:
  - ein oder mehrere stabile unterstützte Anhänge: Export mit Linkfeldern je Datei
  - kein Anhang nach Timeout: Blockade/Fehlerstatus
  - vorhandene, aber noch instabile Anhänge: weiter warten; nach Timeout greift die Pflicht-Fehlerlogik

### Schnittstellenprofile und Templatepakete

- Die kompakte Geraete-/Template-Bestandsaufnahme steht in `docs/GERAETE_PROFILE_TEMPLATE_MATRIX.md`.
- Die erste offizielle Paketvorlage fuer `MEDISTAR + NIDEK ARK1S` steht in `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_ARK1S.md`; der technische Export-/Import-Testweg erzeugt die ZIP temporaer mit `TemplatePackageExporter`, prueft sie mit `TemplatePackageImporter` und deckt den UI-nahen Importvorschau-Pfad ab. Eine dauerhaft abgelegte ZIP-Paketdatei bleibt bis zur Freigabe nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md` offen.
- `MEDISTAR + NIDEK AR360` ist als zweiter praktischer Auto-Refraktor-Workflow fuer die XDT-Rueckgabe validiert: BuiltIn-Geraeteprofil, Exportprofil und Schnittstellenprofil sind vorhanden; ARMedian, FarPD, VD, UTF-16-XML, `8402 = AR360` und die `6228`-Zeilen sind testseitig und praktisch belegt. Der AR360-Templatepaket-Test ist analog zu ARK1S abgesichert; XDT-Anhangfall und eine offizielle ZIP-Ablage bleiben offen.
- `MEDISTAR + NIDEK NT530P` ist als testseitig direkt nutzbarer Tonometrie-/Pachymetrie-Kandidat vorhanden: echte UTF-16-XML-Fixture, BuiltIn-Geraeteprofil, Exportprofil mit mehrzeiliger `6205`-Tonometrie samt Header, `6220`-Pachymetrie samt Header, Schnittstellenprofil, selektiver Templatepaket-Test und korrigierter Nachlauf nach Mehrfachanhang-Export sind abgesichert. Praktische MEDISTAR-Nachpruefung und NT530P-JPG-Anhangfall bleiben offen.
- `MEDISTAR + TOPCON CL300` ist als erster TOPCON-Lensmeter-Referenzkandidat praktisch validiert: echte CL-300-XML-Fixtures mit `nsCommon`/`nsLM`, BuiltIn-Geraeteprofil, Exportprofil, Schnittstellenprofil, `6228`-MedistarLine-Ausgabe mit ADD, PD und signierten H/V-Prismenkomponenten, selektiver Templatepaket-Test und Praxisprotokoll sind vorhanden.
- `MEDISTAR + TOPCON Solos` ist als weiterer TOPCON-Lensmeter-Kandidat testseitig vorbereitet: echte `SolosExportSample.xml`-Fixture mit `ModelName = SOLOS`, BuiltIn-Geraeteprofil, Exportprofil, Schnittstellenprofil, `6228`-MedistarLine-Ausgabe mit PD und signierten H-Prismen, selektiver Templatepaket-Test und Fixture-Protokoll sind vorhanden. Transmission und PDF-Berichte bleiben ohne echte gefuellte Fixtures offen.
- `MEDISTAR + TOPCON KR800S` ist als praktisch validierter TOPCON-Mehruntersuchungs-Kandidat vorhanden: echte Shift-JIS-XML-Fixtures mit `nsCommon`/`nsREF`/`nsKM`/`nsSBJ`, BuiltIn-Geraeteprofil, Exportprofil, Schnittstellenprofil, `6228` REF, `6221` KM, konservative `6227` SBJ-Zeilen mit getrennten Header-/Messwertzeilen, selektiver Templatepaket-Test und E2E-Praxisprotokoll sind vorhanden. Alte persistierte BuiltIn-Exportprofile mit leeren Einzelplatzhalter-Templates oder KM ueber `6228` werden gezielt repariert.
- `MEDISTAR + TOPCON KR-1` ist als Keratorefraktometer-Kandidat testseitig vorbereitet: echte Shift-JIS-Serial0001-Fixture mit `ModelName = KR-1`, `Measure type="REF"`, REF-Medianwerte ueber `6228`, BuiltIn-Geraeteprofil, Exportprofil, Schnittstellenprofil, selektiver Templatepaket-Test und Fixture-Protokoll sind vorhanden. KM/KRT/periphere KRTs bleiben ohne echte KM/KRT-Fixture offen.
- `MEDISTAR + TOPCON TRK2P` ist als praktisch validierter TOPCON-Mehruntersuchungs-Kandidat vorhanden: echte XML-Fixtures mit `nsCommon`/`nsREF`/`nsKM`/`nsTM`, inklusive TM/CCT-only-Teilmessung, BuiltIn-Geraeteprofil, Exportprofil, Schnittstellenprofil, `6228` REF, `6221` KM, `6220` CCT/Pachy mit Header, mehrzeilige `6205`-Tonometrie im NT530P-Stil, optionaler `6227` SBJ-Pfad, selektiver Templatepaket-Test, E2E-Praxisprotokoll und Repair alter persistierter BuiltIns sind vorhanden.
- `MEDISTAR + TOPCON CT1P` ist als praktisch validierter TOPCON-Tono/Pachy-Kandidat vorhanden: echte Serial0214-XML-Fixture mit `nsCommon`/`nsTM`, BuiltIn-Geraeteprofil, Exportprofil, Schnittstellenprofil, `6205` Tonometrie im NT530P/TRK2P-Mehrzeilenformat, `6220` Pachymetrie, einseitig unvollstaendige CorrectedIOP/CCT-Bloecke ohne leere Fragmente, selektiver Templatepaket-Test und E2E-Praxisprotokoll sind vorhanden.
- `MEDISTAR + TOPCON CT800A` ist als TOPCON-Non-Contact-Tonometer-Kandidat testseitig vorbereitet: drei echte CT-800A-XML-Fixtures mit `nsCommon`/`nsTM`, BuiltIn-Geraeteprofil, Exportprofil, Schnittstellenprofil, `6205` Tonometrie, vollstaendige CorrectedIOP/CCT-Detailzeilen nur bei verwertbaren per-eye-Daten, selektiver Templatepaket-Test und Fixture-Protokoll sind vorhanden.
- `MEDISTAR + TOPCON CV5000` ist als erster bidirektionaler TOPCON-Phoropter-Kandidat testseitig vorbereitet und im Scanpfad angebunden: AIS-Historienparser fuer MEDISTAR-Karteikartenzeilen, Auswahlfenster `Werte an Phoropter uebergeben`, CV-5000-Import-XML-Writer, CV-5000-Rueckgabeparser, BuiltIn-Geraeteprofil, Exportprofil, Schnittstellenprofil, fachlich getrennte Rueckgabe (`Prescription` vollstaendig ueber `6228`, `Full Correction` vollstaendig ueber `6227`, keine `6330`-Zeilen), schnittstellenprofilbasierte Ausgabe-an-Geraet-Konfiguration, selektiver Templatepaket-Test und Fixture-Protokoll sind vorhanden. Das Auswahlfenster startet per `🔝` TopMost und bietet `Nichts senden`, wodurch keine `CVImport.xml` geschrieben wird, der Rueckgabe-Workflow aber offen bleibt. Das Geraeteprofil markiert nur die bidirektionale Faehigkeit; Ausgabeordner, Dateiname und Format liegen im Schnittstellenprofil. Der Rueckweg liest MEDISTAR-Historien-AIS-Dateien im CV-5000-Kontext tolerant. Der zweiphasige Lauf verarbeitet aktuell im Eingangsordner liegende stabile, lesbare und fachlich passende Dateien ohne interne AlreadyProcessed-/Duplicate-Sperre; Nachlauf/Archivierung entscheiden organisatorisch, ob Dateien nach Erfolg aus dem Importordner verschwinden. Der MEDISTAR-Import der erzeugten Rueckgabedatei ist mit korrigierter Feldzuordnung praktisch bestaetigt.
- `MEDISTAR + Dokumentanhang` ist als AttachmentOnly-/Dokumentgeraete-V1-Kandidat vorhanden: AIS-Datei plus eine oder mehrere stabile Anhangdateien und je Datei eigene `6302`-`6305`-Linkfelder; `6304` wird mit Benutzerbeschreibung oder Originaldateiname gefuellt, `6305` bleibt der technische Pfad. Es werden keine Messwertfelder erzeugt. Fuer mehrere nacheinander geschriebene Dateien gibt es profilbezogen `Abschluss nach Wartezeit` und `Manuell bestaetigen`; im manuellen Modus bleibt der Dialog offen und sammelt weitere Dateien bis `Uebertragen`. Der Dokument-Importordner ist die Anhangquelle, alte deaktivierte technische Anhang-Flags blockieren AttachmentOnly nicht mehr, und die Monitoring-Karte bleibt vor Vorgangsbeginn neutral.
- `MEDISTAR + Manuelle Dokumentuebergabe` ist als eigener V1-Kandidat vorhanden: Erst eine AIS-Datei startet den Uebertragen-Dialog mit leerer Liste, das gruene Geraeteanbindungsfenster bleibt angedockt, der Anwender fuegt Dateien per Drag-&-Drop, Dateiauswahl oder Datei-Zwischenablage hinzu, und erst `Uebertragen` exportiert je Datei eigene `6302`-`6305`-Linkfelder. Nach Abschluss wird der interne State freigegeben, damit der naechste AIS-Vorgang auch mit gleichem Dateinamen wieder einen neuen leeren Dialog oeffnet. Ein Geraete-Importordner ist fuer diesen Modus nicht erforderlich.
- Die manuelle Dokumentuebergabe aktualisiert den offenen Dialog nur noch inkrementell, schreibt TextBox-Inhalte nicht mehr bei jedem Tastendruck zurueck und bleibt standardmaessig per kompaktem `🔝`-Schalter im Vordergrund. Polling stoert die Texteingabe dadurch deutlich weniger; die XDT-Fachlogik bleibt unveraendert.
- Der Templatepaket-Export ist auf eine schlanke V1-Auswahl umgestellt: Der Benutzer waehlt ein Schnittstellenprofil als Paketbasis, und das Paket enthaelt nur dieses Schnittstellenprofil plus referenziertes AIS-, Geraete- und Exportprofil.
- BuiltIn-Profile werden nicht überschrieben.
- UserDefined-Profile werden separat gespeichert.
- Profile werden JSON-basiert unter `%LocalAppData%\XdtDeviceBridge\profiles` verwaltet.
- Templatepaket-Export und Templatepaket-Import sind vorhanden.
- UserDefined-Exportprofile koennen schlank gewartet werden: unreferenzierte UserDefined-Exportprofile lassen sich nach Sicherheitsabfrage loeschen, Exportregeln lassen sich nur aus UserDefined-Exportprofilen entfernen.
- Neue AIS-, Geraete- und Exportprofile koennen als schlanke V1 im Tab `Profile & Templates` angelegt werden. AIS/Geraet nutzen einfache Dialoge mit kurzen Hilfetexten zu System, Codierung, Geraetetyp und Parserbasis. `Neues Exportprofil anlegen` startet sichtbar einen leeren Entwurf mit eindeutigem Namen; gespeichert wird erst bewusst als UserDefined. BuiltIns bleiben geschuetzt und es erfolgt keine automatische Aktivierung.
- UserDefined-AIS-, Geraete-, Export- und Schnittstellenprofile koennen umbenannt werden. Dabei wird nur der sichtbare Name gespeichert; IDs, Referenzen, Exportregeln, Ordnerpfade, XDT-Anhang-Einstellungen und Aktivierungsstatus bleiben unveraendert. BuiltIns bleiben geschuetzt.
- Templatepaket-ZIP-Dateien erhalten keine eigene App-interne Umbenennungsverwaltung. Paketnamen werden weiter ueber Datei-/Release-Regel gefuehrt; Zielnamen importierter Profile bleiben in der Importvorschau editierbar.
- Das generische Dokumentgeraet nutzt den bestehenden Mehrfachanhang-Pfad als direkt nutzbaren V1-Kandidaten: XML, PDF, Bild-, DICOM-, Text-, Video- und Audiodateien sind reine Anhaenge; sichtbare Dateibeschreibungen werden pro Datei ueber `6304` ausgegeben. Der Automatikabschluss wartet standardmaessig auf eine kurze Ruhezeit nach der letzten Datei, alternativ entscheidet der Anwender im Uebertragen-Dialog.
- Importierte Templatepakete werden validiert.
- Konflikte werden analysiert: gleiche ID, gleicher Name, BuiltIn-Schutz, UserDefined-Konflikte, fehlende Abhängigkeiten und prüfpflichtige Ordner-/XDT-Anhang-Einstellungen.
- Aus der Analyse wird ein Importplan erzeugt.
- Der Dry-Run und die UI-Importvorschau zeigen geplante Aktionen, Ziel-IDs/Zielnamen, Abhängigkeiten und Warnungen; wenn wegen `Ueberspringen` keine Abhängigkeiten anzuzeigen sind, erscheint ein kurzer Leerhinweis.
- Die WPF-Importvorschau ist stabilisiert: Vorschau-Erstellung laeuft ueber einen testbaren Preview-Service, parallele Importvorschauen werden verhindert, rekursive Auswahlereignisse werden ignoriert, und sichtbare Hinweise/Warnungen sind deutsch lokalisiert sowie dedupliziert.
- Bei Konflikten und BuiltIn-Schutz ist `Überspringen` der sichere Standard; `Als Kopie importieren` wird nur nach bewusster Benutzerwahl ausgefuehrt.
- Zielnamen fuer bewusst gewaehlte Kopien sind in der Vorschau editierbar; leere oder doppelte Namen blockieren die Uebernahme.
- Sichere Benutzeraktionen sind möglich:
  - `Neu importieren`
  - `Als Kopie importieren`
  - `Bestehendes behalten`
  - `Überspringen`
- Die explizite Übernahme als UserDefined-Profile ist vorhanden.
- `ImportAsNew` und `ImportAsCopy` werden geschrieben; `KeepExisting`, `Skip` und blockierte Profile werden nicht geschrieben.
- BuiltIn-Profile werden nicht überschrieben.
- Abhängigkeiten von importierten Schnittstellenprofilen werden auf lokale oder neu importierte Zielprofile zugeordnet.
- Importierte Schnittstellenprofile bleiben inaktiv.
- `IsAttachmentProcessingEnabled` wird bei importierten Schnittstellenprofilen deaktiviert.
- XDT-Anhang-Einstellungen bleiben erhalten und müssen vor Aktivierung geprüft werden.
- `ReplaceExisting` bleibt deaktiviert.
- Der sichere Importfluss ist E2E-nah automatisiert getestet.

### Abdockbare Geraeteanbindungsfenster

- Status: V1 praxisabgenommen, siehe `docs/PRAXISABNAHME_GERAETEFENSTER_V1.md`.
- Im Tab `Verarbeitung` ist die erste manuelle Grundlage vorhanden: Monitoring-/Geraetekarten koennen pro Schnittstellenprofil abgedockt und ueber das Floating-Fenster wieder angedockt werden.
- Pin setzt das Floating-Fenster pro Profil in den Vordergrund; Positionsmerken speichert Position, Groesse und Abdockstatus persistent als UI-State unter AppData.
- Persistierte Floating-Fenster werden erst nach der sicheren MainWindow-Anzeige wiederhergestellt; falls ein Restore fehlschlaegt, wird die Karte angedockt statt die App zu blockieren.
- Schliessen per `X` dockt sicher zurueck. Radar-/Scanbalken und `-`/`+`-Scanintervallsteuerung sind im Floating-Fenster sichtbar.
- Fuer TOPCON CV5000 / CV-5000S ist als Pilot eine neue grafische Monitoring-Darstellung vorhanden: kompakteres Geraetefenster, groesseres Geraetebild, linker Infoblock, Statuskugel mit gruenem Dateieingangs-Flash und Puls nur bei laufender Ueberwachung sowie mittige Intervallsteuerung. `Letzter Scan` und `Automatik` sind dort in die Details verschoben; andere Geraete bleiben vorerst unveraendert beim Radar.
- Erste automatische Oeffnung ist vorhanden: relevante Monitoring-Aktivitaet wie AIS-/Geraetedatei, Anhang-/Paketstatus oder Verarbeitungsergebnis dockt nur das betroffene Floating-Fenster ab und bringt es nach vorne.
- Wiederholte Durchlaeufe sind abgesichert: neue Dateiversionen mit gleichem Namen erzeugen erneut einen profilbezogenen AutoDetach-Impuls; identische Scan-Wiederholungen bleiben weiterhin gegen Spam dedupliziert.
- AlreadyProcessed-/Duplikat-Sperren werden fuer die automatische Verarbeitung nicht mehr verwendet. Aktuelle stabile und passende Eingangsdateien werden verarbeitet; Archiv-, Verschiebe- und Entfernen-Regeln sorgen danach fuer saubere Importordner, ohne unbekannte Dateien anzufassen.
- Nach Vorgangsreset und manuellem Andocken per `X` wird der profilbezogene Monitoring-Dedupe-State freigegeben, damit eine neue AIS-Datei wieder AutoDetach ausloest.
- Bei stabil erkanntem Geraetedatei-Eingang wird der App-Content-Signalton `04_praxis_terminal_signal.wav` abgespielt; AIS-, Anhang-, Export- und Statusmeldungen bleiben stumm, ein kurzer Cooldown pro Schnittstellenprofil verhindert Sound-Spam.
- Automatisches Zurueckandocken ist vorhanden: nach terminalem Monitoring-Abschluss startet fuer automatisch geoeffnete, nicht gepinnte Fenster eine Restlaufzeit von 5 Sekunden; neue Aktivitaet, Pin oder manuelles Andocken bricht den Countdown pro Profil ab.
- Systray-Grundfunktion ist vorhanden: Minimieren oder `X` blendet das Hauptfenster in den Infobereich aus, Doppelklick beziehungsweise Kontextmenue `Oeffnen` stellt es wieder her, `Beenden` schliesst die App bewusst.
- Vorgangsreset `↺` ist in angedockten Monitoring-Karten und Floating-Fenstern vorhanden: Der aktuelle Vorgang eines Schnittstellenprofils kann nach Sicherheitsabfrage verworfen werden; AIS-, Geraete- und optionaler XDT-Anhang-Eingangsordner dieses Profils werden top-level geleert, Export-, Archiv- und Fehlerordner sowie Unterordner bleiben unangetastet.
- Floating-Fenster sind leicht verbreitert; die Symbolleiste `🗗 ↺ 📌 🔝` bleibt horizontal.
- Noch nicht enthalten sind Autostart, Windows-Dienst, UI-Einstellung fuer die Rueckdock-Zeit, sichtbarer Countdown-Hinweis, UI-Schalter fuer Signalton und ein ggf. eigenes Systray-Icon.
- Die Funktion bleibt UI-/Reset-Funktion und aendert keine Verarbeitung ausserhalb des explizit bestaetigten Resets, keine Profile und kein Aktivierungsmodell.

### Aktivierungsassistent fuer importierte Schnittstellenprofile

- Der Aktivierungsassistent ist als reine Vorschau vorbereitet, aber noch nicht produktiv aktivierend.
- Im Tab `Schnittstellenprofile` gibt es den Bereich `Pruefung vor Aktivierung` mit Status, Aktivierbarkeit, Blocker-/Warnungs-/Hinweiszaehlern, strukturierter Ordnerpruefung, Ordner-Erreichbarkeit, strukturierter XDT-Anhang-Konfiguration und eingeklappter Tabelle `Alle Pruefpunkte`. Die Pruefung arbeitet mit den aktuellen UI-Entwurfswerten, ohne zu speichern.
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
- Sicherer Templatepaket-Importfluss ist E2E-nah testseitig abgesichert: Export/Import, Validierung, Konfliktanalyse, Importplan, Benutzerwahl, Dry-Run, UserDefined-Übernahme und Abhängigkeitszuordnung.
- MEDISTAR + NIDEK ARK1S ist als Referenzpaket reproduzierbar testseitig export-/importgeprueft; die ZIP wird dabei nur im temporaeren Testordner erzeugt, und die App-Importvorschau ist gegen den zuvor beobachteten Freeze abgesichert.
- MEDISTAR + NIDEK AR360 ist als zweites Referenzpaket reproduzierbar testseitig export-/importgeprueft und fuer Auto-Refraktor-XDT-Rueckgabe praktisch validiert; das Protokoll steht in `docs/E2E_TESTPROTOKOLL_MEDISTAR_AR360.md`.
- MEDISTAR + NIDEK LM7 ist als Lensmeter-Referenzkandidat reproduzierbar testseitig export-/importgeprueft und fuer Lensmeter-XDT-Rueckgabe praktisch validiert; das Protokoll steht in `docs/E2E_TESTPROTOKOLL_MEDISTAR_LM7.md`.
- Geraeteanbindungsfenster V1 sind fuer AR360/ARK1S praxisabgenommen: Systray, Floating-Fenster, Pin/Position, Auto-Oeffnung, Auto-Zurueckandocken, Signalton und Reset sind dokumentiert in `docs/PRAXISABNAHME_GERAETEFENSTER_V1.md`.

Praxis- und Fixture-Protokolle: `docs/E2E_TESTPROTOKOLL_MEDISTAR_ARK1S_XDT_ANHANG.md`, `docs/E2E_TESTPROTOKOLL_MEDISTAR_AR360.md`, `docs/E2E_TESTPROTOKOLL_MEDISTAR_LM7.md`, `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_CL300.md`, `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_KR800S.md`, `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_KR1.md`, `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_TRK2P.md`, `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_CT1P.md`, `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_CT800A.md`, `docs/E2E_TESTPROTOKOLL_MEDISTAR_TOPCON_CV5000.md` und `docs/PRAXISABNAHME_GERAETEFENSTER_V1.md`. Die vollstaendige Abarbeitung aller weiteren Testfaelle aus `docs/END_TO_END_TESTPLAN.md` bleibt als separater Schritt offen.

## 3. Was ist vorbereitet, aber noch nicht vollstaendig produktiv freigegeben?

- NIDEK AR360 / AR-360A LAN/XML ist fuer Auto-Refraktor-XDT-Rueckgabe praktisch validiert; offen bleiben AR360-XDT-Anhangfall und offizielles ZIP-Release-Artefakt.
- NIDEK LM7/LM7P LAN/XML ist mit echter XML-Fixture, `Sphare`/`Sphere`-Toleranz, MEDISTAR-Lensmeter-Ausgabe, Reparatur alter persistierter BuiltIn-Exportpfade und Templatepaket-Kandidat testseitig vorbereitet; die Lensmeter-XDT-Rueckgabe ist praktisch in MEDISTAR validiert.
- NIDEK NT530P: echte XML-Fixture, BuiltIn-Schnittstellenprofil, korrigierter mehrzeiliger `6205`-/`6220`-Export ohne EV-Zusatz, Mehrfachanhang-Linkfelder, Nachlauf-/Monitoring-Reset und Templatepaket-Kandidat sind testseitig vorhanden; praktische MEDISTAR-Nachpruefung offen.
- Generisches Dokumentgeraet / AttachmentOnly: BuiltIn-Schnittstellenprofil, sichtbare Option `Dokumentationstext erfassen`, pro-Datei-Beschreibung ueber `6304`, mehrere `6302`-`6305`-Anhaenge, XML als Anhang statt Messwertdatei, Ruhezeit-/Bestaetigungsabschluss und Templatepaket-Kandidat sind testseitig vorhanden; praktische MEDISTAR-Abnahme offen.
- TOPCON CL300: echte XML-Fixtures, BuiltIn-Schnittstellenprofil, `6228`-Lensmeter-Ausgabe, Templatepaket-Kandidat und praktische MEDISTAR-Abnahme sind vorhanden; offen bleiben weitere Beobachtung der H/V-Prismendarstellung und ein offizielles ZIP-Artefakt nach Release-Regel.
- TOPCON KR800S: praktisch validiert; leere Live-Platzhalter durch BuiltIn-Repair behoben, getrennte SBJ-`6227`-Zeilen dokumentiert.
- TOPCON KR-1: fixture-validiert mit echter Shift-JIS-Serial0001-Fixture, `6228` REF aus Medianwerten, Templatepaket-Kandidat und Fixture-Protokoll; praktische MEDISTAR-Abnahme und echte KM/KRT-Fixture noch offen.
- TOPCON TRK2P: praktisch validiert mit echten Serial0001-/Serial0135-/Serial1165-XML-Fixtures, REF-`6228`, KM-`6221`, Pachy-`6220`, Tono-`6205`, TM/CCT-only-Teilmessung, optionalem SBJ-`6227`-Pfad und Templatepaket-Kandidat.
- TOPCON CT1P: praktisch validiert mit echter Serial0214-Fixture, `6205` Tono, `6220` Pachy, einseitig verwertbarem CorrectedIOP/CCT, Templatepaket-Kandidat und E2E-Praxisprotokoll; echte beidseitige CorrectedIOP/CCT-Datei noch sammeln.
- TOPCON CT800A: fixture-validiert mit drei echten CT-800A-Fixtures, `6205` Tono, vollstaendigen CorrectedIOP/CCT-Detailzeilen nur bei Serial0069 und ausgelassenen unvollstaendigen CorrectedIOP-/ERROR-Bloecken in Serial0070/Serial0071; praktische MEDISTAR-Abnahme noch offen.
- TOPCON CV5000: bidirektionaler Phoropter-Kandidat mit AIS-Historienparser, angebundenem Auswahlfenster `Werte an Phoropter uebergeben`, CV-5000-Import-XML-Writer, Rueckgabeparser, `6228`-Prescription-Export, `6227`-Full-Correction-Export, BuiltIn-Profilen, Schnittstellenprofil-Konfiguration fuer Ausgabe an Geraet, Templatepaket-Kandidat und Fixture-Protokoll; Rueckweg mit MEDISTAR-Historien-AIS und MEDISTAR-Import der Rueckgabedatei sind abgesichert.
- Fertige, auslieferbare Geraete-Templatepakete als ZIP-Artefakte. Fuer ARK1S, AR360, LM7, NT530P, CL300, Solos, KR800S, KR-1, TRK2P, CT1P, CT800A, CV5000 und Dokumentanhang sind Referenz- beziehungsweise Kandidatendokumentation und reproduzierbare Export-/Import-Tests bereits vorhanden; dauerhafte ZIPs folgen erst nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md`.
- `ReplaceExisting` für UserDefined-Profile.
- Freie Konfliktlösungs-/Bearbeitungsdialoge.
- Manuelle Zielnamen-/ID-Bearbeitung in der UI.
- Produktive Aktivierung importierter Schnittstellenprofile; Pruefung, Guard und V1-Preview sind nur read-only vorbereitet.
- Produktiver `ActivationExecutor` mit frischem Laden, finaler Evaluation + Guard und sicherer UserDefined-Speicherung.
- Vollständiger Profil-Assistent für unbekannte Geräte; die schlanke V1-Anlage von AIS-, Geraete- und Exportprofilen als UserDefined ist vorhanden, ersetzt aber noch keinen gefuehrten Datei-/Messwert-Assistenten.
- Digitale Lizenzsignatur.
- Online-Lizenzierung.
- Harte produktive Lizenzdurchsetzung.
- Installer / Deployment.
- Vollständige AIS-Unterstützung außerhalb MEDISTAR.
- praktische Mehrfachanhang-Abnahme in MEDISTAR, insbesondere fuer NT530P-JPG-Aufnahmen.
- Praktische MEDISTAR-Abnahme des Dokumentanhang-Kandidaten ueber den Baukasten hinaus.

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
- Mehrere XDT-Anhänge nur als einzelne stabile unterstützte Dateien mit eigenen Linkfeldgruppen übergeben.
- Keine ZIP-/Sammellogik fuer mehrere Anhaenge.
- Keine produktive Änderung von Exportprofilen durch Baukasten-Test oder Testexport.
- XDT-Längenpräfixe nicht manuell konfigurieren, sondern zentral erzeugen.

## 5. Empfohlene nächste Entwicklungsphasen

### Phase 1: Dokumentation und Version konsolidieren

- Roadmap, Projektüberblick, Changelog und End-to-End-Testplan aufeinander abstimmen.
- Aktuellen Stand `0.1.0-prototype` sauber vom nächsten Meilenstein trennen.
- Praxisvalidierung MEDISTAR + NIDEK ARK1S + XDT-Anhang-Link als bestanden führen.
- Restliche E2E-Fälle klar als offen markieren.

### Phase 2: Fertige Geraeteprofile und Templatepakete

- MEDISTAR + NIDEK ARK1S als stabilen Referenzworkflow und Referenzpaket 1 schuetzen.
- Die dokumentierte ARK1S-Paketvorlage und den reproduzierbaren Export-/Import-Testweg als Grundlage fuer ein spaeteres ZIP-Release-Artefakt nutzen.
- NIDEK AR360 als zweiten Referenzworkflow und Referenzpaket 2 stabil halten; offizielle ZIP-Ablage und ggf. separaten XDT-Anhangtest planen.
- NIDEK LM7/LM7P als dritten Referenzkandidaten stabil halten; der testseitige Profil-/Templatepaket-Kandidat, der reparierte Live-/Preview-Pfad und das MEDISTAR-Praxisprotokoll sind vorhanden.
- NIDEK NT530P als naechsten praktischen MEDISTAR-Testkandidaten mit korrigiertem `6205`-/`6220`-Layout, JPG-Mehrfachanhaengen und sauberem Ordner-/Karten-Nachlauf erneut abnehmen; TOPCON CL300, TOPCON KR800S, TOPCON TRK2P und TOPCON CT1P als praktisch validierte TOPCON-Referenzkandidaten beibehalten, H/V-Prisma beziehungsweise SBJ-Funktionstestfaelle weiter beobachten; TOPCON Solos, TOPCON KR-1 und TOPCON CT800A praktisch in MEDISTAR abnehmen und echte Transmission-/PDF-, KM/KRT- beziehungsweise weitere Tonometrie-Faelle sammeln; TOPCON CV5000 als ersten bidirektionalen Phoropter-Kandidaten weiter live validieren, insbesondere den MEDISTAR-Import der erzeugten `6228`-/`6227`-Rueckgabe.
- Generisches Dokumentgeraet als AttachmentOnly-V1 praktisch testen: mehrere Anhaenge, Ruhezeit nach letzter Datei, manueller Uebertragen-Dialog, pro-Datei-`6304`-Beschreibung, keine `6228`/`6205`/`6220`-Messwertausgabe.

### Phase 3: Baukasten schlank halten

- Der Baukasten bleibt fuer Tests, Vorschau und Sonderfaelle.
- Keine zusaetzlichen Assistenten- oder Wizard-Ebenen einbauen, solange fertige Profile/Templatepakete fehlen.
- Wenn ein Geraete-Datei-Explorer entsteht, zunaechst read-only: Datei laden, SourcePaths anzeigen, keine Profiländerung.

### Phase 4: Geräte-Datei-Explorer / Profil-Assistent

- Unbekannte Geräte-/XML-/Textdateien analysieren.
- SourcePaths anzeigen.
- Feldvorschläge und Platzhalter ableiten.
- Schlanke AIS-/Geraete-/Exportprofil-Anlage ist vorhanden; als naechster Ausbau bleibt der gefuehrte Profilentwurf mit Messwertuebernahme.

### Phase 5: Produktive Validierung vorbereiteter Geräteprofile

- LM7/LM7P mit der echten LAN/XML-Fixture ist testseitig und praktisch fuer die Lensmeter-XDT-Rueckgabe validiert; alte persistierte BuiltIn-LM7-Exportprofile werden auf die passenden `MedistarLine`-Parserpfade repariert. Naechster Schritt sind weitere Prisma-/PD-Sonderfaelle.
- NT530P mit dem vorhandenen BuiltIn-Schnittstellenprofil in MEDISTAR erneut praktisch testen, inklusive korrigierter Tonometrie-/Pachymetrieanzeige, optionalem JPG-Mehrfachanhang, Archiv-/Entfernen-Nachlauf und neutralem Monitoring-Kartenstatus nach Export.
- TOPCON CL300 mit dem vorhandenen BuiltIn-Schnittstellenprofil als praktisch validierten Referenzkandidaten halten: Serial0001 ohne ADD/PD/Prisma und Serial1521 mit ADD, PD und signierten H/V-Prismenkomponenten sind protokolliert.
- TOPCON Solos mit dem vorhandenen BuiltIn-Schnittstellenprofil praktisch in MEDISTAR abnehmen: `SolosExportSample.xml` deckt `6228`-Lensmeter-Ausgabe mit PD und H-Prisma ab; echte Transmission-Werte und PDF-Berichte bleiben fuer spaetere Erweiterung zu sammeln.
- TOPCON KR800S mit dem vorhandenen BuiltIn-Schnittstellenprofil als validierten Referenzkandidaten halten: Serial0036 und Serial0426 decken `6228` REF, `6221` KM und konservative `6227` SBJ-Ausgabe mit getrennten Header-/Messwertzeilen ab.
- TOPCON KR-1 mit dem vorhandenen BuiltIn-Schnittstellenprofil praktisch in MEDISTAR abnehmen: Serial0001 deckt Shift-JIS, `Measure type="REF"` und `6228`-Ausgabe aus REF-Medianwerten ab; echte KM/KRT-/periphere-KRT-Dateien bleiben fuer spaetere `6221`-Erweiterung zu sammeln.
- TOPCON TRK2P mit dem vorhandenen BuiltIn-Schnittstellenprofil als validierten Referenzkandidaten halten: Serial0001 deckt REF/KM/TM ohne CCT ab, Serial0135 deckt REF/KM/TM mit CorrectedIOP-CCT-Fallback fuer `6220` ab, Serial1165 deckt TM/CCT-only mit ERROR-Eintraegen und CCT-Mittelwertregel ab; SBJ bleibt optional und ist in den aktuellen Fixtures nicht enthalten.
- TOPCON CT1P mit dem vorhandenen BuiltIn-Schnittstellenprofil als validierten Referenzkandidaten halten: Serial0214 deckt TM-Listen/Average, rechte CorrectedIOP/CCT-Werte und ausgelassene unvollstaendige linke CCT-/Korrekturwerte ab; weitere Datei mit vollstaendigem linken CorrectedIOP/CCT waere fuer die beidseitige Praxisdarstellung wertvoll.
- TOPCON CT800A mit dem vorhandenen BuiltIn-Schnittstellenprofil praktisch in MEDISTAR abnehmen: Serial0069 deckt TM-Listen/Average und vollstaendige CorrectedIOP/CCT-Detailzeilen ab, Serial0070/Serial0071 decken Tonometrie ohne vollstaendige CCT-/Korrekturwerte und ERROR-Detailbloecke ab.
- TOPCON CV5000 mit dem vorhandenen BuiltIn-Schnittstellenprofil weiter praktisch validieren: Ausgabeordner im Bereich `Ausgabe an Geraet` setzen, AIS-Historienauswahl aus dem angebundenen TopMost-Dialog mit `Patient_mit_Phoropter_Daten.XDT`, Schreiben der CV-5000-Import-XML oder bewusstes `Nichts senden`, Einlesen am Phoropter und Rueckgabe mit `Prescription` unter `6228` sowie `Full Correction` unter `6227` pruefen. Nach erfolgreicher Rueckgabe soll die Nachlaufkonfiguration Importordner sauber halten; bleiben `Patient.gdt` oder die Phoropter-Rueckgabe bewusst liegen, ist erneute Verarbeitung beim naechsten Scan zulaessig. Die Fixture-Tests fuer Importwriter, UI-Sichtbarkeit, Dialog-Trigger-Policy, Rueckgabeparser und toleranten Rueckweg mit MEDISTAR-Historien-AIS sind vorhanden.
- MEDISTAR + Dokumentanhang mit PDF/JPG/XML/MP4/MP3/WAV-Dateien praktisch pruefen, inklusive Ruhezeitmodus, manuellem Bestaetigungsmodus und sichtbarer `6304`-Beschreibung je Datei; Vorschaukachel-Galerie und OCR bleiben separate spaetere Themen.
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

1. Release-Regel aus `docs/TEMPLATEPAKET_RELEASE_REGEL.md` fuer ARK1S und AR360 anwenden, praktische App-Importabnahme bestaetigen und danach offizielle ZIP-Artefakte ablegen.
2. Geraeteanbindungsfenster V1 als abgenommenen Block beibehalten; Komfortthemen wie Rueckdock-Zeit-UI, Countdown-Hinweis, Ton-Schalter oder eigenes Systray-Icon nur nach Praxisfeedback priorisieren.
3. `docs/GERAETE_PROFILE_TEMPLATE_MATRIX.md` als Arbeitsliste fuer Geraete-/Templatepakete fortfuehren.
4. LM7/LM7P als praktisch validierten Referenzkandidaten beibehalten und weitere Prisma-/PD-Beispielfaelle sammeln.
5. Fuer LM7/LM7P danach ueber ein offizielles ZIP-Artefakt nach Release-Regel entscheiden.
6. Die neue schlanke Profilanlage im Live-Test mit einer UserDefined-AIS-Kopie, einem einfachen UserDefined-Geraeteprofil und einer Exportprofil-Kopie pruefen.
7. Danach NIDEK NT530P praktisch in MEDISTAR mit korrigierter `6205`/`6220`-Ausgabe und JPG-Mehrfachanhaengen validieren; TOPCON CL300, TOPCON KR800S, TOPCON TRK2P und TOPCON CT1P als validierte Referenzkandidaten beibehalten, TOPCON Solos, TOPCON KR-1 und TOPCON CT800A praktisch abnehmen, H/V-Prisma sowie weitere SBJ-/KM-KRT-/Tonometrie-Funktionstestfaelle beobachten.
8. TOPCON CV5000 bidirektional weiter live testen: erzeugte CV-5000-Importdatei am Phoropter einlesen, Phoropter-Rueckgabe mit `Prescription` unter `6228` und `Full Correction` unter `6227` verarbeiten und besonders direkte Folgezyklen mit gleichnamiger neu geschriebener `Patient.gdt` sowie gleichnamig ueberschriebener/geaenderter Rueckgabedatei ohne Reset beobachten.
9. MEDISTAR + Dokumentanhang und MEDISTAR + Manuelle Dokumentuebergabe im Live-Test mit mehreren Anhangdateien, TopMost-Dialog, Zwischenablage/Dateiauswahl/Drag-&-Drop und pro-Datei-`6304`-Text pruefen.
10. Restliche E2E-Testfälle mit realen Testordnern ausführen und mit `docs/E2E_TESTPROTOKOLL_TEMPLATE.md` protokollieren.
11. Read-only Aktivierungsassistent nur als geparkten Regressionsstand weiterfuehren.
12. Lizenzsignatur- und Installer-Themen erst nach weiterem Profil-/Template-Nutzen priorisieren.

## 7. Risiken / offene Entscheidungen

- JSON vs SQLite: JSON ist aktuell einfach und transparent; SQLite kann später bei Historie, Audit und größerem Profilbestand sinnvoll werden.
- Windows-Dienst später ja/nein: aktuell bewusst nein; für echten Dauerbetrieb eventuell später erneut bewerten.
- FileSystemWatcher später ja/nein: aktuell periodischer Scan; ein Watcher könnte später ergänzen, aber nicht die Stabilitätsprüfung ersetzen.
- Lizenzsperre wann aktivieren: erst nach Signaturprüfung, Karenzzeitentscheidung und klarer Fehlerkommunikation.
- EV-Verknüpfung vs `6302`-`6305`-Link: Der strukturierte Link über `6302` bis `6305` wurde für MEDISTAR + NIDEK ARK1S praktisch validiert; andere AIS-Zielsysteme müssen separat geprüft werden.
- Mehrfachanhang-Zuordnung: mehrere stabile unterstuetzte Dateien im Anhang-Importordner werden jetzt einzeln uebergeben; spaetere Heuristiken anhand XML-Verweisen wie `PACHYImage` brauchen weiter sichere Patienten-/Auftragsbezüge.
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
