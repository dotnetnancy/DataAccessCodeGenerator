using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

using SprocDataLayerGenerator.Data;
using CommonLibrary.Base.Database;
using ColumnMapping = SprocDataLayerGenerator.Column.Mapping.InformationSchemaConstraintColumnUsage;


namespace SprocDataLayerGenerator.List
{
    public class InformationSchemaConstraintColumnUsage : List<Data.InformationSchemaConstraintColumnUsage>
    {
        private BaseDatabase _baseDatabase = new BaseDatabase();

        public InformationSchemaConstraintColumnUsage()
        {
        }

        public InformationSchemaConstraintColumnUsage(SqlDataReader reader)
        {
            AddItemsToListBySqlDataReader(reader);
        }

        private void AddItemsToListBySqlDataReader(SqlDataReader reader)
        {
            while (reader.Read())
            {
                Data.InformationSchemaConstraintColumnUsage informationSchemaConstraintColumnUsageData =
                    new Data.InformationSchemaConstraintColumnUsage();

                informationSchemaConstraintColumnUsageData.ConstraintCatalog =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.CONSTRAINT_CATALOG),
                    reader);

                informationSchemaConstraintColumnUsageData.ConstraintName =
                    _baseDatabase.resolveNullString(reader.GetOrdinal(ColumnMapping.CONSTRAINT_NAME),
                    reader);

                informationSchemaConstraintColumnUsageData.ConstraintSchema =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.CONSTRAINT_SCHEMA),
                    reader);          

                informationSchemaConstraintColumnUsageData.TableCatalog =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.TABLE_CATALOG),
                    reader);

                informationSchemaConstraintColumnUsageData.TableName =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.TABLE_NAME),
                    reader);

                informationSchemaConstraintColumnUsageData.TableSchema =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.TABLE_SCHEMA),
                    reader);

                informationSchemaConstraintColumnUsageData.ColumnName =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.COLUMN_NAME),
                    reader);

                this.Add(informationSchemaConstraintColumnUsageData);
            }
        }

    }
}
