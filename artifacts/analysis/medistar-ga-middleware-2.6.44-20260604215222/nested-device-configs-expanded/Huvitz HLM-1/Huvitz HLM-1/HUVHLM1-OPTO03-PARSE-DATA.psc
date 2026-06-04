const
  VERSION = '1.0.55.61';
  DATE = '25.09.2024 14:36:47';
  TEXT = 'Copyright (c) 2024 team2work GmbH';
  
  DATA_SEPARATOR_01 = '   ';
  DATA_SEPARATOR_02 = ' ';
  DATA_SEPARATOR_03 = '=';
  
  ARRAY_SIZE_01 = 5;
  ARRAY_SIZE_02 = 9;
  ARRAY_SIZE_03 = 2;
  
  RIGHT_EYE_DATA_ARRAY_INDEX = 2;
  LEFT_EYE_DATA_ARRAY_INDEX  = 3;
  
  OUTPUT_DATA_NAME_VALUE_SEPARATOR = '=';
  OUTPUT_DATA_VALUE_SEPARATOR      = ' ';
  OUTPUT_UV_INDEX_SEPARATOR        = ';';
  
  OUTPUT_DATA_LINE_SEPARATOR       = #10;
  PARAM_NAME_ARRAY_INDEX           = 0;
  PARAM_VALUE_ARRAY_INDEX          = 1;
  
  GDT_FID_MEASURE_DATA = '6228';
  GDT_FID_COMMENT      = '6227';
  
  GDT_LINE_PREFIX              = '  ';
  GDT_SIGN_SEPARATOR           = ' ';
  GDT_AXIS_SEPARATOR           = ' ';
  GDT_AXIS_SEPARATOR_MAX_COUNT = 3;
  
  DATA_FORMAT = 'LMTORK';

var
  Data, Sep1, Sep2, Sep3, EyeData, TempData, ParsedData, strGDT_LINE_PREFIX: String;
  arrData1, arrData2, arrData3: TStringArray;
  i, j: Integer;
  ParamName, ParamValue, S1, PrismData: String;
  bolAddUVMeasureIndexToGDT: Boolean;
  bolUseStrictValueChecking, bolAddPrefixForEachEyeLine, bolAddVDValueToOutput, bolAddPDValueToOutput: Boolean;
  bolAddSign, bolAddSignSeparator, bolAddAxisSeparator: Boolean;
  R_Line, L_Line, ActiveLine: String;

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
  S:= T2WStringReplace(S, '+', '', False, False);
  
  if (AddAxisSeparator) then
    while Length(S) < GDT_AXIS_SEPARATOR_MAX_COUNT do
      S:= GDT_AXIS_SEPARATOR + S;
  
  Result:= Result + S;
end;

begin
  // Der UV Index aus der Messung wird mittels BOOLEAN Variable per GDT exportiert.
  // Soll der UV Index als z.B. "Y" Zeile (GDTID 6227) in die MD eingetragen werden,
  // dann setzt man die BOOLEAN Variable auf True andernfalls auf False.
  // Bitte Schreibweise und Zeilenende ";" beachten.
  bolAddUVMeasureIndexToGDT:= True;
  
  // Verwenden Sie "True", damit alle Werte auf richtigkeit und Vollständigkeit
  // geprüft werden. Wählen Sie "False", damit die Prüfung auf die AIS Anwendung
  // verlagert wird.
  bolUseStrictValueChecking:= False;
    
  // Verwende "True", damit vor jeder Zeile (pro Auge) Anzahl x Leerzeichen als Prefix
  // vorangestellt werden, benutze "False", damit dies nicht geschiet.
  bolAddPrefixForEachEyeLine:= False;
    
  // Verwende "True", damit der VD Wert an die V1 Zeile angehängt wird,
  // benutze "False", damit der VD Wert ignoriert werden kann.
  bolAddVDValueToOutput:= True;

  // Verwende "True", damit der PD Wert an die V1 Zeile angehängt wird,
  // benutze "False", damit der PD Wert ignoriert werden kann.
  bolAddPDValueToOutput:= True;
      
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

  if Length(FRawDataString) <= 0 then
  begin
    FLastErrorCode:= -2;
    FLastErrorMessage:= 'Keine COM-Daten für die Verarbeitung verfügbar';

    DoPSError;

    Exit;
  end;

  if (bolAddPrefixForEachEyeLine) then
  begin
    strGDT_LINE_PREFIX:= GDT_LINE_PREFIX;
  end;

  TempData:= '';
  ParsedData:= '';
  PrismData:= '';

  Sep1:= DATA_SEPARATOR_01;
  Sep2:= DATA_SEPARATOR_02;
  Sep3:= DATA_SEPARATOR_03;

  Data:= String(FRawDataString);
  Data:= Trim(Data);

  ActiveLine:= '';
  
  R_Line:= '';
  L_Line:= '';

  // Get array from 1st separator
  arrData1:= Explode(Sep1, Data, 0);

  if (Length(arrData1) <> ARRAY_SIZE_01) then
  begin
    FLastErrorCode:= -3;
    FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): "' + IntToStr(ARRAY_SIZE_01) + '" Parameter erwartet, "' + IntToStr(Length(arrData1)) + '" Parameter gefunden';

    if (bolUseStrictValueChecking) then
    begin
      DoPSError;

      Exit;
    end;
  end;

  // ---

  for i:= RIGHT_EYE_DATA_ARRAY_INDEX to LEFT_EYE_DATA_ARRAY_INDEX do
  begin
    // Process right eye
    EyeData:= arrData1[i];

    // Get array from 2nd separator
    arrData2:= Explode(Sep2, EyeData, 0);

    if (Length(arrData2) <> ARRAY_SIZE_02) then
    begin
      if i = RIGHT_EYE_DATA_ARRAY_INDEX then
      begin
        FLastErrorCode:= -4;
        FLastErrorMessage:= 'Kein valides Datenformat (Rechtes Auge): "' + IntToStr(ARRAY_SIZE_02) + '" Parameter erwartet, "' + IntToStr(Length(arrData2)) + '" Parameter gefunden';
      end
      else if i = LEFT_EYE_DATA_ARRAY_INDEX then
      begin
        FLastErrorCode:= -6;
        FLastErrorMessage:= 'Kein valides Datenformat (Linkes Auge): "' + IntToStr(ARRAY_SIZE_02) + '" Parameter erwartet, "' + IntToStr(Length(arrData2)) + '" Parameter gefunden';
      end;

      if (bolUseStrictValueChecking) then
      begin
        DoPSError;

        Exit;
      end;
    end;

    for j:= 0 to Length(arrData2) - 1 do
    begin
      S1:= arrData2[j];

      if (S1 = '') then
        Continue;

      if (S1 = 'R:') then
      begin
        R_Line:= 'R.:';
        ActiveLine:= 'R';

        Continue;
      end;

      if (S1 = 'L:') then
      begin
        L_Line:= 'L.:';
        ActiveLine:= 'L';
        
        Continue;
      end;

      arrData3:= Explode(Sep3, S1, 0);

      if Length(arrData3) <> ARRAY_SIZE_03 then
      begin
        if i = RIGHT_EYE_DATA_ARRAY_INDEX then
        begin
          FLastErrorCode:= -5;
          FLastErrorMessage:= 'Kein valides Datenformat (Rechtes Auge - Einzelner Parameter): "' + IntToStr(ARRAY_SIZE_03) + '" Parameter erwartet, "' + IntToStr(Length(arrData3)) + '" Parameter gefunden';
        end
        else if i = LEFT_EYE_DATA_ARRAY_INDEX then
        begin
          FLastErrorCode:= -7;
          FLastErrorMessage:= 'Kein valides Datenformat (Linkes Auge - Einzelner Parameter): "' + IntToStr(ARRAY_SIZE_03) + '" Parameter erwartet, "' + IntToStr(Length(arrData3)) + '" Parameter gefunden';
        end;

        if (bolUseStrictValueChecking) then
        begin
          DoPSError;

          Exit;
        end;
      end;

      if (Length(arrData3) = 1) then
      begin
        SetLength(arrData3, 2);
        arrData3[PARAM_VALUE_ARRAY_INDEX] := '0';
      end;

      // Sphere
      if (T2WStartsStr('S=', S1)) then
      begin
        ParamValue:= arrData3[PARAM_VALUE_ARRAY_INDEX];
        ParamValue:= FormatSignValue('S=', ParamValue, bolAddSign, bolAddSignSeparator);

        if (ActiveLine = 'R') then
        begin
          R_Line:= R_Line + '' + ParamValue;
        end
        else if (ActiveLine = 'L') then
        begin
          L_Line:= L_Line + '' + ParamValue;
        end;        

        Continue;
      end;

      // Cylinder
      if (T2WStartsStr('C=', S1)) then
      begin
        ParamValue:= arrData3[PARAM_VALUE_ARRAY_INDEX];
        ParamValue:= FormatSignValue('Z=', ParamValue, bolAddSign, bolAddSignSeparator);

        if (ActiveLine = 'R') then
        begin
          R_Line:= R_Line + ' ' + ParamValue;
        end
        else if (ActiveLine = 'L') then
        begin
          L_Line:= L_Line + ' ' + ParamValue;
        end;

        Continue;
      end;

      // Axis
      if (T2WStartsStr('A=', S1)) then
      begin
        ParamValue:= arrData3[PARAM_VALUE_ARRAY_INDEX];
        ParamValue:= FormatAxisValue('*', ParamValue, bolAddAxisSeparator);

        if (ActiveLine = 'R') then
        begin
          R_Line:= R_Line + '' + ParamValue;
        end
        else if (ActiveLine = 'L') then
        begin
          L_Line:= L_Line + '' + ParamValue;
        end;

        Continue;
      end;

      // Prism
      if (T2WStartsStr('PX=', S1)) then
      begin
        ParamValue:= arrData3[PARAM_VALUE_ARRAY_INDEX];
        ParamValue:= FormatSignValue('P= ', ParamValue, bolAddSign, False);

        PrismData:= ParamValue;

        if T2WStartsStr('-', ParamValue) then
          PrismData:= PrismData + OUTPUT_DATA_VALUE_SEPARATOR + 'O'
        else
          PrismData:= PrismData + OUTPUT_DATA_VALUE_SEPARATOR + 'I';

        Continue;
      end;

      //
      if (T2WStartsStr('PY=', S1)) then
      begin
        ParamValue:= arrData3[PARAM_VALUE_ARRAY_INDEX];
        ParamValue:= FormatSignValue('P=', ParamValue, bolAddSign, False);

        ParamValue:= T2WStringReplace(ParamValue, 'P=', '', True, False);

        PrismData:= PrismData + OUTPUT_DATA_VALUE_SEPARATOR + ParamValue;

        if T2WStartsStr('-', ParamValue) then
          PrismData:= PrismData + OUTPUT_DATA_VALUE_SEPARATOR + 'D'
        else
          PrismData:= PrismData + OUTPUT_DATA_VALUE_SEPARATOR + 'U';

        Continue;
      end;

      if (PrismData <> '') then
      begin     
        if (ActiveLine = 'R') then
        begin
          R_Line:= R_Line + ' ' + PrismData;
        end
        else if (ActiveLine = 'L') then
        begin
          L_Line:= L_Line + ' ' + PrismData;
        end;

        PrismData:= '';
      end;

      // ADD
      if (T2WStartsStr('ADD=', S1)) then
      begin
        ParamValue:= arrData3[PARAM_VALUE_ARRAY_INDEX];
        ParamValue:= FormatSignValue('A=', ParamValue, bolAddSign, bolAddSignSeparator);

        if (ActiveLine = 'R') then
        begin
          R_Line:= R_Line + ' ' + ParamValue;
        end
        else if (ActiveLine = 'L') then
        begin
          L_Line:= L_Line + ' ' + ParamValue;
        end;

        Continue;
      end;

      // PD
      if (T2WStartsStr('PD=', S1)) and (bolAddPDValueToOutput) then
      begin
        ParamValue:= arrData3[PARAM_VALUE_ARRAY_INDEX];
        ParamValue:= FormatSignValue('PD=', ParamValue, bolAddSign, bolAddSignSeparator);

        if (ActiveLine = 'R') then
        begin
          R_Line:= R_Line + ' ' + ParamValue;
        end
        else if (ActiveLine = 'L') then
        begin
          L_Line:= L_Line + ' ' + ParamValue;
        end;

        Continue;
      end;

      // UV
      if (T2WStartsStr('UR=', S1)) then
      begin
        ParamName:= arrData3[PARAM_NAME_ARRAY_INDEX];

        ParamValue:= arrData3[PARAM_VALUE_ARRAY_INDEX];
        ParamValue:= ParamValue + '%';

        ParamName:= T2WStringReplace(ParamName, 'UR', 'R: UV', False, False);

        TempData:= TempData + ParamName + OUTPUT_DATA_VALUE_SEPARATOR + OUTPUT_DATA_NAME_VALUE_SEPARATOR + OUTPUT_DATA_VALUE_SEPARATOR + ParamValue;

        Continue;
      end;

      // UV
      if (T2WStartsStr('UL=', S1)) then
      begin
        ParamName:= arrData3[PARAM_NAME_ARRAY_INDEX];

        ParamValue:= arrData3[PARAM_VALUE_ARRAY_INDEX];
        ParamValue:= ParamValue + '%';

        ParamName:= T2WStringReplace(ParamName, 'UL', 'L: UV', False, False);

        TempData:= TempData + OUTPUT_UV_INDEX_SEPARATOR + OUTPUT_DATA_VALUE_SEPARATOR + ParamName + OUTPUT_DATA_VALUE_SEPARATOR + OUTPUT_DATA_NAME_VALUE_SEPARATOR + OUTPUT_DATA_VALUE_SEPARATOR + ParamValue;

        Continue;
      end;
    end;
  end;

  // Build result
  ParsedData := '';

  if (R_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + R_Line + FOutputLineSeparator;
  end;

  if (L_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
  end;

  // Append UV index with GDT comment field id to measure data
  if (bolAddUVMeasureIndexToGDT) then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_COMMENT + FOutputLineSeparator + TempData;
  end;
  
  FParsedDataString:= RawByteString(ParsedData);  
end.
