function ViewReport(element, parentObject) {
    this.Element = element;
    this.ParentObject = parentObject;
    this.IsFirst = true;
    this.Controls = [];
};

ViewReport.prototype.Bind = function () {
    if (this.IsFirst) {
        var attr = {
            "content": this.getClientID("content"),
            "currentPage": this.getClientID("currentPage"),
            "totalPages": this.getClientID("totalPages"),
            "prevBtn": this.getClientID("prevBtn"),
            "nextBtn": this.getClientID("nextBtn"),
            "exportPDFBtn": this.getClientID("exportPDFBtn"),
            "exportXLSBtn": this.getClientID("exportXLSBtn"),
            "refreshBtn": this.getClientID("refreshBtn")
        };

        $(this.Element).append(TemplateEngine.Format(TemplateManager.templates['ViewReport_Main'], attr));
        this.IsFirst = false;

        var parentObj = this;
        this.Controls.CurrentPage = new TextBoxControl(this, this.getClientID("currentPage"), "", "", ValueType.Int);
        this.Controls.CurrentPage.SetValue(0);

        $(this.Controls.CurrentPage.Element).bind("keyup", function (e) {
            parentObj.validateInteger.call(this, e);
        });

        this.Controls.TotalPages = new TextBoxControl(this, this.getClientID("totalPages"), "", "", ValueType.Int);
        this.Controls.TotalPages.Block();
        this.Controls.TotalPages.SetValue(0);

        $(document.getElementById(this.getClientID("exportPDFBtn"))).bind('click', function () {
            parentObj.exportPDFBtnClick.call(parentObj);
        });

        $(document.getElementById(this.getClientID("exportXLSBtn"))).bind('click', function () {
            parentObj.exportXLSBtnClick.call(parentObj);
        });

        $(document.getElementById(this.getClientID("refreshBtn"))).bind('click', function () {
            parentObj.refreshBtnClick.call(parentObj, true);
        });
    }
};

ViewReport.prototype.exportPDFBtnClick = function () {
    var parentObj = this;
    var params = this.ParentObject.ParentObject.GetReportParamsForViewReport(Consts.FORMAT_TYPE.PDF);
    if (params != null) {
        $.blockUI({ message: "Please wait loading...", css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });
        this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
            parentObj.successfullRunReport.call(parentObj, data);
        }, function (e) { parentObj.sendErrorCallbackForGetData.call(parentObj, e); });
    }
};

ViewReport.prototype.exportXLSBtnClick = function () {
    var parentObj = this;
    var params = this.ParentObject.ParentObject.GetReportParamsForViewReport(Consts.FORMAT_TYPE.Excel);
    if (params != null) {
        $.blockUI({ message: "Please wait loading...", css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });
        this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
            parentObj.successfullRunReport.call(parentObj, data);
        }, function (e) { parentObj.sendErrorCallbackForGetData.call(parentObj, e); });
    }
};

ViewReport.prototype.refreshBtnClick = function (isHide) {
    var parentObj = this;
    var params = null;

    if(document.getElementById("SlideLeftBttn").style.display != 'none' && isHide)
        this.ParentObject.ParentObject.HideLeft();

    var type = this.ParentObject.ParentObject.Controls.PdfXlsContl.GetValue() * 1;
    if (type != Consts.FORMAT_TYPE.GRID) if ($.browser.msie) type = Consts.FORMAT_TYPE.MSHTML;
   

    params = this.ParentObject.ParentObject.GetReportParamsForViewReport(type);

    if (params != null) {
        $.blockUI({ message: "Please wait loading...", css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });
        this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
            parentObj.successfullRunReport.call(parentObj, data, type);
        }, function (e) { parentObj.sendErrorCallbackForGetData.call(parentObj, e); });
    }
};

ViewReport.prototype.successfullRunReport = function (data, type) {
    var iframe = document.getElementById(this.getClientID("content"));

    if (type != Consts.FORMAT_TYPE.GRID) {
        if (data && data.Result.Status == 0 && data.Result.ReportUrl != null && data.Result.ReportUrl.toString() != "" /*&& data.Result.Content != ""*/) {
            iframe.style.display = '';
            iframe.src = data.Result.ReportUrl.toString();
        }
    } else {
        iframe.style.display = 'none';
        var gridData = {};
        gridData.aaData = [];
        gridData.aoColumns = [];

        for (var column in data.Result.UserReportFields) {
            if (data.Result.UserReportFields[column].ColumnOrder > -1) {
                var className = "row";
                if (data.Result.UserReportFields[column].DataType == Consts.DATA_TYPE.DATE || data.Result.UserReportFields[column].DataType == Consts.DATA_TYPE.TIME) {
                    className = "crow";
                }
                else if (data.Result.UserReportFields[column].DataType == Consts.DATA_TYPE.INTEGER || data.Result.UserReportFields[column].DataType == Consts.DATA_TYPE.DECIMAL || 
                    data.Result.UserReportFields[column].DataType == Consts.DATA_TYPE.MONEY || data.Result.UserReportFields[column].DataType == Consts.DATA_TYPE.PERCENTAGE)
                {
                    className = "rrow";
                }
                gridData.aoColumns.push({ "sTitle": data.Result.UserReportFields[column].Name, "sClass": className, "mData": data.Result.UserReportFields[column].SQLName });
            }
        }

        gridData.aaData = data.Result.GridData;

        $("#viewGrid").dataTable({
            "aaData": gridData.aaData,
            "aoColumns": gridData.aoColumns,
            "sStripeOdd": "",
            "sStripeEven": "",
            "bSort": false,
            "bFilter": false,
            "bDestroy": true,
            "iDisplayLength": 24,
            "bLengthChange": false,
            "bPaginate": true,
            "sPaginationType": "full_numbers"
        });

    }
    $.unblockUI();
};

ViewReport.prototype.validateInteger = function (e) {
    var keyCode = e.keyCode;
    if ((keyCode >= 48 && keyCode <= 57) || (keyCode >= 35 && keyCode <= 40) || keyCode == 8 || keyCode == 46 || (keyCode >= 96 && keyCode <= 105) || keyCode == 144 || keyCode == 20) return;
    var str = this.value;
    if (!str) return;
    var re = /[^0-9]/gi;
    this.value = str.replace(re, "");
};

ViewReport.prototype.sendAjax = function (url, params, callbackSuccess, callbackError) {
    $.ajax({
        type: "POST",
        dataType: 'json',
        url: url,
        data: params,
        success: callbackSuccess,
        error: callbackError
    });
};

ViewReport.prototype.sendErrorCallbackForGetData = function (e) {
    $.unblockUI();
    this.sendErrorCallback(e);
    if (e.responseText.indexOf("timeout") > -1) {
        this.goToMain();
    }
};

ViewReport.prototype.sendErrorCallback = function (e) {
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

ViewReport.prototype.getClientID = function (name) {
    return this.Element.id + '_' + name;
};