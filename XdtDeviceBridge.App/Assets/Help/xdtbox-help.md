# Was ist XDTBox?

XDTBox verbindet medizinische Untersuchungsgeräte lokal mit dem Arztinformationssystem. Die App arbeitet ohne Cloudpflicht und ohne Windows-Dienst. Sie liest AIS-/GDT-/XDT-Patientendateien, wartet auf passende Gerätedaten und erzeugt die konfigurierte Ergebnisdatei für das AIS.

Die Verarbeitung findet nur auf dem lokalen System und in den konfigurierten Ordnern statt. XDTBox startet keine Verarbeitung, wenn die App geschlossen ist.

# Grundprinzip AIS -> XDTBox -> Gerät/AIS

Das AIS legt eine Patientendatei im Ordner "AIS-Patienten Datei an XDTBox" ab. XDTBox überwacht diesen Ordner und kombiniert die Patientendaten mit Messdaten des gewählten Geräteprofils.

Bei LAN-/UNC-Geräten schreibt das Gerät seine Datei in "Gerätedatei an XDTBox". Bei vorbereiteten RS232-Geräten kommen Rohdaten über einen COM-Port. Das Ergebnis wird nach "Ergebnisdatei an AIS" geschrieben. Die Untersuchungsart 8402 kommt aus AIS und wird nicht künstlich von XDTBox erfunden.

# LAN/UNC-Dateiworkflow

NetworkLan beschreibt den bisherigen Datei-/UNC-Workflow. Das Gerät oder die Gerätesoftware schreibt eine Messdatei in einen Netzwerk- oder lokalen Eingangsordner. XDTBox prüft die Datei auf Stabilität, verarbeitet passende Dateipaare und erzeugt die XDT-Rückgabe.

Import-, Export-, Archiv- und Fehlerordner werden im Tab "Schnittstellenprofile" gepflegt.

# Serielle RS232-Geräteanbindung

SerialRs232 ist als zusätzliche Gerätequelle vorbereitet. COM-Port, Baudrate, Datenbits, Stoppbits, Parität, Flusskontrolle und bidirektionale Option können im Schnittstellenprofil gepflegt werden.

Der RS232-Testbereich zeigt Rohtext, Hexdump und bei NIDEK RS232 erkannte Frames, Segmente und Messwertkandidaten. Produktive serielle Messwertausgabe muss pro Gerät mit echten Rohdaten praktisch validiert werden.

# Sicherheit und lokale Verarbeitung

XDTBox baut keine Cloudverbindung auf, startet keinen Windows-Dienst, verwendet keinen FileSystemWatcher und legt keinen Windows-Autostart an. Die Überwachung startet beim Öffnen der App und kann im Tab "Verarbeitung" gestoppt oder wieder gestartet werden.

Backups sind Konfigurationssicherungen. Patientendaten, Messdateien, Archivordner-Inhalte, Fehlerordner-Inhalte und erzeugte Ergebnisdateien werden nicht gesichert.

# Automatische Überwachung

Beim Start der geöffneten XDTBox-App startet die Überwachung automatisch für aktive Schnittstellenprofile. Gefundene passende Dateipaare werden automatisch verarbeitet, sobald die Überwachung läuft.

Wenn keine aktiven Profile vorhanden sind oder ein Ordner fehlt, zeigt XDTBox Status- und Fehlermeldungen, ohne die App zu beenden.

# Überwachung starten und stoppen

"Überwachung starten" aktiviert die periodische Prüfung der aktiven Schnittstellenprofile. "Überwachung stoppen" hält die Überwachung für diese Sitzung an. Nach einem manuellen Stopp startet XDTBox nicht automatisch erneut, solange der Anwender nicht wieder auf "Überwachung starten" klickt.

# Statusmeldungen und Gerätefenster

Die Gerätefenster zeigen Gerätebild oder Platzhalter, erwartete Eingänge, Kachelstatus und Statuskugel. Rot bedeutet gestoppt, grün pulsierend bedeutet laufende Überwachung, und der kurze weiß/gelb/weiß-Blitz signalisiert Dateieingang.

Floating-Fenster können angedockt, gepinnt und automatisch zurückgedockt werden. Die fachliche Verarbeitung bleibt davon unabhängig.

# Profile und Templates

BuiltIn-Profile liefern geprüfte Startkonfigurationen. UserDefined-Profile sind benutzerdefinierte Kopien oder Neuanlagen und dürfen umbenannt, gelöscht oder erweitert werden, soweit die UI dies anbietet.

Templatepakete können exportiert und importiert werden. Importierte Profile bleiben inaktiv, bis sie bewusst konfiguriert und später aktiviert werden.

# XDT-Baukasten

Der Tab "XDT-Baukasten" ist eine eigenständige Entwurfs- und Testoberfläche. Er ersetzt noch nicht den alten Tab "Profile & Templates", bereitet aber dessen spätere Ablösung vor.

Im Baukasten wählen Sie AIS-Profil, Geräteprofil und Mapping-/Exportprofil, laden Testdaten und starten eine Vorschau. Diese Vorschau arbeitet nur mit Entwurfsdaten: Es wird keine produktive Datei in den AIS-Exportordner geschrieben, nichts verschoben, nichts archiviert und keine Ordnerüberwachung gestartet.

Die Rohdatenanzeigen zeigen links AIS-Testdaten oder bei RS232-Geräten empfangene COM-Port-Rohdaten und rechts die Gerätetestdatei. "Verarbeitung starten" erzeugt eine Vorschau mit vier Ansichten: "Roh-XDT", "Ansicht im AIS", "Geräteausgabe" und "Diagnose". Die Vorschau zeigt links Baukasten-Zeilennummern. Diese Nummern dienen nur der Orientierung und gehören nicht zur echten XDT- oder XML-Ausgabe.

Die "Ansicht im AIS" zeigt nur die fachlich sichtbaren Karteikartenzeilen. Patientendaten, Untersuchungsart-Feldnummern, technische Feldnummern wie 8402 oder 6228, SourcePaths und Parserdetails erscheinen dort nicht. Diese Informationen bleiben in "Roh-XDT" und "Diagnose" verfügbar.

Exportregeln werden im Baukasten als Arbeitskopie angezeigt. Der Regelbereich unterscheidet "Export an AIS" und bei bidirektionalen Geräten "Export an Gerät". "Export an AIS" beschreibt die spätere Ausgabe an MEDISTAR/AIS. "Export an Gerät" beschreibt nur die Vorschau der Datei, die an ein bidirektionales Gerät gesendet würde, zum Beispiel CVImport.xml für CV-5000 oder RTImport-XML für RT-6100. Diese Geräteausgabe wird im Baukasten nicht produktiv geschrieben. Die Regel-Tabelle zeigt eine laufende Nummer. Wenn Sie eine Regel anklicken, markiert XDTBox die zugehörige Ausgabezeile in der passenden Ansicht. Erzeugt eine Regel aktuell keine Ausgabe, zeigt der Baukasten einen Hinweis.

Regeln können über das Plus ergänzt und über die Mülltonne aus der Arbeitskopie entfernt werden. Ein leerer SourcePath ist für feste Überschriften oder Notizen erlaubt, wenn im Regeltext ein fester Text steht. Platzhalter zeigen rechts den aktuell eingelesenen Beispielwert; Geräteplatzhalter stammen aus der aktuell geladenen Testdatei. Bei bidirektionalen Geräten zeigen Ausgabe-an-Gerät-Platzhalter Patientendaten und verfügbare historische Werte für die Geräteausgabe. Änderungen an unterstützten Geräteausgabe-Regeln aktualisieren die Geräteausgabe-Vorschau sofort. Im Baukasten blockieren Modellabweichungen die Vorschau nicht, solange die Datei lesbar ist und Parser/Mapping verwertbare Daten liefern. XDTBox zeigt dann eine Warnung und schreibt die Abweichung in die Diagnose, damit neue Geräte, ähnliche Modelle und Exportprofile bewusst verglichen werden können. Die produktive Verarbeitung prüft strenger. Ein Klick fügt den Platzhalter in den Entwurf ein und aktualisiert die Vorschau. Der Zurück-Pfeil nimmt die letzten Baukasten-Änderungen schrittweise zurück. BuiltIn-Profile werden dadurch nicht direkt überschrieben.

"Templatepaket laden" ist als lokale Paketbibliothek vorbereitet und zeigt in V1 eine sichtbare Hinweismeldung. Bis diese Bibliothek vollständig geführt wird, verwenden Sie "Template Paket importieren" oder wählen AIS, Gerät und Exportprofil manuell.

# Neues Gerät anlegen und Gerät laden

"Neues Gerät anlegen" erstellt ein UserDefined-Geräteprofil. "Gerät laden" zeigt bestehende Geräteprofile und erlaubt, ein Gerätebild zu setzen oder zurückzusetzen.

BuiltIn-Geräte werden fachlich nicht überschrieben. Bildänderungen für BuiltIns laufen über lokale Overrides.

# RS232-Testfunktion und NIDEK-Auswertung

Im Tab "Profile & Templates" kann ein COM-Port zeitlich begrenzt abgehört werden. Ohne Gerät oder bei falschen Parametern zeigt XDTBox verständliche Statusmeldungen.

Die NIDEK-RS232-Auswertung erkennt SOH/STX/ETB/EOT-Frames, optionale Checksummen, Header und erste LM-/NT-/PM-Kandidaten. Unbekannte Segmente bleiben roh erhalten und werden nicht als Messwerte exportiert.

# Schnittstellenprofile

Ein Schnittstellenprofil verbindet AIS-Profil, Geräteprofil und Exportprofil. Es enthält außerdem Ordner, RS232-Parameter, XDT-Anhang-Einstellungen, CV-5000-Ausgabeparameter und Laufzeitwerte.

Ein neues Schnittstellenprofil wird als UserDefined und inaktiv angelegt. Konfiguration und produktive Aktivierung sind getrennte Schritte.

# Ordner Default und Ordner anlegen

"Ordner Default" trägt Standardpfade unter C:\XDTBox\<Gerätename> in die sichtbaren Pfadfelder ein. Dabei werden noch keine Ordner erstellt.

"Ordner anlegen" erstellt nur die aktuell eingetragenen Ordner. Dateien werden nicht gelöscht, verschoben oder bereinigt.

# XDT-Anhänge für AIS

XDT-Anhang Import und XDT-Anhang Export steuern die Übergabe externer Dokumentdateien an das AIS. Je Datei können Linkfelder 6302 bis 6305 erzeugt werden, wenn das jeweilige Profil diese Funktion nutzt.

Dokumentanhang-Workflows verändern keine Messwertparser und erzeugen keine künstlichen Messwerte.

# Lizenzstatus verstehen

Die V1-Lizenz begrenzt ausschließlich die Anzahl aktivierter Geräteanbindungen. LAN/UNC, SerialRs232, Dokumentanhang, Profile/Templates, RS232-Testbereich und Parseranalyse sind keine separaten Lizenzmodule.

Die endgültige produktive Lizenzblockade ist in dieser Version nicht hart aktiviert. Der Lizenz-Tab zeigt Status, Warnungen, Geräteanzahl und Karenzzeiten transparent an.

# Lizenzanforderung exportieren und Lizenz importieren

Tragen Sie im Tab "Lizenz" die Kundendaten ein und exportieren Sie die Lizenzanforderung. Gerätenamen in der Anfrage dienen nur der Dokumentation beim Hersteller; lizenzpflichtig ist ausschließlich die Anzahl aktiver Geräteanbindungen.

Eine signierte .xdtboxlic-Lizenz wird über "Lizenz importieren" eingelesen und lokal gespeichert.

# Hardwaretausch und 7 Tage Karenzzeit

Bei Hardwaretausch bitte neue Lizenz anfordern. Karenzzeit 7 Tage ab Umzug der Hardware.

InstallationId bleibt führend. Eine auf anderer Hardware wiederhergestellte Lizenz kann ungültig sein; erzeugen Sie nach dem Umzug eine neue Lizenzanforderung.

# Sicherung erstellen

Der Tab "Sicherung/Umzug" erstellt eine .xdtboxbackup-Datei. Gesichert werden Konfigurationen, UserDefined-Profile, lokale Gerätebilder, Bild-Overrides, Kundendaten und optional die importierte Lizenzdatei.

Es werden keine Patientendaten oder Messdateien gesichert. Import-, Export-, Archiv- und Fehlerordner-Inhalte bleiben außerhalb der Sicherung.

# Sicherung wiederherstellen

Eine Wiederherstellung ersetzt lokale XDTBox-Konfigurationen. Stoppen Sie vorher die Überwachung. Das Backup wird anhand von Manifest, ProductCode und Formatversion geprüft.

Nach der Wiederherstellung werden Profile und Lizenzstatus neu geladen. Auf neuer Hardware kann eine neue Lizenzanforderung nötig sein.

# NIDEK ARK1S

NIDEK ARK1S ist ein validierter Autorefraktor-Workflow für MEDISTAR. XDTBox nutzt die AIS-Patientendatei und die passende Geräte-XML-Datei und erzeugt MEDISTAR-kompatible Ergebniszeilen.

# NIDEK AR360

NIDEK AR360 / AR-360A ist als Autorefraktor-Kandidat mit eigenem Profil vorbereitet und testseitig abgesichert.

# NIDEK LM7/LM7P

NIDEK LM7 / LM-7P ist als Lensmeter-Workflow vorbereitet. Werte werden nur aus echten XML- oder validierten RS232-Rohdaten übernommen.

# NIDEK NT530P

NIDEK NT530P / NT-530P ist als Tonometrie-/Pachymetrie-Kandidat vorbereitet. Fehlerhafte oder unvollständige Daten erzeugen keine künstlichen Messwerte.

# TOPCON CL-300

TOPCON CL-300 ist als Lensmeter-Kandidat vorbereitet. Lensmeterwerte werden als konfigurierte XDT-Ergebniszeilen ausgegeben.

# TOPCON KR800S

TOPCON KR800S ist als Mehruntersuchungsgerät vorbereitet. Autorefraktion, Keratometrie und weitere Kandidaten werden nur aus vorhandenen Gerätedaten gebildet.

# TOPCON TRK2P

TOPCON TRK-2P ist als Mehruntersuchungsgerät vorbereitet. Tonometrie, Pachymetrie, Autorefraktion und Keratometrie hängen von den gelieferten Gerätedaten ab.

# TOPCON CT-1P

TOPCON CT-1P ist als Tonometrie-/Pachymetrie-Kandidat vorbereitet. Unvollständige Teilblöcke werden defensiv behandelt.

# TOPCON CV-5000/CV-5000S

TOPCON CV-5000 / CV-5000S ist als bidirektionaler Phoropter-Kandidat vorbereitet. Ausgabe an das Gerät und Rückgabe vom Gerät sind fachlich getrennt. Es werden keine 6330-Zeilen künstlich erzeugt.

# TOPCON Solos

TOPCON Solos ist als Lensmeter-Kandidat vorbereitet. PDF-Berichte und Transmission bleiben abhängig von echten gefüllten Beispieldaten.

# TOPCON CT-800A

TOPCON CT-800A ist als Non-Contact-Tonometer-Kandidat vorbereitet. Korrigierte IOP-/CCT-Details werden nur bei verwertbaren Daten ausgegeben.

# TOPCON KR-1

TOPCON KR-1 ist als Keratorefraktometer-Kandidat vorbereitet. KM/KRT-Ausgabe bleibt von echten verwertbaren Daten abhängig.

# Dokumentanhang

Der Dokumentanhang-Workflow übergibt Dokumentdateien als externe AIS-Anhänge. Er verarbeitet keine medizinischen Messwerte.

# Manuelle Dokumentübergabe

Die manuelle Dokumentübergabe öffnet ein Übertragungsfenster nach AIS-Dateieingang. Dateien können manuell ausgewählt oder per Drag-and-Drop ergänzt werden.

# RS232 NIDEK allgemein

Die NIDEK-RS232-Familie nutzt ASCII-Frames mit Steuerzeichen wie SOH, STX, ETB und EOT. Der Testbereich zeigt Frames und Kandidaten an; produktiver Export erfolgt erst nach Gerätevalidierung.

# Fehlerbehebung

Keine AIS-Datei gefunden: Prüfen Sie den Ordner "AIS-Patienten Datei an XDTBox" und ob das AIS eine Datei schreibt.

Gerätedatei fehlt: Prüfen Sie "Gerätedatei an XDTBox" oder bei RS232 den COM-Port.

Datei nicht stabil: Warten Sie, bis das Gerät die Datei vollständig geschrieben hat.

AIS-Patientendaten fehlen: Prüfen Sie die AIS-Datei und die Patientendatenfelder.

Parserfehler: Prüfen Sie Gerätetyp, Dateiformat und ob das Profil zum Gerät passt.

Exportordner nicht erreichbar: Prüfen Sie Pfad, Netzwerkfreigabe und Berechtigungen.

Lizenz ungültig oder für andere Installation: Importieren Sie die passende .xdtboxlic oder erzeugen Sie eine neue Lizenzanforderung.

COM-Port nicht gefunden oder belegt: Prüfen Sie Gerätemanager, Kabel, Adapter und andere Programme.

RS232 keine Daten empfangen: Prüfen Sie Baudrate, Datenbits, Stoppbits, Parität, Flusskontrolle und ob das Gerät Daten sendet.

MEDISTAR zeigt Werte nicht an: Prüfen Sie Exportordner, Rückgabedatei, 8402 aus AIS und die importierten XDT-Ergebniszeilen.
