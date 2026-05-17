# Templatepaket-Release-Regel

Stand: 2026-05-17

Diese Regel gilt fuer offizielle, dauerhaft abgelegte Templatepaket-ZIP-Dateien. Sie ist bewusst klein gehalten und ersetzt keine grosse Releaseverwaltung.

## Freigabevoraussetzungen

Ein Templatepaket-ZIP darf als offizielles Artefakt abgelegt werden, wenn:

1. der selektive Exporttest fuer das Schnittstellenprofil gruen ist
2. Import-, Preview- und DryRun-Test gruen sind
3. das Paket keine Live-Pfade, Kunden- oder Patientendaten enthaelt
4. eine praktische App-Importpruefung erfolgt ist
5. BuiltIn-Schutz bestaetigt ist
6. importierte Schnittstellenprofile inaktiv bleiben
7. `IsAttachmentProcessingEnabled` nach Import deaktiviert bleibt
8. Templatepaket- und E2E-Dokumentation vorhanden sind
9. der Dateiname eindeutig versioniert ist

## Vorgesehene Dateinamen

- `medistar-nidek-ark1s-v1.templatepackage.zip`
- `medistar-nidek-ar360-v1.templatepackage.zip`

## Grenzen

- Keine manuell zusammengebauten ZIPs.
- Keine Signierung oder Installerlogik in dieser Regel.
- Keine automatische Aktivierung durch Import.
- Keine Kundendaten, Patientendaten oder Live-System-Pfade.
