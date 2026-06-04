const
  VERSION = '1.0.55.40';
  DATE = '04.06.2024 09:52:05';
  TEXT = 'Copyright (c) 2024 team2work GmbH';

  MEASURE_TYPE = '<MEASURE_TYPE/>';

  // Gibt die Listeneinträge an, die verwendet werden sollen,
  // falls keine Mittelwerteinträge in der Messung enthalten sind.
  // Liste der Einträge beginnend mit 0, d.h. 0 ist der erste Eintrag.
  USE_R_LIST_ENTRY = 0;
  USE_L_LIST_ENTRY = 0;

  XPATH_EXPRESSION_01 = '//Ophthalmology/Common/Patient/ID';
  XPATH_EXPRESSION_02 = '//Ophthalmology/Measure[@type]';
  XPATH_EXPRESSION_03 = '//Ophthalmology/Measure/PixelToArea';
        
  XPATH_EXPRESSION_04 = '//Ophthalmology/Measure/' + MEASURE_TYPE + '/R/List/';
  XPATH_EXPRESSION_05 = '//Ophthalmology/Measure/' + MEASURE_TYPE + '/R/Image/FileName';

  XPATH_EXPRESSION_06 = '//Ophthalmology/Measure/' + MEASURE_TYPE + '/L/List/';
  XPATH_EXPRESSION_07 = '//Ophthalmology/Measure/' + MEASURE_TYPE + '/L/Image/FileName';
  
  XPATH_EXPRESSION_08 = '//Ophthalmology/Measure/' + MEASURE_TYPE + '/R/List/Image/FileName';  
  XPATH_EXPRESSION_09 = '//Ophthalmology/Measure/' + MEASURE_TYPE + '/L/List/Image/FileName';    
 
  GDT_FID_PATIENT_ID = '3000';
  GDT_FID_MEASURE_DATA = '6228';
  
  GDT_FID_FILE_ARCHIVE_NUMBER = '6302';
  GDT_FID_FILE_FORMAT         = '6303';
  GDT_FID_FILE_DESCRIPTION    = '6304';
  GDT_FID_FILE_URL            = '6305';

var
  arrData: TStringArrayArray; 
  bolAddExternalFilesToGdtFile: Boolean;
  ParsedData, PatientID: String;  
  MeasureType, PixelToArea, PixelToAreaUnit, R_Image, L_Image, R_Line, L_Line: String;
  strXPATH_EXPRESSION_04, strXPATH_EXPRESSION_05, strXPATH_EXPRESSION_06, strXPATH_EXPRESSION_07, strXPATH_EXPRESSION_08, strXPATH_EXPRESSION_09: String;
  
  R_NUM, R_NUM_UNIT, R_CD, R_CD_UNIT, R_AVG, R_AVG_UNIT, R_SD, R_SD_UNIT, R_CV, R_CV_UNIT: String;
  R_MAX, R_MAX_UNIT, R_MIN, R_MIN_UNIT, R_HEX, R_HEX_UNIT, R_CT, R_CT_UNIT, R_FIX: String;
  R_AREA0, R_AREA0_UNIT, R_AREA100, R_AREA100_UNIT, R_AREA200, R_AREA200_UNIT, R_AREA300, R_AREA300_UNIT: String;
  R_AREA400, R_AREA400_UNIT, R_AREA500, R_AREA500_UNIT, R_AREA600, R_AREA600_UNIT: String;  
  R_AREA700, R_AREA700_UNIT, R_AREA800, R_AREA800_UNIT, R_AREA900, R_AREA900_UNIT: String;
  R_APEX3, R_APEX3_UNIT, R_APEX4, R_APEX4_UNIT, R_APEX5, R_APEX5_UNIT, R_APEX6, R_APEX6_UNIT: String;
  R_APEX7, R_APEX7_UNIT, R_APEX8, R_APEX8_UNIT, R_APEX9, R_APEX9_UNIT, R_APEX10, R_APEX10_UNIT: String;
  R_IMAGE_BMP: String;
         
  L_NUM, L_NUM_UNIT, L_CD, L_CD_UNIT, L_AVG, L_AVG_UNIT, L_SD, L_SD_UNIT, L_CV, L_CV_UNIT: String;
  L_MAX, L_MAX_UNIT, L_MIN, L_MIN_UNIT, L_HEX, L_HEX_UNIT, L_CT, L_CT_UNIT, L_FIX: String;
  L_AREA0, L_AREA0_UNIT, L_AREA100, L_AREA100_UNIT, L_AREA200, L_AREA200_UNIT, L_AREA300, L_AREA300_UNIT: String;
  L_AREA400, L_AREA400_UNIT, L_AREA500, L_AREA500_UNIT, L_AREA600, L_AREA600_UNIT: String;  
  L_AREA700, L_AREA700_UNIT, L_AREA800, L_AREA800_UNIT, L_AREA900, L_AREA900_UNIT: String;
  L_APEX3, L_APEX3_UNIT, L_APEX4, L_APEX4_UNIT, L_APEX5, L_APEX5_UNIT, L_APEX6, L_APEX6_UNIT: String;
  L_APEX7, L_APEX7_UNIT, L_APEX8, L_APEX8_UNIT, L_APEX9, L_APEX9_UNIT, L_APEX10, L_APEX10_UNIT: String;
  L_IMAGE_BMP: String;        
        
begin
  // Aktiviere den Import aller externen Bildquellen.
  // Verwende "True", damit der Import aktiviert ist, benutze "False", damit Bilder
  // nicht importiert und per GDT weiterverarbeitet werden.
  bolAddExternalFilesToGdtFile:= True;

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
 
   MeasureType:= '';
        
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_02);
  
  if Length(arrData) > 0 then
  begin
    MeasureType:= arrData[0][4];
  end;
  
  if (MeasureType = '') then
  begin
    MeasureType:= 'SM';
  end;

  PixelToArea:= '';
   PixelToAreaUnit:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_03);

  if Length(arrData) > 0 then
  begin
    PixelToArea:= arrData[0][2];
    PixelToAreaUnit:= arrData[0][4];
  end;
  
  // -----------------------------------------------------------------
  //
  // -----------------------------------------------------------------  
  
  // main xpath part for right eye
  strXPATH_EXPRESSION_04:= XPATH_EXPRESSION_04;
  strXPATH_EXPRESSION_04:= T2WStringReplace(strXPATH_EXPRESSION_04, MEASURE_TYPE, MeasureType, False, False);

  //
  R_NUM:= '';
  R_NUM_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'NUM');

  if Length(arrData) > 0 then
  begin
    R_NUM:= arrData[0][2];
    R_NUM_UNIT:= arrData[0][4];
  end;
  
  // 
  R_CD:= '';
  R_CD_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'CD');

  if Length(arrData) > 0 then
  begin
    R_CD:= arrData[0][2];
    R_CD_UNIT:= arrData[0][4];
  end;
  
  //  
  R_AVG:= '';
  R_AVG_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'AVG');

  if Length(arrData) > 0 then
  begin
    R_AVG:= arrData[0][2];
    R_AVG_UNIT:= arrData[0][4];
  end;
  
  //  
  R_SD:= '';
  R_SD_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'SD');

  if Length(arrData) > 0 then
  begin
    R_SD:= arrData[0][2];
    R_SD_UNIT:= arrData[0][4];
  end;
  
  //    
  R_CV:= '';
  R_CV_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'CV');

  if Length(arrData) > 0 then
  begin
    R_CV:= arrData[0][2];
    R_CV_UNIT:= arrData[0][4];
  end;
  
  //  
  R_MAX:= '';
  R_MAX_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'MAX');

  if Length(arrData) > 0 then
  begin
    R_MAX:= arrData[0][2];
    R_MAX_UNIT:= arrData[0][4];
  end;
  
  //
  R_MIN:= '';
  R_MIN_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'MIN');

  if Length(arrData) > 0 then
  begin
    R_MIN:= arrData[0][2];
    R_MIN_UNIT:= arrData[0][4];
  end;
  
  // 
  R_HEX:= '';
  R_HEX_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'HEX');

  if Length(arrData) > 0 then
  begin
    R_HEX:= arrData[0][2];
    R_HEX_UNIT:= arrData[0][4];
  end;
  
  //
  R_CT:= '';
  R_CT_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'CT');

  if Length(arrData) > 0 then
  begin
    R_CT:= arrData[0][2];
    R_CT_UNIT:= arrData[0][4];
  end;
  
  // 
  R_FIX:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'FIX');

  if Length(arrData) > 0 then
  begin
    R_FIX:= arrData[0][2];
  end;
  
  // 
  R_AREA0:= '';
  R_AREA0_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'Area0');

  if Length(arrData) > 0 then
  begin
    R_AREA0:= arrData[0][2];
    R_AREA0_UNIT:= arrData[0][4];
  end;
  
  //
  R_AREA100:= '';
  R_AREA100_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'Area100');

  if Length(arrData) > 0 then
  begin
    R_AREA100:= arrData[0][2];
    R_AREA100_UNIT:= arrData[0][4];
  end;
  
  //  
  R_AREA200:= '';
  R_AREA200_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'Area200');

  if Length(arrData) > 0 then
  begin
    R_AREA200:= arrData[0][2];
    R_AREA200_UNIT:= arrData[0][4];
  end;
  
  // 
  R_AREA300:= '';
  R_AREA300_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'Area300');

  if Length(arrData) > 0 then
  begin
    R_AREA300:= arrData[0][2];
    R_AREA300_UNIT:= arrData[0][4];
  end;
  
  // 
  R_AREA400:= '';
  R_AREA400_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'Area400');

  if Length(arrData) > 0 then
  begin
    R_AREA400:= arrData[0][2];
    R_AREA400_UNIT:= arrData[0][4];
  end;
  
  //
  R_AREA500:= '';
  R_AREA500_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'Area500');

  if Length(arrData) > 0 then
  begin
    R_AREA500:= arrData[0][2];
    R_AREA500_UNIT:= arrData[0][4];
  end;
  
  //
  R_AREA600:= '';
  R_AREA600_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'Area600');

  if Length(arrData) > 0 then
  begin
    R_AREA600:= arrData[0][2];
    R_AREA600_UNIT:= arrData[0][4];
  end;
  
  // 
  R_AREA700:= '';
  R_AREA700_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'Area700');

  if Length(arrData) > 0 then
  begin
    R_AREA700:= arrData[0][2];
    R_AREA700_UNIT:= arrData[0][4];
  end;
  
  // 
  R_AREA800:= '';
  R_AREA800_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'Area800');

  if Length(arrData) > 0 then
  begin
    R_AREA800:= arrData[0][2];
    R_AREA800_UNIT:= arrData[0][4];
  end;
  
  // 
  R_AREA900:= '';
  R_AREA900_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'Area900');

  if Length(arrData) > 0 then
  begin
    R_AREA900:= arrData[0][2];
    R_AREA900_UNIT:= arrData[0][4];
  end;
  
  // 
  R_APEX3:= '';
  R_APEX3_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'Apex3');

  if Length(arrData) > 0 then
  begin
    R_APEX3:= arrData[0][2];
    R_APEX3_UNIT:= arrData[0][4];
  end;
  
  // 
  R_APEX4:= '';
  R_APEX4_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'Apex4');

  if Length(arrData) > 0 then
  begin
    R_APEX4:= arrData[0][2];
    R_APEX4_UNIT:= arrData[0][4];
  end;
  
  // 
  R_APEX5:= '';
  R_APEX5_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'Apex5');

  if Length(arrData) > 0 then
  begin
    R_APEX5:= arrData[0][2];
    R_APEX5_UNIT:= arrData[0][4];
  end;
  
  // 
  R_APEX6:= '';
  R_APEX6_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'Apex6');

  if Length(arrData) > 0 then
  begin
    R_APEX6:= arrData[0][2];
    R_APEX6_UNIT:= arrData[0][4];
  end;
  
  // 
  R_APEX7:= '';
  R_APEX7_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'Apex7');

  if Length(arrData) > 0 then
  begin
    R_APEX7:= arrData[0][2];
    R_APEX7_UNIT:= arrData[0][4];
  end;
  
  // 
  R_APEX8:= '';
  R_APEX8_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'Apex8');

  if Length(arrData) > 0 then
  begin
    R_APEX8:= arrData[0][2];
    R_APEX8_UNIT:= arrData[0][4];
  end;
  
  //
  R_APEX9:= '';
  R_APEX9_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'Apex9');

  if Length(arrData) > 0 then
  begin
    R_APEX9:= arrData[0][2];
    R_APEX9_UNIT:= arrData[0][4];
  end;
  
  // 
  R_APEX10:= '';
  R_APEX10_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'Apex10');

  if Length(arrData) > 0 then
  begin
    R_APEX10:= arrData[0][2];
    R_APEX10_UNIT:= arrData[0][4];
  end;
  
  // 
  R_IMAGE_BMP:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_04 + 'Image/FileName');  
  
  if Length(arrData) > 0 then
  begin
    R_IMAGE_BMP:= R_IMAGE_BMP + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_ARCHIVE_NUMBER + FOutputLineSeparator + '0001' + FOutputLineSeparator;
    R_IMAGE_BMP:= R_IMAGE_BMP + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_FORMAT + FOutputLineSeparator + 'BMP' + FOutputLineSeparator;
    R_IMAGE_BMP:= R_IMAGE_BMP + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_DESCRIPTION + FOutputLineSeparator + 'Bild Messung' + FOutputLineSeparator;
    R_IMAGE_BMP:= R_IMAGE_BMP + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_URL + FOutputLineSeparator + arrData[0][2] + FOutputLineSeparator;
  end;  

  //
  strXPATH_EXPRESSION_05:= XPATH_EXPRESSION_05;
  strXPATH_EXPRESSION_05:= T2WStringReplace(strXPATH_EXPRESSION_05, MEASURE_TYPE, MeasureType, False, False);
    
  // R_Image
  R_Image:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_05); 
  
  if (Length(arrData) > 0) then
  begin
    R_Image:= R_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_ARCHIVE_NUMBER + FOutputLineSeparator + '0002' + FOutputLineSeparator;
    R_Image:= R_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_FORMAT + FOutputLineSeparator + 'JPG' + FOutputLineSeparator;
    R_Image:= R_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_DESCRIPTION + FOutputLineSeparator + 'Bild Messung' + FOutputLineSeparator;
    R_Image:= R_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_URL + FOutputLineSeparator + arrData[0][2] + FOutputLineSeparator;
  end
  else
  begin
    strXPATH_EXPRESSION_08:= XPATH_EXPRESSION_08;
    strXPATH_EXPRESSION_08:= T2WStringReplace(strXPATH_EXPRESSION_08, MEASURE_TYPE, MeasureType, False, False);
  
    R_Image:= '';
  
    SetLength(arrData, 0);
    arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_08); 
  
    if (Length(arrData) > 0) then
    begin
      R_Image:= R_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_ARCHIVE_NUMBER + FOutputLineSeparator + '0002' + FOutputLineSeparator;
      R_Image:= R_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_FORMAT + FOutputLineSeparator + 'BMP' + FOutputLineSeparator;
      R_Image:= R_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_DESCRIPTION + FOutputLineSeparator + 'Bild Messung' + FOutputLineSeparator;
      R_Image:= R_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_URL + FOutputLineSeparator + arrData[0][2] + FOutputLineSeparator;
    end;  
  end;
  
  // -----------------------------------------------------------------
  //
  // -----------------------------------------------------------------  
  
  // main xpath part for left eye
  strXPATH_EXPRESSION_06:= XPATH_EXPRESSION_06;
  strXPATH_EXPRESSION_06:= T2WStringReplace(strXPATH_EXPRESSION_06, MEASURE_TYPE, MeasureType, False, False); 
  
  //
  L_NUM:= '';
  L_NUM_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'NUM');

  if Length(arrData) > 0 then
  begin
    L_NUM:= arrData[0][2];
    L_NUM_UNIT:= arrData[0][4];
  end;
  
  // 
  L_CD:= '';
  L_CD_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'CD');

  if Length(arrData) > 0 then
  begin
    L_CD:= arrData[0][2];
    L_CD_UNIT:= arrData[0][4];
  end;   
  
  //  
  L_AVG:= '';
  L_AVG_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'AVG');

  if Length(arrData) > 0 then
  begin
    L_AVG:= arrData[0][2];
    L_AVG_UNIT:= arrData[0][4];
  end;  
  
  //  
  L_SD:= '';
  L_SD_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'SD');

  if Length(arrData) > 0 then
  begin
    L_SD:= arrData[0][2];
    L_SD_UNIT:= arrData[0][4];
  end;
  
  //    
  L_CV:= '';
  L_CV_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'CV');

  if Length(arrData) > 0 then
  begin
    L_CV:= arrData[0][2];
    L_CV_UNIT:= arrData[0][4];
  end;
  
  //  
  L_MAX:= '';
  L_MAX_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'MAX');

  if Length(arrData) > 0 then
  begin
    L_MAX:= arrData[0][2];
    L_MAX_UNIT:= arrData[0][4];
  end;  
  
  //
  L_MIN:= '';
  L_MIN_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'MIN');

  if Length(arrData) > 0 then
  begin
    L_MIN:= arrData[0][2];
    L_MIN_UNIT:= arrData[0][4];
  end;  
  
  // 
  L_HEX:= '';
  L_HEX_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'HEX');

  if Length(arrData) > 0 then
  begin
    L_HEX:= arrData[0][2];
    L_HEX_UNIT:= arrData[0][4];
  end;
  
  //
  L_CT:= '';
  L_CT_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'CT');

  if Length(arrData) > 0 then
  begin
    L_CT:= arrData[0][2];
    L_CT_UNIT:= arrData[0][4];
  end;
  
  // 
  L_FIX:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'FIX');

  if Length(arrData) > 0 then
  begin
    L_FIX:= arrData[0][2];
  end;  
  
  // 
  L_AREA0:= '';
  L_AREA0_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'Area0');

  if Length(arrData) > 0 then
  begin
    L_AREA0:= arrData[0][2];
    L_AREA0_UNIT:= arrData[0][4];
  end;
  
  //
  L_AREA100:= '';
  L_AREA100_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'Area100');

  if Length(arrData) > 0 then
  begin
    L_AREA100:= arrData[0][2];
    L_AREA100_UNIT:= arrData[0][4];
  end;
  
  //  
  L_AREA200:= '';
  L_AREA200_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'Area200');

  if Length(arrData) > 0 then
  begin
    L_AREA200:= arrData[0][2];
    L_AREA200_UNIT:= arrData[0][4];
  end;  
  
  // 
  L_AREA300:= '';
  L_AREA300_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'Area300');

  if Length(arrData) > 0 then
  begin
    L_AREA300:= arrData[0][2];
    L_AREA300_UNIT:= arrData[0][4];
  end;
  
  // 
  L_AREA400:= '';
  L_AREA400_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'Area400');

  if Length(arrData) > 0 then
  begin
    L_AREA400:= arrData[0][2];
    L_AREA400_UNIT:= arrData[0][4];
  end;
  
  //
  L_AREA500:= '';
  L_AREA500_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'Area500');

  if Length(arrData) > 0 then
  begin
    L_AREA500:= arrData[0][2];
    L_AREA500_UNIT:= arrData[0][4];
  end;  
  
  //
  L_AREA600:= '';
  L_AREA600_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'Area600');

  if Length(arrData) > 0 then
  begin
    L_AREA600:= arrData[0][2];
    L_AREA600_UNIT:= arrData[0][4];
  end;
  
  // 
  L_AREA700:= '';
  L_AREA700_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'Area700');

  if Length(arrData) > 0 then
  begin
    L_AREA700:= arrData[0][2];
    L_AREA700_UNIT:= arrData[0][4];
  end;  
  
  // 
  L_AREA800:= '';
  L_AREA800_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'Area800');

  if Length(arrData) > 0 then
  begin
    L_AREA800:= arrData[0][2];
    L_AREA800_UNIT:= arrData[0][4];
  end;
  
  // 
  L_AREA900:= '';
  L_AREA900_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'Area900');

  if Length(arrData) > 0 then
  begin
    L_AREA900:= arrData[0][2];
    L_AREA900_UNIT:= arrData[0][4];
  end;
  
  // 
  L_APEX3:= '';
  L_APEX3_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'Apex3');

  if Length(arrData) > 0 then
  begin
    L_APEX3:= arrData[0][2];
    L_APEX3_UNIT:= arrData[0][4];
  end;  
  
  // 
  L_APEX4:= '';
  L_APEX4_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'Apex4');

  if Length(arrData) > 0 then
  begin
    L_APEX4:= arrData[0][2];
    L_APEX4_UNIT:= arrData[0][4];
  end;
  
  // 
  L_APEX5:= '';
  L_APEX5_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'Apex5');

  if Length(arrData) > 0 then
  begin
    L_APEX5:= arrData[0][2];
    L_APEX5_UNIT:= arrData[0][4];
  end;
  
  // 
  L_APEX6:= '';
  L_APEX6_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'Apex6');

  if Length(arrData) > 0 then
  begin
    L_APEX6:= arrData[0][2];
    L_APEX6_UNIT:= arrData[0][4];
  end;
  
  // 
  L_APEX7:= '';
  L_APEX7_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'Apex7');

  if Length(arrData) > 0 then
  begin
    L_APEX7:= arrData[0][2];
    L_APEX7_UNIT:= arrData[0][4];
  end;
  
  // 
  L_APEX8:= '';
  L_APEX8_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'Apex8');

  if Length(arrData) > 0 then
  begin
    L_APEX8:= arrData[0][2];
    L_APEX8_UNIT:= arrData[0][4];
  end;  
  
  //
  L_APEX9:= '';
  L_APEX9_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'Apex9');

  if Length(arrData) > 0 then
  begin
    L_APEX9:= arrData[0][2];
    L_APEX9_UNIT:= arrData[0][4];
  end;  
  
  // 
  L_APEX10:= '';
  L_APEX10_UNIT:= '';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'Apex10');

  if Length(arrData) > 0 then
  begin
    L_APEX10:= arrData[0][2];
    L_APEX10_UNIT:= arrData[0][4];
  end;  
  
  // 
  L_IMAGE_BMP:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_06 + 'Image/FileName');  
  
  if Length(arrData) > 0 then
  begin
    L_IMAGE_BMP:= L_IMAGE_BMP + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_ARCHIVE_NUMBER + FOutputLineSeparator + '0003' + FOutputLineSeparator;
    L_IMAGE_BMP:= L_IMAGE_BMP + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_FORMAT + FOutputLineSeparator + 'BMP' + FOutputLineSeparator;
    L_IMAGE_BMP:= L_IMAGE_BMP + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_DESCRIPTION + FOutputLineSeparator + 'Bild Messung' + FOutputLineSeparator;
    L_IMAGE_BMP:= L_IMAGE_BMP + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_URL + FOutputLineSeparator + arrData[0][2] + FOutputLineSeparator;
  end;  

  //
  strXPATH_EXPRESSION_07:= XPATH_EXPRESSION_07;
  strXPATH_EXPRESSION_07:= T2WStringReplace(strXPATH_EXPRESSION_07, MEASURE_TYPE, MeasureType, False, False);
    
  // L_Image
  L_Image:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_07); 
  
  if Length(arrData) > 0 then
  begin
    L_Image:= L_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_ARCHIVE_NUMBER + FOutputLineSeparator + '0004' + FOutputLineSeparator;
    L_Image:= L_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_FORMAT + FOutputLineSeparator + 'JPG' + FOutputLineSeparator;
    L_Image:= L_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_DESCRIPTION + FOutputLineSeparator + 'Bild Messung' + FOutputLineSeparator;
    L_Image:= L_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_URL + FOutputLineSeparator + arrData[0][2] + FOutputLineSeparator;
  end
  else
  begin
    strXPATH_EXPRESSION_09:= XPATH_EXPRESSION_09;
    strXPATH_EXPRESSION_09:= T2WStringReplace(strXPATH_EXPRESSION_09, MEASURE_TYPE, MeasureType, False, False);
  
    L_Image:= '';
  
    SetLength(arrData, 0);
    arrData:= DoPSGetXMLData(strXPATH_EXPRESSION_09); 
  
    if Length(arrData) > 0 then
    begin
      L_Image:= L_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_ARCHIVE_NUMBER + FOutputLineSeparator + '0004' + FOutputLineSeparator;
      L_Image:= L_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_FORMAT + FOutputLineSeparator + 'BMP' + FOutputLineSeparator;
      L_Image:= L_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_DESCRIPTION + FOutputLineSeparator + 'Bild Messung' + FOutputLineSeparator;
      L_Image:= L_Image + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_URL + FOutputLineSeparator + arrData[0][2] + FOutputLineSeparator;
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

  R_Line:= R_Line + 'R.:';
  R_Line:= R_Line + 'NUM= ' + R_NUM + ' ';
  R_Line:= R_Line + 'CD= ' + R_CD + ' ';
  R_Line:= R_Line + 'AVG= ' + R_AVG + ' ';
  R_Line:= R_Line + 'SD= ' + R_SD + ' ';
  R_Line:= R_Line + 'CV= ' + R_CV + ' ';
  R_Line:= R_Line + 'MAX= ' + R_MAX + ' ';
  R_Line:= R_Line + 'MIN= ' + R_MIN + ' ';
        
  R_Line:= Trim(R_Line); 

  if R_Line <> '' then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + R_Line + FOutputLineSeparator;
  end;  
  
  R_Line:= '';     
        
  R_Line:= R_Line + 'R.:';        
  R_Line:= R_Line + 'HEX= ' + R_HEX + ' ';
  R_Line:= R_Line + 'CT= ' + R_CT + ' ';  
  R_Line:= R_Line + 'FIX= ' + R_FIX;
  
  R_Line:= Trim(R_Line);
  
  if R_Line <> '' then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + R_Line + FOutputLineSeparator;
  end;  

  // Add left eye
  L_Line:= '';

  L_Line:= L_Line + 'L.:';
  L_Line:= L_Line + 'NUM= ' + L_NUM + ' ';
  L_Line:= L_Line + 'CD= ' + L_CD + ' ';
  L_Line:= L_Line + 'AVG= ' + L_AVG + ' ';
  L_Line:= L_Line + 'SD= ' + L_SD + ' ';
  L_Line:= L_Line + 'CV= ' + L_CV + ' ';
  L_Line:= L_Line + 'MAX= ' + L_MAX + ' ';
  L_Line:= L_Line + 'MIN= ' + L_MIN + ' ';
        
  L_Line:= Trim(L_Line); 
        
  if L_Line <> '' then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
  end;

  L_Line:= '';     
        
  L_Line:= L_Line + 'L.:';        
  L_Line:= L_Line + 'HEX= ' + L_HEX + ' ';
  L_Line:= L_Line + 'CT= ' + L_CT + ' ';  
  L_Line:= L_Line + 'FIX= ' + L_FIX;

  L_Line:= Trim(L_Line);

  if L_Line <> '' then
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

  // Set output
  FParsedDataString:= RawByteString(ParsedData);
end.
