const
  VERSION = '1.0.55.53';
  DATE = '30.11.2022 13:47:21';
  TEXT = 'Copyright (c) 2022 team2work GmbH';
	
  DATA_SEPARATOR_01 = #13;
  DATA_SEPARATOR_02 = ' ';

  LINE_IDENTIFIER_R   = 'R';
  LINE_IDENTIFIER_L   = 'L';
   
  IOP_UNIT = 'mmHg';
  
  GDT_FID_PATIENT_ID   = '3000';
  GDT_FID_MEASURE_DATA = '6228';
  GDT_FID_COMMENT      = '6227';
	
  GDT_FID_FILE_ARCHIVE_NUMBER  = '6302';
  GDT_FID_FILE_FORMAT          = '6303';
  GDT_FID_FILE_DESCRIPTION     = '6304';
  GDT_FID_FILE_URL             = '6305';

  GDT_LINE_PREFIX              = '  ';
  GDT_SIGN_SEPARATOR           = ' ';
  GDT_AXIS_SEPARATOR           = ' ';
  GDT_AXIS_SEPARATOR_MAX_COUNT = 3;
  
var
  Data, Sep1, Sep2, S1, ParsedData, strGDT_LINE_PREFIX, R_Line, L_Line: String;
  strDateTime, strMeasurementEyeRight, strMeasurementEyeLeft: String;
  arrData1, arrDataR, arrDataL, arrDataTime: TStringArray;
  i: Integer;
  bolUseTimeFromMeasurement, bolUseStrictValueChecking, bolAddPrefixForEachEyeLine: Boolean;
  
function FormatSignValue(const ID, Value: String; AddSign: Boolean; AddSignSeparator: Boolean): String;
var
  S1, S2: String;
begin
  Result:= ID;

  S1:= Value;
  
  if AddSign then
  begin
    if S1[1] = '+' then
    begin
      S1:= Copy(S1, 2, Length(S1) - 1);
      S2:= '+';
    end
    else if S1[1] = '-' then
    begin
      S1:= Copy(S1, 2, Length(S1) - 1);
      S2:= '-';
    end
    else
    begin
      S2:= '+';
    end;
  end
  else
  begin
    S2:= '';
  end;
      
  if AddSignSeparator then
    S2:= S2 + GDT_SIGN_SEPARATOR;
  
  Result:= Result + S2 + S1;
end;

function FormatAxisValue(const ID, Value: String; AddAxisSeparator: Boolean): String;
var
  S: String;
begin
  Result:= ID;
  
  S:= Value;
  
  if AddAxisSeparator then
    while Length(S) < GDT_AXIS_SEPARATOR_MAX_COUNT do
      S:= GDT_AXIS_SEPARATOR + S;
  
  Result:= Result + S;
end;
  
begin
  // Man hat die Möglichkeit die "echte" Zeit der Messung aus den Messungsdaten
  // zu verwenden oder die aktuelle Zeit während der Verarbeitung der Messung.
  // Je nach Einstellung des Gerätes, kann dort eine andere Zeit ausgegeben werden.
  // Verwenden Sie "True", damit der Zeitwert aus der Messung übernommen wird oder 
  // wählen Sie "False", damit die Zeit aktuell ermittelt wird.
  bolUseTimeFromMeasurement := False;
		
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
  SetLength(arrDataR, 0);
  SetLength(arrDataL, 0);
  SetLength(arrDataTime, 0);

  // Reset tmp vars
  strDateTime := '';
  strMeasurementEyeRight := '';
  strMeasurementEyeLeft := '';

  // Parse raw data
  for i := 0 to Length(arrData1) - 1 do
  begin
    S1 := Trim(arrData1[i]);

    if (S1 = '') then
     Continue;
     
    strDateTime:= '';

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
  if (strMeasurementEyeRight = '') 
  and (bolUseStrictValueChecking) then
   begin
    FLastErrorCode := -5;
    FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter "R" nicht vorhanden';

    DoPSError();

    Exit;
  end;
  
  if (strMeasurementEyeLeft = '') 
  and (bolUseStrictValueChecking) then
   begin
    FLastErrorCode := -6;
    FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter "L" nicht vorhanden';

    DoPSError();

    Exit;
  end;
  
  // each eye measurement line starts with the measurement count (2)
  strMeasurementEyeRight:=  Copy(strMeasurementEyeRight, 3, Length(strMeasurementEyeRight));
  strMeasurementEyeLeft:=  Copy(strMeasurementEyeLeft, 3, Length(strMeasurementEyeLeft));  	

  // load data into the arrays
  arrDataR := Explode(Sep2, strMeasurementEyeRight, 0);
  arrDataL := Explode(Sep2, strMeasurementEyeLeft, 0);

  // Remove empty array values
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
  
  // Start parsing ophthalmology data down here 
  
  // Reset parsed data
  ParsedData := '';
  	
  // Add right eye
  R_Line:= ''; 
  
  if (Length(arrDataR) > 0) then
  begin
    R_Line:= 'R = '; 
    
    for i := 0 to Length(arrDataR) - 1 do
    begin
      if (i = Length(arrDataR) - 1) then
      begin
        R_Line:= R_Line + '[' + arrDataR[i] + ']';
      end
      else
      begin
        R_Line:= R_Line + arrDataR[i] + ' ';
      end;
    end;
  end;
  
  R_Line:= Trim(R_Line);

  // Add left eye
  L_Line:= '';

  if (Length(arrDataL) > 0) then
  begin
    L_Line:= '// L = ';
    
    for i := 0 to Length(arrDataL) - 1 do
    begin
      if (i = Length(arrDataL) - 1) then
      begin
        L_Line:= L_Line + '[' + arrDataL[i] + ']';
      end
      else
      begin
        L_Line:= L_Line + arrDataL[i] + ' ';
      end;
    end;
  end;
  
  L_Line:= Trim(L_Line); 	   

  if (R_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + R_Line;
  end;

  if (L_Line <> '') then
  begin
    if (R_Line = '') then
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator
    else
      ParsedData:= ParsedData + ' ';
    
    ParsedData:= ParsedData + L_Line;
  end;
  
  if (R_Line <> '') or (L_Line <> '') then
  begin
    ParsedData:= ParsedData + ' ' + IOP_UNIT + ' ' + GetCurrentTime(False);
  end;    

  // Set output
  FParsedDataString := RawByteString(ParsedData);        
end.	