const
  VERSION = '1.0.55.63';
  DATE = '30.04.2024 14:56:30';
  TEXT = 'Copyright (c) 2024 team2work GmbH';
  
  DATA_SEPARATOR_01      = #13#10;
  DATA_SEPARATOR_02      = ' ';
  DATA_SEPARATOR_03      = ':';
  DATA_SEPARATOR_04      = 'E';
    
  LINE_IDENTIFIER        = 'S';
  LINE_IDENTIFIER_R_REF  = 'S-R-R'; 
  LINE_IDENTIFIER_L_REF  = 'S-R-L'; 
  LINE_IDENTIFIER_R_KER  = 'S-K-R'; 
  LINE_IDENTIFIER_L_KER  = 'S-K-L';   

  LINE_IDENTIFIER_T_R01  = 'T-R01';
  LINE_IDENTIFIER_T_R_A  = 'T-R-A'; 
  LINE_IDENTIFIER_P_R01  = 'P-R01';
  LINE_IDENTIFIER_P_R_A  = 'P-R-A';
  LINE_IDENTIFIER_T_L01  = 'T-L01';  
  LINE_IDENTIFIER_T_L_A  = 'T-L-A';
  LINE_IDENTIFIER_P_L01  = 'P-L01';
  LINE_IDENTIFIER_P_L_A  = 'P-L-A';

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
  arrData1, arrData2, arrData3: TStringArray;
  i, j: Integer;
  Data, Sep1, Sep2, Sep3, Sep4, S1, S, ParsedData, strGDT_LINE_PREFIX, R_Line, L_Line, S3, Tonometer_Line, Pachymeter_Line: String;
  PD, R_SPH, R_CYL, R_AXIS, L_SPH, L_CYL, L_AXIS, PD_R, PD_L: String;
  T_R01, T_R_A, P_R01, P_R_A, T_L01, T_L_A, P_L01, P_L_A: String;
  Keratometer_R_Line, Keratometer_L_Line: String;
  K_R_SPH, K_R_CYL, K_R_AXIS, K_L_SPH, K_L_CYL, K_L_AXIS: String;

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
  Sep2:= DATA_SEPARATOR_04;
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
  R_SPH:= '';
  R_CYL:= '';
  R_AXIS:= '';
    
  L_SPH:= '';
  L_CYL:= '';
  L_AXIS:= '';

  PD:= '';
  PD_R:= '';
  PD_L:= '';

  T_R01:= '';
  T_R_A:= '';
  P_R01:= '';
  P_R_A:= '';
  T_L01:= '';
  T_L_A:= '';
  P_L01:= '';
  P_L_A:= '';
  
  K_R_SPH:= '';
  K_R_CYL:= '';
  K_R_AXIS:= '';
    
  K_L_SPH:= '';
  K_L_CYL:= '';
  K_L_AXIS:= '';
  
  Tonometer_Line:= '';
  Pachymeter_Line:= '';

  // Parse raw data
  for i := 0 to Length(arrData1) - 1 do
  begin
    S1:= Trim(arrData1[i]);

    if (S1 = '') then
     Continue;
     
    // S No=00003 R R ES-R-R -00.50 -00.25 +042    E
    // S No=03139 R B ES-R-R -00.75 -00.50 +025 66 ES-R-L -00.25 -00.75 +111 66 E
    // S No=00006 B B ES-R-R +00.75 -01.25 +110 63 ES-R-L +00.25 -00.75 +077 63 ES-K-R +07.82 +07.70 +108    ES-K-L +07.90 +07.81 +033    E
          
    // T2WMessageBoxS(S1);
    
    if (T2WStartsStr(LINE_IDENTIFIER, S1)) then
    begin
      arrData2 := Explode(Sep2, S1, 0);
      
      for j := 0 to Length(arrData2) - 1 do
      begin
        S:= Trim(arrData2[j]);

        if (S = '') then
          Continue;
                
        // T2WMessageBoxS(S);
                
        // Refraktometer Right
        if (T2WStartsStr(LINE_IDENTIFIER_R_REF, S)) then
        begin
          S3:= '';
                
          arrData3 := Explode(Sep4, S, 0);

          if Length(arrData3) > 0 then
          begin
            R_SPH:= arrData3[1];
          
            if (R_SPH <> '') then
            begin
              R_SPH:= FormatSignValue('S=', Trim(R_SPH), bolAddSign, bolAddSignSeparator);
            end;
            
            R_CYL:= arrData3[2];
          
            if (R_CYL <> '') then
            begin
              R_CYL:= FormatSignValue('Z=', Trim(R_CYL), bolAddSign, bolAddSignSeparator); 
            end;
             
            R_AXIS:= arrData3[3];
  
            if (R_AXIS <> '') then
            begin
              R_AXIS:= FormatAxisValue('*', Trim(R_AXIS), bolAddAxisSeparator);
            end;
            
            if Length(arrData3) = 5 then
            begin           
              PD_R:= 'PD= ' + arrData3[4];
            end;
          end;
        end;
                
        // Refraktometer Left
        if (T2WStartsStr(LINE_IDENTIFIER_L_REF, S)) then
        begin
          S3:= '';
          
          arrData3 := Explode(Sep4, S, 0);

          if Length(arrData3) > 0 then
          begin
            L_SPH:= arrData3[1];
          
            if (L_SPH <> '') then
            begin
              L_SPH:= FormatSignValue('S=', Trim(L_SPH), bolAddSign, bolAddSignSeparator);
            end;
            
            L_CYL:= arrData3[2];
          
            if (L_CYL <> '') then
            begin
              L_CYL:= FormatSignValue('Z=', Trim(L_CYL), bolAddSign, bolAddSignSeparator); 
            end;
             
            L_AXIS:= arrData3[3];
  
            if (L_AXIS <> '') then
            begin
              L_AXIS:= FormatAxisValue('*', Trim(L_AXIS), bolAddAxisSeparator);
            end;
            
            if Length(arrData3) = 5 then
            begin
              PD_L:= 'PD= ' + arrData3[4];
            end;
          end; 
        end;

        // Keratometer Right
        if (T2WStartsStr(LINE_IDENTIFIER_R_KER, S)) then
        begin
          S3:= '';
          
          arrData3 := Explode(Sep4, S, 0);
          
          if (Length(arrData3) > 0) then
          begin
            K_R_SPH:= arrData3[1];
          
            if (K_R_SPH <> '') then
            begin
              K_R_SPH:= FormatSignValue('S=', Trim(K_R_SPH), bolAddSign, bolAddSignSeparator);
            end;
            
            K_R_CYL:= arrData3[2];
          
            if (K_R_CYL <> '') then
            begin
              K_R_CYL:= FormatSignValue('Z=', Trim(K_R_CYL), bolAddSign, bolAddSignSeparator); 
            end;
             
            K_R_AXIS:= arrData3[3];
  
            if (K_R_AXIS <> '') then
            begin
              K_R_AXIS:= FormatAxisValue('*', Trim(K_R_AXIS), bolAddAxisSeparator);
            end;
          end;
        end;
                
        // Keratometer Left
        if (T2WStartsStr(LINE_IDENTIFIER_L_KER, S)) then
        begin
          S3:= '';
          
          arrData3 := Explode(Sep4, S, 0);
          
          if Length(arrData3) > 0 then
          begin
            K_L_SPH:= arrData3[1];
          
            if (K_L_SPH <> '') then
            begin
              K_L_SPH:= FormatSignValue('S=', Trim(K_L_SPH), bolAddSign, bolAddSignSeparator);
            end;
            
            K_L_CYL:= arrData3[2];
          
            if (K_L_CYL <> '') then
            begin
              K_L_CYL:= FormatSignValue('Z=', Trim(K_L_CYL), bolAddSign, bolAddSignSeparator); 
            end;
             
            K_L_AXIS:= arrData3[3];
  
            if (K_L_AXIS <> '') then
            begin
              K_L_AXIS:= FormatAxisValue('*', Trim(K_L_AXIS), bolAddAxisSeparator);
            end;
          end;
        end;
      end;
    end;
     
    // Tonometer Value Right
    if (T2WStartsStr(LINE_IDENTIFIER_T_R01, S1)) then
    begin
      arrData2 := Explode(Sep3, S1, 0);
      
      if (Length(arrData2) >= 1)  then
      begin
        T_R01:= Trim(arrData2[1]);
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
    
    // Pachymeter Value Right
    if (T2WStartsStr(LINE_IDENTIFIER_P_R01, S1)) then
    begin
      arrData2 := Explode(Sep3, S1, 0);
      
      if (Length(arrData2) >= 1)  then
      begin
        P_R01:= Trim(arrData2[1]);
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
    
    // Tonometer Value Left
    if (T2WStartsStr(LINE_IDENTIFIER_T_L01, S1)) then
    begin
      arrData2 := Explode(Sep3, S1, 0);
      
      if (Length(arrData2) >= 1)  then
      begin
        T_L01:= Trim(arrData2[1]);
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
    
    // Pachymeter Value Left
    if (T2WStartsStr(LINE_IDENTIFIER_P_L01, S1)) then
    begin
      arrData2 := Explode(Sep3, S1, 0);
      
      if (Length(arrData2) >= 1)  then
      begin
        P_L01:= Trim(arrData2[1]);
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
  end;
  
  // Start parsing ophthalmology data down here  

  // Build result
  ParsedData := '';
      
  // Add right eye
  R_Line:= '';       
         
  if (R_SPH <> '') and (R_CYL <> '') and (R_AXIS <> '') then
  begin
    R_Line:= R_Line + 'R.:' + R_SPH + ' ' + R_CYL + R_AXIS;
  end;      

  // Add PD Global
  if (PD <> '') 
  and (bolAddPDValueToOutput) then
  begin
    if R_Line <> '' then
    begin
      R_Line:= R_Line + ' ';
    end;
    
    R_Line:= R_Line + PD;
  end;
  
  // Add PD Right
  if (PD_R <> '') 
  and (bolAddPDValueToOutput) then  
  begin
    if (R_Line <> '') then
    begin
      R_Line:= R_Line + ' ';
    end;
    
    R_Line:= R_Line + PD_R;
  end;
  
  if (R_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + R_Line + FOutputLineSeparator;
  end;         
         
  // Add left eye
  L_Line:= '';    
         
  if (L_SPH <> '') and (L_CYL <> '') and (L_AXIS <> '') then
  begin
    L_Line:= L_Line + 'L.:' + L_SPH + ' ' + L_CYL + L_AXIS;
  end;
  
  // Add PD Left
  if (PD_L <> '') 
  and (bolAddPDValueToOutput) then  
  begin
    if (L_Line <> '') then
    begin
      L_Line:= L_Line + ' ';
    end;
    
    L_Line:= L_Line + PD_L;
  end;
         
  if (L_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
  end;
  
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
  
  if (T_R01 <> '') or (T_R_A <> '') then
  begin
    Tonometer_Line:= Tonometer_Line + 'R = ' + T_R01 + ' ' + '[' + T_R_A + ']';
  end;
  
  if (T_L01 <> '') or (T_L_A <> '') then
  begin
    if (Tonometer_Line <> '') then
    begin
      Tonometer_Line:= Tonometer_Line + ' ';
    end; 
  
    Tonometer_Line:= Tonometer_Line + '// L = ' + T_L01 + '[' + T_L_A + ']';
  end;
  
  if (Tonometer_Line <> '') then
  begin
    Tonometer_Line:= Tonometer_Line + ' ' + XPATH_UNIT + ' ' + GetCurrentTime(False);
  end;
  
  if (Tonometer_Line <> '') 
  and (bolUseTonometerValues) then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_SIGNATURE + FOutputLineSeparator;
    ParsedData:= ParsedData + Tonometer_Line + FOutputLineSeparator;
  end;
  
  // Keratometer
  Keratometer_R_Line:= '';
  Keratometer_L_Line:= '';     
         
  if (K_R_SPH <> '') and (K_R_CYL <> '') and (K_R_AXIS <> '') then
  begin
    Keratometer_R_Line:= Keratometer_R_Line + 'R.:' + K_R_SPH + ' ' + K_R_CYL + K_R_AXIS;
  end;
       
  if (K_L_SPH <> '') and (K_L_CYL <> '') and (K_L_AXIS <> '') then
  begin
    Keratometer_L_Line:= Keratometer_L_Line + 'L.:' + K_L_SPH + ' ' + K_L_CYL + K_L_AXIS;
  end;
  
  if (Keratometer_R_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_RESULT + FOutputLineSeparator;
    ParsedData:= ParsedData + Keratometer_R_Line + FOutputLineSeparator;
  end; 

  if (Keratometer_L_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FOREIGN_RESULT + FOutputLineSeparator;
    ParsedData:= ParsedData + Keratometer_L_Line + FOutputLineSeparator;
  end; 

  // Set result
  FParsedDataString := RawByteString(ParsedData);
end.