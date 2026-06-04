const
	VERSION = '1.0.2.67';
	DATE = '13.07.2020 13:45:19';
	TEXT = 'Copyright (c) 2020 team2work GmbH';

	// #13 -> CR / #10 -> LF
	DATA_SEPARATOR_01 = #13#10;
	DATA_SEPARATOR_02 = '  ';
	
	LINE_IDENTIFIER_M = 'M ';
	LINE_IDENTIFIER_T = 'T ';
	LINE_IDENTIFIER_R = 'R ';
	LINE_IDENTIFIER_L = 'L ';
	LINE_IDENTIFIER_PD = 'PD=';
	LINE_IDENTIFIER_R1 = 'R1';
	
	GDT_FID_MEASURE_DATA = '6228';

var
	Data, Sep1, Sep2, ParsedData: String;
	arrData1, arrDataVD, arrDataR, arrDataL, arrDataPD: TStringArray;
	i, Index: Integer;
	S1: String;
	bolStrictExaminationOfValues: Boolean;
        bolAddPPSValueToOutput: Boolean;
	
begin
	// Aktiviere strikte Prüfung, ob alle "Felder" (VD, PD, R, L) vorhanden sein müssen.
	// Verwende "True", damit die Prüfung aktiviert ist, benutze "False", damit die
	// Prüfung ausgeschalten ist.
	bolStrictExaminationOfValues:= False;
	
	// Verwende "True", damit der PPS Wert hinzugefügt wird, benutze "False", damit der
	// PPS Wert nicht per GDT exportiert wird. PPS = Photopic Pupil Size.
	bolAddPPSValueToOutput:= True;
	
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

	// Get array from 1st separator
	arrData1:= Explode(Sep1, Data, 0);

	if Length(arrData1) <= 0 then
	begin
		FLastErrorCode:= -3;
		FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Keine Parameter gefunden';

		DoPSError();

		Exit;
	end;
	
	SetLength(arrDataVD, 0);
	SetLength(arrDataR, 0);
	SetLength(arrDataL, 0);
	SetLength(arrDataPD, 0);
	
	for i:= 0 to Length(arrData1) - 1 do
	begin
		S1:= arrData1[i];
		
		if T2WStartsText(LINE_IDENTIFIER_M, S1) then
		begin
			Continue;
		end
		else if T2WStartsText(LINE_IDENTIFIER_T, S1) then
		begin
			S1:= T2WStringReplace(S1, LINE_IDENTIFIER_T, '', False, False);
			S1:= T2WStringReplace(S1, 'VD', 'VD=  ', False, False);
			
			arrDataVD:= Explode(Sep2, S1, 0);
		end
		else if T2WStartsText(LINE_IDENTIFIER_R, S1) then
		begin
		  if not T2WContainsText(S1, LINE_IDENTIFIER_R1) then
		  begin
			S1:= T2WStringReplace(S1, LINE_IDENTIFIER_R, 'R.:  ', False, False);
			S1:= T2WStringReplace(S1, 'S  ', 'S=  ', False, False);
			S1:= T2WStringReplace(S1, ' C  ', '  Z=  ', False, False);
			S1:= T2WStringReplace(S1, ' A  ', '  *  ', False, False);
			S1:= T2WStringReplace(S1, ' PPS  ', '  PPS=  ', False, False);
			
			arrDataR:= Explode(Sep2, S1, 0);
		  end;			
		end
		else if T2WStartsText(LINE_IDENTIFIER_L, S1) then
		begin
		  if not T2WContainsText(S1, LINE_IDENTIFIER_R1) then
		  begin		
			S1:= T2WStringReplace(S1, LINE_IDENTIFIER_L, 'L.:  ', False, False);
			S1:= T2WStringReplace(S1, 'S  ', 'S=  ', False, False);
			S1:= T2WStringReplace(S1, ' C  ', '  Z=  ', False, False);
			S1:= T2WStringReplace(S1, ' A  ', '  *  ', False, False);
			S1:= T2WStringReplace(S1, ' PPS  ', '  PPS=  ', False, False);
			
			arrDataL:= Explode(Sep2, S1, 0);
		  end;			
		end
		else if T2WStartsText(LINE_IDENTIFIER_PD, S1) then
		begin
			S1:= T2WStringReplace(S1, 'PD=', 'PD=  ', False, False);
			
			arrDataPD:= Explode(Sep2, S1, 0);
		end
		else
		begin
			Continue;
		end;
	end;

	// Remove empty array values
	arrDataVD:= RemoveEmptyArrayValues(arrDataVD);
	arrDataR:= RemoveEmptyArrayValues(arrDataR);
	arrDataL:= RemoveEmptyArrayValues(arrDataL);
	arrDataPD:= RemoveEmptyArrayValues(arrDataPD);

	// Trim all array values
	arrDataVD:= TrimArrayValues(arrDataVD);
	arrDataR:= TrimArrayValues(arrDataR);
	arrDataL:= TrimArrayValues(arrDataL);
	arrDataPD:= TrimArrayValues(arrDataPD);
	
	// check if we want to use the strict examination check
	// for availability of the values.
	if (bolStrictExaminationOfValues) then
	begin	
  	  // Check array lengths
	  if Length(arrDataVD) <= 0 then
	  begin
		  FLastErrorCode:= -4;
		  FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter "VD" nicht vorhanden';

		  DoPSError();

		  Exit;
	  end;
	
	  if Length(arrDataR) <= 0 then
	  begin
		  FLastErrorCode:= -5;
		  FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter "R" nicht vorhanden';

		  DoPSError();

		  Exit;
	  end;
	
	  if Length(arrDataL) <= 0 then
	  begin
		  FLastErrorCode:= -6;
		  FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter "L" nicht vorhanden';

		  DoPSError();

		  Exit;
	  end;
	
	  if Length(arrDataPD) <= 0 then
	  begin
		  FLastErrorCode:= -7;
		  FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter "PD" nicht vorhanden';

		  DoPSError();

		  Exit;
	  end;	
	end;	
	
	// Start parsing ophthalmology data down here 
	
	// Reset parsed data
	ParsedData:= '';
	
	// Process right eye
	
	Index:= InArray('R.:', False, arrDataR);
	
	if Index >= 0 then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + arrDataR[Index];
	end;
	
	Index:= InArray('S=', False, arrDataR);
	
	if (Index >= 0)
	and ((Index + 1) < (Length(arrDataR)))
	then
	begin
	  ParsedData:= ParsedData + arrDataR[Index] + arrDataR[Index + 1];
	end;
	
	Index:= InArray('Z=', False, arrDataR);
	
	if (Index >= 0)
	and ((Index + 1) < (Length(arrDataR)))
	then
	begin
	  ParsedData:= ParsedData + ' ' + arrDataR[Index] + arrDataR[Index + 1];
	end;
	
	Index:= InArray('*', False, arrDataR);
	
	if (Index >= 0)
	and ((Index + 1) < (Length(arrDataR)))
	then
	begin
	  if (arrDataL[Index + 1] <> 'PPS=') then
	  begin
           ParsedData:= ParsedData + arrDataR[Index] + arrDataR[Index + 1];
	  end
        else
        begin
          ParsedData:= ParsedData + arrDataR[Index] + ''; 
        end;	 		
	end;
	
	Index:= InArray('PD=', False, arrDataPD);
	
	if (Index >= 0)
	and ((Index + 1) < (Length(arrDataPD)))
	then
	begin
	  ParsedData:= ParsedData + ' ' + arrDataPD[Index] + ' ' + arrDataPD[Index + 1];
	end;
	
	Index:= InArray('VD=', False, arrDataVD);
	
	if (Index >= 0)
	and ((Index + 1) < (Length(arrDataVD)))
	then
	begin
	  ParsedData:= ParsedData + ' ' + arrDataVD[Index] + ' ' + arrDataVD[Index + 1];
	end;
	
	if (bolAddPPSValueToOutput) then
	begin	
	  Index:= InArray('PPS=', False, arrDataR);
	
	  if (Index >= 0)
	  and ((Index + 1) < (Length(arrDataR)))
	  then
	  begin
            ParsedData:= ParsedData + ' ' + arrDataR[Index] + ' ' + arrDataR[Index + 1];
	  end;	
	end;
	
	ParsedData:= ParsedData + #13#10;
		
	// Process left eye
	
	Index:= InArray('L.:', False, arrDataL);
	
	if Index >= 0 then
	begin
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	  ParsedData:= ParsedData + arrDataL[Index];
	end;
	
	Index:= InArray('S=', False, arrDataL);
	
	if (Index >= 0)
	and ((Index + 1) < (Length(arrDataL)))
	then
	begin
	  ParsedData:= ParsedData + arrDataL[Index] + ' ' + arrDataL[Index + 1];
	end;
	
	Index:= InArray('Z=', False, arrDataL);
	
	if (Index >= 0)
	and ((Index + 1) < (Length(arrDataL)))
	then
	begin
	  ParsedData:= ParsedData + ' ' + arrDataL[Index] + arrDataL[Index + 1];
	end;
	
	Index:= InArray('*',  False, arrDataL);
	
	if (Index >= 0)
	and ((Index + 1) < (Length(arrDataL)))
	then
	begin
	  if (arrDataL[Index + 1] <> 'PPS=') then
	  begin
           ParsedData:= ParsedData + arrDataL[Index] + arrDataL[Index + 1];
	  end
        else
        begin
         ParsedData:= ParsedData + arrDataL[Index] + ''; 
        end;	  
	end;
	
	if (bolAddPPSValueToOutput) then
	begin	
	  Index:= InArray('PPS=', False, arrDataL);
	
	  if (Index >= 0)
	  and ((Index + 1) < (Length(arrDataL)))
	  then
	  begin
	    ParsedData:= ParsedData + ' ' + arrDataL[Index] +  ' ' + arrDataL[Index + 1];
	  end;
	end;
	
	// Set result
	FParsedDataString:= RawByteString(ParsedData);
end.