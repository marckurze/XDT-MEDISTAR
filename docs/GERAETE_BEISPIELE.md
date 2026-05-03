# Gerätebeispiele und Profilableitungen

## 1. Zweck des Dokuments

Dieses Dokument sammelt Erkenntnisse aus bereitgestellten Beispielordnern verschiedener ophthalmologischer Geräte. Es dient als Grundlage für spätere Geräteprofile, Export-/Mapping-Profile, PDF-Dokumentenerzeugung und MEDISTAR-EV-Verknüpfungen.

Hinweis: Die aktuelle App implementiert diese Profile noch nicht vollständig. Das Dokument dient der fachlichen Vorbereitung.

## 2. Übersicht erkannter Geräte

| Hersteller | Gerät | Gerätetyp | beobachtete Dateiformate | Untersuchungsarten | Zusatzdateien | MEDISTAR-Zielbild | spätere Profilrelevanz |
| --- | --- | --- | --- | --- | --- | --- | --- |
| NIDEK | AR1S | Autorefraktometer | XML | Refraktion, PD | keine im aktuellen Standardfall | zwei `6228`-Ergebniszeilen rechts/links, `8000=6310`, `8402` aus AIS | erstes validiertes Standardprofil, Referenz für Refraktionsformatierung |
| NIDEK | LM7 | Lensmeter / Scheitelbrechwertmesser | XML oder gerätespezifisches Format aus Beispieldaten | Brillenwerte, Sphäre, Zylinder, Achse, Addition, Prisma, Basisrichtung, PD | keine zwingend erkennbar | Lensmeter-Ergebniszeilen mit Sphäre/Zylinder/Achse/Prisma/PD | zweites naheliegendes Geräteprofil wegen ähnlicher refraktiver Ergebniszeilen |
| NIDEK | NT530P | Non-Contact-Tonometer / Pachymeter | XML, JPG | Tonometrie, Pachymetrie, korrigierter IOP, Messbilder/Protokollverweise | JPG-Bilder, ggf. XML-Verweise wie `PACHYImage` | Pachymetrie- und Tonometriezeilen, optional EV-Verweis | wichtig für Attachments, PDF-Protokolle und EV-Verknüpfung |
| TOPCON | CL300 | Lensmeter | Ophthalmology-/JOIA-XML | Lensmeterdaten, Sphäre, Zylinder, Achse, PD | keine zwingend erkennbar | Lensmeter-Ergebniszeilen ähnlich LM7 | erstes TOPCON-/JOIA-Profil mit Namespace- und Attributanforderungen |
| TOPCON | KR800 | Autorefraktometer / Keratometer | Ophthalmology-/JOIA-XML | `REF`, `KM`, `SBJ` | keine zwingend erkennbar | getrennte Ergebniszeilen für Refraktion, Keratometrie und optional subjektive Daten | relevant für Mehruntersuchungsdateien und Measure-Type-Selektion |
| TOPCON | TRK2P | Tonometer / Refraktions-Keratometer je nach Dateninhalt | Ophthalmology-/JOIA-XML | `TM`, `CCT` | keine zwingend erkennbar | Tonometrie- und Pachymetrieausgabe | relevant für Tonometrie/CCT-Kombination und JOIA-Parserlogik |

## 3. NIDEK AR1S

Gerätetyp: Autorefraktometer

Dateiformat: XML

Untersuchungsarten:

- Refraktion
- PD

Werte:

- Sphäre rechts/links
- Zylinder rechts/links
- Achse rechts/links
- SE rechts/links
- PD/FarPD
- PD/NearPD

Validierter aktueller Prototyp:

- `8000 = 6310`
- `8402 = Untersuchungsart aus AIS`, z. B. `ARK1S`
- `6228` Ergebnis rechts
- `6228` Ergebnis links

Beispielausgabe MEDISTAR:

```text
V1 R.:S=- 0.25 Z=- 0.25* 49                              PD=61
V1 L.:S=+ 0.00 Z=- 0.50* 63                              PD=61
```

Status: bereits im Prototyp erfolgreich validiert.

## 4. NIDEK LM7

Gerätetyp: Lensmeter / Scheitelbrechwertmesser

Dateiformat: XML oder gerätespezifisch aus Beispieldaten

Untersuchungsarten:

- gemessene Brillenwerte
- Sphäre
- Zylinder
- Achse
- Prisma
- Basisrichtung
- PD

Beispielausgabe MEDISTAR:

```text
V0   R.:S=+ 6.50 Z=- 1.75*172 P=0.75 OUT 1.00 UP           PD= 59
V0   L.:S=+ 6.00 Z=- 2.25*  2 P=0.50 OUT 1.50 UP
```

Ableitung:

- eigenes Exporttemplate erforderlich
- Dioptrienformatierung erforderlich
- Achsenformatierung erforderlich
- Prisma-/Basisformatierung erforderlich
- PD optional rechts/gesamt

Spätere Profilanforderung:

- Geräteprofil `NIDEK LM7`
- Exportprofil `MEDISTAR + NIDEK LM7`
- Ergebniszeilen über `6228` oder MEDISTAR-konfiguriertes Ziel prüfen

## 5. NIDEK NT530P

Gerätetyp: Non-Contact-Tonometer / Pachymeter

Dateiformate:

- XML
- JPG-Begleitdateien

Untersuchungsarten:

- Tonometrie
- Pachymetrie
- korrigierter Augeninnendruck
- optional Messbilder/Protokolldateien

Beispielausgabe MEDISTAR:

```text
Y  PR: 559 560 558 [559] µm
Y  PL: 559 560 [560] µm
Y  PR: Gemessen = 12.7 mmHg; Korrigiert = 12.3 mmHg; ...
P  R = 12 11 15 [12.7] // L = 14 13 15 [14.0] mmHg 14:51 / EV:{000000003B} NT-530P Messung
```

Zusatzdateien:

- JPG-Dateien können mitgeliefert werden
- XML kann auf Bilddateien verweisen, z. B. `PACHYImage`

Ableitung:

- mehrere Ergebniszeilen erforderlich
- Pachymetrie-Template
- Tonometrie-Template
- korrigierter IOP optional
- Attachment-/EV-Verknüpfung relevant
- optionale PDF-Erzeugung sinnvoll

Spätere Profilanforderung:

- `DeviceProfileDefinition` mit mehreren Untersuchungsarten
- `ExportProfileDefinition` mit mehreren `6228`-Regeln oder MEDISTAR-spezifischen Zielzeilen
- `AttachmentDefinition` / `DocumentExportRule`
- EV-Verweis als optionales Exporttemplate

## 6. TOPCON CL300

Gerätetyp: Lensmeter

Dateiformat:

- XML im Ophthalmology-/JOIA-Format
- Namespaces möglich

Beispielhafte SourcePath-Struktur:

```text
Ophthalmology/Common/Company = TOPCON
Ophthalmology/Common/ModelName = CL-300
Ophthalmology/Measure[@type='LM']/LM/R/Sphere
Ophthalmology/Measure[@type='LM']/LM/R/Cylinder
Ophthalmology/Measure[@type='LM']/LM/R/Axis
Ophthalmology/Measure[@type='LM']/PD/B/Distance
```

Untersuchungsarten:

- Lensmeterdaten
- Sphäre
- Zylinder
- Achse
- PD

Ableitung:

- XML-Parser muss Namespaces und Attribute handhaben
- eigenes Geräteprofil `TOPCON CL300`
- ähnliches MEDISTAR-Template wie Lensmeter, aber andere SourcePaths

## 7. TOPCON KR800

Gerätetyp: Autorefraktometer/Keratometer

Dateiformat:

- XML im Ophthalmology-/JOIA-Format

Untersuchungsarten:

- `REF` = Refraktion
- `KM` = Keratometrie
- `SBJ` = subjektive Refraktion / Visus / PD

Ableitung:

- ein Gerät liefert mehrere Untersuchungsarten in einer Datei
- Exportprofil muss auswählen können, welche Untersuchungen übernommen werden
- Refraktion und Keratometrie benötigen getrennte Ergebnis-Templates
- Mapping muss `Measure[@type='...']` unterscheiden können

Spätere Profilanforderung:

- mehrere Messwertgruppen
- mehrere Ergebniszeilen
- optionale Zusammenfassung oder getrennte Ausgabe

## 8. TOPCON TRK2P

Gerätetyp: Tonometer / Refraktions-Keratometer je nach Dateninhalt

Dateiformat:

- XML im Ophthalmology-/JOIA-Format

Untersuchungsarten:

- `TM` = Tonometrie
- `CCT` = zentrale Hornhautdicke / Pachymetrie

Ableitung:

- Tonometrie-Template
- Pachymetrie-Template
- mögliche Kombination mit korrigiertem IOP
- XML-Attribute und Namespaces beachten

## 9. Allgemeine Erkenntnisse aus den Beispieldaten

1. Jedes Gerät liefert andere Dateistrukturen.
2. XML ist nicht gleich XML; NIDEK und TOPCON unterscheiden sich stark.
3. TOPCON-/JOIA-Dateien können Namespaces und Measure-Type-Attribute nutzen.
4. Ein Gerät kann mehrere Untersuchungsarten in einer Datei liefern.
5. MEDISTAR erwartet oft fertige Ergebniszeilen statt einzelner technischer Wertefelder.
6. Feld `6228` ist für Ergebnistext besonders relevant.
7. Feld `8402` Untersuchungsart kommt aus dem AIS und muss durchgereicht werden.
8. Feld `8000=6310` ist für den MEDISTAR-XDT-Import relevant.
9. Zusatzdateien wie JPG/PDF müssen optional unterstützt werden.
10. EV-Verweise sind ein eigener Exportmechanismus.
11. Optional selbst erzeugte PDF-Protokolle können einen deutlichen Mehrwert bieten.

## 10. Anforderungen an spätere Parser

- XML-Parser muss Attribute unterstützen.
- XML-Parser muss Namespaces unterstützen oder normalisieren.
- Parser muss mehrere Messwertgruppen erkennen.
- Parser muss wiederholte Elemente eindeutig machen.
- Parser muss Begleitdateien erkennen können.
- Parsermodus muss pro Geräteprofil konfigurierbar sein:
  - XML
  - XPath
  - Regex
  - CSV
  - Text
  - GDT/XDT
  - gerätespezifische Sonderparser

## 11. Anforderungen an spätere Exportprofile

- mehrere Ergebniszeilen möglich
- gleicher Zielfeldcode mehrfach möglich, z. B. `6228`
- mehrere Untersuchungsarten pro Datei
- Ausgabe als Ergebnistext, Einzelwerte, Kategorie/Wert-Paar oder Dokumentverweis
- EV-Zeile optional
- PDF-Dokument optional
- Ausgabe-Syntax frei editierbar
- Formatfunktionen je Platzhalter nutzbar:
  - Raw
  - Diopter
  - Axis
  - Pd
  - Iop
  - Pachy
  - Prism
  - Keratometry

## 12. Priorisierung

Empfohlene Reihenfolge für spätere Umsetzung:

1. ARK1S stabil halten.
2. LM7 als zweites Geräteprofil vorbereiten, weil es ähnliche refraktive Ergebniszeilen nutzt.
3. NT530P untersuchen, weil dort Tonometrie, Pachymetrie und Attachments/EV relevant werden.
4. TOPCON CL300 als erstes JOIA-/Namespace-XML-Profil vorbereiten.
5. TOPCON KR800 wegen Mehruntersuchungsdateien.
6. TOPCON TRK2P wegen Tonometrie/CCT.

## 13. Abgrenzung

Dieses Dokument ist eine fachliche Auswertung der Beispieldaten.

Es implementiert keine neuen Geräteprofile.

Es verändert nicht den bestehenden MEDISTAR/NIDEK-ARK1S-Prototyp.

Neue Profile sollen später schrittweise und testgetrieben umgesetzt werden.
