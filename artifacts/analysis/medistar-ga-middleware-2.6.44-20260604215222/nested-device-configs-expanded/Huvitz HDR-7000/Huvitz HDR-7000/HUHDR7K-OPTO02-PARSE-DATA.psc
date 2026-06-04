const
  VERSION = '1.0.41.42';
  DATE = '10.11.2025 07:16:11';
  TEXT = 'Copyright (c) 2025 CompuGroup Medical Deutschland AG';
  
  CSV_LINE_SEPARATOR = ',';
  CSV_VALUE_SEPARATOR = ',';

  GDT_FID_PATIENT_ID     = '3000';
  GDT_FID_MEASURE_DATA   = '6228';
  GDT_FID_COMMENT        = '6227';
  GDT_FID_RESULT         = '6220';
  GDT_FID_FOREIGN_RESULT = '6221';
  GDT_FID_SIGNATURE      = '8990';
  GDT_FID_DIAG           = '6205';  
  
  GDT_FID_FILE_ARCHIVE_NUMBER = '6302';
  GDT_FID_FILE_FORMAT         = '6303';
  GDT_FID_FILE_DESCRIPTION    = '6304';
  GDT_FID_FILE_URL            = '6305';
  
  GDT_LINE_PREFIX = '  ';
  GDT_SIGN_SEPARATOR = ' ';
  GDT_AXIS_SEPARATOR = ' ';
  GDT_AXIS_SEPARATOR_MAX_COUNT = 3; 
  
  CSV_FIELD_NUMBER_PATIENT_ID       = 5;
  CSV_FIELD_NUMBER_PD_RIGHT         = 10;
  CSV_FIELD_NUMBER_PD_LEFT          = 11;
  CSV_FIELD_NUMBER_WORKING_DISTANCE = 15;
  CSV_FIELD_NUMBER_VA_FAR_OU        = 16;
  
  CSV_FIELD_NUMBER_LENSOMETRY_DATA_SPH_RIGHT        = 22;
  CSV_FIELD_NUMBER_LENSOMETRY_DATA_CYL_RIGHT        = 23;
  CSV_FIELD_NUMBER_LENSOMETRY_DATA_AXIS_RIGHT       = 24;
  CSV_FIELD_NUMBER_LENSOMETRY_DATA_ADD_RIGHT        = 25;
  CSV_FIELD_NUMBER_LENSOMETRY_DATA_HOR_PRISM_RIGHT  = 26;
  CSV_FIELD_NUMBER_LENSOMETRY_DATA_VERT_PRISM_RIGHT = 27;
  CSV_FIELD_NUMBER_LENSOMETRY_DATA_SPH_LEFT         = 28;
  CSV_FIELD_NUMBER_LENSOMETRY_DATA_CYL_LEFT         = 29;
  CSV_FIELD_NUMBER_LENSOMETRY_DATA_AXIS_LEFT        = 30;
  CSV_FIELD_NUMBER_LENSOMETRY_DATA_ADD_LEFT         = 31;
  CSV_FIELD_NUMBER_LENSOMETRY_DATA_HOR_PRISM_LEFT   = 32;
  CSV_FIELD_NUMBER_LENSOMETRY_DATA_VERT_PRISM_LEFT  = 33;
  
  CSV_FIELD_NUMBER_REFRACTOMETRY_DATA_SPH_RIGHT  = 37;
  CSV_FIELD_NUMBER_REFRACTOMETRY_DATA_CYL_RIGHT  = 38;
  CSV_FIELD_NUMBER_REFRACTOMETRY_DATA_AXIS_RIGHT = 39;
  CSV_FIELD_NUMBER_REFRACTOMETRY_DATA_SPH_LEFT   = 40;
  CSV_FIELD_NUMBER_REFRACTOMETRY_DATA_CYL_LEFT   = 41;
  CSV_FIELD_NUMBER_REFRACTOMETRY_DATA_AXIS_LEFT  = 42; 

  CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_SPH_RIGHT        = 80;
  CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_CYL_RIGHT        = 81;
  CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_AXIS_RIGHT       = 82;
  CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_ADD_RIGHT        = 83;
  CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_HOR_PRISM_RIGHT  = 84;
  CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_VERT_PRISM_RIGHT = 85;
  
  CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_SPH_LEFT         = 86;
  CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_CYL_LEFT         = 87;
  CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_AXIS_LEFT        = 88;
  CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_ADD_LEFT         = 89;
  CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_HOR_PRISM_LEFT   = 90;
  CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_VERT_PRISM_LEFT  = 91;
  
  // Diese Werte müssen übereinstimmen mit den Werten
  // aus der Device INI Datei.
  NonGDTDataLineLensmeter = 'V0';
  NonGDTDataLineRefraktometerObjektiv = 'V1';
  NonGDTDataLinePhoropter = 'V2';
  NonGDTDataLineVerordnung = 'V3';
  NonGDTDataLineRefraktometerSubjektiv = 'V4';
  NonGDTDataLineVisusKorrektur = 'V5';
  NonGDTDataLineSondereintraege = 'V6';
  NonGDTDataLineKeratometer = 'V7';
  NonGDTDataLineVisus = 'V';

function FormatSignValue(const ID, Value: String; AddSign: Boolean; AddSignSeparator: Boolean): String;
var
  S1, S2: String;
begin
  Result:= ID;

  S1:= Value;
  
  if (AddSign) then
  begin
    if (S1[1] = '+') then
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
      
  if (AddSignSeparator) then
    if not T2WStartsStr(GDT_SIGN_SEPARATOR, S1) then
      S2:= S2 + GDT_SIGN_SEPARATOR;

  Result:= Result + S2 + S1;
end;

function FormatAxisValue(const ID, Value: String; AddAxisSeparator: Boolean): String;
var
  S: String;
begin
  Result:= ID;
  
  S:= Value;
  
  if (AddAxisSeparator) then
    while Length(S) < GDT_AXIS_SEPARATOR_MAX_COUNT do
      S:= GDT_AXIS_SEPARATOR + S;
  
  Result:= Result + S;
end;

var
  arrData: TStringArray;
  ParsedData, S, PatientID, Data: String;
  R_Line, L_Line: String;
  Keratometer_R_Line, Keratometer_L_Line: String; 
  R_S, R_Z, R_Axis, R_PD, R_ADD, R_H_PRISM, R_V_PRISM: String;
  L_S, L_Z, L_Axis, L_PD, L_ADD, L_H_PRISM, L_V_PRISM: String;
  PD, WD, VD, VA: String;
  K_R_SPH, K_R_CYL, K_R_AXIS, K_L_SPH, K_L_CYL, K_L_AXIS: String;
  bolAddSign, bolAddSignSeparator, bolAddAxisSeparator, bolAddPDValueToOutput, bolAddVDValueToOutput: Boolean;
  bolAddWDValueToOutput, bolAddVAValueToOutput: Boolean;
  i, intFieldNr: Integer;
  LM_R_S, LM_R_Z, LM_R_Axis, LM_R_ADD, LM_L_S, LM_L_Z, LM_L_Axis, LM_L_ADD: String;
  REF_R_S, REF_R_Z, REF_R_Axis, REF_L_S, REF_L_Z, REF_L_Axis: String;
  
begin
  // Verwende "True", damit immer ein Vorzeichen hinzugefügt wird, 
  // benutze "False", damit nur der gemessene Wert eingetragen wird wie er vom Gerät kommt 
  bolAddSign:= True;

  // Verwende "True", damit nach jedem Vorzeichen der Wert aus GDT_SIGN_SEPARATOR 
  // angefügt wird, benutze "False", damit kein Abstand zwischen Vorzeichen und Wert eingefügt wird
  bolAddSignSeparator:= True;
  
  // Verwende "True", damit der Achsenseparator angefügt wird,
  // benutze "False", damit der Achsenseparator nicht verwendet wird
  bolAddAxisSeparator:= True;
  
  // Verwende "True", damit der PD Wert (rechts & links) hinzugefügt wird, benutze "False",
  // damit der PD Wert nicht per GDT exportiert wird.
  bolAddPDValueToOutput:= True;  
  
  // Verwende "True", damit der VD Wert hinzugefügt wird, benutze "False",
  // damit der VD Wert nicht per GDT exportiert wird.
  bolAddVDValueToOutput:= False;
  
  // Verwende "True", damit der VA Wert hinzugefügt wird, benutze "False",
  // damit der VA Wert nicht per GDT exportiert wird.
  bolAddVAValueToOutput:= False;

  // Verwende "True", damit der WD Wert hinzugefügt wird, benutze "False", damit der
  // WD Wert nicht per GDT exportiert wird.
  bolAddWDValueToOutput:= True;

  // --- Don't edit script down below ---
  
  // Clear parsed data string
  FParsedDataString:= '';
  
  if (Length(FRawDataString) <= 0) then
  begin
    FLastErrorCode := -2;
    FLastErrorMessage := 'Keine Daten für die Verarbeitung der Daten gefunden';

    DoPSError();
 
    Exit;
  end;
  
  // T2WMessageBoxS(FRawDataString);
  
  // Reset variables
  PatientID:= '';
  
  intFieldNr:= 0;
  
  R_S:= '';
  R_Z:= ''; 
  R_Axis:= '';
  R_PD:= '';
  R_ADD:= '';
  R_H_PRISM:= '';
  R_V_PRISM:= '';
  
  L_S:= '';
  L_Z:= '';
  L_Axis:= '';
  L_PD:= '';
  L_ADD:= '';
  L_H_PRISM:= '';
  L_V_PRISM:= '';
  
  WD:= '';
  VD:= '';
  PD:= '';
  VA:= '';
  
  K_R_SPH:= '';
  K_R_CYL:= ''; 
  K_R_AXIS:= ''; 
  K_L_SPH:= ''; 
  K_L_CYL:= ''; 
  K_L_AXIS:= '';

  LM_R_S:= '';
  LM_R_Z:= ''; 
  LM_R_Axis:= '';
  LM_R_ADD:= '';  
  LM_L_S:= '';
  LM_L_Z:= '';
  LM_L_Axis:= '';  
  LM_L_ADD:= '';
  
  REF_R_S:= '';
  REF_R_Z:= ''; 
  REF_R_Axis:= ''; 
  REF_L_S:= '';
  REF_L_Z:= '';
  REF_L_Axis:= ''; 
  
  Data:= String(FRawDataString);
  Data:= Trim(Data);
  
  // Get array of CSV lines
  arrData:= Explode(CSV_LINE_SEPARATOR, Data, 0);
  
  if (Length(arrData) <= 0) then
  begin
    FLastErrorCode:= -3;
    FLastErrorMessage:= 'Keine CSV-Zeilen für die Verarbeitung verfügbar';

    DoPSError();

    Exit;
  end;

  // We use CSV-Format Export in Protocol Version 1.2
  // It is important to set the correct protocol version
  // in device settings, to be safe to use the correct field numbers.
  
  // TODO:
  
  if (Length(arrData) < 188) then
  begin
    FLastErrorCode:= -4;
    FLastErrorMessage:= 'Keine gültigen CSV-Zeilen für die Verarbeitung verfügbar';

    DoPSError();

    Exit;
  end;  
  
  for i:= 0 to Length(arrData) - 1 do
  begin
    S:= arrData[i];
    
    if (S <> '') then
    begin
      S := T2WStringReplace(S, '"', '', True, False);
    end;
    
    intFieldNr:= i + 1;
    
    // T2WMessageBoxS(IntToStr(intFieldNr) + ' ' + S);
    
    if (intFieldNr = CSV_FIELD_NUMBER_PATIENT_ID) then
    begin
      PatientID:= Trim(S);
      Continue;
    end;
    
    if (intFieldNr = CSV_FIELD_NUMBER_PD_RIGHT) then
    begin
      R_PD:= Trim(S);
      Continue;
    end;
    
    if (intFieldNr = CSV_FIELD_NUMBER_PD_LEFT) then
    begin
      L_PD:= Trim(S);
      Continue;
    end;
    
    if (intFieldNr = CSV_FIELD_NUMBER_WORKING_DISTANCE) then
    begin
      WD:= Trim(S);
      Continue;
    end;
    
    // -------------------------------------------------------------

    if (intFieldNr =  CSV_FIELD_NUMBER_LENSOMETRY_DATA_SPH_RIGHT) then
    begin
      LM_R_S:= Trim(S);
      Continue;
    end;    

    if (intFieldNr = CSV_FIELD_NUMBER_LENSOMETRY_DATA_CYL_RIGHT) then
    begin
      LM_R_Z:= Trim(S);
      Continue;
    end;  
    
    if (intFieldNr = CSV_FIELD_NUMBER_LENSOMETRY_DATA_AXIS_RIGHT) then
    begin
      LM_R_Axis:= Trim(S);
      Continue;
    end;  
    
    if (intFieldNr = CSV_FIELD_NUMBER_LENSOMETRY_DATA_ADD_RIGHT) then
    begin
      LM_R_ADD:= Trim(S);
      Continue;
    end;      

    if (intFieldNr =  CSV_FIELD_NUMBER_LENSOMETRY_DATA_SPH_LEFT) then
    begin
      LM_L_S:= Trim(S);
      Continue;
    end;    

    if (intFieldNr = CSV_FIELD_NUMBER_LENSOMETRY_DATA_CYL_LEFT) then
    begin
      LM_L_Z:= Trim(S);
      Continue;
    end;  
    
    if (intFieldNr = CSV_FIELD_NUMBER_LENSOMETRY_DATA_AXIS_LEFT ) then
    begin
      LM_L_Axis:= Trim(S);
      Continue;
    end;

    if (intFieldNr = CSV_FIELD_NUMBER_LENSOMETRY_DATA_ADD_LEFT) then
    begin
      LM_L_ADD:= Trim(S);
      Continue;
    end;  
    
    // -------------------------------------------------------------    
    
    if (intFieldNr = CSV_FIELD_NUMBER_REFRACTOMETRY_DATA_SPH_RIGHT) then
    begin
      REF_R_S:= Trim(S);
      Continue;
    end;    
    
    if (intFieldNr = CSV_FIELD_NUMBER_REFRACTOMETRY_DATA_CYL_RIGHT) then
    begin
      REF_R_Z:= Trim(S);
      Continue;
    end;        
    
    if (intFieldNr = CSV_FIELD_NUMBER_REFRACTOMETRY_DATA_AXIS_RIGHT) then
    begin
      REF_R_Axis:= Trim(S);
      Continue;
    end;      
    
    if (intFieldNr = CSV_FIELD_NUMBER_REFRACTOMETRY_DATA_SPH_LEFT) then
    begin
      REF_L_S:= Trim(S);
      Continue;
    end;    
    
    if (intFieldNr = CSV_FIELD_NUMBER_REFRACTOMETRY_DATA_CYL_LEFT) then
    begin
      REF_L_Z:= Trim(S);
      Continue;
    end;    
    
    if (intFieldNr = CSV_FIELD_NUMBER_REFRACTOMETRY_DATA_AXIS_LEFT) then
    begin
      REF_L_Axis:= Trim(S);
      Continue;
    end;  
    
    // -------------------------------------------------------------
    
    if (intFieldNr = CSV_FIELD_NUMBER_VA_FAR_OU) then
    begin
      VA:= Trim(S);
      Continue;
    end;

    // -------------------------------------------------------------    

    // Final Prescription Data(FAR:Sph-Right)
    if (intFieldNr = CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_SPH_RIGHT) then
    begin
      R_S:= Trim(S);
      Continue;
    end;
    
    // Final Prescription Data(FAR:Cyl-Right)
    if (intFieldNr = CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_CYL_RIGHT) then
    begin
      R_Z:= Trim(S);
      Continue;
    end;
    
    // Final Prescription Data(FAR:Axis-Right)
    if (intFieldNr = CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_AXIS_RIGHT) then
    begin
      R_Axis:= Trim(S);
      Continue;
    end;
    
    // Final Prescription Data(FAR:ADD-Right)
    if (intFieldNr = CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_ADD_RIGHT) then
    begin
      R_ADD:= Trim(S);
      Continue;
    end;
    
    // Final Prescription Data(FAR:Hor.Prism-Right)
    if (intFieldNr = CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_HOR_PRISM_RIGHT) then
    begin
      R_H_PRISM:= Trim(S);
      Continue;
    end;
    
    // Final Prescription Data(FAR:Vert.Prism-Right)
    if (intFieldNr = CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_VERT_PRISM_RIGHT) then
    begin
      R_V_PRISM:= Trim(S);
      Continue;
    end;
        
    // -------------------------------------------------------------

    // Final Prescription Data(FAR:Sph-Left)
    if (intFieldNr = CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_SPH_LEFT) then
    begin
      L_S:= Trim(S);
      Continue;
    end;
    
    // Final Prescription Data(FAR:Cyl-Left)
    if (intFieldNr = CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_CYL_LEFT) then
    begin
      L_Z:= Trim(S);
      Continue;
    end;
    
    // Final Prescription Data(FAR:Axis-Left)
    if (intFieldNr = CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_AXIS_LEFT) then
    begin
      L_Axis:= Trim(S);
      Continue;
    end;
    
    // Final Prescription Data(FAR:ADD-Left)
    if (intFieldNr = CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_ADD_LEFT) then
    begin
      L_ADD:= Trim(S);
      Continue;
    end;
    
    // Final Prescription Data(FAR:Hor.Prism-Left)
    if (intFieldNr = CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_HOR_PRISM_LEFT) then
    begin
      L_H_PRISM:= Trim(S);
      Continue;
    end;
    
    // Final Prescription Data(FAR:Vert.Prism-Left)
    if (intFieldNr = CSV_FIELD_NUMBER_FINAL_PRESCRIPTION_DATA_FAR_VERT_PRISM_LEFT) then
    begin
      L_V_PRISM:= Trim(S);
      Continue;
    end;
    
    // TODO:
    
  end;

  // Build output
  ParsedData:= '';

  // Add patient ID
  if (PatientID <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
    ParsedData:= ParsedData + PatientID + FOutputLineSeparator;
  end;
  
  // Clear variables
  R_Line:= '';
  L_Line:= '';
  
  // Format values right eye
  if (R_S <> '') then
  begin
    R_S:= FormatSignValue('S=', R_S, bolAddSign, bolAddSignSeparator);
  end;
  
  if (R_Z <> '') then
  begin
    R_Z:= FormatSignValue('Z=', R_Z, bolAddSign, bolAddSignSeparator);
  end;
  
  if (R_Axis <> '') then
  begin
    R_Axis:= FormatAxisValue('*', R_Axis, bolAddAxisSeparator);
  end;
  
  if (R_ADD <> '') then
  begin
    R_ADD:= FormatSignValue('A=', R_ADD, bolAddSign, bolAddSignSeparator);
  end;
   
  // Format values left eye
  if (L_S <> '') then
  begin
    L_S:= FormatSignValue('S=', L_S, bolAddSign, bolAddSignSeparator);
  end;
  
  if (L_Z <> '') then
  begin
    L_Z:= FormatSignValue('Z=', L_Z, bolAddSign, bolAddSignSeparator);
  end;
  
  if (L_Axis <> '') then
  begin
    L_Axis:= FormatAxisValue('*', L_Axis, bolAddAxisSeparator);
  end;
  
  if (R_ADD <> '') then
  begin
    L_ADD:= FormatSignValue('A=', L_ADD, bolAddSign, bolAddSignSeparator);
  end;
  
  // Add right eye
  if (R_S <> '') and (R_Z <> '') and (R_Axis <> '') then
  begin
    R_Line:= R_Line + 'R.:' + R_S + ' ' + R_Z + R_Axis;   
  end;
  
  // Add left eye
  if (L_S <> '') and (L_Z <> '') and (L_Axis <> '') then
  begin
    L_Line:= L_Line + 'L.:' + L_S + ' ' + L_Z + L_Axis;
  end;
  
  // Add PD
  if (bolAddPDValueToOutput) then
  begin
    if (R_PD <> '') then
    begin
      if (R_Line <> '') then
      begin
        R_Line:= R_Line + ' ';
      end;
      
      R_Line:= R_Line + 'PD= ' + R_PD; 
    end;
    
    if (L_PD <> '') then
    begin
      if (L_Line <> '') then
      begin
        L_Line:= L_Line + ' ';
      end;
      
      L_Line:= L_Line + 'PD= ' + L_PD; 
    end;
  end;
    
  // Add WD if exists
  if (WD <> '') 
  and (bolAddWDValueToOutput) then
  begin
    // Append WD index with GDT comment field id to measure data
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_COMMENT + FOutputLineSeparator;
    ParsedData:= ParsedData + 'WD= ' + Trim(WD) + FOutputLineSeparator;
  end;
  
  if (bolAddVDValueToOutput) then
  begin
    if (VD <> '') then
    begin
      // TODO:
    end;
  end;
  
  if (bolAddVAValueToOutput) then
  begin
    if (VA <> '') then
    begin
      // TODO:
    end;
  end;

  if (R_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + R_Line + FOutputLineSeparator;
  end;
  
  if (L_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
  end;

  // Keratometer
  Keratometer_R_Line:= '';
  Keratometer_L_Line:= '';

  if (K_R_SPH <> '') 
  and (K_R_CYL <> '') 
  and (K_R_AXIS <> '') then
  begin
    Keratometer_R_Line:= Keratometer_R_Line + 'R.:' + K_R_SPH + ' ' + K_R_CYL + K_R_AXIS;
  end;
       
  if (K_L_SPH <> '') 
  and (K_L_CYL <> '') 
  and (K_L_AXIS <> '') then
  begin
    Keratometer_L_Line:= Keratometer_L_Line + 'L.:' + K_L_SPH + ' ' + K_L_CYL + K_L_AXIS;
  end;
  
  if (Keratometer_R_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_RESULT + FOutputLineSeparator;
    ParsedData:= ParsedData + Keratometer_R_Line + FOutputLineSeparator;
  end; 

  if (Keratometer_L_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FOREIGN_RESULT + FOutputLineSeparator;
    ParsedData:= ParsedData + Keratometer_L_Line + FOutputLineSeparator;
  end;
  
  // Set output
  FParsedDataString:= RawByteString(ParsedData);
end.
