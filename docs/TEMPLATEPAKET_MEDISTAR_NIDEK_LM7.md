# Templatepaket MEDISTAR + NIDEK LM7

Dieses Dokument beschreibt den Templatepaket-Kandidaten fuer `MEDISTAR + NIDEK LM7` / `LM-7P`. Die technische Verarbeitung ist mit einer echten LM7-XML-Fixture abgesichert; eine praktische MEDISTAR-Abnahme steht noch aus. Ein offizielles ZIP-Artefakt wird erst nach der Release-Regel in `docs/TEMPLATEPAKET_RELEASE_REGEL.md` dauerhaft abgelegt.

## Enthaltene Profile

| Profiltyp | ID | Name | Hinweis |
| --- | --- | --- | --- |
| AIS-Profil | `ais-medistar-default` | `MEDISTAR` | BuiltIn |
| Geraeteprofil | `device-nidek-lm7-default` | `NIDEK LM7` | BuiltIn, Lensmeter XML |
| Exportprofil | `export-medistar-nidek-lm7-default` | `MEDISTAR + NIDEK LM7 Export` | BuiltIn, `6228`-Lensmeterzeilen |
| Schnittstellenprofil | `interface-medistar-nidek-lm7-default` | `MEDISTAR + NIDEK LM7` | BuiltIn, inaktiv |

## Datenformat

- LM-7/LM-7P schreibt pro Messung eine XML-Datei mit Root `Ophthalmology`.
- Unterstuetzt werden `Measure type="LM"` beziehungsweise `Measure Type="LM"` und die Versionen `NIDEK_V1.00` / `NIDEK_V1.01`.
- Der Parser akzeptiert die Handbuch-Schreibweise `Sphere`/`NearSphere` und die echte Geraeteschreibweise `Sphare`/`NearSphare`.
- `NIDEK_LM_Stylesheet.xsl` ist nur fuer die Anzeige der XML-Datei und wird nicht als Geraetedatei verarbeitet.

## MEDISTAR-Ausgabe

Die MEDISTAR-Zeilen werden aus den XML-Werten erzeugt und nicht aus den Formatbeispielen uebernommen. Fuer die echte Fixture ergeben sich:

```text
R.:S=+ 6.25 Z=- 3.25*  3
L.:S=+ 6.50 Z=- 2.75*170 A=+ 1.50
```

Optionale Werte werden nur ausgegeben, wenn sie vorhanden sind: `ADD` als `A=`, `ADD2` als `A2=`, Prisma aus `PrismX`/`PrismY` mit Basisrichtung und Gesamt-PD aus `PD/Distance`. Leere Werte werden nicht mit `0` aufgefuellt.

Hinweis zum Live-Preview-Fix: Falls lokal bereits ein aelteres BuiltIn-Exportprofil `MEDISTAR + NIDEK LM7 Export` mit `Device.R/LM/Median/...`-Platzhaltern gespeichert war, wird genau dieses BuiltIn beim Katalogstart auf die berechneten `MedistarLine`-Pfade repariert. Dadurch werden die LM7-Werte im Vorschau-/Exportpfad wieder aufgeloest; UserDefined-Profile bleiben unveraendert.

## Testabdeckung

`NidekLm7ProfileTests` prueft Erkennung, echte XML-Werte, `Sphare`/`Sphere`-Alias, MEDISTAR-Ausgabe, BuiltIn-Profile, Parser-/Exportpfad-Paritaet und den reparierten persistierten Live-Katalogpfad. `MedistarNidekLm7TemplatePackageTests` prueft den selektiven Templatepaket-Export und -Import temporaer im Testordner. Das Paket enthaelt nur MEDISTAR, NIDEK LM7, das passende Exportprofil und das Schnittstellenprofil; ARK1S-, AR360-, NT530P- und TOPCON-Profile sind nicht enthalten.

## Offen

- Praktische MEDISTAR-Abnahme mit echter LM7-Datei.
- Weitere Dateien fuer Prisma- und PD-Faelle sammeln.
- Offizielles ZIP-Artefakt erst nach Release-Regel und App-Abnahme ablegen.
