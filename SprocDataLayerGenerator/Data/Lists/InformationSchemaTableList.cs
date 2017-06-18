using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

using SprocDataLayerGenerator.Data;
using CommonLibrary.Base.Database;
using ColumnMapping = SprocDataLayerGenerator.Column.Mapping.InformationSchemaTable;

namespace SprocDataLayerGenerator.List
{
    public class InformationSchemaTable : List<Data.InformationSchemaTable>
    {    
      //base database
        private BaseDatabase _baseDatabase = new BaseDatabase();

       /// <summary>
       /// empty constructor
       /// </summary>
        public InformationSchemaTable()
        {
        }

        /// <summary>
        /// constructor fills object with data from sqldatareader
        /// </summary>
        /// <param name="reader"></param>
        public InformationSchemaTable(SqlDataReader reader)
        {
            AddItemsToListBySqlDataReader(reader);
        }

        /// <summary>
        /// adds items to this list using a filled sql data reader
        /// </summary>
        /// <param name="reader"></param>
        public void AddItemsToListBySqlDataReader(SqlDataReader reader)
        {
            //we do not use the using block here because the BaseDatabase class uses the CommandBehavior.
            //CloseConnection syntax on the connection object, we use it on the datalayer instead.
            while (reader.Read())
            {
                Data.InformationSchemaTable informationSchemaTableData = new Data.InformationSchemaTable();

                informationSchemaTableData.TableCatalog =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.TABLE_CATALOG), reader);
                informationSchemaTableData.TableName =
                    _baseDatabase.resolveNullString(reader.GetOrdinal(ColumnMapping.TABLE_NAME), reader);
                informationSchemaTableData.TableSchema =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.TABLE_SCHEMA), reader);
                informationSchemaTableData.TableType =
                    _baseDatabase.resolveNullStringToNull(reader.GetOrdinal(ColumnMapping.TABLE_TYPE), reader);

                this.Add(informationSchemaTableData);
            }
        }
    }
}
