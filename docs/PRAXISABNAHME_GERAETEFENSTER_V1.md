# Praxisabnahme Gerätefenster V1

## Zweck

Die Geräteanbindungsfenster dienen der Praxisüberwachung aktiver Schnittstellenprofile im Tab `Verarbeitung`.

Die App kann im Hintergrund laufen, während relevante Geräteaktivität sichtbar und akustisch gemeldet wird. Pro Schnittstellenprofil arbeitet die Anzeige unabhängig, zum Beispiel für MEDISTAR + NIDEK ARK1S und MEDISTAR + NIDEK AR360.

## Status

Status: V1 praxisnah abgenommen.

Technischer Referenzstand:

- `dotnet build XdtDeviceBridge.sln` erfolgreich
- `dotnet test XdtDeviceBridge.sln` erfolgreich
- 1034 Tests bestanden
- 0 fehlgeschlagen
- 0 übersprungen

## Abgenommene Funktionen

### Systray-Grundfunktion

- Die App läuft im Infobereich weiter.
- Das Hauptfenster kann minimiert oder ausgeblendet werden.
- Öffnen über das Tray-Icon funktioniert.
- Beenden über das Tray-Menü funktioniert bewusst und schließt die App wirklich.

### Abdockbare Gerätefenster

- Jede Monitoring-Karte kann manuell abgedockt werden.
- Jede Karte kann wieder angedockt werden.
- Schließen per `X` dockt sicher zurück.
- ARK1S und AR360 verhalten sich unabhängig voneinander.

### Fensterposition und Pin

- `📌` Position merken funktioniert.
- Fensterposition und Größe werden über App-Neustart wiederhergestellt.
- `🔝` Pin / TopMost funktioniert pro Floating-Fenster getrennt.
- Ein Fenster kann TopMost sein, während ein anderes nicht TopMost ist.

### Automatisches Abdocken

- Relevante Monitoring-Aktivität öffnet genau das betroffene Gerätefenster.
- Andere Gerätefenster bleiben unverändert.

### Automatisches Zurückandocken

- Nach terminalem Abschluss startet die Restlaufzeit.
- Nicht gepinnte Fenster docken nach ca. 5 Sekunden zurück.
- Gepinnte Fenster bleiben offen.

### Signalton

- Der WAV-Signalton wird nur bei Gerätedatei-Eingang abgespielt.
- AIS-Dateien lösen keinen Ton aus.
- Export-, Status- und UI-Meldungen lösen keinen Ton aus.
- Der Ton kommt auch bei späteren Durchläufen erneut korrekt.

### Reset

- Der Reset-Button `↺` funktioniert in angedockten Karten und Floating-Fenstern.
- Vor dem Reset erscheint eine Sicherheitsabfrage.
- Reset verwirft den aktuellen Vorgang des gewählten Schnittstellenprofils.
- Reset leert die Eingangsordner dieses Profils:
  - AIS-Importordner
  - Geräte-Importordner
  - XDT-Anhang-Importordner, falls konfiguriert
- Export-, Archiv- und Fehlerordner werden nicht geleert.
- Unterordner werden nicht rekursiv gelöscht.
- Der Monitoring-Zustand wird sauber zurückgesetzt.

### Buttonlayout

- Die Symbolbuttons stehen in einer Reihe: `🗗 ↺ 📌 🔝`.
- Die transparente Buttonoptik ist abgenommen.

## Live-Test-Ergebnis

- Manuelles Abdocken und Andocken funktionieren.
- Automatisches Abdocken funktioniert.
- Automatisches Zurückandocken funktioniert.
- Der Ton kommt korrekt nur bei Gerätedatei.
- Reset funktioniert wie geplant.
- Die Symbolbuttons passen optisch in eine Zeile.
- ARK1S und AR360 bleiben getrennt.

## Sicherheitsgrenzen

- Keine Änderung an fachlicher Verarbeitung.
- Kein FileSystemWatcher.
- Kein Windows-Dienst.
- Kein Autostart.
- Keine Aktivierungslogik.
- Keine Profiländerung.
- Reset wirkt nur auf das ausgewählte Schnittstellenprofil.
- Reset leert nur Eingangsordner des gewählten Profils.
- Keine Exportordner.
- Keine Archivordner.
- Keine Fehlerordner.
- Keine rekursive Löschung.

## Offene spätere Themen

- Einstellbare Rückdock-Zeit über UI.
- Sichtbarer Countdown-Hinweis im Floating-Fenster.
- Optional eigener App-Icon für Systray.
- Optional Benutzeroption für Ton an/aus.
- Weiteres Geräteprofil LM7/LM7P nach geeigneten Beispieldaten.
