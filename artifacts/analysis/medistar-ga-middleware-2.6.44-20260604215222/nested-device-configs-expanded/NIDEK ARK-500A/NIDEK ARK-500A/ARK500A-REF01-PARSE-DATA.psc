const
  VERSION = '1.0.21.78';
  DATE = '31.01.2023 10:56:21';
  TEXT = 'Copyright (c) 2023 team2work GmbH';

  IF_MODE = 'NCP10'; // NIDEK or NCP10
  
  DATA_SEPARATOR_SOH = #$01; // SOH (Start-of-Header)
  DATA_SEPARATOR_STX = #$02; // STX (Start-Text)
  DATA_SEPARATOR_ETX = #$03; // ETX (End-Text)
  DATA_SEPARATOR_ETB = #$17; // ETB (End-of-Transmission-Block) 
  DATA_SEPARATOR_EOT = #$04; // EOT (End-of-Transmission)  

  DATA_SEPARATOR_01  = #$17; // ETB

  GDT_FID_PATIENT_ID = '3000';
  GDT_FID_MEASURE_DATA = '6228';
  GDT_FID_COMMENT = '6227';
  GDT_FID_FILE_ARCHIVE_NUMBER = '6302';
  GDT_FID_FILE_FORMAT = '6303';
  GDT_FID_FILE_DESCRIPTION = '6304';
  GDT_FID_FILE_URL = '6305';    
    
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

var
  Data, Sep1, S1, ParsedData, PatientID: String;
  arrData1: TStringArray;
  i: Integer;
  bolAddSign, bolAddSignSeparator, bolAddAxisSeparator: Boolean;
  bolRMdataFound, bolAddPrismAndBaseToOutput, bolAddVDValueToOutput, bolAddPDValueToOutput: Boolean;
  R_Line, L_Line: String;
  VD, WD, PD: String;
  L_S, L_Z, L_Axis, L_PD, R_S, R_Z, R_Axis, R_PD: String;
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
			
  // Verwende "True", damit der VD Wert an die V1 Zeile angehängt wird,
  // benutze "False", damit der VD Wert ignoriert werden kann.
  bolAddVDValueToOutput := True;

  // Verwende "True", damit der PD Wert an die V1 Zeile angehängt wird,
  // benutze "False", damit der PD Wert ignoriert werden kann.
  bolAddPDValueToOutput := True;
			
  // --- Don't edit script down below --- 	
	
  // Clear parsed data string
  FParsedDataString:= '';

  if Length(FRawDataString) <= 0 then
  begin
    FLastErrorCode:= -2;
    FLastErrorMessage:= 'Keine Daten für die Verarbeitung der Daten gefunden';

    DoPSError();
 
    Exit;
  end;

  Sep1:= DATA_SEPARATOR_01;

  Data:= String(FRawDataString);
  Data:= Trim(Data);

  // Get array from 1st separator
  arrData1:= Explode(Sep1, Data, 0);

  if Length(arrData1) <= 0 then
  begin
    FLastErrorCode:= -3;
    FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Keine Parameter gefunden';

    DoPSError();

    Exit;
  end;

  // Reset tmp vars
  bolRMdataFound:= False;
  
  PatientID:= '';
  	
  VD:= '';
  WD:= '';
  PD:= '';
  	
  L_S:= '';
  L_Z:= ''; 
  L_Axis:= '';
  L_PD:= '';
  
  R_S:= '';
  R_Z:= '';
  R_Axis:= '';
  R_PD:= '';		

  // Parse raw data
  for i := 0 to Length(arrData1) - 1 do
  begin
    S1 := arrData1[i];

    if (Trim(S1) = '') then
     Continue;
    
    // T2WMessageBoxS(S1);
    
    // check if we have some Refraktometer data here
    if T2WStartsStr('DRM', S1) then
    begin
      // AR measurement data
      bolRMdataFound:= True;
      	
      Continue; 
    end;
    
    // Patient ID
    if T2WStartsStr('IP', S1) then
    begin
      PatientID:= Copy(S1, 3, Length(S1)-2);
            
      Continue;      
    end;
    
    // VD
    if T2WStartsStr('VD', S1) then
    begin
      VD:= Copy(S1, 3, 5); 
            
      Continue;
    end;    
    
    // WD
    if T2WStartsStr('WD', S1) then
    begin
      WD:= Copy(S1, 3, 2); 
      	
      Continue;
    end;
    
    // Left eye data
    if T2WStartsStr('OL', S1) then    
    begin  
      L_S:= Copy(S1, 3, 6);  
      L_Z:= Copy(S1, 9, 6); 
      L_Axis:= Copy(S1, 15, 3);
      		
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
      
      if (Trim(R_Axis) <> '') then 	
      begin	
        L_Axis := Trim(L_Axis);
        L_Axis := FormatAxisValue('*', L_Axis, bolAddAxisSeparator);      	      	      	 
      end;
      
      Continue;
    end;
    
    // Right eye data
    if T2WStartsStr('OR', S1) then    
    begin
      R_S:= Copy(S1, 3, 6);  
      R_Z:= Copy(S1, 9, 6); 
      R_Axis:= Copy(S1, 15, 3);
      	      	      	
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
    
    // PD
    if T2WStartsStr('PD', S1) then
    begin
      PD:= Copy(S1, 3, 2);
      L_PD:= Copy(S1, 5, 2);
      R_PD:= Copy(S1, 7, 2);
      
      Continue;
    end; 
  end;
  
	// Build result
	ParsedData:= '';
		
	if (PatientID <> '') then
	begin
	  S1:= PatientID;
	  
	  PatientID:= '';
	  PatientID:= PatientID + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
	  PatientID:= PatientID + S1 + FOutputLineSeparator;
	end;		 
 
	// Add right eye
	R_Line:= '';

	if (R_S <> '') and (R_Z <> '') and (R_Axis <> '') then
	begin
	  R_Line:= R_Line + 'R.:' + R_S + ' ' + R_Z + R_Axis;
	end;
	
	if (R_PD <> '') then
	begin
	  if (R_Line <> '') then
	  begin
	    R_Line:= R_Line + ' ';
	  end;	
	
	  R_Line:= R_Line + 'PD= ' + R_PD;
	end;
	
	if (PD <> '') and ((R_PD = '')) then
	begin
	  if (R_Line <> '') then
	  begin
	    R_Line:= R_Line + ' ';
	  end;	
	
	  R_Line:= R_Line + 'PD= ' + PD;	
	end; 	

	if (R_Line <> '') then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + R_Line + FOutputLineSeparator;
	end;

	// Add left eye
	L_Line:= '';

	if (L_S <> '') and (L_Z <> '') and (L_Axis <> '') then
	begin
	  L_Line:= L_Line + 'L.:' + L_S + ' ' + L_Z + L_Axis;
	end;

	if (L_PD <> '') then
	begin
	  if (L_Line <> '') then
	  begin
	    L_Line:= L_Line + ' ';
	  end;	
	
	  L_Line:= L_Line + 'PD= ' + L_PD;
	end; 

	if (L_Line <> '') then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
	end;
	
	// if no measurement data was transferd or processed, we have to create an empty set of gdt data
	if (ParsedData = '') then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + '';
	end;	

  // Set result
  FParsedDataString := RawByteString(ParsedData);
end.