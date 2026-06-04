const
  VERSION = '1.0.21.76';
  DATE = '21.11.2022 08:56:21';
  TEXT = 'Copyright (c) 2022 team2work GmbH';
 
  DATA_SEPARATOR_SOH = #$01; // SOH (Start-of-Header)
  DATA_SEPARATOR_STX = #$02; // STX (Start-Text)
  DATA_SEPARATOR_ETX = #$03; // ETX (End-Text)
  DATA_SEPARATOR_ETB = #$17; // ETB (End-of-Transmission-Block) 
  DATA_SEPARATOR_EOT = #$04; // EOT (End-of-Transmission)  
    
  DATA_SEPARATOR_STAR_CHAR = #$2A; // Star Char (*)
  
  DATA_LINE_SEPARATOR_01 = #$0D#$0A;
  DATA_LINE_SEPARATOR_02 = #$0D;

  RIGHT_EYE_START_MARKER = 'R.:';
  LEFT_EYE_START_MARKER = 'L.:';
  RIGHT_EYE_START_MARKER_ARK_F = 'F R.:';
  LEFT_EYE_START_MARKER_ARK_F = 'F L.:';
  RIGHT_EYE_START_MARKER_ARK_N = 'N R.:';
  LEFT_EYE_START_MARKER_ARK_N = 'N L.:';
  RIGHT_EYE_START_MARKER_RKT_F = 'F R.:';
  LEFT_EYE_START_MARKER_RKT_F = 'F L.:';
  RIGHT_EYE_START_MARKER_RKT_N = 'N R.:';
  LEFT_EYE_START_MARKER_RKT_N = 'N L.:';
  
  SPHERE_IDENTIFIER = 'S=';
  CYLINDER_IDENTIFIER = 'Z=';
  AXIS_IDENTIFIER = '*';
  A_IDENTIFIER = 'A=';
  A2_IDENTIFIER = 'A2=';
  P_IDENTIFIER = 'P=';
  PD_IDENTIFIER = 'PD=';
  VD_IDENTIFIER = 'VD=';
  WD_IDENTIFIER = 'WD=';
  
  // Diese Werte müssen übereinstimmen mit den Werten
  // aus der Device INI Datei.
  NonGDTDataLineLensmeter = 'V0';
  NonGDTDataLineRefraktometerObjektiv = 'V1';
  NonGDTDataLinePhoropter = 'V2';
  NonGDTDataLineVerordnung = 'V3';
  NonGDTDataLineRefraktometerSubjektiv = 'V4';
  NonGDTDataLineVisusKorrektur = 'V5';
  NonGDTDataLineSondereintraege = 'V6';
  NonGDTDataLineKeratometer = 'V7';
  NonGDTDataLineVisus = 'V';
  
  NumericArrayValues = #32#43#45#46#48#49#50#51#52#53#54#55#56#57;

function FormatNumberValue(const S: String): String;
var
  S1, S2, S3: String;
  Sep1Pos: Integer;
begin
  Result:= S;
  
  if S = '' then
    Exit;
  
  S1:= Trim(S);
  
  if (S1[1] = '-') or (S1[1] = '+') then
  begin
    S2:= S1[1];
    S3:= Trim(Copy(S1, 2, Length(S1)));
    
    Sep1Pos:= Pos('.', S3);
    
    if Sep1Pos = 2 then
      S2:= S2 + ' ';
    
    Result:= S2 + S3;
    
    if (Result = '- 0.00') then
    begin
      Result:= '+ 0.00';
    end;       
    
    Exit;
  end;
  
  Result:= '+';
  
  Sep1Pos:= Pos('.', S1);
  
  if Sep1Pos = 2 then
    Result:= Result + ' ';
  
  Result:= Result + S1;
  
  if (Result = '- 0.00') then
  begin
    Result:= '+ 0.00';
  end;
end;

function FormatAxisValue(const S: String): String;
begin
  Result:= Trim(S);
  
  if (Result = '') or (Result = '000') then
  begin
    Result:= '  0';
    Exit;
  end;
    
  while Length(Result) < 3 do
    Result:= ' ' + Result;
    
  if (Result = ' 00') then
  begin
    Result:= '  0';
    Exit;
  end;  
end;

function FormatPDValue(const S: String): String;
begin
  Result:= Trim(S);
  
  if (Result = '') or (Result = '000') then
  begin
    Result:= ' 0';
    Exit;
  end;
    
  while Length(Result) < 2 do
    Result:= ' ' + Result;
end;

function FindNextIdentifierLineIndex(const S: String;
                                     StartIndex: Integer;
                                     const NumericArrayValues: String): Integer;
var
  i, j: Integer;
  Found: Boolean;
begin
  Result:= -1;

  if S = '' then
    Exit;

  for i:= StartIndex to Length(S) do
  begin
    Found:= False;

    for j:= 1 to Length(NumericArrayValues) do
    begin
      if S[i] <> NumericArrayValues[j] then
        Continue;

      Found:= True;

      Break;
    end;

    if Found then
      Continue;

    Result:= i - StartIndex;
    Break;
  end;

  if Result = -1 then
    Result:= Length(S);
end;

function GetDataForIdentifier(const S: String;
                              const Identifier: String;
                              const NumericArrayValues: String;
                              var Data: String): String;
var
  DataStartIndex, DataEndIndex: Integer;
begin
  Result:= S;
  Data:= '';

  if not T2WContainsStr(S, Identifier) then
    Exit;

  // Get data start index
  DataStartIndex:= Pos(Identifier, S);

  // Find next identifier line index
  DataEndIndex:= FindNextIdentifierLineIndex(S,
                                             DataStartIndex + Length(Identifier),
                                             NumericArrayValues);

  if DataEndIndex <= 0 then
  begin
    Data:= Result;
    Exit;
  end;

  Data:= Copy(S, DataStartIndex + Length(Identifier), DataEndIndex);

  Result:= Copy(S, 1, DataStartIndex - 1) + Copy(S, DataEndIndex + Length(Identifier) + 1, Length(S));
end;

function GetPDetailValue(const S: String; 
                         const P: String; 
                         const V1: String; 
                         const V2: String): String;
var
  S1: String;
begin
  Result:= '';

  if S = '' then
    Exit;

  if P = '' then
    Exit;

  if T2WStartsStr(V1, S) then
  begin
    Result:= Trim(P) + V1;
    Exit;
  end;

  if T2WStartsStr(V2, S) then
  begin
    Result:= Trim(P) + V2;
    Exit;
  end;

  S1:= Copy(S, 2, Length(S));

  if T2WContainsStr(S1, V1) then
    Result:= Trim(Copy(S1, 1, Pos(V1, S1) - 1)) + V1
  else if T2WContainsStr(S1, V2) then
    Result:= Trim(Copy(S1, 1, Pos(V2, S1) - 1)) + V2;
end;

var
  GDTDataToSend: TStringArray;
  DataToSend, S, COMReceiveString: String;
  i: Integer;
  PD, WD: String;
  ID_BLOCK, AR_BLOCK, LM_BLOCK, LM_ADD_BLOCK, LM_PRISM_BLOCK: String;
  LM_R_S, LM_R_Z, LM_R_AX, LM_R_A, LM_R_A2, LM_R_PH, LM_R_PV, LM_R_P, LM_R_PD: String;
  LM_L_S, LM_L_Z, LM_L_AX, LM_L_A, LM_L_A2, LM_L_PH, LM_L_PV, LM_L_P, LM_L_PD: String;
  AR_R_S, AR_R_Z, AR_R_AX, AR_R_A, AR_R_A2, AR_R_PH, AR_R_PV, AR_R_P, AR_R_PD, AR_R_VD: String;
  AR_L_S, AR_L_Z, AR_L_AX, AR_L_A, AR_L_A2, AR_L_PH, AR_L_PV, AR_L_P: String;
  FN_R_S, FN_R_Z, FN_R_AX, FN_R_A, FN_R_A2, FN_R_PH, FN_R_PV, FN_R_P, FN_R_PD, FN_R_VD: String;
  FN_L_S, FN_L_Z, FN_L_AX, FN_L_A, FN_L_A2, FN_L_PH, FN_L_PV, FN_L_P, FN_L_PD: String; 
  bolRemoveLeadingLineWhiteSpace, bolIdParameterExtended, bolSendIdBlock: Boolean;
begin
  // Verwende "True", damit führende Leerzeichen aus jeder Datenzeile entfernt werden, 
  // benutze "False", damit führende Leerzeichen in jeder Datenzeile erhalten bleiben.
  bolRemoveLeadingLineWhiteSpace:= True;
  
  // Verwende "True", damit der Extended ID Block verwendet wird. 
  // Der ID Block besteht bei Extended aus 20 Zeichen und bei Standard aus 12 Zeichen.
  // Die Einstellung ob Standard oder Extended kann im NIDEK RT-2100 Einstellungsmenü
  // getätigt werden. D.h. wenn Standard dann "False", wenn Extended dann "True".
  bolIdParameterExtended:= False;
  
  // Verwende "True", damit der ID Block verwendet wird, benutze "False", damit
  // der ID Block nicht an das Gerät übertragen wird. 
  bolSendIdBlock:= False;
    
  // --- Don't edit script down below ---
  
  if not FDisableSendingOfData then
  begin
    SetLength(GDTDataToSend, 0);
    
    DataToSend:= '';
    
    // The computer requires the RT to transmit the data.
    // The command is as follows.
    // [SH] C** [SX] RS [EB] [ET]
    DataToSend:= DATA_SEPARATOR_SOH + 
                 'C' + 
                 DATA_SEPARATOR_STAR_CHAR + 
                 DATA_SEPARATOR_STAR_CHAR + 
                 DATA_SEPARATOR_STX + 
                 'RS' + 
                 DATA_SEPARATOR_ETB + 
                 DATA_SEPARATOR_EOT;
    
    if (DataToSend <> '') then
    begin
      DoCOMPSSendData(DataToSend);
    end;
    
    // The RT acknowloedges the data transmission from the PC.
    // The command is as follows.
    // [SH] CRL [SX] SD [EB] [ET]
    DoCOMPSReceiveData();

    if (FGlobalTimeoutReached) then
    begin
      if Length(FCOMBuffer) <= 0 then
      begin
        FLastCOMErrorCode:= -4;
        FLastCOMErrorMessage:= 'Keine Daten für die Verarbeitung der Daten verfügbar';

        DoCOMPSError;      
              
        Exit;
      end;
    end;

    if (FErrorOccurred) then
    begin
      Exit;
    end;
    
    // check buffer
    // [SH] CRL [SX] SD [EB] [ET]
    COMReceiveString:= DATA_SEPARATOR_SOH +
                       'CRL' + 
                       DATA_SEPARATOR_STX +
                       'SD' +
                       DATA_SEPARATOR_ETB +
                       DATA_SEPARATOR_EOT
                       ;

    if (COMReceiveString = FCOMBufferString) then
    begin
      // T2WMessageBoxS(FCOMBufferString);
    end
    else
    begin
      COMReceiveString:= COMReceiveString + 
                         DATA_LINE_SEPARATOR_01
                         ; 

      if (COMReceiveString = FCOMBufferString) then
      begin
        // T2WMessageBoxS(FCOMBufferString);
      end
      else
      begin
        //FLastCOMErrorCode:= -5;
        //FLastCOMErrorMessage:= 'Keine passende Rückmeldung für die Verarbeitung der Daten erhalten';

        //DoCOMPSError;    
    
        //Exit;
      end;
    end;
    
    // The PC transmits the data to the RT according to the transmission data format.
    
    // 0x1 DRM 0x2 OR+ 3.00+ 3.00180 0x17 OL+ 6.00+ 7.00 90 0x17 PD12 0x17 0x4
    
    // [SH] [ID No. block] [EB] [AR SCA block] [EB] [AR PD block] [EB] 
    // [LM SCA block] [EB] [LM ADD block] [EB] [LM PRISM block] [EB]
    // [LM PD block] [EB] [ET]    
     
    // 
    DataToSend:= '';    
    
    // 
    ID_BLOCK:= '';
    AR_BLOCK:= '';
    LM_BLOCK:= '';
    LM_ADD_BLOCK:= '';
    LM_PRISM_BLOCK:= '';

    //
    PD:= '';
    WD:= '';
    
    //
    LM_R_S:= '';
    LM_R_Z:= '';
    LM_R_AX:= '';
    LM_R_A:= '';
    LM_R_A2:= '';
    LM_R_PH:= '';
    LM_R_PV:= '';
    LM_R_P:= '';
    LM_R_PD:= '';
    
    // 
    LM_L_S:= '';
    LM_L_Z:= '';
    LM_L_AX:= '';
    LM_L_A:= '';
    LM_L_A2:= '';
    LM_L_PH:= '';
    LM_L_PV:= '';
    LM_L_P:= '';
    LM_L_PD:= '';
    
    //
    AR_R_S:= '';
    AR_R_Z:= '';
    AR_R_AX:= '';
    AR_R_A:= '';
    AR_R_A2:= '';
    AR_R_PH:= '';
    AR_R_PV:= '';
    AR_R_P:= '';
    AR_R_PD:= '';
    AR_R_VD:= '';
    
    // 
    AR_L_S:= '';
    AR_L_Z:= '';
    AR_L_AX:= '';
    AR_L_A:= '';
    AR_L_A2:= '';
    AR_L_PH:= '';
    AR_L_PV:= '';
    AR_L_P:= '';
    
    //
    FN_R_S:= '';
    FN_R_Z:= '';
    FN_R_AX:= '';
    FN_R_A:= '';
    FN_R_A2:= '';
    FN_R_PH:= '';
    FN_R_PV:= '';
    FN_R_P:= '';
    FN_R_PD:= '';
    FN_R_VD:= '';  
    
    // 
    FN_L_S:= '';
    FN_L_Z:= '';
    FN_L_AX:= '';
    FN_L_A:= '';
    FN_L_A2:= '';
    FN_L_PH:= '';
    FN_L_PV:= '';
    FN_L_P:= '';
    FN_L_PD:= '';
    
    // DATA FORMAT (THE PC TO THE RT)
    // The following data format is used to transmit all data.
      
    // [SH] [ID No. block] [EB] [AR SCA block] [EB] [AR PD block] [EB]
    // [LM SCA block] [EB] [LM ADD block] [EB] [LM PRISM block] [EB]
    // [LM PD block] [EB] [ET]
      
    // The block which has no data is simply removed when the data is transmitted in
    // the above format.  
      
    // Get "Lensmeter" data lines
    DoCOMPSGetNonGDTDataForGroup(NonGDTDataLineLensmeter,
                                 GDTDataToSend);
    
    if Length(GDTDataToSend) > 0 then
    begin
      for i:= 0 to Length(GDTDataToSend) - 1 do
      begin
        S:= GDTDataToSend[i];
        
        // Remove leading white space
        if bolRemoveLeadingLineWhiteSpace then
          S:= TrimLeft(S);
        
        if T2WStartsStr(RIGHT_EYE_START_MARKER, S) then
        begin
          // Remove eye identifier
          S:= Copy(S, Length(RIGHT_EYE_START_MARKER) + 1, Length(S));
          
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   LM_R_S);
          
          // Get cylinder data
          S:= GetDataForIdentifier(S,
                                   CYLINDER_IDENTIFIER,
                                   NumericArrayValues,
                                   LM_R_Z);
          
          // Get axis data
          S:= GetDataForIdentifier(S,
                                   AXIS_IDENTIFIER,
                                   NumericArrayValues,
                                   LM_R_AX);
          
          // Get A data
          S:= GetDataForIdentifier(S,
                                   A_IDENTIFIER,
                                   NumericArrayValues,
                                   LM_R_A);
          
          // Get A2 data
          S:= GetDataForIdentifier(S,
                                   A2_IDENTIFIER,
                                   NumericArrayValues,
                                   LM_R_A2);
                                   
          // Get PD data
          S:= GetDataForIdentifier(S,
                                   PD_IDENTIFIER,
                                   NumericArrayValues,
                                   LM_R_PD);                                    
          
          // Der "P"-Wert muss aktuell der letzte verarbeitete Wert
          // pro Zeile sein, da er komplexer formatiert ist und 
          // zusätzliche Zeichen enthält, die nicht zur 
          // Zahlendarstellung gehören
          
          // Get P data
          S:= GetDataForIdentifier(S,
                                   P_IDENTIFIER,
                                   NumericArrayValues,
                                   LM_R_P);
          
          // Get P details
          LM_R_PH:= GetPDetailValue(S, LM_R_P, 'I', 'O');
          LM_R_PV:= GetPDetailValue(S, LM_R_P, 'U', 'D');
        end
        else if T2WStartsStr(LEFT_EYE_START_MARKER, S) then
        begin
          // Remove eye identifier
          S:= Copy(S, Length(LEFT_EYE_START_MARKER) + 1, Length(S));
          
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   LM_L_S);
          
          // Get cylinder data
          S:= GetDataForIdentifier(S,
                                   CYLINDER_IDENTIFIER,
                                   NumericArrayValues,
                                   LM_L_Z);
          
          // Get axis data
          S:= GetDataForIdentifier(S,
                                   AXIS_IDENTIFIER,
                                   NumericArrayValues,
                                   LM_L_AX);
          
          // Get A data
          S:= GetDataForIdentifier(S,
                                   A_IDENTIFIER,
                                   NumericArrayValues,
                                   LM_L_A);
          
          // Get A2 data
          S:= GetDataForIdentifier(S,
                                   A2_IDENTIFIER,
                                   NumericArrayValues,
                                   LM_L_A2);
                                   
          // Get PD data
          S:= GetDataForIdentifier(S,
                                   PD_IDENTIFIER,
                                   NumericArrayValues,
                                   LM_L_PD);                                 
          
          // Der "P"-Wert muss aktuell der letzte verarbeitete Wert
          // pro Zeile sein, da er komplexer formatiert ist und 
          // zusätzliche Zeichen enthält, die nicht zur 
          // Zahlendarstellung gehören
          
          // Get P data
          S:= GetDataForIdentifier(S,
                                   P_IDENTIFIER,
                                   NumericArrayValues,
                                   LM_L_P);
          
          // Get P details
          LM_L_PH:= GetPDetailValue(S, LM_L_P, 'I', 'O');
          LM_L_PV:= GetPDetailValue(S, LM_L_P, 'U', 'D');
        end;
      end;
      
      // Block data of LM SCA
      LM_BLOCK:= '';
      LM_ADD_BLOCK:= '';
      LM_PRISM_BLOCK:= '';
      
      if (LM_R_S <> '') and (LM_R_Z <> '') and (LM_R_AX <> '') then
      begin
       LM_BLOCK:= 'DLM' +
                  DATA_SEPARATOR_STX
                  ;
             
       LM_BLOCK:= LM_BLOCK +
                  ' R' +      
                  FormatNumberValue(LM_R_S) + FormatNumberValue(LM_R_Z) + FormatAxisValue(LM_R_AX) +
                  DATA_SEPARATOR_ETB
                  ;
      end;
      
      if (LM_L_S <> '') and (LM_L_Z <> '') and (LM_L_AX <> '') then
      begin
        if (LM_BLOCK = '') then
        begin
          LM_BLOCK:= 'DLM' +
                     DATA_SEPARATOR_STX
                     ;
        end;
      
        LM_BLOCK:= LM_BLOCK +
                   ' L' +  
                   FormatNumberValue(LM_L_S) + FormatNumberValue(LM_L_Z) + FormatAxisValue(LM_L_AX) +
                   DATA_SEPARATOR_ETB
                   ;
      end;
       
      // Block data of LM ADD
      if (LM_R_A <> '') or (LM_L_A <> '') then
      begin
        if (LM_BLOCK = '') then
        begin
          LM_BLOCK:= 'DLM' +
                     DATA_SEPARATOR_STX
                     ;
        end;
      
        if (LM_R_A <> '') then
        begin
          LM_ADD_BLOCK:= LM_ADD_BLOCK +
                         'AR' +
                         FormatNumberValue(LM_R_A) +
                         DATA_SEPARATOR_ETB
                         ;
        end;
        
        if (LM_L_A <> '') then
        begin
          LM_ADD_BLOCK:= LM_ADD_BLOCK +
                         'AL' +
                         FormatNumberValue(LM_L_A) +
                         DATA_SEPARATOR_ETB
                         ;
        end;
      end;
          
      // Block data of LM PRISM
      if (LM_R_PH <> '') and (LM_R_PV <> '') then
      begin
        if (LM_BLOCK = '') then
        begin
          LM_BLOCK:= 'DLM' +
                     DATA_SEPARATOR_STX
                     ;
        end;
        
        LM_PRISM_BLOCK:= 'PR' +
                         LM_R_PH +  
                         DATA_SEPARATOR_ETB +
                         'PR' +
                         LM_R_PV +
                         DATA_SEPARATOR_ETB
                         ;  
      end;

      if (LM_L_PH <> '') and (LM_L_PV <> '') then
      begin
        if (LM_BLOCK = '') then
        begin
          LM_BLOCK:= 'DLM' +
                     DATA_SEPARATOR_STX
                     ;
        end;
       
        LM_PRISM_BLOCK:= LM_PRISM_BLOCK +
                         'PL' +
                         LM_L_PH +
                         DATA_SEPARATOR_ETB + 
                         'PL' +
                         LM_L_PV +
                         DATA_SEPARATOR_ETB
                         ;
      end;
    end;

    // Get "Refraktometer / Objektiv" data lines
    DoCOMPSGetNonGDTDataForGroup(NonGDTDataLineRefraktometerObjektiv,
                                 GDTDataToSend);
    
    if Length(GDTDataToSend) > 0 then
    begin
      for i:= 0 to Length(GDTDataToSend) - 1 do
      begin
        S:= GDTDataToSend[i];
        
        // Remove leading white space
        if bolRemoveLeadingLineWhiteSpace then
          S:= TrimLeft(S);
        
        if (T2WStartsStr(RIGHT_EYE_START_MARKER, S))
        or (T2WStartsStr(RIGHT_EYE_START_MARKER_ARK_F, S))
        or (T2WStartsStr(RIGHT_EYE_START_MARKER_ARK_N, S))
        then
        begin
          AR_R_S:= '';
          AR_R_Z:= '';
          AR_R_AX:= '';
          AR_R_A:= '';
          AR_R_A2:= '';
          AR_R_PH:= '';
          AR_R_PV:= '';
          AR_R_P:= '';
          AR_R_PD:= '';
          AR_R_VD:= '';
          
          // Remove eye identifier
          if T2WStartsStr(RIGHT_EYE_START_MARKER, S) then
            S:= Copy(S, Length(RIGHT_EYE_START_MARKER) + 1, Length(S))
          else if T2WStartsStr(RIGHT_EYE_START_MARKER_ARK_F, S) then
            S:= Copy(S, Length(RIGHT_EYE_START_MARKER_ARK_F) + 1, Length(S))
          else if T2WStartsStr(RIGHT_EYE_START_MARKER_ARK_N, S) then
            S:= Copy(S, Length(RIGHT_EYE_START_MARKER_ARK_N) + 1, Length(S));
          
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   AR_R_S);
          
          // Get cylinder data
          S:= GetDataForIdentifier(S,
                                   CYLINDER_IDENTIFIER,
                                   NumericArrayValues,
                                   AR_R_Z);
          
          // Get axis data
          S:= GetDataForIdentifier(S,
                                   AXIS_IDENTIFIER,
                                   NumericArrayValues,
                                   AR_R_AX);
          
          // Get A data
          S:= GetDataForIdentifier(S,
                                   A_IDENTIFIER,
                                   NumericArrayValues,
                                   AR_R_A);
          
          // Get A2 data
          S:= GetDataForIdentifier(S,
                                   A2_IDENTIFIER,
                                   NumericArrayValues,
                                   AR_R_A2);
          
          // Get PD data
          S:= GetDataForIdentifier(S,
                                   PD_IDENTIFIER,
                                   NumericArrayValues,
                                   AR_R_PD);
          
          // Get VD data
          S:= GetDataForIdentifier(S,
                                   VD_IDENTIFIER,
                                   NumericArrayValues,
                                   AR_R_VD);
          
          // Der "P"-Wert muss aktuell der letzte verarbeitete Wert
          // pro Zeile sein, da er komplexer formatiert ist und 
          // zusätzliche Zeichen enthält, die nicht zur 
          // Zahlendarstellung gehören
          
          // Get P data
          S:= GetDataForIdentifier(S,
                                   P_IDENTIFIER,
                                   NumericArrayValues,
                                   AR_R_P);
          
          // Get P details
          AR_R_PH:= GetPDetailValue(S, AR_R_P, 'I', 'O');
          AR_R_PV:= GetPDetailValue(S, AR_R_P, 'U', 'D');
        end
        else if (T2WStartsStr(LEFT_EYE_START_MARKER, S))
        or (T2WStartsStr(LEFT_EYE_START_MARKER_ARK_F, S))
        or (T2WStartsStr(LEFT_EYE_START_MARKER_ARK_N, S))
        then
        begin
          AR_L_S:= '';
          AR_L_Z:= '';
          AR_L_AX:= '';
          AR_L_A:= '';
          AR_L_A2:= '';
          AR_L_PH:= '';
          AR_L_PV:= '';
          AR_L_P:= '';
          
          // Remove eye identifier
          if T2WStartsStr(LEFT_EYE_START_MARKER, S) then
            S:= Copy(S, Length(LEFT_EYE_START_MARKER) + 1, Length(S))
          else if T2WStartsStr(LEFT_EYE_START_MARKER_ARK_F, S) then
            S:= Copy(S, Length(LEFT_EYE_START_MARKER_ARK_F) + 1, Length(S))
          else if T2WStartsStr(LEFT_EYE_START_MARKER_ARK_N, S) then
            S:= Copy(S, Length(LEFT_EYE_START_MARKER_ARK_N) + 1, Length(S));
          
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   AR_L_S);
          
          // Get cylinder data
          S:= GetDataForIdentifier(S,
                                   CYLINDER_IDENTIFIER,
                                   NumericArrayValues,
                                   AR_L_Z);
          
          // Get axis data
          S:= GetDataForIdentifier(S,
                                   AXIS_IDENTIFIER,
                                   NumericArrayValues,
                                   AR_L_AX);
          
          // Get A data
          S:= GetDataForIdentifier(S,
                                   A_IDENTIFIER,
                                   NumericArrayValues,
                                   AR_L_A);
          
          // Get A2 data
          S:= GetDataForIdentifier(S,
                                   A2_IDENTIFIER,
                                   NumericArrayValues,
                                   AR_L_A2);
          
          // Der "P"-Wert muss aktuell der letzte verarbeitete Wert
          // pro Zeile sein, da er komplexer formatiert ist und 
          // zusätzliche Zeichen enthält, die nicht zur 
          // Zahlendarstellung gehören
          
          // Get P data
          S:= GetDataForIdentifier(S,
                                   P_IDENTIFIER,
                                   NumericArrayValues,
                                   AR_L_P);
          
          // Get P details
          AR_L_PH:= GetPDetailValue(S, AR_L_P, 'I', 'O');
          AR_L_PV:= GetPDetailValue(S, AR_L_P, 'U', 'D');
        end;
      end;
      
      // Block data of AR SCA 
      AR_BLOCK:= '';
             
      if (AR_R_S <> '') and (AR_R_Z <> '') and (AR_R_AX <> '') then
      begin
        AR_BLOCK:= 'DRM' +
                   DATA_SEPARATOR_STX
                   ;      
        
        AR_BLOCK:= AR_BLOCK + 
                   'OR' +
                   FormatNumberValue(AR_R_S) + FormatNumberValue(AR_R_Z) + FormatAxisValue(AR_R_AX) +
                   DATA_SEPARATOR_ETB
                   ;    
      end;          
      
      if (AR_L_S <> '') and (AR_L_Z <> '') and (AR_L_AX <> '') then
      begin
        if (AR_BLOCK = '') then
        begin
          AR_BLOCK:= 'DRM' +
                     DATA_SEPARATOR_STX
                     ;
        end;
            
        AR_BLOCK:= AR_BLOCK + 
                   'OL' +  
                   FormatNumberValue(AR_L_S) + FormatNumberValue(AR_L_Z) + FormatAxisValue(AR_L_AX) +
                   DATA_SEPARATOR_ETB
                   ;   
      end;          
               
      if (AR_R_A <> '') or (AR_L_A <> '') then
      begin
        // do nothing
      end;           
                 
      if (AR_R_PH <> '') or (AR_L_PH <> '') then 
      begin
        // do nothing
      end;
      
      if (AR_R_PV <> '') or (AR_L_PV <> '') then                 
      begin
        // do nothing
      end;
      
      if (AR_R_PD <> '') then                
      begin
        if (AR_BLOCK = '') then
        begin
          AR_BLOCK:= 'DRM' +
                     DATA_SEPARATOR_STX
                     ;
        end;      
        
        AR_BLOCK:= AR_BLOCK + 
                   'PD' +
                   FormatPDValue(Trim(AR_R_PD)) +
                   DATA_SEPARATOR_ETB
                   ;
      end;           
    end;
    
    // Get "Refraktometer / Subjektiv" data lines
    DoCOMPSGetNonGDTDataForGroup(NonGDTDataLineRefraktometerSubjektiv,
                                 GDTDataToSend);
    
    if Length(GDTDataToSend) > 0 then
    begin
      for i:= 0 to Length(GDTDataToSend) - 1 do
      begin
        S:= GDTDataToSend[i];
        
        // TODO:
      end;
    end;
    
    // Get "Phoropter" data lines
    DoCOMPSGetNonGDTDataForGroup(NonGDTDataLinePhoropter,
                                 GDTDataToSend);
    
    if Length(GDTDataToSend) > 0 then
    begin
      for i:= 0 to Length(GDTDataToSend) - 1 do
      begin
        S:= GDTDataToSend[i];
        
        // Remove leading white space
        if bolRemoveLeadingLineWhiteSpace then
          S:= TrimLeft(S);
        
        if (T2WStartsStr(RIGHT_EYE_START_MARKER, S))
        or (T2WStartsStr(RIGHT_EYE_START_MARKER_RKT_F, S))
        or (T2WStartsStr(RIGHT_EYE_START_MARKER_RKT_N, S))
        then
        begin
          FN_R_S:= '';
          FN_R_Z:= '';
          FN_R_AX:= '';
          FN_R_A:= '';
          FN_R_A2:= '';
          FN_R_PH:= '';
          FN_R_PV:= '';
          FN_R_P:= '';
          FN_R_PD:= '';
          FN_R_VD:= '';        
          
          // Remove eye identifier
          if T2WStartsStr(RIGHT_EYE_START_MARKER, S) then
            S:= Copy(S, Length(RIGHT_EYE_START_MARKER) + 1, Length(S))
          else if T2WStartsStr(RIGHT_EYE_START_MARKER_RKT_F, S) then
            S:= Copy(S, Length(RIGHT_EYE_START_MARKER_RKT_F) + 1, Length(S))
          else if T2WStartsStr(RIGHT_EYE_START_MARKER_RKT_N, S) then
            S:= Copy(S, Length(RIGHT_EYE_START_MARKER_RKT_N) + 1, Length(S));
          
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   FN_R_S);
          
          // Get cylinder data
          S:= GetDataForIdentifier(S,
                                   CYLINDER_IDENTIFIER,
                                   NumericArrayValues,
                                   FN_R_Z);
          
          // Get axis data
          S:= GetDataForIdentifier(S,
                                   AXIS_IDENTIFIER,
                                   NumericArrayValues,
                                   FN_R_AX);
          
          // Get A data
          S:= GetDataForIdentifier(S,
                                   A_IDENTIFIER,
                                   NumericArrayValues,
                                   FN_R_A);
          
          // Get A2 data
          S:= GetDataForIdentifier(S,
                                   A2_IDENTIFIER,
                                   NumericArrayValues,
                                   FN_R_A2);
          
          // Get PD data
          S:= GetDataForIdentifier(S,
                                   PD_IDENTIFIER,
                                   NumericArrayValues,
                                   FN_R_PD);
          
          // Get VD data
          S:= GetDataForIdentifier(S,
                                   VD_IDENTIFIER,
                                   NumericArrayValues,
                                   FN_R_VD);
                       
          // Der "P"-Wert muss aktuell der letzte verarbeitete Wert
          // pro Zeile sein, da er komplexer formatiert ist und 
          // zusätzliche Zeichen enthält, die nicht zur 
          // Zahlendarstellung gehören
          
          // Get P data
          S:= GetDataForIdentifier(S,
                                   P_IDENTIFIER,
                                   NumericArrayValues,
                                   FN_R_P);
          
          // Get P details
          FN_R_PH:= GetPDetailValue(S, FN_R_P, 'I', 'O');
          FN_R_PV:= GetPDetailValue(S, FN_R_P, 'U', 'D');
        end
        else if (T2WStartsStr(LEFT_EYE_START_MARKER, S))
        or (T2WStartsStr(LEFT_EYE_START_MARKER_RKT_F, S))
        or (T2WStartsStr(LEFT_EYE_START_MARKER_RKT_N, S))
        then
        begin
          FN_L_S:= '';
          FN_L_Z:= '';
          FN_L_AX:= '';
          FN_L_A:= '';
          FN_L_A2:= '';
          FN_L_PH:= '';
          FN_L_PV:= '';
          FN_L_P:= '';
          FN_L_PD:= '';
          
          // Remove eye identifier
          if T2WStartsStr(RIGHT_EYE_START_MARKER, S) then
            S:= Copy(S, Length(LEFT_EYE_START_MARKER) + 1, Length(S))
          else if T2WStartsStr(LEFT_EYE_START_MARKER_RKT_F, S) then
            S:= Copy(S, Length(LEFT_EYE_START_MARKER_RKT_F) + 1, Length(S))
          else if T2WStartsStr(LEFT_EYE_START_MARKER_RKT_N, S) then
            S:= Copy(S, Length(LEFT_EYE_START_MARKER_RKT_N) + 1, Length(S));
          
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   FN_L_S);
          
          // Get cylinder data
          S:= GetDataForIdentifier(S,
                                   CYLINDER_IDENTIFIER,
                                   NumericArrayValues,
                                   FN_L_Z);
          
          // Get axis data
          S:= GetDataForIdentifier(S,
                                   AXIS_IDENTIFIER,
                                   NumericArrayValues,
                                   FN_L_AX);
          
          // Get A data
          S:= GetDataForIdentifier(S,
                                   A_IDENTIFIER,
                                   NumericArrayValues,
                                   FN_L_A);
          
          // Get A2 data
          S:= GetDataForIdentifier(S,
                                   A2_IDENTIFIER,
                                   NumericArrayValues,
                                   FN_L_A2);
          
          // Get PD data
          S:= GetDataForIdentifier(S,
                                   PD_IDENTIFIER,
                                   NumericArrayValues,
                                   FN_L_PD);
          
          // Der "P"-Wert muss aktuell der letzte verarbeitete Wert
          // pro Zeile sein, da er komplexer formatiert ist und 
          // zusätzliche Zeichen enthält, die nicht zur 
          // Zahlendarstellung gehören
          
          // Get P data
          S:= GetDataForIdentifier(S,
                                   P_IDENTIFIER,
                                   NumericArrayValues,
                                   FN_L_P);
          
          // Get P details
          FN_L_PH:= GetPDetailValue(S, FN_L_P, 'I', 'O');
          FN_L_PV:= GetPDetailValue(S, FN_L_P, 'U', 'D');
        end;
      end;
      
      // TODO:

    end;

    // --------------------------------------------------------

    // Format of Each Block 
    DataToSend:= '';  
     
    if (bolSendIdBlock) then
    begin    
      // Block data of ID No
      ID_BLOCK:= 'DRL' +
                 DATA_SEPARATOR_STX +
                 'ID' +
                 '            '
                 ;
    
      if (bolIdParameterExtended) then
        ID_BLOCK:= ID_BLOCK + 
                   '        '
                   ;
    
      ID_BLOCK:= ID_BLOCK +
                 DATA_SEPARATOR_ETB
                 ; 
    end;    
    
    // The following data format is used to transmit all data
    // [SH] 
    DataToSend:= DATA_SEPARATOR_SOH
                 ;
    
    // Block data of ID No.
    if (ID_BLOCK <> '') then
    begin
      DataToSend:= DataToSend + 
                   ID_BLOCK
                   ;     
    end;
    
    // Choose here which block should be transmitted to the RT
    
    // Block data of AR SCA
    if (AR_BLOCK <> '') then
    begin
      DataToSend:= DataToSend + 
                   AR_BLOCK
                   ;  
    end;    
        
    if (LM_BLOCK <> '') then 
    begin
      DataToSend:= DataToSend + 
                   LM_BLOCK
                   ;     
    end; 
   
    if (LM_ADD_BLOCK <> '') then
    begin
      DataToSend:= DataToSend + 
                   LM_ADD_BLOCK
                   ;     
    end;    
      
    if (LM_PRISM_BLOCK <> '') then
    begin
      DataToSend:= DataToSend + 
                   LM_PRISM_BLOCK
                   ;     
    end;   

    // [ET]
    DataToSend:= DataToSend + 
                 DATA_SEPARATOR_EOT
                 ;
    
    // T2WMessageBoxS(DataToSend);
    
    if (DataToSend <> '') then
    begin           
      DoCOMPSSendData(DataToSend);
    end;
  end;
  
  DoCOMPSReceiveData();

  // DoCOMPSSaveFile('test1.txt');

  if (FGlobalTimeoutReached) then
  begin
    if Length(FCOMBuffer) <= 0 then
      Exit;
  end;

  if (FErrorOccurred) then
  begin
    Exit;
  end;
end.