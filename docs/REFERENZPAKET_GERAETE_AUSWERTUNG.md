# Referenzdaten-Geraeteauswertung

Stand: 2026-06-05

Diese Auswertung fasst die statische Analyse von 100 entpackten Geraeteordnern zusammen. Es wurden keine Programme aus den Referenzdaten ausgefuehrt. Ausgewertet wurden nur Konfigurationen, Parser-/COM-Logik, XML-Schemata, Setup-/INI-Dateien, Textbeispiele und Dokumentation.

## Leitlinien

- Keine fremden Produkt- oder Systemnamen werden in XDTBox-UI, Code, Tests oder Kundendokumentation uebernommen.
- Neue BuiltIns werden nur angelegt, wenn Hersteller, Modell, Geraeteart, Anschlussart, Parserlogik, MEDISTAR-Ausgabe und synthetische Tests belastbar ableitbar sind.
- Parser-Skripte und COM-Logik zaehlen als technische Datenquelle, auch wenn keine echte Rohdatei vorhanden ist. Solche Geraete werden als Parser-Kandidaten dokumentiert und in einem eigenen Batch umgesetzt, sobald die neue Parserstrecke abgesichert ist.
- Bestehende funktionierende XDTBox-Anbindungen bleiben unveraendert, ausser wenn ein Alias oder ein sicherer Fallback eindeutig passt.

## Ausgeschlossen

Folgende Hersteller/Geraetebereiche wurden gemaess Auftrag nicht implementiert:

| Hersteller / Bereich | Entscheidung |
| --- | --- |
| Essilor | ausgeschlossen |
| Fertilly | ausgeschlossen |
| Heidelberg | ausgeschlossen |
| iCare | ausgeschlossen |
| Luneautech | ausgeschlossen |
| Siemens | ausgeschlossen |
| team2Work / team2work | ausgeschlossen |
| Roentgen-Anbindung | ausgeschlossen |

Hinweis zur Namenskorrektur: Der Hersteller wird in XDTBox als `Möller-Wedel` gefuehrt, wenn spaeter ein BuiltIn daraus entsteht.

## Neu implementiert

| Hersteller | Modell | Geraeteart | Anschluss | Parserdaten | MEDISTAR-Ausgabe | Entscheidung |
| --- | --- | --- | --- | --- | --- | --- |
| NIDEK | ARK-510A | Autorefraktor | Datei/LAN XML | `Data`-XML mit `Company`, `ModelName`, `VD`, `R/AR/ARMedian`, `L/AR/ARMedian`, `PD/PDList` | `6228` REF-Zeilen rechts/links | BuiltIn Geraet, Exportprofil, Schnittstellenprofil und Tests ergaenzt |
| NIDEK | ARK-560A | Autorefraktor | Datei/LAN XML | gleiche ARK-5xx-Struktur wie ARK-510A | `6228` REF-Zeilen rechts/links | BuiltIn Geraet, Exportprofil, Schnittstellenprofil und Tests ergaenzt |
| NIDEK | LM-1800P | Lensmeter | Datei/LAN XML | `Ophthalmology/Common`, `Measure Type="LM"`, LM-Augenbloecke `I`/`S`, Schreibweise `Sphare` | `6228` Lensmeter-Zeilen ueber vorhandene NIDEK-Lensmeter-Fachlogik | BuiltIn Geraet, Exportprofil, Schnittstellenprofil und Tests ergaenzt |

## Bestehende Anbindungen mit sicherer Verbesserung

| Hersteller | Modell / Familie | Verbesserung |
| --- | --- | --- |
| TOPCON | KR-800 / KR800S | `KR-800` wird als Alias der vorhandenen KR800S-XML-Familie erkannt. Die bestehende KR800S-Anbindung bleibt fachlich unveraendert. |
| NIDEK | LM7 / LM7P / LM-1800P | Die vorhandene NIDEK-Lensmeter-Fachlogik nutzt jetzt eine gemeinsame Modellpruefung und akzeptiert bei LM-1800P die alternativen Augenbloecke `I` und `S`. |

## Bereits vorhanden und nicht geaendert

| Hersteller | Geraet / Familie | Grund |
| --- | --- | --- |
| NIDEK | ARK1S, AR360, LM7/LM7P, NT530P, RT-2100/3100/5100 RS232, RT-6100 | Bereits als BuiltIn vorhanden; keine sichere Verbesserung ausser den oben genannten Aliases/Fallbacks erforderlich. |
| TOPCON | CL300, Solos, KR800S, KR-1, TRK2P, CT1P, CT800A, CV5000/CV5000S | Bereits als BuiltIn vorhanden; Referenzdaten lieferten in diesem Batch keine risikoarme fachliche Aenderung. |
| Dokumentgeraete | Generisches Dokumentgeraet, manuelle Dokumentauswahl | Bereits als BuiltIn vorhanden; keine Messwertparser betroffen. |

## Parser-Kandidaten aus Referenzlogik

Diese Geraete besitzen auswertbare Parser- oder COM-Logik, aber keine vollstaendig abgesicherte XDTBox-Parserstrecke in diesem Batch. Sie werden nicht verworfen; sie sind die naechsten Kandidaten fuer eigene Parserklassen mit synthetischen Fixtures.

| Hersteller | Modelle | Datenlage | Naechster Schritt |
| --- | --- | --- | --- |
| Huvitz | HRK-8000A, HRK-9000A | Parserlogik fuer Ref/KM-Zeilen und Feldkennungen `6228`/`6221` erkennbar; RS232-Parameter teilweise ableitbar | neutralen Huvitz-Textparser bauen, synthetische Textfixtures aus Scriptlogik erzeugen |
| Huvitz | HLM-1, HLM-7000P, HLM-9000 | Lensmeter-Parserlogik ableitbar, echte Rohdateien fehlen | Lensmeter-Textparser vorbereiten, Prisma/ADD/PD nur bei belegter Regel ausgeben |
| Huvitz | HNT-1P | Tonometrie-/Pachymetrie-Logik teilweise ableitbar | `6205`/`6220`-Mapper nur nach vollstaendiger Feldstruktur bauen |
| Huvitz | HDR-7000, HDR-9000 | Phoropter-/Refraktor-Kandidaten mit bidirektionaler Relevanz | Rueckgabe und PC->Geraet getrennt analysieren, kein BuiltIn ohne Sendeframe-Test |
| TOMEY | mehrere Zielgeraete | einzelne Parser-/Setup-Logik vorhanden, Geraetearten gemischt | Herstellerbatch TOMEY mit Typtrennung REF/KM/TM/Pachy |
| Shin-Nippon | Accuref/DL/DR/NCT/SLM-Familien | teilweise Parserlogik, keine einheitliche Rohdatenbasis | pro Formatfamilie synthetische Fixtures ableiten |
| Reichert | 7CR NCT, LensChek Plus | teilweise Setup-/Parserhinweise | getrennte Tonometer-/Lensmeter-Batches |
| Rodenstock | CX 800, Phoromat 2000 | teilweise Refraktor-/Phoropterhinweise | erst nach klarer Frame-/Textstruktur als BuiltIn |
| Möller-Wedel | Visutron-Familie | teilweise Hinweise, Herstellername korrigiert | Parserdetails und Anschlussart nachziehen |
| Canon | RK-F2, TX-20P, OCT A-1 Xephilo | teilweise Daten; OCT/Bildgeraet gesondert zu bewerten | Messwertgeraete vor Bild-/OCT-Workflows priorisieren |
| ZEISS | IOLMaster/VISU-Familien | teilweise Daten, teils Spezialgeraete | erst nach klarer XDTBox-Messartzuordnung |
| Visionix | Optovue/Retinomax/VX-Familien | Spezialgeraete und gemischte Datenlage | nur bei klarer Messwert- oder Dokumentuebergabe umsetzen |

## Nicht implementiert wegen unzureichender oder widerspruechlicher Datenlage

| Hersteller | Geraet | Grund |
| --- | --- | --- |
| TOPCON | RM-800 | XML-Hinweise verweisen auf eine andere Modellfamilie; kein eigenes BuiltIn ohne belastbare Modellabgrenzung. |
| TOPCON | TRK-3 Omnia | Referenzstruktur liegt nahe an bestehendem TRK2P; keine sichere eigenstaendige Modell-/Mappingentscheidung. |
| NIDEK | NT-510 / NT-530 | Hinweise liegen nahe an vorhandener NT530P-Struktur; keine neue BuiltIn-ID ohne echte Modellfixture. |
| NIDEK | LM-1800PD | LM-1800P-Fallback ist implementiert; PD-spezifische Variante braucht eigene Rohdaten oder eindeutige Regel. |
| NIDEK | AR-1 / AR-1S / AR-310A / AR-600 / AR-660A / ARK-1 / ARK-1E / ARK-1F / ARK-500A | AR-/ARK-Familie ist technisch nah, aber je Modell fehlt in diesem Batch eine vollstaendige, eindeutig getestete Struktur. |
| NIDEK | CEM-530 / TONOREF II / TONOREF III | Geraeteart und Ausgabe koennen gemischt sein; keine halbfertigen Kombi-BuiltIns. |
| Haag-Streit | Refractor-900 | Phoropterkandidat ohne ausreichende PC->Geraet- und Rueckgabe-Absicherung. |

## Uebernommene technische Regeln

- NIDEK ARK-5xx: einfache `Data`-XML-Struktur wird ueber die bestehende XML-Flattening-Logik verarbeitet; Exportprofile nutzen `ARMedian` fuer rechts/links, `PD/PDList[@No='1']/FarPD` und `VD`.
- NIDEK LM-1800P: die Schreibvariante `Sphare` wird ueber vorhandene Aliaslogik als `Sphere` verfuegbar; alternative Augenbloecke `I` und `S` werden fuer die vorbereiteten MEDISTAR-Lines auf `R` und `L` abgebildet.
- TOPCON KR-800: Modellalias `KR-800` nutzt die vorhandene KR800S-Erkennung, ohne die bestehende KR800S-Fachlogik zu veraendern.
- Scriptbasierte Textparser: Feldkennungen wie `6228`, `6227`, `6221`, `6220` und `6205` werden als Hinweise dokumentiert, aber nicht blind als XDTBox-Code kopiert.

## Offene Folgeschritte

1. Huvitz als ersten scriptbasierten Parser-Batch umsetzen: Textparser, synthetische Fixtures, BuiltIns nur fuer die Modelle mit sauberer Regelableitung.
2. TOMEY und Shin-Nippon danach nach Geraeteart trennen.
3. Widerspruechliche TOPCON-/NIDEK-Aliasfaelle erst mit echter Rohdatei oder eindeutiger Herstellerstruktur entscheiden.
4. Keine Referenzrohdateien, PDFs oder fremden Skripte in den Kundeninstaller aufnehmen.
