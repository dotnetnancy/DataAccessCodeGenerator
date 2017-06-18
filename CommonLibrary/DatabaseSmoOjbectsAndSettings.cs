using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Text;

using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Server;
using System.Runtime.Serialization;

namespace CommonLibrary
{
    [DataContract]
    public class DatabaseSmoObjectsAndSettings
    {

        private string _databaseName = string.Empty;

        public string DatabaseName
        {
            get { return _databaseName; }
            set { _databaseName = value; }
        }
        private string _dataSource = string.Empty;

        public string DataSource
        {
            get { return _dataSource; }
            set { _dataSource = value; }
        }
        private string _initialCatalog = string.Empty;

        public string InitialCatalog
        {
            get { return _initialCatalog; }
            set { _initialCatalog = value; }
        }
        private string _userId = string.Empty;

        public string UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }
        private string _password = string.Empty;

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
        private bool _trustedConnection = false;

        public bool TrustedConnection
        {
            get { return _trustedConnection; }
            set { _trustedConnection = value; }
        }

        private string _schema = string.Empty;

        public string Schema
        {
            get { return _schema; }
            set { _schema = value; }
        }

        SqlConnection _connection = null;

        public SqlConnection Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }
        ServerConnection _serverConnection = null;

        public ServerConnection ServerConnection_Property
        {
            get { return _serverConnection; }
            set { _serverConnection = value; }
        }
        Server _server = null;

        public Server Server_Property
        {
            get { return _server; }
            set { _server = value; }
        }
        Database _db = null;

        public Database Database_Property
        {
            get { return _db; }
            set { _db = value; }
        }
        string _connectionString = string.Empty;

        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        public DatabaseSmoObjectsAndSettings(string databaseName,
                                              string dataSource,
                                              string initialCatalog,
                                              string userId,
                                              string password,
                                              bool trustedConnection)
        {
            SetLocalProperties(databaseName, dataSource, initialCatalog, userId, 
                               password, trustedConnection);               
        }

        public DatabaseSmoObjectsAndSettings(string databaseName,
                                              string dataSource,
                                              string initialCatalog,
                                              string userId,
                                              string password,
                                              bool trustedConnection,
                                              string schema)
        {
            SetLocalProperties(databaseName, dataSource, initialCatalog, userId, 
                               password, trustedConnection, schema);
        }

        public void SetLocalProperties(string databaseName,
                  string dataSource,
                  string initialCatalog,
                  string userId,
                  string password,
                  bool trustedConnection)
        {
            _databaseName = databaseName;
            _dataSource = dataSource;
            _initialCatalog = initialCatalog;
            _userId = userId;
            _password = password;
            _trustedConnection = trustedConnection;

            _connectionString = BuildConnectionString(databaseName, dataSource, initialCatalog, userId, password, trustedConnection);

            _connection = new SqlConnection(_connectionString);
            _serverConnection = new ServerConnection(_connection);
            _server = new Server(_serverConnection);
            //huge performance hit if we do not do this
            _server.SetDefaultInitFields(typeof(Microsoft.SqlServer.Management.Smo.StoredProcedure), "IsSystemObject");
            _db = _server.Databases[databaseName];

        }

        public void SetLocalProperties(string databaseName,
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

            _connectionString = BuildConnectionString(databaseName, dataSource, initialCatalog, userId, password, trustedConnection);       

            _connection = new SqlConnection(_connectionString);
            _serverConnection = new ServerConnection(_connection);
            _server = new Server(_serverConnection);
            _server.SetDefaultInitFields(typeof(Microsoft.SqlServer.Management.Smo.StoredProcedure), "IsSystemObject");
            _db = _server.Databases[databaseName];

        }

        public string BuildConnectionString(string databaseName,
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

            return builder.ConnectionString;
        }
    }
}
