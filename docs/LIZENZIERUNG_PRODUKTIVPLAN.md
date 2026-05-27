# XDTBox Lizenzierung - Produktivplan

Stand: 2026-05-27

Dieser Plan beschreibt den aktuellen Lizenzierungsstand von XDTBox, die festgelegten fachlichen V1-Entscheidungen und die naechsten technischen Schritte fuer die Durchsetzung der signierten Offline-Lizenz. Die kryptografische Pruefung und der `.xdtboxlic`-Import sind vorbereitet; es wurde bewusst noch keine harte produktive Sperre in Parsern, Exportern oder der laufenden Geraeteverarbeitung aktiviert.

## 1. Fachliche V1-Entscheidungen

Lizenzpflichtig ist in XDTBox V1 ausschliesslich die Anzahl aktivierter Geraeteanbindungen, technisch also die Anzahl aktiver Schnittstellenprofile.

Nicht separat lizenzpflichtig sind:

- LAN/UNC-Dateiworkflow
- SerialRs232
- Dokumentanhang
- Profile & Templates
- RS232-Testbereich
- Parseranalyse
- Lizenzanforderung
- Einrichtung und Konfiguration

Die Lizenz begrenzt nur:

- `MaxActiveDeviceConnections`

Beispiel: Eine Lizenz fuer 3 Geraeteanbindungen erlaubt maximal 3 aktive Schnittstellenprofile. Weitere Profile duerfen angelegt, importiert und konfiguriert werden, duerfen aber nach Ablauf der Karenzzeit nicht aktiv starten beziehungsweise nicht aktiv betrieben werden, solange die Lizenzgrenze ueberschritten ist.

Nach Ablauf der Karenzzeit werden nicht lizenzierte aktive Geraeteanbindungen blockiert. Die Blockade muss zentral vor dem Start der Ueberwachung beziehungsweise vor produktivem Betrieb stattfinden und klar angezeigt werden. Parser und XDT-Erzeugung duerfen nicht mitten in der Verarbeitung hart abbrechen.

Bei ungueltiger Signatur lautet die verbindliche Meldung:

```text
Lizenzdatei ist ungültig oder wurde verändert.
```

Bei ungueltiger Signatur werden betroffene aktive Geraeteanbindungen blockiert. Es gibt keine stille Weiterverarbeitung mit manipulierter Lizenz.

InstallationId bleibt fuehrend. Bei Hardwaretausch muss eine neue Lizenzanforderung erzeugt werden. Ein spaeterer Tab `Hardwareumzug` soll den Export und Import der gesamten Einrichtung vorbereiten. Nach Hardwareumzug gilt eine Karenzzeit von 7 Tagen, um eine neue Lizenz einzuspielen.

Hinweis im Lizenz-Tab:

```text
Achtung: Bei Hardwaretausch bitte neue Lizenz anfordern, Karenzzeit 7 Tage ab Umzug der Hardware.
```

## 2. Aktueller Iststand

XDTBox enthaelt bereits einen lokalen Lizenz-Prototypen. Er ist Anzeige-, Anfrage-, Import- und Bewertungsschicht, aber noch keine produktive Durchsetzungsschicht.

Vorhanden sind:

- Tab `Lizenz` in `MainWindow.xaml`
- lokale Installationskennung
- Lizenzanfrage als JSON
- Lizenzdatei-Import fuer signierte `.xdtboxlic`-Dateien
- Legacy-Import alter JSON-Lizenzen als unsignierter Uebergang
- Lizenzstatusanzeige
- Bewertung aktiver lizenzpflichtiger Schnittstellenprofile
- Tabellenanzeige lizenzierter beziehungsweise nicht gedeckter Anbindungen
- Karenzzeitmodell fuer nicht gedeckte aktive lizenzpflichtige Schnittstellenprofile
- Aktivierungsvorschau mit Lizenzhinweisen, aber ohne harte Sperre
- echte kryptografische Signaturpruefung fuer V1-Lizenzen mit RSA-PSS/SHA-256
- Public-Key-Auswahl ueber `KeyId`

Nicht produktiv vorhanden sind:

- final angeschlossenes Produktiv-Gate fuer Start/Aktivierung
- Hardwareumzug-Tab
- Online-Lizenzierung

Wichtig: Das bestehende Feld `Signature` in `LicenseInfo` ist Legacy-Anzeige. Produktive V1-Lizenzen verwenden stattdessen `LicenseEnvelope.SignatureBase64Url`; Legacy-JSON-Lizenzen werden nicht als signiert gueltig angezeigt.

## 3. Beteiligte Klassen und Dateien

UI:

- `XdtDeviceBridge.App/MainWindow.xaml`
- `XdtDeviceBridge.App/MainWindow.xaml.cs`

Bestehende Core-/Infrastructure-Bausteine:

- `LicenseInfo`
- `LicenseInfoValidator`
- `LicenseType`
- `LicenseStatus`
- `LicenseEvaluator`
- `LicenseEvaluationResult`
- `LicenseRequest`
- `LicenseRequestDevice`
- `LicenseRequestBuilder`
- `InstallationInfo`
- `LicensedDeviceStateEvaluator`
- `LicensedDeviceGracePeriodService`
- `LicenseFileRepository`
- `LicenseRequestFileRepository`
- `LicensedDeviceGracePeriodRepository`

Vorbereitete V1-Bausteine:

- `LicenseEnvelope`
- `LicensePayload`
- `LicenseEnvelopeReader`
- `LicenseSignatureVerifier`
- `LicensePublicKeyProvider`
- `LicenseImportService`
- `LicenseSignatureVerificationStatus`
- `LicenseV1PolicyEvaluator`
- `LicenseV1DeviceConnectionCounter`
- `XdtBoxLicenseConstants`
- internes Console-Projekt `XdtBox.LicenseIssuer`

Persistenzorte laut `AppDataPathProvider`:

- `%LocalAppData%\XdtDeviceBridge\installation.json`
- `%LocalAppData%\XdtDeviceBridge\licenses\license.xdtboxlic`
- `%LocalAppData%\XdtDeviceBridge\licenses\license.json` fuer Legacy/Uebergang
- `%LocalAppData%\XdtDeviceBridge\licenses\device-grace-periods.json`
- `%LocalAppData%\XdtDeviceBridge\license-requests\`

## 4. InstallationInfo und Maschinenbindung

Aktuell enthaelt `InstallationInfo`:

- `InstallationId`
- `MachineName`
- `UserName`
- `IsTerminalServer`
- `CreatedAt`

V1-Entscheidung:

- `InstallationId` bleibt fuehrend.
- Bei Hardwaretausch wird eine neue Lizenzanforderung erzeugt.
- Nach Hardwareumzug gilt eine 7-Tage-Karenzzeit.
- `MachineName` und `UserName` sollen perspektivisch nicht mehr im Klartext Bestandteil der Lizenzanforderung sein.
- Falls spaeter ein MachineFingerprint verwendet wird, dann nur gehasht beziehungsweise pseudonymisiert.

## 5. Konkretes V1-Lizenzmodell

XDTBox V1 verwendet eine offlinefaehige signierte JSON-Lizenz.

Format:

- Datei-Endung `.xdtboxlic`
- JSON-Envelope mit Base64Url-Payload und Base64Url-Signatur
- asymmetrische Signatur ueber exakt die Payload-Bytes
- Algorithmus in V1: `RSA-PSS-SHA256`
- privater Schluessel nur beim Hersteller
- oeffentlicher Schluessel in der App
- keine Secrets in der App
- kein privater Produktionsschluessel im Repository

RSA-PSS mit SHA-256 wurde fuer V1 gewaehlt, weil es in .NET 8 ohne zusaetzliche Kryptografie-Abhaengigkeit verfuegbar ist. Ed25519 bleibt spaeter moeglich, wenn dafuer eine saubere, langfristig gepflegte Abhaengigkeit oder native Plattformunterstuetzung genutzt wird.

### LicenseEnvelope

```text
string PayloadBase64Url
string SignatureBase64Url
string Algorithm
string KeyId
string FormatVersion
```

### LicensePayload

```text
string LicenseId
string ProductCode = "XDTBOX"
string LicenseeName
string? CustomerNumber
string InstallationId
int MaxActiveDeviceConnections
DateTime ValidFromUtc
DateTime ValidUntilUtc
int GraceDays = 7
DateTime IssuedAtUtc
string Issuer = "Technik-Apparat"
string LicenseType
string? Notes
```

Keine personenbezogenen Daten werden erzwungen. Patientendaten duerfen nie in Lizenzanfrage oder Lizenzdatei enthalten sein.

## 6. Hersteller-Lizenzerstellung

Der Hersteller kann aus einer Lizenzanforderung oder einer expliziten InstallationId eine signierte Lizenzdatei erzeugen.

Internes Tool:

```text
XdtBox.LicenseIssuer
```

Aufgaben:

- Lizenzanforderung lesen
- Lizenznehmer, Laufzeit und `MaxActiveDeviceConnections` erfassen
- `LicensePayload` erzeugen
- Payload kanonisch serialisieren
- Payload mit privatem RSA-PSS-Schluessel signieren
- `LicenseEnvelope` schreiben
- Datei mit Endung `.xdtboxlic` erzeugen

Beispielaufruf:

```powershell
XdtBox.LicenseIssuer.exe --request "C:\XDTBox\Lizenzaktivierung\requests\praxis.json" --licensee "Praxis Muster" --customer-number "K12345" --max-active-device-connections 3 --valid-from "2026-05-27" --valid-until "2027-05-27" --key-id "xdtbox-prod-2026-01" --private-key "C:\XDTBox\Lizenzaktivierung\keys\xdtbox_private.pem" --out "C:\XDTBox\Lizenzaktivierung\licenses\praxis-muster.xdtboxlic"
```

Das Tool ist framework-dependent fuer Windows x64 nach `C:\XDTBox\Lizenzaktivierung` veroeffentlichbar. Der private Schluessel wird nur ueber `--private-key` aus einem externen PEM-Pfad geladen.

`XdtBox.LicenseIssuer` ist ein Kommandozeilenwerkzeug. Produktive Nutzung erfolgt aus PowerShell oder CMD mit Parametern. Ein Doppelklick beziehungsweise Start ohne Parameter zeigt Hilfe, Beispiel, Private-Key-Hinweise und wartet in einer interaktiven Konsole auf Tastendruck, damit das Fenster nicht sofort verschwindet. Fuer reine Hilfe kann auch die mitveroeffentlichte Datei `XdtBox.LicenseIssuer.Hilfe.bat` genutzt werden.

Mehrzeiliges CMD-Beispiel:

```bat
XdtBox.LicenseIssuer.exe ^
  --request "C:\XDTBox\Lizenzaktivierung\requests\kunde-request.json" ^
  --licensee "Praxis Muster" ^
  --max-active-device-connections 3 ^
  --valid-from "2026-05-27" ^
  --valid-until "2027-05-27" ^
  --grace-days 7 ^
  --license-type "Production" ^
  --key-id "xdtbox-prod-2026-01" ^
  --private-key "C:\XDTBox\Lizenzaktivierung\keys\xdtbox_private.pem" ^
  --out "C:\XDTBox\Lizenzaktivierung\licenses\praxis-muster.xdtboxlic"
```

Sicherheitsregel:

- Privater Produktionsschluessel kommt nicht ins Repository.
- Privater Produktionsschluessel kommt nicht in die App.
- Testschluessel sind nur fuer Unit-Tests erlaubt und muessen klar als Testschluessel markiert sein.
- Es wird keine Dummy-Produktionssignatur eingebaut.
- In der App liegt nur ein Public Key. Der private Produktionsschluessel wird ausschliesslich beim Hersteller gehalten.

## 7. Lizenzdatei und Lizenzkey

V1 bevorzugt Dateiimport:

- `.xdtboxlic` enthaelt `LicenseEnvelope` als JSON.
- `PayloadBase64Url` enthaelt die unveraenderten signierten Payload-Bytes.
- `SignatureBase64Url` enthaelt die Signatur ueber diese Payload-Bytes.
- Legacy-`license.json` bleibt fuer den Uebergang lesbar, wird aber als unsigniert gekennzeichnet.

Spaeter kann ein Textkey ergaenzt werden:

- technisch gleicher Inhalt
- Base64Url-kodiertes `LicenseEnvelope`
- Prefix zum Beispiel `XDTBOX-1-`

Keine einfachen Seriennummern ohne Signatur verwenden. Ein kurzer Key wie `XDTB-ABCD-EFGH` reicht nicht aus.

## 8. Verhalten bei Lizenzproblemen

Vorgesehenes V1-Verhalten:

- Gueltig: aktive Geraeteanbindungen duerfen starten.
- Bald ablaufend: dezenter Hinweis.
- Abgelaufen innerhalb Karenzzeit: Warnung, Weiterbetrieb bis Karenzende.
- Abgelaufen nach Karenzzeit: aktive nicht lizenzierte Geraeteanbindungen werden blockiert.
- MaxActiveDeviceConnections ueberschritten innerhalb Karenzzeit: Warnung.
- MaxActiveDeviceConnections ueberschritten nach Karenzzeit: betroffene aktive Geraeteanbindungen werden blockiert.
- Ungueltige Signatur: Meldung `Lizenzdatei ist ungültig oder wurde verändert.` und Blockade betroffener Geraeteanbindungen.
- InstallationId passt nicht: neue Lizenzanforderung erzeugen.
- Hardwaretausch: Einrichtung spaeter ueber `Hardwareumzug` exportieren/importieren, danach 7 Tage Karenzzeit fuer neue Lizenz.

## 9. Empfohlene technische Gates

Sinnvolle zentrale Gates fuer den naechsten Schritt:

1. Beim Speichern eines Schnittstellenprofils als aktiv.
2. Beim Aktivieren ueber einen spaeteren Aktivierungsassistenten.
3. Beim Starten der Ueberwachung eines aktiven Schnittstellenprofils.
4. Beim Import von Templatepaketen nur als Hinweis; importierte Profile bleiben inaktiv.

Nicht empfohlen:

- Gate im Parser
- Gate im XDT-Exportbuilder
- Gate in einzelnen Geraeteparsern
- separate Modul-Gates fuer LAN, RS232 oder Dokumentanhang in V1
- stille Fail-Closed-Sperre ohne UI-Meldung

## 10. Muss vor Produktivbetrieb

- Produktiven Public Key festlegen und einpflegen.
- Hersteller-Issuer-Tool mit privatem Schluessel ausserhalb des Repositories erstellen.
- Private Keys ausserhalb des Repositories halten.
- Lizenzdatei nur nach Signaturpruefung produktiv akzeptieren; `.xdtboxlic` ist dafuer vorbereitet.
- Lizenzrequest auf echte App-Version und finalen datensparsamen Inhalt umstellen.
- Lizenzrequest datensparsam ueberarbeiten.
- 7-Tage-Hardwareumzug-Karenz fachlich und technisch modellieren.
- Zentrales Gate vor Start/Aktivierung anschliessen.
- Manipulations-, Ablauf-, Installation- und MaxActiveDeviceConnections-Tests fuer das zentrale Gate ergaenzen.
- UI-Meldungen fuer ungueltige Signatur, Ablauf, Karenz, falsche Installation und Hardwareumzug finalisieren.

## 11. Sollte vor Produktivbetrieb

- Lizenzdetails kopierbar fuer Support machen.
- Alte Lizenz beim Import archivieren.
- Hinweis auf baldigen Ablauf ergaenzen.
- Lizenzstatus im Verarbeitungsbereich beziehungsweise Geraetefenster sichtbar machen.
- Lizenzverletzungen in Log/Audit aufnehmen.
- Spaeteren Tab `Hardwareumzug` planen.

## 12. Spaeter

- Tab `Hardwareumzug`:
  - gesamte Einrichtung exportieren
  - Einrichtung auf neuer Hardware importieren
  - 7-Tage-Karenz fuer neue Lizenz starten
- Online-Lizenzpruefung optional.
- Kundenportal optional.
- automatische Lizenzverlaengerung optional.
- Vertrags-/Rechnungsverwaltung ausserhalb der App.

## 13. Offene technische Folgeentscheidungen

- Wie wird die 7-Tage-Hardwareumzug-Karenz lokal manipulationsarm gespeichert?
- Wie werden betroffene Geraeteanbindungen im Monitoring-Fenster konkret markiert?
- Wann werden alte unsignierte Testlizenzen im Produktbetrieb vollstaendig deaktiviert?
- Wird der bestehende Tab `Lizenz` spaeter in `Lizenzierung` umbenannt?

## 14. Naechste Umsetzungsschritte

1. Produktiven Public Key und Key-Rotation-Prozess festlegen.
2. Privaten Produktionsschluessel extern sicher ablegen und Backup-/Zugriffsprozess definieren.
3. Lizenzanforderung final datensparsam machen und echte App-Version setzen.
4. Zentrale Start-/Aktivierungs-Gates anschliessen.
5. Monitoring- und Lizenz-Tab-Status fuer blockierte Geraeteanbindungen ergaenzen.
6. Hardwareumzug-Tab als separaten Folgeschritt spezifizieren.
