using System;
using System.Collections.Generic;
using System.Text;

namespace SprocDataLayerGenerator.Column.Mapping
{
    public static class MetaSqlDependency
    {
        public const string REFERENCED_OBJECT = "ReferencedObject";
        public const string REFERENCED_TYPE = "ReferencedType";
        public const string REFERENCING_OBJECT = "ReferencingObject";
        public const string CLASS = "class";
        public const string CLASS_DESC = "class_desc";
        public const string OBJECT_ID = "object_id";
        public const string COLUMN_ID = "column_id";
        public const string REFERENCED_MAJOR_ID = "referenced_major_id";
        public const string REFERENCED_MINOR_ID = "referenced_minor_id";
        public const string IS_SELECTED = "is_selected";
        public const string IS_UPDATED = "is_updated";
        public const string IS_SELECT_ALL = "is_select_all";
    }
    public static class InformationSchemaTable
    {
        public const string TABLE_CATALOG = "TABLE_CATALOG";
        public const string TABLE_SCHEMA = "TABLE_SCHEMA";
        public const string TABLE_NAME = "TABLE_NAME";
        public const string TABLE_TYPE = "TABLE_TYPE";
    }
}
