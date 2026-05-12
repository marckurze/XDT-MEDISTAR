# Geraeteprofile und Templatepakete - Matrix

Stand: 2026-05-12

Ziel dieser Matrix ist eine schlanke Priorisierung: Anwender sollen moeglichst fertige Geraeteprofile und Templatepakete nutzen, statt den Baukasten als Normalweg zu brauchen. Der Baukasten bleibt wichtig fuer Sonderfaelle, Tests, Vorschau und kundenspezifische Anpassungen.

## Kurzbestand

- Praktisch validiert ist `MEDISTAR + NIDEK ARK1S`, inklusive XDT-Anhang-Link ueber `6302`, `6303`, optional `6304` und `6305`.
- BuiltIn-Geraeteprofile existieren fuer NIDEK ARK1S, NIDEK LM7, NIDEK NT530P, TOPCON CL300, TOPCON KR800 und TOPCON TRK2P.
- BuiltIn-MEDISTAR-Exportprofile existieren fuer dieselben sechs Geraete.
- Ein fertiges BuiltIn-Schnittstellenprofil existiert aktuell nur fuer `MEDISTAR + NIDEK ARK1S`.
- Die Templatepaket-Infrastruktur ist vorhanden und testseitig abgesichert. Fuer MEDISTAR + NIDEK ARK1S gibt es die offizielle Paketvorlage `docs/TEMPLATEPAKET_MEDISTAR_NIDEK_ARK1S.md`; der Export erfolgt selektiv aus dem Schnittstellenprofil und nimmt nur die benoetigten Abhaengigkeiten auf. ZIP-Struktur, Import/DryRun und sicherer UserDefined-Import sind reproduzierbar im Test geprueft. Ein dauerhaftes ZIP-Release-Artefakt ist noch nicht eingecheckt.
- Repository-Testdaten enthalten `nidek-ark1s-sample.xml` sowie generische GDT-Beispiele; fuer LM7/LM7P, NT530P und TOPCON liegen im Repository keine vollstaendigen Geraete-Beispieldateien.

## Matrix

| Hersteller | Geraet | Typ | Status | Parser/Reader | Geraeteprofil | Tests | Templatepaket | AIS-Ziel | XDT-Anhang | Naechste sinnvolle Massnahme | Risiko / Unsicherheit |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| NIDEK | ARK1S | Autorefraktor | produktiv validiert | `XmlDeviceParser`, ARK1S-Testdatei | BuiltIn `device-nidek-ark1s-default` plus altes Standardprofil | Core-/Pipeline-, DeviceProfile-, ExportProfile-, selektive ARK1S-TemplatePackage-Tests | offizielle Vorlage vorhanden; selektiver Export/Import reproduzierbar testgeprueft; ZIP-Release-Artefakt noch offen | MEDISTAR | validiert fuer externen Link | Release-Ablage fuer ZIP-Artefakt festlegen und praktische App-Importabnahme durchfuehren | Nicht durch neue Profil-/Templatearbeit regressieren |
| NIDEK | LM7/LM7P | Lensmeter | vorbereitet / dokumentiert | XML-SourcePaths dokumentiert; Parser generisch XML | BuiltIn `device-nidek-lm7-default` | Definitionen und Exportprofil testseitig validiert | fehlt | MEDISTAR | optional / nicht geraetespezifisch validiert | Repraesentative LM7/LM7P-Dateien ins Testdatenkonzept bringen und fertiges Paket bauen | Linkes Auge, PD, Prisma-Notation und `Sphare`/`Sphere`-Variante brauchen Praxisabgleich |
| NIDEK | NT530P | Tonometer / Pachymeter | vorbereitet / dokumentiert | XML-SourcePaths dokumentiert; Anhangbezug fachlich relevant | BuiltIn `device-nidek-nt530p-default` | Definitionen und Exportprofil testseitig validiert | fehlt | MEDISTAR | ja, wegen JPG-/Bildverweisen relevant, aber nicht produktiv validiert | Nach LM7 Datenlage pruefen: echte XML/JPG-Beispiele, Anhangfall und MEDISTAR-Ausgabe validieren | Attachment-Zuordnung, korrigierter IOP und Pachymetrieausgabe koennen fachlich abweichen |
| TOPCON | CL300 | Lensmeter | vorbereitet / dokumentiert | JOIA/Ophthalmology-XML; Namespace-Normalisierung noch Risiko | BuiltIn `device-topcon-cl300-default` | Definitionen und Exportprofil testseitig validiert | fehlt | MEDISTAR | eher nein / optional | Echte CL300-Dateien sammeln und Namespace-/Measure-Pfade pruefen | Parser-Namespace-Verhalten und Prisma/Additionswerte sind noch nicht produktiv bestaetigt |
| TOPCON | KR800 | Autorefraktor / Keratometer | vorbereitet / dokumentiert | JOIA/Ophthalmology-XML; Mehruntersuchungen REF/KM/SBJ | BuiltIn `device-topcon-kr800-default` | Definitionen und Exportprofil testseitig validiert | fehlt | MEDISTAR | eher nein / optional | Nach Datenlage REF zuerst validieren, KM/SBJ getrennt pruefen | Mehruntersuchungsdateien koennen falsch gruppiert oder unvollstaendig gemappt werden |
| TOPCON | TRK2P | Tonometer / Pachymeter | vorbereitet / dokumentiert | JOIA/Ophthalmology-XML; TM/CCT-Strukturen | BuiltIn `device-topcon-trk2p-default` | Definitionen und Exportprofil testseitig validiert | fehlt | MEDISTAR | eher nein / optional | Echte TM/CCT-Dateien pruefen und Einheit/Umrechnung fachlich bestaetigen | CCT-Werte in mm vs. MEDISTAR-Zielbild in um/µm sind nicht produktiv abgenommen |

## V1-Prioritaeten

1. MEDISTAR + NIDEK ARK1S sauber halten: keine Regression am validierten Kernworkflow, reproduzierbaren Export-/Import-Testweg beibehalten und erst danach ein offizielles ZIP-Release-Artefakt ablegen.
2. NIDEK LM7/LM7P praktisch nutzbar machen: vorhandene Definitionen mit repraesentativen Dateien validieren und daraus ein fertiges Profil-/Templatepaket erstellen.
3. Danach nach Datenlage entscheiden: NIDEK NT530P ist fachlich wertvoll wegen Tonometrie/Pachymetrie und Anhangbezug; TOPCON CL300/KR800/TRK2P folgen nur mit belastbaren Beispiel- und Testdaten.

## Luecken fuer direkte Nutzung ohne Baukasten

- Fertige Templatepaket-Dateien fehlen noch; fuer ARK1S sind Vorlage, selektiver Export und technischer Export-/Import-Testweg vorhanden.
- Fuer alle vorbereiteten Geraete ausser ARK1S fehlen Repository-Testdaten oder praktische Abnahmeprotokolle.
- Ein BuiltIn-Schnittstellenprofil existiert nur fuer MEDISTAR + NIDEK ARK1S.
- Bei TOPCON ist Namespace-/JOIA-Verhalten noch gezielt mit echten Dateien zu pruefen.
- Bei NT530P und TRK2P sind Tonometrie/Pachymetrie-Ausgabe, Einheiten und optionale Anhaenge fachlich zu validieren.

## Rolle des Baukastens

Der Standardweg soll ein fertiges Geraeteprofil plus fertiges Templatepaket sein. Der Baukasten bleibt fuer Sonderfaelle, Tests, Vorschau und kundenspezifische Anpassungen. Neue Arbeit soll deshalb zuerst konkrete Profile, Tests, Beispielpakete und Validierungsprotokolle liefern, nicht zusaetzliche abstrakte Assistenten.
