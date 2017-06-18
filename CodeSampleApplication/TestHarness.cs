using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CommonLibrary;

namespace CodeSampleApplication
{
    public partial class frmTestHarness : Form
    {
        public frmTestHarness()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        

        }

        /// <summary>
        /// inserts an arbitrary composite customer into the database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InsertCompositeCustomer_Click(object sender, EventArgs e)
        {
            try
            {
                //perhaps an ActiveContext type of object would be available from a global location and would already have
                //this DatabaseSmoObjectsAndSettings instantiated and configured from a
                //storage medium like a config file etc.  For the purpose of this testing
                //just going to instantiate this with arbitrary values to show how it would work           

                string databaseName = "CodeSampleApplication";
                string dataSource = @"IBM-5C1076B185C";
                string initialCatalog = "CodeSampleApplication";
                string userId = string.Empty;
                string password = string.Empty;
                bool trustedConnection = true;
                string schema = "dbo";

                CommonLibrary.DatabaseSmoObjectsAndSettings databaseSmoObjectsAndSettings =
                                                new CommonLibrary.DatabaseSmoObjectsAndSettings(databaseName,
                                                                                                dataSource,
                                                                                                 initialCatalog,
                                                                                                 userId,
                                                                                                 password,
                                                                                                 trustedConnection);

                //you would probably only want to instantiate this once, just for testing purposes instantiate it
                //with every button click
                CustomerController customerController = new CustomerController(databaseSmoObjectsAndSettings);
                customerController.TestHarnessAddSingleCompositeCustomer();
            }
            catch (Exception ex)
            {
                //normally would have an assembly/classes available for the logging and reporting
                //of exceptions that would encapsulate this

                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.Append("Exception Has Occurred:  ");
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append("Message:  ");
                stringBuilder.Append(ex.Message);
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append("StackTrace:  ");
                stringBuilder.Append(ex.StackTrace);

                MessageBox.Show(stringBuilder.ToString());

            }

            MessageBox.Show("Composite Customer Inserted");
  
        }
    }
}