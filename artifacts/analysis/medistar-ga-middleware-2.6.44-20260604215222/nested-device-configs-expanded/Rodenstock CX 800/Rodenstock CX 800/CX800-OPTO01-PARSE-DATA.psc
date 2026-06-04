const
  VERSION = '1.0.55.77';
  DATE = '14.06.2022 08:22:44';
  TEXT = 'Copyright (c) 2022 team2work GmbH';

  DATA_SEPARATOR_01            = #13#10;
  DATA_SEPARATOR_02            = ',';

  LINE_IDENTIFIER_PATIENT_ID   = '[PT_ID]';
  LINE_IDENTIFIER_VD           = '[VD]';
  LINE_IDENTIFIER_PD           = '[PD]';  
  LINE_IDENTIFIER_SCA_R        = '[POWER_R],A';
  LINE_IDENTIFIER_SCA_L        = '[POWER_L],A';

  LINE_IDENTIFIER_INF_R_A      = '[INF_R],A';
  LINE_IDENTIFIER_INF_L_A      = '[INF_L],A';

  LINE_IDENTIFIER_K1_R         = '[K1_R]';
  LINE_IDENTIFIER_K2_R         = '[K2_R]';  
  LINE_IDENTIFIER_AV_R         = '[AV_R]';    
  LINE_IDENTIFIER_CYL_R        = '[CYL_R]';  

  LINE_IDENTIFIER_K1_L         = '[K1_L]';
  LINE_IDENTIFIER_K2_L         = '[K2_L]';  
  LINE_IDENTIFIER_AV_L         = '[AV_L]';    
  LINE_IDENTIFIER_CYL_L        = '[CYL_L]'; 

  GDT_LINE_PREFIX              = '  ';
  GDT_SIGN_SEPARATOR           = ' ';
  GDT_AXIS_SEPARATOR           = ' ';
  GDT_AXIS_SEPARATOR_MAX_COUNT = 3;

  GDT_FID_PATIENT_ID           = '3000';
  GDT_FID_MEASURE_DATA         = '6228';
  
  GDT_FID_RESULT               = '6220';
  GDT_FID_FOREIGN_RESULT       = '6221';  

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
  Data, Sep1, Sep2, ParsedData, S1, PatientID, R_Line, L_Line, R_Line_Kera, L_Line_Kera: String;
  arrData1, arrDataVD, arrDataSCA_R, arrDataSCA_L, arrDataPD, arrDataPatientID: TStringArray;
  arrDataK1_R, arrDataK2_R, arrDataCYL_R: TStringArray;
  arrDataK1_L, arrDataK2_L, arrDataCYL_L: TStringArray;  	
  i: Integer;
  bolAddVDValueToOutput, bolAddPDValueToOutput, bolAddGDTLinePrefix: Boolean;
  bolAddSign, bolAddSignSeparator, bolAddAxisSeparator, bolAddKeratometerToOutput: Boolean;
  R_S, R_Z, R_Axis, L_S, L_Z, L_Axis, PD, VD: String;
  R_KeraDataIndex, L_KeraDataIndex: Integer;
  R_K1_1, R_K1_2, R_K1_3: String;
  R_K2_1, R_K2_2, R_K2_3: String;
  CYL_R_Z, CYL_R_AXIS: String;	
  L_K1_1, L_K1_2, L_K1_3: String;
  L_K2_1, L_K2_2, L_K2_3: String;
  CYL_L_Z, CYL_L_AXIS: String;  	
	
begin
  // Verwende "True", damit der VD Wert an die V1 Zeile angehängt wird,
  // benutze "False", damit der VD Wert ignoriert werden kann.
  bolAddVDValueToOutput := True;

  // Verwende "True", damit der PD Wert an die V1 Zeile angehängt wird,
  // benutze "False", damit der PD Wert ignoriert werden kann.
  bolAddPDValueToOutput := True;

  // Verwende "True", damit jeder GDT Zeile das Prefix aus GDT_LINE_PREFIX
  // vorangestellt wird,
  // benutze "False", damit das GDT-Zeilen-Prefix ignoriert wird
  bolAddGDTLinePrefix:= True;

  // Verwende "True", damit immer ein Vorzeichen hinzugefügt wird,
  // benutze "False", damit nur der gemessene Wert eingetragen wird wie er
  // vom Gerät kommt
  bolAddSign:= True;
  
  // Verwende "True", damit nach jedem Vorzeichen der Wert aus GDT_SIGN_SEPARATOR
  // angefügt wird, benutze "False", damit kein Abstand zwischen Vorzeichen und Wert eingefügt wird
  bolAddSignSeparator:= True;
        
  // Verwende "True", damit der Achsenseparator angefügt wird,
  // benutze "False", damit der Achsenseparator nicht verwendet wird
  bolAddAxisSeparator:= True;
  
  // Verwende "True", damit die Keratometer Daten angefügt werden,
  // benutze "False", damit die Keratometer Daten nicht angefügt werden
  bolAddKeratometerToOutput:= True;
  
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
  Sep2:= DATA_SEPARATOR_02;

  Data:= String(FRawDataString);
  Data:= Trim(Data);

  R_KeraDataIndex:= -1;
  L_KeraDataIndex:= -1;

  // Get array from 1st separator
  arrData1:= Explode(Sep1, Data, 0);

  if Length(arrData1) <= 0 then
  begin
    FLastErrorCode:= -3;
    FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Keine Parameter gefunden';

    DoPSError();

    Exit;
  end;

  SetLength(arrDataPatientID, 0);
  SetLength(arrDataVD, 0);
  SetLength(arrDataSCA_R, 0);
  SetLength(arrDataSCA_L, 0);
  SetLength(arrDataPD, 0);

  SetLength(arrDataK1_R, 0);
  SetLength(arrDataK2_R, 0); 
  SetLength(arrDataCYL_R, 0);    
  
  SetLength(arrDataK1_L, 0);
  SetLength(arrDataK2_L, 0); 
  SetLength(arrDataCYL_L, 0);   

  for i:= 0 to Length(arrData1) - 1 do
  begin
    S1:= arrData1[i];

    if (Trim(S1) = '') then
    begin
      Continue;
    end;

    // PATIENT
    if T2WStartsText(LINE_IDENTIFIER_PATIENT_ID, S1) then
    begin
      S1:= T2WStringReplace(S1, LINE_IDENTIFIER_PATIENT_ID, '', False, False);

      if (T2WStartsStr(Sep2, S1)) then
      begin
        S1:= Copy(S1, 2, Length(S1));
      end;

      if (T2WStartsStr('*', S1)) then
      begin
        S1:= Copy(S1, 2, Length(S1));
      end;

      arrDataPatientID:= Explode(Sep2, S1, 0);
      	
      Continue;	
    end;

    // VD
    if T2WStartsText(LINE_IDENTIFIER_VD, S1) then
    begin
      S1:= T2WStringReplace(S1, LINE_IDENTIFIER_VD, '', False, False);

      if (T2WStartsStr(Sep2, S1)) then
      begin
        S1:= Copy(S1, 2, Length(S1));
      end;

      arrDataVD:= Explode(Sep2, S1, 0);
      	
      Continue;
    end;

    // R SCA
    if T2WStartsText(LINE_IDENTIFIER_SCA_R, S1) then
    begin
     S1:= T2WStringReplace(S1, LINE_IDENTIFIER_SCA_R, '', False, False);

      if (T2WStartsStr(Sep2, S1)) then
      begin
        S1:= Copy(S1, 2, Length(S1));
      end;

      arrDataSCA_R:= Explode(Sep2, S1, 0);
      	
      Continue;
    end;

    // L SCA
    if T2WStartsText(LINE_IDENTIFIER_SCA_L, S1) then
    begin
      S1:= T2WStringReplace(S1, LINE_IDENTIFIER_SCA_L, '', False, False);

      if (T2WStartsStr(Sep2, S1)) then
      begin
        S1:= Copy(S1, 2, Length(S1));
      end;

      arrDataSCA_L:= Explode(Sep2, S1, 0);
      	
      Continue;
    end;

    // PD
    if T2WStartsText(LINE_IDENTIFIER_PD, S1) and (Length(arrDataPD) = 0) then
    begin
      S1:= T2WStringReplace(S1, LINE_IDENTIFIER_PD, '', False, False);

      if (T2WStartsStr(Sep2, S1)) then
      begin
        S1:= Copy(S1, 2, Length(S1));
      end;

      arrDataPD:= Explode(Sep2, S1, 0);
      	
      Continue;
    end;

    // begin keratometer data parsing
      
    if T2WStartsText(LINE_IDENTIFIER_INF_R_A, S1) then
    begin
      R_KeraDataIndex:= i;
      
      Continue;
    end;
        
    if (R_KeraDataIndex >= 0) and (T2WStartsText(LINE_IDENTIFIER_K1_R, S1)) then
    begin
      S1:= T2WStringReplace(S1, LINE_IDENTIFIER_K1_R, '', False, False);

      if (T2WStartsStr(Sep2, S1)) then
      begin
        S1:= Copy(S1, 2, Length(S1));
      end;

      arrDataK1_R:= Explode(Sep2, S1, 0);     

      Continue;
    end;  
      
    if (R_KeraDataIndex >= 0) and (T2WStartsText(LINE_IDENTIFIER_K2_R, S1)) then
    begin
      S1:= T2WStringReplace(S1, LINE_IDENTIFIER_K2_R, '', False, False);

      if (T2WStartsStr(Sep2, S1)) then
      begin
        S1:= Copy(S1, 2, Length(S1));
      end;

      arrDataK2_R:= Explode(Sep2, S1, 0);          

      Continue;
    end;      
      
    if (R_KeraDataIndex >= 0) and (T2WStartsText(LINE_IDENTIFIER_CYL_R, S1)) then
    begin
      S1:= T2WStringReplace(S1, LINE_IDENTIFIER_CYL_R, '', False, False);

      if (T2WStartsStr(Sep2, S1)) then
      begin
        S1:= Copy(S1, 2, Length(S1));
      end;

      arrDataCYL_R:= Explode(Sep2, S1, 0);      
     
      Continue;
    end;      

    if T2WStartsText(LINE_IDENTIFIER_INF_L_A, S1) then
    begin
      L_KeraDataIndex:= i;
      
      Continue;
    end;    
         
    if (L_KeraDataIndex >= 0) and (T2WStartsText(LINE_IDENTIFIER_K1_L, S1)) then
    begin
      S1:= T2WStringReplace(S1, LINE_IDENTIFIER_K1_L, '', False, False);

      if (T2WStartsStr(Sep2, S1)) then
      begin
        S1:= Copy(S1, 2, Length(S1));
      end;

      arrDataK1_L:= Explode(Sep2, S1, 0);     

      Continue;
    end;  
      
    if (L_KeraDataIndex >= 0) and (T2WStartsText(LINE_IDENTIFIER_K2_L, S1)) then
    begin
      S1:= T2WStringReplace(S1, LINE_IDENTIFIER_K2_L, '', False, False);

      if (T2WStartsStr(Sep2, S1)) then
      begin
        S1:= Copy(S1, 2, Length(S1));
      end;

      arrDataK2_L:= Explode(Sep2, S1, 0);          

      Continue;
    end;      
       
    if (L_KeraDataIndex >= 0) and (T2WStartsText(LINE_IDENTIFIER_CYL_L, S1)) then
    begin
      S1:= T2WStringReplace(S1, LINE_IDENTIFIER_CYL_L, '', False, False);

      if (T2WStartsStr(Sep2, S1)) then
      begin
        S1:= Copy(S1, 2, Length(S1));
      end;

      arrDataCYL_L:= Explode(Sep2, S1, 0);      
     
      Continue;
    end;     
    
    // add more measurement data here...
  end;

  // Remove empty array values
  arrDataPatientID:= RemoveEmptyArrayValues(arrDataPatientID);
  arrDataVD:= RemoveEmptyArrayValues(arrDataVD);
  arrDataSCA_R:= RemoveEmptyArrayValues(arrDataSCA_R);
  arrDataSCA_L:= RemoveEmptyArrayValues(arrDataSCA_L);
  arrDataPD:= RemoveEmptyArrayValues(arrDataPD);

  arrDataK1_R:= RemoveEmptyArrayValues(arrDataK1_R);
  arrDataK2_R:= RemoveEmptyArrayValues(arrDataK2_R);  	
  arrDataCYL_R:= RemoveEmptyArrayValues(arrDataCYL_R);

  arrDataK1_L:= RemoveEmptyArrayValues(arrDataK1_L);
  arrDataK2_L:= RemoveEmptyArrayValues(arrDataK2_L);  	
  arrDataCYL_L:= RemoveEmptyArrayValues(arrDataCYL_L);

  // Trim all array values
  arrDataPatientID:= TrimArrayValues(arrDataPatientID);
  arrDataVD:= TrimArrayValues(arrDataVD);
  arrDataSCA_R:= TrimArrayValues(arrDataSCA_R);
  arrDataSCA_L:= TrimArrayValues(arrDataSCA_L);
  arrDataPD:= TrimArrayValues(arrDataPD);

  arrDataK1_R:= TrimArrayValues(arrDataK1_R);
  arrDataK2_R:= TrimArrayValues(arrDataK2_R);  	
  arrDataCYL_R:= TrimArrayValues(arrDataCYL_R);
  	
  arrDataK1_L:= TrimArrayValues(arrDataK1_L);
  arrDataK2_L:= TrimArrayValues(arrDataK2_L);  	
  arrDataCYL_L:= TrimArrayValues(arrDataCYL_L);  	

  // Start parsing ophthalmology data down here 
	
  // Reset parsed data
  ParsedData:= '';

  // Patient ID
  PatientID:= '';

  if Length(arrDataPatientID) > 0 then
  begin
    PatientID:= PatientID + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
    PatientID:= PatientID + arrDataPatientID[0] + FOutputLineSeparator;
  end;

  // PD
  PD:= '';

  if Length(arrDataPD) > 0 then
  begin
    PD:= PD + 'PD= ' + arrDataPD[0];
  end;

  // VD
  VD:= '';

  if Length(arrDataVD) > 0 then
  begin
    VD:= VD + 'VD= ' + arrDataVD[0];
  end;

  // R_SCA
  R_S:= '';
  R_Z:= '';
  R_Axis:= '';


  // KAI:

  if Length(arrDataSCA_R) >= 3 then
  begin
    R_S:= R_S + FormatSignValue('S=', arrDataSCA_R[0], bolAddSign, bolAddSignSeparator);
    R_Z:= R_Z + FormatSignValue('Z=', arrDataSCA_R[1], bolAddSign, bolAddSignSeparator);
    R_Axis:= R_Axis + FormatAxisValue('*', arrDataSCA_R[2], bolAddAxisSeparator);
  end;
  
  if Length(arrDataSCA_R) = 2 then
  begin
    R_S:= R_S + FormatSignValue('S=', arrDataSCA_R[0], bolAddSign, bolAddSignSeparator);
    R_Z:= R_Z + FormatSignValue('Z=', arrDataSCA_R[1], bolAddSign, bolAddSignSeparator);
    R_Axis:= R_Axis + FormatAxisValue('*', '0', bolAddAxisSeparator);
  end;  

  // L_SCA
  L_S:= '';
  L_Z:= '';
  L_Axis:= '';

  if Length(arrDataSCA_L) >= 3 then
  begin
    L_S:= L_S + FormatSignValue('S=', arrDataSCA_L[0], bolAddSign, bolAddSignSeparator);
    L_Z:= L_Z + FormatSignValue('Z=', arrDataSCA_L[1], bolAddSign, bolAddSignSeparator);
    L_Axis:= L_Axis + FormatAxisValue('*', arrDataSCA_L[2], bolAddAxisSeparator);
  end;
  
  if Length(arrDataSCA_L) = 2 then
  begin
    L_S:= L_S + FormatSignValue('S=', arrDataSCA_L[0], bolAddSign, bolAddSignSeparator);
    L_Z:= L_Z + FormatSignValue('Z=', arrDataSCA_L[1], bolAddSign, bolAddSignSeparator);
    L_Axis:= L_Axis + FormatAxisValue('*', '0', bolAddAxisSeparator);
  end;

  // Add right eye
  R_Line:= '';
	
  if (R_S <> '') or (R_Z <> '') or (R_Axis <> '') then
  begin
    if (bolAddGDTLinePrefix) then
    begin
      R_Line:= R_Line + GDT_LINE_PREFIX;
    end;
      	
    R_Line:= R_Line + 'R.:' + R_S + ' ' + R_Z + R_Axis;
  end;

  // Add left eye
  L_Line:= '';
	
  if (L_S <> '') or (L_Z <> '') or (L_Axis <> '') then
  begin
    if (bolAddGDTLinePrefix) then
    begin
      L_Line:= L_Line + GDT_LINE_PREFIX;
    end;
	
    L_Line:= L_Line + 'L.:' + L_S + ' ' + L_Z + L_Axis;
  end;

  // Add PD
  if (PD <> '') and (bolAddPDValueToOutput) then
  begin
    if (R_Line <> '') then
    begin
      R_Line:= R_Line + ' ';
    end;
		
    R_Line:= R_Line + PD;
  end;

  // Add VD
  if (VD <> '') and (bolAddVDValueToOutput) then
  begin
    if (R_Line <> '') then
    begin
      R_Line:= R_Line + ' ';
    end;
		
    R_Line:= R_Line + VD;
  end;

  // Add right eye to gdt output
  if (R_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + R_Line + FOutputLineSeparator;
  end;

  // Add left eye to gdt output
  if (L_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
  end;
  
  // Parse kera data down here
  R_Line_Kera:= '';
  L_Line_Kera:= ''; 
  
  if (bolAddKeratometerToOutput) then
  begin
    // K1_R
    R_K1_1:= '';
    R_K1_2:= '';
    R_K1_3:= '';
  
    if Length(arrDataK1_R) >= 3 then
    begin
      R_K1_1:= R_K1_1 + Trim(arrDataK1_R[0]);
      R_K1_2:= R_K1_2 + Trim(arrDataK1_R[1]);
      R_K1_3:= R_K1_3 + FormatAxisValue('*', arrDataK1_R[2], bolAddAxisSeparator);
    end;  
  
    // K2_R
    R_K2_1:= '';
    R_K2_2:= '';
    R_K2_3:= '';  
  
    if Length(arrDataK2_R) >= 3 then
    begin
      R_K2_1:= R_K2_1 + Trim(arrDataK2_R[0]);
      R_K2_2:= R_K2_2 + Trim(arrDataK2_R[1]);
      R_K2_3:= R_K2_3 + FormatAxisValue('*', arrDataK2_R[2], bolAddAxisSeparator);
    end;    
  
    // CYL_R
    CYL_R_Z:= '';
    CYL_R_AXIS:= '';
  
    if Length(arrDataCYL_R) >= 2 then
    begin
      CYL_R_Z:= CYL_R_Z + FormatSignValue('Z=', arrDataCYL_R[0], bolAddSign, bolAddSignSeparator);
      CYL_R_AXIS:= CYL_R_AXIS + FormatAxisValue('*', arrDataCYL_R[1], bolAddAxisSeparator);
    end;  
  
    // K1_L
    L_K1_1:= '';
    L_K1_2:= '';
    L_K1_3:= '';
  
    if Length(arrDataK1_L) >= 3 then
    begin
      L_K1_1:= L_K1_1 + Trim(arrDataK1_L[0]);
      L_K1_2:= L_K1_2 + Trim(arrDataK1_L[1]);
      L_K1_3:= L_K1_3 + FormatAxisValue('*', arrDataK1_L[2], bolAddAxisSeparator);
    end;    
  
    // K2_L  
    L_K2_1:= '';
    L_K2_2:= '';
    L_K2_3:= '';  
  
    if Length(arrDataK2_L) >= 3 then
    begin
      L_K2_1:= L_K2_1 + Trim(arrDataK2_L[0]);
      L_K2_2:= L_K2_2 + Trim(arrDataK2_L[1]);
      L_K2_3:= L_K2_3 + FormatAxisValue('*', arrDataK2_L[2], bolAddAxisSeparator);
    end;    
  
    // CYL_L 
    CYL_L_Z:= '';
    CYL_L_AXIS:= '';
  
    if Length(arrDataCYL_L) >= 2 then
    begin
      CYL_L_Z:= CYL_L_Z + FormatSignValue('Z=', arrDataCYL_L[0], bolAddSign, bolAddSignSeparator);
      CYL_L_AXIS:= CYL_L_AXIS + FormatAxisValue('*', arrDataCYL_L[1], bolAddAxisSeparator);
    end;    
  
    // BUGFIX: if empty lines are in output file, the importer could not load converted data
    ParsedData:= Trim(ParsedData); 
  
    // Add right eye 
    if (R_K1_1 <> '') and (R_K1_2 <> '') and (R_K1_3 <> '') then
    begin
      if (bolAddGDTLinePrefix) then
      begin
        R_Line_Kera:= R_Line_Kera + GDT_LINE_PREFIX;
      end;
      	
      R_Line_Kera:= R_Line_Kera + 'Kera R: ' + 'B1=' + R_K1_2 + R_K1_3;
    end;   
  
    if (R_K2_1 <> '') and (R_K2_2 <> '') and (R_K2_3 <> '') then
    begin
      if (R_Line_Kera = '') then
      begin
        if (bolAddGDTLinePrefix) then
        begin
          R_Line_Kera:= R_Line_Kera + GDT_LINE_PREFIX;
        end; 
      
        R_Line_Kera:= R_Line_Kera + 'Kera R: ';   
      end;
  
      R_Line_Kera:= R_Line_Kera + ' ' + 'B2=' + R_K2_2 + R_K2_3;
    end;  
  
    if (CYL_R_Z <> '') and (CYL_R_AXIS <> '') then
    begin
      if (R_Line_Kera = '') then
      begin
        if (bolAddGDTLinePrefix) then
        begin
          R_Line_Kera:= R_Line_Kera + GDT_LINE_PREFIX;
        end; 
      
        R_Line_Kera:= R_Line_Kera + 'Kera R: ';   
      end;  
  
      R_Line_Kera:= R_Line_Kera + ' ' + CYL_R_Z + CYL_R_AXIS;
    end;  
  
    // Add left eye  
    if (L_K1_1 <> '') and (L_K1_2 <> '') and (L_K1_3 <> '') then
    begin
      if (bolAddGDTLinePrefix) then
      begin
        L_Line_Kera:= L_Line_Kera + GDT_LINE_PREFIX;
      end;
      	
      L_Line_Kera:= L_Line_Kera + 'Kera L: ' + 'B1=' + L_K1_2 + L_K1_3;
    end;   
 
    if (L_K2_1 <> '') and (L_K2_2 <> '') and (L_K2_3 <> '') then
    begin
      if (L_Line_Kera = '') then
      begin
        if (bolAddGDTLinePrefix) then
        begin
          L_Line_Kera:= L_Line_Kera + GDT_LINE_PREFIX;
        end; 
      
        L_Line_Kera:= L_Line_Kera + 'Kera L: ';   
      end;
  
      L_Line_Kera:= L_Line_Kera + ' ' + 'B2=' + L_K2_2 + L_K2_3;
    end; 
 
    if (CYL_L_Z <> '') and (CYL_L_AXIS <> '') then
    begin
      if (L_Line_Kera = '') then
      begin
        if (bolAddGDTLinePrefix) then
        begin
          L_Line_Kera:= L_Line_Kera + GDT_LINE_PREFIX;
        end; 
      
        L_Line_Kera:= L_Line_Kera + 'Kera L: ';   
      end;  
  
      L_Line_Kera:= L_Line_Kera + ' ' + CYL_L_Z + CYL_L_AXIS;
    end;
  end;
  
  // Add new line separator
  if (R_Line_Kera <> '') or (L_Line_Kera <> '') then
    ParsedData:= ParsedData + FOutputLineSeparator;  
  
  // Add right eye to gdt output
  if (R_Line_Kera <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_RESULT  + FOutputLineSeparator;
    ParsedData:= ParsedData + R_Line_Kera + FOutputLineSeparator;
  end;  
  
  // Add left eye to gdt output
  if (L_Line_Kera <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FOREIGN_RESULT + FOutputLineSeparator;
    ParsedData:= ParsedData + L_Line_Kera + FOutputLineSeparator;
  end;  
 
  // Set result
  FParsedDataString:= RawByteString(ParsedData);  
end.