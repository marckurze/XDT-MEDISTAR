# NIDEK RS232 Protokollnotizen

Stand: 2026-05-27

Diese Notiz dokumentiert die vorbereitete NIDEK-RS232-Protokollschicht in XDTBox. Sie ist eine technische Grundlage fuer Praxistests mit echten seriellen Geraeten, keine Freigabe fuer automatische medizinische Ausgabe.

## Grundrahmen

- RS-232C, asynchron, halbduplex
- ASCII-basierte Nutzdaten
- Steuerzeichen:
  - `SOH = 0x01`
  - `STX = 0x02`
  - `ETB = 0x17`
  - `EOT = 0x04`
  - `CR = 0x0D`
  - `LF = 0x0A`

`STX` ist im Code fest als `0x02` definiert. OCR- oder PDF-Extraktionsartefakte werden nicht als Quelle fuer Steuerzeichen verwendet.

Typisches Frameformat:

```text
SOH [Header] STX [DataSegment] ETB [DataSegment] ETB ... EOT [optional Checksum] [optional CR/LF]
```

Der Reader toleriert `CR` nach `ETB`, `CR/LF` nach `EOT`, Noise vor `SOH`, mehrere Frames in einem Puffer und unvollstaendige Frames.

## Modi

- `Nidek`: vorbereiteter NIDEK-Modus mit RS/SD-Kommandopfad fuer spaetere bidirektionale Workflows.
- `Ncp10`: Listenmodus; das Geraet sendet nach Print/Send direkt, keine RS/SD-Kommandos erforderlich.
- `Ncp20Prepared`: reserviert, noch nicht produktiv.
- `PcPrepared`: reserviert, noch nicht produktiv.
- `Unknown`: toleranter Analysemodus.

## Checksumme

Bei NCP10 kann nach `EOT` eine vierstellige ASCII-Hex-Checksumme folgen. Die vorbereitete Berechnung summiert alle Bytes von `SOH` bis `EOT` inklusive, ohne nachfolgendes `CR/LF`, und vergleicht die unteren zwei Bytes als vier Hex-Zeichen.

Fehlende Checksumme ist kein Absturz. Im NCP10-Modus wird sie als Warnung markiert.

## Nutzdaten

Vorbereitet sind:

- LM/Lensmeter-Frames mit Header `DLM`
  - Modellsegment `IDNIDEK/...`
  - Datum/Zeit `DAyyyy.MM.dd.HH:mm`
  - Refraktionssegmente fuer Single/Right/Left mit Sphere, Cylinder, Axis
  - ADD aus `A`, `AR`, `AL`
  - NearSphere aus `N`, `NR`, `NL`
  - PD aus `PD...`
  - `IE`/`DE` als Fehlersegmente
  - XDT-Kandidat nur `6228`
- NT/Tonometrie-Frames mit Header `DNT`
  - R/L-Segmente mit Count, Einzelwerten und `AV`
  - Fehlercodes `APL`, `OVR`, `ERR`
  - XDT-Kandidat nur `6205`
- PM/Pachymetrie-Frames mit Header `DPM`
  - R/L-Segmente mit Count, Einzelwerten und `AV`
  - Fehlercodes `BLK`, `ALM`, `ERR`
  - XDT-Kandidat nur `6220`

Unbekannte Segmente werden als Rohsegmente erhalten. UV/Transmission, Prisma-Details und weitere Spezialsegmente werden ohne validierte Zielfeldregel nicht nach MEDISTAR exportiert.

## Testdialog

Im Tab `Profile & Templates` kann die RS232-Testfunktion auf `NIDEK RS232` gestellt werden. Die Auswertung zeigt:

- Frames
- Header und DeviceCode
- Segmente
- Checksumme vorhanden/gueltig/ungueltig
- erkannte Hersteller-/Modellinformationen
- Messwertkandidaten
- Warnungen und Fehlersegmente

Es findet kein automatischer XDT-Export aus dem Testdialog statt.

## Grenzen

- LAN bleibt der bestehende Datei-/UNC-Workflow.
- Bestehende XML-/LAN-Parser werden nicht veraendert.
- Es gibt noch keinen produktiven seriellen Dauerbetrieb.
- Es werden keine medizinischen Werte erfunden.
- Produktive serielle Parserfreigabe benoetigt echte Rohdatenfixtures und Praxistest je Geraet.
