using System;
using System.Configuration;
using System.Data;
using System.Web.ApplicationServices;
using ReportBuilderAjax.Web.Common;
using ReportBuilderAjax.Web.Models;
using ReportBuilderAjax.Web.Services;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.DomainServices.Hosting;
using System.ServiceModel.DomainServices.EntityFramework;
using System.ServiceModel.DomainServices.Server;
using ReportBuilderUILib.Filter;

namespace ReportBuilderAjax.Web
{
	[EnableClientAccess()]
	public class ReportDomainService : LinqToEntitiesDomainService<ReportProjectDBEntities>  //DomainService
	{
	    private string previewImageURL
	    {
            get
            {
                if (ConfigurationManager.AppSettings["PreviewImageURL"] != null)
                {
                    return ConfigurationManager.AppSettings["PreviewImageURL"];
                }
                return "";
            }
	    }

        private string clearSlashDelimeter(bool fromRight, string url, string delimeter)
        {
            if (!string.IsNullOrEmpty(url))
            {
                if (!fromRight)
                {
                    if (url.LastIndexOf(@delimeter) == url.Length - 1)
                    {
                        url = url.Substring(0, url.Length - 1);
                    }
                }
                else
                {
                    if (url.IndexOf(@delimeter) == 0)
                    {
                        url = url.Substring(1, url.Length - 1);
                    }
                }
            }
            return url;
        }

        private string combineUrls(string url1, string url2)
        {
            return string.Format("{0}/{1}", clearSlashDelimeter(false, url1, @"/"), clearSlashDelimeter(true, clearSlashDelimeter(false, url2, @"/"), @"/"));
        }

        public IQueryable<ReportFolder> GetReportFolders()
        {
            var reports = from rf in this.ObjectContext.ReportFolder
                          where rf.ReportFolderID != -1
                          orderby rf.OrderIndex
                          select rf; 

            List<ReportFolder> list = new List<ReportFolder>();
            foreach (ReportFolder reportFolder in reports)
            {
                if (!list.Contains(reportFolder))
                {
                    list.Add(reportFolder);
                }
            }
            return list.AsQueryable();
        }

        public Rpt GetReportById(int reportId)
        {
            var item = (from r in this.ObjectContext.Report
                             join f in this.ObjectContext.Filter on r.Filters.FilterID equals f.FilterID
                             where r.ReportID == reportId
                             select new Rpt
                                        {
                                            ID = r.ReportID,
                                            Name = r.ReportName,
                                            Description = r.Description,
                                            FilterSystemType = f.SystemType,
                                            ReportFolderID = r.ReportFolderID
                                        }).SingleOrDefault();
            return item;
        }

        public IQueryable<Rpt> GetReports(string userName, bool clientIsAssociation)
        {
            AuthenticationService authenticationService = new AuthenticationService();
            List<string> userCheckpoints = authenticationService.GetUserCheckpoints(userName).CheckpointIds;
            return this.GetReports(userName, clientIsAssociation, userCheckpoints);
        }

	    public IQueryable<Rpt> GetReports(string userName, bool clientIsAssociation, List<string> userCheckpoints)
		{
	        bool viewAllReports = userCheckpoints.Contains("rb_ViewAllReports");

	        List<Rpt> mylist = new List<Rpt>();
			var myreports = (from r in this.ObjectContext.Report
                            .Include("Filters")
							orderby r.ReportName
                            where r.ReportFolderID != -1 && (!clientIsAssociation ? r.ReportID != (int)ReportEnum.LossRatioWithLossSummary : true)
							select r).ToList();

            List<Report_ICESecurityCheckpoint> myReportJoin = (from rtjn in this.ObjectContext.Report_ICESecurityCheckpoint
                               where userCheckpoints.Contains(rtjn.CheckpointID.Trim())
                               select rtjn).ToList();

			foreach (var item in myreports)
			{
                if (mylist.FirstOrDefault(r => r.ID == item.ReportID) == null && (myReportJoin.FirstOrDefault(rj => rj.ReportID == item.ReportID) != null || viewAllReports))
                {
                    Rpt rpt = new Rpt()
                    {
                        ID = item.ReportID,
                        Name = item.ReportName,
                        Description = item.Description,
                        FilterSystemType = item.Filters != null ? item.Filters.SystemType : null,
                        ReportFolderID = item.ReportFolderID
                    };
                    mylist.Add(rpt);
                }
			}

			return mylist.AsQueryable();
		}

        public IQueryable<Rpt> GetReportsForCheckpointConfiguration(string checkpointID)
        {
            var reportTypeJoinList = (from rtj in this.ObjectContext.Report_ICESecurityCheckpoint
                                     where rtj.CheckpointID.Trim() == checkpointID.Trim()
                                     select rtj).ToList();

            var reportTypes = (from rf in this.ObjectContext.Report
                                where rf.ReportFolderID != -1
                                select rf).ToList();

            List<Rpt> mylist = new List<Rpt>();
            foreach (Report reportType in reportTypes)
            {
                mylist.Add(new Rpt
                {
                    ID = reportType.ReportID,
                    ReportFolderID = reportType.ReportFolderID,
                    Name = reportType.ReportName,
                    IsUseWithCheckpoint = reportTypeJoinList.Where(rfj => rfj.ReportID == reportType.ReportID).FirstOrDefault() != null
                });
            }

            return mylist.AsQueryable();
        }

        public IQueryable<UserRpt> GetUserReports(int userID, string clientNumber, string userName)
        {
            AuthenticationService authenticationService = new AuthenticationService();
            List<string> userCheckpoints = authenticationService.GetUserCheckpoints(userName).CheckpointIds;
            return this.GetUserReports(userID, clientNumber, userName, userCheckpoints);
        }

        public IQueryable<UserRpt> GetUserReports(int userID, string clientNumber, string userName, List<string> userCheckpoints)
        {
            bool viewAllReports = userCheckpoints.Contains("rb_ViewAllReports");

            List<UserRpt> mylist = new List<UserRpt>();
            
            var myreports = (from r in this.ObjectContext.UserReport
                            .Include("ReportLayoutStyle")
                            .Include("Report")
                            where r.UserID == userID && r.ClientNumber == clientNumber && (r.IsDeleted == false || r.IsDeleted == null) && r.IsTransient == false
                            select new
                            {
                                UserReportID = r.UserReportID,
                                ReportName = r.Report.ReportName,
                                UserReportName = r.UserReportName,
                                CreatedDate = r.CreatedDate,
                                ModifiedDate = r.ModifiedDate,
                                IsCustom = r.IsCustom,
                                ReportLayoutStyleID = r.ReportLayoutStyleID,
                                ReportLayoutStyleName = r.ReportLayoutStyle.ReportLayoutStyleName,
                                FormatTypeID = r.FormatTypeID,
                                FilterSystemType = r.Report != null && r.Report.Filters != null ? r.Report.Filters.SystemType : null,
                                ReportID = r.Report != null ? r.Report.ReportID : -1,
                                IsSummaryOnly = r.IsSummaryOnly,
                                IsTurnOffPageBreak = r.IsTurnOffPageBreak,
                                IncludeTitlePage = r.IncludeTitlePage
                            }).ToList();

            List<Report_ICESecurityCheckpoint> myReportTypesJoin = (from rtj in this.ObjectContext.Report_ICESecurityCheckpoint
                                                               where userCheckpoints.Contains(rtj.CheckpointID.Trim())
                                                                select rtj).ToList();

            var parms = (from p in ObjectContext.UserReportParameter
                        .Include("UserReport")
                        .Include("ReportParameter.DataType")                        
                         where p.UserReport.UserID == userID && (p.UserReport.IsDeleted == false || p.UserReport.IsDeleted == null)
                         select new { UserReportID = p.UserReport.UserReportID, ReportParameterName = p.ReportParameter.ReportParameterName, FilterValue = p.ParameterValue }).ToList();

            var schedules = (from s in ObjectContext.Schedule
                                 .Include("UserReport")
                             join u in ObjectContext.UserReport on s.UserReport.UserReportID equals u.UserReportID
                             where u.UserID == userID && s.IsActive && (u.IsDeleted == false || u.IsDeleted == null)
                             select new {ID = s.ScheduleID, UserReportID = s.UserReport.UserReportID}).ToList();

            foreach (var item in myreports)
            {
                bool isCustomDateRange = false;
                var parm =
                    parms.Where(
                        p =>
                        p.UserReportID == item.UserReportID &&
                        (p.ReportParameterName.ToLower() == "dateperiod" ||
                         p.ReportParameterName.ToLower() == "daterange")).FirstOrDefault();
                if (parm != null)
                {
                    isCustomDateRange = parm.FilterValue == "6";
                }

                UserRpt userReport = new UserRpt()
                {
                    UserReportID = item.UserReportID,
                    Name = item.ReportName,
                    UserReportName = item.UserReportName,
                    CreatedDate = item.CreatedDate,
                    ModifiedDate = item.ModifiedDate,
                    IsCustom = item.IsCustom,
                    ReportLayoutStyleID = item.ReportLayoutStyleID,
                    ReportLayoutStyleName = item.ReportLayoutStyleName,
                    FormatType = (FormatTypeEnum)item.FormatTypeID,
                    IsExistsSchedules = schedules.FirstOrDefault(s => s.UserReportID == item.UserReportID) != null ? true : false,
                    FilterSystemType = item.FilterSystemType,
                    ReportID = item.ReportID,
                    IsSummaryOnly = item.IsSummaryOnly,
                    IsCustomDateRange = isCustomDateRange,
                    IsTurnOffPageBreak = item.IsTurnOffPageBreak,
                    IncludeTitlePage = item.IncludeTitlePage
                };

                if (mylist.Where(r => r.UserReportID == item.UserReportID).FirstOrDefault() == null && (myReportTypesJoin.Where(rj => rj.ReportID == item.ReportID).FirstOrDefault() != null || viewAllReports))
                {
                    mylist.Add(userReport);
                }
            }

            return mylist.AsQueryable();
        }

        public UserRpt GetUserReportById(int userID, string clientNumber, string userName, int userReportId, List<string> userCheckpoints)
        {
            var myreports = (from r in this.ObjectContext.UserReport
                            .Include("ReportLayoutStyle")
                            .Include("Report")
                             where r.UserID == userID && r.ClientNumber == clientNumber && (r.IsDeleted == false || r.IsDeleted == null) && r.IsTransient == false && r.UserReportID == userReportId
                             select new
                             {
                                 UserReportID = r.UserReportID,
                                 ReportName = r.Report.ReportName,
                                 UserReportName = r.UserReportName,
                                 CreatedDate = r.CreatedDate,
                                 ModifiedDate = r.ModifiedDate,
                                 IsCustom = r.IsCustom,
                                 ReportLayoutStyleID = r.ReportLayoutStyleID,
                                 ReportLayoutStyleName = r.ReportLayoutStyle.ReportLayoutStyleName,
                                 FormatTypeID = r.FormatTypeID,
                                 FilterSystemType = r.Report != null && r.Report.Filters != null ? r.Report.Filters.SystemType : null,
                                 ReportID = r.Report != null ? r.Report.ReportID : -1,
                                 IsSummaryOnly = r.IsSummaryOnly,
                                 IsTurnOffPageBreak = r.IsTurnOffPageBreak,
                                 IncludeTitlePage = r.IncludeTitlePage
                             });
            
            foreach (var item in myreports)
            {
                UserRpt userReport = new UserRpt()
                {
                    UserReportID = item.UserReportID,
                    Name = item.ReportName,
                    UserReportName = item.UserReportName,
                    CreatedDate = item.CreatedDate,
                    ModifiedDate = item.ModifiedDate,
                    IsCustom = item.IsCustom,
                    ReportLayoutStyleID = item.ReportLayoutStyleID,
                    ReportLayoutStyleName = item.ReportLayoutStyleName,
                    FormatType = (FormatTypeEnum)item.FormatTypeID,
                    FilterSystemType = item.FilterSystemType,
                    ReportID = item.ReportID,
                    IsSummaryOnly = item.IsSummaryOnly,
                    IsTurnOffPageBreak = item.IsTurnOffPageBreak,
                    IncludeTitlePage = item.IncludeTitlePage
                };
                return userReport;
            }

            return null;
        }

        public List<string> GetUserReportNames(int userID, string clientNumber, string userName, List<string> userCheckpoints)
        {
            bool viewAllReports = userCheckpoints.Contains("rb_ViewAllReports");

            List<string> mylist = new List<string>();

            var myreports = (from r in this.ObjectContext.UserReport
                             where r.UserID == userID && r.ClientNumber == clientNumber && (r.IsDeleted == false || r.IsDeleted == null) && r.IsTransient == false
                             select new
                             {
                                 UserReportID = r.UserReportID,
                                 UserReportName = r.UserReportName,
                                 ReportID = r.ReportID
                             }).ToList();

            List<Report_ICESecurityCheckpoint> myReportTypesJoin = (from rtj in this.ObjectContext.Report_ICESecurityCheckpoint
                                                                    where userCheckpoints.Contains(rtj.CheckpointID.Trim())
                                                                    select rtj).ToList();

            foreach (var item in myreports)
            {
                if (myReportTypesJoin.Where(rj => rj.ReportID == item.ReportID).FirstOrDefault() != null || viewAllReports)
                {
                    if (!mylist.Contains(item.UserReportName))
                    {
                        mylist.Add(item.UserReportName);
                    }
                }
            }

            return mylist;
        }

		public IQueryable<Rpt> GetUserReport(int userReportID)
		{
			// Only a single report should be returned, but
			// RIA Services for VS2008 can not return
			// a single entity, so we have to do it
			// this way

			List<Rpt> mylist = new List<Rpt>();

			var userRpt = from r in this.ObjectContext.UserReport
                          where r.UserReportID == userReportID && (r.IsDeleted == false || r.IsDeleted == null)
						  select r;

			foreach (var item in userRpt)
			{
				mylist.Add(new Rpt() {
					ID = item.UserReportID,
					Name = item.UserReportName,
					Description = item.UserReportName
				});
			}

			return mylist.AsQueryable();
		}

		public IQueryable<RptField> GetUserReportSummarizeFields(int userReportID)
        {
            var userSummarizefieldList = (from fSumField in this.ObjectContext.UserReportSummarizeField
                            join f in this.ObjectContext.UserReportField on fSumField.UserReportID equals f.UserReport.UserReportID
                            join rf in this.ObjectContext.ReportField on f.ReportField.ReportFieldID equals rf.ReportFieldID
                            where fSumField.UserReport.UserReportID == userReportID && (fSumField.UserReport.IsDeleted == false || fSumField.UserReport.IsDeleted == null)
                                    && (fSumField.UserReportFieldID == f.UserReportFieldID)                            
                            select new
                                   {
                                       SortDirection = f.SortDirection,   
                                       ReportFieldID = rf.ReportFieldID, 
                                       UserReportFieldID = f.UserReportFieldID,
                                       CustomName = f.CustomName,
                                       ReportField = f.ReportField,
                                       ColumnOrder = f.ColumnOrder,
                                       ColumnWidthFactor = rf.ColumnWidthFactor,
                                       SortOrder = f.SortOrder,
                                       GroupOrder = f.GroupOrder,
                                       GroupSummaryExpression = rf.GroupSummaryExpression,
                                       IncludePageBreak = f.IncludePageBreak,
                                       IsDisplayInReport = rf.IsDisplayInReport,
                                       IsGroupByDefault = rf.IsGroupByDefault,
                                       IsGroupable = rf.IsGroupable,
                                       DataTypeID = f.ReportField.DataType.DataTypeID,
                                       SQLFieldName = f.ReportField.SQLFieldName,
                                       IsSummarizable = rf.IsSummarizable,
									   Category = rf.Category,
                                       FieldValueExpression = rf.FieldValueExpression,
                                       UseSummaryExpresionGroup = rf.UseSummaryExpresionGroup 

                                   }).ToList();


			List<RptField> mySummarizeFieldlist = new List<RptField>();

            foreach (var sumfield in userSummarizefieldList)
            {
                SortDirectionEnum sd = SortDirectionEnum.Ascending;
                if (sumfield.SortDirection == "D")
                {
                    sd = SortDirectionEnum.Descending;
                }
                else
                {
                    sd = SortDirectionEnum.Ascending;
                }

				RptField mySumItem = new RptField()
                {
                    ID = sumfield.UserReportFieldID,
                    ReportFieldID = sumfield.ReportFieldID,  
                    ReportID = userReportID,
                    Name = sumfield.CustomName,
                    SQLName = sumfield.SQLFieldName,
                    DataType = (DataTypeEnum)sumfield.DataTypeID,
                    ColumnOrder = sumfield.ColumnOrder,
                    ColumnWidthFactor = sumfield.ColumnWidthFactor,
                    SortOrder = sumfield.SortOrder,
                    SortDirection = sd,
                    GroupOrder = sumfield.GroupOrder,
                    GroupSummaryExpression = sumfield.GroupSummaryExpression,                       
                    IncludePageBreak = sumfield.IncludePageBreak,
                    IsDisplayInReport = sumfield.IsDisplayInReport,
                    IsGroupByDefault = sumfield.IsGroupByDefault,
                    IsUsed = sumfield.ColumnOrder > 0 ? true : false,
                    IsGroupable = sumfield.IsGroupable,
                    IsSummarizable = sumfield.IsSummarizable,
					Category = sumfield.Category,
                    FieldValueExpression = sumfield.FieldValueExpression,
                    UseSummaryExpresionGroup = sumfield.UseSummaryExpresionGroup 

                };

                mySummarizeFieldlist.Add(mySumItem);
            }

            return mySummarizeFieldlist.AsQueryable();          
        }

        private bool ClientSpecificCodeAvailable(int reportID)
        {
            bool isAvailable = false;

            if (reportID == (int)(ReportEnum.TransactionRegister) || reportID == (int)(ReportEnum.MonthlyBillingReport)
               || reportID == (int)(ReportEnum.LitigationManagementBasic) || reportID == (int)(ReportEnum.LitigationManagementDetail)
               || reportID == (int)(ReportEnum.ActivityReport) || reportID == (int)(ReportEnum.ClaimLag)
               || reportID == (int)(ReportEnum.LossRunSummary) || reportID == (int)(ReportEnum.LossRunDetailExpanded)
               || reportID == (int)(ReportEnum.PAndIReimbursableReport) || reportID == (int)(ReportEnum.ReserveChangeReport)
               || reportID == (int)(ReportEnum.ManagedCareDiagnosticSavings) || reportID == (int)(ReportEnum.ManagedCareSavingsAndFeesDetail)
               || reportID == (int)(ReportEnum.LossRunSummaryForPowerUser))
            {
                isAvailable = true;
            }

            return isAvailable;
        }
       
		public IQueryable<RptField> GetReportFields(int reportID, string clientNumber)
		{
			List<RptField> mylist = new List<RptField>();

			var fields = (from f in this.ObjectContext.ReportField
							 .Include("DataType")
							 .Include("Report")
						 where f.Report.ReportID == reportID
						 orderby f.ColumnOrder
						 select f).ToList();
            var i = 1;

            List<AssnSpecificFieldDefinition> clientSpecificCodeFields = new List<AssnSpecificFieldDefinition>();

            if (ClientSpecificCodeAvailable(reportID))
            {
                clientSpecificCodeFields = (from f in this.ObjectContext.AssnSpecificFieldDefinition
                                               where f.AssociationNumber == clientNumber
                                               select f).ToList();
            }

            foreach (var field in fields)
            {
                SortDirectionEnum sd = SortDirectionEnum.Ascending;

                if (!field.isClientSpecificCode)
                {
                    mylist.Add(new RptField()
                 {
                     ID =  i++,
                     RptFieldID = field.ReportFieldID,
                     ReportFieldID = field.ReportFieldID,
                     ReportID = reportID,
                     Name = (field.IsCalculated && field.FieldName.Length > 0 && field.FieldName[0] != '@') ? '@' + field.FieldName : field.FieldName,
                     SQLName = field.SQLFieldName,
                     DataType = (DataTypeEnum)field.DataType.DataTypeID,
                     ColumnOrder = field.ColumnOrder,
                     ColumnWidthFactor = field.ColumnWidthFactor,
                     SortOrder = -1,
                     SortDirection = sd,
                     GroupOrder = -1,
                     GroupSummaryExpression = field.GroupSummaryExpression,
                     IsGroupable = field.IsGroupable,
                     IncludePageBreak = field.IncludePageBreak,
                     IsDisplayInReport = field.IsDisplayInReport,
                     IsGroupByDefault = field.IsGroupByDefault,
                     IsSummarizable = field.IsSummarizable,
                     IsUsed = field.ColumnOrder > 0 ? true : false,
                     Category = field.Category,
                     FieldValueExpression = field.FieldValueExpression,
                     IsCalculated = field.IsCalculated,
                     CalculationDescription = field.CalculationDescription,
                     UseSummaryExpresionGroup = field.UseSummaryExpresionGroup,
                     IsClientSpecificCode = field.isClientSpecificCode
                 });

                }
                else
                {
                    var specificCodeToAdd = from f in clientSpecificCodeFields
                                            where f.FieldName == field.SQLFieldName 
                                            select f;

                    if (specificCodeToAdd.Count() > 0)
                    {

                        foreach (var specificField in specificCodeToAdd)
                        {
                            mylist.Add(new RptField()
                            {
                                ID = i++,
                                RptFieldID = field.ReportFieldID,
                                ReportFieldID = field.ReportFieldID,
                                ReportID = reportID,
                                Name = specificField.FieldDescription,
                                SQLName = specificField.FieldName,
                                DataType = (DataTypeEnum)field.DataType.DataTypeID,
                                ColumnOrder = field.ColumnOrder,
                                ColumnWidthFactor = field.ColumnWidthFactor,
                                SortOrder = -1,
                                SortDirection = sd,
                                GroupOrder = -1,
                                GroupSummaryExpression = field.GroupSummaryExpression,
                                IsGroupable = field.IsGroupable,
                                IncludePageBreak = field.IncludePageBreak,
                                IsDisplayInReport = field.IsDisplayInReport,
                                IsGroupByDefault = field.IsGroupByDefault,
                                IsSummarizable = field.IsSummarizable,
                                IsUsed = field.ColumnOrder > 0 ? true : false,
                                Category = field.Category,
                                FieldValueExpression = field.FieldValueExpression,
                                IsCalculated = field.IsCalculated,
                                CalculationDescription = field.CalculationDescription,
                                UseSummaryExpresionGroup = field.UseSummaryExpresionGroup,
                                CoverageCode = specificField.coverage_code,
                                IsClientSpecificCode = field.isClientSpecificCode
                            });
                        }
                    }
                }
            }			

			return mylist.AsQueryable();
		}         

        //Get all user report fields and also any available report fields (that is, the report fields not used yet on this report) for this client
		public IQueryable<RptField> GetUserReportFields(int userReportID, int reportID, string associationNumber)
		{
			List<RptField> userReportFieldList = new List<RptField>();
            var i = 0; 
            var fieldsInUse = (from f in this.ObjectContext.UserReportField
                             where f.UserReport.UserReportID == userReportID
                             orderby f.ColumnOrder
                             select f);

            var allReportFields = (from rf in this.ObjectContext.ReportField
                                   join f in fieldsInUse on rf.ReportFieldID equals f.ReportField.ReportFieldID into userRptField
                                   from urf in userRptField.DefaultIfEmpty()
                                   where rf.Report.ReportID == reportID && (urf.UserReport.IsDeleted == false || urf.UserReport.IsDeleted == null)
                                   orderby urf.ColumnOrder
                                   select new
                                   {
                                       SortDirection = (urf == null) ?  "A" : urf.SortDirection,
                                       ReportFieldID = rf.ReportFieldID,
                                       UserReportFieldID = (urf == null) ? rf.ReportFieldID : urf.UserReportFieldID, //TODO: this is a strange and confusing thing to do. You should never set the UserReportFieldID equal to the ReportFieldID.  Why is this necessary?
                                       CustomName = (urf == null) ? rf.FieldName : urf.CustomName,
                                       ColumnOrder = (urf == null) ? rf.ColumnOrder : urf.ColumnOrder,
                                       ColumnWidthFactor = rf.ColumnWidthFactor,
                                       SortOrder = (urf == null) ? -1 : urf.SortOrder,
                                       GroupOrder = (urf == null) ? -1 : urf.GroupOrder,
                                       IncludePageBreak = (urf == null) ? rf.IncludePageBreak : urf.IncludePageBreak,
                                       SQLFieldName = rf.SQLFieldName,
                                       ReportField = rf,
                                       DataTypeID = rf.DataType.DataTypeID,
                                       GroupSummaryExpression = rf.GroupSummaryExpression,
                                       IsDisplayInReport = rf.IsDisplayInReport,
                                       IsGroupByDefault = rf.IsGroupByDefault,
                                       IsGroupable = rf.IsGroupable,
                                       IsSummarizable = rf.IsSummarizable,
                                       IsClientSpecific = rf.IsClientSpecific, 
									   Category = rf.Category,
                                       FieldValueExpression = rf.FieldValueExpression,
                                       IsCalculated = rf.IsCalculated,
                                       CalculationDescription = rf.CalculationDescription,
                                       UseSummaryExpresionGroup = rf.UseSummaryExpresionGroup,
                                       CoverageCode = (urf == null) ? string.Empty : urf.CoverageCode,
                                       IsClientSpecificCode = rf.isClientSpecificCode
                                   }).ToList();

            List<AssnSpecificFieldDefinition> clientSpecificCodeFields = new List<AssnSpecificFieldDefinition>();

            if (ClientSpecificCodeAvailable(reportID))
            {
                clientSpecificCodeFields = (from f in this.ObjectContext.AssnSpecificFieldDefinition
                                               where f.AssociationNumber == associationNumber
                                               select f).ToList();
            }

            foreach (var reportField in allReportFields)
            {
                if (reportField.IsClientSpecificCode)
                {
                    clientSpecificCodeFields.RemoveAll(x => x.coverage_code == reportField.CoverageCode && x.FieldName == reportField.SQLFieldName);
                }
            }


            List<int> clientSpecificFields = new List<int>(); 
            clientSpecificFields = (from crf in this.ObjectContext.ClientReportField where crf.ClientNumber == associationNumber select crf.ReportFieldID).ToList(); 

            var allClientFields = from rf in allReportFields                             
                             where (rf.IsClientSpecific == false || clientSpecificFields.Contains(rf.ReportFieldID))
                             select rf;

            foreach (var clientField in allClientFields)
			{
				SortDirectionEnum sd = SortDirectionEnum.Ascending;
                if (clientField.SortDirection == "D")
				{
					sd = SortDirectionEnum.Descending;
				}
				else
				{
					sd = SortDirectionEnum.Ascending;
				}

                if (!clientField.IsClientSpecificCode)
                {
                    RptField reportfield = new RptField()
                    {
                        ID = i++,
                        RptFieldID = clientField.UserReportFieldID,
                        ReportFieldID = clientField.ReportFieldID,
                        ReportID = userReportID,
                        Name = (clientField.IsCalculated && clientField.CustomName.Length > 0 && clientField.CustomName[0] != '@') ? '@' + clientField.CustomName : clientField.CustomName,
                        SQLName = clientField.SQLFieldName,
                        DataType = (DataTypeEnum)clientField.DataTypeID,
                        ColumnOrder = clientField.ColumnOrder,
                        ColumnWidthFactor = clientField.ColumnWidthFactor,
                        SortOrder = clientField.SortOrder,
                        SortDirection = sd,
                        GroupOrder = clientField.GroupOrder,
                        GroupSummaryExpression = clientField.GroupSummaryExpression,
                        IncludePageBreak = clientField.IncludePageBreak,
                        IsDisplayInReport = clientField.IsDisplayInReport,
                        IsGroupByDefault = clientField.IsGroupByDefault,
                        IsGroupable = clientField.IsGroupable,
                        IsSummarizable = clientField.IsSummarizable,
                        Category = clientField.Category,
                        FieldValueExpression = clientField.FieldValueExpression,
                        IsCalculated = clientField.IsCalculated,
                        CalculationDescription = clientField.CalculationDescription,
                        UseSummaryExpresionGroup = clientField.UseSummaryExpresionGroup,
                        CoverageCode = clientField.CoverageCode,
                        IsClientSpecificCode = clientField.IsClientSpecificCode
                    };
                    userReportFieldList.Add(reportfield);
                }
                else
                {
                    if (clientField.ColumnOrder > 0)
                    {
                        RptField reportfield = new RptField()
                        {
                            ID = i++,
                            RptFieldID = clientField.UserReportFieldID,
                            ReportFieldID = clientField.ReportFieldID,
                            ReportID = userReportID,
                            Name = (clientField.IsCalculated && clientField.CustomName.Length > 0 && clientField.CustomName[0] != '@') ? '@' + clientField.CustomName : clientField.CustomName,
                            SQLName = clientField.SQLFieldName + "_" + clientField.CoverageCode,
                            DataType = (DataTypeEnum)clientField.DataTypeID,
                            ColumnOrder = clientField.ColumnOrder,
                            ColumnWidthFactor = clientField.ColumnWidthFactor,
                            SortOrder = clientField.SortOrder,
                            SortDirection = sd,
                            GroupOrder = clientField.GroupOrder,
                            GroupSummaryExpression = clientField.GroupSummaryExpression,
                            IncludePageBreak = clientField.IncludePageBreak,
                            IsDisplayInReport = clientField.IsDisplayInReport,
                            IsGroupByDefault = clientField.IsGroupByDefault,
                            IsGroupable = clientField.IsGroupable,
                            IsSummarizable = clientField.IsSummarizable,
                            Category = clientField.Category,
                            FieldValueExpression = clientField.FieldValueExpression,
                            IsCalculated = clientField.IsCalculated,
                            CalculationDescription = clientField.CalculationDescription,
                            UseSummaryExpresionGroup = clientField.UseSummaryExpresionGroup,
                            CoverageCode = clientField.CoverageCode,
                            IsClientSpecificCode = clientField.IsClientSpecificCode
                        };
                        userReportFieldList.Add(reportfield);
                    }
                    
                    if (clientSpecificCodeFields.Count() > 0)
                    {
                        var specificCodeToAdd = from f in clientSpecificCodeFields
                                                where f.FieldName == clientField.SQLFieldName 
                                                select f;

                        foreach (var field in specificCodeToAdd)
                        {
                            RptField reportfield = new RptField()
                            {
                                ID = i++,
                                RptFieldID = clientField.ReportFieldID,
                                ReportFieldID = clientField.ReportFieldID,
                                ReportID = userReportID,
                                Name = field.FieldDescription,
                                SQLName = field.FieldName + '_' + field.coverage_code,
                                DataType = (DataTypeEnum)clientField.DataTypeID,
                                ColumnOrder = -1,
                                ColumnWidthFactor = clientField.ColumnWidthFactor,
                                SortOrder = clientField.SortOrder,
                                SortDirection = sd,
                                GroupOrder = clientField.GroupOrder,
                                GroupSummaryExpression = clientField.GroupSummaryExpression,
                                IncludePageBreak = clientField.IncludePageBreak,
                                IsDisplayInReport = clientField.IsDisplayInReport,
                                IsGroupByDefault = clientField.IsGroupByDefault,
                                IsGroupable = clientField.IsGroupable,
                                IsSummarizable = clientField.IsSummarizable,
                                Category = clientField.Category,
                                FieldValueExpression = clientField.FieldValueExpression,
                                IsCalculated = clientField.IsCalculated,
                                CalculationDescription = clientField.CalculationDescription,
                                UseSummaryExpresionGroup = clientField.UseSummaryExpresionGroup,
                                CoverageCode = field.coverage_code,
                                IsClientSpecificCode = clientField.IsClientSpecificCode
                            };
                            userReportFieldList.Add(reportfield);

                        }
                    }
                }  
			}

            return userReportFieldList.AsQueryable();
		}

        public IQueryable<RptLayoutStyle> GetReportLayoutStyles(int reportID)
		{
            List<RptLayoutStyle> mylist = new List<RptLayoutStyle>();

            var layoutStyles = (from g in this.ObjectContext.ReportLayoutStyle
                            where g.ReportID == reportID
                            && g.IsUsed == true
                            orderby g.OrderIndex  
                            select g).ToList();

            foreach (var item in layoutStyles)
            {
                mylist.Add(new RptLayoutStyle()
                {
                    ID = item.ReportLayoutStyleID,
                    Name = item.ReportLayoutStyleName,
                    IsCustom = item.IsCustom,
                    PreviewImagePath = !string.IsNullOrEmpty(previewImageURL) && !string.IsNullOrEmpty(item.PreviewImagePath) ? combineUrls(combineUrls(previewImageURL, "Small"), item.PreviewImagePath) : null
                });
            }

			return mylist.AsQueryable();
		}      	

		[Delete]
		public void DeleteReport(int userReportID, int reportId)
		{
            var userReportToDelete = this.ObjectContext.UserReport
                                    .Where(x => x.UserReportID == userReportID)
                                    .First();

            userReportToDelete.IsDeleted = true;
            
            //SubscriptionManager subscriptionManager = new SubscriptionManager();
            List<Schedule> scheduleList = ObjectContext.Schedule.Include("ScheduleRecipient").Where(s => s.UserReport.UserReportID == userReportID && s.IsActive).ToList();
            foreach (Schedule schedule in scheduleList)
            {
                List<ScheduleRecipient> recipients = schedule.ScheduleRecipient.ToList();
                foreach (ScheduleRecipient recipient in recipients)
                {
                    if (hasScheduleDeliveryLog(schedule))
                    {
                        recipient.IsActive = false;
                    }
                    else
                    {
                        ObjectContext.DeleteObject(recipient);
                    }
                }
                if (hasScheduleDeliveryLog(schedule))
                {
                    schedule.IsActive = false;
                }
                else
                {
                    ObjectContext.DeleteObject(schedule);    // Remove the schedule from our database    
                }  
                
                //subscriptionManager.DeleteSubscription(schedule.SubscriptionID);  // Remove the subscription from RS
            }
		    this.ObjectContext.SaveChanges();
		}

        public IQueryable<RptParameter> GetUserReportParameters(int userReportID, int reportID)
		{
			List<RptParameter> myparameters = new List<RptParameter>();

			var userParameterList = (from p in ObjectContext.UserReportParameter
						.Include("UserReport")
						.Include("ReportParameter.DataType")
                        where p.UserReport.UserReportID == userReportID && (p.UserReport.IsDeleted == false || p.UserReport.IsDeleted == null)
						select p);

            var parameterList = (from rp in ObjectContext.ReportParameter
                                join p in userParameterList on rp.ReportParameterID equals p.ReportParameterID into
                                    userParameterField
                                from urp in userParameterField.DefaultIfEmpty()
                                where
                                    rp.Report.ReportID == reportID &&
                                    (urp.UserReport.IsDeleted == false || urp.UserReport.IsDeleted == null)
                                select new
                                           {
                                               ID = rp.ReportParameterID,
                                               ReportID = reportID,
                                               Name = rp.ReportParameterName,
                                               Description = rp.Description,
                                               DataType = (DataTypeEnum) rp.DataType.DataTypeID,
                                               DefaultValue = (urp == null ? rp.DefaultValue : urp.ParameterValue),
                                               FilterString = (urp == null ? "" : urp.FilterString),
                                               IsReportParameter = rp.IsReportParameter,
                                               IsQueryParameter = rp.IsQueryParameter,
                                               OrderIndex = rp.OrderIndex,
                                               UserReportParameterID = (urp == null ? 0 : urp.UserReportParameterID)

                                           }).ToList();

            foreach (var parameter in parameterList)
			{
				myparameters.Add(new RptParameter() {
					ID = parameter.ID,
					ReportID = reportID,
                    UserReportParameterID = parameter.UserReportParameterID,
					Name = parameter.Name,
					Description = parameter.Description ,
					DataType = (DataTypeEnum)parameter.DataType,
					DefaultValue = parameter.DefaultValue,
                    FilterString = parameter.FilterString,
                    IsReportParameter = parameter.IsReportParameter,
                    IsQueryParameter = parameter.IsQueryParameter,
                    OrderIndex = parameter.OrderIndex,                      
				});
			}
            
			return myparameters.AsQueryable();
		}

	    public IQueryable<UserRptOutput> GetReportOutputs(int userID, string clientNumber)
	    {
	        List<UserRptOutput> list = new List<UserRptOutput>();

	        var reportOutputs = from uo in this.ObjectContext.UserReportOutput
	                                .Include("UserReportOutput.UserReport")
	                            join u in this.ObjectContext.UserReport on uo.UserReport.UserReportID equals u.UserReportID
	                            where uo.UserID == userID && uo.ClientNumber == clientNumber
	                            orderby uo.UserReportID
	                            select new
	                                       {
	                                           ID = uo.UserReportOutputID,
	                                           UserReportID = uo.UserReportID,
	                                           FileName = uo.FileName,
	                                           CreatedDate = uo.CreatedDate,
	                                           Description = uo.UserReport != null ? uo.UserReport.UserReportName : ""
	                                       };

	        foreach (var reportOutput in reportOutputs)
	        {
	            list.Add(new UserRptOutput
	                         {
	                             CreatedDate = reportOutput.CreatedDate,
	                             FileName = reportOutput.FileName,
	                             ID = reportOutput.ID,
	                             ParentId = reportOutput.UserReportID,
	                             Description = reportOutput.Description
	                         });
	        }

	        return list.AsQueryable();
	    }

        public IQueryable<CheckpointObj> GetCheckpoints()
        {
            List<CheckpointObj> resultList = new List<CheckpointObj>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["AuthenticationConnection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_GET_CHECKPOINTS_LIST, CommandType.StoredProcedure,
                                                        new object[]{ });
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count != 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    resultList.Add(new CheckpointObj { CheckpointDescription = row["CheckpointDescription"].ToString(), CheckpointID = row["CheckpointID"].ToString() });
                }
            }

            return resultList.AsQueryable();
        }

        public IQueryable<AdminUserName> GetAdminUserName(string userName)
        {
            return this.ObjectContext.AdminUserName.Where(adm => adm.userName.ToUpper() == userName.ToUpper());
        }
        
        [Delete]
        public void DeleteDeliveredReport(UserRptOutput userReport)
        {

            var deliveredReportHistoryToDelete =  (from log in this.ObjectContext.ReportDeliveryLog 
                                                  join uro in this.ObjectContext.UserReportOutput on new { UserReportOutputID = (Int32)log.UserReportOutputID } equals new { UserReportOutputID = uro.UserReportOutputID } 
                                                  where uro.FileName == userReport.FileName
                                                  select log).First();
           this.ObjectContext.DeleteObject(deliveredReportHistoryToDelete);
           this.ObjectContext.SaveChanges();


            var deliveredReportToDelete = this.ObjectContext.UserReportOutput
                                    .Where(x => x.FileName == userReport.FileName)
                                    .First();
           this.ObjectContext.DeleteObject(deliveredReportToDelete);
           this.ObjectContext.SaveChanges();
        }

        public IQueryable<CheckpointObj> GetUserCheckpoints(string userId)
        {
            AuthenticationService authenticationService = new AuthenticationService();
            List<string> userCheckpoints = authenticationService.GetUserCheckpoints(userId).CheckpointIds;

            // RIA won't less you pass an IQueryable of strings.  You have to pass an collection of objects.
            var checkpoints = new List<CheckpointObj>();
            foreach (var item in userCheckpoints)
            {
                checkpoints.Add(new CheckpointObj() { CheckpointID = item, CheckpointDescription = string.Empty });
            }

            return checkpoints.AsQueryable();
        }

        #region Schedules
        public IQueryable<Schedule> GetSchedules(int userReportID)
        {
            var mySchedules = from s in this.ObjectContext.Schedule
                              .Include("ScheduleRecipient")
                              where s.UserReportID == userReportID && s.IsActive == true
                              orderby s.ScheduleID
                              select s;

            return mySchedules;
        }

        public Schedule GetSchedule(int scheduleID)
        {
            return ObjectContext.Schedule.Include("ScheduleRecipient").FirstOrDefault(s => s.ScheduleID == scheduleID && s.IsActive == true);
        }

        public void InsertSchedule(Schedule schedule)
        {
            schedule.IsActive = true;
            if ((schedule.EntityState != EntityState.Detached))
            {
                this.ObjectContext.ObjectStateManager.ChangeObjectState(schedule, EntityState.Added);
            }
            else
            {
                this.ObjectContext.Schedule.AddObject(schedule);
            }
        }

        public void UpdateSchedule(Schedule currentSchedule)
        {
            /*SubscriptionManager subscriptionManager = new SubscriptionManager();
            subscriptionManager.UpdateSubscription(currentSchedule);

            // Add the entire graph so duplicate keys on new Entities are allowed.
            this.ObjectContext.Schedule.AddObject(currentSchedule);

            // Get the root Entity into the correct state.
            Schedule original = this.ChangeSet.GetOriginal(currentSchedule);
            //currentSchedule.CreatedDate = original.CreatedDate;

            if (original == null)
            {
                this.ObjectContext.ObjectStateManager.ChangeObjectState(currentSchedule, EntityState.Unchanged);
            }
            else
            {
                this.ObjectContext.Schedule.AttachAsModified(currentSchedule, original);
            }

            // Loop through all children and get them into the correct state.
            foreach (ScheduleRecipient recipient in this.ChangeSet.GetAssociatedChanges(currentSchedule, s => s.ScheduleRecipient))
            {
                ChangeOperation change = this.ChangeSet.GetChangeOperation(recipient);

                switch (change)
                {
                    case ChangeOperation.Delete:
                        if (recipient.EntityState == EntityState.Detached)
                        {
                            this.ObjectContext.ScheduleRecipient.Attach(recipient);
                        }
                        this.ObjectContext.DeleteObject(recipient);
                        break;
                    case ChangeOperation.Insert:
                        // This is already done for us.
                        recipient.CreatedDate = DateTime.Now;
                        break;
                    case ChangeOperation.None:
                        this.ObjectContext.ObjectStateManager.ChangeObjectState(recipient, EntityState.Unchanged);
                        break;
                    case ChangeOperation.Update:
                        this.ObjectContext.ScheduleRecipient.AttachAsModified(recipient, this.ChangeSet.GetOriginal(recipient));
                        break;
                    default:
                        break;
                }
            }*/
        }

        public void DeleteSchedule(Schedule schedule)
        {
            /*deleteScheduleFromDatabase(schedule);
            
            // TODO: we should really open a trans, delete the schedule, and rollback the trans if deleting the subscription fails
            SubscriptionManager subscriptionManager = new SubscriptionManager();
            subscriptionManager.DeleteSubscription(schedule.SubscriptionID);*/
        }

        [Invoke]
        public DateTime GetServerDateTime()
        {
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), cstZone);
            return cstTime;
        }

        [Invoke]
        public bool GetUserHasSSNAccess(string userName)
        {
            FilterService service = new FilterService();
            return service.GetUserHasSSNAccess(userName);
        }

        public IQueryable<CcmsiUser> GetCcmsiUsersBySchedule(int scheduleID)
        {
            // NOTE: 
            // Rather than executing a SP that joins the ReportProjectDB.ScheduleRecipient and the Store1.userlogin table to find the user info,
            //  pass a list of user IDs and execute a SP only against the Store1.userlogin table.  This is done because in the dev environment
            //  the ReportProjectDB and Store1 databases are on different servers, so a SP joining them won't work.

            // build a list of the user IDs of this schedules recipients 
            Schedule schedule = GetSchedule(scheduleID);
            var userIDs = 
                from s in ObjectContext.ScheduleRecipient 
                where s.ScheduleID == scheduleID && s.IsActive
                select s.UserID;

            // delegate to the filter service which is set up to execute SPs against the Store1 DB
            FilterService filterService = new FilterService();
            return filterService.GetCcmsiUsers(userIDs.ToList()).AsQueryable();
        }

        public CcmsiUser GetCcmsiUser(int userID)
        {
            // delegate to the filter service which is set up to execute SPs against the Store1 DB
            FilterService filterService = new FilterService();
            return filterService.GetCcmsiUser(userID);
        }

        /// <summary>
        /// When the user changes the report layout style for an existing user report, any schedules in RS for that user report 
        /// have to be "moved" from the old report layout style to the new report layout style
        /// </summary>
        /// <param name="userReport"></param>
        internal void RelocateSchedulesForUserReport(int userReportID)
        {
            /*ReportProjectDBEntities entities = new ReportProjectDBEntities();
            var scheduleList = from s in entities.Schedule
                               .Include("ScheduleRecipient")
                               where s.UserReportID == userReportID
                               select s;

            // create new subscriptions under new report layout style and delete the old ones
            SubscriptionManager subscriptionMgr = new SubscriptionManager();
            if (scheduleList.Count() > 0)
            {
                foreach (Schedule schedule in scheduleList)
                {
                    string oldSubscriptionID = schedule.SubscriptionID;
                    string newSubscriptionID = subscriptionMgr.CreateSubscription(schedule);
                    schedule.SubscriptionID = newSubscriptionID;
                    subscriptionMgr.DeleteSubscription(oldSubscriptionID);
                }

                entities.SaveChanges();
            }*/
        }

        /// <summary>
        /// For each schedule attached to this user report, this will routine will
        /// update the corresponding subscription in Reporting Services
        /// </summary>
        /// <param name="userReportID"></param>
        internal void SynchronizeSchedulesForUserReport(int userReportID)
        {
            /*ReportProjectDBEntities entities = new ReportProjectDBEntities();
            var scheduleList = from s in entities.Schedule
                               .Include("ScheduleRecipient")
                               where s.UserReportID == userReportID
                               select s;

            SubscriptionManager subscriptionMgr = new SubscriptionManager();
            if (scheduleList.Count() > 0)
            {
                foreach (Schedule schedule in scheduleList)
                {
                    subscriptionMgr.UpdateSubscription(schedule);
                }
            }*/
        }
        
        #endregion

        protected override bool PersistChangeSet()
        {
            // store a list of the schedules that are being created
            /*List<Schedule> newSchedules = new List<Schedule>();
            foreach(ChangeSetEntry entry in this.ChangeSet.ChangeSetEntries)
            {
                if (entry.Entity.GetType() == typeof(Schedule))
                {
                    ChangeOperation change = this.ChangeSet.GetChangeOperation(entry.Entity);
                    if (change == ChangeOperation.Insert)
                    {
                        newSchedules.Add((Schedule)entry.Entity);
                    }
                }
            }*/

            // persist the changeset in the normal way
            bool result = base.PersistChangeSet();

            // create a reporting services subscription for each new schedule
            //  --if this fails, delete the schedule otherwise update the schedule with the RS subscription ID
           /* foreach (Schedule newSchedule in newSchedules)
            {
                try
                {
                    SubscriptionManager subscriptionManager = new SubscriptionManager();
                    newSchedule.SubscriptionID = subscriptionManager.CreateSubscription(newSchedule);
                }
                catch
                {
                    deleteScheduleFromDatabase(newSchedule);
                    throw;
                }
                finally
                {
                    ObjectContext.SaveChanges();
                }
            }*/


            return result;
        }

        private bool hasScheduleDeliveryLog(Schedule schedule)
        {
            var hasDeliveryLogCount = this.ObjectContext.ReportDeliveryLog.Where(rd => rd.ScheduleID == schedule.ScheduleID).Count();

            if (hasDeliveryLogCount > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void deleteScheduleFromDatabase(Schedule schedule)
        {
            if ((schedule.EntityState == EntityState.Detached))
            {
                this.ObjectContext.Schedule.Attach(schedule);
            }

            foreach (ScheduleRecipient recipient in this.ChangeSet.GetAssociatedChanges(schedule, s => s.ScheduleRecipient))
            {
                if (recipient.EntityState == EntityState.Detached)
                {
                    this.ObjectContext.ScheduleRecipient.Attach(recipient);
                }
                if (hasScheduleDeliveryLog(schedule))
                {
                    recipient.IsActive = false;
                }
                else
                {
                    this.ObjectContext.DeleteObject(recipient);
                }
            }
            if (hasScheduleDeliveryLog(schedule))
            {
                schedule.IsActive = false;
            }
            else
            {
                this.ObjectContext.Schedule.DeleteObject(schedule);   
            }            
        }

        public IQueryable<ReportDeliveryLogObj> GetReportDeliveryLog(int userId, string assNum, int currentPage)
        {
            List<ReportDeliveryLogObj> deliveryLogObjs = new List<ReportDeliveryLogObj>();

            var reportDeliveryLogs = (from rd in this.ObjectContext.ReportDeliveryLog
                                      join sr in this.ObjectContext.ScheduleRecipient on rd.ScheduleRecipientID equals sr.ScheduleRecipientID into sr_join
                                      from sr in sr_join.DefaultIfEmpty()
                            join s in this.ObjectContext.Schedule on rd.ScheduleID equals s.ScheduleID
                            join ur in this.ObjectContext.UserReport on s.UserReportID equals ur.UserReportID
                            join r in this.ObjectContext.Report on ur.ReportID equals r.ReportID
                            join uro in this.ObjectContext.UserReportOutput on new { UserReportOutputID = (Int32)rd.UserReportOutputID } equals new { UserReportOutputID = uro.UserReportOutputID } into uro_join
                            from uro in uro_join.DefaultIfEmpty()
                            where rd.Schedule.UserReport.CreatedByUserID == userId && rd.Schedule.UserReport.ClientNumber == assNum
                            orderby
                              rd.DeliveryDate descending
                            select new {
                              rd.ReportDeliveryLogID,
                              rd.DeliveryDate,
                              ScheduleID = rd.ScheduleID,
                              RecipientID = (Int32?)sr.UserID,
                              ReportName = ur.UserReportName,
                              ReportType = r.ReportName,
                              ReportLink = (ur.RowsCount > 0 || (ur.RowsCount == 0 && !s.IsNotSendWithNoData)) ? (uro.FileName ?? "") : "",                              
                              DeliveryMethods = sr != null ? (sr.DeliveryMethodTypeID == 1 ? "Email" : "Application") : "Email",
                              IsActive = (sr.IsActive == null ? true : sr.IsActive) 
                            }).ToList().Select(x => new
                            {
                                ReportDeliveryLogID = x.ReportDeliveryLogID,
                                DeliveryDate = x.DeliveryDate.AddSeconds(-(x.DeliveryDate.Second)).AddMilliseconds(-(x.DeliveryDate.Millisecond)),
                                ScheduleID = x.ScheduleID,
                                RecipientID = x.RecipientID,
                                ReportName = x.ReportName,
                                ReportType = x.ReportName,
                                ReportLink = x.ReportLink,
                                DeliveryMethods = x.DeliveryMethods,
                                IsActive = x.IsActive
                            }).ToList(); ;

            List<int> scheduleIds = new List<int>();
            foreach(var item in reportDeliveryLogs)
            {
                if (!scheduleIds.Contains(item.ScheduleID))
                {
                    scheduleIds.Add(item.ScheduleID);
                }
            }

            var schedulesList = (from s in this.ObjectContext.Schedule
                                 join sr in this.ObjectContext.ScheduleRecipient on s.ScheduleID equals sr.ScheduleID into sr_join
                                 from sr in sr_join.DefaultIfEmpty()
                             where scheduleIds.Contains(s.ScheduleID) && s.CreatedByUserID == userId
                             select new
                                        {
                                            ScheduleObj = s,
                                            ScheduleID = s.ScheduleID,
                                            Subject = s.Subject,
                                            RecurrenceTypeID = s.RecurrenceTypeID,
                                            ScheduleStart = s.ScheduleStart,
                                            ScheduleStop = s.ScheduleStop,
                                            UserID = (Int32?)sr.UserID,
                                            AdditionalEmailAddresses = s.AdditionalEmailAddresses,
                                            DeliveryMethods = sr != null ? (sr.DeliveryMethodTypeID == 1 ? "Email" : "Application") : "Email",
                                            DeliveryMethodTypeID = sr != null ? sr.DeliveryMethodTypeID : 1
                                        }).ToList();
            
            List<int> userIds = new List<int>(); 
            foreach (var reportDeliveryLog in reportDeliveryLogs)
            {
                if (reportDeliveryLog.RecipientID.HasValue && !userIds.Contains(reportDeliveryLog.RecipientID.Value))
                {
                    userIds.Add(reportDeliveryLog.RecipientID.Value);
                }
            }

            foreach (var schedule in schedulesList)
            {
                if (schedule.UserID.HasValue && !userIds.Contains(schedule.UserID.Value))
                {
                    userIds.Add(schedule.UserID.Value);
                }
            }

            var previewReportDeliveryLog = reportDeliveryLogs.Count > 0 ? reportDeliveryLogs[0] : null; 

            FilterService filterService = new FilterService();
            List<CcmsiUser> users = filterService.GetCcmsiUsers(userIds);
            int f = 1;

            var hasReportDeliveryLogs = reportDeliveryLogs.Select(rd => new
                                                                    {
                                                                        DeliveryDate = rd.DeliveryDate,
                                                                        ScheduleID = rd.ScheduleID,
                                                                        IsActive = rd.IsActive,
                                                                        DeliveryMethods = rd.DeliveryMethods
                                                                    }
                                                                    ).Distinct().ToList();


            int totalCount = hasReportDeliveryLogs.Count();

            hasReportDeliveryLogs = hasReportDeliveryLogs.Skip((currentPage - 1) * 10).Take(10).ToList();
            
            var reportDeliveryLogsFilter = (from rd in reportDeliveryLogs
                                            join hrd in hasReportDeliveryLogs on rd.ScheduleID equals hrd.ScheduleID
                                            where rd.DeliveryDate == hrd.DeliveryDate && rd.IsActive == hrd.IsActive && rd.DeliveryMethods == hrd.DeliveryMethods
                                            select rd
                                            ).ToList();

            foreach (var reportDeliveryLog in reportDeliveryLogsFilter)
            {
                List<ReportDeliveryScheduleObj> reportDeliveryScheduleObjs = new List<ReportDeliveryScheduleObj>();
                string additionalEmails = "";
                var schedules = schedulesList.Where(sl => sl.ScheduleID == reportDeliveryLog.ScheduleID).ToList();
                if (schedules != null)
                {
                    var list = schedules;

                        if (list != null && list.Count != 0)
                        {
                            bool isFirst = true;
                            ReportDeliveryScheduleObj rDSObj = new ReportDeliveryScheduleObj();
                            string recipients = "";
                            foreach (var s in list)
                            {
                                if (isFirst)
                                {
                                    rDSObj.DeliveryMethods = s.DeliveryMethods;
                                    rDSObj.Period = ((Schedule) s.ScheduleObj).RecurrenceDescription;
                                    rDSObj.ScheduleStop = s.ScheduleStop != null && s.ScheduleStop.HasValue? s.ScheduleStop.Value.ToShortDateString() : string.Empty;
                                    rDSObj.ScheduleStart = s.ScheduleStart.Date.ToShortDateString();
                                    rDSObj.TimeOfDay = reportDeliveryLog.DeliveryDate.TimeOfDay.ToString();
                                    rDSObj.ScheduleName = s.Subject;
                                    if (!string.IsNullOrEmpty(s.AdditionalEmailAddresses))
                                    {
                                        string[] emails = s.AdditionalEmailAddresses.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                                        foreach (var email in emails)
                                        {
                                            additionalEmails += string.IsNullOrEmpty(additionalEmails) ? email : "; " + email;
                                        }
                                    }
                                    isFirst = false;
                                }
                            }

                            var hasIndivudualRecipients = reportDeliveryLogs.Where(sc => (sc.DeliveryDate != reportDeliveryLog.DeliveryDate || sc.IsActive != reportDeliveryLog.IsActive || sc.DeliveryMethods != reportDeliveryLog.DeliveryMethods) && sc.ScheduleID == reportDeliveryLog.ScheduleID);
                            var hasRecipients = reportDeliveryLogs.Where(sc => sc.DeliveryDate == reportDeliveryLog.DeliveryDate && sc.IsActive == reportDeliveryLog.IsActive && sc.DeliveryMethods == reportDeliveryLog.DeliveryMethods && sc.ScheduleID == reportDeliveryLog.ScheduleID);

                            if (hasIndivudualRecipients.Count() > 0 && hasRecipients.Count() == 1)
                            {
                                rDSObj.Recipients = reportDeliveryLog.RecipientID.HasValue ? users.Where(u => u.ID == reportDeliveryLog.RecipientID).FirstOrDefault().FullName : additionalEmails;
                                rDSObj.DeliveryMethods = reportDeliveryLog.DeliveryMethods;
                            }
                            else
                            {
                                foreach (var s in hasRecipients)
                                {
                                    recipients += s.RecipientID.HasValue ? users.Where(u => u.ID == s.RecipientID).FirstOrDefault().FullName + "; " : "";
                                }
                                rDSObj.Recipients = recipients;
                                rDSObj.DeliveryMethods = reportDeliveryLog.DeliveryMethods;
                            }  
                            reportDeliveryScheduleObjs.Add(rDSObj);
                        }
                }

                string[] additionalEmail = additionalEmails.Split(new string[]{";"}, StringSplitOptions.RemoveEmptyEntries);
                if (additionalEmail.Length > 1)
                {
                    additionalEmails = additionalEmail[0];
                }

                if (f == 1 || ((previewReportDeliveryLog.ScheduleID != reportDeliveryLog.ScheduleID) || ((previewReportDeliveryLog.DeliveryDate != reportDeliveryLog.DeliveryDate || previewReportDeliveryLog.IsActive != reportDeliveryLog.IsActive || previewReportDeliveryLog.DeliveryMethods != reportDeliveryLog.DeliveryMethods) && previewReportDeliveryLog.ScheduleID == reportDeliveryLog.ScheduleID)))
                {
                    deliveryLogObjs.Add(new ReportDeliveryLogObj
                                            {
                                                 DeliveryDate =  reportDeliveryLog.DeliveryDate.ToShortDateString(),
                                                 ReportDeliveryLogID = reportDeliveryLog.ReportDeliveryLogID,
                                                 ReportName = reportDeliveryLog.ReportName,
                                                 ReportType = reportDeliveryLog.ReportType,
                                                 DeliveryMethods = reportDeliveryLog.DeliveryMethods,
                                                 Recipient = reportDeliveryLog.RecipientID.HasValue ? users.Where(u => u.ID == reportDeliveryLog.RecipientID).FirstOrDefault().FullName : additionalEmails,
                                                 ReportLink = !string.IsNullOrEmpty(reportDeliveryLog.ReportLink) ? string.Format("Folder={0}&File={1}", users.Where(u => u.ID == reportDeliveryLog.RecipientID).FirstOrDefault().UserID, reportDeliveryLog.ReportLink) : "",
                                                 TotalCount = totalCount,
                                                 Schedules = reportDeliveryScheduleObjs
                                            });

                    previewReportDeliveryLog = reportDeliveryLog;
                    f++;
                }
            }

            return deliveryLogObjs.AsQueryable();
        }

        public IQueryable<UserReleaseNoteVersion> GetUserReleaseNote(int userId)
        {
            List<UserReleaseNoteVersion> userReleaseNoteVersionList = new List<UserReleaseNoteVersion>();

            var userReleaseNoteList = (from urn in this.ObjectContext.UserReleaseNote
                                       join rn in this.ObjectContext.ReleaseNote on urn.ReleaseNoteID equals rn.ReleaseNoteID
                                       where urn.UserID == userId

                                       select new
                                         {
                                             ReleaseNoteID = rn.ReleaseNoteID,
                                             ReleaseNoteDate = rn.ReleaseDate,
                                             ReleaseNoteNumber = rn.ReleaseNumber,
                                             ReleaseNoteTitle = rn.ReleaseTitle,
                                             IsReleaseNoteRead = urn.IsReleaseNoteRead,
                                             DocumentLink = rn.DOCUMENT
                                         }).ToList();
            
            foreach (var releaseNote in userReleaseNoteList)
            {
                userReleaseNoteVersionList.Add(new UserReleaseNoteVersion
                {
                    ReleaseNoteID = releaseNote.ReleaseNoteID,
                    ReleaseNoteDate = (DateTime)(releaseNote.ReleaseNoteDate),
                    ReleaseNoteNumber = releaseNote.ReleaseNoteNumber,
                    ReleaseNoteTitle =releaseNote.ReleaseNoteTitle,
                    IsReleaseNoteRead =releaseNote.IsReleaseNoteRead,
                    DocumentLink = !string.IsNullOrEmpty(releaseNote.DocumentLink) ? string.Format("Folder={0}&File={1}", ConfigurationManager.AppSettings["ReleaseNoteFolder"], releaseNote.DocumentLink) : string.Empty
                });

            }
            return userReleaseNoteVersionList.AsQueryable();
        }
        
        //public IQueryable<UserReleaseNoteVersion> IsExistNewReleaseNote(int userId)
        //{
        //    List<UserReleaseNoteVersion> userReleaseNoteVersionList = new List<UserReleaseNoteVersion>();

        //    var userReleaseNoteList = (from urn in this.ObjectContext.UserReleaseNote
        //                               join rn in this.ObjectContext.ReleaseNote on urn.ReleaseNoteID equals rn.ReleaseNoteID
        //                               orderby urn.CreatedDate descending
        //                               where urn.UserID == userId
        //                               select new
        //                               {
        //                                   ReleaseNoteID = rn.ReleaseNoteID,
        //                                   ReleaseNoteDate = rn.ReleaseDate,
        //                                   ReleaseNoteNumber = rn.ReleaseNumber,
        //                                   ReleaseNoteTitle = rn.ReleaseTitle,
        //                                   DocumentLink = rn.DOCUMENT,
        //                                   IsReleaseNoteNotificationViewed = urn.IsReleaseNoteNotificationViewed,
        //                                   IsReleaseNoteRead = urn.IsReleaseNoteRead
        //                               }).Take(1).ToList();

        //    foreach (var releaseNote in userReleaseNoteList)
        //    {
        //        if (!releaseNote.IsReleaseNoteRead && !releaseNote.IsReleaseNoteNotificationViewed)
        //        {
        //            userReleaseNoteVersionList.Add(new UserReleaseNoteVersion
        //            {
        //                ReleaseNoteID = releaseNote.ReleaseNoteID,
        //                ReleaseNoteDate = (DateTime)(releaseNote.ReleaseNoteDate),
        //                ReleaseNoteNumber = releaseNote.ReleaseNoteNumber,
        //                ReleaseNoteTitle = releaseNote.ReleaseNoteTitle,
        //                IsReleaseNoteRead = releaseNote.IsReleaseNoteRead,
        //                DocumentLink = !string.IsNullOrEmpty(releaseNote.DocumentLink) ? string.Format("Folder={0}&File={1}", ConfigurationManager.AppSettings["ReleaseNoteFolder"], releaseNote.DocumentLink) : string.Empty
        //            });
        //        }

        //    }
        //    return userReleaseNoteVersionList.AsQueryable();
        //}

        [Query(IsComposable = false)]
        public UserReleaseNoteVersion MarkReleaseNoteAsRead(int releaseNoteID, int userID)
        {
            var entities = new ReportProjectDBEntities();
            var userReleaseNote = entities.UserReleaseNote.First(urn => urn.ReleaseNoteID == releaseNoteID && urn.UserID == userID);
            userReleaseNote.IsReleaseNoteRead = true;

            entities.SaveChanges();

            var releaseNote = (from urn in ObjectContext.UserReleaseNote
                               join rn in ObjectContext.ReleaseNote on urn.ReleaseNoteID equals rn.ReleaseNoteID
                               where urn.UserID == userReleaseNote.UserID && urn.ReleaseNoteID == userReleaseNote.ReleaseNoteID

                               select new 
                                   {
                                       ReleaseNoteID = rn.ReleaseNoteID,
                                       ReleaseNoteDate = rn.CreatedDate,
                                       ReleaseNoteNumber = rn.ReleaseNumber,
                                       ReleaseNoteTitle = rn.ReleaseTitle,                                               
                                       DocumentLink = rn.DOCUMENT
                                   }).First();

            return new UserReleaseNoteVersion
                       {
                           ReleaseNoteID = releaseNote.ReleaseNoteID,
                           ReleaseNoteDate = releaseNote.ReleaseNoteDate,
                           ReleaseNoteNumber = releaseNote.ReleaseNoteNumber,
                           ReleaseNoteTitle = releaseNote.ReleaseNoteTitle,
                           IsReleaseNoteRead = true,
                           DocumentLink = !string.IsNullOrEmpty(releaseNote.DocumentLink) ? string.Format("Folder={0}&File={1}", ConfigurationManager.AppSettings["ReleaseNoteFolder"], releaseNote.DocumentLink) : string.Empty
                       };
        }


        //[Query(IsComposable = false)]
        //public UserReleaseNoteVersion MarkReleaseNoteAsNotificationView(int releaseNoteID, int userID, bool isRead)
        //{
        //    var entities = new ReportProjectDBEntities();
        //    var userReleaseNote = entities.UserReleaseNote.First(urn => urn.ReleaseNoteID == releaseNoteID && urn.UserID == userID);
        //    userReleaseNote.IsReleaseNoteRead = isRead;
        //    userReleaseNote.IsReleaseNoteNotificationViewed = true;

        //    entities.SaveChanges();

        //    var releaseNote = (from urn in ObjectContext.UserReleaseNote
        //                       join rn in ObjectContext.ReleaseNote on urn.ReleaseNoteID equals rn.ReleaseNoteID
        //                       where urn.UserID == userReleaseNote.UserID && urn.ReleaseNoteID == userReleaseNote.ReleaseNoteID

        //                       select new
        //                       {
        //                           ReleaseNoteID = rn.ReleaseNoteID,
        //                           ReleaseNoteDate = rn.CreatedDate,
        //                           ReleaseNoteNumber = rn.ReleaseNumber,
        //                           ReleaseNoteTitle = rn.ReleaseTitle,
        //                           DocumentLink = rn.DOCUMENT
        //                       }).First();

        //    return new UserReleaseNoteVersion
        //    {
        //        ReleaseNoteID = releaseNote.ReleaseNoteID,
        //        ReleaseNoteDate = releaseNote.ReleaseNoteDate,
        //        ReleaseNoteNumber = releaseNote.ReleaseNoteNumber,
        //        ReleaseNoteTitle = releaseNote.ReleaseNoteTitle,
        //        IsReleaseNoteRead = isRead,
        //        IsReleaseNoteNotificationViewed = true,
        //        DocumentLink = !string.IsNullOrEmpty(releaseNote.DocumentLink) ? string.Format("Folder={0}&File={1}", ConfigurationManager.AppSettings["ReleaseNoteFolder"], releaseNote.DocumentLink) : string.Empty
        //    };
        //}

        public Dictionary<string, object> GetAllData(int reportID, int userReportID, string userName, string assNum, int userID)
        {
            Dictionary<string, object> resultDictionary = new Dictionary<string, object>();
            AuthenticationService authenticationService = new AuthenticationService();
            //UserAssociation association = authenticationService.GetUserAssociations().FirstOrDefault(ua => ua.AssnNum.ToLower() == assNum.ToLower());
            List<string> userCheckpoints = authenticationService.GetUserCheckpoints(userName).CheckpointIds;
            FilterService filterService = new FilterService();

            List<CheckpointObj> listCheckpoints = this.GetUserCheckpoints(userName).ToList();
            
            Rpt rpt = this.GetReportById(reportID);
            UserRpt userRpt = this.GetUserReportById(userID, assNum, userName, userReportID, userCheckpoints);
            resultDictionary.Add("UserReportSummarizeFields", this.GetUserReportSummarizeFields(userReportID).ToList());

            bool isNewReport = userReportID == -1;
            bool isCustomReport = userRpt != null && userRpt.IsCustom;

            resultDictionary.Add("UserID", userName);
            resultDictionary.Add("AssociationNumber", assNum);
            resultDictionary.Add("AssociationName", /*association.AssnLongName*/"Test");
            resultDictionary.Add("UserReportName", userRpt != null ? userRpt.UserReportName : "");
            resultDictionary.Add("ReportName", rpt.Name);
            resultDictionary.Add("ReportLayoutStyleID", userRpt != null ? userRpt.ReportLayoutStyleID : 0);
            resultDictionary.Add("FilterSystemType", rpt.FilterSystemType);
            resultDictionary.Add("Format", userRpt != null ? (int)userRpt.FormatType : (int)FormatTypeEnum.PDF);
            resultDictionary.Add("IsNew", isNewReport);
            resultDictionary.Add("IsCustom", isCustomReport);
            resultDictionary.Add("SummaryOnly", userRpt != null && userRpt.IsSummaryOnly);
            resultDictionary.Add("IsTurnOffPageBreak", userRpt != null && userRpt.IsTurnOffPageBreak);
            resultDictionary.Add("IncludeTitlePage", userRpt == null || userRpt.IncludeTitlePage);
            resultDictionary.Add("HasSSNAccess",
                                 (reportID == (int)ReportEnum.CheckRegister ||
                                  reportID == (int)ReportEnum.TransactionRegister ||
                                  reportID == (int)ReportEnum.ManagedCareBillsAndChargesByProvider) &&
                                 filterService.GetUserHasSSNAccess(userName));
            resultDictionary.Add("IsExcelFormatEnabled", !((reportID == (int)ReportEnum.LitigationManagementDetail) || (reportID == (int)ReportEnum.MadicareQueryFucntion)));
            resultDictionary.Add("IsPDFFormatEnabled", reportID != (int)ReportEnum.LossRunSummaryWithCurrentPaid);
            resultDictionary.Add("UserCheckpoints", listCheckpoints);
            resultDictionary.Add("ClientSpecificFields", filterService.GetClientSpecificFields(assNum));
            resultDictionary.Add("UserReportNamesList", this.GetUserReportNames(userID, assNum, userName, userCheckpoints));
            HierarchyLabels hierarchyLabels = filterService.GetHierarchyLabels(userName, assNum);
            resultDictionary.Add("HierarchyLabels", hierarchyLabels);

            List<RptLayoutStyle> layoutStyles = this.GetReportLayoutStyles(reportID).ToList();
            List<RptLayoutStyle> layoutStyleList = new List<RptLayoutStyle>();
            foreach (RptLayoutStyle layoutStyle in layoutStyles)
            {
                string label = EnsureClientSpecificHierarchyName(layoutStyle.Name, hierarchyLabels);
                if (!string.IsNullOrEmpty(label))
                {
                    layoutStyle.Name = label;
                    layoutStyleList.Add(layoutStyle);
                }
            }
            resultDictionary.Add("ReportLayoutStyles", layoutStyleList);
            List<RptParameter> rptParameters = this.GetUserReportParameters(userReportID, reportID).ToList();
            resultDictionary.Add("FilterData", (isNewReport) ? new List<FilterData>() : rptParameters.Select(rptParameter => new FilterData { FilterName = rptParameter.Name, Value = rptParameter.DefaultValue, FilterString = rptParameter.FilterString, FilterType = (ReportBuilderUILib.Filter.DataTypeEnum)rptParameter.DataType}).ToList());
            resultDictionary.Add("UserReportParameters", rptParameters);
            resultDictionary.Add("UserReportFields", this.GetReportFields(reportID, assNum).ToList());

            List<RptField> listRptFields = this.GetUserReportFields(userReportID, reportID, assNum).ToList();
            List<RptFieldInfo> userReportFieldList = new List<RptFieldInfo>();
            foreach (RptField rptField in listRptFields)
            {
                string hierarchyLabel = GetHierarchyLabel(rptField.SQLName, hierarchyLabels);

                int columnOrder = (isNewReport ? rptField.ColumnOrder : (rptField.RptFieldID == rptField.ReportFieldID && isCustomReport ? -1 : rptField.ColumnOrder));

                userReportFieldList.Add(new RptFieldInfo
                {
                    RptFieldInfoID = rptField.RptFieldID,
                    ID = rptField.ID,
                    ReportFieldID = rptField.ReportFieldID,
                    Name = string.IsNullOrEmpty(hierarchyLabel) ? rptField.Name : (rptField.ID == rptField.ReportFieldID ? hierarchyLabel : (isCustomReport ? rptField.Name : hierarchyLabel)),
                    CanDrag = true,
                    Tag = columnOrder,
                    GroupOrder = rptField.GroupOrder,
                    GroupSummaryExpression = rptField.GroupSummaryExpression,
                    ColumnOrder = rptField.ColumnOrder,
                    SortOrder = rptField.SortOrder,
                    SortDirection = (int)rptField.SortDirection,
                    ReportID = rptField.ReportID,
                    SQLName = rptField.SQLName,
                    DataType = (int)rptField.DataType,
                    ColumnWidthFactor = rptField.ColumnWidthFactor,
                    IsGroupable = rptField.IsGroupable,
                    IncludePageBreak = rptField.IncludePageBreak,
                    IsDisplayInReport = rptField.IsDisplayInReport,
                    IsGroupByDefault = rptField.IsGroupByDefault,
                    IsSummarizable = rptField.IsSummarizable,
                    IsUsed = (columnOrder > 0 ? true : false),
                    IsVisible = GetHierarchyLabelIsVisible(rptField.SQLName, hierarchyLabels),
                    Category = rptField.Category,
                    FieldValueExpression = rptField.FieldValueExpression,
                    IsCalculated = rptField.IsCalculated,
                    CalculationDescription = rptField.CalculationDescription,
                    UseSummaryExpresionGroup = rptField.UseSummaryExpresionGroup,
                    CoverageCode = rptField.CoverageCode,
                    IsClientSpecificCode = rptField.IsClientSpecificCode
                });
            }

            resultDictionary.Add("ReportFields", userReportFieldList);
            resultDictionary.Add("UserReportSumFieldsFields", GetUserReportSummarizeFields(userReportID));
            return resultDictionary;
        }

        public Dictionary<string, object> GetAllDataForGrid(string userName, string assNum, int userID)
        {
            Dictionary<string, object> resultDictionary = new Dictionary<string, object>();
            List<UserRpt> listReports = this.GetUserReports(userID, assNum, userName).ToList();
            resultDictionary.Add("ListReports", listReports);
            resultDictionary.Add("ReportFolders", GetReportFolders().ToList());
            resultDictionary.Add("Reports", GetReports(userName, false).ToList());
            
            return resultDictionary;
        }

        public Dictionary<string, object> GetSchedules(string userName, string assNum, int userID, int userReportID)
        {
            Dictionary<string, object> resultDictionary = new Dictionary<string, object>();
            List<Dictionary<string,object>> listSchedules = new List<Dictionary<string, object>>();
            foreach (Schedule schedule in this.GetSchedules(userReportID))
            {
                listSchedules.Add(new Dictionary<string, object>
                {
                    {"ScheduleID", schedule.ScheduleID},
                    {"UserReportID", schedule.UserReportID},
                    {"RecurrenceDescription", schedule.RecurrenceDescription},
                    {"Time", schedule.ScheduleStart.ToString("h:mm tt")},
                    {"ScheduleStart", schedule.ScheduleStart.ToString("MM/dd/yyyy")},
                    {"ScheduleStop", schedule.ScheduleStop == null ? string.Empty :  Convert.ToDateTime(schedule.ScheduleStop).ToString("MM/dd/yyyy")}
                });
            }
            resultDictionary.Add("listSchedules", listSchedules);
            resultDictionary.Add("userReportID", userReportID);
            return resultDictionary;
        }
        

	    #region Hierarchy

        private string GetHierarchyLabel(string sqlFieldName, HierarchyLabels hierarchyLabels)
        {
            string label = string.Empty;

            sqlFieldName = (string.IsNullOrEmpty(sqlFieldName)) ? string.Empty : sqlFieldName;

            switch (sqlFieldName.ToLower())
            {
                case "member":
                case "membername":
                case "member_name":
                    label = CapitalizeString(hierarchyLabels.MemberLabel); break;
                case "location":
                case "locationname":
                    label = CapitalizeString(hierarchyLabels.LocationLabel); break;
                case "group1code":
                case "group1codename":
                    label = CapitalizeString(hierarchyLabels.Group1Label); break;
                case "group2code":
                case "group2codename":
                    label = CapitalizeString(hierarchyLabels.Group2Label); break;
                case "group3code":
                case "group3codename":
                    label = CapitalizeString(hierarchyLabels.Group3Label); break;
                case "group4code":
                case "group4codename":
                    label = CapitalizeString(hierarchyLabels.Group4Label); break;
                case "specialanalysis1": label = CapitalizeString(hierarchyLabels.SpecialAnalysis1Label); break;
                case "specialanalysis2": label = CapitalizeString(hierarchyLabels.SpecialAnalysis2Label); break;
                case "specialanalysis3": label = CapitalizeString(hierarchyLabels.SpecialAnalysis3Label); break;
                case "specialanalysis4": label = CapitalizeString(hierarchyLabels.SpecialAnalysis4Label); break;
                case "specialanalysis5": label = CapitalizeString(hierarchyLabels.SpecialAnalysis5Label); break;
                case "membernumber":
                    label = CapitalizeString(hierarchyLabels.MemberLabel) + " #"; break;
                case "locationnumber":
                    label = CapitalizeString(hierarchyLabels.LocationLabel) + " #"; break;
                default: label = string.Empty; break;
            }
            return label;
        }

        private bool GetHierarchyLabelIsVisible(string sqlFieldName, HierarchyLabels hierarchyLabels)
        {
            var isVisible = true;

            sqlFieldName = (string.IsNullOrEmpty(sqlFieldName)) ? string.Empty : sqlFieldName;

            switch (sqlFieldName.ToLower())
            {
                case "member":
                case "membername":
                case "member_name":
                    isVisible = hierarchyLabels.MemberLabelVisible; break;
                case "location":
                case "locationname": isVisible = hierarchyLabels.LocationLabelVisible; break;
                case "group1code":
                case "group1codename":
                    isVisible = hierarchyLabels.Group1LabelVisible; break;
                case "group2code":
                case "group2codename":
                    isVisible = hierarchyLabels.Group2LabelVisible; break;
                case "group3code":
                case "group3codename":
                    isVisible = hierarchyLabels.Group3LabelVisible; break;
                case "group4code":
                case "group4codename":
                    isVisible = hierarchyLabels.Group4LabelVisible; break;
                case "specialanalysis1": isVisible = hierarchyLabels.SpecialAnalysis1LabelVisible; break;
                case "specialanalysis2": isVisible = hierarchyLabels.SpecialAnalysis2LabelVisible; break;
                case "specialanalysis3": isVisible = hierarchyLabels.SpecialAnalysis3LabelVisible; break;
                case "specialanalysis4": isVisible = hierarchyLabels.SpecialAnalysis4LabelVisible; break;
                case "specialanalysis5": isVisible = hierarchyLabels.SpecialAnalysis5LabelVisible; break;
            }

            return isVisible;
        }

        private string EnsureClientSpecificHierarchyName(string layoutStyleName, HierarchyLabels hierarchyLabels)
        {
            string resultString = layoutStyleName;

            if (resultString.Contains("Location"))
            {
                if (hierarchyLabels.LocationLabelVisible)
                {
                    resultString = resultString.Replace("Location", CapitalizeString(hierarchyLabels.LocationLabel));
                }
                else
                {
                    return null;
                }
            }

            if (resultString.Contains("Member"))
            {
                if (hierarchyLabels.MemberLabelVisible)
                {
                    resultString = resultString.Replace("Member", CapitalizeString(hierarchyLabels.MemberLabel));
                }
                else
                {
                    return null;
                }
            }

            if (resultString.Contains("Group Code 1"))
            {
                if (hierarchyLabels.Group1LabelVisible)
                {
                    resultString = resultString.Replace("Group Code 1", CapitalizeString(hierarchyLabels.Group1Label));
                }
                else
                {
                    return null;
                }
            }
            if (layoutStyleName.Contains("Group Code 2"))
            {
                if (hierarchyLabels.Group2LabelVisible)
                {
                    resultString = resultString.Replace("Group Code 2", CapitalizeString(hierarchyLabels.Group2Label));
                }
                else
                {
                    return null;
                }
            }
            if (resultString.Contains("Group Code 3"))
            {
                if (hierarchyLabels.Group3LabelVisible)
                {
                    resultString = resultString.Replace("Group Code 3", CapitalizeString(hierarchyLabels.Group3Label));
                }
                else
                {
                    return null;
                }
            }
            if (resultString.Contains("Group Code 4"))
            {
                if (hierarchyLabels.Group4LabelVisible)
                {
                    resultString = resultString.Replace("Group Code 4", CapitalizeString(hierarchyLabels.Group4Label));
                }
                else
                {
                    return null;
                }
            }
            return resultString;
        }


        private string CapitalizeString(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            string resultString = "";
            string[] values = value.ToLower().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in values)
            {
                resultString += UppercaseFirst(s) + " ";
            }
            return resultString.Trim();
        }

        private string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        #endregion

        public Dictionary<string, object> GetReleaseNotesData(string userName, string assNum, int userID)
        {
            Dictionary<string, object> resultDictionary = new Dictionary<string, object>();
            List<UserReleaseNoteVersion> releaseNotes = this.GetUserReleaseNote(userID).ToList();
            resultDictionary.Add("ReleaseNotes", releaseNotes);
            return resultDictionary;
        }

        public Dictionary<string, object> GetDeliveredReportsData(string userName, string assNum, int userID)
        {
            Dictionary<string, object> resultDictionary = new Dictionary<string, object>();
            resultDictionary.Add("ReportOutputs", GetReportOutputs(userID, assNum).ToList());
            resultDictionary.Add("ReportDeliveryLog", GetReportDeliveryLog(userID, assNum, 1).ToList());
            return resultDictionary;
        }

       public Dictionary<string, object> GetDeliveredReportsDataByPage(string userName, string assNum, int userID, int currentPage)
        {
            Dictionary<string, object> resultDictionary = new Dictionary<string, object>();
            resultDictionary.Add("ReportDeliveryLog", GetReportDeliveryLog(userID, assNum, currentPage).ToList());
            return resultDictionary;
        }

        public Dictionary<string, object> UpdateDocumentStatus(int releaseNoteID, string userName, string assNum, int userID)
        {
            Dictionary<string, object> resultDictionary = new Dictionary<string, object>();
            this.MarkReleaseNoteAsRead(releaseNoteID, userID);
            return resultDictionary;
        }

        public void InsertException(string userId, string exceptionText)
        {
            ApplicationLog exception = new ApplicationLog();
            exception.CreatedDate = DateTime.Now;
            exception.Message = exceptionText;
            exception.Source = "Delivery Ext";
            exception.MessageType = "Error";
            this.ObjectContext.AddToApplicationLog(exception);
            this.ObjectContext.SaveChanges();
        }
    }
}


