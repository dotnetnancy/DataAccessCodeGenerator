using System;
using System.Collections.Generic;
using System.Reflection;

namespace CommonLibrary.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ResolveNullValueAttribute : Attribute
    {
        public string TypeName;
        public string Value;
        
        public ResolveNullValueAttribute(string typeName, string value)
        {
            TypeName = typeName;
            Value = value;
        }
    }
}