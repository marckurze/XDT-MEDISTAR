const
  VERSION = '1.0.55.55';
  DATE = '05.03.2026 11:36:09';
  TEXT = 'Copyright (c) 2026 CompuGroup Medical Deutschland AG';
  
  DATA_SEPARATOR_01    = #$0D;  // \r
  // DATA_SEPARATOR_01    = #$0D#$0A; // \r\n 
  
  LINE_IDENTIFIER_R    = 'R';
  LINE_IDENTIFIER_L    = 'L';

  GDT_FID_PATIENT_ID   = '3000';
  GDT_FID_MEASURE_DATA = '6228';
  GDT_FID_COMMENT      = '6227';
  
  GDT_LINE_PREFIX              = '  ';
  GDT_SIGN_SEPARATOR           = ' ';
  GDT_AXIS_SEPARATOR           = ' ';
  GDT_AXIS_SEPARATOR_MAX_COUNT = 3;

  FIELD_NO_LINE_IDENTIFIER_R   = 1;
  FIELD_NO_R_SPHERE            = 2;
  FIELD_NO_R_CYLINDER          = 3;
  FIELD_NO_R_AXIS              = 4;

  FIELD_NO_LINE_IDENTIFIER_L   = 6;
  FIELD_NO_L_SPHERE            = 7;
  FIELD_NO_L_CYLINDER          = 8;
  FIELD_NO_L_AXIS              = 9;
  
  FIELD_NO_PD                  = 11;
 
var
  Data, Sep1, S1, ParsedData, strGDT_LINE_PREFIX, VD, PD, R_Line, L_Line: String;
  R_SPH, R_CYL, R_AXIS, L_SPH, L_CYL, L_AXIS: String; 
  arrData1: TStringArray;
  i, R_DataIndex, L_DataIndex: Integer;
  bolUseStrictValueChecking, bolAddPrefixForEachEyeLine, bolAddVDValueToOutput, bolAddPDValueToOutput: Boolean;
  bolAddSign, bolAddSignSeparator, bolAddAxisSeparator, bolUseStrictValueParsing, bolUseStrictValueParsingPossible: Boolean;
 
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
  
  Result:= Result + S2 + Trim(S1);
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
 
begin

  // Verwenden Sie "True", damit alle Werte auf Richtigkeit und Vollständigkeit
  // geprüft werden. Wählen Sie "False", damit die Prüfung auf die AIS Anwendung
  // verlagert wird.
  bolUseStrictValueChecking:= False;
    
  // Verwende "True", damit vor jeder Zeile (pro Auge) Anzahl x Leerzeichen als Prefix
  // vorangestellt werden, benutze "False", damit dies nicht geschieht.
  bolAddPrefixForEachEyeLine:= False;
    
  // Verwende "True", damit der VD Wert an die V1 Zeile angehängt wird,
  // benutze "False", damit der VD Wert ignoriert werden kann.
  bolAddVDValueToOutput:= False;

  // Verwende "True", damit der PD Wert an die V1 Zeile angehängt wird,
  // benutze "False", damit der PD Wert ignoriert werden kann.
  bolAddPDValueToOutput:= False;
      
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

  // Verwende "True", damit die Felder direkt adressiert werden, d.h. es wird das Feld[5] 
  // direkt gelesen. Benutze "False", damit die Felder nach einander gelesen werden.
  bolUseStrictValueParsing:= True;  
      
  // --- Don't edit script down below ---   
  
  // Clear parsed data string
  FParsedDataString:= '';

  if (Length(FRawDataString) <= 0) then
  begin
    FLastErrorCode:= -2;
    FLastErrorMessage:= 'Keine Daten für die Verarbeitung der Daten gefunden';

    DoPSError();
 
    Exit;
  end;

  // Some firmware versions send \r instead of \r\n, so if no data
  // could be read and converted, change line break config at top of the script
  Sep1:= DATA_SEPARATOR_01;

  Data:= String(FRawDataString);
  
  if (not bolUseStrictValueParsing) then
  begin
    Data:= Trim(Data); 
  end;

  if (bolAddPrefixForEachEyeLine) then
  begin
    strGDT_LINE_PREFIX:= GDT_LINE_PREFIX;
  end;    

  // Get array from 1st separator
  arrData1:= Explode(Sep1, Data, 0);

  if (Length(arrData1) <= 0) then
  begin
    FLastErrorCode:= -3;
    FLastErrorMessage:= 'Kein valides Datenformat (Messdaten): Keine Parameter gefunden';

    DoPSError();

    Exit;
  end;
  
  // Check if we have the field count for parsing directly
  if (bolUseStrictValueParsing) then
  begin 
    if (Length(arrData1) < 10) then
    begin
      bolUseStrictValueParsing:= False;
    end;  
  end;
  
  // T2WMessageBoxS(IntToStr(Length(arrData1)));
  
  // Reset vars
  VD:= '';
  PD:= '';
    
  R_SPH:= '';
  R_CYL:= '';
  R_AXIS:= '';
    
  L_SPH:= '';
  L_CYL:= '';
  L_AXIS:= ''; 

  R_DataIndex:= -1;
  L_DataIndex:= -1;
  
  bolUseStrictValueParsingPossible:= False;
  
  // Parse raw data
  for i:= 0 to Length(arrData1) - 1 do
  begin
    S1:= Trim(arrData1[i]);

    // T2WMessageBoxS(S1);
    // T2WMessageBoxS(IntToStr(i));

    if (S1 = '') then
    begin
      Continue;
    end;
             
    if T2WStartsStr(LINE_IDENTIFIER_R,
                    S1) then
    begin
      R_DataIndex:= i;
      
      if (R_DataIndex = FIELD_NO_LINE_IDENTIFIER_R) then
      begin
        bolUseStrictValueParsingPossible:= True;
      end
      else
      begin
        bolUseStrictValueParsingPossible:= False; 
      end;
           
      Continue;
    end;
    
    if T2WStartsStr(LINE_IDENTIFIER_L,
                    S1) then
    begin
      L_DataIndex:= i;
      R_DataIndex:= -1;
      
      if (L_DataIndex = FIELD_NO_LINE_IDENTIFIER_L) then
      begin
        bolUseStrictValueParsingPossible:= True;
      end
      else
      begin
        bolUseStrictValueParsingPossible:= False; 
      end;   

      Continue;
    end;
    
    // start parsing right eye    

    // R_SPHERE
    if (R_DataIndex >= 0)
    and (R_SPH = '')
    then
    begin 
      if (bolUseStrictValueParsing)
      and (bolUseStrictValueParsingPossible) then
      begin
        if (i <> FIELD_NO_R_SPHERE) then
        begin
          Continue;
        end;
      end;
    
      R_SPH:= S1;
      R_SPH:= Trim(R_SPH);
      R_SPH:= FormatSignValue('S=', R_SPH, bolAddSign, bolAddSignSeparator);
                
      Continue;
    end;

    // R_CYLINDER   
    if (R_DataIndex >= 0)
    and (R_CYL = '')
    then
    begin
      if (bolUseStrictValueParsing)
      and (bolUseStrictValueParsingPossible) then
      begin
        if (i <> FIELD_NO_R_CYLINDER) then
        begin
          Continue;
        end;
      end;
    
      R_CYL:= S1;        
      R_CYL:= Trim(R_CYL);    
      R_CYL:= FormatSignValue('Z=', R_CYL, bolAddSign, bolAddSignSeparator); 
          
      Continue;
    end;
    
    // R_AXIS 
    if (R_DataIndex >= 0)
    and (R_AXIS = '')
    then
    begin
      if (bolUseStrictValueParsing)
      and (bolUseStrictValueParsingPossible) then
      begin
        if (i <> FIELD_NO_R_AXIS) then
        begin
          Continue;
        end;
      end;
    
      R_AXIS:= S1;        
      R_AXIS:= Trim(R_AXIS);      
      R_AXIS:= FormatAxisValue('*', R_AXIS, bolAddAxisSeparator); 
          
      Continue;
    end;
    
    // start parsing left eye 
    
    // L_SPHERE     
    if (L_DataIndex >= 0)
    and (L_SPH = '')
    then
    begin
      if (bolUseStrictValueParsing)
      and (bolUseStrictValueParsingPossible) then
      begin
        if (i <> FIELD_NO_L_SPHERE) then
        begin
          Continue;
        end;
      end;
    
      L_SPH:= S1;        
      L_SPH:= Trim(L_SPH);
      L_SPH:= FormatSignValue('S=', L_SPH, bolAddSign, bolAddSignSeparator);
          
      Continue;
    end;
    
    // L_CYLINDER   
    if (L_DataIndex >= 0)
    and (L_CYL = '')
    then
    begin
      if (bolUseStrictValueParsing)
      and (bolUseStrictValueParsingPossible) then
      begin
        if (i <> FIELD_NO_L_CYLINDER) then
        begin
          Continue;
        end;
      end;
    
      L_CYL:= S1;        
      L_CYL:= Trim(L_CYL);
      L_CYL:= FormatSignValue('Z=', L_CYL, bolAddSign, bolAddSignSeparator); 
          
      Continue;
    end;
    
    // L_AXIS
    if (L_DataIndex >= 0)
    and (L_AXIS = '')
    then
    begin
      if (bolUseStrictValueParsing)
      and (bolUseStrictValueParsingPossible) then
      begin
        if (i <> FIELD_NO_L_AXIS) then
        begin
          Continue;
        end;
      end;
    
      L_AXIS:= S1;        
      L_AXIS:= Trim(L_AXIS);
      L_AXIS:= FormatAxisValue('*', L_AXIS, bolAddAxisSeparator); 
          
      Continue;
    end;
    
    // PD
    if (PD = '')
    and (bolAddPDValueToOutput) then
    begin
      if (bolUseStrictValueParsing)
      and (bolUseStrictValueParsingPossible) then
      begin
        if (i = FIELD_NO_PD) then
        begin
          PD:= S1;
          PD:= 'PD= ' + Trim(PD);
          
          Continue;
        end;
      end;
    end;   
  end;

  // Start parsing ophthalmology data down here  

  // Build result
  ParsedData:= '';
      
  // Add right eye
  R_Line:= '';       
         
  if (R_SPH <> '') 
  or (R_CYL <> '') 
  or (R_AXIS <> '') then
  begin
    R_Line:= R_Line + 'R.:' + R_SPH + ' ' + R_CYL + R_AXIS;
  end;      

  // Add PD
  if (PD <> '') 
  and (bolAddPDValueToOutput) then
  begin
    if (R_Line <> '') then
    begin
      R_Line:= R_Line + ' ';
    end;
    
    R_Line:= R_Line + PD;
  end;
  
  // Add VD
  if (VD <> '') 
  and (bolAddVDValueToOutput) then
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
         
  if (L_SPH <> '') 
  or (L_CYL <> '') 
  or (L_AXIS <> '') then
  begin
    L_Line:= L_Line + 'L.:' + L_SPH + ' ' + L_CYL + L_AXIS;
  end;
  
  // Add PD
  if (PD <> '') 
  and (bolAddPDValueToOutput)
  and (R_Line = '') then
  begin
    if (L_Line <> '') then
    begin
      L_Line:= L_Line + ' ';
    end;
    
    L_Line:= L_Line + PD;
  end;  
  
  // Add VD
  if (VD <> '') 
  and (bolAddVDValueToOutput)
  and (R_Line = '') then
  begin
    if (L_Line <> '') then
    begin
      L_Line:= L_Line + ' ';
    end;
    
    L_Line:= L_Line + VD;
  end;
       
  if (L_Line <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + L_Line + FOutputLineSeparator;
  end;
       
  // Set result
  FParsedDataString:= RawByteString(ParsedData);
end.
