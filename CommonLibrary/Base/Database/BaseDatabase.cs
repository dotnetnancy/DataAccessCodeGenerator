using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Reflection;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Text;

namespace CommonLibrary.Base.Database
{

    public class BeginTransactionException : ApplicationException
    {
        public BeginTransactionException()
            : base()
        {
        }
        public BeginTransactionException(string message)
            : base(message)
        {
        }
        public BeginTransactionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

    }
    public class BaseDatabase   
    {

        public enum TypeOfSql
        {
            Dynamic = 1,
            StoredProcedure = 2
        }

        public enum TypeOfExecution
        {
            NonQuery = 1,
            Scalar = 2
        }

        public enum TransactionBehavior
        {
            Begin = 1,
            Enlist = 2
        }

        private DateTime defaultDateTime = new DateTime(1900, 1, 1, 1, 1, 1, 1);

        public BaseDatabase()
        {
        }
        public const string DOT_DOT = "..";
        public const char GET_ALL = '*';
        public DataTable sortDataTable(DataTable table, string sortColumnName, bool sortInAscendingOrder)
        {
            if (sortInAscendingOrder)
            {
                table.DefaultView.Sort = sortColumnName + " asc ";
            }
            else
            {
                table.DefaultView.Sort = sortColumnName + " desc ";
            }
            return table;
        }

        public int[] getIntArrayFromDatatable(DataTable tableWithData)
        {
            int[] returnInt = new int[tableWithData.Rows.Count];
            int indexNumber = 0;
            foreach (DataRow oRow in tableWithData.Rows)
            {
                returnInt[indexNumber] = this.getInt(oRow, 0);
                indexNumber++;
            }

            return returnInt;

        }

        public Guid[] getGuidArrayFromDatatable(DataTable tableWithData)
        {
            Guid[] returnGuid = new Guid[tableWithData.Rows.Count];
            int indexNumber = 0;
            foreach (DataRow oRow in tableWithData.Rows)
            {
                returnGuid[indexNumber] = this.getGuid(oRow, 0);
                indexNumber++;
            }

            return returnGuid;

        }
        public string[] getStringArrayFromDatatable(DataTable tableWithData)
        {
            string[] returnString = new string[tableWithData.Rows.Count];
            int indexNumber = 0;
            foreach (DataRow oRow in tableWithData.Rows)
            {
                returnString[indexNumber] = this.getString(oRow, 0);
                indexNumber++;
            }

            return returnString;

        }

        public int executeNonQueryStoredProcedure(string connectionString, 
                                                    string storedProcedure, 
                                                    SqlParameter sqlParameter)
        {

            int returnCode = 0;
            int numberOfRowsAffected = 0;
            SqlParameter [] sqlParameters = new SqlParameter[] {sqlParameter};

            List<string> errors = new List<string>();

            errors = ExecuteNonQueryStoredProcedure(out numberOfRowsAffected,
                                            out returnCode,
                                            connectionString,
                                            storedProcedure,
                                            sqlParameters);

            if (errors.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string strError in errors)
                {
                    sb.Append(strError);
                    sb.Append(Environment.NewLine);
                }

                throw new ApplicationException(sb.ToString());

            }

            return returnCode;

        }

        public int executeNonQueryStoredProcedure(string connectionString,
                                                   string storedProcedure,
                                                   SqlParameter[] sqlParameters)
        {

            int returnCode = 0;
            int numberOfRowsAffected = 0;

            List<string> errors = new List<string>();

            errors = ExecuteNonQueryStoredProcedure(out numberOfRowsAffected,
                                            out returnCode,
                                            connectionString,
                                            storedProcedure,
                                            sqlParameters);

            if (errors.Count > 0)
            {
                 StringBuilder sb = new StringBuilder();
                foreach(string strError in errors)
                {
                    sb.Append(strError);
                    sb.Append(Environment.NewLine);
                }

                throw new ApplicationException(sb.ToString());

            }

            return returnCode;

        }

        public int executeNonQueryStoredProcedure(string connectionString,
                                           string storedProcedure)
        {

            int returnCode = 0;
            int numberOfRowsAffected = 0;
            ExecuteNonQueryStoredProcedure(out numberOfRowsAffected,
                                            out returnCode,
                                            connectionString,
                                            storedProcedure);

            return returnCode;

        }

        public void ExecuteNonQueryDynamicSql(string connectionString,
                                              string sqlString)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.ConnectionString = connectionString;
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(sqlString, sqlConnection);
                sqlCommand.CommandType = CommandType.Text;               

                sqlCommand.ExecuteNonQuery();               
                sqlCommand.Dispose();
            }          

        }
                                             


        public int ExecuteNonQueryStoredProcedure(out int numberOfRowsAffected,
                                            string connectionString,
                                            string storedProcedure,
                                            SqlParameter [] parameters)
        {
            int returnCode = 0;

            List<string> errors = new List<string>();

            errors = ExecuteNonQueryStoredProcedure(out returnCode,
                                            out numberOfRowsAffected,
                                            connectionString,
                                            storedProcedure,
                                            parameters);

            if (errors.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string strError in errors)
                {
                    sb.Append(strError);
                    sb.Append(Environment.NewLine);
                }

                throw new ApplicationException(sb.ToString());

            }
            return returnCode;
            

        }

        public List<string> ExecuteNonQueryStoredProcedure(out int returnCode,
                                                    out int numberOfRowsAffected,
                                                    string connectionString,
                                                    string storedProcedure,
                                                    SqlParameter [] parameters)
        {
            List<string> errors = new List<string>();
            numberOfRowsAffected = 0;
            returnCode = -1;
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConnection.ConnectionString = connectionString;
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand(storedProcedure, sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    foreach (SqlParameter sqlParameter in parameters)
                    {
                        if (sqlParameter.SqlDbType == SqlDbType.Image)
                        {
                            byte[] imageByteArray; 
                            Image imageToConvert = (Image)sqlParameter.Value;
                           
                            try
                            {
                                using (MemoryStream ms = new MemoryStream())
                                {                                                                       
                                    try
                                    {
                                        imageToConvert.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                        imageByteArray = ms.ToArray();
                                        sqlParameter.Value = imageByteArray;
                                    }
                                        //this is the crappiest workaround i have ever seen, however
                                        //microsoft claims that this is by design - the problem is that when
                                        // you open an image "FromFile" the handle does not get released
                                        //and then you cannot save "back to the same spot", therefore
                                        //this is the only way to make a "true copy" and then be able to 
                                        //get the byte array associated with the image
                                    catch (System.Runtime.InteropServices.ExternalException invalidEx)
                                    {
                                        Bitmap original = (Bitmap)imageToConvert;
                                        Bitmap ret = new Bitmap(original.Width, original.Height, PixelFormat.Format24bppRgb);
                                        // copy the original image into the new one
                                        using (Graphics g = Graphics.FromImage(ret))
                                        {
                                            g.DrawImage(original, 0, 0);
                                        }
                                        //return ret;
                                        ret.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                        imageByteArray = ms.ToArray();
                                        sqlParameter.Value = imageByteArray;                                        
                                      
                                    }                                  
                                }
                            }
                            catch (System.Runtime.InteropServices.ExternalException invalidEx)
                            {                              

                                errors.Add(invalidEx.Message + invalidEx.StackTrace);
                            }
                            catch (Exception ex)
                            {
                                errors.Add(ex.Message + ex.StackTrace);
                            }                                                      
                        }
                        sqlCommand.Parameters.Add(sqlParameter);                        
                    }

                    numberOfRowsAffected = sqlCommand.ExecuteNonQuery();
                    returnCode = 0; //TODO - Decide how to handle return values (int)sqlCommand.Parameters["@RETURN_VALUE"].Value;
                    sqlCommand.Dispose();
                }
                catch (SqlException sqlEx)
                {
                    for (int i = 0; i < sqlEx.Errors.Count; i++)
                    {
                        errors.Add(sqlEx.Errors[i].ToString());
                    }
                }
                catch (InvalidOperationException iEx)
                {
                    errors.Add(iEx.Message + iEx.StackTrace);
                }
                catch (InvalidCastException castEx)
                {
                    errors.Add(castEx.Message + castEx.StackTrace);
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message + ex.StackTrace);
                }
            }

            return errors;
        }

        public List<string> ExecuteNonQueryStoredProcedure(out int returnCode,
                                            out int numberOfRowsAffected,
                                            string connectionString,
                                            string storedProcedure)
        {
            List<string> errors = new List<string>();
            numberOfRowsAffected = 0;
            returnCode = -1;
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConnection.ConnectionString = connectionString;
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand(storedProcedure, sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                   
                    numberOfRowsAffected = sqlCommand.ExecuteNonQuery();
                    returnCode = 0; //TODO - Decide how to handle return values (int)sqlCommand.Parameters["@RETURN_VALUE"].Value;
                    sqlCommand.Dispose();
                }
                catch (SqlException sqlEx)
                {
                    for (int i = 0; i < sqlEx.Errors.Count; i++)
                    {
                        errors.Add(sqlEx.Errors[i].ToString());
                    }
                }
                catch (InvalidOperationException iEx)
                {
                    errors.Add(iEx.Message + iEx.StackTrace);
                }
            }

            return errors;
        }


       
        public DataSet getDatasetFromSP(string connectionString, string storedProcedure, SqlParameter[] sqlParameters)
        {
            DataSet dataset = new DataSet();

            // Load SP into datatable
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                SqlCommand sqlCommand = new SqlCommand(storedProcedure, sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                foreach (SqlParameter sqlParameter in sqlParameters)
                {
                    //command.Parameters.Add(sqlParameter);
                    SqlParameter newParameter = new SqlParameter(sqlParameter.ParameterName,
                        sqlParameter.SqlDbType,
                        sqlParameter.Size,
                        sqlParameter.Direction,
                        sqlParameter.IsNullable,
                        sqlParameter.Precision,
                        sqlParameter.Scale,
                        sqlParameter.SourceColumn,
                        sqlParameter.SourceVersion,
                        sqlParameter.Value);
                    sqlCommand.Parameters.Add(newParameter);
                }

                using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
                {
                    sqlDataAdapter.Fill(dataset);
                }
                sqlCommand.Dispose();
            }
            return dataset;

        }

        public DataSet getDatasetFromSP(string connectionString, string storedProcedure, SqlParameter sqlParameter)
        {
            DataSet dataset = new DataSet();

            // Load SP into datatable
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                SqlCommand sqlCommand = new SqlCommand(storedProcedure, sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.Add(sqlParameter);

                using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
                {
                    sqlDataAdapter.Fill(dataset);
                }
                sqlCommand.Dispose();
            }
            return dataset;

        }
        public SqlParameter getInputVarcharParameter(string parameterValue, string parameterName)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = parameterName;
            sqlParameter.SqlDbType = SqlDbType.NVarChar;
            sqlParameter.Direction = ParameterDirection.Input;
            if (parameterValue != null)
            {
                sqlParameter.Value = parameterValue;
            }
            else
            {
                sqlParameter.Value = DBNull.Value;
            }
            return sqlParameter;
        }

        public SqlParameter getInputNTextParameter(string parameterValue, string parameterName)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = parameterName;
            sqlParameter.SqlDbType = SqlDbType.NText;
            sqlParameter.Direction = ParameterDirection.Input;
            if (parameterValue != null)
            {
                sqlParameter.Value = parameterValue;
            }
            else
            {
                sqlParameter.Value = DBNull.Value;
            }


            return sqlParameter;
        }


        public SqlParameter getInputIntParameter(int parameterValue, string parameterName)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = parameterName;
            sqlParameter.SqlDbType = SqlDbType.Int;
            sqlParameter.Direction = ParameterDirection.Input;
            sqlParameter.Value = parameterValue;

            return sqlParameter;
        }

        public SqlParameter getInputIntNullableParameter(int? parameterValue, string parameterName)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = parameterName;
            sqlParameter.SqlDbType = SqlDbType.Int;
            sqlParameter.Direction = ParameterDirection.Input;
            if (parameterValue.HasValue)
            {
                sqlParameter.Value = parameterValue.Value;
            }
            else
            {
                sqlParameter.Value = DBNull.Value;
            }
            return sqlParameter;
        }

        public SqlParameter getInputGuidParameter(Guid parameterValue, string parameterName)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = parameterName;
            sqlParameter.SqlDbType = SqlDbType.UniqueIdentifier;
            sqlParameter.Direction = ParameterDirection.Input;
            sqlParameter.Value = new SqlGuid(parameterValue.ToByteArray());
            return sqlParameter;
        }

        public SqlParameter getInputCharParameter(char parameterValue, string parameterName)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = parameterName;
            sqlParameter.SqlDbType = SqlDbType.Char;
            sqlParameter.Direction = ParameterDirection.Input;
            sqlParameter.Value = parameterValue;

            return sqlParameter;
        }

        public SqlParameter getInputNVarCharParameter(string parameterValue, string parameterName)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = parameterName;
            sqlParameter.SqlDbType = SqlDbType.NVarChar;
            sqlParameter.Direction = ParameterDirection.Input;
            if (parameterValue != null)
            {
                sqlParameter.Value = parameterValue;
            }
            else
            {
                sqlParameter.Value = DBNull.Value;
            }


            return sqlParameter;
        }

        public SqlParameter getReturnIntParameter()
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = "@RETURN_VALUE";
            sqlParameter.SqlDbType = SqlDbType.Int;
            sqlParameter.Direction = ParameterDirection.ReturnValue;

            return sqlParameter;
        }

        public SqlParameter getInputDateTimeParameter(DateTime parameterValue, string parameterName)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = parameterName;
            sqlParameter.SqlDbType = SqlDbType.DateTime;
            sqlParameter.Direction = ParameterDirection.Input;
            sqlParameter.Value = parameterValue;

            return sqlParameter;
        }

        public SqlParameter getInputBoolParameter(bool parameterValue, string parameterName)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = parameterName;
            sqlParameter.SqlDbType = SqlDbType.Bit;
            sqlParameter.Direction = ParameterDirection.Input;
            sqlParameter.Value = parameterValue;

            return sqlParameter;
        }

        public SqlParameter getInputBoolNullableParameter(bool? parameterValue, string parameterName)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = parameterName;
            sqlParameter.SqlDbType = SqlDbType.Bit;
            sqlParameter.Direction = ParameterDirection.Input;
            if (parameterValue.HasValue)
            {
                sqlParameter.Value = parameterValue.Value;
            }
            else
            {
                sqlParameter.Value = DBNull.Value;
            }


            return sqlParameter;
        }

        public SqlParameter getInputDoubleParameter(double parameterValue, string parameterName)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = parameterName;
            sqlParameter.SqlDbType = SqlDbType.Real;
            sqlParameter.Direction = ParameterDirection.Input;
            sqlParameter.Value = parameterValue;

            return sqlParameter;
        }

        public SqlParameter getInputDoubleNullableParameter(double? parameterValue, string parameterName)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = parameterName;
            sqlParameter.SqlDbType = SqlDbType.Real;
            sqlParameter.Direction = ParameterDirection.Input;

            if (parameterValue.HasValue)
            {
                sqlParameter.Value = parameterValue.Value;
            }
            else
            {
                sqlParameter.Value = DBNull.Value;
            }


            return sqlParameter;
        }

        public SqlParameter getOutputBoolParameter(string parameterName)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = parameterName;
            sqlParameter.SqlDbType = SqlDbType.Bit;
            sqlParameter.Direction = ParameterDirection.Output;

            return sqlParameter;
        }
        public SqlParameter getOutputDateTimeParameter(string parameterName)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = parameterName;
            sqlParameter.SqlDbType = SqlDbType.DateTime;
            sqlParameter.Direction = ParameterDirection.Output;

            return sqlParameter;
        }
        public SqlParameter getOutputIntParameter(string parameterName)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = parameterName;
            sqlParameter.SqlDbType = SqlDbType.Int;
            sqlParameter.Direction = ParameterDirection.Output;

            return sqlParameter;
        }

        /// <summary>
        /// returns a datareader from a string of sql
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public SqlDataReader getDataReaderFromStringOfSql(string connectionString, string sql)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sql, connection);
            command.CommandType = CommandType.Text;
            connection.Open();
            SqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
            return reader;
        }   

        /// <summary>
        /// returns a SqlDataReader from a stored procedure
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="storedProcedure"></param>
        /// <returns></returns>
        public SqlDataReader getDataReaderFromSP(string connectionString, string storedProcedure)
        {
           SqlConnection connection = new SqlConnection(connectionString);
            
                SqlCommand command = new SqlCommand(storedProcedure, connection);
                command.CommandType = CommandType.StoredProcedure;
                
                connection.Open();

                //SqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);  
                SqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);  
         
                return reader;   
        }

        public DataSet getDatasetFromSP(string connectionString, string storedProcedure)
        {
            DataSet dataset = new DataSet();

            // Load SP into datatable
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                SqlCommand sqlCommand = new SqlCommand(storedProcedure, sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
                {
                    sqlDataAdapter.Fill(dataset);
                }
                sqlCommand.Dispose();
            }
            return dataset;

        }

        /// <summary>
        /// returns a SqlDataReader from a stored procedure using a given array of parameters
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="storedProcedure"></param>
        /// <param name="sqlParameters"></param>
        /// <returns></returns>
        public SqlDataReader getDataReaderFromSP(string connectionString, string storedProcedure, 
            SqlParameter[] sqlParameters)
        {
            SqlConnection connection = new SqlConnection(connectionString);

            SqlCommand command = new SqlCommand(storedProcedure, connection);
            command.CommandType = CommandType.StoredProcedure;

            foreach (SqlParameter sqlParameter in sqlParameters)
            {
                //command.Parameters.Add(sqlParameter);
                SqlParameter newParameter = new SqlParameter(sqlParameter.ParameterName,
                    sqlParameter.SqlDbType,
                    sqlParameter.Size,
                    sqlParameter.Direction,
                    sqlParameter.IsNullable,
                    sqlParameter.Precision,
                    sqlParameter.Scale,
                    sqlParameter.SourceColumn,
                    sqlParameter.SourceVersion,
                    sqlParameter.Value);
                    command.Parameters.Add(newParameter);
            }

            connection.Open();
            
            SqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
            
            return reader;   
        }

        /// <summary>
        /// returns a SqlDataReader from a stored procedure using a given array of parameters
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="storedProcedure"></param>
        /// <param name="sqlParameters"></param>
        /// <returns></returns>
        public SqlDataReader getDataReaderFromSP(string storedProcedure,
            SqlConnection connection,
            ref SqlTransaction transaction,
            TransactionBehavior transactionBehavior,
            TypeOfSql typeOfSql, 
            SqlParameter[] sqlParameters)
        {
            SqlDataReader reader = null;

            using (SqlCommand sqlCommand = new SqlCommand(storedProcedure, connection))
            {
                sqlCommand.CommandType = GetCommandTypeByTypeOfSql(typeOfSql);

                sqlCommand.Connection = connection;

                PrepareTransaction(connection, ref transaction, sqlCommand, transactionBehavior);
                foreach (SqlParameter sqlParameter in sqlParameters)
                {
                    //command.Parameters.Add(sqlParameter);
                    SqlParameter newParameter = new SqlParameter(sqlParameter.ParameterName,
                        sqlParameter.SqlDbType,
                        sqlParameter.Size,
                        sqlParameter.Direction,
                        sqlParameter.IsNullable,
                        sqlParameter.Precision,
                        sqlParameter.Scale,
                        sqlParameter.SourceColumn,
                        sqlParameter.SourceVersion,
                        sqlParameter.Value);
                        sqlCommand.Parameters.Add(newParameter);
                }
                reader = sqlCommand.ExecuteReader();
            }

            return reader;
        }

        public SqlDataReader getDataReaderFromSP(string storedProcedure,
                                                SqlConnection connection,
                                                ref SqlTransaction transaction,
                                                TransactionBehavior transactionBehavior,
                                                TypeOfSql typeOfSql)
        {
            SqlDataReader reader = null;

            using (SqlCommand sqlCommand = new SqlCommand(storedProcedure, connection))
            {
                sqlCommand.CommandType = GetCommandTypeByTypeOfSql(typeOfSql);

                sqlCommand.Connection = connection;

                PrepareTransaction(connection, ref transaction, sqlCommand, transactionBehavior);
                
                reader = sqlCommand.ExecuteReader();
            }

            return reader;
        }


        public DataTable getTableFromSP(string connectionString, string storedProcedure, SqlParameter[] sqlParameters)
        {
            DataTable table = new DataTable();

            // Load SP into datatable
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                SqlCommand sqlCommand = new SqlCommand(storedProcedure, sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                foreach (SqlParameter sqlParameter in sqlParameters)
                {
                    //command.Parameters.Add(sqlParameter);
                    SqlParameter newParameter = new SqlParameter(sqlParameter.ParameterName,
                        sqlParameter.SqlDbType,
                        sqlParameter.Size,
                        sqlParameter.Direction,
                        sqlParameter.IsNullable,
                        sqlParameter.Precision,
                        sqlParameter.Scale,
                        sqlParameter.SourceColumn,
                        sqlParameter.SourceVersion,
                        sqlParameter.Value);
                        sqlCommand.Parameters.Add(newParameter);
                }

                using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
                {
                    sqlDataAdapter.Fill(table);
                }
                sqlCommand.Dispose();
            }
            return table;

        }

        /// <summary>
        /// return a SqlDataReader from a stored procedure using a single parameter
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="storedProcedure"></param>
        /// <param name="sqlParameter"></param>
        /// <returns></returns>
        public SqlDataReader getDataReaderFromSP(string connectionString, string storedProcedure, SqlParameter sqlParameter)
        {
            SqlConnection connection = new SqlConnection(connectionString);

            SqlCommand command = new SqlCommand(storedProcedure, connection);
            command.CommandType = CommandType.StoredProcedure;           
            command.Parameters.Add(sqlParameter);            

            connection.Open();

            SqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);

            return reader;   
        }

        public DataTable getTableFromSP(string connectionString, string storedProcedure, SqlParameter sqlParameter)
        {
            DataTable table = new DataTable();

            // Load SP into datatable
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                SqlCommand sqlCommand = new SqlCommand(storedProcedure, sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.Add(sqlParameter);

                using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
                {
                    sqlDataAdapter.Fill(table);
                }
                sqlCommand.Dispose();
            }
            return table;

        }
        public DataTable getTableFromSP(string connectionString, string storedProcedure)
        {
            DataTable table = new DataTable();

            // Load SP into datatable
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                SqlCommand sqlCommand = new SqlCommand(storedProcedure, sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
                {
                    sqlDataAdapter.Fill(table);
                }
                sqlCommand.Dispose();
            }
            return table;

        }

        /// <summary>
        /// this method will return the value at the position of the reader for the column specified or it 
        /// will return null, as a System.Nullable Type
        /// </summary>
        /// <param name="column"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        [CommonLibrary.CustomAttributes.ResolveNullValue("System.Nullable<bool>", "null")]
        public System.Nullable<bool> resolveNullBooleanToNullableDataType(string column, SqlDataReader reader)
        {
            bool? data = null;

            if (!reader.IsDBNull(reader.GetOrdinal(column)))
            {
                data = Convert.ToBoolean(reader[column]);
            }
            return data;
        }

        [CommonLibrary.CustomAttributes.ResolveNullValue("System.Nullable<bool>", "null")]
        public System.Nullable<bool> resolveNullBooleanToNullableDataType(int columnIndex, SqlDataReader reader)
        {
            bool? data = null;

            if (!reader.IsDBNull(columnIndex))
            {
                data = Convert.ToBoolean(reader[columnIndex]);
            }
            return data;
        }

        /// <summary>
        /// resolves a null value from the database by converting the value of the reader at its current position
        /// for the specified column to a boolean value
        /// </summary>
        /// <param name="column"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        
        [CommonLibrary.CustomAttributes.ResolveNullValue("bool","0")]
        public bool resolveNullBoolean(string column, SqlDataReader reader)
        {
                    
              bool data = (reader.IsDBNull(reader.GetOrdinal(column)))
                                ? false : Convert.ToBoolean(reader[column]);          
            return data;
        }

        /// <summary>
        /// resolves a null value from the database by converting the value of the reader at its current position
        /// for the specified columnIndex to a boolean value
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        [CommonLibrary.CustomAttributes.ResolveNullValue("bool","0")]
        public bool resolveNullBoolean(int columnIndex, SqlDataReader reader)
        {

            bool data = (reader.IsDBNull(columnIndex))
                              ? false : Convert.ToBoolean(reader[columnIndex]);
            return data;
        }

        public bool getBool(DataRow row, string fieldName)
        {
            if (row[fieldName] != System.DBNull.Value)
            {
                return (bool)row[fieldName];
            }
            else
            {
                return false;
            }
        }
        public bool getBool(DataRowView row, string fieldName)
        {
            if (row[fieldName] != System.DBNull.Value)
            {
                return (bool)row[fieldName];
            }
            else
            {
                return false;
            }
        }

        public string getBinaryString(string fieldName, SqlDataReader reader)
        {
            System.Text.UnicodeEncoding decoder = new System.Text.UnicodeEncoding();
            string binaryValueAsString = string.Empty;
            if (reader[fieldName] != System.DBNull.Value)
            {
                SqlBinary binValue = (SqlBinary) reader.GetSqlBinary(reader.GetOrdinal(fieldName));
                binaryValueAsString = decoder.GetString(binValue.Value);
            }

            return binaryValueAsString;
        }

        /// <summary>
        /// this method will return the value at the position of the reader for the column specified or it 
        /// will return null, as a string which is a nullable type inherently
        /// </summary>
        /// <param name="column"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        [CommonLibrary.CustomAttributes.ResolveNullValue("string", "null")]
        public string resolveNullStringToNull(string column, SqlDataReader reader)
        {
            String data = (reader.IsDBNull(reader.GetOrdinal(column)))
                          ? null : reader[column].ToString();
            return data;
        }

        public byte resolveNullByteToMinValue(int columnIndex, SqlDataReader reader)
        {
            byte data = (reader.IsDBNull(columnIndex))
                ? byte.MinValue : Convert.ToByte(reader[columnIndex]);
            return data;
        }

        public char []  resolveNullCharToNull(int columnIndex, SqlDataReader reader)
        {
            char [] data = (reader.IsDBNull(columnIndex))
                          ? null : reader[columnIndex].ToString().ToCharArray();
            return data;
        }

        public char[] resolveNullChar(int columnIndex, SqlDataReader reader)
        {
            char[] data = (reader.IsDBNull(columnIndex))
                          ? Char.MinValue.ToString().ToCharArray() : reader[columnIndex].ToString().ToCharArray();
            return data;
        }


        /// <summary>
        /// this method will return the value at the position of the reader for the column index specified or it 
        /// will return null, as a string which is a nullable type inherently
        /// </summary>
        /// <param name="column"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        [CommonLibrary.CustomAttributes.ResolveNullValue("string", "null")]
        public string resolveNullStringToNull(int columnIndex, SqlDataReader reader)
        {
            String data = (reader.IsDBNull(columnIndex))
                          ? null : reader[columnIndex].ToString();
            return data;
        }

        /// <summary>
        /// this method will return the value at the position of the reader for the column specified or it 
        /// will return an empty string
        /// </summary>
        /// <param name="column"></param>
        /// <param name="reader"></param>
        /// <returns></returns>   
        [CommonLibrary.CustomAttributes.ResolveNullValue("string", "")]
        public string resolveNullString(string column, SqlDataReader reader)
        {           
            String data = (reader.IsDBNull(reader.GetOrdinal(column))) 
                           ? string.Empty : reader[column].ToString();
            return data;
        }

        /// <summary>
        /// this method will return the value at the position of the reader for the column index specified or it 
        /// will return an empty string
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="reader"></param>
        /// <returns></returns>   
        [CommonLibrary.CustomAttributes.ResolveNullValue("string", "")]
        public string resolveNullString(int columnIndex, SqlDataReader reader)
        {
            String data = (reader.IsDBNull(columnIndex))
                           ? string.Empty : reader[columnIndex].ToString();
            return data;
        }


        public string getString(DataRow row, string fieldName)
        {
            if (row[fieldName] != System.DBNull.Value)
            {
                return row[fieldName].ToString();
            }
            else
            {
                return "";
            }
        }
        public string getString(DataRow row, int fieldIndex)
        {
            if (row[fieldIndex] != System.DBNull.Value)
            {
                return row[fieldIndex].ToString();
            }
            else
            {
                return "";
            }
        }
        public string getString(DataRowView row, string fieldName)
        {
            if (row[fieldName] != System.DBNull.Value)
            {
                return row[fieldName].ToString();
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// this method will return the value at the position of the reader for the column specified or it 
        /// will return null, as a System.Nullable Type
        /// </summary>
        /// <param name="column"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        [CommonLibrary.CustomAttributes.ResolveNullValue(@"System.Nullable<System.DateTime>","null")]
        public System.Nullable<System.DateTime> resolveNullDateTimeToNullableDataType(string column, SqlDataReader reader)
        {
            DateTime? data = null;

            if (!reader.IsDBNull(reader.GetOrdinal(column)))
            {
                data = DateTime.Parse(reader[column].ToString());
            }
            return data;
        }

        [CommonLibrary.CustomAttributes.ResolveNullValue(@"System.Nullable<System.DateTime>", "null")]
        public System.Nullable<System.DateTime> resolveNullDateTimeToNullableDataType(int columnIndex, SqlDataReader reader)
        {
            DateTime? data = null;

            if (!reader.IsDBNull(columnIndex))
            {
                data = DateTime.Parse(reader[columnIndex].ToString());
            }
            return data;
        }

        /// <summary>
        /// this method will return the value at the position of the reader for the column specified or it 
        /// will return the "default date" as defined in this class
        /// </summary>
        /// <param name="column"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        [CommonLibrary.CustomAttributes.ResolveNullValue("System.DateTime",@"1900, 1, 1, 1, 1, 1, 1")]
        public DateTime resolveNullDateTime(string column, SqlDataReader reader)
        {
            DateTime data = (reader.IsDBNull(reader.GetOrdinal(column)))
                      ? defaultDateTime : DateTime.Parse(reader[column].ToString());
            return data;
        }


        /// <summary>
        /// this method will return the value at the position of the reader for the column index specified or it 
        /// will return the "default date" as defined in this class
        /// </summary>
        /// <param name="column"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        [CommonLibrary.CustomAttributes.ResolveNullValue("System.DateTime",@"1900, 1, 1, 1, 1, 1, 1")]
        public DateTime resolveNullDateTime(int columnIndex, SqlDataReader reader)
        {
            DateTime data = (reader.IsDBNull(columnIndex))
                      ? defaultDateTime : DateTime.Parse(reader[columnIndex].ToString());
            return data;
        }


        public DateTime getDateTime(DataRow row, string fieldName)
        {
            if (row[fieldName] != System.DBNull.Value)
            {
                return (DateTime)row[fieldName];
            }
            else
            {
                return defaultDateTime;
            }
        }
        public DateTime getDateTime(DataRowView row, string fieldName)
        {
            if (row[fieldName] != System.DBNull.Value)
            {
                return (DateTime)row[fieldName];
            }
            else
            {
                return defaultDateTime;
            }
        }

        /// <summary>
        /// this method will return the value at the position of the reader for the column specified or it 
        /// will return null, as a System.Nullable Type
        /// </summary>
        /// <param name="column"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        [CommonLibrary.CustomAttributes.ResolveNullValue(@"System.Nullable<int>", "null")]
        public System.Nullable<int> resolveNullInt32ToNullableDataType(string column, SqlDataReader reader)
        {
            int? data = null;

            if (!reader.IsDBNull(reader.GetOrdinal(column)))
            {
                data = Convert.ToInt32(reader[column]);
            }
            return data;
        }

        [CommonLibrary.CustomAttributes.ResolveNullValue(@"System.Nullable<int>", "null")]
        public System.Nullable<int> resolveNullInt32ToNullableDataType(int columnIndex, SqlDataReader reader)
        {
            int? data = null;

            if (!reader.IsDBNull(columnIndex))
            {
                data = Convert.ToInt32(reader[columnIndex]);
            }
            return (int?)data;
        }


        /// <summary>
        /// this method will return the value at the position of the reader for the column specified or it 
        /// will return 0
        /// </summary>
        /// <param name="column"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        [CommonLibrary.CustomAttributes.ResolveNullValue("int","0")]
        public int resolveNullInt32(string column, SqlDataReader reader)
        {
            int data = (reader.IsDBNull(reader.GetOrdinal(column)))
                           ? (int)0 : (int)reader[column];
            return data;
        }

        /// <summary>
        /// this method will return the value at the position of the reader for the columnIndex specified or it 
        /// will return 0
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        [CommonLibrary.CustomAttributes.ResolveNullValue("int", "null")]
        public int resolveNullInt32(int columnIndex, SqlDataReader reader)
        {
            int data = (reader.IsDBNull(columnIndex))
                           ? (int)0 : (int)reader[columnIndex];
            return data;
        }


        public int getInt(DataRow row, string fieldName)
        {
            if (row[fieldName] != System.DBNull.Value)
            {
                return (int)row[fieldName];
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// this method will return the value at the position of the reader for the column specified or it 
        /// will return null, as a System.Nullable Type
        /// </summary>
        /// <param name="column"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        [CommonLibrary.CustomAttributes.ResolveNullValue(@"System.Nullable<double>", "null")]
        public System.Nullable<double> resolveNullDoubleToNullableDataType(string column, SqlDataReader reader)
        {
            double? data = null;

            if (!reader.IsDBNull(reader.GetOrdinal(column)))
            {
                data = double.Parse( reader[column].ToString());
            }            
            return data;
        }

        /// <summary>
        /// this method will return the value at the position of the reader for the column index specified or it 
        /// will return null, as a System.Nullable Type
        /// </summary>
        /// <param name="column"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        [CommonLibrary.CustomAttributes.ResolveNullValue(@"System.Nullable<double>", "null")]
        public System.Nullable<double> resolveNullDoubleToNullableDataType(int columnIndex, SqlDataReader reader)
        {
            double? data = null;

            if (!reader.IsDBNull(columnIndex))
            {
                data = double.Parse(reader[columnIndex].ToString());
            }
            return data;
        }


        /// <summary>
        /// this method will return the value at the position of the reader for the column specified or it 
        /// will return 0
        /// </summary>
        /// <param name="column"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        [CommonLibrary.CustomAttributes.ResolveNullValue("double","0")]
        public double resolveNullDouble(string column, SqlDataReader reader)
        {
            double data = (reader.IsDBNull(reader.GetOrdinal(column)))
                ? 0 : double.Parse(reader[column].ToString());
            return data;
        }

        [CommonLibrary.CustomAttributes.ResolveNullValue("double", "0")]
        public double resolveNullDouble(int columnIndex , SqlDataReader reader)
        {
            double data = (reader.IsDBNull(columnIndex))
                ? 0 : double.Parse(reader[columnIndex].ToString());
            return data;
        }

        public double getDouble(DataRow row, string fieldName)
        {
            if (row[fieldName] != System.DBNull.Value)
            {
                return (double)row[fieldName];
            }
            else
            {
                return 0;
            }
        }

        public double getDouble(DataRowView row, string fieldName)
        {
            if (row[fieldName] != System.DBNull.Value)
            {
                return (double)row[fieldName];
            }
            else
            {
                return 0;
            }
        }

        public Guid getGuid(DataRow row, int fieldIndex)
        {
            if (row[fieldIndex] != System.DBNull.Value)
            {
                return (Guid)row[fieldIndex];
            }
            else
            {
                return Guid.Empty;
            }
        }

        public int getInt(DataRow row, int fieldIndex)
        {
            if (row[fieldIndex] != System.DBNull.Value)
            {
                return (int)row[fieldIndex];
            }
            else
            {
                return 0;
            }
        }

        public int getInt(DataRowView row, string fieldName)
        {
            if (row[fieldName] != System.DBNull.Value)
            {
                return (int)row[fieldName];
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// this method will return the value at the position of the reader for the column specified or it 
        /// will return null, as a System.Nullable Type
        /// </summary>
        /// <param name="column"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        [CommonLibrary.CustomAttributes.ResolveNullValue(@"System.Nullable<short>", "null")]
        public System.Nullable<short> resolveNullSmallIntToIntToNullableDataType(string column, SqlDataReader reader)
        {
            short? data = null;
            if (!reader.IsDBNull(reader.GetOrdinal(column)))
            {
                data = Int16.Parse(reader[column].ToString());
            }
            return data;
        }

        [CommonLibrary.CustomAttributes.ResolveNullValue(@"System.Nullable<short>", "null")]
        public System.Nullable<short> resolveNullSmallIntToNullableDataType(string column, SqlDataReader reader)
        {
            short? data = null;
            if (!reader.IsDBNull(reader.GetOrdinal(column)))
            {
                data = Int16.Parse(reader[column].ToString());
            }
            return data;
        }

        /// <summary>
        /// this method will return the value at the position of the reader for the column specified or it 
        /// will return (int)0
        /// </summary>
        /// <param name="column"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        [CommonLibrary.CustomAttributes.ResolveNullValue("short", "0")]
        public int resolveNullSmallIntToInt(string column, SqlDataReader reader)
        {
            int data = (reader.IsDBNull(reader.GetOrdinal(column)))
                          ? (int)0 : (int)Int16.Parse(reader[column].ToString());
            return data;

        }


        [CommonLibrary.CustomAttributes.ResolveNullValue("short", "0")]
        public short resolveNullSmallInt(int columnIndex, SqlDataReader reader)
        {
            short data = (reader.IsDBNull(columnIndex))
                ? Convert.ToInt16(0) : Convert.ToInt16(reader[columnIndex].ToString());
            return data;
        }

        [CommonLibrary.CustomAttributes.ResolveNullValue("short", "0")]
        public short resolveNullSmallInt(string column, SqlDataReader reader)
        {
            short data = (reader.IsDBNull(reader.GetOrdinal(column)))
                          ? Convert.ToInt16(0) : Int16.Parse(reader[column].ToString());
            return data;

        }

        public Guid retrieveGuidFromDataReader(int columnIndex, SqlDataReader reader)
        {
            Guid data = (Guid)reader[columnIndex];
            return data;
        }

        public Guid retrieveNullableGuidFromDataReader(int columnIndex, SqlDataReader reader)
        {
            Guid data = (reader.IsDBNull(columnIndex)) ? Guid.NewGuid() : (Guid)reader[columnIndex];            
            return data;
        }


        public object retrieveNullableTypeFromDataReader(int columnIndex, SqlDataReader reader)
        {
            object data = reader[columnIndex];
            return data;
        }

        public byte [] retrieveNullableByteArrayFromDataReader(int columnIndex, SqlDataReader reader)
        {
            byte [] data = (byte [])reader[columnIndex];
            return data;
        }

        public Image retrieveNullableImageFromDataReader(int columnIndex, SqlDataReader reader)
        {

            System.Drawing.Image newImage;

            //Byte[] value = (Byte[])reader[columnIndex];

            Byte [] blob = new Byte[(reader.GetBytes(columnIndex, 0, null, 0, int.MaxValue))];
            reader.GetBytes(columnIndex, 0, blob, 0, blob.Length);
            
            using (MemoryStream ms = new MemoryStream(blob, 0, blob.Length))
            {
                ms.Write(blob, 0, blob.Length);

                newImage = Image.FromStream(ms,true);               
            }
            Image data = newImage;
            return data;
        }

        public int getIntFromSmallInt(DataRow row, string fieldName)
        {
            if (row[fieldName] != System.DBNull.Value)
            {
                return Int16.Parse(row[fieldName].ToString());
            }
            else
            {
                return 0;
            }
        }
        public int getIntFromSmallInt(DataRowView row, string fieldName)
        {
            if (row[fieldName] != System.DBNull.Value)
            {
                return Int16.Parse(row[fieldName].ToString());
            }
            else
            {
                return 0;
            }
        }

        [CommonLibrary.CustomAttributes.ResolveNullValue(@"System.Nullable<long>", "null")]
        public System.Nullable<long> resolveNullLongToNullableDataType(int columnIndex, SqlDataReader reader)
        {
            long? data = null;

            if (!reader.IsDBNull(columnIndex))
            {
                data = long.Parse(reader[columnIndex].ToString());
            }
            return data;
        }

        [CommonLibrary.CustomAttributes.ResolveNullValue("long", "0")]
        public long resolveNullLong(int columnIndex, SqlDataReader reader)
        {
            long data = (reader.IsDBNull(columnIndex))
                ? 0 : long.Parse(reader[columnIndex].ToString());
            return data;
        }

        [CommonLibrary.CustomAttributes.ResolveNullValue(@"System.Nullable<float>", "null")]
        public System.Nullable<float> resolveNullFloatToNullableDataType(int columnIndex, SqlDataReader reader)
        {
            float? data = null;

            if (!reader.IsDBNull(columnIndex))
            {
                data = float.Parse(reader[columnIndex].ToString());
            }
            return data;
        }

        [CommonLibrary.CustomAttributes.ResolveNullValue("float", "0")]
        public float resolveNullFloat(int columnIndex, SqlDataReader reader)
        {
            float data = (reader.IsDBNull(columnIndex))
                ? 0 : float.Parse(reader[columnIndex].ToString());
            return data;
        }

        [CommonLibrary.CustomAttributes.ResolveNullValue(@"System.Nullable<decimal>", "null")]
        public System.Nullable<decimal> resolveNullDecimalToNullableDataType(int columnIndex, SqlDataReader reader)
        {
            decimal? data = null;

            if (!reader.IsDBNull(columnIndex))
            {
                data = decimal.Parse(reader[columnIndex].ToString());
            }
            return data;
        }

        [CommonLibrary.CustomAttributes.ResolveNullValue("decimal", "0")]
        public decimal resolveNullDecimal(int columnIndex, SqlDataReader reader)
        {
            decimal data = (reader.IsDBNull(columnIndex))
                ? 0 : decimal.Parse(reader[columnIndex].ToString());
            return data;
        }

        public object GetValueFromInputObjectForSprocParameter<T>(string parameterName,
                                                        T typePassedIn)
        {
            string columnName = CommonLibrary.Utility.DatabaseHelperMethods.GetColumnNameFromSqlParameterName(parameterName);
            object value = null;

            Type type = typePassedIn.GetType();

            PropertyInfo[] propertyInfos = type.GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                PropertyInfo derivedPropertyInfo = type.GetProperty(propertyInfo.Name);
                if (DoesPropertyInfoColumnAttributeMatchColumnName(derivedPropertyInfo, columnName))
                {
                    value = derivedPropertyInfo.GetValue(typePassedIn, null);
                    break;
                }
            }
            return value;

        }

        public bool DoesPropertyInfoColumnAttributeMatchColumnName(PropertyInfo propertyInfo,
                                                           string columnName)
        {
            object[] attributes = propertyInfo.GetCustomAttributes(false);
            bool doesPropertyColumnMappingMatchColumnName = false;
            if (attributes != null)
            {
                foreach (Attribute attribute in attributes)
                {
                    if (attribute is CommonLibrary.CustomAttributes.DatabaseColumnAttribute)
                    {
                        CommonLibrary.CustomAttributes.DatabaseColumnAttribute databaseColumnAttribute
                            = (CommonLibrary.CustomAttributes.DatabaseColumnAttribute)attribute;
                        string columnMapping = databaseColumnAttribute.DatabaseColumn;
                        if (columnMapping == columnName)
                        {
                            doesPropertyColumnMappingMatchColumnName = true;
                            break;
                        }
                    }
                }
            }
            return doesPropertyColumnMappingMatchColumnName;
        }

        public void ExecuteNonQuery(SqlConnection connection,
                            TypeOfSql typeOfSql,
                            TypeOfExecution typeOfExecution,
                            TransactionBehavior transactionBehavior,
                            string sqlString,
                            SqlParameter[] sqlParameters,
                            ref SqlTransaction transaction,
                            out object returned)
        {
            returned = null;

            OpenConnectionIfClosed(connection);

            using (SqlCommand sqlCommand = new SqlCommand(sqlString, connection))
            {
                sqlCommand.CommandType = GetCommandTypeByTypeOfSql(typeOfSql);

                sqlCommand.Connection = connection;

                PrepareTransaction(connection, ref transaction, sqlCommand, transactionBehavior);

                AddParametersToCommand(sqlCommand, sqlParameters);

                try
                {
                    switch (typeOfExecution)
                    {
                        case TypeOfExecution.NonQuery:
                            {
                                returned = sqlCommand.ExecuteNonQuery();
                                break;
                            }
                        case TypeOfExecution.Scalar:
                            {
                                returned = sqlCommand.ExecuteScalar();
                                break;
                            }
                        default:
                            {
                                returned = sqlCommand.ExecuteNonQuery();
                                break;
                            }
                    }
                }
                catch (SqlException sqlEx)
                {
                    throw sqlEx;
                }
            }

        }

        public void ExecuteNonQuery(SqlConnection connection,
                                    TypeOfSql typeOfSql,
                                    TypeOfExecution typeOfExecution,
                                    TransactionBehavior transactionBehavior,
                                    string sqlString,
                                    ref SqlTransaction transaction,
                                    out object returned)
        {
            returned = null;

            OpenConnectionIfClosed(connection);

            using (SqlCommand sqlCommand = new SqlCommand(sqlString, connection))
            {
                sqlCommand.CommandType = GetCommandTypeByTypeOfSql(typeOfSql);

                sqlCommand.Connection = connection;

                PrepareTransaction(connection, ref transaction, sqlCommand, transactionBehavior);

                try
                {
                    switch (typeOfExecution)
                    {
                        case TypeOfExecution.NonQuery:
                            {
                                returned = sqlCommand.ExecuteNonQuery();
                                break;
                            }
                        case TypeOfExecution.Scalar:
                            {
                                returned = sqlCommand.ExecuteScalar();
                                break;
                            }
                        default:
                            {
                                returned = sqlCommand.ExecuteNonQuery();
                                break;
                            }
                    }
                }
                catch (SqlException sqlEx)
                {
                    throw sqlEx;
                }
            }
        }

        public void RollbackTransaction(ref SqlTransaction transaction)
        {
            if (transaction != null)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (InvalidOperationException invalidOpEx)
                {
                    //by this point it is clear that transaction is already rolled back
                    //new information shows that if it has already been rolled back an 
                    //invalid operation exception is thrown.  Do nothing here.  Callers
                    //responsible for proper handling
                }

            }
        }

        public void CommitTransaction(ref SqlTransaction transaction)
        {
            if (transaction != null)
            {
                try
                {
                    transaction.Commit();
                }
                catch (InvalidOperationException invalidOpEx)
                {
                    //by this point it is clear that transaction is already committed or rolled back
                    //new information shows that if it has already been rolled back or committed an 
                    //invalid operation exception is thrown.  Do nothing here.  Callers responsible
                    //for proper handling
                }
            }

        }

        public void AddParametersToCommand(SqlCommand sqlCommand, SqlParameter[] sqlParameters)
        {
            foreach (SqlParameter sqlParameter in sqlParameters)
            {
                //command.Parameters.Add(sqlParameter);
                SqlParameter newParameter = new SqlParameter(sqlParameter.ParameterName,
                    sqlParameter.SqlDbType,
                    sqlParameter.Size,
                    sqlParameter.Direction,
                    sqlParameter.IsNullable,
                    sqlParameter.Precision,
                    sqlParameter.Scale,
                    sqlParameter.SourceColumn,
                    sqlParameter.SourceVersion,
                    sqlParameter.Value);
                sqlCommand.Parameters.Add(newParameter);
            }
        }

        public void PrepareTransaction(SqlConnection connection,
            ref SqlTransaction transaction,
            SqlCommand command,
            TransactionBehavior behavior)
        {
            switch (behavior)
            {
                //we assume consumer knows to choose when to begin their transaction
                case TransactionBehavior.Begin:
                    {
                        if (transaction == null)
                        {
                            transaction = connection.BeginTransaction();
                        }
                        command.Transaction = transaction;
                        break;
                    }
                //make sure that the consumer has called a begin transaction before trying to enlist
                //we could do it for them but the intention may not be to start the transaction with
                //this item
                case TransactionBehavior.Enlist:
                    {
                        if (transaction == null)
                        {
                            throw new BeginTransactionException("Controller must call with a BeginTransaction TransactionBehvior before trying to Enlist");
                        }
                        command.Transaction = transaction;
                        break;
                    }
            }
        }

        public CommandType GetCommandTypeByTypeOfSql(TypeOfSql typeOfSql)
        {
            switch (typeOfSql)
            {
                case TypeOfSql.Dynamic:
                    return CommandType.Text;
                    break;
                case TypeOfSql.StoredProcedure:
                    return CommandType.StoredProcedure;
                    break;
                default:
                    return CommandType.StoredProcedure;
                    break;

            }
        }
        public void OpenConnectionIfClosed(SqlConnection connection)
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
        }

        public DatabaseSmoObjectsAndSettings GetNewDatabaseSmoObjectsAndSettings(string connectionString)
        {
            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder(connectionString);
            DatabaseSmoObjectsAndSettings smo = new DatabaseSmoObjectsAndSettings(
                sb.InitialCatalog,
                sb.DataSource,
                sb.InitialCatalog,
                sb.UserID,
                sb.Password,
                sb.IntegratedSecurity);

            return smo;
        }

       
    }
}

