const
  VERSION = '1.0.55.79';
  DATE = '11.10.2024 13:04:43';
  TEXT = 'Copyright (c) 2024 team2work GmbH';

  DATA_ACK               = #$06;   // ACK
  DATA_NAK               = #$15;   // NAK
  DATA_STX               = #$02;   // STX (Start-Text)
  DATA_ETX               = #$03;   // ETX (End-Text)  
    
  DATA_ETX_RESULT        = $03;    // ETX
  
  DATA_LINE_SEPARATOR_01 = #$0D#$0A;
    
  SPHERE_IDENTIFIER      = 'S=';
  CYLINDER_IDENTIFIER    = 'Z=';
  AXIS_IDENTIFIER        = '*';
  A_IDENTIFIER           = 'A=';
  A2_IDENTIFIER          = 'A2=';
  P_IDENTIFIER           = 'P=';
  PD_IDENTIFIER          = 'PD=';
  VD_IDENTIFIER          = 'VD=';
  WD_IDENTIFIER          = 'WD=';
  
  RIGHT_EYE_START_MARKER               = 'R.:';
  LEFT_EYE_START_MARKER                = 'L.:';
  
  RIGHT_EYE_START_MARKER_ARK_F         = 'F R.:';
  LEFT_EYE_START_MARKER_ARK_F          = 'F L.:';
  RIGHT_EYE_START_MARKER_ARK_N         = 'N R.:';
  LEFT_EYE_START_MARKER_ARK_N          = 'N L.:';
  
  RIGHT_EYE_START_MARKER_RKT_F         = 'F R.:';
  LEFT_EYE_START_MARKER_RKT_F          = 'F L.:';
  RIGHT_EYE_START_MARKER_RKT_N         = 'N R.:';
  LEFT_EYE_START_MARKER_RKT_N          = 'N L.:';
  
  RIGHT_EYE_START_MARKER_LM_F          = 'F R.:';
  LEFT_EYE_START_MARKER_LM_F           = 'F L.:';
  RIGHT_EYE_START_MARKER_LM_N          = 'N R.:';
  LEFT_EYE_START_MARKER_LM_N           = 'N L.:';

  // Diese Werte müssen übereinstimmen mit den Werten
  // aus der Device INI Datei.
  NonGDTDataLineLensmeter              = 'V0';
  NonGDTDataLineRefraktometerObjektiv  = 'V1';
  NonGDTDataLinePhoropter              = 'V2';
  NonGDTDataLineVerordnung             = 'V3';
  NonGDTDataLineRefraktometerSubjektiv = 'V4';
  NonGDTDataLineVisusKorrektur         = 'V5';
  NonGDTDataLineSondereintraege        = 'V6';
  NonGDTDataLineKeratometer            = 'V7';
  NonGDTDataLineVisus                  = 'V';
  
  NumericArrayValues = #32#43#45#46#48#49#50#51#52#53#54#55#56#57; 
  
function FormatNumberValue(const S: String; const UseDefaultZeroValue: Boolean): String;
var
  S1, S2, S3: String;
  Sep1Pos: Integer;
begin
  Result:= S;
  
  if (S = '') and (not UseDefaultZeroValue) then
  begin
    Exit;
  end;
  
  S1:= Trim(S);
  
  if (S = '') and (UseDefaultZeroValue) then
  begin
    S1:= '0.00';    
  end;
   
  if (S1[1] = '-') or (S1[1] = '+') then
  begin
    S2:= S1[1];
    S3:= Trim(Copy(S1, 2, Length(S1)));
    
    Sep1Pos:= Pos('.', S3);
    
    if (Sep1Pos = 2) then
      S2:= ' ' + S2 + ' ';
    
    Result:= S2 + S3;
    
    while Length(Result) < 7 do
      Result:= ' ' + Result;    
    
    Exit;
  end;
    
  Result:= '+';
  
  if (S = '') and (UseDefaultZeroValue) then
  begin
    Result:= '';
  end;
  
  Sep1Pos:= Pos('.', S1);
  
  if (Sep1Pos = 2) then
    Result:= ' ' + Result + ' ';
  
  Result:= Result + S1;
  
  while Length(Result) < 7 do
    Result:= ' ' + Result;  
end;

function FormatAxisValue(const S: String; const UseDefaultZeroValue: Boolean): String;
begin
  Result:= Trim(S);
  
  if (Result = '') and (not UseDefaultZeroValue) then
  begin
    Exit;  
  end;
  
  if (Result = '') and (UseDefaultZeroValue) then
  begin
    Result:= '0.00';
  end;
  
  while Length(Result) < 3 do
    Result:= ' ' + Result;

  while Length(Result) < 7 do
    Result:= ' ' + Result;
end;

function FormatPrismValue(const S: String; const UseDefaultZeroValue: Boolean; const EyeMarker: String): String;
var
  S1: String;
begin
  S1:= Trim(S);
  
  if (S1 = '') and (not UseDefaultZeroValue) then
  begin
    Exit;
  end;  
  
  if (S1 = '') and (UseDefaultZeroValue) then
  begin
    if (EyeMarker = 'R') then
    begin
      S1:= '0.00';
    end;
    
    if (EyeMarker = 'L') then
    begin
      S1:= '0.00';
    end;
    
    Result:= Trim(S1);
  
    while Length(Result) < 7 do
      Result:= ' ' + Result;       
    
    if (EyeMarker = 'R') then
    begin
      Result:= Result + ' IN';
    end;
    
    if (EyeMarker = 'L') then
    begin
      Result:= Result + ' UP';
    end;    
     
    Exit;   
  end;
  
  Result:= Trim(S1);
  
  while Length(Result) < 8 do
    Result:= ' ' + Result;    
   
  if T2WEndsStr('I', Result) then
  begin
    Result:= T2WStringReplace(Result, 'I', ' IN', True, False);
    Exit;
  end;
  
  if T2WEndsStr('D', Result) then
  begin
    Result:= T2WStringReplace(Result, 'D', ' DOWN', True, False);
    Exit;
  end;  
  
  if T2WEndsStr('U', Result) then
  begin
    Result:= T2WStringReplace(Result, 'U', ' UP', True, False); 
    Exit;
  end;   
   
  if T2WEndsStr('O', Result) then
  begin
    Result:= T2WStringReplace(Result, 'O', ' OUT', True, False);
    Exit;
  end;
end;

function FormatPDValue(const S: String; const UseDefaultZeroValue: Boolean): String;
begin
  Result:= Trim(S);
  
  if (Result = '') and (not UseDefaultZeroValue) then
  begin
    Exit;
  end;
  
  if (Result = '') and (UseDefaultZeroValue) then
  begin
    Result:= '0.00'; 
  end;
  
  while Length(Result) < 7 do
    Result:= ' ' + Result;  
end;

function FormatVDValue(const S: String; const UseDefaultZeroValue: Boolean): String;
begin
  Result:= Trim(S);
  
  if (Result = '') and (not UseDefaultZeroValue) then
  begin
    Exit; 
  end;
  
  if (Result = '') and (UseDefaultZeroValue) then
  begin
    Result:= '00';
  end;   
  
  while Length(Result) < 7 do
    Result:= ' ' + Result;  
end;

function GetPDValue(const S1, S2: String; const UseDefaultZeroValue: Boolean): String;
begin
  if (S1 <> '') then
  begin
    Result:= Trim(S1);
    Exit;
  end;
  
  if (S2 <> '') then
  begin
    Result:= Trim(S2);
    Exit;  
  end;
  
  Result:= '';
  
  if (Result = '') and (UseDefaultZeroValue) then
  begin
    Result:= '0.00';
  end;  
end;

function GetVDValue(const S1, S2: String; const UseDefaultZeroValue: Boolean): String;
begin
  if (S1 <> '') then
  begin
    Result:= Trim(S1);
    Exit;
  end;
  
  if (S2 <> '') then
  begin
    Result:= Trim(S2);
    Exit;  
  end;
  
  Result:= '';
  
  if (Result = '') and (UseDefaultZeroValue) then
  begin
    Result:= '00';
  end;    
end;

function FindNextIdentifierLineIndex(const S: String;
                                     StartIndex: Integer;
                                     const NumericArrayValues: String): Integer;
var
  i, j: Integer;
  Found: Boolean;
begin
  Result:= -1;

  if (S = '') then
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

    if (Found) then
      Continue;

    Result:= i - StartIndex;
    Break;
  end;

  if (Result = -1) then
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

  if (DataEndIndex <= 0) then
  begin
    Data:= Result;
    Exit;
  end;

  Data:= Copy(S, DataStartIndex + Length(Identifier), DataEndIndex);

  Result:= Copy(S, 1, DataStartIndex - 1) + Copy(S, DataEndIndex + Length(Identifier) + 1, Length(S));
end;

function GetPrismDetailValue(const S: String; 
                             const P: String; 
                             const V1: String; 
                             const V2: String): String;
var
  S1: String;
begin
  Result:= '';

  if (S = '') then
    Exit;

  if (P = '') then
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
  DataToSend, EMRDriverDataInputMessageType, S: String;
  i: Integer;
  LM_R_S, LM_R_S_N, LM_R_S_F, LM_R_Z, LM_R_AX, LM_R_A, LM_R_A2, LM_R_PH, LM_R_PV, LM_R_P, LM_R_PD, LM_R_VD: String;
  LM_L_S, LM_L_S_N, LM_L_S_F, LM_L_Z, LM_L_AX, LM_L_A, LM_L_A2, LM_L_PH, LM_L_PV, LM_L_P, LM_L_PD, LM_L_VD: String; 
  AR_R_S, AR_R_S_N, AR_R_S_F, AR_R_Z, AR_R_AX, AR_R_A, AR_R_A2, AR_R_PH, AR_R_PV, AR_R_P, AR_R_PD, AR_R_VD: String;
  AR_L_S, AR_L_S_N, AR_L_S_F, AR_L_Z, AR_L_AX, AR_L_A, AR_L_A2, AR_L_PH, AR_L_PV, AR_L_P, AR_L_PD, AR_L_VD: String;  
  FN_R_S, FN_R_S_N, FN_R_S_F, FN_R_Z, FN_R_AX, FN_R_A, FN_R_A2, FN_R_PH, FN_R_PV, FN_R_P, FN_R_PD, FN_R_VD: String;
  FN_L_S, FN_L_S_N, FN_L_S_F, FN_L_Z, FN_L_AX, FN_L_A, FN_L_A2, FN_L_PH, FN_L_PV, FN_L_P, FN_L_PD, FN_L_VD: String;  
  bolRemoveLeadingLineWhiteSpace, bolMarkerFoundN, bolMarkerFoundF, bolDefaultFound, bolDisableSendingOfData: Boolean;
  bolLensmeterDataFound, bolRefraktometerDataFound, bolIgnoreVerticalDistanceHSAValue, bolPhoropterDataFound: Boolean;
  bolAddAdditionalReceiveForBadAckHandling: Boolean;
begin
  // Verwende "True", damit führende Leerzeichen aus jeder Datenzeile entfernt werden, 
  // benutze "False", damit führende Leerzeichen in jeder Datenzeile erhalten bleiben.
  bolRemoveLeadingLineWhiteSpace:= True;
  
  // Der Visutron Plus verfügt über 1 mögliches Übertragungsprotokoll.
  // Der EMR driver sendet und empfängt anhand des ausgewählten Protokolls die Daten.
  // Der Protokolltyp muss an die Gerätekonfiguration angepasst werden.
  EMRDriverDataInputMessageType:= 'Vis';

  // Verwende "True", damit der VD / HSA () Wert NICHT ans Gerät übermittelt wird,
  // benutze "False", damit der Wert berücksichtigt wird.
  bolIgnoreVerticalDistanceHSAValue:= False;
  
  // Verwende "True", wenn das Gerät "zuviele" ACK Antworten sendet,
  // die Verbindung bricht nach dem Senden der Daten ab.
  bolAddAdditionalReceiveForBadAckHandling:= False;

  // --- Don't edit script down below ---
  
  bolDisableSendingOfData := FDisableSendingOfData;
  
  if not FDisableSendingOfData then
  begin
    // do nothing
  end;  
  
  SetLength(GDTDataToSend, 0);
  
  // reset all local variables
  bolLensmeterDataFound:= False;
  bolRefraktometerDataFound:= False;
  bolPhoropterDataFound:= False;
  
  //
  LM_R_S:= '';
  LM_R_S_N:= '';
  LM_R_S_F:= '';
  LM_R_Z:= '';
  LM_R_AX:= '';
  LM_R_A:= '';
  LM_R_A2:= '';
  LM_R_PH:= '';
  LM_R_PV:= '';
  LM_R_P:= '';
  LM_R_PD:= '';
  LM_R_VD:= '';
  
  // 
  LM_L_S:= '';
  LM_L_S_N:= '';
  LM_L_S_F:= '';
  LM_L_Z:= '';
  LM_L_AX:= '';
  LM_L_A:= '';
  LM_L_A2:= '';
  LM_L_PH:= '';
  LM_L_PV:= '';
  LM_L_P:= '';
  LM_L_PD:= '';
  LM_L_VD:= '';

  //
  AR_R_S:= '';
  AR_R_S_N:= '';
  AR_R_S_F:= '';
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
  AR_L_S_N:= '';
  AR_L_S_F:= '';
  AR_L_Z:= '';
  AR_L_AX:= '';
  AR_L_A:= '';
  AR_L_A2:= '';
  AR_L_PH:= '';
  AR_L_PV:= '';
  AR_L_P:= ''; 
  AR_L_PD:= '';
  AR_L_VD:= '';
  
  //
  FN_R_S:= '';
  FN_R_S_N:= '';
  FN_R_S_F:= '';
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
  FN_L_S_N:= '';
  FN_L_S_F:= '';
  FN_L_Z:= '';
  FN_L_AX:= '';
  FN_L_A:= '';
  FN_L_A2:= '';
  FN_L_PH:= '';
  FN_L_PV:= '';
  FN_L_P:= '';
  FN_L_PD:= ''; 
  FN_L_VD:= '';
  
  // Get "Lensmeter" data lines
  DoCOMPSGetNonGDTDataForGroup(NonGDTDataLineLensmeter, GDTDataToSend);
  
  if Length(GDTDataToSend) > 0 then
  begin
    bolLensmeterDataFound:= True;
    
    for i:= 0 to Length(GDTDataToSend) - 1 do
    begin
      S:= GDTDataToSend[i];
      
      // Remove leading white space
      if (bolRemoveLeadingLineWhiteSpace) then
        S:= TrimLeft(S);
      
      if (T2WStartsStr(RIGHT_EYE_START_MARKER, S))
      or (T2WStartsStr(RIGHT_EYE_START_MARKER_LM_F, S))
      or (T2WStartsStr(RIGHT_EYE_START_MARKER_LM_N, S))
      then
      begin
        bolMarkerFoundN:= False;
        bolMarkerFoundF:= False; 
        bolDefaultFound:= False;      
               
        if T2WStartsStr(RIGHT_EYE_START_MARKER_LM_N, S) then
        begin
          bolMarkerFoundN:= True;
        end;
        
        if T2WStartsStr(RIGHT_EYE_START_MARKER_LM_F, S) then
        begin
          bolMarkerFoundF:= True;
        end;
        
        if T2WStartsStr(RIGHT_EYE_START_MARKER, S) then
        begin
          bolDefaultFound:= True; 
        end;       
                
        // Remove eye identifier
        if T2WStartsStr(RIGHT_EYE_START_MARKER, S) then
          S:= Copy(S, Length(RIGHT_EYE_START_MARKER) + 1, Length(S))
        else if T2WStartsStr(RIGHT_EYE_START_MARKER_LM_F, S) then
          S:= Copy(S, Length(RIGHT_EYE_START_MARKER_LM_F) + 1, Length(S))
        else if T2WStartsStr(RIGHT_EYE_START_MARKER_LM_N, S) then
          S:= Copy(S, Length(RIGHT_EYE_START_MARKER_LM_N) + 1, Length(S));
        
        if (bolMarkerFoundN) then
        begin
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   LM_R_S_N);       
        end;
        
        if (bolMarkerFoundF) then
        begin
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   LM_R_S_F);
        end;        
        
        if (bolDefaultFound) then
        begin
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   LM_R_S);
        end;
  
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
                                 
        // Get VD data
        S:= GetDataForIdentifier(S,
                                 VD_IDENTIFIER,
                                 NumericArrayValues,
                                 LM_R_VD);                                
                                                                            
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
        LM_R_PH:= GetPrismDetailValue(S, LM_R_P, 'I', 'O');
        LM_R_PV:= GetPrismDetailValue(S, LM_R_P, 'U', 'D');
      end
      else if (T2WStartsStr(LEFT_EYE_START_MARKER, S))
      or (T2WStartsStr(LEFT_EYE_START_MARKER_LM_F, S))
      or (T2WStartsStr(LEFT_EYE_START_MARKER_LM_N, S))
      then
      begin      
        bolMarkerFoundN:= False;
        bolMarkerFoundF:= False;
        bolDefaultFound:= False;
        
        if T2WStartsStr(LEFT_EYE_START_MARKER_LM_N, S) then
        begin
          bolMarkerFoundN:= True;
        end;
        
        if T2WStartsStr(LEFT_EYE_START_MARKER_LM_F, S) then
        begin
          bolMarkerFoundF:= True;
        end;    
            
        if T2WStartsStr(LEFT_EYE_START_MARKER, S) then
        begin
          bolDefaultFound:= True; 
        end;               
                
        // Remove eye identifier
        if T2WStartsStr(LEFT_EYE_START_MARKER, S) then
          S:= Copy(S, Length(LEFT_EYE_START_MARKER) + 1, Length(S))
        else if T2WStartsStr(LEFT_EYE_START_MARKER_LM_F, S) then
          S:= Copy(S, Length(LEFT_EYE_START_MARKER_LM_F) + 1, Length(S))
        else if T2WStartsStr(LEFT_EYE_START_MARKER_LM_N, S) then
          S:= Copy(S, Length(LEFT_EYE_START_MARKER_LM_N) + 1, Length(S));
        
        if (bolMarkerFoundN) then
        begin
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   LM_L_S_N);        
        end;
        
        if (bolMarkerFoundF) then
        begin
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   LM_L_S_F);          
        end;   
        
        if (bolDefaultFound) then
        begin
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   LM_L_S);          
        end;

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
        
        // Get VD data
        S:= GetDataForIdentifier(S,
                                 VD_IDENTIFIER,
                                 NumericArrayValues,
                                 LM_L_VD);           
        
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
        LM_L_PH:= GetPrismDetailValue(S, LM_L_P, 'I', 'O');
        LM_L_PV:= GetPrismDetailValue(S, LM_L_P, 'U', 'D');
      end;
    end; 
  end; 
 
  // check "Lensmeter" data lines
  if (LM_R_S_F = '') then
  begin
    if (LM_R_S <> '') then
    begin
      LM_R_S_F:= LM_R_S;
    end;
  end;
  
  if (LM_L_S_F = '') then
  begin
    if (LM_L_S <> '') then
    begin
      LM_L_S_F:= LM_L_S;
    end;
  end; 
  
  // Get "Refraktometer / Objektiv" data lines
  DoCOMPSGetNonGDTDataForGroup(NonGDTDataLineRefraktometerObjektiv, GDTDataToSend);
  
  if Length(GDTDataToSend) > 0 then
  begin
    bolRefraktometerDataFound:= True;
   
    for i:= 0 to Length(GDTDataToSend) - 1 do
    begin
      S:= GDTDataToSend[i];
      
      // Remove leading white space
      if (bolRemoveLeadingLineWhiteSpace) then
        S:= TrimLeft(S);
      
      if (T2WStartsStr(RIGHT_EYE_START_MARKER, S))
      or (T2WStartsStr(RIGHT_EYE_START_MARKER_ARK_F, S))
      or (T2WStartsStr(RIGHT_EYE_START_MARKER_ARK_N, S))
      then
      begin
        bolMarkerFoundN:= False;
        bolMarkerFoundF:= False;
        bolDefaultFound:= False;
        
        AR_R_S:= '';
        AR_R_S_N:= '';
        AR_R_S_F:= '';
        AR_R_Z:= '';
        AR_R_AX:= '';
        AR_R_A:= '';
        AR_R_A2:= '';
        AR_R_PH:= '';
        AR_R_PV:= '';
        AR_R_P:= '';
        AR_R_PD:= '';
        AR_R_VD:= '';
        
        if T2WStartsStr(RIGHT_EYE_START_MARKER_ARK_N, S) then
        begin
          bolMarkerFoundN:= True;
        end;
        
        if T2WStartsStr(RIGHT_EYE_START_MARKER_ARK_F, S) then
        begin
          bolMarkerFoundF:= True;
        end;  
        
        if T2WStartsStr(RIGHT_EYE_START_MARKER, S) then
        begin
          bolDefaultFound:= True;
        end;
        
        // Remove eye identifier
        if T2WStartsStr(RIGHT_EYE_START_MARKER, S) then
          S:= Copy(S, Length(RIGHT_EYE_START_MARKER) + 1, Length(S))
        else if T2WStartsStr(RIGHT_EYE_START_MARKER_ARK_F, S) then
          S:= Copy(S, Length(RIGHT_EYE_START_MARKER_ARK_F) + 1, Length(S))
        else if T2WStartsStr(RIGHT_EYE_START_MARKER_ARK_N, S) then
          S:= Copy(S, Length(RIGHT_EYE_START_MARKER_ARK_N) + 1, Length(S));
        
        if (bolMarkerFoundN) then
        begin
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   AR_R_S_N); 
        end;
        
        if (bolMarkerFoundF) then
        begin
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   AR_R_S_F); 
        end;    

        if (bolDefaultFound) then
        begin
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   AR_R_S);         
        end;

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
        AR_R_PH:= GetPrismDetailValue(S, AR_R_P, 'I', 'O');
        AR_R_PV:= GetPrismDetailValue(S, AR_R_P, 'U', 'D');
      end
      else if (T2WStartsStr(LEFT_EYE_START_MARKER, S))
      or (T2WStartsStr(LEFT_EYE_START_MARKER_ARK_F, S))
      or (T2WStartsStr(LEFT_EYE_START_MARKER_ARK_N, S))
      then
      begin
        bolMarkerFoundN:= False;
        bolMarkerFoundF:= False;
        bolDefaultFound:= False;
        
        AR_L_S:= '';
        AR_L_S_N:= '';
        AR_L_S_F:= '';
        AR_L_Z:= '';
        AR_L_AX:= '';
        AR_L_A:= '';
        AR_L_A2:= '';
        AR_L_PH:= '';
        AR_L_PV:= '';
        AR_L_P:= '';
        AR_L_PD:= '';
        AR_L_VD:= '';        
        
        if T2WStartsStr(LEFT_EYE_START_MARKER_ARK_N, S) then
        begin
          bolMarkerFoundN:= True;
        end;
        
        if T2WStartsStr(LEFT_EYE_START_MARKER_ARK_F, S) then
        begin
          bolMarkerFoundF:= True;
        end;  
        
        if T2WStartsStr(LEFT_EYE_START_MARKER, S) then
        begin
          bolDefaultFound:= True;
        end;
        
        // Remove eye identifier
        if T2WStartsStr(LEFT_EYE_START_MARKER, S) then
          S:= Copy(S, Length(LEFT_EYE_START_MARKER) + 1, Length(S))
        else if T2WStartsStr(LEFT_EYE_START_MARKER_ARK_F, S) then
          S:= Copy(S, Length(LEFT_EYE_START_MARKER_ARK_F) + 1, Length(S))
        else if T2WStartsStr(LEFT_EYE_START_MARKER_ARK_N, S) then
          S:= Copy(S, Length(LEFT_EYE_START_MARKER_ARK_N) + 1, Length(S));
        
        if (bolMarkerFoundN) then
        begin
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   AR_L_S_N); 
        end;
        
        if (bolMarkerFoundF) then
        begin       
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   AR_L_S_F); 
        end;    
        
        if (bolDefaultFound) then
        begin
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   AR_L_S);         
        end;
        
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
                                 
        // Get PD data
        S:= GetDataForIdentifier(S,
                                 PD_IDENTIFIER,
                                 NumericArrayValues,
                                 AR_L_PD);
        
        // Get VD data
        S:= GetDataForIdentifier(S,
                                 VD_IDENTIFIER,
                                 NumericArrayValues,
                                 AR_L_VD);                                 
        
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
        AR_L_PH:= GetPrismDetailValue(S, AR_L_P, 'I', 'O');
        AR_L_PV:= GetPrismDetailValue(S, AR_L_P, 'U', 'D');
      end;
    end;            
  end;  
  
  // 24-11-2022 BUGFIX: check "Refraktometer / Objektiv" data lines
  if (AR_R_S_F = '') then
  begin
    if (AR_R_S <> '') then
    begin
      AR_R_S_F:= AR_R_S;
    end;
  end;
  
  if (AR_L_S_F = '') then
  begin
    if (AR_L_S <> '') then
    begin
      AR_L_S_F:= AR_L_S;
    end;
  end;

  // Get "Phoropter" data lines
  DoCOMPSGetNonGDTDataForGroup(NonGDTDataLinePhoropter, GDTDataToSend);
  
  if Length(GDTDataToSend) > 0 then
  begin
    bolPhoropterDataFound := True;
    
    for i:= 0 to Length(GDTDataToSend) - 1 do
    begin
      S:= GDTDataToSend[i];
      
      // Remove leading white space
      if (bolRemoveLeadingLineWhiteSpace) then
        S:= TrimLeft(S);
      
      if (T2WStartsStr(RIGHT_EYE_START_MARKER, S))
      or (T2WStartsStr(RIGHT_EYE_START_MARKER_RKT_F, S))
      or (T2WStartsStr(RIGHT_EYE_START_MARKER_RKT_N, S))
      then
      begin
        bolMarkerFoundN:= False;
        bolMarkerFoundF:= False;
        bolDefaultFound:= False;
      
        FN_R_S:= '';
        FN_R_S_N:= '';
        FN_R_S_F:= '';        
        FN_R_Z:= '';
        FN_R_AX:= '';
        FN_R_A:= '';
        FN_R_A2:= '';
        FN_R_PH:= '';
        FN_R_PV:= '';
        FN_R_P:= '';
        FN_R_PD:= '';
        FN_R_VD:= '';        
        
        if T2WStartsStr(RIGHT_EYE_START_MARKER_RKT_N, S) then
        begin
          bolMarkerFoundN:= True;
        end;
        
        if T2WStartsStr(RIGHT_EYE_START_MARKER_RKT_F, S) then
        begin
          bolMarkerFoundF:= True;
        end;  
        
        if T2WStartsStr(RIGHT_EYE_START_MARKER, S) then
        begin
          bolDefaultFound:= True;
        end;       
        
        // Remove eye identifier
        if T2WStartsStr(RIGHT_EYE_START_MARKER, S) then
          S:= Copy(S, Length(RIGHT_EYE_START_MARKER) + 1, Length(S))
        else if T2WStartsStr(RIGHT_EYE_START_MARKER_RKT_F, S) then
          S:= Copy(S, Length(RIGHT_EYE_START_MARKER_RKT_F) + 1, Length(S))
        else if T2WStartsStr(RIGHT_EYE_START_MARKER_RKT_N, S) then
          S:= Copy(S, Length(RIGHT_EYE_START_MARKER_RKT_N) + 1, Length(S));
        
        if (bolMarkerFoundN) then
        begin
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   FN_R_S_N);
        end;
        
        if (bolMarkerFoundF) then
        begin
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   FN_R_S_F);
        end; 
        
        if (bolDefaultFound) then
        begin
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   FN_R_S);
        end;          
        
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
        FN_R_PH:= GetPrismDetailValue(S, FN_R_P, 'I', 'O');
        FN_R_PV:= GetPrismDetailValue(S, FN_R_P, 'U', 'D');
      end
      else if (T2WStartsStr(LEFT_EYE_START_MARKER, S))
      or (T2WStartsStr(LEFT_EYE_START_MARKER_RKT_F, S))
      or (T2WStartsStr(LEFT_EYE_START_MARKER_RKT_N, S))
      then
      begin
        bolMarkerFoundN:= False;
        bolMarkerFoundF:= False;
        bolDefaultFound:= False;
        
        FN_L_S:= '';
        FN_L_S_N:= '';
        FN_L_S_F:= '';        
        FN_L_Z:= '';
        FN_L_AX:= '';
        FN_L_A:= '';
        FN_L_A2:= '';
        FN_L_PH:= '';
        FN_L_PV:= '';
        FN_L_P:= '';
        FN_L_PD:= '';
        FN_L_VD:= '';
        
        if T2WStartsStr(LEFT_EYE_START_MARKER_RKT_N, S) then
        begin
          bolMarkerFoundN:= True;
        end;
        
        if T2WStartsStr(LEFT_EYE_START_MARKER_RKT_F, S) then
        begin
          bolMarkerFoundF:= True;
        end;  
        
        if T2WStartsStr(LEFT_EYE_START_MARKER, S) then
        begin
          bolDefaultFound:= True;
        end;  
                    
        // Remove eye identifier
        if T2WStartsStr(RIGHT_EYE_START_MARKER, S) then
          S:= Copy(S, Length(LEFT_EYE_START_MARKER) + 1, Length(S))
        else if T2WStartsStr(LEFT_EYE_START_MARKER_RKT_F, S) then
          S:= Copy(S, Length(LEFT_EYE_START_MARKER_RKT_F) + 1, Length(S))
        else if T2WStartsStr(LEFT_EYE_START_MARKER_RKT_N, S) then
          S:= Copy(S, Length(LEFT_EYE_START_MARKER_RKT_N) + 1, Length(S));
        
        if (bolMarkerFoundN) then
        begin
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   FN_L_S_N);
        end;
        
        if (bolMarkerFoundF) then
        begin
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   FN_L_S_F);
        end;          
        
        if (bolDefaultFound) then
        begin
          // Get sphere data
          S:= GetDataForIdentifier(S,
                                   SPHERE_IDENTIFIER,
                                   NumericArrayValues,
                                   FN_L_S);
        end;        
                
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
        FN_L_PH:= GetPrismDetailValue(S, FN_L_P, 'I', 'O');
        FN_L_PV:= GetPrismDetailValue(S, FN_L_P, 'U', 'D');
      end;
    end;
  end;
  
  // check "Phoropter / Subjektiv" (CO) data lines
  if (FN_R_S_F = '') then
  begin
    if (FN_R_S <> '') then
    begin
      FN_R_S_F:= FN_R_S;
    end;
  end;
  
  if (FN_L_S_F = '') then
  begin
    if (FN_L_S <> '') then
    begin
      FN_L_S_F:= FN_L_S;
    end;
  end;

  // The Visutron plus will accept data input messages from an external computer system in
  // order to transfer refraction measurement data from the external system into the Visutron
  // plus.
  DataToSend:= '';
  
  // ---------------------------------------------------------------------------------

  // Check the selected EMR driver data input message type here.
  if (EMRDriverDataInputMessageType = 'Vis') and (not bolDisableSendingOfData) then
  begin
    // send LM data
    DataToSend:= ''; 
    DataToSend:= DataToSend + DATA_STX; 
    
    DataToSend:= DataToSend + 'COMP' + DATA_LINE_SEPARATOR_01;
    DataToSend:= DataToSend + 'DATA' + DATA_LINE_SEPARATOR_01; 
    
    if (bolLensmeterDataFound) then
    begin
      DataToSend:= DataToSend + 'LM' + DATA_LINE_SEPARATOR_01;
    
      DataToSend:= DataToSend + 'R' + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_F:' + FormatNumberValue(LM_R_S_F, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_N:' + FormatNumberValue(LM_R_S_N, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'CYL  :' + FormatNumberValue(LM_R_Z, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'AXIS :' + FormatAxisValue(LM_R_AX, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'PRISM:' + FormatPrismValue(LM_R_PH, True, 'R') + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'ACC  :' + FormatNumberValue(LM_R_A, True) + DATA_LINE_SEPARATOR_01;
              
      DataToSend:= DataToSend + 'L' + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_F:' + FormatNumberValue(LM_L_S_F, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_N:' + FormatNumberValue(LM_L_S_N, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'CYL  :' + FormatNumberValue(LM_L_Z, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'AXIS :' + FormatAxisValue(LM_L_AX, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'PRISM:' + FormatPrismValue(LM_L_PV, True, 'L') + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'ACC  :' + FormatNumberValue(LM_L_A, True) + DATA_LINE_SEPARATOR_01; // Accommodation range (A or ACC) (IGNORED)   
      
      if (bolIgnoreVerticalDistanceHSAValue) then
      begin
        DataToSend:= DataToSend + 'HSA   :' + '     00' + DATA_LINE_SEPARATOR_01;
      end
      else
      begin
        DataToSend:= DataToSend + 'HSA   :' + FormatVDValue(GetVDValue(LM_R_VD, LM_L_VD, True), True) + DATA_LINE_SEPARATOR_01;
      end;   
         
      DataToSend:= DataToSend + 'PD   :' + FormatPDValue(GetPDValue(LM_R_PD, LM_L_PD, True), True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'BLUR :' + '   0.00' + DATA_LINE_SEPARATOR_01; // Blur point (IGNORED)       
    end;

    DataToSend:= DataToSend + DATA_ETX;
    
    // send LM data to the device (Computer -> Visutron Plus)
    if (DataToSend <> '') and (bolLensmeterDataFound) then
    begin
      DoCOMPSSendData(DataToSend);
    end;
    
    if (FErrorOccurred) then
    begin
      Exit;
    end;      

    // ---------------------------------------------------------------------------------

    // send AR data 
    DataToSend:= ''; 
    DataToSend:= DataToSend + DATA_STX; 
    
    DataToSend:= DataToSend + 'COMP' + DATA_LINE_SEPARATOR_01;
    DataToSend:= DataToSend + 'DATA' + DATA_LINE_SEPARATOR_01; 
    
    if (bolRefraktometerDataFound) then
    begin
      DataToSend:= DataToSend + 'AR' + DATA_LINE_SEPARATOR_01;    
    
      DataToSend:= DataToSend + 'R' + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_F:' + FormatNumberValue(AR_R_S_F, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_N:' + FormatNumberValue(AR_R_S_N, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'CYL  :' + FormatNumberValue(AR_R_Z, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'AXIS :' + FormatAxisValue(AR_R_AX, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'PRISM:' + FormatPrismValue(AR_R_PH, True, 'R') + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'ACC  :' + FormatNumberValue(AR_R_A, True) + DATA_LINE_SEPARATOR_01; // Accommodation range (A or ACC) (IGNORED)       

      DataToSend:= DataToSend + 'L' + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_F:' + FormatNumberValue(AR_L_S_F, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_N:' + FormatNumberValue(AR_L_S_N, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'CYL  :' + FormatNumberValue(AR_L_Z, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'AXIS :' + FormatAxisValue(AR_L_AX, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'PRISM:' + FormatPrismValue(AR_L_PV, True, 'L') + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'ACC  :' + FormatNumberValue(AR_L_A, True) + DATA_LINE_SEPARATOR_01; // Accommodation range (A or ACC) (IGNORED)   

      if (bolIgnoreVerticalDistanceHSAValue) then
      begin
        DataToSend:= DataToSend + 'HSA   :' + '     00' + DATA_LINE_SEPARATOR_01;
      end
      else
      begin
        DataToSend:= DataToSend + 'HSA   :' + FormatVDValue(GetVDValue(AR_R_VD, AR_L_VD, True), True) + DATA_LINE_SEPARATOR_01;
      end;
         
      DataToSend:= DataToSend + 'PD   :' + FormatPDValue(GetPDValue(AR_R_PD, AR_L_PD, True), True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'BLUR :' + '   0.00' + DATA_LINE_SEPARATOR_01; // Blur point (IGNORED)           
    end;
        
    DataToSend:= DataToSend + DATA_ETX;
        
    // send all data to the device (Computer -> Visutron plus)
    if (DataToSend <> '') and (bolRefraktometerDataFound) then
    begin
      DoCOMPSSendData(DataToSend);
    end;
    
    if (FErrorOccurred) then
    begin
      Exit;
    end;
    
    // ---------------------------------------------------------------------------------

    // send CO data     
    DataToSend:= ''; 
    DataToSend:= DataToSend + DATA_STX; 
    
    DataToSend:= DataToSend + 'COMP' + DATA_LINE_SEPARATOR_01;
    DataToSend:= DataToSend + 'DATA' + DATA_LINE_SEPARATOR_01; 
     
    if (bolPhoropterDataFound) then
    begin
      DataToSend:= DataToSend + 'CO' + DATA_LINE_SEPARATOR_01;    
    
      DataToSend:= DataToSend + 'R' + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_F:' + FormatNumberValue(FN_R_S_F, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_N:' + FormatNumberValue(FN_R_S_N, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'CYL  :' + FormatNumberValue(FN_R_Z, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'AXIS :' + FormatAxisValue(FN_R_AX, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'PRISM:' + FormatPrismValue(FN_R_PH, True, 'R') + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'ACC  :' + FormatNumberValue(FN_R_A, True) + DATA_LINE_SEPARATOR_01; // Accommodation range (A or ACC) (IGNORED)       
      
      DataToSend:= DataToSend + 'L' + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_F:' + FormatNumberValue(FN_L_S_F, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_N:' + FormatNumberValue(FN_L_S_N, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'CYL  :' + FormatNumberValue(FN_L_Z, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'AXIS :' + FormatAxisValue(FN_L_AX, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'PRISM:' + FormatPrismValue(FN_L_PV, True, 'L') + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'ACC  :' + FormatNumberValue(FN_L_A, True) + DATA_LINE_SEPARATOR_01; // Accommodation range (A or ACC) (IGNORED)   

      if (bolIgnoreVerticalDistanceHSAValue) then
      begin
        DataToSend:= DataToSend + 'HSA   :' + '     00' + DATA_LINE_SEPARATOR_01;
      end
      else
      begin
        DataToSend:= DataToSend + 'HSA   :' + FormatVDValue(GetVDValue(FN_R_VD, FN_L_VD, True), True) + DATA_LINE_SEPARATOR_01;
      end;
         
      DataToSend:= DataToSend + 'PD   :' + FormatPDValue(GetPDValue(FN_R_PD, FN_L_PD, True), True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'BLUR :' + '   0.00' + DATA_LINE_SEPARATOR_01; // Blur point (IGNORED)           
    end;    

    DataToSend:= DataToSend + DATA_ETX;
        
    // send all data to the device (Computer -> Visutron Plus)
    if (DataToSend <> '') and (bolPhoropterDataFound) then
    begin
      DoCOMPSSendData(DataToSend);
    end;
    
    if (FErrorOccurred) then
    begin
      Exit;
    end;
  end;  
  
  // A data output message is sent by the Visutron Plus when
  // the OUT button on the Controller keyboard is pressed.
  DoCOMPSReceiveData();

  // DoCOMPSSaveFile('test1.txt');
  
  if (FGlobalTimeoutReached) then
  begin
    if Length(FCOMBuffer) <= 0 then
    begin
      Exit;
    end;  
  end;

  if (FErrorOccurred) then
  begin
    Exit;
  end;

  if Length(FCOMBuffer) > 0 then
  begin
    // check the buffer, if we have a valid "end", send the response to the device
    if (FCOMBuffer[Length(FCOMBuffer)-1] = DATA_ETX_RESULT) then
    begin
      // T2WMessageBoxS(FCOMBufferString);
    end
    else
    begin
      // don't exit here
    end; 
  end;

  // The receiving computer system must respond to the output
  // message within 2 seconds with an <ACK> or a <NAK> character 
  DataToSend:= DATA_ACK;
 
  if (DataToSend <> '') then
  begin
    DoCOMPSSendData(DataToSend);
  end;
  
  if (FErrorOccurred) then
  begin
    Exit;
  end;
  
  // If we don't send any measurement data to the device, 
  // we have to exit here.
  if (not bolLensmeterDataFound) 
  and (not bolRefraktometerDataFound)
  and (not bolPhoropterDataFound) then
  begin
    Exit;
  end;
  
  // receive serial data from device
  DoCOMPSReceiveData();

  if FGlobalTimeoutReached then
  begin
    if Length(FCOMBuffer) <= 0 then
      Exit;
  end;

  if (FErrorOccurred) then
  begin
    Exit;
  end;
  
  // The receiving computer system must respond to the output
  // message within 2 seconds with an <ACK> or a <NAK> character 
  DataToSend:= DATA_ACK;
 
  if (DataToSend <> '') then
  begin
    DoCOMPSSendData(DataToSend);
  end;
  
  if (FErrorOccurred) then
  begin
    Exit;
  end;
  
  // If the device answers with to much ACK responses, we could
  // not receive the real data anymore. If we have such a bad behavoir
  // the boolean to true, ot wait again for the real measurement.
  if (bolAddAdditionalReceiveForBadAckHandling) then
  begin
    // receive serial data from device
    DoCOMPSReceiveData();

    if FGlobalTimeoutReached then
    begin
      if Length(FCOMBuffer) <= 0 then
        Exit;
    end;

    if (FErrorOccurred) then
    begin
      Exit;
    end;
  end;
end.