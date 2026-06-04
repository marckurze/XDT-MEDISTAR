const
  VERSION = '1.0.55.43';
  DATE = '15.09.2022 13:37:41';
  TEXT = 'Copyright (c) 2022 team2work GmbH';

  DATA_LINE_SEPARATOR_01 = #$0D; // #$0D#$0A

  DEVICE_IDENTIFIER = 'VISUREF100';

  GDT_FID_PATIENT_ID = '3000';
  GDT_FID_MEASURE_DATA = '6228';
  GDT_FID_RESULT = '6220';
  GDT_FID_FOREIGN_RESULT = '6221';	
	
  GDT_LINE_PREFIX = '  ';
  GDT_SIGN_SEPARATOR = ' ';
  GDT_AXIS_SEPARATOR = ' ';
  GDT_AXIS_SEPARATOR_MAX_COUNT = 3;

function FormatSignValue(const ID, Value: String; AddSign: Boolean; AddSignSeparator: Boolean): String;
var
  S1, S2: String;
begin
  if (T2WContainsStr(Value, '*')) then
  begin
    // Result:= '';
    // Exit;
  end;

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
  if (T2WContainsStr(Value, '*')) then
  begin
    // Result:= '';
    // Exit;
  end;  
  
  Result:= ID;
  
  S:= Value;
  
  if AddAxisSeparator then
    while Length(S) < GDT_AXIS_SEPARATOR_MAX_COUNT do
      S:= GDT_AXIS_SEPARATOR + S;
  
  Result:= Result + S;
end;

var
  arrData: TStringArray;
  bolAddGDTLinePrefix, bolAddSign, bolAddSignSeparator, bolAddAxisSeparator: Boolean;
  bolAddVDValueToOutput, bolAddPDValueToOutput, bolAddKeratometryValueToOutput: Boolean;
  i, indexMeasurementStart: Integer;
  R_Line, L_Line, PatientID, S1, ParsedData, Data: String;
  DeviceName, TimeStampDate, TimeStampTime, MeasuredEye, VD, PD: String;  
  RightEye, Right_SPH, Right_CYL, Right_AXIS: String;
  LeftEye, Left_SPH, Left_CYL, Left_AXIS: String;
  RightEyeKeratometry, Right_KER_Radius_Flat, Right_COR_PWR_Flat, Right_KER_AXIS_Flat: String;
  LeftEyeKeratometry, Left_KER_Radius_Flat, Left_COR_PWR_Flat, Left_KER_AXIS_Flat: String;
  Right_KER_Radius_Steep, Right_COR_PWR_Steep, Right_KER_AXIS_Steep, Right_AVE_KER_Radius: String;
  Left_KER_Radius_Steep, Left_COR_PWR_Steep, Left_KER_AXIS_Steep, Left_AVE_KER_Radius: String;
  Right_AVE_COR_PWR, Right_KER_CYL, Right_KER_AXIS: String;
  Left_AVE_COR_PWR, Left_KER_CYL, Left_KER_AXIS: String;	
  Device_Serial_Number: String;
  Line_Kr_Result, Line_Av_Result: String;

begin

  // Verwende "True", damit der VD Wert an die V1 Zeile angehängt wird,
  // benutze "False", damit der VD Wert ignoriert werden kann.
  bolAddVDValueToOutput:= True;

  // Verwende "True", damit der PD Wert an die V1 Zeile angehängt wird,
  // benutze "False", damit der PD Wert ignoriert werden kann.
  bolAddPDValueToOutput:= True;

  // Verwende "True", damit jeder GDT Zeile das Prefix aus GDT_LINE_PREFIX 
  // vorangestellt wird,
  // benutze "False", damit das GDT-Zeilen-Prefix ignoriert wird
  bolAddGDTLinePrefix:= False;

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
  	
  // Verwende "True", damit die Keratometer Daten ausgegeben werden,
  // benutze "False", damit nur die Refraktor Daten ausgegeben werden.
  bolAddKeratometryValueToOutput:= True;  

  // --- Don't edit script down below ---

  // Clear parsed data string
  FParsedDataString := '';

  if Length(FRawDataString) <= 0 then
  begin
    FLastErrorCode := -2;
    FLastErrorMessage := 'Keine Daten für die Verarbeitung der Daten gefunden';

    DoPSError();
 
    Exit;
  end;

  Data:= String(FRawDataString);
  Data:= Trim(Data);

  // Get array of lines
  arrData := Explode(DATA_LINE_SEPARATOR_01, Data, 0);

  if (Length(arrData) <= 0) then
   begin
    FLastErrorCode := -3;
    FLastErrorMessage := 'Keine Zeilen für die Verarbeitung verfügbar';

    DoPSError();

    Exit;
  end;
  
  // set default values
  indexMeasurementStart:= -1;
  
  PatientID:= '';
  
  R_Line:= '';
  L_Line:= '';  
  
  DeviceName:= '';
  TimeStampDate:= '';
  TimeStampTime:= '';
  
  MeasuredEye:= '';
  VD:= '';
  PD:= '';
  	 
  RightEye:= '';
  Right_SPH:= '';
  Right_CYL:= '';
  Right_AXIS:= '';
  	
  RightEyeKeratometry:= '';
  Right_KER_Radius_Flat:= '';	
  Right_COR_PWR_Flat:= '';
  Right_KER_AXIS_Flat:= '';
  Right_KER_Radius_Steep:= '';
  Right_COR_PWR_Steep:= '';	
  Right_KER_AXIS_Steep:= '';	
  Right_AVE_KER_Radius:= '';	
  Right_AVE_COR_PWR:= '';	
  Right_KER_CYL:= '';
  Right_KER_AXIS:= '';

  LeftEye:= '';
  Left_SPH:= '';
  Left_CYL:= '';
  Left_AXIS:= '';  
  
  LeftEyeKeratometry:= '';
  Left_KER_Radius_Flat:= '';
  Left_COR_PWR_Flat:= '';
  Left_KER_AXIS_Flat:= '';
  Left_KER_Radius_Steep:= '';
  Left_COR_PWR_Steep:= '';
  Left_KER_AXIS_Steep:= '';
  Left_AVE_KER_Radius:= '';
  Left_AVE_COR_PWR:= '';
  Left_KER_CYL:= '';
  Left_KER_AXIS:= '';	
  
  Device_Serial_Number:= '';

  // Parse raw data
  for i := 0 to Length(arrData) - 1 do
  begin
    S1:= Trim(arrData[i]);
  
    if (S1 = '') then
      Continue;

    // Device Name
    if (S1 = DEVICE_IDENTIFIER) then
    begin
      indexMeasurementStart:= i;

      DeviceName:= S1;

      Continue;
    end;

    // Time Stamp
    if (TimeStampDate = '')
    and (indexMeasurementStart <> -1) then
    begin
      TimeStampDate:= S1;
      
      Continue;
    end;    

    if (TimeStampTime = '')
    and (indexMeasurementStart <> -1) then
    begin
      TimeStampTime:= S1;
      
      Continue;
    end;
    
    // Measured Eye, Eye, Vertex Distance and Pupil Distance     
    if (MeasuredEye = '')
    and (indexMeasurementStart <> -1) then
    begin
      MeasuredEye:= S1;

      Continue;
    end;
        
    if (VD = '')
    and (indexMeasurementStart <> -1) then
    begin
      VD:= S1;

      Continue;
    end;
    
    if (PD = '')
    and (indexMeasurementStart <> -1) then
    begin
      PD:= S1;

      Continue;
    end;    
   
    // -----------------------------------

    // Right Eye Refraction
    if (RightEye = '')
    and (S1 = 'RR')
    and (indexMeasurementStart <> -1) then
    begin
      RightEye:= S1;
    
      Continue;
    end;
    
    // Right SPH        
    if (Right_SPH = '') 
    and (RightEye = 'RR')
    and (indexMeasurementStart <> -1) then
    begin
      Right_SPH:= S1;
    
      Continue;
    end;
    
    // Right CYL   
    if (Right_CYL = '') 
    and (RightEye = 'RR')
    and (indexMeasurementStart <> -1) then
    begin
      Right_CYL:= S1;
    
      Continue;
    end;
    
    // Right AXIS           
    if (Right_AXIS = '') 
    and (RightEye = 'RR')
    and (indexMeasurementStart <> -1) then
    begin
      Right_AXIS:= S1;
    
      Continue;
    end;
    
    // -----------------------------------    
    
    // Right Eye Keratometry
    if (RightEyeKeratometry = '')
    and (S1 = 'RK')
    and (indexMeasurementStart <> -1) then
    begin
      RightEyeKeratometry:= S1;
    
      Continue;
    end;
    
    // Right KER Radius Flat    
    if (Right_KER_Radius_Flat = '') 
    and (RightEyeKeratometry = 'RK')
    and (indexMeasurementStart <> -1) then
    begin
      Right_KER_Radius_Flat:= S1;
    
      Continue;
    end;
    
    // Right COR PWR Flat    
    if (Right_COR_PWR_Flat = '') 
    and (RightEyeKeratometry = 'RK')
    and (indexMeasurementStart <> -1) then
    begin
      Right_COR_PWR_Flat:= S1;
    
      Continue;
    end;
    
    // Right KER AXIS Flat
    if (Right_KER_AXIS_Flat = '') 
    and (RightEyeKeratometry = 'RK')
    and (indexMeasurementStart <> -1) then
    begin
      Right_KER_AXIS_Flat:= S1;
    
      Continue;
    end;
    
    // Right KER Radius Steep   
    if (Right_KER_Radius_Steep = '') 
    and (RightEyeKeratometry = 'RK')
    and (indexMeasurementStart <> -1) then
    begin
      Right_KER_Radius_Steep:= S1;
    
      Continue;
    end;
    
    // Right COR PWR Steep       
    if (Right_COR_PWR_Steep = '') 
    and (RightEyeKeratometry = 'RK')
    and (indexMeasurementStart <> -1) then
    begin
      Right_COR_PWR_Steep:= S1;
    
      Continue;
    end;
    
    // Right KER AXIS Steep        
    if (Right_KER_AXIS_Steep = '') 
    and (RightEyeKeratometry = 'RK')
    and (indexMeasurementStart <> -1) then
    begin
      Right_KER_AXIS_Steep:= S1;
    
      Continue;
    end;
    
    // Right AVE KER Radius
    if (Right_AVE_KER_Radius = '') 
    and (RightEyeKeratometry = 'RK')
    and (indexMeasurementStart <> -1) then
    begin
      Right_AVE_KER_Radius:= S1;
    
      Continue;
    end;
    
    // Right AVE COR PWR	
    if (Right_AVE_COR_PWR = '') 
    and (RightEyeKeratometry = 'RK')
    and (indexMeasurementStart <> -1) then
    begin
      Right_AVE_COR_PWR:= S1;
    
      Continue;
    end;
    
    // Right KER CYL     
    if (Right_KER_CYL = '') 
    and (RightEyeKeratometry = 'RK')
    and (indexMeasurementStart <> -1) then
    begin
      Right_KER_CYL:= S1;
    
      Continue;
    end;
    
    // Right KER AXIS       
    if (Right_KER_AXIS = '') 
    and (RightEyeKeratometry = 'RK')
    and (indexMeasurementStart <> -1) then
    begin
      Right_KER_AXIS:= S1;
    
      Continue;
    end;

    // ----------------------------------- 
    
    // Left Eye Refraction
    if (LeftEye = '')
    and (S1 = 'LR')
    and (indexMeasurementStart <> -1) then
    begin
      LeftEye:= S1;
    
      Continue;
    end;    
    
    // Left SPH        
    if (Left_SPH = '') 
    and (LeftEye = 'LR')
    and (indexMeasurementStart <> -1) then
    begin
      Left_SPH:= S1;
    
      Continue;
    end;    
    
    // Left CYL   
    if (Left_CYL = '') 
    and (LeftEye = 'LR')
    and (indexMeasurementStart <> -1) then
    begin
      Left_CYL:= S1;
    
      Continue;
    end;    
    
    // Left AXIS           
    if (Left_AXIS = '') 
    and (LeftEye = 'LR')
    and (indexMeasurementStart <> -1) then
    begin
      Left_AXIS:= S1;
    
      Continue;
    end;
    
    // -----------------------------------
    
    // Left Eye Keratometry    
    if (LeftEyeKeratometry = '')
    and (S1 = 'LK')
    and (indexMeasurementStart <> -1) then
    begin
      LeftEyeKeratometry:= S1;
    
      Continue;
    end;
    
    // Left KER Radius Flat    
    if (Left_KER_Radius_Flat = '') 
    and (LeftEyeKeratometry = 'LK')
    and (indexMeasurementStart <> -1) then
    begin
      Left_KER_Radius_Flat:= S1;
    
      Continue;
    end;
    
    // Left COR PWR Flat        
    if (Left_COR_PWR_Flat = '') 
    and (LeftEyeKeratometry = 'LK')
    and (indexMeasurementStart <> -1) then
    begin
      Left_COR_PWR_Flat:= S1;
    
      Continue;
    end;
    
    // Left KER AXIS Flat
    if (Left_KER_AXIS_Flat = '') 
    and (LeftEyeKeratometry = 'LK')
    and (indexMeasurementStart <> -1) then
    begin
      Left_KER_AXIS_Flat:= S1;
    
      Continue;
    end;
    
    // Left KER Radius Steep        
    if (Left_KER_Radius_Steep = '') 
    and (LeftEyeKeratometry = 'LK')
    and (indexMeasurementStart <> -1) then
    begin
      Left_KER_Radius_Steep:= S1;
    
      Continue;
    end;
    
    // Left COR PWR Steep    
    if (Left_COR_PWR_Steep = '') 
    and (LeftEyeKeratometry = 'LK')
    and (indexMeasurementStart <> -1) then
    begin
      Left_COR_PWR_Steep:= S1;
    
      Continue;
    end;
    
    // Left KER AXIS Steep    
    if (Left_KER_AXIS_Steep = '') 
    and (LeftEyeKeratometry = 'LK')
    and (indexMeasurementStart <> -1) then
    begin
      Left_KER_AXIS_Steep:= S1;
    
      Continue;
    end;
    
    // Left AVE KER Radius    
    if (Left_AVE_KER_Radius = '') 
    and (LeftEyeKeratometry = 'LK')
    and (indexMeasurementStart <> -1) then
    begin
      Left_AVE_KER_Radius:= S1;
    
      Continue;
    end;
    
    // Left AVE COR PWR    
    if (Left_AVE_COR_PWR = '') 
    and (LeftEyeKeratometry = 'LK')
    and (indexMeasurementStart <> -1) then
    begin
      Left_AVE_COR_PWR:= S1;
    
      Continue;
    end;
    
    // Left KER CYL    
    if (Left_KER_CYL = '') 
    and (LeftEyeKeratometry = 'LK')
    and (indexMeasurementStart <> -1) then
    begin
      Left_KER_CYL:= S1;
    
      Continue;
    end;
    
    // Left KER AXIS    
    if (Left_KER_AXIS = '') 
    and (LeftEyeKeratometry = 'LK')
    and (indexMeasurementStart <> -1) then
    begin
      Left_KER_AXIS:= S1;
    
      Continue;
    end;
    
    // -----------------------------------
    
    // Device Serial Number    
    if (Device_Serial_Number = '')
    and (indexMeasurementStart <> -1) then
    begin
      Device_Serial_Number:= S1;
    
      Continue;
    end;      

    //     
    
  end;

  // Build output
  ParsedData := '';

  // Add patient ID
  if (PatientID <> '') then
   begin
    ParsedData := ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
    ParsedData := ParsedData + PatientID + FOutputLineSeparator;
  end;
  
  // VD
  if (VD <> '') then
  begin
    VD:= 'VD= ' + VD;
  end;  
   
  // PD
  if (PD <> '') then
  begin
    PD:= 'PD= ' + PD;
  end;
  
  // Add right eye     
  R_Line:= '';

  if (Right_SPH <> '') then
  begin
    if (not T2WContainsStr(Right_SPH, '*')) then
    begin
      Right_SPH:= FormatSignValue('S=', Right_SPH, bolAddSign, bolAddSignSeparator);
    end;
  end;

  if (Right_CYL <> '') then
  begin
    if (not T2WContainsStr(Right_CYL, '*')) then
    begin
      Right_CYL:= FormatSignValue('Z=', Right_CYL, bolAddSign, bolAddSignSeparator);
    end;
  end;  
  
  if (Right_AXIS <> '') then
  begin
    if (not T2WContainsStr(Right_AXIS, '*')) then
    begin
      Right_AXIS:= FormatAxisValue('*', Right_AXIS, bolAddAxisSeparator);
    end;
  end;  

  if (Right_SPH <> '') and (Right_CYL <> '') and (Right_AXIS <> '') then
  begin
    if (bolAddGDTLinePrefix) then
      R_Line:= R_Line + GDT_LINE_PREFIX;
      	
    R_Line:= R_Line + 'R.:' + Right_SPH + ' ' + Right_CYL + Right_AXIS;
  end;

  // Add PD
  if (PD <> '') 
  and (bolAddPDValueToOutput) 
  and (not T2WContainsStr(PD, '*'))  then
   begin
    if (R_Line <> '') then
    begin
      R_Line:= R_Line + ' ';
    end;
		
    R_Line:= R_Line + PD;
  end;
	
  // Add VD
  if (VD <> '') 
  and (bolAddVDValueToOutput)
  and (not T2WContainsStr(VD, '*'))  then
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

  if (Left_SPH <> '') then
  begin
    if (not T2WContainsStr(Left_SPH, '*')) then
    begin
      Left_SPH:= FormatSignValue('S=', Left_SPH, bolAddSign, bolAddSignSeparator);
    end;
  end;
  
  if (Left_CYL <> '') then
  begin
    if (not T2WContainsStr(Left_CYL, '*')) then
    begin
      Left_CYL:= FormatSignValue('Z=', Left_CYL, bolAddSign, bolAddSignSeparator);
    end;
  end;  
  
  if (Left_AXIS <> '') then
  begin
    if (not T2WContainsStr(Left_AXIS, '*')) then
    begin
      Left_AXIS:= FormatAxisValue('*', Left_AXIS, bolAddAxisSeparator);
    end;
  end; 
  
  if (Left_SPH <> '') and (Left_CYL <> '') and (Left_AXIS <> '') then
  begin
    if (bolAddGDTLinePrefix) then
      L_Line:= L_Line + GDT_LINE_PREFIX;
    	
    L_Line:= L_Line + 'L.:' + Left_SPH + ' ' + Left_CYL + Left_AXIS;
  end;

  if (L_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
  end;

  // add keratometry parts
	
  // KR
  Line_Kr_Result:= '';
  Line_Kr_Result:= Line_Kr_Result + 'R:' + ' ';
  Line_Kr_Result:= Line_Kr_Result + FormatSignValue('R1=', Right_KER_Radius_Flat, bolAddSign, bolAddSignSeparator);		
  Line_Kr_Result:= Line_Kr_Result + FormatAxisValue('*', Right_KER_AXIS_Flat, bolAddAxisSeparator);	
  Line_Kr_Result:= Line_Kr_Result + ' ';
  Line_Kr_Result:= Line_Kr_Result + FormatSignValue('R2=', Right_KER_Radius_Steep, bolAddSign, bolAddSignSeparator);
  Line_Kr_Result:= Line_Kr_Result + FormatAxisValue('*', Right_KER_AXIS_Steep, bolAddAxisSeparator);	
	
  Line_Kr_Result:= Line_Kr_Result + ' // L: ';	
  Line_Kr_Result:= Line_Kr_Result + FormatSignValue('R1=', Left_KER_Radius_Flat, bolAddSign, bolAddSignSeparator);	
  Line_Kr_Result:= Line_Kr_Result + FormatAxisValue('*', Left_KER_AXIS_Flat, bolAddAxisSeparator);			
  Line_Kr_Result:= Line_Kr_Result + ' ';
  Line_Kr_Result:= Line_Kr_Result + FormatSignValue('R2=', Left_KER_Radius_Steep, bolAddSign, bolAddSignSeparator);				
  Line_Kr_Result:= Line_Kr_Result + FormatAxisValue('*', Left_KER_AXIS_Steep, bolAddAxisSeparator);				

  if (Line_Kr_Result <> '') 
  and (bolAddKeratometryValueToOutput) then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_RESULT + FOutputLineSeparator;
    ParsedData:= ParsedData + Line_Kr_Result + FOutputLineSeparator;
  end;		
	
  // AV
  Line_Av_Result:= ''; 
  Line_Av_Result:= Line_Av_Result + 'R: ';
  Line_Av_Result:= Line_Av_Result + FormatSignValue('AV=', Right_AVE_KER_Radius, bolAddSign, bolAddSignSeparator) + ' ';
  Line_Av_Result:= Line_Av_Result + FormatSignValue('CYL=', Right_KER_CYL, bolAddSign, bolAddSignSeparator);	
  Line_Av_Result:= Line_Av_Result + ' // L: ';
  Line_Av_Result:= Line_Av_Result + FormatSignValue('AV=', Left_AVE_KER_Radius, bolAddSign, bolAddSignSeparator) + ' ';	
  Line_Av_Result:= Line_Av_Result + FormatSignValue('CYL=', Left_KER_CYL, bolAddSign, bolAddSignSeparator);		

  if (Line_Av_Result <> '') 
  and (bolAddKeratometryValueToOutput) then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_FOREIGN_RESULT + FOutputLineSeparator;
    ParsedData:= ParsedData + Line_Av_Result + FOutputLineSeparator;
  end;	

  // Set output
  FParsedDataString := RawByteString(ParsedData);
end.