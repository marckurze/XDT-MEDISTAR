const
	VERSION = '1.0.2.38';
	DATE = '15.07.2020 11:27:17';
	TEXT = 'Copyright (c) 2020 team2work GmbH';
  
  CSV_LINE_SEPARATOR = #13#10;
  CSV_VALUE_SEPARATOR = ',';
  
  CSV_IDENTIFIER_01 = '[PT_ID]';
  CSV_IDENTIFIER_02 = '[RL_1]';
  CSV_IDENTIFIER_03 = '[DENSITY_1]';
  CSV_IDENTIFIER_04 = '[THK_1]';
  CSV_IDENTIFIER_05 = '[RL_2]';
  CSV_IDENTIFIER_06 = '[DENSITY_2]';
  CSV_IDENTIFIER_07 = '[THK_2]';
  CSV_IDENTIFIER_08 = '[FILES_N]';
  CSV_IDENTIFIER_09 = '[FILE]';
	
	GDT_FID_PATIENT_ID = '3000';
	GDT_FID_MEASURE_DATA = '6228';
	GDT_FID_COMMENT = '6227';
	GDT_FID_FILE_ARCHIVE_NUMBER = '6302';
	GDT_FID_FILE_FORMAT = '6303';
	GDT_FID_FILE_DESCRIPTION = '6304';
	GDT_FID_FILE_URL = '6305';
  
  EXTERNAL_FILES_IMAGES_ARCHIVE_NUMBER = 'Bild';
  EXTERNAL_FILES_IMAGES_FORMAT = 'JPG';

var
	arrData: TStringArray;
	arrData2: TStringArray;
	ParsedData, S, PatientID: String;
  R_DataBlockEntered, L_DataBlockEntered: Boolean;
  R_CD, R_CCT, L_CD, L_CCT, R_Image, L_Image: String;
	i, ImageCount, ImageCounter: Integer;
  bolAddExternalFilesImagesToGdtFile: Boolean;
  Images: String;
begin
	// Aktiviere den Import aller externen Bildquellen.
	// Verwende "True", damit der Import aktiviert ist, benutze "False", damit Bilder
	// nicht importiert und per GDT weiterverarbeitet werden.
  bolAddExternalFilesImagesToGdtFile:= True;

	// Clear parsed data string
	FParsedDataString:= '';
  
  // Get array of CSV lines
  arrData:= Explode(CSV_LINE_SEPARATOR, FRawDataString, 0);
  
  if Length(arrData) <= 0 then
  begin
    FLastErrorCode:= -4;
    FLastErrorMessage:= 'Keine CSV-Zeilen für die Verarbeitung verfügbar';

    DoPSError;

    Exit;
  end;
  
  PatientID:= '';
  R_DataBlockEntered:= False;
  R_CD:= '';
  R_CCT:= '';
  L_DataBlockEntered:= False;
  L_CD:= '';
  L_CCT:= '';
  R_Image:= '';
  L_Image:= '';
  ImageCount:= 0;
  ImageCounter:= 0;
  
  for i:= 0 to Length(arrData) - 1 do
  begin
    S:= arrData[i];
    
    if T2WStartsStr(CSV_IDENTIFIER_01, S) then
    begin
      arrData2:= Explode(CSV_VALUE_SEPARATOR, S, 0);
      
      if Length(arrData2) <> 2 then
      begin
        // TODO: May be show error here
        Continue;
      end;
      
      PatientID:= arrData2[1];
      
      // Overwrite patient ID if it's not the same
      if PatientID <> FPatientID then
        PatientID:= FPatientID;
    end;
    
    if T2WStartsStr(CSV_IDENTIFIER_02, S) then
      R_DataBlockEntered:= True;
    
    if (T2WStartsStr(CSV_IDENTIFIER_03, S)) and (R_DataBlockEntered) then
    begin
      arrData2:= Explode(CSV_VALUE_SEPARATOR, S, 0);
      
      if Length(arrData2) <> 2 then
      begin
        // TODO: May be show error here
        Continue;
      end;
      
      R_CD:= arrData2[1];
    end;
    
    if (T2WStartsStr(CSV_IDENTIFIER_04, S)) and (R_DataBlockEntered) then
    begin
      arrData2:= Explode(CSV_VALUE_SEPARATOR, S, 0);
      
      if Length(arrData2) <> 2 then
      begin
        // TODO: May be show error here
        Continue;
      end;
      
      R_CCT:= arrData2[1];
    end;
    
    if T2WStartsStr(CSV_IDENTIFIER_05, S) then
      L_DataBlockEntered:= True;
    
    if (T2WStartsStr(CSV_IDENTIFIER_06, S)) and (L_DataBlockEntered) then
    begin
      arrData2:= Explode(CSV_VALUE_SEPARATOR, S, 0);
      
      if Length(arrData2) <> 2 then
      begin
        // TODO: May be show error here
        Continue;
      end;
      
      L_CD:= arrData2[1];
    end;
    
    if (T2WStartsStr(CSV_IDENTIFIER_07, S)) and (L_DataBlockEntered) then
    begin
      arrData2:= Explode(CSV_VALUE_SEPARATOR, S, 0);
      
      if Length(arrData2) <> 2 then
      begin
        // TODO: May be show error here
        Continue;
      end;
      
      L_CCT:= arrData2[1];
    end;
    
    if not bolAddExternalFilesImagesToGdtFile then
      Continue;
    
    if T2WStartsStr(CSV_IDENTIFIER_08, S) then
    begin
      arrData2:= Explode(CSV_VALUE_SEPARATOR, S, 0);
      
      if Length(arrData2) <> 2 then
      begin
        // TODO: May be show error here
        Continue;
      end;
      
      ImageCount:= StrToIntDef(arrData2[1], 0);
    end;
    
    if (T2WStartsStr(CSV_IDENTIFIER_09, S)) and (ImageCount > 0) and (ImageCounter < ImageCount) then
    begin
      arrData2:= Explode(CSV_VALUE_SEPARATOR, S, 0);
      
      if Length(arrData2) <> 5 then
      begin
        // TODO: May be show error here
        Continue;
      end;
      
      ImageCounter:= ImageCounter + 1;
      
      if LowerCase(arrData2[3]) = 'r' then
        R_Image:= arrData2[1]
      else if LowerCase(arrData2[3]) = 'l' then
        L_Image:= arrData2[1];
    end;
  end;
  
  // Build output
  ParsedData:= '';

	// Add patient ID
	if PatientID <> '' then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
	  ParsedData:= ParsedData + PatientID + FOutputLineSeparator;
	end;
  
  if R_CD <> '' then
  begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + 'R CD = ' + R_CD + FOutputLineSeparator;
  end;
  
  if R_CCT <> '' then
  begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + 'R CCT = ' + R_CCT + FOutputLineSeparator;
  end;
  
  if L_CD <> '' then
  begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + 'L CD = ' + L_CD + FOutputLineSeparator;
  end;
  
  if L_CCT <> '' then
  begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + 'L CCT = ' + L_CCT + FOutputLineSeparator;
  end;
	
	// Add images
  if bolAddExternalFilesImagesToGdtFile then
  begin
    Images:= '';
    
    if R_Image <> '' then
    begin
      Images:= Images + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_ARCHIVE_NUMBER + FOutputLineSeparator + '0001' + FOutputLineSeparator;
      Images:= Images + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_FORMAT + FOutputLineSeparator + EXTERNAL_FILES_IMAGES_FORMAT + FOutputLineSeparator;
      Images:= Images + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_DESCRIPTION + FOutputLineSeparator + EXTERNAL_FILES_IMAGES_ARCHIVE_NUMBER + FOutputLineSeparator;
      Images:= Images + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_URL + FOutputLineSeparator + DoPSGetFullPath(R_Image) + FOutputLineSeparator;
    end;
    
    if L_Image <> '' then
    begin
      Images:= Images + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_ARCHIVE_NUMBER + FOutputLineSeparator + '0001' + FOutputLineSeparator;
      Images:= Images + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_FORMAT + FOutputLineSeparator + EXTERNAL_FILES_IMAGES_FORMAT + FOutputLineSeparator;
      Images:= Images + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_DESCRIPTION + FOutputLineSeparator + EXTERNAL_FILES_IMAGES_ARCHIVE_NUMBER + FOutputLineSeparator;
      Images:= Images + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_URL + FOutputLineSeparator + DoPSGetFullPath(L_Image) + FOutputLineSeparator;
    end;
    
    ParsedData:= ParsedData + Images;
  end;
	
	// Set output
	FParsedDataString:= RawByteString(ParsedData);
end.
