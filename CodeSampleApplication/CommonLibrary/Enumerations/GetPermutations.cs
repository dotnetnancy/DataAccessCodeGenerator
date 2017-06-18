using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLibrary.Enumerations
{
    public enum GetPermutations
    {
        ByPrimaryKey = 0,
        ByFuzzyCriteria = 1,
        ByExplicitCriteria = 2,
        AllByColumnMappings = 3
    }
}
