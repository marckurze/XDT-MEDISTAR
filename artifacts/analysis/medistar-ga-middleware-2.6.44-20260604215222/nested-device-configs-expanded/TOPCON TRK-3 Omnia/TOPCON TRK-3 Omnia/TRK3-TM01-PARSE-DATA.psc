const
  VERSION = '1.0.55.38';
  DATE = '07.01.2026 07:24:11';
  TEXT = 'Copyright (c) 2026 CompuGroup Medical Deutschland AG';

  XPATH_EXPRESSION_01 = '//Ophthalmology/*[name()=''nsCommon:Common'']/*[name()=''nsCommon:Patient'']/*[name()=''nsCommon:ID'']';
  
  XPATH_EXPRESSION_02 = '//Ophthalmology/*[name()=''nsTM:Measure''][@type=''TM'']/*[name()=''nsTM:TM'']/*[name()=''nsTM:R'']/*[name()=''nsTM:List'']/*[name()=''nsTM:IOP_mmHg'']';
  XPATH_EXPRESSION_03 = '//Ophthalmology/*[name()=''nsTM:Measure''][@type=''TM'']/*[name()=''nsTM:TM'']/*[name()=''nsTM:R'']/*[name()=''nsTM:Average'']/*[name()=''nsTM:IOP_mmHg'']';
  XPATH_EXPRESSION_04 = '//Ophthalmology/*[name()=''nsTM:Measure''][@type=''TM'']/*[name()=''nsTM:TM'']/*[name()=''nsTM:L'']/*[name()=''nsTM:List'']/*[name()=''nsTM:IOP_mmHg'']';
  XPATH_EXPRESSION_05 = '//Ophthalmology/*[name()=''nsTM:Measure''][@type=''TM'']/*[name()=''nsTM:TM'']/*[name()=''nsTM:L'']/*[name()=''nsTM:Average'']/*[name()=''nsTM:IOP_mmHg'']';
  
  XPATH_EXPRESSION_06 = '//Ophthalmology/*[name()=''nsTM:Measure''][@type=''TM'']/*[name()=''nsTM:CorrectedIOP'']/*[name()=''nsTM:Formula1'']/*[name()=''nsTM:R'']/*[name()=''nsTM:CCT'']';
  XPATH_EXPRESSION_07 = '//Ophthalmology/*[name()=''nsTM:Measure''][@type=''TM'']/*[name()=''nsTM:CorrectedIOP'']/*[name()=''nsTM:Formula1'']/*[name()=''nsTM:L'']/*[name()=''nsTM:CCT'']'; 

  XPATH_EXPRESSION_08 = '//Ophthalmology/*[name()=''nsTM:Measure''][@type=''TM'']/*[name()=''nsTM:CorrectedIOP'']/*[name()=''nsTM:Formula1'']/*[name()=''nsTM:R'']/*[name()=''nsTM:Measured'']/*[name()=''nsTM:IOP_mmHg'']';  
  XPATH_EXPRESSION_09 = '//Ophthalmology/*[name()=''nsTM:Measure''][@type=''TM'']/*[name()=''nsTM:CorrectedIOP'']/*[name()=''nsTM:Formula1'']/*[name()=''nsTM:R'']/*[name()=''nsTM:Corrected'']/*[name()=''nsTM:IOP_mmHg'']'; 

  XPATH_EXPRESSION_10 = '//Ophthalmology/*[name()=''nsTM:Measure''][@type=''TM'']/*[name()=''nsTM:CorrectedIOP'']/*[name()=''nsTM:Formula1'']/*[name()=''nsTM:L'']/*[name()=''nsTM:Measured'']/*[name()=''nsTM:IOP_mmHg'']';  
  XPATH_EXPRESSION_11 = '//Ophthalmology/*[name()=''nsTM:Measure''][@type=''TM'']/*[name()=''nsTM:CorrectedIOP'']/*[name()=''nsTM:Formula1'']/*[name()=''nsTM:L'']/*[name()=''nsTM:Corrected'']/*[name()=''nsTM:IOP_mmHg'']'; 

  XPATH_EXPRESSION_12 = '//Ophthalmology/*[name()=''nsTM:Measure''][@type=''TM'']/*[name()=''nsTM:CorrectedIOP'']/*[name()=''nsTM:Formula1'']/*[name()=''nsTM:R'']/*[name()=''nsTM:Param1'']';
  XPATH_EXPRESSION_13 = '//Ophthalmology/*[name()=''nsTM:Measure''][@type=''TM'']/*[name()=''nsTM:CorrectedIOP'']/*[name()=''nsTM:Formula1'']/*[name()=''nsTM:R'']/*[name()=''nsTM:Param2'']';

  XPATH_EXPRESSION_14 = '//Ophthalmology/*[name()=''nsTM:Measure''][@type=''TM'']/*[name()=''nsTM:CorrectedIOP'']/*[name()=''nsTM:Formula1'']/*[name()=''nsTM:L'']/*[name()=''nsTM:Param1'']';
  XPATH_EXPRESSION_15 = '//Ophthalmology/*[name()=''nsTM:Measure''][@type=''TM'']/*[name()=''nsTM:CorrectedIOP'']/*[name()=''nsTM:Formula1'']/*[name()=''nsTM:L'']/*[name()=''nsTM:Param2'']';

  XPATH_UNIT   = 'mmHg';
  XPATH_UNIT_2 = 'mm';
  
  GDT_FID_PATIENT_ID          = '3000';
  GDT_FID_MEASURE_DATA        = '6228';
  GDT_FID_FILE_ARCHIVE_NUMBER = '6302';
  GDT_FID_FILE_FORMAT         = '6303';
  GDT_FID_FILE_DESCRIPTION    = '6304';
  GDT_FID_FILE_URL            = '6305';
  
  GDT_FID_DIAG                = '6205';
  GDT_FID_RESULT              = '6220';
  GDT_FID_FOREIGN_RESULT      = '6221';
  GDT_FID_COMMENT             = '6227';
  GDT_FID_SIGNATURE           = '8990'; 

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
  R_NTList_mmHG, R_NTAverage_mmHG, L_NTList_mmHG, L_NTAverage_mmHG: String;
  R_Line, L_Line: String;
  i: Integer;
  bolAddExternalFilesImagesToGdtFile, bolAddExternalFilesDocumentsToGdtFile, bolUseDefaultMedistarFormat: Boolean;
  Documents, Images: String;
  R_nsTMCorrectedIOP_Param1, R_nsTMCorrectedIOP_Param2, R_nsTMCorrectedIOP_CCT: String;
  L_nsTMCorrectedIOP_Param1, L_nsTMCorrectedIOP_Param2, L_nsTMCorrectedIOP_CCT: String;
  R_nsTMCorrectedIOP_Measured_IOP_mmHg, R_nsTMCorrectedIOP_Corrected_IOP_mmHg: String;
  L_nsTMCorrectedIOP_Measured_IOP_mmHg, L_nsTMCorrectedIOP_Corrected_IOP_mmHg: String;
  R_CorrectedIOP_Line_1, R_CorrectedIOP_Line_2: String;
  L_CorrectedIOP_Line_1, L_CorrectedIOP_Line_2: String;

begin
  // Aktiviere den Import aller externen Bildquellen.
  // Verwende "True", damit der Import aktiviert ist, benutze "False", damit Bilder
  // nicht importiert und per GDT weiterverarbeitet werden.
  bolAddExternalFilesImagesToGdtFile:= False;
  
  // Aktiviere den Import aller externen Dokumentquellen.
  // Verwende "True", damit der Import aktiviert ist, benutze "False", damit Dokumente
  // nicht importiert und per GDT weiterverarbeitet werden.
  bolAddExternalFilesDocumentsToGdtFile:= False;
  
  // Verwendet den Formatierungs-Standard für MEDISTAR bei den CorrectedIOP Werten,
  // Verwende "True", damit der Standard genutzt wird, verwende "False", damit die
  // "eigene" MD-Formatierung genutzt wird.
  bolUseDefaultMedistarFormat:= True;

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
  
  // L_NTList_mmHG
  L_NTList_mmHG:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_04);
  
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
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_05);
  
  if Length(arrData) > 0 then
  begin
    L_NTAverage_mmHG:= L_NTAverage_mmHG + '[' + arrData[0][2] + ']';
  end;
  
  // CorrectedIOP
  
  // R_nsTMCorrectedIOP_Param1
  R_nsTMCorrectedIOP_Param1:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_12);
  
  if (Length(arrData) > 0) then
  begin
    R_nsTMCorrectedIOP_Param1:= arrData[0][2];
  end;
  
  // R_nsTMCorrectedIOP_Param2
  R_nsTMCorrectedIOP_Param2:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_13);
  
  if (Length(arrData) > 0) then
  begin
    R_nsTMCorrectedIOP_Param2:= arrData[0][2];
  end;
  
  // R_nsTMCorrectedIOP_CCT
  R_nsTMCorrectedIOP_CCT:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_06);
  
  if (Length(arrData) > 0) then
  begin
    R_nsTMCorrectedIOP_CCT:= arrData[0][2];
  end;
  
  // L_nsTMCorrectedIOP_Param1
  L_nsTMCorrectedIOP_Param1:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_14);
  
  if (Length(arrData) > 0) then
  begin
    L_nsTMCorrectedIOP_Param1:= arrData[0][2];
  end;
  
  // L_nsTMCorrectedIOP_Param2
  L_nsTMCorrectedIOP_Param2:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_15);
  
  if (Length(arrData) > 0) then
  begin
    L_nsTMCorrectedIOP_Param2:= arrData[0][2];
  end;
  
  // L_nsTMCorrectedIOP_CCT
  L_nsTMCorrectedIOP_CCT:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_07);
  
  if (Length(arrData) > 0) then
  begin
    L_nsTMCorrectedIOP_CCT:= arrData[0][2];
  end;
  
  // R_nsTMCorrectedIOP_Measured_IOP_mmHg
  R_nsTMCorrectedIOP_Measured_IOP_mmHg:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_08);
  
  if (Length(arrData) > 0) then
  begin
    R_nsTMCorrectedIOP_Measured_IOP_mmHg:= arrData[0][2];
  end;
  
  // R_nsTMCorrectedIOP_Corrected_IOP_mmHg
  R_nsTMCorrectedIOP_Corrected_IOP_mmHg:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_09);
  
  if (Length(arrData) > 0) then
  begin
    R_nsTMCorrectedIOP_Corrected_IOP_mmHg:= arrData[0][2];
  end;
  
  // L_nsTMCorrectedIOP_Measured_IOP_mmHg
  L_nsTMCorrectedIOP_Measured_IOP_mmHg:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_10);
  
  if (Length(arrData) > 0) then
  begin
    L_nsTMCorrectedIOP_Measured_IOP_mmHg:= arrData[0][2];
  end;
  
  // L_nsTMCorrectedIOP_Corrected_IOP_mmHg
  L_nsTMCorrectedIOP_Corrected_IOP_mmHg:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_11);
  
  if (Length(arrData) > 0) then
  begin
    L_nsTMCorrectedIOP_Corrected_IOP_mmHg:= arrData[0][2];
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
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + R_Line;
  end;
  
  if (L_Line <> '') then
  begin
    if (R_Line = '') then
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator
    else
      ParsedData:= ParsedData + ' ';
    
    ParsedData:= ParsedData + L_Line;
  end;
  
  if (R_Line <> '') or (L_Line <> '') then
  begin
    ParsedData:= ParsedData + ' ' + XPATH_UNIT + ' ' + GetCurrentTime(False);
  end;
  
  if (R_Line <> '') or (L_Line <> '') then
  begin
    ParsedData:= ParsedData + FOutputLineSeparator;
  end;
  
  // Add CorrectedIOP TM und Pachy
  if (bolUseDefaultMedistarFormat) then
  begin
    // R
    R_CorrectedIOP_Line_1:= '';
    R_CorrectedIOP_Line_1:= R_CorrectedIOP_Line_1 + 'PR: ';
    R_CorrectedIOP_Line_1:= R_CorrectedIOP_Line_1 + 'Gemessen = ' + R_nsTMCorrectedIOP_Measured_IOP_mmHg + ' ' + XPATH_UNIT + '; Korrigiert = ' + R_nsTMCorrectedIOP_Corrected_IOP_mmHg + ' ' + XPATH_UNIT;
    
    R_CorrectedIOP_Line_2:= '';
    R_CorrectedIOP_Line_2:= R_CorrectedIOP_Line_2 + 'PR: ';
    R_CorrectedIOP_Line_2:= R_CorrectedIOP_Line_2 + 'Param1 = ' + R_nsTMCorrectedIOP_Param1 + ' ' + XPATH_UNIT_2 + '; Param2 = ' + R_nsTMCorrectedIOP_Param2 + '; CCT = ' + R_nsTMCorrectedIOP_CCT + ' ' + XPATH_UNIT_2; 
    
    // L
    L_CorrectedIOP_Line_1:= '';
    L_CorrectedIOP_Line_1:= L_CorrectedIOP_Line_1 + 'PL: ';
    L_CorrectedIOP_Line_1:= L_CorrectedIOP_Line_1 + 'Gemessen = ' + L_nsTMCorrectedIOP_Measured_IOP_mmHg + ' ' + XPATH_UNIT + '; Korrigiert = ' + L_nsTMCorrectedIOP_Corrected_IOP_mmHg + ' ' + XPATH_UNIT;
    
    L_CorrectedIOP_Line_2:= '';
    L_CorrectedIOP_Line_2:= L_CorrectedIOP_Line_2 + 'PL: ';
    L_CorrectedIOP_Line_2:= L_CorrectedIOP_Line_2 + 'Param1 = ' + L_nsTMCorrectedIOP_Param1 + ' ' + XPATH_UNIT_2 + '; Param2 = ' + L_nsTMCorrectedIOP_Param2 + '; CCT = ' + L_nsTMCorrectedIOP_CCT + ' ' + XPATH_UNIT_2; 
  end
  else
  begin
    R_CorrectedIOP_Line_1:= '';
    R_CorrectedIOP_Line_2:= '';
    L_CorrectedIOP_Line_1:= '';
    L_CorrectedIOP_Line_2:= '';
  end;
  
  // Add Result
  if (R_CorrectedIOP_Line_1 <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_RESULT + FOutputLineSeparator;
    ParsedData:= ParsedData + R_CorrectedIOP_Line_1 + FOutputLineSeparator;
  end;
  
  if (R_CorrectedIOP_Line_2 <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FOREIGN_RESULT + FOutputLineSeparator;
    ParsedData:= ParsedData + R_CorrectedIOP_Line_2 + FOutputLineSeparator;
  end;
  
  //  
  if (L_CorrectedIOP_Line_1 <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_DIAG + FOutputLineSeparator;
    ParsedData:= ParsedData + L_CorrectedIOP_Line_1 + FOutputLineSeparator;
  end;
  
  if (L_CorrectedIOP_Line_2 <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_COMMENT + FOutputLineSeparator;
    ParsedData:= ParsedData + L_CorrectedIOP_Line_2 + FOutputLineSeparator;
  end;  
  
  // Add images
  if (bolAddExternalFilesImagesToGdtFile) then
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
