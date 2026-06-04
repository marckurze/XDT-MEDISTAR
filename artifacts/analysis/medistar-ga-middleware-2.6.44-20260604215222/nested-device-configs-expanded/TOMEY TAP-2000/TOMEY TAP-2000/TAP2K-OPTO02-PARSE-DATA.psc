const
  VERSION = '1.0.55.62';
  DATE = '27.05.2024 13:08:09';
  TEXT = 'Copyright (c) 2024 team2work GmbH';

  DATA_SEPARATOR_01 = #$17; // ETB (End-of-Transmission-Block)
  DATA_SEPARATOR_02 = #$7C; // Pipe Char (|)
  
  PHOROPTER_DATA_INDEX_PD = '*PD' + DATA_SEPARATOR_02;
  PHOROPTER_DATA_INDEX_WD = '*WD' + DATA_SEPARATOR_02;
  
  PHOROPTER_DATA_INDEX_FN = '*FN'; 
  PHOROPTER_DATA_INDEX_AR = '*AR';   
  PHOROPTER_DATA_INDEX_LM = '*LM';
  PHOROPTER_DATA_INDEX_SJ = '*SJ';
  
  PHOROPTER_DATA_INDEX_FN_SP = '*SP' + DATA_SEPARATOR_02;
  PHOROPTER_DATA_INDEX_FN_CY = '*CY' + DATA_SEPARATOR_02;
  PHOROPTER_DATA_INDEX_FN_AX = '*AX' + DATA_SEPARATOR_02;
  PHOROPTER_DATA_INDEX_FN_AD = '*AD' + DATA_SEPARATOR_02;
  PHOROPTER_DATA_INDEX_FN_VA = '*VA' + DATA_SEPARATOR_02;
  PHOROPTER_DATA_INDEX_FN_PH = '*PH' + DATA_SEPARATOR_02;
  PHOROPTER_DATA_INDEX_FN_PV = '*PV' + DATA_SEPARATOR_02;
  
  PHOROPTER_DATA_INDEX_AR_SP = '*SP' + DATA_SEPARATOR_02;  
  PHOROPTER_DATA_INDEX_AR_CY = '*CY' + DATA_SEPARATOR_02;  
  PHOROPTER_DATA_INDEX_AR_AX = '*AX' + DATA_SEPARATOR_02;
  PHOROPTER_DATA_INDEX_AR_AD = '*AD' + DATA_SEPARATOR_02;  
  PHOROPTER_DATA_INDEX_AR_VA = '*VA' + DATA_SEPARATOR_02;
  PHOROPTER_DATA_INDEX_AR_PH = '*PH' + DATA_SEPARATOR_02;  
  PHOROPTER_DATA_INDEX_AR_PV = '*PV' + DATA_SEPARATOR_02;  
  PHOROPTER_DATA_INDEX_AR_KM = '*KM' + DATA_SEPARATOR_02;   
  PHOROPTER_DATA_INDEX_AR_AV = '*AV' + DATA_SEPARATOR_02;  
  
  PHOROPTER_DATA_INDEX_LM_SP = '*SP' + DATA_SEPARATOR_02;  
  PHOROPTER_DATA_INDEX_LM_CY = '*CY' + DATA_SEPARATOR_02;  
  PHOROPTER_DATA_INDEX_LM_AX = '*AX' + DATA_SEPARATOR_02;
  PHOROPTER_DATA_INDEX_LM_AD = '*AD' + DATA_SEPARATOR_02;  
  PHOROPTER_DATA_INDEX_LM_VA = '*VA' + DATA_SEPARATOR_02;
  PHOROPTER_DATA_INDEX_LM_PH = '*PH' + DATA_SEPARATOR_02;  
  PHOROPTER_DATA_INDEX_LM_PV = '*PV' + DATA_SEPARATOR_02;  
  PHOROPTER_DATA_INDEX_LM_KM = '*KM' + DATA_SEPARATOR_02;   
  PHOROPTER_DATA_INDEX_LM_AV = '*AV' + DATA_SEPARATOR_02;  
   
  RIGHT_EYE_START_MARKER = 'R.:';
  LEFT_EYE_START_MARKER = 'L.:';
  
  PRISM_HORIZONTAL_IDENTIFIER_SEPARATOR = ' ';
  PRISM_HORIZONTAL_IDENTIFIER_IN = 'BI' + PRISM_HORIZONTAL_IDENTIFIER_SEPARATOR;
  PRISM_HORIZONTAL_IDENTIFIER_OUT = 'BO' + PRISM_HORIZONTAL_IDENTIFIER_SEPARATOR;
  PRISM_HORIZONTAL_VALUE_IN = 'I';
  PRISM_HORIZONTAL_VALUE_OUT = 'O';
  
  PRISM_VERTICAL_IDENTIFIER_SEPARATOR = ' ';
  PRISM_VERTICAL_IDENTIFIER_UP = 'BU' + PRISM_VERTICAL_IDENTIFIER_SEPARATOR;
  PRISM_VERTICAL_IDENTIFIER_DOWN = 'BD' + PRISM_VERTICAL_IDENTIFIER_SEPARATOR;
  PRISM_VERTICAL_VALUE_UP = 'U';
  PRISM_VERTICAL_VALUE_DOWN = 'D';
	
  GDT_FID_MEASURE_DATA_01 = '6228';
  GDT_FID_COMMENT_01 = '6227';
  
  DEVICE_NAME = 'TAP2K';
  DEVICE_TYPE = 'PHOROPTER';
  
  GDT_SIGN_SEPARATOR = ' ';
  GDT_AXIS_SEPARATOR = ' ';
  GDT_AXIS_SEPARATOR_MAX_COUNT = 3;  
  
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
  Data, Sep1, S1, ParsedData, PD, WD, RightEye, LeftEye: String;
  FN_SP, FN_CY, FN_AX, FN_AD, FN_VA, FN_PH, FN_PV: String;
  arrData1, arrPD: TStringArray;
  arrFN_SP, arrFN_CY, arrFN_AX, arrFN_AD, arrFN_VA, arrFN_PH, arrFN_PV: TStringArray;
  i, FN_DataIndex, SJ_DataIndex, AR_DataIndex, LM_DataIndex: Integer;
  bolAddWDValueToOutput, bolAddSign, bolAddSignSeparator, bolAddAxisSeparator: Boolean;
  
begin
  // Verwende "True", damit der WD Wert hinzugefügt wird, benutze "False", damit der
  // WD Wert nicht per GDT exportiert wird.
  bolAddWDValueToOutput:= True; 
 
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
  PD:= '';
  WD:= '';
  
  FN_SP:= '';
  FN_CY:= '';
  FN_AX:= '';
  FN_AD:= '';
  FN_VA:= '';
  FN_PH:= '';
  FN_PV:= '';
  
  //
  FN_DataIndex:= -1;
  AR_DataIndex:= -1;
  LM_DataIndex:= -1;
  SJ_DataIndex:= -1;
    
  //
  for i := 0 to Length(arrData1) - 1 do
  begin
    S1:= Trim(arrData1[i]);

    if S1 = '' then
      Continue;
  
    if T2WEndsStr(DATA_SEPARATOR_02,
                  S1) then
    begin
      S1:= Copy(S1, 1, Length(S1) - 1);
    end;
  
    //
    if T2WStartsStr(PHOROPTER_DATA_INDEX_PD,
	            S1) then
    begin
      PD:= T2WStringReplace(S1,
                            PHOROPTER_DATA_INDEX_PD,
			    '',
			    False,
			    False);
      
      PD:= Trim(PD);
      
      arrPD:= Explode(DATA_SEPARATOR_02, PD, 0);
      
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
    if T2WStartsStr(PHOROPTER_DATA_INDEX_WD,
                    S1) then
    begin
      WD:= T2WStringReplace(S1,
                            PHOROPTER_DATA_INDEX_WD,
                            '',
                            False,
                            False);
        
      WD:= Trim(WD);
      
      Continue;
    end;
          
    // -- BEGIN FN --
    
    if T2WStartsStr(PHOROPTER_DATA_INDEX_FN,
                    S1) then
    begin
      FN_DataIndex:= i;
      
      Continue;
    end;
  
    //
    if (FN_DataIndex >= 0)
    and (T2WStartsStr(PHOROPTER_DATA_INDEX_FN_SP,
                      S1))
    then
    begin
      FN_SP:= T2WStringReplace(S1,
                               PHOROPTER_DATA_INDEX_FN_SP,
                               '',
                               False,
                               False);
        
      FN_SP:= Trim(FN_SP);
      
      arrFN_SP:= Explode(DATA_SEPARATOR_02, FN_SP, 0);
      
      if Length(arrFN_SP) < 2 then
      begin
        FLastErrorCode := -5;
        FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter FN -> SP nicht korrekt';

        DoPSError();

        Exit;
      end;
      
      Continue;
    end;
  
    //
    if (FN_DataIndex >= 0)
    and (T2WStartsStr(PHOROPTER_DATA_INDEX_FN_CY,
                      S1))
    then
    begin
      FN_CY:= T2WStringReplace(S1,
                               PHOROPTER_DATA_INDEX_FN_CY,
                               '',
                               False,
                               False);
        
      FN_CY:= Trim(FN_CY);
      
      arrFN_CY:= Explode(DATA_SEPARATOR_02, FN_CY, 0);
      
      if Length(arrFN_CY) < 2 then
      begin
        FLastErrorCode := -6;
        FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter FN -> CY nicht korrekt';

        DoPSError();

        Exit;
      end;
      
      Continue;
    end;
  
    //
    if (FN_DataIndex >= 0)
    and (T2WStartsStr(PHOROPTER_DATA_INDEX_FN_AX,
                      S1))
    then
    begin
      FN_AX:= T2WStringReplace(S1,
                               PHOROPTER_DATA_INDEX_FN_AX,
                               '',
                               False,
                               False);
        
      FN_AX:= Trim(FN_AX);
      
      arrFN_AX:= Explode(DATA_SEPARATOR_02, FN_AX, 0);
      
      if Length(arrFN_AX) < 2 then
      begin
        FLastErrorCode := -7;
        FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter FN -> AX nicht korrekt';

        DoPSError();

        Exit;
      end;
      
      Continue;
    end;
  
    //
    if (FN_DataIndex >= 0)
    and (T2WStartsStr(PHOROPTER_DATA_INDEX_FN_AD,
                      S1))
    then
    begin
      FN_AD:= T2WStringReplace(S1,
                               PHOROPTER_DATA_INDEX_FN_AD,
                               '',
                               False,
                               False);
      
      FN_AD:= Trim(FN_AD);
      
      arrFN_AD:= Explode(DATA_SEPARATOR_02, FN_AD, 0);
      
      if Length(arrFN_AD) < 2 then
      begin
        FLastErrorCode := -8;
        FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter FN -> AD nicht korrekt';

        DoPSError();

        Exit;
      end;
      
      Continue;
    end;
  
    //
    if (FN_DataIndex >= 0)
    and (T2WStartsStr(PHOROPTER_DATA_INDEX_FN_VA,
                      S1))
    then
    begin
      FN_VA:= T2WStringReplace(S1,
                               PHOROPTER_DATA_INDEX_FN_VA,
                               '',
                               False,
                               False);
        
      FN_VA:= Trim(FN_VA);
      
      arrFN_VA:= Explode(DATA_SEPARATOR_02, FN_VA, 0);
      
      if Length(arrFN_VA) < 2 then
      begin
        FLastErrorCode := -9;
        FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter FN -> VA nicht korrekt';

        DoPSError();

        Exit;
      end;
      
      Continue;
    end;
  
    //
    if (FN_DataIndex >= 0)
    and (T2WStartsStr(PHOROPTER_DATA_INDEX_FN_PH,
                      S1))
    then
    begin
      FN_PH:= T2WStringReplace(S1,
                               PHOROPTER_DATA_INDEX_FN_PH,
                               '',
                               False,
                               False);
        
      FN_PH:= Trim(FN_PH);
      
      arrFN_PH:= Explode(DATA_SEPARATOR_02, FN_PH, 0);
      
      if Length(arrFN_PH) < 2 then
      begin
        FLastErrorCode := -10;
        FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter FN -> PH nicht korrekt';

        DoPSError();

        Exit;
      end;
      
      Continue;
    end;
  
    //
    if (FN_DataIndex >= 0)
    and (T2WStartsStr(PHOROPTER_DATA_INDEX_FN_PV,
                      S1))
    then
    begin
      FN_PV:= T2WStringReplace(S1,
                               PHOROPTER_DATA_INDEX_FN_PV,
                               '',
                               False,
                               False);
        
      FN_PV:= Trim(FN_PV);
      
      arrFN_PV:= Explode(DATA_SEPARATOR_02, FN_PV, 0);
      
      if Length(arrFN_PV) < 2 then
      begin
        FLastErrorCode := -11;
        FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Parameter FN -> PV nicht korrekt';

        DoPSError();

        Exit;
      end;
      
      Continue;
    end;
    
    // 
    
    // -- END FN --
    
    // -- BEGIN AR --

    // -- END AR --    
    
    // -- BEGIN LM --

    // -- END LM --     
       
    // -- BEGIN SJ --
    
    if T2WStartsStr(PHOROPTER_DATA_INDEX_SJ,
                    S1) then
    begin
      SJ_DataIndex:= i;
      
      Continue;
    end; 
    
    // -- END SJ --          
  end;
  
  // Start parsing ophthalmology data down here  

  // Reset parsed data
  ParsedData:= '';
    
  // -- BEGIN FN --
  if (FN_DataIndex <> -1) then
  begin
    // 
  end;
    
  // Build right eye output
  RightEye:= RIGHT_EYE_START_MARKER;
  
  //
  if (FN_SP <> '') then
  begin
    // FIX: 30.09.2020 (Jochen)
    //      Add support for correct number formatting
    RightEye:= RightEye + FormatSignValue('S=', Trim(arrFN_SP[1]), bolAddSign, bolAddSignSeparator);
  end;
  
  //
  if (FN_CY <> '') then
  begin
    if RightEye <> RIGHT_EYE_START_MARKER then
      RightEye:= RightEye + ' ';
    
    // FIX: 30.09.2020 (Jochen)
    //      Add support for correct number formatting
    RightEye:= RightEye + FormatSignValue('Z=', Trim(arrFN_CY[1]), bolAddSign, bolAddSignSeparator);
  end;
  
  //
  if (FN_AX <> '') then
  begin
    // FIX: 30.09.2020 (Jochen)
    //      Add support for correct number formatting
    RightEye:= RightEye + FormatAxisValue('*', Trim(arrFN_AX[1]), bolAddAxisSeparator);
  end;
  
  // 
  if (FN_PH <> '')
  or (FN_PV <> '')
  then
  begin
    if RightEye <> RIGHT_EYE_START_MARKER then
      RightEye:= RightEye + ' ';
    
    RightEye:= RightEye + 'P= ';
  end;
  
  //
  if (FN_PH <> '') then
  begin
    // IMPORTANT: Check here array length because of different formatting options for line "*PH"
    if Length(arrFN_PH) = 2 then
    begin
      S1:= Trim(arrFN_PH[1]);
      
      if T2WStartsStr(PRISM_HORIZONTAL_IDENTIFIER_IN,
                      S1) then
      begin
        S1:= Copy(S1, Length(PRISM_HORIZONTAL_IDENTIFIER_IN) + 1, Length(S1));
        
        S1:= S1 + ' ' + PRISM_HORIZONTAL_VALUE_IN;
      end;
      
      if T2WStartsStr(PRISM_HORIZONTAL_IDENTIFIER_OUT,
                      S1) then
      begin
        S1:= Copy(S1, Length(PRISM_HORIZONTAL_IDENTIFIER_OUT) + 1, Length(S1));
        
        S1:= S1 + ' ' + PRISM_HORIZONTAL_VALUE_OUT;
      end;
	
      RightEye:= RightEye + S1;
    end
    else if Length(arrFN_PH) = 4 then
    begin
      RightEye:= RightEye + Trim(arrFN_PH[3]) + ' ' + Trim(arrFN_PH[2]);
    end;
  end;
  
  //
  if (FN_PV <> '') then
  begin
    if FN_PH <> '' then
      RightEye:= RightEye + ' ';
    
    // IMPORTANT: Check here array length because of different formatting options for line "*PV"
    if Length(arrFN_PV) = 2 then
    begin
      S1:= Trim(arrFN_PV[1]);
      
      if T2WStartsStr(PRISM_VERTICAL_IDENTIFIER_UP,
                      S1) then
      begin
        S1:= Copy(S1, Length(PRISM_VERTICAL_IDENTIFIER_UP) + 1, Length(S1));
        
        S1:= S1 + ' ' + PRISM_VERTICAL_VALUE_UP;
      end;
      
      if T2WStartsStr(PRISM_VERTICAL_IDENTIFIER_DOWN,
                      S1) then
      begin
        S1:= Copy(S1, Length(PRISM_VERTICAL_IDENTIFIER_DOWN) + 1, Length(S1));
        
        S1:= S1 + ' ' + PRISM_VERTICAL_VALUE_DOWN;
      end;
        
      RightEye:= RightEye + S1;
    end
    else if Length(arrFN_PV) = 4 then
    begin
      RightEye:= RightEye + Trim(arrFN_PV[3]) + ' ' + Trim(arrFN_PV[2]);
    end;
  end;
  
  //
  if (PD <> '') then
  begin
    if RightEye <> RIGHT_EYE_START_MARKER then
      RightEye:= RightEye + ' ';
    
    RightEye:= RightEye + 'PD= ' + Trim(arrPD[1]);
  end;
    
  // 
  if (FN_AD <> '') then  
  begin
    if RightEye <> RIGHT_EYE_START_MARKER then
      RightEye:= RightEye + ' ';
    
    RightEye:= RightEye + 'A= ' + Trim(arrFN_AD[1]);      
  end;  
    
  // Build left eye output
  LeftEye:= LEFT_EYE_START_MARKER;
  
  //
  if (FN_SP <> '') then
  begin
    // FIX: 30.09.2020 (Jochen)
    //      Add support for correct number formatting
    LeftEye:= LeftEye + FormatSignValue('S=', Trim(arrFN_SP[0]), bolAddSign, bolAddSignSeparator);
  end;
  
  //
  if (FN_CY <> '') then
  begin
    if LeftEye <> LEFT_EYE_START_MARKER then
      LeftEye:= LeftEye + ' ';
    
    // FIX: 30.09.2020 (Jochen)
    //      Add support for correct number formatting
    LeftEye:= LeftEye + FormatSignValue('Z=', Trim(arrFN_CY[0]), bolAddSign, bolAddSignSeparator);
  end;
  
  //
  if (FN_AX <> '') then
  begin
    // FIX: 30.09.2020 (Jochen)
    //      Add support for correct number formatting
    LeftEye:= LeftEye + FormatAxisValue('*', Trim(arrFN_AX[0]), bolAddAxisSeparator);
  end;
  
  //
  if (FN_PH <> '')
  or (FN_PV <> '')
  then
  begin
    if LeftEye <> LEFT_EYE_START_MARKER then
      LeftEye:= LeftEye + ' ';
    
    LeftEye:= LeftEye + 'P= ';
  end;
  
  //
  if (FN_PH <> '') then
  begin
    // IMPORTANT: Check here array length because of different formatting options for line "*PH"
    if Length(arrFN_PH) = 2 then
    begin
      S1:= Trim(arrFN_PH[0]);
      
      if T2WStartsStr(PRISM_HORIZONTAL_IDENTIFIER_IN,
                      S1) then
      begin
        S1:= Copy(S1, Length(PRISM_HORIZONTAL_IDENTIFIER_IN) + 1, Length(S1));
        
        S1:= S1 + ' ' + PRISM_HORIZONTAL_VALUE_IN;
      end;
      
      if T2WStartsStr(PRISM_HORIZONTAL_IDENTIFIER_OUT,
                      S1) then
      begin
        S1:= Copy(S1, Length(PRISM_HORIZONTAL_IDENTIFIER_OUT) + 1, Length(S1));
        
        S1:= S1 + ' ' + PRISM_HORIZONTAL_VALUE_OUT;
      end;
      
      LeftEye:= LeftEye + S1;
    end
    else if Length(arrFN_PH) = 4 then
    begin
      LeftEye:= LeftEye + Trim(arrFN_PH[1]) + ' ' + Trim(arrFN_PH[0]);
    end;
  end;
  
  //
  if (FN_PV <> '') then
  begin
    if FN_PH <> '' then
      LeftEye:= LeftEye + ' ';
    
    // IMPORTANT: Check here array length because of different formatting options for line "*PV"
    if Length(arrFN_PV) = 2 then
    begin
      S1:= Trim(arrFN_PV[0]);
      
      if T2WStartsStr(PRISM_VERTICAL_IDENTIFIER_UP,
                      S1) then
      begin
        S1:= Copy(S1, Length(PRISM_VERTICAL_IDENTIFIER_UP) + 1, Length(S1));
        
        S1:= S1 + ' ' + PRISM_VERTICAL_VALUE_UP;
      end;
      
      if T2WStartsStr(PRISM_VERTICAL_IDENTIFIER_DOWN,
                      S1) then
      begin
        S1:= Copy(S1, Length(PRISM_VERTICAL_IDENTIFIER_DOWN) + 1, Length(S1));
        
        S1:= S1 + ' ' + PRISM_VERTICAL_VALUE_DOWN;
      end;
        
      LeftEye:= LeftEye + S1;
    end
    else if Length(arrFN_PV) = 4 then
    begin
      LeftEye:= LeftEye + Trim(arrFN_PV[1]) + ' ' + Trim(arrFN_PV[0]);
    end;
  end;
  
  //
  if (PD <> '') then
  begin
    if LeftEye <> LEFT_EYE_START_MARKER then
      LeftEye:= LeftEye + ' ';
    
    LeftEye:= LeftEye + 'PD= ' + Trim(arrPD[0]);
  end;
   
  // 
  if (FN_AD <> '') then
  begin
    if LeftEye <> LEFT_EYE_START_MARKER then
      LeftEye:= LeftEye + ' ';
    
    LeftEye:= LeftEye + 'A= ' + Trim(arrFN_AD[0]);
  end;
  
  // -- END FN --
         
  // Add right eye to final output
  ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA_01 + FOutputLineSeparator + RightEye + FOutputLineSeparator;
  
  // Add left eye to final output
  ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA_01 + FOutputLineSeparator + LeftEye;
  
  // Add WD if it exists
  if (WD <> '') and (bolAddWDValueToOutput) then
  begin
    // Append WD index with GDT comment field id to measure data
    ParsedData := ParsedData + FOutputLineSeparator + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_COMMENT_01 + FOutputLineSeparator + 'WD= ' + Trim(WD);
  end;
  
  // Set result
  FParsedDataString := RawByteString(ParsedData);
end.