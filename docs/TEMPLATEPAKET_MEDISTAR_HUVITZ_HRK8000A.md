# Templatepaket MEDISTAR + Huvitz HRK-8000A

Stand: 2026-06-05

Status: Kandidat aus Referenzlogik, noch keine praktische MEDISTAR-Abnahme.

## Inhalt

- AIS-Profil: MEDISTAR
- Geraeteprofil: `device-huvitz-hrk8000a-default`
- Exportprofil: `export-medistar-huvitz-hrk8000a-default`
- Schnittstellenprofil: `interface-medistar-huvitz-hrk8000a-default`
- Anschluss: SerialRs232, 9600 Baud, 8 Datenbits, keine Paritaet, 1 Stoppbit
- Parser: `HuvitzText`

## Ausgabe

- REF rechts/links nach `6228`
- KM nach `6221`
- keine automatische `6330`
- keine kuenstlichen Trennzeilen

Die aktuelle Testbasis ist eine synthetische Textfixture aus Referenzlogik. Echte HRK-8000A-Rohdaten bleiben fuer die Praxisabnahme erforderlich.
