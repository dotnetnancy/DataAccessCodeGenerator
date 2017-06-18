using System;
using System.Collections.Generic;
using System.Text;

namespace SprocDataLayerGenerator.Property.Mapping
{
    public static class InformationSchemaTable
    {
        //the const variable is the actual Field in the Database, then we map the actual property in 
        //the data class (dto)
        public const string TABLE_CATALOG = "TableCatalog";
        public const string TABLE_NAME = "TableName";
        public const string TABLE_SCHEMA = "TableSchema";
        public const string TABLE_TYPE = "TableType";
    }

}
