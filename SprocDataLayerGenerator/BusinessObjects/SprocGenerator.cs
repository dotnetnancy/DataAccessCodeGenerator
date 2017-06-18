using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Text;

using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Server;

using SprocDataLayerGenerator.Data;
using SprocDataLayerGenerator.Data.Access;
using SprocDataLayerGenerator.Sproc.Mapping;
using SqlDbTypeConstants = CommonLibrary.Constants.SqlDbConstants;
using SqlNativeTypeConstants = CommonLibrary.Constants.NativeSqlConstants;
using TableConstraintTypeConstants = SprocDataLayerGenerator.Constants.TableConstraintTypesConstants;
using DbHelper = CommonLibrary.Utility.DatabaseHelperMethods;
using Sql = CommonLibrary.Constants.SqlConstants;
using StringHelper = CommonLibrary.Utility.StringManipulation;

namespace SprocDataLayerGenerator.BusinessObjects
{

    public class SprocGenerator
    {      


        #region private member variables

        private string _databaseName = string.Empty;
        private string _dataSource = string.Empty;
        private string _initialCatalog = string.Empty;
        private string _userId = string.Empty;
        private string _password = string.Empty;
        private bool _trustedConnection = false;
        private string _connectionString = string.Empty;

        private List<MetaInformationSchema> _metaDataList = new List<MetaInformationSchema>();
        private List<StoredProcedure> _listOfGeneratedStoredProcedures = new List<StoredProcedure>();
        SqlConnection _connection = null;
        ServerConnection _serverConnection = null;
        Server _server = null;
        Database _db = null;
        private string _schema = string.Empty;


        public Database MyDatabase
        {
            get { return _db; }
            set { _db = value; }
        }
        public Server MyServer
        {
            get { return _server; }
            set { _server = value; }
        }
        public ServerConnection MyServerConnection
        {
            get { return _serverConnection; }
            set { _serverConnection = value; }
        }
        public SqlConnection Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }
        public List<StoredProcedure> ListOfGeneratedStoredProcedures
        {
            get { return _listOfGeneratedStoredProcedures; }
            set { _listOfGeneratedStoredProcedures = value; }
        }
        public List<MetaInformationSchema> MetaDataList
        {
            get { return _metaDataList; }
            set { _metaDataList = value; }
        }

        #endregion


        public SprocGenerator(string databaseName,
                              string dataSource,
                              string initialCatalog,
                              string userId,
                              string password,
                              bool trustedConnection,
                              string schema)
        {
            _databaseName = databaseName;
            _dataSource = dataSource;
            _initialCatalog = initialCatalog;
            _userId = userId;
            _password = password;
            _trustedConnection = trustedConnection;
            _schema = schema;

            _connectionString = BuildConnectionStringFillLocalDBSettings(databaseName,
                                                          dataSource,
                                                          initialCatalog,
                                                          userId,
                                                          password,
                                                          trustedConnection);

            MetaInformationSchemaManager metaInformationManager =
                new MetaInformationSchemaManager(databaseName,
                                                 dataSource,
                                                 initialCatalog,
                                                 userId,
                                                 password,
                                                 trustedConnection,                                                 
                                                 schema);

            _metaDataList = metaInformationManager.MetaDataList;
        }

        public SprocGenerator(string databaseName,
                      string dataSource,
                      string initialCatalog,
                      string userId,
                      string password,
                      bool trustedConnection,
                      string tableName,
                      string schema)
        {
            _databaseName = databaseName;
            _dataSource = dataSource;
            _initialCatalog = initialCatalog;
            _userId = userId;
            _password = password;
            _trustedConnection = trustedConnection;
            _schema = schema;

            _connectionString = BuildConnectionStringFillLocalDBSettings(databaseName,
                                                        dataSource,
                                                        initialCatalog,
                                                        userId,
                                                        password,
                                                        trustedConnection);

            MetaInformationSchemaManager metaInformationManager =
                new MetaInformationSchemaManager(databaseName,
                                                 dataSource,
                                                 initialCatalog,
                                                 userId,
                                                 password,
                                                 trustedConnection,
                                                 tableName,
                                                 schema);

            _metaDataList = metaInformationManager.MetaDataList;
        }

        public string BuildConnectionStringFillLocalDBSettings(string databaseName,
                                                        string dataSource,
                                                        string initialCatalog,
                                                        string userId,
                                                        string password,
                                                        bool trustedConnection)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.DataSource = dataSource;
            builder.InitialCatalog = initialCatalog;
            builder.UserID = userId;
            builder.Password = password;
            builder.IntegratedSecurity = trustedConnection;

            _connection = new SqlConnection(builder.ConnectionString);
            _serverConnection = new ServerConnection(_connection);
            _server = new Server(_serverConnection);           
            _db = _server.Databases[databaseName];    

            string connectionString = builder.ConnectionString;
            return connectionString;

        }

        public List<StoredProcedure> GetStoredProcedures(List<MetaInformationSchema> schemasToCreate)
        {
            GenerateStoredProcedures(schemasToCreate);
            return _listOfGeneratedStoredProcedures;
        }     

        public List<StoredProcedure> GetStoredProcedures()
        {
            GenerateStoredProcedures();
            return _listOfGeneratedStoredProcedures;
        }

        //will generate for all columns that exist in the table, and all tables
        public void GenerateStoredProcedures()
        {
            GenerateStoredProcedures(_metaDataList);           
        }

        //will generate for all tables included in the set and all columns for those tables
        public void GenerateStoredProcedures(List<MetaInformationSchema> schemasToCreate)
        {
            Dictionary<MetaInformationSchema, List.InformationSchemaColumn> schemaToColumnsDictionary =
                new Dictionary<MetaInformationSchema, List.InformationSchemaColumn>();

            foreach (MetaInformationSchema schema in schemasToCreate)
            {
                schemaToColumnsDictionary.Add(schema, schema.MetaColumns);
            }
            GenerateStoredProcedures(schemaToColumnsDictionary);
        }

        public void GenerateStoredProcedures(Dictionary<MetaInformationSchema, List.InformationSchemaColumn> schemaToColumnsDictionary)
        {
            foreach (KeyValuePair<MetaInformationSchema, List.InformationSchemaColumn> kvp in schemaToColumnsDictionary)
            {
                GenerateStoredProcedures(kvp.Key, kvp.Value);
            }
        }

        public void GenerateStoredProcedures(MetaInformationSchema schema,
                                             List.InformationSchemaColumn columns)
        {
           
            PredicateFunctions predicateFunctions = new PredicateFunctions();

            Data.InformationSchemaTable table = schema.MetaTable;
            GenerateStoredProcedures(table,
                                    schema.MetaTableConstraints,
                                    columns,
                                    schema.MetaColumnToConstraintColumnUsage);

        }

        public void GenerateStoredProcedures(Data.InformationSchemaTable table,
                                             List.InformationSchemaTableConstraint tableConstraints,
                                             List.InformationSchemaColumn columns,
                                             Dictionary<Data.InformationSchemaColumn,
            List.InformationSchemaConstraintColumnUsage> constraintColumnUsage
           )
        {
            List<StoredProcedureParameter> allParameters = GetStoredProcedureParameters(columns);
            List<StoredProcedureParameter> primaryKeyParameters = GetPrimaryKeyColumns(allParameters,
                                                                                        constraintColumnUsage,
                                                                                        tableConstraints);
            string sprocGetByPrimaryKeysSprocName = DbHelper.GenerateGetByPrimaryKeySprocName(table.TableName);

           

            if (primaryKeyParameters.Count > 0)
            {
                List<StoredProcedureParameter> parametersForGetByPrimaryKey =
               CloneParametersForUseWithAnotherSproc(primaryKeyParameters);

                StoredProcedure sprocGetByPrimaryKeys = GenerateGetByPrimaryKeySproc(parametersForGetByPrimaryKey,
                                                                         table,
                                                                         columns,
                                                                         sprocGetByPrimaryKeysSprocName);               

                _listOfGeneratedStoredProcedures.Add(sprocGetByPrimaryKeys);
          
                List<StoredProcedureParameter> parametersForUpdateByPrimaryKey =
                    CloneParametersForUseWithAnotherSproc(allParameters);

                string updateByPrimaryKeySprocName = DbHelper.GenerateUpdateByPrimaryKeySprocName(table.TableName);

                StoredProcedure sprocUpdateByPrimaryKey = GenerateUpdateByPrimaryKeySproc(parametersForUpdateByPrimaryKey,
                                                                                          table,
                                                                                          columns,
                                                                                          updateByPrimaryKeySprocName,
                                                                                          primaryKeyParameters);
                _listOfGeneratedStoredProcedures.Add(sprocUpdateByPrimaryKey);

                List<StoredProcedureParameter> parametersForDeleteByPrimaryKey =
                    CloneParametersForUseWithAnotherSproc(allParameters);

                string sprocDeleteByPrimaryKeyName = DbHelper.GenerateDeleteByPrimaryKeySprocName(table.TableName);
                StoredProcedure sprocDeleteByPrimaryKey = GenerateDeleteByPrimaryKeySproc(parametersForDeleteByPrimaryKey,
                                                                                              table,
                                                                                              columns,
                                                                                              sprocDeleteByPrimaryKeyName,
                                                                                              primaryKeyParameters);
                _listOfGeneratedStoredProcedures.Add(sprocDeleteByPrimaryKey);
            }

            string sprocGetAllSprocName = DbHelper.GenerateGetAllSprocName(table.TableName);

            StoredProcedure sprocGetAll = GenerateGetAllSproc(table,
                                                              columns,
                                                              sprocGetAllSprocName);

            _listOfGeneratedStoredProcedures.Add(sprocGetAll);

            string sprocGetByCriteriaFuzzySprocName = DbHelper.GenerateGetByCriteriaFuzzySprocName(table.TableName);

            List<StoredProcedureParameter> parametersForGetByCriteriaFuzzy =
                CloneParametersForUseWithAnotherSproc(allParameters);

            StoredProcedure sprocGetByCriteriaFuzzy = GenerateGetByCriteriaFuzzySproc(parametersForGetByCriteriaFuzzy,
                                                                  table,
                                                                  columns,
                                                                  sprocGetByCriteriaFuzzySprocName);

            _listOfGeneratedStoredProcedures.Add(sprocGetByCriteriaFuzzy);

            string sprocGetByCriteriaExactSprocName = DbHelper.GenerateGetByCriteriaExactSprocName(table.TableName);

            List<StoredProcedureParameter> parametersForGetByCriteriaExact =
                CloneParametersForUseWithAnotherSproc(allParameters);

            StoredProcedure sprocGetByCriteriaExact = GenerateGetByCriteriaExactSproc(parametersForGetByCriteriaExact,
                                                                  table,
                                                                  columns,
                                                                  sprocGetByCriteriaExactSprocName);

            _listOfGeneratedStoredProcedures.Add(sprocGetByCriteriaExact);

            //you can only attach a parameter to one sproc

            List<StoredProcedureParameter> parametersForInsert =
                CloneParametersForUseWithAnotherSproc(allParameters);

            List<StoredProcedureParameter> primaryKeyParametersForInsert =
                CloneParametersForUseWithAnotherSproc(primaryKeyParameters);           


            string insertSprocName = DbHelper.GenerateInsertSprocName(table.TableName);

            StoredProcedure sprocInsert = GenerateInsertSproc(parametersForInsert,
                                                              table,
                                                              columns,
                                                              insertSprocName,
                                                              primaryKeyParameters);
           
            _listOfGeneratedStoredProcedures.Add(sprocInsert);



        }

        public void SetPrimaryKeyOutParameters(List<StoredProcedureParameter> parameters,
                                            string tableName,
                                            StoredProcedure sproc,
                                            List<StoredProcedureParameter> primaryKeyParameters)
        {
            foreach (StoredProcedureParameter parameter in parameters)
            {
                foreach (StoredProcedureParameter primaryKeyParameter in primaryKeyParameters)
                {
                    if (primaryKeyParameter.Name == parameter.Name)
                    {
                        string parameterName = parameter.Name;
                        string columnName = parameterName.Remove(0,1);
                        if (!CommonLibrary.Utility.DatabaseHelperMethods.IsIdentityColumn(tableName, columnName, _connectionString))
                        {
                            parameter.IsOutputParameter = true;
                        }
                    }
                }
            }
        }

        public void SetIdentityOutParameters(List<StoredProcedureParameter> parameters,
                                 string tableName,
                                 StoredProcedure sproc)
        {
            List<StoredProcedureParameter> parametersToSet = new List<StoredProcedureParameter>();

            foreach (StoredProcedureParameter parameter in parameters)
            {
                string parameterName = parameter.Name;
                string columnName = parameterName.Remove(0, 1);

                if (CommonLibrary.Utility.DatabaseHelperMethods.IsIdentityColumn(tableName,
                                                                                 columnName,
                                                                                 _connectionString))
                {
                    parametersToSet.Add(parameter);
                }
            }

            if (parametersToSet.Count > 0)
            {
                foreach(StoredProcedureParameter parameterToSet in parametersToSet)
                {
                    //sproc.Parameters.Remove(parameterToRemove);
                    parameterToSet.IsOutputParameter = true;
                }
            }
        }



        public void RemoveIdentityParameters(List<StoredProcedureParameter> parameters,
                         string tableName,
                         StoredProcedure sproc)
        {
            List<StoredProcedureParameter> parametersToRemove = new List<StoredProcedureParameter>();

            foreach (StoredProcedureParameter parameter in parameters)
            {
                string parameterName = parameter.Name;
                string columnName = parameterName.Remove(0, 1);

                if (CommonLibrary.Utility.DatabaseHelperMethods.IsIdentityColumn(tableName,
                                                                                 columnName,
                                                                                 _connectionString))
                {
                    parametersToRemove.Add(parameter);
                }
            }

            if (parametersToRemove.Count > 0)
            {
                foreach (StoredProcedureParameter parameterToRemove in parametersToRemove)
                {
                    sproc.Parameters.Remove(parameterToRemove);
                    //parameterToSet.IsOutputParameter = true;
                }
            }
        }


        public List<StoredProcedureParameter> CloneParametersForUseWithAnotherSproc(List<StoredProcedureParameter> parameters)
        {
            List<StoredProcedureParameter> newParameters = new List<StoredProcedureParameter>();
            foreach (StoredProcedureParameter parameter in parameters)
            {
                newParameters.Add(CloneStoredProcedureParameter(parameter));
            }
            return newParameters;
        }


        public string GenerateGetAllSprocBody(List.InformationSchemaColumn columns,
                                              string tableName)
        {
            string sprocBody = string.Empty;
            StringBuilder sb = new StringBuilder();

            sb.Append(Sql.SELECT);
            sb.Append(Sql.SPACE);

            sb.Append(GenerateGetAllColumnsList(columns));

            sb.Append(Environment.NewLine);
            sb.Append(Sql.FROM);
            sb.Append(Sql.SPACE);
            sb.Append(tableName);           

            return sb.ToString();
        }
                                              

        public StoredProcedure GenerateGetByPrimaryKeySproc(List<StoredProcedureParameter> parameters,
                                                       Data.InformationSchemaTable table,
                                                        List.InformationSchemaColumn columns,
                                                        string sprocName)
        {
            StoredProcedure sproc = new StoredProcedure();
            sproc.Parent = _db;

            sproc.Schema = _schema;
           
            sproc.Name = sprocName;           
            sproc.TextMode = false;
            sproc.AnsiNullsStatus = false;
            sproc.QuotedIdentifierStatus = false; 
            bool setDefault = false;
            bool removeInvalidSearchParameters = true;

            List<StoredProcedureParameter> parametersToRemove = 
                SetParameterDataTypesAndReturnInvalidParameters(parameters,
                                                                setDefault,
                                                                columns,
                                                                sproc,
                                                                removeInvalidSearchParameters);
            

            foreach (StoredProcedureParameter parameterToRemove in parametersToRemove)
            {
                if (parameters.Contains(parameterToRemove))
                {
                    parameters.Remove(parameterToRemove);
                }
            }

            string sprocBody = GenerateGetByPrimaryKeySprocBody(parameters,
                                        columns,
                                        table.TableName);            

            sproc.TextBody = sprocBody;                         
            
            return sproc;
        }


        public StoredProcedure GenerateGetByCriteriaFuzzySproc(List<StoredProcedureParameter> parameters,
                                               Data.InformationSchemaTable table,
                                                List.InformationSchemaColumn columns,
                                                string sprocName)
        {
            StoredProcedure sproc = new StoredProcedure();
            sproc.Parent = _db;

            sproc.Schema = _schema;

            sproc.Name = sprocName;
            sproc.TextMode = false;
            sproc.AnsiNullsStatus = false;
            sproc.QuotedIdentifierStatus = false;
            bool setDefault = true;
            bool removeImageTypeParameters = true;

            List<StoredProcedureParameter> parametersToRemove = 
                SetParameterDataTypesAndReturnInvalidParameters(parameters,
                                                                setDefault,
                                                                columns,
                                                                sproc,
                                                                removeImageTypeParameters);

            foreach (StoredProcedureParameter parameterToRemove in parametersToRemove)
            {
                if (parameters.Contains(parameterToRemove))
                {
                    parameters.Remove(parameterToRemove);
                }
            }

            string sprocBody = GenerateGetByCriteriaFuzzySprocBody(parameters,
                                        columns,
                                        table.TableName);

            sproc.TextBody = sprocBody;

            return sproc;
        }

        public StoredProcedure GenerateGetByCriteriaExactSproc(List<StoredProcedureParameter> parameters,
                                       Data.InformationSchemaTable table,
                                        List.InformationSchemaColumn columns,
                                        string sprocName)
        {
            StoredProcedure sproc = new StoredProcedure();
            sproc.Parent = _db;

            sproc.Schema = _schema;

            sproc.Name = sprocName;
            sproc.TextMode = false;
            sproc.AnsiNullsStatus = false;
            sproc.QuotedIdentifierStatus = false;
            bool setDefault = true;
            bool removeImageTypeParameters = true;

            List<StoredProcedureParameter> parametersToRemove =
                SetParameterDataTypesAndReturnInvalidParameters(parameters,
                                                                setDefault,
                                                                columns,
                                                                sproc,
                                                                removeImageTypeParameters);

            foreach (StoredProcedureParameter parameterToRemove in parametersToRemove)
            {
                if (parameters.Contains(parameterToRemove))
                {
                    parameters.Remove(parameterToRemove);
                }
            }

            string sprocBody = GenerateGetByCriteriaExactSprocBody(parameters,
                                        columns,
                                        table.TableName);

            sproc.TextBody = sprocBody;

            return sproc;
        }

        public StoredProcedure GenerateGetAllSproc(Data.InformationSchemaTable table,
                                                List.InformationSchemaColumn columns,
                                                string sprocName)
        {
            StoredProcedure sproc = new StoredProcedure();
            sproc.Parent = _db;

            sproc.Schema = _schema;

            sproc.Name = sprocName;
            sproc.TextMode = false;
            sproc.AnsiNullsStatus = false;
            sproc.QuotedIdentifierStatus = false;
            bool setDefault = false;

            string sprocBody = GenerateGetAllSprocBody(columns,
                                                       table.TableName);

            sproc.TextBody = sprocBody;

            return sproc;
        }

        public StoredProcedure GenerateInsertSproc(List<StoredProcedureParameter> parameters,
                                               Data.InformationSchemaTable table,
                                                List.InformationSchemaColumn columns,
                                                string sprocName,
                                                List<StoredProcedureParameter> primaryKeyParameters)
        {
            StoredProcedure sproc = new StoredProcedure();
            sproc.Parent = _db;

            sproc.Schema = _schema;

            sproc.Name = sprocName;
            sproc.TextMode = false;
            sproc.AnsiNullsStatus = false;
            sproc.QuotedIdentifierStatus = false;
            bool setDefault = false;
            bool removeInvalidSearchParameters = false;
            
            SetParameterDataTypesAndReturnInvalidParameters(parameters,
                                                                setDefault,
                                                                columns,
                                                                sproc,
                                                                removeInvalidSearchParameters);
            SetIdentityOutParameters(parameters,
                                    table.TableName,
                                    sproc);

            SetPrimaryKeyOutParameters(parameters,
                                    table.TableName,
                                    sproc,
                                    primaryKeyParameters);            


            string sprocBody = GenerateInsertSprocBody(parameters,
                                        columns,
                                        table.TableName,
                                        primaryKeyParameters);

            sproc.TextBody = sprocBody;

            return sproc;
        }
        public StoredProcedure GenerateUpdateByPrimaryKeySproc(List<StoredProcedureParameter> parameters,
                                              Data.InformationSchemaTable table,
                                               List.InformationSchemaColumn columns,
                                               string sprocName,
                                                List<StoredProcedureParameter> primaryKeyParameters)
        {
            StoredProcedure sproc = new StoredProcedure();
            sproc.Parent = _db;

            sproc.Schema = _schema;

            sproc.Name = sprocName;
            sproc.TextMode = false;
            sproc.AnsiNullsStatus = false;
            sproc.QuotedIdentifierStatus = false;
            bool setDefault = false;
            bool removeInvalidSearchParameters = false;

            SetParameterDataTypesAndReturnInvalidParameters(parameters,
                                                                setDefault,
                                                                columns,
                                                                sproc,
                                                                removeInvalidSearchParameters);
            //SetIdentityOutParameters(parameters,
            //                       table.TableName,
            //                       sproc);

            string sprocBody = GenerateUpdateByPrimaryKeySprocBody(parameters,
                                                                   columns,
                                                                   table.TableName,
                                                                   primaryKeyParameters);

            sproc.TextBody = sprocBody;

            return sproc;
        }

        public StoredProcedure GenerateDeleteByPrimaryKeySproc(List<StoredProcedureParameter> parameters,
                                      Data.InformationSchemaTable table,
                                       List.InformationSchemaColumn columns,
                                       string sprocName,
                                        List<StoredProcedureParameter> primaryKeyParameters)
        {
            StoredProcedure sproc = new StoredProcedure();
            sproc.Parent = _db;

            sproc.Schema = _schema;

            sproc.Name = sprocName;
            sproc.TextMode = false;
            sproc.AnsiNullsStatus = false;
            sproc.QuotedIdentifierStatus = false;
            bool setDefault = false;
            bool removeInvalidSearchParameters = false;

            SetParameterDataTypesAndReturnInvalidParameters(parameters,
                                                                setDefault,
                                                                columns,
                                                                sproc,
                                                                removeInvalidSearchParameters);

           
                RemoveNonPrimaryKeyParameters(parameters, sproc, primaryKeyParameters);           


            string sprocBody = GenerateDeleteByPrimaryKeySprocBody(parameters,
                                                                   columns,
                                                                   table.TableName,
                                                                   primaryKeyParameters);

            sproc.TextBody = sprocBody;

            return sproc;
        }

        public List<StoredProcedureParameter> RemoveNonPrimaryKeyParameters(List<StoredProcedureParameter> parameters,           
            StoredProcedure sproc,
            List<StoredProcedureParameter> primaryKeyParameters)
        {
             List<StoredProcedureParameter> parametersToRemove = new List<StoredProcedureParameter>();              
              for (int i = 0; i < parameters.Count; i++)
              {
                  //parameters[i].Parent = sproc;
                  bool isPrimaryKey = false;
                  foreach (StoredProcedureParameter primaryKeyParameter in primaryKeyParameters)
                  {
                      if (parameters[i].Name == primaryKeyParameter.Name)
                      {
                          isPrimaryKey = true;
                          break;
                      }
                  }
                  if (!isPrimaryKey)
                  {
                      if (sproc.Parameters.Contains(parameters[i].Name))
                      {
                          sproc.Parameters.Remove(parameters[i]);
                          parametersToRemove.Add(parameters[i]);
                      }
                     
                  }
              }
              return parametersToRemove;

        }

        public List<StoredProcedureParameter> SetParameterDataTypesAndReturnInvalidParameters(
            List<StoredProcedureParameter> parameters,
            bool setDefault,
            List.InformationSchemaColumn columns,
            StoredProcedure sproc,
            bool removeInvalidSearchParameters)
        {
            PredicateFunctions predicateFunctions = new PredicateFunctions();
            List<StoredProcedureParameter> parametersToRemove = new List<StoredProcedureParameter>();

            for (int i = 0; i < parameters.Count; i++)
            {
                parameters[i].Parent = sproc;

                predicateFunctions.ColumnNameHolder = parameters[i].UserData.ToString();
                Data.InformationSchemaColumn column;
                column = columns.Find(predicateFunctions.FindInformationSchemaColumn);
                if (setDefault)
                {                    
                   parameters[i].DefaultValue = "null";
                }

                parameters[i].IsOutputParameter = false;

                switch (column.DataType)
                {
                    case SqlNativeTypeConstants.IMAGE:
                        {
                            if (parameters.Contains(parameters[i]))
                            {
                                parameters[i].DataType = DataType.Image;
                                if (removeInvalidSearchParameters)
                                {
                                    parametersToRemove.Add(parameters[i]);
                                }
                                else
                                {
                                    sproc.Parameters.Add(parameters[i]);
                                }
                            }
                            break;
                        }
                    case SqlNativeTypeConstants.TEXT:
                        {
                            parameters[i].DataType = DataType.Text;
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }
                    case SqlNativeTypeConstants.TINYINT:
                        {
                            parameters[i].DataType = DataType.TinyInt;
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }
                    case SqlNativeTypeConstants.SMALLINT:
                        {
                            parameters[i].DataType = DataType.SmallInt;
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }
                    case SqlNativeTypeConstants.INT:
                        {
                            parameters[i].DataType = DataType.Int;
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }
                    case SqlNativeTypeConstants.SMALLDATETIME:
                        {
                            parameters[i].DataType = DataType.SmallDateTime;
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }
                    case SqlNativeTypeConstants.REAL:
                        {
                            parameters[i].DataType = DataType.Real;
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }
                    case SqlNativeTypeConstants.MONEY:
                        {
                            parameters[i].DataType = DataType.Money;
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }
                    case SqlNativeTypeConstants.DATETIME:
                        {
                            parameters[i].DataType = DataType.DateTime;
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }
                    case SqlNativeTypeConstants.FLOAT:
                        {
                            parameters[i].DataType = DataType.Float;
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }
                    case SqlNativeTypeConstants.NTEXT:
                        {
                            parameters[i].DataType = DataType.NText;
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }
                    case SqlNativeTypeConstants.BIT:
                        {
                            parameters[i].DataType = DataType.Bit;
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }
                    case SqlNativeTypeConstants.DECIMAL:
                        {
                            parameters[i].DataType = DataType.Decimal(resolveNullIntToZero(column.NumericScale),
                                                                  resolveNullIntToZero(column.NumericPrecision));
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }
                    case SqlNativeTypeConstants.SMALLMONEY:
                        {
                            parameters[i].DataType = DataType.SmallMoney;
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }
                    case SqlNativeTypeConstants.BIGINT:
                        {
                            parameters[i].DataType = DataType.BigInt;
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }
                    case SqlNativeTypeConstants.VARBINARY:
                        {
                            parameters[i].DataType = DataType.VarBinary(resolveNullIntToZero(column.CharacterOctetLength));
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }
                    case SqlNativeTypeConstants.VARCHAR:
                        {
                            parameters[i].DataType = DataType.VarChar(resolveNullIntToZero(column.CharacterMaximumLength));
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }
                    case SqlNativeTypeConstants.SQL_VARIANT:
                        {
                            parameters[i].DataType = DataType.Variant;
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }
                    case SqlNativeTypeConstants.BINARY:
                        {
                            parameters[i].DataType = DataType.Binary(resolveNullIntToZero(column.CharacterOctetLength));
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }
                    case SqlNativeTypeConstants.CHAR:
                        {
                            parameters[i].DataType = DataType.Char(resolveNullIntToZero(column.CharacterMaximumLength));
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }
                    case SqlNativeTypeConstants.NVARCHAR:
                        {
                            parameters[i].DataType = DataType.NVarChar(resolveNullIntToZero(column.CharacterMaximumLength));
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }
                    case SqlNativeTypeConstants.NCHAR:
                        {
                            parameters[i].DataType = DataType.NChar(resolveNullIntToZero(column.CharacterMaximumLength));
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }
                    case SqlNativeTypeConstants.UNIQUEIDENTIFIER:
                        {
                            parameters[i].DataType = DataType.UniqueIdentifier;
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }

                    case SqlNativeTypeConstants.NUMERIC:
                        {
                            int numericScale = -1;
                            int numericPrecision = -1;

                            if (column.NumericScale != null)
                                numericScale = Convert.ToInt32(column.NumericScale);

                            if (column.NumericPrecision != null)
                                numericPrecision = Convert.ToInt32(column.NumericPrecision);

                            parameters[i].DataType = DataType.Numeric(numericScale, numericPrecision);
                            sproc.Parameters.Add(parameters[i]);
                            break;
                        }
                    case SqlNativeTypeConstants.XML:
                        {
                            if (parameters.Contains(parameters[i]))
                            {
                                parameters[i].DataType = Microsoft.SqlServer.Management.Smo.DataType.Xml(column.TableSchema +
                                    CommonLibrary.Constants.ClassCreationConstants.DOT_OPERATOR +
                                    column.TableName);                                    
                                   
                                if (removeInvalidSearchParameters)
                                {
                                    parametersToRemove.Add(parameters[i]);
                                }
                                else
                                {
                                    sproc.Parameters.Add(parameters[i]);
                                }
                            }
                            break;
                        }

                }
            }
            return parametersToRemove;
        }

        public Dictionary<StoredProcedure,string> CreateNewStoredProceduresAndReplaceExistingOnes(List<StoredProcedure> sprocs)
        {
            Dictionary<StoredProcedure, string> sprocsThatCannotBeCreated = new Dictionary<StoredProcedure, string>();

            foreach (StoredProcedure sproc in sprocs)
            {
                if (_db.StoredProcedures.Contains(sproc.Name))
                {
                    _db.StoredProcedures[sproc.Name].Drop();
                }
                try
                {
                    sproc.Create();
                }
                catch (SmoException smo)
                {
                    string message = string.Empty;
                    message = smo.Message;
                    if (smo.InnerException != null)
                    {
                        
                        message += Sql.COMMA_PLUS_SPACE + "Inner Exception: " + smo.InnerException.InnerException.Message;
                    }
                    sprocsThatCannotBeCreated.Add(sproc, message + Environment.NewLine + smo.StackTrace);
                }
            }
            return sprocsThatCannotBeCreated;
        }

        public Dictionary<StoredProcedure, string> DropStoredProcedures(List<StoredProcedure> sprocs)
        {
            Dictionary<StoredProcedure, string> sprocsThatCannotBeDropped = new Dictionary<StoredProcedure, string>();

            foreach (StoredProcedure sproc in sprocs)
            {
                try
                {
                    if (_db.StoredProcedures.Contains(sproc.Name))
                    {
                        _db.StoredProcedures[sproc.Name].Drop();
                    }              
                }
                catch (SmoException smo)
                {
                    sprocsThatCannotBeDropped.Add(sproc, smo.Message);
                }
            }
            return sprocsThatCannotBeDropped;
        }

        public Dictionary<StoredProcedure,string> CreateNewStoredProceduresDoNotReplaceExistingOnes(List<StoredProcedure> sprocs)
        {
            Dictionary<StoredProcedure, string> sprocsThatExistOrCannotBeCreated = new Dictionary<StoredProcedure, string>();
            foreach (StoredProcedure sproc in sprocs)
            {
                try
                {
                    if (_db.StoredProcedures.Contains(sproc.Name))
                    {
                        sprocsThatExistOrCannotBeCreated.Add(sproc, "sproc already exists");
                    }
                    else
                    {
                        sproc.Create();
                    }
                }
                catch (SmoException smo)
                {
                    sprocsThatExistOrCannotBeCreated.Add(sproc, smo.Message);
                }
            }
            return sprocsThatExistOrCannotBeCreated;
        }

        public string RemoveInvalidCharsFromDefaultValueFromSql(string value)
        {
            string cleanedString = string.Empty;

            cleanedString = Regex.Replace(value, @"[()]", "");

            return cleanedString;
        }

        public string GenerateGetByPrimaryKeySprocBody(List<StoredProcedureParameter> parameters,
                                                       List.InformationSchemaColumn columns,
                                                       string tableName)
        {
            string sprocBody = string.Empty;
            StringBuilder sb = new StringBuilder();

            sb.Append(Sql.SELECT);
            sb.Append(Sql.SPACE);

            sb.Append(GenerateGetColumnsList(columns));

            sb.Append(Environment.NewLine);
            sb.Append(Sql.FROM);
            sb.Append(Sql.SPACE);
            sb.Append(tableName);
            sb.Append(Sql.SPACE);
            sb.Append(Environment.NewLine);
            sb.Append(Sql.WHERE);
            sb.Append(Sql.SPACE);
            sb.Append(GenerateGetWhereListPrimaryKey(parameters));

            return sb.ToString();
            
        }

        public string GenerateInsertSprocBody(List<StoredProcedureParameter> parameters,
                                               List.InformationSchemaColumn columns,
                                               string tableName,
                                                List<StoredProcedureParameter> primaryKeyParameters)
        {
            string sprocBody = string.Empty;
            StringBuilder sb = new StringBuilder();

            sb.Append(Sql.INSERT);
            sb.Append(Sql.SPACE);
            sb.Append(Sql.INTO);
            sb.Append(Sql.SPACE);
            sb.Append(tableName);
            sb.Append(Sql.SPACE);
            sb.Append(Environment.NewLine);

            sb.Append(GenerateInsertColumnsList(columns));

            sb.Append(Environment.NewLine);

            sb.Append(Sql.VALUES);
            sb.Append(Sql.SPACE);
            sb.Append(GenerateInsertValuesList(parameters,tableName));
            sb.Append(Sql.SPACE);
            sb.Append(Environment.NewLine);

            sb.Append(GetIdentityColumnsSelectStatements(primaryKeyParameters,tableName));

            return sb.ToString();

        }

        public string GetIdentityColumnsSelectStatements(List<StoredProcedureParameter> primaryKeyParameters,
                                                         string tableName)
        {
            StringBuilder sb = new StringBuilder();

            foreach (StoredProcedureParameter primaryKeyParameter in primaryKeyParameters)
            {
                if (CommonLibrary.Utility.DatabaseHelperMethods.IsIdentityColumn(tableName,
                                                                                 primaryKeyParameter.UserData.ToString(),
                                                                                 _connectionString))
                {
                    //SET @ProductID = SCOPE_IDENTITY()
                    sb.Append(Sql.SET);
                    sb.Append(Sql.SPACE);
                    sb.Append(primaryKeyParameter.Name);
                    sb.Append(Sql.SPACE);
                    sb.Append(Sql.EQUALS);
                    sb.Append(Sql.SPACE);
                    sb.Append(Sql.IDENT_CURRENT);
                    sb.Append(Sql.SQL_OPEN_BRACKET);
                    sb.Append(Sql.SINGLE_QUOTE);
                    sb.Append(tableName);
                    sb.Append(Sql.SINGLE_QUOTE);
                    sb.Append(Sql.SQL_CLOSE_BRACKET);
                    sb.Append(Environment.NewLine);
                }
            }
            return sb.ToString();
        }

        public void SetPrimaryKeyColumnsToInAndOut(List<StoredProcedureParameter> primaryKeyParameters,
                                                   string tableName)
        {            

            foreach (StoredProcedureParameter parameter in primaryKeyParameters)
            {
                if (!CommonLibrary.Utility.DatabaseHelperMethods.IsIdentityColumn(tableName,
                                                                                parameter.UserData.ToString(),
                                                                                _connectionString))
                {
                    //if not an identity then set the parameter to in/out                     
                    parameter.IsOutputParameter = true;
                  
                }
            }                      
        }

        public bool AreAllParametersForUpdatePartOfPrimaryKey(List<StoredProcedureParameter> parameters,
            List<StoredProcedureParameter> primaryKeyParameters)
        {
            Dictionary<string, bool> parameterToFoundInPrimaryKeyParameters =
                new Dictionary<string, bool>();
            foreach (StoredProcedureParameter parameter in parameters)
            {
                parameterToFoundInPrimaryKeyParameters.Add(parameter.Name, false);
                foreach (StoredProcedureParameter primaryKeyParameter in primaryKeyParameters)
                {
                    if (parameter.Name == primaryKeyParameter.Name)
                    {
                        parameterToFoundInPrimaryKeyParameters[parameter.Name] = true;
                    }
                }
            }

            bool areAllParametersForUpdateFoundInPrimaryKeys = true;

            foreach (KeyValuePair<string, bool> kvp in parameterToFoundInPrimaryKeyParameters)
            {
                if (!kvp.Value)
                {
                    areAllParametersForUpdateFoundInPrimaryKeys = false;
                    break;
                }
            }
            return areAllParametersForUpdateFoundInPrimaryKeys;

        }

        public string GenerateUpdateByPrimaryKeySprocBody(List<StoredProcedureParameter> parameters,
                                       List.InformationSchemaColumn columns,
                                       string tableName,
                                        List<StoredProcedureParameter> primaryKeyParameters)
        {
            //we do not allow the updating of the primary key, so if all of the parameters avaialable are part of the 
            //primary key then we just have a sproc that does not have an implementation.
            if (!AreAllParametersForUpdatePartOfPrimaryKey(parameters, primaryKeyParameters))
            {
                string sprocBody = string.Empty;
                StringBuilder sb = new StringBuilder();

                sb.Append(Sql.UPDATE);
                sb.Append(Sql.SPACE);
                sb.Append(tableName);
                sb.Append(Sql.SPACE);
                sb.Append(Environment.NewLine);
                sb.Append(Sql.SET);
                sb.Append(Sql.SPACE);

                sb.Append(GenerateUpdateSetList(columns, parameters, tableName, primaryKeyParameters));

                sb.Append(Environment.NewLine);
                sb.Append(Sql.WHERE);
                sb.Append(Sql.SPACE);
                sb.Append(GenerateGetWhereListPrimaryKey(primaryKeyParameters));
                sb.Append(Environment.NewLine);

                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }
            

        }

        public string GenerateDeleteByPrimaryKeySprocBody(List<StoredProcedureParameter> parameters,
                               List.InformationSchemaColumn columns,
                               string tableName,
                                List<StoredProcedureParameter> primaryKeyParameters)
        {
            string sprocBody = string.Empty;
            StringBuilder sb = new StringBuilder();

            sb.Append(Sql.DELETE);
            sb.Append(Sql.SPACE);
            sb.Append(Sql.FROM);
            sb.Append(Sql.SPACE);
            sb.Append(tableName);
            sb.Append(Sql.SPACE);            
            sb.Append(Environment.NewLine);
            sb.Append(Sql.WHERE);
            sb.Append(Sql.SPACE);
            sb.Append(GenerateGetWhereListPrimaryKey(primaryKeyParameters));
            sb.Append(Environment.NewLine);

            return sb.ToString();

        }


        public string GenerateGetWhereListPrimaryKey(List<StoredProcedureParameter> parameters)
        {
            int currentColumn = 0;
            int columnCount = parameters.Count;
            StringBuilder sb = new StringBuilder();

            int parameterCount = parameters.Count;

            foreach (StoredProcedureParameter parameter in parameters)
            {
                if(currentColumn == 0 && parameterCount > 1)
                {
                    sb.Append(Sql.SQL_OPEN_BRACKET);
                }

                if (parameterCount > 1)
                {
                    sb.Append(Sql.SQL_OPEN_BRACKET);
                }
                sb.Append(Sql.SPACE);
                sb.Append(parameter.UserData);
                sb.Append(Sql.SPACE);
                sb.Append(Sql.EQUALS);
                sb.Append(Sql.SPACE);
                sb.Append(parameter.Name);
                sb.Append(Sql.SPACE);
                
                if (parameterCount > 1)
                {
                    sb.Append(Sql.SQL_CLOSE_BRACKET);
                }
                sb.Append(Sql.SPACE);
                sb.Append(Environment.NewLine);

                if (parameterCount > 1)
                {
                    if (currentColumn != columnCount - 1)
                    {
                        sb.Append(Sql.AND);
                        sb.Append(Sql.SPACE);
                    }
                    if (currentColumn == columnCount - 1)
                    {
                        sb.Append(Sql.SQL_CLOSE_BRACKET);
                    }
                }
                currentColumn++;
            }
            return sb.ToString();

        }

        public string GenerateGetByCriteriaFuzzySprocBody(List<StoredProcedureParameter> parameters,
                                             List.InformationSchemaColumn columns,
                                             string tableName)
        {
            string sprocBody = string.Empty;
            StringBuilder sb = new StringBuilder();

            sb.Append(Sql.SELECT);
            sb.Append(Sql.SPACE);

            sb.Append(GenerateGetColumnsList(columns));

            sb.Append(Environment.NewLine);
            sb.Append(Sql.FROM);
            sb.Append(Sql.SPACE);
            sb.Append(tableName);
            sb.Append(Sql.SPACE);
            sb.Append(Environment.NewLine);
            sb.Append(Sql.WHERE);
            sb.Append(Sql.SPACE);
            sb.Append(GenerateGetWhereListByCriteriaFuzzy(parameters));

            return sb.ToString();
        }

        public string GenerateGetByCriteriaExactSprocBody(List<StoredProcedureParameter> parameters,
                                             List.InformationSchemaColumn columns,
                                             string tableName)
        {
            string sprocBody = string.Empty;
            StringBuilder sb = new StringBuilder();

            sb.Append(Sql.SELECT);
            sb.Append(Sql.SPACE);

            sb.Append(GenerateGetColumnsList(columns));

            sb.Append(Environment.NewLine);
            sb.Append(Sql.FROM);
            sb.Append(Sql.SPACE);
            sb.Append(tableName);
            sb.Append(Sql.SPACE);
            sb.Append(Environment.NewLine);
            sb.Append(Sql.WHERE);
            sb.Append(Sql.SPACE);
            sb.Append(GenerateGetWhereListByCriteriaExact(parameters));

            return sb.ToString();
        }

        public string GenerateGetWhereListByCriteriaExact(List<StoredProcedureParameter> parameters)
        {
            int currentColumn = 0;
            int columnCount = parameters.Count;
            StringBuilder sb = new StringBuilder();

            int parameterCount = parameters.Count;


            foreach (StoredProcedureParameter parameter in parameters)
            {
                //if (currentColumn == 0 && parameterCount > 1)
                //{
                //    sb.Append(Sql.SQL_OPEN_BRACKET);
                //}

                if (parameterCount > 1)
                {
                    sb.Append(Sql.SQL_OPEN_BRACKET);
                }
                sb.Append(Sql.SPACE);
                sb.Append(GetComparisonStatementByTypeForExactCriteriaSprocs(parameter));
                sb.Append(Sql.SPACE);
                sb.Append(Sql.OR);
                sb.Append(Sql.SPACE);
                sb.Append(parameter.Name);
                sb.Append(Sql.SPACE);
                sb.Append(Sql.EQUALS);
                sb.Append(Sql.SPACE);
                sb.Append(Sql.NULL);
                sb.Append(Sql.SPACE);
                if (parameterCount > 1)
                {
                    sb.Append(Sql.SQL_CLOSE_BRACKET);
                }
                sb.Append(Sql.SPACE);
                sb.Append(Environment.NewLine);

                if (parameterCount > 1)
                {
                    if (currentColumn != columnCount - 1)
                    {
                        sb.Append(Sql.AND);
                        sb.Append(Sql.SPACE);
                    }
                    //if (currentColumn == columnCount - 1)
                    //{
                    //    sb.Append(Sql.SQL_CLOSE_BRACKET);
                    //}
                }
                currentColumn++;
            }
            return sb.ToString();

        }


        public string GenerateGetWhereListByCriteriaFuzzy(List<StoredProcedureParameter> parameters)
        {
            int currentColumn = 0;
            int columnCount = parameters.Count;
            StringBuilder sb = new StringBuilder();

            int parameterCount = parameters.Count;


            foreach (StoredProcedureParameter parameter in parameters)
            {                
                //if (currentColumn == 0 && parameterCount > 1)
                //{
                //    sb.Append(Sql.SQL_OPEN_BRACKET);
                //}

                if (parameterCount > 1)
                {
                    sb.Append(Sql.SQL_OPEN_BRACKET);
                }
                sb.Append(Sql.SPACE);
                sb.Append(GetComparisonStatementByTypeForFuzzyCriteriaSprocs(parameter));
                sb.Append(Sql.SPACE);
                sb.Append(Sql.OR);
                sb.Append(Sql.SPACE);
                sb.Append(parameter.Name);
                sb.Append(Sql.SPACE);
                sb.Append(Sql.EQUALS);
                sb.Append(Sql.SPACE);
                sb.Append(Sql.NULL);
                sb.Append(Sql.SPACE);
                if (parameterCount > 1)
                {
                    sb.Append(Sql.SQL_CLOSE_BRACKET);
                }
                sb.Append(Sql.SPACE);
                sb.Append(Environment.NewLine);

                if (parameterCount > 1)
                {
                    if (currentColumn != columnCount - 1)
                    {
                        sb.Append(Sql.AND);
                        sb.Append(Sql.SPACE);
                    }
                    //if (currentColumn == columnCount - 1)
                    //{
                    //    sb.Append(Sql.SQL_CLOSE_BRACKET);
                    //}
                }
                currentColumn++;
            }
            return sb.ToString();

        }

        public string GetComparisonStatementByTypeForExactCriteriaSprocs(StoredProcedureParameter parameter)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(parameter.UserData);
            sb.Append(Sql.SPACE);
            sb.Append(Sql.EQUALS);
            sb.Append(Sql.SPACE);
            sb.Append(parameter.Name);
           
            return sb.ToString();

        }

        public string GetComparisonStatementByTypeForFuzzyCriteriaSprocs(StoredProcedureParameter parameter)
        {
            StringBuilder sb = new StringBuilder();
            string comparisonOperator = GetComparisonOperatorByType(parameter.DataType.SqlDataType);

            sb.Append(parameter.UserData);
            sb.Append(Sql.SPACE);
            switch (comparisonOperator)
            {
                case Sql.EQUALS:
                    {
                        sb.Append(Sql.EQUALS);
                        sb.Append(Sql.SPACE);
                        sb.Append(parameter.Name);
                        break;
                    }
                case Sql.LIKE:
                    {
                        sb.Append(Sql.LIKE);
                        sb.Append(Sql.SPACE);
                        //sb.Append(@"'%");
                        sb.Append(parameter.Name);
                        sb.Append(Sql.SPACE);
                        sb.Append(@"+");
                        sb.Append(Sql.SPACE);
                        sb.Append(@"'%'");
                        break;
                    }
            }
            return sb.ToString();
           
        }

        public string GetComparisonOperatorByType(SqlDataType type)
        {
            string comparisonOperator = Sql.EQUALS;
            switch (type)
            {
                case SqlDataType.TinyInt:
                case SqlDataType.SmallInt:
                case SqlDataType.Int:
                case SqlDataType.Real:
                case SqlDataType.Money:
                case SqlDataType.Float:
                case SqlDataType.Bit:
                case SqlDataType.Decimal:
                case SqlDataType.SmallMoney:
                case SqlDataType.BigInt:
                case SqlDataType.UniqueIdentifier:
                case SqlDataType.SmallDateTime:
                case SqlDataType.DateTime:
                case SqlDataType.Numeric:
                    {
                        comparisonOperator = Sql.EQUALS;
                        break;
                    }
                case SqlDataType.VarChar:
                case SqlDataType.Char:
                case SqlDataType.NVarChar:
                case SqlDataType.NChar:
               
                    {
                        comparisonOperator = Sql.LIKE;
                        break;
                    }
            }
            return comparisonOperator;
        }

        public string GetDefaultValueStringByType(StoredProcedureParameter parameter)
        {
            StringBuilder sb = new StringBuilder();

            switch (parameter.DataType.SqlDataType)
            {         
              
                case SqlDataType.TinyInt:                    
                case SqlDataType.SmallInt:                    
                case SqlDataType.Int:                                 
                case SqlDataType.Real:                    
                case SqlDataType.Money:                    
                case SqlDataType.Float: 
                case SqlDataType.Bit:                    
                case SqlDataType.Decimal:                    
                case SqlDataType.SmallMoney:                    
                case SqlDataType.BigInt:                 
                case SqlDataType.UniqueIdentifier:
                case SqlDataType.Numeric:
                    {
                        try
                        {
                            string defaultValue = parameter.DefaultValue.ToString();
                            string strippedValue = RemoveInvalidCharsFromDefaultValueFromSql(defaultValue);
                            sb.Append(strippedValue);
                        }
                        catch
                        {
                            sb.Append("null");
                        }
                        break;
                    }
              
                case SqlDataType.VarChar:                   
                case SqlDataType.Char:                   
                case SqlDataType.NVarChar:                    
                case SqlDataType.NChar:                   
                case SqlDataType.SmallDateTime:                    
                case SqlDataType.DateTime:
                    {
                       
                        try
                        {
                            string value = parameter.DefaultValue.ToString();
                            string strippedValue = RemoveInvalidCharsFromDefaultValueFromSql(value);
                            if (strippedValue == "null")
                            {
                                sb.Append(strippedValue);
                            }
                            else
                            {
                                sb.Append(Sql.SINGLE_QUOTE);
                                sb.Append(strippedValue);
                                sb.Append(Sql.SINGLE_QUOTE);
                            }
                        }
                        catch
                        {
                            sb.Append("null");
                        }
                        
                        break;
                    }
            }
            return sb.ToString() ;
                   
        }

        public string GenerateGetColumnsList(List.InformationSchemaColumn columns)
        {
            int columnCount = columns.Count;
            int currentCount = 0;
            StringBuilder sb = new StringBuilder();
            foreach (Data.InformationSchemaColumn column in columns)
            {
                if (currentCount != columnCount - 1)
                {
                    sb.Append(column.ColumnName);
                    sb.Append(Sql.COMMA_PLUS_SPACE);
                }
                else
                {
                    sb.Append(column.ColumnName);
                }
                currentCount++;
            }
            return sb.ToString();
        }
        public string GenerateUpdateSetList(List.InformationSchemaColumn columns,
                                            List<StoredProcedureParameter> parameters,
                                            string tableName,
                                            List<StoredProcedureParameter> primaryKeyParameters)
        {
           
            int currentCount = 0;
            StringBuilder sb = new StringBuilder();
            PredicateFunctions predicateFunctions = new PredicateFunctions();

            List<Data.InformationSchemaColumn> columnsToUse = new List<Data.InformationSchemaColumn>();
            foreach (Data.InformationSchemaColumn column in columns)
            {
                if (!CommonLibrary.Utility.DatabaseHelperMethods.IsIdentityColumn(column.TableName,
                                                                                column.ColumnName,
                                                                                _connectionString))
                {
                    bool primaryKeyFound = false;
                    foreach (StoredProcedureParameter primaryKeyParameter in primaryKeyParameters)
                    {
                       string columnName = 
                           CommonLibrary.Utility.DatabaseHelperMethods.GetColumnNameFromSqlParameterName(primaryKeyParameter.Name);
                       if (column.ColumnName == columnName)
                       {
                           primaryKeyFound = true;
                           break;
                       }
                    }
                    //do not allow setting of primary key columns
                    if (!primaryKeyFound)
                    {
                        columnsToUse.Add(column);
                    }
                }
            }

            int columnCount = columnsToUse.Count;

            foreach (Data.InformationSchemaColumn columnToUse in columnsToUse)
            {
                predicateFunctions.StoredProcedureParameterUserDataHolder = columnToUse.ColumnName;
                StoredProcedureParameter parameterFound = parameters.Find(predicateFunctions.FindSqlParameterByUserData);


                    

                if (currentCount != columnCount - 1 && (columnCount > 1))
                {
                    sb.Append(columnToUse.ColumnName);
                    sb.Append(Sql.SPACE);
                    sb.Append(Sql.EQUALS);
                    sb.Append(Sql.SPACE);
                    sb.Append(parameterFound.Name);
                    sb.Append(Sql.COMMA_PLUS_SPACE);
                    sb.Append(Environment.NewLine);
                }
                if(currentCount == columnCount - 1)
                {
                    sb.Append(columnToUse.ColumnName);
                    sb.Append(Sql.SPACE);
                    sb.Append(Sql.EQUALS);
                    sb.Append(Sql.SPACE);
                    sb.Append(parameterFound.Name);
                    sb.Append(Sql.SPACE);
                }
                currentCount++;
            }
            return sb.ToString();
        }

        public string GenerateInsertValuesList(List<StoredProcedureParameter> parameters,string tableName)
        {
           
            int currentCount = 0;
            StringBuilder sb = new StringBuilder();
            List<StoredProcedureParameter> parametersToUse = new List<StoredProcedureParameter>();

            foreach (StoredProcedureParameter parameter in parameters)
            {
                string parameterName = parameter.Name;
                string columnName = parameterName.Remove(0, 1);

                if (!CommonLibrary.Utility.DatabaseHelperMethods.IsIdentityColumn(tableName,
                                                                                 columnName,
                                                                                 _connectionString))
                {
                    parametersToUse.Add(parameter);
                }
            }

            int parameterCount = parametersToUse.Count;
            foreach (StoredProcedureParameter parameterToUse in parametersToUse)
            {   
                
                    if (currentCount == 0)
                    {
                        sb.Append(Sql.SQL_OPEN_BRACKET);
                        sb.Append(Sql.SPACE);
                    }
                    if (currentCount != parameterCount - 1 && (parameterCount > 1))
                    {
                        sb.Append(parameterToUse.Name);
                        sb.Append(Sql.COMMA_PLUS_SPACE);
                    }
                    if (currentCount == parameterCount - 1)
                    {
                        sb.Append(parameterToUse.Name);
                        sb.Append(Sql.SQL_CLOSE_BRACKET);
                    }
                    currentCount++;               
            }
            return sb.ToString();
        }

        public string GenerateInsertColumnsList(List.InformationSchemaColumn columns)
        {
            
            int currentCount = 0;
            StringBuilder sb = new StringBuilder();

            List<Data.InformationSchemaColumn> columnsToUse = new List<Data.InformationSchemaColumn>();
            foreach (Data.InformationSchemaColumn column in columns)
            {
                if (!CommonLibrary.Utility.DatabaseHelperMethods.IsIdentityColumn(column.TableName,
                                                                                column.ColumnName,
                                                                                _connectionString))
                {
                    columnsToUse.Add(column);
                }
            }

            int columnCount = columnsToUse.Count;
            foreach (Data.InformationSchemaColumn columnToUse in columnsToUse)
            {
                //do not insert into identity columns
                
                    if (currentCount == 0)
                    {
                        sb.Append(Sql.SQL_OPEN_BRACKET);
                        sb.Append(Sql.SPACE);
                    }
                    if (currentCount != columnCount - 1 && (columnCount > 1))
                    {
                        sb.Append(columnToUse.ColumnName);
                        sb.Append(Sql.COMMA_PLUS_SPACE);
                        
                    }
                    if (currentCount == columnCount - 1)
                    {
                        sb.Append(columnToUse.ColumnName);
                        sb.Append(Sql.SQL_CLOSE_BRACKET);
                    }
                    currentCount++;
                
                
            }
            return sb.ToString();
        }

        public string GenerateGetAllColumnsList(List.InformationSchemaColumn columns)
        {
            int columnCount = columns.Count;
            int currentCount = 0;
            StringBuilder sb = new StringBuilder();
            foreach (Data.InformationSchemaColumn column in columns)
            {
                if (currentCount != columnCount - 1)
                {
                    sb.Append(column.ColumnName);
                    sb.Append(Sql.COMMA_PLUS_SPACE);
                }
                else
                {
                    sb.Append(column.ColumnName);
                }
                currentCount++;
            }
            return sb.ToString();
        }


                                                       
                                            

        public List<StoredProcedureParameter> GetPrimaryKeyColumns(List<StoredProcedureParameter> parameters,
                                                                  Dictionary<Data.InformationSchemaColumn,
                                                                             List.InformationSchemaConstraintColumnUsage> columnsToConstraintUsage,
                                                                  List.InformationSchemaTableConstraint tableConstraints)
        {
            List<StoredProcedureParameter> newParametersToReturn = new List<StoredProcedureParameter>();
            PredicateFunctions predicateFunctions = new PredicateFunctions();
            Dictionary<Data.InformationSchemaColumn, List.InformationSchemaConstraintColumnUsage> constraintToColumn
            = new Dictionary<SprocDataLayerGenerator.Data.InformationSchemaColumn, SprocDataLayerGenerator.List.InformationSchemaConstraintColumnUsage>();

            List<StoredProcedureParameter> foundParameters = new List<StoredProcedureParameter>();

            foreach (Data.InformationSchemaTableConstraint tableConstraint in tableConstraints)
            {
                predicateFunctions.TableNameHolder = tableConstraint.TableName;
                predicateFunctions.ConstraintNameHolder = tableConstraint.ConstraintName;
                if (tableConstraint.ConstraintType == TableConstraintTypeConstants.PRIMARY_KEY)
                {
                    foreach (KeyValuePair<Data.InformationSchemaColumn, List.InformationSchemaConstraintColumnUsage> kvp in columnsToConstraintUsage)
                    {
                        if (kvp.Value.FindAll(predicateFunctions.FindConstraintColumnUsageByTableNameAndConstraintName).Count > 0)
                        {
                            constraintToColumn.Add(kvp.Key, kvp.Value);
                        }                        
                    }
                }

            }
            if (constraintToColumn.Count > 0)
            {
                foreach (KeyValuePair<Data.InformationSchemaColumn, List.InformationSchemaConstraintColumnUsage> kvp2 in constraintToColumn)
                {
                    predicateFunctions.StoredProcedureParameterUserDataHolder = kvp2.Key.ColumnName;
                    foundParameters.AddRange(parameters.FindAll(predicateFunctions.FindSqlParameterByUserData));
                }

            }
            foreach (StoredProcedureParameter parameter in foundParameters)
            {
                //must create a deep copy because we cannot reuse StoredProcedureParameters that already belong
                //to another sproc, so when i go to create the get by criteria for example, i cannot because
                //they already belong to the GetbyPrimaryKey sproc.
                newParametersToReturn.Add(CloneStoredProcedureParameter(parameter));

            }

            return newParametersToReturn;
        }

        public StoredProcedureParameter CloneStoredProcedureParameter(StoredProcedureParameter parameter)
        {
            StoredProcedureParameter newParameter = new StoredProcedureParameter();
            newParameter.Name = parameter.Name;
            newParameter.UserData = parameter.UserData;
            
            return newParameter;
        }
                                                                   

        public List<StoredProcedureParameter> GetStoredProcedureParameters(List.InformationSchemaColumn columns)
        {

            List<StoredProcedureParameter> parameters = new List<StoredProcedureParameter>();
            try
            {
                foreach (Data.InformationSchemaColumn column in columns)
                {
                    StoredProcedureParameter parameter = new StoredProcedureParameter();
                    parameter.Name = Sql.AT_SYMBOL + column.ColumnName;
                    parameter.UserData = column.ColumnName;
                    parameters.Add(parameter);
                }
            }
            catch (Exception ex)
            {

            }
            return parameters;
        }

        


        public int resolveNullIntToZero(int? value)
        {
            int retVal = -1;
            if (value == null)
            {
                retVal = 0;
            }
            else
            {
                retVal = Convert.ToInt32(value);
            }
            return retVal;
        }
        


    }

}
