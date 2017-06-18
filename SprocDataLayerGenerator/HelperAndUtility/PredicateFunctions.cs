using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Server;
using System.Reflection;



namespace SprocDataLayerGenerator
{
    public class PredicateFunctions
    {
        private string _tableNameHolder = null;
        private string _constraintNameHolder = null;
        private string _columnNameHolder = null;
        private string _storedProcedureParameterUserDataHolder = null;
        private string _assemblyFullName = null;
        private string _sprocNameHolder = null;
        private string _referencedTypeHolder = null;
        private string _referencedObjectHolder = null;
        private string _referencingObjectHolder = null;
        private string _publicPropertyNameHolder = null;
        private int? _colOrdinalHolder = null;

        public int? ColOrdinalHolder
        {
            get { return _colOrdinalHolder; }
            set { _colOrdinalHolder = value; }
        }

        public string PublicPropertyNameHolder
        {
            get { return _publicPropertyNameHolder; }
            set { _publicPropertyNameHolder = value; }
        }

        public string ReferencingObjectHolder
        {
            get { return _referencingObjectHolder; }
            set { _referencingObjectHolder = value; }
        }

        public string ReferencedObjectHolder
        {
            get
            {
                return _referencedObjectHolder;
            }
            set
            {
                _referencedObjectHolder = value;
            }
        }

        public string ReferencedTypeHolder
        {
            get { return _referencedTypeHolder; }
            set { _referencedTypeHolder = value; }
        }
        
       
        public PredicateFunctions()
        {
        }

        public string TableNameHolder
        {
            get
            {
                return _tableNameHolder;
            }
            set
            {
                _tableNameHolder = value;
            }
        }

        public string ConstraintNameHolder
        {
            get
            {
                return _constraintNameHolder;
            }
            set
            {
                _constraintNameHolder = value;
            }
        }

        public string ColumnNameHolder
        {
            get { return _columnNameHolder; }
            set { _columnNameHolder = value; }
        }

        public string StoredProcedureParameterUserDataHolder
        {
            get { return _storedProcedureParameterUserDataHolder; }
            set { _storedProcedureParameterUserDataHolder = value; }
        }

        public string AssemblyFullName
        {
            get { return _assemblyFullName; }
            set { _assemblyFullName = value; }
        }

        public string SprocNameHolder
        {
            get { return _sprocNameHolder; }
            set { _sprocNameHolder = value; }
        }


        public bool FindTableConstraintByTableName(Data.InformationSchemaTableConstraint tableConstraint)
        {
            bool found = false;
            if (tableConstraint.TableName == TableNameHolder)
            {
                found = true;
            }
            return found;
        }

        public bool FindTableConstraintByConstraintName(Data.InformationSchemaTableConstraint tableConstraint)
        {
            bool found = false;
            if (tableConstraint.ConstraintName == ConstraintNameHolder)
            {
                found = true;
            }
            return found;
        }

        public bool FindTableColumnByTableName(Data.InformationSchemaColumn tableColumn)
        {
            bool found = false;
            if (tableColumn.TableName == TableNameHolder)
            {
                found = true;
            }
            return found;
        }

        public bool FindConstraintColumnUsageByTableName(Data.InformationSchemaConstraintColumnUsage constraintColumnUsage)
        {
            bool found = false;
            if (constraintColumnUsage.TableName == TableNameHolder)
            {
                found = true;
            }
            return found;
        }

        public bool FindConstraintColumnUsageByColumnNameAndTableName(Data.InformationSchemaConstraintColumnUsage constraintColumnUsage)
        {
            bool found = false;
            if (constraintColumnUsage.TableName == TableNameHolder
                && constraintColumnUsage.ColumnName == ColumnNameHolder)
            {
                found = true;
            }
            return found;
        }


        public bool FindConstraintColumnUsageByConstraintName(Data.InformationSchemaConstraintColumnUsage constrainColumnUsage)
        {
            bool found = false;
            if (constrainColumnUsage.ConstraintName == ConstraintNameHolder)
            {
                found = true;
            }
            return found;
        }

        public bool FindInformationSchemaColumn(Data.InformationSchemaColumn column)
        {
            bool found = false;
            if (column.ColumnName == ColumnNameHolder)
            {
                found = true;
            }
            return found;
        }

        public bool FindInformationSchemaConstraintColumnUsageByColumnName(Data.InformationSchemaConstraintColumnUsage constraintColumnUsage)
        {
            bool found = false;
            if (constraintColumnUsage.ColumnName == ColumnNameHolder)
            {
                found = true;
            }
            return found;
        }       

        public bool FindConstraintColumnUsageByTableNameAndConstraintName(Data.InformationSchemaConstraintColumnUsage constraintUsage)
        {
            bool found = false;
            if (constraintUsage.TableName == TableNameHolder
                && constraintUsage.ConstraintName == ConstraintNameHolder)
            {
                found = true;
            }
            return found;

        }

        public bool FindSqlParameterByUserData(StoredProcedureParameter parameter)
        {
            bool found = false;
            if (parameter.UserData == StoredProcedureParameterUserDataHolder)
            {
                found = true;
            }
            return found;
        }

        public bool FindMetaInformationSchemaByTableName(Data.MetaInformationSchema schema)
        {
            bool found = false;
            if (schema.MetaTable.TableName == TableNameHolder)
            {
                found = true;
            }
            return found;
        }

        public bool FindAssemblyLoadedInMemoryByFullAssemblyName(Object assembly)
        {
            bool found = false;
            if (assembly.GetType().FullName == AssemblyFullName)
            {
                found = true;
            }
            return found;
        }

        public bool FindSprocGeneratedBySprocName(StoredProcedure sproc)
        {
            bool found = false;
            if (this.SprocNameHolder == sproc.Name)
            {
                found = true;
            }
            return found;
        }

        public bool FindMetaSqlDependenciesByReferencedType(Data.MetaSqlDependency metaSqlDependency)
        {
            bool found = false;
            if (metaSqlDependency.ReferencedType.Trim() == ReferencedTypeHolder.Trim())
            {
                found = true;
            }
            return found;
        }

        public bool FindMetaSqlDependenciesByReferencedObject(Data.MetaSqlDependency metaSqlDependency)
        {
            bool found = false;
            if (metaSqlDependency.ReferencedObject.Trim() == ReferencedObjectHolder.Trim())
            {
                found = true;
            }
            return found;
        }

        

        public bool FindMetaSqlSprocBySprocName(Data.MetaSprocSqlDependency metaSprocSqlDependency)
        {
            bool found = false;
            if (metaSprocSqlDependency.MainStoredProcedure.Trim() == SprocNameHolder)
            {
                found = true;
            }
            return found;
        }

        public bool FindPropertyInfoByPublicPropertyName(PropertyInfo propertyInfo)
        {
            bool found = false;
            if (propertyInfo.Name == this.PublicPropertyNameHolder)
            {
                found = true;
            }
            return found;
        }

        public bool FindInformationSchemaColumnByColumnOrdinal(Data.InformationSchemaColumn column)
        {
            bool found = false;
            if (column.OrdinalPosition == this.ColOrdinalHolder)
            {
                found = true;
            }
            return found;
        }

        
    }


}
