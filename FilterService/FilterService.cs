using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using ReportBuilderAjax.Web.Common;
using ReportBuilderAjax.Web;

namespace ReportBuilderAjax.Web.Services
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class FilterService
    {
        private const int DEFAULT_MAX_COMBO_RECORD_COUNT = 200;
        private const int DEFAULT_EMAIL_SEARCH_ITEMS_PER_PAGE_COUNT = 10;
        private const int DEFAULT_MAX_REPORT_RECORD_COUNT = 65536;

        public int MaxComboRecordCount
        {
            get
            {
                if (ConfigurationManager.AppSettings["MaxComboRecordCount"] != null)
                {
                    return int.Parse(ConfigurationManager.AppSettings["MaxComboRecordCount"]);
                }
                return DEFAULT_MAX_COMBO_RECORD_COUNT;
            }
        }

        public int EmailSearchItemsPerPageCount
        {
            get
            {
                if (ConfigurationManager.AppSettings["EmailSearchItemsPerPageCount"] != null)
                {
                    return int.Parse(ConfigurationManager.AppSettings["EmailSearchItemsPerPageCount"]);
                }
                return DEFAULT_EMAIL_SEARCH_ITEMS_PER_PAGE_COUNT;
            }
        }

        private int MaxReportRecordCount()
        {
            if (ConfigurationManager.AppSettings["MaxReportRecordCount"] != null)
            {
                return int.Parse(ConfigurationManager.AppSettings["MaxReportRecordCount"]);
            }
            return DEFAULT_MAX_REPORT_RECORD_COUNT;
        }

        private List<Members> getMemberListFromDB(string userid, string assocNum, string filterValue, string selectedValue)
        {
            List<Members> resultList = new List<Members>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_MEMBERS_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_USER_ID, userid,
                                                                Consts.SP_PARAM_ASSN_NUM, assocNum,
                                                                Consts.SP_PARAM_NUM_RECORDS, MaxComboRecordCount,
                                                                Consts.SP_PARAM_MEMBER_NAME_FILTER, filterValue,
                                                                Consts.SP_PARAM_MEMBER_NUMBER_FILTER, selectedValue
                                                            });

            if (dataTable != null && dataTable.Rows != null)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    resultList.Add(new Members { Description = row["description"].ToString(), State = row["state"].ToString(), Value = row["value"].ToString() });
                }
            }
            return resultList;
        }

        private List<Location> getLocationListFromDB(string userid, string assocNum, string memberNum, string filterValue, string selectedValue)
        {
            List<Location> resultList = new List<Location>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_LOCATIONS_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_USER_ID, userid,
                                                                Consts.SP_PARAM_ASSN_NUM, assocNum,
                                                                Consts.SP_PARAM_MEMBER_NUM, memberNum,
                                                                Consts.SP_PARAM_NUM_RECORDS, MaxComboRecordCount,
                                                                Consts.SP_PARAM_MEMBER_NAME_FILTER, filterValue,
                                                                Consts.SP_PARAM_LOCATION, selectedValue
                                                            });

            if (dataTable != null && dataTable.Rows != null)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    resultList.Add(new Location
                    {
                        Address1 = row["address1"].ToString(),
                        Address2 = row["address2"].ToString(),
                        City = row["city"].ToString(),
                        Description = row["description"].ToString(),
                        DescriptionAddress = row["descriptionAddress"].ToString(),
                        State = row["state"].ToString(),
                        Value = row["value"].ToString(),
                        Zip = row["zip"].ToString()
                    });
                }
            }
            return resultList;
        }

        private List<GroupCode> getGroupCode1ListFromDB(string memberNum, string location, string filterValue, string selectedValue)
        {
            List<GroupCode> resultList = new List<GroupCode>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_GROUP1_CODES_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_MEMBER_NUM, memberNum,
                                                                Consts.SP_PARAM_LOCATION, location,
                                                                Consts.SP_PARAM_INCLUDE_INACTIVE, false,
                                                                Consts.SP_PARAM_NUM_RECORDS, MaxComboRecordCount,
                                                                Consts.SP_PARAM_GROUP_CODE_NAME_FILTER, filterValue,
                                                                Consts.SP_PARAM_GROUP_CODE, selectedValue
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    resultList.Add(new GroupCode { Description = row["description"].ToString(), Value = row["value"].ToString() });
                }
            }

            return resultList;
        }

        private List<GroupCode> getGroupCode2ListFromDB(string memberNum, string location, string group1Code, string filterValue, string selectedValue)
        {
            List<GroupCode> resultList = new List<GroupCode>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_GROUP2_CODES_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_MEMBER_NUM, memberNum,
                                                                Consts.SP_PARAM_LOCATION, location,
                                                                Consts.SP_PARAM_GROUP1_CODE, group1Code,
                                                                Consts.SP_PARAM_INCLUDE_INACTIVE, false,
                                                                Consts.SP_PARAM_NUM_RECORDS, MaxComboRecordCount,
                                                                Consts.SP_PARAM_GROUP_CODE_NAME_FILTER, filterValue,
                                                                Consts.SP_PARAM_GROUP_CODE, selectedValue
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<GroupCode> gcList = new List<GroupCode>();
                foreach (DataRow row in dataTable.Rows)
                {
                    resultList.Add(new GroupCode { Description = row["description"].ToString(), Value = row["value"].ToString() });
                }
            }

            return resultList;
        }

        private List<GroupCode> getGroupCode3ListFromDB(string memberNum, string location, string group1Code, string group2Code, string filterValue, string selectedValue)
        {
            List<GroupCode> resultList = new List<GroupCode>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_GROUP3_CODES_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_MEMBER_NUM, memberNum,
                                                                Consts.SP_PARAM_LOCATION, location,
                                                                Consts.SP_PARAM_GROUP1_CODE, group1Code,
                                                                Consts.SP_PARAM_GROUP2_CODE, group2Code,
                                                                Consts.SP_PARAM_INCLUDE_INACTIVE, false,
                                                                Consts.SP_PARAM_NUM_RECORDS, MaxComboRecordCount,
                                                                Consts.SP_PARAM_GROUP_CODE_NAME_FILTER, filterValue,
                                                                Consts.SP_PARAM_GROUP_CODE, selectedValue
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<GroupCode> gcList = new List<GroupCode>();
                foreach (DataRow row in dataTable.Rows)
                {
                    resultList.Add(new GroupCode { Description = row["description"].ToString(), Value = row["value"].ToString() });
                }
            }

            return resultList;
        }

        private List<GroupCode> getGroupCode4ListFromDB(string memberNum, string location, string group1Code, string group2Code, string group3Code, string filterValue, string selectedValue)
        {
            List<GroupCode> resultList = new List<GroupCode>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_GROUP4_CODES_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_MEMBER_NUM, memberNum,
                                                                Consts.SP_PARAM_LOCATION, location,
                                                                Consts.SP_PARAM_GROUP1_CODE, group1Code,
                                                                Consts.SP_PARAM_GROUP2_CODE, group2Code,
                                                                Consts.SP_PARAM_GROUP3_CODE, group3Code,
                                                                Consts.SP_PARAM_INCLUDE_INACTIVE, false,
                                                                Consts.SP_PARAM_NUM_RECORDS, MaxComboRecordCount,
                                                                Consts.SP_PARAM_GROUP_CODE_NAME_FILTER, filterValue,
                                                                Consts.SP_PARAM_GROUP_CODE, selectedValue
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<GroupCode> gcList = new List<GroupCode>();
                foreach (DataRow row in dataTable.Rows)
                {
                    resultList.Add(new GroupCode { Description = row["description"].ToString(), Value = row["value"].ToString() });
                }
            }

            return resultList;
        }

        private List<PlaintiffAttorney> getPlaintiffAttorneysListFromDB(string userid, string assocNum)
        {

            List<PlaintiffAttorney> resultList = new List<PlaintiffAttorney>();
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.PlaintiffAttorneyList = new List<PlaintiffAttorney>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_PLAINTIFF_ATTORNEY_SELECT, CommandType.StoredProcedure,
                                                       new object[]
                                                            {
                                                                Consts.SP_PARAM_USER_ID, userid,
                                                                Consts.SP_PARAM_ASSN_NUM, assocNum
                                                            });

            if (dataTable != null && dataTable.Rows != null)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    resultList.Add(new PlaintiffAttorney { Description = row["description"].ToString().ToUpper(), Value = row["value"].ToString(), FirmName = row["FirmName"].ToString() });
                }
            }
            return resultList;
        }

        private List<PlaintiffFirm> getPlaintiffFirmsListFromDB(string userid, string assocNum)
        {

            List<PlaintiffFirm> resultList = new List<PlaintiffFirm>();
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.PlaintiffFirmList = new List<PlaintiffFirm>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_PLAINTIFF_FIRM_SELECT, CommandType.StoredProcedure,
                                                       new object[]
                                                            {
                                                                Consts.SP_PARAM_USER_ID, userid,
                                                                Consts.SP_PARAM_ASSN_NUM, assocNum
                                                            });

            if (dataTable != null && dataTable.Rows != null)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    resultList.Add(new PlaintiffFirm { Description = row["description"].ToString().ToUpper(), Value = row["value"].ToString() });
                }
            }
            return resultList;
        }

        [OperationContract]
        public FilterComboObj GetMembers(string userid, string assocNum, string filterValue, string selectedValue)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.MemberList = new List<Members>();

            List<Members> memberList = getMemberListFromDB(userid, assocNum, filterValue, null);
            int countWithoutFiltering = memberList.Count;

            if (memberList.FirstOrDefault(ml => ml.Value == selectedValue) == null && !string.IsNullOrEmpty(selectedValue))
            {
                List<Members> members = getMemberListFromDB(userid, assocNum, filterValue, selectedValue);
                if (members.Count != 0)
                {
                    memberList = memberList.Take(MaxComboRecordCount - 1).ToList();
                    memberList.Add(members[0]);
                }
            }

            filterComboObj.MemberList = memberList;

            if (String.IsNullOrEmpty(selectedValue))
            {
                filterComboObj.FilterValue = filterValue;
            }
            else
            {
                // the selected value was provided by the app which is a member number and should find one record.  
                //  Get the user friendly member name description so the app can show this to the user as the provided filter instead
                if (filterComboObj.MemberList.Count == 1 && countWithoutFiltering != 1)
                {
                    filterComboObj.FilterValue = filterComboObj.MemberList[0].Description;
                }
            }

            filterComboObj.IsTruncated = (filterComboObj.MemberList.Count == MaxComboRecordCount);
            return filterComboObj;
        }

        [OperationContract]
        public FilterComboObj GetLocations(string userid, string assocNum, string memberNum, string filterValue, string selectedValue)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.LocationList = new List<Location>();

            List<Location> locationList = getLocationListFromDB(userid, assocNum, memberNum, filterValue, null);
            int countWithoutFiltering = locationList.Count;

            if (locationList.FirstOrDefault(ml => ml.Value == selectedValue) == null && !string.IsNullOrEmpty(selectedValue))
            {
                List<Location> locations = getLocationListFromDB(userid, assocNum, memberNum, filterValue, selectedValue);
                if (locations.Count != 0)
                {
                    locationList = locationList.Take(MaxComboRecordCount - 1).ToList();
                    locationList.Add(locations[0]);
                }
            }

            filterComboObj.LocationList = locationList;

            if (String.IsNullOrEmpty(selectedValue))
            {
                filterComboObj.FilterValue = filterValue;
            }
            else
            {
                // the selected value was provided by the app which is a member number and should find one record.  
                //  Get the user friendly member name description so the app can show this to the user as the provided filter instead
                if (filterComboObj.LocationList.Count == 1 && countWithoutFiltering != 1)
                {
                    filterComboObj.FilterValue = filterComboObj.LocationList[0].Description;
                }
            }

            filterComboObj.IsTruncated = (filterComboObj.LocationList.Count == MaxComboRecordCount);
            return filterComboObj;
        }

        [OperationContract]
        public FilterComboObj GetGroup1Codes(string userid, string assocNum, string memberNum, string location, string filterValue, string selectedValue)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.GroupCodeList = new List<GroupCode>();

            List<GroupCode> groupCodeList = getGroupCode1ListFromDB(memberNum, location, filterValue, null);
            int countWithoutFiltering = groupCodeList.Count;

            if (groupCodeList.FirstOrDefault(ml => ml.Value == selectedValue) == null && !string.IsNullOrEmpty(selectedValue))
            {
                List<GroupCode> groupCodes = getGroupCode1ListFromDB(memberNum, location, filterValue, selectedValue);
                if (groupCodes.Count != 0)
                {
                    groupCodeList = groupCodeList.Take(MaxComboRecordCount - 1).ToList();
                    groupCodeList.Add(groupCodes[0]);
                }
            }

            filterComboObj.GroupCodeList = groupCodeList;

            if (String.IsNullOrEmpty(selectedValue))
            {
                filterComboObj.FilterValue = filterValue;
            }
            else
            {
                // the selected value was provided by the app which is a member number and should find one record.  
                //  Get the user friendly member name description so the app can show this to the user as the provided filter instead
                if (filterComboObj.GroupCodeList.Count == 1 && countWithoutFiltering != 1)
                {
                    filterComboObj.FilterValue = filterComboObj.GroupCodeList[0].Description;
                }
            }

            filterComboObj.IsTruncated = (filterComboObj.GroupCodeList.Count == MaxComboRecordCount);
            return filterComboObj;
        }

        [OperationContract]
        public FilterComboObj GetGroup2Codes(string userid, string assocNum, string memberNum, string location, string group1Code, string filterValue, string selectedValue)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.GroupCodeList = new List<GroupCode>();

            List<GroupCode> groupCodeList = getGroupCode2ListFromDB(memberNum, location, group1Code, filterValue, null);
            int countWithoutFiltering = groupCodeList.Count;

            if (groupCodeList.FirstOrDefault(ml => ml.Value == selectedValue) == null && !string.IsNullOrEmpty(selectedValue))
            {
                List<GroupCode> groupCodes = getGroupCode2ListFromDB(memberNum, location, group1Code, filterValue, selectedValue);
                if (groupCodes.Count != 0)
                {
                    groupCodeList = groupCodeList.Take(MaxComboRecordCount - 1).ToList();
                    groupCodeList.Add(groupCodes[0]);
                }
            }

            filterComboObj.GroupCodeList = groupCodeList;

            if (String.IsNullOrEmpty(selectedValue))
            {
                filterComboObj.FilterValue = filterValue;
            }
            else
            {
                // the selected value was provided by the app which is a member number and should find one record.  
                //  Get the user friendly member name description so the app can show this to the user as the provided filter instead
                if (filterComboObj.GroupCodeList.Count == 1 && countWithoutFiltering != 1)
                {
                    filterComboObj.FilterValue = filterComboObj.GroupCodeList[0].Description;
                }
            }

            filterComboObj.IsTruncated = (filterComboObj.GroupCodeList.Count == MaxComboRecordCount);
            return filterComboObj;
        }

        [OperationContract]
        public FilterComboObj GetGroup3Codes(string userid, string assocNum, string memberNum, string location, string group1Code, string group2Code, string filterValue, string selectedValue)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.GroupCodeList = new List<GroupCode>();

            List<GroupCode> groupCodeList = getGroupCode3ListFromDB(memberNum, location, group1Code, group2Code, filterValue, null);
            int countWithoutFiltering = groupCodeList.Count;

            if (groupCodeList.FirstOrDefault(ml => ml.Value == selectedValue) == null && !string.IsNullOrEmpty(selectedValue))
            {
                List<GroupCode> groupCodes = getGroupCode3ListFromDB(memberNum, location, group1Code, group2Code, filterValue, selectedValue);
                if (groupCodes.Count != 0)
                {
                    groupCodeList = groupCodeList.Take(MaxComboRecordCount - 1).ToList();
                    groupCodeList.Add(groupCodes[0]);
                }
            }

            filterComboObj.GroupCodeList = groupCodeList;

            if (String.IsNullOrEmpty(selectedValue))
            {
                filterComboObj.FilterValue = filterValue;
            }
            else
            {
                // the selected value was provided by the app which is a member number and should find one record.  
                //  Get the user friendly member name description so the app can show this to the user as the provided filter instead
                if (filterComboObj.GroupCodeList.Count == 1 && countWithoutFiltering != 1)
                {
                    filterComboObj.FilterValue = filterComboObj.GroupCodeList[0].Description;
                }
            }

            filterComboObj.IsTruncated = (filterComboObj.GroupCodeList.Count == MaxComboRecordCount);
            return filterComboObj;
        }

        [OperationContract]
        public FilterComboObj GetGroup4Codes(string userid, string assocNum, string memberNum, string location, string group1Code, string group2Code, string group3Code, string filterValue, string selectedValue)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.GroupCodeList = new List<GroupCode>();

            List<GroupCode> groupCodeList = getGroupCode4ListFromDB(memberNum, location, group1Code, group2Code, group3Code, filterValue, null);
            int countWithoutFiltering = groupCodeList.Count;

            if (groupCodeList.FirstOrDefault(ml => ml.Value == selectedValue) == null && !string.IsNullOrEmpty(selectedValue))
            {
                List<GroupCode> groupCodes = getGroupCode4ListFromDB(memberNum, location, group1Code, group2Code, group3Code, filterValue, selectedValue);
                if (groupCodes.Count != 0)
                {
                    groupCodeList = groupCodeList.Take(MaxComboRecordCount - 1).ToList();
                    groupCodeList.Add(groupCodes[0]);
                }
            }

            filterComboObj.GroupCodeList = groupCodeList;

            if (String.IsNullOrEmpty(selectedValue))
            {
                filterComboObj.FilterValue = filterValue;
            }
            else
            {
                // the selected value was provided by the app which is a member number and should find one record.  
                //  Get the user friendly member name description so the app can show this to the user as the provided filter instead
                if (filterComboObj.GroupCodeList.Count == 1 && countWithoutFiltering != 1)
                {
                    filterComboObj.FilterValue = filterComboObj.GroupCodeList[0].Description;
                }
            }

            filterComboObj.IsTruncated = (filterComboObj.GroupCodeList.Count == MaxComboRecordCount);
            return filterComboObj;
        }

        [OperationContract]
        public FilterComboObj GetCoverages(string userid)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.CoverageList = new List<Coverage>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_COVERAGE_CODES_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_USER_ID, userid
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<Coverage> coverageList = new List<Coverage>();
                foreach (DataRow row in dataTable.Rows)
                {
                    coverageList.Add(new Coverage { Description = row["description"].ToString().ToUpper(), Value = row["value"].ToString() });
                }
                filterComboObj.CoverageList = coverageList;
            }

            filterComboObj.FilterValue = string.Empty;

            filterComboObj.IsTruncated = false;

            return filterComboObj;
        }

        [OperationContract]
        public FilterComboObj GetRRENumbers(string assocNum, string userid)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.RRENumberList = new List<RRENumber>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_GET_RRENUMBERS_LIST, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_USER_ID, userid,
                                                                Consts.SP_PARAM_ASSN_NUM, assocNum
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<RRENumber> rreNumberList = new List<RRENumber>();
                foreach (DataRow row in dataTable.Rows)
                {
                    rreNumberList.Add(new RRENumber { Description = row["description"].ToString().ToUpper(), Value = row["value"].ToString() });
                }
                filterComboObj.RRENumberList = rreNumberList;
            }

            filterComboObj.FilterValue = string.Empty;

            filterComboObj.IsTruncated = false;

            return filterComboObj;
        }

        [OperationContract]
        public FilterComboObj GetCoveragesByInjury(string userid, Boolean isEmployeeInjury)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.CoverageList = new List<Coverage>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_COVERAGE_CODES_BY_INJURY_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_USER_ID, userid,
                                                                Consts.SP_PARAM_IS_EMPLOYEE_INJURY,isEmployeeInjury
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<Coverage> coverageList = new List<Coverage>();
                foreach (DataRow row in dataTable.Rows)
                {
                    coverageList.Add(new Coverage { Description = row["description"].ToString().ToUpper(), Value = row["value"].ToString() });
                }
                filterComboObj.CoverageList = coverageList;
            }

            filterComboObj.FilterValue = string.Empty;

            filterComboObj.IsTruncated = false;

            return filterComboObj;
        }
        

        [OperationContract]
        public FilterComboObj GetPrimaryCarriers(string assocNum, string userid)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.PrimaryCarrierList = new List<PrimaryCarrier>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_PRIMARY_CARRIER_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_USER_ID, userid,
                                                                Consts.SP_PARAM_ASSN_NUM, assocNum
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<PrimaryCarrier> primaryCarrierList = new List<PrimaryCarrier>();
                foreach (DataRow row in dataTable.Rows)
                {
                    primaryCarrierList.Add(new PrimaryCarrier { Description = row["description"].ToString().ToUpper(), Value = row["value"].ToString() });
                }
                filterComboObj.PrimaryCarrierList = primaryCarrierList;
            }

            filterComboObj.FilterValue = string.Empty;

            filterComboObj.IsTruncated = false;

            return filterComboObj;
        }

        [OperationContract]
        public FilterComboObj GetIssuingCarriers(string primaryCarrierList, string userid, string assocNum)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.IssuingCarrierList = new List<IssuingCarrier>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_ISSUING_CARRIER_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_USER_ID, userid,
                                                                Consts.SP_PARAM_PRIMARY_CARRIER_LIST, primaryCarrierList,
                                                                Consts.SP_PARAM_ASSN_NUM, assocNum
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<IssuingCarrier> issuingCarrierList = new List<IssuingCarrier>();
                foreach (DataRow row in dataTable.Rows)
                {
                    issuingCarrierList.Add(new IssuingCarrier { Description = row["description"].ToString().ToUpper(), Value = row["value"].ToString() });
                }
                filterComboObj.IssuingCarrierList = issuingCarrierList;
            }

            filterComboObj.FilterValue = string.Empty;

            filterComboObj.IsTruncated = false;

            return filterComboObj;
        }

        [OperationContract]
        public FilterComboObj GetVenues(string assocNum, string userid)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.VenueList = new List<Venue>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_VENUE_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_USER_ID, userid,
                                                                Consts.SP_PARAM_ASSN_NUM, assocNum
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<Venue> venueList = new List<Venue>();
                foreach (DataRow row in dataTable.Rows)
                {
                    venueList.Add(new Venue { Description = row["description"].ToString().ToUpper(), Value = row["value"].ToString() });
                }
                filterComboObj.VenueList = venueList;
            }

            filterComboObj.FilterValue = string.Empty;

            filterComboObj.IsTruncated = false;

            return filterComboObj;
        }

        [OperationContract]
        public FilterComboObj GetDefenseFirms(string assocNum, string userid)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.DefenseFirmList = new List<DefenseFirm>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_DEFENSE_FIRM_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_USER_ID, userid,
                                                                Consts.SP_PARAM_ASSN_NUM, assocNum
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<DefenseFirm> defenseFirmList = new List<DefenseFirm>();
                foreach (DataRow row in dataTable.Rows)
                {
                    defenseFirmList.Add(new DefenseFirm { Description = row["description"].ToString().ToUpper(), Value = row["value"].ToString() });
                }
                filterComboObj.DefenseFirmList = defenseFirmList;
            }

            filterComboObj.FilterValue = string.Empty;

            filterComboObj.IsTruncated = false;

            return filterComboObj;
        }

        [OperationContract]
        public FilterComboObj GetDefenseAttorneys(string defenseFirmList, string userid, string assocNum)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.DefenseAttorneyList = new List<DefenseAttorney>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_DEFENSE_ATTORNEY_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_USER_ID, userid,
                                                                Consts.SP_PARAM_DEFENSE_FIRM_LIST, defenseFirmList,
                                                                Consts.SP_PARAM_ASSN_NUM, assocNum
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<DefenseAttorney> defenseAttorneyList = new List<DefenseAttorney>();
                foreach (DataRow row in dataTable.Rows)
                {
                    defenseAttorneyList.Add(new DefenseAttorney { Description = row["description"].ToString().ToUpper(), Value = row["value"].ToString() });
                }
                filterComboObj.DefenseAttorneyList = defenseAttorneyList;
            }

            filterComboObj.FilterValue = string.Empty;

            filterComboObj.IsTruncated = false;

            return filterComboObj;
        }

        [OperationContract]
        public FilterComboObj GetPlaintiffFirms(string userid, string assocNum)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.PlaintiffFirmList = new List<PlaintiffFirm>();

            var plaintiffFirmList = getPlaintiffFirmsListFromDB(userid, assocNum);

            filterComboObj.PlaintiffFirmList = plaintiffFirmList;
            filterComboObj.FilterValue = string.Empty;
            filterComboObj.IsTruncated = (filterComboObj.PlaintiffFirmList.Count >= MaxComboRecordCount);

            return filterComboObj;
        }

        [OperationContract]
        public FilterComboObj GetPlaintiffAttorneys(string userid, string assocNum)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.PlaintiffAttorneyList = new List<PlaintiffAttorney>();

            List<PlaintiffAttorney> plaintiffAttorneyList = getPlaintiffAttorneysListFromDB(userid, assocNum);
            
            filterComboObj.PlaintiffAttorneyList = plaintiffAttorneyList;
            filterComboObj.FilterValue = string.Empty;
            filterComboObj.IsTruncated = (filterComboObj.PlaintiffAttorneyList.Count >= MaxComboRecordCount);

            return filterComboObj;
        }

       
        [OperationContract]
        public FilterComboObj GetRegions(string userid, string assocNum)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.RegionList = new List<Region>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_REGION_BY_CLIENT_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_USER_ID, userid,
                                                                Consts.SP_PARAM_ASSN_NUM, assocNum
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<Region> regionList = new List<Region>();
                foreach (DataRow row in dataTable.Rows)
                {
                    regionList.Add(new Region { Description = row["description"].ToString().ToUpper(), Value = row["value"].ToString() });
                }
                filterComboObj.RegionList = regionList;
            }

            filterComboObj.FilterValue = string.Empty;

            filterComboObj.IsTruncated = false;

            return filterComboObj;
        }

        [OperationContract]
        public FilterComboObj GetOffices(string userid, string assocNum, string regionList)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.OfficeList = new List<Office>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_OFFICE_BY_CLIENT_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_USER_ID, userid,
                                                                Consts.SP_PARAM_REGION_LIST, regionList,
                                                                Consts.SP_PARAM_ASSN_NUM, assocNum
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<Office> officeList = new List<Office>();
                foreach (DataRow row in dataTable.Rows)
                {
                    officeList.Add(new Office { Description = row["description"].ToString().ToUpper(), Value = row["value"].ToString() });
                }
                filterComboObj.OfficeList = officeList;
            }

            filterComboObj.FilterValue = string.Empty;

            filterComboObj.IsTruncated = false;

            return filterComboObj;
        }

        [OperationContract]
        public FilterComboObj GetAdjusters(string assocNum)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.AdjusterList = new List<Adjuster>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_ADJUSTER_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_ASSN_NUM, assocNum                                                                
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<Adjuster> adjusterList = new List<Adjuster>();
                foreach (DataRow row in dataTable.Rows)
                {
                    adjusterList.Add(new Adjuster { Description = row["description"].ToString().ToUpper(), Value = row["value"].ToString() });
                }
                filterComboObj.AdjusterList = adjusterList;
            }

            filterComboObj.FilterValue = string.Empty;

            filterComboObj.IsTruncated = false;

            return filterComboObj;
        }

        [OperationContract]
        public String GetVenueNameListByVenueId(string venueIdList)
        {
            String list = String.Empty;
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_GET_VENUE_NAME_LIST, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_VENUE_ID_LIST, venueIdList
                                                            });

            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    list = row["venueList"].ToString().ToUpper();
                }
            }

            return list;
        }

        [OperationContract]
        public TrialDateRange GetTrialDatesByDateRange(DateTime startDate, DateTime endDate, int trialDateRangeValue)
        {
            TrialDateRange trialDateRange = new TrialDateRange();
            trialDateRange.TrialDateRangeValue = trialDateRangeValue;
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_GET_TRIAL_DATES_BY_PERIOD, CommandType.StoredProcedure,
                                                    new object[]
                                                        {
                                                            Consts.SP_PARAM_START_DATE, startDate,
                                                            Consts.SP_PARAM_END_DATE, endDate, 
                                                            Consts.SP_PARAM_TRIAL_DATE_PERIOD, trialDateRangeValue

                                                        });

            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count == 1)
            {
                trialDateRange.StartTrialDate = DateTime.Parse(dataTable.Rows[0]["StartDate"].ToString());
                trialDateRange.EndTrialDate = DateTime.Parse(dataTable.Rows[0]["EndDate"].ToString());
            }
            else
            {
                trialDateRange.StartTrialDate = startDate;
                trialDateRange.EndTrialDate = endDate;
            }
            return trialDateRange;
        }

        [OperationContract]
        public String GetDefenseFirmNameListByDefenseFirmId(string defenseFirmIdList)
        {
            String list = String.Empty;
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_GET_DEFENSE_FIRM_NAME_LIST, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_DEFENSE_FIRM_ID_LIST, defenseFirmIdList
                                                            });

            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    list = row["defenseFirmList"].ToString().ToUpper();
                }
            }

            return list;
        }

        [OperationContract]
        public String GetPlaintiffFirmNameListByPlaintiffFirmId(string plaintiffFirmIdList)
        {
            String list = String.Empty;
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_GET_PLAINTIFF_FIRM_NAME_LIST, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_PLAINTIFF_FIRM_ID_LIST, plaintiffFirmIdList
                                                            });

            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    list = row["plaintiffFirmList"].ToString().ToUpper();
                }
            }

            return list;
        }

        [OperationContract]
        public String GetPrimaryCarrierNameListByPrimaryId(string primaryCarrierIdList)
        {
            String list = String.Empty;
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_GET_PRIMARY_CARRIER_NAME_LIST, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_PRIMARY_CARRIER_ID_LIST, primaryCarrierIdList
                                                            });

            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    list = row["primaryCarrierList"].ToString().ToUpper();
                }
            }

            return list;
        }

        [OperationContract]
        public String GetIssuingCarrierNameListByIssuingId(string issuingCarrierIdList)
        {
            String list = String.Empty;
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_GET_ISSUING_CARRIER_NAME_LIST, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_ISSUING_CARRIER_ID_LIST, issuingCarrierIdList
                                                            });

            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    list = row["issuingCarrierList"].ToString().ToUpper();
                }
            }

            return list;
        }

        [OperationContract]
        public String GetAdjusterNameListByAdjusterId(string adjusterIdList)
        {
            String list = String.Empty;
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_GET_ADJUSTER_NAME_LIST, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_ADJUSTER_ID_LIST, adjusterIdList
                                                            });

            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    list = row["adjusterList"].ToString().ToUpper();
                }
            }

            return list;
        }

        //Test Begin
        [OperationContract]
        public List<ClassObj> GetReserveClasses(string assNum, string coverages)
        {
            return GetClasses(assNum, coverages);
        }
        //Test End

        [OperationContract]
        public List<AggregatePolicy> GetAggregatePolicies(string userid, string assocNum)
        {
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_AGGREGATE_POLICY_BY_CLIENT__SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_USER_ID, userid,
                                                                Consts.SP_PARAM_ASSN_NUM , assocNum 

                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<AggregatePolicy> policyList = new List<AggregatePolicy>();
                foreach (DataRow row in dataTable.Rows)
                {
                    policyList.Add(new AggregatePolicy { Description = row["description"].ToString().ToUpper(), Value = row["value"].ToString() });
                }
                return policyList;
            }
            return new List<AggregatePolicy>();
        }

        [OperationContract]
        public List<Policy> GetSirCashFlowProtectionEndPolicies(string userid, string assocNum)
        {
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_SIR_CASH_FLOW_PROTECTION_POLICY_BY_CLIENT__SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_USER_ID, userid,
                                                                Consts.SP_PARAM_ASSN_NUM , assocNum 

                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<Policy> policyList = new List<Policy>();
                foreach (DataRow row in dataTable.Rows)
                {
                    policyList.Add(new Policy { Description = row["description"].ToString().ToUpper(), Value = row["value"].ToString() });
                }
                return policyList;
            }
            return new List<Policy>();
        }

        [OperationContract]
        public List<Policy> GetPolicies(string userid, string assocNum)
        {
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_POLICY_BY_CLIENT__SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_USER_ID, userid,
                                                                Consts.SP_PARAM_ASSN_NUM , assocNum 

                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<Policy> policyList = new List<Policy>();
                foreach (DataRow row in dataTable.Rows)
                {
                    policyList.Add(new Policy { Description = row["description"].ToString().ToUpper(), Value = row["value"].ToString() });
                }
                return policyList;
            }
            return new List<Policy>();
        }

        [OperationContract]
        public List<Policy> GetPoliciesDeductible(string userid, string assocNum)
        {
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_POLICY_BY_DEDUCTIBLE_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_USER_ID, userid,
                                                                Consts.SP_PARAM_ASSN_NUM , assocNum 

                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<Policy> policyList = new List<Policy>();
                foreach (DataRow row in dataTable.Rows)
                {
                    policyList.Add(new Policy { Description = row["description"].ToString().ToUpper(), Value = row["value"].ToString() });
                }
                return policyList;
            }
            return new List<Policy>();
        }        
        

        [OperationContract]
        public String GetPolicyNumberListByPolicyId(string policyIdList)
        {
            String list = String.Empty;
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_GET_POLICY_NUMBER_LIST_BY_POLICY_ID_LIST, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_POLICY_ID_LIST, policyIdList
                                                            });

            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    list = row["policyNumberList"].ToString().ToUpper();
                }
            }

            return list;
        }

        [OperationContract]
        public String GetRRENumberListByRreStoreId(string rreNumberStoreIdList, string userid, string assocNum)
        {
            String list = String.Empty;
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_GET_RRE_NUMBER_LIST_BY_RRE_STORE_ID_LIST, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_RRE_NUMBER_STORE_ID_LIST, rreNumberStoreIdList,
                                                                Consts.SP_PARAM_USER_ID, userid,
                                                                Consts.SP_PARAM_ASSN_NUM , assocNum 
                                                            });

            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    list = row["rreNumberList"].ToString().ToUpper();
                }
            }

            return list;
        }
        [OperationContract]
        public List<ClassObj> GetClasses(string assNum, string coverages)
        {
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_CLASS_CODES_SELECT, CommandType.StoredProcedure, new object[]
                                                                                                                            {
                                                                                                                                Consts.SP_PARAM_ASSN_NUM , assNum,
                                                                                                                                Consts.SP_PARAM_COVERAGES, coverages
                                                                                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<ClassObj> classList = new List<ClassObj>();
                foreach (DataRow row in dataTable.Rows)
                {
                    classList.Add(new ClassObj { Value = row["class_code"].ToString().ToUpper() });
                }
                return classList;
            }
            return new List<ClassObj>();
        }

        [OperationContract]
        public List<SubClass> GetSubClasses(string userid, string assocNum, string classCode)
        {
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_SUBCLASS_CODES_SELECT, CommandType.StoredProcedure, new object[]
                                                            {  
                                                                Consts.SP_PARAM_CLASS_CODE, classCode
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<SubClass> subClassList = new List<SubClass>();
                foreach (DataRow row in dataTable.Rows)
                {
                    subClassList.Add(new SubClass { Description = row["description"].ToString(), Value = row["value"].ToString() });
                }
                return subClassList;
            }
            return new List<SubClass>();
        }

        [OperationContract]
        public List<SubClass> GetMedicalSubClasses(string userid, string assocNum)
        {
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_MEDICAL_SUBCLASS_CODES_SELECT, CommandType.StoredProcedure, new object[]
                                                            {  
                                                                
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<SubClass> subClassList = new List<SubClass>();
                foreach (DataRow row in dataTable.Rows)
                {
                    subClassList.Add(new SubClass { Description = row["description"].ToString(), Value = row["value"].ToString() });
                }
                return subClassList;
            }
            return new List<SubClass>();
        }

        [OperationContract]
        public List<Discounter> GetDiscounters(string userid, string assocNum)
        {
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_DISCOUNTERS_SELECT, CommandType.StoredProcedure, new object[] {  });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<Discounter> list = new List<Discounter>();
                foreach (DataRow row in dataTable.Rows)
                {
                    list.Add(new Discounter { Description = row["description"].ToString(), Value = row["value"].ToString() });
                }
                return list;
            }
            return new List<Discounter>();
        }

        
        [OperationContract]
        public HierarchyLabels GetHierarchyLabels(string userId, string assocNum)
        {
            HierarchyLabels hierarchyLabels = new HierarchyLabels();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_CLIENT_CONFIGURATION_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_ASSN_NUM, assocNum
                                                            });

            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count == 1)
            {
                hierarchyLabels.MemberLabel = string.IsNullOrEmpty(dataTable.Rows[0]["MemberLabel"].ToString().Trim()) ? "Members" : dataTable.Rows[0]["MemberLabel"].ToString().Trim();
                hierarchyLabels.MemberLabelVisible = !string.IsNullOrEmpty(dataTable.Rows[0]["MemberLabel"].ToString().Trim());
                hierarchyLabels.LocationLabel = string.IsNullOrEmpty(dataTable.Rows[0]["LocationLabel"].ToString().Trim()) ? "Locations" : dataTable.Rows[0]["LocationLabel"].ToString().Trim();
                hierarchyLabels.LocationLabelVisible = !string.IsNullOrEmpty(dataTable.Rows[0]["LocationLabel"].ToString().Trim());
                hierarchyLabels.Group1Label = string.IsNullOrEmpty(dataTable.Rows[0]["Group1Label"].ToString().Trim()) ? "Group1 Code" : dataTable.Rows[0]["Group1Label"].ToString().Trim();
                hierarchyLabels.Group1LabelVisible = !string.IsNullOrEmpty(dataTable.Rows[0]["Group1Label"].ToString().Trim());
                hierarchyLabels.Group2Label = string.IsNullOrEmpty(dataTable.Rows[0]["Group2Label"].ToString().Trim()) ? "Group2 Code" : dataTable.Rows[0]["Group2Label"].ToString().Trim();
                hierarchyLabels.Group2LabelVisible = !string.IsNullOrEmpty(dataTable.Rows[0]["Group2Label"].ToString().Trim());
                hierarchyLabels.Group3Label = string.IsNullOrEmpty(dataTable.Rows[0]["Group3Label"].ToString().Trim()) ? "Group3 Code" : dataTable.Rows[0]["Group3Label"].ToString().Trim();
                hierarchyLabels.Group3LabelVisible = !string.IsNullOrEmpty(dataTable.Rows[0]["Group3Label"].ToString().Trim());
                hierarchyLabels.Group4Label = string.IsNullOrEmpty(dataTable.Rows[0]["Group4Label"].ToString().Trim()) ? "Group4 Code" : dataTable.Rows[0]["Group4Label"].ToString().Trim();
                hierarchyLabels.Group4LabelVisible = !string.IsNullOrEmpty(dataTable.Rows[0]["Group4Label"].ToString().Trim());
                hierarchyLabels.SpecialAnalysis1Label = string.IsNullOrEmpty(dataTable.Rows[0]["SpecialAnalysis1Label"].ToString().Trim()) ? "Special Analysis1" : dataTable.Rows[0]["SpecialAnalysis1Label"].ToString().Trim();
                hierarchyLabels.SpecialAnalysis1LabelVisible = !string.IsNullOrEmpty(dataTable.Rows[0]["SpecialAnalysis1Label"].ToString().Trim());
                hierarchyLabels.SpecialAnalysis2Label = string.IsNullOrEmpty(dataTable.Rows[0]["SpecialAnalysis2Label"].ToString().Trim()) ? "Special Analysis2" : dataTable.Rows[0]["SpecialAnalysis2Label"].ToString().Trim();
                hierarchyLabels.SpecialAnalysis2LabelVisible = !string.IsNullOrEmpty(dataTable.Rows[0]["SpecialAnalysis2Label"].ToString().Trim());
                hierarchyLabels.SpecialAnalysis3Label = string.IsNullOrEmpty(dataTable.Rows[0]["SpecialAnalysis3Label"].ToString().Trim()) ? "Special Analysis3" : dataTable.Rows[0]["SpecialAnalysis3Label"].ToString().Trim();
                hierarchyLabels.SpecialAnalysis3LabelVisible = !string.IsNullOrEmpty(dataTable.Rows[0]["SpecialAnalysis3Label"].ToString().Trim());
                hierarchyLabels.SpecialAnalysis4Label = string.IsNullOrEmpty(dataTable.Rows[0]["SpecialAnalysis4Label"].ToString().Trim()) ? "Special Analysis4" : dataTable.Rows[0]["SpecialAnalysis4Label"].ToString().Trim();
                hierarchyLabels.SpecialAnalysis4LabelVisible = !string.IsNullOrEmpty(dataTable.Rows[0]["SpecialAnalysis4Label"].ToString().Trim());
                hierarchyLabels.SpecialAnalysis5Label = string.IsNullOrEmpty(dataTable.Rows[0]["SpecialAnalysis5Label"].ToString().Trim()) ? "Special Analysis5" : dataTable.Rows[0]["SpecialAnalysis5Label"].ToString().Trim();
                hierarchyLabels.SpecialAnalysis5LabelVisible = !string.IsNullOrEmpty(dataTable.Rows[0]["SpecialAnalysis5Label"].ToString().Trim());                
            }
            return hierarchyLabels;
        }

        [OperationContract]
        public FilterComboObj GetStateOfJurisdiction(string assNum)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.StateOfJurisdictionsList = new List<StateOfJurisdiction>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_STATE_TYPE_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_ASSN_NUM, assNum
                                                                
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<StateOfJurisdiction> stateList = new List<StateOfJurisdiction>();
                foreach (DataRow row in dataTable.Rows)
                {
                    stateList.Add(new StateOfJurisdiction { Description = row["description"].ToString(), Value = row["value"].ToString() });
                }
                filterComboObj.StateOfJurisdictionsList = stateList;
            }

            filterComboObj.FilterValue = string.Empty;

            filterComboObj.IsTruncated = false;

            return filterComboObj;
        }

        [OperationContract]
        public FilterComboObj GetClientAnalysis1(string userId, string assocNum, string filterValue)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.ClientAnalysesList = new List<ClientAnalysis>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataSet dataSet = dbHelper.GetDataSet(Consts.SP_NAME_CLIENT_ANALYSIS_1_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_ASSN_NUM, assocNum,
                                                                Consts.SP_PARAM_NUM_RECORDS, MaxComboRecordCount,
                                                                Consts.SP_PARAM_FILTER_VALUE, filterValue
                                                            });

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count == 2)
            {
                if (dataSet.Tables[0] != null && dataSet.Tables[0].Rows != null && dataSet.Tables[0].Rows.Count != 0)
                {
                    List<ClientAnalysis> caList = new List<ClientAnalysis>();
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        caList.Add(new ClientAnalysis { Description = row["description"].ToString(), Value = row["value"].ToString() });
                    }
                    filterComboObj.ClientAnalysesList = caList;
                }
                
                if (dataSet.Tables[1] != null && dataSet.Tables[1].Rows != null && dataSet.Tables[1].Rows.Count != 0)
                {
                    string countString = dataSet.Tables[1].Rows[0][0].ToString();
                    if (!string.IsNullOrEmpty(countString))
                    {
                        filterComboObj.CountWithoutTruncate = Convert.ToInt32(countString);
                    }
                }
            }

            filterComboObj.FilterValue = filterValue;

            filterComboObj.IsTruncated = (filterComboObj.ClientAnalysesList.Count == MaxComboRecordCount);

            return filterComboObj;
        }

        [OperationContract]
        public FilterComboObj GetClientAnalysis2(string userId, string assocNum, string filterValue)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.ClientAnalysesList = new List<ClientAnalysis>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataSet dataSet = dbHelper.GetDataSet(Consts.SP_NAME_CLIENT_ANALYSIS_2_SELECT, CommandType.StoredProcedure,
                                                         new object[]
                                                            {
                                                                Consts.SP_PARAM_ASSN_NUM, assocNum,
                                                                Consts.SP_PARAM_NUM_RECORDS, MaxComboRecordCount,
                                                                Consts.SP_PARAM_FILTER_VALUE, filterValue
                                                            });

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count == 2)
            {
                if (dataSet.Tables[0] != null && dataSet.Tables[0].Rows != null && dataSet.Tables[0].Rows.Count != 0)
                {
                    List<ClientAnalysis> caList = new List<ClientAnalysis>();
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        caList.Add(new ClientAnalysis { Description = row["description"].ToString(), Value = row["value"].ToString() });
                    }
                    filterComboObj.ClientAnalysesList = caList;
                }

                if (dataSet.Tables[1] != null && dataSet.Tables[1].Rows != null && dataSet.Tables[1].Rows.Count != 0)
                {
                    string countString = dataSet.Tables[1].Rows[0][0].ToString();
                    if (!string.IsNullOrEmpty(countString))
                    {
                        filterComboObj.CountWithoutTruncate = Convert.ToInt32(countString);
                    }
                }
            }

            filterComboObj.FilterValue = filterValue;

            filterComboObj.IsTruncated = (filterComboObj.ClientAnalysesList.Count == MaxComboRecordCount);

            return filterComboObj;
        }

        [OperationContract]
        public FilterComboObj GetClientAnalysis3(string userId, string assocNum, string filterValue)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.ClientAnalysesList = new List<ClientAnalysis>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataSet dataSet = dbHelper.GetDataSet(Consts.SP_NAME_CLIENT_ANALYSIS_3_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_ASSN_NUM, assocNum,
                                                                Consts.SP_PARAM_NUM_RECORDS, MaxComboRecordCount,
                                                                Consts.SP_PARAM_FILTER_VALUE, filterValue
                                                            });

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count == 2)
            {
                if (dataSet.Tables[0] != null && dataSet.Tables[0].Rows != null && dataSet.Tables[0].Rows.Count != 0)
                {
                    List<ClientAnalysis> caList = new List<ClientAnalysis>();
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        caList.Add(new ClientAnalysis { Description = row["description"].ToString(), Value = row["value"].ToString() });
                    }
                    filterComboObj.ClientAnalysesList = caList;
                }

                if (dataSet.Tables[1] != null && dataSet.Tables[1].Rows != null && dataSet.Tables[1].Rows.Count != 0)
                {
                    string countString = dataSet.Tables[1].Rows[0][0].ToString();
                    if (!string.IsNullOrEmpty(countString))
                    {
                        filterComboObj.CountWithoutTruncate = Convert.ToInt32(countString);
                    }
                }
            }

            filterComboObj.FilterValue = filterValue;

            filterComboObj.IsTruncated = (filterComboObj.ClientAnalysesList.Count == MaxComboRecordCount);

            return filterComboObj;
        }

        [OperationContract]
        public FilterComboObj GetClientAnalysis4(string userId, string assocNum, string filterValue)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.ClientAnalysesList = new List<ClientAnalysis>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataSet dataSet = dbHelper.GetDataSet(Consts.SP_NAME_CLIENT_ANALYSIS_4_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_ASSN_NUM, assocNum,
                                                                Consts.SP_PARAM_NUM_RECORDS, MaxComboRecordCount,
                                                                Consts.SP_PARAM_FILTER_VALUE, filterValue
                                                            });

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count == 2)
            {
                if (dataSet.Tables[0] != null && dataSet.Tables[0].Rows != null && dataSet.Tables[0].Rows.Count != 0)
                {
                    List<ClientAnalysis> caList = new List<ClientAnalysis>();
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        caList.Add(new ClientAnalysis { Description = row["description"].ToString(), Value = row["value"].ToString() });
                    }
                    filterComboObj.ClientAnalysesList = caList;
                }

                if (dataSet.Tables[1] != null && dataSet.Tables[1].Rows != null && dataSet.Tables[1].Rows.Count != 0)
                {
                    string countString = dataSet.Tables[1].Rows[0][0].ToString();
                    if (!string.IsNullOrEmpty(countString))
                    {
                        filterComboObj.CountWithoutTruncate = Convert.ToInt32(countString);
                    }
                }
            }

            filterComboObj.FilterValue = filterValue;

            filterComboObj.IsTruncated = (filterComboObj.ClientAnalysesList.Count == MaxComboRecordCount);

            return filterComboObj;
        }

        [OperationContract]
        public FilterComboObj GetClientAnalysis5(string userId, string assocNum, string filterValue)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.ClientAnalysesList = new List<ClientAnalysis>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataSet dataSet = dbHelper.GetDataSet(Consts.SP_NAME_CLIENT_ANALYSIS_5_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_ASSN_NUM, assocNum,
                                                                Consts.SP_PARAM_NUM_RECORDS, MaxComboRecordCount,
                                                                Consts.SP_PARAM_FILTER_VALUE, filterValue
                                                            });

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count == 2)
            {
                if (dataSet.Tables[0] != null && dataSet.Tables[0].Rows != null && dataSet.Tables[0].Rows.Count != 0)
                {
                    List<ClientAnalysis> caList = new List<ClientAnalysis>();
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        caList.Add(new ClientAnalysis { Description = row["description"].ToString(), Value = row["value"].ToString() });
                    }
                    filterComboObj.ClientAnalysesList = caList;
                }

                if (dataSet.Tables[1] != null && dataSet.Tables[1].Rows != null && dataSet.Tables[1].Rows.Count != 0)
                {
                    string countString = dataSet.Tables[1].Rows[0][0].ToString();
                    if (!string.IsNullOrEmpty(countString))
                    {
                        filterComboObj.CountWithoutTruncate = Convert.ToInt32(countString);
                    }
                }
            }

            filterComboObj.FilterValue = filterValue;

            filterComboObj.IsTruncated = (filterComboObj.ClientAnalysesList.Count == MaxComboRecordCount);

            return filterComboObj;
        }

        [OperationContract]
        public List<string> GetFundYears(string assocNum, DateTime startDate)
        {
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_FUND_YEARS_SELECT, CommandType.StoredProcedure,
                                    new object[] { Consts.SP_PARAM_ASSN_NUM, assocNum,
                                        Consts.SP_PARAM_START_DATE, startDate
                                    });

            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<string> fundYearList = new List<string>();
                foreach (DataRow row in dataTable.Rows)
                {
                    string coverageStartDate = DateTime.Parse(row["covg_start"].ToString()).ToString("MM/dd/yyyy");
                    string coverageEndDate = DateTime.Parse(row["covg_end"].ToString()).ToString("MM/dd/yyyy");
                    fundYearList.Add(coverageStartDate + " - " + coverageEndDate);
                }
                return fundYearList;
            }
            return new List<string>();
        }

        [OperationContract]
        public List<ClientSpecific> GetClientSpecificFields(string assocNum)
        {
            
            List<ClientSpecific>  clientSpecificList = new List<ClientSpecific>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_CLIENT_SPECIFIC_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_ASSN_NUM, assocNum
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                //List<ClientSpecific> clientSpecificList = new List<ClientSpecific>();
                foreach (DataRow row in dataTable.Rows)
                {
                    clientSpecificList.Add(new ClientSpecific { Description = row["description"].ToString().ToUpper(), FieldName = row["fieldName"].ToString() });
                }
                //filterComboObj.ClientSpecificList = clientSpecificList;
            }
            return clientSpecificList;
            //filterComboObj.FilterValue = string.Empty;

            //filterComboObj.IsTruncated = false;

            //return clientSpecificList; // filterComboObj;
        }
        

        [OperationContract]
        public DateRange GetDatesByDateRange(DateTime startDate, DateTime endDate, int dateRangeValue, int lastXdays)
        {
            DateRange dateRange = new DateRange();
            dateRange.DateRangeValue = dateRangeValue;
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_GET_DATES_BY_PERIOD, CommandType.StoredProcedure,
                                                    new object[]
                                                        {
                                                            Consts.SP_PARAM_START_DATE, startDate,
                                                            Consts.SP_PARAM_END_DATE, endDate, 
                                                            Consts.SP_PARAM_DATE_PERIOD, dateRangeValue,
                                                            Consts.SP_PARAM_LAST_X_DAYS, lastXdays

                                                        });

            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count == 1)
            {
                dateRange.StartDate = DateTime.Parse(dataTable.Rows[0]["StartDate"].ToString());
                dateRange.EndDate = DateTime.Parse(dataTable.Rows[0]["EndDate"].ToString());
            }
            else
            {
                dateRange.StartDate = startDate;
                dateRange.EndDate = endDate;
            }
            return dateRange;
        }

        [OperationContract]
        public AsOfDateType GetAsOfDateByDateType(DateTime asOfDate, int asOfDateTypeValue)
        {
            AsOfDateType asOfDateType = new AsOfDateType();
            asOfDateType.AsOfDateTypeValue = asOfDateTypeValue;
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_GET_AS_OF_DATE_BY_RANGE, CommandType.StoredProcedure,
                                                    new object[]
                                                        {
                                                            Consts.SP_PARAM_AS_OF_DATE, asOfDate,
                                                            Consts.SP_PARAM_AS_OF_DATE_TYPE, asOfDateTypeValue
                                                        });

            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count == 1)
            {
                asOfDateType.AsOfDate = DateTime.Parse(dataTable.Rows[0]["AsOfDate"].ToString());
            }
            else
            {
                asOfDateType.AsOfDate = asOfDate;
            }
            return asOfDateType;
        }

        [OperationContract]
        public bool GetUserHasSSNAccess(string userName)
        {
            int result;
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["AuthenticationConnection"].ConnectionString);
            result = dbHelper.GetScalar<int>(Consts.SP_NAME_GET_USER_SSN_ACCESS_CHECKPOINT, CommandType.StoredProcedure,
                                                  new object[]
                                                        {
                                                            Consts.SP_PARAM_USER_ID, userName
                                                        });
            return (result == 1);
        }

        [OperationContract]
        public CcmsiUserSearchResult SearchCcmsiUsers(string userId, string assocNum, string filterValue, int currentPage)
        {
            CcmsiUserSearchResult searchResult = new CcmsiUserSearchResult();
            searchResult.ItemsPerPage = EmailSearchItemsPerPageCount;
            searchResult.UserList = new List<CcmsiUser>();

            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataSet dataSet = dbHelper.GetDataSet(Consts.SP_NAME_SEARCH_CCMSI_USER, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_ASSN_NUM, assocNum,
                                                                Consts.SP_PARAM_USER_ID, userId,
                                                                Consts.SP_PARAM_FILTER_VALUE, filterValue,
                                                                Consts.SP_PARAM_CURRENT_PAGE, currentPage
                                                            });

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count == 2)
            {
                if (dataSet.Tables[0] != null && dataSet.Tables[0].Rows != null && dataSet.Tables[0].Rows.Count != 0)
                {
                    List<CcmsiUser> userList = new List<CcmsiUser>();
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        userList.Add(
                            new CcmsiUser { 
                                EmailAddress = row["EmailAddress"].ToString(), 
                                FirstName = row["FirstName"].ToString().CapitalizeEachWord(),
                                FullName = row["FullName"].ToString().CapitalizeEachWord(),
                                ID = Convert.ToInt32(row["ID"]),
                                LastName = row["LastName"].ToString().CapitalizeEachWord(),
                                UserID = row["UserID"].ToString()
                        });
                    }
                    searchResult.UserList = userList;
                }

                if (dataSet.Tables[1] != null && dataSet.Tables[1].Rows != null && dataSet.Tables[1].Rows.Count != 0)
                {
                    string countString = dataSet.Tables[1].Rows[0][0].ToString();
                    if (!string.IsNullOrEmpty(countString))
                    {
                        searchResult.CountWithoutTruncate = Convert.ToInt32(countString);
                    }
                }
            }

            return searchResult;
        }

        [OperationContract]
        public FilterComboObj GetCastPolicyCoverages(string userid)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.CoverageList = new List<Coverage>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_CAST_POLICY_COVERAGE_CODES_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_USER_ID, userid
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<Coverage> coverageList = new List<Coverage>();
                foreach (DataRow row in dataTable.Rows)
                {
                    coverageList.Add(new Coverage { Description = row["description"].ToString().ToUpper(), Value = row["value"].ToString() });
                }
                filterComboObj.CoverageList = coverageList;
            }

            filterComboObj.FilterValue = string.Empty;

            filterComboObj.IsTruncated = false;

            return filterComboObj;
        }

      
        public CcmsiUser GetCcmsiUser(int ID)
        {
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataSet dataSet = dbHelper.GetDataSet(Consts.SP_NAME_GET_CCMSI_USER, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_ID, ID
                                                            });

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count == 1)
            {
                if (dataSet.Tables[0] != null && dataSet.Tables[0].Rows != null && dataSet.Tables[0].Rows.Count != 0)
                {
                    DataRow row = dataSet.Tables[0].Rows[0];
                    return new CcmsiUser
                    {
                        EmailAddress = row["EmailAddress"].ToString(),
                        FirstName = row["FirstName"].ToString().CapitalizeEachWord(),
                        FullName = row["FullName"].ToString().CapitalizeEachWord(),
                        ID = Convert.ToInt32(row["ID"]),
                        LastName = row["LastName"].ToString().CapitalizeEachWord(),
                        UserID = row["UserID"].ToString()
                    };
                }
            }

            return null;
        }

        public CcmsiUser GetCcmsiUser(string ID)
        {
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataSet dataSet = dbHelper.GetDataSet(Consts.SP_NAME_GET_CCMSI_BY_USER, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_ID, ID
                                                            });

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count == 1)
            {
                if (dataSet.Tables[0] != null && dataSet.Tables[0].Rows != null && dataSet.Tables[0].Rows.Count != 0)
                {
                    DataRow row = dataSet.Tables[0].Rows[0];
                    return new CcmsiUser
                    {
                        EmailAddress = row["EmailAddress"].ToString(),
                        FirstName = row["FirstName"].ToString().CapitalizeEachWord(),
                        FullName = row["FullName"].ToString().CapitalizeEachWord(),
                        ID = Convert.ToInt32(row["ID"]),
                        LastName = row["LastName"].ToString().CapitalizeEachWord(),
                        UserID = row["UserID"].ToString()
                    };
                }
            }

            return null;
        }

        public List<CcmsiUser> GetCcmsiUsers(List<int> userIDList)
        {
            // build comma delimited string
            string userIDs = string.Empty;
            int count = 0;
            foreach (int userID in userIDList)
            {
                if (count > 0) userIDs += ",";
                userIDs += userID.ToString();
                count++;
            }

            List<CcmsiUser> userList = new List<CcmsiUser>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataSet dataSet = dbHelper.GetDataSet(Consts.SP_NAME_GET_CCMSI_USERS, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_USER_IDS, userIDs
                                                            });

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count == 1)
            {
                if (dataSet.Tables[0] != null && dataSet.Tables[0].Rows != null && dataSet.Tables[0].Rows.Count != 0)
                {
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        userList.Add(new CcmsiUser { 
                                EmailAddress = row["EmailAddress"].ToString(), 
                                FirstName = row["FirstName"].ToString().CapitalizeEachWord(),
                                FullName = row["FullName"].ToString().CapitalizeEachWord(),
                                ID = Convert.ToInt32(row["ID"]),
                                LastName = row["LastName"].ToString().CapitalizeEachWord(),
                                UserID = row["UserID"].ToString()
                        });

                    }
                }
            }
            return userList;
        }

        [OperationContract]
        public FilterComboObj GetAdjuster(string assNum)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.AdjusterList = new List<Adjuster>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_ADJUSTER_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_ASSN_NUM, assNum
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<Adjuster> adjusterList = new List<Adjuster>();
                foreach (DataRow row in dataTable.Rows)
                {
                    adjusterList.Add(new Adjuster { Description = row["description"].ToString().ToUpper(), Value = row["value"].ToString() });
                }
                filterComboObj.AdjusterList = adjusterList;
            }

            filterComboObj.FilterValue = string.Empty;

            filterComboObj.IsTruncated = false;

            return filterComboObj;
        }

        [OperationContract]
        public FilterComboObj GetHandlingOffice(string assNum)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.HandlingOfficeList = new List<HandlingOffice>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_HANDLING_OFFICE_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_ASSN_NUM, assNum
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<HandlingOffice> handlingOfficeList = new List<HandlingOffice>();
                foreach (DataRow row in dataTable.Rows)
                {
                    handlingOfficeList.Add(new HandlingOffice { Description = row["description"].ToString().ToUpper(), Value = row["value"].ToString() });
                }
                filterComboObj.HandlingOfficeList = handlingOfficeList;
            }

            filterComboObj.FilterValue = string.Empty;

            filterComboObj.IsTruncated = false;

            return filterComboObj;
        }

        public string GetHierarchyDescriptionsString(string assNum, string userid, string hierarchy)
        {
            string resultString = "";
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_HIERARCHY_DESCRIPTION_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_ASSN_NUM, assNum,
                                                                Consts.SP_PARAM_USER_ID, userid,
                                                                Consts.SP_PARAM_HIERARCHY, hierarchy
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count == 1)
            {
                resultString = dataTable.Rows[0][0].ToString();
            }
            return resultString;
        }

        [OperationContract]
        public FilterComboObj GetBankAccounts(string assNum)
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.BankAccountList = new List<BankAccount>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_BANK_ACCOUNT_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_ASSN_NUM, assNum
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<BankAccount> bankAccountList = new List<BankAccount>();
                foreach (DataRow row in dataTable.Rows)
                {
                    bankAccountList.Add(new BankAccount { Description = row["description"].ToString().ToUpper(), Value = row["value"].ToString() });
                }
                filterComboObj.BankAccountList = bankAccountList;
            }

            filterComboObj.FilterValue = string.Empty;

            filterComboObj.IsTruncated = false;

            return filterComboObj;

        }

        [OperationContract]
        public String GetBankAccountNameListByAccountNumber(string assNum, string bankAccountNumberList)
        {
            String list = String.Empty;
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_GET_BANK_ACCOUNT_NAME_LIST, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_ASSN_NUM, assNum,
                                                                Consts.SP_PARAM_BANK_ACCOUNT_NUMBER_LIST, bankAccountNumberList
                                                            });

            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    list = row["bankAccountList"].ToString().ToUpper();
                }
            }

            return list;
        }


        [OperationContract]
        public FilterComboObj GetPayeeFeinTax(string assnNum, string filterValue, string selectedValue)
        {
            var filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.PayeeFeinTaxList = new List<PayeeFeinTax>();
            var dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            var dataSet = dbHelper.GetDataSet(Consts.SP_NAME_PAYEE_FEIN_TAX_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_ASSN_NUM, assnNum,
                                                                Consts.SP_PARAM_NUM_RECORDS, MaxComboRecordCount,
                                                                Consts.SP_PARAM_FILTER_VALUE, filterValue,
                                                                Consts.SP_PARAM_PAYEE_FEIN_SELECTED, selectedValue
                                                            }
                );

            if (dataSet != null && dataSet.Tables.Count == 2)
            {
                if (dataSet.Tables[0] != null && dataSet.Tables[0].Rows != null && dataSet.Tables[0].Rows.Count != 0)
                {

                    var payeeFeinTaxList = new List<PayeeFeinTax>();
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        payeeFeinTaxList.Add(new PayeeFeinTax { Description = row["description"].ToString(), Value = row["value"].ToString() });
                    }
                    filterComboObj.PayeeFeinTaxList = payeeFeinTaxList;
                }


                if (dataSet.Tables[1] != null && dataSet.Tables[1].Rows != null && dataSet.Tables[1].Rows.Count != 0)
                {
                    var countString = dataSet.Tables[1].Rows[0][0].ToString();
                    if (!string.IsNullOrEmpty(countString))
                    {
                        filterComboObj.CountWithoutTruncate = Convert.ToInt32(countString);
                    }
                }
            }

            filterComboObj.FilterValue = filterValue;
            filterComboObj.IsTruncated = filterComboObj.PayeeFeinTaxList.Count == MaxComboRecordCount;

            return filterComboObj;
        }

        [OperationContract]
        public string GetPayeeFeinTaxNumberList(string assnNum, string payeeFeinNumberList)
        {
            var list = string.Empty;
            var dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            var dataTable = dbHelper.GetDataTable(Consts.SP_NAME_GET_PAYEE_FEIN_TAX_NUMBER_LIST, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_ASSN_NUM, assnNum,
                                                                Consts.SP_PARAM_PAYEE_FEIN_NUMBER_LIST, payeeFeinNumberList
                                                            });

            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    list = row["PayeeFEINNumberList"].ToString().ToUpper();
                }
            }

            return list;
        }

        [OperationContract]
        public List<NoteType> GetNoteTypes()
        {
            var dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            var dataTable = dbHelper.GetDataTable(Consts.SP_NAME_NOTE_TYPES_SELECT, CommandType.StoredProcedure, new object[] { });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                var list = new List<NoteType>();
                foreach (DataRow row in dataTable.Rows)
                {
                    list.Add(new NoteType { Description = row["description"].ToString(), Value = row["value"].ToString() });
                }
                return list;
            }
            return new List<NoteType>();
        }

        [OperationContract]
        public List<CcmsiUser> GetUsersEnteredNotes(string assnNum)
        {
            var dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            var dataTable = dbHelper.GetDataTable(Consts.SP_NAME_NOTE_INPUT_BY_SELECT, CommandType.StoredProcedure,
                                                    new object[]
                                                        {
                                                            Consts.SP_PARAM_ASSN_NUM, assnNum
                                                        });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                var list = new List<CcmsiUser>();
                foreach (DataRow row in dataTable.Rows)
                {
                    list.Add(new CcmsiUser { UserID = row["userid"].ToString(), FullName = row["name"].ToString() });
                }
                return list;
            }
            return new List<CcmsiUser>();
        }

        [OperationContract]
        public FilterComboObj GetPeriodList(string userid, string assocNum, int periodType) 
        {
            FilterComboObj filterComboObj = new FilterComboObj();
            filterComboObj.MaxRecordCount = MaxComboRecordCount;
            filterComboObj.MultiPeriodList = new List<MultiPeriod>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_PERIOD_LIST_SELECT, CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_PERIOD_TYPE,periodType,
                                                                Consts.SP_PARAM_ASSN_NUM, assocNum                                                                
                                                            });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                List<MultiPeriod> periodList = new List<MultiPeriod>();
                foreach (DataRow row in dataTable.Rows)
                {
                    periodList.Add(new MultiPeriod { Description = row["description"].ToString().ToUpper(), Value = row["value"].ToString() });
                }
                filterComboObj.MultiPeriodList = periodList;
            }

            filterComboObj.FilterValue = string.Empty;

            filterComboObj.IsTruncated = false;

            return filterComboObj;
        }

        [OperationContract]
        public string VerifyTurnOffPageBreak(string verifyRowCountSP, List<object> myParams)
        {
            int maxRecordCount = MaxReportRecordCount();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);

            int recordCount = dbHelper.GetScalar<int>(verifyRowCountSP, CommandType.StoredProcedure, myParams.ToArray());
            if (recordCount > maxRecordCount)
            {
                return String.Format("The report cannot be run because it returns too much data.  It would return {0} records, while the maximum allowed record count is {1}.  Please narrow your filter criteria and try again.",
                       String.Format("{0:n0}", recordCount), String.Format("{0:n0}", maxRecordCount)); ;
            }
            return null;
        }

    }
}
