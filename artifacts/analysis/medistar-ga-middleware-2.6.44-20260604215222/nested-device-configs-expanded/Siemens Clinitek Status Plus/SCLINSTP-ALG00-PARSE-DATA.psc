const
  VERSION = '1.0.2.11';
  DATE = '13.03.2026 14:33:12';
  TEXT = 'Copyright (c) 2026 CompuGroup Medical Deutschland AG';

  LINE_SEPARATOR = ',';

  GDT_FID_PATIENT_ID = '3000';
  GDT_FID_MEASURE_DATA = '6228';
  GDT_FID_COMMENT = '6227';
  GDT_FID_FILE_ARCHIVE_NUMBER = '6302';
  GDT_FID_FILE_FORMAT = '6303';
  GDT_FID_FILE_DESCRIPTION = '6304';
  GDT_FID_FILE_URL = '6305';
  
var
  arrData: TStringArray;
  ParsedData, Data, PatientID, S: String;
  Line_1, Line_2, Line_3: String;
  bolUseStrictValueChecking, bolDataError: Boolean;
  SequenceNo, DateVal, TimeVal, OperatorID, MeasuringStripType, Color, Clarity: String;
  Glukose, Bilirubin, Ketone, SpecificGravity, OccultBlood, pHValue, Protein: String;
  Urobilinogen, Nitrit, Leukozyten: String;
  i: Integer;

function GetDataFromArray(const arrData: TStringArray; 
                          const intIndex: Integer): String;
var  
  strReturnValue: String;
begin
  strReturnValue:= '';

  if (Length(arrData) > 0) then  
  begin
    try
      strReturnValue:= arrData[intIndex - 1];  
    except
      strReturnValue:= ''; 
    end;
  end;

  Result:= strReturnValue;  
end;

function GetParameterBlockDataFromArray(const arrData: TStringArray; 
                                        const intIndex: Integer; 
                                        const intValueIndex: Integer;
                                        const intAddOnIndex: Integer): String;
var  
  strReturnValue: String;
begin
  strReturnValue:= '';

  if (Length(arrData) > 0) then  
  begin
    try
      strReturnValue:= arrData[intIndex - 1];
      strReturnValue:= strReturnValue + '=' + arrData[intValueIndex - 1] + ';';
    except
      strReturnValue:= ''; 
    end;
  end;

  Result:= strReturnValue;  
end;

begin

  // Verwenden Sie "True", damit alle Werte auf richtigkeit und Vollständigkeit
  // geprüft werden. Wählen Sie "False", damit die Prüfung auf die AIS Anwendung
  // verlagert wird.
  bolUseStrictValueChecking:= False; 

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

  // T2WMessageBoxS(FRawDataString);
  
  bolDataError:= False;

  PatientID:= '';  

  Data:= String(FRawDataString);
  Data:= Trim(Data);

  // Get array of lines
  arrData:= Explode(LINE_SEPARATOR, Data, 0);

  if (Length(arrData) <= 0) then
  begin
    FLastErrorCode:= -3;
    FLastErrorMessage:= 'Keine Zeilen für die Verarbeitung verfügbar';

    DoPSError();

    Exit;
  end;
    
  // Loop through data, to check for some errors
  for i:= 0 to Length(arrData) - 1 do
  begin   
    S:= arrData[i];
    S:= Trim(S);
    
    // T2WMessageBoxS(S);

    if (S = '') then
    begin
      Continue;
    end;
    
    // TODO: Error handling 
    
    if (T2WContainsStr(S, 'ERR')) then
    begin    
      bolDataError:= True;
    end  
    else if (T2WContainsStr(S, 'ERROR')) then
    begin    
      bolDataError:= True;
    end;  
  end;
  
  if (bolDataError) then
  begin
    // TODO: 
  end;
  
  // Farbe=Keine Angabe; Klarheit=Keine Angabe; GLU=Negativ;
  // BIL=Negativ; KET=Negativ; SG=1.020; OBL=0 Ery/uL; pH=6.0;
  // PRO=Negativ; UBG=3.2 umol/L; NIT=Negativ; LEU=0 Leu/uL;
  
  // Error code if something is wrong with the test strip:
  // Farbe=Keine Angabe; Klarheit=Keine Angabe; E69=;
  // Farbe=Keine Angabe; Klarheit=Keine Angabe; E58=;
    
  // Get data from array, we use as index the real field number, not the array index
  SequenceNo:= GetDataFromArray(arrData, 1);
  DateVal:= GetDataFromArray(arrData, 2);  
  TimeVal:= GetDataFromArray(arrData, 3); 
  
  // 4 empty
  
  OperatorID:= GetDataFromArray(arrData, 5); // or PatentID???
  
  // 6 empty
  // 7 empty
  // 8 empty
  // 9 empty
  
  MeasuringStripType:= GetDataFromArray(arrData, 10); 
  Color:= GetDataFromArray(arrData, 11); 
  Clarity:= GetDataFromArray(arrData, 12);

  // Parameter block (each group of three: abbreviation, value, blank)
  // Starting in field 13, the data continues in groups of three:

  Glukose:= GetParameterBlockDataFromArray(arrData, 13, 14, 15); 
  Bilirubin:= GetParameterBlockDataFromArray(arrData, 16, 17, 18); 
  Ketone:= GetParameterBlockDataFromArray(arrData, 19, 20, 21); 
  SpecificGravity:= GetParameterBlockDataFromArray(arrData, 22, 23, 24); 
  OccultBlood:= GetParameterBlockDataFromArray(arrData, 25, 26, 27);  
  pHValue:= GetParameterBlockDataFromArray(arrData, 28, 29, 30);   
  Protein:= GetParameterBlockDataFromArray(arrData, 31, 32, 33);
  Urobilinogen:= GetParameterBlockDataFromArray(arrData, 34, 35, 36);
  Nitrit:= GetParameterBlockDataFromArray(arrData, 37, 38, 39);
  Leukozyten:= GetParameterBlockDataFromArray(arrData, 40, 41, 42);  

  // Fields 43–50 empty

  // Build output
  ParsedData:= '';

  // Add patient ID
  if (PatientID <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
    ParsedData:= ParsedData + PatientID + FOutputLineSeparator;
  end;
  
  // Build vars
  if (Color <> '') then
  begin
    Color:= 'Farbe=' + Color + ';';
  end;
  
  if (Clarity <> '') then
  begin
    Clarity:= 'Klarheit=' + Clarity + ';';
  end;
  
  // Format output
  
  // Farbe=Keine Angabe; Klarheit=Keine Angabe; GLU=Negativ;
  Line_1:= Color + ' ' + Clarity + ' ' + Glukose;
  
  // BIL=Negativ; KET=Negativ; SG=1.020; OBL=0 Ery/uL; pH=6.0; 
  Line_2:= Bilirubin + ' ' + Ketone + ' ' + SpecificGravity + ' ' + OccultBlood + ' ' + pHValue;

   // PRO=Negativ; UBG=3.2 umol/L; NIT=Negativ; LEU=0 Leu/uL; 
   Line_3:= Protein + ' ' + Urobilinogen + ' ' + Nitrit + ' ' + Leukozyten;

  // Farbe=Keine Angabe; Klarheit=Keine Angabe; GLU=Negativ; 
  if (Line_1 <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + Trim(Line_1) + FOutputLineSeparator;
  end;

  // BIL=Negativ; KET=Negativ; SG=1.020; OBL=0 Ery/uL; pH=6.0;
  if (Line_2 <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + Trim(Line_2) + FOutputLineSeparator;
  end;

  // PRO=Negativ; UBG=3.2 umol/L; NIT=Negativ; LEU=0 Leu/uL;
  if (Line_3 <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + Trim(Line_3) + FOutputLineSeparator;
  end;

  // Set output
  FParsedDataString:= RawByteString(ParsedData);
end.
