using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Microsoft.SqlServer.Management.Smo;

using Sql = CommonLibrary.Constants.SqlConstants;
using SqlDbConstants = CommonLibrary.Constants.SqlDbConstants;
using SqlNativeTypeConstants = CommonLibrary.Constants.NativeSqlConstants;
using SqlTypeConstants = CommonLibrary.Constants.SqlDbConstants;


namespace CommonLibrary.Utility
{
    public static class DatabaseHelperMethods
    {

        public static DatabaseSmoObjectsAndSettings BuildDatabaseSmoObjectsAndSettings(string databaseName,
                                                             string dataSource,
                                                             string initialCatalog,
                                                             string userId,
                                                             string password,
                                                             bool trustedConnection)
        {
            DatabaseSmoObjectsAndSettings smoObjectsAndSettings =
                new DatabaseSmoObjectsAndSettings(databaseName, dataSource, initialCatalog, 
                                                  userId, password, trustedConnection);
            return smoObjectsAndSettings;

        }

        public static DatabaseSmoObjectsAndSettings BuildDatabaseSmoObjectsAndSettings(string databaseName,
                                                             string dataSource,
                                                             string initialCatalog,
                                                             string userId,
                                                             string password,
                                                             bool trustedConnection,
                                                             string schema)
        {
            DatabaseSmoObjectsAndSettings smoObjectsAndSettings =
                new DatabaseSmoObjectsAndSettings(databaseName, dataSource, initialCatalog,
                                                  userId, password, trustedConnection,schema);
            return smoObjectsAndSettings;

        }

        public static string GenerateGetAllSprocName(string tableName)
        {
            string sprocName = string.Empty;
            sprocName = Sql.GET + tableName;
            return sprocName;
        }

        public static string GenerateGetByPrimaryKeySprocName(string tableName)
        {
            string sprocName = string.Empty;
            sprocName = Sql.GET + tableName + Sql.BY_PK;
            return sprocName;
        }

        public static string GenerateGetByCriteriaFuzzySprocName(string tableName)
        {
            string sprocName = string.Empty;
            sprocName = Sql.GET + tableName + Sql.BY_CRITERIA_FUZZY;
            return sprocName;
        }

        public static string GenerateGetByCriteriaExactSprocName(string tableName)
        {
            string sprocName = string.Empty;
            sprocName = Sql.GET + tableName + Sql.BY_CRITERIA_EXACT;
            return sprocName;
        }

        public static string GenerateInsertSprocName(string tableName)
        {
            string sprocName = string.Empty;
            sprocName = Sql.INSERT + tableName;
            return sprocName;
        }

        public static string GenerateUpdateByPrimaryKeySprocName(string tableName)
        {
            string sprocName = string.Empty;
            sprocName = Sql.UPDATE + tableName + Sql.BY_PK;
            return sprocName;
        }

        public static string GenerateDeleteByPrimaryKeySprocName(string tableName)
        {
            string sprocName = string.Empty;
            sprocName = Sql.DELETE + tableName + Sql.BY_PK;
            return sprocName;
        }

        public static bool IsIdentityColumn(string tableName, 
                                            string columnName,
                                            DatabaseSmoObjectsAndSettings databaseSmoObjectsAndSettings)
        {
            bool isIdentity = false;
            CommonLibrary.Base.Database.BaseDatabase baseDatabase =
                new CommonLibrary.Base.Database.BaseDatabase();

            SqlParameter paramTableName = new SqlParameter("@TableName", SqlDbType.NVarChar,255);
            paramTableName.Value = tableName;

            SqlParameter paramColumnName = new SqlParameter("@ColumnName", SqlDbType.NVarChar,255);
            paramColumnName.Value = columnName;

            SqlParameter outIsIdentity = new SqlParameter("@IsIdentity", SqlDbType.Bit);
            outIsIdentity.Direction = ParameterDirection.Output;

            SqlParameter[] parameters = new SqlParameter[3];

            parameters[0] = paramTableName;
            parameters[1] = paramColumnName;
            parameters[2] = outIsIdentity;           
        
            using (SqlConnection sqlConnection = new SqlConnection(databaseSmoObjectsAndSettings.ConnectionString))
            {
                sqlConnection.ConnectionString = databaseSmoObjectsAndSettings.ConnectionString;
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand("IsIdentity", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                foreach (SqlParameter sqlParameter in parameters)
                {
                    sqlCommand.Parameters.Add(sqlParameter);
                }
                
                sqlCommand.ExecuteNonQuery();
                isIdentity = (bool)sqlCommand.Parameters["@IsIdentity"].Value;
                sqlCommand.Dispose();
            }       
            
            return isIdentity;
        }

        public static bool IsIdentityColumn(string tableName,
                                    string columnName,
                                    string connectionString)
        {
            bool isIdentity = false;
            CommonLibrary.Base.Database.BaseDatabase baseDatabase =
                new CommonLibrary.Base.Database.BaseDatabase();

            SqlParameter paramTableName = new SqlParameter("@TableName", SqlDbType.NVarChar, 255);
            paramTableName.Value = tableName;

            SqlParameter paramColumnName = new SqlParameter("@ColumnName", SqlDbType.NVarChar, 255);
            paramColumnName.Value = columnName;

            SqlParameter outIsIdentity = new SqlParameter("@IsIdentity", SqlDbType.Bit);
            outIsIdentity.Direction = ParameterDirection.Output;

            SqlParameter[] parameters = new SqlParameter[3];

            parameters[0] = paramTableName;
            parameters[1] = paramColumnName;
            parameters[2] = outIsIdentity;

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.ConnectionString = connectionString;
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand("IsIdentity", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                foreach (SqlParameter sqlParameter in parameters)
                {
                    sqlCommand.Parameters.Add(sqlParameter);
                }

                sqlCommand.ExecuteNonQuery();
                isIdentity = (bool)sqlCommand.Parameters["@IsIdentity"].Value;
                sqlCommand.Dispose();
            }

            return isIdentity;
        }

        public static string GetColumnNameFromSqlParameterName(string parameterName)
        {
            string parameterNameWithoutAtSymbol = parameterName.Remove(0, 1);
            return parameterNameWithoutAtSymbol;
        }

        public static Type GetTypeFromSqlDbType(SqlDbType sqlDbType)
        {
            Type typeToReturn = null;

                switch (sqlDbType.ToString())
                {
                    case SqlTypeConstants.IMAGE:
                        {
                            typeToReturn = typeof(System.Drawing.Image);
                            break;
                        }
                    case SqlTypeConstants.TEXT:
                        {
                            typeToReturn = typeof(String);
                            break;
                        }
                    case SqlTypeConstants.TINYINT:
                        {
                            typeToReturn = typeof(Byte);
                            break;
                        }
                    case SqlTypeConstants.SMALLINT:
                        {
                            typeToReturn = typeof(Int16);
                            break;
                        }
                    case SqlTypeConstants.INT:
                        {
                            typeToReturn = typeof(Int32);
                            break;
                        }
                    case SqlTypeConstants.SMALLDATETIME:
                        {
                            typeToReturn = typeof(DateTime);
                            break;
                        }
                    case SqlTypeConstants.REAL:
                        {
                            typeToReturn = typeof(Single);
                            break;
                        }
                    case SqlTypeConstants.MONEY:
                        {
                            typeToReturn = typeof(Decimal);
                            break;
                        }
                    case SqlTypeConstants.DATETIME:
                        {
                            typeToReturn = typeof(DateTime);
                            break;
                        }
                    case SqlTypeConstants.FLOAT:
                        {
                            typeToReturn = typeof(Double);
                            break;
                        }
                    
                    case SqlTypeConstants.BIT:
                        {
                            typeToReturn = typeof(Boolean);
                            break;
                        }
                    case SqlTypeConstants.DECIMAL:
                        {
                           typeToReturn = typeof(Decimal);
                            break;
                        }
                    case SqlTypeConstants.SMALLMONEY:
                        {
                            typeToReturn = typeof(Decimal);
                            break;
                        }
                    case SqlTypeConstants.BIGINT:
                        {
                            typeToReturn = typeof(Int64);
                            break;
                        }
                    case SqlTypeConstants.VARBINARY:
                        {
                            typeToReturn = typeof(Byte []);
                            break;
                        }
                    case SqlTypeConstants.VARCHAR:
                        {
                            typeToReturn = typeof(String);
                            break;
                        }
                    case SqlTypeConstants.SQL_VARIANT:
                        {
                           typeToReturn = typeof(Object);
                            break;
                        }
                    case SqlTypeConstants.BINARY:
                        {
                            typeToReturn = typeof(Byte []);
                            break;
                        }
                    case SqlTypeConstants.CHAR:
                        {
                            typeToReturn = typeof(String);
                            break;
                        }
                    case SqlTypeConstants.NVARCHAR:
                        {
                           typeToReturn = typeof(String);
                            break;
                        }
                    case SqlTypeConstants.NCHAR:
                        {
                           typeToReturn = typeof(String);
                            break;
                        }
                    case SqlTypeConstants.UNIQUEIDENTIFIER:
                        {
                           typeToReturn = typeof(Guid);
                            break;
                        }
                    case SqlTypeConstants.NUMERIC:
                        {
                            typeToReturn = typeof(Decimal);
                            break;
                        }

                }
                return typeToReturn;
        }

        public static SqlDbType GetSqlDbTypeFromStoredProcedureParameterDataType(Microsoft.SqlServer.Management.Smo.DataType smoType)
        {
            SqlDbType sqlDbType = SqlDbType.Variant;
            string smoTypeString = smoType.SqlDataType.ToString();
            switch (smoTypeString)
            {
                case SqlDbConstants.BIGINT:
                    {
                        sqlDbType = SqlDbType.BigInt;
                        break;
                    }
                case SqlDbConstants.BINARY:
                    {
                        sqlDbType = SqlDbType.Binary;
                        break;
                    }
                case SqlDbConstants.BIT:
                    {
                        sqlDbType = SqlDbType.Bit;
                        break;
                    }
                case SqlDbConstants.CHAR:
                    {
                        sqlDbType = SqlDbType.Char;
                        break;
                    }
                case SqlDbConstants.DATETIME:
                    {
                        sqlDbType = SqlDbType.DateTime;
                        break;
                    }
                case SqlDbConstants.DECIMAL:
                    {
                        sqlDbType = SqlDbType.Decimal;
                        break;
                    }
                case SqlDbConstants.FLOAT:
                    {
                        sqlDbType = SqlDbType.Float;
                        break;
                    }
                case SqlDbConstants.IMAGE:
                    {
                        sqlDbType = SqlDbType.Image;
                        break;
                    }
                case SqlDbConstants.INT:
                    {
                        sqlDbType = SqlDbType.Int;
                        break;
                    }
                case SqlDbConstants.MONEY:
                    {
                        sqlDbType = SqlDbType.Money;
                        break;
                    }
                case SqlDbConstants.NCHAR:
                    {
                        sqlDbType = SqlDbType.NChar;
                        break;
                    }
                case SqlDbConstants.NTEXT:
                    {
                        sqlDbType = SqlDbType.NText;
                        break;
                    }
                case SqlDbConstants.NUMERIC:
                    {
                        sqlDbType = SqlDbType.Decimal;
                        break;
                    }
                case SqlDbConstants.NVARCHAR:
                    {
                        sqlDbType = SqlDbType.NVarChar;
                        break;
                    }
                case SqlDbConstants.REAL:
                    {
                        sqlDbType = SqlDbType.Real;
                        break;
                    }
                case SqlDbConstants.SMALLDATETIME:
                    {
                        sqlDbType = SqlDbType.SmallDateTime;
                        break;
                    }
                case SqlDbConstants.SMALLINT:
                    {
                        sqlDbType = SqlDbType.SmallInt;
                        break;
                    }
                case SqlDbConstants.SMALLMONEY:
                    {
                        sqlDbType = SqlDbType.SmallMoney;
                        break;
                    }
                case SqlDbConstants.SQL_VARIANT:
                    {
                        sqlDbType = SqlDbType.Variant;
                        break;
                    }
                case SqlDbConstants.TEXT:
                    {
                        sqlDbType = SqlDbType.Text;
                        break;
                    }
                case SqlDbConstants.TIMESTAMP:
                    {
                        sqlDbType = SqlDbType.Timestamp;
                        break;
                    }
                case SqlDbConstants.TINYINT:
                    {
                        sqlDbType = SqlDbType.TinyInt;
                        break;
                    }
                case SqlDbConstants.UNIQUEIDENTIFIER:
                    {
                        sqlDbType = SqlDbType.UniqueIdentifier;
                        break;
                    }
                case SqlDbConstants.VARBINARY:
                    {
                        sqlDbType = SqlDbType.VarBinary;
                        break;
                    }
                case SqlDbConstants.VARCHAR:
                    {
                        sqlDbType = SqlDbType.VarChar;
                        break;
                    }
                case SqlDbConstants.VARIANT:
                    {
                        sqlDbType = SqlDbType.Variant;
                        break;
                    }
            }
            return sqlDbType;
        }
    }
}
