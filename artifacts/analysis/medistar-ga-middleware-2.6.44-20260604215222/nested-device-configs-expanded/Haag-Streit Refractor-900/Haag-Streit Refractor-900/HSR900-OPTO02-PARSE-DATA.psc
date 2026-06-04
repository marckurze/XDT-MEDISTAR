const
  VERSION = '1.0.55.94';
  DATE = '18.07.2025 10:38:13';
  TEXT = 'Copyright (c) 2025 CompuGroup Medical Deutschland AG';

  DATA_SEPARATOR_01                = #$0D#$0A;
  DATA_SEPARATOR_02                = #$3A;     // Colon Char (:)

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

  PHOROPTER_DATA_INDEX_PD_G        = 'PD_G';
  PHOROPTER_DATA_INDEX_VIS_S_B     = 'VIS_S_B';
  PHOROPTER_DATA_INDEX_VIS_C_B     = 'VIS_C_B';

  PHOROPTER_DATA_INDEX_RIGHT_EYE_2 = 'RIGHT';
  PHOROPTER_DATA_INDEX_LEFT_EYE_2  = 'LEFT';

  PHOROPTER_DATA_INDEX_SPH_F_R     = 'SPH_F_R';
  PHOROPTER_DATA_INDEX_SPH_N_R     = 'SPH_N_R';
  PHOROPTER_DATA_INDEX_CYL_R       = 'CYL_R';
  PHOROPTER_DATA_INDEX_AXIS_R      = 'AXIS_R';
  PHOROPTER_DATA_INDEX_PRISM_R     = 'PRISM_R';
  PHOROPTER_DATA_INDEX_ACC_R       = 'ACC_R';
  PHOROPTER_DATA_INDEX_VIS_S_R     = 'VIS_S_R';
  PHOROPTER_DATA_INDEX_VIS_C_R     = 'VIS_C_R';
  PHOROPTER_DATA_INDEX_PD_R        = 'PD_R';

  PHOROPTER_DATA_INDEX_SPH_F_L     = 'SPH_F_L';
  PHOROPTER_DATA_INDEX_SPH_N_L     = 'SPH_N_L';
  PHOROPTER_DATA_INDEX_CYL_L       = 'CYL_L';
  PHOROPTER_DATA_INDEX_AXIS_L      = 'AXIS_L';
  PHOROPTER_DATA_INDEX_PRISM_L     = 'PRISM_L';
  PHOROPTER_DATA_INDEX_ACC_L       = 'ACC_L';
  PHOROPTER_DATA_INDEX_VIS_S_L     = 'VIS_S_L';
  PHOROPTER_DATA_INDEX_VIS_C_L     = 'VIS_C_L';
  PHOROPTER_DATA_INDEX_PD_L        = 'PD_L';

  RIGHT_EYE_START_MARKER           = 'R.:';
  LEFT_EYE_START_MARKER            = 'L.:';
  
  GDT_FID_MEASURE_DATA_01          = '6228';
  GDT_FID_COMMENT_01               = '6227';
  GDT_FID_FOREIGN_RESULT_01        = '6221';
  GDT_FID_RESULT_01                = '6220';

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
      
  if (AddSignSeparator) then
    if not T2WStartsStr(GDT_SIGN_SEPARATOR, S1) then
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

function CalculateAdditionValue(const SPH_F, SPH_N: String): String;
var
  strSPH_F, strSPH_N, strResult: String;
  a1, a2, Res: Extended; 
  bolRes: Boolean;
begin
  Result:= '';
  
  if (SPH_F = '') or (SPH_N = '') then
    Exit;
 
  strResult:= ''; 
 
  strSPH_F:= Trim(SPH_F);
  strSPH_N:= Trim(SPH_N);  
  
  strSPH_F:= Trim(T2WStringReplace(strSPH_F, ' ', '', True, True));
  strSPH_N:= Trim(T2WStringReplace(strSPH_N, ' ', '', True, True));

  bolRes:= T2WStrToFloatUniversal(strSPH_F, False, a1);
  
  bolRes:= T2WStrToFloatUniversal(strSPH_N, False, a2);  

  if (a2 < a1) then
  begin
    // Der Nahwert muss größer als der Fernwert sein
    strResult:= '0.00';
  end
  else
  begin      
    Res:= a1 - a2;
    
    if (Res < 0) then
      Res:= Res * -1;

    strResult:= Trim(T2WFloatToStrF(Res, 10, 2));
    strResult:= Trim(T2WStringReplace(strResult, ',', '.', True, True));  
  end;     
   
  Result:= strResult;
end; 
  
var
  Data, Sep1, S1, ParsedData, PD_G, HSA, BLUR, VisusRightLeftEye: String;
  RightEye_F, LeftEye_F, RightEye_N, LeftEye_N, VIS_S_B, VIS_C_B: String;
  i, DataIndexRightEye, DataIndexLeftEye: Integer;
  R_SPH_F, R_SPH_N, R_CYL, R_AXIS, R_PRISM, R_ACC, R_VIS_S_R, R_VIS_C_R, R_PD_R, R_ADD: String;
  R_arr_SPH_F, R_arr_SPH_N, R_arr_CYL, R_arr_AXIS, R_arr_PRISM, R_arr_ACC, R_arr_PD, R_arr_VIS_S_R, R_arr_VIS_C_R: TStringArray;
  arrData1, arrPD, arrHSA, arrBLUR, arr_VIS_S_B, arr_VIS_C_B: TStringArray;
  L_SPH_F, L_SPH_N, L_CYL, L_AXIS, L_PRISM, L_ACC, L_VIS_S_L, L_VIS_C_L, L_PD_L, L_ADD: String;
  L_arr_SPH_F, L_arr_SPH_N, L_arr_CYL, L_arr_AXIS, L_arr_PRISM, L_arr_ACC, L_arr_PD, L_arr_VIS_S_L, L_arr_VIS_C_L: TStringArray;
  bolAddSign, bolAddSignSeparator, bolAddAxisSeparator, bolAddVDValueToOutput, bolAddPrismDataToOutput, bolDontExportNearValues: Boolean;
  bolUseSeparatedVisusExportLine, bolVisVP, bolVis900, bolAddPDValueToOutput: Boolean;
  
begin
  // Verwende "True", damit der VD (HSA) Wert hinzugefügt wird, benutze "False",
  // damit der VD Wert nicht per GDT exportiert wird.
  bolAddVDValueToOutput:= False;

  // Verwende "True", damit immer ein Vorzeichen hinzugefügt wird,
  // benutze "False", damit nur der gemessene Wert eingetragen wird wie er 
  // vom Gerät kommt. 
  bolAddSign:= True;
  
  // Verwende "True", damit nach jedem Vorzeichen der Wert aus GDT_SIGN_SEPARATOR 
  // angefügt wird, benutze "False", damit kein Abstand zwischen Vorzeichen
  // und Wert eingefügt wird.
  bolAddSignSeparator:= True;
  
  // Verwende "True", damit der Achsenseparator angefügt wird,
  // benutze "False", damit der Achsenseparator nicht verwendet wird.
  bolAddAxisSeparator:= True; 
  
  // Verwende "True", damit die Prismen Daten angefügt wird,
  // benutze "False", damit der Prismen Daten nicht verwendet wird. 
  // Wenn das Gerät keinen Prismenkompensator hat, dann hier False verwenden.
  bolAddPrismDataToOutput:= False;
  
  // Verwende "True", damit die Nahwerte NICHT eingetragen werden,
  // benutze "False", damit die Nahwerte per GDT exportiert werden.
  bolDontExportNearValues:= True;  
 
  // Verwende "True", damit die Visus B Daten eine eigene Zeile erhalten,
  // benutze "False", damit nur eine komplette Visus Zeile exportiert wird.
  bolUseSeparatedVisusExportLine:= False;
  
  // Verwende "True", damit der PD Wert hinzugefügt wird, benutze "False",
  // damit der PD Wert nicht per GDT exportiert wird.
  bolAddPDValueToOutput:= False;  
    
  // --- Don't edit script down below ---   

  // Clear parsed data string
  FParsedDataString:= '';

  if Length(FRawDataString) <= 0 then
  begin
    FLastErrorCode:= -2;
    FLastErrorMessage:= 'Keine Daten für die Verarbeitung der Daten gefunden';

    DoPSError();
 
    Exit;
  end;
  
  // T2WMessageBoxS(FRawDataString);
  
  // DoPSSaveFile('test1.txt');  

  Sep1:= DATA_SEPARATOR_01;
  
  Data:= String(FRawDataString);
  Data:= Trim(Data);

  // Get array from 1st separator
  arrData1:= Explode(Sep1, Data, 0);  
  
  if Length(arrData1) <= 0 then
  begin
    FLastErrorCode:= -3;
    FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Keine Parameter gefunden';

    DoPSError();

    Exit;
  end;

  // Parse raw data
  bolVisVP:= False;
  bolVis900:= False;

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
  R_ADD:= '';

  L_SPH_F:= '';
  L_SPH_N:= '';
  L_CYL:= '';
  L_AXIS:= '';
  L_PRISM:= '';
  L_ACC:= '';
  L_VIS_S_L:= '';
  L_VIS_C_L:= '';
  L_PD_L:= '';
  L_ADD:= '';

  DataIndexRightEye:= -1;
  DataIndexLeftEye:= -1;

  // loop through the data
  for i:= 0 to Length(arrData1) - 1 do
  begin
    S1:= Trim(arrData1[i]);

    if (S1 = '') then
    begin
      Continue;
    end;

    // check which protocoll we have here
    if (S1 = 'VIS') then
    begin
      // "VisVP" EMR driver data output, not existend for Haag-Streit Refractor 900
      bolVisVP:= True;

      Continue;
    end;

    if (S1 = 'VIS900') then
    begin
      // "Vis900" EMR driver data output, default value of Haag-Streit Refractor 900
      bolVis900:= True;

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
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter SPH_F nicht korrekt';

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
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter SPH_N nicht korrekt';

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
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter CYL nicht korrekt';

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
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter AXIS nicht korrekt';

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
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter PRISM nicht korrekt';

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
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter ACC nicht korrekt';

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
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter SPH_F nicht korrekt';

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
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter SPH_N nicht korrekt';

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
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter CYL nicht korrekt';

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
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter AXIS nicht korrekt';

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
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter PRISM nicht korrekt';

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
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter ACC nicht korrekt';

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
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter PD nicht korrekt';

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
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter HSA nicht korrekt';

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
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter BLUR nicht korrekt';

          DoPSError();

          Exit;
        end;

        Continue;
      end;
    end;

    // if we have the "Vis900" protocoll, use this peace of code...
    if (bolVis900) then
    begin
      //
      if T2WStartsStr(PHOROPTER_DATA_INDEX_RIGHT_EYE_2, S1) then
      begin
        DataIndexRightEye:= i;
        DataIndexLeftEye:= -1;

        Continue;
      end;

      //
      if (DataIndexRightEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_SPH_F_R, S1))
      then
      begin
        R_SPH_F:= Trim(S1);

        R_arr_SPH_F:= Explode(DATA_SEPARATOR_02, R_SPH_F, 0);
      
        if Length(R_arr_SPH_F) < 2 then
        begin
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter SPH_F_R nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexRightEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_SPH_N_R, S1))
      then
      begin
        R_SPH_N:= Trim(S1);
      
        R_arr_SPH_N:= Explode(DATA_SEPARATOR_02, R_SPH_N, 0);
      
        if Length(R_arr_SPH_N) < 2 then
        begin
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter SPH_N_R nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexRightEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_CYL_R, S1))
      then
      begin
        R_CYL:= Trim(S1);
      
        R_arr_CYL:= Explode(DATA_SEPARATOR_02, R_CYL, 0);
      
        if Length(R_arr_CYL) < 2 then
        begin
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter CYL_R nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexRightEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_AXIS_R, S1))
      then
      begin
        R_AXIS:= Trim(S1);
      
        R_arr_AXIS:= Explode(DATA_SEPARATOR_02, R_AXIS, 0);
      
        if Length(R_arr_AXIS) < 2 then
        begin
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter AXIS_R nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexRightEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_PRISM_R, S1))
      then
      begin
        R_PRISM:= Trim(S1);
      
        R_arr_PRISM:= Explode(DATA_SEPARATOR_02, R_PRISM, 0);
      
        if Length(R_arr_PRISM) < 2 then
        begin
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter PRISM_R nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;
      
      // 
      if (DataIndexRightEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_ACC_R, S1))
      then
      begin
        R_ACC:= Trim(S1);
      
        R_arr_ACC:= Explode(DATA_SEPARATOR_02, R_ACC, 0);
      
        if Length(R_arr_ACC) < 2 then
        begin
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter ACC_R nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexRightEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_VIS_S_R, S1))
      then
      begin
        R_VIS_S_R:= Trim(S1);
      
        R_arr_VIS_S_R:= Explode(DATA_SEPARATOR_02, R_VIS_S_R, 0);
      
        if Length(R_arr_VIS_S_R) < 2 then
        begin
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter VIS_S_R nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexRightEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_VIS_C_R, S1))
      then
      begin
        R_VIS_C_R:= Trim(S1);
      
        R_arr_VIS_C_R:= Explode(DATA_SEPARATOR_02, R_VIS_C_R, 0);
      
        if Length(R_arr_VIS_C_R) < 2 then
        begin
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter VIS_C_R nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexRightEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_PD_R, S1))
      then
      begin
        R_PD_R:= Trim(S1);
      
        R_arr_PD:= Explode(DATA_SEPARATOR_02, R_PD_R, 0);
      
        if Length(R_arr_PD) < 2 then
        begin
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter PD_R nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // -----------------------------------------------------------------------

      //
      if T2WStartsStr(PHOROPTER_DATA_INDEX_LEFT_EYE_2, S1) then
      begin
        DataIndexLeftEye:= i;
        DataIndexRightEye:= -1;

        Continue;
      end;
      
      //
      if (DataIndexLeftEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_SPH_F_L, S1))
      then
      begin
        L_SPH_F:= Trim(S1);

        L_arr_SPH_F:= Explode(DATA_SEPARATOR_02, L_SPH_F, 0);
      
        if Length(L_arr_SPH_F) < 2 then
        begin
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter SPH_F_L nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexLeftEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_SPH_N_L, S1))
      then
      begin
        L_SPH_N:= Trim(S1);
      
        L_arr_SPH_N:= Explode(DATA_SEPARATOR_02, L_SPH_N, 0);
      
        if Length(L_arr_SPH_N) < 2 then
        begin
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter SPH_N_L nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;
       
      // 
      if (DataIndexLeftEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_CYL_L, S1))
      then
      begin
        L_CYL:= Trim(S1);
      
        L_arr_CYL:= Explode(DATA_SEPARATOR_02, L_CYL, 0);
      
        if Length(L_arr_CYL) < 2 then
        begin
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter CYL_L nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexLeftEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_AXIS_L, S1))
      then
      begin
        L_AXIS:= Trim(S1);
      
        L_arr_AXIS:= Explode(DATA_SEPARATOR_02, L_AXIS, 0);
      
        if Length(L_arr_AXIS) < 2 then
        begin
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter AXIS_L nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexLeftEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_PRISM_L, S1))
      then
      begin
        L_PRISM:= Trim(S1);
      
        L_arr_PRISM:= Explode(DATA_SEPARATOR_02, L_PRISM, 0);
      
        if Length(L_arr_PRISM) < 2 then
        begin
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter PRISM_L nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexLeftEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_ACC_L, S1))
      then
      begin
        L_ACC:= Trim(S1);
      
        L_arr_ACC:= Explode(DATA_SEPARATOR_02, L_ACC, 0);
      
        if Length(L_arr_ACC) < 2 then
        begin
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter ACC_L nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexLeftEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_VIS_S_L, S1))
      then
      begin
        L_VIS_S_L:= Trim(S1);
      
        L_arr_VIS_S_L:= Explode(DATA_SEPARATOR_02, L_VIS_S_L, 0);
      
        if Length(L_arr_VIS_S_L) < 2 then
        begin
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter VIS_S_L nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexLeftEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_VIS_C_L, S1))
      then
      begin
        L_VIS_C_L:= Trim(S1);
      
        L_arr_VIS_C_L:= Explode(DATA_SEPARATOR_02, L_VIS_C_L, 0);
      
        if Length(L_arr_VIS_C_L) < 2 then
        begin
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter VIS_C_L nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // 
      if (DataIndexLeftEye >= 0)
      and (T2WStartsStr(PHOROPTER_DATA_INDEX_PD_L, S1))
      then
      begin
        L_PD_L:= Trim(S1);
      
        L_arr_PD:= Explode(DATA_SEPARATOR_02, L_PD_L, 0);
      
        if Length(L_arr_PD) < 2 then
        begin
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter PD_L nicht korrekt';

          DoPSError();

          Exit;
        end;
      
        Continue;
      end;

      // -----------------------------------------------------------------------

      //
      if T2WStartsStr(PHOROPTER_DATA_INDEX_PD_G, S1) then
      begin
        PD_G:= Trim(S1);

        arrPD:= Explode(DATA_SEPARATOR_02, PD_G, 0);

        if Length(arrPD) < 2 then
        begin
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter PD_G nicht korrekt';

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
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter HSA nicht korrekt';

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
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter BLUR nicht korrekt';

          DoPSError();

          Exit;
        end;

        Continue;
      end;

      //
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_VIS_S_B, S1)) then
      begin
        VIS_S_B:= Trim(S1);
      
        arr_VIS_S_B:= Explode(DATA_SEPARATOR_02, VIS_S_B, 0);
      
        if Length(arr_VIS_S_B) < 2 then
        begin
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter VIS_S_B nicht korrekt';

          DoPSError();

          Exit;
        end;

        Continue;
      end;

      // 
      if (T2WStartsStr(PHOROPTER_DATA_INDEX_VIS_C_B, S1)) then
      begin
        VIS_C_B:= Trim(S1);
      
        arr_VIS_C_B:= Explode(DATA_SEPARATOR_02, VIS_C_B, 0);
      
        if Length(arr_VIS_C_B) < 2 then
        begin
          FLastErrorCode:= -4;
          FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter VIS_C_B nicht korrekt';

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
    if (R_PRISM <> '') and (bolAddPrismDataToOutput) then
    begin
      if (Trim(R_PRISM) <> '0.00') then
      begin
        if (RightEye_F <> RIGHT_EYE_START_MARKER) then
          RightEye_F:= RightEye_F + ' ';

        RightEye_F:= RightEye_F + 'P= ';       
      end;
    end;

    // 
    if (R_PRISM <> '') and (bolAddPrismDataToOutput) then
    begin
      S1:= Trim(R_arr_PRISM[1]);
      S1:= T2WStringReplace(S1, 'IN', 'I', True, False);
      S1:= T2WStringReplace(S1, 'OUT', 'O', True, False);

      RightEye_F:= RightEye_F + S1;
    end;

    // 
    if (R_ACC <> '') then
    begin
      // do nothing
    end;
    
    // ADD
    R_ADD:= '';
    
    if (R_SPH_F <> '') and (R_SPH_N <> '') then
    begin
      R_ADD:= CalculateAdditionValue(Trim(R_arr_SPH_F[1]), Trim(R_arr_SPH_N[1]));
 
      if (R_ADD <> '') then
      begin
        if RightEye_F <> RIGHT_EYE_START_MARKER then
          RightEye_F:= RightEye_F + ' ';

        RightEye_F:= RightEye_F + FormatSignValue('A=', Trim(R_ADD), bolAddSign, bolAddSignSeparator);     
      end;
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
    if (L_PRISM <> '') and (bolAddPrismDataToOutput) then
    begin
      if (Trim(L_PRISM) <> '0.00') then
      begin
        if LeftEye_F <> LEFT_EYE_START_MARKER then
          LeftEye_F:= LeftEye_F + ' ';

        LeftEye_F:= LeftEye_F + 'P= ';       
      end;
    end;

    //
    if (L_PRISM <> '') and (bolAddPrismDataToOutput) then
    begin
      S1:= Trim(L_arr_PRISM[1]);
      S1:= T2WStringReplace(S1, 'UP', 'U', True, False);
      S1:= T2WStringReplace(S1, 'DOWN', 'D', True, False);

      LeftEye_F:= LeftEye_F + S1;
    end;

    // 
    if (L_ACC <> '') then
    begin
      // do nothing
    end;
    
    // ADD
    L_ADD:= '';
    
    if (L_SPH_F <> '') and (L_SPH_N <> '') then
    begin
      L_ADD:= CalculateAdditionValue(Trim(L_arr_SPH_F[1]), Trim(L_arr_SPH_N[1])); 
                 
      if (L_ADD <> '') then
      begin
        if LeftEye_F <> LEFT_EYE_START_MARKER then
          LeftEye_F:= LeftEye_F + ' ';

        LeftEye_F:= LeftEye_F + FormatSignValue('A=', Trim(L_ADD), bolAddSign, bolAddSignSeparator);      
      end;       
    end;      

    // -------------------------------------------------------------------------

    //
    if (PD_G <> '') and (bolAddPDValueToOutput) then
    begin
      if RightEye_F <> RIGHT_EYE_START_MARKER then
        RightEye_F:= RightEye_F + ' ';

      RightEye_F:= RightEye_F + 'PD= ' + Trim(arrPD[1]);
    end;

    //
    if (PD_G <> '') and (bolAddPDValueToOutput) then
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
      // do nothing
    end;

    // -------------------------------------------------------------------------

    // 
    if (R_SPH_N <> '') and (RightEye_N <> '') and (not bolDontExportNearValues) then
    begin      
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA_01 + FOutputLineSeparator + RightEye_N + FOutputLineSeparator;
    end;

    //
    if (L_SPH_N <> '') and (LeftEye_N <> '') and (not bolDontExportNearValues) then
    begin     
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA_01 + FOutputLineSeparator + LeftEye_N;
    end;

    // Add right eye (F) to final output
    if (RightEye_F <> '') then  
    begin  
      ParsedData:= ParsedData + FOutputLineSeparator;
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA_01 + FOutputLineSeparator + RightEye_F;
    end;

    // Add left eye (F) to final output
    if (LeftEye_F <> '') then   
    begin 
      ParsedData:= ParsedData + FOutputLineSeparator;
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA_01 + FOutputLineSeparator + LeftEye_F;
    end;
  end;

  // ---------------------------------------------------------------------------

  // if we have the "Vis900" protocoll, use this peace of code...
  // default value of Haag-Streit Refractor 900
  if (bolVis900) then
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
    if (R_PRISM <> '') and (bolAddPrismDataToOutput) then
    begin
      if (Trim(R_PRISM) <> '0.00') then
      begin
        if RightEye_F <> RIGHT_EYE_START_MARKER then
          RightEye_F:= RightEye_F + ' ';

        RightEye_F:= RightEye_F + 'P= ';     
      end;
    end;

    // 
    if (R_PRISM <> '') and (bolAddPrismDataToOutput) then
    begin
      S1:= Trim(R_arr_PRISM[1]);
      S1:= T2WStringReplace(S1, 'IN', 'I', True, False);
      S1:= T2WStringReplace(S1, 'OUT', 'O', True, False);

      RightEye_F:= RightEye_F + S1;
    end;

    // 
    if (R_ACC <> '') then
    begin
      // do nothing
    end;
    
    // ADD
    R_ADD:= '';

    if (R_SPH_F <> '') and (R_SPH_N <> '') then
    begin
      R_ADD:= CalculateAdditionValue(Trim(R_arr_SPH_F[1]), Trim(R_arr_SPH_N[1]));

      if (R_ADD <> '') then
      begin
        if RightEye_F <> RIGHT_EYE_START_MARKER then
          RightEye_F:= RightEye_F + ' ';

        RightEye_F:= RightEye_F + FormatSignValue('A=', Trim(R_ADD), bolAddSign, bolAddSignSeparator);     
      end;
    end; 

    // 
    if (R_PD_R <> '') and (bolAddPDValueToOutput) then
    begin
      if RightEye_F <> RIGHT_EYE_START_MARKER then
        RightEye_F:= RightEye_F + ' ';

      RightEye_F:= RightEye_F + 'PD= ' + Trim(R_arr_PD[1]);
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
    if (L_PRISM <> '') and (bolAddPrismDataToOutput) then
    begin
      if (Trim(L_PRISM) <> '0.00') then
      begin
        if LeftEye_F <> LEFT_EYE_START_MARKER then
          LeftEye_F:= LeftEye_F + ' ';

        LeftEye_F:= LeftEye_F + 'P= ';
      end;
    end;

    // 
    if (L_PRISM <> '') and (bolAddPrismDataToOutput) then
    begin
      S1:= Trim(L_arr_PRISM[1]);
      S1:= T2WStringReplace(S1, 'UP', 'U', True, False);
      S1:= T2WStringReplace(S1, 'DOWN', 'D', True, False);

      LeftEye_F:= LeftEye_F + S1;
    end;

    // 
    if (L_ACC <> '') then
    begin
      // do nothing
    end;
    
    // ADD
    L_ADD:= '';

    if (L_SPH_F <> '') and (L_SPH_N <> '') then
    begin
      L_ADD:= CalculateAdditionValue(Trim(L_arr_SPH_F[1]), Trim(L_arr_SPH_N[1]));     
      
      if (L_ADD <> '') then
      begin
        if LeftEye_F <> LEFT_EYE_START_MARKER then
          LeftEye_F:= LeftEye_F + ' ';

        LeftEye_F:= LeftEye_F + FormatSignValue('A=', Trim(L_ADD), bolAddSign, bolAddSignSeparator);       
      end;
    end; 

    // 
    if (L_PD_L <> '') and (bolAddPDValueToOutput) then
    begin
      if LeftEye_F <> LEFT_EYE_START_MARKER then
        LeftEye_F:= LeftEye_F + ' ';

      LeftEye_F:= LeftEye_F + 'PD= ' + Trim(L_arr_PD[1]);
    end;

    // -------------------------------------------------------------------------

    //
    if (PD_G <> '') and (bolAddPDValueToOutput) then
    begin
      // do nothing
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
      // do nothing
    end;

    // -------------------------------------------------------------------------

    // 
    if (R_SPH_N <> '') and (RightEye_N <> '') and (not bolDontExportNearValues) then
    begin     
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA_01 + FOutputLineSeparator + RightEye_N + FOutputLineSeparator;
    end;

    //
    if (L_SPH_N <> '') and (LeftEye_N <> '') and (not bolDontExportNearValues) then
    begin     
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA_01 + FOutputLineSeparator + LeftEye_N;
    end;

    // Add right eye (F) to final output
    if (RightEye_F <> '') then 
    begin  
      ParsedData:= ParsedData + FOutputLineSeparator;
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA_01 + FOutputLineSeparator + RightEye_F;
    end;

    // Add left eye (F) to final output
    if (LeftEye_F <> '') then    
    begin
      ParsedData:= ParsedData + FOutputLineSeparator;
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA_01 + FOutputLineSeparator + LeftEye_F;
    end;

    //
    if (R_VIS_S_R <> '') or (R_VIS_C_R <> '') then
    begin
      VisusRightLeftEye:= VisusRightLeftEye + 'R:';

      if (R_VIS_S_R <> '') then
      begin
        VisusRightLeftEye:= VisusRightLeftEye + 'sc=' + Trim(R_arr_VIS_S_R[1]);
      end;

      if (R_VIS_C_R <> '') then
      begin
        if (R_VIS_S_R <> '') then
          VisusRightLeftEye:= VisusRightLeftEye + ' ';

        VisusRightLeftEye:= VisusRightLeftEye + 'cc=' + Trim(R_arr_VIS_C_R[1]);
      end;
    end;

    if (L_VIS_S_L <> '') or (L_VIS_C_L <> '') then
    begin
      VisusRightLeftEye:= VisusRightLeftEye + ' //L:';

      if (L_VIS_S_L <> '') then
      begin
        VisusRightLeftEye:= VisusRightLeftEye + 'sc=' + Trim(L_arr_VIS_S_L[1]);
      end;

      if (L_VIS_C_L <> '') then
      begin
        if (L_VIS_S_L <> '') then
          VisusRightLeftEye:= VisusRightLeftEye + ' ';

        VisusRightLeftEye:= VisusRightLeftEye + 'cc=' + Trim(L_arr_VIS_C_L[1]);
      end;
    end;  

    if (not bolUseSeparatedVisusExportLine) then
    begin
      if (VIS_S_B <> '') or (VIS_C_B <> '') then
      begin
        if (VisusRightLeftEye <> '') then
        begin
          VisusRightLeftEye:= VisusRightLeftEye + ' ';
        end;

        VisusRightLeftEye:= VisusRightLeftEye + '//' + 'B ';
        
        if (VIS_S_B <> '') then
        begin
          VisusRightLeftEye:= VisusRightLeftEye + 'sc=' + Trim(arr_VIS_S_B[1]);
        end;
        
        if (VIS_C_B <> '') then
        begin
          if (VIS_S_B <> '') then
          begin
            VisusRightLeftEye:= VisusRightLeftEye + ' ';
          end;

          VisusRightLeftEye:= VisusRightLeftEye + 'cc=' + Trim(arr_VIS_C_B[1]);
        end;                
      end;
    end;
    
    if (VisusRightLeftEye <> '') then
    begin
      ParsedData:= ParsedData + FOutputLineSeparator;
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_RESULT_01 + FOutputLineSeparator + VisusRightLeftEye;
    end;    
    
    if (bolUseSeparatedVisusExportLine) then
    begin
      if (VIS_S_B <> '') or (VIS_C_B <> '') then
      begin
        ParsedData:= ParsedData + FOutputLineSeparator;
        ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FOREIGN_RESULT_01 + FOutputLineSeparator;

        ParsedData:= ParsedData + 'B:';

        if (VIS_S_B <> '') then
        begin
          ParsedData:= ParsedData + 'sc=' + Trim(arr_VIS_S_B[1]);
        end;

        if (VIS_C_B <> '') then
        begin
          if (VIS_S_B <> '') then
          begin
            ParsedData:= ParsedData + ' ';
          end;

          ParsedData:= ParsedData + 'cc=' + Trim(arr_VIS_C_B[1]);
        end;
      end;    
    end;
  end;

  // Set result
  FParsedDataString:= RawByteString(Trim(ParsedData));
end.
