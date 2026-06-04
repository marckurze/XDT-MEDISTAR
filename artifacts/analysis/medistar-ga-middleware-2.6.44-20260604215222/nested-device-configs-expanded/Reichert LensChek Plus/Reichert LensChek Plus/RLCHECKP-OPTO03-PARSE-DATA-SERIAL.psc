const
  VERSION = '1.0.4.39';
  DATE = '29.09.2020 15:31:11';
  TEXT = 'Copyright (c) 2020 team2work GmbH';
	
  DATA_SEPARATOR_01 = #13#10;

  LINE_IDENTIFIER_R = '<R>';
  LINE_IDENTIFIER_L = '<L>';
  
  DATA_INDEX_S = 'S :';
  DATA_INDEX_C = 'C :';
  DATA_INDEX_A = 'A :';
  DATA_INDEX_ADD = 'ADD:';
  DATA_INDEX_P = 'P :';
 
  GDT_FID_MEASURE_DATA = '6228';
  
  DEVICE_NAME = 'RLCHECKP'; 
  
var
  Data, Sep1, S1, ParsedData, R_S, L_S, R_C, L_C, R_Axis, L_Axis, R_Prism, L_Prism, R_ADD, L_ADD, R_Line, L_Line: String;
  R_DataIndex, L_DataIndex, i: Integer;
  arrData1: TStringArray;
  
begin		
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
  R_ADD := '';
  L_ADD := '';
  R_Prism := '';
  L_Prism := '';

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
          
      Continue;
    end; 
    
    //     
    if (R_DataIndex >= 0)
    and (T2WStartsStr(DATA_INDEX_ADD,
                      S1))
    then
    begin
      R_ADD := T2WStringReplace(S1,
                              DATA_INDEX_ADD,
                              '',
                              False,
                              False);
        
      R_ADD := Trim(R_ADD);
          
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
          
      Continue;
    end; 
    
    //
    if (L_DataIndex >= 0)
    and (T2WStartsStr(DATA_INDEX_ADD,
                      S1))
    then
    begin
      L_ADD := T2WStringReplace(S1,
                              DATA_INDEX_ADD,
                              '',
                              False,
                              False);
        
      L_ADD := Trim(L_ADD);
          
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
          
      Continue;
    end;
  end;
  
  // check the values

  // Start parsing ophthalmology data down here  

  // Reset parsed data
  ParsedData := '';
  
  // Add right eye
  R_Line:= '';
	
  if (R_S <> '') and (R_C <> '') and (R_Axis <> '') then
  begin
    R_Line := R_Line + 'R.:' + 'S=' + R_S + ' ' + 'Z=' + R_C + '*' + R_Axis;
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
  
  // Add richt addition
  if (R_ADD <> '') then
  begin
    if (R_Line <> '') then
    begin
      R_Line := R_Line + ' ';
    end;  
  
    R_Line := R_Line + 'ADD= ' + R_ADD;
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
    L_Line := L_Line + 'L.:' + 'S=' + L_S + ' ' + 'Z=' + L_C + '*' + L_Axis;
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
  if (L_ADD <> '') then
  begin
    if (L_Line <> '') then
    begin
      L_Line := L_Line + ' ';
    end;  
  
    L_Line := L_Line + 'ADD= ' + L_ADD;
  end;    	
	
  if (L_Line <> '') then
  begin
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData := ParsedData + L_Line + FOutputLineSeparator;
  end;	      
          
  // Set result
  FParsedDataString := RawByteString(ParsedData);
end.	