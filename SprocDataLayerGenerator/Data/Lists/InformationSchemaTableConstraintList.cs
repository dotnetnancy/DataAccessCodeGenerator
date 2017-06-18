using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

using SprocDataLayerGenerator.Data;
using CommonLibrary.Base.Database;
using ColumnMapping = SprocDataLayerGenerator.Column.Mapping.InformationSchemaTableConstraintColumn;


namespace SprocDataLayerGenerator.List
{
    public class InformationSchemaTableConstraint : List<Data.InformationSchemaTableConstraint>
    {
        private BaseDatabase _baseDatabase = new BaseDatabase();

        public InformationSchemaTableConstraint()
        {
        }

        public InformationSchemaTableConstraint(SqlDataReader reader)
        {
            AddItemsToListBySqlDataReader(reader);
        }

        private void AddItemsToListBySqlDataReader(SqlDataReader reader)
        {
            while (reader.Read())
            {
                Data.InformationSchemaTableConstraint informationSchemaTableConstraint =
                    new Data.InformationSchemaTableConstraint();

                informationSchemaTableConstraint.ConstraintCatalog =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.CONSTRAINT_CATALOG),
                    reader);

                informationSchemaTableConstraint.ConstraintName =
                    _baseDatabase.resolveNullString(reader.GetOrdinal(ColumnMapping.CONSTRAINT_NAME),
                    reader);

                informationSchemaTableConstraint.ConstraintSchema =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.CONSTRAINT_SCHEMA),
                    reader);

                informationSchemaTableConstraint.ConstraintType =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.CONSTRAINT_TYPE),
                    reader);

                informationSchemaTableConstraint.InitiallyDeferred =
                    _baseDatabase.resolveNullString(reader.GetOrdinal(ColumnMapping.INITIALLY_DEFERRED),
                    reader);

                informationSchemaTableConstraint.IsDeferrable =
                    _baseDatabase.resolveNullString(reader.GetOrdinal(ColumnMapping.IS_DEFERRABLE),
                    reader);

                informationSchemaTableConstraint.TableCatalog =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.TABLE_CATALOG),
                    reader);

                informationSchemaTableConstraint.TableName =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.TABLE_NAME),
                    reader);

                informationSchemaTableConstraint.TableSchema =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.TABLE_SCHEMA),
                    reader);

                this.Add(informationSchemaTableConstraint);
            }
        }

    }
}
