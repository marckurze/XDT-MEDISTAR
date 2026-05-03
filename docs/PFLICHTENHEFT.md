# Pflichtenheft – XdtDeviceBridge

## 1. Projektbezeichnung

**Projektname:** XdtDeviceBridge  
**Arbeitstitel:** PraxisBridge XDT  
**Zielplattform:** Windows-PC in Arztpraxis / Augenarztpraxis  
**Technologieempfehlung:** .NET 8 oder höher, C#, WPF, SQLite, xUnit  
**Betriebsart:** Lokale Desktop-Anwendung mit Ordnerüberwachung

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

Die Anwendung arbeitet nach folgendem Ablauf:

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
- lokales Konfigurationsverzeichnis
- lokale SQLite-Datenbank

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

SQLite

Zu speichern sind:

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

Die Anwendung muss pro Profil konfigurierbare Optionen für Import- und Exportordner erhalten. Diese Optionen dienen dazu, alte Patientendateien, Gerätedateien oder Rückgabedateien kontrolliert aus Arbeitsordnern zu entfernen, ohne den Praxisbetrieb zu gefährden.

### 9.1 AIS-Importordner

Vor dem Warten auf eine neue AIS-Datei darf optional der AIS-Importordner geleert werden.

- Option: `AIS-Importordner vor neuer Verarbeitung leeren`
- Zweck: alte Patientendateien aus vorherigen Untersuchungen entfernen
- Sicherheitsanforderung: Löschen nur im konfigurierten Ordner, niemals rekursiv außerhalb des Profilordners

### 9.2 Geräte-Importordner

Vor dem Warten auf eine neue Gerätedatei darf optional der Geräte-Importordner geleert werden.

- Option: `Geräte-Importordner vor neuer Untersuchung leeren`
- Zweck: alte Messdateien vorheriger Untersuchungen entfernen
- Sicherheitsanforderung: Löschen nur im konfigurierten Ordner, niemals rekursiv außerhalb des Profilordners

### 9.3 Exportordner zum AIS

Nach erfolgreicher Übertragung oder Verarbeitung darf optional der Exportordner bereinigt werden.

- Option: `Exportordner nach erfolgreicher Übertragung leeren`
- Zweck: verhindern, dass alte Rückgabedateien erneut vom AIS verarbeitet werden oder den Betrieb stören
- Diese Option muss getrennt von den Importordner-Optionen konfigurierbar sein

### 9.4 Sicherheitsanforderungen für Löschoptionen

- Die Löschoptionen müssen pro Geräte-/Exportprofil konfigurierbar sein.
- Löschen darf nur nach klarer Benutzerkonfiguration erfolgen.
- Standardmäßig müssen Löschoptionen deaktiviert sein.
- Optional soll statt endgültigem Löschen auch `Verschieben in Archivordner` möglich sein.
- Fehler beim Löschen müssen protokolliert und sichtbar gemacht werden.
- Dateien, die gerade gesperrt oder in Verarbeitung sind, dürfen nicht gelöscht werden.
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
- Option: AIS-Importordner vor Verarbeitung leeren
- Option: Geräte-Importordner vor Verarbeitung leeren
- Option: Exportordner nach erfolgreicher Übertragung leeren
- Option: Dateien nach Verarbeitung archivieren statt löschen
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
- vollständige Schnittstellenprofile als Kombi-Template

Definitionen:

- AIS-Template: beschreibt generelle Anforderungen eines Arztinformationssystems, z. B. MEDISTAR, ALBIS oder TURBOMED.
- Geräte-Template: beschreibt generelle Parser- und Messwertlogik eines Gerätes oder Gerätetyps, z. B. NIDEK ARK1S, Zeiss Humphrey oder ein Topcon-Gerät.
- Export-/Mapping-Template: beschreibt, wie ausgelesene Werte in XDT/GDT-Ausgabefelder, Ergebniszeilen oder Ergebnisblöcke geschrieben werden.
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
- Pfade dürfen beim Import nicht blind übernommen werden, ohne dass der Benutzer sie prüfen kann

Anforderung:

Nach dem Import muss die App die Pfade prüfen und den Benutzer auffordern, fehlende oder ungültige Ordner neu zu setzen.

### 12.7 Sicherheit beim Import

Der Import darf keine gefährlichen Einstellungen ungeprüft aktivieren.

Besonders kritisch:

- automatische Löschoptionen
- Ordnerbereinigung
- Exportordner nach Übertragung leeren
- Pfade zu Root-Verzeichnissen oder Systemordnern
- unbekannte oder fehlerhafte Mapping-Regeln

Anforderungen:

- Löschoptionen sollen nach Import standardmäßig überprüft werden müssen.
- Gefährliche Pfade dürfen nicht automatisch aktiv werden.
- Importierte Profile müssen validiert werden.
- Fehlerhafte oder unvollständige Profile dürfen nicht produktiv aktiviert werden.
- Die App muss verständlich anzeigen, welche Punkte nach dem Import geprüft werden müssen.

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

Für die aktuelle Version wird noch keine Umsetzung von EV-Verweisen, PDF-/JPG-Ablage oder Mehruntersuchungs-Profilen implementiert. Die Beispieldaten dienen zunächst zur Erweiterung des Pflichtenhefts und der Architektur. Die funktionsfähige MEDISTAR/NIDEK-ARK1S-Verarbeitung bleibt unverändert.

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
  - EV-Verweis auf Messprotokoll
- KR800:
  - Refraktion rechts/links
  - Keratometrie rechts/links
  - optional subjektive/VA-/PD-Daten

Das Export-/Mapping-Profil muss dafür mehrere Ausgabezeilen, mehrere Ziel-Feldkennungen und denselben Ziel-Feldcode mehrfach unterstützen.

### 14.4 Zusatzdateien, Attachments und Dokumentenverweise

Die App muss perspektivisch Begleitdateien unterstützen.

Beispiele:

- NIDEK NT530P erzeugt zusätzlich JPG-Dateien.
- Andere Geräte können PDF-Protokolle erzeugen.
- Die XML-Datei kann auf Zusatzdateien verweisen, z. B. `PACHYImage`.

Anforderungen:

- zugehörige Begleitdateien zur Messung erkennen
- Begleitdateien anhand Dateinamen, Zeitstempel oder XML-Verweis zuordnen
- Begleitdateien in eine definierte Zielordnerstruktur kopieren
- Zielordnerstruktur pro AIS/Profil konfigurierbar machen
- optional vorhandene Dateien nicht überschreiben
- Dateinamen eindeutig erzeugen
- Fehler bei fehlenden Begleitdateien protokollieren
- Begleitdateien dürfen keine Verarbeitung blockieren, wenn sie als optional markiert sind
- Begleitdateien müssen Verarbeitung blockieren, wenn sie als erforderlich markiert sind

### 14.5 MEDISTAR EV-Verweise

Für MEDISTAR muss perspektivisch ein Mechanismus für externe Verweise unterstützt werden.

Beispiel:

```text
EV:{000000003B} NT-530P Messung
```

Durch Doppelklick in MEDISTAR kann ein hinterlegtes Dokument oder Messprotokoll geöffnet werden.

Anforderungen:

- EV-Verweise müssen pro Exportprofil konfigurierbar sein
- EV-Kennung/Schlüssel muss erzeugt oder übernommen werden können
- Ablagepfad für Dokumente muss konfigurierbar sein
- Windows-Ordnerstruktur muss pro AIS-Profil definierbar sein
- Exportregel muss Ergebnistext und EV-Verweis kombinieren können

Beispielausgabe:

```text
P  R = 12 11 15 [12.7] // L = 14 13 15 [14.0] mmHg 14:51 / EV:{...} NT-530P Messung
```

Hinweis: Die exakte MEDISTAR-EV-Struktur muss später anhand funktionierender Praxisbeispiele validiert werden.

### 14.6 XML-Parser und Namespaces

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

### 14.7 MEDISTAR-Beispielausgaben aus Beispieldaten

NIDEK LM7:

```text
V0   R.:S=+ 6.50 Z=- 1.75*172 P=0.75 OUT 1.00 UP           PD= 59
V0   L.:S=+ 6.00 Z=- 2.25*  2 P=0.50 OUT 1.50 UP
```

NIDEK NT530P:

```text
Y  PR: 559 560 558 [559] µm
Y  PL: 559 560 [560] µm
P  R = 12 11 15 [12.7] // L = 14 13 15 [14.0] mmHg 14:51 / EV:{...} NT-530P Messung
```

NIDEK ARK1S:

```text
V1 R.:S=- 0.25 Z=- 0.25* 49                              PD=61
V1 L.:S=+ 0.00 Z=- 0.50* 63                              PD=61
```

---

## 15. Optionale Dokumentenerzeugung und EV-Verknüpfung

### 15.1 Ziel

Die Anwendung soll perspektivisch optional aus den eingelesenen AIS- und Gerätedaten ein lesbares Messprotokoll erzeugen können.

Hintergrund: Viele Gerätedateien sind maschinenlesbar, aber für Ärzte nicht angenehm lesbar. Ein automatisch erzeugtes PDF-Messprotokoll kann die gemessenen Werte übersichtlich darstellen und bei Bedarf ausgedruckt oder über einen MEDISTAR-EV-Verweis geöffnet werden.

### 15.2 Dokumenttyp

Die bevorzugte Ausgabeform für selbst erzeugte Protokolle ist PDF.

Begründung:

- gut lesbar
- druckbar
- archivierbar
- mehrseitig möglich
- geeignet für Messprotokolle
- optional mit Praxislogo oder Layout erweiterbar

JPEG oder andere Bildformate sollen nur verwendet werden, wenn das Gerät selbst solche Dateien liefert oder ein AIS dies ausdrücklich benötigt.

### 15.3 Optionale Aktivierung

Die PDF-/Dokumentenerzeugung muss pro Schnittstellenprofil optional aktivierbar sein.

Konfigurierbare Optionen:

- Dokument erzeugen: ja/nein
- Dokumenttyp, zunächst PDF
- Dokument zusätzlich zum XDT-Export erzeugen
- Dokument nur erzeugen, wenn Verarbeitung erfolgreich war
- Dokumenterzeugung als Pflicht oder optional
- Verhalten bei Fehlern der Dokumenterzeugung

### 15.4 Dokumentinhalt

Das erzeugte PDF soll aus AIS-Daten und Gerätewerten aufgebaut werden können.

Mögliche Inhalte:

- Praxis-/Systemhinweis
- Patientennummer
- Patientenname
- Geburtsdatum
- Untersuchungsart
- Gerät/Hersteller/Modell
- Untersuchungsdatum/Uhrzeit
- Messwerte rechts/links
- PD-Werte
- Tonometrie
- Pachymetrie
- Keratometrie
- Einzelmessungen optional
- technische Zusatzwerte optional
- Hinweis auf Quelle/Gerätedatei optional

### 15.5 Layout und Templates

Die Dokumenterzeugung soll perspektivisch templatebasiert erfolgen.

Konfigurierbar sein sollen:

- Titel
- Abschnitte
- Tabellen
- Reihenfolge der Werte
- sichtbare Messwertgruppen
- Beschriftungen
- Einheiten
- optional Logo/Briefkopf
- optional Fußzeile
- Dateiname
- Ablagepfad

Die PDF-Templates sollen je Geräte-/Exportprofil unterschiedlich sein können.

### 15.6 Dokumentablage

Der Ablageort für erzeugte Dokumente muss frei konfigurierbar sein.

Erlaubt sein sollen:

- lokale Ordner
- Netzwerkpfade
- UNC-Pfade, z. B. `\\SERVER\Freigabe\XdtBridge\Dokumente`

Anforderungen:

- Ablagepfad pro Interface-/Exportprofil definierbar
- Schreibrechte müssen geprüft werden
- Ordner muss existieren oder optional erstellt werden können
- Dateinamen müssen eindeutig erzeugt werden
- vorhandene Dateien dürfen nicht unbeabsichtigt überschrieben werden
- Pfade müssen durch `FolderSafetyValidator` oder vergleichbare Logik geprüft werden
- keine Ablage in Systemordnern oder unsicheren Root-Pfaden

### 15.7 Ordnerstruktur

Die App soll eine konfigurierbare Ordnerstruktur für Dokumente unterstützen.

Beispiele:

- pro Jahr/Monat
- pro Praxis/Standort
- pro Gerät
- pro Patientennummer
- pro Untersuchungsdatum

Beispiel:

```text
\\SERVER\Medistar\EV\ARK1S\2026\05\4701-1\ARK1S_4701-1_20260502_150533.pdf
```

### 15.8 EV-Verknüpfung in MEDISTAR

Für MEDISTAR soll perspektivisch optional ein EV-Verweis erzeugt werden können, der auf das erzeugte PDF oder eine vom Gerät gelieferte Zusatzdatei verweist.

Anforderungen:

- EV-Verweis erzeugen: ja/nein
- EV-Texttemplate konfigurierbar
- EV-Kennung/Referenz muss erzeugt oder aus AIS-/Gerätekontext abgeleitet werden können
- EV-Zeile muss mit XDT-Ergebniszeilen kombinierbar sein
- Doppelklick in MEDISTAR soll später das abgelegte Dokument öffnen können
- genaue MEDISTAR-EV-Struktur muss anhand funktionierender Praxisbeispiele validiert werden

Beispiel:

```text
EV:{000000003B} NT-530P Messung
```

### 15.9 Zusammenspiel mit bestehenden Gerätedateien

Die App muss zwei Fälle unterscheiden können:

Fall A:

Das Gerät liefert selbst eine Zusatzdatei, z. B. JPG oder PDF. Die App ordnet diese Datei der Messung zu, kopiert sie in den Zielordner und erzeugt optional einen EV-Verweis.

Fall B:

Das Gerät liefert nur maschinenlesbare Werte. Die App erzeugt selbst ein PDF-Messprotokoll aus den gelesenen Werten und erzeugt optional einen EV-Verweis.

Beide Fälle sollen pro Profil konfigurierbar sein.

### 15.10 Verhalten bei Fehlern

Wenn die Dokumenterzeugung fehlschlägt:

- Fehler muss protokolliert werden
- Benutzer muss verständliche Meldung erhalten
- Verhalten muss konfigurierbar sein:
  - Verarbeitung abbrechen, wenn Dokument Pflicht ist
  - Verarbeitung fortsetzen, wenn Dokument optional ist

Wenn ein EV-Verweis erzeugt werden soll, aber das Dokument nicht geschrieben werden konnte:

- EV-Verweis darf nicht blind erzeugt werden
- Fehler muss sichtbar sein

### 15.11 Datenschutz und Sicherheit

Erzeugte Dokumente enthalten potenziell Patientendaten und medizinische Messwerte.

Anforderungen:

- Ablage nur in konfigurierten Ordnern
- keine automatische Ablage in unsicheren temporären Ordnern
- Zugriffsschutz liegt beim Praxis-/Windows-/Netzwerkrechtekonzept
- Pfade und Dateinamen dürfen keine unnötigen Patientendaten enthalten, sofern vermeidbar
- Protokollierung soll keine vollständigen medizinischen Inhalte unnötig duplizieren
- Dokumentexport muss im AVV/Datenschutzkonzept berücksichtigt werden

### 15.12 Abgrenzung Version 1

Für die aktuelle Version wird noch keine PDF-Erzeugung und kein produktiver EV-Verweis implementiert.

Der aktuelle Prototyp erzeugt:

- MEDISTAR-kompatible XDT-Ergebniszeilen
- noch keine PDF-Protokolle
- noch keine EV-Dokumentverknüpfung

Die Dokumentenerzeugung wird als zukünftiger optionaler Baustein vorgesehen.
