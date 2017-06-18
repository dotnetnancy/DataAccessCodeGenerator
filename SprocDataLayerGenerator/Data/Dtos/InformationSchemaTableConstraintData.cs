using System;
using System.Collections.Generic;
using System.Text;



namespace SprocDataLayerGenerator.Data
{
    public class InformationSchemaTableConstraint
    {
        private string _constraintCatalog = string.Empty;      
        private string _constraintSchema = string.Empty;      
        private string _constraintName = string.Empty;       
        private string _tableCatalog = string.Empty;       
        private string _tableSchema = string.Empty;      
        private string _tableName = string.Empty;       
        private string _constraintType = string.Empty;      
        private string _isDeferrable = string.Empty;
        private string _initiallyDeferred = string.Empty;

        public string IsDeferrable
        {
            get { return _isDeferrable; }
            set { _isDeferrable = value; }
        }       

        public string ConstraintType
        {
            get { return _constraintType; }
            set { _constraintType = value; }
        }

        public string TableName
        {
            get { return _tableName; }
            set { _tableName = value; }
        }

        public string TableSchema
        {
            get { return _tableSchema; }
            set { _tableSchema = value; }
        }

        public string TableCatalog
        {
            get { return _tableCatalog; }
            set { _tableCatalog = value; }
        }

        public string ConstraintName
        {
            get { return _constraintName; }
            set { _constraintName = value; }
        }

        public string ConstraintSchema
        {
            get { return _constraintSchema; }
            set { _constraintSchema = value; }
        }

        public string ConstraintCatalog
        {
            get { return _constraintCatalog; }
            set { _constraintCatalog = value; }
        }

        public string InitiallyDeferred
        {
            get { return _initiallyDeferred; }
            set { _initiallyDeferred = value; }
        }
    }
}
