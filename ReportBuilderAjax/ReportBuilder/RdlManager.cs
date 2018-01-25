using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.ReportingServices.RdlObjectModel;
using ReportBuilderAjax.Web.HttpHandlers;
using ReportBuilderAjax.Web.Services;
using RDL = Microsoft.ReportingServices.RdlObjectModel;
using RS = ReportBuilderAjax.Web.ReportingService2005;
using RE = ReportBuilderAjax.Web.ReportExecution2005;
using System.Configuration;
using ParameterValue = ReportBuilderAjax.Web.ReportExecution2005.ParameterValue;

namespace ReportBuilderAjax.Web.ReportBuilder
{
    public enum DateRange
    {
        None = 0,
        LastMonth = 1,
        LastQuarter = 2,
        LastYear = 3,
        MonthToDate = 4,
        YearToDate = 5,
        Custom = 6,
        LastCalendarWeek = 7,
        Last2Weeks = 8,
        AllDates = 9,
        LastBusinessWeek = 10,
        LastXDays = 11,
        Yesterday = 12,
        LastWeek = 13,
        PreviousBusinessDay = 14
    }

    enum TrialDateRange
    {
        None = 0,
        Next30Days = 1,
        Next60Days = 2,
        Next90Days = 3,
        Prior30Days = 4,
        Prior60Days = 5,
        Prior90Days = 6,
        Custom = 7,
        AllDates = 8
    }

    enum YesNoNARange
    {
        Yes = 1,
        No = 2,
        NA = 3
    }

    enum AsOfDateType
    {
        LastDayOfLastWeek = 1,
        LastDayOfLastMonth = 2,
        LastDayOfLastQuarter = 3,
        LastDayOfLastYear = 4,
        Custom = 5,
        Yesterday = 6
    }

    enum DateType
    {
        DateOfLoss = 1,
        ClaimEntryDate = 2,
        DateClosed = 3
    }

    enum WorkflowStatus
    {
        CheckPrinted = 1,
        ReleasedToPrint = 2
    }

    enum ClaimsReceivedCalculation
    {
        NewClaimsOnly = 1,
        NewClaimsTransfers = 2,
        NewClaimsReopened = 3,
        NewClaimsTransfersReopened = 4
    }
   
    enum IncludeClosedClaimsEnum
    {
        None = 0,
        All = 1,
        OnlyCurrentMonth = 2
    }

    public enum PeriodType
    {
        CalendarYear = 1,
        FiscalYear = 2
    }

    public enum TimePeriod
    {
        CurrentYear = 1,
        YearsBack1 = 2,
        YearsBack2 = 3,
        YearsBack3 = 4,
        YearsBack4 = 5,
        YearsBack5 = 6,
        Custom = 7

    }

    public class RdlManager
    {
        public const string CULTURE_INFO = "en-US";
        private const string TABLE_NAME = "table1";
        private RS.ReportingService2005 _reportService;         //TODO: necessary only for canceling a report
        private RE.ReportExecutionService _reportExecution;

        public RdlManager()
        {
            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(
                            ConfigurationManager.AppSettings["ReportServerUserID"],
                            ConfigurationManager.AppSettings["ReportServerPassword"],
                            ConfigurationManager.AppSettings["ReportServerDomain"]);

            _reportService = new RS.ReportingService2005();
            _reportService.Url = ConfigurationManager.AppSettings["ReportServerURL"];
            _reportService.Credentials = credentials;
            //_reportService.Credentials = System.Net.CredentialCache.DefaultCredentials;

            _reportExecution = new RE.ReportExecutionService();
            //_reportExecution.Timeout = 60000;             // 1 minute
            //_reportExecution.Timeout = 600000;            // 10 minutes
            //_reportExecution.Timeout = 720000;              // 12 minutes
            _reportExecution.Timeout = 7140000;           // 1 hour, 59 minutes.  To guarantee the channel to the client is still open when the report times out, so it can be informed of the failure--
            //      the client stops listening for a response after 2 hours.
            //_reportExecution.Timeout = 7200000;           // 2 hours
            _reportExecution.Credentials = credentials;
            _reportExecution.Url = ConfigurationManager.AppSettings["ReportExecutionURL"];
            //_reportExecution.Credentials = System.Net.CredentialCache.DefaultCredentials;
        }

        public string GetReportHeader(int reportID, int formatTypeID, int reportLayoutStyleID, int indx)
        {

            var rdlComponentTypeId = (int)(formatTypeID == (int)FormatTypeEnum.Excel && indx >= 0 ? -1 : (int)RdlComponentTypeEnum.Header);

            return getRdlComponent(reportID, formatTypeID, reportLayoutStyleID, rdlComponentTypeId);
        }

        public string GetReportFooter(int reportID, int formatTypeID, int reportLayoutStyleID)
        {
            return getRdlComponent(reportID, formatTypeID, reportLayoutStyleID, (int)RdlComponentTypeEnum.Footer);
        }

        private string getRdlComponent(int reportID, int formatTypeID, int reportLayoutStyleID, int rdlComponentTypeID)
        {
            ReportProjectDBEntities entities = new ReportProjectDBEntities();
            var rdlComponentObj = (from rc in entities.RdlComponent
                                   join rlRdl in entities.ReportLayoutStyleRdl on rc.RdlComponentID equals rlRdl.RdlComponentID
                                   join rl in entities.ReportLayoutStyle on rlRdl.ReportLayoutStyleID equals rl.ReportLayoutStyleID
                                   join ft in entities.FormatType on rlRdl.FormatTypeID equals ft.FormatTypeID
                                   where rc.RdlComponentTypeID == rdlComponentTypeID &&
                                           rl.ReportLayoutStyleID == reportLayoutStyleID && rl.ReportID == reportID && ft.FormatTypeID == formatTypeID
                                   select rc).FirstOrDefault();
            if (rdlComponentObj != null)
            {
                return rdlComponentObj.RdlData;
            }
            return "";
        }

        public void CustomizeReport(UserReport userReport, RDL.Report rdl)
        {
            if (rdl != null)
            {
                List<SubRpt> subReports = GetSubreports(userReport.ReportID);
                List<SubRptParameter> subReportParameters = GetSubreportParameters(userReport.ReportID);

                //Get user report Fields
                List<RptField> fields = new List<RptField>();
                fields = GetUserReportFields(userReport.UserReportID);

                // get the user report sum fields (i.e., the fields for which a total line must be shown in the group footer--these are all the numeric fields)
                List<RptField> summarizeFields = null;
                summarizeFields = GetUserReportSummarizeFields(userReport.UserReportID);

                if (userReport.IsSummaryOnly)
                {
                    UpdateTablixForSummaryOnlyReport(rdl, userReport, fields, summarizeFields, subReports, subReportParameters);
                }
                else
                {
                    UpdateTablix(rdl, userReport, fields, summarizeFields, subReports, subReportParameters);
                }
            }
        }

        #region Render FilterKey       

        public string GetFilterKey(List<RptParameter> parms, List<RptField> fields, string reportLayoutStyleName, bool isCustom, int reportID)
        {
            //_reportLayoutStyleName = reportLayoutStyleName;
            string filterKey = "";
            string splitter = ";";
            FilterService filterService = new FilterService();
            List<RptParameter> orderedList = parms.Where(p => p.OrderIndex > -1).OrderBy(p => p.OrderIndex).ToList();
            HierarchyLabels hierarchyLabels = null;
            RptParameter userIdParameter = parms.Where(ol => ol.Name.ToLower() == "userid").FirstOrDefault();
            RptParameter assnNumParameter = parms.Where(ol => ol.Name.ToLower() == "assnnum").FirstOrDefault();
            CcmsiUser ccmsiUser = filterService.GetCcmsiUser(userIdParameter.DefaultValue);
 
            if (userIdParameter != null && assnNumParameter != null)
            {
                hierarchyLabels = filterService.GetHierarchyLabels(userIdParameter.DefaultValue, assnNumParameter.DefaultValue);
            }

            Services.DateRange dateRange = null;
            RptParameter dateRangeParameter = orderedList.Where(ol => ol.Name.ToLower() == "dateperiod" || ol.Name.ToLower() == "daterange").FirstOrDefault();
            RptParameter startDateParameter = orderedList.Where(ol => ol.Name.ToLower() == "startdate").FirstOrDefault();
            RptParameter endDateParameter = orderedList.Where(ol => ol.Name.ToLower() == "enddate").FirstOrDefault();
            RptParameter lastXDays = orderedList.Where(ol => ol.Name.ToLower() == "lastxdays").FirstOrDefault();
            if (dateRangeParameter != null && startDateParameter != null && endDateParameter != null)
            {
                if (Convert.ToInt32(dateRangeParameter.DefaultValue) != (int)DateRange.Custom && Convert.ToInt32(dateRangeParameter.DefaultValue) != (int)DateRange.None)
                {
                    dateRange = filterService.GetDatesByDateRange(DateTime.Parse(startDateParameter.DefaultValue, new CultureInfo(CULTURE_INFO)), DateTime.Parse(endDateParameter.DefaultValue, new CultureInfo(CULTURE_INFO)), Convert.ToInt32(dateRangeParameter.DefaultValue), Convert.ToInt32(lastXDays.DefaultValue));
                }
                else
                {
                    dateRange = new Services.DateRange { DateRangeValue = Convert.ToInt32(dateRangeParameter.DefaultValue), StartDate = DateTime.Parse(startDateParameter.DefaultValue, new CultureInfo(CULTURE_INFO)), EndDate = DateTime.Parse(endDateParameter.DefaultValue, new CultureInfo(CULTURE_INFO)) };
                }
            }

            Services.DateRange dateRangePeriod = null;
            RptParameter dateRangeParameterPeriod = orderedList.Where(ol => ol.Name.ToLower() == "dateoflossdateperiod" || ol.Name.ToLower() == "dateperiodreservechange" || ol.Name.ToLower() == "perioddaterange" || ol.Name.ToLower() == "mqfdaterange").FirstOrDefault();
            RptParameter startDateParameterPeriod = orderedList.Where(ol => ol.Name.ToLower() == "dateoflossstartdate" || ol.Name.ToLower() == "startdatereservechange" || ol.Name.ToLower() == "perioddatestartdate" || ol.Name.ToLower() == "mqfstartdate").FirstOrDefault();
            RptParameter endDateParameterPeriod = orderedList.Where(ol => ol.Name.ToLower() == "dateoflossenddate" || ol.Name.ToLower() == "enddatereservechange" || ol.Name.ToLower() == "perioddateenddate" || ol.Name.ToLower() == "mqfenddate").FirstOrDefault();
            RptParameter lastXDaysPeriod = orderedList.Where(ol => ol.Name.ToLower() == "dateoflosslastxdays" || ol.Name.ToLower() == "lastxdaysreservechange" || ol.Name.ToLower() == "perioddatelastxdays" || ol.Name.ToLower() == "mqflastxdays").FirstOrDefault();
            if (dateRangeParameterPeriod != null && startDateParameterPeriod != null && endDateParameterPeriod != null)
            {
                if (Convert.ToInt32(dateRangeParameterPeriod.DefaultValue) != (int)DateRange.Custom && Convert.ToInt32(dateRangeParameterPeriod.DefaultValue) != (int)DateRange.None)
                {
                    dateRangePeriod = filterService.GetDatesByDateRange(DateTime.Parse(startDateParameterPeriod.DefaultValue, new CultureInfo(CULTURE_INFO)), DateTime.Parse(endDateParameterPeriod.DefaultValue, new CultureInfo(CULTURE_INFO)), Convert.ToInt32(dateRangeParameterPeriod.DefaultValue), Convert.ToInt32(lastXDaysPeriod.DefaultValue));
                }
                else
                {
                    dateRangePeriod = new Services.DateRange { DateRangeValue = Convert.ToInt32(dateRangeParameterPeriod.DefaultValue), StartDate = DateTime.Parse(startDateParameterPeriod.DefaultValue, new CultureInfo(CULTURE_INFO)), EndDate = DateTime.Parse(endDateParameterPeriod.DefaultValue, new CultureInfo(CULTURE_INFO)) };
                }
            }

            Services.TrialDateRange trialDateRange = null;
            RptParameter trialDateRangeParameter = orderedList.Where(ol => ol.Name.ToLower() == "trialdate").FirstOrDefault();
            RptParameter startTrialDateParameter = orderedList.Where(ol => ol.Name.ToLower() == "starttrialdate").FirstOrDefault();
            RptParameter endTrialDateParameter = orderedList.Where(ol => ol.Name.ToLower() == "endtrialdate").FirstOrDefault();
            if (trialDateRangeParameter != null && startTrialDateParameter != null && endTrialDateParameter != null)
            {
                if (Convert.ToInt32(trialDateRangeParameter.DefaultValue) != (int)TrialDateRange.Custom && Convert.ToInt32(trialDateRangeParameter.DefaultValue) != (int)TrialDateRange.None)
                {
                    trialDateRange = filterService.GetTrialDatesByDateRange(DateTime.Parse(startTrialDateParameter.DefaultValue, new CultureInfo(CULTURE_INFO)), DateTime.Parse(endTrialDateParameter.DefaultValue, new CultureInfo(CULTURE_INFO)), Convert.ToInt32(trialDateRangeParameter.DefaultValue));
                }
                else
                {
                    trialDateRange = new Services.TrialDateRange { TrialDateRangeValue = Convert.ToInt32(trialDateRangeParameter.DefaultValue), StartTrialDate = DateTime.Parse(startTrialDateParameter.DefaultValue, new CultureInfo(CULTURE_INFO)), EndTrialDate = DateTime.Parse(endTrialDateParameter.DefaultValue, new CultureInfo(CULTURE_INFO)) };
                }
            }
			
            Services.AsOfDateType asOfDateType = null;
            RptParameter asOfDateTypeParameter = orderedList.Where(ol => ol.Name.ToLower() == "asofdatetype").FirstOrDefault();
            RptParameter asOfDateParameter = orderedList.Where(ol => ol.Name.ToLower() == "asofdate").FirstOrDefault();        
            if (asOfDateParameter != null && asOfDateTypeParameter != null)
            {
                if (Convert.ToInt32(asOfDateTypeParameter.DefaultValue) != (int)AsOfDateType.Custom)
                {
                    asOfDateType = filterService.GetAsOfDateByDateType(DateTime.Parse(asOfDateParameter.DefaultValue, new CultureInfo(CULTURE_INFO)), Convert.ToInt32(asOfDateTypeParameter.DefaultValue));
                }
                else
                {
                    asOfDateType = new Services.AsOfDateType { AsOfDate = DateTime.Parse(asOfDateParameter.DefaultValue, new CultureInfo(CULTURE_INFO)), AsOfDateTypeValue = Convert.ToInt32(asOfDateTypeParameter.DefaultValue) };
                }
            }

            foreach (RptParameter rptParameter in orderedList)
            {
               //if (rptParameter.Name.ToLower() == "lastxdays" || rptParameter.Name.ToLower() == "lastxdaysreservechange" || rptParameter.Name.ToLower() == "dateoflosslastxdays" || rptParameter.Name.ToLower() == "perioddatelastxdays" || rptParameter.Name.ToLower() == "mqflastxdays") continue;               
                if (rptParameter.Name.ToLower().Contains("lastxdays")) continue;

                string value = "";
                switch (rptParameter.Name.ToLower())
                {
                    case "createdby":
                        value = ccmsiUser.FullName;
                        break;
                    case "datetype":
                    case "capamounttimebasis":
                        switch (Convert.ToInt32(rptParameter.DefaultValue))
                        {
                            case (int)DateType.ClaimEntryDate:
                                value = "Claim Entry Date";
                                break;
                            case (int)DateType.DateOfLoss:
                                value = "Date Of Loss";
                                break;
                            case (int)DateType.DateClosed:
                                value = "Date Closed";
                                break;
                            default:
                                value = "N/A";
                                break;
                        }
                        break;
                    case "dateperiod":
                    case "daterange":
                    case "perioddaterange":
                    case "mqfdaterange":
                    case "dateoflossdateperiod":
                    case "dateperiodreservechange":
                    case "noteentrydateperiod":
                        switch (Convert.ToInt32(rptParameter.DefaultValue))
                        {
                            case (int)DateRange.None:
                                value = "None";
                                break;
                            case (int)DateRange.Custom:
                                value = "Custom";
                                break;
                            case (int)DateRange.LastMonth:
                                value = "Last Month";
                                break;
                            case (int)DateRange.LastQuarter:
                                value = "Last Quarter";
                                break;
                            case (int)DateRange.LastYear:
                                value = "Last Year";
                                break;
                            case (int)DateRange.MonthToDate:
                                value = "Month To Date";
                                break;
                            case (int)DateRange.YearToDate:
                                value = "Year To Date";
                                break;
                            case (int)DateRange.LastCalendarWeek:
                                value = "Last Calendar Week";
                                break;
                            case (int)DateRange.Last2Weeks:
                                value = "Last 2 Weeks";
                                break;
                            case (int)DateRange.AllDates:
                                value = "All Dates";
                                break;
                            case (int)DateRange.LastBusinessWeek:
                                value = "Last Business Week";
                                break;
                            case (int)DateRange.LastXDays:
                                string lastxdaysParameterName;

                                if (rptParameter.Name.ToLower() == "dateperiodreservechange")
                                {
                                    lastxdaysParameterName = "lastxdaysreservechange";
                                }
                                else if (rptParameter.Name.ToLower() == "dateoflossdateperiod")
                                {
                                    lastxdaysParameterName = "dateoflosslastxdays";
                                }
                                else if (rptParameter.Name.ToLower() == "perioddaterange")
                                {
                                    lastxdaysParameterName = "perioddatelastxdays";
                                }
                                else if (rptParameter.Name.ToLower() == "mqfdaterange")
                                {
                                    lastxdaysParameterName = "mqflastxdays";
                                }
                                else
                                {
                                    lastxdaysParameterName = "lastxdays";
                                }

                                RptParameter lastXDaysParameter = orderedList.Where(ol => ol.Name.ToLower() == (lastxdaysParameterName.ToString())).FirstOrDefault();
                                if (lastXDaysParameter != null)
                                    value = string.Format("Last {0} Days", lastXDaysParameter.DefaultValue);
                                break;
                            case (int)DateRange.Yesterday:
                                value = "Yesterday";
                                break;
                            case (int)DateRange.LastWeek:
                                value = "Last Week";
                                break;
                            case (int)DateRange.PreviousBusinessDay:
                                value = "Previous Business Day";
                                break;
                        }
                        break;
                    case "asofdatetype":
                        switch (Convert.ToInt32(rptParameter.DefaultValue))
                        {
                            case (int)AsOfDateType.LastDayOfLastMonth:
                                value = "Last Day of Last Month";
                                break;
                            case (int)AsOfDateType.Custom:
                                value = "Custom";
                                break;
                            case (int)AsOfDateType.LastDayOfLastWeek:
                                value = "Last Day of Last Week";
                                break;
                            case (int)AsOfDateType.LastDayOfLastQuarter:
                                value = "Last Day of Last Quarter";
                                break;
                            case (int)AsOfDateType.LastDayOfLastYear:
                                value = "Last Day of Last Year";
                                break;
                            case (int)AsOfDateType.Yesterday:
                                value = "Yesterday";
                                break;
                        }
                        break;
                    case "asofdate":
                        value = asOfDateType != null ? asOfDateType.AsOfDate.ToString("MM/dd/yyyy") : rptParameter.DefaultValue;
                        break;
                    case "startdate":
                        value = dateRange != null ? dateRange.StartDate.ToString("MM/dd/yyyy hh:mm tt", new CultureInfo(CULTURE_INFO)) : rptParameter.DefaultValue;
                        break;
                    case "enddate":
                        value = dateRange != null ? dateRange.EndDate.ToString("MM/dd/yyyy hh:mm tt", new CultureInfo(CULTURE_INFO)) : rptParameter.DefaultValue;
                        break;
                    case "perioddatestartdate":
                    case "dateoflossstartdate":
                    case "startdatereservechange":
                    case "mqfstartdate":
                        value = dateRangePeriod != null ? dateRangePeriod.StartDate.ToString("MM/dd/yyyy hh:mm tt", new CultureInfo(CULTURE_INFO)) : rptParameter.DefaultValue;
                        break;
                    case "perioddateenddate":
                    case "dateoflossenddate":
                    case "enddatereservechange":
                    case "mqfenddate":
                        value = dateRangePeriod != null ? dateRangePeriod.EndDate.ToString("MM/dd/yyyy hh:mm tt", new CultureInfo(CULTURE_INFO)) : rptParameter.DefaultValue;
                        break;
                    case "hierarchy":
                        value = string.IsNullOrEmpty(rptParameter.DefaultValue) || string.IsNullOrEmpty(rptParameter.DefaultValue.Replace(",", "")) ? "All"
                            : userIdParameter != null && assnNumParameter != null ? filterService.GetHierarchyDescriptionsString(assnNumParameter.DefaultValue, userIdParameter.DefaultValue, rptParameter.DefaultValue) : "All";
                        break;
                    case "totalincurredoperand":
                        value = GetRelationalOperatorValue(rptParameter);
                        break;
                    case "reservechangesrelationaloperator":
                        value = GetRelationalOperatorValue(rptParameter);
                        break;
                    case "workflowstatus":
                        switch (Convert.ToInt32(rptParameter.DefaultValue))
                        {
                            case (int)WorkflowStatus.CheckPrinted:
                                value = "Check Printed";
                                break;
                            case (int)WorkflowStatus.ReleasedToPrint:
                                value = "Released to Print";
                                break;
                        }
                        break;
                    case "activity":
                        value = !string.IsNullOrEmpty(rptParameter.DefaultValue) ? getActivityFilterKey(rptParameter.DefaultValue, reportID) : "Any Value";
                        break;
                    case "showclaimactivityxlastdays":
                        if (rptParameter.DefaultValue == "0")
                        {
                            value =  orderedList.Where(ol => ol.Name.ToLower() == "lastxdays").FirstOrDefault().DefaultValue;
                        }
                        else
                        {
                            value = rptParameter.DefaultValue;
                        }
                        break;
                    case "stateofjurisdiction":
                        if (string.IsNullOrEmpty(rptParameter.DefaultValue) || (rptParameter.DefaultValue == "a;"))
                        {
                            value = "All";
                        }
                        else if (rptParameter.DefaultValue == "c;")
                        {
                            value = "All Covered";
                        }
                        else
                        {
                            value = rptParameter.DefaultValue.Substring(2);
                        }
                        break;
                    case "claimsreceivedoption":
                        switch (Convert.ToInt32(rptParameter.DefaultValue))
                        {
                            case (int)ClaimsReceivedCalculation.NewClaimsOnly:
                                value = "New Claims Only";
                                break;
                            case (int)ClaimsReceivedCalculation.NewClaimsTransfers:
                                value = "New Claims + Transfers";
                                break;
                            case (int)ClaimsReceivedCalculation.NewClaimsReopened:
                                value = "New Claims + Reopened";
                                break;
                            case (int)ClaimsReceivedCalculation.NewClaimsTransfersReopened:
                                value = "New Claims + Transfers + Reopened";
                                break;
                        }
                        break;
                    case "trialdate":
                        switch (Convert.ToInt32(rptParameter.DefaultValue))
                        {
                            case (int)TrialDateRange.Next30Days:
                                value = "Next 30 Days";
                                break;
                            case (int)TrialDateRange.Custom:
                                value = "Custom";
                                break;
                            case (int)TrialDateRange.Next60Days:
                                value = "Next 60 Days";
                                break;
                            case (int)TrialDateRange.Next90Days:
                                value = "Next 90 Days";
                                break;
                            case (int)TrialDateRange.AllDates:
                                value = "All Dates";
                                break;
                            case (int)TrialDateRange.Prior30Days:
                                value = "Prior 30 Days";
                                break;
                            case (int)TrialDateRange.Prior60Days:
                                value = "Prior 60 Days";
                                break;
                            case (int)TrialDateRange.Prior90Days:
                                value = "Prior 90 Days";
                                break;
                        }
                        break;
                    case "starttrialdate":
                        value = trialDateRange != null ? trialDateRange.StartTrialDate.ToString("MM/dd/yyyy hh:mm tt", new CultureInfo(CULTURE_INFO)) : rptParameter.DefaultValue;
                        break;
                    case "endtrialdate":
                        value = trialDateRange != null ? trialDateRange.EndTrialDate.ToString("MM/dd/yyyy hh:mm tt", new CultureInfo(CULTURE_INFO)) : rptParameter.DefaultValue;
                        break;
                    case "suitfiled":
                        switch (Convert.ToInt32(rptParameter.DefaultValue))
                        {
                            case (int)YesNoNARange.Yes:
                                value = "Yes";
                                break;
                            case (int)YesNoNARange.No:
                                value = "No";
                                break;
                            case (int)YesNoNARange.NA:
                                value = "N/A";
                                break;
                        }
                        break;
                    case "defendantatty":
                        switch (Convert.ToInt32(rptParameter.DefaultValue))
                        {
                            case (int)YesNoNARange.Yes:
                                value = "Yes";
                                break;
                            case (int)YesNoNARange.No:
                                value = "No";
                                break;
                            case (int)YesNoNARange.NA:
                                value = "N/A";
                                break;
                        }
                        break;
                    case "claimantatty":
                        switch (Convert.ToInt32(rptParameter.DefaultValue))
                        {
                            case (int)YesNoNARange.Yes:
                                value = "Yes";
                                break;
                            case (int)YesNoNARange.No:
                                value = "No";
                                break;
                            case (int)YesNoNARange.NA:
                                value = "N/A";
                                break;
                        }
                        break;
                    case "lossratiocondition":
                        value = getLossRatioConditionFilterKey(rptParameter.DefaultValue);
                        break;
                    case "bankaccounts":
                        if (string.IsNullOrEmpty(rptParameter.DefaultValue))
                        {
                            value = "All";
                        }
                        else
                        {
                            value = GetBankAccountNameList(assnNumParameter.DefaultValue, rptParameter.DefaultValue);
                        }
                        break;
                    case "includeclosedclaims":
                        switch (Convert.ToInt32(rptParameter.DefaultValue))
                        {
                            case (int)IncludeClosedClaimsEnum.None:
                                value = "None";
                                break;
                            case (int)IncludeClosedClaimsEnum.All:
                                value = "All";
                                break;
                            case (int)IncludeClosedClaimsEnum.OnlyCurrentMonth:
                                value = "Only Current Month";
                                break;
                        }
                        break;
                    default:
                        switch (rptParameter.DataType)
                        {
                            case DataTypeEnum.Bool:
                                value = !string.IsNullOrEmpty(rptParameter.DefaultValue) && rptParameter.DefaultValue.ToLower() == "true" ? "Yes" : "No";
                                break;
                            case DataTypeEnum.Money:
                                value = ConvertToMoney(rptParameter.DefaultValue);
                                break;
                            case DataTypeEnum.String:
                                if (!string.IsNullOrEmpty(rptParameter.DefaultValue) && (rptParameter.DefaultValue.ToLower().StartsWith("e;") || rptParameter.DefaultValue.ToLower().StartsWith("i;")))
                                {
                                    if (rptParameter.DefaultValue.ToLower() == "e;" || rptParameter.DefaultValue.ToLower() == "i;")
                                    {
                                        value = string.Format("{0} - All", rptParameter.DefaultValue.Replace("e;", "Exclude").Replace("i;", "Include"));

                                        if (value == "Exclude - All") value = "All";
                                    }
                                    else
                                    {
                                        var excludeIncludeLabel = rptParameter.DefaultValue.ToLower().StartsWith("e;") ? "Exclude - " : "Include - ";
                                        value = rptParameter.Name.ToLower() == "payeefeintaxs" ? string.Format("{0}{1}", excludeIncludeLabel, GetPayeeFeinNumberList(assnNumParameter.DefaultValue, rptParameter.DefaultValue)) : rptParameter.DefaultValue.Replace("e;", "Exclude - ").Replace("i;", "Include - ");                                        
                                    }
                                }
                                else
                                {
                                    if (rptParameter.Name == "RRENumbers")
                                    {
                                        value = GetRRENumberList(rptParameter.DefaultValue, userIdParameter.DefaultValue, assnNumParameter.DefaultValue);
                                    }

                                    if (rptParameter.Name == "PoliciesNumberList")
                                    {
                                        value = GetPolicyNumberList(rptParameter.DefaultValue);
                                    }
                                
                                    if (IsLitigationReport(reportID) && !string.IsNullOrEmpty(rptParameter.DefaultValue)
                                        && (rptParameter.Name == "Venues" || rptParameter.Name == "Adjusters"
                                            || rptParameter.Name == "PrimaryCarriers" || rptParameter.Name == "IssuingCarriers"
                                            )
                                        )
                                    {
                                        switch (rptParameter.Name)
                                        {
                                            case "Venues":
                                                value = GetVenueNameList(rptParameter.DefaultValue);
                                                break;                                                
                                            case "Adjusters":
                                                value = GetAdjusterNameList(rptParameter.DefaultValue);
                                                break;
                                            case "PrimaryCarriers":
                                                value = GetPrimaryCarrierNameList(rptParameter.DefaultValue);
                                                break;
                                            case "IssuingCarriers":
                                                value = GetIssuingCarrierNameList(rptParameter.DefaultValue);
                                                break;
                                        }
                                    }

                                    if (rptParameter.Name == "ComparisonPeriods")
                                    {
                                        string[] comparisonValues = rptParameter.DefaultValue.Split(';');
                                        int periodType = Convert.ToInt32(comparisonValues[0].ToString());

                                        value = GetPeriodType(periodType);
                                        value += GetComparisonPeriods(comparisonValues[1].ToString());
                                    }

                                    if (value.Length == 0 )
                                    {
                                        value = !string.IsNullOrEmpty(rptParameter.DefaultValue) ? rptParameter.DefaultValue : "All";
                                    }
                                    
                                }
                                break;
                           
                            default:
                                value = rptParameter.DefaultValue;
                                break;
                        }
                        break;
                }
                if (getHierarchyLabelIsVisible(hierarchyLabels, rptParameter.Name))
                {
                    string paramKey = string.Format("{0}: {1}", getHierarchyLabel(hierarchyLabels, rptParameter.Name, rptParameter.Description, reportID, rptParameter.Name), value);
                    filterKey += string.IsNullOrEmpty(filterKey) ? paramKey : splitter + " " + paramKey;
                }
            }

            var sortBy = getSortByFilterKey(fields);
            if (isCustom && !string.IsNullOrEmpty(sortBy))
            {
                filterKey += "; " + sortBy;
            }

            var groupBy = getGroupByFilterKey(hierarchyLabels, fields, reportLayoutStyleName, isCustom);

            if (!string.IsNullOrEmpty(groupBy))
                filterKey += "; " + groupBy;

            return filterKey;
        }

        private string GetPeriodType(int periodType)
        {
           return  (periodType == (int)PeriodType.CalendarYear?  "Period Type: Calendar Year ": "Period Type: Fiscal Year ")  + "\n";
        }

        private string GetPeriodTime(int periodTime)
        {
            string period = string.Empty;

            switch (periodTime)
                {
                    case (int)TimePeriod.CurrentYear:
                        period = "Current Year";
                        break;
                    case(int)TimePeriod.Custom:
                        period = "Custom";
                        break;
                    case (int)TimePeriod.YearsBack1:
                        period = "1 Year Back";
                        break;
                    case (int)TimePeriod.YearsBack2:
                        period = "2 Year Back";
                        break;
                    case (int)TimePeriod.YearsBack3:
                        period = "3 Year Back";
                        break;
                    case (int)TimePeriod.YearsBack4:
                        period = "4 Year Back";
                        break;
                    case (int)TimePeriod.YearsBack5:
                        period = "5 Year Back";
                        break;
                }
            return period;
        }

        private string GetPeriodDateValue(string date)
        {
            return Convert.ToDateTime(date).ToShortDateString();
        }

        private string GetComparisonPeriods(string periodValues)
        {
            string result = string.Empty;
            string[] values = periodValues.Split(',');

            int periodCount = (int)(values.Length / 5);

            for (int rowIndex = 0; rowIndex < periodCount; rowIndex++)
            {
                for (int colIndex = 0; colIndex < 5; colIndex++)
                {
                    string item = values[(rowIndex * 5) + colIndex];
                    if (colIndex == 0)
                     {
                         result += GetPeriodTime(Convert.ToInt32(item)) + ": ";
                     }
                     else  if(colIndex != 0)
                     {
                         if (colIndex == 3)
                             result += " Compare Period: ";
                         
                        result += GetPeriodDateValue(item) + (colIndex !=2 && colIndex !=4 ?" - ":string.Empty);
                     }
                }
                result += "\n";
            }

           

            return result;
        }

        private string getActivityFilterKey(string value, int reportId)
        {
            var activities = value.Split(',');
            var index = 0;
            var result = string.Empty;

            while (index < activities.Count())
            {
                if (reportId == (int)ReportEnum.CheckRegister || reportId == (int)ReportEnum.TransactionRegister)
                {
                    result = getMathSignAsString(activities[index].ToString()) + " $" + activities[index + 1];
                    return result;
                }
                else
                {                    
                    //ReportEnum.ActivityReport
                    if ((activities[index] == "No Adjuster Activity (No Note Added)" )||(activities[index] == "No Supervisor Activity" )||(activities[index] == "No File Note" )) 
                    {
                        result += activities[index]  + ", ";
                    }
                    else
                    {
                        result += 
                            (activities[index + 1] == "Any Value" ? activities[index] + " " + activities[index + 1] : activities[index] + activities[index + 1] + activities[index + 2]) + ", ";
                    }

                    index += 3;
                }
            }

            return result.Substring(0, result.Length - 2);
        }

        private string getLossRatioConditionFilterKey(string value)
        {

            if (value.IndexOf("Any Value") > -1) return "Any Value";
            
            var filterValues = value.Split(',');
            
            if(filterValues[3] == "None")
            {
                return string.Format("{0} {1} {2}", filterValues[0], filterValues[1], filterValues[2]);
            }
            else
            {
                return value.Replace(",", " ");
            }

        }

        private string GetAdjusterNameList(string stringValue)
        {
            var filterService = new FilterService();
            return filterService.GetAdjusterNameListByAdjusterId(stringValue);
        }


        private string GetVenueNameList(string stringValue)
        {
            var filterService = new FilterService();
            return filterService.GetVenueNameListByVenueId(stringValue);
        }

        private string GetPrimaryCarrierNameList(string stringValue)
        {
            var filterService = new FilterService();
            return filterService.GetPrimaryCarrierNameListByPrimaryId(stringValue);
        }


        private string GetIssuingCarrierNameList(string stringValue)
        {
            var filterService = new FilterService();
            return filterService.GetIssuingCarrierNameListByIssuingId(stringValue);
        }

        private string GetBankAccountNameList(string assocNum, string stringValue)
        {
            var filterService = new FilterService();
            return filterService.GetBankAccountNameListByAccountNumber(assocNum, stringValue);
        }

        private string GetPayeeFeinNumberList(string assnNum, string stringValue)
        {
            var filterService = new FilterService();
            return filterService.GetPayeeFeinTaxNumberList(assnNum, stringValue);
        }

        private bool IsLitigationReport(int reportId)
        {
            return (reportId == (int)ReportEnum.LitigationManagementBasic ||
                    reportId == (int)ReportEnum.LitigationManagementDetail);
        }

        private string getMathSignAsString(string sign)
        {
            string value;
            switch (sign)
            {
                case "=":
                    value = "Equals";
                    break;
                case ">=":
                    value = "Greater Than or Equal To";
                    break;
                case ">":
                    value = "Greater Than";
                    break;
                case "<=":
                    value = "Less Than or Equal To";
                    break;
                case "<":
                    value = "Less Than";
                    break;
                default:
                    value = "Any Value";
                    break;
            }
            return value;
        }

        private string GetRelationalOperatorValue(RptParameter rptParameter)
        {
            return getMathSignAsString(rptParameter.DefaultValue);
        }

        private string getSortByFilterKey(List<RptField> fields)
        {
            string returnString = "Sort by: ";

            if (fields.Where(l => l.IsDisplayInReport && l.SortOrder > 0).ToList().Count == 0) return "";

            foreach (RptField item in fields.Where(l => l.IsDisplayInReport && l.SortOrder > 0).OrderBy(l => l.SortOrder))
            {
                returnString += (item.Name.StartsWith("@") ? item.Name.Substring(1) : item.Name) + " - " + item.SortDirection + ", ";
            }

            return returnString.Substring(0, returnString.Length - 2);
        }

        private string getGroupByFilterKey(HierarchyLabels hierarchyLabels, List<RptField> fields, string reportLayoutStyleName, bool isCustom)
        {
            if (isCustom)
            {
                if (fields.Where(l => l.IsUsed && l.GroupOrder > 0 && !l.IsSummarizable).ToList().Count == 0)
                    return "";

                string returnString = "Summary by: ";
                foreach (RptField item in fields.Where(l => l.IsUsed && l.GroupOrder > 0 && !l.IsSummarizable).OrderBy(l => l.GroupOrder))
                {
                    returnString += item.Name + ", ";
                }
                return returnString.Substring(0, returnString.Length - 2);
            }
            string groupingLabel = getHierarchyLabelForGrouping(hierarchyLabels, reportLayoutStyleName);
            if (!groupingLabel.ToLower().Contains("summary by"))
            {
                groupingLabel = "Summary by " + groupingLabel;
            }
            return groupingLabel.Replace("Summary by ", "Summary by: ");
        }

        private string getHierarchyLabelForGrouping(HierarchyLabels hierarchyLabels, string reportLayoutStyleName)
        {
            string resultString = reportLayoutStyleName;

            if (resultString.Contains("Location"))
            {
                if (hierarchyLabels.LocationLabelVisible)
                {
                    resultString = resultString.Replace("Location", capitalizeString(hierarchyLabels.LocationLabel));
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
                    resultString = resultString.Replace("Member", capitalizeString(hierarchyLabels.MemberLabel));
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
                    resultString = resultString.Replace("Group Code 1", capitalizeString(hierarchyLabels.Group1Label));
                }
                else
                {
                    return null;
                }
            }
            if (reportLayoutStyleName.Contains("Group Code 2"))
            {
                if (hierarchyLabels.Group2LabelVisible)
                {
                    resultString = resultString.Replace("Group Code 2", capitalizeString(hierarchyLabels.Group2Label));
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
                    resultString = resultString.Replace("Group Code 3", capitalizeString(hierarchyLabels.Group3Label));
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
                    resultString = resultString.Replace("Group Code 4", capitalizeString(hierarchyLabels.Group4Label));
                }
                else
                {
                    return null;
                }
            }
            return resultString;
        }

        private string getHierarchyLabel(HierarchyLabels hierarchyLabels, string fieldName, string defaultLabel, int reportID, string rptParameterName)
        {
            if (hierarchyLabels == null) return defaultLabel;
            string label = string.Empty;

            switch (fieldName.ToLower())
            {
                case "member":
                case "membername":
                case "member_name":
                    label = capitalizeString(hierarchyLabels.MemberLabel); break;
                case "location":
                case "locationname": label = capitalizeString(hierarchyLabels.LocationLabel); break;
                case "group1code":
                case "group1codename":
                    label = capitalizeString(hierarchyLabels.Group1Label); break;
                case "group2code":
                case "group2codename":
                    label = capitalizeString(hierarchyLabels.Group2Label); break;
                case "group3code":
                case "group3codename":
                    label = capitalizeString(hierarchyLabels.Group3Label); break;
                case "group4code":
                case "group4codename":
                    label = capitalizeString(hierarchyLabels.Group4Label); break;
                case "specialanalysis1": label = capitalizeString(hierarchyLabels.SpecialAnalysis1Label); break;
                case "specialanalysis2": label = capitalizeString(hierarchyLabels.SpecialAnalysis2Label); break;
                case "specialanalysis3": label = capitalizeString(hierarchyLabels.SpecialAnalysis3Label); break;
                case "specialanalysis4": label = capitalizeString(hierarchyLabels.SpecialAnalysis4Label); break;
                case "specialanalysis5": label = capitalizeString(hierarchyLabels.SpecialAnalysis5Label); break;                
                default:

                    if (reportID == (int)ReportEnum.CheckRegister)
                    {
                        label = rptParameterName.ToLower() == "activity" ? "Check Amount" : defaultLabel;
                    }
                    else if(reportID == (int)ReportEnum.TransactionRegister)
                    {
                        label = rptParameterName.ToLower() == "activity" ? "Transaction Amount" : defaultLabel;
                    }
                    else label = defaultLabel; 
                break;
            }
            return label;
        }

        private bool getHierarchyLabelIsVisible(HierarchyLabels hierarchyLabels, string fieldName)
        {
            var isVisible = true;
            if (hierarchyLabels == null) return isVisible;
            switch (fieldName.ToLower())
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

        private string GetPolicyNumberList(string stringValue)
        {
            var filterService = new FilterService();
            var resultString = filterService.GetPolicyNumberListByPolicyId(stringValue);
            return string.IsNullOrEmpty(resultString) ? "All" : resultString;
        }

        private string GetRRENumberList(string stringValue, string userid, string assocNum)
        {
            var filterService = new FilterService();
            var resultString = filterService.GetRRENumberListByRreStoreId(stringValue, userid, assocNum);
            return string.IsNullOrEmpty(resultString) ? "All" : resultString;
        }

        

        private string ConvertToMoney(string stringValue)
        {
            stringValue = stringValue.Replace("$", "").Replace(",", "");
            Decimal decValue = 0;
            Decimal.TryParse(stringValue, out decValue);
            return String.Format("{0:c}", decValue);
        }

        private string capitalizeString(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            string resultString = "";
            string[] values = value.ToLower().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in values)
            {
                resultString += uppercaseFirst(s) + " ";
            }
            return resultString.Trim();
        }

        private string uppercaseFirst(string s)
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

        private void UpdateTablix(RDL.Report rdl, UserReport userRpt, List<RptField> fields, List<RptField> summarizeFields, List<SubRpt> subReports, List<SubRptParameter> subReportParameters)
        {
            RDL.Tablix table;

            var tb = (from t in rdl.Body.ReportItems
                      where t.Name == TABLE_NAME
                      select t).FirstOrDefault();

            if (tb != null)
            {
                table = (RDL.Tablix)tb;
                int idx = rdl.Body.ReportItems.IndexOf(tb);

                TableRdlGenerator generator = new TableRdlGenerator(table, userRpt.FormatTypeID, userRpt.IsSummaryOnly);
                rdl.Body.ReportItems[idx] = generator.ModifyTablix(userRpt,TABLE_NAME, fields, summarizeFields, subReports, subReportParameters);
            }

            var groups = (from g in fields
                          where g.GroupOrder > -1
                          orderby g.GroupOrder ascending
                          select g).ToList();

            var table2 = (from t in rdl.Body.ReportItems
                          where t.Name == "table2"
                          select t).FirstOrDefault();
            if (table2 != null)
            {
                if (userRpt.ReportID != (int)ReportEnum.ManagedCareSavingsAndFees)
                {
                    if (userRpt.FormatTypeID == (int)FormatTypeEnum.Excel)
                    {
                        table2.Top = new ReportSize(groups.Count * 0.25 + 1.33, SizeTypes.Inch);
                    }
                    else
                    {
                        table2.Top = new ReportSize(groups.Count * 0.25 * 2 + 1, SizeTypes.Inch);
                    }
                }
            }

            if (userRpt.ReportID == (int)ReportEnum.AggregateReportDetailIncurred || userRpt.ReportID == (int)ReportEnum.AggregateReportDetailPaid ||
                userRpt.ReportID == (int)ReportEnum.AggregateReportSummaryIncurred || userRpt.ReportID == (int)ReportEnum.AggregateReportSummaryPaid)
            {
                rdl.Body.Height = new ReportSize(groups.Count * 0.25 + 0.44 + subReports.Count * 0.17, SizeTypes.Inch);
            }

        }

        private void UpdateTablixForSummaryOnlyReport(RDL.Report rdl, UserReport userRpt, List<RptField> fields, List<RptField> summarizeFields, List<SubRpt> subReports, List<SubRptParameter> subReportParameters)
        {
            
            RDL.Tablix table;
            ReportItem reportItem = (from t in rdl.Body.ReportItems
                                     where t.Name == TABLE_NAME
                                     select t).FirstOrDefault();

            if (reportItem != null)
            {
                table = (RDL.Tablix)reportItem;
                int idx = rdl.Body.ReportItems.IndexOf(reportItem);

                TableRdlGenerator tbl = new TableRdlGenerator(userRpt.FormatTypeID);
                rdl.Body.ReportItems[idx] = tbl.ModifyTablix(table, userRpt.IsSummaryOnly, TABLE_NAME, fields, summarizeFields, subReports, subReportParameters);
            }

            //
            // TODO: add back special logic for a few reports or even better, get rid of this somehow
            //

            //var groups = (from g in fields
            //              where g.GroupOrder > -1 && !string.IsNullOrEmpty(g.GroupExpression) && g.GroupExpression.IndexOf("Sum(Fields") == -1
            //              orderby g.GroupOrder ascending
            //              select g).ToList();

            //var table2 = (from t in _rdl.Body.ReportItems
            //              where t.Name == "table2"
            //              select t).FirstOrDefault();
            //if (table2 != null)
            //{
            //    if (_parentReportName.ToLower() != ("Managed Care Savings and Fees").ToLower())
            //    {
            //        if (_reportFormat.ToLower() == "excel")
            //        {
            //            table2.Top = new ReportSize(groups.Count * 0.25 + 1.33, SizeTypes.Inch);
            //        }
            //        else
            //        {
            //            table2.Top = new ReportSize(groups.Count * 0.25 * 2 + 1, SizeTypes.Inch);
            //        }
            //    }
            //}

            //if (_reportName.Contains("Aggregate"))
            //{
            //    _rdl.Body.Height = new ReportSize(groups.Count * 0.25 + 0.44 + _subReports.Count * 0.17, SizeTypes.Inch);
            //}

        }

        public byte[] RenderReport(UserReport userReport, List<RptParameter> reportParms)
        {
            byte[] reportArray = null;
            string deviceInfo = null;
            string encoding;
            string mimeType;
            string extension;
            RE.Warning[] warningsArray = null;
            string[] streamIDs = null;
            string pathName = ConfigurationManager.AppSettings["ReportServerPath"];


            if (userReport != null)
            {
                List<RE.ParameterValue> paramList = new List<RE.ParameterValue>();

                foreach (RptParameter item in reportParms)
                {
                    if (item.IsReportParameter)
                    {
                        RE.ParameterValue param = new RE.ParameterValue();
                        param.Name = item.Name;
                        param.Value = item.DefaultValue;
                        paramList.Add(param);
                    }
                }

                paramList.Add(new ParameterValue { Name = "userReportID", Value = userReport.UserReportID.ToString() });              

                RE.ExecutionInfo ei = new RE.ExecutionInfo();
                RE.ExecutionHeader eh = new RE.ExecutionHeader();

                _reportExecution.ExecutionHeaderValue = eh;
                ei = _reportExecution.LoadReport(pathName + userReport.ReportLayoutStyle.ReportLink, null);                 

                _reportExecution.SetExecutionParameters(paramList.ToArray(), "en-us");

                reportArray = _reportExecution.Render(userReport.FormatTypeID == (int)FormatTypeEnum.HTML ? "HTML4.0" : Enum.GetName(typeof(FormatTypeEnum), userReport.FormatTypeID), deviceInfo,
                                                      out extension, out encoding, out mimeType, out warningsArray,
                                                      out streamIDs);

                ei = _reportExecution.GetExecutionInfo();
            }

            return reportArray;
        }

        public void CancelReport(GenerateReportRequest reportRequest)
        {
            // cancel the rendering report

            /*
             TODO:
             * Allow jobs to be cancelled on the backend to conserve resources.
             * This used to be possible before when we uploaded RDL for every user report,
             * so we were (virtually) guaranteed that only a single job per uploaded report would be running at one time. 
             * So we could safely use the report name returned by ListJobs to identity the running job.  
             * But now that we can have the same report being executed by many users simultaneously this is not possible anymore.  
             * When the user cancels a report we can't be sure which job to cancel.
            */

            // find the job for the rendering report
            RS.Job[] jobs = _reportService.ListJobs();
            string jobID = String.Empty;
            string reportName = String.Empty;

            if (reportRequest is GenerateStandardReportRequest)
            {
                GenerateStandardReportRequest request = (GenerateStandardReportRequest)reportRequest;
                reportName = request.Parameters.Where(ol => ol.Name.ToLower() == "reportname").FirstOrDefault().DefaultValue;
            }
            else if (reportRequest is GenerateCustomReportRequest)
            {
                GenerateCustomReportRequest request = (GenerateCustomReportRequest)reportRequest;
                reportName = request.Parameters.Where(ol => ol.Name.ToLower() == "reportname").FirstOrDefault().DefaultValue;
            }


            foreach (RS.Job job in jobs.Where(jobItem => jobItem.Machine.Contains(System.Environment.MachineName)))
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Job ID: {0} | Name : {1} | Path: {2} | Action: {3} | Description: {4}",
                    job.JobID, job.Name, job.Path, job.Action.ToString(), job.Description));

                reportName = reportName.Replace(" ", String.Empty);
                if (job.Name.Contains(reportName))
                {
                    jobID = job.JobID;

                    if (string.IsNullOrEmpty(jobID))
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("Cancelling Report | No Job found for request ID: {0}", reportRequest.ReportRequestGuid));
                    }
                    else
                    {
                        //Cancel Jobs belongs to the machine name
                        System.Diagnostics.Debug.WriteLine(String.Format("Cancelling Report | Request ID: {1} | Job ID: {0}", reportRequest.ReportRequestGuid, jobID));
                        _reportService.CancelJob(jobID);
                    }
                }
            }
        }

        #region User Report methods

        public List<RptField> GetClientSpecificCodeFields(int userReportID)
        {
            ReportProjectDBEntities entities = new ReportProjectDBEntities();
            //Get user report Fields
            var fields = from f in entities.UserReportField
            .Include("ReportField.DataType")
                         join rf in entities.ReportField on f.ReportField.ReportFieldID equals rf.ReportFieldID
                         where f.UserReport.UserReportID == userReportID && (f.UserReport.IsDeleted == false || f.UserReport.IsDeleted == null) && rf.isClientSpecificCode 
                         orderby f.ColumnOrder
                         select new
                         {
                             SortDirection = f.SortDirection,
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
                             FieldValueExpression = rf.FieldValueExpression,
                             UseSummaryExpresionGroup = rf.UseSummaryExpresionGroup,
                             CoverageCode = f.CoverageCode 

                         };

            List<RptField> myUserReportfieldList = new List<RptField>();
            foreach (var userReportField in fields.ToList())
            {
                myUserReportfieldList.Add(new RptField()
                {
                    ID = userReportField.UserReportFieldID,
                    ReportID = userReportID,
                    Name = userReportField.CustomName,
                    SQLName = userReportField.ReportField.SQLFieldName + '_' + userReportField.CoverageCode,
                    DataType = (DataTypeEnum)userReportField.DataTypeID,
                    ColumnOrder = userReportField.ColumnOrder,
                    ColumnWidthFactor = userReportField.ColumnWidthFactor,
                    SortOrder = userReportField.SortOrder,
                    SortDirection = userReportField.SortDirection == "D" ? SortDirectionEnum.Descending : SortDirectionEnum.Ascending,
                    GroupOrder = userReportField.GroupOrder,
                    GroupSummaryExpression = userReportField.GroupSummaryExpression,
                    IncludePageBreak = userReportField.IncludePageBreak,
                    IsDisplayInReport = userReportField.IsDisplayInReport,
                    IsGroupByDefault = userReportField.IsGroupByDefault,
                    IsGroupable = userReportField.IsGroupable,
                    IsSummarizable = userReportField.IsSummarizable,
                    IsUsed = userReportField.ColumnOrder > 0 ? true : false,
                    FieldValueExpression = userReportField.FieldValueExpression,
                    UseSummaryExpresionGroup = userReportField.UseSummaryExpresionGroup
                });
            }

            return myUserReportfieldList;
        }

        public List<RptField> GetUserReportFields(int userReportID)
        {
            ReportProjectDBEntities entities = new ReportProjectDBEntities();
            //Get user report Fields
            var fields = from f in entities.UserReportField
            .Include("ReportField.DataType")
                         join rf in entities.ReportField on f.ReportField.ReportFieldID equals rf.ReportFieldID
                         where f.UserReport.UserReportID == userReportID && (f.UserReport.IsDeleted == false || f.UserReport.IsDeleted == null)
                         orderby f.ColumnOrder
                         select new
                         {
                             SortDirection = f.SortDirection,
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
                             FieldValueExpression = rf.FieldValueExpression,
                             UseSummaryExpresionGroup = rf.UseSummaryExpresionGroup, 
                             CoverageCode = f.CoverageCode,
                             IsClientSpecificCode = rf.isClientSpecificCode 
                             
                         };

            List<RptField> myUserReportfieldList = new List<RptField>();
            foreach (var userReportField in fields.ToList())
            {
                myUserReportfieldList.Add(new RptField()
                {
                    ID = userReportField.UserReportFieldID,
                    ReportID = userReportID,
                    Name = userReportField.CustomName,
                    SQLName = userReportField.IsClientSpecificCode ? userReportField.ReportField.SQLFieldName + '_' + userReportField.CoverageCode  : userReportField.ReportField.SQLFieldName,
                    DataType = (DataTypeEnum)userReportField.DataTypeID,
                    ColumnOrder = userReportField.ColumnOrder,
                    ColumnWidthFactor = userReportField.ColumnWidthFactor,
                    SortOrder = userReportField.SortOrder,
                    SortDirection = userReportField.SortDirection == "D" ? SortDirectionEnum.Descending : SortDirectionEnum.Ascending,
                    GroupOrder = userReportField.GroupOrder,
                    GroupSummaryExpression = userReportField.GroupSummaryExpression,
                    IncludePageBreak = userReportField.IncludePageBreak,
                    IsDisplayInReport = userReportField.IsDisplayInReport,
                    IsGroupByDefault = userReportField.IsGroupByDefault,
                    IsGroupable = userReportField.IsGroupable,
                    IsSummarizable = userReportField.IsSummarizable,
                    IsUsed = userReportField.ColumnOrder > 0 ? true : false,
                    FieldValueExpression = userReportField.FieldValueExpression, 
                    UseSummaryExpresionGroup = userReportField.UseSummaryExpresionGroup 
                });
            }

            return myUserReportfieldList;
        }

        public List<RptParameter> GetUserReportParameters(int userReportID)
        {
            ReportProjectDBEntities entities = new ReportProjectDBEntities();

            var parameterList = from p in entities.UserReportParameter
                               .Include("UserReport")
                               .Include("ReportParameter.DataType")
                                where p.UserReport.UserReportID == userReportID
                                select p;

            // convert the UserReportParameter list to a RptParameter list
            List<RptParameter> myparameters = new List<RptParameter>();
            foreach (var parameter in parameterList)
            {
                myparameters.Add(new RptParameter()
                {
                    ID = parameter.UserReportParameterID,
                    ReportID = userReportID,
                    Name = parameter.ReportParameter.ReportParameterName,
                    Description = parameter.ReportParameter.Description,
                    DataType = (DataTypeEnum)parameter.ReportParameter.DataType.DataTypeID,
                    DefaultValue = parameter.ParameterValue,
                    IsQueryParameter = parameter.ReportParameter.IsQueryParameter,
                    IsReportParameter = parameter.ReportParameter.IsReportParameter,
                    OrderIndex = parameter.ReportParameter.OrderIndex
                });
            }
            return myparameters;
        }

        public List<RptField> GetUserReportSummarizeFields(int userReportID)
        {
            ReportProjectDBEntities entities = new ReportProjectDBEntities();

            var fields = from f in entities.UserReportField
           .Include("ReportField.DataType")
                         join rf in entities.ReportField on f.ReportField.ReportFieldID equals rf.ReportFieldID
                         where f.UserReport.UserReportID == userReportID && (f.UserReport.IsDeleted == false || f.UserReport.IsDeleted == null)
                         orderby f.ColumnOrder
                         select new
                         {
                             SortDirection = f.SortDirection,
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
                             FieldValueExpression = rf.FieldValueExpression,
                             UseSummaryExpresionGroup = rf.UseSummaryExpresionGroup 
                         };

            var sumfields = from f in entities.UserReportSummarizeField
                      .Include("UserReportField.ReportField.DataType")
                            where f.UserReport.UserReportID == userReportID
                            select f;

            var list = from f in fields
                       join sc in sumfields
                       on f.UserReportFieldID equals sc.UserReportFieldID
                       select f;

            List<RptField> userSummarizeRptFieldList = null;
            if (list.ToList().Count > 0)
            {
                userSummarizeRptFieldList = new List<RptField>();
                foreach (var sumItem in list)
                {
                    RptField mySumItem = new RptField()
                    {
                        ID = sumItem.UserReportFieldID,
                        ReportID = userReportID,
                        Name = sumItem.CustomName,
                        ColumnOrder = sumItem.ColumnOrder,
                        ColumnWidthFactor = sumItem.ColumnWidthFactor,
                        SortOrder = sumItem.SortOrder,
                        SortDirection = sumItem.SortDirection == "D" ? SortDirectionEnum.Descending : SortDirectionEnum.Ascending,
                        GroupOrder = sumItem.GroupOrder,
                        GroupSummaryExpression = sumItem.GroupSummaryExpression,
                        IncludePageBreak = sumItem.IncludePageBreak,
                        IsDisplayInReport = sumItem.IsDisplayInReport,
                        IsGroupByDefault = sumItem.IsGroupByDefault,
                        IsGroupable = sumItem.IsGroupable,
                        IsSummarizable = sumItem.IsSummarizable,
                        IsUsed = sumItem.ColumnOrder > 0 ? true : false,
                        DataType = sumItem.ReportField != null ? (DataTypeEnum)sumItem.DataTypeID : DataTypeEnum.String,
                        SQLName = sumItem.ReportField != null ? sumItem.ReportField.SQLFieldName : "",
                        FieldValueExpression = sumItem.FieldValueExpression,
                        UseSummaryExpresionGroup = sumItem.UseSummaryExpresionGroup 
                    };

                    userSummarizeRptFieldList.Add(mySumItem);
                }
            }
            return userSummarizeRptFieldList;
        }

        public List<SubRpt> GetSubreports(int reportID)
        {
            ReportProjectDBEntities entities = new ReportProjectDBEntities();

            var subReports = from p in entities.SubReport
                             where p.ReportID == reportID
                             orderby p.OrderIndex
                             select p;

            List<SubRpt> mySubreports = new List<SubRpt>();
            foreach (var subReport in subReports)
            {
                mySubreports.Add(new SubRpt()
                {
                    SubReportID = subReport.SubReportID,
                    SubReportName = subReport.SubReportName,
                    SPName = subReport.SPName
                });
            }

            return mySubreports;
        }

        public List<SubRptParameter> GetSubreportParameters(int reportID)
        {
            ReportProjectDBEntities entities = new ReportProjectDBEntities();
            var subReportParameters = from p in entities.SubReportParameter
                                      join sr in entities.SubReport on p.SubReportID equals sr.SubReportID
                                      where sr.ReportID == reportID
                                      select p;

            List<SubRptParameter> mySubRptParameters = new List<SubRptParameter>();
            foreach (var subReportParameter in subReportParameters)
            {
                mySubRptParameters.Add(new SubRptParameter()
                {
                    SubReportID = subReportParameter.SubReportID,
                    SubReportParameterName = subReportParameter.SubReportParameterName,
                    DefaultValue = subReportParameter.FieldValueExpression
                });
            }

            return mySubRptParameters;
        }

        #endregion
    }
}
