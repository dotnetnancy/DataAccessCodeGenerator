using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Server;

using SprocDataLayerGenerator.BusinessObjects;
using SprocDataLayerGenerator.Data;
using BusinessLayerGenerator.BusinessObjects;

namespace TestSprocGenerator
{
    public partial class Form1 : Form
    {
        List<StoredProcedure> _sprocsGenerated = new List<StoredProcedure>();
        List<StoredProcedure> _sprocsThatCouldNotBeGenerated = new List<StoredProcedure>();
        SprocGenerator _sprocGenerator = null;
        MetaInformationSchemaManager _metaInformationSchemaManager;
        DataLayerGenerator _sprocDataLayerGenerator;
        List<StoredProcedure> _customSprocs = new List<StoredProcedure>();
        CommonLibrary.DatabaseSmoObjectsAndSettings _databaseSmoObjectsAndSettings = null;

        Dictionary<string, string> _mainSprocNameToInputDto = new Dictionary<string, string>();
        Dictionary<string, List<string>> _sprocToDtoListsUsed = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> _sprocToInputDtosUsed = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> _sprocToDtosUsed = new Dictionary<string, List<string>>();
        Dictionary<string, string> _standaloneSprocToInputDto = new Dictionary<string, string>();

        DataLayerGenerator _dataLayerGenerator;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            string databaseName = txtDatabaseName.Text;
            string dataSource = txtDataSource.Text;
            string initialCatalog = txtInitialCatalog.Text;
            string userId = txtUserId.Text;
            string password = txtPassword.Text;
            bool trustedConnection = chkTrustedConnection.Checked;
            string schema = txtSchema.Text;

            try
            {

                SprocGenerator sprocGenerator = new SprocGenerator(databaseName,
                                                                   dataSource,
                                                                   initialCatalog,
                                                                   userId,
                                                                   password,
                                                                   trustedConnection,
                                                                   schema);
                _sprocGenerator = sprocGenerator;

                MetaInformationSchemaManager metaInformationSchemaManager =
                    new MetaInformationSchemaManager(databaseName,
                    dataSource,
                    initialCatalog,
                    userId,
                    password,
                    trustedConnection,
                    schema);

                SprocDataLayerGenerator.PredicateFunctions predicateFunctions =
                    new SprocDataLayerGenerator.PredicateFunctions();

                List<string> tablesToGen = new List<string>();
                List<MetaInformationSchema> schemasToGen = new List<MetaInformationSchema>();

                if (this.treeAvailableTables.Nodes.Count > 0)
                {
                    foreach (TreeNode node in treeAvailableTables.Nodes)
                    {
                        if (node.Checked)
                        {
                            tablesToGen.Add(node.Text);
                        }
                    }
                }

                if (tablesToGen.Count > 0)
                {
                    foreach (string tableName in tablesToGen)
                    {
                        predicateFunctions.TableNameHolder = tableName;
                        schemasToGen.AddRange(metaInformationSchemaManager.MetaDataList.FindAll(predicateFunctions.FindMetaInformationSchemaByTableName));
                    }
                }

                List<StoredProcedure> sprocsGenerated = sprocGenerator.GetStoredProcedures(schemasToGen);
                //StringBuilder sb = new StringBuilder();
                //foreach (StoredProcedure sproc in sprocsGenerated)
                //{
                //    sb.Append(sproc.TextHeader);
                //    sb.Append(sproc.TextBody);
                //    sb.Append(Environment.NewLine);
                //    sb.Append(Environment.NewLine);
                //    _sprocsGenerated.Add(sproc);
                //}
                //txtSprocsGenerated.Text = sb.ToString();

                Dictionary<StoredProcedure, string> sprocsThatCouldNotBeCreated;

                if (chkOverwriteExisting.Checked)
                {
                    sprocsThatCouldNotBeCreated = sprocGenerator.CreateNewStoredProceduresAndReplaceExistingOnes(sprocsGenerated);
                }
                else
                {
                    sprocsThatCouldNotBeCreated = sprocGenerator.CreateNewStoredProceduresDoNotReplaceExistingOnes(sprocsGenerated);
                }

                if (sprocsThatCouldNotBeCreated.Count > 0)
                {
                    StringBuilder errorBuilder = new StringBuilder();

                    errorBuilder.Append("Errors Encountered, could not create the following procedures:  ");
                    errorBuilder.Append(Environment.NewLine);

                    foreach (KeyValuePair<StoredProcedure, string> kvp in sprocsThatCouldNotBeCreated)
                    {
                        errorBuilder.Append("SprocName:  " + kvp.Key.Name + "ErrorMessage:  " + kvp.Value);
                        errorBuilder.Append(Environment.NewLine);
                        _sprocsThatCouldNotBeGenerated.Add(kvp.Key);
                    }
                    MessageBox.Show(errorBuilder.ToString());
                }
                else
                {
                    MessageBox.Show("Sproc Generation complete without errors");
                }
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                MessageBox.Show(sqlEx.Message);

            }
            catch (Microsoft.SqlServer.Management.Smo.SmoException smo)
            {
                MessageBox.Show(smo.Message + smo.StackTrace);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            btnGetCustomSprocs.Enabled = true;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //txtDatabaseName.Text = "master";
            //txtDatabaseName.Text = "ModelDatabase";
            //txtDataSource.Text = @".\SQLEXPRESS";
            ////txtInitialCatalog.Text = "master";
            //txtInitialCatalog.Text = "ModelDatabase";
            //chkTrustedConnection.Checked = true;
            //txtSchema.Text = "dbo";

            //txtDatabaseName.Text = "PeerGroupDevelopmentDatabase";
            //txtDataSource.Text = @"IBM-5C1076B185C";
            ////txtInitialCatalog.Text = "master";
            //txtInitialCatalog.Text = "PeerGroupDevelopmentDatabase";
            //chkTrustedConnection.Checked = true;
            //txtSchema.Text = "dbo";

            //txtDatabaseName.Text = @"C:\DEVELOPMENT\AURORA\TRUNK\DESIGNER\DATA\DATABASE\BMSVERSION30.MDF";
            //txtDataSource.Text = @".\SQLEXPRESS";
            ////txtInitialCatalog.Text = "master";
            //txtInitialCatalog.Text = @"C:\DEVELOPMENT\AURORA\TRUNK\DESIGNER\DATA\DATABASE\BMSVERSION30.MDF";
            //chkTrustedConnection.Checked = true;
            //txtSchema.Text = "dbo";
           

            //txtDatabaseName.Text = "Northwind";
            //txtDataSource.Text = @"IBM-5C1076B185C";
            ////txtInitialCatalog.Text = "master";
            //txtInitialCatalog.Text = "Northwind";
            //chkTrustedConnection.Checked = true;
            //txtSchema.Text = "sa";
            //txtUserId.Text = "sa";
            //txtPassword.Text = "sql3xpr3$$";

            //txtDatabaseName.Text = "BCDCentralItineraryStore";
            //txtDataSource.Text = @".\NI";
            ////txtInitialCatalog.Text = "master";
            //txtInitialCatalog.Text = "BCDCentralItineraryStore";
            //chkTrustedConnection.Checked = true;
            //txtSchema.Text = "dbo";

            //txtDatabaseName.Text = "Northwind";
            //txtDataSource.Text = @".\NI";
            ////txtInitialCatalog.Text = "master";
            //txtInitialCatalog.Text = "Northwind";
            //chkTrustedConnection.Checked = true;
            //txtSchema.Text = "dbo";

            txtDatabaseName.Text = "KidZonePortal";
            txtDataSource.Text = @".\NI";
            //txtInitialCatalog.Text = "master";
            txtInitialCatalog.Text = "KidZonePortal";
            chkTrustedConnection.Checked = true;
            txtSchema.Text = "dbo";





            //txtDatabaseName.Text = "CodeSampleApplication";
            //txtDataSource.Text = @"IBM-5C1076B185C";
            ////txtInitialCatalog.Text = "master";
            //txtInitialCatalog.Text = "CodeSampleApplication";
            //chkTrustedConnection.Checked = true;
            //txtSchema.Text = "dbo";

            //txtDatabaseName.Text = @"C:\DEVELOPMENT\AURORA\TRUNK\DESIGNER\DATA\DATABASE\BMSVERSION50.MDF";
            //txtDataSource.Text = @".\SQLEXPRESS";
            ////txtInitialCatalog.Text = "master";
            //txtInitialCatalog.Text = @"C:\DEVELOPMENT\AURORA\TRUNK\DESIGNER\DATA\DATABASE\BMSVERSION50.MDF";
            //chkTrustedConnection.Checked = true;
            //txtSchema.Text = "dbo";

            chkOverwriteExisting.Checked = true;
            chkOverwriteExistingDtos.Checked = true;
            chkOverwriteExistingLists.Checked = true;
            btnGenerateLists.Enabled = false;
            btnTestDataAccess.Enabled = true;
            btnGetCustomSprocs.Enabled = true;            
            btnGenerateCustomSprocDtos.Enabled = true;
            btnTestGeneratedDataAccess.Enabled = true;
            chkSelectAll.Enabled = true;
        }

        private void btnDropSprocs_Click(object sender, EventArgs e)
        {
            List<StoredProcedure> sprocsToDrop = new List<StoredProcedure>();
            foreach (StoredProcedure sproc in _sprocsGenerated)
            {
                if (!_sprocsThatCouldNotBeGenerated.Contains(sproc))
                {
                    sprocsToDrop.Add(sproc);
                }
            }
            if (sprocsToDrop.Count > 0)
            {
                _sprocGenerator.DropStoredProcedures(sprocsToDrop);
            }
        }

        private void btnGenerateDtos_Click(object sender, EventArgs e)
        {
           SprocDataLayerGenerator.PredicateFunctions predicateFunctions = new SprocDataLayerGenerator.PredicateFunctions();

             string databaseName = txtDatabaseName.Text;
            string dataSource = txtDataSource.Text;
            string initialCatalog = txtInitialCatalog.Text;
            string userId = txtUserId.Text;
            string password = txtPassword.Text;
            bool trustedConnection = chkTrustedConnection.Checked;
            string schema = txtSchema.Text;

            MetaInformationSchemaManager metaInformationSchemaManager = 
                new MetaInformationSchemaManager(databaseName,
                                                 dataSource,
                                                 initialCatalog,
                                                 userId,
                                                 password,
                                                 trustedConnection,
                                                 schema);

            _metaInformationSchemaManager = metaInformationSchemaManager;

            DataLayerGenerator dataLayerGenerator = new DataLayerGenerator(metaInformationSchemaManager,
                                                                           this.GetType().Namespace);
            _sprocDataLayerGenerator = dataLayerGenerator;

            bool overwriteExisting = chkOverwriteExistingDtos.Checked;           

            List<string> tablesToGen = new List<string>();
            List<MetaInformationSchema> schemasToGen = new List<MetaInformationSchema>();

            if (this.treeAvailableTables.Nodes.Count > 0)
            {
                foreach (TreeNode node in treeAvailableTables.Nodes)
                {
                    if (node.Checked)
                    {
                        tablesToGen.Add(node.Text);
                    }
                }
            }

            if (tablesToGen.Count > 0)
            {
                foreach (string tableName in tablesToGen)
                {
                    predicateFunctions.TableNameHolder = tableName;
                    schemasToGen.AddRange(metaInformationSchemaManager.MetaDataList.FindAll(predicateFunctions.FindMetaInformationSchemaByTableName));
                }
            }

            
            dataLayerGenerator.GenerateDtoClasses(schemasToGen,overwriteExisting);
            //predicateFunctions.TableNameHolder = "TblGrid";
            //MetaInformationSchema metaInformationSchema =
            //    metaInformationSchemaManager.MetaDataList.Find(predicateFunctions.FindMetaInformationSchemaByTableName);
            //dataLayerGenerator.GenerateOneDtoClass(metaInformationSchema, overwriteExisting);
            MessageBox.Show("Dto Generation Complete");
            btnGenerateLists.Enabled = true;
        }

        private void btnGenerateLists_Click(object sender, EventArgs e)
        {
            string databaseName = txtDatabaseName.Text;
            string dataSource = txtDataSource.Text;
            string initialCatalog = txtInitialCatalog.Text;
            string userId = txtUserId.Text;
            string password = txtPassword.Text;
            bool trustedConnection = chkTrustedConnection.Checked;
            string schema = txtSchema.Text;

            SprocDataLayerGenerator.PredicateFunctions predicateFunctions =
                new SprocDataLayerGenerator.PredicateFunctions();
            

            MetaInformationSchemaManager metaInformationSchemaManager = //_metaInformationSchemaManager;
                new MetaInformationSchemaManager(databaseName,
                                                 dataSource,
                                                 initialCatalog,
                                                 userId,
                                                 password,
                                                 trustedConnection,
                                                 schema);

            DataLayerGenerator dataLayerGenerator = //_sprocDataLayerGenerator;
                new DataLayerGenerator(metaInformationSchemaManager,
                                       this.GetType().Namespace);

            dataLayerGenerator.AssembliesGeneratedInMemory = _sprocDataLayerGenerator.AssembliesGeneratedInMemory;

            bool overwriteExisting = chkOverwriteExistingDtos.Checked;
            if (dataLayerGenerator.AssembliesGeneratedInMemory.Count == 0)
            {
                MessageBox.Show("you must create dtos before generating lists based upon dtos, check the app.config file for the settings named CommonLibraryDllLocation and CurrentAppRunningExeLocation and set the path appropriately to your environment");
                btnGenerateLists.Enabled = false;
            }
            else
            {
                List<string> tablesToGen = new List<string>();
                List<MetaInformationSchema> schemasToGen = new List<MetaInformationSchema>();

                if (this.treeAvailableTables.Nodes.Count > 0)
                {
                    foreach (TreeNode node in treeAvailableTables.Nodes)
                    {
                        if (node.Checked)
                        {
                            tablesToGen.Add(node.Text);
                        }
                    }
                }

                if (tablesToGen.Count > 0)
                {
                    foreach (string tableName in tablesToGen)
                    {
                        predicateFunctions.TableNameHolder = tableName;
                        schemasToGen.AddRange(metaInformationSchemaManager.MetaDataList.FindAll(predicateFunctions.FindMetaInformationSchemaByTableName));
                    }
                }
                dataLayerGenerator.GenerateListClasses(schemasToGen, overwriteExisting);
                MessageBox.Show("List Generation Complete");
            }
            btnTestDataAccess.Enabled = true;

        }

        private void btnTestDataAccess_Click(object sender, EventArgs e)
        {
            string databaseName = txtDatabaseName.Text;
            string dataSource = txtDataSource.Text;
            string initialCatalog = txtInitialCatalog.Text;
            string userId = txtUserId.Text;
            string password = txtPassword.Text;
            bool trustedConnection = chkTrustedConnection.Checked;
            string schema = txtSchema.Text;

            _databaseSmoObjectsAndSettings = new CommonLibrary.DatabaseSmoObjectsAndSettings(databaseName,
                                                                                             dataSource,
                                                                                             initialCatalog,
                                                                                             userId,
                                                                                             password,
                                                                                             trustedConnection);


            string connectionString = _databaseSmoObjectsAndSettings.ConnectionString;

            //TestSprocGenerator.Data.SingleTable.Dto.TblGrid dtoGrid = new TestSprocGenerator.Data.SingleTable.Dto.TblGrid();


            //CommonLibrary.Base.Database.BaseDataAccess<TestSprocGenerator.Data.SingleTable.Dto.TblGrid> _baseDataAccess =
            //    new CommonLibrary.Base.Database.BaseDataAccess<TestSprocGenerator.Data.SingleTable.Dto.TblGrid>(_databaseSmoObjectsAndSettings);

            //dtoGrid.GridKey = 2830;
            //dtoGrid.Name = "Enterprise Computing View Setup";
            //dtoGrid.NodeKey = 64;
            //dtoGrid.NoOfRows = 2;
            //dtoGrid.NoOfCols = 10;

            //List<TestSprocGenerator.Data.SingleTable.Dto.TblGrid> listReturned =
            //    _baseDataAccess.Get(dtoGrid, CommonLibrary.Enumerations.GetPermutations.ByExplicitCriteria);

            //dtoGrid = new TestSprocGenerator.Data.SingleTable.Dto.TblGrid();
            //dtoGrid.Name = "ent";

            //listReturned = _baseDataAccess.Get(dtoGrid, CommonLibrary.Enumerations.GetPermutations.ByFuzzyCriteria);

            //dtoGrid = new TestSprocGenerator.Data.SingleTable.Dto.TblGrid();
            //dtoGrid.GridKey = 2830;
            //dtoGrid.Name = "SomeName";
            //listReturned
            //   = _baseDataAccess.Get(dtoGrid, CommonLibrary.Enumerations.GetPermutations.ByPrimaryKey);

            //listReturned =
            //    _baseDataAccess.Get(dtoGrid, CommonLibrary.Enumerations.GetPermutations.AllByColumnMappings);


            //CommonLibrary.Base.Database.BaseDataAccess<TestSprocGenerator.Data.SingleTable.Dto.TestTable> _baseDataAccessTestTable =
            //    new CommonLibrary.Base.Database.BaseDataAccess<TestSprocGenerator.Data.SingleTable.Dto.TestTable>(_databaseSmoObjectsAndSettings);

            //TestSprocGenerator.Data.SingleTable.Dto.TestTable testTable =
            //    new TestSprocGenerator.Data.SingleTable.Dto.TestTable();


            //testTable.TestTableGuid = Guid.NewGuid();
            //testTable.TestTableCharValue = new Char[] { 'C' };
            //testTable.TestTableCreatedDate = DateTime.Now;
            //testTable.TestTableDateLastModified = DateTime.Now;
            //testTable.TestTableImageValue = Image.FromFile(@"C:\Documents and Settings\All Users\Documents\My Pictures\Sample Pictures\Sunset.jpg");
            //testTable.TestTableDecimalValue = 1.0M;
            //testTable.TestTableBitValue = true;
            //testTable.TestTableDescription = "test description";
            //testTable.TestTableMoneyValue = 5.00m;
            //testTable.TestTableName = "Test Table name";
            //testTable.TestTableNumericValue = 4;


            //_baseDataAccessTestTable.Insert(testTable);

            //testTable.TestTableID = 3;
            //testTable.TestTableGuid = new Guid("6975a402-dd9e-4daf-9f30-06ba65117618");
            //testTable.TestTableDescription = "Updated Description";

            //_baseDataAccessTestTable.Update(testTable);

            //testTable.TestTableGuid = Guid.NewGuid();
            //testTable.TestTableCharValue = new Char[] { 'C' };
            //testTable.TestTableCreatedDate = DateTime.Now;
            //testTable.TestTableDateLastModified = DateTime.Now;
            //testTable.TestTableImageValue = Image.FromFile(@"C:\Documents and Settings\All Users\Documents\My Pictures\Sample Pictures\Sunset.jpg");
            //testTable.TestTableDecimalValue = 1.0M;
            //testTable.TestTableBitValue = true;
            //testTable.TestTableDescription = "this one should be deleted";
            //testTable.TestTableMoneyValue = 5.00m;
            //testTable.TestTableName = "Test Table name";
            //testTable.TestTableNumericValue = 4;

            //_baseDataAccessTestTable.Insert(testTable);

            //_baseDataAccessTestTable.Delete(testTable);
        }

        private void btnGetCustomSprocs_Click(object sender, EventArgs e)
        {
            _customSprocs.Clear();
            treeCustomSprocs.Nodes.Clear();
            
            string databaseName = txtDatabaseName.Text;
            string dataSource = txtDataSource.Text;
            string initialCatalog = txtInitialCatalog.Text;
            string userId = txtUserId.Text;
            string password = txtPassword.Text;
            bool trustedConnection = chkTrustedConnection.Checked;
            string schema = txtSchema.Text;

            _databaseSmoObjectsAndSettings = new CommonLibrary.DatabaseSmoObjectsAndSettings(databaseName,
                                                                                             dataSource,
                                                                                             initialCatalog,
                                                                                             userId,
                                                                                             password,
                                                                                             trustedConnection);

            _dataLayerGenerator = new DataLayerGenerator(_databaseSmoObjectsAndSettings,
                                                         this.GetType().Namespace);

            SprocDataLayerGenerator.PredicateFunctions predicateFunctions = new SprocDataLayerGenerator.PredicateFunctions();
            List<StoredProcedure> customSprocsNotGenerated = new List<StoredProcedure>();


            CommonLibrary.DatabaseSmoObjectsAndSettings databaseSmoObjectsAndSettings =
                new CommonLibrary.DatabaseSmoObjectsAndSettings(databaseName,
                                                                dataSource,
                                                                initialCatalog,
                                                                userId,
                                                                password,
                                                                trustedConnection,
                                                                schema);

              SprocGenerator sprocGenerator = new SprocGenerator(databaseName,
                                                                   dataSource,
                                                                   initialCatalog,
                                                                   userId,
                                                                   password,
                                                                   trustedConnection,
                                                                   schema);
                _sprocGenerator = sprocGenerator;
            List<StoredProcedure> sprocsGenerated = _sprocGenerator.GetStoredProcedures();
            if (sprocsGenerated != null)
            {
                foreach (StoredProcedure sproc in databaseSmoObjectsAndSettings.Database_Property.StoredProcedures)
                {
                    predicateFunctions.SprocNameHolder = sproc.Name;
                    if (!(sprocsGenerated.FindAll(predicateFunctions.FindSprocGeneratedBySprocName).Count > 0))
                    {
                        customSprocsNotGenerated.Add(sproc);
                    }
                }
            }

            if (customSprocsNotGenerated.Count > 0)
            {
                foreach (StoredProcedure customSproc in customSprocsNotGenerated)
                {
                    if (!customSproc.IsSystemObject)
                    {
                        treeCustomSprocs.Nodes.Add(customSproc.Name);
                        _customSprocs.Add(customSproc);
                    }
                }
            }
            btnGenerateCustomSprocDtos.Enabled = true;
            chkSelectAll.Enabled = true;
        }

        private void btnGenerateCustomSprocDtos_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> mainSprocNameToInputDto = new Dictionary<string,string>();
            Dictionary<string, List<string>> sprocToDtoListsUsed = new Dictionary<string,List<string>>();
            Dictionary<string, List<string>> sprocToInputDtosUsed = new Dictionary<string,List<string>>();
            Dictionary<string, List<string>> sprocToDtosUsed = new Dictionary<string,List<string>>();
            Dictionary<string, string> standaloneSprocToInputDto = new Dictionary<string,string>();
            
            SprocDataLayerGenerator.PredicateFunctions predicateFunctions = 
                new SprocDataLayerGenerator.PredicateFunctions();

            List<string> customSprocsGenerateDtos = new List<string>();
            List<StoredProcedure> customSprocsChosen = new List<StoredProcedure>();
           
            if (treeCustomSprocs.Nodes.Count > 0)
            {
                foreach (TreeNode node in treeCustomSprocs.Nodes)
                {
                    if (node.Checked)
                    {
                        customSprocsGenerateDtos.Add(node.Text);
                    }
                }
            }

            if (customSprocsGenerateDtos.Count > 0)
            {
                foreach (string customSprocName in customSprocsGenerateDtos)
                {
                    predicateFunctions.SprocNameHolder = customSprocName;
                    customSprocsChosen.AddRange(_customSprocs.FindAll(predicateFunctions.FindSprocGeneratedBySprocName));
                }
            }

            if (customSprocsChosen.Count > 0)
            {

                _dataLayerGenerator.GenerateCustomSprocsDtosAndLists(customSprocsChosen,
                                                                     out mainSprocNameToInputDto,
                                                                     out sprocToDtoListsUsed,
                                                                     out sprocToInputDtosUsed,
                                                                     out sprocToDtosUsed,
                                                                     out standaloneSprocToInputDto);
            }

            _mainSprocNameToInputDto = mainSprocNameToInputDto;
            _sprocToDtoListsUsed = sprocToDtoListsUsed;
            _sprocToInputDtosUsed = sprocToInputDtosUsed;                                                       
            _sprocToDtosUsed = sprocToDtosUsed;
            _standaloneSprocToInputDto = standaloneSprocToInputDto;
            

            btnGenerateDataAccessClass.Enabled = true;
            MessageBox.Show("Data Access Methods and Input Dtos and Result List Dtos for Custom Sprocs Generation Complete");
            MessageBox.Show("Now include the classes that the generator created into the solution and compile");
        }

        private void btnTestGeneratedDataAccess_Click(object sender, EventArgs e)
        {

            string databaseName = txtDatabaseName.Text;
            string dataSource = txtDataSource.Text;
            string initialCatalog = txtInitialCatalog.Text;
            string userId = txtUserId.Text;
            string password = txtPassword.Text;
            bool trustedConnection = chkTrustedConnection.Checked;
            string schema = txtSchema.Text;

            _databaseSmoObjectsAndSettings = new CommonLibrary.DatabaseSmoObjectsAndSettings(databaseName,
                                                                                             dataSource,
                                                                                             initialCatalog,
                                                                                             userId,
                                                                                             password,
                                                                                             trustedConnection);
            string connectionString = _databaseSmoObjectsAndSettings.ConnectionString;

            //TestSprocGenerator.Data.SprocTable.Input.Dto.GetAllGridTablesAndQuestionsByGridKey inputAll =
            //    new TestSprocGenerator.Data.SprocTable.Input.Dto.GetAllGridTablesAndQuestionsByGridKey();

            //inputAll.GridKey = 3002;

            //TestSprocGenerator.Data.Accessor.CustomSproc customSprocAccessor
            //    = new TestSprocGenerator.Data.Accessor.CustomSproc();


            //DataSet ds = customSprocAccessor.GetAllGridTablesAndQuestionsByGridKey_DataSet(connectionString, inputAll);

            //TestSprocGenerator.Data.SprocTable.Input.Dto.GetGridCellFontOverrides inputFontOver =
            //    new TestSprocGenerator.Data.SprocTable.Input.Dto.GetGridCellFontOverrides();

            //inputFontOver.GridKey = 3002;

            //TestSprocGenerator.SprocTable.Result.List.GetGridCellFontOverrides gridCellOverridesList =
            //    customSprocAccessor.GetGridCellFontOverrides(connectionString, inputFontOver);

            //DataSet ds1 =
            //    customSprocAccessor.GetGridCellFontOverrides_DataSet(connectionString, inputFontOver);

            //TestSprocGenerator.Data.SprocTable.Input.Dto.GetGrids gridInput = new TestSprocGenerator.Data.SprocTable.Input.Dto.GetGrids();
            //gridInput.GridKey = 0;
            //TestSprocGenerator.SprocTable.Result.List.GetGrids gridsReturned =
            //    customSprocAccessor.GetGrids(connectionString, gridInput);



        }

        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            bool checkedValue = chkSelectAll.Checked;
           
                if (treeCustomSprocs.Nodes.Count > 0)
                {
                    foreach (TreeNode node in treeCustomSprocs.Nodes)
                    {
                        node.Checked = checkedValue;
                    }
                }
           
        }      


        private void chkHaveYouCompiledTheDataAccessLayerIntoTheAssembly_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void btnGenerateBusinessObjects_Click(object sender, EventArgs e)
        {

            MessageBox.Show("Have you compiled your dto and list classes into the current app?  If not, then the business classes will not be generated");
            string databaseName = txtDatabaseName.Text;
            string dataSource = txtDataSource.Text;
            string initialCatalog = txtInitialCatalog.Text;
            string userId = txtUserId.Text;
            string password = txtPassword.Text;
            bool trustedConnection = chkTrustedConnection.Checked;
            string schema = txtSchema.Text;

            CommonLibrary.DatabaseSmoObjectsAndSettings databaseSettings =
                new CommonLibrary.DatabaseSmoObjectsAndSettings(databaseName, dataSource, initialCatalog,
                                                                userId, password, trustedConnection, schema);
            _databaseSmoObjectsAndSettings = databaseSettings;

            BusinessLayerGeneration businessGeneration = new BusinessLayerGeneration(databaseSettings,this.GetType().Namespace);
            bool overwriteExisting = true;
            //this is just a test method so that we generate for only the test table to start with
            //businessGeneration.TestTableGenerateBusinessLayer(overwriteExisting);
            businessGeneration.GenerateBusinessLayer(overwriteExisting);
            MessageBox.Show("Business Objects Generation Complete");
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string databaseName = txtDatabaseName.Text;
            string dataSource = txtDataSource.Text;
            string initialCatalog = txtInitialCatalog.Text;
            string userId = txtUserId.Text;
            string password = txtPassword.Text;
            bool trustedConnection = chkTrustedConnection.Checked;
            string schema = txtSchema.Text;

            CommonLibrary.DatabaseSmoObjectsAndSettings databaseSettings =
                new CommonLibrary.DatabaseSmoObjectsAndSettings(databaseName, dataSource, initialCatalog,
                                                                userId, password, trustedConnection, schema);
            _databaseSmoObjectsAndSettings = databaseSettings;

            //Business.SingleTable.Bo.TestTable boTestTable =
            //    new TestSprocGenerator.Business.SingleTable.Bo.TestTable(databaseSettings);

            //Business.SingleTable.Bo.TblFilterDefinitions tblFilterDefinitions =
            //    new TestSprocGenerator.Business.SingleTable.Bo.TblFilterDefinitions(databaseSettings);

            ////do not forget, if you do not have a primary key on a table then you will get errors as 
            ////cannot find GetByPrimaryKey etc - needs to be resolved

            ////also, if the autoseed ID column is not specified as part of the primary key, then 
            ////the insert sproc will still expect it, so IDs should always be part of the primary key
            ////until this can be resolved.

            //this code generator does not support the use of names in the database
            //that have any spaces in them, remove all spaces from object names
            //in your database or else it cannot create the stored procedure parameters
            //or private and public members properly, you will get errors.

            //boTestTable.TestTableBitValue = true;
            //boTestTable.TestTableCharValue = "c".ToCharArray();
            //boTestTable.TestTableCreatedDate = DateTime.Now;
            //boTestTable.TestTableDateLastModified = DateTime.Now;
            //boTestTable.TestTableDecimalValue = 1.0m;
            //boTestTable.TestTableDescription = "description";
            //boTestTable.TestTableGuid = System.Guid.NewGuid();
            //boTestTable.TestTableMoneyValue = 2.0m;
            //boTestTable.TestTableName = "test table name";
            //boTestTable.TestTableNumericValue = 25.0m;
            //boTestTable.TestTableImageValue = Image.FromFile(@"C:\Documents and Settings\All Users\Documents\My Pictures\Sample Pictures\Sunset.jpg");

            //boTestTable.Insert();

            //boTestTable.TestTableName = "Updated Test Table Name";
            //boTestTable.Update();

            //boTestTable.Delete();

            //boTestTable.TestTableGuid = new Guid("44d64a90-60a3-4202-8b55-043f8d6c911c");
            //boTestTable.TestTableID = 4;
            //boTestTable.GetByPrimaryKey();

            //TestSprocGenerator.Business.SingleTable.Bo.List.TestTable listOfAll =
            //    new TestSprocGenerator.Business.SingleTable.Bo.List.TestTable(_databaseSmoObjectsAndSettings);
            //TestSprocGenerator.Business.SingleTable.Bo.TestTable dummyBo = new TestSprocGenerator.Business.SingleTable.Bo.TestTable();
            //listOfAll.FillByGetAll(dummyBo);

            //TestSprocGenerator.Business.SingleTable.Bo.List.TblFilterDefinitions listOfAll =
            //    new TestSprocGenerator.Business.SingleTable.Bo.List.TblFilterDefinitions(_databaseSmoObjectsAndSettings);
            //TestSprocGenerator.Business.SingleTable.Bo.TblFilterDefinitions dummyBo =
            //    new TestSprocGenerator.Business.SingleTable.Bo.TblFilterDefinitions();
            //listOfAll.FillByGetAll(dummyBo);

            //listOfAll.FillByPrimaryKey(boTestTable);

            //boTestTable = new TestSprocGenerator.Business.SingleTable.Bo.TestTable();

            //boTestTable.TestTableName = "test table";
            //boTestTable.TestTableDescription = "updated";
            //listOfAll.FillByCriteriaFuzzy(boTestTable);

            //boTestTable = new TestSprocGenerator.Business.SingleTable.Bo.TestTable();
            //boTestTable.TestTableGuid = new Guid("6975a402-dd9e-4daf-9f30-06ba65117618");
            //listOfAll.FillByCriteriaExact(boTestTable);


        }

        private void btnGenerateDataAccessClass_Click(object sender, EventArgs e)
        {
            _dataLayerGenerator.GenerateDataAccessClass(_mainSprocNameToInputDto,
                _sprocToDtoListsUsed,
                _sprocToInputDtosUsed,
                _sprocToDtosUsed,
                _standaloneSprocToInputDto);
        }

        private void chkSelectDeselectAllTables_CheckedChanged(object sender, EventArgs e)
        {

            bool checkedValue = this.chkSelectDeselectAllTables.Checked;

            if (this.treeAvailableTables.Nodes.Count > 0)
            {
                foreach (TreeNode node in treeAvailableTables.Nodes)
                {
                    node.Checked = checkedValue;
                }
            }
        }

        private void btnLoadTables_Click(object sender, EventArgs e)
        {
             string databaseName = txtDatabaseName.Text;
            string dataSource = txtDataSource.Text;
            string initialCatalog = txtInitialCatalog.Text;
            string userId = txtUserId.Text;
            string password = txtPassword.Text;
            bool trustedConnection = chkTrustedConnection.Checked;
            string schema = txtSchema.Text;

            try
            {

                SprocGenerator sprocGenerator = new SprocGenerator(databaseName,
                                                                   dataSource,
                                                                   initialCatalog,
                                                                   userId,
                                                                   password,
                                                                   trustedConnection,
                                                                   schema);
                _sprocGenerator = sprocGenerator;

                MetaInformationSchemaManager metaInformationSchemaManager =
                   new MetaInformationSchemaManager(databaseName,
                   dataSource,
                   initialCatalog,
                   userId,
                   password,
                   trustedConnection,
                   schema);

                foreach (MetaInformationSchema metaSchema in metaInformationSchemaManager.MetaDataList)
                {
                    //the reason we do this is because views blow up and are returned
                    //with this set from the query, so we just make sure we are looking
                    //only at base table type objects
                    if (metaSchema.MetaTable.TableType == CommonLibrary.Constants.SqlConstants.BASE_TABLE)
                    {
                        treeAvailableTables.Nodes.Add(metaSchema.MetaTable.TableName);
                    }
                }

                foreach (TreeNode node in treeAvailableTables.Nodes)
                {
                    node.Checked = true;
                }
            }

            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                MessageBox.Show(sqlEx.Message);

            }
            catch (Microsoft.SqlServer.Management.Smo.SmoException smo)
            {
                MessageBox.Show(smo.Message + smo.StackTrace);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
       
    }
}