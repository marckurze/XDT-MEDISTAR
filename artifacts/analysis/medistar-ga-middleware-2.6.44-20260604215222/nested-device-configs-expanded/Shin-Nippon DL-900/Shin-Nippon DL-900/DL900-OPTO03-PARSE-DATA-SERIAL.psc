const
  VERSION = '1.0.55.41';
  DATE = '31.08.2022 13:54:11';
  TEXT = 'Copyright (c) 2022 team2work GmbH';
	
  DATA_SEPARATOR_01 = #13#10;

  LINE_IDENTIFIER_R = '<R>';
  LINE_IDENTIFIER_L = '<L>';
  
  DATA_INDEX_S = 'S :';
  DATA_INDEX_C = 'C :';
  DATA_INDEX_A = 'A :';
  DATA_INDEX_P = 'P :';
  DATA_INDEX_ADD = 'ADD:';  
 
  GDT_FID_MEASURE_DATA = '6228';
  
  GDT_SIGN_SEPARATOR = ' ';
  GDT_AXIS_SEPARATOR = ' ';
  GDT_AXIS_SEPARATOR_MAX_COUNT = 3;
  
function FormatSignValue(const ID, Value: String; AddSign: Boolean; AddSignSeparator: Boolean): String;
var
  S1, S2: String;
begin
  Result:= ID;

  S1:= Value;
  
  if AddSign then
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
      
  if AddSignSeparator then
    S2:= S2 + GDT_SIGN_SEPARATOR;
  
  Result:= Result + S2 + S1;
end;

function FormatAxisValue(const ID, Value: String; AddAxisSeparator: Boolean): String;
var
  S: String;
begin
  Result:= ID;
  
  S:= Value;
  
  if AddAxisSeparator then
    while Length(S) < GDT_AXIS_SEPARATOR_MAX_COUNT do
      S:= GDT_AXIS_SEPARATOR + S;
  
  Result:= Result + S;
end;  

function FormatPrismValue(const S: String): String;
var
  S1: String;
  arr_PRISM: TStringArray;
begin
  S1:= Trim(S);
  
  if (S1 <> '') then
  begin
    arr_PRISM:= Explode(' ', S1, 0);
  
    if (Length(arr_PRISM) = 2) then
    begin
      S1:= arr_PRISM[1] + ' ' + arr_PRISM[0];
    end;
    
    if (Length(arr_PRISM) = 4) then   
    begin
      S1:= arr_PRISM[1] + ' ' + arr_PRISM[0] + ' ' + arr_PRISM[3] + ' ' + arr_PRISM[2];
    end;
         
  end;
  
  Result:= Trim(S1);
end; 
    
var
  Data, Sep1, S1, ParsedData, R_S, L_S, R_C, L_C, R_Axis, L_Axis, R_Prism, R_Add, L_Prism, R_Line, L_Line, L_Add: String;
  R_DataIndex, L_DataIndex, i: Integer;
  arrData1: TStringArray;
  bolAddSign, bolAddSignSeparator, bolAddAxisSeparator: Boolean;
  
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

  // Parse raw data 
  R_DataIndex := -1;
  L_DataIndex := -1;

  R_S := '';
  L_S := '';
  R_C := '';
  L_C := '';

  R_Axis := '';
  L_Axis := '';

  R_Prism := '';
  L_Prism := '';

  R_Add := '';
  L_Add := '';

  // Parse raw data
  for i := 0 to Length(arrData1) - 1 do
  begin
    S1 := Trim(arrData1[i]);

    if (S1 = '') then
     Continue;

    // start parsing right eye

    //
    if T2WStartsStr(LINE_IDENTIFIER_R,
                    S1) then
    begin
      R_DataIndex := i;
      
      Continue;
    end;    
    
    // 
    if (R_DataIndex >= 0)
    and (T2WStartsStr(DATA_INDEX_S,
                      S1))
    then
    begin
      R_S := T2WStringReplace(S1,
                              DATA_INDEX_S,
                              '',
                              False,
                              False);
        
      R_S := Trim(R_S);
      R_S := FormatSignValue('S=', R_S, bolAddSign, bolAddSignSeparator);
          
      Continue;
    end;    
   
    //  
    if (R_DataIndex >= 0)
    and (T2WStartsStr(DATA_INDEX_C,
                      S1))
    then
    begin
      R_C := T2WStringReplace(S1,
                              DATA_INDEX_C,
                              '',
                              False,
                              False);
        
      R_C := Trim(R_C);
      R_C := FormatSignValue('Z=', R_C, bolAddSign, bolAddSignSeparator);
          
      Continue;
    end;   
    
    //     
    if (R_DataIndex >= 0)
    and (T2WStartsStr(DATA_INDEX_A,
                      S1))
    then
    begin
      R_Axis := T2WStringReplace(S1,
                                 DATA_INDEX_A,
                                 '',
                                 False,
                                 False);
        
      R_Axis := Trim(R_Axis);
      R_Axis := FormatAxisValue('*', R_Axis, bolAddAxisSeparator);
          
      Continue;
    end; 
        
    //
    if (R_DataIndex >= 0)
    and (T2WStartsStr(DATA_INDEX_P,
                      S1))
    then
    begin
      R_Prism := T2WStringReplace(S1,
                                  DATA_INDEX_P,
                                  '',
                                  False,
                                  False);
        
      R_Prism := Trim(R_Prism);     
      R_Prism := FormatPrismValue(R_Prism);    
                   
      Continue;
    end; 

    //
    if (R_DataIndex >= 0)
    and (T2WStartsStr(DATA_INDEX_ADD,
                      S1))
    then
    begin
      R_Add := T2WStringReplace(S1,
                                DATA_INDEX_ADD,
                                '',
                                False,
                                False);

      R_Add := Trim(R_Add);
      R_Add := FormatSignValue('A=', R_Add, bolAddSign, bolAddSignSeparator);
          
      Continue;
    end;     
    
    // start parsing left eye 
    
    //
    if T2WStartsStr(LINE_IDENTIFIER_L,
                    S1) then
    begin
      L_DataIndex := i;
      R_DataIndex := -1;
      
      Continue;
    end; 
    
    //
    if (L_DataIndex >= 0)
    and (T2WStartsStr(DATA_INDEX_S,
                      S1))
    then
    begin
      L_S := T2WStringReplace(S1,
                              DATA_INDEX_S,
                              '',
                              False,
                              False);
        
      L_S := Trim(L_S);
      L_S := FormatSignValue('S=', L_S, bolAddSign, bolAddSignSeparator)
          
      Continue;
    end;   
    
    //  
    if (L_DataIndex >= 0)
    and (T2WStartsStr(DATA_INDEX_C,
                      S1))
    then
    begin
      L_C := T2WStringReplace(S1,
                              DATA_INDEX_C,
                              '',
                              False,
                              False);
        
      L_C := Trim(L_C);
      L_C := FormatSignValue('Z=', L_C, bolAddSign, bolAddSignSeparator);
          
      Continue;
    end; 
    
    //    
    if (L_DataIndex >= 0)
    and (T2WStartsStr(DATA_INDEX_A,
                      S1))
    then
    begin
      L_Axis := T2WStringReplace(S1,
                                 DATA_INDEX_A,
                                 '',
                                 False,
                                 False);
        
      L_Axis := Trim(L_Axis);
      L_Axis := FormatAxisValue('*', L_Axis, bolAddAxisSeparator);
          
      Continue;
    end; 
        
    //     
    if (L_DataIndex >= 0)
    and (T2WStartsStr(DATA_INDEX_P,
                      S1))
    then
    begin
      L_Prism := T2WStringReplace(S1,
                                  DATA_INDEX_P,
                                  '',
                                  False,
                                  False);
        
      L_Prism := Trim(L_Prism);
      L_Prism := FormatPrismValue(L_Prism);
          
      Continue;
    end;
    
    //
    if (L_DataIndex >= 0)
    and (T2WStartsStr(DATA_INDEX_ADD,
                      S1))
    then
    begin
      L_Add := T2WStringReplace(S1,
                                DATA_INDEX_ADD,
                                '',
                                False,
                                False);
        
      L_Add := Trim(L_Add);
      L_Add := FormatSignValue('A=', L_Add, bolAddSign, bolAddSignSeparator);
          
      Continue;
    end;
  end;

  // Start parsing ophthalmology data down here  

  // Reset parsed data
  ParsedData := '';
  
  // Add right eye
  R_Line:= '';
	
  if (R_S <> '') and (R_C <> '') and (R_Axis <> '') then
  begin
    R_Line := R_Line + 'R.:' + R_S + ' ' + R_C + R_Axis;
  end;
  
  // Add right prism
  if (R_Prism <> '') then
  begin
    if (R_Line <> '') then
    begin
      R_Line := R_Line + ' ';
    end;
		
    R_Line := R_Line + 'P= ' + R_Prism;
  end;
  
  // Add right addition
  if (R_Add <> '') then
  begin
    if (R_Line <> '') then
    begin
      R_Line := R_Line + ' ';
    end;
		
    R_Line := R_Line + R_Add;
  end; 

  if (R_Line <> '') then
  begin
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData := ParsedData + R_Line + FOutputLineSeparator;
  end;  
      
  // Add left eye
  L_Line := '';  
  
  if (L_S <> '') and (L_C <> '') and (L_Axis <> '') then
  begin
    L_Line := L_Line + 'L.:' + L_S + ' ' + L_C + L_Axis;
  end;  

  // Add left prism
  if (L_Prism <> '') then
  begin
    if (L_Line <> '') then
    begin
      L_Line := L_Line + ' ';
    end;
		
    L_Line := L_Line + 'P= ' + L_Prism;
  end;  
  
  // Add left addition
  if (L_Add <> '') then
  begin
    if (L_Line <> '') then
    begin
      L_Line := L_Line + ' ';
    end;
		
    L_Line := L_Line + L_Add;
  end; 
  	
  if (L_Line <> '') then
  begin
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData := ParsedData + L_Line + FOutputLineSeparator;
  end;	      
          
  // Set result
  FParsedDataString := RawByteString(ParsedData);
end.	