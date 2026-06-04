const
  VERSION = '1.0.55.43';
  DATE = '01.07.2025 12:30:28';
  TEXT = 'Copyright (c) 2025 CompuGroup Medical Deutschland AG';

  // Hier können die Listeneinträge gewählt werden, falls keine
  // Mittelwerte vorhanden sind
  USE_KM_R_LIST_ENTRY = 0;
  USE_KM_L_LIST_ENTRY = 0;

  XPATH_EXPRESSION_01 = '//Ophthalmology/*[name()=''nsCommon:Common'']/*[name()=''nsCommon:Patient'']/*[name()=''nsCommon:ID'']';

  // R (median)
  XPATH_EXPRESSION_02 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:R'']/*[name()=''nsKM:Median'']/*[name()=''nsKM:R1'']/*[name()=''nsKM:Radius'']';
  XPATH_EXPRESSION_03 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:R'']/*[name()=''nsKM:Median'']/*[name()=''nsKM:R1'']/*[name()=''nsKM:Axis'']';
  XPATH_EXPRESSION_28 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:R'']/*[name()=''nsKM:Median'']/*[name()=''nsKM:R1'']/*[name()=''nsKM:Power'']';

  XPATH_EXPRESSION_04 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:R'']/*[name()=''nsKM:Median'']/*[name()=''nsKM:R2'']/*[name()=''nsKM:Radius'']';
  XPATH_EXPRESSION_30 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:R'']/*[name()=''nsKM:Median'']/*[name()=''nsKM:R2'']/*[name()=''nsKM:Axis'']';
  XPATH_EXPRESSION_05 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:R'']/*[name()=''nsKM:Median'']/*[name()=''nsKM:R2'']/*[name()=''nsKM:Power'']';
  
  XPATH_EXPRESSION_06 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:R'']/*[name()=''nsKM:Median'']/*[name()=''nsKM:Average'']/*[name()=''nsKM:Radius'']';
  XPATH_EXPRESSION_32 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:R'']/*[name()=''nsKM:Median'']/*[name()=''nsKM:Average'']/*[name()=''nsKM:Power'']';
  
  XPATH_EXPRESSION_07 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:R'']/*[name()=''nsKM:Median'']/*[name()=''nsKM:Cylinder'']/*[name()=''nsKM:Power'']';
  XPATH_EXPRESSION_40 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:R'']/*[name()=''nsKM:Median'']/*[name()=''nsKM:Cylinder'']/*[name()=''nsKM:Axis'']';

  // L (median)
  XPATH_EXPRESSION_09 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:L'']/*[name()=''nsKM:Median'']/*[name()=''nsKM:R1'']/*[name()=''nsKM:Radius'']';
  XPATH_EXPRESSION_10 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:L'']/*[name()=''nsKM:Median'']/*[name()=''nsKM:R1'']/*[name()=''nsKM:Axis'']';
  XPATH_EXPRESSION_34 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:L'']/*[name()=''nsKM:Median'']/*[name()=''nsKM:R1'']/*[name()=''nsKM:Power'']';

  XPATH_EXPRESSION_11 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:L'']/*[name()=''nsKM:Median'']/*[name()=''nsKM:R2'']/*[name()=''nsKM:Radius'']';
  XPATH_EXPRESSION_12 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:L'']/*[name()=''nsKM:Median'']/*[name()=''nsKM:R2'']/*[name()=''nsKM:Axis'']';
  XPATH_EXPRESSION_36 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:L'']/*[name()=''nsKM:Median'']/*[name()=''nsKM:R2'']/*[name()=''nsKM:Power'']';
  
  XPATH_EXPRESSION_13 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:L'']/*[name()=''nsKM:Median'']/*[name()=''nsKM:Average'']/*[name()=''nsKM:Radius'']';
  XPATH_EXPRESSION_38 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:L'']/*[name()=''nsKM:Median'']/*[name()=''nsKM:Average'']/*[name()=''nsKM:Power'']';
  
  XPATH_EXPRESSION_14 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:L'']/*[name()=''nsKM:Median'']/*[name()=''nsKM:Cylinder'']/*[name()=''nsKM:Power'']';
  XPATH_EXPRESSION_41 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:L'']/*[name()=''nsKM:Median'']/*[name()=''nsKM:Cylinder'']/*[name()=''nsKM:Axis'']';

  // R (list)
  XPATH_EXPRESSION_16 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:R'']/*[name()=''nsKM:List'']/*[name()=''nsKM:R1'']/*[name()=''nsKM:Radius'']';
  XPATH_EXPRESSION_29 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:R'']/*[name()=''nsKM:List'']/*[name()=''nsKM:R1'']/*[name()=''nsKM:Axis'']';
  XPATH_EXPRESSION_17 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:R'']/*[name()=''nsKM:List'']/*[name()=''nsKM:R1'']/*[name()=''nsKM:Power'']';

  XPATH_EXPRESSION_18 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:R'']/*[name()=''nsKM:List'']/*[name()=''nsKM:R2'']/*[name()=''nsKM:Radius'']';
  XPATH_EXPRESSION_31 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:R'']/*[name()=''nsKM:List'']/*[name()=''nsKM:R2'']/*[name()=''nsKM:Axis'']';
  XPATH_EXPRESSION_19 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:R'']/*[name()=''nsKM:List'']/*[name()=''nsKM:R2'']/*[name()=''nsKM:Power'']';
    
  XPATH_EXPRESSION_20 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:R'']/*[name()=''nsKM:List'']/*[name()=''nsKM:Average'']/*[name()=''nsKM:Radius'']';
  XPATH_EXPRESSION_33 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:R'']/*[name()=''nsKM:List'']/*[name()=''nsKM:Average'']/*[name()=''nsKM:Power'']';
  
  XPATH_EXPRESSION_21 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:R'']/*[name()=''nsKM:List'']/*[name()=''nsKM:Cylinder'']/*[name()=''nsKM:Power'']';
  XPATH_EXPRESSION_42 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:R'']/*[name()=''List'']/*[name()=''nsKM:Cylinder'']/*[name()=''nsKM:Axis'']';

  // L (list)
  XPATH_EXPRESSION_22 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:L'']/*[name()=''nsKM:List'']/*[name()=''nsKM:R1'']/*[name()=''nsKM:Radius'']';
  XPATH_EXPRESSION_23 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:L'']/*[name()=''nsKM:List'']/*[name()=''nsKM:R1'']/*[name()=''nsKM:Axis'']';
  XPATH_EXPRESSION_35 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:L'']/*[name()=''nsKM:List'']/*[name()=''nsKM:R1'']/*[name()=''nsKM:Power'']';

  XPATH_EXPRESSION_24 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:L'']/*[name()=''nsKM:List'']/*[name()=''nsKM:R2'']/*[name()=''nsKM:Radius'']';
  XPATH_EXPRESSION_37 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:L'']/*[name()=''nsKM:List'']/*[name()=''nsKM:R2'']/*[name()=''nsKM:Axis'']';
  XPATH_EXPRESSION_25 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:L'']/*[name()=''nsKM:List'']/*[name()=''nsKM:R2'']/*[name()=''nsKM:Power'']';
  
  XPATH_EXPRESSION_26 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:L'']/*[name()=''nsKM:List'']/*[name()=''nsKM:Average'']/*[name()=''nsKM:Radius'']';
  XPATH_EXPRESSION_39 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:L'']/*[name()=''nsKM:List'']/*[name()=''nsKM:Average'']/*[name()=''nsKM:Power'']';
  
  XPATH_EXPRESSION_27 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:L'']/*[name()=''nsKM:List'']/*[name()=''nsKM:Cylinder'']/*[name()=''nsKM:Power'']';
  XPATH_EXPRESSION_43 = '//Ophthalmology/*[name()=''nsKM:Measure''][@type=''KM'']/*[name()=''nsKM:KM'']/*[name()=''nsKM:L'']/*[name()=''List'']/*[name()=''nsKM:Cylinder'']/*[name()=''nsKM:Axis'']';

  GDT_FID_PATIENT_ID = '3000';
  GDT_FID_MEASURE_DATA = '6228';
  GDT_FID_COMMENT = '6227';
  GDT_FID_FILE_ARCHIVE_NUMBER = '6302';
  GDT_FID_FILE_FORMAT = '6303';
  GDT_FID_FILE_DESCRIPTION = '6304';
  GDT_FID_FILE_URL = '6305';

  GDT_LINE_PREFIX = '  ';
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
  ParsedData, PatientID: String;
  R_KMMedian_R1Radius, R_KMMedian_R1Axis, R_KMMedian_R2Radius, R_KMMedian_R2Axis, R_KMMedian_AverageRadius, R_KMMedian_KMCylinderPower: String;
  L_KMMedian_R1Radius, L_KMMedian_R1Axis, L_KMMedian_R2Radius, L_KMMedian_R2Axis, L_KMMedian_AverageRadius, L_KMMedian_KMCylinderPower: String;
  R_Line1_Seg1, R_Line1_Seg2, R_Line1, R_Line2, L_Line1_Seg1, L_Line1_Seg2, L_Line1, L_Line2: String;
  bolAddExternalFilesToGdtFile, bolAddPowerValueToOutput: Boolean;
  R_KMMedian_R1Power, R_KMMedian_R2Power, R_KMMedian_AveragePower, L_KMMedian_R1Power, L_KMMedian_R2Power, L_KMMedian_AveragePower: String;
  R_KMMedian_KMCylinderAxis, L_KMMedian_KMCylinderAxis: String;
  bolAddSign, bolAddSignSeparator, bolAddAxisSeparator: Boolean;
begin
  // Aktiviere den Import aller externen Bildquellen.
  // Verwende "True", damit der Import aktiviert ist, benutze "False", damit Bilder
  // nicht importiert und per GDT weiterverarbeitet werden.
  bolAddExternalFilesToGdtFile:= False;

  // Verwende "True", damit der <Power> Wert an die V7 Zeile angehängt wird,
  // benutze "False", damit der <Power> Wert ignoriert werden kann.
  bolAddPowerValueToOutput := False;

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

  // R_KMMedian_R1Radius
  R_KMMedian_R1Radius:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_02);

  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_16);

    if Length(arrData) > USE_KM_R_LIST_ENTRY then
    begin
      R_KMMedian_R1Radius:= R_KMMedian_R1Radius + 'R1= ' + arrData[USE_KM_R_LIST_ENTRY][2];
    end;
  end
  else
  begin
    R_KMMedian_R1Radius:= R_KMMedian_R1Radius + 'R1= ' + arrData[0][2];
  end;

  // R_KMMedian_R1Power
  R_KMMedian_R1Power:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_28);

  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_29);

    if Length(arrData) > USE_KM_R_LIST_ENTRY then
      begin
        R_KMMedian_R1Power:= R_KMMedian_R1Power + ' ' + arrData[USE_KM_R_LIST_ENTRY][2] + ' ';
    end;
  end
  else
  begin
    R_KMMedian_R1Power:= R_KMMedian_R1Power + ' ' + arrData[0][2] + ' ';
  end;

  // R_KMMedian_R1Axis
  R_KMMedian_R1Axis:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_03);

  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_17);

    if Length(arrData) > USE_KM_R_LIST_ENTRY then
     begin
       R_KMMedian_R1Axis:= R_KMMedian_R1Axis + FormatAxisValue('*', Trim(arrData[USE_KM_R_LIST_ENTRY][2]), bolAddAxisSeparator);  
    end;
  end
  else
  begin
    R_KMMedian_R1Axis:= R_KMMedian_R1Axis + FormatAxisValue('*', Trim(arrData[0][2]), bolAddAxisSeparator);  
  end;

  // R_KMMedian_R2Radius
  R_KMMedian_R2Radius:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_04);

  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_18);

    if Length(arrData) > USE_KM_R_LIST_ENTRY then
    begin
      R_KMMedian_R2Radius:= R_KMMedian_R2Radius + 'R2= ' + arrData[USE_KM_R_LIST_ENTRY][2];
    end;
  end
  else
  begin
    R_KMMedian_R2Radius:= R_KMMedian_R2Radius + 'R2= ' + arrData[0][2];
  end;

  // R_KMMedian_R2Power
  R_KMMedian_R2Power:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_30);

  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_31);

    if Length(arrData) > USE_KM_R_LIST_ENTRY then
    begin
      R_KMMedian_R2Power:= R_KMMedian_R2Power + ' ' + arrData[USE_KM_R_LIST_ENTRY][2] + ' ';
    end;
  end
  else
  begin
    R_KMMedian_R2Power:= R_KMMedian_R2Power + ' ' + arrData[0][2] + ' ';
  end;

  // R_KMMedian_R2Axis
  R_KMMedian_R2Axis:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_05);

  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_19);

    if Length(arrData) > USE_KM_R_LIST_ENTRY then
    begin
      R_KMMedian_R2Axis:= R_KMMedian_R2Axis + FormatAxisValue('*', Trim(arrData[USE_KM_R_LIST_ENTRY][2]), bolAddAxisSeparator);  
    end;
  end
  else
  begin
    R_KMMedian_R2Axis:= R_KMMedian_R2Axis + FormatAxisValue('*', Trim(arrData[0][2]), bolAddAxisSeparator);
  end;

  // R_KMMedian_AverageRadius
  R_KMMedian_AverageRadius:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_06);

  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_20);

    if Length(arrData) > USE_KM_R_LIST_ENTRY then
    begin
      R_KMMedian_AverageRadius:= R_KMMedian_AverageRadius + 'AV= ' + arrData[USE_KM_R_LIST_ENTRY][2];
    end;
  end
  else
  begin
    R_KMMedian_AverageRadius:= R_KMMedian_AverageRadius + 'AV= ' + arrData[0][2];
  end;

  // R_KMMedian_AveragePower
  R_KMMedian_AveragePower:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_32);

  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_33);

    if Length(arrData) > USE_KM_R_LIST_ENTRY then
    begin
      R_KMMedian_AveragePower:= R_KMMedian_AveragePower + ' ' + arrData[USE_KM_R_LIST_ENTRY][2];
    end;
  end
  else
  begin
    R_KMMedian_AveragePower:= R_KMMedian_AveragePower + ' ' + arrData[0][2];
  end;

  // R_KMMedian_KMCylinderPower
  R_KMMedian_KMCylinderPower:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_07);

  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_21);

    if Length(arrData) > USE_KM_R_LIST_ENTRY then
    begin           
      R_KMMedian_KMCylinderPower:= R_KMMedian_KMCylinderPower + FormatSignValue('CYL=', Trim(arrData[USE_KM_R_LIST_ENTRY][2]), bolAddSign, bolAddSignSeparator);
    end;
  end
  else
  begin  
    R_KMMedian_KMCylinderPower:= R_KMMedian_KMCylinderPower + FormatSignValue('CYL=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);  
  end;
  
  // R_KMMedian_KMCylinderAxis
  R_KMMedian_KMCylinderAxis:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_40);

  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_42);

    if Length(arrData) > USE_KM_R_LIST_ENTRY then
    begin
      R_KMMedian_KMCylinderAxis:= R_KMMedian_KMCylinderAxis + ' ' + arrData[USE_KM_R_LIST_ENTRY][2];
    end;
  end
  else
  begin
    R_KMMedian_KMCylinderAxis:= R_KMMedian_KMCylinderAxis + ' ' + arrData[0][2];
  end;  

  // L_KMMedian_R1Radius
  L_KMMedian_R1Radius:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_09);

  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_22);

    if Length(arrData) > USE_KM_L_LIST_ENTRY then
    begin
      L_KMMedian_R1Radius:= L_KMMedian_R1Radius + 'R1= ' + arrData[USE_KM_L_LIST_ENTRY][2];
    end;
  end
  else
  begin
    L_KMMedian_R1Radius:= L_KMMedian_R1Radius + 'R1= ' + arrData[0][2];
  end;

  // L_KMMedian_R1Power
  L_KMMedian_R1Power:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_34);

  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_35);

    if Length(arrData) > USE_KM_L_LIST_ENTRY then
    begin
      L_KMMedian_R1Power:= L_KMMedian_R1Power + ' ' + arrData[USE_KM_L_LIST_ENTRY][2] + ' ';
    end;
  end
  else
  begin
    L_KMMedian_R1Power:= L_KMMedian_R1Power + ' ' + arrData[0][2] + ' ';
  end;

  // L_KMMedian_R1Axis
  L_KMMedian_R1Axis:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_10);

  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_23);

    if Length(arrData) > USE_KM_L_LIST_ENTRY then
    begin
      L_KMMedian_R1Axis:= L_KMMedian_R1Axis + FormatAxisValue('*', Trim(arrData[USE_KM_L_LIST_ENTRY][2]), bolAddAxisSeparator);  
    end;
  end
  else
  begin
    L_KMMedian_R1Axis:= L_KMMedian_R1Axis + FormatAxisValue('*', Trim(arrData[0][2]), bolAddAxisSeparator);  
  end;

  // L_KMMedian_R2Radius
  L_KMMedian_R2Radius:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_11);

  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_24);

    if Length(arrData) > USE_KM_L_LIST_ENTRY then
    begin
      L_KMMedian_R2Radius:= L_KMMedian_R2Radius + 'R2= ' + arrData[USE_KM_L_LIST_ENTRY][2];
    end;
  end
  else
  begin
    L_KMMedian_R2Radius:= L_KMMedian_R2Radius + 'R2= ' + arrData[0][2];
  end;

  // L_KMMedian_R2Power
  L_KMMedian_R2Power:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_36);

  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_37);

    if Length(arrData) > USE_KM_L_LIST_ENTRY then
    begin
      L_KMMedian_R2Power:= L_KMMedian_R2Power + ' ' + arrData[USE_KM_L_LIST_ENTRY][2] + ' ';
    end;
  end
  else
  begin
    L_KMMedian_R2Power:= L_KMMedian_R2Power + ' ' + arrData[0][2] + ' ';
  end;

  // L_KMMedian_R2Axis
  L_KMMedian_R2Axis:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_12);

  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_25);

    if Length(arrData) > USE_KM_L_LIST_ENTRY then
    begin
      L_KMMedian_R2Axis:= L_KMMedian_R2Axis + FormatAxisValue('*', Trim(arrData[USE_KM_L_LIST_ENTRY][2]), bolAddAxisSeparator);  
    end;
  end
  else
  begin
    L_KMMedian_R2Axis:= L_KMMedian_R2Axis + FormatAxisValue('*', Trim(arrData[0][2]), bolAddAxisSeparator);    
  end;

  // L_KMMedian_AverageRadius
  L_KMMedian_AverageRadius:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_13);

  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_26);

    if Length(arrData) > USE_KM_L_LIST_ENTRY then
    begin
      L_KMMedian_AverageRadius:= L_KMMedian_AverageRadius + 'AV= ' + arrData[USE_KM_L_LIST_ENTRY][2];
    end;
  end
  else
  begin
    L_KMMedian_AverageRadius:= L_KMMedian_AverageRadius + 'AV= ' + arrData[0][2];
  end;

  // L_KMMedian_AveragePower
  L_KMMedian_AveragePower:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_38);

  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_39);

    if Length(arrData) > USE_KM_L_LIST_ENTRY then
    begin
      L_KMMedian_AveragePower:= L_KMMedian_AveragePower + ' ' + arrData[USE_KM_L_LIST_ENTRY][2];
    end;
  end
  else
  begin
    L_KMMedian_AveragePower:= L_KMMedian_AveragePower + ' ' + arrData[0][2];
  end;

  // L_KMMedian_KMCylinderPower
  L_KMMedian_KMCylinderPower:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_14);

  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_27);

    if Length(arrData) > USE_KM_L_LIST_ENTRY then
    begin
      L_KMMedian_KMCylinderPower:= L_KMMedian_KMCylinderPower + FormatSignValue('CYL=', Trim(arrData[USE_KM_L_LIST_ENTRY][2]), bolAddSign, bolAddSignSeparator);
    end;
  end
  else
  begin   
    L_KMMedian_KMCylinderPower:= L_KMMedian_KMCylinderPower + FormatSignValue('CYL=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);  
  end;
  
  // L_KMMedian_KMCylinderAxis
  L_KMMedian_KMCylinderAxis:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_41);

  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_43);

    if Length(arrData) > USE_KM_L_LIST_ENTRY then
    begin
      L_KMMedian_KMCylinderAxis:= L_KMMedian_KMCylinderAxis + ' ' + arrData[USE_KM_L_LIST_ENTRY][2];
    end;
  end
  else
  begin
    L_KMMedian_KMCylinderAxis:= L_KMMedian_KMCylinderAxis + ' ' + arrData[0][2];      
  end;  

  // -----------------------------------------------------------------
  //
  // -----------------------------------------------------------------

  // Build result
  ParsedData:= '';

  // Add patient ID
  if (PatientID <> '') then
  begin
    ParsedData:= ParsedData + PatientID;
  end;

  if (not bolAddPowerValueToOutput) then
  begin
    R_KMMedian_R1Power:= '';
    R_KMMedian_R2Power:= '';
    R_KMMedian_AveragePower:= '';

    L_KMMedian_R1Power:= '';
    L_KMMedian_R2Power:= '';
    L_KMMedian_AveragePower:= '';
    
    R_KMMedian_KMCylinderAxis:= '';
    L_KMMedian_KMCylinderAxis:= '';
  end;

  // Add right eye
  R_Line1_Seg1:= '';

  if (R_KMMedian_R1Radius <> '') and (R_KMMedian_R1Axis <> '') then
  begin
    R_Line1_Seg1:= R_Line1_Seg1 + R_KMMedian_R1Radius + R_KMMedian_R1Power + R_KMMedian_R1Axis;
  end;

  R_Line1_Seg2:= '';

  if (R_KMMedian_R2Radius <> '') and (R_KMMedian_R2Axis <> '') then
  begin
    R_Line1_Seg2:= R_Line1_Seg2 + R_KMMedian_R2Radius + R_KMMedian_R2Power + R_KMMedian_R2Axis;
  end;

  R_Line1:= '';

  if (R_Line1_Seg1 <> '') then
  begin
    R_Line1:= R_Line1 + 'R: ' + R_Line1_Seg1 + ' ';
  end;

  if (R_Line1_Seg2 <> '') then
  begin
    if R_Line1 = '' then
      R_Line1:= R_Line1 + 'R: ' + R_Line1_Seg2 + ' '
    else
      R_Line1:= R_Line1 + R_Line1_Seg2 + ' ';
  end;

  R_Line1:= Trim(R_Line1);

  R_Line2:= '';

  if (R_KMMedian_AverageRadius <> '') then
  begin
    R_Line2:= R_Line2 + 'R: ' + R_KMMedian_AverageRadius + R_KMMedian_AveragePower;
  end;

  if (R_KMMedian_AverageRadius <> '') then
  begin
    if R_Line2 = '' then
      R_Line2:= R_Line2 + 'R: '
    else
      R_Line2:= R_Line2 + ' ';

    R_Line2:= R_Line2 + R_KMMedian_KMCylinderPower + R_KMMedian_KMCylinderAxis;
  end;

  // Add left eye
  L_Line1_Seg1:= '';

  if (L_KMMedian_R1Radius <> '') and (L_KMMedian_R1Axis <> '') then
  begin
    L_Line1_Seg1:= L_Line1_Seg1 + L_KMMedian_R1Radius + L_KMMedian_R1Power + L_KMMedian_R1Axis;
  end;

  L_Line1_Seg2:= '';

  if (L_KMMedian_R2Radius <> '') and (L_KMMedian_R2Axis <> '') then
  begin
    L_Line1_Seg2:= L_Line1_Seg2 + L_KMMedian_R2Radius + L_KMMedian_R2Power + L_KMMedian_R2Axis;
  end;

  L_Line1:= '';

  if (L_Line1_Seg1 <> '') then
  begin
    L_Line1:= L_Line1 + '// L: ' + L_Line1_Seg1 + ' ';
  end;

  if (L_Line1_Seg2 <> '') then
  begin
    if L_Line1 = '' then
      L_Line1:= L_Line1 + '// L: ' + L_Line1_Seg2 + ' '
    else
      L_Line1:= L_Line1 + L_Line1_Seg2 + ' ';
  end;

  L_Line1:= Trim(L_Line1);

  L_Line2:= '';

  if (L_KMMedian_AverageRadius <> '') then
  begin
    L_Line2:= L_Line2 + '// L: ' + L_KMMedian_AverageRadius + L_KMMedian_AveragePower;
  end;

  if (L_KMMedian_AverageRadius <> '') then
  begin
    if L_Line2 = '' then
      L_Line2:= L_Line2 + '// L: '
    else
      L_Line2:= L_Line2 + ' ';

    L_Line2:= L_Line2 + L_KMMedian_KMCylinderPower + L_KMMedian_KMCylinderAxis;
  end;

  // Build final output
  if (R_Line1 <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + R_Line1;
  end;

  if (L_Line1 <> '') then
  begin
    if (R_Line1 = '') then
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator
    else
      ParsedData:= ParsedData + ' ';

    ParsedData:= ParsedData + L_Line1;
  end;

  if (R_Line1 <> '') or (L_Line1 <> '') then
    ParsedData:= ParsedData + FOutputLineSeparator;

  if (R_Line2 <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + R_Line2;
  end;

  if (L_Line2 <> '') then
  begin
    if R_Line2 = '' then
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator
    else
      ParsedData:= ParsedData + ' ';

    ParsedData:= ParsedData + L_Line2;
  end;

  if (R_Line2 <> '') or (L_Line2 <> '') then
  begin  
    ParsedData:= ParsedData + FOutputLineSeparator;
  end;    

  // Set output
  FParsedDataString:= RawByteString(ParsedData);
end.

