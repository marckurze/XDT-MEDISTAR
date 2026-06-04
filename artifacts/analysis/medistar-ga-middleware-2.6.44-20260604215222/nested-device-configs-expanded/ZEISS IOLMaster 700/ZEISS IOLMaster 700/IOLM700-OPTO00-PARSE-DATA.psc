const
  VERSION = '1.0.55.40';
  DATE = '02.03.2022 09:47:08';
  TEXT = 'Copyright (c) 2022 team2work GmbH';
  
  GDT_FID_PATIENT_ID               = '3000';
  GDT_FID_MEASURE_DATA             = '6228';
  GDT_FID_COMMENT                  = '6227';
  GDT_FID_RESULT                   = '6220';
  
  GDT_FID_FILE_ARCHIVE_NUMBER      = '6302';
  GDT_FID_FILE_FORMAT              = '6303';
  GDT_FID_FILE_DESCRIPTION         = '6304';
  GDT_FID_FILE_URL                 = '6305';  
  
  XPATH_EXPRESSION_01 = '//*[local-name()="IOLMaster700Export"]/*[local-name()="ExamData"]/*[local-name()="Patient"]/*[local-name()="Id"]';
   
  // Od = R 
  XPATH_PREFIX_01     = '//*[local-name()="IOLMaster700Export"]/*[local-name()="ExamData"]/*[local-name()="Exam"]/*[local-name()="Od"]';
  
  XPATH_EXPRESSION_02 = XPATH_PREFIX_01 + '/*[local-name()="AxialData"]/*[local-name()="Acd"]/*[local-name()="Quality"]';
  XPATH_EXPRESSION_03 = XPATH_PREFIX_01 + '/*[local-name()="AxialData"]/*[local-name()="Acd"]/*[local-name()="Value"]';
  XPATH_EXPRESSION_04 = XPATH_PREFIX_01 + '/*[local-name()="AxialData"]/*[local-name()="Al"]/*[local-name()="Quality"]';
  XPATH_EXPRESSION_05 = XPATH_PREFIX_01 + '/*[local-name()="AxialData"]/*[local-name()="Al"]/*[local-name()="Value"]';
  XPATH_EXPRESSION_06 = XPATH_PREFIX_01 + '/*[local-name()="AxialData"]/*[local-name()="Aqd"]/*[local-name()="Quality"]';
  XPATH_EXPRESSION_07 = XPATH_PREFIX_01 + '/*[local-name()="AxialData"]/*[local-name()="Aqd"]/*[local-name()="Value"]';
  XPATH_EXPRESSION_08 = XPATH_PREFIX_01 + '/*[local-name()="AxialData"]/*[local-name()="Cct"]/*[local-name()="Quality"]';
  XPATH_EXPRESSION_09 = XPATH_PREFIX_01 + '/*[local-name()="AxialData"]/*[local-name()="Cct"]/*[local-name()="Value"]';
  XPATH_EXPRESSION_10 = XPATH_PREFIX_01 + '/*[local-name()="AxialData"]/*[local-name()="Lt"]/*[local-name()="Quality"]';
  XPATH_EXPRESSION_11 = XPATH_PREFIX_01 + '/*[local-name()="AxialData"]/*[local-name()="Lt"]/*[local-name()="Value"]';
    
  XPATH_EXPRESSION_12 = XPATH_PREFIX_01 + '/*[local-name()="Keratometry"]/*[local-name()="MaximalRadius"]';
  XPATH_EXPRESSION_13 = XPATH_PREFIX_01 + '/*[local-name()="Keratometry"]/*[local-name()="FlatK"]';
  XPATH_EXPRESSION_14 = XPATH_PREFIX_01 + '/*[local-name()="Keratometry"]/*[local-name()="MaximalAxis"]';
  XPATH_EXPRESSION_15 = XPATH_PREFIX_01 + '/*[local-name()="Keratometry"]/*[local-name()="MinimalRadius"]';
  XPATH_EXPRESSION_16 = XPATH_PREFIX_01 + '/*[local-name()="Keratometry"]/*[local-name()="SteepK"]';
  XPATH_EXPRESSION_17 = XPATH_PREFIX_01 + '/*[local-name()="Keratometry"]/*[local-name()="MinimalAxis"]';
  XPATH_EXPRESSION_18 = XPATH_PREFIX_01 + '/*[local-name()="Keratometry"]/*[local-name()="RefractiveIndex"]';
  XPATH_EXPRESSION_19 = XPATH_PREFIX_01 + '/*[local-name()="Keratometry"]/*[local-name()="DeltaD"]';
  XPATH_EXPRESSION_20 = XPATH_PREFIX_01 + '/*[local-name()="Keratometry"]/*[local-name()="DeltaAxis"]';
  XPATH_EXPRESSION_21 = XPATH_PREFIX_01 + '/*[local-name()="Keratometry"]/*[local-name()="Quality"]';
  
  XPATH_EXPRESSION_22 = XPATH_PREFIX_01 + '/*[local-name()="WhiteToWhite"]/*[local-name()="WtwCenter"]/*[local-name()="X"]';
  XPATH_EXPRESSION_23 = XPATH_PREFIX_01 + '/*[local-name()="WhiteToWhite"]/*[local-name()="WtwCenter"]/*[local-name()="Y"]';
  XPATH_EXPRESSION_24 = XPATH_PREFIX_01 + '/*[local-name()="WhiteToWhite"]/*[local-name()="WtwDiameter"]';
  XPATH_EXPRESSION_25 = XPATH_PREFIX_01 + '/*[local-name()="WhiteToWhite"]/*[local-name()="PupilCenter"]/*[local-name()="X"]';
  XPATH_EXPRESSION_26 = XPATH_PREFIX_01 + '/*[local-name()="WhiteToWhite"]/*[local-name()="PupilCenter"]/*[local-name()="Y"]';
  XPATH_EXPRESSION_27 = XPATH_PREFIX_01 + '/*[local-name()="WhiteToWhite"]/*[local-name()="PupilDiameter"]';
  XPATH_EXPRESSION_28 = XPATH_PREFIX_01 + '/*[local-name()="WhiteToWhite"]/*[local-name()="Quality"]';  

  XPATH_EXPRESSION_29 = XPATH_PREFIX_01 + '/*[local-name()="Warnings"]/*[local-name()="Warning"]';  

  // Os = L
  XPATH_PREFIX_02     = '//*[local-name()="IOLMaster700Export"]/*[local-name()="ExamData"]/*[local-name()="Exam"]/*[local-name()="Os"]';
  
  XPATH_EXPRESSION_30 = XPATH_PREFIX_02 + '/*[local-name()="AxialData"]/*[local-name()="Acd"]/*[local-name()="Quality"]';
  XPATH_EXPRESSION_31 = XPATH_PREFIX_02 + '/*[local-name()="AxialData"]/*[local-name()="Acd"]/*[local-name()="Value"]';
  XPATH_EXPRESSION_32 = XPATH_PREFIX_02 + '/*[local-name()="AxialData"]/*[local-name()="Al"]/*[local-name()="Quality"]';
  XPATH_EXPRESSION_33 = XPATH_PREFIX_02 + '/*[local-name()="AxialData"]/*[local-name()="Al"]/*[local-name()="Value"]';
  XPATH_EXPRESSION_34 = XPATH_PREFIX_02 + '/*[local-name()="AxialData"]/*[local-name()="Aqd"]/*[local-name()="Quality"]';
  XPATH_EXPRESSION_35 = XPATH_PREFIX_02 + '/*[local-name()="AxialData"]/*[local-name()="Aqd"]/*[local-name()="Value"]';
  XPATH_EXPRESSION_36 = XPATH_PREFIX_02 + '/*[local-name()="AxialData"]/*[local-name()="Cct"]/*[local-name()="Quality"]';
  XPATH_EXPRESSION_37 = XPATH_PREFIX_02 + '/*[local-name()="AxialData"]/*[local-name()="Cct"]/*[local-name()="Value"]';
  XPATH_EXPRESSION_38 = XPATH_PREFIX_02 + '/*[local-name()="AxialData"]/*[local-name()="Lt"]/*[local-name()="Quality"]';
  XPATH_EXPRESSION_39 = XPATH_PREFIX_02 + '/*[local-name()="AxialData"]/*[local-name()="Lt"]/*[local-name()="Value"]';

  XPATH_EXPRESSION_40 = XPATH_PREFIX_02 + '/*[local-name()="Keratometry"]/*[local-name()="MaximalRadius"]';
  XPATH_EXPRESSION_41 = XPATH_PREFIX_02 + '/*[local-name()="Keratometry"]/*[local-name()="FlatK"]';
  XPATH_EXPRESSION_42 = XPATH_PREFIX_02 + '/*[local-name()="Keratometry"]/*[local-name()="MaximalAxis"]';
  XPATH_EXPRESSION_43 = XPATH_PREFIX_02 + '/*[local-name()="Keratometry"]/*[local-name()="MinimalRadius"]';
  XPATH_EXPRESSION_44 = XPATH_PREFIX_02 + '/*[local-name()="Keratometry"]/*[local-name()="SteepK"]';
  XPATH_EXPRESSION_45 = XPATH_PREFIX_02 + '/*[local-name()="Keratometry"]/*[local-name()="MinimalAxis"]';
  XPATH_EXPRESSION_46 = XPATH_PREFIX_02 + '/*[local-name()="Keratometry"]/*[local-name()="RefractiveIndex"]';
  XPATH_EXPRESSION_47 = XPATH_PREFIX_02 + '/*[local-name()="Keratometry"]/*[local-name()="DeltaD"]';
  XPATH_EXPRESSION_48 = XPATH_PREFIX_02 + '/*[local-name()="Keratometry"]/*[local-name()="DeltaAxis"]';
  XPATH_EXPRESSION_49 = XPATH_PREFIX_02 + '/*[local-name()="Keratometry"]/*[local-name()="Quality"]';

  XPATH_EXPRESSION_50 = XPATH_PREFIX_02 + '/*[local-name()="WhiteToWhite"]/*[local-name()="WtwCenter"]/*[local-name()="X"]';
  XPATH_EXPRESSION_51 = XPATH_PREFIX_02 + '/*[local-name()="WhiteToWhite"]/*[local-name()="WtwCenter"]/*[local-name()="Y"]';
  XPATH_EXPRESSION_52 = XPATH_PREFIX_02 + '/*[local-name()="WhiteToWhite"]/*[local-name()="WtwDiameter"]';
  XPATH_EXPRESSION_53 = XPATH_PREFIX_02 + '/*[local-name()="WhiteToWhite"]/*[local-name()="PupilCenter"]/*[local-name()="X"]';
  XPATH_EXPRESSION_54 = XPATH_PREFIX_02 + '/*[local-name()="WhiteToWhite"]/*[local-name()="PupilCenter"]/*[local-name()="Y"]';
  XPATH_EXPRESSION_55 = XPATH_PREFIX_02 + '/*[local-name()="WhiteToWhite"]/*[local-name()="PupilDiameter"]';
  XPATH_EXPRESSION_56 = XPATH_PREFIX_02 + '/*[local-name()="WhiteToWhite"]/*[local-name()="Quality"]';  

  XPATH_EXPRESSION_57 = XPATH_PREFIX_02 + '/*[local-name()="Warnings"]/*[local-name()="Warning"]'; 
  
  XPATH_EXPRESSION_100 = '//*[local-name()="IOLMaster700Export"]/*[local-name()="ExamData"]/*[local-name()="Exam"]/*[local-name()="Warnings"]/*[local-name()="Warning"]';
 
  GDT_SIGN_SEPARATOR = ' ';
  GDT_AXIS_SEPARATOR = ' ';
  GDT_AXIS_SEPARATOR_MAX_COUNT = 3; 
  
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
  
var
  arrData: TStringArrayArray;
  ParsedData, PatientID, R_Line_Kr, L_Line_Kr, R_Line_IOL, L_Line_IOL: String;
  bolAddExternalFilesToGdtFile, bolAddGDTLinePrefix, bolAddSign, bolAddSignSeparator, bolAddAxisSeparator: Boolean;  
  R_MaximalRadius, R_MaximalAxis, R_MinimalRadius, R_MinimalAxis: String;
  L_MaximalRadius, L_MaximalAxis, L_MinimalRadius, L_MinimalAxis: String;
  R_Acd, R_Al, L_Acd, L_Al: String;
  
begin  
  // Aktiviere den Import aller externen Bildquellen.
  // Verwende "True", damit der Import aktiviert ist, benutze "False", damit Bilder
  // nicht importiert und per GDT weiterverarbeitet werden.
  bolAddExternalFilesToGdtFile:= False;

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

  // -----------------------------------------------------------------

  // OD-OS denotes the right eye (Oculus Dexter) and left eye (Oculus Sinister), as is commonly used in everyday documentation by eye doctors.
  
  // OD - right eye (Oculus Dexter) - R
    
  // 
  R_MaximalRadius:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_12);

  if Length(arrData) > 0 then
  begin
    R_MaximalRadius:= R_MaximalRadius + FormatSignValue('R1=', arrData[0][2], bolAddSign, bolAddSignSeparator);
  end;

  //
  R_MaximalAxis:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_14);

  if Length(arrData) > 0 then
  begin
    R_MaximalAxis:= R_MaximalAxis + FormatAxisValue('*', arrData[0][2], bolAddAxisSeparator);
  end; 
  
  //  
  R_MinimalRadius:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_15);

  if Length(arrData) > 0 then
  begin
    R_MinimalRadius:= R_MinimalRadius + FormatSignValue('R2=', arrData[0][2], bolAddSign, bolAddSignSeparator);
  end;

  //  
  R_MinimalAxis:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_17);

  if Length(arrData) > 0 then
  begin
    R_MinimalAxis:= R_MinimalAxis + FormatAxisValue('*', arrData[0][2], bolAddAxisSeparator);
  end;

  // 
  R_Acd := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_03);

  if Length(arrData) > 0 then
  begin
    R_Acd := R_Acd + 'VKT=' + arrData[0][2];
  end;
  
  //
  R_Al := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_05);

  if Length(arrData) > 0 then
  begin
    R_Al := R_Al + 'AL=' + arrData[0][2];
  end;
  
  //   

  // -----------------------------------------------------------------
  
  // OS - left eye (Oculus Sinister) - L
  
  // 
  L_MaximalRadius:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_40);

  if Length(arrData) > 0 then
  begin
    L_MaximalRadius:= L_MaximalRadius + FormatSignValue('R1=', arrData[0][2], bolAddSign, bolAddSignSeparator);
  end; 
  
  //
  L_MaximalAxis:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_42);

  if Length(arrData) > 0 then
  begin
    L_MaximalAxis:= L_MaximalAxis + FormatAxisValue('*', arrData[0][2], bolAddAxisSeparator);
  end;   
  
  //  
  L_MinimalRadius:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_43);

  if Length(arrData) > 0 then
  begin
    L_MinimalRadius:= L_MinimalRadius + FormatSignValue('R2=', arrData[0][2], bolAddSign, bolAddSignSeparator);
  end; 
  
  //  
  L_MinimalAxis:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_45);

  if Length(arrData) > 0 then
  begin
    L_MinimalAxis:= L_MinimalAxis + FormatAxisValue('*', arrData[0][2], bolAddAxisSeparator);
  end;  
  
  // 
  L_Acd := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_31);

  if Length(arrData) > 0 then
  begin
    L_Acd := L_Acd + 'VKT=' + arrData[0][2];
  end;
  
  //
  L_Al := '';

  SetLength(arrData, 0);
  arrData := DoPSGetXMLData(XPATH_EXPRESSION_33);

  if Length(arrData) > 0 then
  begin
    L_Al := L_Al + 'AL=' + arrData[0][2];
  end;
  
  //   

  // ----------------------------------------------------------------- 
  
  // OD - right eye (Oculus Dexter) - R
  R_Line_Kr:= ''; 
  R_Line_Kr:= R_Line_Kr + 'R:';
  R_Line_Kr:= R_Line_Kr + ' ' + R_MaximalRadius + R_MaximalAxis + ' ' + R_MinimalRadius + R_MinimalAxis;
  
  //
  R_Line_IOL:= '';
  R_Line_IOL:= R_Line_IOL + 'R:';
  R_Line_IOL:= R_Line_IOL + ' ' + R_Acd + ' ' + R_Al;

  // OS - left eye (Oculus Sinister) - L  
  L_Line_Kr:= '';  
  L_Line_Kr:= L_Line_Kr + ' // L:';
  L_Line_Kr:= L_Line_Kr + ' ' + L_MaximalRadius + L_MaximalAxis + ' ' + L_MinimalRadius + L_MinimalAxis;
  
  //
  L_Line_IOL:= '';
  L_Line_IOL:= L_Line_IOL + ' // L:';
  L_Line_IOL:= L_Line_IOL + ' ' + L_Acd + ' ' + L_Al;

  // -----------------------------------------------------------------

  // T2WMessageBoxS();
  
  // Build result
  ParsedData:= '';

  // Add patient ID
  if (PatientID <> '') then
  begin
    ParsedData:= ParsedData + PatientID;
  end;  

  // add Keratometry
  if (R_Line_Kr <> '') and (L_Line_Kr <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + R_Line_Kr + L_Line_Kr + FOutputLineSeparator;
  end;
    
  // IOL
  if (R_Line_IOL <> '') and (L_Line_IOL <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_COMMENT + FOutputLineSeparator;
    ParsedData:= ParsedData + R_Line_IOL + L_Line_IOL + FOutputLineSeparator;
  end;  
  
  //

  // Set output
  FParsedDataString := RawByteString(ParsedData);
end.
      