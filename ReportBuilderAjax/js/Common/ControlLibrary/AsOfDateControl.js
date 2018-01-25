var AsOfDateControl = function (id, reportId) {
    this.ReportId = reportId;
    this.Element = document.getElementById(id);
    this.Id = id;
    this.Controls = new Array();
    this.AsOfDateRanges = new Array();
    this.Data = new Array();
};

AsOfDateControl.prototype.Bind = function(template) {
    var attr = {
        "asOfDateType": this.getClientID("asOfDateType"),
        "asOfDate": this.getClientID("asOfDate")
    };

    $(this.Element).append(TemplateEngine.Format(template, attr));

    //Start Data range dropdown menu
    var DropDownAttr = {
        "idDropDown": this.getClientID("asOfDateTypeDrop"),
        "dropDownHide": "dropDownHide",
        "attribute": "AsOfDateType"
    };
    var DropDownTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_DropDownControl'], DropDownAttr);
    $(document.getElementById(this.getClientID("asOfDateType"))).append(DropDownTempl);

    this.Controls.AsOfDateTypeDropDown = new newDropDownControl(this, DropDownAttr["idDropDown"], -1, -1, ValueType.Int, null, false, 205);
    this.Controls.AsOfDateTypeDropDown.ClearOptions();

    this.Controls.AsOfDateTypeDropDown.EventSet.AfterDataBind.List.SetDisplayState = function (e) {
        this.ParentObject.AsOfDateTypeChanged(this.GetOptionValue(this.Value));
    };

    //Start DateFrom calendar input
    var calendarDateFrom = {
        "idCalendarInput": this.getClientID("asOfDateC"),
        "idCalendarContainer": "asOfDateContainer"
    };
    var calendarDateFromTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_CalendarInput'], calendarDateFrom);
    $(document.getElementById(this.getClientID("asOfDate"))).append(calendarDateFromTempl);
    $("#" + this.getClientID("asOfDateC")).focus(function () {
        $(document.getElementById("asOfDateContainer")).removeClass('calendarConteiner');
        $(document.getElementById("asOfDateContainer")).addClass('calendarConteinerFocus');
    });
    $("#" + this.getClientID("asOfDateC")).bind('blur', function () {
        $(document.getElementById("asOfDateContainer")).removeClass('calendarConteinerFocus');
        $(document.getElementById("asOfDateContainer")).addClass('calendarConteiner');
    });

    //Minimum Date new DateTime(2010, 07, 06);

    this.Controls.AsOfDateControl = new CalendarControl(this, calendarDateFrom["idCalendarInput"], "", "", ValueType.Date, {}, new Date(2010, 6, 6, 0 , 0 , 0 , 0));
    //End DateFrom calendar input
    this.fillAsOfDataRanges();
};

AsOfDateControl.prototype.AsOfDateTypeChanged = function (value) {
    this.Controls.AsOfDateControl.Calendar.datepicker('disable');
    var nowDate = new Date();
    var newAsOfDate = nowDate;

    switch (value * 1)
    {
        case Consts.AS_OF_DATE_TYPE.LAST_DAY_OF_LAST_WEEK:
            newAsOfDate = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate() - nowDate.getDay() - 1, 0, 0, 0, 0);
            break;
        case Consts.AS_OF_DATE_TYPE.LAST_DAY_OF_LAST_MONTH:
            newAsOfDate = new Date(nowDate.getFullYear(), nowDate.getMonth() - 1, daysInMonth(nowDate.getFullYear(), nowDate.getMonth()));
            break;
        case Consts.AS_OF_DATE_TYPE.LAST_DAY_OF_LAST_QUARTER:
            var qyear = (nowDate.getMonth() + 1 <= 3) ? (nowDate.getFullYear() - 1) : (nowDate.getFullYear());
            var newStart = new Date(qyear, (nowDate.getMonth() - (nowDate.getMonth() % 3) + 10) % 12, 1);
            newAsOfDate = new Date(qyear, (newStart.getMonth() + 1), daysInMonth(qyear, newStart.getMonth() + 2));
            break;
        case Consts.AS_OF_DATE_TYPE.LAST_DAY_OF_LAST_YEAR:
            newAsOfDate = new Date(nowDate.getFullYear() - 1, 11, 31, 0, 0, 0, 0);
            break;
        case Consts.AS_OF_DATE_TYPE.CUSTOM:
            this.Controls.AsOfDateControl.Calendar.datepicker('enable');
            newAsOfDate = new Date();
            break;
        case Consts.AS_OF_DATE_TYPE.YESTERDAY:
            newAsOfDate = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate() - 1, 0, 0, 0, 0);
            break;
        default:
            break;
    }

    this.Controls.AsOfDateControl.SetValue(returnShortDate(newAsOfDate));
};

AsOfDateControl.prototype.fillAsOfDataRanges = function () {
    var defaultValue = null;
    this.AsOfDateRanges = new Array();
    this.AsOfDateRanges[this.AsOfDateRanges.length] = { Name: "Yesterday", Value: Consts.AS_OF_DATE_TYPE.YESTERDAY };
    this.AsOfDateRanges[this.AsOfDateRanges.length] = { Name: "Last Day of Last Week", Value: Consts.AS_OF_DATE_TYPE.LAST_DAY_OF_LAST_WEEK };
    defaultValue = { Name: "Last Day of Last Month", Value: Consts.AS_OF_DATE_TYPE.LAST_DAY_OF_LAST_MONTH };
    this.AsOfDateRanges[this.AsOfDateRanges.length] = defaultValue;
    this.AsOfDateRanges[this.AsOfDateRanges.length] = { Name: "Last Day of Last Quarter", Value: Consts.AS_OF_DATE_TYPE.LAST_DAY_OF_LAST_QUARTER };
    this.AsOfDateRanges[this.AsOfDateRanges.length] = { Name: "Last Day of Last Year", Value: Consts.AS_OF_DATE_TYPE.LAST_DAY_OF_LAST_YEAR };
    this.AsOfDateRanges[this.AsOfDateRanges.length] = { Name: "Custom", Value: Consts.AS_OF_DATE_TYPE.CUSTOM };

    for (var index in this.AsOfDateRanges) {
        this.Controls.AsOfDateTypeDropDown.AppendOption(this.AsOfDateRanges[index].Value, this.AsOfDateRanges[index].Name);
    }

    this.Controls.AsOfDateTypeDropDown.SetDropDownValue(defaultValue.Value);
};


AsOfDateControl.prototype.getAsOfDateValue = function () {
    return this.Controls.AsOfDateControl.Value + " 00:00 AM";
};

AsOfDateControl.prototype.getAsOfDateTypeValue = function () {
    return this.Controls.AsOfDateTypeDropDown.GetOptionValue(this.Controls.AsOfDateTypeDropDown.Value);
};

AsOfDateControl.prototype.getClientID = function (name) {
    return this.Element.id + '_' + name;
};