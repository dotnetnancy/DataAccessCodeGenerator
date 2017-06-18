using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

using SprocDataLayerGenerator.Data;
using CommonLibrary.Base.Database;
using ColumnMapping = SprocDataLayerGenerator.Column.Mapping.MetaSqlDependency;

namespace SprocDataLayerGenerator.List
{
    public class MetaSqlDependency : List<Data.MetaSqlDependency>
    {
        //base database
        private BaseDatabase _baseDatabase = new BaseDatabase();

        /// <summary>
        /// empty constructor
        /// </summary>
        public MetaSqlDependency()
        {
        }

        /// <summary>
        /// constructor fills object with data from sqldatareader
        /// </summary>
        /// <param name="reader"></param>
        public MetaSqlDependency(SqlDataReader reader)
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
                Data.MetaSqlDependency metaSqlDependencyData = new Data.MetaSqlDependency();

                metaSqlDependencyData.Class =
                    _baseDatabase.resolveNullString(reader.GetOrdinal(ColumnMapping.CLASS), reader);
                metaSqlDependencyData.ClassDesc =
                    _baseDatabase.resolveNullString(reader.GetOrdinal(ColumnMapping.CLASS_DESC), reader);
                metaSqlDependencyData.ColumnId =
                    _baseDatabase.resolveNullInt32(reader.GetOrdinal(ColumnMapping.COLUMN_ID), reader);
                metaSqlDependencyData.IsSelectAll =
                    _baseDatabase.resolveNullBoolean(reader.GetOrdinal(ColumnMapping.IS_SELECT_ALL), reader);
                metaSqlDependencyData.IsSelected =
                    _baseDatabase.resolveNullBoolean(reader.GetOrdinal(ColumnMapping.IS_SELECTED), reader);
                metaSqlDependencyData.IsUpdated =
                    _baseDatabase.resolveNullBoolean(reader.GetOrdinal(ColumnMapping.IS_UPDATED), reader);
                metaSqlDependencyData.ObjectId =
                    _baseDatabase.resolveNullInt32(reader.GetOrdinal(ColumnMapping.OBJECT_ID), reader);
                metaSqlDependencyData.ReferencedMajorId =
                    _baseDatabase.resolveNullInt32(reader.GetOrdinal(ColumnMapping.REFERENCED_MAJOR_ID), reader);
                metaSqlDependencyData.ReferencedMinorId =
                    _baseDatabase.resolveNullInt32(reader.GetOrdinal(ColumnMapping.REFERENCED_MINOR_ID), reader);
                metaSqlDependencyData.ReferencedObject =
                    _baseDatabase.resolveNullString(reader.GetOrdinal(ColumnMapping.REFERENCED_OBJECT), reader);
                metaSqlDependencyData.ReferencedType =
                    _baseDatabase.resolveNullString(reader.GetOrdinal(ColumnMapping.REFERENCED_TYPE), reader);
                metaSqlDependencyData.ReferencingObject =
                    _baseDatabase.resolveNullString(reader.GetOrdinal(ColumnMapping.REFERENCING_OBJECT), reader);


                this.Add(metaSqlDependencyData);
            }
        }
    }
}
