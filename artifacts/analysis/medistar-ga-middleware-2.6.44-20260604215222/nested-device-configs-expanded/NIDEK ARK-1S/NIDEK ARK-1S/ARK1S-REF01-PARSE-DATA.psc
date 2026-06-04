const
	VERSION = '1.0.25.40';
	DATE = '26.04.2024 10:26:51';
	TEXT = 'Copyright (c) 2024 team2work GmbH';
	
	XPATH_EXPRESSION_01 = '//Data/Patient/ID';
	XPATH_EXPRESSION_02 = '//Data/VD';
	XPATH_EXPRESSION_03 = '//Data/R/AR/ARMedian/Sphere';
	XPATH_EXPRESSION_04 = '//Data/R/AR/ARMedian/Cylinder';
	XPATH_EXPRESSION_05 = '//Data/R/AR/ARMedian/Axis';
	XPATH_EXPRESSION_06 = '//Data/R/AR/RingImage';
	XPATH_EXPRESSION_07 = '//Data/L/AR/ARMedian/Sphere';
	XPATH_EXPRESSION_08 = '//Data/L/AR/ARMedian/Cylinder';
	XPATH_EXPRESSION_09 = '//Data/L/AR/ARMedian/Axis';
	XPATH_EXPRESSION_10 = '//Data/L/AR/RingImage';
	XPATH_EXPRESSION_11 = '//Data/PD/PDList/FarPD';
	XPATH_EXPRESSION_12 = '//Data/R/AR/ARList/Sphere';
	XPATH_EXPRESSION_13 = '//Data/R/AR/ARList/Cylinder';
	XPATH_EXPRESSION_14 = '//Data/R/AR/ARList/Axis';
	XPATH_EXPRESSION_15 = '//Data/L/AR/ARList/Sphere';
	XPATH_EXPRESSION_16 = '//Data/L/AR/ARList/Cylinder';
	XPATH_EXPRESSION_17 = '//Data/L/AR/ARList/Axis';
	XPATH_EXPRESSION_18 = '//Data/R/VA/BCVA';
	XPATH_EXPRESSION_19 = '//Data/L/VA/BCVA';
  XPATH_EXPRESSION_20 = '//Data/R/VA/LVA';
  XPATH_EXPRESSION_21 = '//Data/L/VA/LVA';
  XPATH_EXPRESSION_22 = '//Data/R/VA/GVA';
  XPATH_EXPRESSION_23 = '//Data/L/VA/GVA';
	XPATH_EXPRESSION_24 = '//Data/R/SR/Sphere';
	XPATH_EXPRESSION_25 = '//Data/R/SR/Cylinder';
	XPATH_EXPRESSION_26 = '//Data/R/SR/Axis'; 
	XPATH_EXPRESSION_27 = '//Data/L/SR/Sphere';
	XPATH_EXPRESSION_28 = '//Data/L/SR/Cylinder';
	XPATH_EXPRESSION_29 = '//Data/L/SR/Axis';

  // Gibt die Listeneinträge an, die verwendet werden sollen,
  // falls keine Mittelwerteinträge in der Messung enthalten sind
  USE_R_LIST_ENTRY = 0;
  USE_L_LIST_ENTRY = 0;

	GDT_FID_FILE_ARCHIVE_NUMBER = '6302';
	GDT_FID_FILE_FORMAT = '6303';
	GDT_FID_FILE_DESCRIPTION = '6304';
	GDT_FID_FILE_URL = '6305';
	GDT_FID_PATIENT_ID = '3000';
	GDT_FID_RESULT = '6220';
  GDT_FID_FOREIGN_RESULT = '6221';
	GDT_FID_MEASURE_DATA = '6228';
	GDT_FID_COMMENT = '6227';
	GDT_FID_SIGNATURE = '8990';
	GDT_FID_DIAG='6205';
	
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
	ParsedData, S, PatientID: String;
	VD, R_S, R_Z, R_Axis, R_Image, L_S, L_Z, L_Axis, L_Image, PD, R_CC, L_CC, R_LVA, L_LVA, R_GVA, L_GVA: String;
	R_Line, L_Line, VisusLine, Subj_R_Line, Subj_L_Line, Visus_R_CC, Visus_L_CC: String;
	SUBJ_R_S, SUBJ_R_Z, SUBJ_R_Axis, SUBJ_L_S, SUBJ_L_Z, SUBJ_L_Axis: String;
  bolAddExternalFilesToGdtFile, bolAddVDValueToOutput, bolAddPDValueToOutput, bolAddCCValuesToOutput, bolAddGvaAndLvaToOutput: Boolean;
  bolAddSign, bolAddSignSeparator, bolAddAxisSeparator, bolAddVisusValuesToOutput: Boolean;
begin
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

	// Aktiviere den Import aller externen Bildquellen.
	// Verwende "True", damit der Import aktiviert ist, benutze "False", damit Bilder
	// nicht importiert und per GDT weiterverarbeitet werden.
  bolAddExternalFilesToGdtFile:= False;

  // Verwende "True", damit der VD Wert an die V1 Zeile angehängt wird,
  // benutze "False", damit der VD Wert ignoriert werden kann.
  bolAddVDValueToOutput := True;

  // Verwende "True", damit der PD Wert an die V1 Zeile angehängt wird,
  // benutze "False", damit der PD Wert ignoriert werden kann.
  bolAddPDValueToOutput := True;

  // Verwende "True", damit der CC Wert an die V5 Zeile angehängt wird,
  // benutze "False", damit der CC Wert ignoriert werden kann.
  bolAddCCValuesToOutput := False;

  // Verwendet "True", damit der GVA und LVA Wert and die V1 Zeile angehängt wird,
  // benutze "False", damit beide Werte ignoriert werden kann.
  bolAddGvaAndLvaToOutput := True;

  // Verwende "True", damit der Visus CC Wert an die V Zeile angehängt wird,
  // benutze "False", damit der Visus CC Wert ignoriert werden kann.
  bolAddVisusValuesToOutput := True;
     
	// --- Don't edit script down below ---

	// Clear parsed data string
	FParsedDataString:= '';

	if (not DoPSXMLDocumentExists) then
	begin
	  FLastErrorCode:= -4;
	  FLastErrorMessage:= 'Keine XML-Daten für die Verarbeitung verfügbar';

	  DoPSError;

	  Exit;
	end;

	if (not DoPSXMLRootNodeExists) then
	begin
	  FLastErrorCode:= -5;
	  FLastErrorMessage:= 'Kein XML-Wurzelknoten für die Verarbeitung verfügbar';

	  DoPSError;

	  Exit;
	end;
	
	// Set default values
	R_Line:= '';
	L_Line:= '';
	VisusLine:= '';
	Subj_R_Line:= '';
	Subj_L_Line:= '';
	
	// Patient ID
	PatientID:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_01);
	
	if Length(arrData) > 0 then
	begin
	  PatientID:= PatientID + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
	  PatientID:= PatientID + arrData[0][2] + FOutputLineSeparator;
	end;
	
	// VD
	VD:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_02);
	
	if Length(arrData) > 0 then
	begin
	  S:= T2WStringReplace(arrData[0][2], ' mm', '', False, False);
		
	  VD:= VD + 'VD= ' + S;
	end;
	
	// R_S
	R_S:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_03);
  
  if Length(arrData) <= 0 then
	begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_12);

    if Length(arrData) > USE_R_LIST_ENTRY then
    begin
      R_S:= R_S + FormatSignValue('S=', Trim(arrData[USE_R_LIST_ENTRY][2]), bolAddSign, bolAddSignSeparator);
    end;
  end
  else
	begin
	  R_S:= R_S + FormatSignValue('S=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
	end;
	
	// R_Z
	R_Z:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_04);
	
	if Length(arrData) <= 0 then
	begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_13);

    if Length(arrData) > USE_R_LIST_ENTRY then
    begin
      R_Z:= R_Z + FormatSignValue('Z=', Trim(arrData[USE_R_LIST_ENTRY][2]), bolAddSign, bolAddSignSeparator);
    end;
  end
  else
	begin
	  R_Z:= R_Z + FormatSignValue('Z=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
	end;
	
	// R_Axis
	R_Axis:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_05);
	
	if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_14);

    if Length(arrData) > USE_R_LIST_ENTRY then
    begin
      R_Axis:= R_Axis + FormatAxisValue('*', Trim(arrData[USE_R_LIST_ENTRY][2]), bolAddAxisSeparator);
    end;
  end
  else
	begin
	  R_Axis:= R_Axis + FormatAxisValue('*', Trim(arrData[0][2]), bolAddAxisSeparator);
	end;
  
  // R_CC
  R_CC:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_18);
	
	if Length(arrData) > 0 then
	begin
    R_CC:= R_CC + 'RA' + ' ' + 'CC:' + arrData[0][2];
    
    Visus_R_CC:= arrData[0][2];
  end;
  
  // R_LVA
  R_LVA:= '';
        
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_20);
	
	if Length(arrData) > 0 then
	begin
    R_LVA:= R_LVA + 'LVA=' + ' ' + arrData[0][2];
  end;
  
  // R_GVA
  R_GVA:= '';
  
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_22);
	
	if Length(arrData) > 0 then
	begin
    R_GVA:= R_GVA + 'GVA=' + ' ' + arrData[0][2];
  end;       
        	
	// R_Image
	R_Image:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_06);
	
	if Length(arrData) > 0 then
	begin
	  R_Image:= R_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_ARCHIVE_NUMBER + FOutputLineSeparator + '0001' + FOutputLineSeparator;
	  R_Image:= R_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_FORMAT + FOutputLineSeparator + 'JPG' + FOutputLineSeparator;
	  R_Image:= R_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_DESCRIPTION + FOutputLineSeparator + 'Bild Messung' + FOutputLineSeparator;
	  R_Image:= R_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_URL + FOutputLineSeparator + arrData[0][2] + FOutputLineSeparator;
	end;
	
	// SUBJ_R_S
	SUBJ_R_S:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_24);
	
	if Length(arrData) > 0 then
	begin
    SUBJ_R_S:= SUBJ_R_S + FormatSignValue('S=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
  end;  
	
	// SUBJ_R_Z
	SUBJ_R_Z:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_25);
	
	if Length(arrData) > 0 then
	begin
    SUBJ_R_Z:= SUBJ_R_Z + FormatSignValue('Z=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
  end; 
	
	// SUBJ_R_Axis
	SUBJ_R_Axis:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_26);
	
	if Length(arrData) > 0 then
	begin
	  SUBJ_R_Axis:= SUBJ_R_Axis + FormatAxisValue('*', Trim(arrData[0][2]), bolAddAxisSeparator);
  end; 	

	// ---------------------------------------------------
	
	// L_S
	L_S:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_07);
	
	if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_15);

    if Length(arrData) > USE_L_LIST_ENTRY then
    begin
      L_S:= L_S + FormatSignValue('S=', Trim(arrData[USE_L_LIST_ENTRY][2]), bolAddSign, bolAddSignSeparator);
    end;
  end
  else
	begin
	  L_S:= L_S + FormatSignValue('S=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
	end;
	
	// L_Z
	L_Z:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_08);
	
	if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_16);

    if Length(arrData) > USE_L_LIST_ENTRY then
    begin
      L_Z:= L_Z + FormatSignValue('Z=', Trim(arrData[USE_L_LIST_ENTRY][2]), bolAddSign, bolAddSignSeparator);;
    end;
  end
  else
	begin
	  L_Z:= L_Z + FormatSignValue('Z=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
	end;
	
	// L_Axis
	L_Axis:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_09);
	
	if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_17);

    if Length(arrData) > USE_L_LIST_ENTRY then
    begin
      L_Axis:= L_Axis + FormatAxisValue('*', Trim(arrData[USE_L_LIST_ENTRY][2]), bolAddAxisSeparator);
    end;
  end
  else
	begin
	  L_Axis:= L_Axis + FormatAxisValue('*', Trim(arrData[0][2]), bolAddAxisSeparator);
	end;
  
  // L_CC
  L_CC:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_19);
	
	if Length(arrData) > 0 then
	begin
    L_CC:= L_CC + 'LA' + ' ' + 'CC:' + arrData[0][2];
    
    Visus_L_CC:= arrData[0][2];
  end;
  
  // L_LVA
  L_LVA:= '';
        
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_21);
	
	if Length(arrData) > 0 then
  begin
    L_LVA:= L_LVA + 'LVA=' + ' ' + arrData[0][2];
  end;  
  
  // L_GVA
  L_GVA:= '';
  
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_23);
	
	if Length(arrData) > 0 then
	begin
    L_GVA:= L_GVA + 'GVA=' + ' ' + arrData[0][2];
  end;       

	// L_Image
	L_Image:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_10);
	
	if Length(arrData) > 0 then
	begin
	  L_Image:= L_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_ARCHIVE_NUMBER + FOutputLineSeparator + '0002' + FOutputLineSeparator;
	  L_Image:= L_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_FORMAT + FOutputLineSeparator + 'JPG' + FOutputLineSeparator;
	  L_Image:= L_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_DESCRIPTION + FOutputLineSeparator + 'Bild Messung' + FOutputLineSeparator;
	  L_Image:= L_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_URL + FOutputLineSeparator + arrData[0][2] + FOutputLineSeparator;
	end;
	
	// SUBJ_L_S
	SUBJ_L_S:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_27);
	
	if Length(arrData) > 0 then
	begin
    SUBJ_L_S:= SUBJ_L_S + FormatSignValue('S=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
  end;
	
	// SUBJ_L_Z
	SUBJ_L_Z:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_28);
	
	if Length(arrData) > 0 then
	begin
    SUBJ_L_Z:= SUBJ_L_Z + FormatSignValue('Z=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
  end;
	
	// SUBJ_L_Axis
	SUBJ_L_Axis:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_29);
	
	if Length(arrData) > 0 then
	begin
	  SUBJ_L_Axis:= SUBJ_L_Axis + FormatAxisValue('*', Trim(arrData[0][2]), bolAddAxisSeparator);
  end;

	// PD
	PD:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_11);
	
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

	if (L_Line <> '') then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
	end;

	// Add images
	if (R_Image <> '') and (bolAddExternalFilesToGdtFile) then
	begin
	  ParsedData:= ParsedData + R_Image;
	end;
	
	if (L_Image <> '') and (bolAddExternalFilesToGdtFile) then
	begin
	  ParsedData:= ParsedData + L_Image;
	end;
  
  // Add CC
	if ((R_CC <> '') or (L_CC <> '')) 
	and (bolAddCCValuesToOutput) then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_COMMENT + FOutputLineSeparator;

    if R_CC <> '' then
      ParsedData:= ParsedData + R_CC;

    if L_CC <> '' then
    begin
      if R_CC <> '' then
        ParsedData:= ParsedData + ' ';

      ParsedData:= ParsedData + L_CC;
    end;

    ParsedData:= ParsedData + FOutputLineSeparator;
	end;
	
	// Add LVA and GVA
	if (bolAddGvaAndLvaToOutput) then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_SIGNATURE + FOutputLineSeparator;
	
    if (R_LVA <> '') or (R_GVA <> '') then
    begin
      ParsedData:= Trim(ParsedData + 'R.:' + R_GVA + ' ' + R_LVA); 
    end;
    
    if (L_LVA <> '') or (L_GVA <> '') then
    begin
      ParsedData:= Trim(ParsedData + ' // L.:' + L_GVA + ' ' + L_LVA);       
    end;          
	
	  ParsedData:= ParsedData + FOutputLineSeparator;
	end;
	
	// Add visus
	if (Visus_R_CC <> '') or (Visus_L_CC <> '') then
	begin
	  VisusLine:= VisusLine + 'R:cc=' + Visus_R_CC + ' //' + 'L:cc=' + Visus_L_CC;
	end;
	
  if (VisusLine <> '') 
  and (bolAddVisusValuesToOutput) then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_DIAG + FOutputLineSeparator;
    ParsedData:= ParsedData + Trim(VisusLine) + FOutputLineSeparator;
  end;
  
	// Add right eye subj
	SUBJ_R_Line:= '';
	
	if (SUBJ_R_S <> '') and (SUBJ_R_Z <> '') and (SUBJ_R_Axis <> '') then
	begin
	  SUBJ_R_Line:= SUBJ_R_Line + 'R.:' + SUBJ_R_S + ' ' + SUBJ_R_Z + SUBJ_R_Axis;
	end;
  
	if (SUBJ_R_Line <> '') then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_RESULT + FOutputLineSeparator;
	  ParsedData:= ParsedData + SUBJ_R_Line + FOutputLineSeparator;
	end;

	// Add left eye subj
	SUBJ_L_Line:= '';
	
	if (SUBJ_L_S <> '') and (SUBJ_L_Z <> '') and (SUBJ_L_Axis <> '') then
	begin
	  SUBJ_L_Line:= SUBJ_L_Line + 'L.:' + SUBJ_R_S + ' ' + SUBJ_L_Z + SUBJ_L_Axis;
	end;
	
	if (SUBJ_L_Line <> '') then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FOREIGN_RESULT + FOutputLineSeparator;
	  ParsedData:= ParsedData + SUBJ_L_Line + FOutputLineSeparator;
	end;

	// Set output
	FParsedDataString:= RawByteString(ParsedData);
end.
