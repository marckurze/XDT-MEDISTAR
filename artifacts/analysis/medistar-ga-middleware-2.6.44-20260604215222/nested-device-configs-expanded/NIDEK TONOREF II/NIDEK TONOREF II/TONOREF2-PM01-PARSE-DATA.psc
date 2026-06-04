const
	VERSION = '1.0.2.37';
	DATE = '21.09.2020 14:06:17';
	TEXT = 'Copyright (c) 2020 team2work GmbH';

	XPATH_EXPRESSION_01 = '//Data/Patient/ID';
	XPATH_EXPRESSION_02 = '//Data/R/PACHY/PACHYList/Thickness';
	XPATH_EXPRESSION_03 = '//Data/R/PACHY/PACHYAverage/Thickness';
	XPATH_EXPRESSION_04 = '//Data/R/PACHY/PACHYImage';
	XPATH_EXPRESSION_05 = '//Data/L/PACHY/PACHYList/Thickness';
	XPATH_EXPRESSION_06 = '//Data/L/PACHY/PACHYAverage/Thickness';
	XPATH_EXPRESSION_07 = '//Data/L/PACHY/PACHYImage';
	
        XPATH_UNIT = #$B5+'m'; // Mikrometer	
	
	GDT_FID_PATIENT_ID = '3000';
	GDT_FID_MEASURE_DATA = '6228';
	GDT_FID_COMMENT = '6227';
	GDT_FID_FILE_ARCHIVE_NUMBER = '6302';
	GDT_FID_FILE_FORMAT = '6303';
	GDT_FID_FILE_DESCRIPTION = '6304';
	GDT_FID_FILE_URL = '6305';

var
	arrData: TStringArrayArray;
	ParsedData, PatientID: String;
	R_PACHYList_Thickness, R_PACHYAverage_Thickness, R_PACHYImage, L_PACHYList_Thickness, L_PACHYAverage_Thickness, L_PACHYImage: String;
	R_Line, L_Line: String;
	i: Integer;
        bolAddExternalFilesToGdtFile: Boolean;

begin
	// Aktiviere den Import aller externen Bildquellen.
	// Verwende "True", damit der Import aktiviert ist, benutze "False", damit Bilder
	// nicht importiert und per GDT weiterverarbeitet werden.
        bolAddExternalFilesToGdtFile:= False;

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
	
	// R_PACHYList_Thickness
	R_PACHYList_Thickness:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_02);
	
	if Length(arrData) > 0 then
	begin
	  for i:= 0 to Length(arrData) - 1 do
	  begin
	    R_PACHYList_Thickness:= R_PACHYList_Thickness + arrData[i][2] + ' ';
	  end;
		
	  R_PACHYList_Thickness:= Trim(R_PACHYList_Thickness);
	end;
	
	// R_PACHYAverage_Thickness
	R_PACHYAverage_Thickness:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_03);
	
	if Length(arrData) > 0 then
	begin
	  R_PACHYAverage_Thickness:= R_PACHYAverage_Thickness + '[' + arrData[0][2] + ']';
	end;
	
	// R_PACHYImage
	R_PACHYImage:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_04);
	
	if Length(arrData) > 0 then
	begin
	  R_PACHYImage:= R_PACHYImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_ARCHIVE_NUMBER + FOutputLineSeparator + '0001' + FOutputLineSeparator;
	  R_PACHYImage:= R_PACHYImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_FORMAT + FOutputLineSeparator + 'JPG' + FOutputLineSeparator;
	  R_PACHYImage:= R_PACHYImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_DESCRIPTION + FOutputLineSeparator + 'Bild Messung' + FOutputLineSeparator;		
	  R_PACHYImage:= R_PACHYImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_URL + FOutputLineSeparator + arrData[0][2] + FOutputLineSeparator;
	end;
	
	// L_PACHYList_Thickness
	L_PACHYList_Thickness:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_05);
	
	if Length(arrData) > 0 then
	begin
	  for i:= 0 to Length(arrData) - 1 do
	  begin
	    L_PACHYList_Thickness:= L_PACHYList_Thickness + arrData[i][2] + ' ';
	  end;
		
	  L_PACHYList_Thickness:= Trim(L_PACHYList_Thickness);
	end;
	
	// L_PACHYAverage_Thickness
	L_PACHYAverage_Thickness:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_06);
	
	if Length(arrData) > 0 then
	begin
	  L_PACHYAverage_Thickness:= L_PACHYAverage_Thickness + '[' + arrData[0][2] + ']';
	end;
	
	// L_PACHYImage
	L_PACHYImage:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_07);
	
	if Length(arrData) > 0 then
	begin
	  L_PACHYImage:= L_PACHYImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_ARCHIVE_NUMBER + FOutputLineSeparator + '0002' + FOutputLineSeparator;
	  L_PACHYImage:= L_PACHYImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_FORMAT + FOutputLineSeparator + 'JPG' + FOutputLineSeparator;
	  L_PACHYImage:= L_PACHYImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_DESCRIPTION + FOutputLineSeparator + 'Bild Messung' + FOutputLineSeparator;
	  L_PACHYImage:= L_PACHYImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_URL + FOutputLineSeparator + arrData[0][2] + FOutputLineSeparator;
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
	
	if R_PACHYList_Thickness <> '' then
	begin
	  R_Line:= R_Line + 'PR: ' + R_PACHYList_Thickness;
	end;
	
	if R_PACHYAverage_Thickness <> '' then
	begin
	  if R_Line <> '' then
	    R_Line:= R_Line + ' '
	  else
	    R_Line:= R_Line + 'PR: ';
		
	  R_Line:= R_Line + R_PACHYAverage_Thickness;
	end;
	
	if R_Line <> '' then
	begin
	  R_Line:= R_Line + ' ' + XPATH_UNIT;
	end;
	
	// Add left eye
	L_Line:= '';
	
	if L_PACHYList_Thickness <> '' then
	begin
	  L_Line:= L_Line + 'PL: ' + L_PACHYList_Thickness
	end;
	
	if L_PACHYAverage_Thickness <> '' then
	begin
	  if L_Line <> '' then
	    L_Line:= L_Line + ' '
	  else
	    L_Line:= L_Line + 'PL: ';
		
	  L_Line:= L_Line + L_PACHYAverage_Thickness;
	end;
	
	if L_Line <> '' then
	begin
	  L_Line:= L_Line + ' ' + XPATH_UNIT;
	end;
	
	if R_Line <> '' then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + R_Line + FOutputLineSeparator;
	end;
	
	if L_Line <> '' then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
	end;
	
	// Add images
	if (R_PACHYImage <> '') and (bolAddExternalFilesToGdtFile) then
	begin
	  ParsedData:= ParsedData + R_PACHYImage;
	end;
	
	if (L_PACHYImage <> '') and (bolAddExternalFilesToGdtFile) then
	begin
	  ParsedData:= ParsedData + L_PACHYImage;
	end;
	
	// Set output
	FParsedDataString:= RawByteString(ParsedData);
end.
