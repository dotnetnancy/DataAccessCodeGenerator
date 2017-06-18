//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.42
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CodeSampleApplication.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using CommonLibrary;
    
    
    [CommonLibrary.CustomAttributes.TableNameAttribute("Person_Contact")]
    public class Person_Contact
    {
        
        private int _personID;
        
        private int _contactID;
        
        private int _purposeID;
        
        private Dictionary<string, bool> _isModifiedDictionary = new Dictionary<string, bool>();
        
        public Person_Contact()
        {
            this.InitializeIsModifiedDictionary();
        }
        
        [CommonLibrary.CustomAttributes.DatabaseColumnAttribute("PersonID")]
        [CommonLibrary.CustomAttributes.PrimaryKey()]
        public virtual int PersonID
        {
            get
            {
                return this._personID;
            }
            set
            {
                this._personID = value;
                this.SetIsModified("PersonID");
            }
        }
        
        [CommonLibrary.CustomAttributes.DatabaseColumnAttribute("ContactID")]
        [CommonLibrary.CustomAttributes.PrimaryKey()]
        public virtual int ContactID
        {
            get
            {
                return this._contactID;
            }
            set
            {
                this._contactID = value;
                this.SetIsModified("ContactID");
            }
        }
        
        [CommonLibrary.CustomAttributes.DatabaseColumnAttribute("PurposeID")]
        [CommonLibrary.CustomAttributes.PrimaryKey()]
        public virtual int PurposeID
        {
            get
            {
                return this._purposeID;
            }
            set
            {
                this._purposeID = value;
                this.SetIsModified("PurposeID");
            }
        }
        
        public virtual Dictionary<string, bool> IsModifiedDictionary
        {
            get
            {
                return this._isModifiedDictionary;
            }
            set
            {
                this._isModifiedDictionary = value;
            }
        }
        
        private void InitializeIsModifiedDictionary()
        {
            this.IsModifiedDictionary.Add("PersonID", false);
            this.IsModifiedDictionary.Add("ContactID", false);
            this.IsModifiedDictionary.Add("PurposeID", false);
        }
        
        private void SetIsModified(string columnName)
        {
            if ((this.IsModifiedDictionary.ContainsKey(columnName) == true))
            {
                IsModifiedDictionary[columnName] = true;
            }
        }
    }
}