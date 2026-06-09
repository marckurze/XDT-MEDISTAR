# E2E-Testprotokoll MEDISTAR + NIDEK RT-3100 RS232

Stand: 2026-06-09

Status: echte RS232-Praxismitschnitte als Parser-/Baukasten-Fixtures validiert; der patientengetriggerte Produktivablauf mit Auswahlfenster, `RS senden, dann Writer ohne SD`, spaeterer RT-Rueckgabe, Mapping und MEDISTAR-XDT-Ausgabe ist am echten RT-3100-Arbeitsplatz bestaetigt. Der Livebefund zeigt, dass RT-3100 Type1 im Praxisaufbau DTR aktiv benoetigt. Die frueheren Sendetestbuttons sind aus der normalen Geraetekachel ausgeblendet. Nach erfolgreicher Rueckgabe/AIS-Ausgabe schliesst die Geraetekachel terminal und darf durch nachlaufende Monitoring-Refreshes nicht erneut erscheinen.

## Ziel

Dieses Protokoll dokumentiert den ersten echten RS232-Mitschnitt eines NIDEK RT-3100 und bleibt zugleich Vorlage fuer weitere Mitschnitte und Live-Abnahmen. Es prueft die serielle NIDEK-RT-Phoropterfamilie ohne echte Patientendaten.

## Vorbereiteter Softwarestand

- BuiltIn-Geraeteprofil: `device-nidek-rt3100-serial-default`
- BuiltIn-Exportprofil: `export-medistar-nidek-rt3100-serial-default`
- BuiltIn-Schnittstellenprofil: `interface-medistar-nidek-rt3100-serial-default`
- Parser: `NidekRtSerialPhoropterParser`
- Writer: `NidekRtSerialPhoropterOutputWriter`
- Kommunikation: `NidekRtSerialPhoropterCommunicationService`
- Live-Diagnose: RT-Floating-Fenster mit COM-Parametern, DTR/RTS/Handshake, RS-/Writer-Hexdump, single-instance Geraetekachel und `COM-Port nur abhoeren`
- Schnittstellenprofil-Sendemodus: `NidekRtSerialSendMode`, Default fuer RT-3100 `RsThenWriterWithoutSd`
- Schnittstellenprofil-Sendeinhalt: `NidekRtSerialOutputFrameVariant`, RT-3100-Default `RT-3100 Praxisvariante (getestet)`; dokumentnahe Varianten fuer Full, AR-only, LM-only, LM ohne ADD, ohne ID und Minimal rechts bleiben vorhanden
- Presets: RT-3100 Type1 2400 7E2 und Type2 9600 8O1

## Testdaten

Vorhanden:

- echter RT-3100-RS232-Rohmitschnitt als Hex-Fixture `XdtDeviceBridge.Tests/TestData/Devices/Nidek/RS232/rt3100-final-prescription-practice-capture-202606xx.hex`
- erfolgreicher RT-3100-`COM-Port nur abhoeren`-Mitschnitt mit DTR aktiv als Hex-Fixture `XdtDeviceBridge.Tests/TestData/Devices/Nidek/RS232/rt3100-final-prescription-dtr-listen-only-practice-capture-20260529.hex`
- weiterer RT-3100-`COM-Port nur abhoeren`-Mitschnitt mit DTR aktiv, ADD und VA als Hex-Fixture `XdtDeviceBridge.Tests/TestData/Devices/Nidek/RS232/rt3100-final-prescription-add-va-practice-capture-20260529.hex`
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
7. Im Schnittstellenprofil den `NIDEK-RT Sendemodus` pruefen. Praxisdefault fuer den bestaetigten RT-3100-Aufbau: `RS senden, dann Writer ohne SD`.
8. Im Schnittstellenprofil den `NIDEK-RT Sendeinhalt` pruefen. RT-3100-Praxisdefault: `RT-3100 Praxisvariante (getestet)`; bei Annahmeproblemen danach `Nur Lensmeter ohne ADD`, `Nur Lensmeter`, `Nur Autoref` und Varianten ohne ID testen.
9. `An RT-3100 senden` klicken.
10. XDTBox sendet RS als `01 43 2A 2A 02 52 53 17 04` (`SH C** SX RS EB ET`), wartet kurz und schreibt den Writer-Frame auch ohne SD. Diagnosefenster pruefen: `RT-3100: RS gesendet.`, `RT-3100: Writer-Frame ohne SD-Warten gesendet.`, Writer-Frame, Hexdump, CTS/DSR/DCD/RI und Status `Warte auf Rueckgabe vom RT-3100`.
11. Bei `RS/SD-Handshake` wartet XDTBox auf SD und schreibt den Writer-Frame nur bei SD. Dieser Modus bleibt fuer andere Installationen verfuegbar, ist aber nicht der RT-3100-Praxisdefault.
12. Wenn keine sofortige Rueckgabe kommt, bleibt der Vorgang wartend: Untersuchung am RT durchfuehren und danach PRINT/SEND ausloesen. XDTBox erzeugt ohne Rueckgabe kein leeres XDT.
13. `COM-Port nur abhoeren` bleibt Diagnose und erzeugt keinen Export. Fuer den produktiven Rueckweg im Wartestatus `Rueckgabe abhoeren und verarbeiten` starten; die empfangene Rueckgabe wird mit dem gespeicherten AIS-Kontext geparst und als MEDISTAR-XDT erzeugt.
14. Die frueheren Sendetestbuttons sind in der normalen Geraetekachel nicht sichtbar. Der funktionierende Modus laeuft automatisch im produktiven RT-3100-Sendeschritt.

## Ergebnis

- Parser erkennt `RT-3100`, Datum `2002/06/16`, Datenquelle `@RT`, Final Right/Left, ADD, VA, PD und WD.
- Aus dem Mitschnitt entstehen `6228`-Zeilen fuer den finalen Phoropter-Verordnungswert.
- Es entstehen keine `6227`-Zeilen, weil keine Subjective-Daten im Mitschnitt vorhanden sind.
- Es entstehen keine `6330`-Zeilen und keine kuenstliche Trennzeile.
- `WD40` wird als WorkingDistance diagnostisch erfasst und nicht als Vertex Distance in die MEDISTAR-Zeile umgedeutet.
- RS232-Scanhinweise beim Start der Ueberwachung werden als Information behandelt und oeffnen das RT-Fenster nicht vor Patienteneingang.
- Die Live-Diagnose macht sichtbar, ob der Profil-COM-Port geoeffnet wurde, welche DTR-/RTS-/Handshake-Werte gesetzt sind und ob Bytes vom RT eintreffen.
- Der Nur-Abhoeren-Livebefund zeigt: DTR aus fuehrte zu keiner Rueckgabe, DTR aktiv/RTS aktiv lieferte einen vollstaendigen 110-Byte-Frame mit Final-R/L, PD und WD ohne ADD.
- Der PC->RT-Livebefund zeigt: Der Pfad RS plus Writer ohne SD-Pflicht ist der bestaetigte RT-3100-Praxispfad; RS/SD lieferte keine SD-Bestaetigung.
- Der Sendemodus wird im Schnittstellenprofil gespeichert. `RsThenWriterWithoutSd` ist fuer RT-3100 der BuiltIn-Default; alte Diagnosemodi verhindern den produktiven RT-3100-Praxispfad nicht.
- Die RS-Anforderung sendet echte ASCII-Sternchen `2A 2A`, passend zu den ausgewerteten produktiven RT-Anbindungen.
- Der Writer sendet LM-SCA-Augenpraefixe als Leerzeichen + `R`/`L` (`20 52`, `20 4C`) und nicht als ASCII-Sternchen.
- Die dokumentnahe PC->RT-Variante sendet LM ADD laut Herstellerformat als `AR`/`AL` und beendet den Nutzdatenrahmen mit `EB ET`.
- Die RT-3100-Praxisvariante reproduziert den live angenommenen Direct-Writer-Frame exakt: 107 Bytes, Legacy-ADD `RA`/`LA` im bekannten Fall `LA+01.50`, kein zusaetzliches `EB` direkt vor `ET`.
- Fuer die naechste Live-Abnahme sind speicherbare Sendeinhaltsvarianten vorhanden: RT-3100 Praxisvariante, alle Werte, AR-only, LM-only, LM ohne ADD, ohne ID und Minimal rechts.
- Modemstatussignale CTS, DSR, DCD und RI werden in der seriellen Diagnose protokolliert, soweit der Adapter sie liefert.
- Nach erfolgreichem Senden ohne sofortige Rueckgabe bleibt der Workflow im Wartestatus; kein leerer Export und kein harter Sendefehler.
- Der wartende Workflow haelt den AIS-Patientenkontext. `Rueckgabe abhoeren und verarbeiten` nutzt diesen Kontext fuer den spaeteren produktiven Export, waehrend `COM-Port nur abhoeren` weiterhin nur Mitschnittdiagnose ist.
- Der Praxislauf vom 2026-06-09 bestaetigt den vollstaendigen RT-3100-Zyklus: AIS-Datei empfangen, Auswahlfenster geoeffnet, Werte gesendet, RT-Rueckgabe empfangen, XDT-Ausgabe an AIS erzeugt und Geraetekachel geschlossen.
- Nach terminalem Erfolg wird der RT-Workflow als abgeschlossen markiert, das Floating-Fenster gedockt/geschlossen und ein erneutes Oeffnen durch Success-/Refresh-/Monitoring-Nachlauf unterdrueckt. Ein neuer Zyklus beginnt erst mit einer neuen AIS-Patientendatei und wieder zuerst mit dem Auswahlfenster.

## Offene Punkte

- weitere echte Mitschnitte, insbesondere Type2 und andere RT-Varianten
- DTR/DSR-/RTS-/Handshake-Verhalten vor Ort weiter pruefen; DTR aktiv ist fuer den getesteten RT-3100-Type1-Aufbau aktuell der bestaetigte Startpunkt
- PC-port-Parameter am Geraet pruefen
- PC->RT-Live-Senden mit `RS senden, dann Writer ohne SD` ist als RT-3100-Praxispfad gesetzt; RS/SD weiter nur bei Bedarf als alternativer Schnittstellenprofilmodus pruefen
- weitere RT-3100-Varianten wie Type2, alternative Sendeinhalte und Subjective-/Zusatzdaten sammeln
- RT-2100 und RT-5100 mit identischem Ablauf praktisch einzeln abnehmen
