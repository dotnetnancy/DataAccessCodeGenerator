namespace TestSprocGenerator
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtDatabaseName = new System.Windows.Forms.TextBox();
            this.lblDatabaseName = new System.Windows.Forms.Label();
            this.lblDataSource = new System.Windows.Forms.Label();
            this.txtDataSource = new System.Windows.Forms.TextBox();
            this.lblInitialCatalog = new System.Windows.Forms.Label();
            this.txtInitialCatalog = new System.Windows.Forms.TextBox();
            this.lblUserId = new System.Windows.Forms.Label();
            this.txtUserId = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.chkTrustedConnection = new System.Windows.Forms.CheckBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.lblSchema = new System.Windows.Forms.Label();
            this.txtSchema = new System.Windows.Forms.TextBox();
            this.btnDropSprocs = new System.Windows.Forms.Button();
            this.chkOverwriteExisting = new System.Windows.Forms.CheckBox();
            this.btnGenerateDtos = new System.Windows.Forms.Button();
            this.chkOverwriteExistingDtos = new System.Windows.Forms.CheckBox();
            this.btnGenerateLists = new System.Windows.Forms.Button();
            this.chkOverwriteExistingLists = new System.Windows.Forms.CheckBox();
            this.btnTestDataAccess = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkSelectDeselectAllTables = new System.Windows.Forms.CheckBox();
            this.treeAvailableTables = new System.Windows.Forms.TreeView();
            this.btnLoadTables = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnGenerateBusinessObjects = new System.Windows.Forms.Button();
            this.btnGetCustomSprocs = new System.Windows.Forms.Button();
            this.treeCustomSprocs = new System.Windows.Forms.TreeView();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnGenerateDataAccessClass = new System.Windows.Forms.Button();
            this.chkSelectAll = new System.Windows.Forms.CheckBox();
            this.btnTestGeneratedDataAccess = new System.Windows.Forms.Button();
            this.btnGenerateCustomSprocDtos = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtDatabaseName
            // 
            this.txtDatabaseName.Location = new System.Drawing.Point(130, 39);
            this.txtDatabaseName.Name = "txtDatabaseName";
            this.txtDatabaseName.Size = new System.Drawing.Size(100, 20);
            this.txtDatabaseName.TabIndex = 0;
            // 
            // lblDatabaseName
            // 
            this.lblDatabaseName.AutoSize = true;
            this.lblDatabaseName.Location = new System.Drawing.Point(24, 39);
            this.lblDatabaseName.Name = "lblDatabaseName";
            this.lblDatabaseName.Size = new System.Drawing.Size(81, 13);
            this.lblDatabaseName.TabIndex = 1;
            this.lblDatabaseName.Text = "DatabaseName";
            // 
            // lblDataSource
            // 
            this.lblDataSource.AutoSize = true;
            this.lblDataSource.Location = new System.Drawing.Point(24, 87);
            this.lblDataSource.Name = "lblDataSource";
            this.lblDataSource.Size = new System.Drawing.Size(67, 13);
            this.lblDataSource.TabIndex = 2;
            this.lblDataSource.Text = "Data Source";
            // 
            // txtDataSource
            // 
            this.txtDataSource.Location = new System.Drawing.Point(130, 79);
            this.txtDataSource.Name = "txtDataSource";
            this.txtDataSource.Size = new System.Drawing.Size(100, 20);
            this.txtDataSource.TabIndex = 3;
            // 
            // lblInitialCatalog
            // 
            this.lblInitialCatalog.AutoSize = true;
            this.lblInitialCatalog.Location = new System.Drawing.Point(24, 127);
            this.lblInitialCatalog.Name = "lblInitialCatalog";
            this.lblInitialCatalog.Size = new System.Drawing.Size(70, 13);
            this.lblInitialCatalog.TabIndex = 4;
            this.lblInitialCatalog.Text = "Initial Catalog";
            // 
            // txtInitialCatalog
            // 
            this.txtInitialCatalog.Location = new System.Drawing.Point(130, 127);
            this.txtInitialCatalog.Name = "txtInitialCatalog";
            this.txtInitialCatalog.Size = new System.Drawing.Size(100, 20);
            this.txtInitialCatalog.TabIndex = 5;
            // 
            // lblUserId
            // 
            this.lblUserId.AutoSize = true;
            this.lblUserId.Location = new System.Drawing.Point(257, 27);
            this.lblUserId.Name = "lblUserId";
            this.lblUserId.Size = new System.Drawing.Size(41, 13);
            this.lblUserId.TabIndex = 6;
            this.lblUserId.Text = "User Id";
            // 
            // txtUserId
            // 
            this.txtUserId.Location = new System.Drawing.Point(363, 27);
            this.txtUserId.Name = "txtUserId";
            this.txtUserId.Size = new System.Drawing.Size(100, 20);
            this.txtUserId.TabIndex = 7;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(257, 61);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(53, 13);
            this.lblPassword.TabIndex = 8;
            this.lblPassword.Text = "Password";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(363, 61);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(100, 20);
            this.txtPassword.TabIndex = 9;
            // 
            // chkTrustedConnection
            // 
            this.chkTrustedConnection.AutoSize = true;
            this.chkTrustedConnection.Location = new System.Drawing.Point(132, 160);
            this.chkTrustedConnection.Name = "chkTrustedConnection";
            this.chkTrustedConnection.Size = new System.Drawing.Size(122, 17);
            this.chkTrustedConnection.TabIndex = 10;
            this.chkTrustedConnection.Text = "TrustedConnection?";
            this.chkTrustedConnection.UseVisualStyleBackColor = true;
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(522, 197);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(143, 23);
            this.btnGenerate.TabIndex = 12;
            this.btnGenerate.Text = "Generate Sprocs";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // lblSchema
            // 
            this.lblSchema.AutoSize = true;
            this.lblSchema.Location = new System.Drawing.Point(257, 98);
            this.lblSchema.Name = "lblSchema";
            this.lblSchema.Size = new System.Drawing.Size(46, 13);
            this.lblSchema.TabIndex = 13;
            this.lblSchema.Text = "Schema";
            // 
            // txtSchema
            // 
            this.txtSchema.Location = new System.Drawing.Point(363, 98);
            this.txtSchema.Name = "txtSchema";
            this.txtSchema.Size = new System.Drawing.Size(100, 20);
            this.txtSchema.TabIndex = 14;
            // 
            // btnDropSprocs
            // 
            this.btnDropSprocs.Location = new System.Drawing.Point(694, 197);
            this.btnDropSprocs.Name = "btnDropSprocs";
            this.btnDropSprocs.Size = new System.Drawing.Size(143, 23);
            this.btnDropSprocs.TabIndex = 15;
            this.btnDropSprocs.Text = "Drop Created Sprocs";
            this.btnDropSprocs.UseVisualStyleBackColor = true;
            this.btnDropSprocs.Click += new System.EventHandler(this.btnDropSprocs_Click);
            // 
            // chkOverwriteExisting
            // 
            this.chkOverwriteExisting.AutoSize = true;
            this.chkOverwriteExisting.Location = new System.Drawing.Point(260, 160);
            this.chkOverwriteExisting.Name = "chkOverwriteExisting";
            this.chkOverwriteExisting.Size = new System.Drawing.Size(116, 17);
            this.chkOverwriteExisting.TabIndex = 16;
            this.chkOverwriteExisting.Text = "Overwrite Existing?";
            this.chkOverwriteExisting.UseVisualStyleBackColor = true;
            // 
            // btnGenerateDtos
            // 
            this.btnGenerateDtos.Location = new System.Drawing.Point(6, 269);
            this.btnGenerateDtos.Name = "btnGenerateDtos";
            this.btnGenerateDtos.Size = new System.Drawing.Size(143, 23);
            this.btnGenerateDtos.TabIndex = 17;
            this.btnGenerateDtos.Text = "Generate Dtos";
            this.btnGenerateDtos.UseVisualStyleBackColor = true;
            this.btnGenerateDtos.Click += new System.EventHandler(this.btnGenerateDtos_Click);
            // 
            // chkOverwriteExistingDtos
            // 
            this.chkOverwriteExistingDtos.AutoSize = true;
            this.chkOverwriteExistingDtos.Location = new System.Drawing.Point(182, 275);
            this.chkOverwriteExistingDtos.Name = "chkOverwriteExistingDtos";
            this.chkOverwriteExistingDtos.Size = new System.Drawing.Size(116, 17);
            this.chkOverwriteExistingDtos.TabIndex = 18;
            this.chkOverwriteExistingDtos.Text = "Overwrite Existing?";
            this.chkOverwriteExistingDtos.UseVisualStyleBackColor = true;
            // 
            // btnGenerateLists
            // 
            this.btnGenerateLists.Location = new System.Drawing.Point(320, 271);
            this.btnGenerateLists.Name = "btnGenerateLists";
            this.btnGenerateLists.Size = new System.Drawing.Size(143, 23);
            this.btnGenerateLists.TabIndex = 19;
            this.btnGenerateLists.Text = "Generate Lists";
            this.btnGenerateLists.UseVisualStyleBackColor = true;
            this.btnGenerateLists.Click += new System.EventHandler(this.btnGenerateLists_Click);
            // 
            // chkOverwriteExistingLists
            // 
            this.chkOverwriteExistingLists.AutoSize = true;
            this.chkOverwriteExistingLists.Location = new System.Drawing.Point(494, 277);
            this.chkOverwriteExistingLists.Name = "chkOverwriteExistingLists";
            this.chkOverwriteExistingLists.Size = new System.Drawing.Size(113, 17);
            this.chkOverwriteExistingLists.TabIndex = 20;
            this.chkOverwriteExistingLists.Text = "Overwrite Exising?";
            this.chkOverwriteExistingLists.UseVisualStyleBackColor = true;
            // 
            // btnTestDataAccess
            // 
            this.btnTestDataAccess.Location = new System.Drawing.Point(734, 273);
            this.btnTestDataAccess.Name = "btnTestDataAccess";
            this.btnTestDataAccess.Size = new System.Drawing.Size(153, 23);
            this.btnTestDataAccess.TabIndex = 21;
            this.btnTestDataAccess.Text = "TestDataAccess";
            this.btnTestDataAccess.UseVisualStyleBackColor = true;
            this.btnTestDataAccess.Click += new System.EventHandler(this.btnTestDataAccess_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkSelectDeselectAllTables);
            this.groupBox1.Controls.Add(this.treeAvailableTables);
            this.groupBox1.Controls.Add(this.btnLoadTables);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.btnTestDataAccess);
            this.groupBox1.Controls.Add(this.lblUserId);
            this.groupBox1.Controls.Add(this.chkOverwriteExistingLists);
            this.groupBox1.Controls.Add(this.txtUserId);
            this.groupBox1.Controls.Add(this.btnGenerateLists);
            this.groupBox1.Controls.Add(this.lblPassword);
            this.groupBox1.Controls.Add(this.chkOverwriteExistingDtos);
            this.groupBox1.Controls.Add(this.txtPassword);
            this.groupBox1.Controls.Add(this.btnGenerateDtos);
            this.groupBox1.Controls.Add(this.lblSchema);
            this.groupBox1.Controls.Add(this.chkOverwriteExisting);
            this.groupBox1.Controls.Add(this.txtSchema);
            this.groupBox1.Controls.Add(this.btnDropSprocs);
            this.groupBox1.Controls.Add(this.chkTrustedConnection);
            this.groupBox1.Controls.Add(this.btnGenerate);
            this.groupBox1.Controls.Add(this.btnGenerateBusinessObjects);
            this.groupBox1.Location = new System.Drawing.Point(2, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(903, 373);
            this.groupBox1.TabIndex = 22;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Single Table";
            // 
            // chkSelectDeselectAllTables
            // 
            this.chkSelectDeselectAllTables.AutoSize = true;
            this.chkSelectDeselectAllTables.Location = new System.Drawing.Point(494, 11);
            this.chkSelectDeselectAllTables.Name = "chkSelectDeselectAllTables";
            this.chkSelectDeselectAllTables.Size = new System.Drawing.Size(117, 17);
            this.chkSelectDeselectAllTables.TabIndex = 26;
            this.chkSelectDeselectAllTables.Text = "Select\\Deselect All";
            this.chkSelectDeselectAllTables.UseVisualStyleBackColor = true;
            this.chkSelectDeselectAllTables.CheckedChanged += new System.EventHandler(this.chkSelectDeselectAllTables_CheckedChanged);
            // 
            // treeAvailableTables
            // 
            this.treeAvailableTables.CheckBoxes = true;
            this.treeAvailableTables.Location = new System.Drawing.Point(494, 34);
            this.treeAvailableTables.Name = "treeAvailableTables";
            this.treeAvailableTables.Size = new System.Drawing.Size(343, 143);
            this.treeAvailableTables.TabIndex = 25;
            // 
            // btnLoadTables
            // 
            this.btnLoadTables.Location = new System.Drawing.Point(155, 197);
            this.btnLoadTables.Name = "btnLoadTables";
            this.btnLoadTables.Size = new System.Drawing.Size(155, 23);
            this.btnLoadTables.TabIndex = 24;
            this.btnLoadTables.Text = "Load Available Tables";
            this.btnLoadTables.UseVisualStyleBackColor = true;
            this.btnLoadTables.Click += new System.EventHandler(this.btnLoadTables_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(734, 321);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(153, 23);
            this.button1.TabIndex = 23;
            this.button1.Text = "TestBusinessOjbect";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnGenerateBusinessObjects
            // 
            this.btnGenerateBusinessObjects.Location = new System.Drawing.Point(6, 321);
            this.btnGenerateBusinessObjects.Name = "btnGenerateBusinessObjects";
            this.btnGenerateBusinessObjects.Size = new System.Drawing.Size(155, 23);
            this.btnGenerateBusinessObjects.TabIndex = 22;
            this.btnGenerateBusinessObjects.Text = "Generate Business Objects";
            this.btnGenerateBusinessObjects.UseVisualStyleBackColor = true;
            this.btnGenerateBusinessObjects.Click += new System.EventHandler(this.btnGenerateBusinessObjects_Click);
            // 
            // btnGetCustomSprocs
            // 
            this.btnGetCustomSprocs.Location = new System.Drawing.Point(42, 36);
            this.btnGetCustomSprocs.Name = "btnGetCustomSprocs";
            this.btnGetCustomSprocs.Size = new System.Drawing.Size(138, 23);
            this.btnGetCustomSprocs.TabIndex = 24;
            this.btnGetCustomSprocs.Text = "Load Custom Sprocs";
            this.btnGetCustomSprocs.UseVisualStyleBackColor = true;
            this.btnGetCustomSprocs.Click += new System.EventHandler(this.btnGetCustomSprocs_Click);
            // 
            // treeCustomSprocs
            // 
            this.treeCustomSprocs.CheckBoxes = true;
            this.treeCustomSprocs.Location = new System.Drawing.Point(10, 94);
            this.treeCustomSprocs.Name = "treeCustomSprocs";
            this.treeCustomSprocs.Size = new System.Drawing.Size(222, 154);
            this.treeCustomSprocs.TabIndex = 25;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnGenerateDataAccessClass);
            this.groupBox2.Controls.Add(this.chkSelectAll);
            this.groupBox2.Controls.Add(this.btnTestGeneratedDataAccess);
            this.groupBox2.Controls.Add(this.btnGenerateCustomSprocDtos);
            this.groupBox2.Controls.Add(this.treeCustomSprocs);
            this.groupBox2.Controls.Add(this.btnGetCustomSprocs);
            this.groupBox2.Location = new System.Drawing.Point(2, 391);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(903, 254);
            this.groupBox2.TabIndex = 26;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "CustomSprocs";
            // 
            // btnGenerateDataAccessClass
            // 
            this.btnGenerateDataAccessClass.Location = new System.Drawing.Point(363, 146);
            this.btnGenerateDataAccessClass.Name = "btnGenerateDataAccessClass";
            this.btnGenerateDataAccessClass.Size = new System.Drawing.Size(188, 23);
            this.btnGenerateDataAccessClass.TabIndex = 29;
            this.btnGenerateDataAccessClass.Text = "Generate Data Access Class";
            this.btnGenerateDataAccessClass.UseVisualStyleBackColor = true;
            this.btnGenerateDataAccessClass.Click += new System.EventHandler(this.btnGenerateDataAccessClass_Click);
            // 
            // chkSelectAll
            // 
            this.chkSelectAll.AutoSize = true;
            this.chkSelectAll.Location = new System.Drawing.Point(10, 71);
            this.chkSelectAll.Name = "chkSelectAll";
            this.chkSelectAll.Size = new System.Drawing.Size(123, 17);
            this.chkSelectAll.TabIndex = 28;
            this.chkSelectAll.Text = "Select / Deselect All";
            this.chkSelectAll.UseVisualStyleBackColor = true;
            this.chkSelectAll.CheckedChanged += new System.EventHandler(this.chkSelectAll_CheckedChanged);
            // 
            // btnTestGeneratedDataAccess
            // 
            this.btnTestGeneratedDataAccess.Location = new System.Drawing.Point(363, 186);
            this.btnTestGeneratedDataAccess.Name = "btnTestGeneratedDataAccess";
            this.btnTestGeneratedDataAccess.Size = new System.Drawing.Size(188, 23);
            this.btnTestGeneratedDataAccess.TabIndex = 27;
            this.btnTestGeneratedDataAccess.Text = "Test Generated Data Access";
            this.btnTestGeneratedDataAccess.UseVisualStyleBackColor = true;
            this.btnTestGeneratedDataAccess.Click += new System.EventHandler(this.btnTestGeneratedDataAccess_Click);
            // 
            // btnGenerateCustomSprocDtos
            // 
            this.btnGenerateCustomSprocDtos.Location = new System.Drawing.Point(363, 94);
            this.btnGenerateCustomSprocDtos.Name = "btnGenerateCustomSprocDtos";
            this.btnGenerateCustomSprocDtos.Size = new System.Drawing.Size(188, 37);
            this.btnGenerateCustomSprocDtos.TabIndex = 26;
            this.btnGenerateCustomSprocDtos.Text = "Generate Selected Custom Sprocs Dtos And Lists";
            this.btnGenerateCustomSprocDtos.UseVisualStyleBackColor = true;
            this.btnGenerateCustomSprocDtos.Click += new System.EventHandler(this.btnGenerateCustomSprocDtos_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(917, 651);
            this.Controls.Add(this.txtInitialCatalog);
            this.Controls.Add(this.lblInitialCatalog);
            this.Controls.Add(this.txtDataSource);
            this.Controls.Add(this.lblDataSource);
            this.Controls.Add(this.lblDatabaseName);
            this.Controls.Add(this.txtDatabaseName);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtDatabaseName;
        private System.Windows.Forms.Label lblDatabaseName;
        private System.Windows.Forms.Label lblDataSource;
        private System.Windows.Forms.TextBox txtDataSource;
        private System.Windows.Forms.Label lblInitialCatalog;
        private System.Windows.Forms.TextBox txtInitialCatalog;
        private System.Windows.Forms.Label lblUserId;
        private System.Windows.Forms.TextBox txtUserId;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.CheckBox chkTrustedConnection;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Label lblSchema;
        private System.Windows.Forms.TextBox txtSchema;
        private System.Windows.Forms.Button btnDropSprocs;
        private System.Windows.Forms.CheckBox chkOverwriteExisting;
        private System.Windows.Forms.Button btnGenerateDtos;
        private System.Windows.Forms.CheckBox chkOverwriteExistingDtos;
        private System.Windows.Forms.Button btnGenerateLists;
        private System.Windows.Forms.CheckBox chkOverwriteExistingLists;
        private System.Windows.Forms.Button btnTestDataAccess;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnGetCustomSprocs;
        private System.Windows.Forms.TreeView treeCustomSprocs;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnGenerateCustomSprocDtos;
        private System.Windows.Forms.Button btnTestGeneratedDataAccess;
        private System.Windows.Forms.CheckBox chkSelectAll;
        private System.Windows.Forms.Button btnGenerateBusinessObjects;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnGenerateDataAccessClass;
        private System.Windows.Forms.Button btnLoadTables;
        private System.Windows.Forms.CheckBox chkSelectDeselectAllTables;
        private System.Windows.Forms.TreeView treeAvailableTables;
    }
}

