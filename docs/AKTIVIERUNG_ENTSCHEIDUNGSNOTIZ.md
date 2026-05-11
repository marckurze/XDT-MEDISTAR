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

- `Aktivierung vorbereiten` ist ein reiner OK-Dialog.
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

1. Praktische UI-Pruefung des aktuellen Aktivierungsassistenten.
2. Fachliche Entscheidung zu `ReadyWithWarnings`.
3. Entscheidung zu Warnungsbestaetigung und Audit.
4. Entscheidung zu Aktivierungsflag und Speicherung.
5. Entscheidung zu `IsAttachmentProcessingEnabled`.
6. Erst danach produktiven Executor entwerfen.
7. Erst danach finalen UI-Schritt planen.
8. Erst danach Implementierung in kleinen, testbaren Schritten.

Status: empfohlen.
