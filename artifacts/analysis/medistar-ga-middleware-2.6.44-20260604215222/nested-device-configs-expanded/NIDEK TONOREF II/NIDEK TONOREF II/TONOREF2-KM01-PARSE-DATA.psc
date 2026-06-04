const
	VERSION = '1.0.2.36';
	DATE = '21.09.2020 14:07:17';
	TEXT = 'Copyright (c) 2020 team2work GmbH';

	XPATH_EXPRESSION_01 = '//Data/Patient/ID';

	XPATH_EXPRESSION_02 = '//Data/R/KM/KMMedian/R1/Radius';
	XPATH_EXPRESSION_03 = '//Data/R/KM/KMMedian/R1/Axis';
	XPATH_EXPRESSION_04 = '//Data/R/KM/KMMedian/R2/Radius';
	XPATH_EXPRESSION_05 = '//Data/R/KM/KMMedian/R2/Axis';
	XPATH_EXPRESSION_06 = '//Data/R/KM/KMMedian/Average/Radius';
	XPATH_EXPRESSION_07 = '//Data/R/KM/KMMedian/KMCylinder/Power';
	XPATH_EXPRESSION_08 = '//Data/R/KM/KMImage';

	XPATH_EXPRESSION_09 = '//Data/L/KM/KMMedian/R1/Radius';
	XPATH_EXPRESSION_10 = '//Data/L/KM/KMMedian/R1/Axis';
	XPATH_EXPRESSION_11 = '//Data/L/KM/KMMedian/R2/Radius';
	XPATH_EXPRESSION_12 = '//Data/L/KM/KMMedian/R2/Axis';
	XPATH_EXPRESSION_13 = '//Data/L/KM/KMMedian/Average/Radius';
	XPATH_EXPRESSION_14 = '//Data/L/KM/KMMedian/KMCylinder/Power';
	XPATH_EXPRESSION_15 = '//Data/L/KM/KMImage';
	
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
	R_KMMedian_R1Radius, R_KMMedian_R1Axis, R_KMMedian_R2Radius, R_KMMedian_R2Axis, R_KMMedian_AverageRadius, R_KMMedian_KMCylinderPower, R_KMImage: String;
	L_KMMedian_R1Radius, L_KMMedian_R1Axis, L_KMMedian_R2Radius, L_KMMedian_R2Axis, L_KMMedian_AverageRadius, L_KMMedian_KMCylinderPower, L_KMImage: String;
	R_Line1_Seg1, R_Line1_Seg2, R_Line1, R_Line2, L_Line1_Seg1, L_Line1_Seg2, L_Line1, L_Line2: String;
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
	
	// R_KMMedian_R1Radius
	R_KMMedian_R1Radius:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_02);
	
	if Length(arrData) > 0 then
	begin
	  R_KMMedian_R1Radius:= R_KMMedian_R1Radius + 'R1=' + arrData[0][2];
	end;
	
	// R_KMMedian_R1Axis
	R_KMMedian_R1Axis:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_03);
	
	if Length(arrData) > 0 then
	begin
	  R_KMMedian_R1Axis:= R_KMMedian_R1Axis + '*' + arrData[0][2];
	end;
	
	// R_KMMedian_R2Radius
	R_KMMedian_R2Radius:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_04);
	
	if Length(arrData) > 0 then
	begin
	  R_KMMedian_R2Radius:= R_KMMedian_R2Radius + 'R2=' + arrData[0][2];
	end;
	
	// R_KMMedian_R2Axis
	R_KMMedian_R2Axis:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_05);
	
	if Length(arrData) > 0 then
	begin
	  R_KMMedian_R2Axis:= R_KMMedian_R2Axis + '*' + arrData[0][2];
	end;
	
	// R_KMMedian_AverageRadius
	R_KMMedian_AverageRadius:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_06);
	
	if Length(arrData) > 0 then
	begin
	  R_KMMedian_AverageRadius:= R_KMMedian_AverageRadius + 'AV=' + arrData[0][2];
	end;
	
	// R_KMMedian_KMCylinderPower
	R_KMMedian_KMCylinderPower:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_07);
	
	if Length(arrData) > 0 then
	begin
	  R_KMMedian_KMCylinderPower:= R_KMMedian_KMCylinderPower + 'CYL=' + arrData[0][2];
	end;
	
	// R_KMImage
	R_KMImage:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_08);
	
	if Length(arrData) > 0 then
	begin
	  R_KMImage:= R_KMImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_ARCHIVE_NUMBER + FOutputLineSeparator + '0001' + FOutputLineSeparator;
	  R_KMImage:= R_KMImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_FORMAT + FOutputLineSeparator + 'JPG' + FOutputLineSeparator;
	  R_KMImage:= R_KMImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_DESCRIPTION + FOutputLineSeparator + 'Bild Messung' + FOutputLineSeparator;
	  R_KMImage:= R_KMImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_URL + FOutputLineSeparator + arrData[0][2] + FOutputLineSeparator;
	end;
	
	// L_KMMedian_R1Radius
	L_KMMedian_R1Radius:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_09);
	
	if Length(arrData) > 0 then
	begin
	  L_KMMedian_R1Radius:= L_KMMedian_R1Radius + 'R1=' + arrData[0][2];
	end;
	
	// L_KMMedian_R1Axis
	L_KMMedian_R1Axis:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_10);
	
	if Length(arrData) > 0 then
	begin
	  L_KMMedian_R1Axis:= L_KMMedian_R1Axis + '*' + arrData[0][2];
	end;
	
	// L_KMMedian_R2Radius
	L_KMMedian_R2Radius:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_11);
	
	if Length(arrData) > 0 then
	begin
	  L_KMMedian_R2Radius:= L_KMMedian_R2Radius + 'R2=' + arrData[0][2];
	end;
	
	// L_KMMedian_R2Axis
	L_KMMedian_R2Axis:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_12);
	
	if Length(arrData) > 0 then
	begin
	  L_KMMedian_R2Axis:= L_KMMedian_R2Axis + '*' + arrData[0][2];
	end;
	
	// L_KMMedian_AverageRadius
	L_KMMedian_AverageRadius:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_13);
	
	if Length(arrData) > 0 then
	begin
	  L_KMMedian_AverageRadius:= L_KMMedian_AverageRadius + 'AV=' + arrData[0][2];
	end;
	
	// L_KMMedian_KMCylinderPower
	L_KMMedian_KMCylinderPower:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_14);
	
	if Length(arrData) > 0 then
	begin
	  L_KMMedian_KMCylinderPower:= L_KMMedian_KMCylinderPower + 'CYL=' + arrData[0][2];
	end;
	
	// L_KMImage
	L_KMImage:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_15);
	
	if Length(arrData) > 0 then
	begin
	  L_KMImage:= L_KMImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_ARCHIVE_NUMBER + FOutputLineSeparator + '0002' + FOutputLineSeparator;
	  L_KMImage:= L_KMImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_FORMAT + FOutputLineSeparator + 'JPG' + FOutputLineSeparator;
          L_KMImage:= L_KMImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_DESCRIPTION + FOutputLineSeparator + 'Bild Messung' + FOutputLineSeparator;
	  L_KMImage:= L_KMImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_URL + FOutputLineSeparator + arrData[0][2] + FOutputLineSeparator;
	end;
	
	// Build result
	ParsedData:= '';

	// Add patient ID
	if PatientID <> '' then
	begin
	  ParsedData:= ParsedData + PatientID;
	end;
	
	// Add right eye
	R_Line1_Seg1:= '';
	
	if (R_KMMedian_R1Radius <> '') and (R_KMMedian_R1Axis <> '') then
	begin
	  R_Line1_Seg1:= R_Line1_Seg1 + R_KMMedian_R1Radius + R_KMMedian_R1Axis;
	end;
	
	R_Line1_Seg2:= '';
	
	if (R_KMMedian_R2Radius <> '') and (R_KMMedian_R2Axis <> '') then
	begin
	  R_Line1_Seg2:= R_Line1_Seg2 + R_KMMedian_R2Radius + R_KMMedian_R2Axis;
	end;
	
	R_Line1:= '';
	
	if R_Line1_Seg1 <> '' then
	begin
	  R_Line1:= R_Line1 + 'R: ' + R_Line1_Seg1 + ' ';
	end;
	
	if R_Line1_Seg2 <> '' then
	begin
	  if R_Line1 = '' then
            R_Line1:= R_Line1 + 'R: ' + R_Line1_Seg2 + ' '
	  else
            R_Line1:= R_Line1 + R_Line1_Seg2 + ' ';
	end;
	
	R_Line1:= Trim(R_Line1);
	
	R_Line2:= '';
	
	if R_KMMedian_AverageRadius <> '' then
	begin
	  R_Line2:= R_Line2 + 'R: ' + R_KMMedian_AverageRadius;
	end;
	
	if R_KMMedian_AverageRadius <> '' then
	begin
	  if R_Line2 = '' then
            R_Line2:= R_Line2 + 'R: '
	  else
            R_Line2:= R_Line2 + ' ';
		
	  R_Line2:= R_Line2 + R_KMMedian_KMCylinderPower;
	end;
	
	// Add left eye
	L_Line1_Seg1:= '';
	
	if (L_KMMedian_R1Radius <> '') and (L_KMMedian_R1Axis <> '') then
	begin
	  L_Line1_Seg1:= L_Line1_Seg1 + L_KMMedian_R1Radius + L_KMMedian_R1Axis;
	end;
	
	L_Line1_Seg2:= '';
	
	if (L_KMMedian_R2Radius <> '') and (L_KMMedian_R2Axis <> '') then
	begin
	  L_Line1_Seg2:= L_Line1_Seg2 + L_KMMedian_R2Radius + L_KMMedian_R2Axis;
	end;
	
	L_Line1:= '';
	
	if L_Line1_Seg1 <> '' then
	begin
	  L_Line1:= L_Line1 + '// L: ' + L_Line1_Seg1 + ' ';
	end;
	
	if L_Line1_Seg2 <> '' then
	begin
	  if L_Line1 = '' then
	    L_Line1:= L_Line1 + '// L: ' + L_Line1_Seg2 + ' '
	  else
	    L_Line1:= L_Line1 + L_Line1_Seg2 + ' ';
	end;
	
	L_Line1:= Trim(L_Line1);
	
	L_Line2:= '';
	
	if L_KMMedian_AverageRadius <> '' then
	begin
	  L_Line2:= L_Line2 + '// L: ' + L_KMMedian_AverageRadius;
	end;
	
	if L_KMMedian_AverageRadius <> '' then
	begin
	  if L_Line2 = '' then
	    L_Line2:= L_Line2 + '// L: '
	  else
	    L_Line2:= L_Line2 + ' ';
		
	  L_Line2:= L_Line2 + L_KMMedian_KMCylinderPower;
	end;
	
	// Build final output
	if R_Line1 <> '' then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + R_Line1;
	end;
	
	if L_Line1 <> '' then
	begin
	  if R_Line1 = '' then
            ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator
	  else
	    ParsedData:= ParsedData + ' ';
		
	  ParsedData:= ParsedData + L_Line1;
	end;
	
	if (R_Line1 <> '') or (L_Line1 <> '') then
	  ParsedData:= ParsedData + FOutputLineSeparator;
	
	if R_Line2 <> '' then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + R_Line2;
	end;
	
	if L_Line2 <> '' then
	begin
	  if R_Line2 = '' then
	    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator
	  else
	    ParsedData:= ParsedData + ' ';
		
	  ParsedData:= ParsedData + L_Line2;
	end;
	
	if (R_Line2 <> '') or (L_Line2 <> '') then
	  ParsedData:= ParsedData + FOutputLineSeparator;
	
	// Add images
	if (R_KMImage <> '') and (bolAddExternalFilesToGdtFile) then
	begin
	  ParsedData:= ParsedData + R_KMImage;
	end;
	
	if (L_KMImage <> '') and (bolAddExternalFilesToGdtFile) then
	begin
	  ParsedData:= ParsedData + L_KMImage;
	end;
	
	// Set output
	FParsedDataString:= RawByteString(ParsedData);
end.
