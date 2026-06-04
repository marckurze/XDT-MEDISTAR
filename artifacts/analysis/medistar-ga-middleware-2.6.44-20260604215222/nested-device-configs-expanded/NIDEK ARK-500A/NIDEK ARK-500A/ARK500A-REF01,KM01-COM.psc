const
  VERSION = '1.0.21.78';
  DATE = '31.01.2023 08:56:21';
  TEXT = 'Copyright (c) 2023 team2work GmbH';

  IF_MODE = 'NCP10'; // NIDEK or NCP10

begin
  // --- Don't edit script down below --- 

  // When the "HF MODE" parameter is set to NCP10... The DTR/DSR control is not performed.
  // The DTR signal of the ARK—500A is constantly at a low state on a line. 
  // The DTR signal of the external computer does not affect the transmission under either
  // condition. In addition, since the RS/SD command control is not performed, pressing the print button
  // sends the data directly.

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