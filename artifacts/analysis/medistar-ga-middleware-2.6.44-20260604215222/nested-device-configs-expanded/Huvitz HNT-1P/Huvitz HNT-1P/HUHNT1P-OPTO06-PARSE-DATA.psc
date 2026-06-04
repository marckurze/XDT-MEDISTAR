const
  VERSION = '1.0.55.64';
  DATE = '23.08.2024 11:25:45';
  TEXT = 'Copyright (c) 2024 team2work GmbH';
  
  DATA_SEPARATOR_01      = #13#10;
  DATA_SEPARATOR_02      = ' ';
  DATA_SEPARATOR_03      = ':';

  LINE_IDENTIFIER_T_R01  = 'T-R01';
  LINE_IDENTIFIER_T_R_A  = 'T-R-A'; 
  LINE_IDENTIFIER_P_R01  = 'P-R01';
  LINE_IDENTIFIER_P_R_A  = 'P-R-A';
  LINE_IDENTIFIER_T_L01  = 'T-L01';  
  LINE_IDENTIFIER_T_L_A  = 'T-L-A';
  LINE_IDENTIFIER_P_L01  = 'P-L01';
  LINE_IDENTIFIER_P_L_A  = 'P-L-A';
  LINE_IDENTIFIER_T_R02  = 'T-R02';
  LINE_IDENTIFIER_T_R03  = 'T-R03';
  LINE_IDENTIFIER_T_L02  = 'T-L02';
  LINE_IDENTIFIER_T_L03  = 'T-L03';
  LINE_IDENTIFIER_P_R02  = 'P-R02';
  LINE_IDENTIFIER_P_R03  = 'P-R03';
  LINE_IDENTIFIER_P_L02  = 'P-L02';
  LINE_IDENTIFIER_P_L03  = 'P-L03';

  GDT_FID_PATIENT_ID     = '3000';
  GDT_FID_MEASURE_DATA   = '6228';
  GDT_FID_COMMENT        = '6227';
  GDT_FID_RESULT         = '6220';
  GDT_FID_FOREIGN_RESULT = '6221';
  GDT_FID_SIGNATURE      = '8990';
  GDT_FID_DIAG           = '6205';
  
  GDT_LINE_PREFIX              = '  ';
  GDT_SIGN_SEPARATOR           = ' ';
  GDT_AXIS_SEPARATOR           = ' ';
  GDT_AXIS_SEPARATOR_MAX_COUNT = 3;
  
  XPATH_UNIT = 'mmHg';
  XPATH_UNIT_PACHY = 'μm';
    
var
  bolUseStrictValueChecking, bolAddPrefixForEachEyeLine, bolAddPDValueToOutput, bolUsePachymeterValues: Boolean;
  bolAddSign, bolAddSignSeparator, bolAddAxisSeparator, bolUseDefaultPachymeterFormat, bolUseTonometerValues: Boolean;
  arrData1, arrData2: TStringArray;
  i: Integer;
  Data, Sep1, Sep3, Sep4, S1, ParsedData, strGDT_LINE_PREFIX, Tonometer_Line, Pachymeter_Line: String;
  T_R01, T_R_A, P_R01, P_R_A, T_L01, T_L_A, P_L01, P_L_A: String;
  T_R02, T_R03, T_L02, T_L03, P_R02, P_R03, P_L02, P_L03: String;

function FormatSignValue(const ID, Value: String; AddSign: Boolean; AddSignSeparator: Boolean): String;
var
  S1, S2: String;
begin
  Result:= ID;

  S1:= Value;
  
  if (AddSign) then
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
    S2:= S2 + GDT_SIGN_SEPARATOR;
  
  Result:= Result + S2 + S1;
end;

function FormatAxisValue(const ID, Value: String; AddAxisSeparator: Boolean): String;
var
  S: String;
begin
  Result:= ID;
  
  S:= Value;
  S:= T2WStringReplace(S, '+', '', False, False);
  
  if (AddAxisSeparator) then
    while (Length(S) < GDT_AXIS_SEPARATOR_MAX_COUNT) do
      S:= GDT_AXIS_SEPARATOR + S;
  
  Result:= Result + S;
end; 

begin
  // Verwenden Sie "True", damit alle Werte auf richtigkeit und Vollständigkeit
  // geprüft werden. Wählen Sie "False", damit die Prüfung auf die AIS Anwendung
  // verlagert wird.
  bolUseStrictValueChecking:= False;
    
  // Verwende "True", damit vor jeder Zeile (pro Auge) Anzahl x Leerzeichen als Prefix
  // vorangestellt werden, benutze "False", damit dies nicht geschiet.
  bolAddPrefixForEachEyeLine:= False;

  // Verwende "True", damit der PD Wert an die Zeile angehängt wird,
  // benutze "False", damit der PD Wert ignoriert werden kann.
  bolAddPDValueToOutput:= False;  
      
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

  // Verwende "True", damit der Pachymeter Wert(e) an die Zeile angehängt wird,
  // benutze "False", damit der Pachymeter Wert(e) ignoriert werden kann.
  bolUsePachymeterValues:= True;
  
  // Verwendet "True", damit die MEDISTAR Standardformatierung verwendet wird,
  // benutze "False", damit eine "Kundenspezifische" Formatierung verwendet werden kann.
  bolUseDefaultPachymeterFormat:= True;
  
  // Verwende "True", damit der Tonometer Wert an die Zeile angehängt wird,
  // benutze "False", damit der Tonometer Wert ignoriert werden kann.
  bolUseTonometerValues:= True;

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
  
  if (bolAddPrefixForEachEyeLine) then
  begin
    strGDT_LINE_PREFIX:= GDT_LINE_PREFIX;
  end;
  
  // T2WMessageBoxS(FRawDataString);

  Sep1:= DATA_SEPARATOR_01;
  Sep3:= DATA_SEPARATOR_03;
  Sep4:= DATA_SEPARATOR_02;

  Data:= String(FRawDataString);
  Data:= Trim(Data);

  // Get array from 1st separator
  arrData1 := Explode(Sep1, Data, 0);

  if (Length(arrData1) <= 0) then
  begin
    FLastErrorCode := -1;
    FLastErrorMessage := 'Kein valides Datenformat (Messdaten): Keine Parameter gefunden';

    DoPSError();

    Exit;
  end;

  // Reset vars
  T_R01:= '';
  T_R_A:= '';
  P_R01:= '';
  P_R_A:= '';
  T_L01:= '';
  T_L_A:= '';
  P_L01:= '';
  P_L_A:= '';
  T_R02:= '';
  T_R03:= '';
  T_L02:= '';
  T_L03:= '';
  P_R02:= '';
  P_R03:= '';
  P_L02:= '';
  P_L03:= '';
  
  Tonometer_Line:= '';
  Pachymeter_Line:= '';

  // Parse raw data
  for i := 0 to Length(arrData1) - 1 do
  begin
    S1:= Trim(arrData1[i]);

    if (S1 = '') then
     Continue;
     
    // T2WMessageBoxS(S1);
     
    // Tonometer Value 1 Right
    if (T2WStartsStr(LINE_IDENTIFIER_T_R01, S1)) then
    begin
      arrData2 := Explode(Sep3, S1, 0);
      
      if (Length(arrData2) >= 1)  then
      begin
        T_R01:= Trim(arrData2[1]);
      end;
    end;
    
    // Tonometer Value 2 Right
    if (T2WStartsStr(LINE_IDENTIFIER_T_R02, S1)) then
    begin
      arrData2 := Explode(Sep3, S1, 0);
      
      if (Length(arrData2) >= 1)  then
      begin
        T_R02:= Trim(arrData2[1]);
      end;
    end;
    
    // Tonometer Value 3 Right
    if (T2WStartsStr(LINE_IDENTIFIER_T_R03, S1)) then
    begin
      arrData2 := Explode(Sep3, S1, 0);
      
      if (Length(arrData2) >= 1)  then
      begin
        T_R03:= Trim(arrData2[1]);
      end;
    end;

    // Tonometer Average Right
    if (T2WStartsStr(LINE_IDENTIFIER_T_R_A, S1)) then
    begin
      arrData2 := Explode(Sep3, S1, 0);
      
      if (Length(arrData2) >= 1)  then
      begin
        T_R_A:= Trim(arrData2[1]);
      end;
    end;
    
    // Pachymeter Value 1 Right
    if (T2WStartsStr(LINE_IDENTIFIER_P_R01, S1)) then
    begin
      arrData2 := Explode(Sep3, S1, 0);
      
      if (Length(arrData2) >= 1)  then
      begin
        P_R01:= Trim(arrData2[1]);
      end;
    end;
    
    // Pachymeter Value 2 Right
    if (T2WStartsStr(LINE_IDENTIFIER_P_R02, S1)) then
    begin
      arrData2 := Explode(Sep3, S1, 0);
      
      if (Length(arrData2) >= 1)  then
      begin
        P_R02:= Trim(arrData2[1]);
      end;
    end;
    
    // Pachymeter Value 3 Right
    if (T2WStartsStr(LINE_IDENTIFIER_P_R03, S1)) then
    begin
      arrData2 := Explode(Sep3, S1, 0);
      
      if (Length(arrData2) >= 1)  then
      begin
        P_R03:= Trim(arrData2[1]);
      end;
    end;  

    // Pachymeter Average Right
    if (T2WStartsStr(LINE_IDENTIFIER_P_R_A, S1)) then
    begin
      arrData2 := Explode(Sep3, S1, 0);
      
      if (Length(arrData2) >= 1)  then
      begin
        P_R_A:= Trim(arrData2[1]);
      end;
    end;
    
    // Tonometer Value 1 Left
    if (T2WStartsStr(LINE_IDENTIFIER_T_L01, S1)) then
    begin
      arrData2 := Explode(Sep3, S1, 0);
      
      if (Length(arrData2) >= 1)  then
      begin
        T_L01:= Trim(arrData2[1]);
      end;
    end;
    
    // Tonometer Value 2 Left
    if (T2WStartsStr(LINE_IDENTIFIER_T_L02, S1)) then
    begin
      arrData2 := Explode(Sep3, S1, 0);
      
      if (Length(arrData2) >= 1)  then
      begin
        T_L02:= Trim(arrData2[1]);
      end;
    end;
    
    // Tonometer Value 3 Left
    if (T2WStartsStr(LINE_IDENTIFIER_T_L03, S1)) then
    begin
      arrData2 := Explode(Sep3, S1, 0);
      
      if (Length(arrData2) >= 1)  then
      begin
        T_L03:= Trim(arrData2[1]);
      end;
    end;

    // Tonometer Average Left
    if (T2WStartsStr(LINE_IDENTIFIER_T_L_A, S1)) then
    begin
      arrData2 := Explode(Sep3, S1, 0);
      
      if (Length(arrData2) >= 1)  then
      begin
        T_L_A:= Trim(arrData2[1]);
      end;
    end;
    
    // Pachymeter Value 1 Left
    if (T2WStartsStr(LINE_IDENTIFIER_P_L01, S1)) then
    begin
      arrData2 := Explode(Sep3, S1, 0);
      
      if (Length(arrData2) >= 1)  then
      begin
        P_L01:= Trim(arrData2[1]);
      end;
    end;   

    // Pachymeter Value 2 Left
    if (T2WStartsStr(LINE_IDENTIFIER_P_L02, S1)) then
    begin
      arrData2 := Explode(Sep3, S1, 0);
      
      if (Length(arrData2) >= 1)  then
      begin
        P_L02:= Trim(arrData2[1]);
      end;
    end;  

    // Pachymeter Value 3 Left
    if (T2WStartsStr(LINE_IDENTIFIER_P_L03, S1)) then
    begin
      arrData2 := Explode(Sep3, S1, 0);
      
      if (Length(arrData2) >= 1)  then
      begin
        P_L03:= Trim(arrData2[1]);
      end;
    end;  

    // Pachymeter Average Left
    if (T2WStartsStr(LINE_IDENTIFIER_P_L_A, S1)) then
    begin
      arrData2 := Explode(Sep3, S1, 0);
      
      if (Length(arrData2) >= 1)  then
      begin
        P_L_A:= Trim(arrData2[1]);
      end;
    end;
    
    // TODO:
    
  end;
  
  // Start parsing ophthalmology data down here  

  // Build result
  ParsedData := '';

  // Pachymeter
  Pachymeter_Line:= '';
  
  if (bolUseDefaultPachymeterFormat) then
  begin
    if (P_R01 <> '') or (P_R_A <> '') then
    begin
      Pachymeter_Line:= Pachymeter_Line + 'PR: ' + P_R01 + ' ' + '[' + P_R_A + ']';
    end; 
  
    if (P_L01 <> '') or (P_L_A <> '') then
    begin
      if (Pachymeter_Line <> '') then
      begin
        Pachymeter_Line:= Pachymeter_Line + ' ';
      end;
      
      Pachymeter_Line:= Pachymeter_Line + '// PL: ' + P_L01 + ' ' + '[' + P_L_A + ']';
    end;
    
    if (Pachymeter_Line <> '') then
    begin
      Pachymeter_Line:= Pachymeter_Line + ' ' + XPATH_UNIT_PACHY;
    end;   
  end
  else
  begin
    if (P_R01 <> '') or (P_R_A <> '') then
    begin
      Pachymeter_Line:= Pachymeter_Line + 'R_HHD = ' + P_R01;
    end; 

    if (P_L01 <> '') or (P_L_A <> '') then
    begin
      if (Pachymeter_Line <> '') then
      begin
        Pachymeter_Line:= Pachymeter_Line + ' ';
      end;
      
      Pachymeter_Line:= Pachymeter_Line + '// L_HHD = ' + P_L01;
    end;
    
    if (Pachymeter_Line <> '') then
    begin
      Pachymeter_Line:= Pachymeter_Line + ' ' + XPATH_UNIT_PACHY + ' ' + GetCurrentTime(False);
    end; 
  end;

  if (Pachymeter_Line <> '')
  and (bolUsePachymeterValues) then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_DIAG + FOutputLineSeparator;
    ParsedData:= ParsedData + Pachymeter_Line + FOutputLineSeparator;
  end;
  
  // Tonometer
  Tonometer_Line:= '';
  
  if (T_R01 <> '') 
  or (T_R02 <> '') 
  or (T_R03 <> '') 
  or (T_R_A <> '') then
  begin
    Tonometer_Line:= Tonometer_Line + 'R = ' + T_R01 + ' ' + T_R02 + ' ' + T_R03 + ' ' + '[' + T_R_A + ']';
  end;

  if (T_L01 <> '') 
  or (T_L02 <> '') 
  or (T_L03 <> '') 
  or (T_L_A <> '') then
  begin
    if (Tonometer_Line <> '') then
    begin
      Tonometer_Line:= Tonometer_Line + ' ';
    end; 
  
    Tonometer_Line:= Tonometer_Line + '// L = ' + T_L01 + ' ' + T_L02 + ' ' + T_L03 + ' ' + '[' + T_L_A + ']';
  end;

  if (Tonometer_Line <> '') then
  begin
    Tonometer_Line:= Tonometer_Line + ' ' + XPATH_UNIT + ' ' + GetCurrentTime(False);
  end;
  
  if (Tonometer_Line <> '') 
  and (bolUseTonometerValues) then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + Tonometer_Line + FOutputLineSeparator;
  end;

  // Set result
  FParsedDataString := RawByteString(ParsedData);
end.