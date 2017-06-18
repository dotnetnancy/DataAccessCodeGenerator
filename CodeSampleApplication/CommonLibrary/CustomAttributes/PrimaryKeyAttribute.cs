using System;
using System.Reflection;

namespace CommonLibrary.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKey : Attribute
    {
        public PrimaryKey()
        {
        }
    }
}
