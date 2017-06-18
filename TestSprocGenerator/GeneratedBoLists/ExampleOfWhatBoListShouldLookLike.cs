using System;
using System.Collections.Generic;
using System.Text;

using CommonLibrary;

namespace TestSprocGenerator.Business.SingleTable.Bo.List
{
    public class TestTable : List<TestSprocGenerator.Business.SingleTable.Bo.TestTable>
    {
        CommonLibrary.DatabaseSmoObjectsAndSettings _databaseSmoObjectsAndSettings = null;

        CommonLibrary.Base.Database.BaseDataAccess<TestSprocGenerator.Data.SingleTable.Dto.TestTable> _baseDataAccess = null;

        const string FILL_DB_SETTINGS_EXCEPTION = @"Please Fill the DatabaseSmoObjectsAndSettings_Property with a filled
                                      DatabaseSmoObjectsAndSettings class and try again";

        CommonLibrary.Base.Business.BaseBusiness<TestSprocGenerator.Business.SingleTable.Bo.TestTable,
                                                 TestSprocGenerator.Data.SingleTable.Dto.TestTable> _baseBusiness =
            new CommonLibrary.Base.Business.BaseBusiness<TestSprocGenerator.Business.SingleTable.Bo.TestTable,
                                                 TestSprocGenerator.Data.SingleTable.Dto.TestTable>();       


        public TestTable()
        {
        }

        public TestTable(CommonLibrary.DatabaseSmoObjectsAndSettings databaseSmoObjectsAndSettings)
        {
            _databaseSmoObjectsAndSettings = databaseSmoObjectsAndSettings;
            _baseDataAccess = new CommonLibrary.Base.Database.BaseDataAccess<TestSprocGenerator.Data.SingleTable.Dto.TestTable>(_databaseSmoObjectsAndSettings);
            _baseBusiness = new CommonLibrary.Base.Business.BaseBusiness<TestSprocGenerator.Business.SingleTable.Bo.TestTable, TestSprocGenerator.Data.SingleTable.Dto.TestTable>();
        }

        //public TestTable(TestSprocGenerator.Business.SingleTable.Bo.TestTable filledBo,
        //                 CommonLibrary.DatabaseSmoObjectsAndSettings databaseSmoObjectsAndSettings)
        //{
        //    _databaseSmoObjectsAndSettings = databaseSmoObjectsAndSettings;
        //    _baseDataAccess = new CommonLibrary.Base.Database.BaseDataAccess<TestSprocGenerator.Data.SingleTable.Dto.TestTable>(_databaseSmoObjectsAndSettings);
        //}

        public void FillByPrimaryKey(TestSprocGenerator.Business.SingleTable.Bo.TestTable bo)
        {
            CommonLibrary.Enumerations.GetPermutations getPermutation =
                   CommonLibrary.Enumerations.GetPermutations.ByPrimaryKey;
            this.FillByGetPermutation(getPermutation, bo);
        }

        public void FillByCriteriaFuzzy(TestSprocGenerator.Business.SingleTable.Bo.TestTable bo)
        {
              CommonLibrary.Enumerations.GetPermutations getPermutation = 
                  CommonLibrary.Enumerations.GetPermutations.ByFuzzyCriteria;
            this.FillByGetPermutation(getPermutation,bo);
        }

        public void FillByCriteriaExact(TestSprocGenerator.Business.SingleTable.Bo.TestTable bo)
        {
            CommonLibrary.Enumerations.GetPermutations getPermutation =
                  CommonLibrary.Enumerations.GetPermutations.ByExplicitCriteria;
            this.FillByGetPermutation(getPermutation, bo);
        }

        public void FillByGetAll(TestSprocGenerator.Business.SingleTable.Bo.TestTable bo)
        {
            CommonLibrary.Enumerations.GetPermutations getPermutation =
                  CommonLibrary.Enumerations.GetPermutations.AllByColumnMappings;
            this.FillByGetPermutation(getPermutation, bo);
        }


        private void FillByGetPermutation(CommonLibrary.Enumerations.GetPermutations getPermutation,
            TestSprocGenerator.Business.SingleTable.Bo.TestTable bo)
        {
            this.Clear();

            TestSprocGenerator.Data.SingleTable.Dto.TestTable dto =
                (TestSprocGenerator.Data.SingleTable.Dto.TestTable)bo;

            List<TestSprocGenerator.Data.SingleTable.Dto.TestTable> dtosReturned = 
                _baseDataAccess.Get(dto, getPermutation);

            int countDtosReturned = dtosReturned.Count;
            if (countDtosReturned > 0)
            {
                int counter = 0;
                for (counter = 0; counter < countDtosReturned; counter++)
                {
                    TestSprocGenerator.Business.SingleTable.Bo.TestTable boToFill =
                    new TestSprocGenerator.Business.SingleTable.Bo.TestTable();
                    _baseBusiness.FillThisWithDto(dtosReturned[counter], boToFill);
                    this.Add(boToFill);
                }
            }

            //foreach (TestSprocGenerator.Data.SingleTable.Dto.TestTable dtoReturned in dtosReturned)
            //{
            //    TestSprocGenerator.Business.SingleTable.Bo.TestTable boToFill = 
            //        new TestSprocGenerator.Business.SingleTable.Bo.TestTable();
            //    _baseBusiness.FillThisWithDto(dtoReturned, boToFill);
            //    this.Add(boToFill);
            //}          
        }

        public List<TestSprocGenerator.Business.SingleTable.Bo.TestTable> FilterList(TestSprocGenerator.Business.SingleTable.Bo.TestTable criteria)
        {
           return _baseBusiness.FilterList(criteria, (List<TestSprocGenerator.Business.SingleTable.Bo.TestTable>)this);
        }

        private bool BaseDataAccessAvailable()
        {
            bool baseDataAccessAvailable = false;
            if (_baseDataAccess == null)
            {
                if (_databaseSmoObjectsAndSettings != null)
                {
                    _baseDataAccess =
                        new CommonLibrary.Base.Database.BaseDataAccess<TestSprocGenerator.Data.SingleTable.Dto.TestTable>(_databaseSmoObjectsAndSettings);
                    baseDataAccessAvailable = true;
                }
            }
            else
            {
                baseDataAccessAvailable = true;
            }
            return baseDataAccessAvailable;
        }

        public DatabaseSmoObjectsAndSettings DatabaseSmoObjectsAndSettings_Property
        {
            get
            {
                return _databaseSmoObjectsAndSettings;
            }
            set
            {
                _databaseSmoObjectsAndSettings = value;
            }
        }
    }
}
