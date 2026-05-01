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
