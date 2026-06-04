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
  bolKMdataFound: Boolean;
  L_Line1, R_Line1: String;
  L_Line2, R_Line2: String;  	
  L_R1, L_R2, L_Axis, L_Ave: String;	
  R_R1, R_R2, R_Axis, R_Ave: String;
  DL_R1, DL_R2, DL_Axis, DL_Ave, DL_Cyl: String;
  DR_R1, DR_R2, DR_Axis, DR_Ave, DR_Cyl: String;
  EL_R1, EL_R2, EL_Axis, EL_Ave: String;
  ER_R1, ER_R2, ER_Axis, ER_Ave: String;
  HL_R1, HL_R2, HL_Axis, HL_Ave, HL_Cyl: String;
  HR_R1, HR_R2, HR_Axis, HR_Ave, HR_Cyl: String;
  SL_CS, SR_CS: String;
  PL_PS, PR_PS: String;

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
  FParsedDataString:= '';

  if Length(FRawDataString) <= 0 then
  begin
    FLastErrorCode:= -4;
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
    FLastErrorCode:= -5;
    FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Keine Parameter gefunden';

    DoPSError();

    Exit;
  end;
  
  // Reset tmp vars
  bolKMdataFound:= False;
  	
  PatientID:= '';
  	
  L_R1:= '';
  L_R2:= '';
  L_Axis:= '';
  L_Ave:= '';
  
  R_R1:= '';
  R_R2:= '';
  R_Axis:= '';
  R_Ave:= '';
  	
  DL_R1:= '';
  DL_R2:= '';
  DL_Axis:= '';
  DL_Ave:= ''; 
  DL_Cyl:= '';
  	
  DR_R1:= '';
  DR_R2:= '';
  DR_Axis:= '';
  DR_Ave:= ''; 
  DR_Cyl:= '';  	
  	
  EL_R1:= '';
  EL_R2:= '';
  EL_Axis:= '';
  EL_Ave:= '';  	
  	
  ER_R1:= '';
  ER_R2:= '';
  ER_Axis:= '';
  ER_Ave:= '';  	
  	
  HL_R1:= '';
  HL_R2:= '';
  HL_Axis:= '';
  HL_Ave:= ''; 
  HL_Cyl:= '';  	
  	
  HR_R1:= '';
  HR_R2:= '';
  HR_Axis:= '';
  HR_Ave:= ''; 
  HR_Cyl:= '';   	
  	
  SL_CS:= '';
  SR_CS:= '';
  
  PL_PS:= '';
  PR_PS:= '';
  
  // Parse raw data
  for i := 0 to Length(arrData1) - 1 do
  begin
    S1 := arrData1[i];

    if (Trim(S1) = '') then
     Continue;
    
    // T2WMessageBoxS(S1);
    
    // check if we have some Keratometer data here
    if T2WStartsStr('DKM', S1) then
    begin
      // KM measurement data
      bolKMdataFound:= True;
      	
      Continue; 
    end;
    
    // Patient ID
    if T2WStartsStr('IP', S1) then
    begin
      PatientID:= Copy(S1, 3, Length(S1)-2);
            
      Continue;      
    end;
    
    // KM measurement (center)
      
    //
    if T2WStartsStr(' L', S1) then
    begin
      L_R1:= Copy(S1, 3, 5);
      L_R2:= Copy(S1, 8, 5);
      L_Axis:= Copy(S1, 13, 3);
      L_Ave:= Copy(S1, 16, 5);

      Continue;
    end;
    
    //
    if T2WStartsStr('DL', S1) then
    begin
      DL_R1:= Copy(S1, 3, 5);
      DL_R2:= Copy(S1, 8, 5);
      DL_Axis:= Copy(S1, 13, 3);
      DL_Ave:= Copy(S1, 16, 5); 
      DL_Cyl:= Copy(S1, 21, 6); 
      
      Continue;
    end;
    
    // 
    if T2WStartsStr(' R', S1) then
    begin
      R_R1:= Copy(S1, 3, 5);
      R_R2:= Copy(S1, 8, 5);
      R_Axis:= Copy(S1, 13, 3);
      R_Ave:= Copy(S1, 16, 5);

      Continue;
    end;
    
    //    
    if T2WStartsStr('DR', S1) then
    begin
      DR_R1:= Copy(S1, 3, 5);
      DR_R2:= Copy(S1, 8, 5);
      DR_Axis:= Copy(S1, 13, 3);
      DR_Ave:= Copy(S1, 16, 5);
      DR_Cyl:= Copy(S1, 21, 6);
      
      Continue;
    end;
    
    // KM peripheral measurement

    // 
    if T2WStartsStr('EL', S1) then
    begin
      EL_R1:= Copy(S1, 3, 5);
      EL_R2:= Copy(S1, 8, 5);
      EL_Axis:= Copy(S1, 13, 3);
      EL_Ave:= Copy(S1, 16, 5);

      Continue;
    end;    
       
    // 
    if T2WStartsStr('HL', S1) then
    begin
      HL_R1:= Copy(S1, 3, 5);
      HL_R2:= Copy(S1, 8, 5);
      HL_Axis:= Copy(S1, 13, 3);
      HL_Ave:= Copy(S1, 16, 5); 
      HL_Cyl:= Copy(S1, 21, 6); 
      
      Continue;
    end;     
    
    //   
    if T2WStartsStr('ER', S1) then
    begin
      ER_R1:= Copy(S1, 3, 5);
      ER_R2:= Copy(S1, 8, 5);
      ER_Axis:= Copy(S1, 13, 3);
      ER_Ave:= Copy(S1, 16, 5);
      
      Continue;
    end;   
  
    // 
    if T2WStartsStr('HR', S1) then
    begin
      HR_R1:= Copy(S1, 3, 5);
      HR_R2:= Copy(S1, 8, 5);
      HR_Axis:= Copy(S1, 13, 3);
      HR_Ave:= Copy(S1, 16, 5);
      HR_Cyl:= Copy(S1, 21, 6); 
      
      Continue;
    end;
    
    // Corneal size (CS) measurement
    
    // 
    if T2WStartsStr('SL', S1) then
    begin
      SL_CS:= Copy(S1, 3, 4);
      
      Continue;
    end; 
    
    // 
    if T2WStartsStr('SR', S1) then
    begin
      SR_CS:= Copy(S1, 3, 4);  
      
      Continue;
    end;
    
    // Pupil size (PS) measurement
    
    // 
    if T2WStartsStr('PL', S1) then
    begin
      PL_PS:= Copy(S1, 3, 4); 
      
      Continue;
    end;     

    // 
    if T2WStartsStr('PR', S1) then
    begin
      PR_PS:= Copy(S1, 3, 4);
      
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
	
  // V7 R: R1=+ 7.91*100 R2=+ 7.82*10 // L: R1=+ 7.89*52 R2=+ 7.77*142
     
  // 
  R_Line1:= '';
  
  if (R_R1 <> '') and (R_R2 <> '') and (R_Axis <> '') and (R_Ave <> '') then
  begin
	  if (R_Line1 = '') then
	    R_Line1:= R_Line1 + 'R: '
	  else
	    R_Line1:= R_Line1 + '';    
    
    if (Trim(R_R1) <> '') then
    begin 	
      R_R1:= Trim(R_R1);
      R_R1:= FormatSignValue('R1=', R_R1, bolAddSign, bolAddSignSeparator);
    end;
    	
    if (Trim(R_R2) <> '') then
    begin 	
      R_R2:= Trim(R_R2);
      R_R2:= FormatSignValue('R2=', R_R2, bolAddSign, bolAddSignSeparator);
    end;    
  
    if (Trim(R_Axis) <> '') then 	
    begin	
      R_Axis:= Trim(R_Axis);
      R_Axis:= FormatAxisValue('*', R_Axis, bolAddAxisSeparator);      	      	      	 
    end;  

    R_Line1:= R_Line1 + R_R1 + R_Axis + ' ' + R_R2 + R_Axis;
  end;  
  
  // 
  L_Line1:= '';
  
  if (L_R1 <> '') and (L_R2 <> '') and (L_Axis <> '') and (L_Ave <> '') then
  begin
	  if (L_Line1 = '') then
	    L_Line1:= L_Line1 + ' // L: '
	  else
	    L_Line1:= L_Line1 + '';
	  
    if (Trim(L_R1) <> '') then
    begin 	
      L_R1:= Trim(L_R1);
      L_R1:= FormatSignValue('R1=', L_R1, bolAddSign, bolAddSignSeparator);
    end;
    	
    if (Trim(L_R2) <> '') then
    begin 	
      L_R2:= Trim(L_R2);
      L_R2:= FormatSignValue('R2=', L_R2, bolAddSign, bolAddSignSeparator);
    end;    
  
    if (Trim(L_Axis) <> '') then 	
    begin	
      L_Axis:= Trim(L_Axis);
      L_Axis:= FormatAxisValue('*', L_Axis, bolAddAxisSeparator);      	      	      	 
    end;
      
    L_Line1:= L_Line1 + L_R1 + L_Axis + ' ' + L_R2 + L_Axis;
  end;
  
  //
  if (R_Line1 <> '') or (L_Line1 <> '') then
  begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + R_Line1 + L_Line1 + FOutputLineSeparator;
  end;
  
  // V7 R: AV=+ 7.87 CYL=- 0.50 // L: AV=+ 7.83 CYL=- 0.75
  R_Line2:= '';

  // 
  if (DR_Ave <> '') and (DR_Cyl <> '') then
  begin
	  if (R_Line2 = '') then
	    R_Line2:= R_Line2 + 'R: '
	  else
	    R_Line2:= R_Line2 + ''; 
    
    if (Trim(DR_Ave) <> '') then
    begin 	
      DR_Ave:= Trim(DR_Ave);
      DR_Ave:= FormatSignValue('AV=', DR_Ave, bolAddSign, bolAddSignSeparator);
    end;      
    
    if (Trim(DR_Cyl) <> '') then
    begin 	
      DR_Cyl:= Trim(DR_Cyl);
      DR_Cyl:= FormatSignValue('CYL=', DR_Cyl, bolAddSign, bolAddSignSeparator);
    end;      
  
    R_Line2:= R_Line2 + DR_Ave + ' ' + DR_Cyl;
  end;
    
  //
  L_Line2:= '';

	//
  if (DL_Ave <> '') and (DL_Cyl <> '') then
  begin
	  if (L_Line2 = '') then
	    L_Line2:= L_Line2 + ' // L: '
	  else
	    L_Line2:= L_Line2 + '';
  
    if (Trim(DL_Ave) <> '') then
    begin 	
      DL_Ave:= Trim(DL_Ave);
      DL_Ave:= FormatSignValue('AV=', DL_Ave, bolAddSign, bolAddSignSeparator);
    end;      
    
    if (Trim(DL_Cyl) <> '') then
    begin 	
      DL_Cyl:= Trim(DL_Cyl);
      DL_Cyl:= FormatSignValue('CYL=', DL_Cyl, bolAddSign, bolAddSignSeparator);
    end;  
    
    L_Line2:= L_Line2 + DL_Ave + ' ' + DL_Cyl;
  end;	
	
  //
  if (R_Line2 <> '') or (L_Line2 <> '') then
  begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + R_Line2 + L_Line2 + FOutputLineSeparator;
  end;

	// V5 RA CC:1.25 LA CC:1.25
	
	// TODO:

	// if no measurement data was transferd or processed, we have to create an empty set of gdt data 
	if (ParsedData = '') then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + '';
	end;

  // Set result
  FParsedDataString := RawByteString(ParsedData);
end.