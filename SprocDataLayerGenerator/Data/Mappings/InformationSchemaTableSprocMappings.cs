using System;
using System.Collections.Generic;
using System.Text;

namespace SprocDataLayerGenerator.Sproc.Mapping
{
    public static class InformationSchemaHelper
    {
        public const string IS_IDENTITY = "IsIdentity";
    }
    public static class InformationSchemaTable
    {
        public const string INFORMATION_SCHEMA_TABLE = "GetInformationSchemaTables";
    }

    public static class InformationSchemaTableConstraint
    {
        public const string INFORMATION_SCHEMA_TABLE_CONSTRAINTS = "GetInformationSchemaTableConstraints";
    }

    public static class InformationSchemaColumn
    {
        public const string INFORMATION_SCHEMA_COLUMN = "GetInformationSchemaColumns";
    }

    public static class All
    {
        public const string INFORMATION_SCHEMA = "GetInformationSchema";
    }

    public static class AllDependencies
    {
        public const string GET_DEPENDENCIES = "GetDependencies";
    }

    public static class InformationSchemaConstraintColumnUsage
    {
        public const string INFORMATION_SCHEMA_CONSTRAINT_COLUMN_USAGE = "GetInformationSchemaColumnUsage";
    }

    public static class GetDependency
    {
        public const string GET_DEPENDENCY = "GetDependency";
    }

    public enum AllResultSets
    {
        INFORMATION_SCHEMA_TABLE = 0,
        INFORMATION_SCHEMA_TABLE_CONSTRAINTS = 1,
        INFORMATION_SCHEMA_COLUMN = 2,
        INFORMATION_SCHEMA_CONSTRAINT_COLUMN_USAGE = 3
    }
    public enum DependencyResultSets
    {
        GET_DEPENDENCY = 0
    }

    public static class FilterParameter
    {
        public const string TABLE_NAME_PARAM = "@TableName";
        public const string COLUMN_NAME_PARAM = "@ColumnName";
        public const string SPROC_NAME_PARAM = "@SprocName";
    }

    public static class ReturnParameter
    {
        public const string IS_IDENTITY = "@IsIdentity";
    }

    
}
