using System;
using System.Collections.Generic;
using System.Text;

namespace SprocDataLayerGenerator.Property.Mapping
{
    public static class InformationSchemaTableConstraint
    {
        public const string CONSTRAINT_CATALOG = "ConstraintCatalog";
        public const string CONSTRAINT_SCHEMA = "ConstraintSchema";
        public const string CONSTRAINT_NAME = "ConstraintName";
        public const string TABLE_CATALOG = "TableCatalog";
        public const string TABLE_SCHEMA = "TableSchema";
        public const string TABLE_NAME = "TableName";
        public const string CONSTRAINT_TYPE = "ConstraintType";
        public const string IS_DEFERRABLE = "IsDeferrable";
        public const string INITIALLY_DEFERRED = "InitiallyDeferred";
    }
}
