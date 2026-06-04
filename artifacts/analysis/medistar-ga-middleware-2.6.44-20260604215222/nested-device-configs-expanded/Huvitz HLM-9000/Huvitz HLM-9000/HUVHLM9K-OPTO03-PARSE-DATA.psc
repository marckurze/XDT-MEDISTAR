const
	VERSION = '1.0.2.36';
	DATE = '14.11.2019 10:06:17';
	TEXT = 'Copyright (c) 2019 team2work GmbH';
	
	DATA_SEPARATOR_01 = '   ';
	DATA_SEPARATOR_02 = ' ';
	DATA_SEPARATOR_03 = '=';
	
	ARRAY_SIZE_01 = 5;
	ARRAY_SIZE_02 = 9;
	ARRAY_SIZE_03 = 2;
	RIGHT_EYE_DATA_ARRAY_INDEX = 2;
	LEFT_EYE_DATA_ARRAY_INDEX = 3;
	
	OUTPUT_DATA_NAME_VALUE_SEPARATOR = '=';
	OUTPUT_DATA_VALUE_SEPARATOR = ' ';
	OUTPUT_UV_INDEX_SEPARATOR = ';';
	
	// OUTPUT_DATA_LINE_SEPARATOR = #13#10;
	PARAM_NAME_ARRAY_INDEX = 0;
	PARAM_VALUE_ARRAY_INDEX = 1;
	
	// GDT_FID_FORMAT = 'GDTFID=';
	GDT_FID_MEASURE_DATA = '6228';
	GDT_FID_COMMENT = '6227';

var
	Data, Sep1, Sep2, Sep3, EyeData, TempData, ParsedData: String;
	arrData1, arrData2, arrData3: TStringArray;
	i, j: Integer;
	ParamName, ParamValue, S1, S2: String;
	bolAddUVMeasureIndexToGDT: Boolean;

begin
	// Der UV Index aus der Messung wird mittels BOOLEAN Variable per GDT exportiert.
	// Soll der UV Index als z.B. "Y" Zeile (GDTID 6227) in die MD eingetragen werden,
	// dann setzt man die BOOLEAN Variable auf True andernfalls auf False.
	// Bitte Schreibweise und Zeilenende ";" beachten.
	bolAddUVMeasureIndexToGDT:= True;
	
	// --- Don't edit script down below ---
	
	// Clear parsed data string
	FParsedDataString:= '';

	if Length(FRawDataString) <= 0 then
	begin
		FLastErrorCode:= -2;
		FLastErrorMessage:= 'Keine COM-Daten für die Verarbeitung verfügbar';

		DoPSError;

		Exit;
	end;

	TempData:= '';
	ParsedData:= '';

	Sep1:= DATA_SEPARATOR_01;
	Sep2:= DATA_SEPARATOR_02;
	Sep3:= DATA_SEPARATOR_03;

	Data:= String(FRawDataString);
	Data:= Trim(Data);

	// Pre-format whole data string
	Data:= T2WStringReplace(Data, 'R: ', 'R.: ', False, False);
	Data:= T2WStringReplace(Data, 'L: ', 'L.: ', False, False);
	Data:= T2WStringReplace(Data, ' C=', ' Z=', True, False);
	Data:= T2WStringReplace(Data, ' A=', ' *=', True, False);
	Data:= T2WStringReplace(Data, ' PX=', ' P=', True, False);
	Data:= T2WStringReplace(Data, ' ADD=', ' A=', True, False);
	// Data:= T2WStringReplace(Data, ' ADD2=', ' A2=', True, False);

	// Get array from 1st separator
	arrData1:= Explode(Sep1, Data, 0);

	if Length(arrData1) <> ARRAY_SIZE_01 then
	begin
		FLastErrorCode:= -3;
		FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): "'+IntToStr(ARRAY_SIZE_01)+'" Parameter erwartet, "'+IntToStr(Length(arrData1))+'" Parameter gefunden';

		DoPSError;

		Exit;
	end;

	// ---

	for i:= RIGHT_EYE_DATA_ARRAY_INDEX to LEFT_EYE_DATA_ARRAY_INDEX do
	begin
		// Process right eye
		EyeData:= arrData1[i];

		// Get array from 2nd separator
		arrData2:= Explode(Sep2, EyeData, 0);

		if Length(arrData2) <> ARRAY_SIZE_02 then
		begin
			if i = RIGHT_EYE_DATA_ARRAY_INDEX then
			begin
				FLastErrorCode:= -4;
				FLastErrorMessage:= 'Kein valides Datenformat (Rechtes Auge): "'+IntToStr(ARRAY_SIZE_02)+'" Parameter erwartet, "'+IntToStr(Length(arrData2))+'" Parameter gefunden';
			end
			else if i = LEFT_EYE_DATA_ARRAY_INDEX then
			begin
				FLastErrorCode:= -6;
				FLastErrorMessage:= 'Kein valides Datenformat (Linkes Auge): "'+IntToStr(ARRAY_SIZE_02)+'" Parameter erwartet, "'+IntToStr(Length(arrData2))+'" Parameter gefunden';
			end;

			DoPSError;

			Exit;
		end;

		// We need to switch array values 6 (PD) and 7 (ADD) for MD output
		S1:= arrData2[6];
		S2:= arrData2[7];
		arrData2[6]:= S2;
		arrData2[7]:= S1;
		
		// Add GDT measure data field id for first line
		ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;

		// First array entry is the eye prefix, so we add it to the output data
		ParsedData:= ParsedData + arrData2[0];

		for j:= 1 to ARRAY_SIZE_02 - 1 do
		begin
			// Get array from 3rd separator
			arrData3:= Explode(Sep3, arrData2[j], 0);
			
			if Length(arrData3) <> ARRAY_SIZE_03 then
			begin
				if i = RIGHT_EYE_DATA_ARRAY_INDEX then
				begin
					FLastErrorCode:= -5;
					FLastErrorMessage:= 'Kein valides Datenformat (Rechtes Auge - Einzelner Parameter): "'+IntToStr(ARRAY_SIZE_03)+'" Parameter erwartet, "'+IntToStr(Length(arrData3))+'" Parameter gefunden';
				end
				else if i = LEFT_EYE_DATA_ARRAY_INDEX then
				begin
					FLastErrorCode:= -7;
					FLastErrorMessage:= 'Kein valides Datenformat (Linkes Auge - Einzelner Parameter): "'+IntToStr(ARRAY_SIZE_03)+'" Parameter erwartet, "'+IntToStr(Length(arrData3))+'" Parameter gefunden';
				end;

				DoPSError;

				Exit;
			end;

			// For the array values with indexes "1" and "3" we do not add value separator
			if (j <> 1)
			and (j <> 3)
			and (j <> 5)
			and (j <> 8)
			then
				ParsedData:= ParsedData + OUTPUT_DATA_VALUE_SEPARATOR;

			if (j = 1)
			or (j = 2)
			or (j = 4)
			then
			begin
				ParamValue:= arrData3[PARAM_VALUE_ARRAY_INDEX];

				// Add formatted param name and value to the output data
				ParsedData:= ParsedData + arrData3[PARAM_NAME_ARRAY_INDEX] + OUTPUT_DATA_NAME_VALUE_SEPARATOR + ParamValue;

				if j = 4 then
				begin
					if T2WStartsStr('-', ParamValue) then
						ParsedData:= ParsedData + OUTPUT_DATA_VALUE_SEPARATOR + 'O'
					else
						ParsedData:= ParsedData + OUTPUT_DATA_VALUE_SEPARATOR + 'I';
				end;

				Continue;
			end
			else if (j = 3) then
			begin
				ParamValue:= arrData3[PARAM_VALUE_ARRAY_INDEX];

				// Add formatted param name and value to the output data
				ParsedData:= ParsedData + arrData3[PARAM_NAME_ARRAY_INDEX] + ParamValue;

				Continue;
			end
			else if (j = 5) then
			begin
				ParamValue:= arrData3[PARAM_VALUE_ARRAY_INDEX];

				// Add formatted param name and value to the output data
				ParsedData:= ParsedData + OUTPUT_DATA_VALUE_SEPARATOR + ParamValue;

				if T2WStartsStr('-', ParamValue) then
					ParsedData:= ParsedData + OUTPUT_DATA_VALUE_SEPARATOR + 'D'
				else
					ParsedData:= ParsedData + OUTPUT_DATA_VALUE_SEPARATOR + 'U';
			end
			else if (j = 6) then
			begin
				ParamValue:= arrData3[PARAM_VALUE_ARRAY_INDEX];

				// Add formatted param name and value to the output data
				ParsedData:= ParsedData + arrData3[PARAM_NAME_ARRAY_INDEX] + OUTPUT_DATA_NAME_VALUE_SEPARATOR + ParamValue;
			end
			else if (j = 7) then
			begin
				ParamValue:= arrData3[PARAM_VALUE_ARRAY_INDEX];

				// Add formatted param name and value to the output data
				ParsedData:= ParsedData + arrData3[PARAM_NAME_ARRAY_INDEX] + OUTPUT_DATA_NAME_VALUE_SEPARATOR + ParamValue;
			end
			else if (j = 8) then
			begin
				ParamName:= arrData3[PARAM_NAME_ARRAY_INDEX];
				ParamValue:= arrData3[PARAM_VALUE_ARRAY_INDEX];
				ParamValue:= ParamValue + '%';

				// Param "UR" needs to be stored in temp var, because output gets
				// appended to parsed data in new line

				if i = RIGHT_EYE_DATA_ARRAY_INDEX then
				begin
					ParamName:= T2WStringReplace(ParamName, 'UR', 'R: UV', False, False);
					TempData:= TempData + ParamName + OUTPUT_DATA_VALUE_SEPARATOR + OUTPUT_DATA_NAME_VALUE_SEPARATOR + OUTPUT_DATA_VALUE_SEPARATOR + ParamValue;
				end
				else if i = LEFT_EYE_DATA_ARRAY_INDEX then
				begin
					// Param "LR" needs to be stored in temp var, because output gets
					// appended to parsed data in new line
					ParamName:= T2WStringReplace(ParamName, 'UL', 'L: UV', False, False);

					TempData:= TempData + OUTPUT_UV_INDEX_SEPARATOR + OUTPUT_DATA_VALUE_SEPARATOR + ParamName + OUTPUT_DATA_VALUE_SEPARATOR + OUTPUT_DATA_NAME_VALUE_SEPARATOR + OUTPUT_DATA_VALUE_SEPARATOR + ParamValue;
				end;
			end;
		end;

		// Add new line separator
		ParsedData:= ParsedData + FOutputLineSeparator;
	end;

	// Append UV index with GDT comment field id to measure data
	if (bolAddUVMeasureIndexToGDT) then
	  ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_COMMENT + FOutputLineSeparator + TempData;

	FParsedDataString:= RawByteString(ParsedData);
end.
