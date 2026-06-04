const
  VERSION = '1.0.55.20';
  DATE = '13.05.2024 10:14:26';
  TEXT = 'Copyright (c) 2024 team2work GmbH';
  
  CSV_LINE_SEPARATOR = #13#10;
  CSV_VALUE_SEPARATOR = ';';
    
  GDT_FID_PATIENT_ID = '3000';
  GDT_FID_MEASURE_DATA = '6228';
  GDT_FID_COMMENT = '6227';
  GDT_FID_FILE_ARCHIVE_NUMBER = '6302';
  GDT_FID_FILE_FORMAT = '6303';
  GDT_FID_FILE_DESCRIPTION = '6304';
  GDT_FID_FILE_URL = '6305';
 
var
  arrData, arrData2: TStringArray;
  ParsedData, S, PatientID, ID, MeasurementDuration: String;
  R_Line, L_Line: String;
  i: Integer;
  R_IOD, R_Quality, L_IOD, L_Quality: String;
  bolUseOnlyLatestValues: Boolean;

begin
  // Verwende "True", damit nur der aktuellste Wert (Messung) genutzt wird,
  // benutze "False", damit alle Messungen verwendet werden.
  bolUseOnlyLatestValues:= True;

  // --- Don't edit script down below ---

  // Clear parsed data string
  FParsedDataString:= '';
  
  // Get array of CSV lines
  arrData:= Explode(CSV_LINE_SEPARATOR, FRawDataString, 0);
  
  if (Length(arrData) <= 0) then
  begin
    FLastErrorCode:= -4;
    FLastErrorMessage:= 'Keine CSV-Zeilen für die Verarbeitung verfügbar';

    DoPSError;

    Exit;
  end;
  
  // Reset vars
	ParsedData:= '';

  R_Line:= '';
  L_Line:= '';
  
  PatientID:= '';
  
  ID:= '';
  MeasurementDuration:= '';

  R_IOD:= '';
  R_Quality:= '';

  L_IOD:= '';
  L_Quality:= '';

  for i:= Length(arrData) - 1 downto 0 do
  begin
    R_Line:= '';
    L_Line:= '';
  
    ID:= '';
    MeasurementDuration:= '';

    R_IOD:= '';
    R_Quality:= '';

    L_IOD:= '';
    L_Quality:= '';
  
    // Get data
    S:= arrData[i];
		
		// T2WMessageBoxS(S);

    arrData2:= Explode(CSV_VALUE_SEPARATOR, S, 0);
    
    if (Length(arrData2) <> 6) then
    begin
      // TODO: May be show error here
      FLastErrorCode:= -5;
      FLastErrorMessage:= 'Keine gültigen CSV-Zeilen für die Verarbeitung verfügbar';
      
      // DoPSError;
      
      // Break;
      // Exit;
    end;

    // "Messdauer";"IOD (Rechtes Auge)";"Qualität (Rechtes Auge)";"IOD (Linkes Auge)";"Qualität (Linkes Auge)";"ID"
    
		if (Length(arrData2) >= 1) then
      MeasurementDuration:= arrData2[0];
    
		if (Length(arrData2) >= 2) then
      R_IOD:= arrData2[1];
		
		if (Length(arrData2) >= 3) then
      R_Quality:= arrData2[2];  
    
		if (Length(arrData2) >= 4) then
      L_IOD:= arrData2[3];
		
		if (Length(arrData2) >= 5) then
      L_Quality:= arrData2[4]; 
    
		if (Length(arrData2) >= 6) then
      ID:= arrData2[5];
     
    // Build right eye 
    if (R_Line = '') then
    begin
      R_Line:= R_Line + 'R.:';
    end;
		
		if (R_IOD = '') then
		begin
		  R_IOD:= ' 0';
	  end;
		
		if (R_Quality = '') then
		begin
		  R_Quality:= 'UNBEKANNT';
	  end;
    
    R_Line:= R_Line + 'IOD: ' + R_IOD + ' ' + 'Qualität: ' + R_Quality + ' ' + 'Messdauer: ' + MeasurementDuration;
    R_Line:= Trim(R_Line);
		
		// Build output
    
    // Add right eye
    if (R_Line <> '') then
    begin
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
      ParsedData:= ParsedData + R_Line + FOutputLineSeparator;
    end;
    
    // Build left eye
    if (L_Line = '') then
    begin
      L_Line:= L_Line + 'L.:';
    end;
		
		if (L_IOD = '') then
		begin
		  L_IOD:= ' 0';
	  end;
		
		if (L_Quality = '') then
		begin
		  L_Quality:= 'UNBEKANNT';
	  end;
     
    L_Line:= L_Line + 'IOD: ' + L_IOD + ' ' + 'Qualität: ' + L_Quality + ' ' + 'Messdauer: ' + MeasurementDuration;
    L_Line:= Trim(L_Line);
    
    // Add left eye
    if (L_Line <> '') then
    begin
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
      ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
    end;
    
    // If selected, break the loop here
    if (bolUseOnlyLatestValues) then
    begin
      Break;
    end;    
  end;
  
  // Build output

  // Add patient ID
  if (PatientID <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
    ParsedData:= ParsedData + PatientID + FOutputLineSeparator;
  end;

  // Set output
  FParsedDataString:= RawByteString(ParsedData);
end.