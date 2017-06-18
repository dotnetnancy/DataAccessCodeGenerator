//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.42
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CodeSampleApplication.Bo
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using CommonLibrary;
    using System.Reflection;
    
    
    public class Person_Address : CodeSampleApplication.Dto.Person_Address
    {
        
        public const string FILL_DB_SETTINGS_EXCEPTION = "Please Fill the DatabaseSmoObjectsAndSettings_Property with a filled DatabaseSmoO" +
            "bjectsAndSettings class and try again";
        
        public const string PRIMARY_KEY_NOT_FOUND_EXCEPTION_VAR_NAME = "PRIMARY_KEY_NOT_FOUND_EXCEPTION_VAR_NAME";
        
        private CommonLibrary.DatabaseSmoObjectsAndSettings _databaseSmoObjectsAndSettings;
        
        private CommonLibrary.Base.Business.BaseBusiness<CodeSampleApplication.Bo.Person_Address, CodeSampleApplication.Dto.Person_Address> _baseBusiness;
        
        private CommonLibrary.Base.Database.BaseDataAccess<CodeSampleApplication.Dto.Person_Address> _baseDataAccess;
        
        public Person_Address()
        {
            _baseBusiness =
                new CommonLibrary.Base.Business.BaseBusiness<CodeSampleApplication.Bo.Person_Address, CodeSampleApplication.Dto.Person_Address>();

        }
        
        public Person_Address(CommonLibrary.DatabaseSmoObjectsAndSettings databaseSmoObjectsAndSettings)
        {
            _databaseSmoObjectsAndSettings = databaseSmoObjectsAndSettings;
            _baseDataAccess = 
                new CommonLibrary.Base.Database.BaseDataAccess<CodeSampleApplication.Dto.Person_Address>(_databaseSmoObjectsAndSettings);
            _baseBusiness = 
                new CommonLibrary.Base.Business.BaseBusiness<CodeSampleApplication.Bo.Person_Address, CodeSampleApplication.Dto.Person_Address>();
        }
        
        public Person_Address(CommonLibrary.DatabaseSmoObjectsAndSettings databaseSmoObjectsAndSettings, CodeSampleApplication.Bo.Person_Address filledBo)
        {
            _databaseSmoObjectsAndSettings = databaseSmoObjectsAndSettings;
            _baseDataAccess = 
                new CommonLibrary.Base.Database.BaseDataAccess<CodeSampleApplication.Dto.Person_Address>(_databaseSmoObjectsAndSettings);
            _baseBusiness = 
                new CommonLibrary.Base.Business.BaseBusiness<CodeSampleApplication.Bo.Person_Address, CodeSampleApplication.Dto.Person_Address>();
            this.FillPropertiesFromBo(filledBo);
        }
        
        public virtual CommonLibrary.DatabaseSmoObjectsAndSettings DatabaseSmoObjectsAndSettings
        {
            get
            {
                return this._databaseSmoObjectsAndSettings;
            }
            set
            {
                this._databaseSmoObjectsAndSettings = value;
            }
        }
        
        private void FillPropertiesFromBo(CodeSampleApplication.Bo.Person_Address filledBo)
        {
            _baseBusiness.FillPropertiesFromBo(filledBo, this);
        }
        
        private void FillThisWithDto(CodeSampleApplication.Dto.Person_Address filledDto)
        {
            _baseBusiness.FillThisWithDto(filledDto, this);
        }
        
        private void FillDtoWithThis()
        {
            _baseBusiness.FillDtoWithThis(this);
        }
        
        private bool BaseDataAccessAvailable()
        {
            bool baseDataAccessAvailable = false;
            if ((_baseDataAccess == null))
            {
                if ((_databaseSmoObjectsAndSettings != null))
                {
                    _baseDataAccess = 
                new CommonLibrary.Base.Database.BaseDataAccess<CodeSampleApplication.Dto.Person_Address>(_databaseSmoObjectsAndSettings);
                }
                baseDataAccessAvailable = true;
            }
            else
            {
                baseDataAccessAvailable = true;
            }
            return baseDataAccessAvailable;
        }
        
        public virtual void Insert()
        {
            if (this.BaseDataAccessAvailable())
            {
                CodeSampleApplication.Dto.Person_Address dto = _baseBusiness.FillDtoWithThis(this);
                CodeSampleApplication.Dto.Person_Address returnDto = _baseDataAccess.Insert(dto);
                this.FillThisWithDto(returnDto);
            }
            else
            {
                throw new System.ApplicationException(FILL_DB_SETTINGS_EXCEPTION);
            }
        }
        
        public virtual void Update()
        {
            if (this.BaseDataAccessAvailable())
            {
                CodeSampleApplication.Dto.Person_Address dto = _baseBusiness.FillDtoWithThis(this);
                CodeSampleApplication.Dto.Person_Address returnDto = _baseDataAccess.Update(dto);
                this.FillThisWithDto(returnDto);
            }
            else
            {
                throw new System.ApplicationException(FILL_DB_SETTINGS_EXCEPTION);
            }
        }
        
        public virtual void Delete()
        {
            if (this.BaseDataAccessAvailable())
            {
                CodeSampleApplication.Dto.Person_Address dto = _baseBusiness.FillDtoWithThis(this);
                CodeSampleApplication.Dto.Person_Address returnDto = _baseDataAccess.Delete(dto);
                this.FillThisWithDto(returnDto);
            }
            else
            {
                throw new System.ApplicationException(FILL_DB_SETTINGS_EXCEPTION);
            }
        }
        
        public virtual void GetByPrimaryKey()
        {
            if (this.BaseDataAccessAvailable())
            {
                CodeSampleApplication.Dto.Person_Address dto = this;
                List<CodeSampleApplication.Dto.Person_Address> returnDto = _baseDataAccess.Get(dto, CommonLibrary.Enumerations.GetPermutations.ByPrimaryKey);
                if ((returnDto.Count > 0))
                {
                    this.FillThisWithDto(returnDto[0]);
                }
                else
                {
                    throw new System.ApplicationException(PRIMARY_KEY_NOT_FOUND_EXCEPTION_VAR_NAME);
                }
            }
            else
            {
                throw new System.ApplicationException(FILL_DB_SETTINGS_EXCEPTION);
            }
        }
    }
}
