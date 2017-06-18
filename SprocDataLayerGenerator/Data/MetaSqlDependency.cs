using System;
using System.Collections.Generic;
using System.Text;

namespace SprocDataLayerGenerator.Data
{
    public class MetaSqlDependency
    {
        private string _referencedObject;

        public string ReferencedObject
        {
            get { return _referencedObject; }
            set { _referencedObject = value; }
        }
        private string _referencedType;

        public string ReferencedType
        {
            get { return _referencedType; }
            set { _referencedType = value; }
        }
        private string _referencingObject;

        public string ReferencingObject
        {
            get { return _referencingObject; }
            set { _referencingObject = value; }
        }
        private string _class;

        public string Class
        {
            get { return _class; }
            set { _class = value; }
        }
        private string _classDesc;

        public string ClassDesc
        {
            get { return _classDesc; }
            set { _classDesc = value; }
        }
        private int _objectId;

        public int ObjectId
        {
            get { return _objectId; }
            set { _objectId = value; }
        }
        private int _columnId;

        public int ColumnId
        {
            get { return _columnId; }
            set { _columnId = value; }
        }
        private int _referencedMajorId;

        public int ReferencedMajorId
        {
            get { return _referencedMajorId; }
            set { _referencedMajorId = value; }
        }
        private int _referencedMinorId;

        public int ReferencedMinorId
        {
            get { return _referencedMinorId; }
            set { _referencedMinorId = value; }
        }
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; }
        }
        private bool _isUpdated;

        public bool IsUpdated
        {
            get { return _isUpdated; }
            set { _isUpdated = value; }
        }
        private bool _isSelectAll;

        public bool IsSelectAll
        {
            get { return _isSelectAll; }
            set { _isSelectAll = value; }
        }

        public MetaSqlDependency()
        {
        }


    }
}
