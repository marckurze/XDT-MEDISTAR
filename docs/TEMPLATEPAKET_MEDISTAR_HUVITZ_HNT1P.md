# Templatepaket MEDISTAR + Huvitz HNT-1P

Stand: 2026-06-05

Status: Kandidat aus Referenzlogik, noch keine praktische MEDISTAR-Abnahme.

## Inhalt

- AIS-Profil: MEDISTAR
- Geraeteprofil: `device-huvitz-hnt1p-default`
- Exportprofil: `export-medistar-huvitz-hnt1p-default`
- Schnittstellenprofil: `interface-medistar-huvitz-hnt1p-default`
- Anschluss: SerialRs232, 115200 Baud, 8 Datenbits, keine Paritaet, 1 Stoppbit
- Parser: `HuvitzText`

## Ausgabe

- Tonometrie nach `6205`
- Pachymetrie nach `6220`
- keine automatische `6330`
- keine kuenstlichen Trennzeilen

Die aktuelle Testbasis ist eine synthetische Textfixture aus Referenzlogik. Echte HNT-1P-Rohdaten bleiben fuer die Praxisabnahme erforderlich.
