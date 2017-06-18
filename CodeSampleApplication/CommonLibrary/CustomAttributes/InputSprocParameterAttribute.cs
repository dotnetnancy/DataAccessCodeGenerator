using System;
using System.Collections.Generic;
using System.Reflection;

namespace CommonLibrary.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class InputSprocParameterAttribute : Attribute
    {       
        public InputSprocParameterAttribute()
        {            
        }
    }
}
