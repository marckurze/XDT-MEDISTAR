const
  VERSION = '1.0.6.52';
  DATE = '13.05.2020 13:05:41';
  TEXT = 'Copyright (c) 2020 team2work GmbH';

  XPATH_EXPRESSION_01 = '//Ophthalmology/*[name()="nsCommon:Common"]/*[name()="nsCommon:Patient"]/*[name()="nsCommon:ID"]';

  XPATH_EXPRESSION_02 = '//Ophthalmology/*[name()="nsTM:Measure"]/*[name()="nsTM:TM"]/*[name()="nsTM:R"]/*[name()="nsTM:List"]/*[name()="nsTM:IOP_mmHg"]';
  XPATH_EXPRESSION_03 = '//Ophthalmology/*[name()="nsTM:Measure"]/*[name()="nsTM:TM"]/*[name()="nsTM:R"]/*[name()="nsTM:Average"]/*[name()="nsTM:IOP_mmHg"]';

  XPATH_EXPRESSION_04 = '//Ophthalmology/*[name()="nsTM:Measure"]/*[name()="nsTM:TM"]/*[name()="nsTM:L"]/*[name()="nsTM:List"]/*[name()="nsTM:IOP_mmHg"]';
  XPATH_EXPRESSION_05 = '//Ophthalmology/*[name()="nsTM:Measure"]/*[name()="nsTM:TM"]/*[name()="nsTM:L"]/*[name()="nsTM:Average"]/*[name()="nsTM:IOP_mmHg"]';

  XPATH_EXPRESSION_06 = '//Ophthalmology/*[name()="nsTM:Measure"]/*[name()="nsTM:CorrectedIOP"]/*[name()="nsTM:Formula1"]/*[name()="nsTM:R"]/*[name()="nsTM:Param1"]';
  XPATH_EXPRESSION_07 = '//Ophthalmology/*[name()="nsTM:Measure"]/*[name()="nsTM:CorrectedIOP"]/*[name()="nsTM:Formula1"]/*[name()="nsTM:R"]/*[name()="nsTM:Param2"]';
  XPATH_EXPRESSION_08 = '//Ophthalmology/*[name()="nsTM:Measure"]/*[name()="nsTM:CorrectedIOP"]/*[name()="nsTM:Formula1"]/*[name()="nsTM:R"]/*[name()="nsTM:CCT"]';
  XPATH_EXPRESSION_09 = '//Ophthalmology/*[name()="nsTM:Measure"]/*[name()="nsTM:CorrectedIOP"]/*[name()="nsTM:Formula1"]/*[name()="nsTM:R"]/*[name()="nsTM:Measured"]/*[name()="nsTM:IOP_mmHg"]';
  XPATH_EXPRESSION_10 = '//Ophthalmology/*[name()="nsTM:Measure"]/*[name()="nsTM:CorrectedIOP"]/*[name()="nsTM:Formula1"]/*[name()="nsTM:R"]/*[name()="nsTM:Corrected"]/*[name()="nsTM:IOP_mmHg"]';

  XPATH_EXPRESSION_11 = '//Ophthalmology/*[name()="nsTM:Measure"]/*[name()="nsTM:CorrectedIOP"]/*[name()="nsTM:Formula1"]/*[name()="nsTM:L"]/*[name()="nsTM:Param1"]';
  XPATH_EXPRESSION_12 = '//Ophthalmology/*[name()="nsTM:Measure"]/*[name()="nsTM:CorrectedIOP"]/*[name()="nsTM:Formula1"]/*[name()="nsTM:L"]/*[name()="nsTM:Param2"]';
  XPATH_EXPRESSION_13 = '//Ophthalmology/*[name()="nsTM:Measure"]/*[name()="nsTM:CorrectedIOP"]/*[name()="nsTM:Formula1"]/*[name()="nsTM:L"]/*[name()="nsTM:CCT"]';
  XPATH_EXPRESSION_14 = '//Ophthalmology/*[name()="nsTM:Measure"]/*[name()="nsTM:CorrectedIOP"]/*[name()="nsTM:Formula1"]/*[name()="nsTM:L"]/*[name()="nsTM:Measured"]/*[name()="nsTM:IOP_mmHg"]';
  XPATH_EXPRESSION_15 = '//Ophthalmology/*[name()="nsTM:Measure"]/*[name()="nsTM:CorrectedIOP"]/*[name()="nsTM:Formula1"]/*[name()="nsTM:L"]/*[name()="nsTM:Corrected"]/*[name()="nsTM:IOP_mmHg"]';

  XPATH_UNIT = 'mmHg';
  XPATH_PARAM1_UNIT = 'mm';
  XPATH_CCT_UNIT = 'mm';

  GDT_FID_PATIENT_ID = '3000';
  GDT_FID_MEASURE_DATA = '6228';
  GDT_FID_COMMENT = '6227';
  GDT_FID_SIGNATURE = '8990';
  GDT_FID_RESULT = '6220';
  GDT_FID_FOREIGN_RESULT = '6221';

var
  arrData: TStringArrayArray;
  ParsedData, PatientID: String;
  R_TMList_mmHg, R_TMAverage_mmHg, R_TMCorrectedIOP_Param1, R_TMCorrectedIOP_Param2, R_TMCorrectedIOP_CCT, R_TMCorrectedIOP_Measured_IOP_mmHg, R_TMCorrectedIOP_Corrected_IOP_mmHg: String;
  L_TMList_mmHg, L_TMAverage_mmHg, L_TMCorrectedIOP_Param1, L_TMCorrectedIOP_Param2, L_TMCorrectedIOP_CCT, L_TMCorrectedIOP_Measured_IOP_mmHg, L_TMCorrectedIOP_Corrected_IOP_mmHg: String;
  R_Line, L_Line: String;
  i: Integer;
  bolAddSpecialLineP0ToGdtFile, bolAddSpecialLineP0UnitToGdtFile: Boolean;

begin
  // Aktiviere die Ausgabe der Spezialzeile P0.
  // Verwende "True" um die Ausgabe zu aktivieren, "False", um die Ausgabe zu deaktivieren
  bolAddSpecialLineP0ToGdtFile:= True;

  // Aktiviere die Ausgabe der Einheit für die Spezialzeile P0.
  // Verwende "True" um die Ausgabe zu aktivieren, "False", um die Ausgabe zu deaktivieren
  bolAddSpecialLineP0UnitToGdtFile:= False;
              
  // --- Don't edit script down below ---

  // Clear parsed data string
  FParsedDataString:= '';

  if not DoPSXMLDocumentExists then
  begin
    FLastErrorCode:= -4;
    FLastErrorMessage:= 'Keine XML-Daten für die Verarbeitung verfügbar';

    DoPSError;

    Exit;
  end;

  if not DoPSXMLRootNodeExists then
  begin
    FLastErrorCode:= -5;
    FLastErrorMessage:= 'Kein XML-Wurzelknoten für die Verarbeitung verfügbar';

    DoPSError;

    Exit;
  end;

  // Patient ID
  PatientID:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_01);

  if Length(arrData) > 0 then
  begin
    PatientID:= PatientID + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
    PatientID:= PatientID + arrData[0][2] + FOutputLineSeparator;
  end;

  // R_TMList_mmHg
  R_TMList_mmHg:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_02);

  if Length(arrData) > 0 then
  begin
    for i:= 0 to Length(arrData) - 1 do
    begin
      R_TMList_mmHg:= R_TMList_mmHg + arrData[i][2] + ' ';
    end;
    
    R_TMList_mmHg:= Trim(R_TMList_mmHg);
  end;

  // R_TMAverage_mmHg
  R_TMAverage_mmHg:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_03);

  if Length(arrData) > 0 then
  begin
    R_TMAverage_mmHg:= R_TMAverage_mmHg + '[' + arrData[0][2] + ']';
  end;

  // R_TMCorrectedIOP_Param1
  R_TMCorrectedIOP_Param1:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_06);

  if Length(arrData) > 0 then
  begin
    R_TMCorrectedIOP_Param1:= R_TMCorrectedIOP_Param1 + 'Param1 = ' + arrData[0][2] + XPATH_PARAM1_UNIT + ';';
  end;

  // R_TMCorrectedIOP_Param2
  R_TMCorrectedIOP_Param2:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_07);

  if Length(arrData) > 0 then
  begin
    R_TMCorrectedIOP_Param2:= R_TMCorrectedIOP_Param2 + 'Param2 = ' + arrData[0][2] + ';';
  end;

  // R_TMCorrectedIOP_CCT
  R_TMCorrectedIOP_CCT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_08);

  if Length(arrData) > 0 then
  begin
    R_TMCorrectedIOP_CCT:= R_TMCorrectedIOP_CCT + 'CCT = ' + arrData[0][2] + XPATH_CCT_UNIT;
  end;

  // R_TMCorrectedIOP_Measured_IOP_mmHg
  R_TMCorrectedIOP_Measured_IOP_mmHg:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_09);

  if Length(arrData) > 0 then
  begin
    R_TMCorrectedIOP_Measured_IOP_mmHg:= R_TMCorrectedIOP_Measured_IOP_mmHg + 'Gemessen = ' + arrData[0][2] + ' ' + XPATH_UNIT + ';';
  end;

  // R_TMCorrectedIOP_Corrected_IOP_mmHg
  R_TMCorrectedIOP_Corrected_IOP_mmHg:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_10);

  if Length(arrData) > 0 then
  begin
    R_TMCorrectedIOP_Corrected_IOP_mmHg:= R_TMCorrectedIOP_Corrected_IOP_mmHg + 'Korrigiert = ' + arrData[0][2] + ' ' + XPATH_UNIT + ';';
  end;

  // L_TMList_mmHg
  L_TMList_mmHg:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_04);

  if Length(arrData) > 0 then
  begin
    for i:= 0 to Length(arrData) - 1 do
    begin
      L_TMList_mmHg:= L_TMList_mmHg + arrData[i][2] + ' ';
    end;
    
    L_TMList_mmHg:= Trim(L_TMList_mmHg);
  end;

  // L_TMAverage_mmHg
  L_TMAverage_mmHg:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_05);

  if Length(arrData) > 0 then
  begin
    L_TMAverage_mmHg:= L_TMAverage_mmHg + '[' + arrData[0][2] + ']';
  end;

  // L_TMCorrectedIOP_Param1
  L_TMCorrectedIOP_Param1:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_11);

  if Length(arrData) > 0 then
  begin
    L_TMCorrectedIOP_Param1:= L_TMCorrectedIOP_Param1 + 'Param1 = ' + arrData[0][2] + XPATH_PARAM1_UNIT + ';';
  end;

  // L_TMCorrectedIOP_Param2
  L_TMCorrectedIOP_Param2:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_12);

  if Length(arrData) > 0 then
  begin
    L_TMCorrectedIOP_Param2:= L_TMCorrectedIOP_Param2 + 'Param2 = ' + arrData[0][2] + ';';
  end;

  // L_TMCorrectedIOP_CCT
  L_TMCorrectedIOP_CCT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_13);

  if Length(arrData) > 0 then
  begin
    L_TMCorrectedIOP_CCT:= L_TMCorrectedIOP_CCT + 'CCT = ' + arrData[0][2] + XPATH_CCT_UNIT;
  end;

  // L_TMCorrectedIOP_Measured_IOP_mmHg
  L_TMCorrectedIOP_Measured_IOP_mmHg:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_14);

  if Length(arrData) > 0 then
  begin
    L_TMCorrectedIOP_Measured_IOP_mmHg:= L_TMCorrectedIOP_Measured_IOP_mmHg + 'Gemessen = ' + arrData[0][2] + ' ' + XPATH_UNIT + ';';
  end;

  // L_TMCorrectedIOP_Corrected_IOP_mmHg
  L_TMCorrectedIOP_Corrected_IOP_mmHg:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_15);

  if Length(arrData) > 0 then
  begin
    L_TMCorrectedIOP_Corrected_IOP_mmHg:= L_TMCorrectedIOP_Corrected_IOP_mmHg + 'Korrigiert = ' + arrData[0][2] + ' ' + XPATH_UNIT + ';';
  end;

  // Build result
  ParsedData:= '';

  // Add patient ID
  if PatientID <> '' then
  begin
    ParsedData:= ParsedData + PatientID;
  end;

  // Add right eye (1)
  R_Line:= '';

  if R_TMCorrectedIOP_Param1 <> '' then
  begin
    R_Line:= R_Line + 'PR: ' + R_TMCorrectedIOP_Param1;
  end;

  if R_TMCorrectedIOP_Param2 <> '' then
  begin
    if R_Line <> '' then
      R_Line:= R_Line + ' '
    else
      R_Line:= R_Line + 'PR: ';
    
    R_Line:= R_Line + R_TMCorrectedIOP_Param2;
  end;

  if R_TMCorrectedIOP_CCT <> '' then
  begin
    if R_Line <> '' then
      R_Line:= R_Line + ' '
    else
      R_Line:= R_Line + 'PR: ';
    
    R_Line:= R_Line + R_TMCorrectedIOP_CCT;
  end;

  if R_Line <> '' then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_RESULT + FOutputLineSeparator;
    ParsedData:= ParsedData + R_Line + FOutputLineSeparator;
  end;

  // Add left eye (1)
  L_Line:= '';

  if L_TMCorrectedIOP_Param1 <> '' then
  begin
    L_Line:= L_Line + 'PL: ' + L_TMCorrectedIOP_Param1;
  end;

  if L_TMCorrectedIOP_Param2 <> '' then
  begin
    if L_Line <> '' then
      L_Line:= L_Line + ' '
    else
      L_Line:= L_Line + 'PL: ';
    
    L_Line:= L_Line + L_TMCorrectedIOP_Param2;
  end;

  if L_TMCorrectedIOP_CCT <> '' then
  begin
    if L_Line <> '' then
      L_Line:= L_Line + ' '
    else
      L_Line:= L_Line + 'PL: ';
    
    L_Line:= L_Line + L_TMCorrectedIOP_CCT;
  end;

  if L_Line <> '' then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FOREIGN_RESULT + FOutputLineSeparator;
    ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
  end;

  // Add right eye (2)
  R_Line:= '';

  if R_TMCorrectedIOP_Measured_IOP_mmHg <> '' then
  begin
    R_Line:= R_Line + 'PR: ' + R_TMCorrectedIOP_Measured_IOP_mmHg;
  end;

  if R_TMCorrectedIOP_Corrected_IOP_mmHg <> '' then
  begin
    if R_Line <> '' then
      R_Line:= R_Line + ' '
    else
      R_Line:= R_Line + 'PR: ';
    
    R_Line:= R_Line + R_TMCorrectedIOP_Corrected_IOP_mmHg;
  end;

  if R_Line <> '' then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_COMMENT + FOutputLineSeparator;
    ParsedData:= ParsedData + R_Line + FOutputLineSeparator;
  end;

  // Add left eye (2)
  L_Line:= '';

  if L_TMCorrectedIOP_Measured_IOP_mmHg <> '' then
  begin
    L_Line:= L_Line + 'PL: ' + L_TMCorrectedIOP_Measured_IOP_mmHg;
  end;

  if L_TMCorrectedIOP_Corrected_IOP_mmHg <> '' then
  begin
    if L_Line <> '' then
      L_Line:= L_Line + ' '
    else
      L_Line:= L_Line + 'PL: ';
    
    L_Line:= L_Line + L_TMCorrectedIOP_Corrected_IOP_mmHg;
  end;

  if L_Line <> '' then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_COMMENT + FOutputLineSeparator;
    ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
  end;

  // Add right eye (3)
  R_Line:= '';

  if R_TMList_mmHg <> '' then
  begin
    R_Line:= R_Line + 'R = ' + R_TMList_mmHg;
  end;

  if R_TMAverage_mmHg <> '' then
  begin
    if R_Line <> '' then
      R_Line:= R_Line + ' '
    else
      R_Line:= R_Line + 'R = ';
    
    R_Line:= R_Line + R_TMAverage_mmHg;
  end;

  // Add left eye (3)
  L_Line:= '';

  if L_TMList_mmHg <> '' then
  begin
    L_Line:= L_Line + '// L = ' + L_TMList_mmHg;
  end;

  if L_TMAverage_mmHg <> '' then
  begin
    if L_Line <> '' then
      L_Line:= L_Line + ' '
    else
      L_Line:= L_Line + '// L = ';
    
    L_Line:= L_Line + L_TMAverage_mmHg;
  end;

  if R_Line <> '' then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + R_Line;
  end;

  if L_Line <> '' then
  begin
    if R_Line = '' then
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator
    else
      ParsedData:= ParsedData + ' ';
    
    ParsedData:= ParsedData + L_Line;
  end;

  if (R_Line <> '') or (L_Line <> '') then
    ParsedData:= ParsedData + ' ' + XPATH_UNIT + ' ' + GetCurrentTime(False);

  // Add special line P0
  if bolAddSpecialLineP0ToGdtFile then
  begin
    R_TMCorrectedIOP_Param1:= '';
    
    SetLength(arrData, 0);
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_06);
    
    if Length(arrData) > 0 then
    begin
      R_TMCorrectedIOP_Param1:= R_TMCorrectedIOP_Param1 + arrData[0][2];
    end;
    
    R_Line:= '';
    
    if R_TMCorrectedIOP_Param1 <> '' then
    begin
      R_Line:= R_Line + 'R_HHD.:' + R_TMCorrectedIOP_Param1;

      if bolAddSpecialLineP0UnitToGdtFile then
        R_Line:= R_Line + XPATH_PARAM1_UNIT;
    end;
    
    L_TMCorrectedIOP_Param1:= '';
    
    SetLength(arrData, 0);
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_11);
    
    if Length(arrData) > 0 then
    begin
      L_TMCorrectedIOP_Param1:= L_TMCorrectedIOP_Param1 + arrData[0][2];
    end;
    
    L_Line:= '';

    if L_TMCorrectedIOP_Param1 <> '' then
    begin
      L_Line:= L_Line + 'L_HHD.:' + L_TMCorrectedIOP_Param1;

      if bolAddSpecialLineP0UnitToGdtFile then
        L_Line:= L_Line + XPATH_PARAM1_UNIT;
    end;

    if (R_Line <> '') or (L_Line <> '') then
    begin
      ParsedData:= ParsedData + FOutputLineSeparator;
      
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_SIGNATURE + FOutputLineSeparator;
      
      if R_Line <> '' then
        ParsedData:= ParsedData + R_Line;
      
      if L_Line <> '' then
      begin
        if R_Line <> '' then
          ParsedData:= ParsedData + ' ';
        
        ParsedData:= ParsedData + L_Line;
      end;
      
      ParsedData:= ParsedData + ' / Zeit:' + GetCurrentTime(False);
    end;
  end;

  // Set output
  FParsedDataString:= RawByteString(ParsedData);
end.
