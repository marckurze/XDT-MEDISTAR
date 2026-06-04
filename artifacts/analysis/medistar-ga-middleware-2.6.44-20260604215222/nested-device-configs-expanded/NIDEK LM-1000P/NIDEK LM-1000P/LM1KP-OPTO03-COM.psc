const
	VERSION = '1.0.21.78';
	DATE = '29.04.2026 13:22:21';
	TEXT = 'Copyright (c) 2026 CompuGroup Medical Deutschland AG';

	DATA_SEPARATOR_SOH = #$01; // SOH (Start-of-Header)
	DATA_SEPARATOR_STX = #$02; // STX (Start-Text)
	DATA_SEPARATOR_ETX = #$03; // ETX (End-Text)
	DATA_SEPARATOR_ETB = #$17; // ETB (End-of-Transmission-Block) 
	DATA_SEPARATOR_EOT = #$04; // EOT (End-of-Transmission)	 

	DATA_LINE_SEPARATOR_01 = #$0D;

var
	DataToSend: String;
	EnableSendingDataToLm: Boolean;

begin
	// Verwenden Sie "True", um den Datenstring an das Gerät zu senden. Benutzen Sie "False",
	// wenn Sie den Datenstring nicht an das Gerät senden möchten. Die Einstellung muss zu den
	// Einstellungen am Gerät passen. Nutzen des ComMode NCP10 am Gerät für eine einfache Übertragung.
	EnableSendingDataToLm:= False;

	// --- Don't edit script down below --- 

	DataToSend:= '';

	if (EnableSendingDataToLm) then
	begin
		// This is a command for computer to request LM to send data.
		// The computer sends this command to LM when the computer becomes ready
		// to receive data.
		DataToSend:= DATA_SEPARATOR_SOH + 
								 'CLM' + 
								 DATA_SEPARATOR_STX + 
								 'SD' + 
								 DATA_SEPARATOR_ETB + 
								 DATA_SEPARATOR_EOT +
								 DATA_LINE_SEPARATOR_01;
	end;

	if (DataToSend <> '') then
	begin
		DoCOMPSSendData(DataToSend);
	end;

	DoCOMPSReceiveData();

	if (FGlobalTimeoutReached) then
	begin
		if (Length(FCOMBuffer) <= 0) then
		begin
			Exit;
		end;
	end;

	if (FErrorOccurred) then
	begin
		Exit;
	end;
end.
