const
	VERSION = '1.0.7.53';
	DATE = '06.07.2020 12:24:39';
	TEXT = 'Copyright (c) 2020 team2work GmbH';
	
	XPATH_EXPRESSION_01 = '//Ophthalmology/*[name()=''nsCommon:Common'']/*[name()=''nsCommon:Patient'']/*[name()=''nsCommon:ID'']';
	XPATH_EXPRESSION_02 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:VD'']';
	XPATH_EXPRESSION_03 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:REF'']/*[name()=''nsREF:R'']/*[name()=''nsREF:Median'']/*[name()=''nsREF:Sphere'']';
	XPATH_EXPRESSION_04 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:REF'']/*[name()=''nsREF:R'']/*[name()=''nsREF:Median'']/*[name()=''nsREF:Cylinder'']';
	XPATH_EXPRESSION_05 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:REF'']/*[name()=''nsREF:R'']/*[name()=''nsREF:Median'']/*[name()=''nsREF:Axis'']';
	XPATH_EXPRESSION_06 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:REF'']/*[name()=''nsREF:L'']/*[name()=''nsREF:Median'']/*[name()=''nsREF:Sphere'']';
	XPATH_EXPRESSION_07 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:REF'']/*[name()=''nsREF:L'']/*[name()=''nsREF:Median'']/*[name()=''nsREF:Cylinder'']';
	XPATH_EXPRESSION_08 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:REF'']/*[name()=''nsREF:L'']/*[name()=''nsREF:Median'']/*[name()=''nsREF:Axis'']';
	XPATH_EXPRESSION_09 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:PD'']/*[name()=''nsREF:Distance'']';
	
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
  
  EXTERNAL_FILES_IMAGES_FILE_MASK = '*.jpg';
  EXTERNAL_FILES_IMAGES_ARCHIVE_NUMBER = 'Bild';
  EXTERNAL_FILES_IMAGES_FORMAT = 'JPG';
  
  EXTERNAL_FILES_DOCUMENTS_FILE_MASK = '*.pdf';
  EXTERNAL_FILES_DOCUMENTS_ARCHIVE_NUMBER = 'Dokument';
  EXTERNAL_FILES_DOCUMENTS_FORMAT = 'PDF';

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
  arrData2: TStringArray;
	ParsedData, S, PatientID: String;
	VD, R_S, R_Z, R_Axis, L_S, L_Z, L_Axis, PD: String;
	R_Line, L_Line: String;
  Documents, Images: String;
  i: Integer;
  bolAddVDValueToOutput, bolAddPDValueToOutput: Boolean;
  bolAddGDTLinePrefix, bolAddSign, bolAddSignSeparator, bolAddAxisSeparator: Boolean;
  bolAddExternalFilesImagesToGdtFile, bolAddExternalFilesDocumentsToGdtFile: Boolean;

begin
	// Aktiviere den Import aller externen Bildquellen.
	// Verwende "True", damit der Import aktiviert ist, benutze "False", damit Bilder
	// nicht importiert und per GDT weiterverarbeitet werden.
  bolAddExternalFilesImagesToGdtFile:= True;
  
	// Aktiviere den Import aller externen Dokumentquellen.
	// Verwende "True", damit der Import aktiviert ist, benutze "False", damit Dokumente
	// nicht importiert und per GDT weiterverarbeitet werden.
  bolAddExternalFilesDocumentsToGdtFile:= True;

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
	  VD:= VD + 'VD= ' + arrData[0][2];
	end;
	
	// R_S
	R_S:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_03);
	
	if Length(arrData) > 0 then
	begin
    R_S:= R_S + FormatSignValue('S=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end;
	
	// R_Z
	R_Z:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_04);
	
	if Length(arrData) > 0 then
	begin
    R_Z:= R_Z + FormatSignValue('Z=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end;
	
	// R_Axis
	R_Axis:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_05);
	
	if Length(arrData) > 0 then
	begin
    R_Axis:= R_Axis + FormatAxisValue('*', arrData[0][2], bolAddAxisSeparator);
	end;
	
	// L_S
	L_S:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_06);
	
	if Length(arrData) > 0 then
	begin
    L_S:= L_S + FormatSignValue('S=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end;
	
	// L_Z
	L_Z:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_07);
	
	if Length(arrData) > 0 then
	begin
    L_Z:= L_Z + FormatSignValue('Z=', arrData[0][2], bolAddSign, bolAddSignSeparator);
	end;
	
	// L_Axis
	L_Axis:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_08);
	
	if Length(arrData) > 0 then
	begin
    L_Axis:= L_Axis + FormatAxisValue('*', arrData[0][2], bolAddAxisSeparator);
	end;
	
	// PD
	PD:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_09);
	
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
    if bolAddGDTLinePrefix then
      R_Line:= R_Line + GDT_LINE_PREFIX;
    
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
    if bolAddGDTLinePrefix then
      L_Line:= L_Line + GDT_LINE_PREFIX;
	  
    L_Line:= L_Line + 'L.:' + L_S + ' ' + L_Z + L_Axis;
	end;
	
	if L_Line <> '' then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
	end;
	
	// Add images
  if bolAddExternalFilesImagesToGdtFile then
  begin
    Images:= '';
    
    arrData2:= DoPSGetExternalFiles(EXTERNAL_FILES_IMAGES_FILE_MASK, True);
    
    if Length(arrData2) > 0 then
    begin
      for i:= 0 to Length(arrData2) - 1 do
      begin
        Images:= Images + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_ARCHIVE_NUMBER + FOutputLineSeparator + '0001' + FOutputLineSeparator;
        Images:= Images + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_FORMAT + FOutputLineSeparator + EXTERNAL_FILES_IMAGES_FORMAT + FOutputLineSeparator;
        Images:= Images + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_DESCRIPTION + FOutputLineSeparator + EXTERNAL_FILES_IMAGES_ARCHIVE_NUMBER + FOutputLineSeparator;
        Images:= Images + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_URL + FOutputLineSeparator + arrData2[i] + FOutputLineSeparator;
      end;
    end;
	  
    ParsedData:= ParsedData + Images;
  end;
	
	// Add documents
  if bolAddExternalFilesDocumentsToGdtFile then
  begin
    Documents:= '';
    
    arrData2:= DoPSGetExternalFiles(EXTERNAL_FILES_DOCUMENTS_FILE_MASK, True);
    
    if Length(arrData2) > 0 then
    begin
      for i:= 0 to Length(arrData2) - 1 do
      begin
        Documents:= Documents + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_ARCHIVE_NUMBER + FOutputLineSeparator + '0001' + FOutputLineSeparator;
        Documents:= Documents + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_FORMAT + FOutputLineSeparator + EXTERNAL_FILES_DOCUMENTS_FORMAT + FOutputLineSeparator;
        Documents:= Documents + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_DESCRIPTION + FOutputLineSeparator + EXTERNAL_FILES_DOCUMENTS_ARCHIVE_NUMBER + FOutputLineSeparator;
        Documents:= Documents + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_URL + FOutputLineSeparator + arrData2[i] + FOutputLineSeparator;
      end;
    end;
	  
    ParsedData:= ParsedData + Documents;
  end;
	
	// Set output
	FParsedDataString:= RawByteString(ParsedData);
end.
