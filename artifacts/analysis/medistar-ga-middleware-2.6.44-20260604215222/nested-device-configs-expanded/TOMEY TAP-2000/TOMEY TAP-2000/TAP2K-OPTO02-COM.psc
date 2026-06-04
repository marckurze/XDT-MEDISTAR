const
  VERSION = '1.0.55.65';
  DATE = '24.05.2024 13:10:24';
  TEXT = 'Copyright (c) 2024 team2work GmbH';
  
  DATA_SEPARATOR_01 = #$01; // SOH (Start-of-Header)
  DATA_SEPARATOR_02 = #$02; // STX (Start-Text)
  DATA_SEPARATOR_03 = #$03; // ETX (End-Text)
  DATA_SEPARATOR_04 = #$04; // EOT (End-of-Transmission)
  DATA_SEPARATOR_05 = #$17; // ETB (End-of-Transmission-Block)
  DATA_SEPARATOR_06 = #$2A; // Star Char (*)
  DATA_SEPARATOR_07 = #$7C; // Pipe Char (|)
  
  DATA_LINE_SEPARATOR_01 = #$0A;

  DATA_HEADER_NAME = 'Phoromat 2000';
  DATA_HEADER_ID = '000000001';
  DATA_HEADER_TIME = '0';
  
  RIGHT_EYE_START_MARKER = 'R.:';
  LEFT_EYE_START_MARKER = 'L.:';
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
      S2:= ' ' + S2;
    
    Result:= S2 + S3;
    
    Exit;
  end;
  
  Result:= '+';
  
  Sep1Pos:= Pos('.', S1);
  
  if Sep1Pos = 2 then
    Result:= ' ' + Result;
  
  Result:= Result + S1;
end;

function FormatAxisValue(const S: String): String;
begin
  Result:= Trim(S);
  
  while Length(Result) < 3 do
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

function GetPDetailValue(const S: String; const P: String; const V1: String; const V2: String): String;
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

function AddPToOutput(const ID: String;
                      const R_Data: String;
                      const L_Data: String;
                      UseAlternativePFormat: Boolean): String;
var
  S1, S2: String;
begin
  Result:= DATA_SEPARATOR_02 + ID + DATA_SEPARATOR_07
           ;
  
  if R_Data <> '' then
  begin
    S1:= Trim(Copy(R_Data, Length(R_Data) - 1, 2));
    S2:= Copy(R_Data, 1, Length(R_Data) - 2);
    
    if not UseAlternativePFormat then
    begin
      Result:= Result + 
               'B' + S1 + ' ' + S2
               ;
    end
    else
    begin
      Result:= Result + 
               S1 + DATA_SEPARATOR_07 + S2
               ;
    end;
  end;
  
  Result:= Result + 
           DATA_SEPARATOR_07
           ;
  
  if L_Data <> '' then
  begin
    S1:= Trim(Copy(L_Data, Length(L_Data) - 1, 2));
    S2:= Copy(L_Data, 1, Length(L_Data) - 2);
    
    if not UseAlternativePFormat then
    begin
      Result:= Result + 
               'B' + S1 + ' ' + S2
               ;
    end
    else
    begin
      Result:= Result + 
               S1 + DATA_SEPARATOR_07 + S2
               ;
    end;
  end;
  
  Result:= Result + 
           DATA_SEPARATOR_07
           ;
  
  Result:= Result + 
           DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
           ;
end;

var
  GDTDataToSend: TStringArray;
  DataToSend, DataHeader, DataFooter, DataPD, DataWD, DataLM, DataAR, DataSJ, DataFN, DataKM, DataAV, DataTime, S: String;
  i: Integer;
  PD, WD: String;
  LM_R_S, LM_R_Z, LM_R_AX, LM_R_A, LM_R_A2, LM_R_PH, LM_R_PV, LM_R_P: String;
  LM_L_S, LM_L_Z, LM_L_AX, LM_L_A, LM_L_A2, LM_L_PH, LM_L_PV, LM_L_P: String;
  AR_R_S, AR_R_Z, AR_R_AX, AR_R_A, AR_R_A2, AR_R_PH, AR_R_PV, AR_R_P, AR_R_PD, AR_R_VD: String;
  AR_L_S, AR_L_Z, AR_L_AX, AR_L_A, AR_L_A2, AR_L_PH, AR_L_PV, AR_L_P, AR_L_PD: String;
  FN_R_S, FN_R_Z, FN_R_AX, FN_R_A, FN_R_A2, FN_R_PH, FN_R_PV, FN_R_P, FN_R_PD: String;
  FN_L_S, FN_L_Z, FN_L_AX, FN_L_A, FN_L_A2, FN_L_PH, FN_L_PV, FN_L_P, FN_L_PD: String;
  
  bolRemoveLeadingLineWhiteSpace, bolUseAlternativePOutputFormat, bolAddEmptyPHAndPVValues: Boolean;
  bolAddPDValueFromAR, bolAddPDValueFromFN, bolUseAlternativeTimeFormat: Boolean;
begin
  // Verwende "True", damit führende Leerzeichen aus jeder Datenzeile entfernt werden, 
  // benutze "False", damit führende Leerzeichen in jeder Datenzeile erhalten bleiben.
  bolRemoveLeadingLineWhiteSpace:= True;
  
  // Verwende "True", damit die Formate "*PH|<I|O>|<Wert>|<U|D>|<Wert>|", 
  // "*PV|<I|O>|<Wert>|<U|D>|<Wert>|" verwendet werden, 
  // benutze "False", damit die Formate "*PH|B<I|O> <Wert>|B<U|D> <Wert>|", 
  // "*PV|B<I|O> <Wert>|B<U|D> <Wert>|" verwendet werden.
  bolUseAlternativePOutputFormat:= False;
  
  // Verwende "True", damit leere PH- und PV-Zeilen hinzugefügt werden, 
  // benutze "False", damit leere PH- und PV-Zeilen nicht hinzugefügt werden.
  bolAddEmptyPHAndPVValues:= True;
  
  // Verwende "True", damit der PD-Wert aus den AR-Daten hinzugefügt wird, 
  // benutze "False", damit der PD-Wert aus den AR-Daten nicht hinzugefügt wird.
  bolAddPDValueFromAR:= False;
  
  // Verwende "True", damit der PD-Wert aus den FN-Daten hinzugefügt wird, 
  // benutze "False", damit der PD-Wert aus den FN-Daten nicht hinzugefügt wird.
  bolAddPDValueFromFN:= True;
  
  // Verwende "True", damit in der Zeile für die Zeitangabe das Jahr mit 
  // 2 Zahlen angegeben wird, 
  // benutze "False", damit in der Zeile für die Zeitangabe das Jahr mit 
  // 4 Zahlen angegeben wird.
  bolUseAlternativeTimeFormat:= True;
 
  // --- Don't edit script down below ---
  
  SetLength(GDTDataToSend,
            0);
  DataToSend:= '';
  DataHeader:= DATA_SEPARATOR_01 + '*PC_SND_S' + DATA_SEPARATOR_04 + DATA_LINE_SEPARATOR_01 +
               DATA_SEPARATOR_02 + '*' + DATA_HEADER_NAME + DATA_SEPARATOR_07 + DATA_HEADER_ID + DATA_SEPARATOR_07 + DATA_HEADER_TIME + DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
               ;
  DataFooter:= DATA_SEPARATOR_01 + '*PC_SND_E' + DATA_SEPARATOR_04 + DATA_LINE_SEPARATOR_01
               ;
  
  DataPD:= '';
  DataWD:= '';
  DataLM:= DATA_SEPARATOR_02 + '*LM' + DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
           ;
  DataAR:= DATA_SEPARATOR_02 + '*AR' + DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
           ;
  DataSJ:= DATA_SEPARATOR_02 + '*SJ' + DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
           ;
  DataFN:= DATA_SEPARATOR_02 + '*FN' + DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
           ;
  DataKM:= DATA_SEPARATOR_02 + '*KM' + DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
           ;
  DataAV:= DATA_SEPARATOR_02 + '*AV' + DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
           ;
  if not bolUseAlternativeTimeFormat then
    DataTime:= DATA_SEPARATOR_02 + '*TIME' + DATA_SEPARATOR_07 + GetCurrentDateTime('yyyy"/"mm"/"dd hh":"nn":"ss') + DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
  else
    DataTime:= DATA_SEPARATOR_02 + '*TIME' + DATA_SEPARATOR_07 + GetCurrentDateTime('00yy"/"mm"/"dd hh":"nn":"ss') + DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
               ;
  
  PD:= '';
  WD:= '';
  
  LM_R_S:= '';
  LM_R_Z:= '';
  LM_R_AX:= '';
  LM_R_A:= '';
  LM_R_A2:= '';
  LM_R_PH:= '';
  LM_R_PV:= '';
  LM_R_P:= '';
  
  LM_L_S:= '';
  LM_L_Z:= '';
  LM_L_AX:= '';
  LM_L_A:= '';
  LM_L_A2:= '';
  LM_L_PH:= '';
  LM_L_PV:= '';
  LM_L_P:= '';
  
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
  
  AR_L_S:= '';
  AR_L_Z:= '';
  AR_L_AX:= '';
  AR_L_A:= '';
  AR_L_A2:= '';
  AR_L_PH:= '';
  AR_L_PV:= '';
  AR_L_P:= '';
  AR_L_PD:= '';
  
  FN_R_S:= '';
  FN_R_Z:= '';
  FN_R_AX:= '';
  FN_R_A:= '';
  FN_R_A2:= '';
  FN_R_PH:= '';
  FN_R_PV:= '';
  FN_R_P:= '';
  FN_R_PD:= '';
  
  FN_L_S:= '';
  FN_L_Z:= '';
  FN_L_AX:= '';
  FN_L_A:= '';
  FN_L_A2:= '';
  FN_L_PH:= '';
  FN_L_PV:= '';
  FN_L_P:= '';
  FN_L_PD:= '';
  
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
    
    if (LM_R_S <> '') or (LM_L_S <> '') then
    begin
      DataLM:= DataLM + 
               DATA_SEPARATOR_02 + '*SP' + 
               DATA_SEPARATOR_07 + FormatNumberValue(LM_L_S) + DATA_SEPARATOR_07 + FormatNumberValue(LM_R_S) + DATA_SEPARATOR_07 + 
               DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
               ;
    end;
    
    if (LM_R_Z <> '') or (LM_L_Z <> '') then
    begin
      DataLM:= DataLM + 
               DATA_SEPARATOR_02 + '*CY' + 
               DATA_SEPARATOR_07 + FormatNumberValue(LM_L_Z) + DATA_SEPARATOR_07 + FormatNumberValue(LM_R_Z) + DATA_SEPARATOR_07 + 
               DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
               ;
    end;
    
    if (LM_R_AX <> '') or (LM_L_AX <> '') then
    begin
      DataLM:= DataLM + 
               DATA_SEPARATOR_02 + '*AX' + 
               DATA_SEPARATOR_07 + FormatAxisValue(LM_L_AX) + DATA_SEPARATOR_07 + FormatAxisValue(LM_R_AX) + DATA_SEPARATOR_07 + 
               DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
               ;
    end;
    
    if (LM_R_A <> '') or (LM_L_A <> '') then
    begin
      DataLM:= DataLM + 
               DATA_SEPARATOR_02 + '*AD' + 
               DATA_SEPARATOR_07 + FormatNumberValue(LM_L_A) + DATA_SEPARATOR_07 + FormatNumberValue(LM_R_A) + DATA_SEPARATOR_07 + 
               DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
               ;
    end;
    
    if (LM_R_PH <> '') or (LM_L_PH <> '') then 
    begin
      DataLM:= DataLM + AddPToOutput('*PH',
                                     LM_R_PH,
                                     LM_L_PH,
                                     bolUseAlternativePOutputFormat);
    end
    else
    begin
      if bolAddEmptyPHAndPVValues then
      begin
        if not bolUseAlternativePOutputFormat then
          DataLM:= DataLM + 
                   DATA_SEPARATOR_02 + '*PH' + 
                   DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + 
                   DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
        else
          DataLM:= DataLM + 
                   DATA_SEPARATOR_02 + '*PH' + 
                   DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + 
                   DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
                   ;
      end;
    end;
    
    if (LM_R_PV <> '') or (LM_L_PV <> '') then 
    begin
      DataLM:= DataLM + AddPToOutput('*PV',
                                     LM_R_PV,
                                     LM_L_PV,
                                     bolUseAlternativePOutputFormat);
    end
    else
    begin
      if bolAddEmptyPHAndPVValues then
      begin
        if not bolUseAlternativePOutputFormat then
          DataLM:= DataLM + 
                   DATA_SEPARATOR_02 + '*PV' + 
                   DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + 
                   DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
        else
          DataLM:= DataLM + 
                   DATA_SEPARATOR_02 + '*PV' + 
                   DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + 
                   DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
                   ;
      end;
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
      
      if T2WStartsStr(RIGHT_EYE_START_MARKER, S) then
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
        S:= Copy(S, Length(RIGHT_EYE_START_MARKER) + 1, Length(S));
        
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
        if (bolAddPDValueFromAR) then
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
      else if T2WStartsStr(LEFT_EYE_START_MARKER, S) then
      begin
        AR_L_S:= '';
        AR_L_Z:= '';
        AR_L_AX:= '';
        AR_L_A:= '';
        AR_L_A2:= '';
        AR_L_PH:= '';
        AR_L_PV:= '';
        AR_L_P:= '';
        AR_L_PD:= '';
        
        // Remove eye identifier
        S:= Copy(S, Length(LEFT_EYE_START_MARKER) + 1, Length(S));
        
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
        
        // Get PD data
        if (bolAddPDValueFromAR) then
          S:= GetDataForIdentifier(S,
                                   PD_IDENTIFIER,
                                   NumericArrayValues,
                                   AR_L_PD);
      end;
    end;
    
    if (AR_R_S <> '') or (AR_L_S <> '') then
    begin
      DataAR:= DataAR + 
               DATA_SEPARATOR_02 + '*SP' + 
               DATA_SEPARATOR_07 + FormatNumberValue(AR_L_S) + DATA_SEPARATOR_07 + FormatNumberValue(AR_R_S) + DATA_SEPARATOR_07 + 
               DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
               ;
    end;
    
    if (AR_R_Z <> '') or (AR_L_Z <> '') then
    begin
      DataAR:= DataAR + 
               DATA_SEPARATOR_02 + '*CY' + 
               DATA_SEPARATOR_07 + FormatNumberValue(AR_L_Z) + DATA_SEPARATOR_07 + FormatNumberValue(AR_R_Z) + DATA_SEPARATOR_07 + 
               DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
               ;
    end;
    
    if (AR_R_AX <> '') or (AR_L_AX <> '') then
    begin
      DataAR:= DataAR + 
               DATA_SEPARATOR_02 + '*AX' + 
               DATA_SEPARATOR_07 + FormatAxisValue(AR_L_AX) + DATA_SEPARATOR_07 + FormatAxisValue(AR_R_AX) + DATA_SEPARATOR_07 + 
               DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
               ;
    end;
    
    if (AR_R_A <> '') or (AR_L_A <> '') then
    begin
      DataAR:= DataAR + 
               DATA_SEPARATOR_02 + '*AD' + 
               DATA_SEPARATOR_07 + FormatNumberValue(AR_L_A) + DATA_SEPARATOR_07 + FormatNumberValue(AR_R_A) + DATA_SEPARATOR_07 + 
               DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
               ;
    end;
    
    if (AR_R_PH <> '') or (AR_L_PH <> '') then 
    begin
      DataAR:= DataAR + AddPToOutput('*PH',
                                     AR_R_PH,
                                     AR_L_PH,
                                     bolUseAlternativePOutputFormat);
    end
    else
    begin
      if bolAddEmptyPHAndPVValues then
      begin
        if not bolUseAlternativePOutputFormat then
          DataAR:= DataAR + 
                   DATA_SEPARATOR_02 + '*PH' + 
                   DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + 
                   DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
        else
          DataAR:= DataAR + 
                   DATA_SEPARATOR_02 + '*PH' + 
                   DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + 
                   DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
                   ;
      end;
    end;
    
    if (AR_R_PV <> '') or (AR_L_PV <> '') then 
    begin
      DataAR:= DataAR + AddPToOutput('*PV',
                                     AR_R_PV,
                                     AR_L_PV,
                                     bolUseAlternativePOutputFormat);
    end
    else
    begin
      if bolAddEmptyPHAndPVValues then
      begin
        if not bolUseAlternativePOutputFormat then
          DataAR:= DataAR + 
                   DATA_SEPARATOR_02 + '*PV' + 
                   DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + 
                   DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
        else
          DataAR:= DataAR + 
                   DATA_SEPARATOR_02 + '*PV' + 
                   DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + 
                   DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
                   ;
      end;
    end;
    
    if (bolAddPDValueFromAR) then
    begin
      if (AR_R_PD <> '') then 
      begin
        if (AR_L_PD = '') then
        begin
          AR_L_PD:= AR_R_PD;
        end;
        
        DataPD:= DataPD + 
                 DATA_SEPARATOR_02 + '*PD' + 
                 DATA_SEPARATOR_07 + Trim(AR_R_PD) + DATA_SEPARATOR_07 + Trim(AR_L_PD) + DATA_SEPARATOR_07 + 
                 DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
                 ;
      end;
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
      
      if T2WStartsStr(RIGHT_EYE_START_MARKER, S) then
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
        
        // Remove eye identifier
        S:= Copy(S, Length(RIGHT_EYE_START_MARKER) + 1, Length(S));
        
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
        if bolAddPDValueFromFN then
          S:= GetDataForIdentifier(S,
                                   PD_IDENTIFIER,
                                   NumericArrayValues,
                                   FN_R_PD);
        
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
      else if T2WStartsStr(LEFT_EYE_START_MARKER, S) then
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
        S:= Copy(S, Length(LEFT_EYE_START_MARKER) + 1, Length(S));
        
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
        if bolAddPDValueFromFN then
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
    
    if (FN_R_S <> '') or (FN_L_S <> '') then
    begin
      DataFN:= DataFN + 
               DATA_SEPARATOR_02 + '*SP' + 
               DATA_SEPARATOR_07 + FormatNumberValue(FN_L_S) + DATA_SEPARATOR_07 + FormatNumberValue(FN_R_S) + DATA_SEPARATOR_07 + 
               DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
               ;
    end;
    
    if (FN_R_Z <> '') or (FN_L_Z <> '') then
    begin
      DataFN:= DataFN + 
               DATA_SEPARATOR_02 + '*CY' + 
               DATA_SEPARATOR_07 + FormatNumberValue(FN_L_Z) + DATA_SEPARATOR_07 + FormatNumberValue(FN_R_Z) + DATA_SEPARATOR_07 + 
               DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
               ;
    end;
    
    if (FN_R_AX <> '') or (FN_L_AX <> '') then
    begin
      DataFN:= DataFN + 
               DATA_SEPARATOR_02 + '*AX' + 
               DATA_SEPARATOR_07 + FormatAxisValue(FN_L_AX) + DATA_SEPARATOR_07 + FormatAxisValue(FN_R_AX) + DATA_SEPARATOR_07 + 
               DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
               ;
    end;
    
    if (FN_R_A <> '') or (FN_L_A <> '') then
    begin
      DataFN:= DataFN + 
               DATA_SEPARATOR_02 + '*AD' + 
               DATA_SEPARATOR_07 + FormatNumberValue(FN_L_A) + DATA_SEPARATOR_07 + FormatNumberValue(FN_R_A) + DATA_SEPARATOR_07 + 
               DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
               ;
    end;
    
    if (FN_R_PH <> '') or (FN_L_PH <> '') then 
    begin
      DataFN:= DataFN + AddPToOutput('*PH',
                                     FN_R_PH,
                                     FN_L_PH,
                                     bolUseAlternativePOutputFormat);
    end
    else
    begin
      if bolAddEmptyPHAndPVValues then
      begin
        if not bolUseAlternativePOutputFormat then
          DataFN:= DataFN + 
                   DATA_SEPARATOR_02 + '*PH' + 
                   DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + 
                   DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
        else
          DataFN:= DataFN + 
                   DATA_SEPARATOR_02 + '*PH' + 
                   DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + 
                   DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
                   ;
      end;
    end;
    
    if (FN_R_PV <> '') or (FN_L_PV <> '') then 
    begin
      DataFN:= DataFN + AddPToOutput('*PV',
                                     FN_R_PV,
                                     FN_L_PV,
                                     bolUseAlternativePOutputFormat);
    end
    else
    begin
      if bolAddEmptyPHAndPVValues then
      begin
        if not bolUseAlternativePOutputFormat then
          DataFN:= DataFN + 
                   DATA_SEPARATOR_02 + '*PV' + 
                   DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + 
                   DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
        else
          DataFN:= DataFN + 
                   DATA_SEPARATOR_02 + '*PV' + 
                   DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + DATA_SEPARATOR_07 + 
                   DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
                   ;
      end;
    end;
    
    if bolAddPDValueFromFN then
    begin
      if (FN_R_PD <> '') or (FN_L_PD <> '') then 
      begin
        DataPD:= DataPD + 
                 DATA_SEPARATOR_02 + '*PD' + 
                 DATA_SEPARATOR_07 + Trim(FN_R_PD) + DATA_SEPARATOR_07 + Trim(FN_L_PD) + DATA_SEPARATOR_07 + 
                 DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
                 ;
      end;
    end;
  end;
  
  // Get "Verordnung" data lines
  DoCOMPSGetNonGDTDataForGroup(NonGDTDataLineVerordnung,
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
  
  // Get "Visus mit Korrektur" data lines
  DoCOMPSGetNonGDTDataForGroup(NonGDTDataLineVisusKorrektur,
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
  DoCOMPSGetNonGDTDataForGroup(NonGDTDataLineSondereintraege,
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
      DataWD:= DataWD + 
               DATA_SEPARATOR_02 + '*WD' + 
               DATA_SEPARATOR_07 + Trim(WD) + DATA_SEPARATOR_07 + 
               DATA_SEPARATOR_05 + DATA_LINE_SEPARATOR_01
               ;
    end;
  end;
  
  // Get "Keratometer" data lines
  DoCOMPSGetNonGDTDataForGroup(NonGDTDataLineKeratometer,
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
  DoCOMPSGetNonGDTDataForGroup(NonGDTDataLineVisus,
                               GDTDataToSend);
  
  if Length(GDTDataToSend) > 0 then
  begin
    for i:= 0 to Length(GDTDataToSend) - 1 do
    begin
      S:= GDTDataToSend[i];
      
      // TODO:
    end;
  end;
  
  
  
  
  
  
  DataToSend:= '';
  
  DataToSend:= DataToSend + DataHeader;
  DataToSend:= DataToSend + DataPD;
  DataToSend:= DataToSend + DataWD;
  DataToSend:= DataToSend + DataLM;
  DataToSend:= DataToSend + DataAR;
  DataToSend:= DataToSend + DataSJ;
  DataToSend:= DataToSend + DataFN;
  DataToSend:= DataToSend + DataKM;
  DataToSend:= DataToSend + DataAV;
  DataToSend:= DataToSend + DataTime;
  DataToSend:= DataToSend + DataFooter;
  
  if DataToSend <> '' then
  begin
    DoCOMPSSendData(DataToSend);
  end;
  
  DoCOMPSReceiveData();

  if FGlobalTimeoutReached then
  begin
    if Length(FCOMBuffer) <= 0 then
      Exit;
  end;

  if FErrorOccurred then
  begin
    Exit;
  end;
end.