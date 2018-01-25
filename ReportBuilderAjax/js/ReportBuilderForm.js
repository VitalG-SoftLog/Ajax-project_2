function ReportBuilderForm(id, reportBuilderHeader) {
    this.Element = document.getElementById(id);
    this.Data = new Array();   
    this.ParentObj = null;    
    this.Interval = 50000;
    this.Timer = null;
    this.IsBusy = false;
    this.IsHasAccess = true;
    this.Controls = new Array();
    this.TestDrop = new Array();
    this._filter = null;
    this.ReportBuilderTabs = null;
    this.ReportLayoutStyles = null;
    this.IsCustomId = "";
    this.IsCustom = false;
    this.ReportId = 1014;
    this.UserReportId = 23898;
    this.ReportBuilderHeader = reportBuilderHeader;
};

ReportBuilderForm.prototype.Load = function (reportId, userReportId) {
	this.ReportId = reportId;
	this.UserReportId = userReportId;
    var params = {
    action: Consts.HANDLER_ACTIONS.GET_ALL_DATA,
        reportID: reportId,
        userReportID: userReportId
    };
    
    var parentObj = this;
    $.blockUI({ message: "Please wait loading...", css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });
    this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
    parentObj.successfullGetData.call(parentObj, data);
    }, function (e) { parentObj.sendErrorCallbackForGetData.call(parentObj, e); });
};


ReportBuilderForm.prototype.successfullGetData = function (data) {
    this.Data = data;
    this.Bind();
    $.unblockUI();
};

ReportBuilderForm.prototype.Bind = function () {
    var obj = this;
    var asd = {
        'ReportNameSpan': this.Data.ReportName.toString().toUpperCase() + ' - ' + (this.Data.IsNew ? "NEW REPORT" : this.Data.UserReportName),
        'reportBuilderTabMainPlaceHolderId': this.getClientID('reportBuilderTabMainPlaceHolderId')
    };

    $('#backButton').click(function () {
        obj.ReportBuilderHeader.ShowReportSelectForm();
    });

    asd.lossRunSummaryFilterContainerId = this.getClientID('lossRunSummaryFilterContainerId');
    var mainDivLeft = TemplateEngine.Format(TemplateManager.templates['ReportBuilderMain_MainLeft'], asd);
    $(this.Element).append(mainDivLeft);

    this.BindFilter();

    // start binding report builder tabs 

    var mainDivRight = document.getElementById('reportBuilderTabs');
    this.ReportBuilderTabs = new ReportTabManager(mainDivRight, this, true, this.Data);

    // end --------------------


    //Start Report Layout Style dropdown menu
    var ReportLayoutStyleDropDownAttr = {
        "idDropDown": this.getClientID("ReportLayoutStyle"),
        "dropDownHide": "dropDownHide",
        "attribute": "ReportLayoutStyle"
    };
    var ReportLayoutStyleDropDownTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_DropDownControl'], ReportLayoutStyleDropDownAttr);
    $(document.getElementById("ReportLayoutStyleDropDown")).append(ReportLayoutStyleDropDownTempl);


    this.Controls.ReportLayoutStyleControls = new newDropDownControl(this, ReportLayoutStyleDropDownAttr["idDropDown"], -1, -1, ValueType.Int, null, false, 227);
    this.Controls.ReportLayoutStyleControls.ClearOptions();

    for (var index in this.Data.ReportLayoutStyles) {
        if (this.Data.ReportLayoutStyles[index].IsCustom) {
            this.IsCustomId = this.Data.ReportLayoutStyles[index].ID;
        }
        this.Controls.ReportLayoutStyleControls.AppendOption(this.Data.ReportLayoutStyles[index].ID, this.Data.ReportLayoutStyles[index].Name);
    }

    var parentObj = this;
    this.Controls.ReportLayoutStyleControls.PropertyChanged = function () {
        if (this.GetOptionValue(this.Value) * 1 != parentObj.IsCustomId) {
            $('#ReportLayoutTab').css('display', 'none');
            $('#ReportLayoutTabContainer').css('display', 'none');
            $($('#ReportLayoutTab')[0].parentNode).css('width', '0px');
            parentObj.ReportBuilderTabs.inactive_tabs();
            parentObj.ReportBuilderTabs.active_tab(2);
            parentObj.ReportBuilderTabs.CurrentTab = 2;
            parentObj.ReportBuilderTabs.BindTab(2);
            parentObj.IsCustom = false;
        } else {
            $('#ReportLayoutTab').css('display', '');
            $('#ReportLayoutTabContainer').css('display', 'none');
            $($('#ReportLayoutTab')[0].parentNode).css('width', '135px');
            parentObj.ReportBuilderTabs.inactive_tabs();
            parentObj.ReportBuilderTabs.active_tab(1);
            parentObj.ReportBuilderTabs.CurrentTab = 1;
            parentObj.ReportBuilderTabs.BindTab(1);
            parentObj.IsCustom = true;
        }
        parentObj._filter.reportLayoutStylesChanged(parentObj.IsCustom);
    };
    this.Controls.ReportLayoutStyleControls.SetDropDownValue(this.Controls.ReportLayoutStyleControls.Options[0].getAttribute('value'));

    $(document.getElementById("SlideLeftBttn")).bind("click", function () {
        obj.HideLeft();
    });

    $(document.getElementById("SlideLeftBttn2")).bind("click", function () {
        obj.ShowLeft();
    });


    //////////////////////////////////
    //////////// CONTROLS ////////////
    //////////////////////////////////

    //start  Bottom Buttons
    var bottomButtonsAttr = {
        'reportBuilderTabBottomPlaceHolderId': this.getClientID("reportBuilderTabBottomPlaceHolderId"),
        "idBottomButtons": this.getClientID("bottomButtons"),
        "SaveReportBttn": this.getClientID("saveReportBttn"),
        "SaveReportAsNewBttn": this.getClientID("saveReportAsNewBttn"),
        "CancelBttn": this.getClientID("cancelBttn"),
        "RunReportBttn": this.getClientID("runReportBttn"),
        "SaveBttn": this.getClientID("saveBttn"),
        "SaveAsNewBttn": this.getClientID("saveAsNewBttn"),
        "SaveInput": this.getClientID("saveInput"),
        "SaveAsNewInput": this.getClientID("saveAsNewInput"),
        "SaveCloud": this.getClientID("saveCloud"),
        "SaveAsNewCloud": this.getClientID("saveAsNewCloud")
    };

    var bottomButtonsTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_BottomButtonsTemplate'], bottomButtonsAttr);
    $(this.Element).append(bottomButtonsTempl);

    this.Controls.PdfXlsContl = new MultiRadioButton("PdfXlsButton", TemplateManager.templates['ReportBuilderForm_MultiRadioHorizontalReportFormatType']);
    this.Controls.PdfXlsContl.Bind();
    this.Controls.PdfXlsContl.SetValue(Consts.FORMAT_TYPE.PDF);

    /*this.Controls.PdfXlsContl.EventSet.PropertyChanged.List.SetFields = function () {
        var excelValue = (this.GetValue() * 1 == Consts.FORMAT_TYPE.Excel);
        var value = obj._filter.Controls.PageBreakControl.GetBoolValue();

        if (value && excelValue) {
            obj.ValidateTurnOffPageBreak();
        }
    };*/

    if (this._filter.Controls.PageBreakControl) {
        this._filter.Controls.PageBreakControl.EventSet.PropertyChanged.List.SetFields = function() {
            var value = this.GetBoolValue();

            var excelValue = (obj.Controls.PdfXlsContl.GetValue() * 1 == Consts.FORMAT_TYPE.Excel);

            if (value && excelValue) {
                obj.ValidateTurnOffPageBreak();
            }

        };
    }

    var saveReportBttnAttr = {
        "idSaveReportBttn": this.getClientID("SaveReportBttn")
    };

    var saveReportBttnTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_SaveReportBttnTemplate'], saveReportBttnAttr);
    $('#' + this.getClientID("saveReportBttn")).append(saveReportBttnTempl);

    var saveReportAsNewBttnAttr = {
        "idSaveReportAsNewBttn": this.getClientID("SaveReportAsNewBttn")
    };

    var saveReportAsNewBttnTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_SaveReportAsNewBttnTemplate'], saveReportAsNewBttnAttr);
    $('#' + this.getClientID("saveReportAsNewBttn")).append(saveReportAsNewBttnTempl);

    var cancelBttnAttr = {
        "idCancelBttn": this.getClientID("CancelBttn")
    };

    var cancelBttnTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_CancelBttnTemplate'], cancelBttnAttr);
    $('#' + this.getClientID("cancelBttn")).append(cancelBttnTempl);

    var runReportBttnAttr = {
        "idRunReportBttn": this.getClientID("RunReportBttn")
    };

    var runReportBttnTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_RunReportBttnTemplate'], runReportBttnAttr);
    $('#' + this.getClientID("runReportBttn")).append(runReportBttnTempl);

    var saveBttnAttr = {
        "idSaveBttn": this.getClientID("SaveBttn")
    };

    var saveBttnTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_SaveBttnTemplate'], saveBttnAttr);
    $('#' + this.getClientID("saveBttn")).append(saveBttnTempl);

    var saveAsNewBttnAttr = {
        "idSaveBttn": this.getClientID("SaveAsNewBttn")
    };

    var saveAsNewBttnTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_SaveBttnTemplate'], saveAsNewBttnAttr);
    $('#' + this.getClientID("saveAsNewBttn")).append(saveAsNewBttnTempl);

    var parentobject = this;
    $('#' + this.getClientID("saveReportBttn")).bind("click", function () {
        parentobject.unhide();
    });

    var parentobjectSaveAsNew = this;
    $('#' + this.getClientID("saveReportAsNewBttn")).bind("click", function () {
        parentobjectSaveAsNew.unhide2();
    });

    var parentobjectSavebttn = this;
    $('#' + this.getClientID("saveBttn")).bind("click", function () {
        parentobjectSavebttn.hide();
        parentobjectSavebttn.save(false);
    });

    var parentobjectSaveAsNewbttn = this;
    $('#' + this.getClientID("saveAsNewBttn")).bind("click", function () {
        parentobjectSaveAsNewbttn.hide2();
        parentobjectSavebttn.save(true);
    });

    var saveInputAttr = {
        "idSaveInput": this.getClientID("SaveInput")
    };

    var saveInputTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_SaveInputTemplate'], saveInputAttr);
    $('#' + this.getClientID("saveInput")).append(saveInputTempl);

    var saveInputObj = this;
    $('#' + this.getClientID("saveInput")).bind("click", function () {
        saveInputObj.hideText();
    });
    $('#' + this.getClientID("SaveInput")).bind("blur", function () {
        saveInputObj.BlurText();
    });

    var saveAsNewInputAttr = {
        "idSaveAsNewInput": this.getClientID("SaveAsNewInput")
    };

    var saveAsNewInputTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_SaveAsNewInputTemplate'], saveAsNewInputAttr);
    $('#' + this.getClientID("saveAsNewInput")).append(saveAsNewInputTempl);

    var saveAsNewInputObj = this;
    $('#' + this.getClientID("saveAsNewInput")).bind("click", function () {
        saveAsNewInputObj.hideText2();
    });
    $('#' + this.getClientID("SaveAsNewInput")).bind("blur", function () {
        saveAsNewInputObj.BlurText2();
    });

    $('#' + this.getClientID("RunReportBttn")).bind("click", function () {
        saveInputObj.RunReport();
    });

    this.Controls.SaveInputContl = new TextBoxControl(this, saveInputAttr["idSaveInput"]);
    this.Controls.SaveInputContl.SetValue(this.Data.IsNew ? 'Report name here' : this.Data.UserReportName);

    this.Controls.SaveAsNewInputContl = new TextBoxControl(this, saveAsNewInputAttr["idSaveAsNewInput"]);
    this.Controls.SaveAsNewInputContl.SetValue(this.Data.IsNew ? 'Report name here' : this.Data.UserReportName);

    this.Controls.SaveInputContl.DataBind();
    this.Controls.SaveAsNewInputContl.DataBind();
    //end  Bottom Buttons
    if (this.Data.ReportLayoutStyleID * 1 != 0) {
        this.Controls.ReportLayoutStyleControls.SetDropDownValue(this.Data.ReportLayoutStyleID);
    }

    this.Controls.PdfXlsContl.SetValue(this.Data.Format + "");
};

ReportBuilderForm.prototype.ValidateTurnOffPageBreak = function () {
    var paramList = this.GetParamList(false);

    var postParam = new Array();

    for (var idx in paramList) {
        postParam[postParam.length] = {
            Name:  paramList[idx].Name,
            Value: paramList[idx].DefaultValue            
        };
    }

    var params = {
        action: Consts.HANDLER_ACTIONS.VALIDATE_PAGE_BREAK,
        reportId: this.ReportId,
        postParams: postParam,
        summaryOnly: this.Data.SummaryOnly, 
        customReport: this.IsCustom
    };

    var parentObj = this;
    $.blockUI({ message: "Please wait loading...", css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });
    this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
        parentObj.successfullValidatePageBreak.call(parentObj, data);
    }, function (e) { parentObj.sendErrorCallbackForGetData.call(parentObj, e); });

};

ReportBuilderForm.prototype.successfullValidatePageBreak = function (data) {
    $.unblockUI();
    
    if (data.alert != '') {
        alert(data.alert);
    }        
};

ReportBuilderForm.prototype.GetParamList = function (asNew) {
    var filterData = this._filter.GetFilterData();
    for (var key in this.Data.UserReportParameters) {
        if (this.Data.UserReportParameters[key]) {
            var item = this.Data.UserReportParameters[key];
            item.DefaultValue = this.GetFilterValueByName(item.Name, filterData, asNew);
            item.FilterString = this.GetFilterStringByName(item.Name, filterData, asNew);
        }
    }
    return this.Data.UserReportParameters;
};


ReportBuilderForm.prototype.GetFilterStringByName = function(filterName, filtersList) {
    var filter = "";
    for(var key in filtersList) {
        if (filtersList[key] && filtersList[key].FilterName.toString().toLowerCase() == filterName.toString().toLowerCase()) {
            return null;//filtersList[key].Value;
        }
    }
    return /*filter != null ? filter.FilterString :*/ null;
};

ReportBuilderForm.prototype.GetGroupingFieldsListToString = function () {
    var listField = "";

    this.Data.ReportFields.sort(function (a, b) {
        var nameA = a.GroupOrder * 1, nameB = b.GroupOrder * 1;
        if (nameA < nameB)
            return -1;
        if (nameA > nameB)
            return 1;
        return 0;
    });

    for (var key in this.Data.ReportFields) {
        if (this.Data.ReportFields[key]) {
            var item = this.Data.ReportFields[key];
            if (item.GroupOrder > 0 && item.IsGroupable) {
                if (listField.Length > 0) {
                    listField += "," + item.SQLName;
                } else {
                    listField += item.SQLName;
                }
            }
        }
    }
    return listField;
};

ReportBuilderForm.prototype.GetFilterValueByName = function (filterName, filtersList, asNew) {
    var value = "";

    switch (filterName.toString().toLowerCase()) {
        case "userid":
            value = this.Data.UserID;
            break;
        case "assnnum":
            value = this.Data.AssociationNumber;
            break;
        case "assnname":
            value = this.Data.AssociationName;
            break;
        case "reportname":
            value = this.Data.ReportName;
            break;
        case "userreportname":
            value = asNew ? this.Controls.SaveAsNewInputContl.GetValue() : this.Controls.SaveInputContl.GetValue();
            break;
        case "ispaid":
            value = (this.Controls.ReportLayoutStyleControls.Value.toString().Contains("Paid") ? true : false).toString();
            break;
        case "iscustom":
            if (this.IsCustom) {
                value = "True";
            } else {
                value = "False";
            }
            break;
        case "ismembergrouping":
            value = (this.Controls.ReportLayoutStyleControls.Value.toString().Contains("Member") ? true : false).toString();
            break;
        case "hasssnaccess":
            value = this.Data.HasSSNAccess.toString().toLowerCase();
            break;
        case "customgrouping":
            value = this.GetGroupingFieldsListToString();
            break;
        case "formattypeid":
            value = this.Controls.PdfXlsContl.GetValue();
            break;
        default:
            for (var key in filtersList) {
                if (filtersList[key] && filtersList[key].FilterName.toString().toLowerCase() == filterName.toString().toLowerCase()) {
                    value = filtersList[key].Value;
                }
            }
            break;
    }

    return value;
};

ReportBuilderForm.prototype.ValidateReportName = function (saveAsNew) {
    var errMsg = "";
    var isValid = true;
    var userReportName = saveAsNew ? this.Controls.SaveAsNewInputContl.GetValue() : this.Controls.SaveInputContl.GetValue();
    if (userReportName == null || userReportName == "") {
        errMsg = "The Report Name can't be empty";
        isValid = false;
    }

    if (isValid) {
        for (var key in this.Data.UserReportNamesList) {
            if (this.Data.UserReportNamesList[key]) {
                var item = this.Data.UserReportNamesList[key];
                if (item.toString().trim().toUpperCase() == userReportName.toString().trim().toUpperCase() && (this.Data.IsNew || saveAsNew)) {
                    errMsg = "The Report Name cannot be a duplicate of an existing report name";
                    isValid = false;
                }
            }
        }
    }

    if (!isValid) {
        alert(errMsg);
    }

    return isValid;
};

ReportBuilderForm.prototype.ValidateColumnList = function () {
    var isValid = false;
    for (var key in this.Data.ReportFields) {
        if (this.Data.ReportFields[key]) {
            var item = this.Data.ReportFields[key];
            if (item.ColumnOrder > 0) {
                isValid = true;
                break;
            }
        }
    }
    if (!isValid) {
        alert("The list of columns used in report can't be empty");
    }
    return isValid;
};

ReportBuilderForm.prototype.ValidateGroupingList = function () {
    if (this.ReportBuilderTabs.IsSummaryOnly() == "true") {
        var isValid = false;
        for (var key in this.Data.ReportFields) {
            if (this.Data.ReportFields[key]) {
                var item = this.Data.ReportFields[key];
                if (item.GroupOrder > 0) {
                    isValid = true;
                    break;
                }
            }
        }
        if (!isValid) {
            alert("Summary only reports must have at least one Group By Field");
        }
        return isValid;
    } else {
        return true;
    }
};

ReportBuilderForm.prototype.save = function (saveAsNew) {
    if (!this._filter.IsValid()) return;
    if (!this.ValidateReportName(saveAsNew)) return;
    if (this.IsCustom) {
        if (!this.ValidateColumnList()) return;
        if (!this.ValidateGroupingList()) return;
        this.saveCustomReport(saveAsNew);
    } else {
        this.saveStandart(saveAsNew);
    }
};

ReportBuilderForm.prototype.saveStandart = function (asNew) {
    var params = {
        action: Consts.HANDLER_ACTIONS.SAVE_STANDART_REPORT,
        reportID: this.ReportId,
        isTurnOffPageBreak: this._filter.IsTurnOfPageBreak(),
        includeTitlePage: this._filter.IncludeTitlePage(),
        formatTypeId: this.Controls.PdfXlsContl.GetValue(),
        reportLayoutStyleID: this.Controls.ReportLayoutStyleControls.GetOptionValue(this.Controls.ReportLayoutStyleControls.Value) * 1,
        parameters: this.GetParamList(asNew || this.Data.IsNew),
        saveAsNew: asNew || this.Data.IsNew,
        userReportName: asNew ? this.Controls.SaveAsNewInputContl.GetValue() : this.Controls.SaveInputContl.GetValue(),
        userReportId: this.UserReportId
    };

    var parentObj = this;
    $.blockUI({ message: "Please wait loading...", css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });
    this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
        parentObj.successfullSaveReport.call(parentObj, data);
    }, function (e) { parentObj.sendErrorCallbackForGetData.call(parentObj, e); });
};

ReportBuilderForm.prototype.GetUserReportFieldList = function (saveAsNew) {
    var resultList = new Array();
    var items = this.Data.ReportFields;
    for (var key in items) {
        if (items[key]) {
            var item = items[key];
            if (item.IsUsed || item.SortOrder > 0 || item.IsGroupByDefault) {
                var len = resultList.length;
                resultList[len] = item;
                resultList[len].ID = saveAsNew ? item.ReportFieldID : item.ID;
            }
        }
    }
    return resultList;
};

ReportBuilderForm.prototype.GetSummarizeFieldList = function () {
    var resultList = new Array();
    var list = this.ReportBuilderTabs.GetCheckedSummarizeFieldIds();
    var items = this.Data.ReportFields;
    for (var key in items) {
        if (items[key]) {
            var item = items[key];
            if (list[item.ReportFieldID] != null) {
                resultList[resultList.length] = item;
            }
        }
    }
    return resultList;
};

ReportBuilderForm.prototype.saveCustomReport = function (asNew) {
    var params = {
        action: Consts.HANDLER_ACTIONS.SAVE_CUSTOM_REPORT,
        reportID: this.ReportId,
        isTurnOffPageBreak: this._filter.IsTurnOfPageBreak(),
        includeTitlePage: this._filter.IncludeTitlePage(),
        formatTypeId: this.Controls.PdfXlsContl.GetValue(),
        reportLayoutStyleID: this.Controls.ReportLayoutStyleControls.GetOptionValue(this.Controls.ReportLayoutStyleControls.Value) * 1,
        parameters: this.GetParamList(asNew || this.Data.IsNew),
        saveAsNew: asNew || this.Data.IsNew,
        userReportName: asNew ? this.Controls.SaveAsNewInputContl.GetValue() : this.Controls.SaveInputContl.GetValue(),
        userReportId: this.UserReportId,
        isSummaryOnly: this.ReportBuilderTabs.IsSummaryOnly(),
        summarizeFields: this.GetSummarizeFieldList(),
        fields: this.GetUserReportFieldList(asNew)
    };
    
    var parentObj = this;
    $.blockUI({ message: "Please wait loading...", css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });
    this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
        parentObj.successfullSaveReport.call(parentObj, data);
    }, function (e) { parentObj.sendErrorCallbackForGetData.call(parentObj, e); });
};

ReportBuilderForm.prototype.successfullSaveReport = function (data) {
    $.unblockUI();
    onTemplatesReady();
};

ReportBuilderForm.prototype.GetReportParamsForViewReport = function (format) {
    var params = {};
    if (!this._filter.IsValid()) return null;
    if (this.IsCustom) {
        if (!this.ValidateColumnList()) return null;
        if (!this.ValidateGroupingList()) return null;
        params = {
            action: Consts.HANDLER_ACTIONS.RUN_CUSTOM_REPORT,
            reportID: this.ReportId,
            isTurnOffPageBreak: this._filter.IsTurnOfPageBreak(),
            includeTitlePage: this._filter.IncludeTitlePage(),
            formatTypeId: format,
            reportLayoutStyleID: this.Controls.ReportLayoutStyleControls.GetOptionValue(this.Controls.ReportLayoutStyleControls.Value) * 1,
            parameters: this.GetParamList(false),
            isSummaryOnly: this.ReportBuilderTabs.IsSummaryOnly(),
            summarizeFields: this.GetSummarizeFieldList(),
            fields: this.GetUserReportFieldList(false)
        };
    }
    else {
        params = {
            action: Consts.HANDLER_ACTIONS.RUN_STANDART_REPORT,
            reportID: this.ReportId,
            isTurnOffPageBreak: this._filter.IsTurnOfPageBreak(),
            includeTitlePage: this._filter.IncludeTitlePage(),
            formatTypeId: format,
            reportLayoutStyleID: this.Controls.ReportLayoutStyleControls.GetOptionValue(this.Controls.ReportLayoutStyleControls.Value) * 1,
            parameters: this.GetParamList(false)
        };
    }
    return params;
};

ReportBuilderForm.prototype.RunReport = function () {
    var parentObj = this;
    var params = this.GetReportParamsForViewReport(this.Controls.PdfXlsContl.GetValue());

    if (params != null) {
        $.blockUI({ message: "Please wait loading...", css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%' } });
        this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function(data) {
            parentObj.successfullRunReport.call(parentObj, data);
        }, function(e) { parentObj.sendErrorCallbackForGetData.call(parentObj, e); });
    }
};

ReportBuilderForm.prototype.successfullRunReport = function (data) {
    if (data && data.Result.Status == 0 && data.Result.ReportUrl != null && data.Result.ReportUrl.toString() != "") {
        var win = window.open("GetFile.aspx?fileName=" + data.Result.ReportUrl, 'Report', 'width=700,height=820,resizable=yes,toolbar=no,menubar=no,location=no,scrollbars=yes,status=yes');
    }
    $.unblockUI();
};

ReportBuilderForm.prototype.BindFilter = function () {
    var attributes = {
        'filterContainerId': this.getClientID("filterContainer")
    };
    var template = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_RootTemplate'], attributes);
    var filterContainer = document.getElementById(this.getClientID('lossRunSummaryFilterContainerId'));
    if (filterContainer)
        $(filterContainer).append(template);

    this._filter = new window[this.Data.FilterSystemType](attributes['filterContainerId'], this);
    this._filter.Data = this.Data.FilterData || [];
    this._filter.HierarchyLabels = this.Data.HierarchyLabels || [];
    this._filter.Bind();
};

ReportBuilderForm.prototype.sendAjax = function (url, params, callbackSuccess, callbackError) {
    $.ajax({
        type: "POST",
        dataType: 'json',
        url: url,
        data: params,
        success: callbackSuccess,
        error: callbackError
    });
};

ReportBuilderForm.prototype.sendErrorCallbackForGetData = function (e) {
    $.unblockUI();
    this.sendErrorCallback(e);
    if (e.responseText.indexOf("timeout") > -1) {
        this.goToMain();
    }
};

ReportBuilderForm.prototype.sendErrorCallback = function (e) {
    this.IsBusy = false;
    
    if (e.responseText.indexOf('Authentication failed') > -1) {
        alert('Authentication failed. Please try to login again.');
    }
    else if (e.status == 0 || e.status == 12029) {
        alert('Your connection has been lost. Please reconnect to the Internet before continuing with this form.');
    }
    else if (e.responseText.indexOf("timeout") > -1) {
        alert("Database timeout. Please try again later");
    }
    else {
        alert('Sorry, internal server error.');
    }
};


ReportBuilderForm.prototype.goToMain = function () {
    
};

ReportBuilderForm.prototype.getClientID = function (name) {
    return this.Element.id + '_' + name;
};

ReportBuilderForm.prototype.unhide = function () {
    $('#' + this.getClientID("saveCloud")).removeClass("SaveCloudDisabled");
    $('#' + this.getClientID("saveCloud")).addClass("SaveCloud");
};

ReportBuilderForm.prototype.hide = function () {
    $('#' + this.getClientID("saveCloud")).removeClass("SaveCloud");
    $('#' + this.getClientID("saveCloud")).addClass("SaveCloudDisabled");
};

ReportBuilderForm.prototype.unhide2 = function () {
    $('#' + this.getClientID("saveAsNewCloud")).removeClass("SaveAsNewCloudDisabled");
    $('#' + this.getClientID("saveAsNewCloud")).addClass("SaveAsNewCloud");
};

ReportBuilderForm.prototype.hide2 = function () {
    $('#' + this.getClientID("saveAsNewCloud")).removeClass("SaveAsNewCloud");
    $('#' + this.getClientID("saveAsNewCloud")).addClass("SaveAsNewCloudDisabled");
};

ReportBuilderForm.prototype.hideText = function () {
    if ($('#' + this.getClientID("SaveInput")).val() == 'Report name here') {
        this.Controls.SaveInputContl.SetValue('');
    }
};

ReportBuilderForm.prototype.BlurText = function () {
    if ($('#' + this.getClientID("SaveInput")).val() == '') {
        this.Controls.SaveInputContl.SetValue('Report name here');
    }
};

ReportBuilderForm.prototype.hideText2 = function () {
    if ($('#' + this.getClientID("SaveAsNewInput")).val() == 'Report name here') {
        this.Controls.SaveAsNewInputContl.SetValue('');
    }
};

ReportBuilderForm.prototype.BlurText2 = function () {
    if ($('#' + this.getClientID("SaveAsNewInput")).val() == '') {
        this.Controls.SaveAsNewInputContl.SetValue('Report name here');
    }
};

ReportBuilderForm.prototype.HideLeft = function () {
    var heightEl = ($(document.getElementById("MainLeft")).height() * 1 - 11) + "px";
    var positionTop = ($(document.getElementById("SlideLeftBttn")).position().top * 1 - 237) + "px";
    var obj = this;
    document.getElementById("MainLeft").style.overflow = "hidden";
    $(document.getElementById("MainRightShadow2")).animate({ marginLeft: "-=336px", width: "+=337px" }, "slow");
    $(document.getElementById("SlideLeftBttn")).animate({ "left": "-=336px" }, "slow");
    $(document.getElementById("MainLeftShadowSlide")).animate({ "left": "-=336px" }, "slow");
    $(document.getElementById("reportBuilderTabs")).animate({ marginLeft: "-=336px" }, "slow");
    $(document.getElementById("MainLeftTable")).animate({ "left": "-=336px" }, "slow", function () {
        $(document.getElementById(obj.getClientID("filterContainer"))).hide();
        $(document.getElementById("MainLeft2")).show();
        document.getElementById("MainLeftCenter").style.height = heightEl;
        document.getElementById("BetweenTabs").style.width = (36 + 'px');
        document.getElementById("reportBuilderTabs").style.marginLeft = "0px";
        document.getElementById("MainRightShadow2").style.marginLeft = "0px";
        document.getElementById("SlideLeftBttn2").style.top = positionTop;
        $(document.getElementById("SlideLeftBttn")).hide();
        $(document.getElementById("SlideLeftBttn2")).show();
        $(document.getElementById("viewReportGridContainer")).css("width", "100%");
    });
};

ReportBuilderForm.prototype.ShowLeft = function () {
    var obj = this;
    document.getElementById("reportBuilderTabs").style.marginLeft = "-336px";
    document.getElementById("MainRightShadow2").style.marginLeft = "-336px";
    document.getElementById("MainLeftTable").style.overflow = "hidden";
    $(document.getElementById("reportBuilderTabs")).animate({ marginLeft: "+=336px" }, "slow");
    $(document.getElementById("MainRightShadow2")).animate({ marginLeft: "+=336px", width: "-=337px" }, "slow");
    $(document.getElementById(obj.getClientID("filterContainer"))).show();
    $(document.getElementById("SlideLeftBttn")).show();
    document.getElementById("SlideLeftBttn").style.backgroundImage = "url(./images/ScrollRightIcon.png)";
    document.getElementById("BetweenTabs").style.width = (6 + 'px');
    $(document.getElementById("MainLeft2")).hide();
    $(document.getElementById("SlideLeftBttn2")).hide();
    $(document.getElementById("SlideLeftBttn")).animate({ "left": "+=336px" }, "slow");
    $(document.getElementById("MainLeftShadowSlide")).animate({ "left": "+=336px" }, "slow");
    $(document.getElementById("MainLeftTable")).animate({ "left": "+=336px" }, "slow", function () {
        document.getElementById("reportBuilderTabs").style.marginLeft = "0px";
        document.getElementById("MainRightShadow2").style.marginLeft = "0px";
        document.getElementById("SlideLeftBttn").style.backgroundImage = "url(./images/ScrollLeftIcon.png)";
        document.getElementById("MainLeft").style.overflow = "";
    });

    $(document.getElementById("viewReportGridContainer")).css("width", "575px").css("overflow-x","scroll");
};

