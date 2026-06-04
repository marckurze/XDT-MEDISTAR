const
  VERSION = '1.0.21.51';
  DATE = '04.07.2025 10:14:19';
  TEXT = 'Copyright (c) 2025 CompuGroup Medical Deutschland AG';

  DATA_SEPARATOR_SOH = #$01; // SOH (Start-of-Header)
  DATA_SEPARATOR_STX = #$02; // STX (Start-Text)
  DATA_SEPARATOR_ETX = #$03; // ETX (End-Text)
  DATA_SEPARATOR_ETB = #$17; // ETB (End-of-Transmission-Block) 
  DATA_SEPARATOR_EOT = #$04; // EOT (End-of-Transmission)  

  DATA_SEPARATOR_01  = #$0D; // #$0D -> CR / #$0A -> LF

  DEVICE_IDENTIFIER       = 'LM';

  LINE_IDENTIFIER_R       = 'LR';
  LINE_IDENTIFIER_R_ADD   = 'AR';
  LINE_IDENTIFIER_R_PRISM = 'PR';  
  
  LINE_IDENTIFIER_L       = 'LL';  
  LINE_IDENTIFIER_L_ADD   = 'AL';
  LINE_IDENTIFIER_L_PRISM = 'PL'; 

  GDT_FID_PATIENT_ID     = '3000';
  GDT_FID_MEASURE_DATA   = '6228';
  GDT_FID_COMMENT        = '6227';
  GDT_FID_RESULT         = '6220';
  GDT_FID_FOREIGN_RESULT = '6221';
  GDT_FID_SIGNATURE      = '8990';
  GDT_FID_DIAG           = '6205';
    
  GDT_LINE_PREFIX    = '  ';
  GDT_SIGN_SEPARATOR = ' ';
  GDT_AXIS_SEPARATOR = ' ';
  GDT_AXIS_SEPARATOR_MAX_COUNT = 3;
      
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
  
  Result:= Result + S2 + Trim(S1);
end;

function FormatAxisValue(const ID, Value: String; AddAxisSeparator: Boolean): String;
var
  S: String;
begin
  Result := ID;
  
  S := Value;
  
  if (AddAxisSeparator) then
    while Length(S) < GDT_AXIS_SEPARATOR_MAX_COUNT do
      S := GDT_AXIS_SEPARATOR + S;
  
  Result := Result + S;
end;

function FormatPrismValue(const S: String): String;
var
  Target: String;
begin
  Target := S;

  // TODO:  

  Result := Target;
end; 

function ReplaceIdentifier(const S: String; const sIdentifier: String): String;
var
  Target: String;
begin
  Target := Trim(S);
  
  if (sIdentifier <> '') then
  begin
    Target := T2WStringReplace(Target, sIdentifier, '', False, False);
  end;

  Result := Target;
end;      
    
var
  Data, Sep1, S1, ParsedData: String;
  arrData1: TStringArray;
  i: Integer;
  bolAddSign, bolAddSignSeparator, bolAddAxisSeparator, bolAddPrismAndBaseToOutput: Boolean;
  R_Line, L_Line: String;
  R_S, R_Z, R_Axis, R_Add, R_Add2, R_Prism_x, R_Prism_y, L_S, L_Z, L_Axis, L_Add, L_Add2, L_Prism_x, L_Prism_y: String;

begin
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
    
  // Verwende "True", damit der Prism und Base Wert angefügt wird,
  // benutze "False", damit der Prism und Base Wert nicht verwendet wird
  bolAddPrismAndBaseToOutput:= True;    
      
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

  // Reset tmp vars
  R_S := '';
  R_Z := '';
  R_Axis := '';
  R_Add := '';   
  R_Add2 := '';
  R_Prism_x := '';
  R_Prism_y := '';
    
  L_S := '';
  L_Z := '';
  L_Axis := '';    
  L_Add := '';
  L_Add2 := '';
  L_Prism_x := '';
  L_Prism_y := '';
 
  // Parse raw data
  for i := 0 to Length(arrData1) - 1 do
  begin
    S1 := arrData1[i];

    if (Trim(S1) = '') then
      Continue;
     
     if (Trim(S1) <> DEVICE_IDENTIFIER) then
     begin
       // TODO:
     end;       
    
    // T2WMessageBoxS(S1);
    
    // start parsing right eye
    
    if T2WStartsStr(LINE_IDENTIFIER_R, Trim(S1)) then
    begin
      S1 := ReplaceIdentifier(S1, LINE_IDENTIFIER_R);
          
      R_S := Copy(S1, 1, 6);    
      R_Z := Copy(S1, 7, 6);    
      R_Axis := Copy(S1, 13, 3);

      if (Trim(R_S) <> '') then
      begin   
        R_S := Trim(R_S);
        R_S := FormatSignValue('S=', R_S, bolAddSign, bolAddSignSeparator);
      end;  
        
      if (Trim(R_Z) <> '') then
      begin
        R_Z := Trim(R_Z);
        R_Z := FormatSignValue('Z=', R_Z, bolAddSign, bolAddSignSeparator);
      end;
      
      if (Trim(R_Axis) <> '') then  
      begin 
        R_Axis := Trim(R_Axis);
        R_Axis := FormatAxisValue('*', R_Axis, bolAddAxisSeparator);                         
      end;
      
      Continue;   
    end;
    
    if T2WStartsStr(LINE_IDENTIFIER_R_ADD, Trim(S1)) then
    begin
      S1 := ReplaceIdentifier(S1, LINE_IDENTIFIER_R_ADD);
      
      R_Add := Copy(S1, 1, 4);
      R_Add2 := Copy(S1, 6, 4);

      if (Trim(R_Add) <> '') then
      begin
        R_Add := Trim(R_Add);
        R_Add := FormatSignValue('A=', R_Add, bolAddSign, bolAddSignSeparator);    
      end;
      
      if (Trim(R_Add2) <> '') then
      begin
        R_Add2 := Trim(R_Add2);
        R_Add2 := FormatSignValue('A=', R_Add2, bolAddSign, bolAddSignSeparator);    
      end;
      
      Continue;
    end;
    
    if (T2WStartsStr(LINE_IDENTIFIER_R_PRISM, Trim(S1))) 
    and (R_Prism_x = '')
    and (R_Prism_y = '') then
    begin
      S1 := ReplaceIdentifier(S1, LINE_IDENTIFIER_R_PRISM);
    
      R_Prism_x := Copy(S1, 1, 6);
      R_Prism_x := FormatPrismValue(R_Prism_x);
      R_Prism_x := 'P= ' + R_Prism_x; 
     
      R_Prism_y := Copy(S1, 7, 6); 
      R_Prism_y := FormatPrismValue(R_Prism_y);      
     
      if (R_Prism_x = '') then
      begin
        R_Prism_y:= 'P= ' + R_Prism_y;    
      end;
     
      Continue;
    end;

    // start parsing left eye
    
    if T2WStartsStr(LINE_IDENTIFIER_L, Trim(S1)) then
    begin
      S1 := ReplaceIdentifier(S1, LINE_IDENTIFIER_L);
      
      L_S := Copy(S1, 1, 6);  
      L_Z := Copy(S1, 7, 6); 
      L_Axis := Copy(S1, 13, 3);
  
      if (Trim(L_S) <> '') then
      begin   
        L_S := Trim(L_S);
        L_S := FormatSignValue('S=', L_S, bolAddSign, bolAddSignSeparator);
      end;  
        
      if (Trim(L_Z) <> '') then
      begin
        L_Z := Trim(L_Z);
        L_Z := FormatSignValue('Z=', L_Z, bolAddSign, bolAddSignSeparator);
      end;
      
      if (Trim(L_Axis) <> '') then  
      begin 
        L_Axis := Trim(L_Axis);
        L_Axis := FormatAxisValue('*', L_Axis, bolAddAxisSeparator);                         
      end;
      
      Continue;
    end;
    
    if T2WStartsStr(LINE_IDENTIFIER_L_ADD, Trim(S1)) then
    begin
      S1 := ReplaceIdentifier(S1, LINE_IDENTIFIER_L_ADD);
      
      L_Add := Copy(S1, 1, 4);
      L_Add2 := Copy(S1, 6, 4);
      
      if (Trim(L_Add) <> '') then
      begin
        L_Add := Trim(L_Add);
        L_Add := FormatSignValue('A=', L_Add, bolAddSign, bolAddSignSeparator);    
      end;
      
      if (Trim(L_Add2) <> '') then
      begin
        L_Add2 := Trim(L_Add2);
        L_Add2 := FormatSignValue('A=', L_Add2, bolAddSign, bolAddSignSeparator);    
      end;
           
      Continue;
    end;

    if (T2WStartsStr(LINE_IDENTIFIER_L_PRISM, Trim(S1))) 
    and (L_Prism_x = '')
    and (L_Prism_y = '') then
    begin
      S1 := ReplaceIdentifier(S1, LINE_IDENTIFIER_L_PRISM);
      
      L_Prism_x := Copy(S1, 1, 6);
      L_Prism_x := FormatPrismValue(L_Prism_x);
      L_Prism_x := 'P= ' + L_Prism_x;       
 
      L_Prism_y := Copy(S1, 7, 6); 
      L_Prism_y := FormatPrismValue(L_Prism_y);      
 
      if (L_Prism_x = '') then
      begin
        L_Prism_y := 'P= ' + L_Prism_y;    
      end; 
  
      Continue;
    end;
  end;

  // Start parsing ophthalmology data down here  

  // Reset parsed data
  ParsedData := '';

  // Add right eye
  R_Line := '';         
         
  if (R_S <> '') 
  and (R_Z <> '') 
  and (R_Axis <> '') then
  begin
    R_Line := R_Line + 'R.:' + R_S + ' ' + R_Z + R_Axis;
  end;         

  if (R_Add <> '') then
  begin
    if (R_Line <> '') then
    begin
      R_Line := R_Line + ' ';
    end;  
  
    R_Line := R_Line + R_Add;
  end;         
  
  if (bolAddPrismAndBaseToOutput) then
  begin
    if (R_Prism_x <> '') 
    or (R_Prism_y <> '') then
    begin
      if (R_Line <> '') then
      begin
        R_Line := R_Line + ' ';
      end;
   
      if (R_Prism_x <> '') then
      begin
        R_Line := R_Line + R_Prism_x;  
   
        if (R_Prism_y <> '') then
        begin
          R_Line := R_Line + ' ';
        end; 
      end;
      
      if (R_Prism_y <> '') then
      begin
        R_Line := R_Line + R_Prism_y;
      end;
    end;
  end;
  
  if (R_Line <> '') then
  begin
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData := ParsedData + R_Line + FOutputLineSeparator;
  end;

  // Add left eye
  L_Line := '';          
         
  if (L_S <> '') 
  and (L_Z <> '') 
  and (L_Axis <> '') then
  begin
    L_Line := L_Line + 'L.:' + L_S + ' ' + L_Z + L_Axis;
  end; 
   
  if (L_Add <> '') then
  begin
    if (L_Line <> '') then
    begin
      L_Line := L_Line + ' ';
    end;  
  
    L_Line := L_Line + L_Add;
  end;         
      
  if (bolAddPrismAndBaseToOutput) then
  begin
    if (L_Prism_x <> '') 
    or (L_Prism_y <> '') then
    begin
      if (L_Line <> '') then
      begin
        L_Line := L_Line + ' ';
      end;
   
      if (L_Prism_x <> '') then
      begin
        L_Line := L_Line + L_Prism_x;  
   
        if (L_Prism_y <> '') then
        begin
          L_Line := L_Line + ' ';
        end; 
      end;
      
      if (L_Prism_y <> '') then
      begin
        L_Line := L_Line + L_Prism_y;
      end;
    end;  
  end;
  
  if (L_Line <> '') then
  begin
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData := ParsedData + L_Line + FOutputLineSeparator;
  end;
          
  // Set result
  FParsedDataString := RawByteString(ParsedData);
end. 
 