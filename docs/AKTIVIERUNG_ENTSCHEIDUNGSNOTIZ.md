# Entscheidungsnotiz: Schlanke V1-Aktivierung

Stand: 2026-05-12

Projekt: XdtDeviceBridge / XDT Verwaltung

Diese Notiz haelt nur noch die fachliche Linie fuer eine schlanke spaetere V1-Aktivierung von Schnittstellenprofilen fest. Ziel ist bewusst kein Wizard, kein Verwaltungsprozess und keine versteckte Automatik.

## Ziel V1

Die spaetere V1 soll aus wenigen klaren Schritten bestehen:

1. UserDefined-Schnittstellenprofil auswaehlen.
2. Pruefung ansehen.
3. Aktivierung vorbereiten.
4. Final einmal bewusst `Profil aktivieren`.

Mehr soll V1 nicht leisten.

## Was V1 kann

Produktive V1-Aktivierung darf spaeter nur moeglich sein, wenn alle Bedingungen erfuellt sind:

- Profil wurde frisch per ID geladen.
- Profil ist `UserDefined`.
- Profil ist nicht `BuiltIn`.
- Evaluation ist `Ready`.
- Es gibt keine Blocker.
- Es gibt keine Warnungen.
- Guard erlaubt die Aktivierung.
- Sichere UserDefined-Speicherung ist vorhanden.
- `IsAttachmentProcessingEnabled` wird nicht automatisch veraendert.
- Aktivierung startet keine Verarbeitung.
- Aktivierung erzeugt, verschiebt, kopiert oder loescht keine Dateien.

`IsActive` ist im aktuellen Modell der naheliegende Kandidat fuer das Aktivierungsflag. Eine produktive Implementierung muss vor dem Speichern technisch bestaetigen, dass dieses Flag der richtige und einzige V1-Schreibpunkt ist.

## Was V1 ausdruecklich nicht kann

V1 enthaelt nicht:

- keine produktive Warnungsbestaetigung
- keine Aktivierung von `ReadyWithWarnings`
- kein Auditmodell
- kein Rollenmodell
- kein Fingerprint-Modell
- keine Deaktivierung
- keine Paketstatus-Sonderlogik
- keine komplexe ActivationPlan-Logik
- keinen mehrstufigen Wizard
- keine automatische Aktivierung durch Import
- keine Aktivierung beim App-Start

`ReadyWithWarnings` wird nur angezeigt. Es wird in V1 nicht produktiv aktiviert und es gibt keine Warnungsbestaetigung, die Warnungen speichern oder uebergehen koennte.

## Aktivierungsregel

Die kompakte V1-Regel lautet:

```text
UserDefined + nicht BuiltIn + frisch geladen + Ready + 0 Blocker + 0 Warnungen + Guard Allowed
=> spaeter grundsaetzlich aktivierbar
```

Alles andere bleibt blockiert oder nicht verfuegbar:

- `ReadyWithWarnings`: nicht aktivierbar in V1
- `Blocked`: nie aktivieren
- `Unknown` / `NotEvaluated` / nicht eindeutig: nie aktivieren
- `BuiltIn`: nie veraendern
- Nicht-`UserDefined`: nie aktivieren

Der bestehende Dialog `Aktivierung vorbereiten` bleibt Vorschau. Er zeigt Bewertung, technische Freigabe, Blocker, Warnungen, Hinweise und den Sicherheitshinweis. Er bestaetigt nichts und aktiviert nichts.

## Sicherheitsgrenzen

Eine spaetere produktive V1-Aktivierung darf nur das UserDefined-Schnittstellenprofil betreffen. Sie darf nicht:

- AIS-/Geraete-/Exportprofile veraendern
- Exportregeln oder Templates veraendern
- BuiltIn-Profile ueberschreiben
- `IsAttachmentProcessingEnabled` automatisch einschalten
- Verarbeitung sofort starten
- Verarbeitung beim App-Start einfuehren
- einen Windows-Dienst, Autostart oder `FileSystemWatcher` einfuehren
- Ordner anlegen, leeren oder bereinigen
- Dateien erzeugen, kopieren, verschieben oder loeschen
- MEDISTAR-/AIS-Default-Exporttemplate-Logik einfuehren
- automatische `6330`-Zeilenlogik einfuehren
- Lizenzdurchsetzung ohne eigene Spezifikation einfuehren

Direkt vor einer spaeteren echten Speicherung muss der Executor frisch laden, frisch evaluieren und den Guard erneut ausfuehren. Alte Preview-Daten duerfen nicht direkt ausgefuehrt werden.

## Aktueller technischer Stand

Vorhanden und weiterhin sinnvoll:

- `InterfaceProfileActivationEvaluationService`
- `InterfaceProfileActivationGuardService`
- `InterfaceProfileActivationPreviewDisplayService`
- `InterfaceProfileActivationPreparationPreviewService`
- `InterfaceProfileActivationPreparationPreviewWindow`
- `IInterfaceProfileActivationExecutor`
- `InterfaceProfileActivationExecutorStub`
- `IInterfaceProfileActivationProfileStore`
- `InterfaceProfileActivationProfileCatalogStore`

Bewusst entfernt beziehungsweise nicht mehr V1-Bestandteil:

- produktive oder vorbereitete Warnungsbestaetigungsarchitektur
- `InterfaceProfileActivationPlan`-/PlannedSteps-Architektur
- Deaktivierungsmodus im Executor
- Fingerprint-/Audit-/Rollen-Kontext im Executor-Request
- `ReadyWithWarnings` als spaeterer Produktivpfad

Der `InterfaceProfileActivationExecutorStub` bleibt nicht-produktiv. Im `ValidateOnly`-Modus kann er optional frisch laden, final Evaluation + Guard simulieren und einen Save-DryRun ausweisen. Er setzt kein `IsActive`, speichert nichts und startet keine Verarbeitung.

## Offene spaetere Themen

Muss vor produktiver V1 entschieden sein:

- bestaetigen, dass `IsActive` das einzige V1-Aktivierungsflag ist
- konkrete sichere Store-Methode fuer UserDefined-Schnittstellenprofile
- produktiver Executor mit frischem Laden, Evaluation und Guard direkt vor Speicherung
- UI-Position und Wortlaut des finalen Buttons `Profil aktivieren`

Kann spaeter separat entschieden werden:

- Umgang mit `ReadyWithWarnings`
- echte Warnungsbestaetigung
- Audit/Log
- Rollenmodell
- Deaktivierung
- Umgang mit laufenden oder wartenden Paketen
- erweiterte Parallelitaets-/Fingerprint-Logik
- Lizenzdurchsetzung

Diese offenen Themen sind nicht Voraussetzung fuer eine schlanke V1, solange V1 strikt nur `Ready` ohne Warnungen aktiviert.
