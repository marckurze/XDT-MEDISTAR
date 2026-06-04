const
  VERSION = '1.0.55.43';
  DATE = '11.11.2025 10:57:11';
  TEXT = 'Copyright (c) 2025 CompuGroup Medical Deutschland AG';
  
  // set default near / far distance values
  DEFAULT_DISTANCE_FAR     = '500.000';
  DEFAULT_DISTANCE_NEAR    = '33.000';
  DEFAULT_NS_SJB_TYPE_NAME = 'Full Correction';
  
  // PatientID
  XPATH_EXPRESSION_01 = '//Ophthalmology/*[name()=''nsCommon:Common'']/*[name()=''nsCommon:Patient'']/*[name()=''nsCommon:ID'']';
    
  // VD
  XPATH_EXPRESSION_02 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:VD'']';
    
  // R
  XPATH_EXPRESSION_03 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:REF'']/*[name()=''nsREF:R'']/*[name()=''nsREF:Median'']/*[name()=''nsREF:Sphere'']';
  XPATH_EXPRESSION_04 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:REF'']/*[name()=''nsREF:R'']/*[name()=''nsREF:Median'']/*[name()=''nsREF:Cylinder'']';
  XPATH_EXPRESSION_05 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:REF'']/*[name()=''nsREF:R'']/*[name()=''nsREF:Median'']/*[name()=''nsREF:Axis'']';

  XPATH_EXPRESSION_12 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:REF'']/*[name()=''nsREF:R'']/*[name()=''nsREF:List'']/*[name()=''nsREF:Sphere'']';
  XPATH_EXPRESSION_13 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:REF'']/*[name()=''nsREF:R'']/*[name()=''nsREF:List'']/*[name()=''nsREF:Cylinder'']';
  XPATH_EXPRESSION_14 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:REF'']/*[name()=''nsREF:R'']/*[name()=''nsREF:List'']/*[name()=''nsREF:Axis'']';
    
  // L
  XPATH_EXPRESSION_07 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:REF'']/*[name()=''nsREF:L'']/*[name()=''nsREF:Median'']/*[name()=''nsREF:Sphere'']';
  XPATH_EXPRESSION_08 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:REF'']/*[name()=''nsREF:L'']/*[name()=''nsREF:Median'']/*[name()=''nsREF:Cylinder'']';
  XPATH_EXPRESSION_09 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:REF'']/*[name()=''nsREF:L'']/*[name()=''nsREF:Median'']/*[name()=''nsREF:Axis'']';
  
  XPATH_EXPRESSION_15 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:REF'']/*[name()=''nsREF:L'']/*[name()=''nsREF:List'']/*[name()=''nsREF:Sphere'']';
  XPATH_EXPRESSION_16 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:REF'']/*[name()=''nsREF:L'']/*[name()=''nsREF:List'']/*[name()=''nsREF:Cylinder'']';
  XPATH_EXPRESSION_17 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:REF'']/*[name()=''nsREF:L'']/*[name()=''nsREF:List'']/*[name()=''nsREF:Axis'']';

  // PD
  XPATH_EXPRESSION_11 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:PD'']/*[name()=''nsREF:Distance'']';
  XPATH_EXPRESSION_21 = '//Ophthalmology/*[name()=''nsREF:Measure''][@type=''REF'']/*[name()=''nsREF:PD'']/*[name()=''nsREF:Near'']';
  
  // Gibt die Listeneinträge an, die verwendet werden sollen,
  // falls keine Mittelwerteinträge in der Messung enthalten sind
  USE_R_LIST_ENTRY = 0;
  USE_L_LIST_ENTRY = 0;
  
  GDT_FID_PATIENT_ID   = '3000';
  GDT_FID_MEASURE_DATA = '6228';
  GDT_FID_COMMENT      = '6227';
  GDT_FID_SIGNATURE    = '8990';
  GDT_FID_RESULT       = '6220';
  GDT_FID_EXT_RESULT   = '6221';
  
  GDT_FID_FILE_ARCHIVE_NUMBER  = '6302';
  GDT_FID_FILE_FORMAT          = '6303';
  GDT_FID_FILE_DESCRIPTION     = '6304';
  GDT_FID_FILE_URL             = '6305';
  
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

function CalculateAdditionValue(const SPH_F, SPH_N: String): String;
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
    Res:= a1 - a2;
    
    if (Res < 0) then
      Res:= Res * -1;

    strResult:= Trim(T2WFloatToStrF(Res, 10, 2));
    strResult:= Trim(T2WStringReplace(strResult, ',', '.', True, True));  
  end;     
   
  Result:= strResult;
end;              

var
  arrData, arrData2, arrData3: TStringArrayArray;
  ParsedData, S, PatientID, sBaseString, nsSBJTypeName, nsSBJDistance: String;
  VD, R_S, R_Z, R_Axis, L_S, L_Z, L_Axis, PD, R_VisusFar, L_VisusFar: String;
  R_Line, L_Line, VisusLineFar: String;
  R_nsSBJSph_N, L_nsSBJSph_N, R_Add, L_Add, R_SPH_F, L_SPH_F: String;
  i, j, nsSBJTypeIndex: Integer;
  bolAddExternalFilesToGdtFile, bolAddVDValueToOutput, bolAddPDValueToOutput, bolAddADDValueToOutput: Boolean;
  bolAddSign, bolAddSignSeparator, bolAddAxisSeparator, bolAddVisusValueToOutput: Boolean;
  bolUseSpecialVisusFormatForOutput: Boolean;
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

  // Aktiviere den Import aller externen Bildquellen.
  // Verwende "True", damit der Import aktiviert ist, benutze "False", damit Bilder
  // nicht importiert und per GDT weiterverarbeitet werden.
  bolAddExternalFilesToGdtFile:= False;

  // Verwende "True", damit der VD Wert an die V1 Zeile angehängt wird,
  // benutze "False", damit der VD Wert ignoriert werden kann.
  bolAddVDValueToOutput:= True;

  // Verwende "True", damit der PD Wert an die V1 Zeile angehängt wird,
  // benutze "False", damit der PD Wert ignoriert werden kann.
  bolAddPDValueToOutput:= True;
  
  // Verwende "True", damit der Visus Wert als die V Zeile exportiert wird,
  // benutze "False", damit der Visus Wert ignoriert werden kann.
  bolAddVisusValueToOutput:= True;
    
  // Verwende "True", damit der ADD Wert exportiert wird,
  // benutze "False", damit der ADD Wert nicht verwendet werden kann.
  bolAddADDValueToOutput:= True;
  
  // Verwende "True", damit die Visus Formatierung so: RAcc: 1,0 LAcc: 1,0 erstellt wird,
  // benutze "False", damit so: R: VA= 0.6 // L: VA= 0.6 erstellt wird.
  bolUseSpecialVisusFormatForOutput:= False; 

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

  // VD
  VD:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_02);
  
  if Length(arrData) > 0 then
  begin
    S:= T2WStringReplace(arrData[0][2], ' mm', '', False, False);
    
    VD:= VD + 'VD= ' + S;
  end;
  
  // -------------------------------------------------------------------

  // R_S
  R_S:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_03);
  
  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_12);

    if Length(arrData) > USE_R_LIST_ENTRY then
    begin
      R_S:= R_S + FormatSignValue('S=', Trim(arrData[USE_R_LIST_ENTRY][2]), bolAddSign, bolAddSignSeparator);
    end;
  end
  else
  begin
    R_S:= R_S + FormatSignValue('S=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
  end;  

  // R_Z
  R_Z:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_04);
  
  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_13);

    if Length(arrData) > USE_R_LIST_ENTRY then
    begin
      R_Z:= R_Z + FormatSignValue('Z=', Trim(arrData[USE_R_LIST_ENTRY][2]), bolAddSign, bolAddSignSeparator);
    end;
  end
  else
  begin
    R_Z:= R_Z + FormatSignValue('Z=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
  end;
  
  // R_Axis
  R_Axis:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_05);
  
  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_14);

    if Length(arrData) > USE_R_LIST_ENTRY then
    begin
      R_Axis:= R_Axis + FormatAxisValue('*', Trim(arrData[USE_R_LIST_ENTRY][2]), bolAddAxisSeparator);
    end;
  end
  else
  begin
    R_Axis:= R_Axis + FormatAxisValue('*', Trim(arrData[0][2]), bolAddAxisSeparator);
  end;
    
  // -------------------------------------------------------------------

  // L_S
  L_S:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_07);
  
  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_15);

    if Length(arrData) > USE_L_LIST_ENTRY then
    begin
      L_S:= L_S + FormatSignValue('S=', Trim(arrData[USE_L_LIST_ENTRY][2]), bolAddSign, bolAddSignSeparator);
    end;
  end
  else
  begin
    L_S:= L_S + FormatSignValue('S=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);      
  end;
  
  // L_Z
  L_Z:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_08);
  
  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_16);

    if Length(arrData) > USE_L_LIST_ENTRY then
    begin
      L_Z:= L_Z + FormatSignValue('Z=', Trim(arrData[USE_L_LIST_ENTRY][2]), bolAddSign, bolAddSignSeparator);;
    end;
  end
  else
  begin
    L_Z:= L_Z + FormatSignValue('Z=', Trim(arrData[0][2]), bolAddSign, bolAddSignSeparator);
  end;
  
  // L_Axis
  L_Axis:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_09);
  
  if Length(arrData) <= 0 then
  begin
    arrData:= DoPSGetXMLData(XPATH_EXPRESSION_17);

    if Length(arrData) > USE_L_LIST_ENTRY then
    begin
      L_Axis:= L_Axis + FormatAxisValue('*', Trim(arrData[USE_L_LIST_ENTRY][2]), bolAddAxisSeparator);
    end;
  end
  else
  begin
    L_Axis:= L_Axis + FormatAxisValue('*', Trim(arrData[0][2]), bolAddAxisSeparator);
  end;
  
  // -------------------------------------------------------------------
        
  // PD
  PD:= '';
  
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(XPATH_EXPRESSION_11);
  
  if Length(arrData) > 0 then
  begin
    PD:= PD + 'PD= ' + arrData[0][2];
  end;

  // -------------------------------------------------------------------

  nsSBJTypeIndex:= -1;
    
  R_VisusFar:= '';
  L_VisusFar:= '';
            
  R_Add:= '';
  L_Add:= '';  
    
  R_SPH_F:= '';
  L_SPH_F:= '';         
      
  // set base path here to get values of all properties
  sBaseString:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']/*[name()=''nsSBJ:Type'']/*[name()=''nsSBJ:TypeName'']';
        
  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(sBaseString);
    
  if Length(arrData) > 0 then
  begin   
    // so, we loop through the response array
    for i:= 0 to Length(arrData) - 1 do
    begin
      nsSBJTypeName:= arrData[i][2];
      
      if (nsSBJTypeName <> DEFAULT_NS_SJB_TYPE_NAME) then
      begin
        Continue;
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
                                    
          if (nsSBJDistance = DEFAULT_DISTANCE_FAR) then
          begin
            // R_VA
            sBaseString:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']';
            sBaseString:= sBaseString + '/*[name()=''nsSBJ:Type''][@No=''' + IntToStr(nsSBJTypeIndex) + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + IntToStr(j+1) + ''']';
            sBaseString:= sBaseString + '/*[name()=''nsSBJ:VA'']';                  
            sBaseString:= sBaseString + '/*[name()=''nsSBJ:R'']';
                    
            SetLength(arrData3, 0);
            arrData3:= DoPSGetXMLData(sBaseString);
  
            if Length(arrData3) > 0 then
            begin
              R_VisusFar:= R_VisusFar + '' + arrData3[0][2];
            end;  
            
            // L_VA
            sBaseString:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']';
            sBaseString:= sBaseString + '/*[name()=''nsSBJ:Type''][@No=''' + IntToStr(nsSBJTypeIndex) + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + IntToStr(j+1) + ''']';
            sBaseString:= sBaseString + '/*[name()=''nsSBJ:VA'']';                  
            sBaseString:= sBaseString + '/*[name()=''nsSBJ:L'']';
                    
            SetLength(arrData3, 0);
            arrData3:= DoPSGetXMLData(sBaseString);
  
            if Length(arrData3) > 0 then
            begin
              L_VisusFar:= L_VisusFar + '' + arrData3[0][2];
            end;

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
            end;  
          end;
                
          if (nsSBJDistance = DEFAULT_DISTANCE_NEAR) then
          begin              
            sBaseString:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']';
            sBaseString:= sBaseString + '/*[name()=''nsSBJ:Type''][@No=''' + IntToStr(nsSBJTypeIndex) + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + IntToStr(j+1) + ''']';
            sBaseString:= sBaseString + '/*[name()=''nsSBJ:RefractionData'']';                  
            sBaseString:= sBaseString + '/*[name()=''nsSBJ:R'']/*[name()=''nsSBJ:Sph'']';                  
                  
            SetLength(arrData3, 0);
            arrData3:= DoPSGetXMLData(sBaseString);                 
                  
            if Length(arrData3) > 0 then
            begin
              R_nsSBJSph_N:= arrData3[0][2];
              
              R_Add:= '';
                
              if (R_SPH_F <> '') and (R_nsSBJSph_N <> '') then
              begin
                R_Add:= CalculateAdditionValue(R_SPH_F, R_nsSBJSph_N);
                
                if (R_Add <> '') then
                begin     
                  R_Add:= FormatSignValue('A=', Trim(R_Add), bolAddSign, bolAddSignSeparator);                                 
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
              L_nsSBJSph_N:= arrData3[0][2];
            
              L_Add:= '';
                
              if (L_SPH_F <> '') and (L_nsSBJSph_N <> '') then
              begin
                L_Add:= CalculateAdditionValue(L_SPH_F, L_nsSBJSph_N);
                
                if (L_Add <> '') then
                begin     
                  L_Add:= FormatSignValue('A=', Trim(L_Add), bolAddSign, bolAddSignSeparator);     
                end;                            
              end;        
            end;
          end;                        
        end;
      end;
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
  
  if (R_S <> '') and (R_Z <> '') and (R_Axis <> '') then
  begin
    R_Line:= R_Line + 'R.:' + R_S + ' ' + R_Z + R_Axis;
      
    if (R_Add <> '') 
    and (bolAddADDValueToOutput) then
    begin
      R_Line:= R_Line + ' ' + R_Add;
    end;        
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
      
    if (L_Add <> '') 
    and (bolAddADDValueToOutput) then
    begin
      L_Line:= L_Line + ' ' + L_Add;
    end;  
  end;

  if (L_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
  end;

  // Add visus line
  VisusLineFar:= '';
  
  if (bolAddVisusValueToOutput) then
  begin
    if (bolUseSpecialVisusFormatForOutput) then
    begin
    VisusLineFar:= VisusLineFar + 'RAcc: ' + R_VisusFar + ' ' + 'LAcc: ' + L_VisusFar;
    end
    else
    begin
      VisusLineFar:= VisusLineFar + 'F' + ' ' + 'R: VA= ' + R_VisusFar + ' // L: VA= ' + L_VisusFar;
    end;    

    if (VisusLineFar <> '') then
    begin
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_RESULT + FOutputLineSeparator;
      ParsedData:= ParsedData + VisusLineFar + FOutputLineSeparator;
    end;    
  end;

  // Set output
  FParsedDataString:= RawByteString(ParsedData);
end.
