const
  VERSION = '1.0.19.40';
  DATE = '07.04.2021 13:37:11';
  TEXT = 'Copyright (c) 2021 team2work GmbH';

  DATA_LINE_SEPARATOR_01 = #$0D#$0A;

  LINE_IDENTIFIER_BREAK  = '------------------------';

  GDT_FID_PATIENT_ID = '3000';
  GDT_FID_MEASURE_DATA = '6228';

var
  arrData, arrData2: TStringArray;
  ParsedData, Data, S1, S2, S3, PatientID, S: String;
  i, indexMeasurementStart: Integer;
  dateTimeS: String;
  Avg_R, Avg_L, unitMeasurement: String;
  R_Line, L_Line: String;
  bolUseTimeFromMeasurement: Boolean;
begin
  // Man hat die Möglichkeit die "echte" Zeit der Messung aus den Messungsdaten
  // zu verwenden oder die aktuelle Zeit während der Verarbeitung der Messung.
  // Je nach Einstellung des Gerätes, kann dort eine andere Zeit ausgegeben werden.
  // Verwenden Sie "True", damit der Zeitwert aus der Messung übernommen wird oder 
  // wählen Sie "False", damit die Zeit aktuell ermittelt wird.
  bolUseTimeFromMeasurement := False;

  // --- Don't edit script down below ---

  // Clear parsed data string
  FParsedDataString := '';

  if Length(FRawDataString) <= 0 then
  begin
    FLastErrorCode := -2;
    FLastErrorMessage := 'Keine Daten für die Verarbeitung der Daten gefunden';

    DoPSError();
 
    Exit;
  end;

  Data:= String(FRawDataString);
  Data:= Trim(Data);

  indexMeasurementStart:= -1;

  // Get array of lines
  arrData := Explode(DATA_LINE_SEPARATOR_01, Data, 0);

  if (Length(arrData) <= 0) then
   begin
    FLastErrorCode := -3;
    FLastErrorMessage := 'Keine Zeilen für die Verarbeitung verfügbar';

    DoPSError();

    Exit;
  end;

  PatientID := '';

  Avg_R:= '';
  Avg_L:= '';
  unitMeasurement:= '';

  R_Line:= '';
  L_Line:= '';

  // set default values
  R_Line:= 'R =';
  L_Line:= ' // L =';

  // Parse raw data
  for i := 0 to Length(arrData) - 1 do
  begin
    S:= '';
    S1:= Trim(arrData[i]);
    S2:= '';
    S3:= '';

    SetLength(arrData2, 0);

    if (S1 = '') then
      Continue;

    // Separator line - The separator lines are always output.
    if T2WStartsStr(LINE_IDENTIFIER_BREAK, S1) then
      Continue;

    // next line must be the date, Date and time
    // The date and time set in the setting mode are output. The output format
    // is changed depending on the setting. The setting of ”YMD”
    // The other example: ”MDY”
    if (T2WContainsStr(S1, '/')) then
    begin
      arrData2:= Explode(' ', S1, 0);

      if Length(arrData2) > 0 then
      begin
        dateTimeS := Trim(arrData2[1]);
      end;

      Continue;
    end;
    
    // Measurement data title
    // The displaying position and the measurement unit of the measurement
    // data for the right and left eyes are output.
    if T2WStartsStr('R', S1) or T2WStartsStr('L', S1) then
    begin
      // get measurement unit
      S:= Trim(Copy(S1, Pos('[', S1)+1, Pos(']', S1)-Pos('[', S1)-1));

      unitMeasurement := S;

      indexMeasurementStart:= i+1;

      Continue;
    end;

    // The measurement data displayed on the screen is output in order of the measurement.
    // The data is output a maximum of 3 lines. The lines without data are not output.
    if (not T2WContainsStr(S1, 'Avg'))
    and (indexMeasurementStart <> -1)
    and (i <= (indexMeasurementStart + 3)) then
    begin
      S:= arrData[i];

      // check which values and how many values we have

      // check if we have right or left and right and left ...
      S2:= Trim(Copy(S, 4, 3));
      S3:= Trim(Copy(S, 17, Length(S)));

      if (S2 <> '') then
      begin
        R_Line:= R_Line + ' ' + S2;
      end;

      if (S3 <> '') then
      begin
        L_Line:= L_Line + ' ' + S3;
      end;

      Continue;
    end;

    // Average value of the measurement data
    // The average values of the measurement data for the right and the left
    // eyes are displayed.
    if (T2WContainsStr(S1, 'Avg')) then
    begin
      arrData2:= Explode('Avg', S1, 0);

      if Length(arrData2) > 0 then
      begin
        Avg_R:= Trim(arrData2[0]);
      
        if Length(arrData2) > 1 then
          Avg_L := Trim(arrData2[1]);
      end;

      Continue;
    end;
  end;

  // Build output
  ParsedData := '';

  // Add patient ID
  if (PatientID <> '') then
   begin
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
    ParsedData := ParsedData + PatientID + FOutputLineSeparator;
  end;

  if (Avg_R <> '') then
    R_Line := Trim(R_Line) + ' ' + '[' + Avg_R + ']';

  if (Avg_R <> '') then
    L_Line := L_Line + ' ' + '[' + Avg_L + ']';

  ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;

  if (bolUseTimeFromMeasurement) then
  begin
    ParsedData := ParsedData + R_Line + L_Line + ' ' + unitMeasurement + ' ' + dateTimeS + FOutputLineSeparator;  
  end
  else
  begin
    ParsedData := ParsedData + R_Line + L_Line + ' ' + unitMeasurement + ' ' + GetCurrentTime(False) + FOutputLineSeparator;  
  end;

  // Set output
  FParsedDataString := RawByteString(ParsedData);
end.