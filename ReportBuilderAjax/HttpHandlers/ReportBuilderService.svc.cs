using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;

namespace ReportBuilderAjax.Web.HttpHandlers
{
	[ServiceContract(Namespace = "")]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class ReportBuilderService
	{

        private void saveUserReportParameters(int userReportID, int userId, List<RptParameter> parameters)
        {
            ReportProjectDBEntities entities = new ReportProjectDBEntities();

            UserReport ur = entities.UserReport.Where(urp => urp.UserReportID == userReportID).First();

            foreach (var parm in parameters)
            {
                UserReportParameter urp = new UserReportParameter();

                urp.UserReport = ur;
                urp.ReportParameter = entities.ReportParameter.Where(r => r.ReportParameterID == parm.ID).FirstOrDefault();
                urp.ParameterValue = parm.DefaultValue;
                urp.CreatedByUserID = userId;
                urp.CreatedDate = DateTime.Now;
                urp.FilterString = parm.FilterString;
                entities.AddToUserReportParameter(urp);
            }

            entities.SaveChanges();
        }

        private int saveUserReport(int reportID, int reportLayoutStyleID, string userReportName, bool isSummaryOnly, int userId, bool isCustom, string associationNumber, int formatTypeID, bool isTransient, bool isTurnOffPageBreak, bool includeTitlePage)
        {
            ReportProjectDBEntities entities = new ReportProjectDBEntities();
            ReportLayoutStyle reportLayoutStyle = entities.ReportLayoutStyle.FirstOrDefault(rl => rl.ReportLayoutStyleID == reportLayoutStyleID);

            UserReport ur = new UserReport();
            ur.ReportLayoutStyleID = reportLayoutStyle.ReportLayoutStyleID;
            ur.FormatTypeID = formatTypeID;
            ur.ReportID = reportID;
            ur.UserID = userId;
            ur.CreatedByUserID = userId;
            ur.CreatedDate = DateTime.Now;
            ur.ModifiedDate = DateTime.Now;
            ur.UserReportName = userReportName;
            ur.ModifiedByUserID = userId;
            ur.IsCustom = isCustom;
            ur.ClientNumber = associationNumber;
            ur.IsSummaryOnly = isSummaryOnly;
            ur.IsTransient = isTransient;
            ur.IsTurnOffPageBreak = isTurnOffPageBreak;
            ur.IncludeTitlePage = includeTitlePage;

            entities.AddToUserReport(ur);               
            entities.SaveChanges();              

            return ur.UserReportID;
        }

		private int saveUserReportFields(int userReportID, List<RptField> fields, int userId, List<RptField> summarizeFields)
        {
            ReportProjectDBEntities entities = new ReportProjectDBEntities();

            UserReport ur = entities.UserReport.Where(urp => urp.UserReportID == userReportID).First();

            foreach (var field in fields)
            {
                string sortdir = "A";
                if (field.SortDirection == SortDirectionEnum.Descending)
                {
                    sortdir = "D";
                }

                UserReportField urf = new UserReportField();
                urf.UserReport = ur;
                urf.ReportField = entities.ReportField.Where(r => r.ReportFieldID == field.ReportFieldID).First();
                urf.CreatedByUserID = userId;
                urf.CreatedDate = DateTime.Now;
                urf.ColumnOrder = field.ColumnOrder;
                urf.CustomName = field.Name;
                urf.SortDirection = sortdir;
                urf.SortOrder = field.SortOrder;
                urf.GroupOrder = field.GroupOrder;
                urf.IncludePageBreak = field.IncludePageBreak;
                urf.CoverageCode = field.CoverageCode;  
                entities.AddToUserReportField(urf);
            }

            entities.SaveChanges();

            saveUserReportSummarizeFields(summarizeFields, entities, ur, userId);

            return ur.UserReportID;
        }

		private int updateUserReportFields(int userReportID, List<RptField> fields, int userId, List<RptField> summarizeFields)
        {
            ReportProjectDBEntities entities = new ReportProjectDBEntities();

            UserReport ur = entities.UserReport.Where(urp => urp.UserReportID == userReportID).First();

            var userReportFielList = from fv in entities.UserReportField
                              .Include("UserReport")
                              where fv.UserReport.UserReportID == userReportID
                              select fv;


            foreach (var field in userReportFielList)
            {
                if (!fields.Any(f => f.RptFieldID == field.UserReportFieldID))
                {
                    entities.UserReportField.DeleteObject(field);
                }
                else
                {
                    RptField myfield = fields.First(f => f.RptFieldID == field.UserReportFieldID);

                    string sortdir = "A";
                    if (myfield.SortDirection == SortDirectionEnum.Descending)
                    {
                        sortdir = "D";
                    }

                    UserReportField urf = new UserReportField();

                    field.ColumnOrder = myfield.ColumnOrder;
                    field.SortOrder = myfield.SortOrder;
                    field.SortDirection = sortdir;
                    field.GroupOrder = myfield.GroupOrder;
                    field.CustomName = myfield.Name;
                    field.IncludePageBreak = myfield.IncludePageBreak;
                    field.CoverageCode = myfield.CoverageCode; 
                }
            }

            //Add New UserReportFields
			foreach (RptField field in fields)
            {
                if (field.RptFieldID == field.ReportFieldID)
                {
                    if (field.IsUsed || field.SortOrder > 0)
                    {
                        string sortdir = "A";
                        if (field.SortDirection == SortDirectionEnum.Descending)
                        {
                            sortdir = "D";
                        }

                        UserReportField urf = new UserReportField();

                        urf.UserReport = ur;
                        urf.ReportField = entities.ReportField.Where(r => r.ReportFieldID == field.RptFieldID).First();
                        urf.CreatedByUserID = userId;
                        urf.CreatedDate = DateTime.Now;
                        urf.ColumnOrder = field.ColumnOrder;
                        urf.CustomName = field.Name;
                        urf.SortDirection = sortdir;
                        urf.SortOrder = field.SortOrder;
                        urf.GroupOrder = field.GroupOrder;
                        urf.IncludePageBreak = field.IncludePageBreak;
                        urf.CoverageCode = field.CoverageCode; 
                        entities.AddToUserReportField(urf);
                    }
                }
            }                 

            entities.SaveChanges();

            saveUserReportSummarizeFields(summarizeFields, entities, ur, userId);

            return ur.UserReportID;
        }

		private void saveUserReportSummarizeFields(List<RptField> summarizeFields, ReportProjectDBEntities entities, UserReport userRpt, int userId)
        {                                      
                if (summarizeFields != null)
                {
					foreach (RptField sumfield in summarizeFields)
                    {
                        UserReportField urfs = entities.UserReportField.First(urf => urf.UserReport.UserReportID == userRpt.UserReportID && urf.ReportField.ReportFieldID == sumfield.ReportFieldID);

                        UserReportSummarizeField ursf = new UserReportSummarizeField();

                        ursf.CreatedByUserID = userId;
                        ursf.CreatedDate = DateTime.Now;
                        ursf.UserReport = userRpt;
                        ursf.UserReportFieldID = urfs.UserReportFieldID;
                        entities.AddToUserReportSummarizeField(ursf);
                    }
                }
            
            entities.SaveChanges();
        }

        private void deleteUserReportFields(int userReportID)
        {
            ReportProjectDBEntities entities = new ReportProjectDBEntities();
            var userReportfieldList = from f in entities.UserReportField
                                      where f.UserReport.UserReportID == userReportID
                                      select f;

            if (userReportfieldList != null && userReportfieldList.Count() > 0)
            {

                foreach (UserReportField field in userReportfieldList)
                {
                    entities.UserReportField.DeleteObject(field);
                }

                entities.SaveChanges();
            }
        }
        
        private void deleteUserReportSummarizeFields(int userReportID)
        {
            ReportProjectDBEntities entities = new ReportProjectDBEntities();
            var sumfields = from f in entities.UserReportSummarizeField
                            where f.UserReport.UserReportID == userReportID
                            select f;

            if (sumfields != null && sumfields.Count() > 0)
            {

                foreach (UserReportSummarizeField field in sumfields)
                {
                    entities.UserReportSummarizeField.DeleteObject(field);
                }

                entities.SaveChanges();
            }
        }

        [OperationContract]
        public int SaveStandardUserReport(int userId, int reportID, int reportLayoutStyleID, int formatTypeID, string userReportName, List<RptParameter> parameters, string associationNumber, bool isTransient, bool isTurnOffPageBreak, bool includeTitlePage)
		{
            int userReportID = saveUserReport(reportID, reportLayoutStyleID, userReportName, false, userId, false, associationNumber, formatTypeID, isTransient, isTurnOffPageBreak, includeTitlePage);

            saveUserReportParameters(userReportID, userId, parameters);

		    return userReportID;
		}

		[OperationContract]
        public int SaveCustomUserReport(int userId, int reportID, int formatTypeID, int reportLayoutStyleID, string userReportName, bool isSummaryOnly, List<RptField> fields, List<RptField> summarizeFields, List<RptParameter> parameters, string associationNumber, bool isTransient, bool includeTitlePage)
		{
            int userReportID = saveUserReport(reportID, reportLayoutStyleID, userReportName, isSummaryOnly, userId, true, associationNumber, formatTypeID, isTransient, false, includeTitlePage);

            saveUserReportFields(userReportID, fields, userId, summarizeFields);

            saveUserReportParameters(userReportID, userId, parameters);

		    return userReportID;
		}

		[OperationContract]
        public void UpdateUserReport(int userReportID, int reportLayoutStyleID, int formatTypeID, string userReportName, bool isSummaryOnly, List<RptParameter> parameters, bool isTurnOffPageBreak, bool includeTitlePage)
		{
            bool reportLayoutStyleChanged = false;

			ReportProjectDBEntities entities = new ReportProjectDBEntities();
            ReportLayoutStyle reportLayoutStyle = entities.ReportLayoutStyle.FirstOrDefault(rs => rs.ReportLayoutStyleID == reportLayoutStyleID);
            UserReport ur = entities.UserReport.First(r => r.UserReportID == userReportID);
			ur.ModifiedDate = DateTime.Now;
            ur.UserReportName = userReportName;
		    ur.IsSummaryOnly = isSummaryOnly;
		    ur.IsTurnOffPageBreak = isTurnOffPageBreak;
            ur.IncludeTitlePage = includeTitlePage;
            ur.IsCustom = reportLayoutStyle.IsCustom;
            if (ur.ReportLayoutStyleID != reportLayoutStyleID) reportLayoutStyleChanged = true;
            ur.ReportLayoutStyle = reportLayoutStyle;
		    ur.FormatType = entities.FormatType.FirstOrDefault(f => f.FormatTypeID == formatTypeID);
			entities.SaveChanges();

			var userReportParameters = from urp in entities.UserReportParameter
                               .Include("UserReport")
							   where urp.UserReport.UserReportID == userReportID
							   select urp;

			foreach (var val in userReportParameters)
			{
				val.ParameterValue = parameters.First(v => v.UserReportParameterID == val.UserReportParameterID).DefaultValue;
			}

            if (!ur.IsCustom)
            {
                deleteUserReportSummarizeFields(userReportID);
                deleteUserReportFields(userReportID);
            }

			entities.SaveChanges();

            ReportDomainService domainService = new ReportDomainService();
            if (reportLayoutStyleChanged)
            {
                domainService.RelocateSchedulesForUserReport(ur.UserReportID);
            }
            else
            {
                domainService.SynchronizeSchedulesForUserReport(ur.UserReportID);
            }
		}

		[OperationContract]
		public void UpdateCustomUserReport(int userReportID, List<RptField> fields, int userId, List<RptField> summarizeFields)
		{
			ReportProjectDBEntities entities = new ReportProjectDBEntities();

			UserReport ur = entities.UserReport.First(r => r.UserReportID == userReportID);
			ur.ModifiedDate = DateTime.Now;

            deleteUserReportSummarizeFields(userReportID);

            updateUserReportFields(userReportID, fields, userId, summarizeFields);           

            entities.SaveChanges();             
       
		}

        [OperationContract]
        public void SetReportTypeCheckpoints(List<Rpt> reportTypes, string checkpointID)
        {
            ReportProjectDBEntities entities = new ReportProjectDBEntities();
            var removeReport_ICESecurityCheckpoint = from rptj in entities.Report_ICESecurityCheckpoint
                              where rptj.CheckpointID.Trim() == checkpointID.Trim()
                              select rptj;

            foreach (Report_ICESecurityCheckpoint Report_ICESecurityCheckpoint in removeReport_ICESecurityCheckpoint)
            {
                entities.DeleteObject(Report_ICESecurityCheckpoint);
            }

            entities.SaveChanges();

            foreach (Rpt rpt in reportTypes)
            {
                entities.AddToReport_ICESecurityCheckpoint(new Report_ICESecurityCheckpoint { CheckpointID = checkpointID, ReportID = rpt.ID });
            }

            entities.SaveChanges();
        }

        [OperationContract]
        public List<object> GetParametersTurnOffPageBreak(int reportID, Dictionary<string, object> parameters, bool isSummaryOnly, int formatTypeID, bool isCustomReport)
        {
            List<object> myParams = new List<object>();

            if (formatTypeID == (int)FormatTypeEnum.Excel)
            {
                bool verifyRowCountSp = true;

                ReportProjectDBEntities entities = new ReportProjectDBEntities();

                var rptObj = (from r in entities.Report
                              where r.ReportID == reportID
                              select new { r.VerifyRowCountSP, r.VerifyRowCountBeforeExecuting }).FirstOrDefault();

                List<ReportParameter> listParameters = (from rp in entities.ReportParameter
                                                            .Include("Report")
                                                        where rp.Report.ReportID == reportID && rp.IsQueryParameter
                                                        select rp).ToList();

                verifyRowCountSp = rptObj.VerifyRowCountBeforeExecuting;

                bool includeOneLineClaimDetail = true;
                foreach (KeyValuePair<string, object> keyValuePair in parameters)
                {
                    if (keyValuePair.Key == "IncludeOneLineClaimDetail")
                    {
                        includeOneLineClaimDetail = Convert.ToBoolean(keyValuePair.Value);
                    }
                }

                if ((reportID == (int)ReportEnum.LossRunSummary) && !includeOneLineClaimDetail)
                {
                    verifyRowCountSp = false;
                }

                if (rptObj != null)
                {
                    if (!isSummaryOnly && verifyRowCountSp)
                    {
                        foreach (KeyValuePair<string, object> keyValuePair in parameters)
                        {
                            ReportParameter parameter = listParameters.Where(lp => lp.ReportParameterName.ToLower() == keyValuePair.Key.ToLower()).FirstOrDefault();
                            if (parameter != null)
                            {
                                parameter.DefaultValue = keyValuePair.Value != null ? keyValuePair.Value.ToString() : "";
                            }
                        }

                        foreach (ReportParameter rptParam in listParameters)
                        {
                            myParams.Add("@" + rptParam.ReportParameterName);
                            myParams.Add(rptParam.DefaultValue);
                        }

                        var isCustomParameterIndex = myParams.IndexOf("@IsCustom");
                        if (isCustomParameterIndex != -1)
                        {
                            myParams[isCustomParameterIndex + 1] = (isCustomReport) ? "True" : "False";
                        }
                        else
                        {
                            myParams.Add("@IsCustom");
                            myParams.Add((isCustomReport) ? "True" : "False");
                        }
                        myParams.Add("#");
                        myParams.Add(rptObj.VerifyRowCountSP.ToString().Trim());
                    }
                }
            }
            return myParams;
        }

    }
}
