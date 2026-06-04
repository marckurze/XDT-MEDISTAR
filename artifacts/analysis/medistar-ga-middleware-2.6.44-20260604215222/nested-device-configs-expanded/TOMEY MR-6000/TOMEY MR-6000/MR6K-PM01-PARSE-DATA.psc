const
  VERSION = '1.0.55.38';
  DATE = '22.08.2024 16:23:04';
  TEXT = 'Copyright (c) 2020 team2work GmbH';

  XPATH_EXPRESSION_01 = '//Ophthalmology/*[name()=''nsCommon:Common'']/*[name()=''nsCommon:Patient'']/*[name()=''nsCommon:ID'']';
  XPATH_EXPRESSION_02 = '//Ophthalmology/*[name()=''nsTM:Measure''][@type=''TM'']/*[name()=''nsTM:PM'']/*[name()=''nsTM:R'']/*[name()=''nsTM:List'']/*[name()=''nsTM:CCT'']';
  XPATH_EXPRESSION_03 = '//Ophthalmology/*[name()=''nsTM:Measure''][@type=''TM'']/*[name()=''nsTM:PM'']/*[name()=''nsTM:R'']/*[name()=''nsTM:Average'']/*[name()=''nsTM:CCT'']';
  XPATH_EXPRESSION_04 = '//Ophthalmology/*[name()=''nsTM:Measure''][@type=''TM'']/*[name()=''nsTM:PM'']/*[name()=''nsTM:L'']/*[name()=''nsTM:List'']/*[name()=''nsTM:CCT'']';
  XPATH_EXPRESSION_05 = '//Ophthalmology/*[name()=''nsTM:Measure''][@type=''TM'']/*[name()=''nsTM:PM'']/*[name()=''nsTM:L'']/*[name()=''nsTM:Average'']/*[name()=''nsTM:CCT'']';

  XPATH_UNIT_2 = #$B5+'m'; // Mikrometer
  XPATH_UNIT = 'mm'; // Millimeter
  
  GDT_FID_PATIENT_ID = '3000';
  GDT_FID_MEASURE_DATA = '6228';
  GDT_FID_COMMENT = '6227';
  GDT_FID_FILE_ARCHIVE_NUMBER = '6302';
  GDT_FID_FILE_FORMAT = '6303';
  GDT_FID_FILE_DESCRIPTION = '6304';
  GDT_FID_FILE_URL = '6305';
  
  EXTERNAL_FILES_IMAGES_FILE_MASK = '*.jpg';
  EXTERNAL_FILES_IMAGES_ARCHIVE_NUMBER = 'Bild';
  EXTERNAL_FILES_IMAGES_FORMAT = 'JPG';
  
  EXTERNAL_FILES_DOCUMENTS_FILE_MASK = '*.pdf';
  EXTERNAL_FILES_DOCUMENTS_ARCHIVE_NUMBER = 'Dokument';
  EXTERNAL_FILES_DOCUMENTS_FORMAT = 'PDF';

var
  arrData: TStringArrayArray;
  arrData2: TStringArray;
  ParsedData, PatientID: String;
  R_PACHYList_Thickness, R_PACHYAverage_Thickness, L_PACHYList_Thickness, L_PACHYAverage_Thickness: String;
  R_Line, L_Line: String;
  i: Integer;
  bolAddExternalFilesImagesToGdtFile, bolAddExternalFilesDocumentsToGdtFile: Boolean;
  Documents, Images: String;

begin
  // Aktiviere den Import aller externen Bildquellen.
  // Verwende "True", damit der Import aktiviert ist, benutze "False", damit Bilder
  // nicht importiert und per GDT weiterverarbeitet werden.
  bolAddExternalFilesImagesToGdtFile:= False;
  
  // Aktiviere den Import aller externen Dokumentquellen.
  // Verwende "True", damit der Import aktiviert ist, benutze "False", damit Dokumente
  // nicht importiert und per GDT weiterverarbeitet werden.
  bolAddExternalFilesDocumentsToGdtFile:= False;

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
  
  // L_PACHYList_Thickness
  L_PACHYList_Thickness:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_04);
  
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
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_05);
  
  if Length(arrData) > 0 then
  begin
    L_PACHYAverage_Thickness:= L_PACHYAverage_Thickness + '[' + arrData[0][2] + ']';
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
