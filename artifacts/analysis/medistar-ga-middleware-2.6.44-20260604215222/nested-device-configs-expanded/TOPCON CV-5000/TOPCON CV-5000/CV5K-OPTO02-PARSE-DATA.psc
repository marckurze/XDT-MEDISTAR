const
	VERSION = '1.0.55.52';
	DATE = '28.09.2022 14:46:11';
	TEXT = 'Copyright (c) 2022 team2work GmbH';

	// set default near and far distance values
	DEFAULT_DISTANCE_FAR_1     = '500,000';
	DEFAULT_DISTANCE_NEAR_1    = '67,000';
		
	DEFAULT_DISTANCE_FAR_2     = '500.000';
	DEFAULT_DISTANCE_NEAR_2    = '67.000';
	
	// set default xml node settings
	DEFAULT_NS_SJB_TYPE_NAME_1 = 'Full Correction';
	DEFAULT_NS_SJB_TYPE_NAME_2 = 'Last Prescription';	  // alte Messwerte
	DEFAULT_NS_SJB_TYPE_NAME_3 = 'Prescription';		    // subjektive Endwerte

  DEFAULT_NS_SJB_TYPE_NAME_4 = 'Objective Data';		  // Refraktometer Daten
  DEFAULT_NS_SJB_TYPE_NAME_5 = 'Current Spectacles';  // Lensmeter Daten

	XPATH_EXPRESSION_01 = '//Ophthalmology/*[name()=''nsCommon:Common'']/*[name()=''nsCommon:Patient'']/*[name()=''nsCommon:ID'']';
  XPATH_EXPRESSION_02 = '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']/*[name()=''nsSBJ:Type'']/*[name()=''nsSBJ:TypeName'']';

	GDT_FID_PATIENT_ID           = '3000';
	GDT_FID_MEASURE_DATA         = '6228';
	GDT_FID_COMMENT              = '6227';
  GDT_FID_RESULT               = '6220';
  GDT_FID_FOREIGN_RESULT       = '6221';
  	        
	GDT_LINE_PREFIX              = '  ';
	GDT_SIGN_SEPARATOR           = ' ';
	GDT_AXIS_SEPARATOR           = ' ';
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
      
  if (AddSignSeparator) then
    S2:= S2 + GDT_SIGN_SEPARATOR;
  
  Result:= Result + S2 + S1;
end;

function FormatAxisValue(const ID, Value: String; AddAxisSeparator: Boolean): String;
var
  S: String;
begin
  Result:= ID;
  
  S:= Value;
  
  if (AddAxisSeparator) then
    while Length(S) < GDT_AXIS_SEPARATOR_MAX_COUNT do
      S:= GDT_AXIS_SEPARATOR + S;
  
  Result:= Result + S;
end;    

function CalculateAdditionValueEx(const SPH_F, SPH_N: String): String;
var
  strSPH_F, strSPH_N, strResult: String;
  a1, a2, Res: Extended; 
  bolRes: Boolean;
begin
  Result:= '';
  
  if (SPH_F = '') or (SPH_N = '') then
    Exit;
 
  strResult:= ''; 
 
  strSPH_F:= Trim(SPH_F);
  strSPH_N:= Trim(SPH_N);  

  strSPH_F:= Trim(T2WStringReplace(strSPH_F, 'S=', '', True, True));
  strSPH_N:= Trim(T2WStringReplace(strSPH_N, 'A=', '', True, True));

  strSPH_F:= Trim(T2WStringReplace(strSPH_F, ' ', '', True, True));
  strSPH_N:= Trim(T2WStringReplace(strSPH_N, ' ', '', True, True));

  bolRes:= T2WStrToFloatUniversal(strSPH_F, False, a1);
  bolRes:= T2WStrToFloatUniversal(strSPH_N, False, a2);  

  if (a2 < a1) then
  begin
    // Der Nahwert muss größer als der Fernwert sein
    strResult:= '0.00';
  end
  else
  begin      
    Res:= a2 - a1;
    
    if (Res < 0) then
      Res:= Res * -1;

    strResult:= Trim(T2WFloatToStrF(Res, 10, 2));
    strResult:= Trim(T2WStringReplace(strResult, ',', '.', True, True));  
  end;     
   
  Result:= strResult;
end;

function FormatValue(const Value: String): String;
begin
  if (Value = '') then
  begin
    Result:= Value;
    Exit;
  end;
  
  if not T2WContainsStr(Value, ',') then
  begin
    Result:= Value;
    Exit;
  end;
  
  Result:= Trim(T2WStringReplace(Value, ',', '.', True, True));
end;                   

var
    arrData, arrData2, arrData3: TStringArrayArray;
    ParsedData, PatientID, sBaseString, nsSBJTypeName, nsSBJDistance: String;
    i, j, nsSBJTypeIndex: Integer;
    R_Line, L_Line: String; 
    bolAddVDValueToOutput, bolAddPDValueToOutput, bolAddADDValueToOutput: Boolean;
    bolAddSign, bolAddSignSeparator, bolAddAxisSeparator, bolAddVisusValueToOutput: Boolean;
    bolOnlyUseFullCorrection: Boolean;
    R_SPH_F, R_CYL_F, R_AXIS_F, R_VD_F, R_PD_F, R_Add, R_SPH_N: String;     
    L_SPH_F, L_CYL_F, L_AXIS_F, L_PD_F, L_Add, L_SPH_N: String;        
    F_VA_Line, F_VA_R, F_VA_L, F_VA_B: String;

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

    // Verwende "True", damit der VD Wert an die V1 Zeile angehängt wird,
    // benutze "False", damit der VD Wert ignoriert werden kann.
    bolAddVDValueToOutput:= True;

    // Verwende "True", damit der PD Wert an die V1 Zeile angehängt wird,
    // benutze "False", damit der PD Wert ignoriert werden kann.
    bolAddPDValueToOutput:= True;
    
    // Verwende "True", damit der ADD Wert exportiert wird,
    // benutze "False", damit der ADD Wert nicht verwendet werden kann.
    bolAddADDValueToOutput:= True; 
    
    // Verwende "True", damit der Visus Wert exportiert wird,
    // benutze "False", damit der Visus Wert nicht verwendet wird.
    bolAddVisusValueToOutput:= True;        

    // Verwende "True", damit nur Werte aus "Full Correction" verwendet werden,
    // benutze "False", damit alle Endwerte verwendet werden.
    bolOnlyUseFullCorrection:= True;

    // --- Don't edit script down below ---

    // Clear parsed data string
    FParsedDataString:= '';

    if not DoPSXMLDocumentExists then
    begin
      FLastErrorCode:= -4;
      FLastErrorMessage:= 'Keine XML-Daten für die Verarbeitung verfügbar';

      DoPSError;

      Exit;
    end;

    if not DoPSXMLRootNodeExists then
    begin
      FLastErrorCode:= -5;
      FLastErrorMessage:= 'Kein XML-Wurzelknoten für die Verarbeitung verfügbar';

      DoPSError;

      Exit;
    end;
    
    // T2WMessageBoxS();
    
    // Patient ID
    PatientID:= '';
    
    SetLength(arrData, 0);
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_01);
    
    if Length(arrData) > 0 then
    begin
      PatientID:= PatientID + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
      PatientID:= PatientID + arrData[0][2] + FOutputLineSeparator;
    end;
    
    // 
    R_SPH_F:= '';
    R_CYL_F:= '';
    R_AXIS_F:= '';
    R_VD_F:= '';
    R_PD_F:= '';
    R_ADD:= '';
    R_SPH_N:= '';
    
    //      
    L_SPH_F:= ''; 
    L_CYL_F:= '';          
    L_AXIS_F:= '';
    L_PD_F:= '';
    L_ADD:= '';
    L_SPH_N:= '';
    	
    // 
    F_VA_R:= '';
    F_VA_L:= '';
    F_VA_B:= '';
    	
    F_VA_Line:= '';

    // 
    nsSBJTypeIndex:= -1;
            
    // set base path here to get values of all properties
    sBaseString:= XPATH_EXPRESSION_02;
    
    SetLength(arrData, 0);
    arrData:= DoPSGetXMLData(sBaseString);

    if Length(arrData) > 0 then
    begin           
      // so, we loop through the response array
      for i:= 0 to Length(arrData) - 1 do
      begin
        nsSBJTypeName:= arrData[i][2];
 
        if (bolOnlyUseFullCorrection) then
        begin
          if (nsSBJTypeName <> DEFAULT_NS_SJB_TYPE_NAME_1) then
          begin
            Continue;
          end;        
        end
        else
        begin
          if (nsSBJTypeName <> DEFAULT_NS_SJB_TYPE_NAME_1) then
          begin
            if (nsSBJTypeName <> DEFAULT_NS_SJB_TYPE_NAME_2) then
            begin	
              if (nsSBJTypeName <> DEFAULT_NS_SJB_TYPE_NAME_3) then
              begin	
                Continue;
              end;
            end;
          end;        
        end;
        
        nsSBJTypeIndex:= i+1;
        
        // create dynamically all xpath variables
        sBaseString:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']';
        sBaseString:= sBaseString + '/*[name()=''nsSBJ:Type''][@No=''' + IntToStr(nsSBJTypeIndex) + ''']/*[name()=''nsSBJ:ExamDistance'']/*[name()=''nsSBJ:Distance'']';
    
        SetLength(arrData2, 0);
        arrData2:= DoPSGetXMLData(sBaseString);
        
        if Length(arrData2) > 0 then
        begin        
          // so, we loop through the response array
          for j:= 0 to Length(arrData2) - 1 do
          begin
            nsSBJDistance:= arrData2[j][2];
              
            // Far distance                        
            if (nsSBJDistance = DEFAULT_DISTANCE_FAR_1) 
            or (nsSBJDistance = DEFAULT_DISTANCE_FAR_2) then
            begin
              //                
              sBaseString:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:Type''][@No=''' + IntToStr(nsSBJTypeIndex) + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + IntToStr(j+1) + ''']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:RefractionData'']';                    
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:R'']/*[name()=''nsSBJ:Sph'']';
                    
              SetLength(arrData3, 0);
              arrData3:= DoPSGetXMLData(sBaseString);                 
              
              if Length(arrData3) > 0 then
              begin
                R_SPH_F:= arrData3[0][2];

                if (R_SPH_F <> '') then
                begin
                  R_SPH_F:= FormatSignValue('S=', Trim(arrData3[0][2]), bolAddSign, bolAddSignSeparator);
                end;  
              end;
              
              //
              sBaseString:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:Type''][@No=''' + IntToStr(nsSBJTypeIndex) + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + IntToStr(j+1) + ''']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:RefractionData'']';                    
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:R'']/*[name()=''nsSBJ:Cyl'']';
                    
              SetLength(arrData3, 0);
              arrData3:= DoPSGetXMLData(sBaseString);                 
              
              if Length(arrData3) > 0 then
              begin
                R_CYL_F:= arrData3[0][2];
                    
                if (R_CYL_F <> '') then
                begin
                  R_CYL_F:= FormatSignValue('Z=', Trim(arrData3[0][2]), bolAddSign, bolAddSignSeparator);
                end;                        
              end;
              
              //           
              sBaseString:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:Type''][@No=''' + IntToStr(nsSBJTypeIndex) + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + IntToStr(j+1) + ''']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:RefractionData'']';                    
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:R'']/*[name()=''nsSBJ:Axis'']';
                    
              SetLength(arrData3, 0);
              arrData3:= DoPSGetXMLData(sBaseString);                 
              
              if Length(arrData3) > 0 then
              begin
                R_AXIS_F:= arrData3[0][2];
                    
                if (R_AXIS_F <> '') then
                begin
                  R_AXIS_F:= FormatAxisValue('*', Trim(arrData3[0][2]), bolAddAxisSeparator);
                end;                        
              end;

              //                 
              sBaseString:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:Type''][@No=''' + IntToStr(nsSBJTypeIndex) + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + IntToStr(j+1) + ''']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:RefractionData'']';                    
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:L'']/*[name()=''nsSBJ:Sph'']';                                 
              
              SetLength(arrData3, 0);
              arrData3:= DoPSGetXMLData(sBaseString);                 
              
              if Length(arrData3) > 0 then
              begin
                L_SPH_F:= arrData3[0][2];
                    
                if (L_SPH_F <> '') then
                begin
                  L_SPH_F:= FormatSignValue('S=', Trim(arrData3[0][2]), bolAddSign, bolAddSignSeparator);
                end;                        
              end;

              //
              sBaseString:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:Type''][@No=''' + IntToStr(nsSBJTypeIndex) + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + IntToStr(j+1) + ''']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:RefractionData'']';                    
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:L'']/*[name()=''nsSBJ:Cyl'']';
                    
              SetLength(arrData3, 0);
              arrData3:= DoPSGetXMLData(sBaseString);                 
              
              if Length(arrData3) > 0 then
              begin
                L_CYL_F:= arrData3[0][2];
                    
                if (L_CYL_F <> '') then
                begin
                  L_CYL_F:= FormatSignValue('Z=', Trim(arrData3[0][2]), bolAddSign, bolAddSignSeparator);
                end;                        
              end;
              
              //           
              sBaseString:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:Type''][@No=''' + IntToStr(nsSBJTypeIndex) + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + IntToStr(j+1) + ''']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:RefractionData'']';                    
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:L'']/*[name()=''nsSBJ:Axis'']';
                    
              SetLength(arrData3, 0);
              arrData3:= DoPSGetXMLData(sBaseString);                 
              
              if Length(arrData3) > 0 then
              begin
                L_AXIS_F:= arrData3[0][2];
                    
                if (L_AXIS_F <> '') then
                begin
                  L_AXIS_F:= FormatAxisValue('*', Trim(arrData3[0][2]), bolAddAxisSeparator);
                end;                        
              end;
              
              //                     
    
              // VD
              sBaseString:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:Type''][@No=''' + IntToStr(nsSBJTypeIndex) + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + IntToStr(j+1) + ''']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:RefractionData'']';                    
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:VD'']';
                    
              SetLength(arrData3, 0);
              arrData3:= DoPSGetXMLData(sBaseString);                 
              
              if Length(arrData3) > 0 then
              begin
                R_VD_F:= arrData3[0][2];
                    
                if (R_VD_F <> '') then
                begin
                  R_VD_F:= 'VD= ' + R_VD_F; 
                end;                        
              end;                          
                    
              // PD R
              sBaseString:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:Type''][@No=''' + IntToStr(nsSBJTypeIndex) + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + IntToStr(j+1) + ''']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:PD'']';                    
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:R'']';              
              
              SetLength(arrData3, 0);
              arrData3:= DoPSGetXMLData(sBaseString);                 
              
              if Length(arrData3) > 0 then
              begin
                R_PD_F:= arrData3[0][2];
                    
                if (R_PD_F <> '') then
                begin
                  R_PD_F:= 'PD= ' + R_PD_F; 
                end;                        
              end;                  
              
              // PD L
              sBaseString:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:Type''][@No=''' + IntToStr(nsSBJTypeIndex) + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + IntToStr(j+1) + ''']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:PD'']';                    
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:L'']';              
              
              SetLength(arrData3, 0);
              arrData3:= DoPSGetXMLData(sBaseString);                 
              
              if Length(arrData3) > 0 then
              begin
                L_PD_F:= arrData3[0][2];
                    
                if (L_PD_F <> '') then
                begin
                  L_PD_F:= 'PD= ' + L_PD_F; 
                end;                        
              end;               
             
              // VA
              
              //
              sBaseString:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:Type''][@No=''' + IntToStr(nsSBJTypeIndex) + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + IntToStr(j+1) + ''']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:VA'']';                    
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:R'']';              
              
              SetLength(arrData3, 0);
              arrData3:= DoPSGetXMLData(sBaseString);                 
              
              if Length(arrData3) > 0 then
              begin
                F_VA_R:= Trim(arrData3[0][2]);                     
              end;
              
              //                 
              sBaseString:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:Type''][@No=''' + IntToStr(nsSBJTypeIndex) + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + IntToStr(j+1) + ''']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:VA'']';                    
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:L'']';              
              
              SetLength(arrData3, 0);
              arrData3:= DoPSGetXMLData(sBaseString);                 
              
              if Length(arrData3) > 0 then
              begin
                F_VA_L:= Trim(arrData3[0][2]);                     
              end;
              
              //               
              sBaseString:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:Type''][@No=''' + IntToStr(nsSBJTypeIndex) + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + IntToStr(j+1) + ''']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:VA'']';                    
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:B'']';              
              
              SetLength(arrData3, 0);
              arrData3:= DoPSGetXMLData(sBaseString);                 
              
              if Length(arrData3) > 0 then
              begin
                F_VA_B:= Trim(arrData3[0][2]);                     
              end;
              
              //              

            end;

            // Near distance
            if (nsSBJDistance = DEFAULT_DISTANCE_NEAR_1) 
            or (nsSBJDistance = DEFAULT_DISTANCE_NEAR_2) then
            begin
              //                
              sBaseString:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:Type''][@No=''' + IntToStr(nsSBJTypeIndex) + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + IntToStr(j+1) + ''']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:RefractionData'']';                    
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:R'']/*[name()=''nsSBJ:Sph'']';
                    
              SetLength(arrData3, 0);
              arrData3:= DoPSGetXMLData(sBaseString);                 
              
              if Length(arrData3) > 0 then
              begin
                R_SPH_N:= arrData3[0][2];

                if (R_SPH_N <> '') then
                begin
                  try                 	
                   R_ADD:= CalculateAdditionValueEx(R_SPH_F, Trim(R_SPH_N));
                   R_ADD:= FormatSignValue('A=', R_ADD, bolAddSign, bolAddSignSeparator);
                  except
                   R_SPH_N:= FormatSignValue('A=', Trim(arrData3[0][2]), bolAddSign, bolAddSignSeparator);
                   R_ADD:= R_SPH_N;                   
                  end; 
                end;  
              end; 
                     
              //                 
              sBaseString:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:Type''][@No=''' + IntToStr(nsSBJTypeIndex) + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + IntToStr(j+1) + ''']';
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:RefractionData'']';                    
              sBaseString:= sBaseString + '/*[name()=''nsSBJ:L'']/*[name()=''nsSBJ:Sph'']';                                 
              
              SetLength(arrData3, 0);
              arrData3:= DoPSGetXMLData(sBaseString);                 
              
              if Length(arrData3) > 0 then
              begin
                L_SPH_N:= arrData3[0][2];
                    
                if (L_SPH_N <> '') then
                begin
                  try                 	
                   L_ADD:= CalculateAdditionValueEx(L_SPH_F, Trim(L_SPH_N));
                   L_ADD:= FormatSignValue('A=', L_ADD, bolAddSign, bolAddSignSeparator);
                  except
                   L_SPH_N:= FormatSignValue('A=', Trim(arrData3[0][2]), bolAddSign, bolAddSignSeparator);
                   L_ADD:= L_SPH_N;                   
                  end;
                end;                        
              end;                     
              
              // 
                                             
            end;
                       
            // 
            
          end;
        end;

        // subjektiven Endwerte (Prescription)  
        if (nsSBJTypeName = DEFAULT_NS_SJB_TYPE_NAME_3) then
        begin	               
          Break; 
        end;           

        // 

      end;
    end;

    // -------------------------------------------------------------------
    
    // Build result
    ParsedData:= '';

    // Add patient ID
    if (PatientID <> '') then
    begin
      ParsedData:= ParsedData + PatientID;
    end;

    // Add right eye
    R_Line:= '';
    
    if (R_SPH_F <> '') and (R_CYL_F <> '') and (R_AXIS_F <> '') then
    begin
      R_Line:= R_Line + 'F R.:' + R_SPH_F + ' ' + R_CYL_F + R_AXIS_F;
            
      if (R_Add <> '') and (bolAddADDValueToOutput) then
      begin
        R_Line:= R_Line + ' ' + R_Add;
      end;                  
    end;

    // Add PD
    if (R_PD_F <> '') and (bolAddPDValueToOutput) then
    begin
      if (R_Line <> '') then
      begin
        R_Line:= R_Line + ' ';
      end;
            
      R_Line:= R_Line + R_PD_F;
    end;

    // Add VD
    if (R_VD_F <> '') and (bolAddVDValueToOutput) then
    begin
      if (R_Line <> '') then
      begin
        R_Line:= R_Line + ' ';
      end;
            
      R_Line:= R_Line + R_VD_F;
    end;

    if (R_Line <> '') then
    begin
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
      ParsedData:= ParsedData + FormatValue(R_Line) + FOutputLineSeparator;
    end;

    // Add left eye
    L_Line:= '';
    
    if (L_SPH_F <> '') and (L_CYL_F <> '') and (L_AXIS_F <> '') then
    begin
      L_Line:= L_Line + 'F L.:' + L_SPH_F + ' ' + L_CYL_F + L_AXIS_F;
            
      if (L_Add <> '') and (bolAddADDValueToOutput) then
      begin
        L_Line:= L_Line + ' ' + L_Add;
      end;  
    end;
    
    // Add PD
    if (L_PD_F <> '') and (bolAddPDValueToOutput) then
    begin
      if (L_Line <> '') then
      begin
        L_Line:= L_Line + ' ';
      end;
            
      L_Line:= L_Line + L_PD_F;
    end;      

    if (L_Line <> '') then
    begin
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
      ParsedData:= ParsedData + FormatValue(L_Line) + FOutputLineSeparator;
    end;
    
    // Add VA
    F_VA_Line:= '';
    	
    if (F_VA_R <> '') or (F_VA_L <> '') or (F_VA_B <> '')  then
    begin
      F_VA_Line:= F_VA_Line + 'cc' + ' ';
      
      if (F_VA_R <> '') then
      begin
        F_VA_Line:= F_VA_Line + 'R = ' + F_VA_R + ' ';
      end;
      
      if (F_VA_L <> '') then
      begin
        F_VA_Line:= F_VA_Line + 'L = ' + F_VA_L + ' ';
      end;      
      
      if (F_VA_B <> '') then
      begin
        F_VA_Line:= F_VA_Line + 'B = ' + F_VA_B;
      end;
    end;

    if (F_VA_Line <> '') 
    and (bolAddVisusValueToOutput) then
    begin
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_RESULT + FOutputLineSeparator;
      ParsedData:= ParsedData + FormatValue(F_VA_Line) + FOutputLineSeparator;      
    end;    
    
    // Set output
    FParsedDataString:= RawByteString(ParsedData);
end.
