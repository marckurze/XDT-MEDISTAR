# E2E-Testprotokoll: MEDISTAR + TOPCON CT-1P

Status: praktisch validiert fuer Tonometrie-/Pachymetrie-XDT-Rueckgabe aus einer echten TOPCON CT-1P XML-Datei.

## Validierter Ablauf

- MEDISTAR liefert eine AIS-GDT/XDT-Datei mit passender Untersuchungsart.
- TOPCON CT-1P liefert eine XML-Geraetedatei.
- Die App erkennt die CT-1P-Datei ueber `Ophthalmology`, `Company = TOPCON`, `ModelName = CT-1P` und `Measure type="TM"`.
- Die App liest TM-/Tonometriewerte.
- Die App liest CorrectedIOP/CCT, soweit vollstaendig vorhanden.
- Die App erzeugt eine MEDISTAR-kompatible XDT-Rueckgabedatei.
- MEDISTAR uebernimmt `8402` aus AIS.
- MEDISTAR uebernimmt `6220` fuer Pachymetrie.
- MEDISTAR uebernimmt `6205` fuer Tonometrie.

## Validierte Ergebnisfelder

- `8000 = 6310`
- `3000` Patientennummer
- `3101` Nachname
- `3102` Vorname
- `3103` Geburtsdatum
- `8402` Untersuchungsart aus AIS
- `6220` Pachymetrie / CCT
- `6205` Tonometrie / TM

## Validiertes Ergebnis

Patientendaten sind anonymisiert; die technischen Messwerte entsprechen der praktischen Ausgabe.

```text
01380006310
0153000<Testpatientennummer>
0173101<Testnachname>
0133102<Testvorname>
0173103<Testgeburtsdatum>
0148402CT-1P
0206220Pachymetrie
0186220RA: 0.767
0196205Tonometrie
0256205PR: 767 [767] µm
0576205PR: Gemessen = 16.0 mmHg; Korrigiert = 5.0 mmHg;
0566205PR: Param1 = 545um; Param2 = 0.050; CCT = 767um
0656205R = 13 12 13 26 [16.0] // L = 44 44 44 [44.0] mmHg 15:20
```

## Fachliche Bestaetigung

- `8402 = CT-1P` kommt aus AIS.
- Pachymetrie wird ueber `6220` ausgegeben.
- Tonometrie wird ueber `6205` ausgegeben.
- Der rechte CCT-/CorrectedIOP-Block wird korrekt ausgegeben.
- Der unvollstaendige linke CCT-/CorrectedIOP-Block wird weggelassen.
- Die linke IOP-Liste und der linke Average bleiben in der Tonometrie-Zusammenfassung enthalten.
- Es entstehen keine leeren Platzhalter.
- Es gab keinen Exportabbruch.
- Es entstehen keine `6228`-/`6221`-/`6227`-Messwertfelder.
- Es entstehen keine `6302`-`6305`-Linkfelder fuer Messwerte.

## XML-Struktur

- TOPCON Ophthalmology XML
- `Common/Company = TOPCON`
- `Common/ModelName = CT-1P`
- `Measure type="TM"`
- Namespaces `nsCommon` / `nsTM`
- echte Testfixture vorhanden

## Ausgabehinweise

- Tonometrie ueber `6205`
- Pachymetrie ueber `6220`
- `8402` aus AIS
- keine `6228`
- keine `6221`
- keine `6227`
- keine `6302`-`6305` fuer Messwerte
- keine leeren optionalen Fragmente
- keine EV-Zusaetze

## Einseitige CorrectedIOP/CCT-Besonderheit

- Die aktuelle Fixture enthaelt einen vollstaendigen rechten CorrectedIOP/CCT-Block.
- Der linke CorrectedIOP/CCT-Block ist unvollstaendig.
- Der vollstaendige rechte Block wird ausgegeben.
- Der unvollstaendige linke Block wird weggelassen.
- Die linke IOP-Liste und der linke Average werden trotzdem in der `6205`-Tonometrie-Zusammenfassung ausgegeben.
- Es werden keine kuenstlichen `LA`-/`PL`-CCT-Werte erzeugt.

## Zukuenftige beidseitige CT-1P-Dateien

Das CT-1P kann fachlich beidseitig messen. Wenn eine zukuenftige echte XML beidseitig vollstaendige CorrectedIOP/CCT-Werte enthaelt, soll die Anbindung rechts und links ausgeben. Eine echte beidseitige Fixture soll ergaenzt werden, sobald sie verfuegbar ist, damit `PR`/`PL` und `RA`/`LA` testseitig fest abgesichert sind.

## Grenzen / offen

- echte beidseitige CT-1P-Datei noch sammeln
- beidseitige `PR`/`PL`- und `RA`/`LA`-Ausgabe spaeter mit Fixture testseitig festnageln
- offizielles ZIP-Artefakt erst nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md`
- keine Live-Pfade, Kundendaten oder Patientendaten dokumentiert
