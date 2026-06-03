# E2E-Testprotokoll MEDISTAR + NIDEK RT-6100

Stand: 2026-06-03

Status: echte XML-Fixtures testseitig validiert, praktische MEM-200-/MEDISTAR-Abnahme offen

## Ziel

Pruefung des bidirektionalen RT-6100-Workflows:

1. AIS/MEDISTAR liefert Patientenkontext und historische Messwerte.
2. XDTBox erzeugt RT-6100-kompatibles Ophthalmology-XML fuer MEM-200.
3. RT-6100 liefert nach Untersuchung eine Ophthalmology-XML-Rueckgabe.
4. XDTBox erzeugt eine MEDISTAR-kompatible XDT-Datei.

## Testumgebung

| Feld | Wert |
| --- | --- |
| Datum | offen |
| Tester | offen |
| XDTBox-Version | offen |
| MEDISTAR-Version | offen |
| RT-6100 / MEM-200 | offen |
| Schnittstellenprofil | MEDISTAR + NIDEK RT-6100 |
| MEM-200-Zielordner | offen, z. B. `DIRECT_RT_0A\TXT` |

## Voraussetzungen

- Keine echten Patientendaten verwenden.
- Testordner sind keine Produktivordner.
- Ausgabe an Geraet ist im Schnittstellenprofil konfiguriert.
- Ueberwachung laeuft innerhalb der geoeffneten XDTBox-App.
- Fuer die Rueckgabe wird eine echte wohlgeformte RT-6100-XML benoetigt.
- Testseitig vorhanden sind echte NIDEK-Fixtures fuer LM-7P, ARK-1s und RT-6100-Rueckgabe.

## Erwartete Geraete-Inputdatei

- Dateiname nach Schnittstellenprofil, Default `RTImport_{PatientNumber}_{yyyyMMdd}_{HHmmss}.xml`
- XML Root `Ophthalmology`
- `Common/Company = NIDEK`
- `Common/ModelName = RT-6100`
- `Common/Version = NIDEK_RT_V1.00`
- `Measure Type="RT"`
- `V0`/Lensmeter als `LM_Base`
- `V1`/Autorefraktion als `REF_Base`
- echte LM-7P-XML kann im Baukasten als `LM_Base`-Quelle dienen
- echte ARK-1s-XML kann im Baukasten als `REF_Base`-Quelle aus `ARMedian` dienen
- SR-/KM-/Bild-/Zusatzdaten werden fuer den RT-6100-Input nicht blind exportiert

## Erwartete MEDISTAR-Rueckgabe

- `Best` wird ueber `6228` ausgegeben.
- `Full` wird ueber `6227` ausgegeben.
- Nur `Distant`/`Standard` wird als MEDISTAR-Hauptausgabe verwendet.
- Keine `6330`.
- Keine kuenstliche Trennzeile.
- `8402` kommt aus AIS/MEDISTAR.

## Durchfuehrung

| Schritt | Ergebnis |
| --- | --- |
| AIS-Testdatei abgelegt | offen |
| RT-6100-Inputdatei erzeugt | offen |
| Inputdatei am MEM-200/RT-6100 eingelesen | offen |
| RT-6100-Rueckgabedatei erzeugt | testseitig mit echter Fixture vorhanden, live offen |
| XDTBox erkennt RT-6100-Rueckgabe | testseitig bestaetigt |
| XDTBox erzeugt MEDISTAR-XDT | offen |
| MEDISTAR importiert `6228`/`6227` korrekt | offen |

## Befund zur OCR-Beispieldatei

Die bereitgestellte Datei `NIDEK  RT6100.XML` ist keine positive Praxisfixture. Sie deklariert `UTF-16`, wirkt aber nicht wie UTF-16 und ist strukturell unvollstaendig. Sie wird nur fuer Diagnoseverhalten bei malformed XML verwendet.

## Bisherige echte Fixtures

- `LM__20251128120038_05D67D.xml`: LM-7P, `Measure type="LM"`, `Sphare/Cylinder/Axis`, Prismenwerte vorhanden und fuer RT-6100-Input aktuell nur diagnostisch.
- `ARK_              _20150528151629.xml`: ARK-1s, `ARMedian` wird fuer `REF_Base` verwendet; `VD` und `WorkingDistance` werden uebernommen; SR/KM/Bild-/Zusatzdaten werden ignoriert.
- `RT__20260602_095132__.xml`: RT-6100-Rueckgabe mit `LM_Base`, `REF_Base`, `Full`, `Best`; `Best -> 6228`, `Full -> 6227` ist testseitig bestaetigt.

## Ergebnis

Noch offen: Import der erzeugten RT-6100-Inputdatei am MEM-200/RT-6100 und praktische MEDISTAR-Abnahme der resultierenden `6228`-/`6227`-Rueckgabe.
