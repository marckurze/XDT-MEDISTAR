const
  VERSION = '1.0.55.52';
  DATE = '05.07.2022 14:41:21';
  TEXT = 'Copyright (c) 2022 team2work GmbH';
	
  DATA_SEPARATOR_01 = #13;
  DATA_SEPARATOR_02 = ' ';

  LINE_IDENTIFIER_IOP = 'IOP';
  LINE_IDENTIFIER_R   = '(R)';
  LINE_IDENTIFIER_L   = '(L)';

  GDT_FID_MEASURE_DATA_1 = '6228';
  GDT_FID_MEASURE_DATA_2 = '6227'; 
  
  DEVICE_NAME = 'REI7CR';
  
  GDT_LINE_PREFIX = '  '; 
  
var
  Data, Sep1, Sep2, S1, ParsedData, strColumnHeaders, strGDT_LINE_PREFIX: String;
  strDateTime, strMeasurementEyeRight, strMeasurementEyeLeft: String;
  arrData1, arrDataHeader, arrDataR, arrDataL, arrDataTime: TStringArray;
  i: Integer;
  bolUseTimeFromMeasurement, bolUseStrictValueChecking, bolAddPrefixForEachEyeLine: Boolean;
  
begin
  // Man hat die Möglichkeit die "echte" Zeit der Messung aus den Messungsdaten
  // zu verwenden oder die aktuelle Zeit während der Verarbeitung der Messung.
  // Je nach Einstellung des Gerätes, kann dort eine andere Zeit ausgegeben werden.
  // Verwenden Sie "True", damit der Zeitwert aus der Messung übernommen wird oder 
  // wählen Sie "False", damit die Zeit aktuell ermittelt wird.
  bolUseTimeFromMeasurement := True;
		
  // Verwenden Sie "True", damit alle Werte auf richtigkeit und Vollständigkeit
  // geprüft werden. Wählen Sie "False", damit die Prüfung auf die AIS Anwendung
  // verlagert wird.
  bolUseStrictValueChecking := False;	
  	
  // Verwende "True", damit vor jeder Zeile (pro Auge) Anzahl x Leerzeichen als Prefix
  // vorangestellt werden, benutze "False", damit dies nicht geschiet.
  bolAddPrefixForEachEyeLine:= False;  		
				
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

  Sep1 := DATA_SEPARATOR_01;
  Sep2 := DATA_SEPARATOR_02;

  Data := String(FRawDataString);
  Data := Trim(Data);

  strGDT_LINE_PREFIX:= '';
  	
  if (bolAddPrefixForEachEyeLine) then
  begin
    strGDT_LINE_PREFIX:= GDT_LINE_PREFIX;
  end;  	
  	
  // Get array from 1st separator
  arrData1 := Explode(Sep1, Data, 0);

  if Length(arrData1) <= 0 then
  begin
    FLastErrorCode := -3;
    FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Keine Parameter gefunden';

    DoPSError();

    Exit;
  end;

  // Set default array length
  SetLength(arrDataHeader, 0);
  SetLength(arrDataR, 0);
  SetLength(arrDataL, 0);
  SetLength(arrDataTime, 0);

  // Reset tmp vars
  strDateTime := '';
  strColumnHeaders := '';
  strMeasurementEyeRight := '';
  strMeasurementEyeLeft := '';

  // Parse raw data
  for i := 0 to Length(arrData1) - 1 do
  begin
    S1 := Trim(arrData1[i]);

    if (S1 = '') then
     Continue;

    // Date and time the measurements were taken
    if (i = 0) then
     begin
      strDateTime:= S1;
    end;

    // Column headers for eye measurements
    if T2WStartsStr(LINE_IDENTIFIER_IOP, S1) then
     begin
      strColumnHeaders := S1;
    end;

    if T2WStartsStr(LINE_IDENTIFIER_R, S1) then
     begin
      strMeasurementEyeRight := S1;
      strMeasurementEyeRight := T2WStringReplace(strMeasurementEyeRight, LINE_IDENTIFIER_R, '', False, False);
    end;

    if T2WStartsStr(LINE_IDENTIFIER_L, S1) then
     begin
      strMeasurementEyeLeft := S1;
      strMeasurementEyeLeft := T2WStringReplace(strMeasurementEyeLeft, LINE_IDENTIFIER_L, '', False, False);
    end;
  end;
  
  // Check the data and the values
  if (bolUseTimeFromMeasurement) then
   begin
    if (strDateTime = '') then
     begin
      strDateTime := GetCurrentTime(False);
     end
    else
     begin
      strDateTime := T2WDelDoubleSpaces(Trim(strDateTime));

      if ((strDateTime) <> '') then
       arrDataTime := Explode(' ', strDateTime, 0);

      if (Length(arrDataTime) > 0) then
       begin
        if (Length(arrDataTime) >= 2) then
         begin
          strDateTime := arrDataTime[1];
        end;
      end;
    end;
   end
  else
   begin
    strDateTime := GetCurrentTime(False);
  end;  

  // Check the given values
  if (strColumnHeaders = '') and (bolUseStrictValueChecking) then
   begin
    FLastErrorCode := -4;
    FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter "COLUMN" nicht vorhanden';

    DoPSError();

    Exit;
  end;

  if (strMeasurementEyeRight = '') and (bolUseStrictValueChecking) then
   begin
    FLastErrorCode := -5;
    FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter "R" nicht vorhanden';

    DoPSError();

    Exit;
  end;
  
  if (strMeasurementEyeLeft = '') and (bolUseStrictValueChecking) then
   begin
    FLastErrorCode := -6;
    FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter "L" nicht vorhanden';

    DoPSError();

    Exit;
  end;

  // load data into the arrays
  arrDataHeader := Explode(Sep2, strColumnHeaders, 0);
  arrDataR := Explode(Sep2, strMeasurementEyeRight, 0);
  arrDataL := Explode(Sep2, strMeasurementEyeLeft, 0);

  // Remove empty array values
  arrDataHeader := RemoveEmptyArrayValues(arrDataHeader);
  arrDataR := RemoveEmptyArrayValues(arrDataR);
  arrDataL := RemoveEmptyArrayValues(arrDataL);

  // compare the arrays
  if (Length(arrDataR) <> Length(arrDataL)) then
   begin
     if (bolUseStrictValueChecking) then
     begin 	
       FLastErrorCode := -7;
       FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter "R" und "L" sind ungleich';

       DoPSError();
  
       Exit;
     end;    
  end;

  if (Length(arrDataHeader) <> Length(arrDataR)) then
   begin
     if (bolUseStrictValueChecking) then
     begin
       FLastErrorCode := -8;
       FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter "R" und "HEADER" sind ungleich';

       DoPSError();

       Exit;
     end;
  end;

  if (Length(arrDataHeader) <> Length(arrDataL)) then
   begin
     if (bolUseStrictValueChecking) then
     begin
       FLastErrorCode := -9;
       FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter "L" und "HEADER" sind ungleich';

       DoPSError();

       Exit;
     end;
  end;

  if (Length(arrDataHeader) < 3) or (Length(arrDataR) < 3) or (Length(arrDataL) < 3) then
   begin
     if (bolUseStrictValueChecking) then
     begin  
       FLastErrorCode := -10;
       FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Nicht alle Daten vorhanden';

       DoPSError();

       Exit;
     end;
  end;

  // Start parsing ophthalmology data down here  

  // Reset parsed data
  ParsedData := '';

  // Process right and left eye   
  if (Length(arrDataR) > 0) and (Length(arrDataL) > 0) then
  begin
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA_2 + FOutputLineSeparator;  
    ParsedData := ParsedData + 'R.:' + ' ' + arrDataR[1] + '  ' + 'L.:' + ' ' + arrDataL[1] + '   ' + 'Score' + ' ' + arrDataR[2] + ' ' + '/' + ' ' + arrDataL[2]; 
    ParsedData := ParsedData + FOutputLineSeparator;  
  
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA_1 + FOutputLineSeparator;
    ParsedData := ParsedData + 'R.:' + ' ' + arrDataR[0] + '  ' + 'L.:' + ' ' + arrDataL[0] + '   ' + 'cc' + ' ' + DEVICE_NAME + ' ' + '/' + ' ' + 'Zeit:' + ' ' + strDateTime;  
  
    // Set result
    FParsedDataString := RawByteString(ParsedData);
    Exit;  
  end;
  
  // process only right eye
  if (Length(arrDataR) > 0) then
  begin
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA_2 + FOutputLineSeparator;  
    ParsedData := ParsedData + 'R.:' + ' ' + arrDataR[1] + '  ' + 'L.:' + ' ' + '' + '   ' + 'Score' + ' ' + arrDataR[2] + ' ' + '/' + ' ' + ''; 
    ParsedData := ParsedData + FOutputLineSeparator;  
  
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA_1 + FOutputLineSeparator;
    ParsedData := ParsedData + 'R.:' + ' ' + arrDataR[0] + '  ' + 'L.:' + ' ' + '' + '   ' + 'cc' + ' ' + DEVICE_NAME + ' ' + '/' + ' ' + 'Zeit:' + ' ' + strDateTime;  
  
    // Set result
    FParsedDataString := RawByteString(ParsedData);
    Exit;  
  end;
  
  // process only left eye 
  if (Length(arrDataL) > 0) then
  begin
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA_2 + FOutputLineSeparator;  
    ParsedData := ParsedData + 'R.:' + ' ' + '' + '  ' + 'L.:' + ' ' + arrDataL[1] + '   ' + 'Score' + ' ' + '' + ' ' + '/' + ' ' + arrDataL[2]; 
    ParsedData := ParsedData + FOutputLineSeparator;  
  
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA_1 + FOutputLineSeparator;
    ParsedData := ParsedData + 'R.:' + ' ' + '' + '  ' + 'L.:' + ' ' + arrDataL[0] + '   ' + 'cc' + ' ' + DEVICE_NAME + ' ' + '/' + ' ' + 'Zeit:' + ' ' + strDateTime;  
  
    // Set result
    FParsedDataString := RawByteString(ParsedData);
    Exit;  
  end;       
end.	