const
  VERSION = '1.5.55.48';
  DATE = '05.12.2022 08:38:44';
  TEXT = 'Copyright (c) 2022 team2work GmbH';

  LINE_SEPARATOR = '@';

  GDT_FID_PATIENT_ID   = '3000';
  GDT_FID_MEASURE_DATA = '6228';
  GDT_FID_COMMENT      = '6227';
  GDT_FID_RESULT       = '6220';

  DATA_SEPARATOR_SOH = #$01; // SOH (Start-of-Header)
  DATA_SEPARATOR_STX = #$02; // STX (Start-Text)
  DATA_SEPARATOR_EOT = #$04; // EOT (End-of-Transmission)

  DATA_LINE_SEPARATOR_01 = #$0D#$0A;
  DATA_LINE_SEPARATOR_02 = #$0D;

var
  arrData, arrDataTmp: TStringArray;
  ParsedData, S, PatientID, A, B: String;
  i, intPosL, intPosPD, intPosWD, intPosRADD, intPosVD, intPosVA, intPosLADD, intPosPR, intPosPL, intPosVisus: Integer;
  R_Line, R_SPH, R_CYL, R_AXIS, R_ADD, R_PD, R_WD, R_VD, R_VA, R_PRISM: String;
  L_Line, L_SPH, L_CYL, L_AXIS, L_ADD, L_PD, L_PRISM: String;
  R_VISUS, L_VISUS, B_VISUS: String;
  bolAddWDValueToOutput, bolAddPDValueToOutput, bolUseStrictMode, bolAddVisusToOutput: Boolean;
  strictCompareChar: Char;
begin
  // Verwende "True", damit der WD Wert hinzugefügt wird, benutze "False", damit der
  // WD Wert nicht per GDT exportiert wird.
  bolAddWDValueToOutput:= True;
  	
  // Verwende "True", damit der PD Wert hinzugefügt wird, benutze "False", damit der
  // PD Wert nicht per GDT exportiert wird.
  bolAddPDValueToOutput:= True;  	
  	
  // Verwende "True", damit nur bestimmte Messungen z.B. "@RT F" gefunden wird, 
  // benutze "False", damit alle "RT" Messungen verwendet werden. 	
  bolUseStrictMode:= False;
  	
  // Verwende "True", damit Visus-Daten zur Ausgabe hinzugefügt werden,
  // benutze "False", damit Visus-Daten nicht exportiert werden.
  bolAddVisusToOutput:= True;

  // Aktivieren des "Strict modes", und setzen eines Zeichens zum identifizieren
  // der gewünschten Messung vom Geröt. Es kann NUR auf ein Zeichen verglichen werden.
  // Möglich Zeichen zum Vergleich sind für die "@RT" Messung: "f", "F".
  strictCompareChar:= 'F';

  // --- Don't edit script down below ---

  // Clear parsed data string
  FParsedDataString := '';

  // T2WMessageBoxS(FRawDataString);
  
  // DoPSSaveFile('test1.txt');

  if (FRawDataString <> '') then
   begin
    if T2WContainsStr(FRawDataString, DATA_LINE_SEPARATOR_01) then
     begin
      FRawDataString := T2WStringReplace(FRawDataString, DATA_LINE_SEPARATOR_01, '', True, False);
      FRawDataString := T2WStringReplace(FRawDataString, DATA_SEPARATOR_SOH, '', True, False);
      FRawDataString := T2WStringReplace(FRawDataString, DATA_SEPARATOR_STX, '', True, False);
      FRawDataString := T2WStringReplace(FRawDataString, DATA_SEPARATOR_EOT, '', True, False);
    end;
    
    if T2WContainsStr(FRawDataString, DATA_LINE_SEPARATOR_02) then
     begin
      FRawDataString := T2WStringReplace(FRawDataString, DATA_LINE_SEPARATOR_02, '', True, False);
      FRawDataString := T2WStringReplace(FRawDataString, DATA_SEPARATOR_SOH, '', True, False);
      FRawDataString := T2WStringReplace(FRawDataString, DATA_SEPARATOR_STX, '', True, False);
      FRawDataString := T2WStringReplace(FRawDataString, DATA_SEPARATOR_EOT, '', True, False);
    end;    
  end;

  // Get array of lines
  arrData := Explode(LINE_SEPARATOR, FRawDataString, 0);

  if Length(arrData) <= 0 then
   begin
    FLastErrorCode := -4;
    FLastErrorMessage := 'Keine Zeilen für die Verarbeitung verfügbar';

    DoPSError();

    Exit;
  end;
  
  PatientID := '';

  // Build output
  ParsedData := '';

  // Add patient ID
  if (PatientID <> '') then
   begin
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
    ParsedData := ParsedData + PatientID + FOutputLineSeparator;
  end;

  for i := 0 to Length(arrData) - 1 do
   begin
    S := arrData[i];
    
    A:= '';
    B:= '';

    // T2WMessageBoxS(S);

    //
    intPosPD:= 0;
    intPosWD:= 0;
    intPosRADD:= 0;
    intPosLADD:= 0;
    intPosVD:= 0;
    intPosVA:= 0;
    intPosPR:= 0;
    intPosPL:= 0;
    intPosVisus:= 0;

    // 
    intPosL:= 0;

    //
    R_SPH:= '';
    R_CYL:= '';
    R_AXIS:= '';
    R_ADD:= '';
    R_PRISM:= '';

    //
    L_SPH:= '';
    L_CYL:= '';
    L_AXIS:= '';
    L_ADD:= '';
    L_PRISM:= '';

    // 
    R_PD:= '';
    R_WD:= '';
    R_VD:= '';
    R_VA:= '';

    //
    L_PD:= '';

    //
    R_Line:= '';
    L_Line:= '';
    	
    //	
    R_VISUS:= '';
    L_VISUS:= '';
    B_VISUS:= '';
    	
    // Data source
    
    // LM : Lensmeter (Lensometry data) For day data, or when day/night is not specified
    // lm : Lensmeter (Lensometry data) For night data
    // RM : Autorefractometer (Objective data) For day data, or when day/night is not specified
    // rm : Autorefractometer (Objective data) For night data
    // WF : Wavefront For day data, or when day/night is not specified
    // wf : Wavefront For night data
    
    // RT : Refractor (Unaided, subjective and final prescription data) For day data, or when day/night is not specified
    // rt : Refractor (Unaided, subjective and final prescription data) For night data
    
    // KM : Keratometer
    // NT : Tonometer (Intraocular pressure data)

    if (T2WStartsStr('RT', S)) or T2WStartsStr('rt', S) then
     begin
      if (Length(S) > 2) then
       begin
        // remove Data source tag from string
        S:= Copy(S, 3, Length(S));
        S:= Trim(S);

        // RT data
        if (S[1] in ['f', 'n', 'a', 'v', 'p', 'F', 'N', 'A', 'V', 'P', 'W']) then
         begin
          if (bolUseStrictMode) then
          begin
            if (S[1] = strictCompareChar) then
            begin
              // do nothing
            end
            else
            begin
              Continue;
            end;
          end;  

          // Text Data Format of Each Data Source
          // Far vision : R = Right eye, L = Left eye, B = Binocular, D = PD, V = DV/CV
          // Near vision : r = Right eye, l = Left eye, b = Binocular, d = PD, v = DV/DC
          if (S[2] in ['r', 'l', 'b', 'd', 'v', 'R', 'L', 'B', 'D', 'V']) then
           begin
            // Add right eye
            if (S[2] = 'r') or (S[2] = 'R') then
             begin
              //
              R_SPH:= Copy(S, 3, 6);

              //
              R_CYL:= Copy(S, 9, 6);

              //
              R_AXIS:= Copy(S, 15, 3);

              //
              if (R_SPH <> '') then
               R_SPH:= 'S=' + R_SPH;

              if (R_CYL <> '') then
               R_CYL:= 'Z=' + R_CYL;

              if (R_AXIS <> '') then
               R_AXIS:= '*' + R_AXIS;

              //
              intPosRADD:= POS('AR', S);

              if (intPosRADD = 0) then
               intPosRADD:= POS('aR', S);

              if (intPosRADD > 0) then
               begin
                R_ADD:= Copy(S, intPosRADD+2, 6);
              end;

              if (R_ADD <> '') then
               R_ADD := 'A=' + R_ADD;

              // 
              intPosVisus:= POS('vR', S);
              	
              if (intPosVisus > 0) then
              begin
                R_VISUS:= Trim(Copy(S, intPosVisus+2, 5));
              end;
              
              intPosVisus:= POS('vL', S);	
              	
              if (intPosVisus > 0) then
              begin
                L_VISUS:= Trim(Copy(S, intPosVisus+2, 5)); 
              end;
              
              intPosVisus:= POS('vB', S);	              	
              	
              if (intPosVisus > 0) then
              begin
                B_VISUS:= Trim(Copy(S, intPosVisus+2, 5)); 
              end;              	

              // 	
              intPosPR:= POS('PR', S);

              if (intPosPR = 0) then
              begin	
                intPosPR:= POS('pR', S);
              end; 	

              if (intPosPR > 0) then
              begin
                R_PRISM:= Copy(S, intPosPR+2, 12);
              end;

              if (R_PRISM <> '') then
              begin
                A:= Copy(R_PRISM, 0, 6);
                B:= Copy(R_PRISM, 7, 12);
                	
                if (A <> '') then
                begin
                  arrDataTmp := Explode(' ', A, 0);
                  	
                  if (Length(arrDataTmp) > 0) then
                  begin
                    A:= arrDataTmp[1] + ' ' + arrDataTmp[0];	
                  end;
                end;
                	
                if (B <> '') then
                begin
                  arrDataTmp := Explode(' ', B, 0);
                  	
                  if (Length(arrDataTmp) > 0) then
                  begin
                    B:= arrDataTmp[1] + ' ' + arrDataTmp[0];	
                  end;
                end;                	
                
                if (A <> '') and (B <> '') then
                begin
                  R_PRISM:= A + ' ' + B;
                end;

                R_PRISM:= 'P= ' + R_PRISM;
              end;

              //
              intPosPD := POS('PD', S);

              if (intPosPD = 0) then
               intPosPD:= POS('pD', S);

              if (intPosPD > 0) and (bolAddPDValueToOutput) then
               begin
                R_PD:= 'PD=' + ' ' + Copy(S, intPosPD+2, 4);
              end;

              //
              intPosWD:= POS('WD', S);

              if (intPosWD <> 0) then
               begin
                R_WD:= 'WD=' + ' ' + Copy(S, intPosWD+2, 2);
              end;

              // 
              intPosVD:= POS('VD', S);

              if (intPosVD <> 0) then
               begin
                R_VD:= Copy(S, intPosVD+2, 5);
              end;

              //
              intPosVA:= POS('VA', S);

              if (intPosVA <> 0) then
               begin
                R_VA:= Copy(S, intPosVA+2, 5);
              end;

              // Add right eye
              R_Line:= '';

              if (R_SPH <> '') and (R_CYL <> '') and (R_AXIS <> '') then
               begin
                R_Line:= R_Line + 'R.:' + R_SPH + ' ' + R_CYL + R_AXIS;

                if (S[1] in ['f', 'n', 'F', 'N']) then
                 begin
                  R_Line:= UpperCase(S[1]) + ' ' + R_Line;
                end;
              end;
              
              if (R_PRISM <> '') then
              begin
                R_Line:= R_Line + ' ' + R_PRISM; 
              end;

              if (R_ADD <> '') then
              begin
                R_Line:= R_Line + ' ' + R_ADD; 
              end; 	               

              if (R_PD <> '') then
              begin
                R_Line:= R_Line + ' ' + R_PD;
              end; 	

              if (R_Line <> '') then
               begin
                ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
                ParsedData:= ParsedData + R_Line + FOutputLineSeparator;
              end;
            end;

            // Add left eye
            intPosL:= POS('FL', S);

            if (intPosL = 0) then
             begin
              intPosL:= POS('fl', S);

              if (intPosL = 0) then
               begin
                intPosL:= POS('Fl', S);

                if (intPosL = 0) then
                 begin
                  intPosL:= POS('fL', S);
                end;
              end;
            end;

            if (intPosL <> 0) then
             begin
              S:= Copy(S, intPosL, Length(S));

              if (S[1] in ['f', 'n', 'a', 'v', 'p', 'F', 'N', 'A', 'V', 'P']) then
               begin
				        if (bolUseStrictMode) then
				        begin
				          if (S[1] = strictCompareChar) then
				          begin
				            // do nothing
				          end
				          else
				          begin
				            Continue;
				          end;   
				        end;             

                // Text Data Format of Each Data Source
                if (S[2] in ['r', 'l', 'b', 'd', 'v', 'R', 'L', 'B', 'D', 'V']) then
                 begin
                  // Add left eye
                  if (S[2] = 'l') or (S[2] = 'L') then
                   begin
                    //
                    L_SPH:= Copy(S, 3, 6);

                    //
                    L_CYL:= Copy(S, 9, 6);

                    //
                    L_AXIS:= Copy(S, 15, 3);

                    //
                    if (L_SPH <> '') then
                     L_SPH:= 'S=' + L_SPH;

                    if (L_CYL <> '') then
                     L_CYL:= 'Z=' + L_CYL;

                    if (L_AXIS <> '') then
                     L_AXIS:= '*' + L_AXIS;

                    //
                    intPosLADD:= POS('AL', S);

                    if (intPosLADD = 0) then
                     intPosLADD:= POS('aL', S);

                    if (intPosLADD > 0) then
                     begin
                      L_ADD:= Copy(S, intPosLADD+2, 6);
                    end;

                    if (L_ADD <> '') then
                     L_ADD := 'A=' + L_ADD;

                    //
                    intPosPL:= POS('PL', S);

                    if (intPosPL = 0) then
                    begin	
                      intPosPL:= POS('pL', S);
                    end; 	

                    if (intPosPL > 0) then
                    begin
                      L_PRISM:= Copy(S, intPosPL+2, 12);
                    end;

                    if (L_PRISM <> '') then
                    begin
                      A:= Copy(L_PRISM, 0, 6);
                      B:= Copy(L_PRISM, 7, 12);                      
                     
			                if (A <> '') then
			                begin
			                  arrDataTmp := Explode(' ', A, 0);
			                  	
			                  if (Length(arrDataTmp) > 0) then
			                  begin
			                    A:= arrDataTmp[1] + ' ' + arrDataTmp[0];	
			                  end;
			                end;
			                	
			                if (B <> '') then
			                begin
			                  arrDataTmp := Explode(' ', B, 0);
			                  	
			                  if (Length(arrDataTmp) > 0) then
			                  begin
			                    B:= arrDataTmp[1] + ' ' + arrDataTmp[0];	
			                  end;
			                end;                	
			                
			                if (A <> '') and (B <> '') then
			                begin
			                  L_PRISM:= A + ' ' + B;
			                end; 

                      L_PRISM:= 'P= ' + L_PRISM;
                    end;

                    //
                    intPosPD := POS('PD', S);

                    if (intPosPD = 0) then
                     intPosPD:= POS('pD', S);

                    if (intPosPD > 0) and (bolAddPDValueToOutput) then
                     begin
                      L_PD:= 'PD=' + ' ' + Copy(S, intPosPD+2, 4);
                    end;

                    // Add left eye
                    L_Line:= '';

                    if (L_SPH <> '') and (L_CYL <> '') and (L_AXIS <> '') then
                     begin
                      L_Line:= L_Line + 'L.:' + L_SPH + ' ' + L_CYL + L_AXIS;

                      if (S[1] in ['f', 'n', 'F', 'N']) then
                       begin
                        L_Line:= UpperCase(S[1]) + ' ' + L_Line;
                      end;
                    end;
                    
                    if (L_PRISM <> '') then
                    begin
                      L_Line:= L_Line + ' ' + L_PRISM; 
                    end;                    
                    
                    if (L_ADD <> '') then
                    begin
                      L_Line:= L_Line + ' ' + L_ADD;  
                    end;	                   
                    
                    if (L_PD <> '') then
                    begin
                      L_Line:= L_Line + ' ' + L_PD;    
                    end; 	                  
                    
                    if (L_Line <> '') then
                     begin
                      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
                      ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
                    end;
                  end;
                end;
              end;
            end;
          end;
        end;
      end;
    end;

    // LM : Lensmeter (Lensometry data) For day data, or when day/night is not specified
    // lm : Lensmeter (Lensometry data) For night data
    if (T2WStartsStr('LM', S)) or T2WStartsStr('lm', S) then
     begin
      if (Length(S) > 2) then
       begin
        // do nothing
      end;
    end;

    // RM : Autorefractometer (Objective data) For day data, or when day/night is not specified
    // rm : Autorefractometer (Objective data) For night data
    if (T2WStartsStr('RM', S)) or T2WStartsStr('rm', S) then
     begin
      if (Length(S) > 2) then
       begin
        // do nothing
      end;
    end;
  end;

  // BUGFIX: if empty lines are in output file, the importer could not load converted data
  ParsedData:= Trim(ParsedData);

  // Add WD if exists
  if (R_WD <> '') and (bolAddWDValueToOutput) then
  begin
    // Append WD index with GDT comment field id to measure data
    ParsedData := ParsedData + FOutputLineSeparator + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_COMMENT + FOutputLineSeparator + Trim(R_WD);
  end;
  
  // Add Visus if exists
  if (bolAddVisusToOutput) then
  begin
    if (R_VISUS <> '') or (L_VISUS <> '') or (B_VISUS <> '') then
    begin
      S:= 'R:cc=' + R_VISUS + ' // L:cc=' + L_VISUS + ' // B:cc=' + B_VISUS;
    
      ParsedData := ParsedData + FOutputLineSeparator + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_RESULT + FOutputLineSeparator + Trim(S);
    end;  
  end;

  // Set output
  FParsedDataString := RawByteString(ParsedData);
end.