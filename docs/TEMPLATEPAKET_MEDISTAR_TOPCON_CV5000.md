# Templatepaket-Kandidat: MEDISTAR + TOPCON CV-5000 / CV-5000S

Stand: 2026-05-23

Dieses Paket beschreibt den ersten bidirektionalen TOPCON-Phoropter-Kandidaten fuer `MEDISTAR + TOPCON CV-5000 / CV-5000S`.

## Status

- Testseitig abgesichert fuer beide Richtungen:
  - AIS/MEDISTAR -> XdtDeviceBridge -> TOPCON-CV-5000-Import-XML
  - TOPCON CV-5000 -> XdtDeviceBridge -> MEDISTAR-XDT-Rueckgabe
- Auswahlfenster und Importdatei-Erzeugung sind im Verarbeitungslauf angebunden. Der Rueckweg verarbeitet MEDISTAR-Historien-AIS-Dateien tolerant, damit Karteikartenzeilen den CV-5000-Rueckexport nicht abbrechen. Nach praktischem MEDISTAR-Importtest wird `Prescription` vollstaendig ueber `6228` und `Full Correction` vollstaendig ueber `6227` ausgegeben; `6330` wird fuer CV-5000 nicht mehr verwendet. Erfolgreiche Rueckgabezyklen erzeugen keinen zusaetzlichen AlreadyProcessed-Fehlernachlauf; neue Versionen gleichnamiger AIS-Dateien wie `Patient.gdt` koennen das Auswahlfenster erneut oeffnen. Der MEDISTAR-Import der erzeugten Rueckgabedatei ist mit dieser Feldzuordnung praktisch bestaetigt.
- Offizielles ZIP-Artefakt wird erst nach der Release-Regel abgelegt.

## Enthaltene BuiltIns

- AIS-Profil: `ais-medistar-default`
- Geraeteprofil: `device-topcon-cv5000-default`
- Exportprofil: `export-medistar-topcon-cv5000-default`
- Schnittstellenprofil: `interface-medistar-topcon-cv5000-default`

## Geraet

- Hersteller: TOPCON
- Modell: CV-5000 / CV-5000S
- Typ: Phoropter
- Dateityp: XML, Dateiendung `.xml` / `.XML`
- Root: `Ophthalmology`
- Erkennung Rueckgabe: `Common/Company = TOPCON`, `Common/ModelName = CV-5000` oder `CV-5000S`, `Measure type="SBJ"`
- Namespaces: `nsCommon`, `nsSBJ`, optional `nsREF`, `nsLM`

## Richtung AIS -> Phoropter

Die App liest Patientendaten und historische MEDISTAR-Karteikartenzeilen aus der AIS-Datei.

Relevante MEDISTAR-Praefixe:

- `V0` Lensmeter
- `V1` Autorefraktor
- `V2` Phoropter
- `V3` Brillenrezept
- `V4` Autorefraktor subjektiv
- `V7` Keratometer, nur erkennen/anzeigen
- `P` Pachymetrie, nur erkennen/anzeigen
- `Y` Tonometrie, nur erkennen/anzeigen

Exportiert werden aktuell nur refraktive, parsebare Datensaetze `V0` bis `V4`. Bei aktiver `Ausgabe an Geraet` oeffnet der Verarbeitungslauf nach AIS-Datei-Eingang das Fenster `Werte an Phoropter uebergeben`; dort sind die neuesten exportierbaren Datensaetze fuer Lensmeter, Autorefraktor und Phoropter vorausgewaehlt und weitere refraktive Datensaetze koennen zusaetzlich markiert werden. Das Fenster startet TopMost mit dem bestehenden `🔝`-Schalter, damit es nicht vom Geraetefenster ueberdeckt wird.

Der Button `Nichts senden` schreibt keine `CVImport.xml` und ist kein Fehlerfall. Er dokumentiert die bewusste Entscheidung, ohne Startwerte am Phoropter weiterzuarbeiten; der Workflow wartet danach weiter auf die spaetere CV-5000-Rueckgabedatei. `Abbrechen` bleibt davon getrennt.

Die erzeugte CV-5000-Import-XML orientiert sich an `CVImport.xml`:

- `Ophthalmology`
- `nsCommon:Common`
- `nsSBJ:Measure type="SBJ"`
- `nsSBJ:RefractionTest`
- je ausgewaehltem Datensatz ein `Type`
- `ExamDistance No="1"` mit `Distance 500.000 cm`
- R/L `Sph`, `Cyl`, `Axis`
- `VD` und `PD/B`, wenn vorhanden
- keine kuenstlichen leeren zweiten ExamDistance-Bloecke

Die Fernentfernung `500.000 cm` ist bewusst konservativ gewaehlt und muss im Livebetrieb mit CV-5000/CV-5000S bestaetigt werden.

## Richtung Phoropter -> AIS

Die CV-5000-Rueckgabe nutzt intern TOPCON-`Measure type="SBJ"`, wird fuer MEDISTAR aber fachlich nach TypeName getrennt.

- `8402` Untersuchungsart kommt aus AIS/MEDISTAR.
- MEDISTAR-Historien-AIS-Dateien mit Karteikartenzeilen werden im CV-5000-Rueckweg tolerant als Patientenkontext gelesen.
- Die `CVImport.xml`-Erzeugung ist Phase 1 und kein terminaler Workflowabschluss; final abgeschlossen wird erst nach erfolgreicher Phoropter-Rueckgabe.
- `Prescription` enthaelt den finalen Verordnungswert: `6228`-Ueberschrift plus rechte/linke `6228`-Zeilen.
- `Full Correction` enthaelt den Maximalwert / die Vollkorrektion: `6227`-Ueberschrift plus rechte/linke `6227`-Zeilen.
- `6330` wird fuer CV-5000/CV-5000S nicht mehr verwendet; es gibt keine generische Zeilentypautomatik.
- `6221`, `6220`, `6205` und `6302` bis `6305` werden fuer CV-5000-Messwerte nicht verwendet.
- Leere Prism-/VA-/Contrast-Bloecke werden ignoriert.
- Fehlende Augen oder optionale Werte erzeugen keine leeren Fragmente.

Beispiel aus der echten CV-5000-Rueckgabe-Fixture:

```text
6228 Phoropter finaler Verordnungswert
6228 R.:S=+ 1.25 Z=- 2.00*  7 PD= 59 VD= 13.75
6228 L.:S=+ 1.25 Z=- 2.00*  7
6227 Phoropter Maximalwert (Vollkorrektion)
6227 R.:S=+ 1.25 Z=- 2.00*  7 PD= 59 VD= 13.75
6227 L.:S=+ 1.25 Z=- 2.00*  7
```

Die bisherige Trennzeile `--` wird nicht mehr verwendet; die Trennung erfolgt ueber die fachlichen Ueberschriften innerhalb der jeweiligen Feldkennung.

## Testfixtures

- `XdtDeviceBridge.Tests/TestData/Devices/Topcon/CV5000/CVImport.xml`
- `XdtDeviceBridge.Tests/TestData/Devices/Topcon/CV5000/M-Serial1234_20130625_170509656_TOPCON_CV-5000_10111.xml`
- `XdtDeviceBridge.Tests/TestData/Devices/Topcon/CV5000/Patient_mit_Phoropter_Daten.XDT`

Die Fixtures enthalten keine dokumentierten Live-Pfade. Patientendaten werden in Dokumentation anonymisiert oder als Testdaten behandelt.

## UI-/Konfigurationsstand

Neue UserDefined-Geraete markieren nur noch die Faehigkeit `Bidirektionales Geraet, z. B. Phoropter`. Ausgabeordner, Dateiname und Format gehoeren nicht ins Geraeteprofil.

Die konkrete Richtung AIS/MEDISTAR -> Geraet wird im Schnittstellenprofil konfiguriert. Fuer CV-5000/CV-5000S ist dort der Bereich `Ausgabe an Geraet` sichtbar mit Aktiv-Schalter, Ausgabeordner, Dateiname `CVImport.xml` und Format `TOPCON CV-5000 XML`. Bleibt der Ausgabeordner leer, oeffnet sich kein stiller Erfolgsweg: Es wird keine Importdatei an das Geraet geschrieben und ein Hinweis ausgegeben.

Der Bereich `XDT-Anhaenge fuer AIS` ist bei CV-5000/CV-5000S bewusst ausgeblendet, weil er fuer diesen Workflow nicht relevant ist. Fuer andere Geraete bleibt die XDT-Anhang-Konfiguration unveraendert sichtbar.

Das BuiltIn-Geraeteprofil CV-5000/CV-5000S ist als bidirektional-faehig markiert. Es erfolgt keine automatische Aktivierung und keine automatische Aenderung bestehender UserDefined-Profile.

Der Duplicate-Schutz bleibt aktiv, ist aber dateiversionsbezogen: identische AIS-/Phoropter-Dateipaare werden nicht doppelt exportiert; eine neu geschriebene `Patient.gdt` mit gleicher Dateiablage startet wieder einen neuen CV-5000-Zyklus ohne Reset.

Nach Phase 1 wird fuer CV-5000/CV-5000S eine Baseline der bereits vorhandenen Phoropter-Dateien gehalten. Dadurch werden alte Rueckgabedateien nicht erneut gekoppelt, waehrend eine neue oder ueberschriebene Rueckgabedatei den finalen MEDISTAR-Export startet, auch wenn ihr Dateizeitstempel nicht neuer als die AIS-Datei ist.

## Tests

- Historienparser: `TopconCv5000ProfileTests`
- CV-5000-Importwriter: `TopconCv5000ProfileTests`
- CV-5000-Rueckgabeparser und MEDISTAR-Export mit `6228`-Prescription und `6227`-Full-Correction: `TopconCv5000ProfileTests`
- CV-5000-Rueckweg mit MEDISTAR-Historien-AIS und genauer AIS-Fehlerdiagnose: `InterfaceProfileManualProcessorTests`
- BuiltIn-Profiltests: `DeviceProfileDefinitionTests`, `ExportProfileDefinitionTests`, `InterfaceProfileDefinitionTests`, `ProfileCatalogServiceTests`
- Schnittstellenprofil-UI-Sichtbarkeit: `InterfaceProfileUiPolicyTests`
- CV-5000-Auswahldialog-Zustand und Dialogentscheidung: `Cv5000PhoropterSelectionStateTests`
- Selektiver Templatepaket-Test: `MedistarTopconCv5000TemplatePackageTests`

## Grenzen / offen

- Im Livebetrieb weiter zu beobachten bleibt der Folgezyklus ohne Reset mit gleichnamiger neu geschriebener AIS-Datei und spaeterer Phoropter-Rueckgabe.
- Die vom Dialog erzeugte `CVImport.xml` muss am echten CV-5000/CV-5000S weiter beobachtet werden.
- Keratometer-, Tonometrie- und Pachymetrie-Karteikartenzeilen werden erkannt, aber mangels eindeutig belegtem CV-5000-Importmapping nicht in die Phoropter-Import-XML geschrieben.
- Offizielle ZIP-Ablage erst nach `docs/TEMPLATEPAKET_RELEASE_REGEL.md`.
