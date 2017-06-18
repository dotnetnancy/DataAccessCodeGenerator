using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Text;

using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Server;

using CommonLibrary;

using SprocDataLayerGenerator.Data;
using SprocDataLayerGenerator.Data.Access;
using SprocDataLayerGenerator.Sproc.Mapping;
using SqlDbTypeConstants = CommonLibrary.Constants.SqlDbConstants;
using SqlNativeTypeConstants = CommonLibrary.Constants.NativeSqlConstants;

namespace SprocDataLayerGenerator.BusinessObjects
{
    public class MetaSprocSqlDependencyManager
    {
        private DatabaseSmoObjectsAndSettings _smoObjectsAndSettings = null;
        private List<MetaSprocSqlDependency> _metaSprocSqlDependencyList = new List<MetaSprocSqlDependency>();

        public List<MetaSprocSqlDependency> MetaSprocSqlDependencyList
        {
            get { return _metaSprocSqlDependencyList; }
            set { _metaSprocSqlDependencyList = value; }
        }

        public MetaSprocSqlDependencyManager(DatabaseSmoObjectsAndSettings databaseSmoObjectsAndSettings,
                                             List<StoredProcedure> sprocs)
        {
            _smoObjectsAndSettings = databaseSmoObjectsAndSettings;
            AddDependecyStoredProcedures();
            foreach (StoredProcedure sproc in sprocs)
            {
                SetMetaDataList(sproc.Name);
            }
            SetRecursiveSprocDependencyDictionary();           
        }     

        public MetaSprocSqlDependencyManager(DatabaseSmoObjectsAndSettings databaseSmoObjectsAndSettings,
                                             string sprocName)
        {
            _smoObjectsAndSettings = databaseSmoObjectsAndSettings;
            AddDependecyStoredProcedures();
            SetMetaDataList(sprocName);
            SetRecursiveSprocDependencyDictionary();           
        }

        public void SetRecursiveSprocDependencyDictionary()
        {
            PredicateFunctions predicateFunctions = new PredicateFunctions();
           

            foreach (MetaSprocSqlDependency metaSprocSqlDependency in _metaSprocSqlDependencyList)
            {
                
                List<string> distinctSprocDependencyNames = 
                    GetDistinctSprocDependecyList(metaSprocSqlDependency.SprocDependencies);

                foreach (string sprocDependencyName in distinctSprocDependencyNames)
                {
                    predicateFunctions.SprocNameHolder = sprocDependencyName;
                    MetaSprocSqlDependency sprocRecursiveDependency =
                        _metaSprocSqlDependencyList.Find(predicateFunctions.FindMetaSqlSprocBySprocName);
                    sprocRecursiveDependency.RecursiveSprocNameToMetaSprocSqlDependency.Add(sprocDependencyName, sprocRecursiveDependency);                   
                }
               

            }


        }

       
        public void SetMetaDataList(string sprocName)
        {
            SprocDataLayerGeneratorDataAccess dataAccess = new SprocDataLayerGeneratorDataAccess();
                       
                List<MetaSqlDependency> allDependenciesForSproc = 
                    dataAccess.GetMetaSqlDependency(_smoObjectsAndSettings.ConnectionString,sprocName);
                //returns all dependencies for one sproc
                MetaSprocSqlDependency oneMetaSprocSqlDependency =
                     this.GetMetaSprocSqlDependencyForOneSproc(allDependenciesForSproc,
                                                                                 sprocName);
                if (oneMetaSprocSqlDependency != null)
                {
                    _metaSprocSqlDependencyList.Add(oneMetaSprocSqlDependency);

                    if (oneMetaSprocSqlDependency.SprocDependencies.Count > 0)
                    {
                        foreach (MetaSqlDependency sprocDependency in oneMetaSprocSqlDependency.SprocDependencies)
                        {
                            SetMetaDataList(sprocDependency.ReferencedObject);
                        }
                    }
                }
            

        }

        public MetaSprocSqlDependency GetMetaSprocSqlDependencyForOneSproc(List<MetaSqlDependency> dependenciesForSproc,
                                                                            string sprocName)
        {
            PredicateFunctions predicateFunctions = new PredicateFunctions();
            string sprocTypeString = Constants.MetaSqlOrSprocDependencyConstants.STORED_PROCEDURE_TYPE;
            string userTableTypeString = Constants.MetaSqlOrSprocDependencyConstants.USER_TABLE_TYPE;

            List.MetaSqlDependency sprocTypeDependencies = new List.MetaSqlDependency();
            List.MetaSqlDependency userTableTypeDependencies = new List.MetaSqlDependency();
            Dictionary<string, List<MetaSqlDependency>> listOfUserTableToColumnsReferenced =
                new Dictionary<string, List<MetaSqlDependency>>();
            List<string> distinctUserTablesList = new List<string>();

            predicateFunctions.ReferencedTypeHolder = sprocTypeString;

            sprocTypeDependencies.AddRange(dependenciesForSproc.FindAll(predicateFunctions.FindMetaSqlDependenciesByReferencedType));

            predicateFunctions.ReferencedTypeHolder = userTableTypeString;

            userTableTypeDependencies.AddRange(dependenciesForSproc.FindAll(predicateFunctions.FindMetaSqlDependenciesByReferencedType));

            if (userTableTypeDependencies.Count > 0)
            {
                distinctUserTablesList = GetDistinctUserTableList(userTableTypeDependencies);
                foreach (string distinctUserTableName in distinctUserTablesList)
                {
                    List<MetaSqlDependency> columnsReferenced = new List<MetaSqlDependency>();
                    predicateFunctions.ReferencedObjectHolder = distinctUserTableName;
                    columnsReferenced.AddRange(userTableTypeDependencies.FindAll(predicateFunctions.FindMetaSqlDependenciesByReferencedObject));
                    if (columnsReferenced.Count > 0)
                    {
                        listOfUserTableToColumnsReferenced.Add(distinctUserTableName, columnsReferenced);
                    }
                }
            }
            MetaSprocSqlDependency metaSprocSqlDependency =
                new MetaSprocSqlDependency(sprocName, 
                                           sprocTypeDependencies, 
                                           userTableTypeDependencies, 
                                           listOfUserTableToColumnsReferenced,
                                           distinctUserTablesList);
            return metaSprocSqlDependency;

        }

        public List<string> GetDistinctSprocDependecyList(List<MetaSqlDependency> sprocTypeDependencies)
        {                   
            List<string> distinctListOfSprocs = new List<string>();
            string currentReferencedObject = string.Empty;
            foreach (MetaSqlDependency sprocTypeDependency in sprocTypeDependencies)
            {
                if (currentReferencedObject == string.Empty)
                {
                    currentReferencedObject = sprocTypeDependency.ReferencedObject;
                    distinctListOfSprocs.Add(currentReferencedObject);
                }
                else
                {
                    currentReferencedObject = sprocTypeDependency.ReferencedObject;
                    if (!distinctListOfSprocs.Contains(currentReferencedObject))
                    {
                        distinctListOfSprocs.Add(currentReferencedObject);
                    }
                }
            }
            return distinctListOfSprocs;
        }      

        
        public List<string> GetDistinctUserTableList(List<MetaSqlDependency> userTableTypeDependencies)
        {
            List<string> distinctListOfUserTables = new List<string>();
            string currentReferencedObject = string.Empty;
            foreach (MetaSqlDependency userTableTypeDependency in userTableTypeDependencies)
            {
                if (currentReferencedObject == string.Empty)
                {
                    currentReferencedObject = userTableTypeDependency.ReferencedObject;
                    distinctListOfUserTables.Add(currentReferencedObject);
                }
                else
                {
                    currentReferencedObject = userTableTypeDependency.ReferencedObject;
                    if (!distinctListOfUserTables.Contains(currentReferencedObject))
                    {
                        distinctListOfUserTables.Add(currentReferencedObject);
                    }
                }
            }
            return distinctListOfUserTables;
        }      

        public void AddDependecyStoredProcedures()
        {
            Database db = _smoObjectsAndSettings.Database_Property;
            StoredProcedure sp = GetDependencyStoredProcedure(Sproc.Mapping.GetDependency.GET_DEPENDENCY,
                                                              db,
                                                              MetaSprocDependencyConstants.GET_DEPENDENCY);
          

            if (db.StoredProcedures.Contains(Sproc.Mapping.GetDependency.GET_DEPENDENCY))
            {
                db.StoredProcedures[Sproc.Mapping.GetDependency.GET_DEPENDENCY].Drop();
            }
            sp.Create();

            sp = GetDependencyStoredProcedure(Sproc.Mapping.AllDependencies.GET_DEPENDENCIES,
                                              db,
                                              MetaSprocDependencyConstants.GET_DEPENDENCIES);
           
            if (db.StoredProcedures.Contains(Sproc.Mapping.AllDependencies.GET_DEPENDENCIES))
            {
                db.StoredProcedures[Sproc.Mapping.AllDependencies.GET_DEPENDENCIES].Drop();
            }
            sp.Create();
        }

        private StoredProcedure GetDependencyStoredProcedure(string procedureName,
                                                            Database db,
                                                            string procedureBody)
        {
            StoredProcedure sp = new StoredProcedure(db, procedureName);

            StoredProcedureParameter sprocName = new StoredProcedureParameter(sp,
                Sproc.Mapping.FilterParameter.SPROC_NAME_PARAM, DataType.NVarChar(255));

            List<StoredProcedureParameter> parameters = new List<StoredProcedureParameter>();
            parameters.Add(sprocName);

            sp.TextMode = false;
            sp.AnsiNullsStatus = false;
            sp.QuotedIdentifierStatus = false;
            sp.TextBody = procedureBody;

            foreach (StoredProcedureParameter parameter in parameters)
            {
                //tableName.DefaultValue = "null";
                sp.Parameters.Add(parameter);
            }


            return sp;
        }
    }
}
