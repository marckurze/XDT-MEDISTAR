# Templatepaket MEDISTAR + NIDEK AR360

Stand: 2026-05-17

## Zweck

Dieses Dokument beschreibt das zweite Referenzpaket fuer `MEDISTAR + NIDEK AR360`. Die Auto-Refraktor-XDT-Rueckgabe wurde praktisch in MEDISTAR validiert; eine dauerhaft abgelegte ZIP-Release-Datei bleibt bis zur Freigabe nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md` offen.

## Enthaltene Profile

| Typ | ID | Name | Status |
| --- | --- | --- | --- |
| AIS-Profil | `ais-medistar-default` | `MEDISTAR` | BuiltIn vorhanden |
| Geraeteprofil | `device-nidek-ar360-default` | `NIDEK AR360` | BuiltIn, praktisch fuer Auto-Refraktor-XDT-Rueckgabe validiert |
| Exportprofil | `export-medistar-nidek-ar360-default` | `MEDISTAR + NIDEK AR360 Export` | BuiltIn, ARMedian-Ausgabe praktisch validiert |
| Schnittstellenprofil | `interface-medistar-nidek-ar360-default` | `MEDISTAR + NIDEK AR360` | BuiltIn, inaktiv |

## Regeln

- XML-Dateien mit beliebigem Dateinamen und Endung `.XML` werden als Geraetedatei erkannt.
- Der generische `XmlDeviceParser` liest die NIDEK-LAN-XML-Struktur inklusive UTF-16.
- Fuer MEDISTAR werden `ARMedian`-Werte verwendet, nicht `ARList`, `TrialLens` oder `ContactLens`.
- Ausgabeformat:

```text
R.:S=+ 2.00 Z=- 1.25*172 PD= 60 VD= 12.00 mm
L.:S=+ 1.00 Z=- 0.75*170
```

Die bereitgestellte MEDISTAR-TXT zeigt rechts Achse `177`; die ARMedian-Beispiel-XML enthaelt rechts `172`. Das Profil gibt deshalb `172` aus.

Das praktische MEDISTAR-Protokoll steht in `docs/E2E_TESTPROTOKOLL_MEDISTAR_AR360.md`. Validiert wurden `8402 = AR360` und die `6228`-Zeilen rechts/links.

## Reproduzierbarer Testweg

`MedistarNidekAr360TemplatePackageTests` erzeugt das Paket temporaer ueber den selektiven Export aus `interface-medistar-nidek-ar360-default`, prueft ZIP-Struktur und Paketinhalt, liest es mit `TemplatePackageImporter` wieder ein und deckt Importvorschau, DryRun und sichere UserDefined-Kopie ab. Das Paket enthaelt nur MEDISTAR, NIDEK AR360, das passende MEDISTAR-Exportprofil und das Schnittstellenprofil; ARK1S-, LM7-, NT530P- und TOPCON-Profile sind nicht enthalten.

## Sicherheit

- Kein ZIP-Artefakt wird in diesem Schritt dauerhaft abgelegt.
- Keine Kunden-/Patientendaten und keine Live-Pfade sind enthalten.
- Importierte Schnittstellenprofile bleiben weiterhin inaktiv.
- XDT-Anhang-Einstellungen wurden in diesem AR360-Test nicht neu validiert und muessen bei spaeterer Nutzung separat geprueft werden.

## Naechster Schritt

Freigaberegel anwenden, danach ein versioniertes ZIP-Artefakt erzeugen und bei Bedarf den AR360-XDT-Anhangfall separat praktisch pruefen.
