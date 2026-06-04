const
  VERSION = '1.0.55.56';
  DATE = '28.05.2024 15:09:43';
  TEXT = 'Copyright (c) 2024 team2work GmbH';
	
  DATA_SEPARATOR_01 = #$0D#$0A;
  DATA_SEPARATOR_02 = ' ';

  LINE_IDENTIFIER_REF = '$REF';
  LINE_IDENTIFIER_KER = '$KER';

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
  bolUseStrictValueChecking, bolAddPrefixForEachEyeLine, bolRefFound, bolKerFound, bolOtherValueFound: Boolean;
  bolAddSign, bolAddSignSeparator, bolAddAxisSeparator: Boolean;
  ResultLine, AdvResultLine, strGDT_LINE_PREFIX, Data, Sep1, Sep2, ParsedData, S1: String;
  R_R1, R_D1, R_AX1, R_R2, R_D2, R_AX2, R_RA, R_RAD, R_C, R_AX: String;
  L_R1, L_D1, L_AX1, L_R2, L_D2, L_AX2, L_RA, L_RAD, L_C, L_AX: String;

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
  bolOtherValueFound:= False;

  R_R1:= '';
  R_D1:= '';
  R_AX1:= '';
  R_R2:= '';
  R_D2:= '';
  R_AX2:= '';
  R_RA:= '';
  R_RAD:= '';
  R_C:= '';
  R_AX:= '';
  
  L_R1:= '';
  L_D1:= '';
  L_AX1:= '';
  L_R2:= '';
  L_D2:= '';
  L_AX2:= '';
  L_RA:= '';
  L_RAD:= '';
  L_C:= '';
  L_AX:= '';

  // Parse raw data
  for i := 0 to Length(arrData1) - 1 do
  begin
    S1:= Trim(arrData1[i]);

    if (S1 = '') then
     Continue;
     
    // T2WMessageBoxS(S1);

    if (T2WStartsStr(LINE_IDENTIFIER_REF, S1)) then
    begin
      bolRefFound:= True;
    end;
    
    if (T2WStartsStr(LINE_IDENTIFIER_KER, S1)) then
    begin
      bolKerFound:= True;
    end;
    
    if (T2WStartsStr('$PUP', S1)) then
    begin
      bolOtherValueFound := True;
      Continue;
    end;

    // R
    if (T2WStartsStr(LINE_IDENTIFIER_R, S1)) and (bolKerFound) and (not bolOtherValueFound) then
    begin
      R_R1:= Copy(S1, Pos('R1:', S1)+Length('R1:'), 5);
      R_D1:= Copy(S1, Pos('D1:', S1)+Length('D1:'), 5);
      R_AX1:= Copy(S1, Pos('AX1:', S1)+Length('AX1:'), 3);
      
      R_R2:= Copy(S1, Pos('R2:', S1)+Length('R2:'), 5);
      R_D2:= Copy(S1, Pos('D2:', S1)+Length('D2:'), 5);     
      R_AX2:= Copy(S1, Pos('AX2:', S1)+Length('AX2:'), 3);
      
      R_RA:= Copy(S1, Pos('RA:', S1)+Length('RA:'), 5);
      R_RAD:= Copy(S1, Pos('RAD:', S1)+Length('RAD:'), 5);
      R_C:= Copy(S1, Pos('C:', S1)+Length('C:'), 6);    
      R_AX:= Copy(S1, Pos('AX:', S1)+Length('AX:'), 3);
    end;
    
    // L     
    if (T2WStartsStr(LINE_IDENTIFIER_L, S1))  and (bolKerFound) and (not bolOtherValueFound) then
    begin
      L_R1:= Copy(S1, Pos('R1:', S1)+Length('R1:'), 5);
      L_D1:= Copy(S1, Pos('D1:', S1)+Length('D1:'), 5);
      L_AX1:= Copy(S1, Pos('AX1:', S1)+Length('AX1:'), 3);
      
      L_R2:= Copy(S1, Pos('R2:', S1)+Length('R2:'), 5);
      L_D2:= Copy(S1, Pos('D2:', S1)+Length('D2:'), 5);     
      L_AX2:= Copy(S1, Pos('AX2:', S1)+Length('AX2:'), 3);
      
      L_RA:= Copy(S1, Pos('RA:', S1)+Length('RA:'), 5);
      L_RAD:= Copy(S1, Pos('RAD:', S1)+Length('RAD:'), 5);
      L_C:= Copy(S1, Pos('C:', S1)+Length('C:'), 6);
      L_AX:= Copy(S1, Pos('AX:', S1)+Length('AX:'), 3);
    end;
  end;
  
  // T2WMessageBoxS('');

  // Start parsing ophthalmology data down here  

  // Build result
  ParsedData:= '';

  ResultLine:= '';    
  
  // Add right eye
  ResultLine:= 'R:';
  
  if (R_R1 <> '') then
  begin
    R_R1:= FormatSignValue('R1=', R_R1, bolAddSign, bolAddSignSeparator);
  end;
  
  if (R_AX1 <> '') then
  begin
    R_AX1:= FormatAxisValue('*', R_AX1, bolAddAxisSeparator);
  end;
   
  ResultLine:= ResultLine + ' ' + R_R1 + R_AX1;
  
  if (R_R2 <> '') then
  begin
    R_R2:= FormatSignValue('R2=', R_R2, bolAddSign, bolAddSignSeparator);
  end;      
     
  if (R_AX2 <> '') then
  begin
    R_AX2:= FormatAxisValue('*', R_AX2, bolAddAxisSeparator);
  end;
  
  ResultLine:= ResultLine + ' ' + R_R2 + R_AX2;  
  ResultLine:= ResultLine + ' ' + '//' + ' L:';

  // Add left eye
  
  if (L_R1 <> '') then
  begin
    L_R1:= FormatSignValue('R1=', L_R1, bolAddSign, bolAddSignSeparator);
  end;
  
  if (L_AX1 <> '') then
  begin
    L_AX1:= FormatAxisValue('*', L_AX1, bolAddAxisSeparator);
  end;
   
  ResultLine:= ResultLine + ' ' + L_R1 + L_AX1;
  
  if (L_R2 <> '') then
  begin
    L_R2:= FormatSignValue('R2=', L_R2, bolAddSign, bolAddSignSeparator);
  end;      
     
  if (L_AX2 <> '') then
  begin
    L_AX2:= FormatAxisValue('*', L_AX2, bolAddAxisSeparator);
  end;

  ResultLine:= ResultLine + ' ' + L_R2 + L_AX2;

	if (ResultLine <> '') then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + ResultLine + FOutputLineSeparator;
	end;
 
	AdvResultLine:= 'R: ';
	AdvResultLine:= AdvResultLine + 'CYL=' + R_C + ' // L: ' + 'CYL=' + L_C;
	
	if (AdvResultLine <> '') then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + AdvResultLine + FOutputLineSeparator;
	end;
	
	AdvResultLine:= '';
	
  if (R_AX <> '') then
  begin
    R_AX:= FormatAxisValue('*', R_AX, bolAddAxisSeparator);
  end;
  
  if (R_RA <> '') then
  begin
    R_RA:= FormatSignValue('ra=', R_RA, bolAddSign, bolAddSignSeparator);
  end;  
	
  if (L_AX <> '') then
  begin
    L_AX:= FormatAxisValue('*', L_AX, bolAddAxisSeparator);
  end;
  
  if (L_RA <> '') then
  begin
    L_RA:= FormatSignValue('ra=', L_RA, bolAddSign, bolAddSignSeparator);
  end;
  
	AdvResultLine:= 'R: ';
	AdvResultLine:= AdvResultLine + R_RA + R_AX + ' // ' + L_RA + L_AX;
  
  	
	
	// V7 R: ra=-1.50*174 				//
	
	// V7 R: CYL=- 0.50 // L: CYL=- 0.50
	
	
	// RA: 7.97 RAD:42.31 C:- 0.88 AX: 45   -> muss rein
	
	
	

	if (AdvResultLine <> '') then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + AdvResultLine + FOutputLineSeparator;
	end;
	

 
  // Set result
  FParsedDataString := RawByteString(ParsedData);
end.	