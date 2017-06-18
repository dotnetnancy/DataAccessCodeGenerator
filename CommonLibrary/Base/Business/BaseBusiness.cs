using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace CommonLibrary.Base.Business
{
    //public class BaseBusiness<T,X> where X : new()
    public class BaseBusiness<T, X> where T : X,new() where X : new()
    {

        private DatabaseSmoObjectsAndSettings _databaseSmoObjectsAndSettings;

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
        

        public BaseBusiness(DatabaseSmoObjectsAndSettings smoSettings)
        {
            _databaseSmoObjectsAndSettings = smoSettings;
        }
    

        public Type GetTypeFromGenericType(T genericType)
        {
            Type returnType = genericType.GetType();
            return returnType;
        }

        public Type GetTypeFromGenericType(X genericType)
        {
            Type returnType = genericType.GetType();
            return returnType;
        }

        public object GetValueFromPropertyColumnMapping(T typePassedIn, string propertyName)
        {
            Type type = typePassedIn.GetType();
            PropertyInfo derivedPropertyInfo = type.GetProperty(propertyName);

            object value = derivedPropertyInfo.GetValue(typePassedIn, null);
            return value;
        }

        public object GetValueFromPropertyColumnMapping(X typePassedIn, string propertyName)
        {
            Type type = typePassedIn.GetType();
            PropertyInfo derivedPropertyInfo = type.GetProperty(propertyName);

            object value = derivedPropertyInfo.GetValue(typePassedIn, null);
            return value;
        }

        public void SetValueFromPropertyColumnMapping(T typePassedIn, string propertyName, object value)
        {
            Type type = typePassedIn.GetType();
            PropertyInfo derivedPropertyInfo = type.GetProperty(propertyName);
            derivedPropertyInfo.SetValue(typePassedIn, value, null);
        }

        public void FillPropertiesFromBo(T filledBo, T boToFill)
        {
            PropertyInfo[] thisProperties = GetTypeFromGenericType(boToFill).GetProperties();
            foreach (PropertyInfo property in thisProperties)
            {
                PropertyInfo boProperty = GetTypeFromGenericType(filledBo).GetProperty(property.Name);
                if (boProperty != null)
                {
                    object value = boProperty.GetValue(filledBo.GetType(), null);
                    property.SetValue(boToFill, value, null);
                }
            }            
        }

        public string[] GetPropertyNameFromIsModifiedDictionary(X oneDto)
        {
            List<string> modifiedPropertyNames = new List<string>();

            Type typePassedIn = oneDto.GetType();

            PropertyInfo[] properties = typePassedIn.GetProperties();

            object isModifiedObject =
                       GetValueFromPropertyColumnMapping(oneDto,
                       CommonLibrary.Constants.ClassCreationConstants.IS_MODIFIED_DICTIONARY_PROPERTY_NAME);

            Dictionary<string, bool> isModifiedDictionary = null;

            if (isModifiedObject is Dictionary<string, bool>)
            {
                isModifiedDictionary = (Dictionary<string, bool>)isModifiedObject;
            }

            foreach (PropertyInfo property in properties)
            {
                string columnName = null;

                object[] attributes = property.GetCustomAttributes(false);

                foreach (Attribute attribute in attributes)
                {
                    if (attribute is CommonLibrary.CustomAttributes.DatabaseColumnAttribute)
                    {
                        columnName = ((CommonLibrary.CustomAttributes.DatabaseColumnAttribute)attribute).DatabaseColumn;
                    }
                }

                if (columnName != null && isModifiedDictionary != null)
                {
                    if (isModifiedDictionary.ContainsKey(columnName))
                    {
                        bool isModified = isModifiedDictionary[columnName];
                        if (isModified)
                        {
                            modifiedPropertyNames.Add(property.Name);
                        }
                    }
                }

            }
            return modifiedPropertyNames.ToArray();
        }

         public List<T> FilterList(T bo, List<T> filterList)
        {
            List<X> dtoFilteredList = new List<X>();
            List<T> returnList = new List<T>();

            CommonLibrary.Utility.Filter.GenericListFiltering<X> genericListFiltering = null;

            X dto = (X)bo;
            string[] propertiesToFilter = GetPropertyNameFromIsModifiedDictionary(dto);
            if (propertiesToFilter.Length > 0)
            {
                List<X> listToFilter = new List<X>();
                foreach (T filterItem in filterList)
                {
                    listToFilter.Add((X)filterItem);
                }

                genericListFiltering =
                    new CommonLibrary.Utility.Filter.GenericListFiltering<X>(dto, propertiesToFilter, listToFilter);

                dtoFilteredList = genericListFiltering.FindAll(genericListFiltering.HasFilterFieldValuesMultipleCriteria);
            }

            foreach (X dtoReturned in dtoFilteredList)
            {
                T newBo = new T();
                FillThisWithDto(dtoReturned, newBo);
                returnList.Add(newBo);
            }
             
            return returnList;  

        }

        public void FillThisWithDto(X dto, T bo)
        {
            PropertyInfo[] thisProperties = GetTypeFromGenericType(bo).GetProperties();
            foreach (PropertyInfo property in thisProperties)
            {
                PropertyInfo boProperty = GetTypeFromGenericType(dto).GetProperty(property.Name);
                if (boProperty != null)
                {
                    object value = boProperty.GetValue(dto, null);
                    property.SetValue(bo, value, null);
                }               
            }
        }

        public X FillDtoWithThis(T bo)
        {
            X dtoReturn =
                new X();

            PropertyInfo[] dtoReturnProperties = GetTypeFromGenericType(dtoReturn).GetProperties();

            foreach (PropertyInfo property in dtoReturnProperties)
            {
                PropertyInfo thisProperty = GetTypeFromGenericType(bo).GetProperty(property.Name);
                if (thisProperty != null)
                {
                    object value = thisProperty.GetValue(bo, null);
                    property.SetValue(dtoReturn, value, null);
                }
            }
            return dtoReturn;
        }

    }
}
