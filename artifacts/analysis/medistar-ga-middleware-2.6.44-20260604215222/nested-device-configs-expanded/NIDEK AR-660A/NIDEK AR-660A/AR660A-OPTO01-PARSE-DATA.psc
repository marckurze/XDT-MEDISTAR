const
  VERSION = '1.0.55.54';
  DATE = '13.02.2024 12:39:53';
  TEXT = 'Copyright (c) 2024 team2work GmbH';
	
  DATA_SEPARATOR_01    = #13;
  DATA_SEPARATOR_02    = ' ';	
	
  LINE_IDENTIFIER_OR   = 'OR';
  LINE_IDENTIFIER_CR   = 'CR';  
  LINE_IDENTIFIER_TR   = 'TR';    
  
  LINE_IDENTIFIER_OL   = 'OL';	
  LINE_IDENTIFIER_CL   = 'CL';  
  LINE_IDENTIFIER_TL   = 'TL';  	 

  LINE_IDENTIFIER_VD   = 'VD';
  LINE_IDENTIFIER_PD   = 'PD';	
  LINE_IDENTIFIER_WD   = 'WD';		

  GDT_FID_PATIENT_ID   = '3000';
  GDT_FID_MEASURE_DATA = '6228';
  GDT_FID_COMMENT      = '6227';
  
  GDT_LINE_PREFIX              = '  ';
  GDT_SIGN_SEPARATOR           = ' ';
  GDT_AXIS_SEPARATOR           = ' ';
  GDT_AXIS_SEPARATOR_MAX_COUNT = 3;  
    
var
  Data, Sep1, Sep2, S1, S, ParsedData, strGDT_LINE_PREFIX, VD, PD, WD, R_Line, L_Line: String;
  R_SPH, R_CYL, R_AXIS, L_SPH, L_CYL, L_AXIS: String;	
  arrData1: TStringArray;
  i: Integer;
  bolUseStrictValueChecking, bolAddPrefixForEachEyeLine, bolAddVDValueToOutput, bolAddPDValueToOutput: Boolean;
  bolAddSign, bolAddSignSeparator, bolAddAxisSeparator: Boolean;
 
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

  // Verwende "True", damit der PD Wert an die V1 Zeile angehängt wird,
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
  
  // T2WMessageBoxS(FRawDataString);

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
  VD:= '';
  WD:= '';
  PD:= '';
  	
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
    S:= '';

    if (S1 = '') then
     Continue;
     
    // T2WMessageBoxS(S1);
     
    // VD
    if (T2WStartsStr(LINE_IDENTIFIER_VD, S1)) then
    begin
      VD:= Copy(S1, 3, Length(S1));
   	  	
      if (VD <> '') then
      begin
        VD:= 'VD= ' + VD;
      end;
    end;
    
    // WD
    if (T2WStartsStr(LINE_IDENTIFIER_WD, S1)) then
    begin
      WD:= Copy(S1, 3, Length(S1));
    end;
    
    // L
    if (T2WStartsStr(LINE_IDENTIFIER_OL, S1)) or (T2WStartsStr(LINE_IDENTIFIER_CL, S1)) or (T2WStartsStr(LINE_IDENTIFIER_TL, S1)) then
    begin
      S:= Copy(S1, 3, Length(S1));

      L_SPH:= Copy(S, 1, 6);
      	
      if (L_SPH <> '') then
      begin
        L_SPH:= FormatSignValue('S=', L_SPH, bolAddSign, bolAddSignSeparator);
      end;
      
      L_CYL:= Copy(S, 7, 6);
      
      if (L_CYL <> '') then
      begin
        L_CYL:= FormatSignValue('Z=', L_CYL, bolAddSign, bolAddSignSeparator); 
      end;	
 	
      L_AXIS:= Copy(S, 13, 3);
      	
      if (L_AXIS <> '') then
      begin
        L_AXIS:= FormatAxisValue('*', L_AXIS, bolAddAxisSeparator);
      end;
    end;    

    // R
    if (T2WStartsStr(LINE_IDENTIFIER_OR, S1)) or (T2WStartsStr(LINE_IDENTIFIER_CR, S1)) or (T2WStartsStr(LINE_IDENTIFIER_TR, S1)) then
    begin
      S:= Copy(S1, 3, Length(S1));

      R_SPH:= Copy(S, 1, 6);
      	
      if (R_SPH <> '') then
      begin
        R_SPH:= FormatSignValue('S=', R_SPH, bolAddSign, bolAddSignSeparator);
      end;
      
      R_CYL:= Copy(S, 7, 6);
      
      if (R_CYL <> '') then
      begin
        R_CYL:= FormatSignValue('Z=', R_CYL, bolAddSign, bolAddSignSeparator); 
      end;	
 	
      R_AXIS:= Copy(S, 13, 3);
      	
      if (R_AXIS <> '') then
      begin
        R_AXIS:= FormatAxisValue('*', R_AXIS, bolAddAxisSeparator);
      end;
    end;    
 
    // PD
    if (T2WStartsStr(LINE_IDENTIFIER_PD, S1)) then
    begin
      PD:= Copy(S1, 3, Length(S1));
   	  	
      if (PD <> '') then
      begin
        PD:= 'PD= ' + PD;
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

  // Add PD
  if (PD <> '') and (bolAddPDValueToOutput) then
  begin
    if R_Line <> '' then
    begin
      R_Line:= R_Line + ' ';
    end;
		
    R_Line:= R_Line + PD;
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
    ParsedData:= ParsedData + R_Line + FOutputLineSeparator;
  end;         
         
  // Add left eye
  L_Line:= '';    
         
  if (L_SPH <> '') and (L_CYL <> '') and (L_AXIS <> '') then
  begin
    L_Line:= L_Line + 'L.:' + L_SPH + ' ' + L_CYL + L_AXIS;
  end;
         
  if (L_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
  end;
       
  // Set result
  FParsedDataString := RawByteString(ParsedData);
end.	