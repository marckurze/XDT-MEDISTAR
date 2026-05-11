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
