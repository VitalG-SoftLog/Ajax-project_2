using System;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using ReportBuilderAjax.Web.Common;
using ReportBuilderAjax.Web.ReportBuilder;

namespace ReportBuilderAjax.Web.HttpHandlers
{
    //[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class ReportGenerationService// : DuplexService
    {
        /*private Dictionary<Guid, GenerateReportRequest> _reportRequestQueue = new Dictionary<Guid, GenerateReportRequest>();
        private Dictionary<Guid, string> _reportNameGuidQueue = new Dictionary<Guid, string>();*/
        private string _ReportFolderPath;

        public ReportGenerationService()
        {
            _ReportFolderPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/");
        }
        /*
        private delegate void generateReportDelegate(string sessionID, GenerateReportRequest reportRequest);

        private void cancelReport(CancelReportRequest request)
        {
            GenerateReportResponse response = new GenerateReportResponse();
            response.Status = ReportRequestStatus.Cancelled;
            try
            {
                if (!_reportRequestQueue.ContainsKey(request.ReportRequestGuid))
                {
                    // report probably already completed or may have errored out, so do nothing
                    System.Diagnostics.Debug.WriteLine(String.Format("No report request in queue to cancel | Request ID: {0} | {1}",
                        request.ReportRequestGuid.ToString(), DateTime.Now.TimeOfDay.ToString()));
                    return;
                }

                GenerateReportRequest rptRequest = _reportRequestQueue[request.ReportRequestGuid];
                System.Diagnostics.Debug.WriteLine(String.Format("Report request being cancelled | Request ID: {0} | Status: {1} | {2}",
                    request.ReportRequestGuid.ToString(), rptRequest.Status.ToString(), DateTime.Now.TimeOfDay.ToString()));

                if (rptRequest.Status == RunningReportStatus.Rendering)
                {
                    rptRequest.Status = RunningReportStatus.Cancelled;
                    RdlManager mgr = new RdlManager();
                    mgr.CancelReport(rptRequest);
                }
                else
                {
                    rptRequest.Status = RunningReportStatus.Cancelled;
                }

            }
            catch (Exception ex)
            {
                handleReportRequestException(ex, response);
                System.Diagnostics.Debug.WriteLine("Non-critical failure cancelling a report: " + ex.ToString());

                // just log and then go on--this is not a critical error because the report will just run to completion and then be ignored
                Logger.Write("Non-critical failure cancelling a report: " + ex.ToString(), "SLLog");
            }
        }
        */
        public GenerateReportResponse GenerateReportExecute(string sessionID, GenerateReportRequest reportRequest)
        {
            int userReportID = -1;
            bool includeTitlePage = true;
            List<RptParameter> parameters = null;
            UserReport userReport = null;
            ReportProjectDBEntities entities = new ReportProjectDBEntities();

            GenerateReportResponse response = new GenerateReportResponse();
            response.ReportRequestGuid = reportRequest.ReportRequestGuid;

            try
            {
                // Get the user report ID (first creating a transient user report, if necessary)
                if (reportRequest is GenerateUserReportRequest)
                {
                    GenerateUserReportRequest request = (GenerateUserReportRequest) reportRequest;
                    userReportID = request.UserReportID;
                }
                else if (reportRequest is GenerateStandardReportRequest)
                {
                    GenerateStandardReportRequest request = (GenerateStandardReportRequest) reportRequest;
                    ReportBuilderService reportBuilderService = new ReportBuilderService();

                    int reportID = request.ReportID;
                    int reportLayoutStyleID = request.ReportLayoutStyleID;
                    int formatTypeID = request.FormatTypeId;
                    bool isTurnOffPageBreak = request.IsTurnOffPageBreak;
                    includeTitlePage = request.IncludeTitlePage;
                    parameters = request.Parameters;
                    userReportID = reportBuilderService.SaveStandardUserReport(request.UserID, reportID,
                                                                               reportLayoutStyleID, formatTypeID,
                                                                               parameters.Where(
                                                                                   p =>
                                                                                   p.Name.ToLower() == "userreportname")
                                                                                   .FirstOrDefault().DefaultValue,
                                                                               parameters, request.AssociationNumber,
                                                                               true, isTurnOffPageBreak,
                                                                               includeTitlePage);
                }
                else if (reportRequest is GenerateCustomReportRequest)
                {
                    GenerateCustomReportRequest request = (GenerateCustomReportRequest) reportRequest;
                    ReportBuilderService reportBuilderService = new ReportBuilderService();

                    int reportID = request.ReportID;
                    int reportLayoutStyleID = request.ReportLayoutStyleID;
                    int formatTypeID = request.FormatTypeId;
                    parameters = request.Parameters;
                    bool isSummaryOnly = request.IsSummaryOnly;
                    includeTitlePage = request.IncludeTitlePage;
                    List<RptField> fields = request.Fields;
                    List<RptField> userSummarizeRptField = request.UserSummarizeRptField;

                    userReportID = reportBuilderService.SaveCustomUserReport(request.UserID, reportID, formatTypeID,
                                                                             reportLayoutStyleID,
                                                                             parameters.Where(
                                                                                 p =>
                                                                                 p.Name.ToLower() == "userreportname").
                                                                                 FirstOrDefault().DefaultValue,
                                                                             isSummaryOnly, fields,
                                                                             userSummarizeRptField, parameters,
                                                                             request.AssociationNumber, true,
                                                                             includeTitlePage);
                }

                // Read the user report and the user report parameters (if necessary)
                userReport = (from ur in entities.UserReport
                                  .Include("ReportLayoutStyle")
                                  .Include("FormatType")
                                  .Include("Report")
                              where ur.UserReportID == userReportID
                              select ur).FirstOrDefault();


                if (userReport.FormatTypeID == (int) FormatTypeEnum.Grid)
                {
                    reportRequest.Status = RunningReportStatus.Rendering;
                    try
                    {
                        ReportDomainService domainService = new ReportDomainService();
                        if (parameters == null)
                        {
                            parameters =
                                domainService.GetUserReportParameters(userReportID, userReport.ReportID).ToList();
                        }
                        List<Dictionary<string, object>> resultDictionary = new List<Dictionary<string, object>>();
                        DBHelper dbHelper =
                            new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);

                        List<object> myParams = new List<object>();

                        foreach (RptParameter rptParam in parameters.Where(p => p.IsQueryParameter).ToList())
                        {
                            myParams.Add("@" + rptParam.Name);
                            if (rptParam.DataType == DataTypeEnum.Bool && !string.IsNullOrEmpty(rptParam.DefaultValue))
                                myParams.Add(
                                    Convert.ToInt32(rptParam.DefaultValue.ToLower().Replace("false", "0").Replace(
                                        "true", "1")));
                            else if (rptParam.DataType == DataTypeEnum.Integer &&
                                     !string.IsNullOrEmpty(rptParam.DefaultValue))
                                myParams.Add(Convert.ToInt32(rptParam.DefaultValue));
                            else myParams.Add(rptParam.DefaultValue);
                        }

                        if (dbHelper.IsExistsCommandParameter(userReport.Report.VerifyRowCountSP.Replace("_Count", "_Report"),
                                                              CommandType.StoredProcedure, "@IsCustom"))
                        {
                            var isCustomParameterIndex = myParams.IndexOf("@IsCustom");
                            if (isCustomParameterIndex != -1)
                            {
                                myParams[isCustomParameterIndex + 1] = 1;
                            }
                            else
                            {
                                myParams.Add("@IsCustom");
                                myParams.Add(1);
                            }
                        }

                        if (userReport.Report.VerifyRowCountSP.Replace("_Count", "_Report") == "rb_PAndIRembursableReport_Report")
                        {
                            var index = 0;
                            foreach (var myParam in myParams)
                            {

                                if (myParam.ToString().ToLower() == "@claimtype")
                                {
                                    break;
                                }
                                index++;
                            }

                            myParams[index] = "@ClaimTypes";
                        }

                        if (userReport.Report.VerifyRowCountSP.Replace("_Count", "_Report") == "rb_LossRunSummary_Report")
                        {
                            myParams.Add("@AssnName");
                            myParams.Add("Test");
                        }

                        List<RptField> fields =
                            domainService.GetUserReportFields(userReportID, userReport.ReportID, userReport.ClientNumber)
                                .ToList();

                        DataTable recordDataTable = dbHelper.GetDataTable(userReport.Report.VerifyRowCountSP.Replace("_Count", "_Report"),
                                                                          CommandType.StoredProcedure,
                                                                          myParams.ToArray());
                        if (recordDataTable != null && recordDataTable.Rows != null && recordDataTable.Rows.Count != 0)
                        {
                            int i = 0;
                            foreach (DataRow row in recordDataTable.Rows)
                            {
                                Dictionary<string, object> drow = new Dictionary<string, object>();
                                foreach (RptField field in fields)
                                {

                                    if (!string.IsNullOrEmpty(field.SQLName) && field.ColumnOrder > 0)
                                    {
                                        if (!drow.ContainsKey(field.SQLName))
                                        {
                                            if (field.DataType == DataTypeEnum.Date)
                                            {
                                                drow.Add(field.SQLName,
                                                         row[field.SQLName] != DBNull.Value
                                                             ? DateTime.Parse(
                                                                 DateTime.Parse(row[field.SQLName].ToString()).
                                                                     ToShortDateString()).ToString("MM/dd/yyyy")
                                                             : new DateTime().ToString("MM/dd/yyyy"));
                                            }
                                            else if (field.DataType == DataTypeEnum.Money)
                                            {
                                                IFormatProvider provider = new CultureInfo("en-US");
                                                drow.Add(field.SQLName,
                                                         row[field.SQLName] != DBNull.Value ? String.Format(provider, "{0:C}", row[field.SQLName]) : "$0.00");
                                            }
                                            else
                                            {
                                                drow.Add(field.SQLName,
                                                         row[field.SQLName] != DBNull.Value ? row[field.SQLName] : null);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ///parser    drow.Add(field.CustomName, CustomValue);
                                    }
                                }
                                i++;
                                drow.Add("ID", i.ToString());
                                resultDictionary.Add(drow);
                            }
                        }

                        response.UserReportFields = fields;
                        response.GridData = resultDictionary;
                        response.Status = ReportRequestStatus.Completed;
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("Report processing has been canceled by the user"))
                        {
                            System.Diagnostics.Debug.WriteLine(
                                String.Format(
                                    "Report processing cancelled by user during render | Request ID: {0} | {1}",
                                    reportRequest.ReportRequestGuid.ToString(), DateTime.Now.TimeOfDay.ToString()));
                            response.Status = ReportRequestStatus.Cancelled;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    if (parameters == null)
                    {
                        ReportDomainService reportDomainService = new ReportDomainService();
                        parameters =
                            reportDomainService.GetUserReportParameters(userReportID, userReport.ReportID).ToList();
                    }

                    //TODO: remove this.  The userReportName can't be used any longer to cancel a job.
                    /*if (!_reportNameGuidQueue.ContainsKey(reportRequest.ReportRequestGuid))
                    {
                        _reportNameGuidQueue.Add(reportRequest.ReportRequestGuid, userReport.UserReportName);
                    }*/

                    System.Diagnostics.Debug.WriteLine(String.Format("Rendering report | Request ID: {0} | {1}",
                                                                     reportRequest.ReportRequestGuid.ToString(),
                                                                     DateTime.Now.TimeOfDay.ToString()));
                    reportRequest.Status = RunningReportStatus.Rendering;

                    try
                    {
                        RdlManager rdlManager = new RdlManager();
                        byte[] rpt = rdlManager.RenderReport(userReport, parameters);

                        // save the rendered report to the file system and send the url back to the client
                        string fileName = createFileName(userReport.UserReportName, userReport.FormatType.Extension);
                        System.IO.File.WriteAllBytes(_ReportFolderPath + fileName, rpt);
                        response.ReportUrl = ConfigurationManager.AppSettings["ReportFolder"] + fileName;
                        response.Status = ReportRequestStatus.Completed;
                        if ((int) FormatTypeEnum.HTML == userReport.FormatTypeID)
                        {
                            StreamReader streamReader = new StreamReader(new MemoryStream(rpt));
                            response.Content = Processing(streamReader.ReadToEnd(), "IMG", "SRC");
                            //var str = Processing(Regex.Replace(response.Content, @"<META(.|\n)*?>", string.Empty), "IMG", "SRC");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("Report processing has been canceled by the user"))
                        {
                            System.Diagnostics.Debug.WriteLine(
                                String.Format(
                                    "Report processing cancelled by user during render | Request ID: {0} | {1}",
                                    reportRequest.ReportRequestGuid.ToString(), DateTime.Now.TimeOfDay.ToString()));
                            response.Status = ReportRequestStatus.Cancelled;
                        }
                        else
                        {
                            Logger.Write(ex, MethodBase.GetCurrentMethod(), ex.Message, "-1");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex, MethodBase.GetCurrentMethod(), ex.Message, "-1");
            }
            finally
            {
                // push the response back to the requestor.  If succeeds includes the url to the completed report, 
                //  if fails includes a user friendly failure message.
                System.Diagnostics.Debug.WriteLine(string.Format("Pushing message to client | Request ID: {0} | Status: {1} | {2} ",
                    reportRequest.ReportRequestGuid.ToString(), response.Status.ToString(), DateTime.Now.TimeOfDay.ToString()));
                //this.PushMessageToClient(sessionID, response);

                // do the non-essential things here -- we don't want a failure on any of these things to prevent
                //  the client from getting the report
                try
                {
                    /*_reportRequestQueue.Remove(reportRequest.ReportRequestGuid);
                    //TODO: remove this?
                    if (_reportNameGuidQueue.ContainsKey(reportRequest.ReportRequestGuid))
                    {
                        _reportNameGuidQueue.Remove(reportRequest.ReportRequestGuid);
                    }*/

                    if (userReport != null && userReport.IsTransient)
                    {
                        deleteTransientUserReport(userReport, entities);
                    }

                    cleanUpTempFiles();
                }
                catch (Exception ex)
                {
                    Logger.Write(ex, MethodBase.GetCurrentMethod(), ex.Message, "-1");
                }
            }
            return response;
        
        }

        public string Processing(string Source, string Tag, string Attribute)
        {
            return Regex.Replace(Source, string.Format(@"(<{0}\b[^>]*?\b)({1}=""(?:[^""]*)"")", Tag, Attribute), string.Empty);
        }
        
        private void deleteTransientUserReport(UserReport userReport, ReportProjectDBEntities entities)
        {
            // delete the summarize fields
            var sumFields = (from ursf in entities.UserReportSummarizeField
                                   where ursf.UserReportID == userReport.UserReportID
                                   select ursf).ToList();
            foreach (UserReportSummarizeField sumField in sumFields)
            {
                entities.DeleteObject(sumField);
            }

            // delete the fields
            //TODO: NOt necessary, must be a cascade delete in the database

            // delete the parameters
            var userReportParms = (from urp in entities.UserReportParameter
                                   where urp.UserReportID == userReport.UserReportID
                                   select urp).ToList();
            foreach (UserReportParameter userReportParameter in userReportParms)
            {
                entities.DeleteObject(userReportParameter);
            }

            // delete the user report
            entities.DeleteObject(userReport);
            
            entities.SaveChanges();
        }
        /*
        private void handleReportRequestException(Exception ex, GenerateReportResponse response)
        {
            if (ex.GetType() == typeof(System.TimeoutException) ||
                (ex.GetType() == typeof(System.Net.WebException) && ex.Message.Contains("The operation has timed out")))
            {
                response.Status = ReportRequestStatus.TimedOut;
                response.StatusMessage = "The report has timed out.  Please narrow your filter criteria";
            }
            else if (ex.GetType() == typeof(System.Web.Services.Protocols.SoapException) &&
                (ex.Message.Contains("ProcessingAbortedException") || ex.Message.Contains("JobCanceledException")))
            {
                response.Status = ReportRequestStatus.Cancelled;
                response.StatusMessage = "The report was cancelled by the system or an administrator.  This is rare but is typically done when a report is consuming too many resources.  You may want to narrow your filter criteria and try again.";
            }
            else
            {
                response.Status = ReportRequestStatus.Failed;
                response.StatusMessage = "An error occurred generating your report.  Please try again.  If you continue to receive this error message, please contact your system administrator.";
            }

            // try to write to log but go on if it fails
            try
            {
                Logger.Write(ex.ToString(), "SLLog");
            }
            catch
            {
            }
        }
        */
        private string createFileName(string userReportName, string extension)
        {
            string pathName = ConfigurationManager.AppSettings["ReportServerPath"];
            Regex reg = new Regex("[^0-9a-zA-Z_]+?");
            return reg.Replace(string.Format("{0}-{1}-{2}", userReportName.Length > 150 ? userReportName.Substring(0, 150 - pathName.Length - 1) : userReportName, DateTime.Now, DateTime.Now.Millisecond), "-") + "." + extension;
        }
        
        private void cleanUpTempFiles()
        {
            try
            {
                //string path = System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/");

                string[] files;
                files = System.IO.Directory.GetFiles(_ReportFolderPath);

                DateTime checkDate = DateTime.Now.AddDays(-1);

                foreach (string file in files)
                {
                    if (System.IO.File.GetCreationTime(file) < checkDate)
                        System.IO.File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                // log and then ignore file delete exceptions for now
                //Logger.Write("Failed to clean up temporary / old report files: " + ex.ToString(), "SLLog");
            }
        }
/*
        protected override void OnMessage(string sessionID, DuplexMessage message)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("Message Received | Type: {0} | {1} ", 
                message.GetType().ToString(), DateTime.Now.TimeOfDay.ToString()));

            if (message is GenerateReportRequest)
            {
                // Add request to queue and set status to "pending"
                GenerateReportRequest request = (GenerateReportRequest)message;
                request.Status = RunningReportStatus.Pending;
                _reportRequestQueue.Add(request.ReportRequestGuid, request);
                System.Diagnostics.Debug.WriteLine(String.Format("Request added to queue | Request ID: {0} | {1}",
                    request.ReportRequestGuid.ToString(), DateTime.Now.TimeOfDay.ToString()));

                // run async and return immediately--if I don't do this these long-running service calls queue and run sequentially 
                generateReportDelegate genRptDelegate = new generateReportDelegate(generateReportExecute);
                IAsyncResult result = genRptDelegate.BeginInvoke(sessionID, request, null, genRptDelegate);
            }
            else if (message is CancelReportRequest)
            {
                CancelReportRequest request = (CancelReportRequest)message;
                cancelReport(request);
            }
        }*/
    }
}
