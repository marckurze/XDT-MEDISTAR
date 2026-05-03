# Architektur Version 2

Dieses Dokument beschreibt die geplante Zielarchitektur für XdtDeviceBridge Version 2. Der aktuelle MEDISTAR/NIDEK-ARK1S-Prototyp bleibt dabei funktionsfähig und bildet das erste validierte Standardprofil. Die neue Architektur soll schrittweise eingeführt werden, ohne den bestehenden Ablauf unnötig zu destabilisieren.

---

## 1. Aktueller Stand des Prototyps

Der aktuelle Prototyp ist eine lokale WPF-Anwendung für Windows. Er verarbeitet Dateien manuell über die Oberfläche:

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

Die zentrale Logik liegt derzeit im Core-Projekt:

- `GdtParser`
- `PatientDataMapper`
- `XmlDeviceParser`
- `MappingEngine`
- `XdtExportBuilder`
- `DefaultDeviceProfiles`
- `ProcessingPipelineService`

Diese Struktur ist für den Prototyp ausreichend, aber noch nicht flexibel genug für mehrere AIS-Systeme, Geräteklassen, Exportprofile, Template-Bibliotheken und Lizenzierung.

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
- optional Archivierung oder Bereinigung nach erfolgreicher Verarbeitung

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

Bei ungültiger oder abgelaufener Lizenz darf keine produktive Verarbeitung erfolgen. Konfiguration, Import/Export und Lizenzaktivierung müssen weiterhin möglich bleiben.

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

## 8. Abgrenzung und Einführungsstrategie

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

Die Architektur soll so vorbereitet werden, dass diese Erweiterungen später möglich sind, ohne den validierten Prototypen neu schreiben zu müssen.
