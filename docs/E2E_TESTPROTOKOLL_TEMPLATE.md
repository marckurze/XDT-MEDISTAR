# E2E-Testprotokoll – XdtDeviceBridge

## 1. Allgemeine Angaben

- Datum:
- Tester:
- App-Version:
- Git-Commit:
- Windows-Version:
- Testgerät / Rechner:
- AIS-Profil:
- Geräteprofil:
- Exportprofil:
- Schnittstellenprofil:

## 2. Testordner

- AIS-Importordner:
- Geräte-Importordner:
- Exportordner ans AIS:
- Archivordner:
- Fehlerordner:
- XDT-Anhang Importordner:
- XDT-Anhang Exportordner:

## 3. Schnittstellenprofil-Konfiguration

- Automatische Verarbeitung aktiv: Ja/Nein
- Überwachung manuell gestartet: Ja/Nein
- Archivierung aktiv: Ja/Nein
- Archivmodus: Copy/Move
- Wartezeit auf Gerätedatei:
- XDT-Anhänge für AIS automatisch verarbeiten: Ja/Nein
- XDT-Anhang ist: Optional/Pflicht
- Wartezeit auf XDT-Anhang:
- Dateistabilität abwarten:
- Ordnerabfrage-Intervall:
- XDT-Anhang Dateiname:
- 6302 Dokumentenname:
- 6303 Dateiformat:
- 6304 Beschreibung:
- 6305 vollständiger Dateipfad/Pfadtemplate:

## 4. Testdateien

- AIS-Datei:
- Gerätedatei:
- XDT-Anhang-Datei:
- erwartete Patientennummer:
- erwarteter Ziel-Dateiname des Anhangs:
- erwarteter 6305-Zielpfad:

## 5. Testfall-Protokolle

| Testfall-Nr. | Testfall-Name | durchgeführt: Ja/Nein | bestanden: Ja/Nein | Exportdatei erzeugt: Ja/Nein | Exportdatei-Pfad | Anhang übertragen: Ja/Nein | Anhang-Zielpfad | Archivierung korrekt: Ja/Nein | Fehlerordner korrekt: Ja/Nein | Statusmeldung | Auffälligkeiten | Screenshot/Notiz |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Normale Verarbeitung ohne XDT-Anhang-Funktion |  |  |  |  |  |  |  |  |  |  |  |
| 2 | Optionaler XDT-Anhang ist sofort vorhanden |  |  |  |  |  |  |  |  |  |  |  |
| 3 | Optionaler XDT-Anhang kommt verspätet, aber innerhalb Timeout |  |  |  |  |  |  |  |  |  |  |  |
| 4 | Optionaler XDT-Anhang kommt nicht |  |  |  |  |  |  |  |  |  |  |  |
| 5 | Pflicht-XDT-Anhang kommt nicht |  |  |  |  |  |  |  |  |  |  |  |
| 6 | Mehrere unterstützte XDT-Anhänge |  |  |  |  |  |  |  |  |  |  |  |
| 7 | Instabile XDT-Anhangdatei |  |  |  |  |  |  |  |  |  |  |  |
| 8 | AIS-Datei wartet auf Gerätedatei |  |  |  |  |  |  |  |  |  |  |  |
| 9 | Gerätedatei kommt später |  |  |  |  |  |  |  |  |  |  |  |
| 10 | Gerätedatei kommt nicht innerhalb Timeout |  |  |  |  |  |  |  |  |  |  |  |
| 11 | Neue AIS-Datei ersetzt alte AIS-Datei |  |  |  |  |  |  |  |  |  |  |  |
| 12 | Nicht unterstützte Datei im XDT-Anhang Importordner |  |  |  |  |  |  |  |  |  |  |  |

## 6. XDT-Prüfung

- [ ] 8000 = 6310 vorhanden
- [ ] 3000 Patientennummer korrekt
- [ ] 3101 Nachname korrekt
- [ ] 3102 Vorname korrekt
- [ ] 3103 Geburtsdatum korrekt
- [ ] 8402 Untersuchungsart korrekt
- [ ] 6228 Ergebniszeilen vorhanden
- [ ] 6302 vorhanden, falls Anhang erwartet
- [ ] 6303 vorhanden, falls Anhang erwartet
- [ ] 6304 vorhanden, falls Beschreibung gesetzt
- [ ] 6305 vorhanden, falls Anhang erwartet
- [ ] 6305 zeigt auf erwarteten Zielpfad
- [ ] XDT-Längenpräfixe plausibel/konsistent
- [ ] keine 6302–6305 bei deaktiviertem/übersprungenem Anhang

## 7. Archiv-/Fehlerprüfung

- [ ] AIS-Datei archiviert
- [ ] Gerätedatei archiviert
- [ ] Anhang korrekt übertragen
- [ ] keine unbekannten Dateien verschoben
- [ ] Fehlerfall erzeugt error.txt
- [ ] Originaldateien im Fehlerfall korrekt erhalten, soweit vorgesehen
- [ ] Exportordner wurde nicht pauschal bereinigt

## 8. Gesamtergebnis

- Gesamtabnahme bestanden: Ja/Nein
- Blockierende Fehler:
- Nicht blockierende Auffälligkeiten:
- Empfohlene nächste Schritte:
