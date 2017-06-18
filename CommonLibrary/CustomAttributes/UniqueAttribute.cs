using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLibrary.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class Unique : Attribute
    {
    }
}
