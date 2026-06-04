const
	VERSION = '1.0.14.27';
	DATE = '13.11.2020 10:37:11';
	TEXT = 'Copyright (c) 2020 team2work GmbH';
	
	XPATH_EXPRESSION_01 = '//Ophthalmology/*[name()="nsCommon:Common"]/*[name()="nsCommon:Patient"]/*[name()="nsCommon:ID"]';
	
	XPATH_EXPRESSION_02 = '//Ophthalmology/*[name()="nsREF:Measure"]/*[name()="nsREF:VD"]';
	
	XPATH_EXPRESSION_03 = '//Ophthalmology/*[name()="nsREF:Measure"]/*[name()="nsREF:REF"]/*[name()="nsREF:R"]/*[name()="nsREF:List"]/*[name()="nsREF:Sphere"]';
	XPATH_EXPRESSION_04 = '//Ophthalmology/*[name()="nsREF:Measure"]/*[name()="nsREF:REF"]/*[name()="nsREF:R"]/*[name()="nsREF:List"]/*[name()="nsREF:Cylinder"]';	
	XPATH_EXPRESSION_05 = '//Ophthalmology/*[name()="nsREF:Measure"]/*[name()="nsREF:REF"]/*[name()="nsREF:R"]/*[name()="nsREF:List"]/*[name()="nsREF:Axis"]';	
	
	XPATH_EXPRESSION_06 = '//Ophthalmology/*[name()="nsREF:Measure"]/*[name()="nsREF:REF"]/*[name()="nsREF:R"]/*[name()="nsREF:Median"]/*[name()="nsREF:Sphere"]';
	XPATH_EXPRESSION_07 = '//Ophthalmology/*[name()="nsREF:Measure"]/*[name()="nsREF:REF"]/*[name()="nsREF:R"]/*[name()="nsREF:Median"]/*[name()="nsREF:Cylinder"]';	
	XPATH_EXPRESSION_08 = '//Ophthalmology/*[name()="nsREF:Measure"]/*[name()="nsREF:REF"]/*[name()="nsREF:R"]/*[name()="nsREF:Median"]/*[name()="nsREF:Axis"]';	
	
	XPATH_EXPRESSION_09 = '//Ophthalmology/*[name()="nsREF:Measure"]/*[name()="nsREF:REF"]/*[name()="nsREF:L"]/*[name()="nsREF:List"]/*[name()="nsREF:Sphere"]';
	XPATH_EXPRESSION_10 = '//Ophthalmology/*[name()="nsREF:Measure"]/*[name()="nsREF:REF"]/*[name()="nsREF:L"]/*[name()="nsREF:List"]/*[name()="nsREF:Cylinder"]';	
	XPATH_EXPRESSION_11 = '//Ophthalmology/*[name()="nsREF:Measure"]/*[name()="nsREF:REF"]/*[name()="nsREF:L"]/*[name()="nsREF:List"]/*[name()="nsREF:Axis"]';	
	
	XPATH_EXPRESSION_12 = '//Ophthalmology/*[name()="nsREF:Measure"]/*[name()="nsREF:REF"]/*[name()="nsREF:L"]/*[name()="nsREF:Median"]/*[name()="nsREF:Sphere"]';
	XPATH_EXPRESSION_13 = '//Ophthalmology/*[name()="nsREF:Measure"]/*[name()="nsREF:REF"]/*[name()="nsREF:L"]/*[name()="nsREF:Median"]/*[name()="nsREF:Cylinder"]';	
	XPATH_EXPRESSION_14 = '//Ophthalmology/*[name()="nsREF:Measure"]/*[name()="nsREF:REF"]/*[name()="nsREF:L"]/*[name()="nsREF:Median"]/*[name()="nsREF:Axis"]';		
	
	XPATH_EXPRESSION_15 = '//Ophthalmology/*[name()="nsREF:Measure"]/*[name()="nsREF:PD"]/*[name()="nsREF:Distance"]';

        // Gibt die Listeneinträge an, die verwendet werden sollen,
        // falls keine Mittelwerteinträge in der Messung enthalten sind
        USE_R_LIST_ENTRY = 0;
        USE_L_LIST_ENTRY = 0;
	
	GDT_FID_PATIENT_ID = '3000';
	GDT_FID_MEASURE_DATA = '6228';
	GDT_FID_COMMENT = '6227';
		
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
	S, VD, R_S, R_Z, R_Axis, L_S, L_Z, L_Axis, PD: String;
	R_Line, L_Line: String;
        bolAddVDValueToOutput, bolAddPDValueToOutput: Boolean;
        bolAddGDTLinePrefix, bolAddSign, bolAddSignSeparator, bolAddAxisSeparator: Boolean;
begin
        // Verwende "True", damit der VD Wert an die V1 Zeile angehängt wird,
        // benutze "False", damit der VD Wert ignoriert werden kann.
        bolAddVDValueToOutput := True;

        // Verwende "True", damit der PD Wert an die V1 Zeile angehängt wird,
        // benutze "False", damit der PD Wert ignoriert werden kann.
        bolAddPDValueToOutput := True;
              
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
	
	if (Length(arrData) > 0) then
	begin
	  S:= arrData[0][2];
	  
	  if (S <> '') and (S <> '-') then
	  begin
	    PatientID:= PatientID + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
	    PatientID:= PatientID + S + FOutputLineSeparator;
	  end;
	end;
	
	// VD
	VD:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_02);
	
	if Length(arrData) > 0 then
	begin	
	  VD:= VD + 'VD= ' + arrData[0][2];
	end;

	// R_S
	R_S:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_06);
  
        if Length(arrData) <= 0 then
	begin
          arrData:= DoPSGetXMLData(XPATH_EXPRESSION_03);
  
          if Length(arrData) > USE_R_LIST_ENTRY then
          begin
            R_S:= R_S + FormatSignValue('S=', arrData[USE_R_LIST_ENTRY][2], bolAddSign, bolAddSignSeparator);
          end;
        end
        else
	begin
	  R_S:= R_S + FormatSignValue('S=', arrData[0][2], bolAddSign, bolAddSignSeparator); 
	end;
	
	// R_Z
	R_Z:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_07);
	
	if Length(arrData) <= 0 then
	begin
          arrData:= DoPSGetXMLData(XPATH_EXPRESSION_04);
  
          if Length(arrData) > USE_R_LIST_ENTRY then
          begin
            R_Z:= R_Z + FormatSignValue('Z=', arrData[USE_R_LIST_ENTRY][2], bolAddSign, bolAddSignSeparator);
         end;
        end
        else
	begin
	  R_Z:= R_Z + FormatSignValue('Z=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end;

	// R_Axis
	R_Axis:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_08);
	
	if Length(arrData) <= 0 then
        begin
          arrData:= DoPSGetXMLData(XPATH_EXPRESSION_05);
  
          if Length(arrData) > USE_R_LIST_ENTRY then
          begin
            R_Axis:= R_Axis + FormatAxisValue('*', arrData[USE_R_LIST_ENTRY][2], bolAddAxisSeparator);
          end;
        end
        else
	begin
	  R_Axis:= R_Axis + FormatAxisValue('*', arrData[0][2], bolAddAxisSeparator);
	end;
	
	// L_S
	L_S:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_12);
	
	if Length(arrData) <= 0 then
        begin
          arrData:= DoPSGetXMLData(XPATH_EXPRESSION_09);
  
          if Length(arrData) > USE_L_LIST_ENTRY then
          begin
            L_S:= L_S + FormatSignValue('S=', arrData[USE_L_LIST_ENTRY][2], bolAddSign, bolAddSignSeparator);
          end;
        end
        else
	begin
	  L_S:= L_S + FormatSignValue('S=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end;
	
	// L_Z
	L_Z:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_13);
	
	if Length(arrData) <= 0 then
        begin
          arrData:= DoPSGetXMLData(XPATH_EXPRESSION_10);
  
          if Length(arrData) > USE_L_LIST_ENTRY then
          begin
            L_Z:= L_Z + FormatSignValue('Z=', arrData[USE_L_LIST_ENTRY][2], bolAddSign, bolAddSignSeparator);
          end;
        end
        else
	begin
	  L_Z:= L_Z + FormatSignValue('Z=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end;

	// L_Axis
	L_Axis:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_14);
	
	if Length(arrData) <= 0 then
        begin
          arrData:= DoPSGetXMLData(XPATH_EXPRESSION_11);
  
          if Length(arrData) > USE_L_LIST_ENTRY then
          begin
            L_Axis:= L_Axis + FormatAxisValue('*', arrData[USE_L_LIST_ENTRY][2], bolAddAxisSeparator);
          end;
        end
        else
	begin
	  L_Axis:= L_Axis + FormatAxisValue('*', arrData[0][2], bolAddAxisSeparator);
	end;

	// PD
	PD:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_15);
	
	if Length(arrData) > 0 then
	begin
	  PD:= PD + 'PD= ' + arrData[0][2];
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
	  R_Line:= R_Line + 'R.:' + R_S + ' ' + R_Z + R_Axis;
	end;
	
	// Add PD
	if (PD <> '') and (bolAddPDValueToOutput) then
	begin
	  if R_Line <> '' then
	  begin
	    R_Line:= R_Line + ' ';
	  end;
		
	  R_Line:= R_Line + PD;
	end;
	
	// Add VD
	if (VD <> '') and (bolAddVDValueToOutput) then
	begin
	  if R_Line <> '' then
	  begin
	    R_Line:= R_Line + ' ';
	  end;
		
	  R_Line:= R_Line + VD;
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
	  L_Line:= L_Line + 'L.:' + L_S + ' ' + L_Z + L_Axis;
	end;
	
	if L_Line <> '' then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
	end;
	  	
	// Set output
	FParsedDataString:= RawByteString(ParsedData);
end.
