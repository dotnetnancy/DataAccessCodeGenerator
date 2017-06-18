using System;
using System.Collections.Generic;
using System.Reflection;

namespace CommonLibrary.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Property,AllowMultiple=true)]
    public class UpdateAttribute : Attribute
    {
        public string SprocName;
        public UpdateAttribute(string sprocName)
        {
            SprocName = sprocName;
        }
    }
}
