function ReportSelectForm(id, reportBuilderHeader) {
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
    this.ReportBuilderHeader = reportBuilderHeader;    
};

ReportSelectForm.prototype.Load = function () {
    this.Element.innerHTML = '';
    var params = {
        action: Consts.HANDLER_ACTIONS.GET_ALL_DATA_FOR_GRID
    };

    var parentObj = this;
    $.blockUI({ message: "Please wait loading...", css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });
    this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
        parentObj.successfullGetData.call(parentObj, data);
    }, function (e) { parentObj.sendErrorCallbackForGetData.call(parentObj, e); });
};


ReportSelectForm.prototype.successfullGetData = function (data) {
    this.Data = data;
//    this.Bind();
    this.setSortReport('CreatedDate', 'asc', data.ListReports);
    this.Bind();
    this.renderHeader('CreatedDate', 'asc');
    $.unblockUI();
};

ReportSelectForm.prototype.Bind = function () {
    var obj = this;
    var rowsPlaceHolderTemplate = '';

    for (var idx in this.Data.ListReports) {
        var report = this.Data.ListReports[idx];
        var userReportID = this.Data.ListReports[idx].UserReportID;

        var iconType = "pdfIcon";
        switch (report.FormatType) {
            case Consts.FORMAT_TYPE.PDF:
                iconType = "pdfIcon";
                break;
            case Consts.FORMAT_TYPE.Excel:
                iconType = "excelIcon";
                break;
            case Consts.FORMAT_TYPE.XML:
                iconType = "xmlIcon";
                break;
            case Consts.FORMAT_TYPE.CSV:
                iconType = "csvIcon";
                break;
            case Consts.FORMAT_TYPE.Image:
                iconType = "imageIcon";
                break;
            case Consts.FORMAT_TYPE.GRID:
                iconType = "gridIcon";
                break;
        }

        var itemAttr = {
            showCustomIcon: this.Data.ListReports[idx].IsCustom ? '' : 'display:none;',
            iconType: iconType,
            spawnType: this.Data.ListReports[idx].IsExistsSchedules ? 'Normal' : 'Grey',
            spawnReportButton: this.getClientID("spawnReportButton") + userReportID,
            runReportButton: this.getClientID("runReportButton") + userReportID,
            editReportButton: this.getClientID("editReportButton") + userReportID,
            deleteReportButton: this.getClientID("deleteReportButton") + userReportID,
            addScheduleReportButton: this.getClientID("addScheduleReportButton") + userReportID,
            reportName: report["UserReportName"],
            reportType: report["Name"],
            dateCreated: $.datepicker.formatDate('mm/dd/yy', new Date(report["CreatedDate"])),
            dateModified: $.datepicker.formatDate('mm/dd/yy', new Date(report["ModifiedDate"])),
            reportId: this.Data.ListReports[idx].ReportID,
            userReportID: userReportID
        };
        rowsPlaceHolderTemplate += TemplateEngine.Format(TemplateManager.templates['ReportSelectForm_ItemTemplate'], itemAttr);

        var rootScheduleAttr = {
            rootScheduleRowsPlaceHolder: this.getClientID("rootScheduleRowsPlaceHolder") + userReportID,
            scheduleRowsPlaceHolder: this.getClientID("scheduleRowsPlaceHolder") + userReportID
        };
        rowsPlaceHolderTemplate += TemplateEngine.Format(TemplateManager.templates['ReportSelectForm_RootScheduleTemplate'], rootScheduleAttr);
    }

    var rootAttr = {
        'rowsPlaceHolder': rowsPlaceHolderTemplate,
        'AddSchedulePopUp': this.getClientID("AddSchedulePopUp")
    };
    var mainDivLeft = TemplateEngine.Format(TemplateManager.templates['ReportSelectForm_RootTemplate'], rootAttr);
    $(this.Element).append(mainDivLeft);

    var reportTypesControl = new ReportTypesControl('reportTypes', this);
    reportTypesControl.ReportFolders = this.Data.ReportFolders;
    reportTypesControl.Reports = this.Data.Reports;
    reportTypesControl.Render();

    this.ShedulerControl = new ShedulerControl(this.getClientID('AddSchedulePopUp'), this);

    for (var idx in this.Data.ListReports) {
        var report = this.Data.ListReports[idx];
        var userReportID = this.Data.ListReports[idx].UserReportID;
        if (this.Data.ListReports[idx].IsExistsSchedules) {
            $('#' + this.getClientID("spawnReportButton") + userReportID).bind("click", function () {
                obj.spawnReportButtonClick($(this).attr("reportId"), $(this).attr("userReportID"), this);
            });
        }
        $('#' + this.getClientID("runReportButton") + userReportID).bind("click", function () {
            obj.runReportButtonClick($(this).attr("reportId"), $(this).attr("userReportID"));
        });
        $('#' + this.getClientID("editReportButton") + userReportID).bind("click", function () {
            obj.editReportButtonClick($(this).attr("reportId"), $(this).attr("userReportID"));
        });
        $('#' + this.getClientID("deleteReportButton") + userReportID).bind("click", function () {
            obj.deleteReportButtonClick($(this).attr("reportId"), $(this).attr("userReportID"));
        });
        $('#' + this.getClientID("addScheduleReportButton") + userReportID).bind("click", function () {
            obj.addScheduleReportButtonClick($(this).attr("reportId"), $(this).attr("userReportID"));
        });
    }

    $('img[sortColumnRoot]').parent().parent().bind('click', function () {
        obj.setSortReport($(this).find('div').find('img')[0].attributes['namecolumn'].value, $(this).find('div').find('img')[0].attributes['sortColumn'].value,
        // this.attributes['namecolumn'].value, this.attributes['sortColumn'].value, 
        obj.Data.ListReports);
        obj.Element.innerHTML = '';
        obj.Bind();
        obj.renderHeader(this.attributes['namecolumn'].value, this.attributes['sortColumn'].value);
    });
};

ReportSelectForm.prototype.setSortReport = function(columnName, sort, data) {
    data.sort(function (a, b) {
        var nameA = a[columnName], nameB = b[columnName];
        if (nameA < nameB) //sort string ascending
        {
            if (sort == "desc")
                return -1;
            else return 1;
        }
        if (nameA > nameB) {
            if (sort == "desc")
                return 1;
            else return -1;
        }
        //        return 0;
    });
};

ReportSelectForm.prototype.renderHeader = function (columnName, sort) {
    $('img[sortColumnRoot]').css('display','none');
    document.getElementById(columnName).style.display = '';
    if (sort == "asc") {
        document.getElementById(columnName).parentElement.parentElement.attributes['sortColumn'].value = 'desc';
        document.getElementById(columnName).attributes['sortColumn'].value = 'desc';
        document.getElementById(columnName).src = './images/sort_desc.png';
    }
    else {
        document.getElementById(columnName).parentElement.parentElement.attributes['sortColumn'].value = 'asc';
        document.getElementById(columnName).attributes['sortColumn'].value = 'asc';
        document.getElementById(columnName).src = './images/sort_asc.png';
    }
};

ReportSelectForm.prototype.renderHeaderSchedules = function (columnName, sort) {
    $('img[sortColumnSchedules]').css('display', 'none');
    document.getElementById(columnName).style.display = '';
    if (sort == "asc") {
        document.getElementById(columnName).parentElement.parentElement.attributes['sortColumn'].value = 'desc';
        document.getElementById(columnName).attributes['sortColumn'].value = 'desc';
        document.getElementById(columnName).src = './images/sort_desc.png';
    }
    else {
        document.getElementById(columnName).parentElement.parentElement.attributes['sortColumn'].value = 'asc';
        document.getElementById(columnName).attributes['sortColumn'].value = 'asc';
        document.getElementById(columnName).src = './images/sort_asc.png';
    }
};

ReportSelectForm.prototype.spawnReportButtonClick = function (reportId, userReportID, sender) {
    //alert('spawnReportButtonClick -reportId:' + reportId + '-userReportID:' + userReportID);
    if (document.getElementById(this.getClientID("rootScheduleRowsPlaceHolder") + userReportID).style.display == "") {
        document.getElementById(this.getClientID("rootScheduleRowsPlaceHolder") + userReportID).style.display = "none";
        sender.className = "Normal";
        return;
    }
    var params = {
        action: Consts.HANDLER_ACTIONS.GET_SCHEDULES,
        userReportID: userReportID
    };

    var parentObj = this;
    $.blockUI({ message: "Please wait loading...", css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });
    this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
        parentObj.successfullGetSchedules.call(parentObj, data);
    }, function (e) { parentObj.sendErrorCallbackForGetData.call(parentObj, e); });
};

ReportSelectForm.prototype.successfullGetSchedules = function (data) {
    this.setSortReport('Time', 'asc', data.listSchedules);
    this.bindSchedules(data);
    this.renderHeaderSchedules('Time' + data.userReportID, 'asc');
};

ReportSelectForm.prototype.bindSchedules = function (data) {
    var obj = this;
    document.getElementById(this.getClientID("rootScheduleRowsPlaceHolder") + data.userReportID).style.display = "";
    document.getElementById(this.getClientID("spawnReportButton") + data.userReportID).className = "NormalMinus";
    var scheduleRowsPlaceHolder = document.getElementById(this.getClientID("scheduleRowsPlaceHolder") + data.userReportID);
    var scheduleRowsTemplate = "";
    var listSchedules = data.listSchedules;
    for (var idx in listSchedules) {
        var schedule = listSchedules[idx];
        var scheduleAttr = {
            editScheduleButton: this.getClientID("editScheduleButton") + schedule.ScheduleID,
            deleteScheduleButton: this.getClientID("deleteScheduleButton") + schedule.ScheduleID,
            ScheduleID: schedule.ScheduleID,
            UserReportID: schedule.UserReportID,
            RecurrenceDescription: schedule.RecurrenceDescription,
            Time: schedule.Time,
            ScheduleStart: schedule.ScheduleStart,
            ScheduleStop: schedule.ScheduleStop
        };
        scheduleRowsTemplate += TemplateEngine.Format(TemplateManager.templates['ReportSelectForm_ScheduleItemTemplate'], scheduleAttr);
    }

    var headerAttr = {
        rows: scheduleRowsTemplate,
        idRecurrenceDescription: "RecurrenceDescription" + data.userReportID,
        idScheduleStart: "ScheduleStart" + data.userReportID,
        idTime: "Time" + data.userReportID,
        idScheduleStop: "ScheduleStop" + data.userReportID,
        idTable: data.userReportID
    };
    scheduleRowsPlaceHolder.innerHTML = TemplateEngine.Format(TemplateManager.templates['ReportSelectForm_HeaderScheduleTemplate'], headerAttr);

    for (var idx in listSchedules) {
        var schedule = listSchedules[idx];
        $('#' + this.getClientID("editScheduleButton") + schedule.ScheduleID).bind("click", function () {
            obj.editScheduleButtonClick($(this).attr("scheduleId"));
        });
        $('#' + this.getClientID("deleteScheduleButton") + schedule.ScheduleID).bind("click", function () {
            obj.deleteScheduleButtonClick($(this).attr("scheduleId"));
        });
    }
//    $('img[sortColumnSchedules=' + data.userReportID + ']').unbind();
    $('img[sortColumnSchedules=' + data.userReportID + ']').parent().parent().bind('click', function () {
//        var dataId = this.id.replace('RecurrenceDescription', '').replace('ScheduleStart', '').replace('Time', '').replace('ScheduleStop', '');
        obj.setSortReport($(this).find('div').find('img')[0].attributes['namecolumn'].value, $(this).find('div').find('img')[0].attributes['sortColumn'].value, data.listSchedules);
        obj.bindSchedules(data);
        obj.renderHeaderSchedules(this.attributes['namecolumn'].value + data.userReportID, this.attributes['sortColumn'].value);
        document.getElementById(obj.getClientID("rootScheduleRowsPlaceHolder") + data.userReportID).style.display = "";
    });
    $.unblockUI();
};

ReportSelectForm.prototype.editScheduleButtonClick = function (scheduleId) {
    alert('editScheduleButtonClick -scheduleId:' + scheduleId);
};

ReportSelectForm.prototype.deleteScheduleButtonClick = function (scheduleId) {
    alert('deleteScheduleButtonClick -scheduleId:' + scheduleId);
};

ReportSelectForm.prototype.runReportButtonClick = function (reportId, userReportID) {
    alert('runReportButtonClick -reportId:' + reportId + '-userReportID:' + userReportID);
};

ReportSelectForm.prototype.editReportButtonClick = function (reportId, userReportID) {
    //alert('editReportButtonClick -reportId:' + reportId + '-userReportID:' + userReportID);
    if (reportId != 1014) return;
    this.ReportBuilderHeader.ShowReportBuilderForm(reportId, userReportID);
};

ReportSelectForm.prototype.deleteReportButtonClick = function (reportId, userReportID) {
    if (confirm('Are you sure you want to delete this report?')) {

        var params = {
            action: Consts.HANDLER_ACTIONS.DELETE_REPORT,
            reportId: reportId,
            userReportID: userReportID
        };

        var parentObj = this;
        $.blockUI({ message: "Please wait loading...", css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%' } });
        this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function(data) {
            window.location = window.location;
        }, function(e) {
            parentObj.sendErrorCallbackForGetData.call(parentObj, e);
        });
    }
};

ReportSelectForm.prototype.addScheduleReportButtonClick = function (reportId, userReportID) {
    
    //this.ShedulerControl.Bind(reportId, userReportID, -1);
     //$.blockUI({ message: null, css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });
};

ReportSelectForm.prototype.sendAjax = function (url, params, callbackSuccess, callbackError) {
    $.ajax({
        type: "POST",
        dataType: 'json',
        url: url,
        data: params,
        success: callbackSuccess,
        error: callbackError
    });
};

ReportSelectForm.prototype.sendErrorCallbackForGetData = function (e) {
    $.unblockUI();
    this.sendErrorCallback(e);
    if (e.responseText.indexOf("timeout") > -1) {
        this.goToMain();
    }
};

ReportSelectForm.prototype.sendErrorCallback = function (e) {
    this.IsBusy = false;
    /*
    if (e.responseText.indexOf('MAPS Login Page') > -1) {
    returnToLogin(winAuthPath + '?ReturnURL=MAPSPage.aspx%3f' + 'QuoteId=' + this.QuoteId + '%26ActiveSectionNumber=' + this.ActiveSectionNumber);
    return;
    }
    */
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

ReportSelectForm.prototype.goToMain = function () {
    /*try {
    window.location = Consts.PAGES.GRID_PAGE + '?currentPage=' + this.CurrentPage + '&filterId=' + this.FilterId + '&sortABC=' + this.SortABC + '&searchParam=' + ReplaceQuoteCharToUnicodeString(this.SearchParam);
    } catch(e) {

    }*/
};

ReportSelectForm.prototype.getClientID = function (name) {
    return this.Element.id + '_' + name;
};

