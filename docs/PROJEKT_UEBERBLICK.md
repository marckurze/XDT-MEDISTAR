# Projektueberblick XdtDeviceBridge / XDT Verwaltung

Stand: 2026-05-06  
Version: `0.1.0-prototype`

Diese Datei dient als Uebergabe an einen neuen Chat oder eine neue Planungssitzung. Sie fasst Ziel, Iststand, Architekturidee, Sicherheitsentscheidungen, validierte Funktionen und offene naechste Schritte zusammen.

## 1. Kurzbeschreibung

XdtDeviceBridge ist eine lokale WPF-Desktop-App fuer die Anbindung augenaerztlicher Messgeraete an ein Arztinformationssystem, aktuell mit Fokus auf MEDISTAR.

Der erste stabile Prototyp verarbeitet validiert:

- MEDISTAR-GDT/XDT-Eingang
- NIDEK ARK1S XML-Geraetedaten
- MEDISTAR-kompatiblen XDT-Export
- Ergebniszeilen fuer die Karteikarte, insbesondere ueber Feld `6228`
- Steuerfeld `8000=6310`
- Untersuchungsart aus dem AIS-Feld `8402`

Die App ist bewusst lokal, dateibasiert und offline gedacht. Sie arbeitet aktuell mit JSON-basierten Profilen im lokalen AppData-Profilkatalog. Eine SQLite-Speicherung ist noch nicht umgesetzt.

## 2. Hauptziel

Ziel ist eine sichere, konfigurierbare XDT-Bridge zwischen:

1. AIS/PVS-Dateien, z. B. MEDISTAR-GDT/XDT
2. Geraetedateien, z. B. XML von augenaerztlichen Messgeraeten
3. XDT-Exportdateien, die vom AIS wieder eingelesen werden koennen

Die App soll perspektivisch verschiedene AIS-, Geraete- und Exportprofile unterstuetzen. Der Anwender soll Profile, Exportregeln und Schnittstellen konfigurieren koennen, ohne die validierte MEDISTAR/NIDEK-ARK1S-Verarbeitung zu beschaedigen.

## 3. Aktueller validierter Workflow

Der praktisch validierte Workflow ist:

1. MEDISTAR erzeugt oder liefert eine AIS-GDT/XDT-Datei.
2. NIDEK ARK1S erzeugt eine XML-Geraetedatei.
3. Die App liest beide Dateien ein.
4. Patientendaten werden aus der AIS-Datei gemappt.
5. Geraetemesswerte werden aus XML ausgelesen.
6. Exportregeln erzeugen XDT-Felder und Ergebniszeilen.
7. Die App schreibt eine XDT-Datei in den Exportordner.
8. MEDISTAR kann die erzeugte XDT-Datei einlesen.

Dieser Workflow funktioniert manuell und inzwischen auch ueber manuell gestartete Ueberwachung mit optionaler automatischer Verarbeitung.

## 4. Aktuelle Version

Die aktuelle stabile Prototypversion ist:

```text
0.1.0-prototype
```

Konsistente Stellen:

- `VERSION`
- `Directory.Build.props`
- `README.md`
- `CHANGELOG.md`

Assembly-Werte:

- `Version`: `0.1.0-prototype`
- `AssemblyVersion`: `0.1.0.0`
- `FileVersion`: `0.1.0.0`
- `InformationalVersion`: `0.1.0-prototype`

## 5. Zentrale Tabs der App

Die App besitzt aktuell diese Hauptbereiche:

- `Verarbeitung`
- `Profile & Templates`
- `Schnittstellenprofile`
- `Lizenz`

### 5.1 Verarbeitung

Der Tab `Verarbeitung` enthaelt zwei Betriebsarten:

1. Manueller Testmodus
2. Manuell startbare Ueberwachung mit optionaler automatischer Verarbeitung

Der manuelle Testmodus bleibt als Diagnose- und Validierungsmodus erhalten.

Der Ueberwachungsbereich zeigt aktive Schnittstellenprofile, scannt periodisch Importordner und zeigt gefundene AIS-/Geraete-Dateipaare an. Die Ueberwachung startet nicht beim App-Start, sondern nur nach Button-Klick.

### 5.2 Profile & Templates

Dieser Tab enthaelt:

- Profiluebersicht
- Templatepaket-Export
- Templatepaket-Import mit Validierung
- Exportprofil-Auswahl
- Exportregeln
- Exportregel-Entwurf
- Regelvorschau
- Gesamtexport-Vorschau
- Platzhalter-Baukasten

Der Baukasten zeigt AIS- und Device-Platzhalter mit fachlichen Namen, ausgelesenen Werten, Formatvorschlaegen und Ausgabeart AIS/Mensch.

### 5.3 Schnittstellenprofile

Dieser Tab dient zur Konfiguration produktionsnaher Anbindungen.

Ein Schnittstellenprofil verknuepft:

- AIS-Profil
- Geraeteprofil
- Exportprofil
- AIS-Importordner
- Geraete-Importordner
- Exportordner ans AIS
- Archivordner
- Fehlerordner
- Aktiv-Haken fuer spaetere/aktuelle automatische Verarbeitung
- Lizenzpflicht-Haken
- Archivierungsmodus Copy/Move
- Fehlerablageoption

BuiltIn-Profile duerfen nicht ueberschrieben werden. Beim Speichern eines BuiltIn-Schnittstellenprofils wird eine UserDefined-Kopie erzeugt.

### 5.4 Lizenz

Der Tab `Lizenz` zeigt:

- Installationsinformationen
- Lizenzstatus
- Export einer Lizenzanfrage
- Import einer Lizenzdatei
- lizenzierte Geraete/Anbindungen
- Bewertung aktiver lizenzpflichtiger Schnittstellenprofile
- Karenzzeiten fuer neue nicht gedeckte Anbindungen

Wichtig: Es gibt aktuell keine harte produktive Lizenzsperre. Die Lizenzlogik ist Anzeige-, Bewertungs- und Vorbereitungslogik.

## 6. Automatik-Prototyp

Die automatische Verarbeitung ist bewusst vorsichtig umgesetzt.

Aktuell gilt:

- keine Verarbeitung beim App-Start
- kein Windows-Dienst
- kein Autostart
- kein FileSystemWatcher
- Ueberwachung nur manuell per Button
- periodischer Scan statt Dauer-Watcher
- automatische Verarbeitung nur bei gesetztem Haken
- keine unbekannten Dateien werden geloescht
- keine Ordner werden pauschal geleert
- Exportordner wird nicht bereinigt

Wenn die Ueberwachung laeuft, scannt die App aktive Schnittstellenprofile regelmaessig. Gefundene Dateien werden nur verarbeitet, wenn sie stabil und lesbar sind und ein AIS-/Geraete-Dateipaar bilden.

## 7. Optionale automatische Verarbeitung

Im Tab `Verarbeitung` gibt es den Haken:

```text
Gefundene Dateipaare automatisch verarbeiten
```

Standard:

- aus

Wenn aus:

- Dateipaare werden nur angezeigt.
- Verarbeitung erfolgt nur ueber den Button zur manuellen Paarverarbeitung.

Wenn an:

- gefundene stabile Dateipaare werden waehrend der manuell gestarteten Ueberwachung automatisch verarbeitet.
- die gleiche Verarbeitungslogik wie bei manueller Paarverarbeitung wird verwendet.
- erzeugte XDT-Dateien werden im konfigurierten Exportordner abgelegt.
- Importdateien werden je nach Schnittstellenprofil archiviert.
- Fehler werden je nach Konfiguration in den Fehlerordner kopiert.

Die App merkt sich waehrend der Laufzeit bereits verarbeitete Dateipaare und exportiert dasselbe Paar nicht erneut.

## 8. Archivierung und Duplikate

Verarbeitete Importdateien koennen pro Schnittstellenprofil archiviert werden.

Archivierungsmodi:

- `Copy`: Dateien werden ins Archiv kopiert, Originale bleiben im Importordner.
- `Move`: Dateien werden ins Archiv verschoben, Originale verschwinden aus dem Importordner.

Empfehlung fuer produktionsnahe Nutzung:

- Archivierung aktivieren
- Archivierungsmodus `Move`
- bereits verarbeitete AIS-Dateien aus Importordner entfernen
- bereits verarbeitete Geraetedateien aus Importordner entfernen

Grund: Wenn Geraetedateien im Importordner liegen bleiben, koennen sie bei jedem Scan erneut gefunden werden. Die Duplikaterkennung verhindert zwar erneuten Export, aber der Ordner bleibt unaufgeraeumt. Mit `Move` werden bekannte verarbeitete Dateien sicher ins Archiv verschoben.

Wichtig:

- keine unbekannten Dateien werden bewegt
- keine Datei wird endgueltig geloescht
- es werden nur bekannte verarbeitete Paare behandelt
- der Exportordner wird nicht geleert

## 9. Fehlerablage

Wenn eine manuelle oder automatische Paarverarbeitung fehlschlaegt und Fehlerablage aktiviert ist:

- AIS-Datei wird in den Fehlerordner kopiert
- Geraetedatei wird in den Fehlerordner kopiert
- `error.txt` wird erzeugt
- Originaldateien bleiben erhalten
- keine Datei wird geloescht
- keine Datei wird verschoben

Der Optionsname `MoveFailedFilesToErrorFolder` existiert aus Kompatibilitaetsgruenden, das aktuelle Verhalten ist aus Sicherheitsgruenden Copy-only.

## 10. Archivstruktur und Fehlerstruktur

Archivierte Dateien werden tagesbezogen abgelegt:

```text
Archivordner/
└── yyyy/
    └── MM/
        └── dd/
            └── Schnittstellenprofil/
                ├── AIS/
                │   └── urspruengliche AIS-Datei
                └── Device/
                    └── urspruengliche Geraetedatei
```

Fehlerdateien werden sinngemaess abgelegt:

```text
Fehlerordner/
└── yyyy/
    └── MM/
        └── dd/
            └── Schnittstellenprofil/
                ├── AIS/
                ├── Device/
                └── error.txt
```

## 11. Profil- und Template-System

Die App unterscheidet mehrere Profilarten:

- AIS-Profile
- Device-/Geraeteprofile
- Exportprofile
- Schnittstellenprofile

BuiltIn-Profile:

- werden mit der App geliefert
- duerfen nicht ueberschrieben werden
- koennen als Grundlage fuer UserDefined-Kopien dienen

UserDefined-Profile:

- werden lokal gespeichert
- liegen im AppData-Profilkatalog
- koennen aus Entwuerfen oder Konfigurationen entstehen

Lokaler Profilkatalog:

```text
%LocalAppData%\XdtDeviceBridge\profiles\
```

Unterordner:

- `ais`
- `devices`
- `exports`
- `interfaces`

Exportprofile werden unter anderem hier gespeichert:

```text
%LocalAppData%\XdtDeviceBridge\profiles\exports\
```

Templatepakete:

- Export ist vorhanden.
- Import ist vorhanden.
- Importvalidierung ist vorhanden.
- Produktive Uebernahme importierter Templatepakete mit Konfliktloesung ist noch offen.

## 12. Exportregel-Baukasten

Der Exportregel-Baukasten ist vorbereitet, um Anwendern technische Platzhalter fachlich verstaendlich anzuzeigen.

Er zeigt unter anderem:

- Platzhalter
- Name Messwert
- ausgelesener Wert
- Ausgabeart AIS/Mensch
- Verwenden-Haken
- Formatvorschlaege

Wichtige Formatfunktionen:

- `Raw`
- `Diopter`
- `Axis`
- `Pd`
- `Iop`
- `Pachy`
- `Prism`
- `Keratometry`
- `Addition`
- `Time`

Der Baukasten kennt inzwischen viele ophthalmologische Begriffe, z. B.:

- Sphaere
- Zylinder
- Achse
- Addition
- Sphaerisches Aequivalent
- Augendruck
- Pachymetrie
- Hornhautradius
- Keratometrie
- Prisma horizontal/vertikal
- Basisrichtung
- Uhrzeit

## 13. RuleType im Exportprofil

Exportregeln koennen verschiedene Typen haben:

- `StaticValue`: fester Wert, z. B. Steuerfeld `8000=6310`
- `AisField`: Wert kommt aus der AIS-Datei
- `DeviceField`: Wert kommt aus der Geraetedatei
- `Template`: Ergebniszeile mit mehreren Platzhaltern

Die produktive Paarverarbeitung kann ExportProfileDefinition-Regeln ueber einen Adapter in die vorhandene MappingEngine-Struktur ueberfuehren.

## 14. Validierter Geraete-/AIS-Stand

Produktiv bzw. praktisch validiert:

- MEDISTAR
- NIDEK ARK1S XML
- MEDISTAR-kompatibler XDT-Export

Vorbereitete BuiltIn-/V2-Profile:

- NIDEK LM7/LM7P
- NIDEK NT530P
- TOPCON CL300
- TOPCON KR800
- TOPCON TRK2P

Wichtig: Diese vorbereiteten Profile sind fachlich und technisch angelegt, aber nicht im gleichen Sinne produktiv validiert wie MEDISTAR + NIDEK ARK1S.

## 15. MEDISTAR

MEDISTAR ist aktuell das zentrale AIS-Ziel.

Relevant:

- GDT/XDT-Eingang
- Patientendaten
- Untersuchungsart aus `8402`
- XDT-Ausgang
- `8000=6310`
- Ergebniszeilen in `6228`

MEDISTAR-Zeilentypen wie `V0`, `V1`, `V2`, `V7`, `P`, `Y` sind informativ. Sie sind hilfreich fuer Templates und Zielbilder, aber keine harte XDT-Schnittstellenlogik der App.

## 16. NIDEK ARK1S

NIDEK ARK1S ist das aktuell validierte Geraeteprofil.

Unterstuetzt sind im Prototyp vor allem:

- XML-Geraetedatei
- Autorefraktor-/ARK-Werte
- rechte/linke Messwerte
- Sphaere
- Zylinder
- Achse
- Ergebniszeilen fuer MEDISTAR

Diese Verarbeitung darf bei weiteren Schritten nicht beschaedigt werden.

## 17. NIDEK LM7/LM7P

Die NIDEK LM7/LM7P LAN/XML-Schnittstelle ist fachlich ausgewertet und dokumentiert.

Wichtige Punkte:

- LAN/WLAN ueber SMB / Common Internet File System
- Geraet schreibt XML-Datei in Windows-Shared-Folder
- dieser Shared Folder entspricht dem Geraete-Importordner
- App scannt/verarbeitet den Ordner
- nach erfolgreicher Verarbeitung ist Archivmodus `Move` sinnvoll, damit der Shared Folder wieder frei wird
- das Geraet kann Network Timeout melden, wenn die XML-Datei zu lange im Shared Folder liegt

Dateiname laut Interface Manual:

```text
LM_<ID>_<YYYYMMDDHHMMSS>_<MAC-Lower-Bytes>.xml
```

Zusatzdatei:

```text
NIDEK_LM_Stylesheet.xsl
```

Die XSL-Datei ist keine Messdatei und darf nicht als Geraetedatei verarbeitet werden. Im aktuellen Klassifizierer ist `.xsl` nicht relevant und wird nicht als DeviceXml behandelt.

XML-Struktur:

```text
Ophthalmology
├── Common
└── Measure Type="LM"
    ├── MeasureMode
    ├── DiopterStep
    ├── AxisStep
    ├── CylinderMode
    ├── PrismDiopterStep
    ├── PrismBaseStep
    ├── PrismMode
    ├── AddMode
    ├── LM
    │   ├── S
    │   ├── R
    │   └── L
    └── PD
```

LM7/LM7P muss perspektivisch NIDEK_V1.00 und NIDEK_V1.01 unterstuetzen.

Wichtige Messwerte:

- `Sphere`
- `Cylinder`
- `Axis`
- `SE`
- `ADD`
- `ADD2`
- `NearSphere`
- `NearSphere2`
- `Prism`
- `PrismBase`
- `PrismX`
- `PrismX/@base`
- `PrismY`
- `PrismY/@base`
- `UVTransmittance`
- `ConfidenceIndex`
- `Error`
- PD-Werte wie `Distance`, `DistanceR`, `DistanceL`, `Near`, `NearR`, `NearL`

Der Parser stellt XML-Attribute als SourcePaths bereit, z. B.:

- `Measure/@Type`
- `PrismX/@base`
- `PrismY/@base`
- `Sphere/@unit`
- `Distance/@unit`

Offen bzw. noch genauer zu planen:

- DeviceParseIssue-/Warnlogik fuer LM7/LM7P-Error-Tags
- produktive Validierung mit echten LM7/LM7P-Dateien
- ggf. FormatPercent fuer UVTransmittance
- ggf. FormatDate fuer Common/Date

## 18. Lizenzsystem

Das Lizenzsystem ist vorbereitet, aber nicht hart durchgesetzt.

Vorhanden:

- InstallationInfo
- Lizenzanfrage exportieren
- Lizenzdatei importieren
- LicenseInfo
- lizenzierte Geraeteanzahl
- Bewertung aktiver lizenzpflichtiger Schnittstellenprofile
- Karenzzeitmodell
- Karenzzeiten speichern/laden
- Karenzzeiten aktualisieren
- Anzeige im Lizenz-Tab

Zaehlregel:

Ein Schnittstellenprofil zaehlt als lizenzpflichtige Anbindung, wenn:

- `IsActive = true`
- `IsLicenseRequired = true`

Nicht gezaehlt werden:

- inaktive Profile
- nicht lizenzpflichtige Profile
- reine Exportprofile ohne Schnittstellenprofil

Keine Sperrlogik:

- Verarbeitung bleibt nutzbar
- Lizenzstatus ist Anzeige/Vorbereitung
- keine Online-Lizenzierung
- keine Signaturpruefung

## 19. Sicherheitliche Entscheidungen

Diese Entscheidungen sind fuer weitere Planung wichtig:

- keine automatische Verarbeitung beim App-Start
- kein Windows-Dienst
- kein Autostart
- kein FileSystemWatcher
- Ueberwachung nur manuell startbar
- automatische Verarbeitung nur mit bewusst gesetztem Haken
- keine pauschale Ordnerleerung
- keine unbekannten Dateien anfassen
- keine Exportordner-Bereinigung
- Importdateien nur gemaess Profiloption archivieren
- Fehlerfall kopiert Dateien in Fehlerordner
- Originaldateien bleiben im Fehlerfall erhalten
- Duplikaterkennung verhindert erneuten Export gleicher Dateipaare
- Archivloeschung ist nur vorbereitet, nicht automatisch aktiv

## 20. Wichtige vorhandene Services/Bausteine

Wichtige Core-/Infrastructure-Bausteine:

- `GdtParser`
- `XmlDeviceParser`
- `PatientDataMapper`
- `MappingEngine`
- `XdtExportBuilder`
- `ExportFileNameBuilder`
- `FileExportService`
- `ProfileCatalogService`
- `ProfileFileRepository`
- `ExportProfileDraftService`
- `InterfaceProfileConfigurationService`
- `FolderSafetyValidator`
- `LicensedDeviceStateEvaluator`
- `LicensedDeviceGracePeriodService`
- `FileStabilityService`
- `DirectorySnapshotService`
- `ImportFileClassifier`
- `PendingImportQueue`
- `AutoImportScannerService`
- `PeriodicAutoImportScanService`
- `InterfaceProfileManualProcessor`
- `ProcessedFileArchiveService`
- `FailedFileCopyService`
- `ArchiveRetentionCleanupService`
- `AutoImportPairProcessingCoordinator`
- `DuplicateImportFileHandler`

## 21. Wichtige offene Themen

Noch nicht final umgesetzt oder bewusst noch nicht aktiviert:

- Windows-Dienst
- Autostart
- echter FileSystemWatcher
- dauerhafte Hintergrundverarbeitung ohne Benutzerstart
- produktive Lizenzsperre
- digitale Signaturpruefung fuer Lizenzdateien
- Online-Lizenzierung
- vollstaendiger Profil-Assistent fuer unbekannte Geraete
- produktive Uebernahme importierter Templatepakete mit Konfliktloesung
- PDF-/EV-Dokumentenerzeugung
- MEDISTAR EV-Verknuepfung
- automatische Archivloeschung im laufenden Betrieb
- SQLite-Speicherung
- vollstaendige AIS-Unterstuetzung ausserhalb MEDISTAR
- Installer / Deployment
- produktive Validierung der vorbereiteten V2-Geraeteprofile

## 22. Dokumentationsstand

Zentrale Dokumente:

- `README.md`: Einstieg, aktueller Automatik-Prototyp, validierter Stand
- `CHANGELOG.md`: Version `0.1.0-prototype`
- `VERSION`: aktuelle Version
- `Directory.Build.props`: Assembly-/Package-Version
- `docs/PFLICHTENHEFT.md`: fachliche und technische Anforderungen, teilweise Zielbild und Iststand gemischt
- `docs/ARCHITEKTUR.md`: Architekturidee, teilweise nicht ganz auf dem neuesten Automatik-Iststand
- `docs/GERAETE_BEISPIELE.md`: Geraetebeispiele, SourcePaths, MEDISTAR-Zielbilder, LM7/LM7P-Manual-Auswertung
- `docs/ISTSTAND_ABGLEICH_BERICHT.md`: Abgleich Dokumentation vs. Code mit offenen Korrekturen

Wichtig fuer neue Planung:

`docs/ISTSTAND_ABGLEICH_BERICHT.md` zeigt, wo Dokumentation und Code noch unscharf auseinanderlaufen. Vor groesseren neuen Features sollten diese Punkte beachtet werden.

## 23. Empfohlene naechste Schritte

Sinnvolle naechste kleine Schritte waeren:

1. README-Korrektur: "Keine Geraeteprofilverwaltung" praezisieren zu "kein vollstaendiger Profil-Assistent".
2. Pflichtenheft bereinigen: SQLite, Ordnerueberwachung und Exportordner-Bereinigung klar als offen/ueberholt markieren.
3. Architektur um aktuellen Stand `0.1.0-prototype` erweitern.
4. Templatepaket-Import produktiv uebernehmen mit Konfliktloesung planen.
5. Profil-Assistent fuer neue Geraete konzipieren.
6. LM7/LM7P mit echten Dateien produktiv testen.
7. DeviceParseIssue-/Warnlogik fuer Geraete-Error-Tags planen.
8. Installer-/Deployment-Konzept erstellen.

## 24. Arbeitsregeln fuer kuenftige Chats

Bei weiteren Implementierungen unbedingt beachten:

- bestehende MEDISTAR/NIDEK-ARK1S-Verarbeitung nicht beschaedigen
- kleine, testbare Schritte
- keine automatische Verarbeitung beim App-Start
- keine unkontrollierte Loeschlogik
- keine pauschale Ordnerbereinigung
- keine Lizenzsperre ohne ausdrueckliche Spezifikation
- BuiltIn-Profile nicht ueberschreiben
- UserDefined-Profile separat speichern
- Tests nach Aenderungen ausfuehren
- bei UI-Aenderungen Lesbarkeit und Bedienbarkeit in WPF beachten

## 25. Kompakter Starttext fuer einen neuen Chat

Der folgende Text kann in einen neuen Chat kopiert werden:

```text
Wir arbeiten an XdtDeviceBridge / XDT Verwaltung, Version 0.1.0-prototype.

Die App ist eine lokale WPF-Desktop-Bridge fuer augenaerztliche Geraetedaten zu MEDISTAR/XDT. Produktiv validiert ist MEDISTAR + NIDEK ARK1S XML: AIS-GDT/XDT einlesen, NIDEK-XML einlesen, Mapping ausfuehren, MEDISTAR-kompatible XDT-Datei mit 8000=6310, 8402 und 6228-Ergebniszeilen erzeugen.

Es gibt Tabs Verarbeitung, Profile & Templates, Schnittstellenprofile und Lizenz.

Die App hat einen manuellen Testmodus und eine manuell startbare periodische Ueberwachung. Es gibt keinen Windows-Dienst, keinen Autostart und keinen FileSystemWatcher. Automatische Verarbeitung laeuft nur, wenn die Ueberwachung manuell gestartet wurde und der Haken "Gefundene Dateipaare automatisch verarbeiten" gesetzt ist.

Schnittstellenprofile verknuepfen AIS-Profil, Geraeteprofil, Exportprofil und Ordner. UserDefined-Profile werden lokal unter %LocalAppData%\XdtDeviceBridge\profiles gespeichert. BuiltIn-Profile duerfen nicht ueberschrieben werden.

Archivierung Copy/Move, Duplikatbehandlung, Fehlerablage mit error.txt, Lizenzanzeige, lizenzierte Anbindungen und Karenzzeitmodell sind vorbereitet. Es gibt keine harte Lizenzsperre.

Vorbereitete V2-Geraeteprofile existieren fuer NIDEK LM7/LM7P, NIDEK NT530P, TOPCON CL300, TOPCON KR800 und TOPCON TRK2P. Nur MEDISTAR + NIDEK ARK1S ist praktisch validiert.

Wichtige Sicherheitsregeln: keine Verarbeitung beim App-Start, keine pauschale Ordnerleerung, keine unbekannten Dateien anfassen, Exportordner nicht bereinigen, Importdateien nur gemaess Profiloption archivieren.

Zentrale Dokumente: README.md, CHANGELOG.md, docs/PFLICHTENHEFT.md, docs/ARCHITEKTUR.md, docs/GERAETE_BEISPIELE.md, docs/ISTSTAND_ABGLEICH_BERICHT.md und docs/PROJEKT_UEBERBLICK.md.

Naechste sinnvolle Planungsfelder: Dokumentationskorrekturen aus ISTSTAND_ABGLEICH_BERICHT, Profil-Assistent, produktive Templatepaket-Uebernahme, LM7/LM7P-Validierung, Installer/Deployment.
```

