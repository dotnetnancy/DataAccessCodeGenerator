using System;
using System.Collections.Generic;
using System.Text;
using ClassCreationHelperConstants = CommonLibrary.Constants.ClassCreationConstants;

namespace CommonLibrary.Utility
{
    public static class ClassCreationHelperMethods
    {
        public static string GetDataLayerMethodName(string sprocName)
        {
            string dataLayerMethodName = sprocName;
            return dataLayerMethodName;
               
        }
        public static string GetListClassName(string tableName)
        {
            string upperCaseStartLetter = ((String)tableName)[0].ToString().ToUpper();
            string firstLetterRemoved = tableName.Remove(0, 1);
            string upperCaseStartLetterName = upperCaseStartLetter + firstLetterRemoved;
            string listClassName = upperCaseStartLetterName;
            return listClassName;
        }

        public static string GetDataAccessClassName()
        {
            return ClassCreationHelperConstants.CUSTOM_SPROC;
        }

        public static string GetBoListFileName(string tableName)
        {
            string boListFileName = tableName +
                                    ClassCreationHelperConstants.BUSINESS +
                                    ClassCreationHelperConstants.DOT_OPERATOR +
                                    ClassCreationHelperConstants.LIST +
                                    ClassCreationHelperConstants.DOT_OPERATOR +
                                    ClassCreationHelperConstants.CS;
            return boListFileName;

        }

        public static string GetBoFileName(string tableName)
        {
            string boFileName = tableName +
                                ClassCreationHelperConstants.BUSINESS +
                                ClassCreationHelperConstants.DOT_OPERATOR +
                                ClassCreationHelperConstants.CS;
            return boFileName;
        }

        public static string GetDtoFileName(string tableName)
        {
            string dtoFileName = tableName +
                ClassCreationHelperConstants.DATA +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.CS;
            return dtoFileName;
        }

        public static string GetDtoForCustomSprocFileName(string tableName)
        {
            string dtoFileName = tableName +
                ClassCreationHelperConstants.DATA +
                ClassCreationHelperConstants.SPROC_TABLE +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.CS;
            return dtoFileName;
        }

        public static string GetInputDtoForCustomSprocFileName(string tableName)
        {
            string dtoFileName = tableName +
                ClassCreationHelperConstants.INPUT +
                ClassCreationHelperConstants.DATA +
                ClassCreationHelperConstants.SPROC_TABLE +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.CS;
            return dtoFileName;
        }

        public static string GetResultListForCustomSprocFileName(string sprocName)
        {
            string resultFileName = sprocName +
                ClassCreationHelperConstants.RESULT + 
                ClassCreationHelperConstants.LIST + 
                ClassCreationHelperConstants.SPROC_TABLE +
                ClassCreationHelperConstants.DOT_OPERATOR +                
                ClassCreationHelperConstants.CS;
            return resultFileName;
        }

        public static string GetListFileName(string tableName)
        {
            string listFileName = tableName + ClassCreationHelperConstants.LIST + ClassCreationHelperConstants.DOT_OPERATOR + ClassCreationHelperConstants.CS;
            return listFileName;
        }

        public static string GetDataAccessClassNamespace(string enclosingApplicationNamespace)
        {
            string dataAccessClassNamespace = enclosingApplicationNamespace +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.DATA +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.ACCESSOR;
            return dataAccessClassNamespace;

        }

        public static string GetBoListNamespace(string enclosingApplicationNamespace)
        {
            string boNamespace = enclosingApplicationNamespace +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.BUSINESS +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.SINGLE_TABLE +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.BO +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.LIST;
                    
            return boNamespace;
        }

        public static string GetBoNamespace(string enclosingApplicationNamespace)
        {
            string boNamespace = enclosingApplicationNamespace +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.BUSINESS +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.SINGLE_TABLE +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.BO;
            return boNamespace;
        }

        public static string GetDtoNamespace(string enclosingApplicationNamespace)
        {
            string dtoNamespace = enclosingApplicationNamespace +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.DATA +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.SINGLE_TABLE + 
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.DTO;
            return dtoNamespace;
        }

        public static string GetDtoForCustomSprocNamespace(string enclosingApplicationNamespace)
        {
            string dtoNamespace = enclosingApplicationNamespace +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.DATA +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.SPROC_TABLE +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.DTO;
            return dtoNamespace;
        }

        public static string GetInputDtoForCustomSprocNamespace(string enclosingApplicationNamespace)
        {
            string dtoNamespace = enclosingApplicationNamespace +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.DATA +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.SPROC_TABLE +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.INPUT + 
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.DTO;
            return dtoNamespace;
        }

        public static string GetResultListForCustomSprocNamespace(string enclosingApplicationNamespace)
        {
            string resultNamespace = enclosingApplicationNamespace +
                ClassCreationHelperConstants.DOT_OPERATOR +                
                ClassCreationHelperConstants.SPROC_TABLE +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.RESULT + 
                ClassCreationHelperConstants.DOT_OPERATOR + 
                ClassCreationHelperConstants.LIST;
            return resultNamespace;
        }

        public static string GetListNamespace(string enclosingApplicationNamespace)
        {
            string listNamespace = enclosingApplicationNamespace +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.SINGLE_TABLE + 
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.LIST;
            return listNamespace;
        }

        public static string GetDtoClassName(string tableName)
        {
            string upperCaseStartLetter = ((String)tableName)[0].ToString().ToUpper();
            string firstLetterRemoved = tableName.Remove(0, 1);
            string upperCaseStartLetterName = upperCaseStartLetter + firstLetterRemoved;
            string dtoClassName = upperCaseStartLetterName;
            return dtoClassName;
        }

        public static string GetBoClassName(string tableName)
        {
            string upperCaseStartLetter = ((String)tableName)[0].ToString().ToUpper();
            string firstLetterRemoved = tableName.Remove(0, 1);
            string upperCaseStartLetterName = upperCaseStartLetter + firstLetterRemoved;
            string boClassName = upperCaseStartLetterName;
            return boClassName;
        }

        public static string GetSprocListClassName(string sprocName)
        {
            string upperCaseStartLetter = ((String)sprocName)[0].ToString().ToUpper();
            string firstLetterRemoved = sprocName.Remove(0, 1);
            string upperCaseStartLetterName = upperCaseStartLetter + firstLetterRemoved;
            string resultClassName = upperCaseStartLetterName;
            return resultClassName;
        }
    }
}
