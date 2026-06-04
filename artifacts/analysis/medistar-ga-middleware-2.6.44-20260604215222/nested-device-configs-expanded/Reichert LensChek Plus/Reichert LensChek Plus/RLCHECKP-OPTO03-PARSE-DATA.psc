const
  VERSION = '1.0.4.37';
  DATE = '30.01.2020 14:06:17';
  TEXT = 'Copyright (c) 2020 team2work GmbH';

  XPATH_EXPRESSION_01 = '//ReichertData-XML/Header/PatientID';
  
  XPATH_EXPRESSION_02 = '//ReichertData-XML/DataSet/LM_Data/OD/Spectacle_Data/Sphere';
  XPATH_EXPRESSION_03 = '//ReichertData-XML/DataSet/LM_Data/OD/Spectacle_Data/Cylinder'; 
  XPATH_EXPRESSION_04 = '//ReichertData-XML/DataSet/LM_Data/OD/Spectacle_Data/Axis'; 
  
  XPATH_EXPRESSION_05 = '//ReichertData-XML/DataSet/LM_Data/OD/Prism_Data/Vert_Prism';
  XPATH_EXPRESSION_06 = '//ReichertData-XML/DataSet/LM_Data/OD/Prism_Data/Vert_PrismBase';  
  XPATH_EXPRESSION_07 = '//ReichertData-XML/DataSet/LM_Data/OD/Prism_Data/Horz_Prism';
  XPATH_EXPRESSION_08 = '//ReichertData-XML/DataSet/LM_Data/OD/Prism_Data/Horz_PrismBase';

  XPATH_EXPRESSION_09 = '//ReichertData-XML/DataSet/LM_Data/OS/Spectacle_Data/Sphere';
  XPATH_EXPRESSION_10 = '//ReichertData-XML/DataSet/LM_Data/OS/Spectacle_Data/Cylinder'; 
  XPATH_EXPRESSION_11 = '//ReichertData-XML/DataSet/LM_Data/OS/Spectacle_Data/Axis'; 

  XPATH_EXPRESSION_12 = '//ReichertData-XML/DataSet/LM_Data/OS/Prism_Data/Vert_Prism';
  XPATH_EXPRESSION_13 = '//ReichertData-XML/DataSet/LM_Data/OS/Prism_Data/Vert_PrismBase';  
  XPATH_EXPRESSION_14 = '//ReichertData-XML/DataSet/LM_Data/OS/Prism_Data/Horz_Prism';
  XPATH_EXPRESSION_15 = '//ReichertData-XML/DataSet/LM_Data/OS/Prism_Data/Horz_PrismBase';

  GDT_FID_PATIENT_ID = '3000';
  GDT_FID_MEASURE_DATA = '6228';

var
  arrData: TStringArrayArray;
  ParsedData, PatientID: String;  
  R_S, R_C, R_Axis, R_Prism, R_Vert_Prism, R_Vert_PrismBase, R_Horz_Prism, R_Horz_PrismBase: String;
  L_S, L_C, L_Axis, L_Prism, L_Vert_Prism, L_Vert_PrismBase, L_Horz_Prism, L_Horz_PrismBase: String;
  R_Line, L_Line: String;

begin
  // Script to parse xml data which are send from the Reichert Capture Software
  
  // Clear parsed data string
  FParsedDataString := '';

  if (not DoPSXMLDocumentExists()) then
  begin
    FLastErrorCode := -4;
    FLastErrorMessage := 'Keine XML-Daten für die Verarbeitung verfügbar';

    DoPSError();

    Exit;
  end;

  if (not DoPSXMLRootNodeExists()) then
  begin
    FLastErrorCode := -5;
    FLastErrorMessage := 'Kein XML-Wurzelknoten für die Verarbeitung verfügbar';

    DoPSError();

    Exit;
  end;
	
  // Patient ID
  PatientID := '';
	
  SetLength(arrData, 0);    
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_01);
	
  if Length(arrData) > 0 then
  begin
    PatientID := PatientID + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
    PatientID := PatientID + arrData[0][2] + FOutputLineSeparator;
  end;
  
  // The first step to understanding your eyeglass prescription is knowing what "OD" and OS" mean. 
  // They are abbreviations for *oculus dexter* and *oculus sinister*, 
  // which are Latin terms for right eye and left eye.
	
  // "OD" -> right eye - *oculus dexter* 
  
  // R_S
  R_S := '';
	
  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_02);
	
  if Length(arrData) > 0 then
  begin
    R_S := R_S + 'S=' + arrData[0][2];
  end;	
	
  // R_C
  R_C := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_03);

  if Length(arrData) > 0 then
  begin
    R_C := R_C + 'Z=' + arrData[0][2];
  end;	
  
  // R_Axis
  R_Axis := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_04);

  if Length(arrData) > 0 then
  begin
    R_Axis := R_Axis + '*' + arrData[0][2];
  end;
	
  // R_Vert_Prism
  R_Vert_Prism := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_05);

  if Length(arrData) > 0 then
  begin
    R_Vert_Prism := R_Vert_Prism + arrData[0][2];
  end;
  
  // R_Vert_PrismBase 
  R_Vert_PrismBase := '';
	
  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_06);

  if Length(arrData) > 0 then
  begin
    R_Vert_PrismBase := R_Vert_PrismBase + arrData[0][2];
  end;	
  
  // R_Horz_Prism
  R_Horz_Prism := '';
  
  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_07);

  if Length(arrData) > 0 then
  begin
    R_Horz_Prism := R_Horz_Prism + arrData[0][2];
  end;	 
  
  // R_Horz_PrismBase
  R_Horz_PrismBase := '';
  
  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_08);

  if Length(arrData) > 0 then
  begin
    R_Horz_PrismBase := R_Horz_PrismBase + arrData[0][2];
  end;	
  
  // Build right prism string   

  // R_Prism
  R_Prism := 'P= ' + R_Horz_Prism + ' ' + R_Horz_PrismBase + ' ' + R_Vert_Prism + ' ' + R_Vert_PrismBase;	

  if (Trim(R_Prism) = 'P=') then
  begin
    R_Prism := '';
  end;

  // "OS" -> left eye - *oculus sinister*
  
  // L_S
  L_S := '';
	  
  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_09);
	
  if Length(arrData) > 0 then
  begin
    L_S := L_S + 'S=' + arrData[0][2];
  end;	
  
  // L_C
  L_C := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_10);

  if Length(arrData) > 0 then
  begin
    L_C := L_C + 'Z=' + arrData[0][2];
  end;	
  
  // L_Axis
  L_Axis := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_11);

  if Length(arrData) > 0 then
  begin
    L_Axis := L_Axis + '*' + arrData[0][2];
  end;
  
  // L_Vert_Prism
  L_Vert_Prism := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_12);

  if Length(arrData) > 0 then
  begin
    L_Vert_Prism := L_Vert_Prism + arrData[0][2];
  end;
  
  // L_Vert_PrismBase 
  L_Vert_PrismBase := '';
	
  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_13);

  if Length(arrData) > 0 then
  begin
    L_Vert_PrismBase := L_Vert_PrismBase + arrData[0][2];
  end;	
  
  // L_Horz_Prism
  L_Horz_Prism := '';
  
  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_14);

  if Length(arrData) > 0 then
  begin
    L_Horz_Prism := L_Horz_Prism + arrData[0][2];
  end;	            
   
  // L_Horz_PrismBase
  L_Horz_PrismBase := '';
  
  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_15);

  if Length(arrData) > 0 then
  begin
    L_Horz_PrismBase := L_Horz_PrismBase + arrData[0][2];
  end;		

  // Build left prism string  	

  // L_Prism
  L_Prism := 'P= ' + L_Horz_Prism + ' ' + L_Horz_PrismBase + ' ' + L_Vert_Prism + ' ' + L_Vert_PrismBase;	
	
  if (Trim(L_Prism) = 'P=') then
  begin
    L_Prism := '';
  end;	
	
  // Build result
  ParsedData := '';

  // Add patient ID
  if (PatientID <> '') then
  begin
    ParsedData := ParsedData + PatientID;
  end;
  
  // Add right eye
  R_Line:= '';
	
  if (R_S <> '') and (R_C <> '') and (R_Axis <> '') then
  begin
    R_Line := R_Line + 'R.:' + R_S + ' ' + R_C + R_Axis;
  end;
  
  // Add right prism
  if (R_Prism <> '') then
  begin
    if (R_Line <> '') then
    begin
      R_Line := R_Line + ' ';
    end;
		
    R_Line := R_Line + R_Prism;
  end;  
  
  if (R_Line <> '') then
  begin
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData := ParsedData + R_Line + FOutputLineSeparator;
  end;
	  
  // Add left eye
  L_Line := '';  
  
  if (L_S <> '') and (L_C <> '') and (L_Axis <> '') then
  begin
    L_Line := L_Line + 'L.:' + L_S + ' ' + L_C + L_Axis;
  end;  

  // Add left prism
  if (L_Prism <> '') then
  begin
    if (L_Line <> '') then
    begin
      L_Line := L_Line + ' ';
    end;
		
    L_Line := L_Line + L_Prism;
  end;  	
	
  if (L_Line <> '') then
  begin
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData := ParsedData + L_Line + FOutputLineSeparator;
  end;	
		
  // Set output
  FParsedDataString := RawByteString(ParsedData);
end.