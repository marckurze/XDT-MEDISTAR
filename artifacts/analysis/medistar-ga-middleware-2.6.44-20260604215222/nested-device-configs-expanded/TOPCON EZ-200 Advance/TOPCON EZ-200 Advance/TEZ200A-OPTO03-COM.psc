const
  VERSION = '1.0.21.78';
  DATE = '25.01.2023 08:56:21';
  TEXT = 'Copyright (c) 2023 team2work GmbH';

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
  // Use boolean as true, to send data string to device. Use boolean as false, 
  // if you don't want to send data string to the device.
  EnableSendingDataToLm:= True;

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

  if FGlobalTimeoutReached then
  begin
    if Length(FCOMBuffer) <= 0 then
      Exit;
  end;

  if FErrorOccurred then
  begin
    Exit;
  end;
end.