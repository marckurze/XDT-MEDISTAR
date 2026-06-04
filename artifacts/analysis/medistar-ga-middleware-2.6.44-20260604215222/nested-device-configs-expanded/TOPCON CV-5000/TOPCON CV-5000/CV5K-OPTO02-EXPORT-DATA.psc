const
  VERSION = '1.0.55.76';
  DATE = '01.12.2022 10:08:04';
  TEXT = 'Copyright (c) 2022 team2work GmbH';
  
  RIGHT_EYE_START_MARKER        = 'R.:';
  LEFT_EYE_START_MARKER         = 'L.:';
  RIGHT_EYE_START_MARKER_ARK_F  = 'F R.:';
  LEFT_EYE_START_MARKER_ARK_F   = 'F L.:';
  RIGHT_EYE_START_MARKER_ARK_N  = 'N R.:';
  LEFT_EYE_START_MARKER_ARK_N   = 'N L.:';
  RIGHT_EYE_START_MARKER_RKT_F  = 'F R.:';
  LEFT_EYE_START_MARKER_RKT_F   = 'F L.:';
  RIGHT_EYE_START_MARKER_RKT_N  = 'N R.:';
  LEFT_EYE_START_MARKER_RKT_N   = 'N L.:';

  RIGHT_EYE_START_MARKER_LM_F   = 'F R.:';
  LEFT_EYE_START_MARKER_LM_F    = 'F L.:';
  RIGHT_EYE_START_MARKER_LM_N   = 'N R.:';
  LEFT_EYE_START_MARKER_LM_N    = 'N L.:';  
  
  SPHERE_IDENTIFIER   = 'S=';
  CYLINDER_IDENTIFIER = 'Z=';
  AXIS_IDENTIFIER     = '*';
  A_IDENTIFIER        = 'A=';
  A2_IDENTIFIER       = 'A2=';
  P_IDENTIFIER        = 'P=';
  B_IDENTIFIER        = 'B=';
  PD_IDENTIFIER       = 'PD=';
  VD_IDENTIFIER       = 'VD=';
  WD_IDENTIFIER       = 'WD=';
  
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

  NumericArrayValues  = #32#43#45#46#48#49#50#51#52#53#54#55#56#57;
  NumericArrayPValues = #32#43#45#46#48#49#50#51#52#53#54#55#56#57;

  XMLTemplateFileName = 'CV5KS-OPTO02-SEND-TEMPLATE-SBJ.xml';
  XMLTemplateEncoding = 0; // 0 = UTF8, 1 = UTF16
  XMLExportFileName   = 'CVImport.xml';
  
  // Dieser Wert gibt an, in welcher Sprache die TOPCON Software
  // betrieben wird. Mögliche Werte sind DE (Deutsch) oder EN (Englisch).
  // Wenn DE dann müssen Werte mit Komma (,) getrennt sein, wenn EN dann mit Punkt (.).
  // Alternativ können die Regionseinstellungen des Gerätes geändert werden.
  // Dann kann die Software auf Deutsch bleiben und der Import funktioniert trotzdem korrekt.
  SOFTWARE_LANGUAGE = 'DE';
  
  // Hier muss/kann passend zur Sprache das Dezimaltrennzeichen eingestellt werden.
  // Das Dezimaltrennzeichen wird für den Import und Export verwendet.
  VALUE_SEPARATOR = ',';
  
function FormatValue(const Value: String): String;
begin
  if (Value = '') then
  begin
    Result:= Value;
    Exit;
  end;
  
  if not T2WContainsStr(Value, ',') then
  begin
    Result:= Value;
    Exit;
  end;
  
  Result:= Trim(T2WStringReplace(Value, ',', '.', True, True));
end; 

function FormatNumberValue(const S: String): String;
var
  S1, S2, S3: String;
begin
  Result:= S;
  
  if S = '' then
    Exit;
  
  S1:= Trim(S);
  
  if (S1[1] = '-') or (S1[1] = '+') then
  begin
    S2:= S1[1];
    S3:= Trim(Copy(S1, 2, Length(S1)));
    
    if S2 = '-' then
      Result:= S2 + S3
    else
      Result:= S3;
    
    Exit;
  end;
  
  Result:= S1;
end;

function FormatAxisValue(const S: String): String;
begin
  Result:= Trim(S);
  
  while Length(Result) > 1 do
  begin
    if Result[1] <> '0' then
      Break;
    
    Result:= Copy(Result, 2, Length(Result) - 1);
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

    Result:= i;
    Break;
  end;

  if Result = -1 then
    Result:= Length(S) + 1;
end;

function FindPreviousIdentifierLineIndex(const S: String;
                                         EndIndex: Integer;
                                         const NumericArrayValues: String): Integer;
var
  i, j: Integer;
  Found: Boolean;
begin
  Result:= -1;

  if S = '' then
    Exit;

  for i:= EndIndex downto 1 do
  begin
    Found:= False;

    for j:= 1 to Length(NumericArrayValues) do
    begin
      if S[i] = NumericArrayValues[j] then
      begin
        Found:= True;

        Break;
      end;
    end;

    if Found then
      Continue;

    Result:= i;
    Break;
  end;
  
  if Result = -1 then
    Result:= 1;
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

  DataStartIndex:= Pos(Identifier, S);

  DataEndIndex:= FindNextIdentifierLineIndex(S,
                                             DataStartIndex + Length(Identifier),
                                             NumericArrayValues);

  if DataEndIndex <= 0 then
  begin
    Data:= Result;
    Exit;
  end;

  Data:= Copy(S, DataStartIndex + Length(Identifier), (DataEndIndex - (DataStartIndex + Length(Identifier))));

  Result:= Copy(S, 1, DataStartIndex - 1) + Copy(S, DataEndIndex, Length(S));
end;

function GetDataForPIdentifier(const S: String;
                               const Identifier: String;
                               const NumericArrayValues: String;
                               var Data: String): String;
var
  Index1, Index2, DataStartIndex, DataEndIndex: Integer;
begin
  Result:= S;
  Data:= '';

  if not T2WContainsStr(S, Identifier) then
    Exit;

  DataStartIndex:= Pos(Identifier, S);
  
  // Check for new P format containing at pos 1 either "I" or "O" followed by a space char
  Index1:= Pos('I ', S);
  
  if Index1 <= 0 then
    Index1:= Pos('O ', S);
  
  if Index1 <= 0 then
  begin
    DataEndIndex:= FindNextIdentifierLineIndex(S,
                                               DataStartIndex + Length(Identifier),
                                               NumericArrayValues);
  end
  else
  begin
    // Find data index for chars 'U' or 'D'
    Index2:= Pos('U ', S);
    
    if Index2 <= 0 then
      Index2:= Pos('D ', S);
    
    if Index2 <= 0 then
    begin
      Data:= Result;
      Exit;
    end;
    
    DataEndIndex:= FindNextIdentifierLineIndex(S,
                                               Index2 + 2,
                                               NumericArrayValues);
  end;

  if DataEndIndex <= 0 then
  begin
    Data:= Result;
    Exit;
  end;

  Data:= Copy(S, DataStartIndex + Length(Identifier), (DataEndIndex - (DataStartIndex + Length(Identifier))));

  Result:= Copy(S, 1, DataStartIndex - 1) + Copy(S, DataEndIndex, Length(S));
end;

function GetPDetailValue(const S: String; const V1: String; const V2: String; var XY: String): String;
var
  DataStartIndex, DataEndIndex: Integer;
begin
  Result:= '';
  XY:= '';

  if S = '' then
    Exit;
  
  DataEndIndex:= Pos(V1, S);
  
  if DataEndIndex <= 0 then
  begin
    DataEndIndex:= Pos(V2, S);
    
    if DataEndIndex <= 0 then
      Exit;
    
    DataEndIndex:= DataEndIndex - Length(V2);
    
    if V2 = 'O' then
      XY:= 'out'
    else if V2 = 'D' then
      XY:= 'down';
  end
  else
  begin
    DataEndIndex:= DataEndIndex - Length(V1);
    
    if V1 = 'I' then
      XY:= 'in'
    else if V1 = 'U' then
      XY:= 'up';
  end;
  
  DataStartIndex:= FindPreviousIdentifierLineIndex(S,
                                                   DataEndIndex,
                                                   NumericArrayValues);

  if DataStartIndex <= 0 then
  begin
    XY:= '';
    
    Exit;
  end;
  
  Result:= Trim(Copy(S, DataStartIndex + 1, DataEndIndex - (DataStartIndex + 1)));
end;

var
  GDTDataToSend: TStringArray;
  S: String;
  i: Integer;
  PD, WD: String;
  LM_R_S, LM_R_Z, LM_R_AX, LM_R_A, LM_R_A2, LM_R_PH, LM_R_PH_X, LM_R_PV, LM_R_PV_Y, LM_R_P, LM_R_B: String;
  LM_L_S, LM_L_Z, LM_L_AX, LM_L_A, LM_L_A2, LM_L_PH, LM_L_PH_X, LM_L_PV, LM_L_PV_Y, LM_L_P, LM_L_B: String;
  AR_R_S, AR_R_Z, AR_R_AX, AR_R_PD, AR_R_VD: String;
  AR_L_S, AR_L_Z, AR_L_AX: String;
  FN_R_S, FN_R_Z, FN_R_AX, FN_R_A, FN_R_A2, FN_R_PH, FN_R_PH_X, FN_R_PV, FN_R_PV_Y, FN_R_P, FN_R_B, FN_R_PD, FN_R_VD: String;
  FN_L_S, FN_L_Z, FN_L_AX, FN_L_A, FN_L_A2, FN_L_PH, FN_L_PH_X, FN_L_PV, FN_L_PV_Y, FN_L_P, FN_L_B, FN_L_PD: String; 
  PatientID, LB, TemplateText, DateS, TimeS, RandS: String;
  bolRemoveLeadingLineWhiteSpace, bolAddPDValueFromFN: Boolean;
  ExportFileName, ExportFileNameID, ExportFileNameDateTimeFormat: String;
  bolLensmeterDataFound, bolRefraktometerDataFound, bolPhoropterDataFound, bolDisableSendingOfData: Boolean;
begin
  // Verwende "True", damit führende Leerzeichen aus jeder Datenzeile entfernt werden, 
  // benutze "False", damit führende Leerzeichen in jeder Datenzeile erhalten bleiben.
  bolRemoveLeadingLineWhiteSpace:= True;
    
  // Verwende "True", damit der PD-Wert aus den FN-Daten (auch REF) hinzugefügt wird, 
  // benutze "False", damit der PD-Wert aus den FN-Daten nicht hinzugefügt wird.
  bolAddPDValueFromFN:= True;
  
  // ID für den Export-Dateinamen, maximal 20 Zeichen
  ExportFileNameID:= '';
  
  // Verwende Dokumentation der Delphi-Funktion "SysUtils.FormatDateTime"
  ExportFileNameDateTimeFormat:= 'yyyymmddhhnnss';
 
  // --- Don't edit script down below ---
  
  bolDisableSendingOfData := False;
  
  // bolDisableSendingOfData := FDisableSendingOfData;
  
  // if not FDisableSendingOfData then
  // begin
  //   // do nothing
  // end;   
  
  SetLength(GDTDataToSend,0);

  bolLensmeterDataFound:= False;
  bolRefraktometerDataFound:= False; 
  bolPhoropterDataFound:= False; 
  
  PD:= '';
  WD:= '';
  
  LM_R_S:= '';
  LM_R_Z:= '';
  LM_R_AX:= '';
  LM_R_A:= '';
  LM_R_A2:= '';
  LM_R_PH:= '';
  LM_R_PH_X:= '';
  LM_R_PV:= '';
  LM_R_PV_Y:= '';
  LM_R_P:= '';
  LM_R_B:= '';
  
  LM_L_S:= '';
  LM_L_Z:= '';
  LM_L_AX:= '';
  LM_L_A:= '';
  LM_L_A2:= '';
  LM_L_PH:= '';
  LM_L_PH_X:= '';
  LM_L_PV:= '';
  LM_L_PV_Y:= '';
  LM_L_P:= '';
  LM_L_B:= '';
  
  AR_R_S:= '';
  AR_R_Z:= '';
  AR_R_AX:= '';
  AR_R_PD:= '';
  AR_R_VD:= '';
  
  AR_L_S:= '';
  AR_L_Z:= '';
  AR_L_AX:= '';
  
  FN_R_S:= '';
  FN_R_Z:= '';
  FN_R_AX:= '';
  FN_R_A:= '';
  FN_R_A2:= '';
  FN_R_PH:= '';
  FN_R_PH_X:= '';
  FN_R_PV:= '';
  FN_R_PV_Y:= '';
  FN_R_P:= '';
  FN_R_B:= '';
  FN_R_PD:= '';
  FN_R_VD:= '';
  
  FN_L_S:= '';
  FN_L_Z:= '';
  FN_L_AX:= '';
  FN_L_A:= '';
  FN_L_A2:= '';
  FN_L_PH:= '';
  FN_L_PH_X:= '';
  FN_L_PV:= '';
  FN_L_PV_Y:= '';
  FN_L_P:= '';
  FN_L_B:= '';
  FN_L_PD:= '';
  
  // Get "Lensmeter" data lines
  DoPSGetNonGDTDataForGroup(NonGDTDataLineLensmeter,
                            GDTDataToSend);
  
  if Length(GDTDataToSend) > 0 then
  begin
    bolLensmeterDataFound:= True;
    
    for i:= 0 to Length(GDTDataToSend) - 1 do
    begin
      S:= GDTDataToSend[i];
      
      // Remove leading white space
      if bolRemoveLeadingLineWhiteSpace then
        S:= TrimLeft(S);

      if (T2WStartsStr(RIGHT_EYE_START_MARKER_LM_F, S))
      or (T2WStartsStr(RIGHT_EYE_START_MARKER_LM_N, S)) then
      begin
        Delete(S, 1, 1);
        
        S:= TrimLeft(S);
      end;
      
      if (T2WStartsStr(LEFT_EYE_START_MARKER_LM_F, S))
      or (T2WStartsStr(LEFT_EYE_START_MARKER_LM_N, S)) then
      begin
        Delete(S, 1, 1);
        
        S:= TrimLeft(S);
      end;
      
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
        
        // Get P data
        S:= GetDataForPIdentifier(S,
                                  P_IDENTIFIER,
                                  NumericArrayValues,
                                  LM_R_P);
        
        // Get B data
        S:= GetDataForIdentifier(S,
                                 B_IDENTIFIER,
                                 NumericArrayValues,
                                 LM_R_B);
        
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
        
        // Get P details
        if LM_R_B = '' then
        begin
          LM_R_PH:= GetPDetailValue(LM_R_P, 'I', 'O', LM_R_PH_X);
          LM_R_PV:= GetPDetailValue(LM_R_P, 'U', 'D', LM_R_PV_Y);
        end;
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
        
        // Get P data
        S:= GetDataForPIdentifier(S,
                                  P_IDENTIFIER,
                                  NumericArrayValues,
                                  LM_L_P);
        
        // Get B data
        S:= GetDataForIdentifier(S,
                                 B_IDENTIFIER,
                                 NumericArrayValues,
                                 LM_L_B);
        
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
        
        // Get P details
        if LM_L_B = '' then
        begin
          LM_L_PH:= GetPDetailValue(LM_L_P, 'I', 'O', LM_L_PH_X);
          LM_L_PV:= GetPDetailValue(LM_L_P, 'U', 'D', LM_L_PV_Y);
        end;
      end;
    end;
  end;
  
  // Get "Refraktometer / Objektiv" data lines
  DoPSGetNonGDTDataForGroup(NonGDTDataLineRefraktometerObjektiv,
                            GDTDataToSend);
  
  if Length(GDTDataToSend) > 0 then
  begin
    bolRefraktometerDataFound:= True;
    
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
      end
      else if (T2WStartsStr(LEFT_EYE_START_MARKER, S))
      or (T2WStartsStr(LEFT_EYE_START_MARKER_ARK_F, S))
      or (T2WStartsStr(LEFT_EYE_START_MARKER_ARK_N, S))
      then
      begin
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
      end;
    end;
  end;
  
  // Get "Phoropter" data lines
  DoPSGetNonGDTDataForGroup(NonGDTDataLinePhoropter,
                            GDTDataToSend);
  
  if Length(GDTDataToSend) > 0 then
  begin
    bolPhoropterDataFound:= True;
    
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
        FN_R_PH:= GetPDetailValue(FN_R_P, 'I', 'O', FN_R_PH_X);
        FN_R_PV:= GetPDetailValue(FN_R_P, 'U', 'D', FN_R_PV_Y);  
      end
      else if (T2WStartsStr(LEFT_EYE_START_MARKER, S))
      or (T2WStartsStr(LEFT_EYE_START_MARKER_RKT_F, S))
      or (T2WStartsStr(LEFT_EYE_START_MARKER_RKT_N, S))
      then
      begin         
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
        FN_L_PH:= GetPDetailValue(FN_L_P, 'I', 'O', FN_L_PH_X);
        FN_L_PV:= GetPDetailValue(FN_L_P, 'U', 'D', FN_L_PV_Y);           
      end;     
    end;
  end;
  
  // Get "Verordnung" data lines
  DoPSGetNonGDTDataForGroup(NonGDTDataLineVerordnung,
                            GDTDataToSend);
  
  if Length(GDTDataToSend) > 0 then
  begin
    for i:= 0 to Length(GDTDataToSend) - 1 do
    begin
      S:= GDTDataToSend[i];
      
      // TODO:
    end;
  end;
  
  // Get "Refraktometer / Subjektiv" data lines
  DoPSGetNonGDTDataForGroup(NonGDTDataLineRefraktometerSubjektiv,
                            GDTDataToSend);
  
  if Length(GDTDataToSend) > 0 then
  begin
    for i:= 0 to Length(GDTDataToSend) - 1 do
    begin
      S:= GDTDataToSend[i];
      
      // TODO:
    end;
  end;
  
  // Get "Visus mit Korrektur" data lines
  DoPSGetNonGDTDataForGroup(NonGDTDataLineVisusKorrektur,
                            GDTDataToSend);
  
  if Length(GDTDataToSend) > 0 then
  begin
    for i:= 0 to Length(GDTDataToSend) - 1 do
    begin
      S:= GDTDataToSend[i];
      
      // TODO:
    end;
  end;
  
  // Get "Sondereinträge" data lines
  DoPSGetNonGDTDataForGroup(NonGDTDataLineSondereintraege,
                            GDTDataToSend);
  
  if Length(GDTDataToSend) > 0 then
  begin
    for i:= 0 to Length(GDTDataToSend) - 1 do
    begin
      S:= GDTDataToSend[i];
      
      // Remove leading white space
      if bolRemoveLeadingLineWhiteSpace then
        S:= TrimLeft(S);
      
      if T2WStartsStr(WD_IDENTIFIER, S) then
      begin
        WD:= '';
        
        // Get WD data
        S:= GetDataForIdentifier(S,
                                 WD_IDENTIFIER,
                                 NumericArrayValues,
                                 WD);
      end;
    end;
    
    if WD <> '' then
    begin
      // TODO: Trim(WD)
    end;
  end;
  
  // Get "Keratometer" data lines
  DoPSGetNonGDTDataForGroup(NonGDTDataLineKeratometer,
                            GDTDataToSend);
  
  if Length(GDTDataToSend) > 0 then
  begin
    for i:= 0 to Length(GDTDataToSend) - 1 do
    begin
      S:= GDTDataToSend[i];
      
      // TODO:
    end;
  end;
  
  // Get "Visus" data lines
  DoPSGetNonGDTDataForGroup(NonGDTDataLineVisus,
                            GDTDataToSend);
  
  if Length(GDTDataToSend) > 0 then
  begin
    for i:= 0 to Length(GDTDataToSend) - 1 do
    begin
      S:= GDTDataToSend[i];
      
      // TODO:
    end;
  end;
  
  // -------------------------------------------------------------------------------------
  
  // Get Patient ID
  PatientID:= '';
  
  DoPSGetPatientID(PatientID);
  
  // Open XML template file and get result as string 
  TemplateText:= '';
  LB:= '';  

  DoPSGetXMLTemplateAsText(XMLTemplateFileName, XMLTemplateEncoding, LB, TemplateText);
  
  if FErrorOccurred then
  begin
    Exit;
  end;

  DateS := GetCurrentDateTime('yyyy-mm-dd');
  TimeS := GetCurrentTime(True);
  RandS := '2452332';

  if (TemplateText <> '') then
  begin
    // Set current date stamp
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', DateS, False, False);  

    // Set current time stamp
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', TimeS, False, False);

    // Set random number
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', RandS, False, False);
  
    // Set patient ID
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', PatientID, False, False);
    
    // ------------------------------------------------------------------------------------------------------
    
    // Last Prescription - Alte Werte V2
    // 
    
    // Set right eye sphere
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', FormatNumberValue(FN_R_S), False, False);
    
    // Set right eye cylinder
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', FormatNumberValue(FN_R_Z), False, False);
    
    // Set right eye axis
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', FormatAxisValue(FN_R_AX), False, False);    
    
    // Set left eye sphere
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', FormatNumberValue(FN_L_S), False, False);
    
    // Set left eye cylinder
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', FormatNumberValue(FN_L_Z), False, False);
    
    // Set left eye axis
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', FormatAxisValue(FN_L_AX), False, False);    
        
    // Set VD
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', FormatNumberValue(FN_R_VD), False, False);    
    
    // Set PD
    if (bolAddPDValueFromFN) then
      TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', FormatNumberValue(FN_R_PD), False, False)
    else
      TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', '', False, False);    

    // Objective Data - Refraktometer V1
    // 

    // Set right eye sphere
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', FormatNumberValue(AR_R_S), False, False);

    // Set right eye cylinder
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', FormatNumberValue(AR_R_Z), False, False);
    
    // Set right eye axis
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', FormatAxisValue(AR_R_AX), False, False);    
    
    // Set left eye sphere
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', FormatNumberValue(AR_L_S), False, False);
    
    // Set left eye cylinder
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', FormatNumberValue(AR_L_Z), False, False);
    
    // Set left eye axis
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', FormatAxisValue(AR_L_AX), False, False);    
       
    // Set VD
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', FormatNumberValue(AR_R_VD), False, False);    
    
    // Set PD
    if (bolAddPDValueFromFN) then
      TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', FormatNumberValue(AR_R_PD), False, False)
    else
      TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', '', False, False);  

    // Current Spectacles - Lensmeter V0
    // 

    // Set right eye sphere
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', FormatNumberValue(LM_R_S), False, False);

    // Set right eye cylinder
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', FormatNumberValue(LM_R_Z), False, False);
    
    // Set right eye axis
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', FormatAxisValue(LM_R_AX), False, False);    
    
    // Set left eye sphere
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', FormatNumberValue(LM_L_S), False, False);
    
    // Set left eye cylinder
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', FormatNumberValue(LM_L_Z), False, False);
    
    // Set left eye axis
    TemplateText:= T2WStringReplace(TemplateText, '<T2W_REPLACE/>', FormatAxisValue(LM_L_AX), False, False);    
   
    // Store XML template text as export file
    ExportFileName:= T2WStringReplace(XMLExportFileName, '%s', ExportFileNameID, False, False);
    ExportFileName:= T2WStringReplace(ExportFileName, '%s', GetCurrentDateTime(ExportFileNameDateTimeFormat), False, False);
    
    if (bolPhoropterDataFound) 
    or (bolLensmeterDataFound) 
    or (bolRefraktometerDataFound) then
    begin
      DoPSStoreXMLTemplateTextAsFile(ExportFileName, TemplateText, XMLTemplateEncoding, LB);
    end;
    
    if FErrorOccurred then
    begin
      Exit;
    end;
  end;

end.