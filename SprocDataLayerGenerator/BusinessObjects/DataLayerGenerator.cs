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
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Server;

using SprocDataLayerGenerator.Data;
using SprocDataLayerGenerator.List;
using SqlDbTypeConstants = CommonLibrary.Constants.SqlDbConstants;
using SqlNativeTypeConstants = CommonLibrary.Constants.NativeSqlConstants;
using TableConstraintTypeConstants = SprocDataLayerGenerator.Constants.TableConstraintTypesConstants;
using ClassCreationHelperConstants = CommonLibrary.Constants.ClassCreationConstants;
using ClassCreationHelperMethods = CommonLibrary.Utility.ClassCreationHelperMethods;
using DatabaseHelperMethods = CommonLibrary.Utility.DatabaseHelperMethods;
using StringHelper = CommonLibrary.Utility.StringManipulation;
using CommonLibrary;

using CommonLibrary.CustomAttributes;



namespace SprocDataLayerGenerator.BusinessObjects
{
    public class DataLayerGenerator
    {
        private List<object> _assembliesGeneratedInMemory =
            new List<object>();

        public List<object> AssembliesGeneratedInMemory
        {
            get { return _assembliesGeneratedInMemory; }
            set { _assembliesGeneratedInMemory = value; }
        }

        MetaInformationSchemaManager _metaInformationSchemaManager = null;
        string _enclosingApplicationNamespace = string.Empty;

        CommonLibrary.DatabaseSmoObjectsAndSettings _databaseSmoObjectsAndSettings = null;

        private const string OUTPUT_PATH_DTO = @"..\..\GeneratedDtos\";
        private const string OUTPUT_PATH_LIST = @"..\..\GeneratedLists\";
        private const string OUTPUT_PATH_DATA_ACCESS = @"..\..\";
        private const string GENERATED_DATA_ACCESS_FILE_NAME = "GeneratedDataAccess.cs";

        public event ResolveEventHandler ReflectionOnlyAssemblyResolve;
        

        public DataLayerGenerator(MetaInformationSchemaManager metaInformationSchemaManager,
            string enclosingApplicationNamespace)
        {
            _metaInformationSchemaManager = metaInformationSchemaManager;
            _enclosingApplicationNamespace = enclosingApplicationNamespace;
            this.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(DataLayerGenerator_ReflectionOnlyAssemblyResolve);
           
        }

        public DataLayerGenerator(CommonLibrary.DatabaseSmoObjectsAndSettings databaseSmoObjectsAndSettings,
                                  string enclosingApplicationNamespace)
        {
            _databaseSmoObjectsAndSettings = databaseSmoObjectsAndSettings;
            _enclosingApplicationNamespace = enclosingApplicationNamespace;
            this.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(DataLayerGenerator_ReflectionOnlyAssemblyResolve);

        }

        public Assembly DataLayerGenerator_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
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

        public MetaSprocSqlDependencyManager GetMetaSprocSqlDependencyManager(List<StoredProcedure> customSprocsToGenerate)
        {
            MetaSprocSqlDependencyManager metaSprocSqlDependencyManager =
               new MetaSprocSqlDependencyManager(_databaseSmoObjectsAndSettings,
                                                 customSprocsToGenerate);
            return metaSprocSqlDependencyManager;
        }

        public MetaInformationSchemaManager GetMetaInformationSchemaManager(CommonLibrary.DatabaseSmoObjectsAndSettings databaseSmoObjectsAndSettings)
        {
            string databaseName = _databaseSmoObjectsAndSettings.DatabaseName;
            string dataSource = _databaseSmoObjectsAndSettings.DataSource;
            string initialCatalog = _databaseSmoObjectsAndSettings.InitialCatalog;
            string userId = _databaseSmoObjectsAndSettings.UserId;
            string password = _databaseSmoObjectsAndSettings.Password;
            bool trustedConnection = _databaseSmoObjectsAndSettings.TrustedConnection;
            string schema = _databaseSmoObjectsAndSettings.Schema;

            MetaInformationSchemaManager metaInformationSchemaManager =
              new MetaInformationSchemaManager(databaseName,
                                               dataSource,
                                               initialCatalog,
                                               userId,
                                               password,
                                               trustedConnection,
                                               schema);
            return metaInformationSchemaManager;
        }

        public void GenerateCustomSprocsDtosAndLists(List<StoredProcedure> customSprocsToGenerate,
                                                     out Dictionary<string, string> mainSprocNameToInputDto,
                                                     out Dictionary<string, List<string>> sprocToDtoListsUsed,
                                                     out Dictionary<string, List<string>> sprocToInputDtosUsed,
                                                     out Dictionary<string, List<string>> sprocToDtosUsed,
                                                     out Dictionary<string, string> standaloneSprocToInputDto)
        {
            PredicateFunctions predicateFunctions = new PredicateFunctions();

            sprocToDtosUsed = 
                GenerateCustomSprocDtos(customSprocsToGenerate);

            sprocToInputDtosUsed =
                GenerateInputCustomSprocDtos(customSprocsToGenerate);

             MetaSprocSqlDependencyManager metaSprocSqlDependencyManager =
                GetMetaSprocSqlDependencyManager(customSprocsToGenerate);

             MetaInformationSchemaManager metaInformationSchemaManager =
                 new MetaInformationSchemaManager(_databaseSmoObjectsAndSettings.DatabaseName,
                                                 _databaseSmoObjectsAndSettings.DataSource,
                                                 _databaseSmoObjectsAndSettings.InitialCatalog,
                                                 _databaseSmoObjectsAndSettings.UserId,
                                                 _databaseSmoObjectsAndSettings.Password,
                                                 _databaseSmoObjectsAndSettings.TrustedConnection,
                                                 _databaseSmoObjectsAndSettings.Schema);
             _metaInformationSchemaManager = metaInformationSchemaManager;

             sprocToDtoListsUsed =
                 GenerateResultListsForSprocs(sprocToDtosUsed, metaSprocSqlDependencyManager,customSprocsToGenerate);
             
            foreach (KeyValuePair<string, List<string>> kvpSprocToDtoListsUsed in sprocToDtoListsUsed)
             {
                //if null then it is a insert,update,or delete
                 if (kvpSprocToDtoListsUsed.Value == null)
                 {
                     //we must call a method to generate the proper input dto and add it to the 
                     //sprocToInputDtosUsed dictionary
                     List<string> dtosUsedValue;
                     sprocToDtosUsed.TryGetValue(kvpSprocToDtoListsUsed.Key, out dtosUsedValue);
                     foreach (string dtoUsed in dtosUsedValue)
                     {
                         string tableName = GetSprocTableNameFromDtoUsedFullName(dtoUsed,_enclosingApplicationNamespace);
                         predicateFunctions.TableNameHolder = tableName;

                         MetaInformationSchema metaInformationSchema = 
                             _metaInformationSchemaManager.MetaDataList.Find(predicateFunctions.FindMetaInformationSchemaByTableName);
                        
                         predicateFunctions.SprocNameHolder = kvpSprocToDtoListsUsed.Key;
                         MetaSprocSqlDependency metaSprocSqlDependency =
                             metaSprocSqlDependencyManager.MetaSprocSqlDependencyList.Find(predicateFunctions.FindMetaSqlSprocBySprocName);
                      
                        //need to find the column dependencies which are found by the referenced minor i think
                         List<Data.MetaSqlDependency> columnDependencies = new List<SprocDataLayerGenerator.Data.MetaSqlDependency>();
                         foreach (KeyValuePair<string, List<Data.MetaSqlDependency>> kvpTableToColumnDependency in
                             metaSprocSqlDependency.TableDependencyToColumnsReferenced)
                         {
                             if (kvpTableToColumnDependency.Key == tableName)
                             {
                                 columnDependencies.AddRange(kvpTableToColumnDependency.Value);
                             }
                         }

                         string inputDtoUsed = GenerateInputDtoForNonGetCustomSproc(metaInformationSchema, kvpSprocToDtoListsUsed.Key,
                                                              tableName,columnDependencies, customSprocsToGenerate);
                         List<string> inputDtosAlreadyMapped;
                         sprocToInputDtosUsed.TryGetValue(kvpSprocToDtoListsUsed.Key, out inputDtosAlreadyMapped);
                         if (inputDtosAlreadyMapped == null)
                         {
                             List<string> inputDtoGeneratedToAdd = new List<string>();
                             inputDtoGeneratedToAdd.Add(inputDtoUsed);
                             sprocToInputDtosUsed.Add(kvpSprocToDtoListsUsed.Key, inputDtoGeneratedToAdd);
                         }

                                                             
                     }

                 }
             }

             Dictionary<string, List<KeyValuePair<string, List<string>>>> 
                 mainSprocNameToSprocsItCallsSprocNameToListOfDtoListsUsed =
                GetMainSprocNameToKeySprocCalledToValueDtoListsUsed(metaSprocSqlDependencyManager, sprocToDtoListsUsed);

            standaloneSprocToInputDto = new Dictionary<string, string>();

             mainSprocNameToInputDto =
                 GenerateMainSprocInputDtos(mainSprocNameToSprocsItCallsSprocNameToListOfDtoListsUsed,
                 sprocToDtoListsUsed,
                 sprocToDtosUsed,
                 metaInformationSchemaManager,
                 metaSprocSqlDependencyManager,
                 out standaloneSprocToInputDto);    
                
             GenerateDataAccessClass(mainSprocNameToInputDto,
                                     sprocToDtoListsUsed,
                                     sprocToInputDtosUsed,
                                     sprocToDtosUsed,
                                     standaloneSprocToInputDto);
        }

        public string GetSprocTableNameFromDtoUsedFullName(string dtoUsedFullName, string enclosingApplicationNamespace)
        {
            string stringToRemove = enclosingApplicationNamespace +
                                    ClassCreationHelperConstants.DOT_OPERATOR +
                                    ClassCreationHelperConstants.DATA +
                                    ClassCreationHelperConstants.DOT_OPERATOR +
                                    ClassCreationHelperConstants.SPROC_TABLE +
                                    ClassCreationHelperConstants.DOT_OPERATOR +
                                    ClassCreationHelperConstants.DTO +
                                    ClassCreationHelperConstants.DOT_OPERATOR;

            string tableNameOnly = dtoUsedFullName.Replace(stringToRemove, "");
            return tableNameOnly;

        }

        public Dictionary<string, string> GenerateMainSprocInputDtos(Dictionary<string, List<KeyValuePair<string, List<string>>>>
                 mainSprocNameToSprocsItCallsSprocNameToListOfDtoListsUsed,
            Dictionary<string, List<string>> sprocToDtoListsUsed,
            Dictionary<string, List<string>> sprocToDtosUsed,
            MetaInformationSchemaManager metaInformationSchemaManager,
            MetaSprocSqlDependencyManager metaSprocSqlDependencyManager,
            out Dictionary<string, string> standaloneSprocToInputDto)
        {
            standaloneSprocToInputDto = new Dictionary<string, string>();

            Dictionary<string, string> mainSprocToInputDto = new Dictionary<string, string>();
            List<string> sprocsGenerated = new List<string>();

            PredicateFunctions predicateFunctions = new PredicateFunctions();

            foreach (KeyValuePair<string, List<KeyValuePair<string, List<string>>>> kvp
                        in mainSprocNameToSprocsItCallsSprocNameToListOfDtoListsUsed)
            {
                string mainSprocName = kvp.Key;
                StoredProcedure sproc = _databaseSmoObjectsAndSettings.Database_Property.StoredProcedures[mainSprocName];
                
                string inputDtoUsed = GenerateInputDtoForMainCustomSproc(mainSprocName,
                                                                                sproc);
                mainSprocToInputDto.Add(mainSprocName, inputDtoUsed);
                sprocsGenerated.Add(mainSprocName);

            }

            List<string> sprocsNotFound = new List<string>();

            foreach (KeyValuePair<string, List<string>> kvpSprocToDtoListsUsed in sprocToDtoListsUsed)
            {
                if (mainSprocToInputDto.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kvpMainSprocToInputDto in mainSprocToInputDto)
                    {
                        MetaSprocSqlDependency metaSprocSqlDependency = null;
                        predicateFunctions.SprocNameHolder = kvpMainSprocToInputDto.Key;
                        metaSprocSqlDependency = metaSprocSqlDependencyManager.MetaSprocSqlDependencyList.Find(predicateFunctions.FindMetaSqlSprocBySprocName);
                        if (metaSprocSqlDependency.SprocDependencies.Count > 0)
                        {
                            predicateFunctions.ReferencedObjectHolder = kvpSprocToDtoListsUsed.Key;
                            Data.MetaSqlDependency metaSqlDependency =
                                metaSprocSqlDependency.SprocDependencies.Find(predicateFunctions.FindMetaSqlDependenciesByReferencedObject);
                            if (metaSqlDependency == null)
                            {
                                //did not find it as a dependency in this main sproc
                                if (!sprocsNotFound.Contains(kvpSprocToDtoListsUsed.Key))
                                {
                                    sprocsNotFound.Add(kvpSprocToDtoListsUsed.Key);
                                }
                            }
                            else
                                if (sprocsNotFound.Contains(kvpSprocToDtoListsUsed.Key))
                                {
                                    //it may not have been found in previous main sprocs dependendencies
                                    //but now it has been found so remove it from the set
                                    sprocsNotFound.Remove(kvpSprocToDtoListsUsed.Key);
                                }
                        }

                    }

                    //so basically if this is still not null then it was not found as a main sproc that calls other sprocs
                    //and it is not in the list of sprocs that the main sproc calls, and therefore it is standalone
                    //even though it really may be called by a main sproc, the sql_dependencies table may not have
                    //an explicit entry for it, so the user would have to pick it out of the list of all available
                    //sprocs to have it generated.       
                }
                
                
            }

            if (sprocsNotFound.Count > 0)
            {
                foreach (string sprocNotFound in sprocsNotFound)
                {
                    string standaloneSprocName = sprocNotFound;
                    StoredProcedure standaloneSproc =
                        _databaseSmoObjectsAndSettings.Database_Property.StoredProcedures[standaloneSprocName];
                    string standaloneInputDtoUsed = GenerateInputDtoForMainCustomSproc(standaloneSprocName, standaloneSproc);
                    standaloneSprocToInputDto.Add(standaloneSprocName, standaloneInputDtoUsed);
                    sprocsGenerated.Add(standaloneSprocName);
                }
            }

            foreach (KeyValuePair<string, List<string>> kvpSprocToDtoListsUsed in sprocToDtoListsUsed)
            {
                //then it was not generated
                if (!sprocsGenerated.Contains(kvpSprocToDtoListsUsed.Key))
                {
                    string standaloneSprocName = kvpSprocToDtoListsUsed.Key;
                    StoredProcedure standaloneSproc =
                        _databaseSmoObjectsAndSettings.Database_Property.StoredProcedures[standaloneSprocName];
                    string standaloneInputDtoUsed = GenerateInputDtoForMainCustomSproc(standaloneSprocName, standaloneSproc);
                    standaloneSprocToInputDto.Add(standaloneSprocName, standaloneInputDtoUsed);
                    sprocsGenerated.Add(standaloneSprocName);
                }
            }


            return mainSprocToInputDto;
        }

        public void GenerateDataAccessClass(Dictionary<string, string> mainSprocNameToInputDto,
                                            Dictionary<string, List<string>> sprocToDtoListsUsed,
                                            Dictionary<string, List<string>> sprocToInputDtosUsed,
                                            Dictionary<string, List<string>> sprocToDtosUsed,
                                            Dictionary<string, string> standaloneSprocToInputDto
                                            )
        {
            bool overwriteExisting = true;
            string outputFileAndPath = OUTPUT_PATH_DATA_ACCESS + GENERATED_DATA_ACCESS_FILE_NAME;

           
            CodeCompileUnit targetUnit = new CodeCompileUnit();
            CodeNamespace dataAccessClass = 
                new CodeNamespace(ClassCreationHelperMethods.GetDataAccessClassNamespace(_enclosingApplicationNamespace));

            List<string> dataAccessClassNamespaces = GetDataAccessClassNamespaceList(_enclosingApplicationNamespace);
            foreach (string strNamespace in dataAccessClassNamespaces)
            {
                dataAccessClass.Imports.Add(new CodeNamespaceImport(strNamespace));
            }

            string dataAccessClassName = ClassCreationHelperMethods.GetDataAccessClassName();
            CodeTypeDeclaration targetClass = new CodeTypeDeclaration(dataAccessClassName);

            targetClass.IsClass = true;
            targetClass.TypeAttributes = TypeAttributes.Public;

            string dataAccessClassFullName = ClassCreationHelperMethods.GetDataAccessClassNamespace(_enclosingApplicationNamespace) +
                                             ClassCreationHelperConstants.DOT_OPERATOR +
                                             dataAccessClassName;

            CodeTypeReference baseDatabaseTypeReference = new CodeTypeReference(typeof(CommonLibrary.Base.Database.BaseDatabase));
            targetClass.BaseTypes.Add(baseDatabaseTypeReference);

            AddEmptyConstructor(MemberAttributes.Public, targetClass);

            dataAccessClass.Types.Add(targetClass);
            targetUnit.Namespaces.Add(dataAccessClass);
            //GenerateCSharpCode(outputFileAndPath, targetUnit, overwriteExisting);


            //CodeTypeReference listTypeReference = new CodeTypeReference("List", new CodeTypeReference[] { new CodeTypeReference(dtoFullName) });
            //targetClass.BaseTypes.Add(listTypeReference);
            //Type type = typeof(CommonLibrary.Base.Database.BaseDatabase);
            //AddPrivateMember(targetClass, GetPrivateMemberName(type.Name), type);

            //going to create the datalayer methods for each individual sproc first
            foreach (KeyValuePair<string, List<string>> kvp in sprocToDtoListsUsed)
            {
                //if the lists used value is null then this is a non-get operation insert,update,delete
                if (kvp.Value != null)
                {
                    List<string> value;
                    sprocToInputDtosUsed.TryGetValue(kvp.Key, out value);
                    if (value != null)
                    {
                        if (value.Count == 1)
                        {
                            GenerateDataLayerMethod(value[0], kvp.Key, kvp.Value[0], targetClass, dataAccessClassFullName, outputFileAndPath);
                            GenerateCSharpCode(outputFileAndPath, targetUnit, overwriteExisting);

                        }
                    }
                }
                else
                {
                    List<string> value;
                    sprocToInputDtosUsed.TryGetValue(kvp.Key, out value);
                    if (value != null)
                    {
                        if (value.Count == 1)
                        {
                            GenerateNonGetDataLayerMethod(value[0], kvp.Key, targetClass, dataAccessClassFullName, outputFileAndPath);
                            GenerateCSharpCode(outputFileAndPath, targetUnit, overwriteExisting);
                        }
                    }
                }
                
            }

            //now going to create the datalayer methods for the MainSprocs
            foreach (KeyValuePair<string, string> kvpMainSprocToInputDto in mainSprocNameToInputDto)
            {
                //this call generates the method that returns dataset
                GenerateMainSprocDataLayerMethod(kvpMainSprocToInputDto.Value, kvpMainSprocToInputDto.Key,
                                                  targetClass, dataAccessClassFullName,outputFileAndPath);
                GenerateCSharpCode(outputFileAndPath, targetUnit, overwriteExisting);

            }

            foreach (KeyValuePair<string, string> kvpStandaloneSprocToInputDto in standaloneSprocToInputDto)
            {
                List<string> listValue;
                sprocToDtoListsUsed.TryGetValue(kvpStandaloneSprocToInputDto.Key, out listValue);

                //if there is a list value then this is a get operation
                if (listValue != null)
                {
                    //this call generates the method that returns dataset
                    GenerateMainSprocDataLayerMethod(kvpStandaloneSprocToInputDto.Value, kvpStandaloneSprocToInputDto.Key, targetClass, dataAccessClassFullName, outputFileAndPath);
                    GenerateCSharpCode(outputFileAndPath, targetUnit, overwriteExisting);

                    List<string> value;
                    sprocToDtoListsUsed.TryGetValue(kvpStandaloneSprocToInputDto.Key, out value);
                    if (value != null)
                    {
                        if (value.Count == 1)
                        {
                            GenerateDataLayerMethod(kvpStandaloneSprocToInputDto.Value,
                                kvpStandaloneSprocToInputDto.Key, value[0], targetClass, dataAccessClassFullName, outputFileAndPath);
                            GenerateCSharpCode(outputFileAndPath, targetUnit, overwriteExisting);

                        }
                    }
                }
                    //if not then this is a non-get operation and we only need one permutation of it
                    //no dataset or strongly typed list returned.
                else
                {
                    List<string> value;
                    sprocToDtoListsUsed.TryGetValue(kvpStandaloneSprocToInputDto.Key, out value);
                    if (value != null)
                    {
                        if (value.Count == 1)
                        {
                            GenerateNonGetDataLayerMethod(kvpStandaloneSprocToInputDto.Value,
                                kvpStandaloneSprocToInputDto.Key, targetClass, dataAccessClassFullName, outputFileAndPath);
                            GenerateCSharpCode(outputFileAndPath, targetUnit, overwriteExisting);

                        }
                    }
                }
            }           

            GenerateCSharpCode(outputFileAndPath, targetUnit, overwriteExisting);            
        }

      
        public bool DoesMethodAlreadyExist(string targetClassName,
                                           string dataLayerMethodName,
                                           string returnTypeFullName,
                                           string outputFileAndPath)
        {
            bool methodFound = false;

            Type typeToReflect = GetTypeToReflect(targetClassName);


            if (typeToReflect != null)
            {
                MethodInfo[] methodInfos = typeToReflect.GetMethods();

                foreach (MethodInfo methodInfo in methodInfos)
                {
                    if (methodInfo.Name == dataLayerMethodName)
                    {
                        if (methodInfo.ReturnParameter.ParameterType.FullName == returnTypeFullName)
                        {
                            methodFound = true;
                            break;
                        }
                    }
                }
            }         

            return methodFound;
        }

        public bool DoesMethodAlreadyExist(string targetClassName,
                                   string dataLayerMethodName,                                   
                                   string outputFileAndPath)
        {
            bool methodFound = false;

            Type typeToReflect = GetTypeToReflect(targetClassName);


            if (typeToReflect != null)
            {
                MethodInfo[] methodInfos = typeToReflect.GetMethods();

                foreach (MethodInfo methodInfo in methodInfos)
                {
                    if (methodInfo.Name == dataLayerMethodName)
                    {                       
                            methodFound = true;
                            break;
                    }
                }
            }

            return methodFound;
        }


        public void GenerateDataLayerMethod(string inputDtoFullName,
                                            string sprocFullName,
                                            string dtoReturnListFullName,
                                            CodeTypeDeclaration targetClass,
                                            string dataAccessClassFullName,
                                            string outputFileAndPath)
        {
            string dataLayerMethodName = ClassCreationHelperMethods.GetDataLayerMethodName(sprocFullName);

            bool methodFound = DoesMethodAlreadyExist(dataAccessClassFullName,
                                                      dataLayerMethodName,
                                                      dtoReturnListFullName,
                                                      outputFileAndPath);
            if (!methodFound)
            {
                PredicateFunctions predicateFunctions = new PredicateFunctions();

                CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
                codeMemberMethod.Name = dataLayerMethodName;
                codeMemberMethod.ReturnType = new CodeTypeReference(dtoReturnListFullName);
                codeMemberMethod.Attributes = MemberAttributes.Public;


                //Type type = typeof(string);
                //AddPrivateMember(targetClass, ClassCreationHelperConstants.VAR_SPROC_NAME, type);          

                List<CodeParameterDeclarationExpression> listOfParameterExpressions =
                    new List<CodeParameterDeclarationExpression>();

                Type connectionStringType = typeof(string);


                CodeParameterDeclarationExpression connectionStringExpression = GetParameterDeclarationExpression(connectionStringType,
    ClassCreationHelperConstants.VAR_CONNECTION_STRING_NAME);
                listOfParameterExpressions.Add(connectionStringExpression);

                //we want this defined in the method we do not need the consumer to provide it.
                //CodeParameterDeclarationExpression sprocNameExpression = GetParameterDeclarationExpression(typeof(string),
                //    ClassCreationHelperConstants.VAR_SPROC_NAME);
                //listOfParameterExpressions.Add(sprocNameExpression);

                predicateFunctions.AssemblyFullName = inputDtoFullName;

                Type typeToReflect = null;
                Object objAssembly = null;

                objAssembly = AssembliesGeneratedInMemory.Find(predicateFunctions.FindAssemblyLoadedInMemoryByFullAssemblyName);

                if (objAssembly != null)
                {
                    typeToReflect = objAssembly.GetType();
                }

                CodeParameterDeclarationExpression inputObjectExpression =
                    GetParameterDeclarationExpression(typeToReflect, ClassCreationHelperConstants.VAR_INPUT_OBJECT_NAME);

                listOfParameterExpressions.Add(inputObjectExpression);


                foreach (CodeParameterDeclarationExpression parameterExpression in listOfParameterExpressions)
                {
                    codeMemberMethod.Parameters.Add(parameterExpression);
                }

                CodeSnippetStatement sqlParametersGetReaderSnippet =
                    new CodeSnippetStatement(GetSqlParametersCodeSnippetFromInput(typeToReflect, sprocFullName));

                codeMemberMethod.Statements.Add(sqlParametersGetReaderSnippet);

                CodeSnippetStatement getFillAndReturnResultListSnippet =
                    new CodeSnippetStatement(GetFillAndReturnResultListSnippet(dtoReturnListFullName));

                codeMemberMethod.Statements.Add(getFillAndReturnResultListSnippet);
                targetClass.Members.Add(codeMemberMethod);
            }
        }

        public void GenerateNonGetDataLayerMethod(string inputDtoFullName,
                                    string sprocFullName,                                    
                                    CodeTypeDeclaration targetClass,
                                    string dataAccessClassFullName,
                                    string outputFileAndPath)
        {
            string dataLayerMethodName = ClassCreationHelperMethods.GetDataLayerMethodName(sprocFullName);

            bool methodFound = DoesMethodAlreadyExist(dataAccessClassFullName,
                                                      dataLayerMethodName,                                                      
                                                      outputFileAndPath);
            if (!methodFound)
            {
                PredicateFunctions predicateFunctions = new PredicateFunctions();

                CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
                codeMemberMethod.Name = dataLayerMethodName;                
                codeMemberMethod.Attributes = MemberAttributes.Public;


                //Type type = typeof(string);
                //AddPrivateMember(targetClass, ClassCreationHelperConstants.VAR_SPROC_NAME, type);          

                List<CodeParameterDeclarationExpression> listOfParameterExpressions =
                    new List<CodeParameterDeclarationExpression>();

                Type connectionStringType = typeof(string);


                CodeParameterDeclarationExpression connectionStringExpression = GetParameterDeclarationExpression(connectionStringType,
    ClassCreationHelperConstants.VAR_CONNECTION_STRING_NAME);
                listOfParameterExpressions.Add(connectionStringExpression);

                //we want this defined in the method we do not need the consumer to provide it.
                //CodeParameterDeclarationExpression sprocNameExpression = GetParameterDeclarationExpression(typeof(string),
                //    ClassCreationHelperConstants.VAR_SPROC_NAME);
                //listOfParameterExpressions.Add(sprocNameExpression);

                predicateFunctions.AssemblyFullName = inputDtoFullName;

                Type typeToReflect = null;
                Object objAssembly = null;

                objAssembly = AssembliesGeneratedInMemory.Find(predicateFunctions.FindAssemblyLoadedInMemoryByFullAssemblyName);

                if (objAssembly != null)
                {
                    typeToReflect = objAssembly.GetType();
                }

                CodeParameterDeclarationExpression inputObjectExpression =
                    GetParameterDeclarationExpression(typeToReflect, ClassCreationHelperConstants.VAR_INPUT_OBJECT_NAME);

                listOfParameterExpressions.Add(inputObjectExpression);


                foreach (CodeParameterDeclarationExpression parameterExpression in listOfParameterExpressions)
                {
                    codeMemberMethod.Parameters.Add(parameterExpression);
                }

                CodeSnippetStatement sqlParametersExecuteNonQuerySnippet =
                    new CodeSnippetStatement(GetSqlParametersForNonGetCodeSnippetFromInput(typeToReflect, sprocFullName));

                codeMemberMethod.Statements.Add(sqlParametersExecuteNonQuerySnippet);

                //CodeSnippetStatement getFillAndReturnResultListSnippet =
                //    new CodeSnippetStatement(GetFillAndReturnResultListSnippet(dtoReturnListFullName));

                //codeMemberMethod.Statements.Add(getFillAndReturnResultListSnippet);
                targetClass.Members.Add(codeMemberMethod);
            }
        }


        public void GenerateMainSprocDataLayerMethod(string inputDtoFullName,
                                    string sprocFullName,
                                    CodeTypeDeclaration targetClass,
                                    string dataAccessClassFullName,
                                    string outputFileAndPath)
        {
            string dataLayerMethodName = ClassCreationHelperMethods.GetDataLayerMethodName(sprocFullName);
            string dataSetTypeName = typeof(DataSet).FullName;

            bool methodFound = DoesMethodAlreadyExist(dataAccessClassFullName,
                                                     dataLayerMethodName,
                                                     dataSetTypeName,
                                                     outputFileAndPath);
            if (!methodFound)
            {

                PredicateFunctions predicateFunctions = new PredicateFunctions();

                CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
                codeMemberMethod.Name = dataLayerMethodName + "_DataSet"; ;
                codeMemberMethod.ReturnType = new CodeTypeReference(typeof(DataSet));
                codeMemberMethod.Attributes = MemberAttributes.Public;

                List<CodeParameterDeclarationExpression> listOfParameterExpressions =
                    new List<CodeParameterDeclarationExpression>();

                Type connectionStringType = typeof(string);


                CodeParameterDeclarationExpression connectionStringExpression = GetParameterDeclarationExpression(connectionStringType,
    ClassCreationHelperConstants.VAR_CONNECTION_STRING_NAME);
                listOfParameterExpressions.Add(connectionStringExpression);

                predicateFunctions.AssemblyFullName = inputDtoFullName;

                Type typeToReflect = null;
                Object objAssembly = null;

                objAssembly = AssembliesGeneratedInMemory.Find(predicateFunctions.FindAssemblyLoadedInMemoryByFullAssemblyName);

                if (objAssembly != null)
                {
                    typeToReflect = objAssembly.GetType();
                }

                CodeParameterDeclarationExpression inputObjectExpression =
                    GetParameterDeclarationExpression(typeToReflect, ClassCreationHelperConstants.VAR_INPUT_OBJECT_NAME);

                listOfParameterExpressions.Add(inputObjectExpression);


                foreach (CodeParameterDeclarationExpression parameterExpression in listOfParameterExpressions)
                {
                    codeMemberMethod.Parameters.Add(parameterExpression);
                }

                CodeSnippetStatement sqlParametersGetDataSetSnippet =
                    new CodeSnippetStatement(GetSqlParametersCodeSnippetForMainSprocFromInput(typeToReflect, sprocFullName));

                codeMemberMethod.Statements.Add(sqlParametersGetDataSetSnippet);

                CodeSnippetStatement getFillAndReturnDataSetSnippet =
                    new CodeSnippetStatement(GetFillAndReturnDataSetSnippet());

                codeMemberMethod.Statements.Add(getFillAndReturnDataSetSnippet);
                targetClass.Members.Add(codeMemberMethod);
            }
        }

        public void GenerateMainSprocNonGetDataLayerMethod(string inputDtoFullName,
                            string sprocFullName,
                            CodeTypeDeclaration targetClass,
                            string dataAccessClassFullName,
                            string outputFileAndPath)
        {
            string dataLayerMethodName = ClassCreationHelperMethods.GetDataLayerMethodName(sprocFullName);
            

            bool methodFound = DoesMethodAlreadyExist(dataAccessClassFullName,
                                                     dataLayerMethodName,                                                     
                                                     outputFileAndPath);
            if (!methodFound)
            {

                PredicateFunctions predicateFunctions = new PredicateFunctions();

                CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
                codeMemberMethod.Name = dataLayerMethodName;                
                codeMemberMethod.Attributes = MemberAttributes.Public;

                List<CodeParameterDeclarationExpression> listOfParameterExpressions =
                    new List<CodeParameterDeclarationExpression>();

                Type connectionStringType = typeof(string);


                CodeParameterDeclarationExpression connectionStringExpression = GetParameterDeclarationExpression(connectionStringType,
    ClassCreationHelperConstants.VAR_CONNECTION_STRING_NAME);
                listOfParameterExpressions.Add(connectionStringExpression);

                predicateFunctions.AssemblyFullName = inputDtoFullName;

                Type typeToReflect = null;
                Object objAssembly = null;

                objAssembly = AssembliesGeneratedInMemory.Find(predicateFunctions.FindAssemblyLoadedInMemoryByFullAssemblyName);

                if (objAssembly != null)
                {
                    typeToReflect = objAssembly.GetType();
                }

                CodeParameterDeclarationExpression inputObjectExpression =
                    GetParameterDeclarationExpression(typeToReflect, ClassCreationHelperConstants.VAR_INPUT_OBJECT_NAME);

                listOfParameterExpressions.Add(inputObjectExpression);


                foreach (CodeParameterDeclarationExpression parameterExpression in listOfParameterExpressions)
                {
                    codeMemberMethod.Parameters.Add(parameterExpression);
                }

                CodeSnippetStatement sqlParametersExecuteSnippet =
                    new CodeSnippetStatement(GetSqlParametersNonGetCodeSnippetForMainSprocFromInput(typeToReflect, sprocFullName));

                codeMemberMethod.Statements.Add(sqlParametersExecuteSnippet);               
               
                targetClass.Members.Add(codeMemberMethod);
            }
        }


        public string GetFillAndReturnResultListSnippet(string dtoReturnListFullName)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB);
            //sb.Append(ClassCreationHelperConstants.LIST);
            //sb.Append(ClassCreationHelperConstants.CSHARP_OPEN_ANGLE_BRACKET);
            sb.Append(dtoReturnListFullName);
            //sb.Append(ClassCreationHelperConstants.CSHARP_CLOSE_ANGLE_BRACKET);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.RETURN_LIST);
            sb.Append(ClassCreationHelperConstants.SEMI_COLON);
            sb.Append(Environment.NewLine);

            sb.Append(ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB);
            sb.Append(ClassCreationHelperConstants.USING);
            sb.Append(ClassCreationHelperConstants.CONDITION_OPEN_BRACKET);
            sb.Append(ClassCreationHelperConstants.READER_VARIABLE_NAME);
            sb.Append(ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET);
            sb.Append(Environment.NewLine);
            sb.Append(ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB);
            sb.Append(ClassCreationHelperConstants.CSHARP_OPEN_BRACE);
            sb.Append(Environment.NewLine);
            sb.Append(ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB + 
                      ClassCreationHelperConstants.TAB);
            sb.Append(ClassCreationHelperConstants.RETURN_LIST);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.EQUALS);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.NEW);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(dtoReturnListFullName);
            sb.Append(ClassCreationHelperConstants.CONDITION_OPEN_BRACKET);
            sb.Append(ClassCreationHelperConstants.READER_VARIABLE_NAME);
            sb.Append(ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET);
            sb.Append(ClassCreationHelperConstants.SEMI_COLON);
            sb.Append(Environment.NewLine);
            sb.Append(ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB);
            sb.Append(ClassCreationHelperConstants.CSHARP_CLOSE_BRACE);
            sb.Append(Environment.NewLine);
            sb.Append(ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB);
            sb.Append(ClassCreationHelperConstants.RETURN_STATEMENT);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.RETURN_LIST);
            sb.Append(ClassCreationHelperConstants.SEMI_COLON);
            sb.Append(Environment.NewLine);

            return sb.ToString();

        }

        public string GetFillAndReturnDataSetSnippet()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB);
            sb.Append(ClassCreationHelperConstants.RETURN_STATEMENT);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.VAR_DATASET);
            sb.Append(ClassCreationHelperConstants.SEMI_COLON);
            sb.Append(Environment.NewLine);

            return sb.ToString();

        }


        public string GetSqlParametersCodeSnippetFromInput(Type typeToReflect, string sprocName)
        {
            PredicateFunctions predicateFunctions = new PredicateFunctions();

            Dictionary<PropertyInfo, string> propertyInfoToColumnName =
                new Dictionary<PropertyInfo, string>();

            StringBuilder sb = new StringBuilder();
            PropertyInfo[] propertyInfos = typeToReflect.GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                bool foundInputSprocParameter = false;
                string columnName = string.Empty;
                object[] attributes = propertyInfo.GetCustomAttributes(false);
                foreach (Attribute attribute in attributes)
                {
                    if (attribute is InputSprocParameterAttribute)
                    {
                        foundInputSprocParameter = true;
                    }
                    if (attribute is DatabaseColumnAttribute)
                    {
                        columnName = ((DatabaseColumnAttribute)attribute).DatabaseColumn;
                    }
                    
                }
                if (foundInputSprocParameter)
                {
                    propertyInfoToColumnName.Add(propertyInfo, columnName);

                }                
            }
            //if (propertyInfoToColumnName.Count > 0)
            //{
                sb.Append(ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB);
                sb.Append(ClassCreationHelperConstants.STRING);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.VAR_SPROC_NAME);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.EQUALS);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.QUOTE);
                sb.Append(sprocName);
                sb.Append(ClassCreationHelperConstants.QUOTE);
                sb.Append(ClassCreationHelperConstants.SEMI_COLON);
                sb.Append(Environment.NewLine);

                sb.Append(ClassCreationHelperConstants.TAB + 
                          ClassCreationHelperConstants.TAB + 
                          ClassCreationHelperConstants.TAB);
                sb.Append(ClassCreationHelperConstants.SQLPARAMETER);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_OPEN_BRACKET);
                sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_CLOSE_BRACKET);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER_ARRAY);
                sb.Append(ClassCreationHelperConstants.SEMI_COLON);
                sb.Append(Environment.NewLine);
                sb.Append(ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB);
                sb.Append(ClassCreationHelperConstants.SQLPARAMETER);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER);
                sb.Append(ClassCreationHelperConstants.SEMI_COLON);
                sb.Append(Environment.NewLine);
                
                List<StoredProcedureParameter> parameters = new List<StoredProcedureParameter>();

                CommonLibrary.Base.Database.BaseDataAccess<string> baseDataAccess 
                    = new CommonLibrary.Base.Database.BaseDataAccess<string>(_databaseSmoObjectsAndSettings);

               parameters = baseDataAccess.GetStoredProcedureInputParameters(sprocName);

               sb.Append(ClassCreationHelperConstants.TAB +
                         ClassCreationHelperConstants.TAB +
                         ClassCreationHelperConstants.TAB);
               sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER_ARRAY);
               sb.Append(ClassCreationHelperConstants.SPACE);
               sb.Append(ClassCreationHelperConstants.EQUALS);
               sb.Append(ClassCreationHelperConstants.SPACE);
               sb.Append(ClassCreationHelperConstants.NEW);
               sb.Append(ClassCreationHelperConstants.SPACE);
               sb.Append(ClassCreationHelperConstants.SQLPARAMETER);
               sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_OPEN_BRACKET);
               sb.Append(parameters.Count.ToString());
               sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_CLOSE_BRACKET);
               sb.Append(ClassCreationHelperConstants.SEMI_COLON);
               sb.Append(Environment.NewLine);
               
               int count = 0;

               foreach (StoredProcedureParameter parameter in parameters)
               {
                   sb.Append(ClassCreationHelperConstants.TAB +
                             ClassCreationHelperConstants.TAB +
                             ClassCreationHelperConstants.TAB);
                   sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER);
                   sb.Append(ClassCreationHelperConstants.SPACE);
                   sb.Append(ClassCreationHelperConstants.EQUALS);
                   sb.Append(ClassCreationHelperConstants.SPACE);
                   sb.Append(ClassCreationHelperConstants.NEW);
                   sb.Append(ClassCreationHelperConstants.SPACE);
                   sb.Append(ClassCreationHelperConstants.SQLPARAMETER);
                   sb.Append(ClassCreationHelperConstants.CONDITION_OPEN_BRACKET);
                   SqlDbType sqlDbType =
                       baseDataAccess.GetSqlDbTypeFromStoredProcedureParameterDataType(parameter.DataType);
                   sb.Append(ClassCreationHelperConstants.QUOTE);
                   sb.Append(parameter.Name);
                   sb.Append(ClassCreationHelperConstants.QUOTE);
                   sb.Append(ClassCreationHelperConstants.COMMA);
                   sb.Append(ClassCreationHelperConstants.SPACE);
                   sb.Append(ClassCreationHelperConstants.QUOTE);
                   sb.Append(sqlDbType.ToString());
                   sb.Append(ClassCreationHelperConstants.QUOTE);
                   sb.Append(ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET);
                   sb.Append(ClassCreationHelperConstants.SEMI_COLON);
                   sb.Append(Environment.NewLine);
                   sb.Append(ClassCreationHelperConstants.TAB +
                             ClassCreationHelperConstants.TAB +
                             ClassCreationHelperConstants.TAB);
                   sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER);
                   sb.Append(ClassCreationHelperConstants.DOT_OPERATOR);
                   sb.Append(ClassCreationHelperConstants.VALUE);
                   sb.Append(ClassCreationHelperConstants.SPACE);
                   sb.Append(ClassCreationHelperConstants.EQUALS);
                   sb.Append(ClassCreationHelperConstants.SPACE);
                   sb.Append(ClassCreationHelperConstants.GET_VALUE_FROM_INPUT_OBJECT_FOR_SPROC_PARAMETER);
                   sb.Append(ClassCreationHelperConstants.CONDITION_OPEN_BRACKET);
                   sb.Append(ClassCreationHelperConstants.QUOTE);
                   sb.Append(parameter.Name);
                   sb.Append(ClassCreationHelperConstants.QUOTE);
                   sb.Append(ClassCreationHelperConstants.COMMA);
                   sb.Append(ClassCreationHelperConstants.SPACE);
                   sb.Append(ClassCreationHelperConstants.VAR_INPUT_OBJECT_NAME);
                   sb.Append(ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET);
                   sb.Append(ClassCreationHelperConstants.SEMI_COLON);
                   sb.Append(Environment.NewLine);
                   sb.Append(ClassCreationHelperConstants.TAB +
                             ClassCreationHelperConstants.TAB +
                             ClassCreationHelperConstants.TAB);

                   sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER_ARRAY);
                   sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_OPEN_BRACKET);
                   sb.Append(count.ToString());
                   sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_CLOSE_BRACKET);
                   sb.Append(ClassCreationHelperConstants.SPACE);
                   sb.Append(ClassCreationHelperConstants.EQUALS);
                   sb.Append(ClassCreationHelperConstants.SPACE);
                   sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER);
                   sb.Append(ClassCreationHelperConstants.SEMI_COLON);
                   sb.Append(Environment.NewLine);
                   count++;
               }


                   sb.Append(ClassCreationHelperConstants.TAB +
                             ClassCreationHelperConstants.TAB +
                             ClassCreationHelperConstants.TAB);
                   sb.Append(ClassCreationHelperConstants.SQLDATAREADER);
                   sb.Append(ClassCreationHelperConstants.SPACE);
                   sb.Append(ClassCreationHelperConstants.READER_VARIABLE_NAME);
                   sb.Append(ClassCreationHelperConstants.SPACE);
                   sb.Append(ClassCreationHelperConstants.EQUALS);
                   sb.Append(ClassCreationHelperConstants.SPACE);
                  
                      sb.Append(ClassCreationHelperConstants.GET_DATAREADER_FROM_SP);
                      sb.Append(ClassCreationHelperConstants.CONDITION_OPEN_BRACKET);
                      sb.Append(ClassCreationHelperConstants.VAR_CONNECTION_STRING_NAME);
                      sb.Append(ClassCreationHelperConstants.COMMA);
                      sb.Append(ClassCreationHelperConstants.SPACE);
                      sb.Append(ClassCreationHelperConstants.VAR_SPROC_NAME);
                if (parameters.Count > 0)
                  {
                      sb.Append(ClassCreationHelperConstants.COMMA);
                      sb.Append(ClassCreationHelperConstants.SPACE);
                      sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER_ARRAY);
                   }
                      sb.Append(ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET);
                      sb.Append(ClassCreationHelperConstants.SEMI_COLON);
                      sb.Append(Environment.NewLine);                  

               //}
               return sb.ToString();

            }

        public string GetSqlParametersForNonGetCodeSnippetFromInput(Type typeToReflect, string sprocName)
        {
            PredicateFunctions predicateFunctions = new PredicateFunctions();

            Dictionary<PropertyInfo, string> propertyInfoToColumnName =
                new Dictionary<PropertyInfo, string>();

            StringBuilder sb = new StringBuilder();
            PropertyInfo[] propertyInfos = typeToReflect.GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                bool foundInputSprocParameter = false;
                string columnName = string.Empty;
                object[] attributes = propertyInfo.GetCustomAttributes(false);
                foreach (Attribute attribute in attributes)
                {
                    if (attribute is InputSprocParameterAttribute)
                    {
                        foundInputSprocParameter = true;
                    }
                    if (attribute is DatabaseColumnAttribute)
                    {
                        columnName = ((DatabaseColumnAttribute)attribute).DatabaseColumn;
                    }

                }
                if (foundInputSprocParameter)
                {
                    propertyInfoToColumnName.Add(propertyInfo, columnName);

                }
            }
            //if (propertyInfoToColumnName.Count > 0)
            //{
            sb.Append(ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB);
            sb.Append(ClassCreationHelperConstants.STRING);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.VAR_SPROC_NAME);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.EQUALS);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.QUOTE);
            sb.Append(sprocName);
            sb.Append(ClassCreationHelperConstants.QUOTE);
            sb.Append(ClassCreationHelperConstants.SEMI_COLON);
            sb.Append(Environment.NewLine);

            sb.Append(ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB);
            sb.Append(ClassCreationHelperConstants.SQLPARAMETER);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_OPEN_BRACKET);
            sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_CLOSE_BRACKET);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER_ARRAY);
            sb.Append(ClassCreationHelperConstants.SEMI_COLON);
            sb.Append(Environment.NewLine);
            sb.Append(ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB);
            sb.Append(ClassCreationHelperConstants.SQLPARAMETER);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER);
            sb.Append(ClassCreationHelperConstants.SEMI_COLON);
            sb.Append(Environment.NewLine);

            List<StoredProcedureParameter> parameters = new List<StoredProcedureParameter>();

            CommonLibrary.Base.Database.BaseDataAccess<string> baseDataAccess
                = new CommonLibrary.Base.Database.BaseDataAccess<string>(_databaseSmoObjectsAndSettings);

            parameters = baseDataAccess.GetStoredProcedureInputParameters(sprocName);

            sb.Append(ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB);
            sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER_ARRAY);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.EQUALS);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.NEW);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.SQLPARAMETER);
            sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_OPEN_BRACKET);
            sb.Append(parameters.Count.ToString());
            sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_CLOSE_BRACKET);
            sb.Append(ClassCreationHelperConstants.SEMI_COLON);
            sb.Append(Environment.NewLine);

            int count = 0;

            foreach (StoredProcedureParameter parameter in parameters)
            {
                sb.Append(ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB);
                sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.EQUALS);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.NEW);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.SQLPARAMETER);
                sb.Append(ClassCreationHelperConstants.CONDITION_OPEN_BRACKET);
                SqlDbType sqlDbType =
                    baseDataAccess.GetSqlDbTypeFromStoredProcedureParameterDataType(parameter.DataType);
                sb.Append(ClassCreationHelperConstants.QUOTE);
                sb.Append(parameter.Name);
                sb.Append(ClassCreationHelperConstants.QUOTE);
                sb.Append(ClassCreationHelperConstants.COMMA);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.QUOTE);
                sb.Append(sqlDbType.ToString());
                sb.Append(ClassCreationHelperConstants.QUOTE);
                sb.Append(ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET);
                sb.Append(ClassCreationHelperConstants.SEMI_COLON);
                sb.Append(Environment.NewLine);
                sb.Append(ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB);
                sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER);
                sb.Append(ClassCreationHelperConstants.DOT_OPERATOR);
                sb.Append(ClassCreationHelperConstants.VALUE);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.EQUALS);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.GET_VALUE_FROM_INPUT_OBJECT_FOR_SPROC_PARAMETER);
                sb.Append(ClassCreationHelperConstants.CONDITION_OPEN_BRACKET);
                sb.Append(ClassCreationHelperConstants.QUOTE);
                sb.Append(parameter.Name);
                sb.Append(ClassCreationHelperConstants.QUOTE);
                sb.Append(ClassCreationHelperConstants.COMMA);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.VAR_INPUT_OBJECT_NAME);
                sb.Append(ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET);
                sb.Append(ClassCreationHelperConstants.SEMI_COLON);
                sb.Append(Environment.NewLine);
                sb.Append(ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB);

                sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER_ARRAY);
                sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_OPEN_BRACKET);
                sb.Append(count.ToString());
                sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_CLOSE_BRACKET);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.EQUALS);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER);
                sb.Append(ClassCreationHelperConstants.SEMI_COLON);
                sb.Append(Environment.NewLine);
                count++;
            }


            sb.Append(ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB);
           

            sb.Append(ClassCreationHelperConstants.EXECUTE_NON_QUERY_STORED_PROCEDURE);
            sb.Append(ClassCreationHelperConstants.CONDITION_OPEN_BRACKET);
            sb.Append(ClassCreationHelperConstants.VAR_CONNECTION_STRING_NAME);
            sb.Append(ClassCreationHelperConstants.COMMA);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.VAR_SPROC_NAME);
            if (parameters.Count > 0)
            {
                sb.Append(ClassCreationHelperConstants.COMMA);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER_ARRAY);
            }
            sb.Append(ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET);
            sb.Append(ClassCreationHelperConstants.SEMI_COLON);
            sb.Append(Environment.NewLine);

            //}
            return sb.ToString();

        }

        public string GetSqlParametersNonGetCodeSnippetForMainSprocFromInput(Type typeToReflect, string sprocName)
        {
            PredicateFunctions predicateFunctions = new PredicateFunctions();

            Dictionary<PropertyInfo, string> propertyInfoToColumnName =
                new Dictionary<PropertyInfo, string>();

            StringBuilder sb = new StringBuilder();
            PropertyInfo[] propertyInfos = typeToReflect.GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                bool foundInputSprocParameter = false;
                string columnName = string.Empty;
                object[] attributes = propertyInfo.GetCustomAttributes(false);
                foreach (Attribute attribute in attributes)
                {
                    if (attribute is InputSprocParameterAttribute)
                    {
                        foundInputSprocParameter = true;
                    }
                    if (attribute is DatabaseColumnAttribute)
                    {
                        columnName = ((DatabaseColumnAttribute)attribute).DatabaseColumn;
                    }

                }
                if (foundInputSprocParameter)
                {
                    propertyInfoToColumnName.Add(propertyInfo, columnName);

                }
            }
            //if (propertyInfoToColumnName.Count > 0)
            //{
            sb.Append(ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB);
            sb.Append(ClassCreationHelperConstants.STRING);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.VAR_SPROC_NAME);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.EQUALS);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.QUOTE);
            sb.Append(sprocName);
            sb.Append(ClassCreationHelperConstants.QUOTE);
            sb.Append(ClassCreationHelperConstants.SEMI_COLON);
            sb.Append(Environment.NewLine);

            sb.Append(ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB);
            sb.Append(ClassCreationHelperConstants.SQLPARAMETER);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_OPEN_BRACKET);
            sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_CLOSE_BRACKET);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER_ARRAY);
            sb.Append(ClassCreationHelperConstants.SEMI_COLON);
            sb.Append(Environment.NewLine);
            sb.Append(ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB);
            sb.Append(ClassCreationHelperConstants.SQLPARAMETER);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER);
            sb.Append(ClassCreationHelperConstants.SEMI_COLON);
            sb.Append(Environment.NewLine);

            List<StoredProcedureParameter> parameters = new List<StoredProcedureParameter>();

            CommonLibrary.Base.Database.BaseDataAccess<string> baseDataAccess
                = new CommonLibrary.Base.Database.BaseDataAccess<string>(_databaseSmoObjectsAndSettings);

            parameters = baseDataAccess.GetStoredProcedureInputParameters(sprocName);

            sb.Append(ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB);
            sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER_ARRAY);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.EQUALS);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.NEW);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.SQLPARAMETER);
            sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_OPEN_BRACKET);
            sb.Append(parameters.Count.ToString());
            sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_CLOSE_BRACKET);
            sb.Append(ClassCreationHelperConstants.SEMI_COLON);
            sb.Append(Environment.NewLine);

            int count = 0;

            foreach (StoredProcedureParameter parameter in parameters)
            {
                sb.Append(ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB);
                sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.EQUALS);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.NEW);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.SQLPARAMETER);
                sb.Append(ClassCreationHelperConstants.CONDITION_OPEN_BRACKET);
                SqlDbType sqlDbType =
                    baseDataAccess.GetSqlDbTypeFromStoredProcedureParameterDataType(parameter.DataType);
                sb.Append(ClassCreationHelperConstants.QUOTE);
                sb.Append(parameter.Name);
                sb.Append(ClassCreationHelperConstants.QUOTE);
                sb.Append(ClassCreationHelperConstants.COMMA);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.QUOTE);
                sb.Append(sqlDbType.ToString());
                sb.Append(ClassCreationHelperConstants.QUOTE);
                sb.Append(ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET);
                sb.Append(ClassCreationHelperConstants.SEMI_COLON);
                sb.Append(Environment.NewLine);
                sb.Append(ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB);
                sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER);
                sb.Append(ClassCreationHelperConstants.DOT_OPERATOR);
                sb.Append(ClassCreationHelperConstants.VALUE);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.EQUALS);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.GET_VALUE_FROM_INPUT_OBJECT_FOR_SPROC_PARAMETER);
                sb.Append(ClassCreationHelperConstants.CONDITION_OPEN_BRACKET);
                sb.Append(ClassCreationHelperConstants.QUOTE);
                sb.Append(parameter.Name);
                sb.Append(ClassCreationHelperConstants.QUOTE);
                sb.Append(ClassCreationHelperConstants.COMMA);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.VAR_INPUT_OBJECT_NAME);
                sb.Append(ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET);
                sb.Append(ClassCreationHelperConstants.SEMI_COLON);
                sb.Append(Environment.NewLine);
                sb.Append(ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB);

                sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER_ARRAY);
                sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_OPEN_BRACKET);
                sb.Append(count.ToString());
                sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_CLOSE_BRACKET);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.EQUALS);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER);
                sb.Append(ClassCreationHelperConstants.SEMI_COLON);
                sb.Append(Environment.NewLine);
                count++;
            }


            sb.Append(ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB +
                      ClassCreationHelperConstants.TAB);
           

            sb.Append(ClassCreationHelperConstants.EXECUTE_NON_QUERY_STORED_PROCEDURE);
            sb.Append(ClassCreationHelperConstants.CONDITION_OPEN_BRACKET);
            sb.Append(ClassCreationHelperConstants.VAR_CONNECTION_STRING_NAME);
            sb.Append(ClassCreationHelperConstants.COMMA);
            sb.Append(ClassCreationHelperConstants.SPACE);
            sb.Append(ClassCreationHelperConstants.VAR_SPROC_NAME);
            if (parameters.Count > 0)
            {
                sb.Append(ClassCreationHelperConstants.COMMA);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER_ARRAY);
            }
            sb.Append(ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET);
            sb.Append(ClassCreationHelperConstants.SEMI_COLON);
            sb.Append(Environment.NewLine);

            //}
            return sb.ToString();

        }


        public string GetSqlParametersCodeSnippetForMainSprocFromInput(Type typeToReflect, string sprocName)
        {
            PredicateFunctions predicateFunctions = new PredicateFunctions();

            Dictionary<PropertyInfo, string> propertyInfoToColumnName =
                new Dictionary<PropertyInfo, string>();

            StringBuilder sb = new StringBuilder();
            PropertyInfo[] propertyInfos = typeToReflect.GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                bool foundInputSprocParameter = false;
                string columnName = string.Empty;
                object[] attributes = propertyInfo.GetCustomAttributes(false);
                foreach (Attribute attribute in attributes)
                {
                    if (attribute is InputSprocParameterAttribute)
                    {
                        foundInputSprocParameter = true;
                    }
                    if (attribute is DatabaseColumnAttribute)
                    {
                        columnName = ((DatabaseColumnAttribute)attribute).DatabaseColumn;
                    }

                }
                if (foundInputSprocParameter)
                {
                    propertyInfoToColumnName.Add(propertyInfo, columnName);

                }
            }
            //if (propertyInfoToColumnName.Count > 0)
            //{
                sb.Append(ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB);
                sb.Append(ClassCreationHelperConstants.STRING);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.VAR_SPROC_NAME);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.EQUALS);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.QUOTE);
                sb.Append(sprocName);
                sb.Append(ClassCreationHelperConstants.QUOTE);
                sb.Append(ClassCreationHelperConstants.SEMI_COLON);
                sb.Append(Environment.NewLine);

                sb.Append(ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB);
                sb.Append(ClassCreationHelperConstants.SQLPARAMETER);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_OPEN_BRACKET);
                sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_CLOSE_BRACKET);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER_ARRAY);
                sb.Append(ClassCreationHelperConstants.SEMI_COLON);
                sb.Append(Environment.NewLine);
                sb.Append(ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB);
                sb.Append(ClassCreationHelperConstants.SQLPARAMETER);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER);
                sb.Append(ClassCreationHelperConstants.SEMI_COLON);
                sb.Append(Environment.NewLine);

                List<StoredProcedureParameter> parameters = new List<StoredProcedureParameter>();

                CommonLibrary.Base.Database.BaseDataAccess<string> baseDataAccess
                    = new CommonLibrary.Base.Database.BaseDataAccess<string>(_databaseSmoObjectsAndSettings);

                parameters = baseDataAccess.GetStoredProcedureInputParameters(sprocName);

                sb.Append(ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB);
                sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER_ARRAY);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.EQUALS);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.NEW);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.SQLPARAMETER);
                sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_OPEN_BRACKET);
                sb.Append(parameters.Count.ToString());
                sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_CLOSE_BRACKET);
                sb.Append(ClassCreationHelperConstants.SEMI_COLON);
                sb.Append(Environment.NewLine);

                int count = 0;

                foreach (StoredProcedureParameter parameter in parameters)
                {
                    sb.Append(ClassCreationHelperConstants.TAB +
                              ClassCreationHelperConstants.TAB +
                              ClassCreationHelperConstants.TAB);
                    sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER);
                    sb.Append(ClassCreationHelperConstants.SPACE);
                    sb.Append(ClassCreationHelperConstants.EQUALS);
                    sb.Append(ClassCreationHelperConstants.SPACE);
                    sb.Append(ClassCreationHelperConstants.NEW);
                    sb.Append(ClassCreationHelperConstants.SPACE);
                    sb.Append(ClassCreationHelperConstants.SQLPARAMETER);
                    sb.Append(ClassCreationHelperConstants.CONDITION_OPEN_BRACKET);
                    SqlDbType sqlDbType =
                        baseDataAccess.GetSqlDbTypeFromStoredProcedureParameterDataType(parameter.DataType);
                    sb.Append(ClassCreationHelperConstants.QUOTE);
                    sb.Append(parameter.Name);
                    sb.Append(ClassCreationHelperConstants.QUOTE);
                    sb.Append(ClassCreationHelperConstants.COMMA);
                    sb.Append(ClassCreationHelperConstants.SPACE);
                    sb.Append(ClassCreationHelperConstants.QUOTE);
                    sb.Append(sqlDbType.ToString());
                    sb.Append(ClassCreationHelperConstants.QUOTE);
                    sb.Append(ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET);
                    sb.Append(ClassCreationHelperConstants.SEMI_COLON);
                    sb.Append(Environment.NewLine);
                    sb.Append(ClassCreationHelperConstants.TAB +
                              ClassCreationHelperConstants.TAB +
                              ClassCreationHelperConstants.TAB);
                    sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER);
                    sb.Append(ClassCreationHelperConstants.DOT_OPERATOR);
                    sb.Append(ClassCreationHelperConstants.VALUE);
                    sb.Append(ClassCreationHelperConstants.SPACE);
                    sb.Append(ClassCreationHelperConstants.EQUALS);
                    sb.Append(ClassCreationHelperConstants.SPACE);
                    sb.Append(ClassCreationHelperConstants.GET_VALUE_FROM_INPUT_OBJECT_FOR_SPROC_PARAMETER);
                    sb.Append(ClassCreationHelperConstants.CONDITION_OPEN_BRACKET);
                    sb.Append(ClassCreationHelperConstants.QUOTE);
                    sb.Append(parameter.Name);
                    sb.Append(ClassCreationHelperConstants.QUOTE);
                    sb.Append(ClassCreationHelperConstants.COMMA);
                    sb.Append(ClassCreationHelperConstants.SPACE);
                    sb.Append(ClassCreationHelperConstants.VAR_INPUT_OBJECT_NAME);
                    sb.Append(ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET);
                    sb.Append(ClassCreationHelperConstants.SEMI_COLON);
                    sb.Append(Environment.NewLine);
                    sb.Append(ClassCreationHelperConstants.TAB +
                              ClassCreationHelperConstants.TAB +
                              ClassCreationHelperConstants.TAB);

                    sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER_ARRAY);
                    sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_OPEN_BRACKET);
                    sb.Append(count.ToString());
                    sb.Append(ClassCreationHelperConstants.CSHARP_SQUARE_CLOSE_BRACKET);
                    sb.Append(ClassCreationHelperConstants.SPACE);
                    sb.Append(ClassCreationHelperConstants.EQUALS);
                    sb.Append(ClassCreationHelperConstants.SPACE);
                    sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER);
                    sb.Append(ClassCreationHelperConstants.SEMI_COLON);
                    sb.Append(Environment.NewLine);
                    count++;
                }


                sb.Append(ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB +
                          ClassCreationHelperConstants.TAB);
                sb.Append(ClassCreationHelperConstants.DATASET);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.VAR_DATASET);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.EQUALS);
                sb.Append(ClassCreationHelperConstants.SPACE);

                sb.Append(ClassCreationHelperConstants.GET_DATASET_FROM_SP);
                sb.Append(ClassCreationHelperConstants.CONDITION_OPEN_BRACKET);
                sb.Append(ClassCreationHelperConstants.VAR_CONNECTION_STRING_NAME);
                sb.Append(ClassCreationHelperConstants.COMMA);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.VAR_SPROC_NAME);
                if (parameters.Count > 0)
                {
                    sb.Append(ClassCreationHelperConstants.COMMA);
                    sb.Append(ClassCreationHelperConstants.SPACE);
                    sb.Append(ClassCreationHelperConstants.VAR_STORED_PROCEDURE_PARAMETER_ARRAY);
                }
                sb.Append(ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET);
                sb.Append(ClassCreationHelperConstants.SEMI_COLON);
                sb.Append(Environment.NewLine);

            //}
            return sb.ToString();

        }


        public Type GetTypeToReflect(string dtoName)
        {
            PredicateFunctions predicateFunctions = new PredicateFunctions();

            Type typeToReflect = null;
            string dtoFullName = dtoName;

            //this would mean that this file already exists and has been compiled into the app
            Assembly asm = Assembly.ReflectionOnlyLoad(_enclosingApplicationNamespace);
            //need to pre-load any dependant assemblies when loading reflectiononly
            Assembly.ReflectionOnlyLoad("CommonLibrary");           

            typeToReflect = asm.GetType(dtoFullName);


            if (typeToReflect == null)
            {
                //not compiled into the assembly yet,  not "included in the project"
                //but we may have this assembly already generated in memory.
                if (AssembliesGeneratedInMemory.Count > 0)
                {
                    predicateFunctions.AssemblyFullName = dtoFullName;

                    Object objAssembly = AssembliesGeneratedInMemory.Find(predicateFunctions.FindAssemblyLoadedInMemoryByFullAssemblyName);
                    if (objAssembly != null)
                    {
                        typeToReflect = objAssembly.GetType();
                    }

                }
            }

            
            return typeToReflect;
        }

       

        public Dictionary<string, List<KeyValuePair<string, List<string>>>> 
            GetMainSprocNameToKeySprocCalledToValueDtoListsUsed(
            MetaSprocSqlDependencyManager metaSprocSqlDependencyManager,
            Dictionary<string, List<string>> sprocToDtoListsUsed)
        {
            Dictionary<string, List<KeyValuePair<string, List<string>>>> mainSprocNameToSprocsItCallsSprocNameToListOfDtoListsUsed =
                new Dictionary<string, List<KeyValuePair<string, List<string>>>>();

            foreach (MetaSprocSqlDependency metaSprocSqlDependency in 
                 metaSprocSqlDependencyManager.MetaSprocSqlDependencyList)
             {
                 //basically this asks if this sproc calls other sprocs and if so, then we want to 
                 //find the sproc in the sprocToDtosUsed dictionary, and then add an item to 
                 //mainSprocNameToSprocsItCallsSprocNameToListOfDtoListsUsed with the main sprocname
                 //and the dictionary of sprocName to DtoListsUsed.
                 if (metaSprocSqlDependency.SprocDependencies.Count > 0)
                 {
                     foreach (Data.MetaSqlDependency metaSqlDependency in metaSprocSqlDependency.SprocDependencies)
                     {
                         if (metaSqlDependency.ReferencingObject == metaSprocSqlDependency.MainStoredProcedure)
                         {
                             List<string> value = null;
                             sprocToDtoListsUsed.TryGetValue(metaSqlDependency.ReferencedObject, out value);
                             if (value != null)
                             {
                                 if (mainSprocNameToSprocsItCallsSprocNameToListOfDtoListsUsed.ContainsKey(metaSprocSqlDependency.MainStoredProcedure))
                                 {
                                     mainSprocNameToSprocsItCallsSprocNameToListOfDtoListsUsed[metaSprocSqlDependency.MainStoredProcedure].Add(
                                         new KeyValuePair<string, List<string>>(metaSqlDependency.ReferencedObject,
                                                                           value));
                                 }
                                 else
                                 {
                                     List<KeyValuePair<string, List<string>>> kvp;
                                     kvp = new List<KeyValuePair<string, List<string>>>();
                                     kvp.Add(new KeyValuePair<string, List<string>>(metaSqlDependency.ReferencedObject,
                                                                               value));
                                     mainSprocNameToSprocsItCallsSprocNameToListOfDtoListsUsed.Add(metaSprocSqlDependency.MainStoredProcedure,kvp );
                                 }
                                    
                             }
                         }
                     }
                 }
             }
             return mainSprocNameToSprocsItCallsSprocNameToListOfDtoListsUsed;
                
        }

        public Dictionary<string, List<string>> GenerateResultListsForSprocs(Dictionary<string, List<string>> sprocToDtosUsed,
                                                 MetaSprocSqlDependencyManager metaSprocSqlDependencyManager,
                                                    List<StoredProcedure> sprocsToGenerate)
        {
            PredicateFunctions predicateFunctions = new PredicateFunctions();
            Dictionary<string, List<string>> sprocsToListsUsed = new Dictionary<string, List<string>>();
            foreach (KeyValuePair<string, List<string>> kvpSprocToDtosUsed in sprocToDtosUsed)
            {
                predicateFunctions.SprocNameHolder = kvpSprocToDtosUsed.Key;
                StoredProcedure sproc = 
                    sprocsToGenerate.Find(predicateFunctions.FindSprocGeneratedBySprocName);
                if (sproc == null)
                {
                    sproc = _databaseSmoObjectsAndSettings.Database_Property.StoredProcedures[predicateFunctions.SprocNameHolder];
                }

                //parse to find Insert, Update, or Delete and if so, then return null with the sproc
                //for the lists that it uses - we will be treating these as insert update or delete
                if (InsertUpdateOrDeleteFound(sproc.TextBody))
                {
                    sprocsToListsUsed.Add(kvpSprocToDtosUsed.Key, null);
                }
                else
                {

                    KeyValuePair<string, List<string>> sprocToListsUsed =
                        GenerateResultListsForSproc(kvpSprocToDtosUsed.Key, kvpSprocToDtosUsed.Value, metaSprocSqlDependencyManager);

                    sprocsToListsUsed.Add(sprocToListsUsed.Key, sprocToListsUsed.Value);
                }
            }            
            return sprocsToListsUsed;
        }

        public bool IsColumnNameFoundInSprocTextBody(string textBody, string columnName)
        {
            bool found = false;
            string lowerCaseTextBody = textBody.ToLower();
            string lowerCaseColumnName = columnName.ToLower();
            if (lowerCaseTextBody.IndexOf(lowerCaseColumnName + ",") > -1)
            {
                found = true;
            }
            else
                if (lowerCaseTextBody.IndexOf(" " + lowerCaseColumnName) > -1)
                {
                    found = true;
                }
                else
                    if (lowerCaseTextBody.IndexOf(lowerCaseColumnName) > -1)
                    {
                        found = true;
                    }

            if (!found)
            {
                lowerCaseColumnName = "[" + lowerCaseColumnName + "]";
                if (lowerCaseTextBody.IndexOf(lowerCaseColumnName + ",") > -1)
                {
                    found = true;
                }
                else
                    if (lowerCaseTextBody.IndexOf(" " + lowerCaseColumnName) > -1)
                    {
                        found = true;
                    }
                    else
                        if (lowerCaseTextBody.IndexOf(lowerCaseColumnName) > -1)
                        {
                            found = true;
                        }

            }
            return found;
        }

        public bool InsertUpdateOrDeleteFound(string textBody)
        {
            bool found = false;
            string lowerCaseTextBody = textBody.ToLower();
            if (lowerCaseTextBody.IndexOf("delete ") > -1)
            {
                found = true;
            }
            else
                if (lowerCaseTextBody.IndexOf("insert ") > -1)
                {
                    found = true;
                }
                else
                    if (lowerCaseTextBody.IndexOf("update ") > -1)
                    {
                        found = true;
                    }

            return found;
            
        }

        public KeyValuePair<string, List<string>> GenerateResultListsForSproc(string sprocName,
                                           List<string> dtosUsedBySproc,
                                           MetaSprocSqlDependencyManager metaSprocSqlDependencyManager)
        {
            string outputFilePath = OUTPUT_PATH_LIST;
            string outputFileName = ClassCreationHelperMethods.GetResultListForCustomSprocFileName(sprocName);
            string resultListNamespace = ClassCreationHelperMethods.GetResultListForCustomSprocNamespace(_enclosingApplicationNamespace);
            string resultListClassName = ClassCreationHelperMethods.GetListClassName(sprocName);
            List<string> resultListNamespaces = this.GetResultNamespaceList(_enclosingApplicationNamespace);
            string outputFileAndPath = outputFilePath + outputFileName;

            bool overwriteExisting = true;

            KeyValuePair<string, List<string>> sprocToListsUsed;
            List<string> listsUsedBySproc = new List<string>();
            foreach(string dtoUsedBySproc in dtosUsedBySproc)
            {               
                string listUsedBySproc = GenerateResultList(sprocName,
                                  dtoUsedBySproc,
                                  resultListNamespace,
                                  resultListNamespaces,
                                  resultListClassName,
                                  outputFileAndPath,
                                  overwriteExisting);
                if (!listsUsedBySproc.Contains(listUsedBySproc))
                {
                    listsUsedBySproc.Add(listUsedBySproc);
                }
            }
            sprocToListsUsed = new KeyValuePair<string, List<string>>(sprocName, listsUsedBySproc);
            return sprocToListsUsed;
        }

        public string GenerateResultList(string sprocName,
                                       string dtoUsedBySprocFullName,
                                       string listNamespace,
                                       List<string> listNamespaces,
                                       string listClassName,
                                      //MetaInformationSchema metaInformationSchema,
                                       string outputFileAndPath,
                                       bool overwriteExisting)
        {
            
            CodeCompileUnit targetUnit = new CodeCompileUnit();
            CodeNamespace list = new CodeNamespace(listNamespace);
            foreach (string strNamespace in listNamespaces)
            {
                list.Imports.Add(new CodeNamespaceImport(strNamespace));
            }
            CodeTypeDeclaration targetClass = new CodeTypeDeclaration(listClassName);

            targetClass.IsClass = true;
            targetClass.TypeAttributes = TypeAttributes.Public;

            string dtoFullName = dtoUsedBySprocFullName;

            CodeTypeReference listTypeReference = 
                new CodeTypeReference("List", new CodeTypeReference[] { new CodeTypeReference(dtoFullName) });
            targetClass.BaseTypes.Add(listTypeReference);
            Type type = typeof(CommonLibrary.Base.Database.BaseDatabase);
            AddPrivateMember(targetClass, GetPrivateMemberName(type.Name), type);

            List<CodeParameterDeclarationExpression> listOfParameterExpressions =
                new List<CodeParameterDeclarationExpression>();

            Type dataReader = typeof(System.Data.SqlClient.SqlDataReader);

            CodeParameterDeclarationExpression sqlDataReaderExpression =
                GetParameterDeclarationExpression(dataReader, ClassCreationHelperConstants.READER);

            listOfParameterExpressions.Add(sqlDataReaderExpression);
            CodeConstructor constructor = new CodeConstructor();
            InitializeConstructor(constructor,
                                 MemberAttributes.Public,
                                 listOfParameterExpressions);

            CodeVariableReferenceExpression var =
                new CodeVariableReferenceExpression(ClassCreationHelperConstants.READER);

            CodeMethodInvokeExpression methodInvoke = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(),
                ClassCreationHelperConstants.ADD_ITEMS_TO_LIST_BY_READER, var);

            constructor.Statements.Add(methodInvoke);
            targetClass.Members.Add(constructor);

            AddEmptyConstructor(MemberAttributes.Public, targetClass);

            GenerateAndAddAddItemsToResultListBySqlDataReaderMethod(targetClass,
                                                             dtoFullName,
                                                              GetPrivateMemberName(listClassName),
                                                              sprocName);
            //AddTableNameAttributeToCodeTypeDeclaration(targetClass, metaInformationSchema);


            list.Types.Add(targetClass);
            targetUnit.Namespaces.Add(list);
            GenerateCSharpCode(outputFileAndPath, targetUnit, overwriteExisting);
            return listNamespace + ClassCreationHelperConstants.DOT_OPERATOR + listClassName;
        }

        public void GenerateAndAddAddItemsToResultListBySqlDataReaderMethod(CodeTypeDeclaration targetClass,
                                                              string dtoFullName,
                                                              string dtoVarName,
                                                              string sprocName
                                                              //MetaInformationSchema metaInformationSchema
            )
        {
            CodeMemberMethod addAddItemsToListBySqlDataReaderMethod = new CodeMemberMethod();
            addAddItemsToListBySqlDataReaderMethod.Attributes =
                MemberAttributes.Public;
            addAddItemsToListBySqlDataReaderMethod.Name = ClassCreationHelperConstants.ADD_ITEMS_TO_LIST_BY_READER;

            Type dataReader = typeof(System.Data.SqlClient.SqlDataReader);

            CodeParameterDeclarationExpression sqlDataReaderExpression =
                 GetParameterDeclarationExpression(dataReader, ClassCreationHelperConstants.READER);


            CodeVariableDeclarationStatement dtoVariableDeclarationStatement =
                new CodeVariableDeclarationStatement(new CodeTypeReference(dtoFullName), "dto", null);

            CodeVariableReferenceExpression dtoVariableReferenceExpression =
             new CodeVariableReferenceExpression("dto");

            CodeTypeReference dtoType = new CodeTypeReference(dtoFullName);

            CodeSnippetStatement tabFiller = new CodeSnippetStatement(ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB);


            CodeAssignStatement dtoObjectNewStatement = new CodeAssignStatement(dtoVariableReferenceExpression,
                new CodeObjectCreateExpression(dtoType));

            addAddItemsToListBySqlDataReaderMethod.Statements.Add(tabFiller);

            addAddItemsToListBySqlDataReaderMethod.Statements.Add(dtoVariableDeclarationStatement);
            addAddItemsToListBySqlDataReaderMethod.Parameters.Add(sqlDataReaderExpression);
            
            //the reason that we do not use the using here is because the sql datareader
            //can have multiple result sets and so we will use the using block in the datalayer
            //methods created instead so that we can read through the multiple sets, due to us
            //using the CommandBehaviour.CloseConnection which will be closed when the sqldatareader
            //is disposed of.

            //CodeSnippetStatement literalUsingBeginStatement =
            //    new CodeSnippetStatement(ClassCreationHelperConstants.TAB +
            //    ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB +
            //    ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.USING +
            //    ClassCreationHelperConstants.CONDITION_OPEN_BRACKET +
            //    sqlDataReaderExpression.Name + ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET +
            //    Environment.NewLine + ClassCreationHelperConstants.TAB +
            //    ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB +
            //    ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.CSHARP_OPEN_BRACE +
            //    Environment.NewLine);

            CodeSnippetStatement literalWhileBeginStatement =
                new CodeSnippetStatement(ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + @"while(reader.Read())"
                                         + Environment.NewLine + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + @"{");

            //addAddItemsToListBySqlDataReaderMethod.Statements.Add(literalUsingBeginStatement);
            addAddItemsToListBySqlDataReaderMethod.Statements.Add(literalWhileBeginStatement);
            addAddItemsToListBySqlDataReaderMethod.Statements.Add(dtoObjectNewStatement);

            CodeSnippetStatement literalDtoFillStatement =
                new CodeSnippetStatement(GetResultDtoAssignmentFromSqlDataReaderStatement(dtoFullName,
                                                                                    //metaInformationSchema,
                                                                                    dtoVariableDeclarationStatement,
                                                                                    sprocName));

            addAddItemsToListBySqlDataReaderMethod.Statements.Add(literalDtoFillStatement);

            CodeSnippetStatement literalWhileEndStatement =
                new CodeSnippetStatement(ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + @"}");


            addAddItemsToListBySqlDataReaderMethod.Statements.Add(literalWhileEndStatement);

            //CodeSnippetStatement literalUsingEndStatement =
            //    new CodeSnippetStatement(Environment.NewLine + ClassCreationHelperConstants.TAB +
            //    ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB +
            //    ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.CSHARP_CLOSE_BRACE +
            //                             Environment.NewLine);
            //addAddItemsToListBySqlDataReaderMethod.Statements.Add(literalUsingEndStatement);

            targetClass.Members.Add(addAddItemsToListBySqlDataReaderMethod);

        }

        public string GetResultDtoAssignmentFromSqlDataReaderStatement(string dtoFullName,
                                                       //MetaInformationSchema metaInformationSchema,
                                                        CodeVariableDeclarationStatement varDeclaration,
                                                        string sprocName)
        {
            string dtoFill = string.Empty;
            PredicateFunctions predicateFunctions = new PredicateFunctions();
            StoredProcedure sproc = _databaseSmoObjectsAndSettings.Database_Property.StoredProcedures[sprocName];

            Dictionary<string, string> propertyNameKeyDatabaseColumnValue = new Dictionary<string, string>();

            Dictionary<Data.InformationSchemaColumn, string> columnsFoundAndTheirPropertyName =
                new Dictionary<SprocDataLayerGenerator.Data.InformationSchemaColumn, string>();

            if (AssembliesGeneratedInMemory.Count > 0)
            {
                predicateFunctions.AssemblyFullName = dtoFullName;
                Type typeToReflect = null;
                Object objAssembly = AssembliesGeneratedInMemory.Find(predicateFunctions.FindAssemblyLoadedInMemoryByFullAssemblyName);
                if (objAssembly != null)
                {
                    typeToReflect = objAssembly.GetType();

                    PropertyInfo[] properties = typeToReflect.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        object[] attributes = property.GetCustomAttributes(true);

                        if (attributes.Length != 0)
                        {
                            DatabaseColumnAttribute databaseColumnAttribute = null;
                            SelectAttribute selectAttribute = null;
                            foreach (object attribute in attributes)
                            {
                               
                                if (attribute is SelectAttribute)
                                {
                                    SelectAttribute oneSelectAttribute =
                                        (SelectAttribute)attribute;
                                    if (oneSelectAttribute.SprocName == sprocName)
                                    {
                                        selectAttribute = oneSelectAttribute;
                                    }
                                }
                                if (attribute is DatabaseColumnAttribute)
                                {
                                    databaseColumnAttribute =
                                        (DatabaseColumnAttribute)attribute;                                    
                                }
                            }
                            if (selectAttribute != null && databaseColumnAttribute != null)
                            {

                                propertyNameKeyDatabaseColumnValue.Add(property.Name,
                                                                           databaseColumnAttribute.DatabaseColumn);

                            }

                        }
                    }

                    if (propertyNameKeyDatabaseColumnValue.Count > 0)
                    {
                        MetaInformationSchemaManager metaInformationSchemaManager
                            = new MetaInformationSchemaManager(_databaseSmoObjectsAndSettings.DatabaseName,
                                                               _databaseSmoObjectsAndSettings.DataSource,
                                                               _databaseSmoObjectsAndSettings.InitialCatalog,
                                                               _databaseSmoObjectsAndSettings.UserId,
                                                               _databaseSmoObjectsAndSettings.Password,
                                                               _databaseSmoObjectsAndSettings.TrustedConnection,
                                                               _databaseSmoObjectsAndSettings.Schema);

                        foreach (MetaInformationSchema metaInformationSchema in metaInformationSchemaManager.MetaDataList)
                        {
                            Dictionary<Data.InformationSchemaColumn, string> oneSetOfColumnsFoundAndTheirPropertyName =
                            new Dictionary<SprocDataLayerGenerator.Data.InformationSchemaColumn, string>();

                            oneSetOfColumnsFoundAndTheirPropertyName = 
                                GetColumnToPropertyName(metaInformationSchema.MetaColumns,
                                                        propertyNameKeyDatabaseColumnValue,sproc);

                            if (oneSetOfColumnsFoundAndTheirPropertyName.Count > 0)
                            {
                                foreach (KeyValuePair<Data.InformationSchemaColumn, string> kvp in oneSetOfColumnsFoundAndTheirPropertyName)
                                {
                                    if (IsColumnNameFoundInSprocTextBody(sproc.TextBody, kvp.Key.ColumnName))
                                    {
                                        columnsFoundAndTheirPropertyName.Add(kvp.Key, kvp.Value);
                                    }
                                }
                            }
                        }                      

                    }
                }                

                dtoFill = GetDtoAssignmentFromSqlDataReaderStatement(columnsFoundAndTheirPropertyName,
                                                                     typeToReflect, varDeclaration);
            }

            return dtoFill;
        }

       
       

        public Dictionary<string,List<string>> GenerateCustomSprocDtos(List<StoredProcedure> customSprocsToGenerate)
        {
            MetaSprocSqlDependencyManager metaSprocSqlDependencyManager =
                GetMetaSprocSqlDependencyManager(customSprocsToGenerate);

            MetaInformationSchemaManager metaInformationSchemaManager =
                GetMetaInformationSchemaManager(_databaseSmoObjectsAndSettings);

            Dictionary<string, List<string>> sprocToDtosUsed = GenerateCustomSprocDtos(metaSprocSqlDependencyManager,
                                    metaInformationSchemaManager);
            return sprocToDtosUsed;
        }

        public Dictionary<string, List<string>> GenerateInputCustomSprocDtos(List<StoredProcedure> customSprocsToGenerate)
        {
            MetaSprocSqlDependencyManager metaSprocSqlDependencyManager =
                GetMetaSprocSqlDependencyManager(customSprocsToGenerate);

            MetaInformationSchemaManager metaInformationSchemaManager =
                GetMetaInformationSchemaManager(_databaseSmoObjectsAndSettings);

            Dictionary<string, List<string>> sprocToDtosUsed = GenerateInputCustomSprocDtos(metaSprocSqlDependencyManager,
                                    metaInformationSchemaManager, customSprocsToGenerate);
            return sprocToDtosUsed;
        }

        public Dictionary<string, List<string>> GenerateCustomSprocDtos(MetaSprocSqlDependencyManager metaSprocSqlDependencyManager,
                                            MetaInformationSchemaManager metaInformationSchemaManager)
        {
            Dictionary<string, List<string>> cumulativeSprocToDtosUsed = new Dictionary<string, List<string>>();

            foreach (MetaSprocSqlDependency metaSprocSqlDependency 
                in metaSprocSqlDependencyManager.MetaSprocSqlDependencyList)
            {
                Dictionary<string,List<string>> sprocToDtosUsed = GenerateCustomSprocDtosForOneMetaSprocSqlDependency(metaSprocSqlDependency,
                    metaSprocSqlDependencyManager.MetaSprocSqlDependencyList,
                    metaInformationSchemaManager);
                foreach (KeyValuePair<string, List<string>> kvp in sprocToDtosUsed)
                {
                    if (!cumulativeSprocToDtosUsed.ContainsKey(kvp.Key))
                    {
                        cumulativeSprocToDtosUsed.Add(kvp.Key, kvp.Value);
                    }
                    else
                    {
                        List<string> currentValues = new List<string>();
                        cumulativeSprocToDtosUsed.TryGetValue(kvp.Key, out currentValues);
                        List<string> newValues = kvp.Value;
                        List<string> allValues = new List<string>();
                        allValues.AddRange(newValues);
                        foreach (string currentValue in currentValues)
                        {
                            if (!newValues.Contains(currentValue))
                            {
                                allValues.Add(currentValue);
                            }
                            
                        }
                        cumulativeSprocToDtosUsed[kvp.Key] = allValues;
                    }
                }

            }
            return cumulativeSprocToDtosUsed;
        }

        public Dictionary<string, List<string>> GenerateInputCustomSprocDtos(MetaSprocSqlDependencyManager metaSprocSqlDependencyManager,
                                    MetaInformationSchemaManager metaInformationSchemaManager, List<StoredProcedure> customSprocsToGenerate)
        {
            Dictionary<string, List<string>> cumulativeSprocToDtosUsed = new Dictionary<string, List<string>>();

            foreach (MetaSprocSqlDependency metaSprocSqlDependency
                in metaSprocSqlDependencyManager.MetaSprocSqlDependencyList)
            {
                Dictionary<string, List<string>> sprocToDtosUsed = GenerateInputCustomSprocDtosForOneMetaSprocSqlDependency(metaSprocSqlDependency,
                    metaSprocSqlDependencyManager.MetaSprocSqlDependencyList,
                    metaInformationSchemaManager, customSprocsToGenerate);
                foreach (KeyValuePair<string, List<string>> kvp in sprocToDtosUsed)
                {
                    if (!cumulativeSprocToDtosUsed.ContainsKey(kvp.Key))
                    {
                        cumulativeSprocToDtosUsed.Add(kvp.Key, kvp.Value);
                    }
                    else
                    {
                        List<string> currentValues = new List<string>();
                        cumulativeSprocToDtosUsed.TryGetValue(kvp.Key, out currentValues);
                        List<string> newValues = kvp.Value;
                        List<string> allValues = new List<string>();
                        allValues.AddRange(newValues);
                        foreach (string currentValue in currentValues)
                        {
                            if (!newValues.Contains(currentValue))
                            {
                                allValues.Add(currentValue);
                            }

                        }
                        cumulativeSprocToDtosUsed[kvp.Key] = allValues;
                    }
                }

            }
            return cumulativeSprocToDtosUsed;
        }

        public Dictionary<string,List<string>> GenerateInputCustomSprocDtosForOneMetaSprocSqlDependency(MetaSprocSqlDependency oneMetaSprocSqlDependency,
                                                                        List<MetaSprocSqlDependency> allSprocSqlDependencies,
                                                                        MetaInformationSchemaManager metaInformationSchemaManager,
                                                                        List<StoredProcedure> customSprocsToGenerate)
        {
            Dictionary<string, List<string>> sprocToDtosUsed = new Dictionary<string, List<string>>();
            PredicateFunctions predicateFunctions = new PredicateFunctions();


            if (oneMetaSprocSqlDependency.SprocDependencies.Count > 0)
            {
                foreach (Data.MetaSqlDependency sprocTypeDependency in oneMetaSprocSqlDependency.SprocDependencies)
                {
                    string sprocName = sprocTypeDependency.ReferencedObject;
                    predicateFunctions.SprocNameHolder = sprocName;
                    MetaSprocSqlDependency referencedMetaSprocSqlDependency =
                        allSprocSqlDependencies.Find(predicateFunctions.FindMetaSqlSprocBySprocName);
                    
                    if (referencedMetaSprocSqlDependency != null)
                    {
                        if (referencedMetaSprocSqlDependency.SprocDependencies.Count > 0)
                        {
                            //there are nested sproc dependencies requires recursion
                            sprocToDtosUsed = GenerateInputCustomSprocDtosForOneMetaSprocSqlDependency(referencedMetaSprocSqlDependency,
                                allSprocSqlDependencies,
                                metaInformationSchemaManager,
                                customSprocsToGenerate);
                        }
                        else
                        {
                            List<string> dtosUsedList = new List<string>();
                            foreach(KeyValuePair<string,List<Data.MetaSqlDependency>> kvp in 
                                referencedMetaSprocSqlDependency.TableDependencyToColumnsReferenced)
                            {
                                predicateFunctions.TableNameHolder = kvp.Key;
                                MetaInformationSchema metaInformationSchema =
                                    metaInformationSchemaManager.MetaDataList.Find(predicateFunctions.FindMetaInformationSchemaByTableName);

                                string dtoUsed = GenerateInputDtoForCustomSproc(metaInformationSchema,
                                                          sprocName,
                                                          kvp.Key,
                                                          kvp.Value, customSprocsToGenerate);
                                if (!dtosUsedList.Contains(dtoUsed))
                                {
                                    dtosUsedList.Add(dtoUsed);
                                }
                            }
                            if (dtosUsedList.Count > 0)
                            {
                                sprocToDtosUsed.Add(sprocName, dtosUsedList);
                            }
                        }
                    }

                    //around this point we should have the dto types that are created/updated with stuff through the loop
                    // and we will also have a dictionary of sprocName to list of types (the dtos) that the sproc uses
                    //and what we want to do is build the data accessor methods? that brings them all together
                    //we could also fill a dictionary that would be sprocName to dataAccessor method name
                }
                //at this point we should have all of the dto types that are involved in all "nested sprocs"
                //and what we want to do is build the data accessor methods? that brings them all together which would be
                //for the "oneMetaSprocSqlDependency" which would be built by the dictionary that contains
                //sprocName to dataAccessorMethodName.
            }

           
            //foreach (KeyValuePair<string, List<Data.MetaSqlDependency>> kvp in
            //                    oneMetaSprocSqlDependency.TableDependencyToColumnsReferenced)
            //{
            //    List<string> dtoReturned = new List<string>();
            //    string topSprocName = string.Empty;
            //    topSprocName = oneMetaSprocSqlDependency.MainStoredProcedure;
            //    predicateFunctions.TableNameHolder = kvp.Key;
            //    MetaInformationSchema metaInformationSchema =
            //        metaInformationSchemaManager.MetaDataList.Find(predicateFunctions.FindMetaInformationSchemaByTableName);
            //   dtoReturned.Add(GenerateDtoForCustomSproc(metaInformationSchema,
            //                              topSprocName,
            //                              kvp.Key,
            //                              kvp.Value));
            //   if (!sprocToDtosUsed.ContainsKey(topSprocName))
            //   {
            //       sprocToDtosUsed.Add(topSprocName, dtoReturned);
            //   }
               
            //}
            
          
            return sprocToDtosUsed;
        }

        public Dictionary<string, List<string>> GenerateCustomSprocDtosForOneMetaSprocSqlDependency(MetaSprocSqlDependency oneMetaSprocSqlDependency,
                                                                List<MetaSprocSqlDependency> allSprocSqlDependencies,
                                                                MetaInformationSchemaManager metaInformationSchemaManager)
        {
            Dictionary<string, List<string>> sprocToDtosUsed = new Dictionary<string, List<string>>();
            PredicateFunctions predicateFunctions = new PredicateFunctions();


            if (oneMetaSprocSqlDependency.SprocDependencies.Count > 0)
            {
                foreach (Data.MetaSqlDependency sprocTypeDependency in oneMetaSprocSqlDependency.SprocDependencies)
                {
                    string sprocName = sprocTypeDependency.ReferencedObject;
                    predicateFunctions.SprocNameHolder = sprocName;
                    MetaSprocSqlDependency referencedMetaSprocSqlDependency =
                        allSprocSqlDependencies.Find(predicateFunctions.FindMetaSqlSprocBySprocName);

                    if (referencedMetaSprocSqlDependency != null)
                    {
                        if (referencedMetaSprocSqlDependency.SprocDependencies.Count > 0)
                        {
                            //there are nested sproc dependencies requires recursion
                            sprocToDtosUsed = GenerateCustomSprocDtosForOneMetaSprocSqlDependency(referencedMetaSprocSqlDependency,
                                allSprocSqlDependencies,
                                metaInformationSchemaManager);
                        }
                        else
                        {
                            List<string> dtosUsedList = new List<string>();
                            foreach (KeyValuePair<string, List<Data.MetaSqlDependency>> kvp in
                                referencedMetaSprocSqlDependency.TableDependencyToColumnsReferenced)
                            {
                                predicateFunctions.TableNameHolder = kvp.Key;
                                MetaInformationSchema metaInformationSchema =
                                    metaInformationSchemaManager.MetaDataList.Find(predicateFunctions.FindMetaInformationSchemaByTableName);

                                string dtoUsed = GenerateDtoForCustomSproc(metaInformationSchema,
                                                          sprocName,
                                                          kvp.Key,
                                                          kvp.Value);
                                if (!dtosUsedList.Contains(dtoUsed))
                                {
                                    dtosUsedList.Add(dtoUsed);
                                }
                            }
                            if (dtosUsedList.Count > 0)
                            {
                                sprocToDtosUsed.Add(sprocName, dtosUsedList);
                            }
                        }
                    }

                    //around this point we should have the dto types that are created/updated with stuff through the loop
                    // and we will also have a dictionary of sprocName to list of types (the dtos) that the sproc uses
                    //and what we want to do is build the data accessor methods? that brings them all together
                    //we could also fill a dictionary that would be sprocName to dataAccessor method name
                }
                //at this point we should have all of the dto types that are involved in all "nested sprocs"
                //and what we want to do is build the data accessor methods? that brings them all together which would be
                //for the "oneMetaSprocSqlDependency" which would be built by the dictionary that contains
                //sprocName to dataAccessorMethodName.
            }


            foreach (KeyValuePair<string, List<Data.MetaSqlDependency>> kvp in
                                oneMetaSprocSqlDependency.TableDependencyToColumnsReferenced)
            {
                List<string> dtoReturned = new List<string>();
                string topSprocName = string.Empty;
                topSprocName = oneMetaSprocSqlDependency.MainStoredProcedure;
                predicateFunctions.TableNameHolder = kvp.Key;
                MetaInformationSchema metaInformationSchema =
                    metaInformationSchemaManager.MetaDataList.Find(predicateFunctions.FindMetaInformationSchemaByTableName);
                dtoReturned.Add(GenerateDtoForCustomSproc(metaInformationSchema,
                                           topSprocName,
                                           kvp.Key,
                                           kvp.Value));
                if (!sprocToDtosUsed.ContainsKey(topSprocName))
                {
                    sprocToDtosUsed.Add(topSprocName, dtoReturned);
                }
                else
                {
                    if (sprocToDtosUsed[topSprocName].Count > 0)
                    {
                        List<string> dtosReturned;
                        sprocToDtosUsed.TryGetValue(topSprocName, out dtosReturned);
                        if (dtosReturned != null)
                        {
                            dtosReturned.AddRange(dtoReturned);
                            //sprocToDtosUsed[topSprocName].Clear();
                            sprocToDtosUsed[topSprocName] = dtosReturned;
                        }
                    }
                }

            }


            return sprocToDtosUsed;
        }

       
        public string GenerateDtoForCustomSproc(MetaInformationSchema metaInformationSchema,
                                              string sprocName,
                                              string tableName,
                                              List<Data.MetaSqlDependency> columnsReferenced)
        {             
            
            string outputFilePath = OUTPUT_PATH_DTO;
            string outputFileName = ClassCreationHelperMethods.GetDtoForCustomSprocFileName(metaInformationSchema.MetaTable.TableName);
            string dtoNamespace = ClassCreationHelperMethods.GetDtoForCustomSprocNamespace(_enclosingApplicationNamespace);
            string dtoClassName = ClassCreationHelperMethods.GetDtoClassName(metaInformationSchema.MetaTable.TableName);
            List<string> dtoNamespaces = GetDtoNamespaceList();
            string outputFileAndPath = outputFilePath + outputFileName;

            bool overwriteExisting = true;

          string dtoFullName = GenerateDtoForCustomSproc(dtoNamespace, dtoNamespaces, dtoClassName,
                            metaInformationSchema, outputFileAndPath, overwriteExisting,sprocName,tableName,columnsReferenced);
          return dtoFullName; 
        }

        public string GenerateInputDtoForNonGetCustomSproc(MetaInformationSchema metaInformationSchema,
                                      string sprocName,
                                      string tableName,
                                      List<Data.MetaSqlDependency> columnsReferenced,
                                      List<StoredProcedure> customSprocsToGenerate)
        {

            string outputFilePath = OUTPUT_PATH_DTO;
            string outputFileName = ClassCreationHelperMethods.GetInputDtoForCustomSprocFileName(sprocName);
            string dtoNamespace = ClassCreationHelperMethods.GetInputDtoForCustomSprocNamespace(_enclosingApplicationNamespace);
            string dtoClassName = ClassCreationHelperMethods.GetDtoClassName(sprocName);
            List<string> dtoNamespaces = GetInputDtoNamespaceList();
            string outputFileAndPath = outputFilePath + outputFileName;

            bool overwriteExisting = true;

            string dtoFullName = GenerateInputDtoForCustomSproc(dtoNamespace,
                                                                dtoNamespaces,
                                                                dtoClassName,
                                                                metaInformationSchema,
                                                                outputFileAndPath,
                                                                overwriteExisting,
                                                                sprocName,
                                                                tableName,
                                                                columnsReferenced,
                                                                customSprocsToGenerate);
            return dtoFullName;
        }

        public string GenerateInputDtoForCustomSproc(MetaInformationSchema metaInformationSchema,
                                              string sprocName,
                                              string tableName,
                                              List<Data.MetaSqlDependency> columnsReferenced,
                                              List<StoredProcedure> customSprocsToGenerate)
        {

            string outputFilePath = OUTPUT_PATH_DTO;
            string outputFileName = ClassCreationHelperMethods.GetInputDtoForCustomSprocFileName(sprocName);
            string dtoNamespace = ClassCreationHelperMethods.GetInputDtoForCustomSprocNamespace(_enclosingApplicationNamespace);
            string dtoClassName = ClassCreationHelperMethods.GetDtoClassName(sprocName);
            List<string> dtoNamespaces = GetInputDtoNamespaceList();
            string outputFileAndPath = outputFilePath + outputFileName;

            bool overwriteExisting = true;

            string dtoFullName = GenerateInputDtoForCustomSproc(dtoNamespace, dtoNamespaces, dtoClassName,
                              metaInformationSchema, outputFileAndPath, overwriteExisting, sprocName, tableName, columnsReferenced,
                              customSprocsToGenerate);
            return dtoFullName;
        }

        public string GenerateInputDtoForMainCustomSproc(string sprocName,                                                                    
                                      StoredProcedure customSprocToGenerate)
        {

            string outputFilePath = OUTPUT_PATH_DTO;
            string outputFileName = ClassCreationHelperMethods.GetInputDtoForCustomSprocFileName(sprocName);
            string dtoNamespace = ClassCreationHelperMethods.GetInputDtoForCustomSprocNamespace(_enclosingApplicationNamespace);
            string dtoClassName = ClassCreationHelperMethods.GetDtoClassName(sprocName);
            List<string> dtoNamespaces = GetInputDtoNamespaceList();
            string outputFileAndPath = outputFilePath + outputFileName;

            bool overwriteExisting = true;

            string dtoFullName = GenerateInputDtoForMainCustomSproc(dtoNamespace, dtoNamespaces, dtoClassName,
                              outputFileAndPath, overwriteExisting, sprocName,
                              customSprocToGenerate);
            return dtoFullName;
        }

        public void GenerateListClasses(List<MetaInformationSchema> metaInformationSchemas,
                                        bool overwriteExisting)
        {
            foreach (MetaInformationSchema metaInformationSchema in metaInformationSchemas)
            {                
                string outputFilePath = @"..\..\GeneratedLists\";
                string outputFileName = ClassCreationHelperMethods.GetListFileName(metaInformationSchema.MetaTable.TableName);
                string listNamespace = ClassCreationHelperMethods.GetListNamespace(_enclosingApplicationNamespace);
                string listClassName = ClassCreationHelperMethods.GetListClassName(metaInformationSchema.MetaTable.TableName);
                List<string> listNamespaces = GetListNamespaceList(_enclosingApplicationNamespace);
                string outputFileAndPath = outputFilePath + outputFileName;
                if (!overwriteExisting)
                {
                    if (!File.Exists(outputFileAndPath))
                    {
                        GenerateList(listNamespace, listNamespaces, listClassName,
                                     metaInformationSchema, outputFileAndPath,overwriteExisting);
                    }
                }
                else
                {
                    GenerateList(listNamespace, listNamespaces, listClassName,
                                    metaInformationSchema, outputFileAndPath,overwriteExisting);
                }

            }
        }


        

       

        public void GenerateList(string listNamespace,
                         List<string> listNamespaces,
                         string listClassName,
                         MetaInformationSchema metaInformationSchema,
                         string outputFileAndPath,
                         bool overwriteExisting)
        {
            CodeCompileUnit targetUnit = new CodeCompileUnit();
            CodeNamespace list = new CodeNamespace(listNamespace);
            foreach (string strNamespace in listNamespaces)
            {
                list.Imports.Add(new CodeNamespaceImport(strNamespace));
            }
            CodeTypeDeclaration targetClass = new CodeTypeDeclaration(listClassName);

            targetClass.IsClass = true;
            targetClass.TypeAttributes = TypeAttributes.Public;

            string dtoFullName = ClassCreationHelperMethods.GetDtoNamespace(_enclosingApplicationNamespace) + ClassCreationHelperConstants.DOT_OPERATOR + ClassCreationHelperMethods.GetDtoClassName(metaInformationSchema.MetaTable.TableName);

            CodeTypeReference listTypeReference = new CodeTypeReference("List", new CodeTypeReference[] { new CodeTypeReference(dtoFullName) });
            targetClass.BaseTypes.Add(listTypeReference);
            Type type = typeof(CommonLibrary.Base.Database.BaseDatabase);
            AddPrivateMember(targetClass, GetPrivateMemberName(type.Name), type);

            List<CodeParameterDeclarationExpression> listOfParameterExpressions =
                new List<CodeParameterDeclarationExpression>();

            Type dataReader = typeof(System.Data.SqlClient.SqlDataReader);

            CodeParameterDeclarationExpression sqlDataReaderExpression =
                GetParameterDeclarationExpression(dataReader, ClassCreationHelperConstants.READER);

            listOfParameterExpressions.Add(sqlDataReaderExpression);
            CodeConstructor constructor = new CodeConstructor();
            InitializeConstructor(constructor,
                                 MemberAttributes.Public,
                                 listOfParameterExpressions);

            CodeVariableReferenceExpression var =
                new CodeVariableReferenceExpression(ClassCreationHelperConstants.READER);

            CodeMethodInvokeExpression methodInvoke = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(),
                ClassCreationHelperConstants.ADD_ITEMS_TO_LIST_BY_READER, var);

            constructor.Statements.Add(methodInvoke);
            targetClass.Members.Add(constructor);

            AddEmptyConstructor(MemberAttributes.Public, targetClass);

            GenerateAndAddAddItemsToListBySqlDataReaderMethod(targetClass,
                                                             dtoFullName,
                                                              GetPrivateMemberName(metaInformationSchema.MetaTable.TableName),
                                                              metaInformationSchema);
            AddTableNameAttributeToCodeTypeDeclaration(targetClass, metaInformationSchema);


            list.Types.Add(targetClass);
            targetUnit.Namespaces.Add(list);
            GenerateCSharpCode(outputFileAndPath, targetUnit, overwriteExisting);
        }

        public void GenerateOneDtoClass(MetaInformationSchema metaInformationSchema, bool overwriteExisting)
        {
           
                string outputFilePath = OUTPUT_PATH_DTO;
                string outputFileName = ClassCreationHelperMethods.GetDtoFileName(metaInformationSchema.MetaTable.TableName);
                string dtoNamespace = ClassCreationHelperMethods.GetDtoNamespace(_enclosingApplicationNamespace);
                string dtoClassName = ClassCreationHelperMethods.GetDtoClassName(metaInformationSchema.MetaTable.TableName);
                List<string> dtoNamespaces = GetDtoNamespaceList();
                string outputFileAndPath = outputFilePath + outputFileName;

                if (!overwriteExisting)
                {
                    if (!File.Exists(outputFileAndPath))
                    {
                        GenerateDto(dtoNamespace, dtoNamespaces, dtoClassName,
                                    metaInformationSchema, outputFileAndPath, overwriteExisting);
                    }
                }
                else
                {
                    GenerateDto(dtoNamespace, dtoNamespaces, dtoClassName,
                                metaInformationSchema, outputFileAndPath, overwriteExisting);
                }

            

        }

        public void GenerateDtoClasses(List<MetaInformationSchema> metaInformationSchemas, bool overwriteExisting)
        {
            foreach (MetaInformationSchema metaInformationSchema in metaInformationSchemas)
            {               
                string outputFilePath = OUTPUT_PATH_DTO;
                string outputFileName = ClassCreationHelperMethods.GetDtoFileName(metaInformationSchema.MetaTable.TableName);
                string dtoNamespace = ClassCreationHelperMethods.GetDtoNamespace(_enclosingApplicationNamespace);
                string dtoClassName = ClassCreationHelperMethods.GetDtoClassName(metaInformationSchema.MetaTable.TableName);
                List<string> dtoNamespaces = GetDtoNamespaceList();
                string outputFileAndPath = outputFilePath + outputFileName;

                if (!overwriteExisting)
                {
                    if (!File.Exists(outputFileAndPath))
                    {
                        GenerateDto(dtoNamespace, dtoNamespaces, dtoClassName,
                                    metaInformationSchema, outputFileAndPath,overwriteExisting);
                    }
                }
                else
                {
                    GenerateDto(dtoNamespace, dtoNamespaces, dtoClassName,
                                metaInformationSchema, outputFileAndPath,overwriteExisting);
                }
                          
            }
           
        }

        public string GenerateDtoForCustomSproc(string dtoNamespace,
                        List<string> dtoNamespaces,
                        string dtoClassName,
                        MetaInformationSchema metaInformationSchema,
                        string outputFileAndPath,
                        bool overwriteExisting,
                        string sprocName,
                        string tableName,
                        List<Data.MetaSqlDependency> columnsReferenced)
        {
            PredicateFunctions predicateFunctions = new PredicateFunctions();
            CodeCompileUnit targetUnit = new CodeCompileUnit();
            Type typeToReflect = null;
            string dtoFullName = dtoNamespace + ClassCreationHelperConstants.DOT_OPERATOR + dtoClassName;
            
            typeToReflect = GetTypeToReflect(dtoFullName);            

            if (typeToReflect == null)
            {
                //not compiled into the assembly yet,  not "included in the project"
                //but we may have this assembly already generated in memory.
                if (AssembliesGeneratedInMemory.Count > 0)
                {
                    predicateFunctions.AssemblyFullName = dtoFullName;

                    Object objAssembly = AssembliesGeneratedInMemory.Find(predicateFunctions.FindAssemblyLoadedInMemoryByFullAssemblyName);
                    if (objAssembly != null)
                    {
                        typeToReflect = objAssembly.GetType();
                    }

                }
            }

            CodeNamespace dto = new CodeNamespace(dtoNamespace);
            foreach (string strNamespace in dtoNamespaces)
            {
                dto.Imports.Add(new CodeNamespaceImport(strNamespace));
            }
            CodeTypeDeclaration targetClass = new CodeTypeDeclaration(dtoClassName);
            targetClass.IsClass = true;
            targetClass.TypeAttributes =
                TypeAttributes.Public;
            dto.Types.Add(targetClass);
            targetUnit.Namespaces.Add(dto);

            //if (typeToReflect != null)
            //{
            //    //since this is a dto it should only have public properties and private members
            //    PropertyInfo[] propertyInfos = typeToReflect.GetProperties();
            //    MemberInfo[] memberInfos = typeToReflect.GetMembers(BindingFlags.NonPublic);
                
            //    AddPrivateMembersAndPropertiesToExistingSprocDto(targetClass, propertyInfos, memberInfos, sprocName);
            //}   

            AddPrivateMembersAndPropertiesToDto(targetClass,
                                                metaInformationSchema,
                                                columnsReferenced,
                                                typeToReflect,
                                                sprocName);

          
               
                //AddPrivateMembersAndPropertiesToDto(targetClass, metaInformationSchema);
                //AddTableNameAttributeToCodeTypeDeclaration(targetClass, metaInformationSchema);
                GenerateCSharpCode(outputFileAndPath, targetUnit, overwriteExisting);

                return dtoFullName;

        }

        public string GenerateInputDtoForCustomSproc(string dtoNamespace,
                List<string> dtoNamespaces,
                string dtoClassName,
                MetaInformationSchema metaInformationSchema,
                string outputFileAndPath,
                bool overwriteExisting,
                string sprocName,
                string tableName,
                List<Data.MetaSqlDependency> columnsReferenced,
            List<StoredProcedure> customSprocsToGenerate)
        {
            PredicateFunctions predicateFunctions = new PredicateFunctions();
            CodeCompileUnit targetUnit = new CodeCompileUnit();
            Type typeToReflect = null;
            string dtoFullName = dtoNamespace + ClassCreationHelperConstants.DOT_OPERATOR + dtoClassName;

            //this would mean that this file already exists and has been compiled into the app

            typeToReflect = GetTypeToReflect(dtoFullName);

            if (typeToReflect == null)
            {
                //not compiled into the assembly yet,  not "included in the project"
                //but we may have this assembly already generated in memory.
                if (AssembliesGeneratedInMemory.Count > 0)
                {
                    predicateFunctions.AssemblyFullName = dtoFullName;

                    Object objAssembly = AssembliesGeneratedInMemory.Find(predicateFunctions.FindAssemblyLoadedInMemoryByFullAssemblyName);
                    if (objAssembly != null)
                    {
                        typeToReflect = objAssembly.GetType();
                    }

                }
            }

            CodeNamespace dto = new CodeNamespace(dtoNamespace);
            foreach (string strNamespace in dtoNamespaces)
            {
                dto.Imports.Add(new CodeNamespaceImport(strNamespace));
            }
            CodeTypeDeclaration targetClass = new CodeTypeDeclaration(dtoClassName);
            targetClass.IsClass = true;
            targetClass.TypeAttributes =
                TypeAttributes.Public;
            dto.Types.Add(targetClass);
            targetUnit.Namespaces.Add(dto);            

            bool generateTheCode = AddPrivateMembersAndPropertiesToInputDto(targetClass,
                                                metaInformationSchema,
                                                columnsReferenced,
                                                typeToReflect,
                                                sprocName,
                                                customSprocsToGenerate);

            //this check is so huge, if we do not check if the column exists in the 
            //AddPrivateMembersAndPropertiesToInputDto
            //method and only regenerate if the column is found which is indicated by the boolean return
            //value of generate the code, then it actually overwrites the class without finding the property
            //and then the class has no property or attributes.  It basically "falls through" empty and overwrites
            //what is already there.
            if (generateTheCode)
            {
                GenerateCSharpCode(outputFileAndPath, targetUnit, overwriteExisting);
            }

            return dtoFullName;
        }

        public string GenerateInputDtoForMainCustomSproc(string dtoNamespace,
        List<string> dtoNamespaces,
        string dtoClassName,       
        string outputFileAndPath,
        bool overwriteExisting,
        string sprocName,              
        StoredProcedure customSprocToGenerate)
        {
            PredicateFunctions predicateFunctions = new PredicateFunctions();
            CodeCompileUnit targetUnit = new CodeCompileUnit();
            Type typeToReflect = null;
            string dtoFullName = dtoNamespace + ClassCreationHelperConstants.DOT_OPERATOR + dtoClassName;

            //this would mean that this file already exists and has been compiled into the app

            typeToReflect = GetTypeToReflect(dtoFullName);

            if (typeToReflect == null)
            {
                //not compiled into the assembly yet,  not "included in the project"
                //but we may have this assembly already generated in memory.
                if (AssembliesGeneratedInMemory.Count > 0)
                {
                    predicateFunctions.AssemblyFullName = dtoFullName;

                    Object objAssembly = AssembliesGeneratedInMemory.Find(predicateFunctions.FindAssemblyLoadedInMemoryByFullAssemblyName);
                    if (objAssembly != null)
                    {
                        typeToReflect = objAssembly.GetType();
                    }

                }
            }

            CodeNamespace dto = new CodeNamespace(dtoNamespace);
            foreach (string strNamespace in dtoNamespaces)
            {
                dto.Imports.Add(new CodeNamespaceImport(strNamespace));
            }
            CodeTypeDeclaration targetClass = new CodeTypeDeclaration(dtoClassName);
            targetClass.IsClass = true;
            targetClass.TypeAttributes =
                TypeAttributes.Public;
            dto.Types.Add(targetClass);
            targetUnit.Namespaces.Add(dto);

            bool generateTheCode = AddPrivateMembersAndPropertiesToMainSprocInputDto(targetClass,                                                                                              
                                                typeToReflect,
                                                sprocName,
                                                customSprocToGenerate);

            //this check is so huge, if we do not check if the column exists in the 
            //AddPrivateMembersAndPropertiesToInputDto
            //method and only regenerate if the column is found which is indicated by the boolean return
            //value of generate the code, then it actually overwrites the class without finding the property
            //and then the class has no property or attributes.  It basically "falls through" empty and overwrites
            //what is already there.
            if (generateTheCode)
            {
                GenerateCSharpCode(outputFileAndPath, targetUnit, overwriteExisting);
            }

            return dtoFullName;
        }

        public void GenerateDto(string dtoNamespace,
                                List<string> dtoNamespaces,
                                string dtoClassName,
                                MetaInformationSchema metaInformationSchema,
                                string outputFileAndPath,
                                bool overwriteExisting)
        {
            CodeCompileUnit targetUnit = new CodeCompileUnit();
            CodeNamespace dto = new CodeNamespace(dtoNamespace);
            foreach (string strNamespace in dtoNamespaces)
            {
                dto.Imports.Add(new CodeNamespaceImport(strNamespace));
            }
            CodeTypeDeclaration targetClass = new CodeTypeDeclaration(dtoClassName);
            targetClass.IsClass = true;
            targetClass.TypeAttributes =
                TypeAttributes.Public;
            dto.Types.Add(targetClass);
            targetUnit.Namespaces.Add(dto);
            AddPrivateMembersAndPropertiesToDto(targetClass, metaInformationSchema);
            AddTableNameAttributeToCodeTypeDeclaration(targetClass, metaInformationSchema);

            CodeMemberMethod setIsModifiedMethod = 
                GenerateIsModifiedDictionary(targetClass,metaInformationSchema);

            AddSetIsModified(setIsModifiedMethod, targetClass, metaInformationSchema);

            GenerateCSharpCode(outputFileAndPath, targetUnit,overwriteExisting);
     
        }
       

        public void AddSetIsModified(CodeMemberMethod setIsModifiedMethod, 
                                    CodeTypeDeclaration targetClass, 
                                    MetaInformationSchema metaInformationSchema)
        {
            foreach (CodeTypeMember member in targetClass.Members)
            {
                if (member is CodeMemberProperty)
                {
                    CodeMemberProperty property = (CodeMemberProperty)member;
                    if (property.Name != ClassCreationHelperConstants.IS_MODIFIED_DICTIONARY_PROPERTY_NAME)
                    {
                        CodeAttributeDeclarationCollection attColl = property.CustomAttributes;

                        foreach (CodeAttributeDeclaration att in attColl)
                        {
                            if (att.AttributeType.BaseType.ToString() == "CommonLibrary.CustomAttributes.DatabaseColumnAttribute")
                            {
                                property.SetStatements.Add(new CodeMethodInvokeExpression(
                                    new CodeThisReferenceExpression(),
                                    setIsModifiedMethod.Name,
                                    new CodePrimitiveExpression(((CodePrimitiveExpression)att.Arguments[0].Value).Value)));
                            }
                        }
                    }
                }
            }         

        }

        public CodeMemberMethod GenerateIsModifiedDictionary(CodeTypeDeclaration targetClass,MetaInformationSchema metaInformationSchema)
        {
             CodeTypeReference myDictionary = new CodeTypeReference(
                "Dictionary",
                new CodeTypeReference[] {
                    new CodeTypeReference(typeof(string)),
                    new CodeTypeReference(typeof(bool))});

            CodeMemberField field = new CodeMemberField(myDictionary,GetPrivateMemberName(
                ClassCreationHelperConstants.IS_MODIFIED_DICTIONARY_PROPERTY_NAME));
            field.InitExpression = new CodeObjectCreateExpression(myDictionary, new CodeObjectCreateExpression[] { });

            targetClass.Members.Add(field);

            CodeMemberProperty property = new CodeMemberProperty();
            property.Attributes = MemberAttributes.Public;
            property.Name = ClassCreationHelperConstants.IS_MODIFIED_DICTIONARY_PROPERTY_NAME;
            property.Type = myDictionary;

            CodeThisReferenceExpression codeThisReferenceExpression = new
               CodeThisReferenceExpression();

            CodeFieldReferenceExpression codefieldReferenceExpression =
                new CodeFieldReferenceExpression(codeThisReferenceExpression, field.Name);
            
            CodePropertyReferenceExpression codePropertyReferenceExpression = 
                new CodePropertyReferenceExpression(codeThisReferenceExpression,property.Name);


            CodeMethodReturnStatement codeMethodReturnStatement =
                new CodeMethodReturnStatement(codefieldReferenceExpression);

            CodeAssignStatement codeAssignStatement =
                new CodeAssignStatement(codefieldReferenceExpression, new CodePropertySetValueReferenceExpression());

            property.GetStatements.Add(codeMethodReturnStatement);
            property.SetStatements.Add(codeAssignStatement);

            property.HasGet = true;
            property.HasSet = true;

            targetClass.Members.Add(property);

            CodeMemberMethod fillDictionaryMethod = new CodeMemberMethod();
            fillDictionaryMethod.Attributes = MemberAttributes.Private;
            fillDictionaryMethod.Name = ClassCreationHelperConstants.INITIALIZE_IS_MODIFIED_DICTIONARY_METHOD_NAME;

            //foreach (Data.InformationSchemaColumn column in metaInformationSchema.MetaColumns)
            //{

            foreach (CodeTypeMember member in targetClass.Members)
            {
                if (member is CodeMemberProperty)
                {
                    CodeMemberProperty cmproperty = (CodeMemberProperty)member;
                    if (cmproperty.Name != ClassCreationHelperConstants.IS_MODIFIED_DICTIONARY_PROPERTY_NAME)
                    {
                        CodeAttributeDeclarationCollection attColl = cmproperty.CustomAttributes;

                        foreach (CodeAttributeDeclaration att in attColl)
                        {
                            if (att.AttributeType.BaseType.ToString() == "CommonLibrary.CustomAttributes.DatabaseColumnAttribute")
                            {                               
                                CodeMethodInvokeExpression invokeAdd =
                                    new CodeMethodInvokeExpression(codePropertyReferenceExpression, "Add",
                                    new CodePrimitiveExpression[] {new CodePrimitiveExpression(((CodePrimitiveExpression)att.Arguments[0].Value).Value),
                        new CodePrimitiveExpression(false)});

                                fillDictionaryMethod.Statements.Add(invokeAdd);
                            }
                        }
                    }
                }
            }
            //}

            targetClass.Members.Add(fillDictionaryMethod);
            AddEmptyConstructor(MemberAttributes.Public, targetClass);

            foreach (CodeTypeMember member in targetClass.Members)
            {
                if (member is CodeConstructor)
                {
                    CodeConstructor constructor = (CodeConstructor)member;
                    constructor.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(),
                        fillDictionaryMethod.Name,
                        new CodeExpression[] { }));
                }
            }
            CodeMemberMethod setIsModifiedMethod = new CodeMemberMethod();
            setIsModifiedMethod.Attributes = MemberAttributes.Private;
            setIsModifiedMethod.Name = ClassCreationHelperConstants.SET_IS_MODIFIED_METHOD_NAME;

            CodeParameterDeclarationExpression columnNameParameter =
                new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(string)),
                ClassCreationHelperConstants.COLUMN_NAME_PARAMETER_VAR);

            CodeVariableReferenceExpression columnNameVariableRef = 
                new CodeVariableReferenceExpression(columnNameParameter.Name);

            setIsModifiedMethod.Parameters.Add(columnNameParameter);

            CodeArrayIndexerExpression propertyIndexerExpression = 
                new CodeArrayIndexerExpression(new CodeVariableReferenceExpression(codePropertyReferenceExpression.PropertyName), 
                columnNameVariableRef);

            

            CodeConditionStatement ifModifiedContainsKey =
                new CodeConditionStatement(new CodeBinaryOperatorExpression(
                new CodeMethodInvokeExpression(codePropertyReferenceExpression, "ContainsKey",columnNameVariableRef), CodeBinaryOperatorType.IdentityEquality,
                new CodePrimitiveExpression(true)),
                new CodeAssignStatement(propertyIndexerExpression,new CodePrimitiveExpression(true)));

            setIsModifiedMethod.Statements.Add(ifModifiedContainsKey);

            targetClass.Members.Add(setIsModifiedMethod);
            return setIsModifiedMethod;                                         
            
            // CodeVariableDeclarationStatement myDictionaryVar =
            //     new CodeVariableDeclarationStatement(myDictionary,
            //           "dict",
            //               new CodeObjectCreateExpression(myDictionary));
            //CodeMemberField field = new CodeMemberField(

          
            // targetClass.Members.Add(myDictionaryVar);


            //methodMain.Statements.Add(
            //      new CodeVariableDeclarationStatement(myDictionary,
            //          "dict",
            //              new CodeObjectCreateExpression(myDictionary)));

        }



        public void GenerateAndAddAddItemsToListBySqlDataReaderMethod(CodeTypeDeclaration targetClass,
                                                                      string dtoFullName,
                                                                      string dtoVarName,
                                                                      MetaInformationSchema metaInformationSchema)
        {
            CodeMemberMethod addAddItemsToListBySqlDataReaderMethod = new CodeMemberMethod();
            addAddItemsToListBySqlDataReaderMethod.Attributes =
                MemberAttributes.Public;
            addAddItemsToListBySqlDataReaderMethod.Name = ClassCreationHelperConstants.ADD_ITEMS_TO_LIST_BY_READER;

            Type dataReader = typeof(System.Data.SqlClient.SqlDataReader);

            CodeParameterDeclarationExpression sqlDataReaderExpression =
                 GetParameterDeclarationExpression(dataReader, ClassCreationHelperConstants.READER);
          

            CodeVariableDeclarationStatement dtoVariableDeclarationStatement =
                new CodeVariableDeclarationStatement(new CodeTypeReference(dtoFullName), "dto",null);

            CodeVariableReferenceExpression dtoVariableReferenceExpression =
             new CodeVariableReferenceExpression("dto");

            CodeTypeReference dtoType = new CodeTypeReference(dtoFullName);

            CodeSnippetStatement tabFiller = new CodeSnippetStatement(ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB);


            CodeAssignStatement dtoObjectNewStatement = new CodeAssignStatement(dtoVariableReferenceExpression,
                new CodeObjectCreateExpression(dtoType));

            addAddItemsToListBySqlDataReaderMethod.Statements.Add(tabFiller);

            addAddItemsToListBySqlDataReaderMethod.Statements.Add(dtoVariableDeclarationStatement);
            addAddItemsToListBySqlDataReaderMethod.Parameters.Add(sqlDataReaderExpression);            

            CodeSnippetStatement literalUsingBeginStatement =
                new CodeSnippetStatement(ClassCreationHelperConstants.TAB + 
                ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + 
                ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.USING + 
                ClassCreationHelperConstants.CONDITION_OPEN_BRACKET + 
                sqlDataReaderExpression.Name + ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET +
                Environment.NewLine + ClassCreationHelperConstants.TAB +
                ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB +
                ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.CSHARP_OPEN_BRACE + 
                Environment.NewLine);

            CodeSnippetStatement literalWhileBeginStatement = 
                new CodeSnippetStatement(ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + @"while(reader.Read())"
                                         + Environment.NewLine + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + @"{");

            addAddItemsToListBySqlDataReaderMethod.Statements.Add(literalUsingBeginStatement);
            addAddItemsToListBySqlDataReaderMethod.Statements.Add(literalWhileBeginStatement);
            addAddItemsToListBySqlDataReaderMethod.Statements.Add(dtoObjectNewStatement);

            CodeSnippetStatement literalDtoFillStatement = 
                new CodeSnippetStatement(GetDtoAssignmentFromSqlDataReaderStatement(dtoFullName,
                                                                                    metaInformationSchema, 
                                                                                    dtoVariableDeclarationStatement));

            addAddItemsToListBySqlDataReaderMethod.Statements.Add(literalDtoFillStatement);

            CodeSnippetStatement literalWhileEndStatement =
                new CodeSnippetStatement(ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + @"}");
            

            addAddItemsToListBySqlDataReaderMethod.Statements.Add(literalWhileEndStatement);

            CodeSnippetStatement literalUsingEndStatement =
                new CodeSnippetStatement(Environment.NewLine + ClassCreationHelperConstants.TAB +
                ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB +
                ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.CSHARP_CLOSE_BRACE +
                                         Environment.NewLine);
            addAddItemsToListBySqlDataReaderMethod.Statements.Add(literalUsingEndStatement);

            targetClass.Members.Add(addAddItemsToListBySqlDataReaderMethod);

        }

        public string GetDtoAssignmentFromSqlDataReaderStatement(string dtoFullName,
                                                               MetaInformationSchema metaInformationSchema,
                                                                CodeVariableDeclarationStatement varDeclaration)
        {
            string dtoFill = string.Empty;
            PredicateFunctions predicateFunctions = new PredicateFunctions();

            Dictionary<string, string> propertyNameKeyDatabaseColumnValue = new Dictionary<string, string>();

            Dictionary<Data.InformationSchemaColumn, string> columnsFoundAndTheirPropertyName =
                new Dictionary<SprocDataLayerGenerator.Data.InformationSchemaColumn, string>();

            if (AssembliesGeneratedInMemory.Count > 0)
            {
                predicateFunctions.AssemblyFullName = dtoFullName;
                Type typeToReflect = null;
                Object objAssembly = AssembliesGeneratedInMemory.Find(predicateFunctions.FindAssemblyLoadedInMemoryByFullAssemblyName);
                if (objAssembly != null)
                {
                   typeToReflect = objAssembly.GetType();                   
                       
                    PropertyInfo[] properties = typeToReflect.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        object[] attributes = property.GetCustomAttributes(true);

                        if (attributes.Length != 0)
                        {
                            foreach (object attribute in attributes)
                            {                                
                                 if(attribute is DatabaseColumnAttribute)
                                {
                                     DatabaseColumnAttribute databaseColumnAttribute = 
                                         (DatabaseColumnAttribute)attribute;

                                     propertyNameKeyDatabaseColumnValue.Add(property.Name,
                                                                            databaseColumnAttribute.DatabaseColumn);
                                   
                                }
                             }                           
                               
                        }                        
                    }
                
                if (propertyNameKeyDatabaseColumnValue.Count > 0)
                {
                    columnsFoundAndTheirPropertyName = 
                        GetColumnToPropertyName(metaInformationSchema.MetaColumns,
                                                propertyNameKeyDatabaseColumnValue);
                }
            }

                dtoFill = GetDtoAssignmentFromSqlDataReaderStatement(columnsFoundAndTheirPropertyName,
                                                                     typeToReflect,varDeclaration);
            }
           
            return dtoFill;
        }

        public string GetDtoAssignmentFromSqlDataReaderStatement(Dictionary<Data.InformationSchemaColumn,
                                                                 string> columnToPropertyNameDictionary,
                                                                 Type dtoType,
                                                                 CodeVariableDeclarationStatement dtoVarDeclaration)
        {            
            StringBuilder sb = new StringBuilder();

            Dictionary<Data.InformationSchemaColumn, string> distinctSet =
                GetDistinctColumnToPropertyName(columnToPropertyNameDictionary);

            foreach (KeyValuePair<Data.InformationSchemaColumn, string> kvp in distinctSet)
            {
                sb.Append(ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + dtoVarDeclaration.Name);
                sb.Append(ClassCreationHelperConstants.DOT_OPERATOR);
                sb.Append(kvp.Value);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.EQUALS);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(Environment.NewLine);
                sb.Append(ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.BASE_DATABASE_VARIABLE_NAME);
                sb.Append(ClassCreationHelperConstants.DOT_OPERATOR);
                sb.Append(GetMethodNameToResolveNull(kvp.Key));
                sb.Append(ClassCreationHelperConstants.CONDITION_OPEN_BRACKET);
                sb.Append(ClassCreationHelperConstants.READER_VARIABLE_NAME);
                sb.Append(ClassCreationHelperConstants.DOT_OPERATOR);
                sb.Append(ClassCreationHelperConstants.GET_ORDINAL_READER_METHOD_NAME);
                sb.Append(ClassCreationHelperConstants.CONDITION_OPEN_BRACKET);
                sb.Append(ClassCreationHelperConstants.QUOTE);
                sb.Append(kvp.Key.ColumnName);
                sb.Append(ClassCreationHelperConstants.QUOTE);
                sb.Append(ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET);
                sb.Append(ClassCreationHelperConstants.COMMA);
                sb.Append(ClassCreationHelperConstants.SPACE);
                sb.Append(ClassCreationHelperConstants.READER_VARIABLE_NAME);
                sb.Append(ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET);
                sb.Append(ClassCreationHelperConstants.SEMI_COLON);
                sb.Append(Environment.NewLine);
            }
            sb.Append(ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.TAB + ClassCreationHelperConstants.THIS);
            sb.Append(ClassCreationHelperConstants.DOT_OPERATOR);
            sb.Append(ClassCreationHelperConstants.ADD);
            sb.Append(ClassCreationHelperConstants.CONDITION_OPEN_BRACKET);
            sb.Append(dtoVarDeclaration.Name);
            sb.Append(ClassCreationHelperConstants.CONDITION_CLOSE_BRACKET);
            sb.Append(ClassCreationHelperConstants.SEMI_COLON);

            return sb.ToString();
        }

        public Dictionary<Data.InformationSchemaColumn, string> GetDistinctColumnToPropertyName(
            Dictionary<Data.InformationSchemaColumn, string> columnToPropertyNameDictionary)
        {
            Dictionary<Data.InformationSchemaColumn, string> distinctSet =
                new Dictionary<Data.InformationSchemaColumn, string>();

            foreach (KeyValuePair<Data.InformationSchemaColumn, string> kvp in columnToPropertyNameDictionary)
            {
                if (!distinctSet.ContainsValue(kvp.Value))
                {
                    distinctSet.Add(kvp.Key, kvp.Value);
                }
            }
            return distinctSet;
        }

        /// <summary>
        /// TODO:  when I support more types in database i need to update this method and the base database
        /// class to have resolve nulls
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetMethodNameToResolveNull(Data.InformationSchemaColumn column)
        {
            string tableName = column.TableName;
            PredicateFunctions predicateFunctions = new PredicateFunctions();
            predicateFunctions.TableNameHolder = tableName;
            MetaInformationSchema metaInformationSchema = 
                _metaInformationSchemaManager.MetaDataList.Find(predicateFunctions.FindMetaInformationSchemaByTableName);
            bool isPrimaryKey = IsPrimaryKeyColumn(column,
                                                   metaInformationSchema.MetaConstraintColumnUsage,
                                                   metaInformationSchema.MetaTableConstraints);

            Type type = GetTypeByColumnDataType(column);
            Type typeToReflect = typeof(CommonLibrary.Base.Database.BaseDatabase);
            MethodInfo[] methodInfos = typeToReflect.GetMethods();
            string methodName = string.Empty;

            bool IsNullable = false;
            if (column.IsNullable == ClassCreationHelperConstants.YES)
            {
                IsNullable = true;
            }

            //Type intType = Type.GetType("System.Nullable`1[System.Int32]");
            //CodeTypeReference typeRef = new CodeTypeReference("System.Nullable`1[System.Int32]");
            CodeTypeReference typeRef = new CodeTypeReference(type.FullName);
            string typeInCSharp = 
                CodeDomProvider.CreateProvider("CSharp").CreateGenerator().GetTypeOutput(typeRef);

            switch (typeInCSharp)
            {
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_STRING:
                    {
                        if (IsNullable && (!isPrimaryKey))
                        {
                            methodName = "resolveNullStringToNull";
                        }
                        else
                        {
                            methodName = "resolveNullString";
                        }
                        break;
                    }
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_INT:
                    {
                        if (IsNullable && (!isPrimaryKey))
                        {
                            methodName = "resolveNullInt32ToNullableDataType";
                        }
                        else
                        {
                            methodName = "resolveNullInt32";
                        }
                        break;
                    }
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_NULLABLE_INT:
                    {
                        if (IsNullable && (!isPrimaryKey))
                        {
                            methodName = "resolveNullInt32ToNullableDataType";
                        }
                        else
                        {
                            methodName = "resolveNullInt32";
                        }
                        break;
                    }
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_BOOL:
                    {
                        if (IsNullable && (!isPrimaryKey))
                        {
                            methodName = "resolveNullBooleanToNullableDataType";
                        }
                        else
                        {
                            methodName = "resolveNullBoolean";
                        }
                        break;
                    }
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_NULLABLE_BOOL:
                    {
                        if (IsNullable && (!isPrimaryKey))
                        {
                            methodName = "resolveNullBooleanToNullableDataType";
                        }
                        else
                        {
                            methodName = "resolveNullBoolean";
                        }
                        break;
                    }
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_DOUBLE:
                    {
                        if (IsNullable && (!isPrimaryKey))
                        {
                            methodName = "resolveNullDoubleToNullableDataType";
                        }
                        else
                        {
                            methodName = "resolveNullDouble";
                        }
                        break;
                    }
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_NULLABLE_DOUBLE:
                    {
                        if (IsNullable && (!isPrimaryKey))
                        {
                            methodName = "resolveNullDoubleToNullableDataType";
                        }
                        else
                        {
                            methodName = "resolveNullDouble";
                        }
                        break;
                    }
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_SHORT:
                    {
                        if (IsNullable && (!isPrimaryKey))
                        {
                            methodName = "resolveNullSmallIntToNullableDataType";
                        }
                        else
                        {
                            methodName = "resolveNullSmallInt";
                        }
                        break;
                    }
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_NULLABLE_SHORT:
                    {
                        if (IsNullable && (!isPrimaryKey))
                        {
                            methodName = "resolveNullSmallIntToNullableDataType";
                        }
                        else
                        {
                            methodName = "resolveNullSmallInt";
                        }
                        break;
                    }
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_DATETIME:
                    {
                        if (IsNullable && (!isPrimaryKey))
                        {
                            methodName = "resolveNullDateTimeToNullableDataType";
                        }
                        else
                        {
                            methodName = "resolveNullDateTime";
                        }

                        break;
                    }
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_NULLABLE_DATETIME:
                    {
                        if (IsNullable && (!isPrimaryKey))
                        {
                            methodName = "resolveNullDateTimeToNullableDataType";
                        }
                        else
                        {
                            methodName = "resolveNullDateTime";
                        }
                        break;
                    }


                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_IMAGE:
                    {
                        methodName = ClassCreationHelperConstants.NULLABLE_IMAGE_READER_METHOD_RETURNS_IMAGE;
                        break;
                    }
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_BYTE:
                    {
                        methodName = "resolveNullByteToMinValue";
                        break;
                    }
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_BYTE_ARRAY:
                    {
                        methodName = ClassCreationHelperConstants.NULLABLE_TYPE_READER_METHOD_RETURNS_OBJECT;
                        break;
                    }
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_LONG:
                    {
                        if (IsNullable && (!isPrimaryKey))
                        {
                            methodName = "resolveNullLongToNullableDataType";
                        }
                        else
                        {
                            methodName = "resolveNullLong";
                        }
                        break;
                    }
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_NULLABLE_LONG:
                    {
                        if (IsNullable && (!isPrimaryKey))
                        {
                            methodName = "resolveNullLongToNullableDataType";
                        }
                        else
                        {
                            methodName = "resolveNullLong";
                        }
                        break;
                    }
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_FLOAT:
                    {
                        if (IsNullable && (!isPrimaryKey))
                        {
                            methodName = "resolveNullFloatToNullableDataType";
                        }
                        else
                        {
                            methodName = "resolveNullFloat";
                        }
                        break;
                    }
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_NULLABLE_FLOAT:
                    {
                        if (IsNullable && (!isPrimaryKey))
                        {
                            methodName = "resolveNullFloatToNullableDataType";
                        }
                        else
                        {
                            methodName = "resolveNullFloat";
                        }
                        break;
                    }
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_DECIMAL:
                    {
                        if (IsNullable && (!isPrimaryKey))
                        {
                            methodName = "resolveNullDecimalToNullableDataType";
                        }
                        else
                        {
                            methodName = "resolveNullDecimal";
                        }
                        break;
                    }
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_NULLABLE_DECIMAL:
                    {
                        if (IsNullable && (!isPrimaryKey))
                        {
                            methodName = "resolveNullDecimalToNullableDataType";
                        }
                        else
                        {
                            methodName = "resolveNullDecimal";
                        }
                        break;
                    }
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_OBJECT:
                    {
                        methodName = ClassCreationHelperConstants.NULLABLE_TYPE_READER_METHOD_RETURNS_OBJECT;
                        break;
                    }
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_CHAR:
                    {
                        if (IsNullable && (!isPrimaryKey))
                        {
                            methodName = "resolveNullCharToNull";
                        }
                        else
                        {
                            methodName = "resolveNullChar";
                        }
                        break;
                    }
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_NULLABLE_CHAR:
                    {
                        if (IsNullable && (!isPrimaryKey))
                        {
                            methodName = "resolveNullCharToNull";
                        }
                        else
                        {
                            methodName = "resolveNullChar";
                        }
                        break;
                    }
                case CommonLibrary.Constants.CSharpDataTypeConstants.CSHARP_GUID:
                    {
                        methodName = "retrieveGuidFromDataReader";
                        break;
                    }
                default:
                    {
                        methodName = ClassCreationHelperConstants.NULLABLE_TYPE_READER_METHOD_RETURNS_OBJECT;
                        break;
                    }                
            }

            return methodName;
        }

        public Dictionary<Data.InformationSchemaColumn, string> GetColumnToPropertyName(List.InformationSchemaColumn columns,
                                                                                       Dictionary<string, string> dbColToPropNames,
                                                                                        StoredProcedure sproc)
        {
            PredicateFunctions predicateFunctions = new PredicateFunctions();

            Dictionary<Data.InformationSchemaColumn, string> foundColumns =
                new Dictionary<SprocDataLayerGenerator.Data.InformationSchemaColumn, string>();

            foreach (KeyValuePair<string, string> kvp in dbColToPropNames)
            {
                predicateFunctions.ColumnNameHolder = kvp.Key;

                Data.InformationSchemaColumn foundColumn = 
                    columns.Find(predicateFunctions.FindInformationSchemaColumn);
                if (foundColumn != null)
                {
                    foundColumns.Add(foundColumn, kvp.Value);
                }
                
            }
            return foundColumns;
        }

        public Dictionary<Data.InformationSchemaColumn, string> GetColumnToPropertyName(List.InformationSchemaColumn columns,
                                                                               Dictionary<string, string> dbColToPropNames)
        {
            PredicateFunctions predicateFunctions = new PredicateFunctions();

            Dictionary<Data.InformationSchemaColumn, string> foundColumns =
                new Dictionary<SprocDataLayerGenerator.Data.InformationSchemaColumn, string>();

            foreach (KeyValuePair<string, string> kvp in dbColToPropNames)
            {
                predicateFunctions.ColumnNameHolder = kvp.Key;

                Data.InformationSchemaColumn foundColumn =
                    columns.Find(predicateFunctions.FindInformationSchemaColumn);
                if (foundColumn != null)
                {
                    foundColumns.Add(foundColumn, kvp.Value);
                }

            }
            return foundColumns;
        }

       

        

        public void GenerateCSharpCode(string fileName, CodeCompileUnit targetUnit,bool overwriteExisting)
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
                                                cp.CompileAssemblyFromFile(cpar,fileName);
           
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

        public void AddTableNameAttributeToCodeTypeDeclaration(CodeTypeDeclaration targetClass,
                                                               MetaInformationSchema metaInformationSchema)
        {
            CodeAttributeDeclaration codeAttributeDeclaration;
            CommonLibrary.CustomAttributes.TableNameAttribute attribute =
                new TableNameAttribute(metaInformationSchema.MetaTable.TableName);
           
                codeAttributeDeclaration =
                 new CodeAttributeDeclaration(GetAttributeTypeReferenceByAttributeForDto(attribute),
                 new CodeAttributeArgument(new CodePrimitiveExpression(metaInformationSchema.MetaTable.TableName)));

            targetClass.CustomAttributes.Add(codeAttributeDeclaration);
        }

        public void AddPrivateMembersAndPropertiesToExistingSprocDto(CodeTypeDeclaration targetClass,
                                                             PropertyInfo[] propertyInfos,
                                                             MemberInfo[] memberInfos,
                                                             string sprocName)
        {
            PredicateFunctions predicateFunctions = new PredicateFunctions();
            List<PropertyInfo> propertyInfoList = new List<PropertyInfo>();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                propertyInfoList.Add(propertyInfo);
            }

            foreach (MemberInfo memberInfo in memberInfos)
            {
                string propertyName = GetPublicMemberName(memberInfo.Name, targetClass.Name);
                predicateFunctions.PublicPropertyNameHolder = propertyName;
                PropertyInfo propertyInfoFound = propertyInfoList.Find(predicateFunctions.FindPropertyInfoByPublicPropertyName);
                if (propertyInfoFound != null)
                {
                    AddPrivateMemberToDto(targetClass, memberInfo);
                    AddPublicPropertyToDto(targetClass, propertyInfoFound, memberInfo, sprocName);
                }

            }
        }


        public void AddPrivateMembersAndPropertiesToDto(CodeTypeDeclaration targetClass,
                                      MetaInformationSchema metaInformationSchema)
        {
            List.InformationSchemaColumn primaryKeyColumns 
                = GetPrimaryKeyColumns(metaInformationSchema.MetaColumnToConstraintColumnUsage,
                                       metaInformationSchema.MetaTableConstraints);

            List.InformationSchemaColumn regularColumns = GetColumnsNotInSet(primaryKeyColumns,
                                                          metaInformationSchema.MetaColumns);          

            foreach (Data.InformationSchemaColumn regularColumn in regularColumns)
            {
                DatabaseColumnAttribute columnMappingAttribute =
                    new DatabaseColumnAttribute(regularColumn.ColumnName);

                AddPrivateMemberToDto(targetClass, regularColumn);
                AddPublicPropertyToDto(targetClass, regularColumn, GetPrivateMemberName(regularColumn.ColumnName),columnMappingAttribute);
            }

            foreach (Data.InformationSchemaColumn primaryKeyColumn in primaryKeyColumns)
            {
                PrimaryKey primaryKeyAttribute = new PrimaryKey();
                DatabaseColumnAttribute columnMappingAttribute =
                    new DatabaseColumnAttribute(primaryKeyColumn.ColumnName);                

                List<Attribute> listOfAttributesForProperty = new List<Attribute>();
                listOfAttributesForProperty.Add(columnMappingAttribute);
                listOfAttributesForProperty.Add(primaryKeyAttribute);

                AddPrivateMemberToDto(targetClass,
                                 primaryKeyColumn);
                AddPublicPropertyToDto(targetClass,
                                  primaryKeyColumn,
                                  GetPrivateMemberName(primaryKeyColumn.ColumnName),
                                  listOfAttributesForProperty);
            }

        }

        public void AddPrivateMembersAndPropertiesToDto(CodeTypeDeclaration targetClass,
                              MetaInformationSchema metaInformationSchema,
                              List<Data.MetaSqlDependency> columnsReferenced,
                              Type typeToReflect,
                              string sprocName)
        {
            List<Data.InformationSchemaColumn> columns = 
                new List<SprocDataLayerGenerator.Data.InformationSchemaColumn>();

             StoredProcedure sproc =
                            _databaseSmoObjectsAndSettings.Database_Property.StoredProcedures[sprocName];
           
                //here we are only adding the columns from the dependencies table
                foreach (Data.MetaSqlDependency columnReferenced in columnsReferenced)
                {
                    List<Attribute> attributesForProperty = new List<Attribute>();
                    if (columnReferenced.IsSelected || columnReferenced.IsSelectAll)
                    {
                        SelectAttribute select = new SelectAttribute(sprocName);
                        attributesForProperty.Add(select);
                    }
                    if (columnReferenced.IsUpdated)
                    {
                        UpdateAttribute update = new UpdateAttribute(sprocName);
                        attributesForProperty.Add(update);
                    }
                    

                    PredicateFunctions predicateFunctions = new PredicateFunctions();
                    predicateFunctions.ColOrdinalHolder = columnReferenced.ReferencedMinorId;
                    Data.InformationSchemaColumn column = metaInformationSchema.MetaColumns.Find(predicateFunctions.FindInformationSchemaColumnByColumnOrdinal);

                    if (column != null)
                    {
                       
                        //if (IsColumnNameFoundInSprocTextBody(sproc.TextBody, column.ColumnName))
                        //{
                            DatabaseColumnAttribute columnAttribute =
                                new DatabaseColumnAttribute(column.ColumnName);
                            attributesForProperty.Add(columnAttribute);
                            AddPrivateAndPublicMembersToSprocDtoIfNotAlreadyExists(typeToReflect, targetClass, column, GetPrivateMemberName(column.ColumnName), attributesForProperty);
                            columns.Add(column);
                        //}
                    }
                }
            
                foreach (Data.InformationSchemaColumn columnToFind in metaInformationSchema.MetaColumns)
                {
                    List<Attribute> attributes = new List<Attribute>();
                    if (!columns.Contains(columnToFind))
                    {
                        //if (IsColumnNameFoundInSprocTextBody(sproc.TextBody, columnToFind.ColumnName))
                        //{
                            SelectAttribute select = new SelectAttribute(sprocName);
                            attributes.Add(select);

                            DatabaseColumnAttribute columnAttribute =
                                new DatabaseColumnAttribute(columnToFind.ColumnName);
                            attributes.Add(columnAttribute);
                            AddPrivateAndPublicMembersToSprocDtoIfNotAlreadyExists(typeToReflect,
                                targetClass, columnToFind,
                                GetPrivateMemberName(columnToFind.ColumnName), attributes);
                            columns.Add(columnToFind);
                        //}

                    }
                }
        }

        public bool AddPrivateMembersAndPropertiesToInputDto(CodeTypeDeclaration targetClass,
                      MetaInformationSchema metaInformationSchema,
                      List<Data.MetaSqlDependency> columnsReferenced,
                      Type typeToReflect,
                      string sprocName,
                      List<StoredProcedure> customSprocsToGenerate)
        {

            bool generateTheCode = false;
            PredicateFunctions predicateFunctions = new PredicateFunctions();
            predicateFunctions.SprocNameHolder = sprocName;

            List<StoredProcedure> storedProceduresInExistence = new List<StoredProcedure>();

            foreach (StoredProcedure sprocInExistence in
                this._databaseSmoObjectsAndSettings.Database_Property.StoredProcedures)
            {
                storedProceduresInExistence.Add(sprocInExistence);
            }

            StoredProcedure sproc =
               storedProceduresInExistence.Find(predicateFunctions.FindSprocGeneratedBySprocName);
            foreach (StoredProcedureParameter parameter in sproc.Parameters)
            {
                List<Attribute> attributesForProperty = new List<Attribute>();
                if (!parameter.IsOutputParameter)
                {
                    InputSprocParameterAttribute inputSprocParamter = new InputSprocParameterAttribute();

                    attributesForProperty.Add(inputSprocParamter);

                    predicateFunctions.ColumnNameHolder = 
                        CommonLibrary.Utility.DatabaseHelperMethods.GetColumnNameFromSqlParameterName(parameter.Name);

                    Data.InformationSchemaColumn column =
                        metaInformationSchema.MetaColumns.Find(predicateFunctions.FindInformationSchemaColumn);

                    if (column != null)
                    {
                        DatabaseColumnAttribute columnAttribute =
                            new DatabaseColumnAttribute(column.ColumnName);
                        attributesForProperty.Add(columnAttribute);
                        AddPrivateAndPublicMembersToSprocDtoIfNotAlreadyExists(typeToReflect, targetClass, column, GetPrivateMemberName(column.ColumnName), attributesForProperty);
                        generateTheCode = true;
                    }
                }
               
            }


            foreach (Data.MetaSqlDependency columnReferenced in columnsReferenced)
            {
                List<Attribute> attributesForProperty = new List<Attribute>();
               
                if (columnReferenced.IsUpdated)
                {
                    UpdateAttribute update = new UpdateAttribute(sprocName);
                    attributesForProperty.Add(update);
                  
                    predicateFunctions.ColOrdinalHolder = columnReferenced.ReferencedMinorId;
                    Data.InformationSchemaColumn column = metaInformationSchema.MetaColumns.Find(predicateFunctions.FindInformationSchemaColumnByColumnOrdinal);

                    if (column != null)
                    {
                        DatabaseColumnAttribute columnAttribute =
                            new DatabaseColumnAttribute(column.ColumnName);
                        attributesForProperty.Add(columnAttribute);
                        AddPrivateAndPublicMembersToSprocDtoIfNotAlreadyExists(typeToReflect, targetClass, column, GetPrivateMemberName(column.ColumnName), attributesForProperty);
                        generateTheCode = true;
                    }
                }
            }
            return generateTheCode;

        }

        public bool AddPrivateMembersAndPropertiesToMainSprocInputDto(CodeTypeDeclaration targetClass,
              Type typeToReflect,
              string sprocName,
              StoredProcedure customSprocToGenerate)
        {
            

            bool generateTheCode = false;
            PredicateFunctions predicateFunctions = new PredicateFunctions();
            predicateFunctions.SprocNameHolder = sprocName;

            List<StoredProcedure> storedProceduresInExistence = new List<StoredProcedure>();

            foreach (StoredProcedure sprocInExistence in
                this._databaseSmoObjectsAndSettings.Database_Property.StoredProcedures)
            {
                storedProceduresInExistence.Add(sprocInExistence);
            }

            StoredProcedure sproc =
               storedProceduresInExistence.Find(predicateFunctions.FindSprocGeneratedBySprocName);
            
            foreach (StoredProcedureParameter parameter in sproc.Parameters)
            {                
                string columnName = DatabaseHelperMethods.GetColumnNameFromSqlParameterName(parameter.Name);

              SqlDbType type = DatabaseHelperMethods.GetSqlDbTypeFromStoredProcedureParameterDataType(parameter.DataType);
                List<Attribute> attributesForProperty = new List<Attribute>();
                if (!parameter.IsOutputParameter)
                {
                    InputSprocParameterAttribute inputSprocParamter = new InputSprocParameterAttribute();

                    attributesForProperty.Add(inputSprocParamter);
                   
                        DatabaseColumnAttribute columnAttribute =
                            new DatabaseColumnAttribute(columnName);
                        attributesForProperty.Add(columnAttribute);
                        AddPrivateAndPublicMembersToSprocDtoIfNotAlreadyExists(typeToReflect, targetClass, columnName,type,
                            GetPrivateMemberName(columnName), attributesForProperty);
                        generateTheCode = true;
                   
                }

            }
            if (!generateTheCode)
            {
                if (sproc.Parameters.Count == 0)
                {
                    generateTheCode = true;
                }
            }
           

            return generateTheCode;

        }

        public void AddPrivateAndPublicMembersToSprocDtoIfNotAlreadyExists(Type typeToReflect,
                                                                           CodeTypeDeclaration targetClass,
                                                                           string columnName,
                                                                           SqlDbType type,
                                                                           string privateMemberName,
                                                                           List<Attribute> attributesForProperty)
        {
            PredicateFunctions predicateFunctions = new PredicateFunctions();

            if (typeToReflect == null)
            {
                AddPrivateMemberToDto(targetClass,columnName,type);
                AddPublicPropertyToSprocDto(targetClass, columnName,type, privateMemberName, attributesForProperty);
            }
            else
            {
                string publicPropertyName = GetPublicMemberName(columnName, targetClass.Name);

                predicateFunctions.PublicPropertyNameHolder = publicPropertyName;

                PropertyInfo propertyInfo = typeToReflect.GetProperty(publicPropertyName);

                if (propertyInfo == null)
                {
                    //property not found and so we need to create it new
                    AddPrivateMemberToDto(targetClass, columnName,type);
                    AddPublicPropertyToSprocDto(targetClass, columnName,type, privateMemberName, attributesForProperty);
                }
                else
                {
                    //property was found so we only want to add any attributes it does not yet have
                    //object[] customAttributesAlreadyDefined = propertyInfo.GetCustomAttributes(false);
                    //AssemblyName [] referencedAssemblies = typeToReflect.Assembly.GetReferencedAssemblies();
                    //foreach (AssemblyName assemblyName in referencedAssemblies)
                    //{
                    //    Assembly.Load(assemblyName);
                    //}
                    
                    IList<CustomAttributeData> customAttributesAlreadyDefined = CustomAttributeData.GetCustomAttributes(propertyInfo);
                    List<Attribute> attributesAlreadyDefinedList = new List<Attribute>();
                    List<Attribute> attributesNewAndNotAlreadyDefined = new List<Attribute>();

                    foreach (CustomAttributeData attribute in customAttributesAlreadyDefined)
                    {
                        Attribute attributeToAdd = null;
                        if (attribute.Constructor.ReflectedType.UnderlyingSystemType.FullName == typeof(DatabaseColumnAttribute).FullName)
                        {
                            foreach (CustomAttributeTypedArgument typedArg in attribute.ConstructorArguments)
                            {
                                string colName = typedArg.Value.ToString();
                                attributesAlreadyDefinedList.Add(new DatabaseColumnAttribute(colName));
                            }
                        }
                        else
                            if (attribute.Constructor.ReflectedType.UnderlyingSystemType.FullName == typeof(SelectAttribute).FullName)
                            {
                                foreach (CustomAttributeTypedArgument typedArg in attribute.ConstructorArguments)
                                {
                                    string sprocName = typedArg.Value.ToString();
                                    attributesAlreadyDefinedList.Add(new SelectAttribute(sprocName));
                                }
                            }
                            else
                                if (attribute.Constructor.ReflectedType.UnderlyingSystemType.FullName == typeof(UpdateAttribute).FullName)
                                {
                                    foreach (CustomAttributeTypedArgument typedArg in attribute.ConstructorArguments)
                                    {
                                        string sprocName = typedArg.Value.ToString();
                                        attributesAlreadyDefinedList.Add(new UpdateAttribute(sprocName));
                                    }
                                }
                        
                    }

                    foreach (Attribute attributeForProperty in attributesForProperty)
                    {
                        bool found = false;

                        
                        foreach (Attribute attributeAlreadyDefined in attributesAlreadyDefinedList)
                        {

                            if (attributeForProperty is CommonLibrary.CustomAttributes.DatabaseColumnAttribute &&
                                attributeAlreadyDefined is CommonLibrary.CustomAttributes.DatabaseColumnAttribute)
                            {
                                DatabaseColumnAttribute databaseColumnAttribute = (DatabaseColumnAttribute)attributeForProperty;
                                DatabaseColumnAttribute databaseColumnAttributeAlreadyDefined = (DatabaseColumnAttribute)attributeAlreadyDefined;
                                if (databaseColumnAttribute.DatabaseColumn == databaseColumnAttributeAlreadyDefined.DatabaseColumn)
                                {
                                    found = true;
                                    break;
                                }
                            }
                            else
                                        if (attributeForProperty is CommonLibrary.CustomAttributes.SelectAttribute
                                            && attributeAlreadyDefined is CommonLibrary.CustomAttributes.SelectAttribute)
                                        {
                                            SelectAttribute selectAttributeForProperty = (SelectAttribute)attributeForProperty;
                                            SelectAttribute selectAttributeAlreadyDefined = (SelectAttribute)attributeAlreadyDefined;
                                            if (selectAttributeForProperty.SprocName == selectAttributeAlreadyDefined.SprocName)
                                            {
                                                found = true;
                                                break;
                                            }
                                        }
                                        else
                                            if (attributeForProperty is CommonLibrary.CustomAttributes.UpdateAttribute &&
                                                attributeAlreadyDefined is CommonLibrary.CustomAttributes.UpdateAttribute)
                                            {
                                                UpdateAttribute updateAttributeForProperty = (UpdateAttribute)attributeForProperty;
                                                UpdateAttribute updateAttributeAlreadyDefined = (UpdateAttribute)attributeAlreadyDefined;
                                                if (updateAttributeForProperty.SprocName == updateAttributeAlreadyDefined.SprocName)
                                                {
                                                    found = true;
                                                    break;
                                                }
                                            }
                               
                        }
                        if (!found)
                        {
                            attributesNewAndNotAlreadyDefined.Add(attributeForProperty);
                        }
                    }

                    AddPrivateMemberToDto(targetClass, columnName,type);
                    AddPublicPropertyWithExistingAttributesToSprocDto(targetClass,
                                                                      columnName,
                                                                      type,
                                                                      privateMemberName,
                                                                      attributesAlreadyDefinedList,
                                                                      attributesNewAndNotAlreadyDefined);

                }
            }
                          
           
        }

        public void AddPrivateAndPublicMembersToSprocDtoIfNotAlreadyExists(Type typeToReflect,
                                                                   CodeTypeDeclaration targetClass,
                                                                  Data.InformationSchemaColumn column,
                                                                   string privateMemberName,
                                                                   List<Attribute> attributesForProperty)
        {
            PredicateFunctions predicateFunctions = new PredicateFunctions();

            if (typeToReflect == null)
            {
                AddPrivateMemberToDto(targetClass, column);
                AddPublicPropertyToSprocDto(targetClass, column, privateMemberName, attributesForProperty);
            }
            else
            {
                string publicPropertyName = GetPublicMemberName(column.ColumnName, targetClass.Name);

                predicateFunctions.PublicPropertyNameHolder = publicPropertyName;

                PropertyInfo propertyInfo = typeToReflect.GetProperty(publicPropertyName);

                if (propertyInfo == null)
                {
                    //property not found and so we need to create it new
                    AddPrivateMemberToDto(targetClass, column);
                    AddPublicPropertyToSprocDto(targetClass, column, privateMemberName, attributesForProperty);
                }
                else
                {
                    //property was found so we only want to add any attributes it does not yet have
                    //object[] customAttributesAlreadyDefined = propertyInfo.GetCustomAttributes(false);
                    //AssemblyName [] referencedAssemblies = typeToReflect.Assembly.GetReferencedAssemblies();
                    //foreach (AssemblyName assemblyName in referencedAssemblies)
                    //{
                    //    Assembly.Load(assemblyName);
                    //}

                    IList<CustomAttributeData> customAttributesAlreadyDefined = CustomAttributeData.GetCustomAttributes(propertyInfo);
                    List<Attribute> attributesAlreadyDefinedList = new List<Attribute>();
                    List<Attribute> attributesNewAndNotAlreadyDefined = new List<Attribute>();

                    foreach (CustomAttributeData attribute in customAttributesAlreadyDefined)
                    {
                        Attribute attributeToAdd = null;
                        if (attribute.Constructor.ReflectedType.UnderlyingSystemType.FullName == typeof(DatabaseColumnAttribute).FullName)
                        {
                            foreach (CustomAttributeTypedArgument typedArg in attribute.ConstructorArguments)
                            {
                                string colName = typedArg.Value.ToString();
                                attributesAlreadyDefinedList.Add(new DatabaseColumnAttribute(colName));
                            }
                        }
                        else
                            if (attribute.Constructor.ReflectedType.UnderlyingSystemType.FullName == typeof(SelectAttribute).FullName)
                            {
                                foreach (CustomAttributeTypedArgument typedArg in attribute.ConstructorArguments)
                                {
                                    string sprocName = typedArg.Value.ToString();
                                    attributesAlreadyDefinedList.Add(new SelectAttribute(sprocName));
                                }
                            }
                            else
                                if (attribute.Constructor.ReflectedType.UnderlyingSystemType.FullName == typeof(UpdateAttribute).FullName)
                                {
                                    foreach (CustomAttributeTypedArgument typedArg in attribute.ConstructorArguments)
                                    {
                                        string sprocName = typedArg.Value.ToString();
                                        attributesAlreadyDefinedList.Add(new UpdateAttribute(sprocName));
                                    }
                                }

                    }

                    foreach (Attribute attributeForProperty in attributesForProperty)
                    {
                        bool found = false;


                        foreach (Attribute attributeAlreadyDefined in attributesAlreadyDefinedList)
                        {

                            if (attributeForProperty is CommonLibrary.CustomAttributes.DatabaseColumnAttribute &&
                                attributeAlreadyDefined is CommonLibrary.CustomAttributes.DatabaseColumnAttribute)
                            {
                                DatabaseColumnAttribute databaseColumnAttribute = (DatabaseColumnAttribute)attributeForProperty;
                                DatabaseColumnAttribute databaseColumnAttributeAlreadyDefined = (DatabaseColumnAttribute)attributeAlreadyDefined;
                                if (databaseColumnAttribute.DatabaseColumn == databaseColumnAttributeAlreadyDefined.DatabaseColumn)
                                {
                                    found = true;
                                    break;
                                }
                            }
                            else
                                if (attributeForProperty is CommonLibrary.CustomAttributes.SelectAttribute
                                    && attributeAlreadyDefined is CommonLibrary.CustomAttributes.SelectAttribute)
                                {
                                    SelectAttribute selectAttributeForProperty = (SelectAttribute)attributeForProperty;
                                    SelectAttribute selectAttributeAlreadyDefined = (SelectAttribute)attributeAlreadyDefined;
                                    if (selectAttributeForProperty.SprocName == selectAttributeAlreadyDefined.SprocName)
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                else
                                    if (attributeForProperty is CommonLibrary.CustomAttributes.UpdateAttribute &&
                                        attributeAlreadyDefined is CommonLibrary.CustomAttributes.UpdateAttribute)
                                    {
                                        UpdateAttribute updateAttributeForProperty = (UpdateAttribute)attributeForProperty;
                                        UpdateAttribute updateAttributeAlreadyDefined = (UpdateAttribute)attributeAlreadyDefined;
                                        if (updateAttributeForProperty.SprocName == updateAttributeAlreadyDefined.SprocName)
                                        {
                                            found = true;
                                            break;
                                        }
                                    }

                        }
                        if (!found)
                        {
                            attributesNewAndNotAlreadyDefined.Add(attributeForProperty);
                        }
                    }

                    AddPrivateMemberToDto(targetClass, column);
                    AddPublicPropertyWithExistingAttributesToSprocDto(targetClass,
                                                                      column,
                                                                      privateMemberName,
                                                                      attributesAlreadyDefinedList,
                                                                      attributesNewAndNotAlreadyDefined);

                }
            }


        }


        public string GetPublicMemberName(string name, string targetClassName)
        {
            string upperCaseStartLetter = ((String)name)[0].ToString().ToUpper();
            string firstLetterRemoved = name.Remove(0, 1);
            string upperCaseStartLetterName = upperCaseStartLetter + 
                                              firstLetterRemoved;
            if (upperCaseStartLetterName == targetClassName)
            {
                upperCaseStartLetterName += ClassCreationHelperConstants.RESOLVE_DUPLICATE_CLASS_AND_PROPERTY_NAME;

            }
            return upperCaseStartLetterName;
        }

        public string GetPrivateMemberName(string name)
        {
            string lowerCaseStartLetter = ((String)name)[0].ToString().ToLower();
            string firstLetterRemoved = name.Remove(0,1);
            string lowerCaseStartLetterName = lowerCaseStartLetter + firstLetterRemoved;
            string privateMemberVariableName = ClassCreationHelperConstants.UNDERSCORE + 
                                               lowerCaseStartLetterName;

            return privateMemberVariableName;            
        }

        private CodeTypeReference GetTypeReferenceByColumnDataType(Data.InformationSchemaColumn column)
        {
            CodeTypeReference codeTypeReference = null;

            switch (column.DataType)
            {
                case SqlNativeTypeConstants.IMAGE:
                    {
                        codeTypeReference = new CodeTypeReference(typeof(Image));
                        break;
                    }
                case SqlNativeTypeConstants.TEXT:
                    {
                       codeTypeReference = new CodeTypeReference(typeof(String));
                        break;
                    }
                case SqlNativeTypeConstants.TINYINT:
                    {
                       codeTypeReference = new CodeTypeReference(typeof(Byte)); 
                        break;
                    }
                case SqlNativeTypeConstants.SMALLINT:
                    {
                        if (column.IsNullable == ClassCreationHelperConstants.YES)
                        {
                            codeTypeReference = new CodeTypeReference(typeof(Int16?));
                        }
                        else
                        {
                            codeTypeReference = new CodeTypeReference(typeof(Int16));
                        }
                        break;
                    }
                case SqlNativeTypeConstants.INT:
                    {
                        if (column.IsNullable == ClassCreationHelperConstants.YES)
                        {
                            codeTypeReference = new CodeTypeReference(typeof(Int32?));
                        }
                        else
                        {
                            codeTypeReference = new CodeTypeReference(typeof(Int32));

                        }
                        break;
                    }
                case SqlNativeTypeConstants.SMALLDATETIME:
                    {
                        if (column.IsNullable == ClassCreationHelperConstants.YES)
                        {
                            codeTypeReference = new CodeTypeReference(typeof(DateTime?));
                        }
                        else
                        {
                            codeTypeReference = new CodeTypeReference(typeof(DateTime));

                        }
                        break;
                    }
                case SqlNativeTypeConstants.REAL:
                    {
                        if (column.IsNullable == ClassCreationHelperConstants.YES)
                        {
                            codeTypeReference = new CodeTypeReference(typeof(Single?));
                        }
                        else
                        {
                            codeTypeReference = new CodeTypeReference(typeof(Single));

                        }
                        break;
                    }
                case SqlNativeTypeConstants.MONEY:
                    {
                        if (column.IsNullable == ClassCreationHelperConstants.YES)
                        {
                            codeTypeReference = new CodeTypeReference(typeof(Decimal?));
                        }
                        else
                        {
                            codeTypeReference = new CodeTypeReference(typeof(Decimal));
                        }
                        break;
                    }
                case SqlNativeTypeConstants.DATETIME:
                    {
                        if (column.IsNullable == ClassCreationHelperConstants.YES)
                        {
                            codeTypeReference = new CodeTypeReference(typeof(DateTime?));
                        }
                        else
                        {
                            codeTypeReference = new CodeTypeReference(typeof(DateTime));
                        }
                        break;
                    }
                case SqlNativeTypeConstants.FLOAT:
                    {
                        if (column.IsNullable == ClassCreationHelperConstants.YES)
                        {
                            codeTypeReference = new CodeTypeReference(typeof(Double?));
                        }
                        else
                        {
                            codeTypeReference = new CodeTypeReference(typeof(Double));

                        }
                        break;
                    }
                case SqlNativeTypeConstants.NTEXT:
                    {
                        codeTypeReference = new CodeTypeReference(typeof(String));
                        break;
                    }
                case SqlNativeTypeConstants.BIT:
                    {
                        if (column.IsNullable == ClassCreationHelperConstants.YES)
                        {
                            codeTypeReference = new CodeTypeReference(typeof(Boolean?));
                        }
                        else
                        {
                            codeTypeReference = new CodeTypeReference(typeof(Boolean));

                        }
                        break;
                    }
                case SqlNativeTypeConstants.DECIMAL:
                    {
                        if (column.IsNullable == ClassCreationHelperConstants.YES)
                        {
                            codeTypeReference = new CodeTypeReference(typeof(Decimal?));
                        }
                        else
                        {
                            codeTypeReference = new CodeTypeReference(typeof(Decimal));
                        }
                        break;                        
                    }
                case SqlNativeTypeConstants.SMALLMONEY:
                    {
                        if (column.IsNullable == ClassCreationHelperConstants.YES)
                        {
                            codeTypeReference = new CodeTypeReference(typeof(Decimal?));
                        }
                        else
                        {
                            codeTypeReference = new CodeTypeReference(typeof(Decimal));
                        }
                        break;
                    }
                case SqlNativeTypeConstants.BIGINT:
                    {
                        if (column.IsNullable == ClassCreationHelperConstants.YES)
                        {
                            codeTypeReference = new CodeTypeReference(typeof(Int64?));
                        }
                        else
                        {
                            codeTypeReference = new CodeTypeReference(typeof(Int64));

                        }
                        break;
                    }
                case SqlNativeTypeConstants.VARBINARY:
                    {                       
                        codeTypeReference = new CodeTypeReference(typeof(Byte []));
                       
                        break;
                    }
                case SqlNativeTypeConstants.VARCHAR:
                    {
                        codeTypeReference = new CodeTypeReference(typeof(String));
                        break;
                    }
                case SqlNativeTypeConstants.SQL_VARIANT:
                    {
                        codeTypeReference = new CodeTypeReference(typeof(Object));
                        break;
                    }
                case SqlNativeTypeConstants.BINARY:
                    {
                        codeTypeReference = new CodeTypeReference(typeof(Byte []));
                        break;
                    }
                case SqlNativeTypeConstants.CHAR:
                    {
                        codeTypeReference = new CodeTypeReference(typeof(char []));
                        break;
                    }
                case SqlNativeTypeConstants.NVARCHAR:
                    {
                        codeTypeReference = new CodeTypeReference(typeof(String));
                        break;
                    }
                case SqlNativeTypeConstants.NCHAR:
                    {
                        codeTypeReference = new CodeTypeReference(typeof(String));
                        break;
                    }
                case SqlNativeTypeConstants.UNIQUEIDENTIFIER:
                    {
                        codeTypeReference = new CodeTypeReference(typeof(Guid));
                        break;
                    }

                case SqlNativeTypeConstants.NUMERIC:
                    {
                        codeTypeReference = new CodeTypeReference(typeof(Decimal));
                        break;
                    }
                case SqlNativeTypeConstants.XML:
                    {
                        codeTypeReference = new CodeTypeReference(typeof(object));
                        break;
                    }

                default:
                    {
                        codeTypeReference = new CodeTypeReference(typeof(object));
                        break;
                    }

            }
            return codeTypeReference;
        }

        private Type GetTypeByColumnDataType(Data.InformationSchemaColumn column)
        {
            Type type = null;

            switch (column.DataType)
            {
                case SqlNativeTypeConstants.IMAGE:
                    {
                        type = typeof(Image);
                        break;
                    }
                case SqlNativeTypeConstants.TEXT:
                    {
                        type = typeof(String);
                        break;
                    }
                case SqlNativeTypeConstants.TINYINT:
                    {
                        type = typeof(Byte);
                        break;
                    }
                case SqlNativeTypeConstants.SMALLINT:
                    {
                        if (column.IsNullable == ClassCreationHelperConstants.YES)
                        {
                            type = typeof(System.Nullable<Int16>);
                        }
                        else
                        {
                            type = typeof(System.Int16);
                        }
                        break;
                    }
                case SqlNativeTypeConstants.INT:
                    {
                        if (column.IsNullable == ClassCreationHelperConstants.YES)
                        {
                            type = typeof(System.Nullable<Int32>);
                        }
                        else
                        {
                            type = typeof(System.Int32);

                        }
                        break;
                    }
                case SqlNativeTypeConstants.SMALLDATETIME:
                    {
                        if (column.IsNullable == ClassCreationHelperConstants.YES)
                        {
                            type = typeof(System.Nullable<System.DateTime>);
                        }
                        else
                        {
                            type = typeof(System.DateTime);

                        }
                        break;
                    }
                case SqlNativeTypeConstants.REAL:
                    {
                        if (column.IsNullable == ClassCreationHelperConstants.YES)
                        {
                            type = typeof(System.Nullable<System.Single>);
                        }
                        else
                        {
                            type = typeof(System.Single);

                        }
                        break;
                    }
                case SqlNativeTypeConstants.MONEY:
                    {
                        if (column.IsNullable == ClassCreationHelperConstants.YES)
                        {
                            type = typeof(System.Nullable<System.Decimal>);
                        }
                        else
                        {
                            type = typeof(System.Decimal);
                        }
                        break;
                    }
                case SqlNativeTypeConstants.DATETIME:
                    {
                        if (column.IsNullable == ClassCreationHelperConstants.YES)
                        {
                            type = typeof(System.Nullable<System.DateTime>);
                        }
                        else
                        {
                            type = typeof(System.DateTime);
                        }
                        break;
                    }
                case SqlNativeTypeConstants.FLOAT:
                    {
                        if (column.IsNullable == ClassCreationHelperConstants.YES)
                        {
                            type = typeof(System.Nullable<System.Double>);
                        }
                        else
                        {
                            type = typeof(System.Double);

                        }
                        break;
                    }
                case SqlNativeTypeConstants.NTEXT:
                    {
                        type = typeof(String);
                        break;
                    }
                case SqlNativeTypeConstants.BIT:
                    {
                        if (column.IsNullable == ClassCreationHelperConstants.YES)
                        {
                            type = typeof(System.Nullable<System.Boolean>);
                        }
                        else
                        {
                            type = typeof(System.Boolean);

                        }
                        break;
                    }
                case SqlNativeTypeConstants.DECIMAL:
                    {
                        if (column.IsNullable == ClassCreationHelperConstants.YES)
                        {
                            type = typeof(System.Nullable<System.Decimal>);
                        }
                        else
                        {
                            type = typeof(System.Decimal);
                        }
                        break;
                    }
                case SqlNativeTypeConstants.SMALLMONEY:
                    {
                        if (column.IsNullable == ClassCreationHelperConstants.YES)
                        {
                            type = typeof(System.Nullable<System.Decimal>);
                        }
                        else
                        {
                            type = typeof(System.Decimal);
                        }
                        break;
                    }
                case SqlNativeTypeConstants.BIGINT:
                    {
                        if (column.IsNullable == ClassCreationHelperConstants.YES)
                        {
                            type = typeof(System.Nullable<System.Int64>);
                        }
                        else
                        {
                            type = typeof(System.Int64);

                        }
                        break;
                    }
                case SqlNativeTypeConstants.VARBINARY:
                    {
                        type = typeof(Byte[]);

                        break;
                    }
                case SqlNativeTypeConstants.VARCHAR:
                    {
                        type = typeof(String);
                        break;
                    }
                case SqlNativeTypeConstants.SQL_VARIANT:
                    {
                        type = typeof(Object);
                        break;
                    }
                case SqlNativeTypeConstants.BINARY:
                    {
                        type = typeof(Byte[]);
                        break;
                    }
                case SqlNativeTypeConstants.CHAR:
                    {
                       
                            type = typeof(char[]);
                        
                        break;
                    }
                case SqlNativeTypeConstants.NVARCHAR:
                    {
                        type = typeof(String);
                        break;
                    }
                case SqlNativeTypeConstants.NCHAR:
                    {
                        type = typeof(String);
                        break;
                    }
                case SqlNativeTypeConstants.UNIQUEIDENTIFIER:
                    {
                        type = typeof(Guid);
                        break;
                    }

                case SqlNativeTypeConstants.NUMERIC:
                    {
                        type = typeof(Decimal);
                        break;
                    }
                case SqlNativeTypeConstants.XML:
                    {
                        type = typeof(object);
                        break;
                    }
                default:
                    {
                        type = typeof(object);
                        break;
                    }

            }
            return type;
        }


        public void AddPrivateMemberToDto(CodeTypeDeclaration targetClass,
                                     Data.InformationSchemaColumn column)
        {
            CodeMemberField columnField = new CodeMemberField();
            columnField.Attributes = MemberAttributes.Private;
            columnField.Name = GetPrivateMemberName(column.ColumnName);
            columnField.Type = GetTypeReferenceByColumnDataType(column);            
            targetClass.Members.Add(columnField);           

        }

        public void AddPrivateMemberToDto(CodeTypeDeclaration targetClass,
                                    string columnName,
                                    SqlDbType type)
        {
            CodeMemberField columnField = new CodeMemberField();
            columnField.Attributes = MemberAttributes.Private;
            columnField.Name = GetPrivateMemberName(columnName);
            columnField.Type = new CodeTypeReference(DatabaseHelperMethods.GetTypeFromSqlDbType(type));
            targetClass.Members.Add(columnField);

        }

        public void AddPrivateMemberToDto(CodeTypeDeclaration targetClass,
                                          MemberInfo privateMember)
        {
            CodeMemberField privateField = new CodeMemberField();
            privateField.Attributes = MemberAttributes.Private;
            privateField.Name = privateMember.Name;
            privateField.Type = new CodeTypeReference(privateMember.DeclaringType);
            targetClass.Members.Add(privateField); 
        }

        public void AddPrivateMember(CodeTypeDeclaration targetClass,
                                     string memberName,
                                     Type type)
        {
            CodeMemberField privateMember = new CodeMemberField();
            privateMember.Attributes = MemberAttributes.Private;
            privateMember.Name = memberName;
            privateMember.Type = new CodeTypeReference(type);
            targetClass.Members.Add(privateMember);          

            privateMember.InitExpression = new CodeObjectCreateExpression(type);
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

        public void AddEmptyConstructor(MemberAttributes memberAttributes,           
            CodeTypeDeclaration targetClass)
        {
            CodeConstructor constructor = new CodeConstructor();
            constructor.Attributes = memberAttributes;          
            targetClass.Members.Add(constructor);

        }

        public CodeParameterDeclarationExpression GetParameterDeclarationExpression(Type type, 
                                                                                    string parameterName)
        {
            CodeParameterDeclarationExpression codeParameterDeclarationExpression =
                new CodeParameterDeclarationExpression(type, parameterName);
            return codeParameterDeclarationExpression;
        }


        public void AddPublicPropertyToDto(CodeTypeDeclaration targetClass,
                                      Data.InformationSchemaColumn column,
                                      string privateMemberVariable,
                                      Attribute attribute)       
             
        {
            CodeMemberProperty columnProperty = new CodeMemberProperty();
            columnProperty.Attributes = MemberAttributes.Public;
            columnProperty.Name = GetPublicMemberName(column.ColumnName,targetClass.Name);
            columnProperty.Type = GetTypeReferenceByColumnDataType(column);

            CodeThisReferenceExpression codeThisReferenceExpression = new
               CodeThisReferenceExpression();

            CodeFieldReferenceExpression codefieldReferenceExpression = 
                new CodeFieldReferenceExpression(codeThisReferenceExpression,privateMemberVariable);
         

            CodeMethodReturnStatement codeMethodReturnStatement =
                new CodeMethodReturnStatement(codefieldReferenceExpression);

            CodeAssignStatement codeAssignStatement =
                new CodeAssignStatement(codefieldReferenceExpression, new CodePropertySetValueReferenceExpression());

            columnProperty.GetStatements.Add(codeMethodReturnStatement);
            columnProperty.SetStatements.Add(codeAssignStatement);

            columnProperty.HasGet = true;
            columnProperty.HasSet = true;           

            CodeAttributeDeclaration codeAttributeDeclaration;
            
            if(attribute is CommonLibrary.CustomAttributes.DatabaseColumnAttribute)
            {                
               codeAttributeDeclaration =
                new CodeAttributeDeclaration(GetAttributeTypeReferenceByAttributeForDto(attribute),
                new CodeAttributeArgument(new CodePrimitiveExpression(column.ColumnName)));
            }
            else
            {
                codeAttributeDeclaration = 
                    new CodeAttributeDeclaration(GetAttributeTypeReferenceByAttributeForDto(attribute));
            }

           
            columnProperty.CustomAttributes.Add(codeAttributeDeclaration);
            targetClass.Members.Add(columnProperty);       
        }

        public void AddPublicPropertyToDto(CodeTypeDeclaration targetClass,
                                           PropertyInfo publicProperty,
                                           MemberInfo privateMember,
                                           string sprocName)
        {
            CodeMemberProperty property = new CodeMemberProperty();
            property.Attributes = MemberAttributes.Public;
            property.Name = publicProperty.Name;
            property.Type = new CodeTypeReference(publicProperty.DeclaringType);

            CodeThisReferenceExpression codeThisReferenceExpression = new
               CodeThisReferenceExpression();

            CodeFieldReferenceExpression codefieldReferenceExpression =
                new CodeFieldReferenceExpression(codeThisReferenceExpression, privateMember.Name);


            CodeMethodReturnStatement codeMethodReturnStatement =
                new CodeMethodReturnStatement(codefieldReferenceExpression);

            CodeAssignStatement codeAssignStatement =
                new CodeAssignStatement(codefieldReferenceExpression, new CodePropertySetValueReferenceExpression());

            property.GetStatements.Add(codeMethodReturnStatement);
            property.SetStatements.Add(codeAssignStatement);

            property.HasGet = true;
            property.HasSet = true;

            foreach (Attribute attribute in property.CustomAttributes)
            {
                CodeAttributeDeclaration codeAttributeDeclaration = null;

                if (attribute is CommonLibrary.CustomAttributes.SelectAttribute)
                {
                    codeAttributeDeclaration =
                     new CodeAttributeDeclaration(GetAttributeTypeReferenceByAttributeForSprocDto(attribute),
                     new CodeAttributeArgument(new CodePrimitiveExpression(sprocName)));
                }
               
                property.CustomAttributes.Add(codeAttributeDeclaration);
            }
            targetClass.Members.Add(property);
        }

        

        public void AddPublicPropertyToDto(CodeTypeDeclaration targetClass,
                              Data.InformationSchemaColumn column,
                              string privateMemberVariable,
                              List<Attribute> attributes)
        {
            CodeMemberProperty columnProperty = new CodeMemberProperty();
            columnProperty.Attributes = MemberAttributes.Public;
            columnProperty.Name = GetPublicMemberName(column.ColumnName, targetClass.Name);
            columnProperty.Type = GetTypeReferenceByColumnDataType(column);

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

            foreach (Attribute attribute in attributes)
            {
                CodeAttributeDeclaration codeAttributeDeclaration;

                if (attribute is CommonLibrary.CustomAttributes.DatabaseColumnAttribute)
                {
                    codeAttributeDeclaration =
                     new CodeAttributeDeclaration(GetAttributeTypeReferenceByAttributeForDto(attribute),
                     new CodeAttributeArgument(new CodePrimitiveExpression(column.ColumnName)));
                }
                else
                {
                    codeAttributeDeclaration =
                        new CodeAttributeDeclaration(GetAttributeTypeReferenceByAttributeForDto(attribute));
                }
                columnProperty.CustomAttributes.Add(codeAttributeDeclaration);
            }
            targetClass.Members.Add(columnProperty);
        }

        public void AddPublicPropertyWithExistingAttributesToSprocDto(CodeTypeDeclaration targetClass,
                      Data.InformationSchemaColumn column,
                      string privateMemberVariable,
                      List<Attribute> existingAttributes,
                      List<Attribute> attributesToAdd)
        {
            List<Attribute> concatenatedAttributeList = new List<Attribute>();
            concatenatedAttributeList.AddRange(existingAttributes);
            concatenatedAttributeList.AddRange(attributesToAdd);
            AddPublicPropertyToSprocDto(targetClass,
                                        column,
                                        privateMemberVariable,
                                        concatenatedAttributeList);
        }

        public void AddPublicPropertyWithExistingAttributesToSprocDto(CodeTypeDeclaration targetClass,
              string columnName,
              SqlDbType type,
              string privateMemberVariable,
              List<Attribute> existingAttributes,
              List<Attribute> attributesToAdd)
        {
            List<Attribute> concatenatedAttributeList = new List<Attribute>();
            concatenatedAttributeList.AddRange(existingAttributes);
            concatenatedAttributeList.AddRange(attributesToAdd);
            AddPublicPropertyToSprocDto(targetClass,
                                        columnName,
                                        type,
                                        privateMemberVariable,
                                        concatenatedAttributeList);
        }

        public void AddPublicPropertyToSprocDto(CodeTypeDeclaration targetClass,
                      Data.InformationSchemaColumn column,
                      string privateMemberVariable,
                      List<Attribute> attributes)
        {
            CodeMemberProperty columnProperty = new CodeMemberProperty();
            columnProperty.Attributes = MemberAttributes.Public;
            columnProperty.Name = GetPublicMemberName(column.ColumnName, targetClass.Name);
            columnProperty.Type = GetTypeReferenceByColumnDataType(column);

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

            foreach (Attribute attribute in attributes)
            {
                CodeAttributeDeclaration codeAttributeDeclaration;
                string sprocName = string.Empty;

                if (attribute is CommonLibrary.CustomAttributes.SelectAttribute)
                {
                    sprocName = ((SelectAttribute)attribute).SprocName;
                    codeAttributeDeclaration =
                                 new CodeAttributeDeclaration(GetAttributeTypeReferenceByAttributeForSprocDto(attribute),
                                 new CodeAttributeArgument(new CodePrimitiveExpression(sprocName)));
                    columnProperty.CustomAttributes.Add(codeAttributeDeclaration);

                }
                else
                    if (attribute is CommonLibrary.CustomAttributes.UpdateAttribute)
                    {
                        sprocName = ((UpdateAttribute)attribute).SprocName;
                        codeAttributeDeclaration =
                             new CodeAttributeDeclaration(GetAttributeTypeReferenceByAttributeForSprocDto(attribute),
                             new CodeAttributeArgument(new CodePrimitiveExpression(sprocName)));
                        columnProperty.CustomAttributes.Add(codeAttributeDeclaration);
                    }
                    else if (attribute is CommonLibrary.CustomAttributes.DatabaseColumnAttribute)
                    {
                        string columnName = ((DatabaseColumnAttribute)attribute).DatabaseColumn;
                        codeAttributeDeclaration =
                             new CodeAttributeDeclaration(GetAttributeTypeReferenceByAttributeForSprocDto(attribute),
                             new CodeAttributeArgument(new CodePrimitiveExpression(columnName)));
                        columnProperty.CustomAttributes.Add(codeAttributeDeclaration);
                    }
                    else if (attribute is InputSprocParameterAttribute)
                    {
                        codeAttributeDeclaration =
                            new CodeAttributeDeclaration(GetAttributeTypeReferenceByAttributeForSprocDto(attribute));
                        columnProperty.CustomAttributes.Add(codeAttributeDeclaration);
                    }              
               
                
            }
            targetClass.Members.Add(columnProperty);
        }

        public void AddPublicPropertyToSprocDto(CodeTypeDeclaration targetClass,
              string columnName,
              SqlDbType type,
              string privateMemberVariable,
              List<Attribute> attributes)
        {
            CodeMemberProperty columnProperty = new CodeMemberProperty();
            columnProperty.Attributes = MemberAttributes.Public;
            columnProperty.Name = GetPublicMemberName(columnName, targetClass.Name);
            columnProperty.Type = new CodeTypeReference(DatabaseHelperMethods.GetTypeFromSqlDbType(type));

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

            foreach (Attribute attribute in attributes)
            {
                CodeAttributeDeclaration codeAttributeDeclaration;
                string sprocName = string.Empty;

                if (attribute is CommonLibrary.CustomAttributes.SelectAttribute)
                {
                    sprocName = ((SelectAttribute)attribute).SprocName;
                    codeAttributeDeclaration =
                                 new CodeAttributeDeclaration(GetAttributeTypeReferenceByAttributeForSprocDto(attribute),
                                 new CodeAttributeArgument(new CodePrimitiveExpression(sprocName)));
                    columnProperty.CustomAttributes.Add(codeAttributeDeclaration);

                }
                else
                    if (attribute is CommonLibrary.CustomAttributes.UpdateAttribute)
                    {
                        sprocName = ((UpdateAttribute)attribute).SprocName;
                        codeAttributeDeclaration =
                             new CodeAttributeDeclaration(GetAttributeTypeReferenceByAttributeForSprocDto(attribute),
                             new CodeAttributeArgument(new CodePrimitiveExpression(sprocName)));
                        columnProperty.CustomAttributes.Add(codeAttributeDeclaration);
                    }
                    else if (attribute is CommonLibrary.CustomAttributes.DatabaseColumnAttribute)
                    {
                        string columnNameFromAttribute = ((DatabaseColumnAttribute)attribute).DatabaseColumn;
                        codeAttributeDeclaration =
                             new CodeAttributeDeclaration(GetAttributeTypeReferenceByAttributeForSprocDto(attribute),
                             new CodeAttributeArgument(new CodePrimitiveExpression(columnNameFromAttribute)));
                        columnProperty.CustomAttributes.Add(codeAttributeDeclaration);
                    }
                    else if (attribute is InputSprocParameterAttribute)
                    {
                        codeAttributeDeclaration =
                            new CodeAttributeDeclaration(GetAttributeTypeReferenceByAttributeForSprocDto(attribute));
                        columnProperty.CustomAttributes.Add(codeAttributeDeclaration);
                    }


            }
            targetClass.Members.Add(columnProperty);
        }


        public void AddPublicPropertyToSprocDto(CodeTypeDeclaration targetClass,
              Data.InformationSchemaColumn column,
              string privateMemberVariable,
              Attribute attribute)
        {
            CodeMemberProperty columnProperty = new CodeMemberProperty();
            columnProperty.Attributes = MemberAttributes.Public;
            columnProperty.Name = GetPublicMemberName(column.ColumnName, targetClass.Name);
            columnProperty.Type = GetTypeReferenceByColumnDataType(column);

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

           
                CodeAttributeDeclaration codeAttributeDeclaration;
                string sprocName = string.Empty;

                if (attribute is CommonLibrary.CustomAttributes.SelectAttribute)
                {
                    sprocName = ((SelectAttribute)attribute).SprocName;
                }
                else
                    if (attribute is CommonLibrary.CustomAttributes.UpdateAttribute)
                    {
                        sprocName = ((UpdateAttribute)attribute).SprocName;
                    }

                codeAttributeDeclaration =
                 new CodeAttributeDeclaration(GetAttributeTypeReferenceByAttributeForSprocDto(attribute),
                 new CodeAttributeArgument(new CodePrimitiveExpression(sprocName)));

                columnProperty.CustomAttributes.Add(codeAttributeDeclaration);
            
            targetClass.Members.Add(columnProperty);
        }


        public void AddPublicPropertyToDto(CodeTypeDeclaration targetClass,
                              Data.InformationSchemaColumn column,
                              string privateMemberVariable)
        {
            CodeMemberProperty columnProperty = new CodeMemberProperty();
            columnProperty.Attributes = MemberAttributes.Public;
            columnProperty.Name = GetPublicMemberName(column.ColumnName,targetClass.Name);
            columnProperty.Type = GetTypeReferenceByColumnDataType(column);

            CodeThisReferenceExpression codeThisReferenceExpression = 
                new CodeThisReferenceExpression();

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

        public CodeTypeReference GetAttributeTypeReferenceByAttributeForDto(Attribute attribute)
        {
            CodeTypeReference codeTypeReference = null;

            if (attribute is CommonLibrary.CustomAttributes.PrimaryKey)
            {
                codeTypeReference = new CodeTypeReference(typeof(CommonLibrary.CustomAttributes.PrimaryKey));                
            }
            else
                if (attribute is CommonLibrary.CustomAttributes.Unique)
                {
                    codeTypeReference = new CodeTypeReference(typeof(CommonLibrary.CustomAttributes.Unique));
                }
                else
                    if (attribute is CommonLibrary.CustomAttributes.ForeignKey)
                    {
                        codeTypeReference = new CodeTypeReference(typeof(CommonLibrary.CustomAttributes.ForeignKey));
                    }
            if (attribute is CommonLibrary.CustomAttributes.DatabaseColumnAttribute)
            {
                codeTypeReference = new CodeTypeReference(typeof(CommonLibrary.CustomAttributes.DatabaseColumnAttribute));
                
            }
            if (attribute is CommonLibrary.CustomAttributes.TableNameAttribute)
            {
                codeTypeReference = new CodeTypeReference(typeof(CommonLibrary.CustomAttributes.TableNameAttribute));
            }

            return codeTypeReference;
        }

        public CodeTypeReference GetAttributeTypeReferenceByAttributeForSprocDto(Attribute attribute)
        {
            CodeTypeReference codeTypeReference = null;

            if (attribute is CommonLibrary.CustomAttributes.SelectAttribute)
            {
                codeTypeReference = new CodeTypeReference(typeof(CommonLibrary.CustomAttributes.SelectAttribute));
            }
            else
                if (attribute is CommonLibrary.CustomAttributes.UpdateAttribute)
                {
                    codeTypeReference = new CodeTypeReference(typeof(CommonLibrary.CustomAttributes.UpdateAttribute));
                }     
                else
                    if (attribute is CommonLibrary.CustomAttributes.DatabaseColumnAttribute)
                    {
                        codeTypeReference = new CodeTypeReference(typeof(CommonLibrary.CustomAttributes.DatabaseColumnAttribute));
                    }
                    else if
                        (attribute is CommonLibrary.CustomAttributes.InputSprocParameterAttribute)
                    {
                        codeTypeReference = new CodeTypeReference(typeof(CommonLibrary.CustomAttributes.InputSprocParameterAttribute));
                    }

            return codeTypeReference;
        }       

        public List.InformationSchemaColumn GetColumnsNotInSet(List.InformationSchemaColumn listOfColumnsToRemoveIfFound,
                                                               List.InformationSchemaColumn listOfAllColumns)
        {
            PredicateFunctions predicateFunctions = new PredicateFunctions();

            foreach (Data.InformationSchemaColumn column in listOfColumnsToRemoveIfFound)
            {
                predicateFunctions.ColumnNameHolder = column.ColumnName;
                int indexOfFoundItem = listOfAllColumns.IndexOf(listOfAllColumns.Find(predicateFunctions.FindInformationSchemaColumn));
                if (indexOfFoundItem > -1)
                {
                    listOfAllColumns.Remove(listOfAllColumns[indexOfFoundItem]);
                }
            }

            return listOfAllColumns;
        }
        

        public List<string> GetDtoNamespaceList()
        {
            List<string> namespaceList = new List<string>();
            namespaceList.Add(ClassCreationHelperConstants.SYSTEM);
            namespaceList.Add(ClassCreationHelperConstants.SYSTEM + ClassCreationHelperConstants.DOT_OPERATOR + ClassCreationHelperConstants.COLLECTIONS + ClassCreationHelperConstants.DOT_OPERATOR + ClassCreationHelperConstants.GENERIC);
            namespaceList.Add(ClassCreationHelperConstants.SYSTEM + ClassCreationHelperConstants.DOT_OPERATOR + ClassCreationHelperConstants.TEXT);
            namespaceList.Add(ClassCreationHelperConstants.COMMONLIBRARY);
            return namespaceList;
        }

        public List<string> GetInputDtoNamespaceList()
        {
            List<string> namespaceList = new List<string>();
            namespaceList.Add(ClassCreationHelperConstants.SYSTEM);
            namespaceList.Add(ClassCreationHelperConstants.SYSTEM + ClassCreationHelperConstants.DOT_OPERATOR + ClassCreationHelperConstants.COLLECTIONS + ClassCreationHelperConstants.DOT_OPERATOR + ClassCreationHelperConstants.GENERIC);
            namespaceList.Add(ClassCreationHelperConstants.SYSTEM + ClassCreationHelperConstants.DOT_OPERATOR + ClassCreationHelperConstants.TEXT);
            namespaceList.Add(ClassCreationHelperConstants.COMMONLIBRARY + 
                              ClassCreationHelperConstants.DOT_OPERATOR +
                              "CustomAttributes");
            return namespaceList;
        }

        public List<string> GetResultNamespaceList(string enclosingApplicationNamespace)
        {
            List<string> namespaceList = new List<string>();
            namespaceList.Add(ClassCreationHelperConstants.SYSTEM);

            namespaceList.Add(ClassCreationHelperConstants.SYSTEM +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.COLLECTIONS +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.GENERIC);

            namespaceList.Add(ClassCreationHelperConstants.SYSTEM +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.DATA);

            namespaceList.Add(ClassCreationHelperConstants.SYSTEM +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.DATA +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.SQLCLIENT);

            namespaceList.Add(ClassCreationHelperConstants.SYSTEM +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.TEXT);

            namespaceList.Add(enclosingApplicationNamespace +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.DATA +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.SPROC_TABLE +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.DTO);

            namespaceList.Add(ClassCreationHelperConstants.COMMONLIBRARY +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.BASE +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.DATABASE);

            return namespaceList;
        }

        public List<string> GetDataAccessClassNamespaceList(string enclosingApplicationNamespace)
        {
            List<string> namespaceList = new List<string>();
            namespaceList.Add(ClassCreationHelperConstants.SYSTEM);

            namespaceList.Add(ClassCreationHelperConstants.SYSTEM +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.COLLECTIONS +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.GENERIC);

            namespaceList.Add(ClassCreationHelperConstants.SYSTEM +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.DATA);

            namespaceList.Add(ClassCreationHelperConstants.SYSTEM +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.DATA +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.SQLCLIENT);

            namespaceList.Add(ClassCreationHelperConstants.SYSTEM +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.TEXT);

            //namespaceList.Add(enclosingApplicationNamespace +
            //    ClassCreationHelperConstants.DOT_OPERATOR +
            //    ClassCreationHelperConstants.DATA +
            //    ClassCreationHelperConstants.DOT_OPERATOR +
            //    ClassCreationHelperConstants.SPROC_TABLE +
            //    ClassCreationHelperConstants.DOT_OPERATOR +
            //    ClassCreationHelperConstants.DTO);

            namespaceList.Add(ClassCreationHelperConstants.COMMONLIBRARY +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.BASE +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.DATABASE);

            return namespaceList;
        }

        public List<string> GetListNamespaceList(string enclosingApplicationNamespace)
        {
            List<string> namespaceList = new List<string>();
            namespaceList.Add(ClassCreationHelperConstants.SYSTEM);

            namespaceList.Add(ClassCreationHelperConstants.SYSTEM + 
                ClassCreationHelperConstants.DOT_OPERATOR + 
                ClassCreationHelperConstants.COLLECTIONS + 
                ClassCreationHelperConstants.DOT_OPERATOR + 
                ClassCreationHelperConstants.GENERIC);

            namespaceList.Add(ClassCreationHelperConstants.SYSTEM +
                ClassCreationHelperConstants.DOT_OPERATOR + 
                ClassCreationHelperConstants.DATA);

            namespaceList.Add(ClassCreationHelperConstants.SYSTEM + 
                ClassCreationHelperConstants.DOT_OPERATOR + 
                ClassCreationHelperConstants.DATA + 
                ClassCreationHelperConstants.DOT_OPERATOR + 
                ClassCreationHelperConstants.SQLCLIENT);

            namespaceList.Add(ClassCreationHelperConstants.SYSTEM +
                ClassCreationHelperConstants.DOT_OPERATOR +
                ClassCreationHelperConstants.TEXT);

            //namespaceList.Add(enclosingApplicationNamespace + 
            //    ClassCreationHelperConstants.DOT_OPERATOR + 
            //    ClassCreationHelperConstants.DATA + 
            //    ClassCreationHelperConstants.DOT_OPERATOR + 
            //    ClassCreationHelperConstants.SINGLE_TABLE + 
            //    ClassCreationHelperConstants.DOT_OPERATOR +
            //    ClassCreationHelperConstants.DTO);

            namespaceList.Add(ClassCreationHelperConstants.COMMONLIBRARY + ClassCreationHelperConstants.DOT_OPERATOR + ClassCreationHelperConstants.BASE + ClassCreationHelperConstants.DOT_OPERATOR + ClassCreationHelperConstants.DATABASE);
            
            return namespaceList;
        }

        public bool IsPrimaryKeyColumn(Data.InformationSchemaColumn column,
                                       List.InformationSchemaConstraintColumnUsage constraintUsage,
                                       List.InformationSchemaTableConstraint tableConstraints)
        {
            PredicateFunctions predicateFunctions = new PredicateFunctions();
            predicateFunctions.TableNameHolder = column.TableName;

            bool isPrimaryKeyColumn = false;
            foreach (Data.InformationSchemaTableConstraint tableConstraint in tableConstraints)
            {
                predicateFunctions.ConstraintNameHolder = tableConstraint.ConstraintName;
                if (tableConstraint.ConstraintType == TableConstraintTypeConstants.PRIMARY_KEY)
                {
                    if (constraintUsage.FindAll(predicateFunctions.FindConstraintColumnUsageByTableNameAndConstraintName).Count > 0)
                    {
                        isPrimaryKeyColumn = true;
                        break;
                    }
                }
            }
            return isPrimaryKeyColumn;
        }

        public List.InformationSchemaColumn GetPrimaryKeyColumns(Dictionary<Data.InformationSchemaColumn,
                                         List.InformationSchemaConstraintColumnUsage> columnsToConstraintUsage,
                                         List.InformationSchemaTableConstraint tableConstraints)
        {
            List.InformationSchemaColumn primaryKeyColumns = new List.InformationSchemaColumn();
            PredicateFunctions predicateFunctions = new PredicateFunctions();

            foreach (Data.InformationSchemaTableConstraint tableConstraint in tableConstraints)
            {
                predicateFunctions.TableNameHolder = tableConstraint.TableName;
                predicateFunctions.ConstraintNameHolder = tableConstraint.ConstraintName;
                if (tableConstraint.ConstraintType == TableConstraintTypeConstants.PRIMARY_KEY)
                {
                    foreach (KeyValuePair<Data.InformationSchemaColumn, List.InformationSchemaConstraintColumnUsage> kvp in columnsToConstraintUsage)
                    {
                        if (kvp.Value.FindAll(predicateFunctions.FindConstraintColumnUsageByTableNameAndConstraintName).Count > 0)
                        {
                            primaryKeyColumns.Add(kvp.Key);
                        }
                    }
                }

            }
            return primaryKeyColumns;
        }

        public List.InformationSchemaColumn GetForeignKeyColumns(Dictionary<Data.InformationSchemaColumn,
                                 List.InformationSchemaConstraintColumnUsage> columnsToConstraintUsage,
                                 List.InformationSchemaTableConstraint tableConstraints)
        {
            List.InformationSchemaColumn foreignKeyColumns = new List.InformationSchemaColumn();
            PredicateFunctions predicateFunctions = new PredicateFunctions();

            foreach (Data.InformationSchemaTableConstraint tableConstraint in tableConstraints)
            {
                predicateFunctions.TableNameHolder = tableConstraint.TableName;
                predicateFunctions.ConstraintNameHolder = tableConstraint.ConstraintName;
                if (tableConstraint.ConstraintType == TableConstraintTypeConstants.FOREIGN_KEY)
                {
                    foreach (KeyValuePair<Data.InformationSchemaColumn, List.InformationSchemaConstraintColumnUsage> kvp in columnsToConstraintUsage)
                    {
                        if (kvp.Value.FindAll(predicateFunctions.FindConstraintColumnUsageByTableNameAndConstraintName).Count > 0)
                        {
                            foreignKeyColumns.Add(kvp.Key);
                        }
                    }
                }

            }
            return foreignKeyColumns;
        }

        public List.InformationSchemaColumn GetUniqueConstraintColumns(Dictionary<Data.InformationSchemaColumn,
                         List.InformationSchemaConstraintColumnUsage> columnsToConstraintUsage,
                         List.InformationSchemaTableConstraint tableConstraints)
        {
            List.InformationSchemaColumn uniqueKeyColumns = new List.InformationSchemaColumn();
            PredicateFunctions predicateFunctions = new PredicateFunctions();

            foreach (Data.InformationSchemaTableConstraint tableConstraint in tableConstraints)
            {
                predicateFunctions.TableNameHolder = tableConstraint.TableName;
                predicateFunctions.ConstraintNameHolder = tableConstraint.ConstraintName;
                if (tableConstraint.ConstraintType == TableConstraintTypeConstants.UNIQUE)
                {
                    foreach (KeyValuePair<Data.InformationSchemaColumn, List.InformationSchemaConstraintColumnUsage> kvp in columnsToConstraintUsage)
                    {
                        if (kvp.Value.FindAll(predicateFunctions.FindConstraintColumnUsageByTableNameAndConstraintName).Count > 0)
                        {
                            uniqueKeyColumns.Add(kvp.Key);
                        }
                    }
                }

            }
            return uniqueKeyColumns;
        }

    }
}
