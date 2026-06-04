const
	VERSION = '1.0.23.80';
	DATE = '24.04.2026 13:15:12';
	TEXT = 'Copyright (c) 2026 CompuGroup Medical Deutschland AG';
	
  // PatientID
	XPATH_EXPRESSION_01 = '//Ophthalmology/Common/Patient/ID';	

  // Types
	PHOROPTER_CORRECTION_TYPE = '<PHOROPTER_CORRECTION_TYPE/>';
	PHOROPTER_VISION          = '<PHOROPTER_VISION/>';
	PHOROPTER_SITUATION       = '<PHOROPTER_SITUATION/>';

  // VD & WD
	XPATH_EXPRESSION_02 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/VD';
	XPATH_EXPRESSION_03 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/WD';

	// R	
	XPATH_EXPRESSION_04 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/R/Sphere';	
	XPATH_EXPRESSION_05 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/R/Cylinder';	
	XPATH_EXPRESSION_06 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/R/Axis';
	XPATH_EXPRESSION_07 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/R/ADD';
	XPATH_EXPRESSION_08 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/R/PD';
		
	XPATH_EXPRESSION_09 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/R/Prism';			
	XPATH_EXPRESSION_10 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/R/PrismBase';			
		
  XPATH_EXPRESSION_11 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/R/PrismX';			
  XPATH_EXPRESSION_12 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/R/PrismY';					
			
  XPATH_EXPRESSION_13 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/R/VA';			
			
	// L	
	XPATH_EXPRESSION_20 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/L/Sphere';	
	XPATH_EXPRESSION_21 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/L/Cylinder';	
	XPATH_EXPRESSION_22 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/L/Axis';	
	XPATH_EXPRESSION_23 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/L/ADD';			
	XPATH_EXPRESSION_24 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/L/PD';		
	
	XPATH_EXPRESSION_25 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/L/Prism';			
	XPATH_EXPRESSION_26 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/L/PrismBase';			
	        
  XPATH_EXPRESSION_27 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/L/PrismX';			        
  XPATH_EXPRESSION_28 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/L/PrismY';			    
       
  XPATH_EXPRESSION_29 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/L/VA';
            
  // PD
  XPATH_EXPRESSION_31 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/B/PD';			

  // VA
  XPATH_EXPRESSION_32 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/B/VA';

  // VISUS SC VA
  XPATH_EXPRESSION_33 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Unaided[@Vision=''' + 'Distant' + ''' and @Situation=''' + 'Standard' + ''']/R/VA';
  XPATH_EXPRESSION_34 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Unaided[@Vision=''' + 'Distant' + ''' and @Situation=''' + 'Standard' + ''']/L/VA';  
  XPATH_EXPRESSION_35 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Unaided[@Vision=''' + 'Distant' + ''' and @Situation=''' + 'Standard' + ''']/B/VA';    

  // Visus CC VA
  XPATH_EXPRESSION_36 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/R/VA';
  XPATH_EXPRESSION_37 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/L/VA';  
  XPATH_EXPRESSION_38 = '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected[@CorrectionType=''' + PHOROPTER_CORRECTION_TYPE + ''' and @Vision=''' + PHOROPTER_VISION + ''' and @Situation=''' + PHOROPTER_SITUATION + ''']/B/VA';

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
  arrData, arrData2: TStringArrayArray;
  arrDataNonGDT: TStringArray;
	ParsedData, S, PatientID: String;	
	WD, PD, VD, VA: String;  
	R_S, R_Z, R_Axis, R_Add, R_PD, R_PRISM, R_PRISMBASE, R_PRISMX, R_PRISMX_BASE, R_PRISMY, R_PRISMY_BASE, R_VA: String; 
	L_S, L_Z, L_Axis, L_Add, L_PD, L_PRISM, L_PRISMBASE, L_PRISMX, L_PRISMX_BASE, L_PRISMY, L_PRISMY_BASE, L_VA: String;
	R_Line, L_Line, VisusLine, VisusLineBase: String;
	sBaseString, sCorrectionType, sVision, sSituation: String;
	Visus_sc_R_VA, Visus_sc_L_VA, Visus_sc_B_VA: String;
	Visus_cc_R_VA, Visus_cc_L_VA, Visus_cc_B_VA: String;
	strXPATH_EXPRESSION_02, strXPATH_EXPRESSION_03, strXPATH_EXPRESSION_04, strXPATH_EXPRESSION_05, strXPATH_EXPRESSION_06, strXPATH_EXPRESSION_07: String;
	strXPATH_EXPRESSION_08, strXPATH_EXPRESSION_09, strXPATH_EXPRESSION_10, strXPATH_EXPRESSION_11, strXPATH_EXPRESSION_12: String;
	strXPATH_EXPRESSION_20, strXPATH_EXPRESSION_21, strXPATH_EXPRESSION_22, strXPATH_EXPRESSION_23, strXPATH_EXPRESSION_24, strXPATH_EXPRESSION_25: String;
	strXPATH_EXPRESSION_26, strXPATH_EXPRESSION_27, strXPATH_EXPRESSION_28, strXPATH_EXPRESSION_31, strXPATH_EXPRESSION_13: String;
	strXPATH_EXPRESSION_29, strXPATH_EXPRESSION_32, strXPATH_EXPRESSION_36, strXPATH_EXPRESSION_37, strXPATH_EXPRESSION_38: String;
  bolAddVDValueToOutput, bolAddWDValueToOutput, bolOnlyUseSubjectiveData, bolReturnUsedLMandREFbaseData, bolUseRT2100PrismaIdentifier: Boolean;
  bolAddSign, bolAddSignSeparator, bolAddAxisSeparator, bolLensmeterDataWasSelected, bolRefraktometerDataWasSelected: Boolean;
  bolAddPrismValuesToOutput, bolAddPrismBaseValueToOutput, bolAddVisusValueToOutput, bolAddSignSeparatorForVisus: Boolean;
  bolCorrectionTypeBestFound, bolCorrectionTypeFullFound, bolIgnoreNightMode, bolUseSubjValuesInOutput, bolAddPDValueToOutput: Boolean;
  i: Integer;
  sCorrectionTypeCustom, sCorrectionTypeVisus: String;

begin

  // Verwende "True", damit der VD Wert an die V2 Zeile angehängt wird,
  // benutze "False", damit der VD Wert ignoriert werden kann.
  bolAddVDValueToOutput := True;

  // Verwende "True", damit der WD Wert hinzugefügt wird, benutze "False", damit der
  // WD Wert nicht per GDT exportiert wird.
  bolAddWDValueToOutput:= True;
  
  // Verwende "True", damit der PD Wert hinzugefügt wird, benutze "False", damit der
  // PD Wert nicht per GDT exportiert wird.
  bolAddPDValueToOutput:= True;
              
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
  
  // Verwende "True", damit nur subjektive Messdaten verarbeitet werden,
  // benutze "False", damit alle Messdaten (außer LM und REF) verarbeitet werden.
  bolOnlyUseSubjectiveData:= True;
  
  // Verwende "True", damit die verwendeten LM und REF Base Daten auch wieder 
  // exportiert werden, benutze "False", damit nur die "RT" Daten exportiert werden.
  // Wenn "True", dann wird KEINE subjektive Messung des Phoropters exportiert, nur LM oder REF.
  bolReturnUsedLMandREFbaseData:= False;
            
  // Verwende "True", damit der Prism Wert an die V2 Zeile angehängt wird,
  // benutze "False", damit der Prism Wert ignoriert werden kann.
  bolAddPrismValuesToOutput:= True;
  
  // Verwende "True", damit der Prism Base Wert an die V2 Zeile angehängt wird,
  // benutze "False", damit der Prism Base Wert ignoriert werden kann.
  bolAddPrismBaseValueToOutput:= True;
  
  // CorrectionType kann sein: "Best", "LM_Base", "REF_Base", "Full", "WF_Base"
  // "Best"     = "Prescription data"
  // "Full"     = "Subjective data"
  // "LM_Base"  = "LM data"
  // "WF_Base"  = "OPD data"
  // "REF_Base" = "Objective AR data"
  
  // Verwende hier den Bezeichner aus dem XML-Messungsdatei, je nach Geräteverwendung
  // kann hier entweder 'Full', oder 'Best'. Full = "Subj" und Best = "Final".
  sCorrectionTypeCustom:= 'Best';
	
  sCorrectionTypeVisus:= 'Best';
  
  // Verwende "True", damit der Visus Wert als die V Zeile exportiert wird,
  // benutze "False", damit der Visus Wert ignoriert werden kann.
  bolAddVisusValueToOutput:= True;
  
  // Verwende "True", damit die Prismen Bezeichner wie beim RT-2100 genutzt werden,
  // benutze "False", damit die original Bezeichner vom RT-6100 benutzt werden.
  bolUseRT2100PrismaIdentifier:= False;
  
  // Verwende "True", damit die SCA Werte des Nacht Modus ignoriert werden,
  // benutze "False", damit alle Werte für die Ausgabe verwendet werden.
  bolIgnoreNightMode:= False;
  
  // Verwende "True", damit im Visus der Wert subj= mit ausgegeben wird,
  // benutze "False", damit subj= nicht in der GDT Ausgabe erscheint.
  bolUseSubjValuesInOutput:= True;
   
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
	
	// check which data was selected for transferring to the phoropter
	bolLensmeterDataWasSelected:= False;
	bolRefraktometerDataWasSelected:= False;
	
	SetLength(arrDataNonGDT, 0);	
	DoPSGetEnabledNonGDTDataGroupList(arrDataNonGDT);
	
	if Length(arrDataNonGDT) > 0 then
	begin
	  for i:= 0 to Length(arrDataNonGDT) - 1 do
	  begin 
	    if T2WContainsStr(arrDataNonGDT[i], 'Lensmeter') then
	    begin
	      bolLensmeterDataWasSelected:= True;
	    end;
	  
	    if T2WContainsStr(arrDataNonGDT[i], 'Refraktometer') then
	    begin
	      bolRefraktometerDataWasSelected:= True;
	    end;	   
	  end;
	end;

	// Patient ID
	PatientID:= '';
	
	SetLength(arrData2, 0);
	arrData2:= DoPSGetXMLData(XPATH_EXPRESSION_01);
	
	if Length(arrData2) > 0 then
	begin
	  S:= arrData2[0][2];
	
	  if (S <> 'NO_ID') then
	  begin
	    PatientID:= PatientID + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
	    PatientID:= PatientID + S + FOutputLineSeparator;
	  end;
	end;
	
	// Build result
	ParsedData:= '';

	// Add patient ID
	if PatientID <> '' then
	begin
	  ParsedData:= ParsedData + PatientID;
	end;
	
	// Clear variables
	WD:= '';
	PD:= '';
	VD:= ''; 
	VA:= '';
	
	R_Line:= '';
	L_Line:= '';
	VisusLine:= '';
	VisusLineBase:= '';
	
	sBaseString:= '';
	sCorrectionType:= '';
	sVision:= '';
	sSituation:= '';
	
	Visus_sc_R_VA:= '';
	Visus_sc_L_VA:= '';
	Visus_sc_B_VA:= '';
	
	Visus_cc_R_VA:= '';
	Visus_cc_L_VA:= '';
	Visus_cc_B_VA:= '';
	
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

	//
	// VA aka Visus SC
	//
	
	if (bolAddVisusValueToOutput) then
	begin
	  // SC R VA
	  Visus_sc_R_VA:= '';
	
	  SetLength(arrData2, 0);
	  arrData2:= DoPSGetXMLData(XPATH_EXPRESSION_33);
	
    if Length(arrData2) > 0 then
    begin
      Visus_sc_R_VA:= Visus_sc_R_VA + FormatSignValue('sc=', arrData2[0][2], False, bolAddSignSeparatorForVisus);
    end;
  
    // SC L VA	
	  Visus_sc_L_VA:= '';
	
	  SetLength(arrData2, 0);
	  arrData2:= DoPSGetXMLData(XPATH_EXPRESSION_34);
	
    if Length(arrData2) > 0 then
    begin
      Visus_sc_L_VA:= Visus_sc_L_VA + FormatSignValue('sc=', arrData2[0][2], False, bolAddSignSeparatorForVisus);
    end;
  
    // SC B VA
    Visus_sc_B_VA:= '';

	  SetLength(arrData2, 0);
	  arrData2:= DoPSGetXMLData(XPATH_EXPRESSION_35);
	
    if Length(arrData2) > 0 then
    begin
      Visus_sc_B_VA:= Visus_sc_B_VA + FormatSignValue('sc=', arrData2[0][2], False, bolAddSignSeparatorForVisus);
    end;
  end;

	// -----------------------------------------------------------------

  // set base path here to get values of all properties
  sBaseString:= '//Ophthalmology/Measure[@Type=''' + 'RT' + ''']/Phoropter/Corrected';

	SetLength(arrData, 0);
	arrData:= DoPSGetXMLData(sBaseString);

	bolCorrectionTypeBestFound:= False;
	bolCorrectionTypeFullFound:= False;

	// so, we check which meassurement modes (CorrectionType) we found, 
	// check if "Best" and / or "Full" is available.
	
  // CorrectionType could be: "Best", "LM_Base", "REF_Base", "Full", "WF_Base"
  
  // "Best" = "Prescription data", "Full" = "Subjective data", "LM_Base" = "LM data"
  // "WF_Base" = "OPD data", "REF_Base" = "Objective AR data"
  if Length(arrData) > 0 then
  begin  
    for i:= 0 to Length(arrData) - 1 do
    begin
      sCorrectionType:= arrData[i][4];
      
      if (sCorrectionType <> '') then
      begin
        if (sCorrectionType = 'Best') then
        begin
          bolCorrectionTypeBestFound:= True;
          Continue;
        end;
        
        if (sCorrectionType = 'Full') then
        begin
          bolCorrectionTypeFullFound:= True;
          Continue;
        end;      
      end;
    end;
  end;
  
  // check which nodes we have and set the correct nodes for the data
  
  // Visus
  
  // Best
  if (sCorrectionTypeVisus = 'Best')
  and (bolCorrectionTypeBestFound) then
  begin
    // it is ok  
  end;

  if (sCorrectionTypeVisus = 'Best')
  and (not bolCorrectionTypeBestFound) then
  begin
    // it is not ok, set other node  
    if (bolCorrectionTypeFullFound) then
    begin
      sCorrectionTypeVisus:= 'Full';
    end;  
  end;

  // Full
  if (sCorrectionTypeVisus = 'Full')
  and (bolCorrectionTypeFullFound) then
  begin
    // it is ok  
  end;

  if (sCorrectionTypeVisus = 'Full')
  and (not bolCorrectionTypeFullFound) then
  begin
    // it is not ok, set other node  
    if (bolCorrectionTypeBestFound) then
    begin
      sCorrectionTypeVisus:= 'Best';
    end;  
  end;
  
  // Data R & L 

  // Best
  if (sCorrectionTypeCustom = 'Best')
  and (bolCorrectionTypeBestFound) then
  begin
    // it is ok  
  end;

  if (sCorrectionTypeCustom = 'Best')
  and (not bolCorrectionTypeBestFound) then
  begin
    // it is not ok, set other node 
    if (bolCorrectionTypeFullFound) then
    begin
      sCorrectionTypeCustom:= 'Full';
    end;  
  end;
  
  // Full
  if (sCorrectionTypeCustom = 'Full')
  and (bolCorrectionTypeFullFound) then
  begin
    // it is ok  
  end;
  
  if (sCorrectionTypeCustom = 'Full')
  and (not bolCorrectionTypeFullFound) then
  begin
    // it is not ok, set other node  
    if (bolCorrectionTypeBestFound) then
    begin
      sCorrectionTypeCustom:= 'Best';
    end;  
  end;

  // reset variable
  sCorrectionType:= '';

  // so, we loop through the response array
  // we determine all available <Corrected> xml nodes 
  if Length(arrData) > 0 then
  begin
    // we check the value of "CorrectionType"
    // FIX: reverse loop, to get all needed values
    for i := Length(arrData) - 1 downto 0 do
    begin      
      // CorrectionType could be: "Best", "LM_Base", "REF_Base", "Full", "WF_Base"
      // "Best" = "Prescription data", "Full" = "Subjective data", "LM_Base" = "LM data"
      // "WF_Base" = "OPD data", "REF_Base" = "Objective AR data"
      sCorrectionType:= arrData[i][4];

      // Vision could be: "Distant", "Near"
      sVision:= arrData[i][6];         
    
      // Situation could be: "Standard", "Night"
      sSituation:= arrData[i][8];

      // we skip "LM_BASE" and "REF_BASE", because we don't want to
      // send the "old" values back
      if (sCorrectionType = 'LM_Base') 
      and (not bolReturnUsedLMandREFbaseData) then
      begin
        Continue;  
      end;
      
      if (sCorrectionType = 'REF_Base') 
      and (not bolReturnUsedLMandREFbaseData) then
      begin
        Continue;
      end;
      
      // we skip "LM_BASE" and "REF_BASE", only if no lensmeter or refraction data was found
      if (sCorrectionType = 'LM_Base') 
      and (bolReturnUsedLMandREFbaseData) 
      and (not bolLensmeterDataWasSelected) then
      begin
        Continue;  
      end;  
                             
      if (sCorrectionType = 'REF_Base') 
      and (bolReturnUsedLMandREFbaseData) 
      and (not bolRefraktometerDataWasSelected) then
      begin
        Continue;
      end;

      if (bolIgnoreNightMode) then
      begin
        if (sSituation <> '') 
        and (sSituation = 'Night') then
        begin      
          Continue;     
          Continue;     
        end;     
      end;
      
      // Visus CC VA
      if (sCorrectionType = sCorrectionTypeVisus) 
      and (bolAddVisusValueToOutput) then
      begin
        // create dynamically all xpath variables
        strXPATH_EXPRESSION_36:= XPATH_EXPRESSION_36;	
        strXPATH_EXPRESSION_36:= T2WStringReplace(strXPATH_EXPRESSION_36, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
        strXPATH_EXPRESSION_36:= T2WStringReplace(strXPATH_EXPRESSION_36, '<PHOROPTER_VISION/>', sVision, False, False);
        strXPATH_EXPRESSION_36:= T2WStringReplace(strXPATH_EXPRESSION_36, '<PHOROPTER_SITUATION/>', sSituation, False, False);
      
        strXPATH_EXPRESSION_37:= XPATH_EXPRESSION_37;	
        strXPATH_EXPRESSION_37:= T2WStringReplace(strXPATH_EXPRESSION_37, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
        strXPATH_EXPRESSION_37:= T2WStringReplace(strXPATH_EXPRESSION_37, '<PHOROPTER_VISION/>', sVision, False, False);
        strXPATH_EXPRESSION_37:= T2WStringReplace(strXPATH_EXPRESSION_37, '<PHOROPTER_SITUATION/>', sSituation, False, False);
      
        strXPATH_EXPRESSION_38:= XPATH_EXPRESSION_38;	
        strXPATH_EXPRESSION_38:= T2WStringReplace(strXPATH_EXPRESSION_38, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
        strXPATH_EXPRESSION_38:= T2WStringReplace(strXPATH_EXPRESSION_38, '<PHOROPTER_VISION/>', sVision, False, False);
        strXPATH_EXPRESSION_38:= T2WStringReplace(strXPATH_EXPRESSION_38, '<PHOROPTER_SITUATION/>', sSituation, False, False);   
      
        // VA R
        Visus_cc_R_VA:= '';

        SetLength(arrData2, 0);
        arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_36);

        if Length(arrData2) > 0 then
        begin
          Visus_cc_R_VA:= FormatSignValue(' cc=', arrData2[0][2], False, bolAddSignSeparatorForVisus);
        end;
        
        // VA L
        Visus_cc_L_VA:= '';

        SetLength(arrData2, 0);
        arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_37);

        if Length(arrData2) > 0 then
        begin
          Visus_cc_L_VA:= FormatSignValue(' cc=', arrData2[0][2], False, bolAddSignSeparatorForVisus);
        end;
        
        // VA B
        Visus_cc_B_VA:= '';

        SetLength(arrData2, 0);
        arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_38);

        if Length(arrData2) > 0 then
        begin
          Visus_cc_B_VA:= FormatSignValue(' cc=', arrData2[0][2], False, bolAddSignSeparatorForVisus);
        end;

        if (sCorrectionType <> sCorrectionTypeCustom) then
        begin    
          Continue;
        end;
      end;

      // if true, we only use Subjective data, but check if we have the matching CorrectionType for the selection
      if (bolOnlyUseSubjectiveData) then
      begin 
        if (sCorrectionType <> sCorrectionTypeCustom) 
        and (not bolReturnUsedLMandREFbaseData) then
        begin
          Continue; 
        end; 
        
        if (sCorrectionType <> sCorrectionTypeCustom) 
        and (bolReturnUsedLMandREFbaseData) then 
        begin
          if (not bolLensmeterDataWasSelected) 
          and (not bolRefraktometerDataWasSelected) then
          begin
            Continue; 
          end;                 
        end;
      end;

      // create dynamically all xpath variables
      strXPATH_EXPRESSION_02:= XPATH_EXPRESSION_02;	
      strXPATH_EXPRESSION_02:= T2WStringReplace(strXPATH_EXPRESSION_02, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_02:= T2WStringReplace(strXPATH_EXPRESSION_02, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_02:= T2WStringReplace(strXPATH_EXPRESSION_02, '<PHOROPTER_SITUATION/>', sSituation, False, False);

      strXPATH_EXPRESSION_03:= XPATH_EXPRESSION_03;
      strXPATH_EXPRESSION_03:= T2WStringReplace(strXPATH_EXPRESSION_03, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_03:= T2WStringReplace(strXPATH_EXPRESSION_03, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_03:= T2WStringReplace(strXPATH_EXPRESSION_03, '<PHOROPTER_SITUATION/>', sSituation, False, False);		

      strXPATH_EXPRESSION_04:= XPATH_EXPRESSION_04;
      strXPATH_EXPRESSION_04:= T2WStringReplace(strXPATH_EXPRESSION_04, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_04:= T2WStringReplace(strXPATH_EXPRESSION_04, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_04:= T2WStringReplace(strXPATH_EXPRESSION_04, '<PHOROPTER_SITUATION/>', sSituation, False, False);	       
           
      strXPATH_EXPRESSION_05:= XPATH_EXPRESSION_05;
      strXPATH_EXPRESSION_05:= T2WStringReplace(strXPATH_EXPRESSION_05, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_05:= T2WStringReplace(strXPATH_EXPRESSION_05, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_05:= T2WStringReplace(strXPATH_EXPRESSION_05, '<PHOROPTER_SITUATION/>', sSituation, False, False);	       
                 
      strXPATH_EXPRESSION_06:= XPATH_EXPRESSION_06;
      strXPATH_EXPRESSION_06:= T2WStringReplace(strXPATH_EXPRESSION_06, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_06:= T2WStringReplace(strXPATH_EXPRESSION_06, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_06:= T2WStringReplace(strXPATH_EXPRESSION_06, '<PHOROPTER_SITUATION/>', sSituation, False, False);	       
                 
      strXPATH_EXPRESSION_07:= XPATH_EXPRESSION_07;
      strXPATH_EXPRESSION_07:= T2WStringReplace(strXPATH_EXPRESSION_07, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_07:= T2WStringReplace(strXPATH_EXPRESSION_07, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_07:= T2WStringReplace(strXPATH_EXPRESSION_07, '<PHOROPTER_SITUATION/>', sSituation, False, False);	       
              
      strXPATH_EXPRESSION_08:= XPATH_EXPRESSION_08;
      strXPATH_EXPRESSION_08:= T2WStringReplace(strXPATH_EXPRESSION_08, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_08:= T2WStringReplace(strXPATH_EXPRESSION_08, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_08:= T2WStringReplace(strXPATH_EXPRESSION_08, '<PHOROPTER_SITUATION/>', sSituation, False, False);	       
                 
      strXPATH_EXPRESSION_09:= XPATH_EXPRESSION_09;
      strXPATH_EXPRESSION_09:= T2WStringReplace(strXPATH_EXPRESSION_09, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_09:= T2WStringReplace(strXPATH_EXPRESSION_09, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_09:= T2WStringReplace(strXPATH_EXPRESSION_09, '<PHOROPTER_SITUATION/>', sSituation, False, False);	       
                 
      strXPATH_EXPRESSION_10:= XPATH_EXPRESSION_10;
      strXPATH_EXPRESSION_10:= T2WStringReplace(strXPATH_EXPRESSION_10, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_10:= T2WStringReplace(strXPATH_EXPRESSION_10, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_10:= T2WStringReplace(strXPATH_EXPRESSION_10, '<PHOROPTER_SITUATION/>', sSituation, False, False);	       
                  
      strXPATH_EXPRESSION_11:= XPATH_EXPRESSION_11;
      strXPATH_EXPRESSION_11:= T2WStringReplace(strXPATH_EXPRESSION_11, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_11:= T2WStringReplace(strXPATH_EXPRESSION_11, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_11:= T2WStringReplace(strXPATH_EXPRESSION_11, '<PHOROPTER_SITUATION/>', sSituation, False, False);	       
                   
      strXPATH_EXPRESSION_12:= XPATH_EXPRESSION_12;
      strXPATH_EXPRESSION_12:= T2WStringReplace(strXPATH_EXPRESSION_12, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_12:= T2WStringReplace(strXPATH_EXPRESSION_12, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_12:= T2WStringReplace(strXPATH_EXPRESSION_12, '<PHOROPTER_SITUATION/>', sSituation, False, False);	       
            
      strXPATH_EXPRESSION_13:= XPATH_EXPRESSION_13;
      strXPATH_EXPRESSION_13:= T2WStringReplace(strXPATH_EXPRESSION_13, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_13:= T2WStringReplace(strXPATH_EXPRESSION_13, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_13:= T2WStringReplace(strXPATH_EXPRESSION_13, '<PHOROPTER_SITUATION/>', sSituation, False, False);       
   
      //     
      strXPATH_EXPRESSION_20:= XPATH_EXPRESSION_20;
      strXPATH_EXPRESSION_20:= T2WStringReplace(strXPATH_EXPRESSION_20, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_20:= T2WStringReplace(strXPATH_EXPRESSION_20, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_20:= T2WStringReplace(strXPATH_EXPRESSION_20, '<PHOROPTER_SITUATION/>', sSituation, False, False);	       
               
      strXPATH_EXPRESSION_21:= XPATH_EXPRESSION_21;
      strXPATH_EXPRESSION_21:= T2WStringReplace(strXPATH_EXPRESSION_21, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_21:= T2WStringReplace(strXPATH_EXPRESSION_21, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_21:= T2WStringReplace(strXPATH_EXPRESSION_21, '<PHOROPTER_SITUATION/>', sSituation, False, False);	       
                    
      strXPATH_EXPRESSION_22:= XPATH_EXPRESSION_22;
      strXPATH_EXPRESSION_22:= T2WStringReplace(strXPATH_EXPRESSION_22, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_22:= T2WStringReplace(strXPATH_EXPRESSION_22, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_22:= T2WStringReplace(strXPATH_EXPRESSION_22, '<PHOROPTER_SITUATION/>', sSituation, False, False);	       
                        
      strXPATH_EXPRESSION_23:= XPATH_EXPRESSION_23;
      strXPATH_EXPRESSION_23:= T2WStringReplace(strXPATH_EXPRESSION_23, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_23:= T2WStringReplace(strXPATH_EXPRESSION_23, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_23:= T2WStringReplace(strXPATH_EXPRESSION_23, '<PHOROPTER_SITUATION/>', sSituation, False, False);	       
                         
      strXPATH_EXPRESSION_24:= XPATH_EXPRESSION_24;
      strXPATH_EXPRESSION_24:= T2WStringReplace(strXPATH_EXPRESSION_24, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_24:= T2WStringReplace(strXPATH_EXPRESSION_24, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_24:= T2WStringReplace(strXPATH_EXPRESSION_24, '<PHOROPTER_SITUATION/>', sSituation, False, False);	       
                        
      strXPATH_EXPRESSION_25:= XPATH_EXPRESSION_25;
      strXPATH_EXPRESSION_25:= T2WStringReplace(strXPATH_EXPRESSION_25, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_25:= T2WStringReplace(strXPATH_EXPRESSION_25, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_25:= T2WStringReplace(strXPATH_EXPRESSION_25, '<PHOROPTER_SITUATION/>', sSituation, False, False);	       
                             
      strXPATH_EXPRESSION_26:= XPATH_EXPRESSION_26;
      strXPATH_EXPRESSION_26:= T2WStringReplace(strXPATH_EXPRESSION_26, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_26:= T2WStringReplace(strXPATH_EXPRESSION_26, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_26:= T2WStringReplace(strXPATH_EXPRESSION_26, '<PHOROPTER_SITUATION/>', sSituation, False, False);	       
                        
      strXPATH_EXPRESSION_27:= XPATH_EXPRESSION_27;
      strXPATH_EXPRESSION_27:= T2WStringReplace(strXPATH_EXPRESSION_27, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_27:= T2WStringReplace(strXPATH_EXPRESSION_27, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_27:= T2WStringReplace(strXPATH_EXPRESSION_27, '<PHOROPTER_SITUATION/>', sSituation, False, False);	       
                       
      strXPATH_EXPRESSION_28:= XPATH_EXPRESSION_28;
      strXPATH_EXPRESSION_28:= T2WStringReplace(strXPATH_EXPRESSION_28, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_28:= T2WStringReplace(strXPATH_EXPRESSION_28, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_28:= T2WStringReplace(strXPATH_EXPRESSION_28, '<PHOROPTER_SITUATION/>', sSituation, False, False);	       
            
      strXPATH_EXPRESSION_29:= XPATH_EXPRESSION_29;
      strXPATH_EXPRESSION_29:= T2WStringReplace(strXPATH_EXPRESSION_29, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_29:= T2WStringReplace(strXPATH_EXPRESSION_29, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_29:= T2WStringReplace(strXPATH_EXPRESSION_29, '<PHOROPTER_SITUATION/>', sSituation, False, False);	          
                
      //
      strXPATH_EXPRESSION_31:= XPATH_EXPRESSION_31;
      strXPATH_EXPRESSION_31:= T2WStringReplace(strXPATH_EXPRESSION_31, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_31:= T2WStringReplace(strXPATH_EXPRESSION_31, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_31:= T2WStringReplace(strXPATH_EXPRESSION_31, '<PHOROPTER_SITUATION/>', sSituation, False, False);	             

      strXPATH_EXPRESSION_32:= XPATH_EXPRESSION_32;
      strXPATH_EXPRESSION_32:= T2WStringReplace(strXPATH_EXPRESSION_32, '<PHOROPTER_CORRECTION_TYPE/>', sCorrectionType, False, False);
      strXPATH_EXPRESSION_32:= T2WStringReplace(strXPATH_EXPRESSION_32, '<PHOROPTER_VISION/>', sVision, False, False);
      strXPATH_EXPRESSION_32:= T2WStringReplace(strXPATH_EXPRESSION_32, '<PHOROPTER_SITUATION/>', sSituation, False, False);

      // -----------------------------------------------------------------

      //
      // VD
      //
      VD:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_02);

      if Length(arrData2) > 0 then
      begin
        VD:= VD + 'VD= ' + arrData2[0][2];
      end;

      //
      // WD
      //
      WD:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_03);

      if Length(arrData2) > 0 then
      begin
        WD:= WD + 'WD= ' + arrData2[0][2];
      end;	

      //
      // RIGHT
      //

      // R_S
      R_S:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_04);

      if Length(arrData2) > 0 then
      begin
        R_S:= R_S + FormatSignValue('S=', arrData2[0][2], bolAddSign, bolAddSignSeparator);
      end;	

      // R_Z
      R_Z:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_05);

      if Length(arrData2) > 0 then
      begin
        R_Z:= R_Z + FormatSignValue('Z=', Trim(arrData2[0][2]), bolAddSign, bolAddSignSeparator);
      end;	

      // R_Axis
      R_Axis:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_06);

      if Length(arrData2) > 0 then
      begin
        R_Axis:= R_Axis + FormatAxisValue('*', arrData2[0][2], bolAddAxisSeparator);
      end;	

      // R_ADD
      R_Add:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_07);

      if Length(arrData2) > 0 then
      begin
        R_Add:= R_Add + FormatSignValue('A=', arrData2[0][2], bolAddSign, bolAddSignSeparator);
      end;	

      // R_PD
      R_PD:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_08);

      if Length(arrData2) > 0 then
      begin
        R_PD:= R_PD + FormatSignValue('PD=', arrData2[0][2], False, bolAddSignSeparator);
      end;	

      // R_PRISM	
      R_PRISM:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_09);

      if Length(arrData2) > 0 then
      begin
        R_PRISM:= R_PRISM + FormatSignValue('P=', arrData2[0][2], bolAddSign, bolAddSignSeparator);
      end;

      // R_PRISMBASE
      R_PRISMBASE:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_10);

      if Length(arrData2) > 0 then
      begin
        R_PRISMBASE:= R_PRISMBASE + FormatSignValue('B=', arrData2[0][2], bolAddSign, bolAddSignSeparator);
      end;

      // R_PRISMX
      R_PRISMX:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_11);

      if Length(arrData2) > 0 then
      begin
        R_PRISMX:= R_PRISMX + FormatSignValue('P=', arrData2[0][2], False, bolAddSignSeparator);
      end;

      // R_PRISMX_BASE
      R_PRISMX_BASE:= '';

      if Length(arrData2) > 0 then
      begin
        R_PRISMX_BASE:= arrData2[0][6];
        
        if R_PRISMX_BASE = 'in' then
        begin
          R_PRISMX_BASE:= 'I';
          
          if (bolUseRT2100PrismaIdentifier) then
          begin
            R_PRISMX_BASE:= 'I';
          end;
        end;
        
        if R_PRISMX_BASE = 'out' then
        begin
          R_PRISMX_BASE:= 'O';
          
          if (bolUseRT2100PrismaIdentifier) then
          begin
            R_PRISMX_BASE:= 'O';
          end;
        end;          
      end;

      // build prismx output string
      R_PRISMX := R_PRISMX + ' ' + R_PRISMX_BASE;
      R_PRISMX := Trim(R_PRISMX);

      // R_PRISMY
      R_PRISMY:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_12);

      if Length(arrData2) > 0 then
      begin
        R_PRISMY:= R_PRISMY + FormatSignValue('', arrData2[0][2], False, bolAddSignSeparator);
      end;

      // R_PRISMY_BASE
      R_PRISMY_BASE:= '';

      if Length(arrData2) > 0 then
      begin
        R_PRISMY_BASE:= arrData2[0][6];
        
        if R_PRISMY_BASE = 'up' then
        begin
          R_PRISMY_BASE:= 'U';
          
          if (bolUseRT2100PrismaIdentifier) then
          begin
            R_PRISMY_BASE:= 'U';
          end;
        end;
        
        if R_PRISMY_BASE = 'down' then
        begin
          R_PRISMY_BASE:= 'D';
          
          if (bolUseRT2100PrismaIdentifier) then
          begin
            R_PRISMY_BASE:= 'D';
          end;
        end;          
      end;

      // build prismy output string
      R_PRISMY := R_PRISMY + ' ' + R_PRISMY_BASE;
      R_PRISMY := Trim(R_PRISMY);
      
      // VA
      R_VA:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_13);

      if (Length(arrData2) > 0) 
      and (bolUseSubjValuesInOutput) then
      begin
        R_VA:= R_VA + FormatSignValue(' subj=', arrData2[0][2], False, bolAddSignSeparatorForVisus);
      end;	 

      // 
      // LEFT
      // 

      // L_S
      L_S:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_20);

      if Length(arrData2) > 0 then
      begin
        L_S:= L_S + FormatSignValue('S=', arrData2[0][2], bolAddSign, bolAddSignSeparator);
      end;	

      // L_Z
      L_Z:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_21);

      if Length(arrData2) > 0 then
      begin
        L_Z:= L_Z + FormatSignValue('Z=', Trim(arrData2[0][2]), bolAddSign, bolAddSignSeparator);
      end;	

      // L_Axis
      L_Axis:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_22);

      if Length(arrData2) > 0 then
      begin
        L_Axis:= L_Axis + FormatAxisValue('*', arrData2[0][2], bolAddAxisSeparator);
      end;	

      // L_ADD
      L_Add:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_23);

      if Length(arrData2) > 0 then
      begin
        L_Add:= L_Add + FormatSignValue('A=', arrData2[0][2], bolAddSign, bolAddSignSeparator);
      end;	

      // L_PD
      L_PD:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_24);

      if Length(arrData2) > 0 then
      begin
        L_PD:= L_PD + FormatSignValue('PD=', arrData2[0][2], False, bolAddSignSeparator);
      end;	

      // L_PRISM	
      L_PRISM:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_25);

      if Length(arrData2) > 0 then
      begin
        L_PRISM:= L_PRISM + FormatSignValue('P=', arrData2[0][2], bolAddSign, bolAddSignSeparator);
      end;

      // L_PRISMBASE
      L_PRISMBASE:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_26);

      if Length(arrData2) > 0 then
      begin
        L_PRISMBASE:= L_PRISMBASE + FormatSignValue('B=', arrData2[0][2], bolAddSign, bolAddSignSeparator);
      end;	

      // L_PRISMX
      L_PRISMX:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_27);

      if Length(arrData2) > 0 then
      begin
        L_PRISMX:= L_PRISMX + FormatSignValue('P=', arrData2[0][2], False, bolAddSignSeparator);
      end;

      // L_PRISMX_BASE
      L_PRISMX_BASE:= '';

      if Length(arrData2) > 0 then
      begin
        L_PRISMX_BASE:= arrData2[0][6];
        
        if L_PRISMX_BASE = 'in' then
        begin
          L_PRISMX_BASE:= 'I';
          
          if (bolUseRT2100PrismaIdentifier) then
          begin
            L_PRISMX_BASE:= 'I';
          end;
        end;
        
        if L_PRISMX_BASE = 'out' then
        begin
          L_PRISMX_BASE:= 'O';
          
          if (bolUseRT2100PrismaIdentifier) then
          begin
            L_PRISMX_BASE:= 'O';
          end;
        end;          
      end;
              
      // build prismx output string 		
      L_PRISMX := L_PRISMX + ' ' + L_PRISMX_BASE;	
      L_PRISMX := Trim(L_PRISMX);

      // L_PRISMY
      L_PRISMY:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_28);

      if Length(arrData2) > 0 then
      begin
        L_PRISMY:= L_PRISMY + FormatSignValue('', arrData2[0][2], False, bolAddSignSeparator);
      end;

      // L_PRISMY_BASE
      L_PRISMY_BASE:= '';

      if Length(arrData2) > 0 then
      begin
        L_PRISMY_BASE:= arrData2[0][6];
        
        if L_PRISMY_BASE = 'up' then
        begin
          L_PRISMY_BASE:= 'U';
          
          if (bolUseRT2100PrismaIdentifier) then
          begin
            L_PRISMY_BASE:= 'U';
          end;
        end;
        
        if L_PRISMY_BASE = 'down' then
        begin
          L_PRISMY_BASE:= 'D';
          
          if (bolUseRT2100PrismaIdentifier) then
          begin
            L_PRISMY_BASE:= 'D';
          end;
        end;          
      end;

      // build prismx output string 		
      L_PRISMY := L_PRISMY + ' ' + L_PRISMY_BASE;	
      L_PRISMY := Trim(L_PRISMY);
      
      // VA
      L_VA:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_29);

      if (Length(arrData2) > 0)
      and (bolUseSubjValuesInOutput) then
      begin
        L_VA:= L_VA + FormatSignValue(' subj=', arrData2[0][2], False, bolAddSignSeparatorForVisus);
      end;

      //
      // PD
      //
      PD:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_31);

      if Length(arrData2) > 0 then
      begin
        PD:= PD + FormatSignValue('PD=', arrData2[0][2], False, bolAddSignSeparator);
      end;	

      // each eye have a PD value, and we have a global PD value, so if one of the eye PD 
      // values are empty, we use the global one.
      if (R_PD = '') then
      begin
        R_PD:= PD;
      end;	

      if (L_PD = '') then
      begin
        L_PD:= PD;
      end;
      
      //
      // VA
      //
      VA:= '';

      SetLength(arrData2, 0);
      arrData2:= DoPSGetXMLData(strXPATH_EXPRESSION_32);

      if (Length(arrData2) > 0) 
      and (bolUseSubjValuesInOutput) then
      begin
        VA:= VA + FormatSignValue(' subj=', arrData2[0][2], False, bolAddSignSeparatorForVisus);
      end;	
      	       
      // -----------------------------------------------------------------

      // Add right eye
      R_Line:= '';

      if (R_S <> '') and (R_Z <> '') and (R_Axis <> '') then
      begin
        R_Line:= R_Line + 'R.:' + R_S + ' ' + R_Z + R_Axis;
      end;

      // add PRISM and BASE if set
      if (R_PRISM <> '') and (R_PRISMBASE <> '') and (bolAddPrismValuesToOutput) then
      begin
        if R_Line <> '' then
        begin
          R_Line:= R_Line + ' ';
        end;
      	
      	if (bolAddPrismBaseValueToOutput) then
          R_Line:= R_Line + R_PRISM + ' ' + R_PRISMBASE     	
      	else 
      	  R_Line:= R_Line + R_PRISM;
      end;

      // add PRISM_X and PRISM_Y
      if (R_PRISMX <> '') and (R_PRISMY <> '') and (bolAddPrismValuesToOutput) then
      begin
        if R_Line <> '' then
        begin
          R_Line:= R_Line + ' ';
        end;	

        R_Line:= R_Line + R_PRISMX + ' ' + R_PRISMY;
      end;
      
      // add ADD
      if (R_Add <> '')  then
      begin
        if R_Line <> '' then
        begin
          R_Line:= R_Line + ' ';
        end;

        R_Line:= R_Line + R_Add;
      end;
      
      // add PD
      if (R_PD <> '') 
      and (bolAddPDValueToOutput) then
      begin
        if R_Line <> '' then
        begin
          R_Line:= R_Line + ' ';
        end;
      	
        R_Line:= R_Line + R_PD;
      end;	
      	
      // Add VD
      if (VD <> '') and (bolAddVDValueToOutput) then
      begin
        if R_Line <> '' then
        begin
          R_Line:= R_Line + ' ';
        end;
      	
        R_Line:= R_Line + VD;
      end;

      if R_Line <> '' then
      begin
        ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
        ParsedData:= ParsedData + R_Line + FOutputLineSeparator;
      end;

      // Add left eye
      L_Line:= '';

      if (L_S <> '') and (L_Z <> '') and (L_Axis <> '') then
      begin
        L_Line:= L_Line + 'L.:' + L_S + ' ' + L_Z + L_Axis;
      end;

      // add PRISM and BASE if set
      if (L_PRISM <> '') and (L_PRISMBASE <> '') and (bolAddPrismValuesToOutput) then
      begin
        if L_Line <> '' then
        begin
          L_Line:= L_Line + ' ';
        end;
        
        if (bolAddPrismBaseValueToOutput) then
          L_Line:= L_Line + L_PRISM + ' ' + L_PRISMBASE
        else     
          L_Line:= L_Line + L_PRISM;
      end;

      // add PRISM_X and PRISM_Y
      if (L_PRISMX <> '') and (L_PRISMY <> '') and (bolAddPrismValuesToOutput) then
      begin
        if L_Line <> '' then
        begin
          L_Line:= L_Line + ' ';
        end;	

        L_Line:= L_Line + L_PRISMX + ' ' + L_PRISMY;
      end;
      
      // add ADD
      if (L_Add <> '')  then
      begin
        if L_Line <> '' then
        begin
          L_Line:= L_Line + ' ';
        end;

        L_Line:= L_Line + L_Add;
      end; 

      // add PD
      if (L_PD <> '') 
      and (bolAddPDValueToOutput) then
      begin
        if L_Line <> '' then
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
      if (WD <> '') and (bolAddWDValueToOutput) then
      begin
        // Append WD index with GDT comment field id to measure data
        ParsedData := ParsedData + FOutputLineSeparator + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_COMMENT + FOutputLineSeparator + 'WD= ' + Trim(WD);
      end;

      // VA aka Visus
      if (bolAddVisusValueToOutput) then
      begin
        if (Visus_sc_R_VA <> '') and (Visus_sc_L_VA <> '') and (Visus_sc_B_VA <> '') then
        begin
          if (Visus_cc_R_VA <> '') and (Visus_cc_L_VA <> '') and (Visus_cc_B_VA <> '') then
          begin
            VisusLine:= VisusLine + 'R:' + Visus_sc_R_VA + Visus_cc_R_VA + R_VA + ' //L:' + Visus_sc_L_VA + Visus_cc_L_VA + L_VA;
            VisusLineBase := VisusLineBase + 'B:' + Visus_sc_B_VA + Visus_cc_B_VA + VA;  
          end
          else
          begin
            VisusLine:= VisusLine + 'R:' + Visus_sc_R_VA + Visus_cc_R_VA + R_VA + ' //L:' + Visus_sc_L_VA + Visus_cc_L_VA + L_VA;
            VisusLineBase := VisusLineBase + 'B:' + Visus_sc_B_VA + Visus_cc_B_VA + VA;            
          end; 
        end
        else
        begin
          if (Visus_cc_R_VA <> '') and (Visus_cc_L_VA <> '') and (Visus_cc_B_VA <> '') then
          begin           
            VisusLine:= VisusLine + 'R:' + Visus_sc_R_VA + Visus_cc_R_VA + R_VA + ' //L:' + Visus_sc_L_VA + Visus_cc_L_VA + L_VA;
            VisusLineBase := VisusLineBase + 'B:' + Visus_sc_B_VA + Visus_cc_B_VA + VA;    
          end
          else
          begin
            if (R_VA <> '') or (L_VA <> '') then
            begin           
              VisusLine:= VisusLine + 'R:' + Trim(R_VA) + ' //L:' + Trim(L_VA);
            end;
            
            if (Visus_sc_B_VA <> '') or (Visus_cc_B_VA <> '') or (VA <> '') then
            begin
              VisusLineBase := VisusLineBase + 'B:' + Visus_sc_B_VA + Visus_cc_B_VA + VA;
            end;
          end;   
        end;

	      if (VisusLine <> '') then
	      begin
	        ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_RESULT + FOutputLineSeparator;
	        ParsedData:= ParsedData + Trim(VisusLine) + FOutputLineSeparator;
	      end;
	      
	      if (VisusLineBase <> '') then
	      begin
	        ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FOREIGN_RESULT + FOutputLineSeparator;
	        ParsedData:= ParsedData + Trim(VisusLineBase) + FOutputLineSeparator;
	      end;
      end;

      // exit the loop if strict mode was selected, aka only send lensmeter / refraction back 
      // if lensmeter / refraction data was selected
      if (bolReturnUsedLMandREFbaseData) then
      begin
        if (sCorrectionType = 'LM_Base') and (bolLensmeterDataWasSelected) then
        begin
          Break;
        end;
      
        if (sCorrectionType = 'REF_Base') and (bolRefraktometerDataWasSelected) then
        begin
          Break;
        end;            
      end;            
    end;        
  end;

	// Set output
	FParsedDataString:= RawByteString(ParsedData);
end.
