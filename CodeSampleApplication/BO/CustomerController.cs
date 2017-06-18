using System;
using System.Collections.Generic;
using System.Text;
using CommonLibrary;

namespace CodeSampleApplication
{
    /// <summary>
    /// Generally my pattern is to create a controller class that is for consumption by the UI which contains a UI representation
    /// of the business.  For example the UI representation may be a Grid with rows columns and cells.  This class also contains 
    /// an instance of a Manager class which encapsulates the calls to the business object/s or business unit that it represents
    /// </summary>
    public class CustomerController
    {
        CustomerManager _customerManager = null;
        DatabaseSmoObjectsAndSettings _databaseSmoObjectsAndSettings = null;
        
        //GridUIRepresentation _gridUIRepresentation = new _gridUIRepresentation();

        public CustomerController(DatabaseSmoObjectsAndSettings databaseSmoObjectsAndSettings)
        {
            _databaseSmoObjectsAndSettings = databaseSmoObjectsAndSettings;
            _customerManager = new CustomerManager(databaseSmoObjectsAndSettings);
            //_gridUIRepresentation.LoadGrid(_customerManager);
        }

        public void TestHarnessAddSingleCompositeCustomer()
        {
            _customerManager.TestHarnessAddSingleCompositeCustomer();
            //_gridUIRepresentation.RefreshGrid(_customerManager);
        }
    }
}
