# Iststand und offene Punkte

Stand: 2026-05-23

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
- als V1 praxisabgenommene Geraeteanbindungsfenster im Tab `Verarbeitung`: Abdocken/Andocken ueber transparenten Symbolbutton `🗗`, Reset `↺` fuer den aktuellen Vorgang eines Schnittstellenprofils mit top-level Leerung der profilbezogenen Eingangsordner, Pin/TopMost `🔝` pro Floating-Fenster, Positionsmerken `📌`, dezente `-`/`+`-Intervallbuttons, sicheres X-Schliessen, Start-Wiederherstellung nach sicherer MainWindow-Anzeige, neue Geraetefenster-Standarddarstellung, automatische Oeffnung bei Monitoring-Aktivitaet, akustisches Signal nur bei Geraetedatei-Eingang, automatisches Zurueckandocken nach terminalem Abschluss und Systray-Grundfunktion fuer den Hintergrundbetrieb
- grafische Geraetefenster-Standarddarstellung aus dem TOPCON-CV5000-Pilot: kompakteres Layout, groesseres Geraetebild oder Platzhalter, linker Infoblock mit kurzen Monitoring-Displaylabels fuer lange Geraetetypen, Statuskugel mit gruenem Dateieingangs-Flash, Puls nur bei laufender Ueberwachung, linksbuendige Intervallsteuerung und `Letzter Scan`/`Automatik` im Detailbereich; leere Eingangskacheln zeigen bei laufender Ueberwachung `wartet` statt `gestoppt`. Neue UserDefined-Geraete koennen optional einen einfachen Geraetebildpfad speichern; bestehende Geraete koennen ueber `Geraet laden` ein lokales Bild bekommen, wobei BuiltIns nur lokale Bild-Overrides erhalten und fachlich unveraendert bleiben. `Geraet laden` und Monitoring nutzen dieselbe Bildaufloesung: Override, Profil-/BuiltIn-Bild, danach Platzhalter. Weitere BuiltIn-Bilder werden geraeteweise als App-Assets ergaenzt.
- AutoRedock haengt direkt am erfolgreichen Verarbeitungsabschluss: automatisch geoeffnete, nicht gepinnte Geraetefenster docken nach Erfolg wieder an; wiederholte Scan-Aktivitaet durch liegende Eingangsdateien schiebt den Countdown nicht endlos weiter. Gepinnte Fenster bleiben weiterhin abgedockt.
- AutoDetach ist fuer wiederholte Durchlaeufe generisch abgesichert: neue Dateiversionen mit gleichem Dateinamen loesen nach AutoRedock oder Reset erneut nur das betroffene Schnittstellenprofil-Fenster aus.
- Reset gibt auch den profilbezogenen Monitoring-Dedupe-State frei; nach `↺` und anschliessendem X-Andocken kann eine neu geschriebene AIS-Datei wieder automatisch nur das betroffene Floating-Fenster oeffnen.
- Datei- und Paketlogik fuer stabile AIS-/Geraete-/XDT-Anhangdateien
- neue kompakte Geraete-/Template-Matrix als Prioritaetenbasis
- offizielle Paketvorlage fuer MEDISTAR + NIDEK ARK1S inklusive reproduzierbarem Export-/Import-Testweg
- NIDEK AR360 / AR-360A als praktisch validierter Auto-Refraktometer-Workflow fuer XDT-Rueckgabe mit BuiltIn-Profilen, ARMedian-Ausgabe und selektivem Templatepaket-Pfad
- NIDEK LM7 / LM-7P als praktisch validierter Lensmeter-Referenzkandidat mit echter XML-Fixture, `Sphare`/`Sphere`-Toleranz, MEDISTAR-Lensmeter-Ausgabe, Reparatur alter persistierter BuiltIn-Exportpfade, MEDISTAR-Praxisprotokoll und selektivem Templatepaket-Test
- NIDEK NT530P / NT-530P als direkt nutzbarer testseitiger MEDISTAR-Kandidat mit echter UTF-16-XML-Fixture, `6220`-Pachymetrie, `6205`-Tonometrie, BuiltIn-Schnittstellenprofil, selektivem Templatepaket-Test und korrigiertem Nachlauf nach erfolgreichem Mehrfachanhang-Export
- TOPCON CL300 als erster praktisch validierter TOPCON-Lensmeter-Referenzkandidat mit echten XML-Fixtures, namespace-toleranter LM-Erkennung, `6228`-MedistarLine-Ausgabe inklusive ADD, PD und signierten H/V-Prismenkomponenten, BuiltIn-Schnittstellenprofil, selektivem Templatepaket-Test und Praxisprotokoll
- TOPCON Solos als fixture-validierter TOPCON-Lensmeter-Kandidat mit echter Solos-XML-Fixture, namespace-toleranter LM-Erkennung, `6228`-MedistarLine-Ausgabe inklusive PD und signierten H-Prismen, eigener BuiltIn-Schnittstellenprofilkette, selektivem Templatepaket-Test und Fixture-Protokoll; Transmission und PDF-Berichte sind mangels echter gefuellter Beispiele bewusst offen
- TOPCON KR800S als praktisch validierter Mehruntersuchungs-Referenzkandidat mit echten Shift-JIS-XML-Fixtures, namespace-toleranter REF/KM/SBJ-Erkennung, `6228`-Autorefraktion, `6221`-Keratometrie, konservativer `6227`-SBJ-Ausgabe mit getrennten Header-/Messwertzeilen, BuiltIn-Schnittstellenprofil, selektivem Templatepaket-Test, Praxisprotokoll und gezielter Reparatur alter persistierter BuiltIn-Exportprofile
- TOPCON KR-1 als fixture-validierter Keratorefraktometer-Kandidat mit echter Shift-JIS-XML-Fixture, namespace-toleranter REF-Erkennung, `6228`-Autorefraktion aus Medianwerten, eigener BuiltIn-Schnittstellenprofilkette, selektivem Templatepaket-Test und Fixture-Protokoll; KM/KRT/periphere KRT-Ausgabe bleibt bis zu einer echten KM/KRT-Fixture bewusst offen
- TOPCON TRK2P als praktisch validierter Mehruntersuchungs-Referenzkandidat mit echten XML-Fixtures inklusive TM/CCT-only-Teilmessung, namespace-toleranter REF/KM/TM/CCT-Erkennung, `6228`-Autorefraktion, `6221`-Keratometrie, `6220`-CCT/Pachy mit Header, mehrzeiliger `6205`-Tonometrie im NT530P-Stil, optionalem `6227`-SBJ-Pfad, BuiltIn-Schnittstellenprofil, selektivem Templatepaket-Test, Praxisprotokoll und gezielter Reparatur alter persistierter BuiltIn-Profile
- TOPCON CT1P als praktisch validierter Tonometrie-/Pachymetrie-Referenzkandidat mit echter Serial0214-XML-Fixture, namespace-toleranter TM-Erkennung, mehrzeiliger `6205`-Tonometrie, `6220`-Pachymetrie, per Auge optionalem CorrectedIOP/CCT, BuiltIn-Schnittstellenprofil, selektivem Templatepaket-Test und Praxisprotokoll; das Geraet misst fachlich vollautomatisch beidseitig, unvollstaendige CorrectedIOP-Bloecke in einer XML werden aber ohne leere Fragmente weggelassen.
- TOPCON CT800A als fixture-validierter Non-Contact-Tonometer-Kandidat mit drei echten CT-800A-XML-Fixtures, namespace-toleranter TM-Erkennung, `6205`-Tonometrie, vollstaendiger CorrectedIOP/CCT-Detailausgabe nur bei verwertbaren per-eye-Daten, eigener BuiltIn-Schnittstellenprofilkette, selektivem Templatepaket-Test und Fixture-Protokoll; 0070/0071 laufen trotz unvollstaendiger CorrectedIOP-/ERROR-Detailbloecke ohne leere Fragmente.
- TOPCON CV5000 / CV-5000S als erster bidirektionaler Phoropter-Kandidat: AIS-Historienparser fuer MEDISTAR-Karteikartenzeilen, Auswahlfenster `Werte an Phoropter uebergeben` im Verarbeitungslauf, CV-5000-Import-XML-Writer, CV-5000-Rueckgabeparser, fachlich getrennte Rueckgabe (`Prescription` vollstaendig ueber `6228`, `Full Correction` vollstaendig ueber `6227`, keine `6330`-Zeilen), BuiltIn-Schnittstellenprofil mit Ausgabe-an-Geraet-Konfiguration, selektiver Templatepaket-Test und Fixture-Protokoll sind vorhanden; das Geraeteprofil markiert nur die bidirektionale Faehigkeit, konkrete Ausgabeordner/Dateiname/Format liegen im Schnittstellenprofil. Das Auswahlfenster bleibt per `🔝`-Schalter standardmaessig im Vordergrund; `Nichts senden` schreibt keine Importdatei, ist kein Fehler und laesst den Workflow fuer die Phoropter-Rueckgabe offen. Der Rueckweg liest MEDISTAR-Historien-AIS-Dateien im CV-5000-Kontext tolerant; nach Phase 1 zaehlen die aktuell stabil und fachlich passend im Eingangsordner liegenden Phoropter-Rueckgabedateien, sodass gleichnamige Folgezyklen wie `Patient.gdt` ohne Reset moeglich sind.
- generischer AttachmentOnly-/Dokumentgeraete-V1-Kandidat `MEDISTAR + Dokumentanhang`: AIS-Datei plus stabile Dokumentdateien erzeugen je Datei eigene `6302`-`6305`-Linkfelder, ohne Messwertparser und ohne `6228`/`6205`/`6220`; `6304` enthaelt Benutzerbeschreibung oder Originaldateiname, `6305` bleibt der technische Pfad.
- Die Schnittstellenprofil-Konfiguration fuer AttachmentOnly ist kontextsensitiv reduziert: Der Geraete-/Dokument-Importordner ist der Dateieingang, separate XDT-Anhang-Importfelder und interne `6302`-`6305`-Templatefelder werden ausgeblendet, `Dokumentationstext erfassen` ist profilbezogen einstellbar, und der manuelle Dialog sammelt weitere stabile Dateien bis zum Klick auf `Uebertragen`.
- AttachmentOnly erzwingt die Anhangverarbeitung zur Laufzeit ueber den Dokument-Importordner, auch wenn alte gespeicherte technische Anhang-Flags deaktiviert sind. Dokumentgeraete erzeugen dadurch wieder je Datei `6302`/`6303`/`6304`/`6305`; eine AIS-only-XDT ohne Dokumentlinks wird fuer Dokumentanhaenge nicht mehr still als Erfolg ausgegeben.
- Die Texteingabe im Dialog `Dokumente uebertragen` ist vom Polling entkoppelt: offene Dialoge werden nicht neu aktiviert oder vollstaendig neu aufgebaut, neue Dateien werden inkrementell ergaenzt und die schwere automatische Paarverarbeitung wird bis zum Klick auf `Uebertragen` nicht bei jedem Scan erneut gestartet.
- `MEDISTAR + Manuelle Dokumentuebergabe` ist als eigener AttachmentOnly-Quellmodus vorhanden: Erst eine AIS-Datei startet den leeren `Dokumente uebertragen`-Dialog, Dateien werden per Drag-&-Drop, Dateiauswahl oder Datei-Zwischenablage hinzugefuegt, der Dialog ist standardmaessig TopMost, pro Datei wird `6304` aus Benutzertext oder Originaldateiname erzeugt, und manuell ausgewaehlte Quelldateien werden nicht geloescht. Nach `Uebertragen` wird der Dialog-/Confirmation-State freigegeben, sodass der naechste AIS-Vorgang wieder einen frischen leeren Dialog oeffnet.
- Dokumentanhang-Karten bleiben vor Vorgangsbeginn neutral; ein roter fehlender Anhang wird erst bei aktivem Paket beziehungsweise Timeout angezeigt.

Vorbereitet, aber noch nicht als produktiv abgenommen:

- fertige, auslieferbare ZIP-Templatepakete fuer vorbereitete und validierte Geraeteprofile; fuer ARK1S, AR360, LM7, CL300, Solos, KR800S, KR-1, TRK2P, CT1P und CT800A sind Referenz-/Kandidatendokumentation und temporaer erzeugte Export-/Import-Tests vorhanden, offizielle ZIP-Artefakte folgen erst nach Release-Regel
- UI-Einstellung fuer die Rueckdock-Zeit und sichtbarer Countdown-Hinweis fuer abdockbare Geraeteanbindungen
- weitere End-to-End-Testfaelle der automatischen AIS-/Geraete-/XDT-Anhang-Verarbeitung; ein Pflicht-Anhang-Praxislauf mit MEDISTAR + NIDEK ARK1S ist dokumentiert, weitere Faelle bleiben offen
- praktische MEDISTAR-Abnahme fuer den generischen Dokumentanhang-Workflow mit mehreren Dateien, Ruhezeit-/Bestaetigungsmodus und pro-Datei-`6304`-Text
- praktische MEDISTAR-Abnahme fuer die manuelle Dokumentuebergabe mit Drag-&-Drop/Dateiauswahl/Zwischenablage, PDF/JPG und sicherer Quell-Dateibehandlung
- praktische MEDISTAR-/CV-5000-Abnahme fuer die bidirektionale Phoropter-Anbindung: erzeugte `CVImport.xml` am CV-5000/CV-5000S weiter beobachten, den Pfad `Nichts senden` im echten Ablauf pruefen und direkte Folgezyklen mit gleichnamiger neu geschriebener `Patient.gdt` sowie ueberschriebener/geaenderter Phoropter-Rueckgabedatei ohne Reset beobachten; der MEDISTAR-Import der Rueckgabedatei mit `6228`-Prescription und `6227`-Full-Correction ist praktisch bestaetigt
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
- schlanke V1-Anlage fuer neue AIS-, Geraete- und Exportprofile als UserDefined; BuiltIns werden nicht ueberschrieben, es wird nichts automatisch aktiviert und keine Verarbeitung gestartet
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

Die Anlagebuttons im Tab `Profile & Templates` sind als schlanke V1 nutzbar. Neue AIS-Profile werden ueber Name, System und Codierung als UserDefined angelegt; Hilfetexte erklaeren MEDISTAR gegenueber `Generisch / anderes AIS` sowie Windows-1252 als typischen GDT/XDT-Fall. Neue Geraeteprofile werden ueber Name, Hersteller, Modell, Geraetetyp und vorhandene Parserbasis angelegt; Hilfetexte erklaeren Geraetetyp als fachliche Kategorie und Parserbasis als vorhandene Leselogik ohne neue Parser. `Neues Exportprofil anlegen` startet sichtbar einen leeren Entwurf, setzt einen eindeutigen Namen und schreibt erst bei bewusstem Speichern eine UserDefined-Kopie. Name-/ID-Konflikte werden blockiert, BuiltIns bleiben unveraendert und es werden keine Schnittstellenprofile automatisch geaendert.

UserDefined-Profile koennen als V1 gezielt umbenannt werden: AIS-, Geraete-, Export- und Schnittstellenprofile aendern dabei nur ihren sichtbaren Namen. Technische IDs, Referenzen, Exportregeln, Ordnerpfade, XDT-Anhang-Einstellungen und Aktivierungsstatus bleiben unveraendert. BuiltIn-Profile sind gegen Umbenennung gesperrt. Templatepaket-ZIP-Dateien werden nicht als App-Objekte verwaltet; ihre Benennung bleibt Dateiname/Release-Regel, waehrend Importvorschau-Zielnamen weiterhin in der bestehenden Vorschau bearbeitet werden.

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

Im Tab `Schnittstellenprofile` gibt es den Bereich `Pruefung vor Aktivierung` mit Gesamtstatus, Aktivierbarkeit, Zaehlern, strukturierter Ordnerpruefung, strukturierter XDT-Anhang-Konfiguration, Tabelle `Alle Pruefpunkte` sowie den Buttons `Pruefung aktualisieren` und `Aktivierung vorbereiten`. Die Pruefung nutzt die aktuellen UI-Entwurfswerte auch vor dem Speichern. Ordner-Erreichbarkeit wird separat als `Ja`, `Nein`, `Pfad fehlt` oder `Nicht geprueft` angezeigt; ein eingetragener, aktuell nicht erreichbarer Ordner ist Warnung/Hinweis, kein harter Blocker.

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
- unterstuetzte Typen wie PDF, JPG/JPEG, PNG, TIF/TIFF, DCM, TXT, XML, MP4, MP3 und WAV
- Stabilitaetspruefung vor Verarbeitung
- automatische Auswahl eines oder mehrerer stabiler unterstuetzter Anhaenge in stabiler Dateinamen-Reihenfolge
- Copy/Move-Transfer mit Kollisionsschutz
- Aufbau externer AIS-Linkfelder:
  - `6302` Dokumentenname
  - `6303` Dateiformat
  - `6304` Beschreibung optional
  - `6305` vollstaendiger Dateipfad
- je erfolgreichem Anhang eine eigene `6302`/`6303`/optional `6304`/`6305`-Feldgruppe
- zentrale Laengenpraefix-Erzeugung ueber `XdtExportBuilder`
- AttachmentOnly-/Dokumentgeraete-Modus: Dokumentdateien gelten als Geraeteeingang, werden aber nur als Anhaenge uebergeben; XML ist dort kein Messwert-XML.

Instabile Anhaenge werden nicht verschoben, verlinkt oder exportiert. Wenn mehrere Kandidaten vorhanden sind und mindestens einer noch instabil ist, wartet die Paketlogik bis zur Stabilitaet beziehungsweise bis zum bestehenden Timeout.

### Paket-Wartelogik

Die Paketlogik ist zweistufig:

1. Eine stabile AIS-Datei wartet auf eine stabile Geraetedatei.
2. Erst nach vollstaendigem AIS-/Geraete-Paar beginnt die XDT-Anhang-Wartezeit.

Weitere Regeln:

- `DeviceFileWaitTimeoutMinutes`, Standard `10`
- neuere AIS-Datei ersetzt aeltere wartende AIS-Datei
- abgelaufene AIS-Auftraege erzeugen keinen Export
- optionaler XDT-Anhang: mehrere stabile Anhaenge werden uebernommen; nach Timeout ohne stabile Anhaenge Export ohne Anhang
- verpflichtender XDT-Anhang: nach Timeout Blockade/Fehler
- mehrere vorhandene, aber noch instabile Anhaenge: warten; nach Timeout greift die optionale beziehungsweise verpflichtende Anhangregel
- nach erfolgreichem Export mit Anhaengen wird das Paket terminal abgeschlossen; die Monitoring-Karte wird auf den naechsten Vorgang zurueckgesetzt und zeigt keinen alten Timeout-Status fuer bereits uebertragene Anhaenge.

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
- Archivmodus `Copy` oder `Move`; ist die Profiloption `aus Importordner entfernen` aktiv, werden genau diese bekannten verarbeiteten AIS-/Geraetedateien aus dem Importordner entfernt beziehungsweise ins Archiv verschoben.
- Fehlerablage verschiebt bekannte betroffene Dateien in den Fehlerordner und schreibt `error.txt`, wenn ein Fehler terminal behandelt werden muss.
- Unbekannte Dateien werden nicht geloescht oder verschoben.
- Exportordner werden nicht pauschal bereinigt.
- Eine fruehere Verarbeitung, ein gleicher Dateiname, gleicher Pfad, gleicher Fingerprint oder aelterer Zeitstempel blockiert die automatische Verarbeitung nicht mehr. Entscheidend ist, ob die Dateien jetzt im konfigurierten Eingangsordner liegen, stabil lesbar sind und fachlich zum Profil passen.
- Doppelexporte werden organisatorisch ueber den konfigurierten Nachlauf vermieden: Archiv, Verschieben oder Entfernen betrifft nur bekannte im aktuellen Vorgang verarbeitete AIS-/Geraetedateien; ohne solche Profilregel bleiben Dateien liegen und koennen erneut verarbeitet werden.

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
- Mehrere XDT-Anhaenge nur als einzelne stabile unterstuetzte Dateien mit eigenen Linkfeldgruppen uebergeben.
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
| XDT-Mehrfachanhaenge | testseitig validiert, inklusive Paketabschluss/Nachlauf | `AttachmentAutoCandidateSelectionServiceTests`, `AttachmentPackageDecisionServiceTests`, `AutoImportPairProcessingCoordinatorTests`, `InterfaceMonitoringCardStatusServiceTests` |
| AttachmentOnly-/Dokumentgeraete-V1 | testseitig validiert | `ImportFileClassifierTests`, `AttachmentImportFolderScannerServiceTests`, `AutoImportScannerServiceTests`, `InterfaceProfileManualProcessorTests`, `MedistarDocumentAttachmentTemplatePackageTests` |
| Baukasten-Testexport mit simuliertem 6305-Zielpfad | testseitig validiert | `BuilderTestExportServiceTests` |
| Automatische Paketlogik AIS -> Geraet -> XDT-Anhang | testseitig validiert | `AutoImportPackageStateServiceTests`, `AutoImportPairProcessingCoordinatorTests`, `AttachmentPackageDecisionServiceTests` |
| Templatepaket-Importpipeline bis UserDefined-Uebernahme | E2E-nah testseitig validiert | `TemplatePackageImportEndToEndTests` und zugehoerige Service-Tests |
| ARK1S-Referenzpaket Export/Import | reproduzierbar testseitig validiert, inklusive UI-nahem Importvorschau-Pfad und sicherem Konfliktstandard `Ueberspringen` | `MedistarNidekArk1sTemplatePackageTests` |
| AR360-Referenzpaket Export/Import | reproduzierbar testseitig validiert, inklusive selektivem Paketinhalt, Importvorschau, DryRun und sicherer UserDefined-Kopie | `MedistarNidekAr360TemplatePackageTests` |
| LM7-Templatepaket-Kandidat Export/Import | reproduzierbar testseitig validiert; echte LM7-XML-Fixture, Stylesheet-Ignoriertest, MEDISTAR-Lensmeterzeilen, Reparatur alter persistierter BuiltIn-LM7-Exportpfade, praktische MEDISTAR-Abnahme und selektiver Paketinhalt sind abgesichert | `docs/E2E_TESTPROTOKOLL_MEDISTAR_LM7.md`, `NidekLm7ProfileTests`, `ProfileCatalogServiceTests`, `MedistarNidekLm7TemplatePackageTests` |
| Abdockbare Geraeteanbindungsfenster V1 | praktisch abgenommen und testseitig fuer State, Persistenz, verzoegerte Wiederherstellung, wiederholtes Auto-Abdocken nach neuen Dateiversionen sowie nach Reset + X-Andocken, Auto-Zurueckandocken, akustisches Signal nur bei Geraetedatei-Eingang, Systray-Fensterzustand und sicheren Vorgangsreset inklusive profilbezogener Eingangsordner-Leerung abgesichert | `docs/PRAXISABNAHME_GERAETEFENSTER_V1.md`, `InterfaceProfileFloatingWindowStateServiceTests`, `InterfaceProfileFloatingWindowStateRepositoryTests`, `InterfaceProfileFloatingWindowRestoreGateTests`, `InterfaceProfileAutoDetachServiceTests`, `InterfaceProfileAutoRedockServiceTests`, `InterfaceProfileNotificationSoundServiceTests`, `TrayWindowStateServiceTests`, `InterfaceProfileMonitoringResetServiceTests`, `InterfaceProfileInputFolderResetServiceTests` |
| BuiltIn/UserDefined-Schutz | testseitig validiert | `ProfileCatalogServiceTests`, TemplateImport-Tests |

Teilweise praktisch abgeschlossen ist die manuelle Praxisabnahme fuer MEDISTAR + NIDEK ARK1S + XDT-Anhang-Link: Pflicht-Anhang vorhanden, Pflicht-Anhang fehlt/Fehlerfall und MEDISTAR-Livesystem-Linkaufruf sind bestanden. Noch offen ist die vollstaendige manuelle Praxisabnahme der weiteren Faelle aus `docs/END_TO_END_TESTPLAN.md`. Fuer die weitere Durchfuehrung steht die ausfuellbare Vorlage `docs/E2E_TESTPROTOKOLL_TEMPLATE.md` bereit.

## 4. Vorbereitet, aber noch nicht produktiv validiert

- NIDEK AR360 / AR-360A: Auto-Refraktor-XDT-Rueckgabe praktisch validiert; Referenzpaket-Export/Import testseitig abgesichert; XDT-Anhangfall und offizielles ZIP-Artefakt offen
- NIDEK LM7/LM7P: Lensmeter-XDT-Rueckgabe praktisch in MEDISTAR validiert; echte XML-Fixture und Templatepaket-Kandidat testseitig vorbereitet; weitere Prisma-/PD-Dateien und offizielles ZIP-Artefakt offen
- NIDEK NT530P: echte XML-Fixture, Parserwerte, korrigierter mehrzeiliger `6205`-/`6220`-Export mit Headern und Templatepaket-Kandidat testseitig vorhanden; praktische MEDISTAR-Abnahme offen
- MEDISTAR + Dokumentanhang: AttachmentOnly-V1 mit optionalem `6227`, mehreren `6302`-`6305`-Anhaengen und Templatepaket-Kandidat testseitig vorhanden; praktische MEDISTAR-Abnahme offen
- TOPCON CL300: echte XML-Fixtures, `6228`-Lensmeter-Ausgabe, Templatepaket-Kandidat und praktische MEDISTAR-Abnahme vorhanden; offen bleiben H/V-Prisma-Beobachtung und offizielles ZIP-Artefakt
- TOPCON KR800S: echte Shift-JIS-XML-Fixtures, REF-`6228`, KM-`6221`, konservative SBJ-`6227`-Ausgabe mit getrennten Full-Correction-Header-/Messwertzeilen, Templatepaket-Kandidat und praktische MEDISTAR-Abnahme vorhanden
- TOPCON TRK2P: echte Serial0001-/Serial0135-/Serial1165-Fixtures, REF/KM/TM/CCT-Fallback, TM/CCT-only-Teilmessung, Templatepaket-Kandidat und praktische MEDISTAR-Abnahme vorhanden; offen bleiben echte SBJ-Praxisfaelle und offizielles ZIP-Artefakt
- TOPCON CT1P: echte Serial0214-Fixture, Tonometrie `6205`, Pachymetrie `6220`, per Auge optionales CorrectedIOP/CCT, BuiltIns, Templatepaket-Kandidat und praktische MEDISTAR-Abnahme vorhanden; offen bleibt eine echte beidseitige CorrectedIOP/CCT-Datei
- TOPCON CT800A: drei echte CT-800A-TM-Fixtures, Tonometrie `6205`, vollstaendige CorrectedIOP/CCT-Detailausgabe nur bei verwertbaren Daten, BuiltIns, Templatepaket-Kandidat und Fixture-Protokoll vorhanden; offen bleibt die praktische MEDISTAR-Abnahme
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
| Profil-Assistent | Roadmap fuehrt Assistent als offen. | Schlanke V1-Anlage fuer AIS-, Geraete- und Exportprofile als UserDefined ist vorhanden; kein vollstaendiger Assistent und keine Aktivierung. | passt | Geraete-Datei-Explorer als naechsten vorbereitenden Schritt planen. |
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
| hoch | Fertige Geraeteprofile und Templatepakete | BuiltIn-Geraeteprofile und MEDISTAR-Exportprofile sind fuer die zentralen Geraete beziehungsweise Workflows vorhanden; praktisch validiert sind MEDISTAR + NIDEK ARK1S, die AR360-Auto-Refraktor-XDT-Rueckgabe, die LM7-Lensmeter-XDT-Rueckgabe, TOPCON CL300, TOPCON KR800S, TOPCON TRK2P inklusive TM/CCT-only-Teilmessung und TOPCON CT1P. NT530P, Solos, KR-1 und CT800A sind testseitig mit echten XML-Fixtures, korrigierter MEDISTAR-Ausgabe und selektivem Templatepaket-Kandidaten direkt nutzbar. Matrix, Referenz-/Kandidatendokumentation, selektiver Export, reproduzierbare Export-/Import-Tests und stabilisierter App-Preview-Pfad sind vorhanden. | Dauerhafte ARK1S-/AR360-/LM7-/CL300-/KR800S-/KR1-/TRK2P-/CT1P-/CT800A-ZIP-Release-Artefakte, AR360-XDT-Anhangfall, weitere LM7-Prisma-/PD-Faelle und die praktische NT530P-/Solos-/KR1-/CT800A-Abnahme fehlen. Fuer CL300 bleibt die H/V-Prisma-Darstellung weiter zu beobachten; fuer KR800S/TRK2P bleiben weitere SBJ-/Funktionstestfaelle zu beobachten; fuer KR-1 fehlen echte KM/KRT-/periphere-KRT-Dateien; fuer CT1P fehlt eine echte beidseitige CorrectedIOP/CCT-Fixture. | Release-Regel fuer ARK1S/AR360 anwenden, LM7-Prisma-/PD-Beispielfaelle sammeln, NT530P in MEDISTAR praktisch mit `6220`/`6205` abnehmen, Solos/KR-1/CT800A praktisch abnehmen und CL300-/KR800S-/TRK2P-/CT1P-Praxisprotokolle als Referenz halten. | Anwender muessen sonst weiterhin den Baukasten nutzen. | BuiltIn-Profile, TemplatePackageExporter, Testdaten |
| mittel | Templatepaket-Import Aktivierungsassistent | Sicherer Import als UserDefined vorhanden; Backend-Bewertung, UI-Pruefvorschau, vorbereitende Aktivierungsvorschau und Guard-Schicht sind read-only vorhanden. | Bewusste Benutzerfreigabe und eigentliche Aktivierung fehlen weiterhin. | Vorerst parken; nur als Sicherheits-/Regressionsstand beibehalten. | Importierte Profile bleiben sicher, aber noch nicht per finalem V1-Klick aktivierbar. | Profilkatalog, Evaluation, Guard |
| niedrig | Geraeteanbindungsfenster V1 ausbauen | V1 ist praktisch abgenommen: Systray, manuelles Abdocken/Andocken, Auto-Oeffnung auch bei spaeteren Durchlaeufen mit gleichem Dateinamen und neuer Dateiversion sowie nach Reset + X-Andocken, Auto-Zurueckandocken, Signalton nur bei Geraetedatei, Reset, Pin/TopMost, Positionsmerken, Radar und Buttonlayout funktionieren profilgetrennt. | Komfortthemen fehlen bewusst: Autostart, Windows-Dienst, UI-Einstellung fuer Rueckdock-Zeit, sichtbarer Countdown-Hinweis, UI-Schalter fuer Signalton und ggf. eigenes Systray-Icon. | V1 beibehalten; Komfortthemen nur nach weiterem Praxisfeedback priorisieren. | Reset leert nur die AIS-/Geraete-/optionalen XDT-Anhang-Eingangsordner des gewaehlten Schnittstellenprofils top-level; Export-, Archiv- und Fehlerordner sowie Unterordner bleiben unangetastet. | `docs/PRAXISABNAHME_GERAETEFENSTER_V1.md`, Verarbeitungstab, Monitoring-Karten |
| hoch | Lizenzsignatur | Lizenzanzeige und Karenzzeitmodell vorhanden. | Digitale Signaturpruefung, Schluesselmodell, Manipulationsschutz. | Signaturformat und Validierungsservice spezifizieren. | Lizenzdateien sind vor produktiver Sperre nicht ausreichend gesichert. | Lizenzmodell, Supportprozess |
| mittel | ReplaceExisting fuer UserDefined | Bewusst deaktiviert; Executor blockiert/ueberspringt. | Konfliktloesungsdialog, Sicherung/Backup, explizite Benutzerentscheidung. | Erst nach konkreter Profil-/Templatepaket-Arbeit als separates Import-Epic planen. | Anwender muessen Konflikte ueber Kopien loesen, Katalog kann wachsen. | Importvorschau, SelectionService |
| mittel | Manuelle Zielnamen-/ID-Bearbeitung im Templateimport | Automatische Kopienamen vorhanden. | UI zum Bearbeiten vorgeschlagener Namen/IDs. | Nur ergaenzen, wenn Anwenderfeedback Bedarf zeigt. | Importnamen koennen weniger sprechend sein. | Importplan, SelectionService |
| mittel | Geraete-Datei-Explorer | Noch kein vollstaendiger Explorer. | Datei anzeigen, SourcePaths untersuchen, Messwerte markieren, Kandidaten fuer Exportregeln uebernehmen. | Kleinen read-only Explorer fuer XML/Geraetedateien bauen. | Neue Geraeteprofile bleiben Codex-/Entwickleraufgabe. | XmlDeviceParser, PlaceholderDisplayHelper |
| mittel | Profil-Assistent fuer unbekannte Geraete | Schlanke V1-Anlage fuer AIS-, Geraete- und Exportprofile als UserDefined ist vorhanden. | Gefuehrtes Erstellen kompletter Geraete-/Export-/Schnittstellenpakete inklusive Datei-Explorer und Messwertuebernahme fehlt. | Nach Geraete-Datei-Explorer planen. | Skalierung auf neue Geraete bleibt weiterhin teilweise Entwickler-/Codex-Aufgabe. | Geraete-Datei-Explorer, ProfileCatalog |
| mittel | NIDEK LM7/LM7P produktiv validieren | Echte LM7-XML-Fixture, Parseralias fuer `Sphare`/`Sphere`, MEDISTAR-Lensmeter-Ausgabe, BuiltIn-Schnittstellenprofil, Templatepaket-Kandidat, Reparatur alter persistierter BuiltIn-Exportpfade und praktische MEDISTAR-Abnahme sind vorhanden. | Weitere Prisma-/PD-Dateien, separat validierter XDT-Anhang-Link und offizielles ZIP-Artefakt fehlen. | Prisma-/PD-Beispielfaelle sammeln und danach ueber ZIP-Release nach Regel entscheiden. | Vorbereitetes Profil koennte bei Prisma/PD-Sonderfaellen noch abweichen. | Testdaten, MEDISTAR-Anforderungen |
| mittel | NIDEK NT530P produktiv validieren | Echte UTF-16-XML-Fixture, Parserwerte, Header-/Einzelzeilen fuer `6205` Tonometrie und `6220` Pachymetrie, BuiltIn-Geraete-/Export-/Schnittstellenprofil, selektiver Templatepaket-Test, Mehrfachanhang-Linkfelder und korrigierter Nachlauf/Monitoring-Reset sind vorhanden. | Praktische MEDISTAR-Nachpruefung fuer den kompletten JPG-Mehrfachanhanglauf nach Nachlauf-Fix fehlt. | Geraetespezifischen E2E-Testlauf mit MEDISTAR erneut durchfuehren und protokollieren. | Karteikarten-Darstellung, Ordnernachlauf oder Attachment-Erwartung koennte fachlich abweichen. | Testdaten, MEDISTAR-Anforderungen |
| mittel | Dokumentgeraete produktiv validieren | BuiltIn `MEDISTAR + Dokumentanhang`, AttachmentOnly-Modus, kontextsensitive Konfiguration, profilbezogene Option `Dokumentationstext erfassen`, pro-Datei-Beschreibung ueber `6304`, mehrere `6302`-`6305`-Linkfeldgruppen, Sammeln bis `Uebertragen` und selektiver Templatepaket-Test sind vorhanden. Der Livefehler mit fehlenden Linkfeldern bei alten deaktivierten Anhang-Flags ist korrigiert. | Praktischer MEDISTAR-Test mit PDF/JPG/XML/Medienanhaengen, pro-Datei-Beschreibung und realistischen Copy/Move-Zielordnern fehlt. | Live-Test mit mehreren stabilen Dokumentdateien protokollieren; dabei Ruhezeit- und manuellen Bestaetigungsmodus, Textoption an/aus und 6302-6305-Ausgabe pruefen. | Dokumentgeraete bleiben ohne Praxisabnahme ein technischer Kandidat. | Testanhaenge, MEDISTAR-Anforderungen |
| mittel | TOPCON CL300 als Referenzkandidat halten | Echte Serial0001-/Serial1521-XML-Fixtures, Namespace-/LM-Erkennung, BuiltIn-Geraete-/Export-/Schnittstellenprofil, `6228`-MedistarLine-Ausgabe mit ADD, PD und signierten H/V-Prismenkomponenten, Templatepaket-Kandidat und praktische MEDISTAR-Abnahme sind vorhanden. | Offizielles ZIP-Artefakt fehlt; die H/V-Prisma-Darstellung sollte in weiteren MEDISTAR-Anzeigen beobachtet werden. | Praxisprotokoll beibehalten, H/V-Prisma bei weiteren CL300-Faellen pruefen und danach ZIP-Release nach Regel entscheiden. | MEDISTAR-Anzeige der H/V-Prismenkomponenten koennte fachlich nachjustiert werden muessen. | Testdaten, MEDISTAR-Anforderungen |
| mittel | TOPCON Solos praktisch abnehmen | Echte SolosExportSample-Fixture, Namespace-/LM-Erkennung, BuiltIn-Geraete-/Export-/Schnittstellenprofil, `6228`-MedistarLine-Ausgabe mit PD und signierten H-Prismen, Templatepaket-Kandidat und Fixture-Protokoll sind vorhanden. | Praktische MEDISTAR-Abnahme, echte Transmission-Fixture und PDF-Berichtablauf fehlen. | Solos-Liveimport in MEDISTAR pruefen, Transmission-/PDF-Beispiele sammeln und erst dann MEDISTAR-Zielregel fuer diese Zusatzwerte festlegen. | Schema enthaelt mehr optionale Felder als die aktuelle Fixture; Transmission darf nicht ohne echte Daten geraten werden. | Testdaten, MEDISTAR-Anforderungen |
| mittel | TOPCON KR800S als Referenzkandidat halten | Echte Shift-JIS-Fixtures, namespace-tolerante REF/KM/SBJ-Erkennung, `6228`/`6221`/`6227`-Export, getrennte SBJ-Header-/Messwertzeilen, BuiltIns, Templatepaket-Test, Repair alter persistierter BuiltIn-Exportregeln und Praxisprotokoll sind vorhanden. | Offizielles ZIP-Artefakt fehlt; weitere SBJ-/Funktionstestfaelle koennen fachlich verfeinert werden. | Praxisprotokoll beibehalten, weitere SBJ-Faelle sammeln und danach ZIP-Release nach Regel entscheiden. | Subjektive `6227`-Zeilen koennten nach Praxisfeedback feiner formuliert werden. | Testdaten, MEDISTAR-Anforderungen |
| mittel | TOPCON KR-1 praktisch abnehmen | Echte Shift-JIS-Serial0001-Fixture, namespace-tolerante REF-Erkennung, `6228`-Median-Ausgabe, BuiltIns, Templatepaket-Test und Fixture-Protokoll sind vorhanden. | Praktische MEDISTAR-Abnahme und echte KM/KRT-/periphere-KRT-Fixture fehlen. | KR-1-Liveimport in MEDISTAR pruefen; sobald echte KM/KRT-Datei vorliegt, `6221`-Zielbild fachlich festlegen und testen. | Ohne KM/KRT-Fixture duerfen keine Keratometerwerte erfunden werden. | Testdaten, MEDISTAR-Anforderungen |
| mittel | TOPCON TRK2P als Referenzkandidat halten | Echte Serial0001-/Serial0135-/Serial1165-Fixtures, namespace-tolerante REF/KM/TM-Erkennung, CCT-Fallback aus CorrectedIOP und TM/CCT-only-Teilimport, `6228`/`6221`/`6220`/`6205`-Export mit `Tonometrie`-/`Pachymetrie`-Headern und mehrzeiliger `6205`-Anzeige, BuiltIns, Repair alter persistierter BuiltIns, Templatepaket-Test und praktische MEDISTAR-Abnahme sind vorhanden. | SBJ ist in den aktuellen Fixtures nicht enthalten; offizielles ZIP-Artefakt fehlt. | Praxisprotokoll beibehalten, echte SBJ-Faelle sammeln und danach ueber ZIP-Release nach Regel entscheiden. | SBJ-Ausgabe koennte nach weiteren Praxisdateien fachlich verfeinert werden muessen. | Testdaten |
| mittel | TOPCON CT1P als Referenzkandidat halten | Echte Serial0214-Fixture, namespace-tolerante TM-Erkennung, `6205`-Tonometrie, `6220`-Pachymetrie, per Auge optionales CorrectedIOP/CCT, BuiltIns, Templatepaket-Test und praktische MEDISTAR-Abnahme sind vorhanden. | Weitere Datei mit vollstaendigem linkem CorrectedIOP/CCT fehlt. | Falls eine Datei mit vollstaendigen rechten und linken CorrectedIOP-/CCT-Werten vorliegt, als zweite Fixture ergaenzen und beidseitige `PR`/`PL`-/`RA`/`LA`-Ausgabe fest absichern. | Herstellerdateien mit anderem Partialmuster koennten Nachjustierung erfordern. | Testdaten |
| mittel | TOPCON CT800A praktisch abnehmen | Drei echte CT-800A-Fixtures, namespace-tolerante TM-Erkennung, `6205`-Tonometrie, vollstaendige CorrectedIOP/CCT-Detailzeilen nur bei verwertbaren Daten, BuiltIns, Templatepaket-Test und Fixture-Protokoll sind vorhanden. | Praktische MEDISTAR-Abnahme und weitere Einseitenfaelle fehlen. | CT-800A-Liveimport mit MEDISTAR pruefen; 0069-artige vollstaendige CorrectedIOP/CCT-Daten und 0070/0071-artige Teilwerte beobachten. | MEDISTAR-Anzeige der `6205`-Detailzeilen koennte praktisch nachjustiert werden muessen. | Testdaten, MEDISTAR-Anforderungen |
| niedrig | AIS-/MEDISTAR-Exporttemplate-Default-Konzept | Bewusst zurueckgestellt; nach Reset nicht implementiert. | Neues Fachkonzept. Bis dahin keine Modelle, Services, UI, 6330-Automatik oder Codex-Umsetzungsauftraege dazu. | Nicht implementieren; fachliche Neubewertung ausserhalb des aktuellen Entwicklungsstrangs abwarten. | Anwender pflegen Exportregeln weiter manuell im Baukasten. | Neues Fachkonzept, MEDISTAR-Fachabstimmung |
| mittel | Weitere AIS-Profile ausser MEDISTAR | Nicht produktiv umgesetzt. | AIS-spezifische Parser, Feldkennungen, Exportregeln und Tests. | Erst nach MEDISTAR-Kern stabilisieren und Bedarf priorisieren. | App bleibt MEDISTAR-zentriert. | AIS-Dokumentation |
| mittel | SQLite vs. JSON | Code nutzt JSON; Pflichtenheft nennt SQLite als Ziel. | Bewusste Architekturentscheidung und ggf. Migrationsplan. | ADR erstellen: JSON fuer Prototyp beibehalten oder SQLite planen. | Doku und Architekturziele bleiben uneindeutig. | Profilkatalog, Deployment |
| mittel | Installer / Deployment | Offen. | Installer, Updateprozess, AppData-Pfade, Support-/Rollback-Anleitung. | Nach Praxis-E2E einen ersten MSIX/Setup-Plan erstellen. | Installation bleibt Entwickler-/Handarbeit. | Versionierung, Signatur, Support |
| mittel | Technische Installations-/Supportdoku | Teilweise vorhanden. | Klare Anleitung fuer Ordner, Berechtigungen, Backup, Logs, Fehleranalyse. | `docs/INSTALLATION_SUPPORT.md` planen. | Praxiseinrichtung wird fehleranfaellig. | Deployment-Konzept |
| mittel | Testdaten/Sample-Dateien | Tests nutzen interne Beispiele, manuelle Testdaten nicht voll paketiert. | Synthetische AIS-, Geraete- und Anhangdateien fuer E2E. | Testdatenverzeichnis mit synthetischen, patientenfreien Dateien definieren. | Manuelle Reproduktion bleibt schwierig. | Datenschutz, E2E-Testplan |
| mittel | Mehrfachanhang-Zuordnung | Mehrere stabile unterstuetzte Dateien werden testseitig einzeln uebertragen und mit eigenen `6302`-`6305`-Gruppen exportiert. | Praktische MEDISTAR-Abnahme und ggf. geraetespezifische Heuristiken anhand XML-Verweisen fehlen. | Mehrfachanhang-Praxislauf mit zwei JPGs protokollieren; danach entscheiden, ob `PACHYImage`-Verweise fuer NT530P fachlich ausgewertet werden sollen. | Reihenfolge, Pflichtfall und Anzeige muessen im Livebetrieb bestaetigt werden. | Patientenbezug, Praxisablauf |
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
- NIDEK LM7/LM7P als praktisch validierten Referenzkandidaten beibehalten; Prisma-/PD-Beispielfaelle und offizielles ZIP-Artefakt bleiben offen.
- NIDEK NT530P als naechsten praktischen MEDISTAR-Testkandidaten mit korrigiertem `6205`/`6220`-Layout und optionalem JPG-Mehrfachanhang abnehmen; TOPCON CT1P als validierten `6205`/`6220`-Referenzkandidaten halten; TOPCON KR-1 mit `6228` und TOPCON CT800A mit `6205` praktisch abnehmen; TOPCON CL300, KR800S und TRK2P als validierte Referenzkandidaten halten.
- Dokumentgeraete-V1 mit AIS-Datei, mehreren Anhaengen und pro-Datei-`6304`-Text praktisch in MEDISTAR pruefen.
- Baukasten schlank halten; keine neue Assistentenarchitektur, solange fertige Profile und Pakete fehlen.

### Phase 4: Geparkter Aktivierungsassistent

- Read-only Aktivierungsassistent ist praktisch in der UI sichtgeprueft und fuer den Vorschau-Status abgenommen; Pruefung, Guard und V1-Preview bleiben ohne produktive Wirkung.
- Produktive Aktivierung vorerst nicht weiter ausbauen.
- `docs/UI_PRUEFPROTOKOLL_AKTIVIERUNGSASSISTENT.md` als Regressionscheck fuer spaetere UI-Aenderungen weiterfuehren.
- AIS-/MEDISTAR-Default-Exporttemplates nicht umsetzen, bis ein neues Fachkonzept vorliegt.

### Phase 5: Geraete-Datei-Explorer / Profil-Assistent

- Read-only Explorer fuer Geraetedateien und SourcePaths.
- Uebernahme erkannter Messwerte in Exportregel-Entwurf.
- Die schlanke V1-Profilanlage ist vorhanden; offen bleibt ein gefuehrter Assistent fuer komplette neue Geraete mit Messwertuebernahme.

### Phase 6: Weitere Geraete validieren

- LM7/LM7P-Prisma-/PD-Sonderfaelle
- NT530P
- TOPCON CL300 H/V-Prisma in weiteren Praxisfaellen beobachten
- TOPCON KR800S als praktisch validierten REF/KM/SBJ-Referenzkandidaten halten
- TOPCON KR-1 als fixture-validierten REF-`6228`-Kandidaten praktisch abnehmen und echte KM/KRT-/periphere-KRT-Datei sammeln
- TOPCON TRK2P als praktisch validierten REF/KM/TM/CCT-Referenzkandidaten halten und echte SBJ-Faelle sammeln
- TOPCON CT1P als praktisch validierten Tonometrie-/Pachymetrie-Referenzkandidaten halten und echte beidseitige CorrectedIOP/CCT-Datei sammeln
- TOPCON CT800A als fixture-validierten Non-Contact-Tonometer-Kandidaten praktisch in MEDISTAR abnehmen

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
- danach weitere LM7/LM7P-Prisma-/PD-Beispielfaelle sammeln und ueber ein offizielles ZIP-Artefakt entscheiden
- danach Build und Tests ausfuehren

Das ist der beste naechste Schritt, weil es den Baukasten vom Normalweg zum Rueckfallwerkzeug macht und den validierten Kernworkflow in ein wiederverwendbares Anwenderpaket ueberfuehrt.

## 10. Keine Produktivlogik-Aenderungen

Dieser Abgleich beschreibt den aktuellen Stand. Produktivverarbeitung und Aktivierungslogik bleiben unveraendert; die validierten ARK1S-/AR360-BuiltIns bleiben geschuetzt. Fuer LM7 wurde das vorbereitete BuiltIn gezielt zum praktisch validierten Lensmeter-Referenzkandidaten ergaenzt. Ergaenzt wurden reproduzierbare Templatepaket-Wege sowie schlanke UI-Funktionen fuer sichere UserDefined-Wartung im Profilbereich.
