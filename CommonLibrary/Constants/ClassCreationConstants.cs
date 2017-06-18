using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLibrary.Constants
{
    public static class ClassCreationConstants
    {
        public const string DATA = "Data";
        public const string BUSINESS = "Business";
        public const string BO = "Bo";
        public const string DOT_OPERATOR = ".";
        public const string DTO = "Dto";
        public const string CS = "cs";
        public const string LIST = "List";
        public const string BASE = "Base";
        public const string DATABASE = "Database";
        public const string SPROC_DATA_LAYER_GENERATOR = "SprocDataLayerGenerator";
        public const string READER = "reader";
        public const string BASE_DATA_ACCESS = "BaseDataAccess";
        public const string BASE_BUSINESS = "BaseBusiness";
       
        public const string ADD_ITEMS_TO_LIST_BY_READER = "AddItemsToListBySqlDataReader";
        public const string SINGLE_TABLE = "SingleTable";
        public const string SPROC_TABLE = "SprocTable";
        public const string RESULT = "Result";
        public const string INPUT = "Input";
        public const string ACCESSOR = "Accessor";
        public const string CUSTOM_SPROC = "CustomSproc";

        public const string SYSTEM = "System";
        public const string COLLECTIONS = "Collections";
        public const string GENERIC = "Generic";
        public const string TEXT = "Text";
        public const string SQLCLIENT = "SqlClient";
        public const string COMMONLIBRARY = "CommonLibrary";
        public const string REFLECTION = "Reflection";

        public const string UNDERSCORE = "_";

        public const string YES = "YES";
        public const string NO = "NO";
        public const string RESOLVE_DUPLICATE_CLASS_AND_PROPERTY_NAME = "_Property";

        public const string QUOTE = "\"";
        public const string EQUALS = "=";
        public const string SPACE = " ";
        public const string CONDITION_OPEN_BRACKET = "(";
        public const string CONDITION_CLOSE_BRACKET = ")";
        public const string CSHARP_OPEN_BRACE = "{";
        public const string CSHARP_CLOSE_BRACE = "}";
        public const string COMMA = ",";
        public const string SEMI_COLON = ";";
        public const string CSHARP_SQUARE_OPEN_BRACKET = "[";
        public const string CSHARP_SQUARE_CLOSE_BRACKET = "]";
        public const string CSHARP_OPEN_ANGLE_BRACKET = "<";
        public const string CSHARP_CLOSE_ANGLE_BRACKET = ">";


        public const string NULL = "null";
        public const string THIS = "this";
        public const string ADD = "Add";
        public const string CLEAR = "Clear";
        public const string COUNT = "Count";

        public const string BASE_DATABASE_VARIABLE_NAME = "_baseDatabase";
        public const string READER_VARIABLE_NAME = "reader";
        public const string DATASET_VARIABLE_NAME = "ds";
        public const string GET_ORDINAL_READER_METHOD_NAME = "GetOrdinal";
        public const string NULLABLE_TYPE_READER_METHOD_RETURNS_OBJECT = "retrieveNullableTypeFromDataReader";
        public const string NULLABLE_IMAGE_READER_METHOD_RETURNS_IMAGE = "retrieveNullableImageFromDataReader";
        public const string TAB = "    ";
        public const string USING = "using";

        public const string VAR_SPROC_NAME = "sprocName";
        public const string VAR_CONNECTION_STRING_NAME = "connectionString";
        public const string VAR_INPUT_OBJECT_NAME = "inputObject";
        public const string VAR_STORED_PROCEDURE_PARAMETER_ARRAY = "paramArray";
        public const string VAR_STORED_PROCEDURE_PARAMETER = "parameter";
        public const string VAR_DATASET = "ds";

        public const string SQLPARAMETER = "SqlParameter";
        public const string NEW = "new";

        public const string GET_DATAREADER_FROM_SP = "getDataReaderFromSP";
        public const string EXECUTE_NON_QUERY_STORED_PROCEDURE = @"executeNonQueryStoredProcedure";

        public const string SQLDATAREADER = "SqlDataReader";
        public const string GET_DATASET_FROM_SP = "getDatasetFromSP";
        public const string DATASET = "DataSet";
        public const string RETURN_LIST = "returnList";
        public const string RETURN_STATEMENT = "return";

        public const string STRING = "string";
        public const string VALUE = "Value";

        public const string GET_VALUE_FROM_INPUT_OBJECT_FOR_SPROC_PARAMETER = "GetValueFromInputObjectForSprocParameter";

        public const string FILL_DB_SETTINGS_EXCEPTION_VAR_NAME = @"FILL_DB_SETTINGS_EXCEPTION";
        public const string PRIMARY_KEY_NOT_FOUND_EXCEPTION_VAR_NAME = @"PRIMARY_KEY_NOT_FOUND_EXCEPTION_VAR_NAME";
        public const string FILL_DB_SETTINGS_EXCEPTION_TEXT = @"Please Fill the DatabaseSmoObjectsAndSettings_Property with a filled DatabaseSmoObjectsAndSettings class and try again";

        public const string DATABASE_SMO_OBJECTS_AND_SETTINGS_VAR_NAME = @"_databaseSmoObjectsAndSettings";
        public const string BASE_DATA_ACCESS_VAR_NAME = @"_baseDataAccess";
        public const string DATABASE_SMO_OBJECTS_AND_SETTINGS_PARAMETER_NAME = "databaseSmoObjectsAndSettings";
        public const string FILLED_BO_PARAMETER_NAME = "filledBo";
        public const string FILLED_DTO_PARAMETER_NAME = "filledDto";
        public const string DTO_PARAMETER_NAME = "dto";
        public const string BO_PARAMETER_NAME = "bo";

        public const string SYSTEM_APPLICATION_EXCEPTION = "System.ApplicationException";


        public const string FILL_BY_GET_PERMUTATION_METHOD_NAME = "FillByGetPermutation";
        public const string FILL_BY_PRIMARY_KEY = "FillByPrimaryKey";
        public const string FILL_BY_CRITERIA_FUZZY = "FillByCriteriaFuzzy";
        public const string FILL_BY_CRITERIA_EXACT = "FillByCriteriaExact";
        public const string FILL_BY_GET_ALL = "FillByGetAll";

        public const string FILL_PROPERTIES_FROM_BO_METHOD_NAME = "FillPropertiesFromBo";
        public const string FILL_THIS_WITH_DTO_METHOD_NAME = "FillThisWithDto";
        public const string FILL_DTO_WITH_THIS_METHOD_NAME = "FillDtoWithThis";
        public const string INSERT_METHOD_NAME = "Insert";
        public const string UPDATE_METHOD_NAME = "Update";
        public const string DELETE_METHOD_NAME = "Delete";
        public const string GET_BY_PRIMARY_KEY_METHOD_NAME = "GetByPrimaryKey";
        public const string GET_METHOD_NAME = "Get";
        public const string BASE_DATA_ACCESS_AVAILABLE_METHOD_NAME = "BaseDataAccessAvailable";

        public const string BASE_BUSINESS_VAR_NAME = "_baseBusiness";
        public const string BASE_DATA_ACCESS_AVAILABLE_VAR = "baseDataAccessAvailable";

        public const string RETURN_DTO_VAR = "returnDto";
        public const string BASE_DATABASE_AVAILABLE_BOOL_VAR = "isBaseDatabaseAvailable";
        public const string IS_MODIFIED_DICTIONARY_PROPERTY_NAME = "IsModifiedDictionary";

        public const string INITIALIZE_IS_MODIFIED_DICTIONARY_METHOD_NAME = "InitializeIsModifiedDictionary";
        public const string SET_IS_MODIFIED_METHOD_NAME = "SetIsModified";
        public const string COLUMN_NAME_PARAMETER_VAR = "columnName";
        public const string GET_PERMUTATION_PARAMETER_VAR = "getPermutation";

        public const string CONTROL_VAR = "control";
        public const string COUNTER_VAR = "counter";
        public const string BO_TO_FILL_VAR = "boToFill";
       

    }
}
