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
| weitere nicht beauftragte Systembereiche | ausgeschlossen |
| Roentgen-Anbindung | ausgeschlossen |

Hinweis zur Namenskorrektur: Der Hersteller wird in XDTBox als `Möller-Wedel` gefuehrt, wenn spaeter ein BuiltIn daraus entsteht.

## Neu implementiert

| Hersteller | Modell | Geraeteart | Anschluss | Parserdaten | MEDISTAR-Ausgabe | Entscheidung |
| --- | --- | --- | --- | --- | --- | --- |
| NIDEK | ARK-510A | Autorefraktor | Datei/LAN XML | `Data`-XML mit `Company`, `ModelName`, `VD`, `R/AR/ARMedian`, `L/AR/ARMedian`, `PD/PDList` | `6228` REF-Zeilen rechts/links | BuiltIn Geraet, Exportprofil, Schnittstellenprofil und Tests ergaenzt |
| NIDEK | ARK-560A | Autorefraktor | Datei/LAN XML | gleiche ARK-5xx-Struktur wie ARK-510A | `6228` REF-Zeilen rechts/links | BuiltIn Geraet, Exportprofil, Schnittstellenprofil und Tests ergaenzt |
| NIDEK | LM-1800P | Lensmeter | Datei/LAN XML | `Ophthalmology/Common`, `Measure Type="LM"`, LM-Augenbloecke `I`/`S`, Schreibweise `Sphare` | `6228` Lensmeter-Zeilen ueber vorhandene NIDEK-Lensmeter-Fachlogik | BuiltIn Geraet, Exportprofil, Schnittstellenprofil und Tests ergaenzt |
| NIDEK | AR-1 | Autorefraktor | Datei/LAN XML | `Data`-XML mit `R/AR/ARMedian`, `L/AR/ARMedian`, `VD` und optionaler `PDList`-Logik wie AR360/ARK-5xx | `6228` REF-Zeilen rechts/links | BuiltIn Geraet, Exportprofil, Schnittstellenprofil, Baukasten-Alias, synthetische XML-Fixture und Templatepaket-Test ergaenzt |
| NIDEK | AR-1S | Autorefraktor mit subjektiver Refraktion | Datei/LAN XML | AR-Struktur plus `R/SR` und `L/SR` fuer subjektive Refraktionswerte | `6228` REF-Zeilen, `6227` subjektive Refraktion | BuiltIn Geraet, Exportprofil, Schnittstellenprofil, synthetische XML-Fixture und Templatepaket-Test ergaenzt |
| NIDEK | AR-310A | Autorefraktor | Datei/LAN XML | gleiche AR-XML-Familie wie AR-1/AR360 mit `ARMedian`, `VD` und optionaler `PDList` | `6228` REF-Zeilen rechts/links | BuiltIn Geraet, Exportprofil, Schnittstellenprofil, Baukasten-Alias, synthetische XML-Fixture und Templatepaket-Test ergaenzt |
| NIDEK | LM-1800PD | Lensmeter | Datei/LAN XML | gleiche NIDEK-Lensmeter-XML-Familie wie LM-1800P/LM7; `ModelName=LM-1800PD`, `Measure Type="LM"` | `6228` Lensmeter-Zeilen rechts/links | BuiltIn-Variante mit eigener Profilkombination, Parser-Alias, synthetischer XML-Fixture und Templatepaket-Test ergaenzt |
| NIDEK | NT-1 | Tonometer/Pachymeter | Datei/LAN XML | NT530P-nahe `Data`-XML-Familie mit `NT`, `CorrectedIOP` und `PACHY` | `6205` Tonometrie, `6220` Pachymetrie | BuiltIn-Variante mit eigener Profilkombination, Parser-Alias, synthetischer XML-Fixture und Templatepaket-Test ergaenzt |
| NIDEK | NT-1E | Tonometer/Pachymeter | Datei/LAN XML | gleiche NT-XML-Familie wie NT-1/NT530P | `6205` Tonometrie, `6220` Pachymetrie | BuiltIn-Variante mit eigener Profilkombination, Parser-Alias, synthetischer XML-Fixture und Templatepaket-Test ergaenzt |
| NIDEK | NT-1P | Tonometer/Pachymeter | Datei/LAN XML | gleiche NT-XML-Familie wie NT-1/NT530P | `6205` Tonometrie, `6220` Pachymetrie | BuiltIn-Variante mit eigener Profilkombination, Parser-Alias, synthetischer XML-Fixture und Templatepaket-Test ergaenzt |
| NIDEK | NT-510 | Tonometer/Pachymeter | Datei/LAN XML | gleiche NT-XML-Familie wie NT530P mit `NT`/`PACHY`-Auswertung | `6205` Tonometrie, `6220` Pachymetrie | BuiltIn-Variante mit eigener Profilkombination, Parser-Alias, synthetischer XML-Fixture und Templatepaket-Test ergaenzt |
| NIDEK | NT-530 | Tonometer/Pachymeter | Datei/LAN XML | gleiche NT-XML-Familie wie NT530P ohne `P`-Suffix im Modellnamen | `6205` Tonometrie, `6220` Pachymetrie | BuiltIn-Variante mit eigener Profilkombination, Parser-Alias, synthetischer XML-Fixture und Templatepaket-Test ergaenzt |
| Huvitz | HRK-8000A | Autorefraktor/Keratometer | RS232 Text | Referenzlogik fuer `S-R-R`, `S-R-L`, `S-K-R`, `S-K-L`; synthetische Fixture, keine echte Praxisrohdatei | `6228` REF-Zeilen, `6221` KM-Zeile | BuiltIn Geraet, Exportprofil, Schnittstellenprofil, Huvitz-Textparser und Tests ergaenzt |
| Huvitz | HRK-9000A | Autorefraktor/Keratometer | RS232 Text | gleiche Huvitz-REF/KM-Textfamilie wie HRK-8000A | `6228` REF-Zeilen, `6221` KM-Zeile | BuiltIn Geraet, Exportprofil, Schnittstellenprofil und Templatepaket-Test ergaenzt |
| Huvitz | HNT-1P | Tonometer/Pachymeter | RS232 Text | Referenzlogik fuer `T-R01..03`, `T-R-A`, `T-L01..03`, `T-L-A`, `P-R01..03`, `P-R-A`, `P-L01..03`, `P-L-A`; synthetische Fixture | `6205` Tonometrie, `6220` Pachymetrie | BuiltIn Geraet, Exportprofil, Schnittstellenprofil, Huvitz-Textparser und Tests ergaenzt |
| Huvitz | HTR-1A | Autorefraktor/Keratometer/Tonometer/Pachymeter | RS232 Text | kombinierte Huvitz-Textfamilie fuer REF/KM/IOP/CCT; synthetische Fixture | `6228`, `6221`, `6205`, `6220` | BuiltIn Geraet, Exportprofil, Schnittstellenprofil, Baukasten-Preview-Test und Templatepaket-Test ergaenzt |
| TOMEY | CF-2000 | Lensmeter | RS232 Text | Referenzlogik fuer `LR/LL` SCA, `AR/AL` ADD, optional `PR/PL` Prisma; synthetische Fixture | `6228` Lensmeter-Zeilen | BuiltIn Geraet, Exportprofil, Schnittstellenprofil, TOMEY-Parser und Tests ergaenzt |
| TOMEY | TL-2000C | Lensmeter | Datei/CSV | TL-CSV-Sektionen fuer `POWER_R/L`, `ADD_R/L`, `PD` und Prisma; synthetische Fixture | `6228` Lensmeter-Zeilen | BuiltIn Geraet, Exportprofil, Schnittstellenprofil und Templatepaket-Test ergaenzt |
| TOMEY | TL-6000 | Lensmeter | Datei/CSV | gleiche TL-CSV-Familie wie TL-2000C | `6228` Lensmeter-Zeilen | BuiltIn Geraet, Exportprofil, Schnittstellenprofil und Templatepaket-Test ergaenzt |
| TOMEY | TL-7000 | Lensmeter | Datei/CSV | gleiche TL-CSV-Familie wie TL-2000C | `6228` Lensmeter-Zeilen | BuiltIn Geraet, Exportprofil, Schnittstellenprofil und Templatepaket-Test ergaenzt |
| TOMEY | MR-6000 | Kombigeraet REF/KM/Tonometrie/Pachymetrie | Datei/XML | MR-XML-Strukturen fuer REF, KM, TM und PM; synthetische Fixture | `6228`, `6221`, `6205`, `6220` | BuiltIn Geraet, Exportprofil, Schnittstellenprofil, Baukasten-Preview-Test und Templatepaket-Test ergaenzt |
| TOMEY | TOP-1000 | Tonometer/Pachymeter | Datei/XML | TOP-XML-Strukturen fuer IOP/CorrectedIOP/CCT; synthetische Fixture | `6205` Tonometrie, `6220` Pachymetrie | BuiltIn Geraet, Exportprofil, Schnittstellenprofil und Templatepaket-Test ergaenzt |
| TOMEY | EM-3000 | Endothel-/Zellmessung | Datei/CSV | CSV-Token fuer `[RL]`, `[NUMBER]`, `[DENSITY]`, `[THK]`, `[FILES_N]`, `[FILE]`; synthetische Fixture | `6228` Messwerte, `6227` Kommentare, `6302` Bild-/Dateiverweise | BuiltIn Geraet, Exportprofil, Schnittstellenprofil, `TomeyEmDeviceParser`, Baukasten-Preview-Test und Templatepaket-Test ergaenzt |
| TOMEY | EM-4000 | Endothel-/Zellmessung | Datei/CSV | CSV-Token fuer `[RL_1]`, `[DENSITY_1]`, `[THK_1]`, `[RL_2]`, `[DENSITY_2]`, `[THK_2]`, `[FILE]`; synthetische Fixture | `6228` CD/CCT-Messwerte, `6227` Kommentare, `6302` Bild-/Dateiverweise | BuiltIn Geraet, Exportprofil, Schnittstellenprofil, `TomeyEmDeviceParser`, BuiltIn- und Templatepaket-Tests ergaenzt |
| Shin-Nippon | Accuref R-800 | Autorefraktor | RS232 Text | Referenzlogik fuer R/L-SCA, PD und VD; synthetische Fixture, keine echte Praxisrohdatei | `6228` REF-Zeilen rechts/links | BuiltIn Geraet, Exportprofil, Schnittstellenprofil, `ShinNipponDeviceParser`, Baukasten-Preview-Test und Templatepaket-Test ergaenzt |
| Shin-Nippon | Accuref K-900 | Autorefraktor/Keratometer | RS232 Text | gemeinsame Accuref-Textfamilie fuer REF und KM; synthetische Fixture | `6228` REF-Zeilen, `6221` KM-Zeilen | BuiltIn Geraet, Exportprofil, Schnittstellenprofil, `ShinNipponDeviceParser`, Baukasten-Preview-Test und Templatepaket-Test ergaenzt |
| Shin-Nippon | DL-1000 | Lensmeter | RS232 Text | DL-/SLM-Lensmetertext mit R/L-SCA, ADD, PD und optionalem Prisma; synthetische Fixture | `6228` Lensmeter-Zeilen | BuiltIn Geraet, Exportprofil, Schnittstellenprofil und Templatepaket-Test ergaenzt |
| Shin-Nippon | DL-800 | Lensmeter | RS232 Text | gleiche DL-/SLM-Lensmeterfamilie wie DL-1000; synthetische Fixture | `6228` Lensmeter-Zeilen | BuiltIn Geraet, Exportprofil, Schnittstellenprofil und Templatepaket-Test ergaenzt |
| Shin-Nippon | DL-900 | Lensmeter | RS232 Text | gleiche DL-/SLM-Lensmeterfamilie wie DL-1000; synthetische Fixture | `6228` Lensmeter-Zeilen | BuiltIn Geraet, Exportprofil, Schnittstellenprofil, Baukasten-Kompatibilitaetstest und Templatepaket-Test ergaenzt |
| Shin-Nippon | NCT-200 | Non-Contact-Tonometer | RS232 Text | IOP-Liste mit Durchschnittswerten; synthetische Fixture | `6205` Tonometrie | BuiltIn Geraet, Exportprofil, Schnittstellenprofil, Baukasten-Kompatibilitaetstest und Templatepaket-Test ergaenzt |
| Shin-Nippon | SLM-4000 | Lensmeter | RS232 Text | gleiche DL-/SLM-Lensmeterfamilie wie DL-1000; synthetische Fixture | `6228` Lensmeter-Zeilen | BuiltIn Geraet, Exportprofil, Schnittstellenprofil und Templatepaket-Test ergaenzt |
| TOPCON | CL-300PDL | Lensmeter | Datei/LAN XML | `Ophthalmology`/JOIA-XML mit `Common.ModelName=CL-300PDL`, `Measure Type="LM"`; CL-300-Familie, synthetische neutrale Fixture | `6228` Lensmeter-Zeilen rechts/links | BuiltIn Geraet, Exportprofil, Schnittstellenprofil, ModelName-Alias, Baukasten-Kompatibilitaet und Templatepaket-Test ergaenzt |
| TOPCON | RM-800 | Autorefraktor | Datei/LAN XML | `Ophthalmology`/JOIA-XML mit `Common.ModelName=RM-800`, `Measure Type="REF"`; REF-Familie, synthetische neutrale Fixture | `6228` REF-Zeilen rechts/links | BuiltIn Geraet, Exportprofil, Schnittstellenprofil, ModelName-Alias und Templatepaket-Test ergaenzt |
| TOPCON | TRK-3 Omnia | Autorefraktor/Keratometer/Tonometer/Pachymeter | Datei/LAN XML | `Ophthalmology`/JOIA-XML mit `Common.ModelName=TRK-3`, REF/KM/TM/CCT wie TRK-2P-Familie; synthetische neutrale Fixture | `6228`, `6221`, `6220`, `6205` | BuiltIn Geraet, Exportprofil, Schnittstellenprofil, ModelName-Alias, Baukasten-Kompatibilitaet und Templatepaket-Test ergaenzt |

## Bestehende Anbindungen mit sicherer Verbesserung

| Hersteller | Modell / Familie | Verbesserung |
| --- | --- | --- |
| TOPCON | KR-800 / KR800S | `KR-800` wird als Alias der vorhandenen KR800S-XML-Familie erkannt. Die bestehende KR800S-Anbindung bleibt fachlich unveraendert. |
| TOPCON | CL-300PDL / CL-300 | `CL-300PDL` wird als eigene BuiltIn-Variante der vorhandenen CL-300-Lensmeter-XML-Familie gefuehrt. |
| TOPCON | RM-800 | `RM-800` nutzt eine eigene BuiltIn-Variante der vorhandenen REF-XML-Auswertung ohne KM-Erfindung. |
| TOPCON | TRK-3 Omnia / TRK-2P | `TRK-3`/`TRK-3 Omnia` wird als eigene BuiltIn-Variante der TRK-2P-Mehruntersuchungsfamilie gefuehrt. |
| NIDEK | AR-/LM-/NT-XML-Familien | Die vorhandenen NIDEK-XML-Familien wurden nur alias-tolerant erweitert: AR-1/AR-1S/AR-310A nutzen die bestehende ARMedian-Logik, LM-1800PD nutzt die vorhandene Lensmeter-Fachlogik, NT-1/NT-1E/NT-1P/NT-510/NT-530 nutzen die NT530P-nahe Tono/Pachy-Auswertung. Bestehende validierte NIDEK-Profile bleiben unveraendert. |

## Bereits vorhanden und nicht geaendert

| Hersteller | Geraet / Familie | Grund |
| --- | --- | --- |
| NIDEK | ARK1S, AR360, ARK-510A, ARK-560A, LM7/LM7P, LM-1800P, LM-1800PD, NT530P, NT-1/NT-1E/NT-1P/NT-510/NT-530, RT-2100/3100/5100 RS232, RT-6100 | Als BuiltIn vorhanden; die neu ergaenzten Restfamilienvarianten sind inaktiv, testseitig abgesichert und warten auf echte Praxisdateien. |
| TOPCON | CL300, CL-300PDL, Solos, KR800S, KR-1, RM-800, TRK2P, TRK-3 Omnia, CT1P, CT800A, CV5000/CV5000S | Bereits als BuiltIn vorhanden oder in diesem Batch als risikoarme Variante ergaenzt; CV-5000/CV-5000S wurde statisch bestaetigt, fachlich aber nicht veraendert. |
| Dokumentgeraete | Generisches Dokumentgeraet, manuelle Dokumentauswahl | Bereits als BuiltIn vorhanden; keine Messwertparser betroffen. |

## Parser-Kandidaten aus Referenzlogik

Diese Geraete besitzen auswertbare Parser- oder COM-Logik, aber keine vollstaendig abgesicherte XDTBox-Parserstrecke in diesem Batch. Sie werden nicht verworfen; sie sind die naechsten Kandidaten fuer eigene Parserklassen mit synthetischen Fixtures.

| Hersteller | Modelle | Datenlage | Naechster Schritt |
| --- | --- | --- | --- |
| Huvitz | HLM-1, HLM-7000P, HLM-9000 | Lensmeter-Parserlogik ableitbar, echte Rohdateien fehlen | Lensmeter-Textparser vorbereiten, Prisma/ADD/PD nur bei belegter Regel ausgeben |
| Huvitz | HDR-7000, HDR-9000 | Phoropter-/Refraktor-Kandidaten mit bidirektionaler Relevanz | Rueckgabe und PC->Geraet getrennt analysieren, kein BuiltIn ohne Sendeframe-Test |
| TOMEY | AP-2500 | Setup-/Konfigurationshinweise ohne ausreichend belegte Messwertrohstruktur | erst mit klarer Rohdaten- oder Parserstrecke als BuiltIn entscheiden |
| TOMEY | TAP-2000 | bidirektionaler Phoropterkandidat mit serieller Relevanz; COM-Parameter und Framebausteine sind sichtbar, aber echte Rueckgabe-/Live-Sendeframes fehlen | Rueckgabe und PC->Geraet getrennt mit Live-/Framefixtures absichern |
| TOPCON | EZ-200 Advance | serielle/RDD-Logik mit `CLM`/`SD`-Handshake und Lensmeter-Ziel `6228`, aber keine echte Rohdatenfixture | erst nach echter Rohdaten-/Frameprobe als seriellen Parser umsetzen |
| TOPCON | RM-8000 | serielle/RDD-Logik mit `6228`/`6227`-Hinweisen, aber keine echte Rohdatenfixture | erst nach echter Rohdaten-/Frameprobe als seriellen REF/SBJ-Parser entscheiden |
| NIDEK | AR-600, AR-660A, ARK-500A, LM-970, LM-1000P, NT-2000 | vorwiegend RDD-/COM- beziehungsweise scriptnahe Logik ohne ausreichend belastbare neutrale Rohfixture | Folge-Batch mit eigener Parserklasse erst starten, wenn die Rohstruktur oder ein synthetisch eindeutig herleitbares Frame abgesichert werden kann |
| Reichert | 7CR NCT, LensChek Plus | teilweise Setup-/Parserhinweise | getrennte Tonometer-/Lensmeter-Batches |
| Rodenstock | CX 800, Phoromat 2000 | teilweise Refraktor-/Phoropterhinweise | erst nach klarer Frame-/Textstruktur als BuiltIn |
| Möller-Wedel | Visutron-Familie | teilweise Hinweise, Herstellername korrigiert | Parserdetails und Anschlussart nachziehen |
| Canon | RK-F2, TX-20P, OCT A-1 Xephilo | teilweise Daten; OCT/Bildgeraet gesondert zu bewerten | Messwertgeraete vor Bild-/OCT-Workflows priorisieren |
| ZEISS | IOLMaster/VISU-Familien | teilweise Daten, teils Spezialgeraete | erst nach klarer XDTBox-Messartzuordnung |
| Visionix | Optovue/Retinomax/VX-Familien | Spezialgeraete und gemischte Datenlage | nur bei klarer Messwert- oder Dokumentuebergabe umsetzen |

## Nicht implementiert wegen unzureichender oder widerspruechlicher Datenlage

| Hersteller | Geraet | Grund |
| --- | --- | --- |
| NIDEK | AR-600 / AR-660A / ARK-500A / LM-970 / LM-1000P / NT-2000 | RDD-/COM- beziehungsweise scriptnahe Logik ist sichtbar, aber ohne ausreichend sichere Rohdaten- oder Framefixture nicht als BuiltIn ausgeliefert. |
| NIDEK | ARK-1 / ARK-1E / ARK-1F | ARK-Familie ist technisch nah, aber die vorliegenden Daten reichen fuer eine eigene BuiltIn-ID mit sauberer KM-/REF-Trennung noch nicht aus. |
| NIDEK | CEM-530 | Endothel-/Zellmessgeraet-Kandidat; Messwerte, Kommentare und Bild-/Dateiverweise sind ohne echte Rohdaten und eindeutige Exportfelder noch nicht belastbar genug. |
| NIDEK | TONOREF II / TONOREF III | Kombigeraete-Kandidaten fuer REF/KM/Tonometrie/Pachymetrie; die Teilmessarten werden erst mit echter Datei oder eindeutigem Parserfixture als BuiltIn getrennt. |
| Haag-Streit | Refractor-900 | Phoropterkandidat ohne ausreichende PC->Geraet- und Rueckgabe-Absicherung. |
| Huvitz | HDR-7000 / HDR-9000 | Bidirektionaler Phoropter-/Refraktorbereich; Rueckgabe- und PC->Geraet-Frames sind noch nicht sicher genug fuer ein BuiltIn. |
| Huvitz | HLM-1 / HLM-7000P / HLM-9000 | Lensmeter-Familie mit erkennbarer COM-/Parserlogik, aber ohne ausreichend abgesicherte Rohdatenstruktur fuer ADD/PD/Prisma und MEDISTAR-`6228`. |
| TOMEY | AP-2500 | Keine ausreichend belastbare Messwertrohstruktur fuer einen XDTBox-Parser. |
| TOMEY | TAP-2000 | Phoropter-/Sendeframe-Workflow waere zu risikoreich ohne echte Rueckgabe- und PC->Geraet-Validierung. Die statisch sichtbaren COM-/Framefragmente werden dokumentiert, aber noch nicht produktiv freigegeben. |
| Shin-Nippon | DR-900 | Refraktions-/Phoropterkandidat mit bidirektionaler Relevanz; Anschluss- und Sendeframe-Datenlage reicht nicht fuer ein sicheres BuiltIn. Kein halbfertiger produktiver Parser ohne getrennte Rueckgabe- und PC->Geraet-Validierung. |

## Uebernommene technische Regeln

- NIDEK ARK-5xx: einfache `Data`-XML-Struktur wird ueber die bestehende XML-Flattening-Logik verarbeitet; Exportprofile nutzen `ARMedian` fuer rechts/links, `PD/PDList[@No='1']/FarPD` und `VD`.
- NIDEK AR-1/AR-1S/AR-310A: die AR-XML-Familie nutzt `ARMedian` fuer `6228`; AR-1S ergaenzt `SR` als subjektive Refraktion nach `6227`. Es wird kein `6330` und kein kuenstlicher Trenner erzeugt.
- NIDEK LM-1800P/LM-1800PD: die Schreibvariante `Sphare` wird ueber vorhandene Aliaslogik als `Sphere` verfuegbar; alternative Augenbloecke `I` und `S` werden fuer die vorbereiteten MEDISTAR-Lines auf `R` und `L` abgebildet.
- NIDEK NT-1/NT-1E/NT-1P/NT-510/NT-530: die NT530P-nahe XML-Familie erzeugt nur `6205` Tonometrie und `6220` Pachymetrie. Es wird keine Refraktionsausgabe geraten.
- TOPCON KR-800: Modellalias `KR-800` nutzt die vorhandene KR800S-Erkennung, ohne die bestehende KR800S-Fachlogik zu veraendern.
- TOPCON CL-300PDL/RM-800/TRK-3 Omnia: nur eindeutige XML-Familienaliasse wurden uebernommen. CL-300PDL bleibt Lensmeter `6228`, RM-800 bleibt REF `6228`, TRK-3 Omnia bleibt REF/KM/CCT/Tonometrie ueber `6228`/`6221`/`6220`/`6205`. Es wurden keine Cross-Device-Messarten geraten.
- Huvitz-Textparser: nur eindeutig abgeleitete Tokens werden ausgewertet. `S-R-*` wird als REF nach `6228`, `S-K-*` als KM nach `6221`, `T-*` als Tonometrie nach `6205` und `P-*` als Pachymetrie/CCT nach `6220` vorbereitet. Die Fixtures sind synthetisch aus Referenzlogik abgeleitet und ersetzen keine Praxisrohdateien.
- TOMEY-Parser: CF-2000 und TL-2000C/TL-6000/TL-7000 werden als Lensmeter nach `6228` vorbereitet. MR-6000 trennt REF nach `6228`, KM nach `6221`, Tonometrie nach `6205` und Pachymetrie/CCT nach `6220`. TOP-1000 liefert Tonometrie nach `6205` und Pachymetrie nach `6220`. EM-3000/EM-4000 liefern Endothel-/Zellmesswerte nach `6228`, Kommentare nach `6227` und Bild-/Dateiverweise nach `6302`. Nur erkannte Werte werden exportiert; fehlende Teilmessungen erzeugen keine kuenstlichen Zeilen.
- Shin-Nippon-Textparser: Accuref R-800/K-900 wird als REF beziehungsweise REF/KM ausgewertet, DL-1000/DL-800/DL-900/SLM-4000 als Lensmeter und NCT-200 als Tonometrie. COM-Defaults stammen aus neutraler Referenzlogik: Accuref 115200 8N1, DL-/SLM-Lensmeter 9600 8N1, NCT-200 19200 8N1. Die Fixtures sind synthetisch und ersetzen keine Praxisrohdateien.
- Scriptbasierte Textparser: Feldkennungen wie `6228`, `6227`, `6221`, `6220` und `6205` werden als Hinweise dokumentiert, aber nicht blind als XDTBox-Code kopiert.
- Externe Referenzartefakte wurden nicht in das Repository uebernommen. Ein Repo-Scan prueft die vier gesperrten externen Marker dynamisch und ohne Klartextspeicherung.

## Offene Folgeschritte

1. Echte Huvitz-Praxisrohdateien fuer HRK-8000A/HRK-9000A/HNT-1P/HTR-1A sammeln und gegen die synthetischen Fixtures validieren.
2. Echte TOMEY-Praxisrohdateien fuer CF-2000/TL-2000C/TL-6000/TL-7000/MR-6000/TOP-1000 sowie EM-3000/EM-4000 sammeln und gegen `TomeyDeviceParser` beziehungsweise `TomeyEmDeviceParser` validieren.
3. Huvitz-HLM-Lensmeter, Huvitz-HDR-Phoropter und TOMEY-TAP-2000 erst nach klarer Rohdaten-/Frame-Struktur als BuiltIn entscheiden.
4. Echte Shin-Nippon-Praxisrohdateien fuer Accuref R-800/K-900, DL-1000/DL-800/DL-900, NCT-200 und SLM-4000 sammeln und gegen `ShinNipponDeviceParser` validieren.
5. Shin-Nippon DR-900 erst nach klarer Rueckgabe- und PC->Geraet-Frame-Struktur entscheiden.
6. TOPCON EZ-200 Advance und RM-8000 erst mit echter serieller Rohdatenprobe oder eindeutigem Framefixture als Parser umsetzen.
7. Widerspruechliche TOPCON-/NIDEK-Aliasfaelle erst mit echter Rohdatei oder eindeutiger Herstellerstruktur entscheiden.
8. Keine Referenzrohdateien, PDFs oder fremden Skripte in den Kundeninstaller aufnehmen.
