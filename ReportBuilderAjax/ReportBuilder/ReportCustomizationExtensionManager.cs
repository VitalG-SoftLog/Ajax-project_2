using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.ReportingServices.RdlObjectModel;
using Microsoft.ReportingServices.RdlObjectModel.Serialization;
using ReportBuilderAjax.Web.Common;
using ReportBuilderSL.Ajax;
using RDL = Microsoft.ReportingServices.RdlObjectModel;
using System.Text;
using DataSet = Microsoft.ReportingServices.RdlObjectModel.DataSet;
using Logger = Microsoft.Practices.EnterpriseLibrary.Logging.Logger;

namespace ReportBuilderAjax.Web.ReportBuilder
{
    public class ReportCustomizationExtensionManager
    {
        private byte[] _reportDefinition;
        private RDL.Report _report = null;
        private RdlSerializer _serializer = new RdlSerializer();
        private Dictionary<string, object> _queryParameters = new Dictionary<string, object>();
        private List<RptField> _rptFields = new List<RptField>();
        private List<RptField> _userSummarizeRptFields = new List<RptField>();
        private List<RptParameter> _rptParameters = new List<RptParameter>();
        static int _textboxNum = 10000;
        private int _userReportID = -1;
        private int _reportID = -1;
        private int _reportLayoutStyleID = -1;
        private string _reportLayoutStyle = string.Empty;
        private bool _isCustomReport = false;
        private bool _isSummaryOnly = false;
        private bool _isTurnOffPageBreak = false;
        private bool _includeTitlePage = true;
        private int _formatTypeID = (int)FormatTypeEnum.PDF;
        private string _filterKey = "";
        private string _sortByInfo = string.Empty;
        private string _groupByInfo = string.Empty;
        private string _RdlHeaderDate = "";
        private TablixMember tablixMemberParent = null;
        private const string disclamerTxt = "* All reports that are run through the current date show data as of midnight the previous day";

        private const string REPORT_KEY_TABLIX_NAME = "ReportKey_Tablix";
        private const string REPORT_TABLE1_NAME = "table1";
        private const string REPORT_TABLE2_NAME = "table2";
        private const string REPORT_TABLE_SUMMARYLEVEL1_NAME = "SummaryLevel1Name";
        private const string REPORT_EXCEL_HEADER_TABLIX_NAME = "ExcelHeader_Tablix";
        private const string REPORT_GRAPH_01_NAME = "Chart01";
        private const string REPORT_GRAPH_02_NAME = "Chart02";
        private const string REPORT_TABLE1_E_NAME = "table1e";
        private const int MAX_STRING_LENGHT_LOWERCASE = 133;
        private const int MAX_STRING_LENGHT_UPPERCASE = 105;
        private const string DISCLAMER_OCURRENCE = "Disclaimer: Occurrence claims may not calculate correctly in the custom layout.";
        private const string REPORT_PARAMETER_FOOTNOTES = "footnotes";

        public ReportCustomizationExtensionManager(byte[] reportDefinition, Dictionary<string, object> queryParameters)
        {
            _reportDefinition = reportDefinition;
            _queryParameters = queryParameters;
        }

        public byte[] CustomizeReport()
        {
            MemoryStream mstream = null;
            try
            {
                using (mstream = new MemoryStream(_reportDefinition))
                {
                    mstream.Position = 0;
                    _report = _serializer.Deserialize(mstream);
                }

                foreach (KeyValuePair<string, object> parameter in _queryParameters)
                {
                    if (parameter.Key.ToLower() == "userreportid")
                    {
                        if (parameter.Value != null)
                        {
                            int.TryParse(parameter.Value.ToString(), out _userReportID);
                        }
                    }
                }

                ReportProjectDBEntities entities = new ReportProjectDBEntities();
                var userRpt = (from ur in entities.UserReport
                                 .Include("Report")
                                 .Include("ReportLayoutStyle")
                               where ur.UserReportID == _userReportID
                               select ur).FirstOrDefault();

                if (userRpt != null)
                {
                    _reportID = userRpt.ReportID;
                    _reportLayoutStyleID = userRpt.ReportLayoutStyleID; 
                    _isCustomReport = userRpt.IsCustom;
                    _isSummaryOnly = userRpt.IsSummaryOnly;
                    _formatTypeID = userRpt.FormatTypeID;
                    _reportLayoutStyle = userRpt.ReportLayoutStyle.ReportLayoutStyleName;
                    _isTurnOffPageBreak = userRpt.IsTurnOffPageBreak;
                    _includeTitlePage = userRpt.IncludeTitlePage;
                }

                if (_isCustomReport)
                {
                    RdlManager rdlManager = new RdlManager();
                    rdlManager.CustomizeReport(userRpt, _report);
                }

                getReportInformation();                

                var rptObj = (from r in entities.Report
                              where r.ReportID == _reportID
                              select new { r.RdlHeaderDate }).FirstOrDefault();
                if (rptObj != null)
                {
                    _RdlHeaderDate = rptObj.RdlHeaderDate;
                }

                addReportHeader();

                if (_formatTypeID != (int)FormatTypeEnum.Excel)
                {
                    addReportFooter();
                }
                                
                if (_formatTypeID == (int)FormatTypeEnum.Excel && (_isTurnOffPageBreak || _isCustomReport))
                {
                    string result = verifyRowCount(_reportID, _queryParameters, _isSummaryOnly, _formatTypeID);
                    if (!string.IsNullOrEmpty(result))
                    {
                        verifyRowCountErrorBox(result);
                    }
                }

                addLogoImages(_queryParameters);
                //addHeaderDateInfo();
                _filterKey = getFilterKey(_userReportID);
                infoGroupBy();
                infoSortBy();
                addReportKeyTablix(_queryParameters, _isCustomReport);
                RemovePageBreaks(_isTurnOffPageBreak);
                //RemoveGroupDetail(_isTurnOffPageBreak);
                rePositionReportItems(_queryParameters);
                if (_isCustomReport)
                {
                    RdlManager rdlManager = new RdlManager();
                    List<RptField> rptList = rdlManager.GetClientSpecificCodeFields(userRpt.UserReportID);
                    if (rptList.Count() > 0)
                    {
                        ChangeDataSetFields(rptList);
                    }

                    if (_reportID == (int)ReportEnum.ReserveChangeReport)
                    {
                        var groupingCount = 0;
                        foreach (var item in _queryParameters)
                        {
                            if (item.Key == "CustomGrouping")
                            {
                                groupingCount = item.Value.ToString().Split(',').Length;
                                var igrouping = 1;
                                while (igrouping <= groupingCount)
                                {
                                    var newDataField = "TotalLevel0" + igrouping.ToString();
                                    ChangeDataSetFields(newDataField);
                                    igrouping++;
                                }
                            }
                        }
                    }           
                }
            }
            catch (Exception ex)
            {
                StringBuilder logMessage = new StringBuilder();
                logMessage.AppendLine();
                logMessage.AppendFormat("{0} | UserReportID = {1} | Report Format = {2} | Report Failed:",
                    System.DateTime.Now.ToString(), _userReportID, Enum.GetName(typeof(FormatTypeEnum), _formatTypeID));
                logMessage.AppendLine();
                logMessage.AppendLine(ex.ToString());
                Logger.Write(logMessage);
                throw ex;
            }

            using (mstream = new MemoryStream())
            {
                _serializer.Serialize(mstream, _report);
            }

            SaveReportRdlFile();

            return mstream.ToArray();
        }

        private void ChangeDataSetFields(string newDataField)
        {
            foreach (var dataSet in _report.DataSets)
            {
                if (dataSet.Name.Contains("DataSet2"))
                {                    
                        Field fieldAdd = new Field();
                        fieldAdd.Name = newDataField;
                        fieldAdd.DataField = newDataField;
                        dataSet.Fields.Add(fieldAdd);                    
                }
            }
        }

        private void ChangeDataSetFields(List<RptField> rptList)
        { 
              foreach (var dataSet in _report.DataSets)
              {
                  if(dataSet.Name.Contains("DataSet2"))
                  {
                      foreach (var field in rptList)
                      {
                          Field fieldAdd = new Field();
                           fieldAdd.Name = field.SQLName;
                           fieldAdd.DataField = field.SQLName; 
                           dataSet.Fields.Add(fieldAdd);       
                      }
                  }             
              }                
        }

        private void RemovePageBreaks(bool removePageBreaks)
        {
            if (_report.Body != null)
            {
                int indexRemove = 0;
                Rectangle newItem = new Rectangle();

                foreach (var item in _report.Body.ReportItems)
                {
                    if (item.Name == REPORT_KEY_TABLIX_NAME && item.GetType().ToString().Contains("Tablix") && !_includeTitlePage)
                    {
                        var reportKeyTablix = (Tablix)item;
                        PageBreak pageBreak = new PageBreak();
                        pageBreak.BreakLocation = BreakLocations.Start;
                        reportKeyTablix.PageBreak = pageBreak;
                    }

                    if (item.Name == REPORT_TABLE1_NAME && item.GetType().ToString().Contains("Tablix") && !_includeTitlePage)
                    {
                        var tablix = (Tablix)item;
                        tablix.PageBreak = null;
                    }

                    if (item.Name == REPORT_TABLE1_NAME && item.GetType().ToString().Contains("Rectangle"))
                    {
                        if (!_includeTitlePage)
                        {
                            var stewardshipTablix = (Tablix)(((Rectangle)item).ReportItems).First();
                            if (stewardshipTablix.Name == REPORT_TABLE_SUMMARYLEVEL1_NAME)
                            {
                                stewardshipTablix.PageBreak = null;
                            }
                        }

                        List<ReportItem> removeReportItems =  new List<ReportItem>((((Rectangle)item).ReportItems));
                        indexRemove = GetIndexRemoveTable(_queryParameters);

                        if (indexRemove != 0)
                        {
                            int index = indexRemove;
                            while (index < 20)
                            {
                                removeReportItems.RemoveAt(indexRemove);
                                index = index + 1;
                            }
                            newItem.Name = "table1";
                            newItem.KeepTogether = false;
                            newItem.Left = new ReportSize(0.06242);
                            newItem.Top = new ReportSize(1.33907);
                            newItem.Width = new ReportSize(10.51567);
                            newItem.Height = new ReportSize(indexRemove * 0.1125);
                            newItem.ReportItems = removeReportItems;
                            
                        }
                    }
                }
                if (indexRemove != 0)
                {
                    _report.Body.ReportItems[1] = newItem;
                }

            }

            if (_report.Body == null || !removePageBreaks) return;
            foreach (var item in _report.Body.ReportItems)
            {
                if (item.Name == REPORT_TABLE1_NAME && item.GetType().ToString().Contains("Tablix"))
                {
                    var tablix = (Tablix)item;

                    foreach (var tablixRow in tablix.TablixRowHierarchy.TablixMembers)
                    {                            
                        RemovePageBreak(tablixRow);
                    }
                }
            }
        }

        //private void RemoveGroupDetail(bool removeGroupDetail)
        //{
        //    if (_report.Body == null || !removeGroupDetail) return;
        //    foreach (var item in _report.Body.ReportItems)
        //    {
        //        if (item.Name == REPORT_TABLE1_NAME && item.GetType().ToString().Contains("Tablix"))
        //        {
        //            var tablix = (Tablix)item;                 
        //            foreach (var tablixRow in tablix.TablixRowHierarchy.TablixMembers)
        //            {
        //                RemoveGroupDetail(tablixRow, tablix.TablixRowHierarchy.TablixMembers);                   
        //            }
        //        }
        //    }
        //}

        //private void RemoveGroupDetail(TablixMember tablixMember, IList<TablixMember> tablixMemberList)
        //{
        //    if (tablixMember.Group != null && tablixMember.Group.Name.Contains("Detail"))
        //    {                   
        //        Visibility visibility = new Visibility();
        //        visibility.Hidden = true;
        //        tablixMemberParent.Visibility = visibility;
        //    }
        //    else
        //    {
        //        if (tablixMember.Group != null && tablixMember.Group.Name != null) tablixMemberParent = tablixMember;
        //    }

        //    if (tablixMember == null  || tablixMember.TablixMembers.Count == 0) return;

        //    foreach (var member in tablixMember.TablixMembers)
        //    {
        //        RemoveGroupDetail(member, tablixMember.TablixMembers);
        //    }
        //}

        private void RemovePageBreak(TablixMember tablixMember)
        {
            if (tablixMember.Group != null)
            {
                tablixMember.Group.PageBreak = null;
            }

            if (tablixMember.TablixMembers.Count == 0) return;

            foreach (var member in tablixMember.TablixMembers)
            {
                member.KeepTogether = false;
                foreach (var tMember in member.TablixMembers)
                {
                    tMember.KeepTogether = false;
                }
                RemovePageBreak(member);
            }
        }

        private int GetIndexRemoveTable(IDictionary<string, object> parameters)
        {
            int indexRemove = 0;

            foreach (KeyValuePair<string, object> keyValuePair in parameters)
            {
                switch (keyValuePair.Key.ToLower())
                {
                    case "summarylevel2name":
                        if (String.IsNullOrEmpty(keyValuePair.Value.ToString()))
                        {
                            indexRemove = 1;
                        }
                        break;
                    case "summarylevel3name":
                        if (String.IsNullOrEmpty(keyValuePair.Value.ToString()))
                        {
                            indexRemove = 2;
                        }
                        break;
                    case "summarylevel4name":
                        if (String.IsNullOrEmpty(keyValuePair.Value.ToString()))
                        {
                            indexRemove = 3;
                        }
                        break;
                    case "summarylevel5name":
                        if (String.IsNullOrEmpty(keyValuePair.Value.ToString()))
                        {
                            indexRemove = 4;
                        }
                        break;
                    case "summarylevel6name":
                        if (String.IsNullOrEmpty(keyValuePair.Value.ToString()))
                        {
                            indexRemove = 5;
                        }
                        break;
                    case "summarylevel7name":
                        if (String.IsNullOrEmpty(keyValuePair.Value.ToString()))
                        {
                            indexRemove = 6;
                        }
                        break;
                    case "summarylevel8name":
                        if (String.IsNullOrEmpty(keyValuePair.Value.ToString()))
                        {
                            indexRemove = 7;
                        }
                        break;
                    case "summarylevel9name":
                        if (String.IsNullOrEmpty(keyValuePair.Value.ToString()))
                        {
                            indexRemove = 8;
                        }
                        break;
                    case "summarylevel10name":
                        if (String.IsNullOrEmpty(keyValuePair.Value.ToString()))
                        {
                            indexRemove = 9;
                        }
                        break;
                    case "summarylevel11name":
                        if (String.IsNullOrEmpty(keyValuePair.Value.ToString()))
                        {
                            indexRemove = 10;
                        }
                        break;
                    case "summarylevel12name":
                        if (String.IsNullOrEmpty(keyValuePair.Value.ToString()))
                        {
                            indexRemove = 11;
                        }
                        break;
                    case "summarylevel13name":
                        if (String.IsNullOrEmpty(keyValuePair.Value.ToString()))
                        {
                            indexRemove = 12;
                        }
                        break;
                    case "summarylevel14name":
                        if (String.IsNullOrEmpty(keyValuePair.Value.ToString()))
                        {
                            indexRemove = 13;
                        }
                        break;
                    case "summarylevel15name":
                        if (String.IsNullOrEmpty(keyValuePair.Value.ToString()))
                        {
                            indexRemove = 14;
                        }
                        break;
                    case "summarylevel16name":
                        if (String.IsNullOrEmpty(keyValuePair.Value.ToString()))
                        {
                            indexRemove = 15;
                        }
                        break;
                    case "summarylevel17name":
                        if (String.IsNullOrEmpty(keyValuePair.Value.ToString()))
                        {
                            indexRemove = 16;
                        }
                        break;
                    case "summarylevel18name":
                        if (String.IsNullOrEmpty(keyValuePair.Value.ToString()))
                        {
                            indexRemove = 17;
                        }
                        break;
                    case "summarylevel19name":
                        if (String.IsNullOrEmpty(keyValuePair.Value.ToString()))
                        {
                            indexRemove = 18;
                        }
                        break;
                    case "summarylevel20name":
                        if (String.IsNullOrEmpty(keyValuePair.Value.ToString()))
                        {
                            indexRemove = 19;
                        }
                        break;
                }


                if (indexRemove != 0)
                {
                    return indexRemove;
                }

            }

            return indexRemove;
        }

        private double getSizeTablix(string tablixName)
        {
            var tablixSizeHeight = 0.00;

            if (_report.Body != null)
            {
                foreach (ReportItem item in _report.Body.ReportItems)
                {
                    if (item.Name == tablixName && item.GetType().ToString().Contains("Tablix"))
                    {
                        Tablix tablix = (Tablix)item;
                        foreach (var tablixRow in tablix.TablixBody.TablixRows)
                        {
                            tablixSizeHeight = tablixSizeHeight + tablixRow.Height.Value;                            
                        }
                    }

                    if (item.Name == tablixName && item.GetType().ToString().Contains("Chart"))
                    {
                        Chart chart = (Chart)item;
                        tablixSizeHeight = tablixSizeHeight + chart.Height.Value;
                    }

                    if (item.Name == tablixName && item.GetType().ToString().Contains("Rectangle"))
                    {
                        Rectangle rectangle = (Rectangle)item;
                        tablixSizeHeight = rectangle.Height.Value + 0.025;
                    }
                }

            }


            return tablixSizeHeight;
        }

        private void rePositionReportItems(Dictionary<string, object> parameters)
        {
            int numberRowsTable2 = 0;
            double graphHeight = 0.0;

            // if Reports were modified with new Layout
            var indxEH = findReportItemIndexOf(REPORT_EXCEL_HEADER_TABLIX_NAME);
            var headerHeight = indxEH >= 0 && _formatTypeID == (int)FormatTypeEnum.Excel ? new ReportSize(1.158 + 0.05, SizeTypes.Inch) : new ReportSize(0.0, SizeTypes.Inch);

            var table2Height = 0.0;

            var indxRKey = findReportItemIndexOf(REPORT_KEY_TABLIX_NAME);
            var reportKeyTablixHeight = getSizeTablix(REPORT_KEY_TABLIX_NAME) + 0.05;
            var mainTableHeight = getSizeTablix(REPORT_TABLE1_NAME) + 0.05;
            if (indxRKey >= 0)
            {
                // "table2 = Totals ";
                var indxT2 = findReportItemIndexOf(REPORT_TABLE2_NAME);
                if (indxT2 >= 0)
                {
                    if (_includeTitlePage)
                    {
                        _report.Body.ReportItems[indxT2].Top = new ReportSize(headerHeight.Value + reportKeyTablixHeight, SizeTypes.Inch);
                    }
                    else
                    {
                        if (_reportID != (int)ReportEnum.ManagedCareSavingsAndFees)
                        {
                            _report.Body.ReportItems[indxT2].Top = new ReportSize(headerHeight.Value + reportKeyTablixHeight + mainTableHeight, SizeTypes.Inch);
                        }
                        else
                        {
                            _report.Body.ReportItems[indxT2].Top = new ReportSize(headerHeight.Value + mainTableHeight, SizeTypes.Inch);
                        }
                    }
                    numberRowsTable2 = (int)(_report.Body.ReportItems[indxT2].Height.Value / 0.22);
                    table2Height = getSizeTablix(REPORT_TABLE2_NAME) + 0.05;
                }

                if (_reportID == (int)ReportEnum.ClaimLag)
                {

                    bool isVisibleChart01 = false;
                    bool isVisibleChart02 = false;
                    foreach (KeyValuePair<string, object> keyValuePair in parameters)
                    {
                        if (keyValuePair.Key.ToLower() == "ShowClaimantReportLagGraph".ToLower())
                        {
                            isVisibleChart01 = Convert.ToBoolean(keyValuePair.Value);
                        }
                        if (keyValuePair.Key.ToLower() == "ShowClaimReceivedLagGraph".ToLower())
                        {
                            isVisibleChart02 = Convert.ToBoolean(keyValuePair.Value);
                        }

                    }

                    var indxGraph01 = findReportItemIndexOf(REPORT_GRAPH_01_NAME);
                    if (indxGraph01 >= 0 && isVisibleChart01)
                    {
                        if (_includeTitlePage)
                        {
                            _report.Body.ReportItems[indxGraph01].Top = new ReportSize(headerHeight.Value + reportKeyTablixHeight + graphHeight + table2Height, SizeTypes.Inch);
                        }
                        else
                        {
                            _report.Body.ReportItems[indxGraph01].Top = new ReportSize(headerHeight.Value + reportKeyTablixHeight + mainTableHeight + graphHeight + table2Height, SizeTypes.Inch);
                        }
                        _report.Body.ReportItems[indxGraph01].Left = new ReportSize(0.30, SizeTypes.Inch);
                        graphHeight = getSizeTablix(REPORT_GRAPH_01_NAME) + 0.05;
                    }

                    var indxGraph02 = findReportItemIndexOf(REPORT_GRAPH_02_NAME);
                    if (indxGraph02 >= 0 && isVisibleChart02)
                    {
                        if (_includeTitlePage)
                        {
                            _report.Body.ReportItems[indxGraph02].Top = new ReportSize(headerHeight.Value + reportKeyTablixHeight + graphHeight + table2Height, SizeTypes.Inch);
                        }
                        else
                        {
                            _report.Body.ReportItems[indxGraph02].Top = new ReportSize(headerHeight.Value + reportKeyTablixHeight + mainTableHeight + graphHeight + table2Height, SizeTypes.Inch);
                        }
                        _report.Body.ReportItems[indxGraph02].Left = new ReportSize(0.30, SizeTypes.Inch);
                        graphHeight = graphHeight + getSizeTablix(REPORT_GRAPH_02_NAME) + 0.05;
                    }
                }

                //"table1 = Main Table ";
                var indxT1 = findReportItemIndexOf(REPORT_TABLE1_NAME);
                var table1Item = _report.Body.ReportItems[indxT1];
                if (indxT1 >= 0)
                {
                    if (table1Item is Tablix)
                    {
                        var i = 0;
                        var table1 = table1Item as Tablix;
                        while (i < table1.TablixColumnHierarchy.TablixMembers.Count - 1)
                        {
                            var tablixMember = table1.TablixColumnHierarchy.TablixMembers[i];
                            if (tablixMember.Visibility != null && tablixMember.Visibility.Hidden.IsExpression)
                            {
                                var hiddenExpression = tablixMember.Visibility.Hidden.Expression;
                                var pattern = new System.Text.RegularExpressions.Regex(@"(?<=Parameters!)(\w+)", System.Text.RegularExpressions.RegexOptions.Compiled);
                                var parameterName = pattern.Match(hiddenExpression).Value.ToLower();

                                //Replace \" for string types
                                var parameterValue = hiddenExpression.Substring(hiddenExpression.LastIndexOf("=") + 1).Trim().Replace("\"", "");
                                var rptParameterValue = parameters.Where(param => param.Key.ToLower() == parameterName).FirstOrDefault().Value;
                                if (rptParameterValue == null)
                                {
                                    rptParameterValue = _rptParameters.Where(param => param.Name.ToLower() == parameterName).FirstOrDefault().DefaultValue;
                                }

                                if (parameterValue.ToLower() == rptParameterValue.ToString().ToLower())
                                {
                                    var column = table1.TablixBody.TablixColumns[i];
                                    table1.TablixBody.TablixColumns.RemoveAt(i);
                                    table1.TablixColumnHierarchy.TablixMembers.RemoveAt(i);

                                    var numCols = table1.TablixBody.TablixColumns.Count;
                                    foreach (var row in table1.TablixBody.TablixRows)
                                    {
                                        foreach (var cell in row.TablixCells.Where(c => c.CellContents != null && c.CellContents.ColSpan > numCols))
                                        {
                                            cell.CellContents.ColSpan = numCols;
                                        }
                                        row.TablixCells.RemoveAt(i);
                                    }

                                    table1.Width = new ReportSize(table1.Width.Value - column.Width.Value);
                                }
                            }
                            i++;
                        }
                    }


                    var leftReportMargin = (_report.Page.PageWidth.Value - table1Item.Width.Value) / 2;
                    ReportSize top = new ReportSize();
                    if (_includeTitlePage)
                    {
                        top = new ReportSize(headerHeight.Value + reportKeyTablixHeight + graphHeight + table2Height, SizeTypes.Inch);
                    }
                    else
                    {
                        top = new ReportSize(headerHeight.Value, SizeTypes.Inch);
                    }
                    var left = _formatTypeID == (int)FormatTypeEnum.PDF ? new ReportSize(leftReportMargin, SizeTypes.Inch) : new ReportSize(0.0, SizeTypes.Inch);

                    table1Item.Top = top;
                    table1Item.Left = left;

                    var indxT1E = findReportItemIndexOf(REPORT_TABLE1_E_NAME);
                    if (indxT1E != -1)
                    {
                        _report.Body.ReportItems[indxT1E].Top = top;
                        _report.Body.ReportItems[indxT1E].Left = left;
                    }
                }

                //Resizing 
                var reportKeyTablix = (Tablix)_report.Body.ReportItems[indxRKey];
                reportKeyTablix.Left = table1Item.Left;
                //Resize the Field Value in the report key column for standard reports
                if (!_isCustomReport)
                {
                    reportKeyTablix.TablixBody.TablixColumns[1].Width = table1Item.Width - reportKeyTablix.TablixBody.TablixColumns[0].Width;
                }
                reportKeyTablix.Width = table1Item.Width;
                if (_includeTitlePage)
                {
                    reportKeyTablix.Top = headerHeight;
                }
                else
                {
                    if (_reportID != (int)ReportEnum.ManagedCareSavingsAndFees)
                    {
                        reportKeyTablix.Top = new ReportSize(headerHeight.Value + mainTableHeight - 0.005, SizeTypes.Inch);
                    }
                    else
                    {
                        reportKeyTablix.Top = new ReportSize(headerHeight.Value + mainTableHeight + table2Height, SizeTypes.Inch);
                    }
                }


                if (indxT2 >= 0)
                {
                    var table2 = (Tablix)_report.Body.ReportItems[indxT2];

                    if (!_isCustomReport)
                    {
                        foreach (var column in table2.TablixBody.TablixColumns)
                        {
                            column.Width = new ReportSize(column.Width.Value / table2.Width.Value * table1Item.Width.Value);
                        }
                    }
                    table2.Width = table1Item.Width;
                    table2.Left = table1Item.Left;
                }
                if (indxEH >= 0)
                {
                    if (_formatTypeID == (int)FormatTypeEnum.PDF)
                    {
                        _report.Body.ReportItems.RemoveAt(indxEH);
                    }
                    else
                    {
                        _report.Body.ReportItems[indxEH].Top = new ReportSize(0.0, SizeTypes.Inch);
                        _report.Body.ReportItems[indxEH].Left = table1Item.Left;
                    }
                }
            }
        }

        private int findReportItemIndexOf(string tableName)
        {
            var tb = (from t in _report.Body.ReportItems
                      where t.Name == tableName
                      select t).FirstOrDefault();

            return (tb == null) ? -1 : _report.Body.ReportItems.IndexOf(tb);
        }

        private void verifyRowCountErrorBox(string result)
        {
            List<ReportItem> removeReportItems = _report.Body.ReportItems.ToList();

            foreach (ReportItem reportItem in removeReportItems)
            {
                _report.Body.ReportItems.Remove(reportItem);
            }

            List<DataSet> removeDataSets = new List<DataSet>();
            if (!string.IsNullOrEmpty(_RdlHeaderDate))
            {
                foreach (DataSet dataSet in _report.DataSets)
                {
                    if (!_RdlHeaderDate.ToLower().Contains(dataSet.Name.ToLower()))
                    {
                        removeDataSets.Add(dataSet);
                    }
                }
            }
            else
            {
                DataSet removeDataSet = _report.DataSets.Where(d => d.Name == "DataSet2").FirstOrDefault();
                if (removeDataSet != null)
                {
                    removeDataSets.Add(removeDataSet);
                }
            }

            if (removeDataSets.Count != 0)
            {
                foreach (DataSet removeDataSet in removeDataSets)
                {
                    _report.DataSets.Remove(removeDataSet);
                }
            }

            Textbox txtbox = new Textbox();
            txtbox.Name = "Error_Text_Box";
            txtbox.CanGrow = true;
            txtbox.KeepTogether = true;
            txtbox.Paragraphs[0].Style = new Style();
            txtbox.Paragraphs[0].Style.TextAlign = new ReportExpression<TextAlignments>("Left");
            txtbox.Paragraphs[0].TextRuns[0].Value = new ReportExpression(result);
            txtbox.Paragraphs[0].TextRuns[0].Style = new Style();
            txtbox.Paragraphs[0].TextRuns[0].Style.TextAlign = new ReportExpression<TextAlignments>("Left");
            txtbox.Paragraphs[0].TextRuns[0].Style.FontFamily = new ReportExpression("Arial");
            txtbox.Paragraphs[0].TextRuns[0].Style.FontSize = new ReportExpression<ReportSize>("9pt");
            txtbox.Paragraphs[0].TextRuns[0].Style.FontWeight = new ReportExpression<FontWeights>("Bold");
            txtbox.Paragraphs[0].TextRuns[0].Style.Color = new ReportExpression<ReportColor>("Black");
            txtbox.Paragraphs[0].TextRuns[0].Style.Format = new ReportExpression();
            txtbox.Style = new Style();
            txtbox.Style.VerticalAlign = new ReportExpression<VerticalAlignments>("Top");
            txtbox.Style.TextAlign = new ReportExpression<TextAlignments>("Left");
            txtbox.Style.PaddingBottom = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingRight = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingLeft = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingTop = new ReportExpression<ReportSize>("1pt");
            _report.Body.ReportItems.Add(txtbox);
        }

        private void addReportHeader()
        {
            RdlManager rdlManager = new RdlManager();
            
            string headerRdl = rdlManager.GetReportHeader(_reportID, _formatTypeID, _reportLayoutStyleID, findReportItemIndexOf(REPORT_KEY_TABLIX_NAME));
            if (!string.IsNullOrEmpty(headerRdl))
            {
                RDL.Report report = loadReportFromRDLString(headerRdl);
                _report.Page.PageHeader = report.Page.PageHeader;
            }
        }

        private void addReportFooter()
        {
            RdlManager rdlManager = new RdlManager();
            string footerRdl = rdlManager.GetReportFooter(_reportID, _formatTypeID, _reportLayoutStyleID);
            if (!string.IsNullOrEmpty(footerRdl))
            {
                RDL.Report report = loadReportFromRDLString(footerRdl);
                _report.Page.PageFooter = report.Page.PageFooter;
            }
        }

        private RDL.Report loadReportFromRDLString(string rdlstring)
        {
            RDL.Report report = new RDL.Report();
            RdlSerializer serializer = new RdlSerializer();

            byte[] byteArray = Encoding.ASCII.GetBytes(rdlstring);

            using (MemoryStream strm = new MemoryStream(byteArray))
            {
                report = serializer.Deserialize(strm);
            }
            return report;
        }

        private string serializeRdl()
        {
            string rdlString = String.Empty;
            RdlSerializer serializer = new RdlSerializer();

            using (MemoryStream memStream = new MemoryStream())
            {
                serializer.Serialize(memStream, _report);
                memStream.Position = 0;
                using (StreamReader reader = new StreamReader(memStream))
                {
                    rdlString = reader.ReadToEnd();
                }
            }

            return rdlString;
        }

        private void addReportKeyTablix(IDictionary<string, object> parameters, bool isCustom)
        {
            ReportSize bodyHeight = new ReportSize(_report.Body.Height.Value + 0.1);
            double tablixTop = 0;
            if (_report.Body.ReportItems != null && _report.Body.ReportItems.Count != 0)
            {
                foreach (ReportItem reportItem in _report.Body.ReportItems)
                {
                    tablixTop += reportItem.Top.Value + reportItem.Height.Value;
                }
            }

            var indx = findReportItemIndexOf(REPORT_KEY_TABLIX_NAME);
            if (indx >= 0)
            {
                _report.Body.ReportItems[indx].Initialize();
                _report.Body.ReportItems[indx] = UpdateReportItem();
            }

        }

        #region Create Report Key Tablix

        private Tablix UpdateReportItem()
        {
            Tablix table = new Tablix();
            table.Name = REPORT_KEY_TABLIX_NAME;
            table.DataSetName = "DataSet2";
            table.KeepTogether = true;

            //table.ZIndex = 2;
            int headerRows = 3;

            TablixColumn keyNameColumn = new TablixColumn();
            ReportSize widthColumnName = new ReportSize(3.0);
            keyNameColumn.Width = widthColumnName;
            TablixColumn keyValueColumn = new TablixColumn();

            ReportSize widthColumnValue = (_formatTypeID == (int)FormatTypeEnum.PDF) ? new ReportSize(_report.Body.ReportItems[findReportItemIndexOf(REPORT_TABLE1_NAME)].Width.Value) : new ReportSize(10.51253);
            keyValueColumn.Width = new ReportSize((widthColumnValue.Value - widthColumnName.Value));

            table.TablixBody.TablixColumns.Add(keyNameColumn);
            table.TablixBody.TablixColumns.Add(keyValueColumn);

            table.Width = new ReportSize((keyValueColumn.Width.Value + widthColumnName.Value));

            //empty Row
            TablixRow rowEmpty = createReportKeyItemRow(string.Format("", getNextNumber()), "");
            rowEmpty.Height = new ReportSize(0.01);
            table.TablixBody.TablixRows.Add(rowEmpty);
            TablixMember rowMemberEmpty = new TablixMember();
            table.TablixRowHierarchy.TablixMembers.Add(rowMemberEmpty);

            //Tablix - Rows
            TablixRow headerRowLayout = new TablixRow();
            headerRowLayout.Height = new ReportSize(0.22);
            table.TablixBody.TablixRows.Add(headerRowLayout);

            TablixCell keyNameCellLayout = createReportKeyHeaderCell("KeyName_TextBoxLayout", "Report Details");
            headerRowLayout.TablixCells.Add(keyNameCellLayout);

            var datetimePrinted = DateTime.Now;

            TablixCell keyValueCellLayout = createReportKeyHeaderCell("KeyValue_TextBoxLayout", "Generated On: " + datetimePrinted.ToString());
            headerRowLayout.TablixCells.Add(keyValueCellLayout);

            TablixMember keyNameMemberLayout = new TablixMember();
            table.TablixColumnHierarchy.TablixMembers.Add(keyNameMemberLayout);
            TablixMember keyValueMemberLayout = new TablixMember();
            table.TablixColumnHierarchy.TablixMembers.Add(keyValueMemberLayout);

            TablixMember headerMemberLayout = new TablixMember();
            table.TablixRowHierarchy.TablixMembers.Add(headerMemberLayout);

            //Layout Style Row
            var reportLayoutStyleLabel = _isCustomReport ? "Custom" : _groupByInfo.Replace(":", "");
            TablixRow rowLayoutStyle = createReportKeyItemRow("Selected Style", reportLayoutStyleLabel);
            table.TablixBody.TablixRows.Add(rowLayoutStyle);
            TablixMember rowMemberLayoutStyle = new TablixMember();
            table.TablixRowHierarchy.TablixMembers.Add(rowMemberLayoutStyle);

            string groupByInfo = _groupByInfo;
            string sortByInfo = _sortByInfo;

            if (!string.IsNullOrEmpty(groupByInfo))
            {
                string[] filterValues = groupByInfo.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                if (filterValues.Length != 0)
                {
                    foreach (var filterValue in filterValues)
                    {
                        if (string.IsNullOrEmpty(filterValue)) continue;

                        string[] prms = filterValue.Split(new char[] { ':' });
                        if (prms.Length >= 2)
                        {
                            string value = prms[1].Replace("then", ",").Replace(",,","");

                            string[] filterSubValues = value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                            if (filterSubValues.Length != 0)
                            {
                                foreach (var filterSubValue in filterSubValues)
                                {
                                    if (string.IsNullOrEmpty(filterSubValue)) continue;
                                    var subPrms = filterSubValue.Split(new char[] { ',' });
                                    
                                    for (var i = 0; i < subPrms.Length; i++)
                                    {
                                        if(string.IsNullOrWhiteSpace(subPrms[i])) continue;

                                        var groupby = String.Empty;
                                        if (i == 0)
                                        {
                                            groupby = prms[0];
                                        }

                                        TablixRow row = createReportKeyItemRow(groupby, subPrms[i]);           
                                        table.TablixBody.TablixRows.Add(row);
                                        TablixMember rowMember = new TablixMember();
                                        table.TablixRowHierarchy.TablixMembers.Add(rowMember);                                       
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(sortByInfo))
            {
                string[] filterValues = sortByInfo.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                if (filterValues.Length != 0)
                {
                    foreach (var filterValue in filterValues)
                    {
                        if (string.IsNullOrEmpty(filterValue)) continue;

                        string[] prms = filterValue.Split(new char[] { ':' });
                        if (prms.Length >= 2)
                        {
                            string value = prms[1];//.Replace("then", "");		

                            string[] filterSubValues = value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                            if (filterSubValues.Length != 0)
                            {
                                foreach (var filterSubValue in filterSubValues)
                                {
                                    if (string.IsNullOrEmpty(filterSubValue)) continue;
                                    string[] subPrms = filterSubValue.Split(new char[] { ',' });
                                    if (subPrms.Length >= 1)
                                    {
                                        for (var i = 0; i < subPrms.Length; i++)
                                        {
                                            string groupby = String.Empty;
                                            if (i == 0)
                                            {
                                                groupby = prms[0];
                                            }
                                            TablixRow row = createReportKeyItemRow(groupby, subPrms[i]);
                                            table.TablixBody.TablixRows.Add(row);
                                            TablixMember rowMember = new TablixMember();
                                            table.TablixRowHierarchy.TablixMembers.Add(rowMember);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //Space between tables 
            //Filter Key
            TablixRow headerRow = new TablixRow();
            headerRow.Height = new ReportSize(0.22);
            table.TablixBody.TablixRows.Add(headerRow);

            TablixCell itemNameCell = createReportKeyHeaderCell(string.Format("textBox_{0}_keyName", getNextNumber()), "Filter Criteria");
            headerRow.TablixCells.Add(itemNameCell);
            TablixCell itemValueCell = createReportKeyHeaderCell(string.Format("textBox_{0}_keyValue", getNextNumber()), " ");
            headerRow.TablixCells.Add(itemValueCell);

            TablixMember hrowMember = new TablixMember();
            table.TablixRowHierarchy.TablixMembers.Add(hrowMember);

            string filterString = _filterKey;

            if (!string.IsNullOrEmpty(filterString))
            {
                string[] filterValues = filterString.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                if (filterValues.Length != 0)
                {
                    foreach (var filterValue in filterValues)
                    {
                        if (string.IsNullOrEmpty(filterValue)) continue;

                        string[] prms = filterValue.Split(new char[] { ':' });
                        if (prms.Length >= 2)
                        {
                            string value = "";
                            for (var i = 1; i < prms.Length; i++)
                            {
                                if (string.IsNullOrEmpty(value))
                                {
                                    value += prms[i];
                                }
                                else
                                {
                                    value += ":" + prms[i];
                                }
                            }


                            if (!prms[0].ToLower().Contains("sort by") && !prms[0].ToLower().Contains("summary by"))
                            {
                                var newHeightSize = (int)(value.Length / MAX_STRING_LENGHT_UPPERCASE) + 1;
                                TablixRow row = createReportKeyItemRow(prms[0], value);
                                table.TablixBody.TablixRows.Add(row);

                                TablixMember rowMember = new TablixMember();
                                table.TablixRowHierarchy.TablixMembers.Add(rowMember);                               
                            }

                        }
                    }
                }
            }

            /// Header  for Notes & Disclaimers
            headerRow = new TablixRow();
            headerRow.Height = new ReportSize(0.22);
            table.TablixBody.TablixRows.Add(headerRow);

            itemNameCell = createReportKeyHeaderCell(string.Format("textBox_{0}_keyName", getNextNumber()), "Notes & Disclaimers");
            headerRow.TablixCells.Add(itemNameCell);
            itemValueCell = createReportKeyHeaderCell(string.Format("textBox_{0}_keyValue", getNextNumber()), " ");
            headerRow.TablixCells.Add(itemValueCell);

            hrowMember = new TablixMember();
            table.TablixRowHierarchy.TablixMembers.Add(hrowMember);

            /////// 2 Last lines

            TablixMember rowMemberfinal = new TablixMember();
            TablixRow rowfinal = null;

            var infoFooterMessage = addInfoFooterMessage();
            if (infoFooterMessage.Length > 0)
            {
                infoFooterMessage = "* " + infoFooterMessage;
                rowfinal = createReportKeyRowInfo(infoFooterMessage);
                table.TablixBody.TablixRows.Add(rowfinal);
                table.TablixRowHierarchy.TablixMembers.Add(rowMemberfinal);
                var newRowHeightParam = ((int)(infoFooterMessage.Length) / MAX_STRING_LENGHT_LOWERCASE) + 1; // 
                rowfinal.Height = _formatTypeID == (int)FormatTypeEnum.Excel ? new ReportSize(0.2 * newRowHeightParam) : new ReportSize(0.2);
                headerRows = headerRows + newRowHeightParam;
            }

            rowfinal = createReportKeyRowInfo(disclamerTxt);
            table.TablixBody.TablixRows.Add(rowfinal);
            table.TablixRowHierarchy.TablixMembers.Add(rowMemberfinal);

            if (
                    (_isCustomReport) && ( _reportID == (int)ReportEnum.SIRDeductibleSpecificDetailIncurred ||
                                           _reportID == (int)ReportEnum.SIRDeductibleSpecificDetailPaid ||
                                           _reportID == (int)ReportEnum.AggregateReportDetailPaid ||
                                           _reportID == (int)ReportEnum.AggregateReportDetailIncurred 
                                         )
                )
            {

                rowfinal = createReportKeyRowInfo(DISCLAMER_OCURRENCE);
                table.TablixBody.TablixRows.Add(rowfinal);
                table.TablixRowHierarchy.TablixMembers.Add(rowMemberfinal);
            }

            /// only comparison report
            if (_reportID == (int)ReportEnum.ComparisonReport)
            {
                TablixMember rowMemberFootNotes = new TablixMember();
                TablixRow rowFootNotes = null;

                var infoFootNotesMessage = addInfoFootNotesMessage();

                if (infoFootNotesMessage.Length > 0)
                {                    
                    /// Space
                    TablixRow rowSpaceEmpty = createReportKeyItemRow(string.Format("", getNextNumber()), "");
                    rowEmpty.Height = new ReportSize(0.01);
                    table.TablixBody.TablixRows.Add(rowSpaceEmpty);
                    TablixMember rowMemberSpaceEmpty = new TablixMember();
                    table.TablixRowHierarchy.TablixMembers.Add(rowMemberSpaceEmpty);
                    
                    /// Header
                    headerRow = new TablixRow();
                    headerRow.Height = new ReportSize(0.22);
                    table.TablixBody.TablixRows.Add(headerRow);

                    itemNameCell = createReportKeyHeaderCell(string.Format("textBox_{0}_keyName", getNextNumber()), "FootNotes");
                    headerRow.TablixCells.Add(itemNameCell);
                    itemValueCell = createReportKeyHeaderCell(string.Format("textBox_{0}_keyValue", getNextNumber()), " ");
                    headerRow.TablixCells.Add(itemValueCell);

                    hrowMember = new TablixMember();
                    table.TablixRowHierarchy.TablixMembers.Add(hrowMember);

                    /// Message
                    rowFootNotes = createReportKeyRowInfoFootNotes(infoFootNotesMessage);
                    table.TablixBody.TablixRows.Add(rowFootNotes);
                    table.TablixRowHierarchy.TablixMembers.Add(rowMemberFootNotes);

                    string[] footNotesMessageValues = infoFootNotesMessage.Split(new string[] { "\r" }, StringSplitOptions.RemoveEmptyEntries);

                    var newRowFootNotesHeightParam = 0;
                    foreach (var footNotesMessageLine in footNotesMessageValues)
                    {                        
                        newRowFootNotesHeightParam =newRowFootNotesHeightParam + ((int)(footNotesMessageLine.Length) / MAX_STRING_LENGHT_LOWERCASE) + 1; 
                    }
                    rowFootNotes.Height = _formatTypeID == (int)FormatTypeEnum.Excel ? new ReportSize(0.2 * newRowFootNotesHeightParam) : new ReportSize(0.2);
                }
            }

            // Row to close table2
            TablixRow rowEmptyFinal = createReportKeyItemRow(string.Format("", getNextNumber()), "");
            rowEmptyFinal.Height = new ReportSize(0.01);
            table.TablixBody.TablixRows.Add(rowEmptyFinal);
            TablixMember rowMemberEmptyFinal = new TablixMember();
            table.TablixRowHierarchy.TablixMembers.Add(rowMemberEmptyFinal);

            return table;
        }

        private void infoSortBy()
        {
            if (!string.IsNullOrEmpty(_filterKey))
            {
                string[] filterKeys = _filterKey.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (filterKeys != null && filterKeys.Length != 0)
                {
                    foreach (string filterKey in filterKeys)
                    {
                        if (filterKey.ToLower().Contains("sort by"))
                        {
                            if (string.IsNullOrEmpty(_sortByInfo))
                            {
                                _sortByInfo += filterKey;
                            }
                            else
                            {
                                _sortByInfo += ", " + filterKey;
                            }
                        }
                    }
                }
            }
        }

        private void infoGroupBy()
        {

            if (!string.IsNullOrEmpty(_filterKey))
            {
                string[] filterKeys = _filterKey.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (filterKeys != null && filterKeys.Length != 0)
                {
                    foreach (string filterKey in filterKeys)
                    {
                        if (filterKey.ToLower().Contains("summary by"))
                        {
                            if (string.IsNullOrEmpty(_groupByInfo))
                            {
                                _groupByInfo += filterKey;
                            }
                            else
                            {
                                _groupByInfo += ", " + filterKey;
                            }
                        }
                    }
                }
            }
        }

        private TablixRow createReportKeyItemRow(string captionColumnName, string valueColumnName)
        {
            TablixRow row = new TablixRow();
            row.Height = new ReportSize(0.22);

            int sepCount = 0;
            if (captionColumnName.ToLower().Trim() == "hierarchy" || captionColumnName.ToLower().Trim() == "member codes hierarchy") 
            {
                string[] capt = valueColumnName.Split(new string[] {"!"}, StringSplitOptions.RemoveEmptyEntries);
                foreach (string cpt in capt)
                {
                    sepCount += cpt.Length > MAX_STRING_LENGHT_UPPERCASE ? (int)(cpt.Length / MAX_STRING_LENGHT_UPPERCASE) + 1 : 1;   
                }
            }

            if (valueColumnName.Length > MAX_STRING_LENGHT_UPPERCASE || sepCount > 1)
            {
                var newHeightSize = sepCount == 0 ? (int)(valueColumnName.Length / MAX_STRING_LENGHT_UPPERCASE) + 1 : sepCount;
                row.Height = _formatTypeID == (int)FormatTypeEnum.Excel ? new ReportSize(0.20 * newHeightSize) : new ReportSize(0.22);
            }

            if (captionColumnName.ToLower() == "selected style")
            {
                captionColumnName = "Report Layout Style";
            }

            TablixCell itemNameCell = createReportKeyItemCell(string.Format("textBox_{0}_keyName", getNextNumber()), captionColumnName.Trim());
            row.TablixCells.Add(itemNameCell);
            TablixCell itemValueCell = (captionColumnName.ToLower().Trim() == "hierarchy" || captionColumnName.ToLower().Trim() == "member codes hierarchy") 
                                           ? createReportKeyItemCellForHierarchy(string.Format("textBox_{0}_keyValue", getNextNumber()),valueColumnName.Trim())
                                           : createReportKeyItemCell(string.Format("textBox_{0}_keyValue", getNextNumber()),valueColumnName.Trim());
            row.TablixCells.Add(itemValueCell);
            return row;
        }

        private TablixRow createReportKeyRowInfo(string valueColumnName)
        {
            TablixRow row = new TablixRow();
            row.Height = new ReportSize(0.22);
            TablixCell itemNameCell = createReportKeyInfoCell(string.Format("textBox_{0}_keyName", getNextNumber()), String.Empty);
            row.TablixCells.Add(itemNameCell);
            TablixCell itemValueCell = createReportKeyInfoCell(string.Format("textBox_{0}_keyValue", getNextNumber()), valueColumnName.Trim());
            row.TablixCells.Add(itemValueCell);
            return row;
        }

        private TablixRow createReportKeyRowInfoFootNotes(string valueColumnName)
        {
            TablixRow row = new TablixRow();
            row.Height = new ReportSize(0.22);
            TablixCell itemNameCell = createReportKeyInfoFootNotesCell(string.Format("textBox_{0}_keyName", getNextNumber()), String.Empty);
            row.TablixCells.Add(itemNameCell);
            TablixCell itemValueCell = createReportKeyInfoFootNotesCell(string.Format("textBox_{0}_keyValue", getNextNumber()), valueColumnName.Trim());
            row.TablixCells.Add(itemValueCell);
            return row;
        }

        private int getNextNumber()
        {
            _textboxNum++;
            return _textboxNum;
        }

        private TablixCell createReportKeyItemCell(string name, string caption)
        {
            TablixCell headerCell = new TablixCell();
            Textbox txtbox = new Textbox();
            headerCell.CellContents = new CellContents();
            headerCell.CellContents.ReportItem = txtbox;
            txtbox.Name = name;
            txtbox.CanGrow = true;
            txtbox.KeepTogether = true;
            txtbox.Paragraphs[0].Style = new Style();
            txtbox.Paragraphs[0].Style.TextAlign = new ReportExpression<TextAlignments>("Left");
            txtbox.Paragraphs[0].TextRuns[0].Value = new ReportExpression(caption);
            txtbox.Paragraphs[0].TextRuns[0].Style = new Style();
            txtbox.Paragraphs[0].TextRuns[0].Style.TextAlign = new ReportExpression<TextAlignments>("Left");
            txtbox.Paragraphs[0].TextRuns[0].Style.FontFamily = new ReportExpression("Arial");
            txtbox.Paragraphs[0].TextRuns[0].Style.FontSize = new ReportExpression<ReportSize>("9pt");
            txtbox.Paragraphs[0].TextRuns[0].Style.FontWeight = new ReportExpression<FontWeights>("Bold");
            txtbox.Paragraphs[0].TextRuns[0].Style.Color = new ReportExpression<ReportColor>("Black");
            txtbox.Paragraphs[0].TextRuns[0].Style.Format = new ReportExpression();
            txtbox.Width = new ReportSize(8.0);
            txtbox.Style = new Style();
            txtbox.Style.BottomBorder = new Border();
            txtbox.Style.BottomBorder.Color = new ReportExpression<ReportColor>("DimGray");
            txtbox.Style.BottomBorder.Style = new ReportExpression<BorderStyles>("Solid");
            txtbox.Style.BottomBorder.Width = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.VerticalAlign = new ReportExpression<VerticalAlignments>("Middle");
            txtbox.Style.TextAlign = new ReportExpression<TextAlignments>("Left");
            txtbox.Style.PaddingBottom = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingRight = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingLeft = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingTop = new ReportExpression<ReportSize>("1pt");
            return headerCell;
        }

        private TablixCell createReportKeyItemCellForHierarchy(string name, string caption)
        {
            string[] captions = caption.Split(new string[]{"!"}, StringSplitOptions.RemoveEmptyEntries);

            TablixCell headerCell = new TablixCell();
            Textbox txtbox = new Textbox();
            headerCell.CellContents = new CellContents();
            headerCell.CellContents.ReportItem = txtbox;
            txtbox.Name = name;
            txtbox.CanGrow = true;
            txtbox.KeepTogether = true;
            var idx = 0;
            foreach (var cpt in captions)
            {
                if (idx > 0)
                {
                    txtbox.Paragraphs.Add(new Paragraph());
                }
                txtbox.Paragraphs[idx].Style = new Style();
                txtbox.Paragraphs[idx].Style.TextAlign = new ReportExpression<TextAlignments>("Left");
                txtbox.Paragraphs[idx].TextRuns[0].Value = new ReportExpression(cpt.Replace("$", ";"));
                txtbox.Paragraphs[idx].TextRuns[0].Style = new Style();
                txtbox.Paragraphs[idx].TextRuns[0].Style.TextAlign = new ReportExpression<TextAlignments>("Left");
                txtbox.Paragraphs[idx].TextRuns[0].Style.FontFamily = new ReportExpression("Arial");
                txtbox.Paragraphs[idx].TextRuns[0].Style.FontSize = new ReportExpression<ReportSize>("9pt");
                txtbox.Paragraphs[idx].TextRuns[0].Style.FontWeight = new ReportExpression<FontWeights>("Bold");
                txtbox.Paragraphs[idx].TextRuns[0].Style.Color = new ReportExpression<ReportColor>("Black");
                txtbox.Paragraphs[idx].TextRuns[0].Style.Format = new ReportExpression();
                idx++;
            }
            txtbox.Width = new ReportSize(8.0);
            txtbox.Style = new Style();
            txtbox.Style.BottomBorder = new Border();
            txtbox.Style.BottomBorder.Color = new ReportExpression<ReportColor>("DimGray");
            txtbox.Style.BottomBorder.Style = new ReportExpression<BorderStyles>("Solid");
            txtbox.Style.BottomBorder.Width = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.VerticalAlign = new ReportExpression<VerticalAlignments>("Middle");
            txtbox.Style.TextAlign = new ReportExpression<TextAlignments>("Left");
            txtbox.Style.PaddingBottom = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingRight = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingLeft = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingTop = new ReportExpression<ReportSize>("1pt");
            return headerCell;
        }

        private TablixCell createReportKeyInfoCell(string name, string caption)
        {
            TablixCell headerCell = new TablixCell();
            Textbox txtbox = new Textbox();

            headerCell.CellContents = new CellContents();
            headerCell.CellContents.ReportItem = txtbox;
            txtbox.Name = name;
            txtbox.CanGrow = true;
            txtbox.KeepTogether = true;
            txtbox.Paragraphs[0].Style = new Style();
            txtbox.Paragraphs[0].Style.TextAlign = new ReportExpression<TextAlignments>("Left");
            txtbox.Paragraphs[0].TextRuns[0].Value = new ReportExpression(caption);
            txtbox.Paragraphs[0].TextRuns[0].Style = new Style();
            txtbox.Paragraphs[0].TextRuns[0].Style.TextAlign = new ReportExpression<TextAlignments>("Left");
            txtbox.Paragraphs[0].TextRuns[0].Style.FontFamily = new ReportExpression("Arial");
            txtbox.Paragraphs[0].TextRuns[0].Style.FontSize = new ReportExpression<ReportSize>("8pt");
            txtbox.Paragraphs[0].TextRuns[0].Style.FontWeight = new ReportExpression<FontWeights>("Bold");
            txtbox.Paragraphs[0].TextRuns[0].Style.Color = new ReportExpression<ReportColor>("Red");
            txtbox.Paragraphs[0].TextRuns[0].Style.Format = new ReportExpression();
            txtbox.Width = new ReportSize(8.0);
            txtbox.Style = new Style();
            txtbox.Style.VerticalAlign = new ReportExpression<VerticalAlignments>("Middle");
            txtbox.Style.TextAlign = new ReportExpression<TextAlignments>("Left");
            txtbox.Style.PaddingBottom = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingRight = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingLeft = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingTop = new ReportExpression<ReportSize>("1pt");

            return headerCell;
        }

        private TablixCell createReportKeyHeaderCell(string name, string caption)
        {
            TablixCell headerCell = new TablixCell();
            Textbox txtbox = new Textbox();
            headerCell.CellContents = new CellContents();
            headerCell.CellContents.ReportItem = txtbox;
            txtbox.Name = name;
            txtbox.CanGrow = true;
            txtbox.KeepTogether = true;
            txtbox.Paragraphs[0].TextRuns[0].Value = new ReportExpression(caption);
            txtbox.Paragraphs[0].TextRuns[0].Style = new Style();
            txtbox.Paragraphs[0].TextRuns[0].Style.FontWeight = new ReportExpression<FontWeights>("Bold");
            txtbox.Paragraphs[0].TextRuns[0].Style.Color = new ReportExpression<ReportColor>("#333b8c");
            txtbox.Style = new Style();
            txtbox.Style.TopBorder = new Border();
            txtbox.Style.TopBorder.Color = new ReportExpression<ReportColor>("DimGray");
            txtbox.Style.TopBorder.Style = new ReportExpression<BorderStyles>("Solid");
            txtbox.Style.TopBorder.Width = new ReportExpression<ReportSize>("2pt");
            txtbox.Style.BottomBorder = new Border();
            txtbox.Style.BottomBorder.Color = new ReportExpression<ReportColor>("DimGray");
            txtbox.Style.BottomBorder.Style = new ReportExpression<BorderStyles>("Solid");
            txtbox.Style.BottomBorder.Width = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.BackgroundColor = new ReportExpression<ReportColor>("WhiteSmoke");
            txtbox.Style.VerticalAlign = new ReportExpression<VerticalAlignments>("Middle");
            txtbox.Style.PaddingBottom = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingRight = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingLeft = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingTop = new ReportExpression<ReportSize>("1pt");
            return headerCell;
        }

        private TablixCell createReportKeyInfoFootNotesCell(string name, string caption)
        {
            TablixCell headerCell = new TablixCell();
            Textbox txtbox = new Textbox();

            headerCell.CellContents = new CellContents();
            headerCell.CellContents.ReportItem = txtbox;
            txtbox.Name = name;
            txtbox.CanGrow = true;
            txtbox.KeepTogether = true;
            txtbox.Paragraphs[0].Style = new Style();
            txtbox.Paragraphs[0].Style.TextAlign = new ReportExpression<TextAlignments>("Left");
            txtbox.Paragraphs[0].TextRuns[0].Value = new ReportExpression(caption);
            txtbox.Paragraphs[0].TextRuns[0].Style = new Style();
            txtbox.Paragraphs[0].TextRuns[0].Style.TextAlign = new ReportExpression<TextAlignments>("Left");
            txtbox.Paragraphs[0].TextRuns[0].Style.FontFamily = new ReportExpression("Arial");
            txtbox.Paragraphs[0].TextRuns[0].Style.FontSize = new ReportExpression<ReportSize>("8pt");
            txtbox.Paragraphs[0].TextRuns[0].Style.FontWeight = new ReportExpression<FontWeights>("Bold");
            txtbox.Paragraphs[0].TextRuns[0].Style.Color = new ReportExpression<ReportColor>("Black");
            txtbox.Paragraphs[0].TextRuns[0].Style.Format = new ReportExpression();
            txtbox.Width = new ReportSize(8.0);
            txtbox.Style = new Style();
            txtbox.Style.VerticalAlign = new ReportExpression<VerticalAlignments>("Middle");
            txtbox.Style.TextAlign = new ReportExpression<TextAlignments>("Left");
            txtbox.Style.PaddingBottom = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingRight = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingLeft = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingTop = new ReportExpression<ReportSize>("1pt");

            return headerCell;
        }

        #endregion

        private void addLogoImages(Dictionary<string, object> parameters)
        {
            string userId = null;
            string assNum = null;

            foreach (KeyValuePair<string, object> pair in parameters)
            {
                if (pair.Key.ToLower() == "userid" && pair.Value != null && pair.Value != null)
                {
                    userId = pair.Value.ToString();
                }

                if (pair.Key.ToLower() == "assnnum" && pair.Value != null && pair.Value != null)
                {
                    assNum = pair.Value.ToString();
                }
            }


            if (_reportID == (int)ReportEnum.ManagedCareBillsAndChargesByProvider || _reportID == (int)ReportEnum.ManagedCarePharmacyPBMSavings
                || _reportID == (int)ReportEnum.ManagedCareSavingsAndFees || _reportID == (int)ReportEnum.ManagedCareSavingsAndFeesDetail
                || _reportID == (int)ReportEnum.ManagedCareSavingsByProviderType || _reportID == (int)ReportEnum.ManagedCareSavingsByState)
            {
                addLogoImage(getLogoImageAsStringByName("compmc"), "companyLogoImage");
            }
            else
            {
                addLogoImage(getLogoImageAsStringByName("ccmsi"), "companyLogoImage");
            }

            addLogoImage(getLogoImageAsString(assNum, userId), "logoImage");
        }

        private void addLogoImage(string imageData, string containerName)
        {


            if (_report.Page.PageHeader != null)
            {
                foreach (ReportItem item in _report.Page.PageHeader.ReportItems)
                {
                    if (item is Image && item.Name.ToLower() == containerName.ToLower())
                    {
                        Image logoImage = item as Image;
                        logoImage.Source = SourceType.Database;
                        logoImage.MIMEType = new ReportExpression("image/png");
                        logoImage.Value = new ReportExpression("=\"" + imageData + "\"");
                    }
                }
            }

            if (_report.Body != null)
            {
                foreach (ReportItem item in _report.Body.ReportItems)
                {
                    if (item.Name == REPORT_EXCEL_HEADER_TABLIX_NAME)
                    {
                        Tablix tablix = (Tablix)item;
                        foreach (var tablixCell in tablix.TablixBody.TablixRows[0].TablixCells)
                        {
                            if (tablixCell.CellContents.ReportItem is Image && tablixCell.CellContents.ReportItem.Name.ToLower() == containerName.ToLower())
                            {
                                Image logoImage = tablixCell.CellContents.ReportItem as Image;
                                logoImage.Source = SourceType.Database;
                                logoImage.MIMEType = new ReportExpression("image/png");
                                logoImage.Value = new ReportExpression("=\"" + imageData + "\"");
                            }
                        }
                    }

                }

            }
        }

        private string addInfoFooterMessage()
        {
            ReportProjectDBEntities entities = new ReportProjectDBEntities();

            var rptObj = (from r in entities.Report
                          where r.ReportID == _reportID
                          select new { r.RdlFooterMessage }).FirstOrDefault();
            if (rptObj != null && rptObj.RdlFooterMessage != null)
            {
                return rptObj.RdlFooterMessage.ToString();
            }
            return String.Empty;
        }

        private string addInfoFootNotesMessage()
        {
            List<RptParameter> myparameters = new List<RptParameter>();
            foreach (var parameter in _rptParameters)
            {
                if (parameter.Name.ToLower() == REPORT_PARAMETER_FOOTNOTES)
                {
                    return parameter.DefaultValue.ToString();
                }
            }

            return String.Empty;
        }

        private string verifyRowCount(int reportID, Dictionary<string, object> parameters, bool isSummaryOnly, int formatTypeID)
        {
            if (formatTypeID == (int)FormatTypeEnum.Excel)
            {
                int maxRecordCount = Convert.ToInt32(ConfigurationManager.AppSettings["MaxReportRecordCount"]);
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

                        DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);

                        List<object> myParams = new List<object>();

                        foreach (ReportParameter rptParam in listParameters)
                        {
                            myParams.Add("@" + rptParam.ReportParameterName);
                            myParams.Add(rptParam.DefaultValue);
                        }

                        var isCustomParameterIndex = myParams.IndexOf("@IsCustom");
                        if (isCustomParameterIndex != -1)
                        {
                            myParams[isCustomParameterIndex + 1] = (_isCustomReport) ? "True" : "False";
                        }
                        else
                        {
                            myParams.Add("@IsCustom");
                            myParams.Add((_isCustomReport) ? "True" : "False");
                        }

                        int recordCount = dbHelper.GetScalar<int>(rptObj.VerifyRowCountSP, CommandType.StoredProcedure, myParams.ToArray());

                        if (recordCount > maxRecordCount)
                        {
                            // If Is Turn Off Page Break, add a message to mention the page breaks.
                            string complementaryMessage = _isTurnOffPageBreak ? "Please either narrow your filter criteria or turn on page breaks" : "Please narrow your filter criteria";

                            return String.Format("The report cannot be run because it returns too much data.  It would return {0} records, while the maximum allowed record count is {1}.  {2} and try again.",
                                   String.Format("{0:n0}", recordCount), String.Format("{0:n0}", maxRecordCount), complementaryMessage);
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                return null;
            }
            else
            {
                return null;
            }

        }

        private string getLogoImageAsString(string assn, string userId)
        {
            LogoManager logoManager = new LogoManager();
            return logoManager.GetLogoAsString(assn, userId);
        }

        private string getLogoImageAsStringByName(string imageName)
        {
            LogoManager logoManager = new LogoManager();
            return logoManager.GetLogoAsStringByName(imageName);
        }

        private void getReportInformation()
        {
            RdlManager rdlManager = new RdlManager();
            _rptFields = rdlManager.GetUserReportFields(_userReportID);
            _rptParameters = rdlManager.GetUserReportParameters(_userReportID);
        }

        private string getFilterKey(int userReportID)
        {
            RdlManager rdlManager = new RdlManager();

            try
            {
                ReportProjectDBEntities entities = new ReportProjectDBEntities();

                // get report name and format                		
                var userRpt = (from ur in entities.UserReport
                                .Include("Report")
                                .Include("ReportLayoutStyle")
                               where ur.UserReportID == userReportID
                               select new { ur.Report.ReportID, ur.Report.ReportName, ur.FormatTypeID, ur.ReportLayoutStyle.ReportLayoutStyleName, ur.IsCustom }).FirstOrDefault();


                return rdlManager.GetFilterKey(_rptParameters, _rptFields, userRpt.ReportLayoutStyleName, userRpt.IsCustom, userRpt.ReportID);
            }
            catch (Exception ex)
            {
                Logger.Write(ex.ToString(), "SLLog");
                throw;
            }
        }

        public void SaveReportRdlFile()
        {
            bool shouldSave = false;
            if (ConfigurationManager.AppSettings["SaveRdlFileForDebugging"] != null)
            {
                try
                {
                    shouldSave = Convert.ToBoolean(ConfigurationManager.AppSettings["SaveRdlFileForDebugging"]);
                }
                catch
                {
                }
            }

            if (shouldSave)
            {
                string path = string.Format("{0}{1}.rdl", System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/"), Guid.NewGuid().ToString());
                RdlSerializer serializer = new RdlSerializer();
                using (FileStream os = new FileStream(path, FileMode.Create))
                {
                    serializer.Serialize(os, _report);

                }
            }
        }
    }
}
