const
	VERSION = '1.0.55.39';
	DATE = '11.11.2022 09:03:34';
	TEXT = 'Copyright (c) 2022 team2work GmbH';

  // Depending on the device configuration, the measurement file can use Latin terms 
  // instead of the identifiers <R> for right and <L> for left. 
  // These would then be: <I> iustum (right) and <S> sinistram (left).		

  // Some Firmware versions use <S> and <I> if only one eye (glasses) was measured
  // If both eyes (glasses) where measured, <R> and <L> will be used

  // R = I (iustum) and L = S (sinistra)
  DEFAULT_IDENTIFIER_RIGHT    = 'R';
  DEFAULT_IDENTIFIER_LEFT     = 'L';
 
  DEFAULT_IDENTIFIER_IUSTUM   = 'I';
  DEFAULT_IDENTIFIER_SINISTRA = 'S';

  // PatientID
  XPATH_EXPRESSION_01 = '//Ophthalmology/Common/Patient/ID';
		
  // R
  XPATH_EXPRESSION_02 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_RIGHT + '/Sphare';
  XPATH_EXPRESSION_03 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_RIGHT + '/Cylinder';	
  XPATH_EXPRESSION_04 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_RIGHT + '/Axis';		
  XPATH_EXPRESSION_05 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_RIGHT + '/ADD';	
  XPATH_EXPRESSION_06 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_RIGHT + '/ADD2';
  XPATH_EXPRESSION_07 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_RIGHT + '/Prism';
  XPATH_EXPRESSION_08 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_RIGHT + '/PrismBase';
  XPATH_EXPRESSION_09 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_RIGHT + '/PrismX';
  XPATH_EXPRESSION_10 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_RIGHT + '/PrismY';
  XPATH_EXPRESSION_11 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_RIGHT + '/UVTransmittance';
  XPATH_EXPRESSION_12 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_RIGHT + '/Sphere';

  // L
  XPATH_EXPRESSION_13 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_LEFT + '/Sphare';
  XPATH_EXPRESSION_14 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_LEFT + '/Cylinder';	
  XPATH_EXPRESSION_15 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_LEFT + '/Axis';		
  XPATH_EXPRESSION_16 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_LEFT + '/ADD';	
  XPATH_EXPRESSION_17 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_LEFT + '/ADD2';
  XPATH_EXPRESSION_18 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_LEFT + '/Prism';
  XPATH_EXPRESSION_19 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_LEFT + '/PrismBase';
  XPATH_EXPRESSION_20 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_LEFT + '/PrismX';
  XPATH_EXPRESSION_21 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_LEFT + '/PrismY';
  XPATH_EXPRESSION_22 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_LEFT + '/UVTransmittance';
  XPATH_EXPRESSION_23 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_LEFT + '/Sphere';
    
  // I
  XPATH_EXPRESSION_24 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_IUSTUM + '/Sphare';
  XPATH_EXPRESSION_25 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_IUSTUM + '/Cylinder';	
  XPATH_EXPRESSION_26 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_IUSTUM + '/Axis';		
  XPATH_EXPRESSION_27 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_IUSTUM + '/ADD';	
  XPATH_EXPRESSION_28 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_IUSTUM + '/ADD2';
  XPATH_EXPRESSION_29 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_IUSTUM + '/Prism';
  XPATH_EXPRESSION_30 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_IUSTUM + '/PrismBase';
  XPATH_EXPRESSION_31 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_IUSTUM + '/PrismX';
  XPATH_EXPRESSION_32 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_IUSTUM + '/PrismY';
  XPATH_EXPRESSION_33 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_IUSTUM + '/UVTransmittance';
  XPATH_EXPRESSION_34 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_IUSTUM + '/Sphere';  
  
  // S
  XPATH_EXPRESSION_35 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_SINISTRA + '/Sphare';
  XPATH_EXPRESSION_36 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_SINISTRA + '/Cylinder';	
  XPATH_EXPRESSION_37 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_SINISTRA + '/Axis';		
  XPATH_EXPRESSION_38 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_SINISTRA + '/ADD';	
  XPATH_EXPRESSION_39 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_SINISTRA + '/ADD2';
  XPATH_EXPRESSION_40 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_SINISTRA + '/Prism';
  XPATH_EXPRESSION_41 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_SINISTRA + '/PrismBase';
  XPATH_EXPRESSION_42 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_SINISTRA + '/PrismX';
  XPATH_EXPRESSION_43 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_SINISTRA + '/PrismY';
  XPATH_EXPRESSION_44 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_SINISTRA + '/UVTransmittance';
  XPATH_EXPRESSION_45 = '//Ophthalmology/Measure[@type=''' + 'LM' + ''']/LM/' + DEFAULT_IDENTIFIER_SINISTRA + '/Sphere';    
    
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

var
  arrData: TStringArrayArray;	
  bolAddExternalFilesToGdtFile, bolAddSign, bolAddSignSeparator, bolAddAxisSeparator: Boolean;
  bolAddPrismAndBaseToOutput, bolAddPrismXandYToOutput: Boolean;
  ParsedData, PatientID: String;	
  R_Line, L_Line: String;
  R_S, R_Z, R_Axis, R_Add, R_Add2, R_PRISM, R_PRISMBASE, R_PRISMX, R_PRISMY, R_PRISMX_BASE, R_PRISMY_BASE, R_UVTransmittance: String;
  L_S, L_Z, L_Axis, L_Add, L_Add2, L_PRISM, L_PRISMBASE, L_PRISMX, L_PRISMY, L_PRISMX_BASE, L_PRISMY_BASE, L_UVTransmittance: String;

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
	
  // Some Firmware versions use <S> and <I> if only one eye (glasses) was measured
  // If both eyes (glasses) where measured, <R> and <L> will be used 

	//
	// RIGHT / IUSTUM
	//

	// R_S
	R_S:= '';

	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_02);

	if Length(arrData) > 0 then
	begin
	  R_S:= R_S + FormatSignValue('S=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end
	else
	begin
	  // There seem to be firmware versions in circulation in which the node 
	  // for the sphere value is written "wrong" in the export XML file.
	  // The <Sphare> node is exported instead of <Sphere>.
	  SetLength(arrData, 0);
	  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_12);	  
	  
	  if Length(arrData) > 0 then
	  begin
	    R_S:= R_S + FormatSignValue('S=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	  end; 	  
	end;
	
	if (R_S = '') then
	begin
		SetLength(arrData, 0);
		arrData:= DoPSGetXMLData(XPATH_EXPRESSION_24);

		if Length(arrData) > 0 then
		begin
		  R_S:= R_S + FormatSignValue('S=', arrData[0][2], bolAddSign, bolAddSignSeparator);
		end
		else
		begin
		  // There seem to be firmware versions in circulation in which the node 
		  // for the sphere value is written "wrong" in the export XML file.
		  // The <Sphare> node is exported instead of <Sphere>.
		  SetLength(arrData, 0);
		  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_34);	  
		  
		  if Length(arrData) > 0 then
		  begin
		    R_S:= R_S + FormatSignValue('S=', arrData[0][2], bolAddSign, bolAddSignSeparator);
		  end; 	  
		end;
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
		SetLength(arrData, 0);
		arrData:= DoPSGetXMLData(XPATH_EXPRESSION_25);

		if Length(arrData) > 0 then
		begin
		  R_Z:= R_Z + FormatSignValue('Z=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
		end;
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
		SetLength(arrData, 0);
		arrData:= DoPSGetXMLData(XPATH_EXPRESSION_26);

		if Length(arrData) > 0 then
		begin
		  R_Axis:= R_Axis + FormatAxisValue('*', arrData[0][2], bolAddAxisSeparator);
		end;
	end;

	// R_ADD
	R_Add:= '';

	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_05);

	if Length(arrData) > 0 then
	begin
	  R_Add:= R_Add + FormatSignValue('A=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end;
	
	if (R_Add = '') then
	begin
		SetLength(arrData, 0);
		arrData:= DoPSGetXMLData(XPATH_EXPRESSION_27);

		if Length(arrData) > 0 then
		begin
		  R_Add:= R_Add + FormatSignValue('A=', arrData[0][2], bolAddSign, bolAddSignSeparator);
		end;
	end;
	
	// 
	// R_ADD2
	R_Add2:= '';

	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_06);

	if Length(arrData) > 0 then
	begin
	  R_Add2:= R_Add2 + FormatSignValue('A2=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end;
	
	if (R_Add2 = '') then
	begin
		SetLength(arrData, 0);
		arrData:= DoPSGetXMLData(XPATH_EXPRESSION_28);

		if Length(arrData) > 0 then
		begin
		  R_Add2:= R_Add2 + FormatSignValue('A2=', arrData[0][2], bolAddSign, bolAddSignSeparator);
		end;
	end;
		
	// R_PRISM	
	R_PRISM:= '';

	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_07);

	if Length(arrData) > 0 then
	begin
	  R_PRISM:= R_PRISM + FormatSignValue('P=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end;
	
	if (R_PRISM = '') then
	begin
		SetLength(arrData, 0);
		arrData:= DoPSGetXMLData(XPATH_EXPRESSION_29);

		if Length(arrData) > 0 then
		begin
		  R_PRISM:= R_PRISM + FormatSignValue('P=', arrData[0][2], bolAddSign, bolAddSignSeparator);
		end;
	end;

	// R_PRISMBASE
	R_PRISMBASE:= '';

	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_08);

	if Length(arrData) > 0 then
	begin
	  R_PRISMBASE:= R_PRISMBASE + FormatSignValue('B=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end;
	
	if (R_PRISMBASE = '') then
	begin
		SetLength(arrData, 0);
		arrData:= DoPSGetXMLData(XPATH_EXPRESSION_30);

		if Length(arrData) > 0 then
		begin
		  R_PRISMBASE:= R_PRISMBASE + FormatSignValue('B=', arrData[0][2], bolAddSign, bolAddSignSeparator);
		end;
	end;
 
	// R_PRISMX
	R_PRISMX:= '';

	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_09);

	if Length(arrData) > 0 then
	begin
	  R_PRISMX:= R_PRISMX + FormatSignValue('P=', arrData[0][2], False, bolAddSignSeparator);
	end;
	
	if (R_PRISMX = '') then
	begin
		SetLength(arrData, 0);
		arrData:= DoPSGetXMLData(XPATH_EXPRESSION_31);

		if Length(arrData) > 0 then
		begin
		  R_PRISMX:= R_PRISMX + FormatSignValue('P=', arrData[0][2], False, bolAddSignSeparator);
		end;
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
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_10);

	if Length(arrData) > 0 then
	begin
	  R_PRISMY:= R_PRISMY + FormatSignValue('', arrData[0][2], False, bolAddSignSeparator);
	end;
	
	if (R_PRISMY = '') then
	begin
		SetLength(arrData, 0);
		arrData:= DoPSGetXMLData(XPATH_EXPRESSION_32);

		if Length(arrData) > 0 then
		begin
		  R_PRISMY:= R_PRISMY + FormatSignValue('', arrData[0][2], False, bolAddSignSeparator);
		end;
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
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_11);

	if Length(arrData) > 0 then
	begin
	  R_UVTransmittance:= R_UVTransmittance + 'R: UV = ' + arrData[0][2] + arrData[0][4] + '; ';
	end;
	
	if (R_UVTransmittance = '') then
	begin
		SetLength(arrData, 0);
		arrData:= DoPSGetXMLData(XPATH_EXPRESSION_33);

		if Length(arrData) > 0 then
		begin
		  R_UVTransmittance:= R_UVTransmittance + 'R: UV = ' + arrData[0][2] + arrData[0][4] + '; ';
		end;
	end;

	//
	// LEFT / SINISTRA
	//

	// L_S
	L_S:= '';

	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_13);

	if Length(arrData) > 0 then
	begin
	  L_S:= L_S + FormatSignValue('S=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end
	else
	begin
	  // There seem to be firmware versions in circulation in which the node 
	  // for the sphere value is written "wrong" in the export XML file.
	  // The <Sphare> node is exported instead of <Sphere>.
	  SetLength(arrData, 0);
	  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_23);	  
	  
	  if Length(arrData) > 0 then
	  begin
	    L_S:= L_S + FormatSignValue('S=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	  end; 	  
	end;
	
	if (L_S = '') then
	begin
		SetLength(arrData, 0);
		arrData:= DoPSGetXMLData(XPATH_EXPRESSION_35);

		if Length(arrData) > 0 then
		begin
		  L_S:= L_S + FormatSignValue('S=', arrData[0][2], bolAddSign, bolAddSignSeparator);
		end
		else
		begin
		  // There seem to be firmware versions in circulation in which the node 
		  // for the sphere value is written "wrong" in the export XML file.
		  // The <Sphare> node is exported instead of <Sphere>.
		  SetLength(arrData, 0);
		  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_45);	  
		  
		  if Length(arrData) > 0 then
		  begin
		    L_S:= L_S + FormatSignValue('S=', arrData[0][2], bolAddSign, bolAddSignSeparator);
		  end; 	  
		end;
	end;

	// L_Z
	L_Z:= '';

	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_14);

	if Length(arrData) > 0 then
	begin
	  L_Z:= L_Z + FormatSignValue('Z=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
	end;
	
	if (L_Z = '') then
	begin
		SetLength(arrData, 0);
		arrData:= DoPSGetXMLData(XPATH_EXPRESSION_36);

		if Length(arrData) > 0 then
		begin
		  L_Z:= L_Z + FormatSignValue('Z=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
		end;
	end;

	// L_Axis
	L_Axis:= '';

	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_15);

	if Length(arrData) > 0 then
	begin
	  L_Axis:= L_Axis + FormatAxisValue('*', arrData[0][2], bolAddAxisSeparator);
	end;
	
	if (L_Axis = '') then
	begin
		SetLength(arrData, 0);
		arrData:= DoPSGetXMLData(XPATH_EXPRESSION_37);

		if Length(arrData) > 0 then
		begin
		  L_Axis:= L_Axis + FormatAxisValue('*', arrData[0][2], bolAddAxisSeparator);
		end;
	end;

	// L_ADD
	L_Add:= '';

	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_16);

	if Length(arrData) > 0 then
	begin
	  L_Add:= L_Add + FormatSignValue('A=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end;
	
	if (L_Add = '') then
	begin
		SetLength(arrData, 0);
		arrData:= DoPSGetXMLData(XPATH_EXPRESSION_38);

		if Length(arrData) > 0 then
		begin
		  L_Add:= L_Add + FormatSignValue('A=', arrData[0][2], bolAddSign, bolAddSignSeparator);
		end;
	end;

	// L_ADD2
	L_Add2:= '';

	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_17);

	if Length(arrData) > 0 then
	begin
	  L_Add2:= L_Add2 + FormatSignValue('A2=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end;
	
	if (L_Add2 = '') then
	begin
		SetLength(arrData, 0);
		arrData:= DoPSGetXMLData(XPATH_EXPRESSION_39);

		if Length(arrData) > 0 then
		begin
		  L_Add2:= L_Add2 + FormatSignValue('A2=', arrData[0][2], bolAddSign, bolAddSignSeparator);
		end;
	end;

	// L_PRISM	
	L_PRISM:= '';

	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_18);

	if Length(arrData) > 0 then
	begin
	  L_PRISM:= L_PRISM + FormatSignValue('P=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end;
	
	if (L_PRISM = '') then
	begin
		SetLength(arrData, 0);
		arrData:= DoPSGetXMLData(XPATH_EXPRESSION_40);

		if Length(arrData) > 0 then
		begin
		  L_PRISM:= L_PRISM + FormatSignValue('P=', arrData[0][2], bolAddSign, bolAddSignSeparator);
		end;
	end;

	// L_PRISMBASE
	L_PRISMBASE:= '';

	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_19);

	if Length(arrData) > 0 then
	begin
	  L_PRISMBASE:= L_PRISMBASE + FormatSignValue('B=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end;
	
	if (L_PRISMBASE = '') then
	begin
		SetLength(arrData, 0);
		arrData:= DoPSGetXMLData(XPATH_EXPRESSION_41);

		if Length(arrData) > 0 then
		begin
		  L_PRISMBASE:= L_PRISMBASE + FormatSignValue('B=', arrData[0][2], bolAddSign, bolAddSignSeparator);
		end;
	end;

	// L_PRISMX
	L_PRISMX:= '';

	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_20);

	if Length(arrData) > 0 then
	begin
	  L_PRISMX:= L_PRISMX + FormatSignValue('P=', arrData[0][2], False, bolAddSignSeparator);
	end;
	
	if (L_PRISMX = '') then
	begin
		SetLength(arrData, 0);
		arrData:= DoPSGetXMLData(XPATH_EXPRESSION_42);

		if Length(arrData) > 0 then
		begin
		  L_PRISMX:= L_PRISMX + FormatSignValue('P=', arrData[0][2], False, bolAddSignSeparator);
		end;
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
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_21);
	
	if Length(arrData) > 0 then
	begin
	  L_PRISMY:= L_PRISMY + FormatSignValue('', arrData[0][2], False, bolAddSignSeparator);
	end;
	
	if (L_PRISMY = '') then
	begin
		SetLength(arrData, 0);
		arrData:= DoPSGetXMLData(XPATH_EXPRESSION_43);
		
		if Length(arrData) > 0 then
		begin
		  L_PRISMY:= L_PRISMY + FormatSignValue('', arrData[0][2], False, bolAddSignSeparator);
		end;
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
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_22);

	if Length(arrData) > 0 then
	begin
	  L_UVTransmittance:= L_UVTransmittance + 'L: UV = ' + arrData[0][2] + arrData[0][4];
	end;
	
	if (L_UVTransmittance = '') then
	begin
		SetLength(arrData, 0);
		arrData:= DoPSGetXMLData(XPATH_EXPRESSION_44);

		if Length(arrData) > 0 then
		begin
		  L_UVTransmittance:= L_UVTransmittance + 'L: UV = ' + arrData[0][2] + arrData[0][4];
		end;
	end;

	// -----------------------------------------------------------------
	//
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