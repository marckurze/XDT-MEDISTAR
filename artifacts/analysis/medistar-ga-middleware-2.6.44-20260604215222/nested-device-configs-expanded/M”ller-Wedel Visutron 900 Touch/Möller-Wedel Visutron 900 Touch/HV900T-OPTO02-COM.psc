const
  VERSION = '1.0.55.94';
  DATE = '14.02.2024 10:55:13';
  TEXT = 'Copyright (c) 2024 team2work GmbH';
  
  DATA_ACK               = #$06;   // ACK
  DATA_NAK               = #$15;   // NAK
  DATA_STX               = #$02;   // STX (Start-Text)
  DATA_ETX               = #$03;   // ETX (End-Text)  
    
  DATA_STX_RESULT        = $02;    // STX
  DATA_ETX_RESULT        = $03;    // ETX
  DATA_ACK_RESULT        = $06;    // ACK
  DATA_NAK_RESULT        = $15;    // NAK
  
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
  
function FormatValue(const strValue: String): String;
begin
  if (strValue = '') then
  	Exit;

  Result:= T2WStringReplace(strValue, ',', '.', True, True);
end;  

function FormatNumberValue(const S: String; const UseDefaultZeroValue: Boolean): String;
var
  S1, S2, S3: String;
  Sep1Pos: Integer;
begin
  Result:= S;
    
  S1:= Trim(S);  
  
  if (S = '') and (not UseDefaultZeroValue) then
  begin
    Exit;
  end;
  
  if (S = '') and (UseDefaultZeroValue) then
  begin
    S1:= '0.00';    
  end;  

  if (S1 = '') then
  begin
    Exit;
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
    Result:= '0';
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

  if (not T2WContainsStr(S, Identifier)) then
  begin    
    Exit;
  end;

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

  Data:= Trim(Copy(S, DataStartIndex + Length(Identifier), DataEndIndex));

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

function CalculateNearSphereValue(const SPH_F, ADD: String): String;
var
  strSPH_F, strSPH_N, strADD, strResult, S2: String;
  a1, a2, Res: Extended; 
  bolRes: Boolean;
begin
  Result:= '';
  
  if (SPH_F = '') 
  or (ADD = '') then
  begin
    Exit;
  end;
 
  strResult:= '';
  strSPH_N:= ''; 

  strSPH_F:= Trim(SPH_F);
  strADD:= Trim(ADD); 
  
  if (strSPH_F = '') 
  or (strADD = '') then
  begin
    Exit;
  end;  
  
  if (strADD = '+ 0.00') then
  begin
    // Result:= '0.00';
    // Exit;
  end; 
  
  if (strADD = '0.00') then
  begin
    // Result:= '0.00';
    // Exit;
  end;
  
  strSPH_F:= Trim(T2WStringReplace(strSPH_F, ' ', '', True, True));
  strADD:= Trim(T2WStringReplace(strADD, ' ', '', True, True));

  if strSPH_F[1] = '+' then
  begin
    S2:= '+';
  end
  else if strSPH_F[1] = '-' then
  begin
    S2:= '';
  end
  else
  begin
    S2:= '+';
  end; 
 
  bolRes:= T2WStrToFloatUniversal(strSPH_F, False, a1);  
  bolRes:= T2WStrToFloatUniversal(strADD, False, a2);  
  
  Res:= a1 + a2;

  strResult:= Trim(T2WFloatToStrF(Res, 10, 2));
  strResult:= Trim(T2WStringReplace(strResult, ',', '.', True, True));
  strResult:= S2 + strResult;

  Result:= strResult;
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
  bolRemoveLeadingLineWhiteSpace, bolMarkerFoundN, bolMarkerFoundF, bolDefaultFound, bolDisableSendingOfData, bolIgnoreCoData: Boolean;
  bolLensmeterDataFound, bolRefraktometerDataFound, bolIgnoreVerticalDistanceHSAValue, bolPhoropterDataFound: Boolean;
  bolDisableNakHandling, bolEnabledSendAllDataAtOnce: Boolean;	
begin
  // Verwende "True", damit führende Leerzeichen aus jeder Datenzeile entfernt werden, 
  // benutze "False", damit führende Leerzeichen in jeder Datenzeile erhalten bleiben.
  bolRemoveLeadingLineWhiteSpace:= True;
  
  // Der Visutron 900 Touch verfügt über 2 mögliche Übertragungsprotokolle.
  // Der EMR driver sendet und empfängt anhand des ausgewählten Protokolls die Daten.
  // Der Protokolltyp muss an die Gerätekonfiguration angepasst werden.
  // Mögliche Optionen sind: "Vis900" und "VisVP" (Schreibweise beachten).
  EMRDriverDataInputMessageType:= 'Vis900';

  // Verwende "True", damit der VD / HSA () Wert NICHT ans Gerät übermittelt wird,
  // benutze "False", damit der Wert berücksichtigt wird.
  bolIgnoreVerticalDistanceHSAValue:= False;
  
  // Verwende "True", damit vorhandene Phoropter Daten nicht in den CO Speicher geladen werden,
  // benutze "False", damit vorhandene Phoropter Daten in den CO Speicher des Gerätes geladen werden.
  bolIgnoreCoData:= False; 
  	
  // Verwende "True", damit NAK, gesendet vom Gerät ignoriert wird, 
  // benutze "False", damit NAK ausgewertet wird.
  bolDisableNakHandling:= False;
  
  // Verwende "True", damit alle Daten (LM,AR,CO) direkt ans Gerät gesendet werden,
  // benutze "False", damit die Daten Blockweise ans Gerät gesendet werden.
  bolEnabledSendAllDataAtOnce:= False;

  // --- Don't edit script down below ---
  
  if (not FDisableSendingOfData) then
  begin
    // do nothing
  end;  
  
  bolDisableSendingOfData := FDisableSendingOfData;
    
  SetLength(GDTDataToSend, 0);
  
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
        	
      // 30-05-2022 FIX: replace "," with "." - kai@team2work.de
      if T2WContainsStr(S, ',') then
      begin    
        S:= Trim(T2WStringReplace(S, ',', '.', True, True));
      end;

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
  if (LM_L_A = '') then
  begin
    LM_L_A:= '0.00';
  end;
  
  if (LM_R_A = '') then
  begin
    LM_R_A:= '0.00';
  end;    
  
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
  
  if (LM_R_S_N = '') then
  begin
    if (LM_R_A <> '') then
    begin
      if (LM_R_S_F <> '') then
      begin
        LM_R_S_N:= CalculateNearSphereValue(LM_R_S_F, LM_R_A);
      end;  
    end;
  end;
  
  if (LM_L_S_N = '') then
  begin
    if (LM_L_A <> '') then
    begin
      if (LM_L_S_F <> '') then
      begin
        LM_L_S_N:= CalculateNearSphereValue(LM_L_S_F, LM_L_A);
      end;        
    end;
    
    if (LM_L_A = '') then
    begin
      if (LM_R_A <> '') then
      begin
        if (LM_L_S_F <> '') then
        begin
          LM_L_S_N:= CalculateNearSphereValue(LM_L_S_F, LM_R_A);
        end;       
      end;
    end;    
  end;   

  if (LM_R_S_N = '') or (Trim(LM_R_S_N) = '0.00') or (Trim(LM_R_S_N) = '+ 0.00') then
  begin
    // LM_R_S_N:= ''; 
  end;
  
  if (LM_L_S_N = '') or (Trim(LM_L_S_N) = '0.00') or (Trim(LM_L_S_N) = '+ 0.00') then
  begin
    // LM_L_S_N:= ''; 
  end;

  // -----------------------------------------------------------------------------
  
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
      
      // 07-11-2022 FIX: replace "," with "." - kai@team2work.de
      if T2WContainsStr(S, ',') then
      begin    
        S:= Trim(T2WStringReplace(S, ',', '.', True, True));
      end;         
      
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
  
  // check "Refraktometer / Objektiv" data lines
  if (AR_L_A = '') then
  begin
    AR_L_A:= '0.00';
  end;
  
  if (FN_R_A = '') then
  begin
    AR_R_A:= '0.00';
  end;  
  
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
  
  if (AR_R_S_N = '') then
  begin
    if (AR_R_A <> '') then
    begin
      if (AR_R_S_F <> '') then
      begin
        AR_R_S_N:= CalculateNearSphereValue(AR_R_S_F, AR_R_A);
      end;  
    end;
  end;
  
  if (AR_L_S_N = '') then
  begin
    if (AR_L_A <> '') then
    begin
      if (AR_L_S_F <> '') then
      begin
        AR_L_S_N:= CalculateNearSphereValue(AR_L_S_F, AR_L_A);
      end;        
    end;
    
    if (AR_L_A = '') then
    begin
      if (AR_R_A <> '') then
      begin
        if (AR_L_S_F <> '') then
        begin
          AR_L_S_N:= CalculateNearSphereValue(AR_L_S_F, AR_R_A);
        end;       
      end;
    end;    
  end;
 
  if (AR_R_S_N = '') or (Trim(AR_R_S_N) = '0.00') or (Trim(AR_R_S_N) = '+ 0.00') then
  begin
    // AR_R_S_N:= ''; 
  end;
  
  if (AR_L_S_N = '') or (Trim(AR_L_S_N) = '0.00') or (Trim(AR_L_S_N) = '+ 0.00') then
  begin
    // AR_L_S_N:= ''; 
  end;

  // -----------------------------------------------------------------------------   
       
  // Get "Phoropter" data lines
  DoCOMPSGetNonGDTDataForGroup(NonGDTDataLinePhoropter, GDTDataToSend);
  
  if Length(GDTDataToSend) > 0 then
  begin
    bolPhoropterDataFound:= True;
    
    if (bolIgnoreCoData) then
    begin
      bolPhoropterDataFound:= False;
    end;
           
    for i:= 0 to Length(GDTDataToSend) - 1 do
    begin
      S:= GDTDataToSend[i];
      
      // Remove leading white space
      if (bolRemoveLeadingLineWhiteSpace) then
        S:= TrimLeft(S);
          
      // 07-11-2022 FIX: replace "," with "." - kai@team2work.de
      if T2WContainsStr(S, ',') then
      begin    
        S:= Trim(T2WStringReplace(S, ',', '.', True, True));
      end;            
               
      if (T2WStartsStr(RIGHT_EYE_START_MARKER, S))
      or (T2WStartsStr(RIGHT_EYE_START_MARKER_RKT_F, S))
      or (T2WStartsStr(RIGHT_EYE_START_MARKER_RKT_N, S))
      then
      begin  
        bolMarkerFoundN:= False;
        bolMarkerFoundF:= False;
        bolDefaultFound:= False;
                  
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

  // check "Phoropter / Subjektiv" data lines
  if (FN_L_A = '') then
  begin
    FN_L_A:= '0.00';
  end;
  
  if (FN_R_A = '') then
  begin
    FN_R_A:= '0.00';
  end;   

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

  if (FN_R_S_N = '') then
  begin
    if (FN_R_A <> '') then
    begin
      if (FN_R_S_F <> '') then
      begin
        FN_R_S_N:= CalculateNearSphereValue(FN_R_S_F, FN_R_A);
      end;  
    end;
  end;
  
  if (FN_L_S_N = '') then
  begin
    if (FN_L_A <> '') then
    begin
      if (FN_L_S_F <> '') then
      begin
        FN_L_S_N:= CalculateNearSphereValue(FN_L_S_F, FN_L_A);
      end;        
    end;
    
    if (FN_L_A = '') then
    begin
      if (FN_R_A <> '') then
      begin
        if (FN_L_S_F <> '') then
        begin
          FN_L_S_N:= CalculateNearSphereValue(FN_L_S_F, FN_R_A);
        end;       
      end;
    end;    
  end;

  if (FN_R_S_N = '') or (Trim(FN_R_S_N) = '0.00') or (Trim(FN_R_S_N) = '+ 0.00') then
  begin
    // FN_R_S_N:= ''; 
  end;
  
  if (FN_L_S_N = '') or (Trim(FN_L_S_N) = '0.00') or (Trim(FN_L_S_N) = '+ 0.00') then
  begin
    // FN_L_S_N:= ''; 
  end;

  // -----------------------------------------------------------------------------   

  // The Visutron 900 touch will accept data input messages from an external computer system in
  // order to transfer refraction measurement data from the external system into the Visutron 900
  // touch.
  DataToSend:= '';
  
  // ---------------------------------------------------------------------------------

  // Check the selected EMR driver data input message type here.
  if (EMRDriverDataInputMessageType = 'VisVP') and (not bolDisableSendingOfData) then
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
      DataToSend:= DataToSend + 'ACC  :' + '   0.00' + DATA_LINE_SEPARATOR_01; // Accommodation range (ACC) (IGNORED)
       
      DataToSend:= DataToSend + 'L' + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_F:' + FormatNumberValue(LM_L_S_F, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_N:' + FormatNumberValue(LM_L_S_N, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'CYL  :' + FormatNumberValue(LM_L_Z, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'AXIS :' + FormatAxisValue(LM_L_AX, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'PRISM:' + FormatPrismValue(LM_L_PV, True, 'L') + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'ACC  :' + '   0.00' + DATA_LINE_SEPARATOR_01; // Accommodation range (ACC) (IGNORED)

      DataToSend:= DataToSend + 'HSA  :' + '     00' + DATA_LINE_SEPARATOR_01; // Corneal vertex distance (IGNORED)
      
      DataToSend:= DataToSend + 'PD   :' + GetPDValue(LM_R_PD, LM_L_PD, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'BLUR :' + '   0.00' + DATA_LINE_SEPARATOR_01; // Blur point (IGNORED)
      
      DataToSend:= DataToSend + 'PATNAME:' + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'PAT_ID :' + DATA_LINE_SEPARATOR_01;      
    end;

    DataToSend:= DataToSend + DATA_ETX;
    
    // send LM data to the device (Computer -> Visutron 900 Touch)
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
      DataToSend:= DataToSend + 'ACC  :' + '   0.00' + DATA_LINE_SEPARATOR_01; // Accommodation range (ACC) (IGNORED)    
    
      DataToSend:= DataToSend + 'L' + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_F:' + FormatNumberValue(AR_L_S_F, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_N:' + FormatNumberValue(AR_L_S_N, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'CYL  :' + FormatNumberValue(AR_L_Z, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'AXIS :' + FormatAxisValue(AR_L_AX, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'PRISM:' + FormatPrismValue(AR_L_PV, True, 'L') + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'ACC  :' + '   0.00' + DATA_LINE_SEPARATOR_01; // Accommodation range (ACC) (IGNORED)    
    
      DataToSend:= DataToSend + 'HSA  :' + '     00' + DATA_LINE_SEPARATOR_01; // Corneal vertex distance (IGNORED)
      
      DataToSend:= DataToSend + 'PD   :' + FormatPDValue(GetPDValue(AR_R_PD, AR_L_PD, True), True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'BLUR :' + '   0.00' + DATA_LINE_SEPARATOR_01; // Blur point (IGNORED) 
      
      DataToSend:= DataToSend + 'PATNAME:' + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'PAT_ID :' + DATA_LINE_SEPARATOR_01;         
    end;
        
    DataToSend:= DataToSend + DATA_ETX;
    
    // send all data to the device (Computer -> Visutron 900 Touch)
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
      DataToSend:= DataToSend + 'ACC  :' + '   0.00' + DATA_LINE_SEPARATOR_01; // Accommodation range (ACC) (IGNORED)    
    
      DataToSend:= DataToSend + 'L' + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_F:' + FormatNumberValue(FN_L_S_F, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_N:' + FormatNumberValue(FN_L_S_N, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'CYL  :' + FormatNumberValue(FN_L_Z, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'AXIS :' + FormatAxisValue(FN_L_AX, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'PRISM:' + FormatPrismValue(FN_L_PV, True, 'L') + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'ACC  :' + '   0.00' + DATA_LINE_SEPARATOR_01; // Accommodation range (ACC) (IGNORED)    
    
      DataToSend:= DataToSend + 'HSA  :' + '     00' + DATA_LINE_SEPARATOR_01; // Corneal vertex distance (IGNORED)
      
      DataToSend:= DataToSend + 'PD   :' + FormatPDValue(GetPDValue(FN_R_PD, FN_L_PD, True), True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'BLUR :' + '   0.00' + DATA_LINE_SEPARATOR_01; // Blur point (IGNORED)  
      
      DataToSend:= DataToSend + 'PATNAME:' + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'PAT_ID :' + DATA_LINE_SEPARATOR_01;        
    end;
        
    DataToSend:= DataToSend + DATA_ETX;
    
    // send all data to the device (Computer -> Visutron 900 Touch)
    if (DataToSend <> '') and (bolPhoropterDataFound) then
    begin
      DoCOMPSSendData(DataToSend);
    end;
    
    if (FErrorOccurred) then
    begin
      Exit;
    end;        
  end;

  // ---------------------------------------------------------------------------------
  //
  // ---------------------------------------------------------------------------------
  
  if (EMRDriverDataInputMessageType = 'Vis900') and (not bolDisableSendingOfData) then
  begin
    // send LM data
    DataToSend:= ''; 
    DataToSend:= DataToSend + DATA_STX; 
    
    DataToSend:= DataToSend + 'COMP900' + DATA_LINE_SEPARATOR_01;
    DataToSend:= DataToSend + 'DATA' + DATA_LINE_SEPARATOR_01; 
		
    if (bolLensmeterDataFound) then
    begin
      DataToSend:= DataToSend + 'LM' + DATA_LINE_SEPARATOR_01;
    
      DataToSend:= DataToSend + 'RIGHT' + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_F_R:' + FormatNumberValue(LM_R_S_F, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_N_R:' + FormatNumberValue(LM_R_S_N, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'CYL_R  :' + FormatNumberValue(LM_R_Z, True) + DATA_LINE_SEPARATOR_01;    
      DataToSend:= DataToSend + 'AXIS_R :' + FormatAxisValue(LM_R_AX, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'PRISM_R:' + FormatPrismValue(LM_R_PH, True, 'R') + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'ACC_R  :' + '   0.00' + DATA_LINE_SEPARATOR_01; // Accommodation range (ACC) (IGNORED)       
      DataToSend:= DataToSend + 'VIS_S_R:' + '      0' + DATA_LINE_SEPARATOR_01; // Uncorrected (S.C) visual acuity (IGNORED)
      DataToSend:= DataToSend + 'VIS_C_R:' + '      0' + DATA_LINE_SEPARATOR_01; // Corrected (C.C.) visual acuity (IGNORED)   
      DataToSend:= DataToSend + 'PD_R   :' + '   0.00' + DATA_LINE_SEPARATOR_01; 

      DataToSend:= DataToSend + 'LEFT' + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_F_L:' + FormatNumberValue(LM_L_S_F, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_N_L:' + FormatNumberValue(LM_L_S_N, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'CYL_L  :' + FormatNumberValue(LM_L_Z, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'AXIS_L :' + FormatAxisValue(LM_L_AX, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'PRISM_L:' + FormatPrismValue(LM_L_PV, True, 'L') + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'ACC_L  :' + '   0.00' + DATA_LINE_SEPARATOR_01; // Accommodation range (ACC) (IGNORED)   
      DataToSend:= DataToSend + 'VIS_S_L:' + '      0' + DATA_LINE_SEPARATOR_01; // Uncorrected (S.C) visual acuity (IGNORED)
      DataToSend:= DataToSend + 'VIS_C_L:' + '      0' + DATA_LINE_SEPARATOR_01; // Corrected (C.C.) visual acuity (IGNORED)   
      DataToSend:= DataToSend + 'PD_L   :' + '   0.00' + DATA_LINE_SEPARATOR_01;    

      DataToSend:= DataToSend + 'BOTH' + DATA_LINE_SEPARATOR_01;
      
      if (bolIgnoreVerticalDistanceHSAValue) then
      begin
        DataToSend:= DataToSend + 'HSA    :' + '     00' + DATA_LINE_SEPARATOR_01;
      end
      else
      begin
        DataToSend:= DataToSend + 'HSA    :' + FormatVDValue(GetVDValue(LM_R_VD, LM_L_VD, True), True) + DATA_LINE_SEPARATOR_01;
      end;   
         
      DataToSend:= DataToSend + 'PD_G   :' + FormatPDValue(GetPDValue(LM_R_PD, LM_L_PD, True), True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'BLUR   :' + '   0.00' + DATA_LINE_SEPARATOR_01; // Blur point (IGNORED)
      DataToSend:= DataToSend + 'VIS_S_B:' + '      0' + DATA_LINE_SEPARATOR_01; // Uncorrected (S.C) binocular visual acuity (IGNORED)
      DataToSend:= DataToSend + 'VIS_C_B:' + '      0' + DATA_LINE_SEPARATOR_01; // Corrected (C.C.) binocular visual acuity (IGNORED)
      
      DataToSend:= DataToSend + 'PATNAME:' + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'PAT_ID :' + DATA_LINE_SEPARATOR_01;     
    end;

    DataToSend:= DataToSend + DATA_ETX;
    
    // send LM data to the device (Computer -> Visutron 900 Touch)
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
    
    DataToSend:= DataToSend + 'COMP900' + DATA_LINE_SEPARATOR_01;
    DataToSend:= DataToSend + 'DATA' + DATA_LINE_SEPARATOR_01; 
    
    if (bolRefraktometerDataFound) then
    begin
      DataToSend:= DataToSend + 'AR' + DATA_LINE_SEPARATOR_01;    
    
      DataToSend:= DataToSend + 'RIGHT' + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_F_R:' + FormatNumberValue(AR_R_S_F, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_N_R:' + FormatNumberValue(AR_R_S_N, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'CYL_R  :' + FormatNumberValue(AR_R_Z, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'AXIS_R :' + FormatAxisValue(AR_R_AX, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'PRISM_R:' + FormatPrismValue(AR_R_PH, True, 'R') + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'ACC_R  :' + '   0.00' + DATA_LINE_SEPARATOR_01; // Accommodation range (ACC) (IGNORED)       
      DataToSend:= DataToSend + 'VIS_S_R:' + '      0' + DATA_LINE_SEPARATOR_01; // Uncorrected (S.C) visual acuity (IGNORED)
      DataToSend:= DataToSend + 'VIS_C_R:' + '      0' + DATA_LINE_SEPARATOR_01; // Corrected (C.C.) visual acuity (IGNORED)   
      DataToSend:= DataToSend + 'PD_R   :' + '   0.00' + DATA_LINE_SEPARATOR_01;     

      DataToSend:= DataToSend + 'LEFT' + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_F_L:' + FormatNumberValue(AR_L_S_F, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_N_L:' + FormatNumberValue(AR_L_S_N, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'CYL_L  :' + FormatNumberValue(AR_L_Z, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'AXIS_L :' + FormatAxisValue(AR_L_AX, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'PRISM_L:' + FormatPrismValue(AR_L_PV, True, 'L') + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'ACC_L  :' + '   0.00' + DATA_LINE_SEPARATOR_01; // Accommodation range (ACC) (IGNORED)   
      DataToSend:= DataToSend + 'VIS_S_L:' + '      0' + DATA_LINE_SEPARATOR_01; // Uncorrected (S.C) visual acuity (IGNORED)
      DataToSend:= DataToSend + 'VIS_C_L:' + '      0' + DATA_LINE_SEPARATOR_01; // Corrected (C.C.) visual acuity (IGNORED)   
      DataToSend:= DataToSend + 'PD_L   :' + '   0.00' + DATA_LINE_SEPARATOR_01;        
 
      DataToSend:= DataToSend + 'BOTH' + DATA_LINE_SEPARATOR_01;
      
      if (bolIgnoreVerticalDistanceHSAValue) then
      begin
        DataToSend:= DataToSend + 'HSA    :' + '     00' + DATA_LINE_SEPARATOR_01;
      end
      else
      begin
        DataToSend:= DataToSend + 'HSA    :' + FormatVDValue(GetVDValue(AR_R_VD, AR_L_VD, True), True) + DATA_LINE_SEPARATOR_01;
      end;
         
      DataToSend:= DataToSend + 'PD_G   :' + FormatPDValue(GetPDValue(AR_R_PD, AR_L_PD, True), True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'BLUR   :' + '   0.00' + DATA_LINE_SEPARATOR_01; // Blur point (IGNORED)
      DataToSend:= DataToSend + 'VIS_S_B:' + '      0' + DATA_LINE_SEPARATOR_01; // Uncorrected (S.C) binocular visual acuity (IGNORED)
      DataToSend:= DataToSend + 'VIS_C_B:' + '      0' + DATA_LINE_SEPARATOR_01; // Corrected (C.C.) binocular visual acuity (IGNORED)
      
      DataToSend:= DataToSend + 'PATNAME:' + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'PAT_ID :' + DATA_LINE_SEPARATOR_01;                
    end;
        
    DataToSend:= DataToSend + DATA_ETX;
        
    // send all data to the device (Computer -> Visutron 900 Touch)
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
    
    DataToSend:= DataToSend + 'COMP900' + DATA_LINE_SEPARATOR_01;
    DataToSend:= DataToSend + 'DATA' + DATA_LINE_SEPARATOR_01; 
    
    if (bolPhoropterDataFound) then
    begin
      DataToSend:= DataToSend + 'CO' + DATA_LINE_SEPARATOR_01;    
    
      DataToSend:= DataToSend + 'RIGHT' + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_F_R:' + FormatNumberValue(FN_R_S_F, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_N_R:' + FormatNumberValue(FN_R_S_N, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'CYL_R  :' + FormatNumberValue(FN_R_Z, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'AXIS_R :' + FormatAxisValue(FN_R_AX, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'PRISM_R:' + FormatPrismValue(FN_R_PH, True, 'R') + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'ACC_R  :' + '   0.00' + DATA_LINE_SEPARATOR_01; // Accommodation range (ACC) (IGNORED)       
      DataToSend:= DataToSend + 'VIS_S_R:' + '      0' + DATA_LINE_SEPARATOR_01; // Uncorrected (S.C) visual acuity (IGNORED)
      DataToSend:= DataToSend + 'VIS_C_R:' + '      0' + DATA_LINE_SEPARATOR_01; // Corrected (C.C.) visual acuity (IGNORED)   
      DataToSend:= DataToSend + 'PD_R   :' + '   0.00' + DATA_LINE_SEPARATOR_01;     

      DataToSend:= DataToSend + 'LEFT' + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_F_L:' + FormatNumberValue(FN_L_S_F, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'SPH_N_L:' + FormatNumberValue(FN_L_S_N, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'CYL_L  :' + FormatNumberValue(FN_L_Z, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'AXIS_L :' + FormatAxisValue(FN_L_AX, True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'PRISM_L:' + FormatPrismValue(FN_L_PV, True, 'L') + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'ACC_L  :' + '   0.00' + DATA_LINE_SEPARATOR_01; // Accommodation range (ACC) (IGNORED)   
      DataToSend:= DataToSend + 'VIS_S_L:' + '      0' + DATA_LINE_SEPARATOR_01; // Uncorrected (S.C) visual acuity (IGNORED)
      DataToSend:= DataToSend + 'VIS_C_L:' + '      0' + DATA_LINE_SEPARATOR_01; // Corrected (C.C.) visual acuity (IGNORED)   
      DataToSend:= DataToSend + 'PD_L   :' + '   0.00' + DATA_LINE_SEPARATOR_01;        
 
      DataToSend:= DataToSend + 'BOTH' + DATA_LINE_SEPARATOR_01;
      
      if (bolIgnoreVerticalDistanceHSAValue) then
      begin
        DataToSend:= DataToSend + 'HSA    :' + '     00' + DATA_LINE_SEPARATOR_01;
      end
      else
      begin
        DataToSend:= DataToSend + 'HSA    :' + FormatVDValue(GetVDValue(FN_R_VD, FN_L_VD, True), True) + DATA_LINE_SEPARATOR_01;
      end;
         
      DataToSend:= DataToSend + 'PD_G   :' + FormatPDValue(GetPDValue(FN_R_PD, FN_L_PD, True), True) + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'BLUR   :' + '   0.00' + DATA_LINE_SEPARATOR_01; // Blur point (IGNORED)
      DataToSend:= DataToSend + 'VIS_S_B:' + '      0' + DATA_LINE_SEPARATOR_01; // Uncorrected (S.C) binocular visual acuity (IGNORED)
      DataToSend:= DataToSend + 'VIS_C_B:' + '      0' + DATA_LINE_SEPARATOR_01; // Corrected (C.C.) binocular visual acuity (IGNORED)   
      
      DataToSend:= DataToSend + 'PATNAME:' + DATA_LINE_SEPARATOR_01;
      DataToSend:= DataToSend + 'PAT_ID :' + DATA_LINE_SEPARATOR_01;             
    end;
        
    DataToSend:= DataToSend + DATA_ETX;
        
    // send all data to the device (Computer -> Visutron 900 Touch)
    if (DataToSend <> '') and (bolPhoropterDataFound) then
    begin
      DoCOMPSSendData(DataToSend);
    end;
    
    if (FErrorOccurred) then
    begin
      Exit;
    end;      
  end;  
  
  // DoCOMPSSaveFileS('ComDataToSend.txt', DataToSend); 
  
  // ---------------------------------------------------------------------------------
  //
  // ---------------------------------------------------------------------------------

  // skip waiting for ack or nak if we skip sending data to the device
  if (not bolDisableSendingOfData) then
  begin
    // A data output message is sent by the Visutron 900 Touch when
    // the OUT button on the Controller keyboard is pressed. Or if
    // we send data to the device, the device respond with ACK or NAK.
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
        if (FCOMBuffer[Length(FCOMBuffer)-1] = DATA_ACK_RESULT) then
        begin
          // T2WMessageBoxS(FCOMBufferString);
        end
        else if (FCOMBuffer[Length(FCOMBuffer)-1] = DATA_NAK_RESULT) then
        begin
          if (not bolDisableNakHandling) then
          begin
            FLastCOMErrorCode := -10;
            FLastCOMErrorMessage := 'NAK vom Gerät empfangen';

            DoCOMPSError();
            Exit;
          end;
        end      
        else
        begin
          Exit;
        end;        
      end; 
    end;
  end;

  // -----------------------------------------------------------------------------

  // second receive serial data from device
  // after receiving ack we have to wait for the real measurement
  DoCOMPSReceiveData();

  if FGlobalTimeoutReached then
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
end.