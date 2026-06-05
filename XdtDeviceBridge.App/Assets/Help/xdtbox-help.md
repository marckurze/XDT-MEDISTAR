# Was ist XDTBox?

XDTBox verbindet medizinische Untersuchungsgeräte lokal mit dem Arztinformationssystem. Die App arbeitet ohne Cloudpflicht und ohne Windows-Dienst. Sie liest AIS-/GDT-/XDT-Patientendateien, wartet auf passende Gerätedaten und erzeugt die konfigurierte Ergebnisdatei für das AIS.

Die Verarbeitung findet nur auf dem lokalen System und in den konfigurierten Ordnern statt. XDTBox startet keine Verarbeitung, wenn die App geschlossen ist.

# Bereiche der XDTBox

Verarbeitung: Produktiver Betrieb. Hier sehen Sie aktive Schnittstellenprofile, Monitoringstatus, eingehende AIS-/Gerätedaten und den Verarbeitungszustand.

XDT-Baukasten: Arbeits- und Testbereich für neue oder bestehende Geräteanbindungen. Hier werden AIS-, Geräte- und Exportprofile kombiniert, Testdaten geladen, Mappingregeln geprüft und Vorschauen für AIS- und Geräteausgabe erstellt.

Profilverwaltung: Zentrale Verwaltung von AIS-, Geräte-, Export- und Schnittstellenprofilen sowie Templates. Hier können Profile gesucht, angezeigt, dupliziert, umbenannt, geschützt gelöscht und gewartet werden.

Schnittstellenprofile: Konkrete Praxisanbindung. Hier werden Ordner, COM-Port, DTR/RTS, RS232-Diagnose, Ausgabeordner, Aktivierungsprüfung und profilbezogene Betriebsparameter eingestellt.

Sicherung/Umzug: Sichern und Wiederherstellen der XDTBox-Konfiguration. Wichtig vor Updates, Hardwarewechsel oder Neuinstallation.

Lizenz: Lizenzstatus, Lizenzanforderung, Lizenzimport, lizenzierte Geräteanzahl und Hinweise zum Hardwaretausch.

# Grundprinzip AIS -> XDTBox -> Gerät/AIS

Das AIS legt eine Patientendatei im Ordner "AIS-Patienten Datei an XDTBox" ab. XDTBox überwacht diesen Ordner und kombiniert die Patientendaten mit Messdaten des gewählten Geräteprofils.

Bei LAN-/UNC-Geräten schreibt das Gerät seine Datei in "Gerätedatei an XDTBox". Bei vorbereiteten RS232-Geräten kommen Rohdaten über einen COM-Port. Das Ergebnis wird nach "Ergebnisdatei an AIS" geschrieben. Die Untersuchungsart 8402 kommt aus AIS und wird nicht künstlich von XDTBox erfunden.

# LAN/UNC-Dateiworkflow

NetworkLan beschreibt den bisherigen Datei-/UNC-Workflow. Das Gerät oder die Gerätesoftware schreibt eine Messdatei in einen Netzwerk- oder lokalen Eingangsordner. XDTBox prüft die Datei auf Stabilität, verarbeitet passende Dateipaare und erzeugt die XDT-Rückgabe.

Import-, Export-, Archiv- und Fehlerordner werden im Tab "Schnittstellenprofile" gepflegt.

# Serielle RS232-Geräteanbindung

SerialRs232 ist als zusätzliche Gerätequelle vorbereitet. COM-Port, Baudrate, Datenbits, Stoppbits, Parität, Flusskontrolle, DTR, RTS und bidirektionale Option können im Schnittstellenprofil gepflegt werden.

Der RS232-Testbereich zeigt Rohtext, Hexdump und bei NIDEK RS232 erkannte Frames, Segmente und Messwertkandidaten. Produktive serielle Messwertausgabe muss pro Gerät mit echten Rohdaten praktisch validiert werden.

# Sicherheit und lokale Verarbeitung

XDTBox baut keine Cloudverbindung auf, startet keinen Windows-Dienst, verwendet keinen FileSystemWatcher und legt keinen Windows-Autostart an. Die Überwachung startet beim Öffnen der App und kann im Tab "Verarbeitung" gestoppt oder wieder gestartet werden.

Backups sind Konfigurationssicherungen. Patientendaten, Messdateien, Archivordner-Inhalte, Fehlerordner-Inhalte und erzeugte Ergebnisdateien werden nicht gesichert.

# Installation, Update und Deinstallation

XDTBox 1.0 wird als klassisches Windows-Setup installiert. Der vorgeschlagene Installationsordner ist C:\XDTBox und kann bei Neuinstallation geändert werden.

Das Setup unterscheidet Neuinstallation und Update. Bei Updates werden App-Dateien ersetzt, Kundendaten bleiben erhalten. Dazu gehören Profile, Schnittstellenprofile, COM-Port-Parameter, Lizenzen, Baukasten-Templates, Gerätebild-Overrides und lokale UI-Einstellungen.

Der Deinstaller entfernt standardmäßig nur die App. Lokale XDTBox-Kundendaten werden nur nach separater ausdrücklicher Bestätigung gelöscht. Externe Praxisordner wie AIS-, Geräte-, Archiv-, Export- oder Fehlerordner werden nicht durch den Installer oder Deinstaller bereinigt.

Vor Updates empfiehlt sich eine Sicherung im Tab "Sicherung/Umzug".

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

Aus geprüften Referenzdaten sind zusätzliche BuiltIn-Profile vorbereitet, zum Beispiel NIDEK ARK-510A, NIDEK ARK-560A, NIDEK AR-1, NIDEK AR-1S, NIDEK AR-310A, NIDEK LM-1800P, NIDEK LM-1800PD, NIDEK NT-1, NIDEK NT-1E, NIDEK NT-1P, NIDEK NT-510, NIDEK NT-530, TOPCON CL-300PDL, TOPCON RM-800, TOPCON TRK-3 Omnia, Huvitz HRK-8000A, HRK-9000A, HNT-1P und HTR-1A, TOMEY CF-2000, TL-2000C, TL-6000, TL-7000, MR-6000, TOP-1000, EM-3000 und EM-4000, Shin-Nippon Accuref R-800, Accuref K-900, DL-1000, DL-800, DL-900, NCT-200 und SLM-4000 sowie Reichert 7CR NCT, Reichert LensChek Plus und Rodenstock CX 800. Weitere Geräte können aus vorhandener Parser- oder COM-Logik abgeleitet werden, werden aber erst als BuiltIn ausgeliefert, wenn Parser, Mapping und Tests ausreichend abgesichert sind. Rodenstock Phoromat 2000 und Möller-Wedel Visutron bleiben Kandidaten.

# Profilverwaltung

Der Tab "Profilverwaltung" bündelt die reine Profilpflege. Dort finden Sie AIS-, Geräte-, Export- und Schnittstellenprofile sowie lokale Templatepakete und Baukasten-Templates in einer gemeinsamen Übersicht mit Suche, BuiltIn/UserDefined-Filter und Profiltypfilter.

BuiltIn-Profile sind geschützt. Sie können nicht direkt umbenannt oder gelöscht werden, lassen sich aber als UserDefined-Kopie duplizieren. UserDefined-Profile können umbenannt, dupliziert und gelöscht werden, solange sie nicht noch von Export- oder Schnittstellenprofilen verwendet werden. Aktive Schnittstellenprofile schützen zusätzlich vor blindem Löschen.

Die Rollen sind getrennt: Der XDT-Baukasten ist Entwurf, Test und Vorschau. Die Profilverwaltung ist Übersicht, Umbenennen, Duplizieren, Löschen, Template-Aufräumen und BuiltIn-Reparatur. Der Tab "Schnittstellenprofile" bleibt für Ordner, COM-Port, DTR/RTS, NIDEK-RT-Sendemodus und konkrete Praxisparameter zuständig.

# XDT-Baukasten

Der Tab "XDT-Baukasten" ist eine eigenständige Entwurfs- und Testoberfläche. Er arbeitet mit eigenen Auswahl-, Import-, Template- und RS232-Diagnosewegen.

Im Baukasten wählen Sie AIS-Profil, Geräteprofil und Mapping-/Exportprofil, laden Testdaten und starten eine Vorschau. Diese Vorschau arbeitet nur mit Entwurfsdaten: Es wird keine produktive Datei in den AIS-Exportordner geschrieben, nichts verschoben, nichts archiviert und keine Ordnerüberwachung gestartet.

Die Rohdatenanzeigen zeigen links AIS-Testdaten oder bei RS232-Geräten empfangene COM-Port-Rohdaten und rechts die Gerätetestdatei. "Verarbeitung starten" erzeugt eine Vorschau mit vier Ansichten: "Roh-XDT", "Ansicht im AIS", "Geräteausgabe" und "Diagnose". Die Vorschau zeigt links Baukasten-Zeilennummern. Diese Nummern dienen nur der Orientierung und gehören nicht zur echten XDT- oder XML-Ausgabe.

Die "Ansicht im AIS" zeigt nur die fachlich sichtbaren Karteikartenzeilen. Patientendaten, Untersuchungsart-Feldnummern, technische Feldnummern wie 8402 oder 6228, SourcePaths und Parserdetails erscheinen dort nicht. Diese Informationen bleiben in "Roh-XDT" und "Diagnose" verfügbar.

Exportregeln werden im Baukasten als Arbeitskopie angezeigt. Der Regelbereich unterscheidet "Export an AIS" und bei bidirektionalen Geräten "Export an Gerät". "Export an AIS" beschreibt die spätere Ausgabe an MEDISTAR/AIS. "Export an Gerät" beschreibt nur die Vorschau der Datei, die an ein bidirektionales Gerät gesendet würde, zum Beispiel CVImport.xml für CV-5000 oder RTImport-XML für RT-6100. Diese Geräteausgabe wird im Baukasten nicht produktiv geschrieben. Die Regel-Tabelle zeigt eine laufende Nummer. Wenn Sie eine Regel anklicken, markiert XDTBox die zugehörige Ausgabezeile in der passenden Ansicht. Erzeugt eine Regel aktuell keine Ausgabe, zeigt der Baukasten einen Hinweis.

Bei NIDEK RT-6100 unterscheidet der Baukasten zwischen Eingabequellen für die Geräteausgabe und Rückgabedateien an das AIS. LM-/ARK-XML-Dateien aus NIDEK-Geräten können als Quellen für die RT-6100-Geräteausgabe dienen und erzeugen in der Vorschau `LM_Base` beziehungsweise `REF_Base`. RT-6100-Rückgabe-XML mit `NIDEK_RT` wird dagegen für "Export an AIS" ausgewertet; `Best` geht nach `6228`, `Full` nach `6227`. Auch hier bleibt der Baukasten reine Vorschau und schreibt keine produktive Geräteausgabedatei.

Regeln können über das Plus ergänzt und über die Mülltonne aus der Arbeitskopie entfernt werden. Ein leerer SourcePath ist für feste Überschriften oder Notizen erlaubt, wenn im Regeltext ein fester Text steht. Platzhalter zeigen rechts den aktuell eingelesenen Beispielwert; Geräteplatzhalter stammen aus der aktuell geladenen Testdatei. Bei bidirektionalen Geräten zeigen Ausgabe-an-Gerät-Platzhalter Patientendaten und verfügbare historische Werte für die Geräteausgabe. Änderungen an unterstützten Geräteausgabe-Regeln aktualisieren die Geräteausgabe-Vorschau sofort. Im Baukasten blockieren Modellabweichungen die Vorschau nicht, solange die Datei lesbar ist und Parser/Mapping verwertbare Daten liefern. XDTBox zeigt dann eine Warnung und schreibt die Abweichung in die Diagnose, damit neue Geräte, ähnliche Modelle und Exportprofile bewusst verglichen werden können. Die produktive Verarbeitung prüft strenger. Ein Klick fügt den Platzhalter in den Entwurf ein und aktualisiert die Vorschau. Der Zurück-Pfeil nimmt die letzten Baukasten-Änderungen schrittweise zurück. BuiltIn-Profile werden dadurch nicht direkt überschrieben.

"Baukasten-Template laden" lädt lokal gespeicherte Baukasten-Arbeitskopien aus der Templatebibliothek. "Konfiguration als Template speichern" speichert die aktuelle Baukasten-Arbeitskopie als UserDefined-Exportprofil und zusätzlich als lokale Baukasten-Template-Datei. "Template Paket importieren" öffnet im Baukasten eine eigene Importvorschau; der Import wird dort als UserDefined-Kopie übernommen.

Die zentrale Profilpflege liegt im Tab "Profilverwaltung". Dort können Profile gesucht, dupliziert, umbenannt, gelöscht, BuiltIns repariert, Baukasten-Templates und Templatepakete verwaltet sowie neue AIS-, Geräte-, Export- und Schnittstellenprofile angelegt werden. Der Baukasten ist nicht von einer früheren Profil-/Template-Oberfläche abhängig.

# Neues Gerät anlegen und Gerät laden

"Neues Gerät anlegen" erstellt ein UserDefined-Geräteprofil. "Gerät laden" zeigt bestehende Geräteprofile und erlaubt, ein Gerätebild zu setzen oder zurückzusetzen.

BuiltIn-Geräte werden fachlich nicht überschrieben. Bildänderungen für BuiltIns laufen über lokale Overrides.

# RS232-Testfunktion und NIDEK-Auswertung

Im Baukasten kann ein COM-Port zeitlich begrenzt abgehört werden; DTR/RTS, Parität, Stopbits, Flusskontrolle und Zeilenende werden direkt im Baukasten-Fenster gesetzt. Zusätzlich kann ein kleines ASCII-Testkommando gesendet werden. Ohne Gerät oder bei falschen Parametern zeigt XDTBox verständliche Statusmeldungen.

Im Tab "Schnittstellenprofile" öffnet "RS232-Diagnose öffnen" dieselbe allgemeine Diagnose mit den aktuell sichtbaren COM-Parametern des gewählten seriellen Schnittstellenprofils. Diese Diagnose startet keine produktive Verarbeitung und übernimmt keine Daten in den Baukasten.

Serielle Diagnose läuft über den Baukasten-Mitschnitt oder über die RS232-Diagnose im Tab "Schnittstellenprofile".

Die NIDEK-RS232-Auswertung erkennt SOH/STX/ETB/EOT-Frames, optionale Checksummen, Header und erste LM-/NT-/PM-Kandidaten. Unbekannte Segmente bleiben roh erhalten und werden nicht als Messwerte exportiert.

# Schnittstellenprofile

Ein Schnittstellenprofil verbindet AIS-Profil, Geräteprofil und Exportprofil. Es enthält außerdem Ordner, RS232-Parameter, XDT-Anhang-Einstellungen, CV-5000-Ausgabeparameter und Laufzeitwerte.

Ein neues Schnittstellenprofil wird als UserDefined und inaktiv angelegt. Konfiguration und produktive Aktivierung sind getrennte Schritte.

Aktive Schnittstellenprofile zaehlen immer als Geraeteanbindung. Der fruehere Lizenzpflicht-Haken wird in der App nicht mehr angezeigt; alte Profildaten koennen dadurch keine aktive Anbindung lizenzfrei schalten.

Die Auswahl im Tab "Schnittstellenprofile" kann nach Geraetehersteller und AIS-System gefiltert werden. Der Button "AIS Ausgabe Info" zeigt ausschliesslich AIS-Ausgabefelder mit Feldkennung, Bedeutung, AIS-Relevanz, Karteikarten-Sichtbarkeit und Hinweis. Geraeteausgabe, echte Messwerte, Rohdaten und Platzhalter werden dort bewusst nicht angezeigt.

Die Untersuchungsart `8402` wird aus der AIS-Datei uebernommen und unveraendert wieder ausgegeben. Empfohlene Defaultwerte fuer neue Profile sind zum Beispiel `LENS`, `PHORO`, `AUTO`, `KERA`, `KOMB`, `TONO`, `PACHY`, `ENDO`, `OCT`, `DOKU` und `MESS`; eine eingehende Untersuchungsart wird dadurch nicht blockiert.

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

# Installation und Updates

Updates duerfen App-Dateien, Hilfe, Themes und BuiltIn-Definitionen ersetzen. Ihre lokalen Kundendaten bleiben erhalten: Profile, Schnittstellenprofile, Baukasten-Templates, lokale Gerätebilder, Bild-Overrides, Lizenzdateien, UI-Einstellungen und Backups.

Bei einer spaeteren Deinstallation bleiben Kundendaten standardmaessig erhalten. Loeschen Sie Kundendaten nur nach bewusster Bestaetigung und vorheriger Sicherung. Externe Praxisordner wie AIS-Import, Geraete-Import, Export, Archiv und Fehlerordner werden von XDTBox nicht blind geloescht.

# NIDEK ARK1S

NIDEK ARK1S ist ein validierter Autorefraktor-Workflow für MEDISTAR. XDTBox nutzt die AIS-Patientendatei und die passende Geräte-XML-Datei und erzeugt MEDISTAR-kompatible Ergebniszeilen.

# NIDEK AR360

NIDEK AR360 / AR-360A ist als Autorefraktor-Kandidat mit eigenem Profil vorbereitet und testseitig abgesichert.

# NIDEK AR-1 / AR-1S / AR-310A

NIDEK AR-1, AR-1S und AR-310A nutzen die NIDEK-XML-Autorefraktor-Familie. AR-Medianwerte werden als `6228` ausgegeben; bei AR-1S kann eine dokumentierte subjektive SR-Quelle als `6227` vorbereitet werden. Es werden keine `6330`-Zeilen und keine kuenstlichen Trennzeilen erzeugt.

# NIDEK LM7/LM7P

NIDEK LM7 / LM-7P ist als Lensmeter-Workflow vorbereitet. Werte werden nur aus echten XML- oder validierten RS232-Rohdaten übernommen.

# NIDEK LM-1800PD

NIDEK LM-1800PD ist als Variante der bestehenden NIDEK-Lensmeter-XML-Familie vorbereitet. Die Ausgabe nutzt Lensmeter-Zeilen über `6228` und bleibt bei den aus der Datei ermittelten Werten.

# NIDEK NT530P

NIDEK NT530P / NT-530P ist als Tonometrie-/Pachymetrie-Kandidat vorbereitet. Fehlerhafte oder unvollständige Daten erzeugen keine künstlichen Messwerte.

# NIDEK NT-1 / NT-1E / NT-1P / NT-510 / NT-530

NIDEK NT-1, NT-1E, NT-1P, NT-510 und NT-530 nutzen die NT530P-XML-Familie. Tonometrie wird über `6205`, Pachymetrie über `6220` ausgegeben; Autorefraktor- oder Phoropter-Zeilen werden daraus nicht künstlich erzeugt.

# TOPCON CL-300

TOPCON CL-300 ist als Lensmeter-Kandidat vorbereitet. Lensmeterwerte werden als konfigurierte XDT-Ergebniszeilen ausgegeben.

# TOPCON CL-300PDL

TOPCON CL-300PDL nutzt die CL-300-Lensmeterfamilie. Die Ausgabe bleibt bei Lensmeter-Zeilen und erzeugt keine zusätzlichen Messarten.

# TOPCON RM-800

TOPCON RM-800 ist als Autorefraktor-Variante vorbereitet. XDTBox gibt erkannte REF-Werte aus und erfindet keine Keratometerwerte, wenn sie nicht in der Gerätedatei stehen.

# TOPCON TRK-3 Omnia

TOPCON TRK-3 Omnia nutzt die TRK-2P-Familie für Autorefraktion, Keratometrie, Tonometrie und Pachymetrie, sofern diese Daten in der Gerätedatei vorhanden sind.

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

# NIDEK RT-2100/RT-3100/RT-5100 RS232

NIDEK RT-2100, RT-3100 und RT-5100 sind als serielle bidirektionale Phoropterfamilie vorbereitet. Fuer RT-2100 ist 2400 7E2 der konservative Standard. RT-3100 und RT-5100 koennen Type 1 mit 2400 7E2 oder Type 2 mit 9600 8O1 verwenden.

Die RT-Presets setzen DTR und RTS standardmaessig aktiv. Beim RT-3100 wurde im Praxisaufbau bestaetigt, dass mit DTR aus keine Rueckgabe empfangen wurde, mit DTR aktiv aber ein vollstaendiger Frame ankam. Der gleiche Praxisaufbau hat den direkten PC->RT-Writer-Frame empfangen; der RS/SD-Handshake lieferte dort keine SD-Bestaetigung.

Im Tab "Schnittstellenprofile" gibt es fuer diese seriellen RT-Profile die Auswahl "NIDEK-RT Sendemodus":

- "Direkt Writer-Frame senden": XDTBox sendet den Writer-Frame direkt, erwartet keine SD-Bestaetigung und wartet danach auf die spaetere Rueckgabe. Das ist der aktuelle Praxisdefault fuer RT-2100/RT-3100/RT-5100.
- "RS/SD-Handshake": XDTBox sendet zuerst RS, erwartet SD und sendet den Writer-Frame nur bei Bestaetigung.
- "RS senden, dann Writer ohne SD": XDTBox sendet RS, wartet kurz und sendet den Writer-Frame auch ohne SD.

Zusaetzlich gibt es "NIDEK-RT Sendeinhalt". Damit kann pro Schnittstellenprofil gespeichert werden, ob die RT-Referenz ohne ID (AR/AL), die RT-3100 Praxisvariante, alle ausgewaehlten Werte, nur Autoref, nur Lensmeter, Lensmeter ohne ADD, Varianten ohne ID oder ein minimaler rechter Testframe gesendet werden. Das ist fuer Praxisabnahmen wichtig, wenn ein RT einen kombinierten Vollframe nicht uebernimmt.

Im produktiven Ablauf oeffnet XDTBox das RT-Fenster nicht schon beim Start der Ueberwachung. Erst wenn eine AIS-Patientendatei ankommt, werden Patient und Historie gelesen und ein Auswahlfenster geoeffnet. Dort koennen Lensmeter- und Autorefraktor-Historienwerte ausgewaehlt werden. Senden erfolgt nur nach Klick auf "An RT senden" ueber den im Schnittstellenprofil gepflegten COM-Port. Danach bleibt der Workflow mit Patientenkontext im Wartestatus, bis die Phoropter-Rueckgabe empfangen und bis EOT plus kurzer Stabilitaetswartezeit verarbeitet wurde.

Das RT-Fenster enthaelt fuer die Praxisabnahme eine einklappbare serielle Diagnose. Sie zeigt COM-Port, Baudrate, Datenbits, Paritaet, Stoppbits, Flusskontrolle, DTR/RTS, CTS/DSR/DCD/RI, RS-Anforderung, erwartete und empfangene SD-Bestaetigung, den PC->RT-Writer-Frame, Hexdump, sichtbare Steuerzeichen und den Empfang bis EOT. Die RS-Anforderung wird als `<SOH>C**<STX>RS<ETB><EOT>` beziehungsweise `01 43 2A 2A 02 52 53 17 04` gesendet. LM-SCA-Augenpraefixe werden dagegen als Leerzeichen + `R`/`L` gesendet, nicht als ASCII-Sternchen. Die RT-Referenz ohne ID (AR/AL) sendet ohne ID-Block, LM ADD als `AR`/`AL` und endet mit `EB ET`; die RT-3100 Praxisvariante reproduziert dagegen bewusst den live angenommenen 107-Byte-Frame mit Legacy-ADD `RA`/`LA` und ohne zusaetzliches `EB` direkt vor `EOT`. "COM-Port nur abhoeren" oeffnet denselben Profil-Port, sendet nichts und erzeugt keinen XDT-Export. "Rueckgabe abhoeren und verarbeiten" ist der produktive Rueckweg im Wartestatus: Die Funktion sendet nichts, nutzt aber den gespeicherten AIS-Kontext und erzeugt bei gueltiger Rueckgabe die MEDISTAR-XDT-Datei.

Der Bereich "Sendetest" im RT-Fenster ist nur fuer die Praxisdiagnose: "RS anfordern" sendet nur RS und wartet auf SD, "DTR-Toggle + RS" schaltet DTR kurz aus/ein und fordert danach RS an, "Direkt Writer-Frame senden" sendet den PC->RT-Frame ohne RS/SD und "RS + Writer ohne SD-Warten" sendet nach kurzer Wartezeit auch ohne SD. In diesem Bereich koennen Frame-Variante und optional "CR nach EOT" getestet werden, ohne den gespeicherten Profilwert zu aendern. Empfohlener Testplan bei Nichtuebernahme: zuerst RT-Referenz ohne ID (AR/AL), danach RT-3100 Praxisvariante, Nur Lensmeter ohne ADD, Nur Lensmeter, Nur Autoref, Varianten ohne ID, optional CR nach EOT. Diese Modi laufen nur nach explizitem Klick, erzeugen keinen produktiven XDT-Export und aendern den gespeicherten Sendemodus im Schnittstellenprofil nicht.

Echte Daten vom RT werden aus RS232-Rohdaten geparst; Final-Werte werden fuer MEDISTAR als 6228 und Subjective-Werte als 6227 vorbereitet. Ein Geraete-Eingangsordner und ein dateibasierter Ausgabeordner an das Geraet sind fuer diese seriellen Phoropter nicht erforderlich. Wenn nach dem Senden nicht sofort eine Rueckgabe kommt, bleibt XDTBox im Wartestatus: Fuehren Sie die Untersuchung am Phoropter durch, loesen Sie danach PRINT/SEND aus und starten Sie bei Bedarf "Rueckgabe abhoeren und verarbeiten". Ohne Rueckgabe erzeugt XDTBox kein leeres XDT. MEDISTAR-Import muss am Geraet weiter praktisch geprueft werden.

# Huvitz HRK/HNT/HTR

Huvitz HRK-8000A und HRK-9000A sind als serielle Textprofile fuer Autorefraktion und Keratometrie vorbereitet. REF-Werte werden fuer MEDISTAR nach `6228`, KM-Werte nach `6221` abgebildet.

Huvitz HNT-1P ist als Tonometer/Pachymeter vorbereitet: Tonometrie geht nach `6205`, Pachymetrie nach `6220`. Huvitz HTR-1A kombiniert REF, KM, Tonometrie und Pachymetrie in einem Profil.

Diese Huvitz-Profile beruhen aktuell auf neutral abgeleiteter Referenzlogik und synthetischen Testfixtures. Echte Praxisrohdateien und MEDISTAR-Importabnahme sollten vor produktiver Nutzung gesammelt und geprueft werden.

# TOMEY CF/TL/MR/TOP/EM

TOMEY CF-2000 ist als serielles Lensmeterprofil vorbereitet. TL-2000C, TL-6000 und TL-7000 sind als dateibasierte Lensmeterprofile vorbereitet. Lensmeter-Werte werden fuer MEDISTAR nach `6228` abgebildet.

TOMEY MR-6000 ist als Kombigeraet vorbereitet: REF geht nach `6228`, KM nach `6221`, Tonometrie nach `6205` und Pachymetrie/CCT nach `6220`. TOMEY TOP-1000 ist als Tonometer/Pachymeter vorbereitet und nutzt `6205` und `6220`.

TOMEY EM-3000 und EM-4000 sind als dateibasierte Endothel-/Zellmessgeraete vorbereitet. Messwerte werden nach `6228`, Kommentare nach `6227` und Bild-/Dateiverweise nach `6302` vorbereitet. Bilddateien selbst werden nicht in den Kundeninstaller aufgenommen.

Diese TOMEY-Profile beruhen aktuell auf neutral abgeleiteter Referenzlogik und synthetischen Testfixtures. Echte Praxisrohdateien und MEDISTAR-Importabnahme sollten vor produktiver Nutzung gesammelt und geprueft werden. TAP-2000 wurde technisch geprueft, bleibt ohne echte Rueckgabe- und Live-Sendeframes aber noch kein produktives BuiltIn.

# Fehlerbehebung

Keine AIS-Datei gefunden: Prüfen Sie den Ordner "AIS-Patienten Datei an XDTBox" und ob das AIS eine Datei schreibt.

Gerätedatei fehlt: Prüfen Sie "Gerätedatei an XDTBox" oder bei RS232 den COM-Port.

Datei nicht stabil: Warten Sie, bis das Gerät die Datei vollständig geschrieben hat.

AIS-Patientendaten fehlen: Prüfen Sie die AIS-Datei und die Patientendatenfelder.

Parserfehler: Prüfen Sie Gerätetyp, Dateiformat und ob das Profil zum Gerät passt.

Exportordner nicht erreichbar: Prüfen Sie Pfad, Netzwerkfreigabe und Berechtigungen.

Lizenz ungültig oder für andere Installation: Importieren Sie die passende .xdtboxlic oder erzeugen Sie eine neue Lizenzanforderung.

COM-Port nicht gefunden oder belegt: Prüfen Sie Gerätemanager, Kabel, Adapter und andere Programme.

RS232 keine Daten empfangen: Prüfen Sie Baudrate, Datenbits, Stoppbits, Parität, Flusskontrolle, DTR, RTS und ob das Gerät Daten sendet.

RT-3100 keine SD-Bestaetigung oder keine Rueckgabe: Pruefen Sie den richtigen COM-Port, Type1/Type2, PC-Port-Einstellung am Phoropter, DTR/RTS/Handshake, Portbelegung durch andere Programme und ob am Phoropter PRINT/SEND ausgeloest wurde. Wenn keine Daten ankommen, DTR aktivieren; der letzte erfolgreiche RT-3100-Abhoertest lief mit DTR aktiv. Wenn Empfang funktioniert, aber keine SD-Antwort kommt, pruefen Sie TX-Leitung PC->RT, Kabel/Adapter, RT-Input-Mode und nutzen Sie im RT-Fenster die Sendetests "RS anfordern" oder "Direkt Writer-Frame senden".

MEDISTAR zeigt Werte nicht an: Prüfen Sie Exportordner, Rückgabedatei, 8402 aus AIS und die importierten XDT-Ergebniszeilen.
