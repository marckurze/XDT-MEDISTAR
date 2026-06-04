const
  VERSION = '1.0.55.56';
  DATE = '27.05.2024 11:53:21';
  TEXT = 'Copyright (c) 2024 team2work GmbH';
	
  DATA_SEPARATOR_01 = #$0D#$0A;
  DATA_SEPARATOR_02 = ' ';	
  
  LINE_IDENTIFIER_REF = '$REF';
  LINE_IDENTIFIER_KER = '$KER';
	
	LINE_IDENTIFIER_VD = 'VD:';
	
	LINE_IDENTIFIER_R  = 'RR';
	LINE_IDENTIFIER_L  = 'LR';

  GDT_FID_PATIENT_ID   = '3000';
  GDT_FID_MEASURE_DATA = '6228';
  GDT_FID_COMMENT      = '6227';
  
  GDT_LINE_PREFIX              = '  ';
  GDT_SIGN_SEPARATOR           = ' ';
  GDT_AXIS_SEPARATOR           = ' ';
  GDT_AXIS_SEPARATOR_MAX_COUNT = 3;
    
var
  arrData1: TStringArray;  
  i: Integer;  
  bolUseStrictValueChecking, bolAddPrefixForEachEyeLine, bolAddVDValueToOutput, bolRefFound, bolKerFound, bolR_SCA, bolL_SCA: Boolean;
  bolAddSign, bolAddSignSeparator, bolAddAxisSeparator: Boolean;
  R_Line, L_Line, strGDT_LINE_PREFIX, Data, Sep1, Sep2, ParsedData, S1: String;
  VD, R_SPH, R_CYL, R_AXIS, L_SPH, L_CYL, L_AXIS: String;	

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
  
  Result:= Result + S2 + Trim(S1);
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
 
begin

  // Verwenden Sie "True", damit alle Werte auf richtigkeit und Vollständigkeit
  // geprüft werden. Wählen Sie "False", damit die Prüfung auf die AIS Anwendung
  // verlagert wird.
  bolUseStrictValueChecking:= False;	
  	
  // Verwende "True", damit vor jeder Zeile (pro Auge) Anzahl x Leerzeichen als Prefix
  // vorangestellt werden, benutze "False", damit dies nicht geschiet.
  bolAddPrefixForEachEyeLine:= False;
  	
  // Verwende "True", damit der VD Wert an die V1 Zeile angehängt wird,
  // benutze "False", damit der VD Wert ignoriert werden kann.
  bolAddVDValueToOutput:= True;
			
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
  Sep2 := DATA_SEPARATOR_02;

  Data := String(FRawDataString);
  Data := Trim(Data);

  if (bolAddPrefixForEachEyeLine) then
  begin
    strGDT_LINE_PREFIX:= GDT_LINE_PREFIX;
  end;  	

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
  bolRefFound:= False;
  bolKerFound:= False;
  
  bolR_SCA:= False;
  bolL_SCA:= False;
  
  VD:= '';

  R_SPH:= '';
  R_CYL:= '';
  R_AXIS:= '';
  	
  L_SPH:= '';
  L_CYL:= '';
  L_AXIS:= '';  	

  // Parse raw data
  for i := 0 to Length(arrData1) - 1 do
  begin
    S1:= Trim(arrData1[i]);

    if (S1 = '') then
     Continue;
     
    if (T2WStartsStr(LINE_IDENTIFIER_REF, S1)) then
    begin
      bolRefFound:= True;
    end;
    
    if (T2WStartsStr(LINE_IDENTIFIER_KER, S1)) then
    begin
      bolKerFound:= True;
    end;    

    // VD
    if (T2WStartsStr(LINE_IDENTIFIER_VD, S1)) and (bolRefFound) and (VD = '') then
   	begin
   	  VD:= Copy(S1, Length(LINE_IDENTIFIER_VD)+1, Length(S1));
   	  	
   	  if (VD <> '') then
   	  begin
   	    VD:= 'VD= ' + VD;
   	  end;
    end;
    
    // R
    if (T2WStartsStr(LINE_IDENTIFIER_R, S1)) and (bolRefFound) and (not bolR_SCA) then
    begin
      R_SPH:= Copy(S1, Pos('S:', S1)+Length('S:'), 6);
    
      if (R_SPH <> '') then
      begin
        R_SPH:= FormatSignValue('S=', R_SPH, bolAddSign, bolAddSignSeparator);
      end;   

      R_CYL:= Copy(S1, Pos('C:', S1)+Length('C:'), 6); 

      if (R_CYL <> '') then
      begin
        R_CYL:= FormatSignValue('Z=', R_CYL, bolAddSign, bolAddSignSeparator); 
      end;	
    
      R_AXIS:= Copy(S1, Pos('AX:', S1)+Length('AX:'), 3);

      if (R_AXIS <> '') then
      begin
        R_AXIS:= FormatAxisValue('*', R_AXIS, bolAddAxisSeparator);
      end;
      
      bolR_SCA:= True;
    end;
       
    // L     
    if (T2WStartsStr(LINE_IDENTIFIER_L, S1)) and (bolRefFound) and (not bolL_SCA) then
    begin
      L_SPH:= Copy(S1, Pos('S:', S1)+Length('S:'), 6);
    
      if (L_SPH <> '') then
      begin
        L_SPH:= FormatSignValue('S=', L_SPH, bolAddSign, bolAddSignSeparator);
      end;   

      L_CYL:= Copy(S1, Pos('C:', S1)+Length('C:'), 6); 

      if (L_CYL <> '') then
      begin
        L_CYL:= FormatSignValue('Z=', L_CYL, bolAddSign, bolAddSignSeparator); 
      end;	
    
      L_AXIS:= Copy(S1, Pos('AX:', S1)+Length('AX:'), 3);

      if (L_AXIS <> '') then
      begin
        L_AXIS:= FormatAxisValue('*', L_AXIS, bolAddAxisSeparator);
      end;
      
      bolL_SCA:= True;
    end;
  end;

  // Start parsing ophthalmology data down here  

  // Build result
  ParsedData := '';
      
  // Add right eye
  R_Line:= '';       
         
	if (R_SPH <> '') or (R_CYL <> '') or (R_AXIS <> '') then
	begin
	  if (R_CYL = 'Z=+ ') then
	  begin
	    R_CYL:= '';
	  end;
	  
	  R_Line:= R_Line + 'R.:' + R_SPH + ' ' + R_CYL + R_AXIS;
	end;      
  
	// Add VD
	if (VD <> '') and (bolAddVDValueToOutput) then
	begin
	  if R_Line <> '' then
	  begin
	    R_Line:= R_Line + ' ';
	  end;
		
	  R_Line:= R_Line + VD;
	end;
         
	if (R_Line <> '') then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + Trim(R_Line) + FOutputLineSeparator;
	end;         
         
  // Add left eye
  L_Line:= '';    
          
	if (L_SPH <> '') or (L_CYL <> '') or (L_AXIS <> '') then
	begin
	  if (L_CYL = 'Z=+ ') then
	  begin
	    L_CYL:= '';
	  end;

	  L_Line:= L_Line + 'L.:' + L_SPH + ' ' + L_CYL + L_AXIS;
	end;
         
	if (L_Line <> '') then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + Trim(L_Line) + FOutputLineSeparator;
	end;

  // Set result
  FParsedDataString := RawByteString(ParsedData);
end.	