using System;
using System.Collections.Generic;
using System.Text;
using CommonLibrary;

namespace CodeSampleApplication
{
    public class CustomerManager
    {
        DatabaseSmoObjectsAndSettings _databaseSmoObjectsAndSettings = null;

        public CustomerManager(DatabaseSmoObjectsAndSettings databaseSmoObjectsAndSettings)
        {
            _databaseSmoObjectsAndSettings = databaseSmoObjectsAndSettings;
        }
        internal void TestHarnessAddSingleCompositeCustomer()
        {
            Customer customer = new Customer(new Bo.Person(_databaseSmoObjectsAndSettings));

            customer.PersonFirstName = "John";
            customer.PersonMiddleInitial = "C";
            customer.PersonLastName = "Smith";

            //definitely just dummy data for the purpose of this Code Sample App
            Bo.Address address1 = new Bo.Address(_databaseSmoObjectsAndSettings);
            address1.AddressStreet = @"123456 Any Street";
            address1.AddressStateID = 1;
            address1.AddressCityID = 1;
            address1.AddressCountryID = 1;

            customer.AddNewCustomerAddress(address1);

            Bo.Contact contact1 = new Bo.Contact(_databaseSmoObjectsAndSettings);
            contact1.ContactCellPhone = @"555-555-5555";
            contact1.ContactHomePhone = @"222-222-2222";
            contact1.ContactFax = @"999-999-9999";
            contact1.ContactEmail = @"myemailaddress@mydomain.com";

            customer.AddNewCustomerContact(contact1);

            customer.Save(); 
        }
    }
}
