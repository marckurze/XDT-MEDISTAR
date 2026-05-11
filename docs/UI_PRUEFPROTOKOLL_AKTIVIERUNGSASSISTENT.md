# UI-Pruefprotokoll Aktivierungsassistent

Stand: 2026-05-11

Projekt: XdtDeviceBridge / XDT Verwaltung

## 1. Pruefziel

Dieses Protokoll prueft den aktuellen Aktivierungsassistenten im Tab `Schnittstellenprofile` aus Anwendersicht und anhand einer statischen XAML-/Codebehind-Pruefung. Ziel ist die Absicherung, dass die vorhandene UI weiterhin reine Vorschau ist und keine produktive Aktivierung, Warnungsbestaetigung, Speicherung, Datei-/Ordneroperation oder Verarbeitung ausloest.

## 2. Pruefgegenstand

Geprueft wurden:

- Bereich `Pruefung vor Aktivierung` im Tab `Schnittstellenprofile`
- Button `Pruefung aktualisieren`
- Button `Aktivierung vorbereiten`
- scrollbares Preview-Fenster `Aktivierung vorbereiten`
- Dialogdaten aus `InterfaceProfileActivationPreparationPreviewService`
- strukturierte Ordnerpruefung
- strukturierte XDT-Anhang-Konfiguration
- Tabelle `Alle Pruefpunkte`
- technische Guard-Anzeige
- Warnungsbestaetigungsvorschau
- ActivationPlan-Anzeige
- ActivationExecutor-Skelett auf produktive Verwendung

## 3. Voraussetzungen

Aktueller bekannter Stand:

- Aktivierungsbewertung, Guard, WarningConfirmation, ActivationPlan und PreparationPreview sind vorhanden.
- `IInterfaceProfileActivationExecutor` und zugehoerige Request-/Result-/Precondition-/Statusmodelle sind nur Skelett.
- Es gibt keine produktive Executor-Implementierung.
- Es gibt keinen Aktivieren-Button.
- Es gibt keine produktive Warnungsbestaetigung.

## 4. Pruefumgebung

Die WPF-Oberflaeche wurde in der Codex-Umgebung nicht praktisch per GUI gestartet oder visuell bedient. Es steht hier keine verlaessliche WPF-UI-Automation mit Sichtpruefung/Screenshot zur Verfuegung.

Die praktische Sichtpruefung des Dialogs `Aktivierung vorbereiten` wurde nach der Umstellung auf das scrollbare Preview-Fenster und nach der Textstraffung durch den Benutzer auf einem Windows-System durchgefuehrt. Rueckmeldung:

- Der Bereich `Pruefung vor Aktivierung` im Tab `Schnittstellenprofile` sieht sauber aus.
- Der Dialog `Aktivierung vorbereiten` ist deutlich besser lesbar.
- Scroll-/Resize-Loesung und Abschnittsgliederung sind ausreichend.
- Die Redundanzen wurden ausreichend reduziert.
- Die Warnungsbestaetigung wird zentral dargestellt.
- Technische Schutzpruefung und Aktivierungsplan sind kompakter.
- Der Sicherheitshinweis ist weiterhin deutlich sichtbar.
- Der Dialog wird insgesamt als sehr gut bewertet; es kann weitergearbeitet werden.

Stattdessen wurde eine statische Pruefung folgender Dateien durchgefuehrt:

- `XdtDeviceBridge.App/MainWindow.xaml`
- `XdtDeviceBridge.App/MainWindow.xaml.cs`
- `XdtDeviceBridge.Infrastructure/InterfaceProfileActivationPreviewDisplayService.cs`
- `XdtDeviceBridge.Infrastructure/InterfaceProfileActivationPreparationPreviewService.cs`
- `XdtDeviceBridge.Infrastructure/IInterfaceProfileActivationExecutor.cs`
- `XdtDeviceBridge.Infrastructure/InterfaceProfileActivationExecutor*.cs`

Bewertung der praktischen UI-Ausfuehrung: In der Codex-Umgebung nicht praktisch ausgefuehrt; durch Benutzer auf Windows praktisch sichtgeprueft und fuer den aktuellen Vorschau-Status abgenommen.

## 5. Statisches Pruefergebnis

Die statische Pruefung ergab:

- Der Bereich `Pruefung vor Aktivierung` ist in `MainWindow.xaml` als eigene `GroupBox` vorhanden.
- Die Reihenfolge unterhalb der Schnittstellenprofil-Konfiguration ist: `Ordnerbereinigung` in `Grid.Row=6`, `Archivierung` in `Grid.Row=7`, `Pruefung vor Aktivierung` in `Grid.Row=8`.
- Die uebergeordnete Grid-Struktur enthaelt ausreichend `Auto`-Rows fuer diese Bereiche; eine unmittelbare Ueberlagerung durch gleiche Grid-Row wurde statisch nicht gefunden.
- `Alle Pruefpunkte` ist als `Expander` mit `IsExpanded="False"` vorhanden.
- `Pruefung aktualisieren` ruft nur `RefreshInterfaceActivationPreview()` auf.
- `Aktivierung vorbereiten` erzeugt Evaluation, Guard, WarningConfirmation und ActivationPlan fuer `Context: "PreviewOnly"`.
- Guard und ActivationPlan werden im Dialog mit `WarningsAccepted: false` erzeugt.
- Der Dialog wird als eigenes scrollbares Preview-Fenster `InterfaceProfileActivationPreparationPreviewWindow` angezeigt.
- Das Fenster besitzt nur eine OK-/Schliessen-Aktion und keine produktiven Buttons.
- Die Dialogdarstellung zeigt Guard, Warnungsbestaetigung und ActivationPlan kompakt gegliedert. Bestaetigungspflichtige Warnungen erscheinen zentral im Abschnitt Warnungsbestaetigung; Guard und Aktivierungsplan wiederholen diese Liste nicht vollstaendig.
- Der Dialogtext enthaelt den Sicherheitshinweis: Es wurden keine Warnungen bestaetigt, keine Aenderungen gespeichert und nichts aktiviert.
- Das ActivationExecutor-Skelett wird in App/UI-Code nicht produktiv verwendet.

## 6. Konkrete Prueffaelle

| Nr. | Prueffall | Erwartetes Ergebnis | Statisch nachvollziehbares Ergebnis | Status |
| --- | --- | --- | --- | --- |
| 1 | Tab `Schnittstellenprofile` oeffnen | Bereich `Pruefung vor Aktivierung` sichtbar; keine Ueberlagerung mit Ordnerbereinigung/Archivierung | `Ordnerbereinigung`, `Archivierung` und `Pruefung vor Aktivierung` liegen in getrennten Grid-Rows 6/7/8. Praktische Benutzersichtpruefung: Bereich sieht sauber aus. | statisch bestanden / praktisch abgenommen |
| 2 | Schnittstellenprofil ohne Auswahl | Verstaendlicher Hinweis oder deaktivierte Aktion; keine Exception | `CreateEmpty()` liefert Hinweis `Bitte waehlen Sie zuerst ein Schnittstellenprofil aus.`; Preview-Fenster zeigt nur OK/Schliessen. | statisch bestanden |
| 3 | Gueltiges UserDefined-Profil mit `Ready` | Status Ready/aktivierbar, keine Blocker, Dialog zeigt Plan Ready, PlannedSteps beschreibend, keine Aktivierung | PreviewService formatiert `Ready` als `Bereit`; PreparationPreview formatiert Plan `Ready` als `vorbereitet`; Preview-Fenster zeigt nur OK/Schliessen. Praktische Datenprobe offen. | statisch plausibel |
| 4 | UserDefined-Profil mit `ReadyWithWarnings` | Warnungen sichtbar, Guard verlangt Warnungsbestaetigung, WarningConfirmation listet Warnungen, Plan `RequiresWarningConfirmation`, keine Checkbox, kein Bestaetigungsbutton | Guard-Aufruf im Dialog verwendet `WarningsAccepted: false`; PreparationPreview formatiert `RequiresWarningConfirmation`, listet bestaetigungspflichtige Warnungen zentral im Abschnitt Warnungsbestaetigung und laesst Guard/Plan nur knapp verweisen. | statisch bestanden |
| 5 | Blocked-Profil | Blocker sichtbar, Guard blockiert, WarningConfirmation nicht moeglich, Plan blockiert, keine Aktivierung | Preview sortiert Blocker zuerst; Guard-/Plan-Anzeigen enthalten Blocker/fehlende Voraussetzungen; Dialog nur OK/Schliessen. Praktische Datenprobe offen. | statisch plausibel |
| 6 | BuiltIn-Profil | Direkte Aktivierung blockiert oder nicht zulaessig; BuiltIn unveraendert | Bewertung/Guard sind in der Service-Kette vorgesehen; Dialog speichert nicht und aendert kein Profil. Praktische BuiltIn-Auswahl offen. | statisch plausibel |
| 7 | Nicht-UserDefined-Profil | Konservativ blockiert oder nicht zulaessig | Guard-/WarningConfirmation-Services werden in der Vorschau verwendet; UI fuehrt keine Aktivierung aus. Praktische Datenprobe offen. | statisch plausibel |
| 8 | XDT-Anhang Pflicht mit fehlenden Ordnern | Fehlende Anhangordner als Problem/Blocker sichtbar; keine Ordneranlage, keine Dateien | Ordner- und Attachment-Displays zeigen XDT-Anhang-Import-/Exportordner; PreviewDisplayService erzeugt nur Displaydaten. | statisch plausibel |
| 9 | XDT-Anhang optional und deaktiviert | Kein Blocker allein wegen deaktivierter optionaler Anhangverarbeitung; Hinweis verstaendlich | Attachment-Anzeige zeigt `Anhangverarbeitung` aktiv/inaktiv; Bewertung bleibt Service-Aufgabe. Praktische Datenprobe offen. | statisch plausibel |
| 10 | Button `Pruefung aktualisieren` | Bewertung aktualisiert sich; kein Profil wird gespeichert; keine Verarbeitung startet | Handler ruft nur Evaluation und `ShowInterfaceActivationPreview(...)` auf. Keine Speicherung/Verarbeitung im Handler gefunden. | statisch bestanden |
| 11 | Button `Aktivierung vorbereiten` | Nur OK-/Schliessen-Dialog; kein Aktivieren-Button; kein Bestaetigungsbutton; keine Speicherung; keine Verarbeitung | Handler oeffnet `InterfaceProfileActivationPreparationPreviewWindow`, nutzt `Context: "PreviewOnly"`, `WarningsAccepted: false`; keine Save-/Start-/Executor-Nutzung im Handler. | statisch bestanden |
| 12 | Scrollbarkeit und Layout | Gesamter Bereich scrollbar; Tabellen/Expander lesbar; keine Textueberlagerung | Tab-Inhalt liegt im vorhandenen ScrollViewer-/Grid-Kontext; relevante Bereiche haben eigene Auto-Rows. Der Vorbereitungdialog besitzt einen eigenen `ScrollViewer` und ist resizebar. Praktische Benutzersichtpruefung: Dialog ist lesbar, verstaendlich und ausreichend gestrafft. | statisch plausibel / praktisch abgenommen |

## 7. Statische Sicherheitspruefung

| Frage | Ergebnis |
| --- | --- |
| Gibt es im Dialog einen Button mit produktiver Aktivierungswirkung? | Nein. Statisch gefunden wurde nur der OK-/Schliessen-Button im Preview-Fenster. |
| Gibt es eine Checkbox zur Warnungsbestaetigung? | Nein. Im Aktivierungsbereich wurde keine solche Checkbox gefunden. |
| Wird im Aktivierungsdialog `IsActive` gesetzt? | Nein. `IsActive` wird im Editor angezeigt/gespeichert, aber nicht im Dialog `Aktivierung vorbereiten` gesetzt. |
| Wird im Aktivierungsdialog `IsAttachmentProcessingEnabled` geaendert? | Nein. Das Flag ist Teil der Schnittstellenprofil-Konfiguration, wird aber im Aktivierungsdialog nicht geaendert. |
| Wird aus der Preview heraus gespeichert? | Nein. Der Preview-/Dialogpfad ruft keine Speicherfunktion auf. |
| Werden Dateien/Ordner angelegt, kopiert, verschoben oder geloescht? | Nein. In den geprueften Aktivierungs-Preview-Pfaden wurden keine Datei-/Ordneroperationen gefunden. |
| Wird Verarbeitung gestartet? | Nein. Der Preview-/Dialogpfad startet keine Ueberwachung und keine Verarbeitung. |
| Wird der ActivationExecutor produktiv verwendet? | Nein. `IInterfaceProfileActivationExecutor` und zugehoerige Modelle werden nur als Skelett gefunden, nicht in App/UI-Flows. |
| Wird das Interface-Skelett irgendwo in UI-Flows ausgefuehrt? | Nein. Keine Referenz aus `XdtDeviceBridge.App` auf das Executor-Skelett gefunden. |
| Bleibt der Dialog ein reiner OK-/Vorschau-Dialog? | Ja, statisch bestaetigt. Die Darstellung ist nun ein scrollbares Preview-Fenster ohne produktive Aktion. |

## 8. Ergebnis / Bewertung

Die statische Pruefung bestaetigt den Sicherheitsrahmen des Aktivierungsassistenten:

- Die UI ist weiterhin Vorschau.
- Es gibt keinen produktiven Aktivieren-Button.
- Es gibt keine produktive Warnungsbestaetigung.
- Es gibt keine UI-Checkbox zur Warnungsbestaetigung.
- Es gibt keine Executor-Ausfuehrung.
- Es wird in den geprueften Aktivierungs-Preview-Pfaden nichts gespeichert.
- Es werden keine Datei-/Ordneroperationen gestartet.
- Es wird keine produktive Verarbeitung gestartet.

Die praktische Sichtpruefung des Dialogs `Aktivierung vorbereiten` wurde nach der Umstellung auf das scrollbare Preview-Fenster und nach der Textstraffung auf einem Windows-System durchgefuehrt. Ergebnis:

- Der Dialog ist lesbar.
- Die Scroll-/Resize-Loesung ist ausreichend.
- Die Abschnitte sind verstaendlich.
- Redundanzen wurden ausreichend reduziert.
- Die Warnungsbestaetigung wird zentral dargestellt.
- Technische Schutzpruefung und Aktivierungsplan sind kompakter.
- Der Sicherheitshinweis bleibt deutlich sichtbar.
- Der Dialog enthaelt weiterhin nur OK/Schliessen.
- Es gibt weiterhin keinen Aktivieren-Button.
- Es gibt weiterhin keine Checkbox.
- Es gibt weiterhin keinen Bestaetigungsbutton.

Bewertung: Die aktuelle Dialogdarstellung ist fuer den Vorschau-Status des Aktivierungsassistenten praktisch abgenommen.

## 9. Offene Punkte

- Echte produktive Aktivierung.
- Echte produktive Warnungsbestaetigung mit UI.
- Produktive `ActivationExecutor`-Implementierung.
- Dauerhafte Speicherung oder Auditierung einer Warnungsbestaetigung, falls spaeter fachlich gewuenscht.
- Rollen-/Berechtigungs- und finale Re-Evaluation-Entscheidungen vor produktiver Ausfuehrung.
- Spaetere Entscheidung gemaess `docs/AKTIVIERUNG_ENTSCHEIDUNGSNOTIZ.md` vor produktiver Aktivierung.

## 10. Empfehlung fuer manuelle Windows-Pruefung

Empfohlene manuelle Reihenfolge:

1. App auf einem Windows-System starten.
2. Tab `Schnittstellenprofile` oeffnen.
3. Bereich unterhalb `Ordnerbereinigung` und `Archivierung` visuell pruefen.
4. Ohne Profilauswahl `Aktivierung vorbereiten` ausloesen, falls Button erreichbar ist.
5. UserDefined-Profil mit `Ready` pruefen.
6. UserDefined-Profil mit `ReadyWithWarnings` pruefen.
7. Blockiertes Profil pruefen.
8. BuiltIn-Profil pruefen.
9. XDT-Anhang Pflicht mit fehlenden Ordnern pruefen.
10. XDT-Anhang optional und deaktiviert pruefen.
11. `Pruefung aktualisieren` ausloesen und sicherstellen, dass keine Speicherung/Verarbeitung startet.
12. `Aktivierung vorbereiten` ausloesen und sicherstellen, dass nur das scrollbare Preview-Fenster mit OK-/Schliessen-Aktion erscheint.
13. Bei kleiner Fensterhoehe/-breite Scrollbarkeit und Dialoglesbarkeit pruefen.

Abnahmekriterium fuer die manuelle Pruefung: Der Anwender kann die Aktivierungspruefung und den Dialog verstehen, ohne den Eindruck zu bekommen, dass bereits aktiviert, bestaetigt, gespeichert oder verarbeitet wurde.

Status: Die praktische Sichtpruefung des aktuellen Dialogstands ist fuer die reine Vorschau abgenommen. Die Liste bleibt als Regressionscheck fuer spaetere UI-Aenderungen erhalten.
