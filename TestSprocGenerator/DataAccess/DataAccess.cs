using System;
using System.Collections.Generic;
using System.Text;

namespace TestSprocGenerator
{
    public class DataAccess<T> : CommonLibrary.Base.Database.BaseDataAccess<T>
    {
      
        public DataAccess(CommonLibrary.DatabaseSmoObjectsAndSettings settings) : base(settings)
        {
        }

        public DataAccess(string databaseName,
                          string dataSource,
                          string initialCatalog,
                          string userId,
                          string password,
                          bool trustedConnection)
            : base(databaseName, dataSource, initialCatalog, userId, password, trustedConnection)
        {
        }

        public DataAccess(string databaseName,
                          string dataSource,
                          string initialCatalog,
                          string userId,
                          string password,
                          bool trustedConnection,
                          string schema)
            : base(databaseName, dataSource, initialCatalog, userId, password, trustedConnection,schema)
        {
        }


    }
}
