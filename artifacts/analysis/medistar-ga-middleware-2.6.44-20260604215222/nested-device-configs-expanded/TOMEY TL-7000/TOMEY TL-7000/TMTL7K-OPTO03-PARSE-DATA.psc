const
  VERSION = '1.0.21.20';
  DATE = '02.07.2025 08:28:32';
  TEXT = 'Copyright (c) 2025 CompuGroup Medical Deutschland AG';
  
  CSV_LINE_SEPARATOR = #13#10;
  CSV_VALUE_SEPARATOR = ',';
  
  CSV_IDENTIFIER_01 = '[PT_ID]';
  CSV_IDENTIFIER_02 = '[POWER_R]';
  CSV_IDENTIFIER_03 = '[ADD_R]';
  CSV_IDENTIFIER_04 = '[PRISM_SEL_R]'; // muss Wert "1" sein, da nur dann PX und PY gemessen werden
  CSV_IDENTIFIER_05 = '[PRISM_R]';
  CSV_IDENTIFIER_06 = '[UV_R]';
  CSV_IDENTIFIER_07 = '[POWER_L]';
  CSV_IDENTIFIER_08 = '[ADD_L]';
  CSV_IDENTIFIER_09 = '[PRISM_SEL_L]'; // muss Wert "1" sein, da nur dann PX und PY gemessen werden
  CSV_IDENTIFIER_10 = '[PRISM_L]';
  CSV_IDENTIFIER_11 = '[UV_L]';
  
  GDT_FID_PATIENT_ID = '3000';
  GDT_FID_MEASURE_DATA = '6228';
  GDT_FID_COMMENT = '6227';
  GDT_FID_FILE_ARCHIVE_NUMBER = '6302';
  GDT_FID_FILE_FORMAT = '6303';
  GDT_FID_FILE_DESCRIPTION = '6304';
  GDT_FID_FILE_URL = '6305';
  
  GDT_AXIS_MAX_CHAR_COUNT = 3;
  GDT_AXIS_SEPARATOR = ' ';

  GDT_PRISM_MAX_CHAR_COUNT = 5;
  GDT_PRISM_FILL_UP_BLANK_SPACES = False;
  GDT_PRISM_PX_POSITIVE_SUBSTITUTE = 'O';
  GDT_PRISM_PX_NEGATIVE_SUBSTITUTE = 'I';
  GDT_PRISM_PY_POSITIVE_SUBSTITUTE = 'U';
  GDT_PRISM_PY_NEGATIVE_SUBSTITUTE = 'D';

  GDT_ADD_ADD_POSITIVE_SIGN = True;

function FormatPowerValue(const Value: String): String;
var
  S1, S2: String;
begin
  Result:= Value;

  S1:= Value;
  
  if Length(S1) < 6 then
  begin
    if Length(S1) = 5 then
    begin
      S2:= S1[1];
      S1:= Copy(S1, 2, Length(S1) - 1);
      
      Result:= S2 + ' ' + S1;
    end;
  end;
end;

function FormatAxisValue(const Value: String): String;
begin
  Result:= Value;
  
  while Length(Result) < GDT_AXIS_MAX_CHAR_COUNT do
    Result:= GDT_AXIS_SEPARATOR + Result;
end;

function FormatPrismValue(const Value, PositiveSubstitute, NegativeSubstitute: String; FillUpBlanks: Boolean): String;
var
  S1, S2: String;
begin
  Result:= '';

  S1:= Value;
  
  S2:= S1[1];
  S1:= Copy(S1, 2, Length(S1) - 1);
  
  if FillUpBlanks then
    while Length(S1) < GDT_PRISM_MAX_CHAR_COUNT do
      S1:= ' ' + S1;
  
  if S2 = '+' then
    Result:= S1 + ' ' + PositiveSubstitute
  else if S2 = '-' then
    Result:= S1 + ' ' + NegativeSubstitute;
end;

function FormatAddValue(const Value: String; AddPositiveSign: Boolean): String;
begin
  Result:= Value;
  
  if AddPositiveSign then
    Result:= '+ ' + Result;
end;

var
  arrData: TStringArray;
  arrData2: TStringArray;
  ParsedData, S, PatientID: String;
  R_Power, R_Add, R_Prism, R_UV, L_Power, L_Add, L_Prism, L_UV: String;
  R_Line, L_Line, UV_Line: String;
  i, R_PrismType, L_PrismType: Integer;
begin
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
  
  R_Power:= '';
  R_Add:= '';
  R_PrismType:= -1;
  R_Prism:= '';
  R_UV:= '';
  L_Power:= '';
  L_Add:= '';
  L_PrismType:= -1;
  L_Prism:= '';
  L_UV:= '';
  
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
      
      Continue;
    end;
    
    if T2WStartsStr(CSV_IDENTIFIER_02, S) then
    begin
      arrData2:= Explode(CSV_VALUE_SEPARATOR, S, 0);
      
      if Length(arrData2) <> 4 then
      begin
        // TODO: May be show error here
        Continue;
      end;
      
      if arrData2[1] <> '' then
        R_Power:= R_Power + 'S=' + FormatPowerValue(arrData2[1]);
      
      if arrData2[2] <> '' then
      begin
        if R_Power <> '' then
          R_Power:= R_Power + ' ';
        
        R_Power:= R_Power + 'Z=' + FormatPowerValue(arrData2[2]);
      end;
      
      if arrData2[3] <> '' then
        R_Power:= R_Power + '*' + FormatAxisValue(arrData2[3]);
      
      Continue;
    end;
    
    if T2WStartsStr(CSV_IDENTIFIER_03, S) then
    begin
      arrData2:= Explode(CSV_VALUE_SEPARATOR, S, 0);
      
      if (Length(arrData2) < 2) or (Length(arrData2) > 4) then
      begin
        // TODO: May be show error here
        Continue;
      end;
      
      if arrData2[1] <> '' then
        R_Add:= R_Add + 'A=' + FormatAddValue(arrData2[1], GDT_ADD_ADD_POSITIVE_SIGN);
      
      if (Length(arrData2) > 2) then
      begin
        if arrData2[2] <> '' then
        begin
          if R_Add <> '' then
            R_Add:= R_Add + ' ';
        
          R_Add:= R_Add + 'A2=' + FormatAddValue(arrData2[2], GDT_ADD_ADD_POSITIVE_SIGN);
        end;
      end;
      
      Continue;
    end;
    
    if T2WStartsStr(CSV_IDENTIFIER_04, S) then
    begin
      arrData2:= Explode(CSV_VALUE_SEPARATOR, S, 0);
      
      if Length(arrData2) <> 2 then
      begin
        // TODO: May be show error here
        Continue;
      end;
      
      R_PrismType:= StrToIntDef(arrData2[1], 0);
      
      Continue;
    end;
    
    if (T2WStartsStr(CSV_IDENTIFIER_05, S)) and (R_PrismType = 1) then
    begin
      arrData2:= Explode(CSV_VALUE_SEPARATOR, S, 0);
      
      if Length(arrData2) <> 3 then
      begin
        // TODO: May be show error here
        Continue;
      end;
      
      if arrData2[1] <> '' then
        R_Prism:= R_Prism + 'P= ' + FormatPrismValue(arrData2[1], GDT_PRISM_PX_POSITIVE_SUBSTITUTE, GDT_PRISM_PX_NEGATIVE_SUBSTITUTE, GDT_PRISM_FILL_UP_BLANK_SPACES);
      
      if arrData2[2] <> '' then
      begin
        if R_Prism <> '' then
          R_Prism:= R_Prism + ' '
        else
          R_Prism:= R_Prism + 'P= ';
        
        R_Prism:= R_Prism + FormatPrismValue(arrData2[2], GDT_PRISM_PY_POSITIVE_SUBSTITUTE, GDT_PRISM_PY_NEGATIVE_SUBSTITUTE, GDT_PRISM_FILL_UP_BLANK_SPACES);
      end;
      
      Continue;
    end;
    
    if T2WStartsStr(CSV_IDENTIFIER_06, S) then
    begin
      arrData2:= Explode(CSV_VALUE_SEPARATOR, S, 0);
      
      if Length(arrData2) <> 2 then
      begin
        // TODO: May be show error here
        Continue;
      end;
      
      if arrData2[1] <> '' then
        R_UV:= R_UV + 'UV = ' + arrData2[1] + '%';
      
      Continue;
    end;
    
    if T2WStartsStr(CSV_IDENTIFIER_07, S) then
    begin
      arrData2:= Explode(CSV_VALUE_SEPARATOR, S, 0);
      
      if Length(arrData2) <> 4 then
      begin
        // TODO: May be show error here
        Continue;
      end;
      
      if arrData2[1] <> '' then
        L_Power:= L_Power + 'S=' + FormatPowerValue(arrData2[1]);
      
      if arrData2[2] <> '' then
      begin
        if L_Power <> '' then
          L_Power:= L_Power + ' ';
        
        L_Power:= L_Power + 'Z=' + FormatPowerValue(arrData2[2]);
      end;
      
      if arrData2[3] <> '' then
        L_Power:= L_Power + '*' + FormatAxisValue(arrData2[3]);
      
      Continue;
    end;
    
    if T2WStartsStr(CSV_IDENTIFIER_08, S) then
    begin
      arrData2:= Explode(CSV_VALUE_SEPARATOR, S, 0);
      
      if (Length(arrData2) < 2) or (Length(arrData2) > 4) then
      begin
        // TODO: May be show error here
        Continue;
      end;
      
      if arrData2[1] <> '' then
        L_Add:= L_Add + 'A=' + FormatAddValue(arrData2[1], GDT_ADD_ADD_POSITIVE_SIGN);
            
      if (Length(arrData2) > 2) then
      begin
        if arrData2[2] <> '' then
        begin
          if L_Add <> '' then
            L_Add:= L_Add + ' ';
        
          L_Add:= L_Add + 'A2=' + FormatAddValue(arrData2[2], GDT_ADD_ADD_POSITIVE_SIGN);
        end;
      end;
          
      Continue;
    end;
    
    if T2WStartsStr(CSV_IDENTIFIER_09, S) then
    begin
      arrData2:= Explode(CSV_VALUE_SEPARATOR, S, 0);
      
      if Length(arrData2) <> 2 then
      begin
        // TODO: May be show error here
        Continue;
      end;
      
      L_PrismType:= StrToIntDef(arrData2[1], 0);
      
      Continue;
    end;
    
    if (T2WStartsStr(CSV_IDENTIFIER_10, S)) and (L_PrismType = 1) then
    begin
      arrData2:= Explode(CSV_VALUE_SEPARATOR, S, 0);
      
      if Length(arrData2) <> 3 then
      begin
        // TODO: May be show error here
        Continue;
      end;
      
      if arrData2[1] <> '' then
        L_Prism:= L_Prism + 'P= ' + FormatPrismValue(arrData2[1], GDT_PRISM_PX_POSITIVE_SUBSTITUTE, GDT_PRISM_PX_NEGATIVE_SUBSTITUTE, GDT_PRISM_FILL_UP_BLANK_SPACES);
      
      if arrData2[2] <> '' then
      begin
        if L_Prism <> '' then
          L_Prism:= L_Prism + ' '
        else
          L_Prism:= L_Prism + 'P= ';
        
        L_Prism:= L_Prism + FormatPrismValue(arrData2[2], GDT_PRISM_PY_POSITIVE_SUBSTITUTE, GDT_PRISM_PY_NEGATIVE_SUBSTITUTE, GDT_PRISM_FILL_UP_BLANK_SPACES);
      end;
      
      Continue;
    end;
    
    if T2WStartsStr(CSV_IDENTIFIER_11, S) then
    begin
      arrData2:= Explode(CSV_VALUE_SEPARATOR, S, 0);
      
      if Length(arrData2) <> 2 then
      begin
        // TODO: May be show error here
        Continue;
      end;
      
      if arrData2[1] <> '' then
        L_UV:= L_UV + 'UV = ' + arrData2[1] + '%';
      
      Continue;
    end;
  end;
  
  // Build UV
  UV_Line:= '';
  
  if R_UV <> '' then
    UV_Line:= UV_Line + 'R: ' + R_UV;
  
  if L_UV <> '' then
  begin
    if UV_Line <> '' then
      UV_Line:= UV_Line + '; ';
    
    UV_Line:= UV_Line + 'L: ' + L_UV;
  end;
  
  // Build right eye
  R_Line:= '';
  
  if (R_Power <> '') or (R_Prism <> '') or (R_Add <> '') then
  begin
    R_Line:= R_Line + 'R.:';
    
    if R_Power <> '' then
      R_Line:= R_Line + R_Power + ' ';
    
    if R_Prism <> '' then
      R_Line:= R_Line + R_Prism + ' ';
    
    if R_Add <> '' then
      R_Line:= R_Line + R_Add + ' ';
    
    R_Line:= Trim(R_Line);
  end;
  
  // Build left eye
  L_Line:= '';
  
  if (L_Power <> '') or (L_Prism <> '') or (L_Add <> '') then
  begin
    L_Line:= L_Line + 'L.:';
    
    if L_Power <> '' then
      L_Line:= L_Line + L_Power + ' ';
    
    if L_Prism <> '' then
      L_Line:= L_Line + L_Prism + ' ';
    
    if L_Add <> '' then
      L_Line:= L_Line + L_Add + ' ';
    
    L_Line:= Trim(L_Line);
  end;
  
  // Build output
  ParsedData:= '';

  // Add patient ID
  if PatientID <> '' then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
    ParsedData:= ParsedData + PatientID + FOutputLineSeparator;
  end;
  
  // Add UV
  if UV_Line <> '' then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_COMMENT + FOutputLineSeparator;
    ParsedData:= ParsedData + UV_Line + FOutputLineSeparator;
  end;
  
  // Add right eye
  if R_Line <> '' then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + R_Line + FOutputLineSeparator;
  end;
  
  // Add left eye
  if L_Line <> '' then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
  end;
  
  // Set output
  FParsedDataString:= RawByteString(ParsedData);
end.

