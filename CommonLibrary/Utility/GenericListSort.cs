using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace CommonLibrary.Utilty.Sort
{

    public enum CollectionSortDirection
    {
        Ascending = -1,
        Descending = 1
    }

    /// <summary>
    /// this class is used to sort any complex first class object of any type, you must provide the
    /// property name exactly as it is defined in the class for this to work properly.  An enhancement to
    /// this class would be to make the property name any type, but right now it is just a string.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericList<T> : List<T>
    {
        public class GenericListComparer : IComparer<T>
        {

            string _sortFieldName;
            CollectionSortDirection _sortDirection;           

            public GenericListComparer(string sortFieldName)
            {
                this._sortFieldName = sortFieldName;

                // default to ascending order
                this._sortDirection = CollectionSortDirection.Ascending;

            }

            public GenericListComparer(string sortFieldName,
                                                CollectionSortDirection sortDirection)
            {
                this._sortFieldName = sortFieldName;
                this._sortDirection = sortDirection;
                
            }


            public int Compare(T objectX, T objectY)
            {
                // Get the values of the relevant property on the x and y objects
                object valueOfX;
                object valueOfY;  

                valueOfX = objectX.GetType().GetProperty(this._sortFieldName).GetValue(objectX, null);
                valueOfY = objectY.GetType().GetProperty(this._sortFieldName).GetValue(objectY, null);          

                IComparable comparableY = valueOfY as IComparable;
                // Flip the value from whatever it was to the opposite.
                return Flip(comparableY.CompareTo(valueOfX));
            }

            private int Flip(int i)
            {
                return (i * (int)_sortDirection);
            }           
        }

        /// <summary>
        /// this method is available to Sort the List by the Sort direction defined.
        /// </summary>
        /// <param name="sortByField"></param>
        /// <param name="sortDirection"></param>
        public void Sort(string sortByField,
                CollectionSortDirection sortDirection)
        {
            try
            {               
                    GenericListComparer comparer = new GenericListComparer(sortByField, sortDirection);
                    this.Sort(comparer);               
            }
            catch (Exception err)
            {
                string desc = err.Message;
            }           
        }

      

    }

    
}
