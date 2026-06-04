const
	VERSION = '1.0.14.40';
	DATE = '19.11.2020 16:48:11';
	TEXT = 'Copyright (c) 2020 team2work GmbH';
		
	XPATH_EXPRESSION_01 = '//Data/Patient/ID';
	
	XPATH_EXPRESSION_02 = '//Data/R/SR/Sphere';
	XPATH_EXPRESSION_03 = '//Data/R/SR/Cylinder';	
	XPATH_EXPRESSION_04 = '//Data/R/SR/Axis';
	XPATH_EXPRESSION_05 = '//Data/R/SR/SE';
	XPATH_EXPRESSION_06 = '//Data/R/SR/ADD';	

	XPATH_EXPRESSION_07 = '//Data/L/SR/Sphere';
	XPATH_EXPRESSION_08 = '//Data/L/SR/Cylinder';
	XPATH_EXPRESSION_09 = '//Data/L/SR/Axis';
	XPATH_EXPRESSION_10 = '//Data/L/SR/SE';
	XPATH_EXPRESSION_11 = '//Data/L/SR/ADD';	

	GDT_FID_PATIENT_ID = '3000';
	GDT_FID_MEASURE_DATA = '6228';
		
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
	R_S, R_Z, R_Axis, R_SE, R_ADD, L_S, L_Z, L_Axis, L_SE, L_ADD: String;
	R_Line, L_Line: String;
        bolAddGDTLinePrefix, bolAddSign, bolAddSignSeparator, bolAddAxisSeparator: Boolean;
        
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
	R_S:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_02);
  
        if Length(arrData) > 0 then
	begin
	  R_S:= R_S + FormatSignValue('S=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end;
	
	// R_Z
	R_Z:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_03);
	
	if Length(arrData) > 0 then
	begin
	  R_Z:= R_Z + FormatSignValue('Z=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end;
	
	// R_Axis
	R_Axis:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_04);
	
	if Length(arrData) > 0 then
        begin
	  R_Axis:= R_Axis + FormatAxisValue('*', arrData[0][2], bolAddAxisSeparator);
	end;
  	
	// R_SE
	R_SE:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_05);
	
	if Length(arrData) > 0 then
	begin
	  R_SE:= R_SE + FormatSignValue('SE=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end; 
	
	// R_ADD
	R_ADD:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_06);
	
	if Length(arrData) > 0 then
	begin
	  R_ADD:= R_ADD + FormatSignValue('A=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end;
  	  		
	// L_S
	L_S:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_07);
	
	if Length(arrData) > 0 then
        begin
	  L_S:= L_S + FormatSignValue('S=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end;
	
	// L_Z
	L_Z:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_08);
	
	if Length(arrData) > 0 then
        begin
	  L_Z:= L_Z + FormatSignValue('Z=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end;
	
	// L_Axis
	L_Axis:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_09);
	
	if Length(arrData) > 0 then
        begin
	  L_Axis:= L_Axis + FormatAxisValue('*', arrData[0][2], bolAddAxisSeparator);
	end;
	
	// L_SE
	L_SE:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_10);
	
	if Length(arrData) > 0 then
	begin
	  L_SE:= L_SE + FormatSignValue('SE=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end; 
	
	// L_ADD
	L_ADD:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_11);
	
	if Length(arrData) > 0 then
	begin
	  L_ADD:= L_ADD + FormatSignValue('A=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end;
	
	// Build result
	ParsedData:= '';

	// Add patient ID
	if PatientID <> '' then
	begin
	  ParsedData:= ParsedData + PatientID;
	end;

	// Add right eye
	R_Line:= '';
	
	if (R_S <> '') and (R_Z <> '') and (R_Axis <> '') then
	begin
          if bolAddGDTLinePrefix then
            R_Line:= R_Line + GDT_LINE_PREFIX;
      	
	  R_Line:= R_Line + Trim('R.:' + R_S + ' ' + R_Z + R_Axis + ' ' + R_ADD + ' ' + R_SE);
	end;
			
	if R_Line <> '' then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + R_Line + FOutputLineSeparator;
	end;
	
	// Add left eye
	L_Line:= '';
	
	if (L_S <> '') and (L_Z <> '') and (L_Axis <> '') then
	begin
          if bolAddGDTLinePrefix then
            L_Line:= L_Line + GDT_LINE_PREFIX;	
	
	  L_Line:= L_Line + Trim('L.:' + L_S + ' ' + L_Z + L_Axis + ' ' + L_ADD + ' ' + L_SE);
	end;
	
	if L_Line <> '' then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
	end;

	// Set output
	FParsedDataString:= RawByteString(ParsedData);
end.
