const
  VERSION = '1.0.55.54';
  DATE = '07.02.2023 12:58:12';
  TEXT = 'Copyright (c) 2023 team2work GmbH';

  XPATH_EXPRESSION_01 = '//optic/optometry/patient/ID';
 
  XPATH_EXPRESSION_02 = '//optic/optometry/objective_mesurement/TONO/tono_right/average';
  XPATH_EXPRESSION_03 = '//optic/optometry/objective_mesurement/TONO/tono_right/PIO/node()';
  XPATH_EXPRESSION_04 = '//optic/optometry/objective_mesurement/TONO/tono_right/PIOc/node()';

  XPATH_EXPRESSION_10 = '//optic/optometry/objective_mesurement/TONO/tono_left/average';    
  XPATH_EXPRESSION_11 = '//optic/optometry/objective_mesurement/TONO/tono_left/PIO/node()';
  XPATH_EXPRESSION_12 = '//optic/optometry/objective_mesurement/TONO/tono_left/PIOc/node()';  

  XPATH_UNIT = 'mmHg';

  GDT_FID_PATIENT_ID     = '3000';
  GDT_FID_MEASURE_DATA   = '6228';
  GDT_FID_COMMENT        = '6227';
  GDT_FID_SIGNATURE      = '8990';
  GDT_FID_RESULT         = '6220';
  GDT_FID_FOREIGN_RESULT = '6221';

var
  arrData: TStringArrayArray;
  ParsedData, PatientID: String;
  R_Line, L_Line: String;
  i: Integer; 
  R_average, R_measurement: String;
  L_average, L_measurement: String;

begin

  // --- Don't edit script down below ---

  // Clear parsed data string
  FParsedDataString:= '';

  if (not DoPSXMLDocumentExists()) then
  begin
    FLastErrorCode:= -4;
    FLastErrorMessage:= 'Keine XML-Daten für die Verarbeitung verfügbar';

    DoPSError();

    Exit;
  end;
  
  if (not DoPSXMLRootNodeExists()) then
  begin
    FLastErrorCode:= -5;
    FLastErrorMessage:= 'Kein XML-Wurzelknoten für die Verarbeitung verfügbar';

    DoPSError();

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
  
  // Right
  
  // Average
  R_average:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_02);

  if (Length(arrData) > 0) then
  begin
    R_average:= '[' + Trim(arrData[0][2]) + ']';
  end;
  
  // measurement
  R_measurement:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_03);  
  
  if Length(arrData) > 0 then
  begin
    for i:= 0 to Length(arrData) - 1 do
    begin
      R_measurement:= R_measurement + ' ' + Trim(arrData[i][2]);
    end;
  end;  
  
  R_measurement:= Trim(R_measurement);
  
  // Left
  
  // Average
  L_average:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_10);

  if (Length(arrData) > 0) then
  begin
    L_average:= '[' + Trim(arrData[0][2]) + ']';
  end;
  
  // measurement
  L_measurement:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_11);  
  
  if Length(arrData) > 0 then
  begin
    for i:= 0 to Length(arrData) - 1 do
    begin
      L_measurement:= L_measurement + ' ' + Trim(arrData[i][2]);
    end;
  end;  
  
  L_measurement:= Trim(L_measurement);   

  // Build result
  ParsedData:= '';

  // Add patient ID
  if (PatientID <> '') then
  begin
    ParsedData:= ParsedData + PatientID;
  end;
  
  R_Line:= '';

  if (R_measurement <> '') or (R_average <> '') then
  begin
    R_Line:= R_Line + 'R = ' + R_measurement + ' ' + R_average;
  end;	

  L_Line:= '';
  	
  if (L_measurement <> '') or (L_average <> '') then
  begin
    L_Line:= L_Line + ' // L = ' + L_measurement + ' ' + L_average;
  end;	  	

  if (R_Line <> '') or (L_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + R_Line + L_Line + ' ' + XPATH_UNIT + ' ' + GetCurrentTime(False) + FOutputLineSeparator;
  end;

  // Set output
  FParsedDataString := RawByteString(ParsedData);
end.