# ZEISS IOLMaster 700 Protokollnotizen

Stand: 2026-06-06

Diese Notizen dokumentieren die testseitig vorbereitete XDTBox-Anbindung fuer ZEISS IOLMaster 700. Die aktuelle Ableitung basiert auf statisch ausgewerteter XML-Struktur und einer neutralen synthetischen Fixture. Eine praktische MEDISTAR-Abnahme mit echter Praxisdatei steht noch aus.

## Verbindung und Profil

- Geraet: ZEISS IOLMaster 700
- Verbindung: Datei/LAN/UNC, XML-Export
- Richtung: Geraet -> XDTBox -> AIS
- Bidirektional: nein
- Parser: `ZeissIolMaster700DeviceParser`
- Standard-Untersuchungsart: `IOL`

## Parserregeln

- XML wird namespace-tolerant gelesen.
- `Company`, `ModelName` und optionale Patient-ID werden als Diagnose-/Platzhalterwerte bereitgestellt.
- VKT/ACD und AL werden pro Auge als IOL-Biometriewerte erkannt.
- Keratometrie-Radien und Achsen werden pro Auge als KM-Werte erkannt.
- Nur erkannte Werte werden exportiert; fehlende Augen oder Teilwerte erzeugen keine kuenstlichen Zeilen.

## MEDISTAR-Ausgabe

- `6227`: IOLMaster-Biometrie mit VKT/AL.
- `6228`: IOLMaster-Keratometrie R1/R2 mit Achsen.
- `6330` wird nicht erzeugt.
- Kuenstliche Trenner werden nicht erzeugt.
- Automatische Anhangsfelder `6302`, `6303` und `6305` sind im IOLMaster-BuiltIn nicht aktiv.

Beispiel fuer die vorbereitete synthetische Fixture:

```text
6227R: VKT=3.33 AL=24.16 // L: VKT=3.38 AL=24.49
6228R: R1=+ 7.48*1.69 R2=+ 7.20*91.69 // L: R1=+ 7.58*172.75 R2=+ 7.24*82.75
```

## Tests

- Parser liest die synthetische IOLMaster-XML-Fixture.
- Exportprofil erzeugt `6227` fuer VKT/AL und `6228` fuer R1/R2.
- Baukasten-Preview nutzt den IOLMaster-spezifischen Parser.
- AIS-Ausgabe-Info zeigt `IOL` als Default-Untersuchungsart sowie `6227`/`6228`, aber keine automatischen Anhangsfelder.

## Offene Abnahme

- Echte IOLMaster-700-Praxis-XML sammeln.
- MEDISTAR-Import der `6227`-/`6228`-Zeilen praktisch pruefen.
- Bei Praxisdateien mit weiteren IOL-Feldern konservativ entscheiden, ob sie Karteikartenwert oder nur Diagnoseplatzhalter bleiben.
