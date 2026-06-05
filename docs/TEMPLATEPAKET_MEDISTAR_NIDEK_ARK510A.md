# Templatepaket MEDISTAR + NIDEK ARK-510A

Status: vorbereitet aus Referenzdaten, praktische MEDISTAR-Abnahme offen.

## Inhalt

- AIS-Profil: MEDISTAR BuiltIn
- Geraeteprofil: `device-nidek-ark510a-default`
- Exportprofil: `export-medistar-nidek-ark510a-default`
- Schnittstellenprofil: `interface-medistar-nidek-ark510a-default`

## Datenformat

Das Geraet wird als Datei-/LAN-XML-Kandidat gefuehrt. Die Referenzstruktur nutzt ein `Data`-XML mit `Company`, `ModelName`, `VD`, `R/AR/ARMedian`, `L/AR/ARMedian` und `PD/PDList`.

## MEDISTAR-Ausgabe

- Autorefraktorwerte rechts/links: `6228`
- `8402` kommt aus der AIS-Datei
- Keine kuenstliche `6330`
- Keine kuenstlichen Trennzeichen

## Offene Punkte

- Echte Praxisdatei sammeln.
- MEDISTAR-Import praktisch pruefen.
- Danach optional offizielles Templatepaket nach Release-Regel ablegen.
