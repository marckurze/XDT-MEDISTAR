# UI-Pruefprotokoll Aktivierungsassistent

Stand: 2026-05-12

Projekt: XdtDeviceBridge / XDT Verwaltung

## 1. Pruefziel

Dieses Protokoll prueft den aktuellen Aktivierungsassistenten im Tab `Schnittstellenprofile`. Ziel ist die Absicherung, dass die vorhandene UI weiterhin reine Vorschau ist und keine produktive Aktivierung, Warnungsbestaetigung, Speicherung, Datei-/Ordneroperation oder Verarbeitung ausloest.

## 2. Pruefgegenstand

Geprueft wurden:

- Bereich `Pruefung vor Aktivierung`
- Button `Pruefung aktualisieren`
- Button `Aktivierung vorbereiten`
- scrollbares Preview-Fenster `Aktivierung vorbereiten`
- Dialogdaten aus `InterfaceProfileActivationPreparationPreviewService`
- technische Guard-Anzeige
- `ActivationExecutor`-Stub auf produktive Verwendung

## 3. Aktueller Stand

- Die UI zeigt Evaluation, technische Freigabe, Blocker, Warnungen, Hinweise und Sicherheitshinweis.
- `Ready` ohne Warnungen kann in der Vorschau als `Aktivierbar nach V1: Ja` erscheinen.
- `ReadyWithWarnings` wird angezeigt, aber in V1 nicht produktiv aktiviert.
- Es gibt keine produktive Warnungsbestaetigung.
- Es gibt keinen ActivationPlan-/PlannedSteps-Abschnitt mehr.
- Es gibt keinen Aktivieren-Button.
- Es gibt keine Checkbox.
- Es gibt keinen Bestaetigungsbutton.

## 4. Praktische Sichtpruefung

Die praktische Sichtpruefung des Dialogs `Aktivierung vorbereiten` wurde nach der Umstellung auf ein scrollbares Preview-Fenster und nach der Textstraffung durch den Benutzer auf einem Windows-System durchgefuehrt. Ergebnis:

- Der Bereich `Pruefung vor Aktivierung` im Tab `Schnittstellenprofile` sieht sauber aus.
- Der Dialog `Aktivierung vorbereiten` ist lesbar.
- Scroll-/Resize-Loesung und Abschnittsgliederung sind ausreichend.
- Redundanzen wurden ausreichend reduziert.
- Der Sicherheitshinweis ist deutlich sichtbar.
- Der Dialog wird insgesamt als sehr gut bewertet.

Nach der anschliessenden V1-Reduktion bleibt diese Bewertung fachlich erhalten; die Anzeige ist nochmals kuerzer geworden.

## 5. Statisches Pruefergebnis

- `Pruefung aktualisieren` ruft nur die read-only Bewertung und Anzeige auf.
- `Aktivierung vorbereiten` erzeugt Evaluation und Guard fuer `Context: "PreviewOnly"`.
- Der Dialog wird als eigenes scrollbares Preview-Fenster `InterfaceProfileActivationPreparationPreviewWindow` angezeigt.
- Das Fenster besitzt nur OK-/Schliessen-Aktion.
- Der Dialogtext enthaelt den Sicherheitshinweis: Es wurde nichts gespeichert und nichts aktiviert.
- Der `ActivationExecutorStub` ist nicht in App/UI-Flows produktiv angebunden.

## 6. Konkrete Prueffaelle

| Nr. | Prueffall | Erwartetes Ergebnis | Status |
| --- | --- | --- | --- |
| 1 | Tab `Schnittstellenprofile` oeffnen | Bereich `Pruefung vor Aktivierung` sichtbar; keine Ueberlagerung mit Ordnerbereinigung/Archivierung | statisch bestanden / praktisch abgenommen |
| 2 | Schnittstellenprofil ohne Auswahl | Verstaendlicher Hinweis; keine Exception | statisch bestanden |
| 3 | UserDefined-Profil mit `Ready` | Status Ready, keine Blocker/Warnungen, `Aktivierbar nach V1: Ja`, keine Aktivierung | statisch plausibel |
| 4 | UserDefined-Profil mit `ReadyWithWarnings` | Warnungen sichtbar, `Aktivierbar nach V1: Nein`, keine Warnungsbestaetigung | statisch bestanden |
| 5 | Blocked-Profil | Blocker sichtbar, technisch blockiert, keine Aktivierung | statisch plausibel |
| 6 | BuiltIn-Profil | Direkte Aktivierung blockiert oder nicht zulaessig; BuiltIn unveraendert | statisch plausibel |
| 7 | Button `Pruefung aktualisieren` | Bewertung aktualisiert sich; kein Profil wird gespeichert; keine Verarbeitung startet | statisch bestanden |
| 8 | Button `Aktivierung vorbereiten` | Nur OK-/Schliessen-Dialog; kein Aktivieren-Button; keine Speicherung; keine Verarbeitung | statisch bestanden |
| 9 | Scrollbarkeit und Layout | Dialog bleibt lesbar und scrollbar | praktisch abgenommen |

## 7. Sicherheitspruefung

| Frage | Ergebnis |
| --- | --- |
| Gibt es im Dialog einen Button mit produktiver Aktivierungswirkung? | Nein. |
| Gibt es eine Checkbox zur Warnungsbestaetigung? | Nein. |
| Wird im Aktivierungsdialog `IsActive` gesetzt? | Nein. |
| Wird im Aktivierungsdialog `IsAttachmentProcessingEnabled` geaendert? | Nein. |
| Wird aus der Preview heraus gespeichert? | Nein. |
| Werden Dateien/Ordner angelegt, kopiert, verschoben oder geloescht? | Nein. |
| Wird Verarbeitung gestartet? | Nein. |
| Wird der ActivationExecutor produktiv verwendet? | Nein. |

## 8. Bewertung

Die aktuelle Dialogdarstellung ist fuer den Vorschau-Status des Aktivierungsassistenten abgenommen. Sie ist schlank genug fuer die V1-Vorbereitung und vermeidet die zuvor aufgebaute Warnungsbestaetigungs-/Plan-Komplexitaet.

## 9. Weiterhin offen

- echte produktive Aktivierung
- produktiver `ActivationExecutor`
- finale Entscheidung zu `IsActive` als Schreibpunkt
- sichere produktive UserDefined-Store-Methode
- finale Evaluation + Guard direkt vor Speicherung
- spaetere Nicht-V1-Themen wie Warnungsbestaetigung, Audit, Deaktivierung oder Rollenmodell
