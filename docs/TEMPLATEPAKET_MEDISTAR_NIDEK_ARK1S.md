# Templatepaket MEDISTAR + NIDEK ARK1S

Stand: 2026-05-17

Status: Referenzpaket 1, reproduzierbar export-/importgeprueft, noch keine eingecheckte ZIP-Paketdatei

## Zweck

Dieses Dokument beschreibt das erste wiederverwendbare Referenzpaket fuer den praktisch validierten Workflow:

```text
MEDISTAR + NIDEK ARK1S
```

Der Workflow ist praktisch validiert, inklusive XDT-Anhang-Link ueber `6302`, `6303`, optional `6304` und `6305`. Das Paket soll Anwendern spaeter einen fertigen Startpunkt geben, damit der Baukasten nur fuer Sonderfaelle, Tests, Vorschau und kundenspezifische Anpassungen gebraucht wird.

## Vorhandene Paketstruktur

Das bestehende Projektformat fuer Templatepakete ist eine ZIP-Datei. Der vorhandene `TemplatePackageExporter` schreibt:

```text
package.json
ais/<ais-profile-id>.json
devices/<device-profile-id>.json
exports/<export-profile-id>.json
interfaces/<interface-profile-id>.json
```

Der vorhandene `TemplatePackageImporter` liest genau diese Struktur wieder ein. Export, Import, Validierung, Konfliktanalyse, Dry-Run, Importvorschau und sichere UserDefined-Uebernahme sind testseitig abgesichert. Die App-Importvorschau nutzt dafuer einen schlanken Preview-Service, damit der UI-Pfad dieselbe pruefbare Importstrecke verwendet.

Der App-Export nutzt jetzt eine Schnittstellenprofil-Auswahl als Paketbasis. Fuer MEDISTAR + NIDEK ARK1S wird das vorhandene Schnittstellenprofil `interface-medistar-nidek-ark1s-default` gewaehlt; der Export nimmt automatisch nur das referenzierte MEDISTAR-AIS-Profil, das NIDEK-ARK1S-Geraeteprofil, das passende MEDISTAR-Exportprofil und dieses Schnittstellenprofil auf. Ein Vollkatalog-Export aller vorbereiteten Geraete ist nicht mehr der Standardweg.

Die Tests erzeugen das Referenzpaket reproduzierbar mit dem vorhandenen `TemplatePackageExporter` in einem temporaeren Testordner und lesen es mit `TemplatePackageImporter` wieder ein. Dabei werden ZIP-Struktur, selektiver Paketinhalt, offensichtliche Live-/Kundendatenmarker, XDT-Anhang-Linkeinstellungen, der UI-nahe Preview-Pfad und der sichere Importfluss gegen einen temporaeren BuiltIn-Katalog geprueft. Die Vorschau zeigt bei uebersprungenen Schnittstellenprofilen jetzt einen deutschen Leerhinweis statt einer unverstaendlich leeren Abhaengigkeitstabelle.

Aktuell wird weiterhin keine manuell zusammengebaute ZIP-Datei eingecheckt. Ein dauerhaftes Release-Artefakt soll erst entstehen, wenn die kleine Freigaberegel aus `docs/TEMPLATEPAKET_RELEASE_REGEL.md` angewendet wurde.

## Paketmetadaten

Vorgeschlagene Metadaten fuer die spaetere Paketdatei:

| Feld | Wert |
| --- | --- |
| Paket-ID | `package-medistar-nidek-ark1s-v1` |
| Name | `MEDISTAR + NIDEK ARK1S Referenzpaket V1` |
| Profilart | `TemplatePackage` |
| Paketformat | `1.0` |
| Version | `1.0.0` |
| Hersteller / Ersteller | `XdtDeviceBridge` |
| Produkt | `MEDISTAR/NIDEK ARK1S` |

Beschreibung:

```text
Offizielles V1-Referenzpaket fuer den praktisch validierten Workflow MEDISTAR + NIDEK ARK1S. Enthaelt keine Kunden-/Patientendaten und keine Live-Pfade.
```

## Enthaltene Profile

| Profilart | Profil-ID | Name | Status |
| --- | --- | --- | --- |
| AIS-Profil | `ais-medistar-default` | `MEDISTAR` | BuiltIn vorhanden |
| Geraeteprofil | `device-nidek-ark1s-default` | `NIDEK ARK1S` | BuiltIn vorhanden, praktisch validierter Referenzworkflow |
| Exportprofil | `export-medistar-nidek-ark1s-default` | `MEDISTAR + NIDEK ARK1S Export` | BuiltIn vorhanden |
| Schnittstellenprofil | `interface-medistar-nidek-ark1s-default` | `MEDISTAR + NIDEK ARK1S` | BuiltIn vorhanden, inaktiv |

## Fachlicher Umfang

Das Paket bildet den validierten ARK1S-Standard ab:

- MEDISTAR-GDT/XDT-Eingang
- NIDEK-ARK1S-XML-Eingang
- MEDISTAR-kompatibler XDT-Export
- `8000 = 6310`
- Patientendatenfelder, insbesondere `3000`, `3101`, `3102`, `3103`
- `8402` Untersuchungsart aus AIS
- `6228` Ergebniszeilen fuer rechts/links
- vorbereitete XDT-Anhang-Linkfelder `6302`, `6303`, optional `6304`, `6305`

Die praktische Validierung des Anhang-Links ist dokumentiert in `docs/E2E_TESTPROTOKOLL_MEDISTAR_ARK1S_XDT_ANHANG.md`.

## XDT-Anhang-Konfiguration

Die vorhandene Schnittstellenprofil-Struktur enthaelt die Linkfeld-Vorlagen:

| XDT-Feld | Bedeutung | Vorhandener Standard |
| --- | --- | --- |
| `6302` | Dokumentname / Anzeige in Karteikarte | `Datei` |
| `6303` | Dateiformat | `{ExtensionUpperWithoutDot}` |
| `6304` | Beschreibung, optional | leer |
| `6305` | vollstaendiger Dateipfad / Pfadtemplate | `{Attachment.TargetFullPath}` |

Wichtige Sicherheitsgrenze: Das Referenzpaket enthaelt keine Live-Pfade. Ordner fuer AIS-Import, Geraete-Import, Export, Archiv, Fehlerablage sowie XDT-Anhang-Import/-Export muessen nach dem Import anwenderspezifisch gesetzt und geprueft werden.

## Import- und Sicherheitserwartung

Beim Import muessen die bestehenden Sicherheitsregeln gelten:

- BuiltIn-Profile werden nicht ueberschrieben.
- Konflikte und BuiltIn-geschuetzte Profile stehen in der Vorschau zunaechst auf `Ueberspringen`.
- Import als UserDefined-Kopie erfolgt nur nach bewusster Auswahl; der vorgeschlagene Zielname kann geaendert werden.
- Hinweise und Warnungen in der Importvorschau sind deutschsprachig und werden dedupliziert.
- Importierte Schnittstellenprofile werden nicht automatisch aktiv.
- `IsAttachmentProcessingEnabled` wird bei importierten Schnittstellenprofilen deaktiviert.
- `IsActive` bleibt aus.
- Ordnerpfade muessen vor produktiver Nutzung geprueft werden.
- Aktivierungslogik bleibt getrennt und wird durch das Paket nicht ausgefuehrt.
- Es wird keine Verarbeitung gestartet.
- Es werden keine Dateien kopiert, verschoben, geloescht oder erzeugt.

## Keine Kundendaten

Diese Vorlage und die spaetere Paketdatei duerfen nicht enthalten:

- Patientendaten
- Kundendaten
- Praxisnamen
- Live-System-Pfade
- echte Serverfreigaben
- echte Export-/Archiv-/Fehlerordner

## Reproduzierbarer Testweg

Der automatisierte Test erzeugt die ZIP-Datei nur temporaer und prueft:

- `package.json` und die Ordner `ais/`, `devices/`, `exports/`, `interfaces/`
- genau die vier Referenzprofile fuer MEDISTAR + NIDEK ARK1S
- keine nicht benoetigten LM7/LM7P-, NT530P- oder TOPCON-Profile
- fehlende AIS-, Geraete- oder Exportprofil-Abhaengigkeiten blockieren den Export vor dem Schreiben der ZIP
- der vorgeschlagene Dateiname ist `medistar-nidek-ark1s-v1.templatepackage.zip`
- XDT-Anhang-Linkeinstellungen fuer `6302`, `6303`, optional `6304` und `6305`
- keine offensichtlichen Live-Pfade, Kundenmarker oder Patientendatenmarker
- UI-nahe Importvorschau kehrt zurueck und schreibt keine Profile
- Konfliktstandard ist `Ueberspringen`; `Als Kopie importieren` wird erst durch Benutzerentscheidung aktiv
- leere Abhaengigkeitsauflösung wird verstaendlich erklaert; nicht aufgeloeste Abhaengigkeiten werden als `Nicht aufgeloest` angezeigt
- Import gegen vorhandene BuiltIn-Profile nur als UserDefined-Kopie
- importiertes Schnittstellenprofil bleibt inaktiv
- `IsAttachmentProcessingEnabled` bleibt nach Import deaktiviert

Der gleichartige Referenzpaket-Status fuer MEDISTAR + NIDEK AR360 ist in `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_AR360.md` dokumentiert. Beide ZIPs bleiben bis zur Release-Regel temporaere Testartefakte.

## Naechster kleiner Schritt

1. Freigaberegel aus `docs/TEMPLATEPAKET_RELEASE_REGEL.md` anwenden.
2. Danach das Artefakt z. B. unter `template-packages/medistar-nidek-ark1s-v1.templatepackage.zip` reproduzierbar erzeugen.
3. Praktische App-Abnahme wiederholen: Paket in der UI importieren, bewusst Kopien auswaehlen, Zielnamen pruefen, stabile Importvorschau pruefen, keine automatische Aktivierung.
