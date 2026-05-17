# Templatepaket-Kandidat MEDISTAR + NIDEK AR360

Stand: 2026-05-17

## Zweck

Dieser Kandidat beschreibt das testgestuetzt vorbereitete Paket fuer `MEDISTAR + NIDEK AR360`. Es wird noch nicht als dauerhaftes ZIP-Release-Artefakt abgelegt.

## Enthaltene Profile

| Typ | ID | Name | Status |
| --- | --- | --- | --- |
| AIS-Profil | `ais-medistar-default` | `MEDISTAR` | BuiltIn vorhanden |
| Geraeteprofil | `device-nidek-ar360-default` | `NIDEK AR360` | BuiltIn, aus AR360-Beispielwerten getestet |
| Exportprofil | `export-medistar-nidek-ar360-default` | `MEDISTAR + NIDEK AR360 Export` | BuiltIn, ARMedian-Ausgabe getestet |
| Schnittstellenprofil | `interface-medistar-nidek-ar360-default` | `MEDISTAR + NIDEK AR360` | BuiltIn, inaktiv |

## Regeln

- XML-Dateien mit beliebigem Dateinamen und Endung `.XML` werden als Geraetedatei erkannt.
- Der generische `XmlDeviceParser` liest die NIDEK-LAN-XML-Struktur inklusive UTF-16.
- Fuer MEDISTAR werden `ARMedian`-Werte verwendet, nicht `ARList`, `TrialLens` oder `ContactLens`.
- Ausgabeformat:

```text
R.:S=+ 2.00 Z=- 1.25*172 PD= 60 VD= 12.00
L.:S=+ 1.00 Z=- 0.75*170
```

Die bereitgestellte MEDISTAR-TXT zeigt rechts Achse `177`; die ARMedian-Beispiel-XML enthaelt rechts `172`. Das Profil gibt deshalb `172` aus.

## Sicherheit

- Kein ZIP-Artefakt wird in diesem Schritt dauerhaft abgelegt.
- Keine Kunden-/Patientendaten und keine Live-Pfade sind enthalten.
- Importierte Schnittstellenprofile bleiben weiterhin inaktiv.
- XDT-Anhang-Einstellungen muessen bei spaeterer Nutzung separat geprueft werden.

## Naechster Schritt

In der App selektiv `MEDISTAR + NIDEK AR360` exportieren, wieder importieren und mit echten AR360-Dateien einen MEDISTAR-Testimport protokollieren.
