# Templatepaket MEDISTAR + Huvitz HTR-1A

Stand: 2026-06-05

Status: Kandidat aus Referenzlogik, noch keine praktische MEDISTAR-Abnahme.

## Inhalt

- AIS-Profil: MEDISTAR
- Geraeteprofil: `device-huvitz-htr1a-default`
- Exportprofil: `export-medistar-huvitz-htr1a-default`
- Schnittstellenprofil: `interface-medistar-huvitz-htr1a-default`
- Anschluss: SerialRs232, 9600 Baud, 8 Datenbits, keine Paritaet, 1 Stoppbit
- Parser: `HuvitzText`

## Ausgabe

- REF rechts/links nach `6228`
- KM nach `6221`
- Tonometrie nach `6205`
- Pachymetrie nach `6220`
- keine automatische `6330`
- keine kuenstlichen Trennzeilen

Die aktuelle Testbasis ist eine synthetische Kombi-Textfixture aus Referenzlogik. Echte HTR-1A-Rohdaten bleiben fuer die Praxisabnahme erforderlich.
