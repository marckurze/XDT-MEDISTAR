const
  VERSION = '1.0.55.92';
  DATE = '13.02.2024 12:38:37';
  TEXT = 'Copyright (c) 2024 team2work GmbH';
  
  DATA_SOH = #$01;
  DATA_STX = #$02;
  DATA_ETB = #$17;  
  DATA_EOT = #$04;  
 
var
  DataToSend: String;
  bolSendData: Boolean;

begin

  // Verwende "True", damit das SD Kommando an das Gerät gesendet wird,
  // benutze "False", damit der Wert nicht berücksichtigt wird.
  bolSendData:= True;
  
  // --- Don't edit script down below ---
  
  DataToSend:= '';
  
  if (bolSendData) then
  begin
    DataToSend:= DATA_SOH + 'CRM' + DATA_STX + 'SD' + DATA_ETB + DATA_EOT;
  end;

  if (DataToSend <> '') 
  and (bolSendData) then
  begin
    DoCOMPSSendData(DataToSend);
    
    if (FErrorOccurred) then
    begin
      Exit;
    end;    
  end;

  DoCOMPSReceiveData();

  if (FGlobalTimeoutReached) then
  begin
    if Length(FCOMBuffer) <= 0 then
      Exit;
  end;

  if (FErrorOccurred) then
  begin
    Exit;
  end;
end.