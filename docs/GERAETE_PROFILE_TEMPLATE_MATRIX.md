# Geraeteprofile und Templatepakete - Matrix

Stand: 2026-05-18

Ziel dieser Matrix ist eine schlanke Priorisierung: Anwender sollen moeglichst fertige Geraeteprofile und Templatepakete nutzen, statt den Baukasten als Normalweg zu brauchen. Der Baukasten bleibt wichtig fuer Sonderfaelle, Tests, Vorschau und kundenspezifische Anpassungen.

## Kurzbestand

- Praktisch validiert ist `MEDISTAR + NIDEK ARK1S`, inklusive XDT-Anhang-Link ueber `6302`, `6303`, optional `6304` und `6305`.
- Praktisch validiert ist neu auch `MEDISTAR + NIDEK AR360` fuer Auto-Refraktor-XDT-Rueckgabe aus AR360-XML; `8402 = AR360` und die `6228`-Zeilen rechts/links wurden in MEDISTAR uebernommen.
- Praktisch validiert ist `MEDISTAR + NIDEK LM7` fuer Lensmeter-XDT-Rueckgabe aus echter LM7-XML; `8402` kommt aus AIS, und die rechten/linken `6228`-Lensmeter-Zeilen wurden in MEDISTAR uebernommen.
- BuiltIn-Geraeteprofile existieren fuer NIDEK ARK1S, NIDEK AR360 / AR-360A, NIDEK LM7, NIDEK NT530P, TOPCON CL300, TOPCON KR800 und TOPCON TRK2P.
- BuiltIn-MEDISTAR-Exportprofile existieren fuer dieselben sieben Geraete.
- Fertige BuiltIn-Schnittstellenprofile existieren aktuell fuer `MEDISTAR + NIDEK ARK1S`, `MEDISTAR + NIDEK AR360` und `MEDISTAR + NIDEK LM7`.
- Die Templatepaket-Infrastruktur ist vorhanden und testseitig abgesichert. Fuer MEDISTAR + NIDEK ARK1S, MEDISTAR + NIDEK AR360 und MEDISTAR + NIDEK LM7 gibt es Referenz- beziehungsweise Kandidatendokumentation; der Export erfolgt selektiv aus dem Schnittstellenprofil und nimmt nur die benoetigten Abhaengigkeiten auf. ZIP-Struktur, Import/DryRun und sicherer UserDefined-Import sind reproduzierbar im Test geprueft. Dauerhafte ZIP-Release-Artefakte werden erst nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md` abgelegt.
- Fuer Sonderfaelle koennen AIS-, Geraete- und Exportprofile schlank als UserDefined aus der App angelegt werden. Das ersetzt noch keinen gefuehrten Geraete-Datei-Explorer und fuehrt keine automatische Aktivierung oder Schnittstellenprofil-Aenderung aus.
- Repository-Testdaten enthalten `nidek-ark1s-sample.xml`, AR360-Beispielfixtures, die echte LM7-XML-Fixture `NIDEK LM7.xml`, das zu ignorierende `NIDEK_LM_Stylesheet.xsl` sowie generische GDT-Beispiele; fuer NT530P und TOPCON liegen im Repository noch keine vollstaendigen Geraete-Beispieldateien.

## Matrix

| Hersteller | Geraet | Typ | Status | Parser/Reader | Geraeteprofil | Tests | Templatepaket | AIS-Ziel | XDT-Anhang | Naechste sinnvolle Massnahme | Risiko / Unsicherheit |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| NIDEK | ARK1S | Autorefraktor | produktiv validiert, Referenzpaket 1 | `XmlDeviceParser`, ARK1S-Testdatei | BuiltIn `device-nidek-ark1s-default` plus altes Standardprofil | Core-/Pipeline-, DeviceProfile-, ExportProfile-, selektive ARK1S-TemplatePackage-Tests | Referenzpaket dokumentiert; selektiver Export/Import reproduzierbar testgeprueft; ZIP-Release-Artefakt noch offen | MEDISTAR | validiert fuer externen Link | Release-Regel anwenden und praktische App-Importabnahme fuer ZIP-Artefakt durchfuehren | Nicht durch neue Profil-/Templatearbeit regressieren |
| NIDEK | AR360 / AR-360A | Autorefraktor | praktisch validiert fuer Auto-Refraktor-XDT-Rueckgabe, Referenzpaket 2 | `XmlDeviceParser`, NIDEK-LAN-XML mit ARMedian | BuiltIn `device-nidek-ar360-default` plus BuiltIn-Schnittstellenprofil | AR360-Fixtures, UTF-16-Lesetest, MEDISTAR-XDT-Erwartungswerte und AR360-TemplatePackage-Tests | Referenzpaket dokumentiert; selektiver Export/Import reproduzierbar testgeprueft; ZIP-Release-Artefakt noch offen | MEDISTAR | in diesem AR360-Test nicht neu validiert | Release-Regel anwenden und ggf. Anhangfall separat entscheiden | Validiert ist Auto-Refraktor-Rueckgabe; XDT-Anhang-Link bleibt fuer AR360 separat offen |
| NIDEK | LM7 / LM-7P | Lensmeter | praktisch validiert fuer Lensmeter-XDT-Rueckgabe, Referenzkandidat 3 | `XmlDeviceParser`, NIDEK-LAN-XML `NIDEK_V1.00/V1.01`, akzeptiert `Sphere/Sphare` und `NearSphere/NearSphare` | BuiltIn `device-nidek-lm7-default` plus BuiltIn-Schnittstellenprofil | echte LM7-Fixture, Stylesheet-Ignoriertest, MEDISTAR-Lensmeter-Ausgabe, Reparatur alter BuiltIn-LM7-Exportpfade, BuiltIn- und selektive LM7-TemplatePackage-Tests | Kandidat dokumentiert; selektiver Export/Import reproduzierbar testgeprueft; ZIP-Release-Artefakt noch offen | MEDISTAR | nein / nicht separat validiert | Praxisprotokoll pflegen, Prisma-/PD-Faelle sammeln und danach ggf. Release-Regel anwenden | Prisma-/PD-Faelle bleiben datenabhaengig offen; keine Werte aus Formatbeispielen uebernehmen |

## Referenzpakete

| Workflow | AIS-Ziel | BuiltIn-Geraeteprofil | BuiltIn-Exportprofil | BuiltIn-Schnittstellenprofil | Templatepaket-Kandidat | Export-/Importtest | praktische MEDISTAR-Validierung | XDT-Anhang-Link validiert | offizielles ZIP-Artefakt | Naechster Schritt |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| MEDISTAR + NIDEK ARK1S | MEDISTAR | `device-nidek-ark1s-default` | `export-medistar-nidek-ark1s-default` | `interface-medistar-nidek-ark1s-default` | ja, `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_ARK1S.md` | ja, `MedistarNidekArk1sTemplatePackageTests` | ja | ja, `6302` bis `6305` | nein | Release-Regel anwenden, App-Importabnahme, dann ZIP ablegen |
| MEDISTAR + NIDEK AR360 | MEDISTAR | `device-nidek-ar360-default` | `export-medistar-nidek-ar360-default` | `interface-medistar-nidek-ar360-default` | ja, `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_AR360.md` | ja, `MedistarNidekAr360TemplatePackageTests` | ja, Auto-Refraktor-XDT-Rueckgabe | nein, nicht separat validiert | nein | Release-Regel anwenden, optional XDT-Anhangfall testen |
| MEDISTAR + NIDEK LM7 | MEDISTAR | `device-nidek-lm7-default` | `export-medistar-nidek-lm7-default` | `interface-medistar-nidek-lm7-default` | ja, `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_LM7.md` | ja, `MedistarNidekLm7TemplatePackageTests` | ja, `docs/E2E_TESTPROTOKOLL_MEDISTAR_LM7.md` | nein, nicht separat validiert | nein | Prisma-/PD-Faelle sammeln, danach ZIP-Release nach Regel entscheiden |

## Weitere vorbereitete Geraete

| Hersteller | Geraet | Typ | Status | Parser/Reader | Geraeteprofil | Tests | Templatepaket | AIS-Ziel | XDT-Anhang | Naechste sinnvolle Massnahme | Risiko / Unsicherheit |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| NIDEK | NT530P | Tonometer / Pachymeter | vorbereitet / dokumentiert | XML-SourcePaths dokumentiert; Anhangbezug fachlich relevant | BuiltIn `device-nidek-nt530p-default` | Definitionen und Exportprofil testseitig validiert | fehlt | MEDISTAR | ja, wegen JPG-/Bildverweisen relevant, aber nicht produktiv validiert | Nach LM7 Datenlage pruefen: echte XML/JPG-Beispiele, Anhangfall und MEDISTAR-Ausgabe validieren | Attachment-Zuordnung, korrigierter IOP und Pachymetrieausgabe koennen fachlich abweichen |
| TOPCON | CL300 | Lensmeter | vorbereitet / dokumentiert | JOIA/Ophthalmology-XML; Namespace-Normalisierung noch Risiko | BuiltIn `device-topcon-cl300-default` | Definitionen und Exportprofil testseitig validiert | fehlt | MEDISTAR | eher nein / optional | Echte CL300-Dateien sammeln und Namespace-/Measure-Pfade pruefen | Parser-Namespace-Verhalten und Prisma/Additionswerte sind noch nicht produktiv bestaetigt |
| TOPCON | KR800 | Autorefraktor / Keratometer | vorbereitet / dokumentiert | JOIA/Ophthalmology-XML; Mehruntersuchungen REF/KM/SBJ | BuiltIn `device-topcon-kr800-default` | Definitionen und Exportprofil testseitig validiert | fehlt | MEDISTAR | eher nein / optional | Nach Datenlage REF zuerst validieren, KM/SBJ getrennt pruefen | Mehruntersuchungsdateien koennen falsch gruppiert oder unvollstaendig gemappt werden |
| TOPCON | TRK2P | Tonometer / Pachymeter | vorbereitet / dokumentiert | JOIA/Ophthalmology-XML; TM/CCT-Strukturen | BuiltIn `device-topcon-trk2p-default` | Definitionen und Exportprofil testseitig validiert | fehlt | MEDISTAR | eher nein / optional | Echte TM/CCT-Dateien pruefen und Einheit/Umrechnung fachlich bestaetigen | CCT-Werte in mm vs. MEDISTAR-Zielbild in um/µm sind nicht produktiv abgenommen |

## V1-Prioritaeten

1. MEDISTAR + NIDEK ARK1S sauber halten: keine Regression am validierten Kernworkflow, reproduzierbaren Export-/Import-Testweg beibehalten und erst danach ein offizielles ZIP-Release-Artefakt ablegen.
2. NIDEK AR360 als zweiten praktischen Referenzworkflow sauber halten: Auto-Refraktor-Rueckgabe ist validiert; Templatepaket-ZIP-Release und ggf. Anhangfall separat entscheiden.
3. NIDEK LM7/LM7P als dritten Referenzkandidaten sauber halten: Lensmeter-XDT-Rueckgabe ist praktisch validiert; Prisma-/PD-Faelle und offizielles ZIP fehlen noch.
4. Danach nach Datenlage entscheiden: NIDEK NT530P ist fachlich wertvoll wegen Tonometrie/Pachymetrie und Anhangbezug; TOPCON CL300/KR800/TRK2P folgen nur mit belastbaren Beispiel- und Testdaten.

## Luecken fuer direkte Nutzung ohne Baukasten

- Fertige Templatepaket-Dateien fehlen noch; fuer ARK1S, AR360 und LM7 sind Vorlage/Kandidat, selektiver Export und technischer Export-/Import-Testweg vorhanden.
- Fuer AR360 gibt es Repository-Testfixtures und ein praktisches MEDISTAR-Abnahmeprotokoll fuer Auto-Refraktor-XDT-Rueckgabe; XDT-Anhang-Link und offizielles ZIP-Release-Artefakt bleiben offen.
- Fuer LM7/LM7P ist die Lensmeter-XDT-Rueckgabe praktisch validiert; offen bleiben weitere Prisma-/PD-Beispielfaelle, ein separat validierter XDT-Anhang-Link und ein offizielles ZIP-Release-Artefakt. Fuer NT530P und TOPCON fehlen Repository-Testdaten oder praktische Abnahmeprotokolle.
- Bei TOPCON ist Namespace-/JOIA-Verhalten noch gezielt mit echten Dateien zu pruefen.
- Bei NT530P und TRK2P sind Tonometrie/Pachymetrie-Ausgabe, Einheiten und optionale Anhaenge fachlich zu validieren.

## Rolle des Baukastens

Der Standardweg soll ein fertiges Geraeteprofil plus fertiges Templatepaket sein. Der Baukasten bleibt fuer Sonderfaelle, Tests, Vorschau und kundenspezifische Anpassungen. Neue Arbeit soll deshalb zuerst konkrete Profile, Tests, Beispielpakete und Validierungsprotokolle liefern, nicht zusaetzliche abstrakte Assistenten.
