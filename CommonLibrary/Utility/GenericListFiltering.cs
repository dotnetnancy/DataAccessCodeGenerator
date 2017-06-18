using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLibrary.Utility.Filter
{
    /// <summary>
    /// this class can be used to filter on any Type, the implementation uses variable names with "dto" in them
    /// which may be misleading.  You can use this class with dto's or with other first class objects where you
    /// are looking for a particular property in a Generic List.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericListFiltering<T> : List<T>
    {
        string  _filterFieldName = string.Empty;
        T _dtoCriteria;
        string[] _filterFieldsArray = null;
        
        /// <summary>
        /// this constructor allows filtering on one property, but you must fill the list after construction
        /// </summary>
        /// <param name="dtoCriteria"></param>
        /// <param name="filterFieldName"></param>
        public GenericListFiltering(T dtoCriteria, string  filterFieldName)
        {
            _filterFieldName = filterFieldName;
            _dtoCriteria = dtoCriteria;
        }

        /// <summary>
        /// allows filtering on one property and provide the list that is to be filtered upon construction
        /// </summary>
        /// <param name="dtoCriteria"></param>
        /// <param name="filterFieldName"></param>
        /// <param name="listToFilter"></param>
        public GenericListFiltering(T dtoCriteria, string filterFieldName, List<T> listToFilter)
        {
            _filterFieldName = filterFieldName;
            _dtoCriteria = dtoCriteria;
            this.AddRange(listToFilter);
        }

        /// <summary>
        /// using this constructor, the consumer provides multiple criteria to filter on, at this point
        /// it is implemented using "AND".  All must match to return the item.  Perhaps later on we could
        /// introduce using OR, LIKE, NOT, etc.
        /// </summary>
        /// <param name="dtoCriteria"></param>
        /// <param name="filterFieldsArray"></param>
        /// <param name="listToFilter"></param>
        public GenericListFiltering(T dtoCriteria, string[] filterFieldsArray, List<T> listToFilter)
        {
            _dtoCriteria = dtoCriteria;
            _filterFieldsArray = filterFieldsArray;
            this.AddRange(listToFilter);
        }

        public bool HasFilterFieldValuesMultipleCriteria(T dtoIn)
        {
            bool found = false;

            object[] dtoValues = new object [_filterFieldsArray.Length];            
            object[] dtoCriteriaValues = new object[_filterFieldsArray.Length];

            int ordinal = 0;
            List<int> comparisonValues = new List<int>();
            foreach (string strPropName in _filterFieldsArray)
            {
                dtoValues[ordinal] = dtoIn.GetType().GetProperty(strPropName).GetValue(dtoIn, null);
                dtoCriteriaValues[ordinal] = _dtoCriteria.GetType().GetProperty(strPropName).GetValue(_dtoCriteria, null);
                IComparable comparable = dtoCriteriaValues[ordinal] as IComparable;
                comparisonValues.Add(comparable.CompareTo(dtoValues[ordinal]));
                ordinal++;
            }

            if (comparisonValues.TrueForAll(IsMatchValue))
            {
                found = true;
            }
            return found;
        }

        public bool IsMatchValue(int val)
        {
            bool found = false;
            if (val == 0)
            {
                found = true;
            }
            return found;
        }

        public bool HasFilterFieldValue(T dtoIn)
        {
            bool found = false;
            object dtoValue = dtoIn.GetType().GetProperty(_filterFieldName).GetValue(dtoIn, null);
            object dtoCriteria = _dtoCriteria.GetType().GetProperty(_filterFieldName).GetValue(_dtoCriteria, null);

            IComparable comparable = dtoCriteria as IComparable;
            
            int result = comparable.CompareTo(dtoValue);

            if (result == 0)
            {
                found = true;
            }
            return found;
        }
    }

}
