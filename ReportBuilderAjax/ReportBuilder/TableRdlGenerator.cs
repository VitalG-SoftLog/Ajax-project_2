using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ReportingServices.RdlObjectModel;
using RX = System.Text.RegularExpressions;

namespace ReportBuilderAjax.Web.ReportBuilder
{
    public class TableRdlGenerator
    {
        static int _textboxNum = 1000;

        List<RptField> _fields;
        List<RptField> _summarizeFields;
        List<SubRpt> _subReports;
        List<SubRptParameter> _subReportParameters;
        string _tablename;
        private bool _isSummaryOnly;
        private int _formatTypeID;
        Tablix _table;
        private int sumTotalHierrachy = 0;
        private UserReport _userRpt;


        private int _numCols = 0;

        public TableRdlGenerator(int formatTypeId)
        {
            _formatTypeID = formatTypeId;
        }

        public TableRdlGenerator(Tablix table, int formatTypeId, bool isSummaryOnly)
        {
            _table = table;
            _formatTypeID = formatTypeId;
            _isSummaryOnly = isSummaryOnly;
           
        }

        private int GetNextNumber()
        {
            _textboxNum++;
            return _textboxNum;
        }

        public Tablix ModifyTablix(UserReport userRpt, string tablename, List<RptField> fields, List<RptField> summarizeFields, List<SubRpt> subReports, List<SubRptParameter> subReportParameters)
        {
            _tablename = tablename;
            _fields = fields;
            _summarizeFields = summarizeFields;

            _subReportParameters = subReportParameters;
            _subReports = subReports;
            _userRpt = userRpt;

            ModifyTablix();

            return _table;
        }

        private void ModifyTablix()
        {
            // Remove existing Tablix items 
            // We will recreate from database configuration
            _table.TablixBody.TablixColumns.Clear();
            _table.TablixBody.TablixRows.Clear();
            _table.TablixColumnHierarchy.TablixMembers.Clear();
            _table.TablixRowHierarchy.TablixMembers.Clear();

            //
            // Create the columns
            // Create a TablixColumn for each field that is selected.
            //      NOTE: Currently only supporting row grouping (not columns)
            //
            var fields = from f in _fields
                         where f.ColumnOrder != -1
                         orderby f.ColumnOrder ascending
                         select f;

            int numCols = fields.ToList().Count;

            _numCols = numCols;
            // Attempt to autosize columns
            double pagewidth = _table.Width.ToInches();

            if (_formatTypeID == (int)FormatTypeEnum.Excel)
            {
                pagewidth = pagewidth + (numCols < 15 ? numCols * 0.7 : (numCols - 10) * 2);
                _table.Width = new ReportSize(pagewidth);
            }

            double sum = fields.Where(order => order.ColumnOrder != -1)
                                .Sum(width => (width.ColumnWidthFactor == -1) ? 1.0 : width.ColumnWidthFactor);
            double widthfactor = pagewidth / sum;

            foreach (RptField field in fields)
            {
                TablixColumn newcolumn = new TablixColumn();
                double width = 1.0 * widthfactor;
                if (field.ColumnWidthFactor > -1)
                {
                    width = field.ColumnWidthFactor * widthfactor;
                }
                if (field.ColumnOrder == -1)
                {
                    width = 0.0; // hidden
                }
                newcolumn.Width = new ReportSize(width);
                _table.TablixBody.TablixColumns.Add(newcolumn);
            }

            //
            // Create the header row 
            //  On the report this just shows the column names
            //  TODO: allow style differences?
            //
            TablixRow headerrow = new TablixRow();
            headerrow.Height = new ReportSize(0.25);

            foreach (RptField field in fields)
            {
                TablixCell col = new TablixCell();
                col = CreateHeaderCell(GetCellName("Header", field), field.Name.StartsWith("@") ? field.Name.Substring(1) : field.Name, field);

                headerrow.TablixCells.Add(col);
            }

            _table.TablixBody.TablixRows.Add(headerrow);

            //
            //  Create the group header row(s).
            //      On the report this will show the column name and value of the field being grouped by
            //

            // get the selected group by fields 
            var groups = (from g in _fields
                          where g.GroupOrder > -1 && g.IsGroupable
                          orderby g.GroupOrder ascending
                          select g).ToList();

            if (groups.Count > 0)
            {
                // Add the group header row(s)
                for (int j = 0; j <= groups.Count - 1; j++)
                {
                    var groupingField = groups[j];
                    TablixRow groupHeaderRow = new TablixRow();

                    Style groupStyle = new Style();
                    groupStyle.FontSize = new ReportExpression<ReportSize>(getFontSize());
                    groupStyle.FontWeight = new ReportExpression<FontWeights>("Bold");
                    groupStyle.BackgroundColor = new ReportExpression<ReportColor>("#E5E5E5");
                    groupStyle.BottomBorder = new Border();
                    groupStyle.BottomBorder.Color = new ReportExpression<ReportColor>("DimGray");
                    groupStyle.BottomBorder.Style = new ReportExpression<BorderStyles>(BorderStyles.Solid);
                    groupStyle.BottomBorder.Width = new ReportExpression<ReportSize>("1pt");
                    groupStyle.TextAlign = new ReportExpression<TextAlignments>(TextAlignments.Left);
                    groupStyle.VerticalAlign = new ReportExpression<VerticalAlignments>(VerticalAlignments.Bottom);

                    // the Group Header only has a value in the first column.
                    //  the output on the report will look simply like this: Column Name: [column value]
                    //  the expression should look like this: ="Column Name: " & Fields![SQLFieldName].Value
                    string cellValue = "=" + '"' + groupingField.Name + ": " + '"' + "& ";  //Example: ="Policy Period: " &                    
                    var fieldValueExpression = GetFieldValueExpression(groupingField, false);
                    cellValue += fieldValueExpression.Substring(1);
                    TablixCell col = new TablixCell();
                    var cellName = GetCellName("GroupHeader", groupingField);
                    col = CreateCell(cellName, cellValue, groupStyle, groupingField, numCols, false);

                    groupHeaderRow.TablixCells.Add(col);

                    // add empty cells for the remaining columns
                    for (int i = 1; i < numCols; i++)
                    {
                        if (_formatTypeID == (int)FormatTypeEnum.Excel)
                        {
                            //add last cell with cellvalue = empty => cell can grow if cellValue.lengh is so long
                            cellValue = string.Empty;
                            col = CreateCell(GetCellName("GroupHeader", groupingField), cellValue, groupStyle, groupingField, 1, false);
                            groupHeaderRow.TablixCells.Add(col);
                        }
                        else
                        {
                            var cellPdf = new TablixCell();
                            groupHeaderRow.TablixCells.Add(cellPdf);
                        }
                    }

                    _table.TablixBody.TablixRows.Add(groupHeaderRow);
                }
            }

            //
            // Create the details row
            //  on the report, this will show the actual values for the selected fields
            //  create a single TablixRow with a TablixCell for each selected field.
            //  TODO: allow style differences?
            //

            TablixRow datarow = new TablixRow();
            datarow.Height = new ReportSize(0.2);

            foreach (RptField field in fields)
            {
                Style datastyle = new Style();
                datastyle.FontSize = new ReportExpression<ReportSize>(getFontSize());

                TablixCell col = new TablixCell();

                string cellValue = GetFieldValueExpression(field, true);

                col = CreateCell(GetCellName("Detail", field), cellValue, datastyle, field, null, false);
                datarow.TablixCells.Add(col);
            }

            _table.TablixBody.TablixRows.Add(datarow);

            //
            //  Create the group footer row(s)
            //      on the report this will show the count of the group by field and typically the sum of all 
            //          numeric fields in the detail row (but it actually just uses the group expression associated
            //          with the field in the database, so it may be any expression--not always a sum)
            //
            if (groups.Count > 0)
            {
                // cycle through all the group by fields
                for (int j = groups.Count - 1; j >= 0; --j)
                {
                    var groupingField = groups[j];

                    // create the footer row
                    TablixRow groupFooterRow = new TablixRow();
                    groupFooterRow.Height = new ReportSize(0.25);

                    // create the footer style
                    Style groupStyle = new Style();
                    groupStyle.FontSize = new ReportExpression<ReportSize>(getFontSize());
                    groupStyle.FontWeight = new ReportExpression<FontWeights>("Bold");
                    groupStyle.BackgroundColor = new ReportExpression<ReportColor>("#E5E5E5");
                    groupStyle.BottomBorder = new Border();
                    groupStyle.BottomBorder.Color = new ReportExpression<ReportColor>("DimGray");
                    groupStyle.BottomBorder.Style = new ReportExpression<BorderStyles>(BorderStyles.Solid);
                    groupStyle.BottomBorder.Width = new ReportExpression<ReportSize>("1pt");
                    groupStyle.TextAlign = new ReportExpression<TextAlignments>(TextAlignments.Left);
                    groupStyle.VerticalAlign = new ReportExpression<VerticalAlignments>(VerticalAlignments.Bottom);

                    // cycle through all the fields selected for the report
                    //  and add those to the footer that were selected to appear there
                    //  (and in addition display the current group by field itself typically as a count)
                    bool groupLoad = false;
                    bool groupFieldLoad = false;
                    int groupSpanValue = -1;
                    int idx = 0;

                    sumTotalHierrachy++;
                    string cellName = string.Empty;
                    foreach (RptField field in fields)
                    {
                        string expression = "";
                        string groupSummaryExpression = GetGroupSummaryExpression(groupingField);
                        TablixCell col = new TablixCell();

                        if (!string.IsNullOrEmpty(groupSummaryExpression) && groupingField.GroupOrder != -1 && !groupLoad && field.SQLName == groupingField.SQLName && field.FieldValueExpression == groupingField.FieldValueExpression)
                        {
                            // special handling for the group by field itself.  Make sure to display it (typically with a count expression) in the footer.
                            expression = "=" + '"' + groupingField.Name + ": " + '"' + groupSummaryExpression;
                            groupSpanValue = GetSpanCol(groupingField, fields.Count());
                            col = CreateCell(GetCellName("GroupFooter", groupingField), expression, groupStyle, groupingField, groupSpanValue, false);
                            groupFooterRow.TablixCells.Add(col);
                            groupLoad = true;
                            idx = 0;
                        }
                        else
                        {
                            idx++;
                            // the _userSummarizeRptField  contains a list of the fields to add to the footer for this grouping 
                            if (_summarizeFields != null && _summarizeFields.Count() > 0)
                            {
                                expression = "";                       
                                foreach (RptField summarizeField in _summarizeFields)
                                {
                                    if (field.SQLName == summarizeField.SQLName && field.FieldValueExpression == summarizeField.FieldValueExpression)
                                    {
                                        expression = GetSummaryFieldValueExpression(summarizeField).Value;
                                        if (_userRpt.ReportID == (int)ReportEnum.ReserveChangeReport)
                                        {

                                           if (expression.Contains("TotalIncurredSum"))
                                           {
                                               expression = " =Sum(Fields!TotalIncurred.Value * Fields!TotalLevel0" + sumTotalHierrachy + ".Value )  ";                                           
                                           }
                                           if (expression.Contains("OutstandingReserveAllClassesSum"))
                                           {
                                               expression = " =Sum(Fields!OutstandingReserveAllClasses.Value * Fields!TotalLevel0" + sumTotalHierrachy + ".Value )  ";
                                           }
                                           if (expression.Contains("TotalRecoverySum"))
                                           {
                                               expression = " =Sum(Fields!TotalRecovery.Value * Fields!TotalLevel0" + sumTotalHierrachy + ".Value )  ";
                                           }
                                           if (expression.Contains("OutstandingReserveIncludedClassesSum"))
                                           {
                                               expression = " =Sum(Fields!OutstandingReserveIncludedClasses.Value * Fields!TotalLevel0" + sumTotalHierrachy + ".Value )  ";
                                           }
                                           if (expression.Contains("TotalDevelopedDeductibleLossSum"))
                                           {
                                               expression = " =Sum(Fields!TotalDevelopedDeductibleLoss.Value * Fields!TotalLevel0" + sumTotalHierrachy + ".Value )  ";
                                           }
                                           if (expression.Contains("TotalIncurredUpToDeductibleSum"))
                                           {
                                               expression = " =Sum(Fields!TotalIncurredUpToDeductible.Value * Fields!TotalLevel0" + sumTotalHierrachy + ".Value )  ";
                                           }
                                           if (expression.Contains("TotalPaidUpToDeductibleSum"))
                                           {
                                               expression = " =Sum(Fields!TotalPaidUpToDeductible.Value * Fields!TotalLevel0" + sumTotalHierrachy + ".Value )  ";
                                           }
                                           if (expression.Contains("AmountPaidOverSIRDedSum"))
                                           {
                                               expression = " =Sum(Fields!AmountPaidOverSIRDed.Value * Fields!TotalLevel0" + sumTotalHierrachy + ".Value )  ";
                                           }  
                                       }

                                        Style summaryStyle = (Style)groupStyle.DeepClone();
                                        summaryStyle.TopBorder = new Border();
                                        summaryStyle.TopBorder.Color = new ReportExpression<ReportColor>("DimGray");
                                        summaryStyle.TopBorder.Style = new ReportExpression<BorderStyles>(BorderStyles.Solid);
                                        summaryStyle.TopBorder.Width = new ReportExpression<ReportSize>("1pt");
                                        summaryStyle.TextAlign = new ReportExpression<TextAlignments>(TextAlignments.Right);
                                        col = CreateCell(GetCellName("GroupFooter", groupingField), expression, summaryStyle, summarizeField, -1, true);
                                        groupFooterRow.TablixCells.Add(col);
                                        groupFieldLoad = true;
                                    }

                                }
                                if (expression == "")
                                {
                                    if (groupLoad || groupFieldLoad)
                                    {
                                        if (idx <= groupSpanValue)
                                        {
                                            col = _formatTypeID == (int)FormatTypeEnum.Excel ? CreateEmptyCell(GetEmptyCellStyle()) : new TablixCell();
                                        }
                                        else
                                        {
                                            col = CreateCell(GetCellName("GroupFooter", field), expression, groupStyle, field, -1, false);
                                        }
                                    }
                                    else
                                    {
                                        col = CreateCell(GetCellName("GroupFooter", field), expression, groupStyle, field, -1, false);
                                    }
                                    groupFooterRow.TablixCells.Add(col);
                                }
                            }
                            else
                            {
                                // this field is not the group by field itself, nor is it selected to be displayed in the group footer.
                                //  just add an empty cell for it
                                expression = "";
                                if (groupLoad || groupFieldLoad)
                                {
                                    col = _formatTypeID == (int)FormatTypeEnum.Excel ? CreateCell(GetCellName("GroupFooter", field), expression, groupStyle, field, -1, false) : new TablixCell();

                                }
                                else
                                {
                                    // not positive, but I think this has to do with spacing--making sure the other cells added to the footer align properly.  
                                    col = CreateCell(GetCellName("GroupFooter", field), expression, groupStyle, field, -1, false);
                                }
                                groupFooterRow.TablixCells.Add(col);
                            }
                        }
                    }
                    groupFooterRow.Height = _formatTypeID == (int)FormatTypeEnum.Excel ? new ReportSize(0.4) : new ReportSize(0.2);
                    _table.TablixBody.TablixRows.Add(groupFooterRow);
                }
            }


            //*************Add Subreports

            //   AddSubreports(_table.TablixBody);

            //********************************** 

            //
            // Create the Tablix Column Hierarchy
            //  practically speaking this is just an empty TablixMember for every selected field
            //
            for (int i = 0; i < numCols; i++)
            {
                TablixMember tcmember = new TablixMember();
                _table.TablixColumnHierarchy.TablixMembers.Add(tcmember);
            }


            //
            // Create the Tablix Row Hierarchy
            //  This is where the report groupings are defined along with the sort order.
            //  If there are multiple groupings they are expressed hierarchically (i.e., one within the other)
            //  TODO: (dps) get an explanation from Sandra about the group by default stuff
            //
            // Header row
            TablixMember trmember = new TablixMember();
            trmember.KeepTogether = true;
            trmember.RepeatOnNewPage = true;
            trmember.KeepWithGroup = KeepWithGroupTypes.After;
            _table.TablixRowHierarchy.TablixMembers.Add(trmember);


            // **** Has Grouping by default*

            var groupsByDefault = (from g in _fields
                                   where g.IsGroupByDefault == true
                                   select g).ToList();

            TablixMember trmemberDefaultGroup = new TablixMember();

            if (groupsByDefault.Count > 0)
            {
                trmemberDefaultGroup = CreateGroupingByDefault(_table.TablixRowHierarchy.TablixMembers, groupsByDefault);
                CreateGroupings(trmemberDefaultGroup.TablixMembers, groups, 0);
            }
            else
            {
                CreateGroupings(_table.TablixRowHierarchy.TablixMembers, groups, 0);
            }

            // *****


            // Create SortExpression entries

            var sorts = (from s in _fields
                         where s.SortOrder > -1
                         orderby s.SortOrder ascending
                         select s).ToList();

            if (sorts.Count > 0)
            {
                foreach (var item in sorts)
                {
                    _table.SortExpressions.Add(GetSortExpression(item));
                }
            }

            //******subreport row******* /

            //if (groupsByDefault.Count > 0)
            //{
            //    AddSubReportRow(trmemberDefaultGroup.TablixMembers);
            //}
            //else
            //{
            //    AddSubReportRow(_table.TablixRowHierarchy.TablixMembers);
            //}

            //_table.Height = new ReportSize(_table.TablixRowHierarchy.TablixMembers.Count*0.5);
        }

        private void AddSubReportRow(IList<TablixMember> members)
        {
            if (_subReports.Count() == 0) return;

            TablixMember trmemberSubreport = new TablixMember();
            trmemberSubreport.KeepTogether = true;
            members.Add(trmemberSubreport);
        }

        private void AddSubreports(TablixBody tb)
        {
            if (_subReports.Count() == 0) return;

            var fields = from f in _fields
                         where f.ColumnOrder != -1
                         orderby f.ColumnOrder ascending
                         select f;

            Tablix tableSubReports = new Tablix();
            TablixRow rowSubReports = new TablixRow();
            TablixCell cellSubReports;

            var colIndex = 0;

            tableSubReports.Name = "tableSubReports";
            tableSubReports.KeepTogether = true;
            rowSubReports.Height = new ReportSize(0.30);

            foreach (RptField field in fields)
            {
                var column = new TablixColumn();
                var tcmember = new TablixMember();

                column.Width = _table.TablixBody.TablixColumns.ElementAt(colIndex).Width;

                tableSubReports.TablixBody.TablixColumns.Add(column);

                rowSubReports.TablixCells.Add(new TablixCell());


                tableSubReports.TablixColumnHierarchy.TablixMembers.Add(tcmember);

                colIndex++;
            }

            foreach (SubRpt subReportItem in _subReports)
            {
                TablixRow datarowSubReport = new TablixRow();
                datarowSubReport.Height = new ReportSize(0.10);
                TablixCell colSubReport = new TablixCell();
                Subreport subReport = new Subreport();
                List<Parameter> paramList = new List<Parameter>();

                var trmember = new TablixMember();

                int iterator = 0;

                foreach (RptField field in fields)
                {
                    if (iterator == 0)
                    {
                        var parameters = from f in _subReportParameters
                                         where f.SubReportID == subReportItem.SubReportID
                                         select f;

                        foreach (SubRptParameter item in parameters)
                        {
                            Parameter itemPara = new Parameter();
                            itemPara.Name = item.SubReportParameterName;
                            itemPara.Value = item.DefaultValue;
                            paramList.Add(itemPara);
                        }

                        subReport.Name = subReportItem.SubReportName;
                        subReport.ReportName = subReportItem.SubReportName;
                        subReport.Parameters = paramList;
                        subReport.KeepTogether = true;
                        colSubReport.CellContents = new CellContents();
                        colSubReport.CellContents.ColSpan = fields.Count();
                        colSubReport.CellContents.ReportItem = subReport;
                        iterator = iterator + 1;
                    }
                    else
                    {
                        colSubReport = new TablixCell();
                    }
                    datarowSubReport.TablixCells.Add(colSubReport);
                }

                tableSubReports.TablixBody.TablixRows.Add(datarowSubReport);

                tableSubReports.TablixRowHierarchy.TablixMembers.Add(trmember);
            }

            cellSubReports = rowSubReports.TablixCells.FirstOrDefault();
            cellSubReports.CellContents = new CellContents();
            cellSubReports.CellContents.ColSpan = fields.Count();
            cellSubReports.CellContents.ReportItem = tableSubReports;

            tb.TablixRows.Add(rowSubReports);
        }

        private int GetSpanCol(RptField item, int fieldCount)
        {
            int colSpan = 0;
            if (_formatTypeID == (int)FormatTypeEnum.Excel)
            {
                colSpan = -1;
                return colSpan;
            }

            if (_summarizeFields != null && _summarizeFields.Count() > 0)
            {
                var list = from f in _summarizeFields
                           orderby f.ColumnOrder ascending
                           select f;

                foreach (RptField field in list)
                {
                    if (field.ColumnOrder > item.ColumnOrder)
                    {
                        colSpan = (field.ColumnOrder - item.ColumnOrder);
                        if (colSpan == 0) { colSpan = -1; }
                        break;
                    }
                }

            }

            if (_summarizeFields == null || _summarizeFields.Count() == 0 || colSpan == 0)
            {
                colSpan = (fieldCount - item.ColumnOrder) + 1;
            }

            return colSpan;
        }

        private int GetGroupSpanCol(RptField item, RptField sumfield, int fieldCount)
        {
            //Logger.Write(String.Format("*** Field Name: {0} | Sum Field Name: {1} | Field Count: {2}", item.Name, sumfield.Name, fieldCount), "SLLog");

            int colSpan = 0;
            int colSpanGroup = 0;

            if (_formatTypeID == (int)FormatTypeEnum.Excel)
            {
                colSpan = -1;
                return colSpan;
            }

            //Logger.Write(String.Format("item.RptFieldSelected.Count: {0} ", item.RptFieldSelected == null ? "Null" : item.RptFieldSelected.Count().ToString()), "SLLog");

            if (_summarizeFields != null && _summarizeFields.Count() > 0)
            {
                var list = from f in _summarizeFields
                           orderby f.ColumnOrder ascending
                           select f;

                foreach (RptField field in list)
                {
                    if (field.ColumnOrder > sumfield.ColumnOrder)
                    {
                        colSpanGroup = (field.ColumnOrder - sumfield.ColumnOrder);
                        if (colSpanGroup == 0) { colSpanGroup = -1; }
                        break;
                    }
                }

                //Logger.Write(String.Format("colSpanGroup: {0}", colSpanGroup), "SLLog");
            }

            //Logger.Write(String.Format("sumfield.ColumnOrder: {0} | item.ColumnOrder: {1}", sumfield.ColumnOrder, item.ColumnOrder), "SLLog");
            if (sumfield.ColumnOrder < item.ColumnOrder)
            {
                colSpan = item.ColumnOrder - sumfield.ColumnOrder;
                if (colSpanGroup != 0 && colSpanGroup < colSpan)
                {
                    colSpan = colSpanGroup;
                }

                //Logger.Write(String.Format("Exiting Block 2 'if'.  colSpan: {0}", colSpan), "SLLog");
            }
            else
            {
                colSpan = colSpanGroup;
                //Logger.Write(String.Format("Exiting Block 2 'else'.  colSpan: {0}", colSpan), "SLLog");
            }

            if (colSpan == 0)
            {
                //Logger.Write(String.Format("In Block 3. Field Count: {0} | sumfield.ColumnOrder: {1}", fieldCount, sumfield.ColumnOrder), "SLLog");
                colSpan = (fieldCount - sumfield.ColumnOrder) + 1;
            }

            return colSpan;
        }

        private TablixCell CreateCell(string cellname, string cellvalue, Style cellstyle, RptField fieldInfo, int? colSpan, bool isSummaryCell)
        {
            TablixCell cell;

            cell = new TablixCell();

            cell.CellContents = new CellContents();
            Textbox txtbox = new Textbox();
            cell.CellContents.ReportItem = txtbox;
            txtbox.Name = cellname;
            txtbox.CanGrow = true;
            txtbox.CanShrink = true;
            txtbox.KeepTogether = true;
            txtbox.Paragraphs[0].TextRuns[0].Value = new ReportExpression(cellvalue);
            //txtbox.Height = cellvalue.Length >= 28 ? new ReportSize(0.5) : new ReportSize(0.25);

            Style mystyle = (Style)cellstyle.DeepClone();

            Style subStyle = new Style();

            // Handle currency formatting
            if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Money)
            {
                mystyle.Format = new ReportExpression("C");
                subStyle.TextAlign = new ReportExpression<TextAlignments>(TextAlignments.Right);
            }
            else if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Date)
            {
                mystyle.Format = new ReportExpression("d");
                subStyle.TextAlign = new ReportExpression<TextAlignments>(TextAlignments.Center);
            }
            else if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Time)
            {
                mystyle.Format = new ReportExpression("HH:mm");
                subStyle.TextAlign = new ReportExpression<TextAlignments>(TextAlignments.Center);
            }
            else
            {
                mystyle.Format = new ReportExpression();
                if (fieldInfo.DataType == DataTypeEnum.String)
                {
                    subStyle.TextAlign = new ReportExpression<TextAlignments>(TextAlignments.Left);
                }
                else if (fieldInfo.DataType == DataTypeEnum.Decimal)
                {
                    mystyle.Format = new ReportExpression("#,0.00;(#,0.00)");
                    subStyle.TextAlign = new ReportExpression<TextAlignments>(TextAlignments.Right);
                }
                else if (fieldInfo.DataType == DataTypeEnum.Percentage)
                {
                    mystyle.Format = new ReportExpression("0.00%");
                    subStyle.TextAlign = new ReportExpression<TextAlignments>(TextAlignments.Right);
                }
                else if (fieldInfo.DataType == DataTypeEnum.Integer)
                {
                    mystyle.Format = new ReportExpression("#,0;(#,0)");
                    subStyle.TextAlign = new ReportExpression<TextAlignments>(TextAlignments.Right);
                }
            }

            txtbox.Paragraphs[0].Style = subStyle;
            txtbox.Paragraphs[0].TextRuns[0].Style = mystyle;

            txtbox.Style = new Style();
            txtbox.Style.PaddingLeft = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingRight = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingTop = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingBottom = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.VerticalAlign = new ReportExpression<VerticalAlignments>(VerticalAlignments.Middle);
            txtbox.Style.BackgroundColor = new ReportExpression<ReportColor>("=iif(RowNumber(\"DataSet2\") Mod 2, \"White\", \"#E5E5E5\")");
            if (colSpan.HasValue)
            {
                //_formatTypeID == (int)FormatTypeEnum.Excel
                //if (colSpan != -1) { cell.CellContents.ColSpan = colSpan.Value; }
                if (colSpan != -1)
                {
                    cell.CellContents.ColSpan = _formatTypeID == (int)FormatTypeEnum.Excel ? 1 : colSpan.Value;
                }


                txtbox.Style.BottomBorder = cellstyle.BottomBorder;
                if (isSummaryCell && !_isSummaryOnly)
                {
                    txtbox.Style.TopBorder = cellstyle.TopBorder;
                }
                txtbox.Style.BackgroundColor = cellstyle.BackgroundColor;
                txtbox.Paragraphs[0].Style.TextAlign = cellstyle.TextAlign;
                txtbox.Style.VerticalAlign = cellstyle.VerticalAlign;
            }

            return cell;
        }

        private TablixCell CreateHeaderCell(string cellname, string cellvalue, RptField fieldInfo)
        {
            TablixCell cell;

            cell = new TablixCell();
            cell.CellContents = new CellContents();
            Textbox txtbox = new Textbox();
            cell.CellContents.ReportItem = txtbox;
            txtbox.Name = cellname;
            txtbox.KeepTogether = true;
            txtbox.CanGrow = true;
            txtbox.Paragraphs[0].TextRuns[0].Value = new ReportExpression(cellvalue);

            Style headerstyle = new Style();
            headerstyle.FontSize = new ReportExpression<ReportSize>(getFontSize());
            headerstyle.FontWeight = new ReportExpression<FontWeights>("Bold");
            headerstyle.Color = new ReportExpression<ReportColor>("#333b8c");

            Style subStyle = new Style();

            // Handle currency formatting
            if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Money)
            {
                headerstyle.Format = new ReportExpression("C");
                subStyle.TextAlign = new ReportExpression<TextAlignments>(TextAlignments.Right);
            }
            else if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Date)
            {
                headerstyle.Format = new ReportExpression("d");
                subStyle.TextAlign = new ReportExpression<TextAlignments>(TextAlignments.Center);
            }
            else if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Time)
            {
                headerstyle.Format = new ReportExpression("HH:mm");
                subStyle.TextAlign = new ReportExpression<TextAlignments>(TextAlignments.Center);
            }
            else if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Decimal)
            {
                headerstyle.Format = new ReportExpression("#,0.00;(#,0.00)");
                subStyle.TextAlign = new ReportExpression<TextAlignments>(TextAlignments.Right);
            }
            else if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Percentage)
            {
                subStyle.TextAlign = new ReportExpression<TextAlignments>(TextAlignments.Right);
            }
            else if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Integer)
            {
                headerstyle.Format = new ReportExpression("#,0;(#,0)");
                subStyle.TextAlign = new ReportExpression<TextAlignments>(TextAlignments.Right);
            }
            else
            {
                headerstyle.Format = new ReportExpression();
            }

            txtbox.Paragraphs[0].Style = subStyle;
            txtbox.Paragraphs[0].TextRuns[0].Style = headerstyle;

            txtbox.Style = new Style();
            txtbox.Style.PaddingLeft = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingRight = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingTop = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.PaddingBottom = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.TopBorder = new Border();
            txtbox.Style.TopBorder.Color = new ReportExpression<ReportColor>("DimGray");
            txtbox.Style.TopBorder.Style = new ReportExpression<BorderStyles>(BorderStyles.Solid);
            txtbox.Style.TopBorder.Width = new ReportExpression<ReportSize>("2pt");
            txtbox.Style.BottomBorder = new Border();
            txtbox.Style.BottomBorder.Color = new ReportExpression<ReportColor>("DimGray");
            txtbox.Style.BottomBorder.Style = new ReportExpression<BorderStyles>(BorderStyles.Solid);
            txtbox.Style.BottomBorder.Width = new ReportExpression<ReportSize>("1pt");
            txtbox.Style.BackgroundColor = new ReportExpression<ReportColor>("WhiteSmoke");
            txtbox.Style.VerticalAlign = new ReportExpression<VerticalAlignments>(VerticalAlignments.Middle);

            return cell;
        }

        private TablixCell CreateCellEx(string cellname, string cellvalue, Style cellstyle1, Style cellstyle2, RptField fieldInfo)
        {
            TablixCell cell;

            cell = new TablixCell();
            cell.CellContents = new CellContents();
            Textbox txtbox = new Textbox();
            cell.CellContents.ReportItem = txtbox;
            txtbox.Name = cellname.Replace("%", "");
            txtbox.KeepTogether = true;
            txtbox.CanGrow = true;
            txtbox.Paragraphs[0].TextRuns[0].Value = new ReportExpression(cellvalue);

            Style mystyle = (Style)cellstyle2.DeepClone();

            // Handle currency formatting
            if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Money)
            {
                mystyle.Format = new ReportExpression("C");
            }
            else if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Date)
            {
                mystyle.Format = new ReportExpression("d");
            }
            else if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Time)
            {
                mystyle.Format = new ReportExpression("HH:mm");
            }
            else
            {
                mystyle.Format = new ReportExpression();
            }

            txtbox.Paragraphs[0].TextRuns[0].Style = mystyle;

            txtbox.Style = cellstyle1;

            return cell;
        }

        private TablixMember CreateGroup(RptField field, DataTypeEnum dataType)
        {
            TablixMember tbmember = new TablixMember();

            tbmember.Group = new Group();
            tbmember.Group.Name = GetCellName("Group", field);
            ReportExpression re;

            if (string.IsNullOrEmpty(field.FieldValueExpression) || !string.IsNullOrEmpty(field.SQLName))
            {
                if (dataType == DataTypeEnum.Date)
                {
                    re = new ReportExpression(string.Format("=CDate(FormatDateTime(Fields!{0}.Value, DateFormat.ShortDate))", field.SQLName));
                }
                else if (dataType == DataTypeEnum.Time)
                {
                    re = new ReportExpression(string.Format("=FormatDateTime(Fields!{0}.Value, DateFormat.ShortTime)", field.SQLName));
                }
                else
                {
                    re = new ReportExpression(string.Format("=Fields!{0}.Value", field.SQLName));
                }
            }
            else
            {
                re = new ReportExpression(field.FieldValueExpression);
            }

            tbmember.Group.GroupExpressions.Add(re);

            //*** Allows a group ascending sort by default ***
            SortExpression sortGroup = new SortExpression();
            sortGroup.Value = re;
            sortGroup.Direction = ConvertSortDirection(SortDirectionEnum.Ascending);
            tbmember.SortExpressions.Add(sortGroup);

            //*** NEW METHOD FOR GROUP SORT GENERATION ***
            List<RptField> summaryFields = _summarizeFields;
            if (summaryFields != null)
            {
                List<RptField> sortfields = (from s in _fields
                                             where s.SortOrder > -1
                                             orderby s.SortOrder ascending
                                             select s).ToList();

                List<RptField> summaryFieldsToSortBy = new List<RptField>();
                foreach (RptField summaryField in summaryFields)
                {
                    foreach (RptField sortField in sortfields)
                    {
                        if (summaryField.ID == sortField.ID)
                        {
                            SortExpression sort = new SortExpression();
                            sort.Value = GetSummarySortExpression(sortField);
                            sort.Direction = ConvertSortDirection(sortField.SortDirection);
                            tbmember.SortExpressions.Add(sort);
                            break;
                        }
                    }
                }
            }

            //*** FORMER METHOD FOR GROUP SORT GENERATION ***
            //SortExpression se = new SortExpression();
            //se.Value = re;

            //var sortfields = from s in _fields
            //                 where s.SortOrder > -1 && s.SQLName == field.SQLName && s.FieldValueExpression == field.FieldValueExpression
            //                 orderby s.SortOrder ascending
            //                 select s;

            //List<RptField> sortedFields = sortfields.ToList();
            //if (sortedFields != null && sortedFields.Count == 1)
            //{
            //    if (sortedFields[0].SortDirection == SortDirectionEnum.Ascending)
            //    {
            //        se.Direction = SortDirections.Ascending;
            //    }
            //    else
            //    {
            //        se.Direction = SortDirections.Descending;
            //    }
            //}

            //tbmember.SortExpressions.Add(se);

            return tbmember;
        }

        private TablixMember CreateDetailGroup()
        {
            TablixMember tbmember = new TablixMember();

            tbmember.Group = new Group();
            tbmember.Group.Name = _table.Name + "_Details";

            // Sorting in Report without Grouping
            var sortfields = from s in _fields
                             where s.SortOrder > -1
                             orderby s.SortOrder ascending
                             select s;

            foreach (RptField item in sortfields)
            {
                tbmember.SortExpressions.Add(GetSortExpression(item));
            }

            return tbmember;
        }

        private void CreateGroupings(IList<TablixMember> members, List<RptField> groups, int index)
        {
            Visibility visibility = new Visibility();
            visibility.Hidden = true;
            if (groups.Count > index)
            {
                TablixMember tb = CreateGroup(groups[index], groups[index].DataType);
                tb.KeepTogether = true;
                tb.KeepWithGroup = KeepWithGroupTypes.None;

                if (groups[index].IncludePageBreak)
                {
                    PageBreak pageBreak = new PageBreak();
                    pageBreak.BreakLocation = BreakLocations.Between;
                    tb.Group.PageBreak = pageBreak;
                }

                if (index > 0)
                {
                    TablixMember tbAfter = new TablixMember();
                    tbAfter.KeepTogether = true;
                    tbAfter.KeepWithGroup = KeepWithGroupTypes.After;
                    members.Add(tbAfter);
                }

                members.Add(tb);
                CreateGroupings(tb.TablixMembers, groups, index + 1);
                if (index > 0)
                {
                    TablixMember tbAfter = new TablixMember();
                    tbAfter.KeepTogether = true;
                    tbAfter.KeepWithGroup = KeepWithGroupTypes.After;
                    members.Add(tbAfter);
                }
            }
            else
            {
                if (groups.Count > 0)
                {
                    TablixMember tbBefore = new TablixMember();
                    tbBefore.KeepWithGroup = KeepWithGroupTypes.After;
                    tbBefore.KeepTogether = true;
                    members.Add(tbBefore);
                }

                TablixMember tb = CreateDetailGroup();
                if (_isSummaryOnly)
                {
                    tb.Visibility = visibility;
                }

                members.Add(tb);
                if (groups.Count > 0)
                {
                    TablixMember tbAfter = new TablixMember();
                    tbAfter.KeepWithGroup = KeepWithGroupTypes.After;
                    tbAfter.KeepTogether = true;
                    members.Add(tbAfter);
                }
            }
        }

        private TablixMember CreateGroupingByDefault(IList<TablixMember> members, List<RptField> groups)
        {
            TablixMember tb = CreateGroup(groups[0], groups[0].DataType);
            tb.KeepTogether = true;
            tb.RepeatOnNewPage = true;
            tb.KeepWithGroup = KeepWithGroupTypes.None;
            if (groups[0].IncludePageBreak)
            {
                PageBreak pageBreak = new PageBreak();
                pageBreak.BreakLocation = BreakLocations.Between;
                tb.Group.PageBreak = pageBreak;
            }
            members.Add(tb);
            return tb;
        }

        private string GetCellName(string prefix, RptField field)
        {
            //TODO (dps): maybe it's better to just generate a random and not try to use the field name at all.  The downside would 
            //  be only that this might be harder to debug if you actually had to look at the RDL
            //TODO (dps): the best solution would be to add an explicit "ReportFieldName" in the database and separate this from the "FieldName" (which is the
            //  default display name) and the SQLFieldName (which is the expression to use to generate the value). 

            // control naming rules in RS:
            //  The name cannot contain spaces, and it must begin with a letter followed by letters, numbers, or the underscore character (_).

            string cleanFieldName = RX.Regex.Replace(field.Name, "[^0-9a-zA-Z_]+?", "");  // keeps only letters, numbers and "_"

            string cellName = String.Format("{0}{1}_{2}",
                String.IsNullOrEmpty(prefix) ? "ctl" : prefix.Trim() + "_",
                cleanFieldName,
                GetNextNumber().ToString());

            return cellName;
        }

        private string getFontSize()
        {
            if (_formatTypeID == (int)FormatTypeEnum.PDF)
            {
                if (_numCols < 11)
                {
                    return "8 pt";
                }
                else if (_numCols >= 11 && _numCols < 13)
                {
                    return "7 pt";
                }
                else if (_numCols >= 13 && _numCols < 15)
                {
                    return "6 pt";
                }
                else if (_numCols >= 15 && _numCols < 17)
                {
                    return "5 pt";
                }
                else if (_numCols >= 17 && _numCols < 30)
                {
                    return "4 pt";
                }
                else if (_numCols >= 30 && _numCols < 40)
                {
                    return "3 pt";
                }
                else if (_numCols >= 40 && _numCols < 48)
                {
                    return "2 pt";
                }

                else if (_numCols >= 48)
                {
                    return "1 pt";
                }
                else
                {
                    return "8 pt";
                }
            }
            else return "8 pt";
        }

        #region Summary Only
        public Tablix ModifyTablix(Tablix table, bool isSummaryOnly, string tablename, List<RptField> fields, List<RptField> summarizeFields, List<SubRpt> subReports, List<SubRptParameter> subReportParameters)
        {
            _table = table;
            _isSummaryOnly = isSummaryOnly;
            _tablename = tablename;
            _fields = fields;
            _summarizeFields = summarizeFields;
            _subReports = subReports;
            _subReportParameters = subReportParameters;

            if (isSummaryOnly)
            {
                ModifyTablixForSummaryOnlyReport();
            }
            else
            {
                ModifyTablix();
            }

            return _table;
        }

        private void ModifyTablixForSummaryOnlyReport()
        {
            #region Clear Tablix

            // Remove any existing Tablix items in the table. 
            //  We will recreate everything from scratch.
            _table.TablixBody.TablixColumns.Clear();
            _table.TablixBody.TablixRows.Clear();
            _table.TablixColumnHierarchy.TablixMembers.Clear();
            _table.TablixRowHierarchy.TablixMembers.Clear();

            #endregion

            #region Create the columns

            // Create a TablixColumn for:
            //  1) each group by field 
            //  2) each summary field 

            // get the selected group by fields.  
            //  check for the GroupOrder?
            var groupByFields = (from g in _fields
                                 where g.GroupOrder > -1 && g.IsGroupable
                                 orderby g.GroupOrder ascending
                                 select g).ToList();

            // get the selected summary fields 
            //  NOTE: the summarize fields are attached separately to each group by field, but it's always the same list for every grouping.  
            List<RptField> summaryFields = new List<RptField>();
            if (groupByFields != null && groupByFields.Count > 0)
            {
                if (_summarizeFields != null)
                {
                    summaryFields = _summarizeFields.OrderBy(f => f.ColumnOrder).ToList();
                }
            }

            int numCols = groupByFields.Count + summaryFields.Count;

            // Attempt to autosize columns
            double pageWidth = _table.Width.ToInches();
            double sum = groupByFields.Sum(width => (width.ColumnWidthFactor == -1) ? 1.0 : width.ColumnWidthFactor);
            sum += summaryFields.Sum(width => (width.ColumnWidthFactor == -1) ? 1.0 : width.ColumnWidthFactor);
            double widthFactor = pageWidth / sum;

            foreach (RptField groupByField in groupByFields)
            {
                double width = groupByField.ColumnWidthFactor > -1 ? groupByField.ColumnWidthFactor * widthFactor : 1.0 * widthFactor;
                TablixColumn newcolumn = new TablixColumn();
                newcolumn.Width = new ReportSize(width);
                _table.TablixBody.TablixColumns.Add(newcolumn);
            }

            foreach (RptField summaryField in summaryFields)
            {
                double width = summaryField.ColumnWidthFactor > -1 ? summaryField.ColumnWidthFactor * widthFactor : 1.0 * widthFactor;
                TablixColumn newcolumn = new TablixColumn();
                newcolumn.Width = new ReportSize(width);
                _table.TablixBody.TablixColumns.Add(newcolumn);
            }
            #endregion

            #region Create the column header row
            //  On the report this header row just shows the column names
            //      at the top of the page

            TablixRow headerRow = new TablixRow();
            headerRow.Height = new ReportSize(0.25);

            foreach (RptField groupByField in groupByFields)
            {
                TablixCell col = new TablixCell();
                col = CreateColumnHeaderCell(GetCellName("Header", groupByField), groupByField.Name.StartsWith("@") ? groupByField.Name.Substring(1) : groupByField.Name, groupByField, true, true);
                headerRow.TablixCells.Add(col);
            }

            foreach (RptField summaryField in summaryFields)
            {
                TablixCell col = new TablixCell();
                col = CreateColumnHeaderCell(GetCellName("Header", summaryField), summaryField.Name.StartsWith("@") ? summaryField.Name.Substring(1) : summaryField.Name, summaryField, true, false);
                headerRow.TablixCells.Add(col);
            }

            _table.TablixBody.TablixRows.Add(headerRow);
            #endregion

            #region Create the group header row(s)
            // On the report each group header row will show just the value 
            //      of the field being grouped by
            var cellValue = string.Empty;

            if (groupByFields.Count > 0)
            {
                // Add the group header row(s)
                for (int i = 0; i <= groupByFields.Count - 1; i++)
                {
                    bool isInnermostGrouping = (i == groupByFields.Count - 1);
                    if (isInnermostGrouping)
                        continue;

                    var groupByField = groupByFields[i];
                    TablixRow groupHeaderRow = new TablixRow();
                    groupHeaderRow.Height = new ReportSize(0.25);

                    Style groupStyle = GetGroupCellStyle();

                    // create empty cells for the previous group by fields, so the cellvalue for this group by field will align with the right column header
                    for (int j = 0; j < i; j++)
                    {
                        if (_formatTypeID == (int)FormatTypeEnum.PDF)
                        {
                            var cell = CreateEmptyCell(GetEmptyCellStyle());
                            groupHeaderRow.TablixCells.Add(cell);
                        }
                        else
                        {
                            cellValue = string.Empty;
                            var cellExcel = CreateCell(GetCellName("GroupHeader", groupByField), cellValue, groupStyle, groupByField, 1, false);
                            groupHeaderRow.TablixCells.Add(cellExcel);
                        }
                    }

                    //
                    // Create the cell containing the value for this group by field
                    //      the Group Header only has a value in a single column.
                    //      the output on the report is simply the value of the field placed in the column created for this field
                    //      the expression should look like this: ="Column Name: " & Fields![SQLFieldName].Value

                    //TODO: the expression to use should be in the database as the FieldValueExpression.  Most of this code is then unnecessary...
                    cellValue = GetFieldValueExpression(groupByField, false); //String.Empty;

                    TablixCell col = new TablixCell();

                    int colSpan = numCols - i;  // this cell should span all the remaining columns
                    col = CreateCell(GetCellName("GroupHeader", groupByField), cellValue, groupStyle, groupByField, colSpan, false);

                    groupHeaderRow.TablixCells.Add(col);



                    for (int j = i + 1; j < numCols; j++)
                    {

                        if (_formatTypeID == (int)FormatTypeEnum.Excel)
                        {
                            //add last cell with cellvalue = empty => cell can grow if cellValue.lengh is so long
                            cellValue = string.Empty;
                            col = CreateCell(GetCellName("GroupHeader", groupByField), cellValue, groupStyle, groupByField, 1, false);
                            groupHeaderRow.TablixCells.Add(col);
                        }
                        else
                        {
                            var cellPdf = new TablixCell();
                            groupHeaderRow.TablixCells.Add(cellPdf);
                        }
                    }

                    _table.TablixBody.TablixRows.Add(groupHeaderRow);
                }
            }
            #endregion

            if (groupByFields.Count > 1)
            {
                createSummaryOnlyHiddenDetailRow(numCols);
            }

            #region Create the group footer rows
            //
            //      on the report this will show the count of the group by field and typically the sum of all 
            //          numeric fields in the detail row (but it actually just uses the group expression associated
            //          with the field in the database, so it may be any expression--not always a sum)
            //
            if (groupByFields.Count > 0)
            {
                // cycle through all the group by fields from the innermost grouping to the outermost grouping
                //  so the footers will be in the correct order
                for (int i = groupByFields.Count - 1; i >= 0; --i)
                {
                    //bool isInnermostGrouping = (i == groupByFields.Count - 1);

                    var groupByField = groupByFields[i];

                    // create the footer row
                    TablixRow groupFooterRow = new TablixRow();
                    groupFooterRow.Height = new ReportSize(0.25);

                    // add a cell with no value for each column of any outer groupings 
                    //  so the value for this group by field lines up in the correct column


                    for (int j = 0; j < i; j++)
                    {

                        Style emptyCellStyle = GetEmptyCellStyle();
                        if (_formatTypeID == (int)FormatTypeEnum.PDF)
                        {
                            var cell = CreateEmptyCell(GetEmptyCellStyle());
                            groupFooterRow.TablixCells.Add(cell);
                        }
                        else
                        {
                            cellValue = string.Empty;
                            var cellExcel = CreateCell(GetCellName("GroupHeader", groupByField), cellValue, emptyCellStyle, groupByField, 1, false);
                            groupFooterRow.TablixCells.Add(cellExcel);
                        }
                    }

                    // add a cell to display the value for this group by field
                    cellValue = GetGroupSummaryExpression(groupByField).Replace("& \" = \" &", "").Trim();
                    if (cellValue.StartsWith("&"))
                    {
                        cellValue = cellValue.Substring(1);
                    }
                    cellValue = "=" + cellValue.Trim();

                    int colSpan = groupByFields.Count - i;  // allow the outer groupings to span the inner grouping columns, because no value will be displayed there

                    if (_formatTypeID == (int)FormatTypeEnum.Excel)
                    {
                        colSpan = 1;
                    }

                    Style cellStyle = GetGroupCellStyle();
                    //if (isInnermostGrouping)
                    //{
                    //    //cellStyle.BackgroundColor = new ReportExpression<ReportColor>("=iif(RowNumber(\"DataSet2\") Mod 2, \"White\", \"#E5E5E5\")");
                    //    cellStyle.BackgroundColor = new ReportExpression<ReportColor>("White");
                    //}
                    //TODO: what does"isSummaryCell" do?

                    TablixCell col = CreateCell(GetCellName("GroupFooter", groupByField), cellValue, cellStyle, groupByField, colSpan, false);
                    groupFooterRow.TablixCells.Add(col);

                    //add empty cells for any innermore group by columns
                    for (int j = i + 1; j < groupByFields.Count; j++)
                    {
                        // this empty cell doesn't have to have anything in it because it is 
                        //  part of the previous column's colspan             
                        var expressionEmpty = string.Empty;
                        var cell = _formatTypeID == (int)FormatTypeEnum.Excel ? CreateCell(GetCellName("GroupFooter", groupByField), expressionEmpty, cellStyle, groupByField, 1, false) : new TablixCell();
                        groupFooterRow.TablixCells.Add(cell);
                    }


                    // add a cell for each summary field
                    foreach (RptField summaryField in summaryFields)
                    {
                        string expression = GetSummaryFieldValueExpression(summaryField).Value;

                        Style summaryCellStyle = GetSummaryCellStyle();
                        //if (isInnermostGrouping)
                        //{
                        //    //summaryCellStyle.BackgroundColor = new ReportExpression<ReportColor>("=iif(RowNumber(\"DataSet2\") Mod 2, \"White\", \"#E5E5E5\")");
                        //    summaryCellStyle.BackgroundColor = new ReportExpression<ReportColor>("White");
                        //}

                        TablixCell cell = CreateCell(GetCellName("GroupFooterTotal", summaryField), expression, summaryCellStyle, summaryField, -1, true);
                        groupFooterRow.TablixCells.Add(cell);
                    }

                    _table.TablixBody.TablixRows.Add(groupFooterRow);
                }
            }
            #endregion

            if (groupByFields.Count == 1)
            {
                createSummaryOnlyHiddenDetailRow(numCols);
            }

            #region Create the Tablix Column Hierarchy
            // practically speaking this is just an empty TablixMember for every column

            for (int i = 0; i < numCols; i++)
            {
                TablixMember tcmember = new TablixMember();
                _table.TablixColumnHierarchy.TablixMembers.Add(tcmember);
            }
            #endregion

            #region Create the Tablix Row Hierarchy
            //  This is where the report groupings are defined along with the sort order.
            //  If there are multiple groupings they are expressed hierarchically (i.e., one within the other)
            //  TODO: (dps) get an explanation from Sandra about the group by default stuff
            //
            // add the column header row
            TablixMember trmember = new TablixMember();
            trmember.KeepTogether = true;
            trmember.RepeatOnNewPage = true;
            trmember.KeepWithGroup = KeepWithGroupTypes.After;
            _table.TablixRowHierarchy.TablixMembers.Add(trmember);

            Visibility visibility = new Visibility();
            visibility.Hidden = true;
            // create each grouping and assemble the groups into a hierarchy
            TablixMember parentGrouping = null;
            for (int i = 0; i < groupByFields.Count; i++)
            {
                RptField groupByField = groupByFields[i];

                bool hasHeader = !(i == groupByFields.Count - 1); // the last grouping will have no header, just a "footer" that will look like a detail row
                TablixMember grouping = CreateGroupingForSummaryOnlyReport(groupByField, hasHeader, true);

                if (i == 0)
                {
                    // this is the first grouping, so add it to the TablixRowHierarchy's TablixMember list
                    _table.TablixRowHierarchy.TablixMembers.Add(grouping);
                }
                else
                {
                    // this is not the first grouping, so insert this into the parent grouping's TablixMember list
                    //  For now, depend on the fact that every grouping will have both a header and footer, 
                    //  except the innermost one (i.e., detail row), so we can always insert this at index 1
                    //  between the header and footer

                    parentGrouping.TablixMembers.Insert(1, grouping);
                }

                if (groupByFields.Count - 1 == i)
                {
                    TablixMember tbmember = new TablixMember();
                    tbmember.Group = new Group();
                    tbmember.Group.Name = _table.Name + "_Details";
                    tbmember.Visibility = visibility;
                    if (groupByFields.Count == 1)
                    {
                        grouping.TablixMembers.Add(tbmember);
                    }
                    else
                    {
                        grouping.TablixMembers.Insert(0, tbmember);
                    }
                }

                parentGrouping = grouping;
            }

            ////// **** Has Grouping by default*

            ////var groupsByDefault = (from g in _fields
            ////                       where g.IsGroupByDefault == true
            ////                       select g).ToList();

            ////TablixMember trmemberDefaultGroup = new TablixMember();

            ////if (groupsByDefault.Count > 0)
            ////{
            ////    trmemberDefaultGroup = CreateGroupingByDefault(_table.TablixRowHierarchy.TablixMembers, groupsByDefault);
            ////    CreateGroupings(trmemberDefaultGroup.TablixMembers, groupByFields, 0);
            ////}
            ////else
            ////{

            //TODO: test whether we need to call this at all if there are no group by fields
            ////        CreateGroupHierarchyForSummaryOnlyReport(_table.TablixRowHierarchy.TablixMembers, groupByFields, 0);
            ////}

            ////// *****        

            #endregion

            #region Create the sort expressions

            // For summary only reports, I don't think the sorts are really relevant 
            //  I think the only sorting that matters is the sorts attached at the group level

            #endregion

            //_table.Height = new ReportSize(_table.TablixRowHierarchy.TablixMembers.Count * 0.5);
        }

        private void createSummaryOnlyHiddenDetailRow(int numCols)
        {
            TablixRow datarow = new TablixRow();
            datarow.Height = _formatTypeID != (int)FormatTypeEnum.Excel ? new ReportSize(0.000001) : new ReportSize(0.001);
            
            for (int k = 0; k < numCols; k++)
            {
                Style datastyle = new Style();
                datastyle.FontSize = new ReportExpression<ReportSize>(getFontSize());

                TablixCell emptyCol = new TablixCell();

                emptyCol = CreateEmptyCell(datastyle);
                datarow.TablixCells.Add(emptyCol);
            }

            _table.TablixBody.TablixRows.Add(datarow);
        }

        private TablixMember CreateGroupingForSummaryOnlyReport(RptField groupByField, bool hasHeader, bool hasFooter)
        {
            // a "grouping" is a TablixMember that consists of the following xml structure:
            //<TablixMember> 
            //
            //    <Group Name="Group1"> 
            //        <!-- Absent entirely if there are no group expressions -->  
            //        <GroupExpressions> 
            //            <GroupExpression>=Fields!MyField.Value</GroupExpression> 
            //        </GroupExpressions> 
            //    </Group> 
            //
            //    <!-- Absent entirely if there are no sort expressions for this group -->  
            //    <SortExpressions> 
            //        <SortExpression> 
            //            <Value>=Fields!MyField.Value</Value> 
            //        </SortExpression> 
            //    </SortExpressions> 
            //
            //    <TablixMembers> 
            //        <!-- represents the group's header row; absent entirely if there is no header-->
            //        <TablixMember> 
            //            <KeepWithGroup>After</KeepWithGroup> 
            //        </TablixMember> 
            //
            //        [ THIS IS WHERE ANY INNER GROUPING IS INSERTED AS ANOTHER TablixMember, 
            //                INLCUDING THE DETAIL ROW WHICH IS REALLY JUST ANOTHER GROUP WITHOUT 
            //                A HEADER, FOOTER, OR GROUP EXPRESSION ]
            //
            //        <!-- represents the group's footer row; absent entirely if there is no footer-->
            //        <TablixMember> 
            //            <KeepWithGroup>Before</KeepWithGroup> 
            //        </TablixMember> 
            //    </TablixMembers> 
            //</TablixMember> 

            // create the tablix member for this grouping, including any page breaks
            TablixMember groupingTablixMember = new TablixMember();
            groupingTablixMember.KeepTogether = true;
            groupingTablixMember.KeepWithGroup = KeepWithGroupTypes.None;

            // create the group node itself and add a group expression and page break
            groupingTablixMember.Group = new Group();
            groupingTablixMember.Group.Name = GetCellName("Group", groupByField);
            ReportExpression re;

            if (string.IsNullOrEmpty(groupByField.FieldValueExpression) || !string.IsNullOrEmpty(groupByField.SQLName))
            {
                if (groupByField.DataType == DataTypeEnum.Date)
                {
                    re = new ReportExpression(string.Format("=CDate(FormatDateTime(Fields!{0}.Value, DateFormat.ShortDate))", groupByField.SQLName));
                }
                else if (groupByField.DataType == DataTypeEnum.Time)
                {
                    re = new ReportExpression(string.Format("=FormatDateTime(Fields!{0}.Value, DateFormat.ShortTime)", groupByField.SQLName));
                }
                else
                {
                    re = new ReportExpression(string.Format("=RTrim(Fields!{0}.Value)", groupByField.SQLName));
                }
            }
            else
            {
                re = new ReportExpression(groupByField.FieldValueExpression);
            }

            groupingTablixMember.Group.GroupExpressions.Add(re);

            if (groupByField.IncludePageBreak)
            {
                PageBreak pageBreak = new PageBreak();
                pageBreak.BreakLocation = BreakLocations.Between;
                groupingTablixMember.Group.PageBreak = pageBreak;
            }

            //
            // create the sort expressions
            //

            // find the sort fields that are also summary fields 
            //  and sort the groups by the summary field group expression
            //  For example, this will allow the groups themselves to be sorted by the sum of the transaction amount

            List<RptField> summaryFields = _summarizeFields;
            if (summaryFields != null)
            {
                List<RptField> sortfields = (from s in _fields
                                             where s.SortOrder > -1
                                             orderby s.SortOrder ascending
                                             select s).ToList();

                List<RptField> summaryFieldsToSortBy = new List<RptField>();
                foreach (RptField summaryField in summaryFields)
                {
                    foreach (RptField sortField in sortfields)
                    {
                        if (summaryField.ID == sortField.ID)
                        {
                            SortExpression sort = new SortExpression();
                            sort.Value = GetSummarySortExpression(sortField);
                            sort.Direction = ConvertSortDirection(sortField.SortDirection);
                            groupingTablixMember.SortExpressions.Add(sort);
                            break;
                        }
                    }
                }
            }

            // automatically sort by the group by field
            //  but make sure the sort direction is correct if the user specified a sort for this field
            SortExpression se = new SortExpression();
            se.Value = re;

            if (groupByField.SortOrder > -1)
            {
                se.Direction = ConvertSortDirection(groupByField.SortDirection);
            }

            groupingTablixMember.SortExpressions.Add(se);

            // create the TablixMember for the header row
            if (hasHeader)
            {
                TablixMember headerRow = new TablixMember();
                headerRow.KeepTogether = true;
                headerRow.KeepWithGroup = KeepWithGroupTypes.After;
                groupingTablixMember.TablixMembers.Add(headerRow);
            }

            // create the TablixMember for the footer row
            if (hasFooter)
            {
                TablixMember footerRow = new TablixMember();
                footerRow.KeepTogether = true;
                footerRow.KeepWithGroup = KeepWithGroupTypes.After;
                groupingTablixMember.TablixMembers.Add(footerRow);
            }

            return groupingTablixMember;
        }

        private Style GetGroupCellStyle()
        {
            Style style = new Style();
            style.FontSize = new ReportExpression<ReportSize>(getFontSize());
            style.FontWeight = new ReportExpression<FontWeights>("Bold");
            style.BackgroundColor = new ReportExpression<ReportColor>("#E5E5E5");
            style.BottomBorder = new Border();
            style.BottomBorder.Color = new ReportExpression<ReportColor>("DimGray");
            style.BottomBorder.Style = new ReportExpression<BorderStyles>(BorderStyles.Solid);
            style.BottomBorder.Width = new ReportExpression<ReportSize>("1pt");
            style.TextAlign = new ReportExpression<TextAlignments>(TextAlignments.Left);
            style.VerticalAlign = new ReportExpression<VerticalAlignments>(VerticalAlignments.Bottom);

            return style;
        }

        private Style GetSummaryCellStyle()
        {
            Style style = new Style();
            style.FontSize = new ReportExpression<ReportSize>(getFontSize());
            style.FontWeight = new ReportExpression<FontWeights>("Bold");
            style.BackgroundColor = new ReportExpression<ReportColor>("#E5E5E5");

            style.BottomBorder = new Border();
            style.BottomBorder.Color = new ReportExpression<ReportColor>("DimGray");
            style.BottomBorder.Style = new ReportExpression<BorderStyles>(BorderStyles.Solid);
            style.BottomBorder.Width = new ReportExpression<ReportSize>("1pt");

            style.TopBorder = new Border();
            style.TopBorder.Color = new ReportExpression<ReportColor>("DimGray");
            style.TopBorder.Style = new ReportExpression<BorderStyles>(BorderStyles.Solid);
            style.TopBorder.Width = new ReportExpression<ReportSize>("1pt");

            style.TextAlign = new ReportExpression<TextAlignments>(TextAlignments.Right);
            style.VerticalAlign = new ReportExpression<VerticalAlignments>(VerticalAlignments.Bottom);

            return style;
        }

        private SortDirections ConvertSortDirection(SortDirectionEnum sortDirection)
        {
            if (sortDirection == SortDirectionEnum.Ascending)
                return SortDirections.Ascending;
            else
                return SortDirections.Descending;
        }

        private Style GetEmptyCellStyle()
        {
            Style style = new Style();
            style.BackgroundColor = new ReportExpression<ReportColor>("#E5E5E5");
            style.BottomBorder = new Border();
            style.BottomBorder.Color = new ReportExpression<ReportColor>("DimGray");
            style.BottomBorder.Style = new ReportExpression<BorderStyles>(BorderStyles.Solid);
            style.BottomBorder.Width = new ReportExpression<ReportSize>("1pt");

            return style;
        }

        private TablixCell CreateEmptyCell(Style emptyCellStyle)
        {
            TablixCell emptyCell = new TablixCell();
            emptyCell.CellContents = new CellContents();    // force the cell contents node to be written out to the xml 

            // put a textbox in the empty cell in order to apply a style
            Textbox emptyTextbox = new Textbox();
            emptyTextbox.Name = "GroupHeader_Empty" + GetNextNumber();
            emptyTextbox.Style = emptyCellStyle;
            emptyCell.CellContents.ReportItem = emptyTextbox;

            return emptyCell;
        }

        private string GetFieldValueExpression(RptField field, Boolean isDetail)
        {
            var expression = string.Empty;

            if (field.FieldValueExpression == null)
            {
                if (isDetail)
                {
                    expression = string.Format("=Fields!{0}.Value", field.SQLName);
                }
                else
                {
                    if (field.DataType == DataTypeEnum.Date)
                    {
                        expression = string.Format("=IIf(IsNothing(Fields!{0}.Value), \"\",FormatDateTime(Fields!{0}.Value, DateFormat.ShortDate))", field.SQLName);
                    }
                    else if (field.DataType == DataTypeEnum.Time)
                    {
                        expression = string.Format("=FormatDateTime(Fields!{0}.Value, DateFormat.ShortTime)", field.SQLName);
                    }
                    else if (field.DataType == DataTypeEnum.Money)
                    {
                        expression = string.Format("=Cstr(FormatCurrency(Fields!{0}.Value,2))", field.SQLName);
                    }
                    else
                    {
                        expression = string.Format("=Fields!{0}.Value", field.SQLName);
                    }
                }
            }
            else
            {
                expression = field.FieldValueExpression;
            }

            return expression;
        }

        private string GetGroupSummaryExpression(RptField field)
        {
            var groupSummaryExpression = string.Empty;

            if (field.GroupSummaryExpression == null)
            {
                if (field.DataType == DataTypeEnum.Date)
                {
                    groupSummaryExpression = string.Format(" & IIf(IsNothing(Fields!{0}.Value), \"\",FormatDateTime(Fields!{0}.Value, DateFormat.ShortDate)) & \" - Count = \" & CStr(FormatNumber(Count(IIf(IsNothing(FormatDateTime(Fields!{0}.Value, DateFormat.ShortDate)), \"NULL\",FormatDateTime(Fields!{0}.Value, DateFormat.ShortDate))),0))", field.SQLName);
                }
                else if (field.DataType == DataTypeEnum.Time)
                {
                    groupSummaryExpression = string.Format(" & FormatDateTime(Fields!{0}.Value, DateFormat.ShortTime) & \" - Count = \" & CStr(FormatNumber(Count(IIf(IsNothing(FormatDateTime(Fields!{0}.Value, DateFormat.ShortTime)), \"NULL\",FormatDateTime(Fields!{0}.Value, DateFormat.ShortTime))),0))", field.SQLName);
                }
                else if (field.DataType == DataTypeEnum.Percentage)
                {
                    groupSummaryExpression = string.Format(" & Sum(Fields!{0}.Value)", field.SQLName);
                }
                else if (field.DataType == DataTypeEnum.Money)
                {
                    groupSummaryExpression = string.Format(" & IIf(Count(Fields!{0}.Value) = 0, \"\",FormatCurrency(Min(Fields!{0}.Value)))  & \" - Count = \" & CStr(FormatNumber(CountRows(),0))", field.SQLName);
                }
                else
                {
                    groupSummaryExpression = string.Format(" & RTrim(Fields!{0}.Value) & \" - Count = \" & CStr(FormatNumber(Count(IIf(IsNothing(Fields!{0}.Value), \"NULL\",Fields!{0}.Value)),0))", field.SQLName);
                }
            }
            else
            {
                groupSummaryExpression = field.GroupSummaryExpression;
            }

            return groupSummaryExpression;
        }

        private SortExpression GetSortExpression(RptField field)
        {
            SortExpression sortExpression = new SortExpression();
            if (!string.IsNullOrEmpty(field.FieldValueExpression))
            {
                sortExpression.Value = !string.IsNullOrEmpty(field.SQLName) ? (field.SQLName.IndexOf(" ") > 0 ?  field.SQLName:  new ReportExpression(string.Format("=Fields!{0}.Value", field.SQLName))): field.FieldValueExpression;
            }
            else
            {
                if (field.DataType == DataTypeEnum.Date)
                {
                    sortExpression.Value = new ReportExpression(string.Format("=CDate(FormatDateTime(Fields!{0}.Value, DateFormat.ShortDate))", field.SQLName));
                }
                else if (field.DataType == DataTypeEnum.Time)
                {
                    sortExpression.Value = new ReportExpression(string.Format("=FormatDateTime(Fields!{0}.Value, DateFormat.ShortTime)", field.SQLName));
                }
                else
                {
                    sortExpression.Value = new ReportExpression(string.Format("=Fields!{0}.Value", field.SQLName));
                }
            }

            if (field.SortDirection == SortDirectionEnum.Descending)
            {
                sortExpression.Direction = SortDirections.Descending;
            }
            else
            {
                sortExpression.Direction = SortDirections.Ascending;
            }

            return sortExpression;
        }

        private ReportExpression GetSummarySortExpression(RptField field)
        {
            var expression = GetGroupSummaryExpression(field).Replace("& \" = \" &", "").Trim();
            expression = (expression.StartsWith("&")) ? expression.Substring(1) : expression;

            if (field.DataType == DataTypeEnum.Integer || field.DataType == DataTypeEnum.Money)
            {
                if (!string.IsNullOrEmpty(field.SQLName))
                {
                    expression = field.SQLName.StartsWith("=") ? string.Format("Sum({0})", field.SQLName.Substring(1).Trim()) : string.Format("Sum(Fields!{0}.Value)", field.SQLName.Trim());
                }
                else
                {
                    expression = string.Format("Sum({0})", field.FieldValueExpression.Substring(1).Trim());
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(field.SQLName))
                {
                    expression = field.SQLName.StartsWith("=") ? field.SQLName.Substring(1).Trim() : field.SQLName;
                }
                else
                {
                    expression = field.FieldValueExpression.Substring(1).Trim();
                }
            }

            if (field.Name == "Premium Loss Ratio")
            {
                expression = expression.Replace("Fields", "Sum(Fields").Replace("Value", "Value)");
            }

            return new ReportExpression("=" + expression);
        }

        private ReportExpression GetSummaryFieldValueExpression(RptField field)
        {
            //TODO: throw exception if this is not a summarizable field 
            //  once the IsSummarizable field is available in the database

            //NOTE: the anomalies noted below should be taken care of in task #4231.

            // default to just using the GroupExpression (slightly modified)
            var expression = GetGroupSummaryExpression(field).Replace("& \" = \" &", "").Trim();
            expression = (expression.StartsWith("&")) ? expression.Substring(1) : expression;

            if (field.DataType == DataTypeEnum.Integer || (field.DataType == DataTypeEnum.Money && field.Name != "Avg Indemnity Paid" && field.Name != "Avg Medical Paid" && field.Name != "Avg Legal Paid"))
            {
                if (!string.IsNullOrEmpty(field.FieldValueExpression))
                {
                    expression = string.Format("Sum({0})", field.FieldValueExpression.Substring(1));
                }
                else
                {
                    if (!field.UseSummaryExpresionGroup) 
                    {
                        expression = string.Format("Sum(Fields!{0}.Value)", field.SQLName);
                    }  
                }
            }

            
            return new ReportExpression("=" + expression);
        }

        private TablixCell CreateColumnHeaderCell(string cellname, string cellvalue, RptField fieldInfo, bool isSummaryOnly, bool isGroupByField)
        {
            Textbox txtbox = new Textbox();
            txtbox.Name = cellname;
            txtbox.KeepTogether = true;
            txtbox.CanGrow = true;
            txtbox.CanShrink = true;
            txtbox.Paragraphs[0].TextRuns[0].Value = new ReportExpression(cellvalue);

            TablixCell cell = new TablixCell();
            cell.CellContents = new CellContents();
            cell.CellContents.ReportItem = txtbox;

            // textbox style
            Style textBoxStyle = new Style();
            textBoxStyle.PaddingLeft = new ReportExpression<ReportSize>("1pt");
            textBoxStyle.PaddingRight = new ReportExpression<ReportSize>("1pt");
            textBoxStyle.PaddingTop = new ReportExpression<ReportSize>("1pt");
            textBoxStyle.PaddingBottom = new ReportExpression<ReportSize>("1pt");
            textBoxStyle.TopBorder = new Border();
            textBoxStyle.TopBorder.Color = new ReportExpression<ReportColor>("DimGray");
            textBoxStyle.TopBorder.Style = new ReportExpression<BorderStyles>(BorderStyles.Solid);
            textBoxStyle.TopBorder.Width = new ReportExpression<ReportSize>("2pt");
            textBoxStyle.BottomBorder = new Border();
            textBoxStyle.BottomBorder.Color = new ReportExpression<ReportColor>("DimGray");
            textBoxStyle.BottomBorder.Style = new ReportExpression<BorderStyles>(BorderStyles.Solid);
            textBoxStyle.BottomBorder.Width = new ReportExpression<ReportSize>("1pt");
            textBoxStyle.BackgroundColor = new ReportExpression<ReportColor>("WhiteSmoke");
            textBoxStyle.VerticalAlign = new ReportExpression<VerticalAlignments>(VerticalAlignments.Middle);
            txtbox.Style = textBoxStyle;

            // paragraph style
            Style paragraphStyle = new Style();
            paragraphStyle.TextAlign = GetTextAlignment(fieldInfo, isSummaryOnly, isGroupByField);
            txtbox.Paragraphs[0].Style = paragraphStyle;

            // textrun style
            Style textRunStyle = new Style();
            textRunStyle.Format = GetCellValueFormat(fieldInfo);
            textRunStyle.FontSize = new ReportExpression<ReportSize>(getFontSize());
            textRunStyle.FontWeight = new ReportExpression<FontWeights>("Bold");
            textRunStyle.Color = new ReportExpression<ReportColor>("#333b8c");
            txtbox.Paragraphs[0].TextRuns[0].Style = textRunStyle;

            return cell;
        }

        private ReportExpression<TextAlignments> GetTextAlignment(RptField fieldInfo, bool isSummaryOnly, bool isGroupByField)
        {
            TextAlignments alignment = TextAlignments.Default;

            if (isSummaryOnly && isGroupByField)
            {
                // these should always be left-aligned because regardless of data type they really always become
                //  strings because we append some text after indicating the count
                alignment = TextAlignments.Left;
            }
            else if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Money)
            {
                alignment = TextAlignments.Right;
            }
            else if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Date)
            {
                alignment = TextAlignments.Center;
            }
            else if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Time)
            {
                alignment = TextAlignments.Center;
            }
            else if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Decimal)
            {
                alignment = TextAlignments.Right;
            }
            else if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Percentage)
            {
                alignment = TextAlignments.Right;
            }
            else if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Integer)
            {
                alignment = TextAlignments.Right;
            }

            return new ReportExpression<TextAlignments>(alignment); ;
        }

        private ReportExpression GetCellValueFormat(RptField fieldInfo)
        {
            ReportExpression expression = null;
            if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Money)
            {
                expression = new ReportExpression("C");
            }
            else if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Date)
            {
                expression = new ReportExpression("d");
            }
            else if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Time)
            {
                expression = new ReportExpression("HH:mm");
            }
            else if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Decimal)
            {
                expression = new ReportExpression("#,0.00;(#,0.00)");
            }
            else if (fieldInfo != null && fieldInfo.DataType == DataTypeEnum.Integer)
            {
                expression = new ReportExpression("#,0;(#,0)");
            }
            else
            {
                expression = new ReportExpression();
            }

            return expression;
        }
        #endregion

        #region Unused Code
        //public Tablix CreateTablix(string tablename, List<RptField> fields)
        //{
        //    _tablename = "table" + _tableNum.ToString();
        //    _tableNum++;

        //    _fields = fields;
        //    _table = new Tablix();
        //    _table.Name = _tablename;

        //    CreateTablix();

        //    return _table;
        //}

        //private void CreateTablix()
        //{

        //    int numCols = _fields.Count;

        //    // Empty TablixBody is created by default

        //    // Create TablixColumns

        //    for (int i = 0; i < numCols; i++)
        //    {
        //        TablixColumn newcolumn = new TablixColumn();
        //        double width = 1.0;
        //        if (_fields[i].Width > -1)
        //        {
        //            width = _fields[i].Width;
        //        }
        //        newcolumn.Width = new ReportSize(width);
        //        _table.TablixBody.TablixColumns.Add(newcolumn);
        //    }

        //    // Create HeaderRow TablixRow

        //    TablixRow headerrow = new TablixRow();
        //    headerrow.Height = new ReportSize(0.25);
        //    Style headerstyle = new Style();
        //    headerstyle.FontSize = new ReportExpression<ReportSize>("12 pt");
        //    headerstyle.FontWeight = new ReportExpression<FontWeights>("Bold");
        //    headerstyle.Color = new ReportExpression<ReportColor>("#333b8c");


        //    for (int i = 0; i < numCols; i++)
        //    {

        //        TablixCell col = new TablixCell();
        //        //col = CreateCell("Header_" + _fields[i].SQLName, _fields[i].Name, headerstyle, _fields[i], null);
        //        col = CreateCell(GetCellName("Header", _fields[i]), _fields[i].Name, headerstyle, _fields[i], null);

        //        headerrow.TablixCells.Add(col);
        //    }

        //    _table.TablixBody.TablixRows.Add(headerrow);

        //    // Create DateRow TablixRow

        //    TablixRow datarow = new TablixRow();
        //    datarow.Height = new ReportSize(0.19);
        //    Style datastyle = new Style();
        //    datastyle.FontSize = new ReportExpression<ReportSize>("9 pt");


        //    for (int i = 0; i < numCols; i++)
        //    {
        //        TablixCell col = new TablixCell();
        //        string expression = string.Format("=Fields!{0}.Value", _fields[i].SQLName);

        //        //col = CreateCell(_fields[i].SQLName, expression, datastyle, _fields[i], null);
        //        col = CreateCell(GetCellName(String.Empty, _fields[i]), expression, datastyle, _fields[i], null);

        //        datarow.TablixCells.Add(col);
        //    }

        //    _table.TablixBody.TablixRows.Add(datarow);


        //    // Empty TablixColumnHierarch and TablixRowHierarchy are
        //    // created by default.

        //    // Need ColumnHierarchy defined for each column

        //    for (int i = 0; i < numCols; i++)
        //    {
        //        TablixMember tcmember = new TablixMember();
        //        _table.TablixColumnHierarchy.TablixMembers.Add(tcmember);
        //    }

        //    // Need RowHierarchies for each row defined above.
        //    TablixMember trmember = new TablixMember();
        //    trmember.KeepTogether = true;
        //    trmember.RepeatOnNewPage = true;
        //    trmember.KeepWithGroup = KeepWithGroupTypes.After;

        //    // Header row Hierarchy
        //    _table.TablixRowHierarchy.TablixMembers.Add(trmember);



        //    trmember = new TablixMember();
        //    trmember.Group = new Group();
        //    trmember.Group.Name = _table.Name + "_Details_Group";


        //    var sortfields = from s in _fields
        //                     where s.SortOrder > -1
        //                     orderby s.SortOrder ascending
        //                     select s;

        //    foreach (RptField item in sortfields)
        //    {
        //        SortExpression se = new SortExpression();

        //        if (item.SQLName.StartsWith("="))
        //        {
        //            se.Value = new ReportExpression(item.SQLName);
        //        }
        //        else
        //        {
        //            se.Value = new ReportExpression(string.Format("=Fields!{0}.Value", item.SQLName));
        //        }

        //        if (item.SortDirection == SortDirectionEnum.Ascending)
        //        {
        //            se.Direction = SortDirections.Ascending;
        //        }
        //        else
        //        {
        //            se.Direction = SortDirections.Descending;
        //        }

        //        trmember.SortExpressions.Add(se);
        //    }

        //    trmember.DataElementName = "Detail_Collection";
        //    trmember.DataElementOutput = DataElementOutputTypes.Output;
        //    trmember.KeepTogether = true;

        //    _table.TablixRowHierarchy.TablixMembers.Add(trmember);


        //    _table.DataSetName = "DataSet1";

        //    _table.KeepTogether = true;

        //    _table.Top = new ReportSize("0.1in");
        //    _table.Left = new ReportSize("0.5in");
        //    _table.Height = new ReportSize("2.9in");
        //    _table.Width = new ReportSize("10.0in");
        //    _table.ZIndex = 1;
        //    Style topstyle = new Style();
        //    topstyle.TopBorder = new Border();
        //    topstyle.TopBorder.Color = new ReportExpression<ReportColor>("DimGray");
        //    topstyle.TopBorder.Style = new ReportExpression<BorderStyles>(BorderStyles.Solid);
        //    topstyle.TopBorder.Width = new ReportExpression<ReportSize>("2pt");
        //    _table.Style = topstyle;

        //}

        //private void ModifyColumns()
        //{
        //    int numFields = _fields.Count;

        //    // We are only going to recreate columns in the single
        //    // 'data' row and 'header' row.

        //    _table.TablixBody.TablixColumns.Clear();
        //    _table.TablixColumnHierarchy.TablixMembers.Clear();

        //    // Find "HeaderRow" and "DataRow" rows

        //    TablixRow headerRow = FindRow("HeaderRow");
        //    int numExistingCols = headerRow.TablixCells.Count;
        //    TablixRow dataRow = FindRow("DataRow");

        //    // Create TablixColumns
        //    // Create a column for each field that is present.

        //    for (int i = 0; i < numFields; i++)
        //    {
        //        TablixColumn newcolumn = new TablixColumn();
        //        double width = 1.0;
        //        if (_fields[i].Width > -1)
        //        {
        //            width = _fields[i].Width;
        //        }
        //        newcolumn.Width = new ReportSize(width);
        //        _table.TablixBody.TablixColumns.Add(newcolumn);
        //    }

        //    for (int i = numFields; i < numExistingCols; i++)
        //    {
        //        TablixColumn newcolumn = new TablixColumn();
        //        newcolumn.Width = new ReportSize(1);
        //        _table.TablixBody.TablixColumns.Add(newcolumn);
        //    }

        //    // Create HeaderRow TablixRow
        //    // TODO: Allow style differences?

        //    TablixRow headerrow = new TablixRow();
        //    headerrow.Height = headerRow.Height;

        //    // assume header style it in cell 0 and all the same.
        //    Style headerstyle1 = headerRow.TablixCells[0].CellContents.ReportItem.Style;
        //    Textbox txtbx = (Textbox)headerRow.TablixCells[0].CellContents.ReportItem;
        //    Style headerstyle2 = txtbx.Paragraphs[0].TextRuns[0].Style;

        //    for (int i = 0; i < numExistingCols; i++)
        //    {
        //        TablixCell col = new TablixCell();

        //        if (i < numFields)
        //        {
        //            //col = CreateCellEx("Header_" + _fields[i].SQLName, _fields[i].Name, headerstyle1, headerstyle2, _fields[i]);
        //            col = CreateCellEx(GetCellName("Header", _fields[i]), _fields[i].Name, headerstyle1, headerstyle2, _fields[i]);
        //        }
        //        else
        //        {
        //            string name = string.Format("Header_{0}", GetNextNumber());
        //            col = CreateCellEx(name, " ", headerstyle1, headerstyle2, null);
        //        }

        //        headerrow.TablixCells.Add(col);
        //    }

        //    int headerIdx = _table.TablixBody.TablixRows.IndexOf(headerRow);
        //    if (headerIdx > -1)
        //    {
        //        _table.TablixBody.TablixRows[headerIdx] = headerrow;
        //    }

        //    // Create DataRow TablixRow for each available field.
        //    // TODO: Style differences?

        //    TablixRow datarow = new TablixRow();
        //    datarow.Height = dataRow.Height;

        //    // assume data style it in cell 0 and all the same (probably bad assumptions).
        //    Style datastyle1 = dataRow.TablixCells[0].CellContents.ReportItem.Style;
        //    txtbx = (Textbox)dataRow.TablixCells[0].CellContents.ReportItem;
        //    Style datastyle2 = txtbx.Paragraphs[0].TextRuns[0].Style;

        //    for (int i = 0; i < numExistingCols; i++)
        //    {
        //        TablixCell col = new TablixCell();
        //        string expression;

        //        if (i < numFields)
        //        {
        //            expression = string.Format("=Fields!{0}.Value", _fields[i].SQLName);
        //            //col = CreateCellEx(_fields[i].SQLName, expression, datastyle1, datastyle2, _fields[i]);
        //            col = CreateCellEx(GetCellName(String.Empty, _fields[i]), expression, datastyle1, datastyle2, _fields[i]);
        //        }
        //        else
        //        {
        //            expression = string.Format("textbox{0}", GetNextNumber());
        //            col = CreateCellEx(expression, " ", datastyle1, datastyle2, null);
        //        }

        //        datarow.TablixCells.Add(col);
        //    }

        //    int dataIdx = _table.TablixBody.TablixRows.IndexOf(dataRow);
        //    if (dataIdx > -1)
        //    {
        //        _table.TablixBody.TablixRows[dataIdx] = datarow;
        //    }

        //    // Empty TablixColumnHierarch and TablixRowHierarchy are
        //    // created by default.

        //    // Need ColumnHierarchy defined for each column

        //    for (int i = 0; i < numExistingCols; i++)
        //    {
        //        TablixMember tcmember = new TablixMember();
        //        _table.TablixColumnHierarchy.TablixMembers.Add(tcmember);
        //    }

        //}

        //private TablixRow FindRow(string rowPrefix)
        //{
        //    TablixRow foundRow = null;

        //    int numRows = _table.TablixBody.TablixRows.Count;

        //    foreach (TablixRow row in _table.TablixBody.TablixRows)
        //    {
        //        foreach (TablixCell cell in row.TablixCells)
        //        {
        //            if (cell.CellContents != null &&
        //                cell.CellContents.ReportItem != null &&
        //                cell.CellContents.ReportItem.Name.StartsWith(rowPrefix))
        //            {
        //                foundRow = row;
        //                break;
        //            }
        //        }

        //        if (foundRow != null)
        //        {
        //            break;
        //        }
        //    }
        //    return foundRow;
        //}

        //#region Create Report Key Tablix
        //public Tablix CreateReportKeyTablix(List<RptParameter> reportParameters, string dataSetName)
        //{
        //    List<string> exceptList = new List<string>{"Fund Year"};

        //    Tablix table = new Tablix();
        //    table.PageBreak = new PageBreak();
        //    table.PageBreak.BreakLocation = BreakLocations.Start;
        //    table.Name = "ReportKey_tablix";
        //    table.DataSetName = dataSetName;

        //    TablixColumn keyNameColumn = new TablixColumn();
        //    keyNameColumn.Width = new ReportSize(3.0);
        //    TablixColumn keyValueColumn = new TablixColumn();
        //    keyValueColumn.Width = new ReportSize(3.0);
        //    table.TablixBody.TablixColumns.Add(keyNameColumn);
        //    table.TablixBody.TablixColumns.Add(keyValueColumn);

        //    TablixRow headerRow = new TablixRow();
        //    headerRow.Height = new ReportSize(0.25);
        //    table.TablixBody.TablixRows.Add(headerRow);

        //    TablixCell keyNameCell = createReportKeyHeaderCell("KeyName_TextBox", "Criteria");
        //    headerRow.TablixCells.Add(keyNameCell);

        //    TablixCell keyValueCell = createReportKeyHeaderCell("KeyValue_TextBox", "Detail");
        //    headerRow.TablixCells.Add(keyValueCell);

        //    TablixMember keyNameMember = new TablixMember();
        //    table.TablixColumnHierarchy.TablixMembers.Add(keyNameMember);
        //    TablixMember keyValueMember = new TablixMember();
        //    table.TablixColumnHierarchy.TablixMembers.Add(keyValueMember);

        //    TablixMember headerMember = new TablixMember();
        //    table.TablixRowHierarchy.TablixMembers.Add(headerMember);

        //    string filterString = "";
        //    foreach (var reportParameter in reportParameters)
        //    {
        //        if (reportParameter.Name.ToLower() == "filterkey")
        //        {
        //            filterString = reportParameter.DefaultValue;
        //        }
        //    }

        //    bool isContainsExcept = false;
        //    if (!string.IsNullOrEmpty(filterString))
        //    {
        //        string filterStringWithoutGroupSorts = "";
        //        int sortIdx = filterString.IndexOf("Sort by");
        //        int groupIdx = filterString.IndexOf("Group by");
        //        if (sortIdx != -1 || groupIdx != -1)
        //        {
        //            if (sortIdx != -1 && groupIdx == -1)
        //            {
        //                filterStringWithoutGroupSorts = filterString.Substring(0, sortIdx);
        //            }
        //            else if (sortIdx == -1 && groupIdx != -1)
        //            {
        //                filterStringWithoutGroupSorts = filterString.Substring(0, groupIdx);
        //            }
        //            else
        //            {
        //                filterStringWithoutGroupSorts = filterString.Substring(0, (sortIdx < groupIdx ? sortIdx : groupIdx));
        //            }
        //        }
        //        else
        //        {
        //            filterStringWithoutGroupSorts = filterString;
        //        }

        //        foreach (string s in exceptList)
        //        {
        //            if (filterStringWithoutGroupSorts.ToLower().Contains(s.ToLower()))
        //            {
        //                isContainsExcept = true;
        //                break;
        //            }
        //        }
        //    }

        //    if (reportParameters.Where(rp => rp.Name.ToLower() == "startdate").ToList().Count != 0 && !isContainsExcept)
        //    {
        //        TablixRow startDateRow = createReportKeyItemRow("Start Date",
        //                                                        "= CDate(First(Fields!StartDate.Value, \"DateSet3\")).ToString(\"MM/dd/yyyy hh:mm tt\") ");
        //        table.TablixBody.TablixRows.Add(startDateRow);

        //        TablixMember startDateRowMember = new TablixMember();
        //        table.TablixRowHierarchy.TablixMembers.Add(startDateRowMember);
        //    }

        //    if (reportParameters.Where(rp => rp.Name.ToLower() == "enddate").ToList().Count != 0 && !isContainsExcept)
        //    {
        //        TablixRow endDateRow = createReportKeyItemRow("End Date",
        //                                                      "= CDate(First(Fields!EndDate.Value, \"DateSet3\")).ToString(\"MM/dd/yyyy hh:mm tt\")");
        //        table.TablixBody.TablixRows.Add(endDateRow);

        //        TablixMember endDateRowMember = new TablixMember();
        //        table.TablixRowHierarchy.TablixMembers.Add(endDateRowMember);
        //    }

        //    if (reportParameters.Where(rp => rp.Name.ToLower() == "asofdate").ToList().Count != 0)
        //    {
        //        TablixRow endDateRow = createReportKeyItemRow("As Of Date",
        //                                                      "= CDate(First(Fields!AsOfDate.Value, \"DataSet2\")).ToString(\"MM/dd/yyyy\")");
        //        table.TablixBody.TablixRows.Add(endDateRow);

        //        TablixMember endDateRowMember = new TablixMember();
        //        table.TablixRowHierarchy.TablixMembers.Add(endDateRowMember);
        //    }

        //    if (!string.IsNullOrEmpty(filterString))
        //    {
        //        string[] filterValues = filterString.Split(new string[]{";"}, StringSplitOptions.RemoveEmptyEntries);
        //        if (filterValues.Length != 0)
        //        {
        //            foreach (var filterValue in filterValues)
        //            {
        //                if (string.IsNullOrEmpty(filterValue)) continue;

        //                string[] prms = filterValue.Split(new char[] {':'});
        //                if (prms.Length >= 2)
        //                {
        //                    string value = "";
        //                    for (var i = 1; i < prms.Length; i++ )
        //                    {
        //                        if (string.IsNullOrEmpty(value))
        //                        {
        //                            value += prms[i];
        //                        }
        //                        else
        //                        {
        //                            value += ":" + prms[i];
        //                        }
        //                    }

        //                    TablixRow row = createReportKeyItemRow(prms[0], value);
        //                    table.TablixBody.TablixRows.Add(row);

        //                    TablixMember rowMember = new TablixMember();
        //                    table.TablixRowHierarchy.TablixMembers.Add(rowMember);
        //                }
        //            }
        //        }
        //    }
        //    return table;
        //}

        //private TablixRow createReportKeyItemRow(string captionColumnName, string valueColumnName)
        //{
        //    TablixRow row = new TablixRow();
        //    row.Height = new ReportSize(0.25);

        //    TablixCell itemNameCell = createReportKeyItemCell(string.Format("textBox_{0}_keyName", GetNextNumber()), captionColumnName.Trim());
        //    row.TablixCells.Add(itemNameCell);
        //    TablixCell itemValueCell = createReportKeyItemCell(string.Format("textBox_{0}_keyValue", GetNextNumber()), valueColumnName.Trim());
        //    row.TablixCells.Add(itemValueCell);
        //    return row;
        //}

        //private TablixCell createReportKeyItemCell(string name, string caption)
        //{
        //    TablixCell headerCell = new TablixCell();
        //    Textbox txtbox = new Textbox();
        //    headerCell.CellContents = new CellContents();
        //    headerCell.CellContents.ReportItem = txtbox;
        //    txtbox.Name = name;
        //    txtbox.CanGrow = true;
        //    txtbox.KeepTogether = true;
        //    txtbox.Paragraphs[0].Style = new Style();
        //    txtbox.Paragraphs[0].Style.TextAlign = new ReportExpression<TextAlignments>("Left");
        //    txtbox.Paragraphs[0].TextRuns[0].Value = new ReportExpression(caption);
        //    txtbox.Paragraphs[0].TextRuns[0].Style = new Style();
        //    txtbox.Paragraphs[0].TextRuns[0].Style.TextAlign = new ReportExpression<TextAlignments>("Left");
        //    txtbox.Paragraphs[0].TextRuns[0].Style.FontFamily = new ReportExpression("Arial");
        //    txtbox.Paragraphs[0].TextRuns[0].Style.FontSize = new ReportExpression<ReportSize>("9pt");
        //    txtbox.Paragraphs[0].TextRuns[0].Style.FontWeight = new ReportExpression<FontWeights>("Bold");
        //    txtbox.Paragraphs[0].TextRuns[0].Style.Color = new ReportExpression<ReportColor>("Black");
        //    txtbox.Paragraphs[0].TextRuns[0].Style.Format = new ReportExpression();
        //    txtbox.Width = new ReportSize(3.0);
        //    txtbox.Style = new Style();
        //    txtbox.Style.BottomBorder = new Border();
        //    txtbox.Style.BottomBorder.Color = new ReportExpression<ReportColor>("DimGray");
        //    txtbox.Style.BottomBorder.Style = new ReportExpression<BorderStyles>("Solid");
        //    txtbox.Style.BottomBorder.Width = new ReportExpression<ReportSize>("1pt");
        //    txtbox.Style.VerticalAlign = new ReportExpression<VerticalAlignments>("Middle");
        //    txtbox.Style.TextAlign = new ReportExpression<TextAlignments>("Left");
        //    txtbox.Style.PaddingBottom = new ReportExpression<ReportSize>("1pt");
        //    txtbox.Style.PaddingRight = new ReportExpression<ReportSize>("1pt");
        //    txtbox.Style.PaddingLeft = new ReportExpression<ReportSize>("1pt");
        //    txtbox.Style.PaddingTop = new ReportExpression<ReportSize>("1pt");
        //    return headerCell;
        //}

        //private TablixCell createReportKeyHeaderCell(string name, string caption)
        //{
        //    TablixCell headerCell = new TablixCell();
        //    Textbox txtbox = new Textbox();
        //    headerCell.CellContents = new CellContents();
        //    headerCell.CellContents.ReportItem = txtbox;
        //    txtbox.Name = name;
        //    txtbox.CanGrow = true;
        //    txtbox.KeepTogether = true;
        //    txtbox.Paragraphs[0].TextRuns[0].Value = new ReportExpression(caption);
        //    txtbox.Paragraphs[0].TextRuns[0].Style = new Style();
        //    txtbox.Paragraphs[0].TextRuns[0].Style.FontWeight = new ReportExpression<FontWeights>("Bold");
        //    txtbox.Paragraphs[0].TextRuns[0].Style.Color = new ReportExpression<ReportColor>("#333b8c");
        //    txtbox.Style = new Style();
        //    txtbox.Style.TopBorder = new Border();
        //    txtbox.Style.TopBorder.Color = new ReportExpression<ReportColor>("DimGray");
        //    txtbox.Style.TopBorder.Style = new ReportExpression<BorderStyles>("Solid");
        //    txtbox.Style.TopBorder.Width = new ReportExpression<ReportSize>("2pt");
        //    txtbox.Style.BottomBorder = new Border();
        //    txtbox.Style.BottomBorder.Color = new ReportExpression<ReportColor>("DimGray");
        //    txtbox.Style.BottomBorder.Style = new ReportExpression<BorderStyles>("Solid");
        //    txtbox.Style.BottomBorder.Width = new ReportExpression<ReportSize>("1pt");
        //    txtbox.Style.BackgroundColor = new ReportExpression<ReportColor>("WhiteSmoke");
        //    txtbox.Style.VerticalAlign = new ReportExpression<VerticalAlignments>("Middle");
        //    txtbox.Style.PaddingBottom = new ReportExpression<ReportSize>("1pt");
        //    txtbox.Style.PaddingRight = new ReportExpression<ReportSize>("1pt");
        //    txtbox.Style.PaddingLeft = new ReportExpression<ReportSize>("1pt");
        //    txtbox.Style.PaddingTop = new ReportExpression<ReportSize>("1pt");
        //    return headerCell;
        //}

        //#endregion

        #endregion
    }
}
