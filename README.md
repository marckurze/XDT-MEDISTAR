# XdtDeviceBridge

XdtDeviceBridge ist ein lokaler Windows-Prototyp fuer die dateibasierte Anbindung von Untersuchungsgeraeten an ein Arztinformationssystem ueber GDT/XDT-Dateien. Der Produktname fuer den spaeteren Einsatz ist XDTBox; Kunden-App und grafisches Hersteller-Lizenztool verwenden das offizielle XDTBox-Icon sowie das neue Logo-/Schriftzug-Branding.

In der Kunden-App startet die periodische Überwachung beim Öffnen der App standardmaessig automatisch fuer aktive Schnittstellenprofile; dieses Verhalten ist ueber das Zahnrad in der Tab-Zeile konfigurierbar. Es gibt weiterhin keinen Windows-Dienst, keinen Windows-Autostart und keinen FileSystemWatcher. XDTBox bringt ausserdem einen Tab `Sicherung/Umzug` fuer lokale Konfigurationssicherungen im Format `.xdtboxbackup` sowie ein lokales Hilfe-/Info-Menue direkt neben den Tabs mit.

Der aktuelle Fokus liegt auf stabilen XDTBox-Praxisanbindungen, dem XDT-Baukasten und der ersten offiziellen Windows-Installer-Version.

Aus einem statisch ausgewerteten Referenzdatenbestand wurden 100 Geraeteordner geprueft. Als erste vollstaendig abgesicherte neue BuiltIns sind NIDEK ARK-510A, NIDEK ARK-560A und NIDEK LM-1800P hinzugekommen; der NIDEK-Restbatch ergaenzt AR-1, AR-1S, AR-310A, LM-1800PD, NT-1, NT-1E, NT-1P, NT-510 und NT-530 als testseitig abgesicherte XML-Varianten der vorhandenen NIDEK-AR-/Lensmeter-/Tono-Pachy-Familien. TOPCON KR-800 wird als Alias der bestehenden KR800S-XML-Familie behandelt. Der TOPCON-Restbatch ergaenzt CL-300PDL, RM-800 und TRK-3 Omnia als testseitig abgesicherte Varianten der vorhandenen TOPCON-XML-Familien. Aus der Huvitz-Referenzlogik sind zusaetzlich HRK-8000A, HRK-9000A, HNT-1P und HTR-1A als serielle Text-BuiltIns vorbereitet. Der TOMEY-Batch ergaenzt CF-2000, TL-2000C, TL-6000, TL-7000, MR-6000, TOP-1000, EM-3000 und EM-4000 mit isolierten Parsern fuer Lensmeter-, REF/KM-, Tonometrie-, Pachymetrie- und Endothel-/Zellmesswerte. Der Shin-Nippon-Batch ergaenzt Accuref R-800, Accuref K-900, DL-1000, DL-800, DL-900, NCT-200 und SLM-4000 als serielle Text-BuiltIns fuer REF, KM, Lensmeter und Tonometrie. Reichert 7CR NCT, Reichert LensChek Plus und Rodenstock CX 800 sind als weiterer Referenzbatch mit isolierten Textparsern vorbereitet: Tonometrie geht nach `6205`, Lensmeter und REF nach `6228`, KM nach `6221`. Batch 8 ergaenzt Canon RK-F2, Canon TX-20P, ZEISS VISULENS 550, ZEISS VISUPLAN 500, ZEISS VISUREF 100, ZEISS IOLMaster 700, Visionix Retinomax 5, Visionix VX 120 und Visionix VX 650; REF/Lensmeter gehen nach `6228`, KM nach `6221`, Tonometrie nach `6205`, Pachymetrie/CCT nach `6220`, IOLMaster-VKT/AL nach `6227` und IOLMaster-R1/R2-Keratometrie nach `6228`. OCT-Geraete werden bewusst nicht als BuiltIn ausgeliefert. Scriptbasierte Geraete ohne echte Rohdaten werden nicht verworfen: Wenn Parserlogik ausreichend eindeutig ist, entstehen daraus XDTBox-Parser mit synthetischen Fixtures; unzureichend belegte Familien bleiben in `docs/REFERENZPAKET_GERAETE_AUSWERTUNG.md` als Kandidaten dokumentiert.

Aktuelle Version:

`1.0`

Dies ist die erste offizielle Installer-Version. XDTBox bleibt eine lokale Windows-Desktop-App ohne Cloudpflicht, ohne Windows-Dienst, ohne Windows-Autostart und ohne FileSystemWatcher. Der validierte Kernworkflow MEDISTAR + NIDEK ARK1S + XDT-Anhang-Link wurde am 2026-05-11 praktisch geprüft; externe Anhänge können über `6302`, `6303`, optional `6304` und `6305` in das AIS übernommen und aus der Karteikarte geöffnet werden.

Praktisch validiert ist aktuell nur der Workflow MEDISTAR + NIDEK ARK1S, einschließlich XDT-Anhang-Link für den geprüften Pflicht-Anhang-Praxislauf. Weitere V2-Geraeteprofile sind vorbereitet und koennen angezeigt bzw. konfiguriert werden, gelten aber noch nicht als produktiv validiert.

Die aktualisierte Roadmap mit Iststand, Sicherheitsentscheidungen und empfohlenen naechsten Entwicklungsphasen steht in [`docs/ROADMAP.md`](docs/ROADMAP.md).

Geräte-Dateianhang-Import und externe Link-Übergabe ans AIS sind für den validierten MEDISTAR/NIDEK-ARK1S-Pflicht-Anhang-Praxislauf praktisch bestätigt. Im aktuellen Prototyp ist der Konfigurationsbereich `XDT-Anhänge für AIS` im Schnittstellenprofil vorbereitet: optionale Import-/Exportordner, `AttachmentFileNameTemplate`, vorbereiteter `AttachmentTransferService` mit Copy/Move-Modus, `ExternalAisLinkFieldBuilder` für semantische Feldwerte, `ExternalAisLinkXdtFieldAdapter` für XDT-Feldcode/Wert-Paare, isolierter `AttachmentExternalLinkPreparationService` zur Orchestrierung auf explizite Eingabe und XDT-Linkfeld-Vorlagen für 6302, 6303, 6304 und 6305. Standard für Geräte-Dateianhänge ist `Move`, damit der XDT-Anhang-Importordner nach erfolgreicher Übernahme sauber bleiben kann; Mehrfachanhang-Heuristiken, Dokument-/Dateianhang-Templates und weitere Geräte-/AIS-Validierungen sind weiterhin offen. XDT-Längenpräfixe werden nicht in der Konfiguration gepflegt, sondern zentral durch den Exportmechanismus erzeugt.

Ein isolierter `AttachmentImportFolderScannerService` ist vorbereitet. Er listet unterstützte XDT-Anhang-Dateitypen im konfigurierten XDT-Anhang Importordner auf und verändert keine Dateien. Vollständige automatische Zuordnung und Mehrfachanhang-Heuristiken bleiben offen.

Eine isolierte automatische Kandidatenauswahl ist ebenfalls vorbereitet: Automatisch eindeutig ist zunächst nur der Fall, dass genau eine unterstützte Anhangdatei im XDT-Anhang Importordner gefunden wurde. Bei mehreren unterstützten Dateien wird nicht automatisch ausgewählt, weil der Patientenbezug unsicher wäre.

Die konservative automatische XDT-Anhang-Vorbereitung ist vorbereitet: Sie greift nur während laufender Überwachung, aktivierter XDT-Anhang-Funktion im Schnittstellenprofil und genau einem unterstützten Anhangkandidaten. Bei erfolgreicher Vorbereitung werden die XDT-Feldcode/Wert-Paare 6302, 6303, optional 6304 und 6305 transient an die erzeugte XDT-Exportdatei angehängt. Bei deaktivierter Funktion, fehlender Eindeutigkeit, mehreren unterstützten Anhängen oder Fehlern bleibt der bestehende Export unverändert. Die XDT-Längenpräfixe werden weiterhin zentral durch den Exportmechanismus erzeugt.

Für vollständige Verarbeitungspakete ist ein zweistufiges Wartemodell vorbereitet: Zuerst wartet eine erkannte AIS-Datei auf eine stabile Gerätedatei. Die Wartezeit ist pro Schnittstellenprofil konfigurierbar, Standard `10` Minuten. Kommt vor der Gerätedatei eine neuere AIS-Datei, ersetzt sie den wartenden Auftrag. Erst wenn AIS- und Gerätedatei als stabiles Paar vorhanden sind, startet die XDT-Anhang-Wartezeit. XDT-Anhänge können pro Schnittstellenprofil `optional` oder als `Pflicht` erwartet werden. Standard ist `optional`, die Standard-Wartezeit beträgt 30 Sekunden. Optional bedeutet: Nach Timeout dürfen Messwerte ohne Anhang übertragen werden. Pflicht bedeutet: Ohne eindeutigen Anhang wird die Verarbeitung blockiert; bei Timeout wird das konkrete Paket terminal als Fehlerfall abgeschlossen, damit neue Untersuchungen weiterlaufen können. Dabei werden nur bekannte AIS-/Gerätedateien dieses Pakets gemäß Fehler-/Archivoptionen behandelt, unbekannte Dateien und Exportordner werden nicht pauschal bereinigt. Mehrere unterstützte Anhänge bleiben unsicher und werden nicht automatisch zugeordnet.

Für langsam schreibende Geräte ist zusätzlich eine Stabilitätsprüfung vorbereitet: XDT-Anhänge werden erst automatisch ausgewählt oder übertragen, wenn sie über die konfigurierte Stabilitätswartezeit unverändert und lesbar bleiben. Standard ist 2 Sekunden. Das periodische Ordnerabfrage-Intervall ist pro Schnittstellenprofil konfigurierbar; der Standard bleibt 5 Sekunden. Es wird weiterhin kein FileSystemWatcher verwendet.

Der alte Tab `Profile & Templates` ist entfernt. Die Funktionen sind bewusst auf drei klar getrennte Bereiche verteilt: `XDT-Baukasten` für Entwurf/Test/Vorschau, `Profilverwaltung` für Pflege/Wartung und `Schnittstellenprofile` für konkrete Praxisanbindungen mit Ordnern, COM-Port, Aktivierung und Laufzeitparametern.

Der Tab `XDT-Baukasten` ist die eigenständige Arbeitsoberfläche für Entwürfe und Testdaten. Er hält AIS-, Geräte- und Exportprofil, Rohdaten, Exportregel-Arbeitskopie und Vorschauzustand getrennt von produktiven Schnittstellenprofilen. `Verarbeitung starten` erzeugt dort ausschließlich eine Vorschau im Speicher; es wird keine produktive Datei geschrieben, nichts archiviert und keine Überwachung gestartet. Die Ergebnisbox bietet `Roh-XDT`, `Ansicht im AIS`, `Geräteausgabe` und `Diagnose`; `Ansicht im AIS` zeigt nur fachlich sichtbare Karteikartenzeilen ohne Patientendatenblock, Feldnummern oder Parserdiagnose. Alle Ausgabeansichten zeigen links Baukasten-Zeilennummern, die nicht Bestandteil des echten Rohtexts sind. Der Exportregelbereich besitzt die Richtungen `Export an AIS` und bei bidirektionalen Geräten `Export an Gerät`, eine eigene Nummernspalte und eine Regel-Ausgabe-Markierung. CV-5000/CV-5000S und NIDEK RT-6100 zeigen eigene Geräteausgabe-Regeln, erweiterte Ausgabe-an-Gerät-Platzhalter und eine Vorschau der XML-Ausgabe, ohne produktive Geräteordner zu beschreiben. Änderungen an unterstützten Geräteausgabe-Regeln wirken direkt in der `Geräteausgabe`-Vorschau. UserDefined-bidirektionale Geräte bleiben offen und können eigene Geräteausgabe-Regeln anlegen. Die Gerätekompatibilität nutzt eine zentrale Baukastenprüfung: ModelName-/Profilabweichungen blockieren die Vorschau im Baukasten nicht, solange Datei, Parser und Mapping verwertbare Daten liefern. Die produktive Verarbeitung prüft weiterhin strenger.

Der Tab `Profilverwaltung` bündelt die reine Profilpflege: Suche, BuiltIn/UserDefined-Filter, Profiltypfilter, Details, Nutzungshinweise, Umbenennen, Duplizieren, geschütztes Löschen, lokales Template-Aufräumen, BuiltIn-Reparatur sowie Aktionen zum Anlegen/Laden von AIS-, Geräte-, Export- und Schnittstellenprofilen. Lokale Baukasten-Templates und Templatepakete können dort umbenannt beziehungsweise exportiert werden; der sichere Templatepaket-Import und das Speichern von Baukasten-Templates laufen über den XDT-Baukasten. BuiltIn-Profile werden nicht überschrieben oder gelöscht; verwendete Profile und aktive Schnittstellenprofile blockieren blindes Löschen.

## Geraete-Steckbriefe

Fuer alle aktuellen BuiltIn-Geraete ist ein kurzer technischer Steckbrief vorbereitet. Die Originaldaten liegen als App-Asset unter `XdtDeviceBridge.App\Assets\DeviceInfo\device-technical-profiles.de.json` und werden ueber die BuiltIn-`deviceProfileId` zugeordnet. Jeder Steckbrief nutzt das offizielle BuiltIn-Geraetebild aus `XdtDeviceBridge.App\Assets\Devices`, beschreibt das Geraet, die sicher ableitbaren Messarten und technische Hinweise ohne interne Ausgabe-Syntax.

Lokale Anpassungen werden nicht ins Original geschrieben. `DeviceTechnicalProfileService` fuehrt Original und lokale Overrides aus `%LocalAppData%\XdtDeviceBridge\device-info-overrides.json` zusammen. Die vorbereitete Steckbriefansicht kann Kurzbeschreibung, technische Hinweise und die `Notiz Techniker zum Geraet` lokal speichern und wieder auf das Original zuruecksetzen. Sicherung/Umzug nimmt diese lokale Override-Datei mit; eine Kopplung an den Profil-/Templateexport ist vorbereitet und als Folgeschritt offen. Die gesammelte Pruefansicht fuer Marc steht in [`docs/GERAETE_STECKBRIEFE.md`](docs/GERAETE_STECKBRIEFE.md).

## Installation, Update und Kundendaten

XDTBox 1.0 besitzt ein reproduzierbares Inno-Setup-Skript und eine Buildanleitung in [`docs/INSTALLER_BUILD_ANLEITUNG.md`](docs/INSTALLER_BUILD_ANLEITUNG.md). Der Default-Installationspfad ist `C:\XDTBox`, der Benutzer kann ihn bei Neuinstallation aendern. Das Setup unterscheidet Neuinstallation und Update, erzeugt `XDTBox_Setup_1.0.exe`, nutzt das offizielle `XDTBox.ico`, legt Startmenue- und optional Desktop-Verknuepfungen an und traegt XDTBox in Windows "Programme & Features" ein.

Die Datenpolitik ist in [`docs/INSTALLATION_UPDATE_DATENPOLITIK.md`](docs/INSTALLATION_UPDATE_DATENPOLITIK.md) festgelegt. App-Dateien, Hilfe, Themes, Standard-Assets und BuiltIn-Definitionen duerfen durch Updates ersetzt beziehungsweise repariert werden. Kundendaten unter `%LocalAppData%\XdtDeviceBridge` bleiben erhalten: Profile, UserDefined-Schnittstellenprofile, COM-Port-/Ordnerparameter, Baukasten-Templates, lokale Geraetebilder, `device-image-overrides.json`, Lizenzdaten inklusive `license.xdtboxlic`, UI-Einstellungen und Backups.

Fuer die Deinstallation gilt Variante B: Die Anwendung wird entfernt, Kundendaten bleiben standardmaessig erhalten und duerfen nur nach ausdruecklicher separater Bestaetigung geloescht werden. Externe Praxisordner wie AIS-Import, Geraete-Import, Export, Archiv oder Fehlerordner werden vom Installer/Deinstaller niemals blind geloescht. Interne Herstellerwerkzeuge und private Lizenzschluessel gehoeren nicht in den Kundeninstaller.

## Aktueller Funktionsumfang

- Einlesen einer AIS-GDT-Datei mit Patientendaten.
- Einlesen einer NIDEK ARK1S XML-Datei mit Messwerten.
- Anzeige von Patientendaten und Messwerten in einer WPF-Oberflaeche.
- Mapping der Patientendaten und Messwerte in MEDISTAR-kompatible XDT-Felder.
- Erzeugen einer XDT-Exportvorschau.
- Manuelles Schreiben einer Exportdatei in einen ausgewaehlten Ordner.
- Konfigurierbar automatisch startende Überwachung innerhalb der geöffneten App; passende Dateipaare werden bei laufender Überwachung automatisch verarbeitet.
- Lokale Konfigurationssicherung und Wiederherstellung ueber `.xdtboxbackup` ohne Patientendaten oder Messdateien; verwendete Geraetebilder blockieren den Restore nicht mehr vollstaendig, sondern werden als Warnung gemeldet.
- Lokales Hilfe-Center und Info-Dialog mit Herstellerdaten in der Tab-Zeile sowie Zahnrad-Einstellungen fuer Autostart- und Systray-Verhalten.
- Vorbereitete V2-Geraeteprofile fuer NIDEK ARK-510A, NIDEK ARK-560A, NIDEK AR-1, NIDEK AR-1S, NIDEK AR-310A, NIDEK LM7/LM7P, NIDEK LM-1800P, NIDEK LM-1800PD, NIDEK NT530P, NIDEK NT-1, NIDEK NT-1E, NIDEK NT-1P, NIDEK NT-510, NIDEK NT-530, NIDEK RT-6100, NIDEK RT-2100/RT-3100/RT-5100 RS232, Huvitz HRK-8000A, Huvitz HRK-9000A, Huvitz HNT-1P, Huvitz HTR-1A, TOMEY CF-2000, TOMEY TL-2000C, TOMEY TL-6000, TOMEY TL-7000, TOMEY MR-6000, TOMEY TOP-1000, TOMEY EM-3000, TOMEY EM-4000, Shin-Nippon Accuref R-800/K-900, Shin-Nippon DL-1000/DL-800/DL-900, Shin-Nippon NCT-200, Shin-Nippon SLM-4000, Reichert 7CR NCT, Reichert LensChek Plus, Rodenstock CX 800, TOPCON CL300/CL-300PDL, TOPCON Solos, TOPCON KR800/KR800S, TOPCON KR-1, TOPCON RM-800, TOPCON TRK2P/TRK-3 Omnia, TOPCON CT1P, TOPCON CT800A und TOPCON CV5000/CV5000S.
- NIDEK-Batch-6-XML-Varianten aus Referenzdaten: AR-1/AR-1S/AR-310A nutzen `ARMedian` fuer `6228`, AR-1S zusaetzlich `SR` fuer `6227`; LM-1800PD nutzt die vorhandene Lensmeter-XML-Fachlogik fuer `6228`; NT-1/NT-1E/NT-1P/NT-510/NT-530 nutzen die NT530P-nahe Tono/Pachy-Auswertung fuer `6205` und `6220`. Alle Profile sind inaktiv, testseitig abgesichert und brauchen echte Praxisdateien fuer die MEDISTAR-Abnahme.
- TOPCON-XML-Varianten aus Referenzdaten: CL-300PDL nutzt die CL-300-Lensmeterlogik nach `6228`, RM-800 nutzt die REF-XML-Familie nach `6228`, und TRK-3 Omnia nutzt die TRK-2P-Familie fuer `6228`, `6221`, `6220` und `6205`. Die Testfixtures sind synthetisch aus eindeutiger Referenzstruktur abgeleitet; echte Praxisrohdateien und MEDISTAR-Abnahme stehen noch aus.
- Huvitz-Textparser fuer serielle Referenzdaten: HRK-8000A/HRK-9000A liefern REF nach `6228` und KM nach `6221`, HNT-1P liefert Tonometrie nach `6205` und Pachymetrie nach `6220`, HTR-1A kombiniert diese Messarten. Die Testfixtures sind synthetisch aus Referenzlogik abgeleitet; echte Praxis-Rohdaten und MEDISTAR-Abnahme stehen noch aus.
- TOMEY-Parser fuer Referenzdaten: CF-2000 sowie TL-2000C/TL-6000/TL-7000 liefern Lensmeter-Zeilen nach `6228`; MR-6000 kombiniert REF `6228`, KM `6221`, Tonometrie `6205` und Pachymetrie `6220`; TOP-1000 liefert Tonometrie `6205` und Pachymetrie `6220`; EM-3000/EM-4000 liefern Endothel-/Zellmesswerte nach `6228`, Kommentare nach `6227` und Bild-/Dateiverweise nach `6302`. Die Testfixtures sind synthetisch aus Referenzlogik abgeleitet; echte Praxis-Rohdaten und MEDISTAR-Abnahme stehen noch aus.
- Shin-Nippon-Textparser fuer Referenzdaten: Accuref R-800 liefert REF nach `6228`, Accuref K-900 liefert REF nach `6228` und KM nach `6221`, DL-1000/DL-800/DL-900/SLM-4000 liefern Lensmeter-Zeilen nach `6228`, und NCT-200 liefert Tonometrie nach `6205`. Die Testfixtures sind synthetisch aus Referenzlogik abgeleitet; echte Praxis-Rohdaten und MEDISTAR-Abnahme stehen noch aus.
- Reichert-/Rodenstock-Textparser fuer Referenzdaten: Reichert 7CR NCT liefert Tonometrie nach `6205`, Reichert LensChek Plus Lensmeter-Zeilen nach `6228`, und Rodenstock CX 800 REF/KM-Zeilen nach `6228` beziehungsweise `6221`. Die Testfixtures sind synthetisch aus Referenzlogik abgeleitet; echte Praxis-Rohdaten und MEDISTAR-Abnahme stehen noch aus. Rodenstock Phoromat 2000 und Möller-Wedel Visutron bleiben Kandidaten.
- Canon-/ZEISS-/Visionix-Parser fuer Referenzdaten: Canon RK-F2 und Visionix VX 120 liefern REF nach `6228`; Canon TX-20P liefert Tonometrie nach `6205` und Pachymetrie nach `6220`; ZEISS VISULENS 550 liefert Lensmeter nach `6228`; ZEISS VISUPLAN 500 liefert Tonometrie nach `6205`; ZEISS VISUREF 100 und Visionix Retinomax 5 liefern REF nach `6228` und KM nach `6221`; ZEISS IOLMaster 700 liefert Biometrie/VKT/AL nach `6227` und R1/R2-Keratometrie nach `6228`; Visionix VX 650 liefert REF nach `6228` und Tonometrie nach `6205`. Die Testfixtures sind synthetisch aus Referenzlogik abgeleitet; echte Praxis-Rohdaten und MEDISTAR-Abnahme stehen noch aus. OCT-Workflows werden nicht geraten.
- Bidirektionaler Phoropter-Kandidat NIDEK RT-6100: XDTBox kann aus MEDISTAR-Historienwerten oder echten NIDEK-LM-/ARK-XML-Quellen konservativ `LM_Base`/`REF_Base`-XML fuer den MEM-200-/`DIRECT_RT_xx\TXT`-Ordner erzeugen. LM-7P-XML wird als Lensmeter-Quelle gelesen, ARK-1s-XML nutzt `ARMedian` fuer `REF_Base`; SR-/KM-/Bild-/Zusatzdaten, LM-Prismen und sonstige nicht freigegebene Felder werden nicht blind exportiert. Echte RT-6100-Rueckgabe-XML wird mit `Best -> 6228` sowie `Full -> 6227` ausgewertet, `Near`/`Night` wird nicht als Hauptausgabe gemappt. Die praktische RT-6100-/MEDISTAR-Abnahme steht noch aus.
- Serielle bidirektionale Phoropter-Kandidaten NIDEK RT-2100 / RT-3100 / RT-5100: XDTBox kennt PDF-basierte RS232-Presets, kann synthetische dokumentnahe RT->PC-Rohdaten sowie echte RT-3100-Praxismitschnitte parsen, `Final -> 6228` und `Subjective -> 6227` abbilden und den produktiven Ablauf patientengetriggert fuehren. Der Sendemodus ist im Schnittstellenprofil speicherbar; die RT-BuiltIns nutzen `Direkt Writer-Frame senden` als Default, `RS/SD-Handshake` und `RS senden, dann Writer ohne SD` bleiben auswaehlbar. Zusaetzlich ist der `NIDEK-RT Sendeinhalt` speicherbar und steht standardmaessig auf `RT-Referenz ohne ID (AR/AL)`, einer aus produktiven RT-Anbindungen abgeleiteten Frameform ohne ID-Block, mit LM ADD `AR`/`AL` und Abschluss `EB ET`. Die `RT-3100 Praxisvariante (getestet)` bleibt als Legacy-Testform mit 107-Byte-Frame, Legacy-ADD `RA`/`LA` und ohne zusaetzliches `EB` vor `ET` erhalten. Die RS-Anforderung sendet echte ASCII-Sternchen (`01 43 2A 2A 02 52 53 17 04`); LM-SCA-Augenpraefixe bleiben dagegen Leerzeichen + `R`/`L`. Fuer die Praxisabnahme zeigt das RT-Fenster eine einklappbare serielle Diagnose mit COM-Parametern, DTR/RTS/Handshake, CTS/DSR/DCD/RI, RS-/SD-/Writer-Hexdump, sichtbaren Steuerzeichen und Timeout-Checkliste. `COM-Port nur abhoeren` prueft die Profilparameter ohne Sendung und ohne Export; `Rueckgabe abhoeren und verarbeiten` nutzt dagegen den wartenden Patientenkontext und erzeugt nach gueltiger Rueckgabe produktiv die MEDISTAR-XDT-Datei.
- Allgemeine serielle Diagnose ist im Tab `Schnittstellenprofile` erreichbar: `RS232-Diagnose oeffnen` oeffnet ein Diagnosefenster mit den aktuell sichtbaren COM-Parametern. Dort koennen COM-Port, Rohtext, Hexdump, DTR/RTS und ein ASCII-Testkommando ohne produktive Verarbeitung geprueft werden.
- Unit-Tests fuer Parser, Mapping, Export und Datei-Export.

## MEDISTAR/NIDEK-Workflow

1. AIS-GDT-Datei einlesen.
2. NIDEK ARK1S XML-Datei einlesen.
3. Patientendaten aus der AIS-GDT-Datei uebernehmen.
4. `8000=6310` als MEDISTAR-XDT-Importsteuerung erzeugen.
5. `8402` als Untersuchungsart aus AIS/GDT uebernehmen.
6. Zwei `6228`-Ergebniszeilen fuer den MEDISTAR-Karteikarteneintrag erzeugen:
   - rechte Messwerte mit `R.:S=...`
   - linke Messwerte mit `L.:S=...`
   - Pupillendistanz mit `PD=...`
7. XDT-Inhalt als Exportdatei schreiben.

## Aktueller Automatik-Prototyp

Der Automatik-Prototyp bereitet die spaetere produktive Ordnerverarbeitung vor und startet in der geoeffneten App standardmaessig automatisch; er kann weiter manuell gestoppt und gestartet werden. Der manuelle Entwurfs- und Vorschautest befindet sich im Tab `XDT-Baukasten`.

### 1. Baukasten-Test

Die App unterstuetzt einen manuellen Testmodus im Tab `XDT-Baukasten`:

- AIS-GDT/XDT-Datei auswaehlen.
- Geraetedatei auswaehlen; die Vorschau verwendet das ausgewaehlte Exportprofil und das dazu passende Geraeteprofil. Dadurch koennen neben ARK1S auch profilierte XML-Workflows wie RT-6100 im Baukasten gegen ihre eigenen Mappingregeln geprueft werden.
- Exportvorschau aktualisieren.
- Exportvorschau anzeigen.
- Vorschau als `Roh-XDT`, `Ansicht im AIS`, `Geraeteausgabe` oder `Diagnose` pruefen.

Dieser manuelle Modus ist vom Betriebsmonitor im Tab `Verarbeitung` getrennt.

### 2. Schnittstellenprofile

Die App unterstuetzt Schnittstellenprofile fuer spaetere bzw. aktuelle Ordnerverarbeitung.

Ein Schnittstellenprofil enthaelt:

- AIS-Profil
- Geraeteprofil
- Exportprofil
- AIS-Importordner
- Geraete-Importordner
- Exportordner ans AIS
- Archivordner
- Fehlerordner
- XDT-Anhang Importordner (optional)
- XDT-Anhang Exportordner (optional)
- XDT-Anhang Dateiname, Standard: `{Ais.PatientNumber}_{Date:ddMMyyyy}_{Time:HHmmss}{ExtensionUpper}`
- XDT-Anhang Übertragung: Kopieren oder Verschieben, Standard `Verschieben`, vorbereitet für spätere Dateianhang-Verarbeitung
- Einschaltfunktion `XDT-Anhänge für AIS automatisch verarbeiten`, Standard aus; spätere Verarbeitung nur bei laufender Überwachung und vorhandener AIS-Patientennummer
- XDT-Anhang-Erwartung: optional oder Pflicht, Standard optional
- Wartezeit auf XDT-Anhang, Standard 30 Sekunden
- Dateistabilität für XDT-Anhänge, Standard 2 Sekunden
- vorbereitete XDT-Linkfeld-Vorlagen 6302 Dokumentenname, 6303 Dateiformat, 6304 Beschreibung und 6305 vollständiger Dateipfad
- Ordnerabfrage-Intervall für den periodischen Scan, Standard 5 Sekunden
- Wartezeit auf Gerätedatei nach AIS-Datei, Standard 10 Minuten
- Aktiv-Haken fuer automatische Verarbeitung
- aktive Schnittstellenprofile zaehlen immer als Geraeteanbindung; ein separater Lizenzpflicht-Haken wird nicht mehr angezeigt
- Archivierungsoptionen
- Fehlerablageoptionen

Die Schnittstellenprofil-Auswahl kann nach `Geraetehersteller` und `AIS-System` gefiltert werden. Filter und Schnittstellenprofil-Dropdown stehen in einer kompakten Kopfzeile; das Profil-Dropdown ist bewusst begrenzt, damit breite Fenster keine unruhige Auswahlflaeche erzeugen. Neue BuiltIn-, Template- und Repair-Profile nutzen sichere Betriebsdefaults: AIS-Importordner vor Verarbeitung bereinigen ist aktiv, Archivierung ist aus und Fehlerablage ist aktiv. Bewusst gespeicherte UserDefined-Einstellungen werden dadurch nicht ungefragt ueberschrieben.

Der Button `AIS Ausgabe Info` zeigt fuer das aktuelle Schnittstellenprofil eine reine AIS-Ausgabetabelle mit Feldkennung, Bedeutung, AIS-Relevanz, Karteikarten-Sichtbarkeit und Hinweisen. Unterhalb der Tabelle erscheint eine Techniker-Hilfe `Standard Zeilenbenennung in MEDISTAR`, die aus den aktiven Exportregeln abgeleitet wird: `V0` Lensmeter, `V1` Autorefraktor, `V2` Phoropter, `V4` subjektive Refraktion, `V7` Keratometer, `V8` Biometrie, `Y` Tonometrie und `P` Pachymetrie. XDTBox erzeugt diese Zeilenkennung nicht selbst; sie wird im MEDISTAR-XDT-Setup passend zur XDT-Feldkennung gepflegt. Reine Dokumentgeraete zeigen `6302`, `6303` und `6305` als Karteikarten-relevant; Messgeraete mit optionalem Anhang behalten diese Felder als optional. Die Untersuchungsart-Defaultlogik priorisiert die Geraeteidentitaet vor optionalen Anhangsfaehigkeiten: Tonometer/Pachymeter bleiben `TONO`, reine Dokument-/AttachmentOnly-Profile bleiben `DOKU`. `8402` wird als `Untersuchungsart` erklaert, bleibt die aus AIS empfangene Untersuchungsart und wird unveraendert wieder ausgegeben; empfohlene Defaultwerte wie `LENS`, `PHORO`, `AUTO`, `KERA`, `KOMB`, `TONO`, `PACHY`, `ENDO`, `OCT`, `DOKU` oder `MESS` dienen nur neuen Profilen und blockieren keine eingehende Untersuchungsart. Das `6305`-Pfadtemplate und die Buttons `Ordner Default`/`Ordner anlegen` liegen in getrennten Grid-Spalten, damit sie nicht ueberlappen.

### 3. Ueberwachung

Die Ueberwachung startet standardmaessig beim Oeffnen der App, wenn aktive Schnittstellenprofile vorhanden sind. Ueber das Zahnrad in der Tab-Zeile kann dieser App-Start-Autostart deaktiviert werden; die Buttons `Ueberwachung starten` und `Ueberwachung stoppen` bleiben erhalten.

Funktionen:

- aktive Schnittstellenprofile werden regelmaessig gescannt
- AIS-Importordner und Geraete-Importordner werden geprueft
- Dateien werden erst verarbeitet, wenn sie stabil und lesbar sind
- fertige AIS-/Geraete-Dateipaare werden in den Monitoring-Karten als Paketstatus sichtbar
- Ueberwachung kann manuell gestoppt und wieder gestartet werden

Wichtig: Es gibt keinen Windows-Dienst, keinen Windows-Autostart und aktuell keinen FileSystemWatcher. Die Ueberwachung basiert auf periodischem Scan innerhalb der geoeffneten App.

Die Monitoring-Meldungen im Tab `Verarbeitung` werden dedupliziert: Wiederholt ein Scan denselben technischen Zustand oder dieselbe Statusmeldung, wird sie nicht erneut als neues Ereignis angehängt. Die Übersicht der aktiven Schnittstellenprofile enthält grünliche Radar-/Glas-Karten pro aktiver Schnittstelle. Die Karten zeigen Profilzuordnung, Scanstatus, erwartete Eingänge wie AIS-Datei, Gerätedatei und optional XDT-Anhang sowie ausklappbare Details. Laufende Scan-/Paket-/Verarbeitungsergebnisse füllen die Karten mit Status wie `Wartet auf AIS`, `Wartet auf Gerät`, `Wartet auf XDT-Anhang`, `Export erfolgreich` oder `Fehler / blockiert`; falls vorhanden werden Patient, erkannte Dateien, XDT-Anhang-Zustand, Exportdatei und Warte-/Restzeiten sichtbar. Die XDT-Anhang-Kachel zeigt während der Wartephase `Pflicht` oder `Optional` plus Restzeit beziehungsweise Timeoutstatus. Die Eingangskacheln zeigen kompakte Live-Daten ohne sichtbare Pfade; Pfade bleiben in Tooltips und im Detailbereich verfügbar. Bei laufender Überwachung zeigt jede Karte eine deutlich sichtbare Radar-/Scanfläche mit schmalem grünem, halbtransparentem, horizontal wanderndem Scanbalken, dessen UI-Animation an das konfigurierte Scanintervall der Schnittstelle angelehnt ist. Das Scanintervall kann in der Karte per `-`/`+` als Schnittstellenprofil-Konfiguration angepasst werden; BuiltIn-Profile werden dabei nicht überschrieben. Detailinformationen wie AIS-Datei, Gerätedatei, Anhang, Export, letzter erfolgreicher Export und letzte Meldung liegen im Bereich `Details`. Die Animation ist nur eine Anzeige und steuert keine Verarbeitung.

### 4. Optionale automatische Verarbeitung

Im Tab `Verarbeitung` gibt es den Haken `Gefundene Dateipaare automatisch verarbeiten`.

Standard:

- deaktiviert

Wenn aktiviert:

- gefundene stabile Dateipaare werden automatisch verarbeitet
- eine XDT-Datei wird im konfigurierten Exportordner erzeugt
- Importdateien werden je nach Profiloption archiviert
- Fehler werden im Fehlerordner abgelegt, sofern konfiguriert

Wenn deaktiviert:

- die Überwachung aktualisiert nur Monitoring-Karten und Ereignisse
- es wird kein produktiver Export gestartet

### 5. Archivierungsmodus

Fuer verarbeitete Importdateien kann pro Schnittstellenprofil eingestellt werden:

- Kopieren
- Verschieben

Kopieren:

- AIS- und Geraetedateien bleiben im Importordner
- Kopien werden im Archivordner abgelegt

Verschieben:

- AIS- und Geraetedateien werden aus den Importordnern entfernt
- Dateien werden im Archivordner abgelegt

Empfehlung fuer produktiven Betrieb:

- Archivierung aktivieren
- Archivierungsmodus `Verschieben` verwenden

Grund: Dadurch bleiben die Importordner sauber und dieselben Dateien werden nicht erneut verarbeitet.

### 6. Archivstruktur

Archivierte Dateien werden in einer Tagesstruktur abgelegt.

Beispiel:

```text
Archivordner/
`-- yyyy/
    `-- MM/
        `-- dd/
            `-- Schnittstellenprofil/
                |-- AIS/
                |   `-- urspruengliche AIS-Datei
                `-- Device/
                    `-- urspruengliche Geraetedatei
```

Beispiel:

```text
C:\GitHub\Archiv\2026\05\03\MEDISTAR_NIDEK_ARK1S\AIS\TestPatient.gdt
C:\GitHub\Archiv\2026\05\03\MEDISTAR_NIDEK_ARK1S\Device\ARK1S.xml
```

### 7. Duplikatvermeidung

Die App verhindert waehrend der Automatik, dass dasselbe Dateipaar mehrfach exportiert wird.

Wenn ein bereits verarbeitetes Paar erneut in den Importordnern auftaucht:

- es wird nicht erneut exportiert
- es wird als bereits verarbeitet erkannt
- je nach Profiloption wird es ins Archiv kopiert oder verschoben
- unbekannte Dateien werden nicht angeruehrt

Wichtig: Duplikate werden nicht anhand medizinischer Messwerte erkannt, sondern anhand der technischen Dateipaar-Verarbeitung.

### 8. Fehlerablage

Wenn eine manuelle oder automatische Paarverarbeitung fehlschlaegt und Fehlerablage aktiviert ist:

- AIS-Datei wird in den Fehlerordner kopiert
- Geraetedatei wird in den Fehlerordner kopiert
- `error.txt` wird erzeugt
- Originaldateien bleiben erhalten, sofern nicht anders vorgesehen
- es erfolgt keine endgueltige Loeschung

Fehlerordner-Struktur entspricht sinngemaess der Archivstruktur:

```text
Fehlerordner/
`-- yyyy/
    `-- MM/
        `-- dd/
            `-- Schnittstellenprofil/
                |-- AIS/
                |-- Device/
                `-- error.txt
```

### 9. Keine Exportordner-Bereinigung

Die fruehere Option `Exportordner nach erfolgreicher Uebertragung leeren` wurde aus der UI entfernt.

Begruendung: Nachdem die App eine XDT-Datei in den Exportordner geschrieben hat, ist das AIS fuer den Abruf zustaendig. Ein automatisches Loeschen direkt nach dem Export waere riskant, weil das AIS die Datei eventuell noch nicht verarbeitet hat.

Die App bereinigt daher den Exportordner nicht.

### 10. Sicherheit

Aktueller Sicherheitsstand:

- keine automatische Verarbeitung beim App-Start
- Ueberwachung nur nach manuellem Start
- automatische Verarbeitung nur mit bewusst gesetztem Haken
- keine unbekannten Dateien werden geloescht
- keine Ordner werden pauschal geleert
- Importdateien werden nur gemaess Profiloption archiviert
- Exportordner wird nicht bereinigt
- Fehler werden nachvollziehbar dokumentiert
- Archivloeschung ist nur vorbereitet, aber nicht automatisch aktiv

### 11. Aktuell validierter Workflow

Der praktisch validierte Workflow ist:

1. Schnittstellenprofil MEDISTAR + NIDEK ARK1S konfigurieren.
2. AIS-Importordner setzen.
3. Geraete-Importordner setzen.
4. Exportordner setzen.
5. Archivordner setzen.
6. Schnittstellenprofil aktivieren.
7. Automatische Verarbeitung im Verarbeitung-Tab starten.
8. GDT-Datei und XML-Datei in die Importordner legen.
9. App erzeugt XDT-Datei.
10. MEDISTAR kann die XDT-Datei einlesen.
11. Importdateien werden ins Archiv verschoben, wenn so konfiguriert.
12. Bei aktivierter XDT-Anhang-Pflichtfunktion werden die Linkfelder `6302`, `6303`, optional `6304` und `6305` erzeugt; der externe Anhang kann aus der MEDISTAR-Karteikarte geöffnet werden.

### 12. Noch nicht produktiv umgesetzt

Noch nicht final umgesetzt bzw. bewusst noch nicht aktiviert:

- Windows-Dienst
- Autostart
- echter FileSystemWatcher
- dauerhafte Hintergrundverarbeitung ohne Benutzerstart
- automatische Archivloeschung im laufenden Betrieb
- Online-Lizenzierung
- produktive Lizenzsperre
- vollstaendiger Profil-Assistent fuer unbekannte Geraete
- `ReplaceExisting` fuer importierte Templatepakete und freie Konfliktloesungsdialoge
- Mehrfachanhang-Heuristiken und manuelle Zuordnung für unsichere Anhangfälle
- Dokument-/Dateianhang-Template für vorhandene Geräteanhänge
- selbst erzeugte PDF-Protokolle
- Installer / Deployment

Templatepakete koennen inzwischen analysiert, in einer Importvorschau geprueft und als sichere UserDefined-Profile uebernommen werden. Ersetzen bestehender Profile und automatische Aktivierung importierter Schnittstellenprofile sind weiterhin nicht aktiv.

### 13. Build und Test

Nach README-Aenderungen sollten weiterhin Build und Tests laufen:

```powershell
dotnet build XdtDeviceBridge.sln
dotnet test XdtDeviceBridge.sln
```

## Voraussetzungen

- Windows 10 oder Windows 11
- .NET 8 SDK
- Visual Studio 2022

## Build

```powershell
dotnet build XdtDeviceBridge.sln
```

## Tests

```powershell
dotnet test XdtDeviceBridge.sln
```

## Start

```powershell
dotnet run --project XdtDeviceBridge.App
```

## Aktueller Lizenz-Prototyp

- Die App erzeugt oder laedt lokale `InstallationInfo`-Daten.
- Angezeigt werden Installation-ID, Computername, Benutzername und Lizenzstatus.
- Eine Offline-Lizenzanfrage kann als JSON-Datei exportiert werden.
- Die Lizenzanfrage kann Kundendaten sowie die Namen aktiver beziehungsweise lizenzpflichtiger Geraeteanbindungen dokumentieren. Diese Namen dienen nur der Herstellerverwaltung; lizenzbindend bleibt ausschliesslich die Anzahl aktiver Geraeteanbindungen.
- Eine signierte Offline-Lizenzdatei kann als `.xdtboxlic` importiert werden.
- Die Lizenzdatei wird mit RSA-PSS/SHA-256 validiert und beim Import automatisch lokal gespeichert; ein zusaetzlicher Speicherschritt ist nicht erforderlich.
- Die gespeicherte `.xdtboxlic` ist fuehrende Lizenzquelle vor Legacy-`license.json`; `Karenzzeiten aktualisieren` bewertet nur neu und setzt die lizenzierte Geraeteanzahl nicht zurueck.
- `Lizenz entfernen` loescht nur die lokal importierte `.xdtboxlic` und eine eventuelle Legacy-`license.json`. Kundendaten, Lizenzanfragen, Profile, Historien und Schluesseldateien bleiben unangetastet.
- Neue Lizenzanforderungen beschreiben immer den aktuellen Gesamtzustand der Installation mit allen aktiven Geraeteanbindungen, nicht nur Differenzen zu einer alten Lizenz.
- Legacy-JSON bleibt nur als unsignierter Uebergang erkennbar.
- Das interne Herstellerwerkzeug `XdtBox.LicenseIssuer.exe` erzeugt `.xdtboxlic`-Dateien aus Lizenzanforderung oder InstallationId. Es ist ein Kommandozeilentool; Doppelklick ohne Parameter zeigt Hilfe und wartet auf Tastendruck.
- Die grafische Hersteller-App `XdtBox.LicenseManager.exe` liest Lizenzanfragen, erzeugt signierte `.xdtboxlic`-Dateien, fuehrt eine lokale Historie ausgestellter Lizenzen und speichert Hersteller-Einstellungen. Sie ist nicht Teil der Endkunden-App; private Schluessel bleiben externe Dateien.
- Der produktive V1-KeyId lautet `xdtbox-prod-2026-01`. Der passende private PEM-Schluessel liegt ausschliesslich beim Hersteller, standardmaessig unter `C:\XDTBox\Lizenzaktivierung\keys\xdtbox_private.pem`; die App enthaelt nur den Public Key.
- Die Lizenz wird aktuell nur angezeigt, aber noch nicht erzwungen.
- Es gibt noch keine Online-Lizenzierung.
- Die MEDISTAR/NIDEK-Verarbeitung bleibt auch ohne Lizenz weiterhin nutzbar.

## Bekannte Einschraenkungen

- Automatik nur nach manuellem Start, kein Windows-Dienst, kein Autostart und kein FileSystemWatcher.
- Kein vollstaendiger Profil-Assistent fuer unbekannte Geraete; vorbereitete Profile koennen angezeigt und konfiguriert werden.
- Keine SQLite-Speicherung.
- Ergebnisformat aktuell nur fuer MEDISTAR/NIDEK ARK1S praktisch validiert.
