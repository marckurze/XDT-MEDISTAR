# E2E-Testprotokoll MEDISTAR + NIDEK RT-6100

Stand: 2026-05-28

Status: vorbereitet, praktische Abnahme offen

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

## Erwartete Geraete-Inputdatei

- Dateiname nach Schnittstellenprofil, Default `RTImport_{PatientNumber}_{yyyyMMdd}_{HHmmss}.xml`
- XML Root `Ophthalmology`
- `Common/Company = NIDEK`
- `Common/ModelName = RT-6100`
- `Common/Version = NIDEK_RT_V1.00`
- `Measure Type="RT"`
- `V0`/Lensmeter als `LM_Base`
- `V1`/Autorefraktion als `REF_Base`

## Erwartete MEDISTAR-Rueckgabe

- `Best` wird ueber `6228` ausgegeben.
- `Full` wird ueber `6227` ausgegeben.
- Keine `6330`.
- Keine kuenstliche Trennzeile.
- `8402` kommt aus AIS/MEDISTAR.

## Durchfuehrung

| Schritt | Ergebnis |
| --- | --- |
| AIS-Testdatei abgelegt | offen |
| RT-6100-Inputdatei erzeugt | offen |
| Inputdatei am MEM-200/RT-6100 eingelesen | offen |
| RT-6100-Rueckgabedatei erzeugt | offen |
| XDTBox erkennt RT-6100-Rueckgabe | offen |
| XDTBox erzeugt MEDISTAR-XDT | offen |
| MEDISTAR importiert `6228`/`6227` korrekt | offen |

## Befund zur OCR-Beispieldatei

Die bereitgestellte Datei `NIDEK  RT6100.XML` ist keine positive Praxisfixture. Sie deklariert `UTF-16`, wirkt aber nicht wie UTF-16 und ist strukturell unvollstaendig. Sie wird nur fuer Diagnoseverhalten bei malformed XML verwendet.

## Ergebnis

Noch offen.
