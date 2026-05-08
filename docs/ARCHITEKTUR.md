# Architektur Version 2

Dieses Dokument beschreibt die geplante Zielarchitektur für XdtDeviceBridge Version 2. Der aktuelle MEDISTAR/NIDEK-ARK1S-Prototyp bleibt dabei funktionsfähig und bildet das erste validierte Standardprofil. Die neue Architektur soll schrittweise eingeführt werden, ohne den bestehenden Ablauf unnötig zu destabilisieren.

---

## 1. Aktueller Stand 0.1.0-prototype

Der aktuelle Prototyp ist eine lokale WPF-Anwendung für Windows. Produktiv/praktisch validiert ist nur der Workflow MEDISTAR + NIDEK ARK1S. Weitere V2-Profile und Automatikbausteine sind vorbereitet bzw. im Prototyp bedienbar, aber nicht im gleichen Umfang produktiv validiert.

### 1.1 Manuelle Verarbeitung

Der manuelle Test- und Diagnosemodus verarbeitet Dateien über die Oberfläche:

1. AIS-GDT-Datei auswählen
2. NIDEK ARK1S XML-Datei auswählen
3. Patientendaten und Messwerte einlesen
4. Mapping anwenden
5. XDT-Exportvorschau erzeugen
6. Exportdatei manuell in einen Zielordner schreiben

Der fachlich validierte Ablauf ist aktuell:

- AIS: MEDISTAR
- Gerät: NIDEK ARK1S / AR-1s
- Eingabe AIS: GDT/XDT-Datei
- Eingabe Gerät: XML-Datei
- Ausgabe: XDT-Datei für MEDISTAR
- Steuerfeld: `8000 = 6310`
- Untersuchungsart: `8402` aus AIS/GDT
- Ergebnis: zwei `6228`-Textzeilen für rechts und links

### 1.2 Manuell startbare periodische Überwachung

Zusätzlich zum manuellen Testmodus besitzt der Prototyp eine manuell startbare Überwachung im Tab `Verarbeitung`.

Aktueller Stand:

- Die Überwachung startet nicht beim App-Start.
- Es gibt keinen Windows-Dienst.
- Es gibt keinen Autostart.
- Es gibt keinen `FileSystemWatcher`.
- Die Überwachung basiert auf periodischem Scan aktiver Schnittstellenprofile.
- Gefundene AIS-/Geräte-Dateipaare werden angezeigt.
- Dateien werden nur berücksichtigt, wenn sie stabil und lesbar sind.
- Es wird keine unbekannte Datei gelöscht oder verschoben.

### 1.3 Optionale automatische Verarbeitung

Während die manuell gestartete Überwachung läuft, kann der Benutzer die Option `Gefundene Dateipaare automatisch verarbeiten` aktivieren.

Wenn diese Option deaktiviert ist, werden Dateipaare nur angezeigt und müssen manuell verarbeitet werden.

Wenn diese Option aktiviert ist:

- stabile gefundene Dateipaare werden nacheinander verarbeitet
- XDT-Dateien werden in den konfigurierten Exportordner geschrieben
- bereits verarbeitete Dateipaare werden nicht erneut exportiert
- Importdateien werden nur gemäß Schnittstellenprofil archiviert
- Fehlerdateien werden nur gemäß Schnittstellenprofil in den Fehlerordner kopiert

### 1.4 Archivierung, Duplikate und Fehlerablage

Der Prototyp unterstützt:

- Archivierung verarbeiteter Importdateien per Kopie oder Verschieben
- Duplikatbehandlung für bereits verarbeitete AIS-/Geräte-Dateipaare
- optionale Fehlerablage mit Kopie der beteiligten Importdateien
- `error.txt` im Fehlerordner
- vorbereitete Archiv-Aufbewahrungslogik ohne automatische Ausführung
- Konfigurationsbereich `XDT-Anhänge für AIS` mit optionalen Import-/Exportordnern, `AttachmentFileNameTemplate`, vorbereitetem `AttachmentTransferService` mit Copy/Move-Modus, Standard `Move`, und vorbereiteten XDT-Linkfeld-Vorlagen 6302/6303/6304/6305, noch ohne produktive Dateianhang-Zuordnung

Sicherheitsgrenzen:

- keine pauschale Ordnerleerung
- keine Exportordner-Bereinigung
- keine unbekannten Dateien anfassen
- keine endgültige Löschung von Importdateien
- keine produktive Geräte-Dateianhang-Verarbeitung und keine produktive externe AIS-Link-Übergabe

Konservativ integriert ist außerdem eine automatische XDT-Anhang-Vorbereitung im bestehenden Automatiklauf: Sie wird nur geprüft, wenn die Überwachung manuell läuft, globale automatische Verarbeitung aktiv ist, das Schnittstellenprofil XDT-Anhänge aktiviert hat, eine AIS-Patientennummer vorhanden ist und genau ein unterstützter Anhangkandidat gefunden wird. Die Vorbereitung kann den explizit ausgewählten Kandidaten über den vorbereiteten Transfer-/Linkfeldpfad bearbeiten. Bei erfolgreicher Vorbereitung werden die semantischen Felder `6302`, `6303`, optional `6304` und `6305` transient an die Exportfeldliste angehängt und über den zentralen XDT-Exportmechanismus mit korrektem Längenpräfix in die erzeugte XDT-Datei geschrieben. Bei deaktivierter Funktion, mehreren unterstützten Anhängen, fehlender Eindeutigkeit oder Fehlern bleibt der bestehende Export unverändert.

Für das Warten auf vollständige Verarbeitungspakete ist ein zweistufiges Modell vorbereitet. Ein Paket besteht aus AIS-Datei, Gerätedatei und optionalem oder verpflichtendem XDT-Anhang. Phase 1: Eine erkannte stabile AIS-Datei wartet auf eine stabile Gerätedatei; die Wartezeit ist pro Schnittstellenprofil konfigurierbar, Standard `10` Minuten. Kommt vor der Gerätedatei eine neuere AIS-Datei, ersetzt sie den wartenden Auftrag. Phase 2: Erst ab vollständigem AIS-/Geräte-Dateipaar startet die XDT-Anhang-Wartezeit. Standard ist `optional`, die Standard-Wartezeit beträgt 30 Sekunden. Optional bedeutet: Nach Ablauf der Wartezeit darf die Messwertverarbeitung ohne Anhang fortgesetzt werden. Pflicht bedeutet: Ohne eindeutigen Anhang wird die Verarbeitung blockiert. Mehrere unterstützte Anhänge bleiben immer unsicher und werden nicht automatisch zugeordnet.

Für langsam schreibende Geräte ist eine Stabilitätsprüfung vorbereitet. Der XDT-Anhang-Scanner kann unterstützte Kandidaten mit Stabilitätsstatus markieren; instabile Kandidaten werden nicht automatisch ausgewählt und vom Vorbereitungspfad nicht verschoben oder kopiert, wenn der Instabilitätsstatus bekannt ist. Die Stabilitätswartezeit ist pro Schnittstellenprofil konfigurierbar, Standard `2` Sekunden. Das periodische Ordnerabfrage-Intervall ist ebenfalls pro Schnittstellenprofil vorbereitet, Standard `5` Sekunden; die manuell gestartete Überwachung nutzt weiterhin periodischen Scan und keinen `FileSystemWatcher`.

### 1.5 Lizenzanzeige

Der Lizenzbereich zeigt den lokalen Lizenzstatus, Lizenzanfragen, importierte Lizenzdateien, lizenzierte Geräte/Anbindungen und Karenzzeiten für neue Anbindungen.

Aktueller Stand:

- aktive lizenzpflichtige Schnittstellenprofile werden bewertet
- Karenzzeiten sind modelliert und können aktualisiert werden
- keine Online-Lizenzierung
- keine digitale Signaturprüfung
- keine harte produktive Lizenzsperre

### 1.6 NIDEK LM7/LM7P architektonisch

NIDEK LM7/LM7P ist als vorbereitetes Lensmeter-Profil eingeordnet.

Architekturannahme:

- Das Gerät schreibt per LAN/WLAN über SMB/Common Internet File System XML-Dateien in einen Shared Folder.
- Dieser Shared Folder entspricht dem Geräte-Importordner eines Schnittstellenprofils.
- Die App liest diese Dateien über periodischen Scan bzw. eine spätere sichere Ordnerüberwachung.
- Nach erfolgreicher Verarbeitung ist Archivmodus `Move` fachlich sinnvoll, damit der Shared Folder wieder frei wird.
- Unterstützt werden sollen LAN-XML-Dateien nach `NIDEK_V1.00` und `NIDEK_V1.01`.
- XML-Attribute wie `@base`, `@unit` und `Measure[@Type='LM']` müssen als SourcePaths verfügbar bleiben.

Die zentrale Logik liegt derzeit in Core- und Infrastructure-Bausteinen:

- `GdtParser`
- `PatientDataMapper`
- `XmlDeviceParser`
- `MappingEngine`
- `XdtExportBuilder`
- `DefaultDeviceProfiles`
- `ProcessingPipelineService`
- `AutoImportScannerService`
- `PeriodicAutoImportScanService`
- `InterfaceProfileManualProcessor`
- `ProcessedFileArchiveService`
- `FailedFileCopyService`
- `AutoImportPairProcessingCoordinator`
- `DuplicateImportFileHandler`

Diese Struktur ist für den Prototyp ausreichend und bildet die Grundlage für weitere Profile. Für produktive Nutzung weiterer AIS-Systeme, Geräteklassen, Templatepakete und Lizenzdurchsetzung sind noch Validierung und Ausbauschritte erforderlich.

---

## 2. Zielarchitektur

Version 2 soll die festen Annahmen des Prototyps in klar getrennte Profil- und Konfigurationsmodelle überführen. Dadurch können weitere AIS/PVS-Systeme, Geräte und Exportformate ergänzt werden, ohne die Produktivlogik für jedes Gerät neu zu verdrahten.

### 2.1 AISProfile

`AISProfile` beschreibt, wie ein Zielsystem Rückgabedateien erwartet.

Typische Inhalte:

- Profil-ID
- Name des AIS, z. B. MEDISTAR, ALBIS, TURBOMED
- erwartete Satzart, z. B. `8000 = 6310`
- Zeichensatz, z. B. `Windows-1252`
- Pflichtfelder
- unterstützte Zielfeldkennungen
- Regeln für Untersuchungsarten, z. B. Verwendung von `8402`
- Vorgaben für Ergebnisfelder, z. B. `6228`, `6330` bis `6399`, `8410` bis `8480`
- Ausgabeart je Untersuchungsart: Textzeile, Einzelwert, Kategorie/Wert-Paar oder Ergebnisblock

### 2.2 DeviceProfile

`DeviceProfile` beschreibt, wie Gerätedateien gelesen und Messwerte erkannt werden.

Typische Inhalte:

- Profil-ID
- Gerätename
- Hersteller
- Modell oder Gerätetyp
- Dateiformat
- Parsermodus: XML, XPath, Regex, CSV, Text, GDT/XDT
- Geräte-Importordner
- erkannte Messwertpfade
- Standardmesswerte
- mögliche Untersuchungsarten
- Information, ob eine Datei mehrere Untersuchungsarten enthalten kann
- Aktiv-/Lizenzpflichtig-Kennzeichen

### 2.3 ExportProfile

`ExportProfile` beschreibt, wie aus AIS-Daten und Gerätewerten Ausgabe-Datensätze entstehen.

Typische Inhalte:

- Profil-ID
- Name
- zugehöriges AIS-Profil
- zugehöriges Geräteprofil
- Mapping-Regeln
- feste Ausgabewerte, z. B. `8000 = 6310`
- durchgereichte AIS-Werte, z. B. `8402`
- zusammengesetzte Ergebniszeilen, z. B. `6228`
- Ausgabe-Syntax pro Zeile
- Ausgabe-Syntax pro Untersuchungsart
- Sortierung der ExportRecords
- Zielzeichensatz

Ein `ExportProfile` muss denselben Ziel-Feldcode mehrfach verwenden können, z. B. zwei `6228`-Zeilen für rechte und linke Messwerte.

### 2.4 InterfaceProfile / Kombiprofil

`InterfaceProfile` ist das produktiv nutzbare Kombiprofil. Es verbindet:

- ein `AISProfile`
- ein `DeviceProfile`
- ein `ExportProfile`
- Ordnerkonfiguration
- Archiv-/Fehlerordner
- Bereinigungsoptionen
- Aktivierungsstatus
- Lizenzstatus je Geräteanbindung

Beispiel:

```text
InterfaceProfile: MEDISTAR + NIDEK ARK1S
AISProfile: MEDISTAR
DeviceProfile: NIDEK ARK1S
ExportProfile: MEDISTAR 6228 Refraktion
```

### 2.5 TemplatePackage

`TemplatePackage` beschreibt exportierbare und importierbare Konfigurationspakete.

Ein Paket kann enthalten:

- ein einzelnes AIS-Template
- ein einzelnes Geräte-Template
- ein einzelnes Export-/Mapping-Template
- ein vollständiges Kombi-Template
- mehrere Profile als Gesamtpaket

Typische Metadaten:

- Paket-ID
- Name
- Beschreibung
- Version
- Ersteller
- Erstellungsdatum
- kompatible App-Version
- enthaltene Profile
- Hinweise zu Hersteller/Gerät/AIS

### 2.6 LicenseState

`LicenseState` beschreibt den lokalen Lizenzzustand der Installation oder App-Instanz.

Typische Inhalte:

- Installations-ID
- Lizenz-ID
- Lizenztyp, z. B. Test, Monatlich, Jahreslizenz
- Testphase aktiv ja/nein
- verbleibende Testtage
- lizenzierte Geräteanzahl
- aktiv eingerichtete lizenzpflichtige Geräteanzahl
- Lizenzbeginn
- Lizenzende
- Signaturstatus
- letzter Prüfzeitpunkt
- Hinweise auf Überschreitung oder Ablauf

Die Lizenzierung soll in der ersten marktfähigen Ausbaustufe offline funktionieren. Die App prüft eine signierte Lizenzdatei lokal mit einem öffentlichen Prüfschlüssel.

### 2.7 Mehrere Untersuchungsarten pro Gerätedatei

Die V2-Architektur muss Geräte unterstützen, deren Messdateien mehrere Untersuchungsarten enthalten. Das betrifft insbesondere kombinierte oder umfangreiche XML-Formate.

Beispiele aus den vorliegenden Beispieldaten:

- NIDEK AR1S: Autorefraktion und PD
- NIDEK LM7: Lensmeter/Scheitelbrechwertmesser mit Sphäre, Zylinder, Achse, Addition, Prisma, Basisrichtung und PD
- NIDEK NT530P: Tonometrie, Pachymetrie, korrigierter IOP und Bild-/Protokollverweise
- TOPCON CL300: Lensmeterdaten im Ophthalmology-/JOIA-XML-Format
- TOPCON KR800: Refraktion, Keratometrie, subjektive Refraktions-/VA-/PD-Daten
- TOPCON TRK2P: Tonometrie und Pachymetrie/CCT

Ein `DeviceProfile` muss dafür künftig beschreiben können:

- welche Untersuchungsarten in einer Datei erkannt werden können
- welche Untersuchungsarten aktiv ausgelesen werden sollen
- welche Untersuchungsarten ignoriert werden sollen
- ob Untersuchungsarten getrennt oder zusammengefasst exportiert werden
- welche Messwertgruppen zu welcher Untersuchungsart gehören

Beispiele für kombinierte Gerätedateien:

- TOPCON KR800 enthält `REF`, `KM` und `SBJ`.
- TOPCON TRK2P enthält `TM` und `CCT`.
- NIDEK NT530P enthält `NT` und `PACHY`.

### 2.8 Mehrere Ergebnis-Templates pro Profil

Ein `ExportProfile` muss mehrere Ergebnis-Templates für eine Gerätedatei enthalten können. Diese Templates können denselben Ziel-Feldcode mehrfach nutzen oder unterschiedliche Ziel-Feldcodes beschreiben.

Beispiele:

- ARK1S:
  - `6228` Ergebnis rechts
  - `6228` Ergebnis links
- LM7:
  - Lensmeter rechts mit Sphäre/Zylinder/Achse/Prisma/PD
  - Lensmeter links mit Sphäre/Zylinder/Achse/Prisma
- NT530P:
  - Pachymetrie rechts
  - Pachymetrie links
  - Tonometrie/korrigierter IOP rechts/links
  - perspektivisch externer AIS-Link auf Messprotokoll
- KR800:
  - Refraktion rechts/links
  - Keratometrie rechts/links
  - optional subjektive/VA-/PD-Daten

Die Ausgabe-Syntax muss pro Untersuchungsart und pro Ergebniszeile konfigurierbar sein.

### 2.9 Geräte-Dateianhang-Verarbeitung und externe Link-Übergabe

Die V2-Architektur soll verbindlich eine Geräte-Dateianhang-Verarbeitung mit externer AIS-Link-Übergabe erhalten. Dieser Baustein ist Zielarchitektur und im Stand `0.1.0-prototype` noch nicht produktiv implementiert.

Ziel ist, bereits vom Untersuchungsgerät erzeugte Dateien, z. B. PDF, JPG, PNG, TIF, DCM oder TXT, aus einem definierten Importordner zu übernehmen, eindeutig zu benennen, in einen AIS-erreichbaren Zielordner zu übertragen und in der XDT-Rückgabe als externer AIS-Link zu referenzieren.

Vorgesehene Konzepte:

- `AttachmentImportFolder`: GA-Dateianhang Import
- `AttachmentExportFolder`: GA-Dateianhang Export
- `AttachmentFileNameTemplate`, Standard: `{Ais.PatientNumber}_{Date:ddMMyyyy}_{Time:HHmmss}{ExtensionUpper}`
- `IsAttachmentProcessingEnabled`: vorbereitete Einschaltfunktion pro Schnittstellenprofil, Standard `false`; spätere Nutzung nur bei manuell gestarteter Überwachung, aktivierter automatischer Verarbeitung und vorhandener AIS-Patientennummer
- `AttachmentHandlingMode`: `None`, `Optional`, `Required`
- `AttachmentTransferMode`: `Copy`, `Move`; Standard ist `Move`, damit der GA-Dateianhang-Importordner nach erfolgreicher Übernahme sauber bleiben kann
- `AttachmentRequirementMode`: `Optional`, `Required`; Standard ist `Optional`
- `AttachmentWaitTimeoutSeconds`: Wartezeit auf einen erwarteten XDT-Anhang, Standard `30`, `0` bedeutet sofort prüfen
- `DeviceFileWaitTimeoutMinutes`: Wartezeit einer AIS-Datei auf eine stabile Gerätedatei, Standard `10`, `0` bedeutet sofort ablaufen lassen
- `AttachmentFileStabilityWaitSeconds`: Zeitraum, über den ein XDT-Anhang unverändert und lesbar bleiben muss, Standard `2`
- `AutoImportScanIntervalSeconds`: Intervall der periodischen Ordnerabfrage, Standard `5`, Minimum `1`
- `AttachmentLinkExport` über XDT-Felder
- `AttachmentExternalLinkDocumentName`: Vorlage für 6302 Dokumentenname
- `AttachmentExternalLinkFileFormat`: Vorlage für 6303 Dateiformat
- `AttachmentExternalLinkDescription`: Vorlage für 6304 Beschreibung
- `AttachmentExternalLinkPathTemplate`: Vorlage für 6305 vollständiger Dateipfad, typischerweise `{Attachment.TargetFullPath}`
- `AttachmentFileNameBuilder`
- `AttachmentTransferService`, isoliert vorbereitet für sichere Übertragung einzelner explizit übergebener Anhangdateien ohne Überschreiben
- `ExternalAisLinkFieldBuilder`, isoliert vorbereitet für semantische Feldwerte zu `6302`, `6303`, `6304` und `6305`
- `ExternalAisLinkXdtFieldAdapter`, isoliert vorbereitet für die Überführung dieser semantischen Werte in XDT-Feldcode/Wert-Paare `6302` bis `6305`, weiterhin ohne Längenpräfix
- `AttachmentExternalLinkPreparationService`, isoliert vorbereitet für die explizite Orchestrierung aus Ziel-Dateiname, Copy/Move-Transfer, externen AIS-Linkfeldwerten und XDT-Feldcode/Wert-Paaren; in der automatischen Paarverarbeitung nur konservativ bei eindeutig genau einem unterstützten Anhang eingebunden
- `AttachmentAutoProcessingEligibilityService`, isoliert vorbereitet für die sichere Vorprüfung, ob eine spätere automatische XDT-Anhang-Verarbeitung grundsätzlich erlaubt wäre; der Service führt keine Dateioperationen aus
- `AttachmentImportFolderScannerService`, isoliert vorbereitet für das reine Auflisten unterstützter Dateien im XDT-Anhang Importordner; der Service scannt nur die oberste Ordnerebene, markiert unterstützte Dateien optional mit Stabilitätsstatus und verändert, verschiebt oder löscht keine Dateien
- `AttachmentImportFolderDiagnosticService`, isoliert vorbereitet für den manuellen Diagnosebereich; er ruft den Scanner mit dem konfigurierten Importordner auf, erzeugt anzeigbare Kandidatenzeilen und startet keine Verarbeitung
- `AttachmentAutoCandidateSelectionService`, isoliert vorbereitet für die sichere automatische Kandidatenentscheidung; automatisch eindeutig ist nur genau eine stabile unterstützte Anhangdatei, mehrere unterstützte oder instabile Dateien blockieren die automatische Auswahl wegen unsicherem Patientenbezug bzw. unvollständigem Schreibvorgang
- `AttachmentPackageDecisionService`, isoliert vorbereitet für die Entscheidung, ob auf einen optionalen oder verpflichtenden XDT-Anhang gewartet, ohne Anhang fortgesetzt, mit eindeutigem Anhang verarbeitet oder wegen fehlender Eindeutigkeit blockiert werden soll
- `AutoImportPackageStateService`, isoliert vorbereitet für die Paket-Phase vor der Verarbeitung: AIS wartet auf Gerätedatei, neuere AIS-Dateien ersetzen ältere wartende Aufträge, abgelaufene AIS-Aufträge werden nur als Status markiert und nicht blind gelöscht
- manueller Diagnosepfad im Tab `Verarbeitung`, der eine explizit ausgewählte Anhangdatei testweise vorbereitet und Ziel-Dateiname, Zielpfad sowie Feldcode/Wert-Paare `6302` bis `6305` anzeigt; er ist nicht Teil des periodischen Scans und nicht in die produktive Exportdatei eingebunden
- `ExternalLinkExportRule` oder vergleichbare Exportregel
- Validierung über `FolderSafetyValidator`
- Kollisionsschutz bei Dateinamen

Die Modelle müssen beschreiben:

- welche Geräte-Dateianhänge erwartet werden
- wie Anhänge gefunden werden, z. B. über Dateinamen, Zeitstempel oder XML-Verweise wie `PACHYImage`
- welche Dateiendungen erlaubt sind
- ob ein Anhang optional oder Pflicht ist
- ob die Datei kopiert oder verschoben wird
- wie der Zielname gebildet wird
- wie vorhandene Zieldateien ohne Überschreiben mit Suffix behandelt werden
- welcher externe AIS-Link daraus entsteht

### 2.10 MEDISTAR externer Link über XDT

Für MEDISTAR soll die externe Link-Übergabe über XDT-Feldkennungen modelliert werden. Die genaue AIS-Wirkung ist mit MEDISTAR zu validieren; fachlich vorgesehen sind:

| Feldkennung | Bedeutung |
| --- | --- |
| `6302` | Dokumentname / Anzeige in der Karteikarte |
| `6303` | Dateiformat, z. B. `PDF`, `JPG`, `DCM`, `TXT` |
| `6304` | optionale Beschreibung |
| `6305` | vollständiger absoluter Dateipfad zur abgelegten Datei |

Der vorbereitete `ExternalAisLinkFieldBuilder` erzeugt semantische Feldwerte, z. B. `6302 -> PDF-Befund`, `6303 -> PDF`, `6304 -> Messprotokoll` und `6305 -> \\SERVER\Freigabe\Datei.pdf`. Der `ExternalAisLinkXdtFieldAdapter` überführt diese Werte in Feldcode/Wert-Paare ohne Längenpräfix. Bei erfolgreicher konservativer automatischer XDT-Anhang-Vorbereitung werden diese Paare an die Exportfeldliste angehängt; der zentrale `XdtExportBuilder` erzeugt daraus die finalen XDT-Zeilen mit korrektem Längenpräfix. Die App soll die Zeilenlänge nicht manuell im UI pflegen lassen.

Ein externes Link-Template darf keine echte Patientenakte als Beispiel enthalten. Beispiele in Dokumentation und Templatepaketen müssen synthetisch sein.

Aus der ausgewerteten Datei `XDT Übergabe externer Link.txt` ergibt sich für MEDISTAR außerdem: Die XDT-Datei enthält die Felder `6302` bis `6305`; die spätere sichtbare Karteikartenzeile mit `EV:{...}` wird von MEDISTAR nach dem Import erzeugt. Die App soll daher den Link über strukturierte XDT-Felder exportieren und keine MEDISTAR-interne `EV:{...}`-Anzeigezeile konstruieren.

### 2.11 Selbst erzeugte PDF-Messprotokolle

Die spätere PDF-Erzeugung durch die App bleibt ein eigener Erweiterungsfall. Sie ist von der Geräte-Dateianhang-Verarbeitung zu unterscheiden:

- Geräte-Dateianhang: Das Gerät liefert bereits eine Datei, die die App übernimmt und verlinkt.
- PDF-Erzeugung: Die App erzeugt selbst aus AIS- und Gerätedaten ein lesbares Messprotokoll.

Für selbst erzeugte Protokolle sind perspektivisch folgende Konzepte sinnvoll:

- `GeneratedDocumentDefinition`
- `DocumentTemplate`
- `DocumentLayoutSection`
- `DocumentStorageOptions`
- `DocumentGenerationResult`
- `DocumentGenerationIssue`

Ein erzeugtes PDF soll aus `PatientData`, AIS-Kontext, Geräteprofil und `MeasurementValues` aufgebaut werden können. Die Dokumenterzeugung darf nur dann die Verarbeitung blockieren, wenn sie im Profil ausdrücklich als Pflicht markiert ist.

Für die aktuelle Version gilt: keine produktive Geräte-Dateianhang-Verarbeitung, keine produktive externe AIS-Link-Übergabe und keine PDF-Erzeugung durch die App.

### 2.12 XML-Parser und Namespaces

Die Architektur muss unterschiedliche XML-Varianten unterstützen.

Beispiele:

- NIDEK XML ohne komplexe Namespaces
- TOPCON/Ophthalmology XML mit JOIA-Namespaces

Geräteprofile müssen Parseroptionen definieren können:

- Namespace ignorieren
- Namespace beibehalten
- bekannte Namespace-Präfixe verwenden
- XPath-/Pfadmodus

SourcePaths sollen für Anwender lesbar bleiben. Namespace-Details dürfen den späteren Mapping-Editor nicht unbedienbar machen.

---

## 3. Datenfluss

Der geplante Datenfluss trennt Eingabe, Kontext, Mapping und Ausgabe klar voneinander.

```text
AIS-GDT
  -> PatientData / AisContext

Gerätedatei
  -> MeasurementValues

AISProfile + DeviceProfile + ExportProfile
  -> Mapping
  -> ExportRecords

ExportRecords
  -> XDT-Datei

zukünftige Geräte-Dateianhänge
  -> AttachmentLinkExport / ExternalLinkExportRule
  -> GA-Dateianhang Export / externer AIS-Link

optional erzeugtes Messprotokoll
  -> DocumentTemplate + MeasurementValues + PatientData
  -> PDF-Dokument
  -> Dokumentablage / optionaler externer AIS-Link
```

### 3.1 AIS-GDT zu PatientData/AisContext

Die AIS-Datei wird durch einen GDT/XDT-Parser gelesen. Daraus entstehen:

- `PatientData`: Patientennummer, Name, Geburtsdatum und weitere Patientendaten
- `AisContext`: AIS-spezifische Kontextwerte, z. B. Untersuchungsart `8402`, Quellsystem, Zielsystem, GDT/XDT-Version

### 3.2 Gerätedatei zu MeasurementValues

Die Gerätedatei wird anhand des `DeviceProfile` interpretiert. Je nach Parsermodus entstehen normalisierte `MeasurementValues`.

Beispiele:

- `R/AR/ARMedian/Sphere`
- `R/AR/ARMedian/Cylinder`
- `L/AR/ARMedian/Sphere`
- `PD/PDList[@No='1']/FarPD`

### 3.3 Profile/Mapping zu ExportRecords

Das `ExportProfile` verbindet AIS-Kontext und Messwerte zu geordneten ExportRecords.

Ein ExportRecord enthält typischerweise:

- Feldkennung
- Wert
- Sortierung
- optional Anzeige-/Debug-Informationen

Beispiele:

- `8000 = 6310`
- `3000 = Patientennummer`
- `8402 = ARK1S`
- `6228 = R.:S=... PD=...`
- `6228 = L.:S=... PD=...`

### 3.4 ExportRecords zu XDT-Datei

Der Exportgenerator schreibt aus den ExportRecords eine XDT-konforme Datei:

- korrekte Feldlängen
- definierter Zeichensatz
- definierte Zeilenreihenfolge
- Zielordner aus dem InterfaceProfile
- optional Archivierung nach erfolgreicher Verarbeitung
- keine pauschale Exportordner-Bereinigung

XDT-Zeilen für externe Links müssen denselben Exportweg nutzen. Auch Felder wie `6302`, `6303`, `6304` und `6305` dürfen nicht als manuell vorformatierte Zeilen mit händisch gepflegtem Längenpräfix in der UI landen.

Schematische Ausgabe:

```text
<Len>6302PDF-Befund
<Len>6303PDF
<Len>6304Messwerte Autorefraktor
<Len>6305\\SERVER\Freigabe\Befunde\Patient_123.pdf
```

`<Len>` ist bewusst symbolisch dargestellt, weil die konkrete Länge vom `XdtExportBuilder` berechnet werden muss.

### 3.5 Geräte-Dateianhänge zu Dokumentablage und externem AIS-Link

Für spätere Geräteprofile mit Bild- oder Protokolldateien muss der Datenfluss um Geräte-Dateianhänge erweitert werden.

Die App soll zugehörige Dateien anhand von Dateinamen, Zeitstempel oder XML-Verweisen erkennen können. Ein Dokument-/Dateianhang-Template erzeugt daraus strukturierte Link-Exportinformationen.

Diese Informationen können anschließend:

- Dateien aus `GA-Dateianhang Import` übernehmen
- Dateien in `GA-Dateianhang Export` kopieren oder verschieben
- eindeutige Dateinamen erzeugen
- fehlende Pflichtdateien als Fehler melden
- fehlende optionale Dateien als Warnung protokollieren
- MEDISTAR externe Links über XDT-Felder `6302`, `6303`, `6304` und `6305` oder andere AIS-Dokumentverweise erzeugen
- keine externen Links erzeugen, wenn die Datei nicht erfolgreich abgelegt wurde

### 3.6 Selbst erzeugte PDF-Messprotokolle

Wenn ein Gerät nur maschinenlesbare Werte liefert, kann die App perspektivisch selbst ein PDF-Messprotokoll erzeugen.

Der Datenfluss besteht dann aus:

1. AIS- und Patientendaten einlesen
2. Gerätedaten normalisieren
3. Dokumenttemplate des Interface-/Exportprofils auswählen
4. Messwerte und Patientenkontext in Abschnitte, Tabellen und Beschriftungen übertragen
5. PDF-Datei erzeugen
6. PDF in den konfigurierten Dokumentordner schreiben
7. optional einen externen AIS-Link in den XDT-Export aufnehmen

Die Dokumenterzeugung ist optional und darf nur dann die Verarbeitung blockieren, wenn sie im Profil ausdrücklich als Pflicht markiert ist.

---

## 4. Speicherorte

### 4.1 Lokale Workstation

Für Einzelplatz- und normale Workstation-Installationen sollen Konfigurationen lokal gespeichert werden.

Geeignete Speicherorte:

- `%AppData%`
- `%LocalAppData%`
- optional später `ProgramData` für maschinenweite Profile

Die Anwendung soll ohne zwingende Administratorrechte lauffähig bleiben.

### 4.2 Terminalserver-Benutzerprofil

In Terminalserver- und Remote-Desktop-Umgebungen müssen Konfigurationen benutzerbezogen getrennt werden.

Anforderungen:

- Speicherung pro Windows-Benutzerprofil
- keine versehentliche Überschreibung anderer Benutzerkonfigurationen
- getrennte Installations-/Instanzkennung je Benutzer oder App-Instanz
- nachvollziehbare Zuordnung von Benutzer, App-Instanz und aktiven Geräteprofilen

### 4.3 Exportierbare Konfigurationspakete

Profile und Templates sollen als Datei oder Paket exportiert werden können.

Mögliche Formate:

- JSON-Datei für einzelne Profile
- ZIP-Paket mit mehreren JSON-Dateien
- später optional signierte oder versionierte Pakete

Konfigurationspakete dürfen keine Patientendaten enthalten.

---

## 5. Konfigurationsimport und -export

Version 2 soll Konfigurationen importieren und exportieren können.

Exportierbar:

- AISProfile
- DeviceProfile
- ExportProfile
- InterfaceProfile
- TemplatePackage
- optional Gesamtpaket aller Profile und Templates

Beim Import muss die App:

- Inhalt vor dem Import anzeigen
- enthaltene Profile und Templates auflisten
- Konflikte erkennen
- vorhandene Profile nicht ungefragt überschreiben
- Import als Kopie ermöglichen
- Ersetzen vorhandener Profile bewusst bestätigen lassen
- ungültige oder fehlende Pfade erkennen
- gefährliche Löschoptionen nach Import deaktiviert oder prüfpflichtig lassen

Nach einem Import müssen lokale Pfade geprüft werden. Das betrifft besonders:

- AIS-Importordner
- Geräte-Importordner
- Exportordner
- Archivordner
- Fehlerordner

---

## 6. Offline-Lizenzierung

Die erste marktfähige Lizenzversion soll offline funktionieren.

Iststand `0.1.0-prototype`: Die App kann Installationsinformationen anzeigen, eine Lizenzanfrage exportieren, eine Lizenzdatei importieren und aktive lizenzpflichtige Schnittstellenprofile bewerten. Die Lizenz wird noch nicht digital signiert geprüft und die produktive Verarbeitung wird noch nicht hart gesperrt.

Grundprinzip:

- Die App erzeugt eine Installations-ID.
- Die App erzeugt einen Lizenz-Anfragecode oder eine Anfrage-Datei.
- Der Hersteller erzeugt eine signierte Lizenzdatei.
- Die App prüft die Lizenzdatei lokal.
- Der private Signaturschlüssel bleibt ausschließlich beim Hersteller.
- Die App enthält nur den öffentlichen Prüfschlüssel.

Lizenzrelevant ist die Anzahl aktiver lizenzpflichtiger Geräteprofile, nicht die reine Installation.

Die Lizenzprüfung muss folgende Zustände abbilden können:

- Testphase aktiv
- gültige Lizenz
- abgelaufene Lizenz
- ungültige Signatur
- zu wenige lizenzierte Geräte
- Lizenz fehlt

Zielbild für eine spätere produktive Lizenzdurchsetzung: Bei ungültiger oder abgelaufener Lizenz darf keine produktive Verarbeitung erfolgen. Konfiguration, Import/Export und Lizenzaktivierung müssen weiterhin möglich bleiben.

---

## 7. Spätere UI-Bereiche

Die Oberfläche soll perspektivisch folgende Bereiche enthalten:

- Start/Verarbeitung
- AIS-Profile
- Geräteprofile
- Export-/Mapping-Profile
- InterfaceProfile / Kombiprofile
- Template-Bibliothek
- Import/Export von Konfigurationen
- Ordner- und Bereinigungsoptionen
- Lizenzbereich
- Protokolle und Fehler

Der Lizenzbereich soll mindestens anzeigen:

- Lizenzstatus
- Testphase
- verbleibende Testtage
- Installations-ID
- Anfragecode erzeugen
- Lizenzdatei importieren
- lizenzierte Geräteanzahl
- aktiv eingerichtete lizenzpflichtige Geräteanzahl
- Lizenzablaufdatum

---

## 8. Implementierte V2-Bausteine

Die folgenden Architekturbausteine sind inzwischen als Grundlage für Version 2 implementiert. Einige davon sind bereits in der Oberfläche des Prototyps nutzbar, andere bleiben vorbereitende Bausteine. Produktiv/praktisch validiert bleibt nur MEDISTAR + NIDEK ARK1S.

### 8.1 Profilmodelle

Im Core-Projekt sind die zentralen Profilmodelle vorbereitet:

- `ProfileMetadata`
- `AisProfile`
- `DeviceProfileDefinition`
- `ExportProfileDefinition`
- `InterfaceProfileDefinition`
- `TemplatePackage`

Diese Modelle beschreiben künftig AIS-Systeme, medizinische Geräte, Export-/Mappingregeln, vollständige Schnittstellenprofile und wiederverwendbare Templatepakete. Der aktuelle MEDISTAR/NIDEK-ARK1S-Ablauf bleibt davon unabhängig funktionsfähig.

### 8.2 Template- und Konfigurationsaustausch

Im Infrastructure-Projekt sind Bausteine für JSON-Speicherung sowie Import und Export von Konfigurationspaketen vorbereitet:

- `ProfileJsonSerializer`
- `ProfileFileRepository`
- `TemplatePackageExporter`
- `TemplatePackageImporter`
- `TemplatePackageImportValidator`
- `TemplatePackageImportConflictAnalyzer`
- `TemplatePackageImportPlanBuilder`
- `TemplatePackageImportDryRunService`

Damit können V2-Profile und Templatepakete technisch serialisiert, als Dateien gespeichert, als ZIP-Paket exportiert, wieder importiert und vor einer späteren produktiven Übernahme validiert werden.

Der vorbereitete `TemplatePackageImportConflictAnalyzer` analysiert ein importiertes Templatepaket gegen den lokalen Profilkatalog, ohne Profile produktiv zu speichern. Er erkennt ID-/Namenskonflikte, BuiltIn-Schutz, fehlende Schnittstellenprofil-Abhängigkeiten und unsichere Ordnerpfade. BuiltIn-Profile bleiben geschützt; UserDefined-Konflikte können später als Kopie oder bewusster Ersatz behandelt werden. XDT-Anhang-Einstellungen bleiben Bestandteil des Schnittstellenprofils und werden beim Import als prüfpflichtige Konfiguration betrachtet.

Auf Basis dieser Analyse erzeugt der vorbereitete `TemplatePackageImportPlanBuilder` einen noch nicht ausgeführten Importplan. Der Plan schlägt sichere Standardaktionen wie `ImportAsNew`, `ImportAsCopy`, `KeepExisting` oder `Blocked` vor; `ReplaceExisting` bleibt eine spätere bewusste Benutzerentscheidung. BuiltIn-Profile werden nie automatisch ersetzt, UserDefined-Konflikte werden zunächst als Kopie geplant, fehlende Abhängigkeiten blockieren den Import und XDT-Anhang-Einstellungen werden als prüfpflichtig markiert. Der Importplan führt noch keine produktive Übernahme aus und schreibt keine Profile in den lokalen Katalog.

Als nächster vorbereiteter Schritt simuliert der `TemplatePackageImportDryRunService` die spätere Ausführung eines Importplans, ohne Dateien zu schreiben. Der Dry-Run zeigt, welche Profile als neu, als Kopie, als Ersatz, als bestehend beibehalten, übersprungen oder blockiert behandelt würden. Er weist geplante Ziel-IDs und Zielnamen aus und simuliert für Schnittstellenprofile das Abhängigkeits-Remapping auf lokale Profile oder auf im Paket importierte Profile. Fehlende oder blockierte Abhängigkeiten werden sichtbar, XDT-Anhang-Einstellungen bleiben prüfpflichtig, und importierte Schnittstellenprofile werden nicht automatisch aktiviert. Auch dieser Schritt ist nur Vorschau und führt keine produktive Übernahme aus.

### 8.3 Offline-Lizenzierung

Für die spätere Offline-Lizenzierung sind folgende Modelle und Infrastrukturbausteine vorhanden:

- `InstallationInfo`
- `InstallationInfoProvider`
- `LicenseInfo`
- `LicenseFileRepository`
- `LicenseEvaluator`
- `LicensedDeviceStateEvaluator`
- `LicensedDeviceGracePeriodService`
- `LicenseRequest`
- `LicenseRequestBuilder`
- `LicenseRequestFileRepository`

Diese Bausteine ermöglichen eine lokale Installationskennung, das Speichern und Laden einer Lizenzdatei, eine lokale Lizenzbewertung aktiver lizenzpflichtiger Schnittstellenprofile, Karenzzeiten sowie das Erzeugen und Speichern einer Offline-Lizenzanfrage. Eine echte Signaturprüfung und produktive Erzwingung der Lizenz sind weiterhin nicht implementiert.

### 8.4 Lokale App-Daten

Für benutzerbezogene lokale App-Daten ist `AppDataPathProvider` vorbereitet. Der Standardpfad liegt unter:

```text
%LocalAppData%\XdtDeviceBridge
```

Darunter sind Pfade für Profile, Templates, Lizenzen, Logs, Lizenzanfragen, Templatepakete und Installationsinformationen vorgesehen. Die Struktur ist für Workstations und Terminalserver-Benutzerprofile geeignet.

### 8.5 Sicherheit

Mit `FolderSafetyValidator` ist eine Sicherheitsprüfung für spätere bzw. explizite Ordnerbereinigungen vorhanden. Der Validator erkennt gefährliche Cleanup-Pfade wie leere oder relative Pfade, Laufwerks- oder Share-Wurzeln, Windows-Systemordner, Program-Files-Ordner und Benutzerprofil-Wurzeln. Nicht vorhandene, aber ansonsten plausible Unterordner werden als Warnung gemeldet.

Es wurde keine pauschale Ordnerleerung implementiert. Dateioperationen beschränken sich im aktuellen Prototyp auf bekannte verarbeitete Dateipaare, Archivierung Copy/Move, Fehlerablage Copy-only und vorbereitete Archiv-Retention-Logik.

### 8.6 Weiterhin aktueller Prototyp

Der aktuelle funktionsfähige Prototyp umfasst:

- manuelle WPF-App
- MEDISTAR GDT/XDT als AIS-Eingabe
- NIDEK ARK1S XML als Geräteeingabe
- MEDISTAR-kompatibler XDT-Export
- Steuerung über `8000 = 6310`
- Untersuchungsart über `8402`
- Ergebnistext über zwei `6228`-Zeilen
- Schnittstellenprofile mit Ordnerkonfiguration
- manuell startbare periodische Überwachung
- optionale automatische Verarbeitung per bewusst gesetztem Haken
- Archivierung per Kopie oder Verschieben
- Duplikatbehandlung für bereits verarbeitete Dateipaare
- Fehlerablage mit `error.txt`
- Lizenzanzeige ohne harte Sperre

### 8.7 Erkenntnisse aus weiteren Beispieldaten

Die weiteren Beispieldaten zeigen, dass die V2-Architektur über das erste ARK1S-Refraktionsprofil hinausgehen muss. Insbesondere müssen Geräteprofile mehrere Untersuchungsarten pro Datei, mehrere Ergebnis-Templates, unterschiedliche XML-Namespaces und optionale oder verpflichtende Begleitdateien beschreiben können.

Beispielhafte MEDISTAR-Ausgaben aus den Daten:

```text
V0   R.:S=+ 6.50 Z=- 1.75*172 P=0.75 OUT 1.00 UP           PD= 59
V0   L.:S=+ 6.00 Z=- 2.25*  2 P=0.50 OUT 1.50 UP

Y  PR: 559 560 558 [559] µm
Y  PL: 559 560 [560] µm
P  R = 12 11 15 [12.7] // L = 14 13 15 [14.0] mmHg 14:51

V1 R.:S=- 0.25 Z=- 0.25* 49                              PD=61
V1 L.:S=+ 0.00 Z=- 0.50* 63                              PD=61
```

---

## 9. Geräte-Datei-Explorer und Profil-Assistent für unbekannte Geräte

### 9.1 Zielbild

Die V2-Architektur soll perspektivisch einen Geräte-Datei-Explorer und einen Profil-Assistenten unterstützen. Damit soll ein Systembetreuer Beispieldateien unbekannter oder neuer Geräte laden, technisch analysieren und daraus Vorschläge für Geräteprofile, Exportprofile und optional vollständige Schnittstellenprofile ableiten können.

Dieser Assistent ist als exploratives Werkzeug vorgesehen. Er darf die bestehende Verarbeitung bekannter Profile nicht beeinflussen und darf unbekannte medizinische Werte nicht ungeprüft als fachlich korrekt interpretieren.

### 9.2 Unterstützte Eingangstypen

Der Analysepfad soll später mehrere Eingangstypen verarbeiten können:

- XML
- GDT/XDT
- TXT
- CSV
- JSON
- proprietäre Textformate
- Dateien mit Begleitdateien, z. B. JPG/PDF

Die Analyse soll so gekapselt werden, dass bekannte produktive Parser weiterhin stabil bleiben. Neue explorative Parser dürfen nicht direkt in die produktive Pipeline eingreifen.

### 9.3 Zukünftige Architekturbausteine

Für diese Ausbaustufe sind folgende Bausteine vorgesehen:

- `DeviceFileAnalyzer`
- `FileTypeDetector`
- `EncodingDetector`
- `XmlStructureAnalyzer`
- `TextStructureAnalyzer`
- `MeasurementCandidate`
- `DeviceProfileWizard`
- `ProfileSuggestionEngine`

Diese Bausteine sollen später aus unbekannten Dateien Profilvorschläge erzeugen, ohne die bestehende Verarbeitung bekannter Profile zu gefährden.

### 9.4 Verantwortlichkeiten der Bausteine

`FileTypeDetector` erkennt den wahrscheinlichen Dateityp und grenzt XML, GDT/XDT, Text, CSV, JSON und proprietäre Formate voneinander ab.

`EncodingDetector` ermittelt oder vermutet den Zeichensatz, z. B. UTF-8 oder Windows-1252.

`XmlStructureAnalyzer` analysiert XML-Dateien, Namespaces, Attribute, wiederholte Gruppen, Pfade und mögliche Messwertknoten.

`TextStructureAnalyzer` untersucht Text-, CSV- und proprietäre Formate auf Tabellenstrukturen, Key-Value-Muster, Feldtrenner, Zeilenmuster und wiederkehrende Blöcke.

`DeviceFileAnalyzer` koordiniert die Analyse einer oder mehrerer Beispieldateien und erzeugt normalisierte Kandidaten.

`MeasurementCandidate` beschreibt einen technisch erkannten Wert, bevor daraus ein bestätigter Messwert im Geräteprofil wird.

`ProfileSuggestionEngine` erzeugt Vorschläge für Messwertnamen, Augenbezug, Gruppen, Einheiten, Wiederholungsnummern und mögliche Exportverwendung.

`DeviceProfileWizard` führt den Benutzer durch die Auswahl, Benennung, Validierung und spätere Speicherung eines neuen Profils.

### 9.5 MeasurementCandidate

Ein `MeasurementCandidate` soll mindestens folgende Informationen tragen können:

- SourcePath
- Rohwert
- Datentyp-Vermutung
- Gruppe
- Auge rechts/links, falls erkennbar
- mögliche Bedeutung
- erkannte Einheit
- Wiederholungsnummer, falls vorhanden
- Beispielwert
- Herkunftsdatei
- Sicherheit der Erkennung, z. B. erkannt, vermutet oder unklar

Aus `MeasurementCandidate`-Objekten können später bestätigte `DeviceMeasurementDefinition`-Einträge entstehen. Dieser Übergang muss bewusst durch den Benutzer erfolgen.

### 9.6 Vorschlagslogik

Die `ProfileSuggestionEngine` darf technische Namen auf mögliche fachliche Bedeutungen abbilden.

Beispiele:

- Sphere/Sph/Sphäre -> Sphäre
- Cyl/Cylinder/Zylinder -> Zylinder
- Axis/Axe/Achse -> Achse
- PD/FarPD/NearPD -> Pupillendistanz
- IOP/Tension/mmHg -> Augeninnendruck
- Pachy/CCT/µm -> Pachymetrie
- K1/K2/R1/R2/Keratometry -> Keratometrie
- R/Right/OD -> rechts
- L/Left/OS -> links

Diese Vorschläge bleiben unverbindlich. Die Architektur muss abbilden können, ob ein Wert automatisch vorgeschlagen, manuell bestätigt oder bewusst ignoriert wurde.

### 9.7 Ablauf im Profil-Assistenten

Der spätere Assistent soll in kleinen, nachvollziehbaren Schritten arbeiten:

1. Gerätename, Hersteller und Gerätetyp erfassen.
2. Eine oder mehrere Beispieldateien laden.
3. Dateityp, Zeichensatz und Struktur analysieren.
4. Technische Werte als `MeasurementCandidate`-Liste anzeigen.
5. Relevante Werte auswählen.
6. Verständliche Namen vergeben oder Vorschläge übernehmen.
7. Exportziel auswählen, z. B. MEDISTAR, ALBIS oder TURBOMED.
8. Export-Template erstellen oder Vorschlag übernehmen.
9. Exportvorschau mit Beispieldaten prüfen.
10. Profil speichern und optional als Template exportieren.

Die Speicherung eines Profils erfolgt erst nach expliziter Benutzerbestätigung.

### 9.8 Mehrere Beispieldateien

Die Analyse soll mehrere Beispieldateien pro Gerät unterstützen. Dadurch können optionale Werte, fehlende Werte, unterschiedliche Patientenfälle, Wiederholungsmessungen und stabile SourcePaths besser erkannt werden.

Die Architektur soll Unterschiede zwischen Dateien sichtbar machen, z. B.:

- Kandidat kommt in allen Dateien vor
- Kandidat kommt nur in einzelnen Dateien vor
- Wiederholungsnummern variieren
- Einheit oder Datentyp weicht ab
- rechte/linke Werte sind nicht in jeder Datei vollständig

### 9.9 Validierung neuer Profile

Bevor ein aus dem Assistenten erzeugtes Profil produktiv verwendet wird, muss es validiert werden.

Zu prüfen sind mindestens:

- Pflichtwerte vorhanden
- Exportvorschau plausibel
- keine unbekannten Pflichtplatzhalter
- Zielfeldkennungen gültig
- Testexport erzeugbar
- optional Testimport ins AIS

Die Validierung soll Fehler, Warnungen und Hinweise getrennt ausgeben.

### 9.10 Grenzen und Abgrenzung

Der Geräte-Datei-Explorer und Profil-Assistent werden in der aktuellen Version nicht implementiert.

Die aktuelle App unterstützt:

- bekannte Profile
- Anzeige erkannter Werte
- manuelle Entwurfsbearbeitung

Die spätere Automatik darf keine blinde medizinische Interpretation vornehmen. Automatisch erkannte Werte sind Vorschläge und müssen fachlich bestätigt werden. Herstellerformate können sich ändern; Profile müssen nach Geräte- oder Softwareupdates erneut geprüft werden.

---

## 10. Nächste Integrationsschritte

Die nächsten sinnvollen Integrationsschritte sollten klein bleiben und den stabilen Prototyp nicht gefährden:

1. Dokumentation von Iststand und Zielbild weiter konsistent halten.
2. Produktive Übernahme importierter Templatepakete mit Konfliktlösung planen.
3. Vollständigen Profil-Assistenten für unbekannte Geräte konzipieren.
4. Vorbereitete V2-Geräteprofile mit echten Praxisdateien validieren.
5. LM7/LM7P-Error-Tags als Gerätewarnung bzw. `DeviceParseIssue` fachlich spezifizieren.
6. Geräte-Dateianhang-Verarbeitung und MEDISTAR externen Link über XDT als nächsten Baukastenbaustein planen.
7. Installer-/Deployment-Konzept erstellen.
8. Echte Ordnerüberwachung bzw. Windows-Dienst erst nach gesonderter Sicherheitsentscheidung planen.

---

## 11. Abgrenzung und Einführungsstrategie

Der aktuelle Prototyp bleibt funktionsfähig. Die bestehende MEDISTAR/NIDEK-ARK1S-Verarbeitung darf durch die neue Architektur nicht gebrochen werden.

Die neue Architektur soll schrittweise eingeführt werden:

1. bestehendes DefaultDeviceProfile fachlich stabil halten
2. Datenmodelle für AISProfile, DeviceProfile, ExportProfile und InterfaceProfile ergänzen
3. vorhandenes MEDISTAR/NIDEK-Profil in diese Modelle überführen
4. JSON-Speicherung für Profile einführen
5. Import/Export für Konfigurationspakete ergänzen
6. Template-Bibliothek lokal bereitstellen
7. Offline-Lizenzierung integrieren
8. spätere UI-Bereiche ausbauen

Nicht Ziel des ersten Schritts:

- Cloud-Template-Bibliothek
- Online-Lizenzierung
- Herstellerdatenbank
- automatische Synchronisation
- vollständige Profilverwaltung für alle Geräteklassen
- produktive Umsetzung von Geräte-Dateianhang-Verarbeitung und MEDISTAR externem Link über XDT
- selbst erzeugte PDF-Messprotokolle
- Mehruntersuchungs-Profile für KR800, TRK2P, NT530P oder vergleichbare Geräte

Die Geräte-Dateianhang-Verarbeitung ist kein optionaler Gedanke mehr, sondern verbindliches Zielbild einer späteren Ausbaustufe. Sie gehört aber nicht zum produktiven Funktionsumfang des aktuellen `0.1.0-prototype`.

Die Architektur soll so vorbereitet werden, dass diese Erweiterungen später möglich sind, ohne den validierten Prototypen neu schreiben zu müssen.

Die aktuellen Beispieldaten dienen zunächst zur Erweiterung des Pflichtenhefts und der Architektur. Die funktionsfähige MEDISTAR/NIDEK-ARK1S-Verarbeitung bleibt unverändert.

Der aktuelle Prototyp erzeugt weiterhin nur MEDISTAR-kompatible XDT-Ergebniszeilen. Produktive Geräte-Dateianhang-Verarbeitung, MEDISTAR externe Links über XDT und selbst erzeugte PDF-Protokolle sind zukünftige Bausteine.
