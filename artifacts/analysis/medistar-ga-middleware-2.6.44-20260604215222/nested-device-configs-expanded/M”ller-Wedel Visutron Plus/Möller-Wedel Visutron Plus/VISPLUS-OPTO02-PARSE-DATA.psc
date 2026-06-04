const
  VERSION = '1.0.55.62';
  DATE = '23.01.2023 08:34:10';
  TEXT = 'Copyright (c) 2023 team2work GmbH';

  DATA_ACK                         = #$06;   // ACK
  DATA_NAK                         = #$15;   // NAK

  DATA_SEPARATOR_01                = #$0D#$0A;
  DATA_SEPARATOR_02                = #$3A;   // Colon Char (:)

  PHOROPTER_DATA_INDEX_RIGHT_EYE_1 = 'R';
  PHOROPTER_DATA_INDEX_LEFT_EYE_1  = 'L';

  PHOROPTER_DATA_INDEX_SPH_F       = 'SPH_F';
  PHOROPTER_DATA_INDEX_SPH_N       = 'SPH_N';
  PHOROPTER_DATA_INDEX_CYL         = 'CYL';
  PHOROPTER_DATA_INDEX_AXIS        = 'AXIS';
  PHOROPTER_DATA_INDEX_PRISM       = 'PRISM';
  PHOROPTER_DATA_INDEX_ACC         = 'ACC';

  PHOROPTER_DATA_INDEX_PD          = 'PD';
  PHOROPTER_DATA_INDEX_HSA         = 'HSA';
  PHOROPTER_DATA_INDEX_BLUR        = 'BLUR';

  RIGHT_EYE_START_MARKER           = 'R.:';
  LEFT_EYE_START_MARKER            = 'L.:';
	
  GDT_FID_MEASURE_DATA_01          = '6228';

  GDT_SIGN_SEPARATOR               = ' ';
  GDT_AXIS_SEPARATOR               = ' ';
  GDT_AXIS_SEPARATOR_MAX_COUNT     = 3;  
  
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
  
  if AddAxisSeparator then
    while Length(S) < GDT_AXIS_SEPARATOR_MAX_COUNT do
      S:= GDT_AXIS_SEPARATOR + S;
  
  Result:= Result + S;
end;    
  
var
  Data, Sep1, S1, ParsedData, PD_G, HSA, BLUR, VisusRightLeftEye: String;
  RightEye_F, LeftEye_F, RightEye_N, LeftEye_N, VIS_S_B, VIS_C_B: String;
  i, DataIndexRightEye, DataIndexLeftEye: Integer;
  R_SPH_F, R_SPH_N, R_CYL, R_AXIS, R_PRISM, R_ACC, R_VIS_S_R, R_VIS_C_R, R_PD_R: String;
  R_arr_SPH_F, R_arr_SPH_N, R_arr_CYL, R_arr_AXIS, R_arr_PRISM, R_arr_ACC: TStringArray;
  arrData1, arrPD, arrHSA, arrBLUR: TStringArray;
  L_SPH_F, L_SPH_N, L_CYL, L_AXIS, L_PRISM, L_ACC, L_VIS_S_L, L_VIS_C_L, L_PD_L: String;
  L_arr_SPH_F, L_arr_SPH_N, L_arr_CYL, L_arr_AXIS, L_arr_PRISM, L_arr_ACC: TStringArray;
  bolAddSign, bolAddSignSeparator, bolAddAxisSeparator, bolAddVDValueToOutput: Boolean;
  bolVisVP: Boolean;
begin
  // Verwende "True", damit der VD (HSA) Wert hinzugefügt wird, benutze "False",
  // damit der VD Wert nicht per GDT exportiert wird.
  bolAddVDValueToOutput:= True;

  // Verwende "True", damit immer ein Vorzeichen hinzugefügt wird,
  // benutze "False", damit nur der gemessene Wert eingetragen wird wie er 
  // vom Gerät kommt. 
  bolAddSign:= True;
  
  // Verwende "True", damit nach jedem Vorzeichen der Wert aus GDT_SIGN_SEPARATOR 
  // angefügt wird, benutze "False", damit kein Abstand zwischen Vorzeichen
  // und Wert eingefügt wird.
  bolAddSignSeparator:= False;
  
  // Verwende "True", damit der Achsenseparator angefügt wird,
  // benutze "False", damit der Achsenseparator nicht verwendet wird.
  bolAddAxisSeparator:= True;   
 
  // --- Don't edit script down below --- 	

  // Clear parsed data string
  FParsedDataString := '';
  
  if Length(FRawDataString) <= 0 then
  begin
    FLastErrorCode := -2;
    FLastErrorMessage := 'Keine Daten für die Verarbeitung der Daten gefunden';

    DoPSError();
 
    Exit;
  end;
  
  // T2WMessageBoxS(FRawDataString);
  
  // DoPSSaveFile('test1.txt');  

  Sep1 := DATA_SEPARATOR_01;
  
  Data := String(FRawDataString);
  Data := Trim(Data);

  // Get array from 1st separator
  arrData1 := Explode(Sep1, Data, 0);  
  
  if Length(arrData1) <= 0 then
  begin
    FLastErrorCode := -3;
    FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Keine Parameter gefunden';

    DoPSError();

    Exit;
  end;

  // Parse raw data
  bolVisVP:= False;

  RightEye_F:= '';
  LeftEye_F:= '';
  RightEye_N:= '';
  LeftEye_N:= '';  

  PD_G:= '';
  HSA:= '';
  BLUR:= '';
  VIS_S_B:= '';
  VIS_C_B:= '';
  VisusRightLeftEye:= '';

  R_SPH_F:= '';
  R_SPH_N:= '';
  R_CYL:= '';
  R_AXIS:= '';
  R_PRISM:= '';
  R_ACC:= '';
  R_VIS_S_R:= '';
  R_VIS_C_R:= '';
  R_PD_R:= '';

  L_SPH_F:= '';
  L_SPH_N:= '';
  L_CYL:= '';
  L_AXIS:= '';
  L_PRISM:= '';
  L_ACC:= '';
  L_VIS_S_L:= '';
  L_VIS_C_L:= '';
  L_PD_L:= '';

  DataIndexRightEye:= -1;
  DataIndexLeftEye:= -1;

  // loop through the data
  for i := 0 to Length(arrData1) - 1 do
  begin
    S1:= Trim(arrData1[i]);

    if (S1 = '') then
    begin
      Continue;
    end;

    // check which protocoll we have
    if (S1 = 'VIS') then
    begin
      // "VisVP" EMR driver data output
      bolVisVP:= True;

      Continue;
    end;

    // if we have the "VisVP" protocoll, use this peace of code...
    if (bolVisVP) then
    begin
      //
      if T2WStartsStr(PHOROPTER_DATA_INDEX_RIGHT_EYE_1, S1) then
      begin
        DataIndexRightEye:= i;
        DataIndexLeftEye:= -1;

        Continue;
      end;

      //
      if (DataIndexRightEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_SPH_F, S1))
      then
      begin
        R_SPH_F:= Trim(S1);
      
        R_arr_SPH_F:= Explode(DATA_SEPARATOR_02, R_SPH_F, 0);
      
        if Length(R_arr_SPH_F) < 2 then
        begin
          FLastErrorCode := -4;
          FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter SPH_F nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexRightEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_SPH_N, S1))
      then
      begin
        R_SPH_N:= Trim(S1);
      
        R_arr_SPH_N:= Explode(DATA_SEPARATOR_02, R_SPH_N, 0);
      
        if Length(R_arr_SPH_N) < 2 then
        begin
          FLastErrorCode := -4;
          FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter SPH_N nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexRightEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_CYL, S1))
      then
      begin
        R_CYL:= Trim(S1);
      
        R_arr_CYL:= Explode(DATA_SEPARATOR_02, R_CYL, 0);
      
        if Length(R_arr_CYL) < 2 then
        begin
          FLastErrorCode := -4;
          FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter CYL nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexRightEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_AXIS, S1))
      then
      begin
        R_AXIS:= Trim(S1);
      
        R_arr_AXIS:= Explode(DATA_SEPARATOR_02, R_AXIS, 0);
      
        if Length(R_arr_AXIS) < 2 then
        begin
          FLastErrorCode := -4;
          FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter AXIS nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexRightEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_PRISM, S1))
      then
      begin
        R_PRISM:= Trim(S1);
      
        R_arr_PRISM:= Explode(DATA_SEPARATOR_02, R_PRISM, 0);
      
        if Length(R_arr_PRISM) < 2 then
        begin
          FLastErrorCode := -4;
          FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter PRISM nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexRightEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_ACC, S1))
      then
      begin
        R_ACC:= Trim(S1);
      
        R_arr_ACC:= Explode(DATA_SEPARATOR_02, R_ACC, 0);
      
        if Length(R_arr_ACC) < 2 then
        begin
          FLastErrorCode := -4;
          FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter ACC nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // -----------------------------------------------------------------------

      //  
      if T2WStartsStr(PHOROPTER_DATA_INDEX_LEFT_EYE_1, S1) then
      begin
        DataIndexLeftEye:= i;
        DataIndexRightEye:= -1;
      
        Continue;
      end;    

      //
      if (DataIndexLeftEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_SPH_F, S1))
      then
      begin
        L_SPH_F:= Trim(S1);
      
        L_arr_SPH_F:= Explode(DATA_SEPARATOR_02, L_SPH_F, 0);
      
        if Length(L_arr_SPH_F) < 2 then
        begin
          FLastErrorCode := -4;
          FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter SPH_F nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexLeftEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_SPH_N, S1))
      then
      begin
        L_SPH_N:= Trim(S1);
      
        L_arr_SPH_N:= Explode(DATA_SEPARATOR_02, L_SPH_N, 0);
      
        if Length(L_arr_SPH_N) < 2 then
        begin
          FLastErrorCode := -4;
          FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter SPH_N nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexLeftEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_CYL, S1))
      then
      begin
        L_CYL:= Trim(S1);
      
        L_arr_CYL:= Explode(DATA_SEPARATOR_02, L_CYL, 0);
      
        if Length(L_arr_CYL) < 2 then
        begin
          FLastErrorCode := -4;
          FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter CYL nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexLeftEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_AXIS, S1))
      then
      begin
        L_AXIS:= Trim(S1);
      
        L_arr_AXIS:= Explode(DATA_SEPARATOR_02, L_AXIS, 0);
      
        if Length(L_arr_AXIS) < 2 then
        begin
          FLastErrorCode := -4;
          FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter AXIS nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexLeftEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_PRISM, S1))
      then
      begin
        L_PRISM:= Trim(S1);
      
        L_arr_PRISM:= Explode(DATA_SEPARATOR_02, L_PRISM, 0);

        if Length(L_arr_PRISM) < 2 then
        begin
          FLastErrorCode := -4;
          FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter PRISM nicht korrekt';

          DoPSError();

          Exit;
        end;

        Continue;
      end;

      //
      if (DataIndexLeftEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_ACC, S1))
      then
      begin
        L_ACC:= Trim(S1);
      
        L_arr_ACC:= Explode(DATA_SEPARATOR_02, L_ACC, 0);
      
        if Length(L_arr_ACC) < 2 then
        begin
          FLastErrorCode := -4;
          FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter ACC nicht korrekt';

          DoPSError();

          Exit;
        end;

        Continue;
      end;

      // -----------------------------------------------------------------------

      //
      if T2WStartsStr(PHOROPTER_DATA_INDEX_PD, S1) then
      begin
        PD_G:= Trim(S1);

        arrPD:= Explode(DATA_SEPARATOR_02, PD_G, 0);

        if Length(arrPD) < 2 then
        begin
          FLastErrorCode := -4;
          FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter PD nicht korrekt';

          DoPSError();

          Exit;
        end;

        Continue;
      end;

      // 
      if T2WStartsStr(PHOROPTER_DATA_INDEX_HSA, S1) then
      begin
        HSA:= Trim(S1);

        arrHSA:= Explode(DATA_SEPARATOR_02, HSA, 0);

        if Length(arrHSA) < 2 then
        begin
          FLastErrorCode := -4;
          FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter HSA nicht korrekt';

          DoPSError();

          Exit;
        end;

        Continue;
      end;

      //
      if T2WStartsStr(PHOROPTER_DATA_INDEX_BLUR, S1) then
      begin
        BLUR:= Trim(S1);

        arrBLUR:= Explode(DATA_SEPARATOR_02, BLUR, 0);

        if Length(arrBLUR) < 2 then
        begin
          FLastErrorCode := -4;
          FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter BLUR nicht korrekt';

          DoPSError();

          Exit;
        end;

        Continue;
      end;
    end;
  end;
  
  // Start parsing ophthalmology data down here...

  // Reset parsed data
  ParsedData:= '';

  // ---------------------------------------------------------------------------

  // if we have the "VisVP" protocoll, use this peace of code...
  if (bolVisVP) then
  begin
    // Build right eye output
    RightEye_F:= RIGHT_EYE_START_MARKER;
    RightEye_N:= RIGHT_EYE_START_MARKER;

    //
    if (R_SPH_F <> '') then
    begin
      RightEye_F:= '' + '' + RightEye_F + FormatSignValue('S=', Trim(R_arr_SPH_F[1]), bolAddSign, bolAddSignSeparator);
    end;

    //
    if (R_SPH_N <> '') then
    begin
      RightEye_N:= 'N' + ' ' + RightEye_N + FormatSignValue('S=', Trim(R_arr_SPH_N[1]), bolAddSign, bolAddSignSeparator);
    end;

    //
    if (R_CYL <> '') then
    begin
      if RightEye_F <> RIGHT_EYE_START_MARKER then
        RightEye_F:= RightEye_F + ' ';

      RightEye_F:= RightEye_F + FormatSignValue('Z=', Trim(R_arr_CYL[1]), bolAddSign, bolAddSignSeparator);
    end;

    //
    if (R_AXIS <> '') then
    begin
      RightEye_F:= RightEye_F + FormatAxisValue('*', Trim(R_arr_AXIS[1]), bolAddAxisSeparator);
    end;

    //
    if (R_PRISM <> '') then
    begin
      if RightEye_F <> RIGHT_EYE_START_MARKER then
        RightEye_F:= RightEye_F + ' ';

      RightEye_F:= RightEye_F + 'P= ';
    end;

    if (R_PRISM <> '') then
    begin
      S1:= Trim(R_arr_PRISM[1]);
      S1:= T2WStringReplace(S1, 'IN', 'I', True, False);
      S1:= T2WStringReplace(S1, 'OUT', 'O', True, False);

      RightEye_F:= RightEye_F + S1;
    end;

    // 
    if (R_ACC <> '') then
    begin
      //
    end;

    // -------------------------------------------------------------------------

    // Build left eye output
    LeftEye_F:= LEFT_EYE_START_MARKER;
    LeftEye_N:= LEFT_EYE_START_MARKER;

    //
    if (L_SPH_F <> '') then
    begin
      LeftEye_F:= '' + '' + LeftEye_F + FormatSignValue('S=', Trim(L_arr_SPH_F[1]), bolAddSign, bolAddSignSeparator);
    end;

    //
    if (L_SPH_N <> '') then
    begin
      LeftEye_N:= 'N' + ' ' + LeftEye_N + FormatSignValue('S=', Trim(L_arr_SPH_N[1]), bolAddSign, bolAddSignSeparator);
    end;

    //
    if (L_CYL <> '') then
    begin
      if LeftEye_F <> LEFT_EYE_START_MARKER then
        LeftEye_F:= LeftEye_F + ' ';

      LeftEye_F:= LeftEye_F + FormatSignValue('Z=', Trim(L_arr_CYL[1]), bolAddSign, bolAddSignSeparator);
    end;

    //
    if (L_AXIS <> '') then
    begin
      LeftEye_F:= LeftEye_F + FormatAxisValue('*', Trim(L_arr_AXIS[1]), bolAddAxisSeparator);
    end;

    //
    if (L_PRISM <> '') then
    begin
      if LeftEye_F <> LEFT_EYE_START_MARKER then
        LeftEye_F:= LeftEye_F + ' ';

      LeftEye_F:= LeftEye_F + 'P= ';
    end;

    if (L_PRISM <> '') then
    begin
      S1:= Trim(L_arr_PRISM[1]);
      S1:= T2WStringReplace(S1, 'UP', 'U', True, False);
      S1:= T2WStringReplace(S1, 'DOWN', 'D', True, False);

      LeftEye_F:= LeftEye_F + S1;
    end;

    // 
    if (L_ACC <> '') then
    begin
      //
    end;

    // -------------------------------------------------------------------------

    //
    if (PD_G <> '') then
    begin
      if RightEye_F <> RIGHT_EYE_START_MARKER then
        RightEye_F:= RightEye_F + ' ';

      RightEye_F:= RightEye_F + 'PD= ' + Trim(arrPD[1]);
    end;

    //
    if (PD_G <> '') then
    begin
      if LeftEye_F <> LEFT_EYE_START_MARKER then
        LeftEye_F:= LeftEye_F + ' ';

      LeftEye_F:= LeftEye_F + 'PD= ' + Trim(arrPD[1]);
    end;

    // 
    if (HSA <> '') and (bolAddVDValueToOutput) then
    begin
      if RightEye_F <> RIGHT_EYE_START_MARKER then
        RightEye_F:= RightEye_F + ' ';

      RightEye_F:= RightEye_F + 'VD= ' + Trim(arrHSA[1]);
    end;

    //
    if (HSA <> '') and (bolAddVDValueToOutput) then
    begin
      if LeftEye_F <> LEFT_EYE_START_MARKER then
        LeftEye_F:= LeftEye_F + ' ';

      LeftEye_F:= LeftEye_F + 'VD= ' + Trim(arrHSA[1]);
    end;

    //
    if (BLUR <> '') then
    begin
      //
    end;

    // -------------------------------------------------------------------------

    // Add right eye (F) to final output
    if (RightEye_F <> '') then    
      ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA_01 + FOutputLineSeparator + RightEye_F + FOutputLineSeparator;

    // Add left eye (F) to final output
    if (LeftEye_F <> '') then    
      ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA_01 + FOutputLineSeparator + LeftEye_F;

    // 
    if (R_SPH_N <> '') and (RightEye_N <> '') then
    begin
      ParsedData := ParsedData + FOutputLineSeparator;
      ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA_01 + FOutputLineSeparator + RightEye_N;
    end;

    //
    if (L_SPH_N <> '') and (LeftEye_N <> '') then
    begin
      ParsedData := ParsedData + FOutputLineSeparator;
      ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA_01 + FOutputLineSeparator + LeftEye_N;
    end;
  end;

  // Set result
  FParsedDataString := RawByteString(Trim(ParsedData));
  
  
  

  
  
end.