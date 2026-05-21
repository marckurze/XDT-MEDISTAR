# E2E-Testprotokoll MEDISTAR + TOPCON TRK2P / TRK-2P

Stand: 2026-05-22

## Status

Praktisch validiert fuer REF-/KM-/TM-/CCT-XDT-Rueckgabe aus echten TOPCON TRK2P XML-Dateien.

Der Workflow wurde mit dem BuiltIn-Schnittstellenprofil `MEDISTAR + TOPCON TRK2P` getestet. Das Geraetefenster ist korrekt angelegt und funktionsfaehig. Validiert wurden eine vollstaendige TRK-2P-Datei mit REF/KM/TM/CCT und eine Teilmessdatei mit nur TM/CCT.

## Validierter Ablauf

- MEDISTAR liefert eine AIS-GDT/XDT-Datei mit passender Untersuchungsart.
- TOPCON TRK2P liefert eine XML-Geraetedatei.
- Die App erkennt die TOPCON-TRK2P-XML-Datei.
- Die App liest die vorhandenen Messbloecke.
- Die App erzeugt eine MEDISTAR-kompatible XDT-Rueckgabedatei.
- MEDISTAR uebernimmt `8402` / Untersuchungsart aus AIS.
- MEDISTAR uebernimmt vorhandene Messarten ueber die passenden Feldkennungen.

## Validierte Ergebnisfelder

- `8000 = 6310`
- `3000` Patientennummer
- `3101` Nachname
- `3102` Vorname
- `3103` Geburtsdatum
- `8402` Untersuchungsart aus AIS
- `6228` REF / Autorefraktor
- `6221` KM / Keratometer
- `6220` Pachymetrie / CCT
- `6205` Tonometrie / TM
- `6227` SBJ / subjektive Refraktion, falls echte SBJ-Werte vorhanden sind

## Validiertes Ergebnis: vollstaendige TRK2P-Datei

Die folgenden XDT-Zeilen sind anonymisiert. Patientennummer, Name und Geburtsdatum sind Platzhalter; die TRK2P-Ergebniszeilen entsprechen dem praktisch validierten Lauf.

```text
01380006310
0153000<Testpatientennummer>
0173101<Testnachname>
0133102<Testvorname>
0173103<Testgeburtsdatum>
0148402TRK2P
0506228R.:S=- 6.53 Z=- 0.42*114 PD= 65 VD= 12.00
0336228L.:S=- 6.53 Z=- 0.47* 65
0916221R: R1=8.56 39.44 *87 R2=8.49 39.76 *177 // L: R1=8.62 39.16 *71 R2=8.51 39.66 *161
0716221R: AV=8.53 39.60 CYL=-0.32 87 // L: AV=8.57 39.41 CYL=-0.50 71
0206220Pachymetrie
0336220RA: 0.907   // LA: 0.880
0196205Tonometrie
0256205PR: 907 [907] µm
0256205PL: 880 [880] µm
0586205PR: Gemessen = 21.0 mmHg; Korrigiert = 16.0 mmHg;
0566205PR: Param1 = 520um; Param2 = 0.012; CCT = 907um
0586205PL: Gemessen = 18.0 mmHg; Korrigiert = 14.0 mmHg;
0566205PL: Param1 = 520um; Param2 = 0.012; CCT = 880um
0626205R = 20 21 21 [21.0] // L = 17 18 19 [18.0] mmHg 17:45
```

Fachlich bestaetigt:

- `6228` REF ist vorhanden.
- `6221` KM ist vorhanden.
- `6220` Pachymetrie ist vorhanden.
- `6205` Tonometrie ist vorhanden.
- Es entstehen keine leeren Platzhalter.
- Die fruehere ueberlange Tonometrie-Gesamtzeile ist in mehrere lesbare `6205`-Zeilen aufgeteilt.
- Tonometrie und Pachymetrie sind analog NT530P gegliedert.

## Validiertes Ergebnis: Teilmessung TM/CCT-only

Die Datei `M-Serial1165_20241126_225512_TOPCON_TRK-2P_5284298.xml` enthaelt nur TM/CCT und keine REF-/KM-/SBJ-Bloecke. Sie wurde praktisch verarbeitet, ohne Exportabbruch und ohne leere Messwertzeilen.

```text
01380006310
0153000<Testpatientennummer>
0173101<Testnachname>
0133102<Testvorname>
0173103<Testgeburtsdatum>
0148402TRK2P
0206220Pachymetrie
0336220RA: 0.510   // LA: 0.517
0196205Tonometrie
0296205PR: 511 509 [510] µm
0336205PL: 516 518 518 [517] µm
0626205R = 15 15 15 [15.0] // L = 13 12 14 [13.0] mmHg 22:55
```

Fachlich bestaetigt:

- `8402 TRK2P` kommt aus AIS.
- `6220` Pachymetrie wird aus CCT erzeugt.
- `6205` Tonometrie wird aus TM erzeugt.
- Es entstehen keine `6228`-Zeilen, weil REF fehlt.
- Es entstehen keine `6221`-Zeilen, weil KM fehlt.
- Es entstehen keine `6227`-Zeilen, weil SBJ fehlt.
- Es entstehen keine leeren Platzhalter.
- Teilmessungen erzeugen keinen Exportabbruch, solange verwertbare Messdaten vorhanden sind.

## XML-Struktur

- TOPCON Ophthalmology XML
- `Common/Company = TOPCON`
- `Common/ModelName = TRK-2P`
- `Measure type="REF"`
- `Measure type="KM"`
- `Measure type="TM"`
- optional `Measure type="CCT"`
- optional `Measure type="SBJ"`
- Namespaces `nsCommon`, `nsREF`, `nsKM`, `nsTM` und optional `nsSBJ`
- echte Fixtures fuer Serial0001, Serial0135 und Serial1165 sind im Testprojekt vorhanden

## Ausgabehinweise

- REF wird ueber `6228` ausgegeben.
- KM wird ueber `6221` ausgegeben.
- Pachymetrie/CCT wird ueber `6220` ausgegeben, wenn CCT-Werte vorhanden sind.
- Tonometrie/TM wird ueber `6205` ausgegeben, wenn TM-Werte vorhanden sind.
- SBJ wird ueber `6227` ausgegeben, aber nur bei echten SBJ-Werten.
- `8402` kommt aus AIS.
- Tonometrie/Pachymetrie werden im lesbaren Mehrzeilenformat wie NT530P ausgegeben.
- Nicht vorhandene Messarten werden nicht als leere Zeilen ausgegeben.
- Leere optionale Fragmente werden ausgelassen.
- Fuer Messwerte werden keine `6302`-`6305`-Dokumentanhangfelder erzeugt.

## Teilmessungsregel

Geraete koennen Teilmessungen liefern. Nicht immer wird alles gemessen, was ein Geraet technisch kann; auch einseitige Messungen sind grundsaetzlich moeglich.

Regel:

- vorhandene Messarten ausgeben
- vorhandene Augen ausgeben
- fehlende Messarten weglassen
- fehlende Augen weglassen
- fehlende optionale Werte weglassen
- kein Exportabbruch, solange mindestens eine fachlich verwertbare Messung vorhanden ist
- keine leeren Platzhalter erzeugen

CCT-`ERROR`-Eintraege werden ignoriert. Wenn mehrere gueltige CCT-Werte ohne Average vorliegen, wird der arithmetische Mittelwert verwendet und auf den auszugebenden Pachy-/Klammerwert gerundet.

## Grenzen und offene Punkte

- SBJ ist laut Geraet/Spezifikation moeglich, war in den aktuellen praktischen XML-Dateien aber nicht enthalten.
- Weitere SBJ-Praxisfaelle koennen spaeter ergaenzt und fachlich verfeinert werden.
- Ein offizielles ZIP-Artefakt wird erst nach der Release-Regel dauerhaft abgelegt.
- Es wurden keine Live-Pfade, Kundendaten oder echten Patientendaten dokumentiert.
