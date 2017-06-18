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

namespace SprocDataLayerGenerator.BusinessObjects
{
    public class MetaInformationSchemaManager
    {

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

        public MetaInformationSchemaManager(string databaseName,
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

            _connectionString = InitializeAndAddInformationSchemaProcedeures(databaseName,
                                                        dataSource,
                                                        initialCatalog,
                                                        userId,
                                                        password,
                                                        trustedConnection);
            SetMetaDataList(_connectionString, tableName);
        }

        public MetaInformationSchemaManager(string databaseName,
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

            _connectionString = InitializeAndAddInformationSchemaProcedeures(databaseName,
                                                          dataSource,
                                                          initialCatalog,
                                                          userId,
                                                          password,
                                                          trustedConnection);

            SetMetaDataList(_connectionString);
        }
        private string InitializeAndAddInformationSchemaProcedeures(string databaseName,
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
            AddInformationSchemaProcedures(_db);

            string connectionString = builder.ConnectionString;
            return connectionString;

        }

        public MetaInformationSchema GetSchemaByTableName(string tableName, List<MetaInformationSchema> schemas)
        {
            MetaInformationSchema schema = null;
            PredicateFunctions predicateFunctions = new PredicateFunctions();
            predicateFunctions.TableNameHolder = tableName;
            schema = schemas.Find(predicateFunctions.FindMetaInformationSchemaByTableName);
            return schema;
        }

        private void AddInformationSchemaProcedures(Database db)
        {

            StoredProcedure sp = GetInformationSchemaStoredProcedure(Sproc.Mapping.InformationSchemaTable.INFORMATION_SCHEMA_TABLE,
                                                                     db,
                                                                     InformationSchemaSprocConstants.GET_INFORMATION_SCHEMA_TABLES);

            if (db.StoredProcedures.Contains(Sproc.Mapping.InformationSchemaTable.INFORMATION_SCHEMA_TABLE))
            {
                db.StoredProcedures[Sproc.Mapping.InformationSchemaTable.INFORMATION_SCHEMA_TABLE].Drop();
            }
            sp.Create();



            sp = GetInformationSchemaStoredProcedure(Sproc.Mapping.InformationSchemaTableConstraint.INFORMATION_SCHEMA_TABLE_CONSTRAINTS,
                                                     db,
                                                     InformationSchemaSprocConstants.GET_INFORMATION_SCHEMA_TABLE_CONSTRAINTS);

            if (db.StoredProcedures.Contains(Sproc.Mapping.InformationSchemaTableConstraint.INFORMATION_SCHEMA_TABLE_CONSTRAINTS))
            {
                db.StoredProcedures[Sproc.Mapping.InformationSchemaTableConstraint.INFORMATION_SCHEMA_TABLE_CONSTRAINTS].Drop();
            }
            sp.Create();


            sp = GetInformationSchemaStoredProcedure(Sproc.Mapping.InformationSchemaColumn.INFORMATION_SCHEMA_COLUMN,
                                                     db,
                                                     InformationSchemaSprocConstants.GET_INFORMATION_SCHEMA_COLUMNS);

            if (db.StoredProcedures.Contains(Sproc.Mapping.InformationSchemaColumn.INFORMATION_SCHEMA_COLUMN))
            {
                db.StoredProcedures[Sproc.Mapping.InformationSchemaColumn.INFORMATION_SCHEMA_COLUMN].Drop();
            }
            sp.Create();

            sp = GetInformationSchemaStoredProcedure(Sproc.Mapping.InformationSchemaConstraintColumnUsage.INFORMATION_SCHEMA_CONSTRAINT_COLUMN_USAGE,
                                                     db,
                                                     InformationSchemaSprocConstants.GET_INFORMATION_SCHEMA_CONSTRAINT_COLUMN_USAGE);

            if (db.StoredProcedures.Contains(Sproc.Mapping.InformationSchemaConstraintColumnUsage.INFORMATION_SCHEMA_CONSTRAINT_COLUMN_USAGE))
            {
                db.StoredProcedures[Sproc.Mapping.InformationSchemaConstraintColumnUsage.INFORMATION_SCHEMA_CONSTRAINT_COLUMN_USAGE].Drop();
            }
            sp.Create();

            sp = GetInformationSchemaStoredProcedure(Sproc.Mapping.All.INFORMATION_SCHEMA,
                                                     db,
                                                     InformationSchemaSprocConstants.GET_INFORMATION_SCHEMA);

            if (db.StoredProcedures.Contains(Sproc.Mapping.All.INFORMATION_SCHEMA))
            {
                db.StoredProcedures[Sproc.Mapping.All.INFORMATION_SCHEMA].Drop();
            }
            sp.Create();


            sp = GetInformationSchemaIsIdentityStoredProcedure(Sproc.Mapping.InformationSchemaHelper.IS_IDENTITY,
                                                               db,
                                                               InformationSchemaSprocConstants.IS_IDENTITY_COLUMN);

            if(db.StoredProcedures.Contains(Sproc.Mapping.InformationSchemaHelper.IS_IDENTITY))
            {
                db.StoredProcedures[Sproc.Mapping.InformationSchemaHelper.IS_IDENTITY].Drop();
            }
            sp.Create();



        }

        private StoredProcedure GetInformationSchemaStoredProcedure(string procedureName,
                                                                    Database db,
                                                                    string procedureBody)
        {
            StoredProcedure sp = new StoredProcedure(db, procedureName);
            StoredProcedureParameter tableName = new StoredProcedureParameter(sp,
                Sproc.Mapping.FilterParameter.TABLE_NAME_PARAM, DataType.NVarChar(255));

            List<StoredProcedureParameter> parameters = new List<StoredProcedureParameter>();
            parameters.Add(tableName);

            sp.TextMode = false;
            sp.AnsiNullsStatus = false;
            sp.QuotedIdentifierStatus = false;
            sp.TextBody = procedureBody;

            foreach (StoredProcedureParameter parameter in parameters)
            {
                tableName.DefaultValue = "null";
                sp.Parameters.Add(parameter);
            }


            return sp;
        }

        private StoredProcedure GetInformationSchemaIsIdentityStoredProcedure(string procedureName,
                                                            Database db,
                                                            string procedureBody
                                                            )
        {
            StoredProcedure sp = new StoredProcedure(db, procedureName);
            StoredProcedureParameter tableName = new StoredProcedureParameter(sp,
                Sproc.Mapping.FilterParameter.TABLE_NAME_PARAM, DataType.NVarChar(255));
            StoredProcedureParameter columnName = new StoredProcedureParameter(sp,
                                                                               Sproc.Mapping.FilterParameter.COLUMN_NAME_PARAM,
                                                                               DataType.NVarChar(255));
            StoredProcedureParameter isIdentity = new StoredProcedureParameter(sp,
                                                                               Sproc.Mapping.ReturnParameter.IS_IDENTITY,
                                                                               DataType.Bit);
            sp.TextMode = false;
            sp.AnsiNullsStatus = false;
            sp.QuotedIdentifierStatus = false;           
            isIdentity.IsOutputParameter = true;

            List<StoredProcedureParameter> parameters = new List<StoredProcedureParameter>();
            parameters.Add(tableName);
            parameters.Add(columnName);
            parameters.Add(isIdentity);

            sp.TextMode = false;
            sp.AnsiNullsStatus = false;
            sp.QuotedIdentifierStatus = false;
            sp.TextBody = procedureBody;

            foreach (StoredProcedureParameter parameter in parameters)
            {
                tableName.DefaultValue = "null";
                columnName.DefaultValue = "null";
                isIdentity.DefaultValue = "0";
                sp.Parameters.Add(parameter);
            }

            return sp;
        }

        private void SetMetaDataList(string connectionString)
        {
            SprocDataLayerGeneratorDataAccess dataAccess = new SprocDataLayerGeneratorDataAccess();
            _metaDataList = dataAccess.GetSchema(connectionString);
        }

        private void SetMetaDataList(string tableName, string connectionString)
        {
            SprocDataLayerGeneratorDataAccess dataAccess = new SprocDataLayerGeneratorDataAccess();
            MetaInformationSchema metaInformationSchema = dataAccess.GetSchema(connectionString, tableName);
            _metaDataList.Add(metaInformationSchema);
        }

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

    }


}
