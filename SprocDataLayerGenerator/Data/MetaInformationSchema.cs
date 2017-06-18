using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

using SprocDataLayerGenerator.Data;

namespace SprocDataLayerGenerator.Data
{
    public class MetaInformationSchema
    {
        private Data.InformationSchemaTable _metaTable = 
            new Data.InformationSchemaTable();

        private List.InformationSchemaColumn _metaColumns = 
            new List.InformationSchemaColumn();

        private List.InformationSchemaTableConstraint _metaTableConstraints = 
            new List.InformationSchemaTableConstraint();

        private List.InformationSchemaConstraintColumnUsage _metaConstraintColumnUsage =
            new SprocDataLayerGenerator.List.InformationSchemaConstraintColumnUsage();

        Dictionary<Data.InformationSchemaColumn,
            List.InformationSchemaConstraintColumnUsage> _metaColumnToConstraintColumnUsage =
            new Dictionary<InformationSchemaColumn, List.InformationSchemaConstraintColumnUsage>();
        
        public MetaInformationSchema()
        {           
        }

        public List.InformationSchemaConstraintColumnUsage MetaConstraintColumnUsage
        {
            get { return _metaConstraintColumnUsage; }
            set { _metaConstraintColumnUsage = value; }
        }


        public Data.InformationSchemaTable MetaTable
        {
            get { return _metaTable; }
            set { _metaTable = value; }
        }

        public List.InformationSchemaColumn MetaColumns
        {
            get { return _metaColumns; }
            set { _metaColumns = value; }
        }

        public List.InformationSchemaTableConstraint MetaTableConstraints
        {
            get { return _metaTableConstraints; }
            set { _metaTableConstraints = value; }
        }

        public Dictionary<Data.InformationSchemaColumn,
            List.InformationSchemaConstraintColumnUsage> MetaColumnToConstraintColumnUsage
        {
            get
            {
                if (this._metaColumnToConstraintColumnUsage.Count == 0)
                {
                    this.FillMetaColumnToConstraintColumnUsage();                   
                }
                return _metaColumnToConstraintColumnUsage;
            }            
        }

        private void FillMetaColumnToConstraintColumnUsage()
        {
            PredicateFunctions predicateFunctions = new PredicateFunctions();
            foreach (Data.InformationSchemaColumn column in MetaColumns)
            {
                predicateFunctions.TableNameHolder = column.TableName;
                predicateFunctions.ColumnNameHolder = column.ColumnName;
                List.InformationSchemaConstraintColumnUsage columnUsage = 
                    new List.InformationSchemaConstraintColumnUsage();
                columnUsage.AddRange(this.MetaConstraintColumnUsage.FindAll(predicateFunctions.FindConstraintColumnUsageByColumnNameAndTableName));
                if (columnUsage.Count > 0)
                {
                    _metaColumnToConstraintColumnUsage.Add(column, columnUsage);
                }

            }
        }

    }
}
