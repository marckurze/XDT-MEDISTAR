# XDTBox Lizenzmanager Web V1

Stand: 2026-06-19

`XdtBox.LicenseWeb` ist eine erste interne Hersteller-Webanwendung fuer den XDTBox Lizenzmanager. Sie liegt im Hauptrepo `XDT-MEDISTAR` und ist bewusst nicht Teil des statischen Website-Projekts `Webseite XDTBox`. Die statische Website bleibt unveraendert und kann spaeter nur auf ein separat gehostetes Portal verlinken.

## Status

- Webframework: ASP.NET Core 8 Razor Pages
- Projekt: `XdtBox.LicenseWeb`
- Solution: `XdtDeviceBridge.sln`
- WPF-Lizenzmanager bleibt parallel bei Version `1.11`
- XDTBox Kunden-App bleibt bei Version `1.12`
- es wurde kein Setup gebaut und keine bestehende App-Version erhoeht
- keine produktive Webfreigabe ohne separate Sicherheits- und Hostingabnahme

Razor Pages wurde gewaehlt, weil die Lizenzverwaltung serverseitig bleiben muss. Lizenzsignatur, Datenzugriff, PDF-Erzeugung und Backup laufen auf dem Server; im Browser liegen keine Lizenzlogik, keine Private Keys und keine Signaturdaten.

## Wiederverwendete Bausteine

- `XdtDeviceBridge.Core`: Kunden-, Installations-, Lizenzrequest-, Historien- und Geraetemodelle
- `XdtDeviceBridge.Infrastructure`: JSON-Repositories, Backup-Service, Lizenzrequest-Repository, Kunden-PDF-Exporter
- `XdtBox.LicenseIssuer`: serverseitige `.xdtboxlic`-Signaturerzeugung

Der lokale WPF-Lizenzmanager wird nicht entfernt und bleibt funktional.

## Login und Schutz

Die Webanwendung nutzt Cookie-Authentifizierung. Alle Lizenzmanager-Seiten sind geschuetzt; nur `/Account/Login` ist anonym erreichbar. Logout entfernt das Auth-Cookie.

Der Adminzugang wird serverseitig konfiguriert:

- `LicenseWeb:Admin:Username`
- `LicenseWeb:Admin:PasswordHash`
- `LicenseWeb:Admin:PasswordSalt`

Das Passwort wird nicht im Klartext gespeichert. V1 nutzt PBKDF2-SHA256 mit Salt. Wenn keine Admin-Konfiguration vorhanden ist, zeigt die Login-Seite einen klaren Hinweis und erlaubt keine Anmeldung.

## Datenroot

Der Datenroot kommt aus `LicenseWeb:DataRoot`. Ist kein Wert konfiguriert, verwendet die Webanwendung `%LocalAppData%\XDTBox\LicenseWeb`.

Der Datenroot darf nicht liegen in:

- `wwwroot`
- dem Web-Projektverzeichnis
- dem statischen Website-Projekt

Die Webanwendung blockiert DataRoot-Konfigurationen unterhalb von `wwwroot` oder dem Projektverzeichnis. Kundendaten, Lizenzdateien und Backups werden serverseitig abgelegt.

## Private-Key-Regeln

Der Private-Key-Pfad wird ausschliesslich serverseitig ueber `LicenseWeb:PrivateKeyPath` konfiguriert.

Harte Regeln:

- kein Private Key in `wwwroot`
- kein Private Key in JavaScript
- kein Private Key im statischen Website-Projekt
- kein Private Key im Repository
- kein Private Key in Backups
- kein Private Key in PDFs
- keine Private-Key-Inhalte in Fehlern oder Logs

Ist kein Private Key konfiguriert oder die Datei nicht vorhanden, bleiben Kundenliste, PDFs und Backup nutzbar. Die Lizenz-Erstellung ist dann deaktiviert und meldet verstaendlich, dass kein serverseitiger Private Key konfiguriert ist.

## Seiten und Funktionen

- Dashboard mit Kunden-, Installations-, Geraete- und Monatssummen
- Kundenliste mit Details, Kunden-PDF und Gesamt-PDF
- Kundendetails mit Stammdaten, Zahlungsdaten, Installationen, aktiven Anbindungen, Standorten, Historie und PDF-Export
- Lizenzanfrage-Upload mit Zuordnung ueber Kundennummer oder InstallationId
- serverseitige Lizenzdatei-Erzeugung und Download bei konfiguriertem Private Key
- Backup-Download und Restore-Upload ohne Private-Key-Daten
- Einstellungen fuer Einzelpreis netto, Datenroot-Status und Private-Key-Status

## Offene Punkte vor produktivem Hosting

- Hostingziel und Domain entscheiden
- HTTPS, Reverse Proxy und HSTS produktiv konfigurieren
- Admin-Credentials per User Secrets, Umgebungsvariablen oder Secret Store setzen
- Private Key in einen geschuetzten Server-/Key-Store ueberfuehren
- Rollenmodell und Audit-Log fuer mehrere Benutzer
- Datenbankpruefung, falls mehrere Serverinstanzen oder echte Mehrbenutzerlast geplant werden
- Backup-/Restore-Prozess organisatorisch abnehmen
- Sicherheitsreview fuer Uploadgroessen, Dateitypen, Logging und Deployment
- Monitoring und Server-Backup einrichten
- Link von `www.xdtbox.de` oder `lizenz.xdtbox.de` erst nach Hostingabnahme setzen
