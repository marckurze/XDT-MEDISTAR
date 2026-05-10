# Pflichtenheft – XdtDeviceBridge

## 1. Projektbezeichnung

**Projektname:** XdtDeviceBridge  
**Arbeitstitel:** PraxisBridge XDT  
**Zielplattform:** Windows-PC in Arztpraxis / Augenarztpraxis  
**Technologieempfehlung:** .NET 8 oder höher, C#, WPF, xUnit; SQLite als spätere Persistenzoption  
**Betriebsart:** Lokale Desktop-Anwendung; Iststand `0.1.0-prototype`: manuelle Verarbeitung und manuell startbarer periodischer Scan; Zielbild: sichere Ordnerüberwachung

---

## 2. Ziel des Projekts

Ziel ist die Entwicklung einer lokal installierbaren Windows-Anwendung, welche Geräteanbindungen in Augenarztpraxen über GDT/XDT-Dateien vereinfacht.

Die Anwendung soll:

1. eine vom Arztinformationssystem erzeugte GDT/XDT-Datei mit Patientendaten einlesen,
2. eine vom Untersuchungsgerät erzeugte Datei mit Untersuchungswerten einlesen,
3. beide Datenquellen grafisch anzeigen,
4. die Werte über eine konfigurierbare Mapping-Logik zusammenführen,
5. daraus eine GDT/XDT-konforme Ausgabedatei erzeugen,
6. diese Ausgabedatei in einen definierten Exportordner schreiben,
7. unterschiedliche Geräteprofile verwalten, ohne dass für jedes Gerät eine neue Spezialsoftware programmiert werden muss.

Die erste konkret zu unterstützende Gerätekonfiguration ist:

**NIDEK ARK1S / AR-1s Autorefraktometer**

---

## 3. Grundprinzip der Anwendung

Das fachliche Zielbild der Anwendung arbeitet nach folgendem Ablauf:

1. Die App überwacht einen AIS-Importordner.
2. Das AIS legt dort eine `.GDT`- oder `.XDT`-Datei mit Patientendaten ab.
3. Die App liest diese Datei ein und zeigt die Patientendaten an.
4. Die App überwacht anschließend den Geräte-Importordner.
5. Das Untersuchungsgerät legt dort eine Messdatei ab.
6. Die App liest die Gerätedatei ein.
7. Die App zeigt alle erkannten Messwerte grafisch an.
8. Die App wendet die gespeicherten Mapping-Regeln des gewählten Geräteprofils an.
9. Die App erzeugt eine XDT-konforme Ausgabedatei.
10. Die App schreibt die Ausgabedatei in den Exportordner des AIS.
11. Die App protokolliert jeden Verarbeitungsschritt.

Aktueller Iststand `0.1.0-prototype`:

- Manuelle Verarbeitung über Dateiauswahl ist vorhanden.
- Eine manuell startbare periodische Überwachung ist vorhanden.
- Es gibt keinen Windows-Dienst.
- Es gibt keinen Autostart.
- Es gibt keinen `FileSystemWatcher`.
- Automatische Verarbeitung erfolgt nur während der manuell gestarteten Überwachung und nur, wenn der Benutzer den entsprechenden Haken setzt.
- Die dauerhafte Ordnerüberwachung bleibt Zielbild für eine spätere Ausbaustufe.

---

## 4. Nicht-Ziele der Version 1

Version 1 soll bewusst klein und stabil bleiben.

Nicht Bestandteil von Version 1:

- keine Cloud-Anbindung
- keine Mehrbenutzerverwaltung
- keine Netzwerkserver-Komponente
- kein Webportal
- keine automatische Online-Aktualisierung
- keine HL7-/FHIR-Kommunikation
- keine Datenbankverbindung zum AIS
- keine direkte Gerätekommunikation per serieller Schnittstelle, TCP/IP oder USB
- keine medizinische Bewertung der Messwerte
- keine automatische Korrektur medizinischer Werte

Die Anwendung verarbeitet ausschließlich Dateien, die von AIS und Untersuchungsgerät bereitgestellt werden.

---

## 5. Zielumgebung

Die Anwendung muss auf klassischen Windows-Systemen in Arztpraxen lauffähig sein.

### 5.1 Unterstützte Betriebssysteme

Mindestens:

- Windows 10
- Windows 11

Optional später:

- Windows Server 2016 oder höher, sofern die Anwendung auf einem Praxisserver laufen soll

### 5.2 Lokale Rechte

Die Anwendung benötigt Schreib- und Leserechte auf:

- AIS-Importordner
- Geräte-Importordner
- Exportordner
- Archivordner
- Fehlerordner
- optional GA-Dateianhang Import
- optional GA-Dateianhang Export
- lokales Konfigurationsverzeichnis
- aktuell JSON-basierter Profilkatalog unter `%LocalAppData%\XdtDeviceBridge\profiles`
- perspektivisch ggf. lokale SQLite-Datenbank

---

## 6. Technologievorgabe

### 6.1 Programmiersprache

C#

### 6.2 Framework

.NET 8 oder höher

### 6.3 Oberfläche

WPF

Begründung:

- stabile Windows-Technologie
- gute Unterstützung lokaler Dateiverarbeitung
- geeignet für Praxis-PCs
- gut testbar
- einfacher Installer möglich
- langfristig wartbar

### 6.4 Persistenz

Aktueller Iststand `0.1.0-prototype`:

- Profile und lokale Konfigurationen werden JSON-basiert im lokalen AppData-Profilkatalog gespeichert.
- Standardpfad: `%LocalAppData%\XdtDeviceBridge\profiles`
- Unterordner u. a. `ais`, `devices`, `exports`, `interfaces`.

SQLite bleibt eine spätere Ziel-/Option, ist aber im aktuellen Prototyp nicht implementiert.

Zu speichern sind perspektivisch:

- Geräteprofile
- Ordnerpfade
- Feldkennungen
- Mapping-Regeln
- Ausgabevorlagen
- Verarbeitungshistorie
- Fehlermeldungen

### 6.5 Tests

xUnit

Es müssen automatisierte Unit-Tests für Parser, Mapping und Exportgenerator erstellt werden.

---

## 7. Projektstruktur

Codex soll folgende Projektstruktur erstellen:

```text
XdtDeviceBridge
│
├── src
│   ├── XdtDeviceBridge.App
│   ├── XdtDeviceBridge.Core
│   ├── XdtDeviceBridge.Infrastructure
│   └── XdtDeviceBridge.Tests
│
├── samples
│   ├── ais
│   │   └── TestPatient.gdt
│   └── devices
│       └── NIDEK_ARK1S.xml
│
├── docs
│   ├── PFLICHTENHEFT.md
│   ├── MAPPING_KONZEPT.md
│   ├── GDT_XDT_NOTIZEN.md
│   └── TESTFAELLE.md
│
└── README.md
```

---

## 8. Flexible AIS-, Geräte- und Exportprofile

Der aktuelle MEDISTAR/NIDEK-ARK1S-Ablauf ist das erste validierte Standardprofil. Die Anwendung darf perspektivisch nicht fest auf MEDISTAR und NIDEK ARK1S verdrahtet bleiben. Stattdessen soll sie flexibel über Profile konfigurierbar sein, damit weitere Arztinformationssysteme, Praxisverwaltungssysteme und Untersuchungsgeräte ohne neue Spezialsoftware angebunden werden können.

### 8.1 Profilarten

Die Anwendung soll perspektivisch drei Profilarten unterstützen:

- AIS-Profil
- Geräteprofil
- Export-/Mapping-Profil

Ein Export-/Mapping-Profil verbindet ein AIS-Profil mit einem Geräteprofil. Es beschreibt, wie Patientendaten aus dem AIS und Messwerte aus der Gerätedatei zu einer Rückgabedatei für das Zielsystem zusammengeführt werden.

### 8.2 AIS-Profil

Ein AIS-Profil beschreibt, wie das Zielsystem die Rückgabedatei erwartet.

Beispiele:

- MEDISTAR
- ALBIS
- TURBOMED
- andere AIS/PVS-Systeme

Ein AIS-Profil muss mindestens definieren können:

- Name des AIS
- erwartete Satzart, z. B. `8000 = 6310`
- Zeichensatz, z. B. `Windows-1252`
- Pflichtfelder
- unterstützte Zielfeldkennungen
- ob Ergebniswerte als Textzeilen, Einzelfelder oder Kategorie/Wert-Paare ausgegeben werden
- ob Feld `8402` Untersuchungsart übernommen werden muss
- ob Feld `6228` Ergebnis verwendet wird
- ob Felder `6330` bis `6399` und `8410` bis `8480` genutzt werden
- welche Ausgabeform das AIS für bestimmte Untersuchungsarten erwartet

### 8.3 Geräteprofil

Ein Geräteprofil beschreibt die Eingabedatei eines Untersuchungsgerätes.

Beispiele:

- NIDEK ARK1S
- weitere NIDEK-Geräte
- Zeiss Medical
- Topcon
- Humphrey
- Tonometer
- Keratometer
- OCT-Geräte
- Kombigeräte mit mehreren Untersuchungsarten

Ein Geräteprofil muss mindestens definieren können:

- Gerätename
- Hersteller
- Dateiformat
- Parsermodus: XML, XPath, Regex, CSV, Text, GDT/XDT
- Importordner für Gerätedateien
- erkannte Messwertpfade
- Standardmesswerte
- mögliche Untersuchungsarten
- ob ein Gerät mehrere Untersuchungsarten in einer Datei liefern kann, z. B. Refraktion, Tonometrie, Keratometrie, PD, Pachymetrie

### 8.4 Export-/Mapping-Profil

Ein Export-/Mapping-Profil beschreibt, wie aus AIS-Daten und Gerätewerten eine Rückgabedatei erzeugt wird.

Es muss möglich sein:

- mehrere Ausgabezeilen zu erzeugen
- mehrere Ausgabe-Feldkennungen zu verwenden
- denselben Ziel-Feldcode mehrfach zu verwenden, z. B. `6228` für mehrere Ergebniszeilen
- Ausgabewerte aus mehreren Messwerten zusammenzusetzen
- feste Werte zu setzen, z. B. `8000 = 6310`
- AIS-Werte durchzureichen, z. B. `8402` Untersuchungsart
- verschiedene Ausgabearten zu definieren:
  - Ergebnistext
  - Einzelwert
  - Kategorie/Wert-Paar
  - Ergebnisblock
- Ausgabe-Syntax pro Zeile frei zu konfigurieren
- je Untersuchungsart unterschiedliche Ausgabeformate zu definieren
- mehrere Untersuchungsarten aus einer Gerätedatei in getrennte Ausgabezeilen oder getrennte Zielfelder zu schreiben

### 8.5 Validiertes erstes Profil: MEDISTAR + NIDEK ARK1S

Das erste validierte Profil ist `MEDISTAR + NIDEK ARK1S`.

Exportlogik:

- `8000 = 6310`
- `3000 = Patientennummer`
- `3101 = Nachname`
- `3102 = Vorname`
- `3103 = Geburtsdatum`
- `8402 = Untersuchungsart` aus AIS, z. B. `ARK1S`
- `6228 = Ergebnis rechts`
- `6228 = Ergebnis links`

Beispielausgabe:

```text
R.:S=- 0.25 Z=- 0.25* 49                              PD=61
L.:S=+ 0.00 Z=- 0.50* 63                              PD=61
```

Hinweis:

Das Präfix in der Karteikarte, z. B. `V1` oder `X`, wird in MEDISTAR konfiguriert und muss nicht zwingend durch die App erzeugt werden.

---

## 9. Ordnerbereinigung und Dateiaufräumung

Die Anwendung muss pro Schnittstellenprofil sichere Optionen erhalten, um bekannte verarbeitete Importdateien kontrolliert aus Arbeitsordnern zu entfernen, ohne den Praxisbetrieb zu gefährden.

Wichtige Sicherheitsentscheidung im aktuellen Stand `0.1.0-prototype`:

- Es gibt keine pauschale Ordnerleerung.
- Es werden keine unbekannten Dateien gelöscht, verschoben oder archiviert.
- Importdateien werden nur behandelt, wenn sie zu einem bekannten verarbeiteten Dateipaar gehören.
- Der Exportordner zum AIS wird nicht pauschal bereinigt.
- Eine frühere Option `Exportordner nach erfolgreicher Übertragung leeren` gilt als überholt und ist in der UI nicht mehr aktiv.

### 9.1 AIS-Importordner

Der AIS-Importordner darf nicht blind geleert werden. Stattdessen kann pro Schnittstellenprofil vorgesehen werden, bereits verarbeitete AIS-Dateien aus dem Importordner zu entfernen.

- Aktuelle UI-Bedeutung: `Bereits verarbeitete AIS-Dateien aus Importordner entfernen`
- Entfernen bedeutet: bekannte verarbeitete Dateien werden gemäß Profiloption ins Archiv kopiert oder verschoben.
- Es werden keine unbekannten neuen AIS-Dateien gelöscht.

### 9.2 Geräte-Importordner

Der Geräte-Importordner darf nicht blind geleert werden. Stattdessen kann pro Schnittstellenprofil vorgesehen werden, bereits verarbeitete Gerätedateien aus dem Importordner zu entfernen.

- Aktuelle UI-Bedeutung: `Bereits verarbeitete Gerätedateien aus Importordner entfernen`
- Entfernen bedeutet: bekannte verarbeitete Dateien werden gemäß Profiloption ins Archiv kopiert oder verschoben.
- Es werden keine unbekannten neuen Gerätedateien gelöscht.

### 9.3 Exportordner zum AIS

Der Exportordner wird nicht pauschal bereinigt.

Begründung: Nachdem die App eine XDT-Datei in den Exportordner geschrieben hat, ist das AIS für den Abruf zuständig. Ein automatisches Löschen direkt nach dem Export wäre riskant, weil das AIS die Datei eventuell noch nicht verarbeitet hat.

### 9.4 Sicherheitsanforderungen für Dateioperationen

- Dateioperationen müssen pro Schnittstellenprofil konfigurierbar sein.
- Standardmäßig dürfen gefährliche Bereinigungsoptionen nicht aktiv sein.
- Importdateien dürfen höchstens gemäß Profiloption ins Archiv kopiert oder verschoben werden.
- Fehler beim Kopieren/Verschieben müssen protokolliert und sichtbar gemacht werden.
- Dateien, die gerade gesperrt oder in Verarbeitung sind, dürfen nicht gelöscht oder verschoben werden.
- Es muss verhindert werden, dass versehentlich falsche Verzeichnisse wie `C:\`, Benutzerordner oder Netzlaufwerk-Wurzeln geleert werden.
- Für den Produktivbetrieb muss eine Sicherheitsprüfung vorgesehen werden. Der Ordner muss explizit im Profil gespeichert sein und darf nicht leer, Root-Verzeichnis oder Systemordner sein.

---

## 10. Auswirkungen auf spätere Konfiguration

Der spätere Einstellungsbereich der Anwendung muss die Profil- und Ordneroptionen vollständig abbilden. Mindestens folgende Einstellungen müssen vorgesehen werden:

- AIS-Profil auswählen
- Geräteprofil auswählen
- Export-/Mapping-Profil bearbeiten
- AIS-Importordner definieren
- Geräte-Importordner definieren
- Exportordner definieren
- Archivordner definieren
- Fehlerordner definieren
- optional GA-Dateianhang Import definieren
- optional GA-Dateianhang Export definieren
- Option: bereits verarbeitete AIS-Dateien aus Importordner entfernen
- Option: bereits verarbeitete Gerätedateien aus Importordner entfernen
- Option: Dateien nach Verarbeitung archivieren
- Option: Archivierungsmodus Kopieren oder Verschieben
- Mapping-Regeln und Ergebnis-Syntax bearbeiten
- pro Untersuchungsart eigene Ausgabe-Syntax definieren

---

## 11. Anforderungen an Sicherheit und Nachvollziehbarkeit

Alle automatisierten Dateioperationen müssen nachvollziehbar und sicher ausgeführt werden.

- Jede Lösch-/Archivierungsaktion muss protokolliert werden.
- Das Protokoll enthält Zeitpunkt, Profilname, Ordner, Dateiname, Aktion und Ergebnis.
- Bei Fehlern muss die App eine verständliche Meldung anzeigen.
- Im Produktivbetrieb darf die App keine Dateien außerhalb der konfigurierten Arbeitsordner löschen.
- Standardverhalten: keine automatische Löschung, solange der Benutzer dies nicht bewusst aktiviert.

---

## 12. Schablonen, Templates und Konfigurationsexport

Die Anwendung soll perspektivisch wiederverwendbare Schablonen für Geräte-, AIS- und Export-/Mapping-Konfigurationen unterstützen. Dadurch sollen erprobte Schnittstellenkonfigurationen nicht für jede Praxis, Workstation oder Gerätevariante neu aufgebaut werden müssen.

### 12.1 Zweck von Templates

Templates sollen Systembetreuer dabei unterstützen, funktionierende Konfigurationen einmal zu erstellen, zu speichern und später kontrolliert wiederzuverwenden.

Ziele:

- Ein Systembetreuer kann eine funktionierende Konfiguration einmal erstellen.
- Diese Konfiguration kann als Template gespeichert werden.
- Das Template kann später als Grundlage für neue Geräteanbindungen, andere Workstations oder andere Praxen verwendet werden.
- Danach müssen nur noch praxis- oder gerätespezifische Details angepasst werden, z. B. Ordnerpfade, Gerätename oder einzelne Mapping-Regeln.

### 12.2 Template-Arten

Die Anwendung soll perspektivisch folgende Template-Arten unterstützen:

- AIS-Templates
- Geräte-Templates
- Export-/Mapping-Templates
- Dokument-/Dateianhang-Templates
- vollständige Schnittstellenprofile als Kombi-Template

Definitionen:

- AIS-Template: beschreibt generelle Anforderungen eines Arztinformationssystems, z. B. MEDISTAR, ALBIS oder TURBOMED.
- Geräte-Template: beschreibt generelle Parser- und Messwertlogik eines Gerätes oder Gerätetyps, z. B. NIDEK ARK1S, Zeiss Humphrey oder ein Topcon-Gerät.
- Export-/Mapping-Template: beschreibt, wie ausgelesene Werte in XDT/GDT-Ausgabefelder, Ergebniszeilen oder Ergebnisblöcke geschrieben werden.
- Dokument-/Dateianhang-Template: beschreibt, wie ein bereits vom Gerät erzeugter Geräte-Dateianhang übernommen, eindeutig benannt, in den AIS-erreichbaren Zielordner übertragen und als externer AIS-Link exportiert werden soll.
- Kombi-Template: enthält eine vollständige, bereits funktionierende Kombination aus AIS-Profil, Geräteprofil, Export-/Mapping-Regeln, Ordnerlogik und Ausgabesyntax.

### 12.3 Template-Verwendung

Die App muss ermöglichen:

- bestehendes Template auswählen
- Template als neue Konfiguration übernehmen
- übernommene Konfiguration bearbeiten
- geänderte Konfiguration unter neuem Namen speichern
- bestehendes Template duplizieren
- Template als Grundlage für ähnliches Gerät verwenden
- Template als Grundlage für ähnliche Praxis verwenden
- Template versionieren oder zumindest mit Versionshinweis versehen
- Template mit Beschreibung versehen

Beispiel:

Ein funktionierendes Template `MEDISTAR + NIDEK ARK1S` kann kopiert werden. Die Kopie wird für ein ähnliches NIDEK-Gerät angepasst. Danach wird die neue Konfiguration als `MEDISTAR + NIDEK Gerät XYZ` gespeichert.

### 12.4 Export von Konfigurationen

Die App muss eine Exportfunktion für Konfigurationen erhalten.

Exportierbar sein müssen:

- einzelne AIS-Templates
- einzelne Geräte-Templates
- einzelne Export-/Mapping-Templates
- vollständige Schnittstellenprofile
- optional alle vorhandenen Templates und Profile als Gesamtpaket

Der Export soll als Datei oder Paket möglich sein, z. B.:

- JSON-Datei
- ZIP-Paket mit mehreren JSON-Dateien
- später optional signiertes oder versioniertes Paket

Das Exportpaket soll mindestens enthalten können:

- AIS-Profil(e)
- Geräteprofil(e)
- Export-/Mapping-Regeln
- Ausgabe-Templates
- Ordneroptionen
- Bereinigungs-/Archivierungsoptionen
- Zeichensatzoptionen
- Beschreibung
- Versionsinformation
- Erstellungsdatum
- optional Hersteller-/Gerätehinweise

### 12.5 Import von Konfigurationen

Die App muss eine Importfunktion für Konfigurationen erhalten.

Ziel:

Ein Systembetreuer kann eine funktionierende Konfiguration auf einer Workstation exportieren und auf einer anderen Workstation oder in einer anderen Praxis importieren.

Beim Import muss möglich sein:

- Importdatei auswählen
- Inhalt vor Import anzeigen
- enthaltene Templates/Profile anzeigen
- auswählen, welche Profile importiert werden sollen
- Konflikte erkennen, z. B. gleicher Profilname existiert bereits
- vorhandene Profile nicht ungefragt überschreiben
- Option anbieten:
  - als Kopie importieren
  - vorhandenes Profil ersetzen
  - vorhandenes Profil beibehalten
- nach Import praxisabhängige Pfade anpassen

### 12.6 Pfade und lokale Workstations

Da die Anwendung lokal auf einzelnen Workstations läuft, müssen importierte Konfigurationen lokal anpassbar sein.

Besonders zu beachten:

- Import-/Exportordner unterscheiden sich je Arbeitsplatz
- Laufwerksbuchstaben können unterschiedlich sein
- Netzwerkpfade können unterschiedlich sein
- Geräteordner können je Workstation anders sein
- GA-Dateianhang Import und GA-Dateianhang Export können je Workstation oder AIS-Arbeitsplatz unterschiedlich sein
- Pfade dürfen beim Import nicht blind übernommen werden, ohne dass der Benutzer sie prüfen kann

Anforderung:

Nach dem Import muss die App die Pfade prüfen und den Benutzer auffordern, fehlende oder ungültige Ordner neu zu setzen.

### 12.7 Sicherheit beim Import

Der Import darf keine gefährlichen Einstellungen ungeprüft aktivieren.

Besonders kritisch:

- automatische Löschoptionen
- Ordnerbereinigung
- historische/überholte Exportordner-Bereinigungsoptionen
- Pfade zu Root-Verzeichnissen oder Systemordnern
- importierte Pfade für GA-Dateianhang Import und GA-Dateianhang Export
- echte Patientendokumente oder Geräte-Dateianhänge in Templatepaketen
- unbekannte oder fehlerhafte Mapping-Regeln

Anforderungen:

- Löschoptionen sollen nach Import standardmäßig überprüft werden müssen.
- Gefährliche Pfade dürfen nicht automatisch aktiv werden.
- Importierte Profile müssen validiert werden.
- Fehlerhafte oder unvollständige Profile dürfen nicht produktiv aktiviert werden.
- Die App muss verständlich anzeigen, welche Punkte nach dem Import geprüft werden müssen.
- Templatepakete dürfen keine echten Patientendokumente oder Geräte-Dateianhänge enthalten.
- Importierte GA-Ordnerpfade müssen geprüft und bei Bedarf lokal neu gesetzt werden.

### 12.8 Template-Bibliothek

Die App soll perspektivisch eine lokale Template-Bibliothek besitzen.

Diese Bibliothek enthält:

- mitgelieferte Standardtemplates
- selbst erstellte Templates
- importierte Templates
- kopierte/geänderte Templates

Jedes Template soll Metadaten enthalten:

- Name
- Typ
- Hersteller
- Gerät/AIS
- Beschreibung
- Version
- Ersteller
- Erstellungsdatum
- Änderungsdatum
- kompatible App-Version
- Hinweis, ob es ein Standardtemplate oder benutzerdefiniertes Template ist

### 12.9 Beispiel validiertes Template

Das erste validierte Template ist:

`MEDISTAR + NIDEK ARK1S`

Es enthält:

- AIS-Profil: MEDISTAR
- Geräteprofil: NIDEK ARK1S
- Parsermodus: XML
- Untersuchungsart aus AIS-Feld `8402`
- Steuerfeld `8000 = 6310`
- Ausgabe über zwei Ergebniszeilen mit Feldkennung `6228`
- Ergebnis rechts
- Ergebnis links
- Windows-1252 als Zeichensatz
- manuelle Datei-Auswahl im aktuellen Prototyp
- später Ordnerüberwachung

### 12.10 Anforderungen an die spätere Oberfläche

Der Einstellungsbereich der App soll später ermöglichen:

- Template-Bibliothek anzeigen
- Template suchen/filtern
- Template duplizieren
- Template bearbeiten
- Template als neue Konfiguration übernehmen
- Konfiguration exportieren
- Konfiguration importieren
- vollständiges Konfigurationspaket exportieren
- vollständiges Konfigurationspaket importieren
- importierte Profile prüfen
- ungültige Pfade hervorheben
- Konflikte beim Import anzeigen

### 12.11 Nachvollziehbarkeit

Jeder Import und Export von Konfigurationen soll protokolliert werden.

Das Protokoll soll enthalten:

- Zeitpunkt
- Benutzer/Workstation, soweit verfügbar
- importierte/exportierte Datei
- enthaltene Profile/Templates
- Ergebnis
- Fehlermeldungen
- ob Profile überschrieben, kopiert oder ausgelassen wurden

### 12.12 Abgrenzung für Version 1

Für die aktuelle erste Version reicht zunächst:

- ein fest eingebautes Standardprofil `MEDISTAR + NIDEK ARK1S`
- später als nächster Schritt JSON-Speicherung eines DeviceProfile/Exportprofils
- noch keine zentrale Cloud-Template-Bibliothek
- noch keine automatische Online-Synchronisation
- keine Herstellerdatenbank

Langfristiges Ziel:

Lokale, exportierbare und importierbare Konfigurationspakete für Systembetreuer, damit Geräteanbindungen in mehreren Praxen und auf mehreren Workstations wiederverwendbar eingerichtet werden können.

---

## 13. Lizenzierung und Abrechnung

Die Anwendung soll langfristig über ein Lizenzmodell betrieben werden, das sich an aktiv eingerichteten und produktiv genutzten Geräteanbindungen orientiert. Für die erste marktfähige Ausbaustufe steht eine offline nutzbare Lizenzierung im Vordergrund. Eine Online-Lizenzierung ist nur als spätere optionale Erweiterung vorgesehen.

### 13.1 Grundsatz der Lizenzierung

Die Anwendung soll langfristig pro aktiv angebundenem medizinischem Gerät lizenziert werden.

Lizenzpflichtig ist nicht die bloße Installation der Anwendung, sondern die Anzahl aktiv eingerichteter und produktiv genutzter Geräteanbindungen.

Beispiel:

- Eine Praxis hat zwei Arbeitsplätze bzw. zwei lokale App-Instanzen.
- Auf Arbeitsplatz 1 sind drei medizinische Geräte angebunden.
- Auf Arbeitsplatz 2 sind vier medizinische Geräte angebunden.
- Abgerechnet werden insgesamt sieben aktive Geräte.

Die Geräteanzahl muss je Installation bzw. je lokaler App-Instanz ermittelbar sein.

### 13.2 Zielarchitektur der ersten Lizenzversion: Offline-Lizenzierung

Für die erste marktfähige Version soll die Lizenzierung primär offline funktionieren.

Begründung:

- Arztpraxen und Außenstellen haben häufig eingeschränkten Internetzugang.
- Firewalls, Proxy-Server, Terminalserver und RDP-Umgebungen können Online-Aktivierung erschweren.
- Die App muss auch in Umgebungen funktionieren, in denen kein direkter Internetzugriff möglich oder erlaubt ist.
- Systembetreuer sollen die App installieren und einrichten können, ohne sofort eine Online-Verbindung zu benötigen.

Die Online-Lizenzierung ist nur als spätere optionale Ausbaustufe vorgesehen, falls sich die Offline-Lizenzierung als zu aufwendig oder für den Vertrieb nicht ausreichend skalierbar erweist.

### 13.3 30-Tage-Testphase

Nach Erstinstallation oder erster Nutzung soll die App eine Testphase ermöglichen.

Anforderungen:

- Standard-Testzeitraum: 30 Tage
- Während der Testphase sind alle Funktionen nutzbar.
- Während der Testphase dürfen mehrere Geräte eingerichtet und getestet werden.
- Die App zeigt den verbleibenden Testzeitraum klar an.
- Während der Testphase soll die Anzahl aktiv eingerichteter Geräte sichtbar sein.
- Nach Ablauf der Testphase darf keine produktive Verarbeitung mehr erfolgen.
- Konfiguration, Anzeige vorhandener Einstellungen, Export von Einstellungen und Lizenzaktivierung müssen weiterhin möglich bleiben.
- Die App soll verständlich anzeigen, dass eine Lizenz erforderlich ist.

### 13.4 Offline-Lizenzierungsablauf

Die App soll für die Offline-Lizenzierung einen Anfragecode bzw. Installationscode erzeugen.

Der Anfragecode soll technische Lizenzinformationen enthalten, aber keine Patientendaten.

Typischer Ablauf:

1. Systembetreuer installiert die App.
2. App erzeugt eine eindeutige Installations-ID.
3. App zeigt Testzeitraum und Anzahl aktiv eingerichteter Geräte an.
4. App erzeugt einen Lizenz-Anfragecode oder eine Anfrage-Datei.
5. Systembetreuer sendet diesen Code bzw. diese Datei an den Hersteller.
6. Hersteller erzeugt eine signierte Lizenzdatei oder einen Aktivierungscode.
7. Systembetreuer importiert die Lizenzdatei oder trägt den Aktivierungscode in der App ein.
8. App prüft die Lizenz lokal ohne Internetverbindung.
9. App aktiviert die Verarbeitung für die lizenzierte Geräteanzahl und den Lizenzzeitraum.

### 13.5 Signierte Lizenzdatei

Die bevorzugte Offline-Methode ist eine signierte Lizenzdatei.

Die Lizenzdatei soll mindestens enthalten:

- Lizenz-ID
- Kunde/Praxis oder Kundennummer
- optionale Standortbezeichnung
- Installations-ID
- lizenzierte Geräteanzahl
- Lizenzbeginn
- Lizenzende oder nächster Prüfzeitraum
- Lizenztyp, z. B. Test, Monatlich, Jahreslizenz
- App-Produktkennung
- App-Version oder Mindestversion
- Erstellungsdatum
- digitale Signatur

Die Lizenzdatei darf durch den Kunden nicht manipulierbar sein.

Technische Anforderungen:

- Der Hersteller signiert die Lizenz mit einem privaten Schlüssel.
- Die App enthält ausschließlich den öffentlichen Prüfschlüssel.
- Der private Schlüssel darf niemals in der App enthalten sein.
- Die App prüft die Signatur lokal.
- Eine manuell geänderte Lizenzdatei muss als ungültig erkannt werden.

### 13.6 Lizenzcode als Alternative

Zusätzlich oder alternativ zur Lizenzdatei kann ein kompakter Lizenzcode unterstützt werden.

Der Lizenzcode soll dieselben Grundinformationen wie die Lizenzdatei abbilden, soweit technisch sinnvoll.

Ein Lizenzcode kann sinnvoll sein, wenn:

- keine Datei übertragen werden soll
- der Systembetreuer telefonisch oder per E-Mail aktivieren möchte
- kleinere Installationen manuell aktiviert werden sollen

Die Lizenzdatei ist jedoch als besser wartbare Standardvariante für die erste Umsetzung zu bevorzugen.

### 13.7 Lizenzpflichtige Einheit

Lizenzpflichtig ist ein aktiv eingerichtetes Geräteprofil bzw. eine aktiv genutzte Geräteanbindung.

Ein Geräteprofil muss perspektivisch mindestens folgende Eigenschaften besitzen:

- Aktiv
- Lizenzpflichtig
- Gerätename
- Hersteller
- Profiltyp
- Installations-ID bzw. Instanz-ID
- optional Seriennummer oder interne Gerätekennung
- Datum der ersten Aktivierung
- Datum der letzten Nutzung
- letzter Verarbeitungszeitpunkt

Nicht aktive oder deaktivierte Testprofile sollen nicht automatisch abrechnungsrelevant sein.

### 13.8 Zählung der Geräte

Die App muss zählen können, wie viele aktive lizenzpflichtige Geräteprofile eingerichtet sind.

Beispiel:

- Geräteprofil NIDEK ARK1S aktiv und lizenzpflichtig = zählt
- Geräteprofil Zeiss Humphrey aktiv und lizenzpflichtig = zählt
- altes Testprofil deaktiviert = zählt nicht
- Profilvorlage/Template ohne aktive Ordnerkonfiguration = zählt nicht
- importiertes Template, aber nicht aktiv genutzt = zählt nicht

Die App darf die produktive Verarbeitung nur erlauben, wenn:

```text
aktive lizenzpflichtige Geräteanzahl <= lizenzierte Geräteanzahl
```

Wenn mehr Geräte aktiv sind als lizenziert, muss die App verständlich anzeigen, welche Geräte deaktiviert oder nachlizenziert werden müssen.

### 13.9 Verhalten bei Lizenzablauf oder ungültiger Lizenz

Wenn die Lizenz abgelaufen, ungültig oder nicht ausreichend ist:

- keine neue produktive Verarbeitung von AIS-/Gerätedateien
- keine neue Exportdatei an das AIS erzeugen
- klare Meldung an den Benutzer
- bestehende Konfiguration bleibt sichtbar
- Einstellungen dürfen weiterhin geöffnet werden
- Export und Import von Konfigurationen bleiben möglich
- Lizenzaktivierung bleibt möglich
- Protokolle bleiben sichtbar

Die App darf nicht vollständig unbedienbar werden, weil der Systembetreuer weiterhin die Möglichkeit zur Nachlizenzierung oder Konfigurationskorrektur benötigt.

### 13.10 Terminalserver- und Remote-Desktop-Betrieb

Die Anwendung muss perspektivisch auch auf Terminalservern bzw. in Remote-Desktop-Umgebungen lauffähig sein.

Hintergrund:

Einige Außenstellen oder Praxen haben keine eigene lokale Serverstruktur. Es kann sein, dass die Arbeitsplätze vollständig über Remote Desktop oder Terminalserver betrieben werden. Die Anwendung soll möglichst nicht unterscheiden müssen, ob sie auf einer normalen Windows-Workstation oder auf einem Terminalserver läuft.

Anforderungen:

- Die App muss lokal pro Windows-Benutzer bzw. pro App-Instanz konfigurierbar sein.
- Lizenz- und Konfigurationsdaten dürfen nicht ausschließlich maschinenweit gespeichert werden.
- Es muss möglich sein, Konfigurationen benutzerbezogen im lokalen Benutzerprofil zu speichern.
- Beispielpfad: `%AppData%` oder `%LocalAppData%`
- Die App soll auf einem Terminalserver mehrere Benutzerprofile sauber trennen können.
- Eine Benutzerinstanz darf nicht versehentlich die Konfiguration einer anderen Benutzerinstanz überschreiben.
- Wenn mehrere Benutzer auf demselben Terminalserver arbeiten, muss nachvollziehbar sein, welche App-Instanz bzw. welcher Benutzer welche Geräteprofile nutzt.
- Die App soll möglichst identisch funktionieren, egal ob Workstation oder Terminalserver.

### 13.11 Installations-ID bei Workstation und Terminalserver

Die Installations-ID muss so gestaltet sein, dass sie sowohl auf Einzelplatz-Workstations als auch auf Terminalservern sinnvoll funktioniert.

Mögliche Konzepte:

- Workstation-Installation: Installations-ID pro Windows-Installation/App-Installation
- Terminalserver: Installations-ID pro Benutzerprofil oder pro App-Instanz
- optional Kombination aus Maschinenkennung, Benutzerkennung und App-spezifischer GUID

Wichtig:

- Die App soll nicht ausschließlich an harte Hardwaremerkmale gebunden werden.
- Hardwaretausch, Windows-Updates oder Benutzerprofil-Migrationen dürfen nicht sofort zu Lizenzchaos führen.
- Eine Reaktivierung oder Lizenzübertragung muss organisatorisch möglich bleiben.
- Die Installations-ID darf keine Patientendaten enthalten.
- Wenn Terminalserverbetrieb erkannt wird, soll die App weiterhin normal funktionieren.

### 13.12 Speicherort für Lizenz- und Konfigurationsdaten

Die App muss perspektivisch definieren, wo Lizenz- und Konfigurationsdaten gespeichert werden.

Für die erste Umsetzung wird empfohlen:

- benutzerbezogene Konfiguration in `%AppData%` oder `%LocalAppData%`
- lokale Lizenzinformationen ebenfalls benutzerbezogen oder installationsbezogen
- exportierbare Konfigurationspakete für Übertragung auf andere Workstations
- keine zwingende Abhängigkeit von Administratorrechten

Optional später:

- maschinenweite Konfiguration unter `ProgramData`
- getrennte Modi:
  - Benutzerinstallation
  - Maschineninstallation
  - Terminalserverinstallation

Die App muss so gebaut werden, dass spätere Speicherort-Strategien möglich bleiben.

### 13.13 Datenschutz

Die Lizenzierung darf keine Patientendaten verarbeiten oder übertragen.

Nicht in Anfragecode, Lizenzdatei oder späterem Online-Lizenzsystem enthalten sein dürfen:

- Patientennamen
- Patientennummern
- Geburtsdaten
- Untersuchungswerte
- XDT-/GDT-Dateiinhalte
- medizinische Befunde
- medizinische Diagnosen

Erlaubte technische Lizenzdaten:

- Installations-ID
- App-Version
- Kundennummer oder Praxis-ID
- optionale Standortbezeichnung
- Anzahl aktiver lizenzpflichtiger Geräteprofile
- Geräteprofilnamen
- Hersteller/Modell der angebundenen Geräte
- Lizenzstatus
- Zeitstempel der letzten Lizenzprüfung
- Benutzer-/Instanzkennung ohne Patientendatenbezug

### 13.14 Abrechnung

Die monatliche Abrechnung soll perspektivisch auf der Anzahl aktiver lizenzpflichtiger Geräte basieren.

Das System muss ermitteln können:

- Anzahl aktiver Geräte pro App-Instanz
- Anzahl aktiver Geräte pro Workstation
- Anzahl aktiver Geräte pro Terminalserver-Benutzerinstanz
- Anzahl aktiver Geräte pro Praxis
- Anzahl aktiver Geräte pro Kunde/Systembetreuer
- Zeitraum der Nutzung
- Lizenzstatus je Geräteanbindung

Für die erste Offline-Umsetzung kann die Abrechnung organisatorisch erfolgen:

- Systembetreuer meldet Anfragecode und Geräteanzahl.
- Hersteller erzeugt Lizenz für passende Geräteanzahl.
- monatliche Rechnung wird auf dieser Basis erstellt.

Später kann die Abrechnung über ein Webportal automatisiert werden.

### 13.15 Online-Lizenzierung als spätere Ausbaustufe

Eine Online-Lizenzierung soll nur als spätere optionale Ausbaustufe vorgesehen werden.

Langfristiges Ziel:

- Kundenkonto anlegen
- Praxis anlegen
- Workstation/Installation registrieren
- Geräteanzahl anzeigen
- Lizenzstatus verwalten
- Zahlungsdaten hinterlegen
- monatliche Abrechnung vorbereiten
- Lizenz aktivieren/deaktivieren
- Installationen sperren oder freigeben
- automatische Lizenzprüfung durch die App

Die Online-Lizenzierung darf die Offline-Fähigkeit der App nicht grundsätzlich zerstören. Auch bei späterer Online-Lizenzierung soll ein Offline-Fallback möglich bleiben.

### 13.16 Kulanz und eingeschränkte Erreichbarkeit

Auch bei späterer Online-Lizenzierung muss die App mit eingeschränkter Erreichbarkeit umgehen können.

Die folgenden Punkte beschreiben das Zielbild einer späteren produktiven Lizenzdurchsetzung. Im aktuellen Stand `0.1.0-prototype` wird der Lizenzstatus angezeigt und für aktive lizenzpflichtige Schnittstellenprofile bewertet; die produktive Verarbeitung wird aber noch nicht hart gesperrt.

Beispiele:

- Lizenzserver kurzzeitig nicht erreichbar
- Praxisfirewall blockiert Zugriff
- Terminalserver hat keinen direkten Internetzugang
- Außenstelle arbeitet nur eingeschränkt online

Anforderungen:

- definierte Kulanzzeit
- verständlicher Hinweis an den Benutzer
- keine sofortige Sperre bei kurzfristigem Verbindungsproblem
- nach Ablauf der Kulanzzeit Sperre der produktiven Verarbeitung
- Konfiguration und Lizenzaktivierung bleiben möglich

### 13.17 Sicherheit

Die Lizenzierung soll manipulationssicher genug für den praktischen Einsatz sein.

Anforderungen:

- lokale Lizenzdaten dürfen nicht einfach editierbar sein
- Lizenzdateien müssen digital signiert werden
- App enthält nur öffentlichen Prüfschlüssel
- private Signaturschlüssel dürfen nicht in der App enthalten sein
- Systemzeit-Manipulation soll zumindest erkannt oder protokolliert werden
- Lizenzverletzungen sollen protokolliert werden
- Lizenzdateien dürfen nicht ohne Prüfung auf andere Installationen übertragbar sein
- bei Terminalserverbetrieb muss die Bindung an Benutzer-/Instanzkontext berücksichtigt werden

### 13.18 Oberfläche für Lizenzierung

Die App soll perspektivisch einen Lizenzbereich erhalten.

Dieser Bereich zeigt:

- Lizenzstatus
- Testphase aktiv ja/nein
- verbleibende Testtage
- Installations-ID
- Anfragecode erzeugen
- Lizenzdatei importieren
- lizenzierte Geräteanzahl
- aktiv eingerichtete lizenzpflichtige Geräteanzahl
- Warnung bei Überschreitung
- Lizenzablaufdatum
- Hinweise zur Kontaktaufnahme mit dem Hersteller

### 13.19 Abgrenzung Version 1

Für die aktuelle technische Version wird zunächst keine vollständige Lizenzierung implementiert.

Das Pflichtenheft hält aber fest:

- Offline-Lizenzierung ist die bevorzugte erste Umsetzungsstufe.
- Online-Lizenzierung ist nur spätere Option.
- Lizenzierung pro aktivem Gerät ist Zielarchitektur.
- 30-Tage-Testphase ist vorgesehen.
- signierte Offline-Lizenzdatei ist bevorzugter Mechanismus.
- Lizenzcode ist mögliche Alternative.
- Terminalserver-/Remote-Desktop-Betrieb muss berücksichtigt werden.
- Lizenz- und Konfigurationsdaten müssen benutzer- bzw. instanzbezogen speicherbar sein.
- keine Patientendaten dürfen zur Lizenzierung verwendet oder übertragen werden.

---

## 14. Erweiterte Geräte, Untersuchungsarten und Zusatzdokumente

Die bisher validierte MEDISTAR/NIDEK-ARK1S-Verarbeitung bildet nur das erste Standardprofil. Die weiteren Beispieldaten zeigen, dass die Anwendung perspektivisch mehrere Geräteklassen, mehrere Untersuchungsarten pro Datei und begleitende Dokumente unterstützen muss.

Für den aktuellen Stand `0.1.0-prototype` ist noch keine produktive Umsetzung von Geräte-Dateianhang-Import, MEDISTAR externem Link über XDT, selbst erzeugter PDF-Protokollerzeugung oder Mehruntersuchungs-Profilen enthalten. Diese Punkte sind Zielanforderungen bzw. spätere Ausbaustufen. Die Beispieldaten dienen zunächst zur Erweiterung des Pflichtenhefts und der Architektur. Die funktionsfähige MEDISTAR/NIDEK-ARK1S-Verarbeitung bleibt unverändert.

### 14.1 Weitere Gerätetypen und Untersuchungsarten

Die App muss perspektivisch nicht nur NIDEK ARK1S und Refraktion unterstützen, sondern auch weitere Geräte und Untersuchungsarten.

Beispiele aus den Beispieldaten:

- NIDEK AR1S: Autorefraktion und PD
- NIDEK LM7: Lensmeter/Scheitelbrechwertmesser mit Sphäre, Zylinder, Achse, Addition, Prisma, Basisrichtung und PD
- NIDEK NT530P: Tonometrie, Pachymetrie, korrigierter IOP und Bild-/Protokollverweise
- TOPCON CL300: Lensmeterdaten im Ophthalmology-/JOIA-XML-Format
- TOPCON KR800: Refraktion, Keratometrie, subjektive Refraktions-/VA-/PD-Daten
- TOPCON TRK2P: Tonometrie und Pachymetrie/CCT

Daraus folgt, dass Geräteprofile künftig nicht nur Messwertpfade, sondern auch Untersuchungsarten, Messwertgruppen, Ausgabearten und gerätespezifische Besonderheiten beschreiben müssen.

### 14.1.1 NIDEK LM7/LM7P LAN-XML als Lensmeter-Profil

Die NIDEK LM-7/LM-7P-LAN-Schnittstelle passt zum Zielbild der XdtDeviceBridge: Das Gerät schreibt Messdateien per LAN/WLAN über SMB/Common Internet File System in einen freigegebenen Windows-Ordner. Dieser Shared Folder entspricht in der App dem Geräte-Importordner. Eine direkte Geräte-API ist nicht erforderlich.

Anforderung:

- Die App muss LM7/LM7P-Messdateien aus einem Geräte-Importordner per Scan/Überwachung aufnehmen können.
- Das Geräteprofil muss LAN-XML-Dateien nach `NIDEK_V1.00` und `NIDEK_V1.01` unterstützen.
- Das Dateinamenmuster `LM_<ID>_<YYYYMMDDHHMMSS>_<MAC-Lower-Bytes>.xml` ist als typische Geräteausgabe zu dokumentieren.
- `NIDEK_LM_Stylesheet.xsl` ist keine Messdatei und darf nicht als Gerätedatei verarbeitet werden.
- Für produktiven Betrieb ist Archivmodus `Move` besonders sinnvoll, weil das Gerät einen Network Timeout melden kann, wenn die XML-Datei nach Ablauf der konfigurierten Zeit noch im Shared Folder liegt.
- Die App darf hierbei keine unbekannten Dateien löschen; verarbeitete Gerätedateien dürfen gemäß Schnittstellenprofil ins Archiv verschoben werden.

XML-Struktur:

```text
Ophthalmology
- Common
- Measure Type="LM"
  - MeasureMode
  - DiopterStep
  - AxisStep
  - CylinderMode
  - PrismDiopterStep
  - PrismBaseStep
  - PrismMode
  - AddMode
  - LM/S
  - LM/R
  - LM/L
  - PD
```

Relevante Common-Felder:

- `Common/Company`
- `Common/ModelName`
- `Common/MachineNo`
- `Common/ROMVersion`
- `Common/Version`
- `Common/Date`
- `Common/Time`
- `Common/Patient/No.`
- `Common/Patient/ID`
- `Common/Operator/ID`

Relevante Messwerte je `S`, `R` und `L`:

- `Sphere`, `Cylinder`, `Axis`
- optional `SE`, `ADD`, `ADD2`, `NearSphere`, `NearSphere2`
- optional `Prism`, `PrismBase`, `PrismX`, `PrismX/@base`, `PrismY`, `PrismY/@base`
- optional `UVTransmittance`
- optional `ConfidenceIndex` und `Error` bei `NIDEK_V1.01`

PD-Werte:

- `Measure[@Type='LM']/PD/Distance`
- `Measure[@Type='LM']/PD/DistanceR`
- `Measure[@Type='LM']/PD/DistanceL`
- `Measure[@Type='LM']/PD/Near`
- `Measure[@Type='LM']/PD/NearR`
- `Measure[@Type='LM']/PD/NearL`

Parser-Anforderungen:

- Fehlende optionale Tags und fehlende S-/R-/L-Knoten dürfen nicht als harte Parserfehler gelten.
- Leere Tags müssen toleriert werden.
- XML-Attribute müssen als Platzhalter verfügbar sein, z. B. `Measure[@Type='LM']/@Type`, `PrismX/@base`, `PrismY/@base`, `Sphere/@unit`, `Distance/@unit`.
- Attribute wie `unit="D"` oder `base="out"` sollen nicht verloren gehen.
- Error-Tags aus `NIDEK_V1.01` sind als SourcePaths/Messwerte vorzusehen bzw. vorbereitet. Eine automatische Einordnung als `DeviceParseIssue` oder Gerätewarnung ist noch offen und muss vor produktiver Nutzung fachlich spezifiziert werden.

Hinweis zu Tagvarianten:

Das bisherige lokale LM7-Praxisfragment enthält die Schreibweise `Sphare` und `NearSphare`. Das Interface Manual beschreibt für die standardisierte LAN-XML-Ausgabe `Sphere` und `NearSphere`. Das Geräteprofil soll beide Varianten berücksichtigen, wobei `Sphere`/`NearSphere` die bevorzugten LAN-XML-Tags sind. Vorhandene Praxispfade dürfen nicht blind entfernt werden.

### 14.2 Mehrere Untersuchungsarten pro Gerätedatei

Ein Gerät kann mehrere Untersuchungsarten in einer Datei liefern.

Beispiele:

- TOPCON KR800 enthält `REF`, `KM` und `SBJ`.
- TOPCON TRK2P enthält `TM` und `CCT`.
- NIDEK NT530P enthält `NT` und `PACHY`.

Die App muss daher pro Geräteprofil definieren können:

- welche Untersuchungsarten ausgelesen werden sollen
- welche Untersuchungsarten ignoriert werden sollen
- welche Untersuchungsarten in getrennte Ergebniszeilen geschrieben werden
- welche Untersuchungsarten in getrennte XDT-Felder geschrieben werden
- welche Untersuchungsarten zusammengefasst werden

### 14.3 Geräteprofile mit mehreren Ergebnis-Templates

Ein Geräteprofil bzw. Exportprofil muss mehrere Ergebnis-Templates enthalten können.

Beispiele:

- ARK1S:
  - `6228` Ergebnis rechts
  - `6228` Ergebnis links
- LM7:
  - Lensmeter rechts mit Sphäre, Zylinder, Achse, Prisma und PD
  - Lensmeter links mit Sphäre, Zylinder, Achse und Prisma
- NT530P:
  - Pachymetrie rechts
  - Pachymetrie links
  - Tonometrie/korrigierter IOP rechts/links
  - perspektivisch externer AIS-Link auf Messprotokoll
- KR800:
  - Refraktion rechts/links
  - Keratometrie rechts/links
  - optional subjektive/VA-/PD-Daten

Das Export-/Mapping-Profil muss dafür mehrere Ausgabezeilen, mehrere Ziel-Feldkennungen und denselben Ziel-Feldcode mehrfach unterstützen.

### 14.4 Geräte-Dateianhänge und externe AIS-Links

Geräte-Dateianhang-Import und externe AIS-Link-Übergabe sind verbindliche zukünftige Anforderungen an den Geräteanbindungs-Baukasten. Im aktuellen Stand `0.1.0-prototype` sind diese Funktionen noch nicht produktiv umgesetzt.

Viele Untersuchungsgeräte können zusätzlich zur Messwertdatei Geräte-Dateianhänge erzeugen, z. B.:

- PDF
- JPG/JPEG
- PNG
- TIF/TIFF
- DCM
- TXT

Diese Dateien sollen künftig über die App ins AIS übernommen werden können. Die App soll dafür:

- einen Geräte-Dateianhang aus einem definierten Importordner übernehmen
- die Datei eindeutig umbenennen
- die umbenannte Datei in einen definierten Exportordner verschieben oder kopieren
- erst nach erfolgreicher Ablage einen externen AIS-Link in der XDT-Rückgabedatei erzeugen
- fehlende oder fehlerhafte Anhänge nachvollziehbar melden

Für MEDISTAR ist die Übergabe eines externen Links über XDT als Zielstruktur mit folgenden Feldkennungen zu dokumentieren:

| Feldkennung | Bedeutung |
| --- | --- |
| `6302` | Dokumentname / Anzeige in der Karteikarte |
| `6303` | Dateiformat, z. B. `PDF`, `JPG`, `DCM`, `TXT` |
| `6304` | optionale Beschreibung, z. B. `Messprotokoll Autorefraktor` |
| `6305` | vollständiger absoluter Dateipfad zur abgelegten Datei |

Anforderungen:

- `6304` ist ein optionales Beschreibungsfeld.
- `6305` muss einen vollständigen Pfad enthalten, z. B. einen lokalen Pfad oder UNC-/Netzwerkpfad.
- Die genaue AIS-Wirkung ist je AIS zu validieren.
- Die Beispieldatei `XDT Übergabe externer Link.txt` bestätigt diese Feldlogik für MEDISTAR und zeigt sowohl UNC-Pfade als auch lokale absolute Pfade.
- Die in MEDISTAR sichtbare Karteikartenanzeige mit `EV:{...}` wird durch MEDISTAR aus der XDT-Übergabe erzeugt. Unsere App soll nicht manuell eine `EV:{...}`-Textzeile pflegen, sondern die fachlichen XDT-Felder korrekt exportieren.
- Die bestehenden MEDISTAR-Zeilentypen für Messwerte bleiben davon getrennt. Der externe AIS-Link ist ein zusätzlicher Exportbaustein.

Schematische, anonymisierte XDT-Übergabe:

```text
<Len>6302PDF-Befund
<Len>6303PDF
<Len>6304Messwerte Autorefraktor
<Len>6305\\SERVER\Freigabe\Befunde\Patient_123.pdf
```

`<Len>` steht für das XDT-Längenpräfix. Es muss vom Exportgenerator berechnet werden und darf nicht manuell im Template gepflegt werden.

### 14.5 GA-Dateianhang Import und GA-Dateianhang Export

Schnittstellenprofile sollen für Geräte-Dateianhänge zwei optionale Ordner erhalten:

- `GA-Dateianhang Import`: Ordner, aus dem vom Untersuchungsgerät erzeugte Zusatzdateien übernommen werden.
- `GA-Dateianhang Export`: Ordner, in den die App umbenannte Zusatzdateien ablegt, damit das AIS sie über einen externen Link öffnen kann.

Diese Ordner sind optional und nur erforderlich, wenn ein Schnittstellenprofil Geräteanhänge ins AIS übernehmen soll. Ohne konfigurierte GA-Dateianhang-Ordner läuft die normale AIS-/Gerätedatei-Verarbeitung unverändert weiter. Die bestehende MEDISTAR/NIDEK-ARK1S-Verarbeitung bleibt unverändert.

### 14.6 Eindeutige Dateibenennung für Geräte-Dateianhänge

Für abgelegte Geräte-Dateianhänge muss ein eindeutiger Dateiname erzeugt werden. Standardvorschlag:

```text
{Ais.PatientNumber}_{Date:ddMMyyyy}_{Time:HHmmss}{ExtensionUpper}
```

Beispiel:

```text
11253_07052026_221723.PDF
```

Regeln:

- Die Patientennummer kommt aus AIS-Feld `3000`.
- Datum und Uhrzeit können zunächst aus dem Verarbeitungszeitpunkt kommen.
- Später kann profilabhängig auch Geräte-Untersuchungsdatum/-zeit verwendet werden.
- Der Dateinamenaufbau muss vorschlagbar, aber editierbar sein.
- Wenn eine Zieldatei bereits existiert, darf sie nicht überschrieben werden.
- Stattdessen muss ein eindeutiger Suffix ergänzt werden, z. B. `_001`, `_002`.

### 14.7 Sicherheitsanforderungen für Geräte-Dateianhänge

Für Geräte-Dateianhänge und externe AIS-Links gelten zusätzliche Sicherheitsanforderungen:

- Keine echten Patientendokumente in Templatepaketen.
- Keine Anhänge blind löschen.
- Keine Zieldateien überschreiben.
- Zielordner müssen sicher validiert werden.
- Root-, System- und unsichere Ordner dürfen nicht automatisch aktiv werden.
- Importierte GA-Ordnerpfade aus Templates müssen geprüft werden.
- Externe AIS-Links dürfen nur erzeugt werden, wenn die Datei erfolgreich im Zielordner abgelegt wurde.
- Bei Fehlern darf kein ungültiger Link erzeugt werden.
- Optionaler Anhang: Die Verarbeitung darf weiterlaufen, wenn das Profil dies so konfiguriert.
- Pflicht-Anhang: Die Verarbeitung muss abbrechen oder als Fehler markiert werden, wenn die Datei fehlt oder nicht abgelegt werden kann.

### 14.8 XML-Parser und Namespaces

Die App muss verschiedene XML-Varianten unterstützen.

Beispiele:

- NIDEK XML ohne komplexe Namespaces
- TOPCON/Ophthalmology XML mit JOIA-Namespaces

Anforderungen:

- XML-Parser muss Namespaces entweder normalisieren oder profilabhängig berücksichtigen können
- SourcePaths sollen für Anwender lesbar bleiben
- Namespace-Details dürfen den Mapping-Editor nicht unbedienbar machen
- Geräteprofil muss Parseroptionen definieren können:
  - Namespace ignorieren
  - Namespace beibehalten
  - bekannte Namespace-Präfixe verwenden
  - XPath-/Pfadmodus

### 14.9 MEDISTAR-Beispielausgaben aus Beispieldaten

NIDEK LM7:

```text
V0   R.:S=+ 6.50 Z=- 1.75*172 P=0.75 OUT 1.00 UP           PD= 59
V0   L.:S=+ 6.00 Z=- 2.25*  2 P=0.50 OUT 1.50 UP
```

NIDEK NT530P:

```text
Y  PR: 559 560 558 [559] µm
Y  PL: 559 560 [560] µm
P  R = 12 11 15 [12.7] // L = 14 13 15 [14.0] mmHg 14:51
```

NIDEK ARK1S:

```text
V1 R.:S=- 0.25 Z=- 0.25* 49                              PD=61
V1 L.:S=+ 0.00 Z=- 0.50* 63                              PD=61
```

---

## 15. Dokument-/Dateianhang-Template und spätere PDF-Erzeugung

### 15.1 Ziel

Das Dokument-/Dateianhang-Template ist künftig verbindlicher Bestandteil des Geräteanbindungs-Baukastens. Es beschreibt, wie bereits vom Gerät erzeugte Geräte-Dateianhänge erkannt, übernommen, eindeutig benannt, in einen AIS-erreichbaren Ordner übertragen und per externem AIS-Link referenziert werden.

Im aktuellen Stand `0.1.0-prototype` ist diese Funktion noch nicht produktiv umgesetzt. Der Prototyp erzeugt weiterhin MEDISTAR-kompatible XDT-Ergebniszeilen für Messwerte, aber noch keine produktiven externen AIS-Links.

Eine PDF-Erzeugung durch die App selbst bleibt ein separater späterer Fall. Dieser Schritt beschreibt vorrangig Geräteanhänge, die bereits als Datei vorliegen.

### 15.2 Dokument- und Dateitypen

Zu unterstützen sind perspektivisch vor allem vom Gerät erzeugte Zusatzdateien:

- PDF-Messprotokolle
- JPG/JPEG- oder PNG-Bilder
- TIF/TIFF-Bilder
- DICOM-Dateien (`DCM`)
- TXT- oder andere technische Begleitdateien

Selbst erzeugte PDF-Messprotokolle können später zusätzlich entstehen, wenn ein Gerät nur maschinenlesbare Werte liefert.

### 15.3 Konfigurierbare Aktivierung

Pro Schnittstellenprofil soll konfigurierbar sein:

- Geräte-Dateianhang-Verarbeitung aus: keine Anhangsuche und kein externer AIS-Link
- optionaler Anhang: Verarbeitung läuft weiter, wenn der Anhang fehlt
- Pflicht-Anhang: Verarbeitung wird als Fehler markiert, wenn der Anhang fehlt oder nicht abgelegt werden kann
- Übertragungsmodus: kopieren oder verschieben
- Dateinamen-Template
- GA-Dateianhang Import
- GA-Dateianhang Export
- Link-Export über AIS-spezifische Feldkennungen

### 15.4 Dokumentinhalt und Templatebezug

Ein Dokument-/Dateianhang-Template soll keine medizinischen Inhalte blind interpretieren. Es beschreibt technische und fachliche Metadaten für den Link:

- Dokumentname für die Karteikarte
- Dateiformat
- optionale Beschreibung
- finaler Dateipfad
- Zuordnung zu AIS-/Geräte-/Exportprofil
- Pflicht- oder Optionalstatus
- erlaubte Dateiendungen
- Dateinamenaufbau

Echte Patientendokumente dürfen nicht Bestandteil eines Templatepakets sein.

### 15.5 Ablage und Ordnerstruktur

Der Ablageort für Geräte-Dateianhänge muss frei konfigurierbar sein.

Erlaubt sein sollen:

- lokale Ordner
- Netzwerkpfade
- UNC-Pfade, z. B. `\\SERVER\Freigabe\XdtBridge\Dokumente`

Anforderungen:

- `GA-Dateianhang Import` pro Schnittstellenprofil konfigurierbar
- `GA-Dateianhang Export` pro Schnittstellenprofil konfigurierbar
- Schreibrechte prüfen
- Zielordner muss existieren oder bewusst erstellt werden können
- Dateinamen müssen eindeutig erzeugt werden
- vorhandene Dateien dürfen nicht überschrieben werden
- Pfade müssen durch `FolderSafetyValidator` oder vergleichbare Logik geprüft werden
- keine Ablage in Systemordnern oder unsicheren Root-Pfaden

Eine konfigurierbare Ordnerstruktur kann später ergänzt werden, z. B. nach Jahr/Monat, Gerät, Patientennummer oder Untersuchungsdatum.

### 15.6 MEDISTAR externer Link über XDT

Für MEDISTAR soll die externe Link-Übergabe über XDT-Feldkennungen erfolgen. Zielstruktur:

| Feldkennung | Bedeutung |
| --- | --- |
| `6302` | Dokumentname / Anzeige in der Karteikarte |
| `6303` | Dateiformat, z. B. `PDF`, `JPG`, `DCM`, `TXT` |
| `6304` | optionale Beschreibung |
| `6305` | vollständiger absoluter Dateipfad zur abgelegten Datei |

Die XDT-Zeilen müssen wie alle anderen XDT-Zeilen korrekt durch den Exportgenerator mit Längenpräfix erzeugt werden. Die Benutzeroberfläche darf keine manuelle Pflege der XDT-Zeilenlängen verlangen.

Ein externer AIS-Link darf nur erzeugt werden, wenn die referenzierte Datei erfolgreich im Zielordner abgelegt wurde.

Die ausgewertete MEDISTAR-Beispieldatei zeigt, dass `6304` in einer echten Übergabe genutzt werden kann, aber für einfache Fälle auch eine Übergabe mit `6302`, `6303` und `6305` möglich ist. `6304` bleibt deshalb als optionales Beschreibungsfeld definiert.

### 15.7 Zusammenspiel mit bestehenden Gerätedateien

Die App muss zwei Fälle unterscheiden können:

Fall A:

Das Gerät liefert selbst eine Zusatzdatei, z. B. JPG, PDF oder DICOM. Die App ordnet diese Datei der Messung zu, kopiert oder verschiebt sie in den GA-Dateianhang Export und erzeugt einen externen AIS-Link.

Fall B:

Das Gerät liefert nur maschinenlesbare Werte. Die App kann später selbst ein PDF-Messprotokoll aus den gelesenen Werten erzeugen und dieses ebenfalls über einen externen AIS-Link referenzieren. Dieser Fall ist eine separate spätere Erweiterung.

### 15.8 Verhalten bei Fehlern

Wenn die Geräte-Dateianhang-Verarbeitung fehlschlägt:

- Fehler muss protokolliert werden.
- Benutzer muss eine verständliche Meldung erhalten.
- Bei optionalem Anhang darf die Messwertverarbeitung weiterlaufen.
- Bei Pflicht-Anhang muss die Verarbeitung abbrechen oder als Fehler markiert werden.

Wenn ein externer AIS-Link erzeugt werden soll, aber die Datei nicht geschrieben werden konnte:

- Es darf kein ungültiger Link erzeugt werden.
- Der Fehler muss sichtbar sein.
- Die fehlerhafte Anhangverarbeitung darf keine unbekannten Dateien löschen oder überschreiben.

### 15.9 Datenschutz und Sicherheit

Geräte-Dateianhänge können Patientendaten und medizinische Messwerte enthalten.

Anforderungen:

- Ablage nur in konfigurierten Ordnern.
- Keine automatische Ablage in unsicheren temporären Ordnern.
- Keine echten Patientendokumente in Templatepaketen.
- Keine Anhänge blind löschen.
- Keine Zieldateien überschreiben.
- Zugriffsschutz liegt beim Praxis-/Windows-/Netzwerkrechtekonzept.
- Pfade und Dateinamen dürfen keine unnötigen Patientendaten enthalten, sofern vermeidbar.
- Protokollierung soll keine vollständigen medizinischen Inhalte unnötig duplizieren.
- Dokument-/Dateianhang-Export muss im AVV/Datenschutzkonzept berücksichtigt werden.

### 15.10 Abgrenzung `0.1.0-prototype`

Für den aktuellen Prototyp gilt:

- MEDISTAR-kompatible XDT-Ergebniszeilen sind produktiv/praktisch für MEDISTAR + NIDEK ARK1S validiert.
- Geräte-Dateianhang-Import ist noch nicht produktiv umgesetzt.
- MEDISTAR externer Link über XDT ist noch nicht produktiv umgesetzt.
- Selbst erzeugte PDF-Protokolle sind noch nicht umgesetzt.

Die Geräte-Dateianhang-Verarbeitung und externe AIS-Link-Übergabe sind keine optionalen Ideen mehr, sondern verbindliche zukünftige Anforderungen. Sie bleiben aber klar vom aktuellen Iststand getrennt.

---

## 16. Geräte-Datei-Explorer und Profil-Assistent für unbekannte Geräte

### 16.1 Ziel

Die Anwendung soll perspektivisch nicht nur bekannte Geräteprofile verwenden, sondern auch unbekannte oder neue Gerätedateien explorativ analysieren können.

Ziel ist, dass ein Systembetreuer eine Beispieldatei eines neuen Gerätes laden kann und die App daraus Vorschläge für ein neues Geräteprofil erzeugt.

Der Geräte-Datei-Explorer soll dabei ausdrücklich als Hilfs- und Analysewerkzeug verstanden werden. Er ersetzt keine fachliche Validierung durch Systembetreuer, Praxis oder Herstellerinformationen.

### 16.2 Unterstützte Eingangstypen

Der Assistent soll perspektivisch verschiedene Dateitypen analysieren können:

- XML
- GDT/XDT
- TXT
- CSV
- JSON
- proprietäre Textformate
- Dateien mit Begleitdateien, z. B. JPG/PDF

Bei Dateien mit Begleitdateien soll der Assistent erkennen können, ob zusätzliche Dateien im gleichen Ordner, in Unterordnern oder über Dateiverweise in der Hauptdatei vorhanden sind.

### 16.3 Automatische Erkennung

Die App soll versuchen, technische Eigenschaften der Beispieldatei automatisch zu erkennen:

- Dateityp
- Zeichensatz
- XML-Struktur
- Namespaces
- wiederholte Messgruppen
- Attribute
- Tabellenstrukturen
- Key-Value-Strukturen
- mögliche Messwertpfade
- mögliche Begleitdateien
- Datum/Uhrzeit
- Hersteller/Modell, falls in der Datei enthalten

Die automatische Erkennung soll transparent anzeigen, welche Annahmen sicher erkannt wurden und welche nur vermutet sind.

### 16.4 Extraktion technischer Werte

Die App soll aus einer Beispieldatei alle technisch erkennbaren Werte extrahieren und als Kandidaten anzeigen.

Für jeden Kandidaten sollen mindestens angezeigt werden:

- SourcePath
- Rohwert
- Datentyp-Vermutung
- Gruppe
- Auge rechts/links, falls erkennbar
- mögliche Bedeutung
- erkannte Einheit
- Wiederholungsnummer, falls vorhanden
- Beispielwert

Die Kandidatenliste soll auch Werte anzeigen können, deren medizinische Bedeutung noch unklar ist. Solche Werte müssen entsprechend markiert werden, z. B. als "Bedeutung noch zu prüfen".

### 16.5 Vorschlagslogik

Die App darf aus technischen Namen und Beispielwerten Vorschläge für verständliche Messwertnamen erzeugen.

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

Diese Vorschläge sind Hilfen und müssen vom Systembetreuer geprüft werden. Die App soll Vorschläge als solche kennzeichnen und nicht als gesicherte medizinische Interpretation darstellen.

### 16.6 Keine blinde medizinische Interpretation

Die App darf unbekannte Werte nicht ungeprüft als medizinisch korrekt interpretieren.

Der Systembetreuer muss relevante Werte bestätigen, benennen und dem Exportprofil zuordnen.

Besonders kritisch sind:

- medizinische Einheiten
- korrigierte oder berechnete Werte
- Mittelwerte gegenüber Einzelmessungen
- rechte/linke Zuordnung
- subjektive gegenüber objektiver Refraktion
- Messgruppen mit Wiederholungsnummern
- herstellerspezifische Sonderfelder

### 16.7 Profil-Assistent

Die App soll perspektivisch einen Assistenten für neue Geräteprofile erhalten.

Vorgesehener Ablauf:

1. Gerätename, Hersteller und Gerätetyp eingeben.
2. Beispieldatei laden.
3. Datei analysieren und erkannte Werte anzeigen.
4. Relevante Werte auswählen.
5. Verständliche Namen vergeben oder Vorschläge übernehmen.
6. Exportziel auswählen, z. B. MEDISTAR, ALBIS oder TURBOMED.
7. Export-Template erstellen oder vorgeschlagenes Template übernehmen.
8. Exportvorschau mit Beispieldaten prüfen.
9. Profil speichern und optional als Template exportieren.

Der Assistent soll das entstehende Geräteprofil, Exportprofil und optional ein vollständiges Schnittstellenprofil nachvollziehbar erzeugen. Vor einer produktiven Nutzung muss der Benutzer die vorgeschlagenen Zuordnungen bestätigen.

### 16.8 Umgang mit mehreren Beispieldateien

Für neue Geräte soll es möglich sein, mehrere Beispieldateien zu laden.

Zweck:

- unterschiedliche Patientenwerte prüfen
- rechte/linke Werte validieren
- optionale Werte erkennen
- fehlende Werte identifizieren
- Wiederholungsmessungen verstehen
- Stabilität der SourcePaths prüfen

Die App soll Unterschiede zwischen Beispieldateien sichtbar machen können, z. B. Felder, die nur in manchen Dateien vorkommen, unterschiedliche Wiederholungszahlen oder abweichende Einheiten.

### 16.9 Validierung neuer Profile

Bevor ein neues Profil produktiv verwendet werden kann, muss es validiert werden.

Mindestens zu prüfen:

- Pflichtwerte vorhanden
- Exportvorschau plausibel
- keine unbekannten Pflichtplatzhalter
- Zielfeldkennungen gültig
- Testexport erzeugbar
- optional Testimport ins AIS

Die Validierung soll verständlich anzeigen, welche Punkte erfüllt sind, welche Warnungen bestehen und welche Fehler eine produktive Nutzung verhindern.

### 16.10 Grenzen der Automatik

Die App soll klar kommunizieren:

- Automatische Erkennung ist nur ein Vorschlag.
- Medizinische Bedeutung muss durch Systembetreuer/Praxis validiert werden.
- Herstellerformate können sich ändern.
- Profile müssen nach Softwareupdates des Geräts erneut geprüft werden.

Die App darf einen neuen Geräteanschluss nicht allein aufgrund automatisch erkannter Werte als produktionsbereit markieren.

### 16.11 Abgrenzung Version 1

Für die aktuelle Version wird dieser Assistent noch nicht implementiert.

Die aktuelle App unterstützt:

- bekannte Profile
- Anzeige erkannter Werte
- manuelle Entwurfsbearbeitung

Der Geräte-Datei-Explorer und Profil-Assistent sind zukünftige Ausbaustufen. Sie sollen später auf den bestehenden V2-Profilmodellen, der Template-Logik und der Exportvorschau aufbauen, ohne die validierte MEDISTAR/NIDEK-ARK1S-Verarbeitung zu gefährden.

---

## 17. Augenärztliche Gerätewerte, MEDISTAR-Zeilenbilder und Messwertbedeutung

### 17.1 Quelle und Zweck

Diese Ergänzung basiert auf der Auswertung des Dokuments `MEDISTAR Geräteanbindung Augenarzt - Erklärung der Werte`.

Ziel ist, aus den Beispielen der MEDISTAR-Karteikartendarstellung fachliche Anforderungen an die XDT-Bridge-App abzuleiten.

Die im Dokument genannten MEDISTAR-Zeilentypen wie `V0`, `V1`, `V2`, `V3`, `V7`, `P` und `Y` sind MEDISTAR-interne Karteikarten-Zeilenbezeichnungen. Sie werden durch MEDISTAR nachgelagert erzeugt bzw. in der Karteikarte angezeigt.

Für die App sind diese Zeilentypen:

- informativ
- als Orientierung für manuell gepflegte MEDISTAR-orientierte Exportprofile relevant
- hilfreich zur Benennung von Exportprofilen
- hilfreich zur Darstellung im Mapping-/Template-Editor

Sie sind aber:

- nicht zwingender Bestandteil des XDT-Exports
- nicht allgemein gültig für andere AIS/PVS-Systeme
- nicht als harte Schnittstellenlogik zu behandeln

Andere AIS/PVS-Systeme können andere Zeilenarten verwenden oder vollständig ohne Zeilentypen arbeiten.

Die App muss deshalb drei Ebenen klar unterscheiden:

1. Technische XDT-/GDT-Ausgabe: Feldkennungen, Werte, Ergebnisfelder, Satzart, Zeichensatz.
2. AIS-spezifische Darstellung: z. B. MEDISTAR-Zeilentypen `V0`, `V1`, `P`, `Y`.
3. Fachliche Bedeutung der Messwerte: Sphäre, Zylinder, Achse, Addition, Augendruck, Pachymetrie, Hornhautradien usw.

### 17.1.1 Zurückgestelltes Thema: AIS-/MEDISTAR-Exporttemplate-Defaults

AIS-/MEDISTAR-spezifische Default-Exporttemplates werden derzeit nicht als verbindliche Umsetzungsanforderung geführt. Das Konzept wird fachlich neu bewertet.

Bis dahin gilt:

- Exportprofile bleiben über den Baukasten manuell konfigurierbar.
- Die bestehende validierte MEDISTAR/NIDEK-ARK1S-Verarbeitung bleibt unverändert.
- Es erfolgt keine automatische Ergänzung von `6330`-Mensch-Zeilen.
- Es erfolgt keine automatische Änderung bestehender Exportprofile.
- BuiltIn-Profile werden nicht verändert.
- Aus den folgenden MEDISTAR-Zeilentypen und Beispielausgaben darf aktuell keine automatische Default-Template-Logik abgeleitet werden.

### 17.2 Informative MEDISTAR-Zeilentypen

Die folgenden Zeilentypen dienen nur als informative Orientierung für MEDISTAR-orientierte manuelle Exportprofile. Sie dürfen nicht als allgemeingültige XDT-Regel und nicht als verbindliche Default-Template-Vorgabe betrachtet werden.

| Zeilentyp | Bedeutung in MEDISTAR | Typische Geräteklasse | Informative Orientierung |
|---|---|---|---|
| `V0` | Brillenwerte / Lensmeter | Lensmeter, z. B. NIDEK LM-1800P, NIDEK LM7, TOPCON CL300 | mögliche manuelle Ausgabe für rechtes und linkes Auge mit Sphäre, Zylinder und Achse; Prisma, Basisrichtung, Addition und PD nur nach fachlicher Prüfung |
| `V1` | objektive Refraktion | Autorefraktor, z. B. NIDEK ARK1S, Topcon TRK-2P/KR800 | mögliche manuelle Ausgabe mit Sphäre, Zylinder und Achse rechts/links; optional SE und PD |
| `V2` | Phoropter / subjektive Refraktion | Phoropter, subjektive Refraktionsdaten | mögliche manuelle Ausgabe mit Fernwertkennzeichnung, Sphäre, Zylinder, Achse, Addition rechts/links; optional Visus/VA |
| `V3` | Rezeptwerte | AIS-interner Rezeptprozess | zunächst informativ; keine primäre Exportanforderung der Geräteanbindung |
| `V7` | Hornhautradien / Keratometrie | Keratometer, Kombigeräte | mögliche manuelle Ausgabe mit R1/K1, R2/K2, Achsen, Durchschnitt/AV und errechnetem Astigmatismus/ra je Auge |
| `P` | Augendruck / NCT / Tonometrie | Non-Contact-Tonometer | mögliche manuelle Ausgabe mit Einzelmessungen, Mittelwert, Einheit mmHg/Torr und Messzeit rechts/links |
| `Y` | Pachymetrie oder korrigierter IOP | Pachymeter, Tonometer/Pachymeter-Kombigeräte | mögliche manuelle Ausgabe mit Hornhautdicke, Mittelwerten und Einheit mm oder µm; korrigierten IOP als eigenen Werttyp behandeln |

Wichtig: `Y` kann in medizinischen Einrichtungen auch für andere Inhalte, z. B. Laborwerte, verwendet werden. `Y` darf daher nicht hart als universeller Pachymetrie-Exporttyp kodiert werden.

### 17.3 Fachliche Messwertdefinitionen

Die App muss erkannte technische Werte nicht nur als Rohpfade, sondern mit fachlicher Bedeutung anzeigen und für Templates nutzbar machen.

| Messwert | Bedeutung | Einheit / Format | Anforderung |
|---|---|---|---|
| Sphäre | sphärische Korrektur; `+` = Weitsichtigkeit, `-` = Kurzsichtigkeit | Dioptrien | numerisch erkennen, benennen und mit `Diopter` formatieren |
| Zylinder | astigmatische Korrektur | Dioptrien | numerisch erkennen, benennen und mit `Diopter` formatieren |
| Achse | Lage der Zylinderwirkung | Grad | erkennen und profilabhängig mit `Axis` formatieren, z. B. `*180` oder `* 49` |
| Addition | Nahzusatz bei Fern-/Nahwerten | Dioptrien, typischerweise positiv | als eigenen Messwerttyp unterstützen, z. B. `R_Add`, `L_Add` |
| Fernwert | Werte für Fernsicht, in MEDISTAR oft `F` | Textbestandteil einer manuell gepflegten Ausgabe | Fern-/Nahwert-Ausgaben ermöglichen, ohne `F` als harte Schnittstellenlogik zu behandeln |
| IOP / Augendruck | Augeninnendruck | mmHg oder Torr | Einzelmessungen, Mittelwert und Messzeit darstellen; Formatfunktion `Iop` |
| Pachymetrie / Hornhautdicke | Hornhautdicke | mm oder µm | Einheit und Darstellung profilabhängig behandeln; Formatfunktion `Pachy` |
| Keratometrie / Hornhautradien | R1/R2 bzw. K1/K2, AV und errechneter Astigmatismus | mm oder optional Dioptrien | als eigene Messgruppe unterstützen; Formatfunktionen `Keratometry`, `Axis`, `Diopter` |

Die App darf medizinische Korrekturen, z. B. korrigierten IOP nach Dresdner Korrekturtabelle, nicht eigenständig berechnen, solange dies nicht fachlich validiert und explizit spezifiziert wurde. Wenn ein Gerät bereits korrigierte Werte liefert, dürfen diese übernommen werden. Eigene Berechnungslogik wäre ein gesondertes Medizinprodukt-/Haftungsthema und ist nicht Bestandteil der aktuellen Version.

### 17.4 Manuelle Exportprofil-Konfiguration und zurückgestellte Default-Templates

Die Beispiele zeigen, dass die Ausgabe stark von Gerätetyp, AIS und gewünschter Karteikartendarstellung abhängt. Sie sind keine verbindlichen Default-Templates und dürfen nicht automatisch in bestehende Profile oder BuiltIns übernommen werden.

Die manuelle Exportprofil-Konfiguration soll weiterhin unterstützen:

- mehrere Ergebniszeilen pro Untersuchung
- denselben Ziel-Feldcode mehrfach verwenden, z. B. `6228`
- Ergebniszeilen mit mehreren Platzhaltern aufbauen
- Ausgabe in medizinisch lesbarer Syntax
- Zeilentyp-Hinweise informativ speichern
- Export je AIS unterschiedlich konfigurieren
- Formatfunktionen pro Platzhalter anwenden
- manuelle Bearbeitung der Ergebniszeile erlauben
- Platzhalter per Baukasten einfügen
- Gesamtexport-Vorschau anzeigen

Informative Beispiele für MEDISTAR-orientierte, aber nicht MEDISTAR-hartkodierte Ergebniszeilen:

```text
R.:S={R_Sphere:Diopter} Z={R_Cylinder:Diopter}*{R_Axis:Axis}
L.:S={L_Sphere:Diopter} Z={L_Cylinder:Diopter}*{L_Axis:Axis}

F R.:S={R_Sphere:Diopter} Z={R_Cylinder:Diopter}*{R_Axis:Axis} A={R_Add:Diopter}
F L.:S={L_Sphere:Diopter} Z={L_Cylinder:Diopter}*{L_Axis:Axis} A={L_Add:Diopter}

R = {R_IOP_1:Iop} [{R_IOP_AVG:Iop}] mmHg {Device.Time}
L = {L_IOP_1:Iop} [{L_IOP_AVG:Iop}] mmHg {Device.Time}

PR: {R_Pachy_1:Pachy} {R_Pachy_2:Pachy} {R_Pachy_3:Pachy} [{R_Pachy_Avg:Pachy}] mm
PL: {L_Pachy_1:Pachy} {L_Pachy_2:Pachy} {L_Pachy_3:Pachy} [{L_Pachy_Avg:Pachy}] mm

R: R1={R_R1:Keratometry}*{R_R1_Axis:Axis} R2={R_R2:Keratometry}*{R_R2_Axis:Axis} //
R: AV={R_AV:Keratometry} //
R: ra={R_RA:Diopter}*{R_RA_Axis:Axis} //
```

### 17.5 Anforderungen an Formatfunktionen

Die App muss folgende Formatfunktionen unterstützen oder perspektivisch unterstützen:

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

Bereits implementierte Formatfunktionen sollen weitergeführt werden. Neue Funktionen müssen rückwärtskompatibel ergänzt werden.

Besonders wichtig:

- Dioptrienwerte mit Vorzeichen und optionalem Leerzeichen
- Achsenwerte rechtsbündig oder roh, je Template
- IOP ohne ungewollte Rundung
- Pachymetrie ohne ungewollte Umrechnung
- Keratometrie ohne ungewollte Rundung
- Zeitwerte im Format `HH:mm`, wenn aus der Gerätedatei verfügbar

### 17.6 Anforderungen an Baukasten und Exportregel-Editor

Der Baukasten muss die fachliche Bedeutung der Werte berücksichtigen.

Pro erkanntem Wert soll angezeigt werden:

- technischer Platzhalter
- verständlicher Messwertname
- ausgelesener Wert
- Ausgabeart AIS/Mensch
- Verwenden-Haken
- vorgeschlagene Formatfunktion
- Gruppe/Untersuchungsart
- Auge rechts/links, falls erkennbar
- Einzelmessung oder Mittelwert, falls erkennbar

Beispiele für verständliche Namen:

- Sphäre rechts
- Zylinder rechts
- Achse rechts
- Sphäre links
- Zylinder links
- Achse links
- Addition rechts
- Augendruck rechts Einzelmessung 1
- Augendruck rechts Mittelwert
- Pachymetrie rechts Mittelwert
- Hornhautradius R1 rechts
- Hornhautradius R2 rechts
- Keratometrie Durchschnitt rechts
- errechneter Zylinder rechts

Die App muss dem Anwender ermöglichen:

- Werte per Haken in den Exportregel-Entwurf einzufügen
- zwischen technischer AIS-Ausgabe und menschenlesbarer Ausgabe zu wählen
- Ergebniszeile manuell nachzubearbeiten
- neue Ergebniszeilen temporär zu erstellen
- Gesamtexport-Vorschau zu prüfen
- später Entwurf als neues Exportprofil zu speichern

### 17.7 Anforderungen an weitere Geräteprofile

Aus der fachlichen Auswertung ergeben sich folgende spätere Profilanforderungen:

| Profiltyp | Muss unterstützen | Informative MEDISTAR-Orientierung |
|---|---|---|
| Lensmeter-Profil | Sphäre, Zylinder, Achse, optional Addition, zweite Addition, Nahsphäre, Prisma/Basisrichtung, UV-Transmission, ConfidenceIndex/Error und PD; für NIDEK LM7/LM7P LAN-XML `NIDEK_V1.00` und `NIDEK_V1.01` | `V0` |
| Autorefraktor-Profil | Sphäre, Zylinder, Achse, PD, optional SE | `V1` |
| Phoropter-Profil | Fernwerte, Sphäre, Zylinder, Achse, Addition | `V2` |
| Tonometer-Profil | IOP-Einzelwerte, IOP-Mittelwert, Messzeit, Einheit mmHg/Torr | `P` |
| Pachymeter-Profil | Hornhautdicke-Einzelwerte, Mittelwert, Einheit mm oder µm | `Y`, nicht hart kodieren |
| Keratometer-Profil | R1/R2, Achsen, Durchschnitt AV, errechneter Zylinder ra | `V7` |

### 17.8 Abgrenzung

Diese Ergänzung implementiert keine neuen Geräteprofile.

Sie implementiert außerdem keine AIS-/MEDISTAR-Default-Exporttemplates und keine automatische `6330`-Zusatzzeile.

Die Informationen dienen zur:

- Verbesserung des Pflichtenhefts
- Verbesserung des Baukasten-/Exportregel-Editors
- besseren Benennung von Messwerten
- besseren Formatvorschlägen
- Vorbereitung weiterer Profile nach gesonderter fachlicher Freigabe

Die MEDISTAR-Zeilentypen bleiben informativ und werden nicht als allgemeingültige XDT-Schnittstellenlogik festgelegt.
