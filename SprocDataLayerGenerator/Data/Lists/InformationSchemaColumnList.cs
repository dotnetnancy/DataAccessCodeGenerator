using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

using SprocDataLayerGenerator.Data;
using CommonLibrary.Base.Database;
using ColumnMapping = SprocDataLayerGenerator.Column.Mapping.InformationSchemaColumn;

namespace SprocDataLayerGenerator.List
{
    public class InformationSchemaColumn : List<Data.InformationSchemaColumn>
    {
        BaseDatabase _baseDatabase = new BaseDatabase();

        public InformationSchemaColumn()
        {
        }

        public InformationSchemaColumn(SqlDataReader reader)
        {
            AddItemsToListBySqlDataReader(reader);
        }

        public void AddItemsToListBySqlDataReader(SqlDataReader reader)
        {
            while (reader.Read())
            {
                Data.InformationSchemaColumn informationSchemaColumnData =
                    new Data.InformationSchemaColumn();

                informationSchemaColumnData.CharacterMaximumLength =
                    _baseDatabase.resolveNullInt32ToNullableDataType(reader.GetOrdinal(ColumnMapping.CHARACTER_MAXIMUM_LENGTH),
                    reader);
                informationSchemaColumnData.CharacterOctetLength =
                    _baseDatabase.resolveNullInt32ToNullableDataType(reader.GetOrdinal(ColumnMapping.CHARACTER_OCTET_LENGTH),
                    reader);
                informationSchemaColumnData.CharacterSetCatalog =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.CHARACTER_SET_CATALOG),
                    reader);
                informationSchemaColumnData.CharacterSetName =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.CHARACTER_SET_NAME),
                    reader);
                informationSchemaColumnData.CharacterSetSchema =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.CHARACTER_SET_SCHEMA),
                    reader);
                informationSchemaColumnData.CollationCatalog =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.COLLATION_CATALOG),
                    reader);
                informationSchemaColumnData.CollationName =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.COLLATION_NAME),
                    reader);
                informationSchemaColumnData.CollationSchema =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.COLLATION_SCHEMA),
                    reader);
                informationSchemaColumnData.ColumnDefault =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.COLUMN_DEFAULT),
                    reader);
                informationSchemaColumnData.ColumnName =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.COLUMN_NAME),
                    reader);
                informationSchemaColumnData.DataType =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.DATA_TYPE),
                    reader);
                informationSchemaColumnData.DatetimePrecision =
                    _baseDatabase.resolveNullInt32ToNullableDataType(reader.GetOrdinal(ColumnMapping.DATETIME_PRECISION),
                    reader);
                informationSchemaColumnData.DomainCatalog =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.DOMAIN_CATALOG),
                    reader);
                informationSchemaColumnData.DomainName =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.DOMAIN_CATALOG),
                    reader);
                informationSchemaColumnData.DomainSchema =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.DOMAIN_NAME),
                    reader);
                informationSchemaColumnData.IsNullable =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.IS_NULLABLE),
                    reader);
                informationSchemaColumnData.NumericPrecision = 
                    _baseDatabase.resolveNullInt32ToNullableDataType(reader.GetOrdinal(ColumnMapping.NUMERIC_PRECISION),
                    reader);
                informationSchemaColumnData.NumericPrecisionRadix =
                    _baseDatabase.resolveNullInt32ToNullableDataType(reader.GetOrdinal(ColumnMapping.NUMERIC_PRECISION_RADIX),
                    reader);
                informationSchemaColumnData.NumericScale =
                    _baseDatabase.resolveNullInt32ToNullableDataType(reader.GetOrdinal(ColumnMapping.NUMERIC_SCALE),
                    reader);
                informationSchemaColumnData.OrdinalPosition =
                    _baseDatabase.resolveNullInt32ToNullableDataType(reader.GetOrdinal(ColumnMapping.ORDINAL_POSITION),
                    reader);
                informationSchemaColumnData.TableCatalog =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.TABLE_CATALOG),
                    reader);
                informationSchemaColumnData.TableName =
                    _baseDatabase.resolveNullString(reader.GetOrdinal(ColumnMapping.TABLE_NAME),
                    reader);
                informationSchemaColumnData.TableSchema =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.TABLE_SCHEMA),
                    reader);
                this.Add(informationSchemaColumnData);
            }
        }
    }
}
