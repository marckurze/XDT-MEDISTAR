const
  VERSION = '1.0.55.41';
  DATE = '14.09.2022 13:20:11';
  TEXT = 'Copyright (c) 2022 team2work GmbH';

  DATA_LINE_SEPARATOR_01 = #$0D; // #$0D#$0A

  DEVICE_IDENTIFIER = 'VISUPLAN500';

  GDT_FID_PATIENT_ID = '3000';
  GDT_FID_MEASURE_DATA = '6228';

var
  arrData, RightEyeIOPmmHgArray, LeftEyeIOPmmHgArray, RightEyeIOPkPaArray, LeftEyeIOPkPaArray: TStringArray;
  bolUseTimeFromMeasurement: Boolean;
  i, indexMeasurementStart: Integer;  
  R_Line, L_Line, PatientID, ParsedData, Data, S, S1: String;
  DeviceName, TimeStampDate, TimeStampTime, MeasuredEye, MeasurementUnit: String;
  RightEyemmHg, R_MeasurementsmmHg, R_IOP1mmHg, R_IOP2mmHg, R_IOP3mmHg, R_IOP4mmHg, R_IOPAVGmmHg: String;
  R_unitMeasurementmmHg, L_unitMeasurementmmHg: String;
  RightEyekPa, R_IOP1kPa, R_IOP2kPa, R_IOP3kPa, R_IOP4kPa, R_IOPAVGkPa: String;  
  LeftEyemmHg, L_MeasurementsmmHg, L_IOP1mmHg, L_IOP2mmHg, L_IOP3mmHg, L_IOP4mmHg, L_IOPAVGmmHg: String;
  LeftEyekPa, L_IOP1kPa, L_IOP2kPa, L_IOP3kPa, L_IOP4kPa, L_IOPAVGkPa: String;  
  DeviceSerialNumber, unitMeasurement: String;

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

  // Get array of lines
  arrData := Explode(DATA_LINE_SEPARATOR_01, Data, 0);

  if (Length(arrData) <= 0) then
   begin
    FLastErrorCode := -3;
    FLastErrorMessage := 'Keine Zeilen für die Verarbeitung verfügbar';

    DoPSError();

    Exit;
  end;
  
  // set default values
  indexMeasurementStart:= -1;
  
  PatientID := '';
  
  R_Line:= '';
  L_Line:= '';  
  
  //
  DeviceName:= '';
  TimeStampDate:= '';
  TimeStampTime:= '';

  //
  MeasuredEye:= '';
  MeasurementUnit:= '';

  //
  RightEyemmHg:= '';   
  R_MeasurementsmmHg:= '';    
  R_IOP1mmHg:= '';     
  R_IOP2mmHg:= '';           
  R_IOP3mmHg:= '';     
  R_IOP4mmHg:= '';    
  R_IOPAVGmmHg:= '';
  
  R_unitMeasurementmmHg:= 'mmHg';
  
  // 
  RightEyekPa:= '';
  R_IOP1kPa:='';
  R_IOP2kPa:= '';
  R_IOP3kPa:= '';
  R_IOP4kPa:= '';
  R_IOPAVGkPa:= '';
  
  //
  LeftEyemmHg:= '';   
  L_MeasurementsmmHg:= '';    
  L_IOP1mmHg:= '';     
  L_IOP2mmHg:= '';           
  L_IOP3mmHg:= '';     
  L_IOP4mmHg:= '';    
  L_IOPAVGmmHg:= '';

  L_unitMeasurementmmHg:= 'mmHg';
  
  //
  LeftEyekPa:= '';
  L_IOP1kPa:='';
  L_IOP2kPa:= '';
  L_IOP3kPa:= '';
  L_IOP4kPa:= '';
  L_IOPAVGkPa:= '';
  
  //
  DeviceSerialNumber:= '';
  unitMeasurement:= '';	
  
  // 
  SetLength(RightEyeIOPmmHgArray, 0);
  SetLength(RightEyeIOPkPaArray, 0);  
  
  SetLength(LeftEyeIOPmmHgArray, 0);
  SetLength(LeftEyeIOPkPaArray, 0);

  // Parse raw data
  for i := 0 to Length(arrData) - 1 do
  begin
    S1:= Trim(arrData[i]);
  
    if (S1 = '') then
      Continue;
    
    // Device Name
    if (S1 = DEVICE_IDENTIFIER) then
    begin
      indexMeasurementStart:= i;

      DeviceName:= S1;

      Continue;
    end;
    
    // Time Stamp
    if (TimeStampDate = '')
    and (indexMeasurementStart <> -1) then
    begin
      TimeStampDate:= S1;
    
      Continue;
    end;    

    if (TimeStampTime = '')
    and (indexMeasurementStart <> -1) then
    begin
      TimeStampTime:= S1;
    
      Continue;
    end;      
    
    // Measured Eye, measurement unit
    if (MeasuredEye = '')
    and (indexMeasurementStart <> -1) then
    begin
      MeasuredEye:= S1;
    
      Continue;
    end;     
    
    if (MeasurementUnit = '')
    and (indexMeasurementStart <> -1) then
    begin
      MeasurementUnit:= S1;
    
      Continue;
    end; 
    
    // --------------------------------------------------
    
    // Right Eye IOP (mmHg)
    
    // Right Eye (mmHg) 
    if (RightEyemmHg = '')
    and (indexMeasurementStart <> -1) then
    begin
      RightEyemmHg:= S1;
    
      Continue;
    end;
    
    // Measurements (mmHg)    
    if (R_MeasurementsmmHg = '')
    and (indexMeasurementStart <> -1) then
    begin
      R_MeasurementsmmHg:= S1;
    
      Continue;
    end;
    
    // IOP 1 (mmHg)    
    if (R_IOP1mmHg = '')
    and (indexMeasurementStart <> -1) then
    begin
      R_IOP1mmHg:= S1;
    
      Continue;
    end; 
       
    // IOP 2 (mmHg)    
    if (R_IOP2mmHg = '')
    and (indexMeasurementStart <> -1) then
    begin
      R_IOP2mmHg:= S1;
    
      Continue;
    end;     
    
    // IOP 3 (mmHg)    
    if (R_IOP3mmHg = '')
    and (indexMeasurementStart <> -1) then
    begin
      R_IOP3mmHg:= S1;
    
      Continue;
    end;       
  
    // IOP 4 (mmHg)    
    if (R_IOP4mmHg = '')
    and (indexMeasurementStart <> -1) then
    begin
      R_IOP4mmHg:= S1;
    
      Continue;
    end;
    
    // IOP AVG (mmHg)  
    if (R_IOPAVGmmHg = '')
    and (indexMeasurementStart <> -1) then
    begin
      R_IOPAVGmmHg:= S1;
    
      Continue;
    end;
    
    // --------------------------------------------------
    
    // Right Eye IOP (kPa) 

    // Right Eye (kPa)
    if (RightEyekPa = '')
    and (indexMeasurementStart <> -1) then
    begin
      RightEyekPa:= S1;
    
      Continue;
    end;
    
    // IOP 1 (kPa)   
    if (R_IOP1kPa = '')
    and (indexMeasurementStart <> -1) then
    begin
      R_IOP1kPa:= S1;
    
      Continue;
    end; 
    
    // IOP 2 (kPa)   
    if (R_IOP2kPa = '')
    and (indexMeasurementStart <> -1) then
    begin
      R_IOP2kPa:= S1;
    
      Continue;
    end;
    
    // IOP 3 (kPa)    
    if (R_IOP3kPa = '')
    and (indexMeasurementStart <> -1) then
    begin
      R_IOP3kPa:= S1;
    
      Continue;
    end;
    
    // IOP 4 (kPa)     
    if (R_IOP4kPa = '')
    and (indexMeasurementStart <> -1) then
    begin
      R_IOP4kPa:= S1;
    
      Continue;
    end;
    
    // IOP AVG (kPa)
    if (R_IOPAVGkPa = '')
    and (indexMeasurementStart <> -1) then
    begin
      R_IOPAVGkPa:= S1;
    
      Continue;
    end;
    
    // --------------------------------------------------

    // Left Eye IOP (mmHg)
    
    // Left Eye (mmHg) 
    if (LeftEyemmHg = '')
    and (indexMeasurementStart <> -1) then
    begin
      LeftEyemmHg:= S1;
    
      Continue;
    end;
    
    // Measurements (mmHg)    
    if (L_MeasurementsmmHg = '')
    and (indexMeasurementStart <> -1) then
    begin
      L_MeasurementsmmHg:= S1;
    
      Continue;
    end;
    
    // IOP 1 (mmHg)    
    if (L_IOP1mmHg = '')
    and (indexMeasurementStart <> -1) then
    begin
      L_IOP1mmHg:= S1;
    
      Continue;
    end; 
       
    // IOP 2 (mmHg)    
    if (L_IOP2mmHg = '')
    and (indexMeasurementStart <> -1) then
    begin
      L_IOP2mmHg:= S1;
    
      Continue;
    end;     
    
    // IOP 3 (mmHg)    
    if (L_IOP3mmHg = '')
    and (indexMeasurementStart <> -1) then
    begin
      L_IOP3mmHg:= S1;
    
      Continue;
    end;       
  
    // IOP 4 (mmHg)    
    if (L_IOP4mmHg = '')
    and (indexMeasurementStart <> -1) then
    begin
      L_IOP4mmHg:= S1;
    
      Continue;
    end;
    
    // IOP AVG (mmHg)  
    if (L_IOPAVGmmHg = '')
    and (indexMeasurementStart <> -1) then
    begin
      L_IOPAVGmmHg:= S1;

      Continue;
    end;
    
    // -------------------------------------------------- 
      
    // Left Eye IOP (kPa) 

    // Left Eye (kPa)
    if (LeftEyekPa = '')
    and (indexMeasurementStart <> -1) then
    begin
      LeftEyekPa:= S1;
    
      Continue;
    end;
    
    // IOP 1 (kPa)   
    if (L_IOP1kPa = '')
    and (indexMeasurementStart <> -1) then
    begin
      L_IOP1kPa:= S1;
    
      Continue;
    end; 
    
    // IOP 2 (kPa)   
    if (L_IOP2kPa = '')
    and (indexMeasurementStart <> -1) then
    begin
      L_IOP2kPa:= S1;
    
      Continue;
    end;
    
    // IOP 3 (kPa)    
    if (L_IOP3kPa = '')
    and (indexMeasurementStart <> -1) then
    begin
      L_IOP3kPa:= S1;
    
      Continue;
    end;
    
    // IOP 4 (kPa)     
    if (L_IOP4kPa = '')
    and (indexMeasurementStart <> -1) then
    begin
      L_IOP4kPa:= S1;
    
      Continue;
    end;
    
    // IOP AVG (kPa)
    if (L_IOPAVGkPa = '')
    and (indexMeasurementStart <> -1) then
    begin
      L_IOPAVGkPa:= S1;
    
      Continue;
    end;    
    
    // --------------------------------------------------
    
    // Device Serial Number
    if (DeviceSerialNumber = '')
    and (indexMeasurementStart <> -1) then
    begin
      DeviceSerialNumber:= S1;
    
      Continue;
    end;       

    // 
    
  end;

  // Right Eye IOP (mmHg)
  SetLength(RightEyeIOPmmHgArray, 5);
  
  RightEyeIOPmmHgArray[0]:= R_IOP1mmHg;
  RightEyeIOPmmHgArray[1]:= R_IOP2mmHg;  
  RightEyeIOPmmHgArray[2]:= R_IOP3mmHg;  
  RightEyeIOPmmHgArray[3]:= R_IOP4mmHg;  
  RightEyeIOPmmHgArray[4]:= '[' + R_IOPAVGmmHg + ']';
  
  // Right Eye IOP (kPa)
  SetLength(RightEyeIOPkPaArray, 5);
  
  RightEyeIOPkPaArray[0]:= R_IOP1kPa;
  RightEyeIOPkPaArray[1]:= R_IOP2kPa;  	
  RightEyeIOPkPaArray[2]:= R_IOP3kPa;  	
  RightEyeIOPkPaArray[3]:= R_IOP4kPa;  	
  RightEyeIOPkPaArray[4]:= R_IOPAVGkPa;  	

  // Left Eye IOP (mmHg)
  SetLength(LeftEyeIOPmmHgArray, 5);
  
  LeftEyeIOPmmHgArray[0]:= L_IOP1mmHg;
  LeftEyeIOPmmHgArray[1]:= L_IOP2mmHg;  
  LeftEyeIOPmmHgArray[2]:= L_IOP3mmHg;  
  LeftEyeIOPmmHgArray[3]:= L_IOP4mmHg;  
  LeftEyeIOPmmHgArray[4]:= '[' + L_IOPAVGmmHg + ']';
  
  // Left Eye IOP (kPa)
  SetLength(LeftEyeIOPkPaArray, 5);
  
  LeftEyeIOPkPaArray[0]:= L_IOP1kPa;
  LeftEyeIOPkPaArray[1]:= L_IOP2kPa;  	
  LeftEyeIOPkPaArray[2]:= L_IOP3kPa;  	
  LeftEyeIOPkPaArray[3]:= L_IOP4kPa;  	
  LeftEyeIOPkPaArray[4]:= L_IOPAVGkPa;

  unitMeasurement:= '';
  	
  if (R_unitMeasurementmmHg <> '') then
  begin
    unitMeasurement:= R_unitMeasurementmmHg;
  end;	
  
  if (unitMeasurement = '')
  and (L_unitMeasurementmmHg <> '') then
  begin
    unitMeasurement:= L_unitMeasurementmmHg;
  end;
  
  if (unitMeasurement = '')	then
  begin
    unitMeasurement:= 'mmHg';
  end;

  // Build output
  ParsedData := '';

  // Add patient ID
  if (PatientID <> '') then
   begin
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
    ParsedData := ParsedData + PatientID + FOutputLineSeparator;
  end;

  //
  R_Line:= 'R =';
  	
  S:= '';
   
  for i := 0 to Length(RightEyeIOPmmHgArray) - 1 do
  begin
    S1:= Trim(RightEyeIOPmmHgArray[i]); 
    
    if (S1 = '') then
      Continue;
    
    if (T2WContainsStr(S1, '*')) then
      Continue;

    S:= S + ' ' + S1;
  end;

  R_Line:= R_Line + S;
  
  // 
  L_Line:= ' // L =';

  S:= '';
   
  for i := 0 to Length(LeftEyeIOPmmHgArray) - 1 do
  begin
    S1:= Trim(LeftEyeIOPmmHgArray[i]); 
    
    if (S1 = '') then
      Continue;
    
    if (T2WContainsStr(S1, '*')) then
      Continue;

    S:= S + ' ' + S1;
  end;

  L_Line:= L_Line + S;

  //R_Line:= T2WStringReplace(R_Line, '{', '', True, True);
  //R_Line:= T2WStringReplace(R_Line, '}', '', True, True);
  	
  //L_Line:= T2WStringReplace(L_Line, '{', '', True, True);
  //L_Line:= T2WStringReplace(L_Line, '}', '', True, True);  		

  //
  ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;

  if (bolUseTimeFromMeasurement) then
  begin
    ParsedData := ParsedData + R_Line + L_Line + ' ' + unitMeasurement + ' ' + TimeStampTime + FOutputLineSeparator;  
  end
  else
  begin
    ParsedData := ParsedData + R_Line + L_Line + ' ' + unitMeasurement + ' ' + GetCurrentTime(False) + FOutputLineSeparator;  
  end;

  // Set output
  FParsedDataString := RawByteString(ParsedData);
end.