# E2E-Testprotokoll MEDISTAR + NIDEK RT-3100 RS232

Stand: 2026-05-29

Status: erster echter RS232-Praxismitschnitt als Parser-/Baukasten-Fixture validiert; patientengetriggerter Produktivablauf mit Auswahlfenster, COM-Senden und Empfang bis EOT ist technisch implementiert. Live-Senden, Rueckgabe nach Sendung und MEDISTAR-Import am echten Arbeitsplatz sind noch offen.

## Ziel

Dieses Protokoll dokumentiert den ersten echten RS232-Mitschnitt eines NIDEK RT-3100 und bleibt zugleich Vorlage fuer weitere Mitschnitte und Live-Abnahmen. Es prueft die serielle NIDEK-RT-Phoropterfamilie ohne echte Patientendaten.

## Vorbereiteter Softwarestand

- BuiltIn-Geraeteprofil: `device-nidek-rt3100-serial-default`
- BuiltIn-Exportprofil: `export-medistar-nidek-rt3100-serial-default`
- BuiltIn-Schnittstellenprofil: `interface-medistar-nidek-rt3100-serial-default`
- Parser: `NidekRtSerialPhoropterParser`
- Writer: `NidekRtSerialPhoropterOutputWriter`
- Kommunikation: `NidekRtSerialPhoropterCommunicationService`
- Live-Diagnose: RT-Floating-Fenster mit COM-Parametern, DTR/RTS/Handshake, RS/SD/Writer-Hexdump und `COM-Port nur abhoeren`
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

## Baukasten-Praxisablauf

1. COM-Port und Type1/Type2 am RT-3100 pruefen.
2. Im XDT-Baukasten `MEDISTAR + NIDEK RT-3100 RS232` waehlen.
3. `COM Port abhoeren` starten und das passende Preset setzen.
4. Am RT-3100 Ausgabe an PC ausloesen.
5. Rohtext und Hexdump sichern.
6. Rohdaten in den Baukasten uebernehmen.
7. Vorschau erzeugen.
8. `Roh-XDT`, `Ansicht im AIS`, `Geraeteausgabe` und `Diagnose` pruefen.
9. Erst nach fachlicher Freigabe einen kontrollierten Live-Sendetest planen.

## Produktiver Zielablauf

1. Aktives RT-3100-Schnittstellenprofil mit AIS-Importordner, Ergebnisordner und COM-Port konfigurieren; ein Geraete-Eingangsordner ist fuer RT-3100 nicht erforderlich.
2. Ueberwachung starten; das RT-Fenster oeffnet dabei noch nicht.
3. AIS-Patientendatei ablegen.
4. XDTBox liest Patient und Historie und oeffnet den RT-Auswahldialog.
5. V0/Lensmeter und/oder V1/Autorefraktion auswaehlen.
6. Optional im RT-Fenster `COM-Port nur abhoeren` testen: Profil-Port und Profil-Parameter werden verwendet, es wird nichts gesendet und kein Export erzeugt.
7. `An RT-3100 senden` klicken.
8. XDTBox sendet RS, wartet auf SD, schreibt den PC->RT-Frame und wartet danach auf die RT-Rueckgabe bis EOT plus Stabilitaetswartezeit. Diagnosefenster pruefen: RS-Hexdump, SD-Antwort, Writer-Frame, Empfangs-Hexdump.
9. Die Rueckgabe wird geparst und als MEDISTAR-XDT erzeugt.

## Ergebnis

- Parser erkennt `RT-3100`, Datum `2002/06/16`, Datenquelle `@RT`, Final Right/Left, ADD, VA, PD und WD.
- Aus dem Mitschnitt entstehen `6228`-Zeilen fuer den finalen Phoropter-Verordnungswert.
- Es entstehen keine `6227`-Zeilen, weil keine Subjective-Daten im Mitschnitt vorhanden sind.
- Es entstehen keine `6330`-Zeilen und keine kuenstliche Trennzeile.
- `WD40` wird als WorkingDistance diagnostisch erfasst und nicht als Vertex Distance in die MEDISTAR-Zeile umgedeutet.
- RS232-Scanhinweise beim Start der Ueberwachung werden als Information behandelt und oeffnen das RT-Fenster nicht vor Patienteneingang.
- Die Live-Diagnose macht sichtbar, ob der Profil-COM-Port geoeffnet wurde, welche DTR-/RTS-/Handshake-Werte gesetzt sind und ob Bytes vom RT eintreffen.

## Offene Punkte

- weitere echte Mitschnitte, insbesondere Type2 und andere RT-Varianten
- DTR/DSR-/RTS-/Handshake-Verhalten vor Ort pruefen
- PC-port-Parameter am Geraet pruefen
- Live-Senden an den Phoropter und die echte Rueckgabe nach Sendung separat freigeben
- MEDISTAR-Import der erzeugten `6228`-Rueckgabe praktisch pruefen
