using System;
using System.Collections.Generic;
using System.Text;

namespace SprocDataLayerGenerator.Data
{
    public class InformationSchemaColumn
    {
        private string _tableCatalog = string.Empty;
        private string _tableSchema = string.Empty;
        private string _tableName = string.Empty;
        private string _columnName = string.Empty;
        private int? _ordinalPosition = null;
        private string _columnDefault = string.Empty;
        private string _isNullable = string.Empty;
        private string _dataType = string.Empty;
        private int? _characterMaximumLength = null;
        private int? _characterOctetLength = null;
        private int? _numericPrecision = null;
        private int? _numericPrecisionRadix = null;
        private int? _numericScale = null;
        private int? _datetimePrecision = null;
        private string _characterSetCatalog = string.Empty;
        private string _characterSetSchema = string.Empty;
        private string _characterSetName = string.Empty;
        private string _collationCatalog = string.Empty;
        private string _collationSchema = string.Empty;
        private string _collationName = string.Empty;
        private string _domainCatalog = string.Empty;
        private string _domainSchema = string.Empty;
        private string _domainName = string.Empty;

        public string DomainName
        {
            get { return _domainName; }
            set { _domainName = value; }
        }

        public string DomainSchema
        {
            get { return _domainSchema; }
            set { _domainSchema = value; }
        }

        public string DomainCatalog
        {
            get { return _domainCatalog; }
            set { _domainCatalog = value; }
        }

        public string CollationName
        {
            get { return _collationName; }
            set { _collationName = value; }
        }

        public string CollationSchema
        {
            get { return _collationSchema; }
            set { _collationSchema = value; }
        }

        public string CollationCatalog
        {
            get { return _collationCatalog; }
            set { _collationCatalog = value; }
        }
        
        public string CharacterSetName
        {
            get { return _characterSetName; }
            set { _characterSetName = value; }
        }

        public string CharacterSetSchema
        {
            get { return _characterSetSchema; }
            set { _characterSetSchema = value; }
        }

        public string CharacterSetCatalog
        {
            get { return _characterSetCatalog; }
            set { _characterSetCatalog = value; }
        }


        public int? DatetimePrecision
        {
            get { return _datetimePrecision; }
            set { _datetimePrecision = value; }
        }


        public int? NumericScale
        {
            get { return _numericScale; }
            set { _numericScale = value; }
        }

        public int? NumericPrecisionRadix
        {
            get { return _numericPrecisionRadix; }
            set { _numericPrecisionRadix = value; }
        }


        public int? NumericPrecision
        {
            get { return _numericPrecision; }
            set { _numericPrecision = value; }
        }

        public int? CharacterOctetLength
        {
            get { return _characterOctetLength; }
            set { _characterOctetLength = value; }
        }

        public int? CharacterMaximumLength
        {
            get { return _characterMaximumLength; }
            set { _characterMaximumLength = value; }
        }

        public string DataType
        {
            get { return _dataType; }
            set { _dataType = value; }
        }

        public string IsNullable
        {
            get { return _isNullable; }
            set { _isNullable = value; }
        }

        public string ColumnDefault
        {
            get { return _columnDefault; }
            set { _columnDefault = value; }
        }

        public int? OrdinalPosition
        {
            get { return _ordinalPosition; }
            set { _ordinalPosition = value; }
        }


        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
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

    }
}
