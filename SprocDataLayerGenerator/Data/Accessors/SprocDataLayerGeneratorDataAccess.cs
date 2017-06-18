using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

using SprocDataLayerGenerator.Data;
using CommonLibrary.Base.Database;
using SprocDataLayerGenerator.Sproc.Mapping;
using SprocDataLayerGenerator.List;

namespace SprocDataLayerGenerator.Data.Access
{
    public class SprocDataLayerGeneratorDataAccess : BaseDatabase
    {

        private string _tableNameHolder = string.Empty;       

        public SprocDataLayerGeneratorDataAccess()
        {            
        }

        public List<MetaSqlDependency> GetMetaSqlDependency(string connectionString, string sprocName)
        {
            return GetSqlDependencyForCustomSprocs(connectionString,sprocName);

        }

        public List<MetaInformationSchema> GetSchema(string connectionString)
        {
            return GetSchemaForUserTables(connectionString);
        }

        public MetaInformationSchema GetSchema(string connectionString, string tableName)
        {
            return GetSchemaForUserTable(connectionString, tableName);
        }

        private List<MetaSqlDependency> GetSqlDependencyForCustomSprocs(string connectionString, string sprocName)
        {
            SqlParameter sprocNameParameter = this.getInputNVarCharParameter(sprocName, Sproc.Mapping.FilterParameter.SPROC_NAME_PARAM);

            SqlDataReader dependencyReader = this.getDataReaderFromSP(connectionString,
                                                                      Sproc.Mapping.AllDependencies.GET_DEPENDENCIES,
                                                                      sprocNameParameter);
            return GetSqlDependencyForCustomSprocs(dependencyReader);
        }

        private List<MetaInformationSchema> GetSchemaForUserTables(string connectionString)
        {
            SqlDataReader schemaReader = this.getDataReaderFromSP(connectionString, 
                                                                  Sproc.Mapping.All.INFORMATION_SCHEMA);
            return GetSchemaForUserTables(schemaReader);
        }

        private MetaInformationSchema GetSchemaForUserTable(string connectionString, string tableName)
        {
            SqlParameter tableNameParameter = this.getInputNVarCharParameter(tableName,
                                                                            Sproc.Mapping.FilterParameter.TABLE_NAME_PARAM);

            SqlDataReader schemaReader = this.getDataReaderFromSP(connectionString,
                                                                  Sproc.Mapping.All.INFORMATION_SCHEMA,
                                                                  tableNameParameter);
            return GetSchemaForUserTable(schemaReader);
        }      

        private MetaInformationSchema GetSchemaForUserTable(SqlDataReader schemaReader)
        {
            List<MetaInformationSchema> metaData = GetSchemaForUserTables(schemaReader);
            if (metaData != null)
            {
                if (metaData.Count == 1)
                {
                    return metaData[0];
                }               
            }
            return null;
        }

        private List<MetaSqlDependency> GetSqlDependencyForCustomSprocs(SqlDataReader dependencyReader)
        {
            List.MetaSqlDependency dependencies = null;
            using (dependencyReader)
            {
                int count = 0;
                do
                {
                    switch (count)
                    {
                        case (int)Sproc.Mapping.DependencyResultSets.GET_DEPENDENCY:
                            {
                                dependencies = 
                                    new SprocDataLayerGenerator.List.MetaSqlDependency(dependencyReader);
                                break;
                            }                   
                    }
                    count++;
                } while (dependencyReader.NextResult());
            }
            return dependencies;        
        }
        

        private List<MetaInformationSchema> GetSchemaForUserTables(SqlDataReader schemaReader)
        {
            List.InformationSchemaTable tables = null;
            List.InformationSchemaTableConstraint tableConstraints = null;
            List.InformationSchemaColumn tableColumns = null;
            List.InformationSchemaConstraintColumnUsage constraintColumnUsage = null;

            using (schemaReader)
            {
                int count = 0;
                do
                {
                    switch (count)
                    {
                        case (int)Sproc.Mapping.AllResultSets.INFORMATION_SCHEMA_TABLE:
                            {
                                tables = 
                                    new SprocDataLayerGenerator.List.InformationSchemaTable(schemaReader);
                                break;
                            }
                        case (int)Sproc.Mapping.AllResultSets.INFORMATION_SCHEMA_TABLE_CONSTRAINTS:
                            {
                                tableConstraints =
                                    new SprocDataLayerGenerator.List.InformationSchemaTableConstraint(schemaReader);
                                break;
                            }
                        case (int)Sproc.Mapping.AllResultSets.INFORMATION_SCHEMA_COLUMN:
                            {
                                tableColumns =
                                    new SprocDataLayerGenerator.List.InformationSchemaColumn(schemaReader);
                                break;
                            }
                        case (int)Sproc.Mapping.AllResultSets.INFORMATION_SCHEMA_CONSTRAINT_COLUMN_USAGE:
                            {
                                constraintColumnUsage =
                                    new SprocDataLayerGenerator.List.InformationSchemaConstraintColumnUsage(schemaReader);
                                break;
                            }                          
                    }
                    count++;
                } while (schemaReader.NextResult());
            }

            return GetMetaInformationSchemaFromLists(tables, tableConstraints, tableColumns, constraintColumnUsage);
        }

        public List<MetaInformationSchema> GetMetaInformationSchemaFromLists(List.InformationSchemaTable tables,
                                                                          List.InformationSchemaTableConstraint tableConstraints,
                                                                          List.InformationSchemaColumn tableColumns,
                                                                          List.InformationSchemaConstraintColumnUsage constraintColumnUsage)
        {
            List<MetaInformationSchema> allSchemas = new List<MetaInformationSchema>();
            PredicateFunctions predicateFunctions = new PredicateFunctions();
            if (tables != null)
            {
                foreach (Data.InformationSchemaTable table in tables)
                {
                    MetaInformationSchema oneSchema = new MetaInformationSchema();
                    oneSchema.MetaTable = table;

                    predicateFunctions.TableNameHolder = table.TableName;

                    if (tableConstraints != null)
                    {
                        oneSchema.MetaTableConstraints.AddRange(tableConstraints.FindAll(predicateFunctions.FindTableConstraintByTableName));
                    }
                    if (tableColumns != null)
                    {
                        oneSchema.MetaColumns.AddRange(tableColumns.FindAll(predicateFunctions.FindTableColumnByTableName));
                    }
                    if (constraintColumnUsage != null)
                    {
                        oneSchema.MetaConstraintColumnUsage.AddRange(constraintColumnUsage.FindAll(predicateFunctions.FindConstraintColumnUsageByTableName));
                    }
                    allSchemas.Add(oneSchema);
                }
            }
            return allSchemas;
        }

    }
}
