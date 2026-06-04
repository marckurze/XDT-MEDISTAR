const
  VERSION = '1.0.55.74';
  DATE = '05.07.2022 14:08:19';
  TEXT = 'Copyright (c) 2022 team2work GmbH';

  // #13 -> CR / #10 -> LF
  DATA_SEPARATOR_01 = #13#10;
  DATA_SEPARATOR_02 = '  ';
	
  LINE_IDENTIFIER_M = 'M ';
  LINE_IDENTIFIER_T = 'T ';
  LINE_IDENTIFIER_R = 'R ';
  LINE_IDENTIFIER_L = 'L ';
  LINE_IDENTIFIER_PD = 'PD=';
	
  GDT_FID_MEASURE_DATA         = '6228';
	
  GDT_LINE_PREFIX              = '  ';
        
  GDT_SIGN_SEPARATOR           = ' ';
  GDT_AXIS_SEPARATOR           = ' ';
  GDT_AXIS_SEPARATOR_MAX_COUNT = 3;	

function FormatSignValue(const ID, Value: String; AddSign: Boolean; AddSignSeparator: Boolean): String;
var
  S1, S2: String;
begin
  Result:= ID;

  S1:= Trim(Value);
  
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
  
  S1:= Trim(S1);
     
  // if the e.g. Spheres value is two digits, 
  // then there must be no space between sign and value 
  if AddSignSeparator then
    if Length(S1) = 4 then
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
	Data, Sep1, Sep2, ParsedData: String;
	arrData1, arrDataVD, arrDataR, arrDataL, arrDataPD: TStringArray;
	i, j, Index: Integer;
	S1, S2, S3, strGDT_LINE_PREFIX: String;
	bolStrictExaminationOfValues: Boolean;
        bolAddPPSValueToOutput: Boolean;
	bolAddSign, bolAddSignSeparator, bolAddAxisSeparator, bolAddPrefixForEachEyeLine: Boolean;
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

	// Aktiviere strikte Prüfung, ob alle "Felder" (VD, PD, R, L) vorhanden sein müssen.
	// Verwende "True", damit die Prüfung aktiviert ist, benutze "False", damit die
	// Prüfung ausgeschalten ist.
	bolStrictExaminationOfValues:= False;
	
	// Verwende "True", damit der PPS Wert hinzugefügt wird, benutze "False", damit der
	// PPS Wert nicht per GDT exportiert wird. PPS = Photopic Pupil Size.
	bolAddPPSValueToOutput:= True;
	
	// Verwende "True", damit vor jeder Zeile (pro Auge) Anzahl x Leerzeichen als Prefix
	// vorangestellt werden, benutze "False", damit dies nicht geschiet.
	bolAddPrefixForEachEyeLine:= False;
	
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

        strGDT_LINE_PREFIX:= '';
        
        if (bolAddPrefixForEachEyeLine) then
        begin
          strGDT_LINE_PREFIX:= GDT_LINE_PREFIX;
        end;
               
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
	    S1:= T2WStringReplace(S1, LINE_IDENTIFIER_R, 'R.:  ', False, False);
	    S1:= T2WStringReplace(S1, 'S  ', 'S=  ', False, False);
	    S1:= T2WStringReplace(S1, ' C  ', '  Z=  ', False, False);
	    S1:= T2WStringReplace(S1, ' A  ', '  *  ', False, False);
	    S1:= T2WStringReplace(S1, ' PPS  ', '  PPS=  ', False, False);
		
	    arrDataR:= Explode(Sep2, S1, 0);
	  end
	  else if T2WStartsText(LINE_IDENTIFIER_L, S1) then
	  begin
	    S1:= T2WStringReplace(S1, LINE_IDENTIFIER_L, 'L.:  ', False, False);
	    S1:= T2WStringReplace(S1, 'S  ', 'S=  ', False, False);
	    S1:= T2WStringReplace(S1, ' C  ', '  Z=  ', False, False);
	    S1:= T2WStringReplace(S1, ' A  ', '  *  ', False, False);
	    S1:= T2WStringReplace(S1, ' PPS  ', '  PPS=  ', False, False);
		
	    arrDataL:= Explode(Sep2, S1, 0);
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
	
	// ---------------------------------------------------------
	
	// Process right eye
	if Length(arrDataR) > 0 then
	begin		
	  Index:= InArray('R.:',
	                  False,
			  arrDataR);
	
  	  if Index >= 0 then
	  begin
	    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	    ParsedData:= ParsedData + strGDT_LINE_PREFIX + arrDataR[Index];
	  end;
	
	  Index:= InArray('S=',
	                  False,
	     		  arrDataR);
	
	  if (Index >= 0)
	  and ((Index + 1) < (Length(arrDataR)))
	  then
	  begin
	    ParsedData:= ParsedData + FormatSignValue('S=', Trim(arrDataR[Index + 1]), bolAddSign, bolAddSignSeparator);
	  end;
	
	  Index:= InArray('Z=',
	                  False,
			  arrDataR);
	
	  if (Index >= 0)
	  and ((Index + 1) < (Length(arrDataR)))
	  then
	  begin
	    S3:= Trim(arrDataR[Index + 1]);
	    	
	    if (S3 = '0.00 A') then
	    begin
	      S3:= '0.00';	
	    end;	

	    ParsedData:= ParsedData + ' ' + FormatSignValue('Z=', S3, bolAddSign, bolAddSignSeparator);	
	  end;

	  Index:= InArray('*',
	                  False,
			  arrDataR);

	  if (Index >= 0)
	  and ((Index + 1) < (Length(arrDataR)))
	  then
	  begin   	  
	    if Length(arrDataL) > 0 then
	    begin	    
	      try
	        if (arrDataL[Index + 1] <> 'PPS=') then
	        begin
	          if (arrDataR[Index + 1] = 'PPS=') then
	          begin
	            // we have no axis value given in the measurement
	            ParsedData:= ParsedData + FormatAxisValue('*', '0', bolAddAxisSeparator);
	          end;	         
	      
	          if (arrDataR[Index + 1] <> 'PPS=') then
	          begin
                    ParsedData:= ParsedData + FormatAxisValue('*', Trim(arrDataR[Index + 1]), bolAddAxisSeparator);                                                              
                  end;
	        end
                else
                begin
                  ParsedData:= ParsedData + arrDataR[Index];
                end;    
              except
                if (arrDataR[Index + 1] = 'PPS=') then
	        begin
	          // we have no axis value given in the measurement
	          ParsedData:= ParsedData + FormatAxisValue('*', '0', bolAddAxisSeparator);
	        end;	         
	      
	        if (arrDataR[Index + 1] <> 'PPS=') then
	        begin
                  ParsedData:= ParsedData + FormatAxisValue('*', Trim(arrDataR[Index + 1]), bolAddAxisSeparator);                                                              
                end;
              end;  
	    end
	    else
	    begin	      
	      if (arrDataR[Index + 1] = 'PPS=') then
	      begin
	        ParsedData:= ParsedData + FormatAxisValue('*', '0', bolAddAxisSeparator);
	      end
	      else
	      begin	       	     	      
	        // ParsedData:= ParsedData + arrDataR[Index];
	        
	        if ((Index + 1) <= (Length(arrDataR))) then
	        begin
	          ParsedData:= ParsedData + FormatAxisValue('*', Trim(arrDataR[Index + 1]), bolAddAxisSeparator);
	        end;
	      end;	
	    end; 		
	  end; 	
	end;	

	// PD
	if (Length(arrDataPD) > 0) and (Length(arrDataR) > 0) then
	begin	
	  Index:= InArray('PD=',
	                  False,
			  arrDataPD);
	
	  if (Index >= 0)
	  and ((Index + 1) < (Length(arrDataPD)))
	  then
	  begin
	    ParsedData:= ParsedData + ' ' + arrDataPD[Index] + ' ' + arrDataPD[Index + 1];
	  end;
	end;
	
	// VD
	if (Length(arrDataVD) > 0) and (Length(arrDataR) > 0) then
	begin
	  Index:= InArray('VD=',
	                  False,
			  arrDataVD);
	
	  if (Index >= 0)
	  and ((Index + 1) < (Length(arrDataVD)))
	  then
	  begin
	    ParsedData:= ParsedData + ' ' + arrDataVD[Index] + ' ' + arrDataVD[Index + 1];
	  end;
	end;
	
	if (bolAddPPSValueToOutput) then
	begin	
	  if Length(arrDataR) > 0 then
	  begin
	    Index:= InArray('PPS=',
	                    False,
	  	            arrDataR);
	
	    if (Index >= 0)
	    and ((Index + 1) < (Length(arrDataR)))
	    then
	    begin
	      ParsedData:= ParsedData + ' ' + arrDataR[Index] + ' ' + arrDataR[Index + 1];
	    end;	
	  end;
	end;
	
	if (ParsedData <> '') then
	  ParsedData:= ParsedData + #13#10;
	
        // ---------------------------------------------------------	
		
	// Process left eye
	if Length(arrDataL) > 0 then
	begin
	  Index:= InArray('L.:',
	                  False,
			  arrDataL);
	
	  if Index >= 0 then
	  begin
	    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
	    ParsedData:= ParsedData + strGDT_LINE_PREFIX + arrDataL[Index];
	  end;
	
	  Index:= InArray('S=',
	                  False,
			  arrDataL);
	
	  if (Index >= 0)
	  and ((Index + 1) < (Length(arrDataL)))
	  then
	  begin
	    ParsedData:= ParsedData + FormatSignValue('S=', Trim(arrDataL[Index + 1]), bolAddSign, bolAddSignSeparator);
	  end;
	
	  Index:= InArray('Z=',
	                  False,
			  arrDataL);
	
	  if (Index >= 0)
	  and ((Index + 1) < (Length(arrDataL)))
	  then
	  begin
	    S3:= Trim(arrDataL[Index + 1]); 
	    
	    if (S3 = '0.00 A') then
	    begin
	      S3:= '0.00';	
	    end;	    
 
	    ParsedData:= ParsedData + ' ' + FormatSignValue('Z=', S3, bolAddSign, bolAddSignSeparator);	
	  end;
	
	  Index:= InArray('*',
	                  False,
			  arrDataL);
	
	  if (Index >= 0)
	  and ((Index + 1) < (Length(arrDataL)))
	  then
	  begin	    
	    if (arrDataL[Index + 1] <> 'PPS=') then
	    begin
	      S1:= Trim(arrDataL[Index + 1]);	      
	      S1:= T2WStringReplace(S1, 'PPS', '', False, False);
	      S1:= Trim(S1);
	    	    
              ParsedData:= ParsedData + FormatAxisValue('*', S1, bolAddAxisSeparator);
	    end
            else
            begin
              ParsedData:= ParsedData + arrDataL[Index]; 
            end;	  
	  end;

	  if (Length(arrDataR) <= 0) then
	  begin
	    // PD
	    if (Length(arrDataPD) > 0) then
	    begin	
	      Index:= InArray('PD=',
	                      False,
		    	  arrDataPD);
	
	      if (Index >= 0)
	      and ((Index + 1) < (Length(arrDataPD)))
	      then
	      begin
	        ParsedData:= ParsedData + ' ' + arrDataPD[Index] + ' ' + arrDataPD[Index + 1];
	      end;
	    end;	  
	  
	    // VD
	    if (Length(arrDataVD) > 0) then
	    begin
	      Index:= InArray('VD=',
	                      False,
		    	  arrDataVD);
	
	      if (Index >= 0)
	      and ((Index + 1) < (Length(arrDataVD)))
	      then
	      begin
	        ParsedData:= ParsedData + ' ' + arrDataVD[Index] + ' ' + arrDataVD[Index + 1];
	      end;
	    end;
	  end;
	
	  if (bolAddPPSValueToOutput) then
	  begin	
	    Index:= InArray('PPS=',
	                    False,
			    arrDataL);
	
	    if (Index >= 0)
	    and ((Index + 1) < (Length(arrDataL)))
	    then
	    begin
	      ParsedData:= ParsedData + ' ' + arrDataL[Index] +  ' ' + arrDataL[Index + 1];
	    end;
	  end;	
	end;

	// Set result
	FParsedDataString:= RawByteString(ParsedData);
end.