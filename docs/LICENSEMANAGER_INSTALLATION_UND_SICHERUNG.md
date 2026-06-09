# XDTBox Lizenzmanager: Installation und Sicherung

Stand: 2026-06-08

Der `XDTBox Lizenzmanager` ist das interne Herstellerwerkzeug fuer Lizenzanfragen, signierte `.xdtboxlic`-Dateien, Kundenverwaltung und nachvollziehbare Lizenzhistorie. Der Projektname bleibt technisch `XdtBox.LicenseManager`; sichtbarer Produktname, Fenster, Setup und PDF-Ausgaben verwenden `XDTBox Lizenzmanager`.

## Version und Lizenzlaufzeit

- Produktname: `XDTBox Lizenzmanager`
- Version: `1.0`
- FileVersion: `1.0.0.0`
- neue XDTBox-Lizenzen werden standardmaessig unbefristet erzeugt
- `Gueltig bis` wird fuer diese Lizenzen als `unbefristet` angezeigt
- eine Lizenz wird nicht durch Ablaufdatum ungueltig, sondern durch falsche InstallationId/Hardware, ungueltige Signatur oder bewusst entfernte lokale Kundendatei auf Kundenseite
- es gibt keine Abo-, Monats- oder Onlinepruefung

## Kunden, Installationen und Zahlungsart

Ein Kunde kann mehrere Installationen beziehungsweise Arbeitsplaetze besitzen. Die Kundenliste summiert aktive Installationen, aktive lizenzpflichtige Geraete und Nettokosten ueber alle nicht stornierten Installationen.

Die Rechnungs-E-Mail ist von der Zahlungsart getrennt. Jeder Kunde kann eine Rechnungs-E-Mail fuehren; die Zahlungsart beschreibt nur den Zahlungskanal:

- `SEPA-Lastschrift`
- `Bankueberweisung`

SEPA benoetigt mindestens plausible IBAN- und Kontoinhaber-Daten. Bankueberweisung benoetigt keine Bankdaten. Alte Kein-SEPA-Daten werden als Bankueberweisung behandelt.

## Lizenzierte Geraete

Der Lizenzmanager uebernimmt nur aktive lizenzpflichtige Geraeteanbindungen aus der Lizenzanfrage beziehungsweise der gespeicherten Installationshistorie. Nicht aktive Profile und nicht lizenzpflichtige Eintraege werden nicht in aktuelle Kunden-/Lizenzlisten uebernommen. Die technische Deckung bleibt weiterhin an Anzahl, InstallationId und Signatur gebunden; die Geraeteliste dient Herstellerverwaltung, Abrechnung und Nachvollziehbarkeit.

Alte Lizenzanfragen ohne detaillierte Geraeteliste bleiben importierbar. In diesem Fall nutzt der Lizenzmanager die angefragte Anzahl aktiver Lizenzplaetze, zeigt aber keinen erfundenen Geraetelisteneintrag an.

## Historie und Detailpflege

Der Tab `Ausgestellte Lizenzen` zeigt oben eine reduzierte Historienliste mit Kundennummer, Kunde, Ort, Geraete und Ausstellungsdatum. Die Detaildaten stehen darunter strukturiert als Kunden-/Zahlungsdaten, Lizenzdaten und Geraeteliste. Einzelne Eintraege koennen aus der lokalen Lizenzmanager-Historie entfernt werden. Dabei werden keine `.xdtboxlic`-Dateien, keine privaten Schluessel und keine Kundenstammdaten geloescht.

Die Kundenliste zeigt neben Einzelkosten auch die Gesamtsumme monatlicher Lizenzen ueber alle aktiven lizenzpflichtigen Geraeteanbindungen. Kundendetails ordnen Installationen, aktive lizenzierte Anbindungen und Lizenzhistorie vertikal, damit kleine Fenster keine Daten ueberlagern.

## Lokales Storno

Eine Installation kann in der Kundendetailansicht lokal als storniert markiert werden. Das reduziert aktive Installationen, aktive Geraete und Kosten in der Herstellerverwaltung, loescht aber keine Kundenstammdaten und macht eine bereits beim Kunden installierte Offline-Lizenzdatei nicht automatisch ungueltig. Der Storno bleibt mit Datum und InstallationId historisch nachvollziehbar.

## PDF-Kundenuebersicht

Der PDF-Export erzeugt eine A4-Querformat-Uebersicht mit grafischer Tabelle:

- Kopfbereich mit Titel, Erstellungsdatum und Einzelpreis netto
- Spalten fuer Kundennummer, Praxis/Firma, Rechnungs-E-Mail, Zahlungsart, IBAN, BIC, Kontoinhaber, Installationen, Geraete, Netto und Gueltigkeit
- Summenzeile fuer Gesamtanzahl Geraete und Gesamtkosten netto
- keine Pipe-Texttabelle
- keine Private-Key- oder Signaturdaten

## Setup

Das eigene Setup wird mit folgendem Skript gebaut:

```powershell
.\scripts\build-xdtbox-licensemanager-installer.ps1
```

Das Skript publisht zuerst in einen Staging-Ordner, validiert die Publish-Ausgabe und ersetzt finale Artefakte erst nach erfolgreichem Publish, sauberer Validierung und erfolgreichem Inno-Build.

Erwartete Artefakte:

- Publish: `artifacts\publish\XDTBox Lizenzmanager`
- Setup-Ordner: `artifacts\Lizenzmanager Setup`
- Setup-Datei: `XDTBox_Lizenzmanager_Setup_1.10.exe`
- Zielordner auf Hersteller-PCs: `C:\XDTBox\Lizenzaktivierung`

Das Setup enthaelt:

- `XdtBox.LicenseManager.exe`
- notwendige DLLs und Assets
- Startmenue-Verknuepfung
- optional Desktop-Verknuepfung
- Deinstaller

Das Setup enthaelt bewusst nicht:

- XDTBox Kunden-App
- `XdtDeviceBridge.App.exe`
- private PEM-Schluessel
- Signaturzertifikate oder Master-Keys
- vorhandene Kunden-/History-/Settings-Dateien
- LicenseManager-Backups
- lokale Kundendaten

## Sicherung und Wiederherstellung

Die LicenseManager-Sicherung enthaelt lokale Herstellerverwaltungsdaten:

- Kundenliste
- Installationen und Storno-Status
- Bank-/SEPA-/Rechnungsdaten
- Netto-Einzelpreis
- Einstellungen
- Lizenzhistorie

Bewusst nicht enthalten:

- private Schluessel
- Signaturzertifikate
- Master-Keys
- Kunden-App
- externe Praxisordner

Damit kann der Lizenzmanager auf einem anderen Hersteller-PC installiert und anschliessend ueber eine Sicherung wieder mit Kunden, Preisen, Zahlungseinstellungen und Historie befuellt werden.
