using System;
using System.Collections.Generic;
using System.Text;

namespace SprocDataLayerGenerator.Data
{
   public class InformationSchemaTable
    {
        private string _tableCatalog = string.Empty;       
        private string _tableSchema = string.Empty;       
        private string _tableName = string.Empty;      
        private string _tableType = string.Empty;

       public string TableCatalog
       {
           get { return _tableCatalog; }
           set { _tableCatalog = value; }
       }

       public string TableSchema
       {
           get { return _tableSchema; }
           set { _tableSchema = value; }
       }

       public string TableName
       {
           get { return _tableName; }
           set { _tableName = value; }
       }
        public string TableType
        {
            get { return _tableType; }
            set { _tableType = value; }
        }



    }
}
