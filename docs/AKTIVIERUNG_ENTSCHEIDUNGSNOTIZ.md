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

## V1-Spezifikation: Aktivierungsflag, Persistenz und finale Pruefung (Vorschlag)

Dieser Abschnitt konkretisiert die vorlaeufige V1-Linie fuer eine spaetere produktive Aktivierung. Er ist eine fachliche Spezifikation fuer einen spaeteren Executor, aber weiterhin keine Implementierung.

### Aktivierungsflag

V1-Vorschlag:

- Ein Schnittstellenprofil gilt produktiv aktiv, wenn ein explizites Aktivierungskennzeichen am UserDefined-Schnittstellenprofil gesetzt ist.
- Im bestehenden Modell ist `IsActive` der naheliegende Kandidat fuer dieses Aktivierungskennzeichen.
- Die Entscheidung, ob `IsActive` endgueltig verwendet wird oder ob ein neues Feld benoetigt wird, muss vor Implementierung des produktiven Executors technisch bestaetigt werden.
- Der Aktivierungsstatus gehoert zum Schnittstellenprofil, nicht zum AIS-Profil, Geraeteprofil oder Exportprofil.
- Aktivierung darf nur UserDefined-Schnittstellenprofile betreffen.
- BuiltIn-Profile werden niemals direkt aktiviert oder geaendert.
- Aktivierung veraendert keine abhaengigen AIS-, Geraete- oder Exportprofile.
- Aktivierung veraendert keine Exportregeln.
- Aktivierung veraendert keine Templates.
- Aktivierung veraendert nicht automatisch `IsAttachmentProcessingEnabled`.
- Aktivierung startet keine Verarbeitung sofort.

Bewusst offen:

- finaler technischer Feldname, falls `IsActive` nicht ausreicht
- Migrationsverhalten, falls ein neues Feld eingefuehrt wird
- UI-Position und Wortlaut eines spaeteren produktiven Aktivieren-Buttons

Status: fachlicher V1-Vorschlag, nicht implementiert.

### Persistenzstelle fuer UserDefined-Schnittstellenprofile

Aus Projektstruktur und Dokumentation erkennbarer Stand:

- Profile werden JSON-basiert unter `%LocalAppData%\XdtDeviceBridge\profiles` verwaltet.
- Schnittstellenprofile liegen in der Profilkatalogstruktur im Unterordner `interfaces`.
- `AppDataPathProvider` leitet den Profilordner aus `%LocalAppData%\XdtDeviceBridge\profiles` ab.
- `ProfileCatalogService` laedt Schnittstellenprofile aus dem `interfaces`-Unterordner und speichert einzelne Schnittstellenprofile per `SaveInterfaceProfileDefinition`.
- BuiltIn- und UserDefined-Profile sind im Modell ueber Metadaten getrennt (`IsBuiltIn`, `IsUserDefined`).
- Beim Konfigurieren eines BuiltIn-Schnittstellenprofils wird eine UserDefined-Kopie erzeugt; importierte Schnittstellenprofile werden als UserDefined und inaktiv uebernommen.

V1-Linie:

- Keine neue separate Aktivierungsdatenbank.
- Keine Speicherung in BuiltIn-Profilen.
- Keine Aenderung an Geraeteprofilen, AIS-Profilen oder Exportprofilen.
- Aktivierungsstatus wird als Teil des UserDefined-Schnittstellenprofils gespeichert.
- Speicherung erfolgt nur nach finaler Re-Evaluation.
- Falls die konkret verwendete Speichermethode keinen ausreichenden BuiltIn-/Overwrite-Schutz bietet, muss dieser Schutz vor produktiver Aktivierung ergaenzt werden.
- Der spaetere Executor darf nicht pauschal den gesamten Katalog zurueckschreiben, sondern soll gezielt das frisch geladene und gepruefte UserDefined-Schnittstellenprofil speichern.

Bewusst offen:

- konkrete produktive Repository-/Store-Methode
- exaktes Serialisierungsformat und eventuelle Versionierung
- Migration bestehender Profile, falls das Aktivierungskennzeichen geaendert wird

Status: fachlicher V1-Vorschlag, nicht implementiert.

### Parallelitaets- und Aenderungsschutz

Problem:

Zwischen Preview-Dialog und spaeterer echter Aktivierung koennen sich Profil, Ordner, abhaengige Profile oder Lizenzstatus aendern. Der Preview-Dialog ist deshalb keine Ausfuehrungsgrundlage.

Mindestschutz fuer V1:

- Profil direkt vor Ausfuehrung neu laden.
- Evaluation neu ausfuehren.
- Guard neu ausfuehren.
- WarningConfirmation neu bewerten.
- ActivationPlan neu erstellen.
- Wenn das Ergebnis nicht mehr `Ready` oder `ReadyWithAcceptedWarnings` ist, nicht speichern.
- Wenn sich relevante Profilfelder geaendert haben, muss eine alte Warnungsbestaetigung ungueltig werden.
- Wenn sich Warning-Codes geaendert haben, muss neu bestaetigt werden.
- Wenn Ordner oder Abhaengigkeiten nicht mehr gueltig sind, muss Aktivierung blockieren.

Optionale spaetere Schutzmechanismen:

- Konfigurations-Fingerprint oder Hash ueber relevante Profilfelder
- `LastModified`-Zeitstempel
- Version oder Revision des UserDefined-Profils
- Vergleich der bestaetigten Warning-Codes mit der aktuellen Warnungsliste
- Vergleich des Profil-Fingerprints aus Warnungsbestaetigung und frisch geladenem Profil

Status: Mindestschutz fachlich gefordert, technische Mechanik noch offen.

### Warnungsbestaetigung und Audit-Grundmodell

Eine produktive Warnungsbestaetigung darf nicht stillschweigend erfolgen und muss mindestens im Aktivierungsvorgang nachvollziehbar sein.

Variante A: Nur im aktuellen Aktivierungsvorgang verwenden

- einfacher umzusetzen
- weniger Persistenz
- keine dauerhafte Bestaetigungsdatenhaltung
- schlechter auditierbar
- bei Abbruch oder Neustart muss erneut bestaetigt werden

Variante B: Dauerhaft auditierbar speichern

- besser nachvollziehbar
- braucht Auditmodell
- braucht Benutzer-/Zeitstempel
- braucht Invalidierungslogik bei Profilaenderung
- braucht klares Speicherziel und Datenschutzgrenzen

Konservative Empfehlung:

- Variante B ist fuer eine spaetere produktive Aktivierung vorzuziehen.
- Variante B darf erst nach separater Spezifikation von Auditmodell, Speicherort, Benutzerkontext, Datenschutzgrenzen und Invalidierungslogik umgesetzt werden.
- Bis dahin darf die bestehende Vorschau keine Warnungen produktiv bestaetigen.

Mindestdaten fuer spaetere Auditierung:

- Profil-ID
- Profilname
- Zeitpunkt
- Benutzerkennung oder lokaler Benutzerkontext, falls verfuegbar
- App-Version
- bestaetigte Warning-Codes oder Warning-Titel
- `EvaluationStatus`
- `GuardDecision`
- `ActivationPlanStatus`
- Hinweistext der bewussten Bestaetigung
- optional Profil-Fingerprint

Sicherheitsgrenzen:

- Keine Patientendaten im Audit.
- Keine Kundendaten in Dokumentation.
- Live-Pfade nur bewusst und sparsam, falls ueberhaupt lokal geloggt.
- Audit darf keine produktive Verarbeitung ausloesen.

Status: fachliche Empfehlung fuer Variante B, nicht implementiert.

### Finale Re-Evaluation direkt vor Speicherung

Ein spaeterer produktiver Executor muss direkt vor Speicherung folgende Pipeline frisch ausfuehren:

1. Schnittstellenprofil neu laden.
2. Pruefen: Profil existiert.
3. Pruefen: Profil ist UserDefined.
4. Pruefen: Profil ist nicht BuiltIn.
5. Evaluation neu erstellen.
6. Guard neu ausfuehren.
7. WarningConfirmation neu bewerten.
8. ActivationPlan neu erstellen.
9. Pruefen: keine Blocker.
10. Pruefen: `Ready` oder `ReadyWithAcceptedWarnings`.
11. Pruefen: Warnungsbestaetigung vorhanden, falls erforderlich.
12. Erst danach speichern.

Wenn eine Pruefung fehlschlaegt:

- keine Speicherung
- keine Profiländerung
- keine Verarbeitung
- klare Fehlermeldung an UI
- optional spaeterer Audit-/Logeintrag als abgebrochene Aktivierung

Status: zwingende V1-Pflicht, nicht implementiert.

### `IsAttachmentProcessingEnabled` bleibt getrennt

V1-Linie:

- Aktivierung des Schnittstellenprofils aktiviert nicht automatisch die XDT-Anhang-Automatik.
- `IsAttachmentProcessingEnabled` bleibt eigene bewusste Einstellung.
- Ein Profil darf aktiv sein, auch wenn `IsAttachmentProcessingEnabled` `false` ist.
- Wenn XDT-Anhang `Required` konfiguriert ist, bleibt die Pruefung streng.
- Eine spaetere UI muss klar unterscheiden:
  - Schnittstellenprofil aktiv
  - XDT-Anhang-Automatik aktiv

Status: fachlicher V1-Vorschlag, nicht implementiert.

### V1-Executor-Spezifikation

Ein spaeterer V1-Executor darf nur speichern, wenn:

- Profil frisch geladen wurde.
- Profil UserDefined ist.
- Profil nicht BuiltIn ist.
- frische Evaluation `Ready` oder `ReadyWithWarnings` ergibt.
- Guard `Allowed` oder `AllowedWithWarnings` ergibt.
- bei `ReadyWithWarnings` eine gueltige Warnungsbestaetigung vorliegt.
- ActivationPlan `Ready` oder `ReadyWithAcceptedWarnings` ist.
- keine Blocker vorhanden sind.
- Persistenz nur das UserDefined-Schnittstellenprofil betrifft.
- `IsAttachmentProcessingEnabled` nicht automatisch geaendert wird.
- keine Verarbeitung sofort gestartet wird.

Noch nicht implementiert.

Weiterhin offen:

- konkreter Feldname des Aktivierungsflags, falls `IsActive` nicht endgueltig bestaetigt wird
- konkrete Repository-/Store-Methode
- Migrationsbedarf
- Audit-Speicherort
- Benutzerrollenmodell
- UI fuer produktive Warnungsbestaetigung
- UI fuer finalen Aktivieren-Button
- Verhalten bei parallelen Aenderungen
- Lizenzdurchsetzung
- ob bestimmte Warnungen zu Blockern werden
- konkrete produktive Deaktivierungsfuehrung gemaess nachfolgender Deaktivierungs-V1-Linie

## V1-Entscheidungslinie: Deaktivierung von Schnittstellenprofilen (Vorschlag)

Dieser Abschnitt beschreibt eine fachliche V1-Linie fuer eine spaetere produktive Deaktivierung. Er ist Konzept und Spezifikation, aber weiterhin keine Implementierung. Es gibt keinen Deaktivieren-Button, keinen produktiven DeactivationExecutor und keine Profil- oder Dateioperationen.

### Bedeutung von deaktiviert

Ein deaktiviertes Schnittstellenprofil bedeutet in V1:

- Das Profil wird vom produktiven periodischen Scan nicht mehr beruecksichtigt.
- Fuer dieses Profil werden keine neuen AIS-/Geraete-Dateipaare mehr gestartet.
- Fuer dieses Profil werden keine neuen XDT-Anhang-Wartephasen mehr begonnen.
- Das Profil bleibt als Konfiguration erhalten.
- Das Profil wird nicht geloescht.
- AIS-, Geraete- und Exportprofile bleiben unveraendert.
- Exportregeln und Templates bleiben unveraendert.
- BuiltIn-Profile bleiben unveraendert.
- Ordnerpfade und XDT-Anhang-Einstellungen bleiben erhalten.
- `IsAttachmentProcessingEnabled` wird durch reine Deaktivierung nicht automatisch geaendert.

Klarstellung:

- Deaktivieren ist nicht Loeschen.
- Deaktivieren ist nicht Ordnerbereinigung.
- Deaktivieren ist nicht Export-, Archiv- oder Fehlerordnerbereinigung.
- Deaktivieren erzeugt keine XDT-Dateien und startet keine Verarbeitung.

### Welche Profile duerfen deaktiviert werden?

V1-Linie:

- Deaktivierung betrifft nur UserDefined-Schnittstellenprofile.
- BuiltIn-Profile werden nicht direkt veraendert.
- Falls ein BuiltIn-Profil in der UI als aktiv wirken sollte, muss vor Implementierung fachlich geklaert werden, ob eine UserDefined-Kopie oder eine separate Aktivierungszuordnung verwendet wird.
- Importierte UserDefined-Profile, die nie aktiviert wurden, benoetigen keine Deaktivierung.
- Bereits deaktivierte Profile sollen bei einer spaeteren Deaktivierungsanforderung keinen Fehler erzeugen, sondern einen klaren Zustand wie `Bereits deaktiviert` liefern.

Bewusst offen:

- konkretes Aktivierungs-/Deaktivierungsflag
- konkrete Persistenzstelle beziehungsweise Store-Methode
- ob Aktivierung und Deaktivierung denselben Executor verwenden oder getrennte Executor-/Servicepfade bekommen

### Deaktivierung und laufende Verarbeitung

Problem:

Ein Schnittstellenprofil kann spaeter deaktiviert werden, waehrend bereits ein Paket laeuft oder wartet. Moegliche Faelle sind:

- AIS-Datei wartet auf Geraetedatei.
- AIS-/Geraete-Paar ist erkannt, wartet aber auf XDT-Anhang.
- XDT-Anhang ist optional und Timeout laeuft.
- XDT-Anhang ist required und Timeout laeuft.
- Export wird gerade vorbereitet.
- Paket ist bereits abgeschlossen.

Konservative V1-Entscheidungslinie:

- Deaktivierung startet keine neue Verarbeitung.
- Deaktivierung loescht keine wartenden Dateien.
- Deaktivierung loescht keine internen Paketinformationen ohne definierte Regel.
- Neue Pakete duerfen nach Deaktivierung nicht mehr begonnen werden.
- Fuer bereits wartende oder laufende Pakete muss vor Implementierung entschieden werden, ob sie kontrolliert zu Ende gefuehrt, kontrolliert abgebrochen oder in einen definierten Blockiert-/Abbruchstatus ueberfuehrt werden.

Konservative Empfehlung:

- Fuer V1 sollte Deaktivierung zunaechst verhindern, dass neue Pakete beginnen.
- Der Umgang mit bereits laufenden oder wartenden Paketen muss vor produktiver Implementierung explizit entschieden und getestet werden.

Weiterhin offen:

- ob laufende Pakete beendet oder abgebrochen werden
- wie ein Abbruch dokumentiert wird
- ob Benutzer vor Deaktivierung auf laufende Pakete hingewiesen werden
- ob Deaktivierung bei laufenden Paketen blockiert wird

### Deaktivierung und XDT-Anhang-Automatik

V1-Linie:

- Deaktivierung des Schnittstellenprofils veraendert `IsAttachmentProcessingEnabled` nicht automatisch.
- Wenn ein Profil spaeter wieder aktiviert wird, bleibt die zuvor konfigurierte XDT-Anhang-Automatik-Einstellung erhalten.
- Waehrend das Schnittstellenprofil deaktiviert ist, darf dessen XDT-Anhang-Automatik keine neuen Anhangverarbeitungen starten.
- Deaktivierung kopiert oder verschiebt keine Anhangdateien.
- Deaktivierung loescht keine Anhangdateien.
- Deaktivierung erzeugt keine Linkfelder `6302`, `6303`, `6304` oder `6305`.

Klarstellung:

Die XDT-Anhang-Automatik bleibt fachlich eine separate Einstellung, ist aber in einem deaktivierten Schnittstellenprofil praktisch nicht produktiv wirksam.

### Deaktivierung und Ordner / Dateien

Zwingende Sicherheitsgrenze:

Deaktivierung darf nicht:

- Ordner loeschen
- Ordner leeren
- Exportordner bereinigen
- Archivordner bereinigen
- Fehlerordner bereinigen
- unbekannte Dateien anfassen
- AIS-Dateien loeschen
- Geraete-Dateien loeschen
- XDT-Anhangdateien loeschen
- Dateien kopieren oder verschieben
- XDT-Exportdateien erzeugen

Deaktivierung ist nur eine Status-/Konfigurationsentscheidung, keine Dateioperation.

### Deaktivierung und Warnungsbestaetigung / Audit

Offene Fragen:

- Soll Deaktivierung auditierbar sein?
- Soll eine fruehere Warnungsbestaetigung bei Deaktivierung bestehen bleiben?
- Soll sie beim erneuten Aktivieren erneut erforderlich sein?
- Wird eine Warnungsbestaetigung ungueltig, wenn ein Profil deaktiviert und spaeter wieder aktiviert wird?
- Soll Deaktivierung einen Zeitstempel und Benutzerkontext speichern?

Konservative Empfehlung:

- Deaktivierung sollte spaeter auditierbar sein.
- Eine fruehere Warnungsbestaetigung sollte bei erneuter Aktivierung nicht blind wiederverwendet werden.
- Vor erneuter Aktivierung muss wieder frisch Evaluation, Guard, WarningConfirmation und ActivationPlan laufen.
- Falls Warnungen noch bestehen, muss erneut bewusst bestaetigt werden.
- Deaktivierung selbst darf keine Warnungen bestaetigen.

Status: Empfehlung, nicht implementiert.

### Finale Direktpruefung vor Deaktivierung

Ein spaeterer DeactivationExecutor oder ein ActivationExecutor mit Deaktivierungsmodus darf nicht ohne Pruefung speichern.

Direkt vor Deaktivierung muss geprueft werden:

- Profil existiert noch.
- Profil ist UserDefined.
- Profil ist nicht BuiltIn.
- Profil ist aktuell aktiv oder bereits deaktiviert.
- Persistenzstelle ist gueltig.
- Keine BuiltIn-Profile werden ueberschrieben.
- Es ist klar, wie mit laufenden Paketen umzugehen ist.
- Falls laufende Pakete vorhanden sind, muss die definierte V1-Regel angewendet werden.

Wenn eine Pruefung fehlschlaegt:

- nicht speichern
- keine Profilaenderung
- keine Verarbeitung
- klare Fehlermeldung an UI
- optional spaeterer Audit-/Logeintrag als abgebrochene Deaktivierung

Status: zwingende V1-Pflicht, nicht implementiert.

### UI-Fuehrung fuer spaetere Deaktivierung

Vorschlag:

Eine spaetere UI sollte Deaktivierung klar von Aktivierung trennen.

Moegliche UI-Regeln:

- Deaktivieren-Button nur bei aktivem UserDefined-Profil.
- Kein Deaktivieren-Button fuer BuiltIn.
- Vor Deaktivierung eine klare Zusammenfassung anzeigen:
  - Profilname
  - aktueller Aktivstatus
  - Hinweis: Keine neuen Pakete werden gestartet.
  - Hinweis: Dateien und Ordner werden nicht geloescht.
  - Hinweis auf offene oder laufende Pakete, falls spaeter ermittelbar.
- Benutzer muss bewusst bestaetigen.
- Kein Deaktivieren aus automatischem Import heraus.
- Kein Deaktivieren beim App-Start.

Bewusst offen:

- genaue UI-Position
- ob eigener Dialog oder Wizard
- ob laufende Pakete die Deaktivierung blockieren

### Deaktivierung und Lizenz

V1-Linie:

- Deaktivierung sollte grundsaetzlich auch bei lizenzpflichtigen Profilen moeglich sein.
- Lizenzmangel darf eine Deaktivierung nicht verhindern, sofern keine andere fachliche Regel dagegen spricht.
- Deaktivierung ist eine risikoreduzierende Aktion und sollte nicht durch Lizenzstatus blockiert werden.
- Lizenzstatus kann im Audit/Log erwaehnt werden, falls spaeter Audit umgesetzt wird.

Status: Vorschlag, nicht implementiert.

### Deaktivierung und erneute Aktivierung

Wenn ein Profil deaktiviert wurde und spaeter erneut aktiviert werden soll:

- erneute Aktivierung muss denselben vollstaendigen Aktivierungsprozess durchlaufen.
- keine alte Preview verwenden.
- keine alte Warnungsbestaetigung blind verwenden.
- frische Evaluation ausfuehren.
- frischen Guard ausfuehren.
- frische WarningConfirmation erstellen.
- frischen ActivationPlan erstellen.
- finale Re-Evaluation direkt vor Speicherung erzwingen.

Deaktivierung ist reversibel, aber Reaktivierung ist keine einfache Ruecknahme, sondern eine neue bewusste Aktivierung.

### V1-Deaktivierungsspezifikation

V1-Deaktivierung bedeutet:

- nur UserDefined-Schnittstellenprofilstatus aendern
- keine BuiltIn-Aenderung
- keine Loeschung
- keine Ordnerbereinigung
- keine Dateioperation
- keine Sofortverarbeitung
- keine Aenderung an `IsAttachmentProcessingEnabled`
- keine automatische Wiederverwendung alter Warnungsbestaetigungen
- neue Pakete werden nach Deaktivierung nicht mehr begonnen
- laufende oder wartende Pakete bleiben vor Implementierung gesondert zu entscheiden
- finale Pruefung direkt vor Speicherung ist Pflicht

Noch nicht implementiert.

Weiterhin offen:

- konkretes Deaktivierungsflag / Aktivierungsflag
- konkrete Store-Methode
- ob Aktivierung und Deaktivierung denselben Executor verwenden
- Umgang mit laufenden oder wartenden Paketen
- Audit-/Logformat fuer Deaktivierung
- UI fuer produktive Deaktivierung
- Benutzerrollenmodell
- Verhalten bei parallelen Aenderungen
- Reaktivierungsdetails
- Testmatrix fuer Deaktivierung

## V1-Entscheidungslinie: laufende und wartende Pakete (Vorschlag)

Dieser Abschnitt konkretisiert die fachliche Linie fuer Paket- und Wartezustaende bei spaeterer produktiver Aktivierung, Deaktivierung und Reaktivierung. Er ist Konzept und Spezifikation, aber weiterhin keine Implementierung. Paketlogik, Scanlogik, UI und Persistenz werden dadurch nicht geaendert.

### Relevante Paket-/Verarbeitungszustaende

Fachlich relevante Zustaende im Umfeld der aktuellen automatischen Verarbeitung sind:

- kein Paket aktiv
- AIS-Datei erkannt, wartet auf Geraetedatei
- neue AIS-Datei ersetzt aeltere wartende AIS-Datei
- AIS-/Geraete-Paar erkannt
- Dateistabilitaet wird abgewartet
- XDT-Anhang-Wartezeit laeuft
- optionaler XDT-Anhang fehlt noch
- Pflicht-XDT-Anhang fehlt noch
- mehrere Anhaenge gefunden oder Zuordnung unsicher
- Export wird vorbereitet
- Export erfolgreich abgeschlossen
- Export fehlgeschlagen
- Pflicht-Anhang-Timeout als terminaler Paketfehler
- Paket terminal abgeschlossen
- neue Untersuchung kann danach wieder starten

V1-Leitplanke:

- Aktivierungs- und Deaktivierungsentscheidungen duerfen diese Zustaende nicht unkontrolliert veraendern.
- Keine Aktivierungs- oder Deaktivierungsentscheidung darf Paketinformationen stillschweigend loeschen.
- Keine Aktivierungs- oder Deaktivierungsentscheidung darf Dateien als Nebenwirkung anfassen.

### Aktivierung und bestehende Dateien

V1-Linie:

Aktivierung darf keine vorhandenen Dateien sofort verarbeiten.

Das bedeutet:

- Aktivierung startet keine Sofortverarbeitung.
- Aktivierung scannt nicht unmittelbar Ordner.
- Aktivierung erzeugt keine XDT-Datei.
- Aktivierung kopiert oder verschiebt keine Anhangdateien.
- Aktivierung loescht keine Dateien.
- Aktivierung bereinigt keine Ordner.
- Der bestehende periodische Scan bleibt das Betriebsmodell.

Offene Fragen:

- Soll bei Aktivierung ein Startzeitpunkt oder Fingerprint gesetzt werden, damit alte Dateien vor Aktivierung nicht unbeabsichtigt verarbeitet werden?
- Darf der erste Scan nach Aktivierung nur Dateien beruecksichtigen, die nach Aktivierungszeitpunkt erstellt oder geaendert wurden?
- Muss der Benutzer vor Aktivierung auf vorhandene Dateien in AIS-, Geraete- oder Anhangordnern hingewiesen werden?

Konservative Empfehlung:

- V1 sollte vermeiden, dass bei Aktivierung sofort alte Importordnerbestaende verarbeitet werden.
- Vor produktiver Implementierung braucht es eine klare Regel fuer Altbestand in AIS-, Geraete- und Anhangordnern.
- Ohne diese Regel darf produktive Aktivierung nicht als "direkt nach Klick verarbeitet Bestand" umgesetzt werden.

### Deaktivierung und neue Pakete

V1-Linie:

Nach Deaktivierung duerfen fuer dieses Schnittstellenprofil keine neuen Pakete mehr begonnen werden.

Das bedeutet:

- keine neue AIS-Datei als neuer Auftrag beginnen
- keine neue Geraetedatei zu einem neuen Auftrag zuordnen
- keine neue XDT-Anhang-Wartephase beginnen
- keine neue Exportvorbereitung beginnen

Deaktivierung wirkt ab dem Zeitpunkt der wirksamen Statusaenderung fuer neue Paketstarts.

### Deaktivierung bei AIS-Datei wartet auf Geraetedatei

Fall:

Eine AIS-Datei wurde erkannt und wartet noch auf die passende Geraetedatei.

Fachliche Optionen:

- Option A: Wartendes Paket kontrolliert abbrechen. Es entsteht kein Export. Die AIS-Datei bleibt unangetastet oder wird nur gemaess bereits definierter Fehler-/Archivregel behandelt.
- Option B: Wartendes Paket noch bis Timeout laufen lassen. Nach Timeout entsteht kein Export.
- Option C: Deaktivierung wird blockiert, solange ein Paket wartet.

Konservative V1-Empfehlung:

- Deaktivierung soll neue Pakete verhindern.
- Fuer eine bereits wartende AIS-Datei ist kontrollierter Abbruch oder definierter Timeout fachlich zu entscheiden.
- Keine Datei darf geloescht werden.
- Keine pauschale Ordnerbereinigung.
- Ohne klare Implementierungsregel darf dieser Fall nicht produktiv umgesetzt werden.

### Deaktivierung bei erkanntem AIS-/Geraete-Paar

Fall:

AIS- und Geraetedatei sind bereits erkannt, aber der Export ist noch nicht abgeschlossen.

Fachliche Optionen:

- Option A: Verarbeitung kontrolliert zu Ende fuehren. Vorteil: Die bereits begonnene Untersuchung wird abgeschlossen. Risiko: Nach Deaktivierung entsteht noch ein Export.
- Option B: Verarbeitung kontrolliert abbrechen. Vorteil: Deaktivierung wirkt sofort. Risiko: Untersuchungsergebnis geht nicht zurueck ins AIS.
- Option C: Deaktivierung blockieren, bis das Paket abgeschlossen ist. Vorteil: klare Datenkonsistenz. Risiko: Benutzer kann das Profil nicht sofort deaktivieren.

Konservative V1-Empfehlung:

- Kein stiller Abbruch.
- Kein unkommentierter Export nach Deaktivierung.
- Eine spaetere UI sollte anzeigen, wenn laufende Pakete vorhanden sind.
- V1 sollte entweder Deaktivierung bei laufendem Paket blockieren oder Benutzer bewusst entscheiden lassen.
- Diese Entscheidung muss vor Implementierung getroffen und getestet werden.

### Deaktivierung waehrend XDT-Anhang-Wartezeit

Fall:

AIS-/Geraete-Paar ist vollstaendig, aber XDT-Anhang-Wartezeit laeuft.

Varianten:

- optionaler Anhang
- Pflicht-Anhang
- mehrere Anhaenge oder unsichere Zuordnung
- Dateistabilitaet noch nicht erfuellt

Konservative V1-Empfehlung:

- Keine Anhangdateien kopieren, verschieben oder loeschen.
- Keine Linkfelder `6302`, `6303`, `6304` oder `6305` erzeugen, nur weil deaktiviert wurde.
- Bereits begonnene Anhang-Wartephase darf nicht unkontrolliert abgebrochen werden.
- Bei Pflicht-Anhang darf keine erfolgreiche Ausgabe ohne Pflicht-Anhang erzeugt werden.
- Die Produktiventscheidung muss festlegen, ob die Wartephase kontrolliert zu Ende gefuehrt wird, das Paket kontrolliert abbricht oder Deaktivierung bis zum Abschluss blockiert.

### Deaktivierung bei Exportvorbereitung

Fall:

Export wird gerade vorbereitet oder steht unmittelbar bevor.

Konservative V1-Empfehlung:

- Kein halbfertiger Export.
- Keine teilweise geschriebenen XDT-Dateien.
- Wenn Export bereits begonnen hat, muessen Atomaritaet und Fehlerbehandlung vor Implementierung geklaert sein.
- Deaktivierung darf nicht mitten in einen Schreibvorgang eingreifen.
- V1 sollte entweder laufende Exportvorbereitung abschliessen lassen, Deaktivierung blockieren oder Abbruch nur vor Beginn des Schreibens erlauben.

### Aktivierung nach vorheriger Deaktivierung / Reaktivierung

Reaktivierung ist eine neue Aktivierung.

Das bedeutet:

- keine alten Preview-Daten verwenden
- keine alte Warnungsbestaetigung blind wiederverwenden
- keine alten Paketinformationen automatisch fortsetzen
- frische Evaluation ausfuehren
- frischen Guard ausfuehren
- frische WarningConfirmation erstellen
- frischen ActivationPlan erstellen
- finale Re-Evaluation direkt vor Speicherung erzwingen

Bewusst offen:

- ob alte wartende Pakete aus der Zeit vor Deaktivierung bei Reaktivierung ignoriert, blockiert oder neu bewertet werden
- ob ein Aktivierungszeitpunkt oder Fingerprint eingefuehrt wird

Konservative Empfehlung:

- Reaktivierung darf alte wartende Pakete nicht stillschweigend wiederaufnehmen.

### Paketinformationen / interne Zustaende

Offene Fragen:

- Gibt es interne Warteschlangen oder nur fluechtige Scan-Zustaende?
- Wo werden wartende AIS-Auftraege gehalten?
- Wie werden terminale Paketfehler abgebildet?
- Sind Paketinformationen persistent oder nur in-memory?
- Wie wird beim Profilwechsel oder bei Deaktivierung mit In-Memory-Zustaenden umgegangen?

Konservative V1-Linie:

- Ohne klare Modellierung duerfen Paketinformationen bei Deaktivierung nicht stillschweigend geloescht werden.
- Neue Pakete nach Deaktivierung verhindern.
- Bestehende Pakete nur nach definierter Regel abbrechen, beenden oder blockieren.

### UI-Anforderungen fuer spaetere produktive Deaktivierung

Eine spaetere produktive Deaktivierungs-UI sollte vor der Deaktivierung anzeigen:

- Profilname
- aktueller Aktivstatus
- ob wartende oder laufende Pakete bekannt sind
- welcher Zustand vorliegt:
  - wartet auf Geraetedatei
  - wartet auf XDT-Anhang
  - Exportvorbereitung
  - Fehler oder Timeout
- welche Folge Deaktivierung hat:
  - neue Pakete werden nicht mehr begonnen
  - vorhandene Dateien werden nicht geloescht
  - laufende Pakete werden nach definierter Regel behandelt

Falls Paketzustand nicht ermittelbar ist:

- UI muss das ehrlich anzeigen.
- Keine falsche Sicherheit vortaeuschen.

### Kompakte V1-Empfehlung

Konservative V1-Linie:

- Aktivierung startet keine Sofortverarbeitung.
- Deaktivierung verhindert neue Paketstarts.
- Deaktivierung loescht keine Dateien und bereinigt keine Ordner.
- Bereits laufende oder wartende Pakete duerfen nicht stillschweigend verworfen werden.
- Vor produktiver Implementierung muss entschieden werden, ob laufende Pakete abgeschlossen werden, abgebrochen werden oder Deaktivierung blockieren.
- Fuer V1 ist `Deaktivierung blockiert, solange ein aktives oder wartendes Paket vorhanden ist` als konservativste Option zu pruefen.
- Reaktivierung ist eine neue Aktivierung und darf alte Pakete nicht stillschweigend wieder aufnehmen.

Noch nicht implementiert.

Weiterhin offen:

- konkrete Paketstatus-Erkennung
- ob Paketstatus persistent oder nur in-memory ist
- Umgang mit AIS-Datei wartet auf Geraetedatei
- Umgang mit XDT-Anhang-Wartezeit
- Umgang mit Exportvorbereitung
- ob Deaktivierung bei laufenden Paketen blockiert
- ob Benutzer laufende Pakete bewusst abbrechen darf
- ob Aktivierungszeitpunkt oder Fingerprint eingefuehrt wird
- ob Altbestand in Importordnern beim Aktivieren ignoriert wird
- wie Reaktivierung mit Altbestand umgeht
- Testmatrix fuer Paket-/Deaktivierungsfaelle

## Kompakter V1-Entscheidungskern

Die detaillierten V1-Regeln stehen in den Abschnitten oben. Dieser Schlussabschnitt ersetzt den frueheren langen Detailbacklog und haelt die Umsetzungslinie bewusst knapp: kein Verwaltungsmonster, kein langer Wizard und keine mehrfachen Bestaetigungen derselben Information.

### Schlanke Benutzerfuehrung

Die spaetere produktive UI soll den Benutzer mit wenigen klaren Schritten fuehren:

1. Profil auswaehlen.
2. Pruefung ansehen.
3. Aktivierung vorbereiten.
4. Falls Warnungen vorhanden sind: bewusst bestaetigen.
5. Final aktivieren oder spaeter bewusst deaktivieren.

Leitplanken fuer die UI:

- Kein ueberlanger Wizard.
- Keine doppelte Benutzerabfrage fuer dieselbe Warnung.
- `Ready`-Profile sollen ohne unnoetige Zusatzhuerden aktivierbar sein.
- Warnungen werden nur bestaetigt, wenn tatsaechlich Warnungen vorhanden sind.
- Vorschau, Warnungsbestaetigung und finale Aktion muessen eindeutig getrennt bleiben.
- Keine versteckte Aktivierung, keine automatische Aktivierung durch Import und keine Aktivierung beim App-Start.

### Harte V1-Regeln

- UserDefined-Schnittstellenprofile duerfen spaeter aktiviert werden; BuiltIn-Profile nicht.
- `Ready` darf aktivierbar sein.
- `ReadyWithWarnings` darf nur nach bewusster Warnungsbestaetigung aktivierbar sein.
- `Blocked`, `Unknown` und `NotAvailable` duerfen nicht aktiviert werden.
- Direkt vor produktiver Speicherung ist eine frische Evaluation, Guard-Pruefung, WarningConfirmation und Planerstellung Pflicht.
- `IsAttachmentProcessingEnabled` bleibt eine eigene Einstellung und wird nicht automatisch geaendert.
- Aktivierung startet keine Sofortverarbeitung, keinen App-Start-Prozess, keinen Dienst, keinen Autostart und keinen FileSystemWatcher.
- Deaktivierung ist keine Loeschung, keine Ordnerbereinigung und keine Dateioperation.
- Deaktivierung verhindert neue Paketstarts.
- Laufende oder wartende Pakete duerfen nicht stillschweigend verworfen, geloescht oder fortgesetzt werden.
- Keine neue MEDISTAR-/AIS-Exporttemplate-Default-Logik, keine automatische `6330`-Zeilenlogik und keine harte Lizenzsperre ohne eigene Spezifikation.

### Muss vor produktiver Aktivierung oder Deaktivierung entschieden sein

- konkretes Aktivierungsflag, voraussichtlich `IsActive`, final bestaetigen
- konkrete UserDefined-Store-Methode und BuiltIn-Overwrite-Schutz
- minimales Warnungsbestaetigungs-/Auditmodell
- finale Re-Evaluation im Executor inklusive frisch geladenem Profil
- Paketregel fuer laufende oder wartende Pakete bei Deaktivierung; konservativ zu pruefen ist Blockieren der Deaktivierung, solange ein aktives oder wartendes Paket vorhanden ist
- UI fuer finalen Aktivieren-/Deaktivieren-Schritt inklusive klarer Buttontexte
- Testmatrix fuer `Ready`, `ReadyWithWarnings`, `Blocked`, BuiltIn/Nicht-UserDefined und Paketfaelle

### Kann spaeter separat entschieden werden

- erweiterte Auditdetails, Logaufbewahrung und Audit-Speicherort
- Rollenmodell, falls das Projekt spaeter Rollen einfuehrt
- Lizenzdurchsetzung und Signaturpruefung
- Fingerprint-/Revisionserkennung ueber die Mindestregel "frisch laden und neu pruefen" hinaus
- Sonderfaelle fuer bewusstes Abbrechen laufender Pakete
- Reaktivierungsdetails fuer Altbestand in Importordnern
- genaue UI-Position eines Deaktivieren-Buttons

### Konsequenz fuer die erste Executor-Stufe

Die erste technische Stufe darf nur Backend sein. Wenn der Executor nicht sicher frisch laden und gezielt ein UserDefined-Schnittstellenprofil speichern kann, muss er defensiv nicht-produktiv bleiben. In diesem Fall liefert er einen klaren Status wie `ReadyButNotExecuted`, `NotAvailable`, `Blocked` oder `RequiresWarningConfirmation`, veraendert aber kein Profil, speichert nichts und startet keine Verarbeitung.

### Technische Bestandspruefung und erster Executor-Stand

Aktueller technischer Befund:

- `IsActive` existiert am `InterfaceProfileDefinition` und ist der naheliegende Kandidat fuer das Aktivierungsflag.
- BuiltIn/UserDefined-Schutz ist ueber `ProfileMetadata.IsBuiltIn` und `ProfileMetadata.IsUserDefined` modelliert.
- UserDefined-Schnittstellenprofile werden im bestehenden Profilkatalog unter `profiles/interfaces` gespeichert.
- `AppDataPaths` und `ProfileCatalogService.Load(...)` bilden den bestehenden Kontext fuer frisches Laden aus dem lokalen Profilkatalog.
- `ProfileCatalogService.SaveInterfaceProfileDefinition(..., overwriteExisting: true)` kann ein Schnittstellenprofil ueberschreiben, erzwingt aber selbst keine produktive finale Re-Evaluation.
- `ProfileMetadata` enthaelt `CreatedAt` und `UpdatedAt`, aber noch keine eigenstaendige Revision, Version fuer Parallelitaet oder Konfigurations-Fingerprint.
- `InterfaceProfileActivationExecutorRequest` transportiert jetzt Zielprofil-ID/-Name, `OperationMode` (`ValidateOnly`, `Activate`, `Deactivate`), Quelle/Anforderungszeitpunkt, Preview-Zeitpunkt, erwartete Preview-Statuswerte, optionalen Fingerprint sowie Warnungsbestaetigungs-Token/-Codes.
- Der Request enthaelt bewusst keine konkrete Store-Service-Referenz. Ein produktiver Executor soll Loader/Store nach Projektmuster per Konstruktor oder vergleichbarer Backend-Verdrahtung erhalten, nicht ueber beliebige Serviceobjekte im Request.
- `InterfaceProfileActivationExecutorResult` kann fehlende produktive Faehigkeiten nun ausdruecken: frisches Laden erforderlich, sichere UserDefined-Speicherung erforderlich, finale Re-Evaluation erforderlich, nicht ausgefuehrt, nicht gespeichert, kein Profil geaendert und fehlende Capabilities.
- Fuer den spaeteren Executor ist eine kleine Store-/Loader-Abstraktion vorbereitet: `IInterfaceProfileActivationProfileStore` mit Load-/Save-Resultmodellen, einem defensiven `InterfaceProfileActivationProfileStoreStub` und dem ValidateOnly-Adapter `InterfaceProfileActivationProfileCatalogStore`.
- Diese Abstraktion ist executor-nah und kein allgemeiner Profilstore. Sie modelliert Zielprofil-ID, gefunden/nicht gefunden, UserDefined/BuiltIn-Schutz und nicht implementierte Speicherung.
- Der aktuelle Store-Stub nutzt nur einen uebergebenen In-Memory-Bestand, ruft keinen Profilkatalog, schreibt nichts und fuehrt keine Datei-/Ordneroperation aus.
- `InterfaceProfileActivationProfileCatalogStore` kann ueber `ProfileCatalogService.Load(...)` und `AppDataPaths` ein Schnittstellenprofil frisch lesen und UserDefined/BuiltIn/Nicht-UserDefined bewerten.
- Die Save-Seite des Catalog-Adapters bleibt absichtlich ValidateOnly/DryRun: `SaveInterfaceProfileDefinition` wird nicht aufgerufen, es wird keine JSON-Datei geschrieben und `IsActive` wird nicht veraendert.

Konservative Entscheidung fuer die erste technische Stufe:

- Es wird noch kein produktiver Backend-Executor gebaut.
- `InterfaceProfileActivationExecutorStub` implementiert `IInterfaceProfileActivationExecutor` defensiv.
- Der Stub bewertet die uebergebenen Preconditions, liefert sinnvolle Statuswerte und benennt fehlende Voraussetzungen jetzt praeziser, etwa fehlende Zielprofil-ID, fehlenden Loader/Store-Kontext, erforderliches frisches Laden und erforderliche finale Re-Evaluation.
- Optional kann der Stub im `ValidateOnly`-Modus einen `IInterfaceProfileActivationProfileStore` verwenden: Dann wird das Zielprofil frisch geladen, `NotFound`, BuiltIn und Nicht-UserDefined werden ueber den Store bewertet und ein Save-DryRun bleibt ohne finale Re-Evaluation blockiert.
- Diese Store-Anbindung ist weiterhin nicht produktiv. Sie setzt kein `IsActive`, ruft keine produktive Speichermethode auf und ist nicht mit UI-Flows verbunden.
- Er setzt `IsActive` nicht, speichert nichts, aendert kein Profil, startet keine Verarbeitung und fuehrt keine Datei-/Ordneroperation aus.
- Der vorbereitete `InterfaceProfileActivationProfileStoreStub` speichert ebenfalls nichts. Selbst ein formal UserDefined-faehiger SaveRequest endet mit `SaveNotImplemented`.
- Der vorbereitete `InterfaceProfileActivationProfileCatalogStore` ist die erste echte Projektstruktur-Anbindung, aber ebenfalls nicht produktiv speichernd: UserDefined-Save wird nur als `SaveWouldBeAllowed`/DryRun gemeldet, wenn finale Re-Evaluation nachgewiesen ist.

Grund:

Eine echte Aktivierung waere erst vertretbar, wenn der Executor selbst frisch laden, finale Re-Evaluation erzwingen und gezielt eine sichere UserDefined-Speicherung ausfuehren kann. Dieser Schritt ist bewusst noch nicht erreicht.
