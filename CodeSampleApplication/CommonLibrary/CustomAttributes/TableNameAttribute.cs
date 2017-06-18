using System;
using System.Collections.Generic;
using System.Reflection;

namespace CommonLibrary.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableNameAttribute : Attribute
    {
        public string TableName;
        public TableNameAttribute(string tableName)
        {
            TableName = tableName;
        }
    }
}
