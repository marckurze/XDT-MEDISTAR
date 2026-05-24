# E2E-Testprotokoll MEDISTAR + TOPCON CT-800A

Stand: 2026-05-24

## Status

Fixture-validierter Non-Contact-Tonometer-Kandidat fuer die MEDISTAR-XDT-Rueckgabe aus echten TOPCON CT-800A XML-Dateien. Die praktische MEDISTAR-Abnahme steht noch aus.

## Validierter Ablauf

- MEDISTAR liefert eine AIS-GDT/XDT-Datei mit passender Untersuchungsart.
- TOPCON CT-800A liefert eine XML-Geraetedatei.
- Die App erkennt TOPCON CT-800A XML ueber `Ophthalmology`, `Company = TOPCON`, `ModelName = CT-800A` und `Measure type="TM"`.
- Die App liest vorhandene Tonometrie-Listen und Average-Werte.
- Die App liest `CorrectedIOP/Formula1` nur je Auge vollstaendig, wenn CCT, Measured und Corrected vorhanden sind.
- Die App erzeugt eine MEDISTAR-kompatible XDT-Rueckgabedatei.
- MEDISTAR uebernimmt `8402` aus der AIS-Datei.
- MEDISTAR soll die `6205`-Tonometriezeilen uebernehmen.

## Validierte Ergebnisfelder

- `8000 = 6310`
- `3000` Patientennummer
- `3101` Nachname
- `3102` Vorname
- `3103` Geburtsdatum
- `8402` Untersuchungsart aus AIS
- `6205` Tonometrie / TM

## Fixture-Ergebnisse

Die folgenden Inhalte stammen aus echten CT-800A XML-Fixtures. Patientendaten sind nicht live dokumentiert; XDT-Laengenpraefixe werden in der App automatisch berechnet.

### Serial0069

`M-Serial0069_20190411_170716_TOPCON_CT-800A_AA3100481.xml`

```text
6205 Tonometrie
6205 PR: 550 [550] um
6205 PL: 550 [550] um
6205 PR: Gemessen = 19.0 mmHg; Korrigiert = 19.0 mmHg;
6205 PR: Param1 = 545um; Param2 = 0.050; CCT = 550um
6205 PL: Gemessen = 17.0 mmHg; Korrigiert = 17.0 mmHg;
6205 PL: Param1 = 545um; Param2 = 0.050; CCT = 550um
6205 R = 18 20 [19.0] // L = 17 16 [17.0] mmHg 17:07
```

### Serial0070

`M-Serial0070_20190411_171259_TOPCON_CT-800A_AA3100481.xml`

```text
6205 Tonometrie
6205 R = 18 18 [18.0] // L = 18 19 [19.0] mmHg 17:12
```

### Serial0071

`M-Serial0071_20190411_175130_TOPCON_CT-800A_AA3100481.xml`

```text
6205 Tonometrie
6205 R = 22 22 [22.0] // L = 25 25 24 [25.0] mmHg 17:51
```

Validiert:

- Alle drei CT-800A XML-Fixtures werden erkannt.
- `Company = TOPCON`, `ModelName = CT-800A`, `Measure type="TM"` werden gelesen.
- Rechte und linke IOP-Listen sowie Average-Werte werden gelesen.
- Zeit aus `Common/Time` wird in der Tonometrie-Zusammenfassung verwendet.
- Vollstaendige CorrectedIOP/CCT-Daten aus Serial0069 werden in `6205`-Detailzeilen ausgegeben.
- Unvollstaendige CorrectedIOP/CCT-Bloecke aus Serial0070/Serial0071 erzeugen keine leeren Fragmente und keinen Exportabbruch.
- Unnamespaced Detail-/ERROR-Bloecke werden nicht als zusaetzliche Messwerte exportiert.
- Keine `6220`, `6228`, `6221`, `6227` oder `6330` fuer CT-800A-Messwerte.

## XML-Struktur

- Root: `Ophthalmology`
- Common-Namespace: `nsCommon`
- Tonometrie-Namespace: `nsTM`
- `Common/Company = TOPCON`
- `Common/ModelName = CT-800A`
- `Measure type="TM"`
- Tonometrie: `TM/R/List/IOP_mmHg`, `TM/L/List/IOP_mmHg`, `TM/R/Average/IOP_mmHg`, `TM/L/Average/IOP_mmHg`
- Optional: `CorrectedIOP/Formula1/R` und `CorrectedIOP/Formula1/L`
- Fixtures: Serial0069, Serial0070 und Serial0071

## Ausgabehinweise

- Ausgabe erfolgt ueber `6205`.
- `8402` kommt aus AIS/MEDISTAR.
- CCT aus `CorrectedIOP` wird bei CT-800A nur innerhalb der `6205`-Tonometrie-Detailzeilen ausgegeben.
- `CCT unit="mm" 0.550` wird zu `550um`.
- `Param1 unit="mm" 0.545` wird zu `545um`.
- CorrectedIOP-Zeilen entstehen nur, wenn CCT, Measured und Corrected fuer dieses Auge vorhanden sind.
- Fehlende optionale Werte und fehlende Augen werden weggelassen.
- ConfidenceIndex und ERROR-/Alignment-Detailbloecke werden nicht als MEDISTAR-Zusatz exportiert.
- Es entstehen keine leeren optionalen Fragmente.

## Grenzen und offene Punkte

- Praktische MEDISTAR-Importvalidierung ist noch offen.
- Fuer CT-800A wird aktuell keine separate `6220`-Pachymetrie aus CorrectedIOP-CCT erzeugt, solange kein validiertes CT-800A-Pachymetrie-Zielformat vorliegt.
- Weitere echte CT-800A-Dateien mit einseitiger Messung koennen die Teilmessungsfaelle praktisch bestaetigen.
- Offizielles ZIP-Artefakt erst nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md`.
- Keine Live-Pfade, Kundendaten oder echten Patientendaten dokumentiert.
