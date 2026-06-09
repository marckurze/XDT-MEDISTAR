# NIDEK RT-2100 / RT-3100 / RT-5100 RS232 Protokollnotizen

Stand: 2026-05-29

Diese Notizen fassen die Auswertung der bereitgestellten Herstellerdokumente zusammen:

- `RT2100_Interface_MRT8  RTZ001FE_.pdf`
- `RT-3100(RT-11)_IntE_34090-P982-A2.pdf`
- `RT-5100_IntME_34085-P992-A0.pdf`

Die Auswertung dient Parser, Writer, BuiltIn-Profilen, XDT-Baukasten-Vorschau und dem produktiven patientengetriggerten RS232-Ablauf. Am 2026-05-29 wurden echte RT-3100-Praxismitschnitte als Fixtures uebernommen. Der Live-Test bestaetigt ausserdem, dass der direkte PC->RT-Writer-Frame vom RT-3100 empfangen wird; der RS/SD-Handshake lieferte in dieser Praxisinstallation keine SD-Antwort. Rueckgabe nach Sendung und MEDISTAR-Import muessen weiterhin praktisch freigegeben werden.

## Gemeinsame Familie

- Geraeteklasse: NIDEK-RT-Phoropter
- Verbindung: RS-232C, DIN 8-pin, ASCII, halbduplex
- Steuerzeichen: `SH`, `SX`, `EB`, `ET`, `CR`
- Aktuelle Zuordnung:
  - `SH` = SOH = `0x01`
  - `SX` = STX = `0x02`
  - `EB` = ETB = `0x17`
  - `ET` = EOT = `0x04`
  - `CR` = `0x0D`
- Datenquellen: `LM`, `RM`, `RT`, `KM`, `NT`; bei RT-3100/RT-5100 zusaetzlich `WF`.
- Gross-/Kleinschreibung wird als Tag-/Nacht- beziehungsweise Variantenhinweis behandelt, aber nicht blind medizinisch umgedeutet.

## Kommunikationsparameter

| Geraet | Preset | Baud | Datenbits | Paritaet | Stopbits | Hinweis |
| --- | --- | ---: | ---: | --- | ---: | --- |
| RT-2100 | Type 1 | 2400 | 7 | Even | 2 | konservativer Standard, DTR/DSR relevant |
| RT-3100 | Type 1 | 2400 | 7 | Even | 2 | gilt, wenn kein Type2-Parameter gesetzt ist |
| RT-3100 | Type 2 | 9600 | 8 | Odd | 1 | PC port parameter muss auf `PC` stehen |
| RT-5100 | Type 1 | 2400 | 7 | Even | 2 | konservativer Standard |
| RT-5100 | Type 2 | 9600 | 8 | Odd | 1 | direkter Output ohne erste DTR/DSR-Schritte moeglich |

Die RT-Type1-/Type2-Presets aktivieren DTR und RTS in XDTBox standardmaessig, weil die Handbuecher DTR/DSR-Sequenzen zeigen und der Praxisaufbau sonst je nach Adapter keine Daten liefert. Der RT-3100-Livebefund vom 2026-05-29 bestaetigt: Mit DTR aus wurde keine Rueckgabe empfangen, mit DTR aktiv/RTS aktiv kam ein vollstaendiger RT-3100-Frame. DTR, RTS und Handshake bleiben im Schnittstellenprofil und im RS232-Testbereich sichtbar steuerbar.

## RT -> PC

Der Parser `NidekRtSerialPhoropterParser` ist tolerant gegen Standard- und Expanded-Header:

- RT-2100: `SH NIDEK RT-2100 ... DAYYYY/MM/DD CR`
- RT-3100: Standard-/Kompatibilitaetszeilen und Expanded-Header `SH NIDEK_RT-3100 CR`, danach `SX ID... CR` und `SX DAYYYY/MM/DD_SN CR`
- RT-5100: Standard `NIDEK_RT-5100 ...` und Expanded-Header analog RT-3100

Refraktionsdaten werden zuerst fuer SCA/ADD/PD sicher ausgewertet. Der erste echte RT-3100-Mitschnitt bestaetigt zudem VA (`VR`/`VL`) und WorkingDistance (`WD`) als diagnostisch nutzbare Werte. Plus-Package-/KM-/NT-Details bleiben je nach Erkennung roh oder diagnostisch, bis echte Praxisdaten vorliegen.

## Bestaetigter RT-3100-Praxismitschnitt

Fixture:

`XdtDeviceBridge.Tests/TestData/Devices/Nidek/RS232/rt3100-final-prescription-practice-capture-202606xx.hex`

Zusaetzliches Nur-Abhoeren-Fixture mit DTR aktiv:

`XdtDeviceBridge.Tests/TestData/Devices/Nidek/RS232/rt3100-final-prescription-dtr-listen-only-practice-capture-20260529.hex`

Zusaetzliches Nur-Abhoeren-Fixture mit DTR aktiv, ADD und VA:

`XdtDeviceBridge.Tests/TestData/Devices/Nidek/RS232/rt3100-final-prescription-add-va-practice-capture-20260529.hex`

Bestaetigtes Frameformat:

- `SH` Header `CR`
- `SX @RT CR`
- je Datenzeile `SX <Daten> CR`
- `ET CR`
- kein `EB`/ETB zwischen den Datenzeilen

Erkannte Inhalte:

- Header: `NIDEK RT-3100 ID             DA2002/06/16`
- Modell: `RT-3100`
- Datum: `2002-06-16`
- ID: leer beziehungsweise nur Spaces
- Datenquelle: `@RT` / Refractor
- `FR- 1.50- 0.75180`: Final Right, S=-1.50, Z=-0.75, Achse 180
- `FL- 1.50- 1.50175`: Final Left, S=-1.50, Z=-1.50, Achse 175
- `AR+ 0.75`, `AL+ 1.25`: ADD rechts/links
- `VR 0.1`, `VL 1.25`: VA rechts/links, diagnostisch
- `PD64.0`: binokulare PD 64.0
- `WD40`: WorkingDistance 40, diagnostisch; nicht als Vertex Distance umgedeutet

Das DTR-Nur-Abhoeren-Fixture enthaelt dieselbe Struktur, aber ohne ADD/VA:

- `FR- 1.25  0.00  0`: Final Right, S=-1.25, Z=0.00, Achse 0
- `FL- 1.25  0.00  0`: Final Left, S=-1.25, Z=0.00, Achse 0
- `PD64.0`: binokulare PD 64.0
- `WD40`: WorkingDistance 40

Das ADD-/VA-Nur-Abhoeren-Fixture enthaelt:

- `FR- 2.25- 1.25180`: Final Right, S=-2.25, Z=-1.25, Achse 180
- `FL- 2.25- 1.00180`: Final Left, S=-2.25, Z=-1.00, Achse 180
- `AR+ 0.50`, `AL+ 0.75`: ADD rechts/links
- `VR 0.05`, `VL 1.6`: VA rechts/links, diagnostisch
- `PD64.0`: binokulare PD 64.0
- `WD40`: WorkingDistance 40

MEDISTAR-Mapping fuer diesen Mitschnitt:

- nur `Final` vorhanden -> nur `6228`
- Header: `Phoropter finaler Verordnungswert`
- R/L-6228-Zeilen enthalten S, Z, Achse, ADD und PD
- keine `6227`, keine `6330`, keine kuenstliche Trennzeile
- `8402` kommt weiterhin aus AIS/MEDISTAR

MEDISTAR-Mapping:

- `Final` / finale Prescription -> `6228` mit Header `Phoropter finaler Verordnungswert`
- `Subjective` -> `6227` mit Header `Phoropter Maximalwert (Vollkorrektion)`
- keine `6330`
- keine kuenstliche Trennzeile
- `8402` kommt aus AIS/MEDISTAR

## PC -> RT

Der Writer `NidekRtSerialPhoropterOutputWriter` erzeugt Frames fuer die Uebergabe von vorhandenen AIS-Historienwerten an das Geraet; im Baukasten bleiben sie Vorschau, im Produktivdialog werden sie erst nach Anwenderklick gesendet.

Unterstuetzt in V1:

- AR SCA
- AR PD
- LM SCA
- LM ADD
- LM PD

RT-2100 nutzt keinen ID-Block. RT-3100 und RT-5100 koennen einen ID-Block enthalten. Fehlende Werte werden weggelassen; es werden keine leeren medizinischen Bloecke erfunden. Prisma wird erst nach echter Datenlage aktiviert.

Herstellerabgleich PC->RT, insbesondere RT-3100-PDF Abschnitt 6:

- Gesamtformat: `SH ID No. block EB AR SCA block EB AR PD block EB LM SCA block EB LM ADD block EB LM PRISM block EB LM PD block EB ET`; nicht vorhandene Bloecke werden entfernt.
- LM SCA: `DLM SX  R... EB  L...`; die im Diagramm gezeichneten `*R`/`*L` bedeuten Leerzeichen + Auge und werden als `20 52`/`20 4C` gesendet.
- Dokumentnahe LM ADD: rechts `AR+..`, links `AL+..`; es gibt keinen `ALM`-Header.
- XDTBox kann den erzeugten Nutzdatenrahmen dokumentnah mit `EB ET` beenden; die optionale Testfunktion `CR nach EOT` haengt nur fuer Live-Diagnosen zusaetzlich `CR` an.
- RT-3100-Praxisvariante: Der live angenommene Direct-Writer-Frame nutzte Legacy-ADD `RA`/`LA` und kein zusaetzliches `EB` direkt vor `ET`. Diese Bytefolge bleibt als eigene Variante erhalten, weil sie am echten RT-3100 bestaetigt wurde.

RS-Anforderung:

- Die ausgewerteten produktiven RT-Anbindungen senden fuer `RS` echte ASCII-Sternchen.
- XDTBox sendet fuer `RS` daher `SH C** SX RS EB ET`.
- Hexdump: `01 43 2A 2A 02 52 53 17 04`.
- Die aeltere Diagnose mit Leerzeichen (`01 43 20 20 20 02 52 53 17 04`) bleibt als historischer Irrtum dokumentiert, wird aber nicht mehr als Default gesendet.
- Auch in LM-SCA-Bloecken sind die gezeichneten Sternchen Platzhalter: XDTBox sendet `DLM SX  R... EB  L...` mit `20 52` und `20 4C`, nicht `2A 52`/`2A 4C`.
- Dokumentnahe LM-ADD-Bloecke werden als `AR`/`AL` erzeugt. Zusaetzlich gibt es die Praxisvariante `RT-3100 Praxisvariante (getestet)`, die den zuvor live angenommenen Frame exakt reproduziert: Legacy-ADD `RA`/`LA`, im bekannten Testfall `LA+01.50`, Laenge 107 Bytes, kein `EB` direkt vor `ET`.
- Die Diagnose nennt erkannte Writer-Bloecke wie ID, AR SCA, AR PD, LM SCA, LM ADD, LM PD und Prism anhand des erzeugten Frames.

Serielle Diagnose im RT-Fenster:

- Die normale Praxis-Geraetekachel zeigt keine eigenen Sendetestbuttons mehr.
- Der zuvor erfolgreiche Pfad `RS + Writer ohne SD-Warten` wird fuer den produktiven RT-3100-Button automatisch verwendet.
- Die Diagnose protokolliert CTS, DSR, DCD und RI, soweit die Windows-API sie fuer den Adapter liefert.

Der erfolgreiche Praxispfad sendet zuerst `RS`, wartet nur kurz auf moegliche Antwortbytes und schreibt den Writer-Frame danach ohne harte SD-Pflicht. Deshalb ist der produktive Sendemodus nicht im Geraeteprofil fest verdrahtet, sondern pro Schnittstellenprofil gespeichert:

- `RS/SD-Handshake`: `RS` senden, `SD` erwarten, danach Writer-Frame senden.
- `RS senden, dann Writer ohne SD`: `RS` senden, kurz warten und den Writer-Frame auch ohne `SD` senden.
- `Direkt Writer-Frame senden`: keinen `RS` senden, keine `SD`-Bestaetigung erwarten, Writer-Frame direkt senden.

Die BuiltIn-Schnittstellenprofile fuer RT-2100/RT-3100/RT-5100 verwenden `RS senden, dann Writer ohne SD` als Default. Fuer RT-3100 setzt XDTBox diesen Praxispfad im produktiven Ablauf auch dann durch, wenn ein aelteres Profil noch einen frueheren Diagnosemodus gespeichert hat.

Der zusaetzliche Schnittstellenprofilwert `NIDEK-RT Sendeinhalt` steuert, welche PC->RT-Bloecke beziehungsweise welche Frameform produktiv in den Writer-Frame kommen:

- `Alle ausgewaehlten Werte`: ID, AR, LM, ADD/PD soweit vorhanden.
- `RT-Referenz ohne ID (AR/AL)`: AR und LM ohne `DRL`-ID-Block, LM ADD als `AR`/`AL`, Abschluss `EB ET`; aktueller BuiltIn-Default fuer RT-2100/3100/5100.
- `RT-3100 Praxisvariante (getestet)`: reproduziert den in der Praxis angenommenen Direct-Writer-Frame mit Legacy-ADD `RA`/`LA` und ohne zusaetzliches `EB` direkt vor `ET`.
- `Nur Autoref`: AR SCA und ggf. AR PD.
- `Nur Lensmeter`: LM SCA, LM ADD und ggf. LM PD.
- `Nur Lensmeter ohne ADD`: LM SCA und ggf. LM PD; wichtig, falls das Praxisgeraet den ADD-Block nicht akzeptiert.
- Varianten `ohne ID`: gleiche Nutzdaten, aber ohne `DRL`-ID-Block.
- `Minimal rechts`: konservativer rechter LM-Testframe.

Im Diagnosebereich koennen diese Varianten unabhaengig vom gespeicherten Profilwert ausprobiert werden. Empfohlene Reihenfolge fuer den naechsten Live-Test, wenn ein Vollframe nicht uebernommen wird: zuerst `RT-3100 Praxisvariante (getestet)`, danach `Nur Lensmeter ohne ADD`, `Nur Lensmeter`, `Nur Autoref`, Varianten ohne ID, optional `CR nach EOT`.

## Produktiver Ablauf in XDTBox

- Beim Start der Ueberwachung wird kein RT-Phoropterfenster geoeffnet.
- Erst eine stabile AIS-Patientendatei startet den Auswahl-/Sendedialog.
- Der Dialog bietet LM-/AR-Historienwerte an; produktiv gesendet werden zunaechst V0/Lensmeter und V1/Autorefraktion.
- Senden erfolgt nur nach ausdruecklichem Anwenderklick ueber den im Schnittstellenprofil konfigurierten COM-Port.
- Der Schnittstellenprofilwert `NIDEK-RT Sendemodus` steuert den produktiven Ablauf. Im RT-3100-Praxisdefault `RS senden, dann Writer ohne SD` sendet XDTBox `SH C** SX RS EB ET` (`01 43 2A 2A 02 52 53 17 04`), wartet kurz und schreibt den Writer-Frame auch ohne `SD`. In `RS/SD-Handshake` erwartet XDTBox `SX SD` und schreibt erst danach den PC->RT-Frame. In `Direkt Writer-Frame senden` wird der Writer-Frame ohne vorheriges `RS` geschrieben.
- Der Schnittstellenprofilwert `NIDEK-RT Sendeinhalt` bestimmt den produktiven Writer-Inhalt. Fuer RT-2100/RT-3100/RT-5100-BuiltIns ist `RT-Referenz ohne ID (AR/AL)` der Default; die RT-3100 Praxisvariante bleibt als separate Legacy-Testform verfuegbar.
- Nach dem Oeffnen des COM-Ports wartet XDTBox kurz, damit DTR/RTS und der RT-Eingang stabil sind. Nach dem Schreiben des Writer-Frames bleibt der COM-Port noch fuer einen baudratenabhaengigen Sendenachlauf offen. Bei 107 Bytes und RT-3100 Type1 `2400/7E2` sind das rund 0,8 Sekunden, damit der Frame nicht nur in den Windows-Treiber geschrieben, sondern auch auf der seriellen Leitung ausgesendet werden kann.
- Nach erfolgreichem Senden wechselt XDTBox in `Warte auf Rueckgabe vom Phoropter`. Eine ausbleibende sofortige Rueckgabe ist kein Sendefehler: Der Anwender fuehrt die Untersuchung am RT durch und loest danach PRINT/SEND aus. Ohne Rueckgabe wird kein leeres XDT erzeugt.
- Sobald eine Rueckgabe empfangen wird, wird sie bis `ET`/EOT gesammelt; danach wartet XDTBox eine kurze Stabilitaetszeit, bevor geparst und exportiert wird.
- Serielle RT-Schnittstellenprofile brauchen keinen Geraete-Eingangsordner und keinen dateibasierten Geraete-Ausgabeordner.
- Das RT-Floating-Fenster enthaelt fuer Live-Abnahmen eine serielle Diagnose: verwendete COM-Parameter, DTR/RTS/Handshake, Port-Status, RS-Anforderung, PC->RT-Writer-Frame, Hexdump und sichtbare Steuerzeichen werden angezeigt. `COM-Port nur abhoeren` oeffnet denselben Profil-Port, sendet nichts und zeigt empfangene Bytes ohne XDT-Export.
- Fuer den wartenden Produktivzustand nach einer Sendung gibt es zusaetzlich `Rueckgabe abhoeren und verarbeiten`. Diese Funktion sendet nichts, empfaengt aber die spaetere RT-Rueckgabe produktiv, verwendet den gespeicherten AIS-Patientenkontext des Pending-Workflows und erzeugt erst bei gueltiger Rueckgabe die MEDISTAR-XDT-Ausgabe. `COM-Port nur abhoeren` bleibt reine Diagnose und exportiert nicht.
- Die frueheren RT-Sendetestbuttons sind aus der normalen Praxis-Geraetekachel entfernt. Der funktionierende RS+Writer-ohne-SD-Pfad laeuft automatisch ueber `An RT-3100 senden`.
- Wenn im `RS/SD-Handshake` keine SD-Bestaetigung eintrifft, zeigt XDTBox eine konkrete Pruefliste zu COM-Port, Type1/Type2, PC-Port-Parameter am RT, DTR/RTS/Handshake und Portbelegung. Im RT-3100-Praxispfad `RS senden, dann Writer ohne SD` gibt es keine harte SD-Fehlermeldung; dort zeigt XDTBox `RS gesendet`, `Writer-Frame ohne SD-Warten gesendet` und den anschliessenden Wartestatus auf PRINT/SEND-Rueckgabe.

## XDT-Baukasten

Im XDT-Baukasten zeigt die Ansicht `Geraeteausgabe` den sichtbaren Steuerzeichen-Text und einen Hexdump. Die Vorschau schreibt keine produktive Datei und sendet nichts an einen COM-Port.

Warnhinweis im Baukasten:

`NIDEK RT-2100/3100/5100 RS232 ist vorbereitet. Bitte echte Praxis-Mitschnitte pruefen, bevor produktiv gesendet wird.`

## Offene Praxispunkte

- weitere echte RT-2100-/RT-3100-/RT-5100-RS232-Mitschnitte
- Pruefung, welche Header-Variante das konkrete Praxisgeraet sendet
- Pruefung von Type1/Type2 und DTR/DSR-Verhalten
- weitere Live-Abnahmen des PC->RT-Sendeframes an RT-2100 und RT-5100; am RT-3100 ist der Pfad `RS senden, dann Writer ohne SD` als produktiver Praxispfad gesetzt
- Rueckgabe nach einer echten Sendung am RT-3100/RT-2100/RT-5100 mit `Rueckgabe abhoeren und verarbeiten` praktisch freigeben
- MEDISTAR-Abnahme der produktiv erzeugten `6228`-/`6227`-Rueckgabe
