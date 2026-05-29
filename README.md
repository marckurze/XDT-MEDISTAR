# XdtDeviceBridge

XdtDeviceBridge ist ein lokaler Windows-Prototyp fuer die dateibasierte Anbindung von Untersuchungsgeraeten an ein Arztinformationssystem ueber GDT/XDT-Dateien. Der Produktname fuer den spaeteren Einsatz ist XDTBox; Kunden-App und grafisches Hersteller-Lizenztool verwenden das offizielle XDTBox-Icon sowie das neue Logo-/Schriftzug-Branding.

In der Kunden-App startet die periodische Überwachung beim Öffnen der App standardmaessig automatisch fuer aktive Schnittstellenprofile; dieses Verhalten ist ueber das Zahnrad in der Tab-Zeile konfigurierbar. Es gibt weiterhin keinen Windows-Dienst, keinen Windows-Autostart und keinen FileSystemWatcher. XDTBox bringt ausserdem einen Tab `Sicherung/Umzug` fuer lokale Konfigurationssicherungen im Format `.xdtboxbackup` sowie ein lokales Hilfe-/Info-Menue direkt neben den Tabs mit.

Der aktuelle Fokus liegt auf dem Workflow MEDISTAR mit einem NIDEK ARK1S / AR-1s Autorefraktometer.

Aktuelle Version:

`0.1.0-prototype`

Dies ist der erste stabile Prototyp. Er wurde mit MEDISTAR und NIDEK ARK1S erfolgreich validiert. Der Workflow MEDISTAR + NIDEK ARK1S + XDT-Anhang-Link wurde am 2026-05-11 praktisch geprüft; externe Anhänge können über `6302`, `6303`, optional `6304` und `6305` in MEDISTAR übernommen und aus der Karteikarte geöffnet werden. Die automatische Verarbeitung ist manuell startbar und bewusst nicht als Windows-Dienst oder Autostart umgesetzt.

Praktisch validiert ist aktuell nur der Workflow MEDISTAR + NIDEK ARK1S, einschließlich XDT-Anhang-Link für den geprüften Pflicht-Anhang-Praxislauf. Weitere V2-Geraeteprofile sind vorbereitet und koennen angezeigt bzw. konfiguriert werden, gelten aber noch nicht als produktiv validiert.

Die aktualisierte Roadmap mit Iststand, Sicherheitsentscheidungen und empfohlenen naechsten Entwicklungsphasen steht in [`docs/ROADMAP.md`](docs/ROADMAP.md).

Geräte-Dateianhang-Import und externe Link-Übergabe ans AIS sind für den validierten MEDISTAR/NIDEK-ARK1S-Pflicht-Anhang-Praxislauf praktisch bestätigt. Im aktuellen Prototyp ist der Konfigurationsbereich `XDT-Anhänge für AIS` im Schnittstellenprofil vorbereitet: optionale Import-/Exportordner, `AttachmentFileNameTemplate`, vorbereiteter `AttachmentTransferService` mit Copy/Move-Modus, `ExternalAisLinkFieldBuilder` für semantische Feldwerte, `ExternalAisLinkXdtFieldAdapter` für XDT-Feldcode/Wert-Paare, isolierter `AttachmentExternalLinkPreparationService` zur Orchestrierung auf explizite Eingabe und XDT-Linkfeld-Vorlagen für 6302, 6303, 6304 und 6305. Der Baukasten im Tab `Profile & Templates` kann eine explizit ausgewählte Anhangdatei für Vorschau und Testexport vorbereiten und zeigt Ziel-Dateiname, Zielpfad sowie vorbereitete XDT-Felder an. Standard für Geräte-Dateianhänge ist `Move`, damit der XDT-Anhang-Importordner nach erfolgreicher Übernahme sauber bleiben kann; Mehrfachanhang-Heuristiken, Dokument-/Dateianhang-Templates und weitere Geräte-/AIS-Validierungen sind weiterhin offen. XDT-Längenpräfixe werden nicht in der Konfiguration gepflegt, sondern zentral durch den Exportmechanismus erzeugt.

Ein isolierter `AttachmentImportFolderScannerService` ist vorbereitet. Er listet unterstützte XDT-Anhang-Dateitypen im konfigurierten XDT-Anhang Importordner auf und verändert keine Dateien. Vollständige automatische Zuordnung und Mehrfachanhang-Heuristiken bleiben offen.

Eine isolierte automatische Kandidatenauswahl ist ebenfalls vorbereitet: Automatisch eindeutig ist zunächst nur der Fall, dass genau eine unterstützte Anhangdatei im XDT-Anhang Importordner gefunden wurde. Bei mehreren unterstützten Dateien wird nicht automatisch ausgewählt, weil der Patientenbezug unsicher wäre.

Die konservative automatische XDT-Anhang-Vorbereitung ist vorbereitet: Sie greift nur während laufender Überwachung, aktivierter XDT-Anhang-Funktion im Schnittstellenprofil und genau einem unterstützten Anhangkandidaten. Bei erfolgreicher Vorbereitung werden die XDT-Feldcode/Wert-Paare 6302, 6303, optional 6304 und 6305 transient an die erzeugte XDT-Exportdatei angehängt. Bei deaktivierter Funktion, fehlender Eindeutigkeit, mehreren unterstützten Anhängen oder Fehlern bleibt der bestehende Export unverändert. Die XDT-Längenpräfixe werden weiterhin zentral durch den Exportmechanismus erzeugt.

Für vollständige Verarbeitungspakete ist ein zweistufiges Wartemodell vorbereitet: Zuerst wartet eine erkannte AIS-Datei auf eine stabile Gerätedatei. Die Wartezeit ist pro Schnittstellenprofil konfigurierbar, Standard `10` Minuten. Kommt vor der Gerätedatei eine neuere AIS-Datei, ersetzt sie den wartenden Auftrag. Erst wenn AIS- und Gerätedatei als stabiles Paar vorhanden sind, startet die XDT-Anhang-Wartezeit. XDT-Anhänge können pro Schnittstellenprofil `optional` oder als `Pflicht` erwartet werden. Standard ist `optional`, die Standard-Wartezeit beträgt 30 Sekunden. Optional bedeutet: Nach Timeout dürfen Messwerte ohne Anhang übertragen werden. Pflicht bedeutet: Ohne eindeutigen Anhang wird die Verarbeitung blockiert; bei Timeout wird das konkrete Paket terminal als Fehlerfall abgeschlossen, damit neue Untersuchungen weiterlaufen können. Dabei werden nur bekannte AIS-/Gerätedateien dieses Pakets gemäß Fehler-/Archivoptionen behandelt, unbekannte Dateien und Exportordner werden nicht pauschal bereinigt. Mehrere unterstützte Anhänge bleiben unsicher und werden nicht automatisch zugeordnet.

Für langsam schreibende Geräte ist zusätzlich eine Stabilitätsprüfung vorbereitet: XDT-Anhänge werden erst automatisch ausgewählt oder übertragen, wenn sie über die konfigurierte Stabilitätswartezeit unverändert und lesbar bleiben. Standard ist 2 Sekunden. Das periodische Ordnerabfrage-Intervall ist pro Schnittstellenprofil konfigurierbar; der Standard bleibt 5 Sekunden. Es wird weiterhin kein FileSystemWatcher verwendet.

Der Tab `Profile & Templates` enthält einen schrittweise organisierten Bereich `Test & Vorschau` für den bisherigen Baukasten-Test: AIS-Testdatei laden, Gerätedatei laden, optional XDT-Anhang einlesen, Messwerte prüfen, Gesamtexport-Vorschau kontrollieren und einen Testexport erstellen. Das verwendete Schnittstellenprofil wird dort mit AIS-, Geräte-, Exportprofil- und XDT-Anhang-Konfiguration angezeigt. Der Button `XDT-Anhang einlesen` öffnet eine Dateiauswahl, sodass ein XDT-Anhang aus beliebigem Speicherort für die externe Template-Vorbereitung gewählt werden kann. Sobald AIS- und Gerätedatei geladen sind, aktualisiert der Baukasten nach dem Einlesen automatisch die Gesamtexport-Vorschau mit den transienten Linkfeldern 6302 bis 6305. Vorschau und Testexport simulieren trotzdem den im Schnittstellenprofil definierten XDT-Anhang-Exportpfad: 6305 zeigt auf `XDT-Anhang Exportordner` plus erzeugten Dateinamen, nicht auf den Quellpfad. `Testexport erstellen` öffnet eine Ordnerauswahl und schreibt Test-XDT-Datei plus korrekt umbenannten Anhang physisch in den gewählten Testordner; der 6305-Wert in der Test-XDT-Datei bleibt auf den simulierten Schnittstellenprofil-Zielpfad ausgerichtet, und der produktive Schnittstellenprofil-Exportordner wird im Baukasten-Test nicht beschrieben. Exportprofile und BuiltIn-Profile werden dadurch nicht verändert. Der Tab `Profile & Templates` ist zur besseren Uebersicht in einklappbare, standardmaessig ausgeklappte Bereiche gegliedert; der Tab `Verarbeitung` bleibt der Betriebsbereich fuer aktive Schnittstellenprofile, Ueberwachung und automatische Verarbeitung.

Neu ist der zusätzliche Tab `XDT-Baukasten`. Er ist als eigenständige Arbeitsoberfläche für Entwürfe und Testdaten aufgebaut und soll den bisherigen Sammelbereich später ersetzen. Der neue Baukasten hält AIS-, Geräte- und Exportprofil, Rohdaten, optionalen Testanhang, Exportregel-Arbeitskopie und Vorschauzustand getrennt von produktiven Schnittstellenprofilen. `Verarbeitung starten` erzeugt dort ausschließlich eine Vorschau im Speicher; es wird keine produktive Datei geschrieben, nichts archiviert und keine Überwachung gestartet. Die Ergebnisbox bietet `Roh-XDT`, `Ansicht im AIS`, `Geräteausgabe` und `Diagnose`; `Ansicht im AIS` zeigt nur fachlich sichtbare Karteikartenzeilen ohne Patientendatenblock, Feldnummern oder Parserdiagnose. Alle Ausgabeansichten zeigen links Baukasten-Zeilennummern, die nicht Bestandteil des echten Rohtexts sind. Der Exportregelbereich besitzt nun die Richtungen `Export an AIS` und bei bidirektionalen Geräten `Export an Gerät`, eine eigene Nummernspalte und eine Regel-Ausgabe-Markierung: Wird eine Regel ausgewählt, hebt der Baukasten die zugehörige Ausgabezeile hervor oder meldet, dass die Regel aktuell keine Ausgabe erzeugt. CV-5000/CV-5000S und NIDEK RT-6100 zeigen eigene Geräteausgabe-Regeln, erweiterte Ausgabe-an-Gerät-Platzhalter und eine Vorschau der XML-Ausgabe, ohne produktive Geräteordner zu beschreiben. Änderungen an unterstützten Geräteausgabe-Regeln wirken direkt in der `Geräteausgabe`-Vorschau. UserDefined-bidirektionale Geräte bleiben offen und können eigene Geräteausgabe-Regeln anlegen. Exportregeln können in der Arbeitskopie ergänzt/entfernt werden, feste Notizen funktionieren ohne `SourcePath`, Platzhalter zeigen rechts aktuelle Beispielwerte aus der geladenen Testdatei und der Undo-Pfeil nimmt Baukasten-Änderungen zurück. Die Gerätekompatibilität nutzt eine zentrale Baukastenprüfung: passende BuiltIns und Modell-Aliase wie CV-5000/CV-5000S werden erkannt, ModelName-/Profilabweichungen blockieren die Vorschau im Baukasten aber nicht, solange Datei, Parser und Mapping verwertbare Daten liefern. Abweichungen erscheinen als Warnung und in der Diagnose; die produktive Verarbeitung prüft weiterhin strenger. `Templatepaket laden` ist in V1 sichtbar als vorbereitete lokale Paketbibliothek gekennzeichnet.

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
- Vorbereitete V2-Geraeteprofile fuer NIDEK LM7/LM7P, NIDEK NT530P, NIDEK RT-6100, NIDEK RT-2100/RT-3100/RT-5100 RS232, TOPCON CL300, TOPCON KR800 und TOPCON TRK2P.
- Bidirektionaler Phoropter-Kandidat NIDEK RT-6100: XDTBox kann aus MEDISTAR-Historienwerten konservativ LM_Base/REF_Base-XML fuer den MEM-200-/DIRECT_RT_xx/TXT-Ordner erzeugen und RT-6100-Rueckgabe-XML mit `Best -> 6228` sowie `Full -> 6227` auswerten. Die praktische RT-6100-/MEDISTAR-Abnahme steht noch aus.
- Serielle bidirektionale Phoropter-Kandidaten NIDEK RT-2100 / RT-3100 / RT-5100: XDTBox kennt PDF-basierte RS232-Presets, kann synthetische dokumentnahe RT->PC-Rohdaten sowie echte RT-3100-Praxismitschnitte parsen, `Final -> 6228` und `Subjective -> 6227` abbilden und den produktiven Ablauf patientengetriggert fuehren: AIS-Datei kommt an, Auswahlfenster oeffnet, der Anwender sendet LM-/AR-Historienwerte bewusst ueber COM-Port, danach wartet XDTBox bis `EOT` plus Stabilitaetswartezeit auf die RT-Rueckgabe. Der RT-3100-Livebefund bestaetigt, dass Type1 in der Praxis mit DTR aktiv empfaengt und dass der Phoropter den direkten PC->RT-Writer-Frame akzeptiert, waehrend der RS/SD-Handshake keine SD-Antwort lieferte. Der Sendemodus ist deshalb im Schnittstellenprofil speicherbar; die RT-BuiltIns nutzen `Direkt Writer-Frame senden` als Default, `RS/SD-Handshake` und `RS senden, dann Writer ohne SD` bleiben auswaehlbar. Die RT-Type1-/Type2-Presets setzen DTR/RTS standardmaessig aktiv. Die RS-Anforderung sendet Handbuch-`*` als Leerzeichen (`01 43 20 20 20 02 52 53 17 04`) und nicht als ASCII-Sternchen; auch die LM-SCA-Augenpraefixe werden als Leerzeichen + `R`/`L` geschrieben. Fuer die Praxisabnahme zeigt das RT-Fenster eine einklappbare serielle Diagnose mit COM-Parametern, DTR/RTS/Handshake, CTS/DSR/DCD/RI, RS-/SD-/Writer-Hexdump, sichtbaren Steuerzeichen und Timeout-Checkliste. `COM-Port nur abhoeren` prueft die Profilparameter ohne Sendung und ohne Export; `Rueckgabe abhoeren und verarbeiten` nutzt dagegen den wartenden Patientenkontext und erzeugt nach gueltiger Rueckgabe produktiv die MEDISTAR-XDT-Datei. Die Sendetestmodi `RS anfordern`, `DTR-Toggle + RS`, `Direkt Writer-Frame senden` und `RS + Writer ohne SD-Warten` sind nur nach explizitem Klick verfuegbar und aendern den gespeicherten Sendemodus nicht. Nach erfolgreichem Senden wartet XDTBox auf die spaetere Rueckgabe nach Untersuchung/PRINT/SEND und erzeugt ohne Rueckgabe kein leeres XDT.
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

Der Automatik-Prototyp bereitet die spaetere produktive Ordnerverarbeitung vor und startet in der geoeffneten App standardmaessig automatisch; er kann weiter manuell gestoppt und gestartet werden. Der manuelle Baukasten-Test befindet sich im Tab `Profile & Templates`.

### 1. Baukasten-Test

Die App unterstuetzt weiterhin einen manuellen Testmodus im Tab `Profile & Templates / Test & Vorschau`:

- AIS-GDT/XDT-Datei auswaehlen.
- Geraetedatei auswaehlen; die Vorschau verwendet das ausgewaehlte Exportprofil und das dazu passende Geraeteprofil. Dadurch koennen neben ARK1S auch profilierte XML-Workflows wie RT-6100 im Baukasten gegen ihre eigenen Mappingregeln geprueft werden.
- Exportvorschau aktualisieren.
- Exportvorschau anzeigen.
- Test-XDT-Datei und optionalen Testanhang in einen frei gewählten Testordner schreiben.

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
- Lizenzpflicht-Haken
- Archivierungsoptionen
- Fehlerablageoptionen

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
