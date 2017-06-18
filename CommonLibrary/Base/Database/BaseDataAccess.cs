using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Linq;

using Microsoft.CSharp;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Server;

using Sql = CommonLibrary.Constants.SqlConstants;
using SqlDbConstants = CommonLibrary.Constants.SqlDbConstants;
using SqlNativeTypeConstants = CommonLibrary.Constants.NativeSqlConstants;
using SqlHelper = CommonLibrary.Utility.DatabaseHelperMethods;
using CSharpDataTypeConstants = CommonLibrary.Constants.CSharpDataTypeConstants;
using ClassCreationHelperMethods = CommonLibrary.Utility.ClassCreationHelperMethods;
using ClassCreationConstants = CommonLibrary.Constants.ClassCreationConstants;
using System.Configuration;



namespace CommonLibrary.Base.Database
{
    public class BaseDataAccess<T> : BaseDatabase
    {
        DatabaseSmoObjectsAndSettings _databaseSmoObjectsAndSettings = null;        

        public DatabaseSmoObjectsAndSettings DatabaseSmoObjectsAndSettings_Property
        {
            get { return _databaseSmoObjectsAndSettings; }
            set { _databaseSmoObjectsAndSettings = value; }
        }

        public BaseDataAccess(DatabaseSmoObjectsAndSettings smoObjectsAndSettings)
        {
            _databaseSmoObjectsAndSettings = smoObjectsAndSettings;
        }

        public BaseDataAccess(string databaseName,
                              string dataSource,
                              string initialCatalog,
                              string userId,
                              string password,
                              bool trustedConnection)
        {
            _databaseSmoObjectsAndSettings =
                new DatabaseSmoObjectsAndSettings(databaseName, dataSource, initialCatalog, 
                                                  userId, password, trustedConnection);
        }

        public BaseDataAccess(string databaseName,
                              string dataSource,
                              string initialCatalog,
                              string userId,
                              string password,
                              bool trustedConnection,
                              string schema)
        {
            _databaseSmoObjectsAndSettings =
                new DatabaseSmoObjectsAndSettings(databaseName, dataSource, initialCatalog, 
                                                  userId, password, trustedConnection,schema);
        }

        public List<T> Get(List<T> multipleDtos, Enumerations.GetPermutations getPermutation)
        {
            List<T> returnList = new List<T>();
            foreach (T dto in multipleDtos)
            {
                returnList.AddRange(Get(dto, getPermutation));
            }
            return returnList;
        }

        public List<T> Get(List<T> multipleDtos, 
            SqlConnection connection,
            ref SqlTransaction transaction,
            BaseDatabase.TransactionBehavior transactionBehavior,
            Enumerations.GetPermutations getPermutation)
        {
            List<T> returnList = new List<T>();
            foreach (T dto in multipleDtos)
            {
                returnList.AddRange(Get(dto,
                    connection,
                    ref transaction,
                    transactionBehavior,
                    getPermutation));
            }
            return returnList;
        }

        public List<T> Insert(List<T> multipleDtos)
        {
            List<T> returnList = new List<T>();
            foreach (T dto in multipleDtos)
            {
                returnList.Add(Insert(dto));
            }
            return returnList;
        }

        public List<T> Update(List<T> multipleDtos)
        {
            List<T> returnList = new List<T>();
            foreach (T dto in multipleDtos)
            {
                returnList.Add(Update(dto));
            }
            return returnList;
        }

        public List<T> Delete(List<T> multipleDtos)
        {
            List<T> returnList = new List<T>();
            foreach (T dto in multipleDtos)
            {
                returnList.Add(Delete(dto));
            }
            return returnList;
        }

        public T Insert(T oneDto)
        {
            Type type = GetTypeOfDtoFromGenericType(oneDto);
            string sprocName = string.Empty;
            object objInstance = oneDto;

            CommonLibrary.CustomAttributes.TableNameAttribute tableAttribute =
               (CommonLibrary.CustomAttributes.TableNameAttribute)Attribute.GetCustomAttribute(type,
                                            typeof(CommonLibrary.CustomAttributes.TableNameAttribute));

            sprocName = InsertSprocName(tableAttribute);
            Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> propertyToColumnToSqlParameter =
    GetPropertyInfoToColumnToSqlParameterForInsert(type, sprocName);            

            SqlParameter [] parameters =
                GetSqlParametersFromPropertyInfoToColumnToSqlParameter(propertyToColumnToSqlParameter,oneDto,true);
            executeNonQueryStoredProcedure(_databaseSmoObjectsAndSettings.ConnectionString, sprocName, parameters);

            foreach (KeyValuePair<PropertyInfo, KeyValuePair<string, SqlParameter>> kvp in propertyToColumnToSqlParameter)
            {
                if (IsPrimaryKey(kvp.Key))
                {
                    SetValueFromPropertyColumnMapping(oneDto, kvp.Key.Name, kvp.Value.Value.Value);

                    //here it should only be the primary key parameters that are output or input/output
                    //and then map them to the one dto respective properties so that the get can occur?
                    //at this point we know which are the primary key columns from the attributes
                    //if there is an attribute on the property.
                }
                
            }            

            //there can only be one.
            List<T> returnFromGet = Get(oneDto, CommonLibrary.Enumerations.GetPermutations.ByPrimaryKey);
            T returnAddedItem = default(T);
            if (returnFromGet!= null)
            {
                if (returnFromGet.Count == 1)
                {
                    returnAddedItem = returnFromGet[0];
                }
            }
            return returnAddedItem;
        }

        public T Update(T oneDto)
        {
            Type type = GetTypeOfDtoFromGenericType(oneDto);
            string sprocName = string.Empty;
            object objInstance = oneDto;

            CommonLibrary.CustomAttributes.TableNameAttribute tableAttribute =
               (CommonLibrary.CustomAttributes.TableNameAttribute)Attribute.GetCustomAttribute(type,
                                            typeof(CommonLibrary.CustomAttributes.TableNameAttribute));

            sprocName = UpdateSprocName(tableAttribute);

            Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> propertyToColumnToSqlParameter =
    GetPropertyInfoToColumnToSqlParameterForUpdate(type, sprocName);

            SqlParameter[] parameters =
              GetSqlParametersFromPropertyInfoToColumnToSqlParameter(propertyToColumnToSqlParameter, oneDto,true);
            executeNonQueryStoredProcedure(_databaseSmoObjectsAndSettings.ConnectionString, sprocName, parameters);

            foreach (KeyValuePair<PropertyInfo, KeyValuePair<string, SqlParameter>> kvp in propertyToColumnToSqlParameter)
            {
                if (IsPrimaryKey(kvp.Key))
                {
                    SetValueFromPropertyColumnMapping(oneDto, kvp.Key.Name, kvp.Value.Value.Value);                  
                }

            }        


            List<T> returnFromGet = Get(oneDto, CommonLibrary.Enumerations.GetPermutations.ByPrimaryKey);
            T returnAddedItem = default(T);
            if (returnFromGet != null)
            {
                if (returnFromGet.Count == 1)
                {
                    returnAddedItem = returnFromGet[0];
                }
            }
            return returnAddedItem;

        }

        public T Delete(T oneDto)
        {
            Type type = GetTypeOfDtoFromGenericType(oneDto);
            string sprocName = string.Empty;

            object objInstance = oneDto;


            CommonLibrary.CustomAttributes.TableNameAttribute tableAttribute =
                (CommonLibrary.CustomAttributes.TableNameAttribute)Attribute.GetCustomAttribute(type,
                                             typeof(CommonLibrary.CustomAttributes.TableNameAttribute));

            sprocName = DeleteSprocName(tableAttribute);

            Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> propertyToColumnToSqlParameter =
                GetPropertyInfoToColumnToSqlParameterForDelete(type, sprocName);

            SqlParameter[] parameters =
                GetSqlParametersFromPropertyInfoToColumnToSqlParameter(propertyToColumnToSqlParameter, oneDto, true);

            //do a get before a delete to return to consumer they may want that info
            //this is a problem if we use by primary key be default cause they may be calling to 
            //delete by criteria instead from BO
            List<T> returnFromGet = Get(oneDto, CommonLibrary.Enumerations.GetPermutations.ByPrimaryKey);
            //List<T> returnFromGet = Get(oneDto, CommonLibrary.Enumerations.GetPermutations.ByExplicitCriteria);
            T returnDeletedItem = default(T);

            if (returnFromGet != null)
            {
                if (returnFromGet.Count == 1)
                {
                    returnDeletedItem = returnFromGet[0];
                }
            }
            executeNonQueryStoredProcedure(_databaseSmoObjectsAndSettings.ConnectionString, sprocName, parameters);

            return returnDeletedItem;
        }

        public List<T> Insert(List<T> multipleDtos,
            SqlConnection connection,
            ref SqlTransaction transaction,
            BaseDatabase.TransactionBehavior transactionBehavior,
            out object returnValue)
        {
            returnValue = null;

            List<T> returnList = new List<T>();
            foreach (T dto in multipleDtos)
            {
                returnList.Add(Insert(dto,connection,
                    ref transaction,
                    transactionBehavior,
                    out returnValue));
            }
            return returnList;
        }

        public List<T> Update(List<T> multipleDtos,
            SqlConnection connection,
            ref SqlTransaction transaction,
            BaseDatabase.TransactionBehavior transactionBehavior,
            out object returnValue)
        {
            returnValue = null;

            List<T> returnList = new List<T>();
            foreach (T dto in multipleDtos)
            {
                returnList.Add(Update(dto, connection,
                    ref transaction,
                    transactionBehavior,
                    out returnValue));
            }
            return returnList;
        }

        public List<T> Delete(List<T> multipleDtos,
            SqlConnection connection,
            ref SqlTransaction transaction,
            BaseDatabase.TransactionBehavior transactionBehavior,
            out object returnValue)
        {
            returnValue = null;

            List<T> returnList = new List<T>();
            foreach (T dto in multipleDtos)
            {
                returnList.Add(Delete(dto, connection,
                    ref transaction,
                    transactionBehavior,
                    out returnValue));
            }
            return returnList;
        }

        public T Insert(T oneDto, 
            SqlConnection connection,
            ref SqlTransaction transaction,
            BaseDatabase.TransactionBehavior transactionBehavior,
            out object returnValue)
        {
            Type type = GetTypeOfDtoFromGenericType(oneDto);
            string sprocName = string.Empty;
            object objInstance = oneDto;

            CommonLibrary.CustomAttributes.TableNameAttribute tableAttribute =
               (CommonLibrary.CustomAttributes.TableNameAttribute)Attribute.GetCustomAttribute(type,
                                            typeof(CommonLibrary.CustomAttributes.TableNameAttribute));

            sprocName = InsertSprocName(tableAttribute);  
         

           

            Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> propertyToColumnToSqlParameter =
    GetPropertyInfoToColumnToSqlParameterForInsert(type, sprocName,ref transaction,transactionBehavior);

            SqlParameter[] parameters =
                GetSqlParametersFromPropertyInfoToColumnToSqlParameter(propertyToColumnToSqlParameter, oneDto, true,
                ref transaction,transactionBehavior);

            

            ExecuteNonQuery(connection,
                            TypeOfSql.StoredProcedure,
                            TypeOfExecution.NonQuery,
                            transactionBehavior,
                            sprocName,
                            parameters,
                            ref transaction,
                            out returnValue);

            foreach (KeyValuePair<PropertyInfo, KeyValuePair<string, SqlParameter>> kvp in propertyToColumnToSqlParameter)
            {
                if (IsPrimaryKey(kvp.Key))
                {
                    SetValueFromPropertyColumnMapping(oneDto, kvp.Key.Name, kvp.Value.Value.Value);

                    //here it should only be the primary key parameters that are output or input/output
                    //and then map them to the one dto respective properties so that the get can occur?
                    //at this point we know which are the primary key columns from the attributes
                    //if there is an attribute on the property.
                }

            }

            //there can only be one.
            //do not do this on the same transaction let a new connection be created or else we have transaction issues 
            //and an insert or udate etc would be rolled back..need to do a get "out of band" or maybe we skip the get altogether
            //since we had to set all the values anyway... and oh by the way, we are in a transaction that probably has not
            //yet comitted so we would have to be able to do a "dirty read" anyway...


            //List<T> returnFromGet = Get(oneDto, connection,
            //    ref transaction,
            //    transactionBehavior,
            //    CommonLibrary.Enumerations.GetPermutations.ByPrimaryKey);            

            //T returnAddedItem = default(T);
            //if (returnFromGet != null)
            //{
            //    if (returnFromGet.Count == 1)
            //    {
            //        returnAddedItem = returnFromGet[0];
            //    }
            //}
            //return returnAddedItem;
            return oneDto;
        }

        public T Update(T oneDto,
            SqlConnection connection,
            ref SqlTransaction transaction,
            BaseDatabase.TransactionBehavior transactionBehavior,
            out object returnValue)
        {
            Type type = GetTypeOfDtoFromGenericType(oneDto);
            string sprocName = string.Empty;
            object objInstance = oneDto;

            CommonLibrary.CustomAttributes.TableNameAttribute tableAttribute =
               (CommonLibrary.CustomAttributes.TableNameAttribute)Attribute.GetCustomAttribute(type,
                                            typeof(CommonLibrary.CustomAttributes.TableNameAttribute));

            sprocName = UpdateSprocName(tableAttribute);


            Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> propertyToColumnToSqlParameter =
    GetPropertyInfoToColumnToSqlParameterForUpdate(type, sprocName, ref transaction, transactionBehavior);

            SqlParameter[] parameters =
              GetSqlParametersFromPropertyInfoToColumnToSqlParameter(propertyToColumnToSqlParameter, oneDto, true,
              ref transaction,transactionBehavior);

            ExecuteNonQuery(connection,
                            TypeOfSql.StoredProcedure,
                            TypeOfExecution.NonQuery,
                            transactionBehavior,
                            sprocName,
                            parameters,
                            ref transaction,
                            out returnValue);

            foreach (KeyValuePair<PropertyInfo, KeyValuePair<string, SqlParameter>> kvp in propertyToColumnToSqlParameter)
            {
                if (IsPrimaryKey(kvp.Key))
                {
                    SetValueFromPropertyColumnMapping(oneDto, kvp.Key.Name, kvp.Value.Value.Value);
                }

            }


            //there can only be one.
            //do not do this on the same transaction let a new connection be created or else we have transaction issues 
            //and an insert or udate etc would be rolled back..need to do a get "out of band" or maybe we skip the get altogether
            //since we had to set all the values anyway... and oh by the way, we are in a transaction that probably has not
            //yet comitted so we would have to be able to do a "dirty read" anyway...


            //List<T> returnFromGet = Get(oneDto, connection,
            //    ref transaction,
            //    transactionBehavior,
            //    CommonLibrary.Enumerations.GetPermutations.ByPrimaryKey);            

            //T returnAddedItem = default(T);
            //if (returnFromGet != null)
            //{
            //    if (returnFromGet.Count == 1)
            //    {
            //        returnAddedItem = returnFromGet[0];
            //    }
            //}
            //return returnAddedItem;
            return oneDto;

        }

        private Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> GetPropertyInfoToColumnToSqlParameterForUpdate(Type type, string sprocName, ref SqlTransaction transaction, TransactionBehavior transactionBehavior)
        {
            PropertyInfo[] properties = type.GetProperties();

            List<StoredProcedureParameter> storedProcedureParameters =
                GetStoredProcedureInputParametersForUpdate(sprocName, properties,ref transaction, transactionBehavior);

            SqlParameter[] parameters = null;

            //these now only have the type and the name we need to provide the value and to get the value
            //we would resolve the column name to the column custom attribute on the property
            Dictionary<string, SqlParameter> dbColumnNameToSqlParamter
                = GetSqlParametersBySprocParameters(storedProcedureParameters);
            Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> propertyToColumnToSqlParameter =
                new Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>>();

            if (dbColumnNameToSqlParamter.Count > 0)
            {
                foreach (KeyValuePair<string, SqlParameter> kvp in dbColumnNameToSqlParamter)
                {
                    foreach (PropertyInfo propertyInfo in properties)
                    {
                        bool propertyInfoMatchColumnMapping = false;
                        propertyInfoMatchColumnMapping =
                            DoesPropertyInfoColumnAttributeMatchColumnName(propertyInfo, kvp.Key);
                        if (propertyInfoMatchColumnMapping)
                        {
                            propertyToColumnToSqlParameter.Add(propertyInfo, kvp);
                        }
                    }
                }
            }
            return propertyToColumnToSqlParameter;
        }      


        public T Delete(T oneDto,
            SqlConnection connection,
            ref SqlTransaction transaction,
            BaseDatabase.TransactionBehavior transactionBehavior,
            out object returnValue)
        {
            Type type = GetTypeOfDtoFromGenericType(oneDto);
            string sprocName = string.Empty;

            object objInstance = oneDto;


            CommonLibrary.CustomAttributes.TableNameAttribute tableAttribute =
                (CommonLibrary.CustomAttributes.TableNameAttribute)Attribute.GetCustomAttribute(type,
                                             typeof(CommonLibrary.CustomAttributes.TableNameAttribute));

            sprocName = DeleteSprocName(tableAttribute);

            Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> propertyToColumnToSqlParameter =
                GetPropertyInfoToColumnToSqlParameterForDelete(type, sprocName, ref transaction, transactionBehavior);

            SqlParameter[] parameters =
                GetSqlParametersFromPropertyInfoToColumnToSqlParameter(propertyToColumnToSqlParameter, oneDto, true,
                ref transaction, transactionBehavior);

            //do a get before a delete to return to consumer they may want that info
            List<T> returnFromGet = Get(oneDto,
                connection,
                ref transaction,
                transactionBehavior,
                Enumerations.GetPermutations.ByPrimaryKey);

            T returnDeletedItem = default(T);

            if (returnFromGet != null)
            {
                if (returnFromGet.Count == 1)
                {
                    returnDeletedItem = returnFromGet[0];
                }
            }

            ExecuteNonQuery(connection,
                            TypeOfSql.StoredProcedure,
                            TypeOfExecution.NonQuery,
                            transactionBehavior,
                            sprocName,
                            parameters,
                            ref transaction,
                            out returnValue);

            return returnDeletedItem;
        }

        public bool IsPrimaryKey(PropertyInfo propertyInfo)
        {            
            bool isPrimaryKey = false;           
           
            if (propertyInfo != null)
            {
                 object [] attributes = propertyInfo.GetCustomAttributes(false);
                if(attributes != null)
                {
                    isPrimaryKey = IsPrimaryKeyParameter(attributes);
                }
            }
            return isPrimaryKey;
        }

        

        public SqlParameter[] GetColumnsForInserts(Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> propertyToColumnToSqlParameter,
                                                              string tableName)
        {
            //List<SqlParameter> listParametersNotIdentity = new List<SqlParameter>();
            SqlParameter[] arrayParameters = new SqlParameter[propertyToColumnToSqlParameter.Count];
            int countParameters = 0;
            foreach (KeyValuePair<PropertyInfo, KeyValuePair<string, SqlParameter>> kvp in propertyToColumnToSqlParameter)
            {               
               arrayParameters[countParameters] = kvp.Value.Value;
               countParameters++;
            }

            return arrayParameters;
        }


        public List<T> Get(T oneDto, Enumerations.GetPermutations getPermutation)
        {
            Type type = GetTypeOfDtoFromGenericType(oneDto);
            string sprocName = string.Empty;

            object objInstance = oneDto;      
           

            CommonLibrary.CustomAttributes.TableNameAttribute tableAttribute =
                (CommonLibrary.CustomAttributes.TableNameAttribute)Attribute.GetCustomAttribute(type,
                                             typeof(CommonLibrary.CustomAttributes.TableNameAttribute));

            sprocName = GetSprocName(tableAttribute, getPermutation);

            Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> propertyToColumnToSqlParameter =
                GetPropertyInfoToColumnToSqlParameter(type, sprocName);

            SqlParameter[] parameters =
                GetSqlParametersFromPropertyInfoToColumnToSqlParameter(propertyToColumnToSqlParameter,oneDto);

            SqlDataReader reader = RunGetSproc(parameters, sprocName);
            List<T> listClass = GetListFromReader(oneDto, reader);
            return listClass;
        }



        public List<T> Get(T oneDto, SqlConnection connection,
            ref SqlTransaction transaction,
            BaseDatabase.TransactionBehavior transactionBehavior,
            Enumerations.GetPermutations getPermutation)
        {
            Type type = GetTypeOfDtoFromGenericType(oneDto);
            string sprocName = string.Empty;

            object objInstance = oneDto;            
           
            CommonLibrary.CustomAttributes.TableNameAttribute tableAttribute =
                (CommonLibrary.CustomAttributes.TableNameAttribute)Attribute.GetCustomAttribute(type,
                                             typeof(CommonLibrary.CustomAttributes.TableNameAttribute));

            sprocName = GetSprocName(tableAttribute, getPermutation);

            Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> propertyToColumnToSqlParameter =
                GetPropertyInfoToColumnToSqlParameter(type, sprocName);

            SqlParameter[] parameters =
                GetSqlParametersFromPropertyInfoToColumnToSqlParameter(propertyToColumnToSqlParameter, oneDto,true,
                ref transaction,transactionBehavior);

            SqlDataReader reader = RunGetSproc(sprocName,
                connection,
                ref transaction,
                transactionBehavior,
                TypeOfSql.StoredProcedure,
                parameters);

            List<T> listClass = GetListFromReader(oneDto, reader);
            return listClass;
        }





        public SqlParameter[] GetSqlParametersFromPropertyInfoToColumnToSqlParameter(Dictionary<PropertyInfo,
                                                                                     KeyValuePair<string, 
                                                                                     SqlParameter>> 
                                                                                     propertyToColumnToSqlParameter,
                                                                                     T oneDto)
        {
            SqlParameter[] parameters = null;
            if (propertyToColumnToSqlParameter.Count > 0)
            {
                int countItems = 0;
                List<SqlParameter> sqlParametersToPass = new List<SqlParameter>();

                foreach (KeyValuePair<PropertyInfo, KeyValuePair<string, SqlParameter>> kvp
                    in propertyToColumnToSqlParameter)
                {
                    object isModifiedObject = 
                        GetValueFromPropertyColumnMapping(oneDto, 
                        CommonLibrary.Constants.ClassCreationConstants.IS_MODIFIED_DICTIONARY_PROPERTY_NAME);

                    Dictionary<string, bool> isModifiedDictionary = null;

                    if (isModifiedObject is Dictionary<string, bool>)
                    {
                        isModifiedDictionary = (Dictionary<string, bool>)isModifiedObject;
                    }

                    string columnName = null;

                    object[] attributes = kvp.Key.GetCustomAttributes(false);

                    foreach (Attribute att in attributes)
                    {
                        if (att is CommonLibrary.CustomAttributes.DatabaseColumnAttribute)
                        {
                            columnName = ((CommonLibrary.CustomAttributes.DatabaseColumnAttribute)att).DatabaseColumn;
                        }
                    }

                    if (isModifiedDictionary != null && columnName != null)
                    {
                        if (isModifiedDictionary.ContainsKey(columnName))
                        {
                            bool isModified = isModifiedDictionary[columnName];
                            if (isModified)
                            {
                                object value = GetValueFromPropertyColumnMapping(oneDto, kvp.Key.Name);

                                sqlParametersToPass.Insert(countItems, kvp.Value.Value);
                                sqlParametersToPass[countItems].Value = value;
                                countItems++;
                            }
                        }
                    }

                }
                if (sqlParametersToPass.Count > 0)
                {
                    parameters = new SqlParameter[sqlParametersToPass.Count];
                    sqlParametersToPass.CopyTo(parameters);
                }
            }
            return parameters;
        }



        public SqlParameter[] GetSqlParametersFromPropertyInfoToColumnToSqlParameter(Dictionary<PropertyInfo,
                                                                             KeyValuePair<string,
                                                                             SqlParameter>>
                                                                             propertyToColumnToSqlParameter,
                                                                             T oneDto,
                                                                             bool dummyValForPrimaryKeyOperations)
        {
            SqlParameter[] parameters = null;
            if (propertyToColumnToSqlParameter.Count > 0)
            {
                int countItems = 0;
                List<SqlParameter> sqlParametersToPass = new List<SqlParameter>();

                foreach (KeyValuePair<PropertyInfo, KeyValuePair<string, SqlParameter>> kvp
                    in propertyToColumnToSqlParameter)
                {
                    ////WHY did i Comment out the check if is modified...??, i think it is causing a problem because
                    ////i am not taking into account the PrimaryKey OUTPUT type parameters for Inserts here.  What i need
                    ////to do is to keep the Primary key parameters and then only add those parameters that are modified
                    ////the reason for the primary key parameters that have a default set in the database, i make those
                    ////in the stored procedures OUTPUT or INPUT OUTPUT parameters, i need to handle this here so that i can do inserts of
                    ////dtos with only having the modified plus OUTPUT parameters reflected otherwise what i am stuck with 
                    ////is assigning a value to each and every property of the dto so that it will be inserted correctly

                    //    object isModifiedObject =
                    //        GetValueFromPropertyColumnMapping(oneDto,
                    //        CommonLibrary.Constants.ClassCreationConstants.IS_MODIFIED_DICTIONARY_PROPERTY_NAME);

                    //    Dictionary<string, bool> isModifiedDictionary = null;

                    //    if (isModifiedObject is Dictionary<string, bool>)
                    //    {
                    //        isModifiedDictionary = (Dictionary<string, bool>)isModifiedObject;
                    //    }

                    //    string columnName = null;

                    //    object[] attributes = kvp.Key.GetCustomAttributes(false);

                    //    foreach (Attribute att in attributes)
                    //    {
                    //        if (att is CommonLibrary.CustomAttributes.DatabaseColumnAttribute)
                    //        {
                    //            columnName = ((CommonLibrary.CustomAttributes.DatabaseColumnAttribute)att).DatabaseColumn;
                    //        }
                    //    }

                    //    bool added = false;

                    //    if (isModifiedDictionary != null && columnName != null)
                    //    {
                    //        if (isModifiedDictionary.ContainsKey(columnName))
                    //        {
                    //            bool isModified = isModifiedDictionary[columnName];
                    //            if (isModified)
                    //            {
                                    object value = GetValueFromPropertyColumnMapping(oneDto, kvp.Key.Name);

                                    sqlParametersToPass.Insert(countItems, kvp.Value.Value);
                                    sqlParametersToPass[countItems].Value = value;
                                    //added = true;
                                    countItems++;
                        //        }
                        //        else
                        //        {
                        //            if (IsPrimaryKey(kvp.Key) && !added)
                        //            {

                        //                sqlParametersToPass.Insert(countItems, kvp.Value.Value);
                        //                countItems++;
                        //                //TODO:  DO THE RIGHT THING FOR PRIMARY KEYS NOT WHAT IS BELOW JUST AN EXAMPLE
                        //                //SetValueFromPropertyColumnMapping(oneDto, kvp.Key.Name, kvp.Value.Value.Value);

                        //                //here it should only be the primary key parameters that are output or input/output
                        //                //and then map them to the one dto respective properties so that the get can occur?
                        //                //at this point we know which are the primary key columns from the attributes
                        //                //if there is an attribute on the property.
                                       
                        //            }                  
                   

                        //        }
                        //    }
                        //}
                    

                }
                if (sqlParametersToPass.Count > 0)
                {
                    parameters = new SqlParameter[sqlParametersToPass.Count];
                    sqlParametersToPass.CopyTo(parameters);
                }
            }
            return parameters;
        }

        public SqlParameter[] GetSqlParametersFromPropertyInfoToColumnToSqlParameter(Dictionary<PropertyInfo,
                                                                     KeyValuePair<string,
                                                                     SqlParameter>>
                                                                     propertyToColumnToSqlParameter,
                                                                     T oneDto,
                                                                     bool dummyValForPrimaryKeyOperations,
                                                                    ref SqlTransaction existingTransaction,
                                                                    TransactionBehavior transactionBehavior)
        {
            SqlParameter[] parameters = null;
            if (propertyToColumnToSqlParameter.Count > 0)
            {
                int countItems = 0;
                List<SqlParameter> sqlParametersToPass = new List<SqlParameter>();

                foreach (KeyValuePair<PropertyInfo, KeyValuePair<string, SqlParameter>> kvp
                    in propertyToColumnToSqlParameter)
                {
                    ////WHY did i Comment out the check if is modified...??, i think it is causing a problem because
                    ////i am not taking into account the PrimaryKey OUTPUT type parameters for Inserts here.  What i need
                    ////to do is to keep the Primary key parameters and then only add those parameters that are modified
                    ////the reason for the primary key parameters that have a default set in the database, i make those
                    ////in the stored procedures OUTPUT or INPUT OUTPUT parameters, i need to handle this here so that i can do inserts of
                    ////dtos with only having the modified plus OUTPUT parameters reflected otherwise what i am stuck with 
                    ////is assigning a value to each and every property of the dto so that it will be inserted correctly

                    //    object isModifiedObject =
                    //        GetValueFromPropertyColumnMapping(oneDto,
                    //        CommonLibrary.Constants.ClassCreationConstants.IS_MODIFIED_DICTIONARY_PROPERTY_NAME);

                    //    Dictionary<string, bool> isModifiedDictionary = null;

                    //    if (isModifiedObject is Dictionary<string, bool>)
                    //    {
                    //        isModifiedDictionary = (Dictionary<string, bool>)isModifiedObject;
                    //    }

                    //    string columnName = null;

                    //    object[] attributes = kvp.Key.GetCustomAttributes(false);

                    //    foreach (Attribute att in attributes)
                    //    {
                    //        if (att is CommonLibrary.CustomAttributes.DatabaseColumnAttribute)
                    //        {
                    //            columnName = ((CommonLibrary.CustomAttributes.DatabaseColumnAttribute)att).DatabaseColumn;
                    //        }
                    //    }

                    //    bool added = false;

                    //    if (isModifiedDictionary != null && columnName != null)
                    //    {
                    //        if (isModifiedDictionary.ContainsKey(columnName))
                    //        {
                    //            bool isModified = isModifiedDictionary[columnName];
                    //            if (isModified)
                    //            {
                    object value = GetValueFromPropertyColumnMapping(oneDto, kvp.Key.Name);

                    sqlParametersToPass.Insert(countItems, kvp.Value.Value);
                    sqlParametersToPass[countItems].Value = value;
                    //added = true;
                    countItems++;
                    //        }
                    //        else
                    //        {
                    //            if (IsPrimaryKey(kvp.Key) && !added)
                    //            {

                    //                sqlParametersToPass.Insert(countItems, kvp.Value.Value);
                    //                countItems++;
                    //                //TODO:  DO THE RIGHT THING FOR PRIMARY KEYS NOT WHAT IS BELOW JUST AN EXAMPLE
                    //                //SetValueFromPropertyColumnMapping(oneDto, kvp.Key.Name, kvp.Value.Value.Value);

                    //                //here it should only be the primary key parameters that are output or input/output
                    //                //and then map them to the one dto respective properties so that the get can occur?
                    //                //at this point we know which are the primary key columns from the attributes
                    //                //if there is an attribute on the property.

                    //            }                  


                    //        }
                    //    }
                    //}


                }
                if (sqlParametersToPass.Count > 0)
                {
                    parameters = new SqlParameter[sqlParametersToPass.Count];
                    sqlParametersToPass.CopyTo(parameters);
                }
            }
            return parameters;
        }




        //public SqlParameter[] GetSqlParametersFromPropertyInfoToColumnToSqlParameterForInsert(Dictionary<PropertyInfo,
        //                                                                     KeyValuePair<string,
        //                                                                     SqlParameter>>
        //                                                                     propertyToColumnToSqlParameter,
        //                                                                     T oneDto)
        //{
        //    SqlParameter[] parameters = null;
        //    if (propertyToColumnToSqlParameter.Count > 0)
        //    {
        //        int countItems = 0;
        //        List<SqlParameter> sqlParametersToPass = new List<SqlParameter>();

        //        foreach (KeyValuePair<PropertyInfo, KeyValuePair<string, SqlParameter>> kvp
        //            in propertyToColumnToSqlParameter)
        //        {
        //            object value = GetValueFromPropertyColumnMapping(oneDto, kvp.Key.Name);

        //            sqlParametersToPass.Insert(countItems, kvp.Value.Value);
        //            sqlParametersToPass[countItems].Value = value;
        //            countItems++;

        //        }
        //        if (sqlParametersToPass.Count > 0)
        //        {
        //            parameters = new SqlParameter[sqlParametersToPass.Count];
        //            sqlParametersToPass.CopyTo(parameters);
        //        }
        //    }
        //    return parameters;
        //}


        public Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> GetPropertyInfoToColumnToSqlParameter(
                                                                          Type type,
                                                                          string sprocName)
        {
            PropertyInfo[] properties = type.GetProperties();

            List<StoredProcedureParameter> storedProcedureParameters =
                GetStoredProcedureInputParameters(sprocName);

            SqlParameter[] parameters = null;

            //these now only have the type and the name we need to provide the value and to get the value
            //we would resolve the column name to the column custom attribute on the property
            Dictionary<string, SqlParameter> dbColumnNameToSqlParamter
                = GetSqlParametersBySprocParameters(storedProcedureParameters);
            Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> propertyToColumnToSqlParameter =
                new Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>>();

            if (dbColumnNameToSqlParamter.Count > 0)
            {
                foreach (KeyValuePair<string, SqlParameter> kvp in dbColumnNameToSqlParamter)
                {
                    foreach (PropertyInfo propertyInfo in properties)
                    {
                        bool propertyInfoMatchColumnMapping = false;
                        propertyInfoMatchColumnMapping =
                            DoesPropertyInfoColumnAttributeMatchColumnName(propertyInfo, kvp.Key);
                        if (propertyInfoMatchColumnMapping)
                        {
                            propertyToColumnToSqlParameter.Add(propertyInfo, kvp);
                        }
                    }
                }
            }
            return propertyToColumnToSqlParameter;
        }

        public Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> GetPropertyInfoToColumnToSqlParameterForInsert(
                                                                  Type type,
                                                                  string sprocName)
        {
            PropertyInfo[] properties = type.GetProperties();

            List<StoredProcedureParameter> storedProcedureParameters =
                GetStoredProcedureInputParametersForInsert(sprocName, properties);

            SqlParameter[] parameters = null;

            //these now only have the type and the name we need to provide the value and to get the value
            //we would resolve the column name to the column custom attribute on the property
            Dictionary<string, SqlParameter> dbColumnNameToSqlParamter
                = GetSqlParametersBySprocParameters(storedProcedureParameters);
            Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> propertyToColumnToSqlParameter =
                new Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>>();

            if (dbColumnNameToSqlParamter.Count > 0)
            {
                foreach (KeyValuePair<string, SqlParameter> kvp in dbColumnNameToSqlParamter)
                {
                    foreach (PropertyInfo propertyInfo in properties)
                    {
                        bool propertyInfoMatchColumnMapping = false;
                        propertyInfoMatchColumnMapping =
                            DoesPropertyInfoColumnAttributeMatchColumnName(propertyInfo, kvp.Key);
                        if (propertyInfoMatchColumnMapping)
                        {
                            propertyToColumnToSqlParameter.Add(propertyInfo, kvp);
                        }
                    }
                }
            }
            return propertyToColumnToSqlParameter;
        }

        /// <summary>
        /// when we have an existing transaction started all retrieves or accesses to the smo database property member variable
        /// fails since it wants all commands to enlist in the transaction for some reason
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sprocName"></param>
        /// <param name="existingTransaction"></param>
        /// <param name="transactionBehavior"></param>
        /// <returns></returns>
        public Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> GetPropertyInfoToColumnToSqlParameterForInsert(
                                                          Type type,
                                                          string sprocName,
                                                           ref SqlTransaction existingTransaction,
                                                            TransactionBehavior transactionBehavior)
        {
            PropertyInfo[] properties = type.GetProperties();

            List<StoredProcedureParameter> storedProcedureParameters =
                GetStoredProcedureInputParametersForInsert(sprocName, properties, ref existingTransaction,transactionBehavior);

            SqlParameter[] parameters = null;

            //these now only have the type and the name we need to provide the value and to get the value
            //we would resolve the column name to the column custom attribute on the property
            Dictionary<string, SqlParameter> dbColumnNameToSqlParamter
                = GetSqlParametersBySprocParameters(storedProcedureParameters,ref existingTransaction,transactionBehavior);
            Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> propertyToColumnToSqlParameter =
                new Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>>();

            if (dbColumnNameToSqlParamter.Count > 0)
            {
                foreach (KeyValuePair<string, SqlParameter> kvp in dbColumnNameToSqlParamter)
                {
                    foreach (PropertyInfo propertyInfo in properties)
                    {
                        bool propertyInfoMatchColumnMapping = false;
                        propertyInfoMatchColumnMapping =
                            DoesPropertyInfoColumnAttributeMatchColumnName(propertyInfo, kvp.Key);
                        if (propertyInfoMatchColumnMapping)
                        {
                            propertyToColumnToSqlParameter.Add(propertyInfo, kvp);
                        }
                    }
                }
            }
            return propertyToColumnToSqlParameter;
        }

        
        /// <summary>
        /// when we have an existing transaction started all retrieves or accesses to the smo database property member variable
        /// fails since it wants all commands to enlist in the transaction for some reason
        /// </summary>
        /// <param name="storedProcedureName"></param>
        /// <param name="properties"></param>
        /// <param name="existingTransaction"></param>
        /// <param name="transactionBehavior"></param>
        /// <returns></returns>
        public List<StoredProcedureParameter> GetStoredProcedureInputParametersForInsert(
            string storedProcedureName,
            PropertyInfo[] properties,
            ref SqlTransaction existingTransaction,
            TransactionBehavior transactionBehavior)
        {

            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder(_databaseSmoObjectsAndSettings.ConnectionString);
            DatabaseSmoObjectsAndSettings smo = new DatabaseSmoObjectsAndSettings(
                sb.InitialCatalog,
                sb.DataSource,
                sb.InitialCatalog,
                sb.UserID,
                sb.Password,
                sb.IntegratedSecurity);

            StoredProcedureCollection sprocCollection = smo.Database_Property.StoredProcedures;
           
            StoredProcedure sprocFound = null;

            List<StoredProcedureParameter> storedProcParameters = new List<StoredProcedureParameter>();

            foreach (StoredProcedure sproc in sprocCollection)
            {
                if (!sproc.IsSystemObject && sproc.Name == storedProcedureName)
                {
                    sprocFound = sproc;
                    break;
                }
            }
            if (sprocFound != null)
            {
                foreach (StoredProcedureParameter parameter in sprocFound.Parameters)
                {
                    if (!parameter.IsOutputParameter)
                    {
                        storedProcParameters.Add(parameter);
                    }
                    else
                    {
                        foreach (PropertyInfo propertyInfo in properties)
                        {
                            string columnName = GetColumnNameFromParameterName(parameter.Name);
                            if (DoesPropertyInfoColumnAttributeMatchColumnName(propertyInfo, columnName))
                            {
                                object[] customAttributes = propertyInfo.GetCustomAttributes(false);
                                if (IsPrimaryKeyParameter(customAttributes))
                                {
                                    storedProcParameters.Add(parameter);
                                }

                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                throw new ApplicationException("Sproc Not Found:  " + storedProcedureName);
            }
            return storedProcParameters;
        }

        public Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> GetPropertyInfoToColumnToSqlParameterForDelete(
                                                          Type type,
                                                          string sprocName)
        {
            PropertyInfo[] properties = type.GetProperties();

            List<StoredProcedureParameter> storedProcedureParameters =
                GetStoredProcedureInputParametersForDelete(sprocName, properties);

            SqlParameter[] parameters = null;

            //these now only have the type and the name we need to provide the value and to get the value
            //we would resolve the column name to the column custom attribute on the property
            Dictionary<string, SqlParameter> dbColumnNameToSqlParamter
                = GetSqlParametersBySprocParameters(storedProcedureParameters);
            Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> propertyToColumnToSqlParameter =
                new Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>>();

            if (dbColumnNameToSqlParamter.Count > 0)
            {
                foreach (KeyValuePair<string, SqlParameter> kvp in dbColumnNameToSqlParamter)
                {
                    foreach (PropertyInfo propertyInfo in properties)
                    {
                        bool propertyInfoMatchColumnMapping = false;
                        propertyInfoMatchColumnMapping =
                            DoesPropertyInfoColumnAttributeMatchColumnName(propertyInfo, kvp.Key);
                        if (propertyInfoMatchColumnMapping)
                        {
                            propertyToColumnToSqlParameter.Add(propertyInfo, kvp);
                        }
                    }
                }
            }
            return propertyToColumnToSqlParameter;
        }

        public Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> GetPropertyInfoToColumnToSqlParameterForDelete(
                                                  Type type,
                                                  string sprocName,
            ref SqlTransaction transaction,
            TransactionBehavior transactionBehavior)
        {
            PropertyInfo[] properties = type.GetProperties();

            List<StoredProcedureParameter> storedProcedureParameters =
                GetStoredProcedureInputParametersForDelete(sprocName, properties, ref transaction, transactionBehavior);

            SqlParameter[] parameters = null;

            //these now only have the type and the name we need to provide the value and to get the value
            //we would resolve the column name to the column custom attribute on the property
            Dictionary<string, SqlParameter> dbColumnNameToSqlParamter
                = GetSqlParametersBySprocParameters(storedProcedureParameters);
            Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> propertyToColumnToSqlParameter =
                new Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>>();

            if (dbColumnNameToSqlParamter.Count > 0)
            {
                foreach (KeyValuePair<string, SqlParameter> kvp in dbColumnNameToSqlParamter)
                {
                    foreach (PropertyInfo propertyInfo in properties)
                    {
                        bool propertyInfoMatchColumnMapping = false;
                        propertyInfoMatchColumnMapping =
                            DoesPropertyInfoColumnAttributeMatchColumnName(propertyInfo, kvp.Key);
                        if (propertyInfoMatchColumnMapping)
                        {
                            propertyToColumnToSqlParameter.Add(propertyInfo, kvp);
                        }
                    }
                }
            }
            return propertyToColumnToSqlParameter;
        }



        public Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> GetPropertyInfoToColumnToSqlParameterForUpdate(
                                                          Type type,
                                                          string sprocName)
        {
            PropertyInfo[] properties = type.GetProperties();

            List<StoredProcedureParameter> storedProcedureParameters =
                GetStoredProcedureInputParametersForUpdate(sprocName, properties);

            SqlParameter[] parameters = null;

            //these now only have the type and the name we need to provide the value and to get the value
            //we would resolve the column name to the column custom attribute on the property
            Dictionary<string, SqlParameter> dbColumnNameToSqlParamter
                = GetSqlParametersBySprocParameters(storedProcedureParameters);
            Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>> propertyToColumnToSqlParameter =
                new Dictionary<PropertyInfo, KeyValuePair<string, SqlParameter>>();

            if (dbColumnNameToSqlParamter.Count > 0)
            {
                foreach (KeyValuePair<string, SqlParameter> kvp in dbColumnNameToSqlParamter)
                {
                    foreach (PropertyInfo propertyInfo in properties)
                    {
                        bool propertyInfoMatchColumnMapping = false;
                        propertyInfoMatchColumnMapping =
                            DoesPropertyInfoColumnAttributeMatchColumnName(propertyInfo, kvp.Key);
                        if (propertyInfoMatchColumnMapping)
                        {
                            propertyToColumnToSqlParameter.Add(propertyInfo, kvp);
                        }
                    }
                }
            }
            return propertyToColumnToSqlParameter;
        }
        public List<T> GetListFromReader(T typePassedIn, SqlDataReader reader)
        {
            List<T> objListToReturn = null;
            Type type = typePassedIn.GetType();
            string typeName = type.Name;
            //where to use reflection to get the table property, find out the list class name
            CommonLibrary.CustomAttributes.TableNameAttribute tableAttribute
                = (CommonLibrary.CustomAttributes.TableNameAttribute)Attribute.GetCustomAttribute(type, typeof(CommonLibrary.CustomAttributes.TableNameAttribute));

            //string listClassName = ClassCreationHelperMethods.GetListClassName(tableAttribute.TableName);

            //i think what we mean to do here is to get the list of dto type which is what has a datareader in constructor

            string listNamespaceAndClass = null;
             string typeFullName = null;;
            
            if(typePassedIn.GetType().FullName.Contains("Bo."))
            {
                listNamespaceAndClass = typePassedIn.GetType().FullName.Replace("Bo.", "List.");
                typeFullName = listNamespaceAndClass.Replace("Business.", "");
            }
            else
                if (typePassedIn.GetType().FullName.Contains("Dto."))
                {
                    listNamespaceAndClass = typePassedIn.GetType().FullName.Replace("Dto.", "List.");
                    typeFullName = listNamespaceAndClass.Replace("Data.","");
                }
            //string listNamespace = ClassCreationHelperMethods.GetListNamespace(type.Assembly.GetName().Name);
            //just for the purposes of this demonstration need to hardcode the more friendly name
            //string listNamespace = Assembly.GetEntryAssembly().GetName().Name + ClassCreationConstants.DOT_OPERATOR + "List";
            //string typeFullName = listNamespace + ClassCreationConstants.DOT_OPERATOR + listClassName;
           
            

            //Type listToReturn = Assembly.GetEntryAssembly().GetType(typeFullName);
            Type listToReturn = Assembly.GetAssembly(type).GetType(typeFullName);

            //TODO:  !!! if listToReturn is null then look in the type.Assembly.ReferencedAssemblies() to load it from a referenced
            //assembly, this would be the case for example where our data classes were in a datalayer assembly and our business
            //classes were in a business assembly

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().ToList())
            {
                listToReturn = assembly.GetType(typeFullName);
                if (listToReturn != null)
                {
                    break;
                }
            }

            //for(int i = 0; i < 
                        
            ConstructorInfo [] constructorInfos = listToReturn.GetConstructors();

            foreach (ConstructorInfo constructorInfo in constructorInfos)
            {
                ParameterInfo[] parameterInfos = constructorInfo.GetParameters();
                foreach (ParameterInfo parameterInfo in parameterInfos)
                {
                    object[] parameterArray = new object[1];
                    if (parameterInfo.ParameterType == typeof(System.Data.SqlClient.SqlDataReader))
                    {
                        parameterArray[0] = reader;
                        objListToReturn = (List<T>)constructorInfo.Invoke(parameterArray);
                    }
                }
            }
           
            return objListToReturn;
        }


        public SqlDataReader RunGetSproc(string storedProcedure,
            SqlConnection connection,
            ref SqlTransaction transaction,
            TransactionBehavior transactionBehavior,
            TypeOfSql typeOfSql,
            SqlParameter[] sqlParameters)
        {
            
            SqlDataReader reader = null;
            if (sqlParameters != null)
            {
                reader = this.getDataReaderFromSP(storedProcedure,
                    connection,
                    ref transaction,
                    transactionBehavior,
                    typeOfSql,
                    sqlParameters);
            }
            else
            {
                reader = this.getDataReaderFromSP(storedProcedure,
                    connection,
                    ref transaction,
                    transactionBehavior,
                    typeOfSql);
            }
            return reader;
        }
        

        public SqlDataReader RunGetSproc(SqlParameter [] parameters, string sprocName)
        {
            SqlDataReader reader = null;
            if (parameters != null)
            {
                reader = getDataReaderFromSP(_databaseSmoObjectsAndSettings.ConnectionString,
                                    sprocName,
                                    parameters);
            }
            else
            {
                reader = getDataReaderFromSP(_databaseSmoObjectsAndSettings.ConnectionString,
                                    sprocName);
            }
            return reader;
        }

        public object GetValueFromPropertyColumnMapping(T typePassedIn, string propertyName)
        {
            Type type = typePassedIn.GetType();
            PropertyInfo derivedPropertyInfo = type.GetProperty(propertyName);

            object value = derivedPropertyInfo.GetValue(typePassedIn, null);
            return value;
        }

        public void SetValueFromPropertyColumnMapping(T typePassedIn, string propertyName, object value)
        {
            Type type = typePassedIn.GetType();
            PropertyInfo derivedPropertyInfo = type.GetProperty(propertyName);
            derivedPropertyInfo.SetValue(typePassedIn, value, null);
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

        public Dictionary<string, SqlParameter> GetSqlParametersBySprocParameters(List<StoredProcedureParameter> parameters, ref SqlTransaction existingTransaction,
                                                                                    TransactionBehavior transactionBehavior)
        {
            Dictionary<string, SqlParameter> dbColumnNameToSqlParameter =
                new Dictionary<string, SqlParameter>();

            if (parameters.Count > 0)
            {
                int countParameter = 0;
                foreach (StoredProcedureParameter sprocParameter in parameters)
                {
                    SqlDbType sqlDbType = GetSqlDbTypeFromStoredProcedureParameterDataType(sprocParameter.DataType);

                    SqlParameter sqlParameter = new SqlParameter(sprocParameter.Name, sqlDbType);
                    if (sprocParameter.IsOutputParameter)
                    {
                        sqlParameter.Direction = ParameterDirection.InputOutput;
                    }
                    dbColumnNameToSqlParameter.Add(GetColumnNameFromParameterName(sqlParameter.ParameterName), sqlParameter);
                }

            }
            return dbColumnNameToSqlParameter;
        }


        public Dictionary<string,SqlParameter> GetSqlParametersBySprocParameters(List<StoredProcedureParameter> parameters)
        {
            Dictionary<string, SqlParameter> dbColumnNameToSqlParameter = 
                new Dictionary<string, SqlParameter>();

            if (parameters.Count > 0)
            {               
                int countParameter = 0;
                foreach (StoredProcedureParameter sprocParameter in parameters)
                {
                   SqlDbType sqlDbType = GetSqlDbTypeFromStoredProcedureParameterDataType(sprocParameter.DataType);
                   
                    SqlParameter sqlParameter = new SqlParameter(sprocParameter.Name,sqlDbType);
                    if (sprocParameter.IsOutputParameter)
                    {
                        sqlParameter.Direction = ParameterDirection.InputOutput;
                    }                   
                    dbColumnNameToSqlParameter.Add(GetColumnNameFromParameterName(sqlParameter.ParameterName), sqlParameter);
                }

            }
            return dbColumnNameToSqlParameter; 
        }

        public Dictionary<string, SqlParameter> GetSqlParametersBySprocParameters(List<StoredProcedureParameter> parameters, 
            ref SqlTransaction transaction)
        {
            Dictionary<string, SqlParameter> dbColumnNameToSqlParameter =
                new Dictionary<string, SqlParameter>();

            if (parameters.Count > 0)
            {
                int countParameter = 0;
                foreach (StoredProcedureParameter sprocParameter in parameters)
                {
                    SqlDbType sqlDbType = GetSqlDbTypeFromStoredProcedureParameterDataType(sprocParameter.DataType);

                    SqlParameter sqlParameter = new SqlParameter(sprocParameter.Name, sqlDbType);
                    if (sprocParameter.IsOutputParameter)
                    {
                        sqlParameter.Direction = ParameterDirection.InputOutput;
                    }
                    dbColumnNameToSqlParameter.Add(GetColumnNameFromParameterName(sqlParameter.ParameterName), sqlParameter);
                }

            }
            return dbColumnNameToSqlParameter;
        }


        public SqlDbType GetSqlDbTypeFromStoredProcedureParameterDataType(Microsoft.SqlServer.Management.Smo.DataType smoType)
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

        public string GetColumnNameFromParameterName(string sqlParameterName)
        {
            string columnName = string.Empty;
           
            int indexOfAtSymbol = sqlParameterName.IndexOf(Sql.AT_SYMBOL);
            if (indexOfAtSymbol > -1)
            {
                columnName = sqlParameterName.Substring(indexOfAtSymbol + 1, sqlParameterName.Length - 1);
            }
            return columnName;
        }

        public string InsertSprocName(CommonLibrary.CustomAttributes.TableNameAttribute tableAttribute)
        {
            string sprocName = string.Empty;
            sprocName = SqlHelper.GenerateInsertSprocName(tableAttribute.TableName);
            return sprocName;
        }

        public string DeleteSprocName(CommonLibrary.CustomAttributes.TableNameAttribute tableAttribute)
        {
            string sprocName = string.Empty;
            sprocName = SqlHelper.GenerateDeleteByPrimaryKeySprocName(tableAttribute.TableName);
            return sprocName;
        }

        public string UpdateSprocName(CommonLibrary.CustomAttributes.TableNameAttribute tableAttribute)
        {
            string sprocName = string.Empty;
            sprocName = SqlHelper.GenerateUpdateByPrimaryKeySprocName(tableAttribute.TableName);
            return sprocName;
        }

        public string GetSprocName(CommonLibrary.CustomAttributes.TableNameAttribute tableAttribute,
                                   CommonLibrary.Enumerations.GetPermutations getPermutation)
        {
            string sprocName = string.Empty;

            switch (getPermutation)
            {
                case CommonLibrary.Enumerations.GetPermutations.ByPrimaryKey:
                    {
                        sprocName = SqlHelper.GenerateGetByPrimaryKeySprocName(tableAttribute.TableName);
                        break;
                    }
                case CommonLibrary.Enumerations.GetPermutations.ByExplicitCriteria:
                    {
                        sprocName = SqlHelper.GenerateGetByCriteriaExactSprocName(tableAttribute.TableName);
                        break;
                    }
                case CommonLibrary.Enumerations.GetPermutations.ByFuzzyCriteria:
                    {
                        sprocName = SqlHelper.GenerateGetByCriteriaFuzzySprocName(tableAttribute.TableName);
                        break;
                    }
                case CommonLibrary.Enumerations.GetPermutations.AllByColumnMappings:
                    {
                        sprocName = SqlHelper.GenerateGetAllSprocName(tableAttribute.TableName);
                        break;
                    }
            }
            return sprocName;
        }

        public List<StoredProcedureParameter> GetStoredProcedureInputParameters(string storedProcedureName)
        {
            StoredProcedureCollection sprocCollection =
                DatabaseSmoObjectsAndSettings_Property.Database_Property.StoredProcedures;
            StoredProcedure sprocFound = null;

            List<StoredProcedureParameter> storedProcParameters = new List<StoredProcedureParameter>();

            foreach (StoredProcedure sproc in sprocCollection)
            {
                if (!sproc.IsSystemObject && sproc.Name == storedProcedureName)
                {
                    sprocFound = sproc;
                    break;
                }
            }
            if (sprocFound != null)
            {
                foreach (StoredProcedureParameter parameter in sprocFound.Parameters)
                {
                    if (!parameter.IsOutputParameter)
                    {
                        storedProcParameters.Add(parameter);
                    }
                }
            }
            return storedProcParameters;
        }

        public List<StoredProcedureParameter> GetStoredProcedureInputParametersForInsert(
            string storedProcedureName,
            PropertyInfo [] properties)
        {
            StoredProcedureCollection sprocCollection =
                DatabaseSmoObjectsAndSettings_Property.Database_Property.StoredProcedures;
            StoredProcedure sprocFound = null;

            List<StoredProcedureParameter> storedProcParameters = new List<StoredProcedureParameter>();

            foreach (StoredProcedure sproc in sprocCollection)
            {
                if (!sproc.IsSystemObject && sproc.Name == storedProcedureName)
                {
                    sprocFound = sproc;
                    break;
                }
            }
            if (sprocFound != null)
            {
                foreach (StoredProcedureParameter parameter in sprocFound.Parameters)
                {
                    if (!parameter.IsOutputParameter)
                    {
                        storedProcParameters.Add(parameter);
                    }
                    else
                    {
                        foreach (PropertyInfo propertyInfo in properties)
                        {
                            string columnName = GetColumnNameFromParameterName(parameter.Name);
                            if (DoesPropertyInfoColumnAttributeMatchColumnName(propertyInfo, columnName))
                            {
                                object[] customAttributes = propertyInfo.GetCustomAttributes(false);
                                if (IsPrimaryKeyParameter(customAttributes))
                                {
                                    storedProcParameters.Add(parameter);
                                }
                                
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                throw new ApplicationException("Sproc Not Found:  " + storedProcedureName);
            }
            return storedProcParameters;
        }

        public List<StoredProcedureParameter> GetStoredProcedureInputParametersForInsert(
           string storedProcedureName,
           PropertyInfo[] properties,
            ref SqlTransaction transaction)
        {
            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder(_databaseSmoObjectsAndSettings.ConnectionString);
            DatabaseSmoObjectsAndSettings smo = new DatabaseSmoObjectsAndSettings(
                sb.InitialCatalog,
                sb.DataSource,
                sb.InitialCatalog,
                sb.UserID,
                sb.Password,
                sb.IntegratedSecurity);

            StoredProcedureCollection sprocCollection = smo.Database_Property.StoredProcedures;
            StoredProcedure sprocFound = null;

            List<StoredProcedureParameter> storedProcParameters = new List<StoredProcedureParameter>();

            foreach (StoredProcedure sproc in sprocCollection)
            {
                if (!sproc.IsSystemObject && sproc.Name == storedProcedureName)
                {
                    sprocFound = sproc;
                    break;
                }
            }
            if (sprocFound != null)
            {
                foreach (StoredProcedureParameter parameter in sprocFound.Parameters)
                {
                    if (!parameter.IsOutputParameter)
                    {
                        storedProcParameters.Add(parameter);
                    }
                    else
                    {
                        foreach (PropertyInfo propertyInfo in properties)
                        {
                            string columnName = GetColumnNameFromParameterName(parameter.Name);
                            if (DoesPropertyInfoColumnAttributeMatchColumnName(propertyInfo, columnName))
                            {
                                object[] customAttributes = propertyInfo.GetCustomAttributes(false);
                                if (IsPrimaryKeyParameter(customAttributes))
                                {
                                    storedProcParameters.Add(parameter);
                                }

                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                throw new ApplicationException("Sproc Not Found:  " + storedProcedureName);
            }
            return storedProcParameters;
        }

        public List<StoredProcedureParameter> GetStoredProcedureInputParametersForDelete(
    string storedProcedureName,
    PropertyInfo[] properties)
        {
            StoredProcedureCollection sprocCollection =
                DatabaseSmoObjectsAndSettings_Property.Database_Property.StoredProcedures;
            StoredProcedure sprocFound = null;

            List<StoredProcedureParameter> storedProcParameters = new List<StoredProcedureParameter>();

            foreach (StoredProcedure sproc in sprocCollection)
            {
                if (!sproc.IsSystemObject && sproc.Name == storedProcedureName)
                {
                    sprocFound = sproc;
                    break;
                }
            }
            if (sprocFound != null)
            {
                foreach (StoredProcedureParameter parameter in sprocFound.Parameters)
                {
                    
                    foreach (PropertyInfo propertyInfo in properties)
                    {
                        string columnName = GetColumnNameFromParameterName(parameter.Name);
                        if (DoesPropertyInfoColumnAttributeMatchColumnName(propertyInfo, columnName))
                        {
                            object[] customAttributes = propertyInfo.GetCustomAttributes(false);
                            if (IsPrimaryKeyParameter(customAttributes))
                            {
                                storedProcParameters.Add(parameter);
                            }
                            break;
                        }
                    }
                    
                }
            }
            return storedProcParameters;
        }

        public List<StoredProcedureParameter> GetStoredProcedureInputParametersForUpdate(
    string storedProcedureName,
    PropertyInfo[] properties)
        {
            StoredProcedureCollection sprocCollection =
                DatabaseSmoObjectsAndSettings_Property.Database_Property.StoredProcedures;
            StoredProcedure sprocFound = null;

            List<StoredProcedureParameter> storedProcParameters = new List<StoredProcedureParameter>();

            foreach (StoredProcedure sproc in sprocCollection)
            {
                if (!sproc.IsSystemObject && sproc.Name == storedProcedureName)
                {
                    sprocFound = sproc;
                    break;
                }
            }
            if (sprocFound != null)
            {
                foreach (StoredProcedureParameter parameter in sprocFound.Parameters)
                {                    
                    storedProcParameters.Add(parameter);                   
                }
            }
            return storedProcParameters;
        }

        public List<StoredProcedureParameter> GetStoredProcedureInputParametersForDelete(
string storedProcedureName,
PropertyInfo[] properties,
            ref SqlTransaction transaction,
            TransactionBehavior transactionBehavior)
        {
            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder(_databaseSmoObjectsAndSettings.ConnectionString);
            DatabaseSmoObjectsAndSettings smo = new DatabaseSmoObjectsAndSettings(
                sb.InitialCatalog,
                sb.DataSource,
                sb.InitialCatalog,
                sb.UserID,
                sb.Password,
                sb.IntegratedSecurity);

            StoredProcedureCollection sprocCollection = smo.Database_Property.StoredProcedures;
            StoredProcedure sprocFound = null;

            List<StoredProcedureParameter> storedProcParameters = new List<StoredProcedureParameter>();

            foreach (StoredProcedure sproc in sprocCollection)
            {
                if (!sproc.IsSystemObject && sproc.Name == storedProcedureName)
                {
                    sprocFound = sproc;
                    break;
                }
            }
            if (sprocFound != null)
            {
                foreach (StoredProcedureParameter parameter in sprocFound.Parameters)
                {

                    foreach (PropertyInfo propertyInfo in properties)
                    {
                        string columnName = GetColumnNameFromParameterName(parameter.Name);
                        if (DoesPropertyInfoColumnAttributeMatchColumnName(propertyInfo, columnName))
                        {
                            object[] customAttributes = propertyInfo.GetCustomAttributes(false);
                            if (IsPrimaryKeyParameter(customAttributes))
                            {
                                storedProcParameters.Add(parameter);
                            }
                            break;
                        }
                    }

                }
            }
            return storedProcParameters;
        }


        public List<StoredProcedureParameter> GetStoredProcedureInputParametersForUpdate(
string storedProcedureName,
PropertyInfo[] properties,ref SqlTransaction transaction, TransactionBehavior transactionBehavior)
        {
            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder(_databaseSmoObjectsAndSettings.ConnectionString);
            DatabaseSmoObjectsAndSettings smo = new DatabaseSmoObjectsAndSettings(
                sb.InitialCatalog,
                sb.DataSource,
                sb.InitialCatalog,
                sb.UserID,
                sb.Password,
                sb.IntegratedSecurity);

            StoredProcedureCollection sprocCollection = smo.Database_Property.StoredProcedures;
            StoredProcedure sprocFound = null;

            List<StoredProcedureParameter> storedProcParameters = new List<StoredProcedureParameter>();

            foreach (StoredProcedure sproc in sprocCollection)
            {
                if (!sproc.IsSystemObject && sproc.Name == storedProcedureName)
                {
                    sprocFound = sproc;
                    break;
                }
            }
            if (sprocFound != null)
            {
                foreach (StoredProcedureParameter parameter in sprocFound.Parameters)
                {
                    storedProcParameters.Add(parameter);
                }
            }
            return storedProcParameters;
        }


      
        public bool IsPrimaryKeyParameter(object[] attributes)
        {
            bool isPrimaryKeyParameter = false;
            foreach (object attribute in attributes)
            {
                if (attribute is CommonLibrary.CustomAttributes.PrimaryKey)
                {
                    isPrimaryKeyParameter = true;
                    break;
                }
            }
            return isPrimaryKeyParameter;
        }

      

        public Type GetTypeOfDtoFromGenericType(T genericType)
        {
            Type returnType = genericType.GetType();
            return returnType;
        }

        


    }
}
