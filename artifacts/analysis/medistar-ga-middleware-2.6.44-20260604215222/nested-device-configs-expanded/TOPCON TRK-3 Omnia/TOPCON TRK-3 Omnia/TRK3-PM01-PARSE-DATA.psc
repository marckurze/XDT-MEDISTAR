const
  VERSION = '1.0.55.43';
  DATE = '01.07.2025 12:30:28';
  TEXT = 'Copyright (c) 2025 CompuGroup Medical Deutschland AG';

  XPATH_EXPRESSION_01 = '//Ophthalmology/*[name()="nsCommon:Common"]/*[name()="nsCommon:Patient"]/*[name()="nsCommon:ID"]';
  XPATH_EXPRESSION_02 = '//Ophthalmology/*[name()="nsTM:Measure"]/*[name()="CCT"]/*[name()="R"]/*[name()="List"]/*[name()="CCT_mm"]';
  XPATH_EXPRESSION_03 = '//Ophthalmology/*[name()="nsTM:Measure"]/*[name()="CCT"]/*[name()="L"]/*[name()="List"]/*[name()="CCT_mm"]';

  XPATH_CCT_UNIT = 'mm';

  GDT_FID_PATIENT_ID = '3000';
  GDT_FID_MEASURE_DATA = '6228';
  GDT_FID_COMMENT = '6227';
  GDT_FID_SIGNATURE = '8990';
  GDT_FID_RESULT = '6220';
  GDT_FID_FOREIGN_RESULT = '6221';

var
  arrData: TStringArrayArray;
  ParsedData, PatientID, OutputLine: String;
  R_CCT_List, L_CCT_List: String;
  i: Integer;

begin
                        
  // --- Don't edit script down below ---

  // Clear parsed data string
  FParsedDataString:= '';

  if (not DoPSXMLDocumentExists) then
  begin
    FLastErrorCode:= -4;
    FLastErrorMessage:= 'Keine XML-Daten für die Verarbeitung verfügbar';

    DoPSError;

    Exit;
  end;

  if (not DoPSXMLRootNodeExists) then
  begin
    FLastErrorCode:= -5;
    FLastErrorMessage:= 'Kein XML-Wurzelknoten für die Verarbeitung verfügbar';

    DoPSError;

    Exit;
  end;
  
  // T2WMessageBoxS();

  // Patient ID
  PatientID:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_01);

  if Length(arrData) > 0 then
  begin
    PatientID:= PatientID + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
    PatientID:= PatientID + arrData[0][2] + FOutputLineSeparator;
  end;
  
  // CCT (Central Corneal Thickness)
  
  // R
  R_CCT_List:= '';
    
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_02);

  if Length(arrData) > 0 then
  begin
    for i:= 0 to Length(arrData) - 1 do
    begin
      R_CCT_List:= R_CCT_List + arrData[i][2] + ' ';
    end;
    
    R_CCT_List:= Trim(R_CCT_List);
  end;
  
  if (R_CCT_List <> '') then
  begin
    R_CCT_List:= 'R.: ' + R_CCT_List;
  end;
  
  // L  
  L_CCT_List:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_03);

  if Length(arrData) > 0 then
  begin
    for i:= 0 to Length(arrData) - 1 do
    begin
      L_CCT_List:= L_CCT_List + arrData[i][2] + ' ';
    end;
    
    L_CCT_List:= Trim(L_CCT_List);
  end;
  
  if (L_CCT_List <> '') then
  begin
    L_CCT_List:= '// L.: ' + L_CCT_List;
  end;
  
  // Build result
  ParsedData:= '';

  // Add patient ID
  if (PatientID <> '') then
  begin
    ParsedData:= ParsedData + PatientID;
  end;
  
  // Build result
  OutputLine:= '';
    
  if (R_CCT_List <> '') or (L_CCT_List <> '') then
  begin
    OutputLine:= 'CCT ';
  
    if (R_CCT_List <> '') then
    begin
      OutputLine:= OutputLine + R_CCT_List + ' ';
    end;

    if (L_CCT_List <> '') then
    begin
      OutputLine:= OutputLine + L_CCT_List + ' ';
    end;
  
    OutputLine:= OutputLine + XPATH_CCT_UNIT + ' ' + GetCurrentTime(False);
  end;  
    
  if (OutputLine <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + OutputLine;
  end;
  
  // TODO: add average ()

  // CCT R.:     () // L.:     () mm 13:16

  // Set output
  FParsedDataString:= RawByteString(ParsedData);
end.

