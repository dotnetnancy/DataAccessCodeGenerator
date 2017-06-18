using System;
using System.Collections.Generic;
using System.Reflection;

namespace CommonLibrary.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Property,AllowMultiple=true)]
    public class SelectAttribute : Attribute
    {
        public string SprocName;
        public SelectAttribute(string sprocName)
        {
            SprocName = sprocName;
        }
    }
}
