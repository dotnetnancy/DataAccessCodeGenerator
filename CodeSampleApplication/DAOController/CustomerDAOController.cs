using System;
using System.Collections.Generic;
using System.Text;
using CodeSampleApplication;
using CodeSampleApplication.Bo;
using System.Data.SqlClient;
using CommonLibrary.Base.Database;
using CommonLibrary.Enumerations;
using CommonLibrary;

namespace CodeSampleApplication.DAOController
{
    class CustomerDAOController
    {
        DatabaseSmoObjectsAndSettings _databaseSmoObjectsAndSettings = null;

        private const string MORE_THAN_ONE_PERSON_RECORD_RETURNED_ERROR_MESSAGE = "Programmer Exception:  More than one Person returned from the Database for the given Key";  

        public CustomerDAOController(DatabaseSmoObjectsAndSettings databaseSmoObjectsAndSettings)
        {
            _databaseSmoObjectsAndSettings = databaseSmoObjectsAndSettings;
        }
        /// <summary>
        /// save the customer and its dependent objects wrapped in one SqlTransaction
        /// </summary>
        /// <param name="customer"></param>
        internal void Save(Customer customer)
        {
            BaseDatabase _baseDatabase = new BaseDatabase();
            
            SqlTransaction transaction = null; 
          


            using (SqlConnection sqlConnection = new SqlConnection(_databaseSmoObjectsAndSettings.ConnectionString))
            {
                // begin and the rest are set to enlist
                BaseDatabase.TransactionBehavior transactionBehavior =
                    BaseDatabase.TransactionBehavior.Begin;

                try
                {

                    SavePerson(customer,
                        sqlConnection,
                        ref transaction,
                        transactionBehavior);

                    SaveAddress(customer,
                        sqlConnection,
                        ref transaction,
                        transactionBehavior);

                    SaveContact(customer,
                        sqlConnection,
                        ref transaction,
                        transactionBehavior);

                    _baseDatabase.CommitTransaction(ref transaction);

                    ResetModifiedState(customer);

                }

                catch (BeginTransactionException beginTrans)
                {                   
                    _baseDatabase.RollbackTransaction(ref transaction);
                    //this is a developer specific type of exception               
                    throw new ApplicationException(beginTrans.Message);
                }

                catch (Exception ex)
                {
                    //ensure transaction is rolled back
                    _baseDatabase.RollbackTransaction(ref transaction);
                    //bubble it back up to consumer for graceful recovery
                    throw ex;
                }
            }

        }

        private void ResetModifiedState(Customer customer)
        {
            customer.ModifiedState = ModifiedState.Unchanged;

            foreach (Address address in customer.CustomerAddresses)
            {
                address.ModifiedState = ModifiedState.Unchanged;
            }

            foreach (Contact contact in customer.CustomerContacts)
            {
               contact.ModifiedState = ModifiedState.Unchanged;
            }
        }

        private void SaveContact(Customer customer,
                                SqlConnection sqlConnection,
                                ref SqlTransaction transaction,
                                BaseDatabase.TransactionBehavior transactionBehavior)
        {
            object returnValueFromExecution = null;

             foreach (Contact contact in customer.CustomerContacts)
            {
                switch (contact.ModifiedState)
                {
                    case ModifiedState.New:
                        {
                            contact.Insert(sqlConnection,
                                ref transaction,
                                transactionBehavior,
                                ref returnValueFromExecution);

                            break;
                        }

                    case ModifiedState.Modified:
                        {
                            contact.Update(sqlConnection,
                                ref transaction,
                                transactionBehavior,
                                ref returnValueFromExecution);
                            break;
                        }

                    case ModifiedState.Deleted:
                        {
                            contact.Delete(sqlConnection,
                               ref transaction,
                               transactionBehavior,
                               ref returnValueFromExecution);
                            break;
                        }
                }
            }
        }

        private void SaveAddress(Customer customer,
                                SqlConnection sqlConnection,
                                ref SqlTransaction transaction,
                                BaseDatabase.TransactionBehavior transactionBehavior)
        {
            object returnValueFromExecution = null;

            foreach (Address address in customer.CustomerAddresses)
            {

                switch (address.ModifiedState)
                {
                    case ModifiedState.New:
                        {
                            address.Insert(sqlConnection,
                                ref transaction,
                                transactionBehavior,
                                ref returnValueFromExecution);

                            break;
                        }

                    case ModifiedState.Modified:
                        {
                            address.Update(sqlConnection,
                                ref transaction,
                                transactionBehavior,
                                ref returnValueFromExecution);
                            break;
                        }

                    case ModifiedState.Deleted:
                        {
                            address.Delete(sqlConnection,
                               ref transaction,
                               transactionBehavior,
                               ref returnValueFromExecution);
                            break;
                        }
                }
            }
        }

        private void SavePerson(Customer customer,
                                SqlConnection sqlConnection,
                                ref SqlTransaction transaction,
                                BaseDatabase.TransactionBehavior transactionBehavior)
        {
            object returnValueFromExecution = null;

            switch (customer.CustomerModifiedState)
            {
                case ModifiedState.New:
                    {
                        ((Person)customer).Insert(sqlConnection,
                            ref transaction,
                            transactionBehavior,
                            ref returnValueFromExecution);

                        break;
                    }

                case ModifiedState.Modified:
                    {
                        ((Person)customer).Update(sqlConnection,
                            ref transaction,
                            transactionBehavior,
                            ref returnValueFromExecution);
                        break;
                    }

                case ModifiedState.Deleted:
                    {
                        ((Person)customer).Delete(sqlConnection,
                           ref transaction,
                           transactionBehavior,
                           ref returnValueFromExecution);
                        break;
                    }
            }
        }


        internal Person GetExistingCustomer(int personId, ref List<Address> addresses, ref List<Contact> contacts)
        {            
            //instantiate DAO
            BaseDataAccess<Dto.Person> personDAO =
                new BaseDataAccess<Dto.Person>(_databaseSmoObjectsAndSettings);

            //instantiate dto
            Dto.Person personDTO = new Dto.Person();

            Bo.Person personToReturn = new Bo.Person(_databaseSmoObjectsAndSettings);

            //set the property, in this case we are going to use this as the search criteria
            personDTO.PersonID = personId;           

            //we know that the criteria that we set is the primary key of the database table, so 
            //choose that as the search permutation
            List<Dto.Person> listOfPersons =
                personDAO.Get(personDTO, GetPermutations.ByPrimaryKey);            

            if (listOfPersons != null)
            {
                //there should only be one
                if (listOfPersons.Count == 1)
                {
                    personToReturn.PersonID = personId;

                    personToReturn.GetByPrimaryKey();

                    addresses = GetExistingAddresses(personId);
                    contacts = GetExistingContacts(personId);

                    return personToReturn;
                }
                else
                {
                    throw new ApplicationException(MORE_THAN_ONE_PERSON_RECORD_RETURNED_ERROR_MESSAGE);
                }
            }
            else
            {
                //if there is no person then there cannot be dependent address or contact records
                return personToReturn;
            }
        }

        private List<Contact> GetExistingContacts(int personId)
        {

            List<Contact> contactsToReturn = new List<Contact>();

            Person_Contact xref = new Person_Contact(_databaseSmoObjectsAndSettings);

            xref.PersonID = personId;

            BaseDataAccess<Person_Contact> xrefDAO =
                new BaseDataAccess<Person_Contact>(_databaseSmoObjectsAndSettings);

            //this get permutation will check the modified state of each property in the dto
            //if modified it is added to the exact criteria list in this case just the personID
            List<Person_Contact> thisPersonsContacts =
                xrefDAO.Get(xref, GetPermutations.ByExplicitCriteria);

            foreach (Person_Contact person_contact in thisPersonsContacts)
            {
                Bo.Contact contact = new Bo.Contact(_databaseSmoObjectsAndSettings);
                contact.ContactID = person_contact.ContactID;
                contact.GetByPrimaryKey();
                contactsToReturn.Add(contact);
            }

            return contactsToReturn;
        }


        private List<Address> GetExistingAddresses(int personId)
        {

            List<Address> addressesToReturn = new List<Address>();

            Person_Address xref = new Person_Address(_databaseSmoObjectsAndSettings);

            xref.PersonID = personId;

            BaseDataAccess<Person_Address> xrefDAO = 
                new BaseDataAccess<Person_Address>(_databaseSmoObjectsAndSettings);

            //this get permutation will check the modified state of each property in the dto
            //if modified it is added to the exact criteria list in this case just the personID
            List<Person_Address> thisPersonsAddresses = 
                xrefDAO.Get(xref, GetPermutations.ByExplicitCriteria);

            foreach (Person_Address person_address in thisPersonsAddresses)
            {
                Bo.Address address = new Bo.Address(_databaseSmoObjectsAndSettings);
                address.AddressID = person_address.AddressID;
                address.GetByPrimaryKey();
                addressesToReturn.Add(address);
            }

            return addressesToReturn;            
        }

        internal bool DoesCustomerExist(int personID)
        {
            bool doesCustomerExist = false;

            //instantiate DAO
            BaseDataAccess<Dto.Person> personDAO =
                new BaseDataAccess<Dto.Person>(_databaseSmoObjectsAndSettings);

            //instantiate dto
            Dto.Person personDTO = new Dto.Person();

            Bo.Person personToReturn = new Bo.Person(_databaseSmoObjectsAndSettings);

            //set the property, in this case we are going to use this as the search criteria
            personDTO.PersonID = personID;

            //we know that the criteria that we set is the primary key of the database table, so 
            //choose that as the search permutation
            List<Dto.Person> listOfPersons =
                personDAO.Get(personDTO, GetPermutations.ByPrimaryKey);

            if (listOfPersons.Count > 0)
            {
                doesCustomerExist = true;
            }

            return doesCustomerExist;
        }
    }
}
