# E2E-Testprotokoll MEDISTAR + NIDEK RT-3100 RS232

Stand: 2026-05-29

Status: erster echter RS232-Praxismitschnitt als Parser-/Baukasten-Fixture validiert; Live-Senden und MEDISTAR-Import am echten Arbeitsplatz noch offen.

## Ziel

Dieses Protokoll dokumentiert den ersten echten RS232-Mitschnitt eines NIDEK RT-3100 und bleibt zugleich Vorlage fuer weitere Mitschnitte und Live-Abnahmen. Es prueft die serielle NIDEK-RT-Phoropterfamilie ohne echte Patientendaten.

## Vorbereiteter Softwarestand

- BuiltIn-Geraeteprofil: `device-nidek-rt3100-serial-default`
- BuiltIn-Exportprofil: `export-medistar-nidek-rt3100-serial-default`
- BuiltIn-Schnittstellenprofil: `interface-medistar-nidek-rt3100-serial-default`
- Parser: `NidekRtSerialPhoropterParser`
- Writer: `NidekRtSerialPhoropterOutputWriter`
- Presets: RT-3100 Type1 2400 7E2 und Type2 9600 8O1

## Testdaten

Vorhanden:

- echter RT-3100-RS232-Rohmitschnitt als Hex-Fixture `XdtDeviceBridge.Tests/TestData/Devices/Nidek/RS232/rt3100-final-prescription-practice-capture-202606xx.hex`
- bestaetigtes Format: `SH Header CR`, `STX @RT CR`, `STX Datenzeilen CR`, `EOT CR`

Noch offen:

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

- Parser erkennt `RT-3100`, Datum `2002/06/16`, Datenquelle `@RT`, Final Right/Left, ADD, VA, PD und WD.
- Aus dem Mitschnitt entstehen `6228`-Zeilen fuer den finalen Phoropter-Verordnungswert.
- Es entstehen keine `6227`-Zeilen, weil keine Subjective-Daten im Mitschnitt vorhanden sind.
- Es entstehen keine `6330`-Zeilen und keine kuenstliche Trennzeile.
- `WD40` wird als WorkingDistance diagnostisch erfasst und nicht als Vertex Distance in die MEDISTAR-Zeile umgedeutet.

## Offene Punkte

- weitere echte Mitschnitte, insbesondere Type2 und andere RT-Varianten
- DTR/DSR-Verhalten vor Ort pruefen
- PC-port-Parameter am Geraet pruefen
- Live-Senden an den Phoropter separat freigeben
- MEDISTAR-Import der erzeugten `6228`-Rueckgabe praktisch pruefen
