const
	VERSION = '1.0.2.67';
	DATE = '13.07.2020 16:45:19';
	TEXT = 'Copyright (c) 2020 team2work GmbH';

	// #13 -> CR / #10 -> LF
	DATA_SEPARATOR_01 = #13#10;
	DATA_SEPARATOR_02 = ' ';
	
	LINE_IDENTIFIER_R = 'R ';
	LINE_IDENTIFIER_L = 'L ';
	LINE_IDENTIFIER_R1 = 'R1';
	
	GDT_FID_MEASURE_DATA = '6228';

var
	Data, Sep1, Sep2, ParsedData: String;
	arrData1, arrDataR, arrDataL: TStringArray;
	i, Index: Integer;
	S1, S2, S3, S4, S5: String;
	bolStrictExaminationOfValues: Boolean;
	
begin
	// Aktiviere strikte Prüfung, ob alle "Felder" (R, L) vorhanden sein müssen.
	// Verwende "True", damit die Prüfung aktiviert ist, benutze "False", damit die
	// Prüfung ausgeschalten ist.
	bolStrictExaminationOfValues:= False;
		
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
	
	SetLength(arrDataR, 0);
	SetLength(arrDataL, 0);
	
	for i:= 0 to Length(arrData1) - 1 do
	begin
		S1:= arrData1[i];
		
                if T2WStartsText(LINE_IDENTIFIER_R, S1) then
		begin
		  if T2WContainsText(S1, LINE_IDENTIFIER_R1) then
		  begin
			  S1:= T2WStringReplace(S1, LINE_IDENTIFIER_R, 'R: ', False, False);

			  arrDataR:= Explode(Sep2, S1, 0);
		  end;			
		end	
		else if T2WStartsText(LINE_IDENTIFIER_L, S1) then
		begin
		  if T2WContainsText(S1, LINE_IDENTIFIER_R1) then
		  begin		
			  S1:= T2WStringReplace(S1, LINE_IDENTIFIER_L, 'L: ', False, False);
			
			  arrDataL:= Explode(Sep2, S1, 0);
		  end;			
		end
		else
		begin
			Continue;
		end;
	end;

	// Remove empty array values
	arrDataR:= RemoveEmptyArrayValues(arrDataR);
	arrDataL:= RemoveEmptyArrayValues(arrDataL);

	// Trim all array values
	arrDataR:= TrimArrayValues(arrDataR);
	arrDataL:= TrimArrayValues(arrDataL);
	
	// check if we want to use the strict examination check
	// for availability of the values.
	if (bolStrictExaminationOfValues) then
	begin	
  	  // Check array lengths
	  if Length(arrDataR) <= 0 then
	  begin
		  FLastErrorCode:= -4;
		  FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter "R" nicht vorhanden';

		  DoPSError();

		  Exit;
	  end;
	
	  if Length(arrDataL) <= 0 then
	  begin
		  FLastErrorCode:= -5;
		  FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Parameter "L" nicht vorhanden';

		  DoPSError();

		  Exit;
	  end;
	end;	
	
	// Start parsing ophthalmology data down here 
	
	// Reset parsed data
	ParsedData := '';

        // Process right eye ()
        S2 := 'R:' + ' ';
        S4 := 'R:' + ' ';
  
        Index := InArray('R:', False, arrDataR);
  
	if Index >= 0 then
	begin
	  Index := InArray('R1', False, arrDataR);
	
	  if (Index >= 0)
	  and ((Index + 1) < (Length(arrDataR)))
	  then
	  begin
	    S2 := S2 + arrDataR[Index] + '= ' + arrDataR[Index + 1];
	  end;

	  Index := InArray('AX', False, arrDataR);

	  if (Index >= 0)
	  and ((Index + 1) < (Length(arrDataR)))
	  then
	  begin
	    S2 := S2 + '*' + arrDataR[Index + 1];
	  end;

	  Index := InArray('R2', False, arrDataR);

	  if (Index >= 0)
	  and ((Index + 1) < (Length(arrDataR)))
	  then
	  begin
	    S2 := S2 + ' ' + arrDataR[Index] + '= ' + arrDataR[Index + 1];
	  end;

	  Index := InArray('AX', False, arrDataR);

	  if (Index >= 0)
	  and ((Index + 1) < (Length(arrDataR)))
	  then
	  begin
	    S2 := S2 + '*' + arrDataR[Index + 1];
	  end;

	  //
	  Index := InArray('CYL', False, arrDataR);

	  if (Index >= 0)
	  and ((Index + 1) < (Length(arrDataR)))
	  then
	  begin
	    S4 := S4 + arrDataR[Index] + '= ' + arrDataR[Index + 1];
	  end;

	  //
	  S2 := S2 + ' ' + '//';
	  S4 := S4 + ' ' + '//';
	end;

        // Process left eye ()
	S3 := 'L:' + ' ';
	S5 := 'L:' + ' ';

	Index := InArray('L:', False, arrDataL);
	
	if Index >= 0 then
	begin
	  Index := InArray('R1', False, arrDataL);
	
	  if (Index >= 0)
	  and ((Index + 1) < (Length(arrDataL)))
	  then
	  begin
	    S3 := S3 + arrDataL[Index] + '= ' + arrDataL[Index + 1];
	  end;

	  Index:= InArray('AX', False, arrDataL);

	  if (Index >= 0)
	  and ((Index + 1) < (Length(arrDataL)))
	  then
	  begin
	    S3 := S3 + '*' + arrDataL[Index + 1];
	  end;

	  Index := InArray('R2', False, arrDataL);
	
	  if (Index >= 0)
	  and ((Index + 1) < (Length(arrDataL)))
	  then
	  begin
	    S3 := S3 + ' ' + arrDataL[Index] + '= ' + arrDataL[Index + 1];
	  end;

	  Index:= InArray('AX', False, arrDataL);

	  if (Index >= 0)
	  and ((Index + 1) < (Length(arrDataL)))
	  then
	  begin
	    S3 := S3 + '*' + arrDataL[Index + 1];
	  end;

	  // 
	  Index := InArray('CYL', False, arrDataL);

	  if (Index >= 0)
	  and ((Index + 1) < (Length(arrDataL)))
	  then
    	  begin
	    S5 := S5 + arrDataL[Index] + '= ' + arrDataL[Index + 1];
	  end;
  	end;

  	//
  	ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
  	ParsedData := ParsedData + S2 + ' ' + S3 + FOutputLineSeparator;
  	
  	//
  	ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
  	ParsedData := ParsedData + S4 + ' ' + S5 + FOutputLineSeparator;

  	// Set result
  	FParsedDataString:= RawByteString(ParsedData);
end.