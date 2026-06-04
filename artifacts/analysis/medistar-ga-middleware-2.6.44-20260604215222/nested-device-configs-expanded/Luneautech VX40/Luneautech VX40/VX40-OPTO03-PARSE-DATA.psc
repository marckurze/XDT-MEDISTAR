const
  VERSION = '1.0.14.53';
  DATE = '14.10.2020 09:58:12';
  TEXT = 'Copyright (c) 2020 team2work GmbH';

  XPATH_EXPRESSION_01 = '//optic/optometry/patient/ID';
  
  XPATH_EXPRESSION_02 = '//optic/optometry/LSM_mesurement/measure_REF/ref_right/sphere';
  XPATH_EXPRESSION_03 = '//optic/optometry/LSM_mesurement/measure_REF/ref_right/cylinder';
  XPATH_EXPRESSION_04 = '//optic/optometry/LSM_mesurement/measure_REF/ref_right/axis';
  XPATH_EXPRESSION_05 = '//optic/optometry/LSM_mesurement/measure_REF/ref_right/addition';
  
  XPATH_EXPRESSION_06 = '//optic/optometry/LSM_mesurement/measure_REF/ref_right/prism/polar/resulting_prism';
  XPATH_EXPRESSION_07 = '//optic/optometry/LSM_mesurement/measure_REF/ref_right/prism/polar/base';

  XPATH_EXPRESSION_08 = '//optic/optometry/LSM_mesurement/measure_REF/ref_left/sphere';
  XPATH_EXPRESSION_09 = '//optic/optometry/LSM_mesurement/measure_REF/ref_left/cylinder';
  XPATH_EXPRESSION_10 = '//optic/optometry/LSM_mesurement/measure_REF/ref_left/axis';
  XPATH_EXPRESSION_11 = '//optic/optometry/LSM_mesurement/measure_REF/ref_left/addition';
  
  XPATH_EXPRESSION_12 = '//optic/optometry/LSM_mesurement/measure_REF/ref_left/prism/polar/resulting_prism';
  XPATH_EXPRESSION_13 = '//optic/optometry/LSM_mesurement/measure_REF/ref_left/prism/polar/base';

  GDT_FID_PATIENT_ID = '3000';
  GDT_FID_MEASURE_DATA = '6228';
  
  GDT_LINE_PREFIX = '  ';
  GDT_SIGN_SEPARATOR = ' ';
  GDT_AXIS_SEPARATOR = ' ';
  GDT_AXIS_SEPARATOR_MAX_COUNT = 3;
  GDT_A_PREFIX = ' ';
  GDT_A_PREFIX_COUNT = 8;

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

function AddAPrefix(const Value: String; AddPrefix: Boolean): String;
var
  i: Integer;
begin
  Result:= Value;
  
  if AddPrefix then
    for i:= 0 to GDT_A_PREFIX_COUNT - 1 do
      Result:= GDT_A_PREFIX + Result;
end;

var
  arrData: TStringArrayArray;
  ParsedData, PatientID: String;  
  R_S, R_C, R_Axis, R_Addition, R_Prism, R_PrismResultingPrism, R_PrismBase: String;
  L_S, L_C, L_Axis, L_Addition, L_Prism, L_PrismResultingPrism, L_PrismBase: String;
  R_Line, L_Line: String;
  bolAddAValueToOutputIfEmpty: Boolean;
  bolAddGDTLinePrefix, bolAddSign, bolAddSignSeparator, bolAddAxisSeparator, bolAddAPrefix, bolAddPrism: Boolean;

begin
  // Script to parse xml data which is send from the Luneautech VX40 device

  // Verwende "True", damit der A Wert an die V0 Zeile angehängt wird,
  // auch wenn kein Wert in der Messung vorhanden ist
  // In diesem Fall wird der A Wert mit "A=" eingetragen
  // benutze "False", damit der A Wert nur dann an die V0 Zeile angehängt 
  // wird, wenn auch tatsächlich ein Wert in der messung vorhanden ist
  bolAddAValueToOutputIfEmpty := False;

  // Verwende "True", damit jeder GDT Zeile das Prefix aus GDT_LINE_PREFIX 
  // vorangestellt wird,
  // benutze "False", damit das GDT-Zeilen-Prefix ignoriert wird
  bolAddGDTLinePrefix:= False;

  // Verwende "True", damit immer ein Vorzeichen hinzugefügt wird, 
  // benutze "False", damit nur der gemessene Wert eingetragen wird wie er 
  // vom Gerät kommt 
  bolAddSign:= True;

  // Verwende "True", damit nach jedem Vorzeichen der Wert aus GDT_SIGN_SEPARATOR 
  // angefügt wird,
  // benutze "False", damit kein Abstand zwischen Vorzeichen und Wert eingefügt wird
  bolAddSignSeparator:= True;

  // Verwende "True", damit der Achsenseparator angefügt wird,
  // benutze "False", damit der Achsenseparator nicht verwendet wird
  bolAddAxisSeparator:= True;

  // Verwende "True", damit das A-Prefix angefügt wird,
  // benutze "False", damit das A-Prefix nicht verwendet wird
  bolAddAPrefix:= True;

  // Verwende "True", damit das Prisma (P- und B-Werte) angefügt wird,
  // benutze "False", damit das Prisma (P- und B-Werte) nicht verwendet wird
  bolAddPrism:= True;

  // --- Don't edit script down below ---

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

  // R_S
  R_S := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_02);

  if (Length(arrData) > 0) and (Trim(arrData[0][2]) <> '') then
  begin
    R_S := R_S + FormatSignValue('S=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator); // 'S=' + arrData[0][2];
  end;

  // R_C
  R_C := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_03);

  if (Length(arrData) > 0) and (Trim(arrData[0][2]) <> '') then
  begin
    R_C := R_C + FormatSignValue('Z=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator); // 'Z=' + arrData[0][2];
  end;

  // R_Axis
  R_Axis := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_04);

  if Length(arrData) > 0 then
  begin
    R_Axis := R_Axis + FormatAxisValue('*', Trim(arrData[0][2]), bolAddAxisSeparator); // '*' + Trim(arrData[0][2]);
  end;

  // R_Addition
  R_Addition := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_05);

  if (Length(arrData) > 0) and (Trim(arrData[0][2]) <> '') then
  begin
    R_Addition := R_Addition + FormatSignValue('A=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator); // 'A=' + Trim(arrData[0][2]);
  end
  else
  begin
    if bolAddAValueToOutputIfEmpty then
    begin
      R_Addition := R_Addition + 'A=';
    end;
  end;

  if R_Addition <> '' then
    R_Addition:= AddAPrefix(R_Addition, bolAddAPrefix);

  // R_PrismResultingPrism
  R_PrismResultingPrism := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_06);

  if Length(arrData) > 0 then
  begin
    R_PrismResultingPrism := R_PrismResultingPrism + arrData[0][2];
  end;

  // R_PrismBase 
  R_PrismBase := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_07);

  if Length(arrData) > 0 then
  begin
    R_PrismBase := R_PrismBase + arrData[0][2];
  end;

  // Build right prism string   

  // R_Prism
  R_Prism := '';

  if bolAddPrism then
  begin
    if R_PrismResultingPrism <> '' then
      R_Prism := R_Prism + 'P=' + R_PrismResultingPrism;
    
    if R_PrismBase <> '' then
    begin
      if R_Prism <> '' then
        R_Prism := R_Prism + ' ';
      
      R_Prism := R_Prism + 'B=' + R_PrismBase;
    end;
  end;

  // L_S
  L_S := '';
    
  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_08);

  if (Length(arrData) > 0) and (Trim(arrData[0][2]) <> '') then
  begin
    L_S := L_S + FormatSignValue('S=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator); // 'S=' + arrData[0][2];
  end;	

  // L_C
  L_C := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_09);

  if (Length(arrData) > 0) and (Trim(arrData[0][2]) <> '') then
  begin
    L_C := L_C + FormatSignValue('Z=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator); // 'Z=' + arrData[0][2];
  end;	

  // L_Axis
  L_Axis := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_10);

  if Length(arrData) > 0 then
  begin
    L_Axis := L_Axis + FormatAxisValue('*', Trim(arrData[0][2]), bolAddAxisSeparator); // '*' + Trim(arrData[0][2]);
  end;

  // L_Addition
  L_Addition := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_11);

  if (Length(arrData) > 0) and (Trim(arrData[0][2]) <> '') then
  begin
    L_Addition := L_Addition + FormatSignValue('A=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator); // 'A=' + arrData[0][2];
  end
  else
  begin
    if bolAddAValueToOutputIfEmpty then
    begin
      L_Addition := L_Addition + 'A=';
    end;
  end;

  if L_Addition <> '' then
    L_Addition:= AddAPrefix(L_Addition, bolAddAPrefix);

  // L_PrismResultingPrism
  L_PrismResultingPrism := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_12);

  if Length(arrData) > 0 then
  begin
    L_PrismResultingPrism := L_PrismResultingPrism + arrData[0][2];
  end;

  // L_PrismBase 
  L_PrismBase := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_13);

  if Length(arrData) > 0 then
  begin
    L_PrismBase := L_PrismBase + arrData[0][2];
  end;

  // Build left prism string

  // L_Prism
  L_Prism := '';

  if bolAddPrism then
  begin
    if L_PrismResultingPrism <> '' then
      L_Prism := L_Prism + 'P=' + L_PrismResultingPrism;
    
    if L_PrismBase <> '' then
    begin
      if L_Prism <> '' then
        L_Prism := L_Prism + ' ';
      
      L_Prism := L_Prism + 'B=' + L_PrismBase;
    end;
  end;

  // Build result
  ParsedData := '';

  // Add patient ID
  if PatientID <> '' then
  begin
    ParsedData := ParsedData + PatientID;
  end;

  // Add right eye
  R_Line:= '';

  if (R_S <> '') and (R_C <> '') and (R_Axis <> '') then
  begin
    R_Line := R_Line + 'R.:' + R_S + ' ' + R_C + R_Axis;
  end;

  // Add right addition
  if R_Addition <> '' then
  begin
    if R_Line <> '' then
    begin
      R_Line := R_Line + ' ';
    end;
    
    R_Line := R_Line + R_Addition;
  end;

  // Add right prism
  if R_Prism <> '' then
  begin
    if R_Line <> '' then
    begin
      R_Line := R_Line + ' ';
    end;
    
    R_Line := R_Line + R_Prism;
  end;

  if R_Line <> '' then
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

  // Add left addition
  if L_Addition <> '' then
  begin
    if L_Line <> '' then
    begin
      L_Line := L_Line + ' ';
    end;
    
    L_Line := L_Line + L_Addition;
  end;

  // Add left prism
  if L_Prism <> '' then
  begin
    if L_Line <> '' then
    begin
      L_Line := L_Line + ' ';
    end;
    
    L_Line := L_Line + L_Prism;
  end;

  if L_Line <> '' then
  begin
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData := ParsedData + L_Line + FOutputLineSeparator;
  end;

  // Set output
  FParsedDataString := RawByteString(ParsedData);
end.