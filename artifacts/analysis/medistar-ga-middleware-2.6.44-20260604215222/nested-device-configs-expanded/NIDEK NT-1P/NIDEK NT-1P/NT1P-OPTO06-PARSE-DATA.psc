const
	VERSION = '1.0.22.44';
	DATE = '30.04.2024 11:11:07';
	TEXT = 'Copyright (c) 2024 team2work GmbH';

	XPATH_EXPRESSION_01 = '//Data/Patient/ID';
	XPATH_EXPRESSION_02 = '//Data/R/NT/NTList/mmHg';
	XPATH_EXPRESSION_03 = '//Data/R/NT/NTAverage/mmHg';
	XPATH_EXPRESSION_04 = '//Data/R/NT/NTImage';
	XPATH_EXPRESSION_05 = '//Data/L/NT/NTList/mmHg';
	XPATH_EXPRESSION_06 = '//Data/L/NT/NTAverage/mmHg';
	XPATH_EXPRESSION_07 = '//Data/L/NT/NTImage';
	XPATH_EXPRESSION_08 = '//Data/R/PACHY/PACHYList/Thickness';
  XPATH_EXPRESSION_09 = '//Data/R/PACHY/PACHYAverage/Thickness';	
	XPATH_EXPRESSION_10 = '//Data/L/PACHY/PACHYList/Thickness';	
  XPATH_EXPRESSION_11 = '//Data/L/PACHY/PACHYAverage/Thickness';

  XPATH_EXPRESSION_12 = '//Data/R/NT/CorrectedIOP/Param1';
  XPATH_EXPRESSION_13 = '//Data/R/NT/CorrectedIOP/Param2';  
  XPATH_EXPRESSION_14 = '//Data/R/NT/CorrectedIOP/CCT';   
  XPATH_EXPRESSION_15 = '//Data/R/NT/CorrectedIOP/Measured/mmHg';   
  XPATH_EXPRESSION_16 = '//Data/R/NT/CorrectedIOP/Corrected/mmHg';
  XPATH_EXPRESSION_17 = '//Data/L/NT/CorrectedIOP/Param1';
  XPATH_EXPRESSION_18 = '//Data/L/NT/CorrectedIOP/Param2';  
  XPATH_EXPRESSION_19 = '//Data/L/NT/CorrectedIOP/CCT';   
  XPATH_EXPRESSION_20 = '//Data/L/NT/CorrectedIOP/Measured/mmHg';   
  XPATH_EXPRESSION_21 = '//Data/L/NT/CorrectedIOP/Corrected/mmHg'; 

	XPATH_UNIT_1 = 'mmHg';
	XPATH_UNIT_2 = 'mm';
	XPATH_UNIT_3 = 'mmHg';
	
	GDT_FID_PATIENT_ID = '3000';
	GDT_FID_MEASURE_DATA = '6228';
  GDT_FID_FOREIGN_RESULT = '6221';
  GDT_FID_RESULT = '6220';
	GDT_FID_FILE_ARCHIVE_NUMBER = '6302';
	GDT_FID_FILE_FORMAT = '6303';
	GDT_FID_FILE_DESCRIPTION = '6304';
	GDT_FID_FILE_URL = '6305';
  GDT_FID_COMMENT = '6227';
  GDT_FID_SIGNATURE = '8990';

var
	arrData: TStringArrayArray;
	ParsedData, PatientID: String;
	R_NTList_mmHG, R_NTAverage_mmHG, R_NTImage, R_PACHYList, R_PACHYAverage: String;
	L_NTList_mmHG, L_NTAverage_mmHG, L_NTImage, L_PACHYList, L_PACHYAverage: String;
	R_Line, L_Line: String;
	i: Integer;
  bolAddExternalFilesToGdtFile, bolAddPachyDataToGdtFile, bolAddCorrectedIOPDataToGdtFile: Boolean;
  CorrectedIOP_R_Line_A, CorrectedIOP_R_Line_B, CorrectedIOP_L_Line_A, CorrectedIOP_L_Line_B: String;
  CorrectedIOP_R_Param1, CorrectedIOP_R_Param2, CorrectedIOP_R_CCT: String;
  CorrectedIOP_L_Param1, CorrectedIOP_L_Param2, CorrectedIOP_L_CCT: String;
  CorrectedIOP_R_Measured_mmHg, CorrectedIOP_L_Measured_mmHg: String;
  CorrectedIOP_R_Corrected_mmHg, CorrectedIOP_L_Corrected_mmHg: String;

begin
	// Aktiviere den Import aller externen Bildquellen.
	// Verwende "True", damit der Import aktiviert ist, benutze "False", damit Bilder
	// nicht importiert und per GDT weiterverarbeitet werden.
  bolAddExternalFilesToGdtFile:= False;
  
  // Verwendet "True", damit die PACHY Daten, wenn vorhanden, exportiert werden können.
	// Benutze "False", damit die PACHY Werte nicht verwendet werden.
  bolAddPachyDataToGdtFile:= True;
  
  // Verwendet "True", damit die CorrectedIOP Daten, wenn vorhanden, exportiert werden können.
	// Benutze "False", damit die CorrectedIOP Werte nicht verwendet werden.
  bolAddCorrectedIOPDataToGdtFile:= True;

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
	
	// Patient ID
	PatientID:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_01);
	
	if Length(arrData) > 0 then
	begin
	  PatientID:= PatientID + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
	  PatientID:= PatientID + arrData[0][2] + FOutputLineSeparator;
	end;
	
	// R_NTList_mmHG
	R_NTList_mmHG:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_02);

	if Length(arrData) > 0 then
	begin
	  for i:= 0 to Length(arrData) - 1 do
	  begin
	    R_NTList_mmHG:= R_NTList_mmHG + arrData[i][2] + ' ';
	  end;
		
	  R_NTList_mmHG:= Trim(R_NTList_mmHG);
	end;
	
	// R_NTAverage_mmHG
	R_NTAverage_mmHG:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_03);
	
	if Length(arrData) > 0 then
	begin
	  R_NTAverage_mmHG:= R_NTAverage_mmHG + '[' + arrData[0][2] + ']';
	end;
	
	// R_NTImage
	R_NTImage:= '';
	
	if (bolAddExternalFilesToGdtFile) then
	begin
	  SetLength(arrData, 0);
	  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_04);
	
	  if Length(arrData) > 0 then
	  begin
	    R_NTImage:= R_NTImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_ARCHIVE_NUMBER + FOutputLineSeparator + '0001' + FOutputLineSeparator;
	    R_NTImage:= R_NTImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_FORMAT + FOutputLineSeparator + 'JPG' + FOutputLineSeparator;
	    R_NTImage:= R_NTImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_DESCRIPTION + FOutputLineSeparator + 'Bild Messung' + FOutputLineSeparator;
	    R_NTImage:= R_NTImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_URL + FOutputLineSeparator + arrData[0][2] + FOutputLineSeparator;
	  end;
	end;
	
	// R_PACHYList
	R_PACHYList:= '';
	
	if (bolAddPachyDataToGdtFile) then
	begin
	  SetLength(arrData, 0);
	  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_08);
	  
		if Length(arrData) > 0 then
		begin
		  for i:= 0 to Length(arrData) - 1 do
		  begin
		    R_PACHYList:= R_PACHYList + arrData[i][2] + ' ';
		  end;
			
		  R_PACHYList:= Trim(R_PACHYList);
		end;
	end;
	
	// R_PACHYAverage
	R_PACHYAverage:= '';
	
	if (bolAddPachyDataToGdtFile) then
	begin
	  SetLength(arrData, 0);
	  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_09);	
	
		if Length(arrData) > 0 then
		begin
		  R_PACHYAverage:= R_PACHYAverage + '[' + arrData[0][2] + ']';
		end;
	end;
	
	// R CorrectedIOP
	if (bolAddCorrectedIOPDataToGdtFile) then
	begin
	  // CorrectedIOP_R_Param1
	  SetLength(arrData, 0);
	  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_12);	
	
		if (Length(arrData) > 0) then
		begin
		  CorrectedIOP_R_Param1:= arrData[0][2];
		end;
		
		// CorrectedIOP_R_Param2
	  SetLength(arrData, 0);
	  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_13);
	
		if (Length(arrData) > 0) then
		begin
		  CorrectedIOP_R_Param2:= arrData[0][2];
		end;
		
		// CorrectedIOP_R_CCT
	  SetLength(arrData, 0);
	  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_14);	
		
		if (Length(arrData) > 0) then
		begin
		  CorrectedIOP_R_CCT:= arrData[0][2];
		end;
		
		// CorrectedIOP_R_Measured_mmHg
	  SetLength(arrData, 0);
	  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_15);	
		
		if (Length(arrData) > 0) then
		begin
		  CorrectedIOP_R_Measured_mmHg:= arrData[0][2];
		end;
		
		// CorrectedIOP_R_Corrected_mmHg
	  SetLength(arrData, 0);
	  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_16);	
		
		if (Length(arrData) > 0) then
		begin
		  CorrectedIOP_R_Corrected_mmHg:= arrData[0][2];
		end;
	end;

	// ---------------------------------------------------------------
	
	// L_NTList_mmHG
	L_NTList_mmHG:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_05);
	
	if Length(arrData) > 0 then
	begin
	  for i:= 0 to Length(arrData) - 1 do
	  begin
	    L_NTList_mmHG:= L_NTList_mmHG + arrData[i][2] + ' ';
	  end;
		
	  L_NTList_mmHG:= Trim(L_NTList_mmHG);
	end;
	
	// L_NTAverage_mmHG
	L_NTAverage_mmHG:= '';
	
	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(XPATH_EXPRESSION_06);
	
	if Length(arrData) > 0 then
	begin
	  L_NTAverage_mmHG:= L_NTAverage_mmHG + '[' + arrData[0][2] + ']';
	end;
	
	// L_NTImage
	L_NTImage:= '';
	
	if (bolAddExternalFilesToGdtFile) then
	begin
	  SetLength(arrData, 0);
	  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_07);
	
	  if Length(arrData) > 0 then
	  begin
	    L_NTImage:= L_NTImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_ARCHIVE_NUMBER + FOutputLineSeparator + '0002' + FOutputLineSeparator;
	    L_NTImage:= L_NTImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_FORMAT + FOutputLineSeparator + 'JPG' + FOutputLineSeparator;
	    L_NTImage:= L_NTImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_DESCRIPTION + FOutputLineSeparator + 'Bild Messung' + FOutputLineSeparator;
	    L_NTImage:= L_NTImage + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_URL + FOutputLineSeparator + arrData[0][2] + FOutputLineSeparator;
	  end;
	end;

	// L_PACHYList
	L_PACHYList:= '';
	
	if (bolAddPachyDataToGdtFile) then
	begin
	  SetLength(arrData, 0);
	  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_10);
	  
		if Length(arrData) > 0 then
		begin
		  for i:= 0 to Length(arrData) - 1 do
		  begin
		    L_PACHYList:= L_PACHYList + arrData[i][2] + ' ';
		  end;
			
		  L_PACHYList:= Trim(L_PACHYList);
		end;
	end;
	
	// L_PACHYAverage
	L_PACHYAverage:= '';
	
	if (bolAddPachyDataToGdtFile) then
	begin
	  SetLength(arrData, 0);
	  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_11);	
	
		if Length(arrData) > 0 then
		begin
		  L_PACHYAverage:= L_PACHYAverage + '[' + arrData[0][2] + ']';
		end;
	end;
	
	// L CorrectedIOP
	if (bolAddCorrectedIOPDataToGdtFile) then
	begin
	  // CorrectedIOP_L_Param1
	  SetLength(arrData, 0);
	  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_17);	
	
		if (Length(arrData) > 0) then
		begin
		  CorrectedIOP_L_Param1:= arrData[0][2];
		end;
		
		// CorrectedIOP_L_Param2
	  SetLength(arrData, 0);
	  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_18);
	
		if (Length(arrData) > 0) then
		begin
		  CorrectedIOP_L_Param2:= arrData[0][2];
		end;
	
		// CorrectedIOP_L_CCT
	  SetLength(arrData, 0);
	  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_19);	
		
		if (Length(arrData) > 0) then
		begin
		  CorrectedIOP_L_CCT:= arrData[0][2];
		end;

		// CorrectedIOP_L_Measured_mmHg
	  SetLength(arrData, 0);
	  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_20);	
		
		if (Length(arrData) > 0) then
		begin
		  CorrectedIOP_L_Measured_mmHg:= arrData[0][2];
		end;

		// CorrectedIOP_L_Corrected_mmHg
	  SetLength(arrData, 0);
	  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_21);	
		
		if (Length(arrData) > 0) then
		begin
		  CorrectedIOP_L_Corrected_mmHg:= arrData[0][2];
		end;
	end;
	
	// ---------------------------------------------------------------

	// Build result
	ParsedData:= '';

	// Add patient ID
	if (PatientID <> '') then
	begin
	  ParsedData:= ParsedData + PatientID;
	end;
	
	// Add right eye
	R_Line:= '';
	
	if (R_NTList_mmHG <> '') then
	begin
	  R_Line:= R_Line + 'R = ' + R_NTList_mmHG;
	end;
	
	if (R_NTAverage_mmHG <> '') then
	begin
	  if (R_Line <> '') then
	    R_Line:= R_Line + ' '
	  else
	    R_Line:= R_Line + 'R = ';
		
	  R_Line:= R_Line + R_NTAverage_mmHG;
	end;
	
	// Add left eye
	L_Line:= '';
	
	if (L_NTList_mmHG <> '') then
	begin
	  L_Line:= L_Line + '// L = ' + L_NTList_mmHG;
	end;
	
	if (L_NTAverage_mmHG <> '') then
	begin
	  if (L_Line <> '') then
	    L_Line:= L_Line + ' '
	  else
	    L_Line:= L_Line + '// L = ';
		
	  L_Line:= L_Line + L_NTAverage_mmHG;
	end;
	
	if (R_Line <> '') then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_COMMENT + FOutputLineSeparator;
	  ParsedData:= ParsedData + R_Line;
	end;
	
	if (L_Line <> '') then
	begin
	  if (R_Line = '') then
	    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_COMMENT + FOutputLineSeparator
	  else
	    ParsedData:= ParsedData + ' ';
		
	  ParsedData:= ParsedData + L_Line;
	end;
	
	if (R_Line <> '') 
	or (L_Line <> '') then
	begin
	  ParsedData:= ParsedData + ' ' + XPATH_UNIT_1 + ' ' + GetCurrentTime(False) + FOutputLineSeparator;
	end;  

	// Add images
	if (R_NTImage <> '') and (bolAddExternalFilesToGdtFile) then
	begin
	  ParsedData:= ParsedData + R_NTImage;
	end;
	
	if (L_NTImage <> '') and (bolAddExternalFilesToGdtFile) then
	begin
	  ParsedData:= ParsedData + L_NTImage;
	end;
	
	//
	// PACHY
	//
	if (bolAddPachyDataToGdtFile) then
	begin
		if (R_PACHYList <> '') 
		or (R_PACHYAverage <> '') then
		begin
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_RESULT + FOutputLineSeparator;
			
			if (R_PACHYList <> '') then
			begin
        ParsedData:= ParsedData + 'PR: ' + R_PACHYList;
			end;
			
			if (R_PACHYAverage <> '') then
			begin
				if (R_PACHYList <> '') then
					ParsedData:= ParsedData + ' ';
	
				ParsedData:= ParsedData + R_PACHYAverage + ' ' + XPATH_UNIT_2;
			end;
			
			ParsedData:= ParsedData + FOutputLineSeparator;
		end;
		 
		if (L_PACHYList <> '') or (L_PACHYAverage <> '') then
		 begin
       ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FOREIGN_RESULT + FOutputLineSeparator;
			 
			 if (L_PACHYList <> '') then
			 begin
         ParsedData:= ParsedData + 'PL: ' + L_PACHYList;
			 end; 
	
			 if (L_PACHYAverage <> '') then
			 begin
			 	 if (L_PACHYList <> '') then
				 	 ParsedData:= ParsedData + ' ';
	
				 ParsedData:= ParsedData + L_PACHYAverage + ' ' + XPATH_UNIT_2;
			 end;

			 ParsedData:= ParsedData + FOutputLineSeparator;
		 end;  
	end;
	
	// Add CorrectedIOP data
	
	// Right
	CorrectedIOP_R_Line_A:= '';
	CorrectedIOP_R_Line_B:= '';	

	CorrectedIOP_R_Line_A:= CorrectedIOP_R_Line_A + 'PR: Gemessen = ' + CorrectedIOP_R_Measured_mmHg + ' ' + XPATH_UNIT_3 + '; Korrigiert = ' + CorrectedIOP_R_Corrected_mmHg + ' ' + XPATH_UNIT_3;
	CorrectedIOP_R_Line_B:= CorrectedIOP_R_Line_B + 'PR: Param1 = ' + CorrectedIOP_R_Param1 + '; Param2 = ' + CorrectedIOP_R_Param2  + '; CCT = ' + CorrectedIOP_R_CCT;	

  // Left
	CorrectedIOP_L_Line_A:= '';
	CorrectedIOP_L_Line_B:= '';	

	CorrectedIOP_L_Line_A:= CorrectedIOP_L_Line_A + 'PL: Gemessen = ' + CorrectedIOP_L_Measured_mmHg + ' ' + XPATH_UNIT_3 + '; Korrigiert = ' + CorrectedIOP_L_Corrected_mmHg + ' ' + XPATH_UNIT_3;
	CorrectedIOP_L_Line_B:= CorrectedIOP_L_Line_B + 'PL: Param1 = ' + CorrectedIOP_L_Param1 + '; Param2 = ' + CorrectedIOP_L_Param2  + '; CCT = ' + CorrectedIOP_L_CCT;	

	if (bolAddCorrectedIOPDataToGdtFile) then
	begin
	  if (CorrectedIOP_R_Line_A <> '') then
	  begin
	    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	    ParsedData:= ParsedData + CorrectedIOP_R_Line_A + FOutputLineSeparator;
	  end;
	  
	  if (CorrectedIOP_R_Line_B <> '') then
	  begin
	    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	    ParsedData:= ParsedData + CorrectedIOP_R_Line_B + FOutputLineSeparator;
	  end;	  
	    
	  if (CorrectedIOP_L_Line_A <> '') then
	  begin
	    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	    ParsedData:= ParsedData + CorrectedIOP_L_Line_A + FOutputLineSeparator;
	  end;
	  
	  if (CorrectedIOP_L_Line_B <> '') then
	  begin
	    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	    ParsedData:= ParsedData + CorrectedIOP_L_Line_B + FOutputLineSeparator;
	  end;   
	end;
	
	// Set output
	FParsedDataString:= RawByteString(ParsedData);
end.
