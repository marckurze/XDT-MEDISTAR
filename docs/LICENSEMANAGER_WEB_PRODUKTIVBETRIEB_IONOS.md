# XDTBox Lizenzmanager Web - Produktivbetrieb / IONOS

Stand: 2026-06-19

Dieses Dokument beschreibt den aktuellen technischen Stand fuer `XdtBox.LicenseWeb` und die IONOS-Einordnung. Es ersetzt keine Hosting-Freigabe und keine Sicherheitsabnahme.

## Kurzfazit

`XdtBox.LicenseWeb` ist als geschuetzte ASP.NET-Core-8-Razor-Pages-Anwendung vorbereitet. Die App kann grundsaetzlich auf einem Windows-/ASP.NET-Core-Hosting laufen, wenn der Server diese Bedingungen erfuellt:

- .NET 8 Runtime vorhanden
- HTTPS mit gueltigem Zertifikat
- nicht-oeffentlicher `DataRoot` ausserhalb von Webroot, Publish-Ordnern und Download-/Asset-Ordnern
- nicht-oeffentlicher Private-Key-Pfad ausserhalb von Webroot, Publish-Ordnern und Download-/Asset-Ordnern
- Schreibrechte fuer DataRoot, Requests, Licenses und Backups
- Admin-Zugang ausschliesslich als Hash/Salt konfiguriert
- regelmaessiges Server-/Dateisystem-Backup

IONOS dokumentiert fuer Windows-Hosting-Pakete .NET 8 LTS mit `TargetFramework net8.0`. Gleichzeitig dokumentiert IONOS fuer Windows-Webhosting-Beschraenkungen unter anderem, dass ASP.NET-Anwendungen keine Dateien ausserhalb ihres eigenen Anwendungsverzeichnisses lesen duerfen und System-Umgebungsvariablen gesperrt sein koennen. Das ist fuer unseren sicheren Betrieb kritisch, weil DataRoot und Private Key gerade nicht im oeffentlichen Web-/Publishbereich liegen duerfen.

Empfehlung: Produktiv nicht auf ein rein restriktives Shared-Webhosting legen, solange IONOS nicht bestaetigt, dass ein nicht-oeffentlicher, app-lesbarer Daten-/Secret-Ordner ausserhalb des Webroots nutzbar ist. Sicherer sind IONOS VPS/Windows Server/Linux Server/Cloud oder ein ASP.NET-Core-Hosting mit explizit getrenntem App-, Data- und Secret-Speicher.

Quellen:

- IONOS: [.NET Core mit Windows Hosting Packages](https://www.ionos.com/help/hosting/net/using-net-core-with-windows-hosting-packages/)
- IONOS: [Restrictions for ASP.NET Applications](https://www.ionos.com/help/hosting/net/restrictions-for-aspnet-applications/)
- IONOS: [Parallel Operation of Several ASP.NET Core Applications](https://www.ionos.com/help/hosting/net/parallel-operation-of-several-aspnet-core-applications-on-one-hosting-package/)

## Aktueller Web-Stand

- Projekt: `XdtBox.LicenseWeb`
- Framework: ASP.NET Core 8 Razor Pages
- interne Web-Version: `0.2.0-web-production-prep`
- Desktop-XDTBox bleibt `1.12`
- Desktop-Lizenzmanager bleibt `1.11`
- kein Desktop-Setup wurde veraendert oder neu gebaut
- statische Website `Webseite XDTBox` bleibt unveraendert

Neue Produktivschutzpunkte:

- `ASPNETCORE_ENVIRONMENT=Production` startet nur mit konfiguriertem Hash-Admin.
- DataRoot wird gegen Webroot, Projekt-/Publish-Verzeichnis und oeffentliche Download-/Asset-Ordner geprueft.
- Private-Key-Pfad wird vor Signaturerzeugung gegen Webroot, Projekt-/Publish-Verzeichnis und oeffentliche Download-/Asset-Ordner geprueft.
- Lizenzsignatur bleibt deaktiviert, wenn Private Key fehlt, unsicher liegt oder nicht lesbar ist.
- Einstellungen zeigen eine Produktivdiagnose fuer Umgebung, DataRoot, Private Key, Backup, Admin und HTTPS.
- Backups enthalten Kunden, Einstellungen und Historie, aber keine Private Keys.
- `appsettings.Production.json`, `.pem` und `.key` werden nicht in Publish-Artefakte aufgenommen.

## Konfiguration

`XdtBox.LicenseWeb/appsettings.Production.example.json` enthaelt eine Platzhalterkonfiguration. Eine echte Produktivdatei darf nicht ins Repository und darf nicht in den Publish-Ordner kopiert werden.

Wichtige Werte:

```json
{
  "LicenseWeb": {
    "DataRoot": "D:\\xdtbox-licenseweb-data",
    "PrivateKeyPath": "D:\\xdtbox-licenseweb-secrets\\xdtbox-license-private.pem",
    "PublicBaseUrl": "https://lizenz.xdtbox.de",
    "EnvironmentLabel": "Produktion",
    "Admin": {
      "Username": "<admin-user>",
      "PasswordHash": "<PBKDF2-SHA256-Base64-Hash>",
      "PasswordSalt": "<PBKDF2-Base64-Salt>",
      "PasswordIterations": 100000
    }
  }
}
```

## Einzelkunden-Migration

Fuer die Uebernahme aus dem lokalen Desktop-Lizenzmanager gibt es eine gezielte Einzelkunden-Migration:

- aus LicenseManager-Backup
- oder aus einem lokalen Desktop-Datenroot, z. B. `C:\XDTBox\Lizenzaktivierung`
- Kundennummer frei waehlbar, fuer den aktuellen Fall vorbereitet mit `10172`
- es wird nur der passende Kunde uebernommen
- passende Historie wird anhand Kundennummer und InstallationId uebernommen
- vorhandene Web-Kundendaten werden per bestehender Merge-Logik zusammengefuehrt
- es erfolgt kein Voll-Restore und kein pauschales Ueberschreiben aller Webdaten

## Betrieb

Vor einer Live-Schaltung pruefen:

1. `dotnet publish XdtBox.LicenseWeb\XdtBox.LicenseWeb.csproj -c Release`
2. Publish-Artefakt auf Secrets scannen.
3. DataRoot und Private-Key-Pfad ausserhalb des Web-/Publishbereichs anlegen.
4. Admin-Hash/Salt setzen, kein Klartextpasswort.
5. HTTPS und Reverse-Proxy/IIS-Header pruefen.
6. Settings-Seite oeffnen und Produktivdiagnose pruefen.
7. Test-Lizenzanfrage mit Testschluessel in einer Testumgebung erzeugen.
8. Erst danach Produktionsschluessel anbinden.

Der echte Produktions-Private-Key darf nie in Repository, `wwwroot`, Publish-Ordner, Download-Ordner, Backup, PDF oder Logdateien liegen.
