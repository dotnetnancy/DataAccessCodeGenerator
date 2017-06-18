using System;
using System.Collections.Generic;
using System.Reflection;

namespace CommonLibrary.CustomAttributes
{
    //[AttributeUsage(AttributeTargets.Property)]
    //need to fix this we really do not want more than one databasecolumnattribute
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DatabaseColumnAttribute : Attribute
    {
        public string DatabaseColumn;
        public DatabaseColumnAttribute(string databaseColumn)
        {
            DatabaseColumn = databaseColumn;
        }
    }
}
