const
  VERSION = '1.0.2.40';
  DATE = '22.07.2020 10:18:12';
  TEXT = 'Copyright (c) 2020 team2work GmbH';

  CSV_LINE_SEPARATOR = #13#10;
  CSV_VALUE_SEPARATOR = ',';

  ARR_DATA_SEPARATOR = '|';

  CSV_IDENTIFIER_01 = '[PT_ID]';

  CSV_IDENTIFIER_02 = '[RL],Left';
  CSV_IDENTIFIER_03 = '[NUMBER]';
  CSV_IDENTIFIER_04 = '[DENSITY]';
  CSV_IDENTIFIER_05 = '[THK]';

  CSV_IDENTIFIER_06 = '[RL],Right';
  CSV_IDENTIFIER_07 = '[NUMBER]';
  CSV_IDENTIFIER_08 = '[DENSITY]';
  CSV_IDENTIFIER_09 = '[THK]';

  CSV_IDENTIFIER_10 = '[FILES_N]';
  CSV_IDENTIFIER_11 = '[FILE]';

  GDT_FID_PATIENT_ID = '3000';
  GDT_FID_MEASURE_DATA = '6228';
  GDT_FID_COMMENT = '6227';
  GDT_FID_FILE_ARCHIVE_NUMBER = '6302';
  GDT_FID_FILE_FORMAT = '6303';
  GDT_FID_FILE_DESCRIPTION = '6304';
  GDT_FID_FILE_URL = '6305';
  
  EXTERNAL_FILES_IMAGES_FORMAT_1 = 'JPG';  
  EXTERNAL_FILES_IMAGES_FORMAT_2 = 'BMP'; 
  
var
  arrData: TStringArray;
  arrData2: TStringArray;
  arrData3: TStringArray;
  ParsedData, S, PatientID: String;
  R_DataBlockEntered, L_DataBlockEntered: Boolean;
  arrImages: TStringArray;
  R_NUMBER, R_DENSITY, R_THK, L_NUMBER, L_DENSITY, L_THK: String;
  i, k, ImageCount, ImageCounter: Integer;
  bolAddExternalFilesImagesToGdtFile: Boolean;
  Images: String;
begin
  // Aktiviere den Import aller externen Bildquellen.
  // Verwende "True", damit der Import aktiviert ist, benutze "False", damit Bilder
  // nicht importiert und per GDT weiterverarbeitet werden.
  bolAddExternalFilesImagesToGdtFile := True;

  // --- Don't edit script down below ---

  // Clear parsed data string
  FParsedDataString := '';

  // Get array of CSV lines
  arrData := Explode(CSV_LINE_SEPARATOR, FRawDataString, 0);

  if Length(arrData) <= 0 then
   begin
    FLastErrorCode := -4;
    FLastErrorMessage := 'Keine CSV-Zeilen für die Verarbeitung verfügbar';

    DoPSError();

    Exit;
  end;
  
  PatientID := '';
	
  R_DataBlockEntered := False;
  R_NUMBER := '';
  R_DENSITY := '';
  R_THK := '';
	
  L_DataBlockEntered := False;
  L_NUMBER := '';
  L_DENSITY := '';
  L_THK := '';

  ImageCount := 0;
  ImageCounter := 0;
  
  for i := 0 to Length(arrData) - 1 do
   begin
    S := arrData[i];
    
    if T2WStartsStr(CSV_IDENTIFIER_01, S) then
     begin
      arrData2 := Explode(CSV_VALUE_SEPARATOR, S, 0);
      
      if Length(arrData2) <> 2 then
      begin
        // TODO: May be show error here
        Continue;
      end;
      
      PatientID:= arrData2[1];
      
      // Overwrite patient ID if it's not the same
      if PatientID <> FPatientID then
       PatientID := FPatientID;
    end;

    //
    if (bolAddExternalFilesImagesToGdtFile) then
     begin
      if T2WStartsStr(CSV_IDENTIFIER_10, S) then
       begin
        arrData2 := Explode(CSV_VALUE_SEPARATOR, S, 0);

        if Length(arrData2) <> 2 then
         begin
          // TODO: May be show error here
          Continue;
        end;

        ImageCount := StrToIntDef(arrData2[1], 0);

        SetLength(arrImages, ImageCount);
      end;

      if (T2WStartsStr(CSV_IDENTIFIER_11, S)) and (ImageCount > 0) and (ImageCounter < ImageCount) then
       begin
        arrData2 := Explode(CSV_VALUE_SEPARATOR, S, 0);

        if Length(arrData2) < 2 then
        begin
          // TODO: May be show error here
          Continue;
        end;

        if Length(arrData2) >= 3 then
         arrImages[ImageCounter] := arrData2[1] + '|' + arrData2[2]
        else
         arrImages[ImageCounter] := arrData2[1];

        ImageCounter := ImageCounter + 1;
      end;
    end;

    // 
    if T2WStartsStr(CSV_IDENTIFIER_06, S) then
     R_DataBlockEntered := True;
    
    if (T2WStartsStr(CSV_IDENTIFIER_03, S)) and (R_DataBlockEntered) then
    begin
      arrData2:= Explode(CSV_VALUE_SEPARATOR, S, 0);
      
      if Length(arrData2) <> 2 then
      begin
        // TODO: May be show error here
        Continue;
      end;
      
      R_NUMBER := arrData2[1];
    end;

    if (T2WStartsStr(CSV_IDENTIFIER_04, S)) and (R_DataBlockEntered) then
    begin
      arrData2:= Explode(CSV_VALUE_SEPARATOR, S, 0);
      
      if Length(arrData2) <> 2 then
      begin
        // TODO: May be show error here
        Continue;
      end;
      
      R_DENSITY := arrData2[1];
    end;

    if (T2WStartsStr(CSV_IDENTIFIER_05, S)) and (R_DataBlockEntered) then
    begin
      arrData2:= Explode(CSV_VALUE_SEPARATOR, S, 0);
      
      if Length(arrData2) <> 2 then
      begin
        // TODO: May be show error here
        Continue;
      end;
      
      R_THK := arrData2[1];
    end;

    //
    if T2WStartsStr(CSV_IDENTIFIER_02, S) then
     L_DataBlockEntered := True;
    
    if (T2WStartsStr(CSV_IDENTIFIER_07, S)) and (L_DataBlockEntered) then
     begin
      arrData2 := Explode(CSV_VALUE_SEPARATOR, S, 0);
      
      if Length(arrData2) <> 2 then
      begin
        // TODO: May be show error here
        Continue;
      end;
      
      L_NUMBER := arrData2[1];
    end;

    if (T2WStartsStr(CSV_IDENTIFIER_08, S)) and (L_DataBlockEntered) then
     begin
      arrData2 := Explode(CSV_VALUE_SEPARATOR, S, 0);

      if Length(arrData2) <> 2 then
      begin
        // TODO: May be show error here
        Continue;
      end;

      L_DENSITY := arrData2[1];
    end;

    if (T2WStartsStr(CSV_IDENTIFIER_09, S)) and (L_DataBlockEntered) then
     begin
      arrData2 := Explode(CSV_VALUE_SEPARATOR, S, 0);

      if Length(arrData2) <> 2 then
      begin
        // TODO: May be show error here
        Continue;
      end;

      L_THK := arrData2[1];
    end;
  end;

  // Build output
  ParsedData := '';

  // Add patient ID
  if (PatientID <> '') then
   begin
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
    ParsedData := ParsedData + PatientID + FOutputLineSeparator;
  end;

  // Add right eye data
  if (R_NUMBER <> '') and (R_DENSITY <> '') and (R_THK <> '') then
  begin
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData := ParsedData + 'R: Anzahl = ' + R_NUMBER + ';';
    ParsedData := ParsedData + ' Dichte = ' + R_DENSITY + ' ' + 'mm2' + ';';
    ParsedData := ParsedData + ' Hornhautdicke = ' + R_THK + ' ' + 'µm' + '' + FOutputLineSeparator;
  end;

  // Add left eye data
  if (L_NUMBER <> '') and (L_DENSITY <> '') and (L_THK <> '') then
  begin
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData := ParsedData + 'L: Anzahl = ' + L_NUMBER + ';';
    ParsedData := ParsedData + ' Dichte = ' + L_DENSITY + ' ' + 'mm2' + ';';
    ParsedData := ParsedData + ' Hornhautdicke = ' + L_THK + ' ' + 'µm' + '' + FOutputLineSeparator;
  end;

  // Add images
  if (bolAddExternalFilesImagesToGdtFile) then
   begin
    Images := '';

    if (Length(arrImages) > 0) then
     begin
      for k := 0 to Length(arrImages) - 1 do
       begin
        arrData3 := Explode(ARR_DATA_SEPARATOR, arrImages[k], 0);

        if Length(arrData3) > 0 then
         begin
          Images := Images + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_ARCHIVE_NUMBER + FOutputLineSeparator + IntToStr(k+1) + FOutputLineSeparator;

          if T2WEndsStr('.bmp', arrData3[0]) then
           begin
            Images := Images + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_FORMAT + FOutputLineSeparator + EXTERNAL_FILES_IMAGES_FORMAT_2 + FOutputLineSeparator;
          end;

          if T2WEndsStr('.jpg', arrData3[0]) then
           begin
            Images := Images + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_FORMAT + FOutputLineSeparator + EXTERNAL_FILES_IMAGES_FORMAT_1 + FOutputLineSeparator;
          end;

          Images := Images + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_DESCRIPTION + FOutputLineSeparator + arrData3[1] + FOutputLineSeparator;
          Images := Images + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FILE_URL + FOutputLineSeparator + DoPSGetFullPath(arrData3[0]) + FOutputLineSeparator;
        end;
      end;
    end;

    ParsedData := ParsedData + Images;
  end;

  // Set output
  FParsedDataString := RawByteString(ParsedData);
end.