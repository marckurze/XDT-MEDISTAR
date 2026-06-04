const
  VERSION = '1.0.22.73';
  DATE = '18.06.2021 12:56:18';
  TEXT = 'Copyright (c) 2021 team2work GmbH';
    
  DATA_LINE_SEPARATOR_01               = #$0D#$0A;

  SPHERE_IDENTIFIER                    = 'S=';
  CYLINDER_IDENTIFIER                  = 'Z=';
  AXIS_IDENTIFIER                      = '*';
  A_IDENTIFIER                         = 'A=';
  A2_IDENTIFIER                        = 'A2=';
  P_IDENTIFIER                         = 'P=';
  PD_IDENTIFIER                        = 'PD=';
  VD_IDENTIFIER                        = 'VD=';
  WD_IDENTIFIER                        = 'WD=';  
  
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
  
function FormatNumberValueEx(const S: String): String;
var
  S1, S2, S3: String;
  Sep1Pos: Integer;
begin
  Result:= S;
  
  if (S = '') then
  begin
    Exit;
  end;
  
  S1:= Trim(S);
  
  if (S1[1] = '-') or (S1[1] = '+') then
  begin
    S2:= S1[1];
    S3:= Trim(Copy(S1, 2, Length(S1)));
    
    Sep1Pos:= Pos('.', S3);
    
    if (Sep1Pos = 2) then
      S2:= ' ' + S2;
    
    Result:= S2 + ' ' + S3;
    Result:= Trim(Result);

    Exit;
  end;
  
  Result:= '+';
  
  Sep1Pos:= Pos('.', S1);
  
  if (Sep1Pos = 2) then
    Result:= ' ' + Result;
  
  Result:= Result + S1;
end;

function FormatAxisValue(const S: String): String;
begin
  Result:= Trim(S);
  
  while Length(Result) < 3 do
    Result:= ' ' + Result;
end;

function GetPDValue(const S1, S2: String): String;
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
end;

function GetVDValue(const S1, S2: String): String;
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

  if DataEndIndex <= 0 then
  begin
    Data:= Result;
    Exit;
  end;

  Data:= Copy(S, DataStartIndex + Length(Identifier), DataEndIndex);

  Result:= Copy(S, 1, DataStartIndex - 1) + Copy(S, DataEndIndex + Length(Identifier) + 1, Length(S));
end;

function GetPrismDetailValueEx(const S: String; 
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
    Result:= Trim(P) + ' ' + V1;
    Exit;
  end;

  if T2WStartsStr(V2, S) then
  begin
    Result:= Trim(P) + ' ' + V2;
    Exit;
  end;

  S1:= Copy(S, 2, Length(S));

  if T2WContainsStr(S1, V1) then
    Result:= Trim(Copy(S1, 1, Pos(V1, S1) - 1)) + ' ' + V1
  else if T2WContainsStr(S1, V2) then
    Result:= Trim(Copy(S1, 1, Pos(V2, S1) - 1)) + ' ' + V2;
end; 

function FormatPrismValueEx(const S: String): String;
var
  S1: String;
  arr_R_PRISM: TStringArray;
begin
  S1:= Trim(S);
  
  if (S1 <> '') then
  begin
    arr_R_PRISM:= Explode(' ', S1, 0);
  
    if (Length(arr_R_PRISM) = 2) then
    begin
      S1:= arr_R_PRISM[1] + ' ' + arr_R_PRISM[0];
    end; 
  end;
  
  Result:= Trim(S1);
end;   
  
var
  GDTDataToSend: TStringArray;
  DataToSend, S: String;
  Asc: Byte;
  i: Integer;  
  Res: Integer;
  bolRemoveLeadingLineWhiteSpace, bolMarkerFoundN, bolMarkerFoundF, bolDefaultFound: Boolean;
  LM_R_S, LM_R_S_N, LM_R_S_F, LM_R_Z, LM_R_AX, LM_R_A, LM_R_A2, LM_R_PH, LM_R_PV, LM_R_P, LM_R_PD, LM_R_VD: String;
  LM_L_S, LM_L_S_N, LM_L_S_F, LM_L_Z, LM_L_AX, LM_L_A, LM_L_A2, LM_L_PH, LM_L_PV, LM_L_P, LM_L_PD, LM_L_VD: String;  
  AR_R_S, AR_R_S_N, AR_R_S_F, AR_R_Z, AR_R_AX, AR_R_A, AR_R_A2, AR_R_PH, AR_R_PV, AR_R_P, AR_R_PD, AR_R_VD: String;
  AR_L_S, AR_L_S_N, AR_L_S_F, AR_L_Z, AR_L_AX, AR_L_A, AR_L_A2, AR_L_PH, AR_L_PV, AR_L_P, AR_L_PD, AR_L_VD: String; 
  FN_R_S, FN_R_S_N, FN_R_S_F, FN_R_Z, FN_R_AX, FN_R_A, FN_R_A2, FN_R_PH, FN_R_PV, FN_R_P, FN_R_PD, FN_R_VD: String;
  FN_L_S, FN_L_S_N, FN_L_S_F, FN_L_Z, FN_L_AX, FN_L_A, FN_L_A2, FN_L_PH, FN_L_PV, FN_L_P, FN_L_PD, FN_L_VD: String;   
  LM_BLOCK, RM_BLOCK, PC_BLOCK: String;
  bolUseLM_BLOCK, bolUseRM_BLOCK, bolUsePC_BLOCK: Boolean;   
begin
  // Verwende "True", damit führende Leerzeichen aus jeder Datenzeile entfernt werden, 
  // benutze "False", damit führende Leerzeichen in jeder Datenzeile erhalten bleiben.
  bolRemoveLeadingLineWhiteSpace:= True;  

  // Verwende "True" pro Zeile, damit der jeweilige Daten Block ans Gerät geschickt werden kann,
  // benutze "False", damit dies pro Block deaktiviert werden kann.
  bolUseLM_BLOCK:= True;
  bolUseRM_BLOCK:= True;
  bolUsePC_BLOCK:= True;  

  // --- Don't edit script down below ---

  SetLength(GDTDataToSend, 0);
  
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

  // Get "Lensmeter" data lines
  DoCOMPSGetNonGDTDataForGroup(NonGDTDataLineLensmeter, GDTDataToSend);
  
  if Length(GDTDataToSend) > 0 then
  begin
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
        LM_R_PH:= GetPrismDetailValueEx(S, LM_R_P, 'I', 'O');
        LM_R_PV:= GetPrismDetailValueEx(S, LM_R_P, 'U', 'D');
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
        LM_L_PH:= GetPrismDetailValueEx(S, LM_L_P, 'I', 'O');
        LM_L_PV:= GetPrismDetailValueEx(S, LM_L_P, 'U', 'D');
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
    
  // Get "Refraktometer / Objektiv" data lines
  DoCOMPSGetNonGDTDataForGroup(NonGDTDataLineRefraktometerObjektiv, GDTDataToSend);
  
  if Length(GDTDataToSend) > 0 then
  begin
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
        AR_R_PH:= GetPrismDetailValueEx(S, AR_R_P, 'I', 'O');
        AR_R_PV:= GetPrismDetailValueEx(S, AR_R_P, 'U', 'D');
      end
      else if (T2WStartsStr(LEFT_EYE_START_MARKER, S))
      or (T2WStartsStr(LEFT_EYE_START_MARKER_ARK_F, S))
      or (T2WStartsStr(LEFT_EYE_START_MARKER_ARK_N, S))
      then
      begin
        bolMarkerFoundN:= False;
        bolMarkerFoundF:= False;
        bolDefaultFound:= False;
                       
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
        AR_L_PH:= GetPrismDetailValueEx(S, AR_L_P, 'I', 'O');
        AR_L_PV:= GetPrismDetailValueEx(S, AR_L_P, 'U', 'D');
      end;
    end;            
  end;   
  
  // check "Refraktometer / Objektiv" data lines
  if (AR_L_S_F = '') then
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

  // Get "Phoropter" data lines
  DoCOMPSGetNonGDTDataForGroup(NonGDTDataLinePhoropter, GDTDataToSend);
                          
  if Length(GDTDataToSend) > 0 then
  begin
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
        FN_R_PH:= GetPrismDetailValueEx(S, FN_R_P, 'I', 'O');
        FN_R_PV:= GetPrismDetailValueEx(S, FN_R_P, 'U', 'D');
      end
      else if (T2WStartsStr(LEFT_EYE_START_MARKER, S))
      or (T2WStartsStr(LEFT_EYE_START_MARKER_ARK_F, S))
      or (T2WStartsStr(LEFT_EYE_START_MARKER_ARK_N, S))
      then
      begin
        bolMarkerFoundN:= False;
        bolMarkerFoundF:= False;
        bolDefaultFound:= False;
                       
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
        
        // Get VD data
        S:= GetDataForIdentifier(S,
                                 VD_IDENTIFIER,
                                 NumericArrayValues,
                                 FN_L_VD);                                 
        
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
        FN_L_PH:= GetPrismDetailValueEx(S, FN_L_P, 'I', 'O');
        FN_L_PV:= GetPrismDetailValueEx(S, FN_L_P, 'U', 'D');
      end;
    end;            
  end; 
  
  // check "Phoropter" data lines
  if (FN_L_S_F = '') then
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

  // now create the lm data block, the rm and pc data block
      
  // LM (Lensmeter)     
  LM_BLOCK:= '';
  LM_BLOCK:= LM_BLOCK + '*' + DATA_LINE_SEPARATOR_01;
  LM_BLOCK:= LM_BLOCK + '#LM' + DATA_LINE_SEPARATOR_01;
  LM_BLOCK:= LM_BLOCK + '# R' + ' ' + FormatNumberValueEX(LM_R_S_F) + ' ' + FormatNumberValueEx(LM_R_Z) + ' ' + FormatAxisValue(LM_R_AX) + DATA_LINE_SEPARATOR_01;
  LM_BLOCK:= LM_BLOCK + '# L' + ' ' + FormatNumberValueEx(LM_L_S_F) + ' ' + FormatNumberValueEx(LM_L_Z) + ' ' + FormatAxisValue(LM_L_AX) + DATA_LINE_SEPARATOR_01;  	
  LM_BLOCK:= LM_BLOCK + '#AR' + ' ' + FormatNumberValueEx(LM_R_A) + DATA_LINE_SEPARATOR_01;  	
  LM_BLOCK:= LM_BLOCK + '#AL' + ' ' + FormatNumberValueEx(LM_L_A) + DATA_LINE_SEPARATOR_01;  	  	
  LM_BLOCK:= LM_BLOCK + '#PR' + ' ' + FormatPrismValueEx(LM_R_PH) + ' ' + FormatPrismValueEx(LM_R_PV) + DATA_LINE_SEPARATOR_01;
  LM_BLOCK:= LM_BLOCK + '#PL' + ' ' + FormatPrismValueEx(LM_L_PH) + ' ' + FormatPrismValueEx(LM_L_PV) + DATA_LINE_SEPARATOR_01;
  LM_BLOCK:= LM_BLOCK + '#pB' + ' ' + GetPDValue(LM_R_PD, LM_L_PD) + DATA_LINE_SEPARATOR_01;
  LM_BLOCK:= LM_BLOCK + '$' + DATA_LINE_SEPARATOR_01;  	
	
  // RM (Refraktometer)
  RM_BLOCK:= '';

  if (LM_BLOCK <> '') then
  begin
    RM_BLOCK:= RM_BLOCK + DATA_LINE_SEPARATOR_01;
  end;
  
  if (LM_BLOCK = '') then
  begin
    RM_BLOCK:= RM_BLOCK + '*' + DATA_LINE_SEPARATOR_01;
  end;
  
  RM_BLOCK:= RM_BLOCK + '#RM' + DATA_LINE_SEPARATOR_01;
  RM_BLOCK:= RM_BLOCK + '# R' + ' ' + FormatNumberValueEx(AR_R_S_F) + ' ' + FormatNumberValueEx(AR_R_Z) + ' ' + FormatAxisValue(AR_R_AX) + DATA_LINE_SEPARATOR_01;
  RM_BLOCK:= RM_BLOCK + '# L' + ' ' + FormatNumberValueEx(AR_L_S_F) + ' ' + FormatNumberValueEx(AR_L_Z) + ' ' + FormatAxisValue(AR_L_AX) + DATA_LINE_SEPARATOR_01;  
  RM_BLOCK:= RM_BLOCK + '#pB' + ' ' + GetPDValue(AR_R_PD, AR_L_PD) + DATA_LINE_SEPARATOR_01;
  RM_BLOCK:= RM_BLOCK + '#pb' + '' + DATA_LINE_SEPARATOR_01;  	
  RM_BLOCK:= RM_BLOCK + '$' + DATA_LINE_SEPARATOR_01; 	  

  // PC (Phoropter)
  PC_BLOCK:= '';
  
  if (RM_BLOCK <> '') then
  begin
    PC_BLOCK:= PC_BLOCK + DATA_LINE_SEPARATOR_01;
  end;
  
  if (RM_BLOCK = '') then
  begin
    PC_BLOCK:= PC_BLOCK + '*' + DATA_LINE_SEPARATOR_01;
  end;  
  
  PC_BLOCK:= PC_BLOCK + '#PC' + DATA_LINE_SEPARATOR_01;
  PC_BLOCK:= PC_BLOCK + '# R' + ' ' + FormatNumberValueEX(FN_R_S_F) + ' ' + FormatNumberValueEx(FN_R_Z) + ' ' + FormatAxisValue(FN_R_AX) + DATA_LINE_SEPARATOR_01;
  PC_BLOCK:= PC_BLOCK + '# L' + ' ' + FormatNumberValueEx(FN_L_S_F) + ' ' + FormatNumberValueEx(FN_L_Z) + ' ' + FormatAxisValue(FN_L_AX) + DATA_LINE_SEPARATOR_01;  	
  PC_BLOCK:= PC_BLOCK + '#AR' + ' ' + FormatNumberValueEx(FN_R_A) + DATA_LINE_SEPARATOR_01;  	
  PC_BLOCK:= PC_BLOCK + '#AL' + ' ' + FormatNumberValueEx(FN_L_A) + DATA_LINE_SEPARATOR_01;  	  	
  PC_BLOCK:= PC_BLOCK + '#PR' + ' ' + FormatPrismValueEx(FN_R_PH) + ' ' + FormatPrismValueEx(FN_R_PV) + DATA_LINE_SEPARATOR_01;
  PC_BLOCK:= PC_BLOCK + '#PL' + ' ' + FormatPrismValueEx(FN_L_PH) + ' ' + FormatPrismValueEx(FN_L_PV) + DATA_LINE_SEPARATOR_01;
  // #Pr
  // #Pl
  PC_BLOCK:= PC_BLOCK + '#pB' + ' ' + GetPDValue(FN_R_PD, FN_L_PD) + DATA_LINE_SEPARATOR_01;
  // #pb

  // now create the send buffer 
  DataToSend:= '';
 
  if (LM_BLOCK <> '') and (bolUseLM_BLOCK) then
  begin
    DataToSend:= DataToSend + LM_BLOCK;
  end;
  
  if (RM_BLOCK <> '') and (bolUseRM_BLOCK) then
  begin
    if (LM_BLOCK <> '') then
    begin
      DataToSend:= DataToSend + DATA_LINE_SEPARATOR_01;
    end;
    
    DataToSend:= DataToSend + RM_BLOCK;
  end;
  
  if (PC_BLOCK <> '') and (bolUsePC_BLOCK) then
  begin
    if (LM_BLOCK <> '') or (RM_BLOCK <> '') then
    begin
      DataToSend:= DataToSend + DATA_LINE_SEPARATOR_01;
    end;
    
    DataToSend:= DataToSend + PC_BLOCK;  
  end;
  
  // T2WMessageBoxS(LM_BLOCK);
  // T2WMessageBoxS(RM_BLOCK);
  // T2WMessageBoxS(PC_BLOCK);

  // It sends the checksum as the error detection code after sending all of the data (after "@").
  // Add up all of the data from the beginning ("*") to the end ("@"), and calculate the
  // checksum of 2byte. The calculated value is converted to the ASCII code, so it is 4byte. It
  // is considered that data transmission is completed by sending the linefeed code after
  // checksum.
  // The auto phoropter does not understand the code from the beginning to the end if the
  // error detection code is not sent, the code is not matched or the linefeed code after the
  // error detection code is not sent. 	 
  DataToSend:= Trim(DataToSend);
  DataToSend := DataToSend + DATA_LINE_SEPARATOR_01;
  DataToSend := DataToSend + '@';
  
  // In case of *ABC...(midstream is omitted)...@
  // 0x2a + 0x41 + 0x42 + 0x43 + ... + 0x40(@) = 0x12AB (checksum is 2byte)
  // After the terminal symbol ("@"), send "12AB\n"(the part of checksum is
  // converted to the ASCII code, so it is 4 byte.)
  Res:= 0;

  if (DataToSend <> '') then
  begin
    for i := 1 to Length(DataToSend) do
    begin
      Asc := Ord(DataToSend[i]);

      Res := Res + Asc;
    end;  

    DataToSend:= DataToSend + T2WIntToHex(Res, 4);
  end;
 
  // string to send must be end with the line break
  DataToSend:= DataToSend + DATA_LINE_SEPARATOR_01;

  // T2WMessageBoxS(DataToSend);

  if (DataToSend <> '') then
  begin
    DoCOMPSSendData(DataToSend);
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
end.