const
  VERSION = '1.0.32.65';
  DATE = '05.11.2025 07:45:11';
  TEXT = 'Copyright (c) 2025 CompuGroup Medical Deutschland AG';

  DATA_SEPARATOR_01                 = #$0D#$0A;
  DATA_SEPARATOR_02                 = ' ';
  
  PHOROPTER_DATA                    = '#RT';
  PHOROPTER_DATA_INDEX_VB           = '#VB';
  PHOROPTER_DATA_INDEX_WB           = '#WB';
  PHOROPTER_DATA_INDEX_RIGHT_EYE    = '# R';
  PHOROPTER_DATA_INDEX_LEFT_EYE     = '# L';
  PHOROPTER_DATA_INDEX_R_ADD        = '#AR';
  PHOROPTER_DATA_INDEX_L_ADD        = '#AL';
  PHOROPTER_DATA_INDEX_R_PRISM_FAR  = '#PR';
  PHOROPTER_DATA_INDEX_R_PRISM_NEAR = '#Pr';
  PHOROPTER_DATA_INDEX_L_PRISM_FAR  = '#PL';
  PHOROPTER_DATA_INDEX_L_PRISM_NEAR = '#Pl';
  PHOROPTER_DATA_INDEX_PD_FAR       = '#pB';
  PHOROPTER_DATA_INDEX_PD_NEAR      = '#pb';

  PHOROPTER_DATA_INDEX_R_V_FAR      = '#vR';
  PHOROPTER_DATA_INDEX_L_V_FAR      = '#vL';
  PHOROPTER_DATA_INDEX_R_V_NEAR     = '#vr';
  PHOROPTER_DATA_INDEX_L_V_NEAR     = '#vl';

  PHOROPTER_DATA_INDEX_VB_FAR       = '#vB';
  PHOROPTER_DATA_INDEX_VB_NEAR      = '#vb';

  PHOROPTER_DATA_INDEX_CV_FAR       = '#CV';
  PHOROPTER_DATA_INDEX_CV_NEAR      = '#Cv';

  PHOROPTER_DATA_INDEX_DV_FAR       = '#DV';
  PHOROPTER_DATA_INDEX_DV_NEAR      = '#Dv';

  PHOROPTER_DATA_INDEX_V_DV_FAR     = '#dV';
  PHOROPTER_DATA_INDEX_V_DV_NEAR    = '#dv';

  PHOROPTER_DATA_INDEX_UV_FAR       = '#uV';
  PHOROPTER_DATA_INDEX_UV_NEAR      = '#uv';

  PHOROPTER_DATA_INDEX_WF           = '#WF';
  PHOROPTER_DATA_INDEX_AA           = '#aA';
  PHOROPTER_DATA_INDEX_HA           = '#HA';
  PHOROPTER_DATA_INDEX_SS           = '#SS';

  PHOROPTER_DATA_NEW_FORMAT         = '<PRESCRIPTION DATA>';

  LINE_IDENTIFIER_R                 = '<R>';
  LINE_IDENTIFIER_L                 = '<L>';
  
  DATA_INDEX_S                      = 'S';
  DATA_INDEX_C                      = 'C';
  DATA_INDEX_A                      = 'A';
  DATA_INDEX_P                      = 'P';
  DATA_INDEX_ADD                    = 'ADD';  

  DATA_INDEX_VD                     = 'VD = ';
  DATA_INDEX_WD                     = 'WD = ';

  RIGHT_EYE_START_MARKER            = 'R.:';
  LEFT_EYE_START_MARKER             = 'L.:';

  GDT_FID_MEASURE_DATA_01           = '6228';
  GDT_FID_COMMENT_01                = '6227';

  GDT_SIGN_SEPARATOR                = ' ';
  GDT_AXIS_SEPARATOR                = ' ';
  GDT_AXIS_SEPARATOR_MAX_COUNT      = 3;  
  
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
  
  Result:= Result + S2 + ' ' + Trim(S1);
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

function CheckZeroValue(const Value: String): String;
var
  S: String;
begin
  Result:= Value;

  S:= Value;

  if (T2WStartsStr('0.00', S)) then
  begin
    Result:= Trim(Copy(S, 1, 4));
  end;
end;    
  
var
  Data, Sep1, S1, ParsedData: String;
  arrData1, arr_VD, arr_WB, arr_R_PRISM_F, arr_R_PRISM_N, arr_L_PRISM_F, arr_L_PRISM_N, arr_eye_data: TStringArray;
  bolRtDataFound, bolNewFormatDataFound, bolRightEyeDataFound, bolLeftEyeDataFound: Boolean;
  PD_F, PD_N, VD, WD, R, L: String;
  R_SPH, R_CYL, R_AXIS, R_ADD, R_PRISM_F, R_PRISM_N: String;
  L_SPH, L_CYL, L_AXIS, L_ADD, L_PRISM_F, L_PRISM_N: String;
  bolAddSign, bolAddSignSeparator, bolAddAxisSeparator, bolAddVDValueToOutput, bolAddWDValueToOutput: Boolean;
  i: Integer;
  RightEye, LeftEye: String;
begin
  // Verwende "True", damit der WD Wert hinzugefügt wird, benutze "False", damit der
  // WD Wert nicht per GDT exportiert wird.
  bolAddWDValueToOutput:= False;

  // Verwende "True", damit der VD Wert hinzugefügt wird, benutze "False",
  // damit der VD Wert nicht per GDT exportiert wird.
  bolAddVDValueToOutput:= False;

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
  bolRtDataFound:= False;
  bolNewFormatDataFound:= False;
  
  bolRightEyeDataFound:= False;
  bolLeftEyeDataFound:= False;

  PD_F:= '';
  PD_N:= '';
  VD:= '';
  WD:= '';
  R:= '';
  L:= '';
  
  R_SPH:= '';
  R_CYL:= '';
  R_AXIS:= '';
  R_ADD:= '';
  R_PRISM_F:= '';
  R_PRISM_N:= '';

  L_SPH:= '';
  L_CYL:= '';
  L_AXIS:= '';
  L_ADD:= '';
  L_PRISM_F:= '';
  L_PRISM_N:= '';

  RightEye:= '';
  LeftEye:= '';
  
  SetLength(arr_VD, 0);
  SetLength(arr_WB, 0);
  SetLength(arr_R_PRISM_F, 0);
  SetLength(arr_R_PRISM_N, 0);
  SetLength(arr_L_PRISM_F, 0);
  SetLength(arr_L_PRISM_N, 0);

  SetLength(arr_eye_data, 0);

  // loop through the data
  for i := 0 to Length(arrData1) - 1 do
  begin
    S1:= Trim(arrData1[i]);

    if (S1 = '') then
    begin
      Continue;
    end;
    
    if (T2WStartsStr(DATA_INDEX_VD, S1)) then   
    begin
      VD:= Trim(S1);
      VD:= Trim(T2WStringReplace(VD, DATA_INDEX_VD, '', True, False));    
    end;
    
    if (T2WStartsStr(DATA_INDEX_WD, S1)) then   
    begin   
      WD:= Trim(S1);
      WD:= Trim(T2WStringReplace(WD, DATA_INDEX_WD, '', True, False));
      WD:= Trim(T2WStringReplace(WD, 'cm', '', True, False));
    end;

    // check if we have rt data
    if (S1 = PHOROPTER_DATA) then
    begin
      bolRtDataFound:= True;
      bolNewFormatDataFound:= False;
    end;
    
    // check if we have data contains the new format
    if (S1 = PHOROPTER_DATA_NEW_FORMAT) 
    or (T2WContainsStr(S1, PHOROPTER_DATA_NEW_FORMAT)) then
    begin
      if (not bolRtDataFound) then
      begin
        bolNewFormatDataFound:= True;
      end;
    end;
    
    // parse data, if we found the new format
    if (bolNewFormatDataFound) then
    begin
      // check which eyes contains the measurement
      if (T2WContainsStr(S1, LINE_IDENTIFIER_R)) then
      begin
        bolRightEyeDataFound:= True;
      end;
      
      if (T2WContainsStr(S1, LINE_IDENTIFIER_L)) then
      begin
        bolLeftEyeDataFound:= True;
      end;
      
      if (T2WContainsStr(S1, DATA_INDEX_S)) then
      begin
        SetLength(arr_eye_data, 0);
        
        arr_eye_data:= Explode(DATA_INDEX_S, S1, 0);
        
        R_SPH:= '';
        L_SPH:= '';
        
        if (Length(arr_eye_data) = 2) then
        begin
          R_SPH:= Trim(arr_eye_data[0]);
          L_SPH:= Trim(arr_eye_data[1]);
        end
        else
        begin
          if (Length(arr_eye_data) > 0) then
          begin
            if (bolRightEyeDataFound) then
            begin
              R_SPH:= Trim(arr_eye_data[0]);
            end;
            
            if (bolLeftEyeDataFound) then
            begin
              L_SPH:= Trim(arr_eye_data[0]);
            end;
          end;
        end;
      end;
      
      if (T2WContainsStr(S1, DATA_INDEX_C)) then
      begin
        SetLength(arr_eye_data, 0);
        
        arr_eye_data:= Explode(DATA_INDEX_C, S1, 0);
        
        R_CYL:= '';
        L_CYL:= '';
        
        if (Length(arr_eye_data) = 2) then
        begin
          R_CYL:= Trim(arr_eye_data[0]);
          L_CYL:= Trim(arr_eye_data[1]);
        end
        else
        begin
          if (Length(arr_eye_data) > 0) then
          begin
            if (bolRightEyeDataFound) then
            begin
              R_CYL:= Trim(arr_eye_data[0]);
            end;
            
            if (bolLeftEyeDataFound) then
            begin
              L_CYL:= Trim(arr_eye_data[0]);
            end;
          end;
        end;
      end;
      
      if (T2WContainsStr(S1, DATA_INDEX_A)) then
      begin
        SetLength(arr_eye_data, 0);
        
        arr_eye_data:= Explode(DATA_INDEX_A, S1, 0);
        
        R_AXIS:= '';
        L_AXIS:= '';
        
        if (Length(arr_eye_data) = 2) then
        begin
          R_AXIS:= Trim(arr_eye_data[0]);
          L_AXIS:= Trim(arr_eye_data[1]);
        end
        else
        begin
          if (Length(arr_eye_data) > 0) then
          begin
            if (bolRightEyeDataFound) then
            begin
              R_AXIS:= Trim(arr_eye_data[0]);
            end;
            
            if (bolLeftEyeDataFound) then
            begin
              L_AXIS:= Trim(arr_eye_data[0]);
            end;
          end;
        end;
      end; 

      if (T2WContainsStr(S1, DATA_INDEX_ADD)) then
      begin
        SetLength(arr_eye_data, 0);
        
        arr_eye_data:= Explode(DATA_INDEX_ADD, S1, 0);
        
        R_ADD:= '';
        L_ADD:= '';
        
        if (Length(arr_eye_data) = 2) then
        begin
          R_ADD:= Trim(arr_eye_data[0]);
          L_ADD:= Trim(arr_eye_data[1]);
        end
        else
        begin
          if (Length(arr_eye_data) > 0) then
          begin
            if (bolRightEyeDataFound) then
            begin
              R_ADD:= Trim(arr_eye_data[0]);
            end;
            
            if (bolLeftEyeDataFound) then
            begin
              L_ADD:= Trim(arr_eye_data[0]);
            end;
          end;
        end;
      end; 

      // TODO: 

    end;

    if (bolRtDataFound) then
    begin
      // 
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_VB, S1)) then
      begin
        VD:= Trim(S1);

        arr_VD:= Explode(DATA_SEPARATOR_02, VD, 0);

        if Length(arr_VD) < 2 then
        begin
          FLastErrorCode := -4;
          FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter VB nicht korrekt';

          DoPSError();

          Exit;
        end;

        Continue;
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_WB, S1)) then
      begin
        WD:= Trim(S1);

        arr_WB:= Explode(DATA_SEPARATOR_02, WD, 0);

        if Length(arr_WB) < 2 then
        begin
          FLastErrorCode := -4;
          FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter WB nicht korrekt';

          DoPSError();

          Exit;
        end;

        Continue;
      end;

      // 
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_RIGHT_EYE, S1)) then
      begin
        R:= Trim(S1);
        R:= Trim(T2WStringReplace(R, PHOROPTER_DATA_INDEX_RIGHT_EYE, '', True, False));
        
        R_SPH:= CheckZeroValue(Trim(Copy(R, 1, 6)));
        R_CYL:= Trim(Copy(R, 8, 6));
        R_AXIS:= Trim(Copy(R, 15, 3));

        Continue;
      end;

      // 
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_LEFT_EYE, S1)) then
      begin
        L:= Trim(S1);
        L:= Trim(T2WStringReplace(L, PHOROPTER_DATA_INDEX_LEFT_EYE, '', True, False));
        
        L_SPH:= CheckZeroValue(Trim(Copy(L, 1, 6)));
        L_CYL:= Trim(Copy(L, 8, 6));
        L_AXIS:= Trim(Copy(L, 15, 3));

        Continue;        
      end;

      // 
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_R_ADD, S1)) then
      begin
        R_ADD:= Trim(S1);
        R_ADD:= Trim(T2WStringReplace(R_ADD, PHOROPTER_DATA_INDEX_R_ADD, '', True, False));
        R_ADD:= Trim(Copy(R_ADD, 1, 6));

        Continue;
      end;

      // 
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_L_ADD, S1)) then
      begin
        L_ADD:= Trim(S1);
        L_ADD:= Trim(T2WStringReplace(L_ADD, PHOROPTER_DATA_INDEX_L_ADD, '', True, False));
        L_ADD:= Trim(Copy(L_ADD, 1, 6));

        Continue;
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_R_PRISM_FAR, S1)) then
      begin
        R_PRISM_F:= Trim(S1);
        R_PRISM_F:= Trim(T2WStringReplace(R_PRISM_F, PHOROPTER_DATA_INDEX_R_PRISM_FAR, '', True, False));

        arr_R_PRISM_F:= Explode(DATA_SEPARATOR_02, R_PRISM_F, 0);

        if Length(arr_R_PRISM_F) < 2 then
        begin
          FLastErrorCode := -4;
          FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter PR nicht korrekt';

          DoPSError();

          Exit;
        end;

        R_PRISM_F:= '';

        if Length(arr_R_PRISM_F) = 2 then
        begin
          R_PRISM_F:= arr_R_PRISM_F[1] + ' ' + arr_R_PRISM_F[0];
        end;

        if Length(arr_R_PRISM_F) = 4 then
        begin
          R_PRISM_F:= arr_R_PRISM_F[1] + ' ' + arr_R_PRISM_F[0] + ' ' + arr_R_PRISM_F[3] + ' ' + arr_R_PRISM_F[2];
        end;

        Continue;
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_L_PRISM_FAR, S1)) then
      begin
        L_PRISM_F:= Trim(S1);
        L_PRISM_F:= Trim(T2WStringReplace(L_PRISM_F, PHOROPTER_DATA_INDEX_L_PRISM_FAR, '', True, False));

        arr_L_PRISM_F:= Explode(DATA_SEPARATOR_02, L_PRISM_F, 0);

        if Length(arr_L_PRISM_F) < 2 then
        begin
          FLastErrorCode := -4;
          FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter LR nicht korrekt';

          DoPSError();

          Exit;
        end;

        L_PRISM_F:= '';

        if Length(arr_L_PRISM_F) = 2 then
        begin
          L_PRISM_F:= arr_L_PRISM_F[1] + ' ' + arr_L_PRISM_F[0];
        end;

        if Length(arr_L_PRISM_F) = 4 then
        begin
          L_PRISM_F:= arr_L_PRISM_F[1] + ' ' + arr_L_PRISM_F[0] + ' ' + arr_L_PRISM_F[3] + ' ' + arr_L_PRISM_F[2];
        end;

        Continue;
      end;

      // 
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_R_PRISM_NEAR, S1)) then
      begin
        R_PRISM_N:= Trim(S1);
        R_PRISM_N:= Trim(T2WStringReplace(R_PRISM_N, PHOROPTER_DATA_INDEX_R_PRISM_NEAR, '', True, False));

        arr_R_PRISM_N:= Explode(DATA_SEPARATOR_02, R_PRISM_N, 0);

        if Length(arr_R_PRISM_N) < 2 then
        begin
          FLastErrorCode := -4;
          FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter Pr nicht korrekt';

          DoPSError();

          Exit;
        end;

        R_PRISM_N:= '';

        if Length(arr_R_PRISM_N) = 2 then
        begin
          R_PRISM_N:= arr_R_PRISM_N[1] + ' ' + arr_R_PRISM_N[0];
        end;

        if Length(arr_R_PRISM_N) = 4 then
        begin
          R_PRISM_N:= arr_R_PRISM_N[1] + ' ' + arr_R_PRISM_N[0] + ' ' + arr_R_PRISM_N[3] + ' ' + arr_R_PRISM_N[2];
        end;

        Continue;
      end;

      // 
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_L_PRISM_NEAR, S1)) then
      begin
        L_PRISM_N:= Trim(S1);
        L_PRISM_N:= Trim(T2WStringReplace(L_PRISM_N, PHOROPTER_DATA_INDEX_L_PRISM_NEAR, '', True, False));

        arr_L_PRISM_N:= Explode(DATA_SEPARATOR_02, L_PRISM_N, 0);

        if Length(arr_L_PRISM_N) < 2 then
        begin
          FLastErrorCode := -4;
          FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter Pl nicht korrekt';

          DoPSError();

          Exit;
        end;

        L_PRISM_N:= '';

        if Length(arr_L_PRISM_N) = 2 then
        begin
          L_PRISM_N:= arr_L_PRISM_N[1] + ' ' + arr_L_PRISM_N[0];
        end;

        if Length(arr_L_PRISM_N) = 4 then
        begin
          L_PRISM_N:= arr_L_PRISM_N[1] + ' ' + arr_L_PRISM_N[0] + ' ' + arr_L_PRISM_N[3] + ' ' + arr_L_PRISM_N[2];
        end;

        Continue;
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_PD_FAR, S1)) then
      begin
        PD_F:= Trim(S1);
        PD_F:= Trim(T2WStringReplace(PD_F, PHOROPTER_DATA_INDEX_PD_FAR, '', True, False));

        Continue;
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_PD_NEAR, S1)) then
      begin
        PD_N:= Trim(S1);
        PD_N:= Trim(T2WStringReplace(PD_N, PHOROPTER_DATA_INDEX_PD_NEAR, '', True, False));

        Continue;
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_R_V_FAR, S1)) then
      begin
        //
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_L_V_FAR, S1)) then
      begin
        //
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_R_V_NEAR, S1)) then
      begin
        //
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_L_V_NEAR, S1)) then
      begin
        //
      end;

      // 
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_VB_FAR, S1)) then
      begin
        //
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_VB_NEAR, S1)) then
      begin
        //
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_CV_FAR, S1)) then
      begin
        //
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_CV_NEAR, S1)) then
      begin
        //
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_DV_FAR, S1)) then
      begin
        //
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_DV_NEAR, S1)) then
      begin
        //
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_V_DV_FAR, S1)) then
      begin
        //
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_V_DV_NEAR, S1)) then
      begin
        //
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_UV_FAR, S1)) then
      begin
        //
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_UV_NEAR, S1)) then
      begin
        //
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_WF, S1)) then
      begin
        //
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_AA, S1)) then
      begin
        //
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_HA, S1)) then
      begin
        //
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_SS, S1)) then
      begin
        //
      end;
    end;
  end;
  
  // Start parsing ophthalmology data down here

  // Reset parsed data
  ParsedData:= '';

  // Build right eye output
  RightEye:= RIGHT_EYE_START_MARKER;

  if (R_SPH <> '') then
  begin
    RightEye:= '' + '' + RightEye + FormatSignValue('S=', Trim(R_SPH), bolAddSign, bolAddSignSeparator);
  end;

  if (R_CYL <> '') then
  begin
    if (RightEye <> RIGHT_EYE_START_MARKER) then
    begin
      RightEye:= RightEye + ' ';
    end;

    RightEye:= RightEye + FormatSignValue('Z=', Trim(R_CYL), bolAddSign, bolAddSignSeparator);
  end;

  if (R_AXIS <> '') then
  begin
    RightEye:= RightEye + FormatAxisValue('*', Trim(R_AXIS), bolAddAxisSeparator);
  end;

  if (R_PRISM_F <> '') then
  begin
    if (RightEye <> RIGHT_EYE_START_MARKER) then
    begin
      RightEye:= RightEye + ' ';
    end;

    RightEye:= RightEye + 'P= ' + R_PRISM_F;
  end;  

  if (R_PRISM_N <> '') then
  begin
    // TODO:
  end;

  if (R_ADD<> '') then
  begin
    if (RightEye <> RIGHT_EYE_START_MARKER) then
    begin
      RightEye:= RightEye + ' ';
    end;

    RightEye:= RightEye + FormatSignValue('A=', Trim(R_ADD), bolAddSign, bolAddSignSeparator);
  end;

  if (PD_F <> '') then
   begin
    if (RightEye <> RIGHT_EYE_START_MARKER) then
    begin
      RightEye:= RightEye + ' ';
    end;

    RightEye:= RightEye + 'PD= ' + PD_F;
  end;

  if (PD_N <> '') then
  begin
    // TODO:
  end;

  if (VD <> '') 
  and (Length(arr_VD) > 0) 
  and (bolAddVDValueToOutput) then
   begin
    if (RightEye <> RIGHT_EYE_START_MARKER) then
    begin
      RightEye:= RightEye + ' ';
    end;

    RightEye:= RightEye + 'VD= ' + Trim(arr_VD[1]);
  end;
  
  if (VD <> '') 
  and (Length(arr_VD) = 0) 
  and (bolAddVDValueToOutput) then
   begin
    if (RightEye <> RIGHT_EYE_START_MARKER) then
    begin
      RightEye:= RightEye + ' ';
    end;

    RightEye:= RightEye + 'VD= ' + Trim(arr_VD[1]);
  end;  

  // ---------------------------------------------------------------------------

  // Build left eye output
  LeftEye:= LEFT_EYE_START_MARKER;

  if (L_SPH <> '') then
  begin
    LeftEye:= '' + '' + LeftEye + FormatSignValue('S=', Trim(L_SPH), bolAddSign, bolAddSignSeparator);
  end;

  if (L_CYL <> '') then
  begin
    if LeftEye <> LEFT_EYE_START_MARKER then
      LeftEye:= LeftEye + ' ';

    LeftEye:= LeftEye + FormatSignValue('Z=', Trim(L_CYL), bolAddSign, bolAddSignSeparator);
  end;

  if (L_AXIS <> '') then
  begin
    LeftEye:= LeftEye + FormatAxisValue('*', Trim(L_AXIS), bolAddAxisSeparator);
  end;

  if (L_PRISM_F <> '') then
  begin
    if LeftEye <> LEFT_EYE_START_MARKER then
      LeftEye:= LeftEye + ' ';

    LeftEye:= LeftEye + 'P= ' + L_PRISM_F;
  end;

  if (L_PRISM_N <> '') then
  begin
    // TODO:
  end;

  if (L_ADD<> '') then
  begin
    if LeftEye <> LEFT_EYE_START_MARKER then
      LeftEye:= LeftEye + ' ';

    LeftEye:= LeftEye + FormatSignValue('A=', Trim(L_ADD), bolAddSign, bolAddSignSeparator);
  end;

  if (PD_F <> '') then
   begin
    if LeftEye <> LEFT_EYE_START_MARKER then
      LeftEye:= LeftEye + ' ';

    LeftEye:= LeftEye + 'PD= ' + PD_F;
  end;

  if (PD_N <> '') then
  begin
    // TODO:
  end;
  
  if (VD <> '') 
  and (Length(arr_VD) > 0) 
  and (bolAddVDValueToOutput) then
   begin
    if (LeftEye <> LEFT_EYE_START_MARKER) then
    begin
      LeftEye:= LeftEye + ' ';
    end;

    LeftEye:= LeftEye + 'VD= ' + Trim(arr_VD[1]);
  end;

  // ---------------------------------------------------------------------------
  
  // Add right eye to final output
  if (RightEye <> '') then
  begin
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA_01 + FOutputLineSeparator + RightEye + FOutputLineSeparator;
  end;

  // Add left eye to final output
  if (LeftEye <> '') then
  begin
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA_01 + FOutputLineSeparator + LeftEye;
  end;

  // Add WD if it exists
  if (WD <> '') 
  and (Length(arr_WB) > 0) 
  and (bolAddWDValueToOutput) then
  begin
    // Append WD index with GDT comment field id to measure data
    ParsedData := ParsedData + FOutputLineSeparator + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_COMMENT_01 + FOutputLineSeparator + 'WD= ' + Trim(arr_WB[1]);
  end;
  
  if (WD <> '') 
  and (Length(arr_WB) = 0) 
  and (bolAddWDValueToOutput) then
  begin
    // Append WD index with GDT comment field id to measure data
    ParsedData := ParsedData + FOutputLineSeparator + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_COMMENT_01 + FOutputLineSeparator + 'WD= ' + Trim(WD);
  end;  

  // Set result
  FParsedDataString := RawByteString(Trim(ParsedData));
end.