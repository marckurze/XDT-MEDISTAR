begin
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
