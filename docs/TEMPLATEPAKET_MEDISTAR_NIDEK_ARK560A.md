# Templatepaket MEDISTAR + NIDEK ARK-560A

Status: vorbereitet aus Referenzdaten, praktische MEDISTAR-Abnahme offen.

## Inhalt

- AIS-Profil: MEDISTAR BuiltIn
- Geraeteprofil: `device-nidek-ark560a-default`
- Exportprofil: `export-medistar-nidek-ark560a-default`
- Schnittstellenprofil: `interface-medistar-nidek-ark560a-default`

## Datenformat

Das Geraet wird als Datei-/LAN-XML-Kandidat gefuehrt. Die Referenzstruktur entspricht der ARK-5xx-Familie mit `Data`-XML, `ARMedian`, `PDList` und `VD`.

## MEDISTAR-Ausgabe

- Autorefraktorwerte rechts/links: `6228`
- `8402` kommt aus der AIS-Datei
- Keine kuenstliche `6330`
- Keine kuenstlichen Trennzeichen

## Offene Punkte

- Echte Praxisdatei sammeln.
- MEDISTAR-Import praktisch pruefen.
- Danach optional offizielles Templatepaket nach Release-Regel ablegen.
