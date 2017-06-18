using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Drawing;
using System.Reflection;

using Microsoft.CSharp;
using CommonLibrary;

using ClassCreationHelperConstants = CommonLibrary.Constants.ClassCreationConstants;

using ClassCreationHelperMethods = CommonLibrary.Utility.ClassCreationHelperMethods;

namespace BusinessLayerGenerator.BusinessObjects
{
    public class BusinessLayerGeneration
    {

        public event ResolveEventHandler ReflectionOnlyAssemblyResolve;
        
        private const string OUTPUT_PATH_BO = @"..\..\GeneratedBos\";
        private const string OUTPUT_PATH_BO_LIST = @"..\..\GeneratedBoLists\";

        private List<object> _assembliesGeneratedInMemory =
           new List<object>();

        CommonLibrary.DatabaseSmoObjectsAndSettings _databaseSmoObjectsAndSettings = null;
        string _enclosingApplicationNamespace = string.Empty;

        public BusinessLayerGeneration(CommonLibrary.DatabaseSmoObjectsAndSettings databaseSmoObjectsAndSettings,
                                      string enclosingApplicationNamespace)
        {
            _databaseSmoObjectsAndSettings = databaseSmoObjectsAndSettings;
            _enclosingApplicationNamespace = enclosingApplicationNamespace;
            this.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(BusinessLayerGeneration_ReflectionOnlyAssemblyResolve);

        }

        public Assembly BusinessLayerGeneration_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {

            Assembly returnAssembly = Assembly.Load(args.Name);
            //Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();

            //AssemblyName[] referencedAssemblies = asm.GetReferencedAssemblies();

            //foreach (AssemblyName assemblyName in referencedAssemblies)
            //{
            //    Assembly.Load(assemblyName);
            //}  
            return returnAssembly;

        }

        public void TestTableGenerateBusinessLayer(bool overwriteExisting)
        {
            Assembly asm = Assembly.ReflectionOnlyLoad(_enclosingApplicationNamespace);
            Assembly.ReflectionOnlyLoad("CommonLibrary");
            Assembly.ReflectionOnlyLoadFrom(@"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\System.dll");
            Assembly.ReflectionOnlyLoadFrom(@"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\System.Windows.Forms.dll");
            Assembly.ReflectionOnlyLoadFrom(@"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\System.Drawing.dll");

            Type[] typesInAssembly = null;

            typesInAssembly = asm.GetTypes();

            string singleTableDtoNamespace = ClassCreationHelperMethods.GetDtoNamespace(_enclosingApplicationNamespace);
            string customSprocDtoNamespace = ClassCreationHelperMethods.GetDtoForCustomSprocNamespace(_enclosingApplicationNamespace);

            foreach (Type type in typesInAssembly)
            {
                string typeNamespace = type.Namespace;

                if (typeNamespace == singleTableDtoNamespace)
                {
                    if (type.FullName == "TestSprocGenerator.Data.SingleTable.Dto.TestTable")
                    {
                        GenerateSingleTableBusinessClass(type, overwriteExisting);
                        GenerateSingleTableBusinessClassList(type, overwriteExisting);
                    }
                }
                else
                    if (typeNamespace == customSprocDtoNamespace)
                    {
                        //process custom sproc dto business classes
                    }

            }
        }


        public void GenerateBusinessLayer(bool overwriteExisting)
        {
            Assembly asm = Assembly.ReflectionOnlyLoad(_enclosingApplicationNamespace);
            Assembly.ReflectionOnlyLoad("CommonLibrary");
            Assembly.ReflectionOnlyLoadFrom(@"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\System.dll");
            Assembly.ReflectionOnlyLoadFrom(@"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\System.Windows.Forms.dll");
            Assembly.ReflectionOnlyLoadFrom(@"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\System.Drawing.dll");

            Type[] typesInAssembly = null;

            typesInAssembly = asm.GetTypes();
            
            string singleTableDtoNamespace = ClassCreationHelperMethods.GetDtoNamespace(_enclosingApplicationNamespace);
            string customSprocDtoNamespace = ClassCreationHelperMethods.GetDtoForCustomSprocNamespace(_enclosingApplicationNamespace);
            
            foreach (Type type in typesInAssembly)
            {
                string typeNamespace = type.Namespace;

                if (typeNamespace == singleTableDtoNamespace)
                {
                    GenerateSingleTableBusinessClass(type,overwriteExisting);
                    GenerateSingleTableBusinessClassList(type, overwriteExisting);
                }
                else
                    if (typeNamespace == customSprocDtoNamespace)
                    {
                        //process custom sproc dto business classes
                    }

            }

        }

        /// <summary>
        /// TODO: implement this as a single table bo list class as i just copied the GenerateSingleTableBusinessClass method
        /// </summary>
        /// <param name="typeOfDto"></param>
        /// <param name="overwriteExisting"></param>
        public void GenerateSingleTableBusinessClassList(Type typeOfDto, bool overwriteExisting)
        {
            PropertyInfo[] properties = typeOfDto.GetProperties();

            Dictionary<string, PropertyInfo> databaseColumnToPrimaryKeyProperty =
                new Dictionary<string, PropertyInfo>();

            Dictionary<string, PropertyInfo> databaseColumnToNonPrimaryKeyProperty =
                new Dictionary<string, PropertyInfo>();

            foreach (PropertyInfo property in properties)
            {
                IList<CustomAttributeData> customAttributes
                    = CustomAttributeData.GetCustomAttributes(property);

                string databaseColumn = string.Empty;
                bool primaryKey = false;

                foreach (CustomAttributeData attribute in customAttributes)
                {
                    if (attribute.Constructor.ReflectedType.UnderlyingSystemType.FullName
                        == typeof(CommonLibrary.CustomAttributes.DatabaseColumnAttribute).FullName)
                    {
                        foreach (CustomAttributeTypedArgument typedArgument in attribute.ConstructorArguments)
                        {
                            databaseColumn = typedArgument.Value.ToString();
                        }
                    }

                    if (attribute.Constructor.ReflectedType.UnderlyingSystemType.FullName ==
                        typeof(CommonLibrary.CustomAttributes.PrimaryKey).FullName)
                    {
                        primaryKey = true;
                    }
                }

                if (primaryKey)
                {
                    databaseColumnToPrimaryKeyProperty.Add(databaseColumn, property);
                }
                else
                {
                    databaseColumnToNonPrimaryKeyProperty.Add(databaseColumn, property);
                }
            }

            GenerateSingleTableBusinessClassList(databaseColumnToPrimaryKeyProperty,
                databaseColumnToNonPrimaryKeyProperty, typeOfDto, overwriteExisting);
        }

        public void GenerateSingleTableBusinessClass(Type typeOfDto, bool overwriteExisting)
        {
            PropertyInfo[] properties = typeOfDto.GetProperties();

            Dictionary<string, PropertyInfo> databaseColumnToPrimaryKeyProperty =
                new Dictionary<string, PropertyInfo>();

            Dictionary<string, PropertyInfo> databaseColumnToNonPrimaryKeyProperty =
                new Dictionary<string, PropertyInfo>();

            foreach (PropertyInfo property in properties)
            {
                IList<CustomAttributeData> customAttributes
                    = CustomAttributeData.GetCustomAttributes(property);

                string databaseColumn = string.Empty;               
                bool primaryKey = false;

                foreach (CustomAttributeData attribute in customAttributes)
                {
                    if(attribute.Constructor.ReflectedType.UnderlyingSystemType.FullName 
                        == typeof(CommonLibrary.CustomAttributes.DatabaseColumnAttribute).FullName)
                    {
                        foreach (CustomAttributeTypedArgument typedArgument in attribute.ConstructorArguments)
                        {
                            databaseColumn = typedArgument.Value.ToString();
                        }
                    }

                    if(attribute.Constructor.ReflectedType.UnderlyingSystemType.FullName == 
                        typeof(CommonLibrary.CustomAttributes.PrimaryKey).FullName)
                    {
                        primaryKey = true;
                    }                   
                }

                if(primaryKey)
                {
                    databaseColumnToPrimaryKeyProperty.Add(databaseColumn,property);
                }
                else
                {
                    databaseColumnToNonPrimaryKeyProperty.Add(databaseColumn,property);
                }
            }

            GenerateSingleTableBusinessClass(databaseColumnToPrimaryKeyProperty,
                databaseColumnToNonPrimaryKeyProperty,typeOfDto,overwriteExisting);

        }

        /// <summary>
        /// TODO: implement this for a single table bo list class, i just copied the GenerateSingleTableBusinessClass
        /// </summary>
        /// <param name="databaseColumnToPrimaryKeyProperty"></param>
        /// <param name="databaseColumnToNonPrimaryKeyProperty"></param>
        /// <param name="typeOfDto"></param>
        /// <param name="overwriteExisting"></param>
        public void GenerateSingleTableBusinessClassList(Dictionary<string, PropertyInfo> databaseColumnToPrimaryKeyProperty,
                                               Dictionary<string, PropertyInfo> databaseColumnToNonPrimaryKeyProperty,
                                                Type typeOfDto,
                                                bool overwriteExisting)
        {

            string tableName = string.Empty;
            string getByPrimaryKeySprocName = string.Empty;
            string getAllSprocName = string.Empty;
            string getByCriteriaFuzzySprocName = string.Empty;
            string getByCriteriaExactSprocName = string.Empty;
            
            IList<CustomAttributeData> customAttributes =
                CustomAttributeData.GetCustomAttributes(typeOfDto);
            foreach (CustomAttributeData customAttributeData in customAttributes)
            {
                if (customAttributeData.Constructor.ReflectedType.UnderlyingSystemType.FullName ==
                     typeof(CommonLibrary.CustomAttributes.TableNameAttribute).FullName)
                {
                    foreach (CustomAttributeTypedArgument typedArg in customAttributeData.ConstructorArguments)
                    {
                        tableName = typedArg.Value.ToString();

                        getByPrimaryKeySprocName =
                            CommonLibrary.Utility.DatabaseHelperMethods.GenerateGetByPrimaryKeySprocName(tableName);
                        getAllSprocName =
                            CommonLibrary.Utility.DatabaseHelperMethods.GenerateGetAllSprocName(tableName);
                        getByCriteriaFuzzySprocName =
                            CommonLibrary.Utility.DatabaseHelperMethods.GenerateGetByCriteriaFuzzySprocName(tableName);
                        getByCriteriaExactSprocName =
                            CommonLibrary.Utility.DatabaseHelperMethods.GenerateGetByCriteriaExactSprocName(tableName);


                    }
                }
            }

            GenerateSingleTableBOList(tableName,
                                  getByPrimaryKeySprocName,
                                  getAllSprocName,
                                  getByCriteriaFuzzySprocName,
                                  getByCriteriaExactSprocName,
                                  databaseColumnToPrimaryKeyProperty,
                                  databaseColumnToNonPrimaryKeyProperty,
                                  overwriteExisting,
                                  typeOfDto);

        }

        public void GenerateSingleTableBusinessClass(Dictionary<string, PropertyInfo> databaseColumnToPrimaryKeyProperty,
                                                       Dictionary<string, PropertyInfo> databaseColumnToNonPrimaryKeyProperty,
                                                        Type typeOfDto,
                                                        bool overwriteExisting)
        {

            string tableName = string.Empty;
            string getByPrimaryKeySprocName = string.Empty;
            string getAllSprocName = string.Empty;
            string insertSprocName = string.Empty;
            string updateSprocName = string.Empty;
            string deleteSprocName = string.Empty;

            IList<CustomAttributeData> customAttributes = 
                CustomAttributeData.GetCustomAttributes(typeOfDto);
                foreach(CustomAttributeData customAttributeData in customAttributes)
                {
                   if(customAttributeData.Constructor.ReflectedType.UnderlyingSystemType.FullName ==
                        typeof(CommonLibrary.CustomAttributes.TableNameAttribute).FullName)
                   {
                       foreach(CustomAttributeTypedArgument typedArg in customAttributeData.ConstructorArguments)
                       {
                           tableName = typedArg.Value.ToString();

                           getByPrimaryKeySprocName =
                               CommonLibrary.Utility.DatabaseHelperMethods.GenerateGetByPrimaryKeySprocName(tableName);
                           getAllSprocName = 
                               CommonLibrary.Utility.DatabaseHelperMethods.GenerateGetAllSprocName(tableName);
                           insertSprocName =
                               CommonLibrary.Utility.DatabaseHelperMethods.GenerateInsertSprocName(tableName);
                           updateSprocName =
                               CommonLibrary.Utility.DatabaseHelperMethods.GenerateUpdateByPrimaryKeySprocName(tableName);
                           deleteSprocName =
                               CommonLibrary.Utility.DatabaseHelperMethods.GenerateDeleteByPrimaryKeySprocName(tableName);


                       }
                   }
                }

                GenerateSingleTableBO(tableName,
                                      getByPrimaryKeySprocName,
                                      getAllSprocName,
                                      insertSprocName,
                                      updateSprocName,
                                      deleteSprocName,
                                      databaseColumnToPrimaryKeyProperty,
                                      databaseColumnToNonPrimaryKeyProperty,
                                      overwriteExisting,
                                      typeOfDto);

        }

        public void GenerateSingleTableBO(string tableName,
                                          string getByPrimaryKeySprocName,
                                          string getAllSprocName,
                                          string insertSprocName,
                                          string updateSprocName,
                                          string deleteSprocName,
                                          Dictionary<string, PropertyInfo> databaseColumnToPrimaryKeyProperty,
                                          Dictionary<string, PropertyInfo> databaseColumnToNonPrimaryKeyProperty,
                                          bool overwriteExisting,
                                          Type typeOfDto)
        {
            string outputFilePath = OUTPUT_PATH_BO;
            string outputFileName = ClassCreationHelperMethods.GetBoFileName(tableName);
            string boNamespace = ClassCreationHelperMethods.GetBoNamespace(_enclosingApplicationNamespace);
            string boClassName = ClassCreationHelperMethods.GetBoClassName(tableName);
            List<string> boNamespaces = GetBoNamespaceList();
            string outputFileAndPath = outputFilePath + outputFileName;

            GenerateSingleTableBO(boNamespace,
                                  boNamespaces,
                                  boClassName,
                                  outputFileAndPath,
                                  overwriteExisting,
                                  tableName,
                                  getByPrimaryKeySprocName,
                                  getAllSprocName,
                                  insertSprocName,
                                  updateSprocName,
                                  deleteSprocName,
                                  databaseColumnToPrimaryKeyProperty,
                                  databaseColumnToNonPrimaryKeyProperty,
                                  typeOfDto);
                                  


        }

        /// <summary>
        /// TODO:  implement this as a bo list class i just copied the GenerateSingleTableBO method
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="getByPrimaryKeySprocName"></param>
        /// <param name="getAllSprocName"></param>
        /// <param name="insertSprocName"></param>
        /// <param name="updateSprocName"></param>
        /// <param name="deleteSprocName"></param>
        /// <param name="databaseColumnToPrimaryKeyProperty"></param>
        /// <param name="databaseColumnToNonPrimaryKeyProperty"></param>
        /// <param name="overwriteExisting"></param>
        /// <param name="typeOfDto"></param>
        public void GenerateSingleTableBOList(string tableName,
                                  string getByPrimaryKeySprocName,
                                  string getAllSprocName,
                                 string getByCriteriaFuzzySprocName,
                                  string getByCriteriaExactSprocName,
                                  Dictionary<string, PropertyInfo> databaseColumnToPrimaryKeyProperty,
                                  Dictionary<string, PropertyInfo> databaseColumnToNonPrimaryKeyProperty,
                                  bool overwriteExisting,
                                  Type typeOfDto)
        {
            string outputFilePath = OUTPUT_PATH_BO_LIST;
            string outputFileName = ClassCreationHelperMethods.GetBoListFileName(tableName);
            string boNamespace = ClassCreationHelperMethods.GetBoListNamespace(_enclosingApplicationNamespace);
            string boClassName = ClassCreationHelperMethods.GetBoClassName(tableName);
            List<string> boNamespaces = GetBoNamespaceList();
            string outputFileAndPath = outputFilePath + outputFileName;


            GenerateSingleTableBOList(boNamespace,
                                  boNamespaces,
                                  boClassName,
                                  outputFileAndPath,
                                  overwriteExisting,
                                  tableName,
                                  getByPrimaryKeySprocName,
                                  getAllSprocName,
                                  getByCriteriaFuzzySprocName,
                                  getByCriteriaExactSprocName,
                                  databaseColumnToPrimaryKeyProperty,
                                  databaseColumnToNonPrimaryKeyProperty,
                                  typeOfDto);



        }


        public void GenerateSingleTableBO(string boNamespace,
                                          List<string> boNamespaces,
                                          string boClassName,
                                          string outputFileAndPath,
                                          bool overwriteExisting,
                                          string tableName,
                                          string getByPrimaryKeySprocName,
                                          string getAllSprocName,
                                          string insertSprocName,
                                          string updateSprocName,
                                          string deleteSprocName,
                                          Dictionary<string, PropertyInfo> databaseColumnToPrimaryKeyProperty,
                                          Dictionary<string, PropertyInfo> databaseColumnToNonPrimaryKeyProperty,
                                          Type typeOfDto)
        {
            CodeCompileUnit targetUnit = new CodeCompileUnit();
            CodeNamespace bo = new CodeNamespace(boNamespace);
            foreach (string strNamespace in boNamespaces)
            {
                bo.Imports.Add(new CodeNamespaceImport(strNamespace));
            }
            CodeTypeDeclaration targetClass = new CodeTypeDeclaration(boClassName);
            targetClass.IsClass = true;
            targetClass.TypeAttributes =
                TypeAttributes.Public;

            CodeTypeReference baseDtoTypeReference = new CodeTypeReference(typeOfDto.FullName);
            targetClass.BaseTypes.Add(baseDtoTypeReference);

            //
            AddEmptyConstructor(MemberAttributes.Public, targetClass);

            bo.Types.Add(targetClass);
            targetUnit.Namespaces.Add(bo);

            CodeMemberField constPublicExceptionField = 
                new CodeMemberField(typeof(string), ClassCreationHelperConstants.FILL_DB_SETTINGS_EXCEPTION_VAR_NAME);
            constPublicExceptionField.Attributes =
                (constPublicExceptionField.Attributes &
                ~MemberAttributes.AccessMask &
                ~MemberAttributes.ScopeMask) |
                MemberAttributes.Public |
                MemberAttributes.Const;
            constPublicExceptionField.InitExpression = new CodePrimitiveExpression(ClassCreationHelperConstants.FILL_DB_SETTINGS_EXCEPTION_TEXT);
            targetClass.Members.Add(constPublicExceptionField);

            CodeMemberField constPublicKeyNotFoundExceptionField =
    new CodeMemberField(typeof(string), ClassCreationHelperConstants.PRIMARY_KEY_NOT_FOUND_EXCEPTION_VAR_NAME);
            constPublicKeyNotFoundExceptionField.Attributes =
                (constPublicKeyNotFoundExceptionField.Attributes &
                ~MemberAttributes.AccessMask &
                ~MemberAttributes.ScopeMask) |
                MemberAttributes.Public |
                MemberAttributes.Const;
            constPublicKeyNotFoundExceptionField.InitExpression = new CodePrimitiveExpression(ClassCreationHelperConstants.PRIMARY_KEY_NOT_FOUND_EXCEPTION_VAR_NAME);
            targetClass.Members.Add(constPublicKeyNotFoundExceptionField);


            CodeMemberField databaseSmoObjectsAndSettingsField =
                new CodeMemberField(typeof(CommonLibrary.DatabaseSmoObjectsAndSettings),
                                    ClassCreationHelperConstants.DATABASE_SMO_OBJECTS_AND_SETTINGS_VAR_NAME);
            targetClass.Members.Add(databaseSmoObjectsAndSettingsField);

            CodeTypeReference baseBusinessCodeTypeReference =
                new CodeTypeReference(ClassCreationHelperConstants.COMMONLIBRARY + 
                                      ClassCreationHelperConstants.DOT_OPERATOR + 
                                      ClassCreationHelperConstants.BASE + 
                                      ClassCreationHelperConstants.DOT_OPERATOR + 
                                      ClassCreationHelperConstants.BUSINESS + 
                                      ClassCreationHelperConstants.DOT_OPERATOR + 
                                      ClassCreationHelperConstants.BASE_BUSINESS,
                new CodeTypeReference [] { new CodeTypeReference(boNamespace + 
                                                                 ClassCreationHelperConstants.DOT_OPERATOR + 
                                                                 boClassName),
                                                                 new CodeTypeReference(typeOfDto)});

            CodeMemberField baseBusinessField = 
                new CodeMemberField(baseBusinessCodeTypeReference,
                ClassCreationHelperConstants.BASE_BUSINESS_VAR_NAME);
            targetClass.Members.Add(baseBusinessField);


            CodeTypeReference baseDataAccessCodeTypeReference =
              new CodeTypeReference(ClassCreationHelperConstants.COMMONLIBRARY +
                                    ClassCreationHelperConstants.DOT_OPERATOR +
                                    ClassCreationHelperConstants.BASE +
                                    ClassCreationHelperConstants.DOT_OPERATOR +
                                    ClassCreationHelperConstants.DATABASE +
                                    ClassCreationHelperConstants.DOT_OPERATOR +
                                    ClassCreationHelperConstants.BASE_DATA_ACCESS,
              new CodeTypeReference[] { new CodeTypeReference(typeOfDto)});

            CodeMemberField baseDataAccessField =
                new CodeMemberField(baseDataAccessCodeTypeReference,
                ClassCreationHelperConstants.BASE_DATA_ACCESS_VAR_NAME);
            targetClass.Members.Add(baseDataAccessField);

            Type databaseSmoObjectsAndSettingsType = typeof(CommonLibrary.DatabaseSmoObjectsAndSettings);
            
            List<CodeParameterDeclarationExpression> listOfParameterExpressions =
               new List<CodeParameterDeclarationExpression>();

            CodeParameterDeclarationExpression databaseSmoObjectsAndSettingsExpression =
               GetParameterDeclarationExpression(databaseSmoObjectsAndSettingsType
               , ClassCreationHelperConstants.DATABASE_SMO_OBJECTS_AND_SETTINGS_PARAMETER_NAME);
            

            listOfParameterExpressions.Add(databaseSmoObjectsAndSettingsExpression);

            CodeConstructor constructor = new CodeConstructor();
            InitializeConstructor(constructor,
                                 MemberAttributes.Public,
                                 listOfParameterExpressions);

            CodeAssignStatement databaseSmoObjectsAndSettingsAssignStatement =
                new CodeAssignStatement(new CodeVariableReferenceExpression(ClassCreationHelperConstants.DATABASE_SMO_OBJECTS_AND_SETTINGS_VAR_NAME),
                                        new CodeVariableReferenceExpression(ClassCreationHelperConstants.DATABASE_SMO_OBJECTS_AND_SETTINGS_PARAMETER_NAME));

            constructor.Statements.Add(databaseSmoObjectsAndSettingsAssignStatement);

            CodeAssignStatement baseDataAccessAssignStatement = 
                new CodeAssignStatement(new CodeVariableReferenceExpression(ClassCreationHelperConstants.BASE_DATA_ACCESS_VAR_NAME),
                                        new CodeSnippetExpression(Environment.NewLine +
                                                                  ClassCreationHelperConstants.TAB +
                                                                  ClassCreationHelperConstants.TAB +
                                                                  ClassCreationHelperConstants.TAB +
                                                                  ClassCreationHelperConstants.TAB +
                                                                  ClassCreationHelperConstants.NEW +
                                                                  ClassCreationHelperConstants.SPACE +
                                                                  ClassCreationHelperConstants.COMMONLIBRARY + 
                                                                  ClassCreationHelperConstants.DOT_OPERATOR +
                                                                  ClassCreationHelperConstants.BASE + 
                                                                  ClassCreationHelperConstants.DOT_OPERATOR + 
                                                                  ClassCreationHelperConstants.DATABASE + 
                                                                  ClassCreationHelperConstants.DOT_OPERATOR +
                                                                  ClassCreationHelperConstants.BASE_DATA_ACCESS +
                                                                  ClassCreationHelperConstants.CSHARP_OPEN_ANGLE_BRACKET +
                                                                  typeOfDto.FullName +
                                                                  ClassCreationHelperConstants.CSHARP_CLOSE_ANGLE_BRACKET + 
                                                                  ClassCreationHelperConstants.CONDITION_OPEN_BRACKET + 
                                                                  ClassCreationHelperConstants.DATABASE_SMO_OBJECTS_AND_SETTINGS_VAR_NAME + 
                                                                  ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET
                                                                  ));

            CodeAssignStatement baseBusinessAssignStatement =
    new CodeAssignStatement(new CodeVariableReferenceExpression(ClassCreationHelperConstants.BASE_BUSINESS_VAR_NAME),
                            new CodeSnippetExpression(Environment.NewLine +
                                                      ClassCreationHelperConstants.TAB +
                                                      ClassCreationHelperConstants.TAB +
                                                      ClassCreationHelperConstants.TAB +
                                                      ClassCreationHelperConstants.TAB +
                                                      ClassCreationHelperConstants.NEW +
                                                      ClassCreationHelperConstants.SPACE +
                                                      ClassCreationHelperConstants.COMMONLIBRARY +
                                                      ClassCreationHelperConstants.DOT_OPERATOR +
                                                      ClassCreationHelperConstants.BASE +
                                                      ClassCreationHelperConstants.DOT_OPERATOR +
                                                      ClassCreationHelperConstants.BUSINESS +
                                                      ClassCreationHelperConstants.DOT_OPERATOR +
                                                      ClassCreationHelperConstants.BASE_BUSINESS +
                                                      ClassCreationHelperConstants.CSHARP_OPEN_ANGLE_BRACKET +
                                                      bo.Name + ClassCreationHelperConstants.DOT_OPERATOR + targetClass.Name +
                                                      ClassCreationHelperConstants.COMMA + 
                                                      ClassCreationHelperConstants.SPACE +
                                                      typeOfDto.FullName +
                                                      ClassCreationHelperConstants.CSHARP_CLOSE_ANGLE_BRACKET +
                                                      ClassCreationHelperConstants.CONDITION_OPEN_BRACKET +
                                                                  ClassCreationHelperConstants.DATABASE_SMO_OBJECTS_AND_SETTINGS_VAR_NAME +
                                                                  ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET
                                                      ));

            constructor.Statements.Add(baseDataAccessAssignStatement);
            constructor.Statements.Add(baseBusinessAssignStatement);
                                                                             
            targetClass.Members.Add(constructor);

            CodeParameterDeclarationExpression filledBoParameter =
               GetParameterDeclarationExpression(boNamespace +
                                                 ClassCreationHelperConstants.DOT_OPERATOR +
                                                 boClassName, ClassCreationHelperConstants.FILLED_BO_PARAMETER_NAME);
            listOfParameterExpressions.Add(filledBoParameter);
            constructor = new CodeConstructor();
            InitializeConstructor(constructor,
                                  MemberAttributes.Public,
                                  listOfParameterExpressions);
            constructor.Statements.Add(databaseSmoObjectsAndSettingsAssignStatement);
            constructor.Statements.Add(baseDataAccessAssignStatement);
            constructor.Statements.Add(baseBusinessAssignStatement);

            CodeVariableReferenceExpression filledBoVar = 
                new CodeVariableReferenceExpression(ClassCreationHelperConstants.FILLED_BO_PARAMETER_NAME);

            CodeThisReferenceExpression thisVar =
                new CodeThisReferenceExpression();

            CodeMethodInvokeExpression fillPropertiesFromBoMethodInvoke =
                new CodeMethodInvokeExpression(new CodeThisReferenceExpression(),
                ClassCreationHelperConstants.FILL_PROPERTIES_FROM_BO_METHOD_NAME,
                filledBoVar);

            constructor.Statements.Add(fillPropertiesFromBoMethodInvoke);

            targetClass.Members.Add(constructor);

            CodeMemberMethod fillPropertiesFromBoMethod =
                new CodeMemberMethod();

            fillPropertiesFromBoMethod.Attributes = MemberAttributes.Private;
            fillPropertiesFromBoMethod.Name = ClassCreationHelperConstants.FILL_PROPERTIES_FROM_BO_METHOD_NAME;

             filledBoParameter =
                new CodeParameterDeclarationExpression(boNamespace +
                                                       ClassCreationHelperConstants.DOT_OPERATOR +
                                                       boClassName,
                                                       ClassCreationHelperConstants.FILLED_BO_PARAMETER_NAME);
            listOfParameterExpressions =
               new List<CodeParameterDeclarationExpression>();

            listOfParameterExpressions.Add(filledBoParameter);

            InitializeCodeMethodParameters(fillPropertiesFromBoMethod, listOfParameterExpressions);

            CodeExpression [] referencesToPassInToFillPropertiesFromBoMethodCall = 
                new CodeExpression [] {filledBoVar,thisVar};

            
            CodeMethodInvokeExpression invokeFillPropertiesFromBoMethod = 
                new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(ClassCreationHelperConstants.BASE_BUSINESS_VAR_NAME),
                ClassCreationHelperConstants.FILL_PROPERTIES_FROM_BO_METHOD_NAME,
                referencesToPassInToFillPropertiesFromBoMethodCall);


            fillPropertiesFromBoMethod.Statements.Add(invokeFillPropertiesFromBoMethod);
            targetClass.Members.Add(fillPropertiesFromBoMethod);

            CodeMemberMethod fillThisWithDtoMethod = new CodeMemberMethod();
            fillThisWithDtoMethod.Attributes = MemberAttributes.Private;
            fillThisWithDtoMethod.Name = ClassCreationHelperConstants.FILL_THIS_WITH_DTO_METHOD_NAME;

            CodeParameterDeclarationExpression filledDtoParameter =
                new CodeParameterDeclarationExpression(typeOfDto,
                                                       ClassCreationHelperConstants.FILLED_DTO_PARAMETER_NAME);

            listOfParameterExpressions =
              new List<CodeParameterDeclarationExpression>();

            listOfParameterExpressions.Add(filledDtoParameter);

            InitializeCodeMethodParameters(fillThisWithDtoMethod, listOfParameterExpressions);

            CodeVariableReferenceExpression filledDtoVar =
               new CodeVariableReferenceExpression(ClassCreationHelperConstants.FILLED_DTO_PARAMETER_NAME);

            CodeExpression[] referencesToPassInToFillThisWithDtoMethodCall =
              new CodeExpression[] { filledDtoVar, thisVar };

            CodeMethodInvokeExpression invokeFillThisWithDtoMethod =
                new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(ClassCreationHelperConstants.BASE_BUSINESS_VAR_NAME),
                ClassCreationHelperConstants.FILL_THIS_WITH_DTO_METHOD_NAME,
                referencesToPassInToFillThisWithDtoMethodCall);

            fillThisWithDtoMethod.Statements.Add(invokeFillThisWithDtoMethod);
            targetClass.Members.Add(fillThisWithDtoMethod);

            CodeMemberMethod fillDtoWithThisMethod = new CodeMemberMethod();
            fillDtoWithThisMethod.Attributes = MemberAttributes.Private;
            fillDtoWithThisMethod.Name = ClassCreationHelperConstants.FILL_DTO_WITH_THIS_METHOD_NAME;

            CodeParameterDeclarationExpression boParameter =
                new CodeParameterDeclarationExpression(boNamespace +
                                                       ClassCreationHelperConstants.DOT_OPERATOR +
                                                       boClassName,
                                                       ClassCreationHelperConstants.BO_PARAMETER_NAME);
            listOfParameterExpressions =
              new List<CodeParameterDeclarationExpression>();

            listOfParameterExpressions.Add(boParameter);

            CodeExpression[] referencesToPassInToFillDtoWithThisMethodCall =
             new CodeExpression[] { thisVar};

            CodeMethodInvokeExpression invokeFillDtoWithThis = 
                new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(ClassCreationHelperConstants.BASE_BUSINESS_VAR_NAME),
                ClassCreationHelperConstants.FILL_DTO_WITH_THIS_METHOD_NAME,
                referencesToPassInToFillDtoWithThisMethodCall);

            fillDtoWithThisMethod.Statements.Add(invokeFillDtoWithThis);
            targetClass.Members.Add(fillDtoWithThisMethod);

            CodeMemberMethod baseDataAccessAvailableMethod = new CodeMemberMethod();
            baseDataAccessAvailableMethod.Attributes = MemberAttributes.Private;
            baseDataAccessAvailableMethod.Name = 
                ClassCreationHelperConstants.BASE_DATA_ACCESS_AVAILABLE_METHOD_NAME;
            baseDataAccessAvailableMethod.ReturnType = new CodeTypeReference(typeof(bool));

            CodeVariableDeclarationStatement boolBaseDataAccessAvailableVar = 
                new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(bool)),
                ClassCreationHelperConstants.BASE_DATA_ACCESS_AVAILABLE_VAR,
                new CodePrimitiveExpression(false)
                );
            boolBaseDataAccessAvailableVar.Name = ClassCreationHelperConstants.BASE_DATA_ACCESS_AVAILABLE_VAR;

            baseDataAccessAvailableMethod.Statements.Add(boolBaseDataAccessAvailableVar);

            CodeConditionStatement databaseObjectsAndSettingsCondition =
                new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(
                ClassCreationHelperConstants.DATABASE_SMO_OBJECTS_AND_SETTINGS_VAR_NAME), CodeBinaryOperatorType.IdentityInequality,
                new CodePrimitiveExpression(null)),
                baseDataAccessAssignStatement);

            CodeStatement [] trueStatements = 
                new CodeStatement[] { databaseObjectsAndSettingsCondition,new CodeAssignStatement(
                new CodeVariableReferenceExpression(
                ClassCreationHelperConstants.BASE_DATA_ACCESS_AVAILABLE_VAR),
                new CodePrimitiveExpression(true))};

            CodeStatement[] falseStatements =
                new CodeStatement[] {new CodeAssignStatement(
                new CodeVariableReferenceExpression(
                ClassCreationHelperConstants.BASE_DATA_ACCESS_AVAILABLE_VAR),
                new CodePrimitiveExpression(true)) };

          
            CodeConditionStatement boolBaseDataAccessAvailableCondition =
                new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(
                ClassCreationHelperConstants.BASE_DATA_ACCESS_VAR_NAME),CodeBinaryOperatorType.ValueEquality,
                new CodePrimitiveExpression(null)),
               trueStatements,
               falseStatements);

            baseDataAccessAvailableMethod.Statements.Add(boolBaseDataAccessAvailableCondition);

            CodeMethodReturnStatement baseDataAccessAvailableReturn =
                new CodeMethodReturnStatement(new CodeVariableReferenceExpression(
                ClassCreationHelperConstants.BASE_DATA_ACCESS_AVAILABLE_VAR));

            baseDataAccessAvailableMethod.Statements.Add(baseDataAccessAvailableReturn);
            targetClass.Members.Add(baseDataAccessAvailableMethod);

            CodeThrowExceptionStatement throwExceptionStatement =  
                new CodeThrowExceptionStatement(
                new CodeObjectCreateExpression(ClassCreationHelperConstants.SYSTEM_APPLICATION_EXCEPTION, 
                new CodeVariableReferenceExpression(constPublicExceptionField.Name)));

            CodeMethodInvokeExpression invokeBaseDataAccessAvailableMethod =
                new CodeMethodInvokeExpression(thisVar,
                ClassCreationHelperConstants.BASE_DATA_ACCESS_AVAILABLE_METHOD_NAME,
                new CodeExpression[] { });

            CodeMemberMethod insertMethod = new CodeMemberMethod();
            insertMethod.Attributes = MemberAttributes.Public;
            insertMethod.Name = ClassCreationHelperConstants.INSERT_METHOD_NAME;

            CodeMemberMethod updateMethod = new CodeMemberMethod();
            updateMethod.Attributes = MemberAttributes.Public;
            updateMethod.Name = ClassCreationHelperConstants.UPDATE_METHOD_NAME;

            CodeMemberMethod deleteMethod = new CodeMemberMethod();
            deleteMethod.Attributes = MemberAttributes.Public;
            deleteMethod.Name = ClassCreationHelperConstants.DELETE_METHOD_NAME;

            CodeMemberMethod getByPrimaryKeyMethod = new CodeMemberMethod();
            getByPrimaryKeyMethod.Attributes = MemberAttributes.Public;
            getByPrimaryKeyMethod.Name = ClassCreationHelperConstants.GET_BY_PRIMARY_KEY_METHOD_NAME;
           

            CodeVariableDeclarationStatement declareDtoVar =
                new CodeVariableDeclarationStatement(new CodeTypeReference(typeOfDto),
                ClassCreationHelperConstants.DTO_PARAMETER_NAME,
                invokeFillDtoWithThis);

            //insertMethod.Statements.Add(declareDtoVar);

            CodeVariableReferenceExpression dtoVar =
                new CodeVariableReferenceExpression(ClassCreationHelperConstants.DTO_PARAMETER_NAME);

            CodeExpression[] referencesToPassInToBaseDataAccessInsertCall =
 new CodeExpression[] { dtoVar };

            CodeMethodInvokeExpression invokeBaseDataAccessInsert =
                new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(ClassCreationHelperConstants.BASE_DATA_ACCESS_VAR_NAME),
                ClassCreationHelperConstants.INSERT_METHOD_NAME,
                referencesToPassInToBaseDataAccessInsertCall);

            CodeVariableDeclarationStatement declareReturnDtoVarForInsert =
                new CodeVariableDeclarationStatement(typeOfDto,
                ClassCreationHelperConstants.RETURN_DTO_VAR,
                invokeBaseDataAccessInsert);

            CodeMethodInvokeExpression invokeBaseDataAccessUpdate =
                new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(
                ClassCreationHelperConstants.BASE_DATA_ACCESS_VAR_NAME),
                ClassCreationHelperConstants.UPDATE_METHOD_NAME,
                referencesToPassInToBaseDataAccessInsertCall);

            CodeVariableDeclarationStatement declareReturnDtoVarForUpdate =
                new CodeVariableDeclarationStatement(typeOfDto,
                ClassCreationHelperConstants.RETURN_DTO_VAR,
                invokeBaseDataAccessUpdate);

            CodeMethodInvokeExpression invokeBaseDataAccessDelete =
                new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(
                ClassCreationHelperConstants.BASE_DATA_ACCESS_VAR_NAME),
                ClassCreationHelperConstants.DELETE_METHOD_NAME,
                referencesToPassInToBaseDataAccessInsertCall);

            CodeVariableDeclarationStatement declareReturnDtoVarForDelete =
                new CodeVariableDeclarationStatement(typeOfDto,
                ClassCreationHelperConstants.RETURN_DTO_VAR,
                invokeBaseDataAccessDelete);            

            CodeVariableDeclarationStatement declareDtoVarForGetByPrimaryKey =
                new CodeVariableDeclarationStatement(new CodeTypeReference(typeOfDto),
                ClassCreationHelperConstants.DTO_PARAMETER_NAME,
                new CodeThisReferenceExpression());

            CodeTypeReferenceExpression enumGetPermutations =
                new CodeTypeReferenceExpression(new CodeTypeReference("CommonLibrary.Enumerations.GetPermutations.ByPrimaryKey"));

            CodeExpression[] referencesToPassInToBaseDataAccessGetByPrimaryKeyCall =
 new CodeExpression[] { dtoVar,enumGetPermutations };



            CodeMethodInvokeExpression invokeBaseDataAccessGetByPrimaryKey =
               new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(
               ClassCreationHelperConstants.BASE_DATA_ACCESS_VAR_NAME),
               ClassCreationHelperConstants.GET_METHOD_NAME,
               referencesToPassInToBaseDataAccessGetByPrimaryKeyCall);

            CodeArrayIndexerExpression dtoIndexerExpression =
               new CodeArrayIndexerExpression(new CodeVariableReferenceExpression(ClassCreationHelperConstants.RETURN_DTO_VAR),
               new CodePrimitiveExpression(0));

            CodeAssignStatement returnDtoVarAssignForGetByPrimaryKey =
                new CodeAssignStatement(dtoIndexerExpression, invokeBaseDataAccessGetByPrimaryKey);

            CodeVariableDeclarationStatement declareReturnDtoVarForGetByPrimaryKey =
                new CodeVariableDeclarationStatement(new CodeTypeReference("List",new CodeTypeReference(typeOfDto)),
                ClassCreationHelperConstants.RETURN_DTO_VAR,
                invokeBaseDataAccessGetByPrimaryKey);


            //insertMethod.Statements.Add(declareReturnDtoVar);

            CodeVariableReferenceExpression dtoReturnVar = 
                new CodeVariableReferenceExpression(ClassCreationHelperConstants.RETURN_DTO_VAR);

            CodeExpression[] referencesToPassInToFillThisWithDto =
                new CodeExpression[] { dtoReturnVar };

            CodeMethodInvokeExpression invokeFillThisWithDtoWithReturnDto =
                new CodeMethodInvokeExpression(thisVar,
                ClassCreationHelperConstants.FILL_THIS_WITH_DTO_METHOD_NAME,
                referencesToPassInToFillThisWithDto);

           //insertMethod.Statements.Add(invokeFillThisWithDtoWithReturnDto);
            

               CodeConditionStatement ifBaseDataAccessAvailableForInsert =
               new CodeConditionStatement(invokeBaseDataAccessAvailableMethod,
               new CodeStatement[] {declareDtoVar,
                declareReturnDtoVarForInsert
                },
               new CodeStatement[] { throwExceptionStatement });

               ifBaseDataAccessAvailableForInsert.TrueStatements.Add(invokeFillThisWithDtoWithReturnDto);         

            insertMethod.Statements.Add(ifBaseDataAccessAvailableForInsert);

            CodeConditionStatement ifBaseDataAccessAvailableForUpdate =
                new CodeConditionStatement(invokeBaseDataAccessAvailableMethod,
                new CodeStatement[] {declareDtoVar,
                    declareReturnDtoVarForUpdate},
                    new CodeStatement[] { throwExceptionStatement });
            ifBaseDataAccessAvailableForUpdate.TrueStatements.Add(invokeFillThisWithDtoWithReturnDto);

            updateMethod.Statements.Add(ifBaseDataAccessAvailableForUpdate);

            CodeConditionStatement ifBaseDataAccessAvailableForDelete =
                new CodeConditionStatement(invokeBaseDataAccessAvailableMethod,
                new CodeStatement[] {declareDtoVar,
                    declareReturnDtoVarForDelete},
                    new CodeStatement[] { throwExceptionStatement });
            ifBaseDataAccessAvailableForDelete.TrueStatements.Add(invokeFillThisWithDtoWithReturnDto);
            
            deleteMethod.Statements.Add(ifBaseDataAccessAvailableForDelete);




            CodeExpression[] referencesToPassInToFillThisWithDtoForGetByPrimaryKey =
               new CodeExpression[] { dtoIndexerExpression };

            CodeMethodInvokeExpression invokeFillThisWithDtoWithReturnDtoForGetByPrimaryKey =
                new CodeMethodInvokeExpression(thisVar,
                ClassCreationHelperConstants.FILL_THIS_WITH_DTO_METHOD_NAME,
                referencesToPassInToFillThisWithDtoForGetByPrimaryKey);

            

            CodeConditionStatement ifDtoReturnCountGreaterThanZero =
                new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(dtoReturnVar, "Count"), CodeBinaryOperatorType.GreaterThan, new CodePrimitiveExpression(0)));
            ifDtoReturnCountGreaterThanZero.TrueStatements.Add(invokeFillThisWithDtoWithReturnDtoForGetByPrimaryKey);

            CodeThrowExceptionStatement throwPrimaryKeyNotFoundExceptionStatement =
              new CodeThrowExceptionStatement(
              new CodeObjectCreateExpression(ClassCreationHelperConstants.SYSTEM_APPLICATION_EXCEPTION,
              new CodeVariableReferenceExpression(constPublicKeyNotFoundExceptionField.Name)));

            ifDtoReturnCountGreaterThanZero.FalseStatements.Add(throwPrimaryKeyNotFoundExceptionStatement);

            CodeConditionStatement ifBaseDataAccessAvailableForGetByPrimaryKey =
             new CodeConditionStatement(invokeBaseDataAccessAvailableMethod,
             new CodeStatement[] {declareDtoVarForGetByPrimaryKey,
                    declareReturnDtoVarForGetByPrimaryKey},
                 new CodeStatement[] { throwExceptionStatement });
            ifBaseDataAccessAvailableForGetByPrimaryKey.TrueStatements.Add(ifDtoReturnCountGreaterThanZero);

            getByPrimaryKeyMethod.Statements.Add(ifBaseDataAccessAvailableForGetByPrimaryKey);

            targetClass.Members.Add(insertMethod);
            targetClass.Members.Add(updateMethod);
            targetClass.Members.Add(deleteMethod);
            targetClass.Members.Add(getByPrimaryKeyMethod);
            

            ////AddPrivateMembersAndPropertiesToDto(targetClass, metaInformationSchema);
            ////AddTableNameAttributeToCodeTypeDeclaration(targetClass, metaInformationSchema);

            AddPublicProperty(targetClass,
                             ClassCreationHelperConstants.DATABASE_SMO_OBJECTS_AND_SETTINGS_PARAMETER_NAME,
                             typeof(CommonLibrary.DatabaseSmoObjectsAndSettings),
                             ClassCreationHelperConstants.DATABASE_SMO_OBJECTS_AND_SETTINGS_VAR_NAME);

            GenerateCSharpCode(outputFileAndPath, targetUnit, overwriteExisting);
        }

        /// <summary>
        /// TODO:  implement this as a list class ,i just copied the GenerateSingleTableBO method
        /// </summary>
        /// <param name="boNamespace"></param>
        /// <param name="boNamespaces"></param>
        /// <param name="boClassName"></param>
        /// <param name="outputFileAndPath"></param>
        /// <param name="overwriteExisting"></param>
        /// <param name="tableName"></param>
        /// <param name="getByPrimaryKeySprocName"></param>
        /// <param name="getAllSprocName"></param>
        /// <param name="insertSprocName"></param>
        /// <param name="updateSprocName"></param>
        /// <param name="deleteSprocName"></param>
        /// <param name="databaseColumnToPrimaryKeyProperty"></param>
        /// <param name="databaseColumnToNonPrimaryKeyProperty"></param>
        /// <param name="typeOfDto"></param>
        public void GenerateSingleTableBOList(string boListNamespace,
                                  List<string> boListNamespaces,
                                  string boListClassName,
                                  string outputFileAndPath,
                                  bool overwriteExisting,
                                  string tableName,
                                  string getByPrimaryKeySprocName,
                                  string getAllSprocName,
                                  string getByCriteriaFuzzySprocName,
                                  string getByCriteriaExactSprocName,                                
                                  Dictionary<string, PropertyInfo> databaseColumnToPrimaryKeyProperty,
                                  Dictionary<string, PropertyInfo> databaseColumnToNonPrimaryKeyProperty,
                                  Type typeOfDto)
        {
            string boNamespace = ClassCreationHelperMethods.GetBoNamespace(_enclosingApplicationNamespace);
            string boClassName = ClassCreationHelperMethods.GetBoClassName(tableName);

            CodeCompileUnit targetUnit = new CodeCompileUnit();
            CodeNamespace boList = new CodeNamespace(boListNamespace);
            foreach (string strNamespace in boListNamespaces)
            {
                boList.Imports.Add(new CodeNamespaceImport(strNamespace));
            }
            CodeTypeDeclaration targetClass = new CodeTypeDeclaration(boListClassName);
            targetClass.IsClass = true;
            targetClass.TypeAttributes =
                TypeAttributes.Public;

            CodeTypeReference baseDtoTypeReference = new CodeTypeReference("List", new CodeTypeReference(typeOfDto));
            targetClass.BaseTypes.Add(baseDtoTypeReference);
            
            //AddNonParameterConstructorButInstantiateBases(MemberAttributes.Public, targetClass,typeOfDto,boNamespace);

            AddEmptyConstructor(MemberAttributes.Public, targetClass);  

            boList.Types.Add(targetClass);

            
            targetUnit.Namespaces.Add(boList);

            CodeMemberField constPublicExceptionField =
                new CodeMemberField(typeof(string), ClassCreationHelperConstants.FILL_DB_SETTINGS_EXCEPTION_VAR_NAME);
            constPublicExceptionField.Attributes =
                (constPublicExceptionField.Attributes &
                ~MemberAttributes.AccessMask &
                ~MemberAttributes.ScopeMask) |
                MemberAttributes.Public |
                MemberAttributes.Const;
            constPublicExceptionField.InitExpression = new CodePrimitiveExpression(ClassCreationHelperConstants.FILL_DB_SETTINGS_EXCEPTION_TEXT);
            targetClass.Members.Add(constPublicExceptionField);

            CodeMemberField constPublicKeyNotFoundExceptionField =
    new CodeMemberField(typeof(string), ClassCreationHelperConstants.PRIMARY_KEY_NOT_FOUND_EXCEPTION_VAR_NAME);
            constPublicKeyNotFoundExceptionField.Attributes =
                (constPublicKeyNotFoundExceptionField.Attributes &
                ~MemberAttributes.AccessMask &
                ~MemberAttributes.ScopeMask) |
                MemberAttributes.Public |
                MemberAttributes.Const;
            constPublicKeyNotFoundExceptionField.InitExpression = new CodePrimitiveExpression(ClassCreationHelperConstants.PRIMARY_KEY_NOT_FOUND_EXCEPTION_VAR_NAME);
            targetClass.Members.Add(constPublicKeyNotFoundExceptionField);


            CodeMemberField databaseSmoObjectsAndSettingsField =
                new CodeMemberField(typeof(CommonLibrary.DatabaseSmoObjectsAndSettings),
                                    ClassCreationHelperConstants.DATABASE_SMO_OBJECTS_AND_SETTINGS_VAR_NAME);
            targetClass.Members.Add(databaseSmoObjectsAndSettingsField);

            CodeTypeReference baseBusinessCodeTypeReference =
                new CodeTypeReference(ClassCreationHelperConstants.COMMONLIBRARY +
                                      ClassCreationHelperConstants.DOT_OPERATOR +
                                      ClassCreationHelperConstants.BASE +
                                      ClassCreationHelperConstants.DOT_OPERATOR +
                                      ClassCreationHelperConstants.BUSINESS +
                                      ClassCreationHelperConstants.DOT_OPERATOR +
                                      ClassCreationHelperConstants.BASE_BUSINESS,
                new CodeTypeReference[] { new CodeTypeReference(boNamespace + 
                                                                 ClassCreationHelperConstants.DOT_OPERATOR + 
                                                                 boClassName),
                                                                 new CodeTypeReference(typeOfDto)});

            CodeMemberField baseBusinessField =
                new CodeMemberField(baseBusinessCodeTypeReference,
                ClassCreationHelperConstants.BASE_BUSINESS_VAR_NAME);
            targetClass.Members.Add(baseBusinessField);


            CodeTypeReference baseDataAccessCodeTypeReference =
              new CodeTypeReference(ClassCreationHelperConstants.COMMONLIBRARY +
                                    ClassCreationHelperConstants.DOT_OPERATOR +
                                    ClassCreationHelperConstants.BASE +
                                    ClassCreationHelperConstants.DOT_OPERATOR +
                                    ClassCreationHelperConstants.DATABASE +
                                    ClassCreationHelperConstants.DOT_OPERATOR +
                                    ClassCreationHelperConstants.BASE_DATA_ACCESS,
              new CodeTypeReference[] { new CodeTypeReference(typeOfDto) });

            CodeMemberField baseDataAccessField =
                new CodeMemberField(baseDataAccessCodeTypeReference,
                ClassCreationHelperConstants.BASE_DATA_ACCESS_VAR_NAME);
            targetClass.Members.Add(baseDataAccessField);

            Type databaseSmoObjectsAndSettingsType = typeof(CommonLibrary.DatabaseSmoObjectsAndSettings);

            List<CodeParameterDeclarationExpression> listOfParameterExpressions =
               new List<CodeParameterDeclarationExpression>();

            CodeParameterDeclarationExpression databaseSmoObjectsAndSettingsExpression =
               GetParameterDeclarationExpression(databaseSmoObjectsAndSettingsType
               , ClassCreationHelperConstants.DATABASE_SMO_OBJECTS_AND_SETTINGS_PARAMETER_NAME);


            listOfParameterExpressions.Add(databaseSmoObjectsAndSettingsExpression);

            CodeConstructor constructor = new CodeConstructor();
            InitializeConstructor(constructor,
                                 MemberAttributes.Public,
                                 listOfParameterExpressions);

            CodeAssignStatement databaseSmoObjectsAndSettingsAssignStatement =
                new CodeAssignStatement(new CodeVariableReferenceExpression(ClassCreationHelperConstants.DATABASE_SMO_OBJECTS_AND_SETTINGS_VAR_NAME),
                                        new CodeVariableReferenceExpression(ClassCreationHelperConstants.DATABASE_SMO_OBJECTS_AND_SETTINGS_PARAMETER_NAME));

            constructor.Statements.Add(databaseSmoObjectsAndSettingsAssignStatement);

            CodeAssignStatement baseDataAccessAssignStatement =
                new CodeAssignStatement(new CodeVariableReferenceExpression(ClassCreationHelperConstants.BASE_DATA_ACCESS_VAR_NAME),
                                        new CodeSnippetExpression(Environment.NewLine +
                                                                  ClassCreationHelperConstants.TAB +
                                                                  ClassCreationHelperConstants.TAB +
                                                                  ClassCreationHelperConstants.TAB +
                                                                  ClassCreationHelperConstants.TAB +
                                                                  ClassCreationHelperConstants.NEW +
                                                                  ClassCreationHelperConstants.SPACE +
                                                                  ClassCreationHelperConstants.COMMONLIBRARY +
                                                                  ClassCreationHelperConstants.DOT_OPERATOR +
                                                                  ClassCreationHelperConstants.BASE +
                                                                  ClassCreationHelperConstants.DOT_OPERATOR +
                                                                  ClassCreationHelperConstants.DATABASE +
                                                                  ClassCreationHelperConstants.DOT_OPERATOR +
                                                                  ClassCreationHelperConstants.BASE_DATA_ACCESS +
                                                                  ClassCreationHelperConstants.CSHARP_OPEN_ANGLE_BRACKET +
                                                                  typeOfDto.FullName +
                                                                  ClassCreationHelperConstants.CSHARP_CLOSE_ANGLE_BRACKET +
                                                                  ClassCreationHelperConstants.CONDITION_OPEN_BRACKET +
                                                                  ClassCreationHelperConstants.DATABASE_SMO_OBJECTS_AND_SETTINGS_VAR_NAME +
                                                                  ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET
                                                                  ));

            CodeAssignStatement baseBusinessAssignStatement =
    new CodeAssignStatement(new CodeVariableReferenceExpression(ClassCreationHelperConstants.BASE_BUSINESS_VAR_NAME),
                            new CodeSnippetExpression(Environment.NewLine +
                                                      ClassCreationHelperConstants.TAB +
                                                      ClassCreationHelperConstants.TAB +
                                                      ClassCreationHelperConstants.TAB +
                                                      ClassCreationHelperConstants.TAB +
                                                      ClassCreationHelperConstants.NEW +
                                                      ClassCreationHelperConstants.SPACE +
                                                      ClassCreationHelperConstants.COMMONLIBRARY +
                                                      ClassCreationHelperConstants.DOT_OPERATOR +
                                                      ClassCreationHelperConstants.BASE +
                                                      ClassCreationHelperConstants.DOT_OPERATOR +
                                                      ClassCreationHelperConstants.BUSINESS +
                                                      ClassCreationHelperConstants.DOT_OPERATOR +
                                                      ClassCreationHelperConstants.BASE_BUSINESS +
                                                      ClassCreationHelperConstants.CSHARP_OPEN_ANGLE_BRACKET +
                                                      boNamespace + ClassCreationHelperConstants.DOT_OPERATOR + targetClass.Name +
                                                      ClassCreationHelperConstants.COMMA +
                                                      ClassCreationHelperConstants.SPACE +
                                                      typeOfDto.FullName +
                                                      ClassCreationHelperConstants.CSHARP_CLOSE_ANGLE_BRACKET +
                                                      ClassCreationHelperConstants.CONDITION_OPEN_BRACKET +
                                                                  ClassCreationHelperConstants.DATABASE_SMO_OBJECTS_AND_SETTINGS_VAR_NAME +
                                                                  ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET
                                                      ));

            constructor.Statements.Add(baseDataAccessAssignStatement);
            constructor.Statements.Add(baseBusinessAssignStatement);

            targetClass.Members.Add(constructor);

            CodeParameterDeclarationExpression filledBoParameter =
               GetParameterDeclarationExpression(boNamespace +
                                                 ClassCreationHelperConstants.DOT_OPERATOR +
                                                 boClassName, ClassCreationHelperConstants.FILLED_BO_PARAMETER_NAME);

           
            listOfParameterExpressions.Add(filledBoParameter);
            //constructor = new CodeConstructor();
            //InitializeConstructor(constructor,
            //                      MemberAttributes.Public,
            //                      listOfParameterExpressions);
            //constructor.Statements.Add(databaseSmoObjectsAndSettingsAssignStatement);
            //constructor.Statements.Add(baseDataAccessAssignStatement);
            //constructor.Statements.Add(baseBusinessAssignStatement);

            //CodeVariableReferenceExpression filledBoVar =
            //    new CodeVariableReferenceExpression(ClassCreationHelperConstants.FILLED_BO_PARAMETER_NAME);

            //CodeThisReferenceExpression thisVar =
            //    new CodeThisReferenceExpression();

            //CodeMethodInvokeExpression fillPropertiesFromBoMethodInvoke =
            //    new CodeMethodInvokeExpression(new CodeThisReferenceExpression(),
            //    ClassCreationHelperConstants.FILL_PROPERTIES_FROM_BO_METHOD_NAME,
            //    filledBoVar);

            //constructor.Statements.Add(fillPropertiesFromBoMethodInvoke);

            //targetClass.Members.Add(constructor);

            CodeMemberMethod fillByGetPermutationMethod =
                new CodeMemberMethod();

            fillByGetPermutationMethod.Attributes = MemberAttributes.Private;
            fillByGetPermutationMethod.Name = ClassCreationHelperConstants.FILL_BY_GET_PERMUTATION_METHOD_NAME;

            CodeTypeReference enumPermutation = new CodeTypeReference("CommonLibrary.Enumerations.GetPermutations");            

            CodeVariableReferenceExpression filledBoVar =
              new CodeVariableReferenceExpression(ClassCreationHelperConstants.FILLED_BO_PARAMETER_NAME);
            CodeVariableReferenceExpression enumPermutationVar =
                new CodeVariableReferenceExpression(ClassCreationHelperConstants.GET_PERMUTATION_PARAMETER_VAR);
            
            CodeVariableReferenceExpression dtoVar =
                new CodeVariableReferenceExpression(ClassCreationHelperConstants.DTO_PARAMETER_NAME);


            CodeParameterDeclarationExpression getPermutationParameter =
                new CodeParameterDeclarationExpression(enumPermutation, ClassCreationHelperConstants.GET_PERMUTATION_PARAMETER_VAR);
    

            filledBoParameter =
               new CodeParameterDeclarationExpression(boNamespace +
                                                      ClassCreationHelperConstants.DOT_OPERATOR +
                                                      boClassName,
                                                      ClassCreationHelperConstants.FILLED_BO_PARAMETER_NAME);
            listOfParameterExpressions =
               new List<CodeParameterDeclarationExpression>();
            listOfParameterExpressions.Add(getPermutationParameter);

            listOfParameterExpressions.Add(filledBoParameter);

            InitializeCodeMethodParameters(fillByGetPermutationMethod, listOfParameterExpressions);
           
            
            CodeExpression[] referencesToPassInToFillByGetPermutationMethodCall =
                new CodeExpression[] { filledBoVar, enumPermutationVar };

            CodeThisReferenceExpression thisVar =
                new CodeThisReferenceExpression();

            CodeMethodInvokeExpression invokeClear =
                new CodeMethodInvokeExpression(thisVar,
                                               ClassCreationHelperConstants.CLEAR,
                                               new CodeExpression[] { });

            //fillByGetPermutationMethod.Statements.Add(invokeClear); 

            CodeTypeReference boTypeReference = new CodeTypeReference(boNamespace + 
                                                                      ClassCreationHelperConstants.DOT_OPERATOR +
                                                                      boClassName);

            CodeCastExpression castDtoTypeToBo = 
                new CodeCastExpression(typeOfDto,filledBoVar);

            CodeVariableDeclarationStatement dtoVarDeclaration =
                new CodeVariableDeclarationStatement(typeOfDto,
                                                     ClassCreationHelperConstants.DTO_PARAMETER_NAME,
                                                     castDtoTypeToBo);

            CodeVariableReferenceExpression dtoVarReference =
                new CodeVariableReferenceExpression(ClassCreationHelperConstants.DTO_PARAMETER_NAME);

            //fillByGetPermutationMethod.Statements.Add(dtoVarDeclaration);

            CodeMethodInvokeExpression invokeBaseDataAccessGet = 
                new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(ClassCreationHelperConstants.BASE_DATA_ACCESS_VAR_NAME),
                ClassCreationHelperConstants.GET_METHOD_NAME,
                new CodeExpression [] {dtoVarReference,enumPermutationVar});

            CodeVariableDeclarationStatement dtosReturned =
                new CodeVariableDeclarationStatement(new CodeTypeReference("List", new CodeTypeReference(typeOfDto)),
                ClassCreationHelperConstants.RETURN_DTO_VAR,
                invokeBaseDataAccessGet);

            //fillByGetPermutationMethod.Statements.Add(dtosReturned);

            CodeMemberMethod baseDataAccessAvailableMethod = new CodeMemberMethod();
            baseDataAccessAvailableMethod.Attributes = MemberAttributes.Private;
            baseDataAccessAvailableMethod.Name =
                ClassCreationHelperConstants.BASE_DATA_ACCESS_AVAILABLE_METHOD_NAME;
            baseDataAccessAvailableMethod.ReturnType = new CodeTypeReference(typeof(bool));

            CodeVariableDeclarationStatement boolBaseDataAccessAvailableVar =
                new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(bool)),
                ClassCreationHelperConstants.BASE_DATA_ACCESS_AVAILABLE_VAR,
                new CodePrimitiveExpression(false)
                );
            boolBaseDataAccessAvailableVar.Name = ClassCreationHelperConstants.BASE_DATA_ACCESS_AVAILABLE_VAR;

            baseDataAccessAvailableMethod.Statements.Add(boolBaseDataAccessAvailableVar);

            CodeConditionStatement databaseObjectsAndSettingsCondition =
                new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(
                ClassCreationHelperConstants.DATABASE_SMO_OBJECTS_AND_SETTINGS_VAR_NAME), CodeBinaryOperatorType.IdentityInequality,
                new CodePrimitiveExpression(null)),
                baseDataAccessAssignStatement);

            CodeStatement[] trueStatements =
                new CodeStatement[] { databaseObjectsAndSettingsCondition,new CodeAssignStatement(
                new CodeVariableReferenceExpression(
                ClassCreationHelperConstants.BASE_DATA_ACCESS_AVAILABLE_VAR),
                new CodePrimitiveExpression(true))};

            CodeStatement[] falseStatements =
                new CodeStatement[] {new CodeAssignStatement(
                new CodeVariableReferenceExpression(
                ClassCreationHelperConstants.BASE_DATA_ACCESS_AVAILABLE_VAR),
                new CodePrimitiveExpression(true)) };


            CodeConditionStatement boolBaseDataAccessAvailableCondition =
                new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(
                ClassCreationHelperConstants.BASE_DATA_ACCESS_VAR_NAME), CodeBinaryOperatorType.ValueEquality,
                new CodePrimitiveExpression(null)),
               trueStatements,
               falseStatements);

            baseDataAccessAvailableMethod.Statements.Add(boolBaseDataAccessAvailableCondition);

            CodeMethodReturnStatement baseDataAccessAvailableReturn =
                new CodeMethodReturnStatement(new CodeVariableReferenceExpression(
                ClassCreationHelperConstants.BASE_DATA_ACCESS_AVAILABLE_VAR));

            baseDataAccessAvailableMethod.Statements.Add(baseDataAccessAvailableReturn);
            targetClass.Members.Add(baseDataAccessAvailableMethod);

            CodeThrowExceptionStatement throwExceptionStatement =
              new CodeThrowExceptionStatement(
              new CodeObjectCreateExpression(ClassCreationHelperConstants.SYSTEM_APPLICATION_EXCEPTION,
              new CodeVariableReferenceExpression(constPublicExceptionField.Name)));


            CodeMethodInvokeExpression invokeBaseDataAccessAvailableMethod =
                new CodeMethodInvokeExpression(thisVar,
                ClassCreationHelperConstants.BASE_DATA_ACCESS_AVAILABLE_METHOD_NAME,
                new CodeExpression[] { });

            CodeStatementCollection coll = new CodeStatementCollection();
            coll.Add(invokeClear);
            coll.Insert(1, dtoVarDeclaration);
            coll.Insert(2, dtosReturned);

            CodeConditionStatement ifBaseDataAccessAvailableForGetByPermutationMethod =
           new CodeConditionStatement(invokeBaseDataAccessAvailableMethod);
            ifBaseDataAccessAvailableForGetByPermutationMethod.TrueStatements.AddRange(coll);

            CodeStatement [] falseItems = new CodeStatement[] { throwExceptionStatement };
            ifBaseDataAccessAvailableForGetByPermutationMethod.FalseStatements.AddRange(falseItems);            

            fillByGetPermutationMethod.Statements.Add(ifBaseDataAccessAvailableForGetByPermutationMethod);
            targetClass.Members.Add(fillByGetPermutationMethod);

            CodeVariableDeclarationStatement declareCounterVariable =
                new CodeVariableDeclarationStatement(typeof(int),
                ClassCreationHelperConstants.COUNTER_VAR);

             CodeAssignStatement counterVariable = 
                 new CodeAssignStatement( new CodeVariableReferenceExpression(
                 ClassCreationHelperConstants.COUNTER_VAR), new CodePrimitiveExpression(0) );

             //CodeArrayIndexerExpression dtoIndexerExpression =
             //  new CodeArrayIndexerExpression(new CodeVariableReferenceExpression(ClassCreationHelperConstants.RETURN_DTO_VAR),
             //  new CodeMethodInvokeExpression(dtosReturned,
             //                                 ClassCreationHelperConstants.COUNT,
             //                                 new CodeExpression [] {}));

            //CodeAssignStatement returnDtoVarAssignForGetByPrimaryKey =
            //    new CodeAssignStatement(dtoIndexerExpression, invokeBaseDataAccessGetByPrimaryKey);

             CodeVariableReferenceExpression dtosReturnedReference =
                 new CodeVariableReferenceExpression(dtosReturned.Name);

            //this is incorrect as the Count is a property and not a method
             //CodeMethodInvokeExpression invokeCountOnDtosReturned =
             //    new CodeMethodInvokeExpression(dtosReturnedReference,
             //    ClassCreationHelperConstants.COUNT,
             //    new CodeExpression[] { });

            //use this instead to refer to a property or call a property.
             CodePropertyReferenceExpression countProperty = new CodePropertyReferenceExpression(dtosReturnedReference, "Count");

             CodeVariableDeclarationStatement controlVariableDeclaration =
                 new CodeVariableDeclarationStatement(typeof(int),
                                                      ClassCreationHelperConstants.CONTROL_VAR,
                                                      countProperty);

            CodeVariableReferenceExpression controlVariableReference = 
                new CodeVariableReferenceExpression(controlVariableDeclaration.Name);
                                                     


             ifBaseDataAccessAvailableForGetByPermutationMethod.TrueStatements.Add(controlVariableDeclaration);

            CodeConditionStatement ifControlVariableCountGreaterThanZero = 
                new CodeConditionStatement(new CodeBinaryOperatorExpression(controlVariableReference,
                                                                            CodeBinaryOperatorType.GreaterThan,
                                                                            new CodePrimitiveExpression(0)));

            ifBaseDataAccessAvailableForGetByPermutationMethod.TrueStatements.Add(ifControlVariableCountGreaterThanZero);

            CodeVariableReferenceExpression counterVariableReference = 
                new CodeVariableReferenceExpression(ClassCreationHelperConstants.COUNTER_VAR);

            CodeBinaryOperatorExpression lessThanControl =
                new CodeBinaryOperatorExpression(counterVariableReference,
                                                CodeBinaryOperatorType.LessThan,
                                                controlVariableReference);
            CodeAssignStatement incrementStatement =
                new CodeAssignStatement(counterVariableReference, new CodeBinaryOperatorExpression(
                                         counterVariableReference, CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1)));
            
            //generate the CodeStatement [] of code statements to be executed within the loop          

                      

            CodeVariableDeclarationStatement declareBoToFill =
                new CodeVariableDeclarationStatement(boTypeReference,
                                                     ClassCreationHelperConstants.BO_TO_FILL_VAR,
                                                     new CodeObjectCreateExpression(boTypeReference, new CodeExpression[] { 
                                                     new CodeVariableReferenceExpression(ClassCreationHelperConstants.DATABASE_SMO_OBJECTS_AND_SETTINGS_VAR_NAME)}));


            
            CodeVariableReferenceExpression boToFillReference = 
                new CodeVariableReferenceExpression(ClassCreationHelperConstants.BO_TO_FILL_VAR);

              CodeArrayIndexerExpression dtoIndexerExpression =
               new CodeArrayIndexerExpression(new CodeVariableReferenceExpression(ClassCreationHelperConstants.RETURN_DTO_VAR),
               counterVariableReference);

              CodeVariableReferenceExpression baseBusinessVarReference =
                  new CodeVariableReferenceExpression(ClassCreationHelperConstants.BASE_BUSINESS_VAR_NAME);
            

              CodeMethodInvokeExpression invokeFillThisWithDto =
                  new CodeMethodInvokeExpression(baseBusinessVarReference,
                                                  ClassCreationHelperConstants.FILL_THIS_WITH_DTO_METHOD_NAME,
                                                  new CodeExpression [] {dtoIndexerExpression,boToFillReference});

              CodeMethodInvokeExpression invokeAdd =
                  new CodeMethodInvokeExpression(thisVar,
                                                 ClassCreationHelperConstants.ADD,
                                                 boToFillReference);



              CodeIterationStatement forLoop = new CodeIterationStatement(counterVariable,
                                                                          lessThanControl,
                                                                          incrementStatement,
                                                                          new CodeStatement[] {
                                                                            declareBoToFill,
                                                                            new CodeExpressionStatement(invokeFillThisWithDto),
                                                                            new CodeExpressionStatement(invokeAdd)});

              ifControlVariableCountGreaterThanZero.TrueStatements.Add(declareCounterVariable); 
            ifControlVariableCountGreaterThanZero.TrueStatements.Add(forLoop);

              CodeTypeReferenceExpression enumGetPermutations =
                  new CodeTypeReferenceExpression(new CodeTypeReference("CommonLibrary.Enumerations.GetPermutations"));

            CodeTypeReferenceExpression enumGetPermutationsByPrimaryKey =
                new CodeTypeReferenceExpression(new CodeTypeReference("CommonLibrary.Enumerations.GetPermutations.ByPrimaryKey"));

            CodeVariableDeclarationStatement getPermutationByPrimaryKey =
                new CodeVariableDeclarationStatement(new CodeTypeReference("CommonLibrary.Enumerations.GetPermutations"),
                                                     ClassCreationHelperConstants.GET_PERMUTATION_PARAMETER_VAR,
                                                     enumGetPermutationsByPrimaryKey);
                                                     
             CodeVariableReferenceExpression getPermutationVar = 
                 new CodeVariableReferenceExpression(ClassCreationHelperConstants.GET_PERMUTATION_PARAMETER_VAR);

             CodeMethodInvokeExpression invokeFillByGetPermutation =
                 new CodeMethodInvokeExpression(thisVar,
                                                ClassCreationHelperConstants.FILL_BY_GET_PERMUTATION_METHOD_NAME,
                                                new CodeExpression[] { getPermutationVar, filledBoVar });

              CodeMemberMethod fillByPrimaryKeyMethod = new CodeMemberMethod();
              fillByPrimaryKeyMethod.Attributes = MemberAttributes.Public;
              fillByPrimaryKeyMethod.Parameters.Add(filledBoParameter);

              fillByPrimaryKeyMethod.Name = ClassCreationHelperConstants.FILL_BY_PRIMARY_KEY;
              fillByPrimaryKeyMethod.Statements.Add(getPermutationByPrimaryKey);
              fillByPrimaryKeyMethod.Statements.Add(invokeFillByGetPermutation);
              targetClass.Members.Add(fillByPrimaryKeyMethod);

              CodeTypeReferenceExpression enumGetPermutationsByFuzzyCriteria =
                new CodeTypeReferenceExpression(new CodeTypeReference("CommonLibrary.Enumerations.GetPermutations.ByFuzzyCriteria"));

              CodeVariableDeclarationStatement getPermutationByFuzzyCriteria =
                  new CodeVariableDeclarationStatement(new CodeTypeReference("CommonLibrary.Enumerations.GetPermutations"),
                                                       ClassCreationHelperConstants.GET_PERMUTATION_PARAMETER_VAR,
                                                       enumGetPermutationsByFuzzyCriteria);

              CodeMemberMethod fillByFuzzyCriteriaMethod = new CodeMemberMethod();              
              fillByFuzzyCriteriaMethod.Attributes = MemberAttributes.Public;
              fillByFuzzyCriteriaMethod.Name = ClassCreationHelperConstants.FILL_BY_CRITERIA_FUZZY;
              fillByFuzzyCriteriaMethod.Parameters.Add(filledBoParameter);

              fillByFuzzyCriteriaMethod.Statements.Add(getPermutationByFuzzyCriteria);
              fillByFuzzyCriteriaMethod.Statements.Add(invokeFillByGetPermutation);
              targetClass.Members.Add(fillByFuzzyCriteriaMethod);

              CodeTypeReferenceExpression enumGetPermutationsByExplicitCriteria =
    new CodeTypeReferenceExpression(new CodeTypeReference("CommonLibrary.Enumerations.GetPermutations.ByExplicitCriteria"));

              CodeVariableDeclarationStatement getPermutationByExplicitCriteria =
                  new CodeVariableDeclarationStatement(new CodeTypeReference("CommonLibrary.Enumerations.GetPermutations"),
                                                       ClassCreationHelperConstants.GET_PERMUTATION_PARAMETER_VAR,
                                                       enumGetPermutationsByExplicitCriteria);

              CodeMemberMethod fillByExplicitCriteriaMethod = new CodeMemberMethod();
              fillByExplicitCriteriaMethod.Attributes = MemberAttributes.Public;
              fillByExplicitCriteriaMethod.Name = ClassCreationHelperConstants.FILL_BY_CRITERIA_EXACT;
              fillByExplicitCriteriaMethod.Parameters.Add(filledBoParameter);

              fillByExplicitCriteriaMethod.Statements.Add(getPermutationByExplicitCriteria);
              fillByExplicitCriteriaMethod.Statements.Add(invokeFillByGetPermutation);
              targetClass.Members.Add(fillByExplicitCriteriaMethod);

              CodeTypeReferenceExpression enumGetPermutationsAllByColumnMappings =
new CodeTypeReferenceExpression(new CodeTypeReference("CommonLibrary.Enumerations.GetPermutations.AllByColumnMappings"));

              CodeVariableDeclarationStatement getPermutationAllByColumnMappings =
                  new CodeVariableDeclarationStatement(new CodeTypeReference("CommonLibrary.Enumerations.GetPermutations"),
                                                       ClassCreationHelperConstants.GET_PERMUTATION_PARAMETER_VAR,
                                                       enumGetPermutationsAllByColumnMappings);

              CodeMemberMethod fillAllByColumnMappingsMethod = new CodeMemberMethod();
              fillAllByColumnMappingsMethod.Attributes = MemberAttributes.Public;
              fillAllByColumnMappingsMethod.Name = ClassCreationHelperConstants.FILL_BY_GET_ALL;
              fillAllByColumnMappingsMethod.Parameters.Add(filledBoParameter);

              fillAllByColumnMappingsMethod.Statements.Add(getPermutationAllByColumnMappings);
              fillAllByColumnMappingsMethod.Statements.Add(invokeFillByGetPermutation);
              targetClass.Members.Add(fillAllByColumnMappingsMethod);           


            AddPublicProperty(targetClass,
                              ClassCreationHelperConstants.DATABASE_SMO_OBJECTS_AND_SETTINGS_PARAMETER_NAME,
                              typeof(CommonLibrary.DatabaseSmoObjectsAndSettings),
                              ClassCreationHelperConstants.DATABASE_SMO_OBJECTS_AND_SETTINGS_VAR_NAME);

            GenerateCSharpCode(outputFileAndPath, targetUnit, overwriteExisting);
        }

        public void AddPublicProperty(CodeTypeDeclaration targetClass,
                                      string propertyName,
                                      Type type,
                                      string privateMemberVariable
                                      )
        {
            CodeMemberProperty columnProperty = new CodeMemberProperty();
            columnProperty.Attributes = MemberAttributes.Public;
            columnProperty.Name = GetPublicMemberName(propertyName, targetClass.Name);
            columnProperty.Type = new CodeTypeReference(type);

            CodeThisReferenceExpression codeThisReferenceExpression = new
               CodeThisReferenceExpression();

            CodeFieldReferenceExpression codefieldReferenceExpression =
                new CodeFieldReferenceExpression(codeThisReferenceExpression, privateMemberVariable);


            CodeMethodReturnStatement codeMethodReturnStatement =
                new CodeMethodReturnStatement(codefieldReferenceExpression);

            CodeAssignStatement codeAssignStatement =
                new CodeAssignStatement(codefieldReferenceExpression, new CodePropertySetValueReferenceExpression());

            columnProperty.GetStatements.Add(codeMethodReturnStatement);
            columnProperty.SetStatements.Add(codeAssignStatement);

            columnProperty.HasGet = true;
            columnProperty.HasSet = true;

            targetClass.Members.Add(columnProperty);
        }

        public string GetPublicMemberName(string name, string targetClassName)
        {
            string upperCaseStartLetter = ((String)name)[0].ToString().ToUpper();
            string firstLetterRemoved = name.Remove(0, 1);
            string upperCaseStartLetterName = upperCaseStartLetter + firstLetterRemoved;
            if (upperCaseStartLetterName == targetClassName)
            {
                upperCaseStartLetterName += ClassCreationHelperConstants.RESOLVE_DUPLICATE_CLASS_AND_PROPERTY_NAME;

            }
            return upperCaseStartLetterName;
        }


        public void AddEmptyConstructor(MemberAttributes memberAttributes,
           CodeTypeDeclaration targetClass)
        {
            CodeConstructor constructor = new CodeConstructor();
            constructor.Attributes = memberAttributes;
            targetClass.Members.Add(constructor);

        }

        public void AddNonParameterConstructorButInstantiateBases(MemberAttributes memberAttributes,
            CodeTypeDeclaration targetClass,Type typeOfDto, string boNamespace)
        {
            CodeConstructor constructor = new CodeConstructor();
            constructor.Attributes = memberAttributes;

            //CodeAssignStatement databaseSmoObjectsAndSettingsAssignStatement =
            //   new CodeAssignStatement(new CodeVariableReferenceExpression(ClassCreationHelperConstants.DATABASE_SMO_OBJECTS_AND_SETTINGS_VAR_NAME),
            //                           new CodeVariableReferenceExpression(ClassCreationHelperConstants.DATABASE_SMO_OBJECTS_AND_SETTINGS_PARAMETER_NAME));

            //constructor.Statements.Add(databaseSmoObjectsAndSettingsAssignStatement);

            CodeAssignStatement baseDataAccessAssignStatement =
                new CodeAssignStatement(new CodeVariableReferenceExpression(ClassCreationHelperConstants.BASE_DATA_ACCESS_VAR_NAME),
                                        new CodeSnippetExpression(Environment.NewLine +
                                                                  ClassCreationHelperConstants.TAB +
                                                                  ClassCreationHelperConstants.TAB +
                                                                  ClassCreationHelperConstants.TAB +
                                                                  ClassCreationHelperConstants.TAB +
                                                                  ClassCreationHelperConstants.NEW +
                                                                  ClassCreationHelperConstants.SPACE +
                                                                  ClassCreationHelperConstants.COMMONLIBRARY +
                                                                  ClassCreationHelperConstants.DOT_OPERATOR +
                                                                  ClassCreationHelperConstants.BASE +
                                                                  ClassCreationHelperConstants.DOT_OPERATOR +
                                                                  ClassCreationHelperConstants.DATABASE +
                                                                  ClassCreationHelperConstants.DOT_OPERATOR +
                                                                  ClassCreationHelperConstants.BASE_DATA_ACCESS +
                                                                  ClassCreationHelperConstants.CSHARP_OPEN_ANGLE_BRACKET +
                                                                  typeOfDto.FullName +
                                                                  ClassCreationHelperConstants.CSHARP_CLOSE_ANGLE_BRACKET +
                                                                  ClassCreationHelperConstants.CONDITION_OPEN_BRACKET +
                                                                  ClassCreationHelperConstants.DATABASE_SMO_OBJECTS_AND_SETTINGS_VAR_NAME +
                                                                  ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET
                                                                  ));

            CodeAssignStatement baseBusinessAssignStatement =
    new CodeAssignStatement(new CodeVariableReferenceExpression(ClassCreationHelperConstants.BASE_BUSINESS_VAR_NAME),
                            new CodeSnippetExpression(Environment.NewLine +
                                                      ClassCreationHelperConstants.TAB +
                                                      ClassCreationHelperConstants.TAB +
                                                      ClassCreationHelperConstants.TAB +
                                                      ClassCreationHelperConstants.TAB +
                                                      ClassCreationHelperConstants.NEW +
                                                      ClassCreationHelperConstants.SPACE +
                                                      ClassCreationHelperConstants.COMMONLIBRARY +
                                                      ClassCreationHelperConstants.DOT_OPERATOR +
                                                      ClassCreationHelperConstants.BASE +
                                                      ClassCreationHelperConstants.DOT_OPERATOR +
                                                      ClassCreationHelperConstants.BUSINESS +
                                                      ClassCreationHelperConstants.DOT_OPERATOR +
                                                      ClassCreationHelperConstants.BASE_BUSINESS +
                                                      ClassCreationHelperConstants.CSHARP_OPEN_ANGLE_BRACKET +
                                                      boNamespace + ClassCreationHelperConstants.DOT_OPERATOR + targetClass.Name +
                                                      ClassCreationHelperConstants.COMMA +
                                                      ClassCreationHelperConstants.SPACE +
                                                      typeOfDto.FullName +
                                                      ClassCreationHelperConstants.CSHARP_CLOSE_ANGLE_BRACKET +
                                                      ClassCreationHelperConstants.CONDITION_OPEN_BRACKET +
                                                      ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET
                                                      ));

            constructor.Statements.Add(baseDataAccessAssignStatement);
            constructor.Statements.Add(baseBusinessAssignStatement);
            targetClass.Members.Add(constructor);
        }

        public void InitializeCodeMethodParameters(CodeMemberMethod method,
                                                   List<CodeParameterDeclarationExpression> parameterExpressions)
        {
            foreach (CodeParameterDeclarationExpression parameterExpression in parameterExpressions)
            {
                method.Parameters.Add(parameterExpression);
            }
        }

        public CodeConstructor InitializeConstructor(CodeConstructor constructor,
                                                    MemberAttributes memberAttributes,
                                                    List<CodeParameterDeclarationExpression> parameterExpressions)
        {

            constructor.Attributes = memberAttributes;

            foreach (CodeParameterDeclarationExpression parameterExpression in parameterExpressions)
            {
                constructor.Parameters.Add(parameterExpression);
            }

            return constructor;
        }

        public CodeParameterDeclarationExpression GetParameterDeclarationExpression(string typeName,
                                                                                   string parameterName)
        {
            CodeParameterDeclarationExpression codeParameterDeclarationExpression =
                new CodeParameterDeclarationExpression(typeName, parameterName);
            return codeParameterDeclarationExpression;
        }

        public CodeParameterDeclarationExpression GetParameterDeclarationExpression(Type type,
                                                                                   string parameterName)
        {
            CodeParameterDeclarationExpression codeParameterDeclarationExpression =
                new CodeParameterDeclarationExpression(type, parameterName);
            return codeParameterDeclarationExpression;
        }

        public void GenerateCSharpCode(string fileName, CodeCompileUnit targetUnit, bool overwriteExisting)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";

            if (overwriteExisting)
            {
                using (StreamWriter sourceWriter = new StreamWriter(fileName))
                {
                    provider.GenerateCodeFromCompileUnit(
                        targetUnit, sourceWriter, options);
                }
            }

            Microsoft.CSharp.CSharpCodeProvider cp
                               = new Microsoft.CSharp.CSharpCodeProvider();
            System.CodeDom.Compiler.ICodeCompiler ic = cp.CreateCompiler();
            System.CodeDom.Compiler.CompilerParameters cpar
                                  = new System.CodeDom.Compiler.CompilerParameters();
            cpar.GenerateInMemory = true;
            cpar.GenerateExecutable = false;
            cpar.ReferencedAssemblies.Add(@System.Configuration.ConfigurationSettings.AppSettings["CommonLibraryDllLocation"]);//@"C:\Development\standalone\Nancy Code Generator\SprocDataLayerGenerator\bin\Debug\CommonLibrary.dll");
            cpar.ReferencedAssemblies.Add(@"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\System.dll");
            cpar.ReferencedAssemblies.Add(@"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\System.Data.dll");
            cpar.ReferencedAssemblies.Add(@"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\System.Drawing.dll");
            cpar.ReferencedAssemblies.Add(@System.Configuration.ConfigurationSettings.AppSettings["CurrentAppRunningExeLocation"]);//@"C:\Development\standalone\Nancy Code Generator\TestSprocGenerator\bin\Debug\TestSprocGenerator.exe");
            cpar.ReferencedAssemblies.Add(@"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\System.XML.dll");

            System.CodeDom.Compiler.CompilerResults cr =
                //= ic.CompileAssemblyFromFile(cpar, fileName);
                                                cp.CompileAssemblyFromFile(cpar, fileName);

            if (cr.Errors.Count > 0)
            {
                cr = cp.CompileAssemblyFromDom(cpar, targetUnit);
            }



            if (cr.Errors.Count == 0 && cr.CompiledAssembly != null)
            {
                object myobj = cr.CompiledAssembly.CreateInstance(targetUnit.Namespaces[0].Name + ClassCreationHelperConstants.DOT_OPERATOR + targetUnit.Namespaces[0].Types[0].Name);
                bool found = false;
                int index = -1;
                foreach (object objectInMemory in _assembliesGeneratedInMemory)
                {

                    if (objectInMemory.GetType().FullName.Equals(myobj.GetType().FullName))
                    {
                        index = _assembliesGeneratedInMemory.IndexOf(objectInMemory);
                        found = true;
                        break;
                    }
                }
                if (found && index > -1)
                {
                    _assembliesGeneratedInMemory.RemoveAt(index);
                    _assembliesGeneratedInMemory.Insert(index, myobj);
                }
                else
                {
                    _assembliesGeneratedInMemory.Add(myobj);
                }
            }

        }

                                          

        public List<string> GetBoNamespaceList()
        {
            List<string> namespaceList = new List<string>();
            namespaceList.Add(ClassCreationHelperConstants.SYSTEM);
            namespaceList.Add(ClassCreationHelperConstants.SYSTEM + ClassCreationHelperConstants.DOT_OPERATOR + ClassCreationHelperConstants.COLLECTIONS + ClassCreationHelperConstants.DOT_OPERATOR + ClassCreationHelperConstants.GENERIC);
            namespaceList.Add(ClassCreationHelperConstants.SYSTEM + ClassCreationHelperConstants.DOT_OPERATOR + ClassCreationHelperConstants.TEXT);
            namespaceList.Add(ClassCreationHelperConstants.COMMONLIBRARY);
            namespaceList.Add(ClassCreationHelperConstants.SYSTEM +
                              ClassCreationHelperConstants.DOT_OPERATOR +
                              ClassCreationHelperConstants.REFLECTION);
            return namespaceList;
        }
                                          


        public Type GetTypeToReflect(string dtoName)
        {
            Type typeToReflect = null;
            string dtoFullName = dtoName;

            //this would mean that this file already exists and has been compiled into the app
            Assembly asm = Assembly.ReflectionOnlyLoad(_enclosingApplicationNamespace);
            //need to pre-load any dependant assemblies when loading reflectiononly
            Assembly.ReflectionOnlyLoad("CommonLibrary");

            typeToReflect = asm.GetType(dtoFullName);
            return typeToReflect;
        }

    }
}
