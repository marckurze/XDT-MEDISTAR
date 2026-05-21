# E2E-Testprotokoll MEDISTAR + TOPCON KR800S / KR-800S

Stand: 2026-05-21

## Status

Praktisch validiert fuer REF-/KM-/SBJ-XDT-Rueckgabe aus echten TOPCON KR800S XML-Dateien.

Der Workflow wurde mit dem BuiltIn-Schnittstellenprofil `MEDISTAR + TOPCON KR800S` getestet. Das Geraetefenster ist korrekt angelegt und funktionsfaehig. Nach der gezielten Reparatur alter persistierter BuiltIn-Exportprofile und der Trennung der SBJ-Header-/Messwertzeilen wird die MEDISTAR-XDT-Rueckgabe korrekt erzeugt.

## Validierter Ablauf

- MEDISTAR liefert eine AIS-GDT-/XDT-Datei mit passender Untersuchungsart.
- TOPCON KR800S liefert eine XML-Geraetedatei.
- Die App erkennt TOPCON KR800S XML ueber `Company = TOPCON`, `ModelName = KR-800S` und die Measure-Bloecke `REF`, `KM` und `SBJ`.
- Die App liest REF-, KM- und SBJ-Werte aus der XML-Datei.
- Die App erzeugt eine MEDISTAR-kompatible XDT-Rueckgabedatei.
- MEDISTAR uebernimmt `8402` / Untersuchungsart aus AIS.
- MEDISTAR uebernimmt `6228` fuer REF / Autorefraktion.
- MEDISTAR uebernimmt `6221` fuer KM / Keratometrie.
- MEDISTAR uebernimmt `6227` fuer SBJ / subjektive Refraktion.

## Validierte Ergebnisfelder

- `8000` = `6310`
- `3000` Patientennummer
- `3101` Nachname
- `3102` Vorname
- `3103` Geburtsdatum
- `8402` Untersuchungsart aus AIS
- `6228` Autorefraktor / REF
- `6221` Keratometer / KM
- `6227` subjektive Refraktion / SBJ

## Validiertes Ergebnis Serial0036

Die Kopf-/Patientenzeilen sind anonymisiert. Die Messwertzeilen entsprechen der praktischen Ausgabe inklusive der dort erzeugten XDT-Laengenpraefixe.

```text
01380006310
0153000<Testpatientennummer>
0173101<Testnachname>
0133102<Testvorname>
0173103<Testgeburtsdatum>
0148402KR800
0506228R.:S=- 5.50 Z=+ 0.00*  0 PD= 58 VD= 13.75
0336228L.:S=- 5.25 Z=+ 0.00*  0
0916221R: R1=7.70 43.75 *180 R2=7.70 43.75 *90 // L: R1=7.70 43.75 *180 R2=7.69 43.75 *90
0696221R: AV=7.70 43.75 CYL=+0.00 0 // L: AV=7.70 43.75 CYL=+0.00 0
0476227Subjektive Refraktion Full Correction FAR:
0706227L.:S=- 6.00 Z=+ 0.00* 93 VA=0.7 PD=58.5 VD=12.00
0486227Subjektive Refraktion Full Correction NEAR:
0706227L.:S=- 8.25 Z=+ 0.00* 93 VA=0.8 PD=58.5 VD=12.00
```

Fachlich bestaetigt:

- `8402 = KR800` kommt aus AIS.
- REF wird ueber `6228` ausgegeben.
- KM wird ueber `6221` ausgegeben.
- SBJ wird ueber `6227` ausgegeben.
- SBJ-Ueberschrift und SBJ-Messwert stehen getrennt.
- Es entstehen keine leeren Platzhalter.
- Es entstehen keine KM-Zeilen ueber `6228`.
- Es entstehen keine unnoetigen `6205`-, `6220`- oder `6302`-`6305`-Messwertfelder.

## Validiertes Ergebnis Serial0426

Fuer Serial0426 liegen testseitig validierte Erwartungswerte fuer den echten KR800S-XML-Inhalt vor. Eine vollstaendige praktische XDT-Datei mit Laengenpraefixen wurde in diesem Protokoll nicht nacherfunden.

```text
8402 KR800S
6228 R.:S=+ 3.75 Z=- 4.00* 13 PD= 66 VD= 13.75
6228 L.:S=+ 3.75 Z=- 2.50*173
6221 R: R1=8.48 39.75 *11 R2=7.79 43.50 *101 // L: R1=8.35 40.50 *171 R2=7.87 43.00 *81
6221 R: AV=8.14 41.75 CYL=-3.75 11 // L: AV=8.11 41.75 CYL=-2.50 171
6227 Subjektive Refraktion Full Correction FAR:
6227 R.:S=+ 3.75 Z=- 4.00* 13 VA=0.6 / L.:S=+ 3.75 Z=- 2.50*173 VA=1.0 PD=66 VD=13.75
6227 Subjektive Refraktion Full Correction NEAR:
6227 R.:S=+ 5.50 Z=- 4.00* 13 / L.:S=+ 5.50 Z=- 2.50*173 PD=66 VD=13.75
```

Fachlich bestaetigt:

- REF-Werte werden ueber `6228` ausgegeben.
- KM-R1/R2 und KM-AV/CYL werden ueber `6221` ausgegeben.
- SBJ Full Correction FAR/NEAR wird ueber getrennte `6227`-Header-/Messwertzeilen ausgegeben.
- `8402` kommt aus AIS.
- Es entstehen keine leeren Platzhalter.
- Es entstehen keine KM-Zeilen ueber `6228`.

## XML-Struktur

- TOPCON Ophthalmology XML
- `Company = TOPCON`
- `ModelName = KR-800S`
- `Measure type="REF"`
- `Measure type="KM"`
- `Measure type="SBJ"`
- Namespaces `nsCommon`, `nsREF`, `nsKM`, `nsSBJ`
- Shift-JIS-Encoding wird beruecksichtigt.
- Echte Testfixtures:
  - `M-Serial0036_20131206_213127_TOPCON_KR-800S_.xml`
  - `M-Serial0426_20241126_145500_TOPCON_KR-800S_4871341.xml`

## Ausgabehinweise

- REF wird ueber `6228` ausgegeben.
- KM wird ueber `6221` ausgegeben.
- SBJ wird ueber `6227` ausgegeben.
- `8402` kommt aus AIS.
- SBJ-Header und SBJ-Messwert stehen getrennt.
- Leere optionale Fragmente werden nicht ausgegeben.
- KM-Zeilen werden nicht ueber `6228` ausgegeben.
- Fuer KR800S werden keine `6205`- oder `6220`-Zeilen erzeugt.
- Fuer KR800S-Messwerte werden keine Anhangfelder `6302` bis `6305` erzeugt.

## Grenzen und offene Punkte

- Die SBJ-Ausgabe bleibt bewusst konservativ.
- Weitere SBJ- und Funktionstestfaelle koennen nach weiteren echten KR800S-Dateien verfeinert werden.
- Die praktische Bewertung weiterer Funktionstests bleibt zu beobachten.
- Ein offizielles ZIP-Artefakt folgt erst nach der Release-Regel.
- Es wurden keine Live-Pfade, Kundendaten oder echten Patientendaten dokumentiert.
