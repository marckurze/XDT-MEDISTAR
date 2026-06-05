# Templatepaket MEDISTAR + NIDEK LM-1800P

Status: vorbereitet aus Referenzdaten, praktische MEDISTAR-Abnahme offen.

## Inhalt

- AIS-Profil: MEDISTAR BuiltIn
- Geraeteprofil: `device-nidek-lm1800p-default`
- Exportprofil: `export-medistar-nidek-lm1800p-default`
- Schnittstellenprofil: `interface-medistar-nidek-lm1800p-default`

## Datenformat

Das Geraet wird als Datei-/LAN-XML-Kandidat gefuehrt. Die Referenzstruktur nutzt `Ophthalmology/Common` und `Measure Type="LM"`. Die LM-1800P-Augenbloecke `I` und `S` werden fuer die XDTBox-Vorschau auf rechts und links abgebildet. Die Schreibweise `Sphare` bleibt ueber die vorhandene Aliaslogik als `Sphere` nutzbar.

## MEDISTAR-Ausgabe

- Lensmeterwerte rechts/links: `6228`
- `8402` kommt aus der AIS-Datei
- Keine kuenstliche `6330`
- Keine kuenstlichen Trennzeichen

## Offene Punkte

- Echte LM-1800P- und LM-1800PD-Dateien sammeln.
- MEDISTAR-Import praktisch pruefen.
- Danach optional offizielles Templatepaket nach Release-Regel ablegen.
