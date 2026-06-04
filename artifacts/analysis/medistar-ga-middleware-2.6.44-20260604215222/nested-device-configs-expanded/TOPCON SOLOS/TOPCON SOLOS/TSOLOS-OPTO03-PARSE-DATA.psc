const
  VERSION = '1.0.55.23';
  DATE = '07.08.2024 09:04:18';
  TEXT = 'Copyright (c) 2024 team2work GmbH';

  DEFAULT_IDENTIFIER_RIGHT = 'R';
  DEFAULT_IDENTIFIER_LEFT  = 'L';
 
  // PatientID
  XPATH_EXPRESSION_01 = '//Ophthalmology/*[name()=''nsCommon:Common'']/*[name()=''nsCommon:Patient'']/*[name()=''nsCommon:ID'']';  

  // R 
  XPATH_EXPRESSION_02 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:R'']/*[name()=''nsLM:Sphere'']';
  XPATH_EXPRESSION_03 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:R'']/*[name()=''nsLM:Cylinder'']';
  XPATH_EXPRESSION_04 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:R'']/*[name()=''nsLM:Axis'']';
  XPATH_EXPRESSION_05 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:R'']/*[name()=''nsLM:SE'']';
  XPATH_EXPRESSION_06 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:R'']/*[name()=''nsLM:ADD'']';
  XPATH_EXPRESSION_07 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:R'']/*[name()=''nsLM:NearSphere'']';
  XPATH_EXPRESSION_08 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:R'']/*[name()=''nsLM:ADD2'']';
  XPATH_EXPRESSION_09 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:R'']/*[name()=''nsLM:NearSphere2'']'; 
  XPATH_EXPRESSION_10 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:R'']/*[name()=''nsLM:Prism'']';
  XPATH_EXPRESSION_11 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:R'']/*[name()=''nsLM:PrismBase'']';
  XPATH_EXPRESSION_12 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:R'']/*[name()=''nsLM:PrismX'']';
  XPATH_EXPRESSION_13 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:R'']/*[name()=''nsLM:PrismY'']';
  XPATH_EXPRESSION_14 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:R'']/*[name()=''nsLM:UVTransmittance'']'; 
  XPATH_EXPRESSION_15 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:R'']/*[name()=''nsLM:ConfidenceIndex'']';   

  // L
  XPATH_EXPRESSION_16 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:L'']/*[name()=''nsLM:Sphere'']';
  XPATH_EXPRESSION_17 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:L'']/*[name()=''nsLM:Cylinder'']';
  XPATH_EXPRESSION_18 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:L'']/*[name()=''nsLM:Axis'']';
  XPATH_EXPRESSION_19 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:L'']/*[name()=''nsLM:SE'']';
  XPATH_EXPRESSION_20 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:L'']/*[name()=''nsLM:ADD'']';
  XPATH_EXPRESSION_21 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:L'']/*[name()=''nsLM:NearSphere'']';
  XPATH_EXPRESSION_22 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:L'']/*[name()=''nsLM:ADD2'']';
  XPATH_EXPRESSION_23 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:L'']/*[name()=''nsLM:NearSphere2'']';
  XPATH_EXPRESSION_24 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:L'']/*[name()=''nsLM:Prism'']';
  XPATH_EXPRESSION_25 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:L'']/*[name()=''nsLM:PrismBase'']';
  XPATH_EXPRESSION_26 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:L'']/*[name()=''nsLM:PrismX'']';
  XPATH_EXPRESSION_27 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:L'']/*[name()=''nsLM:PrismY'']';
  XPATH_EXPRESSION_28 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:L'']/*[name()=''nsLM:UVTransmittance'']';
  XPATH_EXPRESSION_29 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:LM'']/*[name()=''nsLM:L'']/*[name()=''nsLM:ConfidenceIndex'']';

  // PD
  XPATH_EXPRESSION_30 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:PD'']/*[name()=''nsLM:Distance'']';
  XPATH_EXPRESSION_31 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:PD'']/*[name()=''nsLM:DistanceR'']';
  XPATH_EXPRESSION_32 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:PD'']/*[name()=''nsLM:DistanceL'']';
  XPATH_EXPRESSION_33 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:PD'']/*[name()=''nsLM:Near'']';
  XPATH_EXPRESSION_34 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:PD'']/*[name()=''nsLM:NearR'']';
  XPATH_EXPRESSION_35 = '//Ophthalmology/*[name()=''nsLM:Measure''][@type=''LM'']/*[name()=''nsLM:PD'']/*[name()=''nsLM:NearL'']';  
  
  GDT_FID_PATIENT_ID   = '3000';
  GDT_FID_MEASURE_DATA = '6228';
  GDT_FID_COMMENT      = '6227';
  
  GDT_FID_FILE_ARCHIVE_NUMBER  = '6302';
  GDT_FID_FILE_FORMAT          = '6303';
  GDT_FID_FILE_DESCRIPTION     = '6304';
  GDT_FID_FILE_URL             = '6305';

  GDT_LINE_PREFIX              = '  ';
  GDT_SIGN_SEPARATOR           = ' ';
  GDT_AXIS_SEPARATOR           = ' ';
  GDT_AXIS_SEPARATOR_MAX_COUNT = 3;

var
  arrData: TStringArrayArray; 
  bolAddExternalFilesToGdtFile, bolAddSign, bolAddSignSeparator, bolAddAxisSeparator: Boolean;
  bolAddPrismAndBaseToOutput, bolAddPrismXandYToOutput: Boolean;
  ParsedData, PatientID: String;
  R_Line, L_Line: String;  
  R_S, R_Z, R_Axis, R_Add, R_Add2, R_PRISM, R_PRISMBASE, R_PRISMX, R_PRISMY, R_PRISMX_BASE, R_PRISMY_BASE, R_UVTransmittance: String;
  L_S, L_Z, L_Axis, L_Add, L_Add2, L_PRISM, L_PRISMBASE, L_PRISMX, L_PRISMY, L_PRISMX_BASE, L_PRISMY_BASE, L_UVTransmittance: String;
  Distance, DistanceR, DistanceL, Near, NearR, NearL: String;

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
        
begin
  // Aktiviere den Import aller externen Bildquellen.
  // Verwende "True", damit der Import aktiviert ist, benutze "False", damit Bilder
  // nicht importiert und per GDT weiterverarbeitet werden.
  bolAddExternalFilesToGdtFile:= False;

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
  
  // Verwende "True", damit der Prism und Base Wert angefügt wird,
  // benutze "False", damit der Prism und Base Wert nicht verwendet wird
  bolAddPrismAndBaseToOutput:= False;
  
  // Verwende "True", damit der Prism X und Y Wert angefügt wird,
  // benutze "False", damit der Prism X und Y Wert nicht verwendet wird
  bolAddPrismXandYToOutput:= True;  

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

  //
  // RIGHT
  //

  // R_S
  R_S:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_02);

  if Length(arrData) > 0 then
  begin
    R_S:= R_S + FormatSignValue('S=', arrData[0][2], bolAddSign, bolAddSignSeparator);
  end;
  
  if (R_S = '') then
  begin
    // do nothing
  end;
  
  // R_Z
  R_Z:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_03);

  if Length(arrData) > 0 then
  begin
    R_Z:= R_Z + FormatSignValue('Z=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
  end;
  
  if (R_Z = '') then
  begin
    // do nothing
  end;

  // R_Axis
  R_Axis:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_04);

  if Length(arrData) > 0 then
  begin
    R_Axis:= R_Axis + FormatAxisValue('*', arrData[0][2], bolAddAxisSeparator);
  end;
  
  if (R_Axis = '') then
  begin
    // do nothing
  end;

  // R_ADD
  R_Add:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_06);

  if Length(arrData) > 0 then
  begin
    R_Add:= R_Add + FormatSignValue('A=', arrData[0][2], bolAddSign, bolAddSignSeparator);
  end;
  
  if (R_Add = '') then
  begin
    // do nothing
  end;

  // R_ADD2
  R_Add2:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_08);

  if Length(arrData) > 0 then
  begin
    R_Add2:= R_Add2 + FormatSignValue('A2=', arrData[0][2], bolAddSign, bolAddSignSeparator);
  end;
  
  if (R_Add2 = '') then
  begin
    // do nothing
  end;
    
  // R_PRISM  
  R_PRISM:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_10);

  if Length(arrData) > 0 then
  begin
    R_PRISM:= R_PRISM + FormatSignValue('P=', arrData[0][2], bolAddSign, bolAddSignSeparator);
  end;
  
  if (R_PRISM = '') then
  begin
    // do nothing
  end;

  // R_PRISMBASE
  R_PRISMBASE:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_11);

  if Length(arrData) > 0 then
  begin
    R_PRISMBASE:= R_PRISMBASE + FormatSignValue('B=', arrData[0][2], bolAddSign, bolAddSignSeparator);
  end;
  
  if (R_PRISMBASE = '') then
  begin
    // do nothing
  end;
 
  // R_PRISMX
  R_PRISMX:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_12);

  if Length(arrData) > 0 then
  begin
    R_PRISMX:= R_PRISMX + FormatSignValue('P=', arrData[0][2], False, bolAddSignSeparator);
  end;
  
  if (R_PRISMX = '') then
  begin
    // do nothing
  end;
  
  // R_PRISMX_BASE
  R_PRISMX_BASE:= '';

  if Length(arrData) > 0 then
  begin
    R_PRISMX_BASE:= arrData[0][6];

    if R_PRISMX_BASE = 'in' then
    begin
      R_PRISMX_BASE:= 'I';
    end;

    if R_PRISMX_BASE = 'out' then
    begin
      R_PRISMX_BASE:= 'O';
    end;          
  end;

  // build prismx output string
  R_PRISMX:= R_PRISMX + ' ' + R_PRISMX_BASE;
  R_PRISMX:= Trim(R_PRISMX);

  // R_PRISMY
  R_PRISMY:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_13);

  if Length(arrData) > 0 then
  begin
    R_PRISMY:= R_PRISMY + FormatSignValue('', arrData[0][2], False, bolAddSignSeparator);
  end;
  
  if (R_PRISMY = '') then
  begin
    // do nothing
  end;

  // R_PRISMY_BASE
  R_PRISMY_BASE:= '';

  if Length(arrData) > 0 then
  begin
    R_PRISMY_BASE:= arrData[0][6];

    if R_PRISMY_BASE = 'up' then
    begin
      R_PRISMY_BASE:= 'U';
    end;

    if R_PRISMY_BASE = 'down' then
    begin
      R_PRISMY_BASE:= 'D';
    end;          
  end;

  // build prismy output string
  R_PRISMY:= R_PRISMY + ' ' + R_PRISMY_BASE;
  R_PRISMY:= Trim(R_PRISMY);
  
  // R_UVTransmittance 
  R_UVTransmittance:= '';
 
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_14);

  if Length(arrData) > 0 then
  begin
    R_UVTransmittance:= R_UVTransmittance + 'R: UV = ' + arrData[0][2] + arrData[0][4] + '; ';
  end;
  
  if (R_UVTransmittance = '') then
  begin
    // do nothing
  end;

  //
  // LEFT
  //

  // L_S
  L_S:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_16);

  if Length(arrData) > 0 then
  begin
    L_S:= L_S + FormatSignValue('S=', arrData[0][2], bolAddSign, bolAddSignSeparator);
  end;
  
  if (L_S = '') then
  begin
    // do nothing
  end;

  // L_Z
  L_Z:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_17);

  if Length(arrData) > 0 then
  begin
    L_Z:= L_Z + FormatSignValue('Z=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
  end;
  
  if (L_Z = '') then
  begin
    // do nothing
  end;

  // L_Axis
  L_Axis:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_18);

  if Length(arrData) > 0 then
  begin
    L_Axis:= L_Axis + FormatAxisValue('*', arrData[0][2], bolAddAxisSeparator);
  end;
  
  if (L_Axis = '') then
  begin
    // do nothing
  end;

  // L_ADD
  L_Add:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_20);

  if Length(arrData) > 0 then
  begin
    L_Add:= L_Add + FormatSignValue('A=', arrData[0][2], bolAddSign, bolAddSignSeparator);
  end;
  
  if (L_Add = '') then
  begin
    // do nothing
  end;

  // L_ADD2
  L_Add2:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_22);

  if Length(arrData) > 0 then
  begin
    L_Add2:= L_Add2 + FormatSignValue('A2=', arrData[0][2], bolAddSign, bolAddSignSeparator);
  end;
  
  if (L_Add2 = '') then
  begin
    // do nothing
  end;

  // L_PRISM  
  L_PRISM:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_24);

  if Length(arrData) > 0 then
  begin
    L_PRISM:= L_PRISM + FormatSignValue('P=', arrData[0][2], bolAddSign, bolAddSignSeparator);
  end;
  
  if (L_PRISM = '') then
  begin
    // do nothing
  end;

  // L_PRISMBASE
  L_PRISMBASE:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_25);

  if Length(arrData) > 0 then
  begin
    L_PRISMBASE:= L_PRISMBASE + FormatSignValue('B=', arrData[0][2], bolAddSign, bolAddSignSeparator);
  end;
  
  if (L_PRISMBASE = '') then
  begin
    // do nothing
  end;

  // L_PRISMX
  L_PRISMX:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_26);

  if Length(arrData) > 0 then
  begin
    L_PRISMX:= L_PRISMX + FormatSignValue('P=', arrData[0][2], False, bolAddSignSeparator);
  end;
  
  if (L_PRISMX = '') then
  begin
    // do nothing
  end;

  // L_PRISMX_BASE
  L_PRISMX_BASE:= '';

  if Length(arrData) > 0 then
  begin
    L_PRISMX_BASE:= arrData[0][6];

    if L_PRISMX_BASE = 'in' then
    begin
      L_PRISMX_BASE:= 'I';
    end;

    if L_PRISMX_BASE = 'out' then
    begin
      L_PRISMX_BASE:= 'O';
    end;          
  end;

  // build prismx output string
  L_PRISMX:= L_PRISMX + ' ' + L_PRISMX_BASE;
  L_PRISMX:= Trim(L_PRISMX);

  // L_PRISMY
  L_PRISMY:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_27);
  
  if Length(arrData) > 0 then
  begin
    L_PRISMY:= L_PRISMY + FormatSignValue('', arrData[0][2], False, bolAddSignSeparator);
  end;
  
  if (L_PRISMY = '') then
  begin
    // do nothing
  end;

  // R_PRISMY_BASE
  L_PRISMY_BASE:= '';

  if Length(arrData) > 0 then
  begin
    L_PRISMY_BASE:= arrData[0][6];

    if L_PRISMY_BASE = 'up' then
    begin
      L_PRISMY_BASE:= 'U';
    end;

    if L_PRISMY_BASE = 'down' then
    begin
      L_PRISMY_BASE:= 'D';
    end;          
  end;

  // build prismy output string
  L_PRISMY:= L_PRISMY + ' ' + L_PRISMY_BASE;
  L_PRISMY:= Trim(L_PRISMY);

  // L_UVTransmittance 
  L_UVTransmittance:= '';
 
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_28);

  if Length(arrData) > 0 then
  begin
    L_UVTransmittance:= L_UVTransmittance + 'L: UV = ' + arrData[0][2] + arrData[0][4];
  end;
  
  if (L_UVTransmittance = '') then
  begin
    // do nothing
  end;
  
  // PD
  Distance:= '';  
  
  Near:= '';
  NearR:= '';
  NearL:= '';
  
  // DistanceR
  DistanceR:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_31);  
  
  if Length(arrData) > 0 then
  begin
    DistanceR:= DistanceR + 'PD= ' + arrData[0][2];
  end;  

  // DistanceL
  DistanceL:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_32);  
  
  if Length(arrData) > 0 then
  begin
    DistanceL:= DistanceL + 'PD= ' + arrData[0][2];
  end;    

  // -----------------------------------------------------------------

  // T2WMessageBoxS();

  // Build result
  ParsedData:= '';

  // Add patient ID
  if (PatientID <> '') then
  begin
    ParsedData:= ParsedData + PatientID;
  end;

  // Add right eye
  R_Line:= '';

  if (R_S <> '') and (R_Z <> '') and (R_Axis <> '') then
  begin
    R_Line:= R_Line + 'R.:' + R_S + ' ' + R_Z + R_Axis;
  end;
    
  // add PRISM and BASE if set
  if (R_PRISM <> '') and (R_PRISMBASE <> '') and (bolAddPrismAndBaseToOutput) then
  begin
    if (R_Line <> '') then
    begin
      R_Line:= R_Line + ' ';
    end;

    R_Line:= R_Line + R_PRISM + ' ' + R_PRISMBASE;      
  end;

  // add PRISM_X and PRISM_Y
  if (R_PRISMX <> '') and (R_PRISMY <> '') and (bolAddPrismXandYToOutput) then
  begin
    if (R_Line <> '') then
    begin
      R_Line:= R_Line + ' ';
    end;  

    R_Line:= R_Line + R_PRISMX + ' ' + R_PRISMY;
  end;
  
  if (R_Add <> '') then
  begin
    if (R_Line <> '') then
    begin
      R_Line:= R_Line + ' ';
    end;  
  
    R_Line:= R_Line + R_Add;
  end;
  
  if (R_Add2 <> '') then
  begin
    if (R_Line <> '') then
    begin
      R_Line:= R_Line + ' ';
    end;  
  
    R_Line:= R_Line + R_Add2;
  end;
  
  if (DistanceR <> '') then
  begin
    if (R_Line <> '') then
    begin
      R_Line:= R_Line + ' ';
    end;  
  
    R_Line:= R_Line + DistanceR;
  end;  

  if (R_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + R_Line + FOutputLineSeparator;
  end;

  // Add left eye
  L_Line:= ''; 
          
  if (L_S <> '') and (L_Z <> '') and (L_Axis <> '') then
  begin
    L_Line:= L_Line + 'L.:' + L_S + ' ' + L_Z + L_Axis;
  end;

  // add PRISM and BASE if set
  if (L_PRISM <> '') and (L_PRISMBASE <> '') and (bolAddPrismAndBaseToOutput) then
  begin
    if (L_Line <> '') then
    begin
      L_Line:= L_Line + ' ';
    end;

    L_Line:= L_Line + L_PRISM + ' ' + L_PRISMBASE;      
  end;

  // add PRISM_X and PRISM_Y
  if (L_PRISMX <> '') and (L_PRISMY <> '') and (bolAddPrismXandYToOutput) then
  begin
    if (L_Line <> '') then
    begin
      L_Line:= L_Line + ' ';
    end;  

    L_Line:= L_Line + L_PRISMX + ' ' + L_PRISMY;
  end;
  
  if (L_Add <> '') then
  begin
    if (L_Line <> '') then
    begin
      L_Line:= L_Line + ' ';
    end;  
  
    L_Line:= L_Line + L_Add;
  end;
  
  if (L_Add2 <> '') then
  begin
    if (L_Line <> '') then
    begin
      L_Line:= L_Line + ' ';
    end;  
  
    L_Line:= L_Line + L_Add2;
  end;
  
  if (DistanceL <> '') then
  begin
    if (L_Line <> '') then
    begin
      L_Line:= L_Line + ' ';
    end;  
  
    L_Line:= L_Line + DistanceL;
  end;  
  
  if (L_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
  end;
 
  if (R_UVTransmittance <> '') and (L_UVTransmittance <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_COMMENT + FOutputLineSeparator;
    ParsedData:= ParsedData + R_UVTransmittance + L_UVTransmittance + FOutputLineSeparator;
  end;
  
  // Set output
  FParsedDataString:= RawByteString(ParsedData);
end.