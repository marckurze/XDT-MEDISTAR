# E2E-Testprotokoll MEDISTAR + NIDEK RT-3100 RS232

Stand: 2026-05-29

Status: vorbereitet, noch keine Praxisabnahme.

## Ziel

Dieses Protokoll ist die Vorlage fuer den kommenden Praxis-Mitschnitt eines NIDEK RT-3100 oder eines kompatiblen RT-2100-Anbindungsfalls. Es prueft die serielle NIDEK-RT-Phoropterfamilie ohne echte Patientendaten.

## Vorbereiteter Softwarestand

- BuiltIn-Geraeteprofil: `device-nidek-rt3100-serial-default`
- BuiltIn-Exportprofil: `export-medistar-nidek-rt3100-serial-default`
- BuiltIn-Schnittstellenprofil: `interface-medistar-nidek-rt3100-serial-default`
- Parser: `NidekRtSerialPhoropterParser`
- Writer: `NidekRtSerialPhoropterOutputWriter`
- Presets: RT-3100 Type1 2400 7E2 und Type2 9600 8O1

## Testdaten

Noch offen:

- echter RS232-Rohmitschnitt Type1
- optional echter RS232-Rohmitschnitt Type2
- synthetische AIS-Datei ohne echte Patientendaten
- optional AIS-Historienwerte V0/V1 fuer PC->RT-Vorschau

## Erwartete MEDISTAR-Ausgabe

- `Final` -> `6228` Phoropter finaler Verordnungswert
- `Subjective` -> `6227` Phoropter Maximalwert (Vollkorrektion)
- keine `6330`
- keine Trennzeile
- `8402` aus AIS

## Praxisablauf

1. COM-Port und Type1/Type2 am RT-3100 pruefen.
2. Im XDT-Baukasten `MEDISTAR + NIDEK RT-3100 RS232` waehlen.
3. `COM Port abhoeren` starten und das passende Preset setzen.
4. Am RT-3100 Ausgabe an PC ausloesen.
5. Rohtext und Hexdump sichern.
6. Rohdaten in den Baukasten uebernehmen.
7. Vorschau erzeugen.
8. `Roh-XDT`, `Ansicht im AIS`, `Geraeteausgabe` und `Diagnose` pruefen.
9. Erst nach fachlicher Freigabe einen kontrollierten Live-Sendetest planen.

## Ergebnis

Noch offen.

## Offene Punkte

- echte Mitschnitte fehlen
- DTR/DSR-Verhalten vor Ort pruefen
- PC-port-Parameter am Geraet pruefen
- Live-Senden an den Phoropter separat freigeben
