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

RS-Anforderung:

- Die in den NIDEK-Formatdiagrammen dargestellten `*` sind Leerzeichen-Platzhalter und werden nicht als ASCII-Sternchen gesendet.
- XDTBox sendet fuer `RS` daher `SH C   SX RS EB ET`.
- Hexdump: `01 43 20 20 20 02 52 53 17 04`.
- Die alte Diagnose `<SOH>C **<STX>RS<ETB><EOT>` war irrefuehrend, weil sie echte `2A 2A`-Bytes beschrieb.
- Auch in LM-SCA-Bloecken sind die gezeichneten Sternchen Platzhalter: XDTBox sendet `DLM SX  R... EB  L...` mit `20 52` und `20 4C`, nicht `2A 52`/`2A 4C`.
- Die Diagnose nennt erkannte Writer-Bloecke wie ID, AR SCA, AR PD, LM SCA, LM ADD, LM PD und Prism anhand des erzeugten Frames.

Live-Sendetestmodi im RT-Fenster:

- `RS anfordern`: sendet nur `SH C   SX RS EB ET`, wartet auf `SD`, sendet keinen Writer-Frame.
- `DTR-Toggle + RS`: setzt DTR kurz zurueck, aktiviert DTR wieder und sendet danach `RS`.
- `Direkt Writer-Frame senden`: sendet den PC->RT-Frame ohne RS/SD, nur nach Warnbestaetigung.
- `RS + Writer ohne SD-Warten`: sendet `RS`, wartet kurz und sendet den Writer-Frame auch ohne SD, nur nach Warnbestaetigung.

Die serielle Diagnose protokolliert zusaetzlich CTS, DSR, DCD und RI, soweit die Windows-API sie fuer den Adapter liefert. Der direkte Writer-Frame wurde am RT-3100 live empfangen. Deshalb ist der produktive Sendemodus nicht im Geraeteprofil fest verdrahtet, sondern pro Schnittstellenprofil gespeichert:

- `RS/SD-Handshake`: `RS` senden, `SD` erwarten, danach Writer-Frame senden.
- `Direkt Writer-Frame senden`: keinen `RS` senden, keine `SD`-Bestaetigung erwarten, Writer-Frame direkt senden.
- `RS senden, dann Writer ohne SD`: `RS` senden, kurz warten und den Writer-Frame auch ohne `SD` senden.

Die BuiltIn-Schnittstellenprofile fuer RT-2100/RT-3100/RT-5100 verwenden `Direkt Writer-Frame senden` als Default, weil dieser Modus am RT-3100 praktisch bestaetigt wurde. Die Sendetestmodi im RT-Fenster bleiben davon getrennt und aendern den gespeicherten Schnittstellenprofilwert nicht.

## Produktiver Ablauf in XDTBox

- Beim Start der Ueberwachung wird kein RT-Phoropterfenster geoeffnet.
- Erst eine stabile AIS-Patientendatei startet den Auswahl-/Sendedialog.
- Der Dialog bietet LM-/AR-Historienwerte an; produktiv gesendet werden zunaechst V0/Lensmeter und V1/Autorefraktion.
- Senden erfolgt nur nach ausdruecklichem Anwenderklick ueber den im Schnittstellenprofil konfigurierten COM-Port.
- Der Schnittstellenprofilwert `NIDEK-RT Sendemodus` steuert den produktiven Ablauf. Im Praxisdefault `Direkt Writer-Frame senden` schreibt XDTBox den PC->RT-Frame direkt, sendet keinen `RS` und erwartet keine `SD`-Bestaetigung. In `RS/SD-Handshake` sendet XDTBox `SH C   SX RS EB ET` (`01 43 20 20 20 02 52 53 17 04`), erwartet `SX SD` und schreibt erst danach den PC->RT-Frame. In `RS senden, dann Writer ohne SD` wird `RS` gesendet, kurz gewartet und der Writer-Frame auch ohne `SD` geschrieben.
- Nach erfolgreichem Senden wechselt XDTBox in `Warte auf Rueckgabe vom Phoropter`. Eine ausbleibende sofortige Rueckgabe ist kein Sendefehler: Der Anwender fuehrt die Untersuchung am RT durch und loest danach PRINT/SEND aus. Ohne Rueckgabe wird kein leeres XDT erzeugt.
- Sobald eine Rueckgabe empfangen wird, wird sie bis `ET`/EOT gesammelt; danach wartet XDTBox eine kurze Stabilitaetszeit, bevor geparst und exportiert wird.
- Serielle RT-Schnittstellenprofile brauchen keinen Geraete-Eingangsordner und keinen dateibasierten Geraete-Ausgabeordner.
- Das RT-Floating-Fenster enthaelt fuer Live-Abnahmen eine serielle Diagnose: verwendete COM-Parameter, DTR/RTS/Handshake, Port-Status, RS-Anforderung, erwartete/empfangene SD-Bestaetigung, PC->RT-Writer-Frame, Hexdump und sichtbare Steuerzeichen werden angezeigt. `COM-Port nur abhoeren` oeffnet denselben Profil-Port, sendet nichts und zeigt empfangene Bytes ohne XDT-Export.
- Der einklappbare Bereich `Sendetest` im RT-Fenster dient nur der Praxisdiagnose. Er kann RS-only, DTR-Toggle, direkten Writer-Frame und RS+Writer-ohne-SD ausloesen; dadurch wird kein produktiver XDT-Export erzeugt.
- Wenn im `RS/SD-Handshake` keine SD-Bestaetigung eintrifft, zeigt XDTBox eine konkrete Pruefliste zu COM-Port, Type1/Type2, PC-Port-Parameter am RT, DTR/RTS/Handshake und Portbelegung. Im `DirectWriterFrame`-Modus gibt es keine SD-Fehlermeldung; dort zeigt XDTBox den gesendeten Writer-Frame und den anschliessenden Wartestatus auf PRINT/SEND-Rueckgabe.

## XDT-Baukasten

Im XDT-Baukasten zeigt die Ansicht `Geraeteausgabe` den sichtbaren Steuerzeichen-Text und einen Hexdump. Die Vorschau schreibt keine produktive Datei und sendet nichts an einen COM-Port.

Warnhinweis im Baukasten:

`NIDEK RT-2100/3100/5100 RS232 ist vorbereitet. Bitte echte Praxis-Mitschnitte pruefen, bevor produktiv gesendet wird.`

## Offene Praxispunkte

- weitere echte RT-2100-/RT-3100-/RT-5100-RS232-Mitschnitte
- Pruefung, welche Header-Variante das konkrete Praxisgeraet sendet
- Pruefung von Type1/Type2 und DTR/DSR-Verhalten
- weitere Live-Abnahmen des PC->RT-Sendeframes an RT-2100 und RT-5100; am RT-3100 ist `DirectWriterFrame` empfangen worden
- Rueckgabe nach einer echten Sendung am RT-3100/RT-2100/RT-5100
- MEDISTAR-Abnahme der `6228`-/`6227`-Rueckgabe
