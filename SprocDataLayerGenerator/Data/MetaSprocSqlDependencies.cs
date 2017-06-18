using System;
using System.Collections.Generic;
using System.Text;

namespace SprocDataLayerGenerator.Data
{
   
    public class MetaSprocSqlDependency
    {
        private string _mainStoredProcedure = string.Empty;

        public string MainStoredProcedure
        {
            get { return _mainStoredProcedure; }
            set { _mainStoredProcedure = value; }
        }
        private List<MetaSqlDependency> _sprocDependencies = new List<MetaSqlDependency>();

        public List<MetaSqlDependency> SprocDependencies
        {
            get { return _sprocDependencies; }
            set { _sprocDependencies = value; }
        }
        private List<MetaSqlDependency> _tableDependencies = new List<MetaSqlDependency>();

        public List<MetaSqlDependency> TableDependencies
        {
            get { return _tableDependencies; }
            set { _tableDependencies = value; }
        }
        private Dictionary<string, List<MetaSqlDependency>> _tableDependencyToColumnsReferenced =
            new Dictionary<string, List<MetaSqlDependency>>();

        private Dictionary<string, MetaSprocSqlDependency> _recursiveSprocNameToMetaSprocSqlDependency =
            new Dictionary<string, MetaSprocSqlDependency>();

        public Dictionary<string, MetaSprocSqlDependency> RecursiveSprocNameToMetaSprocSqlDependency
        {
            get { return _recursiveSprocNameToMetaSprocSqlDependency; }
            set { _recursiveSprocNameToMetaSprocSqlDependency = value; }
        }

        public Dictionary<string, List<MetaSqlDependency>> TableDependencyToColumnsReferenced
        {
            get { return _tableDependencyToColumnsReferenced; }
            set { _tableDependencyToColumnsReferenced = value; }
        }
        private List<string> _distinctUserTablesList = new List<string>();

        public List<string> DistinctUserTablesList
        {
            get { return _distinctUserTablesList; }
            set { _distinctUserTablesList = value; }
        }
       

        public MetaSprocSqlDependency(string sprocName, 
                                      List.MetaSqlDependency sprocDependencies,
                                      List.MetaSqlDependency tableDependencies,
                                      Dictionary<string,
                                      List<MetaSqlDependency>> tableToColumnsReferenced,
                                      List<string> distinctUserTablesList
                                      )
        {
            _mainStoredProcedure = sprocName;
            _sprocDependencies = sprocDependencies;
            _tableDependencies = tableDependencies;
            _tableDependencyToColumnsReferenced = tableToColumnsReferenced;
            _distinctUserTablesList = distinctUserTablesList;            

        }

        

    }
}
