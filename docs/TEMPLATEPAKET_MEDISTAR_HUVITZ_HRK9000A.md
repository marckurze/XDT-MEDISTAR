# Templatepaket MEDISTAR + Huvitz HRK-9000A

Stand: 2026-06-05

Status: Kandidat aus Referenzlogik, noch keine praktische MEDISTAR-Abnahme.

## Inhalt

- AIS-Profil: MEDISTAR
- Geraeteprofil: `device-huvitz-hrk9000a-default`
- Exportprofil: `export-medistar-huvitz-hrk9000a-default`
- Schnittstellenprofil: `interface-medistar-huvitz-hrk9000a-default`
- Anschluss: SerialRs232, 9600 Baud, 8 Datenbits, keine Paritaet, 1 Stoppbit
- Parser: `HuvitzText`

## Ausgabe

- REF rechts/links nach `6228`
- KM nach `6221`
- keine automatische `6330`
- keine kuenstlichen Trennzeilen

Die aktuelle Testbasis folgt derselben Huvitz-REF/KM-Textfamilie wie HRK-8000A. Echte HRK-9000A-Rohdaten bleiben fuer die Praxisabnahme erforderlich.
