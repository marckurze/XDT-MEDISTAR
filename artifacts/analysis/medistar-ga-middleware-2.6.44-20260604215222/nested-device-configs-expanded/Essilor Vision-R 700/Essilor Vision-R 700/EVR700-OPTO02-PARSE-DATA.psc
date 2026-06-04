const
  VERSION = '1.0.55.82';
  DATE = '16.01.2026 12:31:01';
  TEXT = 'Copyright (c) 2026 CompuGroup Medical Deutschland AG';
  
  // Patient ID
  XPATH_EXPRESSION_01 = '//Ophthalmology/*[name()=''nsCommon:Common'']/*[name()=''nsCommon:Patient'']/*[name()=''nsCommon:ID'']';  

  // GDT
  GDT_FID_PATIENT_ID     = '3000';
  GDT_FID_MEASURE_DATA   = '6228';
  GDT_FID_COMMENT        = '6227';
  GDT_FID_RESULT         = '6220';
  GDT_FID_FOREIGN_RESULT = '6221';

  GDT_LINE_PREFIX = '  ';
  GDT_SIGN_SEPARATOR = ' ';
  GDT_AXIS_SEPARATOR = ' ';
  GDT_AXIS_SEPARATOR_MAX_COUNT = 3; 
  
  // Diese Werte müssen übereinstimmen mit den Werten
  // aus der Device INI Datei.
  NonGDTDataLineLensmeter = 'V0';
  NonGDTDataLineRefraktometerObjektiv = 'V1';
  NonGDTDataLinePhoropter = 'V2';
  NonGDTDataLineVerordnung = 'V3';
  NonGDTDataLineRefraktometerSubjektiv = 'V4';
  NonGDTDataLineVisusKorrektur = 'V5';
  NonGDTDataLineSondereintraege = 'V6';
  NonGDTDataLineKeratometer = 'V7';
  NonGDTDataLineVisus = 'V';

function FormatSignValue(const ID, Value: String; AddSign: Boolean; AddSignSeparator: Boolean): String;
var
  S1, S2: String;
begin
  Result:= ID;

  S1:= Trim(Value);
  
  if (AddSign) then
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
  begin
    S2:= S2 + GDT_SIGN_SEPARATOR;
  end;  
  
  Result:= Result + S2 + S1;
end;

function FormatAxisValue(const ID, Value: String; AddAxisSeparator: Boolean): String;
var
  S: String;
begin
  Result:= ID;
  
  S:= Value;
  
  if (AddAxisSeparator) then
  begin
    while (Length(S) < GDT_AXIS_SEPARATOR_MAX_COUNT) do
    begin
      S:= GDT_AXIS_SEPARATOR + S;
    end;  
  end;    
  
  Result:= Result + S;
end;

var
  arrData, arrData2, arrData3, arrData4: TStringArrayArray;
  arrDataNonGDT: TStringArray;
  ParsedData, S, PatientID: String; 
  nsSBJ_TypeName, nsSBJ_TypeNo, nsSBJ_ExamDistanceNo, nsSBJ_Distance: String;
  i, j: Integer;  
  sBaseString, sBaseStringSub: String;
  bolAddVDValueToOutput, bolAddWDValueToOutput: Boolean;
  bolAddSign, bolAddSignSeparator, bolAddAxisSeparator, bolLensmeterDataWasSelected, bolRefraktometerDataWasSelected: Boolean;
  bolAddPrismValuesToOutput, bolAddPrismBaseValueToOutput, bolAddVisusValueToOutput, bolAddSignSeparatorForVisus: Boolean;
  WD, PD, VD, B_VA, B_PD: String; 
  R_S, R_Z, R_Axis, R_Add, R_PD, R_PRISM, R_PRISMBASE, R_PRISMX, R_PRISMX_BASE, R_PRISMY, R_PRISMY_BASE, R_VA: String; 
  L_S, L_Z, L_Axis, L_Add, L_PD, L_PRISM, L_PRISMBASE, L_PRISMX, L_PRISMX_BASE, L_PRISMY, L_PRISMY_BASE, L_VA: String;  
  R_Line, L_Line: String;  
  default_nsSBJ_TypeName_String, default_nsSBJ_Distance_String: String;
  bolFound: Boolean;
  
begin

  // Verwende "True", damit der VD Wert an die V2 Zeile angehängt wird,
  // benutze "False", damit der VD Wert ignoriert werden kann.
  bolAddVDValueToOutput := True;

  // Verwende "True", damit der WD Wert hinzugefügt wird, benutze "False", damit der
  // WD Wert nicht per GDT exportiert wird.
  bolAddWDValueToOutput:= True;
              
  // Verwende "True", damit immer ein Vorzeichen hinzugefügt wird, 
  // benutze "False", damit nur der gemessene Wert eingetragen wird wie er 
  // vom Gerät kommt 
  bolAddSign:= True;

  // Verwende "True", damit nach jedem Vorzeichen der Wert aus GDT_SIGN_SEPARATOR 
  // angefügt wird, benutze "False", damit kein Abstand zwischen Vorzeichen und Wert eingefügt wird
  bolAddSignSeparator:= True;
  
  // Verwende "True", damit nach jedem Vorzeichen der Wert aus GDT_SIGN_SEPARATOR 
  // angefügt wird, benutze "False", damit kein Abstand zwischen Vorzeichen und Wert eingefügt wird 
  bolAddSignSeparatorForVisus:= False;
  
  // Verwende "True", damit der Achsenseparator angefügt wird,
  // benutze "False", damit der Achsenseparator nicht verwendet wird
  bolAddAxisSeparator:= True;
           
  // Verwende "True", damit der Prism Wert an die V2 Zeile angehängt wird,
  // benutze "False", damit der Prism Wert ignoriert werden kann.
  bolAddPrismValuesToOutput:= True;
  
  // Verwende "True", damit der Prism Base Wert an die V2 Zeile angehängt wird,
  // benutze "False", damit der Prism Base Wert ignoriert werden kann.
  bolAddPrismBaseValueToOutput:= True;

  // Verwende "True", damit der Visus Wert als die V Zeile exportiert wird,
  // benutze "False", damit der Visus Wert ignoriert werden kann.
  bolAddVisusValueToOutput:= True;

  // Setze hier, welche Werte aus welcher Messung genutzt werden sollen.
  // Wert von "<nsSBJ:TypeName>" kann sein:
  // Prescription
  // Full Correction
  // Objective Data
  // Current Spectacles
  // Subj Night High Precision
  // Subj Night Standard
  // Subj Day Standard
  // Subj Day High Precision
  
  // Werte von "<nsSBJ:Distance>" können sein:
  // 500.000
  // 40.000
  
  // 10000
  // 40

  default_nsSBJ_TypeName_String:= 'Full Correction';
  default_nsSBJ_Distance_String:= '10000';

  // --- Don't edit script down below ---

  // Clear parsed data string
  FParsedDataString:= '';

  if (not DoPSXMLDocumentExists) then
  begin
    FLastErrorCode:= -4;
    FLastErrorMessage:= 'Keine XML-Daten für die Verarbeitung verfügbar';

    DoPSError();

    Exit;
  end;

  if (not DoPSXMLRootNodeExists) then
  begin
    FLastErrorCode:= -5;
    FLastErrorMessage:= 'Kein XML-Wurzelknoten für die Verarbeitung verfügbar';

    DoPSError();

    Exit;
  end;
  
  // check which data was selected for transferring to the device
  bolLensmeterDataWasSelected:= False;
  bolRefraktometerDataWasSelected:= False;
  
  SetLength(arrDataNonGDT, 0);  
  DoPSGetEnabledNonGDTDataGroupList(arrDataNonGDT);
  
  if (Length(arrDataNonGDT) > 0) then
  begin
    for i:= 0 to Length(arrDataNonGDT) - 1 do
    begin 
      if (T2WContainsStr(arrDataNonGDT[i], 'Lensmeter')) then
      begin
        bolLensmeterDataWasSelected:= True;
      end;
    
      if (T2WContainsStr(arrDataNonGDT[i], 'Refraktometer')) then
      begin
        bolRefraktometerDataWasSelected:= True;
      end;     
    end;
  end;

  // Patient ID
  PatientID:= '';
  
  SetLength(arrData2, 0);
  arrData2:= DoPSGetXMLData(XPATH_EXPRESSION_01);
  
  if (Length(arrData2) > 0) then
  begin
    S:= arrData2[0][2];
  
    PatientID:= PatientID + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
    PatientID:= PatientID + S + FOutputLineSeparator;
  end;
  
  // Build result
  ParsedData:= '';

  // Add patient id
  if (PatientID <> '') then
  begin
    ParsedData:= ParsedData + PatientID;
  end;
  
  // Clear variables
  R_Line:= '';
  L_Line:= '';
  
  R_S:= '';
  R_Z:= '';
  R_Axis:= '';
  R_Add:= '';
  R_PD:= '';
  R_PRISM:= '';
  R_PRISMBASE:= '';
  R_PRISMX:= '';
  R_PRISMX_BASE:= '';
  R_PRISMY:= '';
  R_PRISMY_BASE:= '';
  R_VA:= '';
  
  L_S:= '';
  L_Z:= '';
  L_Axis:= '';
  L_Add:= '';
  L_PD:= '';
  L_PRISM:= '';
  L_PRISMBASE:= '';
  L_PRISMX:= '';
  L_PRISMX_BASE:= '';
  L_PRISMY:= '';
  L_PRISMY_BASE:= '';
  L_VA:= '';
  
  WD:= '';
  PD:= '';
  VD:= ''; 
  B_VA:= '';
  B_PD:= '';

  // Reset
  SetLength(arrData, 0);
  SetLength(arrData2, 0);
  SetLength(arrData3, 0);
  SetLength(arrData4, 0);
  
  nsSBJ_TypeName:= '';
  nsSBJ_TypeNo:= '';
  nsSBJ_ExamDistanceNo:= '';
  nsSBJ_Distance:= '';
  
  bolFound:= False;
  
  // Set base path here to get values of all properties
  sBaseString:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']/*[name()=''nsSBJ:Type'']';

  SetLength(arrData, 0);
  arrData:= DoPSGetXMLData(sBaseString);
  
  // We check which meassurement modes are available
  if (Length(arrData) > 0) then
  begin
    for i := 0 to Length(arrData) - 1 do
    begin
      // get number of different measure types
      nsSBJ_TypeNo:= arrData[i][4];
      
      if (nsSBJ_TypeNo = '') then
      begin
        Continue;
      end;

      // create new search base string for number of measure types
      sBaseString:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']/*[name()=''nsSBJ:Type''][@No=''' + nsSBJ_TypeNo + ''']/*[name()=''nsSBJ:TypeName'']';
      
      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(sBaseString);
      
      if (Length(arrData2) > 0) then
      begin
        nsSBJ_TypeName:= arrData2[0][2];
      end;

      if (nsSBJ_TypeName = '') then
      begin
        Continue;
      end;
      
      // <nsSBJ:TypeName> could be:
      
      // Prescription
      // Full Correction
      // Objective Data
      // Current Spectacles
      // Subj Night High Precision
      // Subj Night Standard
      // Subj Day Standard
      // Subj Day High Precision

      if (default_nsSBJ_TypeName_String <> '') then
      begin
        if (default_nsSBJ_TypeName_String = nsSBJ_TypeName) then
        begin
          bolFound:= True;
        end;      
      end;
    end;
  end;
  
  // Check type, if configured type was not found, reset 
  // and use first meassurement which was found
  if (not bolFound) then
  begin
    default_nsSBJ_TypeName_String:= '';
    default_nsSBJ_Distance_String:= '';
  end;

  // Reset
  nsSBJ_TypeName:= '';
  nsSBJ_TypeNo:= '';
  nsSBJ_ExamDistanceNo:= '';
  nsSBJ_Distance:= '';

  // We loop through the response array
  if (Length(arrData) > 0) then
  begin
    for i := 0 to Length(arrData) - 1 do
    begin
      // get number of different measure types
      nsSBJ_TypeNo:= arrData[i][4];
      
      if (nsSBJ_TypeNo = '') then
      begin
        Continue;
      end;

      // create new search base string for number of measure types
      sBaseString:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']/*[name()=''nsSBJ:Type''][@No=''' + nsSBJ_TypeNo + ''']/*[name()=''nsSBJ:TypeName'']';
      
      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(sBaseString);
      
      if (Length(arrData2) > 0) then
      begin
        nsSBJ_TypeName:= arrData2[0][2];
      end;

      if (nsSBJ_TypeName = '') then
      begin
        Continue;
      end;
      
      // <nsSBJ:TypeName> could be:
      // Prescription
      // Full Correction
      // Objective Data
      // Current Spectacles
      // Subj Night High Precision
      // Subj Night Standard
      // Subj Day Standard
      // Subj Day High Precision
 
      // T2WMessageBoxS(nsSBJ_TypeName);
      
      if (default_nsSBJ_TypeName_String <> '') then
      begin
        if (default_nsSBJ_TypeName_String = nsSBJ_TypeName) then
        begin
          // do nothing
        end
        else
        begin
          Continue;
        end;        
      end;

      // check measure type and get data
      // create dynamically all xpath variables
      sBaseStringSub:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']/*[name()=''nsSBJ:Type''][@No=''' + nsSBJ_TypeNo + ''']/*[name()=''nsSBJ:ExamDistance'']';

      SetLength(arrData3, 0);
      arrData3:= DoPSGetXMLData(sBaseStringSub);
      
      if (Length(arrData3) > 0) then
      begin
        for j := 0 to Length(arrData3) - 1 do
        begin
          nsSBJ_ExamDistanceNo:= arrData3[j][4];
          
          if (nsSBJ_ExamDistanceNo = '') then
          begin
            Continue;
          end;
          
          // T2WMessageBoxS(nsSBJ_ExamDistanceNo);
          
          nsSBJ_Distance:= '';
          
          sBaseStringSub:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']/*[name()=''nsSBJ:Type''][@No=''' + nsSBJ_TypeNo + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + nsSBJ_ExamDistanceNo + ''']';
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:Distance'']';

          SetLength(arrData4, 0);
          arrData4:= DoPSGetXMLData(sBaseStringSub);

          if (Length(arrData4) > 0) then
          begin
            nsSBJ_Distance:= arrData4[0][2];
          end;
          
          // T2WMessageBoxS(nsSBJ_Distance);
          
          if (default_nsSBJ_Distance_String <> '') then
          begin
            if (default_nsSBJ_Distance_String = nsSBJ_Distance) then
            begin
              // do nothing
            end
            else
            begin
              Continue;
            end;  
          end;

          //
          // Right
          //

          // R_S
          R_S:= '';
          
          sBaseStringSub:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']/*[name()=''nsSBJ:Type''][@No=''' + nsSBJ_TypeNo + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + nsSBJ_ExamDistanceNo + ''']';
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:RefractionData'']/*[name()=''nsSBJ:R'']';
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:Sph'']';

          SetLength(arrData4, 0);
          arrData4:= DoPSGetXMLData(sBaseStringSub);

          if (Length(arrData4) > 0) then
          begin
            R_S:= FormatSignValue('S=', arrData4[0][2], bolAddSign, bolAddSignSeparator);
          end;

          // R_Z
          R_Z:= '';
          
          sBaseStringSub:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']/*[name()=''nsSBJ:Type''][@No=''' + nsSBJ_TypeNo + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + nsSBJ_ExamDistanceNo + ''']';
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:RefractionData'']/*[name()=''nsSBJ:R'']';
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:Cyl'']';

          SetLength(arrData4, 0);
          arrData4:= DoPSGetXMLData(sBaseStringSub);

          if (Length(arrData4) > 0) then
          begin
            R_Z:= FormatSignValue('Z=', Trim(arrData4[0][2]), bolAddSign, bolAddSignSeparator);
          end;  
      
          // R_Axis
          R_Axis:= '';
          
          sBaseStringSub:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']/*[name()=''nsSBJ:Type''][@No=''' + nsSBJ_TypeNo + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + nsSBJ_ExamDistanceNo + ''']';
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:RefractionData'']/*[name()=''nsSBJ:R'']';
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:Axis'']';

          SetLength(arrData4, 0);
          arrData4:= DoPSGetXMLData(sBaseStringSub);

          if (Length(arrData4) > 0) then
          begin
            R_Axis:= FormatAxisValue('*', arrData4[0][2], bolAddAxisSeparator);
          end; 
                    
          // TODO: PRISM

          //
          // Left
          // 
      
          // L_S
          L_S:= '';
          
          sBaseStringSub:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']/*[name()=''nsSBJ:Type''][@No=''' + nsSBJ_TypeNo + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + nsSBJ_ExamDistanceNo + ''']';
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:RefractionData'']/*[name()=''nsSBJ:L'']';
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:Sph'']';

          SetLength(arrData4, 0);
          arrData4:= DoPSGetXMLData(sBaseStringSub);

          if (Length(arrData4) > 0) then
          begin
            L_S:= FormatSignValue('S=', arrData4[0][2], bolAddSign, bolAddSignSeparator);
          end;

          // L_Z
          L_Z:= '';
          
          sBaseStringSub:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']/*[name()=''nsSBJ:Type''][@No=''' + nsSBJ_TypeNo + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + nsSBJ_ExamDistanceNo + ''']';
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:RefractionData'']/*[name()=''nsSBJ:L'']';
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:Cyl'']';

          SetLength(arrData4, 0);
          arrData4:= DoPSGetXMLData(sBaseStringSub);

          if Length(arrData4) > 0 then
          begin
            L_Z:= FormatSignValue('Z=', Trim(arrData4[0][2]), bolAddSign, bolAddSignSeparator);
          end;  
      
          // L_Axis
          L_Axis:= '';
          
          sBaseStringSub:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']/*[name()=''nsSBJ:Type''][@No=''' + nsSBJ_TypeNo + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + nsSBJ_ExamDistanceNo + ''']';
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:RefractionData'']/*[name()=''nsSBJ:L'']';
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:Axis'']';

          SetLength(arrData4, 0);
          arrData4:= DoPSGetXMLData(sBaseStringSub);

          if (Length(arrData4) > 0) then
          begin
            L_Axis:= FormatAxisValue('*', arrData4[0][2], bolAddAxisSeparator);
          end; 
      
          // TODO: PRISM

          //
          // VD
          // 
          sBaseStringSub:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']/*[name()=''nsSBJ:Type''][@No=''' + nsSBJ_TypeNo + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + nsSBJ_ExamDistanceNo + ''']';
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:RefractionData'']/*[name()=''nsSBJ:VD'']';
      
          VD:= '';

          SetLength(arrData4, 0);
          arrData4:= DoPSGetXMLData(sBaseStringSub);

          if (Length(arrData4) > 0) then
          begin
            VD:= VD + 'VD= ' + arrData4[0][2];
          end;

          //
          // VA
          // 
          
          R_VA:= '';
          
          sBaseStringSub:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']/*[name()=''nsSBJ:Type''][@No=''' + nsSBJ_TypeNo + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + nsSBJ_ExamDistanceNo + ''']'; 
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:VA'']'; 
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:R'']';  
           
          SetLength(arrData4, 0);
          arrData2:= DoPSGetXMLData(sBaseStringSub);

          if (Length(arrData4) > 0) then
          begin
            R_VA:= FormatSignValue(' subj=', arrData4[0][2], False, bolAddSignSeparatorForVisus);
          end;

          L_VA:= '';
          
          sBaseStringSub:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']/*[name()=''nsSBJ:Type''][@No=''' + nsSBJ_TypeNo + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + nsSBJ_ExamDistanceNo + ''']'; 
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:VA'']'; 
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:L'']';  
           
          SetLength(arrData4, 0);
          arrData2:= DoPSGetXMLData(sBaseStringSub);

          if (Length(arrData4) > 0) then
          begin
            L_VA:= FormatSignValue(' subj=', arrData4[0][2], False, bolAddSignSeparatorForVisus);
          end;
          
          B_VA:= '';
           
          sBaseStringSub:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']/*[name()=''nsSBJ:Type''][@No=''' + nsSBJ_TypeNo + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + nsSBJ_ExamDistanceNo + ''']'; 
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:VA'']'; 
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:B'']';  
           
          SetLength(arrData4, 0);
          arrData2:= DoPSGetXMLData(sBaseStringSub);

          if (Length(arrData4) > 0) then
          begin
            B_VA:= FormatSignValue(' subj=', arrData4[0][2], False, bolAddSignSeparatorForVisus);
          end;
           
          //
          // PD
          // 
          
          R_PD:= '';
          
          sBaseStringSub:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']/*[name()=''nsSBJ:Type''][@No=''' + nsSBJ_TypeNo + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + nsSBJ_ExamDistanceNo + ''']'; 
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:PD'']';  
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:R'']'; 
           
          SetLength(arrData4, 0);
          arrData4:= DoPSGetXMLData(sBaseStringSub);

          if (Length(arrData4) > 0) then
          begin
            R_PD:= FormatSignValue('PD=', arrData4[0][2], False, bolAddSignSeparator);
          end; 
          
          L_PD:= '';
          
          sBaseStringSub:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']/*[name()=''nsSBJ:Type''][@No=''' + nsSBJ_TypeNo + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + nsSBJ_ExamDistanceNo + ''']'; 
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:PD'']';  
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:L'']'; 
           
          SetLength(arrData4, 0);
          arrData4:= DoPSGetXMLData(sBaseStringSub);

          if (Length(arrData4) > 0) then
          begin
            L_PD:= FormatSignValue('PD=', arrData4[0][2], False, bolAddSignSeparator);
          end;
          
          B_PD:= '';
          
          sBaseStringSub:= '//Ophthalmology/*[name()=''nsSBJ:Measure''][@type=''SBJ'']/*[name()=''nsSBJ:RefractionTest'']/*[name()=''nsSBJ:Type''][@No=''' + nsSBJ_TypeNo + ''']/*[name()=''nsSBJ:ExamDistance''][@No=''' + nsSBJ_ExamDistanceNo + ''']'; 
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:PD'']';  
          sBaseStringSub:= sBaseStringSub + '/*[name()=''nsSBJ:B'']'; 
           
          SetLength(arrData4, 0);
          arrData4:= DoPSGetXMLData(sBaseStringSub);

          if (Length(arrData4) > 0) then
          begin
            B_PD:= FormatSignValue('PD=', arrData4[0][2], False, bolAddSignSeparator);
          end; 

          // TODO:
          Break;
        end;
      end;

      // TODO:
      Break;
    end;        
  end;
  
  // Create the GDT output 
  
  //
  // Add right eye
  //
  R_Line:= '';

  if (R_S <> '') and (R_Z <> '') and (R_Axis <> '') then
  begin
    R_Line:= R_Line + 'R.:' + R_S + ' ' + R_Z + R_Axis;
  end;

  // Add PRISM and BASE if set
  if (R_PRISM <> '') and (R_PRISMBASE <> '') and (bolAddPrismValuesToOutput) then
  begin
    if (R_Line <> '') then
    begin
      R_Line:= R_Line + ' ';
    end;
    
    if (bolAddPrismBaseValueToOutput) then
      R_Line:= R_Line + R_PRISM + ' ' + R_PRISMBASE       
    else 
      R_Line:= R_Line + R_PRISM;
  end;

  // Add PRISM_X and PRISM_Y
  if (R_PRISMX <> '') and (R_PRISMY <> '') and (bolAddPrismValuesToOutput) then
  begin
    if (R_Line <> '') then
    begin
      R_Line:= R_Line + ' ';
    end;  

    R_Line:= R_Line + R_PRISMX + ' ' + R_PRISMY;
  end;
  
  // Add ADD
  if (R_Add <> '')  then
  begin
    if (R_Line <> '') then
    begin
      R_Line:= R_Line + ' ';
    end;

    R_Line:= R_Line + R_Add;
  end;
  
  // Add PD
  if (R_PD <> '') then
  begin
    if (R_Line <> '') then
    begin
      R_Line:= R_Line + ' ';
    end;
    
    R_Line:= R_Line + R_PD;
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
    
  //
  // Add left eye
  //
  L_Line:= '';

  if (L_S <> '') and (L_Z <> '') and (L_Axis <> '') then
  begin
    L_Line:= L_Line + 'L.:' + L_S + ' ' + L_Z + L_Axis;
  end;

  // Add PRISM and BASE if set
  if (L_PRISM <> '') and (L_PRISMBASE <> '') and (bolAddPrismValuesToOutput) then
  begin
    if (L_Line <> '') then
    begin
      L_Line:= L_Line + ' ';
    end;
    
    if (bolAddPrismBaseValueToOutput) then
      L_Line:= L_Line + L_PRISM + ' ' + L_PRISMBASE
    else     
      L_Line:= L_Line + L_PRISM;
  end;

  // Add PRISM_X and PRISM_Y
  if (L_PRISMX <> '') and (L_PRISMY <> '') and (bolAddPrismValuesToOutput) then
  begin
    if (L_Line <> '') then
    begin
      L_Line:= L_Line + ' ';
    end;  

    L_Line:= L_Line + L_PRISMX + ' ' + L_PRISMY;
  end;
  
  // Add ADD
  if (L_Add <> '')  then
  begin
    if (L_Line <> '') then
    begin
      L_Line:= L_Line + ' ';
    end;

    L_Line:= L_Line + L_Add;
  end; 

  // Add PD
  if (L_PD <> '') then
  begin
    if (L_Line <> '') then
    begin
      L_Line:= L_Line + ' ';
    end;
    
    L_Line:= L_Line + L_PD;
  end;    

  if (L_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
  end;

  // Add WD if it exists
  if (WD <> '')  and (bolAddWDValueToOutput) then
  begin
    // Append WD index with GDT comment field id to measure data
    ParsedData := ParsedData + FOutputLineSeparator + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_COMMENT + FOutputLineSeparator + 'WD= ' + Trim(WD);
  end;
  
  // Set output
  FParsedDataString:= RawByteString(ParsedData);
end.
