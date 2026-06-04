const
  VERSION = '1.0.55.65';
  DATE = '03.06.2024 14:55:36';
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
      
var
  bolUseStrictValueChecking, bolAddPrefixForEachEyeLine, bolAddPDValueToOutput: Boolean;
  bolAddSign, bolAddSignSeparator, bolAddAxisSeparator: Boolean;
  arrData1, arrData2, arrData3: TStringArray;
  i, j: Integer;
  Data, Sep1, Sep2, Sep3, Sep4, S1, S, ParsedData, strGDT_LINE_PREFIX, R_Line, L_Line, S3: String;
  PD, R_SPH, R_CYL, R_AXIS, L_SPH, L_CYL, L_AXIS, PD_R, PD_L: String;
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
  bolAddPDValueToOutput:= True;
      
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
  
  K_R_SPH:= '';
  K_R_CYL:= '';
  K_R_AXIS:= '';
    
  K_L_SPH:= '';
  K_L_CYL:= '';
  K_L_AXIS:= '';

  // Parse raw data
  for i := 0 to Length(arrData1) - 1 do
  begin
    S1:= Trim(arrData1[i]);

    if (S1 = '') then
     Continue;
  
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
    if (R_Line <> '') then
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