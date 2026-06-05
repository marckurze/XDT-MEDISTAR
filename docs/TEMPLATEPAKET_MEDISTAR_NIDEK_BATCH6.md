# Templatepakete MEDISTAR + NIDEK Batch 6

Dieser Sammelvermerk beschreibt die testseitig abgesicherten Templatepaket-Kandidaten fuer die NIDEK-Restfamilie aus Batch 6.

Die Profile sind als Startkonfigurationen gedacht. Sie bleiben inaktiv, enthalten keine Praxisordner, keine Patientendaten und keine Referenzpaket-Rohdaten. Die praktische MEDISTAR-Abnahme mit echten Dateien steht noch aus.

## Kandidaten

| Templatepaket | Geraeteprofil | Exportprofil | Schnittstellenprofil | Ausgabe |
| --- | --- | --- | --- | --- |
| MEDISTAR + NIDEK AR-1 | `device-nidek-ar1-default` | `export-medistar-nidek-ar1-default` | `interface-medistar-nidek-ar1-default` | REF nach `6228` |
| MEDISTAR + NIDEK AR-1S | `device-nidek-ar1s-default` | `export-medistar-nidek-ar1s-default` | `interface-medistar-nidek-ar1s-default` | REF nach `6228`, subjektive Refraktion nach `6227` |
| MEDISTAR + NIDEK AR-310A | `device-nidek-ar310a-default` | `export-medistar-nidek-ar310a-default` | `interface-medistar-nidek-ar310a-default` | REF nach `6228` |
| MEDISTAR + NIDEK LM-1800PD | `device-nidek-lm1800pd-default` | `export-medistar-nidek-lm1800pd-default` | `interface-medistar-nidek-lm1800pd-default` | Lensmeter nach `6228` |
| MEDISTAR + NIDEK NT-1 | `device-nidek-nt1-default` | `export-medistar-nidek-nt1-default` | `interface-medistar-nidek-nt1-default` | Tonometrie nach `6205`, Pachymetrie nach `6220` |
| MEDISTAR + NIDEK NT-1E | `device-nidek-nt1e-default` | `export-medistar-nidek-nt1e-default` | `interface-medistar-nidek-nt1e-default` | Tonometrie nach `6205`, Pachymetrie nach `6220` |
| MEDISTAR + NIDEK NT-1P | `device-nidek-nt1p-default` | `export-medistar-nidek-nt1p-default` | `interface-medistar-nidek-nt1p-default` | Tonometrie nach `6205`, Pachymetrie nach `6220` |
| MEDISTAR + NIDEK NT-510 | `device-nidek-nt510-default` | `export-medistar-nidek-nt510-default` | `interface-medistar-nidek-nt510-default` | Tonometrie nach `6205`, Pachymetrie nach `6220` |
| MEDISTAR + NIDEK NT-530 | `device-nidek-nt530-default` | `export-medistar-nidek-nt530-default` | `interface-medistar-nidek-nt530-default` | Tonometrie nach `6205`, Pachymetrie nach `6220` |

## Technische Grundlage

- AR-1, AR-1S und AR-310A nutzen die vorhandene NIDEK-AR-XML-Familie mit `ARMedian`.
- AR-1S ergaenzt `SR` als subjektive Refraktion nach `6227`.
- LM-1800PD nutzt die vorhandene NIDEK-Lensmeter-XML-Familie.
- NT-1, NT-1E, NT-1P, NT-510 und NT-530 nutzen die NT530P-nahe XML-Auswertung fuer Tonometrie und Pachymetrie.
- Fehlende Werte erzeugen keine kuenstlichen Zeilen.
- Es wird kein `6330` und kein kuenstlicher Trenner erzeugt.

## Tests

`NidekReferenceBatch6ProfileTests` prueft synthetische neutrale XML-Fixtures fuer Parser und MEDISTAR-Ausgabe. `MedistarNidekReferenceBatch6TemplatePackageTests` prueft den selektiven Templatepaket-Export, die ZIP-Struktur, inaktive Schnittstellenprofile und dass keine Live-Pfade oder Referenzartefakte in die Pakete gelangen.

## Offene Punkte

- Echte Praxisdateien je Modell sammeln.
- MEDISTAR-Import praktisch validieren.
- CEM-530, TONOREF II/III sowie RDD-/COM-nahe Restgeraete bleiben separate Folgeentscheidungen.
