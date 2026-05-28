# Templatepaket MEDISTAR + NIDEK RT-6100

Stand: 2026-05-28

Dieses Dokument beschreibt den vorbereiteten Templatepaket-Kandidaten fuer den bidirektionalen Phoropter NIDEK RT-6100.

## Zweck

`MEDISTAR + NIDEK RT-6100` verbindet zwei Richtungen:

1. AIS/MEDISTAR -> XDTBox -> RT-6100/MEM-200
2. RT-6100 -> XDTBox -> AIS/MEDISTAR

Die technische Anbindung ist `NetworkLan`: Datei-/UNC-/Shared-Folder-Austausch, nicht RS232.

## Enthaltene BuiltIns

- Geraeteprofil: `device-nidek-rt6100-default`
- Exportprofil: `export-medistar-nidek-rt6100-default`
- Schnittstellenprofil: `interface-medistar-nidek-rt6100-default`

Das Schnittstellenprofil ist als BuiltIn vorbereitet, aber nicht automatisch aktiv. Konkrete Praxisordner werden im UserDefined-Schnittstellenprofil gepflegt.

## Ordner

Der RT-6100 besitzt weiterhin:

- AIS-Patienten Datei an XDTBox
- Geraetedatei an XDTBox fuer RT-6100-Rueckgabedateien
- Ergebnisdatei an AIS
- Archiv
- Fehler

Zusaetzlich ist `Ausgabe an Geraet` relevant. Der Ausgabeordner zeigt auf den MEM-200-/Shared-Folder-Unterordner fuer direkte RT-6100-XML-Eingabe, zum Beispiel:

```text
\\MEM-200\DATA\DIRECT_RT_0A\TXT
```

Weitere moegliche `DIRECT_RT_xx\TXT`-Ordner sind in `docs/NIDEK_RT6100_PROTOKOLL_NOTIZEN.md` dokumentiert. XDTBox erzwingt keinen festen Ordner.

## Eingabe an das Geraet

XDTBox erzeugt eine RT-6100-Ophthalmology-XML-Datei. Default-Dateiname:

```text
RTImport_{PatientNumber}_{yyyyMMdd}_{HHmmss}.xml
```

Konservatives Mapping:

- MEDISTAR `V0`/Lensmeter -> `LM_Base`
- MEDISTAR `V1`/Autorefraktion -> `REF_Base`

Andere historische Werte werden aktuell nicht an den RT-6100 geschrieben, solange keine praktische Freigabe vorliegt.

## Rueckgabe an MEDISTAR

Der Rueckgabeparser erkennt NIDEK RT-6100-Ophthalmology-XML mit `Measure Type="RT"`.

Mapping:

- `Best` -> `6228` finaler Verordnungswert
- `Full` -> `6227` Maximalwert/Vollkorrektion

Bewusst ausgeschlossen:

- keine `6330`
- keine kuenstliche Trennzeile
- keine erfundenen Werte
- keine automatische Ausgabe von Prisma/Visusfeldern ohne MEDISTAR-Fachfreigabe

`8402` kommt aus AIS/MEDISTAR.

## Tests

Automatisiert abgesichert sind:

- RT-6100-BuiltIn-Profile
- selektiver Templatepaket-Export/-Import
- Inputwriter fuer `LM_Base`/`REF_Base`
- Rueckgabeparser fuer `Best` und `Full`
- keine `6330`
- malformed OCR-XML erzeugt Diagnose statt falscher Messwerte

## Offene Abnahme

Noch nicht praktisch abgenommen:

- Einlesen der erzeugten Importdatei am echten RT-6100/MEM-200.
- Echte wohlgeformte RT-6100-Rueckgabe nach Untersuchung.
- MEDISTAR-Import der `6228`-/`6227`-Rueckgabe.
- Endgueltige Freigabe eines offiziellen ZIP-Artefakts nach Release-Regel.
