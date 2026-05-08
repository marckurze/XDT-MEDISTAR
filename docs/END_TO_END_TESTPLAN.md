# End-to-End-Testplan: automatische AIS-/Geräte-/XDT-Anhang-Verarbeitung

## 1. Ziel des Testplans

Dieser Testplan beschreibt reproduzierbare manuelle und automatisierte Prüfungen für die zweistufige automatische Verarbeitung in XdtDeviceBridge.

Die fachliche Kernregel lautet:

1. Phase 1: Eine stabile AIS-Datei wartet auf eine stabile Gerätedatei.
2. Phase 2: Erst nach erkanntem vollständigem AIS-/Geräte-Dateipaar beginnt die Wartezeit auf einen optionalen oder verpflichtenden XDT-Anhang.

Der Plan prüft insbesondere:

- Verarbeitung ohne XDT-Anhang-Funktion bleibt unverändert.
- Optionale XDT-Anhänge werden nur bei eindeutiger Zuordnung übernommen.
- Verpflichtende XDT-Anhänge blockieren bei fehlender oder uneindeutiger Zuordnung.
- Instabile Dateien werden nicht verarbeitet, verschoben oder verlinkt.
- Neuere AIS-Dateien ersetzen ältere wartende AIS-Aufträge.
- Es gibt weiterhin keinen FileSystemWatcher, keinen Windows-Dienst, keinen Autostart und keine Verarbeitung beim App-Start.

## 2. Voraussetzungen

- Build der Lösung ist erfolgreich.
- Testsuite ist erfolgreich.
- Anwendung ist lokal startbar.
- Schnittstellenprofil für MEDISTAR + NIDEK ARK1S ist konfiguriert.
- Manuell startbare Überwachung wird im Tab `Verarbeitung` gestartet.
- Automatische Verarbeitung wird nur über den Haken `Gefundene Dateipaare automatisch verarbeiten` aktiviert.
- Testdateien enthalten keine echten Patientendaten.
- Alle Testordner sind Arbeitsordner, keine Root-, System- oder Benutzerprofil-Wurzeln.

## 3. Testordner

Empfohlene reproduzierbare Ordnerstruktur:

| Zweck | Beispielpfad |
|---|---|
| AIS-Importordner | `C:\GitHub\AIS Import` |
| Geräte-Importordner | `C:\GitHub\GA Import` |
| Exportordner ans AIS | `C:\GitHub\Export` |
| Archivordner | `C:\GitHub\Archiv` |
| Fehlerordner | `C:\GitHub\Fehler` |
| XDT-Anhang Importordner | `C:\GitHub\AnhangImp` |
| XDT-Anhang Exportordner | `C:\GitHub\AnhangExp` |

Vor jedem manuellen Testfall:

1. Anwendung schließen oder Überwachung stoppen.
2. Testordner kontrolliert leeren oder neue eindeutige Unterordner verwenden.
3. Keine produktiven Patienten- oder Geräteordner verwenden.
4. Sicherstellen, dass keine alte Exportdatei oder alter Anhang das Ergebnis verfälscht.

## 4. Testdateien

Benötigt werden synthetische oder anonymisierte Testdateien:

- AIS-GDT/XDT-Datei mit Patientennummer in Feld `3000`.
- NIDEK ARK1S XML-Gerätedatei.
- Unterstützte XDT-Anhänge:
  - `.pdf`
  - `.jpg`
  - `.jpeg`
  - `.png`
  - `.tif`
  - `.tiff`
  - `.dcm`
  - `.txt`
- Nicht unterstützte Datei, z. B. `.docx` oder `.tmp`.

Für MEDISTAR/NIDEK ARK1S sollten die bekannten validierten Testdateien bzw. synthetische Kopien davon verwendet werden.

## 5. Profilkonfiguration

Standardkonfiguration für die manuellen Testfälle:

| Einstellung | Wert |
|---|---|
| Schnittstellenprofil aktiv | ja |
| globale automatische Verarbeitung | ja |
| Überwachung | manuell starten |
| Archivierung | aktiv |
| Archivmodus | `Move` / `Verschieben` |
| XDT-Anhänge für AIS automatisch verarbeiten | je Testfall ja/nein |
| XDT-Anhang ist | optional oder Pflicht |
| Wartezeit auf Gerätedatei | 10 Minuten |
| Wartezeit auf XDT-Anhang | 30 Sekunden |
| Dateistabilität abwarten | 2 Sekunden |
| Ordnerabfrage-Intervall | 5 Sekunden |
| XDT-Anhang Dateiname | `{Ais.PatientNumber}_{Date:ddMMyyyy}_{Time:HHmmss}{ExtensionUpper}` |
| 6302 Dokumentenname | `Datei` |
| 6303 Dateiformat | `{ExtensionUpperWithoutDot}` |
| 6304 Beschreibung | `Test M.Kurze` |
| 6305 vollständiger Dateipfad | `{Attachment.TargetFullPath}` |

## 6. Testfälle

### Testfall 1: Normale Verarbeitung ohne XDT-Anhang-Funktion

Ablauf:

1. XDT-Anhang-Automatik deaktivieren.
2. AIS-Datei in AIS-Importordner legen.
3. Gerätedatei in Geräte-Importordner legen.
4. Überwachung starten.
5. Automatische Verarbeitung aktivieren.

Erwartung:

- Exportdatei wird erzeugt.
- Export enthält keine Felder `6302`, `6303`, `6304`, `6305`.
- AIS- und Gerätedatei werden gemäß Archivoption archiviert.
- Bestehende MEDISTAR/NIDEK-ARK1S-Verarbeitung bleibt unverändert.

### Testfall 2: Optionaler XDT-Anhang ist sofort vorhanden

Ablauf:

1. XDT-Anhang-Automatik aktivieren.
2. XDT-Anhang ist `optional`.
3. AIS-Datei in AIS-Importordner legen.
4. Gerätedatei in Geräte-Importordner legen.
5. Genau eine unterstützte PDF in XDT-Anhang Importordner legen.

Erwartung:

- Exportdatei wird erzeugt.
- PDF wird nach XDT-Anhang Exportordner verschoben.
- Export enthält `6302`, `6303`, `6304`, `6305`.
- `6305` zeigt auf den finalen Zielpfad.
- XDT-Längenpräfixe sind korrekt.
- `6304` ist enthalten, wenn Beschreibung gesetzt ist.

### Testfall 3: Optionaler XDT-Anhang kommt verspätet, aber innerhalb Timeout

Ablauf:

1. AIS-Datei zuerst ablegen.
2. Gerätedatei später ablegen.
3. Nach vollständigem Paar PDF erst einige Sekunden später ablegen.
4. PDF kommt innerhalb `AttachmentWaitTimeoutSeconds`.

Erwartung:

- Kein Export ohne Anhang vor Timeout.
- Export wartet auf Anhang.
- Export enthält `6302`, `6303`, `6304`, `6305`.
- Anhang wird verschoben.
- XDT-Anhang-Wartezeit startet erst nach vollständigem AIS-/Geräte-Paar.

### Testfall 4: Optionaler XDT-Anhang kommt nicht

Ablauf:

1. XDT-Anhang ist `optional`.
2. AIS-Datei und Gerätedatei ablegen.
3. Keinen Anhang ablegen.
4. Timeout abwarten.

Erwartung:

- Exportdatei wird nach Timeout erzeugt.
- Export enthält keine `6302`, `6303`, `6304`, `6305`.
- Statusmeldung sinngemäß: `XDT-Anhang optional: Timeout erreicht, Export ohne Anhang`.

### Testfall 5: Pflicht-XDT-Anhang kommt nicht

Ablauf:

1. XDT-Anhang ist `Pflicht`.
2. AIS-Datei und Gerätedatei ablegen.
3. Keinen Anhang ablegen.
4. Timeout abwarten.

Erwartung:

- Keine Exportdatei wird erzeugt.
- Statusmeldung sinngemäß: `XDT-Anhang Pflicht: Timeout erreicht, Verarbeitung blockiert`.
- AIS-/Gerätedateien werden nicht fälschlich als erfolgreich verarbeitet archiviert.

### Testfall 6: Mehrere unterstützte XDT-Anhänge

Ablauf:

1. AIS-Datei und Gerätedatei ablegen.
2. Zwei PDFs oder PDF + JPG in XDT-Anhang Importordner legen.

Erwartung optional:

- Export ohne Anhang oder Warnstatus gemäß aktueller Spezifikation.
- Keine Datei wird automatisch ausgewählt.
- Keine Anhangdatei wird verschoben.
- Keine `6302` bis `6305`, wenn keine eindeutige Zuordnung möglich ist.

Erwartung Pflicht:

- Keine Exportdatei.
- Fehler/Blockade wegen mehrerer unterstützter Anhänge.
- Keine Anhangdatei wird verschoben.

### Testfall 7: Instabile XDT-Anhangdatei

Ablauf:

1. PDF wird in den XDT-Anhang Importordner geschrieben.
2. Datei während der Stabilitätswartezeit verändern oder geöffnet/gesperrt halten.
3. App scannt während Datei noch instabil ist.

Erwartung:

- Datei wird nicht verschoben.
- Datei wird nicht verlinkt.
- Erst nach stabiler Datei darf Verarbeitung erfolgen.
- Keine beschädigte PDF im Exportordner.

### Testfall 8: AIS-Datei wartet auf Gerätedatei

Ablauf:

1. AIS-Datei ablegen.
2. Keine Gerätedatei ablegen.
3. Wartezeit auf Gerätedatei noch nicht überschreiten.

Erwartung:

- Keine Exportdatei.
- Status sinngemäß: `Warte auf Gerätedatei.`
- AIS-Datei wird nicht als erfolgreich verarbeitet archiviert.

### Testfall 9: Gerätedatei kommt später

Ablauf:

1. AIS-Datei ablegen.
2. Mehrere Minuten warten.
3. Gerätedatei innerhalb `DeviceFileWaitTimeoutMinutes` ablegen.

Erwartung:

- Paar wird verarbeitet.
- Exportdatei entsteht.
- Bei aktivem optionalem/pflichtigem Anhang startet die Anhang-Wartezeit erst nach Gerätedatei.

### Testfall 10: Gerätedatei kommt nicht innerhalb Timeout

Ablauf:

1. AIS-Datei ablegen.
2. Keine Gerätedatei bis `DeviceFileWaitTimeoutMinutes`.

Erwartung:

- Kein Export.
- Status sinngemäß: `AIS-Datei abgelaufen: keine Gerätedatei innerhalb der Wartezeit gefunden.`
- Keine unbekannten Dateien werden gelöscht.

### Testfall 11: Neue AIS-Datei ersetzt alte AIS-Datei

Ablauf:

1. AIS-Datei A ablegen.
2. Keine Gerätedatei ablegen.
3. AIS-Datei B ablegen.
4. Danach Gerätedatei ablegen.

Erwartung:

- AIS-Datei B ist maßgeblich.
- AIS-Datei A wird ersetzt/abgelaufen markiert.
- Export verwendet Patientendaten aus B.
- A wird nicht verarbeitet.
- Keine unbekannte Datei wird gelöscht.

### Testfall 12: Nicht unterstützte Datei im XDT-Anhang Importordner

Ablauf:

1. `.docx` oder `.tmp` in XDT-Anhang Importordner legen.
2. AIS- und Gerätedatei korrekt ablegen.

Erwartung:

- Nicht unterstützte Datei wird nicht verarbeitet.
- Nicht unterstützte Datei wird nicht verschoben.
- Optionaler Anhang: Export ohne Anhang nach Timeout.
- Pflicht-Anhang: Blockade, weil kein unterstützter Anhang vorhanden ist.

## 7. Erwartete Ergebnisse

Allgemeine erwartete Ergebnisse über alle Testfälle:

- Nur stabile AIS-, Geräte- und XDT-Anhangdateien werden verarbeitet.
- Kein automatischer Start beim App-Start.
- Keine automatische Verarbeitung ohne manuell gestartete Überwachung.
- Keine unbekannten Dateien werden gelöscht, verschoben oder verändert.
- Keine Mehrfachanhang-Heuristik anhand Dateiname oder Zeitstempel.
- Mehrere unterstützte Anhänge werden nicht automatisch zugeordnet.
- Exportordner wird nicht pauschal bereinigt.

## 8. Prüfpunkte in der Exportdatei

Zu prüfen:

- Enthält `8000 = 6310`.
- Enthält Patientennummer `3000`.
- Enthält `8402` Untersuchungsart.
- Enthält `6228` Ergebniszeilen.
- Bei erfolgreichem Anhang:
  - enthält `6302`
  - enthält `6303`
  - enthält optional `6304`, wenn Beschreibung gesetzt ist
  - enthält `6305`
- Bei deaktiviertem, fehlendem, instabilem oder übersprungenem Anhang:
  - keine `6302`
  - keine `6303`
  - keine `6304`
  - keine `6305`
- XDT-Längenpräfixe sind korrekt.
- Backslashes im Pfad bleiben erhalten.
- `6305` verweist auf die tatsächlich im XDT-Anhang Exportordner abgelegte Datei.

## 9. Prüfpunkte im Archiv

Zu prüfen:

- Bei erfolgreicher Verarbeitung und Archivmodus `Move`:
  - AIS-Datei liegt im Archiv.
  - Gerätedatei liegt im Archiv.
  - Originale liegen nicht mehr im Importordner.
- Bei blockierter Pflicht-Anhang-Verarbeitung:
  - AIS-/Gerätedatei werden nicht als erfolgreich verarbeitet archiviert.
- Bei abgelaufener AIS-Datei:
  - kein Export wurde erzeugt.
  - keine unbekannten Dateien wurden gelöscht.
  - falls später eine sichere Ablage für abgelaufene Dateien aktiv wird, muss sie nachvollziehbar dokumentiert sein.

## 10. Prüfpunkte im XDT-Anhang Exportordner

Zu prüfen:

- Bei erfolgreichem Anhang:
  - Anhangdatei wurde mit Template eindeutig benannt.
  - Dateiname entspricht z. B. `11253_07052026_221723.PDF`.
  - vorhandene Zieldateien werden nicht überschrieben.
  - Suffixe wie `_001`, `_002` werden bei Kollisionen verwendet.
- Bei optionalem Timeout ohne Anhang:
  - keine neue Anhang-Zieldatei.
- Bei Pflicht-Timeout:
  - keine neue Anhang-Zieldatei.
- Bei mehreren Anhängen:
  - keine Datei wird automatisch verschoben.
- Bei instabiler Datei:
  - keine beschädigte oder halbe Datei wird exportiert.

## 11. Fehler-/Warnmeldungen

Zu erwartende Statusmeldungen:

- `Warte auf Gerätedatei.`
- `Vorherige AIS-Datei wurde durch neuere AIS-Datei ersetzt.`
- `AIS-Datei abgelaufen: keine Gerätedatei innerhalb der Wartezeit gefunden.`
- `Dateipaar vollständig, warte auf XDT-Anhang.`
- `XDT-Anhang optional: Timeout erreicht, Export ohne Anhang.`
- `XDT-Anhang Pflicht: Timeout erreicht, Verarbeitung blockiert.`
- `Mehrere XDT-Anhänge gefunden: keine automatische Zuordnung.`
- `XDT-Anhang ist noch nicht stabil; wird später erneut geprüft.`

## 12. Automatisierte Testabdeckung

Aktuell relevante automatisierte Tests:

| Praxisfall | Automatisierte Abdeckung |
|---|---|
| Verarbeitung ohne Attachment bleibt ohne `6302` bis `6305` | `AutoImportPairProcessingCoordinatorTests`, `InterfaceProfileManualProcessorTests` |
| Optionaler Anhang sofort vorhanden | `ProcessReadyPairs_AttachmentShouldPrepareExactlyOneSupportedCandidate` |
| Optionaler Anhang kommt innerhalb Timeout | `ProcessReadyPairs_OptionalAttachmentShouldWaitAndThenUseAttachmentWithinTimeout` |
| Optionaler Anhang Timeout | `ProcessReadyPairs_AttachmentShouldSkipWhenScannerFindsNoSupportedCandidate` |
| Pflicht-Anhang Timeout | `ProcessReadyPairs_RequiredAttachmentShouldBlockAfterTimeout` |
| Mehrere Anhänge optional/Pflicht-Entscheidung | `AttachmentPackageDecisionServiceTests`, `ProcessReadyPairs_AttachmentShouldSkipMultipleSupportedCandidates` |
| Instabiler Anhang | `ProcessReadyPairs_AttachmentShouldSkipUnstableSupportedCandidate`, `AttachmentAutoCandidateSelectionServiceTests`, `AttachmentImportFolderScannerServiceTests` |
| AIS wartet auf Gerätedatei | `AutoImportPackageStateServiceTests.Evaluate_ShouldWaitForDeviceFileWhenOnlyAisFileExists` |
| Gerätedatei kommt später | `AutoImportPackageStateServiceTests.Evaluate_ShouldReturnReadyPairWhenDeviceFileArrivesWithinTimeout` |
| Gerätedatei kommt nicht | `AutoImportPackageStateServiceTests.Evaluate_ShouldExpireAisFileWhenDeviceFileDoesNotArrive` |
| Neue AIS-Datei ersetzt alte | `AutoImportPackageStateServiceTests.Evaluate_ShouldReplaceWaitingAisFileWithNewerAisFile` |
| Neue AIS-Datei wird für späteres Paar verwendet | `AutoImportPackageStateServiceTests.Evaluate_ShouldUseNewerAisAfterReplacementWhenDeviceArrives` |
| XDT-Längenpräfixe für Linkfelder | `XdtExportBuilderTests`, `ExternalAisLinkXdtFieldAdapterTests`, `InterfaceProfileManualProcessorTests` |

Offene automatisierte Lücken:

- Ein echter WPF-End-to-End-Test mit laufender UI-Überwachung wird nicht automatisiert ausgeführt.
- Manuelle Dateisystemtests mit real langsam schreibenden Geräten bleiben Praxisabnahme.
- Keine produktive Mehrfachanhang-Zuordnungsheuristik, daher keine Tests für automatische Auswahl aus mehreren Anhängen.

## 13. Ergebnisprotokoll für manuelle Tests

| Testfall | Datum/Uhrzeit | Tester | Profil | Ergebnis bestanden ja/nein | Exportdatei | Anhang-Zieldatei | Auffälligkeiten | Screenshot/Notiz |
|---|---|---|---|---|---|---|---|---|
| 1 |  |  |  |  |  |  |  |  |
| 2 |  |  |  |  |  |  |  |  |
| 3 |  |  |  |  |  |  |  |  |
| 4 |  |  |  |  |  |  |  |  |
| 5 |  |  |  |  |  |  |  |  |
| 6 |  |  |  |  |  |  |  |  |
| 7 |  |  |  |  |  |  |  |  |
| 8 |  |  |  |  |  |  |  |  |
| 9 |  |  |  |  |  |  |  |  |
| 10 |  |  |  |  |  |  |  |  |
| 11 |  |  |  |  |  |  |  |  |
| 12 |  |  |  |  |  |  |  |  |

## 14. Abnahmekriterien

Der automatische AIS-/Geräte-/XDT-Anhang-Ablauf gilt für diesen Prototyp als abgenommen, wenn:

- Alle automatisierten Tests grün sind.
- Manuelle Testfälle 1 bis 12 mit synthetischen Testdaten erfolgreich protokolliert wurden.
- Export ohne aktivierte XDT-Anhang-Funktion unverändert bleibt.
- Optionaler Anhang innerhalb Timeout korrekt mit `6302` bis `6305` exportiert wird.
- Optionaler Anhang nach Timeout weggelassen wird.
- Pflicht-Anhang nach Timeout blockiert.
- Mehrere unterstützte Anhänge niemals automatisch ausgewählt werden.
- Instabile Dateien niemals verschoben, verlinkt oder verarbeitet werden.
- Neue AIS-Datei ältere wartende AIS-Datei ersetzt.
- Keine unbekannten Dateien gelöscht oder verschoben werden.
