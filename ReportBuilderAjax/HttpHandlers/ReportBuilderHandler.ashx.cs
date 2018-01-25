using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Script.Serialization;
using MS.Internal.Xml.XPath;
using Newtonsoft.Json;
using ReportBuilderAjax.Web.Attributes;
using ReportBuilderAjax.Web.Common;
using ReportBuilderAjax.Web.Models;
using ReportBuilderAjax.Web.Services;
using ReportBuilderSL.Ajax;

namespace ReportBuilderAjax.Web.HttpHandlers
{
    public class ReportBuilderHandler : RFBaseTemplateHandler
    {
        private string[] sep = { "/" };

        private List<RptField> getUserReportFieldList(bool saveAsNew, List<RptFieldInfo> list)
        {
            List<RptField> fields = new List<RptField>();

            foreach (RptFieldInfo rptFieldInfo in list)
            {
                if (rptFieldInfo.IsUsed || rptFieldInfo.SortOrder > 0 || rptFieldInfo.IsGroupByDefault)
                {
                    RptField rptField = (new RptField
                    {
                        ID = saveAsNew ? rptFieldInfo.ReportFieldID : rptFieldInfo.ID,
                        ReportFieldID = rptFieldInfo.ReportFieldID,
                        RptFieldID = rptFieldInfo.RptFieldInfoID,
                        ReportID = rptFieldInfo.ReportID,
                        Name = rptFieldInfo.Name,
                        SQLName = rptFieldInfo.SQLName,
                        DataType = (DataTypeEnum)rptFieldInfo.DataType,
                        ColumnOrder = rptFieldInfo.ColumnOrder,
                        ColumnWidthFactor = rptFieldInfo.ColumnWidthFactor,
                        SortOrder = rptFieldInfo.SortOrder,
                        SortDirection = (SortDirectionEnum)rptFieldInfo.SortDirection,
                        GroupOrder = rptFieldInfo.GroupOrder,
                        GroupSummaryExpression = rptFieldInfo.GroupSummaryExpression,
                        IncludePageBreak = rptFieldInfo.IncludePageBreak,
                        IsDisplayInReport = rptFieldInfo.IsDisplayInReport,
                        IsGroupByDefault = rptFieldInfo.IsGroupByDefault,
                        IsUsed = rptFieldInfo.IsUsed,
                        IsGroupable = rptFieldInfo.IsGroupable,
                        IsSummarizable = rptFieldInfo.IsSummarizable,
                        FieldValueExpression = rptFieldInfo.FieldValueExpression,
                        CoverageCode = rptFieldInfo.CoverageCode,
                        IsClientSpecificCode = rptFieldInfo.IsClientSpecificCode
                    });
                    fields.Add(rptField);
                }
            }

            return fields;
        }
        
        private List<RptField> getSummarizeFieldList(List<RptFieldInfo> list)
        {
            List<RptField> userSummarizeFieldList = new List<RptField>();

            foreach (RptFieldInfo summarizeField in list)
            {
                RptField newSummarizeField = (new RptField
                {
                    ID = summarizeField.ID,
                    ReportFieldID = summarizeField.ReportFieldID,
                    ReportID = summarizeField.ReportID,
                    Name = summarizeField.Name,
                    SQLName = summarizeField.SQLName,
                    DataType = (DataTypeEnum)summarizeField.DataType,
                    ColumnOrder = summarizeField.ColumnOrder,
                    ColumnWidthFactor = summarizeField.ColumnWidthFactor,
                    SortOrder = summarizeField.SortOrder,
                    SortDirection = (SortDirectionEnum)summarizeField.SortDirection,
                    GroupOrder = summarizeField.GroupOrder,
                    GroupSummaryExpression = summarizeField.GroupSummaryExpression,
                    IncludePageBreak = summarizeField.IncludePageBreak,
                    IsDisplayInReport = summarizeField.IsDisplayInReport,
                    IsGroupByDefault = summarizeField.IsGroupByDefault,
                    FieldValueExpression = summarizeField.FieldValueExpression
                });

                userSummarizeFieldList.Add(newSummarizeField);
            }

            return userSummarizeFieldList;
        }

        private void saveFile(string fileSaveName, FileMode fileMode, Stream inputStream)
        {
            FileStream fs = null;
            try
            {
                fs = File.Open(fileSaveName, fileMode, FileAccess.Write);
            }
            catch (Exception ex)
            {
                Logger.Write(ex, MethodBase.GetCurrentMethod(), "Can't read or create file:", fileSaveName);
            }

            if (fs != null)
            {
                byte[] buffer = new byte[4096];
                bool isValid = true;
                fs.Seek(fileMode == FileMode.Append ? fs.Length : 0, SeekOrigin.Begin);
                while (isValid)
                {
                    int read = inputStream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                    {
                        isValid = false;
                    }
                    fs.Write(buffer, 0, read);
                }
                fs.Close();
            }
        }

        private static string addZeroIfNeed(int value)
        {
            if (value < 10)
            {
                return string.Format("0{0}", value);
            }
            return value.ToString();
        }

        private Dictionary<string, object> returnFilterResult(FilterComboObj filterComboObj, IList itemList)
        {
            Dictionary<string, object> resultDictionary = new Dictionary<string, object>();
            resultDictionary.Add("ResultList", itemList);
            resultDictionary.Add("FilterValue", filterComboObj.FilterValue);
            resultDictionary.Add("IsTruncated", filterComboObj.IsTruncated);
            resultDictionary.Add("MaxRecordCount", filterComboObj.MaxRecordCount);
            return resultDictionary;
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> UpdateDocumentStatus(int releaseNoteID)
        {
            ReportDomainService domainService = new ReportDomainService();
            return domainService.UpdateDocumentStatus(releaseNoteID, UserContext.UserID, UserContext.AssociationNumber, UserContext.ID);
        }
        
        [RFAjaxAccessible]
        public Dictionary<string, object> GetReleaseNotesData()
        {
            ReportDomainService domainService = new ReportDomainService();
            return domainService.GetReleaseNotesData(UserContext.UserID, UserContext.AssociationNumber, UserContext.ID);
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> GetDeliveredReportsData()
        {
            ReportDomainService domainService = new ReportDomainService();
            return domainService.GetDeliveredReportsData(UserContext.UserID, UserContext.AssociationNumber, UserContext.ID);
        }


        string _FileServerPath = string.Empty;
        public string FileServerPath
        {
            get
            {
                if (string.IsNullOrEmpty(_FileServerPath))
                {
                    _FileServerPath = ConfigurationManager.AppSettings["FileServerPath"].ToString();
                }

                return _FileServerPath;
            }
        }
        [RFAjaxAccessible]
        public bool DeleteDeliveredReport(object deleteReport)
        {
            UserRptOutput userReportOutput = JavaScriptConvert.DeserializeObject<UserRptOutput>(JavaScriptConvert.SerializeObject(deleteReport));
            if (userReportOutput != null)
            {
                var file = Path.Combine(FileServerPath, UserContext.UserID, userReportOutput.FileName);

                try
                {
                      if (File.Exists(file))     File.Delete(file);
                }
                catch
                {
                    return false;
                }

                try
                {
                    ReportDomainService service = new ReportDomainService();
                    service.DeleteDeliveredReport(userReportOutput);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
        
        [RFAjaxAccessible]
        public Dictionary<string, object> GetDeliveredReportsDataByPage(int currentPage)
        {
            ReportDomainService domainService = new ReportDomainService();
            return domainService.GetDeliveredReportsDataByPage(UserContext.UserID, UserContext.AssociationNumber, UserContext.ID, currentPage);
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> GetAllData(int reportID, int userReportID)
        {
            ReportDomainService domainService = new ReportDomainService();
            return domainService.GetAllData(reportID, userReportID, UserContext.UserID, UserContext.AssociationNumber, UserContext.ID);
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> GetHeaderData()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("userName", UserContext.Name);
            result.Add("assnNumber", UserContext.AssociationNumber);

            AuthenticationService autService = new AuthenticationService();
            result.Add("ClientList", autService.GetUserAssociations(UserContext.UserID));
            return result;
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> SwitchClient(string AssociationNumber)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            UserContext.AssociationNumber = AssociationNumber;
            return result;

        }

        [RFAjaxAccessible]
        public Dictionary<string, object> DeleteReport(int reportId, int userReportID)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            ReportDomainService domainService = new ReportDomainService();
            domainService.DeleteReport(userReportID, reportId);
            return result;
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> GetAllDataForGrid()
        {
            ReportDomainService domainService = new ReportDomainService();
            try
            {
                return domainService.GetAllDataForGrid(UserContext.UserID, UserContext.AssociationNumber, UserContext.ID);
            }
            catch (Exception e)
            {
                Logger.Write(e, MethodBase.GetCurrentMethod(), e.Message, "-1");
                throw;
            }
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> GetSchedules(int userReportID)
        {
            ReportDomainService domainService = new ReportDomainService();
            return domainService.GetSchedules(UserContext.UserID, UserContext.AssociationNumber, UserContext.ID, userReportID);
        }
        
        [RFAjaxAccessible]
        public Dictionary<string, object> ValidatePageBreak(int reportId, object postParams, bool summaryOnly, bool customReport)
        {
            ReportBuilderService service = new ReportBuilderService();
            Dictionary<string, object> result = new Dictionary<string, object>();
            Dictionary<string, object> parameter = new Dictionary<string, object>();

            List<Dictionary<string, object>> parameters =
                JavaScriptConvert.DeserializeObject<List<Dictionary<string,object>>>(
                    JavaScriptConvert.SerializeObject(postParams));

            string client = String.Empty;

            foreach (Dictionary<string, object> item in  parameters)
            {
                string name = "";
                string value = "";
                foreach(KeyValuePair<string,object> dItem in item)
                {
                    if (dItem.Key == "Name")
                    {
                        name = dItem.Value.ToString();
                    }

                    if (dItem.Key == "Value")
                    {
                        value = dItem.Value.ToString();
                    }
                }

                parameter.Add(name, value);
            }
            
            List<object> returnItems =  service.GetParametersTurnOffPageBreak(reportId, parameter, summaryOnly, (int)FormatTypeEnum.Excel, customReport);

            if (returnItems.Count > 0)
            {
                int indxSp = returnItems.IndexOf("#");
                string spRowCount = returnItems.ElementAt(indxSp + 1).ToString();
                returnItems.RemoveAt(indxSp);
                returnItems.RemoveAt(indxSp);
                FilterService filterService = new FilterService();
                client = filterService.VerifyTurnOffPageBreak(spRowCount, returnItems);
                
            }

            result.Add("alert", client);

            return result;
        } 

        [RFAjaxAccessible]
        public Dictionary<string, object> GetCoverages()
        {
            FilterService filterService = new FilterService();
            FilterComboObj filterComboObj = filterService.GetCoverages(UserContext.UserID);
            return returnFilterResult(filterComboObj, filterComboObj.CoverageList);
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> GetStateOfJurisdiction(bool isAll)
        {
            FilterService filterService = new FilterService();
            FilterComboObj filterComboObj = filterService.GetStateOfJurisdiction(isAll ? null : UserContext.AssociationNumber);
            return returnFilterResult(filterComboObj, filterComboObj.StateOfJurisdictionsList);
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> GetClientAnalysis1(string filterValue)
        {
            FilterService filterService = new FilterService();
            FilterComboObj filterComboObj = filterService.GetClientAnalysis1(UserContext.UserID, UserContext.AssociationNumber, filterValue);
            return returnFilterResult(filterComboObj, filterComboObj.ClientAnalysesList);
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> GetClientAnalysis2(string filterValue)
        {
            FilterService filterService = new FilterService();
            FilterComboObj filterComboObj = filterService.GetClientAnalysis2(UserContext.UserID, UserContext.AssociationNumber, filterValue);
            return returnFilterResult(filterComboObj, filterComboObj.ClientAnalysesList);
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> GetClientAnalysis3(string filterValue)
        {
            FilterService filterService = new FilterService();
            FilterComboObj filterComboObj = filterService.GetClientAnalysis3(UserContext.UserID, UserContext.AssociationNumber, filterValue);
            return returnFilterResult(filterComboObj, filterComboObj.ClientAnalysesList);
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> GetClientAnalysis4(string filterValue)
        {
            FilterService filterService = new FilterService();
            FilterComboObj filterComboObj = filterService.GetClientAnalysis4(UserContext.UserID, UserContext.AssociationNumber, filterValue);
            return returnFilterResult(filterComboObj, filterComboObj.ClientAnalysesList);
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> GetClientAnalysis5(string filterValue)
        {
            FilterService filterService = new FilterService();
            FilterComboObj filterComboObj = filterService.GetClientAnalysis5(UserContext.UserID, UserContext.AssociationNumber, filterValue);
            return returnFilterResult(filterComboObj, filterComboObj.ClientAnalysesList);
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> GetClasses(string coverages)
        {
            FilterService filterService = new FilterService();
            List<ClassObj> filterComboObj = filterService.GetClasses(UserContext.AssociationNumber, coverages);
            return new Dictionary<string, object> { { "ResultList", filterComboObj }};
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> GetMembers(string filterValue, string selectedValue)
        {
            FilterService filterService = new FilterService();
            FilterComboObj filterComboObj = filterService.GetMembers(UserContext.UserID, UserContext.AssociationNumber, filterValue, selectedValue);
            return returnFilterResult(filterComboObj, filterComboObj.MemberList);
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> GetLocations(string filterValue, string selectedValue, string memberNumber)
        {
            FilterService filterService = new FilterService();
            FilterComboObj filterComboObj = filterService.GetLocations(UserContext.UserID, UserContext.AssociationNumber, memberNumber, filterValue, selectedValue);
            return returnFilterResult(filterComboObj, filterComboObj.LocationList);
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> GetGroupCode1s(string filterValue, string selectedValue, string memberNumber, string location)
        {
            FilterService filterService = new FilterService();
            FilterComboObj filterComboObj = filterService.GetGroup1Codes(UserContext.UserID, UserContext.AssociationNumber, memberNumber, location, filterValue, selectedValue);
            return returnFilterResult(filterComboObj, filterComboObj.GroupCodeList);
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> GetGroupCode2s(string filterValue, string selectedValue, string memberNumber, string location, string group1Code)
        {
            FilterService filterService = new FilterService();
            FilterComboObj filterComboObj = filterService.GetGroup2Codes(UserContext.UserID, UserContext.AssociationNumber, memberNumber, location, group1Code, filterValue, selectedValue);
            return returnFilterResult(filterComboObj, filterComboObj.GroupCodeList);
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> GetGroupCode3s(string filterValue, string selectedValue, string memberNumber, string location, string group1Code, string group2Code)
        {
            FilterService filterService = new FilterService();
            FilterComboObj filterComboObj = filterService.GetGroup3Codes(UserContext.UserID, UserContext.AssociationNumber, memberNumber, location, group1Code, group2Code, filterValue, selectedValue);
            return returnFilterResult(filterComboObj, filterComboObj.GroupCodeList);
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> GetGroupCode4s(string filterValue, string selectedValue, string memberNumber, string location, string group1Code, string group2Code, string group3Code)
        {
            FilterService filterService = new FilterService();
            FilterComboObj filterComboObj = filterService.GetGroup4Codes(UserContext.UserID, UserContext.AssociationNumber, memberNumber, location, group1Code, group2Code, group3Code, filterValue, selectedValue);
            return returnFilterResult(filterComboObj, filterComboObj.GroupCodeList);
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> RunStandartReport(int reportID, bool isTurnOffPageBreak, bool includeTitlePage, int formatTypeId, int reportLayoutStyleID, object parameters)
        {
            Dictionary<string, object> resultDictionary = new Dictionary<string, object>();
            try
            {
               
                List<RptParameter> list = JavaScriptConvert.DeserializeObject<List<RptParameter>>(JavaScriptConvert.SerializeObject(parameters));
                ReportGenerationService reportGenerationService = new ReportGenerationService();
                GenerateStandardReportRequest generateStandardReportRequest = new GenerateStandardReportRequest();
                generateStandardReportRequest.AssociationNumber = UserContext.AssociationNumber;
                generateStandardReportRequest.FormatTypeId = formatTypeId;
                generateStandardReportRequest.IncludeTitlePage = includeTitlePage;
                generateStandardReportRequest.IsTurnOffPageBreak = isTurnOffPageBreak;
                generateStandardReportRequest.Parameters = list;
                generateStandardReportRequest.ReportID = reportID;
                generateStandardReportRequest.ReportLayoutStyleID = reportLayoutStyleID;
                generateStandardReportRequest.UserID = UserContext.ID;
                resultDictionary.Add("Result", reportGenerationService.GenerateReportExecute((new Guid()).ToString(), generateStandardReportRequest));
            }
            catch (Exception e)
            {
                Logger.Write(e, MethodBase.GetCurrentMethod(), e.Message, "-1");
                throw;
            }
            
            return resultDictionary;
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> RunCustomReport(int reportID, bool isTurnOffPageBreak, bool includeTitlePage, int formatTypeId, int reportLayoutStyleID, object parameters, bool isSummaryOnly, object fields, object summarizeFields)
        {
            Dictionary<string, object> resultDictionary = new Dictionary<string, object>();

            try
            {
                List<RptParameter> list = JavaScriptConvert.DeserializeObject<List<RptParameter>>(JavaScriptConvert.SerializeObject(parameters));
                List<RptField> listFields = JavaScriptConvert.DeserializeObject<List<RptField>>(JavaScriptConvert.SerializeObject(fields));
                List<RptField> summFields = JavaScriptConvert.DeserializeObject<List<RptField>>(JavaScriptConvert.SerializeObject(summarizeFields));
                ReportGenerationService reportGenerationService = new ReportGenerationService();
                GenerateCustomReportRequest generateCustomReportRequest = new GenerateCustomReportRequest();
                generateCustomReportRequest.UserID = UserContext.ID;
                generateCustomReportRequest.AssociationNumber = UserContext.AssociationNumber;
                generateCustomReportRequest.FormatTypeId = formatTypeId;
                generateCustomReportRequest.ReportID = reportID;
                generateCustomReportRequest.ReportLayoutStyleID = reportLayoutStyleID;
                generateCustomReportRequest.Parameters = list;
                generateCustomReportRequest.IncludeTitlePage = includeTitlePage;
                generateCustomReportRequest.Fields = listFields;
                generateCustomReportRequest.UserSummarizeRptField = summFields;
                generateCustomReportRequest.IsSummaryOnly = isSummaryOnly;
                resultDictionary.Add("Result", reportGenerationService.GenerateReportExecute((new Guid()).ToString(), generateCustomReportRequest));
            }
            catch (Exception e)
            {
                Logger.Write(e, MethodBase.GetCurrentMethod(), e.Message, "-1");
                throw;
            }
            
            return resultDictionary;   
        }
        
        [RFAjaxAccessible]
        public Dictionary<string, object> SaveStandartReport(int reportID, bool isTurnOffPageBreak, bool includeTitlePage, int formatTypeId, int reportLayoutStyleID, object parameters, string userReportName, bool saveAsNew, int userReportId)
        {
            Dictionary<string, object> resultDictionary = new Dictionary<string, object>();
            ReportBuilderService svc = new ReportBuilderService();
            List<RptParameter> list = JavaScriptConvert.DeserializeObject<List<RptParameter>>(JavaScriptConvert.SerializeObject(parameters));
            if (saveAsNew)
            {
                int newUserReportId = svc.SaveStandardUserReport(UserContext.ID, reportID, reportLayoutStyleID, formatTypeId, userReportName, list, UserContext.AssociationNumber, false, isTurnOffPageBreak, includeTitlePage);
            }
            else
            {
                svc.UpdateUserReport(userReportId, reportLayoutStyleID, formatTypeId, userReportName, false, list, isTurnOffPageBreak, includeTitlePage);
            }
            return resultDictionary;
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> SaveCustomReport(int reportID, bool isTurnOffPageBreak, bool includeTitlePage, int formatTypeId, int reportLayoutStyleID, object parameters, string userReportName, bool saveAsNew, int userReportId, bool isSummaryOnly, object fields, object summarizeFields)
        {
            Dictionary<string, object> resultDictionary = new Dictionary<string, object>();
            ReportBuilderService svc = new ReportBuilderService();
            List<RptParameter> list = JavaScriptConvert.DeserializeObject<List<RptParameter>>(JavaScriptConvert.SerializeObject(parameters));
            List<RptFieldInfo> listFields = JavaScriptConvert.DeserializeObject<List<RptFieldInfo>>(JavaScriptConvert.SerializeObject(fields));
            List<RptFieldInfo> summFields = JavaScriptConvert.DeserializeObject<List<RptFieldInfo>>(JavaScriptConvert.SerializeObject(summarizeFields));
            if (saveAsNew)
            {
                svc.SaveCustomUserReport(UserContext.ID, reportID, formatTypeId, reportLayoutStyleID, userReportName, isSummaryOnly, getUserReportFieldList(saveAsNew, listFields), getSummarizeFieldList(summFields), list, UserContext.AssociationNumber, false, includeTitlePage);
            }
            else
            {
                svc.UpdateUserReport(userReportId, reportLayoutStyleID, formatTypeId, userReportName, isSummaryOnly, list, false, includeTitlePage);

                svc.UpdateCustomUserReport(userReportId, getUserReportFieldList(saveAsNew, listFields), UserContext.ID, getSummarizeFieldList(summFields));
            }
            return resultDictionary;
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> UploadAttachmentsWithUploadStatus()
        {
            int chunk = 0;
            int chunks = 0;
            string fileName = "";
            int maxFileAge = 60 * 60;
            int count = 1;

            if (currentRequest.Params.AllKeys.Contains("chunk"))
            {
                int chunkOut = 0;
                if (int.TryParse(currentRequest.Params["chunk"].ToString(), out chunkOut))
                {
                    chunk = chunkOut;
                }
            }
            if (currentRequest.Params.AllKeys.Contains("chunks"))
            {
                int chunksOut = 0;
                if (int.TryParse(currentRequest.Params["chunks"].ToString(), out chunksOut))
                {
                    chunks = chunksOut;
                }
            }

            if (currentRequest.Params.AllKeys.Contains("name"))
            {
                fileName = currentRequest.Params["name"].ToString();
            }


            var folder = "CompanyLogo";
            string[] folders = ConfigurationManager.AppSettings[Const.LOGO_IMAGE_URL].Split(sep, StringSplitOptions.RemoveEmptyEntries);

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[Const.LOGO_IMAGE_URL]))
                folder = folders[folders.Length - 1];

            FileInfo fileInfo = new FileInfo(Path.Combine(currentRequest.PhysicalApplicationPath + folder, fileName));
            string ext = fileInfo.Extension;
            string fileName_a = fileInfo.Name;

            if (chunks < 2 && File.Exists(Path.Combine(currentRequest.PhysicalApplicationPath + folder, fileName)))
            {
                while (File.Exists(Path.Combine(currentRequest.PhysicalApplicationPath + folder, string.Format("{0}{1}.{2}", fileName_a, count, ext))))
                {
                    count++;
                }

                fileName = string.Format("{0}{1}.{2}", fileName_a, count, ext);
            }

            for (int i = 1; i <= count; i++)
            {
                if (File.Exists(Path.Combine(currentRequest.PhysicalApplicationPath + folder, string.Format("{0}{1}.{2}", fileName_a, count, ext))))
                {
                    File.Delete(Path.Combine(currentRequest.PhysicalApplicationPath + folder, string.Format("{0}{1}.{2}", fileName_a, count, ext)));
                }
            }

            HttpPostedFile file = null;
            try
            {
                file = currentRequest.Files.Get("file");
            }
            catch (Exception ex) { }

            string fileSaveName = "";

            if (file != null)
            {
                fileSaveName = Path.Combine(currentRequest.PhysicalApplicationPath + folder, fileName);

                if (currentRequest.ContentType.Contains("multipart"))
                {
                    if (File.Exists(fileSaveName))
                    {
                        saveFile(fileSaveName, FileMode.Append, file.InputStream);
                    }
                    else
                    {
                        saveFile(fileSaveName, FileMode.Create, file.InputStream);
                    }
                }
                else
                {
                    saveFile(fileSaveName, FileMode.Create, file.InputStream);
                }
            }

            return new Dictionary<string, object> { { "jsonrpc", "2.0" }, { "result", null }, { "id", "id" }};
        }
        
        [RFAjaxAccessible]
        public Dictionary<string, object> UploadAttachmentsFileUpload(string fileName, string realFileName)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            var folder = "CompanyLogo";
            string[] folders = ConfigurationManager.AppSettings[Const.LOGO_IMAGE_URL].Split(sep, StringSplitOptions.RemoveEmptyEntries);

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[Const.LOGO_IMAGE_URL]))
                folder = folders[folders.Length - 1];

            DateTime uploadedDate = DateTime.Now;
            string name = string.Format("{0}_{1}.png", UserContext.UserID, UserContext.AssociationNumber);
            string fileSaveName = Path.Combine(currentRequest.PhysicalApplicationPath + folder, name);
            string existFileName = Path.Combine(currentRequest.PhysicalApplicationPath + folder, fileName);
            if (File.Exists(existFileName))
            {
                try
                {
                    if (File.Exists(fileSaveName)) File.Delete(fileSaveName);

                    File.Copy(existFileName, fileSaveName);
                    File.Delete(existFileName);
                }
                catch (Exception ex)
                {
                    Logger.Write(ex, MethodBase.GetCurrentMethod(), "Error saving file.", UserContext.UserID);
                    throw ex;
                }
            }

            FileInfo file = new FileInfo(fileSaveName);

            result.Add("FileSaveName", Path.Combine(ConfigurationManager.AppSettings[Const.LOGO_IMAGE_URL], name + "?" + Guid.NewGuid()));
            result.Add("FileName", fileSaveName);
            result.Add("DateUploaded", string.Format("{0}/{1}/{2}", addZeroIfNeed(uploadedDate.Month),
                                                               addZeroIfNeed(uploadedDate.Day), uploadedDate.Year));
            result.Add("FileSize", Math.Round((decimal)file.Length / 1024));
            result.Add("FileInfo", "");

            return result;
        }
        
        [RFAjaxAccessible]
        public Dictionary<string, object> GetLogoImage()
        {
            LogoManager logoManager = new LogoManager();
            Guid guid = new Guid();
            Dictionary<string, object> result = new Dictionary<string, object>();

            if(logoManager.Exists(UserContext.AssociationNumber, UserContext.UserID))
            {
                result.Add("FileSaveName", Path.Combine(ConfigurationManager.AppSettings[Const.LOGO_IMAGE_URL], string.Format("{0}_{1}.png?{2}", UserContext.UserID, UserContext.AssociationNumber, Guid.NewGuid())));
                 
            }
            else
            {
                result.Add("FileSaveName", string.Empty);
            }

            return result;
        }
        
        [RFAjaxAccessible]  
        public Dictionary<string, object> RemoveCustomLogo()
        {
            LogoManager logoManager = new LogoManager();

            Dictionary<string, object> result = new Dictionary<string, object>();

            if (logoManager.Exists(UserContext.AssociationNumber, UserContext.UserID))
            {
                logoManager.Delete(UserContext.AssociationNumber, UserContext.UserID);
                result.Add("Status", "OK");
            }
            else
            {
                result.Add("Status", "NOT EXISTS");
            }

            return result;
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> GetCheckpointsConfigurationData()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            ReportDomainService domainService = new ReportDomainService();

            result.Add("Checkpoints", domainService.GetCheckpoints().ToList());
            result.Add("ReportFolders", domainService.GetReportFolders().ToList());

            return result;
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> GetReportsForCheckpoint(string checkpointId)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            ReportDomainService domainService = new ReportDomainService();

            result.Add("Reports", domainService.GetReportsForCheckpointConfiguration(checkpointId).ToList());

            return result;
        }

        [RFAjaxAccessible]
        public Dictionary<string, object> SetReportsForCheckpoint(string checkpointId, object reports)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            ReportDomainService domainService = new ReportDomainService();
            ReportBuilderService reportBuilderService = new ReportBuilderService();

            string array = JavaScriptConvert.SerializeObject(reports);

            JavaScriptArray arr = JavaScriptConvert.DeserializeObject(array) as JavaScriptArray;
            JavaScriptSerializer jser = new JavaScriptSerializer();



            List<Rpt> list = (List<Rpt>)jser.Deserialize<List<Rpt>>(JavaScriptConvert.SerializeObject(arr));

            reportBuilderService.SetReportTypeCheckpoints(list,checkpointId);

            result.Add("status","ok");

            return result;
        }
    }
}