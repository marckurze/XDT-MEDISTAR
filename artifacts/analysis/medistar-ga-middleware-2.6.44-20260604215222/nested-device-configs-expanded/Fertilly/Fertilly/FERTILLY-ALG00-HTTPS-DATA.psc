const
  VERSION = '1.0.5.10';
  DATE = '25.05.2023 14:18:38';
  TEXT = 'Copyright (c) 2023 team2work GmbH';
  
  MIN_MSGA_VERSION_NUMBER = '1.5.39.87';

  // ------------------------------------------

  GDT_IDENTIFIER = 'Fertilly';

  LANGUAGE = 'de'; // Translate data to this language, only 2 char langs are allowed
  
  RESULT_FORMAT = ': '; // Divider char for result key:value pairs
  
  // ------------------------------------------

  JSON_API_CALL_01 = 'cgm/users/?page=0&size=100000';
  JSON_API_CALL_02 = 'cgm/users/<userid>/surveys/?filterCompleted=<filter_completed>&page=0&size=100000&survey_type=BASE_DATA%7CMEDICAL';

  FILE_TRANSLATE_SELECT_OPTIONS = 'FERTILLY-translate_select.lang';
  FILE_TRANSLATE_JSON_KEYS = 'FERTILLY-translate_json_keys.' + LANGUAGE;

  GDT_FID_PATIENT_ID = '3000';

  GDT_FID_SIZE = '3622';
  GDT_FID_WEIGHT = '3623';

  GDT_FID_COUNT_6228_LINES = '6226'; // Number of following continuation lines of the identifier 6228

  GDT_FID_COMMENT = '6227';
  GDT_FID_MEASURE_DATA = '6228';

  GDT_FID_FILE_ARCHIVE_NUMBER = '6302';
  GDT_FID_FILE_FORMAT = '6303';
  GDT_FID_FILE_DESCRIPTION = '6304';
  GDT_FID_FILE_URL = '6305';

  GDT_FID_SIGNATURE = '8990';

  EXTERNAL_FILES_IMAGES_ARCHIVE_NUMBER = 'Bild';
  EXTERNAL_FILES_IMAGES_FORMAT = 'JPG';

  GDT_LINE_PREFIX = '  ';

type
  tJsonType = (tUnknown,tBoolean,tText,tEnum,tDate,tArray);

function CountStringOccurrences(const str: String;
                                const SearchString: String): Integer;
var
  arrayData: TStringArray;
begin
  Result:= 0;
    
  if (str = '') 
  or (SearchString = '') then
    Exit; 

  arrayData:= Explode(SearchString, str, 0);

  Result:= Length(arrayData) - 1;
end;

function GenderJsonValueToGenderGdtValue(const ResValue: String): String;
begin
  Result:= '';
  
  if (UpperCase(ResValue) = 'MALE') then
  begin
    Result:= '1';
  end
  else if (UpperCase(ResValue) = 'FEMALE') then
  begin
    Result:= '2';
  end
  else
  begin
    // do nothing
  end;
end;

function IndexOfArray(const ArrayData: TStringArray; 
                      const SearchString: String): Integer;
var
  index: Integer;
begin  
  Result:= -1;
    
  for index:= Low(ArrayData) To High(ArrayData) do
  begin
    if (SearchString = ArrayData[index]) then
    begin
      Result:= index;
      Break;
    end;
  end;
end;

function IfTranslationIsEmpty(const strResultString: String;
                              const ResStr: String; 
                              const ResValue: String;
                              const strFormat: String): String;
begin                           
  Result:= strResultString;
  
  if (strResultString = '') 
  and (ResStr <> '')
  and (ResValue <> '')
  then
  begin
    Result:= ResStr + strFormat + ResValue;
  end;
end;

function T2WBoolToStr(const ResValue: String;
                      const pLanguage: String): String;
var
  pValue: Boolean;
  ResTrue, ResFalse: String;
begin
  if (ResValue = '') then
    Exit;
  
  try
    pValue:= T2WStrToBool(ResValue);
  except
    Exit;
  end;

  if (LowerCase(pLanguage) = 'de') then
  begin
    ResTrue:= 'ja';
    ResFalse:= 'nein';
  end
  else if (LowerCase(pLanguage) = 'en') then
  begin
    ResTrue:= 'true';
    ResFalse:= 'false';
  end
  else
  begin
    ResTrue:= 'true';
    ResFalse:= 'false';
  end;  

  if (pValue) then
    Result:= ResTrue
  else
    Result:= ResFalse;
end;

function T2WBoolToStrEx(const ResValue: Boolean): String;
begin
  if (ResValue) then
  begin
    Result:= 'true';
  end
  else
  begin
    Result:= 'false';
  end;
end;

function GetValueTranslation(const ResValue: String; 
                             const pType: tJsonType;
                             const pLanguage: String): String;
begin
  Result := ResValue;
  
  case pType of
    tUnknown: begin
      Result:= '';
    end;

    tBoolean: begin
      Result:= T2WBoolToStr(ResValue, pLanguage);
    end;
  
    tText: begin
      Result:= '';
    end;
  
    tEnum: begin
      Result:= '';
    end;
    
    tDate: begin
      if (LowerCase(pLanguage) = 'de') then
        Result:= T2WConvertDateTimeString(ResValue, 'dd.mm.yyyy')
      else
        Result:= '';
    end;
    
    tArray: begin
      Result:= '';
    end;
  end;
end;

function GetArrayDataForTranslation(const JsonKey: String;
                                    const File: TStringList;
                                    const pLanguage: String): String;
var
  iIndex: Integer;
  strHelper, S: String;
begin
  Result:= '';
    
  iIndex:= -1;
    
  strHelper:= pLanguage + '.' + JsonKey;
  
  if (File <> nil) then
  begin
    iIndex:= File.IndexOfName(strHelper);
  end;    

  if (iIndex >= 0) then
  begin
    S:= File.ValueFromIndex[iIndex];
    
    if (S <> '') then
    begin
      S:= T2WStringReplace(S, '[', '', False, False);
      S:= T2WStringReplace(S, ']', '', False, False);
    end;
    
    Result:= S;
  end;    
end;

function GetTranslation(const ResStr: String; 
                        const ResValue: String; 
                        const File: TStringList; 
                        const pType: tJsonType;
                        const strFormat: String;
                        const pLanguage: String;
                        const AddValue: Boolean): String;
var
  iIndex: Integer;
  strHelper: String;
begin  
  iIndex:= -1;
  
  if (File <> nil) then
  begin
    iIndex:= File.IndexOfName(ResStr);
  end;  
  
  if (iIndex >= 0) then
  begin      
    strHelper:= GetValueTranslation(ResValue, pType, pLanguage);
    
    if (strHelper <> '') then
    begin  
      if (AddValue) then
        Result:= File.ValueFromIndex[iIndex] + strFormat + strHelper
      else
        Result:= File.ValueFromIndex[iIndex] + strFormat;   
    end
    else
    begin
      if (AddValue) then
        Result:= File.ValueFromIndex[iIndex] + strFormat + ResValue
      else  
        Result:= File.ValueFromIndex[iIndex] + strFormat; 
    end;   
  end
  else
  begin
    Result:= '';
  end; 
end;

function LoadTranslationFileToList(const Filename: String;
                                   var FileList: TStringList;
                                   const DeleteCommentLines: Boolean): Boolean;
var
  i: Integer;
begin
  Result:= False;
  
  if (FileList = nil) then
    Exit;
    
  FileList.Clear;
  FileList.Sorted:= True;
  FileList.Duplicates:= dupIgnore;

  if not T2WFileExists(FileName) then
    Exit;

  try
    FileList.LoadFromFile(Filename);
  except
    Exit;
  end;
  
  if (DeleteCommentLines) then
  begin
    for i:= 0 to FileList.Count -1 do
    begin
      if T2WStartsStr(';', FileList.Strings[i]) then
      begin
        FileList.Delete(i);
      end;
    end;
  end;
  
  Result:= True;
end;

function GetEnumTranslation(const ResStr: String; 
                            const ResValue: String;
                            const File: TStringList;
                            const pLanguage: String;
                            const TranslatedText: String;
                            const strFormat: String): String;
var
  iIndex, resIndex: Integer;
  strHelper, defValues, langValues: String;
  arrayDataDef, arrayDataLang, arrayData: TStringArray;
begin
  iIndex:= -1;
  
  // Set enum line prefix
  strHelper:= 'default' + '.' + ResStr;
  
  if (File <> nil) then
  begin
    iIndex:= File.IndexOfName(strHelper);
  end;

  if (iIndex >= 0) then
  begin      
    // If default entry is found, we can create the enum array
    defValues:= File.ValueFromIndex[iIndex];
    
    arrayDataDef:= Explode(',', defValues, 0);
    
    // Get the index of the given string from json response
    resIndex:= IndexOfArray(arrayDataDef, ResValue);

    // Load the language file for the given language
    iIndex:= -1;
    
    strHelper:= pLanguage + '.' + ResStr;
    
    if (File <> nil) then
    begin
      iIndex:= File.IndexOfName(strHelper);
    end;
    
    if (iIndex >= 0) then
    begin
      langValues:= File.ValueFromIndex[iIndex];
    
      // If entry is found, we can create the enum array
      arrayDataLang:= Explode(',', langValues, 0);

      if (resIndex <> -1) then
      begin
        if (TranslatedText <> '') then
        begin  
          if T2WContainsStr(TranslatedText, strFormat) then
          begin
            arrayData:= Explode(strFormat, TranslatedText, 0);
              
            if Length(arrayData) = 2 then
            begin
              Result:= arrayData[0] + strFormat + arrayDataLang[resIndex];
            end;
          end
          else
          begin
            Result:= TranslatedText + strFormat + arrayDataLang[resIndex];
          end; 
        end
        else
        begin
          Result:= ResStr + strFormat + arrayDataLang[resIndex];
        end;
      end
      else
      begin
        Result:= '';
      end;
    end
    else
    begin
      Result:= '';
    end;
  end
  else
  begin
    Result:= '';
  end; 
end;

function TranslateKeyValuePair(const JsonKey: String;
                               const JsonValue: String;
                               const pType: tJsonType;
                               const list: TStringList;
                               const enum_list: TStringList;
                               const pLanguage: String;
                               const strFormat: String): String;
var
  strText, strEnumText, S, strHelper, strTranslatedValues: String;
  arrayData, arrayTranslationData, arrayValue: TStringArray;
  i, j: Integer;
begin
  Result:= '';
    
  strText:= '';
  strEnumText:= '';
  strTranslatedValues:= '';

  if (pType = tArray) then
  begin
    // Get translated text
    strText:= GetTranslation(JsonKey, JsonValue, list, pType, strFormat, pLanguage, False);
  end
  else
  begin
    // Get translated text
    strText:= GetTranslation(JsonKey, JsonValue, list, pType, strFormat, pLanguage, True);
  end;

  // Check type
  if (pType = tEnum) then
  begin
    // Get translated text for enum fields (selects, arrays)
    strEnumText:= GetEnumTranslation(JsonKey, JsonValue, enum_list, pLanguage, strText, strFormat);
  end;
  
  if (strEnumText <> '') then
  begin
    Result:= strEnumText;
    Exit;
  end;  
  
  // Check type
  if (pType = tArray) then
  begin
    // load data for translation
    S:= GetArrayDataForTranslation(JsonKey, enum_list, pLanguage);
    
    if (S <> '') then
    begin
      // Load translations into an array
      arrayTranslationData:= Explode(',', S, 0); 
      
      if (Length(arrayTranslationData) > 0) then
      begin
        // Load data to translate into an array
        arrayData:= Explode(',', JsonValue, 0);
      
        if (Length(arrayData) > 0) then
        begin
          for i:= 0 to Length(arrayData) - 1 do
          begin
            strHelper:= arrayData[i];

            for j:= 0 to Length(arrayTranslationData) - 1 do  
            begin
              arrayValue:= Explode(':', arrayTranslationData[j], 0);
              
              if (Length(arrayValue) = 2) then
              begin
                if (strHelper = arrayValue[0]) then
                begin
                  if (strTranslatedValues <> '') then
                  begin
                    strTranslatedValues:= strTranslatedValues + ','; 
                  end;

                  strTranslatedValues:= strTranslatedValues + arrayValue[1];
                end;
              end;
            end;
          end;
        end;
      end;    
    end;
  end;
  
  if (strTranslatedValues <> '') then
  begin
    strText:= strText + strTranslatedValues;
  end;

  // Check possible translated text
  if (strText = '') then
  begin
    strText:= IfTranslationIsEmpty(strText, JsonKey, JsonValue, strFormat);
  end;
    
  Result:= strText;
end;

procedure GetJsonValuesAndSetGdtOutputLine(const FullJsonKey: String;
                                          const RawJsonKey: String;
                                          const pType: tJsonType;
                                          const list: TStringList;
                                          const enum_list: TStringList;
                                          const pLanguage: String;
                                          const strFormat: String;
                                          var ParsedData: String);
var
  S1, S2: String;
begin
  //  Get value from json response
  S1:= DoHTTPSPSGetJSONValueS(FullJsonKey);
    
  // Check type
  if (pType = tArray) then
  begin
    S1:= T2WStringReplace(S1, '"', '', True, False);
    S1:= T2WStringReplace(S1, '[', '', False, False);
    S1:= T2WStringReplace(S1, ']', '', False, False);
    S1:= T2WStringReplace(S1, '\/', '/', False, False);
  end;

  //  Get new translated value
  S2:= TranslateKeyValuePair(RawJsonKey, S1, pType, list, enum_list, pLanguage, strFormat);

  // Generate GDT output here
  if (S2 <> '') then
  begin
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + S2 + FOutputLineSeparator;
  end;
end;

function GetErrorMessage(const ErrorCode: Integer; 
                         const File: TStringList): String;
var
  iIndex: Integer;
begin  
  iIndex:= -1;
  
  if (File <> nil) then
  begin
    iIndex:= File.IndexOfName(IntToStr(ErrorCode));
  end;  
  
  if (iIndex >= 0) then
  begin
    Result:= File.ValueFromIndex[iIndex];
  end;
end;

var
  bolAddExternalFilesImagesToGdtFile, bolfilterCompletedSurveys, bolAddGDTLinePrefix: Boolean;
  Images: String;
  S, S1, S2, ParsedData, UserID, UserSEX, PatientID, AppScriptPath, ParsedDataString: String;
  PatientData: TStringArray;
  i, totalElements, intGdtLinesCount: Integer;
  bolGdtPatientDataFound: Boolean;
  list, enum_list: TStringList;
begin
  // Verwende "True", damit nur vollständige Umfragen (Surveys) verwendet werden, 
  // benutze "False", damit alle Umfragen (Surveys) verarbeitet werden.
  bolfilterCompletedSurveys:= True;

  // Aktiviere den Import aller externen Bildquellen.
  // Verwende "True", damit der Import aktiviert ist, benutze "False", damit Bilder
  // nicht importiert und per GDT weiterverarbeitet werden.
  bolAddExternalFilesImagesToGdtFile:= False;

  // Verwende "True", damit jeder GDT Zeile das Prefix aus GDT_LINE_PREFIX 
  // vorangestellt wird, benutze "False", damit das GDT-Zeilen-Prefix ignoriert wird
  bolAddGDTLinePrefix:= False;    

  // --- Don't edit script down below ---

  (*
  if CheckMSGAVersionNumber(MIN_MSGA_VERSION_NUMBER) then
  begin
    T2WMessageBoxS('Versionsnummer passt.');
  end
  else
  begin
    T2WMessageBoxS('Versionsnummer passt nicht.');
  end;
  *)

  // Clear parsed data string
  FHTTPSDataString:= '';
  
  // Get all user data with "cgm/users/"
  DoHTTPSPSGetRequest(JSON_API_CALL_01);
  
  // T2WMessageBoxS(FHTTPSBufferString);
  
  // Get patient data from GDT file
  DoHTTPSPSGetGDTPatientData(PatientData);
  
  // T2WMessageBoxS(PatientData[0]);
  
  // GDT-Datenschlüssel
  // 0  = Nachname
  // 1  = Vorname
  // 2  = Geburtsdatum
  // 3  = Titel (Anrede)
  // 4  = Versicherungsnummer
  // 5  = Ort
  // 6  = Anschrift
  // 7  = Versicherungsart
  // 8  = Geschlecht
  // 9  = Größe
  // 10 = Gewicht
  // 11 = Ethnische Gruppe
  
  if (Length(PatientData) = 0) then
  begin
    FLastHTTPSErrorCode:= -3;
    FLastHTTPSErrorMessage:= 'Keine GDT Patienten-Daten für die Verarbeitung verfügbar';

    DoHTTPSPSError;

    Exit;
  end;

  // Get the items count
  S:= DoHTTPSPSGetJSONValueS('totalElements');
    
  if (S <> '') then
  begin
    try
      totalElements:= StrToInt(S);
    except
      totalElements:= 0;
    end;    
  end
  else
  begin
    totalElements:= 0;
  end;
 
  bolGdtPatientDataFound:= False;
    
  UserID:= '';
  UserSEX:= '';

  // loop through web api data, find the selected gdt user,
  // Compare gdt patient data with json result data from web call
  for i:= 0 to totalElements - 1 do
  begin
    S:= DoHTTPSPSGetJSONValueS('items[' + IntToStr(i) + '].person.data.last_name');
  
    if (S = PatientData[0]) then
    begin
      S:= DoHTTPSPSGetJSONValueS('items[' + IntToStr(i) + '].person.data.first_name');  
 
      if (S = PatientData[1]) then
      begin
        S:= DoHTTPSPSGetJSONValueS('items[' + IntToStr(i) + '].person.data.sex');

        UserSEX:= UpperCase(S);
         
        S:= GenderJsonValueToGenderGdtValue(UserSEX);
        
        if (S = PatientData[8]) then
        begin
          S:= DoHTTPSPSGetJSONValueS('items[' + IntToStr(i) + '].person.data.birthday');

          S := T2WConvertDateTimeString(S, 'ddmmyyyy');

          if (S = PatientData[2]) then
          begin
            UserID:= DoHTTPSPSGetJSONValueS('items[' + IntToStr(i) + '].id');
          
            if (UserID <> '') then
            begin
              bolGdtPatientDataFound:= True;
            end;
          end;
        end;
      end;
    end;
  end;

  // If the selected / exported patient does not matches any json data item
  if (not bolGdtPatientDataFound) then
  begin
    FLastHTTPSErrorCode:= -4;
    FLastHTTPSErrorMessage:= 'Keine passenden Patienten-Daten für die Verarbeitung verfügbar';

    DoHTTPSPSError;

    Exit;
  end;
  
  if (UserID = '') then
  begin
    FLastHTTPSErrorCode:= -5;
    FLastHTTPSErrorMessage:= 'Keine passenden Patienten-Daten für die Verarbeitung verfügbar';

    DoHTTPSPSError;

    Exit;
  end;
  
  if (UserSEX = '') then
  begin
    FLastHTTPSErrorCode:= -6;
    FLastHTTPSErrorMessage:= 'Keine passenden Patienten-Daten für die Verarbeitung verfügbar';

    DoHTTPSPSError;

    Exit;
  end;  
  
  // Create dynamically the next web api call
  S:= JSON_API_CALL_02;
  S:= T2WStringReplace(S, '<userid>', UserID, False, False);
  S:= T2WStringReplace(S, '<filter_completed>', T2WBoolToStrEx(bolfilterCompletedSurveys), False, False);

  // Get all user surveys with "cgm/users/<userid>/surveys/"
  DoHTTPSPSGetRequest(S);
  
  // T2WMessageBoxS(FHTTPSBufferString);
  
  // Get the items count
  S:= DoHTTPSPSGetJSONValueS('totalElements');
  
  if (S <> '') then
  begin
    try
      totalElements:= StrToInt(S);
    except
      totalElements:= 0;  
    end;  
  end
  else
  begin
    totalElements:= 0;
  end;
  
  if (totalElements = 0) then
  begin
    FLastHTTPSErrorCode:= -7;
    FLastHTTPSErrorMessage:= 'Keine Umfragen-Daten für die Verarbeitung verfügbar';  
  
    DoHTTPSPSError;

    Exit;
  end;  
  
  // Load translate files to the internal lists
  list:= TStringList.Create();
  enum_list:= TStringList.Create();  

  try
    // Get current absolute script path
    AppScriptPath:= DoHTTPSPSGetPSScriptPath();

    // Load language list file
    if (not LoadTranslationFileToList(AppScriptPath + FILE_TRANSLATE_JSON_KEYS, list, True)) then
    begin
      FLastHTTPSErrorCode:= -8;
      FLastHTTPSErrorMessage:= 'Keine Sprach-Daten für die Verarbeitung verfügbar';

      DoHTTPSPSError;

      Exit;
    end;
         
    // Load enum list file
    if (not LoadTranslationFileToList(AppScriptPath + FILE_TRANSLATE_SELECT_OPTIONS, enum_list, True)) then
    begin
      FLastHTTPSErrorCode:= -9;
      FLastHTTPSErrorMessage:= 'Keine erweiterten Sprach-Daten für die Verarbeitung verfügbar';

      DoHTTPSPSError;

      Exit;
    end;
    
    // Patient ID
    PatientID:= '';
      
    // patient id not given in json web data

    if (PatientID <> '') then
    begin
      PatientID:= PatientID + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_PATIENT_ID + FOutputLineSeparator;
      PatientID:= PatientID + FOutputLineSeparator;
    end;
 	
    // Add patient ID
    if (PatientID <> '') then
    begin
      ParsedData:= ParsedData + PatientID;
    end;
    
    // Build result
    ParsedData:= '';    
    ParsedDataString:= '';
    
    intGdtLinesCount:= 0;
    
    // Add a simple data identifier
    ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
    ParsedData:= ParsedData + GDT_IDENTIFIER + FOutputLineSeparator;

    // Parse json web api data and set gdt output
    for i:= 0 to totalElements - 1 do
    begin
      // Check gender value for selected patient
      if (UserSEX = 'MALE') then
      begin
        // Add here all gender specific keys for male   
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.previous_pregnancy', 'previous_pregnancy', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.previous_pregnancy_w_current_partner', 'previous_pregnancy_w_current_partner', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.date_previous_pregnancy', 'date_previous_pregnancy', tDate, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.weight_in_kg', 'weight_in_kg', tText, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.height_in_cm', 'height_in_cm', tText, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.medication', 'medication', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.which_medication', 'which_medication', tText, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.allergies', 'allergies', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.which_allergies', 'which_allergies', tText, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.undescended_testicles', 'undescended_testicles', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.undescended_testicles_therapy', 'undescended_testicles_therapy', tEnum, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);

        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.genital_inflammation_injury_tumors', 'genital_inflammation_injury_tumors', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.which_genital_inflammation_injury_tumors', 'which_genital_inflammation_injury_tumors', tText, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.genital_inflammation_injury_tumors_date', 'genital_inflammation_injury_tumors_date', tDate, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.genital_inflammation_injury_tumors_therapy', 'genital_inflammation_injury_tumors_therapy', tText, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.varicocele', 'varicocele', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.varicocele_surgery', 'varicocele_surgery', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.varicocele_surgery_date', 'varicocele_surgery_date', tDate, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.abdominal_surgery', 'abdominal_surgery', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.which_abdominal_surgery', 'which_abdominal_surgery', tText, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.abdominal_surgery_date', 'abdominal_surgery_date', tDate, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.alcohol_consumption', 'alcohol_consumption', tEnum, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.cigarette_consumption', 'cigarette_consumption', tEnum, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.drug_use', 'drug_use', tEnum, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.psychotherapy', 'psychotherapy', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.satisfied_w_sexuality', 'satisfied_w_sexuality', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.semen_analysis', 'semen_analysis', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.semen_analysis_date', 'semen_analysis_date', tDate, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
      end
      else if (UserSEX = 'FEMALE') then
      begin
        // Add here all gender specific keys for female
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.want_child', 'want_child', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.want_child_since_year', 'want_child_since_year', tText, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.want_child_since_month', 'want_child_since_month', tText, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.married', 'married', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.cycle_duration', 'cycle_duration', tText, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.period_duration', 'period_duration', tText, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.date_of_last_period', 'date_of_last_period', tDate, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.weight_in_kg', 'weight_in_kg', tText, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.height_in_cm', 'height_in_cm', tText, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
  
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.alcohol_consumption', 'alcohol_consumption', tEnum, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.cigarette_consumption', 'cigarette_consumption', tEnum, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.drug_use', 'drug_use', tEnum, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.previous_pregnancy', 'previous_pregnancy', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.previous_pregnancy_year', 'previous_pregnancy_year', tText, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.previous_pregnancy_outcome', 'previous_pregnancy_outcome', tEnum, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.abdominal_operations', 'abdominal_operations', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.which_abdominal_operation', 'which_abdominal_operation', tText, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.abdominal_operation_date', 'abdominal_operation_date', tDate, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);

        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.medical_conditions', 'medical_conditions', tArray, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);

        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.allergies', 'allergies', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.which_allergies', 'which_allergies', tText, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.medication', 'medication', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.which_medication', 'which_medication', tText, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.mmr_vaccination', 'mmr_vaccination', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.covid_vaccination', 'covid_vaccination', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.previous_ART', 'previous_ART', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.previous_ART_date', 'previous_ART_date', tDate, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.previous_ART_clinic', 'previous_ART_clinic', tText, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.previous_ART_type', 'previous_ART_type', tText, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.no_retrieved', 'no_retrieved', tText, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.no_fertilized', 'no_fertilized', tText, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.no_transferred', 'no_transferred', tText, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.pregnancy_from_ART', 'pregnancy_from_ART', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.outcome_pregnancy_from_ART', 'outcome_pregnancy_from_ART', tEnum, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.psychotherapy', 'psychotherapy', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
        GetJsonValuesAndSetGdtOutputLine('items[' + IntToStr(i) + '].survey.data.satisfied_w_sexuality', 'satisfied_w_sexuality', tBoolean, list, enum_list, LANGUAGE, RESULT_FORMAT, ParsedData);
      end
      else
      begin
        // do nothing
      end;
    end;
    
    // get GDT_FID_MEASURE_DATA lines count
    intGdtLinesCount:= CountStringOccurrences(ParsedData, GDT_FID_MEASURE_DATA);

    if (intGdtLinesCount > 0) then
    begin
      // TODO:
    end;

    // Add other data to gdt output
    if (ParsedDataString <> '') then
    begin
      ParsedData:= ParsedData + (FOutputGDTIdentifier + FOutputGDTValueSeparator) + GDT_FID_MEASURE_DATA + FOutputLineSeparator;
      ParsedData:= ParsedData + ParsedDataString + FOutputLineSeparator;
    end;

    // Add images and other files to gdt output
    if (bolAddExternalFilesImagesToGdtFile) then
    begin
      Images:= '';
    
      // TODO:
    
      ParsedData:= ParsedData + Images;
    end;

    // Set output
    FHTTPSDataString:= RawByteString(ParsedData);
  finally
    list.Free();
    enum_list.Free();
     
    list := nil;
    enum_list := nil;
  end;
end.
