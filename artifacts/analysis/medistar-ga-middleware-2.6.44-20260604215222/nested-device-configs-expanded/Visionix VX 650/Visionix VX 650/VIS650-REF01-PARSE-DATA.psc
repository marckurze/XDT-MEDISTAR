const
  VERSION = '1.0.55.54';
  DATE = '06.02.2023 12:58:12';
  TEXT = 'Copyright (c) 2023 team2work GmbH';

  XPATH_EXPRESSION_01 = '//optic/optometry/patient/ID';
  
  // OBJECTIVE MEASUREMENT
  
  // Photopic
  XPATH_EXPRESSION_02 = '//optic/optometry/objective_mesurement/measure_WF/wf_right/Photopic/sphere';
  XPATH_EXPRESSION_03 = '//optic/optometry/objective_mesurement/measure_WF/wf_right/Photopic/cylinder';
  XPATH_EXPRESSION_04 = '//optic/optometry/objective_mesurement/measure_WF/wf_right/Photopic/axis';

  XPATH_EXPRESSION_05 = '//optic/optometry/objective_mesurement/measure_WF/wf_left/Photopic/sphere';
  XPATH_EXPRESSION_06 = '//optic/optometry/objective_mesurement/measure_WF/wf_left/Photopic/cylinder';
  XPATH_EXPRESSION_07 = '//optic/optometry/objective_mesurement/measure_WF/wf_left/Photopic/axis';

  // Mesopic
  XPATH_EXPRESSION_08 = '//optic/optometry/objective_mesurement/measure_WF/wf_right/Mesopic/sphere';
  XPATH_EXPRESSION_09 = '//optic/optometry/objective_mesurement/measure_WF/wf_right/Mesopic/cylinder';
  XPATH_EXPRESSION_10 = '//optic/optometry/objective_mesurement/measure_WF/wf_right/Mesopic/axis';  
  
  XPATH_EXPRESSION_11 = '//optic/optometry/objective_mesurement/measure_WF/wf_left/Mesopic/sphere';
  XPATH_EXPRESSION_12 = '//optic/optometry/objective_mesurement/measure_WF/wf_left/Mesopic/cylinder';
  XPATH_EXPRESSION_13 = '//optic/optometry/objective_mesurement/measure_WF/wf_left/Mesopic/axis';

  // Near Vision fast
  XPATH_EXPRESSION_15 = '//optic/optometry/objective_mesurement/measure_WF/wf_right/Near_Vision_fast/sphere';
  XPATH_EXPRESSION_16 = '//optic/optometry/objective_mesurement/measure_WF/wf_right/Near_Vision_fast/cylinder';
  XPATH_EXPRESSION_17 = '//optic/optometry/objective_mesurement/measure_WF/wf_right/Near_Vision_fast/axis';
  XPATH_EXPRESSION_18 = '//optic/optometry/objective_mesurement/measure_WF/wf_right/Near_Vision_fast/addition';  
  
  XPATH_EXPRESSION_19 = '//optic/optometry/objective_mesurement/measure_WF/wf_left/Near_Vision_fast/sphere';
  XPATH_EXPRESSION_20 = '//optic/optometry/objective_mesurement/measure_WF/wf_left/Near_Vision_fast/cylinder';
  XPATH_EXPRESSION_21 = '//optic/optometry/objective_mesurement/measure_WF/wf_left/Near_Vision_fast/axis';
  XPATH_EXPRESSION_22 = '//optic/optometry/objective_mesurement/measure_WF/wf_left/Near_Vision_fast/addition';
  
  // VD
  XPATH_EXPRESSION_14 = '//optic/optometry/objective_mesurement/measure_WF/VD';

  GDT_FID_PATIENT_ID     = '3000';
  GDT_FID_MEASURE_DATA   = '6228';
  GDT_FID_COMMENT        = '6227';
  GDT_FID_SIGNATURE      = '8990';
  GDT_FID_RESULT         = '6220';
  GDT_FID_FOREIGN_RESULT = '6221';
    
  GDT_LINE_PREFIX = '  ';
  GDT_SIGN_SEPARATOR = ' ';
  GDT_AXIS_SEPARATOR = ' ';
  GDT_AXIS_SEPARATOR_MAX_COUNT = 3;
  
  GDT_A_PREFIX = ' ';
  GDT_A_PREFIX_COUNT = 2;

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
  bolAddGDTLinePrefix, bolAddSign, bolAddSignSeparator, bolAddAxisSeparator, bolAddAPrefix, bolAddVDToOutput: Boolean;
  R_Line, L_Line: String;  	
  objective_measurement_measure_WF_VD: String;
  objective_measurement_measure_WF_R_Photopic_SPHERE: String;
  objective_measurement_measure_WF_R_Photopic_CYLINDER: String;
  objective_measurement_measure_WF_R_Photopic_AXIS: String;
  R_S, R_C, R_Axis, R_Add: String;
  objective_measurement_measure_WF_R_Near_Vision_fast_SPHERE: String;
  objective_measurement_measure_WF_R_Near_Vision_fast_CYLINDER: String;	
  objective_measurement_measure_WF_R_Near_Vision_fast_AXIS: String;
  objective_measurement_measure_WF_R_Near_Vision_fast_ADDITION: String;  	
  objective_measurement_measure_WF_L_Photopic_SPHERE: String;
  objective_measurement_measure_WF_L_Photopic_CYLINDER: String;
  objective_measurement_measure_WF_L_Photopic_AXIS: String;
  L_S, L_C, L_Axis, L_Add: String;	
  objective_measurement_measure_WF_L_Near_Vision_fast_SPHERE: String;
  objective_measurement_measure_WF_L_Near_Vision_fast_CYLINDER: String;	
  objective_measurement_measure_WF_L_Near_Vision_fast_AXIS: String;
  objective_measurement_measure_WF_L_Near_Vision_fast_ADDITION: String;    	

begin

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
  	
  // Verwende "True", damit der VD Wert angefügt wird,
  // benutze "False", damit der VD Wert nicht verwendet wird
  bolAddVDToOutput:= True;

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

  // ---------------------------------------------------------
  
  // OBJECTIVE MEASUREMENT
  
  // Right
  
  //
  objective_measurement_measure_WF_R_Photopic_SPHERE:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_02);

  if (Length(arrData) > 0) then
  begin
    objective_measurement_measure_WF_R_Photopic_SPHERE:= FormatSignValue('S=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
  end;
  
  // 
  objective_measurement_measure_WF_R_Photopic_CYLINDER:= '';
  	
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_03);

  if (Length(arrData) > 0) then
  begin
    objective_measurement_measure_WF_R_Photopic_CYLINDER := FormatSignValue('Z=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
  end;
  
  // 
  objective_measurement_measure_WF_R_Photopic_AXIS:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_04);

  if Length(arrData) > 0 then
  begin
    objective_measurement_measure_WF_R_Photopic_AXIS := FormatAxisValue('*', Trim(arrData[0][2]), bolAddAxisSeparator);
  end;
  
  // Near_Vision_fast_right
  
  // 
  objective_measurement_measure_WF_R_Near_Vision_fast_SPHERE:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_15);

  if (Length(arrData) > 0) then
  begin
    objective_measurement_measure_WF_R_Near_Vision_fast_SPHERE:= FormatSignValue('S=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
  end;  
  
  // 
  objective_measurement_measure_WF_R_Near_Vision_fast_CYLINDER:= '';
  	
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_16);

  if (Length(arrData) > 0) then
  begin
    objective_measurement_measure_WF_R_Near_Vision_fast_CYLINDER := FormatSignValue('Z=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
  end;
  
  // 
  objective_measurement_measure_WF_R_Near_Vision_fast_AXIS:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_17);

  if Length(arrData) > 0 then
  begin
    objective_measurement_measure_WF_R_Near_Vision_fast_AXIS := FormatAxisValue('*', Trim(arrData[0][2]), bolAddAxisSeparator);
  end;
  
  //  
  objective_measurement_measure_WF_R_Near_Vision_fast_ADDITION:= '';
  	
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_18);

  if (Length(arrData) > 0) then
  begin
    objective_measurement_measure_WF_R_Near_Vision_fast_ADDITION := FormatSignValue('A=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
  end;  	

  R_S:= objective_measurement_measure_WF_R_Near_Vision_fast_SPHERE;
  R_C:= objective_measurement_measure_WF_R_Near_Vision_fast_CYLINDER;
  R_Axis:= objective_measurement_measure_WF_R_Near_Vision_fast_AXIS;
  R_Add:= objective_measurement_measure_WF_R_Near_Vision_fast_ADDITION;

  // Left

  //
  objective_measurement_measure_WF_L_Photopic_SPHERE:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_05);

  if (Length(arrData) > 0) then
  begin
    objective_measurement_measure_WF_L_Photopic_SPHERE:= FormatSignValue('S=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
  end;

  // 
  objective_measurement_measure_WF_L_Photopic_CYLINDER:= '';
  	
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_06);

  if (Length(arrData) > 0) then
  begin
    objective_measurement_measure_WF_L_Photopic_CYLINDER:= FormatSignValue('Z=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
  end;
  
  // 
  objective_measurement_measure_WF_L_Photopic_AXIS:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_07);

  if Length(arrData) > 0 then
  begin
    objective_measurement_measure_WF_L_Photopic_AXIS:= FormatAxisValue('*', Trim(arrData[0][2]), bolAddAxisSeparator);
  end;
  
  // Near_Vision_fast_left
  
  // 
  objective_measurement_measure_WF_L_Near_Vision_fast_SPHERE:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_19);

  if (Length(arrData) > 0) then
  begin
    objective_measurement_measure_WF_L_Near_Vision_fast_SPHERE:= FormatSignValue('S=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
  end;
  
  // 
  objective_measurement_measure_WF_L_Near_Vision_fast_CYLINDER:= '';
  	
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_20);

  if (Length(arrData) > 0) then
  begin
    objective_measurement_measure_WF_L_Near_Vision_fast_CYLINDER:= FormatSignValue('Z=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
  end;
  
  // 
  objective_measurement_measure_WF_L_Near_Vision_fast_AXIS:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_21);

  if Length(arrData) > 0 then
  begin
    objective_measurement_measure_WF_L_Near_Vision_fast_AXIS:= FormatAxisValue('*', Trim(arrData[0][2]), bolAddAxisSeparator);
  end;     
  
  //  
  objective_measurement_measure_WF_L_Near_Vision_fast_ADDITION:= '';
  	
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_22);

  if (Length(arrData) > 0) then
  begin
    objective_measurement_measure_WF_L_Near_Vision_fast_ADDITION:= FormatSignValue('A=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
  end;

  L_S:= objective_measurement_measure_WF_L_Near_Vision_fast_SPHERE;
  L_C:= objective_measurement_measure_WF_L_Near_Vision_fast_CYLINDER;
  L_Axis:= objective_measurement_measure_WF_L_Near_Vision_fast_AXIS;
  L_Add:= objective_measurement_measure_WF_L_Near_Vision_fast_ADDITION;

  // VD
  objective_measurement_measure_WF_VD:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_14);

  if (Length(arrData) > 0) then
  begin
    objective_measurement_measure_WF_VD:= 'VD= ' + Trim(arrData[0][2]);
  end;

  // Build result
  ParsedData:= '';

  // Add patient ID
  if (PatientID <> '') then
  begin
    ParsedData:= ParsedData + PatientID;
  end;

  // Add right eye
  R_Line:= '';

  if (R_S <> '') and (R_C <> '') and (R_Axis <> '') then
  begin
    R_Line:= R_Line + 'R.:' + R_S + ' ' + R_C + R_Axis;
  end;
  
  // Add right addition
  if (R_Add <> '') then
  begin
    if (R_Line <> '') then
    begin
      R_Line:= R_Line + ' ';
    end;
    
    R_Line:= R_Line + R_Add;
  end;
  
  if (bolAddVDToOutput) and (objective_measurement_measure_WF_VD <> '') then
  begin
    if (R_Line <> '') then
    begin
      R_Line:= R_Line + ' ';
    end; 
    
    R_Line:= R_Line + objective_measurement_measure_WF_VD;
  end;

  if (R_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + R_Line + FOutputLineSeparator;
  end;
    
  // Add left eye
  L_Line:= '';

  if (L_S <> '') and (L_C <> '') and (L_Axis <> '') then
  begin
    L_Line:= L_Line + 'L.:' + L_S + ' ' + L_C + L_Axis;
  end;
  
  // Add left addition
  if (L_Add <> '') then
  begin
    if (L_Line <> '') then
    begin
      L_Line:= L_Line + ' ';
    end;
    
    L_Line:= L_Line + L_Add;
  end;  

  if (L_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
  end;

  // Set output
  FParsedDataString := RawByteString(ParsedData);
end.