const
  VERSION = '1.0.23.40';
  DATE = '04.10.2022 14:49:08';
  TEXT = 'Copyright (c) 2022 team2work GmbH';

  XPATH_EXPRESSION_01 = '//Ophthalmology/*[name()=''nsCommon:Common'']/*[name()=''nsCommon:Patient'']/*[name()=''nsCommon:ID'']';
  
  XPATH_EXPRESSION_02 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:R'']/*[name()=''nsLM:Sphere'']';
  XPATH_EXPRESSION_03 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:R'']/*[name()=''nsLM:Cylinder'']';
  XPATH_EXPRESSION_04 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:R'']/*[name()=''nsLM:Axis'']';
  XPATH_EXPRESSION_05 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:R'']/*[name()=''nsLM:ADD'']';
  XPATH_EXPRESSION_06 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:R'']/*[name()=''nsLM:ADD2'']';
  XPATH_EXPRESSION_07 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:R'']/*[name()=''nsLM:PrismX'']';
  XPATH_EXPRESSION_08 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:R'']/*[name()=''nsLM:PrismY'']';
  
  XPATH_EXPRESSION_09 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:L'']/*[name()=''nsLM:Sphere'']';
  XPATH_EXPRESSION_10 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:L'']/*[name()=''nsLM:Cylinder'']';
  XPATH_EXPRESSION_11 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:L'']/*[name()=''nsLM:Axis'']';
  XPATH_EXPRESSION_12 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:L'']/*[name()=''nsLM:ADD'']';
  XPATH_EXPRESSION_13 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:L'']/*[name()=''nsLM:ADD2'']'; 
  XPATH_EXPRESSION_14 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:L'']/*[name()=''nsLM:PrismX'']';
  XPATH_EXPRESSION_15 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:L'']/*[name()=''nsLM:PrismY'']';
	
  GDT_FID_PATIENT_ID               = '3000';
  GDT_FID_MEASURE_DATA             = '6228';
  
  GDT_AXIS_MAX_CHAR_COUNT          = 3;
  GDT_AXIS_SEPARATOR               = ' ';
  
  GDT_ADD_ADD_POSITIVE_SIGN        = True;

function FormatSCValue(const Value: String): String;
var
  S1, S2: String;
begin
  Result:= Value;

  S1:= Value;
  
  if Length(S1) < 6 then
  begin
    if Length(S1) = 5 then
    begin
      S2:= S1[1];
      S1:= Copy(S1, 2, Length(S1) - 1);
      
      Result:= S2 + ' ' + S1;
    end;
  end;
end;

function FormatAxisValue(const Value: String): String;
begin
  Result:= Value;
  
  while Length(Result) < GDT_AXIS_MAX_CHAR_COUNT do
    Result:= GDT_AXIS_SEPARATOR + Result;
end;

function FormatPrismValue(const Value, PrismBase: String): String;
var
  S1, S2: String;
begin
  Result:= '';

  S1:= Value;
  S2:= '';
  
  if PrismBase = 'in' then
  begin
    S2:= 'I';
  end
  else if PrismBase = 'out' then
  begin
    S2:= 'O';
  end 
  else if PrismBase = 'up' then
  begin
    S2:= 'U';
  end   
  else if PrismBase = 'down' then
  begin
    S2:= 'D';
  end;     
    
  Result:= S1 + ' ' + S2;    
end;

function FormatAddValue(const Value: String; AddPositiveSign: Boolean): String;
begin
  Result:= Value;
  
  if (AddPositiveSign) then
    Result:= '+ ' + Result;
end;

var
  arrData: TStringArrayArray;
  ParsedData, PatientID: String;  
  R_S, R_C, R_Axis, R_Add1, R_Add2, R_Prism_H, R_Prism_V: String;
  L_S, L_C, L_Axis, L_Add1, L_Add2, L_Prism_H, L_Prism_V: String;
  R_Line, L_Line: String;

begin
  
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
  
  // R_S
  R_S := '';
	
  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_02);
	
  if Length(arrData) > 0 then
  begin
    R_S := R_S + 'S=' + FormatSCValue(arrData[0][2]);
  end;	
	
  // R_C
  R_C := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_03);

  if Length(arrData) > 0 then
  begin
    R_C := R_C + 'Z=' + FormatSCValue(arrData[0][2]);
  end;	
  
  // R_Axis
  R_Axis := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_04);

  if Length(arrData) > 0 then
  begin
    R_Axis := R_Axis + '*' + FormatAxisValue(arrData[0][2]);
  end;
  
  // R_Add1
  R_Add1 := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_05);

  if Length(arrData) > 0 then
  begin
    R_Add1 := R_Add1 + 'A=' + FormatAddValue(arrData[0][2], GDT_ADD_ADD_POSITIVE_SIGN);
  end;
  
  // R_Add2
  R_Add2 := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_06);

  if Length(arrData) > 0 then
  begin
    R_Add2 := R_Add2 + 'A2=' + FormatAddValue(arrData[0][2], GDT_ADD_ADD_POSITIVE_SIGN);
  end;
	
  // R_Prism_H
  R_Prism_H := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_07);

  if Length(arrData) > 0 then
  begin
    R_Prism_H := R_Prism_H + FormatPrismValue(arrData[0][2], arrData[0][6]);
  end;
  
  // R_Prism_V 
  R_Prism_V := '';
	
  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_08);

  if Length(arrData) > 0 then
  begin
    R_Prism_V := R_Prism_V + FormatPrismValue(arrData[0][2], arrData[0][6]);
  end;
  
  // L_S
  L_S := '';
	  
  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_09);
	
  if Length(arrData) > 0 then
  begin
    L_S := L_S + 'S=' + FormatSCValue(arrData[0][2]);
  end;	
  
  // L_C
  L_C := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_10);

  if Length(arrData) > 0 then
  begin
    L_C := L_C + 'Z=' + FormatSCValue(arrData[0][2]);
  end;	
  
  // L_Axis
  L_Axis := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_11);

  if Length(arrData) > 0 then
  begin
    L_Axis := L_Axis + '*' + FormatAxisValue(arrData[0][2]);
  end;
  
  // L_Add1
  L_Add1 := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_12);

  if Length(arrData) > 0 then
  begin
    L_Add1 := L_Add1 + 'A=' + FormatAddValue(arrData[0][2], GDT_ADD_ADD_POSITIVE_SIGN);
  end;
  
  // L_Add2
  L_Add2 := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_13);

  if Length(arrData) > 0 then
  begin
    L_Add2 := L_Add2 + 'A2=' + FormatAddValue(arrData[0][2], GDT_ADD_ADD_POSITIVE_SIGN);
  end;
  
  // L_Prism_H
  L_Prism_H := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_14);

  if Length(arrData) > 0 then
  begin
    L_Prism_H := L_Prism_H + FormatPrismValue(arrData[0][2], arrData[0][6]);
  end;
  
  // L_Prism_V 
  L_Prism_V := '';
	
  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_15);

  if Length(arrData) > 0 then
  begin
    L_Prism_V := L_Prism_V + FormatPrismValue(arrData[0][2], arrData[0][6]);
  end;
  
  // Build right eye
  R_Line:= '';
  
  if (R_S <> '') or (R_C <> '') or (R_Axis <> '') or (R_Prism_H <> '') or (R_Prism_V <> '') or (R_Add1 <> '') or (R_Add2 <> '') then
  begin
    R_Line:= R_Line + 'R.:';
    
    if R_S <> '' then
      R_Line:= R_Line + R_S + ' ';
    
    if R_C <> '' then
      R_Line:= R_Line + R_C + ' ';
    
    if R_Axis <> '' then
      R_Line:= R_Line + R_Axis + ' ';
    
    if (R_Prism_H <> '') or (R_Prism_V <> '') then
      R_Line:= R_Line + 'P= ';
    
    if R_Prism_H <> '' then
      R_Line:= R_Line + R_Prism_H + ' ';
    
    if R_Prism_V <> '' then
      R_Line:= R_Line + R_Prism_V + ' ';
    
    if R_Add1 <> '' then
      R_Line:= R_Line + R_Add1 + ' ';
    
    if R_Add2 <> '' then
      R_Line:= R_Line + R_Add2 + ' ';
    
    R_Line:= Trim(R_Line);
  end;
  
  // Build left eye
  L_Line:= '';
  
  if (L_S <> '') or (L_C <> '') or (L_Axis <> '') or (L_Prism_H <> '') or (L_Prism_V <> '') or (L_Add1 <> '') or (L_Add2 <> '') then
  begin
    L_Line:= L_Line + 'L.:';
    
    if L_S <> '' then
      L_Line:= L_Line + L_S + ' ';
    
    if L_C <> '' then
      L_Line:= L_Line + L_C + ' ';
    
    if L_Axis <> '' then
      L_Line:= L_Line + L_Axis + ' ';
    
    if (L_Prism_H <> '') or (L_Prism_V <> '') then
      L_Line:= L_Line + 'P= ';
    
    if L_Prism_H <> '' then
      L_Line:= L_Line + L_Prism_H + ' ';
    
    if L_Prism_V <> '' then
      L_Line:= L_Line + L_Prism_V + ' ';
    
    if L_Add1 <> '' then
      L_Line:= L_Line + L_Add1 + ' ';
    
    if L_Add2 <> '' then
      L_Line:= L_Line + L_Add2 + ' ';
    
    L_Line:= Trim(L_Line);
  end;
  
  // Build result
  ParsedData := '';

  // Add patient ID
  if PatientID <> '' then
  begin
    ParsedData := ParsedData + PatientID;
  end;
  
  // Add right eye
  if R_Line <> '' then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + R_Line + FOutputLineSeparator;
  end;
  
  // Add left eye
  if L_Line <> '' then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
  end;
		
  // Set output
  FParsedDataString := RawByteString(ParsedData);
end.
