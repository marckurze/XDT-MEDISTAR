const
  VERSION = '1.0.22.54';
  DATE = '02.07.2020 11:29:41';
  TEXT = 'Copyright (c) 2021 team2work GmbH';

  XPATH_EXPRESSION_00 = '//Measurement';

  XPATH_EXPRESSION_01 = '//Measurement/*[name()="PatientID"]';
 
  XPATH_EXPRESSION_02 = '//Measurement/Eye[@type=''OD'']/IOPAvg';
  XPATH_EXPRESSION_03 = '//Measurement/Eye[@type=''OD'']/CCT';
  XPATH_EXPRESSION_04 = '//Measurement/Eye[@type=''OD'']/CIOP';
  XPATH_EXPRESSION_05 = '//Measurement/Eye[@type=''OD'']/IOP';

  XPATH_EXPRESSION_06 = '//Measurement/Eye[@type=''OS'']/IOPAvg';
  XPATH_EXPRESSION_07 = '//Measurement/Eye[@type=''OS'']/CCT';
  XPATH_EXPRESSION_08 = '//Measurement/Eye[@type=''OS'']/CIOP';
  XPATH_EXPRESSION_09 = '//Measurement/Eye[@type=''OS'']/IOP';  

  XPATH_UNIT = 'mmHg'; // iop_unit
  XPATH_CCT_UNIT = 'mm'; // cct_unit

  GDT_FID_PATIENT_ID = '3000';

  GDT_FID_MEASURE_DATA = '6228';
  GDT_FID_RESULT = '6220';
  GDT_FID_FOREIGN_RESULT = '6221';

var
  arrData: TStringArrayArray;
  ParsedData, PatientID: String;
  R_Line, L_Line, iop_unit, cct_unit: String;
  i, MeasurementIopCount: Integer;    
  R_TMList_mmHg, R_TMAverage_mmHg, R_TMCorrectedIOP_CCT, R_TMCorrectedIOP_Corrected_IOP_mmHg: String;
  L_TMList_mmHg, L_TMAverage_mmHg, L_TMCorrectedIOP_CCT, L_TMCorrectedIOP_Corrected_IOP_mmHg: String;
begin
  // Setzen Sie hier die Anzahl der IOP Messungen, welche exportiert werden sollen.
  // Es werden Anzahl x Messungen plus der Durchschnittswert per GDT exportiert.
  // Die Anzahl muss zwischen 1 und 99 liegen.
  MeasurementIopCount:= 3;
              
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
   
  // get measurement units
  iop_unit:= '';
  cct_unit:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_00);  
  
  if Length(arrData) > 0 then
  begin
    try
      if (arrData[0][3] = 'cct_unit') then
      begin
        cct_unit:= arrData[0][4];
      end;
  
      if (arrData[0][5] = 'iop_unit') then
      begin
        iop_unit:= arrData[0][6];
      end;      
    except
      iop_unit:= '';
      cct_unit:= '';    
    end;
  end;  
  
  if (iop_unit = '') then
  begin
    iop_unit:= XPATH_UNIT;
  end;
 
  if (cct_unit = '') then
  begin
    cct_unit:= XPATH_CCT_UNIT;
  end; 
 
  // Patient ID
  PatientID:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_01);

  if Length(arrData) > 0 then
  begin
    if (arrData[0][2] <> 'TOP-1000') then
    begin
      PatientID:= PatientID + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
      PatientID:= PatientID + arrData[0][2] + FOutputLineSeparator;    
    end;
  end;
 
  // --------------------------------------------------------------------
  
  // OD oculus dexter (right eye) 
  
  // R_TMAverage_mmHg (IOPAvg)
  R_TMAverage_mmHg:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_02);

  if Length(arrData) > 0 then
  begin
    R_TMAverage_mmHg:= R_TMAverage_mmHg + '[' + arrData[0][2] + ']';
  end;
   
  // R_TMCorrectedIOP_CCT (CCT)
  R_TMCorrectedIOP_CCT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_03);

  if Length(arrData) > 0 then
  begin
    R_TMCorrectedIOP_CCT:= R_TMCorrectedIOP_CCT + 'CCT = ' + arrData[0][2] + cct_unit;
  end; 
  
  // R_TMCorrectedIOP_Corrected_IOP_mmHg (CIOP)
  R_TMCorrectedIOP_Corrected_IOP_mmHg:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_04);

  if Length(arrData) > 0 then
  begin
    R_TMCorrectedIOP_Corrected_IOP_mmHg:= R_TMCorrectedIOP_Corrected_IOP_mmHg + 'CIOP = ' + arrData[0][2];
  end; 
  
  // R_TMList_mmHg
  R_TMList_mmHg:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_05);

  if Length(arrData) > 0 then
  begin
    for i:= 0 to Length(arrData) - 1 do
    begin
      if (i >= MeasurementIopCount) then
      begin
        Break;
      end;
      
      R_TMList_mmHg:= R_TMList_mmHg + ' ' + arrData[i][2]; 
      
      // quality = arrData[i][4]
    end;
  end;

  R_TMList_mmHg:= Trim(R_TMList_mmHg);

  // --------------------------------------------------------------------
  
  // OS oculus sinister (left eye) 
  
  // L_TMAverage_mmHg (IOPAvg)
  L_TMAverage_mmHg:= ''; 

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_06);

  if Length(arrData) > 0 then
  begin
    L_TMAverage_mmHg:= L_TMAverage_mmHg + '[' + arrData[0][2] + ']';
  end;  
  
  // L_TMCorrectedIOP_CCT (CCT)
  L_TMCorrectedIOP_CCT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_07);

  if Length(arrData) > 0 then
  begin
    L_TMCorrectedIOP_CCT:= L_TMCorrectedIOP_CCT + 'CCT = ' + arrData[0][2] + cct_unit;
  end;  
  
  // L_TMCorrectedIOP_Corrected_IOP_mmHg (CIOP)
  L_TMCorrectedIOP_Corrected_IOP_mmHg:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_08);

  if Length(arrData) > 0 then
  begin
    L_TMCorrectedIOP_Corrected_IOP_mmHg:= L_TMCorrectedIOP_Corrected_IOP_mmHg + 'CIOP = ' + arrData[0][2];
  end;
  
  // L_TMList_mmHg
  L_TMList_mmHg:= '';  
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_09);  
  
  if Length(arrData) > 0 then
  begin
    for i:= 0 to Length(arrData) - 1 do
    begin
      if (i >= MeasurementIopCount) then
      begin
        Break;
      end;    
    
      L_TMList_mmHg:= L_TMList_mmHg + ' ' + arrData[i][2]; 
      
      // quality = arrData[i][4]
    end;
  end;

  L_TMList_mmHg:= Trim(L_TMList_mmHg);
  
  // --------------------------------------------------------------------
  
  // Build result
  ParsedData:= '';

  // Add patient ID
  if (PatientID <> '') then
  begin
    ParsedData:= ParsedData + PatientID;
  end;

  // Add right eye (1)
  R_Line:= '';

  if (R_TMCorrectedIOP_Corrected_IOP_mmHg <> '') then
  begin
    if (R_Line <> '') then
      R_Line:= R_Line + ' '
    else
      R_Line:= R_Line + 'PR: ';
    
    R_Line:= R_Line + R_TMCorrectedIOP_Corrected_IOP_mmHg;
  end;

  if (R_TMCorrectedIOP_CCT <> '') then
  begin
    if (R_Line <> '') then
      R_Line:= R_Line + ' '
    else
      R_Line:= R_Line + 'PR: ';
    
    R_Line:= R_Line + R_TMCorrectedIOP_CCT;
  end;

  if (R_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_RESULT + FOutputLineSeparator;
    ParsedData:= ParsedData + R_Line + FOutputLineSeparator;
  end;

  // Add left eye (1)
  L_Line:= '';

  if (L_TMCorrectedIOP_Corrected_IOP_mmHg <> '') then
  begin
    if (L_Line <> '') then
      L_Line:= L_Line + ' '
    else
      L_Line:= L_Line + 'PL: ';
    
    L_Line:= L_Line + L_TMCorrectedIOP_Corrected_IOP_mmHg;
  end;

  if (L_TMCorrectedIOP_CCT <> '') then
  begin
    if (L_Line <> '') then
      L_Line:= L_Line + ' '
    else
      L_Line:= L_Line + 'PL: ';
    
    L_Line:= L_Line + L_TMCorrectedIOP_CCT;
  end;

  if (L_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FOREIGN_RESULT + FOutputLineSeparator;
    ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
  end;

  // -------------------------------------------------------------------- 

  // Add right eye (2)
  R_Line:= '';

  if (R_TMList_mmHg <> '') then
  begin
    R_Line:= R_Line + 'R = ' + R_TMList_mmHg;
  end;

  if (R_TMAverage_mmHg <> '') then
  begin
    if (R_Line <> '') then
      R_Line:= R_Line + ' '
    else
      R_Line:= R_Line + 'R = ';
    
    R_Line:= R_Line + R_TMAverage_mmHg;
  end;

  // Add left eye (2)
  L_Line:= '';

  if (L_TMList_mmHg <> '') then
  begin
    L_Line:= L_Line + '// L = ' + L_TMList_mmHg;
  end;

  if (L_TMAverage_mmHg <> '') then
  begin
    if (L_Line <> '') then
      L_Line:= L_Line + ' '
    else
      L_Line:= L_Line + '// L = ';
    
    L_Line:= L_Line + L_TMAverage_mmHg;
  end;

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
    ParsedData:= ParsedData + ' ' + iop_unit + ' ' + GetCurrentTime(False);
  end;  

  // Set output
  FParsedDataString:= RawByteString(ParsedData);
end.