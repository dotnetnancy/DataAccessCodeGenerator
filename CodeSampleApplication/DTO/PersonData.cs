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
    
    
    [CommonLibrary.CustomAttributes.TableNameAttribute("Person")]
    public class Person
    {
        
        private string _personFirstName;
        
        private string _personMiddleInitial;
        
        private string _personLastName;
        
        private int _personID;
        
        private Dictionary<string, bool> _isModifiedDictionary = new Dictionary<string, bool>();
        
        public Person()
        {
            this.InitializeIsModifiedDictionary();
        }
        
        [CommonLibrary.CustomAttributes.DatabaseColumnAttribute("PersonFirstName")]
        public virtual string PersonFirstName
        {
            get
            {
                return this._personFirstName;
            }
            set
            {
                this._personFirstName = value;
                this.SetIsModified("PersonFirstName");
            }
        }
        
        [CommonLibrary.CustomAttributes.DatabaseColumnAttribute("PersonMiddleInitial")]
        public virtual string PersonMiddleInitial
        {
            get
            {
                return this._personMiddleInitial;
            }
            set
            {
                this._personMiddleInitial = value;
                this.SetIsModified("PersonMiddleInitial");
            }
        }
        
        [CommonLibrary.CustomAttributes.DatabaseColumnAttribute("PersonLastName")]
        public virtual string PersonLastName
        {
            get
            {
                return this._personLastName;
            }
            set
            {
                this._personLastName = value;
                this.SetIsModified("PersonLastName");
            }
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
            this.IsModifiedDictionary.Add("PersonFirstName", false);
            this.IsModifiedDictionary.Add("PersonMiddleInitial", false);
            this.IsModifiedDictionary.Add("PersonLastName", false);
            this.IsModifiedDictionary.Add("PersonID", false);
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
