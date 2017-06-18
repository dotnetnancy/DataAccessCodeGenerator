using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLibrary.Constants
{
    public static class SqlConstants
    {
        public const string GET = "Get";
        public const string BY_PK = "ByPrimaryKey";
        public const string BY_CRITERIA_FUZZY = "ByCriteriaFuzzy";
        public const string BY_CRITERIA_EXACT = "ByCriteriaExact";

        public const string SPACE = " ";
        public const string COMMA_PLUS_SPACE = ", ";
        public const string EQUALS = "=";
        public const string LIKE = "Like";
        public const string SQL_OPEN_BRACKET = "(";
        public const string SQL_CLOSE_BRACKET = ")";
        public const string SINGLE_QUOTE = "'";
        public const string AT_SYMBOL = "@";

        public const string SELECT = "Select";
        public const string UPDATE = "Update";
        public const string INSERT = "Insert";
        public const string DELETE = "Delete";
        public const string INTO = "Into";
        public const string FROM = "From";
        public const string WHERE = "Where";
        public const string ORDERBY = "Order By";
        public const string VALUES = "Values";

        public const string AND = "And";
        public const string OR = "Or";
        public const string IF = "If";
        public const string EXISTS = "Exists";
        public const string NOT = "Not";
        public const string SET = "Set";
        public const string NULL = "null";

        public const string SCOPE_IDENTITY = @"SCOPE_IDENTITY";
        public const string IDENT_CURRENT = @"IDENT_CURRENT";

        public const string BASE_TABLE = @"BASE TABLE";
        public const string VIEW = @"VIEW";
    }
}
