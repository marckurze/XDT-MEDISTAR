# E2E-Testprotokoll MEDISTAR + NIDEK LM7 / LM-7

Status: praktisch validiert fuer Lensmeter-XDT-Rueckgabe aus echter NIDEK-LM7-XML-Beispieldatei.

Datum der Dokumentation: 2026-05-18

## Validierter Ablauf

- MEDISTAR liefert eine AIS-GDT/XDT-Datei mit passender Untersuchungsart.
- NIDEK LM7 liefert eine XML-Geraetedatei mit Lensmeter-Messwerten.
- XdtDeviceBridge liest die AIS-Datei und die LM7-XML-Datei ein.
- XdtDeviceBridge erzeugt eine MEDISTAR-kompatible XDT-Rueckgabedatei.
- MEDISTAR uebernimmt `8402` / Untersuchungsart aus der AIS-Datei.
- MEDISTAR uebernimmt die `6228`-Ergebniszeilen fuer rechts und links.

## Validierte Ergebnisfelder

| Feld | Bedeutung | Status |
| --- | --- | --- |
| `8000` | Nachrichtenart `6310` | validiert |
| `3000` | Patientennummer | validiert, anonymisiert |
| `3101` | Nachname | validiert, anonymisiert |
| `3102` | Vorname | validiert, anonymisiert |
| `3103` | Geburtsdatum | validiert, anonymisiert |
| `8402` | Untersuchungsart aus AIS/MEDISTAR | validiert |
| `6228` | rechte Lensmeter-Zeile | validiert |
| `6228` | linke Lensmeter-Zeile | validiert |

## Validiertes Ergebnis

Anonymisierte XDT-Struktur:

```text
01380006310
0153000<Patientennummer>
0173101<Testnachname>
0133102<Testvorname>
0173103<Geburtsdatum>
0xx8402<LM7-Untersuchungsart aus AIS>
0xx6228R.:S=+ 6.25 Z=- 3.25*  3
0xx6228L.:S=+ 6.50 Z=- 2.75*170 A=+ 1.50
```

Anonymisierte MEDISTAR-Karteikartenanzeige:

```text
V0 XD:<LM7-Untersuchungsart aus AIS>
V0 R.:S=+ 6.25 Z=- 3.25*  3
V0 L.:S=+ 6.50 Z=- 2.75*170 A=+ 1.50
```

Die im Praxislauf sichtbare Untersuchungsart wurde als Test-/Praxisbezeichnung behandelt und hier bewusst anonymisiert. Es werden keine echten Patientendaten, Kundendaten oder Live-Pfade dokumentiert.

## Fachliche Bestaetigung

- AIS-Datei wurde eingelesen.
- NIDEK-LM7-Geraetedatei wurde eingelesen.
- Exportdatei wurde erzeugt.
- MEDISTAR hat die Lensmeter-Zeilen uebernommen.
- `8402` / Untersuchungsart kommt aus AIS/MEDISTAR und wird nicht hart aus dem Geraet gesetzt.
- Rechte Lensmeter-Zeile enthaelt die echten XML-Werte `Sphere/Sphare=+6.25`, `Cylinder=-3.25`, `Axis=3`.
- Linke Lensmeter-Zeile enthaelt die echten XML-Werte `Sphere/Sphare=+6.50`, `Cylinder=-2.75`, `Axis=170`, `ADD=+1.50`.
- `A=+ 1.50` wird nur links ausgegeben, weil nur links `ADD` vorhanden ist.
- Leere optionale Fragmente wie `P=`, `PD=`, leeres `A=` oder `A2=` bleiben nicht stehen.
- Werte aus `MEDISTAR Eintrag.txt` wurden nicht als Messwerte uebernommen; die TXT dient nur als Formatbeispiel.

## Datenquelle und XML-Struktur

- Testgrundlage ist eine echte NIDEK-LM7-XML-Datei aus dem bereitgestellten Beispielmaterial.
- Die XML-Struktur nutzt `Ophthalmology`, `Common`, `Measure Type="LM"` bzw. `Measure type="LM"` und `LM/R` sowie `LM/L`.
- `Common/ModelName` ist `LM-7`.
- `Common/Version` ist in der getesteten Datei `NIDEK_V1.00`.
- Die echte XML-Datei nutzt die Geraeteschreibweise `Sphare` und `NearSphare`.
- Der Parser akzeptiert zusaetzlich die Handbuch-Schreibweise `Sphere` und `NearSphere`.
- `NIDEK_LM_Stylesheet.xsl` ist ein Anzeige-Stylesheet und wird nicht als Geraetedatei verarbeitet.

## Mapping- und BuiltIn-Hinweis

Der vorherige Live-Befund mit leeren `6228`-Zeilen wurde durch ein altes persistiertes BuiltIn-Exportprofil verursacht, das noch `Device.R/LM/Median/...`-Pfade verwendete. Der Katalog repariert dieses konkrete BuiltIn `export-medistar-nidek-lm7-default` beim Start auf:

```text
Device.Measure[@Type='LM']/LM/R/MedistarLine
Device.Measure[@Type='LM']/LM/L/MedistarLine
```

Der praktische MEDISTAR-Lauf bestaetigt den korrigierten Live-/Preview-Pfad.

## Grenzen und offene Punkte

- Prisma- und PD-Faelle benoetigen weitere echte LM7-Dateien, wenn diese Werte praktisch genutzt werden sollen.
- Der XDT-Anhang-Link wurde fuer LM7 nicht separat praktisch validiert.
- Ein offizielles ZIP-Artefakt wird erst nach der Release-Regel in `docs/TEMPLATEPAKET_RELEASE_REGEL.md` dauerhaft abgelegt.
- Dieses Protokoll dokumentiert keine echten Patientendaten, Kundendaten oder Live-Pfade.
