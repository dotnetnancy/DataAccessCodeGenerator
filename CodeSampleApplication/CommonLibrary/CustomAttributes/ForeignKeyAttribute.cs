using System;
using System.Collections.Generic;
using System.Reflection;

namespace CommonLibrary.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKey : Attribute
    {
        Dictionary<string, string> TableToPrimaryKeyColumnDictionary;

        public ForeignKey(Dictionary<string, string> tableToPrimaryKeyColumnDictionary)
        {
            TableToPrimaryKeyColumnDictionary = tableToPrimaryKeyColumnDictionary;
        }
    }
}
