using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CSharp;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Server;


using Microsoft.SqlServer.Server;

namespace CommonLibrary.Predicates
{
    public class BaseDatabasePredicateFunctions
    {
        private string _storedProcedureNameHolder;

        public string StoredProcedureNameHolder
        {
            get { return _storedProcedureNameHolder; }
            set { _storedProcedureNameHolder = value; }
        }

        public bool FindSprocBySprocName(StoredProcedure sproc)
        {
            bool found = false;
            if (sproc.Name == StoredProcedureNameHolder)
            {
                found = true;
            }
            return found;
        }

    }
}
