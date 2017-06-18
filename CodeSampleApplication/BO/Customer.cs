using System;
using System.Collections.Generic;
using System.Text;
using CodeSampleApplication.Bo;
using CodeSampleApplication.DAOController;
using CommonLibrary.Enumerations;

namespace CodeSampleApplication
{
    public class Customer : Person
    {
        /// <summary>
        /// customer addresses if they exist for this personID
        /// </summary>
        List<Address> _customerAddresses = new List<Address>();    
    
        /// <summary>
        /// contact records if they exist for this personID
        /// </summary>
        List<Contact> _customerContacts = new List<Contact>();

        /// <summary>
        /// DAOController class orchestrates the business objects to data layer communication, translation and method calls
        /// </summary>
        CustomerDAOController _customerDaoController = null;       
         
        /// <summary>
        /// constructor to provide property values, based on the existence/or not of the primary key, the object
        /// is loaded appropriately and its modified state for a later save operation is evaluated
        /// </summary>
        /// <param name="person"></param>
        public Customer(Person person) :base(person.DatabaseSmoObjectsAndSettings)
        {
            _customerDaoController = new CustomerDAOController(person.DatabaseSmoObjectsAndSettings);

           List<Address> addresses = new List<Address>();
            List<Contact> contacts = new List<Contact>();

             if (person != null)
             {
                 //if person exists in db, fill this object with existing information
                 if (_customerDaoController.DoesCustomerExist(person.PersonID))
                 {
                     Person existingPerson = _customerDaoController.GetExistingCustomer(person.PersonID, ref addresses, ref contacts);
                     Load(existingPerson, addresses, contacts);
                 }
                 else
                 {
                     //this is a new person
                     person.ModifiedState = ModifiedState.New;
                     Load(person, addresses, contacts);
                 }
             }            
        }

        /// <summary>
        /// Load from person, addresses and contacts provided
        /// </summary>
        /// <param name="person"></param>
        /// <param name="addresses"></param>
        /// <param name="contacts"></param>
        private void Load(Person person, List<Address> addresses, List<Contact> contacts)
        {
            base.PersonID = person.PersonID;
            base.PersonFirstName = person.PersonFirstName;
            base.PersonMiddleInitial = person.PersonMiddleInitial;
            base.PersonLastName = person.PersonLastName;
            base.ModifiedState = person.ModifiedState;
            base.DatabaseSmoObjectsAndSettings = person.DatabaseSmoObjectsAndSettings;            

            Load(addresses);
            Load(contacts);
        }

        /// <summary>
        /// load addresses 
        /// </summary>
        /// <param name="addresses"></param>
        private void Load(List<Address> addresses)
        {   
            _customerAddresses.Clear();
            _customerAddresses = addresses;
        }

        /// <summary>
        /// load contacts 
        /// </summary>
        /// <param name="contacts"></param>
        private void Load(List<Contact> contacts)
        {
            _customerContacts.Clear();
            _customerContacts = contacts;
        }

       /// <summary>
       /// calls to the controller that manages the saving of this composite object
       /// </summary>
       public void Save()
        {          
            _customerDaoController.Save(this);
        }

        /// <summary>
        /// override the base Property to evaluate the modified state
        /// </summary>
        public override string PersonFirstName
        {
            get
            {
                return base.PersonFirstName;
            }
            set
            {
                base.PersonFirstName = value;
                if (ModifiedState != ModifiedState.New)
                {
                    ModifiedState = ModifiedState.Modified;
                }
            }
        }

        /// <summary>
        /// override the base Property to evaluate the modified state
        /// </summary>
        public override string PersonLastName
        {
            get
            {
                return base.PersonLastName;
            }
            set
            {
                base.PersonLastName = value;
                if (ModifiedState != ModifiedState.New)
                {
                    ModifiedState = ModifiedState.Modified;
                }
            }
        }

        /// <summary>
        /// override the base Property to evaluate the modified state
        /// </summary>
        public override string PersonMiddleInitial
        {
            get
            {
                return base.PersonMiddleInitial;
            }
            set
            {
                base.PersonMiddleInitial = value;
                if (ModifiedState != ModifiedState.New)
                {
                    ModifiedState = ModifiedState.Modified;
                }
            }
        }

        /// <summary>
        /// add a new customer contact
        /// </summary>
        /// <param name="contact"></param>
        public void AddNewCustomerContact(Contact contact)
        {
            contact.ModifiedState = ModifiedState.New;
            _customerContacts.Add(contact);
        }

        /// <summary>
        /// add a new customer address
        /// </summary>
        /// <param name="address"></param>
        public void AddNewCustomerAddress(Address address)
        {
            address.ModifiedState = ModifiedState.New;
            _customerAddresses.Add(address);
        }

        /// <summary>
        /// Dictionary to store any dependent Adresses, keyed on AddressID property
        /// </summary>
        public List<Address> CustomerAddresses
        {
            get { return _customerAddresses; }
            private set { _customerAddresses = value; }
        }

        /// <summary>
        /// Dictionary to store any dependent Contacts, keyed on ContactID property
        /// </summary>
        public List<Contact> CustomerContacts
        {
            get { return _customerContacts; }
            private set { _customerContacts = value; }
        }

        /// <summary>
        ///  modified state
        /// </summary>
        public ModifiedState CustomerModifiedState
        {
            get
            {
                return this.ModifiedState;
            }
        }     
       
    }
}
