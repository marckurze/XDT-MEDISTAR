# Entscheidungsnotiz: Produktive Aktivierung von Schnittstellenprofilen

Stand: 2026-05-11

Projekt: XdtDeviceBridge / XDT Verwaltung

Diese Notiz sammelt die fachlichen Entscheidungen, die vor einer echten produktiven Aktivierung von Schnittstellenprofilen getroffen werden muessen. Sie ist bewusst ein Konzeptdokument.

Aktueller Grundsatz:

- Der Aktivierungsassistent ist nur Vorschau.
- Es gibt keinen produktiven Aktivieren-Button.
- Es gibt keine produktive Warnungsbestaetigung.
- Es gibt keine produktive `ActivationExecutor`-Implementierung.
- Es wird nichts gespeichert, kein Profil veraendert und keine Verarbeitung gestartet.
- Das vorhandene `ActivationExecutor`-Skelett beschreibt nur einen spaeteren Vertrag.

## Vorlaeufige fachliche Entscheidungslinie V1 (Vorschlag)

Dieser Abschnitt ist eine fachliche Vorentscheidung fuer eine spaetere produktive Aktivierung. Er ist noch keine Implementierung und keine Freigabe fuer produktive Aktivierung.

### Aktivierbare Zustaende

Vorschlag fuer V1:

- `Blocked` darf niemals produktiv aktiviert werden.
- `Unknown`, `NotAvailable` oder nicht eindeutig bewertete Zustaende duerfen niemals produktiv aktiviert werden.
- `Ready` darf grundsaetzlich produktiv aktivierbar sein.
- `ReadyWithWarnings` darf nur nach bewusster Warnungsbestaetigung produktiv aktivierbar sein.
- Wenn einzelne Warnungen spaeter fachlich als kritisch eingestuft werden, muessen diese Warnungen in der Bewertung zu Blockern hochgestuft werden.
- Eine Aktivierung darf niemals aus alten Preview-Daten heraus erfolgen.
- Direkt vor produktiver Aktivierung muss eine frische Evaluation, Guard-Pruefung, WarningConfirmation-Pruefung und ActivationPlan-Erstellung erfolgen.

Fachliche Einordnung:

- `ReadyWithWarnings` ist kein Fehlerzustand.
- `ReadyWithWarnings` bedeutet: Aktivierung ist fachlich moeglich, aber nur nach bewusster Benutzerentscheidung.
- Ohne Bestaetigung bleibt der spaetere Aktivierungsprozess blockiert.
- Mit Bestaetigung darf ein spaeterer Executor theoretisch fortfahren, sofern alle anderen Bedingungen erfuellt sind.

Grenzen:

- Warnungen duerfen nicht stillschweigend akzeptiert werden.
- Warnungen duerfen nicht automatisch durch Import akzeptiert werden.
- Warnungen duerfen nicht allein durch Oeffnen des Dialogs akzeptiert werden.
- Die aktuelle UI zeigt Warnungen nur an und bestaetigt sie nicht.

Status: Vorschlag, nicht implementiert.

### Warnungsbestaetigung

Eine spaetere produktive Warnungsbestaetigung sollte mindestens enthalten:

- Profil-ID
- Profilname
- Zeitpunkt
- Benutzerkennung oder ausfuehrender Benutzerkontext, falls verfuegbar
- bestaetigte Warning-Codes oder Warning-Titel
- `EvaluationStatus` zum Zeitpunkt der Bestaetigung
- `GuardDecision` zum Zeitpunkt der Bestaetigung
- `ActivationPlanStatus` zum Zeitpunkt der Bestaetigung
- Hinweistext, dass die Aktivierung trotz Warnungen bewusst fortgesetzt wurde

Konservative Gueltigkeitsregel:

- Wenn sich das Schnittstellenprofil relevant aendert, muss die Warnungsbestaetigung ungueltig werden.
- Relevante Aenderungen sind insbesondere:
  - AIS-Profil
  - Geraeteprofil
  - Exportprofil
  - Import-/Exportordner
  - XDT-Anhang-Modus `Optional`/`Required`
  - XDT-Anhang-Import-/Exportordner
  - Dateiname-Template
  - Transfermodus
  - `6302`, `6303`, `6304`, `6305`
  - Lizenzstatus oder lizenzpflichtige Bewertung

Status: Vorschlag, nicht implementiert.

### Bedeutung von aktiv in V1

Ein Schnittstellenprofil ist in V1 aktiv, wenn es vom bestehenden Betriebsmodell beruecksichtigt werden darf:

- Das Profil darf vom bestehenden periodischen Scan beruecksichtigt werden.
- Aktivierung startet keine Verarbeitung sofort.
- Aktivierung startet keine Verarbeitung beim App-Start.
- Aktivierung installiert keinen Dienst.
- Aktivierung richtet keinen Autostart ein.
- Aktivierung richtet keinen `FileSystemWatcher` ein.
- Aktivierung veraendert keine AIS-/Geraete-/Exportprofile automatisch.
- Aktivierung veraendert keine BuiltIn-Profile.
- Aktivierung betrifft nur das UserDefined-Schnittstellenprofil.

Bewusst offen:

- welches konkrete Flag verwendet wird, zum Beispiel `IsActive` oder ein anderes bestehendes Aktivierungskennzeichen
- wo genau gespeichert wird
- wie parallele Aenderungen erkannt werden

Status: Vorschlag, konkrete Persistenz noch offen.

### `IsAttachmentProcessingEnabled`

Konservativer Vorschlag fuer V1:

- Produktive Aktivierung eines Schnittstellenprofils veraendert `IsAttachmentProcessingEnabled` nicht automatisch.
- XDT-Anhang-Automatik bleibt eine eigene bewusste Einstellung.
- Wenn `IsAttachmentProcessingEnabled` deaktiviert ist, darf das Schnittstellenprofil trotzdem aktiv sein.
- Wenn XDT-Anhang `Required` ist, muss die Pruefung weiterhin sicherstellen, dass die Anhang-Konfiguration plausibel ist.
- Eine spaetere UI darf klar anzeigen, dass Aktivierung des Profils und Aktivierung der Anhang-Automatik getrennte Dinge sind.

Status: Vorschlag, nicht implementiert.

### Finale Direktpruefung vor Speicherung

Ein spaeterer produktiver Executor darf niemals nur auf den Daten des Preview-Dialogs arbeiten. Direkt vor einer echten Aktivierung muss neu geprueft werden:

- Profil existiert noch.
- Profil ist UserDefined.
- Profil ist nicht BuiltIn.
- abhaengige AIS-/Geraete-/Exportprofile existieren noch.
- Ordnerpruefung ist weiterhin gueltig.
- XDT-Anhang-Konfiguration ist weiterhin gueltig.
- Evaluation ist `Ready` oder `ReadyWithWarnings` mit Bestaetigung.
- Guard erlaubt die Aktivierung.
- WarningConfirmation ist vorhanden, falls erforderlich.
- ActivationPlan ist `Ready` oder `ReadyWithAcceptedWarnings`.
- Es sind keine Blocker vorhanden.

Wenn eine dieser Pruefungen fehlschlaegt, darf nicht gespeichert werden.

Status: zwingende fachliche Leitplanke fuer eine spaetere Implementierung, noch nicht implementiert.

### Kurzfassung der vorlaeufigen V1-Linie

- Aktivierung nur fuer UserDefined-Schnittstellenprofile.
- BuiltIn bleibt geschuetzt.
- `Blocked` und `Unknown` niemals aktivieren.
- `Ready` ist aktivierbar.
- `ReadyWithWarnings` ist nur nach bewusster Bestaetigung aktivierbar.
- `IsAttachmentProcessingEnabled` bleibt eine separate Einstellung.
- Aktivierung startet keine Sofortverarbeitung.
- Finale Re-Evaluation direkt vor Speicherung ist Pflicht.

Weiterhin offen:

- konkretes Aktivierungsflag
- konkrete Persistenzstelle
- konkrete UI fuer produktive Warnungsbestaetigung
- Audit-/Logformat
- Benutzerrollenmodell
- endgueltiger produktiver Executor
- Lizenzdurchsetzung
- Verhalten bei parallelen Aenderungen
- ob bestimmte Warnungen spaeter zu Blockern werden

## 1. Wann darf ein Profil produktiv aktiviert werden?

Aktueller Stand:

- `Ready` kann technisch als vorbereitbar gelten.
- `ReadyWithWarnings` wird aktuell konservativ blockiert, solange Warnungen nicht bewusst bestaetigt wurden.
- `Blocked` darf nicht aktiviert werden.
- `Unknown`, `NotAvailable` oder nicht eindeutig bewertete Zustaende duerfen nicht aktiviert werden.

Offene Entscheidung:

- Darf `ReadyWithWarnings` nach bewusster Bestaetigung produktiv aktiviert werden?
- Sollen bestimmte Warnungen weiterhin blockierend bleiben?
- Wer entscheidet, ob eine Warnung fachlich harmlos oder aktivierungsrelevant ist?

Empfohlene konservative Richtung:

- `Ready` darf spaeter aktivierbar sein.
- `ReadyWithWarnings` darf erst nach expliziter Warnungsbestaetigung aktivierbar sein.
- Einzelne Warnungen koennen spaeter zu Blockern hochgestuft werden, falls die Fachentscheidung das verlangt.

Risiko bei falscher Entscheidung:

- Profile koennten trotz kritischer Konfiguration produktiv laufen.
- Anwender koennten Warnungen als rein kosmetisch missverstehen.

Technische Auswirkung:

- Guard, WarningConfirmation und ActivationPlan muessen unmittelbar vor echter Aktivierung neu berechnet werden.
- Der spaetere Executor darf nicht nur alte Preview-Daten verwenden.

Status: empfohlen, aber fachlich noch freizugeben.

## 2. Warnungsbestaetigung

Aktueller Stand:

- Warnungen werden als bestaetigungspflichtige Items modelliert.
- Es gibt keine Checkbox, keinen Bestaetigungsbutton und keine Speicherung.

Offene Entscheidung:

- Gilt eine Warnungsbestaetigung nur im aktuellen Dialog?
- Soll sie dauerhaft gespeichert werden?
- Soll sie auditierbar sein?
- Wer bestaetigt?
- Wann verfaellt eine Bestaetigung?
- Muss erneut bestaetigt werden, wenn sich Ordner, Profile, XDT-Anhang-Felder oder Lizenzstatus aendern?

Empfohlene konservative Richtung:

- Warnungsbestaetigungen nicht stillschweigend speichern.
- Falls sie gespeichert werden, dann mit Zeitstempel, Benutzerkennung, Profil-ID, Evaluation-/Plan-Status und bestaetigten Warning-Codes.
- Bei relevanter Profil- oder Abhaengigkeitsaenderung muss die Bestaetigung ungueltig werden.

Risiko bei falscher Entscheidung:

- Veraltete Warnungsbestaetigungen koennten spaeter eine riskante Aktivierung erlauben.
- Nicht nachvollziehbare Bestaetigungen erschweren Support und Audit.

Technische Auswirkung:

- Es braucht ein klares Modell fuer bestaetigte Warning-Codes und deren Gueltigkeit.
- Eine spaetere UI muss sichtbar zwischen Vorschau und echter Bestaetigung unterscheiden.

Status: offen.

## 3. Aktivierungsflag

Aktueller Stand:

- Schnittstellenprofile haben aktuell `IsActive`.
- Importierte Schnittstellenprofile bleiben inaktiv.
- Das vorhandene Executor-Skelett setzt kein Flag.

Offene Entscheidung:

- Wird `IsActive` als produktives Aktivierungsflag verwendet?
- Gibt es ein anderes Aktivierungsflag?
- Wird Aktivierung pro Schnittstellenprofil gespeichert?
- Welche Auswirkung hat Aktivierung auf Monitoring und periodischen Scan?
- Darf ein Profil aktiv sein, wenn `IsAttachmentProcessingEnabled` deaktiviert ist?

Empfohlene konservative Richtung:

- Aktivierung nur fuer UserDefined-Schnittstellenprofile.
- BuiltIn bleibt unveraendert.
- Aktivierung startet keine Verarbeitung sofort.
- Periodischer Scan bleibt Betriebsmodell.
- Keine Verarbeitung beim App-Start.

Risiko bei falscher Entscheidung:

- Importierte Profile koennten unkontrolliert in die Verarbeitung geraten.
- BuiltIn-Profile koennten unbeabsichtigt veraendert werden.

Technische Auswirkung:

- Der spaetere Executor muss vor dem Speichern eindeutig wissen, welches Flag veraendert wird.
- Monitoring darf erst nach bewusster Aktivierung und laufendem Scanmodell reagieren.

Status: offen, konservative Richtung empfohlen.

## 4. `IsAttachmentProcessingEnabled`

Aktueller Stand:

- Importierte Schnittstellenprofile behalten XDT-Anhang-Einstellungen.
- `IsAttachmentProcessingEnabled` wird bei importierten Profilen deaktiviert.
- Die Aktivierungspruefung bewertet die Anhang-Konfiguration nur lesend.

Offene Entscheidung:

- Soll eine Profilaktivierung `IsAttachmentProcessingEnabled` veraendern?
- Oder bleibt XDT-Anhang-Automatik eine separate Einstellung?

Empfohlene konservative Richtung:

- Aktivierung soll `IsAttachmentProcessingEnabled` nicht automatisch aendern.
- XDT-Anhang-Automatik bleibt eine explizite eigene Einstellung.
- Required/Optional, Ordner und Linkfelder muessen trotzdem bewertet werden.

Risiko bei falscher Entscheidung:

- Ein importiertes Profil koennte unbemerkt Anhangdateien verarbeiten.
- Pflicht-Anhang-Verhalten koennte produktiv starten, bevor Ordner und Ablage bewusst geprueft sind.

Technische Auswirkung:

- Der Executor darf Anhang-Automatik nicht nebenbei aktivieren.
- Eine spaetere UI muss Aktivierung und Anhang-Automatik klar trennen.

Status: empfohlen.

## 5. Speicherung und Persistenz

Aktueller Stand:

- UserDefined-Profile werden separat gespeichert.
- BuiltIn-Profile werden nicht ueberschrieben.
- Der Aktivierungsassistent speichert nichts.

Offene Entscheidung:

- Welche Profilfelder werden bei Aktivierung gespeichert?
- Wo werden UserDefined-Profile gespeichert?
- Wie wird verhindert, dass BuiltIn ueberschrieben wird?
- Wie wird ein paralleler Aenderungsstand erkannt?
- Muss direkt vor Speicherung erneut Evaluation, Guard und Plan laufen?

Empfohlene konservative Richtung:

- Direkt vor Speicherung finale Re-Evaluation erzwingen.
- Nur UserDefined-Profile speichern.
- BuiltIn niemals ueberschreiben.
- Keine Exportprofile oder Geraeteprofile automatisch aendern.
- Kein `ReplaceExisting` im Aktivierungsprozess einfuehren.

Risiko bei falscher Entscheidung:

- Ein alter Preview-Stand koennte nach Konfigurationsaenderung trotzdem gespeichert werden.
- Abhaengigkeiten oder Ordner koennten zwischen Vorschau und Aktivierung ungueltig geworden sein.

Technische Auswirkung:

- Der spaetere Executor muss Speichern als letzten Schritt nach frischer Pruefung behandeln.
- Es braucht eine klare Fehlerantwort, wenn sich der Profilstand geaendert hat.

Status: offen, konservative Richtung empfohlen.

## 6. Finale Direktpruefung unmittelbar vor Aktivierung

Aktueller Stand:

- Preview, Guard, WarningConfirmation und ActivationPlan koennen aktuelle Zustaende anzeigen.
- Sie sind noch keine Ausfuehrungsgrundlage.

Pflicht vor echter Aktivierung:

- Evaluation neu erstellen.
- Guard neu ausfuehren.
- WarningConfirmation pruefen.
- ActivationPlan neu erstellen.
- Profil noch vorhanden?
- Profil weiterhin UserDefined?
- Profil weiterhin nicht BuiltIn?
- Abhaengige Profile weiterhin vorhanden?
- Ordnerstatus weiterhin plausibel?
- XDT-Anhang-Konfiguration weiterhin plausibel?
- Lizenzhinweise weiterhin beruecksichtigt?

Empfohlene konservative Richtung:

- Keine Aktivierung aus alten Preview-Daten heraus.
- Preview ist nur Anzeige, nicht Ausfuehrungsgrundlage.
- Der Executor muss direkt vor Ausfuehrung frisch pruefen.

Risiko bei falscher Entscheidung:

- Aktivierung koennte auf einem veralteten, inzwischen ungueltigen Zustand basieren.

Technische Auswirkung:

- `InterfaceProfileActivationExecutorRequest` darf zwar Plan und Evaluation referenzieren, aber der produktive Executor muss deren Aktualitaet absichern oder neu erzeugen.

Status: empfohlen.

## 7. Audit / Log

Aktueller Stand:

- Es gibt noch keinen produktiven Aktivierungs-Audit.
- Diese Notiz enthaelt keine Patientendaten, Live-Pfade oder Kundendaten.

Offene Entscheidung:

- Soll Aktivierung in ein Log geschrieben werden?
- Welche Daten duerfen und sollen geloggt werden?
- Benutzer?
- Zeitstempel?
- Profil-ID?
- Alter und neuer Aktivstatus?
- Bestaetigte Warnungen?
- Blockerfreiheit?
- App-Version?

Empfohlene konservative Richtung:

- Aktivierung sollte auditierbar sein.
- Keine Patientendaten im Aktivierungslog.
- Keine produktiven Kundendaten oder Live-System-Pfade in Dokumentation uebernehmen.
- Pfade im lokalen Log nur bewusst und sparsam.

Risiko bei falscher Entscheidung:

- Aktivierungen waeren spaeter nicht nachvollziehbar.
- Logs koennten unnoetig sensible Informationen enthalten.

Technische Auswirkung:

- Vor produktiver Implementierung braucht es ein minimales Auditmodell.
- Audit darf nicht mit medizinischen Daten vermischt werden.

Status: offen.

## 8. Benutzerrolle / Berechtigung

Aktueller Stand:

- Im aktuellen Projekt ist keine Rollenlogik fuer Aktivierung produktiv umgesetzt.
- Der Assistent zeigt nur Vorschau.

Offene Entscheidung:

- Wer darf aktivieren?
- Braucht es eine Admin-/Technikerrolle?
- Reicht der lokale Windows-Benutzer?
- Gibt es im Projekt spaeter Rollen?
- Muss Aktivierung bewusst bestaetigt werden?

Empfohlene konservative Richtung:

- Aktivierung nur durch bewusst handelnden Benutzer.
- Keine automatische Aktivierung durch Import.
- Keine Aktivierung beim App-Start.
- Keine Aktivierung ohne sichtbare Pruefung.

Risiko bei falscher Entscheidung:

- Importierte Profile koennten von falscher Stelle produktiv freigegeben werden.

Technische Auswirkung:

- Eine spaetere UI braucht eine klare finale Bestaetigung und optional Benutzer-/Rolleninformation.

Status: offen.

## 9. UI-Fuehrung

Aktueller Stand:

- `Aktivierung vorbereiten` ist ein reines scrollbares Preview-Fenster mit OK-/Schliessen-Aktion.
- Der Dialog zeigt Bewertung, Guard, Warnungsbestaetigungsvorschau und ActivationPlan.
- Es gibt keinen Aktivieren-Button.

Offene Entscheidung:

- Bleibt `Aktivierung vorbereiten` ein Dialog?
- Wird spaeter ein echter mehrstufiger Wizard benoetigt?
- Wo wird eine Warnungsbestaetigung angezeigt?
- Wo sitzt ein finaler Aktivieren-Button?
- Wie wird verhindert, dass Benutzer Vorschau mit echter Aktivierung verwechseln?

Empfohlene konservative Richtung:

- Spaeter eigener finaler Schritt mit klarer Beschriftung.
- Deutliche Trennung zwischen Vorschau und produktiver Aktivierung.
- Kein Aktivieren-Button im aktuellen Preview-Dialog, bis alle fachlichen Entscheidungen getroffen sind.

Risiko bei falscher Entscheidung:

- Anwender koennten eine Vorschau fuer eine echte Freigabe halten.
- Produktive Aktivierung koennte ohne genuegend Kontext erfolgen.

Technische Auswirkung:

- UI-Texte und Button-Benennung muessen sehr eindeutig sein.

Status: offen.

## 10. Lizenz

Aktueller Stand:

- Lizenzstatus und Karenzzeitmodell sind vorbereitet.
- Es gibt keine harte produktive Lizenzsperre.
- Keine Online-Lizenzierung ist implementiert.

Offene Entscheidung:

- Darf ein lizenzpflichtiges Profil ohne gueltige Lizenz aktiviert werden?
- Bleibt Lizenz nur Warnung?
- Wird spaeter eine harte Sperre eingefuehrt?
- Wie wirkt das Karenzzeitmodell?
- Wann wird Lizenzstatus geprueft?

Empfohlene konservative Richtung:

- Keine harte Lizenzsperre ohne gesonderte Spezifikation.
- Lizenzstatus im Aktivierungsprozess sichtbar machen.
- Harte Sperre erst nach separatem Lizenzkonzept.

Risiko bei falscher Entscheidung:

- Aktivierung koennte gegen das spaetere Lizenzmodell laufen.
- Eine zu harte Sperre koennte Praxisbetrieb unnoetig blockieren.

Technische Auswirkung:

- Lizenzbewertung bleibt vorerst Hinweis/Warnung, bis das Lizenzkonzept abgeschlossen ist.

Status: offen.

## 11. Produktive Verarbeitung nach Aktivierung

Aktueller Stand:

- Die App startet keine Verarbeitung beim App-Start.
- Ueberwachung wird manuell gestartet.
- Periodischer Scan bleibt aktuelles Betriebsmodell.

Offene Entscheidung:

- Startet Aktivierung sofort Verarbeitung?
- Wird nur beim naechsten periodischen Scan gearbeitet?
- Wie wird verhindert, dass beim Aktivieren alte Dateien verarbeitet werden?
- Muss es einen Initialzustand oder Startzeitpunkt geben?

Empfohlene konservative Richtung:

- Aktivierung startet keine Verarbeitung sofort.
- Kein `FileSystemWatcher`.
- Kein Autostart.
- Periodischer Scan bleibt Betriebsmodell.
- Keine Verarbeitung beim App-Start.

Risiko bei falscher Entscheidung:

- Alte Dateien koennten direkt nach Aktivierung unerwartet verarbeitet werden.
- Aktivierung koennte Betriebsverhalten ausloesen, das der Benutzer nicht erwartet.

Technische Auswirkung:

- Ein spaeterer Executor darf keine Scan- oder Verarbeitungslogik starten.
- Ein moeglicher Initialzustand muss separat spezifiziert werden.

Status: empfohlen, Details offen.

## 12. Schutz vor unbeabsichtigten Dateioperationen

Weiterhin gueltig:

- Keine Ordnerbereinigung durch Aktivierung.
- Keine pauschale Ordnerleerung.
- Exportordner nicht bereinigen.
- Keine unbekannten Dateien anfassen.
- Keine Anhangdateien beim Aktivieren kopieren oder verschieben.
- Keine XDT-Datei beim Aktivieren erzeugen.

Risiko bei falscher Entscheidung:

- Aktivierung koennte ungewollt produktive Dateien veraendern oder entfernen.

Technische Auswirkung:

- Der spaetere Executor darf nur Profilzustand behandeln, keine Import-/Exportdateien.

Status: festzuhalten.

## 13. Offener Produktivumfang

Noch nicht implementiert:

- produktiver `ActivationExecutor`
- finaler Aktivieren-Button
- UI fuer echte Warnungsbestaetigung
- Speicherung der Warnungsbestaetigung
- Audit-/Logeintrag
- finale Re-Evaluation im Executor
- tatsaechliches Setzen eines Aktivierungsflags
- Persistenz der Profiländerung
- Rollen-/Berechtigungspruefung
- Lizenzdurchsetzung
- Start-/Initialzustand fuer produktive Verarbeitung nach Aktivierung

Status: offen.

## 14. Empfohlene naechste Schritte

Konservative Reihenfolge:

1. Vorlaeufige V1-Linie fachlich abnehmen oder anpassen.
2. Entscheidung zu Warnungsbestaetigung und Audit treffen.
3. Entscheidung zu Aktivierungsflag und Speicherung treffen.
4. Entscheidung zu `IsAttachmentProcessingEnabled` bestaetigen.
5. Erst danach produktiven Executor entwerfen.
6. Erst danach finalen UI-Schritt planen.
7. Erst danach Implementierung in kleinen, testbaren Schritten.

Status: empfohlen.
